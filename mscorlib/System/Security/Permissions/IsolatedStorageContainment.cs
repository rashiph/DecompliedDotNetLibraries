namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true)]
    public enum IsolatedStorageContainment
    {
        AdministerIsolatedStorageByUser = 0x70,
        ApplicationIsolationByMachine = 0x45,
        ApplicationIsolationByRoamingUser = 0x65,
        ApplicationIsolationByUser = 0x15,
        AssemblyIsolationByMachine = 0x40,
        AssemblyIsolationByRoamingUser = 0x60,
        AssemblyIsolationByUser = 0x20,
        DomainIsolationByMachine = 0x30,
        DomainIsolationByRoamingUser = 80,
        DomainIsolationByUser = 0x10,
        None = 0,
        UnrestrictedIsolatedStorage = 240
    }
}

