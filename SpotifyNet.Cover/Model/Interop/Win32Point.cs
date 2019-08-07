using System;
using System.Runtime.InteropServices;

namespace SpotifyNet.Cover.Model.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
        public Int32 X;
        public Int32 Y;
    };
}
