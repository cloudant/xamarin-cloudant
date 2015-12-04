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

using IBM.Cloudant.Client;

namespace Test.Shared
{

    [TestFixture ()]
    public class CloudantClientTests
    {

        private CloudantClient client;

        [TestFixtureSetUp]
        public void FixtureSetup ()
        {
            client = new CloudantClientBuilder (TestConstants.account) {
                username = TestConstants.username,
                password = TestConstants.password
            }.GetResult ();
        }


        [Test ()]
        public void TestGreenPath ()
        {
            //Test CloudantClient creation with valid parms.
            CloudantClient testClient = null;
            Assert.DoesNotThrow (() => 
                testClient = new CloudantClientBuilder (TestConstants.account) {
                username = TestConstants.username,
                password = TestConstants.password
            }.GetResult (),
                "Test failed while instantiationg a CloudantClient using valid parameters.");
            Assert.IsNotNull (testClient, "Test failed because client object must not be null after it has been built with valid parms.");

        }

        [Test ()]
        public void TestClientCreationValidAccount ()
        {
            //Test CloudantClient creation with valid accountUri
            CloudantClient testClient = null;
            Uri validUri = new Uri (string.Format ("https://{0}", TestConstants.account));
            Assert.DoesNotThrow (() => 
                testClient = new CloudantClientBuilder (validUri).GetResult (),
                "Test failed while instantiationg a CloudantClient using a valid uri.");
            Assert.IsNotNull (testClient, "Test failed because client object must not be null after it has been built with a valid uri.");

        }

        [Test ()]
        public void TestClienturlEncodesUsername ()
        {
            //Test loginUsername is url encoded and accepts any characters.
            string user = "My u$erN@m3 !#:/%";
            Assert.DoesNotThrow (() => 
                new CloudantClientBuilder (TestConstants.account) {
                username = user,
                password = TestConstants.password
            }.GetResult (),
                "Test falied creating a client with loginUsername: " + user);

        }

        [Test ()]
        public void TestClienturlEncodesPassword ()
        {
            //Test password is url encoded and accepts any characters.
            string pass = "My p@$$w0rd !#:/%";
            Assert.DoesNotThrow (() => 
                new CloudantClientBuilder (TestConstants.account) {
                username = TestConstants.username,
                password = pass
            }.GetResult (),
                "Test falied creating a client with password: " + pass);
        }


        /// <summary>
        /// Test a NoDocumentException is thrown when trying an operation on a DB that doesn't exist.
        /// </summary>
        [Test ()]
        public void NonExistentDatabaseException ()
        {
            string DBName = "database_doesnt_exist";
            var db = client.Database (DBName);
            Assert.Throws<AggregateException> (() => db.ListIndices ().Wait (),
                "Test failed checking that exception is thrown when a database doesn't exist.");
        }


        /// <summary>
        /// Tests for invalid parameters while creating a CloudantClient object.
        /// </summary>
        [Test ()]
        public void CloudantClientBuilderNegativeTests ()
        {
            //Test Account must not be empty string.
            Assert.Throws<ArgumentException> (() => new CloudantClientBuilder ("").GetResult (),
                "Test failed because an empty account parameter didn't report an error.");

        }

        [Test ()]
        public void BuilderAccountWithInvalidHost ()
        {
            //Test Account must be a valid host.
            Assert.Throws<ArgumentException> (() => new CloudantClientBuilder ("invalid!host").GetResult (),
                "Test failed because invalid account parameter 'invalid!host' didn't report an error.");
        }

        [Test ()]
        public void BuilderAccountNameWithSpaces ()
        {

            Assert.Throws<ArgumentException> (() => new CloudantClientBuilder ("host with spaces").GetResult (),
                "Test failed because invalid account parameter 'host with spaces' didn't report an error.");
        }

        [Test ()]
        public void BuilderNullServerURI ()
        {
            //Test accountUri must not be null.
            Uri accountUri = null;
            Assert.Throws<ArgumentException> (() => new CloudantClientBuilder (accountUri).GetResult (),
                "Test failed because accountUri parameter must not be null.");
        }

        [Test ()]
        public void BuilderWithRelativeURI ()
        {

            //Test account Uri must be an absolute Uri
            var accountUri = new Uri ("/account/path", UriKind.Relative);
            Assert.Throws<ArgumentException> (() => new CloudantClientBuilder (accountUri).GetResult (),
                "Test failed because accountUri parameter must not be a relative Uri.");
        }

        [Test ()]
        public void BuilderWithUsernameNullPassword ()
        {
            //Test when loginUsername is entered, password must not be null.
            Assert.Throws<ArgumentException> (() => 
                new CloudantClientBuilder (TestConstants.account) {
                username = TestConstants.username
            }.GetResult (),
                "Test failed because a password wasn't specified, but loginUsername was set.");
        }

        [Test ()]
        public void BuilderWithPasswordNullUsername ()
        {

            //Test when password is entered, loginUsername must not be null.
            Assert.Throws<ArgumentException> (() =>
                new CloudantClientBuilder (TestConstants.account) {
                password = TestConstants.password
            }.GetResult (),
                "Test failed because loginUsername wasn't specified, but password was set.");

        }


        [Test ()]
        public void BuilderWithAccountNameWithDots ()
        {
            Assert.Throws<ArgumentException> (() => {
                new CloudantClientBuilder ("my.account").GetResult ();
            });
        }

        [Test ()]
        public void BuilderWithAccountNameWithDNSName ()
        {
            Assert.Throws<ArgumentException> (() => {
                new CloudantClientBuilder ("my.account.com").GetResult ();
            });
        }

        [Test ()]
        public void BuilderStripsUserNameAndPassword ()
        {
            var builder = new CloudantClientBuilder (new Uri ("https://username:password@username.cloudant.com"));
            Assert.AreEqual ("username", builder.username);
            Assert.AreEqual ("password", builder.password);
            Assert.AreEqual (new Uri ("https://username.cloudant.com"), builder.accountUri);
        }

        [Test ()]
        public void BuilderPassesThroughURIWithNoCreds ()
        {
            var builder = new CloudantClientBuilder (new Uri ("https://username.cloudant.com"));
            Assert.AreEqual (new Uri ("https://username.cloudant.com"), builder.accountUri);
        }
    }

}

