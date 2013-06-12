namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections;

    public sealed class X509Certificate2Enumerator : IEnumerator
    {
        private IEnumerator baseEnumerator;

        private X509Certificate2Enumerator()
        {
        }

        internal X509Certificate2Enumerator(X509Certificate2Collection mappings)
        {
            this.baseEnumerator = mappings.GetEnumerator();
        }

        public bool MoveNext()
        {
            return this.baseEnumerator.MoveNext();
        }

        public void Reset()
        {
            this.baseEnumerator.Reset();
        }

        bool IEnumerator.MoveNext()
        {
            return this.baseEnumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            this.baseEnumerator.Reset();
        }

        public X509Certificate2 Current
        {
            get
            {
                return (X509Certificate2) this.baseEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.baseEnumerator.Current;
            }
        }
    }
}

