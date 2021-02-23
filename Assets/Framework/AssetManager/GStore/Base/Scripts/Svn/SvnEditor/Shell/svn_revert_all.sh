#!/bin/bash

echo -----------------------
echo GStore : svn revert all $1
echo -----------------------

svn_cmd=/usr/local/bin/svn
localPath=$1
username=$2
password=$3


if [ "$#" == "1" ];then
	$svn_cmd revert —R $localPath
	$svn_cmd status $localPath | grep ^\? | awk '{print $2}' | xargs rm -rf
fi

if [ "$#" == "3" ];then
	$svn_cmd revert —R $localPath --username=$username --password=$password --no-auth-cache
	$svn_cmd status $localPath --username=$username --password=$password | grep ^\? | awk '{print $2}' | xargs rm -rf
fi




