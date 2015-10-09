﻿//
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

using Com.Cloudant.Client.Internal.Http;

namespace Test.Shared
{
	[TestFixture]
	public class HttpHelperTests
	{
		private String DBName;
		private Uri uri;
		private HttpHelper helper = new HttpHelper();

		[SetUp]
		public void setup(){
			DBName = "httphelpertests" + DateTime.Now.Ticks;
			uri = new Uri(TestConstants.validTestUrl () + DBName);
		}


		/// <summary>
		/// Tests the http helper.
		/// PUT, POST, GET, DELETE
		/// </summary>
		[Test]
		public void testHttpHelper(){
			//Test PUT
			Task<HttpResponseMessage> putResponse = helper.sendPut (uri, null, null);
			putResponse.Wait ();
			Assert.AreEqual (HttpStatusCode.Created, putResponse.Result.StatusCode);
		

			//Test POST
			Dictionary<string,object> content = new Dictionary<string,object>(); 
			content.Add ("stringKey", "nicestringvalue");
			content.Add ("numberKey", 42);

			Task<HttpResponseMessage> postResponse = helper.sendPost (uri, null, content);
			postResponse.Wait ();

			Debug.WriteLine ("POST response: " + postResponse.Result.StatusCode);
			Assert.AreEqual (HttpStatusCode.Created, postResponse.Result.StatusCode);

			Task<string> stringContent = postResponse.Result.Content.ReadAsStringAsync ();
			stringContent.Wait();


			//Test GET
			Task<HttpResponseMessage> getResponse = helper.sendGet(uri, null);
			getResponse.Wait ();
			Debug.WriteLine ("GET response: " + getResponse.Result.StatusCode);
			Assert.AreEqual(HttpStatusCode.OK, getResponse.Result.StatusCode);


			//Test DELETE
			Task<HttpResponseMessage> r = helper.sendDelete (uri, null);
			r.Wait ();
			Assert.AreEqual (HttpStatusCode.OK, r.Result.StatusCode);
		}

	}
}

