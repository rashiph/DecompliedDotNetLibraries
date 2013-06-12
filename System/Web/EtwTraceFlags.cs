namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Size=1)]
    internal struct EtwTraceFlags
    {
        internal const int None = 0;
        internal const int Infrastructure = 1;
        internal const int Module = 2;
        internal const int Page = 4;
        internal const int AppSvc = 8;
    }
}

