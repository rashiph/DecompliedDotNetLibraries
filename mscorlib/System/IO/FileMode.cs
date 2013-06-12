namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum FileMode
    {
        Append = 6,
        Create = 2,
        CreateNew = 1,
        Open = 3,
        OpenOrCreate = 4,
        Truncate = 5
    }
}

