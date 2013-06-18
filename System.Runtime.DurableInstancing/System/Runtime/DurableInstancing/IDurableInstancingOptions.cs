namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Xml.Linq;

    internal interface IDurableInstancingOptions
    {
        void SetScopeName(XName scopeName);
    }
}

