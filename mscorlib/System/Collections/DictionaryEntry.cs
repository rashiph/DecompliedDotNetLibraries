namespace System.Collections
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential), ComVisible(true)]
    public struct DictionaryEntry
    {
        private object _key;
        private object _value;
        public DictionaryEntry(object key, object value)
        {
            this._key = key;
            this._value = value;
        }

        public object Key
        {
            get
            {
                return this._key;
            }
            set
            {
                this._key = value;
            }
        }
        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
    }
}

