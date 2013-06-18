namespace Microsoft.JScript
{
    using System;

    internal sealed class HashtableEntry
    {
        internal uint hashCode;
        internal object key;
        internal HashtableEntry next;
        internal object value;

        internal HashtableEntry(object key, object value, uint hashCode, HashtableEntry next)
        {
            this.key = key;
            this.value = value;
            this.hashCode = hashCode;
            this.next = next;
        }
    }
}

