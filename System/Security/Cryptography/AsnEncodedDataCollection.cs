namespace System.Security.Cryptography
{
    using System;
    using System.Collections;
    using System.Reflection;

    public sealed class AsnEncodedDataCollection : ICollection, IEnumerable
    {
        private ArrayList m_list;
        private Oid m_oid;

        public AsnEncodedDataCollection()
        {
            this.m_list = new ArrayList();
            this.m_oid = null;
        }

        public AsnEncodedDataCollection(AsnEncodedData asnEncodedData) : this()
        {
            this.m_list.Add(asnEncodedData);
        }

        public int Add(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            if (this.m_oid != null)
            {
                string strA = this.m_oid.Value;
                string strB = asnEncodedData.Oid.Value;
                if ((strA != null) && (strB != null))
                {
                    if (string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        throw new CryptographicException(SR.GetString("Cryptography_Asn_MismatchedOidInCollection"));
                    }
                }
                else if ((strA != null) || (strB != null))
                {
                    throw new CryptographicException(SR.GetString("Cryptography_Asn_MismatchedOidInCollection"));
                }
            }
            return this.m_list.Add(asnEncodedData);
        }

        public void CopyTo(AsnEncodedData[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public AsnEncodedDataEnumerator GetEnumerator()
        {
            return new AsnEncodedDataEnumerator(this);
        }

        public void Remove(AsnEncodedData asnEncodedData)
        {
            if (asnEncodedData == null)
            {
                throw new ArgumentNullException("asnEncodedData");
            }
            this.m_list.Remove(asnEncodedData);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException(SR.GetString("Arg_RankMultiDimNotSupported"));
            }
            if ((index < 0) || (index >= array.Length))
            {
                throw new ArgumentOutOfRangeException("index", SR.GetString("ArgumentOutOfRange_Index"));
            }
            if ((index + this.Count) > array.Length)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            for (int i = 0; i < this.Count; i++)
            {
                array.SetValue(this[i], index);
                index++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new AsnEncodedDataEnumerator(this);
        }

        public int Count
        {
            get
            {
                return this.m_list.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public AsnEncodedData this[int index]
        {
            get
            {
                return (AsnEncodedData) this.m_list[index];
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
    }
}

