namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal class Datatype_List : Datatype_anySimpleType
    {
        private DatatypeImplementation itemType;
        private int minListSize;

        internal Datatype_List(DatatypeImplementation type) : this(type, 0)
        {
        }

        internal Datatype_List(DatatypeImplementation type, int minListSize)
        {
            this.itemType = type;
            this.minListSize = minListSize;
        }

        internal override int Compare(object value1, object value2)
        {
            Array array = (Array) value1;
            Array array2 = (Array) value2;
            if (array.Length != array2.Length)
            {
                return -1;
            }
            XmlAtomicValue[] valueArray = array as XmlAtomicValue[];
            if (valueArray != null)
            {
                XmlAtomicValue[] valueArray2 = array2 as XmlAtomicValue[];
                for (int j = 0; j < valueArray.Length; j++)
                {
                    XmlSchemaType xmlType = valueArray[j].XmlType;
                    if ((xmlType != valueArray2[j].XmlType) || !xmlType.Datatype.IsEqual(valueArray[j].TypedValue, valueArray2[j].TypedValue))
                    {
                        return -1;
                    }
                }
                return 0;
            }
            for (int i = 0; i < array.Length; i++)
            {
                if (this.itemType.Compare(array.GetValue(i), array2.GetValue(i)) != 0)
                {
                    return -1;
                }
            }
            return 0;
        }

        internal override XmlValueConverter CreateValueConverter(XmlSchemaType schemaType)
        {
            XmlSchemaType baseItemType = null;
            XmlSchemaSimpleType baseXmlSchemaType;
            XmlSchemaComplexType type3 = schemaType as XmlSchemaComplexType;
            if (type3 != null)
            {
                do
                {
                    baseXmlSchemaType = type3.BaseXmlSchemaType as XmlSchemaSimpleType;
                    if (baseXmlSchemaType != null)
                    {
                        break;
                    }
                    type3 = type3.BaseXmlSchemaType as XmlSchemaComplexType;
                    if (type3 == null)
                    {
                        break;
                    }
                }
                while (type3 != XmlSchemaComplexType.AnyType);
            }
            else
            {
                baseXmlSchemaType = schemaType as XmlSchemaSimpleType;
            }
            if (baseXmlSchemaType != null)
            {
                do
                {
                    XmlSchemaSimpleTypeList content = baseXmlSchemaType.Content as XmlSchemaSimpleTypeList;
                    if (content != null)
                    {
                        baseItemType = content.BaseItemType;
                        break;
                    }
                    baseXmlSchemaType = baseXmlSchemaType.BaseXmlSchemaType as XmlSchemaSimpleType;
                }
                while ((baseXmlSchemaType != null) && (baseXmlSchemaType != DatatypeImplementation.AnySimpleType));
            }
            if (baseItemType == null)
            {
                baseItemType = DatatypeImplementation.GetSimpleTypeFromTypeCode(schemaType.Datatype.TypeCode);
            }
            return XmlListConverter.Create(baseItemType.ValueConverter);
        }

        internal override Exception TryParseValue(object value, XmlNameTable nameTable, IXmlNamespaceResolver namespaceResolver, out object typedValue)
        {
            Exception exception;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            string s = value as string;
            typedValue = null;
            if (s != null)
            {
                return this.TryParseValue(s, nameTable, namespaceResolver, out typedValue);
            }
            try
            {
                object obj2 = this.ValueConverter.ChangeType(value, this.ValueType, namespaceResolver);
                Array array = obj2 as Array;
                bool hasLexicalFacets = this.itemType.HasLexicalFacets;
                bool hasValueFacets = this.itemType.HasValueFacets;
                System.Xml.Schema.FacetsChecker facetsChecker = this.itemType.FacetsChecker;
                XmlValueConverter valueConverter = this.itemType.ValueConverter;
                for (int i = 0; i < array.Length; i++)
                {
                    object obj3 = array.GetValue(i);
                    if (hasLexicalFacets)
                    {
                        string parseString = (string) valueConverter.ChangeType(obj3, typeof(string), namespaceResolver);
                        exception = facetsChecker.CheckLexicalFacets(ref parseString, this.itemType);
                        if (exception != null)
                        {
                            return exception;
                        }
                    }
                    if (hasValueFacets)
                    {
                        exception = facetsChecker.CheckValueFacets(obj3, this.itemType);
                        if (exception != null)
                        {
                            return exception;
                        }
                    }
                }
                if (this.HasLexicalFacets)
                {
                    string str3 = (string) this.ValueConverter.ChangeType(obj2, typeof(string), namespaceResolver);
                    exception = DatatypeImplementation.listFacetsChecker.CheckLexicalFacets(ref str3, this);
                    if (exception != null)
                    {
                        return exception;
                    }
                }
                if (this.HasValueFacets)
                {
                    exception = DatatypeImplementation.listFacetsChecker.CheckValueFacets(obj2, this);
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

        internal override Exception TryParseValue(string s, XmlNameTable nameTable, IXmlNamespaceResolver nsmgr, out object typedValue)
        {
            typedValue = null;
            Exception exception = DatatypeImplementation.listFacetsChecker.CheckLexicalFacets(ref s, this);
            if (exception == null)
            {
                object obj2;
                ArrayList list = new ArrayList();
                if (this.itemType.Variety == XmlSchemaDatatypeVariety.Union)
                {
                    string[] strArray = XmlConvert.SplitString(s);
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        object obj3;
                        exception = this.itemType.TryParseValue(strArray[i], nameTable, nsmgr, out obj3);
                        if (exception != null)
                        {
                            return exception;
                        }
                        XsdSimpleValue value2 = (XsdSimpleValue) obj3;
                        list.Add(new XmlAtomicValue(value2.XmlType, value2.TypedValue, nsmgr));
                    }
                    obj2 = list.ToArray(typeof(XmlAtomicValue));
                }
                else
                {
                    string[] strArray2 = XmlConvert.SplitString(s);
                    for (int j = 0; j < strArray2.Length; j++)
                    {
                        exception = this.itemType.TryParseValue(strArray2[j], nameTable, nsmgr, out typedValue);
                        if (exception != null)
                        {
                            return exception;
                        }
                        list.Add(typedValue);
                    }
                    obj2 = list.ToArray(this.itemType.ValueType);
                }
                if (list.Count < this.minListSize)
                {
                    return new XmlSchemaException("Sch_EmptyAttributeValue", string.Empty);
                }
                exception = DatatypeImplementation.listFacetsChecker.CheckValueFacets(obj2, this);
                if (exception == null)
                {
                    typedValue = obj2;
                    return null;
                }
            }
            return exception;
        }

        internal override System.Xml.Schema.FacetsChecker FacetsChecker
        {
            get
            {
                return DatatypeImplementation.listFacetsChecker;
            }
        }

        internal DatatypeImplementation ItemType
        {
            get
            {
                return this.itemType;
            }
        }

        internal override Type ListValueType
        {
            get
            {
                return this.itemType.ListValueType;
            }
        }

        public override XmlTokenizedType TokenizedType
        {
            get
            {
                return this.itemType.TokenizedType;
            }
        }

        public override XmlTypeCode TypeCode
        {
            get
            {
                return this.itemType.TypeCode;
            }
        }

        internal override RestrictionFlags ValidRestrictionFlags
        {
            get
            {
                return (RestrictionFlags.WhiteSpace | RestrictionFlags.Enumeration | RestrictionFlags.Pattern | RestrictionFlags.MaxLength | RestrictionFlags.MinLength | RestrictionFlags.Length);
            }
        }

        public override Type ValueType
        {
            get
            {
                return this.ListValueType;
            }
        }
    }
}

