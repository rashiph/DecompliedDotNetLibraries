namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xml;

    [DebuggerDisplay("{_nodeType}")]
    internal class XamlScannerNode
    {
        public XamlScannerNode(XamlAttribute attr)
        {
            this.LineNumber = attr.LineNumber;
            this.LinePosition = attr.LinePosition;
        }

        public XamlScannerNode(IXmlLineInfo lineInfo)
        {
            if (lineInfo != null)
            {
                this.LineNumber = lineInfo.LineNumber;
                this.LinePosition = lineInfo.LinePosition;
            }
        }

        public bool IsCtorForcingMember { get; set; }

        public bool IsEmptyTag { get; set; }

        public bool IsTextXML { get; set; }

        public int LineNumber { get; private set; }

        public int LinePosition { get; private set; }

        public ScannerNodeType NodeType { get; set; }

        public string Prefix { get; set; }

        public XamlMember PropertyAttribute { get; set; }

        public XamlText PropertyAttributeText { get; set; }

        public XamlMember PropertyElement { get; set; }

        public XamlText TextContent { get; set; }

        public XamlType Type { get; set; }

        public string TypeNamespace { get; set; }
    }
}

