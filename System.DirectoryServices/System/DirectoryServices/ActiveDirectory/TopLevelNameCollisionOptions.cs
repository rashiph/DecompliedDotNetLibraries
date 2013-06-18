namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    public enum TopLevelNameCollisionOptions
    {
        DisabledByAdmin = 2,
        DisabledByConflict = 4,
        NewlyCreated = 1,
        None = 0
    }
}

