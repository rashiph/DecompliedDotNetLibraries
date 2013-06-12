namespace System.Reflection.Emit
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum AssemblyBuilderAccess
    {
        ReflectionOnly = 6,
        Run = 1,
        RunAndCollect = 9,
        RunAndSave = 3,
        Save = 2
    }
}

