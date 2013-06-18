namespace System.Runtime.Serialization
{
    using System;

    internal class TimeDataContract : StringDataContract
    {
        internal TimeDataContract() : base(DictionaryGlobals.timeLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

