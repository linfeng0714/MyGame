#!/bin/bash

echo -----------------------
echo GStore : svn revert $1
echo -----------------------

svn_cmd=/usr/local/bin/svn
localPath=$1
username=$2
password=$3


if [ "$#" == "1" ];then
	$svn_cmd revert -R $localPath
fi

if [ "$#" == "3" ];then
	$svn_cmd revert -R $localPath --username=$username --password=$password --no-auth-cache
fi




