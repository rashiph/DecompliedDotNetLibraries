namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum CallingConventions
    {
        Any = 3,
        ExplicitThis = 0x40,
        HasThis = 0x20,
        Standard = 1,
        VarArgs = 2
    }
}

