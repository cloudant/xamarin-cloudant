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

namespace IBM.Cloudant.Client
{
    /// <summary>
    /// This interface is not intended to be implemented by consumers. You must implement one of the sub-interfaces:
    /// 
    /// <list type="bullet">
    ///     <item><term><see cref="IBM.Cloudant.Client.IHttpConnectionRequestInterceptor"/> or </term></item>
    ///     <item><term><see cref="IBM.Cloudant.Client.IHttpConnectionResponseInterceptor"/></term></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// This is the base interface for HTTP connection interceptors.
    /// </remarks>
    public interface IHttpConnectionInterceptor
    {
        
    }
}

