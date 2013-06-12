namespace System.Web.SessionState
{
    using System;

    [Flags]
    internal enum SessionStateItemFlags
    {
        None,
        Uninitialized,
        IgnoreCacheItemRemoved
    }
}

