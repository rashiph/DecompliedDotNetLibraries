namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum HostSecurityManagerOptions
    {
        AllFlags = 0x1f,
        HostAppDomainEvidence = 1,
        HostAssemblyEvidence = 4,
        HostDetermineApplicationTrust = 8,
        [Obsolete("AppDomain policy levels are obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        HostPolicyLevel = 2,
        HostResolvePolicy = 0x10,
        None = 0
    }
}

