namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xaml.MS.Impl;
    using System.Xml;

    public class XamlXmlWriter : XamlWriter
    {
        private WriterState currentState;
        private string deferredValue;
        private bool deferredValueIsME;
        private bool isFirstElementOfWhitespaceSignificantCollection;
        private Stack<List<XamlNode>> meNodesStack;
        private XamlMarkupExtensionWriter meWriter;
        private Stack<Frame> namespaceScopes;
        private XmlWriter output;
        private PositionalParameterStateInfo ppStateInfo;
        private Dictionary<string, string> prefixAssignmentHistory;
        private XamlSchemaContext schemaContext;
        private XamlXmlWriterSettings settings;

        public XamlXmlWriter(Stream stream, XamlSchemaContext schemaContext) : this(stream, schemaContext, null)
        {
        }

        public XamlXmlWriter(TextWriter textWriter, XamlSchemaContext schemaContext) : this(textWriter, schemaContext, null)
        {
        }

        public XamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext) : this(xmlWriter, schemaContext, null)
        {
        }

        public XamlXmlWriter(Stream stream, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if ((settings != null) && settings.CloseOutput)
            {
                XmlWriterSettings settings2 = new XmlWriterSettings {
                    CloseOutput = true
                };
                this.InitializeXamlXmlWriter(XmlWriter.Create(stream, settings2), schemaContext, settings);
            }
            else
            {
                this.InitializeXamlXmlWriter(XmlWriter.Create(stream), schemaContext, settings);
            }
        }

        public XamlXmlWriter(TextWriter textWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (textWriter == null)
            {
                throw new ArgumentNullException("textWriter");
            }
            if ((settings != null) && settings.CloseOutput)
            {
                XmlWriterSettings settings2 = new XmlWriterSettings {
                    CloseOutput = true
                };
                this.InitializeXamlXmlWriter(XmlWriter.Create(textWriter, settings2), schemaContext, settings);
            }
            else
            {
                this.InitializeXamlXmlWriter(XmlWriter.Create(textWriter), schemaContext, settings);
            }
        }

        public XamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (xmlWriter == null)
            {
                throw new ArgumentNullException("xmlWriter");
            }
            this.InitializeXamlXmlWriter(xmlWriter, schemaContext, settings);
        }

        private void AssignNamespacePrefix(string ns, string prefix)
        {
            string str;
            this.namespaceScopes.Peek().AssignNamespacePrefix(ns, prefix);
            if (this.prefixAssignmentHistory.TryGetValue(prefix, out str))
            {
                if (str != ns)
                {
                    this.prefixAssignmentHistory[prefix] = null;
                }
            }
            else
            {
                this.prefixAssignmentHistory.Add(prefix, ns);
            }
        }

        private string BuildTypeArgumentsString(IList<XamlType> typeArguments)
        {
            StringBuilder builder = new StringBuilder();
            foreach (XamlType type in typeArguments)
            {
                if (builder.Length != 0)
                {
                    builder.Append(", ");
                }
                builder.Append(this.ConvertXamlTypeToString(type));
            }
            return builder.ToString();
        }

        private void CheckIsDisposed()
        {
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException("XamlXmlWriter");
            }
        }

        private void CheckMemberForUniqueness(XamlMember property)
        {
            if (!this.settings.AssumeValidInput)
            {
                Frame frame = this.namespaceScopes.Peek();
                if ((frame.AllocatingNodeType != XamlNodeType.StartObject) && (frame.AllocatingNodeType != XamlNodeType.GetObject))
                {
                    Frame item = this.namespaceScopes.Pop();
                    frame = this.namespaceScopes.Peek();
                    this.namespaceScopes.Push(item);
                }
                if (frame.Members == null)
                {
                    frame.Members = new XamlPropertySet();
                }
                else if (frame.Members.Contains(property))
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterDuplicateMember", new object[] { property.Name }));
                }
                frame.Members.Add(property);
            }
        }

        internal static bool ContainsConsecutiveInnerSpaces(string s)
        {
            for (int i = 1; i < (s.Length - 1); i++)
            {
                if ((s[i] == KnownStrings.SpaceChar) && (s[i + 1] == KnownStrings.SpaceChar))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool ContainsLeadingSpace(string s)
        {
            return (s[0] == KnownStrings.SpaceChar);
        }

        internal static bool ContainsTrailingSpace(string s)
        {
            return (s[s.Length - 1] == KnownStrings.SpaceChar);
        }

        internal static bool ContainsWhitespaceThatIsNotSpace(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (((s[i] == KnownStrings.TabChar) || (s[i] == KnownStrings.NewlineChar)) || (s[i] == KnownStrings.ReturnChar))
                {
                    return true;
                }
            }
            return false;
        }

        private string ConvertXamlTypeToString(XamlType typeArgument)
        {
            StringBuilder builder = new StringBuilder();
            this.ConvertXamlTypeToStringHelper(typeArgument, builder);
            return builder.ToString();
        }

        private void ConvertXamlTypeToStringHelper(XamlType type, StringBuilder builder)
        {
            string str;
            string prefix = this.LookupPrefix(type.GetXamlNamespaces(), out str);
            string typeName = GetTypeName(type);
            string str4 = (prefix == string.Empty) ? typeName : (prefix + ":" + typeName);
            builder.Append(str4);
            if (type.TypeArguments != null)
            {
                bool flag = false;
                builder.Append("(");
                foreach (XamlType type2 in type.TypeArguments)
                {
                    if (flag)
                    {
                        builder.Append(", ");
                    }
                    this.ConvertXamlTypeToStringHelper(type2, builder);
                    flag = true;
                }
                builder.Append(")");
            }
        }

        private string DefinePrefix(string ns)
        {
            if (!this.IsPrefixEverUsedForAnotherNamespace(string.Empty, ns))
            {
                return string.Empty;
            }
            string preferredPrefix = this.SchemaContext.GetPreferredPrefix(ns);
            string prefix = preferredPrefix;
            int num = 0;
            while (this.IsPrefixEverUsedForAnotherNamespace(prefix, ns))
            {
                num++;
                prefix = preferredPrefix + num.ToString(TypeConverterHelper.InvariantEnglishUS);
            }
            if (prefix != string.Empty)
            {
                XmlConvert.VerifyNCName(prefix);
            }
            return prefix;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && !base.IsDisposed)
                {
                    if (this.settings.CloseOutput)
                    {
                        this.output.Close();
                    }
                    else
                    {
                        this.Flush();
                    }
                    ((IDisposable) this.meWriter).Dispose();
                }
            }
            finally
            {
                this.output.Dispose();
                base.Dispose(disposing);
            }
        }

        private string FindPrefix(IList<string> namespaces, out string chosenNamespace)
        {
            string prefix = this.LookupPrefix(namespaces, out chosenNamespace);
            if (prefix == null)
            {
                chosenNamespace = namespaces[0];
                prefix = this.DefinePrefix(chosenNamespace);
                this.AssignNamespacePrefix(chosenNamespace, prefix);
                return prefix;
            }
            if (this.IsShadowed(chosenNamespace, prefix))
            {
                prefix = this.DefinePrefix(chosenNamespace);
                this.AssignNamespacePrefix(chosenNamespace, prefix);
            }
            return prefix;
        }

        public void Flush()
        {
            this.output.Flush();
        }

        private static XamlType GetContainingXamlType(XamlXmlWriter writer)
        {
            Stack<Frame>.Enumerator enumerator = writer.namespaceScopes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if ((enumerator.Current.AllocatingNodeType == XamlNodeType.StartMember) && (enumerator.Current.Member != XamlLanguage.Items))
                {
                    return (((enumerator.Current.Member == null) || enumerator.Current.Member.IsUnknown) ? null : enumerator.Current.Member.Type);
                }
                if (enumerator.Current.AllocatingNodeType == XamlNodeType.StartObject)
                {
                    return enumerator.Current.Type;
                }
            }
            return null;
        }

        internal static string GetTypeName(XamlType type)
        {
            string name = type.Name;
            if (type.IsMarkupExtension && type.Name.EndsWith("Extension", false, TypeConverterHelper.InvariantEnglishUS))
            {
                name = type.Name.Substring(0, type.Name.Length - "Extension".Length);
            }
            return name;
        }

        internal static bool HasSignificantWhitespace(string s)
        {
            if (s == string.Empty)
            {
                return false;
            }
            if ((!ContainsLeadingSpace(s) && !ContainsTrailingSpace(s)) && !ContainsConsecutiveInnerSpaces(s))
            {
                return ContainsWhitespaceThatIsNotSpace(s);
            }
            return true;
        }

        private void InitializeXamlXmlWriter(XmlWriter xmlWriter, XamlSchemaContext schemaContext, XamlXmlWriterSettings settings)
        {
            if (schemaContext == null)
            {
                throw new ArgumentNullException("schemaContext");
            }
            this.schemaContext = schemaContext;
            this.output = xmlWriter;
            this.settings = (settings == null) ? new XamlXmlWriterSettings() : settings.Copy();
            this.currentState = Start.State;
            this.namespaceScopes = new Stack<Frame>();
            Frame item = new Frame {
                AllocatingNodeType = XamlNodeType.StartObject
            };
            this.namespaceScopes.Push(item);
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("xml", "http://www.w3.org/XML/1998/namespace");
            this.prefixAssignmentHistory = dictionary;
            this.meNodesStack = new Stack<List<XamlNode>>();
            this.meWriter = new XamlMarkupExtensionWriter(this);
            this.ppStateInfo = new PositionalParameterStateInfo(this);
        }

        internal static bool IsImplicit(XamlMember xamlMember)
        {
            if (!xamlMember.IsDirective)
            {
                return false;
            }
            if (((xamlMember != XamlLanguage.Items) && (xamlMember != XamlLanguage.Initialization)) && (xamlMember != XamlLanguage.PositionalParameters))
            {
                return (xamlMember == XamlLanguage.UnknownContent);
            }
            return true;
        }

        private bool IsPrefixEverUsedForAnotherNamespace(string prefix, string ns)
        {
            string str;
            return (this.prefixAssignmentHistory.TryGetValue(prefix, out str) && (ns != str));
        }

        private bool IsShadowed(string ns, string prefix)
        {
            foreach (Frame frame in this.namespaceScopes)
            {
                string str;
                if (frame.TryLookupNamespace(prefix, out str))
                {
                    return (str != ns);
                }
            }
            throw new InvalidOperationException(System.Xaml.SR.Get("PrefixNotInFrames", new object[] { prefix }));
        }

        internal string LookupPrefix(IList<string> namespaces, out string chosenNamespace)
        {
            chosenNamespace = null;
            foreach (Frame frame in this.namespaceScopes)
            {
                foreach (string str2 in namespaces)
                {
                    string str;
                    if (frame.TryLookupPrefix(str2, out str))
                    {
                        chosenNamespace = str2;
                        return str;
                    }
                }
            }
            return null;
        }

        private static bool StringStartsWithCurly(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            return (s[0] == '{');
        }

        private bool TypeArgumentsContainNamespaceThatNeedsDefinition(XamlType type)
        {
            string str;
            string prefix = this.LookupPrefix(type.GetXamlNamespaces(), out str);
            if ((prefix == null) || this.IsShadowed(str, prefix))
            {
                return true;
            }
            if (type.TypeArguments != null)
            {
                foreach (XamlType type2 in type.TypeArguments)
                {
                    if (this.TypeArgumentsContainNamespaceThatNeedsDefinition(type2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void WriteDeferredNamespaces(XamlNodeType nodeType)
        {
            Frame frame = this.namespaceScopes.Peek();
            if (frame.AllocatingNodeType != nodeType)
            {
                Frame item = this.namespaceScopes.Pop();
                frame = this.namespaceScopes.Peek();
                this.namespaceScopes.Push(item);
            }
            foreach (KeyValuePair<string, string> pair in frame.GetSortedPrefixMap())
            {
                this.output.WriteAttributeString("xmlns", pair.Key, null, pair.Value);
            }
        }

        public override void WriteEndMember()
        {
            this.CheckIsDisposed();
            this.currentState.WriteEndMember(this);
        }

        public override void WriteEndObject()
        {
            this.CheckIsDisposed();
            this.currentState.WriteEndObject(this);
        }

        public override void WriteGetObject()
        {
            this.CheckIsDisposed();
            XamlType type = null;
            Frame frame = this.namespaceScopes.Peek();
            if (frame.AllocatingNodeType == XamlNodeType.StartMember)
            {
                type = frame.Member.Type;
            }
            this.currentState.WriteObject(this, type, true);
        }

        public override void WriteNamespace(System.Xaml.NamespaceDeclaration namespaceDeclaration)
        {
            this.CheckIsDisposed();
            if (namespaceDeclaration == null)
            {
                throw new ArgumentNullException("namespaceDeclaration");
            }
            if (namespaceDeclaration.Prefix == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NamespaceDeclarationPrefixCannotBeNull"), "namespaceDeclaration");
            }
            if (namespaceDeclaration.Namespace == null)
            {
                throw new ArgumentException(System.Xaml.SR.Get("NamespaceDeclarationNamespaceCannotBeNull"), "namespaceDeclaration");
            }
            if (namespaceDeclaration.Prefix == "xml")
            {
                throw new ArgumentException(System.Xaml.SR.Get("NamespaceDeclarationCannotBeXml"), "namespaceDeclaration");
            }
            this.currentState.WriteNamespace(this, namespaceDeclaration);
        }

        public override void WriteStartMember(XamlMember property)
        {
            this.CheckIsDisposed();
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }
            if (!property.IsNameValid)
            {
                throw new ArgumentException(System.Xaml.SR.Get("MemberHasInvalidXamlName", new object[] { property.Name }), "property");
            }
            this.currentState.WriteStartMember(this, property);
        }

        public override void WriteStartObject(XamlType type)
        {
            this.CheckIsDisposed();
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (!type.IsNameValid)
            {
                throw new ArgumentException(System.Xaml.SR.Get("TypeHasInvalidXamlName", new object[] { type.Name }), "type");
            }
            this.currentState.WriteObject(this, type, false);
            if (type.TypeArguments != null)
            {
                this.WriteTypeArguments(type);
            }
        }

        private void WriteTypeArguments(XamlType type)
        {
            if (this.TypeArgumentsContainNamespaceThatNeedsDefinition(type))
            {
                this.WriteUndefinedNamespaces(type);
            }
            this.WriteStartMember(XamlLanguage.TypeArguments);
            this.WriteValue(this.BuildTypeArgumentsString(type.TypeArguments));
            this.WriteEndMember();
        }

        private void WriteUndefinedNamespaces(XamlType type)
        {
            string str;
            IList<string> xamlNamespaces = type.GetXamlNamespaces();
            string prefix = this.LookupPrefix(xamlNamespaces, out str);
            if (prefix == null)
            {
                str = xamlNamespaces[0];
                prefix = this.DefinePrefix(str);
                this.currentState.WriteNamespace(this, new System.Xaml.NamespaceDeclaration(str, prefix));
            }
            else if (this.IsShadowed(str, prefix))
            {
                prefix = this.DefinePrefix(str);
                this.currentState.WriteNamespace(this, new System.Xaml.NamespaceDeclaration(str, prefix));
            }
            if (type.TypeArguments != null)
            {
                foreach (XamlType type2 in type.TypeArguments)
                {
                    this.WriteUndefinedNamespaces(type2);
                }
            }
        }

        public override void WriteValue(object value)
        {
            this.CheckIsDisposed();
            if (value == null)
            {
                this.WriteStartObject(XamlLanguage.Null);
                this.WriteEndObject();
            }
            else
            {
                string str = value as string;
                if (str == null)
                {
                    throw new ArgumentException(System.Xaml.SR.Get("XamlXmlWriterCannotWriteNonstringValue"), "value");
                }
                this.currentState.WriteValue(this, str);
            }
        }

        private static void WriteXmlSpace(XamlXmlWriter writer)
        {
            writer.output.WriteAttributeString("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.schemaContext;
            }
        }

        public XamlXmlWriterSettings Settings
        {
            get
            {
                return this.settings.Copy();
            }
        }

        private class End : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.End();

            private End()
            {
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class ExpandPositionalParameters : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.ExpandPositionalParameters();

            private ExpandPositionalParameters()
            {
            }

            private void ExpandPositionalParametersIntoProperties(XamlXmlWriter writer)
            {
                XamlType objectXamlType = writer.namespaceScopes.Peek().Type;
                if (objectXamlType.UnderlyingType == null)
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("ExpandPositionalParametersWithoutUnderlyingType", new object[] { objectXamlType.GetQualifiedName() }));
                }
                int count = writer.ppStateInfo.NodesList.Count;
                ParameterInfo[] parametersInfo = this.GetParametersInfo(objectXamlType, count);
                List<XamlMember> allPropertiesWithCAA = this.GetAllPropertiesWithCAA(objectXamlType);
                if (parametersInfo.Length != allPropertiesWithCAA.Count)
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("ConstructorNotFoundForGivenPositionalParameters"));
                }
                for (int i = 0; i < parametersInfo.Length; i++)
                {
                    ParameterInfo info = parametersInfo[i];
                    XamlMember member = null;
                    foreach (XamlMember member2 in allPropertiesWithCAA)
                    {
                        if ((member2.Type.UnderlyingType == info.ParameterType) && (XamlObjectReader.GetConstructorArgument(member2) == info.Name))
                        {
                            member = member2;
                            break;
                        }
                    }
                    if (member == null)
                    {
                        throw new XamlXmlWriterException(System.Xaml.SR.Get("ConstructorNotFoundForGivenPositionalParameters"));
                    }
                    XamlMember data = objectXamlType.GetMember(member.Name);
                    if (data.IsReadOnly)
                    {
                        throw new XamlXmlWriterException(System.Xaml.SR.Get("ExpandPositionalParametersWithReadOnlyProperties"));
                    }
                    writer.ppStateInfo.NodesList[i].Insert(0, new XamlNode(XamlNodeType.StartMember, data));
                    writer.ppStateInfo.NodesList[i].Add(new XamlNode(XamlNodeType.EndMember));
                }
            }

            private List<XamlMember> GetAllPropertiesWithCAA(XamlType objectXamlType)
            {
                ICollection<XamlMember> allMembers = objectXamlType.GetAllMembers();
                ICollection<XamlMember> allExcludedReadOnlyMembers = objectXamlType.GetAllExcludedReadOnlyMembers();
                List<XamlMember> list = new List<XamlMember>();
                foreach (XamlMember member in allMembers)
                {
                    if (!string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(member)))
                    {
                        list.Add(member);
                    }
                }
                foreach (XamlMember member2 in allExcludedReadOnlyMembers)
                {
                    if (!string.IsNullOrEmpty(XamlObjectReader.GetConstructorArgument(member2)))
                    {
                        list.Add(member2);
                    }
                }
                return list;
            }

            private ParameterInfo[] GetParametersInfo(XamlType objectXamlType, int numOfParameters)
            {
                IList<XamlType> positionalParameters = objectXamlType.GetPositionalParameters(numOfParameters);
                if (positionalParameters == null)
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("ConstructorNotFoundForGivenPositionalParameters"));
                }
                Type[] paramTypes = new Type[numOfParameters];
                int num = 0;
                foreach (XamlType type in positionalParameters)
                {
                    Type underlyingType = type.UnderlyingType;
                    if (underlyingType == null)
                    {
                        throw new XamlXmlWriterException(System.Xaml.SR.Get("ConstructorNotFoundForGivenPositionalParameters"));
                    }
                    paramTypes[num++] = underlyingType;
                }
                ConstructorInfo constructor = objectXamlType.GetConstructor(paramTypes);
                if (constructor == null)
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("ConstructorNotFoundForGivenPositionalParameters"));
                }
                return constructor.GetParameters();
            }

            private void ThrowIfFailed(bool fail, string operation)
            {
                if (fail)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { operation }));
                }
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.ppStateInfo.Writer.WriteEndMember();
                this.ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteEndMember");
                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    this.ExpandPositionalParametersIntoProperties(writer);
                    this.WriteNodes(writer);
                }
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                writer.ppStateInfo.Writer.WriteEndObject();
                this.ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteEndObject");
                XamlNode item = new XamlNode(XamlNodeType.EndObject);
                writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
                writer.ppStateInfo.CurrentDepth--;
            }

            private void WriteNodes(XamlXmlWriter writer)
            {
                List<List<XamlNode>> nodesList = writer.ppStateInfo.NodesList;
                writer.ppStateInfo.Reset();
                writer.currentState = writer.ppStateInfo.ReturnState;
                foreach (List<XamlNode> list2 in nodesList)
                {
                    foreach (XamlNode node in list2)
                    {
                        writer.currentState.WriteNode(writer, node);
                    }
                }
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                if (isObjectFromMember)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteGetObject" }));
                }
                writer.ppStateInfo.Writer.WriteStartObject(type);
                this.ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteStartObject");
                XamlNode item = new XamlNode(XamlNodeType.StartObject, type);
                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    writer.ppStateInfo.NodesList.Add(new List<XamlNode> { item });
                }
                else
                {
                    writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
                }
                writer.ppStateInfo.CurrentDepth++;
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                writer.ppStateInfo.Writer.WriteStartMember(property);
                this.ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteStartMember");
                XamlNode item = new XamlNode(XamlNodeType.StartMember, property);
                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    writer.ppStateInfo.NodesList.Add(new List<XamlNode> { item });
                }
                else
                {
                    writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
                }
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.ppStateInfo.Writer.WriteValue(value);
                this.ThrowIfFailed(writer.ppStateInfo.Writer.Failed, "WriteValue");
                XamlNode item = new XamlNode(XamlNodeType.Value, value);
                if (writer.ppStateInfo.CurrentDepth == 0)
                {
                    writer.ppStateInfo.NodesList.Add(new List<XamlNode> { item });
                }
                else
                {
                    writer.ppStateInfo.NodesList[writer.ppStateInfo.NodesList.Count - 1].Add(item);
                }
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class Frame
        {
            private Dictionary<string, string> namespaceMap = new Dictionary<string, string>();
            private Dictionary<string, string> prefixMap = new Dictionary<string, string>();

            public void AssignNamespacePrefix(string ns, string prefix)
            {
                if (this.prefixMap.ContainsKey(prefix))
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterPrefixAlreadyDefinedInCurrentScope", new object[] { prefix }));
                }
                if (this.namespaceMap.ContainsKey(ns))
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterNamespaceAlreadyHasPrefixInCurrentScope", new object[] { ns }));
                }
                this.prefixMap[prefix] = ns;
                this.namespaceMap[ns] = prefix;
            }

            private static int CompareByKey(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
            {
                return string.Compare(x.Key, y.Key, false, TypeConverterHelper.InvariantEnglishUS);
            }

            public List<KeyValuePair<string, string>> GetSortedPrefixMap()
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, string> pair in this.prefixMap)
                {
                    list.Add(pair);
                }
                list.Sort(new Comparison<KeyValuePair<string, string>>(XamlXmlWriter.Frame.CompareByKey));
                return list;
            }

            public bool IsEmpty()
            {
                return (this.namespaceMap.Count == 0);
            }

            public bool TryLookupNamespace(string prefix, out string ns)
            {
                if (prefix == "xml")
                {
                    ns = "http://www.w3.org/XML/1998/namespace";
                    return true;
                }
                return this.prefixMap.TryGetValue(prefix, out ns);
            }

            public bool TryLookupPrefix(string ns, out string prefix)
            {
                if (ns == "http://www.w3.org/XML/1998/namespace")
                {
                    prefix = "xml";
                    return true;
                }
                return this.namespaceMap.TryGetValue(ns, out prefix);
            }

            public XamlNodeType AllocatingNodeType { get; set; }

            public bool IsContent { get; set; }

            public bool IsObjectFromMember { get; set; }

            public XamlMember Member { get; set; }

            public XamlPropertySet Members { get; set; }

            public XamlType Type { get; set; }
        }

        private class InMember : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMember();

            private InMember()
            {
            }

            private XamlXmlWriter.Frame FindFrameWithXmlSpacePreserve(XamlXmlWriter writer)
            {
                Stack<XamlXmlWriter.Frame>.Enumerator enumerator = writer.namespaceScopes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    XamlXmlWriter.Frame current = enumerator.Current;
                    if ((current.AllocatingNodeType != XamlNodeType.GetObject) && ((current.AllocatingNodeType != XamlNodeType.StartMember) || (!current.IsContent && !XamlXmlWriter.IsImplicit(current.Member))))
                    {
                        break;
                    }
                }
                return enumerator.Current;
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                if ((writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartObject) && (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.GetObject))
                {
                    XamlXmlWriter.Frame item = new XamlXmlWriter.Frame {
                        AllocatingNodeType = XamlNodeType.StartObject
                    };
                    writer.namespaceScopes.Push(item);
                }
                writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                if ((writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartObject) && (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.GetObject))
                {
                    XamlXmlWriter.Frame item = new XamlXmlWriter.Frame {
                        AllocatingNodeType = isObjectFromMember ? XamlNodeType.GetObject : XamlNodeType.StartObject
                    };
                    writer.namespaceScopes.Push(item);
                }
                writer.namespaceScopes.Peek().Type = type;
                writer.namespaceScopes.Peek().IsObjectFromMember = isObjectFromMember;
                writer.isFirstElementOfWhitespaceSignificantCollection = false;
                if (isObjectFromMember)
                {
                    if (!writer.namespaceScopes.Peek().IsEmpty())
                    {
                        throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteObjectNotSupportedInCurrentState"));
                    }
                    XamlXmlWriter.Frame frame2 = writer.namespaceScopes.Pop();
                    XamlXmlWriter.Frame frame3 = writer.namespaceScopes.Peek();
                    writer.namespaceScopes.Push(frame2);
                    if (frame3.AllocatingNodeType == XamlNodeType.StartMember)
                    {
                        XamlType type2 = frame3.Member.Type;
                        if (((type2 != null) && !type2.IsCollection) && !type2.IsDictionary)
                        {
                            throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterIsObjectFromMemberSetForArraysOrNonCollections"));
                        }
                    }
                    writer.currentState = XamlXmlWriter.InRecord.State;
                }
                else
                {
                    XamlXmlWriter.WriterState.WriteStartElementForObject(writer, type);
                    writer.currentState = XamlXmlWriter.InRecordTryAttributes.State;
                }
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                XamlXmlWriter.Frame frame = writer.namespaceScopes.Peek();
                if (frame.AllocatingNodeType != XamlNodeType.StartMember)
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteValue" }));
                }
                if (frame.Member.DeclaringType == XamlLanguage.XData)
                {
                    writer.output.WriteRaw(value);
                    writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                }
                else if (XamlXmlWriter.HasSignificantWhitespace(value))
                {
                    XamlType containingXamlType = XamlXmlWriter.GetContainingXamlType(writer);
                    if ((containingXamlType != null) && !containingXamlType.IsWhitespaceSignificantCollection)
                    {
                        this.WriteXmlSpaceOrThrow(writer, value);
                        writer.output.WriteValue(value);
                        writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                    }
                    else if (XamlXmlWriter.ContainsConsecutiveInnerSpaces(value) || XamlXmlWriter.ContainsWhitespaceThatIsNotSpace(value))
                    {
                        if (!writer.isFirstElementOfWhitespaceSignificantCollection)
                        {
                            throw new InvalidOperationException(System.Xaml.SR.Get("WhiteSpaceInCollection", new object[] { value, containingXamlType.Name }));
                        }
                        this.WriteXmlSpaceOrThrow(writer, value);
                        writer.output.WriteValue(value);
                        writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                    }
                    else
                    {
                        if (XamlXmlWriter.ContainsLeadingSpace(value) && writer.isFirstElementOfWhitespaceSignificantCollection)
                        {
                            this.WriteXmlSpaceOrThrow(writer, value);
                            writer.output.WriteValue(value);
                            writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                        }
                        if (XamlXmlWriter.ContainsTrailingSpace(value))
                        {
                            writer.deferredValue = value;
                            writer.currentState = XamlXmlWriter.InMemberAfterValueWithSignificantWhitespace.State;
                        }
                        else
                        {
                            writer.output.WriteValue(value);
                            writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                        }
                    }
                }
                else
                {
                    writer.output.WriteValue(value);
                    writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                }
                if (writer.currentState != XamlXmlWriter.InMemberAfterValueWithSignificantWhitespace.State)
                {
                    writer.isFirstElementOfWhitespaceSignificantCollection = false;
                }
            }

            private void WriteXmlSpaceOrThrow(XamlXmlWriter writer, string value)
            {
                XamlXmlWriter.Frame frame = this.FindFrameWithXmlSpacePreserve(writer);
                if (frame.AllocatingNodeType == XamlNodeType.StartMember)
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("CannotWriteXmlSpacePreserveOnMember", new object[] { frame.Member, value }));
                }
                XamlXmlWriter.WriteXmlSpace(writer);
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberAfterEndObject : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMemberAfterEndObject();

            private InMemberAfterEndObject()
            {
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                writer.currentState.WriteEndMember(writer);
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteValue(writer, value);
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberAfterValue : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMemberAfterValue();

            private InMemberAfterValue()
            {
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                XamlXmlWriter.Frame frame = writer.namespaceScopes.Pop();
                if (!XamlXmlWriter.IsImplicit(frame.Member) && !frame.IsContent)
                {
                    writer.output.WriteEndElement();
                }
                writer.currentState = XamlXmlWriter.InRecord.State;
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberAfterValueWithSignificantWhitespace : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMemberAfterValueWithSignificantWhitespace();

            private InMemberAfterValueWithSignificantWhitespace()
            {
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                if (writer.isFirstElementOfWhitespaceSignificantCollection)
                {
                    XamlXmlWriter.WriteXmlSpace(writer);
                    writer.output.WriteValue(writer.deferredValue);
                    writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                    writer.currentState.WriteEndMember(writer);
                    writer.isFirstElementOfWhitespaceSignificantCollection = false;
                }
                else
                {
                    XamlType containingXamlType = XamlXmlWriter.GetContainingXamlType(writer);
                    throw new InvalidOperationException(System.Xaml.SR.Get("WhiteSpaceInCollection", new object[] { writer.deferredValue, containingXamlType.Name }));
                }
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.output.WriteValue(writer.deferredValue);
                writer.currentState = XamlXmlWriter.InMemberAfterValue.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberAttributedMember : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMemberAttributedMember();

            private InMemberAttributedMember()
            {
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                XamlXmlWriter.WriterState.WriteMemberAsAttribute(writer);
                if (!writer.deferredValueIsME && XamlXmlWriter.StringStartsWithCurly(writer.deferredValue))
                {
                    writer.output.WriteValue("{}" + writer.deferredValue);
                }
                else
                {
                    writer.output.WriteValue(writer.deferredValue);
                }
                writer.namespaceScopes.Pop();
                writer.output.WriteEndAttribute();
                writer.currentState = XamlXmlWriter.InRecordTryAttributes.State;
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberTryAttributes : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMemberTryAttributes();

            private InMemberTryAttributes()
            {
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                if (((type != null) && type.IsMarkupExtension) && !type.IsGeneric)
                {
                    writer.meWriter.Reset();
                    writer.meNodesStack.Push(new List<XamlNode>());
                    writer.currentState = XamlXmlWriter.TryCurlyForm.State;
                    writer.currentState.WriteObject(writer, type, isObjectFromMember);
                }
                else
                {
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                    writer.currentState = XamlXmlWriter.InMember.State;
                    writer.currentState.WriteObject(writer, type, isObjectFromMember);
                }
                writer.isFirstElementOfWhitespaceSignificantCollection = false;
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.deferredValue = value;
                writer.deferredValueIsME = false;
                writer.currentState = XamlXmlWriter.InMemberTryAttributesAfterValue.State;
                writer.isFirstElementOfWhitespaceSignificantCollection = false;
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InMemberTryAttributesAfterValue : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InMemberTryAttributesAfterValue();

            private InMemberTryAttributesAfterValue()
            {
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.currentState = XamlXmlWriter.InMemberAttributedMember.State;
                writer.currentState.WriteEndMember(writer);
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                writer.output.WriteValue(writer.deferredValue);
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                writer.output.WriteValue(writer.deferredValue);
                writer.isFirstElementOfWhitespaceSignificantCollection = false;
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InRecord : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InRecord();

            private InRecord()
            {
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                XamlXmlWriter.Frame frame = writer.namespaceScopes.Pop();
                if ((frame.AllocatingNodeType != XamlNodeType.StartObject) && (frame.AllocatingNodeType != XamlNodeType.GetObject))
                {
                    throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteEndObject" }));
                }
                if (!frame.IsObjectFromMember)
                {
                    writer.output.WriteEndElement();
                }
                if (writer.namespaceScopes.Count > 0)
                {
                    writer.currentState = XamlXmlWriter.InMemberAfterEndObject.State;
                }
                else
                {
                    writer.Flush();
                    writer.currentState = XamlXmlWriter.End.State;
                }
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartMember)
                {
                    XamlXmlWriter.Frame item = new XamlXmlWriter.Frame {
                        AllocatingNodeType = XamlNodeType.StartMember,
                        Type = writer.namespaceScopes.Peek().Type
                    };
                    writer.namespaceScopes.Push(item);
                }
                writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                writer.CheckMemberForUniqueness(property);
                if (writer.namespaceScopes.Peek().AllocatingNodeType != XamlNodeType.StartMember)
                {
                    XamlXmlWriter.Frame item = new XamlXmlWriter.Frame {
                        AllocatingNodeType = XamlNodeType.StartMember,
                        Type = writer.namespaceScopes.Peek().Type
                    };
                    writer.namespaceScopes.Push(item);
                }
                writer.namespaceScopes.Peek().Member = property;
                XamlType type = writer.namespaceScopes.Peek().Type;
                if ((((property == XamlLanguage.Items) && (type != null)) && type.IsWhitespaceSignificantCollection) || (property == XamlLanguage.UnknownContent))
                {
                    writer.isFirstElementOfWhitespaceSignificantCollection = true;
                }
                XamlType type2 = writer.namespaceScopes.Peek().Type;
                if (XamlXmlWriter.IsImplicit(property))
                {
                    if (!writer.namespaceScopes.Peek().IsEmpty())
                    {
                        throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteStartMember" }));
                    }
                }
                else
                {
                    if (property == type2.ContentProperty)
                    {
                        if (!writer.namespaceScopes.Peek().IsEmpty())
                        {
                            throw new InvalidOperationException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteStartMember" }));
                        }
                        writer.currentState = XamlXmlWriter.TryContentProperty.State;
                        return;
                    }
                    XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                    writer.WriteDeferredNamespaces(XamlNodeType.StartMember);
                }
                if (property == XamlLanguage.PositionalParameters)
                {
                    writer.namespaceScopes.Pop();
                    if ((type2 != null) && type2.ConstructionRequiresArguments)
                    {
                        throw new XamlXmlWriterException(System.Xaml.SR.Get("ExpandPositionalParametersinTypeWithNoDefaultConstructor"));
                    }
                    writer.ppStateInfo.ReturnState = State;
                    writer.currentState = XamlXmlWriter.ExpandPositionalParameters.State;
                }
                else
                {
                    writer.currentState = XamlXmlWriter.InMember.State;
                }
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class InRecordTryAttributes : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.InRecordTryAttributes();

            private InRecordTryAttributes()
            {
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                writer.currentState = XamlXmlWriter.InRecord.State;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState.WriteEndObject(writer);
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.currentState = XamlXmlWriter.InRecord.State;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                XamlType type = writer.namespaceScopes.Peek().Type;
                if ((((property == XamlLanguage.Items) && (type != null)) && type.IsWhitespaceSignificantCollection) || (property == XamlLanguage.UnknownContent))
                {
                    writer.isFirstElementOfWhitespaceSignificantCollection = true;
                }
                if (property.IsAttachable || property.IsDirective)
                {
                    string str;
                    string prefix = writer.LookupPrefix(property.GetXamlNamespaces(), out str);
                    if ((prefix == null) || writer.IsShadowed(str, prefix))
                    {
                        writer.currentState = XamlXmlWriter.InRecord.State;
                        writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                        writer.currentState.WriteStartMember(writer, property);
                        return;
                    }
                }
                writer.CheckMemberForUniqueness(property);
                XamlXmlWriter.Frame item = new XamlXmlWriter.Frame {
                    AllocatingNodeType = XamlNodeType.StartMember,
                    Type = writer.namespaceScopes.Peek().Type,
                    Member = property
                };
                writer.namespaceScopes.Push(item);
                XamlType type2 = writer.namespaceScopes.Peek().Type;
                if (property == XamlLanguage.PositionalParameters)
                {
                    writer.namespaceScopes.Pop();
                    if ((type2 != null) && type2.ConstructionRequiresArguments)
                    {
                        throw new XamlXmlWriterException(System.Xaml.SR.Get("ExpandPositionalParametersinTypeWithNoDefaultConstructor"));
                    }
                    writer.ppStateInfo.ReturnState = State;
                    writer.currentState = XamlXmlWriter.ExpandPositionalParameters.State;
                }
                else if (XamlXmlWriter.IsImplicit(property))
                {
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    writer.currentState = XamlXmlWriter.InMember.State;
                }
                else if (property == type2.ContentProperty)
                {
                    writer.currentState = XamlXmlWriter.TryContentPropertyInTryAttributesState.State;
                }
                else if ((property.IsDirective && (property.Type != null)) && (property.Type.IsCollection || property.Type.IsDictionary))
                {
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                    writer.currentState = XamlXmlWriter.InMember.State;
                }
                else
                {
                    writer.currentState = XamlXmlWriter.InMemberTryAttributes.State;
                }
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class PositionalParameterStateInfo
        {
            public PositionalParameterStateInfo(XamlXmlWriter xamlXmlWriter)
            {
                XamlMarkupExtensionWriterSettings meSettings = new XamlMarkupExtensionWriterSettings {
                    ContinueWritingWhenPrefixIsNotFound = true
                };
                this.Writer = new XamlMarkupExtensionWriter(xamlXmlWriter, meSettings);
                this.Reset();
            }

            public void Reset()
            {
                this.NodesList = new List<List<XamlNode>>();
                this.Writer.Reset();
                this.Writer.WriteStartObject(XamlLanguage.MarkupExtension);
                this.Writer.WriteStartMember(XamlLanguage.PositionalParameters);
                this.CurrentDepth = 0;
            }

            public int CurrentDepth { get; set; }

            public List<List<XamlNode>> NodesList { get; set; }

            public XamlXmlWriter.WriterState ReturnState { get; set; }

            public XamlMarkupExtensionWriter Writer { get; set; }
        }

        private class Start : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.Start();

            private Start()
            {
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.AssignNamespacePrefix(namespaceDeclaration.Namespace, namespaceDeclaration.Prefix);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.namespaceScopes.Peek().Type = type;
                writer.namespaceScopes.Peek().IsObjectFromMember = isObjectFromMember;
                if (isObjectFromMember)
                {
                    throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteObjectNotSupportedInCurrentState"));
                }
                XamlXmlWriter.WriterState.WriteStartElementForObject(writer, type);
                writer.currentState = XamlXmlWriter.InRecordTryAttributes.State;
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class TryContentProperty : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.TryContentProperty();

            private TryContentProperty()
            {
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                XamlMember member = writer.namespaceScopes.Peek().Member;
                if (XamlLanguage.String.CanAssignTo(member.Type))
                {
                    writer.namespaceScopes.Peek().IsContent = true;
                }
                else
                {
                    writer.namespaceScopes.Peek().IsContent = false;
                    XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                }
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteValue(writer, value);
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class TryContentPropertyInTryAttributesState : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.TryContentPropertyInTryAttributesState();

            private TryContentPropertyInTryAttributesState()
            {
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteNamespace(writer, namespaceDeclaration);
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                writer.namespaceScopes.Peek().IsContent = true;
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                writer.currentState = XamlXmlWriter.InMember.State;
                writer.currentState.WriteObject(writer, type, isObjectFromMember);
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                XamlMember member = writer.namespaceScopes.Peek().Member;
                if (XamlLanguage.String.CanAssignTo(member.Type) && (value != string.Empty))
                {
                    writer.namespaceScopes.Peek().IsContent = true;
                    writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                    writer.currentState = XamlXmlWriter.InMember.State;
                    writer.currentState.WriteValue(writer, value);
                }
                else
                {
                    writer.namespaceScopes.Peek().IsContent = false;
                    writer.currentState = XamlXmlWriter.InMemberTryAttributes.State;
                    writer.currentState.WriteValue(writer, value);
                }
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private class TryCurlyForm : XamlXmlWriter.WriterState
        {
            private static XamlXmlWriter.WriterState state = new XamlXmlWriter.TryCurlyForm();

            private TryCurlyForm()
            {
            }

            public override void WriteEndMember(XamlXmlWriter writer)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.EndMember));
                writer.meWriter.WriteEndMember();
                if (writer.meWriter.Failed)
                {
                    this.WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteEndObject(XamlXmlWriter writer)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.EndObject));
                writer.meWriter.WriteEndObject();
                if (writer.meWriter.Failed)
                {
                    this.WriteNodesInXmlForm(writer);
                }
                if (writer.meWriter.MarkupExtensionString != null)
                {
                    writer.meNodesStack.Pop();
                    writer.deferredValue = writer.meWriter.MarkupExtensionString;
                    writer.deferredValueIsME = true;
                    writer.currentState = XamlXmlWriter.InMemberTryAttributesAfterValue.State;
                }
            }

            public override void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.NamespaceDeclaration, namespaceDeclaration));
                writer.meWriter.WriteNamespace(namespaceDeclaration);
                if (writer.meWriter.Failed)
                {
                    this.WriteNodesInXmlForm(writer);
                }
            }

            private void WriteNodesInXmlForm(XamlXmlWriter writer)
            {
                writer.WriteDeferredNamespaces(XamlNodeType.StartObject);
                XamlXmlWriter.WriterState.WriteMemberAsElement(writer);
                writer.currentState = XamlXmlWriter.InMember.State;
                foreach (XamlNode node in writer.meNodesStack.Pop())
                {
                    writer.currentState.WriteNode(writer, node);
                }
            }

            public override void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                if (!isObjectFromMember)
                {
                    writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.StartObject, type));
                    writer.meWriter.WriteStartObject(type);
                }
                else
                {
                    writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.GetObject));
                    writer.meWriter.WriteGetObject();
                }
                if (writer.meWriter.Failed)
                {
                    this.WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.StartMember, property));
                writer.meWriter.WriteStartMember(property);
                if (writer.meWriter.Failed)
                {
                    this.WriteNodesInXmlForm(writer);
                }
            }

            public override void WriteValue(XamlXmlWriter writer, string value)
            {
                writer.meNodesStack.Peek().Add(new XamlNode(XamlNodeType.Value, value));
                writer.meWriter.WriteValue(value);
                if (writer.meWriter.Failed)
                {
                    this.WriteNodesInXmlForm(writer);
                }
            }

            public static XamlXmlWriter.WriterState State
            {
                get
                {
                    return state;
                }
            }
        }

        private abstract class WriterState
        {
            protected WriterState()
            {
            }

            public virtual void WriteEndMember(XamlXmlWriter writer)
            {
                throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteEndMember" }));
            }

            public virtual void WriteEndObject(XamlXmlWriter writer)
            {
                throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteEndObject" }));
            }

            protected static void WriteMemberAsAttribute(XamlXmlWriter writer)
            {
                XamlXmlWriter.Frame frame = writer.namespaceScopes.Peek();
                XamlType type = frame.Type;
                XamlMember member = frame.Member;
                string name = member.Name;
                if (member.IsDirective)
                {
                    string str2;
                    string prefix = writer.FindPrefix(member.GetXamlNamespaces(), out str2);
                    WriteStartAttribute(writer, prefix, name, str2);
                }
                else if (member.IsAttachable)
                {
                    string str4;
                    string str5 = writer.FindPrefix(member.GetXamlNamespaces(), out str4);
                    if (member.DeclaringType == type)
                    {
                        name = member.Name;
                    }
                    else
                    {
                        name = XamlXmlWriter.GetTypeName(member.DeclaringType) + "." + member.Name;
                    }
                    WriteStartAttribute(writer, str5, name, str4);
                }
                else
                {
                    writer.output.WriteStartAttribute(name);
                }
            }

            protected static void WriteMemberAsElement(XamlXmlWriter writer)
            {
                string str;
                XamlXmlWriter.Frame frame = writer.namespaceScopes.Peek();
                XamlType type = frame.Type;
                XamlMember member = frame.Member;
                XamlType type2 = member.IsAttachable ? member.DeclaringType : type;
                string prefix = (member.IsAttachable || member.IsDirective) ? writer.FindPrefix(member.GetXamlNamespaces(), out str) : writer.FindPrefix(type.GetXamlNamespaces(), out str);
                string localName = member.IsDirective ? member.Name : (XamlXmlWriter.GetTypeName(type2) + "." + member.Name);
                writer.output.WriteStartElement(prefix, localName, str);
            }

            public virtual void WriteNamespace(XamlXmlWriter writer, System.Xaml.NamespaceDeclaration namespaceDeclaration)
            {
                throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteNamespace" }));
            }

            protected internal void WriteNode(XamlXmlWriter writer, XamlNode node)
            {
                switch (node.NodeType)
                {
                    case XamlNodeType.None:
                        return;

                    case XamlNodeType.StartObject:
                        writer.currentState.WriteObject(writer, node.XamlType, false);
                        return;

                    case XamlNodeType.GetObject:
                    {
                        XamlType type = null;
                        XamlXmlWriter.Frame frame = writer.namespaceScopes.Peek();
                        if (frame.AllocatingNodeType == XamlNodeType.StartMember)
                        {
                            type = frame.Member.Type;
                        }
                        writer.currentState.WriteObject(writer, type, true);
                        return;
                    }
                    case XamlNodeType.EndObject:
                        writer.currentState.WriteEndObject(writer);
                        return;

                    case XamlNodeType.StartMember:
                        writer.currentState.WriteStartMember(writer, node.Member);
                        return;

                    case XamlNodeType.EndMember:
                        writer.currentState.WriteEndMember(writer);
                        return;

                    case XamlNodeType.Value:
                        writer.currentState.WriteValue(writer, node.Value as string);
                        return;

                    case XamlNodeType.NamespaceDeclaration:
                        writer.currentState.WriteNamespace(writer, node.NamespaceDeclaration);
                        return;
                }
                throw new NotSupportedException(System.Xaml.SR.Get("MissingCaseXamlNodes"));
            }

            public virtual void WriteObject(XamlXmlWriter writer, XamlType type, bool isObjectFromMember)
            {
                throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteObject" }));
            }

            private static void WriteStartAttribute(XamlXmlWriter writer, string prefix, string local, string ns)
            {
                if (prefix == string.Empty)
                {
                    writer.output.WriteStartAttribute(local);
                }
                else
                {
                    writer.output.WriteStartAttribute(prefix, local, ns);
                }
            }

            protected static void WriteStartElementForObject(XamlXmlWriter writer, XamlType type)
            {
                string str2;
                string typeName = XamlXmlWriter.GetTypeName(type);
                string prefix = writer.FindPrefix(type.GetXamlNamespaces(), out str2);
                writer.output.WriteStartElement(prefix, typeName, str2);
            }

            public virtual void WriteStartMember(XamlXmlWriter writer, XamlMember property)
            {
                throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteStartMember" }));
            }

            public virtual void WriteValue(XamlXmlWriter writer, string value)
            {
                throw new XamlXmlWriterException(System.Xaml.SR.Get("XamlXmlWriterWriteNotSupportedInCurrentState", new object[] { "WriteValue" }));
            }
        }
    }
}

