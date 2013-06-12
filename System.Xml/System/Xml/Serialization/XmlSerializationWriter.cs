namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    public abstract class XmlSerializationWriter : XmlSerializationGeneratedCode
    {
        private string aliasBase = "q";
        private bool escapeName = true;
        private string idBase;
        private XmlSerializerNamespaces namespaces;
        private int nextId;
        private Hashtable objectsInUse;
        private Hashtable references;
        private ArrayList referencesToWrite;
        private bool soap12;
        private int tempNamespacePrefix;
        private Hashtable typeEntries;
        private Hashtable usedPrefixes;
        private XmlWriter w;

        protected XmlSerializationWriter()
        {
        }

        protected void AddWriteCallback(Type type, string typeName, string typeNs, XmlSerializationWriteCallback callback)
        {
            TypeEntry entry = new TypeEntry {
                typeName = typeName,
                typeNs = typeNs,
                type = type,
                callback = callback
            };
            this.typeEntries[type] = entry;
        }

        protected Exception CreateChoiceIdentifierValueException(string value, string identifier, string name, string ns)
        {
            return new InvalidOperationException(Res.GetString("XmlChoiceIdentifierMismatch", new object[] { value, identifier, name, ns }));
        }

        protected Exception CreateInvalidAnyTypeException(object o)
        {
            return this.CreateInvalidAnyTypeException(o.GetType());
        }

        protected Exception CreateInvalidAnyTypeException(Type type)
        {
            return new InvalidOperationException(Res.GetString("XmlIllegalAnyElement", new object[] { type.FullName }));
        }

        protected Exception CreateInvalidChoiceIdentifierValueException(string type, string identifier)
        {
            return new InvalidOperationException(Res.GetString("XmlInvalidChoiceIdentifierValue", new object[] { type, identifier }));
        }

        protected Exception CreateInvalidEnumValueException(object value, string typeName)
        {
            return new InvalidOperationException(Res.GetString("XmlUnknownConstant", new object[] { value, typeName }));
        }

        protected Exception CreateMismatchChoiceException(string value, string elementName, string enumValue)
        {
            return new InvalidOperationException(Res.GetString("XmlChoiceMismatchChoiceException", new object[] { elementName, value, enumValue }));
        }

        protected Exception CreateUnknownAnyElementException(string name, string ns)
        {
            return new InvalidOperationException(Res.GetString("XmlUnknownAnyElement", new object[] { name, ns }));
        }

        protected Exception CreateUnknownTypeException(object o)
        {
            return this.CreateUnknownTypeException(o.GetType());
        }

        protected Exception CreateUnknownTypeException(Type type)
        {
            if (typeof(IXmlSerializable).IsAssignableFrom(type))
            {
                return new InvalidOperationException(Res.GetString("XmlInvalidSerializable", new object[] { type.FullName }));
            }
            if (!new TypeScope().GetTypeDesc(type).IsStructLike)
            {
                return new InvalidOperationException(Res.GetString("XmlInvalidUseOfType", new object[] { type.FullName }));
            }
            return new InvalidOperationException(Res.GetString("XmlUnxpectedType", new object[] { type.FullName }));
        }

        protected static byte[] FromByteArrayBase64(byte[] value)
        {
            return value;
        }

        protected static string FromByteArrayHex(byte[] value)
        {
            return XmlCustomFormatter.FromByteArrayHex(value);
        }

        protected static string FromChar(char value)
        {
            return XmlCustomFormatter.FromChar(value);
        }

        protected static string FromDate(DateTime value)
        {
            return XmlCustomFormatter.FromDate(value);
        }

        protected static string FromDateTime(DateTime value)
        {
            return XmlCustomFormatter.FromDateTime(value);
        }

        protected static string FromEnum(long value, string[] values, long[] ids)
        {
            return XmlCustomFormatter.FromEnum(value, values, ids, null);
        }

        protected static string FromEnum(long value, string[] values, long[] ids, string typeName)
        {
            return XmlCustomFormatter.FromEnum(value, values, ids, typeName);
        }

        protected static string FromTime(DateTime value)
        {
            return XmlCustomFormatter.FromTime(value);
        }

        protected static string FromXmlName(string name)
        {
            return XmlCustomFormatter.FromXmlName(name);
        }

        protected static string FromXmlNCName(string ncName)
        {
            return XmlCustomFormatter.FromXmlNCName(ncName);
        }

        protected static string FromXmlNmToken(string nmToken)
        {
            return XmlCustomFormatter.FromXmlNmToken(nmToken);
        }

        protected static string FromXmlNmTokens(string nmTokens)
        {
            return XmlCustomFormatter.FromXmlNmTokens(nmTokens);
        }

        protected string FromXmlQualifiedName(XmlQualifiedName xmlQualifiedName)
        {
            return this.FromXmlQualifiedName(xmlQualifiedName, true);
        }

        protected string FromXmlQualifiedName(XmlQualifiedName xmlQualifiedName, bool ignoreEmpty)
        {
            if (xmlQualifiedName == null)
            {
                return null;
            }
            if (xmlQualifiedName.IsEmpty && ignoreEmpty)
            {
                return null;
            }
            return this.GetQualifiedName(this.EscapeName ? XmlConvert.EncodeLocalName(xmlQualifiedName.Name) : xmlQualifiedName.Name, xmlQualifiedName.Namespace);
        }

        private string GetId(object o, bool addToReferencesList)
        {
            if (this.references == null)
            {
                this.references = new Hashtable();
                this.referencesToWrite = new ArrayList();
            }
            string str = (string) this.references[o];
            if (str == null)
            {
                str = this.idBase + "id" + ++this.nextId.ToString(CultureInfo.InvariantCulture);
                this.references.Add(o, str);
                if (addToReferencesList)
                {
                    this.referencesToWrite.Add(o);
                }
            }
            return str;
        }

        private XmlQualifiedName GetPrimitiveTypeName(Type type)
        {
            return this.GetPrimitiveTypeName(type, true);
        }

        private XmlQualifiedName GetPrimitiveTypeName(Type type, bool throwIfUnknown)
        {
            XmlQualifiedName primitiveTypeNameInternal = GetPrimitiveTypeNameInternal(type);
            if (throwIfUnknown && (primitiveTypeNameInternal == null))
            {
                throw this.CreateUnknownTypeException(type);
            }
            return primitiveTypeNameInternal;
        }

        internal static XmlQualifiedName GetPrimitiveTypeNameInternal(Type type)
        {
            string str;
            string ns = "http://www.w3.org/2001/XMLSchema";
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    str = "boolean";
                    break;

                case TypeCode.Char:
                    str = "char";
                    ns = "http://microsoft.com/wsdl/types/";
                    break;

                case TypeCode.SByte:
                    str = "byte";
                    break;

                case TypeCode.Byte:
                    str = "unsignedByte";
                    break;

                case TypeCode.Int16:
                    str = "short";
                    break;

                case TypeCode.UInt16:
                    str = "unsignedShort";
                    break;

                case TypeCode.Int32:
                    str = "int";
                    break;

                case TypeCode.UInt32:
                    str = "unsignedInt";
                    break;

                case TypeCode.Int64:
                    str = "long";
                    break;

                case TypeCode.UInt64:
                    str = "unsignedLong";
                    break;

                case TypeCode.Single:
                    str = "float";
                    break;

                case TypeCode.Double:
                    str = "double";
                    break;

                case TypeCode.Decimal:
                    str = "decimal";
                    break;

                case TypeCode.DateTime:
                    str = "dateTime";
                    break;

                case TypeCode.String:
                    str = "string";
                    break;

                default:
                    if (type == typeof(XmlQualifiedName))
                    {
                        str = "QName";
                    }
                    else if (type == typeof(byte[]))
                    {
                        str = "base64Binary";
                    }
                    else if (type == typeof(Guid))
                    {
                        str = "guid";
                        ns = "http://microsoft.com/wsdl/types/";
                    }
                    else if (type == typeof(XmlNode[]))
                    {
                        str = "anyType";
                    }
                    else
                    {
                        return null;
                    }
                    break;
            }
            return new XmlQualifiedName(str, ns);
        }

        private string GetQualifiedName(string name, string ns)
        {
            if ((ns == null) || (ns.Length == 0))
            {
                return name;
            }
            string prefix = this.w.LookupPrefix(ns);
            if (prefix == null)
            {
                if (ns == "http://www.w3.org/XML/1998/namespace")
                {
                    prefix = "xml";
                }
                else
                {
                    prefix = this.NextPrefix();
                    this.WriteAttribute("xmlns", prefix, null, ns);
                }
            }
            else if (prefix.Length == 0)
            {
                return name;
            }
            return (prefix + ":" + name);
        }

        private TypeEntry GetTypeEntry(Type t)
        {
            if (this.typeEntries == null)
            {
                this.typeEntries = new Hashtable();
                this.InitCallbacks();
            }
            return (TypeEntry) this.typeEntries[t];
        }

        internal void Init(XmlWriter w, XmlSerializerNamespaces namespaces, string encodingStyle, string idBase, TempAssembly tempAssembly)
        {
            this.w = w;
            this.namespaces = namespaces;
            this.soap12 = encodingStyle == "http://www.w3.org/2003/05/soap-encoding";
            this.idBase = idBase;
            base.Init(tempAssembly);
        }

        protected abstract void InitCallbacks();
        private bool IsIdDefined(object o)
        {
            return ((this.references != null) && this.references.Contains(o));
        }

        private Hashtable ListUsedPrefixes(Hashtable nsList, string prefix)
        {
            Hashtable hashtable = new Hashtable();
            int length = prefix.Length;
            foreach (string str in this.namespaces.Namespaces.Keys)
            {
                if (str.Length <= length)
                {
                    continue;
                }
                string s = str;
                int num1 = s.Length;
                if (((s.Length > length) && (s.Length <= (length + "2147483647".Length))) && s.StartsWith(prefix, StringComparison.Ordinal))
                {
                    bool flag = true;
                    for (int i = length; i < s.Length; i++)
                    {
                        if (!char.IsDigit(s, i))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        long num3 = long.Parse(s.Substring(length), CultureInfo.InvariantCulture);
                        if (num3 <= 0x7fffffffL)
                        {
                            int key = (int) num3;
                            if (!hashtable.ContainsKey(key))
                            {
                                hashtable.Add(key, key);
                            }
                        }
                    }
                }
            }
            if (hashtable.Count > 0)
            {
                return hashtable;
            }
            return null;
        }

        private string NextPrefix()
        {
            if (this.usedPrefixes == null)
            {
                return (this.aliasBase + ++this.tempNamespacePrefix);
            }
            while (this.usedPrefixes.ContainsKey(++this.tempNamespacePrefix))
            {
            }
            return (this.aliasBase + this.tempNamespacePrefix);
        }

        protected static Assembly ResolveDynamicAssembly(string assemblyFullName)
        {
            return DynamicAssemblies.Get(assemblyFullName);
        }

        protected void TopLevelElement()
        {
            this.objectsInUse = new Hashtable();
        }

        private void WriteArray(string name, string ns, object o, Type type)
        {
            string typeName;
            string typeNs;
            Type arrayElementType = TypeScope.GetArrayElementType(type, null);
            StringBuilder builder = new StringBuilder();
            if (!this.soap12)
            {
                while ((arrayElementType.IsArray || typeof(IEnumerable).IsAssignableFrom(arrayElementType)) && (this.GetPrimitiveTypeName(arrayElementType, false) == null))
                {
                    arrayElementType = TypeScope.GetArrayElementType(arrayElementType, null);
                    builder.Append("[]");
                }
            }
            if (arrayElementType == typeof(object))
            {
                typeName = "anyType";
                typeNs = "http://www.w3.org/2001/XMLSchema";
            }
            else
            {
                TypeEntry typeEntry = this.GetTypeEntry(arrayElementType);
                if (typeEntry != null)
                {
                    typeName = typeEntry.typeName;
                    typeNs = typeEntry.typeNs;
                }
                else if (!this.soap12)
                {
                    XmlQualifiedName primitiveTypeName = this.GetPrimitiveTypeName(arrayElementType);
                    typeName = primitiveTypeName.Name;
                    typeNs = primitiveTypeName.Namespace;
                }
                else
                {
                    XmlQualifiedName name2 = this.GetPrimitiveTypeName(arrayElementType, false);
                    if (name2 != null)
                    {
                        typeName = name2.Name;
                        typeNs = name2.Namespace;
                    }
                    else
                    {
                        for (Type type3 = arrayElementType.BaseType; type3 != null; type3 = type3.BaseType)
                        {
                            typeEntry = this.GetTypeEntry(type3);
                            if (typeEntry != null)
                            {
                                break;
                            }
                        }
                        if (typeEntry != null)
                        {
                            typeName = typeEntry.typeName;
                            typeNs = typeEntry.typeNs;
                        }
                        else
                        {
                            typeName = "anyType";
                            typeNs = "http://www.w3.org/2001/XMLSchema";
                        }
                    }
                }
            }
            if (builder.Length > 0)
            {
                typeName = typeName + builder.ToString();
            }
            if ((this.soap12 && (name != null)) && (name.Length > 0))
            {
                this.WriteStartElement(name, ns, null, false);
            }
            else
            {
                this.WriteStartElement("Array", "http://schemas.xmlsoap.org/soap/encoding/", null, true);
            }
            this.WriteId(o, false);
            if (type.IsArray)
            {
                Array array = (Array) o;
                int length = array.Length;
                if (this.soap12)
                {
                    this.w.WriteAttributeString("itemType", "http://www.w3.org/2003/05/soap-encoding", this.GetQualifiedName(typeName, typeNs));
                    this.w.WriteAttributeString("arraySize", "http://www.w3.org/2003/05/soap-encoding", length.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    this.w.WriteAttributeString("arrayType", "http://schemas.xmlsoap.org/soap/encoding/", this.GetQualifiedName(typeName, typeNs) + "[" + length.ToString(CultureInfo.InvariantCulture) + "]");
                }
                for (int i = 0; i < length; i++)
                {
                    this.WritePotentiallyReferencingElement("Item", "", array.GetValue(i), arrayElementType, false, true);
                }
            }
            else
            {
                int num3 = typeof(ICollection).IsAssignableFrom(type) ? ((ICollection) o).Count : -1;
                if (this.soap12)
                {
                    this.w.WriteAttributeString("itemType", "http://www.w3.org/2003/05/soap-encoding", this.GetQualifiedName(typeName, typeNs));
                    if (num3 >= 0)
                    {
                        this.w.WriteAttributeString("arraySize", "http://www.w3.org/2003/05/soap-encoding", num3.ToString(CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    this.w.WriteAttributeString("arrayType", "http://schemas.xmlsoap.org/soap/encoding/", this.GetQualifiedName(typeName, typeNs) + ((num3 >= 0) ? ("[" + num3 + "]") : "[]"));
                }
                IEnumerator enumerator = ((IEnumerable) o).GetEnumerator();
                if (enumerator != null)
                {
                    while (enumerator.MoveNext())
                    {
                        this.WritePotentiallyReferencingElement("Item", "", enumerator.Current, arrayElementType, false, true);
                    }
                }
            }
            this.w.WriteEndElement();
        }

        protected void WriteAttribute(string localName, string value)
        {
            if (value != null)
            {
                this.w.WriteAttributeString(localName, null, value);
            }
        }

        protected void WriteAttribute(string localName, byte[] value)
        {
            if (value != null)
            {
                this.w.WriteStartAttribute(null, localName, null);
                XmlCustomFormatter.WriteArrayBase64(this.w, value, 0, value.Length);
                this.w.WriteEndAttribute();
            }
        }

        protected void WriteAttribute(string localName, string ns, string value)
        {
            if (((value != null) && (localName != "xmlns")) && !localName.StartsWith("xmlns:", StringComparison.Ordinal))
            {
                int index = localName.IndexOf(':');
                if (index < 0)
                {
                    if (ns == "http://www.w3.org/XML/1998/namespace")
                    {
                        string prefix = this.w.LookupPrefix(ns);
                        if ((prefix == null) || (prefix.Length == 0))
                        {
                            prefix = "xml";
                        }
                        this.w.WriteAttributeString(prefix, localName, ns, value);
                    }
                    else
                    {
                        this.w.WriteAttributeString(localName, ns, value);
                    }
                }
                else
                {
                    string str2 = localName.Substring(0, index);
                    this.w.WriteAttributeString(str2, localName.Substring(index + 1), ns, value);
                }
            }
        }

        protected void WriteAttribute(string localName, string ns, byte[] value)
        {
            if (((value != null) && (localName != "xmlns")) && !localName.StartsWith("xmlns:", StringComparison.Ordinal))
            {
                int index = localName.IndexOf(':');
                if (index < 0)
                {
                    if (ns == "http://www.w3.org/XML/1998/namespace")
                    {
                        string prefix = this.w.LookupPrefix(ns);
                        if ((prefix == null) || (prefix.Length == 0))
                        {
                            prefix = "xml";
                        }
                        this.w.WriteStartAttribute("xml", localName, ns);
                    }
                    else
                    {
                        this.w.WriteStartAttribute(null, localName, ns);
                    }
                }
                else
                {
                    string str2 = localName.Substring(0, index);
                    str2 = this.w.LookupPrefix(ns);
                    this.w.WriteStartAttribute(str2, localName.Substring(index + 1), ns);
                }
                XmlCustomFormatter.WriteArrayBase64(this.w, value, 0, value.Length);
                this.w.WriteEndAttribute();
            }
        }

        protected void WriteAttribute(string prefix, string localName, string ns, string value)
        {
            if (value != null)
            {
                this.w.WriteAttributeString(prefix, localName, null, value);
            }
        }

        private void WriteElement(XmlNode node, string name, string ns, bool isNullable, bool any)
        {
            if (typeof(XmlAttribute).IsAssignableFrom(node.GetType()))
            {
                throw new InvalidOperationException(Res.GetString("XmlNoAttributeHere"));
            }
            if (node is XmlDocument)
            {
                node = ((XmlDocument) node).DocumentElement;
                if (node == null)
                {
                    if (isNullable)
                    {
                        this.WriteNullTagEncoded(name, ns);
                    }
                    return;
                }
            }
            if (any)
            {
                if ((((node is XmlElement) && (name != null)) && (name.Length > 0)) && ((node.LocalName != name) || (node.NamespaceURI != ns)))
                {
                    throw new InvalidOperationException(Res.GetString("XmlElementNameMismatch", new object[] { node.LocalName, node.NamespaceURI, name, ns }));
                }
            }
            else
            {
                this.w.WriteStartElement(name, ns);
            }
            node.WriteTo(this.w);
            if (!any)
            {
                this.w.WriteEndElement();
            }
        }

        protected void WriteElementEncoded(XmlNode node, string name, string ns, bool isNullable, bool any)
        {
            if (node == null)
            {
                if (isNullable)
                {
                    this.WriteNullTagEncoded(name, ns);
                }
            }
            else
            {
                this.WriteElement(node, name, ns, isNullable, any);
            }
        }

        protected void WriteElementLiteral(XmlNode node, string name, string ns, bool isNullable, bool any)
        {
            if (node == null)
            {
                if (isNullable)
                {
                    this.WriteNullTagLiteral(name, ns);
                }
            }
            else
            {
                this.WriteElement(node, name, ns, isNullable, any);
            }
        }

        protected void WriteElementQualifiedName(string localName, XmlQualifiedName value)
        {
            this.WriteElementQualifiedName(localName, null, value, null);
        }

        protected void WriteElementQualifiedName(string localName, string ns, XmlQualifiedName value)
        {
            this.WriteElementQualifiedName(localName, ns, value, null);
        }

        protected void WriteElementQualifiedName(string localName, XmlQualifiedName value, XmlQualifiedName xsiType)
        {
            this.WriteElementQualifiedName(localName, null, value, xsiType);
        }

        protected void WriteElementQualifiedName(string localName, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
        {
            if (value != null)
            {
                if ((value.Namespace == null) || (value.Namespace.Length == 0))
                {
                    this.WriteStartElement(localName, ns, null, true);
                    this.WriteAttribute("xmlns", "");
                }
                else
                {
                    this.w.WriteStartElement(localName, ns);
                }
                if (xsiType != null)
                {
                    this.WriteXsiType(xsiType.Name, xsiType.Namespace);
                }
                this.w.WriteString(this.FromXmlQualifiedName(value, false));
                this.w.WriteEndElement();
            }
        }

        protected void WriteElementString(string localName, string value)
        {
            this.WriteElementString(localName, null, value, null);
        }

        protected void WriteElementString(string localName, string ns, string value)
        {
            this.WriteElementString(localName, ns, value, null);
        }

        protected void WriteElementString(string localName, string value, XmlQualifiedName xsiType)
        {
            this.WriteElementString(localName, null, value, xsiType);
        }

        protected void WriteElementString(string localName, string ns, string value, XmlQualifiedName xsiType)
        {
            if (value != null)
            {
                if (xsiType == null)
                {
                    this.w.WriteElementString(localName, ns, value);
                }
                else
                {
                    this.w.WriteStartElement(localName, ns);
                    this.WriteXsiType(xsiType.Name, xsiType.Namespace);
                    this.w.WriteString(value);
                    this.w.WriteEndElement();
                }
            }
        }

        protected void WriteElementStringRaw(string localName, string value)
        {
            this.WriteElementStringRaw(localName, null, value, null);
        }

        protected void WriteElementStringRaw(string localName, byte[] value)
        {
            this.WriteElementStringRaw(localName, null, value, null);
        }

        protected void WriteElementStringRaw(string localName, string ns, string value)
        {
            this.WriteElementStringRaw(localName, ns, value, null);
        }

        protected void WriteElementStringRaw(string localName, string ns, byte[] value)
        {
            this.WriteElementStringRaw(localName, ns, value, null);
        }

        protected void WriteElementStringRaw(string localName, string value, XmlQualifiedName xsiType)
        {
            this.WriteElementStringRaw(localName, null, value, xsiType);
        }

        protected void WriteElementStringRaw(string localName, byte[] value, XmlQualifiedName xsiType)
        {
            this.WriteElementStringRaw(localName, null, value, xsiType);
        }

        protected void WriteElementStringRaw(string localName, string ns, string value, XmlQualifiedName xsiType)
        {
            if (value != null)
            {
                this.w.WriteStartElement(localName, ns);
                if (xsiType != null)
                {
                    this.WriteXsiType(xsiType.Name, xsiType.Namespace);
                }
                this.w.WriteRaw(value);
                this.w.WriteEndElement();
            }
        }

        protected void WriteElementStringRaw(string localName, string ns, byte[] value, XmlQualifiedName xsiType)
        {
            if (value != null)
            {
                this.w.WriteStartElement(localName, ns);
                if (xsiType != null)
                {
                    this.WriteXsiType(xsiType.Name, xsiType.Namespace);
                }
                XmlCustomFormatter.WriteArrayBase64(this.w, value, 0, value.Length);
                this.w.WriteEndElement();
            }
        }

        protected void WriteEmptyTag(string name)
        {
            this.WriteEmptyTag(name, null);
        }

        protected void WriteEmptyTag(string name, string ns)
        {
            if ((name != null) && (name.Length != 0))
            {
                this.WriteStartElement(name, ns, null, false);
                this.w.WriteEndElement();
            }
        }

        protected void WriteEndElement()
        {
            this.w.WriteEndElement();
        }

        protected void WriteEndElement(object o)
        {
            this.w.WriteEndElement();
            if ((o != null) && (this.objectsInUse != null))
            {
                this.objectsInUse.Remove(o);
            }
        }

        protected void WriteId(object o)
        {
            this.WriteId(o, true);
        }

        private void WriteId(object o, bool addToReferencesList)
        {
            if (this.soap12)
            {
                this.w.WriteAttributeString("id", "http://www.w3.org/2003/05/soap-encoding", this.GetId(o, addToReferencesList));
            }
            else
            {
                this.w.WriteAttributeString("id", this.GetId(o, addToReferencesList));
            }
        }

        protected void WriteNamespaceDeclarations(XmlSerializerNamespaces xmlns)
        {
            if (xmlns != null)
            {
                foreach (DictionaryEntry entry in xmlns.Namespaces)
                {
                    string key = (string) entry.Key;
                    string ns = (string) entry.Value;
                    if (this.namespaces != null)
                    {
                        string str3 = this.namespaces.Namespaces[key] as string;
                        if ((str3 != null) && (str3 != ns))
                        {
                            throw new InvalidOperationException(Res.GetString("XmlDuplicateNs", new object[] { key, ns }));
                        }
                    }
                    string str4 = ((ns == null) || (ns.Length == 0)) ? null : this.Writer.LookupPrefix(ns);
                    if ((str4 == null) || (str4 != key))
                    {
                        this.WriteAttribute("xmlns", key, null, ns);
                    }
                }
            }
            this.namespaces = null;
        }

        protected void WriteNullableQualifiedNameEncoded(string name, string ns, XmlQualifiedName value, XmlQualifiedName xsiType)
        {
            if (value == null)
            {
                this.WriteNullTagEncoded(name, ns);
            }
            else
            {
                this.WriteElementQualifiedName(name, ns, value, xsiType);
            }
        }

        protected void WriteNullableQualifiedNameLiteral(string name, string ns, XmlQualifiedName value)
        {
            if (value == null)
            {
                this.WriteNullTagLiteral(name, ns);
            }
            else
            {
                this.WriteElementQualifiedName(name, ns, value, null);
            }
        }

        protected void WriteNullableStringEncoded(string name, string ns, string value, XmlQualifiedName xsiType)
        {
            if (value == null)
            {
                this.WriteNullTagEncoded(name, ns);
            }
            else
            {
                this.WriteElementString(name, ns, value, xsiType);
            }
        }

        protected void WriteNullableStringEncodedRaw(string name, string ns, string value, XmlQualifiedName xsiType)
        {
            if (value == null)
            {
                this.WriteNullTagEncoded(name, ns);
            }
            else
            {
                this.WriteElementStringRaw(name, ns, value, xsiType);
            }
        }

        protected void WriteNullableStringEncodedRaw(string name, string ns, byte[] value, XmlQualifiedName xsiType)
        {
            if (value == null)
            {
                this.WriteNullTagEncoded(name, ns);
            }
            else
            {
                this.WriteElementStringRaw(name, ns, value, xsiType);
            }
        }

        protected void WriteNullableStringLiteral(string name, string ns, string value)
        {
            if (value == null)
            {
                this.WriteNullTagLiteral(name, ns);
            }
            else
            {
                this.WriteElementString(name, ns, value, null);
            }
        }

        protected void WriteNullableStringLiteralRaw(string name, string ns, string value)
        {
            if (value == null)
            {
                this.WriteNullTagLiteral(name, ns);
            }
            else
            {
                this.WriteElementStringRaw(name, ns, value, null);
            }
        }

        protected void WriteNullableStringLiteralRaw(string name, string ns, byte[] value)
        {
            if (value == null)
            {
                this.WriteNullTagLiteral(name, ns);
            }
            else
            {
                this.WriteElementStringRaw(name, ns, value, null);
            }
        }

        protected void WriteNullTagEncoded(string name)
        {
            this.WriteNullTagEncoded(name, null);
        }

        protected void WriteNullTagEncoded(string name, string ns)
        {
            if ((name != null) && (name.Length != 0))
            {
                this.WriteStartElement(name, ns, null, true);
                this.w.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                this.w.WriteEndElement();
            }
        }

        protected void WriteNullTagLiteral(string name)
        {
            this.WriteNullTagLiteral(name, null);
        }

        protected void WriteNullTagLiteral(string name, string ns)
        {
            if ((name != null) && (name.Length != 0))
            {
                this.WriteStartElement(name, ns, null, false);
                this.w.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                this.w.WriteEndElement();
            }
        }

        protected void WritePotentiallyReferencingElement(string n, string ns, object o)
        {
            this.WritePotentiallyReferencingElement(n, ns, o, null, false, false);
        }

        protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType)
        {
            this.WritePotentiallyReferencingElement(n, ns, o, ambientType, false, false);
        }

        protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType, bool suppressReference)
        {
            this.WritePotentiallyReferencingElement(n, ns, o, ambientType, suppressReference, false);
        }

        protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType, bool suppressReference, bool isNullable)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    this.WriteNullTagEncoded(n, ns);
                }
            }
            else
            {
                Type t = o.GetType();
                if ((((Convert.GetTypeCode(o) == TypeCode.Object) && !(o is Guid)) && ((t != typeof(XmlQualifiedName)) && !(o is XmlNode[]))) && (t != typeof(byte[])))
                {
                    if ((suppressReference || this.soap12) && !this.IsIdDefined(o))
                    {
                        this.WriteReferencedElement(n, ns, o, ambientType);
                    }
                    else if (n == null)
                    {
                        TypeEntry typeEntry = this.GetTypeEntry(t);
                        this.WriteReferencingElement(typeEntry.typeName, typeEntry.typeNs, o, isNullable);
                    }
                    else
                    {
                        this.WriteReferencingElement(n, ns, o, isNullable);
                    }
                }
                else
                {
                    bool xsiType = (t != ambientType) && !t.IsEnum;
                    TypeEntry entry2 = this.GetTypeEntry(t);
                    if (entry2 != null)
                    {
                        if (n == null)
                        {
                            this.WriteStartElement(entry2.typeName, entry2.typeNs, null, true);
                        }
                        else
                        {
                            this.WriteStartElement(n, ns, null, true);
                        }
                        if (xsiType)
                        {
                            this.WriteXsiType(entry2.typeName, entry2.typeNs);
                        }
                        entry2.callback(o);
                        this.w.WriteEndElement();
                    }
                    else
                    {
                        this.WriteTypedPrimitive(n, ns, o, xsiType);
                    }
                }
            }
        }

        private void WriteReferencedElement(object o, Type ambientType)
        {
            this.WriteReferencedElement(null, null, o, ambientType);
        }

        private void WriteReferencedElement(string name, string ns, object o, Type ambientType)
        {
            if (name == null)
            {
                name = string.Empty;
            }
            Type c = o.GetType();
            if (c.IsArray || typeof(IEnumerable).IsAssignableFrom(c))
            {
                this.WriteArray(name, ns, o, c);
            }
            else
            {
                TypeEntry typeEntry = this.GetTypeEntry(c);
                if (typeEntry == null)
                {
                    throw this.CreateUnknownTypeException(c);
                }
                this.WriteStartElement((name.Length == 0) ? typeEntry.typeName : name, (ns == null) ? typeEntry.typeNs : ns, null, true);
                this.WriteId(o, false);
                if (ambientType != c)
                {
                    this.WriteXsiType(typeEntry.typeName, typeEntry.typeNs);
                }
                typeEntry.callback(o);
                this.w.WriteEndElement();
            }
        }

        protected void WriteReferencedElements()
        {
            if (this.referencesToWrite != null)
            {
                for (int i = 0; i < this.referencesToWrite.Count; i++)
                {
                    this.WriteReferencedElement(this.referencesToWrite[i], null);
                }
            }
        }

        protected void WriteReferencingElement(string n, string ns, object o)
        {
            this.WriteReferencingElement(n, ns, o, false);
        }

        protected void WriteReferencingElement(string n, string ns, object o, bool isNullable)
        {
            if (o == null)
            {
                if (isNullable)
                {
                    this.WriteNullTagEncoded(n, ns);
                }
            }
            else
            {
                this.WriteStartElement(n, ns, null, true);
                if (this.soap12)
                {
                    this.w.WriteAttributeString("ref", "http://www.w3.org/2003/05/soap-encoding", this.GetId(o, true));
                }
                else
                {
                    this.w.WriteAttributeString("href", "#" + this.GetId(o, true));
                }
                this.w.WriteEndElement();
            }
        }

        protected void WriteRpcResult(string name, string ns)
        {
            if (this.soap12)
            {
                this.WriteElementQualifiedName("result", "http://www.w3.org/2003/05/soap-rpc", new XmlQualifiedName(name, ns), null);
            }
        }

        protected void WriteSerializable(IXmlSerializable serializable, string name, string ns, bool isNullable)
        {
            this.WriteSerializable(serializable, name, ns, isNullable, true);
        }

        protected void WriteSerializable(IXmlSerializable serializable, string name, string ns, bool isNullable, bool wrapped)
        {
            if (serializable == null)
            {
                if (isNullable)
                {
                    this.WriteNullTagLiteral(name, ns);
                }
            }
            else
            {
                if (wrapped)
                {
                    this.w.WriteStartElement(name, ns);
                }
                serializable.WriteXml(this.w);
                if (wrapped)
                {
                    this.w.WriteEndElement();
                }
            }
        }

        protected void WriteStartDocument()
        {
            if (this.w.WriteState == WriteState.Start)
            {
                this.w.WriteStartDocument();
            }
        }

        protected void WriteStartElement(string name)
        {
            this.WriteStartElement(name, null, null, false, null);
        }

        protected void WriteStartElement(string name, string ns)
        {
            this.WriteStartElement(name, ns, null, false, null);
        }

        protected void WriteStartElement(string name, string ns, bool writePrefixed)
        {
            this.WriteStartElement(name, ns, null, writePrefixed, null);
        }

        protected void WriteStartElement(string name, string ns, object o)
        {
            this.WriteStartElement(name, ns, o, false, null);
        }

        protected void WriteStartElement(string name, string ns, object o, bool writePrefixed)
        {
            this.WriteStartElement(name, ns, o, writePrefixed, null);
        }

        protected void WriteStartElement(string name, string ns, object o, bool writePrefixed, XmlSerializerNamespaces xmlns)
        {
            if ((o != null) && (this.objectsInUse != null))
            {
                if (this.objectsInUse.ContainsKey(o))
                {
                    throw new InvalidOperationException(Res.GetString("XmlCircularReference", new object[] { o.GetType().FullName }));
                }
                this.objectsInUse.Add(o, o);
            }
            string prefix = null;
            bool flag = false;
            if (this.namespaces != null)
            {
                foreach (string str2 in this.namespaces.Namespaces.Keys)
                {
                    string str3 = (string) this.namespaces.Namespaces[str2];
                    if ((str2.Length > 0) && (str3 == ns))
                    {
                        prefix = str2;
                    }
                    if (str2.Length == 0)
                    {
                        if ((str3 == null) || (str3.Length == 0))
                        {
                            flag = true;
                        }
                        if (ns != str3)
                        {
                            writePrefixed = true;
                        }
                    }
                }
                this.usedPrefixes = this.ListUsedPrefixes(this.namespaces.Namespaces, this.aliasBase);
            }
            if ((writePrefixed && (prefix == null)) && ((ns != null) && (ns.Length > 0)))
            {
                prefix = this.w.LookupPrefix(ns);
                if ((prefix == null) || (prefix.Length == 0))
                {
                    prefix = this.NextPrefix();
                }
            }
            if ((prefix == null) && (xmlns != null))
            {
                prefix = xmlns.LookupPrefix(ns);
            }
            if ((flag && (prefix == null)) && ((ns != null) && (ns.Length != 0)))
            {
                prefix = this.NextPrefix();
            }
            this.w.WriteStartElement(prefix, name, ns);
            if (this.namespaces != null)
            {
                foreach (string str4 in this.namespaces.Namespaces.Keys)
                {
                    string str5 = (string) this.namespaces.Namespaces[str4];
                    if ((str4.Length != 0) || ((str5 != null) && (str5.Length != 0)))
                    {
                        if ((str5 == null) || (str5.Length == 0))
                        {
                            if (str4.Length > 0)
                            {
                                throw new InvalidOperationException(Res.GetString("XmlInvalidXmlns", new object[] { str4 }));
                            }
                            this.WriteAttribute("xmlns", str4, null, str5);
                        }
                        else if (this.w.LookupPrefix(str5) == null)
                        {
                            if ((prefix == null) && (str4.Length == 0))
                            {
                                break;
                            }
                            this.WriteAttribute("xmlns", str4, null, str5);
                        }
                    }
                }
            }
            this.WriteNamespaceDeclarations(xmlns);
        }

        protected void WriteTypedPrimitive(string name, string ns, object o, bool xsiType)
        {
            string data = null;
            string str2;
            string str3 = "http://www.w3.org/2001/XMLSchema";
            bool flag = true;
            bool flag2 = false;
            Type type = o.GetType();
            bool flag3 = false;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    data = XmlConvert.ToString((bool) o);
                    str2 = "boolean";
                    break;

                case TypeCode.Char:
                    data = FromChar((char) o);
                    str2 = "char";
                    str3 = "http://microsoft.com/wsdl/types/";
                    break;

                case TypeCode.SByte:
                    data = XmlConvert.ToString((sbyte) o);
                    str2 = "byte";
                    break;

                case TypeCode.Byte:
                    data = XmlConvert.ToString((byte) o);
                    str2 = "unsignedByte";
                    break;

                case TypeCode.Int16:
                    data = XmlConvert.ToString((short) o);
                    str2 = "short";
                    break;

                case TypeCode.UInt16:
                    data = XmlConvert.ToString((ushort) o);
                    str2 = "unsignedShort";
                    break;

                case TypeCode.Int32:
                    data = XmlConvert.ToString((int) o);
                    str2 = "int";
                    break;

                case TypeCode.UInt32:
                    data = XmlConvert.ToString((uint) o);
                    str2 = "unsignedInt";
                    break;

                case TypeCode.Int64:
                    data = XmlConvert.ToString((long) o);
                    str2 = "long";
                    break;

                case TypeCode.UInt64:
                    data = XmlConvert.ToString((ulong) o);
                    str2 = "unsignedLong";
                    break;

                case TypeCode.Single:
                    data = XmlConvert.ToString((float) o);
                    str2 = "float";
                    break;

                case TypeCode.Double:
                    data = XmlConvert.ToString((double) o);
                    str2 = "double";
                    break;

                case TypeCode.Decimal:
                    data = XmlConvert.ToString((decimal) o);
                    str2 = "decimal";
                    break;

                case TypeCode.DateTime:
                    data = FromDateTime((DateTime) o);
                    str2 = "dateTime";
                    break;

                case TypeCode.String:
                    data = (string) o;
                    str2 = "string";
                    flag = false;
                    break;

                default:
                    if (type == typeof(XmlQualifiedName))
                    {
                        str2 = "QName";
                        flag3 = true;
                        if (name == null)
                        {
                            this.w.WriteStartElement(str2, str3);
                        }
                        else
                        {
                            this.w.WriteStartElement(name, ns);
                        }
                        data = this.FromXmlQualifiedName((XmlQualifiedName) o, false);
                    }
                    else if (type == typeof(byte[]))
                    {
                        data = string.Empty;
                        flag2 = true;
                        str2 = "base64Binary";
                    }
                    else if (type == typeof(Guid))
                    {
                        data = XmlConvert.ToString((Guid) o);
                        str2 = "guid";
                        str3 = "http://microsoft.com/wsdl/types/";
                    }
                    else
                    {
                        if (!typeof(XmlNode[]).IsAssignableFrom(type))
                        {
                            throw this.CreateUnknownTypeException(type);
                        }
                        if (name == null)
                        {
                            this.w.WriteStartElement("anyType", "http://www.w3.org/2001/XMLSchema");
                        }
                        else
                        {
                            this.w.WriteStartElement(name, ns);
                        }
                        XmlNode[] nodeArray = (XmlNode[]) o;
                        for (int i = 0; i < nodeArray.Length; i++)
                        {
                            if (nodeArray[i] != null)
                            {
                                nodeArray[i].WriteTo(this.w);
                            }
                        }
                        this.w.WriteEndElement();
                        return;
                    }
                    break;
            }
            if (!flag3)
            {
                if (name == null)
                {
                    this.w.WriteStartElement(str2, str3);
                }
                else
                {
                    this.w.WriteStartElement(name, ns);
                }
            }
            if (xsiType)
            {
                this.WriteXsiType(str2, str3);
            }
            if (data == null)
            {
                this.w.WriteAttributeString("nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            }
            else if (flag2)
            {
                XmlCustomFormatter.WriteArrayBase64(this.w, (byte[]) o, 0, ((byte[]) o).Length);
            }
            else if (flag)
            {
                this.w.WriteRaw(data);
            }
            else
            {
                this.w.WriteString(data);
            }
            this.w.WriteEndElement();
        }

        protected void WriteValue(string value)
        {
            if (value != null)
            {
                this.w.WriteString(value);
            }
        }

        protected void WriteValue(byte[] value)
        {
            if (value != null)
            {
                XmlCustomFormatter.WriteArrayBase64(this.w, value, 0, value.Length);
            }
        }

        protected void WriteXmlAttribute(XmlNode node)
        {
            this.WriteXmlAttribute(node, null);
        }

        protected void WriteXmlAttribute(XmlNode node, object container)
        {
            XmlAttribute attribute = node as XmlAttribute;
            if (attribute == null)
            {
                throw new InvalidOperationException(Res.GetString("XmlNeedAttributeHere"));
            }
            if (attribute.Value != null)
            {
                if ((attribute.NamespaceURI == "http://schemas.xmlsoap.org/wsdl/") && (attribute.LocalName == "arrayType"))
                {
                    string str;
                    XmlQualifiedName xmlQualifiedName = TypeScope.ParseWsdlArrayType(attribute.Value, out str, (container is XmlSchemaObject) ? ((XmlSchemaObject) container) : null);
                    string str2 = this.FromXmlQualifiedName(xmlQualifiedName, true) + str;
                    this.WriteAttribute("arrayType", "http://schemas.xmlsoap.org/wsdl/", str2);
                }
                else
                {
                    this.WriteAttribute(attribute.Name, attribute.NamespaceURI, attribute.Value);
                }
            }
        }

        protected void WriteXsiType(string name, string ns)
        {
            this.WriteAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", this.GetQualifiedName(name, ns));
        }

        protected bool EscapeName
        {
            get
            {
                return this.escapeName;
            }
            set
            {
                this.escapeName = value;
            }
        }

        protected ArrayList Namespaces
        {
            get
            {
                if (this.namespaces != null)
                {
                    return this.namespaces.NamespaceList;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    this.namespaces = null;
                }
                else
                {
                    XmlQualifiedName[] namespaces = (XmlQualifiedName[]) value.ToArray(typeof(XmlQualifiedName));
                    this.namespaces = new XmlSerializerNamespaces(namespaces);
                }
            }
        }

        protected XmlWriter Writer
        {
            get
            {
                return this.w;
            }
            set
            {
                this.w = value;
            }
        }

        internal class TypeEntry
        {
            internal XmlSerializationWriteCallback callback;
            internal Type type;
            internal string typeName;
            internal string typeNs;
        }
    }
}

