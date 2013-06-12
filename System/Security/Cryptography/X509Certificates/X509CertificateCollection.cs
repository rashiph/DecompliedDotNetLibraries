namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    public class X509CertificateCollection : CollectionBase
    {
        public X509CertificateCollection()
        {
        }

        public X509CertificateCollection(X509CertificateCollection value)
        {
            this.AddRange(value);
        }

        public X509CertificateCollection(X509Certificate[] value)
        {
            this.AddRange(value);
        }

        public int Add(X509Certificate value)
        {
            return base.List.Add(value);
        }

        public void AddRange(X509Certificate[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Length; i++)
            {
                this.Add(value[i]);
            }
        }

        public void AddRange(X509CertificateCollection value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            for (int i = 0; i < value.Count; i++)
            {
                this.Add(value[i]);
            }
        }

        public bool Contains(X509Certificate value)
        {
            foreach (X509Certificate certificate in base.List)
            {
                if (certificate.Equals(value))
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(X509Certificate[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        public X509CertificateEnumerator GetEnumerator()
        {
            return new X509CertificateEnumerator(this);
        }

        public override int GetHashCode()
        {
            int num = 0;
            foreach (X509Certificate certificate in this)
            {
                num += certificate.GetHashCode();
            }
            return num;
        }

        public int IndexOf(X509Certificate value)
        {
            return base.List.IndexOf(value);
        }

        public void Insert(int index, X509Certificate value)
        {
            base.List.Insert(index, value);
        }

        public void Remove(X509Certificate value)
        {
            base.List.Remove(value);
        }

        public X509Certificate this[int index]
        {
            get
            {
                return (X509Certificate) base.List[index];
            }
            set
            {
                base.List[index] = value;
            }
        }

        public class X509CertificateEnumerator : IEnumerator
        {
            private IEnumerator baseEnumerator;
            private IEnumerable temp;

            public X509CertificateEnumerator(X509CertificateCollection mappings)
            {
                this.temp = mappings;
                this.baseEnumerator = this.temp.GetEnumerator();
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

            public X509Certificate Current
            {
                get
                {
                    return (X509Certificate) this.baseEnumerator.Current;
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
}

