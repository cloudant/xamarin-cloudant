Contributing
=======

The **Cloudant Cient** library is written in C# using Xamarin Studio.

# Developer Certificate of Origin

In order for us to accept pull-requests, the contributor must sign-off a
[Developer Certificate of Origin (DCO)](DCO1.1.txt). This clarifies the
intellectual property license granted with any contribution. It is for your
protection as a Contributor as well as the protection of IBM and its customers;
it does not change your rights to use your own Contributions for any other purpose.

Please read the agreement and acknowledge it by ticking the appropriate box in the PR
 text, for example:

- [x] Tick to sign-off your agreement to the Developer Certificate of Origin (DCO) 1.1

# Requirements

[Xamarin Studio](https://xamarin.com/platform) with a valid license.


# Getting Started

Load the workspace in Xamarin Studio.
`open ./CloudantClient-Xamarin.mdw`.

The workspace contains 2 solutions:
  - **CloudantClient-Xamarin** -
  Contains the source code of this library and automated tests.
  - **CrossPlatformSample** - Contains the sample application for the Xamarin component store.

**Important:** If you experience problems after loading the workspace, see the **Frequent Problems** topic in this file.

## Automated tests

Automated Test are written using the NUnit framework.

#### Configure tests
Before you begin, you need to configure your Cloudant account.
Open `Test.Shared/TestConstants.cs` and provide valid values for **account**, **username**, and **password**.

#### Run tests
There's a test project for each supported platform (iOS and Android).  To run the tests for a platform, just right-click over the project for the platform and select `Run Item`.

#### Contribute additional tests

New tests must be added in the `Tests.Shared` project. All tests must be platform independent.

## Build the Xamarin store component

The elements to build the component configured at `./component/component.yaml`. In this file you can change properties like **version, name, or contents** of the component.

To build the component, run the script at `./component/packageComponent.sh`.

The output of this script is `./component/CloudantClient-[a].[b].xam`, where *a* and *b* represent the version.

#### Install component into local Xamarin Studio

Run `./component mono xamarin-component.exe install CloudantClient-[a].[b].xam`

**IMPORTANT:** See the **Frequent Problems** section for known problems while reinstalling a component.


## Writing documentation

Source code must be documented using C# [XML Documentation Comments](https://msdn.microsoft.com/en-us/library/b2s063f7.aspx).

Additional documentation could be provided with XML files in the `./doc` directory.  For example, XML documentation comments are not supported on namespaces. You can provide documentation for a namespace by editing the file `.doc/ns.[the-namespace].xml`.

## Frequent Problems

#### 1. Package dependency libraries not being updated

**Problem:** Errors appear when building the solution because the package referenced libraries do not exist.

**Reason:** This usually happens when you load the solution for the first time. There is an issue where the package referenced libraries are not correctly updated.

**Solution:** Run the following 2 commands:
- `nuget restore ./src/CloudantClient-Xamarin.sln`
- `nuget restore ./samples/CrossPlatformSample/CrossPlatformSample.sln`

#### 2. 'mono <component> install' fails to update existing component in Xamarin Studio

**Problem:** Xamarin Studio doesn't show the updated component after installing with `mono xamarin-component.exe install CloudantClient.*.*.xam`.

**Reason:** This happens if you have already added the component to any project in the solution.  Xamarin Studio caches a copy of the component and it doesn't get updated unless it has a new version.

**Solution:**
- Remove the component from any projects using it.
- Go to the `./Components` directory for the solution and remove all references to CloudantClient*. Note that projects also have a `./Components` directory, but this must be done in the `./Components` directory that exists in the solution.
- Close and reopen the solution.
- Run the install command `mono xamarin-component.exe install CloudantClient.*.*.xam`

#### 3. Project compiles with warning 'All projects referencing CloudantClient.PCL.csproj must install nuget package Microsoft.Bcl.Build.' even when Microsoft.Bcl.Build is referenced in the project.

**Problem:** The warning shows up even with the correct package referenced.

**Reason:** This is a known Xamarin bug.  See https://bugzilla.xamarin.com/show_bug.cgi?id=29809 for details.

**Solution:** This should be resolved when the Xamarin bug is fixed.
