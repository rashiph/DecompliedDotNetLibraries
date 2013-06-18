namespace System.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    internal class NameScope : INameScopeDictionary, INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private HybridDictionary _nameMap;

        public void Add(KeyValuePair<string, object> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ReferenceIsNull", new object[] { "item.Key" }), "item");
            }
            if (item.Value == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ReferenceIsNull", new object[] { "item.Value" }), "item");
            }
            this.Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            this.RegisterName(key, value);
        }

        public void Clear()
        {
            this._nameMap = null;
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            if (item.Key == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("ReferenceIsNull", new object[] { "item.Key" }), "item");
            }
            return this.ContainsKey(item.Key);
        }

        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return (this.FindName(key) != null);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (this._nameMap == null)
            {
                array = null;
            }
            else
            {
                foreach (DictionaryEntry entry in this._nameMap)
                {
                    array[arrayIndex++] = new KeyValuePair<string, object>((string) entry.Key, entry.Value);
                }
            }
        }

        public object FindName(string name)
        {
            if (((this._nameMap != null) && (name != null)) && !(name == string.Empty))
            {
                return this._nameMap[name];
            }
            return null;
        }

        private IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new Enumerator(this._nameMap);
        }

        public void RegisterName(string name, object scopedElement)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (scopedElement == null)
            {
                throw new ArgumentNullException("scopedElement");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NameScopeNameNotEmptyString"));
            }
            if (!NameValidationHelper.IsValidIdentifierName(name))
            {
                throw new ArgumentException(System.Xaml.SR.Get("NameScopeInvalidIdentifierName", new object[] { name }));
            }
            if (this._nameMap == null)
            {
                this._nameMap = new HybridDictionary();
                this._nameMap[name] = scopedElement;
            }
            else
            {
                object obj2 = this._nameMap[name];
                if (obj2 == null)
                {
                    this._nameMap[name] = scopedElement;
                }
                else if (scopedElement != obj2)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("NameScopeDuplicateNamesNotAllowed", new object[] { name }));
                }
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if (!this.Contains(item))
            {
                return false;
            }
            if (item.Value != this[item.Key])
            {
                return false;
            }
            return this.Remove(item.Key);
        }

        public bool Remove(string key)
        {
            if (!this.ContainsKey(key))
            {
                return false;
            }
            this.UnregisterName(key);
            return true;
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGetValue(string key, out object value)
        {
            if (!this.ContainsKey(key))
            {
                value = null;
                return false;
            }
            value = this.FindName(key);
            return true;
        }

        public void UnregisterName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NameScopeNameNotEmptyString"));
            }
            if ((this._nameMap == null) || (this._nameMap[name] == null))
            {
                throw new ArgumentException(System.Xaml.SR.Get("NameScopeNameNotFound", new object[] { name }));
            }
            this._nameMap.Remove(name);
        }

        public int Count
        {
            get
            {
                if (this._nameMap == null)
                {
                    return 0;
                }
                return this._nameMap.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public object this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                return this.FindName(key);
            }
            set
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.RegisterName(key, value);
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                if (this._nameMap == null)
                {
                    return null;
                }
                List<string> list = new List<string>();
                foreach (string str in this._nameMap.Keys)
                {
                    list.Add(str);
                }
                return list;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                if (this._nameMap == null)
                {
                    return null;
                }
                List<object> list = new List<object>();
                foreach (object obj2 in this._nameMap.Values)
                {
                    list.Add(obj2);
                }
                return list;
            }
        }

        private class Enumerator : IEnumerator<KeyValuePair<string, object>>, IDisposable, IEnumerator
        {
            private IDictionaryEnumerator _enumerator = null;

            public Enumerator(HybridDictionary nameMap)
            {
                if (nameMap != null)
                {
                    this._enumerator = nameMap.GetEnumerator();
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public bool MoveNext()
            {
                if (this._enumerator == null)
                {
                    return false;
                }
                return this._enumerator.MoveNext();
            }

            void IEnumerator.Reset()
            {
                if (this._enumerator != null)
                {
                    this._enumerator.Reset();
                }
            }

            public KeyValuePair<string, object> Current
            {
                get
                {
                    if (this._enumerator == null)
                    {
                        return new KeyValuePair<string, object>();
                    }
                    return new KeyValuePair<string, object>((string) this._enumerator.Key, this._enumerator.Value);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

