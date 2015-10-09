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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Com.Cloudant.Client.Internal.Http;

namespace Com.Cloudant.Client
{
	/// <summary>
	/// The main object for the Cloudant client public API.
	/// </summary>
	public class CloudantClient
	{
		private Uri accountUri;
		private String username;
		private String password;

		/// <summary>
		/// HttpHelper object used by this client instance to execute http requests. 
		/// </summary>
		public HttpHelper httpHelper;


		/// <summary>
		/// Constructs a new instance of this class and connects to the cloudant server using a builder class.
		/// </summary>
		/// <param name="builder">Builder class. <see cref="Com.Cloudant.Client.CloudantClientBuilder"/> </param>
		public CloudantClient(CloudantClientBuilder builder){
			this.accountUri = builder.accountUri;
			this.username = builder.username;
			this.password = builder.password;

			initHttpHelper (builder.interceptors);

			if (!string.IsNullOrWhiteSpace (username) && !string.IsNullOrWhiteSpace (password)) {
				httpHelper.addGlobalHeaders ("Authorization", "Basic " + Convert.ToBase64String (System.Text.UTF8Encoding.UTF8.GetBytes (username + ":" + password)));
			}
		}


		/// <summary>
		/// Gets a Database reference
		/// </summary>
		/// <param name="dbname">name of database to access</param>
		/// <param name="create">flag indicating whether to create the database if it does not exist.</param>
		/// <returns>A Task with the Database object instance.</returns>
		public Task<Database> database(String dbname, Boolean create) {

			if(string.IsNullOrEmpty(dbname))
				throw new DataException(DataException.Database_DatabaseModificationFailure, "Database name parameter may not be null or empty.");

			if (create) {
				return createDB (this, dbname);
			} else {
				return Task<Database>.Run (() => {
					return new Database (this, dbname);
				});
			}
		}



		/// <summary>
		/// Deletes the database.
		/// </summary>
		/// <returns>A Task to mointor this action.</returns>
		/// <param name="db">Database</param>
		public Task deleteDB(Database db){
			Debug.WriteLine("enter CloudantClient::deleteDB() name:"+db.dbname);

			Task result = Task.Run (() => {
				Task<HttpResponseMessage> deleteTask = httpHelper.sendDelete (new Uri(WebUtility.UrlEncode(db.dbname), UriKind.Relative) , null);

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
			Debug.WriteLine ("==== exit CloudantClient::deleteDB");
			return result;
		}

		// ======== PRIVATE HELPERS =============


		private Task<Database> createRemoteDatabase(Database db, Uri uri){
			Debug.WriteLine ("CloudantClient::createRemoteDatabase(Uri)");
			Database outerself = db;
			Task<Database> result = Task.Run (() => {
				Task<HttpResponseMessage> httpTask = httpHelper.sendPut (uri, null, null);
				httpTask.Wait ();

				if (httpTask.IsFaulted) {
					string errorMessage = string.Format ("Error occurred during creation of remote database at URL: {0}",
						uri.ToString () + ".  Error: " + httpTask.Exception.Message);
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
				return (Database)outerself;
			});
			Debug.WriteLine ("==== exit CloudantClient::createRemoteDatabase");
			return result;
		}

		private static Task<Database> createDB(CloudantClient client, String dbname){
			Debug.WriteLine ("==== enter CloudantClient::createDB(CloudantClient,String)");
			Uri uri = new Uri (client.accountUri, WebUtility.UrlEncode(dbname));
			Debug.WriteLine ("Database::createDB  uri: " + uri);

			if (uri == null) {
				Debug.WriteLine ("ERROR: url parameter cannot be null");
				throw new DataException(DataException.Database_DatabaseModificationFailure, "url parameter cannot be null");
			}

			string urlString = uri.ToString();
			if(urlString.EndsWith("/"))
				urlString = urlString.Substring(0, urlString.Length-1);


			Uri actualUri;
			if(!Uri.TryCreate(urlString, UriKind.Absolute, out actualUri)){
				Debug.WriteLine("ERROR:url parameter invalid");
				throw new DataException(DataException.Database_DatabaseModificationFailure, "url parameter invalid");
			}


			String remoteName = actualUri.AbsolutePath;
			if (remoteName == null || remoteName.Length==0 || remoteName.Equals("/") ) {
				Debug.WriteLine("ERROR: database name cannot be null or empty string.");
				throw new DataException(DataException.Database_DatabaseModificationFailure, "database name cannot be null or empty string.");
			}

			string [] paths = remoteName.Split("/".ToCharArray());
			remoteName = paths[paths.Length - 1];

			Database db = new Database (client, dbname);

			Debug.WriteLine ("==== exit CloudantClient::createDB");
			return client.createRemoteDatabase(db,actualUri);
		}


		private void initHttpHelper(List<IHttpConnectionInterceptor> interceptors){
			if (interceptors != null) {
				List<IHttpConnectionRequestInterceptor> requestInterceptors = new List<IHttpConnectionRequestInterceptor> ();
				List<IHttpConnectionResponseInterceptor> responseInterceptors = new List<IHttpConnectionResponseInterceptor> ();

				foreach (IHttpConnectionInterceptor httpConnInterceptor in interceptors) {
					if(httpConnInterceptor == null || 
						! (httpConnInterceptor is IHttpConnectionRequestInterceptor || httpConnInterceptor is IHttpConnectionResponseInterceptor))
						throw new DataException(DataException.Configuration_InvalidHttpInterceptor, string.Format("Http interceptors must implement either IHttpConnectionRequestInterceptor or " +
							"IHttpConnectionResponseInterceptor interfaces. Class {0} doesn't implement either if these interfaces.",httpConnInterceptor.GetType()));

					if (httpConnInterceptor is IHttpConnectionRequestInterceptor)
						requestInterceptors.Add (httpConnInterceptor as IHttpConnectionRequestInterceptor);

					if (httpConnInterceptor is IHttpConnectionResponseInterceptor)
						responseInterceptors.Add (httpConnInterceptor as IHttpConnectionResponseInterceptor);
				}

				httpHelper = new HttpHelper ( accountUri, requestInterceptors, responseInterceptors);

			} else {
				httpHelper = new HttpHelper (accountUri);
			}
		}
	}
}