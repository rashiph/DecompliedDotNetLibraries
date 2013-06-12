namespace Microsoft.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum RegistryHive
    {
        ClassesRoot = -2147483648,
        CurrentConfig = -2147483643,
        CurrentUser = -2147483647,
        DynData = -2147483642,
        LocalMachine = -2147483646,
        PerformanceData = -2147483644,
        Users = -2147483645
    }
}

