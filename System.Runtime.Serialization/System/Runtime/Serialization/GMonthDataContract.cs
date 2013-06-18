namespace System.Runtime.Serialization
{
    using System;

    internal class GMonthDataContract : StringDataContract
    {
        internal GMonthDataContract() : base(DictionaryGlobals.gMonthLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

