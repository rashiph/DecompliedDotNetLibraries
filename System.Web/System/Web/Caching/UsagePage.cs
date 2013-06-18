namespace System.Web.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct UsagePage
    {
        internal UsageEntry[] _entries;
        internal int _pageNext;
        internal int _pagePrev;
    }
}

