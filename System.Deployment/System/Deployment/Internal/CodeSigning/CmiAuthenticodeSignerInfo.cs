namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    internal class CmiAuthenticodeSignerInfo
    {
        private uint m_algHash;
        private string m_description;
        private string m_descriptionUrl;
        private int m_error;
        private string m_hash;
        private X509Chain m_signerChain;
        private System.Deployment.Internal.CodeSigning.CmiAuthenticodeTimestamperInfo m_timestamperInfo;

        internal CmiAuthenticodeSignerInfo()
        {
        }

        internal CmiAuthenticodeSignerInfo(int errorCode)
        {
            this.m_error = errorCode;
        }

        internal CmiAuthenticodeSignerInfo(System.Deployment.Internal.CodeSigning.Win32.AXL_SIGNER_INFO signerInfo, System.Deployment.Internal.CodeSigning.Win32.AXL_TIMESTAMPER_INFO timestamperInfo)
        {
            this.m_error = (int) signerInfo.dwError;
            if (signerInfo.pChainContext != IntPtr.Zero)
            {
                this.m_signerChain = new X509Chain(signerInfo.pChainContext);
            }
            this.m_algHash = signerInfo.algHash;
            if (signerInfo.pwszHash != IntPtr.Zero)
            {
                this.m_hash = Marshal.PtrToStringUni(signerInfo.pwszHash);
            }
            if (signerInfo.pwszDescription != IntPtr.Zero)
            {
                this.m_description = Marshal.PtrToStringUni(signerInfo.pwszDescription);
            }
            if (signerInfo.pwszDescriptionUrl != IntPtr.Zero)
            {
                this.m_descriptionUrl = Marshal.PtrToStringUni(signerInfo.pwszDescriptionUrl);
            }
            if (timestamperInfo.dwError != 0x800b0100)
            {
                this.m_timestamperInfo = new System.Deployment.Internal.CodeSigning.CmiAuthenticodeTimestamperInfo(timestamperInfo);
            }
        }

        internal string Description
        {
            get
            {
                return this.m_description;
            }
        }

        internal string DescriptionUrl
        {
            get
            {
                return this.m_descriptionUrl;
            }
        }

        internal int ErrorCode
        {
            get
            {
                return this.m_error;
            }
        }

        internal string Hash
        {
            get
            {
                return this.m_hash;
            }
        }

        internal uint HashAlgId
        {
            get
            {
                return this.m_algHash;
            }
        }

        internal X509Chain SignerChain
        {
            get
            {
                return this.m_signerChain;
            }
        }

        internal System.Deployment.Internal.CodeSigning.CmiAuthenticodeTimestamperInfo TimestamperInfo
        {
            get
            {
                return this.m_timestamperInfo;
            }
        }
    }
}

