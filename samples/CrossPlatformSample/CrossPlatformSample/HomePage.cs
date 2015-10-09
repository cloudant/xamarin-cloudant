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
using Com.Cloudant.Client;
using Com.Cloudant.Client.Model;

namespace CrossPlatformSample
{
	public class HomePage : ContentPage
	{
		class CommandItem
		{
			public string Title { set; get; }
			public Action ItemSelected { set; get; }
		};

		private CloudantClient client { get;}
		private Boolean docCreated { set; get; }
		private Boolean indexCreated { set; get; }


		public HomePage (CloudantClient client)
		{

			this.client = client;
			this.docCreated = false;
			this.indexCreated = false;

			List<CommandItem> items = new List<CommandItem> {
				new CommandItem { Title = " 1. Create Database", ItemSelected = OnCreateDB },
				new CommandItem { Title = " 2. Save Document", ItemSelected = OnSaveDocument },
				new CommandItem { Title = " 3. Retrieve Document", ItemSelected = OnRetrieveDocument},
				new CommandItem { Title = " 4. Create Index", ItemSelected = OnCreateIndex },
				new CommandItem { Title = " 5. List Indexes", ItemSelected = OnListIndexes },
				new CommandItem { Title = " 6. Find By Index", ItemSelected = OnFindByIndex },
				new CommandItem { Title = " 7. Delete Index", ItemSelected = OnDeleteIndex },
				new CommandItem { Title = " 8. Delete Database", ItemSelected = OnDeleteDB }
			};

			DataTemplate dataTemplate = new DataTemplate(() =>
				{
					Label nameLabel = new Label{TextColor = Color.Black};
					nameLabel.SetBinding(Label.TextProperty, "Title");

					return new ViewCell{
						View = new StackLayout
						{
							Padding = new Thickness(10, 1),
							Orientation = StackOrientation.Horizontal,
							Children = 
							{
//								new Image {
//									WidthRequest = 40,
//									HeightRequest = 30,
//									BackgroundColor = Color.FromHex("3B99D4")
//								},
								new StackLayout
								{
									VerticalOptions = LayoutOptions.Center,
									Spacing = 0,

									Children = 
									{
										nameLabel,
									}
								}
							}
						}
					};
				});


			ListView listView = new ListView {
				ItemsSource = items,
				ItemTemplate = dataTemplate,
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Color.White
			};
					

			BackgroundColor = Color.FromHex("3B99D4");
			Title = "Cloudant Sample";

			Content = new StackLayout { 
				VerticalOptions = LayoutOptions.Start,
				Padding = new Thickness(0, 40),
				Children = {
					new Label { Text = "Cloudant Sample", XAlign = TextAlignment.Center, TextColor=Color.White},

					listView,
				}
			};
				
			listView.ItemSelected += (sender, e) => {
				if (e.SelectedItem == null) return;

				(e.SelectedItem as CommandItem).ItemSelected ();
				listView.SelectedItem = null; 
			};

		}

		private Database db = null;
		private DocumentRevision lastSaved = null;


		private async void OnCreateDB ()
		{
			Task<Database> dbTask = client.database ("sampledb", true);
			await dbTask; 
			db = dbTask.Result;

			if (dbTask.IsFaulted)
				await DisplayAlert ("Create Database failed", "Database name: " + db.dbname, "OK");
			else {
				await DisplayAlert ("Created Database","Database name: " + db.dbname,"OK");
			}
		}

		private async void OnSaveDocument ()
		{
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			}
			Dictionary<string, object> docContents = new Dictionary<string, object> ();
			docContents.Add ("item1", "value1");
			docContents.Add ("item2", "value2");
			docContents.Add ("int1", 1);

			DocumentRevision doc = new DocumentRevision ();
			doc.body = docContents;

			Task<DocumentRevision> saveTask = db.save (doc);
			saveTask.Wait ();
			lastSaved = saveTask.Result;

			if (saveTask.IsFaulted)
				await DisplayAlert ("Error saving document", saveTask.Exception.Message, "OK");
			else {
				docCreated = true;
				DocumentRevision rev = saveTask.Result;
				await DisplayAlert ("Saved document", "docId:\n"+rev.docId+"\nrevId:\n"+rev.revId, "OK");
			}
		}

		private async void OnRetrieveDocument ()
		{
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			} else if (docCreated == false){
				await DisplayAlert ("Error","Document must be saved first", "OK");
				return;
			}				

			Task<DocumentRevision> findTask = db.find (lastSaved.docId);
			findTask.Wait ();

			if (findTask.IsFaulted)
				await DisplayAlert ("Error on find document", findTask.Exception.Message, "OK");
			else {
				DocumentRevision rev = findTask.Result;
				await DisplayAlert ("Retrieved document", "docId:\n"+rev.docId+"\nrevId:\n"+rev.revId, "OK");
			}
		}

		private async void OnCreateIndex ()
		{
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			}

			string indexName = "sampleIndex";
			string designDocName = "sampleIndexDoc";
			string indexField = "sampleIndexField";

			Task indexTask = db.createIndex (indexName, designDocName, "json",
				                 new IndexField[]{ new IndexField (indexField) });
			indexTask.Wait ();

			if (indexTask.IsFaulted)
				await DisplayAlert ("Error creating index", indexTask.Exception.Message, "OK");
			else {
				this.indexCreated = true;
				await DisplayAlert ("Created Index", "index name: " + indexName, "OK");
			}
		}

		private async void OnListIndexes(){
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			}

			Task<List<Index>> indexListTask = db.listIndices ();

			List<Index> indexList = indexListTask.Result;

			if (indexList != null && indexList.Count > 0) {
				string displayString="";
				foreach (Index index in indexList) {
					displayString += index.name + "\n";
				}
				await DisplayAlert ("Indexes", displayString, "OK");
			} else {
				await DisplayAlert ("Database has no indexes.", "Database has no indexes.", "OK");
			}
		}

		private async void OnFindByIndex ()
		{
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			}
			string indexName = "index1";
			string designDocName = "index1design";
			string indexField = "int1";

			Task indexTask = db.createIndex (indexName, designDocName, "json",
				new IndexField[]{ new IndexField (indexField) });
			indexTask.Wait ();

			String selectorJSON = "\"selector\": {\"int1\": {\"$eq\":1} }";

			Task <List<DocumentRevision>> findTask = db.findByIndex(selectorJSON,
				new FindByIndexOptions()
				.sort(new IndexField(indexField, IndexField.SortOrder.desc))
			);

			findTask.Wait ();

			if (findTask.IsFaulted)
				await DisplayAlert ("Error finding by index", findTask.Exception.Message, "OK");
			else {
				DocumentRevision rev = findTask.Result [0];
				await DisplayAlert ("Find By Index succeeded", "docId:\n"+rev.docId+"\nrevId:\n"+rev.revId, "OK");
			}
		}

		private async void OnDeleteIndex ()
		{
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			} else if (indexCreated == false){
				await DisplayAlert ("Error","Index must be created first", "OK");
				return;
			}				
			string indexName = "sampleIndex";
			string designDocName = "sampleIndexDoc";

			Task indexTask = db.deleteIndex (indexName, designDocName);
			indexTask.Wait ();

			if (indexTask.IsFaulted)
				await DisplayAlert ("Error deleting index", indexTask.Exception.Message, "OK");
			else
				await DisplayAlert ("Deleted Index", "index name: " + indexName, "OK");
		}


		private async void OnDeleteDB ()
		{
			if (db == null) {
				await DisplayAlert ("Error","DB must be created first", "OK");
				return;
			}
			string name = db.dbname;
			Task deleteDbTask = client.deleteDB (db);
			deleteDbTask.Wait ();

			if (deleteDbTask.IsFaulted)
				await DisplayAlert ("Error deleting database", deleteDbTask.Exception.Message, "OK");
			else
				await DisplayAlert("Database deleted","Database name: "+name,"OK");
		}
	}
}