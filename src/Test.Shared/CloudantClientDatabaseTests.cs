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
        public void FixtureSetUp()
        {
            client = new CloudantClientBuilder(TestConstants.account)
            {
                username = TestConstants.username,
                password = TestConstants.password
            }.GetResult();
        }

        [SetUp] //Runs before each test.
        public void Setup()
        {
            DBName = TestConstants.defaultDatabaseName + DateTime.Now.Ticks;
        }

        [TearDown] //Runs after each test.
        protected void tearDown()
        {
            if (db != null)
            {
                db.DeleteAsync().Wait();
            }
        }

        [Test]
        public void testEnsureExistsDoesntErrorWhenCalledTwice()
        {
            db = client.Database(DBName);
            Assert.DoesNotThrow(async () =>
                {
                    await db.EnsureExistsAsync().ConfigureAwait(continueOnCapturedContext: false);
                    await db.EnsureExistsAsync().ConfigureAwait(continueOnCapturedContext: false);
                });
        }

        [Test]
        public void testDBCreationGreenPath()
        {
            
            db = client.Database(DBName);
            db.EnsureExistsAsync().Wait();
            Assert.NotNull(db);

            //Test db names are url encoded
            string databaseName = "az09_$()+-/";
            Assert.DoesNotThrow(async () =>
                {
                    var newDb = client.Database(databaseName);
                    await newDb.EnsureExistsAsync().ConfigureAwait(continueOnCapturedContext: false);
                    //Clean up
                    await newDb.DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);
                },
                "Test failed to create a database with name " + databaseName);
        }



        [Test]
        public void TestDBCreationFailsWithNullName()
        {
            Assert.Throws<ArgumentException>(async () => await client.Database(null)
                .EnsureExistsAsync()
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        [Test]
        public void TestDBCreationFailsWithEmptyName()
        {
            Assert.Throws<ArgumentException>(async () => await client.Database("")
                .EnsureExistsAsync()
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        [Test]
        public void TestDBCreationFailsWithSpacesInName()
        {
            Assert.Throws<ArgumentException>(async () => await client.Database("name with spaces")
                .EnsureExistsAsync()
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        [Test]
        public void TestDBCreationFailsnameStartingWithCaptial()
        {
            Assert.Throws<ArgumentException>(async () => await client.Database("Invalid")
                .EnsureExistsAsync()
                .ConfigureAwait(continueOnCapturedContext: false));
        }

        [Test]
        public void testDBCreationFailsWithAtSymbol()
        {
            Assert.Throws<ArgumentException>(async () => await client.Database("invalid@")
                .EnsureExistsAsync()
                .ConfigureAwait(continueOnCapturedContext: false));
        }

            
        [Test]
        public void testDBCreationFailsWithExclamation()
        {
            Assert.Throws<ArgumentException>(async () => await client.Database("invalid!")
                .EnsureExistsAsync()
                .ConfigureAwait(continueOnCapturedContext: false));
        }
    }
}

