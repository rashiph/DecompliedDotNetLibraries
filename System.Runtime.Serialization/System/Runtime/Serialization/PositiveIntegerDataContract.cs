namespace System.Runtime.Serialization
{
    using System;

    internal class PositiveIntegerDataContract : LongDataContract
    {
        internal PositiveIntegerDataContract() : base(DictionaryGlobals.positiveIntegerLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

