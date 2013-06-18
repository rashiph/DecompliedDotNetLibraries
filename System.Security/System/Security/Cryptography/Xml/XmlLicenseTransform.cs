namespace System.Security.Cryptography.Xml
{
    using System;
    using System.IO;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Xml;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class XmlLicenseTransform : Transform
    {
        private const string ElementIssuer = "issuer";
        private Type[] inputTypes = new Type[] { typeof(XmlDocument) };
        private XmlDocument license;
        private XmlNamespaceManager namespaceManager;
        private const string NamespaceUriCore = "urn:mpeg:mpeg21:2003:01-REL-R-NS";
        private Type[] outputTypes = new Type[] { typeof(XmlDocument) };
        private IRelDecryptor relDecryptor;

        public XmlLicenseTransform()
        {
            base.Algorithm = "urn:mpeg:mpeg21:2003:01-REL-R-NS:licenseTransform";
        }

        private void DecryptEncryptedGrants(XmlNodeList encryptedGrantList, IRelDecryptor decryptor)
        {
            XmlElement element = null;
            XmlElement element2 = null;
            XmlElement element3 = null;
            EncryptionMethod encryptionMethod = null;
            KeyInfo keyInfo = null;
            CipherData data = null;
            int num = 0;
            int count = encryptedGrantList.Count;
            while (num < count)
            {
                element = encryptedGrantList[num].SelectSingleNode("//r:encryptedGrant/enc:EncryptionMethod", this.namespaceManager) as XmlElement;
                element2 = encryptedGrantList[num].SelectSingleNode("//r:encryptedGrant/dsig:KeyInfo", this.namespaceManager) as XmlElement;
                element3 = encryptedGrantList[num].SelectSingleNode("//r:encryptedGrant/enc:CipherData", this.namespaceManager) as XmlElement;
                if (((element != null) && (element2 != null)) && (element3 != null))
                {
                    encryptionMethod = new EncryptionMethod();
                    keyInfo = new KeyInfo();
                    data = new CipherData();
                    encryptionMethod.LoadXml(element);
                    keyInfo.LoadXml(element2);
                    data.LoadXml(element3);
                    MemoryStream toDecrypt = null;
                    Stream stream = null;
                    StreamReader reader = null;
                    try
                    {
                        toDecrypt = new MemoryStream(data.CipherValue);
                        stream = this.relDecryptor.Decrypt(encryptionMethod, keyInfo, toDecrypt);
                        if ((stream == null) || (stream.Length == 0L))
                        {
                            throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_XrmlUnableToDecryptGrant"));
                        }
                        reader = new StreamReader(stream);
                        string str = reader.ReadToEnd();
                        encryptedGrantList[num].ParentNode.InnerXml = str;
                    }
                    finally
                    {
                        if (toDecrypt != null)
                        {
                            toDecrypt.Close();
                        }
                        if (stream != null)
                        {
                            stream.Close();
                        }
                        if (reader != null)
                        {
                            reader.Close();
                        }
                    }
                    encryptionMethod = null;
                    keyInfo = null;
                    data = null;
                }
                element = null;
                element2 = null;
                element3 = null;
                num++;
            }
        }

        protected override XmlNodeList GetInnerXml()
        {
            return null;
        }

        public override object GetOutput()
        {
            return this.license;
        }

        public override object GetOutput(Type type)
        {
            if ((type != typeof(XmlDocument)) || !type.IsSubclassOf(typeof(XmlDocument)))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Cryptography_Xml_TransformIncorrectInputType"), "type");
            }
            return this.GetOutput();
        }

        public override void LoadInnerXml(XmlNodeList nodeList)
        {
        }

        public override void LoadInput(object obj)
        {
            if (base.Context == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_XrmlMissingContext"));
            }
            this.license = new XmlDocument();
            this.license.PreserveWhitespace = true;
            this.namespaceManager = new XmlNamespaceManager(this.license.NameTable);
            this.namespaceManager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
            this.namespaceManager.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            this.namespaceManager.AddNamespace("r", "urn:mpeg:mpeg21:2003:01-REL-R-NS");
            XmlElement element = null;
            XmlElement element2 = null;
            XmlNode oldChild = null;
            element = base.Context.SelectSingleNode("ancestor-or-self::r:issuer[1]", this.namespaceManager) as XmlElement;
            if (element == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_XrmlMissingIssuer"));
            }
            oldChild = element.SelectSingleNode("descendant-or-self::dsig:Signature[1]", this.namespaceManager) as XmlElement;
            if (oldChild != null)
            {
                oldChild.ParentNode.RemoveChild(oldChild);
            }
            element2 = element.SelectSingleNode("ancestor-or-self::r:license[1]", this.namespaceManager) as XmlElement;
            if (element2 == null)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_XrmlMissingLicence"));
            }
            XmlNodeList list = element2.SelectNodes("descendant-or-self::r:license[1]/r:issuer", this.namespaceManager);
            int num = 0;
            int count = list.Count;
            while (num < count)
            {
                if (((list[num] != element) && (list[num].LocalName == "issuer")) && (list[num].NamespaceURI == "urn:mpeg:mpeg21:2003:01-REL-R-NS"))
                {
                    list[num].ParentNode.RemoveChild(list[num]);
                }
                num++;
            }
            XmlNodeList encryptedGrantList = element2.SelectNodes("/r:license/r:grant/r:encryptedGrant", this.namespaceManager);
            if (encryptedGrantList.Count > 0)
            {
                if (this.relDecryptor == null)
                {
                    throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_Xml_XrmlMissingIRelDecryptor"));
                }
                this.DecryptEncryptedGrants(encryptedGrantList, this.relDecryptor);
            }
            this.license.InnerXml = element2.OuterXml;
        }

        public IRelDecryptor Decryptor
        {
            get
            {
                return this.relDecryptor;
            }
            set
            {
                this.relDecryptor = value;
            }
        }

        public override Type[] InputTypes
        {
            get
            {
                return this.inputTypes;
            }
        }

        public override Type[] OutputTypes
        {
            get
            {
                return this.outputTypes;
            }
        }
    }
}

