# Getting Started with Cloudant Client

## Basic document CRUD examples using a Cloudant database

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

## Finding Data

Xamarin-cloudant directly supports the [Cloudant Query API ](https://docs.cloudant.com/cloudant_query.html).

### Create Indexes

```csharp
// create a JSON index
await db.CreateJsonIndexAsync(fields:new List<SortField>(){
        new SortField(){
            name = "foo",
            sort = Sort.desc
        },
        new SortField(){
            name = "bar"
            sort = Sort.asc
        }
    });

// create a text index
await db.CreateTextIndexAsync(fields: new List<TextIndexField>(){
        new TextIndexField(){
            new TextIndexField(){
                name = "foo",
                type = Type.String
            }
        }
    })
```

### Query for documents


```csharp
var documents = await db.Query (new Dictionary<string,object>(){
        ["foo"] = "bar"
});

foreach (document in documents){
    Console.WriteLine ("Found document with id: "+document.docId);
}

```
### Delete indexes

```csharp
// delete a JSON index
await  db.deleteIndex ("example",
    "exampleDesignDoc",
    Index.json);

// delete a text index
await db.deleteIndex ("example",
    "exampleDesignDoc",
    Index.text);

```
