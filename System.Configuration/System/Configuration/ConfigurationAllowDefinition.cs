namespace System.Configuration
{
    using System;

    public enum ConfigurationAllowDefinition
    {
        Everywhere = 300,
        MachineOnly = 0,
        MachineToApplication = 200,
        MachineToWebRoot = 100
    }
}

