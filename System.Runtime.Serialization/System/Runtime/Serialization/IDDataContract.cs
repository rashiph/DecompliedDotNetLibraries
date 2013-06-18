namespace System.Runtime.Serialization
{
    using System;

    internal class IDDataContract : StringDataContract
    {
        internal IDDataContract() : base(DictionaryGlobals.XSDIDLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

