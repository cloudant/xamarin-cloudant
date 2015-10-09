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
using NUnit.Framework;

using Com.Cloudant.Client.Internal.Http;
using Com.Cloudant.Client;

namespace Test.Shared
{
	[TestFixture]
	public class InterceptorsTests
	{
		private CloudantClient client;
		private Database db;
		private String DBName;

		[SetUp] //Runs before each test.
		public void Setup ()
		{
			DBName = TestConstants.defaultDatabaseName + DateTime.Now.Ticks;
		}
			
		[TearDown] //Runs after each test.
		protected void tearDown() {
			if (db != null) {
				Task deleteDBTask = client.deleteDB(db);
				deleteDBTask.Wait ();

				if (deleteDBTask.IsFaulted)
					Debug.WriteLine ("Failed to delete remote DB name: " + DBName + "\nError: " + deleteDBTask.Exception.Message);
			}
		}

		[Test]
		public void testBasicAuthInterceptor() {
			BasicAuthenticationInterceptor basicAuthInterceptor = new BasicAuthenticationInterceptor (TestConstants.loginUsername, TestConstants.password);

			client = new CloudantClientBuilder (TestConstants.account) {
				interceptors = new List<IHttpConnectionInterceptor>(){basicAuthInterceptor}
			}.GetResult ();

			Task<Database> dbTask = client.database (DBName, true);

			Assert.DoesNotThrow( () => {
				dbTask.Wait ();
				db = dbTask.Result;},
				"Exception thrown while creating database using BasicAuth interceptor. ");

			Assert.False(dbTask.IsFaulted, "Create database task is failed.  Cause: " + (dbTask.IsFaulted ? dbTask.Exception.Message : "") );
			Assert.NotNull(db);

		}

		[Test]
		public void testCookieInterceptor() {
			CookieInterceptor cookieInterceptor = new CookieInterceptor (TestConstants.loginUsername, TestConstants.password);

			client = new CloudantClientBuilder (TestConstants.account) {
				interceptors = new List<IHttpConnectionInterceptor>(){cookieInterceptor}
			}.GetResult ();

			Task<Database> dbTask = client.database (DBName, true);

			Assert.DoesNotThrow( () => {
				dbTask.Wait ();
				db = dbTask.Result;},
				"Exception thrown while creating database using cookie interceptor. ");

			Assert.False(dbTask.IsFaulted, "Create database task is failed.  Cause: " + (dbTask.IsFaulted ? dbTask.Exception.Message : "") );
			Assert.NotNull(db);
		}
	}
}

