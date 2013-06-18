namespace System.Runtime.Serialization
{
    using System;

    internal class NonPositiveIntegerDataContract : LongDataContract
    {
        internal NonPositiveIntegerDataContract() : base(DictionaryGlobals.nonPositiveIntegerLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

