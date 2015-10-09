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

namespace Test.Shared
{
	public class TestConstants
	{
		/// <summary>
		/// The cloudant.com account hostname to connect to. For example
		/// sampleaccount.cloudant.com or http://sampleaccount.cloudant.com:1234
		/// </summary>
		public static readonly string account =  "your-cloudant-account";

		/// <summary>
		/// The cloudant user ID.
		/// </summary>
		public static readonly string loginUsername = "your-cloudant-username";

		/// <summary>
		/// The password credential for user ID speficied in <see cref="Test.Shared.TestConstants.cloudantUser"/>
		/// </summary>
		public static readonly string password = "your-cloudant-password";

		/// <summary>
		/// The default name of the database for all tests.  Most tests will append a date time stamp to make this name unique.
		/// </summary>
		public static readonly string defaultDatabaseName = "xamarintests";



		public static string validTestUrl(){			
			return string.Format (@"http://{0}:{1}@{2}/", loginUsername, password, account);
		}


		/// <summary>
		/// Initializes the <see cref="Test.Shared.TestConstants"/> class and validates the required test settings have been configured.
		/// </summary>
		static TestConstants(){
			
			//TODO: Load constants from properties or configuration file.
			validateTestSettings ();
		}
			
		public static void validateTestSettings(){
			if(	   string.IsNullOrWhiteSpace(account) 	|| account.StartsWith("your-cloudant") 
				|| string.IsNullOrWhiteSpace(loginUsername) 		|| loginUsername.StartsWith("your-cloudant") 
				|| string.IsNullOrWhiteSpace(password) 	|| password.StartsWith("your-cloudant")){

				Console.Error.WriteLine ("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
				Console.Error.WriteLine ("!!! ERROR: Tests failed because you have not configured the cloudant !!!");
				Console.Error.WriteLine ("!!!        account, loginUsername, or password in TestConstants.cs   !!!"); 
				Console.Error.WriteLine ("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");

				throw new Exception("Tests failed because you have not configured your cloudant account, loginUsername, or password in TestConstants.cs");
			}
		}

	}
}

