namespace System.Runtime.Serialization
{
    using System;

    internal class XsDurationDataContract : TimeSpanDataContract
    {
        internal XsDurationDataContract() : base(DictionaryGlobals.TimeSpanLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

