namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Deployment.Internal.Isolation.Manifest;

    internal class HashCollection : IEnumerable
    {
        protected ArrayList _hashes = new ArrayList();

        public void AddHash(byte[] digestValue, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_DIGESTMETHOD digestMethod, System.Deployment.Internal.Isolation.Manifest.CMS_HASH_TRANSFORM transform)
        {
            Hash hash = new Hash(digestValue, digestMethod, transform);
            this._hashes.Add(hash);
        }

        public HashEnumerator GetEnumerator()
        {
            return new HashEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._hashes.Count;
            }
        }

        public class HashEnumerator : IEnumerator
        {
            private HashCollection _hashCollection;
            private int _index;

            public HashEnumerator(HashCollection hashCollection)
            {
                this._hashCollection = hashCollection;
                this._index = -1;
            }

            public bool MoveNext()
            {
                this._index++;
                return (this._index < this._hashCollection._hashes.Count);
            }

            public void Reset()
            {
                this._index = -1;
            }

            public Hash Current
            {
                get
                {
                    return (Hash) this._hashCollection._hashes[this._index];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this._hashCollection._hashes[this._index];
                }
            }
        }
    }
}

