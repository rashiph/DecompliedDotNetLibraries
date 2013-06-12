namespace System.Web
{
    using System;
    using System.Collections;

    internal class HttpStaticObjectsEnumerator : IDictionaryEnumerator, IEnumerator
    {
        private IDictionaryEnumerator _enum;

        internal HttpStaticObjectsEnumerator(IDictionaryEnumerator e)
        {
            this._enum = e;
        }

        public bool MoveNext()
        {
            return this._enum.MoveNext();
        }

        public void Reset()
        {
            this._enum.Reset();
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
                return new DictionaryEntry(this._enum.Key, this.Value);
            }
        }

        public object Key
        {
            get
            {
                return this._enum.Key;
            }
        }

        public object Value
        {
            get
            {
                HttpStaticObjectsEntry entry = (HttpStaticObjectsEntry) this._enum.Value;
                if (entry == null)
                {
                    return null;
                }
                return entry.Instance;
            }
        }
    }
}

