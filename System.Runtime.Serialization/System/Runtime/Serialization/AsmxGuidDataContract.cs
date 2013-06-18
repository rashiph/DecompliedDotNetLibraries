namespace System.Runtime.Serialization
{
    using System;

    internal class AsmxGuidDataContract : GuidDataContract
    {
        internal AsmxGuidDataContract() : base(DictionaryGlobals.GuidLocalName, DictionaryGlobals.AsmxTypesNamespace)
        {
        }
    }
}

