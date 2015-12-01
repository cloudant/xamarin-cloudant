# Getting Started with Cloudant Client


The basics of document CRUD from a Cloudant database.

```csharp
using IBM.Cloudant.Client;

// create a CloudantClient instance
var client = new CloudantClientBuilder ("my-account-name"){
    username = "username",
    password = "password"
}.GetResult();

// access the database
var db = client.Database ("databasename");

// ensure the database exists on the server
db.EnsureExists ();

// create a document
var createTask = db.Create (new DocumentRevision(){
    docId = "doc1"
    body = new Dictionary(){
        ["hello"] = "world"
    }
    });
createTask.Wait ();

// read a document
var readTask = db.Read ( "docId");
readTask.Wait ();
var document = readTask.Result;

// update a document
document.body["updated"] = true;

var updateTask = db.Update (document);
updateTask.Wait ();
var updatedDocument = updateTask.Result;

// delete a document
db.Delete(updatedDocument).Wait();
```

### Finding Data

Xamarin-cloudant directly supports the [Cloudant Query API ](https://docs.cloudant.com/cloudant_query.html).

#### Create Indexes

```csharp
// create a JSON index
db.CreateJsonIndex (fields:new List<SortField>(){
        new SortField(){
            name = "foo",
            sort = Sort.desc
        },
        new SortField(){
            name = "bar"
            sort = Sort.asc
        }
    }).Wait ();

// create a text index
db.CreateTextIndex (fields: new List<TextIndexField>(){
        new TextIndexField(){
            new TextIndexField(){
                name = "foo",
                type = Type.String
            }
        }
    })
```

#### Query for documents


```csharp
var queryTask = db.Query (new Dictionary<string,object>(){
        ["foo"] = "bar"
});
queryTask.Wait();

var documents = queryTask.Result;
foreach (document in documents){
    Console.WriteLine ("Found document with id: "+document.docId);
}

```
#### Deleting an index

```csharp
// delete a JSON index
var deleteJSONIndexTask = db.deleteIndex ("example",
    "exampleDesignDoc",
    Index.json);
deleteJSONIndexTask.wait();

// delete a text index
var deleteTextIndexTask = db.deleteIndex ("example",
    "exampleDesignDoc",
    Index.text);
deleteTextIndexTask.wait();

```
