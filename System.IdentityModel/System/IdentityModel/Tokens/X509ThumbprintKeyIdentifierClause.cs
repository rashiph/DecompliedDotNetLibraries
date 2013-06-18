namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;
    using System.Security.Cryptography.X509Certificates;

    public class X509ThumbprintKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        public X509ThumbprintKeyIdentifierClause(X509Certificate2 certificate) : this(GetHash(certificate), false)
        {
        }

        public X509ThumbprintKeyIdentifierClause(byte[] thumbprint) : this(thumbprint, true)
        {
        }

        internal X509ThumbprintKeyIdentifierClause(byte[] thumbprint, bool cloneBuffer) : base(null, thumbprint, cloneBuffer)
        {
        }

        private static byte[] GetHash(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            return certificate.GetCertHash();
        }

        public byte[] GetX509Thumbprint()
        {
            return base.GetBuffer();
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                return false;
            }
            return base.Matches(GetHash(certificate));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509ThumbprintKeyIdentifierClause(Hash = 0x{0})", new object[] { base.ToHexString() });
        }
    }
}

