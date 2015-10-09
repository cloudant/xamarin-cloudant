#!/bin/bash

# This script used to package the component for submision 
# to the Xamarin Component Store.

# Need to Clean the project because API help isn't always recreated.
/Applications/"Xamarin Studio.app"/Contents/MacOS/mdtool build -t:Clean ../src/CloudantClient.PCL/CloudantClient.PCL.csproj

# Build the library, need to build the project in advance because 
# we need the API documentation XML to build the monodoc help.
/Applications/"Xamarin Studio.app"/Contents/MacOS/mdtool build  -c:Release ../src/CloudantClient.PCL/CloudantClient.PCL.csproj

# Build the monodoc help
../doc/syncMonodocHelp.sh Release

# Assemble component for Xamarin store.
mono xamarin-component.exe package