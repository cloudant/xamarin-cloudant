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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Com.Cloudant.Client.Internal.Http
{
	/// <summary>
	/// Internal class to manage the http connection.
	/// </summary>
	public class HttpHelper : IHttpHelper
	{

		private static readonly String DEFAULT_CHARSET = "UTF-8";

		private HttpClient client = new HttpClient (new HttpClientHandler (){ UseCookies = false });// By setting UseCookies = false, we allow
																									// interceptors to set the Cookie header.

		public List<HttpConnectionRequestInterceptor> requestInterceptors { set; get; }   = new List<HttpConnectionRequestInterceptor> ();
		public List<HttpConnectionResponseInterceptor> responseInterceptors { set; get; } = new List<HttpConnectionResponseInterceptor> ();
		private int numberOfRetries = 10;


		public HttpHelper (){
		}

		public HttpHelper(List<HttpConnectionRequestInterceptor> requestInterceptors, List<HttpConnectionResponseInterceptor> responseInterceptors){
			Debug.WriteLine ("ENTRY HttpHelper: Constructor() - requestInterceptors: "+requestInterceptors+"  responseInterceptors: "+responseInterceptors);
			this.requestInterceptors = requestInterceptors;
			this.responseInterceptors = responseInterceptors;
		}

		public Task<HttpResponseMessage> sendGet(Uri uri, Dictionary<String,String> headers){
			Debug.WriteLine ("HttpHelper :: SendGet()");
			return SendJSONRequest (HttpMethod.Get, uri, headers, null);
		}

		public Task<HttpResponseMessage> sendDelete (Uri uri, Dictionary<String, String> headers){
			Debug.WriteLine ("HttpHelper :: SendDelete()");
			return SendJSONRequest (HttpMethod.Delete, uri, headers, null);
		}

		public Task<HttpResponseMessage> sendPut (Uri uri, Dictionary<String, String> headers, Dictionary<String, Object> body){
			Debug.WriteLine ("HttpHelper :: SendPut()");
			return SendJSONRequest (HttpMethod.Put, uri, headers, body);
		}

		public Task<HttpResponseMessage> sendPost (Uri uri, Dictionary<String, String> headers, Dictionary<String, Object> body){
			Debug.WriteLine ("HttpHelper :: SendPost()");
			return SendJSONRequest (HttpMethod.Post, uri, headers, body);
		}

		public void addGlobalHeaders (string name, string value){
			client.DefaultRequestHeaders.Add(name, value);
		}

		public void removeGlobalHeader(string name){
			client.DefaultRequestHeaders.Remove (name);
		}

		public void setNumberOfRetries(int numberOfRetries){
			this.numberOfRetries = numberOfRetries;
		}



		private Task<HttpResponseMessage> SendJSONRequest(HttpMethod method, Uri uri, Dictionary<string, string> headers, Dictionary<string, Object> body){

			Dictionary<string,string> actualHeaders = (headers == null) ? new Dictionary<string,string> () : new Dictionary<string,string> (headers);
			actualHeaders.Add ("Accept", "application/json"); 

			HttpContent content=null;
			if (body != null) {
				string json = JsonConvert.SerializeObject (body);

				content = new System.Net.Http.StringContent (json);
				content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset="+DEFAULT_CHARSET);	
			}
			return SendRequest(method,uri,actualHeaders,content);
		}
			


		private Task<HttpResponseMessage> SendRequest(HttpMethod method, Uri uri, Dictionary<string, string> headers, HttpContent content){
			Debug.WriteLine ("HttpHelper:sendRequest()  METHOD:"+method);

			Boolean retry = true;
			int n = numberOfRetries;

			Task<HttpResponseMessage> result = Task.Run( () => {
				while (retry && n-- > 0) {
					retry = false;

					using (var request = new System.Net.Http.HttpRequestMessage (method, uri)) {

						// If the URI has authentication infromation, use it.
						if(uri.UserInfo!=null && uri.UserInfo.Length>0)
							request.Headers.Authorization = new AuthenticationHeaderValue ("Basic", Convert.ToBase64String (System.Text.UTF8Encoding.UTF8.GetBytes (uri.UserInfo)));

						if (headers != null) {
							foreach (KeyValuePair<string,string> item in headers) { 

								request.Headers.TryAddWithoutValidation(item.Key, item.Value);  
							}
						}
				
						request.Content = content;

							
						//Call request interceptors
						HttpConnectionInterceptorContext currHttpConnContext = new HttpConnectionInterceptorContext (request, null);

						foreach (HttpConnectionRequestInterceptor requestInterceptor in requestInterceptors) {
							currHttpConnContext = requestInterceptor.InterceptRequest (currHttpConnContext);
						}


						//Process the request
						try {
							Debug.WriteLine (FormatHttpRequestLog (request));

							HttpResponseMessage response = client.SendAsync (request).Result as HttpResponseMessage;
							Debug.WriteLine (FormatHttpResponseLog (response));

							//Call response interceptors
							currHttpConnContext.responseMsg = response;
							foreach (HttpConnectionResponseInterceptor responseInterceptor in responseInterceptors) {
								currHttpConnContext = responseInterceptor.InterceptResponse (currHttpConnContext);
							}
								
							retry = currHttpConnContext.replayRequest;

							if(!retry)
								return response;
								
						} catch (AggregateException ae) {
							Debug.WriteLine ("=== Sent request, got EXCEPTION response. Message: " + ae.InnerException.Message);
							throw ae.InnerException;
						}
					}
				}

				Debug.WriteLine ("Maximum number of retries reached");
				throw new Exception ("Maximum number of retries reached.  An HttpConnectionResponseInterceptor has set the replayRequest " +
					"flag to true, but the maximum number of retries ["+numberOfRetries+"] has been reached.");
			});
				
			return result;
		}




		private string FormatHttpRequestLog(HttpRequestMessage request){
			string contentString = request.Content == null ? "null" : request.Content.ToString ();
			return string.Format ("Http Request\n   {0,-10}:{1}\n   {2,-10}:{3}\n   {4,-10}:{5}\n   {6,-10}:{7}\n   {8,-10}:{9}", 
				"METHOD", request.Method,
				"URI", SanitizeUri (request.RequestUri),
				"HEADERS", FormatAndSanitizeHeaders (request.Headers),
				"CONTENT", contentString,
				"PROPERTIES",FormatProperties(request.Properties));
		}

		private string FormatHttpResponseLog(HttpResponseMessage response){
			string contentString = response.Content == null ? "null" : response.Content.ToString ();
			return string.Format("Http Response\n   {0,-8}:[{1}] {2}\n   {3,-8}:{4}\n   {5,-8}:{6}\n   {7,-8}:{8}",
				"URI", response.RequestMessage.Method, SanitizeUri(response.RequestMessage.RequestUri),
				"STATUS", response.StatusCode,
				"HEADERS",FormatAndSanitizeHeaders(response.Headers),
				"CONTENT", contentString);
		}


		private string SanitizeUri(Uri uri){
			if(uri.UserInfo != null && uri.UserInfo.Length > 0) {
				return uri.Scheme + "://[USER]:[PASS]@" + uri.ToString ().Substring (uri.ToString ().IndexOf (uri.Host));
			}
			return uri.ToString ();
		}


		private string FormatAndSanitizeHeaders(HttpHeaders headers){
			
			if (headers == null)
				return "null";


			int count = 0;
			string result = string.Empty;			
			foreach (KeyValuePair<String,IEnumerable<string>> header in headers) {
				foreach (string value in header.Value) {
					if (header.Key == "Authorization")
						result += string.Format ("\n\t[{0}]{1} : {2}", count++, header.Key, value.Split (' ') [0] + " *****");
					else
						result += string.Format ("\n\t[{0}]{1} : {2}", count++, header.Key, value);
				}
			}
			return result;
		}

		private string FormatProperties(IDictionary<string, object> props){

			if (props == null)
				return "null";
			else if (props.Keys.Count == 0)
				return "none";


			string result = string.Empty;
			int count = 0;
			foreach (string key in props.Keys) {
				object value; 
				props.TryGetValue (key, out value);
				result += string.Format("\n\t[{0}] {1} : {2}", count++, key, value);
			}
			return result;
		}
	}
}