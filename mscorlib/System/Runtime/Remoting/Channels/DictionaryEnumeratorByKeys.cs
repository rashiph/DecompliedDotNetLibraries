namespace System.Runtime.Remoting.Channels
{
    using System;
    using System.Collections;

    internal class DictionaryEnumeratorByKeys : IDictionaryEnumerator, IEnumerator
    {
        private IEnumerator _keyEnum;
        private IDictionary _properties;

        public DictionaryEnumeratorByKeys(IDictionary properties)
        {
            this._properties = properties;
            this._keyEnum = properties.Keys.GetEnumerator();
        }

        public bool MoveNext()
        {
            return this._keyEnum.MoveNext();
        }

        public void Reset()
        {
            this._keyEnum.Reset();
        }

        public object Current
        {
            get
            {
                return this.Entry;
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                return new DictionaryEntry(this.Key, this.Value);
            }
        }

        public object Key
        {
            get
            {
                return this._keyEnum.Current;
            }
        }

        public object Value
        {
            get
            {
                return this._properties[this.Key];
            }
        }
    }
}

