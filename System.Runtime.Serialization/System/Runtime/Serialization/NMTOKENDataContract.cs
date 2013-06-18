namespace System.Runtime.Serialization
{
    using System;

    internal class NMTOKENDataContract : StringDataContract
    {
        internal NMTOKENDataContract() : base(DictionaryGlobals.NMTOKENLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

