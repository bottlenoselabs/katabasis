// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using C2CS;
using static FNA3D_Image;

namespace Katabasis
{
    public static unsafe class FNA
    {
        private static int _readGlobal;
        private static int _writeGlobal;
        private static readonly Dictionary<IntPtr, Stream> _writeStreams = new();
        private static readonly Dictionary<IntPtr, Stream> _readStreams = new();

        private static readonly FNA3D_Image_ReadFunc _readFunc = new() { Pointer = &INTERNAL_Read };
        private static readonly FNA3D_Image_WriteFunc _writeFunc = new() { Pointer = &INTERNAL_Write };
        private static readonly FNA3D_Image_SkipFunc _skipFunc = new() { Pointer = &INTERNAL_Skip };
        private static readonly FNA3D_Image_EOFFunc _eofFunc = new() { Pointer = &INTERNAL_EOF };

        [UnmanagedCallersOnly]
        private static int INTERNAL_Read(
			void* context,
			CString8U data,
			int size)
		{
			Stream stream;
			lock (_readStreams)
			{
				stream = _readStreams[(IntPtr)context];
			}

			byte[] buf = new byte[size]; // FIXME: Preallocate!
			var result = stream.Read(buf, 0, size);
			Marshal.Copy(buf, 0, data, result);
			return result;
		}

        [UnmanagedCallersOnly]
        private static void INTERNAL_Write(
			void* context,
			void* data,
			int size)
		{
			Stream stream;
			lock (_writeStreams)
			{
				stream = _writeStreams[(IntPtr)context];
			}

			byte[] buf = new byte[size]; // FIXME: Preallocate!
			Marshal.Copy((IntPtr)data, buf, 0, size);
			stream.Write(buf, 0, size);
		}

        [UnmanagedCallersOnly]
        private static void INTERNAL_Skip(void* context, int n)
		{
			Stream stream;
			lock (_readStreams)
			{
				stream = _readStreams[(IntPtr)context];
			}

			stream.Seek(n, SeekOrigin.Current);
		}

        [UnmanagedCallersOnly]
        private static int INTERNAL_EOF(void* context)
		{
			Stream stream;
			lock (_readStreams)
			{
				stream = _readStreams[(IntPtr)context];
			}

			return stream.Position == stream.Length ? 1 : 0;
		}

        public static IntPtr ReadImageStream(
            Stream stream,
            out int width,
            out int height,
            out int len,
            int forceW = -1,
            int forceH = -1,
            bool zoom = false)
        {
            IntPtr context;
            lock (_readStreams)
            {
                context = (IntPtr)_readGlobal++;
                _readStreams.Add(context, stream);
            }

            int w, h, l;
            var pixels = FNA3D_Image_Load(
                _readFunc,
                _skipFunc,
                _eofFunc,
                (void*)context,
                &w,
                &h,
                &l,
                forceW,
                forceH,
                (byte)(zoom ? 1 : 0));
            width = w;
            height = h;
            len = l;

            lock (_readStreams)
            {
                _readStreams.Remove(context);
            }

            return (IntPtr)pixels;
        }

        public static void WritePNGStream(
            Stream stream,
            int srcW,
            int srcH,
            int dstW,
            int dstH,
            IntPtr data)
        {
            IntPtr context;
            lock (_writeStreams)
            {
                context = (IntPtr)_writeGlobal++;
                _writeStreams.Add(context, stream);
            }

            FNA3D_Image_SavePNG(
                _writeFunc,
                (void*)context,
                srcW,
                srcH,
                dstW,
                dstH,
                (byte*)data);

            lock (_writeStreams)
            {
                _writeStreams.Remove(context);
            }
        }

        public static void WriteJPGStream(
            Stream stream,
            int srcW,
            int srcH,
            int dstW,
            int dstH,
            IntPtr data,
            int quality)
        {
            IntPtr context;
            lock (_writeStreams)
            {
                context = (IntPtr)_writeGlobal++;
                _writeStreams.Add(context, stream);
            }

            FNA3D_Image_SaveJPG(
                _writeFunc,
                (void*)context,
                srcW,
                srcH,
                dstW,
                dstH,
                (byte*)data,
                quality);

            lock (_writeStreams)
            {
                _writeStreams.Remove(context);
            }
        }
    }
}
