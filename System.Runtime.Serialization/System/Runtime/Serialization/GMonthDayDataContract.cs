namespace System.Runtime.Serialization
{
    using System;

    internal class GMonthDayDataContract : StringDataContract
    {
        internal GMonthDayDataContract() : base(DictionaryGlobals.gMonthDayLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

