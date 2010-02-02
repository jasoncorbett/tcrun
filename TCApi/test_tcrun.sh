#!/bin/bash

BASEDIR=`dirname "$0"`
cd $BASEDIR
VER=`cat Version.txt`

cd Output/tcrun-$VER/
./tcrun.sh -s TCApi >/dev/null 2>&1

./tcrun.sh TCApi
