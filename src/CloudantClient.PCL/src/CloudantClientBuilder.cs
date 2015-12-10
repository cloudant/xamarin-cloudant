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


namespace IBM.Cloudant.Client
{
	/// <summary>
	/// Cloudant client builder.
	/// </summary>
	/// <remarks>
	/// This class is used for building a CloudantClient with specified options.
	/// </remarks>
	public class CloudantClientBuilder
	{
		/* Required parameters */

		/// <summary>
		/// The URI to the Cloudant system.
		/// </summary>
		/// <value>The Cloudant account URI.</value>
		public Uri accountUri { get; }

		/* Optional parameters */

		/// <summary>
		/// User ID in the Cloudant system.
		/// </summary>
		public string username  { set; get; }


		/// <summary>
		/// The authentication credential for the user ID specified in <see cref="IBM.Cloudant.Client.CloudantClientBuilder.username"/> 
		/// </summary>
		public string password  { set; get; }


		/// <summary>
		/// The HTTP connection interceptors. <see cref="IBM.Cloudant.Client.Internal.Http.IHttpConnectionInterceptor"/>
		/// </summary>
		public List<IHttpConnectionInterceptor> interceptors  { set; get; }


		/// <summary>
		/// Initializes a new instance of the <see cref="IBM.Cloudant.Client.CloudantClientBuilder"/> class with the given account name.
		/// </summary>
		/// <remarks>Connections will be created using the https protocol.</remarks>
		/// <param name="account">The cloudant.com account name to connect to. For example, if your Cloudant account is 
		/// accessed via the URL example.cloudant.com, pass example herer</param>
		public CloudantClientBuilder(string account)
		{
			if (string.IsNullOrWhiteSpace(account))
			{
				throw new ArgumentException("must not be null or empty", "account");
			}

			var hostNameType = Uri.CheckHostName(account);
			if ((hostNameType != UriHostNameType.Basic && hostNameType != UriHostNameType.Dns) || account.Contains("."))
			{
				throw new ArgumentException("Account parameter does not appear to contain a valid Cloudant account name. Did you provide a URL instead?");
			}

			string accountUriString = string.Format("https://{0}.cloudant.com", account);
			this.accountUri = new Uri(accountUriString, UriKind.Absolute);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="IBM.Cloudant.Client.CloudantClientBuilder"/> class.
		/// </summary>
		/// <param name="accountUri">URI for CouchDB or Cloudant Local instance.</param>
		public CloudantClientBuilder(Uri accountUri)
		{
			if (accountUri == null)
			{
				throw new ArgumentException("must not be null", "accountUri");
			}
			if (!accountUri.IsAbsoluteUri || accountUri.IsFile)
			{
				throw new ArgumentException("The 'accountUri' argument must be an absolute URI and must be a network location.", "accountUri");
			}

			// Strip the userinfo and fill the username and password field
			var builder = new UriBuilder(accountUri);
			if (builder.UserName != null)
			{
				this.username = builder.UserName;
				builder.UserName = null;
			}
			if (builder.Password != null)
			{
				this.password = builder.Password;
				builder.Password = null;
			}
			this.accountUri = builder.Uri;
		}


		/// <summary>
		/// Builds a CloudantClient with the given parameters.
		/// </summary>
		/// <returns>A CloudantClient for the given account.</returns>
		public CloudantClient GetResult()
		{ 
			if (!string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(password))
			{
				throw new ArgumentException("username was set, but password is null or empty.");
			}
			if (!string.IsNullOrWhiteSpace(password) && string.IsNullOrWhiteSpace(username))
			{
				throw new ArgumentException("password was set, but username is null or empty.");
			}

			// Add additional validation here. For example, throw an error if 
			// both basic and cookie authorization are set at the same time. Cookie
			// authorization has not been implemented yet.

			return new CloudantClient(this);
		}
	}
}

