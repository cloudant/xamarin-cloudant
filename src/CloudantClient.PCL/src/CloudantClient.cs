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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;



namespace IBM.Cloudant.Client
{
    /// <summary>
    /// The main object for the Cloudant client public API.
    /// </summary>
    /// <remarks>
    /// This class is used for creating, deleting, or connecting to a Cloudant database.
    /// </remarks>
    public class CloudantClient
    {
        internal Uri accountUri;
        private String username;
        private String password;

        /// <summary>
        /// HttpHelper object used by this client instance to execute http requests. 
        /// </summary>
        public HttpHelper httpHelper;


        /// <summary>
        /// Constructs a new instance of this class and connects to the cloudant server using a builder class.
        /// </summary>
        /// <param name="builder">Builder class. <see cref="IBM.Cloudant.Client.CloudantClientBuilder"/> </param>
        public CloudantClient(CloudantClientBuilder builder)
        {
            this.accountUri = builder.accountUri;
            this.username = builder.username;
            this.password = builder.password;


            List<IHttpConnectionInterceptor> interceptors = new List<IHttpConnectionInterceptor>();
            if (builder.interceptors != null)
            {
                interceptors.AddRange(builder.interceptors);
            }

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                var cookieInterceptor = new CookieInterceptor(username, password);
                interceptors.Add(cookieInterceptor);
            }

            InitHttpHelper(interceptors);


        }


        /// <summary>
        /// Creates a database object that represents a database on the server. 
        /// However the database may not exist on the server yet. You should
        /// call <see cref="IBM.Cloudant.Client.Database.EnsureExists"/> to
        /// ensure the database exists on the server before performing
        /// reads or writes.
        /// </summary>
        /// <param name="dbname">name of database to access</param>
        /// <returns>A database object that represents a database on the server</returns>
        public Database  Database(String dbname)
        {

            if (string.IsNullOrEmpty(dbname))
            {
                throw new DataException(DataException.Database_DatabaseModificationFailure, 
                    "Database name parameter may not be null or empty.");
            }

            return new Database(this, dbname);
        }
            

        // ======== PRIVATE HELPERS =============

        private void InitHttpHelper(List<IHttpConnectionInterceptor> interceptors)
        {
            if (interceptors != null)
            {
                List<IHttpConnectionRequestInterceptor> requestInterceptors = new List<IHttpConnectionRequestInterceptor>();
                List<IHttpConnectionResponseInterceptor> responseInterceptors = new List<IHttpConnectionResponseInterceptor>();

                foreach (IHttpConnectionInterceptor httpConnInterceptor in interceptors)
                {
                    if (httpConnInterceptor == null ||
                        !(httpConnInterceptor is IHttpConnectionRequestInterceptor || httpConnInterceptor is IHttpConnectionResponseInterceptor))
                        throw new DataException(DataException.Configuration_InvalidHttpInterceptor, string.Format("Http interceptors must implement either IHttpConnectionRequestInterceptor or " +
                                "IHttpConnectionResponseInterceptor interfaces. Class {0} doesn't implement either if these interfaces.", httpConnInterceptor.GetType()));

                    if (httpConnInterceptor is IHttpConnectionRequestInterceptor)
                        requestInterceptors.Add(httpConnInterceptor as IHttpConnectionRequestInterceptor);

                    if (httpConnInterceptor is IHttpConnectionResponseInterceptor)
                        responseInterceptors.Add(httpConnInterceptor as IHttpConnectionResponseInterceptor);
                }

                httpHelper = new HttpHelper(accountUri, requestInterceptors, responseInterceptors);

            }
            else
            {
                httpHelper = new HttpHelper(accountUri);
            }
        }
    }
}