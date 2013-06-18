namespace System.Runtime.Serialization
{
    using System;

    internal class GYearDataContract : StringDataContract
    {
        internal GYearDataContract() : base(DictionaryGlobals.gYearLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

