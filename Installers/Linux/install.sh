#!/bin/bash

WGET=$(which wget)
if [ "$WGET" == "" ]; then
  echo "wget not found.  You might need to install it with your package manager."
  exit 1
fi

MONO=$(which mono)
if [ "$MONO" == "" ]; then
  echo "mono not found.  You might need to install it with your package manager."
  exit 1
fi

GIT=$(which git)
if [ "$GIT" == "" ]; then
  echo "git not found.  You might need to install it with your package manager."
  exit 1
fi

if [ ! -d ~/.config ]; then
  mkdir ~/.config
fi

cd ~/.config/
$WGET -O Protobuild.exe https://github.com/Protobuild/Protobuild/raw/master/Protobuild.exe
if [ $? -ne 0 ]; then
  # Fallback to HTTP
  $WGET -O Protobuild.exe https://github.com/Protobuild/Protobuild/raw/master/Protobuild.exe
fi
if [ $? -ne 0 ]; then
  echo "Unable to download Protobuild from the internet"
  exit 1
fi
$MONO Protobuild.exe --install "https-nuget-v3://api.nuget.org/v3/index.json|Protobuild.Manager"

echo "Installation complete."
