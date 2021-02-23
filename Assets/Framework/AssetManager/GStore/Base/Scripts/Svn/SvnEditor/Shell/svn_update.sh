#!/bin/bash

echo -----------------------
echo GStore : svn update $1
echo -----------------------

svn_cmd=/usr/local/bin/svn
localPath=$1
username=$2
password=$3

if [ "$#" == "1" ];then
	$svn_cmd update $localPath
fi

if [ "$#" == "3" ];then
	$svn_cmd update $localPath --username=$username --password=$password --no-auth-cache
fi




