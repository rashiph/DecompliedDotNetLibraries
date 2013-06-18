namespace System.Configuration
{
    using System;

    [Flags]
    public enum ConfigurationPropertyOptions
    {
        IsAssemblyStringTransformationRequired = 0x10,
        IsDefaultCollection = 1,
        IsKey = 4,
        IsRequired = 2,
        IsTypeStringTransformationRequired = 8,
        IsVersionCheckRequired = 0x20,
        None = 0
    }
}

