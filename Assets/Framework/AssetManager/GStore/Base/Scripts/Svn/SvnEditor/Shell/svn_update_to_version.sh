#!/bin/bash

echo -----------------------
echo GStore : svn update to version $1 $2
echo -----------------------

svn_cmd=/usr/local/bin/svn
localPath=$1
version=$2
username=$3
password=$4

if [ "$#" == "2" ];then
	$svn_cmd update $localPath -r $version
fi

if [ "$#" == "4" ];then
	$svn_cmd update $localPath -r $version --username=$username --password=$password --no-auth-cache
fi




