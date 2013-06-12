namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public class X509Extension : AsnEncodedData
    {
        private bool m_critical;

        protected X509Extension()
        {
        }

        internal X509Extension(IntPtr pExtension)
        {
            CAPIBase.CERT_EXTENSION cert_extension = (CAPIBase.CERT_EXTENSION) Marshal.PtrToStructure(pExtension, typeof(CAPIBase.CERT_EXTENSION));
            this.m_critical = cert_extension.fCritical;
            string pszObjId = cert_extension.pszObjId;
            base.m_oid = new Oid(pszObjId, System.Security.Cryptography.OidGroup.ExtensionOrAttribute, false);
            byte[] destination = new byte[cert_extension.Value.cbData];
            if (cert_extension.Value.pbData != IntPtr.Zero)
            {
                Marshal.Copy(cert_extension.Value.pbData, destination, 0, destination.Length);
            }
            base.m_rawData = destination;
        }

        internal X509Extension(string oid) : base(new Oid(oid, System.Security.Cryptography.OidGroup.ExtensionOrAttribute, false))
        {
        }

        public X509Extension(AsnEncodedData encodedExtension, bool critical) : this(encodedExtension.Oid, encodedExtension.RawData, critical)
        {
        }

        public X509Extension(Oid oid, byte[] rawData, bool critical) : base(oid, rawData)
        {
            if ((base.Oid == null) || (base.Oid.Value == null))
            {
                throw new ArgumentNullException("oid");
            }
            if (base.Oid.Value.Length == 0)
            {
                throw new ArgumentException(SR.GetString("Arg_EmptyOrNullString"), "oid.Value");
            }
            this.m_critical = critical;
        }

        public X509Extension(string oid, byte[] rawData, bool critical) : this(new Oid(oid, System.Security.Cryptography.OidGroup.ExtensionOrAttribute, true), rawData, critical)
        {
        }

        public override void CopyFrom(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            X509Extension extension = asnEncodedData as X509Extension;
            if (extension == null)
            {
                throw new ArgumentException(SR.GetString("Cryptography_X509_ExtensionMismatch"));
            }
            base.CopyFrom(asnEncodedData);
            this.m_critical = extension.Critical;
        }

        public bool Critical
        {
            get
            {
                return this.m_critical;
            }
            set
            {
                this.m_critical = value;
            }
        }
    }
}

