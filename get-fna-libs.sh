#!/bin/bash

# Get the directory of this script
MY_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
FNA_LIBS_DIR=$MY_DIR/fnalibs

 # Downloading
echo "Downloading latest FNA libraries ..."
curl https://dl.dropboxusercontent.com/s/fk6a5yre6agja92/fnalibs.tar.bz2 > "$MY_DIR/fnalibs.tar.bz2"
if [ $? -eq 0 ]; then
    echo "Finished downloading!"
else
    >&2 echo "ERROR: Unable to download successfully."
    exit 1
fi

# Decompressing
echo "Decompressing FNA libraries ..."
mkdir -p $FNA_LIBS_DIR
tar xjC $FNA_LIBS_DIR -f $MY_DIR/fnalibs.tar.bz2
if [ $? -eq 0 ]; then
    echo "Finished decompressing!"
    rm $MY_DIR/fnalibs.tar.bz2
else
    >&2 echo "ERROR: Unable to decompress successfully."
    exit 1
fi

mkdir -p $MY_DIR/lib

# Move files to specific places...
echo "Moving files ..."
# FAudio
FAUDIO_LIB_DIR=$MY_DIR/lib/FAudio
mkdir -p $FAUDIO_LIB_DIR/linux-x64
mkdir -p $FAUDIO_LIB_DIR/osx-x64
mkdir -p $FAUDIO_LIB_DIR/win-x64
mkdir -p $FAUDIO_LIB_DIR/win-x86
mv $FNA_LIBS_DIR/lib64/libFAudio.so.0 $FAUDIO_LIB_DIR/linux-x64/libFAudio.so.0
mv $FNA_LIBS_DIR/osx/libFAudio.0.dylib $FAUDIO_LIB_DIR/osx-x64/libFAudio.0.dylib
mv $FNA_LIBS_DIR/x64/FAudio.dll $FAUDIO_LIB_DIR/win-x64/FAudio.dll
mv $FNA_LIBS_DIR/x86/FAudio.dll $FAUDIO_LIB_DIR/win-x86/FAudio.dll
# FNA3D
FNA3D_LIB_DIR=$MY_DIR/lib/FNA3D
mkdir -p $FNA3D_LIB_DIR/linux-x64
mkdir -p $FNA3D_LIB_DIR/osx-x64
mkdir -p $FNA3D_LIB_DIR/win-x64
mkdir -p $FNA3D_LIB_DIR/win-x86
mv $FNA_LIBS_DIR/lib64/libFNA3D.so.0 $FNA3D_LIB_DIR/linux-x64/libFNA3D.so.0
mv $FNA_LIBS_DIR/osx/libFNA3D.0.dylib $FNA3D_LIB_DIR/osx-x64/libFNA3D.0.dylib
mv $FNA_LIBS_DIR/osx/libMoltenVK.dylib $FNA3D_LIB_DIR/osx-x64/libMoltenVK.dylib
mv $FNA_LIBS_DIR/osx/libvulkan.1.dylib $FNA3D_LIB_DIR/osx-x64/libvulkan.1.dylib
mv $FNA_LIBS_DIR/x64/FNA3D.dll $FNA3D_LIB_DIR/win-x64/FNA3D.dll
mv $FNA_LIBS_DIR/x86/FNA3D.dll $FNA3D_LIB_DIR/win-x86/FNA3D.dll
# SDL2
SDL2_LIB_DIR=$MY_DIR/lib/SDL2
mkdir -p $SDL2_LIB_DIR/linux-x64
mkdir -p $SDL2_LIB_DIR/osx-x64
mkdir -p $SDL2_LIB_DIR/win-x64
mkdir -p $SDL2_LIB_DIR/win-x86
mv $FNA_LIBS_DIR/lib64/libSDL2-2.0.so.0 $SDL2_LIB_DIR/linux-x64/libSDL2-2.0.so.0
mv $FNA_LIBS_DIR/osx/libSDL2-2.0.0.dylib $SDL2_LIB_DIR/osx-x64/libSDL2-2.0.0.dylib
mv $FNA_LIBS_DIR/x64/SDL2.dll $SDL2_LIB_DIR/win-x64/SDL2.dll
mv $FNA_LIBS_DIR/x86/SDL2.dll $SDL2_LIB_DIR/win-x86/SDL2.dll
# theorafile
THEORAFILE_LIB_DIR=$MY_DIR/lib/theorafile
mkdir -p $THEORAFILE_LIB_DIR/linux-x64
mkdir -p $THEORAFILE_LIB_DIR/osx-x64
mkdir -p $THEORAFILE_LIB_DIR/win-x64
mkdir -p $THEORAFILE_LIB_DIR/win-x86
mv $FNA_LIBS_DIR/lib64/libtheorafile.so $THEORAFILE_LIB_DIR/linux-x64/libtheorafile.so
mv $FNA_LIBS_DIR/osx/libtheorafile.dylib $THEORAFILE_LIB_DIR/osx-x64/libtheorafile.dylib
mv $FNA_LIBS_DIR/x64/libtheorafile.dll $THEORAFILE_LIB_DIR/win-x64/libtheorafile.dll
mv $FNA_LIBS_DIR/x86/libtheorafile.dll $THEORAFILE_LIB_DIR/win-x86/libtheorafile.dll
echo "Finished moving files!"

## Delete uncompressed folder
rm -rf $FNA_LIBS_DIR
