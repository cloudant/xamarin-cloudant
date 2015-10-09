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
using Newtonsoft.Json;

using Com.Cloudant.Client.Internal.Http;

namespace Com.Cloudant.Client
{
	/// <summary>
	/// The main object for the Cloudant client public API.
	/// </summary>
	public class CloudantClient
	{
		private String accountName;
		private String loginUsername;
		private String password;

		public HttpHelper httpHelper;

		public string accountURL { private set; get; }


		/// <summary>
		/// Constructs a new instance of this class and connects to the cloudant server using a builder class.
		/// </summary>
		/// <param name="builder">Builder class. <see cref="Com.Cloudant.Client.CloudantClientBuilder"/> </param>
		public CloudantClient(CloudantClientBuilder builder){
			this.accountName = builder.accountName;
			this.loginUsername = builder.loginUsername;
			this.password = builder.password;

			Dictionary<string,string> url = parseAccount (this.accountName);
			if (!string.IsNullOrWhiteSpace (loginUsername) && !string.IsNullOrWhiteSpace (password))
				this.accountURL = string.Format (@"{0}://{1}:{2}@{3}:{4}/", url["scheme"], WebUtility.UrlEncode(loginUsername), WebUtility.UrlEncode(password), url["hostname"], url["port"]);
			else
				this.accountURL = string.Format (@"{0}://{1}:{2}/", url["scheme"], url["hostname"], url["port"]);

			validateSettings ();
			initHttpHelper (builder.interceptors);
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
				Task<HttpResponseMessage> deleteTask = httpHelper.sendDelete (db.remoteUri, null);

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

		private void validateSettings(){
			// Validate the URL
			if(!string.IsNullOrWhiteSpace(accountURL)){
				try{
					new Uri(this.accountURL);
				} catch(FormatException e){
					throw new DataException (DataException.Account_InvalidAccountSettings, "One or more CloudantClient parameter is incorrect. "+e.Message);
				}
			}
				
		}


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
			Uri uri = new Uri (client.accountURL+ WebUtility.UrlEncode(dbname));
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
				List<HttpConnectionRequestInterceptor> requestInterceptors = new List<HttpConnectionRequestInterceptor> ();
				List<HttpConnectionResponseInterceptor> responseInterceptors = new List<HttpConnectionResponseInterceptor> ();

				foreach (IHttpConnectionInterceptor httpConnInterceptor in interceptors) {
					if (httpConnInterceptor != null && httpConnInterceptor is HttpConnectionRequestInterceptor)
						requestInterceptors.Add (httpConnInterceptor as HttpConnectionRequestInterceptor);

					if (httpConnInterceptor != null && httpConnInterceptor is HttpConnectionResponseInterceptor)
						responseInterceptors.Add (httpConnInterceptor as HttpConnectionResponseInterceptor);
				}

				httpHelper = new HttpHelper (requestInterceptors, responseInterceptors);
			} else {
				httpHelper = new HttpHelper ();
			}
		}


		private Dictionary<String, String> parseAccount(string account) {
			if(string.IsNullOrWhiteSpace(account)){
				throw new ArgumentException ("must not be null or empty", "account");
			}
			this.accountName = account;
			Debug.WriteLine("Parsing {0}", account);
			Dictionary<String, String> h = new Dictionary<String, String>();
			if (account.StartsWith("http://") || account.StartsWith("https://")) {
				// user is specifying a uri
				try {
					Uri uri = new Uri(account);

					h.Add("scheme", uri.Scheme );
					h.Add("hostname", uri.Host );
					h.Add("port", uri.Port.ToString() );

				} catch (Exception e) {
					throw new ArgumentException ("Error parsing account. See inner exception for details.", "account", e);
				}
			} else {
				h.Add("scheme", "https");
				h.Add ("hostname", account.Contains (".cloudant.com") ? account : account + ".cloudant.com");
				h.Add("port", "443");
			}
			return h;
		}
	}
}