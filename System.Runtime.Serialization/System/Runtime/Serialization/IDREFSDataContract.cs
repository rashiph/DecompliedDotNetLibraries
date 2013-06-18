namespace System.Runtime.Serialization
{
    using System;

    internal class IDREFSDataContract : StringDataContract
    {
        internal IDREFSDataContract() : base(DictionaryGlobals.IDREFSLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

