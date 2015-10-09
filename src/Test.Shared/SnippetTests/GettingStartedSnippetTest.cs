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
using NUnit.Framework;
using System;
using Com.Cloudant.Client;
using System.Collections.Generic;
using Com.Cloudant.Client.Model;
using System.Threading.Tasks;

namespace Test.Shared
{
	/// <summary>
	/// Test the snippets included in .component/GettingStarted.md
	/// </summary>
	[TestFixture ()]
	public class GettingStartedSnippetTest
	{
		[Test ()]
		public void TestCase ()
		{

			////////////////////  SNIPPET START ////////////////////////////
			// Create a client instance
			Uri accountUri = new Uri("https://my-cloudant-account.cloudant.com");
			CloudantClient client = new CloudantClientBuilder (accountUri) {
				username = "my-username",
				password = "my-password"
			}.GetResult ();
			////////////////////  SNIPPET END   ///////////////////////////


			/********************************************************
			******** Correct with valid values so test work. 
			*********************************************************/
			accountUri = new Uri("https://"+TestConstants.account);
			client = new CloudantClientBuilder (accountUri) {
				username = TestConstants.username,
				password = TestConstants.password
			}.GetResult ();
			/********************************************************/


			Assert.NotNull (client, "Test failed because client object null.");


			////////////////////  SNIPPET START ////////////////////////////
			// Create Database
			Database db = client.database ("my-database", true).Result;
			client.deleteDB (db).Wait();
			db = client.database ("my-database", true).Result;
			////////////////////  SNIPPET END   ///////////////////////////


			Assert.NotNull (db, "Test failed because db object was null.");


			////////////////////  SNIPPET START ////////////////////////////
			//Data to be saved
			Dictionary<string, object> person = new Dictionary<string, object> ();
			person.Add ("name", "Mike");
			person.Add ("age", 32);
			person.Add ("married", false);
			////////////////////  SNIPPET END   ///////////////////////////




			////////////////////  SNIPPET START ////////////////////////////
			// Save the data
			DocumentRevision personDoc = new DocumentRevision (){
				docId = "person",
				body = person};
			personDoc = db.save (personDoc).Result;
			////////////////////  SNIPPET END   ///////////////////////////


			Assert.NotNull (personDoc, "Test failed because personDoc is null.");
			Assert.NotNull (personDoc.body, "Test failed because personDoc.body is null.");
			Assert.AreEqual ("Mike", personDoc.body ["name"], "Test failed because docRevision body had unexpected data.");


			////////////////////  SNIPPET START ////////////////////////////
			//Update the data
			personDoc.body ["married"] = true;
			personDoc = db.update (personDoc).Result;
			////////////////////  SNIPPET END   ///////////////////////////


			Assert.AreEqual(true, personDoc.body["married"], "Test failed because docRevision body had unexpected data in the 'married' field.");


			////////////////////  SNIPPET START ////////////////////////////
			//Retrieve the data
			DocumentRevision retrievedDoc = db.find("person").Result;

			Dictionary<string,object> retrievedPerson = retrievedDoc.body;

			Console.WriteLine("Name: "+retrievedPerson["name"]);
			Console.WriteLine("Age: "+retrievedPerson["age"]);
			Console.WriteLine("Married: "+retrievedPerson["married"]);
			////////////////////  SNIPPET END   ///////////////////////////


			Assert.AreEqual (32, retrievedPerson ["age"], "Test failed because the retrieved document had unexpected data in the 'age' field.");


			////////////////////  SNIPPET START ////////////////////////////
			// Create index
			db.createIndex ("index_married", 
				"index_married_design", "json",
				new IndexField[]{ new IndexField ("married") }
			).Wait();
			////////////////////  SNIPPET END   ///////////////////////////



			////////////////////  SNIPPET START ////////////////////////////
			// Create a selector with your search criteria.
			string selectorJSON = "\"selector\": {\"married\": {\"$eq\":true} }";
			////////////////////  SNIPPET END   ///////////////////////////



			////////////////////  SNIPPET START ////////////////////////////
			// Find documents that match the search criteria.
			Task <List<DocumentRevision>> findTask = db.findByIndex(selectorJSON,
				new FindByIndexOptions()
				.sort(new IndexField("married", IndexField.SortOrder.desc))
			);
			findTask.Wait ();

			List<DocumentRevision> searchResult = findTask.Result;

			//Display the result
			Console.WriteLine ("Number of records found where married==true : " + searchResult.Count);
			foreach(DocumentRevision d in searchResult)
				Console.WriteLine (string.Format("Name: {0}  Age: {1}", d.body ["name"], d.body["age"]));
			////////////////////  SNIPPET END   ///////////////////////////


			Assert.AreEqual (1, searchResult.Count, "Test failed because findByIndex returned an unexpected number of documents.");

			//Cleanup
			client.deleteDB (db);

		}
	}
}

