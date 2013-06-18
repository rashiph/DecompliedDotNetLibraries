namespace System.Web.Compilation
{
    using System;

    [Flags]
    public enum PrecompilationFlags
    {
        AllowPartiallyTrustedCallers = 0x20,
        Clean = 8,
        CodeAnalysis = 0x10,
        Default = 0,
        DelaySign = 0x40,
        FixedNames = 0x80,
        ForceDebug = 4,
        OverwriteTarget = 2,
        Updatable = 1
    }
}

