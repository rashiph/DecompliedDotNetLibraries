namespace System.Runtime.Serialization
{
    using System;

    internal class GYearMonthDataContract : StringDataContract
    {
        internal GYearMonthDataContract() : base(DictionaryGlobals.gYearMonthLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

