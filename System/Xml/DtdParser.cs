namespace System.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml.Schema;

    internal class DtdParser : IDtdParser
    {
        private char[] chars;
        private int charsUsed;
        private int colonPos;
        private int condSectionDepth;
        private int[] condSectionEntityIds;
        private const int CondSectionEntityIdsInitialSize = 2;
        private int curPos;
        private int currentEntityId;
        private string documentBaseUri = string.Empty;
        private string externalDtdBaseUri = string.Empty;
        private int externalEntitiesDepth;
        private bool freeFloatingDtd;
        private bool hasFreeFloatingInternalSubset;
        private StringBuilder internalSubsetValueSb;
        private LineInfo literalLineInfo = new LineInfo(0, 0);
        private char literalQuoteChar = '"';
        private XmlNameTable nameTable;
        private ScanningFunction nextScaningFunction;
        private bool normalize = true;
        private string publicId = string.Empty;
        private IDtdParserAdapter readerAdapter;
        private IDtdParserAdapterWithValidation readerAdapterWithValidation;
        private ScanningFunction savedScanningFunction;
        private ScanningFunction scanningFunction;
        private SchemaInfo schemaInfo;
        private StringBuilder stringBuilder;
        private bool supportNamespaces = true;
        private string systemId = string.Empty;
        private int tokenStartPos;
        private Dictionary<string, UndeclaredNotation> undeclaredNotations;
        private bool v1Compat;
        private bool validate;
        private bool whitespaceSeen;
        private XmlCharType xmlCharType = XmlCharType.Instance;

        private DtdParser()
        {
        }

        private void AddUndeclaredNotation(string notationName)
        {
            UndeclaredNotation notation2;
            if (this.undeclaredNotations == null)
            {
                this.undeclaredNotations = new Dictionary<string, UndeclaredNotation>();
            }
            UndeclaredNotation notation = new UndeclaredNotation(notationName, this.LineNo, this.LinePos - notationName.Length);
            if (this.undeclaredNotations.TryGetValue(notationName, out notation2))
            {
                notation.next = notation2.next;
                notation2.next = notation;
            }
            else
            {
                this.undeclaredNotations.Add(notationName, notation);
            }
        }

        internal static IDtdParser Create()
        {
            return new DtdParser();
        }

        private bool EatPublicKeyword()
        {
            while ((this.charsUsed - this.curPos) < 6)
            {
                if (this.ReadData() == 0)
                {
                    return false;
                }
            }
            if (((this.chars[this.curPos + 1] != 'U') || (this.chars[this.curPos + 2] != 'B')) || (((this.chars[this.curPos + 3] != 'L') || (this.chars[this.curPos + 4] != 'I')) || (this.chars[this.curPos + 5] != 'C')))
            {
                return false;
            }
            this.curPos += 6;
            return true;
        }

        private bool EatSystemKeyword()
        {
            while ((this.charsUsed - this.curPos) < 6)
            {
                if (this.ReadData() == 0)
                {
                    return false;
                }
            }
            if (((this.chars[this.curPos + 1] != 'Y') || (this.chars[this.curPos + 2] != 'S')) || (((this.chars[this.curPos + 3] != 'T') || (this.chars[this.curPos + 4] != 'E')) || (this.chars[this.curPos + 5] != 'M')))
            {
                return false;
            }
            this.curPos += 6;
            return true;
        }

        private XmlQualifiedName GetNameQualified(bool canHavePrefix)
        {
            if (this.colonPos == -1)
            {
                return new XmlQualifiedName(this.nameTable.Add(this.chars, this.tokenStartPos, this.curPos - this.tokenStartPos));
            }
            if (canHavePrefix)
            {
                return new XmlQualifiedName(this.nameTable.Add(this.chars, this.colonPos + 1, (this.curPos - this.colonPos) - 1), this.nameTable.Add(this.chars, this.tokenStartPos, this.colonPos - this.tokenStartPos));
            }
            this.Throw(this.tokenStartPos, "Xml_ColonInLocalName", this.GetNameString());
            return null;
        }

        private string GetNameString()
        {
            return new string(this.chars, this.tokenStartPos, this.curPos - this.tokenStartPos);
        }

        private string GetNmtokenString()
        {
            return this.GetNameString();
        }

        private Token GetToken(bool needWhiteSpace)
        {
            this.whitespaceSeen = false;
        Label_0007:
            switch (this.chars[this.curPos])
            {
                case ' ':
                case '\t':
                    this.whitespaceSeen = true;
                    this.curPos++;
                    goto Label_0007;

                case '%':
                    if ((this.charsUsed - this.curPos) < 2)
                    {
                        goto Label_0387;
                    }
                    if (!this.xmlCharType.IsWhiteSpace(this.chars[this.curPos + 1]))
                    {
                        if (this.IgnoreEntityReferences)
                        {
                            this.curPos++;
                        }
                        else
                        {
                            this.HandleEntityReference(true, false, false);
                        }
                        goto Label_0007;
                    }
                    break;

                case '\n':
                    this.whitespaceSeen = true;
                    this.curPos++;
                    this.readerAdapter.OnNewLine(this.curPos);
                    goto Label_0007;

                case '\r':
                    this.whitespaceSeen = true;
                    if (this.chars[this.curPos + 1] == '\n')
                    {
                        if (this.Normalize)
                        {
                            this.SaveParsingBuffer();
                            this.readerAdapter.CurrentPosition++;
                        }
                        this.curPos += 2;
                    }
                    else
                    {
                        if (((this.curPos + 1) >= this.charsUsed) && !this.readerAdapter.IsEof)
                        {
                            goto Label_0387;
                        }
                        this.chars[this.curPos] = '\n';
                        this.curPos++;
                    }
                    this.readerAdapter.OnNewLine(this.curPos);
                    goto Label_0007;

                case '\0':
                    if (this.curPos != this.charsUsed)
                    {
                        this.ThrowInvalidChar(this.chars, this.charsUsed, this.curPos);
                    }
                    goto Label_0387;
            }
            if ((needWhiteSpace && !this.whitespaceSeen) && (this.scanningFunction != ScanningFunction.ParamEntitySpace))
            {
                this.Throw(this.curPos, "Xml_ExpectingWhiteSpace", this.ParseUnexpectedToken(this.curPos));
            }
            this.tokenStartPos = this.curPos;
        Label_01FD:
            switch (this.scanningFunction)
            {
                case ScanningFunction.SubsetContent:
                    return this.ScanSubsetContent();

                case ScanningFunction.Name:
                    return this.ScanNameExpected();

                case ScanningFunction.QName:
                    return this.ScanQNameExpected();

                case ScanningFunction.Nmtoken:
                    return this.ScanNmtokenExpected();

                case ScanningFunction.Doctype1:
                    return this.ScanDoctype1();

                case ScanningFunction.Doctype2:
                    return this.ScanDoctype2();

                case ScanningFunction.Element1:
                    return this.ScanElement1();

                case ScanningFunction.Element2:
                    return this.ScanElement2();

                case ScanningFunction.Element3:
                    return this.ScanElement3();

                case ScanningFunction.Element4:
                    return this.ScanElement4();

                case ScanningFunction.Element5:
                    return this.ScanElement5();

                case ScanningFunction.Element6:
                    return this.ScanElement6();

                case ScanningFunction.Element7:
                    return this.ScanElement7();

                case ScanningFunction.Attlist1:
                    return this.ScanAttlist1();

                case ScanningFunction.Attlist2:
                    return this.ScanAttlist2();

                case ScanningFunction.Attlist3:
                    return this.ScanAttlist3();

                case ScanningFunction.Attlist4:
                    return this.ScanAttlist4();

                case ScanningFunction.Attlist5:
                    return this.ScanAttlist5();

                case ScanningFunction.Attlist6:
                    return this.ScanAttlist6();

                case ScanningFunction.Attlist7:
                    return this.ScanAttlist7();

                case ScanningFunction.Entity1:
                    return this.ScanEntity1();

                case ScanningFunction.Entity2:
                    return this.ScanEntity2();

                case ScanningFunction.Entity3:
                    return this.ScanEntity3();

                case ScanningFunction.Notation1:
                    return this.ScanNotation1();

                case ScanningFunction.CondSection1:
                    return this.ScanCondSection1();

                case ScanningFunction.CondSection2:
                    return this.ScanCondSection2();

                case ScanningFunction.CondSection3:
                    return this.ScanCondSection3();

                case ScanningFunction.SystemId:
                    return this.ScanSystemId();

                case ScanningFunction.PublicId1:
                    return this.ScanPublicId1();

                case ScanningFunction.PublicId2:
                    return this.ScanPublicId2();

                case ScanningFunction.ClosingTag:
                    return this.ScanClosingTag();

                case ScanningFunction.ParamEntitySpace:
                    this.whitespaceSeen = true;
                    this.scanningFunction = this.savedScanningFunction;
                    goto Label_01FD;

                default:
                    return Token.None;
            }
        Label_0387:
            if ((this.readerAdapter.IsEof || (this.ReadData() == 0)) && !this.HandleEntityEnd(false))
            {
                if (this.scanningFunction == ScanningFunction.SubsetContent)
                {
                    return Token.Eof;
                }
                this.Throw(this.curPos, "Xml_IncompleteDtdContent");
            }
            goto Label_0007;
        }

        private string GetValue()
        {
            if (this.stringBuilder.Length == 0)
            {
                return new string(this.chars, this.tokenStartPos, (this.curPos - this.tokenStartPos) - 1);
            }
            return this.stringBuilder.ToString();
        }

        private string GetValueWithStrippedSpaces()
        {
            string str = (this.stringBuilder.Length == 0) ? new string(this.chars, this.tokenStartPos, (this.curPos - this.tokenStartPos) - 1) : this.stringBuilder.ToString();
            return StripSpaces(str);
        }

        private bool HandleEntityEnd(bool inLiteral)
        {
            IDtdEntityInfo info;
            this.SaveParsingBuffer();
            if (!this.readerAdapter.PopEntity(out info, out this.currentEntityId))
            {
                return false;
            }
            this.LoadParsingBuffer();
            if (info == null)
            {
                if (this.scanningFunction == ScanningFunction.ParamEntitySpace)
                {
                    this.scanningFunction = this.savedScanningFunction;
                }
                return false;
            }
            if (info.IsExternal)
            {
                this.externalEntitiesDepth--;
            }
            if (!inLiteral && (this.scanningFunction != ScanningFunction.ParamEntitySpace))
            {
                this.savedScanningFunction = this.scanningFunction;
                this.scanningFunction = ScanningFunction.ParamEntitySpace;
            }
            return true;
        }

        private bool HandleEntityReference(bool paramEntity, bool inLiteral, bool inAttribute)
        {
            this.curPos++;
            return this.HandleEntityReference(this.ScanEntityName(), paramEntity, inLiteral, inAttribute);
        }

        private bool HandleEntityReference(XmlQualifiedName entityName, bool paramEntity, bool inLiteral, bool inAttribute)
        {
            int num;
            this.SaveParsingBuffer();
            if ((paramEntity && this.ParsingInternalSubset) && !this.ParsingTopLevelMarkup)
            {
                this.Throw((this.curPos - entityName.Name.Length) - 1, "Xml_InvalidParEntityRef");
            }
            SchemaEntity entity = this.VerifyEntityReference(entityName, paramEntity, true, inAttribute);
            if (entity == null)
            {
                return false;
            }
            if (entity.ParsingInProgress)
            {
                this.Throw((this.curPos - entityName.Name.Length) - 1, paramEntity ? "Xml_RecursiveParEntity" : "Xml_RecursiveGenEntity", entityName.Name);
            }
            if (entity.IsExternal)
            {
                if (!this.readerAdapter.PushEntity(entity, out num))
                {
                    return false;
                }
                this.externalEntitiesDepth++;
            }
            else
            {
                if (entity.Text.Length == 0)
                {
                    return false;
                }
                if (!this.readerAdapter.PushEntity(entity, out num))
                {
                    return false;
                }
            }
            this.currentEntityId = num;
            if ((paramEntity && !inLiteral) && (this.scanningFunction != ScanningFunction.ParamEntitySpace))
            {
                this.savedScanningFunction = this.scanningFunction;
                this.scanningFunction = ScanningFunction.ParamEntitySpace;
            }
            this.LoadParsingBuffer();
            return true;
        }

        private void Initialize(IDtdParserAdapter readerAdapter)
        {
            this.readerAdapter = readerAdapter;
            this.readerAdapterWithValidation = readerAdapter as IDtdParserAdapterWithValidation;
            this.nameTable = readerAdapter.NameTable;
            IDtdParserAdapterWithValidation validation = readerAdapter as IDtdParserAdapterWithValidation;
            if (validation != null)
            {
                this.validate = validation.DtdValidation;
            }
            IDtdParserAdapterV1 rv = readerAdapter as IDtdParserAdapterV1;
            if (rv != null)
            {
                this.v1Compat = rv.V1CompatibilityMode;
                this.normalize = rv.Normalization;
                this.supportNamespaces = rv.Namespaces;
            }
            this.schemaInfo = new SchemaInfo();
            this.schemaInfo.SchemaType = SchemaType.DTD;
            this.stringBuilder = new StringBuilder();
            Uri baseUri = readerAdapter.BaseUri;
            if (baseUri != null)
            {
                this.documentBaseUri = baseUri.ToString();
            }
            this.freeFloatingDtd = false;
        }

        private void InitializeFreeFloatingDtd(string baseUri, string docTypeName, string publicId, string systemId, string internalSubset, IDtdParserAdapter adapter)
        {
            int num2;
            this.Initialize(adapter);
            if ((docTypeName == null) || (docTypeName.Length == 0))
            {
                throw XmlConvert.CreateInvalidNameArgumentException(docTypeName, "docTypeName");
            }
            XmlConvert.VerifyName(docTypeName);
            int index = docTypeName.IndexOf(':');
            if (index == -1)
            {
                this.schemaInfo.DocTypeName = new XmlQualifiedName(this.nameTable.Add(docTypeName));
            }
            else
            {
                this.schemaInfo.DocTypeName = new XmlQualifiedName(this.nameTable.Add(docTypeName.Substring(0, index)), this.nameTable.Add(docTypeName.Substring(index + 1)));
            }
            if ((systemId != null) && (systemId.Length > 0))
            {
                num2 = this.xmlCharType.IsOnlyCharData(systemId);
                if (num2 >= 0)
                {
                    this.ThrowInvalidChar(this.curPos, systemId, num2);
                }
                this.systemId = systemId;
            }
            if ((publicId != null) && (publicId.Length > 0))
            {
                num2 = this.xmlCharType.IsPublicId(publicId);
                if (num2 >= 0)
                {
                    this.ThrowInvalidChar(this.curPos, publicId, num2);
                }
                this.publicId = publicId;
            }
            if ((internalSubset != null) && (internalSubset.Length > 0))
            {
                this.readerAdapter.PushInternalDtd(baseUri, internalSubset);
                this.hasFreeFloatingInternalSubset = true;
            }
            Uri uri = this.readerAdapter.BaseUri;
            if (uri != null)
            {
                this.documentBaseUri = uri.ToString();
            }
            this.freeFloatingDtd = true;
        }

        private bool IsAttributeValueType(Token token)
        {
            return ((token >= Token.CDATA) && (token <= Token.NOTATION));
        }

        private void LoadParsingBuffer()
        {
            this.chars = this.readerAdapter.ParsingBuffer;
            this.charsUsed = this.readerAdapter.ParsingBufferLength;
            this.curPos = this.readerAdapter.CurrentPosition;
        }

        private void OnUnexpectedError()
        {
            this.Throw(this.curPos, "Xml_InternalError");
        }

        private void Parse(bool saveInternalSubset)
        {
            if (this.freeFloatingDtd)
            {
                this.ParseFreeFloatingDtd();
            }
            else
            {
                this.ParseInDocumentDtd(saveInternalSubset);
            }
            this.schemaInfo.Finish();
            if (this.validate && (this.undeclaredNotations != null))
            {
                foreach (UndeclaredNotation notation in this.undeclaredNotations.Values)
                {
                    for (UndeclaredNotation notation2 = notation; notation2 != null; notation2 = notation2.next)
                    {
                        this.SendValidationEvent(XmlSeverityType.Error, new XmlSchemaException("Sch_UndeclaredNotation", notation.name, this.BaseUriStr, notation.lineNo, notation.linePos));
                    }
                }
            }
        }

        private void ParseAttlistDecl()
        {
            if (this.GetToken(true) == Token.QName)
            {
                SchemaElementDecl decl;
                XmlQualifiedName nameQualified = this.GetNameQualified(true);
                if (!this.schemaInfo.ElementDecls.TryGetValue(nameQualified, out decl) && !this.schemaInfo.UndeclaredElementDecls.TryGetValue(nameQualified, out decl))
                {
                    decl = new SchemaElementDecl(nameQualified, nameQualified.Namespace);
                    this.schemaInfo.UndeclaredElementDecls.Add(nameQualified, decl);
                }
                SchemaAttDef attrDef = null;
                while (true)
                {
                    Token token = this.GetToken(false);
                    if (token != Token.QName)
                    {
                        if (token == Token.GreaterThan)
                        {
                            if (((this.v1Compat && (attrDef != null)) && ((attrDef.Prefix.Length > 0) && attrDef.Prefix.Equals("xml"))) && (attrDef.Name.Name == "space"))
                            {
                                attrDef.Reserved = SchemaAttDef.Reserve.XmlSpace;
                                if (attrDef.Datatype.TokenizedType != XmlTokenizedType.ENUMERATION)
                                {
                                    this.Throw("Xml_EnumerationRequired", string.Empty, attrDef.LineNumber, attrDef.LinePosition);
                                }
                                if (this.validate)
                                {
                                    attrDef.CheckXmlSpace(this.readerAdapterWithValidation.ValidationEventHandling);
                                }
                            }
                            return;
                        }
                        break;
                    }
                    XmlQualifiedName name = this.GetNameQualified(true);
                    attrDef = new SchemaAttDef(name, name.Namespace) {
                        IsDeclaredInExternal = !this.ParsingInternalSubset,
                        LineNumber = this.LineNo,
                        LinePosition = this.LinePos - (this.curPos - this.tokenStartPos)
                    };
                    bool ignoreErrors = decl.GetAttDef(attrDef.Name) != null;
                    this.ParseAttlistType(attrDef, decl, ignoreErrors);
                    this.ParseAttlistDefault(attrDef, ignoreErrors);
                    if ((attrDef.Prefix.Length > 0) && attrDef.Prefix.Equals("xml"))
                    {
                        if (attrDef.Name.Name == "space")
                        {
                            if (this.v1Compat)
                            {
                                string str = attrDef.DefaultValueExpanded.Trim();
                                if (str.Equals("preserve") || str.Equals("default"))
                                {
                                    attrDef.Reserved = SchemaAttDef.Reserve.XmlSpace;
                                }
                            }
                            else
                            {
                                attrDef.Reserved = SchemaAttDef.Reserve.XmlSpace;
                                if (attrDef.TokenizedType != XmlTokenizedType.ENUMERATION)
                                {
                                    this.Throw("Xml_EnumerationRequired", string.Empty, attrDef.LineNumber, attrDef.LinePosition);
                                }
                                if (this.validate)
                                {
                                    attrDef.CheckXmlSpace(this.readerAdapterWithValidation.ValidationEventHandling);
                                }
                            }
                        }
                        else if (attrDef.Name.Name == "lang")
                        {
                            attrDef.Reserved = SchemaAttDef.Reserve.XmlLang;
                        }
                    }
                    if (!ignoreErrors)
                    {
                        decl.AddAttDef(attrDef);
                    }
                }
            }
            this.OnUnexpectedError();
        }

        private void ParseAttlistDefault(SchemaAttDef attrDef, bool ignoreErrors)
        {
            switch (this.GetToken(true))
            {
                case Token.REQUIRED:
                    attrDef.Presence = SchemaDeclBase.Use.Required;
                    return;

                case Token.IMPLIED:
                    attrDef.Presence = SchemaDeclBase.Use.Implied;
                    return;

                case Token.FIXED:
                    attrDef.Presence = SchemaDeclBase.Use.Fixed;
                    if (this.GetToken(true) != Token.Literal)
                    {
                        goto Label_00CF;
                    }
                    break;

                case Token.Literal:
                    break;

                default:
                    goto Label_00CF;
            }
            if ((this.validate && (attrDef.Datatype.TokenizedType == XmlTokenizedType.ID)) && !ignoreErrors)
            {
                this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_AttListPresence", string.Empty);
            }
            if (attrDef.TokenizedType != XmlTokenizedType.CDATA)
            {
                attrDef.DefaultValueExpanded = this.GetValueWithStrippedSpaces();
            }
            else
            {
                attrDef.DefaultValueExpanded = this.GetValue();
            }
            attrDef.ValueLineNumber = this.literalLineInfo.lineNo;
            attrDef.ValueLinePosition = this.literalLineInfo.linePos + 1;
            DtdValidator.SetDefaultTypedValue(attrDef, this.readerAdapter);
            return;
        Label_00CF:
            this.OnUnexpectedError();
        }

        private void ParseAttlistType(SchemaAttDef attrDef, SchemaElementDecl elementDecl, bool ignoreErrors)
        {
            string str;
            Token token = this.GetToken(true);
            if (token != Token.CDATA)
            {
                elementDecl.HasNonCDataAttribute = true;
            }
            if (!this.IsAttributeValueType(token))
            {
                if (token != Token.LeftParen)
                {
                    goto Label_02A2;
                }
                attrDef.TokenizedType = XmlTokenizedType.ENUMERATION;
                attrDef.SchemaType = XmlSchemaType.GetBuiltInSimpleType(attrDef.Datatype.TypeCode);
                if (this.GetToken(false) != Token.Nmtoken)
                {
                    goto Label_02A2;
                }
                attrDef.AddValue(this.GetNameString());
            Label_0215:
                switch (this.GetToken(false))
                {
                    case Token.RightParen:
                        return;

                    case Token.Or:
                    {
                        if (this.GetToken(false) != Token.Nmtoken)
                        {
                            break;
                        }
                        string nmtokenString = this.GetNmtokenString();
                        if (((this.validate && !this.v1Compat) && ((attrDef.Values != null) && attrDef.Values.Contains(nmtokenString))) && !ignoreErrors)
                        {
                            this.SendValidationEvent(XmlSeverityType.Error, new XmlSchemaException("Xml_AttlistDuplEnumValue", nmtokenString, this.BaseUriStr, this.LineNo, this.LinePos));
                        }
                        attrDef.AddValue(nmtokenString);
                        goto Label_0215;
                    }
                }
                goto Label_02A2;
            }
            attrDef.TokenizedType = (XmlTokenizedType) token;
            attrDef.SchemaType = XmlSchemaType.GetBuiltInSimpleType(attrDef.Datatype.TypeCode);
            Token token2 = token;
            if (token2 != Token.ID)
            {
                if (token2 != Token.NOTATION)
                {
                    return;
                }
            }
            else
            {
                if (this.validate && elementDecl.IsIdDeclared)
                {
                    SchemaAttDef attDef = elementDecl.GetAttDef(attrDef.Name);
                    if (((attDef == null) || (attDef.Datatype.TokenizedType != XmlTokenizedType.ID)) && !ignoreErrors)
                    {
                        this.SendValidationEvent(XmlSeverityType.Error, "Sch_IdAttrDeclared", elementDecl.Name.ToString());
                    }
                }
                elementDecl.IsIdDeclared = true;
                return;
            }
            if (this.validate)
            {
                if (elementDecl.IsNotationDeclared && !ignoreErrors)
                {
                    this.SendValidationEvent(this.curPos - 8, XmlSeverityType.Error, "Sch_DupNotationAttribute", elementDecl.Name.ToString());
                }
                else
                {
                    if (((elementDecl.ContentValidator != null) && (elementDecl.ContentValidator.ContentType == XmlSchemaContentType.Empty)) && !ignoreErrors)
                    {
                        this.SendValidationEvent(this.curPos - 8, XmlSeverityType.Error, "Sch_NotationAttributeOnEmptyElement", elementDecl.Name.ToString());
                    }
                    elementDecl.IsNotationDeclared = true;
                }
            }
            if ((this.GetToken(true) != Token.LeftParen) || (this.GetToken(false) != Token.Name))
            {
                goto Label_02A2;
            }
        Label_0128:
            str = this.GetNameString();
            if (!this.schemaInfo.Notations.ContainsKey(str))
            {
                this.AddUndeclaredNotation(str);
            }
            if (((this.validate && !this.v1Compat) && ((attrDef.Values != null) && attrDef.Values.Contains(str))) && !ignoreErrors)
            {
                this.SendValidationEvent(XmlSeverityType.Error, new XmlSchemaException("Xml_AttlistDuplNotationValue", str, this.BaseUriStr, this.LineNo, this.LinePos));
            }
            attrDef.AddValue(str);
            switch (this.GetToken(false))
            {
                case Token.RightParen:
                    return;

                case Token.Or:
                    if (this.GetToken(false) == Token.Name)
                    {
                        goto Label_0128;
                    }
                    break;
            }
        Label_02A2:
            this.OnUnexpectedError();
        }

        private void ParseComment()
        {
            this.SaveParsingBuffer();
            try
            {
                if (this.SaveInternalSubsetValue)
                {
                    this.readerAdapter.ParseComment(this.internalSubsetValueSb);
                    this.internalSubsetValueSb.Append("-->");
                }
                else
                {
                    this.readerAdapter.ParseComment(null);
                }
            }
            catch (XmlException exception)
            {
                if ((exception.ResString != "Xml_UnexpectedEOF") || (this.currentEntityId == 0))
                {
                    throw;
                }
                this.SendValidationEvent(XmlSeverityType.Error, "Sch_ParEntityRefNesting", null);
            }
            this.LoadParsingBuffer();
        }

        private void ParseCondSection()
        {
            int currentEntityId = this.currentEntityId;
            switch (this.GetToken(false))
            {
                case Token.IGNORE:
                    if (this.GetToken(false) == Token.LeftBracket)
                    {
                        if (this.validate && (currentEntityId != this.currentEntityId))
                        {
                            this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                        }
                        if (this.GetToken(false) == Token.CondSectionEnd)
                        {
                            if (this.validate && (currentEntityId != this.currentEntityId))
                            {
                                this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                                return;
                            }
                            return;
                        }
                    }
                    break;

                case Token.INCLUDE:
                    if (this.GetToken(false) != Token.LeftBracket)
                    {
                        break;
                    }
                    if (this.validate && (currentEntityId != this.currentEntityId))
                    {
                        this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                    }
                    if (this.validate)
                    {
                        if (this.condSectionEntityIds == null)
                        {
                            this.condSectionEntityIds = new int[2];
                        }
                        else if (this.condSectionEntityIds.Length == this.condSectionDepth)
                        {
                            int[] destinationArray = new int[this.condSectionEntityIds.Length * 2];
                            Array.Copy(this.condSectionEntityIds, 0, destinationArray, 0, this.condSectionEntityIds.Length);
                            this.condSectionEntityIds = destinationArray;
                        }
                        this.condSectionEntityIds[this.condSectionDepth] = currentEntityId;
                    }
                    this.condSectionDepth++;
                    return;
            }
            this.OnUnexpectedError();
        }

        private void ParseElementDecl()
        {
            if (this.GetToken(true) == Token.QName)
            {
                SchemaElementDecl decl = null;
                XmlQualifiedName nameQualified = this.GetNameQualified(true);
                if (this.schemaInfo.ElementDecls.TryGetValue(nameQualified, out decl))
                {
                    if (this.validate)
                    {
                        this.SendValidationEvent(this.curPos - nameQualified.Name.Length, XmlSeverityType.Error, "Sch_DupElementDecl", this.GetNameString());
                    }
                }
                else
                {
                    if (this.schemaInfo.UndeclaredElementDecls.TryGetValue(nameQualified, out decl))
                    {
                        this.schemaInfo.UndeclaredElementDecls.Remove(nameQualified);
                    }
                    else
                    {
                        decl = new SchemaElementDecl(nameQualified, nameQualified.Namespace);
                    }
                    this.schemaInfo.ElementDecls.Add(nameQualified, decl);
                }
                decl.IsDeclaredInExternal = !this.ParsingInternalSubset;
                switch (this.GetToken(true))
                {
                    case Token.ANY:
                        decl.ContentValidator = ContentValidator.Any;
                        goto Label_016B;

                    case Token.EMPTY:
                        decl.ContentValidator = ContentValidator.Empty;
                        goto Label_016B;

                    case Token.LeftParen:
                    {
                        int currentEntityId = this.currentEntityId;
                        Token token = this.GetToken(false);
                        if (token != Token.None)
                        {
                            if (token != Token.PCDATA)
                            {
                                break;
                            }
                            ParticleContentValidator pcv = new ParticleContentValidator(XmlSchemaContentType.Mixed);
                            pcv.Start();
                            pcv.OpenGroup();
                            this.ParseElementMixedContent(pcv, currentEntityId);
                            decl.ContentValidator = pcv.Finish(true);
                        }
                        else
                        {
                            ParticleContentValidator validator2 = null;
                            validator2 = new ParticleContentValidator(XmlSchemaContentType.ElementOnly);
                            validator2.Start();
                            validator2.OpenGroup();
                            this.ParseElementOnlyContent(validator2, currentEntityId);
                            decl.ContentValidator = validator2.Finish(true);
                        }
                        goto Label_016B;
                    }
                }
            }
            this.OnUnexpectedError();
            return;
        Label_016B:
            if (this.GetToken(false) != Token.GreaterThan)
            {
                this.ThrowUnexpectedToken(this.curPos, ">");
            }
        }

        private void ParseElementMixedContent(ParticleContentValidator pcv, int startParenEntityId)
        {
            bool flag = false;
            int num = -1;
            int currentEntityId = this.currentEntityId;
        Label_000B:
            switch (this.GetToken(false))
            {
                case Token.RightParen:
                    pcv.CloseGroup();
                    if (this.validate && (this.currentEntityId != startParenEntityId))
                    {
                        this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                    }
                    if ((this.GetToken(false) == Token.Star) && flag)
                    {
                        pcv.AddStar();
                        return;
                    }
                    if (flag)
                    {
                        this.ThrowUnexpectedToken(this.curPos, "*");
                    }
                    return;

                case Token.Or:
                    if (flag)
                    {
                        pcv.AddChoice();
                        break;
                    }
                    flag = true;
                    break;

                default:
                    goto Label_0131;
            }
            if (this.validate)
            {
                num = this.currentEntityId;
                if (currentEntityId < num)
                {
                    this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                }
            }
            if (this.GetToken(false) == Token.QName)
            {
                XmlQualifiedName nameQualified = this.GetNameQualified(true);
                if (pcv.Exists(nameQualified) && this.validate)
                {
                    this.SendValidationEvent(XmlSeverityType.Error, "Sch_DupElement", nameQualified.ToString());
                }
                pcv.AddName(nameQualified, null);
                if (this.validate)
                {
                    currentEntityId = this.currentEntityId;
                    if (currentEntityId < num)
                    {
                        this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                    }
                }
                goto Label_000B;
            }
        Label_0131:
            this.OnUnexpectedError();
            goto Label_000B;
        }

        private void ParseElementOnlyContent(ParticleContentValidator pcv, int startParenEntityId)
        {
            Stack<ParseElementOnlyContent_LocalFrame> stack = new Stack<ParseElementOnlyContent_LocalFrame>();
            ParseElementOnlyContent_LocalFrame item = new ParseElementOnlyContent_LocalFrame(startParenEntityId);
            stack.Push(item);
        Label_0014:
            switch (this.GetToken(false))
            {
                case Token.LeftParen:
                    pcv.OpenGroup();
                    item = new ParseElementOnlyContent_LocalFrame(this.currentEntityId);
                    stack.Push(item);
                    goto Label_0014;

                case Token.GreaterThan:
                    this.Throw(this.curPos, "Xml_InvalidContentModel");
                    goto Label_0159;

                case Token.QName:
                    pcv.AddName(this.GetNameQualified(true), null);
                    this.ParseHowMany(pcv);
                    break;

                default:
                    goto Label_0153;
            }
        Label_0083:
            switch (this.GetToken(false))
            {
                case Token.RightParen:
                    pcv.CloseGroup();
                    if (this.validate && (this.currentEntityId != item.startParenEntityId))
                    {
                        this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                    }
                    this.ParseHowMany(pcv);
                    goto Label_0159;

                case Token.GreaterThan:
                    this.Throw(this.curPos, "Xml_InvalidContentModel");
                    goto Label_0159;

                case Token.Or:
                    if (item.parsingSchema == Token.Comma)
                    {
                        this.Throw(this.curPos, "Xml_InvalidContentModel");
                    }
                    pcv.AddChoice();
                    item.parsingSchema = Token.Or;
                    goto Label_0014;

                case Token.Comma:
                    if (item.parsingSchema == Token.Or)
                    {
                        this.Throw(this.curPos, "Xml_InvalidContentModel");
                    }
                    pcv.AddSequence();
                    item.parsingSchema = Token.Comma;
                    goto Label_0014;
            }
        Label_0153:
            this.OnUnexpectedError();
        Label_0159:
            stack.Pop();
            if (stack.Count <= 0)
            {
                return;
            }
            item = stack.Peek();
            goto Label_0083;
        }

        private void ParseEntityDecl()
        {
            bool isParameter = false;
            SchemaEntity entity = null;
            Token token2 = this.GetToken(true);
            if (token2 != Token.Name)
            {
                if (token2 != Token.Percent)
                {
                    goto Label_01E0;
                }
                isParameter = true;
                if (this.GetToken(true) != Token.Name)
                {
                    goto Label_01E0;
                }
            }
            XmlQualifiedName nameQualified = this.GetNameQualified(false);
            entity = new SchemaEntity(nameQualified, isParameter) {
                BaseURI = this.BaseUriStr,
                DeclaredURI = (this.externalDtdBaseUri.Length == 0) ? this.documentBaseUri : this.externalDtdBaseUri
            };
            if (isParameter)
            {
                if (!this.schemaInfo.ParameterEntities.ContainsKey(nameQualified))
                {
                    this.schemaInfo.ParameterEntities.Add(nameQualified, entity);
                }
            }
            else if (!this.schemaInfo.GeneralEntities.ContainsKey(nameQualified))
            {
                this.schemaInfo.GeneralEntities.Add(nameQualified, entity);
            }
            entity.DeclaredInExternal = !this.ParsingInternalSubset;
            entity.ParsingInProgress = true;
            Token idTokenType = this.GetToken(true);
            switch (idTokenType)
            {
                case Token.PUBLIC:
                case Token.SYSTEM:
                    string str;
                    string str2;
                    this.ParseExternalId(idTokenType, Token.EntityDecl, out str2, out str);
                    entity.IsExternal = true;
                    entity.Url = str;
                    entity.Pubid = str2;
                    if (this.GetToken(false) == Token.NData)
                    {
                        if (isParameter)
                        {
                            this.ThrowUnexpectedToken(this.curPos - 5, ">");
                        }
                        if (!this.whitespaceSeen)
                        {
                            this.Throw(this.curPos - 5, "Xml_ExpectingWhiteSpace", "NDATA");
                        }
                        if (this.GetToken(true) != Token.Name)
                        {
                            goto Label_01E0;
                        }
                        entity.NData = this.GetNameQualified(false);
                        string name = entity.NData.Name;
                        if (!this.schemaInfo.Notations.ContainsKey(name))
                        {
                            this.AddUndeclaredNotation(name);
                        }
                    }
                    break;

                case Token.Literal:
                    entity.Text = this.GetValue();
                    entity.Line = this.literalLineInfo.lineNo;
                    entity.Pos = this.literalLineInfo.linePos;
                    break;

                default:
                    goto Label_01E0;
            }
            if (this.GetToken(false) == Token.GreaterThan)
            {
                entity.ParsingInProgress = false;
                return;
            }
        Label_01E0:
            this.OnUnexpectedError();
        }

        private void ParseExternalId(Token idTokenType, Token declType, out string publicId, out string systemId)
        {
            LineInfo keywordLineInfo = new LineInfo(this.LineNo, this.LinePos - 6);
            publicId = null;
            systemId = null;
            if (this.GetToken(true) != Token.Literal)
            {
                this.ThrowUnexpectedToken(this.curPos, "\"", "'");
            }
            if (idTokenType == Token.SYSTEM)
            {
                systemId = this.GetValue();
                if (systemId.IndexOf('#') >= 0)
                {
                    this.Throw((this.curPos - systemId.Length) - 1, "Xml_FragmentId", new string[] { systemId.Substring(systemId.IndexOf('#')), systemId });
                }
                if ((declType == Token.DOCTYPE) && !this.freeFloatingDtd)
                {
                    this.literalLineInfo.linePos++;
                    this.readerAdapter.OnSystemId(systemId, keywordLineInfo, this.literalLineInfo);
                }
            }
            else
            {
                publicId = this.GetValue();
                int invCharPos = this.xmlCharType.IsPublicId(publicId);
                if (invCharPos >= 0)
                {
                    this.ThrowInvalidChar(((this.curPos - 1) - publicId.Length) + invCharPos, publicId, invCharPos);
                }
                if ((declType == Token.DOCTYPE) && !this.freeFloatingDtd)
                {
                    this.literalLineInfo.linePos++;
                    this.readerAdapter.OnPublicId(publicId, keywordLineInfo, this.literalLineInfo);
                    if (this.GetToken(false) == Token.Literal)
                    {
                        if (!this.whitespaceSeen)
                        {
                            this.Throw("Xml_ExpectingWhiteSpace", new string(this.literalQuoteChar, 1), this.literalLineInfo.lineNo, this.literalLineInfo.linePos);
                        }
                        systemId = this.GetValue();
                        this.literalLineInfo.linePos++;
                        this.readerAdapter.OnSystemId(systemId, keywordLineInfo, this.literalLineInfo);
                    }
                    else
                    {
                        this.ThrowUnexpectedToken(this.curPos, "\"", "'");
                    }
                }
                else if (this.GetToken(false) == Token.Literal)
                {
                    if (!this.whitespaceSeen)
                    {
                        this.Throw("Xml_ExpectingWhiteSpace", new string(this.literalQuoteChar, 1), this.literalLineInfo.lineNo, this.literalLineInfo.linePos);
                    }
                    systemId = this.GetValue();
                }
                else if (declType != Token.NOTATION)
                {
                    this.ThrowUnexpectedToken(this.curPos, "\"", "'");
                }
            }
        }

        private void ParseExternalSubset()
        {
            if (this.readerAdapter.PushExternalSubset(this.systemId, this.publicId))
            {
                Uri baseUri = this.readerAdapter.BaseUri;
                if (baseUri != null)
                {
                    this.externalDtdBaseUri = baseUri.ToString();
                }
                this.externalEntitiesDepth++;
                this.LoadParsingBuffer();
                this.ParseSubset();
            }
        }

        private void ParseFreeFloatingDtd()
        {
            if (this.hasFreeFloatingInternalSubset)
            {
                this.LoadParsingBuffer();
                this.ParseInternalSubset();
                this.SaveParsingBuffer();
            }
            if ((this.systemId != null) && (this.systemId.Length > 0))
            {
                this.ParseExternalSubset();
            }
        }

        private void ParseHowMany(ParticleContentValidator pcv)
        {
            switch (this.GetToken(false))
            {
                case Token.Star:
                    pcv.AddStar();
                    return;

                case Token.QMark:
                    pcv.AddQMark();
                    return;

                case Token.Plus:
                    pcv.AddPlus();
                    return;
            }
        }

        private void ParseInDocumentDtd(bool saveInternalSubset)
        {
            this.LoadParsingBuffer();
            this.scanningFunction = ScanningFunction.QName;
            this.nextScaningFunction = ScanningFunction.Doctype1;
            if (this.GetToken(false) != Token.QName)
            {
                this.OnUnexpectedError();
            }
            this.schemaInfo.DocTypeName = this.GetNameQualified(true);
            Token idTokenType = this.GetToken(false);
            switch (idTokenType)
            {
                case Token.SYSTEM:
                case Token.PUBLIC:
                    this.ParseExternalId(idTokenType, Token.DOCTYPE, out this.publicId, out this.systemId);
                    idTokenType = this.GetToken(false);
                    break;

                case Token.GreaterThan:
                    goto Label_00A1;

                case Token.LeftBracket:
                    if (saveInternalSubset)
                    {
                        this.SaveParsingBuffer();
                        this.internalSubsetValueSb = new StringBuilder();
                    }
                    this.ParseInternalSubset();
                    goto Label_00A1;
            }
            this.OnUnexpectedError();
        Label_00A1:
            this.SaveParsingBuffer();
            if ((this.systemId != null) && (this.systemId.Length > 0))
            {
                this.ParseExternalSubset();
            }
        }

        private void ParseInternalSubset()
        {
            this.ParseSubset();
        }

        private void ParseNotationDecl()
        {
            if (this.GetToken(true) != Token.Name)
            {
                this.OnUnexpectedError();
            }
            XmlQualifiedName nameQualified = this.GetNameQualified(false);
            SchemaNotation notation = null;
            if (!this.schemaInfo.Notations.ContainsKey(nameQualified.Name))
            {
                if (this.undeclaredNotations != null)
                {
                    this.undeclaredNotations.Remove(nameQualified.Name);
                }
                notation = new SchemaNotation(nameQualified);
                this.schemaInfo.Notations.Add(notation.Name.Name, notation);
            }
            else if (this.validate)
            {
                this.SendValidationEvent(this.curPos - nameQualified.Name.Length, XmlSeverityType.Error, "Sch_DupNotation", nameQualified.Name);
            }
            Token idTokenType = this.GetToken(true);
            switch (idTokenType)
            {
                case Token.SYSTEM:
                case Token.PUBLIC:
                    string str;
                    string str2;
                    this.ParseExternalId(idTokenType, Token.NOTATION, out str, out str2);
                    if (notation != null)
                    {
                        notation.SystemLiteral = str2;
                        notation.Pubid = str;
                    }
                    break;

                default:
                    this.OnUnexpectedError();
                    break;
            }
            if (this.GetToken(false) != Token.GreaterThan)
            {
                this.OnUnexpectedError();
            }
        }

        private void ParsePI()
        {
            this.SaveParsingBuffer();
            if (this.SaveInternalSubsetValue)
            {
                this.readerAdapter.ParsePI(this.internalSubsetValueSb);
                this.internalSubsetValueSb.Append("?>");
            }
            else
            {
                this.readerAdapter.ParsePI(null);
            }
            this.LoadParsingBuffer();
        }

        private void ParseSubset()
        {
            Token token;
        Label_0000:
            token = this.GetToken(false);
            int currentEntityId = this.currentEntityId;
            switch (token)
            {
                case Token.AttlistDecl:
                    this.ParseAttlistDecl();
                    goto Label_01E6;

                case Token.ElementDecl:
                    this.ParseElementDecl();
                    goto Label_01E6;

                case Token.EntityDecl:
                    this.ParseEntityDecl();
                    goto Label_01E6;

                case Token.NotationDecl:
                    this.ParseNotationDecl();
                    goto Label_01E6;

                case Token.Comment:
                    this.ParseComment();
                    goto Label_01E6;

                case Token.PI:
                    this.ParsePI();
                    goto Label_01E6;

                case Token.CondSectionStart:
                    if (this.ParsingInternalSubset)
                    {
                        this.Throw(this.curPos - 3, "Xml_InvalidConditionalSection");
                    }
                    this.ParseCondSection();
                    currentEntityId = this.currentEntityId;
                    goto Label_01E6;

                case Token.CondSectionEnd:
                    if (this.condSectionDepth <= 0)
                    {
                        this.Throw(this.curPos - 3, "Xml_UnexpectedCDataEnd");
                    }
                    else
                    {
                        this.condSectionDepth--;
                        if (this.validate && (this.currentEntityId != this.condSectionEntityIds[this.condSectionDepth]))
                        {
                            this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                        }
                    }
                    goto Label_01E6;

                case Token.Eof:
                    if (this.ParsingInternalSubset && !this.freeFloatingDtd)
                    {
                        this.Throw(this.curPos, "Xml_IncompleteDtdContent");
                    }
                    if (this.condSectionDepth != 0)
                    {
                        this.Throw(this.curPos, "Xml_UnclosedConditionalSection");
                    }
                    return;

                case Token.RightBracket:
                    if (!this.ParsingInternalSubset)
                    {
                        this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                        break;
                    }
                    if (this.condSectionDepth != 0)
                    {
                        this.Throw(this.curPos, "Xml_UnclosedConditionalSection");
                    }
                    if (this.internalSubsetValueSb != null)
                    {
                        this.SaveParsingBuffer(this.curPos - 1);
                        this.schemaInfo.InternalDtdSubset = this.internalSubsetValueSb.ToString();
                        this.internalSubsetValueSb = null;
                    }
                    if (this.GetToken(false) == Token.GreaterThan)
                    {
                        break;
                    }
                    this.ThrowUnexpectedToken(this.curPos, ">");
                    return;

                default:
                    goto Label_01E6;
            }
            return;
        Label_01E6:
            if (this.currentEntityId != currentEntityId)
            {
                if (this.validate)
                {
                    this.SendValidationEvent(this.curPos, XmlSeverityType.Error, "Sch_ParEntityRefNesting", string.Empty);
                }
                else if (!this.v1Compat)
                {
                    this.Throw(this.curPos, "Sch_ParEntityRefNesting");
                }
            }
            goto Label_0000;
        }

        private string ParseUnexpectedToken(int startPos)
        {
            if (!this.xmlCharType.IsNCNameSingleChar(this.chars[startPos]))
            {
                return new string(this.chars, startPos, 1);
            }
            int index = startPos;
            while (true)
            {
                if (!this.xmlCharType.IsNCNameSingleChar(this.chars[index]))
                {
                    break;
                }
                index++;
            }
            int num2 = index - startPos;
            return new string(this.chars, startPos, (num2 > 0) ? num2 : 1);
        }

        private int ReadData()
        {
            this.SaveParsingBuffer();
            int num = this.readerAdapter.ReadData();
            this.LoadParsingBuffer();
            return num;
        }

        private bool ReadDataInName()
        {
            int num = this.curPos - this.tokenStartPos;
            this.curPos = this.tokenStartPos;
            bool flag = this.ReadData() != 0;
            this.tokenStartPos = this.curPos;
            this.curPos += num;
            return flag;
        }

        private void SaveParsingBuffer()
        {
            this.SaveParsingBuffer(this.curPos);
        }

        private void SaveParsingBuffer(int internalSubsetValueEndPos)
        {
            if (this.SaveInternalSubsetValue)
            {
                int currentPosition = this.readerAdapter.CurrentPosition;
                if ((internalSubsetValueEndPos - currentPosition) > 0)
                {
                    this.internalSubsetValueSb.Append(this.chars, currentPosition, internalSubsetValueEndPos - currentPosition);
                }
            }
            this.readerAdapter.CurrentPosition = this.curPos;
        }

        private Token ScanAttlist1()
        {
            char ch = this.chars[this.curPos];
            if (ch == '>')
            {
                this.curPos++;
                this.scanningFunction = ScanningFunction.SubsetContent;
                return Token.GreaterThan;
            }
            if (!this.whitespaceSeen)
            {
                this.Throw(this.curPos, "Xml_ExpectingWhiteSpace", this.ParseUnexpectedToken(this.curPos));
            }
            this.ScanQName();
            this.scanningFunction = ScanningFunction.Attlist2;
            return Token.QName;
        }

        private Token ScanAttlist2()
        {
        Label_0000:
            switch (this.chars[this.curPos])
            {
                case 'I':
                    if ((this.charsUsed - this.curPos) < 6)
                    {
                        goto Label_0471;
                    }
                    this.scanningFunction = ScanningFunction.Attlist6;
                    if (this.chars[this.curPos + 1] != 'D')
                    {
                        this.Throw(this.curPos, "Xml_InvalidAttributeType");
                    }
                    if (this.chars[this.curPos + 2] != 'R')
                    {
                        this.curPos += 2;
                        return Token.ID;
                    }
                    if ((this.chars[this.curPos + 3] != 'E') || (this.chars[this.curPos + 4] != 'F'))
                    {
                        this.Throw(this.curPos, "Xml_InvalidAttributeType");
                    }
                    if (this.chars[this.curPos + 5] != 'S')
                    {
                        this.curPos += 5;
                        return Token.IDREF;
                    }
                    this.curPos += 6;
                    return Token.IDREFS;

                case 'N':
                    if (((this.charsUsed - this.curPos) < 8) && !this.readerAdapter.IsEof)
                    {
                        goto Label_0471;
                    }
                    switch (this.chars[this.curPos + 1])
                    {
                        case 'M':
                            if (((this.chars[this.curPos + 2] != 'T') || (this.chars[this.curPos + 3] != 'O')) || (((this.chars[this.curPos + 4] != 'K') || (this.chars[this.curPos + 5] != 'E')) || (this.chars[this.curPos + 6] != 'N')))
                            {
                                this.Throw(this.curPos, "Xml_InvalidAttributeType");
                            }
                            this.scanningFunction = ScanningFunction.Attlist6;
                            if (this.chars[this.curPos + 7] == 'S')
                            {
                                this.curPos += 8;
                                return Token.NMTOKENS;
                            }
                            this.curPos += 7;
                            return Token.NMTOKEN;

                        case 'O':
                            if ((((this.chars[this.curPos + 2] != 'T') || (this.chars[this.curPos + 3] != 'A')) || ((this.chars[this.curPos + 4] != 'T') || (this.chars[this.curPos + 5] != 'I'))) || ((this.chars[this.curPos + 6] != 'O') || (this.chars[this.curPos + 7] != 'N')))
                            {
                                this.Throw(this.curPos, "Xml_InvalidAttributeType");
                            }
                            this.curPos += 8;
                            this.scanningFunction = ScanningFunction.Attlist3;
                            return Token.NOTATION;
                    }
                    break;

                case 'C':
                    if ((this.charsUsed - this.curPos) < 5)
                    {
                        goto Label_0471;
                    }
                    if (((this.chars[this.curPos + 1] != 'D') || (this.chars[this.curPos + 2] != 'A')) || ((this.chars[this.curPos + 3] != 'T') || (this.chars[this.curPos + 4] != 'A')))
                    {
                        this.Throw(this.curPos, "Xml_InvalidAttributeType1");
                    }
                    this.curPos += 5;
                    this.scanningFunction = ScanningFunction.Attlist6;
                    return Token.CDATA;

                case 'E':
                    if ((this.charsUsed - this.curPos) >= 9)
                    {
                        this.scanningFunction = ScanningFunction.Attlist6;
                        if (((this.chars[this.curPos + 1] != 'N') || (this.chars[this.curPos + 2] != 'T')) || ((this.chars[this.curPos + 3] != 'I') || (this.chars[this.curPos + 4] != 'T')))
                        {
                            this.Throw(this.curPos, "Xml_InvalidAttributeType");
                        }
                        switch (this.chars[this.curPos + 5])
                        {
                            case 'I':
                                if ((this.chars[this.curPos + 6] != 'E') || (this.chars[this.curPos + 7] != 'S'))
                                {
                                    this.Throw(this.curPos, "Xml_InvalidAttributeType");
                                }
                                this.curPos += 8;
                                return Token.ENTITIES;

                            case 'Y':
                                this.curPos += 6;
                                return Token.ENTITY;
                        }
                        this.Throw(this.curPos, "Xml_InvalidAttributeType");
                    }
                    goto Label_0471;

                case '(':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Nmtoken;
                    this.nextScaningFunction = ScanningFunction.Attlist5;
                    return Token.LeftParen;

                default:
                    this.Throw(this.curPos, "Xml_InvalidAttributeType");
                    goto Label_0471;
            }
            this.Throw(this.curPos, "Xml_InvalidAttributeType");
        Label_0471:
            if (this.ReadData() == 0)
            {
                this.Throw(this.curPos, "Xml_IncompleteDtdContent");
            }
            goto Label_0000;
        }

        private Token ScanAttlist3()
        {
            if (this.chars[this.curPos] == '(')
            {
                this.curPos++;
                this.scanningFunction = ScanningFunction.Name;
                this.nextScaningFunction = ScanningFunction.Attlist4;
                return Token.LeftParen;
            }
            this.ThrowUnexpectedToken(this.curPos, "(");
            return Token.None;
        }

        private Token ScanAttlist4()
        {
            switch (this.chars[this.curPos])
            {
                case ')':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Attlist6;
                    return Token.RightParen;

                case '|':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Name;
                    this.nextScaningFunction = ScanningFunction.Attlist4;
                    return Token.Or;
            }
            this.ThrowUnexpectedToken(this.curPos, ")", "|");
            return Token.None;
        }

        private Token ScanAttlist5()
        {
            switch (this.chars[this.curPos])
            {
                case ')':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Attlist6;
                    return Token.RightParen;

                case '|':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Nmtoken;
                    this.nextScaningFunction = ScanningFunction.Attlist5;
                    return Token.Or;
            }
            this.ThrowUnexpectedToken(this.curPos, ")", "|");
            return Token.None;
        }

        private Token ScanAttlist6()
        {
            while (true)
            {
                switch (this.chars[this.curPos])
                {
                    case '"':
                    case '\'':
                        this.ScanLiteral(LiteralType.AttributeValue);
                        this.scanningFunction = ScanningFunction.Attlist1;
                        return Token.Literal;

                    case '#':
                    {
                        if ((this.charsUsed - this.curPos) < 6)
                        {
                            break;
                        }
                        char ch2 = this.chars[this.curPos + 1];
                        switch (ch2)
                        {
                            case 'F':
                                if (((this.chars[this.curPos + 2] != 'I') || (this.chars[this.curPos + 3] != 'X')) || ((this.chars[this.curPos + 4] != 'E') || (this.chars[this.curPos + 5] != 'D')))
                                {
                                    this.Throw(this.curPos, "Xml_ExpectAttType");
                                }
                                this.curPos += 6;
                                this.scanningFunction = ScanningFunction.Attlist7;
                                return Token.FIXED;

                            case 'I':
                                if ((this.charsUsed - this.curPos) < 8)
                                {
                                    break;
                                }
                                if ((((this.chars[this.curPos + 2] != 'M') || (this.chars[this.curPos + 3] != 'P')) || ((this.chars[this.curPos + 4] != 'L') || (this.chars[this.curPos + 5] != 'I'))) || ((this.chars[this.curPos + 6] != 'E') || (this.chars[this.curPos + 7] != 'D')))
                                {
                                    this.Throw(this.curPos, "Xml_ExpectAttType");
                                }
                                this.curPos += 8;
                                this.scanningFunction = ScanningFunction.Attlist1;
                                return Token.IMPLIED;
                        }
                        if (ch2 != 'R')
                        {
                            this.Throw(this.curPos, "Xml_ExpectAttType");
                            break;
                        }
                        if ((this.charsUsed - this.curPos) < 9)
                        {
                            break;
                        }
                        if ((((this.chars[this.curPos + 2] != 'E') || (this.chars[this.curPos + 3] != 'Q')) || ((this.chars[this.curPos + 4] != 'U') || (this.chars[this.curPos + 5] != 'I'))) || (((this.chars[this.curPos + 6] != 'R') || (this.chars[this.curPos + 7] != 'E')) || (this.chars[this.curPos + 8] != 'D')))
                        {
                            this.Throw(this.curPos, "Xml_ExpectAttType");
                        }
                        this.curPos += 9;
                        this.scanningFunction = ScanningFunction.Attlist1;
                        return Token.REQUIRED;
                    }
                    default:
                        this.Throw(this.curPos, "Xml_ExpectAttType");
                        break;
                }
                if (this.ReadData() == 0)
                {
                    this.Throw(this.curPos, "Xml_IncompleteDtdContent");
                }
            }
        }

        private Token ScanAttlist7()
        {
            switch (this.chars[this.curPos])
            {
                case '"':
                case '\'':
                    this.ScanLiteral(LiteralType.AttributeValue);
                    this.scanningFunction = ScanningFunction.Attlist1;
                    return Token.Literal;
            }
            this.ThrowUnexpectedToken(this.curPos, "\"", "'");
            return Token.None;
        }

        private Token ScanClosingTag()
        {
            if (this.chars[this.curPos] != '>')
            {
                this.ThrowUnexpectedToken(this.curPos, ">");
            }
            this.curPos++;
            this.scanningFunction = ScanningFunction.SubsetContent;
            return Token.GreaterThan;
        }

        private Token ScanCondSection1()
        {
            if (this.chars[this.curPos] != 'I')
            {
                this.Throw(this.curPos, "Xml_ExpectIgnoreOrInclude");
            }
            this.curPos++;
        Label_0030:
            if ((this.charsUsed - this.curPos) >= 5)
            {
                switch (this.chars[this.curPos])
                {
                    case 'G':
                        if ((((this.chars[this.curPos + 1] == 'N') && (this.chars[this.curPos + 2] == 'O')) && ((this.chars[this.curPos + 3] == 'R') && (this.chars[this.curPos + 4] == 'E'))) && !this.xmlCharType.IsNameSingleChar(this.chars[this.curPos + 5]))
                        {
                            this.nextScaningFunction = ScanningFunction.CondSection3;
                            this.scanningFunction = ScanningFunction.CondSection2;
                            this.curPos += 5;
                            return Token.IGNORE;
                        }
                        break;

                    case 'N':
                        if ((this.charsUsed - this.curPos) >= 6)
                        {
                            if ((((this.chars[this.curPos + 1] == 'C') && (this.chars[this.curPos + 2] == 'L')) && ((this.chars[this.curPos + 3] == 'U') && (this.chars[this.curPos + 4] == 'D'))) && ((this.chars[this.curPos + 5] == 'E') && !this.xmlCharType.IsNameSingleChar(this.chars[this.curPos + 6])))
                            {
                                this.nextScaningFunction = ScanningFunction.SubsetContent;
                                this.scanningFunction = ScanningFunction.CondSection2;
                                this.curPos += 6;
                                return Token.INCLUDE;
                            }
                        }
                        else
                        {
                            goto Label_01C0;
                        }
                        break;
                }
                this.Throw(this.curPos - 1, "Xml_ExpectIgnoreOrInclude");
                return Token.None;
            }
        Label_01C0:
            if (this.ReadData() == 0)
            {
                this.Throw(this.curPos, "Xml_IncompleteDtdContent");
            }
            goto Label_0030;
        }

        private Token ScanCondSection2()
        {
            if (this.chars[this.curPos] != '[')
            {
                this.ThrowUnexpectedToken(this.curPos, "[");
            }
            this.curPos++;
            this.scanningFunction = this.nextScaningFunction;
            return Token.LeftBracket;
        }

        private unsafe Token ScanCondSection3()
        {
            int num = 0;
        Label_0012:
            while (((this.xmlCharType.charProperties[this.chars[this.curPos]] & 0x40) != 0) && (this.chars[this.curPos] != ']'))
            {
                this.curPos++;
            }
            switch (this.chars[this.curPos])
            {
                case '&':
                case '\'':
                case '\t':
                case '"':
                    this.curPos++;
                    goto Label_0012;

                case '<':
                    if ((this.charsUsed - this.curPos) < 3)
                    {
                        break;
                    }
                    if ((this.chars[this.curPos + 1] == '!') && (this.chars[this.curPos + 2] == '['))
                    {
                        num++;
                        this.curPos += 3;
                    }
                    else
                    {
                        this.curPos++;
                    }
                    goto Label_0012;

                case ']':
                    if ((this.charsUsed - this.curPos) < 3)
                    {
                        break;
                    }
                    if ((this.chars[this.curPos + 1] == ']') && (this.chars[this.curPos + 2] == '>'))
                    {
                        if (num <= 0)
                        {
                            this.curPos += 3;
                            this.scanningFunction = ScanningFunction.SubsetContent;
                            return Token.CondSectionEnd;
                        }
                        num--;
                        this.curPos += 3;
                    }
                    else
                    {
                        this.curPos++;
                    }
                    goto Label_0012;

                case '\n':
                    this.curPos++;
                    this.readerAdapter.OnNewLine(this.curPos);
                    goto Label_0012;

                case '\r':
                    if (this.chars[this.curPos + 1] == '\n')
                    {
                        this.curPos += 2;
                    }
                    else
                    {
                        if (((this.curPos + 1) >= this.charsUsed) && !this.readerAdapter.IsEof)
                        {
                            break;
                        }
                        this.curPos++;
                    }
                    this.readerAdapter.OnNewLine(this.curPos);
                    goto Label_0012;

                default:
                    if (this.curPos != this.charsUsed)
                    {
                        char ch = this.chars[this.curPos];
                        if (XmlCharType.IsHighSurrogate(ch))
                        {
                            if ((this.curPos + 1) == this.charsUsed)
                            {
                                break;
                            }
                            this.curPos++;
                            if (XmlCharType.IsLowSurrogate(this.chars[this.curPos]))
                            {
                                this.curPos++;
                                goto Label_0012;
                            }
                        }
                        this.ThrowInvalidChar(this.chars, this.charsUsed, this.curPos);
                        return Token.None;
                    }
                    break;
            }
            if (this.readerAdapter.IsEof || (this.ReadData() == 0))
            {
                if (this.HandleEntityEnd(false))
                {
                    goto Label_0012;
                }
                this.Throw(this.curPos, "Xml_UnclosedConditionalSection");
            }
            this.tokenStartPos = this.curPos;
            goto Label_0012;
        }

        private Token ScanDoctype1()
        {
            switch (this.chars[this.curPos])
            {
                case 'S':
                    if (!this.EatSystemKeyword())
                    {
                        this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
                    }
                    this.nextScaningFunction = ScanningFunction.Doctype2;
                    this.scanningFunction = ScanningFunction.SystemId;
                    return Token.SYSTEM;

                case '[':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.SubsetContent;
                    return Token.LeftBracket;

                case '>':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;

                case 'P':
                    if (!this.EatPublicKeyword())
                    {
                        this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
                    }
                    this.nextScaningFunction = ScanningFunction.Doctype2;
                    this.scanningFunction = ScanningFunction.PublicId1;
                    return Token.PUBLIC;
            }
            this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
            return Token.None;
        }

        private Token ScanDoctype2()
        {
            switch (this.chars[this.curPos])
            {
                case '>':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;

                case '[':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.SubsetContent;
                    return Token.LeftBracket;
            }
            this.Throw(this.curPos, "Xml_ExpectSubOrClose");
            return Token.None;
        }

        private Token ScanElement1()
        {
        Label_0000:
            switch (this.chars[this.curPos])
            {
                case '(':
                    this.scanningFunction = ScanningFunction.Element2;
                    this.curPos++;
                    return Token.LeftParen;

                case 'A':
                    if ((this.charsUsed - this.curPos) < 3)
                    {
                        goto Label_011B;
                    }
                    if ((this.chars[this.curPos + 1] == 'N') && (this.chars[this.curPos + 2] == 'Y'))
                    {
                        this.curPos += 3;
                        this.scanningFunction = ScanningFunction.ClosingTag;
                        return Token.ANY;
                    }
                    break;

                case 'E':
                    if ((this.charsUsed - this.curPos) < 5)
                    {
                        goto Label_011B;
                    }
                    if (((this.chars[this.curPos + 1] != 'M') || (this.chars[this.curPos + 2] != 'P')) || ((this.chars[this.curPos + 3] != 'T') || (this.chars[this.curPos + 4] != 'Y')))
                    {
                        break;
                    }
                    this.curPos += 5;
                    this.scanningFunction = ScanningFunction.ClosingTag;
                    return Token.EMPTY;
            }
            this.Throw(this.curPos, "Xml_InvalidContentModel");
        Label_011B:
            if (this.ReadData() == 0)
            {
                this.Throw(this.curPos, "Xml_IncompleteDtdContent");
            }
            goto Label_0000;
        }

        private Token ScanElement2()
        {
            if (this.chars[this.curPos] == '#')
            {
                while ((this.charsUsed - this.curPos) < 7)
                {
                    if (this.ReadData() == 0)
                    {
                        this.Throw(this.curPos, "Xml_IncompleteDtdContent");
                    }
                }
                if ((((this.chars[this.curPos + 1] == 'P') && (this.chars[this.curPos + 2] == 'C')) && ((this.chars[this.curPos + 3] == 'D') && (this.chars[this.curPos + 4] == 'A'))) && ((this.chars[this.curPos + 5] == 'T') && (this.chars[this.curPos + 6] == 'A')))
                {
                    this.curPos += 7;
                    this.scanningFunction = ScanningFunction.Element6;
                    return Token.PCDATA;
                }
                this.Throw(this.curPos + 1, "Xml_ExpectPcData");
            }
            this.scanningFunction = ScanningFunction.Element3;
            return Token.None;
        }

        private Token ScanElement3()
        {
            switch (this.chars[this.curPos])
            {
                case '(':
                    this.curPos++;
                    return Token.LeftParen;

                case '>':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;
            }
            this.ScanQName();
            this.scanningFunction = ScanningFunction.Element4;
            return Token.QName;
        }

        private Token ScanElement4()
        {
            Token star;
            this.scanningFunction = ScanningFunction.Element5;
            switch (this.chars[this.curPos])
            {
                case '*':
                    star = Token.Star;
                    break;

                case '+':
                    star = Token.Plus;
                    break;

                case '?':
                    star = Token.QMark;
                    break;

                default:
                    return Token.None;
            }
            if (this.whitespaceSeen)
            {
                this.Throw(this.curPos, "Xml_ExpectNoWhitespace");
            }
            this.curPos++;
            return star;
        }

        private Token ScanElement5()
        {
            switch (this.chars[this.curPos])
            {
                case '>':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.SubsetContent;
                    return Token.GreaterThan;

                case '|':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Element3;
                    return Token.Or;

                case ')':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Element4;
                    return Token.RightParen;

                case ',':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Element3;
                    return Token.Comma;
            }
            this.Throw(this.curPos, "Xml_ExpectOp");
            return Token.None;
        }

        private Token ScanElement6()
        {
            switch (this.chars[this.curPos])
            {
                case ')':
                    this.curPos++;
                    this.scanningFunction = ScanningFunction.Element7;
                    return Token.RightParen;

                case '|':
                    this.curPos++;
                    this.nextScaningFunction = ScanningFunction.Element6;
                    this.scanningFunction = ScanningFunction.QName;
                    return Token.Or;
            }
            this.ThrowUnexpectedToken(this.curPos, ")", "|");
            return Token.None;
        }

        private Token ScanElement7()
        {
            this.scanningFunction = ScanningFunction.ClosingTag;
            if ((this.chars[this.curPos] == '*') && !this.whitespaceSeen)
            {
                this.curPos++;
                return Token.Star;
            }
            return Token.None;
        }

        private Token ScanEntity1()
        {
            if (this.chars[this.curPos] == '%')
            {
                this.curPos++;
                this.nextScaningFunction = ScanningFunction.Entity2;
                this.scanningFunction = ScanningFunction.Name;
                return Token.Percent;
            }
            this.ScanName();
            this.scanningFunction = ScanningFunction.Entity2;
            return Token.Name;
        }

        private Token ScanEntity2()
        {
            switch (this.chars[this.curPos])
            {
                case 'P':
                    if (!this.EatPublicKeyword())
                    {
                        this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
                    }
                    this.nextScaningFunction = ScanningFunction.Entity3;
                    this.scanningFunction = ScanningFunction.PublicId1;
                    return Token.PUBLIC;

                case 'S':
                    if (!this.EatSystemKeyword())
                    {
                        this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
                    }
                    this.nextScaningFunction = ScanningFunction.Entity3;
                    this.scanningFunction = ScanningFunction.SystemId;
                    return Token.SYSTEM;

                case '"':
                case '\'':
                    this.ScanLiteral(LiteralType.EntityReplText);
                    this.scanningFunction = ScanningFunction.ClosingTag;
                    return Token.Literal;
            }
            this.Throw(this.curPos, "Xml_ExpectExternalIdOrEntityValue");
            return Token.None;
        }

        private Token ScanEntity3()
        {
            if (this.chars[this.curPos] == 'N')
            {
                while ((this.charsUsed - this.curPos) < 5)
                {
                    if (this.ReadData() == 0)
                    {
                        goto Label_009A;
                    }
                }
                if (((this.chars[this.curPos + 1] == 'D') && (this.chars[this.curPos + 2] == 'A')) && ((this.chars[this.curPos + 3] == 'T') && (this.chars[this.curPos + 4] == 'A')))
                {
                    this.curPos += 5;
                    this.scanningFunction = ScanningFunction.Name;
                    this.nextScaningFunction = ScanningFunction.ClosingTag;
                    return Token.NData;
                }
            }
        Label_009A:
            this.scanningFunction = ScanningFunction.ClosingTag;
            return Token.None;
        }

        private XmlQualifiedName ScanEntityName()
        {
            try
            {
                this.ScanName();
            }
            catch (XmlException exception)
            {
                this.Throw("Xml_ErrorParsingEntityName", string.Empty, exception.LineNumber, exception.LinePosition);
            }
            if (this.chars[this.curPos] != ';')
            {
                this.ThrowUnexpectedToken(this.curPos, ";");
            }
            XmlQualifiedName nameQualified = this.GetNameQualified(false);
            this.curPos++;
            return nameQualified;
        }

        private unsafe Token ScanLiteral(LiteralType literalType)
        {
            char ch = this.chars[this.curPos];
            char ch2 = (literalType == LiteralType.AttributeValue) ? ' ' : '\n';
            int currentEntityId = this.currentEntityId;
            this.literalLineInfo.Set(this.LineNo, this.LinePos);
            this.curPos++;
            this.tokenStartPos = this.curPos;
            this.stringBuilder.Length = 0;
        Label_006C:
            while (((this.xmlCharType.charProperties[this.chars[this.curPos]] & 0x80) != 0) && (this.chars[this.curPos] != '%'))
            {
                this.curPos++;
            }
            if ((this.chars[this.curPos] == ch) && (this.currentEntityId == currentEntityId))
            {
                if (this.stringBuilder.Length > 0)
                {
                    this.stringBuilder.Append(this.chars, this.tokenStartPos, this.curPos - this.tokenStartPos);
                }
                this.curPos++;
                this.literalQuoteChar = ch;
                return Token.Literal;
            }
            int charCount = this.curPos - this.tokenStartPos;
            if (charCount > 0)
            {
                this.stringBuilder.Append(this.chars, this.tokenStartPos, charCount);
                this.tokenStartPos = this.curPos;
            }
            switch (this.chars[this.curPos])
            {
                case '\t':
                    if ((literalType == LiteralType.AttributeValue) && this.Normalize)
                    {
                        this.stringBuilder.Append(' ');
                        this.tokenStartPos++;
                    }
                    this.curPos++;
                    goto Label_006C;

                case '\n':
                    this.curPos++;
                    if (this.Normalize)
                    {
                        this.stringBuilder.Append(ch2);
                        this.tokenStartPos = this.curPos;
                    }
                    this.readerAdapter.OnNewLine(this.curPos);
                    goto Label_006C;

                case '\r':
                    if (this.chars[this.curPos + 1] != '\n')
                    {
                        if ((this.curPos + 1) == this.charsUsed)
                        {
                            goto Label_05CE;
                        }
                        this.curPos++;
                        if (this.Normalize)
                        {
                            this.stringBuilder.Append(ch2);
                            this.tokenStartPos = this.curPos;
                        }
                        goto Label_02E2;
                    }
                    if (!this.Normalize)
                    {
                        goto Label_0290;
                    }
                    if (literalType != LiteralType.AttributeValue)
                    {
                        this.stringBuilder.Append(this.readerAdapter.IsEntityEolNormalized ? "\r\n" : "\n");
                        break;
                    }
                    this.stringBuilder.Append(this.readerAdapter.IsEntityEolNormalized ? "  " : " ");
                    break;

                case '"':
                case '\'':
                case '>':
                    this.curPos++;
                    goto Label_006C;

                case '%':
                    if (literalType == LiteralType.EntityReplText)
                    {
                        this.HandleEntityReference(true, true, literalType == LiteralType.AttributeValue);
                        this.tokenStartPos = this.curPos;
                    }
                    else
                    {
                        this.curPos++;
                    }
                    goto Label_006C;

                case '&':
                    if (literalType != LiteralType.SystemOrPublicID)
                    {
                        if ((this.curPos + 1) == this.charsUsed)
                        {
                            goto Label_05CE;
                        }
                        if (this.chars[this.curPos + 1] == '#')
                        {
                            this.SaveParsingBuffer();
                            int num3 = this.readerAdapter.ParseNumericCharRef(this.SaveInternalSubsetValue ? this.internalSubsetValueSb : null);
                            this.LoadParsingBuffer();
                            this.stringBuilder.Append(this.chars, this.curPos, num3 - this.curPos);
                            this.readerAdapter.CurrentPosition = num3;
                            this.tokenStartPos = num3;
                            this.curPos = num3;
                        }
                        else
                        {
                            this.SaveParsingBuffer();
                            if (literalType == LiteralType.AttributeValue)
                            {
                                int num4 = this.readerAdapter.ParseNamedCharRef(true, this.SaveInternalSubsetValue ? this.internalSubsetValueSb : null);
                                this.LoadParsingBuffer();
                                if (num4 >= 0)
                                {
                                    this.stringBuilder.Append(this.chars, this.curPos, num4 - this.curPos);
                                    this.readerAdapter.CurrentPosition = num4;
                                    this.tokenStartPos = num4;
                                    this.curPos = num4;
                                }
                                else
                                {
                                    this.HandleEntityReference(false, true, true);
                                    this.tokenStartPos = this.curPos;
                                }
                            }
                            else
                            {
                                int num5 = this.readerAdapter.ParseNamedCharRef(false, null);
                                this.LoadParsingBuffer();
                                if (num5 >= 0)
                                {
                                    this.tokenStartPos = this.curPos;
                                    this.curPos = num5;
                                }
                                else
                                {
                                    this.stringBuilder.Append('&');
                                    this.curPos++;
                                    this.tokenStartPos = this.curPos;
                                    XmlQualifiedName entityName = this.ScanEntityName();
                                    this.VerifyEntityReference(entityName, false, false, false);
                                }
                            }
                        }
                    }
                    else
                    {
                        this.curPos++;
                    }
                    goto Label_006C;

                case '<':
                    if (literalType == LiteralType.AttributeValue)
                    {
                        this.Throw(this.curPos, "Xml_BadAttributeChar", XmlException.BuildCharExceptionArgs('<', '\0'));
                    }
                    this.curPos++;
                    goto Label_006C;

                default:
                    if (this.curPos != this.charsUsed)
                    {
                        char ch3 = this.chars[this.curPos];
                        if (XmlCharType.IsHighSurrogate(ch3))
                        {
                            if ((this.curPos + 1) == this.charsUsed)
                            {
                                goto Label_05CE;
                            }
                            this.curPos++;
                            if (XmlCharType.IsLowSurrogate(this.chars[this.curPos]))
                            {
                                this.curPos++;
                                goto Label_006C;
                            }
                        }
                        this.ThrowInvalidChar(this.chars, this.charsUsed, this.curPos);
                        return Token.None;
                    }
                    goto Label_05CE;
            }
            this.tokenStartPos = this.curPos + 2;
            this.SaveParsingBuffer();
            this.readerAdapter.CurrentPosition++;
        Label_0290:
            this.curPos += 2;
        Label_02E2:
            this.readerAdapter.OnNewLine(this.curPos);
            goto Label_006C;
        Label_05CE:
            if ((this.readerAdapter.IsEof || (this.ReadData() == 0)) && ((literalType == LiteralType.SystemOrPublicID) || !this.HandleEntityEnd(true)))
            {
                this.Throw(this.curPos, "Xml_UnclosedQuote");
            }
            this.tokenStartPos = this.curPos;
            goto Label_006C;
        }

        private void ScanName()
        {
            this.ScanQName(false);
        }

        private Token ScanNameExpected()
        {
            this.ScanName();
            this.scanningFunction = this.nextScaningFunction;
            return Token.Name;
        }

        private unsafe void ScanNmtoken()
        {
            this.tokenStartPos = this.curPos;
        Label_000C:
            while (((this.xmlCharType.charProperties[this.chars[this.curPos]] & 8) != 0) || (this.chars[this.curPos] == ':'))
            {
                this.curPos++;
            }
            if (this.curPos < this.charsUsed)
            {
                if ((this.curPos - this.tokenStartPos) == 0)
                {
                    this.Throw(this.curPos, "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(this.chars, this.charsUsed, this.curPos));
                }
            }
            else
            {
                int num = this.curPos - this.tokenStartPos;
                this.curPos = this.tokenStartPos;
                if (this.ReadData() == 0)
                {
                    if (num > 0)
                    {
                        this.tokenStartPos = this.curPos;
                        this.curPos += num;
                        return;
                    }
                    this.Throw(this.curPos, "Xml_UnexpectedEOF", "NmToken");
                }
                this.tokenStartPos = this.curPos;
                this.curPos += num;
                goto Label_000C;
            }
        }

        private Token ScanNmtokenExpected()
        {
            this.ScanNmtoken();
            this.scanningFunction = this.nextScaningFunction;
            return Token.Nmtoken;
        }

        private Token ScanNotation1()
        {
            switch (this.chars[this.curPos])
            {
                case 'P':
                    if (!this.EatPublicKeyword())
                    {
                        this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
                    }
                    this.nextScaningFunction = ScanningFunction.ClosingTag;
                    this.scanningFunction = ScanningFunction.PublicId1;
                    return Token.PUBLIC;

                case 'S':
                    if (!this.EatSystemKeyword())
                    {
                        this.Throw(this.curPos, "Xml_ExpectExternalOrClose");
                    }
                    this.nextScaningFunction = ScanningFunction.ClosingTag;
                    this.scanningFunction = ScanningFunction.SystemId;
                    return Token.SYSTEM;
            }
            this.Throw(this.curPos, "Xml_ExpectExternalOrPublicId");
            return Token.None;
        }

        private Token ScanPublicId1()
        {
            if ((this.chars[this.curPos] != '"') && (this.chars[this.curPos] != '\''))
            {
                this.ThrowUnexpectedToken(this.curPos, "\"", "'");
            }
            this.ScanLiteral(LiteralType.SystemOrPublicID);
            this.scanningFunction = ScanningFunction.PublicId2;
            return Token.Literal;
        }

        private Token ScanPublicId2()
        {
            if ((this.chars[this.curPos] != '"') && (this.chars[this.curPos] != '\''))
            {
                this.scanningFunction = this.nextScaningFunction;
                return Token.None;
            }
            this.ScanLiteral(LiteralType.SystemOrPublicID);
            this.scanningFunction = this.nextScaningFunction;
            return Token.Literal;
        }

        private void ScanQName()
        {
            this.ScanQName(this.SupportNamespaces);
        }

        private unsafe void ScanQName(bool isQName)
        {
            this.tokenStartPos = this.curPos;
            int num = -1;
        Label_000E:
            if (((this.xmlCharType.charProperties[this.chars[this.curPos]] & 4) != 0) || (this.chars[this.curPos] == ':'))
            {
                this.curPos++;
            }
            else if ((this.curPos + 1) >= this.charsUsed)
            {
                if (this.ReadDataInName())
                {
                    goto Label_000E;
                }
                this.Throw(this.curPos, "Xml_UnexpectedEOF", "Name");
            }
            else
            {
                this.Throw(this.curPos, "Xml_BadStartNameChar", XmlException.BuildCharExceptionArgs(this.chars, this.charsUsed, this.curPos));
            }
        Label_00A5:
            while ((this.xmlCharType.charProperties[this.chars[this.curPos]] & 8) != 0)
            {
                this.curPos++;
            }
            if (this.chars[this.curPos] == ':')
            {
                if (isQName)
                {
                    if (num != -1)
                    {
                        this.Throw(this.curPos, "Xml_BadNameChar", XmlException.BuildCharExceptionArgs(':', '\0'));
                    }
                    num = this.curPos - this.tokenStartPos;
                    this.curPos++;
                    goto Label_000E;
                }
                this.curPos++;
                goto Label_00A5;
            }
            if (this.curPos == this.charsUsed)
            {
                if (this.ReadDataInName())
                {
                    goto Label_00A5;
                }
                if (this.tokenStartPos == this.curPos)
                {
                    this.Throw(this.curPos, "Xml_UnexpectedEOF", "Name");
                }
            }
            this.colonPos = (num == -1) ? -1 : (this.tokenStartPos + num);
        }

        private Token ScanQNameExpected()
        {
            this.ScanQName();
            this.scanningFunction = this.nextScaningFunction;
            return Token.QName;
        }

        private Token ScanSubsetContent()
        {
        Label_0000:
            switch (this.chars[this.curPos])
            {
                case '<':
                    switch (this.chars[this.curPos + 1])
                    {
                        case '!':
                            switch (this.chars[this.curPos + 2])
                            {
                                case 'E':
                                    if (this.chars[this.curPos + 3] != 'L')
                                    {
                                        if (this.chars[this.curPos + 3] != 'N')
                                        {
                                            if ((this.charsUsed - this.curPos) >= 4)
                                            {
                                                this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                                                return Token.None;
                                            }
                                        }
                                        else if ((this.charsUsed - this.curPos) >= 8)
                                        {
                                            if (((this.chars[this.curPos + 4] != 'T') || (this.chars[this.curPos + 5] != 'I')) || ((this.chars[this.curPos + 6] != 'T') || (this.chars[this.curPos + 7] != 'Y')))
                                            {
                                                this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                                            }
                                            this.curPos += 8;
                                            this.scanningFunction = ScanningFunction.Entity1;
                                            return Token.EntityDecl;
                                        }
                                        goto Label_0513;
                                    }
                                    if ((this.charsUsed - this.curPos) < 9)
                                    {
                                        goto Label_0513;
                                    }
                                    if (((this.chars[this.curPos + 4] != 'E') || (this.chars[this.curPos + 5] != 'M')) || (((this.chars[this.curPos + 6] != 'E') || (this.chars[this.curPos + 7] != 'N')) || (this.chars[this.curPos + 8] != 'T')))
                                    {
                                        this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                                    }
                                    this.curPos += 9;
                                    this.scanningFunction = ScanningFunction.QName;
                                    this.nextScaningFunction = ScanningFunction.Element1;
                                    return Token.ElementDecl;

                                case 'N':
                                    if ((this.charsUsed - this.curPos) < 10)
                                    {
                                        goto Label_0513;
                                    }
                                    if ((((this.chars[this.curPos + 3] != 'O') || (this.chars[this.curPos + 4] != 'T')) || ((this.chars[this.curPos + 5] != 'A') || (this.chars[this.curPos + 6] != 'T'))) || (((this.chars[this.curPos + 7] != 'I') || (this.chars[this.curPos + 8] != 'O')) || (this.chars[this.curPos + 9] != 'N')))
                                    {
                                        this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                                    }
                                    this.curPos += 10;
                                    this.scanningFunction = ScanningFunction.Name;
                                    this.nextScaningFunction = ScanningFunction.Notation1;
                                    return Token.NotationDecl;

                                case '[':
                                    this.curPos += 3;
                                    this.scanningFunction = ScanningFunction.CondSection1;
                                    return Token.CondSectionStart;

                                case '-':
                                    if (this.chars[this.curPos + 3] == '-')
                                    {
                                        this.curPos += 4;
                                        return Token.Comment;
                                    }
                                    if ((this.charsUsed - this.curPos) >= 4)
                                    {
                                        this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                                    }
                                    goto Label_0513;

                                case 'A':
                                    if ((this.charsUsed - this.curPos) < 9)
                                    {
                                        goto Label_0513;
                                    }
                                    if ((((this.chars[this.curPos + 3] != 'T') || (this.chars[this.curPos + 4] != 'T')) || ((this.chars[this.curPos + 5] != 'L') || (this.chars[this.curPos + 6] != 'I'))) || ((this.chars[this.curPos + 7] != 'S') || (this.chars[this.curPos + 8] != 'T')))
                                    {
                                        this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                                    }
                                    this.curPos += 9;
                                    this.scanningFunction = ScanningFunction.QName;
                                    this.nextScaningFunction = ScanningFunction.Attlist1;
                                    return Token.AttlistDecl;
                            }
                            if ((this.charsUsed - this.curPos) >= 3)
                            {
                                this.Throw(this.curPos + 2, "Xml_ExpectDtdMarkup");
                            }
                            goto Label_0513;

                        case '?':
                            this.curPos += 2;
                            return Token.PI;
                    }
                    if ((this.charsUsed - this.curPos) < 2)
                    {
                        goto Label_0513;
                    }
                    this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
                    return Token.None;

                case ']':
                    if (((this.charsUsed - this.curPos) < 2) && !this.readerAdapter.IsEof)
                    {
                        goto Label_0513;
                    }
                    if (this.chars[this.curPos + 1] != ']')
                    {
                        this.curPos++;
                        this.scanningFunction = ScanningFunction.ClosingTag;
                        return Token.RightBracket;
                    }
                    if (((this.charsUsed - this.curPos) < 3) && !this.readerAdapter.IsEof)
                    {
                        goto Label_0513;
                    }
                    if ((this.chars[this.curPos + 1] == ']') && (this.chars[this.curPos + 2] == '>'))
                    {
                        this.curPos += 3;
                        return Token.CondSectionEnd;
                    }
                    break;
            }
            if ((this.charsUsed - this.curPos) != 0)
            {
                this.Throw(this.curPos, "Xml_ExpectDtdMarkup");
            }
        Label_0513:
            if (this.ReadData() == 0)
            {
                this.Throw(this.charsUsed, "Xml_IncompleteDtdContent");
            }
            goto Label_0000;
        }

        private Token ScanSystemId()
        {
            if ((this.chars[this.curPos] != '"') && (this.chars[this.curPos] != '\''))
            {
                this.ThrowUnexpectedToken(this.curPos, "\"", "'");
            }
            this.ScanLiteral(LiteralType.SystemOrPublicID);
            this.scanningFunction = this.nextScaningFunction;
            return Token.Literal;
        }

        private void SendValidationEvent(XmlSeverityType severity, XmlSchemaException e)
        {
            IValidationEventHandling validationEventHandling = this.readerAdapterWithValidation.ValidationEventHandling;
            if (validationEventHandling != null)
            {
                validationEventHandling.SendEvent(e, severity);
            }
        }

        private void SendValidationEvent(XmlSeverityType severity, string code, string arg)
        {
            this.SendValidationEvent(severity, new XmlSchemaException(code, arg, this.BaseUriStr, this.LineNo, this.LinePos));
        }

        private void SendValidationEvent(int pos, XmlSeverityType severity, string code, string arg)
        {
            this.SendValidationEvent(severity, new XmlSchemaException(code, arg, this.BaseUriStr, this.LineNo, this.LinePos + (pos - this.curPos)));
        }

        internal static string StripSpaces(string value)
        {
            int length = value.Length;
            if (length <= 0)
            {
                return string.Empty;
            }
            int startIndex = 0;
            StringBuilder builder = null;
            while (value[startIndex] == ' ')
            {
                startIndex++;
                if (startIndex == length)
                {
                    return " ";
                }
            }
            int num3 = startIndex;
            while (num3 < length)
            {
                if (value[num3] == ' ')
                {
                    int num4 = num3 + 1;
                    while ((num4 < length) && (value[num4] == ' '))
                    {
                        num4++;
                    }
                    if (num4 == length)
                    {
                        if (builder == null)
                        {
                            return value.Substring(startIndex, num3 - startIndex);
                        }
                        builder.Append(value, startIndex, num3 - startIndex);
                        return builder.ToString();
                    }
                    if (num4 > (num3 + 1))
                    {
                        if (builder == null)
                        {
                            builder = new StringBuilder(length);
                        }
                        builder.Append(value, startIndex, (num3 - startIndex) + 1);
                        startIndex = num4;
                        num3 = num4 - 1;
                    }
                }
                num3++;
            }
            if (builder == null)
            {
                if (startIndex != 0)
                {
                    return value.Substring(startIndex, length - startIndex);
                }
                return value;
            }
            if (num3 > startIndex)
            {
                builder.Append(value, startIndex, num3 - startIndex);
            }
            return builder.ToString();
        }

        IDtdInfo IDtdParser.ParseFreeFloatingDtd(string baseUri, string docTypeName, string publicId, string systemId, string internalSubset, IDtdParserAdapter adapter)
        {
            this.InitializeFreeFloatingDtd(baseUri, docTypeName, publicId, systemId, internalSubset, adapter);
            this.Parse(false);
            return this.schemaInfo;
        }

        IDtdInfo IDtdParser.ParseInternalDtd(IDtdParserAdapter adapter, bool saveInternalSubset)
        {
            this.Initialize(adapter);
            this.Parse(saveInternalSubset);
            return this.schemaInfo;
        }

        private void Throw(int curPos, string res)
        {
            this.Throw(curPos, res, string.Empty);
        }

        private void Throw(int curPos, string res, string arg)
        {
            this.curPos = curPos;
            Uri baseUri = this.readerAdapter.BaseUri;
            this.readerAdapter.Throw(new XmlException(res, arg, this.LineNo, this.LinePos, (baseUri == null) ? null : baseUri.ToString()));
        }

        private void Throw(int curPos, string res, string[] args)
        {
            this.curPos = curPos;
            Uri baseUri = this.readerAdapter.BaseUri;
            this.readerAdapter.Throw(new XmlException(res, args, this.LineNo, this.LinePos, (baseUri == null) ? null : baseUri.ToString()));
        }

        private void Throw(string res, string arg, int lineNo, int linePos)
        {
            Uri baseUri = this.readerAdapter.BaseUri;
            this.readerAdapter.Throw(new XmlException(res, arg, lineNo, linePos, (baseUri == null) ? null : baseUri.ToString()));
        }

        private void ThrowInvalidChar(char[] data, int length, int invCharPos)
        {
            this.Throw(invCharPos, "Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(data, length, invCharPos));
        }

        private void ThrowInvalidChar(int pos, string data, int invCharPos)
        {
            this.Throw(pos, "Xml_InvalidCharacter", XmlException.BuildCharExceptionArgs(data, invCharPos));
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken)
        {
            this.ThrowUnexpectedToken(pos, expectedToken, null);
        }

        private void ThrowUnexpectedToken(int pos, string expectedToken1, string expectedToken2)
        {
            string str = this.ParseUnexpectedToken(pos);
            if (expectedToken2 != null)
            {
                this.Throw(this.curPos, "Xml_UnexpectedTokens2", new string[] { str, expectedToken1, expectedToken2 });
            }
            else
            {
                this.Throw(this.curPos, "Xml_UnexpectedTokenEx", new string[] { str, expectedToken1 });
            }
        }

        private SchemaEntity VerifyEntityReference(XmlQualifiedName entityName, bool paramEntity, bool mustBeDeclared, bool inAttribute)
        {
            SchemaEntity entity;
            if (paramEntity)
            {
                this.schemaInfo.ParameterEntities.TryGetValue(entityName, out entity);
            }
            else
            {
                this.schemaInfo.GeneralEntities.TryGetValue(entityName, out entity);
            }
            if (entity == null)
            {
                if (paramEntity)
                {
                    if (this.validate)
                    {
                        this.SendValidationEvent((this.curPos - entityName.Name.Length) - 1, XmlSeverityType.Error, "Xml_UndeclaredParEntity", entityName.Name);
                    }
                }
                else if (mustBeDeclared)
                {
                    if (!this.ParsingInternalSubset)
                    {
                        if (this.validate)
                        {
                            this.SendValidationEvent((this.curPos - entityName.Name.Length) - 1, XmlSeverityType.Error, "Xml_UndeclaredEntity", entityName.Name);
                        }
                    }
                    else
                    {
                        this.Throw((this.curPos - entityName.Name.Length) - 1, "Xml_UndeclaredEntity", entityName.Name);
                    }
                }
                return null;
            }
            if (!entity.NData.IsEmpty)
            {
                this.Throw((this.curPos - entityName.Name.Length) - 1, "Xml_UnparsedEntityRef", entityName.Name);
            }
            if (inAttribute && entity.IsExternal)
            {
                this.Throw((this.curPos - entityName.Name.Length) - 1, "Xml_ExternalEntityInAttValue", entityName.Name);
            }
            return entity;
        }

        private string BaseUriStr
        {
            get
            {
                Uri baseUri = this.readerAdapter.BaseUri;
                if (baseUri == null)
                {
                    return string.Empty;
                }
                return baseUri.ToString();
            }
        }

        private bool IgnoreEntityReferences
        {
            get
            {
                return (this.scanningFunction == ScanningFunction.CondSection3);
            }
        }

        private int LineNo
        {
            get
            {
                return this.readerAdapter.LineNo;
            }
        }

        private int LinePos
        {
            get
            {
                return (this.curPos - this.readerAdapter.LineStartPosition);
            }
        }

        private bool Normalize
        {
            get
            {
                return this.normalize;
            }
        }

        private bool ParsingInternalSubset
        {
            get
            {
                return (this.externalEntitiesDepth == 0);
            }
        }

        private bool ParsingTopLevelMarkup
        {
            get
            {
                return ((this.scanningFunction == ScanningFunction.SubsetContent) || ((this.scanningFunction == ScanningFunction.ParamEntitySpace) && (this.savedScanningFunction == ScanningFunction.SubsetContent)));
            }
        }

        private bool SaveInternalSubsetValue
        {
            get
            {
                return ((this.readerAdapter.EntityStackLength == 0) && (this.internalSubsetValueSb != null));
            }
        }

        private bool SupportNamespaces
        {
            get
            {
                return this.supportNamespaces;
            }
        }

        private enum LiteralType
        {
            AttributeValue,
            EntityReplText,
            SystemOrPublicID
        }

        private class ParseElementOnlyContent_LocalFrame
        {
            public DtdParser.Token parsingSchema;
            public int startParenEntityId;

            public ParseElementOnlyContent_LocalFrame(int startParentEntityIdParam)
            {
                this.startParenEntityId = startParentEntityIdParam;
                this.parsingSchema = DtdParser.Token.None;
            }
        }

        private enum ScanningFunction
        {
            SubsetContent,
            Name,
            QName,
            Nmtoken,
            Doctype1,
            Doctype2,
            Element1,
            Element2,
            Element3,
            Element4,
            Element5,
            Element6,
            Element7,
            Attlist1,
            Attlist2,
            Attlist3,
            Attlist4,
            Attlist5,
            Attlist6,
            Attlist7,
            Entity1,
            Entity2,
            Entity3,
            Notation1,
            CondSection1,
            CondSection2,
            CondSection3,
            Literal,
            SystemId,
            PublicId1,
            PublicId2,
            ClosingTag,
            ParamEntitySpace,
            None
        }

        private enum Token
        {
            CDATA,
            ID,
            IDREF,
            IDREFS,
            ENTITY,
            ENTITIES,
            NMTOKEN,
            NMTOKENS,
            NOTATION,
            None,
            PERef,
            AttlistDecl,
            ElementDecl,
            EntityDecl,
            NotationDecl,
            Comment,
            PI,
            CondSectionStart,
            CondSectionEnd,
            Eof,
            REQUIRED,
            IMPLIED,
            FIXED,
            QName,
            Name,
            Nmtoken,
            Quote,
            LeftParen,
            RightParen,
            GreaterThan,
            Or,
            LeftBracket,
            RightBracket,
            PUBLIC,
            SYSTEM,
            Literal,
            DOCTYPE,
            NData,
            Percent,
            Star,
            QMark,
            Plus,
            PCDATA,
            Comma,
            ANY,
            EMPTY,
            IGNORE,
            INCLUDE
        }

        private class UndeclaredNotation
        {
            internal int lineNo;
            internal int linePos;
            internal string name;
            internal DtdParser.UndeclaredNotation next;

            internal UndeclaredNotation(string name, int lineNo, int linePos)
            {
                this.name = name;
                this.lineNo = lineNo;
                this.linePos = linePos;
                this.next = null;
            }
        }
    }
}

