namespace System.Activities.Debugger
{
    using System;
    using System.Collections.Generic;
    using System.Xaml;
    using System.Xml.Linq;

    internal class XamlValidatingReader
    {
        private System.Activities.Debugger.XamlNode current;
        private int lastLineNumber;
        private int lastLinePosition;
        private Stack<System.Activities.Debugger.XamlNode> nodes;
        private XamlReader reader;
        private IXamlLineInfo xamlLineInfo;

        public XamlValidatingReader(XamlReader reader, IXamlLineInfo xamlLineInfo)
        {
            this.reader = reader;
            this.xamlLineInfo = xamlLineInfo;
            this.nodes = new Stack<System.Activities.Debugger.XamlNode>();
        }

        private System.Activities.Debugger.XamlNode CreateSpecializedNode()
        {
            switch (this.reader.NodeType)
            {
                case XamlNodeType.StartObject:
                    return this.CreateXamlStartRecordNode();

                case XamlNodeType.GetObject:
                    return this.CreateXamlGetObjectNode();

                case XamlNodeType.EndObject:
                    return this.CreateXamlEndRecordNode();

                case XamlNodeType.StartMember:
                    return this.CreateXamlStartMemberNode();

                case XamlNodeType.EndMember:
                    return this.CreateXamlEndMemberNode();

                case XamlNodeType.Value:
                    return this.CreateXamlAtomNode();

                case XamlNodeType.NamespaceDeclaration:
                    return this.CreateXamlNamespaceNode();
            }
            return null;
        }

        private System.Activities.Debugger.XamlNode CreateXamlAtomNode()
        {
            return new XamlAtomNode { LineNumber = this.xamlLineInfo.LineNumber, LinePosition = this.xamlLineInfo.LinePosition, Value = this.reader.Value };
        }

        private System.Activities.Debugger.XamlNode CreateXamlEndMemberNode()
        {
            XamlStartMemberNode node = (XamlStartMemberNode) this.nodes.Pop();
            return new XamlEndMemberNode { LinePosition = this.xamlLineInfo.LinePosition, LineNumber = this.xamlLineInfo.LineNumber, RecordType = node.RecordType, Member = node.Member };
        }

        private System.Activities.Debugger.XamlNode CreateXamlEndRecordNode()
        {
            XamlStartRecordNode node2 = this.nodes.Pop() as XamlStartRecordNode;
            return new XamlEndRecordNode { LineNumber = this.xamlLineInfo.LineNumber, LinePosition = this.xamlLineInfo.LinePosition, TypeName = (node2 != null) ? node2.TypeName : null, RecordType = (node2 != null) ? node2.RecordType : null };
        }

        private System.Activities.Debugger.XamlNode CreateXamlGetObjectNode()
        {
            XamlGetObjectNode item = new XamlGetObjectNode {
                LineNumber = this.xamlLineInfo.LineNumber,
                LinePosition = this.xamlLineInfo.LinePosition
            };
            this.nodes.Push(item);
            return item;
        }

        private System.Activities.Debugger.XamlNode CreateXamlNamespaceNode()
        {
            return new XamlNamespaceNode { Namespace = this.reader.Namespace };
        }

        private System.Activities.Debugger.XamlNode CreateXamlStartMemberNode()
        {
            XamlStartMemberNode item = new XamlStartMemberNode {
                LinePosition = this.xamlLineInfo.LinePosition,
                LineNumber = this.xamlLineInfo.LineNumber,
                RecordType = this.reader.Member.Type,
                Member = this.reader.Member
            };
            this.nodes.Push(item);
            return item;
        }

        private System.Activities.Debugger.XamlNode CreateXamlStartRecordNode()
        {
            XamlStartRecordNode item = new XamlStartRecordNode {
                LineNumber = this.xamlLineInfo.LineNumber,
                LinePosition = this.xamlLineInfo.LinePosition,
                TypeName = XName.Get(this.reader.Type.Name, this.reader.Type.PreferredXamlNamespace),
                RecordType = this.reader.Type
            };
            this.nodes.Push(item);
            return item;
        }

        public bool Read()
        {
            if (!this.reader.Read())
            {
                return false;
            }
            this.current = this.CreateSpecializedNode();
            if ((this.current.LineNumber == 0) || (this.current.LinePosition == 0))
            {
                this.current.LineNumber = this.lastLineNumber;
                this.current.LinePosition = this.lastLinePosition;
            }
            else
            {
                this.lastLineNumber = this.current.LineNumber;
                this.lastLinePosition = this.current.LinePosition;
            }
            return true;
        }

        public System.Activities.Debugger.XamlNode Current
        {
            get
            {
                return this.current;
            }
        }

        public bool EndOfInput
        {
            get
            {
                return this.reader.IsEof;
            }
        }
    }
}

