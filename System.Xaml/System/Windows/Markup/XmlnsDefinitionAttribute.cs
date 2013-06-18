namespace System.Windows.Markup
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35"), AttributeUsage(AttributeTargets.Assembly, AllowMultiple=true)]
    public sealed class XmlnsDefinitionAttribute : Attribute
    {
        private string _assemblyName;
        private string _clrNamespace;
        private string _xmlNamespace;

        public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
        {
            if (xmlNamespace == null)
            {
                throw new ArgumentNullException("xmlNamespace");
            }
            if (clrNamespace == null)
            {
                throw new ArgumentNullException("clrNamespace");
            }
            this._xmlNamespace = xmlNamespace;
            this._clrNamespace = clrNamespace;
        }

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
            }
            set
            {
                this._assemblyName = value;
            }
        }

        public string ClrNamespace
        {
            get
            {
                return this._clrNamespace;
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

