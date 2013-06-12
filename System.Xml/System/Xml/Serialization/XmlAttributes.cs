namespace System.Xml.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public class XmlAttributes
    {
        private static Type ignoreAttributeType;
        private XmlAnyAttributeAttribute xmlAnyAttribute;
        private XmlAnyElementAttributes xmlAnyElements;
        private XmlArrayAttribute xmlArray;
        private XmlArrayItemAttributes xmlArrayItems;
        private XmlAttributeAttribute xmlAttribute;
        private XmlChoiceIdentifierAttribute xmlChoiceIdentifier;
        private object xmlDefaultValue;
        private XmlElementAttributes xmlElements;
        private XmlEnumAttribute xmlEnum;
        private bool xmlIgnore;
        private bool xmlns;
        private XmlRootAttribute xmlRoot;
        private XmlTextAttribute xmlText;
        private XmlTypeAttribute xmlType;

        public XmlAttributes()
        {
            this.xmlElements = new XmlElementAttributes();
            this.xmlArrayItems = new XmlArrayItemAttributes();
            this.xmlAnyElements = new XmlAnyElementAttributes();
        }

        public XmlAttributes(ICustomAttributeProvider provider)
        {
            this.xmlElements = new XmlElementAttributes();
            this.xmlArrayItems = new XmlArrayItemAttributes();
            this.xmlAnyElements = new XmlAnyElementAttributes();
            object[] customAttributes = provider.GetCustomAttributes(false);
            XmlAnyElementAttribute attribute = null;
            for (int i = 0; i < customAttributes.Length; i++)
            {
                if (((customAttributes[i] is XmlIgnoreAttribute) || (customAttributes[i] is ObsoleteAttribute)) || (customAttributes[i].GetType() == IgnoreAttribute))
                {
                    this.xmlIgnore = true;
                    break;
                }
                if (customAttributes[i] is XmlElementAttribute)
                {
                    this.xmlElements.Add((XmlElementAttribute) customAttributes[i]);
                }
                else if (customAttributes[i] is XmlArrayItemAttribute)
                {
                    this.xmlArrayItems.Add((XmlArrayItemAttribute) customAttributes[i]);
                }
                else if (customAttributes[i] is XmlAnyElementAttribute)
                {
                    XmlAnyElementAttribute attribute2 = (XmlAnyElementAttribute) customAttributes[i];
                    if (((attribute2.Name == null) || (attribute2.Name.Length == 0)) && (attribute2.NamespaceSpecified && (attribute2.Namespace == null)))
                    {
                        attribute = attribute2;
                    }
                    else
                    {
                        this.xmlAnyElements.Add((XmlAnyElementAttribute) customAttributes[i]);
                    }
                }
                else if (customAttributes[i] is DefaultValueAttribute)
                {
                    this.xmlDefaultValue = ((DefaultValueAttribute) customAttributes[i]).Value;
                }
                else if (customAttributes[i] is XmlAttributeAttribute)
                {
                    this.xmlAttribute = (XmlAttributeAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlArrayAttribute)
                {
                    this.xmlArray = (XmlArrayAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlTextAttribute)
                {
                    this.xmlText = (XmlTextAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlEnumAttribute)
                {
                    this.xmlEnum = (XmlEnumAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlRootAttribute)
                {
                    this.xmlRoot = (XmlRootAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlTypeAttribute)
                {
                    this.xmlType = (XmlTypeAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlAnyAttributeAttribute)
                {
                    this.xmlAnyAttribute = (XmlAnyAttributeAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlChoiceIdentifierAttribute)
                {
                    this.xmlChoiceIdentifier = (XmlChoiceIdentifierAttribute) customAttributes[i];
                }
                else if (customAttributes[i] is XmlNamespaceDeclarationsAttribute)
                {
                    this.xmlns = true;
                }
            }
            if (this.xmlIgnore)
            {
                this.xmlElements.Clear();
                this.xmlArrayItems.Clear();
                this.xmlAnyElements.Clear();
                this.xmlDefaultValue = null;
                this.xmlAttribute = null;
                this.xmlArray = null;
                this.xmlText = null;
                this.xmlEnum = null;
                this.xmlType = null;
                this.xmlAnyAttribute = null;
                this.xmlChoiceIdentifier = null;
                this.xmlns = false;
            }
            else if (attribute != null)
            {
                this.xmlAnyElements.Add(attribute);
            }
        }

        internal static object GetAttr(ICustomAttributeProvider provider, Type attrType)
        {
            object[] customAttributes = provider.GetCustomAttributes(attrType, false);
            if (customAttributes.Length == 0)
            {
                return null;
            }
            return customAttributes[0];
        }

        private static Type IgnoreAttribute
        {
            get
            {
                if (ignoreAttributeType == null)
                {
                    ignoreAttributeType = typeof(object).Assembly.GetType("System.XmlIgnoreMemberAttribute");
                    if (ignoreAttributeType == null)
                    {
                        ignoreAttributeType = typeof(XmlIgnoreAttribute);
                    }
                }
                return ignoreAttributeType;
            }
        }

        public XmlAnyAttributeAttribute XmlAnyAttribute
        {
            get
            {
                return this.xmlAnyAttribute;
            }
            set
            {
                this.xmlAnyAttribute = value;
            }
        }

        public XmlAnyElementAttributes XmlAnyElements
        {
            get
            {
                return this.xmlAnyElements;
            }
        }

        public XmlArrayAttribute XmlArray
        {
            get
            {
                return this.xmlArray;
            }
            set
            {
                this.xmlArray = value;
            }
        }

        public XmlArrayItemAttributes XmlArrayItems
        {
            get
            {
                return this.xmlArrayItems;
            }
        }

        public XmlAttributeAttribute XmlAttribute
        {
            get
            {
                return this.xmlAttribute;
            }
            set
            {
                this.xmlAttribute = value;
            }
        }

        public XmlChoiceIdentifierAttribute XmlChoiceIdentifier
        {
            get
            {
                return this.xmlChoiceIdentifier;
            }
        }

        public object XmlDefaultValue
        {
            get
            {
                return this.xmlDefaultValue;
            }
            set
            {
                this.xmlDefaultValue = value;
            }
        }

        public XmlElementAttributes XmlElements
        {
            get
            {
                return this.xmlElements;
            }
        }

        public XmlEnumAttribute XmlEnum
        {
            get
            {
                return this.xmlEnum;
            }
            set
            {
                this.xmlEnum = value;
            }
        }

        internal XmlAttributeFlags XmlFlags
        {
            get
            {
                XmlAttributeFlags flags = (XmlAttributeFlags) 0;
                if (this.xmlElements.Count > 0)
                {
                    flags |= XmlAttributeFlags.Elements;
                }
                if (this.xmlArrayItems.Count > 0)
                {
                    flags |= XmlAttributeFlags.ArrayItems;
                }
                if (this.xmlAnyElements.Count > 0)
                {
                    flags |= XmlAttributeFlags.AnyElements;
                }
                if (this.xmlArray != null)
                {
                    flags |= XmlAttributeFlags.Array;
                }
                if (this.xmlAttribute != null)
                {
                    flags |= XmlAttributeFlags.Attribute;
                }
                if (this.xmlText != null)
                {
                    flags |= XmlAttributeFlags.Text;
                }
                if (this.xmlEnum != null)
                {
                    flags |= XmlAttributeFlags.Enum;
                }
                if (this.xmlRoot != null)
                {
                    flags |= XmlAttributeFlags.Root;
                }
                if (this.xmlType != null)
                {
                    flags |= XmlAttributeFlags.Type;
                }
                if (this.xmlAnyAttribute != null)
                {
                    flags |= XmlAttributeFlags.AnyAttribute;
                }
                if (this.xmlChoiceIdentifier != null)
                {
                    flags |= XmlAttributeFlags.ChoiceIdentifier;
                }
                if (this.xmlns)
                {
                    flags |= XmlAttributeFlags.XmlnsDeclarations;
                }
                return flags;
            }
        }

        public bool XmlIgnore
        {
            get
            {
                return this.xmlIgnore;
            }
            set
            {
                this.xmlIgnore = value;
            }
        }

        public bool Xmlns
        {
            get
            {
                return this.xmlns;
            }
            set
            {
                this.xmlns = value;
            }
        }

        public XmlRootAttribute XmlRoot
        {
            get
            {
                return this.xmlRoot;
            }
            set
            {
                this.xmlRoot = value;
            }
        }

        public XmlTextAttribute XmlText
        {
            get
            {
                return this.xmlText;
            }
            set
            {
                this.xmlText = value;
            }
        }

        public XmlTypeAttribute XmlType
        {
            get
            {
                return this.xmlType;
            }
            set
            {
                this.xmlType = value;
            }
        }
    }
}

