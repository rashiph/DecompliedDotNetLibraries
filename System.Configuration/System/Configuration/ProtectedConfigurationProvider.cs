namespace System.Configuration
{
    using System;
    using System.Configuration.Provider;
    using System.Runtime;
    using System.Xml;

    public abstract class ProtectedConfigurationProvider : ProviderBase
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProtectedConfigurationProvider()
        {
        }

        public abstract XmlNode Decrypt(XmlNode encryptedNode);
        public abstract XmlNode Encrypt(XmlNode node);
    }
}

