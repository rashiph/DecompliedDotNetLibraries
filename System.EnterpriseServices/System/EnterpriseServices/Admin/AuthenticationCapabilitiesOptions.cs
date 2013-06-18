namespace System.EnterpriseServices.Admin
{
    using System;

    [Serializable]
    internal enum AuthenticationCapabilitiesOptions
    {
        DynamicCloaking = 0x40,
        None = 0,
        SecureReference = 2,
        StaticCloaking = 0x20
    }
}

