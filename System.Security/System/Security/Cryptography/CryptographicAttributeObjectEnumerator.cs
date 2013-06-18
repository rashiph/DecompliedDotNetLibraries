namespace System.Security.Cryptography
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CryptographicAttributeObjectEnumerator : IEnumerator
    {
        private CryptographicAttributeObjectCollection m_attributes;
        private int m_current;

        private CryptographicAttributeObjectEnumerator()
        {
        }

        internal CryptographicAttributeObjectEnumerator(CryptographicAttributeObjectCollection attributes)
        {
            this.m_attributes = attributes;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_attributes.Count - 1))
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

        public CryptographicAttributeObject Current
        {
            get
            {
                return this.m_attributes[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_attributes[this.m_current];
            }
        }
    }
}

