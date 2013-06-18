namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct EtwTraceLevel
    {
        internal const int None = 0;
        internal const int Fatal = 1;
        internal const int Error = 2;
        internal const int Warning = 3;
        internal const int Information = 4;
        internal const int Verbose = 5;
    }
}

