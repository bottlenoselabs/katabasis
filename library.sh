#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
LIBS_DIR=$DIR/lib
mkdir -p $LIBS_DIR

if [[ ! -z "$1" ]]; then
    TARGET_BUILD_PLATFORM="$1"
fi

function set_target_build_platform_host() {
    uname_str="$(uname -a)"
    case "${uname_str}" in
        *Microsoft*)    TARGET_BUILD_PLATFORM="microsoft";;
        *microsoft*)    TARGET_BUILD_PLATFORM="microsoft";;
        Linux*)         TARGET_BUILD_PLATFORM="linux";;
        Darwin*)        TARGET_BUILD_PLATFORM="apple";;
        CYGWIN*)        TARGET_BUILD_PLATFORM="linux";;
        MINGW*)         TARGET_BUILD_PLATFORM="microsoft";;
        *Msys)          TARGET_BUILD_PLATFORM="microsoft";;
        *)              TARGET_BUILD_PLATFORM="UNKNOWN:${uname_str}"
    esac
}

function set_target_build_platform {
    if [[ ! -z "$TARGET_BUILD_PLATFORM" ]]; then
        if [[ $TARGET_BUILD_PLATFORM == "default" ]]; then
            set_target_build_platform_host
            echo "Build platform: '$TARGET_BUILD_PLATFORM' (host default)"
        else
            if [[ "$TARGET_BUILD_PLATFORM" == "microsoft" || "$TARGET_BUILD_PLATFORM" == "linux" || "$TARGET_BUILD_PLATFORM" == "apple" ]]; then
                echo "Build platform: '$TARGET_BUILD_PLATFORM' (cross-compile override)"
            else
                echo "Unknown '$TARGET_BUILD_PLATFORM' passed as first argument. Use 'default' to use the host build platform or use either: 'microsoft', 'linux', 'apple'."
                exit 1
            fi
        fi
    else
        set_target_build_platform_host
        echo "Build platform: '$TARGET_BUILD_PLATFORM' (host default)"
    fi
}

set_target_build_platform

# Build SDL
echo "Building SDL from source..."
$DIR/ext/sdl-cs/library.sh $TARGET_BUILD_PLATFORM
mv -v $DIR/ext/sdl-cs/lib/* $LIBS_DIR
echo "Building SDL from source finished!"

# Build FNA3D
echo "Building FNA3D from source..."
if [[ "$TARGET_BUILD_PLATFORM" == "linux" ]]; then
    SDL_LIBRARY_FILE_PATH="$LIBS_DIR/libSDL2.so"
elif [[ "$TARGET_BUILD_PLATFORM" == "apple" ]]; then
    SDL_LIBRARY_FILE_PATH="$LIBS_DIR/libSDL2.dylib"
elif [[ "$TARGET_BUILD_PLATFORM" == "microsoft" ]]; then
    SDL_LIBRARY_FILE_PATH="$LIBS_DIR/SDL2.dll"
fi
$DIR/ext/FNA3D-cs/library.sh $TARGET_BUILD_PLATFORM $SDL_LIBRARY_FILE_PATH $DIR/ext/sdl-cs/ext/SDL/include
mv -v $DIR/ext/FNA3D-cs/lib/* $LIBS_DIR
echo "Building FNA3D from source finished!"

# Build cimgui
echo "Building imgui from source..."
$DIR/ext/imgui-cs/library.sh $TARGET_BUILD_PLATFORM
mv -v $DIR/ext/imgui-cs/lib/* $LIBS_DIR
echo "Building imgui from source finished!"

function download_fna_libraries() {
    echo "Downloading latest FNA libraries ..."
    FNA_LIBS_DIR=$DIR/fnalibs
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

    # Move files to specific places...
    echo "Moving files ..."
    if [[ "$TARGET_BUILD_PLATFORM" == "linux" ]]; then
        mv $FNA_LIBS_DIR/lib64/libFAudio.so.0 $LIBS_DIR/libFAudio.so
        mv $FNA_LIBS_DIR/lib64/libtheorafile.so $LIBS_DIR/libtheorafile.so
    elif [[ "$TARGET_BUILD_PLATFORM" == "apple" ]]; then
        mv $FNA_LIBS_DIR/osx/libFAudio.0.dylib $LIBS_DIR/libFAudio.dylib
        mv $FNA_LIBS_DIR/osx/libMoltenVK.dylib $LIBS_DIR/libMoltenVK.dylib #FNA3D
        mv $FNA_LIBS_DIR/osx/libvulkan.1.dylib $LIBS_DIR/libvulkan.dylib #FNA3D
        mv $FNA_LIBS_DIR/osx/libtheorafile.dylib $LIBS_DIR/libtheorafile.dylib
    elif [[ "$TARGET_BUILD_PLATFORM" == "microsoft" ]]; then
        mv $FNA_LIBS_DIR/x64/FAudio.dll $LIBS_DIR/FAudio.dll
        mv $FNA_LIBS_DIR/x64/libtheorafile.dll $LIBS_DIR/libtheorafile.dll
    fi
    echo "Finished moving files!"

    ## Delete uncompressed folder
    rm -rf $FNA_LIBS_DIR
}

download_fna_libraries