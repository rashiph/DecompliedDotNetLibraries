namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class DynamicActivityXamlReader : XamlReader, IXamlLineInfo
    {
        private readonly XamlMember activityPropertyAttributes;
        private readonly XamlMember activityPropertyName;
        private readonly XamlMember activityPropertyType;
        private readonly XamlMember activityPropertyValue;
        private readonly XamlType activityPropertyXamlType;
        private XamlType activityReplacementXamlType;
        private XamlType activityXamlType;
        private readonly XamlType baseActivityXamlType;
        private BufferedPropertyList bufferedProperties;
        private bool bufferMembers;
        private BuilderMemberNode currentBuilderMember;
        private int depth;
        private bool frontLoadedDirectives;
        private bool hasLineInfo;
        private readonly XamlReader innerReader;
        private IXamlLineInfo innerReaderLineInfo;
        private int inXClassDepth;
        private bool isBuilder;
        private readonly NamespaceTable namespaceTable;
        private XamlNodeQueue nodeQueue;
        private XamlReader nodeReader;
        private IXamlLineInfo nodeReaderLineInfo;
        private bool notRewriting;
        private XamlSchemaContext schemaContext;
        private readonly XamlType typeXamlType;
        private readonly XamlType xamlTypeXamlType;
        private XamlTypeName xClassName;
        internal static readonly XamlMember xPropertyAttributes = XamlLanguage.Property.GetMember("Attributes");
        internal static readonly XamlMember xPropertyName = XamlLanguage.Property.GetMember("Name");
        internal static readonly XamlMember xPropertyType = XamlLanguage.Property.GetMember("Type");

        public DynamicActivityXamlReader(XamlReader innerReader) : this(innerReader, null)
        {
        }

        public DynamicActivityXamlReader(XamlReader innerReader, XamlSchemaContext schemaContext) : this(false, innerReader, schemaContext)
        {
        }

        public DynamicActivityXamlReader(bool isBuilder, XamlReader innerReader, XamlSchemaContext schemaContext)
        {
            this.isBuilder = isBuilder;
            this.innerReader = innerReader;
            this.schemaContext = schemaContext ?? innerReader.SchemaContext;
            this.xamlTypeXamlType = this.schemaContext.GetXamlType(typeof(XamlType));
            this.typeXamlType = this.schemaContext.GetXamlType(typeof(System.Type));
            this.baseActivityXamlType = this.schemaContext.GetXamlType(typeof(Activity));
            this.activityPropertyXamlType = this.schemaContext.GetXamlType(typeof(DynamicActivityProperty));
            this.activityPropertyType = this.activityPropertyXamlType.GetMember("Type");
            this.activityPropertyName = this.activityPropertyXamlType.GetMember("Name");
            this.activityPropertyValue = this.activityPropertyXamlType.GetMember("Value");
            this.activityPropertyAttributes = this.activityPropertyXamlType.GetMember("Attributes");
            this.namespaceTable = new NamespaceTable();
            this.frontLoadedDirectives = true;
            this.nodeQueue = new XamlNodeQueue(this.schemaContext);
            this.nodeReader = this.nodeQueue.Reader;
            IXamlLineInfo info = innerReader as IXamlLineInfo;
            if ((info != null) && info.HasLineInfo)
            {
                this.innerReaderLineInfo = info;
                this.nodeReaderLineInfo = (IXamlLineInfo) this.nodeQueue.Reader;
                this.hasLineInfo = true;
            }
        }

        private static XamlException CreateXamlException(string message, IXamlLineInfo lineInfo)
        {
            if ((lineInfo != null) && lineInfo.HasLineInfo)
            {
                return new XamlException(message, null, lineInfo.LineNumber, lineInfo.LinePosition);
            }
            return new XamlException(message);
        }

        private static void DecrementIfPositive(ref int a)
        {
            if (a > 0)
            {
                a--;
            }
        }

        private void DisableRewrite()
        {
            this.notRewriting = true;
            this.nodeReader = this.innerReader;
            this.nodeReaderLineInfo = this.innerReader as IXamlLineInfo;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.innerReader.Close();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void EnterObject()
        {
            this.depth++;
            if (this.depth >= 2)
            {
                this.frontLoadedDirectives = false;
            }
        }

        private void ExitObject()
        {
            if (this.depth < this.inXClassDepth)
            {
                this.inXClassDepth = 0;
            }
            this.depth--;
            if ((this.currentBuilderMember != null) && (this.currentBuilderMember.Depth == this.depth))
            {
                this.currentBuilderMember.FlushBuffer(this.nodeQueue.Writer);
                this.currentBuilderMember = null;
            }
            this.frontLoadedDirectives = false;
            if ((this.depth == 0) && (this.bufferedProperties != null))
            {
                this.bufferedProperties.FlushTo(this.nodeQueue, this);
            }
        }

        private static void IncrementIfPositive(ref int a)
        {
            if (a > 0)
            {
                a++;
            }
        }

        private bool IsXClassName(XamlType xamlType)
        {
            if (((xamlType == null) || (this.xClassName == null)) || (xamlType.Name != this.xClassName.Name))
            {
                return false;
            }
            string preferredXamlNamespace = xamlType.PreferredXamlNamespace;
            if (!preferredXamlNamespace.Contains("clr-namespace:"))
            {
                return false;
            }
            preferredXamlNamespace = preferredXamlNamespace.Substring("clr-namespace:".Length);
            int index = preferredXamlNamespace.IndexOf(';');
            if ((index < 0) || (index > preferredXamlNamespace.Length))
            {
                index = preferredXamlNamespace.Length;
            }
            string str2 = preferredXamlNamespace.Substring(0, index);
            return (this.xClassName.Namespace == str2);
        }

        private bool ProcessCurrentNode()
        {
            bool flag = false;
            this.namespaceTable.ManageNamespace(this.innerReader);
            switch (this.innerReader.NodeType)
            {
                case XamlNodeType.StartObject:
                    this.EnterObject();
                    if (this.depth != 1)
                    {
                        goto Label_049A;
                    }
                    if (!(this.innerReader.Type.UnderlyingType == typeof(Activity)))
                    {
                        if ((this.innerReader.Type.IsGeneric && (this.innerReader.Type.UnderlyingType != null)) && (this.innerReader.Type.UnderlyingType.GetGenericTypeDefinition() == typeof(Activity<>)))
                        {
                            System.Type type2;
                            this.activityXamlType = this.innerReader.Type;
                            System.Type underlyingType = this.innerReader.Type.TypeArguments[0].UnderlyingType;
                            if (this.isBuilder)
                            {
                                type2 = typeof(ActivityBuilder<>).MakeGenericType(new System.Type[] { underlyingType });
                            }
                            else
                            {
                                type2 = typeof(DynamicActivity<>).MakeGenericType(new System.Type[] { underlyingType });
                            }
                            this.activityReplacementXamlType = this.SchemaContext.GetXamlType(type2);
                        }
                        else
                        {
                            this.DisableRewrite();
                            return false;
                        }
                        break;
                    }
                    this.activityXamlType = this.innerReader.Type;
                    if (!this.isBuilder)
                    {
                        this.activityReplacementXamlType = this.SchemaContext.GetXamlType(typeof(DynamicActivity));
                        break;
                    }
                    this.activityReplacementXamlType = this.SchemaContext.GetXamlType(typeof(ActivityBuilder));
                    break;

                case XamlNodeType.GetObject:
                    this.EnterObject();
                    goto Label_049A;

                case XamlNodeType.EndObject:
                case XamlNodeType.EndMember:
                    this.ExitObject();
                    goto Label_049A;

                case XamlNodeType.StartMember:
                {
                    XamlMember member = this.innerReader.Member;
                    if (!this.IsXClassName(member.DeclaringType))
                    {
                        if (this.frontLoadedDirectives && (member == XamlLanguage.FactoryMethod))
                        {
                            this.DisableRewrite();
                            return false;
                        }
                        this.depth++;
                        if (this.depth != 2)
                        {
                            if (this.bufferMembers)
                            {
                                this.currentBuilderMember = new BuilderMemberNode(this, this.depth);
                                this.innerReader.Read();
                                return true;
                            }
                        }
                        else if ((member.DeclaringType != this.activityXamlType) && (member.DeclaringType != this.baseActivityXamlType))
                        {
                            if (member != XamlLanguage.Class)
                            {
                                if (member == XamlLanguage.Members)
                                {
                                    if (this.bufferedProperties == null)
                                    {
                                        this.bufferedProperties = new BufferedPropertyList(this);
                                    }
                                    this.bufferedProperties.BufferDefinitions(this);
                                    this.depth--;
                                    return true;
                                }
                                if (member == XamlLanguage.ClassAttributes)
                                {
                                    this.nodeQueue.Writer.WriteStartMember(this.activityReplacementXamlType.GetMember("Attributes"), this.innerReaderLineInfo);
                                    this.WriteWrappedMember(false);
                                    flag = true;
                                    return true;
                                }
                            }
                            else
                            {
                                this.inXClassDepth = this.depth;
                                this.nodeQueue.Writer.WriteStartMember(this.activityReplacementXamlType.GetMember("Name"), this.innerReaderLineInfo);
                                flag = true;
                            }
                        }
                        else
                        {
                            XamlMember xamlMember = this.activityReplacementXamlType.GetMember(member.Name);
                            if (xamlMember == null)
                            {
                                throw FxTrace.Exception.AsError(CreateXamlException(System.Activities.SR.MemberNotSupportedByActivityXamlServices(member.Name), this.innerReaderLineInfo));
                            }
                            this.nodeQueue.Writer.WriteStartMember(xamlMember, this.innerReaderLineInfo);
                            if (xamlMember.Name == "Constraints")
                            {
                                this.WriteWrappedMember(true);
                                flag = true;
                                return true;
                            }
                            flag = true;
                            if (this.isBuilder && (xamlMember.Name == "Implementation"))
                            {
                                this.bufferMembers = true;
                            }
                        }
                        goto Label_049A;
                    }
                    if (this.bufferedProperties == null)
                    {
                        this.bufferedProperties = new BufferedPropertyList(this);
                    }
                    this.bufferedProperties.BufferDefaultValue(member.Name, this.activityPropertyValue, this.innerReader, this.innerReaderLineInfo);
                    return true;
                }
                case XamlNodeType.Value:
                    if ((this.inXClassDepth >= this.depth) && (this.xClassName == null))
                    {
                        string str = (string) this.innerReader.Value;
                        string xamlNamespace = "";
                        string name = str;
                        int length = str.LastIndexOf('.');
                        if (length > 0)
                        {
                            xamlNamespace = str.Substring(0, length);
                            name = str.Substring(length + 1);
                        }
                        this.xClassName = new XamlTypeName(xamlNamespace, name);
                    }
                    goto Label_049A;

                default:
                    goto Label_049A;
            }
            this.nodeQueue.Writer.WriteStartObject(this.activityReplacementXamlType, this.innerReaderLineInfo);
            flag = true;
        Label_049A:
            if (!flag)
            {
                if ((this.currentBuilderMember != null) && (this.depth >= this.currentBuilderMember.Depth))
                {
                    bool flag2 = this.currentBuilderMember.ProcessNode(this.innerReader, this.nodeQueue.Writer, this.depth, this.innerReaderLineInfo);
                    if (!flag2)
                    {
                        bool exitObject = this.currentBuilderMember.ExitObject;
                        this.currentBuilderMember = null;
                        if (exitObject)
                        {
                            this.ExitObject();
                            this.ExitObject();
                        }
                    }
                    return flag2;
                }
                this.nodeQueue.Writer.WriteNode(this.innerReader, this.innerReaderLineInfo);
            }
            return false;
        }

        public override bool Read()
        {
            if (!this.notRewriting)
            {
                bool flag = this.innerReader.Read();
                for (bool flag2 = true; flag2 && !this.innerReader.IsEof; flag2 = this.ProcessCurrentNode())
                {
                }
                if (this.notRewriting)
                {
                    return flag;
                }
            }
            return this.nodeReader.Read();
        }

        private void WriteWrappedMember(bool stripWhitespace)
        {
            this.nodeQueue.Writer.WriteGetObject(this.innerReaderLineInfo);
            this.nodeQueue.Writer.WriteStartMember(XamlLanguage.Items, this.innerReaderLineInfo);
            XamlReader reader = this.innerReader.ReadSubtree();
            reader.Read();
            reader.Read();
            while (!reader.IsEof)
            {
                bool flag = false;
                if (reader.NodeType == XamlNodeType.Value)
                {
                    string str = reader.Value as string;
                    if ((str != null) && (str.Trim().Length == 0))
                    {
                        flag = true;
                    }
                }
                if (flag && stripWhitespace)
                {
                    reader.Read();
                }
                else
                {
                    XamlWriterExtensions.Transform(reader.ReadSubtree(), this.nodeQueue.Writer, this.innerReaderLineInfo, false);
                }
            }
            this.nodeQueue.Writer.WriteEndObject(this.innerReaderLineInfo);
            this.nodeQueue.Writer.WriteEndMember(this.innerReaderLineInfo);
            reader.Close();
            this.ExitObject();
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
                return this.nodeReader.IsEof;
            }
        }

        public int LineNumber
        {
            get
            {
                if (this.hasLineInfo)
                {
                    return this.nodeReaderLineInfo.LineNumber;
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
                    return this.nodeReaderLineInfo.LinePosition;
                }
                return 0;
            }
        }

        public override XamlMember Member
        {
            get
            {
                return this.nodeReader.Member;
            }
        }

        public override NamespaceDeclaration Namespace
        {
            get
            {
                return this.nodeReader.Namespace;
            }
        }

        public override XamlNodeType NodeType
        {
            get
            {
                return this.nodeReader.NodeType;
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.schemaContext;
            }
        }

        public override XamlType Type
        {
            get
            {
                return this.nodeReader.Type;
            }
        }

        public override object Value
        {
            get
            {
                return this.nodeReader.Value;
            }
        }

        private class BufferedPropertyList
        {
            private bool alreadyBufferedDefinitions;
            private XamlNodeQueue outerNodes;
            private DynamicActivityXamlReader parent;
            private Dictionary<string, ActivityPropertyHolder> propertyHolders;
            private Dictionary<string, ValueHolder> valueHolders;

            public BufferedPropertyList(DynamicActivityXamlReader parent)
            {
                this.parent = parent;
                this.outerNodes = new XamlNodeQueue(parent.SchemaContext);
            }

            public void BufferDefaultValue(string propertyName, XamlMember propertyValue, XamlReader reader, IXamlLineInfo lineInfo)
            {
                if (this.alreadyBufferedDefinitions)
                {
                    this.ProcessDefaultValue(propertyName, propertyValue, reader.ReadSubtree(), lineInfo);
                }
                else
                {
                    if (this.valueHolders == null)
                    {
                        this.valueHolders = new Dictionary<string, ValueHolder>();
                    }
                    ValueHolder holder = new ValueHolder(this.parent.SchemaContext, propertyValue, reader, lineInfo);
                    this.valueHolders[propertyName] = holder;
                }
            }

            public void BufferDefinitions(DynamicActivityXamlReader parent)
            {
                XamlReader reader = parent.innerReader.ReadSubtree();
                IXamlLineInfo innerReaderLineInfo = parent.innerReaderLineInfo;
                reader.Read();
                this.outerNodes.Writer.WriteStartMember(parent.activityReplacementXamlType.GetMember("Properties"), innerReaderLineInfo);
                this.outerNodes.Writer.WriteGetObject(innerReaderLineInfo);
                this.outerNodes.Writer.WriteStartMember(XamlLanguage.Items, innerReaderLineInfo);
                bool flag = reader.Read();
                while (flag)
                {
                    if ((reader.NodeType == XamlNodeType.StartObject) && (reader.Type == XamlLanguage.Property))
                    {
                        ActivityPropertyHolder holder = new ActivityPropertyHolder(parent, reader.ReadSubtree());
                        this.PropertyHolders.Add(holder.Name, holder);
                        this.outerNodes.Writer.WriteValue(holder, innerReaderLineInfo);
                    }
                    else
                    {
                        this.outerNodes.Writer.WriteNode(reader, innerReaderLineInfo);
                        flag = reader.Read();
                    }
                }
                this.outerNodes.Writer.WriteEndObject(innerReaderLineInfo);
                this.outerNodes.Writer.WriteEndMember(innerReaderLineInfo);
                reader.Close();
                this.alreadyBufferedDefinitions = true;
                this.FlushValueHolders(parent);
            }

            public void FlushTo(XamlNodeQueue targetNodeQueue, DynamicActivityXamlReader parent)
            {
                this.FlushValueHolders(parent);
                XamlReader reader = this.outerNodes.Reader;
                IXamlLineInfo readerInfo = parent.hasLineInfo ? (reader as IXamlLineInfo) : null;
                while (reader.Read())
                {
                    if (reader.NodeType == XamlNodeType.Value)
                    {
                        ActivityPropertyHolder holder = reader.Value as ActivityPropertyHolder;
                        if (holder != null)
                        {
                            holder.CopyTo(targetNodeQueue, readerInfo);
                            continue;
                        }
                    }
                    targetNodeQueue.Writer.WriteNode(reader, readerInfo);
                }
            }

            private void FlushValueHolders(DynamicActivityXamlReader parent)
            {
                if (this.valueHolders != null)
                {
                    foreach (KeyValuePair<string, ValueHolder> pair in this.valueHolders)
                    {
                        this.ProcessDefaultValue(pair.Key, pair.Value.PropertyValue, pair.Value.ValueReader, parent.innerReaderLineInfo);
                    }
                    this.valueHolders = null;
                }
            }

            public void ProcessDefaultValue(string propertyName, XamlMember propertyValue, XamlReader reader, IXamlLineInfo lineInfo)
            {
                ActivityPropertyHolder holder;
                if (!this.PropertyHolders.TryGetValue(propertyName, out holder))
                {
                    throw FxTrace.Exception.AsError(DynamicActivityXamlReader.CreateXamlException(System.Activities.SR.InvalidProperty(propertyName), lineInfo));
                }
                holder.ProcessDefaultValue(propertyValue, reader, lineInfo);
            }

            private Dictionary<string, ActivityPropertyHolder> PropertyHolders
            {
                get
                {
                    if (this.propertyHolders == null)
                    {
                        this.propertyHolders = new Dictionary<string, ActivityPropertyHolder>();
                    }
                    return this.propertyHolders;
                }
            }

            private class ActivityPropertyHolder
            {
                private XamlNodeQueue nodes;
                private DynamicActivityXamlReader parent;

                public ActivityPropertyHolder(DynamicActivityXamlReader parent, XamlReader reader)
                {
                    this.parent = parent;
                    this.nodes = new XamlNodeQueue(parent.SchemaContext);
                    IXamlLineInfo innerReaderLineInfo = parent.innerReaderLineInfo;
                    reader.Read();
                    this.nodes.Writer.WriteStartObject(parent.activityPropertyXamlType, innerReaderLineInfo);
                    int num = 1;
                    int a = 0;
                    int num3 = 0;
                    bool flag = reader.Read();
                    while (flag)
                    {
                        XamlMember activityPropertyName;
                        switch (reader.NodeType)
                        {
                            case XamlNodeType.StartObject:
                            case XamlNodeType.GetObject:
                            {
                                num++;
                                DynamicActivityXamlReader.IncrementIfPositive(ref a);
                                DynamicActivityXamlReader.IncrementIfPositive(ref num3);
                                if ((num3 <= 0) || !(reader.Type == parent.xamlTypeXamlType))
                                {
                                    goto Label_0231;
                                }
                                this.nodes.Writer.WriteStartObject(parent.typeXamlType, innerReaderLineInfo);
                                flag = reader.Read();
                                continue;
                            }
                            case XamlNodeType.EndObject:
                            {
                                num--;
                                if (num != 0)
                                {
                                    goto Label_0213;
                                }
                                flag = reader.Read();
                                continue;
                            }
                            case XamlNodeType.StartMember:
                                if (!(reader.Member.DeclaringType == XamlLanguage.Property))
                                {
                                    goto Label_0231;
                                }
                                activityPropertyName = reader.Member;
                                if (!(activityPropertyName == DynamicActivityXamlReader.xPropertyName))
                                {
                                    break;
                                }
                                activityPropertyName = parent.activityPropertyName;
                                if (a == 0)
                                {
                                    a = 1;
                                }
                                goto Label_0115;

                            case XamlNodeType.EndMember:
                                DynamicActivityXamlReader.DecrementIfPositive(ref a);
                                DynamicActivityXamlReader.DecrementIfPositive(ref num3);
                                goto Label_0231;

                            case XamlNodeType.Value:
                                if (a != 1)
                                {
                                    goto Label_014F;
                                }
                                this.Name = reader.Value as string;
                                goto Label_0231;

                            default:
                                goto Label_0231;
                        }
                        if (activityPropertyName == DynamicActivityXamlReader.xPropertyType)
                        {
                            activityPropertyName = parent.activityPropertyType;
                            if (num3 == 0)
                            {
                                num3 = 1;
                            }
                        }
                        else
                        {
                            if (activityPropertyName != DynamicActivityXamlReader.xPropertyAttributes)
                            {
                                throw FxTrace.Exception.AsError(DynamicActivityXamlReader.CreateXamlException(System.Activities.SR.PropertyMemberNotSupportedByActivityXamlServices(activityPropertyName.Name), innerReaderLineInfo));
                            }
                            activityPropertyName = parent.activityPropertyAttributes;
                        }
                    Label_0115:
                        this.nodes.Writer.WriteStartMember(activityPropertyName, innerReaderLineInfo);
                        flag = reader.Read();
                        continue;
                    Label_014F:
                        if (num3 == 1)
                        {
                            XamlTypeName xamlTypeName = XamlTypeName.Parse(reader.Value as string, parent.namespaceTable);
                            XamlType xamlType = parent.SchemaContext.GetXamlType(xamlTypeName);
                            if (xamlType == null)
                            {
                                throw FxTrace.Exception.AsError(DynamicActivityXamlReader.CreateXamlException(System.Activities.SR.InvalidPropertyType(reader.Value as string, this.Name), innerReaderLineInfo));
                            }
                            this.Type = xamlType;
                        }
                        goto Label_0231;
                    Label_0213:
                        DynamicActivityXamlReader.DecrementIfPositive(ref a);
                        DynamicActivityXamlReader.DecrementIfPositive(ref num3);
                    Label_0231:
                        this.nodes.Writer.WriteNode(reader, innerReaderLineInfo);
                        flag = reader.Read();
                    }
                    reader.Close();
                }

                public void CopyTo(XamlNodeQueue targetNodeQueue, IXamlLineInfo readerInfo)
                {
                    XamlServices.Transform(this.nodes.Reader, targetNodeQueue.Writer, false);
                    targetNodeQueue.Writer.WriteEndObject(readerInfo);
                }

                public void ProcessDefaultValue(XamlMember propertyValue, XamlReader subReader, IXamlLineInfo lineInfo)
                {
                    XamlReader reader;
                    bool flag = false;
                    subReader.Read();
                    if (!subReader.Member.IsNameValid)
                    {
                        throw FxTrace.Exception.AsError(DynamicActivityXamlReader.CreateXamlException(System.Activities.SR.InvalidXamlMember(subReader.Member.Name), lineInfo));
                    }
                    this.nodes.Writer.WriteStartMember(propertyValue, lineInfo);
                    subReader.Read();
                    if (subReader.NodeType == XamlNodeType.GetObject)
                    {
                        subReader.Read();
                        subReader.Read();
                        reader = subReader.ReadSubtree();
                        reader.Read();
                    }
                    else
                    {
                        reader = subReader;
                    }
                    if ((reader.NodeType != XamlNodeType.EndMember) && (reader.NodeType != XamlNodeType.StartObject))
                    {
                        flag = true;
                        this.nodes.Writer.WriteStartObject(this.Type, lineInfo);
                        this.nodes.Writer.WriteStartMember(XamlLanguage.Initialization, lineInfo);
                    }
                    while (!reader.IsEof)
                    {
                        this.nodes.Writer.WriteNode(reader, lineInfo);
                        reader.Read();
                    }
                    reader.Close();
                    if (!object.ReferenceEquals(reader, subReader))
                    {
                        subReader.Read();
                        while (subReader.Read())
                        {
                            this.nodes.Writer.WriteNode(subReader, lineInfo);
                        }
                    }
                    if (flag)
                    {
                        this.nodes.Writer.WriteEndObject(lineInfo);
                        this.nodes.Writer.WriteEndMember(lineInfo);
                    }
                    subReader.Close();
                }

                public string Name { get; private set; }

                public XamlType Type { get; private set; }
            }

            private class ValueHolder
            {
                private XamlNodeQueue nodes;

                public ValueHolder(XamlSchemaContext schemaContext, XamlMember propertyValue, XamlReader reader, IXamlLineInfo lineInfo)
                {
                    this.nodes = new XamlNodeQueue(schemaContext);
                    this.PropertyValue = propertyValue;
                    XamlWriterExtensions.Transform(reader.ReadSubtree(), this.nodes.Writer, lineInfo, true);
                }

                public XamlMember PropertyValue { get; private set; }

                public XamlReader ValueReader
                {
                    get
                    {
                        return this.nodes.Reader;
                    }
                }
            }
        }

        private class BuilderMemberNode
        {
            private XamlNodeQueue bufferedNodes;
            private XamlMember currentMember;
            private int currentMemberLineNumber;
            private int currentMemberLinePosition;
            private DynamicActivityXamlReader parent;

            public BuilderMemberNode(DynamicActivityXamlReader parent, int depth)
            {
                this.parent = parent;
                this.Depth = depth;
                this.currentMember = parent.innerReader.Member;
                if (parent.hasLineInfo)
                {
                    this.currentMemberLineNumber = parent.innerReaderLineInfo.LineNumber;
                    this.currentMemberLinePosition = parent.innerReaderLineInfo.LinePosition;
                }
                this.bufferedNodes = new XamlNodeQueue(parent.SchemaContext);
            }

            public void FlushBuffer(XamlWriter targetWriter)
            {
                targetWriter.WriteStartMember(this.currentMember, this.currentMemberLineNumber, this.currentMemberLinePosition);
                XamlServices.Transform(this.bufferedNodes.Reader, targetWriter, false);
                this.bufferedNodes = null;
            }

            public bool ProcessNode(XamlReader reader, XamlWriter targetWriter, int currentDepth, IXamlLineInfo readerLineInfo)
            {
                if ((currentDepth == this.Depth) && ((reader.NodeType == XamlNodeType.NamespaceDeclaration) || (reader.NodeType == XamlNodeType.None)))
                {
                    this.bufferedNodes.Writer.WriteNode(reader, readerLineInfo);
                    reader.Read();
                    return true;
                }
                if ((((reader.NodeType == XamlNodeType.StartObject) && reader.Type.IsGeneric) && ((reader.Type.UnderlyingType != null) && (reader.Type.Name == "PropertyReferenceExtension"))) && (reader.Type.UnderlyingType.GetGenericTypeDefinition() == typeof(PropertyReferenceExtension<>)))
                {
                    if (this.bufferedNodes.Count > 0)
                    {
                        XamlServices.Transform(this.bufferedNodes.Reader, targetWriter, false);
                        this.bufferedNodes = null;
                    }
                    XamlType type = reader.Type;
                    XamlReader reader2 = reader.ReadSubtree();
                    XamlType xamlType = reader.SchemaContext.GetXamlType(typeof(ActivityBuilder));
                    XamlType type3 = reader.SchemaContext.GetXamlType(typeof(ActivityPropertyReference));
                    targetWriter.WriteStartMember(xamlType.GetAttachableMember("PropertyReference"), readerLineInfo);
                    reader2.Read();
                    targetWriter.WriteStartObject(type3, readerLineInfo);
                    targetWriter.WriteStartMember(type3.GetMember("TargetProperty"), readerLineInfo);
                    targetWriter.WriteValue(this.currentMember.Name, readerLineInfo);
                    targetWriter.WriteEndMember(readerLineInfo);
                    bool flag = reader2.Read();
                    bool flag2 = false;
                    while (flag)
                    {
                        if (((reader2.NodeType == XamlNodeType.StartMember) && (reader2.Member.DeclaringType == type)) && (reader2.Member.Name == "PropertyName"))
                        {
                            flag2 = true;
                        }
                        else if (flag2)
                        {
                            if (reader2.NodeType == XamlNodeType.EndMember)
                            {
                                flag2 = false;
                            }
                            else if (reader2.NodeType == XamlNodeType.Value)
                            {
                                targetWriter.WriteStartMember(type3.GetMember("SourceProperty"), readerLineInfo);
                                targetWriter.WriteValue((string) reader2.Value, readerLineInfo);
                                targetWriter.WriteEndMember(readerLineInfo);
                            }
                        }
                        flag = reader2.Read();
                    }
                    targetWriter.WriteEndObject(readerLineInfo);
                    targetWriter.WriteEndMember(readerLineInfo);
                    this.ExitObject = true;
                    reader2.Close();
                }
                else
                {
                    this.FlushBuffer(targetWriter);
                    targetWriter.WriteNode(reader, readerLineInfo);
                }
                return false;
            }

            public int Depth { get; private set; }

            public bool ExitObject { get; private set; }
        }
    }
}

