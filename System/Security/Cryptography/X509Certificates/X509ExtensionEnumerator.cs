namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections;

    public sealed class X509ExtensionEnumerator : IEnumerator
    {
        private int m_current;
        private X509ExtensionCollection m_extensions;

        private X509ExtensionEnumerator()
        {
        }

        internal X509ExtensionEnumerator(X509ExtensionCollection extensions)
        {
            this.m_extensions = extensions;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_extensions.Count - 1))
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

        public X509Extension Current
        {
            get
            {
                return this.m_extensions[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_extensions[this.m_current];
            }
        }
    }
}

