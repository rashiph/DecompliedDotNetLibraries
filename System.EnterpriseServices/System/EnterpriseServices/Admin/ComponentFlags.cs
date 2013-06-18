namespace System.EnterpriseServices.Admin
{
    using System;

    [Serializable]
    internal enum ComponentFlags
    {
        AlreadyInstalled = 0x10,
        COMPlusPropertiesFound = 2,
        InterfacesFound = 8,
        NotInApplication = 0x20,
        ProxyFound = 4,
        TypeInfoFound = 1
    }
}

