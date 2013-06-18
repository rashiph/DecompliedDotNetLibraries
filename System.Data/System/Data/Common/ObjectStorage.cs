namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Dynamic;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class ObjectStorage : DataStorage
    {
        private static readonly XmlSerializerFactory _serializerFactory = new XmlSerializerFactory();
        private static Dictionary<KeyValuePair<Type, XmlRootAttribute>, XmlSerializer> _tempAssemblyCache;
        private static readonly object _tempAssemblyCacheLock = new object();
        private static readonly object defaultValue = null;
        private readonly bool implementsIXmlSerializable;
        private object[] values;

        internal ObjectStorage(DataColumn column, Type type) : base(column, type, defaultValue, DBNull.Value, typeof(ICloneable).IsAssignableFrom(type))
        {
            this.implementsIXmlSerializable = typeof(IXmlSerializable).IsAssignableFrom(type);
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            object obj3 = this.values[recordNo1];
            object obj2 = this.values[recordNo2];
            if (obj3 == obj2)
            {
                return 0;
            }
            if (obj3 == null)
            {
                return -1;
            }
            if (obj2 == null)
            {
                return 1;
            }
            IComparable comparable = obj3 as IComparable;
            if (comparable != null)
            {
                try
                {
                    return comparable.CompareTo(obj2);
                }
                catch (ArgumentException exception)
                {
                    ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                }
            }
            return this.CompareWithFamilies(obj3, obj2);
        }

        private int CompareTo(object valueNo1, object valueNo2)
        {
            if (valueNo1 == null)
            {
                return -1;
            }
            if (valueNo2 == null)
            {
                return 1;
            }
            if (valueNo1 == valueNo2)
            {
                return 0;
            }
            if (valueNo1 == base.NullValue)
            {
                return -1;
            }
            if (valueNo2 == base.NullValue)
            {
                return 1;
            }
            if (valueNo1 is IComparable)
            {
                try
                {
                    return ((IComparable) valueNo1).CompareTo(valueNo2);
                }
                catch (ArgumentException exception)
                {
                    ExceptionBuilder.TraceExceptionWithoutRethrow(exception);
                }
            }
            return this.CompareWithFamilies(valueNo1, valueNo2);
        }

        public override int CompareValueTo(int recordNo1, object value)
        {
            object obj2 = this.Get(recordNo1);
            if ((obj2 is IComparable) && (value.GetType() == obj2.GetType()))
            {
                return ((IComparable) obj2).CompareTo(value);
            }
            if (obj2 == value)
            {
                return 0;
            }
            if (obj2 == null)
            {
                if (base.NullValue == value)
                {
                    return 0;
                }
                return -1;
            }
            if ((base.NullValue != value) && (value != null))
            {
                return this.CompareWithFamilies(obj2, value);
            }
            return 1;
        }

        private int CompareWithFamilies(object valueNo1, object valueNo2)
        {
            Families family = this.GetFamily(valueNo1.GetType());
            Families families2 = this.GetFamily(valueNo2.GetType());
            if (family < families2)
            {
                return -1;
            }
            if (family > families2)
            {
                return 1;
            }
            switch (family)
            {
                case Families.DATETIME:
                    valueNo1 = Convert.ToDateTime(valueNo1, base.FormatProvider);
                    valueNo2 = Convert.ToDateTime(valueNo1, base.FormatProvider);
                    break;

                case Families.NUMBER:
                    valueNo1 = Convert.ToDouble(valueNo1, base.FormatProvider);
                    valueNo2 = Convert.ToDouble(valueNo2, base.FormatProvider);
                    break;

                case Families.BOOLEAN:
                    valueNo1 = Convert.ToBoolean(valueNo1, base.FormatProvider);
                    valueNo2 = Convert.ToBoolean(valueNo2, base.FormatProvider);
                    break;

                case Families.ARRAY:
                {
                    Array array = (Array) valueNo1;
                    Array array2 = (Array) valueNo2;
                    if (array.Length <= array2.Length)
                    {
                        if (array.Length < array2.Length)
                        {
                            return -1;
                        }
                        for (int i = 0; i < array.Length; i++)
                        {
                            int num2 = this.CompareTo(array.GetValue(i), array2.GetValue(i));
                            if (num2 != 0)
                            {
                                return num2;
                            }
                        }
                        return 0;
                    }
                    return 1;
                }
                default:
                    valueNo1 = valueNo1.ToString();
                    valueNo2 = valueNo2.ToString();
                    break;
            }
            return ((IComparable) valueNo1).CompareTo(valueNo2);
        }

        public override string ConvertObjectToXml(object value)
        {
            if ((value == null) || (value == base.NullValue))
            {
                return string.Empty;
            }
            Type dataType = base.DataType;
            if ((dataType == typeof(byte[])) || ((dataType == typeof(object)) && (value is byte[])))
            {
                return Convert.ToBase64String((byte[]) value);
            }
            if ((dataType == typeof(Type)) || ((dataType == typeof(object)) && (value is Type)))
            {
                return ((Type) value).AssemblyQualifiedName;
            }
            if (!DataStorage.IsTypeCustomType(value.GetType()))
            {
                return (string) SqlConvert.ChangeTypeForXML(value, typeof(string));
            }
            if (Type.GetTypeCode(value.GetType()) != TypeCode.Object)
            {
                return value.ToString();
            }
            StringWriter w = new StringWriter(base.FormatProvider);
            if (this.implementsIXmlSerializable)
            {
                using (XmlTextWriter writer2 = new XmlTextWriter(w))
                {
                    ((IXmlSerializable) value).WriteXml(writer2);
                }
                return w.ToString();
            }
            GetXmlSerializer(value.GetType()).Serialize((TextWriter) w, value);
            return w.ToString();
        }

        public override void ConvertObjectToXml(object value, XmlWriter xmlWriter, XmlRootAttribute xmlAttrib)
        {
            if (xmlAttrib == null)
            {
                ((IXmlSerializable) value).WriteXml(xmlWriter);
            }
            else
            {
                GetXmlSerializer(value.GetType(), xmlAttrib).Serialize(xmlWriter, value);
            }
        }

        public override object ConvertXmlToObject(string s)
        {
            Type dataType = base.DataType;
            if (dataType == typeof(byte[]))
            {
                return Convert.FromBase64String(s);
            }
            if (dataType == typeof(Type))
            {
                return Type.GetType(s);
            }
            if (dataType == typeof(Guid))
            {
                return new Guid(s);
            }
            if (dataType == typeof(Uri))
            {
                return new Uri(s);
            }
            if (this.implementsIXmlSerializable)
            {
                object obj2 = Activator.CreateInstance(base.DataType, true);
                StringReader input = new StringReader(s);
                using (XmlTextReader reader = new XmlTextReader(input))
                {
                    ((IXmlSerializable) obj2).ReadXml(reader);
                }
                return obj2;
            }
            StringReader textReader = new StringReader(s);
            return GetXmlSerializer(dataType).Deserialize(textReader);
        }

        public override object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib)
        {
            object obj2 = null;
            bool flag = false;
            bool flag2 = false;
            if (xmlAttrib != null)
            {
                return GetXmlSerializer(base.DataType, xmlAttrib).Deserialize(xmlReader);
            }
            Type type = null;
            string attribute = xmlReader.GetAttribute("InstanceType", "urn:schemas-microsoft-com:xml-msdata");
            if ((attribute == null) || (attribute.Length == 0))
            {
                string xsdTypeName = xmlReader.GetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance");
                if ((xsdTypeName != null) && (xsdTypeName.Length > 0))
                {
                    string[] strArray = xsdTypeName.Split(new char[] { ':' });
                    if ((strArray.Length == 2) && (xmlReader.LookupNamespace(strArray[0]) == "http://www.w3.org/2001/XMLSchema"))
                    {
                        xsdTypeName = strArray[1];
                    }
                    type = XSDSchema.XsdtoClr(xsdTypeName);
                    flag = true;
                }
                else if (base.DataType == typeof(object))
                {
                    flag2 = true;
                }
            }
            if (flag2)
            {
                return xmlReader.ReadString();
            }
            if (attribute == "Type")
            {
                obj2 = Type.GetType(xmlReader.ReadString());
                xmlReader.Read();
                return obj2;
            }
            if (null == type)
            {
                type = (attribute == null) ? base.DataType : DataStorage.GetType(attribute);
            }
            if ((type == typeof(char)) || (type == typeof(Guid)))
            {
                flag = true;
            }
            if (type == typeof(object))
            {
                throw ExceptionBuilder.CanNotDeserializeObjectType();
            }
            if (!flag)
            {
                obj2 = Activator.CreateInstance(type, true);
                ((IXmlSerializable) obj2).ReadXml(xmlReader);
                return obj2;
            }
            if (((type == typeof(string)) && (xmlReader.NodeType == XmlNodeType.Element)) && xmlReader.IsEmptyElement)
            {
                obj2 = string.Empty;
            }
            else
            {
                obj2 = xmlReader.ReadString();
                if (type != typeof(byte[]))
                {
                    obj2 = SqlConvert.ChangeTypeForXML(obj2, type);
                }
                else
                {
                    obj2 = Convert.FromBase64String(obj2.ToString());
                }
            }
            xmlReader.Read();
            return obj2;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            object[] objArray = (object[]) store;
            objArray[storeIndex] = this.values[record];
            bool flag = this.IsNull(record);
            nullbits.Set(storeIndex, flag);
            if (!flag && (objArray[storeIndex] is DateTime))
            {
                DateTime time = (DateTime) objArray[storeIndex];
                if (time.Kind == DateTimeKind.Local)
                {
                    objArray[storeIndex] = DateTime.SpecifyKind(time.ToUniversalTime(), DateTimeKind.Local);
                }
            }
        }

        public override object Get(int recordNo)
        {
            object obj2 = this.values[recordNo];
            if (obj2 != null)
            {
                return obj2;
            }
            return base.NullValue;
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new object[recordCount];
        }

        private Families GetFamily(Type dataType)
        {
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.Boolean:
                    return Families.BOOLEAN;

                case TypeCode.Char:
                    return Families.STRING;

                case TypeCode.SByte:
                    return Families.STRING;

                case TypeCode.Byte:
                    return Families.STRING;

                case TypeCode.Int16:
                    return Families.NUMBER;

                case TypeCode.UInt16:
                    return Families.NUMBER;

                case TypeCode.Int32:
                    return Families.NUMBER;

                case TypeCode.UInt32:
                    return Families.NUMBER;

                case TypeCode.Int64:
                    return Families.NUMBER;

                case TypeCode.UInt64:
                    return Families.NUMBER;

                case TypeCode.Single:
                    return Families.NUMBER;

                case TypeCode.Double:
                    return Families.NUMBER;

                case TypeCode.Decimal:
                    return Families.NUMBER;

                case TypeCode.DateTime:
                    return Families.DATETIME;

                case TypeCode.String:
                    return Families.STRING;
            }
            if (typeof(TimeSpan) == dataType)
            {
                return Families.DATETIME;
            }
            if (dataType.IsArray)
            {
                return Families.ARRAY;
            }
            return Families.STRING;
        }

        internal static XmlSerializer GetXmlSerializer(Type type)
        {
            VerifyIDynamicMetaObjectProvider(type);
            return _serializerFactory.CreateSerializer(type);
        }

        internal static XmlSerializer GetXmlSerializer(Type type, XmlRootAttribute attribute)
        {
            XmlSerializer serializer = null;
            KeyValuePair<Type, XmlRootAttribute> key = new KeyValuePair<Type, XmlRootAttribute>(type, attribute);
            Dictionary<KeyValuePair<Type, XmlRootAttribute>, XmlSerializer> dictionary = _tempAssemblyCache;
            if ((dictionary == null) || !dictionary.TryGetValue(key, out serializer))
            {
                lock (_tempAssemblyCacheLock)
                {
                    dictionary = _tempAssemblyCache;
                    if ((dictionary != null) && dictionary.TryGetValue(key, out serializer))
                    {
                        return serializer;
                    }
                    VerifyIDynamicMetaObjectProvider(type);
                    if (dictionary != null)
                    {
                        Dictionary<KeyValuePair<Type, XmlRootAttribute>, XmlSerializer> dictionary2 = new Dictionary<KeyValuePair<Type, XmlRootAttribute>, XmlSerializer>(1 + dictionary.Count, TempAssemblyComparer.Default);
                        foreach (KeyValuePair<KeyValuePair<Type, XmlRootAttribute>, XmlSerializer> pair2 in dictionary)
                        {
                            dictionary2.Add(pair2.Key, pair2.Value);
                        }
                        dictionary = dictionary2;
                    }
                    else
                    {
                        dictionary = new Dictionary<KeyValuePair<Type, XmlRootAttribute>, XmlSerializer>(TempAssemblyComparer.Default);
                    }
                    key = new KeyValuePair<Type, XmlRootAttribute>(type, new XmlRootAttribute());
                    key.Value.ElementName = attribute.ElementName;
                    key.Value.Namespace = attribute.Namespace;
                    key.Value.DataType = attribute.DataType;
                    key.Value.IsNullable = attribute.IsNullable;
                    serializer = _serializerFactory.CreateSerializer(type, attribute);
                    dictionary.Add(key, serializer);
                    _tempAssemblyCache = dictionary;
                }
            }
            return serializer;
        }

        public override bool IsNull(int record)
        {
            return (null == this.values[record]);
        }

        public override void Set(int recordNo, object value)
        {
            if (base.NullValue == value)
            {
                this.values[recordNo] = null;
            }
            else if ((base.DataType == typeof(object)) || base.DataType.IsInstanceOfType(value))
            {
                this.values[recordNo] = value;
            }
            else
            {
                Type type = value.GetType();
                if ((base.DataType == typeof(Guid)) && (type == typeof(string)))
                {
                    this.values[recordNo] = new Guid((string) value);
                }
                else
                {
                    if (!(base.DataType == typeof(byte[])))
                    {
                        throw ExceptionBuilder.StorageSetFailed();
                    }
                    if (type == typeof(bool))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((bool) value);
                    }
                    else if (type == typeof(char))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((char) value);
                    }
                    else if (type == typeof(short))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((short) value);
                    }
                    else if (type == typeof(int))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((int) value);
                    }
                    else if (type == typeof(long))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((long) value);
                    }
                    else if (type == typeof(ushort))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((ushort) value);
                    }
                    else if (type == typeof(uint))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((uint) value);
                    }
                    else if (type == typeof(ulong))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((ulong) value);
                    }
                    else if (type == typeof(float))
                    {
                        this.values[recordNo] = BitConverter.GetBytes((float) value);
                    }
                    else
                    {
                        if (type != typeof(double))
                        {
                            throw ExceptionBuilder.StorageSetFailed();
                        }
                        this.values[recordNo] = BitConverter.GetBytes((double) value);
                    }
                }
            }
        }

        public override void SetCapacity(int capacity)
        {
            object[] destinationArray = new object[capacity];
            if (this.values != null)
            {
                Array.Copy(this.values, 0, destinationArray, 0, Math.Min(capacity, this.values.Length));
            }
            this.values = destinationArray;
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (object[]) store;
            for (int i = 0; i < this.values.Length; i++)
            {
                if (this.values[i] is DateTime)
                {
                    DateTime time = (DateTime) this.values[i];
                    if (time.Kind == DateTimeKind.Local)
                    {
                        this.values[i] = DateTime.SpecifyKind(time, DateTimeKind.Utc).ToLocalTime();
                    }
                }
            }
        }

        internal static void VerifyIDynamicMetaObjectProvider(Type type)
        {
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(type) && !typeof(IXmlSerializable).IsAssignableFrom(type))
            {
                throw ADP.InvalidOperation(System.Data.Res.GetString("Xml_DynamicWithoutXmlSerializable"));
            }
        }

        private enum Families
        {
            DATETIME,
            NUMBER,
            STRING,
            BOOLEAN,
            ARRAY
        }

        private class TempAssemblyComparer : IEqualityComparer<KeyValuePair<Type, XmlRootAttribute>>
        {
            internal static readonly IEqualityComparer<KeyValuePair<Type, XmlRootAttribute>> Default = new ObjectStorage.TempAssemblyComparer();

            private TempAssemblyComparer()
            {
            }

            public bool Equals(KeyValuePair<Type, XmlRootAttribute> x, KeyValuePair<Type, XmlRootAttribute> y)
            {
                if (x.Key != y.Key)
                {
                    return false;
                }
                return (((x.Value == null) && (y.Value == null)) || (((((x.Value != null) && (y.Value != null)) && ((x.Value.ElementName == y.Value.ElementName) && (x.Value.Namespace == y.Value.Namespace))) && (x.Value.DataType == y.Value.DataType)) && (x.Value.IsNullable == y.Value.IsNullable)));
            }

            public int GetHashCode(KeyValuePair<Type, XmlRootAttribute> obj)
            {
                return (obj.Key.GetHashCode() + obj.Value.ElementName.GetHashCode());
            }
        }
    }
}

