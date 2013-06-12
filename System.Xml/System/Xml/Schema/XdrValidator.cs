namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal sealed class XdrValidator : BaseValidator
    {
        private Hashtable attPresence;
        private IdRefNode idRefListHead;
        private Hashtable IDs;
        private System.Xml.Schema.Parser inlineSchemaParser;
        private bool isProcessContents;
        private XmlQualifiedName name;
        private XmlNamespaceManager nsManager;
        private const int STACK_INCREMENT = 10;
        private HWStack validationStack;
        private const string x_schema = "x-schema:";

        internal XdrValidator(BaseValidator validator) : base(validator)
        {
            this.name = XmlQualifiedName.Empty;
            this.Init();
        }

        internal XdrValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling) : base(reader, schemaCollection, eventHandling)
        {
            this.name = XmlQualifiedName.Empty;
            this.Init();
        }

        internal void AddID(string name, object node)
        {
            if (this.IDs == null)
            {
                this.IDs = new Hashtable();
            }
            this.IDs.Add(name, node);
        }

        public static void CheckDefaultValue(string value, SchemaAttDef attdef, SchemaInfo sinfo, XmlNamespaceManager nsManager, XmlNameTable NameTable, object sender, ValidationEventHandler eventhandler, string baseUri, int lineNo, int linePos)
        {
            try
            {
                XmlSchemaDatatype datatype = attdef.Datatype;
                if (datatype != null)
                {
                    if (datatype.TokenizedType != XmlTokenizedType.CDATA)
                    {
                        value = value.Trim();
                    }
                    if (value.Length != 0)
                    {
                        object pVal = datatype.ParseValue(value, NameTable, nsManager);
                        XmlTokenizedType tokenizedType = datatype.TokenizedType;
                        if (tokenizedType == XmlTokenizedType.ENTITY)
                        {
                            if (datatype.Variety == XmlSchemaDatatypeVariety.List)
                            {
                                string[] strArray = (string[]) pVal;
                                for (int i = 0; i < strArray.Length; i++)
                                {
                                    BaseValidator.ProcessEntity(sinfo, strArray[i], sender, eventhandler, baseUri, lineNo, linePos);
                                }
                            }
                            else
                            {
                                BaseValidator.ProcessEntity(sinfo, (string) pVal, sender, eventhandler, baseUri, lineNo, linePos);
                            }
                        }
                        else if ((tokenizedType == XmlTokenizedType.ENUMERATION) && !attdef.CheckEnumeration(pVal))
                        {
                            XmlSchemaException ex = new XmlSchemaException("Sch_EnumerationValue", pVal.ToString(), baseUri, lineNo, linePos);
                            if (eventhandler == null)
                            {
                                throw ex;
                            }
                            eventhandler(sender, new ValidationEventArgs(ex));
                        }
                        attdef.DefaultValueTyped = pVal;
                    }
                }
            }
            catch
            {
                XmlSchemaException exception2 = new XmlSchemaException("Sch_AttributeDefaultDataType", attdef.Name.ToString(), baseUri, lineNo, linePos);
                if (eventhandler == null)
                {
                    throw exception2;
                }
                eventhandler(sender, new ValidationEventArgs(exception2));
            }
        }

        private void CheckForwardRefs()
        {
            IdRefNode next;
            for (IdRefNode node = this.idRefListHead; node != null; node = next)
            {
                if (this.FindId(node.Id) == null)
                {
                    base.SendValidationEvent(new XmlSchemaException("Sch_UndeclaredId", node.Id, base.reader.BaseURI, node.LineNo, node.LinePos));
                }
                next = node.Next;
                node.Next = null;
            }
            this.idRefListHead = null;
        }

        private void CheckValue(string value, SchemaAttDef attdef)
        {
            try
            {
                base.reader.TypedValueObject = null;
                bool flag = attdef != null;
                XmlSchemaDatatype datatype = flag ? attdef.Datatype : base.context.ElementDecl.Datatype;
                if (datatype != null)
                {
                    if (datatype.TokenizedType != XmlTokenizedType.CDATA)
                    {
                        value = value.Trim();
                    }
                    if (value.Length != 0)
                    {
                        object pVal = datatype.ParseValue(value, base.NameTable, this.nsManager);
                        base.reader.TypedValueObject = pVal;
                        switch (datatype.TokenizedType)
                        {
                            case XmlTokenizedType.ENTITY:
                            case XmlTokenizedType.ID:
                            case XmlTokenizedType.IDREF:
                                if (datatype.Variety == XmlSchemaDatatypeVariety.List)
                                {
                                    string[] strArray = (string[]) pVal;
                                    for (int i = 0; i < strArray.Length; i++)
                                    {
                                        this.ProcessTokenizedType(datatype.TokenizedType, strArray[i]);
                                    }
                                }
                                else
                                {
                                    this.ProcessTokenizedType(datatype.TokenizedType, (string) pVal);
                                }
                                break;
                        }
                        SchemaDeclBase base2 = flag ? ((SchemaDeclBase) attdef) : ((SchemaDeclBase) base.context.ElementDecl);
                        if ((base2.MaxLength != 0xffffffffL) && (value.Length > base2.MaxLength))
                        {
                            base.SendValidationEvent("Sch_MaxLengthConstraintFailed", value);
                        }
                        if ((base2.MinLength != 0xffffffffL) && (value.Length < base2.MinLength))
                        {
                            base.SendValidationEvent("Sch_MinLengthConstraintFailed", value);
                        }
                        if ((base2.Values != null) && !base2.CheckEnumeration(pVal))
                        {
                            if (datatype.TokenizedType == XmlTokenizedType.NOTATION)
                            {
                                base.SendValidationEvent("Sch_NotationValue", pVal.ToString());
                            }
                            else
                            {
                                base.SendValidationEvent("Sch_EnumerationValue", pVal.ToString());
                            }
                        }
                        if (!base2.CheckValue(pVal))
                        {
                            if (flag)
                            {
                                base.SendValidationEvent("Sch_FixedAttributeValue", attdef.Name.ToString());
                            }
                            else
                            {
                                base.SendValidationEvent("Sch_FixedElementValue", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                            }
                        }
                    }
                }
            }
            catch (XmlSchemaException)
            {
                if (attdef != null)
                {
                    base.SendValidationEvent("Sch_AttributeValueDataType", attdef.Name.ToString());
                }
                else
                {
                    base.SendValidationEvent("Sch_ElementValueDataType", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                }
            }
        }

        public override void CompleteValidation()
        {
            if (this.HasSchema)
            {
                this.CheckForwardRefs();
            }
            else
            {
                base.SendValidationEvent(new XmlSchemaException("Xml_NoValidation", string.Empty), XmlSeverityType.Warning);
            }
        }

        public override object FindId(string name)
        {
            if (this.IDs != null)
            {
                return this.IDs[name];
            }
            return null;
        }

        private void Init()
        {
            this.nsManager = base.reader.NamespaceManager;
            if (this.nsManager == null)
            {
                this.nsManager = new XmlNamespaceManager(base.NameTable);
                this.isProcessContents = true;
            }
            this.validationStack = new HWStack(10);
            base.textValue = new StringBuilder();
            this.name = XmlQualifiedName.Empty;
            this.attPresence = new Hashtable();
            this.Push(XmlQualifiedName.Empty);
            base.schemaInfo = new SchemaInfo();
            base.checkDatatype = false;
        }

        private void LoadSchema(string uri)
        {
            if (!base.schemaInfo.TargetNamespaces.ContainsKey(uri) && (base.XmlResolver != null))
            {
                SchemaInfo sinfo = null;
                if (base.SchemaCollection != null)
                {
                    sinfo = base.SchemaCollection.GetSchemaInfo(uri);
                }
                if (sinfo != null)
                {
                    if (sinfo.SchemaType != SchemaType.XDR)
                    {
                        throw new XmlException("Xml_MultipleValidaitonTypes", string.Empty, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                    }
                    base.schemaInfo.Add(sinfo, base.EventHandler);
                }
                else
                {
                    this.LoadSchemaFromLocation(uri);
                }
            }
        }

        private void LoadSchemaFromLocation(string uri)
        {
            if (XdrBuilder.IsXdrSchema(uri))
            {
                string relativeUri = uri.Substring("x-schema:".Length);
                XmlReader reader = null;
                SchemaInfo sinfo = null;
                try
                {
                    Uri absoluteUri = base.XmlResolver.ResolveUri(base.BaseUri, relativeUri);
                    Stream input = (Stream) base.XmlResolver.GetEntity(absoluteUri, null, null);
                    reader = new XmlTextReader(absoluteUri.ToString(), input, base.NameTable);
                    ((XmlTextReader) reader).XmlResolver = base.XmlResolver;
                    System.Xml.Schema.Parser parser = new System.Xml.Schema.Parser(SchemaType.XDR, base.NameTable, base.SchemaNames, base.EventHandler) {
                        XmlResolver = base.XmlResolver
                    };
                    parser.Parse(reader, uri);
                    while (reader.Read())
                    {
                    }
                    sinfo = parser.XdrSchema;
                }
                catch (XmlSchemaException exception)
                {
                    base.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception.Message }, XmlSeverityType.Error);
                }
                catch (Exception exception2)
                {
                    base.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception2.Message }, XmlSeverityType.Warning);
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
                if ((sinfo != null) && (sinfo.ErrorCount == 0))
                {
                    base.schemaInfo.Add(sinfo, base.EventHandler);
                    base.SchemaCollection.Add(uri, sinfo, null, false);
                }
            }
        }

        private void Pop()
        {
            if (this.validationStack.Length > 1)
            {
                this.validationStack.Pop();
                base.context = (ValidationState) this.validationStack.Peek();
            }
        }

        private void ProcessElement()
        {
            this.Push(base.elementName);
            if (this.isProcessContents)
            {
                this.nsManager.PopScope();
            }
            base.context.ElementDecl = this.ThoroughGetElementDecl();
            if (base.context.ElementDecl != null)
            {
                this.ValidateStartElement();
                this.ValidateEndStartElement();
                base.context.NeedValidateChildren = true;
                base.context.ElementDecl.ContentValidator.InitValidation(base.context);
            }
        }

        private void ProcessInlineSchema()
        {
            if (!this.inlineSchemaParser.ParseReaderNode())
            {
                this.inlineSchemaParser.FinishParsing();
                SchemaInfo xdrSchema = this.inlineSchemaParser.XdrSchema;
                if ((xdrSchema != null) && (xdrSchema.ErrorCount == 0))
                {
                    foreach (string str in xdrSchema.TargetNamespaces.Keys)
                    {
                        if (!base.schemaInfo.HasSchema(str))
                        {
                            base.schemaInfo.Add(xdrSchema, base.EventHandler);
                            base.SchemaCollection.Add(str, xdrSchema, null, false);
                            break;
                        }
                    }
                }
                this.inlineSchemaParser = null;
            }
        }

        private void ProcessTokenizedType(XmlTokenizedType ttype, string name)
        {
            switch (ttype)
            {
                case XmlTokenizedType.ID:
                    if (this.FindId(name) == null)
                    {
                        this.AddID(name, base.context.LocalName);
                        return;
                    }
                    base.SendValidationEvent("Sch_DupId", name);
                    return;

                case XmlTokenizedType.IDREF:
                    if (this.FindId(name) != null)
                    {
                        break;
                    }
                    this.idRefListHead = new IdRefNode(this.idRefListHead, name, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                    return;

                case XmlTokenizedType.IDREFS:
                    break;

                case XmlTokenizedType.ENTITY:
                    BaseValidator.ProcessEntity(base.schemaInfo, name, this, base.EventHandler, base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                    break;

                default:
                    return;
            }
        }

        private void Push(XmlQualifiedName elementName)
        {
            base.context = (ValidationState) this.validationStack.Push();
            if (base.context == null)
            {
                base.context = new ValidationState();
                this.validationStack.AddToTop(base.context);
            }
            base.context.LocalName = elementName.Name;
            base.context.Namespace = elementName.Namespace;
            base.context.HasMatched = false;
            base.context.IsNill = false;
            base.context.NeedValidateChildren = false;
        }

        private XmlQualifiedName QualifiedName(string name, string ns)
        {
            return new XmlQualifiedName(name, XmlSchemaDatatype.XdrCanonizeUri(ns, base.NameTable, base.SchemaNames));
        }

        private SchemaElementDecl ThoroughGetElementDecl()
        {
            if (base.reader.Depth == 0)
            {
                this.LoadSchema(string.Empty);
            }
            if (base.reader.MoveToFirstAttribute())
            {
                do
                {
                    string namespaceURI = base.reader.NamespaceURI;
                    string localName = base.reader.LocalName;
                    if (Ref.Equal(namespaceURI, base.SchemaNames.NsXmlNs))
                    {
                        this.LoadSchema(base.reader.Value);
                        if (this.isProcessContents)
                        {
                            this.nsManager.AddNamespace((base.reader.Prefix.Length == 0) ? string.Empty : base.reader.LocalName, base.reader.Value);
                        }
                    }
                    if (Ref.Equal(namespaceURI, base.SchemaNames.QnDtDt.Namespace) && Ref.Equal(localName, base.SchemaNames.QnDtDt.Name))
                    {
                        base.reader.SchemaTypeObject = XmlSchemaDatatype.FromXdrName(base.reader.Value);
                    }
                }
                while (base.reader.MoveToNextAttribute());
                base.reader.MoveToElement();
            }
            SchemaElementDecl elementDecl = base.schemaInfo.GetElementDecl(base.elementName);
            if ((elementDecl == null) && base.schemaInfo.TargetNamespaces.ContainsKey(base.context.Namespace))
            {
                base.SendValidationEvent("Sch_UndeclaredElement", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
            }
            return elementDecl;
        }

        public override void Validate()
        {
            if (this.IsInlineSchemaStarted)
            {
                this.ProcessInlineSchema();
            }
            else
            {
                switch (base.reader.NodeType)
                {
                    case XmlNodeType.Element:
                        this.ValidateElement();
                        if (!base.reader.IsEmptyElement)
                        {
                            return;
                        }
                        break;

                    case XmlNodeType.Attribute:
                        return;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.SignificantWhitespace:
                        base.ValidateText();
                        return;

                    case XmlNodeType.Whitespace:
                        base.ValidateWhitespace();
                        return;

                    case XmlNodeType.EndElement:
                        break;

                    default:
                        return;
                }
                this.ValidateEndElement();
            }
        }

        private void ValidateChildElement()
        {
            if (base.context.NeedValidateChildren)
            {
                int errorCode = 0;
                base.context.ElementDecl.ContentValidator.ValidateElement(base.elementName, base.context, out errorCode);
                if (errorCode < 0)
                {
                    XmlSchemaValidator.ElementValidationError(base.elementName, base.context, base.EventHandler, base.reader, base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
                }
            }
        }

        private void ValidateElement()
        {
            base.elementName.Init(base.reader.LocalName, XmlSchemaDatatype.XdrCanonizeUri(base.reader.NamespaceURI, base.NameTable, base.SchemaNames));
            this.ValidateChildElement();
            if (base.SchemaNames.IsXDRRoot(base.elementName.Name, base.elementName.Namespace) && (base.reader.Depth > 0))
            {
                this.inlineSchemaParser = new System.Xml.Schema.Parser(SchemaType.XDR, base.NameTable, base.SchemaNames, base.EventHandler);
                this.inlineSchemaParser.StartParsing(base.reader, null);
                this.inlineSchemaParser.ParseReaderNode();
            }
            else
            {
                this.ProcessElement();
            }
        }

        private void ValidateEndElement()
        {
            if (this.isProcessContents)
            {
                this.nsManager.PopScope();
            }
            if (base.context.ElementDecl != null)
            {
                if (base.context.NeedValidateChildren && !base.context.ElementDecl.ContentValidator.CompleteValidation(base.context))
                {
                    XmlSchemaValidator.CompleteValidationError(base.context, base.EventHandler, base.reader, base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
                }
                if (base.checkDatatype)
                {
                    string str = !base.hasSibling ? base.textString : base.textValue.ToString();
                    this.CheckValue(str, null);
                    base.checkDatatype = false;
                    base.textValue.Length = 0;
                    base.textString = string.Empty;
                }
            }
            this.Pop();
        }

        private void ValidateEndStartElement()
        {
            if (base.context.ElementDecl.HasDefaultAttribute)
            {
                for (int i = 0; i < base.context.ElementDecl.DefaultAttDefs.Count; i++)
                {
                    base.reader.AddDefaultAttribute((SchemaAttDef) base.context.ElementDecl.DefaultAttDefs[i]);
                }
            }
            if (base.context.ElementDecl.HasRequiredAttribute)
            {
                try
                {
                    base.context.ElementDecl.CheckAttributes(this.attPresence, base.reader.StandAlone);
                }
                catch (XmlSchemaException exception)
                {
                    exception.SetSource(base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                    base.SendValidationEvent(exception);
                }
            }
            if (base.context.ElementDecl.Datatype != null)
            {
                base.checkDatatype = true;
                base.hasSibling = false;
                base.textString = string.Empty;
                base.textValue.Length = 0;
            }
        }

        private void ValidateStartElement()
        {
            if (base.context.ElementDecl != null)
            {
                if (base.context.ElementDecl.SchemaType != null)
                {
                    base.reader.SchemaTypeObject = base.context.ElementDecl.SchemaType;
                }
                else
                {
                    base.reader.SchemaTypeObject = base.context.ElementDecl.Datatype;
                }
                if ((base.reader.IsEmptyElement && !base.context.IsNill) && (base.context.ElementDecl.DefaultValueTyped != null))
                {
                    base.reader.TypedValueObject = base.context.ElementDecl.DefaultValueTyped;
                    base.context.IsNill = true;
                }
                if (base.context.ElementDecl.HasRequiredAttribute)
                {
                    this.attPresence.Clear();
                }
            }
            if (base.reader.MoveToFirstAttribute())
            {
                do
                {
                    if (base.reader.NamespaceURI != base.SchemaNames.NsXmlNs)
                    {
                        try
                        {
                            base.reader.SchemaTypeObject = null;
                            SchemaAttDef attributeXdr = base.schemaInfo.GetAttributeXdr(base.context.ElementDecl, this.QualifiedName(base.reader.LocalName, base.reader.NamespaceURI));
                            if (attributeXdr != null)
                            {
                                if ((base.context.ElementDecl != null) && base.context.ElementDecl.HasRequiredAttribute)
                                {
                                    this.attPresence.Add(attributeXdr.Name, attributeXdr);
                                }
                                base.reader.SchemaTypeObject = (attributeXdr.SchemaType != null) ? ((object) attributeXdr.SchemaType) : ((object) attributeXdr.Datatype);
                                if (attributeXdr.Datatype != null)
                                {
                                    string str = base.reader.Value;
                                    this.CheckValue(str, attributeXdr);
                                }
                            }
                        }
                        catch (XmlSchemaException exception)
                        {
                            exception.SetSource(base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                            base.SendValidationEvent(exception);
                        }
                    }
                }
                while (base.reader.MoveToNextAttribute());
                base.reader.MoveToElement();
            }
        }

        private bool HasSchema
        {
            get
            {
                return (base.schemaInfo.SchemaType != SchemaType.None);
            }
        }

        private bool IsInlineSchemaStarted
        {
            get
            {
                return (this.inlineSchemaParser != null);
            }
        }

        public override bool PreserveWhitespace
        {
            get
            {
                if (base.context.ElementDecl == null)
                {
                    return false;
                }
                return base.context.ElementDecl.ContentValidator.PreserveWhitespace;
            }
        }
    }
}

