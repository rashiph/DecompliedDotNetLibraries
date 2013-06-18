namespace System.Runtime.Serialization
{
    using System;

    internal class NameDataContract : StringDataContract
    {
        internal NameDataContract() : base(DictionaryGlobals.NameLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

