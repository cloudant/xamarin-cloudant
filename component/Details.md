# IBM Cloudant SDK

The IBM Cloudant SDK for Xamarin helps developers build cross-platform applications which store, query and manipulate data stored in the Cloudant database as a service. Cloudant is a simple and highly-scalable JSON database managed by experts.

## Key Features

- Cloudant provides a managed, scalable, globally-distributed, open-source store for your all your data.
- Pure C# library.
- Provides create, read, update and delete for documents and Cloudant Query capabilities to allow working with data on Cloudant.
- Automatically uses Cloudant's cookie-based sessions for greater security.
- Fully asynchronous operation.

## Getting Started

This sample illustrates basic document manipulation using the SDK.

```csharp
using IBM.Cloudant.Client;

// create a CloudantClient instance
var client = new CloudantClientBuilder("my-account-name"){
    username = "username",
    password = "password"
}.GetResult();

// access the database called "databasename"
var db = client.Database("databasename");

// ensure the database exists on the server
await db.EnsureExistsAsync();

// create a document
var createdDocument = await db.CreateAsync(new DocumentRevision(){
    docId = "doc1"
    body = new Dictionary<string,object>(){
        ["hello"] = "world"
    }
    });

// read a document
var readDocument = await db.ReadAsync("docId");

// update a document
document.body["updated"] = true;

var updatedDocument = await db.UpdateAsync(document);

// delete a document
await db.DeleteAsync(updatedDocument);
```


## Release Notes

### 0.2

- Initial release.
- Supports document create, read, update and delete.
- Supports querying using Cloudant Query.
