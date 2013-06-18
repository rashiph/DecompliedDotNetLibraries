namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Class, Inherited=true, AllowMultiple=false)]
    public sealed class XamlSetMarkupExtensionAttribute : Attribute
    {
        public XamlSetMarkupExtensionAttribute(string xamlSetMarkupExtensionHandler)
        {
            this.XamlSetMarkupExtensionHandler = xamlSetMarkupExtensionHandler;
        }

        public string XamlSetMarkupExtensionHandler { get; private set; }
    }
}

