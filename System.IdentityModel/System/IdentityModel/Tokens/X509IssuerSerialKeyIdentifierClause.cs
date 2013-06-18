namespace System.IdentityModel.Tokens
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;

    public class X509IssuerSerialKeyIdentifierClause : SecurityKeyIdentifierClause
    {
        private readonly string issuerName;
        private readonly string issuerSerialNumber;

        public X509IssuerSerialKeyIdentifierClause(X509Certificate2 certificate) : base(null)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            this.issuerName = certificate.Issuer;
            this.issuerSerialNumber = Asn1IntegerConverter.Asn1IntegerToDecimalString(certificate.GetSerialNumber());
        }

        public X509IssuerSerialKeyIdentifierClause(string issuerName, string issuerSerialNumber) : base(null)
        {
            if (string.IsNullOrEmpty(issuerName))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerName");
            }
            if (string.IsNullOrEmpty(issuerSerialNumber))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuerSerialNumber");
            }
            this.issuerName = issuerName;
            this.issuerSerialNumber = issuerSerialNumber;
        }

        public override bool Matches(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            X509IssuerSerialKeyIdentifierClause objB = keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
            return (object.ReferenceEquals(this, objB) || ((objB != null) && objB.Matches(this.issuerName, this.issuerSerialNumber)));
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                return false;
            }
            return this.Matches(certificate.Issuer, Asn1IntegerConverter.Asn1IntegerToDecimalString(certificate.GetSerialNumber()));
        }

        public bool Matches(string issuerName, string issuerSerialNumber)
        {
            if (issuerName == null)
            {
                return false;
            }
            if (this.issuerSerialNumber != issuerSerialNumber)
            {
                return false;
            }
            if (this.issuerName == issuerName)
            {
                return true;
            }
            bool flag = false;
            try
            {
                if (CryptoHelper.IsEqual(new X500DistinguishedName(this.issuerName).RawData, new X500DistinguishedName(issuerName).RawData))
                {
                    flag = true;
                }
            }
            catch (CryptographicException exception)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
            }
            return flag;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509IssuerSerialKeyIdentifierClause(Issuer = '{0}', Serial = '{1}')", new object[] { this.IssuerName, this.IssuerSerialNumber });
        }

        public string IssuerName
        {
            get
            {
                return this.issuerName;
            }
        }

        public string IssuerSerialNumber
        {
            get
            {
                return this.issuerSerialNumber;
            }
        }
    }
}

