# Creating databases

Store client data on a remote Cloudant database.

## Creating remote data stores

Create Store objects to access a remote database. You need to supply a Uri for the data store.  The function remoteStore executes asynchronously using C# Tasks.

**Important:** The database name must be in lower case.

The following example creates a remote store.

```cs
Uri storeUri = new Uri(cloudantUrl + DBName);

Task<Store> task = Store.RemoteStore(storeUri);
task.Wait ();

if(task.IsFaulted){
	Debug.WriteLine ("Failed to create remote DB name: " + DBName + "\nError: " + task.Exception.InnerException.Message);
	//Handle Error
}else{
	remoteStore = task.Result;
	Debug.WriteLine("Sucessfully created store: "+remoteStore.Name);
}
```
