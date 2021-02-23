#!/bin/bash

echo --------------------
echo GStore : svn import $1 $2
echo --------------------

svn_cmd=/usr/local/bin/svn
local_path=$1
svn_url=$2
message=$3
username=$4
password=$5

if [ "$#" == "3" ];then
	$svn_cmd import $local_path $svn_url -m message
fi

if [ "$#" == "5" ];then
	$svn_cmd import $local_path $svn_url --username=$username --password=$password --no-auth-cache -m message
fi