namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class SignerInfoEnumerator : IEnumerator
    {
        private int m_current;
        private SignerInfoCollection m_signerInfos;

        private SignerInfoEnumerator()
        {
        }

        internal SignerInfoEnumerator(SignerInfoCollection signerInfos)
        {
            this.m_signerInfos = signerInfos;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_signerInfos.Count - 1))
            {
                return false;
            }
            this.m_current++;
            return true;
        }

        public void Reset()
        {
            this.m_current = -1;
        }

        public SignerInfo Current
        {
            get
            {
                return this.m_signerInfos[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_signerInfos[this.m_current];
            }
        }
    }
}

