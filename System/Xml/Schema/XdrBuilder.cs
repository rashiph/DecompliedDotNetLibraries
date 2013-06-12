namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal sealed class XdrBuilder : SchemaBuilder
    {
        private AttributeContent _AttributeDef;
        private DeclBaseInfo _BaseDecl;
        private ParticleContentValidator _contentValidator;
        private XmlNamespaceManager _CurNsMgr;
        private XdrEntry _CurState;
        private ElementContent _ElementDef;
        private GroupContent _GroupDef;
        private HWStack _GroupStack;
        private XmlNameTable _NameTable;
        private XdrEntry _NextState;
        private XmlReader _reader;
        private SchemaInfo _SchemaInfo;
        private SchemaNames _SchemaNames;
        private HWStack _StateHistory;
        private string _TargetNamespace;
        private string _Text;
        private Hashtable _UndeclaredElements = new Hashtable();
        private DeclBaseInfo _UndefinedAttributeTypes;
        private string _XdrName;
        private string _XdrPrefix;
        private PositionInfo positionInfo;
        private static readonly XdrEntry[] S_SchemaEntries = new XdrEntry[] { new XdrEntry(SchemaNames.Token.Empty, S_XDR_Root_Element, null, null, null, null, false), new XdrEntry(SchemaNames.Token.XdrRoot, S_XDR_Root_SubElements, S_XDR_Root_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitRoot), new XdrBeginChildFunction(XdrBuilder.XDR_BeginRoot), new XdrEndChildFunction(XdrBuilder.XDR_EndRoot), false), new XdrEntry(SchemaNames.Token.XdrElementType, S_XDR_ElementType_SubElements, S_XDR_ElementType_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitElementType), new XdrBeginChildFunction(XdrBuilder.XDR_BeginElementType), new XdrEndChildFunction(XdrBuilder.XDR_EndElementType), false), new XdrEntry(SchemaNames.Token.XdrAttributeType, S_XDR_AttributeType_SubElements, S_XDR_AttributeType_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitAttributeType), new XdrBeginChildFunction(XdrBuilder.XDR_BeginAttributeType), new XdrEndChildFunction(XdrBuilder.XDR_EndAttributeType), false), new XdrEntry(SchemaNames.Token.XdrElement, null, S_XDR_Element_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitElement), null, new XdrEndChildFunction(XdrBuilder.XDR_EndElement), false), new XdrEntry(SchemaNames.Token.XdrAttribute, null, S_XDR_Attribute_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitAttribute), new XdrBeginChildFunction(XdrBuilder.XDR_BeginAttribute), new XdrEndChildFunction(XdrBuilder.XDR_EndAttribute), false), new XdrEntry(SchemaNames.Token.XdrGroup, S_XDR_Group_SubElements, S_XDR_Group_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitGroup), null, new XdrEndChildFunction(XdrBuilder.XDR_EndGroup), false), new XdrEntry(SchemaNames.Token.XdrDatatype, null, S_XDR_ElementDataType_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitElementDtType), null, new XdrEndChildFunction(XdrBuilder.XDR_EndElementDtType), true), new XdrEntry(SchemaNames.Token.XdrDatatype, null, S_XDR_AttributeDataType_Attributes, new XdrInitFunction(XdrBuilder.XDR_InitAttributeDtType), null, new XdrEndChildFunction(XdrBuilder.XDR_EndAttributeDtType), true) };
        private static readonly XdrAttributeEntry[] S_XDR_Attribute_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaType, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildAttribute_Type)), new XdrAttributeEntry(SchemaNames.Token.SchemaRequired, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildAttribute_Required)), new XdrAttributeEntry(SchemaNames.Token.SchemaDefault, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildAttribute_Default)) };
        private static readonly XdrAttributeEntry[] S_XDR_AttributeDataType_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaDtType, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtType)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtValues, XmlTokenizedType.NMTOKENS, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtValues)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMaxLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtMaxLength)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMinLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtMinLength)) };
        private static readonly XdrAttributeEntry[] S_XDR_AttributeType_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaName, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_Name)), new XdrAttributeEntry(SchemaNames.Token.SchemaRequired, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_Required)), new XdrAttributeEntry(SchemaNames.Token.SchemaDefault, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_Default)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtType, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtType)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtValues, XmlTokenizedType.NMTOKENS, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtValues)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMaxLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtMaxLength)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMinLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildAttributeType_DtMinLength)) };
        private static readonly int[] S_XDR_AttributeType_SubElements = new int[] { 8 };
        private static readonly XdrAttributeEntry[] S_XDR_Element_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaType, XmlTokenizedType.QName, 0x100, new XdrBuildFunction(XdrBuilder.XDR_BuildElement_Type)), new XdrAttributeEntry(SchemaNames.Token.SchemaMinOccurs, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElement_MinOccurs)), new XdrAttributeEntry(SchemaNames.Token.SchemaMaxOccurs, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElement_MaxOccurs)) };
        private static readonly XdrAttributeEntry[] S_XDR_ElementDataType_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaDtType, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtType)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtValues, XmlTokenizedType.NMTOKENS, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtValues)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMaxLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtMaxLength)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMinLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtMinLength)) };
        private static readonly XdrAttributeEntry[] S_XDR_ElementType_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaName, XmlTokenizedType.QName, 0x100, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_Name)), new XdrAttributeEntry(SchemaNames.Token.SchemaContent, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_Content)), new XdrAttributeEntry(SchemaNames.Token.SchemaModel, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_Model)), new XdrAttributeEntry(SchemaNames.Token.SchemaOrder, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_Order)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtType, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtType)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtValues, XmlTokenizedType.NMTOKENS, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtValues)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMaxLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtMaxLength)), new XdrAttributeEntry(SchemaNames.Token.SchemaDtMinLength, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildElementType_DtMinLength)) };
        private static readonly int[] S_XDR_ElementType_SubElements = new int[] { 4, 6, 3, 5, 7 };
        private static readonly XdrAttributeEntry[] S_XDR_Group_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaOrder, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildGroup_Order)), new XdrAttributeEntry(SchemaNames.Token.SchemaMinOccurs, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildGroup_MinOccurs)), new XdrAttributeEntry(SchemaNames.Token.SchemaMaxOccurs, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildGroup_MaxOccurs)) };
        private static readonly int[] S_XDR_Group_SubElements = new int[] { 4, 6 };
        private static readonly XdrAttributeEntry[] S_XDR_Root_Attributes = new XdrAttributeEntry[] { new XdrAttributeEntry(SchemaNames.Token.SchemaName, XmlTokenizedType.CDATA, new XdrBuildFunction(XdrBuilder.XDR_BuildRoot_Name)), new XdrAttributeEntry(SchemaNames.Token.SchemaId, XmlTokenizedType.QName, new XdrBuildFunction(XdrBuilder.XDR_BuildRoot_ID)) };
        private static readonly int[] S_XDR_Root_Element = new int[] { 1 };
        private static readonly int[] S_XDR_Root_SubElements = new int[] { 2, 3 };
        private const int SchemaContentElement = 4;
        private const int SchemaContentEmpty = 1;
        private const int SchemaContentMixed = 3;
        private const int SchemaContentNone = 0;
        private const int SchemaContentText = 2;
        private const int SchemaFlagsNs = 0x100;
        private const int SchemaOrderAll = 4;
        private const int SchemaOrderChoice = 3;
        private const int SchemaOrderMany = 1;
        private const int SchemaOrderNone = 0;
        private const int SchemaOrderSequence = 2;
        private const int StackIncrement = 10;
        private ValidationEventHandler validationEventHandler;
        private const string x_schema = "x-schema:";
        private const int XdrAttribute = 5;
        private const int XdrAttributeDatatype = 8;
        private const int XdrAttributeType = 3;
        private const int XdrElement = 4;
        private const int XdrElementDatatype = 7;
        private const int XdrElementType = 2;
        private const int XdrGroup = 6;
        private const int XdrSchema = 1;
        private System.Xml.XmlResolver xmlResolver;

        internal XdrBuilder(XmlReader reader, XmlNamespaceManager curmgr, SchemaInfo sinfo, string targetNamspace, XmlNameTable nameTable, SchemaNames schemaNames, ValidationEventHandler eventhandler)
        {
            this._SchemaInfo = sinfo;
            this._TargetNamespace = targetNamspace;
            this._reader = reader;
            this._CurNsMgr = curmgr;
            this.validationEventHandler = eventhandler;
            this._StateHistory = new HWStack(10);
            this._ElementDef = new ElementContent();
            this._AttributeDef = new AttributeContent();
            this._GroupStack = new HWStack(10);
            this._GroupDef = new GroupContent();
            this._NameTable = nameTable;
            this._SchemaNames = schemaNames;
            this._CurState = S_SchemaEntries[0];
            this.positionInfo = PositionInfo.GetPositionInfo(this._reader);
            this.xmlResolver = new XmlUrlResolver();
        }

        private void AddOrder()
        {
            switch (this._GroupDef._Order)
            {
                case 1:
                case 3:
                    this._contentValidator.AddChoice();
                    return;

                case 2:
                    this._contentValidator.AddSequence();
                    return;
            }
            throw new XmlException("Xml_UnexpectedToken", "NAME");
        }

        private XmlSchemaDatatype CheckDatatype(string str)
        {
            XmlSchemaDatatype datatype = XmlSchemaDatatype.FromXdrName(str);
            if (datatype == null)
            {
                this.SendValidationEvent("Sch_UnknownDtType", str);
                return datatype;
            }
            if ((datatype.TokenizedType == XmlTokenizedType.ID) && !this._AttributeDef._Global)
            {
                if (this._ElementDef._ElementDecl.IsIdDeclared)
                {
                    this.SendValidationEvent("Sch_IdAttrDeclared", XmlQualifiedName.ToString(this._ElementDef._ElementDecl.Name.Name, this._ElementDef._ElementDecl.Prefix));
                }
                this._ElementDef._ElementDecl.IsIdDeclared = true;
            }
            return datatype;
        }

        private void CheckDefaultAttValue(SchemaAttDef attDef)
        {
            XdrValidator.CheckDefaultValue(attDef.DefaultValueRaw.Trim(), attDef, this._SchemaInfo, this._CurNsMgr, this._NameTable, null, this.validationEventHandler, this._reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition);
        }

        private static void CompareMinMaxLength(uint cMin, uint cMax, XdrBuilder builder)
        {
            if (((cMin != uint.MaxValue) && (cMax != uint.MaxValue)) && (cMin > cMax))
            {
                builder.SendValidationEvent("Sch_DtMinMaxLength");
            }
        }

        internal override void EndChildren()
        {
            if (this._CurState._EndChildFunc != null)
            {
                this._CurState._EndChildFunc(this);
            }
            this.Pop();
        }

        private int GetContent(XmlQualifiedName qname)
        {
            int num = 0;
            if (this._SchemaNames.TokenToQName[11].Equals(qname))
            {
                num = 1;
                this._ElementDef._AllowDataType = false;
                return num;
            }
            if (this._SchemaNames.TokenToQName[12].Equals(qname))
            {
                num = 4;
                this._ElementDef._AllowDataType = false;
                return num;
            }
            if (this._SchemaNames.TokenToQName[10].Equals(qname))
            {
                num = 3;
                this._ElementDef._AllowDataType = false;
                return num;
            }
            if (this._SchemaNames.TokenToQName[13].Equals(qname))
            {
                return 2;
            }
            this.SendValidationEvent("Sch_UnknownContent", qname.Name);
            return num;
        }

        private bool GetModel(XmlQualifiedName qname)
        {
            if (this._SchemaNames.TokenToQName[7].Equals(qname))
            {
                return true;
            }
            if (this._SchemaNames.TokenToQName[8].Equals(qname))
            {
                return false;
            }
            this.SendValidationEvent("Sch_UnknownModel", qname.Name);
            return false;
        }

        private bool GetNextState(XmlQualifiedName qname)
        {
            if (this._CurState._NextStates != null)
            {
                for (int i = 0; i < this._CurState._NextStates.Length; i++)
                {
                    if (this._SchemaNames.TokenToQName[(int) S_SchemaEntries[this._CurState._NextStates[i]]._Name].Equals(qname))
                    {
                        this._NextState = S_SchemaEntries[this._CurState._NextStates[i]];
                        return true;
                    }
                }
            }
            return false;
        }

        private int GetOrder(XmlQualifiedName qname)
        {
            if (this._SchemaNames.TokenToQName[15].Equals(qname))
            {
                return 2;
            }
            if (this._SchemaNames.TokenToQName[0x10].Equals(qname))
            {
                return 3;
            }
            if (this._SchemaNames.TokenToQName[0x11].Equals(qname))
            {
                return 1;
            }
            this.SendValidationEvent("Sch_UnknownOrder", qname.Name);
            return 0;
        }

        private static void HandleMinMax(ParticleContentValidator pContent, uint cMin, uint cMax)
        {
            if (pContent != null)
            {
                if (cMax == uint.MaxValue)
                {
                    if (cMin == 0)
                    {
                        pContent.AddStar();
                    }
                    else
                    {
                        pContent.AddPlus();
                    }
                }
                else if (cMin == 0)
                {
                    pContent.AddQMark();
                }
            }
        }

        internal override bool IsContentParsed()
        {
            return true;
        }

        private bool IsGlobal(int flags)
        {
            return (flags == 0x100);
        }

        private bool IsSkipableAttribute(XmlQualifiedName qname)
        {
            string strA = qname.Namespace;
            if ((((strA.Length == 0) || Ref.Equal(strA, this._SchemaNames.NsXdr)) || Ref.Equal(strA, this._SchemaNames.NsDataType)) && ((!Ref.Equal(strA, this._SchemaNames.NsDataType) || (this._CurState._Name != SchemaNames.Token.XdrDatatype)) || ((!this._SchemaNames.QnDtMax.Equals(qname) && !this._SchemaNames.QnDtMin.Equals(qname)) && (!this._SchemaNames.QnDtMaxExclusive.Equals(qname) && !this._SchemaNames.QnDtMinExclusive.Equals(qname)))))
            {
                return false;
            }
            return true;
        }

        private bool IsSkipableElement(XmlQualifiedName qname)
        {
            string strA = qname.Namespace;
            if (((strA == null) || Ref.Equal(strA, this._SchemaNames.NsXdr)) && (!this._SchemaNames.TokenToQName[0x26].Equals(qname) && !this._SchemaNames.TokenToQName[0x27].Equals(qname)))
            {
                return false;
            }
            return true;
        }

        internal static bool IsXdrSchema(string uri)
        {
            return (((uri.Length >= "x-schema:".Length) && (string.Compare(uri, 0, "x-schema:", 0, "x-schema:".Length, StringComparison.Ordinal) == 0)) && !uri.StartsWith("x-schema:#", StringComparison.Ordinal));
        }

        private static bool IsYes(object obj, XdrBuilder builder)
        {
            XmlQualifiedName name = (XmlQualifiedName) obj;
            if (name.Name == "yes")
            {
                return true;
            }
            if (name.Name != "no")
            {
                builder.SendValidationEvent("Sch_UnknownRequired");
            }
            return false;
        }

        private bool LoadSchema(string uri)
        {
            if (this.xmlResolver != null)
            {
                uri = this._NameTable.Add(uri);
                if (this._SchemaInfo.TargetNamespaces.ContainsKey(uri))
                {
                    return false;
                }
                SchemaInfo sinfo = null;
                Uri baseUri = this.xmlResolver.ResolveUri(null, this._reader.BaseURI);
                XmlReader reader = null;
                try
                {
                    Uri absoluteUri = this.xmlResolver.ResolveUri(baseUri, uri.Substring("x-schema:".Length));
                    Stream input = (Stream) this.xmlResolver.GetEntity(absoluteUri, null, null);
                    reader = new XmlTextReader(absoluteUri.ToString(), input, this._NameTable);
                    sinfo = new SchemaInfo();
                    System.Xml.Schema.Parser parser = new System.Xml.Schema.Parser(SchemaType.XDR, this._NameTable, this._SchemaNames, this.validationEventHandler) {
                        XmlResolver = this.xmlResolver
                    };
                    parser.Parse(reader, uri);
                    sinfo = parser.XdrSchema;
                }
                catch (XmlException exception)
                {
                    this.SendValidationEvent("Sch_CannotLoadSchema", new string[] { uri, exception.Message }, XmlSeverityType.Warning);
                    sinfo = null;
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
                    this._SchemaInfo.Add(sinfo, this.validationEventHandler);
                    return true;
                }
            }
            return false;
        }

        private static void ParseDtMaxLength(ref uint cVal, object obj, XdrBuilder builder)
        {
            if (uint.MaxValue != cVal)
            {
                builder.SendValidationEvent("Sch_DupDtMaxLength");
            }
            if (!ParseInteger((string) obj, ref cVal) || (cVal < 0))
            {
                builder.SendValidationEvent("Sch_DtMaxLengthInvalid", obj.ToString());
            }
        }

        private static void ParseDtMinLength(ref uint cVal, object obj, XdrBuilder builder)
        {
            if (uint.MaxValue != cVal)
            {
                builder.SendValidationEvent("Sch_DupDtMinLength");
            }
            if (!ParseInteger((string) obj, ref cVal) || (cVal < 0))
            {
                builder.SendValidationEvent("Sch_DtMinLengthInvalid", obj.ToString());
            }
        }

        private static bool ParseInteger(string str, ref uint n)
        {
            return uint.TryParse(str, NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite, NumberFormatInfo.InvariantInfo, out n);
        }

        private static uint ParseMaxOccurs(object obj, XdrBuilder builder)
        {
            uint maxValue = uint.MaxValue;
            string str = (string) obj;
            if (!str.Equals("*") && (!ParseInteger(str, ref maxValue) || ((maxValue != uint.MaxValue) && (maxValue != 1))))
            {
                builder.SendValidationEvent("Sch_MaxOccursInvalid");
            }
            return maxValue;
        }

        private static uint ParseMinOccurs(object obj, XdrBuilder builder)
        {
            uint n = 1;
            if (!ParseInteger((string) obj, ref n) || ((n != 0) && (n != 1)))
            {
                builder.SendValidationEvent("Sch_MinOccursInvalid");
            }
            return n;
        }

        private void Pop()
        {
            this._CurState = (XdrEntry) this._StateHistory.Pop();
        }

        private void PopGroupInfo()
        {
            this._GroupDef = (GroupContent) this._GroupStack.Pop();
        }

        internal override void ProcessAttribute(string prefix, string name, string ns, string value)
        {
            XmlQualifiedName name2 = new XmlQualifiedName(name, XmlSchemaDatatype.XdrCanonizeUri(ns, this._NameTable, this._SchemaNames));
            for (int i = 0; i < this._CurState._Attributes.Length; i++)
            {
                XdrAttributeEntry entry = this._CurState._Attributes[i];
                if (this._SchemaNames.TokenToQName[(int) entry._Attribute].Equals(name2))
                {
                    XdrBuildFunction function = entry._BuildFunc;
                    if (entry._Datatype.TokenizedType == XmlTokenizedType.QName)
                    {
                        string str;
                        XmlQualifiedName name3 = XmlQualifiedName.Parse(value, this._CurNsMgr, out str);
                        name3.Atomize(this._NameTable);
                        if (str.Length != 0)
                        {
                            if (entry._Attribute != SchemaNames.Token.SchemaType)
                            {
                                throw new XmlException("Xml_UnexpectedToken", "NAME");
                            }
                        }
                        else if (this.IsGlobal(entry._SchemaFlags))
                        {
                            name3 = new XmlQualifiedName(name3.Name, this._TargetNamespace);
                        }
                        else
                        {
                            name3 = new XmlQualifiedName(name3.Name);
                        }
                        function(this, name3, str);
                        return;
                    }
                    function(this, entry._Datatype.ParseValue(value, this._NameTable, this._CurNsMgr), string.Empty);
                    return;
                }
            }
            if ((ns == this._SchemaNames.NsXmlNs) && IsXdrSchema(value))
            {
                this.LoadSchema(value);
            }
            else if (!this.IsSkipableAttribute(name2))
            {
                this.SendValidationEvent("Sch_UnsupportedAttribute", XmlQualifiedName.ToString(name2.Name, prefix));
            }
        }

        internal override void ProcessCData(string value)
        {
            if (this._CurState._AllowText)
            {
                this._Text = value;
            }
            else
            {
                this.SendValidationEvent("Sch_TextNotAllowed", value);
            }
        }

        internal override bool ProcessElement(string prefix, string name, string ns)
        {
            XmlQualifiedName qname = new XmlQualifiedName(name, XmlSchemaDatatype.XdrCanonizeUri(ns, this._NameTable, this._SchemaNames));
            if (this.GetNextState(qname))
            {
                this.Push();
                if (this._CurState._InitFunc != null)
                {
                    this._CurState._InitFunc(this, qname);
                }
                return true;
            }
            if (!this.IsSkipableElement(qname))
            {
                this.SendValidationEvent("Sch_UnsupportedElement", XmlQualifiedName.ToString(name, prefix));
            }
            return false;
        }

        internal override void ProcessMarkup(XmlNode[] markup)
        {
            throw new InvalidOperationException(Res.GetString("Xml_InvalidOperation"));
        }

        private void Push()
        {
            this._StateHistory.Push();
            this._StateHistory[this._StateHistory.Length - 1] = this._CurState;
            this._CurState = this._NextState;
        }

        private void PushGroupInfo()
        {
            this._GroupStack.Push();
            this._GroupStack[this._GroupStack.Length - 1] = GroupContent.Copy(this._GroupDef);
        }

        private void SendValidationEvent(string code)
        {
            this.SendValidationEvent(code, string.Empty);
        }

        private void SendValidationEvent(string code, string msg)
        {
            this.SendValidationEvent(new XmlSchemaException(code, msg, this._reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition), XmlSeverityType.Error);
        }

        private void SendValidationEvent(XmlSchemaException e, XmlSeverityType severity)
        {
            this._SchemaInfo.ErrorCount++;
            if (this.validationEventHandler != null)
            {
                this.validationEventHandler(this, new ValidationEventArgs(e, severity));
            }
            else if (severity == XmlSeverityType.Error)
            {
                throw e;
            }
        }

        private void SendValidationEvent(string code, string[] args, XmlSeverityType severity)
        {
            this.SendValidationEvent(new XmlSchemaException(code, args, this._reader.BaseURI, this.positionInfo.LineNumber, this.positionInfo.LinePosition), severity);
        }

        private void SetAttributePresence(SchemaAttDef pAttdef, bool fRequired)
        {
            if (SchemaDeclBase.Use.Fixed != pAttdef.Presence)
            {
                if (fRequired || (SchemaDeclBase.Use.Required == pAttdef.Presence))
                {
                    if (pAttdef.DefaultValueTyped != null)
                    {
                        pAttdef.Presence = SchemaDeclBase.Use.Fixed;
                    }
                    else
                    {
                        pAttdef.Presence = SchemaDeclBase.Use.Required;
                    }
                }
                else if (pAttdef.DefaultValueTyped != null)
                {
                    pAttdef.Presence = SchemaDeclBase.Use.Default;
                }
                else
                {
                    pAttdef.Presence = SchemaDeclBase.Use.Implied;
                }
            }
        }

        internal override void StartChildren()
        {
            if (this._CurState._BeginChildFunc != null)
            {
                this._CurState._BeginChildFunc(this);
            }
        }

        private static void XDR_BeginAttribute(XdrBuilder builder)
        {
            if (builder._BaseDecl._TypeName.IsEmpty)
            {
                builder.SendValidationEvent("Sch_MissAttribute");
            }
            SchemaAttDef pAttdef = null;
            XmlQualifiedName name = builder._BaseDecl._TypeName;
            string ns = builder._BaseDecl._Prefix;
            if (builder._ElementDef._AttDefList != null)
            {
                pAttdef = (SchemaAttDef) builder._ElementDef._AttDefList[name];
            }
            if (pAttdef == null)
            {
                SchemaAttDef def2;
                XmlQualifiedName key = name;
                if (ns.Length == 0)
                {
                    key = new XmlQualifiedName(name.Name, builder._TargetNamespace);
                }
                if (builder._SchemaInfo.AttributeDecls.TryGetValue(key, out def2))
                {
                    pAttdef = def2.Clone();
                    pAttdef.Name = name;
                }
                else if (ns.Length != 0)
                {
                    builder.SendValidationEvent("Sch_UndeclaredAttribute", XmlQualifiedName.ToString(name.Name, ns));
                }
            }
            if (pAttdef != null)
            {
                builder.XDR_CheckAttributeDefault(builder._BaseDecl, pAttdef);
            }
            else
            {
                pAttdef = new SchemaAttDef(name, ns);
                DeclBaseInfo info = new DeclBaseInfo {
                    _Checking = true,
                    _Attdef = pAttdef,
                    _TypeName = builder._BaseDecl._TypeName,
                    _ElementDecl = builder._ElementDef._ElementDecl,
                    _MinOccurs = builder._BaseDecl._MinOccurs,
                    _Default = builder._BaseDecl._Default,
                    _Next = builder._UndefinedAttributeTypes
                };
                builder._UndefinedAttributeTypes = info;
            }
            builder._ElementDef._ElementDecl.AddAttDef(pAttdef);
        }

        private static void XDR_BeginAttributeType(XdrBuilder builder)
        {
            if (builder._AttributeDef._Name.IsEmpty)
            {
                builder.SendValidationEvent("Sch_MissAttribute");
            }
        }

        private static void XDR_BeginElementType(XdrBuilder builder)
        {
            string code = null;
            string msg = null;
            if (builder._ElementDef._ElementDecl.Name.IsEmpty)
            {
                code = "Sch_MissAttribute";
                msg = "name";
            }
            else
            {
                if (builder._ElementDef._HasDataType)
                {
                    if (!builder._ElementDef._AllowDataType)
                    {
                        code = "Sch_DataTypeTextOnly";
                        goto Label_01F4;
                    }
                    builder._ElementDef._ContentAttr = 2;
                }
                else if (builder._ElementDef._ContentAttr == 0)
                {
                    switch (builder._ElementDef._OrderAttr)
                    {
                        case 0:
                            builder._ElementDef._ContentAttr = 3;
                            builder._ElementDef._OrderAttr = 1;
                            break;

                        case 1:
                            builder._ElementDef._ContentAttr = 3;
                            break;

                        case 2:
                            builder._ElementDef._ContentAttr = 4;
                            break;

                        case 3:
                            builder._ElementDef._ContentAttr = 4;
                            break;
                    }
                }
                bool isOpen = builder._contentValidator.IsOpen;
                ElementContent content = builder._ElementDef;
                switch (builder._ElementDef._ContentAttr)
                {
                    case 1:
                        builder._ElementDef._ElementDecl.ContentValidator = ContentValidator.Empty;
                        builder._contentValidator = null;
                        break;

                    case 2:
                        builder._ElementDef._ElementDecl.ContentValidator = ContentValidator.TextOnly;
                        builder._GroupDef._Order = 1;
                        builder._contentValidator = null;
                        break;

                    case 3:
                        if ((content._OrderAttr != 0) && (content._OrderAttr != 1))
                        {
                            code = "Sch_MixedMany";
                            goto Label_01F4;
                        }
                        builder._GroupDef._Order = 1;
                        content._MasterGroupRequired = true;
                        builder._contentValidator.IsOpen = isOpen;
                        break;

                    case 4:
                        builder._contentValidator = new ParticleContentValidator(XmlSchemaContentType.ElementOnly);
                        if (content._OrderAttr == 0)
                        {
                            builder._GroupDef._Order = 2;
                        }
                        content._MasterGroupRequired = true;
                        builder._contentValidator.IsOpen = isOpen;
                        break;
                }
                if ((content._ContentAttr == 3) || (content._ContentAttr == 4))
                {
                    builder._contentValidator.Start();
                    builder._contentValidator.OpenGroup();
                }
            }
        Label_01F4:
            if (code != null)
            {
                builder.SendValidationEvent(code, msg);
            }
        }

        private static void XDR_BeginRoot(XdrBuilder builder)
        {
            if (builder._TargetNamespace == null)
            {
                if (builder._XdrName != null)
                {
                    builder._TargetNamespace = builder._NameTable.Add("x-schema:#" + builder._XdrName);
                }
                else
                {
                    builder._TargetNamespace = string.Empty;
                }
            }
            builder._SchemaInfo.TargetNamespaces.Add(builder._TargetNamespace, true);
        }

        private static void XDR_BuildAttribute_Default(XdrBuilder builder, object obj, string prefix)
        {
            builder._BaseDecl._Default = obj;
        }

        private static void XDR_BuildAttribute_Required(XdrBuilder builder, object obj, string prefix)
        {
            if (IsYes(obj, builder))
            {
                builder._BaseDecl._MinOccurs = 1;
            }
        }

        private static void XDR_BuildAttribute_Type(XdrBuilder builder, object obj, string prefix)
        {
            builder._BaseDecl._TypeName = (XmlQualifiedName) obj;
            builder._BaseDecl._Prefix = prefix;
        }

        private static void XDR_BuildAttributeType_Default(XdrBuilder builder, object obj, string prefix)
        {
            builder._AttributeDef._Default = obj;
        }

        private static void XDR_BuildAttributeType_DtMaxLength(XdrBuilder builder, object obj, string prefix)
        {
            ParseDtMaxLength(ref builder._AttributeDef._MaxLength, obj, builder);
        }

        private static void XDR_BuildAttributeType_DtMinLength(XdrBuilder builder, object obj, string prefix)
        {
            ParseDtMinLength(ref builder._AttributeDef._MinLength, obj, builder);
        }

        private static void XDR_BuildAttributeType_DtType(XdrBuilder builder, object obj, string prefix)
        {
            XmlQualifiedName name = (XmlQualifiedName) obj;
            builder._AttributeDef._HasDataType = true;
            builder._AttributeDef._AttDef.Datatype = builder.CheckDatatype(name.Name);
        }

        private static void XDR_BuildAttributeType_DtValues(XdrBuilder builder, object obj, string prefix)
        {
            builder._AttributeDef._EnumerationRequired = true;
            builder._AttributeDef._AttDef.Values = new List<string>((string[]) obj);
        }

        private static void XDR_BuildAttributeType_Name(XdrBuilder builder, object obj, string prefix)
        {
            XmlQualifiedName key = (XmlQualifiedName) obj;
            builder._AttributeDef._Name = key;
            builder._AttributeDef._Prefix = prefix;
            builder._AttributeDef._AttDef.Name = key;
            if (builder._ElementDef._ElementDecl != null)
            {
                if (builder._ElementDef._AttDefList[key] == null)
                {
                    builder._ElementDef._AttDefList.Add(key, builder._AttributeDef._AttDef);
                }
                else
                {
                    builder.SendValidationEvent("Sch_DupAttribute", XmlQualifiedName.ToString(key.Name, prefix));
                }
            }
            else
            {
                key = new XmlQualifiedName(key.Name, builder._TargetNamespace);
                builder._AttributeDef._AttDef.Name = key;
                if (!builder._SchemaInfo.AttributeDecls.ContainsKey(key))
                {
                    builder._SchemaInfo.AttributeDecls.Add(key, builder._AttributeDef._AttDef);
                }
                else
                {
                    builder.SendValidationEvent("Sch_DupAttribute", XmlQualifiedName.ToString(key.Name, prefix));
                }
            }
        }

        private static void XDR_BuildAttributeType_Required(XdrBuilder builder, object obj, string prefix)
        {
            builder._AttributeDef._Required = IsYes(obj, builder);
        }

        private static void XDR_BuildElement_MaxOccurs(XdrBuilder builder, object obj, string prefix)
        {
            builder._ElementDef._MaxVal = ParseMaxOccurs(obj, builder);
        }

        private static void XDR_BuildElement_MinOccurs(XdrBuilder builder, object obj, string prefix)
        {
            builder._ElementDef._MinVal = ParseMinOccurs(obj, builder);
        }

        private static void XDR_BuildElement_Type(XdrBuilder builder, object obj, string prefix)
        {
            XmlQualifiedName key = (XmlQualifiedName) obj;
            if (!builder._SchemaInfo.ElementDecls.ContainsKey(key) && (((SchemaElementDecl) builder._UndeclaredElements[key]) == null))
            {
                SchemaElementDecl decl = new SchemaElementDecl(key, prefix);
                builder._UndeclaredElements.Add(key, decl);
            }
            builder._ElementDef._HasType = true;
            if (builder._ElementDef._ExistTerminal)
            {
                builder.AddOrder();
            }
            else
            {
                builder._ElementDef._ExistTerminal = true;
            }
            builder._contentValidator.AddName(key, null);
        }

        private static void XDR_BuildElementType_Content(XdrBuilder builder, object obj, string prefix)
        {
            builder._ElementDef._ContentAttr = builder.GetContent((XmlQualifiedName) obj);
        }

        private static void XDR_BuildElementType_DtMaxLength(XdrBuilder builder, object obj, string prefix)
        {
            ParseDtMaxLength(ref builder._ElementDef._MaxLength, obj, builder);
        }

        private static void XDR_BuildElementType_DtMinLength(XdrBuilder builder, object obj, string prefix)
        {
            ParseDtMinLength(ref builder._ElementDef._MinLength, obj, builder);
        }

        private static void XDR_BuildElementType_DtType(XdrBuilder builder, object obj, string prefix)
        {
            builder._ElementDef._HasDataType = true;
            string name = ((string) obj).Trim();
            if (name.Length == 0)
            {
                builder.SendValidationEvent("Sch_MissDtvalue");
            }
            else
            {
                XmlSchemaDatatype datatype = XmlSchemaDatatype.FromXdrName(name);
                if (datatype == null)
                {
                    builder.SendValidationEvent("Sch_UnknownDtType", name);
                }
                builder._ElementDef._ElementDecl.Datatype = datatype;
            }
        }

        private static void XDR_BuildElementType_DtValues(XdrBuilder builder, object obj, string prefix)
        {
            builder._ElementDef._EnumerationRequired = true;
            builder._ElementDef._ElementDecl.Values = new List<string>((string[]) obj);
        }

        private static void XDR_BuildElementType_Model(XdrBuilder builder, object obj, string prefix)
        {
            builder._contentValidator.IsOpen = builder.GetModel((XmlQualifiedName) obj);
        }

        private static void XDR_BuildElementType_Name(XdrBuilder builder, object obj, string prefix)
        {
            XmlQualifiedName key = (XmlQualifiedName) obj;
            if (builder._SchemaInfo.ElementDecls.ContainsKey(key))
            {
                builder.SendValidationEvent("Sch_DupElementDecl", XmlQualifiedName.ToString(key.Name, prefix));
            }
            builder._ElementDef._ElementDecl.Name = key;
            builder._ElementDef._ElementDecl.Prefix = prefix;
            builder._SchemaInfo.ElementDecls.Add(key, builder._ElementDef._ElementDecl);
            if (builder._UndeclaredElements[key] != null)
            {
                builder._UndeclaredElements.Remove(key);
            }
        }

        private static void XDR_BuildElementType_Order(XdrBuilder builder, object obj, string prefix)
        {
            builder._ElementDef._OrderAttr = builder._GroupDef._Order = builder.GetOrder((XmlQualifiedName) obj);
        }

        private static void XDR_BuildGroup_MaxOccurs(XdrBuilder builder, object obj, string prefix)
        {
            builder._GroupDef._MaxVal = ParseMaxOccurs(obj, builder);
            builder._GroupDef._HasMaxAttr = true;
        }

        private static void XDR_BuildGroup_MinOccurs(XdrBuilder builder, object obj, string prefix)
        {
            builder._GroupDef._MinVal = ParseMinOccurs(obj, builder);
            builder._GroupDef._HasMinAttr = true;
        }

        private static void XDR_BuildGroup_Order(XdrBuilder builder, object obj, string prefix)
        {
            builder._GroupDef._Order = builder.GetOrder((XmlQualifiedName) obj);
            if ((builder._ElementDef._ContentAttr == 3) && (builder._GroupDef._Order != 1))
            {
                builder.SendValidationEvent("Sch_MixedMany");
            }
        }

        private static void XDR_BuildRoot_ID(XdrBuilder builder, object obj, string prefix)
        {
        }

        private static void XDR_BuildRoot_Name(XdrBuilder builder, object obj, string prefix)
        {
            builder._XdrName = (string) obj;
            builder._XdrPrefix = prefix;
        }

        private void XDR_CheckAttributeDefault(DeclBaseInfo decl, SchemaAttDef pAttdef)
        {
            if (((decl._Default != null) || (pAttdef.DefaultValueTyped != null)) && (decl._Default != null))
            {
                pAttdef.DefaultValueRaw = pAttdef.DefaultValueExpanded = (string) decl._Default;
                this.CheckDefaultAttValue(pAttdef);
            }
            this.SetAttributePresence(pAttdef, 1 == decl._MinOccurs);
        }

        private static void XDR_EndAttribute(XdrBuilder builder)
        {
            builder._BaseDecl.Reset();
        }

        private static void XDR_EndAttributeDtType(XdrBuilder builder)
        {
            string code = null;
            if (!builder._AttributeDef._HasDataType)
            {
                code = "Sch_MissAttribute";
            }
            else if (builder._AttributeDef._AttDef.Datatype != null)
            {
                XmlTokenizedType tokenizedType = builder._AttributeDef._AttDef.Datatype.TokenizedType;
                if ((tokenizedType == XmlTokenizedType.ENUMERATION) && !builder._AttributeDef._EnumerationRequired)
                {
                    code = "Sch_MissDtvaluesAttribute";
                }
                else if ((tokenizedType != XmlTokenizedType.ENUMERATION) && builder._AttributeDef._EnumerationRequired)
                {
                    code = "Sch_RequireEnumeration";
                }
            }
            if (code != null)
            {
                builder.SendValidationEvent(code);
            }
        }

        private static void XDR_EndAttributeType(XdrBuilder builder)
        {
            string code = null;
            if (builder._AttributeDef._HasDataType && (builder._AttributeDef._AttDef.Datatype != null))
            {
                XmlTokenizedType tokenizedType = builder._AttributeDef._AttDef.Datatype.TokenizedType;
                if ((tokenizedType == XmlTokenizedType.ENUMERATION) && !builder._AttributeDef._EnumerationRequired)
                {
                    code = "Sch_MissDtvaluesAttribute";
                }
                else if ((tokenizedType != XmlTokenizedType.ENUMERATION) && builder._AttributeDef._EnumerationRequired)
                {
                    code = "Sch_RequireEnumeration";
                }
                else
                {
                    if ((builder._AttributeDef._Default == null) || (tokenizedType != XmlTokenizedType.ID))
                    {
                        goto Label_00A3;
                    }
                    code = "Sch_DefaultIdValue";
                }
                goto Label_0164;
            }
            builder._AttributeDef._AttDef.Datatype = XmlSchemaDatatype.FromXmlTokenizedType(XmlTokenizedType.CDATA);
        Label_00A3:
            CompareMinMaxLength(builder._AttributeDef._MinLength, builder._AttributeDef._MaxLength, builder);
            builder._AttributeDef._AttDef.MaxLength = builder._AttributeDef._MaxLength;
            builder._AttributeDef._AttDef.MinLength = builder._AttributeDef._MinLength;
            if (builder._AttributeDef._Default != null)
            {
                builder._AttributeDef._AttDef.DefaultValueRaw = builder._AttributeDef._AttDef.DefaultValueExpanded = (string) builder._AttributeDef._Default;
                builder.CheckDefaultAttValue(builder._AttributeDef._AttDef);
            }
            builder.SetAttributePresence(builder._AttributeDef._AttDef, builder._AttributeDef._Required);
        Label_0164:
            if (code != null)
            {
                builder.SendValidationEvent(code);
            }
        }

        private static void XDR_EndElement(XdrBuilder builder)
        {
            if (builder._ElementDef._HasType)
            {
                HandleMinMax(builder._contentValidator, builder._ElementDef._MinVal, builder._ElementDef._MaxVal);
            }
            else
            {
                builder.SendValidationEvent("Sch_MissAttribute");
            }
        }

        private static void XDR_EndElementDtType(XdrBuilder builder)
        {
            if (!builder._ElementDef._HasDataType)
            {
                builder.SendValidationEvent("Sch_MissAttribute");
            }
            builder._ElementDef._ElementDecl.ContentValidator = ContentValidator.TextOnly;
            builder._ElementDef._ContentAttr = 2;
            builder._ElementDef._MasterGroupRequired = false;
            builder._contentValidator = null;
        }

        private static void XDR_EndElementType(XdrBuilder builder)
        {
            SchemaElementDecl decl = builder._ElementDef._ElementDecl;
            if ((builder._UndefinedAttributeTypes != null) && (builder._ElementDef._AttDefList != null))
            {
                DeclBaseInfo info = builder._UndefinedAttributeTypes;
                DeclBaseInfo info2 = info;
                while (info != null)
                {
                    SchemaAttDef pAttdef = null;
                    if (info._ElementDecl == decl)
                    {
                        XmlQualifiedName name = info._TypeName;
                        pAttdef = (SchemaAttDef) builder._ElementDef._AttDefList[name];
                        if (pAttdef != null)
                        {
                            info._Attdef = pAttdef.Clone();
                            info._Attdef.Name = name;
                            builder.XDR_CheckAttributeDefault(info, pAttdef);
                            if (info == builder._UndefinedAttributeTypes)
                            {
                                info = builder._UndefinedAttributeTypes = info._Next;
                                info2 = info;
                            }
                            else
                            {
                                info2._Next = info._Next;
                                info = info2._Next;
                            }
                        }
                    }
                    if (pAttdef == null)
                    {
                        if (info != builder._UndefinedAttributeTypes)
                        {
                            info2 = info2._Next;
                        }
                        info = info._Next;
                    }
                }
            }
            if (builder._ElementDef._MasterGroupRequired)
            {
                builder._contentValidator.CloseGroup();
                if (!builder._ElementDef._ExistTerminal)
                {
                    if (builder._contentValidator.IsOpen)
                    {
                        builder._ElementDef._ElementDecl.ContentValidator = ContentValidator.Any;
                        builder._contentValidator = null;
                    }
                    else if (builder._ElementDef._ContentAttr != 3)
                    {
                        builder.SendValidationEvent("Sch_ElementMissing");
                    }
                }
                else if (builder._GroupDef._Order == 1)
                {
                    builder._contentValidator.AddStar();
                }
            }
            if (decl.Datatype != null)
            {
                XmlTokenizedType tokenizedType = decl.Datatype.TokenizedType;
                if ((tokenizedType == XmlTokenizedType.ENUMERATION) && !builder._ElementDef._EnumerationRequired)
                {
                    builder.SendValidationEvent("Sch_MissDtvaluesAttribute");
                }
                if ((tokenizedType != XmlTokenizedType.ENUMERATION) && builder._ElementDef._EnumerationRequired)
                {
                    builder.SendValidationEvent("Sch_RequireEnumeration");
                }
            }
            CompareMinMaxLength(builder._ElementDef._MinLength, builder._ElementDef._MaxLength, builder);
            decl.MaxLength = builder._ElementDef._MaxLength;
            decl.MinLength = builder._ElementDef._MinLength;
            if (builder._contentValidator != null)
            {
                builder._ElementDef._ElementDecl.ContentValidator = builder._contentValidator.Finish(true);
                builder._contentValidator = null;
            }
            builder._ElementDef._ElementDecl = null;
            builder._ElementDef._AttDefList = null;
        }

        private static void XDR_EndGroup(XdrBuilder builder)
        {
            if (!builder._ElementDef._ExistTerminal)
            {
                builder.SendValidationEvent("Sch_ElementMissing");
            }
            builder._contentValidator.CloseGroup();
            if (builder._GroupDef._Order == 1)
            {
                builder._contentValidator.AddStar();
            }
            if (((1 == builder._GroupDef._Order) && builder._GroupDef._HasMaxAttr) && (builder._GroupDef._MaxVal != uint.MaxValue))
            {
                builder.SendValidationEvent("Sch_ManyMaxOccurs");
            }
            HandleMinMax(builder._contentValidator, builder._GroupDef._MinVal, builder._GroupDef._MaxVal);
            builder.PopGroupInfo();
        }

        private static void XDR_EndRoot(XdrBuilder builder)
        {
            while (builder._UndefinedAttributeTypes != null)
            {
                SchemaAttDef def;
                XmlQualifiedName key = builder._UndefinedAttributeTypes._TypeName;
                if (key.Namespace.Length == 0)
                {
                    key = new XmlQualifiedName(key.Name, builder._TargetNamespace);
                }
                if (builder._SchemaInfo.AttributeDecls.TryGetValue(key, out def))
                {
                    builder._UndefinedAttributeTypes._Attdef = def.Clone();
                    builder._UndefinedAttributeTypes._Attdef.Name = key;
                    builder.XDR_CheckAttributeDefault(builder._UndefinedAttributeTypes, builder._UndefinedAttributeTypes._Attdef);
                }
                else
                {
                    builder.SendValidationEvent("Sch_UndeclaredAttribute", key.Name);
                }
                builder._UndefinedAttributeTypes = builder._UndefinedAttributeTypes._Next;
            }
            foreach (SchemaElementDecl decl in builder._UndeclaredElements.Values)
            {
                builder.SendValidationEvent("Sch_UndeclaredElement", XmlQualifiedName.ToString(decl.Name.Name, decl.Prefix));
            }
        }

        private static void XDR_InitAttribute(XdrBuilder builder, object obj)
        {
            if (builder._BaseDecl == null)
            {
                builder._BaseDecl = new DeclBaseInfo();
            }
            builder._BaseDecl._MinOccurs = 0;
        }

        private static void XDR_InitAttributeDtType(XdrBuilder builder, object obj)
        {
            if (builder._AttributeDef._HasDataType)
            {
                builder.SendValidationEvent("Sch_DupDtType");
            }
        }

        private static void XDR_InitAttributeType(XdrBuilder builder, object obj)
        {
            AttributeContent content = builder._AttributeDef;
            content._AttDef = new SchemaAttDef(XmlQualifiedName.Empty, null);
            content._Required = false;
            content._Prefix = null;
            content._Default = null;
            content._MinVal = 0;
            content._MaxVal = 1;
            content._EnumerationRequired = false;
            content._HasDataType = false;
            content._Global = builder._StateHistory.Length == 2;
            content._MaxLength = uint.MaxValue;
            content._MinLength = uint.MaxValue;
        }

        private static void XDR_InitElement(XdrBuilder builder, object obj)
        {
            if ((builder._ElementDef._HasDataType || (builder._ElementDef._ContentAttr == 1)) || (builder._ElementDef._ContentAttr == 2))
            {
                builder.SendValidationEvent("Sch_ElementNotAllowed");
            }
            builder._ElementDef._AllowDataType = false;
            builder._ElementDef._HasType = false;
            builder._ElementDef._MinVal = 1;
            builder._ElementDef._MaxVal = 1;
        }

        private static void XDR_InitElementDtType(XdrBuilder builder, object obj)
        {
            if (builder._ElementDef._HasDataType)
            {
                builder.SendValidationEvent("Sch_DupDtType");
            }
            if (!builder._ElementDef._AllowDataType)
            {
                builder.SendValidationEvent("Sch_DataTypeTextOnly");
            }
        }

        private static void XDR_InitElementType(XdrBuilder builder, object obj)
        {
            builder._ElementDef._ElementDecl = new SchemaElementDecl();
            builder._contentValidator = new ParticleContentValidator(XmlSchemaContentType.Mixed);
            builder._contentValidator.IsOpen = true;
            builder._ElementDef._ContentAttr = 0;
            builder._ElementDef._OrderAttr = 0;
            builder._ElementDef._MasterGroupRequired = false;
            builder._ElementDef._ExistTerminal = false;
            builder._ElementDef._AllowDataType = true;
            builder._ElementDef._HasDataType = false;
            builder._ElementDef._EnumerationRequired = false;
            builder._ElementDef._AttDefList = new Hashtable();
            builder._ElementDef._MaxLength = uint.MaxValue;
            builder._ElementDef._MinLength = uint.MaxValue;
        }

        private static void XDR_InitGroup(XdrBuilder builder, object obj)
        {
            if ((builder._ElementDef._ContentAttr == 1) || (builder._ElementDef._ContentAttr == 2))
            {
                builder.SendValidationEvent("Sch_GroupDisabled");
            }
            builder.PushGroupInfo();
            builder._GroupDef._MinVal = 1;
            builder._GroupDef._MaxVal = 1;
            builder._GroupDef._HasMaxAttr = false;
            builder._GroupDef._HasMinAttr = false;
            if (builder._ElementDef._ExistTerminal)
            {
                builder.AddOrder();
            }
            builder._ElementDef._ExistTerminal = false;
            builder._contentValidator.OpenGroup();
        }

        private static void XDR_InitRoot(XdrBuilder builder, object obj)
        {
            builder._SchemaInfo.SchemaType = SchemaType.XDR;
            builder._ElementDef._ElementDecl = null;
            builder._ElementDef._AttDefList = null;
            builder._AttributeDef._AttDef = null;
        }

        internal System.Xml.XmlResolver XmlResolver
        {
            set
            {
                this.xmlResolver = value;
            }
        }

        private sealed class AttributeContent
        {
            internal SchemaAttDef _AttDef;
            internal object _Default;
            internal bool _EnumerationRequired;
            internal bool _Global;
            internal bool _HasDataType;
            internal uint _MaxLength;
            internal uint _MaxVal;
            internal uint _MinLength;
            internal uint _MinVal;
            internal XmlQualifiedName _Name;
            internal string _Prefix;
            internal bool _Required;
        }

        private sealed class DeclBaseInfo
        {
            internal SchemaAttDef _Attdef;
            internal bool _Checking;
            internal object _Default;
            internal SchemaElementDecl _ElementDecl;
            internal uint _MaxOccurs;
            internal uint _MinOccurs;
            internal XmlQualifiedName _Name;
            internal XdrBuilder.DeclBaseInfo _Next;
            internal string _Prefix;
            internal object _Revises;
            internal XmlQualifiedName _TypeName;
            internal string _TypePrefix;

            internal DeclBaseInfo()
            {
                this.Reset();
            }

            internal void Reset()
            {
                this._Name = XmlQualifiedName.Empty;
                this._Prefix = null;
                this._TypeName = XmlQualifiedName.Empty;
                this._TypePrefix = null;
                this._Default = null;
                this._Revises = null;
                this._MaxOccurs = 1;
                this._MinOccurs = 1;
                this._Checking = false;
                this._ElementDecl = null;
                this._Next = null;
                this._Attdef = null;
            }
        }

        private sealed class ElementContent
        {
            internal bool _AllowDataType;
            internal Hashtable _AttDefList;
            internal int _ContentAttr;
            internal SchemaElementDecl _ElementDecl;
            internal bool _EnumerationRequired;
            internal bool _ExistTerminal;
            internal bool _HasDataType;
            internal bool _HasType;
            internal bool _MasterGroupRequired;
            internal uint _MaxLength;
            internal uint _MaxVal;
            internal uint _MinLength;
            internal uint _MinVal;
            internal int _OrderAttr;
        }

        private sealed class GroupContent
        {
            internal bool _HasMaxAttr;
            internal bool _HasMinAttr;
            internal uint _MaxVal;
            internal uint _MinVal;
            internal int _Order;

            internal static XdrBuilder.GroupContent Copy(XdrBuilder.GroupContent other)
            {
                XdrBuilder.GroupContent to = new XdrBuilder.GroupContent();
                Copy(other, to);
                return to;
            }

            internal static void Copy(XdrBuilder.GroupContent from, XdrBuilder.GroupContent to)
            {
                to._MinVal = from._MinVal;
                to._MaxVal = from._MaxVal;
                to._Order = from._Order;
            }
        }

        private sealed class XdrAttributeEntry
        {
            internal SchemaNames.Token _Attribute;
            internal XdrBuilder.XdrBuildFunction _BuildFunc;
            internal XmlSchemaDatatype _Datatype;
            internal int _SchemaFlags;

            internal XdrAttributeEntry(SchemaNames.Token a, XmlTokenizedType ttype, XdrBuilder.XdrBuildFunction build)
            {
                this._Attribute = a;
                this._Datatype = XmlSchemaDatatype.FromXmlTokenizedType(ttype);
                this._SchemaFlags = 0;
                this._BuildFunc = build;
            }

            internal XdrAttributeEntry(SchemaNames.Token a, XmlTokenizedType ttype, int schemaFlags, XdrBuilder.XdrBuildFunction build)
            {
                this._Attribute = a;
                this._Datatype = XmlSchemaDatatype.FromXmlTokenizedType(ttype);
                this._SchemaFlags = schemaFlags;
                this._BuildFunc = build;
            }
        }

        private delegate void XdrBeginChildFunction(XdrBuilder builder);

        private delegate void XdrBuildFunction(XdrBuilder builder, object obj, string prefix);

        private delegate void XdrEndChildFunction(XdrBuilder builder);

        private sealed class XdrEntry
        {
            internal bool _AllowText;
            internal XdrBuilder.XdrAttributeEntry[] _Attributes;
            internal XdrBuilder.XdrBeginChildFunction _BeginChildFunc;
            internal XdrBuilder.XdrEndChildFunction _EndChildFunc;
            internal XdrBuilder.XdrInitFunction _InitFunc;
            internal SchemaNames.Token _Name;
            internal int[] _NextStates;

            internal XdrEntry(SchemaNames.Token n, int[] states, XdrBuilder.XdrAttributeEntry[] attributes, XdrBuilder.XdrInitFunction init, XdrBuilder.XdrBeginChildFunction begin, XdrBuilder.XdrEndChildFunction end, bool fText)
            {
                this._Name = n;
                this._NextStates = states;
                this._Attributes = attributes;
                this._InitFunc = init;
                this._BeginChildFunc = begin;
                this._EndChildFunc = end;
                this._AllowText = fText;
            }
        }

        private delegate void XdrInitFunction(XdrBuilder builder, object obj);
    }
}

