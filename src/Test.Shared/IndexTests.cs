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
using System.Diagnostics;
using NUnit.Framework;
using IBM.Cloudant.Client;

namespace Test.Shared
{
	[TestFixture]
	public class IndexTests
	{
		private static Database db;
		private static CloudantClient client;

		[SetUp]
		public void setup()
		{
			client = new CloudantClientBuilder(TestConstants.account)
			{
				username = TestConstants.username,
				password = TestConstants.password
			}.GetResult();

			string DBName = "httphelpertests" + DateTime.Now.Ticks;
            db = client.Database (DBName);
            db.EnsureExistsAsync ().Wait ();
		}

		[TearDown]
		public void tearDown()
		{
            db.DeleteAsync ().Wait ();
		}



		[Test] 
		public void createJsonIndexAllFields()
		{
			var sortItem = new SortField();
			sortItem.name = "foo";
			sortItem.sort = Sort.asc;
			var fields = new List<SortField>()
			{
				sortItem
			};

            var task = db.CreateJsonIndexAsync (fields: fields, indexName: "myindex", designDocumentName: "myddoc");
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test] 
		public void createJsonIndexOnlyRequired()
		{
			var sortItem = new SortField();
			sortItem.name = "foo";
			sortItem.sort = Sort.asc;
			var fields = new List<SortField>()
			{
				sortItem
			};

            var task = db.CreateJsonIndexAsync (fields: fields);
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test] 
		public void createJsonIndexNamed()
		{
			var sortItem = new SortField();
			sortItem.name = "foo";
			sortItem.sort = Sort.asc;
			var fields = new List<SortField>()
			{
				sortItem
			};

            var task = db.CreateJsonIndexAsync (fields: fields, indexName: "myindex");
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test] 
		public void createJsonIndexNamedDdoc()
		{
			var sortItem = new SortField();
			sortItem.name = "foo";
			sortItem.sort = Sort.asc;
			var fields = new List<SortField>()
			{
				sortItem
			};

            var task = db.CreateJsonIndexAsync (fields: fields, designDocumentName: "myddoc");
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test]
		public void createTextIndexWithAllValues()
		{
			IList<TextIndexField> fields = new List<TextIndexField>()
			{
				new TextIndexField()
				{
					name = "foo",
					type = TextIndexFieldType.String
				}
			};

			var selector = new Dictionary<string,object>()
			{
              ["foo" ] = "bar"  
			};

            var task = db.CreateTextIndexAsync (fields: fields,
				           indexName: "myindex",
				           designDocumentName: "myddoc",
				           selector: selector,
				           defaultFieldEnabled: true,
				           defaultFieldAnalyzer: "english");
			task.Wait();
			Assert.IsFalse(task.IsFaulted);
		}

		[Test]
		public void createTextIndexOnlyDefaultField()
		{
            var task = db.CreateTextIndexAsync (defaultFieldEnabled: true);
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test]
		public void createTextIndexOnlyFields()
		{
			IList<TextIndexField> fields = new List<TextIndexField>()
			{
				new TextIndexField()
				{
					name = "foo",
					type = TextIndexFieldType.String
				}
			};

            var task = db.CreateTextIndexAsync (fields: fields);
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test]
		public void createTextIndexOnlyNameDefaultField()
		{
            var task = db.CreateTextIndexAsync (indexName: "defaultFieldIndex", defaultFieldEnabled: true);
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test]
		public void createTextIndexOnlyDDocNameDefaultField()
		{
            var task = db.CreateTextIndexAsync (designDocumentName: "myddoc", defaultFieldEnabled: true);
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}

		[Test]
		public void createTextIndexOnlySelectorDefaultField()
		{
            var task = db.CreateTextIndexAsync (defaultFieldEnabled: true, selector: new Dictionary<string,object> () {
                ["foo" ] = "bar"  
				});
			task.Wait();
			Assert.IsFalse(task.IsFaulted);
		}

		[Test]
		public void createTexIndexOnlyDefaultFieldAnalyzer()
		{
            var task = db.CreateTextIndexAsync (defaultFieldAnalyzer: "english", defaultFieldEnabled: true);
            task.Wait ();
            Assert.IsFalse (task.IsFaulted);
		}
            

	}
}

