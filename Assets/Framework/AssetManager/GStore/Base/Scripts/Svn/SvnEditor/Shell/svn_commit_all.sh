#!/bin/bash

echo -----------------------
echo GStore : svn commit add delete motify $1 $2
echo -----------------------

svn_cmd=/usr/local/bin/svn
svn_message=$2
svn_username=$3
svn_password=$4

cd $1

if [ "$#" == "2" ];then
	#需要增加的文件 - 支持符号@和空格
	$svn_cmd status . | grep '^\?' | tr '^\?' ' ' | sed 's/[ ]*//' | sed 's/[ ]/\\ /g' | sed 's/$/&@/' | xargs svn add

	#需要删除的文件 - 支持符号@和空格
	$svn_cmd status . | grep '^\!' | tr '^\!' ' ' | sed 's/[ ]*//' | sed 's/[ ]/\\ /g' | sed 's/$/&@/' | xargs svn del

	$svn_cmd commit -m "$svn_message"
fi

if [ "$#" == "4" ];then
	#需要增加的文件 - 支持符号@和空格
	$svn_cmd status . --username=$svn_username --password=$svn_password --no-auth-cache | grep '^\?' | tr '^\?' ' ' | sed 's/[ ]*//' | sed 's/[ ]/\\ /g' | sed 's/$/&@/' | xargs svn add

	#需要删除的文件 - 支持符号@和空格
	$svn_cmd status . --username=$svn_username --password=$svn_password --no-auth-cache | grep '^\!' | tr '^\!' ' ' | sed 's/[ ]*//' | sed 's/[ ]/\\ /g' | sed 's/$/&@/' | xargs svn del

	$svn_cmd commit -m "$svn_message" --username=$svn_username --password=$svn_password --no-auth-cache
fi