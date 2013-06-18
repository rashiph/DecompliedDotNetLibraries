namespace System.Configuration
{
    using System;

    [Flags]
    internal enum ConfigurationValueFlags
    {
        Default = 0,
        Inherited = 1,
        Locked = 4,
        Modified = 2,
        XMLParentInherited = 8
    }
}

