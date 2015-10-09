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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Com.Cloudant.Client.Internal.Http
{
	/// <summary>
	/// Adds cookie authentication support to http requests.
	///
	/// It does this by adding the cookie header for CouchDB
	///	using request interceptor pipeline in <see cref="Com.Cloudant.Client.Internal.Http.HttpHelper"/>.
	///
	/// If a response has a response code of 401, it will fetch a cookie from
	/// the server using provided credentials and tell <see cref="Com.Cloudant.Client.Internal.Http.HttpHelper"/>
	/// to reply the request by setting <see cref="Com.Cloudant.Client.Internal.Http.HttpConnectionInterceptorContext.replayRequest"/>
	/// property to true.
	///
	/// If the request to get the cookie for use in future request fails with a 401 status code
	///	(or any status that indicates client error) cookie authentication will not be attempted again.
	/// </summary>
	/// <remarks>
	/// This class is used for adding cookie authentication to HTTP requests.
	/// </remarks>
	public class CookieInterceptor : IHttpConnectionRequestInterceptor, IHttpConnectionResponseInterceptor
	{
		private HttpClient client = new HttpClient();

		private string cookie = null;
		private Boolean shouldAttemptCookieRequest = true;
		private string username;
		private string password;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.Internal.Http.CookieInterceptor"/> class.
		/// </summary>
		/// <param name="username">The username to use when getting the cookie</param>
		/// <param name="password">The password to use when getting the cookie</param>
		public CookieInterceptor (string username, string password)
		{
			this.username = username;
			this.password = password;
		}


		/// <summary>
		/// Intercepts the HttpRequest before it is sent.
		/// </summary>
		/// <returns>The http connection interceptor context with any modifications.</returns>
		/// <param name="context">Http connection interceptor context with the current state.</param>
		public HttpConnectionInterceptorContext InterceptRequest(HttpConnectionInterceptorContext context){

			HttpRequestMessage requestMeg = context.requestMsg;

			if (shouldAttemptCookieRequest) {
				if (cookie == null) {
					cookie = getCookie (context.requestMsg.RequestUri);
				}
			
				context.requestMsg.Headers.Add ("Cookie", cookie);
			}

			return context;
		}

		/// <summary>
		///  Intercepts the HttpResponse before it is consumed.
		/// </summary>
		/// <returns>The http connection interceptor context</returns>
		/// <param name="context">Http connection interceptor context with the current state</param>
		public HttpConnectionInterceptorContext InterceptResponse(HttpConnectionInterceptorContext context) {

			HttpResponseMessage responseMsg = context.responseMsg;

			try {
				if ((int)responseMsg.StatusCode == 401) {
					//we need to get a new cookie
					cookie = getCookie(responseMsg.RequestMessage.RequestUri);
					//don't resend request, failed to get cookie
					if(cookie != null) {
						context.replayRequest = true;
					} else {
						context.replayRequest = false;
					}
				}
			} catch (Exception e) {
				Debug.WriteLine("Failed to process response interceptor. "+e.Message);
			}

			return context;
		}


		private string getCookie(Uri uri){
			try {
				Uri sessionUri = new Uri(uri.Scheme+"://"+uri.Host+"/_session");

				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, sessionUri);

				var encodedContent = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("name", this.username),
					new KeyValuePair<string, string>("password", this.password),
				};

				request.Content = new FormUrlEncodedContent(encodedContent);
				request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");


				HttpResponseMessage response = client.SendAsync (request).Result as HttpResponseMessage;

				int responseCode = (int)response.StatusCode;


				if(responseCode / 100 == 2){
					string cookieHeader ="";
					foreach(String value in response.Headers.GetValues("Set-Cookie")){
						cookieHeader = value;
					}

					if(SessionHasStarted(response.Content)){
						return cookieHeader;
					} else {
						return null;
					}

				} else if(responseCode == 401){
					shouldAttemptCookieRequest  = false;
					Debug.WriteLine("Credentials are incorrect, cookie authentication will not be" +
						" attempted again by this interceptor object");
				} else if (responseCode / 100 == 5){
					Debug.WriteLine(string.Format("Failed to get cookie from server, response code {0}, cookie auth",
						responseCode));
				}  else {
					// catch any other response code
					Debug.WriteLine(string.Format("Failed to get cookie from server, response code {0}, " +
						"cookie authentication will not be attempted again",
						responseCode));
					shouldAttemptCookieRequest = false;
				}
			} catch (AggregateException ae){
				Debug.WriteLine ("Failed to get cookie. " + ae.InnerException.Message);

			} catch (Exception e){
				Debug.WriteLine ("Failed to get cookie. " + e.Message);
			}
				
			return null;
		}

		private Boolean SessionHasStarted(HttpContent content){
			//check the response body
			Dictionary<string,object> jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>  (content.ReadAsStringAsync ().Result);

			// only check for ok:true, https://issues.apache.org/jira/browse/COUCHDB-1356
			// means we cannot check that the name returned is the one we sent.

			Object obj;
			jsonResponse.TryGetValue("ok", out obj);

			if(obj is Boolean)
				return (Boolean)obj;

			return false;
		}
	}
}