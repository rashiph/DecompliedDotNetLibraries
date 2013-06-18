namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;

    public interface ISecurityCapabilities
    {
        ProtectionLevel SupportedRequestProtectionLevel { get; }

        ProtectionLevel SupportedResponseProtectionLevel { get; }

        bool SupportsClientAuthentication { get; }

        bool SupportsClientWindowsIdentity { get; }

        bool SupportsServerAuthentication { get; }
    }
}

