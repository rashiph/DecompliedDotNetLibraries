namespace System.Web.Services.Protocols
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class ClientTypeCache
    {
        private Hashtable cache = new Hashtable();

        internal void Add(Type key, object value)
        {
            lock (this)
            {
                if (this.cache[key] != value)
                {
                    Hashtable hashtable = new Hashtable();
                    foreach (object obj2 in this.cache.Keys)
                    {
                        hashtable.Add(obj2, this.cache[obj2]);
                    }
                    this.cache = hashtable;
                    this.cache[key] = value;
                }
            }
        }

        internal object this[Type key]
        {
            get
            {
                return this.cache[key];
            }
        }
    }
}

