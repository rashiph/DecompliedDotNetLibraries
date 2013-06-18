namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class Pkcs9MessageDigest : Pkcs9AttributeObject
    {
        private bool m_decoded;
        private byte[] m_messageDigest;

        public Pkcs9MessageDigest() : base("1.2.840.113549.1.9.4")
        {
        }

        internal Pkcs9MessageDigest(byte[] encodedMessageDigest) : base("1.2.840.113549.1.9.4", encodedMessageDigest)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            base.CopyFrom(asnEncodedData);
            this.m_decoded = false;
        }

        private void Decode()
        {
            this.m_messageDigest = PkcsUtils.DecodeOctetBytes(base.RawData);
            this.m_decoded = true;
        }

        public byte[] MessageDigest
        {
            get
            {
                if (!this.m_decoded && (base.RawData != null))
                {
                    this.Decode();
                }
                return this.m_messageDigest;
            }
        }
    }
}

