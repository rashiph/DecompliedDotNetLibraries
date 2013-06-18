namespace System.Runtime.Serialization
{
    using System;

    internal class NegativeIntegerDataContract : LongDataContract
    {
        internal NegativeIntegerDataContract() : base(DictionaryGlobals.negativeIntegerLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

