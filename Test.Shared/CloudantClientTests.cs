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
using System.Threading.Tasks;
using NUnit.Framework;

using Com.Cloudant.Client;
using Com.Cloudant.Client.Internal.Http;

namespace Test.Shared
{

	[TestFixture ()]
	public class CloudantClientTests
	{

		private CloudantClient client;

		[TestFixtureSetUp]
		public void FixtureSetup(){
			client = new CloudantClientBuilder (TestConstants.account) {
				loginUsername = TestConstants.loginUsername,
				password = TestConstants.password
			}.GetResult ();
		}
			

		[Test()]
		public void TestGreenPath(){
			CloudantClient client=null; 

			//Test CloudantClient creation with valid parms.
			Assert.DoesNotThrow( () => 
				client = new CloudantClientBuilder (TestConstants.account) {
					loginUsername = TestConstants.loginUsername,
					password = TestConstants.password }.GetResult (),
				"Test failed while instantiationg a CloudantClient using valid parameters.");
			Assert.IsNotNull (client, "Test failed because client object must not be null after it has been built with valid parms.");


			//Test loginUsername is url encoded and accepts any characters.
			string user = "My u$erN@m3 !#:/%";
			Assert.DoesNotThrow( () => 
				new CloudantClientBuilder (TestConstants.account){
					loginUsername = user,
					password = TestConstants.password }.GetResult (),
				"Test falied creating a client with loginUsername: "+user);

			//Test password is url encoded and accepts any characters.
			string pass ="My p@$$w0rd !#:/%";
			Assert.DoesNotThrow( () => 
				new CloudantClientBuilder (TestConstants.account){
					loginUsername = TestConstants.loginUsername,
					password = pass }.GetResult (),
				"Test falied creating a client with password: "+pass);
		}

		/// <summary>
		/// Tests the Cloudant account could be entered in various different formats like:
		///  - <CLOUDANT-ACCOUNT>
		///  - http://<CLOUDANT-ACCOUNT>
		///  - http://<CLOUDANT-ACCOUNT>.cloudant.com
		///  - http://<CLOUDANT-ACCOUNT>.cloudant.com:1234
		///  - https://<CLOUDANT-ACCOUNT>
		/// </summary>
		[Test()]
		public void TestAccountParser(){
			string baseAccount = TestConstants.account;

			// First determine what is the Cloudant hostname, without scheme, port number, or cloudant.com 
			if (baseAccount.StartsWith ("http://"))
				baseAccount = baseAccount.Substring ("http://".Length);
			if(baseAccount.StartsWith ("https://"))
				baseAccount = baseAccount.Substring ("https://".Length);
			if (baseAccount.Contains (".cloudant.com"))
				baseAccount = baseAccount.Substring(0, baseAccount.IndexOf(".cloudant.com"));
			if (baseAccount.Contains (":")) //Removes any port specification
				baseAccount = baseAccount.Substring(0, baseAccount.LastIndexOf(":"));

			// Build a list of possible variations for the account parameter.
			List<string> validAccounts = new List<string> () {
				baseAccount,
				baseAccount + ".cloudant.com",
				"http://" + baseAccount,
				"http://" + baseAccount + ".cloudant.com",
				"http://" + baseAccount + ".cloudant.com:1234",
				"https://" + baseAccount,
				"https://" + baseAccount +".cloudant.com:1234"
			};

			// Test all values
			foreach (string account in validAccounts) {
				Assert.DoesNotThrow (() => new CloudantClientBuilder (account).GetResult (),
					"Test failed while parsing account " + account);
			}
		}


		/// <summary>
		/// Test a NoDocumentException is thrown when trying an operation on a DB that doesn't exist.
		/// </summary>
		[Test()]
		public void NonExistentDatabaseException(){
			string DBName = "database_doesnt_exist";
			Task<Database> dbTask = client.database (DBName, false);
			Assert.Throws<AggregateException> (() => dbTask.Result.listIndices ().Wait(),
				"Test failed checking that exception is thrown when a database doesn't exist.");
		}


		/// <summary>
		/// Validate that no exception bubbles up when trying to create a DB that already exists.
		/// </summary>
		[Test()]
		public void ExistingDatabaseCreateException(){
			Database database= null;
			try {
				//create a DB for this test
				Assert.DoesNotThrow(()=> database = client.database("cloudant_client_test", true).Result);

				//do a get with create true for the already existing DB
				Assert.DoesNotThrow(()=> database = client.database("cloudant_client_test", true).Result,
				"Test failed because an exception was thrown while attempting to create a database that already exists.");
			} finally {
				//clean up the DB created by this test
				client.deleteDB(database).Wait();
			}
		}


		/// <summary>
		/// Tests for invalid parameters while creating a CloudantClient object.
		/// </summary>
		[Test()]
		public void CloudantClientBuilderNegativeTests() {
			//Test Account must not be empty string.
			Assert.Throws<ArgumentException>(()=>new CloudantClientBuilder ("").GetResult (),
				"Test failed because an empty account parameter didn't report an error.");


			//Test Account must be a valid host.
			Assert.Throws<DataException>(()=>new CloudantClientBuilder ("invalid!host").GetResult (),
				"Test failed because invalid account parameter 'invalid!host' didn't report an error.");

			Assert.Throws<DataException>(()=>new CloudantClientBuilder ("host with spaces").GetResult (),
				"Test failed because invalid account parameter 'host with spaces' didn't report an error.");


			//Test when loginUsername is entered, password must not be null.
			Assert.Throws<ArgumentException>( () => 
				new CloudantClientBuilder (TestConstants.account){
					loginUsername = TestConstants.loginUsername}.GetResult (),
				"Test failed because a password wasn't specified, but loginUsername was set.");


			//Test when password is entered, loginUsername must not be null.
			Assert.Throws<ArgumentException>( () =>
				new CloudantClientBuilder (TestConstants.account){
					password = TestConstants.password}.GetResult (),
				"Test failed because loginUsername wasn't specified, but password was set.");

		}
	}

}

