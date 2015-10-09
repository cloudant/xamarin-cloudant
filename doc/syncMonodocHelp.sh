#!/bin/bash

DESCRIPTION="This script synchronizes the XML documentation files in
the ./doc directory with the source comments. The script 
must run after a successful build of the target."

TARGET="$1"

if [ "$TARGET" != "Debug" ] && [ "$TARGET" != "Release" ]
	then
	echo "Usage: syncMonodocHelp.sh [target]
	      
 - target: Valid values are 'Debug' or 'Release'.
	
$DESCRIPTION
"
	exit 1
fi

echo "Building monodoc help... "
echo "  TARGET:$TARGET"

DIR="${BASH_SOURCE%/*}"
if [[ ! -d "$DIR" ]]; then DIR="$PWD"; fi

# Update the help in the /doc directory
mdoc update --delete -fno-assembly-versions -L/Developer/MonoAndroid/usr/lib/mandroid/platforms/android-21 -L/Developer/MonoTouch/usr/lib/mono/2.1/Facades -L/Developer/MonoTouch/usr/lib/mono/Xamarin.iOS/ "--import=$DIR/../src/CloudantClient.PCL/bin/$TARGET/CloudantClient.PCL.xml" "-o" "$DIR" "$DIR/../src/CloudantClient.PCL/bin/$TARGET/CloudantClient.PCL.dll"

# Assemble the help
mdoc assemble --debug -o $DIR/CloudantClient $DIR

mdoc export-msxdoc --out=$DIR/../src/CloudantClient.PCL/bin/$TARGET/CloudantClient.PCL.xml $DIR