//
//  Copyright (c) 2015 IBM Corp. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
//  except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software distributed under the
//  License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
//  either express or implied. See the License for the specific language governing permissions 
//  and limitations under the License.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;


namespace IBM.Cloudant.Client
{
    /// <summary>
    /// Contains a Database Public API implementation.
    /// </summary>
    /// <remarks>
    /// This class is used for performing CRUD, index, and query operations on a Cloudant database.
    /// </remarks>
    public class Database
    {
        // Special document fields
        private static readonly string DOC_ID = "_id";
        private static readonly string DOC_REV = "_rev";
        private static readonly string DOC_DELETED = "_deleted";

        // REST response fields
        private static readonly string DOC_RESPONSE_ID = "id";
        private static readonly string DOC_RESPONSE_REV = "rev";

        private static readonly string INDEX_NAME_JSON_KEY = "name";
        //private static readonly string INDEX_FIELDS_JSON_KEY = "fields";
        private static readonly string INDEX_TYPE_JSON_KEY = "type";
        private static readonly string INDEX_TYPE_JSON_VALUE = "json";
        private static readonly string INDEX_DESIGN_DOCUMENT_JSON_KEY = "ddoc";

        private CloudantClient client;
        private string dbNameUrlEncoded;

        /// <summary>
        /// Name of this database object.
        /// </summary>
        public String dbname { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="IBM.Cloudant.Client.Database"/> class.
        /// </summary>
        /// <param name="client">The CloudantClient instance.</param>
        /// <param name="name">The name of the database.</param>
        public Database(CloudantClient client, String name) {
            //validate the dbName

            Regex strPattern = new Regex("^[a-z][a-z0-9_"+Regex.Escape ("$")+ Regex.Escape ("(")+Regex.Escape (")")+
                Regex.Escape ("+")+ Regex.Escape ("/")+ "-]*$");

            if(!strPattern.IsMatch (name)){
                throw new ArgumentException ("A database must be named with all lowercase letters (a-z), digits (0-9)," +
                    " or any of the _$()+-/ characters. The name has to start with a lowercase letter (a-z). ");
            }

            this.client = client;
            dbname = name;
            dbNameUrlEncoded = WebUtility.UrlEncode (name);
        }

        /// <summary>
        /// Ensures the database exists, this method blocks until complete. 
        /// </summary>
        public void EnsureExists(){
            Task<Database> result = Task.Run (() => {
                var dbUri = new Uri (client.accountUri.ToString () + dbNameUrlEncoded);
                Task<HttpResponseMessage> httpTask = client.httpHelper.sendPut (dbUri, null, null);
                httpTask.Wait ();

                if (httpTask.IsFaulted) {
                    string errorMessage = string.Format ("Error occurred during creation of remote database at URL: {0}",
                        dbUri.ToString () + ".  Error: " + httpTask.Exception.Message);
                    Debug.WriteLine (errorMessage);
                    throw new DataException (DataException.Database_DatabaseModificationFailure, errorMessage);

                } else {

                    int httpStatus = (int)httpTask.Result.StatusCode;
                    if (httpStatus != 200 && httpStatus != 201 && httpStatus != 412) {
                        String errorMessage = String.Format("Failed to create remote database.\nHTTP_Status: {0}\nJSON Body: {1}",
                            httpStatus, httpTask.Result.ReasonPhrase);
                        Debug.WriteLine(errorMessage);
                        throw new DataException(DataException.Database_DatabaseModificationFailure,
                            errorMessage, httpTask.Exception);
                    }
                }

                return this;
            });
            result.Wait ();
        }

        /// <summary>
        /// Deletes the database this object represents.
        /// </summary>
        /// <returns>A Task to mointor this action.</returns>
        public Task Delete(){
        
            Task result = Task.Run (() => {
                Task<HttpResponseMessage> deleteTask = client.httpHelper.sendDelete (new Uri(WebUtility.UrlEncode(dbname), UriKind.Relative) , null);

                deleteTask.ContinueWith( (antecedent) => {
                    if(deleteTask.IsFaulted){
                        throw new DataException(DataException.Database_DatabaseModificationFailure, deleteTask.Exception.Message, deleteTask.Exception);
                    }

                    var httpStatus = deleteTask.Result.StatusCode;
                    if(deleteTask.Result.StatusCode != System.Net.HttpStatusCode.OK){
                        string errorMessage = String.Format("Failed to delete remote database.\nHTTP_Status: {0}\nJSON Body: {1}",
                            httpStatus, deleteTask.Result.ReasonPhrase);
                        throw new DataException(DataException.Database_DatabaseModificationFailure, errorMessage);
                    }
                });

            });

            return result;
        
        }
            
        /// <summary>
        /// Save the specified revisionToSave using HTTP <tt>PUT</tt> request.
        /// </summary>
        /// <param name="revisionToSave">Revision to save.</param>
        public Task<DocumentRevision> Save(DocumentRevision revisionToSave)
        {
            Debug.WriteLine ("==== enter Database::save(Object)");
            if (revisionToSave == null) {
                string errorMessage = "The input parameter revisionToSave cannot be null.";
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
            }

            Debug.WriteLine ("==== exit Database::save");
            return CreateDocumentRevision (revisionToSave);
        }

        /// <summary>
        /// Update the specified revisionToUpdate.  The revision must have the correct <code>_id</code> and
        /// <code>_rev</code> values.
        /// </summary>
        /// <param name="revisionToUpdate">Revision to update.</param>
        public Task<DocumentRevision> Update(DocumentRevision revisionToUpdate)
        {
            Debug.WriteLine ("==== enter Database::update(Object)"); 
            if (revisionToUpdate == null) {
                string errorMessage = "The input parameter revisionToSave cannot be null.";
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
            }

            Debug.WriteLine ("==== exit Database::remove");
            return UpdateDocumentRevision (revisionToUpdate);
        }

        /// <summary>
        /// Find the specified document.
        /// </summary>
        /// <param name="documentId">The id of the object to find.</param>
        public Task<DocumentRevision> Find(String documentId){
            Debug.WriteLine ("==== enter Database::find(String)");
            try {
                if(string.IsNullOrWhiteSpace(documentId)){
                    throw new DataException(DataException.Database_FetchDocumentRevisionFailure, "Unable to fetch document revision.  documentId " +
                        "parameter must not be null or empty");
                }

                Uri requestUrl = new Uri (dbNameUrlEncoded + "/" + documentId, UriKind.Relative);
                Debug.WriteLine ("fetch document relative URI is: "+requestUrl.ToString());

                Task<DocumentRevision> result = Task.Run (() => {
                    // Send the HTTP request
                    Task<HttpResponseMessage> httpTask = client.httpHelper.sendGet(requestUrl, null);
                    Task<DocumentRevision> revisionTask = httpTask.ContinueWith( (antecedent) => {
                        if(httpTask.IsFaulted){
                            Debug.WriteLine ("HTTP request to fetch document task failed with error:"+httpTask.Exception.Message);
                            throw new DataException(DataException.Database_FetchDocumentRevisionFailure, httpTask.Exception.Message, httpTask.Exception);
                        }

                        // Read in the response JSON into a Dictionary
                        Task<String> readContentTask = httpTask.Result.Content.ReadAsStringAsync();
                        readContentTask.Wait();
                        Dictionary<string, object> responseJSON = JsonConvert.DeserializeObject<Dictionary<string, object>> (readContentTask.Result); 

                        Debug.WriteLine("Cloudant find document response.  Relative URI: " + requestUrl.ToString() + ". " +
                            "Document: " + documentId + "\nResponse content:" + readContentTask.Result);

                        // Check the HTTP status code
                        int httpStatus = (int)httpTask.Result.StatusCode;
                        if(httpStatus != 200){
                            string errorMessage = String.Format("Failed to find document revision.\nRequest URL: {0}\nHTTP_Status: {1}\nJSONBody: {2}",
                                requestUrl.ToString(),httpStatus,readContentTask.Result);
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_FetchDocumentRevisionFailure, errorMessage);
                        }

                        Object responseDocumentId;
                        responseJSON.TryGetValue(DOC_ID,out responseDocumentId);
                        if (responseDocumentId == null || responseDocumentId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a documentId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_FetchDocumentRevisionFailure, errorMessage);
                        }


                        Object responseRevisionId;
                        responseJSON.TryGetValue(DOC_REV,out responseRevisionId);
                        if (responseRevisionId == null || responseRevisionId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a revisionId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_FetchDocumentRevisionFailure, errorMessage);
                        }

                        // create new DocumentRevision with docId and revId returned
                        DocumentRevision createdDoc = new DocumentRevision((string)responseDocumentId,(string)responseRevisionId,responseJSON);
                        Debug.WriteLine ("returning DocumentRevision\n docId:{0}\n revId:{1}\n body:{2}",
                            responseDocumentId,
                            responseRevisionId,
                            JsonConvert.SerializeObject(responseJSON));

                        return createdDoc;
                    });
                    return revisionTask;
                });
                Debug.WriteLine ("==== exit Database::find");
                return result;
            } catch (Exception e) {
                throw new DataException(DataException.Database_FetchDocumentRevisionFailure, e.Message, e);
            }
        }

        /// <summary>
        /// Remove the specified DocumentRevision.
        /// </summary>
        /// <param name="revisionToRemove">Revision to remove.</param>
        public Task<String> Remove(DocumentRevision revisionToRemove){
            Debug.WriteLine ("==== enter Database::delete(Object)");

            try {
                if (revisionToRemove == null) {
                    throw new DataException(DataException.Database_DeleteDocumentRevisionFailure,
                        "Unable to delete document revisionToRemove.  revision parameter must not be null");
                }

                Uri requestUrl = new Uri( dbNameUrlEncoded + "/" 
                    + revisionToRemove.docId + "?rev=" + revisionToRemove.revId, UriKind.Relative);
                Task<String> result = Task.Run (() => {
                    // Send the HTTP request
                    Task<HttpResponseMessage> httpTask = client.httpHelper.sendDelete(requestUrl, null);
                    Task<String> responseTask = httpTask.ContinueWith( (antecedent) => {
                        if(httpTask.IsFaulted){
                            Debug.WriteLine ("HTTP request to delete document task failed with error:"+httpTask.Exception.Message);
                            throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, httpTask.Exception.Message, httpTask.Exception);
                        }

                        // Read in the response JSON into a Dictionary
                        Task<String> readContentTask = httpTask.Result.Content.ReadAsStringAsync();
                        readContentTask.Wait();
                        Dictionary<string, object> responseJSON = JsonConvert.DeserializeObject<Dictionary<string, object>> (readContentTask.Result); 

                        Debug.WriteLine("Cloudant delete document response.  Relative URI: " + requestUrl.ToString() + ". " +
                            "Document: " + revisionToRemove.docId + "\nResponse content:" + readContentTask.Result);

                        // Check the HTTP status code
                        int httpStatus = (int)httpTask.Result.StatusCode;
                        if(httpStatus < 200 || httpStatus >=300){
                            string errorMessage = String.Format("Failed to delete document revision.\nRequest URL: {0}\nHTTP_Status: {1}\nJSONBody: {2}",
                                requestUrl.ToString(),httpStatus,readContentTask.Result);
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, errorMessage);
                        }

                        Object responseRevisionId;
                        responseJSON.TryGetValue(DOC_RESPONSE_REV,out responseRevisionId);
                        if (responseRevisionId == null || responseRevisionId.Equals("")) {
                            string errorMessage = "\ndocument delete JSON Response didn't contain a revisionId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, errorMessage);
                        }
                        return (string)responseRevisionId;
                    });
                    return responseTask;
                });
                Debug.WriteLine ("==== exit Database::delete");
                return result;
            } catch (Exception e) {
                throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, e.Message, e);
            }
        }


        /// <summary>
        /// Create a new index.
        /// Also see: <a href="http://docs.cloudant.com/api/cloudant-query.html#creating-a-new-index">
        /// http://docs.cloudant.com/api/cloudant-query.html#creating-a-new-index</a>
        /// </summary>
        /// <param name="indexName">optional name of the index (if not provided one will be generated)</param>
        /// <param name="designDocName">optional name of the design doc in which the index will be created</param>
        /// <param name="indexType">optional, type of index (only "json" as of now)</param>
        /// <param name="fields">array of fields in the index</param>
        public Task CreateIndex(String indexName, String designDocName, String indexType,
                IndexField[] fields) {

            if (fields == null || fields.Length == 0)
                //throw new DataException(DataException.Database_IndexModificationFailure, "index fields must not be null or empty");
            throw new DataException (DataException.Database_IndexModificationFailure, "fields parameter must be non-empty and a valid array of" +
                typeof(IndexField) + " objects.");

            if(indexType != null && !indexType.Equals(INDEX_TYPE_JSON_VALUE))
                throw new DataException(DataException.Database_IndexModificationFailure, "Only 'json' indexType supported.");
            
                
            Task result = Task.Run( () => {
                String indexDefn = GetIndexDefinition(indexName, designDocName, indexType, fields);
                CreateIndex(indexDefn).Wait();
            });

            return result;
        }


        /// <summary>
        /// Create a new Index
        /// See <a href="http://docs.cloudant.com/api/cloudant-query.html#creating-a-new-index">
        /// http://docs.cloudant.com/api/cloudant-query.html#creating-a-new-index</a>
        /// </summary>
        /// <param name="indexDefinition"> Index definition. See documentation for correct format.</param>
        public Task CreateIndex(String indexDefinition) {
            
            if(string.IsNullOrWhiteSpace (indexDefinition))
                throw new DataException(DataException.Database_IndexModificationFailure, "indexDefinition may not be null or empty");

            Task result = Task.Run (() => {
                Uri indexUri = new Uri (dbNameUrlEncoded + "/_index", UriKind.Relative);
                Debug.WriteLine ("index relative URI: " + indexUri);


                Dictionary <string,object> body;
                try{
                    body = JsonConvert.DeserializeObject<Dictionary<string,object>> (indexDefinition);
                } catch(Exception){
                    throw new DataException(DataException.Database_IndexModificationFailure, "Error creating index: indexDefinition contains invalid JSON.");
                }
                    
                Task<HttpResponseMessage> httpTask = client.httpHelper.sendPost (indexUri, null, body);
                httpTask.Wait ();

                if (!httpTask.IsFaulted) {
                    if (httpTask.Result.StatusCode == HttpStatusCode.OK || httpTask.Result.StatusCode == HttpStatusCode.Created) { //Status code 200 or 201
                        Debug.WriteLine (string.Format ("Created Index: '{0}'", indexDefinition));
                        return;
                    } else {
                        Debug.WriteLine (string.Format ("Error creating index : '{0}'", indexDefinition));
                        throw new DataException(DataException.Database_IndexModificationFailure,
                                string.Format ("Error creating index : '{0}'", indexDefinition)); 
                    }
                } 
            });

            return result;
        }



            
        /// <summary>
        /// Finds documents based using an index.
        /// See <a href="https://docs.cloudant.com/cloudant_query.html#finding-documents-using-an-index">
        /// https://docs.cloudant.com/cloudant_query.html#finding-documents-using-an-index</a>
        /// </summary>
        /// <returns>List of matching documents.</returns>
        /// <param name="selectorJson">JSON String describing criteria used to select documents.
        ///                     Is of the form "selector": your data here </param>
        /// <param name="options">Options describing the query options to apply.  </param>
        public Task<List<DocumentRevision>> FindByIndex(String selectorJson, FindByIndexOptions
            options) {
            if(selectorJson == null){
                throw new DataException(DataException.Database_QueryError, "selectorJson parameter cannot be null");
            }
            if(options == null){
                throw new DataException(DataException.Database_QueryError, "options parameter cannot be null");
            }

            // POST query
            Task<List<DocumentRevision>> result = Task.Run <List<DocumentRevision>> ( () => {
                Uri indexUri = new Uri(dbNameUrlEncoded + "/_find", UriKind.Relative);
                String queryBody = GetFindByIndexBody(selectorJson, options);
                Dictionary<string,string> headers = new Dictionary<string,string> ();
                Dictionary <string,object> body = JsonConvert.DeserializeObject<Dictionary<string,object>> (queryBody);

                Task<HttpResponseMessage> httpTask = client.httpHelper.sendPost (indexUri, headers, body);
                Task<List<DocumentRevision>> responseTask = httpTask.ContinueWith( (antecedent) => {
                    if(httpTask.IsFaulted){
                        Debug.WriteLine ("HTTP request to findByIndex task failed with error:"+httpTask.Exception.Message);
                        throw new DataException(DataException.Database_QueryError, httpTask.Exception.Message, httpTask.Exception);
                    }

                    // Check the HTTP status code
                    int httpStatus = (int)httpTask.Result.StatusCode;
                    if(httpStatus < 200 || httpStatus > 300){
                        string errorMessage = String.Format("findByIndex failed.\nHTTP_Status: {0}\nErrorMessage: {1}",
                            httpStatus, httpTask.Result.ReasonPhrase);
                        Debug.WriteLine (errorMessage);
                        throw new DataException(DataException.Database_QueryError, errorMessage);
                    }

                    // Read in the response JSON into a Dictionary
                    Task<String> readContentTask = httpTask.Result.Content.ReadAsStringAsync();
                    readContentTask.Wait();
                    Dictionary<string, object> responseJSON = JsonConvert.DeserializeObject<Dictionary<string, object>> (readContentTask.Result); 
                    Debug.WriteLine("response JSON:{0}", JsonConvert.SerializeObject(responseJSON));

                    List<DocumentRevision> list = new List<DocumentRevision>();
                    Object documents;
                    responseJSON.TryGetValue("docs",out documents);
                    JArray doclist = (JArray) documents;
                    foreach (JToken token in doclist){
                        String documentId = token.Value<String>(DOC_ID);
                        if (documentId == null || documentId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a documentId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }

                        String revisionId = token.Value<String>(DOC_REV);
                        if (revisionId == null || revisionId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a revisionId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }

                        // create new DocumentRevision with docId and revId returned
                        var documentBody = token.ToObject<Dictionary<String,Object>>();
                        DocumentRevision createdDoc = new DocumentRevision(documentId,revisionId,documentBody);
                        Debug.WriteLine ("findByIndex() : adding DocumentRevision\n docId:{0}\n revId:{1}\n body:{2}",
                            documentId,
                            revisionId,
                            JsonConvert.SerializeObject(documentBody));

                        list.Add(createdDoc);
                    }

                    return list;
                });             
                return responseTask;
            });
            return result;
        }

        /// <summary>
        /// Lists all indices.
        /// </summary>
        /// <returns>List of Index</returns>
        public Task<List<Index>> ListIndices() {

            Task<List<Index>> result = Task.Run <List<Index>> (() => {
                List<Index> taskResult = new List<Index>();

                Uri indexUri = new Uri (dbNameUrlEncoded + "/_index/", UriKind.Relative);

                Task<HttpResponseMessage> httpTask = client.httpHelper.sendGet (indexUri, null);
                httpTask.Wait ();

                JObject jsonIndex = JsonConvert.DeserializeObject<JObject>(httpTask.Result.Content.ReadAsStringAsync().Result);

                JToken indexes;
                jsonIndex.TryGetValue("indexes", out indexes);

                if(indexes is Newtonsoft.Json.Linq.JArray){
                    JArray indexesArray = (JArray)indexes;

                    foreach(JToken token in indexesArray){
                        Index index = new Index(token.SelectToken(INDEX_DESIGN_DOCUMENT_JSON_KEY).ToString(),
                            token.SelectToken(INDEX_NAME_JSON_KEY).ToString(), 
                            token.SelectToken(INDEX_TYPE_JSON_KEY).ToString());

                        foreach(JObject indexArray in token.SelectToken("def.fields")){

                            foreach(JProperty prop in indexArray.Properties()){

                                if(prop.Value.ToString() == IndexField.SortOrder.asc.ToString())
                                    index.AddIndexField(prop.Name, IndexField.SortOrder.asc );
                                else if(prop.Value.ToString() == IndexField.SortOrder.desc.ToString())
                                    index.AddIndexField(prop.Name, IndexField.SortOrder.desc );
                                else
                                    throw new DataException(DataException.Database_IndexModificationFailure, "invalid index field sort order value.");
                            }
                        }

                        taskResult.Add(index);
                    } 
                } else
                    throw new DataException(DataException.Database_IndexModificationFailure, "Got unexpected JSON for indexes. "+jsonIndex.ToString());

                return taskResult;
            });

            return result;
        }


        /// <summary>
        /// Delete an index
        /// </summary>
        /// <returns>A Task</returns>
        /// <param name="indexName">name of the index</param>
        /// <param name="designDocId">ID of the design doc</param>
        public Task DeleteIndex(String indexName, String designDocId) {

            if(string.IsNullOrWhiteSpace (indexName))
                throw new DataException(DataException.Database_IndexModificationFailure, "indexName may not be null or empty.");
            if(string.IsNullOrWhiteSpace (designDocId))
                throw new DataException(DataException.Database_IndexModificationFailure, "designDocId may not be null or empty");


            Task result = Task.Run( () => {
                Uri indexUri = new Uri(dbNameUrlEncoded + "/_index/"+designDocId+"/json/"+indexName, UriKind.Relative);

                Task<HttpResponseMessage> httpTask = client.httpHelper.sendDelete (indexUri, null);
                httpTask.Wait();

                if(httpTask.Result.StatusCode == HttpStatusCode.OK)
                    return;
                else if(httpTask.Result.StatusCode == HttpStatusCode.NotFound)
                    throw new DataException(DataException.Database_IndexModificationFailure, 
                        string.Format("Index with name [{0}] and design doc [{1}] does not exist.",indexName, designDocId));
                else
                    throw new DataException(DataException.Database_IndexModificationFailure, 
                        string.Format("Error deleting index: {0}", httpTask.Result.Content.ReadAsStringAsync().Result));
            });
            return result;
        }

        // ======== PRIVATE HELPERS =============

        private Dictionary<String,Object> ConvertDocToJSONPayload(DocumentRevision rev){
            Debug.WriteLine ("==== enter Database::convertDocToJSONPayload(DocumentRevision)");
            Dictionary<String,Object> result = new Dictionary<String, object> ();

            if (rev.isDeleted) {
                Debug.WriteLine ("adding key:{0} value:true",DOC_DELETED);
                result.Add (DOC_DELETED, true);
            }

            if (rev.docId != null){
                Debug.WriteLine ("adding key:{0} value:{1}",DOC_ID,rev.docId);
                result.Add (DOC_ID, rev.docId);
            }

            if (rev.revId != null) {
                Debug.WriteLine ("adding key:{0} value:{1}",DOC_REV,rev.revId);
                result.Add(DOC_REV, rev.revId);
            }

            foreach (KeyValuePair<string,Object> entry in rev.body) {
                Debug.WriteLine("document body key/value: {0}, {1}",
                    entry.Key,
                    entry.Value);
                result.Add (entry.Key, entry.Value);
            }

            Debug.WriteLine ("==== exit Database::convertDocToJSONPayload");
            return result;
        }

        private Task<DocumentRevision> CreateDocumentRevision(DocumentRevision doc){
            Debug.WriteLine ("=== enter Database::createDocumentRevision()");
            try {
                Uri uri = null;
                if (doc.docId != null && doc.revId != null){
                    uri = new Uri (dbNameUrlEncoded + "/"+doc.docId+"?rev=" +doc.revId, UriKind.Relative);
                } else {
                    uri = new Uri( dbNameUrlEncoded, UriKind.Relative);
                }
                Debug.WriteLine ("reltive URL is: "+uri.ToString());

                if (doc.body == null) {
                    Dictionary<String,Object> empty = new Dictionary<String,Object> ();
                    doc.body = empty;
                }

                Dictionary<String,Object> payload = ConvertDocToJSONPayload (doc);
                Debug.WriteLine ("Create document payload is: "+JsonConvert.SerializeObject(payload));
                Task<DocumentRevision> result = Task.Run (() => {
                    // Send the HTTP request
                    Task<HttpResponseMessage> httpTask = client.httpHelper.sendPost(uri, null, payload);
                    Task<DocumentRevision> revisionTask = httpTask.ContinueWith( (antecedent) => {
                        if(httpTask.IsFaulted){
                            Debug.WriteLine ("HTTP request to create document task failed with error:"+httpTask.Exception.Message);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, httpTask.Exception.Message, httpTask.Exception);
                        }

                        // Check the HTTP status code
                        int httpStatus = (int)httpTask.Result.StatusCode;
                        if(httpStatus < 200 || httpStatus > 300){
                            string errorMessage = String.Format("Failed to create a new document.\nHTTP_Status: {0}\nErrorMessage: {1}",
                                httpStatus, httpTask.Result.ReasonPhrase);
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }

                        // Read in the response JSON into a Dictionary
                        Task<String> readContentTask = httpTask.Result.Content.ReadAsStringAsync();
                        readContentTask.Wait();
                        Dictionary<string, object> responseJSON = JsonConvert.DeserializeObject<Dictionary<string, object>> (readContentTask.Result); 
                        Debug.WriteLine("response JSON:{0}", JsonConvert.SerializeObject(responseJSON));

                        Object documentId;
                        responseJSON.TryGetValue(DOC_RESPONSE_ID,out documentId);
                        if (documentId == null || documentId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a documentId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }


                        Object revisionId;
                        responseJSON.TryGetValue(DOC_RESPONSE_REV,out revisionId);
                        if (revisionId == null || revisionId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a revisionId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }

                        // create new DocumentRevision with docId and revId returned
                        DocumentRevision createdDoc = new DocumentRevision((string)documentId,(string)revisionId,payload);
                        Debug.WriteLine ("returning DocumentRevision\n docId:{0}\n revId:{1}\n body:{2}",
                            documentId,
                            revisionId,
                            JsonConvert.SerializeObject(payload));

                        return createdDoc;
                    });

                    return revisionTask;
                });

                if(result.IsFaulted){
                    Debug.WriteLine ("HTTP request to create document task failed with error:"+result.Exception.Message);
                    throw new DataException(DataException.Database_SaveDocumentRevisionFailure, result.Exception.Message, result.Exception);
                }

                Debug.WriteLine ("=== exit Database::createDocumentRevision");
                return result;

            } catch (Exception e) {
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, e.Message, e);
            }

        }
        private Task<DocumentRevision> UpdateDocumentRevision(DocumentRevision doc){
            Debug.WriteLine ("=== enter updateDocumentRevision()");
            try {
                Uri uri = null;
                if (doc.docId != null){
                    uri = new Uri (dbNameUrlEncoded + "/"+doc.docId, UriKind.Relative);
                } else {
                    string errorMessage ="HTTP request to update document failed: the document revision must contain docId";
                    throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                }
                Debug.WriteLine ("relative URI is: "+uri.ToString());

                if (doc.body == null) {
                    Dictionary<String,Object> empty = new Dictionary<String,Object> ();
                    doc.body =empty;
                }

                Dictionary<String,Object> payload = ConvertDocToJSONPayload (doc);

                Debug.WriteLine ("Update document payload is: "+JsonConvert.SerializeObject(payload));
                Task<DocumentRevision> result = Task.Run (() => {
                    // Send the HTTP request
                    Task<HttpResponseMessage> httpTask = client.httpHelper.sendPut(uri, null, payload);
                    Task<DocumentRevision> revisionTask = httpTask.ContinueWith( (antecedent) => {
                        if(httpTask.IsFaulted){
                            Debug.WriteLine ("HTTP request to create document task failed with error:"+httpTask.Exception.Message);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, httpTask.Exception.Message, httpTask.Exception);
                        }

                        // Check the HTTP status code
                        int httpStatus = (int)httpTask.Result.StatusCode;
                        if(httpStatus < 200 || httpStatus > 300){
                            string errorMessage = String.Format("Failed to update document.\nHTTP_Status: {0}\nErrorMessage: {1}",
                                httpStatus, httpTask.Result.ReasonPhrase);
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }

                        // Read in the response JSON into a Dictionary
                        Task<String> readContentTask = httpTask.Result.Content.ReadAsStringAsync();
                        readContentTask.Wait();
                        Dictionary<String, Object> responseJSON = JsonConvert.DeserializeObject<Dictionary<string, object>> (readContentTask.Result); 

                        Object documentId;
                        responseJSON.TryGetValue(DOC_RESPONSE_ID,out documentId);
                        if (documentId == null || documentId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a documentId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }


                        Object revisionId;
                        responseJSON.TryGetValue(DOC_RESPONSE_REV,out revisionId);
                        if (revisionId == null || revisionId.Equals("")) {
                            string errorMessage = "\nJSON Response didn't contain a revisionId.";
                            Debug.WriteLine (errorMessage);
                            throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
                        }

                        // create new DocumentRevision with docId and revId returned
                        DocumentRevision createdDoc = new DocumentRevision((string)documentId,(string)revisionId,payload);
                        Debug.WriteLine ("returning DocumentRevision\n docId:{0}\n revId:{1}\n body:{2}",
                            documentId,
                            revisionId,
                            JsonConvert.SerializeObject(payload));

                        return createdDoc;
                    });
                    return revisionTask;
                });
                Debug.WriteLine ("==== exit Database::updateDocumentRevision");
                return result;
            } catch (Exception e) {
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, e.Message, e);
            }
        }


        /// <summary>
        /// Form a create index json from parameters
        /// </summary>
        /// <returns>The index definition.</returns>
        /// <param name="indexName">Index name.</param>
        /// <param name="designDocName">Design document name.</param>
        /// <param name="indexType">Index type.</param>
        /// <param name="fields">Fields.</param>
        private String GetIndexDefinition(String indexName, String designDocName,
            string indexType, IndexField[] fields) {

            if (fields == null || fields.Length == 0)
                throw new DataException(DataException.Database_IndexModificationFailure, "index fields must not be null or empty");
            
            bool addComma = false;
            string json = "{";
            if (!(indexName == null || indexName.Length==0 )) {
                json += "\"name\": \"" + indexName + "\"";
                addComma = true;
            }
            if (!(designDocName == null || designDocName.Length==0 )) {
                if (addComma) {
                    json += ",";
                }
                json += "\"ddoc\": \"" + designDocName + "\"";
                addComma = true;
            }
            if (!(indexType == null || indexType.Length==0)) {
                if (addComma) {
                    json += ",";
                }
                json += "\"type\": \"" + indexType + "\"";
                addComma = true;
            }

            if (addComma) {
                json += ",";
            }
            json += "\"index\": { \"fields\": [";
            for (int i = 0; i < fields.Length; i++) {
                json += "{\"" + fields[i].name + "\": " + "\"" + fields[i].sortOrder + "\"}";
                if (i + 1 < fields.Length) {
                    json += ",";
                }
            }

            return json + "] }}";
        }

        private String GetFindByIndexBody(String selectorJson,
            FindByIndexOptions options) {

            StringBuilder rf = null;
            if (options.GetFields().Count > 0) {
                rf = new StringBuilder("\"fields\": [");
                int i = 0;
                foreach (String s in options.GetFields()) {
                    if (i > 0) {
                        rf.Append(",");
                    }
                    rf.Append("\"").Append(s).Append("\"");
                    i++;
                }
                rf.Append("]");
            }

            StringBuilder so = null;
            if (options.GetSort().Count > 0) {
                so = new StringBuilder("\"sort\": [");
                int i = 0;
                foreach (IndexField idxfld in options.GetSort()) {
                    if (i > 0) {
                        so.Append(",");
                    }
                    so.Append("{\"")
                        .Append(idxfld.name)
                        .Append("\": \"")
                        .Append(idxfld.sortOrder)
                        .Append("\"}");
                    i++;
                }
                so.Append("]");
            }
                
            // needs to start with selector
            if (!(selectorJson.Trim().StartsWith("\"selector\""))) {
                throw new DataException(DataException.Database_QueryError, "selector JSON must begin with 'selector' keyworddocume");
            }

            StringBuilder finalbody = new StringBuilder();
            finalbody.Append("{" + selectorJson);

            if (rf != null) {
                finalbody.Append(",")
                    .Append(rf.ToString());
            }
            if (so != null) {
                finalbody.Append(",")
                    .Append(so.ToString());
            }
            if (options.GetLimit() > 0) {
                finalbody.Append(",")
                    .Append("\"limit\": ")
                    .Append(options.GetLimit());
            }
            if (options.GetSkip() > 0) {
                finalbody.Append(",")
                    .Append("\"skip\": ")
                    .Append(options.GetSkip());
            }
            if (options.GetReadQuorum() > 0) {
                finalbody.Append(",")
                    .Append("\"r\": ")
                    .Append(options.GetReadQuorum());
            }
            if (options.GetUseIndex() != null) {
                finalbody.Append(",")
                    .Append("\"use_index\": ")
                    .Append(options.GetUseIndex());
            }
            finalbody.Append("}");

            return finalbody.ToString();
        }

            
    }
}

