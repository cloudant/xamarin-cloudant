﻿using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using Com.Cloudant.Client;

namespace CrossPlatformSample.iOS
{
	public class Application
	{
		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			//FIXME:  Need to use a class from the PLC, otherwise it is not loaded in the app.
			CloudantClient client = null; 

			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}

