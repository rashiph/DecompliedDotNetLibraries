namespace System.Runtime.Serialization
{
    using System;

    internal class NCNameDataContract : StringDataContract
    {
        internal NCNameDataContract() : base(DictionaryGlobals.NCNameLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

