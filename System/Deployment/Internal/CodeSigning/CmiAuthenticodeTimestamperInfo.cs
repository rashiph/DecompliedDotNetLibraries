namespace System.Deployment.Internal.CodeSigning
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    internal class CmiAuthenticodeTimestamperInfo
    {
        private uint m_algHash;
        private int m_error;
        private X509Chain m_timestamperChain;
        private DateTime m_timestampTime;

        private CmiAuthenticodeTimestamperInfo()
        {
        }

        internal CmiAuthenticodeTimestamperInfo(System.Deployment.Internal.CodeSigning.Win32.AXL_TIMESTAMPER_INFO timestamperInfo)
        {
            this.m_error = (int) timestamperInfo.dwError;
            this.m_algHash = timestamperInfo.algHash;
            long fileTime = (long) ((((ulong) timestamperInfo.ftTimestamp.dwHighDateTime) << 0x20) | ((ulong) timestamperInfo.ftTimestamp.dwLowDateTime));
            this.m_timestampTime = DateTime.FromFileTime(fileTime);
            if (timestamperInfo.pChainContext != IntPtr.Zero)
            {
                this.m_timestamperChain = new X509Chain(timestamperInfo.pChainContext);
            }
        }

        internal int ErrorCode
        {
            get
            {
                return this.m_error;
            }
        }

        internal uint HashAlgId
        {
            get
            {
                return this.m_algHash;
            }
        }

        internal X509Chain TimestamperChain
        {
            get
            {
                return this.m_timestamperChain;
            }
        }

        internal DateTime TimestampTime
        {
            get
            {
                return this.m_timestampTime;
            }
        }
    }
}

