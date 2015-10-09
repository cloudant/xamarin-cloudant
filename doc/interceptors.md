HTTP Interceptors
=====

HTTP Interceptors allow the developer to modify the HTTP requests and responses.

Interceptors can be used to implement your own authentication schemes, for example OAuth, or provide custom headers so you can perform your own analysis on usage. They can also be used
to monitor or log the requests made by the library.

To monitor or make changes to HTTP requests, implement one or both of the following in your class:

- To modify the outgoing request, `HTTPConnectionRequestInterceptor`.
- To examine the incoming response, `HTTPConnectionResponseInterceptor`.

For an example of how to implement a interceptor, see the `CookieInterceptor` class.

In order to add an HTTP Interceptor to a remote store, you call the appropriate Store constructor .


For example, this is how to add an instance of `CookieInterceptor` to a remote store:

```cs

Uri storeUri = new Uri(cloudantUrl + DBName);
CookieInterceptor interceptor = new CookieInterceptor("username","password");

Task<Store> task = Store.RemoteStore(storeUri, interceptor);
```


## Things to Avoid

The `Com.Cloudant.Http.HttpConnectionInterceptorContext` object provides access to the underlying `Com.Cloudant.Http.HttpHelper`, `System.Net.Http.HttpRequestMessage`, and `System.Net.Http.HttpResponseMessage` classes. This allows you to change settings and interact with the http request in ways would could potentially cause errors.

For example, reading a `System.Net.Http.HttpResponseMessage` object's input stream will consume the response data, meaning that the initial operation will not receive the data from Cloudant.

Currently the API has only been tested and verified for the following:

* Request Interceptors can modify the request headers and body.
* Response Interceptors can only set the interceptor context replay flag.

Changing anything else is unsupported. In the future, the number of supported APIs is likely to be expanded.
