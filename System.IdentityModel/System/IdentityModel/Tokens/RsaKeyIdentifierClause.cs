namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;
    using System.Security.Cryptography;
    using System.Xml;

    public class RsaKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private static string clauseType = "http://www.w3.org/2000/09/xmldsig#RSAKeyValue";
        private readonly RSA rsa;
        private readonly RSAParameters rsaParameters;
        private RsaSecurityKey rsaSecurityKey;

        public RsaKeyIdentifierClause(RSA rsa) : base(clauseType)
        {
            if (rsa == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            }
            this.rsa = rsa;
            this.rsaParameters = rsa.ExportParameters(false);
        }

        public override SecurityKey CreateKey()
        {
            if (this.rsaSecurityKey == null)
            {
                this.rsaSecurityKey = new RsaSecurityKey(this.rsa);
            }
            return this.rsaSecurityKey;
        }

        public byte[] GetExponent()
        {
            return System.IdentityModel.SecurityUtils.CloneBuffer(this.rsaParameters.Exponent);
        }

        public byte[] GetModulus()
        {
            return System.IdentityModel.SecurityUtils.CloneBuffer(this.rsaParameters.Modulus);
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            RsaKeyIdentifierClause objB = keyIdentifierClause as RsaKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.rsa)));
        }

        public bool Matches(RSA rsa)
        {
            if (rsa == null)
            {
                return false;
            }
            RSAParameters parameters = rsa.ExportParameters(false);
            return (System.IdentityModel.SecurityUtils.MatchesBuffer(this.rsaParameters.Modulus, parameters.Modulus) && System.IdentityModel.SecurityUtils.MatchesBuffer(this.rsaParameters.Exponent, parameters.Exponent));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "RsaKeyIdentifierClause(Modulus = {0}, Exponent = {1})", new object[] { Convert.ToBase64String(this.rsaParameters.Modulus), Convert.ToBase64String(this.rsaParameters.Exponent) });
        }

        public void WriteExponentAsBase64(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteBase64(this.rsaParameters.Exponent, 0, this.rsaParameters.Exponent.Length);
        }

        public void WriteModulusAsBase64(XmlWriter writer)
        {
            if (writer == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("writer");
            }
            writer.WriteBase64(this.rsaParameters.Modulus, 0, this.rsaParameters.Modulus.Length);
        }

        public override bool CanCreateKey
        {
            get
            {
                return true;
            }
        }

        public RSA Rsa
        {
            get
            {
                return this.rsa;
            }
        }
    }
}

