namespace System.Web
{
    using System;

    internal enum FileAction
    {
        Added = 1,
        Dispose = -2,
        Error = -1,
        Modified = 3,
        Overwhelming = 0,
        Removed = 2,
        RenamedNewName = 5,
        RenamedOldName = 4
    }
}

