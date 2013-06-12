namespace System.Web.Caching
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct UsageEntryLink
    {
        internal UsageEntryRef _next;
        internal UsageEntryRef _prev;
    }
}

