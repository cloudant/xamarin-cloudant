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
        public void setup(){
            client = new CloudantClientBuilder (TestConstants.account) {
                username = TestConstants.username,
                password = TestConstants.password
            }.GetResult ();

            string DBName = "httphelpertests" + DateTime.Now.Ticks;
            db = client.Database(DBName);
            db.EnsureExists ();
        }

        [TearDown]
        public void tearDown(){
            db.Delete ().Wait ();
        }

        [Test]
        public void indexTestAll() {
            string indexName = "indexName";
            string designDocName = "designDocName";
            string fieldName = "fieldName";

            Task indexTask = db.CreateIndex(indexName, designDocName, null,
                new IndexField[]{new IndexField(fieldName, IndexField.SortOrder.asc)});
            indexTask.Wait ();

            Task<List<Index>> indices = db.ListIndices ();
            indices.Wait ();

            Assert.True (indices.Result.Count == 2, "Should have 2 indices. Found: "+indices.Result.Count);
            Assert.True (indices.Result [1].name == indexName);
            Assert.True (indices.Result [1].indexFields.Count == 1, "Should have 1 IndexField. Found: " + indices.Result [1].indexFields.Count);

            Task deleteIndexTask = db.DeleteIndex (indexName, designDocName);
            deleteIndexTask.Wait ();

            Task<List<Index>> newIndices = db.ListIndices ();
            newIndices.Wait ();

            Assert.True (newIndices.Result.Count == 1, "Should have 1 index after deletion. Found: "+newIndices.Result.Count);
        }


        [Test]
        public void negativeTests(){
            string indexName = "myIndex";
            string designDocId = "myDesignDoc";
            string indexFieldName = "myIndexFieldName";


            //Index field not null.
            Assert.Throws<DataException> (() => {
                Task indexTask = db.CreateIndex (indexName, designDocId, null, null);
                indexTask.Wait ();
            }, "Index field must not be null.");


            //Index field not empty.
            Assert.Throws<DataException> (() => {
                Task indexTask = db.CreateIndex (indexName, designDocId, null, new IndexField[]{ });
                indexTask.Wait ();
            }, "Index field must not be empty.");
                

            //DeleteIndex - indexName must not be empty.
            Assert.Throws<DataException> (() => {
                Task indexTask = db.DeleteIndex(null, designDocId);
                indexTask.Wait ();
            }, "DeleteIndex - indexName must not be empty.");


            //DeleteIndex - designDocId must not be empty.
            Assert.Throws<DataException> (() => {
                Task indexTask = db.DeleteIndex(indexName, "  ");
                indexTask.Wait ();
            }, "DeleteIndex - designDocId must not be empty.");


            //DeleteIndex - index does not exist.
            Assert.Throws<AggregateException> (() => {
                Task indexTask = db.DeleteIndex("doesntExist", "doesntExist");
                indexTask.Wait ();
            }, "DeleteIndex - index does not exist.");


            //Index name invalid characters.
            Assert.Throws<AggregateException> (() => {
                Task indexTask = db.CreateIndex ("\"", designDocId, null, new IndexField[]{new IndexField (indexFieldName, IndexField.SortOrder.asc) });
                indexTask.Wait ();
            }, "Index name contains invalid characters.");


            //indexType 'text' not supported.
            Assert.Throws<DataException> (() => {
                Task indexTask = db.CreateIndex (indexName, designDocId, "text",
                                     new IndexField[]{ new IndexField (indexFieldName, IndexField.SortOrder.asc) });
                indexTask.Wait ();
            });
        }

    }
}

