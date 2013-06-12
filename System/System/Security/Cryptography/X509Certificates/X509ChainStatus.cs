namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct X509ChainStatus
    {
        private X509ChainStatusFlags m_status;
        private string m_statusInformation;
        public X509ChainStatusFlags Status
        {
            get
            {
                return this.m_status;
            }
            set
            {
                this.m_status = value;
            }
        }
        public string StatusInformation
        {
            get
            {
                if (this.m_statusInformation == null)
                {
                    return string.Empty;
                }
                return this.m_statusInformation;
            }
            set
            {
                this.m_statusInformation = value;
            }
        }
    }
}

