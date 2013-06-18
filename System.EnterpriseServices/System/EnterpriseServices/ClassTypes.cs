namespace System.EnterpriseServices
{
    using System;

    [Serializable, Flags]
    internal enum ClassTypes
    {
        All = 3,
        Event = 1,
        Normal = 2
    }
}

