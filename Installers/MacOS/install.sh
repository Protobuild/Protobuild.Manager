#!/bin/bash

CURL=$(which curl)
if [ "$CURL" == "" ]; then
  echo "curl not found.  You might need to install it with brew install wget."
  exit 1
fi

MONO=$(which mono)
if [ "$MONO" == "" ]; then
  echo "mono not found.  You might need to download Mono from the website: http://www.mono-project.com/download/#download-mac"
  exit 1
fi

cd ~/
$CURL -L https://github.com/Protobuild/Protobuild/raw/master/Protobuild.exe > Protobuild.exe
if [ $? -ne 0 ]; then
  # Fallback to HTTP
  $CURL -L https://github.com/Protobuild/Protobuild/raw/master/Protobuild.exe > Protobuild.exe
fi
if [ $? -ne 0 ]; then
  echo "Unable to download Protobuild from the internet"
  exit 1
fi
mono Protobuild.exe --install "https-nuget-v3://api.nuget.org/v3/index.json|Protobuild.Manager"
chmod a+x ~/Applications/Protobuild.Manager.app/Contents/MacOS/Protobuild.Manager

echo "Installation complete."

open -a ~/Applications/Protobuild.Manager.app
