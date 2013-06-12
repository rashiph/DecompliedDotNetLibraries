namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;

    internal sealed class DtdValidator : BaseValidator
    {
        private Hashtable attPresence;
        private IdRefNode idRefListHead;
        private Hashtable IDs;
        private XmlQualifiedName name;
        private static NamespaceManager namespaceManager = new NamespaceManager();
        private bool processIdentityConstraints;
        private const int STACK_INCREMENT = 10;
        private HWStack validationStack;

        internal DtdValidator(XmlValidatingReaderImpl reader, IValidationEventHandling eventHandling, bool processIdentityConstraints) : base(reader, null, eventHandling)
        {
            this.name = XmlQualifiedName.Empty;
            this.processIdentityConstraints = processIdentityConstraints;
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

        public static void CheckDefaultValue(SchemaAttDef attdef, SchemaInfo sinfo, IValidationEventHandling eventHandling, string baseUriStr)
        {
            try
            {
                if (baseUriStr == null)
                {
                    baseUriStr = string.Empty;
                }
                XmlSchemaDatatype datatype = attdef.Datatype;
                if (datatype != null)
                {
                    object defaultValueTyped = attdef.DefaultValueTyped;
                    XmlTokenizedType tokenizedType = datatype.TokenizedType;
                    if (tokenizedType == XmlTokenizedType.ENTITY)
                    {
                        if (datatype.Variety == XmlSchemaDatatypeVariety.List)
                        {
                            string[] strArray = (string[]) defaultValueTyped;
                            for (int i = 0; i < strArray.Length; i++)
                            {
                                BaseValidator.ProcessEntity(sinfo, strArray[i], eventHandling, baseUriStr, attdef.ValueLineNumber, attdef.ValueLinePosition);
                            }
                        }
                        else
                        {
                            BaseValidator.ProcessEntity(sinfo, (string) defaultValueTyped, eventHandling, baseUriStr, attdef.ValueLineNumber, attdef.ValueLinePosition);
                        }
                    }
                    else if (((tokenizedType == XmlTokenizedType.ENUMERATION) && !attdef.CheckEnumeration(defaultValueTyped)) && (eventHandling != null))
                    {
                        XmlSchemaException exception = new XmlSchemaException("Sch_EnumerationValue", defaultValueTyped.ToString(), baseUriStr, attdef.ValueLineNumber, attdef.ValueLinePosition);
                        eventHandling.SendEvent(exception, XmlSeverityType.Error);
                    }
                }
            }
            catch (Exception)
            {
                if (eventHandling != null)
                {
                    XmlSchemaException exception2 = new XmlSchemaException("Sch_AttributeDefaultDataType", attdef.Name.ToString());
                    eventHandling.SendEvent(exception2, XmlSeverityType.Error);
                }
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
                    object pVal = datatype.ParseValue(value, base.NameTable, namespaceManager);
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
            if (base.schemaInfo.SchemaType == SchemaType.DTD)
            {
                do
                {
                    this.ValidateEndElement();
                }
                while (this.Pop());
                this.CheckForwardRefs();
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

        private bool GenEntity(XmlQualifiedName qname)
        {
            string name = qname.Name;
            if (name[0] == '#')
            {
                return false;
            }
            if (SchemaEntity.IsPredefinedEntity(name))
            {
                return false;
            }
            SchemaEntity entity = this.GetEntity(qname, false);
            if (entity == null)
            {
                throw new XmlException("Xml_UndeclaredEntity", name);
            }
            if (!entity.NData.IsEmpty)
            {
                throw new XmlException("Xml_UnparsedEntityRef", name);
            }
            if (base.reader.StandAlone && entity.DeclaredInExternal)
            {
                base.SendValidationEvent("Sch_StandAlone");
            }
            return true;
        }

        private SchemaEntity GetEntity(XmlQualifiedName qname, bool fParameterEntity)
        {
            SchemaEntity entity;
            if (fParameterEntity)
            {
                if (base.schemaInfo.ParameterEntities.TryGetValue(qname, out entity))
                {
                    return entity;
                }
            }
            else if (base.schemaInfo.GeneralEntities.TryGetValue(qname, out entity))
            {
                return entity;
            }
            return null;
        }

        private void Init()
        {
            this.validationStack = new HWStack(10);
            base.textValue = new StringBuilder();
            this.name = XmlQualifiedName.Empty;
            this.attPresence = new Hashtable();
            base.schemaInfo = new SchemaInfo();
            base.checkDatatype = false;
            this.Push(this.name);
        }

        private bool MeetsStandAloneConstraint()
        {
            if ((base.reader.StandAlone && (base.context.ElementDecl != null)) && (base.context.ElementDecl.IsDeclaredInExternal && (base.context.ElementDecl.ContentValidator.ContentType == XmlSchemaContentType.ElementOnly)))
            {
                base.SendValidationEvent("Sch_StandAlone");
                return false;
            }
            return true;
        }

        private bool Pop()
        {
            if (this.validationStack.Length > 1)
            {
                this.validationStack.Pop();
                base.context = (ValidationState) this.validationStack.Peek();
                return true;
            }
            return false;
        }

        private void ProcessElement()
        {
            SchemaElementDecl elementDecl = base.schemaInfo.GetElementDecl(base.elementName);
            this.Push(base.elementName);
            if (elementDecl != null)
            {
                base.context.ElementDecl = elementDecl;
                this.ValidateStartElement();
                this.ValidateEndStartElement();
                base.context.NeedValidateChildren = true;
                elementDecl.ContentValidator.InitValidation(base.context);
            }
            else
            {
                base.SendValidationEvent("Sch_UndeclaredElement", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                base.context.ElementDecl = null;
            }
        }

        private void ProcessTokenizedType(XmlTokenizedType ttype, string name)
        {
            switch (ttype)
            {
                case XmlTokenizedType.ID:
                    if (!this.processIdentityConstraints)
                    {
                        break;
                    }
                    if (this.FindId(name) == null)
                    {
                        this.AddID(name, base.context.LocalName);
                        return;
                    }
                    base.SendValidationEvent("Sch_DupId", name);
                    return;

                case XmlTokenizedType.IDREF:
                    if (!this.processIdentityConstraints || (this.FindId(name) != null))
                    {
                        break;
                    }
                    this.idRefListHead = new IdRefNode(this.idRefListHead, name, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                    return;

                case XmlTokenizedType.IDREFS:
                    break;

                case XmlTokenizedType.ENTITY:
                    BaseValidator.ProcessEntity(base.schemaInfo, name, this, base.EventHandler, base.Reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
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

        public static void SetDefaultTypedValue(SchemaAttDef attdef, IDtdParserAdapter readerAdapter)
        {
            try
            {
                string defaultValueExpanded = attdef.DefaultValueExpanded;
                XmlSchemaDatatype datatype = attdef.Datatype;
                if (datatype != null)
                {
                    if (datatype.TokenizedType != XmlTokenizedType.CDATA)
                    {
                        defaultValueExpanded = defaultValueExpanded.Trim();
                    }
                    attdef.DefaultValueTyped = datatype.ParseValue(defaultValueExpanded, readerAdapter.NameTable, readerAdapter.NamespaceResolver);
                }
            }
            catch (Exception)
            {
                IValidationEventHandling validationEventHandling = ((IDtdParserAdapterWithValidation) readerAdapter).ValidationEventHandling;
                if (validationEventHandling != null)
                {
                    XmlSchemaException exception = new XmlSchemaException("Sch_AttributeDefaultDataType", attdef.Name.ToString());
                    validationEventHandling.SendEvent(exception, XmlSeverityType.Error);
                }
            }
        }

        public override void Validate()
        {
            if (base.schemaInfo.SchemaType != SchemaType.DTD)
            {
                if ((base.reader.Depth == 0) && (base.reader.NodeType == XmlNodeType.Element))
                {
                    base.SendValidationEvent("Xml_NoDTDPresent", this.name.ToString(), XmlSeverityType.Warning);
                }
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
                    case XmlNodeType.Entity:
                    case XmlNodeType.Document:
                    case XmlNodeType.DocumentType:
                    case XmlNodeType.DocumentFragment:
                    case XmlNodeType.Notation:
                        return;

                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        base.ValidateText();
                        return;

                    case XmlNodeType.EntityReference:
                        if (!this.GenEntity(new XmlQualifiedName(base.reader.LocalName, base.reader.Prefix)))
                        {
                            base.ValidateText();
                        }
                        return;

                    case XmlNodeType.ProcessingInstruction:
                    case XmlNodeType.Comment:
                        this.ValidatePIComment();
                        return;

                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        if (this.MeetsStandAloneConstraint())
                        {
                            base.ValidateWhitespace();
                        }
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
            base.elementName.Init(base.reader.LocalName, base.reader.Prefix);
            if (((base.reader.Depth == 0) && !base.schemaInfo.DocTypeName.IsEmpty) && !base.schemaInfo.DocTypeName.Equals(base.elementName))
            {
                base.SendValidationEvent("Sch_RootMatchDocType");
            }
            else
            {
                this.ValidateChildElement();
            }
            this.ProcessElement();
        }

        private void ValidateEndElement()
        {
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
            if (base.context.ElementDecl.HasRequiredAttribute)
            {
                try
                {
                    base.context.ElementDecl.CheckAttributes(this.attPresence, base.Reader.StandAlone);
                }
                catch (XmlSchemaException exception)
                {
                    exception.SetSource(base.Reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
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

        private void ValidatePIComment()
        {
            if (base.context.NeedValidateChildren && (base.context.ElementDecl.ContentValidator == ContentValidator.Empty))
            {
                base.SendValidationEvent("Sch_InvalidPIComment");
            }
        }

        private void ValidateStartElement()
        {
            if (base.context.ElementDecl != null)
            {
                base.Reader.SchemaTypeObject = base.context.ElementDecl.SchemaType;
                if (base.Reader.IsEmptyElement && (base.context.ElementDecl.DefaultValueTyped != null))
                {
                    base.Reader.TypedValueObject = base.context.ElementDecl.DefaultValueTyped;
                    base.context.IsNill = true;
                }
                if (base.context.ElementDecl.HasRequiredAttribute)
                {
                    this.attPresence.Clear();
                }
            }
            if (base.Reader.MoveToFirstAttribute())
            {
                do
                {
                    try
                    {
                        base.reader.SchemaTypeObject = null;
                        SchemaAttDef attDef = base.context.ElementDecl.GetAttDef(new XmlQualifiedName(base.reader.LocalName, base.reader.Prefix));
                        if (attDef != null)
                        {
                            if ((base.context.ElementDecl != null) && base.context.ElementDecl.HasRequiredAttribute)
                            {
                                this.attPresence.Add(attDef.Name, attDef);
                            }
                            base.Reader.SchemaTypeObject = attDef.SchemaType;
                            if ((attDef.Datatype != null) && !base.reader.IsDefault)
                            {
                                this.CheckValue(base.Reader.Value, attDef);
                            }
                        }
                        else
                        {
                            base.SendValidationEvent("Sch_UndeclaredAttribute", base.reader.Name);
                        }
                    }
                    catch (XmlSchemaException exception)
                    {
                        exception.SetSource(base.Reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                        base.SendValidationEvent(exception);
                    }
                }
                while (base.Reader.MoveToNextAttribute());
                base.Reader.MoveToElement();
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

        private class NamespaceManager : XmlNamespaceManager
        {
            public override string LookupNamespace(string prefix)
            {
                return prefix;
            }
        }
    }
}

