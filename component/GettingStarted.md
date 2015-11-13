# Getting Started with Cloudant Client

## Overview

[IBM Cloudant®](https://cloudant.com) is a NoSQL database platform built for the cloud.

The **Cloudant Client** component enables your Xamarin applications to interact with databases in the Cloudant service.

To use this library you will need a valid Cloudant account in either a local system or in the cloud service.  If you don't have an account, you can sign up for a free trial [here](https://cloudant.com/sign-up/).


## Getting Started

First, instantiate a `CloudantClient` object using your Cloudant account information.

```csharp
using Com.Cloudant.Client;
...
Uri accountUri = new Uri("https://my-cloudant-account.cloudant.com");
            CloudantClient client = new CloudantClientBuilder (accountUri) {
                username = "my-username",
                password = "my-password"
            }.GetResult ();
```

Then, we get a `Database` object.

```csharp
Database db = client.database ("my-database", true);
```

Once we have the `Database` we can save, update, and retrieve documents. We can also create indexes to search the database.

#### Saving Data

For this example, we'll store the following data.

```csharp
//Data to be saved
Dictionary<string, object> person = new Dictionary<string, object> ();
person.Add ("name", "Mike");
person.Add ("age", 32);
person.Add ("married", false);
```

First, create a 'DocumentRevision' and set the data to be saved in the `body` property.
**Tip:** `docId` is an optional property, however, if you don't provide it, Cloudant will generate one for you.  This could complicate the logic of your application because you may have to track the docIdif you want to reference the document in the future.

```csharp
DocumentRevision personDoc = new DocumentRevision (){
            docId = "person",
            body = person
        };
```

Then save the data.

```csharp
personDoc = db.save (personDoc).Result;
```

#### Retrieving Data

Now you want to retrieve the data.  You start by finding your document and creating a `DocumentRevision`.

```csharp
DocumentRevision retrievedDoc = db.find("person").Result;
```

Then you can access the data from the `body` of the `DocumentRevision`

```csharp
Dictionary<string,object> retrievedPerson = retrievedDoc.body;

Console.WriteLine("Name: "+retrievedPerson["name"]);
Console.WriteLine("Age: "+retrievedPerson["age"]);
Console.WriteLine("Married: "+retrievedPerson["married"]);
```

#### Updating Saved Data

To update your document, modify the data in the `DocumentRevision body` property and then use the `update()` API.

```csharp
personDoc.body ["married"] = true;
personDoc = db.update (personDoc).Result;
```

#### Searching your Data

We want to search the database for documents that meet a certain criteria.  For example, we want to find all married people.

We start by creating an index for the `married` field.

```csharp
db.createIndex ("index_married",
                "index_married_design", "json",
                new IndexField[]{ new IndexField ("married") }
).Wait();
```

Then, create a selector with your search criteria.  

```csharp
string selectorJSON = "\"selector\": {\"married\": {\"$eq\":true} }";
```
**Tip:** To learn more about selectors, refer to the [Cloudant documentation](https://docs.cloudant.com/cloudant_query.html#query-parameters).  

Finally, call the `findByIndex()` API and display your results.

```csharp
Task <List<DocumentRevision>> findTask = db.findByIndex(selectorJSON,
                new FindByIndexOptions()
                .sort(new IndexField("married", IndexField.SortOrder.desc))
            );
findTask.Wait ();

List<DocumentRevision> searchResult = findTask.Result;

//Display the result
Console.WriteLine ("Number of records found where married==true : " + searchResult.Count);
foreach(DocumentRevision d in searchResult)
        Console.WriteLine (string.Format("Name: {0}  Age: {1}", d.body ["name"], d.body["age"]));
```
