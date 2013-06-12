namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum FileAccess
    {
        Read = 1,
        ReadWrite = 3,
        Write = 2
    }
}

