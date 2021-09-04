#!/bin/bash

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
FNA_LIBS_DIR=$DIR/fnalibs

# SDL
$DIR/ext/sdl-cs/library.sh

# FNA3D
echo "Building FNA3D"
$DIR/ext/FNA3D-cs/library.sh /Users/lstranks/Programming/FNA3D-cs/lib/libSDL2-2.0.dylib $DIR/SDL/include

# Downloading
echo "Downloading latest FNA libraries ..."
curl -L https://github.com/deccer/FNA-libs/archive/refs/heads/main.zip > "$DIR/fnalibs.zip"
if [ $? -eq 0 ]; then
    echo "Finished downloading!"
else
    >&2 echo "ERROR: Unable to download successfully."
    exit 1
fi

# Decompressing
echo "Decompressing FNA libraries ..."
mkdir -p $FNA_LIBS_DIR
unzip $DIR/fnalibs.zip -d $FNA_LIBS_DIR
if [ $? -eq 0 ]; then
    echo "Finished decompressing!"
    rm $DIR/fnalibs.zip
else
    >&2 echo "ERROR: Unable to decompress successfully."
    exit 1
fi

FNA_LIBS_DIR=$DIR/fnalibs/FNA-libs-main
LIB_DIR=$DIR/lib
mkdir -p $LIB_DIR

# Move files to specific places...
echo "Moving files ..."
# FAudio
mv $FNA_LIBS_DIR/lib64/libFAudio.so.0 $LIB_DIR/libFAudio.so
mv $FNA_LIBS_DIR/osx/libFAudio.0.dylib $LIB_DIR/libFAudio.dylib
mv $FNA_LIBS_DIR/x64/FAudio.dll $LIB_DIR/FAudio.dll
# FNA3D
mv $FNA_LIBS_DIR/lib64/libFNA3D.so.0 $LIB_DIR/libFNA3D.so
mv $FNA_LIBS_DIR/osx/libFNA3D.0.dylib $LIB_DIR/libFNA3D.dylib
mv $FNA_LIBS_DIR/osx/libMoltenVK.dylib $LIB_DIR/libMoltenVK.dylib
mv $FNA_LIBS_DIR/osx/libvulkan.1.dylib $LIB_DIR/libvulkan.dylib
# theorafile
mv $FNA_LIBS_DIR/lib64/libtheorafile.so $LIB_DIR/libtheorafile.so
mv $FNA_LIBS_DIR/osx/libtheorafile.dylib $LIB_DIR/libtheorafile.dylib
mv $FNA_LIBS_DIR/x64/libtheorafile.dll $LIB_DIR/libtheorafile.dll
echo "Finished moving files!"

## Delete uncompressed folder
rm -rf $FNA_LIBS_DIR
