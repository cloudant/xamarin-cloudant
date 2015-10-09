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
using System.Threading.Tasks;
using Com.Cloudant.Client;
using Com.Cloudant.Client.Model;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Test.Shared
{
	[TestFixture]
	public class CRUDTests
	{
		private CloudantClient client;
		private Database db;
		private String DBName;

		protected static String ageKey = "age";
		protected static String ageIndex = "ageIndex";
		protected static String designDocName = "designDocName";
		protected static List<IndexField> ageIndexFields = new List<IndexField> ();
		protected static int ageBaseValue = 0;
		protected static String nameKey = "name";
		protected static String nameValue = "data";


		[SetUp]
		public void Setup ()
		{
			DBName = TestConstants.defaultDatabaseName + DateTime.Now.Ticks;

			client = new CloudantClientBuilder (TestConstants.account) {
				username = TestConstants.username,
				password = TestConstants.password
			}.GetResult ();

			// create the database
			try{
				Task<Database> dbTask = client.database(DBName,true);
				dbTask.Wait();
				db = dbTask.Result;
				Assert.NotNull(db);
			} catch(AggregateException ae){
				Assert.Fail ("Create remote database failed.  Cause: " + ae.Message );
			}
			catch (Exception e){
				Assert.Fail ("Unexpected failure: " + e.Message);
			}
		}

		[TearDown] //Runs after each test.
		protected void tearDown() {
			if (db != null) {
				Task deleteTask = client.deleteDB (db);
				deleteTask.Wait ();

				if (deleteTask.IsFaulted)
					Debug.WriteLine ("Failed to delete remote DB name: " + DBName + "\nError: " + deleteTask.Exception.Message);
			}
		}

		[Test]
		public void testSaveFetchDeleteDocument() {

			String stringKey = "stringKey";
			String stringValue = "nicestringvalue";

			String numberKey = "numberKey";
			int numberValue = 42;

			String newKey = "newKey";
			int newValue = 43;

			Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
			dictionary.Add(stringKey, stringValue);
			dictionary.Add(numberKey, numberValue);

			// Insert a document
			DocumentRevision revision = new DocumentRevision();
			revision.body = dictionary;

			Task<DocumentRevision> task = db.save(revision);
			task.Wait ();

			// Validate save result
			Assert.False(task.IsFaulted,"create unexpectedly failed");
			Assert.True(task.Result is DocumentRevision,"DocumentRevision not returned on create");
			DocumentRevision savedRevision = (DocumentRevision) task.Result;
			Assert.NotNull(savedRevision.docId, "savedRevision.docId == null");
			Assert.NotNull(savedRevision.revId, "savedRevision.revId == null");

			// perform find
			task = db.find(savedRevision.docId);
			task.Wait();

			// Validate find result
			Assert.False(task.IsFaulted,"create unexpectedly failed");
			Assert.True(task.Result is DocumentRevision,"DocumentRevision not returned on create");
			DocumentRevision fetchedRevision = (DocumentRevision) task.Result;

			fetchedRevision.body.Add(newKey, newValue);
			fetchedRevision.body.Remove(stringKey);

			// perform update
			task = db.update(fetchedRevision);
			task.Wait();

			Assert.False(task.IsFaulted,"update unexpectedly failed");
			Assert.True(task.Result is DocumentRevision,"DocumentRevision not returned on create");
			DocumentRevision updatedRevision = (DocumentRevision) task.Result;
			Assert.NotNull(updatedRevision.docId,"updatedRevision.docId == null");
			Assert.NotNull(updatedRevision.revId,"updatedRevision.revId == null");
			Assert.True(updatedRevision.body.ContainsKey(newKey),"updatedBody did not contain newKey as expected");
			Assert.False(updatedRevision.body.ContainsKey(stringKey),"updatedBody did contained stringKey when not expected");

			// remove the document from the database
			Task<String> deleteTask = db.remove(updatedRevision);
			deleteTask.Wait();
			Assert.False(deleteTask.IsFaulted,"delete unexpectedly failed");
			Assert.NotNull(deleteTask.Result,"delete result not returned as expected.");


		}

		[Test]
		public void testBasicSaveWithoutBody() {
			// Save DocumentRevision without a body
			DocumentRevision revision = new DocumentRevision();
			Task<DocumentRevision> saveTask = db.save(revision);
			saveTask.Wait();
			Assert.True(!saveTask.IsFaulted,"failed to save DocumentRevision with empty body");
		}

		[Test]
		public void testBasicSaveWithBody() {
			DocumentRevision revision = new DocumentRevision();
			Dictionary<String, Object> body = new Dictionary<String, Object>();
			body.Add("key", "value");
			revision.body = body;

			// Save DocumentRevision with a body
			Task<DocumentRevision> saveTask = db.save(revision);
			saveTask.Wait();
			Assert.True(!saveTask.IsFaulted,"failed to save DocumentRevision with body");
		}

		[Test]
		public void testInvalidSave() {
			try{
				// Save null
				Task<DocumentRevision> saveTask = db.save(null);
				saveTask.Wait();
				Assert.False(true,"save should raise exception on save of null");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidSave exception caught.  Cause:" + e.Message);
			}
		}

		[Test]
		public void testInvalidFetchWithNullInput() {
			try{
				// fetch null
				Task<DocumentRevision> fetchByIdTask = db.find(null);
				fetchByIdTask.Wait();
				Assert.True(fetchByIdTask.IsFaulted,"find should produce fault on fetch of null");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidFetchWithNullInput exception caught.  Cause:" + e.Message);
			}
		}

		[Test]
		public void testInvalidFetchWithEmptyString() {
			try{
				// fetch empty string
				Task<DocumentRevision> fetchByIdTask = db.find("");
				fetchByIdTask.Wait();
				Assert.True(fetchByIdTask.IsFaulted,"find should produce fault on find of empty string");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidFetchWithEmptyString exception caught.  Cause:" + e.Message);
			}
		}

		[Test]
		public void testInvalidFetchNonexistId() {
			try{
				// fetch id that doesn't exist
				Task<DocumentRevision> fetchByIdTask = db.find("1234");
				fetchByIdTask.Wait();
				Assert.True(fetchByIdTask.IsFaulted,"find should produce fault on find of empty string");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidFetchNonexistId exception caught.  Cause:" + e.InnerException.Message);
			}
		}

		[Test]
		public void testInvalidDeleteWithNullInput() {
			try{
				// delete null
				Task<String> deleteTask = db.remove(null);
				deleteTask.Wait();
				Assert.True(deleteTask.IsFaulted,"remove should produce fault on remove of null");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidDeleteWithNullInput exception caught.  Cause:" + e.Message);
			}

		}

		[Test]
		public void testInvalidDeleteWithoutBody() {
			try{
				// Save DocumentRevision without a body
				DocumentRevision revision = new DocumentRevision();
				Task <String> deleteTask = db.remove(revision);
				deleteTask.Wait();
				Assert.True(deleteTask.IsFaulted,"delete DocumentRevision that does not exist should fail");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidDeleteWithoutBody exception caught.  Cause:" + e.InnerException.Message);
			}
		}

		[Test]
		public void testEqualityQueryTest() {
			doQuerySetup();

			String selectorJSON = "\"selector\": {\"" + ageKey + "\": {\"$eq\":5} }";
			Task <List<DocumentRevision>> task = db.findByIndex(selectorJSON,
				new FindByIndexOptions()
				.sort(new IndexField(ageKey, IndexField.SortOrder.desc))
				);
			
			task.Wait ();
			List<DocumentRevision> result = task.Result;

			Assert.False(task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull(task.Result, "Query result was null");
			Assert.True(result.Count == 1);

			DocumentRevision revision = result.Find (item => item.body != null);
			Object age;
			revision.body.TryGetValue(ageKey, out age);
			Assert.True((long)age==5);
		}

		[Test]
		public void testGreaterThanQueryWithSortOptionTest() {
			doQuerySetup();

			String selectorJSON = "\"selector\": {\"" + ageKey + "\": {\"$gt\":1} }";
			Task <List<DocumentRevision>> task = db.findByIndex(selectorJSON,
				new FindByIndexOptions()
				.sort(new IndexField(ageKey, IndexField.SortOrder.desc))
			);

			task.Wait ();
			List<DocumentRevision> result = task.Result;

			Assert.False(task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull(task.Result, "Query result was null");
			Assert.True(result.Count == 18);

			DocumentRevision revision = result[0];
			Object age;
			revision.body.TryGetValue(ageKey, out age);
			Assert.True((long)age==19);
		}

		[Test]
		public void testLimitQueryTest() {
			doQuerySetup ();

			String selectorJSON = "\"selector\": {\"" + ageKey + "\": {\"$gt\":10} }";
			Task <List<DocumentRevision>> task = db.findByIndex (selectorJSON,
				new FindByIndexOptions ()
				.limit (5)
			);

			task.Wait ();
			List<DocumentRevision> result = task.Result;

			Assert.False (task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull (task.Result, "Query result was null");
			Assert.True (result.Count == 5);

			foreach (DocumentRevision revision in result) {
				Object age;
				revision.body.TryGetValue (ageKey, out age);
				Assert.True ((long)age > 10);
			}
		}
				
		[Test]
		public void testSkipQueryTest() {
			doQuerySetup ();

			String selectorJSON = "\"selector\": {\"" + ageKey + "\": {\"$gt\":10} }";
			Task <List<DocumentRevision>> task = db.findByIndex (selectorJSON,
				new FindByIndexOptions ()
				.skip(5)
			);

			task.Wait ();
			List<DocumentRevision> result = task.Result;

			Assert.False (task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull (task.Result, "Query result was null");
			Assert.True (result.Count == 4);

			foreach (DocumentRevision revision in result) {
				Object age;
				revision.body.TryGetValue (ageKey, out age);
				Assert.True ((long)age > 10);
			}
		}

		[Test]
		public void testInvalidSelectorQueryTest() {
			try{
				doQuerySetup ();

				String selectorJSON = "\"invalidKeyword\": {\"" + ageKey + "\": {\"$gt\":10} }";
				Task <List<DocumentRevision>> task = db.findByIndex (selectorJSON,
					new FindByIndexOptions ()
					.skip(5)
				);

				task.Wait ();
				Assert.True(task.IsFaulted,"findByIndex() with invalid selector JSON should fail");
			}
			catch (Exception e){
				Assert.Pass ("expected testInvalidSelectorQueryTest exception caught.  Cause:" + e.InnerException.Message);
			}

		}

		// Private helpers
		private void doQuerySetupWithFields(String indexName, List<IndexField> indexFields) {

			Task indexTask = db.createIndex (indexName, designDocName, "json", indexFields.ToArray());
			indexTask.Wait ();

			for (int i = 0; i < 20; i++) {
				Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
				dictionary.Add(nameKey, nameValue + i);
				dictionary.Add(ageKey, ageBaseValue + i);

				DocumentRevision revision = new DocumentRevision();
				revision.body = dictionary;
				Task<DocumentRevision> task = db.save(revision);
				task.Wait ();
			}				
		}

		private void doQuerySetup() {
			ageIndexFields.Add (new IndexField (ageKey));
			doQuerySetupWithFields(ageIndex, ageIndexFields);
		}

	}
}

