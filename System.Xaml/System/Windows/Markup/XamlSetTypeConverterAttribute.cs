namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
    public sealed class XamlSetTypeConverterAttribute : Attribute
    {
        public XamlSetTypeConverterAttribute(string xamlSetTypeConverterHandler)
        {
            this.XamlSetTypeConverterHandler = xamlSetTypeConverterHandler;
        }

        public string XamlSetTypeConverterHandler { get; private set; }
    }
}

