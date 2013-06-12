namespace System.Runtime.Remoting.Messaging
{
    using System;
    using System.Collections;

    internal class MessageDictionaryEnumerator : IDictionaryEnumerator, IEnumerator
    {
        private IDictionaryEnumerator _enumHash;
        private MessageDictionary _md;
        private int i = -1;

        public MessageDictionaryEnumerator(MessageDictionary md, IDictionary hashtable)
        {
            this._md = md;
            if (hashtable != null)
            {
                this._enumHash = hashtable.GetEnumerator();
            }
            else
            {
                this._enumHash = null;
            }
        }

        public bool MoveNext()
        {
            if (this.i == -2)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
            }
            this.i++;
            if (this.i < this._md._keys.Length)
            {
                return true;
            }
            if ((this._enumHash != null) && this._enumHash.MoveNext())
            {
                return true;
            }
            this.i = -2;
            return false;
        }

        public void Reset()
        {
            this.i = -1;
            if (this._enumHash != null)
            {
                this._enumHash.Reset();
            }
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
                if (this.i < 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                if (this.i < this._md._keys.Length)
                {
                    return this._md._keys[this.i];
                }
                return this._enumHash.Key;
            }
        }

        public object Value
        {
            get
            {
                if (this.i < 0)
                {
                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_InternalState"));
                }
                if (this.i < this._md._keys.Length)
                {
                    return this._md.GetMessageValue(this.i);
                }
                return this._enumHash.Value;
            }
        }
    }
}

