namespace System.Security.Cryptography
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngUIPolicy
    {
        private string m_creationTitle;
        private string m_description;
        private string m_friendlyName;
        private CngUIProtectionLevels m_protectionLevel;
        private string m_useContext;

        public CngUIPolicy(CngUIProtectionLevels protectionLevel) : this(protectionLevel, null)
        {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName) : this(protectionLevel, friendlyName, null)
        {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description) : this(protectionLevel, friendlyName, description, null)
        {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description, string useContext) : this(protectionLevel, friendlyName, description, useContext, null)
        {
        }

        public CngUIPolicy(CngUIProtectionLevels protectionLevel, string friendlyName, string description, string useContext, string creationTitle)
        {
            this.m_creationTitle = creationTitle;
            this.m_description = description;
            this.m_friendlyName = friendlyName;
            this.m_protectionLevel = protectionLevel;
            this.m_useContext = useContext;
        }

        public string CreationTitle
        {
            get
            {
                return this.m_creationTitle;
            }
        }

        public string Description
        {
            get
            {
                return this.m_description;
            }
        }

        public string FriendlyName
        {
            get
            {
                return this.m_friendlyName;
            }
        }

        public CngUIProtectionLevels ProtectionLevel
        {
            get
            {
                return this.m_protectionLevel;
            }
        }

        public string UseContext
        {
            get
            {
                return this.m_useContext;
            }
        }
    }
}

