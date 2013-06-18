namespace System.Activities.Debugger
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xml;

    internal abstract class XamlNode : IXmlLineInfo
    {
        protected XamlNode()
        {
            this.LineNumber = 0;
            this.LinePosition = 0;
        }

        public bool HasLineInfo()
        {
            return ((this.LineNumber != -1) && (this.LinePosition != -1));
        }

        public Uri BaseUri { get; set; }

        public int LineNumber { get; set; }

        public int LinePosition { get; set; }

        public abstract XamlNodeType NodeType { get; }

        public string XmlLang { get; set; }
    }
}

