namespace System.Collections
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{value}", Name="[{key}]", Type="")]
    internal class KeyValuePairs
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object key;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object value;

        public KeyValuePairs(object key, object value)
        {
            this.value = value;
            this.key = key;
        }

        public object Key
        {
            get
            {
                return this.key;
            }
        }

        public object Value
        {
            get
            {
                return this.value;
            }
        }
    }
}

