namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum PolicyLevelType
    {
        User,
        Machine,
        Enterprise,
        AppDomain
    }
}

