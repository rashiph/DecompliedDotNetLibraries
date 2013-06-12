namespace System.Runtime.InteropServices.ComTypes
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FILETIME
    {
        public int dwLowDateTime;
        public int dwHighDateTime;
    }
}

