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

using Xamarin.Forms;
using Com.Cloudant.Client;

namespace CrossPlatformSample
{
	public class App : Application
	{
		public App ()
		{
			//Validate configuration on AppSettings.cs has been changed
			if (AppSettings.account.StartsWith ("your-cloudant") || AppSettings.loginUsername.StartsWith ("your-cloudant") || AppSettings.password.StartsWith ("your-cloudant"))
				MainPage = new ConfigErrorPage ("To run this sample, you must first modify AppSettings.cs to provide your Cloudant account.");

			else {
				try{
					CloudantClient client = new CloudantClientBuilder (AppSettings.account) {
						loginUsername = AppSettings.loginUsername, 
						password = AppSettings.password
					}.GetResult ();

					//TODO: Verify Cloudant can be accessed.
					MainPage = new HomePage (client);

				} catch (Exception e){
					MainPage = new ConfigErrorPage ("Unable to create a CloudantClient. One or more account parameter in AppSettings.cs is incorrect. "+e.Message);	
				}
			}
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}

