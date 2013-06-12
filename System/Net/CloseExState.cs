namespace System.Net
{
    using System;

    [Flags]
    internal enum CloseExState
    {
        Normal,
        Abort,
        Silent
    }
}

