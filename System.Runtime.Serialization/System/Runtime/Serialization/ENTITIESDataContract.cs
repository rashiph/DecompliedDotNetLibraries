namespace System.Runtime.Serialization
{
    using System;

    internal class ENTITIESDataContract : StringDataContract
    {
        internal ENTITIESDataContract() : base(DictionaryGlobals.ENTITIESLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

