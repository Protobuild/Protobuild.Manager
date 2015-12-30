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

if [ ! -d ~/.config ]; then
  mkdir ~/.config
fi

cd ~/.config/
$WGET -O Protobuild.exe https://protobuild.org/get
$MONO Protobuild.exe --install https://protobuild.org/hach-que/Protobuild.Manager

echo "Installation complete."
