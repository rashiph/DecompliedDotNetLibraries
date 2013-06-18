namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    [Serializable]
    internal class ContextDictionary : IDictionary<string, string>, ICollection<KeyValuePair<string, string>>, IEnumerable<KeyValuePair<string, string>>, IEnumerable
    {
        private IDictionary<string, string> dictionaryStore;
        private static ContextDictionary empty;

        public ContextDictionary()
        {
            this.dictionaryStore = new Dictionary<string, string>();
        }

        public ContextDictionary(IDictionary<string, string> context)
        {
            this.dictionaryStore = new Dictionary<string, string>();
            if (context != null)
            {
                bool flag = context is ContextDictionary;
                foreach (KeyValuePair<string, string> pair in context)
                {
                    if (flag)
                    {
                        this.dictionaryStore.Add(pair);
                    }
                    else
                    {
                        this.Add(pair);
                    }
                }
            }
        }

        public void Add(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Key");
            }
            if (item.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Value");
            }
            ValidateKeyValueSpace(item.Key);
            this.dictionaryStore.Add(item);
        }

        public void Add(string key, string value)
        {
            if (value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
            }
            ValidateKeyValueSpace(key);
            this.dictionaryStore.Add(key, value);
        }

        public void Clear()
        {
            this.dictionaryStore.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Key");
            }
            if (item.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Value");
            }
            ValidateKeyValueSpace(item.Key);
            return this.dictionaryStore.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            ValidateKeyValueSpace(key);
            return this.dictionaryStore.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            this.dictionaryStore.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.dictionaryStore.GetEnumerator();
        }

        private static bool IsLetterOrDigit(char c)
        {
            if ((('A' > c) || (c > 'Z')) && (('a' > c) || (c > 'z')))
            {
                return (('0' <= c) && (c <= '9'));
            }
            return true;
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            if (item.Key == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Key");
            }
            if (item.Value == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("item.Value");
            }
            ValidateKeyValueSpace(item.Key);
            return this.dictionaryStore.Remove(item);
        }

        public bool Remove(string key)
        {
            ValidateKeyValueSpace(key);
            return this.dictionaryStore.Remove(key);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dictionaryStore.GetEnumerator();
        }

        public bool TryGetValue(string key, out string value)
        {
            ValidateKeyValueSpace(key);
            return this.dictionaryStore.TryGetValue(key, out value);
        }

        internal static bool TryValidateKeyValueSpace(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("key");
            }
            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i];
                if ((!IsLetterOrDigit(c) && (c != '-')) && ((c != '_') && (c != '.')))
                {
                    return false;
                }
            }
            return true;
        }

        private static void ValidateKeyValueSpace(string key)
        {
            if (!TryValidateKeyValueSpace(key))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("key", System.ServiceModel.SR.GetString("InvalidCookieContent", new object[] { key })));
            }
        }

        public int Count
        {
            get
            {
                return this.dictionaryStore.Count;
            }
        }

        internal static ContextDictionary Empty
        {
            get
            {
                if (empty == null)
                {
                    ContextDictionary dictionary = new ContextDictionary {
                        dictionaryStore = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(0))
                    };
                    empty = dictionary;
                }
                return empty;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return this.dictionaryStore.IsReadOnly;
            }
        }

        public string this[string key]
        {
            get
            {
                ValidateKeyValueSpace(key);
                return this.dictionaryStore[key];
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                ValidateKeyValueSpace(key);
                this.dictionaryStore[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return this.dictionaryStore.Keys;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                return this.dictionaryStore.Values;
            }
        }
    }
}

