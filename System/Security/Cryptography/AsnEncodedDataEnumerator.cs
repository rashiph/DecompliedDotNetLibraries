namespace System.Security.Cryptography
{
    using System;
    using System.Collections;

    public sealed class AsnEncodedDataEnumerator : IEnumerator
    {
        private AsnEncodedDataCollection m_asnEncodedDatas;
        private int m_current;

        private AsnEncodedDataEnumerator()
        {
        }

        internal AsnEncodedDataEnumerator(AsnEncodedDataCollection asnEncodedDatas)
        {
            this.m_asnEncodedDatas = asnEncodedDatas;
            this.m_current = -1;
        }

        public bool MoveNext()
        {
            if (this.m_current == (this.m_asnEncodedDatas.Count - 1))
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

        public AsnEncodedData Current
        {
            get
            {
                return this.m_asnEncodedDatas[this.m_current];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.m_asnEncodedDatas[this.m_current];
            }
        }
    }
}

