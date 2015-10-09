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
using System.Net.Http;

namespace Com.Cloudant.Client.Internal.Http
{
	/// <summary>
	/// Class used to pass the current state of an Http connection to <see cref="Com.Cloudant.Client.Internal.Http.IHttpConnectionInterceptor"/> 
	/// </summary>
	public class HttpConnectionInterceptorContext
	{
		/// <summary>
		/// Indicates whether the HttpRequest should be replayed when it has been intercepted.
		/// </summary>
		/// <value><c>true</c> if the request should be replayed.</value>
		public bool replayRequest { set; get; }

		/// <summary>
		/// The HttpRequestMessage being processed.
		/// </summary>
		/// <value>The request message.</value>
		public HttpRequestMessage requestMsg { set; get; }

		/// <summary>
		/// The HttpResponseMessage being processed.
		/// </summary>
		/// <value>The response message.</value>
		public HttpResponseMessage responseMsg { set; get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.Internal.Http.HttpConnectionInterceptorContext"/> class with
		/// the given HttpRequest and HttpResponse message.
		/// </summary>
		/// <param name="requestMessage">Request message.</param>
		/// <param name="responseMessage">Response message.</param>
		public HttpConnectionInterceptorContext (HttpRequestMessage requestMessage, HttpResponseMessage responseMessage)
		{
			this.requestMsg = requestMessage;
			this.responseMsg = responseMessage;
		}
	}
}

