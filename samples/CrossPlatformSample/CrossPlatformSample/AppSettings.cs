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

namespace CrossPlatformSample
{
	public static class AppSettings
	{
		/// <summary>
		/// The cloudant.com account hostname to connect to. For example
		/// sampleaccount.cloudant.com
		/// </summary>
		public static readonly string account = "your-cloudant-username.cloudant.com";

		/// <summary>
		/// The cloudant user ID.
		/// </summary>
		public static readonly string username = "your-cloudant-username";

		/// <summary>
		/// The authentication credential for user ID speficied in <see cref="CrossPlatformSample.username"/>
		/// </summary>
		public static readonly string password = "your-cloudant-password";
	}
}
