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
using System.Net.Http;
using System.Threading.Tasks;

namespace Com.Cloudant.Client.Internal.Http
{
	public interface IHttpHelper {

		Task<HttpResponseMessage> sendGet(Uri uri, Dictionary<String,String> headers);

		Task<HttpResponseMessage> sendDelete (Uri uri, Dictionary<String, String> headers);
	
		Task<HttpResponseMessage> sendPut (Uri uri, Dictionary<String, String> headers, Dictionary<String, Object> body);

		Task<HttpResponseMessage> sendPost (Uri uri, Dictionary<String, String> headers, Dictionary<String, Object> body);

		void addGlobalHeaders (string name, string value);

	}
}

