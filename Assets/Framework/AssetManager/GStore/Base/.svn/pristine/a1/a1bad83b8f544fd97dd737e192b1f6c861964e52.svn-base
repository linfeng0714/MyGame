#!/bin/bash

echo --------------------
echo GStore : svn checkout to version $1 $2 $3
echo --------------------

svn_cmd=/usr/local/bin/svn
svn_url=$1
local_path=$2
version=$3
username=$4
password=$5

if [ "$#" == "3" ];then
	$svn_cmd checkout $svn_url $local_path -r $version
fi

if [ "$#" == "5" ];then
	$svn_cmd checkout $svn_url $local_path -r $version --username=$username --password=$password --no-auth-cache
fi

