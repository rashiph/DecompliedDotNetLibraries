namespace System.Security
{
    using System;

    [Flags]
    internal enum PermissionTokenType
    {
        BuiltIn = 8,
        DontKnow = 4,
        IUnrestricted = 2,
        Normal = 1
    }
}

