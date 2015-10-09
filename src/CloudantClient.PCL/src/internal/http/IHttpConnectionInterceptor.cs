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

namespace Com.Cloudant.Client.Internal.Http
{
	/// <summary>
	/// This interface is not intended to be implemented by consumers. You must implement one of the sub-interfaces:
	/// 
	/// <list type="bullet">
	/// 	<item><term><see cref="Com.Cloudant.Client.Internal.Http.IHttpConnectionRequestInterceptor"/> or </term></item>
	/// 	<item><term><see cref="Com.Cloudant.Client.Internal.Http.IHttpConnectionResponseInterceptor"/></term></item>
	/// </list>
	/// </summary>
	public interface IHttpConnectionInterceptor
	{
		
	}
}

