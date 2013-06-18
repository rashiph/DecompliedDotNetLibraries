namespace System.Runtime.Serialization
{
    using System;

    internal class IDREFDataContract : StringDataContract
    {
        internal IDREFDataContract() : base(DictionaryGlobals.IDREFLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

