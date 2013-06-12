namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;

    internal class XmlListConverter : XmlBaseConverter
    {
        protected XmlValueConverter atomicConverter;

        protected XmlListConverter(XmlBaseConverter atomicConverter) : base(atomicConverter)
        {
            this.atomicConverter = atomicConverter;
        }

        protected XmlListConverter(XmlSchemaType schemaType) : base(schemaType)
        {
        }

        protected XmlListConverter(XmlBaseConverter atomicConverter, Type clrTypeDefault) : base(atomicConverter, clrTypeDefault)
        {
            this.atomicConverter = atomicConverter;
        }

        protected override object ChangeListType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            Type sourceType = value.GetType();
            if (destinationType == XmlBaseConverter.ObjectType)
            {
                destinationType = base.DefaultClrType;
            }
            if (!(value is IEnumerable) || !this.IsListType(destinationType))
            {
                throw this.CreateInvalidClrMappingException(sourceType, destinationType);
            }
            if (destinationType == XmlBaseConverter.StringType)
            {
                if (sourceType == XmlBaseConverter.StringType)
                {
                    return value;
                }
                return this.ListAsString((IEnumerable) value, nsResolver);
            }
            if (sourceType == XmlBaseConverter.StringType)
            {
                value = this.StringAsList((string) value);
            }
            if (destinationType.IsArray)
            {
                Type elementType = destinationType.GetElementType();
                if (elementType == XmlBaseConverter.ObjectType)
                {
                    return this.ToArray<object>(value, nsResolver);
                }
                if (sourceType == destinationType)
                {
                    return value;
                }
                if (elementType == XmlBaseConverter.BooleanType)
                {
                    return this.ToArray<bool>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.ByteType)
                {
                    return this.ToArray<byte>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.ByteArrayType)
                {
                    return this.ToArray<byte[]>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.DateTimeType)
                {
                    return this.ToArray<DateTime>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.DateTimeOffsetType)
                {
                    return this.ToArray<DateTimeOffset>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.DecimalType)
                {
                    return this.ToArray<decimal>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.DoubleType)
                {
                    return this.ToArray<double>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.Int16Type)
                {
                    return this.ToArray<short>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.Int32Type)
                {
                    return this.ToArray<int>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.Int64Type)
                {
                    return this.ToArray<long>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.SByteType)
                {
                    return this.ToArray<sbyte>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.SingleType)
                {
                    return this.ToArray<float>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.StringType)
                {
                    return this.ToArray<string>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.TimeSpanType)
                {
                    return this.ToArray<TimeSpan>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.UInt16Type)
                {
                    return this.ToArray<ushort>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.UInt32Type)
                {
                    return this.ToArray<uint>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.UInt64Type)
                {
                    return this.ToArray<ulong>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.UriType)
                {
                    return this.ToArray<Uri>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.XmlAtomicValueType)
                {
                    return this.ToArray<XmlAtomicValue>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.XmlQualifiedNameType)
                {
                    return this.ToArray<XmlQualifiedName>(value, nsResolver);
                }
                if (elementType == XmlBaseConverter.XPathItemType)
                {
                    return this.ToArray<XPathItem>(value, nsResolver);
                }
                if (elementType != XmlBaseConverter.XPathNavigatorType)
                {
                    throw this.CreateInvalidClrMappingException(sourceType, destinationType);
                }
                return this.ToArray<XPathNavigator>(value, nsResolver);
            }
            if ((sourceType == base.DefaultClrType) && (sourceType != XmlBaseConverter.ObjectArrayType))
            {
                return value;
            }
            return this.ToList(value, nsResolver);
        }

        public override object ChangeType(object value, Type destinationType, IXmlNamespaceResolver nsResolver)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            return this.ChangeListType(value, destinationType, nsResolver);
        }

        public static XmlValueConverter Create(XmlValueConverter atomicConverter)
        {
            if (atomicConverter == XmlUntypedConverter.Untyped)
            {
                return XmlUntypedConverter.UntypedList;
            }
            if (atomicConverter == XmlAnyConverter.Item)
            {
                return XmlAnyListConverter.ItemList;
            }
            if (atomicConverter == XmlAnyConverter.AnyAtomic)
            {
                return XmlAnyListConverter.AnyAtomicList;
            }
            return new XmlListConverter((XmlBaseConverter) atomicConverter);
        }

        private Exception CreateInvalidClrMappingException(Type sourceType, Type destinationType)
        {
            if (sourceType == destinationType)
            {
                return new InvalidCastException(Res.GetString("XmlConvert_TypeListBadMapping", new object[] { base.XmlTypeName, sourceType.Name }));
            }
            return new InvalidCastException(Res.GetString("XmlConvert_TypeListBadMapping2", new object[] { base.XmlTypeName, sourceType.Name, destinationType.Name }));
        }

        private bool IsListType(Type type)
        {
            if ((!(type == XmlBaseConverter.IListType) && !(type == XmlBaseConverter.ICollectionType)) && (!(type == XmlBaseConverter.IEnumerableType) && !(type == XmlBaseConverter.StringType)))
            {
                return type.IsArray;
            }
            return true;
        }

        private string ListAsString(IEnumerable list, IXmlNamespaceResolver nsResolver)
        {
            StringBuilder builder = new StringBuilder();
            foreach (object obj2 in list)
            {
                if (obj2 != null)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(' ');
                    }
                    builder.Append(this.atomicConverter.ToString(obj2, nsResolver));
                }
            }
            return builder.ToString();
        }

        private List<string> StringAsList(string value)
        {
            return new List<string>(XmlConvert.SplitString(value));
        }

        private T[] ToArray<T>(object list, IXmlNamespaceResolver nsResolver)
        {
            IList list2 = list as IList;
            if (list2 != null)
            {
                T[] localArray = new T[list2.Count];
                for (int i = 0; i < list2.Count; i++)
                {
                    localArray[i] = (T) this.atomicConverter.ChangeType(list2[i], typeof(T), nsResolver);
                }
                return localArray;
            }
            IEnumerable enumerable = list as IEnumerable;
            List<T> list3 = new List<T>();
            foreach (object obj2 in enumerable)
            {
                list3.Add((T) this.atomicConverter.ChangeType(obj2, typeof(T), nsResolver));
            }
            return list3.ToArray();
        }

        private IList ToList(object list, IXmlNamespaceResolver nsResolver)
        {
            IList list2 = list as IList;
            if (list2 != null)
            {
                object[] objArray = new object[list2.Count];
                for (int i = 0; i < list2.Count; i++)
                {
                    objArray[i] = this.atomicConverter.ChangeType(list2[i], XmlBaseConverter.ObjectType, nsResolver);
                }
                return objArray;
            }
            IEnumerable enumerable = list as IEnumerable;
            List<object> list3 = new List<object>();
            foreach (object obj2 in enumerable)
            {
                list3.Add(this.atomicConverter.ChangeType(obj2, XmlBaseConverter.ObjectType, nsResolver));
            }
            return list3;
        }
    }
}

