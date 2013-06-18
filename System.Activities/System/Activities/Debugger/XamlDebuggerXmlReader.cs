namespace System.Activities.Debugger
{
    using System;
    using System.Activities;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xaml;

    public class XamlDebuggerXmlReader : XamlReader, IXamlLineInfo
    {
        private static System.Type attachingTypeName = typeof(XamlDebuggerXmlReader);
        private XamlType attachingXamlTypeName;
        private BracketLocator bracketLocator;
        private System.Activities.Debugger.XamlNode current;
        public static readonly AttachableMemberIdentifier EndColumnName = new AttachableMemberIdentifier(attachingTypeName, "EndColumn");
        public static readonly AttachableMemberIdentifier EndLineName = new AttachableMemberIdentifier(attachingTypeName, "EndLine");
        public static readonly AttachableMemberIdentifier FileNameName = new AttachableMemberIdentifier(attachingTypeName, "FileName");
        private readonly bool hasLineInfo;
        private Stack<System.Activities.Debugger.XamlNode> lineInfoStateStack;
        private Stack<System.Activities.Debugger.XamlNode> records;
        public static readonly AttachableMemberIdentifier StartColumnName = new AttachableMemberIdentifier(attachingTypeName, "StartColumn");
        public static readonly AttachableMemberIdentifier StartLineName = new AttachableMemberIdentifier(attachingTypeName, "StartLine");
        private ReadState state;
        private XamlValidatingReader underlyingReader;
        private XamlSchemaContext xsc;

        public XamlDebuggerXmlReader(XamlReader underlyingReader, TextReader textReader) : this(underlyingReader, underlyingReader as IXamlLineInfo, textReader)
        {
        }

        public XamlDebuggerXmlReader(XamlReader underlyingReader, IXamlLineInfo xamlLineInfo, TextReader textReader)
        {
            this.underlyingReader = new XamlValidatingReader(underlyingReader, xamlLineInfo);
            this.xsc = underlyingReader.SchemaContext;
            this.attachingXamlTypeName = new XamlType(attachingTypeName, this.xsc);
            this.records = new Stack<System.Activities.Debugger.XamlNode>();
            this.lineInfoStateStack = new Stack<System.Activities.Debugger.XamlNode>();
            this.state = InReaderState.Instance;
            this.hasLineInfo = (xamlLineInfo != null) && xamlLineInfo.HasLineInfo;
            if (((xamlLineInfo != null) && xamlLineInfo.HasLineInfo) && (textReader != null))
            {
                this.bracketLocator = new BracketLocator(textReader);
            }
        }

        public static void CopyAttachedSourceLocation(object source, object destination)
        {
            int num;
            int num2;
            int num3;
            int num4;
            if ((AttachablePropertyServices.TryGetProperty<int>(source, StartLineName, out num) && AttachablePropertyServices.TryGetProperty<int>(source, StartColumnName, out num2)) && (AttachablePropertyServices.TryGetProperty<int>(source, EndLineName, out num3) && AttachablePropertyServices.TryGetProperty<int>(source, EndColumnName, out num4)))
            {
                SetStartLine(destination, num);
                SetStartColumn(destination, num2);
                SetEndLine(destination, num3);
                SetEndColumn(destination, num4);
            }
        }

        private static bool DebuggableNode(System.Activities.Debugger.XamlNode node)
        {
            System.Type c = null;
            switch (node.NodeType)
            {
                case XamlNodeType.StartObject:
                {
                    XamlStartRecordNode node2 = node as XamlStartRecordNode;
                    if ((node2 != null) && (node2.RecordType != null))
                    {
                        c = node2.RecordType.UnderlyingType;
                    }
                    break;
                }
                case XamlNodeType.EndObject:
                {
                    XamlEndRecordNode node3 = node as XamlEndRecordNode;
                    if ((node3 != null) && (node3.RecordType != null))
                    {
                        c = node3.RecordType.UnderlyingType;
                    }
                    break;
                }
                case XamlNodeType.StartMember:
                {
                    XamlStartMemberNode node4 = node as XamlStartMemberNode;
                    if ((node4 != null) && (node4.RecordType != null))
                    {
                        c = node4.RecordType.UnderlyingType;
                    }
                    break;
                }
                case XamlNodeType.EndMember:
                {
                    XamlEndMemberNode node5 = node as XamlEndMemberNode;
                    if ((node5 != null) && (node5.RecordType != null))
                    {
                        c = node5.RecordType.UnderlyingType;
                    }
                    break;
                }
            }
            bool flag = false;
            if (((c != null) && typeof(Activity).IsAssignableFrom(c)) && (!typeof(IExpressionContainer).IsAssignableFrom(c) && !typeof(IValueSerializableExpression).IsAssignableFrom(c)))
            {
                flag = true;
            }
            return flag;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetEndColumn(object instance)
        {
            return GetIntegerAttachedProperty(instance, EndColumnName);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetEndLine(object instance)
        {
            return GetIntegerAttachedProperty(instance, EndLineName);
        }

        public static object GetFileName(object instance)
        {
            string str;
            if (AttachablePropertyServices.TryGetProperty<string>(instance, FileNameName, out str))
            {
                return str;
            }
            return string.Empty;
        }

        private static int GetIntegerAttachedProperty(object instance, AttachableMemberIdentifier memberIdentifier)
        {
            int num;
            if (AttachablePropertyServices.TryGetProperty<int>(instance, memberIdentifier, out num))
            {
                return num;
            }
            return -1;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetStartColumn(object instance)
        {
            return GetIntegerAttachedProperty(instance, StartColumnName);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public static object GetStartLine(object instance)
        {
            return GetIntegerAttachedProperty(instance, StartLineName);
        }

        internal void PrepareLineInfo()
        {
            System.Activities.Debugger.XamlNode node = this.records.Pop();
            FilePosition position = new FilePosition(node.LineNumber, node.LinePosition);
            FilePosition position2 = new FilePosition(this.Current.LineNumber, this.Current.LinePosition);
            if (this.bracketLocator != null)
            {
                FilePosition y = this.bracketLocator.FindCloseBracketBefore(position2);
                FilePosition x = this.bracketLocator.FindOpenBracketAfter(position);
                if (FilePosition.Comparer.Compare(x, y) > 0)
                {
                    position2 = y;
                }
                else
                {
                    position2 = this.bracketLocator.FindCloseBracketAfter(position2);
                }
                FilePosition position5 = this.bracketLocator.FindOpenBracketBefore(position);
                if ((position5.Line == position.Line) && ((position5.Column + 1) == position.Column))
                {
                    position = position5;
                }
                else
                {
                    position = x;
                }
                position2.Column++;
            }
            this.PrepareLineInfo(position, position2);
        }

        private void PrepareLineInfo(FilePosition startPosition, FilePosition endPosition)
        {
            this.lineInfoStateStack.Clear();
            this.PushToLineInfoStateStack(this.attachingXamlTypeName.GetAttachableMember(EndColumnName.MemberName), endPosition.Column);
            this.PushToLineInfoStateStack(this.attachingXamlTypeName.GetAttachableMember(EndLineName.MemberName), endPosition.Line);
            this.PushToLineInfoStateStack(this.attachingXamlTypeName.GetAttachableMember(StartColumnName.MemberName), startPosition.Column);
            this.PushToLineInfoStateStack(this.attachingXamlTypeName.GetAttachableMember(StartLineName.MemberName), startPosition.Line);
        }

        private void PushToLineInfoStateStack(XamlMember member, int value)
        {
            XamlEndMemberNode item = new XamlEndMemberNode {
                Member = member
            };
            this.lineInfoStateStack.Push(item);
            XamlAtomNode node2 = new XamlAtomNode {
                Value = value
            };
            this.lineInfoStateStack.Push(node2);
            XamlStartMemberNode node3 = new XamlStartMemberNode {
                Member = member
            };
            this.lineInfoStateStack.Push(node3);
        }

        public override bool Read()
        {
            this.state.Read(this);
            this.current = this.state.GetCurrent(this);
            return !this.underlyingReader.EndOfInput;
        }

        public static void SetEndColumn(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, EndColumnName, value);
        }

        public static void SetEndLine(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, EndLineName, value);
        }

        public static void SetFileName(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, FileNameName, value);
        }

        public static void SetStartColumn(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, StartColumnName, value);
        }

        public static void SetStartLine(object instance, object value)
        {
            AttachablePropertyServices.SetProperty(instance, StartLineName, value);
        }

        private System.Activities.Debugger.XamlNode Current
        {
            get
            {
                return this.current;
            }
        }

        public bool HasLineInfo
        {
            get
            {
                return this.hasLineInfo;
            }
        }

        public override bool IsEof
        {
            get
            {
                return this.underlyingReader.EndOfInput;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this.hasLineInfo)
                {
                    return this.Current.LineNumber;
                }
                return 0;
            }
        }

        public int LinePosition
        {
            get
            {
                if (this.hasLineInfo)
                {
                    return this.Current.LinePosition;
                }
                return 0;
            }
        }

        public override XamlMember Member
        {
            get
            {
                XamlStartMemberNode current = this.current as XamlStartMemberNode;
                return current.Member;
            }
        }

        public override NamespaceDeclaration Namespace
        {
            get
            {
                XamlNamespaceNode current = this.current as XamlNamespaceNode;
                return current.Namespace;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this.current.NodeType;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.xsc;
            }
        }

        public override XamlType Type
        {
            get
            {
                XamlStartRecordNode current = this.current as XamlStartRecordNode;
                return current.RecordType;
            }
        }

        public override object Value
        {
            get
            {
                XamlAtomNode current = this.current as XamlAtomNode;
                return current.Value;
            }
        }

        private class BracketLocator
        {
            private List<XamlDebuggerXmlReader.FilePosition> closeBracketPositions;
            private List<XamlDebuggerXmlReader.FilePosition> openBracketPositions;

            public BracketLocator(TextReader reader)
            {
                string str;
                this.openBracketPositions = new List<XamlDebuggerXmlReader.FilePosition>();
                this.closeBracketPositions = new List<XamlDebuggerXmlReader.FilePosition>();
                XamlDebuggerXmlReader.FilePosition item = new XamlDebuggerXmlReader.FilePosition(0, 0);
                this.openBracketPositions.Add(item);
                this.closeBracketPositions.Add(item);
                int line = 1;
                while ((str = reader.ReadLine()) != null)
                {
                    for (int i = 0; i < str.Length; i++)
                    {
                        if (str[i] == '<')
                        {
                            this.openBracketPositions.Add(new XamlDebuggerXmlReader.FilePosition(line, i + 1));
                        }
                        else if (str[i] == '>')
                        {
                            this.closeBracketPositions.Add(new XamlDebuggerXmlReader.FilePosition(line, i + 1));
                        }
                    }
                    line++;
                }
                item = new XamlDebuggerXmlReader.FilePosition(line, 1);
                this.openBracketPositions.Add(item);
                this.closeBracketPositions.Add(item);
            }

            public XamlDebuggerXmlReader.FilePosition FindCloseBracketAfter(XamlDebuggerXmlReader.FilePosition position)
            {
                return GetPositionAfter(this.closeBracketPositions, position);
            }

            public XamlDebuggerXmlReader.FilePosition FindCloseBracketBefore(XamlDebuggerXmlReader.FilePosition position)
            {
                return GetPositionBefore(this.closeBracketPositions, position);
            }

            public XamlDebuggerXmlReader.FilePosition FindOpenBracketAfter(XamlDebuggerXmlReader.FilePosition position)
            {
                return GetPositionAfter(this.openBracketPositions, position);
            }

            public XamlDebuggerXmlReader.FilePosition FindOpenBracketBefore(XamlDebuggerXmlReader.FilePosition position)
            {
                return GetPositionBefore(this.openBracketPositions, position);
            }

            private static XamlDebuggerXmlReader.FilePosition GetPositionAfter(List<XamlDebuggerXmlReader.FilePosition> positions, XamlDebuggerXmlReader.FilePosition afterThis)
            {
                int num = positions.BinarySearch(afterThis, XamlDebuggerXmlReader.FilePosition.Comparer);
                if (num < 0)
                {
                    num = ~num;
                }
                else
                {
                    num++;
                }
                return positions[num];
            }

            private static XamlDebuggerXmlReader.FilePosition GetPositionBefore(List<XamlDebuggerXmlReader.FilePosition> positions, XamlDebuggerXmlReader.FilePosition beforeThis)
            {
                int num = positions.BinarySearch(beforeThis, XamlDebuggerXmlReader.FilePosition.Comparer);
                if (num < 0)
                {
                    num = ~num;
                }
                return positions[num - 1];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FilePosition
        {
            public int Line;
            public int Column;
            public static XamlDebuggerXmlReader.FilePositionComparer Comparer;
            public FilePosition(int line, int column)
            {
                this.Line = line;
                this.Column = column;
            }

            static FilePosition()
            {
                Comparer = new XamlDebuggerXmlReader.FilePositionComparer();
            }
        }

        private class FilePositionComparer : IComparer<XamlDebuggerXmlReader.FilePosition>
        {
            public int Compare(XamlDebuggerXmlReader.FilePosition x, XamlDebuggerXmlReader.FilePosition y)
            {
                if (x.Line == y.Line)
                {
                    return (x.Column - y.Column);
                }
                return (x.Line - y.Line);
            }
        }

        private class InAttachingLineInfoState : XamlDebuggerXmlReader.ReadState
        {
            public static readonly XamlDebuggerXmlReader.InAttachingLineInfoState Instance = new XamlDebuggerXmlReader.InAttachingLineInfoState();

            public override System.Activities.Debugger.XamlNode GetCurrent(XamlDebuggerXmlReader reader)
            {
                return reader.lineInfoStateStack.Pop();
            }

            public override void Read(XamlDebuggerXmlReader reader)
            {
                if (reader.lineInfoStateStack.Count == 0)
                {
                    reader.state = XamlDebuggerXmlReader.InReaderState.Instance;
                }
            }
        }

        private class InReaderState : XamlDebuggerXmlReader.ReadState
        {
            public static readonly XamlDebuggerXmlReader.InReaderState Instance = new XamlDebuggerXmlReader.InReaderState();

            public override System.Activities.Debugger.XamlNode GetCurrent(XamlDebuggerXmlReader reader)
            {
                return reader.underlyingReader.Current;
            }

            public override void Read(XamlDebuggerXmlReader reader)
            {
                if (!base.UnderlyingReaderRead(reader))
                {
                    reader.current = null;
                }
                else
                {
                    reader.current = reader.underlyingReader.Current;
                    if (XamlDebuggerXmlReader.DebuggableNode(reader.current))
                    {
                        switch (reader.Current.NodeType)
                        {
                            case XamlNodeType.StartObject:
                                reader.records.Push(reader.Current);
                                return;

                            case XamlNodeType.GetObject:
                                return;

                            case XamlNodeType.EndObject:
                                reader.PrepareLineInfo();
                                reader.state = XamlDebuggerXmlReader.InAttachingLineInfoState.Instance;
                                return;
                        }
                    }
                }
            }
        }

        private abstract class ReadState
        {
            protected ReadState()
            {
            }

            public abstract System.Activities.Debugger.XamlNode GetCurrent(XamlDebuggerXmlReader reader);
            public abstract void Read(XamlDebuggerXmlReader reader);
            protected bool UnderlyingReaderRead(XamlDebuggerXmlReader reader)
            {
                return reader.underlyingReader.Read();
            }
        }
    }
}

