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
using System.Net.Http;
using System.Threading.Tasks;

namespace Com.Cloudant.Client.Internal.Http
{
	/// <summary>
	/// Internal interface to define the http connection.
	/// </summary>
	public interface IHttpHelper {

		/// <summary>
		/// Sends an Http GET request to the given URI using the given headers.
		/// </summary>
		/// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
		/// <param name="uri">URI for this request.</param>
		/// <param name="headers">Http headers for this request.</param>
		Task<HttpResponseMessage> sendGet(Uri uri, Dictionary<String,String> headers);

		/// <summary>
		/// Sends an Http DELETE request to the given URI using the given headers.
		/// </summary>
		/// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
		/// <param name="uri">URI for this request.</param>
		/// <param name="headers">Http headers for this request.</param>
		Task<HttpResponseMessage> sendDelete (Uri uri, Dictionary<String, String> headers);
	
		/// <summary>
		/// Sends an Http PUT request to the given URI using the given headers and content.
		/// </summary>
		/// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
		/// <param name="uri">URI for this request.</param>
		/// <param name="headers">Http headers for this request.</param>
		/// <param name="body">The request contents.</param>
		Task<HttpResponseMessage> sendPut (Uri uri, Dictionary<String, String> headers, Dictionary<String, Object> body);

		/// <summary>
		/// Sends an Http POST request to the given URI using the given headers and content.
		/// </summary>
		/// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
		/// <param name="uri">URI for this request.</param>
		/// <param name="headers">Http headers for this request.</param>
		/// <param name="body">The request contents.</param>
		Task<HttpResponseMessage> sendPost (Uri uri, Dictionary<String, String> headers, Dictionary<String, Object> body);

		/// <summary>
		/// Adds global headers to be used by every request sent by this HttpHelper.
		/// </summary>
		/// <param name="name">Header name.</param>
		/// <param name="value">Header value.</param>
		void addGlobalHeaders (string name, string value);

	}
}

