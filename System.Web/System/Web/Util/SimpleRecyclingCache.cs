namespace System.Web.Util
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class SimpleRecyclingCache
    {
        private static Hashtable _hashtable;
        private const int MAX_SIZE = 100;

        internal SimpleRecyclingCache()
        {
            this.CreateHashtable();
        }

        private void CreateHashtable()
        {
            _hashtable = new Hashtable(100, StringComparer.OrdinalIgnoreCase);
        }

        internal object this[object key]
        {
            get
            {
                return _hashtable[key];
            }
            set
            {
                lock (this)
                {
                    if (_hashtable.Count >= 100)
                    {
                        _hashtable.Clear();
                    }
                    _hashtable[key] = value;
                }
            }
        }
    }
}

