namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;
    using System.Security.Cryptography.X509Certificates;

    public class X509RawDataKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        private X509Certificate2 certificate;
        private X509AsymmetricSecurityKey key;

        public X509RawDataKeyIdentifierClause(X509Certificate2 certificate) : this(GetRawData(certificate), false)
        {
            this.certificate = certificate;
        }

        public X509RawDataKeyIdentifierClause(byte[] certificateRawData) : this(certificateRawData, true)
        {
        }

        internal X509RawDataKeyIdentifierClause(byte[] certificateRawData, bool cloneBuffer) : base(null, certificateRawData, cloneBuffer)
        {
        }

        public override SecurityKey CreateKey()
        {
            if (this.key == null)
            {
                if (this.certificate == null)
                {
                    this.certificate = new X509Certificate2(base.GetBuffer());
                }
                this.key = new X509AsymmetricSecurityKey(this.certificate);
            }
            return this.key;
        }

        private static byte[] GetRawData(X509Certificate certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            return certificate.GetRawCertData();
        }

        public byte[] GetX509RawData()
        {
            return base.GetBuffer();
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                return false;
            }
            return base.Matches(GetRawData(certificate));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509RawDataKeyIdentifierClause(RawData = {0})", new object[] { base.ToBase64String() });
        }

        public override bool CanCreateKey
        {
            get
            {
                return true;
            }
        }
    }
}

