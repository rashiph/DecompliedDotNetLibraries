namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true), TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public sealed class XmlnsPrefixAttribute : Attribute
    {
        private string _prefix;
        private string _xmlNamespace;

        public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
        {
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
            this._xmlNamespace = xmlNamespace;
            this._prefix = prefix;
        }

        public string Prefix
        {
            get
            {
                return this._prefix;
            }
        }

        public string XmlNamespace
        {
            get
            {
                return this._xmlNamespace;
            }
        }
    }
}

