namespace System.Runtime.Caching
{
    using System;

    internal class MemoryCacheKey
    {
        protected byte _bits;
        private int _hash;
        private string _key;

        internal MemoryCacheKey(string key)
        {
            this._key = key;
            this._hash = key.GetHashCode();
        }

        internal int Hash
        {
            get
            {
                return this._hash;
            }
        }

        internal string Key
        {
            get
            {
                return this._key;
            }
        }
    }
}

