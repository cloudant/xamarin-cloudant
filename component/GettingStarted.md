# Cloudant Xamarin Client

This is the official Cloudant library for Xamarin

* [Installation and Usage](#installation-and-usage)
* [Getting Started](#getting-started)
* [API Reference](#api-reference)
* [Development](#development)
* [License](#license)

## Installation and Usage

Xamarin Store:

Install from Xamarin component store.

## Getting Started

Now it's time to begin doing real work with Cloudant and Xamarin. For working code samples of any of the API's please go to our Test suite.

Initialize your Cloudant connection by constructing a *Com.Cloudant.Client.CloudantClient* . If you are connecting the managed service on cloudant.com, supply the *account* to connect to, *userName* and *password*. If you are connecting to Cloudant Local supply its URL, the *userName* and  *password*

Connecting to the managed service at cloudant.com example
~~~ cs

CloudantClient client = new CloudantClientBuilder("mdb")
   {loginUsername = "mdb",
    password = "passw0rd"}.GetResult();

Console.WriteLine("Connected to Cloudant");
~~~


Connecting to Cloudant Local example
~~~ cs

CloudantClient client = new CloudantClientBuilder("https://9.149.23.12")
   {loginUsername = "mdb",
    password = "passw0rd"}.GetResult();

Console.WriteLine("Connected to Cloudant");

~~~

### Complete example

Here is simple but complete example of working with data:

~~~ cs

string password = System.getProperty("cloudant_password");
CloudantClient client = new CloudantClientBuilder("mdb")
   {loginUsername = "mdb",
    password = password}.GetResult();

// Clean up the database we created previously.
client.deleteDB("alice");

// Create a new database.
client.createDB("alice");

// specify the database we are going to use
Database db = client.database("alice", false);

// and insert a document in it
db.save(new Rabbit(true));
Console.WriteLine("You have inserted the Rabbit");
Rabbit r = db.find(Rabbit.class,"rabbit");
Console.WriteLine(r);

   ...
public class Rabbit {
	private boolean crazy;
	private string _id = "rabbit";

	public Rabbit(boolean isCrazy) {
		crazy = isCrazy;
	}

	public string ToString() {
		return " { id : " + _id + ", rev : " + _rev + ", crazy : " + crazy + "}";
	}
}
~~~

If you run this example, you will see:

    you have inserted the rabbit.
    { crazy: true,
      id: rabbit,
      rev: 1-6e4cb465d49c0368ac3946506d26335d
    }

## API Reference

Refer to XML documentation at ...

## Development

Details about this project and development information, including how to run the automated tests is included in [CONTRIBUTING.md](./CONTRIBUTING.md)

## License

Copyright 2014-2015 Cloudant, an IBM company.

Licensed under the apache license, version 2.0 (the "license"); you may not use this file except in compliance with the license.  you may obtain a copy of the license at

    http://www.apache.org/licenses/LICENSE-2.0.html

Unless required by applicable law or agreed to in writing, software distributed under the license is distributed on an "as is" basis, without warranties or conditions of any kind, either express or implied. See the license for the specific language governing permissions and limitations under the license.
