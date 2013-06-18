namespace System.Runtime.DurableInstancing
{
    using System;

    [Flags]
    public enum InstanceValueConsistency
    {
        None,
        InDoubt,
        Partial
    }
}

