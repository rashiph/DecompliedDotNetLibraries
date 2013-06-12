namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum PolicyStatementAttribute
    {
        Nothing,
        Exclusive,
        LevelFinal,
        All
    }
}

