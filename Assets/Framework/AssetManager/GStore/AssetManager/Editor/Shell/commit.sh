#!/bin/sh
echo ------------------------------------------------
echo GStore : start commit files...
echo ------------------------------------------------
batch_dir=$(cd `dirname $0`; pwd)/

#设置字符集，否则提交中文文件会报错
export LC_CTYPE=en_US.UTF-8

#第一个参数是备注
arg1="$1"

cd $batch_dir../../../../..

svn_cmd=/usr/local/bin/svn

#需要增加的文件
to_add_file=(`$svn_cmd status "$@" | grep ^? | awk '{printf "%s ", $2}'`)
if [ "$to_add_file" != "" ];then
	$svn_cmd add --no-ignore ${to_add_file[*]}
fi

#再提交
$svn_cmd commit -m "$@"

