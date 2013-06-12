namespace System.Security.Cryptography
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;

    public sealed class OidCollection : ICollection, IEnumerable
    {
        private ArrayList m_list = new ArrayList();

        public int Add(Oid oid)
        {
            return this.m_list.Add(oid);
        }

        public void CopyTo(Oid[] array, int index)
        {
            ((ICollection) this).CopyTo(array, index);
        }

        public OidEnumerator GetEnumerator()
        {
            return new OidEnumerator(this);
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
            return new OidEnumerator(this);
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

        public Oid this[int index]
        {
            get
            {
                return (this.m_list[index] as Oid);
            }
        }

        public Oid this[string oid]
        {
            get
            {
                string str = System.Security.Cryptography.X509Certificates.X509Utils.FindOidInfo(2, oid, System.Security.Cryptography.OidGroup.AllGroups);
                if (str == null)
                {
                    str = oid;
                }
                foreach (Oid oid2 in this.m_list)
                {
                    if (oid2.Value == str)
                    {
                        return oid2;
                    }
                }
                return null;
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

