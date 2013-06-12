namespace System.Collections.Specialized
{
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    internal class StringDictionaryWithComparer : StringDictionary
    {
        public StringDictionaryWithComparer() : this(StringComparer.OrdinalIgnoreCase)
        {
        }

        public StringDictionaryWithComparer(IEqualityComparer comparer)
        {
            base.ReplaceHashtable(new Hashtable(comparer));
        }

        public override void Add(string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            base.contents.Add(key, value);
        }

        public override bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return base.contents.ContainsKey(key);
        }

        public override void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            base.contents.Remove(key);
        }

        public override string this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                return (string) base.contents[key];
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                base.contents[key] = value;
            }
        }
    }
}

