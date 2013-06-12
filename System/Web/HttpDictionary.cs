namespace System.Web
{
    using System;
    using System.Collections.Specialized;

    internal class HttpDictionary : NameObjectCollectionBase
    {
        internal HttpDictionary() : base(Misc.CaseInsensitiveInvariantKeyComparer)
        {
        }

        internal string[] GetAllKeys()
        {
            return base.BaseGetAllKeys();
        }

        internal string GetKey(int index)
        {
            return base.BaseGetKey(index);
        }

        internal object GetValue(int index)
        {
            return base.BaseGet(index);
        }

        internal object GetValue(string key)
        {
            return base.BaseGet(key);
        }

        internal void SetValue(string key, object value)
        {
            base.BaseSet(key, value);
        }

        internal int Size
        {
            get
            {
                return this.Count;
            }
        }
    }
}

