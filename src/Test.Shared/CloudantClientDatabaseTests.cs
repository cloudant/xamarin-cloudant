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
using System.Threading.Tasks;
using NUnit.Framework;
using IBM.Cloudant.Client;

namespace Test.Shared
{
	[TestFixture]
	public class CloudantClientDatabaseTests
	{
		
		private String DBName;
		private CloudantClient client;
		private Database db;

		[TestFixtureSetUp] //Runs once.
		public void FixtureSetUp(){
			client = new CloudantClientBuilder (TestConstants.account) {
				username = TestConstants.username,
				password = TestConstants.password
			}.GetResult ();
		}

		[SetUp] //Runs before each test.
		public void Setup ()
		{
			DBName = TestConstants.defaultDatabaseName + DateTime.Now.Ticks;
		}
			
		[TearDown] //Runs after each test.
		protected void tearDown() {
			if (db != null) {
				Task deleteDBTask = db.Delete ();
				deleteDBTask.Wait ();

				if (deleteDBTask.IsFaulted)
					Debug.WriteLine ("Failed to delete remote DB name: " + DBName + "\nError: " + deleteDBTask.Exception.Message);
			}
		}

		[Test]
		public void testEnsureExistsDoesntErrorWhenCalledTwice(){
			db = client.Database (DBName);
			Assert.DoesNotThrow (() => {
				db.EnsureExists ();
				db.EnsureExists ();
			});
		}

		[Test]
		public void testDBCreationGreenPath() {
			db = client.Database(DBName);
			db.EnsureExists ();
			Assert.NotNull(db);

			//Test db names are url encoded
			string databaseName = "az09_$()+-/";
			Assert.DoesNotThrow( () => {
				var newDb = client.Database(databaseName);
				newDb.EnsureExists();
				//Clean up
				newDb.Delete().Wait(); },
				"Test failed to create a database with name " + databaseName);
		}

			
		[Test]
		/// <summary>
		/// Tests validation of incorrect database names.
		/// </summary>
		public void testDBCreationInvalidNames(){
			
			// These invalid names produce a DataException 
			string [] invalidNames_DataException = new string[] { null, string.Empty};

			foreach (string name in invalidNames_DataException) {

				Assert.Throws<DataException> (() => {
					var db = client.Database (name);
					db.EnsureExists (); },
					"Test failed because invalid database name {0} should have produced an error. ", new []{name});
			}


			// The following invalid names produce an AggregateException because these are detected when an Http connection is attempted. 
			string [] invalidNames_AggreggateException = new string[] { "name with spaces", "InVaLidName", "inv@lid", "inval!d"};

			foreach (string name in invalidNames_AggreggateException) {

				Assert.Throws<ArgumentException> (() => {
					var db = client.Database (name);
					db.EnsureExists(); },
					"Test failed because invalid database name {0} should have produced an error. ", new []{name} );
			}
		}
	}
}

