namespace System.Security.AccessControl
{
    using System;

    [Flags]
    public enum PropagationFlags
    {
        None,
        NoPropagateInherit,
        InheritOnly
    }
}

