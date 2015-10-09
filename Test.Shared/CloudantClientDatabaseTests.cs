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
using Com.Cloudant.Client;

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
				loginUsername = TestConstants.loginUsername,
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
				Task deleteDBTask = client.deleteDB(db);
				deleteDBTask.Wait ();

				if (deleteDBTask.IsFaulted)
					Debug.WriteLine ("Failed to delete remote DB name: " + DBName + "\nError: " + deleteDBTask.Exception.Message);
			}
		}

		[Test]
		public void testDBCreationGreenPath() {
			Task<Database> remoteDBTask = client.database(DBName, true);
			remoteDBTask.Wait ();

			if(remoteDBTask.IsFaulted){
				Assert.Fail ("Create database failed.  Cause: " + remoteDBTask.Exception.Message );
			}else{
				Assert.False (remoteDBTask.IsFaulted, "Create database failed.  Cause: " + (remoteDBTask.IsFaulted ? remoteDBTask.Exception.Message : "") );
				db = remoteDBTask.Result;
				Assert.NotNull(db);
			}


			//Test db names are url encoded
			string databaseName = "az09_$()+-/";
			Assert.DoesNotThrow( () => {
				Task<Database> newDbTask = client.database(databaseName,true);
				newDbTask.Wait ();
				//Clean up
				client.deleteDB(newDbTask.Result).Wait(); },
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
					Task<Database> invalidDBNameTask = client.database (name, true);
					invalidDBNameTask.Wait (); },
					"Test failed because invalid database name {0} should have produced an error. ", new []{name});
			}


			// The following invalid names produce an AggregateException because these are detected when an Http connection is attempted. 
			string [] invalidNames_AggreggateException = new string[] { "name with spaces", "InVaLidName", "inv@lid", "inval!d"};

			foreach (string name in invalidNames_AggreggateException) {

				Assert.Throws<AggregateException> (() => {
					Task<Database> invalidDBNameTask = client.database (name, true);
					invalidDBNameTask.Wait (); },
					"Test failed because invalid database name {0} should have produced an error. ", new []{name} );
			}
		}
			

		[Test]
		/// <summary>
		/// Tests the database creation method is asynchronous.
		/// </summary>
		public void testAsyncDBCreation(){
			
			Task<Database> remoteDBTask = client.database(DBName, true);
			long l = 0;
			while(!remoteDBTask.IsCompleted){
				l++;  //This operation is ocurring asynchronous to store creation.
			}
			Assert.True (l > 100, "Test failed because database creation didn't completed asynchronously.");
		}
	}
}

