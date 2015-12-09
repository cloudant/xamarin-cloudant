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
        // REST response fields
        private static readonly string DOC_RESPONSE_REV = "rev";

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
        public Database(CloudantClient client, String name)
        {
            //validate the dbName

            Regex strPattern = new Regex("^[a-z][a-z0-9_" + Regex.Escape("$") + Regex.Escape("(") + Regex.Escape(")") +
                                   Regex.Escape("+") + Regex.Escape("/") + "-]*$");

            if (!strPattern.IsMatch(name))
            {
                throw new ArgumentException("A database must be named with all lowercase letters (a-z), digits (0-9)," +
                    " or any of the _$()+-/ characters. The name has to start with a lowercase letter (a-z). ");
            }

            this.client = client;
            dbname = name;
            dbNameUrlEncoded = WebUtility.UrlEncode(name);
        }

        /// <summary>
        /// Ensures the database exists. 
        /// </summary>
        public async Task EnsureExistsAsync()
        {
            var dbUri = new Uri(client.accountUri.ToString() + dbNameUrlEncoded);
            var response = await client.httpHelper.PutAsync(dbUri, null, null).ConfigureAwait(continueOnCapturedContext: false);

            var httpStatus = (int)response.StatusCode;
            if (httpStatus != 200 && httpStatus != 201 && httpStatus != 412)
            {
                String errorMessage = String.Format("Failed to create remote database.\nHTTP_Status: {0}\nJSON Body: {1}",
                                          httpStatus, response.ReasonPhrase);
                Debug.WriteLine(errorMessage);
                throw new DataException(DataException.Database_DatabaseModificationFailure,
                    errorMessage);
            }

        }

        /// <summary>
        /// Deletes the database this object represents.
        /// </summary>
        /// <returns>A Task to monitor this action.</returns>
        public async Task DeleteAsync()
        {
            var response = await client.httpHelper.DeleteAsync(
                               new Uri(
                                   WebUtility.UrlEncode(dbname),
                                   UriKind.Relative),
                               null).ConfigureAwait(continueOnCapturedContext: false);

            var httpStatus = response.StatusCode;
            if (httpStatus != System.Net.HttpStatusCode.OK)
            {
                string errorMessage = String.Format(
                                          "Failed to delete remote database.\nHTTP_Status: {0}\nJSON Body: {1}",
                                          httpStatus,
                                          Database.DeserializeObject<Dictionary<string, object>>(await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false)));
                throw new DataException(DataException.Database_DatabaseModificationFailure, errorMessage);
            } 
        }

        /// <summary>
        /// Create the specified document in the database.
        /// </summary>
        /// <param name="revision">Revision to create.</param>
        public async Task<DocumentRevision> CreateAsync(DocumentRevision revision)
        {
            Debug.WriteLine("==== enter Database::Create(Object)");
            if (revision == null)
            {
                string errorMessage = "The input parameter revision cannot be null.";
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
            }
                
            string encodedDocId = null;
            if (revision.docId != null)
            {
                encodedDocId = Uri.EscapeDataString(revision.docId);
            }
            var uri = new Uri(dbNameUrlEncoded + "/" + encodedDocId, UriKind.Relative);
            Debug.WriteLine("reltive URL is: " + uri.ToString());

            var payload = Database.SerializeObject(revision);
            Debug.WriteLine("Create document payload is: " + Database.SerializeObject(payload));

            // Send the HTTP request
            HttpResponseMessage response;

            if (revision.docId != null)
            {
                response = await client.httpHelper.PutAsync(uri, null, payload).ConfigureAwait(continueOnCapturedContext: false);
            }
            else
            {
                response = await client.httpHelper.PostAsync(uri, null, payload).ConfigureAwait(continueOnCapturedContext: false);
            }

            // Check the HTTP status code
            var httpStatus = (int)response.StatusCode;
            if (httpStatus < 200 || httpStatus > 300)
            {
                var errorMessage = String.Format("Failed to create a new document.\nHTTP_Status: {0}\nErrorMessage: {1}",
                                       httpStatus, response.ReasonPhrase);
                Debug.WriteLine(errorMessage);
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
            }

            // Read in the response JSON into a Dictionary
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            var document = Database.DeserializeObject<DocumentRevision>(content);
            document.body = revision.body; //response doesn't contain document content
            return document;
        }

        /// <summary>
        /// Update a document with a new revision on the server. 
        /// The revision must have the id and revId of the document being updated.
        /// 
        /// </summary>
        /// <param name="revisionToUpdate">Revision to update.</param>
        public async Task<DocumentRevision> UpdateAsync(DocumentRevision revision)
        {
            Debug.WriteLine("==== enter Database::update(Object)"); 
            if (revision == null)
            {
                var errorMessage = "The input parameter revision cannot be null.";
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);

            }                

            if (revision.revId == null)
            {
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure,
                    "The document revision must contain a revId to perform an update");
            }
                
            Uri uri = null;

            if (revision.docId != null)
            {
                uri = new Uri(dbNameUrlEncoded + "/" + Uri.EscapeDataString(revision.docId), UriKind.Relative);
            }
            else
            {
                var errorMessage = "HTTP request to update document failed: the document revision must contain docId";
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
            }
            Debug.WriteLine("relative URI is: " + uri.ToString());

            if (revision.body == null)
            {
                var empty = new Dictionary<String,Object>();
                revision.body = empty;
            }

            var payload = Database.SerializeObject(revision);

            Debug.WriteLine("Update document payload is: " + Database.SerializeObject(payload));
            // Send the HTTP request
            var response = await client.httpHelper.PutAsync(uri, null, payload).ConfigureAwait(continueOnCapturedContext: false);

            // Check the HTTP status code
            int httpStatus = (int)response.StatusCode;
            if (httpStatus < 200 || httpStatus > 300)
            {
                var errorMessage = String.Format("Failed to update document.\nHTTP_Status: {0}\nErrorMessage: {1}",
                                       httpStatus, response.ReasonPhrase);
                Debug.WriteLine(errorMessage);
                throw new DataException(DataException.Database_SaveDocumentRevisionFailure, errorMessage);
            }

            // Read in the response JSON into a Dictionary
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            var document = Database.DeserializeObject<DocumentRevision>(content);
            document.body = revision.body; //response doesn't contain document content
            return document;
        }

        /// <summary>
        /// Reads the specified document, from the database.
        /// </summary>
        /// <param name="documentId">The id of the document to read.</param>
        public async Task<DocumentRevision> ReadAsync(String documentId)
        {
            Debug.WriteLine("==== enter Database::Read(String)");
            try
            {
                if (string.IsNullOrWhiteSpace(documentId))
                {
                    throw new DataException(DataException.Database_FetchDocumentRevisionFailure, "Unable to fetch document revision.  documentId " +
                        "parameter must not be null or empty");
                }

                var requestUrl = new Uri(dbNameUrlEncoded + "/" + Uri.EscapeDataString(documentId), UriKind.Relative);
                Debug.WriteLine("fetch document relative URI is: " + requestUrl.ToString());


                // Send the HTTP request
                var response = await client.httpHelper.GetAsync(requestUrl, null).ConfigureAwait(continueOnCapturedContext: false);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    // temp exception should be fixed as part of the common http handling issues #30 and #27
                    throw new DataException(
                        DataException.Database_FetchDocumentRevisionFailure,
                        "Error occured reading document");
                }

                // Read in the response JSON into a Dictionary
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);


                Debug.WriteLine(content);
                var document = Database.DeserializeObject<DocumentRevision>(content);
                return document;

            }
            catch (Exception e)
            {
                throw new DataException(DataException.Database_FetchDocumentRevisionFailure, e.Message, e);
            }
        }

        /// <summary>
        /// Delete the specified Document Revision.
        /// </summary>
        /// <param name="revision">Document revision to delete.</param>
        public async Task<String> DeleteAsync(DocumentRevision revision)
        {
            Debug.WriteLine("==== enter Database::delete(Object)");

            try
            {
                if (revision == null)
                {
                    throw new DataException(DataException.Database_DeleteDocumentRevisionFailure,
                        "Unable to delete document revision.  revision parameter must not be null");
                }

                var requestUrl = new Uri(
                                     dbNameUrlEncoded + "/"
                                     + Uri.EscapeDataString(revision.docId) + "?rev=" + Uri.EscapeDataString(revision.revId),
                                     UriKind.Relative);
                // Send the HTTP request
                var response = await client.httpHelper.DeleteAsync(requestUrl, null).ConfigureAwait(continueOnCapturedContext: false);

                // Read in the response JSON into a Dictionary
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                Dictionary<string, object> responseJSON = Database.DeserializeObject<Dictionary<string, object>>(content); 

                Debug.WriteLine("Cloudant delete document response.  Relative URI: " + requestUrl.ToString() + ". " +
                    "Document: " + revision.docId + "\nResponse content:" + content);

                // Check the HTTP status code
                int httpStatus = (int)response.StatusCode;
                if (httpStatus < 200 || httpStatus >= 300)
                {
                    string errorMessage = String.Format("Failed to delete document revision.\nRequest URL: {0}\nHTTP_Status: {1}\nJSONBody: {2}",
                                              requestUrl.ToString(), httpStatus, content);
                    Debug.WriteLine(errorMessage);
                    throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, errorMessage);
                }

                Object responseRevisionId;
                responseJSON.TryGetValue(DOC_RESPONSE_REV, out responseRevisionId);
                if (responseRevisionId == null || responseRevisionId.Equals(""))
                {
                    string errorMessage = "\ndocument delete JSON Response didn't contain a revisionId.";
                    Debug.WriteLine(errorMessage);
                    throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, errorMessage);
                }
                return (string)responseRevisionId;
            }
            catch (Exception e)
            {
                throw new DataException(DataException.Database_DeleteDocumentRevisionFailure, e.Message, e);
            }
        }

        ///<summary>
        /// Creates a Cloudant Query index of type <c>json</c>
        /// </summary>
        /// <param name="fields"> The fields of which to index, specified using <see href="https://docs.cloudant.com/cloudant_query.html#sort-syntax">Sort Syntax</see> </param>
        /// <param name="indexName"> Optional. The name to call this index, if ommited, CouchDB will generate the name for the index. </param>
        /// <param name="designDocumentName"> Optional. The name of the design document to save this index definition</param>
        /// <returns>A Task that represents the async operation</returns>
        public async Task CreateJsonIndexAsync(IList<SortField> fields,
                                               string indexName = null,
                                               string designDocumentName = null)
        {

            //validate fields for sort syntax compliance

            var requestDict = new Dictionary<string, object>()
            {
                ["type" ] = "json"
            };

            var convertedFieldList = new List<object>();
            foreach (SortField sort  in fields)
            {
                if (sort.sort != null)
                {
                    convertedFieldList.Add(new Dictionary<string,string>()
                        {
                        [sort.name ] = sort.sort.ToString()
                        });
                }
                else
                {
                    convertedFieldList.Add(sort.name);
                }
            }
                

            var index = new Dictionary<string,object>()
            {
                ["fields" ] = convertedFieldList
            };

            requestDict.Add("index", index);


            if (indexName != null)
            {
                requestDict.Add("name", indexName);
            }

            if (designDocumentName != null)
            {
                requestDict.Add("ddoc", designDocumentName);
            }
                
            await this.CreateIndexAsync(requestDict).ConfigureAwait(continueOnCapturedContext: false);
        }

        /// <summary>
        /// Creates a Cloudant Query index of type <c>text</c>
        /// </summary>
        /// <returns>A task which represents the async operation</returns>
        /// <param name="fields">Optional - The fields to index, this must follow the format specified in the
        /// <see href="https://docs.cloudant.com/cloudant_query.html#creating-an-index">Cloudant documentation</see> </param>
        /// <param name="indexName"></param>
        /// <param name="designDocumentName"></param>
        /// <param name="selector">Optional - A selector to select documents based on a query</param>
        /// <param name="defaultFieldEnabled">Optional (defaults to false)- Enables the default_field for the index, 
        /// default_field needs to be enabled in order to use the<c>$text</c> operator in queries.</param>
        /// <param name="defaultFieldAnalyzer">Optional, CouchDb will use the default analyzer if one is not specified. 
        /// This specifies the anayalzer to use for $text query operations</param>
        public async Task CreateTextIndexAsync(IList<TextIndexField> fields = null,
                                               string indexName = null, 
                                               string designDocumentName = null,
                                               IDictionary<string,object> selector = null,
                                               Boolean defaultFieldEnabled = false,
                                               string defaultFieldAnalyzer = null)
        {
            var index = new Dictionary<string,object>();
            var requestDict = new Dictionary<string, object>()
            {
                ["type" ] = "text",
                ["index" ] = index,

            };

            if (indexName != null)
            {
                requestDict.Add("name", indexName);
            }

            if (designDocumentName != null)
            {
                requestDict.Add("ddoc", designDocumentName);
            }
                    
            if (fields != null)
            {


                //validate the fields
                var fieldsDictList = new List<Dictionary<string,string>>();

                if (fields.Count > 0)
                { //equal to zero will cause indexing all fields
                    foreach (TextIndexField sf in fields)
                    {
                        fieldsDictList.Add(new Dictionary<string,string>()
                            {
                            ["name" ] = sf.name,
                            ["type" ] = sf.type.ToString().ToLower()
                            });
                    }
                }

                index.Add("fields", fieldsDictList);
            }

            if (selector != null)
            {
                index.Add("selector", selector);
            }


            var default_field = new Dictionary<String,Object>()
            {
                ["enabled" ] = defaultFieldEnabled

            };

            if (defaultFieldAnalyzer != null)
            {
                default_field.Add("analyzer", defaultFieldAnalyzer);
            }

            index.Add("default_field", default_field);

            await this.CreateIndexAsync(requestDict).ConfigureAwait(continueOnCapturedContext: false);
        }


        private async Task CreateIndexAsync(Dictionary<String,Object> indexDefinition)
        {
            Uri indexUri = new Uri(dbNameUrlEncoded + "/_index", UriKind.Relative);
            Debug.WriteLine("index relative URI: " + indexUri);

            var payload = Database.SerializeObject(indexDefinition);
            var response = await client.httpHelper.PostAsync(indexUri, null, payload).ConfigureAwait(continueOnCapturedContext: false);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
            { //Status code 200 or 201
                Debug.WriteLine(string.Format("Created Index: '{0}'", Database.SerializeObject(indexDefinition)));
                return;
            }
            else
            {
                Debug.WriteLine(string.Format(
                        "Error creating index : '{0}'",
                        Database.SerializeObject(indexDefinition)));
                throw new DataException(DataException.Database_IndexModificationFailure,
                    string.Format("Error creating index : '{0}'", Database.SerializeObject(indexDefinition))); 
            }
        }


        /// <summary>
        /// Query the database for documents matching a selector.
        /// </summary>
        /// <param name="selector">the selector to use to match documents</param>
        /// <param name="fields">Optional, a subset of fields to return for
        ///  each document matching the selector</param>
        /// <param name="limit">Optional, limit the number of results from the query</param>
        /// <param name="skip">Optional, skip this number of matching documents</param>
        /// <param name="sort">Optional, how to sort the matching documents</param>
        /// <param name="bookmark">Optional, text indexes only, a bookmark from where to continue receiving 
        /// results.</param>
        /// <param name="useIndex">Optional, The name of the index to use</param>
        /// <param name="r">Optional, the read quorum.  WARNING: This is an advanced option and is rarely, 
        /// if ever, needed. It will be detrimental to performance </param>
        /// <seealso href="https://docs.cloudant.com/cloudant_query.html#finding-documents-using-an-index">Cloudant 
        /// Documentation</seealso>
        public async Task<IList<DocumentRevision>> QueryAsync(IDictionary<string,object> selector,
                                                              IList<string> fields = null,
                                                              int limit = -1, 
                                                              int skip = -1,
                                                              IList<SortField> sort = null,
                                                              string bookmark = null,
                                                              string useIndex = null,
                                                              int r = -1)
        {
            if (selector == null)
            {
                throw new DataException(DataException.Database_QueryError, "selectorparameter cannot be null");
            }

            // build the body
            var body = new Dictionary<string,object>()
            {
                ["selector" ] = selector
                
            };

            if (fields != null)
            {
                body.Add("fields", fields);
            }

            if (limit > -1)
            {
                body.Add("limit", limit);
            }

            if (skip > -1)
            {
                body.Add("skip", skip);
            }

            if (sort != null)
            {
                var convertedSortList = new List<object>();
                foreach (SortField s  in sort)
                {
                    if (s.sort != null)
                    {
                        convertedSortList.Add(new Dictionary<string,string>()
                            {
                            [s.name ] = s.sort.ToString()
                            });
                    }
                    else
                    {
                        convertedSortList.Add(s.name);
                    }
                }
                body.Add("sort", convertedSortList);
            }

            if (bookmark != null)
            {
                body.Add("bookmark", bookmark);
            }

            if (useIndex != null)
            {
                body.Add("use_index", useIndex);
            }

            if (r > -1)
            {
                body.Add("r", r);
            }
                

            Uri indexUri = new Uri(dbNameUrlEncoded + "/_find", UriKind.Relative);
            var payload = Database.SerializeObject(body);

            var response = await client.httpHelper.PostAsync(indexUri, null, payload).ConfigureAwait(continueOnCapturedContext: false);

            // Check the HTTP status code
            int httpStatus = (int)response.StatusCode;
            if (httpStatus < 200 || httpStatus > 300)
            {
                string errorMessage = String.Format("findByIndex failed.\nHTTP_Status: {0}\nErrorMessage: {1}",
                                          httpStatus, response.ReasonPhrase);
                Debug.WriteLine(errorMessage);
                throw new DataException(DataException.Database_QueryError, errorMessage);
            }

            // Read in the response JSON into a Dictionary
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);

            var docs = Database.DeserializeObject<IList<DocumentRevision>>(content);
            return docs;

        }

        /// <summary>
        /// Lists all indices.
        /// </summary>
        /// <returns>List of Index</returns>
        public async Task<IList<Index>> ListIndicesAsync()
        {

            Uri indexUri = new Uri(dbNameUrlEncoded + "/_index/", UriKind.Relative);

            var response = await client.httpHelper.GetAsync(indexUri, null).ConfigureAwait(continueOnCapturedContext: false);


            if (response.StatusCode != HttpStatusCode.OK)
            {
                //temp exception, future PR for issues #30 and #27 will fix this
                throw new DataException(-1, "Failed to list database indices"); 
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
            return Database.DeserializeObject < IList<Index>>(content);
        }


        /// <summary>
        /// Delete an index
        /// </summary>
        /// <returns>A Task</returns>
        /// <param name="indexName">name of the index</param>
        /// <param name="designDocId">ID of the design doc</param>
        /// <param name="indexType">The type of index to delete</param>
        public async Task DeleteIndexAsync(String indexName, String designDocId, IndexType indexType)
        {

            if (string.IsNullOrWhiteSpace(indexName))
                throw new DataException(
                    DataException.Database_IndexModificationFailure,
                    "indexName may not be null or empty.");
            if (string.IsNullOrWhiteSpace(designDocId))
                throw new DataException(
                    DataException.Database_IndexModificationFailure,
                    "designDocId may not be null or empty");


            String indexTypeString = indexType.ToString();

            Uri indexUri = new Uri(
                               dbNameUrlEncoded + "/_index/" + designDocId + "/" + indexTypeString + "/" + indexName,
                               UriKind.Relative);

            var response = await client.httpHelper.DeleteAsync(indexUri, null);

            if (response.StatusCode == HttpStatusCode.OK)
                return;
            else if (response.StatusCode == HttpStatusCode.NotFound)
                throw new DataException(
                    DataException.Database_IndexModificationFailure, 
                    string.Format(
                        "Index with name [{0}] and design doc [{1}] does not exist.",
                        indexName,
                        designDocId));
            else
                throw new DataException(DataException.Database_IndexModificationFailure, 
                    string.Format("Error deleting index: {0}", await response.Content.ReadAsStringAsync()));
        }

        // ======== PRIVATE HELPERS =============

        private static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, Database.converters());
        }

        private static string SerializeObject(object obj)
        {
            return JsonConvert.SerializeObject(obj, Database.converters());
        }

        private static JsonConverter[] converters()
        {
            return new JsonConverter[]
            {
                new ListIndicesConverter(),
                new QueryDocumentRevisionConverter(),
                new SortFieldConverter(),
                new DocumentRevisionConverter(),
                new IndexConverter()
            };
        }
            
                        
    }
}

