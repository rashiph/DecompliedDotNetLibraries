namespace System.Runtime.Serialization
{
    using System;

    internal class NMTOKENSDataContract : StringDataContract
    {
        internal NMTOKENSDataContract() : base(DictionaryGlobals.NMTOKENSLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

