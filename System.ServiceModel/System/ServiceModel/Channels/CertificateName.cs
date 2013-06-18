namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal sealed class CertificateName
    {
        private string dn;

        public CertificateName(string dn)
        {
            this.dn = dn;
        }

        [DllImport("Crypt32.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Auto, SetLastError=true)]
        private static extern bool CertStrToName(CertEncodingType dwCertEncodingType, [MarshalAs(UnmanagedType.LPTStr)] string pszX500, StringType dwStrType, IntPtr pvReserved, [In, Out] byte[] pbEncoded, [In, Out] ref int pcbEncoded, [MarshalAs(UnmanagedType.LPTStr)] ref StringBuilder ppszError);
        public CryptoApiBlob GetCryptoApiBlob()
        {
            return new CryptoApiBlob(this.GetEncodedName());
        }

        private byte[] GetEncodedName()
        {
            int pcbEncoded = 0;
            StringBuilder ppszError = null;
            CertStrToName(CertEncodingType.PKCS7AsnEncoding | CertEncodingType.X509AsnEncoding, this.DistinguishedName, StringType.ReverseFlag | StringType.OIDNameString, IntPtr.Zero, null, ref pcbEncoded, ref ppszError);
            byte[] pbEncoded = new byte[pcbEncoded];
            if (!CertStrToName(CertEncodingType.PKCS7AsnEncoding | CertEncodingType.X509AsnEncoding, this.DistinguishedName, StringType.ReverseFlag | StringType.OIDNameString, IntPtr.Zero, pbEncoded, ref pcbEncoded, ref ppszError))
            {
                PeerExceptionHelper.ThrowInvalidOperation_PeerCertGenFailure(PeerExceptionHelper.GetLastException());
            }
            return pbEncoded;
        }

        public string DistinguishedName
        {
            get
            {
                return this.dn;
            }
        }

        [Flags]
        private enum CertEncodingType
        {
            PKCS7AsnEncoding = 0x10000,
            X509AsnEncoding = 1
        }

        [Flags]
        private enum StringType
        {
            CommaFlag = 0x4000000,
            CRLFFlag = 0x8000000,
            DisableIE4UTF8Flag = 0x10000,
            EnableT61UnicodeFlag = 0x20000,
            EnableUTF8UnicodeFlag = 0x40000,
            NoPlusFlag = 0x20000000,
            NoQuotingFlag = 0x10000000,
            OIDNameString = 2,
            ReverseFlag = 0x2000000,
            SemicolonFlag = 0x40000000,
            SimpleNameString = 1,
            X500NameString = 3
        }
    }
}

