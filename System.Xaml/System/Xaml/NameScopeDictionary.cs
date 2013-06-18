namespace System.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;
    using System.Xaml.MS.Impl;

    internal class NameScopeDictionary : INameScopeDictionary, INameScope, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private HybridDictionary _nameMap;
        private FrugalObjectList<string> _names;
        private INameScope _underlyingNameScope;

        public NameScopeDictionary()
        {
        }

        public NameScopeDictionary(INameScope underlyingNameScope)
        {
            if (underlyingNameScope == null)
            {
                throw new ArgumentNullException("underlyingNameScope");
            }
            this._names = new FrugalObjectList<string>();
            this._underlyingNameScope = underlyingNameScope;
        }

        public object FindName(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NameScopeNameNotEmptyString"));
            }
            if (this._underlyingNameScope != null)
            {
                return this._underlyingNameScope.FindName(name);
            }
            if (this._nameMap == null)
            {
                return null;
            }
            return this._nameMap[name];
        }

        private IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new Enumerator(this);
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
            if (this._underlyingNameScope != null)
            {
                this._names.Add(name);
                this._underlyingNameScope.RegisterName(name, scopedElement);
            }
            else if (this._nameMap == null)
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

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
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
            if (this._underlyingNameScope != null)
            {
                this._underlyingNameScope.UnregisterName(name);
                this._names.Remove(name);
            }
            else
            {
                if ((this._nameMap == null) || (this._nameMap[name] == null))
                {
                    throw new ArgumentException(System.Xaml.SR.Get("NameScopeNameNotFound", new object[] { name }));
                }
                this._nameMap.Remove(name);
            }
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal INameScope UnderlyingNameScope
        {
            get
            {
                return this._underlyingNameScope;
            }
        }

        private class Enumerator : IEnumerator<KeyValuePair<string, object>>, IDisposable, IEnumerator
        {
            private HybridDictionary _nameMap;
            private FrugalObjectList<string> _names;
            private INameScope _underlyingNameScope;
            private IDictionaryEnumerator dictionaryEnumerator;
            private int index;

            public Enumerator(NameScopeDictionary nameScopeDictionary)
            {
                this._nameMap = nameScopeDictionary._nameMap;
                this._underlyingNameScope = nameScopeDictionary._underlyingNameScope;
                this._names = nameScopeDictionary._names;
                if (this._underlyingNameScope != null)
                {
                    this.index = -1;
                }
                else if (this._nameMap != null)
                {
                    this.dictionaryEnumerator = this._nameMap.GetEnumerator();
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public bool MoveNext()
            {
                if (this._underlyingNameScope != null)
                {
                    if (this.index == (this._names.Count - 1))
                    {
                        return false;
                    }
                    this.index++;
                    return true;
                }
                return ((this._nameMap != null) && this.dictionaryEnumerator.MoveNext());
            }

            void IEnumerator.Reset()
            {
                if (this._underlyingNameScope != null)
                {
                    this.index = -1;
                }
                else
                {
                    this.dictionaryEnumerator.Reset();
                }
            }

            public KeyValuePair<string, object> Current
            {
                get
                {
                    if (this._underlyingNameScope != null)
                    {
                        string key = this._names[this.index];
                        return new KeyValuePair<string, object>(key, this._underlyingNameScope.FindName(key));
                    }
                    if (this._nameMap != null)
                    {
                        return new KeyValuePair<string, object>((string) this.dictionaryEnumerator.Key, this.dictionaryEnumerator.Value);
                    }
                    return new KeyValuePair<string, object>();
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

