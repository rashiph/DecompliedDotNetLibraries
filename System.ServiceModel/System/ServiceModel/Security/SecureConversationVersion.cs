namespace System.ServiceModel.Security
{
    using System;
    using System.Xml;

    public abstract class SecureConversationVersion
    {
        private readonly XmlDictionaryString prefix;
        private readonly XmlDictionaryString scNamespace;

        internal SecureConversationVersion(XmlDictionaryString ns, XmlDictionaryString prefix)
        {
            this.scNamespace = ns;
            this.prefix = prefix;
        }

        public static SecureConversationVersion Default
        {
            get
            {
                return WSSecureConversationFeb2005;
            }
        }

        public XmlDictionaryString Namespace
        {
            get
            {
                return this.scNamespace;
            }
        }

        public XmlDictionaryString Prefix
        {
            get
            {
                return this.prefix;
            }
        }

        public static SecureConversationVersion WSSecureConversation13
        {
            get
            {
                return WSSecureConversationVersion13.Instance;
            }
        }

        public static SecureConversationVersion WSSecureConversationFeb2005
        {
            get
            {
                return WSSecureConversationVersionFeb2005.Instance;
            }
        }

        private class WSSecureConversationVersion13 : SecureConversationVersion
        {
            private static readonly SecureConversationVersion.WSSecureConversationVersion13 instance = new SecureConversationVersion.WSSecureConversationVersion13();

            protected WSSecureConversationVersion13() : base(DXD.SecureConversationDec2005Dictionary.Namespace, DXD.SecureConversationDec2005Dictionary.Prefix)
            {
            }

            public static SecureConversationVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }

        private class WSSecureConversationVersionFeb2005 : SecureConversationVersion
        {
            private static readonly SecureConversationVersion.WSSecureConversationVersionFeb2005 instance = new SecureConversationVersion.WSSecureConversationVersionFeb2005();

            protected WSSecureConversationVersionFeb2005() : base(XD.SecureConversationFeb2005Dictionary.Namespace, XD.SecureConversationFeb2005Dictionary.Prefix)
            {
            }

            public static SecureConversationVersion Instance
            {
                get
                {
                    return instance;
                }
            }
        }
    }
}

