namespace System.DirectoryServices.ActiveDirectory
{
    using System;

    [Flags]
    public enum ActiveDirectorySiteOptions
    {
        AutoInterSiteTopologyDisabled = 0x10,
        AutoMinimumHopDisabled = 4,
        AutoTopologyDisabled = 1,
        ForceKccWindows2003Behavior = 0x40,
        GroupMembershipCachingEnabled = 0x20,
        None = 0,
        RandomBridgeHeaderServerSelectionDisabled = 0x100,
        RedundantServerTopologyEnabled = 0x400,
        StaleServerDetectDisabled = 8,
        TopologyCleanupDisabled = 2,
        UseHashingForReplicationSchedule = 0x200,
        UseWindows2000IstgElection = 0x80
    }
}

