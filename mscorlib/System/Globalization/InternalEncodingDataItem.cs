namespace System.Globalization
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct InternalEncodingDataItem
    {
        internal unsafe char* webName;
        internal int codePage;
    }
}

