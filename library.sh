#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

if [[ ! -z "$1" ]]; then
    TARGET_BUILD_OS="$1"
fi

if [[ ! -z "$2" ]]; then
    TARGET_BUILD_ARCH="$2"
fi

function set_target_build_os {
    if [[ -z "$TARGET_BUILD_OS" || $TARGET_BUILD_OS == "default" ]]; then
        uname_str="$(uname -a)"
        case "${uname_str}" in
            *Microsoft*)    TARGET_BUILD_OS="microsoft";;
            *microsoft*)    TARGET_BUILD_OS="microsoft";;
            Linux*)         TARGET_BUILD_OS="linux";;
            Darwin*)        TARGET_BUILD_OS="apple";;
            CYGWIN*)        TARGET_BUILD_OS="linux";;
            MINGW*)         TARGET_BUILD_OS="microsoft";;
            *Msys)          TARGET_BUILD_OS="microsoft";;
            *)              TARGET_BUILD_OS="UNKNOWN:${uname_str}"
        esac
        echo "Target build operating system: '$TARGET_BUILD_OS' (default)"
    else
        if [[ "$TARGET_BUILD_OS" == "microsoft" || "$TARGET_BUILD_OS" == "linux" || "$TARGET_BUILD_OS" == "apple" ]]; then
            echo "Target build operating system: '$TARGET_BUILD_OS' (override)"
        else
            echo "Unknown '$TARGET_BUILD_OS' passed as first argument. Use 'default' to use the host build platform or use either: 'microsoft', 'linux', 'apple'."
            exit 1
        fi
    fi
}

function set_target_build_arch {
    if [[ -z "$TARGET_BUILD_ARCH" || $TARGET_BUILD_ARCH == "default" ]]; then
        TARGET_BUILD_ARCH="$(uname -m)"
        echo "Target build CPU architecture: '$TARGET_BUILD_ARCH' (default)"
    else
        if [[ "$TARGET_BUILD_ARCH" == "x86_64" || "$TARGET_BUILD_ARCH" == "arm64" ]]; then
            echo "Target build CPU architecture: '$TARGET_BUILD_ARCH' (override)"
        else
            echo "Unknown '$TARGET_BUILD_ARCH' passed as second argument. Use 'default' to use the host CPU architecture or use either: 'x86_64', 'arm64'."
            exit 1
        fi
    fi
}

set_target_build_os
set_target_build_arch

if [[ "$TARGET_BUILD_OS" == "microsoft" && "$TARGET_BUILD_ARCH" == "x86_64" ]]; then
    RID=win-x64
elif [[ "$TARGET_BUILD_OS" == "linux" && "$TARGET_BUILD_ARCH" == "x86_64" ]]; then
    RID=linux-x64
elif [[ "$TARGET_BUILD_OS" == "apple" && "$TARGET_BUILD_ARCH" == "x86_64" ]]; then
    RID=osx-x64
elif [[ "$TARGET_BUILD_OS" == "apple" && "$TARGET_BUILD_ARCH" == "arm64" ]]; then
    RID=osx-arm64
else 
    echo "Unknown Runtime Identifier for '$TARGET_BUILD_OS' and '$TARGET_BUILD_ARCH'."
    exit 1
fi
LIBS_DIR=$DIR/lib/$RID
mkdir -p $LIBS_DIR

# Build SDL
echo "Building SDL from source..."
$DIR/ext/sdl-cs/library.sh $TARGET_BUILD_OS $TARGET_BUILD_ARCH
mv -v $DIR/ext/sdl-cs/lib/* $LIBS_DIR
if [[ "$TARGET_BUILD_OS" == "linux" ]]; then
    SDL_LIBRARY_FILE_PATH="$LIBS_DIR/libSDL2.so"
elif [[ "$TARGET_BUILD_OS" == "apple" ]]; then
    SDL_LIBRARY_FILE_PATH="$LIBS_DIR/libSDL2.dylib"
elif [[ "$TARGET_BUILD_OS" == "microsoft" ]]; then
    SDL_LIBRARY_FILE_PATH="$LIBS_DIR/SDL2.dll"
fi
echo "Building SDL from source finished!"

# Build FNA3D
echo "Building FNA3D from source..."
$DIR/ext/FNA3D-cs/library.sh $TARGET_BUILD_OS $TARGET_BUILD_ARCH $SDL_LIBRARY_FILE_PATH $DIR/ext/sdl-cs/ext/SDL/include
mv -v $DIR/ext/FNA3D-cs/lib/* $LIBS_DIR
echo "Building FNA3D from source finished!"

# Build FAudio
echo "Building FAudio from source..."
$DIR/ext/FAudio-cs/library.sh $TARGET_BUILD_OS $TARGET_BUILD_ARCH $SDL_LIBRARY_FILE_PATH $DIR/ext/sdl-cs/ext/SDL/include
mv -v $DIR/ext/FAudio-cs/lib/* $LIBS_DIR
echo "Building FAudio from source finished!"

# Build cimgui
echo "Building imgui from source..."
$DIR/ext/imgui-cs/library.sh $TARGET_BUILD_OS $TARGET_BUILD_ARCH
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
    if [[ "$TARGET_BUILD_OS" == "linux" ]]; then
        mv $FNA_LIBS_DIR/lib64/libtheorafile.so $LIBS_DIR/libtheorafile.so
    elif [[ "$TARGET_BUILD_OS" == "apple" ]]; then
        mv $FNA_LIBS_DIR/osx/libMoltenVK.dylib $LIBS_DIR/libMoltenVK.dylib #FNA3D
        mv $FNA_LIBS_DIR/osx/libvulkan.1.dylib $LIBS_DIR/libvulkan.dylib #FNA3D
        mv $FNA_LIBS_DIR/osx/libtheorafile.dylib $LIBS_DIR/libtheorafile.dylib
    elif [[ "$TARGET_BUILD_OS" == "microsoft" ]]; then
        mv $FNA_LIBS_DIR/x64/libtheorafile.dll $LIBS_DIR/libtheorafile.dll
    fi
    echo "Finished moving files!"

    ## Delete uncompressed folder
    rm -rf $FNA_LIBS_DIR
}

download_fna_libraries