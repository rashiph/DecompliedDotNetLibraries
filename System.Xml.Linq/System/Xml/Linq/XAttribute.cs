namespace System.Xml.Linq
{
    using MS.Internal.Xml.Linq.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Xml;

    [TypeDescriptionProvider(typeof(XTypeDescriptionProvider<XAttribute>))]
    public class XAttribute : XObject
    {
        private static IEnumerable<XAttribute> emptySequence;
        internal XName name;
        internal XAttribute next;
        internal string value;

        public XAttribute(XAttribute other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }
            this.name = other.name;
            this.value = other.value;
        }

        public XAttribute(XName name, object value)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            string stringValue = XContainer.GetStringValue(value);
            ValidateAttribute(name, stringValue);
            this.name = name;
            this.value = stringValue;
        }

        internal int GetDeepHashCode()
        {
            return (this.name.GetHashCode() ^ this.value.GetHashCode());
        }

        internal string GetPrefixOfNamespace(XNamespace ns)
        {
            string namespaceName = ns.NamespaceName;
            if (namespaceName.Length == 0)
            {
                return string.Empty;
            }
            if (base.parent != null)
            {
                return ((XElement) base.parent).GetPrefixOfNamespace(ns);
            }
            switch (namespaceName)
            {
                case "http://www.w3.org/XML/1998/namespace":
                    return "xml";

                case "http://www.w3.org/2000/xmlns/":
                    return "xmlns";
            }
            return null;
        }

        [CLSCompliant(false)]
        public static explicit operator bool(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToBoolean(attribute.value.ToLower(CultureInfo.InvariantCulture));
        }

        [CLSCompliant(false)]
        public static explicit operator DateTime(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return DateTime.Parse(attribute.value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        }

        [CLSCompliant(false)]
        public static explicit operator DateTimeOffset(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToDateTimeOffset(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator decimal(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToDecimal(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator double(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToDouble(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator Guid(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToGuid(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator int(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToInt32(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator long(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToInt64(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator DateTime?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new DateTime?(DateTime.Parse(attribute.value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
        }

        [CLSCompliant(false)]
        public static explicit operator DateTimeOffset?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new DateTimeOffset?(XmlConvert.ToDateTimeOffset(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator decimal?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new decimal?(XmlConvert.ToDecimal(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator double?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new double?(XmlConvert.ToDouble(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator Guid?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new Guid?(XmlConvert.ToGuid(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator TimeSpan?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new TimeSpan?(XmlConvert.ToTimeSpan(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator TimeSpan(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToTimeSpan(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator bool?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new bool?(XmlConvert.ToBoolean(attribute.value.ToLower(CultureInfo.InvariantCulture)));
        }

        [CLSCompliant(false)]
        public static explicit operator int?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new int?(XmlConvert.ToInt32(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator string(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return attribute.value;
        }

        [CLSCompliant(false)]
        public static explicit operator long?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new long?(XmlConvert.ToInt64(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator float?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new float?(XmlConvert.ToSingle(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator uint(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToUInt32(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator uint?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new uint?(XmlConvert.ToUInt32(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator ulong(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToUInt64(attribute.value);
        }

        [CLSCompliant(false)]
        public static explicit operator ulong?(XAttribute attribute)
        {
            if (attribute == null)
            {
                return null;
            }
            return new ulong?(XmlConvert.ToUInt64(attribute.value));
        }

        [CLSCompliant(false)]
        public static explicit operator float(XAttribute attribute)
        {
            if (attribute == null)
            {
                throw new ArgumentNullException("attribute");
            }
            return XmlConvert.ToSingle(attribute.value);
        }

        public void Remove()
        {
            if (base.parent == null)
            {
                throw new InvalidOperationException(System.Xml.Linq.Res.GetString("InvalidOperation_MissingParent"));
            }
            ((XElement) base.parent).RemoveAttribute(this);
        }

        public void SetValue(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            this.Value = XContainer.GetStringValue(value);
        }

        public override string ToString()
        {
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                XmlWriterSettings settings = new XmlWriterSettings {
                    ConformanceLevel = ConformanceLevel.Fragment
                };
                using (XmlWriter writer2 = XmlWriter.Create(writer, settings))
                {
                    writer2.WriteAttributeString(this.GetPrefixOfNamespace(this.name.Namespace), this.name.LocalName, this.name.NamespaceName, this.value);
                }
                return writer.ToString().Trim();
            }
        }

        private static void ValidateAttribute(XName name, string value)
        {
            string namespaceName = name.NamespaceName;
            if (namespaceName == "http://www.w3.org/2000/xmlns/")
            {
                if (value.Length == 0)
                {
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationPrefixed", new object[] { name.LocalName }));
                }
                if (value != "http://www.w3.org/XML/1998/namespace")
                {
                    if (value == "http://www.w3.org/2000/xmlns/")
                    {
                        throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationXmlns"));
                    }
                    switch (name.LocalName)
                    {
                        case "xml":
                            throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationXml"));

                        case "xmlns":
                            throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationXmlns"));
                    }
                }
                else if (name.LocalName != "xml")
                {
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationXml"));
                }
            }
            else if ((namespaceName.Length == 0) && (name.LocalName == "xmlns"))
            {
                if (value == "http://www.w3.org/XML/1998/namespace")
                {
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationXml"));
                }
                if (value == "http://www.w3.org/2000/xmlns/")
                {
                    throw new ArgumentException(System.Xml.Linq.Res.GetString("Argument_NamespaceDeclarationXmlns"));
                }
            }
        }

        public static IEnumerable<XAttribute> EmptySequence
        {
            get
            {
                if (emptySequence == null)
                {
                    emptySequence = new XAttribute[0];
                }
                return emptySequence;
            }
        }

        public bool IsNamespaceDeclaration
        {
            get
            {
                string namespaceName = this.name.NamespaceName;
                if (namespaceName.Length == 0)
                {
                    return (this.name.LocalName == "xmlns");
                }
                return (namespaceName == "http://www.w3.org/2000/xmlns/");
            }
        }

        public XName Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
        }

        public XAttribute NextAttribute
        {
            get
            {
                if ((base.parent != null) && (((XElement) base.parent).lastAttr != this))
                {
                    return this.next;
                }
                return null;
            }
        }

        public override XmlNodeType NodeType
        {
            get
            {
                return XmlNodeType.Attribute;
            }
        }

        public XAttribute PreviousAttribute
        {
            get
            {
                if (base.parent == null)
                {
                    return null;
                }
                XAttribute lastAttr = ((XElement) base.parent).lastAttr;
                while (lastAttr.next != this)
                {
                    lastAttr = lastAttr.next;
                }
                if (lastAttr == ((XElement) base.parent).lastAttr)
                {
                    return null;
                }
                return lastAttr;
            }
        }

        public string Value
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.value;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                ValidateAttribute(this.name, value);
                bool flag = base.NotifyChanging(this, XObjectChangeEventArgs.Value);
                this.value = value;
                if (flag)
                {
                    base.NotifyChanged(this, XObjectChangeEventArgs.Value);
                }
            }
        }
    }
}

