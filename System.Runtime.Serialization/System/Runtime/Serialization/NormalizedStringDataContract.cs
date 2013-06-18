namespace System.Runtime.Serialization
{
    using System;

    internal class NormalizedStringDataContract : StringDataContract
    {
        internal NormalizedStringDataContract() : base(DictionaryGlobals.normalizedStringLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

