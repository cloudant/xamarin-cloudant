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
using IBM.Cloudant.Client;
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
		protected static List<SortField> ageIndexFields = new List<SortField>();
		protected static int ageBaseValue = 0;
		protected static String nameKey = "name";
		protected static String nameValue = "data";


		[SetUp]
		public void Setup()
		{
			DBName = TestConstants.defaultDatabaseName + DateTime.Now.Ticks;

			client = new CloudantClientBuilder(TestConstants.account)
			{
				username = TestConstants.username,
				password = TestConstants.password
			}.GetResult();

			// create the database
			try
			{
				db = client.Database(DBName);
				db.EnsureExistsAsync().Wait();
				Assert.NotNull(db);
			}
			catch (AggregateException ae)
			{
				Assert.Fail("Create remote database failed.  Cause: " + ae.Message);
			}
			catch (Exception e)
			{
				Assert.Fail("Unexpected failure: " + e.Message);
			}
		}

		[TearDown] //Runs after each test.
        protected void tearDown()
		{
			if (db != null)
			{
				Task deleteTask = db.DeleteAsync();
				deleteTask.Wait();

				if (deleteTask.IsFaulted)
					Debug.WriteLine("Failed to delete remote DB name: " + DBName + "\nError: " + deleteTask.Exception.Message);
			}
		}

		[Test]
		public void testSaveFetchDeleteDocument()
		{

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

			Task<DocumentRevision> task = db.CreateAsync(revision);
			task.Wait();

			// Validate save result
			Assert.False(task.IsFaulted, "create unexpectedly failed");
			Assert.True(task.Result is DocumentRevision, "DocumentRevision not returned on create");
			DocumentRevision savedRevision = (DocumentRevision)task.Result;
			Assert.NotNull(savedRevision.docId, "savedRevision.docId == null");
			Assert.NotNull(savedRevision.revId, "savedRevision.revId == null");

			// perform find
			task = db.ReadAsync(savedRevision.docId);
			task.Wait();

			// Validate find result
			Assert.False(task.IsFaulted, "create unexpectedly failed");
			Assert.True(task.Result is DocumentRevision, "DocumentRevision not returned on create");
			DocumentRevision fetchedRevision = (DocumentRevision)task.Result;

			fetchedRevision.body.Add(newKey, newValue);
			fetchedRevision.body.Remove(stringKey);

			// perform update
			task = db.UpdateAsync(fetchedRevision);
			task.Wait();

			Assert.False(task.IsFaulted, "update unexpectedly failed");
			Assert.True(task.Result is DocumentRevision, "DocumentRevision not returned on create");
			DocumentRevision updatedRevision = (DocumentRevision)task.Result;
			Assert.NotNull(updatedRevision.docId, "updatedRevision.docId == null");
			Assert.NotNull(updatedRevision.revId, "updatedRevision.revId == null");
			Assert.True(updatedRevision.body.ContainsKey(newKey), "updatedBody did not contain newKey as expected");
			Assert.False(updatedRevision.body.ContainsKey(stringKey), "updatedBody did contained stringKey when not expected");

			// remove the document from the database
			Task<String> deleteTask = db.DeleteAsync(updatedRevision);
			deleteTask.Wait();
			Assert.False(deleteTask.IsFaulted, "delete unexpectedly failed");
			Assert.NotNull(deleteTask.Result, "delete result not returned as expected.");

		}

		[Test]
		public void testBasicSaveWithoutBody()
		{
			// Save DocumentRevision without a body
			DocumentRevision revision = new DocumentRevision();
			Task<DocumentRevision> saveTask = db.CreateAsync(revision);
			saveTask.Wait();
			Assert.True(!saveTask.IsFaulted, "failed to save DocumentRevision with empty body");
		}

		[Test]
		public void testBasicSaveWithBody()
		{
			DocumentRevision revision = new DocumentRevision();
			Dictionary<String, Object> body = new Dictionary<String, Object>();
			body.Add("key", "value");
			revision.body = body;

			// Save DocumentRevision with a body
			Task<DocumentRevision> saveTask = db.CreateAsync(revision);
			saveTask.Wait();
			Assert.True(!saveTask.IsFaulted, "failed to save DocumentRevision with body");
		}

		[Test]
		public void testInvalidSave()
		{
			try
			{
				// Save null
				Task<DocumentRevision> saveTask = db.CreateAsync(null);
				saveTask.Wait();
				Assert.False(true, "save should raise exception on save of null");
			}
			catch (Exception e)
			{
				Assert.Pass("expected testInvalidSave exception caught.  Cause:" + e.Message);
			}
		}

		[Test]
		public void testInvalidFetchWithNullInput()
		{
			try
			{
				// fetch null
				Task<DocumentRevision> fetchByIdTask = db.ReadAsync(null);
				fetchByIdTask.Wait();
				Assert.True(fetchByIdTask.IsFaulted, "find should produce fault on fetch of null");
			}
			catch (Exception e)
			{
				Assert.Pass("expected testInvalidFetchWithNullInput exception caught.  Cause:" + e.Message);
			}
		}

		[Test]
		public void testInvalidFetchWithEmptyString()
		{
			try
			{
				// fetch empty string
				Task<DocumentRevision> fetchByIdTask = db.ReadAsync("");
				fetchByIdTask.Wait();
				Assert.True(fetchByIdTask.IsFaulted, "find should produce fault on find of empty string");
			}
			catch (Exception e)
			{
				Assert.Pass("expected testInvalidFetchWithEmptyString exception caught.  Cause:" + e.Message);
			}
		}

		[Test]
		public void testInvalidFetchNonexistId()
		{
			// fetch id that doesn't exist
			Assert.Throws<AggregateException>(() =>
				{
					Task<DocumentRevision> fetchByIdTask = db.ReadAsync("1234");
					fetchByIdTask.Wait();

				});   
		}

		[Test]
		public void testInvalidDeleteWithNullInput()
		{
			try
			{
				// delete null
				Task<String> deleteTask = db.DeleteAsync(null);
				deleteTask.Wait();
				Assert.True(deleteTask.IsFaulted, "remove should produce fault on remove of null");
			}
			catch (Exception e)
			{
				Assert.Pass("expected testInvalidDeleteWithNullInput exception caught.  Cause:" + e.Message);
			}

		}

		[Test]
		public void testInvalidDeleteWithoutBody()
		{
			try
			{
				// Save DocumentRevision without a body
				DocumentRevision revision = new DocumentRevision();
				Task <String> deleteTask = db.DeleteAsync(revision);
				deleteTask.Wait();
				Assert.True(deleteTask.IsFaulted, "delete DocumentRevision that does not exist should fail");
			}
			catch (Exception e)
			{
				Assert.Pass("expected testInvalidDeleteWithoutBody exception caught.  Cause:" + e.InnerException.Message);
			}
		}

		[Test]
		public void testEqualityQueryTest()
		{
			doQuerySetup();

			var sortField = new SortField(){ sort = Sort.desc, name = ageKey };

			var task = db.QueryAsync(selector: new Dictionary<string,object>()
				{
                [ageKey ] = 5
				}, sort: new List<SortField>()
				{
					sortField
				});
            
			task.Wait();
			IList<DocumentRevision> result = task.Result;

			Assert.False(task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull(task.Result, "Query result was null");
			Assert.True(result.Count == 1);

			DocumentRevision revision = result[0];
			Object age;
			revision.body.TryGetValue(ageKey, out age);
			Assert.True((long)age == 5);
		}

		[Test]
		public void testGreaterThanQueryWithSortOptionTest()
		{
			doQuerySetup();

			var sortField = new SortField();
			sortField.name = ageKey;
			sortField.sort = Sort.desc;

			var task = db.QueryAsync(selector: new Dictionary<string,object>()
				{
                [ageKey ] = new Dictionary<string,int>()
					{
                    ["$gt" ] = 1
					}
				}, sort: new List<SortField>()
				{
					sortField
				});
                
			task.Wait();
			IList<DocumentRevision> result = task.Result;

			Assert.False(task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull(task.Result, "Query result was null");
			Assert.True(result.Count == 18);

			DocumentRevision revision = result[0];
			Object age;
			revision.body.TryGetValue(ageKey, out age);
			Assert.True((long)age == 19);
		}

		[Test]
		public void testLimitQueryTest()
		{
			doQuerySetup();

			var sortField = new SortField()
			{
				name = ageKey,
				sort = Sort.desc
			};

			var task = db.QueryAsync(selector: new Dictionary<string,object>()
				{
                [ageKey ] = new Dictionary<string,int>()
					{
                    ["$gt" ] = 10
					}
				}, limit: 5);
                
			task.Wait();
			IList<DocumentRevision> result = task.Result;

			Assert.False(task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull(task.Result, "Query result was null");
			Assert.True(result.Count == 5);

			foreach (DocumentRevision revision in result)
			{
				Object age;
				revision.body.TryGetValue(ageKey, out age);
				Assert.True((long)age > 10);
			}
		}

		[Test]
		public void testSkipQueryTest()
		{
			doQuerySetup();

			var sortField = new SortField();
			sortField.name = ageKey;

			var task = db.QueryAsync(selector: new Dictionary<string,object>()
				{
                [ageKey ] = new Dictionary<string,int>()
					{
                    ["$gt" ] = 10
					}
				}, skip: 5);

			task.Wait();
			IList<DocumentRevision> result = task.Result;

			Assert.False(task.IsFaulted, "findByIndex() failed");
			Assert.IsNotNull(task.Result, "Query result was null");
			Assert.True(result.Count == 4);

			foreach (DocumentRevision revision in result)
			{
				Object age;
				revision.body.TryGetValue(ageKey, out age);
				Assert.True((long)age > 10);
			}
		}

		[Test]
		public void testCreateDocumentWithSlash()
		{
			var document = new DocumentRevision()
			{
				docId = "my/document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);

		}

		[Test]
		public void testCreateDocumentInDbWithSlash()
		{
            
			var db = client.Database("my/database");
			try
			{
				db.EnsureExistsAsync().Wait();

				var document = new DocumentRevision()
				{
					docId = "my/document",
					body = new Dictionary<string,Object>()
					{
                    ["hello" ] = "world"
					}
				};

				var savedDocument = CreateAndAssert(document);

				//read
				ReadAndAssert(document, savedDocument);

				// update
				var updated = UpdateAndAssert(document, savedDocument);

				// delete
				DeleteAndAssert(updated);
			}
			finally
			{
				db.DeleteAsync().Wait();
			}

		}


		[Test]
		public void testCreateDocumentWithColon()
		{
			var document = new DocumentRevision()
			{
				docId = "my:document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithQuestionMark()
		{
			var document = new DocumentRevision()
			{
				docId = "my?document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithAt()
		{
			var document = new DocumentRevision()
			{
				docId = "my@document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithHash()
		{
			var document = new DocumentRevision()
			{
				docId = "my#document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithSquareBrackets()
		{
			var document = new DocumentRevision()
			{
				docId = "my[document]",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithDollar()
		{
			var document = new DocumentRevision()
			{
				docId = "my$document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithApostrophe()
		{
			var document = new DocumentRevision()
			{
				docId = "my'document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithAmpersand()
		{
			var document = new DocumentRevision()
			{
				docId = "my&document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithRoundBrackets()
		{
			var document = new DocumentRevision()
			{
				docId = "my(document)",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}


		[Test]
		public void testCreateDocumentWithEquals()
		{
			var document = new DocumentRevision()
			{
				docId = "my=document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithSemiColon()
		{
			var document = new DocumentRevision()
			{
				docId = "my;document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithComma()
		{
			var document = new DocumentRevision()
			{
				docId = "my,document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithPlus()
		{
			var document = new DocumentRevision()
			{
				docId = "my+document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithStar()
		{
			var document = new DocumentRevision()
			{
				docId = "my*document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}

		[Test]
		public void testCreateDocumentWithExclamation()
		{
			var document = new DocumentRevision()
			{
				docId = "my!document",
				body = new Dictionary<string,Object>()
				{
                    ["hello" ] = "world"
				}
			};

			var savedDocument = CreateAndAssert(document);

			//read
			ReadAndAssert(document, savedDocument);

			// update
			var updated = UpdateAndAssert(document, savedDocument);

			// delete
			DeleteAndAssert(updated);
		}



		// Private helpers

		private DocumentRevision CreateAndAssert(DocumentRevision document)
		{
			Task<DocumentRevision> task = db.CreateAsync(document);
			task.Wait();
			Assert.IsFalse(task.IsFaulted);
			Assert.IsNotNull(task.Result);
			var savedDocument = task.Result;

			Assert.AreEqual(document.docId, savedDocument.docId);
			Assert.AreEqual(document.body, savedDocument.body);

			return savedDocument;
		}

		private void ReadAndAssert(DocumentRevision document, DocumentRevision savedDocument)
		{
			var readDocumentTask = db.ReadAsync(document.docId);
			readDocumentTask.Wait();
			Assert.IsFalse(readDocumentTask.IsFaulted);
			Assert.AreEqual(savedDocument, readDocumentTask.Result);
		}

		private DocumentRevision UpdateAndAssert(DocumentRevision document, DocumentRevision savedDocument)
		{
			savedDocument.body.Add("updated", true);
			var updateTask = db.UpdateAsync(savedDocument);
			updateTask.Wait();
			Assert.IsFalse(updateTask.IsFaulted);
			Assert.AreEqual(document.docId, updateTask.Result.docId);
			return updateTask.Result;
		}

		private void DeleteAndAssert(DocumentRevision updated)
		{
			var deleteTask = db.DeleteAsync(updated);
			deleteTask.Wait();
			Assert.IsFalse(deleteTask.IsFaulted);
		}


		private void doQuerySetupWithFields(String indexName, List<SortField> indexFields)
		{

			var indexTask = db.CreateJsonIndexAsync(fields: indexFields, indexName: indexName);
			indexTask.Wait();

			for (int i = 0; i < 20; i++)
			{
				Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
				dictionary.Add(nameKey, nameValue + i);
				dictionary.Add(ageKey, ageBaseValue + i);

				DocumentRevision revision = new DocumentRevision();
				revision.body = dictionary;
				Task<DocumentRevision> task = db.CreateAsync(revision);
				task.Wait();
			}               
		}

		private void doQuerySetup()
		{
			var indexField = new SortField()
			{
				name = ageKey
			};
			ageIndexFields.Add(indexField);
			doQuerySetupWithFields(ageIndex, ageIndexFields);
		}



	}


}

