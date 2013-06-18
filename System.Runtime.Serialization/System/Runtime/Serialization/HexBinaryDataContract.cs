namespace System.Runtime.Serialization
{
    using System;

    internal class HexBinaryDataContract : StringDataContract
    {
        internal HexBinaryDataContract() : base(DictionaryGlobals.hexBinaryLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

