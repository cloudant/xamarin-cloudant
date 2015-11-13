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

namespace CrossPlatformSample
{
    public class ConfigErrorPage : ContentPage
    {

        public ConfigErrorPage (string errorMessage)
        {
            BackgroundColor = Color.FromHex("3B99D4");
            Title = "Cloudant Sample";
            Content = new StackLayout { 
                Padding = new Thickness(10, 40),
                Children = {
                    new Label { Text = "Configure Cloudant Account" , TextColor=Color.White, HorizontalOptions=LayoutOptions.Center},
                    new Label { Text = "", HeightRequest=30},
                    new Label { Text = errorMessage , TextColor=Color.White},
                    new Label { Text = "", HeightRequest=30},
                    new Label { Text = "Please restart the application.", TextColor=Color.White}
                }
            };
        }
    }
}


