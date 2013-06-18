namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class Pkcs9DocumentName : Pkcs9AttributeObject
    {
        private bool m_decoded;
        private string m_documentName;

        public Pkcs9DocumentName() : base("1.3.6.1.4.1.311.88.2.1")
        {
        }

        public Pkcs9DocumentName(string documentName) : base("1.3.6.1.4.1.311.88.2.1", Encode(documentName))
        {
            this.m_documentName = documentName;
            this.m_decoded = true;
        }

        public Pkcs9DocumentName(byte[] encodedDocumentName) : base("1.3.6.1.4.1.311.88.2.1", encodedDocumentName)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        private void Decode()
        {
            this.m_documentName = PkcsUtils.DecodeOctetString(base.RawData);
            this.m_decoded = true;
        }

        private static byte[] Encode(string documentName)
        {
            if (string.IsNullOrEmpty(documentName))
            {
                throw new ArgumentNullException("documentName");
            }
            return PkcsUtils.EncodeOctetString(documentName);
        }

        public string DocumentName
        {
            get
            {
                if (!this.m_decoded && (base.RawData != null))
                {
                    this.Decode();
                }
                return this.m_documentName;
            }
        }
    }
}

