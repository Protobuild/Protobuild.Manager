#!/bin/bash

CURL=$(which curl)
if [ "$CURL" == "" ]; then
  echo "curl not found.  You might need to install it with brew install wget."
  exit 1
fi

cd ~/
$CURL -L https://protobuild.org/get > Protobuild.exe
if [ $? -ne 0 ]; then
  # Fallback to HTTP
  $CURL -L http://protobuild.org/get > Protobuild.exe
fi
if [ $? -ne 0 ]; then
  echo "Unable to download Protobuild from the internet"
  exit 1
fi
/usr/local/bin/mono Protobuild.exe --install https://protobuild.org/hach-que/Protobuild.Manager
chmod a+x ~/Applications/Protobuild.Manager.app/Contents/MacOS/Protobuild.Manager

echo "Installation complete."

open -a ~/Applications/Protobuild.Manager.app
