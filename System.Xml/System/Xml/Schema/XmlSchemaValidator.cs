namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public sealed class XmlSchemaValidator
    {
        private Hashtable attPresence;
        private bool attrValid;
        private bool checkEntity;
        private SchemaInfo compiledSchemaInfo;
        private ValidationState context;
        private XmlQualifiedName contextQName;
        private ValidatorState currentState;
        private static readonly XmlSchemaDatatype dtCDATA = XmlSchemaDatatype.FromXmlTokenizedType(XmlTokenizedType.CDATA);
        private IDtdInfo dtdSchemaInfo;
        private static readonly XmlSchemaDatatype dtQName = XmlSchemaDatatype.FromXmlTokenizedTypeXsd(XmlTokenizedType.QName);
        private static readonly XmlSchemaDatatype dtStringArray = dtCDATA.DeriveByList(null);
        private IXmlLineInfo dummyPositionInfo;
        private static XmlSchemaAttribute[] EmptyAttributeArray = new XmlSchemaAttribute[0];
        private static XmlSchemaParticle[] EmptyParticleArray = new XmlSchemaParticle[0];
        private IdRefNode idRefListHead;
        private Hashtable IDs;
        private bool isRoot;
        private static string[] MethodNames = new string[] { "None", "Initialize", "top-level ValidateAttribute", "top-level ValidateText or ValidateWhitespace", "ValidateElement", "ValidateAttribute", "ValidateEndOfAttributes", "ValidateText", "ValidateWhitespace", "ValidateEndElement", "SkipToEndElement", "EndValidation" };
        private XmlNameTable nameTable;
        private IXmlNamespaceResolver nsResolver;
        private string NsXml;
        private string NsXmlNs;
        private string NsXs;
        private string NsXsi;
        private XmlSchemaObject partialValidationType;
        private IXmlLineInfo positionInfo;
        private XmlSchemaContentProcessing processContents = XmlSchemaContentProcessing.Strict;
        private const string Quote = "'";
        private bool rootHasSchema;
        private XmlSchemaSet schemaSet;
        private Uri sourceUri;
        private string sourceUriString;
        private const int STACK_INCREMENT = 10;
        private int startIDConstraint = -1;
        private StringBuilder textValue;
        private Hashtable validatedNamespaces;
        private object validationEventSender;
        private XmlSchemaValidationFlags validationFlags;
        private HWStack validationStack;
        internal static bool[,] ValidStates = new bool[,] { { true, true, false, false, false, false, false, false, false, false, false, false }, { false, true, true, true, true, false, false, false, false, false, false, true }, { false, false, false, false, false, false, false, false, false, false, false, true }, { false, false, false, true, true, false, false, false, false, false, false, true }, { false, false, false, true, false, true, true, false, false, true, true, false }, { false, false, false, false, false, true, true, false, false, true, true, false }, { false, false, false, false, true, false, false, true, true, true, true, false }, { false, false, false, false, true, false, false, true, true, true, true, false }, { false, false, false, false, true, false, false, true, true, true, true, false }, { false, false, false, true, true, false, false, true, true, true, true, true }, { false, false, false, true, true, false, false, true, true, true, true, true }, { false, true, false, false, false, false, false, false, false, false, false, false } };
        private SchemaAttDef wildID;
        private XmlCharType xmlCharType = XmlCharType.Instance;
        private System.Xml.XmlResolver xmlResolver;
        private static XmlSchemaAttribute xsiNilSO;
        private string xsiNilString;
        private string xsiNoNamespaceSchemaLocationString;
        private static XmlSchemaAttribute xsiNoNsSLSO;
        private string xsiSchemaLocationString;
        private static XmlSchemaAttribute xsiSLSO;
        private static XmlSchemaAttribute xsiTypeSO;
        private string xsiTypeString;

        public event System.Xml.Schema.ValidationEventHandler ValidationEventHandler;

        public XmlSchemaValidator(XmlNameTable nameTable, XmlSchemaSet schemas, IXmlNamespaceResolver namespaceResolver, XmlSchemaValidationFlags validationFlags)
        {
            if (nameTable == null)
            {
                throw new ArgumentNullException("nameTable");
            }
            if (schemas == null)
            {
                throw new ArgumentNullException("schemas");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }
            this.nameTable = nameTable;
            this.nsResolver = namespaceResolver;
            this.validationFlags = validationFlags;
            if (((validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != XmlSchemaValidationFlags.None) || ((validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != XmlSchemaValidationFlags.None))
            {
                this.schemaSet = new XmlSchemaSet(nameTable);
                this.schemaSet.ValidationEventHandler += schemas.GetEventHandler();
                this.schemaSet.CompilationSettings = schemas.CompilationSettings;
                this.schemaSet.XmlResolver = schemas.GetResolver();
                this.schemaSet.Add(schemas);
                this.validatedNamespaces = new Hashtable();
            }
            else
            {
                this.schemaSet = schemas;
            }
            this.Init();
        }

        private void AddIdentityConstraints()
        {
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            this.context.Constr = new ConstraintStruct[elementDecl.Constraints.Length];
            int num = 0;
            for (int i = 0; i < elementDecl.Constraints.Length; i++)
            {
                this.context.Constr[num++] = new ConstraintStruct(elementDecl.Constraints[i]);
            }
            for (int j = 0; j < this.context.Constr.Length; j++)
            {
                if (this.context.Constr[j].constraint.Role != CompiledIdentityConstraint.ConstraintRole.Keyref)
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
                        if (constr[m].constraint.name == this.context.Constr[j].constraint.refer)
                        {
                            flag = true;
                            if (constr[m].keyrefTable == null)
                            {
                                constr[m].keyrefTable = new Hashtable();
                            }
                            this.context.Constr[j].qualifiedTable = constr[m].keyrefTable;
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
                    this.SendValidationEvent("Sch_RefNotInScope", QNameString(this.context.LocalName, this.context.Namespace));
                }
            }
            if (this.startIDConstraint == -1)
            {
                this.startIDConstraint = this.validationStack.Length - 1;
            }
        }

        public void AddSchema(XmlSchema schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException("schema");
            }
            if ((this.validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != XmlSchemaValidationFlags.None)
            {
                string targetNamespace = schema.TargetNamespace;
                if (targetNamespace == null)
                {
                    targetNamespace = string.Empty;
                }
                Hashtable schemaLocations = this.schemaSet.SchemaLocations;
                DictionaryEntry[] array = new DictionaryEntry[schemaLocations.Count];
                schemaLocations.CopyTo(array, 0);
                if ((this.validatedNamespaces[targetNamespace] != null) && (this.schemaSet.FindSchemaByNSAndUrl(schema.BaseUri, targetNamespace, array) == null))
                {
                    this.SendValidationEvent("Sch_ComponentAlreadySeenForNS", targetNamespace, XmlSeverityType.Error);
                }
                if (schema.ErrorCount == 0)
                {
                    try
                    {
                        this.schemaSet.Add(schema);
                        this.RecompileSchemaSet();
                    }
                    catch (XmlSchemaException exception)
                    {
                        this.SendValidationEvent("Sch_CannotLoadSchema", new string[] { schema.BaseUri.ToString(), exception.Message }, exception);
                    }
                    for (int i = 0; i < schema.ImportedSchemas.Count; i++)
                    {
                        XmlSchema schema2 = (XmlSchema) schema.ImportedSchemas[i];
                        targetNamespace = schema2.TargetNamespace;
                        if (targetNamespace == null)
                        {
                            targetNamespace = string.Empty;
                        }
                        if ((this.validatedNamespaces[targetNamespace] != null) && (this.schemaSet.FindSchemaByNSAndUrl(schema2.BaseUri, targetNamespace, array) == null))
                        {
                            this.SendValidationEvent("Sch_ComponentAlreadySeenForNS", targetNamespace, XmlSeverityType.Error);
                            this.schemaSet.RemoveRecursive(schema);
                            return;
                        }
                    }
                }
            }
        }

        private void AddXmlNamespaceSchema()
        {
            XmlSchemaSet schemas = new XmlSchemaSet();
            schemas.Add(Preprocessor.GetBuildInSchema());
            schemas.Compile();
            this.schemaSet.Add(schemas);
            this.RecompileSchemaSet();
        }

        private void AddXsiAttributes(ArrayList attList)
        {
            BuildXsiAttributes();
            if (this.attPresence[xsiTypeSO.QualifiedName] == null)
            {
                attList.Add(xsiTypeSO);
            }
            if (this.attPresence[xsiNilSO.QualifiedName] == null)
            {
                attList.Add(xsiNilSO);
            }
            if (this.attPresence[xsiSLSO.QualifiedName] == null)
            {
                attList.Add(xsiSLSO);
            }
            if (this.attPresence[xsiNoNsSLSO.QualifiedName] == null)
            {
                attList.Add(xsiNoNsSLSO);
            }
        }

        private void AttributeIdentityConstraints(string name, string ns, object obj, string sobj, XmlSchemaDatatype datatype)
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
                                    this.SendValidationEvent("Sch_FieldSingleValueExpected", name);
                                }
                                else
                                {
                                    axis.Ks[axis.Column] = new TypedObject(obj, sobj, datatype);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static string BuildElementName(XmlQualifiedName qname)
        {
            return BuildElementName(qname.Name, qname.Namespace);
        }

        internal static string BuildElementName(string localName, string ns)
        {
            if (ns.Length != 0)
            {
                return Res.GetString("Sch_ElementNameAndNamespace", new object[] { localName, ns });
            }
            return Res.GetString("Sch_ElementName", new object[] { localName });
        }

        private static void BuildXsiAttributes()
        {
            if (xsiTypeSO == null)
            {
                XmlSchemaAttribute attribute = new XmlSchemaAttribute {
                    Name = "type"
                };
                attribute.SetQualifiedName(new XmlQualifiedName("type", "http://www.w3.org/2001/XMLSchema-instance"));
                attribute.SetAttributeType(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.QName));
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiTypeSO, attribute, null);
            }
            if (xsiNilSO == null)
            {
                XmlSchemaAttribute attribute2 = new XmlSchemaAttribute {
                    Name = "nil"
                };
                attribute2.SetQualifiedName(new XmlQualifiedName("nil", "http://www.w3.org/2001/XMLSchema-instance"));
                attribute2.SetAttributeType(XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.Boolean));
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiNilSO, attribute2, null);
            }
            if (xsiSLSO == null)
            {
                XmlSchemaSimpleType builtInSimpleType = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
                XmlSchemaAttribute attribute3 = new XmlSchemaAttribute {
                    Name = "schemaLocation"
                };
                attribute3.SetQualifiedName(new XmlQualifiedName("schemaLocation", "http://www.w3.org/2001/XMLSchema-instance"));
                attribute3.SetAttributeType(builtInSimpleType);
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiSLSO, attribute3, null);
            }
            if (xsiNoNsSLSO == null)
            {
                XmlSchemaSimpleType type2 = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
                XmlSchemaAttribute attribute4 = new XmlSchemaAttribute {
                    Name = "noNamespaceSchemaLocation"
                };
                attribute4.SetQualifiedName(new XmlQualifiedName("noNamespaceSchemaLocation", "http://www.w3.org/2001/XMLSchema-instance"));
                attribute4.SetAttributeType(type2);
                Interlocked.CompareExchange<XmlSchemaAttribute>(ref xsiNoNsSLSO, attribute4, null);
            }
        }

        private object CheckAttributeValue(object value, SchemaAttDef attdef)
        {
            object typedValue = null;
            SchemaDeclBase decl = attdef;
            XmlSchemaDatatype datatype = attdef.Datatype;
            string s = value as string;
            Exception innerException = null;
            if (s != null)
            {
                innerException = datatype.TryParseValue(s, this.nameTable, this.nsResolver, out typedValue);
                if (innerException == null)
                {
                    goto Label_0050;
                }
                goto Label_0078;
            }
            innerException = datatype.TryParseValue(value, this.nameTable, this.nsResolver, out typedValue);
            if (innerException != null)
            {
                goto Label_0078;
            }
        Label_0050:
            if (!decl.CheckValue(typedValue))
            {
                this.attrValid = false;
                this.SendValidationEvent("Sch_FixedAttributeValue", attdef.Name.ToString());
            }
            return typedValue;
        Label_0078:
            this.attrValid = false;
            if (s == null)
            {
                s = XmlSchemaDatatype.ConcatenatedToString(value);
            }
            this.SendValidationEvent("Sch_AttributeValueDataTypeDetailed", new string[] { attdef.Name.ToString(), s, this.GetTypeName(decl), innerException.Message }, innerException);
            return null;
        }

        private void CheckElementProperties()
        {
            if (this.context.ElementDecl.IsAbstract)
            {
                this.SendValidationEvent("Sch_AbstractElement", QNameString(this.context.LocalName, this.context.Namespace));
            }
        }

        private object CheckElementValue(string stringValue)
        {
            object typedValue = null;
            SchemaDeclBase elementDecl = this.context.ElementDecl;
            Exception innerException = elementDecl.Datatype.TryParseValue(stringValue, this.nameTable, this.nsResolver, out typedValue);
            if (innerException != null)
            {
                this.SendValidationEvent("Sch_ElementValueDataTypeDetailed", new string[] { QNameString(this.context.LocalName, this.context.Namespace), stringValue, this.GetTypeName(elementDecl), innerException.Message }, innerException);
                return null;
            }
            if (!elementDecl.CheckValue(typedValue))
            {
                this.SendValidationEvent("Sch_FixedElementValue", QNameString(this.context.LocalName, this.context.Namespace));
            }
            return typedValue;
        }

        private void CheckForwardRefs()
        {
            IdRefNode next;
            for (IdRefNode node = this.idRefListHead; node != null; node = next)
            {
                if (this.FindId(node.Id) == null)
                {
                    this.SendValidationEvent(new XmlSchemaValidationException("Sch_UndeclaredId", node.Id, this.sourceUriString, node.LineNo, node.LinePos), XmlSeverityType.Error);
                }
                next = node.Next;
                node.Next = null;
            }
            this.idRefListHead = null;
        }

        private SchemaAttDef CheckIsXmlAttribute(XmlQualifiedName attQName)
        {
            SchemaAttDef def = null;
            if (Ref.Equal(attQName.Namespace, this.NsXml) && ((this.validationFlags & XmlSchemaValidationFlags.AllowXmlAttributes) != XmlSchemaValidationFlags.None))
            {
                if (!this.compiledSchemaInfo.Contains(this.NsXml))
                {
                    this.AddXmlNamespaceSchema();
                }
                this.compiledSchemaInfo.AttributeDecls.TryGetValue(attQName, out def);
            }
            return def;
        }

        internal object CheckMixedValueConstraint(string elementValue)
        {
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            if (this.context.IsNill)
            {
                return null;
            }
            if (elementValue.Length == 0)
            {
                this.context.IsDefault = true;
                return elementDecl.DefaultValueTyped;
            }
            SchemaDeclBase base2 = elementDecl;
            if ((base2.Presence == SchemaDeclBase.Use.Fixed) && !elementValue.Equals(elementDecl.DefaultValueRaw))
            {
                this.SendValidationEvent("Sch_FixedElementValue", elementDecl.Name.ToString());
            }
            return elementValue;
        }

        private void CheckRequiredAttributes(SchemaElementDecl currentElementDecl)
        {
            foreach (SchemaAttDef def in currentElementDecl.AttDefs.Values)
            {
                if ((this.attPresence[def.Name] == null) && ((def.Presence == SchemaDeclBase.Use.Required) || (def.Presence == SchemaDeclBase.Use.RequiredFixed)))
                {
                    this.SendValidationEvent("Sch_MissRequiredAttribute", def.Name.ToString());
                }
            }
        }

        private void CheckStateTransition(ValidatorState toState, string methodName)
        {
            if (!ValidStates[(int) this.currentState, (int) toState])
            {
                if (this.currentState == ValidatorState.None)
                {
                    throw new InvalidOperationException(Res.GetString("Sch_InvalidStartTransition", new string[] { methodName, MethodNames[1] }));
                }
                throw new InvalidOperationException(Res.GetString("Sch_InvalidStateTransition", new string[] { MethodNames[(int) this.currentState], methodName }));
            }
            this.currentState = toState;
        }

        private void CheckTokenizedTypes(XmlSchemaDatatype dtype, object typedValue, bool attrValue)
        {
            if (typedValue != null)
            {
                switch (dtype.TokenizedType)
                {
                    case XmlTokenizedType.ENTITY:
                    case XmlTokenizedType.ID:
                    case XmlTokenizedType.IDREF:
                        if (dtype.Variety == XmlSchemaDatatypeVariety.List)
                        {
                            string[] strArray = (string[]) typedValue;
                            for (int i = 0; i < strArray.Length; i++)
                            {
                                this.ProcessTokenizedType(dtype.TokenizedType, strArray[i], attrValue);
                            }
                            return;
                        }
                        this.ProcessTokenizedType(dtype.TokenizedType, (string) typedValue, attrValue);
                        break;
                }
            }
        }

        private SchemaElementDecl CheckXsiTypeAndNil(SchemaElementDecl elementDecl, string xsiType, string xsiNil, ref bool declFound)
        {
            XmlQualifiedName empty = XmlQualifiedName.Empty;
            if (xsiType != null)
            {
                object typedValue = null;
                Exception innerException = dtQName.TryParseValue(xsiType, this.nameTable, this.nsResolver, out typedValue);
                if (innerException != null)
                {
                    this.SendValidationEvent("Sch_InvalidValueDetailedAttribute", new string[] { "type", xsiType, dtQName.TypeCodeString, innerException.Message }, innerException);
                }
                else
                {
                    empty = typedValue as XmlQualifiedName;
                }
            }
            if (elementDecl != null)
            {
                if (elementDecl.IsNillable)
                {
                    if (xsiNil != null)
                    {
                        this.context.IsNill = XmlConvert.ToBoolean(xsiNil);
                        if (this.context.IsNill && (elementDecl.Presence == SchemaDeclBase.Use.Fixed))
                        {
                            this.SendValidationEvent("Sch_XsiNilAndFixed");
                        }
                    }
                }
                else if (xsiNil != null)
                {
                    this.SendValidationEvent("Sch_InvalidXsiNill");
                }
            }
            if (empty.IsEmpty)
            {
                if ((elementDecl != null) && elementDecl.IsAbstract)
                {
                    this.SendValidationEvent("Sch_AbstractElement", QNameString(this.context.LocalName, this.context.Namespace));
                    elementDecl = null;
                }
                return elementDecl;
            }
            SchemaElementDecl typeDecl = this.compiledSchemaInfo.GetTypeDecl(empty);
            XmlSeverityType warning = XmlSeverityType.Warning;
            if (this.HasSchema && (this.processContents == XmlSchemaContentProcessing.Strict))
            {
                warning = XmlSeverityType.Error;
            }
            if ((typeDecl == null) && (empty.Namespace == this.NsXs))
            {
                XmlSchemaType simpleTypeFromXsdType = DatatypeImplementation.GetSimpleTypeFromXsdType(empty);
                if (simpleTypeFromXsdType == null)
                {
                    simpleTypeFromXsdType = XmlSchemaType.GetBuiltInComplexType(empty);
                }
                if (simpleTypeFromXsdType != null)
                {
                    typeDecl = simpleTypeFromXsdType.ElementDecl;
                }
            }
            if (typeDecl == null)
            {
                this.SendValidationEvent("Sch_XsiTypeNotFound", empty.ToString(), warning);
                elementDecl = null;
                return elementDecl;
            }
            declFound = true;
            if (typeDecl.IsAbstract)
            {
                this.SendValidationEvent("Sch_XsiTypeAbstract", empty.ToString(), warning);
                elementDecl = null;
                return elementDecl;
            }
            if ((elementDecl != null) && !XmlSchemaType.IsDerivedFrom(typeDecl.SchemaType, elementDecl.SchemaType, elementDecl.Block))
            {
                this.SendValidationEvent("Sch_XsiTypeBlockedEx", new string[] { empty.ToString(), QNameString(this.context.LocalName, this.context.Namespace) });
                elementDecl = null;
                return elementDecl;
            }
            if (elementDecl != null)
            {
                typeDecl = typeDecl.Clone();
                typeDecl.Constraints = elementDecl.Constraints;
                typeDecl.DefaultValueRaw = elementDecl.DefaultValueRaw;
                typeDecl.DefaultValueTyped = elementDecl.DefaultValueTyped;
                typeDecl.Block = elementDecl.Block;
            }
            this.context.ElementDeclBeforeXsi = elementDecl;
            elementDecl = typeDecl;
            return elementDecl;
        }

        private void ClearPSVI()
        {
            if (this.textValue != null)
            {
                this.textValue.Length = 0;
            }
            this.attPresence.Clear();
            this.wildID = null;
        }

        internal static void CompleteValidationError(ValidationState context, System.Xml.Schema.ValidationEventHandler eventHandler, object sender, string sourceUri, int lineNo, int linePos, XmlSchemaSet schemaSet)
        {
            ArrayList expected = null;
            bool getParticles = schemaSet != null;
            if (context.ElementDecl != null)
            {
                if (getParticles)
                {
                    expected = context.ElementDecl.ContentValidator.ExpectedParticles(context, true, schemaSet);
                }
                else
                {
                    expected = context.ElementDecl.ContentValidator.ExpectedElements(context, true);
                }
            }
            if ((expected == null) || (expected.Count == 0))
            {
                if (context.TooComplex)
                {
                    SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_IncompleteContentComplex", new string[] { BuildElementName(context.LocalName, context.Namespace), Res.GetString("Sch_ComplexContentModel") }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                }
                SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_IncompleteContent", BuildElementName(context.LocalName, context.Namespace), sourceUri, lineNo, linePos), XmlSeverityType.Error);
            }
            else if (context.TooComplex)
            {
                SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_IncompleteContentExpectingComplex", new string[] { BuildElementName(context.LocalName, context.Namespace), PrintExpectedElements(expected, getParticles), Res.GetString("Sch_ComplexContentModel") }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
            }
            else
            {
                SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_IncompleteContentExpecting", new string[] { BuildElementName(context.LocalName, context.Namespace), PrintExpectedElements(expected, getParticles) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
            }
        }

        private void ElementIdentityConstraints()
        {
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            string localName = this.context.LocalName;
            string uRN = this.context.Namespace;
            for (int i = this.startIDConstraint; i < this.validationStack.Length; i++)
            {
                if (((ValidationState) this.validationStack[i]).Constr != null)
                {
                    ConstraintStruct[] constr = ((ValidationState) this.validationStack[i]).Constr;
                    for (int j = 0; j < constr.Length; j++)
                    {
                        if (constr[j].axisSelector.MoveToStartElement(localName, uRN))
                        {
                            constr[j].axisSelector.PushKS(this.positionInfo.LineNumber, this.positionInfo.LinePosition);
                        }
                        for (int k = 0; k < constr[j].axisFields.Count; k++)
                        {
                            LocatedActiveAxis axis = (LocatedActiveAxis) constr[j].axisFields[k];
                            if (axis.MoveToStartElement(localName, uRN) && (elementDecl != null))
                            {
                                if ((elementDecl.Datatype == null) || (elementDecl.ContentValidator.ContentType == XmlSchemaContentType.Mixed))
                                {
                                    this.SendValidationEvent("Sch_FieldSimpleTypeExpected", localName);
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

        internal static void ElementValidationError(XmlQualifiedName name, ValidationState context, System.Xml.Schema.ValidationEventHandler eventHandler, object sender, string sourceUri, int lineNo, int linePos, XmlSchemaSet schemaSet)
        {
            ArrayList expected = null;
            if (context.ElementDecl != null)
            {
                ContentValidator contentValidator = context.ElementDecl.ContentValidator;
                XmlSchemaContentType contentType = contentValidator.ContentType;
                if ((contentType == XmlSchemaContentType.ElementOnly) || (((contentType == XmlSchemaContentType.Mixed) && (contentValidator != ContentValidator.Mixed)) && (contentValidator != ContentValidator.Any)))
                {
                    bool getParticles = schemaSet != null;
                    if (getParticles)
                    {
                        expected = contentValidator.ExpectedParticles(context, false, schemaSet);
                    }
                    else
                    {
                        expected = contentValidator.ExpectedElements(context, false);
                    }
                    if ((expected == null) || (expected.Count == 0))
                    {
                        if (context.TooComplex)
                        {
                            SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_InvalidElementContentComplex", new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name), Res.GetString("Sch_ComplexContentModel") }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                        }
                        else
                        {
                            SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_InvalidElementContent", new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                        }
                    }
                    else if (context.TooComplex)
                    {
                        SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_InvalidElementContentExpectingComplex", new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name), PrintExpectedElements(expected, getParticles), Res.GetString("Sch_ComplexContentModel") }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                    }
                    else
                    {
                        SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_InvalidElementContentExpecting", new string[] { BuildElementName(context.LocalName, context.Namespace), BuildElementName(name), PrintExpectedElements(expected, getParticles) }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                    }
                }
                else if (contentType == XmlSchemaContentType.Empty)
                {
                    SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_InvalidElementInEmptyEx", new string[] { QNameString(context.LocalName, context.Namespace), name.ToString() }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                }
                else if (!contentValidator.IsOpen)
                {
                    SendValidationEvent(eventHandler, sender, new XmlSchemaValidationException("Sch_InvalidElementInTextOnlyEx", new string[] { QNameString(context.LocalName, context.Namespace), name.ToString() }, sourceUri, lineNo, linePos), XmlSeverityType.Error);
                }
            }
        }

        private void EndElementIdentityConstraints(object typedValue, string stringValue, XmlSchemaDatatype datatype)
        {
            string localName = this.context.LocalName;
            string uRN = this.context.Namespace;
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
                                    this.SendValidationEvent("Sch_FieldSingleValueExpected", localName);
                                }
                                else if ((typedValue != null) && (stringValue.Length != 0))
                                {
                                    axis.Ks[axis.Column] = new TypedObject(typedValue, stringValue, datatype);
                                }
                            }
                            axis.EndElement(localName, uRN);
                        }
                        if (structArray[j].axisSelector.EndElement(localName, uRN))
                        {
                            sequence = structArray[j].axisSelector.PopKS();
                            switch (structArray[j].constraint.Role)
                            {
                                case CompiledIdentityConstraint.ConstraintRole.Unique:
                                    if (sequence.IsQualified())
                                    {
                                        if (!structArray[j].qualifiedTable.Contains(sequence))
                                        {
                                            goto Label_0283;
                                        }
                                        this.SendValidationEvent(new XmlSchemaValidationException("Sch_DuplicateKey", new string[] { sequence.ToString(), structArray[j].constraint.name.ToString() }, this.sourceUriString, sequence.PosLine, sequence.PosCol));
                                    }
                                    break;

                                case CompiledIdentityConstraint.ConstraintRole.Key:
                                    if (sequence.IsQualified())
                                    {
                                        goto Label_0195;
                                    }
                                    this.SendValidationEvent(new XmlSchemaValidationException("Sch_MissingKey", structArray[j].constraint.name.ToString(), this.sourceUriString, sequence.PosLine, sequence.PosCol));
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
                    Label_0195:
                        if (structArray[j].qualifiedTable.Contains(sequence))
                        {
                            this.SendValidationEvent(new XmlSchemaValidationException("Sch_DuplicateKey", new string[] { sequence.ToString(), structArray[j].constraint.name.ToString() }, this.sourceUriString, sequence.PosLine, sequence.PosCol));
                        }
                        else
                        {
                            structArray[j].qualifiedTable.Add(sequence, sequence);
                        }
                        continue;
                    Label_0283:
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
                                this.SendValidationEvent(new XmlSchemaValidationException("Sch_UnresolvedKeyref", sequence2.ToString(), this.sourceUriString, sequence2.PosLine, sequence2.PosCol));
                            }
                        }
                    }
                }
            }
        }

        public void EndValidation()
        {
            if (this.validationStack.Length > 1)
            {
                throw new InvalidOperationException(Res.GetString("Sch_InvalidEndValidation"));
            }
            this.CheckStateTransition(ValidatorState.Finish, MethodNames[11]);
            this.CheckForwardRefs();
        }

        private static void EnumerateAny(StringBuilder builder, string namespaces)
        {
            StringBuilder builder2 = new StringBuilder();
            if ((namespaces == "##any") || (namespaces == "##other"))
            {
                builder2.Append(namespaces);
            }
            else
            {
                string[] strArray = XmlConvert.SplitString(namespaces);
                builder2.Append(strArray[0]);
                for (int i = 1; i < strArray.Length; i++)
                {
                    builder2.Append(", ");
                    builder2.Append(strArray[i]);
                }
            }
            builder.Append(Res.GetString("Sch_AnyElementNS", new object[] { builder2.ToString() }));
        }

        private SchemaElementDecl FastGetElementDecl(XmlQualifiedName elementName, object particle)
        {
            SchemaElementDecl elementDecl = null;
            if (particle != null)
            {
                XmlSchemaElement element = particle as XmlSchemaElement;
                if (element != null)
                {
                    elementDecl = element.ElementDecl;
                }
                else
                {
                    XmlSchemaAny any = (XmlSchemaAny) particle;
                    this.processContents = any.ProcessContentsCorrect;
                }
            }
            if ((elementDecl != null) || (this.processContents == XmlSchemaContentProcessing.Skip))
            {
                return elementDecl;
            }
            if (this.isRoot && (this.partialValidationType != null))
            {
                if (this.partialValidationType is XmlSchemaElement)
                {
                    XmlSchemaElement partialValidationType = (XmlSchemaElement) this.partialValidationType;
                    if (elementName.Equals(partialValidationType.QualifiedName))
                    {
                        return partialValidationType.ElementDecl;
                    }
                    this.SendValidationEvent("Sch_SchemaElementNameMismatch", elementName.ToString(), partialValidationType.QualifiedName.ToString());
                    return elementDecl;
                }
                if (this.partialValidationType is XmlSchemaType)
                {
                    XmlSchemaType type = (XmlSchemaType) this.partialValidationType;
                    return type.ElementDecl;
                }
                this.SendValidationEvent("Sch_ValidateElementInvalidCall", string.Empty);
                return elementDecl;
            }
            return this.compiledSchemaInfo.GetElementDecl(elementName);
        }

        private object FindId(string name)
        {
            if (this.IDs != null)
            {
                return this.IDs[name];
            }
            return null;
        }

        internal string GetConcatenatedValue()
        {
            return this.textValue.ToString();
        }

        internal string GetDefaultAttributePrefix(string attributeNS)
        {
            IDictionary<string, string> namespacesInScope = this.nsResolver.GetNamespacesInScope(XmlNamespaceScope.All);
            string key = null;
            foreach (KeyValuePair<string, string> pair in namespacesInScope)
            {
                if (Ref.Equal(this.nameTable.Add(pair.Value), attributeNS))
                {
                    key = pair.Key;
                    if (key.Length != 0)
                    {
                        return key;
                    }
                }
            }
            return key;
        }

        public XmlSchemaAttribute[] GetExpectedAttributes()
        {
            if ((this.currentState == ValidatorState.Element) || (this.currentState == ValidatorState.Attribute))
            {
                SchemaElementDecl elementDecl = this.context.ElementDecl;
                ArrayList attList = new ArrayList();
                if (elementDecl != null)
                {
                    foreach (SchemaAttDef def in elementDecl.AttDefs.Values)
                    {
                        if (this.attPresence[def.Name] == null)
                        {
                            attList.Add(def.SchemaAttribute);
                        }
                    }
                }
                if (this.nsResolver.LookupPrefix(this.NsXsi) != null)
                {
                    this.AddXsiAttributes(attList);
                }
                return (attList.ToArray(typeof(XmlSchemaAttribute)) as XmlSchemaAttribute[]);
            }
            if ((this.currentState == ValidatorState.Start) && (this.partialValidationType != null))
            {
                XmlSchemaAttribute partialValidationType = this.partialValidationType as XmlSchemaAttribute;
                if (partialValidationType != null)
                {
                    return new XmlSchemaAttribute[] { partialValidationType };
                }
            }
            return EmptyAttributeArray;
        }

        public XmlSchemaParticle[] GetExpectedParticles()
        {
            if ((this.currentState == ValidatorState.Start) || (this.currentState == ValidatorState.TopLevelTextOrWS))
            {
                if (this.partialValidationType != null)
                {
                    XmlSchemaElement partialValidationType = this.partialValidationType as XmlSchemaElement;
                    if (partialValidationType != null)
                    {
                        return new XmlSchemaParticle[] { partialValidationType };
                    }
                    return EmptyParticleArray;
                }
                ICollection values = this.schemaSet.GlobalElements.Values;
                ArrayList particles = new ArrayList(values.Count);
                foreach (XmlSchemaElement element2 in values)
                {
                    ContentValidator.AddParticleToExpected(element2, this.schemaSet, particles, true);
                }
                return (particles.ToArray(typeof(XmlSchemaParticle)) as XmlSchemaParticle[]);
            }
            if (this.context.ElementDecl != null)
            {
                ArrayList list2 = this.context.ElementDecl.ContentValidator.ExpectedParticles(this.context, false, this.schemaSet);
                if (list2 != null)
                {
                    return (list2.ToArray(typeof(XmlSchemaParticle)) as XmlSchemaParticle[]);
                }
            }
            return EmptyParticleArray;
        }

        private XmlSchemaElement GetSchemaElement()
        {
            SchemaElementDecl elementDeclBeforeXsi = this.context.ElementDeclBeforeXsi;
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            if ((elementDeclBeforeXsi != null) && (elementDeclBeforeXsi.SchemaElement != null))
            {
                XmlSchemaElement element = (XmlSchemaElement) elementDeclBeforeXsi.SchemaElement.Clone(null);
                element.SchemaTypeName = XmlQualifiedName.Empty;
                element.SchemaType = elementDecl.SchemaType;
                element.SetElementType(elementDecl.SchemaType);
                element.ElementDecl = elementDecl;
                return element;
            }
            return elementDecl.SchemaElement;
        }

        private XmlSchemaElement GetSubstitutionGroupHead(XmlQualifiedName member)
        {
            XmlSchemaElement element = this.compiledSchemaInfo.GetElement(member);
            if (element != null)
            {
                XmlQualifiedName substitutionGroup = element.SubstitutionGroup;
                if (!substitutionGroup.IsEmpty)
                {
                    XmlSchemaElement element2 = this.compiledSchemaInfo.GetElement(substitutionGroup);
                    if (element2 != null)
                    {
                        if ((element2.BlockResolved & XmlSchemaDerivationMethod.Substitution) != XmlSchemaDerivationMethod.Empty)
                        {
                            this.SendValidationEvent("Sch_SubstitutionNotAllowed", new string[] { member.ToString(), substitutionGroup.ToString() });
                            return null;
                        }
                        if (!XmlSchemaType.IsDerivedFrom(element.ElementSchemaType, element2.ElementSchemaType, element2.BlockResolved))
                        {
                            this.SendValidationEvent("Sch_SubstitutionBlocked", new string[] { member.ToString(), substitutionGroup.ToString() });
                            return null;
                        }
                        return element2;
                    }
                }
            }
            return null;
        }

        private string GetTypeName(SchemaDeclBase decl)
        {
            string typeCodeString = decl.SchemaType.QualifiedName.ToString();
            if (typeCodeString.Length == 0)
            {
                typeCodeString = decl.Datatype.TypeCodeString;
            }
            return typeCodeString;
        }

        public void GetUnspecifiedDefaultAttributes(ArrayList defaultAttributes)
        {
            if (defaultAttributes == null)
            {
                throw new ArgumentNullException("defaultAttributes");
            }
            this.CheckStateTransition(ValidatorState.Attribute, "GetUnspecifiedDefaultAttributes");
            this.GetUnspecifiedDefaultAttributes(defaultAttributes, false);
        }

        internal void GetUnspecifiedDefaultAttributes(ArrayList defaultAttributes, bool createNodeData)
        {
            this.currentState = ValidatorState.Attribute;
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            if ((elementDecl != null) && elementDecl.HasDefaultAttribute)
            {
                for (int i = 0; i < elementDecl.DefaultAttDefs.Count; i++)
                {
                    SchemaAttDef def = (SchemaAttDef) elementDecl.DefaultAttDefs[i];
                    if (!this.attPresence.Contains(def.Name) && (def.DefaultValueTyped != null))
                    {
                        string attributeNS = this.nameTable.Add(def.Name.Namespace);
                        string array = string.Empty;
                        if (attributeNS.Length > 0)
                        {
                            array = this.GetDefaultAttributePrefix(attributeNS);
                            if ((array == null) || (array.Length == 0))
                            {
                                this.SendValidationEvent("Sch_DefaultAttributeNotApplied", new string[] { def.Name.ToString(), QNameString(this.context.LocalName, this.context.Namespace) });
                                continue;
                            }
                        }
                        XmlSchemaDatatype dtype = def.Datatype;
                        if (createNodeData)
                        {
                            ValidatingReaderNodeData data = new ValidatingReaderNodeData {
                                LocalName = this.nameTable.Add(def.Name.Name),
                                Namespace = attributeNS,
                                Prefix = this.nameTable.Add(array),
                                NodeType = XmlNodeType.Attribute
                            };
                            AttributePSVIInfo info = new AttributePSVIInfo();
                            XmlSchemaInfo attributeSchemaInfo = info.attributeSchemaInfo;
                            if (def.Datatype.Variety == XmlSchemaDatatypeVariety.Union)
                            {
                                XsdSimpleValue defaultValueTyped = def.DefaultValueTyped as XsdSimpleValue;
                                attributeSchemaInfo.MemberType = defaultValueTyped.XmlType;
                                dtype = defaultValueTyped.XmlType.Datatype;
                                info.typedAttributeValue = defaultValueTyped.TypedValue;
                            }
                            else
                            {
                                info.typedAttributeValue = def.DefaultValueTyped;
                            }
                            attributeSchemaInfo.IsDefault = true;
                            attributeSchemaInfo.Validity = XmlSchemaValidity.Valid;
                            attributeSchemaInfo.SchemaType = def.SchemaType;
                            attributeSchemaInfo.SchemaAttribute = def.SchemaAttribute;
                            data.RawValue = attributeSchemaInfo.XmlType.ValueConverter.ToString(info.typedAttributeValue);
                            data.AttInfo = info;
                            defaultAttributes.Add(data);
                        }
                        else
                        {
                            defaultAttributes.Add(def.SchemaAttribute);
                        }
                        this.CheckTokenizedTypes(dtype, def.DefaultValueTyped, true);
                        if (this.HasIdentityConstraints)
                        {
                            this.AttributeIdentityConstraints(def.Name.Name, def.Name.Namespace, def.DefaultValueTyped, def.DefaultValueRaw, dtype);
                        }
                    }
                }
            }
        }

        private void Init()
        {
            this.validationStack = new HWStack(10);
            this.attPresence = new Hashtable();
            this.Push(XmlQualifiedName.Empty);
            this.dummyPositionInfo = new PositionInfo();
            this.positionInfo = this.dummyPositionInfo;
            this.validationEventSender = this;
            this.currentState = ValidatorState.None;
            this.textValue = new StringBuilder(100);
            this.xmlResolver = new XmlUrlResolver();
            this.contextQName = new XmlQualifiedName();
            this.Reset();
            this.RecompileSchemaSet();
            this.NsXs = this.nameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.NsXsi = this.nameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.NsXmlNs = this.nameTable.Add("http://www.w3.org/2000/xmlns/");
            this.NsXml = this.nameTable.Add("http://www.w3.org/XML/1998/namespace");
            this.xsiTypeString = this.nameTable.Add("type");
            this.xsiNilString = this.nameTable.Add("nil");
            this.xsiSchemaLocationString = this.nameTable.Add("schemaLocation");
            this.xsiNoNamespaceSchemaLocationString = this.nameTable.Add("noNamespaceSchemaLocation");
        }

        public void Initialize()
        {
            if ((this.currentState != ValidatorState.None) && (this.currentState != ValidatorState.Finish))
            {
                throw new InvalidOperationException(Res.GetString("Sch_InvalidStateTransition", new string[] { MethodNames[(int) this.currentState], MethodNames[1] }));
            }
            this.currentState = ValidatorState.Start;
            this.Reset();
        }

        public void Initialize(XmlSchemaObject partialValidationType)
        {
            if ((this.currentState != ValidatorState.None) && (this.currentState != ValidatorState.Finish))
            {
                throw new InvalidOperationException(Res.GetString("Sch_InvalidStateTransition", new string[] { MethodNames[(int) this.currentState], MethodNames[1] }));
            }
            if (partialValidationType == null)
            {
                throw new ArgumentNullException("partialValidationType");
            }
            if ((!(partialValidationType is XmlSchemaElement) && !(partialValidationType is XmlSchemaAttribute)) && !(partialValidationType is XmlSchemaType))
            {
                throw new ArgumentException(Res.GetString("Sch_InvalidPartialValidationType"));
            }
            this.currentState = ValidatorState.Start;
            this.Reset();
            this.partialValidationType = partialValidationType;
        }

        private object InternalValidateEndElement(XmlSchemaInfo schemaInfo, object typedValue)
        {
            if (this.validationStack.Length <= 1)
            {
                throw new InvalidOperationException(Res.GetString("Sch_InvalidEndElementMultiple", new object[] { MethodNames[9] }));
            }
            this.CheckStateTransition(ValidatorState.EndElement, MethodNames[9]);
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            XmlSchemaSimpleType memberType = null;
            XmlSchemaType schemaType = null;
            XmlSchemaElement schemaElement = null;
            string stringValue = string.Empty;
            if (elementDecl != null)
            {
                if (this.context.CheckRequiredAttribute && elementDecl.HasRequiredAttribute)
                {
                    this.CheckRequiredAttributes(elementDecl);
                }
                if (!this.context.IsNill && this.context.NeedValidateChildren)
                {
                    switch (elementDecl.ContentValidator.ContentType)
                    {
                        case XmlSchemaContentType.TextOnly:
                            if (typedValue != null)
                            {
                                typedValue = this.ValidateAtomicValue(typedValue, out memberType);
                                break;
                            }
                            stringValue = this.textValue.ToString();
                            typedValue = this.ValidateAtomicValue(stringValue, out memberType);
                            break;

                        case XmlSchemaContentType.ElementOnly:
                            if (typedValue != null)
                            {
                                throw new InvalidOperationException(Res.GetString("Sch_InvalidEndElementCallTyped"));
                            }
                            break;

                        case XmlSchemaContentType.Mixed:
                            if ((elementDecl.DefaultValueTyped != null) && (typedValue == null))
                            {
                                stringValue = this.textValue.ToString();
                                typedValue = this.CheckMixedValueConstraint(stringValue);
                            }
                            break;
                    }
                    if (!elementDecl.ContentValidator.CompleteValidation(this.context))
                    {
                        CompleteValidationError(this.context, this.eventHandler, this.nsResolver, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition, this.schemaSet);
                        this.context.Validity = XmlSchemaValidity.Invalid;
                    }
                }
                if (this.HasIdentityConstraints)
                {
                    XmlSchemaType type4 = (memberType == null) ? elementDecl.SchemaType : memberType;
                    this.EndElementIdentityConstraints(typedValue, stringValue, type4.Datatype);
                }
                schemaType = elementDecl.SchemaType;
                schemaElement = this.GetSchemaElement();
            }
            if (schemaInfo != null)
            {
                schemaInfo.SchemaType = schemaType;
                schemaInfo.SchemaElement = schemaElement;
                schemaInfo.MemberType = memberType;
                schemaInfo.IsNil = this.context.IsNill;
                schemaInfo.IsDefault = this.context.IsDefault;
                if ((this.context.Validity == XmlSchemaValidity.NotKnown) && this.StrictlyAssessed)
                {
                    this.context.Validity = XmlSchemaValidity.Valid;
                }
                schemaInfo.Validity = this.context.Validity;
            }
            this.Pop();
            return typedValue;
        }

        private void LoadSchema(string uri, string url)
        {
            XmlReader reader = null;
            try
            {
                Uri absoluteUri = this.xmlResolver.ResolveUri(this.sourceUri, url);
                Stream input = (Stream) this.xmlResolver.GetEntity(absoluteUri, null, null);
                XmlReaderSettings readerSettings = this.schemaSet.ReaderSettings;
                readerSettings.CloseInput = true;
                readerSettings.XmlResolver = this.xmlResolver;
                reader = XmlReader.Create(input, readerSettings, absoluteUri.ToString());
                this.schemaSet.Add(uri, reader, this.validatedNamespaces);
                while (reader.Read())
                {
                }
            }
            catch (XmlSchemaException exception)
            {
                this.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception.Message }, exception);
            }
            catch (Exception exception2)
            {
                this.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception2.Message }, exception2, XmlSeverityType.Warning);
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
            ValidationState state = (ValidationState) this.validationStack.Pop();
            if (this.startIDConstraint == this.validationStack.Length)
            {
                this.startIDConstraint = -1;
            }
            this.context = (ValidationState) this.validationStack.Peek();
            if (state.Validity == XmlSchemaValidity.Invalid)
            {
                this.context.Validity = XmlSchemaValidity.Invalid;
            }
            if (state.ValidationSkipped)
            {
                this.context.ValidationSkipped = true;
            }
            this.processContents = this.context.ProcessContents;
        }

        internal static string PrintExpectedElements(ArrayList expected, bool getParticles)
        {
            if (!getParticles)
            {
                return PrintNames(expected);
            }
            string str = Res.GetString("Sch_ContinuationString", new string[] { " " });
            XmlSchemaParticle particle = null;
            XmlSchemaParticle particle2 = null;
            ArrayList list = new ArrayList();
            StringBuilder builder = new StringBuilder();
            if (expected.Count == 1)
            {
                particle2 = expected[0] as XmlSchemaParticle;
            }
            else
            {
                for (int i = 1; i < expected.Count; i++)
                {
                    particle = expected[i - 1] as XmlSchemaParticle;
                    particle2 = expected[i] as XmlSchemaParticle;
                    XmlQualifiedName qualifiedName = particle.GetQualifiedName();
                    if (qualifiedName.Namespace != particle2.GetQualifiedName().Namespace)
                    {
                        list.Add(qualifiedName);
                        PrintNamesWithNS(list, builder);
                        list.Clear();
                        builder.Append(str);
                    }
                    else
                    {
                        list.Add(qualifiedName);
                    }
                }
            }
            list.Add(particle2.GetQualifiedName());
            PrintNamesWithNS(list, builder);
            return builder.ToString();
        }

        private static string PrintNames(ArrayList expected)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("'");
            builder.Append(expected[0].ToString());
            for (int i = 1; i < expected.Count; i++)
            {
                builder.Append(" ");
                builder.Append(expected[i].ToString());
            }
            builder.Append("'");
            return builder.ToString();
        }

        private static void PrintNamesWithNS(ArrayList expected, StringBuilder builder)
        {
            XmlQualifiedName name = null;
            name = expected[0] as XmlQualifiedName;
            if (expected.Count == 1)
            {
                if (name.Name == "*")
                {
                    EnumerateAny(builder, name.Namespace);
                }
                else if (name.Namespace.Length != 0)
                {
                    builder.Append(Res.GetString("Sch_ElementNameAndNamespace", new object[] { name.Name, name.Namespace }));
                }
                else
                {
                    builder.Append(Res.GetString("Sch_ElementName", new object[] { name.Name }));
                }
            }
            else
            {
                bool flag = false;
                bool flag2 = true;
                StringBuilder builder2 = new StringBuilder();
                for (int i = 0; i < expected.Count; i++)
                {
                    name = expected[i] as XmlQualifiedName;
                    if (name.Name == "*")
                    {
                        flag = true;
                    }
                    else
                    {
                        if (flag2)
                        {
                            flag2 = false;
                        }
                        else
                        {
                            builder2.Append(", ");
                        }
                        builder2.Append(name.Name);
                    }
                }
                if (flag)
                {
                    builder2.Append(", ");
                    builder2.Append(Res.GetString("Sch_AnyElement"));
                }
                else if (name.Namespace.Length != 0)
                {
                    builder.Append(Res.GetString("Sch_ElementNameAndNamespace", new object[] { builder2.ToString(), name.Namespace }));
                }
                else
                {
                    builder.Append(Res.GetString("Sch_ElementName", new object[] { builder2.ToString() }));
                }
            }
        }

        private void ProcessEntity(string name)
        {
            if (this.checkEntity)
            {
                IDtdEntityInfo entity = null;
                if (this.dtdSchemaInfo != null)
                {
                    entity = this.dtdSchemaInfo.LookupEntity(name);
                }
                if (entity == null)
                {
                    this.SendValidationEvent("Sch_UndeclaredEntity", name);
                }
                else if (entity.IsUnparsedEntity)
                {
                    this.SendValidationEvent("Sch_UnparsedEntityRef", name);
                }
            }
        }

        private void ProcessSchemaLocations(string xsiSchemaLocation, string xsiNoNamespaceSchemaLocation)
        {
            bool flag = false;
            if (xsiNoNamespaceSchemaLocation != null)
            {
                flag = true;
                this.LoadSchema(string.Empty, xsiNoNamespaceSchemaLocation);
            }
            if (xsiSchemaLocation != null)
            {
                object obj2;
                Exception innerException = dtStringArray.TryParseValue(xsiSchemaLocation, this.nameTable, this.nsResolver, out obj2);
                if (innerException != null)
                {
                    this.SendValidationEvent("Sch_InvalidValueDetailedAttribute", new string[] { "schemaLocation", xsiSchemaLocation, dtStringArray.TypeCodeString, innerException.Message }, innerException);
                    return;
                }
                string[] strArray = (string[]) obj2;
                flag = true;
                try
                {
                    for (int i = 0; i < (strArray.Length - 1); i += 2)
                    {
                        this.LoadSchema(strArray[i], strArray[i + 1]);
                    }
                }
                catch (XmlSchemaException exception2)
                {
                    this.SendValidationEvent(exception2);
                }
            }
            if (flag)
            {
                this.RecompileSchemaSet();
            }
        }

        private void ProcessTokenizedType(XmlTokenizedType ttype, string name, bool attrValue)
        {
            switch (ttype)
            {
                case XmlTokenizedType.ID:
                    if (!this.ProcessIdentityConstraints)
                    {
                        break;
                    }
                    if (this.FindId(name) == null)
                    {
                        if (this.IDs == null)
                        {
                            this.IDs = new Hashtable();
                        }
                        this.IDs.Add(name, this.context.LocalName);
                        return;
                    }
                    if (attrValue)
                    {
                        this.attrValid = false;
                    }
                    this.SendValidationEvent("Sch_DupId", name);
                    return;

                case XmlTokenizedType.IDREF:
                    if (!this.ProcessIdentityConstraints || (this.FindId(name) != null))
                    {
                        break;
                    }
                    this.idRefListHead = new IdRefNode(this.idRefListHead, name, this.positionInfo.LineNumber, this.positionInfo.LinePosition);
                    return;

                case XmlTokenizedType.IDREFS:
                    break;

                case XmlTokenizedType.ENTITY:
                    this.ProcessEntity(name);
                    break;

                default:
                    return;
            }
        }

        private void Push(XmlQualifiedName elementName)
        {
            this.context = (ValidationState) this.validationStack.Push();
            if (this.context == null)
            {
                this.context = new ValidationState();
                this.validationStack.AddToTop(this.context);
            }
            this.context.LocalName = elementName.Name;
            this.context.Namespace = elementName.Namespace;
            this.context.HasMatched = false;
            this.context.IsNill = false;
            this.context.IsDefault = false;
            this.context.CheckRequiredAttribute = true;
            this.context.ValidationSkipped = false;
            this.context.Validity = XmlSchemaValidity.NotKnown;
            this.context.NeedValidateChildren = false;
            this.context.ProcessContents = this.processContents;
            this.context.ElementDeclBeforeXsi = null;
            this.context.Constr = null;
        }

        internal static string QNameString(string localName, string ns)
        {
            if (ns.Length == 0)
            {
                return localName;
            }
            return (ns + ":" + localName);
        }

        internal void RecompileSchemaSet()
        {
            if (!this.schemaSet.IsCompiled)
            {
                try
                {
                    this.schemaSet.Compile();
                }
                catch (XmlSchemaException exception)
                {
                    this.SendValidationEvent(exception);
                }
            }
            this.compiledSchemaInfo = this.schemaSet.CompiledInfo;
        }

        private void Reset()
        {
            this.isRoot = true;
            this.rootHasSchema = true;
            while (this.validationStack.Length > 1)
            {
                this.validationStack.Pop();
            }
            this.startIDConstraint = -1;
            this.partialValidationType = null;
            if (this.IDs != null)
            {
                this.IDs.Clear();
            }
            if (this.ProcessSchemaHints)
            {
                this.validatedNamespaces.Clear();
            }
        }

        private void SaveTextValue(object value)
        {
            string str = value.ToString();
            this.textValue.Append(str);
        }

        private void SendValidationEvent(string code)
        {
            this.SendValidationEvent(code, string.Empty);
        }

        private void SendValidationEvent(XmlSchemaException e)
        {
            this.SendValidationEvent(new XmlSchemaValidationException(e.GetRes, e.Args, e.SourceUri, e.LineNumber, e.LinePosition), XmlSeverityType.Error);
        }

        private void SendValidationEvent(XmlSchemaValidationException e)
        {
            this.SendValidationEvent(e, XmlSeverityType.Error);
        }

        private void SendValidationEvent(string code, string[] args)
        {
            this.SendValidationEvent(new XmlSchemaValidationException(code, args, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string arg)
        {
            this.SendValidationEvent(new XmlSchemaValidationException(code, arg, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SendValidationEvent(XmlSchemaValidationException e, XmlSeverityType severity)
        {
            bool flag = false;
            if (severity == XmlSeverityType.Error)
            {
                flag = true;
                this.context.Validity = XmlSchemaValidity.Invalid;
            }
            if (flag)
            {
                if (this.eventHandler == null)
                {
                    throw e;
                }
                this.eventHandler(this.validationEventSender, new ValidationEventArgs(e, severity));
            }
            else if (this.ReportValidationWarnings && (this.eventHandler != null))
            {
                this.eventHandler(this.validationEventSender, new ValidationEventArgs(e, severity));
            }
        }

        private void SendValidationEvent(string code, string[] args, Exception innerException)
        {
            this.SendValidationEvent(new XmlSchemaValidationException(code, args, innerException, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition), XmlSeverityType.Error);
        }

        private void SendValidationEvent(string code, string arg1, string arg2)
        {
            this.SendValidationEvent(new XmlSchemaValidationException(code, new string[] { arg1, arg2 }, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition));
        }

        private void SendValidationEvent(string code, string msg, XmlSeverityType severity)
        {
            if ((severity != XmlSeverityType.Warning) || this.ReportValidationWarnings)
            {
                this.SendValidationEvent(new XmlSchemaValidationException(code, msg, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
            }
        }

        private void SendValidationEvent(string code, string[] args, Exception innerException, XmlSeverityType severity)
        {
            if ((severity != XmlSeverityType.Warning) || this.ReportValidationWarnings)
            {
                this.SendValidationEvent(new XmlSchemaValidationException(code, args, innerException, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
            }
        }

        internal static void SendValidationEvent(System.Xml.Schema.ValidationEventHandler eventHandler, object sender, XmlSchemaValidationException e, XmlSeverityType severity)
        {
            if (eventHandler != null)
            {
                eventHandler(sender, new ValidationEventArgs(e, severity));
            }
            else if (severity == XmlSeverityType.Error)
            {
                throw e;
            }
        }

        internal void SetDtdSchemaInfo(IDtdInfo dtdSchemaInfo)
        {
            this.dtdSchemaInfo = dtdSchemaInfo;
            this.checkEntity = true;
        }

        public void SkipToEndElement(XmlSchemaInfo schemaInfo)
        {
            if (this.validationStack.Length <= 1)
            {
                throw new InvalidOperationException(Res.GetString("Sch_InvalidEndElementMultiple", new object[] { MethodNames[10] }));
            }
            this.CheckStateTransition(ValidatorState.SkipToEndElement, MethodNames[10]);
            if (schemaInfo != null)
            {
                SchemaElementDecl elementDecl = this.context.ElementDecl;
                if (elementDecl != null)
                {
                    schemaInfo.SchemaType = elementDecl.SchemaType;
                    schemaInfo.SchemaElement = this.GetSchemaElement();
                }
                else
                {
                    schemaInfo.SchemaType = null;
                    schemaInfo.SchemaElement = null;
                }
                schemaInfo.MemberType = null;
                schemaInfo.IsNil = this.context.IsNill;
                schemaInfo.IsDefault = this.context.IsDefault;
                schemaInfo.Validity = this.context.Validity;
            }
            this.context.ValidationSkipped = true;
            this.currentState = ValidatorState.SkipToEndElement;
            this.Pop();
        }

        private void ThrowDeclNotFoundWarningOrError(bool declFound)
        {
            if (declFound)
            {
                this.processContents = this.context.ProcessContents = XmlSchemaContentProcessing.Skip;
                this.context.NeedValidateChildren = false;
            }
            else if (this.HasSchema && (this.processContents == XmlSchemaContentProcessing.Strict))
            {
                this.processContents = this.context.ProcessContents = XmlSchemaContentProcessing.Skip;
                this.context.NeedValidateChildren = false;
                this.SendValidationEvent("Sch_UndeclaredElement", QNameString(this.context.LocalName, this.context.Namespace));
            }
            else
            {
                this.SendValidationEvent("Sch_NoElementSchemaFound", QNameString(this.context.LocalName, this.context.Namespace), XmlSeverityType.Warning);
            }
        }

        private object ValidateAtomicValue(object parsedValue, out XmlSchemaSimpleType memberType)
        {
            memberType = null;
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            object typedValue = null;
            if (!this.context.IsNill)
            {
                SchemaDeclBase decl = elementDecl;
                XmlSchemaDatatype dtype = elementDecl.Datatype;
                Exception innerException = dtype.TryParseValue(parsedValue, this.nameTable, this.nsResolver, out typedValue);
                if (innerException != null)
                {
                    string str = parsedValue as string;
                    if (str == null)
                    {
                        str = XmlSchemaDatatype.ConcatenatedToString(parsedValue);
                    }
                    this.SendValidationEvent("Sch_ElementValueDataTypeDetailed", new string[] { QNameString(this.context.LocalName, this.context.Namespace), str, this.GetTypeName(decl), innerException.Message }, innerException);
                    return null;
                }
                if (!decl.CheckValue(typedValue))
                {
                    this.SendValidationEvent("Sch_FixedElementValue", QNameString(this.context.LocalName, this.context.Namespace));
                }
                if (dtype.Variety == XmlSchemaDatatypeVariety.Union)
                {
                    XsdSimpleValue value2 = typedValue as XsdSimpleValue;
                    memberType = value2.XmlType;
                    typedValue = value2.TypedValue;
                    dtype = memberType.Datatype;
                }
                this.CheckTokenizedTypes(dtype, typedValue, false);
            }
            return typedValue;
        }

        private object ValidateAtomicValue(string stringValue, out XmlSchemaSimpleType memberType)
        {
            object typedValue = null;
            memberType = null;
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            if (!this.context.IsNill)
            {
                if ((stringValue.Length == 0) && (elementDecl.DefaultValueTyped != null))
                {
                    SchemaElementDecl elementDeclBeforeXsi = this.context.ElementDeclBeforeXsi;
                    if ((elementDeclBeforeXsi != null) && (elementDeclBeforeXsi != elementDecl))
                    {
                        if (elementDecl.Datatype.TryParseValue(elementDecl.DefaultValueRaw, this.nameTable, this.nsResolver, out typedValue) != null)
                        {
                            this.SendValidationEvent("Sch_InvalidElementDefaultValue", new string[] { elementDecl.DefaultValueRaw, QNameString(this.context.LocalName, this.context.Namespace) });
                        }
                        else
                        {
                            this.context.IsDefault = true;
                        }
                    }
                    else
                    {
                        this.context.IsDefault = true;
                        typedValue = elementDecl.DefaultValueTyped;
                    }
                }
                else
                {
                    typedValue = this.CheckElementValue(stringValue);
                }
                XsdSimpleValue value2 = typedValue as XsdSimpleValue;
                XmlSchemaDatatype dtype = elementDecl.Datatype;
                if (value2 != null)
                {
                    memberType = value2.XmlType;
                    typedValue = value2.TypedValue;
                    dtype = memberType.Datatype;
                }
                this.CheckTokenizedTypes(dtype, typedValue, false);
            }
            return typedValue;
        }

        public object ValidateAttribute(string localName, string namespaceUri, string attributeValue, XmlSchemaInfo schemaInfo)
        {
            if (attributeValue == null)
            {
                throw new ArgumentNullException("attributeValue");
            }
            return this.ValidateAttribute(localName, namespaceUri, null, attributeValue, schemaInfo);
        }

        public object ValidateAttribute(string localName, string namespaceUri, XmlValueGetter attributeValue, XmlSchemaInfo schemaInfo)
        {
            if (attributeValue == null)
            {
                throw new ArgumentNullException("attributeValue");
            }
            return this.ValidateAttribute(localName, namespaceUri, attributeValue, null, schemaInfo);
        }

        private object ValidateAttribute(string lName, string ns, XmlValueGetter attributeValueGetter, string attributeStringValue, XmlSchemaInfo schemaInfo)
        {
            if (lName == null)
            {
                throw new ArgumentNullException("localName");
            }
            if (ns == null)
            {
                throw new ArgumentNullException("namespaceUri");
            }
            ValidatorState toState = (this.validationStack.Length > 1) ? ValidatorState.Attribute : ValidatorState.TopLevelAttribute;
            this.CheckStateTransition(toState, MethodNames[(int) toState]);
            object typedValue = null;
            this.attrValid = true;
            XmlSchemaValidity notKnown = XmlSchemaValidity.NotKnown;
            XmlSchemaAttribute schemaAttribute = null;
            XmlSchemaSimpleType xmlType = null;
            ns = this.nameTable.Add(ns);
            if (Ref.Equal(ns, this.NsXmlNs))
            {
                return null;
            }
            SchemaAttDef def = null;
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            XmlQualifiedName key = new XmlQualifiedName(lName, ns);
            if (this.attPresence[key] != null)
            {
                this.SendValidationEvent("Sch_DuplicateAttribute", key.ToString());
                if (schemaInfo != null)
                {
                    schemaInfo.Clear();
                }
                return null;
            }
            if (Ref.Equal(ns, this.NsXsi))
            {
                lName = this.nameTable.Add(lName);
                if ((Ref.Equal(lName, this.xsiTypeString) || Ref.Equal(lName, this.xsiNilString)) || (Ref.Equal(lName, this.xsiSchemaLocationString) || Ref.Equal(lName, this.xsiNoNamespaceSchemaLocationString)))
                {
                    this.attPresence.Add(key, SchemaAttDef.Empty);
                }
                else
                {
                    this.attrValid = false;
                    this.SendValidationEvent("Sch_NotXsiAttribute", key.ToString());
                }
            }
            else
            {
                AttributeMatchState state2;
                object obj4;
                XmlSchemaObject partialValidationType = (this.currentState == ValidatorState.TopLevelAttribute) ? this.partialValidationType : null;
                def = this.compiledSchemaInfo.GetAttributeXsd(elementDecl, key, partialValidationType, out state2);
                switch (state2)
                {
                    case AttributeMatchState.AttributeFound:
                        break;

                    case AttributeMatchState.AnyIdAttributeFound:
                        if (this.wildID != null)
                        {
                            this.SendValidationEvent("Sch_MoreThanOneWildId", string.Empty);
                        }
                        else
                        {
                            this.wildID = def;
                            XmlSchemaComplexType schemaType = elementDecl.SchemaType as XmlSchemaComplexType;
                            if (!schemaType.ContainsIdAttribute(false))
                            {
                                break;
                            }
                            this.SendValidationEvent("Sch_AttrUseAndWildId", string.Empty);
                        }
                        goto Label_0409;

                    case AttributeMatchState.UndeclaredElementAndAttribute:
                        def = this.CheckIsXmlAttribute(key);
                        if (def != null)
                        {
                            break;
                        }
                        if (((elementDecl != null) || (this.processContents != XmlSchemaContentProcessing.Strict)) || ((key.Namespace.Length == 0) || !this.compiledSchemaInfo.Contains(key.Namespace)))
                        {
                            if (this.processContents != XmlSchemaContentProcessing.Skip)
                            {
                                this.SendValidationEvent("Sch_NoAttributeSchemaFound", key.ToString(), XmlSeverityType.Warning);
                            }
                        }
                        else
                        {
                            this.attrValid = false;
                            this.SendValidationEvent("Sch_UndeclaredAttribute", key.ToString());
                        }
                        goto Label_0409;

                    case AttributeMatchState.UndeclaredAttribute:
                        def = this.CheckIsXmlAttribute(key);
                        if (def != null)
                        {
                            break;
                        }
                        this.attrValid = false;
                        this.SendValidationEvent("Sch_UndeclaredAttribute", key.ToString());
                        goto Label_0409;

                    case AttributeMatchState.AnyAttributeLax:
                        this.SendValidationEvent("Sch_NoAttributeSchemaFound", key.ToString(), XmlSeverityType.Warning);
                        goto Label_0409;

                    case AttributeMatchState.ProhibitedAnyAttribute:
                        def = this.CheckIsXmlAttribute(key);
                        if (def != null)
                        {
                            break;
                        }
                        this.attrValid = false;
                        this.SendValidationEvent("Sch_ProhibitedAttribute", key.ToString());
                        goto Label_0409;

                    case AttributeMatchState.ProhibitedAttribute:
                        this.attrValid = false;
                        this.SendValidationEvent("Sch_ProhibitedAttribute", key.ToString());
                        goto Label_0409;

                    case AttributeMatchState.AttributeNameMismatch:
                        this.attrValid = false;
                        this.SendValidationEvent("Sch_SchemaAttributeNameMismatch", new string[] { key.ToString(), ((XmlSchemaAttribute) partialValidationType).QualifiedName.ToString() });
                        goto Label_0409;

                    case AttributeMatchState.ValidateAttributeInvalidCall:
                        this.currentState = ValidatorState.Start;
                        this.attrValid = false;
                        this.SendValidationEvent("Sch_ValidateAttributeInvalidCall", string.Empty);
                        goto Label_0409;

                    default:
                        goto Label_0409;
                }
                schemaAttribute = def.SchemaAttribute;
                if (elementDecl != null)
                {
                    this.attPresence.Add(key, def);
                }
                if (attributeValueGetter != null)
                {
                    obj4 = attributeValueGetter();
                }
                else
                {
                    obj4 = attributeStringValue;
                }
                typedValue = this.CheckAttributeValue(obj4, def);
                XmlSchemaDatatype dtype = def.Datatype;
                if ((dtype.Variety == XmlSchemaDatatypeVariety.Union) && (typedValue != null))
                {
                    XsdSimpleValue value2 = typedValue as XsdSimpleValue;
                    xmlType = value2.XmlType;
                    dtype = value2.XmlType.Datatype;
                    typedValue = value2.TypedValue;
                }
                this.CheckTokenizedTypes(dtype, typedValue, true);
                if (this.HasIdentityConstraints)
                {
                    this.AttributeIdentityConstraints(key.Name, key.Namespace, typedValue, obj4.ToString(), dtype);
                }
            }
        Label_0409:
            if (!this.attrValid)
            {
                notKnown = XmlSchemaValidity.Invalid;
            }
            else if (def != null)
            {
                notKnown = XmlSchemaValidity.Valid;
            }
            if (schemaInfo != null)
            {
                schemaInfo.SchemaAttribute = schemaAttribute;
                schemaInfo.SchemaType = (schemaAttribute == null) ? null : schemaAttribute.AttributeSchemaType;
                schemaInfo.MemberType = xmlType;
                schemaInfo.IsDefault = false;
                schemaInfo.Validity = notKnown;
            }
            if (this.ProcessSchemaHints && (this.validatedNamespaces[ns] == null))
            {
                this.validatedNamespaces.Add(ns, ns);
            }
            return typedValue;
        }

        public void ValidateElement(string localName, string namespaceUri, XmlSchemaInfo schemaInfo)
        {
            this.ValidateElement(localName, namespaceUri, schemaInfo, null, null, null, null);
        }

        public void ValidateElement(string localName, string namespaceUri, XmlSchemaInfo schemaInfo, string xsiType, string xsiNil, string xsiSchemaLocation, string xsiNoNamespaceSchemaLocation)
        {
            bool flag;
            if (localName == null)
            {
                throw new ArgumentNullException("localName");
            }
            if (namespaceUri == null)
            {
                throw new ArgumentNullException("namespaceUri");
            }
            this.CheckStateTransition(ValidatorState.Element, MethodNames[4]);
            this.ClearPSVI();
            this.contextQName.Init(localName, namespaceUri);
            XmlQualifiedName contextQName = this.contextQName;
            object particle = this.ValidateElementContext(contextQName, out flag);
            SchemaElementDecl elementDecl = this.FastGetElementDecl(contextQName, particle);
            this.Push(contextQName);
            if (flag)
            {
                this.context.Validity = XmlSchemaValidity.Invalid;
            }
            if (((this.validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != XmlSchemaValidationFlags.None) && (this.xmlResolver != null))
            {
                this.ProcessSchemaLocations(xsiSchemaLocation, xsiNoNamespaceSchemaLocation);
            }
            if (this.processContents != XmlSchemaContentProcessing.Skip)
            {
                if ((elementDecl == null) && (this.partialValidationType == null))
                {
                    elementDecl = this.compiledSchemaInfo.GetElementDecl(contextQName);
                }
                bool declFound = elementDecl != null;
                if ((xsiType != null) || (xsiNil != null))
                {
                    elementDecl = this.CheckXsiTypeAndNil(elementDecl, xsiType, xsiNil, ref declFound);
                }
                if (elementDecl == null)
                {
                    this.ThrowDeclNotFoundWarningOrError(declFound);
                }
            }
            this.context.ElementDecl = elementDecl;
            XmlSchemaElement schemaElement = null;
            XmlSchemaType schemaType = null;
            if (elementDecl != null)
            {
                this.CheckElementProperties();
                this.attPresence.Clear();
                this.context.NeedValidateChildren = this.processContents != XmlSchemaContentProcessing.Skip;
                this.ValidateStartElementIdentityConstraints();
                elementDecl.ContentValidator.InitValidation(this.context);
                schemaType = elementDecl.SchemaType;
                schemaElement = this.GetSchemaElement();
            }
            if (schemaInfo != null)
            {
                schemaInfo.SchemaType = schemaType;
                schemaInfo.SchemaElement = schemaElement;
                schemaInfo.IsNil = this.context.IsNill;
                schemaInfo.Validity = this.context.Validity;
            }
            if (this.ProcessSchemaHints && (this.validatedNamespaces[namespaceUri] == null))
            {
                this.validatedNamespaces.Add(namespaceUri, namespaceUri);
            }
            if (this.isRoot)
            {
                this.isRoot = false;
            }
        }

        private object ValidateElementContext(XmlQualifiedName elementName, out bool invalidElementInContext)
        {
            object element = null;
            int errorCode = 0;
            XmlSchemaElement substitutionGroupHead = null;
            invalidElementInContext = false;
            if (!this.context.NeedValidateChildren)
            {
                return element;
            }
            if (this.context.IsNill)
            {
                this.SendValidationEvent("Sch_ContentInNill", QNameString(this.context.LocalName, this.context.Namespace));
                return null;
            }
            if ((this.context.ElementDecl.ContentValidator.ContentType == XmlSchemaContentType.Mixed) && (this.context.ElementDecl.Presence == SchemaDeclBase.Use.Fixed))
            {
                this.SendValidationEvent("Sch_ElementInMixedWithFixed", QNameString(this.context.LocalName, this.context.Namespace));
                return null;
            }
            XmlQualifiedName qualifiedName = elementName;
            bool flag = false;
        Label_00AA:
            element = this.context.ElementDecl.ContentValidator.ValidateElement(qualifiedName, this.context, out errorCode);
            if (element == null)
            {
                if (errorCode == -2)
                {
                    this.SendValidationEvent("Sch_AllElement", elementName.ToString());
                    invalidElementInContext = true;
                    this.processContents = this.context.ProcessContents = XmlSchemaContentProcessing.Skip;
                    return null;
                }
                flag = true;
                substitutionGroupHead = this.GetSubstitutionGroupHead(qualifiedName);
                if (substitutionGroupHead != null)
                {
                    qualifiedName = substitutionGroupHead.QualifiedName;
                    goto Label_00AA;
                }
            }
            if (flag)
            {
                XmlSchemaElement element2 = element as XmlSchemaElement;
                if (element2 == null)
                {
                    element = null;
                }
                else if (element2.RefName.IsEmpty)
                {
                    this.SendValidationEvent("Sch_InvalidElementSubstitution", BuildElementName(elementName), BuildElementName(element2.QualifiedName));
                    invalidElementInContext = true;
                    this.processContents = this.context.ProcessContents = XmlSchemaContentProcessing.Skip;
                }
                else
                {
                    element = this.compiledSchemaInfo.GetElement(elementName);
                    this.context.NeedValidateChildren = true;
                }
            }
            if (element == null)
            {
                ElementValidationError(elementName, this.context, this.eventHandler, this.nsResolver, this.sourceUriString, this.positionInfo.LineNumber, this.positionInfo.LinePosition, this.schemaSet);
                invalidElementInContext = true;
                this.processContents = this.context.ProcessContents = XmlSchemaContentProcessing.Skip;
            }
            return element;
        }

        public object ValidateEndElement(XmlSchemaInfo schemaInfo)
        {
            return this.InternalValidateEndElement(schemaInfo, null);
        }

        public object ValidateEndElement(XmlSchemaInfo schemaInfo, object typedValue)
        {
            if (typedValue == null)
            {
                throw new ArgumentNullException("typedValue");
            }
            if (this.textValue.Length > 0)
            {
                throw new InvalidOperationException(Res.GetString("Sch_InvalidEndElementCall"));
            }
            return this.InternalValidateEndElement(schemaInfo, typedValue);
        }

        public void ValidateEndOfAttributes(XmlSchemaInfo schemaInfo)
        {
            this.CheckStateTransition(ValidatorState.EndOfAttributes, MethodNames[6]);
            SchemaElementDecl elementDecl = this.context.ElementDecl;
            if ((elementDecl != null) && elementDecl.HasRequiredAttribute)
            {
                this.context.CheckRequiredAttribute = false;
                this.CheckRequiredAttributes(elementDecl);
            }
            if (schemaInfo != null)
            {
                schemaInfo.Validity = this.context.Validity;
            }
        }

        private void ValidateStartElementIdentityConstraints()
        {
            if (this.ProcessIdentityConstraints && (this.context.ElementDecl.Constraints != null))
            {
                this.AddIdentityConstraints();
            }
            if (this.HasIdentityConstraints)
            {
                this.ElementIdentityConstraints();
            }
        }

        public void ValidateText(string elementValue)
        {
            if (elementValue == null)
            {
                throw new ArgumentNullException("elementValue");
            }
            this.ValidateText(elementValue, null);
        }

        public void ValidateText(XmlValueGetter elementValue)
        {
            if (elementValue == null)
            {
                throw new ArgumentNullException("elementValue");
            }
            this.ValidateText(null, elementValue);
        }

        private void ValidateText(string elementStringValue, XmlValueGetter elementValueGetter)
        {
            ValidatorState toState = (this.validationStack.Length > 1) ? ValidatorState.Text : ValidatorState.TopLevelTextOrWS;
            this.CheckStateTransition(toState, MethodNames[(int) toState]);
            if (this.context.NeedValidateChildren)
            {
                if (this.context.IsNill)
                {
                    this.SendValidationEvent("Sch_ContentInNill", QNameString(this.context.LocalName, this.context.Namespace));
                }
                else
                {
                    switch (this.context.ElementDecl.ContentValidator.ContentType)
                    {
                        case XmlSchemaContentType.TextOnly:
                            if (elementValueGetter == null)
                            {
                                this.SaveTextValue(elementStringValue);
                                return;
                            }
                            this.SaveTextValue(elementValueGetter());
                            return;

                        case XmlSchemaContentType.Empty:
                            this.SendValidationEvent("Sch_InvalidTextInEmpty", string.Empty);
                            return;

                        case XmlSchemaContentType.ElementOnly:
                        {
                            string str = (elementValueGetter != null) ? elementValueGetter().ToString() : elementStringValue;
                            if (!this.xmlCharType.IsOnlyWhitespace(str))
                            {
                                ArrayList expected = this.context.ElementDecl.ContentValidator.ExpectedParticles(this.context, false, this.schemaSet);
                                if ((expected == null) || (expected.Count == 0))
                                {
                                    this.SendValidationEvent("Sch_InvalidTextInElement", BuildElementName(this.context.LocalName, this.context.Namespace));
                                    return;
                                }
                                this.SendValidationEvent("Sch_InvalidTextInElementExpecting", new string[] { BuildElementName(this.context.LocalName, this.context.Namespace), PrintExpectedElements(expected, true) });
                                return;
                            }
                            return;
                        }
                        case XmlSchemaContentType.Mixed:
                            if (this.context.ElementDecl.DefaultValueTyped == null)
                            {
                                break;
                            }
                            if (elementValueGetter == null)
                            {
                                this.SaveTextValue(elementStringValue);
                                break;
                            }
                            this.SaveTextValue(elementValueGetter());
                            return;

                        default:
                            return;
                    }
                }
            }
        }

        public void ValidateWhitespace(string elementValue)
        {
            if (elementValue == null)
            {
                throw new ArgumentNullException("elementValue");
            }
            this.ValidateWhitespace(elementValue, null);
        }

        public void ValidateWhitespace(XmlValueGetter elementValue)
        {
            if (elementValue == null)
            {
                throw new ArgumentNullException("elementValue");
            }
            this.ValidateWhitespace(null, elementValue);
        }

        private void ValidateWhitespace(string elementStringValue, XmlValueGetter elementValueGetter)
        {
            ValidatorState toState = (this.validationStack.Length > 1) ? ValidatorState.Whitespace : ValidatorState.TopLevelTextOrWS;
            this.CheckStateTransition(toState, MethodNames[(int) toState]);
            if (this.context.NeedValidateChildren)
            {
                if (this.context.IsNill)
                {
                    this.SendValidationEvent("Sch_ContentInNill", QNameString(this.context.LocalName, this.context.Namespace));
                }
                switch (this.context.ElementDecl.ContentValidator.ContentType)
                {
                    case XmlSchemaContentType.TextOnly:
                        if (elementValueGetter == null)
                        {
                            this.SaveTextValue(elementStringValue);
                            return;
                        }
                        this.SaveTextValue(elementValueGetter());
                        return;

                    case XmlSchemaContentType.Empty:
                        this.SendValidationEvent("Sch_InvalidWhitespaceInEmpty", string.Empty);
                        return;

                    case XmlSchemaContentType.ElementOnly:
                        break;

                    case XmlSchemaContentType.Mixed:
                        if (this.context.ElementDecl.DefaultValueTyped == null)
                        {
                            break;
                        }
                        if (elementValueGetter == null)
                        {
                            this.SaveTextValue(elementStringValue);
                            break;
                        }
                        this.SaveTextValue(elementValueGetter());
                        return;

                    default:
                        return;
                }
            }
        }

        internal XmlSchemaContentType CurrentContentType
        {
            get
            {
                if (this.context.ElementDecl == null)
                {
                    return XmlSchemaContentType.Empty;
                }
                return this.context.ElementDecl.ContentValidator.ContentType;
            }
        }

        internal XmlSchemaContentProcessing CurrentProcessContents
        {
            get
            {
                return this.processContents;
            }
        }

        private bool HasIdentityConstraints
        {
            get
            {
                return (this.ProcessIdentityConstraints && (this.startIDConstraint != -1));
            }
        }

        private bool HasSchema
        {
            get
            {
                if (this.isRoot)
                {
                    this.isRoot = false;
                    if (!this.compiledSchemaInfo.Contains(this.context.Namespace))
                    {
                        this.rootHasSchema = false;
                    }
                }
                return this.rootHasSchema;
            }
        }

        public IXmlLineInfo LineInfoProvider
        {
            get
            {
                return this.positionInfo;
            }
            set
            {
                if (value == null)
                {
                    this.positionInfo = this.dummyPositionInfo;
                }
                else
                {
                    this.positionInfo = value;
                }
            }
        }

        internal bool ProcessIdentityConstraints
        {
            get
            {
                return ((this.validationFlags & XmlSchemaValidationFlags.ProcessIdentityConstraints) != XmlSchemaValidationFlags.None);
            }
        }

        internal bool ProcessInlineSchema
        {
            get
            {
                return ((this.validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) != XmlSchemaValidationFlags.None);
            }
        }

        internal bool ProcessSchemaHints
        {
            get
            {
                if ((this.validationFlags & XmlSchemaValidationFlags.ProcessInlineSchema) == XmlSchemaValidationFlags.None)
                {
                    return ((this.validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != XmlSchemaValidationFlags.None);
                }
                return true;
            }
        }

        internal bool ProcessSchemaLocation
        {
            get
            {
                return ((this.validationFlags & XmlSchemaValidationFlags.ProcessSchemaLocation) != XmlSchemaValidationFlags.None);
            }
        }

        internal bool ReportValidationWarnings
        {
            get
            {
                return ((this.validationFlags & XmlSchemaValidationFlags.ReportValidationWarnings) != XmlSchemaValidationFlags.None);
            }
        }

        internal XmlSchemaSet SchemaSet
        {
            get
            {
                return this.schemaSet;
            }
        }

        public Uri SourceUri
        {
            get
            {
                return this.sourceUri;
            }
            set
            {
                this.sourceUri = value;
                this.sourceUriString = this.sourceUri.ToString();
            }
        }

        private bool StrictlyAssessed
        {
            get
            {
                return ((((this.processContents == XmlSchemaContentProcessing.Strict) || (this.processContents == XmlSchemaContentProcessing.Lax)) && (this.context.ElementDecl != null)) && !this.context.ValidationSkipped);
            }
        }

        public object ValidationEventSender
        {
            get
            {
                return this.validationEventSender;
            }
            set
            {
                this.validationEventSender = value;
            }
        }

        internal XmlSchemaValidationFlags ValidationFlags
        {
            get
            {
                return this.validationFlags;
            }
        }

        public System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.xmlResolver = value;
            }
        }
    }
}

