namespace System.Runtime.Serialization
{
    using System;

    internal class NonNegativeIntegerDataContract : LongDataContract
    {
        internal NonNegativeIntegerDataContract() : base(DictionaryGlobals.nonNegativeIntegerLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

