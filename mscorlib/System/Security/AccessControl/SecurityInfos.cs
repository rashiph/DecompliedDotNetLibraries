namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum SecurityInfos
    {
        DiscretionaryAcl = 4,
        Group = 2,
        Owner = 1,
        SystemAcl = 8
    }
}

