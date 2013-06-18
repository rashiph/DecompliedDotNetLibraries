namespace System.Management
{
    using System;

    [Flags]
    public enum ComparisonSettings
    {
        IgnoreCase = 0x10,
        IgnoreClass = 8,
        IgnoreDefaultValues = 4,
        IgnoreFlavor = 0x20,
        IgnoreObjectSource = 2,
        IgnoreQualifiers = 1,
        IncludeAll = 0
    }
}

