namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class TempAssemblyCache
    {
        private Hashtable cache = new Hashtable();

        internal void Add(string ns, object o, TempAssembly assembly)
        {
            TempAssemblyCacheKey key = new TempAssemblyCacheKey(ns, o);
            lock (this)
            {
                if (this.cache[key] != assembly)
                {
                    Hashtable hashtable = new Hashtable();
                    foreach (object obj2 in this.cache.Keys)
                    {
                        hashtable.Add(obj2, this.cache[obj2]);
                    }
                    this.cache = hashtable;
                    this.cache[key] = assembly;
                }
            }
        }

        internal TempAssembly this[string ns, object o]
        {
            get
            {
                return (TempAssembly) this.cache[new TempAssemblyCacheKey(ns, o)];
            }
        }
    }
}

