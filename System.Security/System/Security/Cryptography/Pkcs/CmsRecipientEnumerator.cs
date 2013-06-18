namespace System.Security.Cryptography.Pkcs
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CmsRecipientEnumerator : IEnumerator
    {
        private int m_current;
        private CmsRecipientCollection m_recipients;

        private CmsRecipientEnumerator()
        {
        }

        internal CmsRecipientEnumerator(CmsRecipientCollection recipients)
        {
            this.m_recipients = recipients;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_recipients.Count - 1))
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

        public CmsRecipient Current
        {
            get
            {
                return this.m_recipients[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_recipients[this.m_current];
            }
        }
    }
}

