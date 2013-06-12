namespace System.Web.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ExpiresPage
    {
        internal ExpiresEntry[] _entries;
        internal int _pageNext;
        internal int _pagePrev;
    }
}

