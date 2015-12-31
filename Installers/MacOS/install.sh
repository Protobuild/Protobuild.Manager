#!/bin/bash

WGET=$(which wget)
if [ "$WGET" == "" ]; then
  echo "wget not found.  You might need to install it with brew install wget."
  exit 1
fi

cd ~/
$WGET -O Protobuild.exe https://protobuild.org/get
if [ $? -ne 0 ]; then
  # Fallback to HTTP
  $WGET -O Protobuild.exe http://protobuild.org/get
fi
if [ $? -ne 0 ]; then
  echo "Unable to download Protobuild from the internet"
  exit 1
fi
/usr/local/bin/mono Protobuild.exe --install https://protobuild.org/hach-que/Protobuild.Manager

echo "Installation complete."
