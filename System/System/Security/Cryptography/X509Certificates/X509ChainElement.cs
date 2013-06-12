namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    public class X509ChainElement
    {
        private X509Certificate2 m_certificate;
        private X509ChainStatus[] m_chainStatus;
        private string m_description;

        private X509ChainElement()
        {
        }

        internal unsafe X509ChainElement(IntPtr pChainElement)
        {
            CAPIBase.CERT_CHAIN_ELEMENT structure = new CAPIBase.CERT_CHAIN_ELEMENT(Marshal.SizeOf(typeof(CAPIBase.CERT_CHAIN_ELEMENT)));
            uint size = (uint) Marshal.ReadInt32(pChainElement);
            if (size > Marshal.SizeOf(structure))
            {
                size = (uint) Marshal.SizeOf(structure);
            }
            System.Security.Cryptography.X509Certificates.X509Utils.memcpy(pChainElement, new IntPtr((void*) &structure), size);
            this.m_certificate = new X509Certificate2(structure.pCertContext);
            if (structure.pwszExtendedErrorInfo == IntPtr.Zero)
            {
                this.m_description = string.Empty;
            }
            else
            {
                this.m_description = Marshal.PtrToStringUni(structure.pwszExtendedErrorInfo);
            }
            if (structure.dwErrorStatus == 0)
            {
                this.m_chainStatus = new X509ChainStatus[0];
            }
            else
            {
                this.m_chainStatus = X509Chain.GetChainStatusInformation(structure.dwErrorStatus);
            }
        }

        public X509Certificate2 Certificate
        {
            get
            {
                return this.m_certificate;
            }
        }

        public X509ChainStatus[] ChainElementStatus
        {
            get
            {
                return this.m_chainStatus;
            }
        }

        public string Information
        {
            get
            {
                return this.m_description;
            }
        }
    }
}

