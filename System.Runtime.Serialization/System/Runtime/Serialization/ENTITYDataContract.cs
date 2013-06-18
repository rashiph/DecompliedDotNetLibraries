namespace System.Runtime.Serialization
{
    using System;

    internal class ENTITYDataContract : StringDataContract
    {
        internal ENTITYDataContract() : base(DictionaryGlobals.ENTITYLocalName, DictionaryGlobals.SchemaNamespace)
        {
        }
    }
}

