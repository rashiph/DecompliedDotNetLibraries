namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class EncryptedType
    {
        internal XmlElement m_cachedXml;
        private System.Security.Cryptography.Xml.CipherData m_cipherData;
        private string m_encoding;
        private System.Security.Cryptography.Xml.EncryptionMethod m_encryptionMethod;
        private string m_id;
        private System.Security.Cryptography.Xml.KeyInfo m_keyInfo;
        private string m_mimeType;
        private EncryptionPropertyCollection m_props;
        private string m_type;

        protected EncryptedType()
        {
        }

        public void AddProperty(EncryptionProperty ep)
        {
            this.EncryptionProperties.Add(ep);
        }

        public abstract XmlElement GetXml();
        public abstract void LoadXml(XmlElement value);

        internal bool CacheValid
        {
            get
            {
                return (this.m_cachedXml != null);
            }
        }

        public virtual System.Security.Cryptography.Xml.CipherData CipherData
        {
            get
            {
                if (this.m_cipherData == null)
                {
                    this.m_cipherData = new System.Security.Cryptography.Xml.CipherData();
                }
                return this.m_cipherData;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.m_cipherData = value;
                this.m_cachedXml = null;
            }
        }

        public virtual string Encoding
        {
            get
            {
                return this.m_encoding;
            }
            set
            {
                this.m_encoding = value;
                this.m_cachedXml = null;
            }
        }

        public virtual System.Security.Cryptography.Xml.EncryptionMethod EncryptionMethod
        {
            get
            {
                return this.m_encryptionMethod;
            }
            set
            {
                this.m_encryptionMethod = value;
                this.m_cachedXml = null;
            }
        }

        public virtual EncryptionPropertyCollection EncryptionProperties
        {
            get
            {
                if (this.m_props == null)
                {
                    this.m_props = new EncryptionPropertyCollection();
                }
                return this.m_props;
            }
        }

        public virtual string Id
        {
            get
            {
                return this.m_id;
            }
            set
            {
                this.m_id = value;
                this.m_cachedXml = null;
            }
        }

        public System.Security.Cryptography.Xml.KeyInfo KeyInfo
        {
            get
            {
                if (this.m_keyInfo == null)
                {
                    this.m_keyInfo = new System.Security.Cryptography.Xml.KeyInfo();
                }
                return this.m_keyInfo;
            }
            set
            {
                this.m_keyInfo = value;
            }
        }

        public virtual string MimeType
        {
            get
            {
                return this.m_mimeType;
            }
            set
            {
                this.m_mimeType = value;
                this.m_cachedXml = null;
            }
        }

        public virtual string Type
        {
            get
            {
                return this.m_type;
            }
            set
            {
                this.m_type = value;
                this.m_cachedXml = null;
            }
        }
    }
}

