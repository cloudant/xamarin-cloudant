Contributing
=======

Cloudant-client is written in C# using the Xamarin environment.

## Requirements

Xamarin Studio


### Installing requirements

Follow the instructions at https://xamarin.com/platform


## Project Structure

### CloudantClient.PCL

Portable Class Library (PCL) project for Cloudant Client.

### Sample

Xamarin sample application that showcases the use of the Cloudant Client.  To run the sample follow the instructions at `./Sample/README.md`

### Test.Shared

Project used to share tests cases among platforms (iOS and Android).  All new tests cases in this solution must be common for all platforms.

### Test.iOS

Project to run the test cases in the iOS environment.  DO NOT add tests cases here, tests should be added to the Test.Shared project.

### Test.Android

Project to run the test cases in the Android environment.  DO NOT add tests cases here, tests should be added to the Test.Shared project.

## Test Suite

Test are written using the Xamarin, which uses the NUnit framework.

#### Configuration

Open the solution `CloudantClient.sln` in Xamarin Studio.

Modify TestConstants.cs  to add your Cloudant account and credentials.

~~~ cs
public static readonly string cloudantHost = "your-cloudant-username.cloudant.com";
public static readonly string cloudantUser = "your-cloudant-username";
public static readonly string cloudantPassword = "your-cloudant-password";
~~~

#### Running the tests

Tests should run in any supported platform.  The solution contains iOS and Android projects to launch the tests from these platforms.

### iOS

Right-click on project Test.iOS, then select `Run With -> [iOS simulator or device]`

### Android

Right-click on project Test.Android, then select `Run With -> [Android simulator or device]`
