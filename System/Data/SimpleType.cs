namespace System.Data
{
    using System;
    using System.Collections;
    using System.Data.Common;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Xml;
    using System.Xml.Schema;

    [Serializable]
    internal sealed class SimpleType : ISerializable
    {
        private SimpleType baseSimpleType;
        private string baseType;
        internal string enumeration;
        private int length;
        private string maxExclusive;
        private string maxInclusive;
        private int maxLength;
        private string minExclusive;
        private string minInclusive;
        private int minLength;
        private string name;
        private string ns;
        private string pattern;
        private XmlQualifiedName xmlBaseType;

        internal SimpleType(string baseType)
        {
            this.name = "";
            this.length = -1;
            this.minLength = -1;
            this.maxLength = -1;
            this.pattern = "";
            this.ns = "";
            this.maxExclusive = "";
            this.maxInclusive = "";
            this.minExclusive = "";
            this.minInclusive = "";
            this.enumeration = "";
            this.baseType = baseType;
        }

        internal SimpleType(XmlSchemaSimpleType node)
        {
            this.name = "";
            this.length = -1;
            this.minLength = -1;
            this.maxLength = -1;
            this.pattern = "";
            this.ns = "";
            this.maxExclusive = "";
            this.maxInclusive = "";
            this.minExclusive = "";
            this.minInclusive = "";
            this.enumeration = "";
            this.name = node.Name;
            this.ns = (node.QualifiedName != null) ? node.QualifiedName.Namespace : "";
            this.LoadTypeValues(node);
        }

        private SimpleType(SerializationInfo info, StreamingContext context)
        {
            this.name = "";
            this.length = -1;
            this.minLength = -1;
            this.maxLength = -1;
            this.pattern = "";
            this.ns = "";
            this.maxExclusive = "";
            this.maxInclusive = "";
            this.minExclusive = "";
            this.minInclusive = "";
            this.enumeration = "";
            this.baseType = info.GetString("SimpleType.BaseType");
            this.baseSimpleType = (SimpleType) info.GetValue("SimpleType.BaseSimpleType", typeof(SimpleType));
            if (info.GetBoolean("SimpleType.XmlBaseType.XmlQualifiedNameExists"))
            {
                string name = info.GetString("SimpleType.XmlBaseType.Name");
                string ns = info.GetString("SimpleType.XmlBaseType.Namespace");
                this.xmlBaseType = new XmlQualifiedName(name, ns);
            }
            else
            {
                this.xmlBaseType = null;
            }
            this.name = info.GetString("SimpleType.Name");
            this.ns = info.GetString("SimpleType.NS");
            this.maxLength = info.GetInt32("SimpleType.MaxLength");
            this.length = info.GetInt32("SimpleType.Length");
        }

        internal bool CanHaveMaxLength()
        {
            SimpleType baseSimpleType = this;
            while (baseSimpleType.BaseSimpleType != null)
            {
                baseSimpleType = baseSimpleType.BaseSimpleType;
            }
            return (string.Compare(baseSimpleType.BaseType, "string", StringComparison.OrdinalIgnoreCase) == 0);
        }

        internal void ConvertToAnnonymousSimpleType()
        {
            this.name = null;
            this.ns = string.Empty;
            SimpleType baseSimpleType = this;
            while (baseSimpleType.baseSimpleType != null)
            {
                baseSimpleType = baseSimpleType.baseSimpleType;
            }
            this.baseType = baseSimpleType.baseType;
            this.baseSimpleType = baseSimpleType.baseSimpleType;
            this.xmlBaseType = baseSimpleType.xmlBaseType;
        }

        internal static SimpleType CreateByteArrayType(string encoding)
        {
            return new SimpleType("base64Binary");
        }

        internal static SimpleType CreateEnumeratedType(string values)
        {
            return new SimpleType("string") { enumeration = values };
        }

        internal static SimpleType CreateLimitedStringType(int length)
        {
            return new SimpleType("string") { maxLength = length };
        }

        internal static SimpleType CreateSimpleType(Type type)
        {
            SimpleType type2 = null;
            if (type == typeof(char))
            {
                type2 = new SimpleType("string") {
                    length = 1
                };
            }
            return type2;
        }

        internal string HasConflictingDefinition(SimpleType otherSimpleType)
        {
            if (otherSimpleType == null)
            {
                return "otherSimpleType";
            }
            if (this.MaxLength != otherSimpleType.MaxLength)
            {
                return "MaxLength";
            }
            if (string.Compare(this.BaseType, otherSimpleType.BaseType, StringComparison.Ordinal) != 0)
            {
                return "BaseType";
            }
            if (((this.BaseSimpleType == null) && (otherSimpleType.BaseSimpleType != null)) && (this.BaseSimpleType.HasConflictingDefinition(otherSimpleType.BaseSimpleType).Length != 0))
            {
                return "BaseSimpleType";
            }
            return string.Empty;
        }

        internal bool IsPlainString()
        {
            return ((((((XSDSchema.QualifiedName(this.baseType) == XSDSchema.QualifiedName("string")) && ADP.IsEmpty(this.name)) && ((this.length == -1) && (this.minLength == -1))) && (((this.maxLength == -1) && ADP.IsEmpty(this.pattern)) && (ADP.IsEmpty(this.maxExclusive) && ADP.IsEmpty(this.maxInclusive)))) && (ADP.IsEmpty(this.minExclusive) && ADP.IsEmpty(this.minInclusive))) && ADP.IsEmpty(this.enumeration));
        }

        internal void LoadTypeValues(XmlSchemaSimpleType node)
        {
            if ((node.Content is XmlSchemaSimpleTypeList) || (node.Content is XmlSchemaSimpleTypeUnion))
            {
                throw ExceptionBuilder.SimpleTypeNotSupported();
            }
            if (node.Content is XmlSchemaSimpleTypeRestriction)
            {
                XmlSchemaSimpleTypeRestriction content = (XmlSchemaSimpleTypeRestriction) node.Content;
                XmlSchemaSimpleType baseXmlSchemaType = node.BaseXmlSchemaType as XmlSchemaSimpleType;
                if ((baseXmlSchemaType != null) && (baseXmlSchemaType.QualifiedName.Namespace != "http://www.w3.org/2001/XMLSchema"))
                {
                    this.baseSimpleType = new SimpleType(node.BaseXmlSchemaType as XmlSchemaSimpleType);
                }
                if (content.BaseTypeName.Namespace == "http://www.w3.org/2001/XMLSchema")
                {
                    this.baseType = content.BaseTypeName.Name;
                }
                else
                {
                    this.baseType = content.BaseTypeName.ToString();
                }
                if (((this.baseSimpleType != null) && (this.baseSimpleType.Name != null)) && (this.baseSimpleType.Name.Length > 0))
                {
                    this.xmlBaseType = this.baseSimpleType.XmlBaseType;
                }
                else
                {
                    this.xmlBaseType = content.BaseTypeName;
                }
                if ((this.baseType == null) || (this.baseType.Length == 0))
                {
                    this.baseType = content.BaseType.Name;
                    this.xmlBaseType = null;
                }
                if (this.baseType == "NOTATION")
                {
                    this.baseType = "string";
                }
                foreach (XmlSchemaFacet facet in content.Facets)
                {
                    if (facet is XmlSchemaLengthFacet)
                    {
                        this.length = Convert.ToInt32(facet.Value, (IFormatProvider) null);
                    }
                    if (facet is XmlSchemaMinLengthFacet)
                    {
                        this.minLength = Convert.ToInt32(facet.Value, (IFormatProvider) null);
                    }
                    if (facet is XmlSchemaMaxLengthFacet)
                    {
                        this.maxLength = Convert.ToInt32(facet.Value, (IFormatProvider) null);
                    }
                    if (facet is XmlSchemaPatternFacet)
                    {
                        this.pattern = facet.Value;
                    }
                    if (facet is XmlSchemaEnumerationFacet)
                    {
                        this.enumeration = !ADP.IsEmpty(this.enumeration) ? (this.enumeration + " " + facet.Value) : facet.Value;
                    }
                    if (facet is XmlSchemaMinExclusiveFacet)
                    {
                        this.minExclusive = facet.Value;
                    }
                    if (facet is XmlSchemaMinInclusiveFacet)
                    {
                        this.minInclusive = facet.Value;
                    }
                    if (facet is XmlSchemaMaxExclusiveFacet)
                    {
                        this.maxExclusive = facet.Value;
                    }
                    if (facet is XmlSchemaMaxInclusiveFacet)
                    {
                        this.maxInclusive = facet.Value;
                    }
                }
            }
            string msdataAttribute = XSDSchema.GetMsdataAttribute(node, "targetNamespace");
            if (msdataAttribute != null)
            {
                this.ns = msdataAttribute;
            }
        }

        internal string QualifiedName(string name)
        {
            if (name.IndexOf(':') == -1)
            {
                return ("xs:" + name);
            }
            return name;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SimpleType.BaseType", this.baseType);
            info.AddValue("SimpleType.BaseSimpleType", this.baseSimpleType);
            XmlQualifiedName xmlBaseType = this.xmlBaseType;
            info.AddValue("SimpleType.XmlBaseType.XmlQualifiedNameExists", xmlBaseType != null);
            info.AddValue("SimpleType.XmlBaseType.Name", (xmlBaseType != null) ? xmlBaseType.Name : null);
            info.AddValue("SimpleType.XmlBaseType.Namespace", (xmlBaseType != null) ? xmlBaseType.Namespace : null);
            info.AddValue("SimpleType.Name", this.name);
            info.AddValue("SimpleType.NS", this.ns);
            info.AddValue("SimpleType.MaxLength", this.maxLength);
            info.AddValue("SimpleType.Length", this.length);
        }

        internal XmlNode ToNode(XmlDocument dc, Hashtable prefixes, bool inRemoting)
        {
            XmlElement element2;
            XmlElement element3 = dc.CreateElement("xs", "simpleType", "http://www.w3.org/2001/XMLSchema");
            if ((this.name != null) && (this.name.Length != 0))
            {
                element3.SetAttribute("name", this.name);
                if (inRemoting)
                {
                    element3.SetAttribute("targetNamespace", "urn:schemas-microsoft-com:xml-msdata", this.Namespace);
                }
            }
            XmlElement newChild = dc.CreateElement("xs", "restriction", "http://www.w3.org/2001/XMLSchema");
            if (!inRemoting)
            {
                if (this.baseSimpleType != null)
                {
                    if ((this.baseSimpleType.Namespace != null) && (this.baseSimpleType.Namespace.Length > 0))
                    {
                        string str = (prefixes != null) ? ((string) prefixes[this.baseSimpleType.Namespace]) : null;
                        if (str != null)
                        {
                            newChild.SetAttribute("base", str + ":" + this.baseSimpleType.Name);
                        }
                        else
                        {
                            newChild.SetAttribute("base", this.baseSimpleType.Name);
                        }
                    }
                    else
                    {
                        newChild.SetAttribute("base", this.baseSimpleType.Name);
                    }
                }
                else
                {
                    newChild.SetAttribute("base", this.QualifiedName(this.baseType));
                }
            }
            else
            {
                newChild.SetAttribute("base", (this.baseSimpleType != null) ? this.baseSimpleType.Name : this.QualifiedName(this.baseType));
            }
            if (this.length >= 0)
            {
                element2 = dc.CreateElement("xs", "length", "http://www.w3.org/2001/XMLSchema");
                element2.SetAttribute("value", this.length.ToString(CultureInfo.InvariantCulture));
                newChild.AppendChild(element2);
            }
            if (this.maxLength >= 0)
            {
                element2 = dc.CreateElement("xs", "maxLength", "http://www.w3.org/2001/XMLSchema");
                element2.SetAttribute("value", this.maxLength.ToString(CultureInfo.InvariantCulture));
                newChild.AppendChild(element2);
            }
            element3.AppendChild(newChild);
            return element3;
        }

        internal SimpleType BaseSimpleType
        {
            get
            {
                return this.baseSimpleType;
            }
        }

        internal string BaseType
        {
            get
            {
                return this.baseType;
            }
        }

        internal int Length
        {
            get
            {
                return this.length;
            }
        }

        internal int MaxLength
        {
            get
            {
                return this.maxLength;
            }
            set
            {
                this.maxLength = value;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
        }

        internal string Namespace
        {
            get
            {
                return this.ns;
            }
        }

        public string SimpleTypeQualifiedName
        {
            get
            {
                if (this.ns.Length == 0)
                {
                    return this.name;
                }
                return (this.ns + ":" + this.name);
            }
        }

        internal XmlQualifiedName XmlBaseType
        {
            get
            {
                return this.xmlBaseType;
            }
        }
    }
}

