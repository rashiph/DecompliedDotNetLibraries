namespace System.EnterpriseServices
{
    using System;

    [Serializable, Flags]
    public enum InstallationFlags
    {
        Configure = 0x400,
        ConfigureComponentsOnly = 0x10,
        CreateTargetApplication = 2,
        Default = 0,
        ExpectExistingTypeLib = 1,
        FindOrCreateTargetApplication = 4,
        Install = 0x200,
        ReconfigureExistingApplication = 8,
        Register = 0x100,
        ReportWarningsToConsole = 0x20
    }
}

