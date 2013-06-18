namespace System.ServiceModel.Channels
{
    using System;
    using System.Net.Security;

    internal class SecurityCapabilities : ISecurityCapabilities
    {
        internal ProtectionLevel requestProtectionLevel;
        internal ProtectionLevel responseProtectionLevel;
        internal bool supportsClientAuth;
        internal bool supportsClientWindowsIdentity;
        internal bool supportsServerAuth;

        public SecurityCapabilities(bool supportsClientAuth, bool supportsServerAuth, bool supportsClientWindowsIdentity, ProtectionLevel requestProtectionLevel, ProtectionLevel responseProtectionLevel)
        {
            this.supportsClientAuth = supportsClientAuth;
            this.supportsServerAuth = supportsServerAuth;
            this.supportsClientWindowsIdentity = supportsClientWindowsIdentity;
            this.requestProtectionLevel = requestProtectionLevel;
            this.responseProtectionLevel = responseProtectionLevel;
        }

        internal static bool IsEqual(ISecurityCapabilities capabilities1, ISecurityCapabilities capabilities2)
        {
            if (capabilities1 == null)
            {
                capabilities1 = None;
            }
            if (capabilities2 == null)
            {
                capabilities2 = None;
            }
            if (capabilities1.SupportedRequestProtectionLevel != capabilities2.SupportedRequestProtectionLevel)
            {
                return false;
            }
            if (capabilities1.SupportedResponseProtectionLevel != capabilities2.SupportedResponseProtectionLevel)
            {
                return false;
            }
            if (capabilities1.SupportsClientAuthentication != capabilities2.SupportsClientAuthentication)
            {
                return false;
            }
            if (capabilities1.SupportsClientWindowsIdentity != capabilities2.SupportsClientWindowsIdentity)
            {
                return false;
            }
            if (capabilities1.SupportsServerAuthentication != capabilities2.SupportsServerAuthentication)
            {
                return false;
            }
            return true;
        }

        private static SecurityCapabilities None
        {
            get
            {
                return new SecurityCapabilities(false, false, false, ProtectionLevel.None, ProtectionLevel.None);
            }
        }

        public ProtectionLevel SupportedRequestProtectionLevel
        {
            get
            {
                return this.requestProtectionLevel;
            }
        }

        public ProtectionLevel SupportedResponseProtectionLevel
        {
            get
            {
                return this.responseProtectionLevel;
            }
        }

        public bool SupportsClientAuthentication
        {
            get
            {
                return this.supportsClientAuth;
            }
        }

        public bool SupportsClientWindowsIdentity
        {
            get
            {
                return this.supportsClientWindowsIdentity;
            }
        }

        public bool SupportsServerAuthentication
        {
            get
            {
                return this.supportsServerAuth;
            }
        }
    }
}

