namespace System.IdentityModel.Tokens
{
    using System;
    using System.Globalization;
    using System.IdentityModel;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    public class X509SubjectKeyIdentifierClause : BinaryKeyIdentifierClause
    {
        private const int SkiDataOffset = 2;
        private const string SubjectKeyIdentifierOid = "2.5.29.14";

        public X509SubjectKeyIdentifierClause(byte[] ski) : this(ski, true)
        {
        }

        internal X509SubjectKeyIdentifierClause(byte[] ski, bool cloneBuffer) : base(null, ski, cloneBuffer)
        {
        }

        public static bool CanCreateFrom(X509Certificate2 certificate)
        {
            return (null != GetSkiRawData(certificate));
        }

        private static byte[] GetSkiRawData(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            }
            X509SubjectKeyIdentifierExtension extension = certificate.Extensions["2.5.29.14"] as X509SubjectKeyIdentifierExtension;
            if (extension != null)
            {
                return extension.RawData;
            }
            return null;
        }

        public byte[] GetX509SubjectKeyIdentifier()
        {
            return base.GetBuffer();
        }

        public bool Matches(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                return false;
            }
            byte[] skiRawData = GetSkiRawData(certificate);
            return ((skiRawData != null) && base.Matches(skiRawData, 2));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "X509SubjectKeyIdentifierClause(SKI = 0x{0})", new object[] { base.ToHexString() });
        }

        public static bool TryCreateFrom(X509Certificate2 certificate, out X509SubjectKeyIdentifierClause keyIdentifierClause)
        {
            byte[] skiRawData = GetSkiRawData(certificate);
            keyIdentifierClause = null;
            if (skiRawData != null)
            {
                byte[] ski = System.IdentityModel.SecurityUtils.CloneBuffer(skiRawData, 2, skiRawData.Length - 2);
                keyIdentifierClause = new X509SubjectKeyIdentifierClause(ski, false);
            }
            return (keyIdentifierClause != null);
        }
    }
}

