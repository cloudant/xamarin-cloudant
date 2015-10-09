#!/bin/bash

# This script used to package the component for submision 
# to the Xamarin Component Store

# NOTE: We are building the help manually becuse xamrin-component.exe fails with an exception.
#       I suspect it is because we build a PCL and the assembly dependencies are different from 
#       what the tool assumes. I had to add this directory  "-L/Developer/MonoTouch/usr/lib/mono/2.1/Facades"
#       for the mdoc command to work correctly.


# remove previous output
rm -r ./doc
rm -r ./doc-html

# Build the library
mdtool build -c:Release ../CloudantClient-Xamarin.sln

# Assemble the help
mdoc update "-L/Developer/MonoAndroid/usr/lib/mandroid/platforms/android-21" "-L/Developer/MonoTouch/usr/lib/mono/2.1/Facades" "-L/Developer/MonoTouch/usr/lib/mono/Xamarin.iOS/" "--import=./../CloudantClient.PCL/bin/Release/CloudantClient.PCL.xml" "-o" "./doc" "./../CloudantClient.PCL/bin/Release/CloudantClient.PCL.dll"
mdoc export-html --out="./doc-html/" ./doc

# Assemble component for Xamarin store.
mono xamarin-component.exe package
