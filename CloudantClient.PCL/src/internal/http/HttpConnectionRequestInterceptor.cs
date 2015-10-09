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

namespace Com.Cloudant.Client.Internal.Http
{
	/// <summary>
	/// A Request Interceptor is run before the request is made to the server. It can use headers to add support
	/// for other authentication methods, for example cookie authentication. 
	/// See <see cref="Com.Cloudant.Client.Internal.Http.HttpConnectionInterceptorContext"/> 
	/// for an example.
	///
	///	Interceptors are executed in a pipeline and modify the context in a serial fashion.
	/// </summary>
	public interface HttpConnectionRequestInterceptor : IHttpConnectionInterceptor
	{

		/// <summary>
		/// Intercept the request.
		/// This method <strong>must not</strong> do any of the following:
		///	  <list type="bullet">
		///		<item><description> (1) Return null. </description></item> 
		/// 	<item><description> (2) Call methods on the underlying HttpRequestMessage which initiate a request.  </description></item> 
		///   </list>
		/// </summary>
		/// <returns>Output context</returns>
		/// <param name="context">Input context</param>
		HttpConnectionInterceptorContext InterceptRequest(HttpConnectionInterceptorContext context);
	}
}

