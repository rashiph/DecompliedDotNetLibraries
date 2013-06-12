namespace System.Configuration.Assemblies
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum AssemblyVersionCompatibility
    {
        SameDomain = 3,
        SameMachine = 1,
        SameProcess = 2
    }
}

