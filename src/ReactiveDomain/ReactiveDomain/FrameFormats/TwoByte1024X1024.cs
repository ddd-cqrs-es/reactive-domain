﻿using System;
using System.Runtime.InteropServices;
using NLog;
using ReactiveDomain.Memory;
using ReactiveDomain.Util;

namespace ReactiveDomain.FrameFormats
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TwoByte1024X1024Frame
    {
        public VideoFrameHeader FrameHeader;
        public fixed byte pixels[2*1024*1024];
    }

    public unsafe class TwoByte1024X1024Image : Image
    {
        private static readonly Logger Log = NLog.LogManager.GetLogger("Common");

        public TwoByte1024X1024Image(byte* buffer)
            : base(buffer, sizeof(TwoByte1024X1024Frame))
        {
        }

        public TwoByte1024X1024Image()
        {
            BufferSize = sizeof(TwoByte1024X1024Frame);
        }

        public static int PixelBufferLength => 2 * 1024 * 1024;

    }
}
