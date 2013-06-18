namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class Pkcs9DocumentDescription : Pkcs9AttributeObject
    {
        private bool m_decoded;
        private string m_documentDescription;

        public Pkcs9DocumentDescription() : base("1.3.6.1.4.1.311.88.2.2")
        {
        }

        public Pkcs9DocumentDescription(string documentDescription) : base("1.3.6.1.4.1.311.88.2.2", Encode(documentDescription))
        {
            this.m_documentDescription = documentDescription;
            this.m_decoded = true;
        }

        public Pkcs9DocumentDescription(byte[] encodedDocumentDescription) : base("1.3.6.1.4.1.311.88.2.2", encodedDocumentDescription)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        private void Decode()
        {
            this.m_documentDescription = PkcsUtils.DecodeOctetString(base.RawData);
            this.m_decoded = true;
        }

        private static byte[] Encode(string documentDescription)
        {
            if (string.IsNullOrEmpty(documentDescription))
            {
                throw new ArgumentNullException("documentDescription");
            }
            return PkcsUtils.EncodeOctetString(documentDescription);
        }

        public string DocumentDescription
        {
            get
            {
                if (!this.m_decoded && (base.RawData != null))
                {
                    this.Decode();
                }
                return this.m_documentDescription;
            }
        }
    }
}

