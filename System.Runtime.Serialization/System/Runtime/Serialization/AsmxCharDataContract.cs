namespace System.Runtime.Serialization
{
    using System;

    internal class AsmxCharDataContract : CharDataContract
    {
        internal AsmxCharDataContract() : base(DictionaryGlobals.CharLocalName, DictionaryGlobals.AsmxTypesNamespace)
        {
        }
    }
}

