namespace System.Xml.Serialization
{
    using System;
    using System.Reflection;

    internal interface INameScope
    {
        object this[string name, string ns] { get; set; }
    }
}

