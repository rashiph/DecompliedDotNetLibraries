namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class RecipientInfoEnumerator : IEnumerator
    {
        private int m_current;
        private RecipientInfoCollection m_recipientInfos;

        private RecipientInfoEnumerator()
        {
        }

        internal RecipientInfoEnumerator(RecipientInfoCollection RecipientInfos)
        {
            this.m_recipientInfos = RecipientInfos;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_recipientInfos.Count - 1))
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

        public RecipientInfo Current
        {
            get
            {
                return this.m_recipientInfos[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_recipientInfos[this.m_current];
            }
        }
    }
}

