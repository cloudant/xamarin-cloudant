Getting Started
=======

## Before you begin

To run this sample you will need access to a Cloudant system. You will need to provide your Cloudant `account` and a valid `user name` and `password` credentials for the user.

To learn more visit https://cloudant.com

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

