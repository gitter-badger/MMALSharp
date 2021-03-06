﻿// <copyright file="MMALBuffer.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace MMALSharp.Native
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable SA1132 // Each field should be declared on its own line

    public enum MMALBufferProperties
    {
        MMAL_BUFFER_HEADER_FLAG_EOS = 1 << 0,
        MMAL_BUFFER_HEADER_FLAG_FRAME_START = 1 << 1,
        MMAL_BUFFER_HEADER_FLAG_FRAME_END = 1 << 2,
        MMAL_BUFFER_HEADER_FLAG_FRAME = MMAL_BUFFER_HEADER_FLAG_FRAME_START | MMAL_BUFFER_HEADER_FLAG_FRAME_END,
        MMAL_BUFFER_HEADER_FLAG_KEYFRAME = 1 << 3,
        MMAL_BUFFER_HEADER_FLAG_DISCONTINUITY = 1 << 4,
        MMAL_BUFFER_HEADER_FLAG_CONFIG = 1 << 5,
        MMAL_BUFFER_HEADER_FLAG_ENCRYPTED = 1 << 6,
        MMAL_BUFFER_HEADER_FLAG_CODECSIDEINFO = 1 << 7,
        MMAL_BUFFER_HEADER_FLAGS_SNAPSHOT = 1 << 8,
        MMAL_BUFFER_HEADER_FLAG_CORRUPTED = 1 << 9,
        MMAL_BUFFER_HEADER_FLAG_TRANSMISSION_FAILED = 1 << 10,
        MMAL_BUFFER_HEADER_FLAG_DECODEONLY = 1 << 11,
        MMAL_BUFFER_HEADER_FLAG_UNKNOWN = 9998,
        MMAL_BUFFER_HEADER_FLAG_COMPLETEFRAME = 9999,
    }

    public static class MMALBuffer
    {
        public static int MMAL_BUFFER_HEADER_VIDEO_FLAG_INTERLACED = 1 << 0;
        public static int MMAL_BUFFER_HEADER_VIDEO_FLAG_TOP_FIELD_FIRST = 1 << 2;
        public static int MMAL_BUFFER_HEADER_VIDEO_FLAG_DISPLAY_EXTERNAL = 1 << 3;
        public static int MMAL_BUFFER_HEADER_VIDEO_FLAG_PROTECTED = 1 << 4;

        // Pointer to void * Pointer to MMAL_BUFFER_HEADER_T -> Returns MMAL_BOOL_T
        public unsafe delegate int MMAL_BH_PRE_RELEASE_CB_T(IntPtr ptr, MMAL_BUFFER_HEADER_T* ptr2);

#pragma warning disable IDE1006 // Naming Styles
        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_acquire", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_buffer_header_acquire(MMAL_BUFFER_HEADER_T* header);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_reset", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_buffer_header_reset(MMAL_BUFFER_HEADER_T* header);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_release", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_buffer_header_release(MMAL_BUFFER_HEADER_T* header);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_release_continue", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_buffer_header_release_continue(MMAL_BUFFER_HEADER_T* header);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_pre_release_cb_set", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_buffer_header_pre_release_cb_set(MMAL_BUFFER_HEADER_T* header, [MarshalAs(UnmanagedType.FunctionPtr)] MMAL_BH_PRE_RELEASE_CB_T cb, void* userdata);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_replicate", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MMALUtil.MMAL_STATUS_T mmal_buffer_header_replicate(MMAL_BUFFER_HEADER_T* header, MMAL_BUFFER_HEADER_T* header2);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_mem_lock", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe MMALUtil.MMAL_STATUS_T mmal_buffer_header_mem_lock(MMAL_BUFFER_HEADER_T* header);

        [DllImport("libmmal.so", EntryPoint = "mmal_buffer_header_mem_unlock", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void mmal_buffer_header_mem_unlock(MMAL_BUFFER_HEADER_T* header);
#pragma warning restore IDE1006 // Naming Styles
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T
    {
        private uint planes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        private uint[] offset, pitch;
        private uint flags;

        public uint Planes => this.planes;

        public uint[] Offset => this.offset;

        public uint[] Pitch => this.pitch;

        public uint Flags => this.flags;

        public MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T(uint planes, uint[] offset, uint[] pitch, uint flags)
        {
            this.planes = planes;
            this.offset = offset;
            this.pitch = pitch;
            this.flags = flags;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MMAL_BUFFER_HEADER_TYPE_SPECIFIC_T
    {
        private MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T video;

        public MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T Video => video;

        public MMAL_BUFFER_HEADER_TYPE_SPECIFIC_T(MMAL_BUFFER_HEADER_VIDEO_SPECIFIC_T video)
        {
            this.video = video;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MMAL_BUFFER_HEADER_PRIVATE_T
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MMAL_BUFFER_HEADER_T
    {
        private MMAL_BUFFER_HEADER_T* next;
        private IntPtr priv;
        private uint cmd;
        public byte* data;
        public uint allocSize, length, offset, flags;
        public long pts, dts;
        
        private IntPtr type, userData;

        public MMAL_BUFFER_HEADER_T* Next => this.next;

        public IntPtr Priv => this.priv;

        public uint Cmd => this.cmd;

        public uint AllocSize => this.allocSize;

        public uint Length => this.length;

        public uint Offset => this.offset;

        public uint Flags => this.flags;

        public long Pts => this.pts;

        public long Dts => this.dts;

        public IntPtr Type => this.type;

        public IntPtr UserData => this.userData;

        public MMAL_BUFFER_HEADER_T(MMAL_BUFFER_HEADER_T* next, IntPtr priv, uint cmd, byte* data, uint allocSize, uint length, uint offset, uint flags, long pts, long dts, IntPtr type, IntPtr userData)
        {
            this.next = next;
            this.priv = priv;
            this.cmd = cmd;
            this.data = data;
            this.allocSize = allocSize;
            this.length = length;
            this.offset = offset;
            this.flags = flags;
            this.pts = pts;
            this.dts = dts;
            this.type = type;
            this.userData = userData;
        }
    }
}