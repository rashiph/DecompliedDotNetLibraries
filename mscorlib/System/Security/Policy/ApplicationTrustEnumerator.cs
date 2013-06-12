namespace System.Security.Policy
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public sealed class ApplicationTrustEnumerator : IEnumerator
    {
        private int m_current;
        [SecurityCritical]
        private ApplicationTrustCollection m_trusts;

        private ApplicationTrustEnumerator()
        {
        }

        [SecurityCritical]
        internal ApplicationTrustEnumerator(ApplicationTrustCollection trusts)
        {
            this.m_trusts = trusts;
            this.m_current = -1;
        }

        [SecuritySafeCritical]
        public bool MoveNext()
        {
            if (this.m_current == (this.m_trusts.Count - 1))
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

        public ApplicationTrust Current
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_trusts[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            [SecuritySafeCritical]
            get
            {
                return this.m_trusts[this.m_current];
            }
        }
    }
}

