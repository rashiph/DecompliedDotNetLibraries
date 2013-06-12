namespace System.Reflection.Emit
{
    using System;

    [Flags]
    internal enum DynamicAssemblyFlags
    {
        AllCritical = 1,
        Aptca = 2,
        Critical = 4,
        None = 0,
        Transparent = 8,
        TreatAsSafe = 0x10
    }
}

