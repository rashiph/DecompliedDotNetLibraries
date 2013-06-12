namespace System.Runtime.InteropServices
{
    using System;

    [Serializable, ComVisible(true)]
    public enum GCHandleType
    {
        Weak,
        WeakTrackResurrection,
        Normal,
        Pinned
    }
}

