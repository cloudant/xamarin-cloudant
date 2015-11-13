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

using IBM.Cloudant.Client;

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
                Task deleteDBTask = db.Delete();
                deleteDBTask.Wait ();

                if (deleteDBTask.IsFaulted)
                    Debug.WriteLine ("Failed to delete remote DB name: " + DBName + "\nError: " + deleteDBTask.Exception.Message);
            }
        }

        [Test]
        public void testBasicAuthInterceptor() {
            BasicAuthenticationInterceptor basicAuthInterceptor = new BasicAuthenticationInterceptor (TestConstants.username, TestConstants.password);

            client = new CloudantClientBuilder (TestConstants.account) {
                interceptors = new List<IHttpConnectionInterceptor>(){basicAuthInterceptor}
            }.GetResult ();

            db = client.Database (DBName);

            Assert.DoesNotThrow( () => {
                db.EnsureExists ();},
                "Exception thrown while creating database using BasicAuth interceptor. ");

            Assert.NotNull(db);

        }

        [Test]
        public void testCookieInterceptor() {
            CookieInterceptor cookieInterceptor = new CookieInterceptor (TestConstants.username, TestConstants.password);

            client = new CloudantClientBuilder (TestConstants.account) {
                interceptors = new List<IHttpConnectionInterceptor>(){cookieInterceptor}
            }.GetResult ();

            db = client.Database (DBName);

            Assert.DoesNotThrow( () => {
                db.EnsureExists ();
                },
                "Exception thrown while creating database using cookie interceptor. ");

            Assert.NotNull(db);
        }

        [Test]
        public void negativeTests(){
            Assert.Throws<DataException> (() => {
                new CloudantClientBuilder (TestConstants.account) {
                    interceptors = new List<IHttpConnectionInterceptor> (){ new BadInterceptor () }
                }.GetResult ();
            });
        }

    }

    internal class BadInterceptor : IHttpConnectionInterceptor {
        public BadInterceptor(){}
    }
}

