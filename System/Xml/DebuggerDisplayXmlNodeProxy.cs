namespace System.Xml
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("{ToString()}")]
    internal struct DebuggerDisplayXmlNodeProxy
    {
        private XmlNode node;
        public DebuggerDisplayXmlNodeProxy(XmlNode node)
        {
            this.node = node;
        }

        public override string ToString()
        {
            XmlNodeType nodeType = this.node.NodeType;
            string str = nodeType.ToString();
            switch (nodeType)
            {
                case XmlNodeType.Element:
                case XmlNodeType.EntityReference:
                    return (str + ", Name=\"" + this.node.Name + "\"");

                case XmlNodeType.Attribute:
                case XmlNodeType.ProcessingInstruction:
                {
                    string str2 = str;
                    return (str2 + ", Name=\"" + this.node.Name + "\", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(this.node.Value) + "\"");
                }
                case XmlNodeType.Text:
                case XmlNodeType.CDATA:
                case XmlNodeType.Comment:
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.XmlDeclaration:
                    return (str + ", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(this.node.Value) + "\"");

                case XmlNodeType.Entity:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Notation:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                    return str;

                case XmlNodeType.DocumentType:
                {
                    XmlDocumentType node = (XmlDocumentType) this.node;
                    string str3 = str;
                    return (str3 + ", Name=\"" + node.Name + "\", SYSTEM=\"" + node.SystemId + "\", PUBLIC=\"" + node.PublicId + "\", Value=\"" + XmlConvert.EscapeValueForDebuggerDisplay(node.InternalSubset) + "\"");
                }
            }
            return str;
        }
    }
}

