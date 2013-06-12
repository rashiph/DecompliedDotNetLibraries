namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal abstract class DatatypeImplementation : XmlSchemaDatatype
    {
        private static XmlSchemaSimpleType anyAtomicType;
        private static XmlSchemaSimpleType anySimpleType;
        private const int anySimpleTypeIndex = 11;
        private DatatypeImplementation baseType;
        internal static System.Xml.Schema.FacetsChecker binaryFacetsChecker = new BinaryFacetsChecker();
        private static Hashtable builtinTypes = new Hashtable();
        private static readonly DatatypeImplementation c_anyAtomicType = new Datatype_anyAtomicType();
        private static readonly DatatypeImplementation c_anySimpleType = new Datatype_anySimpleType();
        private static readonly DatatypeImplementation c_anyURI = new Datatype_anyURI();
        private static readonly DatatypeImplementation c_base64Binary = new Datatype_base64Binary();
        private static readonly DatatypeImplementation c_boolean = new Datatype_boolean();
        private static readonly DatatypeImplementation c_byte = new Datatype_byte();
        private static readonly DatatypeImplementation c_char = new Datatype_char();
        private static readonly DatatypeImplementation c_date = new Datatype_date();
        private static readonly DatatypeImplementation c_dateTime = new Datatype_dateTime();
        private static readonly DatatypeImplementation c_dateTimeNoTz = new Datatype_dateTimeNoTimeZone();
        private static readonly DatatypeImplementation c_dateTimeTz = new Datatype_dateTimeTimeZone();
        private static readonly DatatypeImplementation c_day = new Datatype_day();
        private static readonly DatatypeImplementation c_dayTimeDuration = new Datatype_dayTimeDuration();
        private static readonly DatatypeImplementation c_decimal = new Datatype_decimal();
        private static readonly DatatypeImplementation c_double = new Datatype_double();
        private static readonly DatatypeImplementation c_doubleXdr = new Datatype_doubleXdr();
        private static readonly DatatypeImplementation c_duration = new Datatype_duration();
        private static readonly DatatypeImplementation c_ENTITIES = ((DatatypeImplementation) c_ENTITY.DeriveByList(1, null));
        private static readonly DatatypeImplementation c_ENTITY = new Datatype_ENTITY();
        private static readonly DatatypeImplementation c_ENUMERATION = new Datatype_ENUMERATION();
        private static readonly DatatypeImplementation c_fixed = new Datatype_fixed();
        private static readonly DatatypeImplementation c_float = new Datatype_float();
        private static readonly DatatypeImplementation c_floatXdr = new Datatype_floatXdr();
        private static readonly DatatypeImplementation c_hexBinary = new Datatype_hexBinary();
        private static readonly DatatypeImplementation c_ID = new Datatype_ID();
        private static readonly DatatypeImplementation c_IDREF = new Datatype_IDREF();
        private static readonly DatatypeImplementation c_IDREFS = ((DatatypeImplementation) c_IDREF.DeriveByList(1, null));
        private static readonly DatatypeImplementation c_int = new Datatype_int();
        private static readonly DatatypeImplementation c_integer = new Datatype_integer();
        private static readonly DatatypeImplementation c_language = new Datatype_language();
        private static readonly DatatypeImplementation c_long = new Datatype_long();
        private static readonly DatatypeImplementation c_month = new Datatype_month();
        private static readonly DatatypeImplementation c_monthDay = new Datatype_monthDay();
        private static readonly DatatypeImplementation c_Name = new Datatype_Name();
        private static readonly DatatypeImplementation c_NCName = new Datatype_NCName();
        private static readonly DatatypeImplementation c_negativeInteger = new Datatype_negativeInteger();
        private static readonly DatatypeImplementation c_NMTOKEN = new Datatype_NMTOKEN();
        private static readonly DatatypeImplementation c_NMTOKENS = ((DatatypeImplementation) c_NMTOKEN.DeriveByList(1, null));
        private static readonly DatatypeImplementation c_nonNegativeInteger = new Datatype_nonNegativeInteger();
        private static readonly DatatypeImplementation c_nonPositiveInteger = new Datatype_nonPositiveInteger();
        private static readonly DatatypeImplementation c_normalizedString = new Datatype_normalizedString();
        internal static readonly DatatypeImplementation c_normalizedStringV1Compat = new Datatype_normalizedStringV1Compat();
        private static readonly DatatypeImplementation c_NOTATION = new Datatype_NOTATION();
        private static readonly DatatypeImplementation c_positiveInteger = new Datatype_positiveInteger();
        private static readonly DatatypeImplementation c_QName = new Datatype_QName();
        private static readonly DatatypeImplementation c_QNameXdr = new Datatype_QNameXdr();
        private static readonly DatatypeImplementation c_short = new Datatype_short();
        private static readonly DatatypeImplementation c_string = new Datatype_string();
        private static readonly DatatypeImplementation c_time = new Datatype_time();
        private static readonly DatatypeImplementation c_timeNoTz = new Datatype_timeNoTimeZone();
        private static readonly DatatypeImplementation c_timeTz = new Datatype_timeTimeZone();
        private static readonly DatatypeImplementation c_token = new Datatype_token();
        private static readonly DatatypeImplementation[] c_tokenizedTypes;
        private static readonly DatatypeImplementation[] c_tokenizedTypesXsd;
        internal static readonly DatatypeImplementation c_tokenV1Compat = new Datatype_tokenV1Compat();
        private static readonly DatatypeImplementation c_unsignedByte = new Datatype_unsignedByte();
        private static readonly DatatypeImplementation c_unsignedInt = new Datatype_unsignedInt();
        private static readonly DatatypeImplementation c_unsignedLong = new Datatype_unsignedLong();
        private static readonly DatatypeImplementation c_unsignedShort = new Datatype_unsignedShort();
        private static readonly DatatypeImplementation c_untypedAtomicType = new Datatype_untypedAtomicType();
        private static readonly DatatypeImplementation c_uuid = new Datatype_uuid();
        private static readonly SchemaDatatypeMap[] c_XdrTypes;
        private static readonly SchemaDatatypeMap[] c_XsdTypes;
        private static readonly DatatypeImplementation c_year = new Datatype_year();
        private static readonly DatatypeImplementation c_yearMonth = new Datatype_yearMonth();
        private static readonly DatatypeImplementation c_yearMonthDuration = new Datatype_yearMonthDuration();
        internal static System.Xml.Schema.FacetsChecker dateTimeFacetsChecker = new DateTimeFacetsChecker();
        private static XmlSchemaSimpleType dayTimeDurationType;
        internal static System.Xml.Schema.FacetsChecker durationFacetsChecker = new DurationFacetsChecker();
        private static XmlSchemaSimpleType[] enumToTypeCode = new XmlSchemaSimpleType[0x37];
        internal static System.Xml.Schema.FacetsChecker listFacetsChecker = new ListFacetsChecker();
        internal static System.Xml.Schema.FacetsChecker miscFacetsChecker = new MiscFacetsChecker();
        private static XmlSchemaSimpleType normalizedStringTypeV1Compat;
        internal static System.Xml.Schema.FacetsChecker numeric2FacetsChecker = new Numeric2FacetsChecker();
        private XmlSchemaType parentSchemaType;
        internal static System.Xml.Schema.FacetsChecker qnameFacetsChecker = new QNameFacetsChecker();
        internal static XmlQualifiedName QnAnySimpleType = new XmlQualifiedName("anySimpleType", "http://www.w3.org/2001/XMLSchema");
        internal static XmlQualifiedName QnAnyType = new XmlQualifiedName("anyType", "http://www.w3.org/2001/XMLSchema");
        private RestrictionFacets restriction;
        internal static System.Xml.Schema.FacetsChecker stringFacetsChecker = new StringFacetsChecker();
        private static XmlSchemaSimpleType tokenTypeV1Compat;
        internal static System.Xml.Schema.FacetsChecker unionFacetsChecker = new UnionFacetsChecker();
        private static XmlSchemaSimpleType untypedAtomicType;
        private XmlValueConverter valueConverter;
        private XmlSchemaDatatypeVariety variety;
        private static XmlSchemaSimpleType yearMonthDurationType;

        static DatatypeImplementation()
        {
            DatatypeImplementation[] implementationArray = new DatatypeImplementation[13];
            implementationArray[0] = c_string;
            implementationArray[1] = c_ID;
            implementationArray[2] = c_IDREF;
            implementationArray[3] = c_IDREFS;
            implementationArray[4] = c_ENTITY;
            implementationArray[5] = c_ENTITIES;
            implementationArray[6] = c_NMTOKEN;
            implementationArray[7] = c_NMTOKENS;
            implementationArray[8] = c_NOTATION;
            implementationArray[9] = c_ENUMERATION;
            implementationArray[10] = c_QNameXdr;
            implementationArray[11] = c_NCName;
            c_tokenizedTypes = implementationArray;
            DatatypeImplementation[] implementationArray2 = new DatatypeImplementation[13];
            implementationArray2[0] = c_string;
            implementationArray2[1] = c_ID;
            implementationArray2[2] = c_IDREF;
            implementationArray2[3] = c_IDREFS;
            implementationArray2[4] = c_ENTITY;
            implementationArray2[5] = c_ENTITIES;
            implementationArray2[6] = c_NMTOKEN;
            implementationArray2[7] = c_NMTOKENS;
            implementationArray2[8] = c_NOTATION;
            implementationArray2[9] = c_ENUMERATION;
            implementationArray2[10] = c_QName;
            implementationArray2[11] = c_NCName;
            c_tokenizedTypesXsd = implementationArray2;
            c_XdrTypes = new SchemaDatatypeMap[] { 
                new SchemaDatatypeMap("bin.base64", c_base64Binary), new SchemaDatatypeMap("bin.hex", c_hexBinary), new SchemaDatatypeMap("boolean", c_boolean), new SchemaDatatypeMap("char", c_char), new SchemaDatatypeMap("date", c_date), new SchemaDatatypeMap("dateTime", c_dateTimeNoTz), new SchemaDatatypeMap("dateTime.tz", c_dateTimeTz), new SchemaDatatypeMap("decimal", c_decimal), new SchemaDatatypeMap("entities", c_ENTITIES), new SchemaDatatypeMap("entity", c_ENTITY), new SchemaDatatypeMap("enumeration", c_ENUMERATION), new SchemaDatatypeMap("fixed.14.4", c_fixed), new SchemaDatatypeMap("float", c_doubleXdr), new SchemaDatatypeMap("float.ieee.754.32", c_floatXdr), new SchemaDatatypeMap("float.ieee.754.64", c_doubleXdr), new SchemaDatatypeMap("i1", c_byte), 
                new SchemaDatatypeMap("i2", c_short), new SchemaDatatypeMap("i4", c_int), new SchemaDatatypeMap("i8", c_long), new SchemaDatatypeMap("id", c_ID), new SchemaDatatypeMap("idref", c_IDREF), new SchemaDatatypeMap("idrefs", c_IDREFS), new SchemaDatatypeMap("int", c_int), new SchemaDatatypeMap("nmtoken", c_NMTOKEN), new SchemaDatatypeMap("nmtokens", c_NMTOKENS), new SchemaDatatypeMap("notation", c_NOTATION), new SchemaDatatypeMap("number", c_doubleXdr), new SchemaDatatypeMap("r4", c_floatXdr), new SchemaDatatypeMap("r8", c_doubleXdr), new SchemaDatatypeMap("string", c_string), new SchemaDatatypeMap("time", c_timeNoTz), new SchemaDatatypeMap("time.tz", c_timeTz), 
                new SchemaDatatypeMap("ui1", c_unsignedByte), new SchemaDatatypeMap("ui2", c_unsignedShort), new SchemaDatatypeMap("ui4", c_unsignedInt), new SchemaDatatypeMap("ui8", c_unsignedLong), new SchemaDatatypeMap("uri", c_anyURI), new SchemaDatatypeMap("uuid", c_uuid)
             };
            c_XsdTypes = new SchemaDatatypeMap[] { 
                new SchemaDatatypeMap("ENTITIES", c_ENTITIES, 11), new SchemaDatatypeMap("ENTITY", c_ENTITY, 11), new SchemaDatatypeMap("ID", c_ID, 5), new SchemaDatatypeMap("IDREF", c_IDREF, 5), new SchemaDatatypeMap("IDREFS", c_IDREFS, 11), new SchemaDatatypeMap("NCName", c_NCName, 9), new SchemaDatatypeMap("NMTOKEN", c_NMTOKEN, 40), new SchemaDatatypeMap("NMTOKENS", c_NMTOKENS, 11), new SchemaDatatypeMap("NOTATION", c_NOTATION, 11), new SchemaDatatypeMap("Name", c_Name, 40), new SchemaDatatypeMap("QName", c_QName, 11), new SchemaDatatypeMap("anySimpleType", c_anySimpleType, -1), new SchemaDatatypeMap("anyURI", c_anyURI, 11), new SchemaDatatypeMap("base64Binary", c_base64Binary, 11), new SchemaDatatypeMap("boolean", c_boolean, 11), new SchemaDatatypeMap("byte", c_byte, 0x25), 
                new SchemaDatatypeMap("date", c_date, 11), new SchemaDatatypeMap("dateTime", c_dateTime, 11), new SchemaDatatypeMap("decimal", c_decimal, 11), new SchemaDatatypeMap("double", c_double, 11), new SchemaDatatypeMap("duration", c_duration, 11), new SchemaDatatypeMap("float", c_float, 11), new SchemaDatatypeMap("gDay", c_day, 11), new SchemaDatatypeMap("gMonth", c_month, 11), new SchemaDatatypeMap("gMonthDay", c_monthDay, 11), new SchemaDatatypeMap("gYear", c_year, 11), new SchemaDatatypeMap("gYearMonth", c_yearMonth, 11), new SchemaDatatypeMap("hexBinary", c_hexBinary, 11), new SchemaDatatypeMap("int", c_int, 0x1f), new SchemaDatatypeMap("integer", c_integer, 0x12), new SchemaDatatypeMap("language", c_language, 40), new SchemaDatatypeMap("long", c_long, 0x1d), 
                new SchemaDatatypeMap("negativeInteger", c_negativeInteger, 0x22), new SchemaDatatypeMap("nonNegativeInteger", c_nonNegativeInteger, 0x1d), new SchemaDatatypeMap("nonPositiveInteger", c_nonPositiveInteger, 0x1d), new SchemaDatatypeMap("normalizedString", c_normalizedString, 0x26), new SchemaDatatypeMap("positiveInteger", c_positiveInteger, 0x21), new SchemaDatatypeMap("short", c_short, 0x1c), new SchemaDatatypeMap("string", c_string, 11), new SchemaDatatypeMap("time", c_time, 11), new SchemaDatatypeMap("token", c_token, 0x23), new SchemaDatatypeMap("unsignedByte", c_unsignedByte, 0x2c), new SchemaDatatypeMap("unsignedInt", c_unsignedInt, 0x2b), new SchemaDatatypeMap("unsignedLong", c_unsignedLong, 0x21), new SchemaDatatypeMap("unsignedShort", c_unsignedShort, 0x2a)
             };
            CreateBuiltinTypes();
        }

        protected DatatypeImplementation()
        {
        }

        protected int Compare(byte[] value1, byte[] value2)
        {
            int length = value1.Length;
            if (length != value2.Length)
            {
                return -1;
            }
            for (int i = 0; i < length; i++)
            {
                if (value1[i] != value2[i])
                {
                    return -1;
                }
            }
            return 0;
        }

        internal static void CreateBuiltinTypes()
        {
            SchemaDatatypeMap map = c_XsdTypes[11];
            XmlQualifiedName qname = new XmlQualifiedName(map.Name, "http://www.w3.org/2001/XMLSchema");
            DatatypeImplementation dataType = FromTypeName(qname.Name);
            anySimpleType = StartBuiltinType(qname, dataType);
            dataType.parentSchemaType = anySimpleType;
            builtinTypes.Add(qname, anySimpleType);
            for (int i = 0; i < c_XsdTypes.Length; i++)
            {
                if (i != 11)
                {
                    map = c_XsdTypes[i];
                    qname = new XmlQualifiedName(map.Name, "http://www.w3.org/2001/XMLSchema");
                    dataType = FromTypeName(qname.Name);
                    XmlSchemaSimpleType type = StartBuiltinType(qname, dataType);
                    dataType.parentSchemaType = type;
                    builtinTypes.Add(qname, type);
                    if (dataType.variety == XmlSchemaDatatypeVariety.Atomic)
                    {
                        enumToTypeCode[(int) dataType.TypeCode] = type;
                    }
                }
            }
            for (int j = 0; j < c_XsdTypes.Length; j++)
            {
                if (j != 11)
                {
                    map = c_XsdTypes[j];
                    XmlSchemaSimpleType derivedType = (XmlSchemaSimpleType) builtinTypes[new XmlQualifiedName(map.Name, "http://www.w3.org/2001/XMLSchema")];
                    if (map.ParentIndex == 11)
                    {
                        FinishBuiltinType(derivedType, anySimpleType);
                    }
                    else
                    {
                        XmlSchemaSimpleType baseType = (XmlSchemaSimpleType) builtinTypes[new XmlQualifiedName(c_XsdTypes[map.ParentIndex].Name, "http://www.w3.org/2001/XMLSchema")];
                        FinishBuiltinType(derivedType, baseType);
                    }
                }
            }
            qname = new XmlQualifiedName("anyAtomicType", "http://www.w3.org/2003/11/xpath-datatypes");
            anyAtomicType = StartBuiltinType(qname, c_anyAtomicType);
            c_anyAtomicType.parentSchemaType = anyAtomicType;
            FinishBuiltinType(anyAtomicType, anySimpleType);
            builtinTypes.Add(qname, anyAtomicType);
            enumToTypeCode[10] = anyAtomicType;
            qname = new XmlQualifiedName("untypedAtomic", "http://www.w3.org/2003/11/xpath-datatypes");
            untypedAtomicType = StartBuiltinType(qname, c_untypedAtomicType);
            c_untypedAtomicType.parentSchemaType = untypedAtomicType;
            FinishBuiltinType(untypedAtomicType, anyAtomicType);
            builtinTypes.Add(qname, untypedAtomicType);
            enumToTypeCode[11] = untypedAtomicType;
            qname = new XmlQualifiedName("yearMonthDuration", "http://www.w3.org/2003/11/xpath-datatypes");
            yearMonthDurationType = StartBuiltinType(qname, c_yearMonthDuration);
            c_yearMonthDuration.parentSchemaType = yearMonthDurationType;
            FinishBuiltinType(yearMonthDurationType, enumToTypeCode[0x11]);
            builtinTypes.Add(qname, yearMonthDurationType);
            enumToTypeCode[0x35] = yearMonthDurationType;
            qname = new XmlQualifiedName("dayTimeDuration", "http://www.w3.org/2003/11/xpath-datatypes");
            dayTimeDurationType = StartBuiltinType(qname, c_dayTimeDuration);
            c_dayTimeDuration.parentSchemaType = dayTimeDurationType;
            FinishBuiltinType(dayTimeDurationType, enumToTypeCode[0x11]);
            builtinTypes.Add(qname, dayTimeDurationType);
            enumToTypeCode[0x36] = dayTimeDurationType;
        }

        internal virtual XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            return null;
        }

        internal override XmlSchemaDatatype DeriveByList(XmlSchemaType schemaType)
        {
            return this.DeriveByList(0, schemaType);
        }

        internal XmlSchemaDatatype DeriveByList(int minSize, XmlSchemaType schemaType)
        {
            if (this.variety == XmlSchemaDatatypeVariety.List)
            {
                throw new XmlSchemaException("Sch_ListFromNonatomic", string.Empty);
            }
            if ((this.variety == XmlSchemaDatatypeVariety.Union) && !((Datatype_union) this).HasAtomicMembers())
            {
                throw new XmlSchemaException("Sch_ListFromNonatomic", string.Empty);
            }
            return new Datatype_List(this, minSize) { variety = XmlSchemaDatatypeVariety.List, restriction = null, baseType = c_anySimpleType, parentSchemaType = schemaType };
        }

        internal override XmlSchemaDatatype DeriveByRestriction(XmlSchemaObjectCollection facets, XmlNameTable nameTable, XmlSchemaType schemaType)
        {
            DatatypeImplementation implementation = (DatatypeImplementation) base.MemberwiseClone();
            implementation.restriction = this.FacetsChecker.ConstructRestriction(this, facets, nameTable);
            implementation.baseType = this;
            implementation.parentSchemaType = schemaType;
            implementation.valueConverter = null;
            return implementation;
        }

        internal static DatatypeImplementation DeriveByUnion(XmlSchemaSimpleType[] types, XmlSchemaType schemaType)
        {
            return new Datatype_union(types) { baseType = c_anySimpleType, variety = XmlSchemaDatatypeVariety.Union, parentSchemaType = schemaType };
        }

        internal static void FinishBuiltinType(XmlSchemaSimpleType derivedType, XmlSchemaSimpleType baseType)
        {
            derivedType.SetBaseSchemaType(baseType);
            derivedType.SetDerivedBy(XmlSchemaDerivationMethod.Restriction);
            if (derivedType.Datatype.Variety == XmlSchemaDatatypeVariety.Atomic)
            {
                XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction {
                    BaseTypeName = baseType.QualifiedName
                };
                derivedType.Content = restriction;
            }
            if (derivedType.Datatype.Variety == XmlSchemaDatatypeVariety.List)
            {
                XmlSchemaSimpleTypeList list = new XmlSchemaSimpleTypeList();
                derivedType.SetDerivedBy(XmlSchemaDerivationMethod.List);
                switch (derivedType.Datatype.TypeCode)
                {
                    case XmlTypeCode.Idref:
                        list.ItemType = list.BaseItemType = enumToTypeCode[0x26];
                        break;

                    case XmlTypeCode.Entity:
                        list.ItemType = list.BaseItemType = enumToTypeCode[0x27];
                        break;

                    case XmlTypeCode.NmToken:
                        list.ItemType = list.BaseItemType = enumToTypeCode[0x22];
                        break;
                }
                derivedType.Content = list;
            }
        }

        private static DatatypeImplementation FromTypeName(string name)
        {
            int index = Array.BinarySearch(c_XsdTypes, name, null);
            if (index >= 0)
            {
                return (DatatypeImplementation) c_XsdTypes[index];
            }
            return null;
        }

        internal static DatatypeImplementation FromXdrName(string name)
        {
            int index = Array.BinarySearch(c_XdrTypes, name, null);
            if (index >= 0)
            {
                return (DatatypeImplementation) c_XdrTypes[index];
            }
            return null;
        }

        internal static DatatypeImplementation FromXmlTokenizedType(XmlTokenizedType token)
        {
            return c_tokenizedTypes[(int) token];
        }

        internal static DatatypeImplementation FromXmlTokenizedTypeXsd(XmlTokenizedType token)
        {
            return c_tokenizedTypesXsd[(int) token];
        }

        internal static XmlSchemaSimpleType[] GetBuiltInTypes()
        {
            return enumToTypeCode;
        }

        internal static XmlSchemaSimpleType GetNormalizedStringTypeV1Compat()
        {
            if (normalizedStringTypeV1Compat == null)
            {
                normalizedStringTypeV1Compat = GetSimpleTypeFromTypeCode(XmlTypeCode.NormalizedString).Clone() as XmlSchemaSimpleType;
                normalizedStringTypeV1Compat.SetDatatype(c_normalizedStringV1Compat);
                normalizedStringTypeV1Compat.ElementDecl = new SchemaElementDecl(c_normalizedStringV1Compat);
                normalizedStringTypeV1Compat.ElementDecl.SchemaType = normalizedStringTypeV1Compat;
            }
            return normalizedStringTypeV1Compat;
        }

        internal static XmlTypeCode GetPrimitiveTypeCode(XmlTypeCode typeCode)
        {
            XmlSchemaSimpleType baseXmlSchemaType = enumToTypeCode[(int) typeCode];
            while (baseXmlSchemaType.BaseXmlSchemaType != AnySimpleType)
            {
                baseXmlSchemaType = baseXmlSchemaType.BaseXmlSchemaType as XmlSchemaSimpleType;
            }
            return baseXmlSchemaType.TypeCode;
        }

        internal static XmlSchemaSimpleType GetSimpleTypeFromTypeCode(XmlTypeCode typeCode)
        {
            return enumToTypeCode[(int) typeCode];
        }

        internal static XmlSchemaSimpleType GetSimpleTypeFromXsdType(XmlQualifiedName qname)
        {
            return (XmlSchemaSimpleType) builtinTypes[qname];
        }

        internal static XmlSchemaSimpleType GetTokenTypeV1Compat()
        {
            if (tokenTypeV1Compat == null)
            {
                tokenTypeV1Compat = GetSimpleTypeFromTypeCode(XmlTypeCode.Token).Clone() as XmlSchemaSimpleType;
                tokenTypeV1Compat.SetDatatype(c_tokenV1Compat);
                tokenTypeV1Compat.ElementDecl = new SchemaElementDecl(c_tokenV1Compat);
                tokenTypeV1Compat.ElementDecl.SchemaType = tokenTypeV1Compat;
            }
            return tokenTypeV1Compat;
        }

        internal string GetTypeName()
        {
            XmlSchemaType parentSchemaType = this.parentSchemaType;
            if ((parentSchemaType == null) || parentSchemaType.QualifiedName.IsEmpty)
            {
                return base.TypeCodeString;
            }
            return parentSchemaType.QualifiedName.ToString();
        }

        internal override bool IsComparable(XmlSchemaDatatype dtype)
        {
            XmlTypeCode typeCode = this.TypeCode;
            XmlTypeCode code2 = dtype.TypeCode;
            if (typeCode != code2)
            {
                if (GetPrimitiveTypeCode(typeCode) == GetPrimitiveTypeCode(code2))
                {
                    return true;
                }
                if (!this.IsDerivedFrom(dtype) && !dtype.IsDerivedFrom(this))
                {
                    return false;
                }
            }
            return true;
        }

        public override bool IsDerivedFrom(XmlSchemaDatatype datatype)
        {
            if (datatype == null)
            {
                return false;
            }
            for (DatatypeImplementation implementation = this; implementation != null; implementation = implementation.baseType)
            {
                if (implementation == datatype)
                {
                    return true;
                }
            }
            if (((DatatypeImplementation) datatype).baseType == null)
            {
                Type type = base.GetType();
                Type c = datatype.GetType();
                if (!(c == type))
                {
                    return type.IsSubclassOf(c);
                }
                return true;
            }
            if (((datatype.Variety == XmlSchemaDatatypeVariety.Union) && !datatype.HasLexicalFacets) && (!datatype.HasValueFacets && (this.variety != XmlSchemaDatatypeVariety.Union)))
            {
                return ((Datatype_union) datatype).IsUnionBaseOf(this);
            }
            return ((((this.variety == XmlSchemaDatatypeVariety.Union) || (this.variety == XmlSchemaDatatypeVariety.List)) && (this.restriction == null)) && (datatype == anySimpleType.Datatype));
        }

        internal override bool IsEqual(object o1, object o2)
        {
            return (this.Compare(o1, o2) == 0);
        }

        public override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            object obj2;
            Exception innerException = this.TryParseValue(s, nameTable, nsmgr, out obj2);
            if (innerException != null)
            {
                throw new XmlSchemaException("Sch_InvalidValueDetailed", new string[] { s, this.GetTypeName(), innerException.Message }, innerException, null, 0, 0, null);
            }
            if (this.Variety == XmlSchemaDatatypeVariety.Union)
            {
                XsdSimpleValue value2 = obj2 as XsdSimpleValue;
                return value2.TypedValue;
            }
            return obj2;
        }

        internal override object ParseValue(string s, Type typDest, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr)
        {
            return this.ValueConverter.ChangeType(this.ParseValue(s, nameTable, nsmgr), typDest, nsmgr);
        }

        internal override object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, bool createAtomicValue)
        {
            object obj2;
            if (!createAtomicValue)
            {
                return this.ParseValue(s, nameTable, nsmgr);
            }
            Exception innerException = this.TryParseValue(s, nameTable, nsmgr, out obj2);
            if (innerException != null)
            {
                throw new XmlSchemaException("Sch_InvalidValueDetailed", new string[] { s, this.GetTypeName(), innerException.Message }, innerException, null, 0, 0, null);
            }
            return obj2;
        }

        internal static XmlSchemaSimpleType StartBuiltinType(XmlQualifiedName qname, XmlSchemaDatatype dataType)
        {
            XmlSchemaSimpleType type = new XmlSchemaSimpleType();
            type.SetQualifiedName(qname);
            type.SetDatatype(dataType);
            type.ElementDecl = new SchemaElementDecl(dataType);
            type.ElementDecl.SchemaType = type;
            return type;
        }

        internal override Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver namespaceResolver, out object typedValue)
        {
            Exception exception = null;
            typedValue = null;
            if (value == null)
            {
                return new ArgumentNullException("value");
            }
            string s = value as string;
            if (s != null)
            {
                return this.TryParseValue(s, nameTable, namespaceResolver, out typedValue);
            }
            try
            {
                object obj2 = value;
                if (value.GetType() != this.ValueType)
                {
                    obj2 = this.ValueConverter.ChangeType(value, this.ValueType, namespaceResolver);
                }
                if (this.HasLexicalFacets)
                {
                    string parseString = (string) this.ValueConverter.ChangeType(value, typeof(string), namespaceResolver);
                    exception = this.FacetsChecker.CheckLexicalFacets(ref parseString, this);
                    if (exception != null)
                    {
                        return exception;
                    }
                }
                if (this.HasValueFacets)
                {
                    exception = this.FacetsChecker.CheckValueFacets(obj2, this);
                    if (exception != null)
                    {
                        return exception;
                    }
                }
                typedValue = obj2;
                return null;
            }
            catch (FormatException exception2)
            {
                exception = exception2;
            }
            catch (InvalidCastException exception3)
            {
                exception = exception3;
            }
            catch (OverflowException exception4)
            {
                exception = exception4;
            }
            catch (ArgumentException exception5)
            {
                exception = exception5;
            }
            return exception;
        }

        internal override void VerifySchemaValid(XmlSchemaObjectTable notations, XmlSchemaObject caller)
        {
        }

        internal static XmlSchemaSimpleType AnyAtomicType
        {
            get
            {
                return anyAtomicType;
            }
        }

        internal static XmlSchemaSimpleType AnySimpleType
        {
            get
            {
                return anySimpleType;
            }
        }

        protected DatatypeImplementation Base
        {
            get
            {
                return this.baseType;
            }
        }

        internal override XmlSchemaWhiteSpace BuiltInWhitespaceFacet
        {
            get
            {
                return XmlSchemaWhiteSpace.Preserve;
            }
        }

        internal static XmlSchemaSimpleType DayTimeDurationType
        {
            get
            {
                return dayTimeDurationType;
            }
        }

        internal override System.Xml.Schema.FacetsChecker FacetsChecker
        {
            get
            {
                return miscFacetsChecker;
            }
        }

        internal override bool HasLexicalFacets
        {
            get
            {
                RestrictionFlags flags = (this.restriction != null) ? this.restriction.Flags : ((RestrictionFlags) 0);
                return ((flags != 0) && ((flags & (RestrictionFlags.FractionDigits | RestrictionFlags.TotalDigits | RestrictionFlags.WhiteSpace | RestrictionFlags.Pattern)) != 0));
            }
        }

        internal override bool HasValueFacets
        {
            get
            {
                RestrictionFlags flags = (this.restriction != null) ? this.restriction.Flags : ((RestrictionFlags) 0);
                return ((flags != 0) && ((flags & (RestrictionFlags.FractionDigits | RestrictionFlags.TotalDigits | RestrictionFlags.MinExclusive | RestrictionFlags.MinInclusive | RestrictionFlags.MaxExclusive | RestrictionFlags.MaxInclusive | RestrictionFlags.Enumeration | RestrictionFlags.MaxLength | RestrictionFlags.MinLength | RestrictionFlags.Length)) != 0));
            }
        }

        internal abstract Type ListValueType { get; }

        internal override RestrictionFacets Restriction
        {
            get
            {
                return this.restriction;
            }
            set
            {
                this.restriction = value;
            }
        }

        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return XmlTokenizedType.None;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.None;
            }
        }

        internal static XmlSchemaSimpleType UntypedAtomicType
        {
            get
            {
                return untypedAtomicType;
            }
        }

        internal abstract RestrictionFlags ValidRestrictionFlags { get; }

        internal override XmlValueConverter ValueConverter
        {
            get
            {
                if (this.valueConverter == null)
                {
                    this.valueConverter = this.CreateValueConverter(this.parentSchemaType);
                }
                return this.valueConverter;
            }
        }

        public override Type ValueType
        {
            get
            {
                return typeof(string);
            }
        }

        public override XmlSchemaDatatypeVariety Variety
        {
            get
            {
                return this.variety;
            }
        }

        internal static XmlSchemaSimpleType YearMonthDurationType
        {
            get
            {
                return yearMonthDurationType;
            }
        }

        private class SchemaDatatypeMap : IComparable
        {
            private string name;
            private int parentIndex;
            private DatatypeImplementation type;

            internal SchemaDatatypeMap(string name, DatatypeImplementation type)
            {
                this.name = name;
                this.type = type;
            }

            internal SchemaDatatypeMap(string name, DatatypeImplementation type, int parentIndex)
            {
                this.name = name;
                this.type = type;
                this.parentIndex = parentIndex;
            }

            public int CompareTo(object obj)
            {
                return string.Compare(this.name, (string) obj, StringComparison.Ordinal);
            }

            public static explicit operator DatatypeImplementation(DatatypeImplementation.SchemaDatatypeMap sdm)
            {
                return sdm.type;
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }

            public int ParentIndex
            {
                get
                {
                    return this.parentIndex;
                }
            }
        }
    }
}

