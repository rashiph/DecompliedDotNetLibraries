namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    internal sealed class XsdValidator : BaseValidator
    {
        private Hashtable attPresence;
        private bool bManageNamespaces;
        private static readonly XmlSchemaDatatype dtCDATA = XmlSchemaDatatype.FromXmlTokenizedType(XmlTokenizedType.CDATA);
        private static readonly XmlSchemaDatatype dtQName = XmlSchemaDatatype.FromXmlTokenizedTypeXsd(XmlTokenizedType.QName);
        private static readonly XmlSchemaDatatype dtStringArray = dtCDATA.DeriveByList(null);
        private IdRefNode idRefListHead;
        private Hashtable IDs;
        private System.Xml.Schema.Parser inlineSchemaParser;
        private XmlNamespaceManager nsManager;
        private string NsXmlNs;
        private string NsXs;
        private string NsXsi;
        private XmlSchemaContentProcessing processContents;
        private const int STACK_INCREMENT = 10;
        private int startIDConstraint;
        private HWStack validationStack;
        private string XsdSchema;
        private string XsiNil;
        private string XsiNoNamespaceSchemaLocation;
        private string XsiSchemaLocation;
        private string XsiType;

        internal XsdValidator(BaseValidator validator) : base(validator)
        {
            this.startIDConstraint = -1;
            this.Init();
        }

        internal XsdValidator(XmlValidatingReaderImpl reader, XmlSchemaCollection schemaCollection, IValidationEventHandling eventHandling) : base(reader, schemaCollection, eventHandling)
        {
            this.startIDConstraint = -1;
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

        private void AddIdentityConstraints()
        {
            base.context.Constr = new ConstraintStruct[base.context.ElementDecl.Constraints.Length];
            int num = 0;
            for (int i = 0; i < base.context.ElementDecl.Constraints.Length; i++)
            {
                base.context.Constr[num++] = new ConstraintStruct(base.context.ElementDecl.Constraints[i]);
            }
            for (int j = 0; j < base.context.Constr.Length; j++)
            {
                if (base.context.Constr[j].constraint.Role != CompiledIdentityConstraint.ConstraintRole.Keyref)
                {
                    continue;
                }
                bool flag = false;
                for (int k = this.validationStack.Length - 1; k >= ((this.startIDConstraint >= 0) ? this.startIDConstraint : (this.validationStack.Length - 1)); k--)
                {
                    if (((ValidationState) this.validationStack[k]).Constr == null)
                    {
                        continue;
                    }
                    ConstraintStruct[] constr = ((ValidationState) this.validationStack[k]).Constr;
                    for (int m = 0; m < constr.Length; m++)
                    {
                        if (constr[m].constraint.name == base.context.Constr[j].constraint.refer)
                        {
                            flag = true;
                            if (constr[m].keyrefTable == null)
                            {
                                constr[m].keyrefTable = new Hashtable();
                            }
                            base.context.Constr[j].qualifiedTable = constr[m].keyrefTable;
                            break;
                        }
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if (!flag)
                {
                    base.SendValidationEvent("Sch_RefNotInScope", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                }
            }
            if (this.startIDConstraint == -1)
            {
                this.startIDConstraint = this.validationStack.Length - 1;
            }
        }

        private void AttributeIdentityConstraints(string name, string ns, object obj, string sobj, SchemaAttDef attdef)
        {
            for (int i = this.startIDConstraint; i < this.validationStack.Length; i++)
            {
                if (((ValidationState) this.validationStack[i]).Constr != null)
                {
                    ConstraintStruct[] constr = ((ValidationState) this.validationStack[i]).Constr;
                    for (int j = 0; j < constr.Length; j++)
                    {
                        for (int k = 0; k < constr[j].axisFields.Count; k++)
                        {
                            LocatedActiveAxis axis = (LocatedActiveAxis) constr[j].axisFields[k];
                            if (axis.MoveToAttribute(name, ns))
                            {
                                if (axis.Ks[axis.Column] != null)
                                {
                                    base.SendValidationEvent("Sch_FieldSingleValueExpected", name);
                                }
                                else if ((attdef != null) && (attdef.Datatype != null))
                                {
                                    axis.Ks[axis.Column] = new TypedObject(obj, sobj, attdef.Datatype);
                                }
                            }
                        }
                    }
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
                    object pVal = datatype.ParseValue(value, base.NameTable, this.nsManager, true);
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
                    if (datatype.Variety == XmlSchemaDatatypeVariety.Union)
                    {
                        pVal = this.UnWrapUnion(pVal);
                    }
                    base.reader.TypedValueObject = pVal;
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
            this.CheckForwardRefs();
        }

        private void ElementIdentityConstraints()
        {
            for (int i = this.startIDConstraint; i < this.validationStack.Length; i++)
            {
                if (((ValidationState) this.validationStack[i]).Constr != null)
                {
                    ConstraintStruct[] constr = ((ValidationState) this.validationStack[i]).Constr;
                    for (int j = 0; j < constr.Length; j++)
                    {
                        if (constr[j].axisSelector.MoveToStartElement(base.reader.LocalName, base.reader.NamespaceURI))
                        {
                            constr[j].axisSelector.PushKS(base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                        }
                        for (int k = 0; k < constr[j].axisFields.Count; k++)
                        {
                            LocatedActiveAxis axis = (LocatedActiveAxis) constr[j].axisFields[k];
                            if (axis.MoveToStartElement(base.reader.LocalName, base.reader.NamespaceURI) && (base.context.ElementDecl != null))
                            {
                                if (base.context.ElementDecl.Datatype == null)
                                {
                                    base.SendValidationEvent("Sch_FieldSimpleTypeExpected", base.reader.LocalName);
                                }
                                else
                                {
                                    axis.isMatched = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void EndElementIdentityConstraints()
        {
            for (int i = this.validationStack.Length - 1; i >= this.startIDConstraint; i--)
            {
                if (((ValidationState) this.validationStack[i]).Constr != null)
                {
                    ConstraintStruct[] structArray = ((ValidationState) this.validationStack[i]).Constr;
                    for (int j = 0; j < structArray.Length; j++)
                    {
                        KeySequence sequence;
                        for (int k = 0; k < structArray[j].axisFields.Count; k++)
                        {
                            LocatedActiveAxis axis = (LocatedActiveAxis) structArray[j].axisFields[k];
                            if (axis.isMatched)
                            {
                                axis.isMatched = false;
                                if (axis.Ks[axis.Column] != null)
                                {
                                    base.SendValidationEvent("Sch_FieldSingleValueExpected", base.reader.LocalName);
                                }
                                else
                                {
                                    string svalue = !base.hasSibling ? base.textString : base.textValue.ToString();
                                    if ((base.reader.TypedValueObject != null) && (svalue.Length != 0))
                                    {
                                        axis.Ks[axis.Column] = new TypedObject(base.reader.TypedValueObject, svalue, base.context.ElementDecl.Datatype);
                                    }
                                }
                            }
                            axis.EndElement(base.reader.LocalName, base.reader.NamespaceURI);
                        }
                        if (structArray[j].axisSelector.EndElement(base.reader.LocalName, base.reader.NamespaceURI))
                        {
                            sequence = structArray[j].axisSelector.PopKS();
                            switch (structArray[j].constraint.Role)
                            {
                                case CompiledIdentityConstraint.ConstraintRole.Unique:
                                    if (sequence.IsQualified())
                                    {
                                        if (!structArray[j].qualifiedTable.Contains(sequence))
                                        {
                                            goto Label_02E0;
                                        }
                                        base.SendValidationEvent(new XmlSchemaException("Sch_DuplicateKey", new string[] { sequence.ToString(), structArray[j].constraint.name.ToString() }, base.reader.BaseURI, sequence.PosLine, sequence.PosCol));
                                    }
                                    break;

                                case CompiledIdentityConstraint.ConstraintRole.Key:
                                    if (sequence.IsQualified())
                                    {
                                        goto Label_01ED;
                                    }
                                    base.SendValidationEvent(new XmlSchemaException("Sch_MissingKey", structArray[j].constraint.name.ToString(), base.reader.BaseURI, sequence.PosLine, sequence.PosCol));
                                    break;

                                case CompiledIdentityConstraint.ConstraintRole.Keyref:
                                    if (((structArray[j].qualifiedTable != null) && sequence.IsQualified()) && !structArray[j].qualifiedTable.Contains(sequence))
                                    {
                                        structArray[j].qualifiedTable.Add(sequence, sequence);
                                    }
                                    break;
                            }
                        }
                        continue;
                    Label_01ED:
                        if (structArray[j].qualifiedTable.Contains(sequence))
                        {
                            base.SendValidationEvent(new XmlSchemaException("Sch_DuplicateKey", new string[] { sequence.ToString(), structArray[j].constraint.name.ToString() }, base.reader.BaseURI, sequence.PosLine, sequence.PosCol));
                        }
                        else
                        {
                            structArray[j].qualifiedTable.Add(sequence, sequence);
                        }
                        continue;
                    Label_02E0:
                        structArray[j].qualifiedTable.Add(sequence, sequence);
                    }
                }
            }
            ConstraintStruct[] constr = ((ValidationState) this.validationStack[this.validationStack.Length - 1]).Constr;
            if (constr != null)
            {
                for (int m = 0; m < constr.Length; m++)
                {
                    if ((constr[m].constraint.Role != CompiledIdentityConstraint.ConstraintRole.Keyref) && (constr[m].keyrefTable != null))
                    {
                        foreach (KeySequence sequence2 in constr[m].keyrefTable.Keys)
                        {
                            if (!constr[m].qualifiedTable.Contains(sequence2))
                            {
                                base.SendValidationEvent(new XmlSchemaException("Sch_UnresolvedKeyref", sequence2.ToString(), base.reader.BaseURI, sequence2.PosLine, sequence2.PosCol));
                            }
                        }
                    }
                }
            }
        }

        private SchemaElementDecl FastGetElementDecl(object particle)
        {
            if (particle != null)
            {
                XmlSchemaElement element = particle as XmlSchemaElement;
                if (element != null)
                {
                    return element.ElementDecl;
                }
                XmlSchemaAny any = (XmlSchemaAny) particle;
                this.processContents = any.ProcessContentsCorrect;
            }
            return null;
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
                this.bManageNamespaces = true;
            }
            this.validationStack = new HWStack(10);
            base.textValue = new StringBuilder();
            this.attPresence = new Hashtable();
            base.schemaInfo = new SchemaInfo();
            base.checkDatatype = false;
            this.processContents = XmlSchemaContentProcessing.Strict;
            this.Push(XmlQualifiedName.Empty);
            this.NsXmlNs = base.NameTable.Add("http://www.w3.org/2000/xmlns/");
            this.NsXs = base.NameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.NsXsi = base.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.XsiType = base.NameTable.Add("type");
            this.XsiNil = base.NameTable.Add("nil");
            this.XsiSchemaLocation = base.NameTable.Add("schemaLocation");
            this.XsiNoNamespaceSchemaLocation = base.NameTable.Add("noNamespaceSchemaLocation");
            this.XsdSchema = base.NameTable.Add("schema");
        }

        public bool IsXSDRoot(string localName, string ns)
        {
            return (Ref.Equal(ns, this.NsXs) && Ref.Equal(localName, this.XsdSchema));
        }

        private void LoadSchema(string uri, string url)
        {
            if ((base.XmlResolver != null) && (!base.SchemaInfo.TargetNamespaces.ContainsKey(uri) || (this.nsManager.LookupPrefix(uri) == null)))
            {
                SchemaInfo sinfo = null;
                if (base.SchemaCollection != null)
                {
                    sinfo = base.SchemaCollection.GetSchemaInfo(uri);
                }
                if (sinfo != null)
                {
                    if (sinfo.SchemaType != SchemaType.XSD)
                    {
                        throw new XmlException("Xml_MultipleValidaitonTypes", string.Empty, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition);
                    }
                    base.SchemaInfo.Add(sinfo, base.EventHandler);
                }
                else if (url != null)
                {
                    this.LoadSchemaFromLocation(uri, url);
                }
            }
        }

        private void LoadSchemaFromLocation(string uri, string url)
        {
            XmlReader reader = null;
            SchemaInfo schemaInfo = null;
            try
            {
                Uri absoluteUri = base.XmlResolver.ResolveUri(base.BaseUri, url);
                Stream input = (Stream) base.XmlResolver.GetEntity(absoluteUri, null, null);
                reader = new XmlTextReader(absoluteUri.ToString(), input, base.NameTable);
                System.Xml.Schema.Parser parser = new System.Xml.Schema.Parser(SchemaType.XSD, base.NameTable, base.SchemaNames, base.EventHandler) {
                    XmlResolver = base.XmlResolver
                };
                SchemaType type = parser.Parse(reader, uri);
                schemaInfo = new SchemaInfo {
                    SchemaType = type
                };
                if (type == SchemaType.XSD)
                {
                    if (base.SchemaCollection.EventHandler == null)
                    {
                        base.SchemaCollection.EventHandler = base.EventHandler;
                    }
                    base.SchemaCollection.Add(uri, schemaInfo, parser.XmlSchema, true);
                }
                base.SchemaInfo.Add(schemaInfo, base.EventHandler);
                while (reader.Read())
                {
                }
            }
            catch (XmlSchemaException exception)
            {
                schemaInfo = null;
                base.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception.Message }, XmlSeverityType.Error);
            }
            catch (Exception exception2)
            {
                schemaInfo = null;
                base.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception2.Message }, XmlSeverityType.Warning);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        private void Pop()
        {
            if (this.validationStack.Length > 1)
            {
                this.validationStack.Pop();
                if (this.startIDConstraint == this.validationStack.Length)
                {
                    this.startIDConstraint = -1;
                }
                base.context = (ValidationState) this.validationStack.Peek();
                this.processContents = base.context.ProcessContents;
            }
        }

        private void ProcessElement(object particle)
        {
            XmlQualifiedName name;
            string str;
            SchemaElementDecl elementDecl = this.FastGetElementDecl(particle);
            this.Push(base.elementName);
            if (this.bManageNamespaces)
            {
                this.nsManager.PushScope();
            }
            this.ProcessXsiAttributes(out name, out str);
            if (this.processContents != XmlSchemaContentProcessing.Skip)
            {
                if (((elementDecl == null) || !name.IsEmpty) || (str != null))
                {
                    elementDecl = this.ThoroughGetElementDecl(elementDecl, name, str);
                }
                if (elementDecl == null)
                {
                    if (this.HasSchema && (this.processContents == XmlSchemaContentProcessing.Strict))
                    {
                        base.SendValidationEvent("Sch_UndeclaredElement", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                    }
                    else
                    {
                        base.SendValidationEvent("Sch_NoElementSchemaFound", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace), XmlSeverityType.Warning);
                    }
                }
            }
            base.context.ElementDecl = elementDecl;
            this.ValidateStartElementIdentityConstraints();
            this.ValidateStartElement();
            if (base.context.ElementDecl != null)
            {
                this.ValidateEndStartElement();
                base.context.NeedValidateChildren = this.processContents != XmlSchemaContentProcessing.Skip;
                base.context.ElementDecl.ContentValidator.InitValidation(base.context);
            }
        }

        private void ProcessInlineSchema()
        {
            if (!this.inlineSchemaParser.ParseReaderNode())
            {
                this.inlineSchemaParser.FinishParsing();
                XmlSchema xmlSchema = this.inlineSchemaParser.XmlSchema;
                string key = null;
                if ((xmlSchema != null) && (xmlSchema.ErrorCount == 0))
                {
                    try
                    {
                        SchemaInfo schemaInfo = new SchemaInfo {
                            SchemaType = SchemaType.XSD
                        };
                        key = (xmlSchema.TargetNamespace == null) ? string.Empty : xmlSchema.TargetNamespace;
                        if (!base.SchemaInfo.TargetNamespaces.ContainsKey(key) && (base.SchemaCollection.Add(key, schemaInfo, xmlSchema, true) != null))
                        {
                            base.SchemaInfo.Add(schemaInfo, base.EventHandler);
                        }
                    }
                    catch (XmlSchemaException exception)
                    {
                        base.SendValidationEvent("Sch_CannotLoadSchema", new string[] { base.BaseUri.AbsoluteUri, exception.Message }, XmlSeverityType.Error);
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

        private void ProcessXsiAttributes(out XmlQualifiedName xsiType, out string xsiNil)
        {
            string[] strArray = null;
            string url = null;
            xsiType = XmlQualifiedName.Empty;
            xsiNil = null;
            if (base.reader.Depth == 0)
            {
                this.LoadSchema(string.Empty, null);
                foreach (string str2 in this.nsManager.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml).Values)
                {
                    this.LoadSchema(str2, null);
                }
            }
            if (base.reader.MoveToFirstAttribute())
            {
                do
                {
                    string namespaceURI = base.reader.NamespaceURI;
                    string localName = base.reader.LocalName;
                    if (Ref.Equal(namespaceURI, this.NsXmlNs))
                    {
                        this.LoadSchema(base.reader.Value, null);
                        if (this.bManageNamespaces)
                        {
                            this.nsManager.AddNamespace((base.reader.Prefix.Length == 0) ? string.Empty : base.reader.LocalName, base.reader.Value);
                        }
                    }
                    else if (Ref.Equal(namespaceURI, this.NsXsi))
                    {
                        if (Ref.Equal(localName, this.XsiSchemaLocation))
                        {
                            strArray = (string[]) dtStringArray.ParseValue(base.reader.Value, base.NameTable, this.nsManager);
                        }
                        else if (Ref.Equal(localName, this.XsiNoNamespaceSchemaLocation))
                        {
                            url = base.reader.Value;
                        }
                        else if (Ref.Equal(localName, this.XsiType))
                        {
                            xsiType = (XmlQualifiedName) dtQName.ParseValue(base.reader.Value, base.NameTable, this.nsManager);
                        }
                        else if (Ref.Equal(localName, this.XsiNil))
                        {
                            xsiNil = base.reader.Value;
                        }
                    }
                }
                while (base.reader.MoveToNextAttribute());
                base.reader.MoveToElement();
            }
            if (url != null)
            {
                this.LoadSchema(string.Empty, url);
            }
            if (strArray != null)
            {
                for (int i = 0; i < (strArray.Length - 1); i += 2)
                {
                    this.LoadSchema(strArray[i], strArray[i + 1]);
                }
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
            base.context.ProcessContents = this.processContents;
            base.context.NeedValidateChildren = false;
            base.context.Constr = null;
        }

        private SchemaElementDecl ThoroughGetElementDecl(SchemaElementDecl elementDecl, XmlQualifiedName xsiType, string xsiNil)
        {
            if (elementDecl == null)
            {
                elementDecl = base.schemaInfo.GetElementDecl(base.elementName);
            }
            if (elementDecl != null)
            {
                if (xsiType.IsEmpty)
                {
                    if (elementDecl.IsAbstract)
                    {
                        base.SendValidationEvent("Sch_AbstractElement", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                        elementDecl = null;
                    }
                }
                else if ((xsiNil != null) && xsiNil.Equals("true"))
                {
                    base.SendValidationEvent("Sch_XsiNilAndType");
                }
                else
                {
                    SchemaElementDecl decl;
                    if (!base.schemaInfo.ElementDeclsByType.TryGetValue(xsiType, out decl) && (xsiType.Namespace == this.NsXs))
                    {
                        XmlSchemaSimpleType simpleTypeFromXsdType = DatatypeImplementation.GetSimpleTypeFromXsdType(new XmlQualifiedName(xsiType.Name, this.NsXs));
                        if (simpleTypeFromXsdType != null)
                        {
                            decl = simpleTypeFromXsdType.ElementDecl;
                        }
                    }
                    if (decl == null)
                    {
                        base.SendValidationEvent("Sch_XsiTypeNotFound", xsiType.ToString());
                        elementDecl = null;
                    }
                    else if (!XmlSchemaType.IsDerivedFrom(decl.SchemaType, elementDecl.SchemaType, elementDecl.Block))
                    {
                        base.SendValidationEvent("Sch_XsiTypeBlockedEx", new string[] { xsiType.ToString(), XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace) });
                        elementDecl = null;
                    }
                    else
                    {
                        elementDecl = decl;
                    }
                }
                if ((elementDecl != null) && elementDecl.IsNillable)
                {
                    if (xsiNil != null)
                    {
                        base.context.IsNill = XmlConvert.ToBoolean(xsiNil);
                        if (base.context.IsNill && (elementDecl.DefaultValueTyped != null))
                        {
                            base.SendValidationEvent("Sch_XsiNilAndFixed");
                        }
                    }
                    return elementDecl;
                }
                if (xsiNil != null)
                {
                    base.SendValidationEvent("Sch_InvalidXsiNill");
                }
            }
            return elementDecl;
        }

        private object UnWrapUnion(object typedValue)
        {
            XsdSimpleValue value2 = typedValue as XsdSimpleValue;
            if (value2 != null)
            {
                typedValue = value2.TypedValue;
            }
            return typedValue;
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

        private object ValidateChildElement()
        {
            object obj2 = null;
            int errorCode = 0;
            if (base.context.NeedValidateChildren)
            {
                if (base.context.IsNill)
                {
                    base.SendValidationEvent("Sch_ContentInNill", base.elementName.ToString());
                    return null;
                }
                obj2 = base.context.ElementDecl.ContentValidator.ValidateElement(base.elementName, base.context, out errorCode);
                if (obj2 != null)
                {
                    return obj2;
                }
                this.processContents = base.context.ProcessContents = XmlSchemaContentProcessing.Skip;
                if (errorCode == -2)
                {
                    base.SendValidationEvent("Sch_AllElement", base.elementName.ToString());
                }
                XmlSchemaValidator.ElementValidationError(base.elementName, base.context, base.EventHandler, base.reader, base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
            }
            return obj2;
        }

        private void ValidateElement()
        {
            base.elementName.Init(base.reader.LocalName, base.reader.NamespaceURI);
            object particle = this.ValidateChildElement();
            if (this.IsXSDRoot(base.elementName.Name, base.elementName.Namespace) && (base.reader.Depth > 0))
            {
                this.inlineSchemaParser = new System.Xml.Schema.Parser(SchemaType.XSD, base.NameTable, base.SchemaNames, base.EventHandler);
                this.inlineSchemaParser.StartParsing(base.reader, null);
                this.ProcessInlineSchema();
            }
            else
            {
                this.ProcessElement(particle);
            }
        }

        private void ValidateEndElement()
        {
            if (this.bManageNamespaces)
            {
                this.nsManager.PopScope();
            }
            if (base.context.ElementDecl != null)
            {
                if (!base.context.IsNill)
                {
                    if (base.context.NeedValidateChildren && !base.context.ElementDecl.ContentValidator.CompleteValidation(base.context))
                    {
                        XmlSchemaValidator.CompleteValidationError(base.context, base.EventHandler, base.reader, base.reader.BaseURI, base.PositionInfo.LineNumber, base.PositionInfo.LinePosition, null);
                    }
                    if (base.checkDatatype && !base.context.IsNill)
                    {
                        string str = !base.hasSibling ? base.textString : base.textValue.ToString();
                        if ((str.Length != 0) || (base.context.ElementDecl.DefaultValueTyped == null))
                        {
                            this.CheckValue(str, null);
                            base.checkDatatype = false;
                        }
                    }
                }
                if (this.HasIdentityConstraints)
                {
                    this.EndElementIdentityConstraints();
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
                    SchemaAttDef attdef = (SchemaAttDef) base.context.ElementDecl.DefaultAttDefs[i];
                    base.reader.AddDefaultAttribute(attdef);
                    if (this.HasIdentityConstraints && !this.attPresence.Contains(attdef.Name))
                    {
                        this.AttributeIdentityConstraints(attdef.Name.Name, attdef.Name.Namespace, this.UnWrapUnion(attdef.DefaultValueTyped), attdef.DefaultValueRaw, attdef);
                    }
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
                if (base.context.ElementDecl.IsAbstract)
                {
                    base.SendValidationEvent("Sch_AbstractElement", XmlSchemaValidator.QNameString(base.context.LocalName, base.context.Namespace));
                }
                base.reader.SchemaTypeObject = base.context.ElementDecl.SchemaType;
                if ((base.reader.IsEmptyElement && !base.context.IsNill) && (base.context.ElementDecl.DefaultValueTyped != null))
                {
                    base.reader.TypedValueObject = this.UnWrapUnion(base.context.ElementDecl.DefaultValueTyped);
                    base.context.IsNill = true;
                }
                else
                {
                    base.reader.TypedValueObject = null;
                }
                if (base.context.ElementDecl.HasRequiredAttribute || this.HasIdentityConstraints)
                {
                    this.attPresence.Clear();
                }
            }
            if (base.reader.MoveToFirstAttribute())
            {
                do
                {
                    if ((base.reader.NamespaceURI != this.NsXmlNs) && (base.reader.NamespaceURI != this.NsXsi))
                    {
                        try
                        {
                            base.reader.SchemaTypeObject = null;
                            XmlQualifiedName qname = new XmlQualifiedName(base.reader.LocalName, base.reader.NamespaceURI);
                            bool skip = this.processContents == XmlSchemaContentProcessing.Skip;
                            SchemaAttDef def = base.schemaInfo.GetAttributeXsd(base.context.ElementDecl, qname, ref skip);
                            if (def != null)
                            {
                                if ((base.context.ElementDecl != null) && (base.context.ElementDecl.HasRequiredAttribute || (this.startIDConstraint != -1)))
                                {
                                    this.attPresence.Add(def.Name, def);
                                }
                                base.reader.SchemaTypeObject = def.SchemaType;
                                if (def.Datatype != null)
                                {
                                    this.CheckValue(base.reader.Value, def);
                                }
                                if (this.HasIdentityConstraints)
                                {
                                    this.AttributeIdentityConstraints(base.reader.LocalName, base.reader.NamespaceURI, base.reader.TypedValueObject, base.reader.Value, def);
                                }
                            }
                            else if (!skip)
                            {
                                if (((base.context.ElementDecl == null) && (this.processContents == XmlSchemaContentProcessing.Strict)) && ((qname.Namespace.Length != 0) && base.schemaInfo.Contains(qname.Namespace)))
                                {
                                    base.SendValidationEvent("Sch_UndeclaredAttribute", qname.ToString());
                                }
                                else
                                {
                                    base.SendValidationEvent("Sch_NoAttributeSchemaFound", qname.ToString(), XmlSeverityType.Warning);
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

        private void ValidateStartElementIdentityConstraints()
        {
            if (base.context.ElementDecl != null)
            {
                if (base.context.ElementDecl.Constraints != null)
                {
                    this.AddIdentityConstraints();
                }
                if (this.HasIdentityConstraints)
                {
                    this.ElementIdentityConstraints();
                }
            }
        }

        public ValidationState Context
        {
            set
            {
                base.context = value;
            }
        }

        public static XmlSchemaDatatype DtQName
        {
            get
            {
                return dtQName;
            }
        }

        private bool HasIdentityConstraints
        {
            get
            {
                return (this.startIDConstraint != -1);
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

