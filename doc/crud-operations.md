# Performing CRUD operations

You can modify the content of a data store.  Operations can be performed using either callbacks or a Promises API.

## Creating data

You can save new objects and save changes to existing objects. Use the same operation for both new and existing objects.

**Save a document.**

The following example saves an object using a callback function.

```cs
// A store that has been previously created.
Store store = existingStore;

String stringKey = "stringKey";
String stringValue = "nicestringvalue";

String numberKey = "numberKey";
int numberValue = 42;

Dictionary<String, Object> dictionary = new Dictionary<String, Object>();
dictionary.Add(stringKey, stringValue);
dictionary.Add(numberKey, numberValue);

MutableDocumentRevision revision = new MutableDocumentRevision();
revision.setBody(dictionary);

// save the document
try{
  Task<Object> task = store.save(revision);
  task.Wait ();

  if(task.IsFaulted){
      // handle Error
  }
  else{
      // saved document is returned in task.Result
      DocumentRevision doc = task.Result;
  }
}
catch (Exception e){
  // handle exception
}
```

## Reading data

You can fetch a document.

**Read documents.**

```cs
// A store that has been previously created.
Store store = existingStore;
// An existing document ID.
String docId = existingDocumentId;

try{
  Task<Object> task = fetchById(docId;
  task.Wait ();

  if(task.IsFaulted){
      // handle Error
  }
  else{
      // document is returned in task.Result
      DocumentRevision doc = task.Result;
  }
}
catch (Exception e){
  // handle exception
}

```

## Updating data

To update a document, run a save on an existing document. Because the item already exists, it is updated.

**Update documents.**

```cs
// A store that has been previously created.
Store store = existingStore;

String newKey = "newKey";
int newValue = 43;

// create a copy of the document revision and update the body
MutableDocumentRevision mutableDocumentRevision = savedRevision.mutableCopy();
Dictionary<String, Object> body = mutableDocumentRevision.getBody();
body.Add(newKey, newValue);
body.Remove(stringKey);
mutableDocumentRevision.setBody(body);

// save the document
try{
  Task<Object> task = store.save(mutableDocumentRevision);
  task.Wait ();

  if(task.IsFaulted){
      // handle Error
  }
  else{
      // saved document is returned in task.Result
      DocumentRevision doc = task.Result;
  }
}
catch (Exception e){
  // handle exception
}

```

### Deleting data

To delete a document, pass the document that you want to delete to the store.

**Delete documents.**

```cs
// A store that has been previously created.
Store store = existingStore;

// A document that exists.
DocumentRevision documentToDelete = existingDocumentRevision;

// delete the document
try{
  Task<Object> task = store.save(documentToDelete);
  task.Wait ();

  if(task.IsFaulted){
      // handle Error
  }
  else{
      // document revision ID is returned in task.Result
      String deletedRevisionId = task.Result;
  }
}
catch (Exception e){
  // handle exception
}
```

Next learn about how to index the data you have created so you can perform complex queries, see [Index management](./doc/creating-indexes.md).
