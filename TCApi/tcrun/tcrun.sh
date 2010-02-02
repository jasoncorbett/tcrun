#!/bin/bash

RUNTIME_DIR=`dirname $0`

export MONO_PATH=$MONO_PATH:$RUNTIME_DIR/lib
mono $RUNTIME_DIR/tcrun.exe "$@"

