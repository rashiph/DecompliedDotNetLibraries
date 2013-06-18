namespace System.Runtime.Serialization
{
    using System;

    internal class IntegerDataContract : LongDataContract
    {
        internal IntegerDataContract() : base(DictionaryGlobals.integerLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

