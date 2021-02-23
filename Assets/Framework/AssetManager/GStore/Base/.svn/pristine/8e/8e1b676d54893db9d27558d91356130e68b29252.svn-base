@echo off
echo --------------------------------------------------
echo svn export diff $1 $2 $3 $4
echo --------------------------------------------------

remotepath=$1
base=$2
target=$3
localpath=$4
username=$5
password=$6

if [ "$username" != "" ]; then
	token="--username $username --password $password"
fi

for line in `svn diff $token --summarize -r $base:$target $remotepath | grep "^[AM]"`
do
    if [ $line != "A" ] && [ $line != "AM" ] && [ $line != "M" ]; then
        filename=`echo "$line" |sed "s|$remotepath||g"`
        # don't export if it's a directory we've already created
        if [ ! -d $localpath$filename ]; then
            directory=`dirname $filename`
            mkdir -p $localpath$directory
            svn export $token -q --force -r $target $line $localpath$filename
			echo $filename
        fi
    fi
done
