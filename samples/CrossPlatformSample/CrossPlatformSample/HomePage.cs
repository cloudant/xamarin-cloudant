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
using System.Threading.Tasks;
using Xamarin.Forms;
using IBM.Cloudant.Client;
using System.Net;

namespace CrossPlatformSample
{
	/// <summary>
	/// Defines the home page of the application.
	/// </summary>
	public class HomePage : ContentPage
	{
		/// <summary>
		/// Class to link the sample items to a code snippet.
		/// </summary>
		class CommandItem
		{
			public string Title { set; get; }
			public Action ItemSelected { set; get; }
		};

		private CloudantClient client { get;}
		private static readonly string dbName = "sampledb";
		private static readonly string docName = "sampleDoc";
		private Database db = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="CrossPlatformSample.HomePage"/> class.
		/// </summary>
		/// <param name="client">Valid CloudantClient object initialized with the account information.</param>
		public HomePage (CloudantClient client)
		{
			this.client = client;

			//List of sample items.
			List<CommandItem> items = new List<CommandItem> {
				new CommandItem { Title = " 1. Create Database", ItemSelected = OnCreateDB },
				new CommandItem { Title = " 2. Save Document", ItemSelected = OnSaveDocument },
				new CommandItem { Title = " 3. Retrieve Document", ItemSelected = OnRetrieveDocument},
				new CommandItem { Title = " 4. Update Document", ItemSelected = OnUpdateDocument},
				new CommandItem { Title = " 5. Create Index", ItemSelected = OnCreateIndex },
				new CommandItem { Title = " 6. List Indexes", ItemSelected = OnListIndexes },
				new CommandItem { Title = " 7. Find By Index", ItemSelected = OnFindByIndex },
				new CommandItem { Title = " 8. Delete Index", ItemSelected = OnDeleteIndex },
				new CommandItem { Title = " 9. Delete Database", ItemSelected = OnDeleteDB }
			};	
				
			InitializeUserInterface (items);
		}


		/// <summary>
		/// Code sample to create a database.
		/// </summary>
		private void OnCreateDB ()
		{
			try{
				Task<Database> dbTask = client.Database (dbName, true);
				dbTask.Wait(); 

				db = dbTask.Result;
				DisplayAlert ("Database Created","Database name: " + db.dbname,"OK");
			
			}catch(Exception e){
				HandleException (e, "Create Database Failed");
			}
		}


		/// <summary>
		/// Code sample to save a document in the database.
		/// </summary>
		private void OnSaveDocument ()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;

			try{
				//Data to be saved
				Dictionary<string, object> docContents = new Dictionary<string, object> ();
				docContents.Add ("Name", "Mike");
				docContents.Add ("age", 28);
				docContents.Add ("boolValue", true);
				docContents.Add ("savedDate", DateTime.Now.ToString());

				//Create a DocumentRevision and set the data to be saved in the body.
				DocumentRevision doc = new DocumentRevision (){
					docId = docName,
					body = docContents
				};

				//Save to the database.
				Task<DocumentRevision> saveTask = db.Save (doc);
				saveTask.Wait ();

				DocumentRevision rev = saveTask.Result;

				DisplayAlert ("Document Saved", DisplayDocument(rev), "OK");

			}catch(AggregateException ae) when (ae.GetBaseException() is DataException &&
						(ae.GetBaseException() as DataException).code == DataException.Database_SaveDocumentRevisionFailure ){

				DisplayAlert("Error Saving Document","The document already exists. To update an existing document use update().", "OK");

			} catch (Exception e){
				HandleException (e, "Error Saving Document");
			}
		}


		/// <summary>
		/// Code sample to retrieve a document from the database.
		/// </summary>
		private void OnRetrieveDocument ()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;

			try{
				Task<DocumentRevision> findTask = db.Find (docName);
				findTask.Wait ();

				DocumentRevision rev = findTask.Result;

				DisplayAlert ("Document Retrieved", DisplayDocument(rev), "OK");

			}
			catch(AggregateException ae) when (ae.GetBaseException () is DataException &&
					(ae.GetBaseException () as DataException).code == DataException.Database_FetchDocumentRevisionFailure ) {

				DisplayAlert ("Error Rerieving Document", "Document does not exist, it must be created first.", "OK");
			}
			catch (Exception e){
				HandleException (e, "Error Retrieving Document");
			}
		}


		private void OnUpdateDocument()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;

			//Retrieve the latest document revision.
			DocumentRevision doc;
			try{
				Task<DocumentRevision> findTask = db.Find (docName);
				findTask.Wait ();
				doc = findTask.Result;
			} catch{
				DisplayAlert ("Error Updating Document", "Document does not exist, it must be created first.", "OK");
				return;
			}

			//Update data in the DocumentRevision.
			doc.body["savedDate"] = DateTime.Now.ToString();

			//Save a new revision in the database.
			try{
				Task<DocumentRevision> updateTask = db.Update (doc);
				updateTask.Wait ();

				DocumentRevision rev = updateTask.Result;
				DisplayAlert ("Document Updated", DisplayDocument(rev), "OK");
			} catch(Exception e){
				HandleException (e, "Error Updating Document");
			}
		}

		/// <summary>
		/// Sample code to create an index in the database.
		/// </summary>
		private void OnCreateIndex ()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;

			string indexName = "sampleIndex";
			string designDocName = "sampleIndexDoc";
			string indexField = "sampleIndexField";

			// Create the index
			try{
				Task indexTask = db.CreateIndex (indexName, designDocName, "json",
					                 new IndexField[]{ new IndexField (indexField) });
				indexTask.Wait ();

				DisplayAlert ("Index Created", "index name: " + indexName, "OK");
			}
			catch (Exception e){
				HandleException (e, "Error Creating Index");
			}
		}

		/// <summary>
		/// Sample code to list all existing indexes in the database.
		/// </summary>
		private void OnListIndexes()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;

			try{
				Task<List<Index>> indexListTask = db.ListIndices ();
				indexListTask.Wait();

				List<Index> indexList = indexListTask.Result;

				if (indexList != null && indexList.Count > 0) {
					string displayString="";
					foreach (Index index in indexList) {
						displayString += index.name + "\n";
					}
					DisplayAlert ("Indexes Found", displayString, "OK");

				} else {
					DisplayAlert ("No Indexes Found", "Database has no indexes.", "OK");
				}
			}
			catch (Exception e){
				HandleException (e, "Error Listing Indexes");
			}
		}

		/// <summary>
		/// Sample code to search for a document with a given index.
		/// </summary>
		private void OnFindByIndex ()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;
			
			string indexName = "index1";
			string designDocName = "index1design";
			string indexField = "age";

			try{
				//Create an index for the field 'age'.
				Task indexTask = db.CreateIndex (indexName, designDocName, "json",
					new IndexField[]{ new IndexField (indexField) });
				indexTask.Wait ();

				String selectorJSON = "\"selector\": {\"age\": {\"$eq\":28} }";

				// Find all documents with indexes that atch the given selector.
				// In this example it returns all documents where 'age' is 28.
				Task <List<DocumentRevision>> findTask = db.FindByIndex(selectorJSON,
					new FindByIndexOptions()
					.Sort(new IndexField(indexField, IndexField.SortOrder.desc))
				);
				findTask.Wait ();

				List<DocumentRevision> searchResult = findTask.Result;

				if(searchResult.Count > 0) {
					DocumentRevision rev = searchResult [0];
					DisplayAlert ("Find By Index Succeeded", DisplayDocument(rev), "OK");
				} else{
					DisplayAlert ("Find By Index Failed", "No documents were found, a document must be created first.", "OK");
				}
			} catch(Exception e){
				HandleException (e, "Error Finding By Index");
			}
		}


		/// <summary>
		/// Sample code to delete an index from the database.
		/// </summary>
		private void OnDeleteIndex ()
		{
			//Database must exist.
			if (!ValidateDatabaseExists ())
				return;
			
			string indexName = "sampleIndex";
			string designDocName = "sampleIndexDoc";

			try{
				Task indexTask = db.DeleteIndex (indexName, designDocName);
				indexTask.Wait ();

				DisplayAlert ("Index Deleted", "index name: " + indexName, "OK");
			} catch (Exception e){
				HandleException (e, "Error Deleting Index");
			}
		}

		/// <summary>
		/// Sample code to delete a database.
		/// </summary>
		private void OnDeleteDB ()
		{
			if (!ValidateDatabaseExists ())
				return;
			
			string name = db.dbname;

			try{
				Task deleteDbTask = client.DeleteDB (db);
				deleteDbTask.Wait ();

				DisplayAlert ("Database Deleted", "Database name: " + name, "OK");
				db = null;
			} catch(Exception e){
				HandleException (e, "Error Deleting Database");
			}
		}




		// ======== PRIVATE HELPERS =============

		/// <summary>
		/// Helper method to validate if a the database already exist.
		/// </summary>
		/// <returns><c>true</c>, if the database exists and we have a reference to it, <c>false</c> otherwise.</returns>
		private bool ValidateDatabaseExists(){
			if (db != null)
				return true;

			DisplayAlert ("Database Doesn't Exist","Database must be created first. Operation failed.", "OK");
			return false;
		}

		/// <summary>
		/// Helper class to convert the document contents to a string.
		/// </summary>
		/// <returns>A string with the dictionary contents.</returns>
		/// <param name="d">A DocumentRevision object.</param>
		private string DisplayDocument(DocumentRevision rev){
			Dictionary<string,object> dictionary = rev.body;

			string body="";
			foreach(string key in dictionary.Keys){
				object value;
				dictionary.TryGetValue(key, out value);
				body+=string.Format("{0} : {1}\n",key, value.ToString());
			}

			return string.Format ("docId: {0}\nrevId: {1}\n\n--- Document data ---\n{2}", rev.docId, rev.revId, body);
		}

		/// <summary>
		/// Helper method to display an error message for a given exception.
		/// </summary>
		/// <param name="e">Exception.</param>
		/// <param name="dialogTitle">Title for the error dialog. Should contain the operation where the error occurred.</param>
		private void HandleException(Exception e, string dialogTitle){
			if (e is AggregateException) {
				if ((e.GetBaseException () is WebException) || (e.GetBaseException() is DataException) ) {
					DisplayAlert (dialogTitle, e.GetBaseException ().Message, "OK");
				} else
					throw e.GetBaseException ();
			}
			else if(e is DataException) {
				DisplayAlert (dialogTitle, e.Message, "OK");
			}
			else{
				Debug.WriteLine ("Unexpected exception: " + e.Message);
				throw e;
			}
		}

		/// <summary>
		/// Helper method to initialize the user interface.
		/// </summary>
		/// <param name="items">List of CommandItems showcased by this app.</param>
		private void InitializeUserInterface(List<CommandItem> items){

			Title = "Cloudant Client Sample";
			BackgroundColor = Color.FromHex("3B99D4");

			//Defines how each cell in the list displays it's data.
			DataTemplate dataTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label{TextColor = Color.Black,
						YAlign = TextAlignment.Center,
						VerticalOptions = LayoutOptions.FillAndExpand
					};
					nameLabel.SetBinding(Label.TextProperty, "Title");

					return new ViewCell{
						View = new StackLayout
						{
							Padding = new Thickness(10, 1),

							Children = 
							{
								nameLabel,
							}
						}
					};
				}
			);

			//Creates a list widget to display the app sample actions.
			ListView listView = new ListView {
				ItemsSource = items,
				ItemTemplate = dataTemplate,
				BackgroundColor = Color.White
			};

			// Configures selection action on the list items.
			listView.ItemSelected += (sender, e) => {
				if (e.SelectedItem == null) return;

				(e.SelectedItem as CommandItem).ItemSelected ();
				listView.SelectedItem = null; 
			};

			//Creates a layout that displays the app header and the list widget.
			Content = new StackLayout { 
				Padding = new Thickness(0, 50),
				Children = {
					new Label { Text = "Cloudant Client Sample", 
						XAlign = TextAlignment.Center, 
						TextColor=Color.White, 
						FontSize=24
					},

					listView,
				}
			};
		}
	}
}