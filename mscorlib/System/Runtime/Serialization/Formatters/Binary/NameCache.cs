namespace System.Runtime.Serialization.Formatters.Binary
{
    using System;
    using System.Collections;

    internal sealed class NameCache
    {
        private static Hashtable ht = new Hashtable();
        private string name;

        internal object GetCachedValue(string name)
        {
            this.name = name;
            return ht[name];
        }

        internal void SetCachedValue(object value)
        {
            ht[this.name] = value;
        }
    }
}

