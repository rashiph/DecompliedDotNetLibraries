namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class Pkcs9ContentType : Pkcs9AttributeObject
    {
        private Oid m_contentType;
        private bool m_decoded;

        public Pkcs9ContentType() : base("1.2.840.113549.1.9.3")
        {
        }

        internal Pkcs9ContentType(byte[] encodedContentType) : base("1.2.840.113549.1.9.3", encodedContentType)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        private void Decode()
        {
            if ((base.RawData.Length < 2) || (base.RawData[1] != (base.RawData.Length - 2)))
            {
                throw new CryptographicException(-2146885630);
            }
            if (base.RawData[0] != 6)
            {
                throw new CryptographicException(-2146881269);
            }
            this.m_contentType = new Oid(PkcsUtils.DecodeObjectIdentifier(base.RawData, 2));
            this.m_decoded = true;
        }

        public Oid ContentType
        {
            get
            {
                if (!this.m_decoded && (base.RawData != null))
                {
                    this.Decode();
                }
                return this.m_contentType;
            }
        }
    }
}

