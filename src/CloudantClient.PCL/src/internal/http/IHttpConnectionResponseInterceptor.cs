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

namespace IBM.Cloudant.Client
{
	/// <summary>
	/// A Response Interceptor is run after the response is obtained from the
	/// server but before the output stream is returned to the original client. The Response
	/// Interceptor enables two main behaviours:
	///   <list type="bullet">
	/// 	<item><term> (1) Modifying the response for every request </term></item>
	///		<item><term> (2) Replaying a (potentially modified) request by reacting to the response. </term></item>
	///	  </list>
	///
	///	Interceptors are executed in a pipeline and modify the context in a serial fashion.
	/// 
	/// See <see cref="Com.Cloudant.Client.Internal.Http.CookieInterceptor"/> for an example.
	/// </summary>
	/// <remarks>
	/// This is the interface for HTTP response interceptors.
	/// </remarks>
	public interface IHttpConnectionResponseInterceptor: IHttpConnectionInterceptor
	{
		/// <summary>
		/// Intercept the response
		///
		/// This method <strong>must not</strong> do any of the following
		///	  <list type="bullet">
		///		<item><term> (1) Return null. </term></item>
		///		<item><term> (2) Read the response stream. </term></item>
		///	  </list>
		/// </summary>
		/// <returns>Output context</returns>
		/// <param name="context">Input context</param>
		HttpConnectionInterceptorContext InterceptResponse(HttpConnectionInterceptorContext context);
	}
}

