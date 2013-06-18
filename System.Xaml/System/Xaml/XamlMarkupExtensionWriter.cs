namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class XamlMarkupExtensionWriter : XamlWriter
    {
        private WriterState currentState;
        private bool failed;
        private XamlMarkupExtensionWriterSettings meSettings;
        private Stack<Node> nodes;
        private StringBuilder sb;
        private XamlXmlWriterSettings settings;
        private XamlXmlWriter xamlXmlWriter;

        public XamlMarkupExtensionWriter(XamlXmlWriter xamlXmlWriter)
        {
            this.Initialize(xamlXmlWriter);
        }

        public XamlMarkupExtensionWriter(XamlXmlWriter xamlXmlWriter, XamlMarkupExtensionWriterSettings meSettings)
        {
            this.meSettings = meSettings;
            this.Initialize(xamlXmlWriter);
        }

        private void CheckMemberForUniqueness(Node objectNode, XamlMember property)
        {
            if (!this.settings.AssumeValidInput)
            {
                if (objectNode.Members == null)
                {
                    objectNode.Members = new XamlPropertySet();
                }
                else if (objectNode.Members.Contains(property))
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterDuplicateMember", new object[] { property.Name }));
                }
                objectNode.Members.Add(property);
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void Initialize(XamlXmlWriter xamlXmlWriter)
        {
            this.xamlXmlWriter = xamlXmlWriter;
            this.settings = xamlXmlWriter.Settings;
            this.meSettings = this.meSettings ?? new XamlMarkupExtensionWriterSettings();
            this.currentState = Start.State;
            this.sb = new StringBuilder();
            this.nodes = new Stack<Node>();
            this.failed = false;
        }

        private string LookupPrefix(XamlMember property)
        {
            string str;
            string prefix = this.xamlXmlWriter.LookupPrefix(property.GetXamlNamespaces(), out str);
            if ((prefix == null) && !this.meSettings.ContinueWritingWhenPrefixIsNotFound)
            {
                this.failed = true;
                return string.Empty;
            }
            return prefix;
        }

        private string LookupPrefix(XamlType type)
        {
            string str;
            string prefix = this.xamlXmlWriter.LookupPrefix(type.GetXamlNamespaces(), out str);
            if ((prefix == null) && !this.meSettings.ContinueWritingWhenPrefixIsNotFound)
            {
                this.failed = true;
                return string.Empty;
            }
            return prefix;
        }

        public void Reset()
        {
            this.currentState = Start.State;
            this.sb = new StringBuilder();
            this.nodes.Clear();
            this.failed = false;
        }

        public override void WriteEndMember()
        {
            this.currentState.WriteEndMember(this);
        }

        public override void WriteEndObject()
        {
            this.currentState.WriteEndObject(this);
        }

        public override void WriteGetObject()
        {
            this.currentState.WriteGetObject(this);
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            this.currentState.WriteNamespace(this, namespaceDeclaration);
        }

        public override void WriteStartMember(XamlMember property)
        {
            this.currentState.WriteStartMember(this, property);
        }

        public override void WriteStartObject(XamlType type)
        {
            this.currentState.WriteStartObject(this, type);
        }

        public override void WriteValue(object value)
        {
            string str = value as string;
            if (str == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("XamlMarkupExtensionWriterCannotWriteNonstringValue"));
            }
            this.currentState.WriteValue(this, str);
        }

        public bool Failed
        {
            get
            {
                return this.failed;
            }
        }

        public string MarkupExtensionString
        {
            get
            {
                if (this.nodes.Count == 0)
                {
                    return this.sb.ToString();
                }
                return null;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.xamlXmlWriter.SchemaContext;
            }
        }

        private class InMember : XamlMarkupExtensionWriter.WriterState
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.InMember();

            private InMember()
            {
            }

            public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                if (!type.IsMarkupExtension)
                {
                    writer.failed = true;
                }
                else
                {
                    string prefix = writer.LookupPrefix(type);
                    writer.sb.Append("{");
                    base.WritePrefix(writer, prefix);
                    writer.sb.Append(XamlXmlWriter.GetTypeName(type));
                    XamlMarkupExtensionWriter.Node item = new XamlMarkupExtensionWriter.Node {
                        NodeType = XamlNodeType.StartObject,
                        XamlType = type
                    };
                    writer.nodes.Push(item);
                    writer.currentState = XamlMarkupExtensionWriter.InObjectBeforeMember.State;
                }
            }

            public override void WriteValue(XamlMarkupExtensionWriter writer, string value)
            {
                base.WriteString(writer, value);
                writer.currentState = XamlMarkupExtensionWriter.InMemberAfterValueOrEndObject.State;
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberAfterValueOrEndObject : XamlMarkupExtensionWriter.WriterState
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.InMemberAfterValueOrEndObject();

            private InMemberAfterValueOrEndObject()
            {
            }

            public override void WriteEndMember(XamlMarkupExtensionWriter writer)
            {
                if (writer.nodes.Count == 0)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                if (writer.nodes.Pop().NodeType != XamlNodeType.StartMember)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                writer.currentState = XamlMarkupExtensionWriter.InObjectAfterMember.State;
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private abstract class InObject : XamlMarkupExtensionWriter.WriterState
        {
            protected InObject()
            {
            }

            protected void UpdateStack(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                if (writer.nodes.Count == 0)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                XamlMarkupExtensionWriter.Node objectNode = writer.nodes.Peek();
                if (objectNode.NodeType != XamlNodeType.StartObject)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                writer.CheckMemberForUniqueness(objectNode, property);
                XamlMarkupExtensionWriter.Node item = new XamlMarkupExtensionWriter.Node {
                    NodeType = XamlNodeType.StartMember,
                    XamlType = objectNode.XamlType,
                    XamlProperty = property
                };
                writer.nodes.Push(item);
            }

            public override void WriteEndObject(XamlMarkupExtensionWriter writer)
            {
                if (writer.nodes.Count == 0)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                if (writer.nodes.Pop().NodeType != XamlNodeType.StartObject)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                writer.sb.Append("}");
                if (writer.nodes.Count == 0)
                {
                    writer.currentState = XamlMarkupExtensionWriter.Start.State;
                }
                else
                {
                    XamlMarkupExtensionWriter.Node node2 = writer.nodes.Peek();
                    if (node2.NodeType != XamlNodeType.StartMember)
                    {
                        throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                    }
                    if (node2.XamlProperty == XamlLanguage.PositionalParameters)
                    {
                        writer.currentState = XamlMarkupExtensionWriter.InPositionalParametersAfterValue.State;
                    }
                    else
                    {
                        writer.currentState = XamlMarkupExtensionWriter.InMemberAfterValueOrEndObject.State;
                    }
                }
            }

            protected void WriteNonPositionalParameterMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                if (XamlXmlWriter.IsImplicit(property) || (property.IsDirective && (property.Type.IsCollection || property.Type.IsDictionary)))
                {
                    writer.failed = true;
                }
                else
                {
                    if (property.IsDirective)
                    {
                        writer.sb.Append(this.Delimiter);
                        base.WritePrefix(writer, writer.LookupPrefix(property));
                        writer.sb.Append(property.Name);
                    }
                    else if (property.IsAttachable)
                    {
                        writer.sb.Append(this.Delimiter);
                        base.WritePrefix(writer, writer.LookupPrefix(property));
                        string str = property.DeclaringType.Name + "." + property.Name;
                        writer.sb.Append(str);
                    }
                    else
                    {
                        writer.sb.Append(this.Delimiter);
                        writer.sb.Append(property.Name);
                    }
                    writer.sb.Append("=");
                    writer.currentState = XamlMarkupExtensionWriter.InMember.State;
                }
            }

            public abstract string Delimiter { get; }
        }

        private class InObjectAfterMember : XamlMarkupExtensionWriter.InObject
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.InObjectAfterMember();

            private InObjectAfterMember()
            {
            }

            public override void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                base.UpdateStack(writer, property);
                base.WriteNonPositionalParameterMember(writer, property);
            }

            public override string Delimiter
            {
                get
                {
                    return ", ";
                }
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InObjectBeforeMember : XamlMarkupExtensionWriter.InObject
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.InObjectBeforeMember();

            private InObjectBeforeMember()
            {
            }

            public override void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                base.UpdateStack(writer, property);
                if (property == XamlLanguage.PositionalParameters)
                {
                    writer.currentState = XamlMarkupExtensionWriter.InPositionalParametersBeforeValue.State;
                }
                else
                {
                    base.WriteNonPositionalParameterMember(writer, property);
                }
            }

            public override string Delimiter
            {
                get
                {
                    return " ";
                }
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private abstract class InPositionalParameters : XamlMarkupExtensionWriter.WriterState
        {
            protected InPositionalParameters()
            {
            }

            public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                writer.sb.Append(this.Delimiter);
                writer.currentState = XamlMarkupExtensionWriter.InMember.State;
                writer.currentState.WriteStartObject(writer, type);
            }

            public override void WriteValue(XamlMarkupExtensionWriter writer, string value)
            {
                writer.sb.Append(this.Delimiter);
                base.WriteString(writer, value);
                writer.currentState = XamlMarkupExtensionWriter.InPositionalParametersAfterValue.State;
            }

            public abstract string Delimiter { get; }
        }

        private class InPositionalParametersAfterValue : XamlMarkupExtensionWriter.InPositionalParameters
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.InPositionalParametersAfterValue();

            private InPositionalParametersAfterValue()
            {
            }

            public override void WriteEndMember(XamlMarkupExtensionWriter writer)
            {
                XamlMarkupExtensionWriter.Node node = writer.nodes.Pop();
                if ((node.NodeType != XamlNodeType.StartMember) || (node.XamlProperty != XamlLanguage.PositionalParameters))
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlMarkupExtensionWriterInputInvalid"));
                }
                writer.currentState = XamlMarkupExtensionWriter.InObjectAfterMember.State;
            }

            public override string Delimiter
            {
                get
                {
                    return ", ";
                }
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InPositionalParametersBeforeValue : XamlMarkupExtensionWriter.InPositionalParameters
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.InPositionalParametersBeforeValue();

            private InPositionalParametersBeforeValue()
            {
            }

            public override string Delimiter
            {
                get
                {
                    return " ";
                }
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class Node
        {
            public XamlPropertySet Members { get; set; }

            public XamlNodeType NodeType { get; set; }

            public XamlMember XamlProperty { get; set; }

            public System.Xaml.XamlType XamlType { get; set; }
        }

        private class Start : XamlMarkupExtensionWriter.WriterState
        {
            private static XamlMarkupExtensionWriter.WriterState state = new XamlMarkupExtensionWriter.Start();

            private Start()
            {
            }

            public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                writer.Reset();
                string prefix = writer.LookupPrefix(type);
                writer.sb.Append("{");
                base.WritePrefix(writer, prefix);
                writer.sb.Append(XamlXmlWriter.GetTypeName(type));
                XamlMarkupExtensionWriter.Node item = new XamlMarkupExtensionWriter.Node {
                    NodeType = XamlNodeType.StartObject,
                    XamlType = type
                };
                writer.nodes.Push(item);
                writer.currentState = XamlMarkupExtensionWriter.InObjectBeforeMember.State;
            }

            public static XamlMarkupExtensionWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private abstract class WriterState
        {
            private static char[] specialChars = new char[] { '\'', '"', ',', '=', '{', '}', '\\', ' ' };

            protected WriterState()
            {
            }

            protected static bool ContainCharacterToEscape(string s)
            {
                return (s.IndexOfAny(specialChars) >= 0);
            }

            protected static string FormatStringInCorrectSyntax(string s)
            {
                StringBuilder builder = new StringBuilder("\"");
                for (int i = 0; i < s.Length; i++)
                {
                    if ((s[i] == '\\') || (s[i] == '"'))
                    {
                        builder.Append(@"\");
                    }
                    builder.Append(s[i]);
                }
                builder.Append("\"");
                return builder.ToString();
            }

            public virtual void WriteEndMember(XamlMarkupExtensionWriter writer)
            {
                writer.failed = true;
            }

            public virtual void WriteEndObject(XamlMarkupExtensionWriter writer)
            {
                writer.failed = true;
            }

            public virtual void WriteGetObject(XamlMarkupExtensionWriter writer)
            {
                writer.failed = true;
            }

            public virtual void WriteNamespace(XamlMarkupExtensionWriter writer, NamespaceDeclaration namespaceDeclaration)
            {
                writer.failed = true;
            }

            protected void WritePrefix(XamlMarkupExtensionWriter writer, string prefix)
            {
                if (prefix != "")
                {
                    writer.sb.Append(prefix);
                    writer.sb.Append(":");
                }
            }

            public virtual void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
            {
                writer.failed = true;
            }

            public virtual void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
            {
                writer.failed = true;
            }

            public void WriteString(XamlMarkupExtensionWriter writer, string value)
            {
                if (ContainCharacterToEscape(value) || (value == string.Empty))
                {
                    value = FormatStringInCorrectSyntax(value);
                }
                writer.sb.Append(value);
            }

            public virtual void WriteValue(XamlMarkupExtensionWriter writer, string value)
            {
                writer.failed = true;
            }
        }
    }
}

