namespace System.Runtime.Serialization
{
    using System;

    internal class TokenDataContract : StringDataContract
    {
        internal TokenDataContract() : base(DictionaryGlobals.tokenLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

