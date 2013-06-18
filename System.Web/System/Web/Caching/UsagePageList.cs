namespace System.Web.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct UsagePageList
    {
        internal int _head;
        internal int _tail;
    }
}

