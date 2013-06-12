namespace System.Security.Cryptography
{
    using System;
    using System.Collections;

    public sealed class OidEnumerator : IEnumerator
    {
        private int m_current;
        private OidCollection m_oids;

        private OidEnumerator()
        {
        }

        internal OidEnumerator(OidCollection oids)
        {
            this.m_oids = oids;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_oids.Count - 1))
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

        public Oid Current
        {
            get
            {
                return this.m_oids[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_oids[this.m_current];
            }
        }
    }
}

