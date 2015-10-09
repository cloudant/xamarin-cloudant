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
using System.Diagnostics;
using System.Collections.Generic;

using Com.Cloudant.Client.Internal.Http;


namespace Com.Cloudant.Client
{
	/// <summary>
	/// Cloudant client builder.
	/// </summary>
	public class CloudantClientBuilder
	{
		/* Required parameters */

		/// <summary>
		/// The cloudant.com account hostname to connect to. For example
		/// sampleaccount.cloudant.com or http://sampleaccount.cloudant.com:1234
		/// </summary>
		public string accountName { get; }


		/* Optional parameters */

		/// <summary>
		/// The apiKey (if using an APIKey, else pass in the account for this parameter also)
		/// </summary>
		public string loginUsername  {set; get;}


		/// <summary>
		/// The Password credential
		/// </summary>
		public string password  {set; get;}


		/// <summary>
		/// The HTTP connection interceptors. <see cref="Com.Cloudant.Client.Internal.Http.IHttpConnectionInterceptor"/>
		/// </summary>
		public List<IHttpConnectionInterceptor> interceptors  {set; get;}


		/// <summary>
		/// Initializes a new instance of the <see cref="Com.Cloudant.Client.CloudantClientBuilder"/> class with the given account value.
		/// </summary>
		/// <param name="account">The cloudant.com account hostname to connect to. For example: sampleaccount.cloudant.com</param>
		public CloudantClientBuilder(string account)
		{
			if(string.IsNullOrWhiteSpace(account)){
				throw new ArgumentException ("must not be null or empty", "account");
			}
			this.accountName = account;
		}
			
		/// <summary>
		/// Builds a CloudantClient with the given parameters.
		/// </summary>
		/// <returns>A CloudantClient for the given account.</returns>
		public CloudantClient GetResult(){ 
			if(!string.IsNullOrWhiteSpace(loginUsername) && string.IsNullOrWhiteSpace(password)){
				throw new ArgumentException("loginUsername was set, but password is null or empty.");
			}
			if(!string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(loginUsername)){
				throw new ArgumentException("password was set, but loginUsername is null or empty.");
			}

			// Add additional validation here. For example, throw an error if 
			// both basic and cookie authorization are set at the same time. Cookie
			// authorization has not been implemented yet.

			return new CloudantClient(this);
		}
	}
}

