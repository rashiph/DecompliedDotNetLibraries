namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Xml;
    using System.Xml.Serialization.Configuration;

    public abstract class XmlSerializationReader : XmlSerializationGeneratedCode
    {
        private string anyURIID;
        private string arrayID;
        private string arraySizeID;
        private string arrayTypeID;
        private string base64BinaryID;
        private string base64ID;
        private string booleanID;
        private string byteID;
        private Hashtable callbacks;
        private string charID;
        private static bool checkDeserializeAdvances;
        private ArrayList collectionFixups;
        private XmlCountingReader countingReader;
        private XmlDocument d;
        private string dateID;
        private string dateTimeID;
        private string decimalID;
        private bool decodeName = true;
        private string doubleID;
        private string durationID;
        private string ENTITIESID;
        private string ENTITYID;
        private XmlDeserializationEvents events;
        private ArrayList fixups;
        private string floatID;
        private string gDayID;
        private string gMonthDayID;
        private string gMonthID;
        private string guidID;
        private string gYearID;
        private string gYearMonthID;
        private string hexBinaryID;
        private string IDID;
        private string IDREFID;
        private string IDREFSID;
        private string instanceNs1999ID;
        private string instanceNs2000ID;
        private string instanceNsID;
        private string integerID;
        private string intID;
        private bool isReturnValue;
        private string itemTypeID;
        private string languageID;
        private string longID;
        private string NameID;
        private string NCNameID;
        private string negativeIntegerID;
        private string nilID;
        private string NMTOKENID;
        private string NMTOKENSID;
        private string nonNegativeIntegerID;
        private string nonPositiveIntegerID;
        private string normalizedStringID;
        private string NOTATIONID;
        private string nullID;
        private string oldDecimalID;
        private string oldTimeInstantID;
        private string positiveIntegerID;
        private string qnameID;
        private XmlReader r;
        private Hashtable referencedTargets;
        private string schemaID;
        private string schemaNonXsdTypesNsID;
        private string schemaNs1999ID;
        private string schemaNs2000ID;
        private string schemaNsID;
        private string shortID;
        private bool soap12;
        private string soap12NsID;
        private string soapNsID;
        private string stringID;
        private Hashtable targets;
        private ArrayList targetsWithoutIds;
        private string timeID;
        private string tokenID;
        private string typeID;
        private Hashtable types;
        private Hashtable typesReverse;
        private string unsignedByteID;
        private string unsignedIntID;
        private string unsignedLongID;
        private string unsignedShortID;
        private string urTypeID;
        private string wsdlArrayTypeID;
        private string wsdlNsID;

        static XmlSerializationReader()
        {
            XmlSerializerSection section = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) as XmlSerializerSection;
            checkDeserializeAdvances = (section != null) && section.CheckDeserializeAdvances;
        }

        protected XmlSerializationReader()
        {
        }

        protected void AddFixup(CollectionFixup fixup)
        {
            if (this.collectionFixups == null)
            {
                this.collectionFixups = new ArrayList();
            }
            this.collectionFixups.Add(fixup);
        }

        protected void AddFixup(Fixup fixup)
        {
            if (this.fixups == null)
            {
                this.fixups = new ArrayList();
            }
            this.fixups.Add(fixup);
        }

        protected void AddReadCallback(string name, string ns, Type type, XmlSerializationReadCallback read)
        {
            XmlQualifiedName name2 = new XmlQualifiedName(this.r.NameTable.Add(name), this.r.NameTable.Add(ns));
            this.callbacks[name2] = read;
            this.types[name2] = type;
            this.typesReverse[type] = name2;
        }

        protected void AddTarget(string id, object o)
        {
            if (id == null)
            {
                if (this.targetsWithoutIds == null)
                {
                    this.targetsWithoutIds = new ArrayList();
                }
                if (o != null)
                {
                    this.targetsWithoutIds.Add(o);
                }
            }
            else
            {
                if (this.targets == null)
                {
                    this.targets = new Hashtable();
                }
                if (!this.targets.Contains(id))
                {
                    this.targets.Add(id, o);
                }
            }
        }

        protected void CheckReaderCount(ref int whileIterations, ref int readerCount)
        {
            if (checkDeserializeAdvances)
            {
                whileIterations++;
                if ((whileIterations & 0x80) == 0x80)
                {
                    if (readerCount == this.ReaderCount)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInternalErrorReaderAdvance"));
                    }
                    readerCount = this.ReaderCount;
                }
            }
        }

        protected string CollapseWhitespace(string value)
        {
            if (value == null)
            {
                return null;
            }
            return value.Trim();
        }

        protected Exception CreateAbstractTypeException(string name, string ns)
        {
            return new InvalidOperationException(Res.GetString("XmlAbstractType", new object[] { name, ns, this.CurrentTag() }));
        }

        protected Exception CreateBadDerivationException(string xsdDerived, string nsDerived, string xsdBase, string nsBase, string clrDerived, string clrBase)
        {
            return new InvalidOperationException(Res.GetString("XmlSerializableBadDerivation", new object[] { xsdDerived, nsDerived, xsdBase, nsBase, clrDerived, clrBase }));
        }

        protected Exception CreateCtorHasSecurityException(string typeName)
        {
            return new InvalidOperationException(Res.GetString("XmlConstructorHasSecurityAttributes", new object[] { typeName }));
        }

        protected Exception CreateInaccessibleConstructorException(string typeName)
        {
            return new InvalidOperationException(Res.GetString("XmlConstructorInaccessible", new object[] { typeName }));
        }

        protected Exception CreateInvalidCastException(Type type, object value)
        {
            return this.CreateInvalidCastException(type, value, null);
        }

        protected Exception CreateInvalidCastException(Type type, object value, string id)
        {
            if (value == null)
            {
                return new InvalidCastException(Res.GetString("XmlInvalidNullCast", new object[] { type.FullName }));
            }
            if (id == null)
            {
                return new InvalidCastException(Res.GetString("XmlInvalidCast", new object[] { value.GetType().FullName, type.FullName }));
            }
            return new InvalidCastException(Res.GetString("XmlInvalidCastWithId", new object[] { value.GetType().FullName, type.FullName, id }));
        }

        protected Exception CreateMissingIXmlSerializableType(string name, string ns, string clrType)
        {
            return new InvalidOperationException(Res.GetString("XmlSerializableMissingClrType", new object[] { name, ns, typeof(XmlIncludeAttribute).Name, clrType }));
        }

        protected Exception CreateReadOnlyCollectionException(string name)
        {
            return new InvalidOperationException(Res.GetString("XmlReadOnlyCollection", new object[] { name }));
        }

        protected Exception CreateUnknownConstantException(string value, Type enumType)
        {
            return new InvalidOperationException(Res.GetString("XmlUnknownConstant", new object[] { value, enumType.Name }));
        }

        protected Exception CreateUnknownNodeException()
        {
            return new InvalidOperationException(Res.GetString("XmlUnknownNode", new object[] { this.CurrentTag() }));
        }

        protected Exception CreateUnknownTypeException(XmlQualifiedName type)
        {
            return new InvalidOperationException(Res.GetString("XmlUnknownType", new object[] { type.Name, type.Namespace, this.CurrentTag() }));
        }

        private string CurrentTag()
        {
            switch (this.r.NodeType)
            {
                case XmlNodeType.Element:
                    return ("<" + this.r.LocalName + " xmlns='" + this.r.NamespaceURI + "'>");

                case XmlNodeType.Text:
                    return this.r.Value;

                case XmlNodeType.CDATA:
                    return "CDATA";

                case XmlNodeType.ProcessingInstruction:
                    return "<?";

                case XmlNodeType.Comment:
                    return "<--";

                case XmlNodeType.EndElement:
                    return ">";
            }
            return "(unknown)";
        }

        private void DoFixups()
        {
            if (this.fixups != null)
            {
                for (int i = 0; i < this.fixups.Count; i++)
                {
                    Fixup fixup = (Fixup) this.fixups[i];
                    fixup.Callback(fixup);
                }
                if (this.collectionFixups != null)
                {
                    for (int j = 0; j < this.collectionFixups.Count; j++)
                    {
                        CollectionFixup fixup2 = (CollectionFixup) this.collectionFixups[j];
                        fixup2.Callback(fixup2.Collection, fixup2.CollectionItems);
                    }
                }
            }
        }

        protected Array EnsureArrayIndex(Array a, int index, Type elementType)
        {
            if (a == null)
            {
                return Array.CreateInstance(elementType, 0x20);
            }
            if (index < a.Length)
            {
                return a;
            }
            Array destinationArray = Array.CreateInstance(elementType, (int) (a.Length * 2));
            Array.Copy(a, destinationArray, index);
            return destinationArray;
        }

        protected void FixupArrayRefs(object fixup)
        {
            Fixup fixup2 = (Fixup) fixup;
            Array source = (Array) fixup2.Source;
            for (int i = 0; i < source.Length; i++)
            {
                string id = fixup2.Ids[i];
                if (id != null)
                {
                    object target = this.GetTarget(id);
                    try
                    {
                        source.SetValue(target, i);
                    }
                    catch (InvalidCastException)
                    {
                        throw new InvalidOperationException(Res.GetString("XmlInvalidArrayRef", new object[] { id, target.GetType().FullName, i.ToString(CultureInfo.InvariantCulture) }));
                    }
                }
            }
        }

        protected int GetArrayLength(string name, string ns)
        {
            if (this.GetNullAttr())
            {
                return 0;
            }
            string attribute = this.r.GetAttribute(this.arrayTypeID, this.soapNsID);
            SoapArrayInfo info = this.ParseArrayType(attribute);
            if (info.dimensions != 1)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidArrayDimentions", new object[] { this.CurrentTag() }));
            }
            XmlQualifiedName name2 = this.ToXmlQualifiedName(info.qname, false);
            if (name2.Name != name)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidArrayTypeName", new object[] { name2.Name, name, this.CurrentTag() }));
            }
            if (name2.Namespace != ns)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidArrayTypeNamespace", new object[] { name2.Namespace, ns, this.CurrentTag() }));
            }
            return info.length;
        }

        private void GetCurrentPosition(out int lineNumber, out int linePosition)
        {
            if (this.Reader is IXmlLineInfo)
            {
                IXmlLineInfo reader = (IXmlLineInfo) this.Reader;
                lineNumber = reader.LineNumber;
                linePosition = reader.LinePosition;
            }
            else
            {
                lineNumber = linePosition = -1;
            }
        }

        protected bool GetNullAttr()
        {
            string attribute = this.r.GetAttribute(this.nilID, this.instanceNsID);
            if (attribute == null)
            {
                attribute = this.r.GetAttribute(this.nullID, this.instanceNsID);
            }
            if (attribute == null)
            {
                attribute = this.r.GetAttribute(this.nullID, this.instanceNs2000ID);
                if (attribute == null)
                {
                    attribute = this.r.GetAttribute(this.nullID, this.instanceNs1999ID);
                }
            }
            return ((attribute != null) && XmlConvert.ToBoolean(attribute));
        }

        private Type GetPrimitiveType(XmlQualifiedName typeName, bool throwOnUnknown)
        {
            this.InitPrimitiveIDs();
            if (((typeName.Namespace == this.schemaNsID) || (typeName.Namespace == this.soapNsID)) || (typeName.Namespace == this.soap12NsID))
            {
                if ((((((typeName.Name == this.stringID) || (typeName.Name == this.anyURIID)) || ((typeName.Name == this.durationID) || (typeName.Name == this.ENTITYID))) || (((typeName.Name == this.ENTITIESID) || (typeName.Name == this.gDayID)) || ((typeName.Name == this.gMonthID) || (typeName.Name == this.gMonthDayID)))) || ((((typeName.Name == this.gYearID) || (typeName.Name == this.gYearMonthID)) || ((typeName.Name == this.IDID) || (typeName.Name == this.IDREFID))) || (((typeName.Name == this.IDREFSID) || (typeName.Name == this.integerID)) || ((typeName.Name == this.languageID) || (typeName.Name == this.NameID))))) || ((((typeName.Name == this.NCNameID) || (typeName.Name == this.NMTOKENID)) || ((typeName.Name == this.NMTOKENSID) || (typeName.Name == this.negativeIntegerID))) || ((((typeName.Name == this.nonPositiveIntegerID) || (typeName.Name == this.nonNegativeIntegerID)) || ((typeName.Name == this.normalizedStringID) || (typeName.Name == this.NOTATIONID))) || ((typeName.Name == this.positiveIntegerID) || (typeName.Name == this.tokenID)))))
                {
                    return typeof(string);
                }
                if (typeName.Name == this.intID)
                {
                    return typeof(int);
                }
                if (typeName.Name == this.booleanID)
                {
                    return typeof(bool);
                }
                if (typeName.Name == this.shortID)
                {
                    return typeof(short);
                }
                if (typeName.Name == this.longID)
                {
                    return typeof(long);
                }
                if (typeName.Name == this.floatID)
                {
                    return typeof(float);
                }
                if (typeName.Name == this.doubleID)
                {
                    return typeof(double);
                }
                if (typeName.Name == this.decimalID)
                {
                    return typeof(decimal);
                }
                if (typeName.Name == this.dateTimeID)
                {
                    return typeof(DateTime);
                }
                if (typeName.Name == this.qnameID)
                {
                    return typeof(XmlQualifiedName);
                }
                if (typeName.Name == this.dateID)
                {
                    return typeof(DateTime);
                }
                if (typeName.Name == this.timeID)
                {
                    return typeof(DateTime);
                }
                if (typeName.Name == this.hexBinaryID)
                {
                    return typeof(byte[]);
                }
                if (typeName.Name == this.base64BinaryID)
                {
                    return typeof(byte[]);
                }
                if (typeName.Name == this.unsignedByteID)
                {
                    return typeof(byte);
                }
                if (typeName.Name == this.byteID)
                {
                    return typeof(sbyte);
                }
                if (typeName.Name == this.unsignedShortID)
                {
                    return typeof(ushort);
                }
                if (typeName.Name == this.unsignedIntID)
                {
                    return typeof(uint);
                }
                if (typeName.Name != this.unsignedLongID)
                {
                    throw this.CreateUnknownTypeException(typeName);
                }
                return typeof(ulong);
            }
            if ((typeName.Namespace == this.schemaNs2000ID) || (typeName.Namespace == this.schemaNs1999ID))
            {
                if ((((((typeName.Name == this.stringID) || (typeName.Name == this.anyURIID)) || ((typeName.Name == this.durationID) || (typeName.Name == this.ENTITYID))) || (((typeName.Name == this.ENTITIESID) || (typeName.Name == this.gDayID)) || ((typeName.Name == this.gMonthID) || (typeName.Name == this.gMonthDayID)))) || ((((typeName.Name == this.gYearID) || (typeName.Name == this.gYearMonthID)) || ((typeName.Name == this.IDID) || (typeName.Name == this.IDREFID))) || (((typeName.Name == this.IDREFSID) || (typeName.Name == this.integerID)) || ((typeName.Name == this.languageID) || (typeName.Name == this.NameID))))) || ((((typeName.Name == this.NCNameID) || (typeName.Name == this.NMTOKENID)) || ((typeName.Name == this.NMTOKENSID) || (typeName.Name == this.negativeIntegerID))) || ((((typeName.Name == this.nonPositiveIntegerID) || (typeName.Name == this.nonNegativeIntegerID)) || ((typeName.Name == this.normalizedStringID) || (typeName.Name == this.NOTATIONID))) || ((typeName.Name == this.positiveIntegerID) || (typeName.Name == this.tokenID)))))
                {
                    return typeof(string);
                }
                if (typeName.Name == this.intID)
                {
                    return typeof(int);
                }
                if (typeName.Name == this.booleanID)
                {
                    return typeof(bool);
                }
                if (typeName.Name == this.shortID)
                {
                    return typeof(short);
                }
                if (typeName.Name == this.longID)
                {
                    return typeof(long);
                }
                if (typeName.Name == this.floatID)
                {
                    return typeof(float);
                }
                if (typeName.Name == this.doubleID)
                {
                    return typeof(double);
                }
                if (typeName.Name == this.oldDecimalID)
                {
                    return typeof(decimal);
                }
                if (typeName.Name == this.oldTimeInstantID)
                {
                    return typeof(DateTime);
                }
                if (typeName.Name == this.qnameID)
                {
                    return typeof(XmlQualifiedName);
                }
                if (typeName.Name == this.dateID)
                {
                    return typeof(DateTime);
                }
                if (typeName.Name == this.timeID)
                {
                    return typeof(DateTime);
                }
                if (typeName.Name == this.hexBinaryID)
                {
                    return typeof(byte[]);
                }
                if (typeName.Name == this.byteID)
                {
                    return typeof(sbyte);
                }
                if (typeName.Name == this.unsignedShortID)
                {
                    return typeof(ushort);
                }
                if (typeName.Name == this.unsignedIntID)
                {
                    return typeof(uint);
                }
                if (typeName.Name != this.unsignedLongID)
                {
                    throw this.CreateUnknownTypeException(typeName);
                }
                return typeof(ulong);
            }
            if (typeName.Namespace == this.schemaNonXsdTypesNsID)
            {
                if (typeName.Name == this.charID)
                {
                    return typeof(char);
                }
                if (typeName.Name != this.guidID)
                {
                    throw this.CreateUnknownTypeException(typeName);
                }
                return typeof(Guid);
            }
            if (throwOnUnknown)
            {
                throw this.CreateUnknownTypeException(typeName);
            }
            return null;
        }

        protected object GetTarget(string id)
        {
            object o = (this.targets != null) ? this.targets[id] : null;
            if (o == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidHref", new object[] { id }));
            }
            this.Referenced(o);
            return o;
        }

        protected XmlQualifiedName GetXsiType()
        {
            string attribute = this.r.GetAttribute(this.typeID, this.instanceNsID);
            if (attribute == null)
            {
                attribute = this.r.GetAttribute(this.typeID, this.instanceNs2000ID);
                if (attribute == null)
                {
                    attribute = this.r.GetAttribute(this.typeID, this.instanceNs1999ID);
                    if (attribute == null)
                    {
                        return null;
                    }
                }
            }
            return this.ToXmlQualifiedName(attribute, false);
        }

        private void HandleUnreferencedObjects()
        {
            if (this.targets != null)
            {
                foreach (DictionaryEntry entry in this.targets)
                {
                    if ((this.referencedTargets == null) || !this.referencedTargets.Contains(entry.Value))
                    {
                        this.UnreferencedObject((string) entry.Key, entry.Value);
                    }
                }
            }
            if (this.targetsWithoutIds != null)
            {
                foreach (object obj2 in this.targetsWithoutIds)
                {
                    if ((this.referencedTargets == null) || !this.referencedTargets.Contains(obj2))
                    {
                        this.UnreferencedObject(null, obj2);
                    }
                }
            }
        }

        internal void Init(XmlReader r, XmlDeserializationEvents events, string encodingStyle, TempAssembly tempAssembly)
        {
            this.events = events;
            if (checkDeserializeAdvances)
            {
                this.countingReader = new XmlCountingReader(r);
                this.r = this.countingReader;
            }
            else
            {
                this.r = r;
            }
            this.d = null;
            this.soap12 = encodingStyle == "http://www.w3.org/2003/05/soap-encoding";
            base.Init(tempAssembly);
            this.schemaNsID = r.NameTable.Add("http://www.w3.org/2001/XMLSchema");
            this.schemaNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema");
            this.schemaNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema");
            this.schemaNonXsdTypesNsID = r.NameTable.Add("http://microsoft.com/wsdl/types/");
            this.instanceNsID = r.NameTable.Add("http://www.w3.org/2001/XMLSchema-instance");
            this.instanceNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema-instance");
            this.instanceNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema-instance");
            this.soapNsID = r.NameTable.Add("http://schemas.xmlsoap.org/soap/encoding/");
            this.soap12NsID = r.NameTable.Add("http://www.w3.org/2003/05/soap-encoding");
            this.schemaID = r.NameTable.Add("schema");
            this.wsdlNsID = r.NameTable.Add("http://schemas.xmlsoap.org/wsdl/");
            this.wsdlArrayTypeID = r.NameTable.Add("arrayType");
            this.nullID = r.NameTable.Add("null");
            this.nilID = r.NameTable.Add("nil");
            this.typeID = r.NameTable.Add("type");
            this.arrayTypeID = r.NameTable.Add("arrayType");
            this.itemTypeID = r.NameTable.Add("itemType");
            this.arraySizeID = r.NameTable.Add("arraySize");
            this.arrayID = r.NameTable.Add("Array");
            this.urTypeID = r.NameTable.Add("anyType");
            this.InitIDs();
        }

        protected abstract void InitCallbacks();
        protected abstract void InitIDs();
        private void InitPrimitiveIDs()
        {
            if (this.tokenID == null)
            {
                this.r.NameTable.Add("http://www.w3.org/2001/XMLSchema");
                this.r.NameTable.Add("http://microsoft.com/wsdl/types/");
                this.stringID = this.r.NameTable.Add("string");
                this.intID = this.r.NameTable.Add("int");
                this.booleanID = this.r.NameTable.Add("boolean");
                this.shortID = this.r.NameTable.Add("short");
                this.longID = this.r.NameTable.Add("long");
                this.floatID = this.r.NameTable.Add("float");
                this.doubleID = this.r.NameTable.Add("double");
                this.decimalID = this.r.NameTable.Add("decimal");
                this.dateTimeID = this.r.NameTable.Add("dateTime");
                this.qnameID = this.r.NameTable.Add("QName");
                this.dateID = this.r.NameTable.Add("date");
                this.timeID = this.r.NameTable.Add("time");
                this.hexBinaryID = this.r.NameTable.Add("hexBinary");
                this.base64BinaryID = this.r.NameTable.Add("base64Binary");
                this.unsignedByteID = this.r.NameTable.Add("unsignedByte");
                this.byteID = this.r.NameTable.Add("byte");
                this.unsignedShortID = this.r.NameTable.Add("unsignedShort");
                this.unsignedIntID = this.r.NameTable.Add("unsignedInt");
                this.unsignedLongID = this.r.NameTable.Add("unsignedLong");
                this.oldDecimalID = this.r.NameTable.Add("decimal");
                this.oldTimeInstantID = this.r.NameTable.Add("timeInstant");
                this.charID = this.r.NameTable.Add("char");
                this.guidID = this.r.NameTable.Add("guid");
                this.base64ID = this.r.NameTable.Add("base64");
                this.anyURIID = this.r.NameTable.Add("anyURI");
                this.durationID = this.r.NameTable.Add("duration");
                this.ENTITYID = this.r.NameTable.Add("ENTITY");
                this.ENTITIESID = this.r.NameTable.Add("ENTITIES");
                this.gDayID = this.r.NameTable.Add("gDay");
                this.gMonthID = this.r.NameTable.Add("gMonth");
                this.gMonthDayID = this.r.NameTable.Add("gMonthDay");
                this.gYearID = this.r.NameTable.Add("gYear");
                this.gYearMonthID = this.r.NameTable.Add("gYearMonth");
                this.IDID = this.r.NameTable.Add("ID");
                this.IDREFID = this.r.NameTable.Add("IDREF");
                this.IDREFSID = this.r.NameTable.Add("IDREFS");
                this.integerID = this.r.NameTable.Add("integer");
                this.languageID = this.r.NameTable.Add("language");
                this.NameID = this.r.NameTable.Add("Name");
                this.NCNameID = this.r.NameTable.Add("NCName");
                this.NMTOKENID = this.r.NameTable.Add("NMTOKEN");
                this.NMTOKENSID = this.r.NameTable.Add("NMTOKENS");
                this.negativeIntegerID = this.r.NameTable.Add("negativeInteger");
                this.nonNegativeIntegerID = this.r.NameTable.Add("nonNegativeInteger");
                this.nonPositiveIntegerID = this.r.NameTable.Add("nonPositiveInteger");
                this.normalizedStringID = this.r.NameTable.Add("normalizedString");
                this.NOTATIONID = this.r.NameTable.Add("NOTATION");
                this.positiveIntegerID = this.r.NameTable.Add("positiveInteger");
                this.tokenID = this.r.NameTable.Add("token");
            }
        }

        private bool IsPrimitiveNamespace(string ns)
        {
            if ((((ns != this.schemaNsID) && (ns != this.schemaNonXsdTypesNsID)) && ((ns != this.soapNsID) && (ns != this.soap12NsID))) && (ns != this.schemaNs2000ID))
            {
                return (ns == this.schemaNs1999ID);
            }
            return true;
        }

        protected bool IsXmlnsAttribute(string name)
        {
            if (!name.StartsWith("xmlns", StringComparison.Ordinal))
            {
                return false;
            }
            return ((name.Length == 5) || (name[5] == ':'));
        }

        private SoapArrayInfo ParseArrayType(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(Res.GetString("XmlMissingArrayType", new object[] { this.CurrentTag() }));
            }
            if (value.Length == 0)
            {
                throw new ArgumentException(Res.GetString("XmlEmptyArrayType", new object[] { this.CurrentTag() }), "value");
            }
            char[] chArray = value.ToCharArray();
            int length = chArray.Length;
            SoapArrayInfo info = new SoapArrayInfo();
            int index = length - 1;
            if (chArray[index] != ']')
            {
                throw new ArgumentException(Res.GetString("XmlInvalidArraySyntax"), "value");
            }
            index--;
            while ((index != -1) && (chArray[index] != '['))
            {
                if (chArray[index] == ',')
                {
                    throw new ArgumentException(Res.GetString("XmlInvalidArrayDimentions", new object[] { this.CurrentTag() }), "value");
                }
                index--;
            }
            if (index == -1)
            {
                throw new ArgumentException(Res.GetString("XmlMismatchedArrayBrackets"), "value");
            }
            int num3 = (length - index) - 2;
            if (num3 > 0)
            {
                string s = new string(chArray, index + 1, num3);
                try
                {
                    info.length = int.Parse(s, CultureInfo.InvariantCulture);
                    goto Label_0163;
                }
                catch (Exception exception)
                {
                    if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                    {
                        throw;
                    }
                    throw new ArgumentException(Res.GetString("XmlInvalidArrayLength", new object[] { s }), "value");
                }
            }
            info.length = -1;
        Label_0163:
            index--;
            info.jaggedDimensions = 0;
            while ((index != -1) && (chArray[index] == ']'))
            {
                index--;
                if (index < 0)
                {
                    throw new ArgumentException(Res.GetString("XmlMismatchedArrayBrackets"), "value");
                }
                if (chArray[index] == ',')
                {
                    throw new ArgumentException(Res.GetString("XmlInvalidArrayDimentions", new object[] { this.CurrentTag() }), "value");
                }
                if (chArray[index] != '[')
                {
                    throw new ArgumentException(Res.GetString("XmlInvalidArraySyntax"), "value");
                }
                index--;
                info.jaggedDimensions++;
            }
            info.dimensions = 1;
            info.qname = new string(chArray, 0, index + 1);
            return info;
        }

        private SoapArrayInfo ParseSoap12ArrayType(string itemType, string arraySize)
        {
            string[] strArray;
            SoapArrayInfo info = new SoapArrayInfo();
            if ((itemType != null) && (itemType.Length > 0))
            {
                info.qname = itemType;
            }
            else
            {
                info.qname = "";
            }
            if ((arraySize != null) && (arraySize.Length > 0))
            {
                strArray = arraySize.Split(null);
            }
            else
            {
                strArray = new string[0];
            }
            info.dimensions = 0;
            info.length = -1;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].Length > 0)
                {
                    if (strArray[i] == "*")
                    {
                        info.dimensions++;
                    }
                    else
                    {
                        try
                        {
                            info.length = int.Parse(strArray[i], CultureInfo.InvariantCulture);
                            info.dimensions++;
                        }
                        catch (Exception exception)
                        {
                            if (((exception is ThreadAbortException) || (exception is StackOverflowException)) || (exception is OutOfMemoryException))
                            {
                                throw;
                            }
                            throw new ArgumentException(Res.GetString("XmlInvalidArrayLength", new object[] { strArray[i] }), "value");
                        }
                    }
                }
            }
            if (info.dimensions == 0)
            {
                info.dimensions = 1;
            }
            return info;
        }

        protected void ParseWsdlArrayType(XmlAttribute attr)
        {
            if ((attr.LocalName == this.wsdlArrayTypeID) && (attr.NamespaceURI == this.wsdlNsID))
            {
                int length = attr.Value.LastIndexOf(':');
                if (length < 0)
                {
                    attr.Value = this.r.LookupNamespace("") + ":" + attr.Value;
                }
                else
                {
                    attr.Value = this.r.LookupNamespace(attr.Value.Substring(0, length)) + ":" + attr.Value.Substring(length + 1);
                }
            }
        }

        private object ReadArray(string typeName, string typeNs)
        {
            SoapArrayInfo info;
            Type arrayElementType = null;
            XmlQualifiedName name;
            bool isPrimitive;
            if (this.soap12)
            {
                string attribute = this.r.GetAttribute(this.itemTypeID, this.soap12NsID);
                string arraySize = this.r.GetAttribute(this.arraySizeID, this.soap12NsID);
                Type type2 = (Type) this.types[new XmlQualifiedName(typeName, typeNs)];
                if (((attribute == null) && (arraySize == null)) && ((type2 == null) || !type2.IsArray))
                {
                    return null;
                }
                info = this.ParseSoap12ArrayType(attribute, arraySize);
                if (type2 != null)
                {
                    arrayElementType = TypeScope.GetArrayElementType(type2, null);
                }
            }
            else
            {
                string str3 = this.r.GetAttribute(this.arrayTypeID, this.soapNsID);
                if (str3 == null)
                {
                    return null;
                }
                info = this.ParseArrayType(str3);
            }
            if (info.dimensions != 1)
            {
                throw new InvalidOperationException(Res.GetString("XmlInvalidArrayDimentions", new object[] { this.CurrentTag() }));
            }
            Type primitiveType = null;
            XmlQualifiedName name2 = new XmlQualifiedName(this.urTypeID, this.schemaNsID);
            if (info.qname.Length > 0)
            {
                name = this.ToXmlQualifiedName(info.qname, false);
                primitiveType = (Type) this.types[name];
            }
            else
            {
                name = name2;
            }
            if (this.soap12 && (primitiveType == typeof(object)))
            {
                primitiveType = null;
            }
            if (primitiveType == null)
            {
                if (!this.soap12)
                {
                    primitiveType = this.GetPrimitiveType(name, true);
                    isPrimitive = true;
                }
                else
                {
                    if (name != name2)
                    {
                        primitiveType = this.GetPrimitiveType(name, false);
                    }
                    if (primitiveType != null)
                    {
                        isPrimitive = true;
                    }
                    else if (arrayElementType == null)
                    {
                        primitiveType = typeof(object);
                        isPrimitive = false;
                    }
                    else
                    {
                        primitiveType = arrayElementType;
                        XmlQualifiedName primitiveTypeNameInternal = (XmlQualifiedName) this.typesReverse[primitiveType];
                        if (primitiveTypeNameInternal == null)
                        {
                            primitiveTypeNameInternal = XmlSerializationWriter.GetPrimitiveTypeNameInternal(primitiveType);
                            isPrimitive = true;
                        }
                        else
                        {
                            isPrimitive = primitiveType.IsPrimitive;
                        }
                        if (primitiveTypeNameInternal != null)
                        {
                            name = primitiveTypeNameInternal;
                        }
                    }
                }
            }
            else
            {
                isPrimitive = primitiveType.IsPrimitive;
            }
            if (!this.soap12 && (info.jaggedDimensions > 0))
            {
                for (int i = 0; i < info.jaggedDimensions; i++)
                {
                    primitiveType = primitiveType.MakeArrayType();
                }
            }
            if (this.r.IsEmptyElement)
            {
                this.r.Skip();
                return Array.CreateInstance(primitiveType, 0);
            }
            this.r.ReadStartElement();
            this.r.MoveToContent();
            int index = 0;
            Array a = null;
            if (primitiveType.IsValueType)
            {
                if (!isPrimitive && !primitiveType.IsEnum)
                {
                    throw new NotSupportedException(Res.GetString("XmlRpcArrayOfValueTypes", new object[] { primitiveType.FullName }));
                }
                int whileIterations = 0;
                int readerCount = this.ReaderCount;
                while (this.r.NodeType != XmlNodeType.EndElement)
                {
                    a = this.EnsureArrayIndex(a, index, primitiveType);
                    a.SetValue(this.ReadReferencedElement(name.Name, name.Namespace), index);
                    index++;
                    this.r.MoveToContent();
                    this.CheckReaderCount(ref whileIterations, ref readerCount);
                }
                a = this.ShrinkArray(a, index, primitiveType, false);
            }
            else
            {
                string[] strArray = null;
                int num5 = 0;
                int num6 = 0;
                int num7 = this.ReaderCount;
                while (this.r.NodeType != XmlNodeType.EndElement)
                {
                    string localName;
                    string namespaceURI;
                    a = this.EnsureArrayIndex(a, index, primitiveType);
                    strArray = (string[]) this.EnsureArrayIndex(strArray, num5, typeof(string));
                    if (this.r.NamespaceURI.Length != 0)
                    {
                        localName = this.r.LocalName;
                        if (this.r.NamespaceURI == this.soapNsID)
                        {
                            namespaceURI = "http://www.w3.org/2001/XMLSchema";
                        }
                        else
                        {
                            namespaceURI = this.r.NamespaceURI;
                        }
                    }
                    else
                    {
                        localName = name.Name;
                        namespaceURI = name.Namespace;
                    }
                    a.SetValue(this.ReadReferencingElement(localName, namespaceURI, out strArray[num5]), index);
                    index++;
                    num5++;
                    this.r.MoveToContent();
                    this.CheckReaderCount(ref num6, ref num7);
                }
                if (this.soap12 && (primitiveType == typeof(object)))
                {
                    Type c = null;
                    for (int j = 0; j < index; j++)
                    {
                        object obj2 = a.GetValue(j);
                        if (obj2 != null)
                        {
                            Type type = obj2.GetType();
                            if (type.IsValueType)
                            {
                                c = null;
                                break;
                            }
                            if ((c == null) || type.IsAssignableFrom(c))
                            {
                                c = type;
                            }
                            else if (!c.IsAssignableFrom(type))
                            {
                                c = null;
                                break;
                            }
                        }
                    }
                    if (c != null)
                    {
                        primitiveType = c;
                    }
                }
                strArray = (string[]) this.ShrinkArray(strArray, num5, typeof(string), false);
                a = this.ShrinkArray(a, index, primitiveType, false);
                Fixup fixup = new Fixup(a, new XmlSerializationFixupCallback(this.FixupArrayRefs), strArray);
                this.AddFixup(fixup);
            }
            this.ReadEndElement();
            return a;
        }

        private byte[] ReadByteArray(bool isBase64)
        {
            ArrayList list = new ArrayList();
            int count = 0x400;
            int num2 = -1;
            int index = 0;
            int num4 = 0;
            byte[] buffer = new byte[count];
            list.Add(buffer);
            while (num2 != 0)
            {
                if (index == buffer.Length)
                {
                    count = Math.Min(count * 2, 0x10000);
                    buffer = new byte[count];
                    index = 0;
                    list.Add(buffer);
                }
                if (isBase64)
                {
                    num2 = this.r.ReadElementContentAsBase64(buffer, index, buffer.Length - index);
                }
                else
                {
                    num2 = this.r.ReadElementContentAsBinHex(buffer, index, buffer.Length - index);
                }
                index += num2;
                num4 += num2;
            }
            byte[] dst = new byte[num4];
            index = 0;
            foreach (byte[] buffer3 in list)
            {
                count = Math.Min(buffer3.Length, num4);
                if (count > 0)
                {
                    Buffer.BlockCopy(buffer3, 0, dst, index, count);
                    index += count;
                    num4 -= count;
                }
            }
            list.Clear();
            return dst;
        }

        protected XmlQualifiedName ReadElementQualifiedName()
        {
            if (this.r.IsEmptyElement)
            {
                XmlQualifiedName name = new XmlQualifiedName(string.Empty, this.r.LookupNamespace(""));
                this.r.Skip();
                return name;
            }
            XmlQualifiedName name2 = this.ToXmlQualifiedName(this.CollapseWhitespace(this.r.ReadString()));
            this.r.ReadEndElement();
            return name2;
        }

        protected void ReadEndElement()
        {
            while (this.r.NodeType == XmlNodeType.Whitespace)
            {
                this.r.Skip();
            }
            if (this.r.NodeType == XmlNodeType.None)
            {
                this.r.Skip();
            }
            else
            {
                this.r.ReadEndElement();
            }
        }

        protected bool ReadNull()
        {
            if (!this.GetNullAttr())
            {
                return false;
            }
            if (this.r.IsEmptyElement)
            {
                this.r.Skip();
                return true;
            }
            this.r.ReadStartElement();
            int whileIterations = 0;
            int readerCount = this.ReaderCount;
            while (this.r.NodeType != XmlNodeType.EndElement)
            {
                this.UnknownNode(null);
                this.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            this.ReadEndElement();
            return true;
        }

        protected XmlQualifiedName ReadNullableQualifiedName()
        {
            if (this.ReadNull())
            {
                return null;
            }
            return this.ReadElementQualifiedName();
        }

        protected string ReadNullableString()
        {
            if (this.ReadNull())
            {
                return null;
            }
            return this.r.ReadElementString();
        }

        protected bool ReadReference(out string fixupReference)
        {
            string str = this.soap12 ? this.r.GetAttribute("ref", "http://www.w3.org/2003/05/soap-encoding") : this.r.GetAttribute("href");
            if (str == null)
            {
                fixupReference = null;
                return false;
            }
            if (!this.soap12)
            {
                if (!str.StartsWith("#", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(Res.GetString("XmlMissingHref", new object[] { str }));
                }
                fixupReference = str.Substring(1);
            }
            else
            {
                fixupReference = str;
            }
            if (this.r.IsEmptyElement)
            {
                this.r.Skip();
            }
            else
            {
                this.r.ReadStartElement();
                this.ReadEndElement();
            }
            return true;
        }

        protected object ReadReferencedElement()
        {
            return this.ReadReferencedElement(null, null);
        }

        protected object ReadReferencedElement(string name, string ns)
        {
            string str;
            return this.ReadReferencingElement(name, ns, out str);
        }

        protected void ReadReferencedElements()
        {
            this.r.MoveToContent();
            int whileIterations = 0;
            int readerCount = this.ReaderCount;
            while ((this.r.NodeType != XmlNodeType.EndElement) && (this.r.NodeType != XmlNodeType.None))
            {
                string str;
                this.ReadReferencingElement(null, null, true, out str);
                this.r.MoveToContent();
                this.CheckReaderCount(ref whileIterations, ref readerCount);
            }
            this.DoFixups();
            this.HandleUnreferencedObjects();
        }

        protected object ReadReferencingElement(out string fixupReference)
        {
            return this.ReadReferencingElement(null, null, out fixupReference);
        }

        protected object ReadReferencingElement(string name, string ns, out string fixupReference)
        {
            return this.ReadReferencingElement(name, ns, false, out fixupReference);
        }

        protected object ReadReferencingElement(string name, string ns, bool elementCanBeType, out string fixupReference)
        {
            object o = null;
            if (this.callbacks == null)
            {
                this.callbacks = new Hashtable();
                this.types = new Hashtable();
                XmlQualifiedName key = new XmlQualifiedName(this.urTypeID, this.r.NameTable.Add("http://www.w3.org/2001/XMLSchema"));
                this.types.Add(key, typeof(object));
                this.typesReverse = new Hashtable();
                this.typesReverse.Add(typeof(object), key);
                this.InitCallbacks();
            }
            this.r.MoveToContent();
            if (this.ReadReference(out fixupReference))
            {
                return null;
            }
            if (this.ReadNull())
            {
                return null;
            }
            string id = this.soap12 ? this.r.GetAttribute("id", "http://www.w3.org/2003/05/soap-encoding") : this.r.GetAttribute("id", null);
            o = this.ReadArray(name, ns);
            if (o == null)
            {
                XmlQualifiedName xsiType = this.GetXsiType();
                if (xsiType == null)
                {
                    if (name == null)
                    {
                        xsiType = new XmlQualifiedName(this.r.NameTable.Add(this.r.LocalName), this.r.NameTable.Add(this.r.NamespaceURI));
                    }
                    else
                    {
                        xsiType = new XmlQualifiedName(this.r.NameTable.Add(name), this.r.NameTable.Add(ns));
                    }
                }
                XmlSerializationReadCallback callback = (XmlSerializationReadCallback) this.callbacks[xsiType];
                if (callback != null)
                {
                    o = callback();
                }
                else
                {
                    o = this.ReadTypedPrimitive(xsiType, elementCanBeType);
                }
            }
            this.AddTarget(id, o);
            return o;
        }

        protected IXmlSerializable ReadSerializable(IXmlSerializable serializable)
        {
            return this.ReadSerializable(serializable, false);
        }

        protected IXmlSerializable ReadSerializable(IXmlSerializable serializable, bool wrappedAny)
        {
            string localName = null;
            string namespaceURI = null;
            if (wrappedAny)
            {
                localName = this.r.LocalName;
                namespaceURI = this.r.NamespaceURI;
                this.r.Read();
                this.r.MoveToContent();
            }
            serializable.ReadXml(this.r);
            if (wrappedAny)
            {
                while (this.r.NodeType == XmlNodeType.Whitespace)
                {
                    this.r.Skip();
                }
                if (this.r.NodeType == XmlNodeType.None)
                {
                    this.r.Skip();
                }
                if (((this.r.NodeType == XmlNodeType.EndElement) && (this.r.LocalName == localName)) && (this.r.NamespaceURI == namespaceURI))
                {
                    this.Reader.Read();
                }
            }
            return serializable;
        }

        protected string ReadString(string value)
        {
            return this.ReadString(value, false);
        }

        protected string ReadString(string value, bool trim)
        {
            string str = this.r.ReadString();
            if ((str != null) && trim)
            {
                str = str.Trim();
            }
            if ((value != null) && (value.Length != 0))
            {
                return (value + str);
            }
            return str;
        }

        private string ReadStringValue()
        {
            if (this.r.IsEmptyElement)
            {
                this.r.Skip();
                return string.Empty;
            }
            this.r.ReadStartElement();
            string str = this.r.ReadString();
            this.ReadEndElement();
            return str;
        }

        protected object ReadTypedNull(XmlQualifiedName type)
        {
            this.InitPrimitiveIDs();
            if (this.IsPrimitiveNamespace(type.Namespace) && (type.Name != this.urTypeID))
            {
                if (((type.Namespace == this.schemaNsID) || (type.Namespace == this.soapNsID)) || (type.Namespace == this.soap12NsID))
                {
                    if ((((((type.Name != this.stringID) && (type.Name != this.anyURIID)) && ((type.Name != this.durationID) && (type.Name != this.ENTITYID))) && (((type.Name != this.ENTITIESID) && (type.Name != this.gDayID)) && ((type.Name != this.gMonthID) && (type.Name != this.gMonthDayID)))) && ((((type.Name != this.gYearID) && (type.Name != this.gYearMonthID)) && ((type.Name != this.IDID) && (type.Name != this.IDREFID))) && (((type.Name != this.IDREFSID) && (type.Name != this.integerID)) && ((type.Name != this.languageID) && (type.Name != this.NameID))))) && ((((type.Name != this.NCNameID) && (type.Name != this.NMTOKENID)) && ((type.Name != this.NMTOKENSID) && (type.Name != this.negativeIntegerID))) && ((((type.Name != this.nonPositiveIntegerID) && (type.Name != this.nonNegativeIntegerID)) && ((type.Name != this.normalizedStringID) && (type.Name != this.NOTATIONID))) && ((type.Name != this.positiveIntegerID) && (type.Name != this.tokenID)))))
                    {
                        if (type.Name == this.intID)
                        {
                            return null;
                        }
                        if (type.Name == this.booleanID)
                        {
                            return null;
                        }
                        if (type.Name == this.shortID)
                        {
                            return null;
                        }
                        if (type.Name == this.longID)
                        {
                            return null;
                        }
                        if (type.Name == this.floatID)
                        {
                            return null;
                        }
                        if (type.Name == this.doubleID)
                        {
                            return null;
                        }
                        if (type.Name == this.decimalID)
                        {
                            return null;
                        }
                        if (type.Name == this.dateTimeID)
                        {
                            return null;
                        }
                        if (type.Name == this.qnameID)
                        {
                            return null;
                        }
                        if (type.Name == this.dateID)
                        {
                            return null;
                        }
                        if (type.Name == this.timeID)
                        {
                            return null;
                        }
                        if (type.Name == this.unsignedByteID)
                        {
                            return null;
                        }
                        if (type.Name == this.byteID)
                        {
                            return null;
                        }
                        if (type.Name == this.unsignedShortID)
                        {
                            return null;
                        }
                        if (type.Name == this.unsignedIntID)
                        {
                            return null;
                        }
                        if (type.Name == this.unsignedLongID)
                        {
                            return null;
                        }
                        if (type.Name == this.hexBinaryID)
                        {
                            return null;
                        }
                        if (type.Name == this.base64BinaryID)
                        {
                            return null;
                        }
                        if ((type.Name == this.base64ID) && ((type.Namespace == this.soapNsID) || (type.Namespace == this.soap12NsID)))
                        {
                            return null;
                        }
                    }
                    return null;
                }
                if (type.Namespace == this.schemaNonXsdTypesNsID)
                {
                    if ((type.Name != this.charID) && (type.Name == this.guidID))
                    {
                        return null;
                    }
                    return null;
                }
            }
            return null;
        }

        protected object ReadTypedPrimitive(XmlQualifiedName type)
        {
            return this.ReadTypedPrimitive(type, false);
        }

        private object ReadTypedPrimitive(XmlQualifiedName type, bool elementCanBeType)
        {
            this.InitPrimitiveIDs();
            if (this.IsPrimitiveNamespace(type.Namespace) && (type.Name != this.urTypeID))
            {
                if (((type.Namespace == this.schemaNsID) || (type.Namespace == this.soapNsID)) || (type.Namespace == this.soap12NsID))
                {
                    if ((type.Name == this.stringID) || (type.Name == this.normalizedStringID))
                    {
                        return this.ReadStringValue();
                    }
                    if ((((((type.Name == this.anyURIID) || (type.Name == this.durationID)) || ((type.Name == this.ENTITYID) || (type.Name == this.ENTITIESID))) || (((type.Name == this.gDayID) || (type.Name == this.gMonthID)) || ((type.Name == this.gMonthDayID) || (type.Name == this.gYearID)))) || ((((type.Name == this.gYearMonthID) || (type.Name == this.IDID)) || ((type.Name == this.IDREFID) || (type.Name == this.IDREFSID))) || (((type.Name == this.integerID) || (type.Name == this.languageID)) || ((type.Name == this.NameID) || (type.Name == this.NCNameID))))) || ((((type.Name == this.NMTOKENID) || (type.Name == this.NMTOKENSID)) || ((type.Name == this.negativeIntegerID) || (type.Name == this.nonPositiveIntegerID))) || (((type.Name == this.nonNegativeIntegerID) || (type.Name == this.NOTATIONID)) || ((type.Name == this.positiveIntegerID) || (type.Name == this.tokenID)))))
                    {
                        return this.CollapseWhitespace(this.ReadStringValue());
                    }
                    if (type.Name == this.intID)
                    {
                        return XmlConvert.ToInt32(this.ReadStringValue());
                    }
                    if (type.Name == this.booleanID)
                    {
                        return XmlConvert.ToBoolean(this.ReadStringValue());
                    }
                    if (type.Name == this.shortID)
                    {
                        return XmlConvert.ToInt16(this.ReadStringValue());
                    }
                    if (type.Name == this.longID)
                    {
                        return XmlConvert.ToInt64(this.ReadStringValue());
                    }
                    if (type.Name == this.floatID)
                    {
                        return XmlConvert.ToSingle(this.ReadStringValue());
                    }
                    if (type.Name == this.doubleID)
                    {
                        return XmlConvert.ToDouble(this.ReadStringValue());
                    }
                    if (type.Name == this.decimalID)
                    {
                        return XmlConvert.ToDecimal(this.ReadStringValue());
                    }
                    if (type.Name == this.dateTimeID)
                    {
                        return ToDateTime(this.ReadStringValue());
                    }
                    if (type.Name == this.qnameID)
                    {
                        return this.ReadXmlQualifiedName();
                    }
                    if (type.Name == this.dateID)
                    {
                        return ToDate(this.ReadStringValue());
                    }
                    if (type.Name == this.timeID)
                    {
                        return ToTime(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedByteID)
                    {
                        return XmlConvert.ToByte(this.ReadStringValue());
                    }
                    if (type.Name == this.byteID)
                    {
                        return XmlConvert.ToSByte(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedShortID)
                    {
                        return XmlConvert.ToUInt16(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedIntID)
                    {
                        return XmlConvert.ToUInt32(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedLongID)
                    {
                        return XmlConvert.ToUInt64(this.ReadStringValue());
                    }
                    if (type.Name == this.hexBinaryID)
                    {
                        return this.ToByteArrayHex(false);
                    }
                    if (type.Name == this.base64BinaryID)
                    {
                        return this.ToByteArrayBase64(false);
                    }
                    if ((type.Name == this.base64ID) && ((type.Namespace == this.soapNsID) || (type.Namespace == this.soap12NsID)))
                    {
                        return this.ToByteArrayBase64(false);
                    }
                    return this.ReadXmlNodes(elementCanBeType);
                }
                if ((type.Namespace == this.schemaNs2000ID) || (type.Namespace == this.schemaNs1999ID))
                {
                    if ((type.Name == this.stringID) || (type.Name == this.normalizedStringID))
                    {
                        return this.ReadStringValue();
                    }
                    if ((((((type.Name == this.anyURIID) || (type.Name == this.anyURIID)) || ((type.Name == this.durationID) || (type.Name == this.ENTITYID))) || (((type.Name == this.ENTITIESID) || (type.Name == this.gDayID)) || ((type.Name == this.gMonthID) || (type.Name == this.gMonthDayID)))) || ((((type.Name == this.gYearID) || (type.Name == this.gYearMonthID)) || ((type.Name == this.IDID) || (type.Name == this.IDREFID))) || (((type.Name == this.IDREFSID) || (type.Name == this.integerID)) || ((type.Name == this.languageID) || (type.Name == this.NameID))))) || ((((type.Name == this.NCNameID) || (type.Name == this.NMTOKENID)) || ((type.Name == this.NMTOKENSID) || (type.Name == this.negativeIntegerID))) || (((type.Name == this.nonPositiveIntegerID) || (type.Name == this.nonNegativeIntegerID)) || (((type.Name == this.NOTATIONID) || (type.Name == this.positiveIntegerID)) || (type.Name == this.tokenID)))))
                    {
                        return this.CollapseWhitespace(this.ReadStringValue());
                    }
                    if (type.Name == this.intID)
                    {
                        return XmlConvert.ToInt32(this.ReadStringValue());
                    }
                    if (type.Name == this.booleanID)
                    {
                        return XmlConvert.ToBoolean(this.ReadStringValue());
                    }
                    if (type.Name == this.shortID)
                    {
                        return XmlConvert.ToInt16(this.ReadStringValue());
                    }
                    if (type.Name == this.longID)
                    {
                        return XmlConvert.ToInt64(this.ReadStringValue());
                    }
                    if (type.Name == this.floatID)
                    {
                        return XmlConvert.ToSingle(this.ReadStringValue());
                    }
                    if (type.Name == this.doubleID)
                    {
                        return XmlConvert.ToDouble(this.ReadStringValue());
                    }
                    if (type.Name == this.oldDecimalID)
                    {
                        return XmlConvert.ToDecimal(this.ReadStringValue());
                    }
                    if (type.Name == this.oldTimeInstantID)
                    {
                        return ToDateTime(this.ReadStringValue());
                    }
                    if (type.Name == this.qnameID)
                    {
                        return this.ReadXmlQualifiedName();
                    }
                    if (type.Name == this.dateID)
                    {
                        return ToDate(this.ReadStringValue());
                    }
                    if (type.Name == this.timeID)
                    {
                        return ToTime(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedByteID)
                    {
                        return XmlConvert.ToByte(this.ReadStringValue());
                    }
                    if (type.Name == this.byteID)
                    {
                        return XmlConvert.ToSByte(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedShortID)
                    {
                        return XmlConvert.ToUInt16(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedIntID)
                    {
                        return XmlConvert.ToUInt32(this.ReadStringValue());
                    }
                    if (type.Name == this.unsignedLongID)
                    {
                        return XmlConvert.ToUInt64(this.ReadStringValue());
                    }
                    return this.ReadXmlNodes(elementCanBeType);
                }
                if (type.Namespace == this.schemaNonXsdTypesNsID)
                {
                    if (type.Name == this.charID)
                    {
                        return ToChar(this.ReadStringValue());
                    }
                    if (type.Name == this.guidID)
                    {
                        return new Guid(this.CollapseWhitespace(this.ReadStringValue()));
                    }
                    return this.ReadXmlNodes(elementCanBeType);
                }
            }
            return this.ReadXmlNodes(elementCanBeType);
        }

        protected XmlDocument ReadXmlDocument(bool wrapped)
        {
            XmlNode node = this.ReadXmlNode(wrapped);
            if (node == null)
            {
                return null;
            }
            XmlDocument document = new XmlDocument();
            document.AppendChild(document.ImportNode(node, true));
            return document;
        }

        protected XmlNode ReadXmlNode(bool wrapped)
        {
            XmlNode node = null;
            if (wrapped)
            {
                if (this.ReadNull())
                {
                    return null;
                }
                this.r.ReadStartElement();
                this.r.MoveToContent();
                if (this.r.NodeType != XmlNodeType.EndElement)
                {
                    node = this.Document.ReadNode(this.r);
                }
                int whileIterations = 0;
                int readerCount = this.ReaderCount;
                while (this.r.NodeType != XmlNodeType.EndElement)
                {
                    this.UnknownNode(null);
                    this.CheckReaderCount(ref whileIterations, ref readerCount);
                }
                this.r.ReadEndElement();
                return node;
            }
            return this.Document.ReadNode(this.r);
        }

        private object ReadXmlNodes(bool elementCanBeType)
        {
            ArrayList list = new ArrayList();
            string localName = this.Reader.LocalName;
            string namespaceURI = this.Reader.NamespaceURI;
            string name = this.Reader.Name;
            string str4 = null;
            string str5 = null;
            int num = 0;
            int lineNumber = -1;
            int linePosition = -1;
            XmlNode unknownNode = null;
            if (this.Reader.NodeType == XmlNodeType.Attribute)
            {
                XmlAttribute attribute = this.Document.CreateAttribute(name, namespaceURI);
                attribute.Value = this.Reader.Value;
                unknownNode = attribute;
            }
            else
            {
                unknownNode = this.Document.CreateElement(name, namespaceURI);
            }
            this.GetCurrentPosition(out lineNumber, out linePosition);
            XmlElement element = unknownNode as XmlElement;
            while (this.Reader.MoveToNextAttribute())
            {
                if (this.IsXmlnsAttribute(this.Reader.Name) || ((this.Reader.Name == "id") && (!this.soap12 || (this.Reader.NamespaceURI == "http://www.w3.org/2003/05/soap-encoding"))))
                {
                    num++;
                }
                if ((this.Reader.LocalName == this.typeID) && (((this.Reader.NamespaceURI == this.instanceNsID) || (this.Reader.NamespaceURI == this.instanceNs2000ID)) || (this.Reader.NamespaceURI == this.instanceNs1999ID)))
                {
                    string str6 = this.Reader.Value;
                    int length = str6.LastIndexOf(':');
                    str4 = (length >= 0) ? str6.Substring(length + 1) : str6;
                    str5 = this.Reader.LookupNamespace((length >= 0) ? str6.Substring(0, length) : "");
                }
                XmlAttribute attribute2 = (XmlAttribute) this.Document.ReadNode(this.r);
                list.Add(attribute2);
                if (element != null)
                {
                    element.SetAttributeNode(attribute2);
                }
            }
            if (elementCanBeType && (str4 == null))
            {
                str4 = localName;
                str5 = namespaceURI;
                XmlAttribute attribute3 = this.Document.CreateAttribute(this.typeID, this.instanceNsID);
                attribute3.Value = name;
                list.Add(attribute3);
            }
            if ((str4 == "anyType") && (((str5 == this.schemaNsID) || (str5 == this.schemaNs1999ID)) || (str5 == this.schemaNs2000ID)))
            {
                num++;
            }
            this.Reader.MoveToElement();
            if (this.Reader.IsEmptyElement)
            {
                this.Reader.Skip();
            }
            else
            {
                this.Reader.ReadStartElement();
                this.Reader.MoveToContent();
                int whileIterations = 0;
                int readerCount = this.ReaderCount;
                while (this.Reader.NodeType != XmlNodeType.EndElement)
                {
                    XmlNode node2 = this.Document.ReadNode(this.r);
                    list.Add(node2);
                    if (element != null)
                    {
                        element.AppendChild(node2);
                    }
                    this.Reader.MoveToContent();
                    this.CheckReaderCount(ref whileIterations, ref readerCount);
                }
                this.ReadEndElement();
            }
            if (list.Count <= num)
            {
                return new object();
            }
            XmlNode[] nodeArray = (XmlNode[]) list.ToArray(typeof(XmlNode));
            this.UnknownNode(unknownNode, null, null);
            return nodeArray;
        }

        private XmlQualifiedName ReadXmlQualifiedName()
        {
            string str;
            bool flag = false;
            if (this.r.IsEmptyElement)
            {
                str = string.Empty;
                flag = true;
            }
            else
            {
                this.r.ReadStartElement();
                str = this.r.ReadString();
            }
            XmlQualifiedName name = this.ToXmlQualifiedName(str);
            if (flag)
            {
                this.r.Skip();
                return name;
            }
            this.ReadEndElement();
            return name;
        }

        protected void Referenced(object o)
        {
            if (o != null)
            {
                if (this.referencedTargets == null)
                {
                    this.referencedTargets = new Hashtable();
                }
                this.referencedTargets[o] = o;
            }
        }

        protected static Assembly ResolveDynamicAssembly(string assemblyFullName)
        {
            return DynamicAssemblies.Get(assemblyFullName);
        }

        protected Array ShrinkArray(Array a, int length, Type elementType, bool isNullable)
        {
            if (a == null)
            {
                if (isNullable)
                {
                    return null;
                }
                return Array.CreateInstance(elementType, 0);
            }
            if (a.Length == length)
            {
                return a;
            }
            Array destinationArray = Array.CreateInstance(elementType, length);
            Array.Copy(a, destinationArray, length);
            return destinationArray;
        }

        protected byte[] ToByteArrayBase64(bool isNull)
        {
            if (isNull)
            {
                return null;
            }
            return this.ReadByteArray(true);
        }

        protected static byte[] ToByteArrayBase64(string value)
        {
            return XmlCustomFormatter.ToByteArrayBase64(value);
        }

        protected byte[] ToByteArrayHex(bool isNull)
        {
            if (isNull)
            {
                return null;
            }
            return this.ReadByteArray(false);
        }

        protected static byte[] ToByteArrayHex(string value)
        {
            return XmlCustomFormatter.ToByteArrayHex(value);
        }

        protected static char ToChar(string value)
        {
            return XmlCustomFormatter.ToChar(value);
        }

        protected static DateTime ToDate(string value)
        {
            return XmlCustomFormatter.ToDate(value);
        }

        protected static DateTime ToDateTime(string value)
        {
            return XmlCustomFormatter.ToDateTime(value);
        }

        protected static long ToEnum(string value, Hashtable h, string typeName)
        {
            return XmlCustomFormatter.ToEnum(value, h, typeName, true);
        }

        protected static DateTime ToTime(string value)
        {
            return XmlCustomFormatter.ToTime(value);
        }

        protected static string ToXmlName(string value)
        {
            return XmlCustomFormatter.ToXmlName(value);
        }

        protected static string ToXmlNCName(string value)
        {
            return XmlCustomFormatter.ToXmlNCName(value);
        }

        protected static string ToXmlNmToken(string value)
        {
            return XmlCustomFormatter.ToXmlNmToken(value);
        }

        protected static string ToXmlNmTokens(string value)
        {
            return XmlCustomFormatter.ToXmlNmTokens(value);
        }

        protected XmlQualifiedName ToXmlQualifiedName(string value)
        {
            return this.ToXmlQualifiedName(value, this.DecodeName);
        }

        internal XmlQualifiedName ToXmlQualifiedName(string value, bool decodeName)
        {
            int length = (value == null) ? -1 : value.LastIndexOf(':');
            string name = (length < 0) ? null : value.Substring(0, length);
            string str2 = value.Substring(length + 1);
            if (decodeName)
            {
                name = XmlConvert.DecodeName(name);
                str2 = XmlConvert.DecodeName(str2);
            }
            if ((name == null) || (name.Length == 0))
            {
                return new XmlQualifiedName(this.r.NameTable.Add(value), this.r.LookupNamespace(string.Empty));
            }
            string ns = this.r.LookupNamespace(name);
            if (ns == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlUndefinedAlias", new object[] { name }));
            }
            return new XmlQualifiedName(this.r.NameTable.Add(str2), ns);
        }

        protected void UnknownAttribute(object o, XmlAttribute attr)
        {
            this.UnknownAttribute(o, attr, null);
        }

        protected void UnknownAttribute(object o, XmlAttribute attr, string qnames)
        {
            if (this.events.OnUnknownAttribute != null)
            {
                int num;
                int num2;
                this.GetCurrentPosition(out num, out num2);
                XmlAttributeEventArgs e = new XmlAttributeEventArgs(attr, num, num2, o, qnames);
                this.events.OnUnknownAttribute(this.events.sender, e);
            }
        }

        protected void UnknownElement(object o, XmlElement elem)
        {
            this.UnknownElement(o, elem, null);
        }

        protected void UnknownElement(object o, XmlElement elem, string qnames)
        {
            if (this.events.OnUnknownElement != null)
            {
                int num;
                int num2;
                this.GetCurrentPosition(out num, out num2);
                XmlElementEventArgs e = new XmlElementEventArgs(elem, num, num2, o, qnames);
                this.events.OnUnknownElement(this.events.sender, e);
            }
        }

        protected void UnknownNode(object o)
        {
            this.UnknownNode(o, null);
        }

        protected void UnknownNode(object o, string qnames)
        {
            if ((this.r.NodeType == XmlNodeType.None) || (this.r.NodeType == XmlNodeType.Whitespace))
            {
                this.r.Read();
            }
            else if (this.r.NodeType != XmlNodeType.EndElement)
            {
                if (this.events.OnUnknownNode != null)
                {
                    this.UnknownNode(this.Document.ReadNode(this.r), o, qnames);
                }
                else if ((this.r.NodeType != XmlNodeType.Attribute) || (this.events.OnUnknownAttribute != null))
                {
                    if ((this.r.NodeType == XmlNodeType.Element) && (this.events.OnUnknownElement == null))
                    {
                        this.r.Skip();
                    }
                    else
                    {
                        this.UnknownNode(this.Document.ReadNode(this.r), o, qnames);
                    }
                }
            }
        }

        private void UnknownNode(XmlNode unknownNode, object o, string qnames)
        {
            if (unknownNode != null)
            {
                if (((unknownNode.NodeType != XmlNodeType.None) && (unknownNode.NodeType != XmlNodeType.Whitespace)) && (this.events.OnUnknownNode != null))
                {
                    int num;
                    int num2;
                    this.GetCurrentPosition(out num, out num2);
                    XmlNodeEventArgs e = new XmlNodeEventArgs(unknownNode, num, num2, o);
                    this.events.OnUnknownNode(this.events.sender, e);
                }
                if (unknownNode.NodeType == XmlNodeType.Attribute)
                {
                    this.UnknownAttribute(o, (XmlAttribute) unknownNode, qnames);
                }
                else if (unknownNode.NodeType == XmlNodeType.Element)
                {
                    this.UnknownElement(o, (XmlElement) unknownNode, qnames);
                }
            }
        }

        protected void UnreferencedObject(string id, object o)
        {
            if (this.events.OnUnreferencedObject != null)
            {
                UnreferencedObjectEventArgs e = new UnreferencedObjectEventArgs(o, id);
                this.events.OnUnreferencedObject(this.events.sender, e);
            }
        }

        protected bool DecodeName
        {
            get
            {
                return this.decodeName;
            }
            set
            {
                this.decodeName = value;
            }
        }

        protected XmlDocument Document
        {
            get
            {
                if (this.d == null)
                {
                    this.d = new XmlDocument(this.r.NameTable);
                    this.d.SetBaseURI(this.r.BaseURI);
                }
                return this.d;
            }
        }

        protected bool IsReturnValue
        {
            get
            {
                return (this.isReturnValue && !this.soap12);
            }
            set
            {
                this.isReturnValue = value;
            }
        }

        protected XmlReader Reader
        {
            get
            {
                return this.r;
            }
        }

        protected int ReaderCount
        {
            get
            {
                if (!checkDeserializeAdvances)
                {
                    return 0;
                }
                return this.countingReader.AdvanceCount;
            }
        }

        protected class CollectionFixup
        {
            private XmlSerializationCollectionFixupCallback callback;
            private object collection;
            private object collectionItems;

            public CollectionFixup(object collection, XmlSerializationCollectionFixupCallback callback, object collectionItems)
            {
                this.callback = callback;
                this.collection = collection;
                this.collectionItems = collectionItems;
            }

            public XmlSerializationCollectionFixupCallback Callback
            {
                get
                {
                    return this.callback;
                }
            }

            public object Collection
            {
                get
                {
                    return this.collection;
                }
            }

            public object CollectionItems
            {
                get
                {
                    return this.collectionItems;
                }
            }
        }

        protected class Fixup
        {
            private XmlSerializationFixupCallback callback;
            private string[] ids;
            private object source;

            public Fixup(object o, XmlSerializationFixupCallback callback, int count) : this(o, callback, new string[count])
            {
            }

            public Fixup(object o, XmlSerializationFixupCallback callback, string[] ids)
            {
                this.callback = callback;
                this.Source = o;
                this.ids = ids;
            }

            public XmlSerializationFixupCallback Callback
            {
                get
                {
                    return this.callback;
                }
            }

            public string[] Ids
            {
                get
                {
                    return this.ids;
                }
            }

            public object Source
            {
                get
                {
                    return this.source;
                }
                set
                {
                    this.source = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SoapArrayInfo
        {
            public string qname;
            public int dimensions;
            public int length;
            public int jaggedDimensions;
        }
    }
}

