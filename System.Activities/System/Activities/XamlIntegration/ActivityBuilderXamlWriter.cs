namespace System.Activities.XamlIntegration
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xaml.Schema;

    internal class ActivityBuilderXamlWriter : XamlWriter
    {
        private XamlMember activityBuilderAttributes;
        private XamlMember activityBuilderName;
        private XamlMember activityBuilderProperties;
        private XamlMember activityBuilderPropertyReference;
        private XamlType activityBuilderXamlType;
        private XamlMember activityPropertyName;
        private XamlType activityPropertyReferenceXamlType;
        private XamlMember activityPropertyType;
        private XamlMember activityPropertyValue;
        private XamlType activityPropertyXamlType;
        private XamlType activityXamlType;
        private int currentDepth;
        private BuilderXamlNode currentState;
        private readonly XamlWriter innerWriter;
        private NamespaceTable namespaceTable;
        private bool notRewriting;
        private Stack<BuilderXamlNode> pendingStates;
        private XamlType typeXamlType;
        private XamlType xamlTypeXamlType;

        public ActivityBuilderXamlWriter(XamlWriter innerWriter)
        {
            this.innerWriter = innerWriter;
            this.currentState = new RootNode(this);
            this.namespaceTable = new NamespaceTable();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                ((IDisposable) this.innerWriter).Dispose();
            }
        }

        private void EnterDepth()
        {
            this.currentDepth++;
            if (this.namespaceTable != null)
            {
                this.namespaceTable.EnterScope();
            }
        }

        private void ExitDepth()
        {
            if (this.currentState.Depth == this.currentDepth)
            {
                this.currentState.Complete();
                if (this.pendingStates.Count > 0)
                {
                    this.currentState = this.pendingStates.Pop();
                }
            }
            this.currentDepth--;
            if (this.namespaceTable != null)
            {
                this.namespaceTable.ExitScope();
            }
        }

        private void PushState(BuilderXamlNode state)
        {
            if (this.pendingStates == null)
            {
                this.pendingStates = new Stack<BuilderXamlNode>();
            }
            this.pendingStates.Push(this.currentState);
            this.currentState = state;
        }

        private void SetActivityType(XamlType activityXamlType, XamlType activityBuilderXamlType)
        {
            if (activityXamlType == null)
            {
                this.notRewriting = true;
            }
            else
            {
                this.activityXamlType = activityXamlType;
                this.activityBuilderXamlType = activityBuilderXamlType;
                this.xamlTypeXamlType = this.SchemaContext.GetXamlType(typeof(XamlType));
                this.typeXamlType = this.SchemaContext.GetXamlType(typeof(Type));
                this.activityPropertyXamlType = this.SchemaContext.GetXamlType(typeof(DynamicActivityProperty));
                this.activityPropertyType = this.activityPropertyXamlType.GetMember("Type");
                this.activityPropertyName = this.activityPropertyXamlType.GetMember("Name");
                this.activityPropertyValue = this.activityPropertyXamlType.GetMember("Value");
                this.activityBuilderName = this.activityBuilderXamlType.GetMember("Name");
                this.activityBuilderAttributes = this.activityBuilderXamlType.GetMember("Attributes");
                this.activityBuilderProperties = this.activityBuilderXamlType.GetMember("Properties");
                this.activityBuilderPropertyReference = this.SchemaContext.GetXamlType(typeof(ActivityBuilder)).GetAttachableMember("PropertyReference");
                this.activityPropertyReferenceXamlType = this.SchemaContext.GetXamlType(typeof(ActivityPropertyReference));
            }
        }

        public override void WriteEndMember()
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteEndMember();
            }
            else
            {
                this.currentState.WriteEndMember();
                this.ExitDepth();
            }
        }

        public override void WriteEndObject()
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteEndObject();
            }
            else
            {
                this.currentState.WriteEndObject();
                this.ExitDepth();
            }
        }

        public override void WriteGetObject()
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteGetObject();
            }
            else
            {
                this.EnterDepth();
                this.currentState.WriteGetObject();
            }
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteNamespace(namespaceDeclaration);
            }
            else
            {
                if (this.namespaceTable != null)
                {
                    this.namespaceTable.AddNamespace(namespaceDeclaration);
                }
                this.currentState.WriteNamespace(namespaceDeclaration);
            }
        }

        public override void WriteStartMember(XamlMember xamlMember)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteStartMember(xamlMember);
            }
            else
            {
                this.EnterDepth();
                this.currentState.WriteStartMember(xamlMember);
            }
        }

        public override void WriteStartObject(XamlType xamlType)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteStartObject(xamlType);
            }
            else
            {
                this.EnterDepth();
                this.currentState.WriteStartObject(xamlType);
            }
        }

        public override void WriteValue(object value)
        {
            if (this.notRewriting)
            {
                this.innerWriter.WriteValue(value);
            }
            else
            {
                this.currentState.WriteValue(value);
            }
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return this.innerWriter.SchemaContext;
            }
        }

        private class AttributesNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private XamlNodeQueue attributeNodes;
            private ActivityBuilderXamlWriter.BuilderClassNode classNode;

            public AttributesNode(ActivityBuilderXamlWriter.BuilderClassNode classNode, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.classNode = classNode;
                this.attributeNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.attributeNodes.Writer;
                base.CurrentWriter.WriteStartMember(XamlLanguage.ClassAttributes);
            }

            protected internal override void Complete()
            {
                this.classNode.SetAttributes(this.attributeNodes);
            }
        }

        private class BuilderClassNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private List<KeyValuePair<string, XamlNodeQueue>> defaultValueNodes;
            private XamlNodeQueue otherNodes;
            private ActivityBuilderXamlWriter.RootNode rootNode;
            private XamlNodeQueue xClassAttributeNodes;
            private string xClassNamespace;
            private XamlNodeQueue xClassNodes;
            private XamlType xClassXamlType;
            private XamlNodeQueue xPropertiesNodes;

            public BuilderClassNode(ActivityBuilderXamlWriter.RootNode rootNode, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.rootNode = rootNode;
                this.otherNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.otherNodes.Writer;
            }

            protected internal override void Complete()
            {
                if (this.otherNodes != null)
                {
                    this.FlushPreamble();
                }
            }

            private void FlushPreamble()
            {
                if (this.otherNodes != null)
                {
                    base.CurrentWriter = base.Writer.innerWriter;
                    string classNamespace = null;
                    if (this.defaultValueNodes != null)
                    {
                        classNamespace = this.xClassNamespace;
                    }
                    this.rootNode.FlushPendingNodes(classNamespace);
                    this.rootNode = null;
                    base.CurrentWriter.WriteStartObject(base.Writer.activityXamlType);
                    if (this.xClassNodes == null)
                    {
                        this.SetXClass(null, null);
                    }
                    XamlServices.Transform(this.xClassNodes.Reader, base.CurrentWriter, false);
                    XamlNodeQueue queue = null;
                    if (this.defaultValueNodes != null)
                    {
                        foreach (KeyValuePair<string, XamlNodeQueue> pair in this.defaultValueNodes)
                        {
                            XamlReader reader = pair.Value.Reader;
                            if (reader.Read())
                            {
                                bool flag = false;
                                if ((reader.NodeType == XamlNodeType.Value) && (reader.Value is string))
                                {
                                    flag = true;
                                }
                                if (flag)
                                {
                                    base.CurrentWriter.WriteStartMember(new XamlMember(pair.Key, this.xClassXamlType, true));
                                    base.CurrentWriter.WriteNode(reader);
                                    XamlServices.Transform(pair.Value.Reader, base.CurrentWriter, false);
                                }
                                else
                                {
                                    if (queue == null)
                                    {
                                        queue = new XamlNodeQueue(base.Writer.SchemaContext);
                                    }
                                    queue.Writer.WriteStartMember(new XamlMember(pair.Key, this.xClassXamlType, true));
                                    queue.Writer.WriteNode(reader);
                                    XamlServices.Transform(pair.Value.Reader, queue.Writer, false);
                                }
                            }
                        }
                    }
                    if (this.xClassAttributeNodes != null)
                    {
                        XamlServices.Transform(this.xClassAttributeNodes.Reader, base.CurrentWriter, false);
                    }
                    if (this.xPropertiesNodes != null)
                    {
                        XamlServices.Transform(this.xPropertiesNodes.Reader, base.CurrentWriter, false);
                    }
                    if (queue != null)
                    {
                        XamlServices.Transform(queue.Reader, base.CurrentWriter, false);
                    }
                    if (this.otherNodes.Count > 0)
                    {
                        XamlServices.Transform(this.otherNodes.Reader, base.CurrentWriter, false);
                    }
                    this.otherNodes = null;
                }
            }

            public void SetAttributes(XamlNodeQueue attributeNodes)
            {
                this.xClassAttributeNodes = attributeNodes;
            }

            public void SetProperties(XamlNodeQueue propertyNodes, List<KeyValuePair<string, XamlNodeQueue>> defaultValueNodes)
            {
                this.xPropertiesNodes = propertyNodes;
                this.defaultValueNodes = defaultValueNodes;
                this.FlushPreamble();
            }

            public void SetXClass(string builderName, XamlNodeQueue nameNodes)
            {
                this.xClassNodes = new XamlNodeQueue(base.Writer.SchemaContext);
                this.xClassNodes.Writer.WriteStartMember(XamlLanguage.Class);
                this.xClassNamespace = null;
                string str = builderName;
                if (string.IsNullOrEmpty(str))
                {
                    str = string.Format(CultureInfo.CurrentCulture, "_{0}", new object[] { Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 4) });
                }
                if (nameNodes != null)
                {
                    XamlServices.Transform(nameNodes.Reader, this.xClassNodes.Writer, false);
                }
                else
                {
                    this.xClassNodes.Writer.WriteValue(str);
                    this.xClassNodes.Writer.WriteEndMember();
                }
                int length = str.LastIndexOf('.');
                if (length > 0)
                {
                    this.xClassNamespace = builderName.Substring(0, length);
                    str = builderName.Substring(length + 1);
                }
                object[] args = new object[] { this.xClassNamespace ?? string.Empty };
                this.xClassNamespace = string.Format(CultureInfo.CurrentUICulture, "clr-namespace:{0}", args);
                this.xClassXamlType = new XamlType(this.xClassNamespace, str, null, base.Writer.SchemaContext);
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if ((base.Writer.currentDepth == (base.Depth + 1)) && !xamlMember.IsAttachable)
                {
                    if (xamlMember == base.Writer.activityBuilderName)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.BuilderNameNode(this, base.Writer));
                        return;
                    }
                    if (xamlMember == base.Writer.activityBuilderAttributes)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.AttributesNode(this, base.Writer));
                        return;
                    }
                    if (xamlMember == base.Writer.activityBuilderProperties)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.PropertiesNode(this, base.Writer));
                        return;
                    }
                    this.FlushPreamble();
                    if (xamlMember.DeclaringType == base.Writer.activityBuilderXamlType)
                    {
                        xamlMember = base.Writer.activityXamlType.GetMember(xamlMember.Name);
                        if (xamlMember == null)
                        {
                            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MemberNotSupportedByActivityXamlServices(xamlMember.Name)));
                        }
                        if (xamlMember.Name == "Implementation")
                        {
                            base.Writer.PushState(new ActivityBuilderXamlWriter.ImplementationNode(base.Writer));
                        }
                    }
                }
                base.WriteStartMember(xamlMember);
            }
        }

        private class BuilderNameNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private string builderName;
            private ActivityBuilderXamlWriter.BuilderClassNode classNode;
            private XamlNodeQueue nameNodes;

            public BuilderNameNode(ActivityBuilderXamlWriter.BuilderClassNode classNode, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.classNode = classNode;
                this.nameNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.nameNodes.Writer;
            }

            protected internal override void Complete()
            {
                this.classNode.SetXClass(this.builderName, this.nameNodes);
            }

            protected internal override void WriteValue(object value)
            {
                if (base.Writer.currentDepth == base.Depth)
                {
                    this.builderName = (string) value;
                }
                base.WriteValue(value);
            }
        }

        private abstract class BuilderXamlNode
        {
            protected BuilderXamlNode(ActivityBuilderXamlWriter writer)
            {
                this.Depth = writer.currentDepth;
                this.Writer = writer;
                this.CurrentWriter = writer.innerWriter;
            }

            protected internal virtual void Complete()
            {
            }

            protected internal virtual void WriteEndMember()
            {
                this.CurrentWriter.WriteEndMember();
            }

            protected internal virtual void WriteEndObject()
            {
                this.CurrentWriter.WriteEndObject();
            }

            protected internal virtual void WriteGetObject()
            {
                this.CurrentWriter.WriteGetObject();
            }

            protected internal virtual void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
            {
                this.CurrentWriter.WriteNamespace(namespaceDeclaration);
            }

            protected internal virtual void WriteStartMember(XamlMember xamlMember)
            {
                this.CurrentWriter.WriteStartMember(xamlMember);
            }

            protected internal virtual void WriteStartObject(XamlType xamlType)
            {
                this.CurrentWriter.WriteStartObject(xamlType);
            }

            protected internal virtual void WriteValue(object value)
            {
                this.CurrentWriter.WriteValue(value);
            }

            public XamlWriter CurrentWriter { get; protected set; }

            public int Depth { get; private set; }

            protected ActivityBuilderXamlWriter Writer { get; private set; }
        }

        private class ImplementationNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private Stack<XamlType> objectTypes;
            private Stack<XamlMember> xamlMembers;

            public ImplementationNode(ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.objectTypes = new Stack<XamlType>();
                this.xamlMembers = new Stack<XamlMember>();
            }

            protected internal override void WriteEndMember()
            {
                if (base.Writer.currentDepth > base.Depth)
                {
                    this.xamlMembers.Pop();
                }
                base.WriteEndMember();
            }

            protected internal override void WriteEndObject()
            {
                this.objectTypes.Pop();
                base.WriteEndObject();
            }

            protected internal override void WriteGetObject()
            {
                if (this.xamlMembers.Peek() == null)
                {
                    this.objectTypes.Push(null);
                }
                else
                {
                    this.objectTypes.Push(this.xamlMembers.Peek().Type);
                }
                base.WriteGetObject();
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if (((xamlMember == base.Writer.activityBuilderPropertyReference) && (this.objectTypes.Count > 0)) && (this.objectTypes.Peek() != null))
                {
                    base.Writer.PushState(new ActivityBuilderXamlWriter.PropertyReferenceNode(this.objectTypes.Peek(), base.Writer, xamlMember));
                }
                else
                {
                    this.xamlMembers.Push(xamlMember);
                    base.WriteStartMember(xamlMember);
                }
            }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                this.objectTypes.Push(xamlType);
                base.WriteStartObject(xamlType);
            }
        }

        private class PropertiesNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private ActivityBuilderXamlWriter.BuilderClassNode classNode;
            private List<KeyValuePair<string, XamlNodeQueue>> defaultValueNodes;
            private XamlNodeQueue propertiesNodes;
            private bool skipGetObject;

            public PropertiesNode(ActivityBuilderXamlWriter.BuilderClassNode classNode, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.classNode = classNode;
                this.propertiesNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.propertiesNodes.Writer;
                base.CurrentWriter.WriteStartMember(XamlLanguage.Members);
            }

            public void AddDefaultValue(string propertyName, XamlNodeQueue value)
            {
                if (this.defaultValueNodes == null)
                {
                    this.defaultValueNodes = new List<KeyValuePair<string, XamlNodeQueue>>();
                }
                if (string.IsNullOrEmpty(propertyName))
                {
                    propertyName = string.Format(CultureInfo.CurrentCulture, "_{0}", new object[] { Guid.NewGuid().ToString().Replace("-", string.Empty) });
                }
                this.defaultValueNodes.Add(new KeyValuePair<string, XamlNodeQueue>(propertyName, value));
            }

            protected internal override void Complete()
            {
                this.classNode.SetProperties(this.propertiesNodes, this.defaultValueNodes);
            }

            protected internal override void WriteEndMember()
            {
                if (!this.skipGetObject || (base.Writer.currentDepth != (base.Depth + 2)))
                {
                    base.WriteEndMember();
                }
            }

            protected internal override void WriteEndObject()
            {
                if (this.skipGetObject && (base.Writer.currentDepth == (base.Depth + 1)))
                {
                    this.skipGetObject = false;
                }
                else
                {
                    base.WriteEndObject();
                }
            }

            protected internal override void WriteGetObject()
            {
                if (base.Writer.currentDepth == (base.Depth + 1))
                {
                    this.skipGetObject = true;
                }
                else
                {
                    base.WriteGetObject();
                }
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if (!this.skipGetObject || (base.Writer.currentDepth != (base.Depth + 2)))
                {
                    base.WriteStartMember(xamlMember);
                }
            }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                if ((xamlType == base.Writer.activityPropertyXamlType) && (base.Writer.currentDepth == (base.Depth + 3)))
                {
                    xamlType = XamlLanguage.Property;
                    base.Writer.PushState(new ActivityBuilderXamlWriter.PropertyNode(this, base.Writer));
                }
                base.WriteStartObject(xamlType);
            }
        }

        private class PropertyNameNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private ActivityBuilderXamlWriter.PropertyNode property;

            public PropertyNameNode(ActivityBuilderXamlWriter.PropertyNode property, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.property = property;
                base.CurrentWriter = property.CurrentWriter;
            }

            protected internal override void WriteValue(object value)
            {
                if (base.Writer.currentDepth == base.Depth)
                {
                    this.property.SetName((string) value);
                }
                base.WriteValue(value);
            }
        }

        private class PropertyNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private XamlNodeQueue defaultValue;
            private ActivityBuilderXamlWriter.PropertiesNode properties;
            private string propertyName;
            private XamlType propertyType;

            public PropertyNode(ActivityBuilderXamlWriter.PropertiesNode properties, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.properties = properties;
                base.CurrentWriter = properties.CurrentWriter;
            }

            protected internal override void Complete()
            {
                if (this.defaultValue != null)
                {
                    if (string.IsNullOrEmpty(this.propertyName))
                    {
                        this.propertyName = string.Format(CultureInfo.CurrentCulture, "_{0}", new object[] { Guid.NewGuid().ToString().Replace("-", string.Empty) });
                    }
                    if ((this.defaultValue != null) && (this.propertyType != null))
                    {
                        this.defaultValue = StripTypeWrapping(this.defaultValue, this.propertyType);
                    }
                    this.properties.AddDefaultValue(this.propertyName, this.defaultValue);
                }
            }

            public void SetDefaultValue(XamlNodeQueue defaultValue)
            {
                this.defaultValue = defaultValue;
            }

            public void SetName(string name)
            {
                this.propertyName = name;
            }

            public void SetType(XamlType type)
            {
                this.propertyType = type;
            }

            private static XamlNodeQueue StripTypeWrapping(XamlNodeQueue valueNodes, XamlType propertyType)
            {
                XamlNodeQueue queue = new XamlNodeQueue(valueNodes.Reader.SchemaContext);
                XamlReader reader = valueNodes.Reader;
                XamlWriter writer = queue.Writer;
                int num = 0;
                bool flag = false;
                bool flag2 = false;
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XamlNodeType.StartObject:
                        {
                            num++;
                            if (((queue.Count != 0) || (num != 1)) || (!(reader.Type == propertyType) || (valueNodes.Count != 5)))
                            {
                                goto Label_00DA;
                            }
                            flag2 = true;
                            continue;
                        }
                        case XamlNodeType.GetObject:
                            num++;
                            goto Label_00DA;

                        case XamlNodeType.EndObject:
                        {
                            num--;
                            if (!flag || (num != 0))
                            {
                                goto Label_00DA;
                            }
                            flag = false;
                            continue;
                        }
                        case XamlNodeType.StartMember:
                        {
                            num++;
                            if (!flag2)
                            {
                                goto Label_00DA;
                            }
                            if ((num != 2) || (reader.Member != XamlLanguage.Initialization))
                            {
                                break;
                            }
                            flag = true;
                            continue;
                        }
                        case XamlNodeType.EndMember:
                        {
                            num--;
                            if (!flag || (num != 1))
                            {
                                goto Label_00DA;
                            }
                            continue;
                        }
                        default:
                            goto Label_00DA;
                    }
                    flag2 = false;
                    queue.Writer.WriteStartObject(propertyType);
                Label_00DA:
                    writer.WriteNode(reader);
                }
                return queue;
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if ((xamlMember.DeclaringType == base.Writer.activityPropertyXamlType) && (base.Writer.currentDepth == (base.Depth + 1)))
                {
                    if (xamlMember == base.Writer.activityPropertyName)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.PropertyNameNode(this, base.Writer));
                        xamlMember = DynamicActivityXamlReader.xPropertyName;
                    }
                    else if (xamlMember == base.Writer.activityPropertyType)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.PropertyTypeNode(this, base.Writer));
                        xamlMember = DynamicActivityXamlReader.xPropertyType;
                    }
                    else if (xamlMember == base.Writer.activityPropertyValue)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.PropertyValueNode(this, base.Writer));
                        xamlMember = null;
                    }
                }
                if (xamlMember != null)
                {
                    base.WriteStartMember(xamlMember);
                }
            }
        }

        private class PropertyReferenceNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private bool inSourceProperty;
            private bool inTargetProperty;
            private XamlMember originalStartMember;
            private XamlType owningType;
            private XamlNodeQueue propertyReferenceNodes;
            private string sourceProperty;
            private string targetProperty;

            public PropertyReferenceNode(XamlType owningType, ActivityBuilderXamlWriter writer, XamlMember originalStartMember) : base(writer)
            {
                this.owningType = owningType;
                this.propertyReferenceNodes = new XamlNodeQueue(writer.SchemaContext);
                this.originalStartMember = originalStartMember;
                base.CurrentWriter = this.propertyReferenceNodes.Writer;
            }

            protected internal override void Complete()
            {
                if (this.targetProperty == null)
                {
                    base.Writer.innerWriter.WriteStartMember(this.originalStartMember);
                    XamlServices.Transform(this.propertyReferenceNodes.Reader, base.Writer.innerWriter, false);
                }
                else
                {
                    XamlMember xamlMember = this.owningType.GetMember(this.targetProperty);
                    if (xamlMember == null)
                    {
                        xamlMember = new XamlMember(this.targetProperty, this.owningType, false);
                    }
                    base.Writer.innerWriter.WriteStartMember(xamlMember);
                    XamlType xamlType = base.Writer.SchemaContext.GetXamlType(typeof(PropertyReferenceExtension<>).MakeGenericType(new Type[] { xamlMember.Type.UnderlyingType }));
                    base.Writer.innerWriter.WriteStartObject(xamlType);
                    base.Writer.innerWriter.WriteStartMember(xamlType.GetMember("PropertyName"));
                    base.Writer.innerWriter.WriteValue(this.sourceProperty);
                    base.Writer.innerWriter.WriteEndMember();
                    base.Writer.innerWriter.WriteEndObject();
                    base.Writer.innerWriter.WriteEndMember();
                }
            }

            protected internal override void WriteEndMember()
            {
                if (base.Writer.currentDepth == (base.Depth + 2))
                {
                    this.inSourceProperty = false;
                    this.inTargetProperty = false;
                }
                base.WriteEndMember();
            }

            protected internal override void WriteStartMember(XamlMember xamlMember)
            {
                if ((base.Writer.currentDepth == (base.Depth + 2)) && (xamlMember.DeclaringType == base.Writer.activityPropertyReferenceXamlType))
                {
                    if (xamlMember.Name == "SourceProperty")
                    {
                        this.inSourceProperty = true;
                    }
                    else if (xamlMember.Name == "TargetProperty")
                    {
                        this.inTargetProperty = true;
                    }
                }
                base.WriteStartMember(xamlMember);
            }

            protected internal override void WriteValue(object value)
            {
                if (this.inSourceProperty)
                {
                    this.sourceProperty = (string) value;
                }
                else if (this.inTargetProperty)
                {
                    this.targetProperty = (string) value;
                }
                base.WriteValue(value);
            }
        }

        private class PropertyTypeNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private ActivityBuilderXamlWriter.PropertyNode property;

            public PropertyTypeNode(ActivityBuilderXamlWriter.PropertyNode property, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.property = property;
                base.CurrentWriter = property.CurrentWriter;
            }

            protected internal override void WriteValue(object value)
            {
                if (base.Writer.currentDepth == base.Depth)
                {
                    XamlTypeName xamlTypeName = XamlTypeName.Parse(value as string, base.Writer.namespaceTable);
                    XamlType xamlType = base.Writer.SchemaContext.GetXamlType(xamlTypeName);
                    this.property.SetType(xamlType);
                }
                base.WriteValue(value);
            }
        }

        private class PropertyValueNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private ActivityBuilderXamlWriter.PropertyNode property;
            private XamlNodeQueue valueNodes;

            public PropertyValueNode(ActivityBuilderXamlWriter.PropertyNode property, ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.property = property;
                this.valueNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.valueNodes.Writer;
            }

            protected internal override void Complete()
            {
                this.property.SetDefaultValue(this.valueNodes);
                base.Complete();
            }
        }

        private class RootNode : ActivityBuilderXamlWriter.BuilderXamlNode
        {
            private XamlNodeQueue pendingNodes;
            private const string PreferredClassAlias = "this";
            private const string PreferredXamlNamespaceAlias = "x";
            private System.Collections.Generic.HashSet<string> rootLevelPrefixes;
            private bool wroteXamlNamespace;

            public RootNode(ActivityBuilderXamlWriter writer) : base(writer)
            {
                this.pendingNodes = new XamlNodeQueue(writer.SchemaContext);
                base.CurrentWriter = this.pendingNodes.Writer;
            }

            public void FlushPendingNodes(string classNamespace)
            {
                base.CurrentWriter = base.Writer.innerWriter;
                if (!base.Writer.notRewriting)
                {
                    if (!this.wroteXamlNamespace)
                    {
                        string prefix = this.GenerateNamespacePrefix("x");
                        this.WriteNamespace(new NamespaceDeclaration("http://schemas.microsoft.com/winfx/2006/xaml", prefix));
                    }
                    if (classNamespace != null)
                    {
                        bool flag = false;
                        XamlReader reader = this.pendingNodes.Reader;
                        XamlWriter innerWriter = base.Writer.innerWriter;
                        while (reader.Read() && (reader.NodeType == XamlNodeType.NamespaceDeclaration))
                        {
                            if (classNamespace.Equals(reader.Namespace.Namespace))
                            {
                                flag = true;
                            }
                            innerWriter.WriteNode(reader);
                        }
                        if (!flag)
                        {
                            string str2 = this.GenerateNamespacePrefix("this");
                            innerWriter.WriteNamespace(new NamespaceDeclaration(classNamespace, str2));
                        }
                        if (!reader.IsEof)
                        {
                            innerWriter.WriteNode(reader);
                        }
                    }
                    this.rootLevelPrefixes = null;
                }
                XamlServices.Transform(this.pendingNodes.Reader, base.Writer.innerWriter, false);
                this.pendingNodes = null;
            }

            private string GenerateNamespacePrefix(string desiredPrefix)
            {
                string str = string.Empty;
                for (int i = 1; i <= 0x3e8; i++)
                {
                    string item = desiredPrefix + str;
                    if (!this.rootLevelPrefixes.Contains(item))
                    {
                        return item;
                    }
                    str = i.ToString(CultureInfo.InvariantCulture);
                }
                return (desiredPrefix + Guid.NewGuid().ToString());
            }

            protected internal override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
            {
                if ((base.Writer.currentDepth == 0) && !this.wroteXamlNamespace)
                {
                    if (namespaceDeclaration.Namespace == "http://schemas.microsoft.com/winfx/2006/xaml")
                    {
                        this.wroteXamlNamespace = true;
                    }
                    else
                    {
                        if (this.rootLevelPrefixes == null)
                        {
                            this.rootLevelPrefixes = new System.Collections.Generic.HashSet<string>();
                        }
                        this.rootLevelPrefixes.Add(namespaceDeclaration.Prefix);
                    }
                }
                base.WriteNamespace(namespaceDeclaration);
            }

            protected internal override void WriteStartObject(XamlType xamlType)
            {
                if (base.Writer.currentDepth == 1)
                {
                    XamlType activityXamlType = null;
                    if (xamlType.UnderlyingType == typeof(ActivityBuilder))
                    {
                        activityXamlType = base.Writer.SchemaContext.GetXamlType(typeof(Activity));
                    }
                    else if ((xamlType.IsGeneric && (xamlType.UnderlyingType != null)) && (xamlType.UnderlyingType.GetGenericTypeDefinition() == typeof(ActivityBuilder<>)))
                    {
                        Type underlyingType = xamlType.TypeArguments[0].UnderlyingType;
                        activityXamlType = base.Writer.SchemaContext.GetXamlType(typeof(Activity<>).MakeGenericType(new Type[] { underlyingType }));
                    }
                    base.Writer.SetActivityType(activityXamlType, xamlType);
                    if (activityXamlType != null)
                    {
                        base.Writer.PushState(new ActivityBuilderXamlWriter.BuilderClassNode(this, base.Writer));
                        return;
                    }
                    this.FlushPendingNodes(null);
                }
                base.WriteStartObject(xamlType);
            }
        }
    }
}

