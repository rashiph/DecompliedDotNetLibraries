namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Xml;

    public abstract class XmlSchemaDatatype
    {
        protected XmlSchemaDatatype()
        {
        }

        public virtual object ChangeType(object value, Type targetType)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            return this.ValueConverter.ChangeType(value, targetType);
        }

        public virtual object ChangeType(object value, Type targetType, IXmlNamespaceResolver namespaceResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (namespaceResolver == null)
            {
                throw new ArgumentNullException("namespaceResolver");
            }
            return this.ValueConverter.ChangeType(value, targetType, namespaceResolver);
        }

        internal abstract int Compare(object value1, object value2);
        internal static string ConcatenatedToString(object value)
        {
            Type type = value.GetType();
            string str = string.Empty;
            if ((type == typeof(IEnumerable)) && (type != typeof(string)))
            {
                StringBuilder builder = new StringBuilder();
                IEnumerator enumerator = (value as IEnumerable).GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    return str;
                }
                builder.Append("{");
                object current = enumerator.Current;
                if (current is IFormattable)
                {
                    builder.Append(((IFormattable) current).ToString("", CultureInfo.InvariantCulture));
                }
                else
                {
                    builder.Append(current.ToString());
                }
                while (enumerator.MoveNext())
                {
                    builder.Append(" , ");
                    current = enumerator.Current;
                    if (current is IFormattable)
                    {
                        builder.Append(((IFormattable) current).ToString("", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(current.ToString());
                    }
                }
                builder.Append("}");
                return builder.ToString();
            }
            if (value is IFormattable)
            {
                return ((IFormattable) value).ToString("", CultureInfo.InvariantCulture);
            }
            return value.ToString();
        }

        internal abstract XmlSchemaDatatype DeriveByList(XmlSchemaType schemaType);
        internal abstract XmlSchemaDatatype DeriveByRestriction(XmlSchemaObjectCollection facets, XmlNameTable nameTable, XmlSchemaType schemaType);
        internal static XmlSchemaDatatype DeriveByUnion(XmlSchemaSimpleType[] types, XmlSchemaType schemaType)
        {
            return DatatypeImplementation.DeriveByUnion(types, schemaType);
        }

        internal static XmlSchemaDatatype FromXdrName(string name)
        {
            return DatatypeImplementation.FromXdrName(name);
        }

        internal static XmlSchemaDatatype FromXmlTokenizedType(XmlTokenizedType token)
        {
            return DatatypeImplementation.FromXmlTokenizedType(token);
        }

        internal static XmlSchemaDatatype FromXmlTokenizedTypeXsd(XmlTokenizedType token)
        {
            return DatatypeImplementation.FromXmlTokenizedTypeXsd(token);
        }

        internal abstract bool IsComparable(XmlSchemaDatatype dtype);
        public virtual bool IsDerivedFrom(XmlSchemaDatatype datatype)
        {
            return false;
        }

        internal abstract bool IsEqual(object o1, object o2);
        public abstract object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr);
        internal abstract object ParseValue(string s, Type typDest, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr);
        internal abstract object ParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, bool createAtomicValue);
        internal abstract Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver namespaceResolver, out object typedValue);
        internal abstract Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue);
        internal string TypeCodeToString(XmlTypeCode typeCode)
        {
            switch (typeCode)
            {
                case XmlTypeCode.None:
                    return "None";

                case XmlTypeCode.Item:
                    return "AnyType";

                case XmlTypeCode.AnyAtomicType:
                    return "AnyAtomicType";

                case XmlTypeCode.String:
                    return "String";

                case XmlTypeCode.Boolean:
                    return "Boolean";

                case XmlTypeCode.Decimal:
                    return "Decimal";

                case XmlTypeCode.Float:
                    return "Float";

                case XmlTypeCode.Double:
                    return "Double";

                case XmlTypeCode.Duration:
                    return "Duration";

                case XmlTypeCode.DateTime:
                    return "DateTime";

                case XmlTypeCode.Time:
                    return "Time";

                case XmlTypeCode.Date:
                    return "Date";

                case XmlTypeCode.GYearMonth:
                    return "GYearMonth";

                case XmlTypeCode.GYear:
                    return "GYear";

                case XmlTypeCode.GMonthDay:
                    return "GMonthDay";

                case XmlTypeCode.GDay:
                    return "GDay";

                case XmlTypeCode.GMonth:
                    return "GMonth";

                case XmlTypeCode.HexBinary:
                    return "HexBinary";

                case XmlTypeCode.Base64Binary:
                    return "Base64Binary";

                case XmlTypeCode.AnyUri:
                    return "AnyUri";

                case XmlTypeCode.QName:
                    return "QName";

                case XmlTypeCode.Notation:
                    return "Notation";

                case XmlTypeCode.NormalizedString:
                    return "NormalizedString";

                case XmlTypeCode.Token:
                    return "Token";

                case XmlTypeCode.Language:
                    return "Language";

                case XmlTypeCode.NmToken:
                    return "NmToken";

                case XmlTypeCode.Name:
                    return "Name";

                case XmlTypeCode.NCName:
                    return "NCName";

                case XmlTypeCode.Id:
                    return "Id";

                case XmlTypeCode.Idref:
                    return "Idref";

                case XmlTypeCode.Entity:
                    return "Entity";

                case XmlTypeCode.Integer:
                    return "Integer";

                case XmlTypeCode.NonPositiveInteger:
                    return "NonPositiveInteger";

                case XmlTypeCode.NegativeInteger:
                    return "NegativeInteger";

                case XmlTypeCode.Long:
                    return "Long";

                case XmlTypeCode.Int:
                    return "Int";

                case XmlTypeCode.Short:
                    return "Short";

                case XmlTypeCode.Byte:
                    return "Byte";

                case XmlTypeCode.NonNegativeInteger:
                    return "NonNegativeInteger";

                case XmlTypeCode.UnsignedLong:
                    return "UnsignedLong";

                case XmlTypeCode.UnsignedInt:
                    return "UnsignedInt";

                case XmlTypeCode.UnsignedShort:
                    return "UnsignedShort";

                case XmlTypeCode.UnsignedByte:
                    return "UnsignedByte";

                case XmlTypeCode.PositiveInteger:
                    return "PositiveInteger";
            }
            return typeCode.ToString();
        }

        internal abstract void VerifySchemaValid(XmlSchemaObjectTable notations, XmlSchemaObject caller);
        internal static string XdrCanonizeUri(string uri, XmlNameTable nameTable, SchemaNames schemaNames)
        {
            string nsXdr;
            int length = 5;
            bool flag = false;
            if ((uri.Length > 5) && uri.StartsWith("uuid:", StringComparison.Ordinal))
            {
                flag = true;
            }
            else if ((uri.Length > 9) && uri.StartsWith("urn:uuid:", StringComparison.Ordinal))
            {
                flag = true;
                length = 9;
            }
            if (flag)
            {
                nsXdr = nameTable.Add(uri.Substring(0, length) + uri.Substring(length, uri.Length - length).ToUpper(CultureInfo.InvariantCulture));
            }
            else
            {
                nsXdr = uri;
            }
            if (Ref.Equal(schemaNames.NsDataTypeAlias, nsXdr) || Ref.Equal(schemaNames.NsDataTypeOld, nsXdr))
            {
                return schemaNames.NsDataType;
            }
            if (Ref.Equal(schemaNames.NsXdrAlias, nsXdr))
            {
                nsXdr = schemaNames.NsXdr;
            }
            return nsXdr;
        }

        internal abstract XmlSchemaWhiteSpace BuiltInWhitespaceFacet { get; }

        internal abstract System.Xml.Schema.FacetsChecker FacetsChecker { get; }

        internal abstract bool HasLexicalFacets { get; }

        internal abstract bool HasValueFacets { get; }

        internal abstract RestrictionFacets Restriction { get; set; }

        public abstract XmlTokenizedType TokenizedType { get; }

        public virtual XmlTypeCode TypeCode
        {
            get
            {
                return XmlTypeCode.None;
            }
        }

        internal string TypeCodeString
        {
            get
            {
                string str = string.Empty;
                XmlTypeCode typeCode = this.TypeCode;
                switch (this.Variety)
                {
                    case XmlSchemaDatatypeVariety.Atomic:
                        if (typeCode != XmlTypeCode.AnyAtomicType)
                        {
                            return this.TypeCodeToString(typeCode);
                        }
                        return "anySimpleType";

                    case XmlSchemaDatatypeVariety.List:
                        if (typeCode != XmlTypeCode.AnyAtomicType)
                        {
                            return ("List of " + this.TypeCodeToString(typeCode));
                        }
                        return "List of Union";

                    case XmlSchemaDatatypeVariety.Union:
                        return "Union";
                }
                return str;
            }
        }

        internal abstract XmlValueConverter ValueConverter { get; }

        public abstract Type ValueType { get; }

        public virtual XmlSchemaDatatypeVariety Variety
        {
            get
            {
                return XmlSchemaDatatypeVariety.Atomic;
            }
        }
    }
}

