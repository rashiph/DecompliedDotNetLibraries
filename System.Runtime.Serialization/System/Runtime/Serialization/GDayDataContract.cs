namespace System.Runtime.Serialization
{
    using System;

    internal class GDayDataContract : StringDataContract
    {
        internal GDayDataContract() : base(DictionaryGlobals.gDayLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

