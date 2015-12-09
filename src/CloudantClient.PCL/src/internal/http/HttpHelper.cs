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
using Newtonsoft.Json;
using System.Runtime.ExceptionServices;

namespace IBM.Cloudant.Client
{
    /// <summary>
    /// Internal class to manage the HTTP connection.
    /// </summary>
    /// <remarks>
    /// This class is used to manage HTTP requests and responses.
    /// </remarks>
    public class HttpHelper
    {
        private HttpClient client = new HttpClient(new HttpClientHandler(){ UseCookies = false });
        // By setting UseCookies = false, we allow
        // interceptors to set the Cookie header.
        /// <summary>
        /// List of interceptors that apply to http requests made by this HttpHelper instance.
        /// </summary>
        /// <value>The request interceptors.</value>
        public List<IHttpConnectionRequestInterceptor> requestInterceptors { set; private get; }  = new List<IHttpConnectionRequestInterceptor> ();

        private int numberOfRetries = 10;

        /// <summary>
        /// List of interceptors that apply to http responses received by this HttpHelper instance.
        /// </summary>
        /// <value>The response interceptors.</value>
        public List<IHttpConnectionResponseInterceptor> responseInterceptors { set; private get; } = new List<IHttpConnectionResponseInterceptor> ();

        private static readonly String DEFAULT_CHARSET = "UTF-8";

        /// <summary>
        /// Initializes a new instance of the <see cref="IBM.Cloudant.Client.HttpHelper"/> class.
        /// </summary>
        /// <param name="baseUri">Base URI for the HttpClient in this helper. All http requests must be relative to this URI.</param>
        public HttpHelper(Uri baseUri)
        {
            client.BaseAddress = baseUri;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBM.Cloudant.Client.HttpHelper"/> class.
        /// </summary>
        /// <param name="requestInterceptors">Request interceptors.</param>
        /// <param name="responseInterceptors">Response interceptors.</param>
        /// <param name="baseUri">Base URI for the HttpClient in this helper. All http requests must be relative to this URI.</param>
        public HttpHelper(Uri baseUri,
                          List<IHttpConnectionRequestInterceptor> requestInterceptors,
                          List<IHttpConnectionResponseInterceptor> responseInterceptors)
        {
            Debug.WriteLine("ENTRY HttpHelper: Constructor() - requestInterceptors: " + requestInterceptors + "  responseInterceptors: " + responseInterceptors);
            client.BaseAddress = baseUri;
            this.requestInterceptors = requestInterceptors;
            this.responseInterceptors = responseInterceptors;
        }

        /// <summary>
        /// Sends a sends a HTTP GET request to the specified endpoint.
        /// <param name="uri"> Endpoint to send the request to.</param>
        /// <param name="headers"> Additional headers to pass for the request</param>
        /// </summary>
        public Task<HttpResponseMessage> GetAsync(Uri uri, Dictionary<String,String> headers)
        {
            Debug.WriteLine("HttpHelper :: GetAsync()");
            return SendJSONRequestAsync(HttpMethod.Get, uri, headers, null);
        }

        /// <summary>
        /// Sends an Http DELETE request to the given URI using the given headers.
        /// </summary>
        /// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
        /// <param name="uri">URI for this request.</param>
        /// <param name="headers">Http headers for this request.</param>
        public Task<HttpResponseMessage> DeleteAsync(Uri uri, Dictionary<String, String> headers)
        {
            Debug.WriteLine("HttpHelper :: DeleteAsync()");
            return SendJSONRequestAsync(HttpMethod.Delete, uri, headers, null);
        }

        /// <summary>
        /// Sends an Http PUT request to the given URI using the given headers and content.
        /// </summary>
        /// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
        /// <param name="uri">URI for this request.</param>
        /// <param name="headers">Http headers for this request.</param>
        /// <param name="body">The request contents.</param>
        public Task<HttpResponseMessage> PutAsync(Uri uri,
                                                  Dictionary<String, String> headers,
                                                  string body)
        {
            Debug.WriteLine("HttpHelper :: PutAsync()");
            return SendJSONRequestAsync(HttpMethod.Put, uri, headers, body);
        }


        /// <summary>
        /// Sends an Http POST request to the given URI using the given headers and content.
        /// </summary>
        /// <returns>An async Task wich once completed contains the resulting HttpResponseMessage.</returns>
        /// <param name="uri">URI for this request.</param>
        /// <param name="headers">Http headers for this request.</param>
        /// <param name="body">The request contents.</param>
        public Task<HttpResponseMessage> PostAsync(Uri uri,
                                                   Dictionary<String, String> headers,
                                                   string body)
        {
            Debug.WriteLine("HttpHelper :: PostAsync()");
            return SendJSONRequestAsync(HttpMethod.Post, uri, headers, body);
        }

        /// <summary>
        /// Adds global headers to the HttpClient.
        /// </summary>
        /// <param name="name">Header name.</param>
        /// <param name="value">Header value.</param>
        public void AddGlobalHeader(string name, string value)
        {
            client.DefaultRequestHeaders.Add(name, value);
        }

        /// <summary>
        /// Removes a header from the HttpClient global headers.
        /// </summary>
        /// <param name="name">Name.</param>
        public void RemoveGlobalHeader(string name)
        {
            client.DefaultRequestHeaders.Remove(name);
        }

        /// <summary>
        /// Maximum number of times to rety a request when it has been intercepted and any interceptor sets
        /// the <see cref="IBM.Cloudant.Client.HttpConnectionInterceptorContext.replayRequest"/>
        /// flag to true.
        /// </summary>
        /// <param name="numberOfRetries">Number of retries.</param>
        public void SetNumberOfRetries(int numberOfRetries)
        {
            this.numberOfRetries = numberOfRetries;
        }



        private Task<HttpResponseMessage> SendJSONRequestAsync(HttpMethod method,
                                                               Uri uri,
                                                               Dictionary<string, string> headers,
                                                               string body)
        {

            var actualHeaders = (headers == null) ? new Dictionary<string,string>() : new Dictionary<string,string>(headers);
            actualHeaders.Add("Accept", "application/json");

            HttpContent content = null;
            if (body != null)
            {
                content = new System.Net.Http.StringContent(body);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=" + DEFAULT_CHARSET);
            }
            return SendRequestAsync(method, uri, actualHeaders, content);
        }



        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method,
                                                                 Uri uri,
                                                                 Dictionary<string, string> headers,
                                                                 HttpContent content)
        {
            Debug.WriteLine("HttpHelper:sendRequestAsync()  METHOD:" + method);

            Boolean retry = true;
            int n = numberOfRetries;

            while (retry && n-- > 0)
            {
                retry = false;

                using (var request = new HttpRequestMessage(method, uri))
                {

                    if (headers != null)
                    {
                        foreach (KeyValuePair<string,string> item in headers)
                        {

                            request.Headers.TryAddWithoutValidation(item.Key, item.Value);
                        }
                    }

                    request.Content = content;


                    //Call request interceptors
                    var currHttpConnContext = new HttpConnectionInterceptorContext(
                                                  request,
                                                  null);

                    foreach (IHttpConnectionRequestInterceptor requestInterceptor in requestInterceptors)
                    {
                        currHttpConnContext = requestInterceptor.InterceptRequest(currHttpConnContext);
                    }


                    //Process the request
                    try
                    {
                        Debug.WriteLine(FormatHttpRequestLog(request));

                        HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(continueOnCapturedContext: false);
                        Debug.WriteLine(FormatHttpResponseLog(response));

                        //Call response interceptors
                        currHttpConnContext.responseMsg = response;
                        foreach (IHttpConnectionResponseInterceptor responseInterceptor in responseInterceptors)
                        {
                            currHttpConnContext = responseInterceptor.InterceptResponse(currHttpConnContext);
                        }

                        retry = currHttpConnContext.replayRequest;

                        if (!retry)
                            return response;

                    }
                    catch (AggregateException ae)
                    {
                        Debug.WriteLine("=== Sent request, got EXCEPTION response. Message: " + ae.GetBaseException().Message);
                        ExceptionDispatchInfo.Capture(ae.GetBaseException()).Throw();
                    }
                }
            }

            Debug.WriteLine("Maximum number of retries reached");
            throw new Exception("Maximum number of retries reached.  An HttpConnectionResponseInterceptor has set the replayRequest " +
                "flag to true, but the maximum number of retries [" + numberOfRetries + "] has been reached.");
        }




        private string FormatHttpRequestLog(HttpRequestMessage request)
        {
            string contentString = request.Content == null ? "null" : request.Content.ToString();
            return string.Format("Http Request\n   {0,-10}:{1}\n   {2,-10}:{3}\n   {4,-10}:{5}\n   {6,-10}:{7}\n   {8,-10}:{9}",
                "METHOD", request.Method,
                "URI", new Uri(client.BaseAddress, request.RequestUri),
                "HEADERS", FormatAndSanitizeHeaders(request.Headers),
                "CONTENT", contentString,
                "PROPERTIES", FormatProperties(request.Properties));
        }

        private string FormatHttpResponseLog(HttpResponseMessage response)
        {
            string contentString = response.Content == null ? "null" : response.Content.ToString();
            return string.Format("Http Response\n   {0,-8}:[{1}] {2}\n   {3,-8}:{4}\n   {5,-8}:{6}\n   {7,-8}:{8}",
                "URI", response.RequestMessage.Method, response.RequestMessage.RequestUri,
                "STATUS", response.StatusCode,
                "HEADERS", FormatAndSanitizeHeaders(response.Headers),
                "CONTENT", contentString);
        }

        /// <summary>
        /// Removes password from Authentication header for safe logging.
        /// </summary>
        /// <returns>A string with the HttpHeaders where the user credentials have been replaced by ***** </returns>
        /// <param name="headers">Headers.</param>
        private string FormatAndSanitizeHeaders(HttpHeaders headers)
        {

            if (headers == null)
                return "null";

            int count = 0;
            string result = string.Empty;
            foreach (KeyValuePair<String,IEnumerable<string>> header in headers)
            {
                foreach (var value in header.Value)
                {
                    var valueToWrite = value;
                    if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) ||
                        header.Key.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
                    {
                        valueToWrite = " ******";
                    }
                    result += string.Format("\n\t[{0}]{1} : {2}", count++, header.Key, valueToWrite);

                }
            }
            return result;
        }

        private string FormatProperties(IDictionary<string, object> props)
        {

            if (props == null)
                return "null";
            else if (props.Keys.Count == 0)
                return "none";


            string result = string.Empty;
            int count = 0;
            foreach (string key in props.Keys)
            {
                object value;
                props.TryGetValue(key, out value);
                result += string.Format("\n\t[{0}] {1} : {2}", count++, key, value);
            }
            return result;
        }
    }
}
