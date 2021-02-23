#!/bin/bash

echo --------------------
echo GStore : svn checkout $1 $2
echo --------------------

svn_cmd=/usr/local/bin/svn
svn_url=$1
local_path=$2
username=$3
password=$4

if [ "$#" == "2" ];then
	$svn_cmd checkout $svn_url $local_path
fi

if [ "$#" == "4" ];then
	$svn_cmd checkout $svn_url $local_path --username=$username --password=$password --no-auth-cache
fi

