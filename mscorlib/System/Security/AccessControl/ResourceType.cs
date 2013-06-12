namespace System.Security.AccessControl
{
    using System;

    public enum ResourceType
    {
        Unknown,
        FileObject,
        Service,
        Printer,
        RegistryKey,
        LMShare,
        KernelObject,
        WindowObject,
        DSObject,
        DSObjectAll,
        ProviderDefined,
        WmiGuidObject,
        RegistryWow6432Key
    }
}

