#!/bin/bash
echo '#######################################'
echo '#Experimental Install Script for Linux#'
echo '#######################################'
#if [ `whoami` = "root" ];then
#	echo "You are in sudo"
#
#else
#	echo "Please run in sudo"
#fi
command -v dotnet >/dev/null 2>&1 || { echo >&2 "This application relies on .NET Core 3.0 or newer."; exit 1;}
mkdir temp
cd temp
git clone https://github.com/creeperlv/LWSwnS.git
cd LWSwnS/LWSwnS
dotnet build
cd ..
cd ..
cd ..
mv -f temp/LWSwnS/LWSwnS/LWSwnS/bin/Debug/netcoreapp3.0/ ./LWSwnS/
rm -rf temp