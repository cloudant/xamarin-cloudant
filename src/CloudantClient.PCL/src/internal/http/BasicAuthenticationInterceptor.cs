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
using System.Net.Http.Headers;

namespace IBM.Cloudant.Client
{
	/// <summary>
	/// Sample interceptor implementation that adds basic authentication to HTTP requests.
	/// 
	/// It does this by adding an Authentication header with Basic authentication using the provided username and password.
	/// </summary>
	/// <remarks>
	/// This class is used for adding basic authentication to HTTP requests.
	/// </remarks>
	public class BasicAuthenticationInterceptor : IHttpConnectionRequestInterceptor, IHttpConnectionResponseInterceptor
	{
		
		private AuthenticationHeaderValue authHeader;

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.Internal.Http.BasicAuthenticationInterceptor"/> class 
		/// with the specified username and password. 
		/// </summary>
		/// <param name="username">Username for the http request.</param>
		/// <param name="password">Password for the user specified by username.</param>
		public BasicAuthenticationInterceptor (string username, string password)
		{
			authHeader = new AuthenticationHeaderValue ("Basic", Convert.ToBase64String (System.Text.UTF8Encoding.UTF8.GetBytes (username+":"+password)));
		}


		/// <summary>
		/// Intercepts the HttpRequest before it is sent.
		/// </summary>
		/// <returns>The http connection interceptor context with any modifications.</returns>
		/// <param name="context">Http connection interceptor context with the current state.</param>
		public HttpConnectionInterceptorContext InterceptRequest(HttpConnectionInterceptorContext context){
			
			context.requestMsg.Headers.Authorization = authHeader;
			return context;
		}

		/// <summary>
		/// Intercepts the HttpResponse before it is consumed.
		/// </summary>
		/// <returns>The http connection interceptor context</returns>
		/// <param name="context">Http connection interceptor context with the current state.</param>
		public HttpConnectionInterceptorContext InterceptResponse(HttpConnectionInterceptorContext context){

			if (context.responseMsg.StatusCode == System.Net.HttpStatusCode.Unauthorized) {

				//Handle authentication error here.

				context.replayRequest = true;
			}
			return context;
		}
	}
}

