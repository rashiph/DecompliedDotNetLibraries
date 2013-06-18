namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    public enum ForestTrustDomainStatus
    {
        Enabled = 0,
        NetBiosNameAdminDisabled = 4,
        NetBiosNameConflictDisabled = 8,
        SidAdminDisabled = 1,
        SidConflictDisabled = 2
    }
}

