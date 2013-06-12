namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum AccessControlSections
    {
        Access = 2,
        All = 15,
        Audit = 1,
        Group = 8,
        None = 0,
        Owner = 4
    }
}

