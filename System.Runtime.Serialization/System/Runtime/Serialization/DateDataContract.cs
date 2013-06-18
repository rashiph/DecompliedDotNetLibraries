namespace System.Runtime.Serialization
{
    using System;

    internal class DateDataContract : StringDataContract
    {
        internal DateDataContract() : base(DictionaryGlobals.dateLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

