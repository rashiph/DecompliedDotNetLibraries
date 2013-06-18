namespace System.Runtime.Serialization
{
    using System;

    internal class LanguageDataContract : StringDataContract
    {
        internal LanguageDataContract() : base(DictionaryGlobals.languageLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

