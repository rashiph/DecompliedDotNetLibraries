namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class KeyInfoEncryptedKey : KeyInfoClause
    {
        private System.Security.Cryptography.Xml.EncryptedKey m_encryptedKey;

        public KeyInfoEncryptedKey()
        {
        }

        public KeyInfoEncryptedKey(System.Security.Cryptography.Xml.EncryptedKey encryptedKey)
        {
            this.m_encryptedKey = encryptedKey;
        }

        public override XmlElement GetXml()
        {
            if (this.m_encryptedKey == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "KeyInfoEncryptedKey");
            }
            return this.m_encryptedKey.GetXml();
        }

        internal override XmlElement GetXml(XmlDocument xmlDocument)
        {
            if (this.m_encryptedKey == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_InvalidElement"), "KeyInfoEncryptedKey");
            }
            return this.m_encryptedKey.GetXml(xmlDocument);
        }

        public override void LoadXml(XmlElement value)
        {
            this.m_encryptedKey = new System.Security.Cryptography.Xml.EncryptedKey();
            this.m_encryptedKey.LoadXml(value);
        }

        public System.Security.Cryptography.Xml.EncryptedKey EncryptedKey
        {
            get
            {
                return this.m_encryptedKey;
            }
            set
            {
                this.m_encryptedKey = value;
            }
        }
    }
}

