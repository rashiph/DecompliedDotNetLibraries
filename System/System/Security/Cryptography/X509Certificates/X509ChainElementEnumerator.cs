namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections;

    public sealed class X509ChainElementEnumerator : IEnumerator
    {
        private X509ChainElementCollection m_chainElements;
        private int m_current;

        private X509ChainElementEnumerator()
        {
        }

        internal X509ChainElementEnumerator(X509ChainElementCollection chainElements)
        {
            this.m_chainElements = chainElements;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_chainElements.Count - 1))
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

        public X509ChainElement Current
        {
            get
            {
                return this.m_chainElements[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_chainElements[this.m_current];
            }
        }
    }
}

