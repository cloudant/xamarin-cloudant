Getting Started
=======

## Before you begin

To run this sample you will need a valid [IBM Cloudant](https://cloudant.com) account in either a local system or in the cloud service. You will need to provide your Cloudant **account** and a valid **user name** and **password** credentials for the user.

If you don't have an account, you can sign up for a free trial [here](https://cloudant.com/sign-up/).

## Configure the sample

- Load the sample by opening the solution `CrossPlatformSolution.sln` in Xamarin Studio.

- Modify the file `CrossPlatformSample\AppSettings.cs` to add your Cloudant account, user name and password credentials.

~~~ cs
public static readonly string account = "your-cloudant-username.cloudant.com";
public static readonly string username = "your-cloudant-username";
public static readonly string password = "your-cloudant-password";
~~~

## Start the sample

The sample has projects for the iOS and Android environments.

Right-click over the desired project (for example `CrossPlatformSample.iOS`) and select `Run Item`.

## Understand the sample

This sample contains working code snippets that showcase how to use this library.

The class `HomePage.cs` contains the sample code snippets.  For example, if you want to see the source for how to create a database, see the function `OnCreateDB`.
