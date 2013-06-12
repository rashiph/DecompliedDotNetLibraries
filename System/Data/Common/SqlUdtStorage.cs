namespace System.Data.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    internal sealed class SqlUdtStorage : DataStorage
    {
        private readonly bool implementsIComparable;
        private readonly bool implementsIXmlSerializable;
        private static readonly Dictionary<Type, object> TypeToNull = new Dictionary<Type, object>();
        private object[] values;

        public SqlUdtStorage(DataColumn column, Type type) : this(column, type, GetStaticNullForUdtType(type))
        {
        }

        private SqlUdtStorage(DataColumn column, Type type, object nullValue) : base(column, type, nullValue, nullValue, typeof(ICloneable).IsAssignableFrom(type))
        {
            this.implementsIXmlSerializable = typeof(IXmlSerializable).IsAssignableFrom(type);
            this.implementsIComparable = typeof(IComparable).IsAssignableFrom(type);
        }

        public override object Aggregate(int[] records, AggregateType kind)
        {
            throw ExceptionBuilder.AggregateException(kind, base.DataType);
        }

        public override int Compare(int recordNo1, int recordNo2)
        {
            return this.CompareValueTo(recordNo1, this.values[recordNo2]);
        }

        public override int CompareValueTo(int recordNo1, object value)
        {
            if (DBNull.Value == value)
            {
                value = base.NullValue;
            }
            if (this.implementsIComparable)
            {
                IComparable comparable = (IComparable) this.values[recordNo1];
                return comparable.CompareTo(value);
            }
            if (base.NullValue != value)
            {
                throw ExceptionBuilder.IComparableNotImplemented(base.DataType.AssemblyQualifiedName);
            }
            INullable nullable = (INullable) this.values[recordNo1];
            if (!nullable.IsNull)
            {
                return 1;
            }
            return 0;
        }

        public override string ConvertObjectToXml(object value)
        {
            StringWriter w = new StringWriter(base.FormatProvider);
            if (this.implementsIXmlSerializable)
            {
                using (XmlTextWriter writer = new XmlTextWriter(w))
                {
                    ((IXmlSerializable) value).WriteXml(writer);
                    goto Label_0047;
                }
            }
            ObjectStorage.GetXmlSerializer(value.GetType()).Serialize((TextWriter) w, value);
        Label_0047:
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
                ObjectStorage.GetXmlSerializer(base.DataType, xmlAttrib).Serialize(xmlWriter, value);
            }
        }

        public override object ConvertXmlToObject(string s)
        {
            if (this.implementsIXmlSerializable)
            {
                object obj2 = Activator.CreateInstance(base.DataType, true);
                StringReader input = new StringReader("<col>" + s + "</col>");
                using (XmlTextReader reader = new XmlTextReader(input))
                {
                    ((IXmlSerializable) obj2).ReadXml(reader);
                }
                return obj2;
            }
            StringReader textReader = new StringReader(s);
            return ObjectStorage.GetXmlSerializer(base.DataType).Deserialize(textReader);
        }

        public override object ConvertXmlToObject(XmlReader xmlReader, XmlRootAttribute xmlAttrib)
        {
            if (xmlAttrib != null)
            {
                return ObjectStorage.GetXmlSerializer(base.DataType, xmlAttrib).Deserialize(xmlReader);
            }
            string attribute = xmlReader.GetAttribute("InstanceType", "urn:schemas-microsoft-com:xml-msdata");
            if (attribute == null)
            {
                string xsdTypeName = xmlReader.GetAttribute("InstanceType", "http://www.w3.org/2001/XMLSchema-instance");
                if (xsdTypeName != null)
                {
                    attribute = XSDSchema.XsdtoClr(xsdTypeName).FullName;
                }
            }
            Type type = (attribute == null) ? base.DataType : Type.GetType(attribute);
            object obj2 = Activator.CreateInstance(type, true);
            ((IXmlSerializable) obj2).ReadXml(xmlReader);
            return obj2;
        }

        public override void Copy(int recordNo1, int recordNo2)
        {
            base.CopyBits(recordNo1, recordNo2);
            this.values[recordNo2] = this.values[recordNo1];
        }

        protected override void CopyValue(int record, object store, BitArray nullbits, int storeIndex)
        {
            object[] objArray = (object[]) store;
            objArray[storeIndex] = this.values[record];
            nullbits.Set(storeIndex, this.IsNull(record));
        }

        public override object Get(int recordNo)
        {
            return this.values[recordNo];
        }

        protected override object GetEmptyStorage(int recordCount)
        {
            return new object[recordCount];
        }

        internal static object GetStaticNullForUdtType(Type type)
        {
            object obj2;
            if (!TypeToNull.TryGetValue(type, out obj2))
            {
                PropertyInfo property = type.GetProperty("Null", BindingFlags.Public | BindingFlags.Static);
                if (property != null)
                {
                    obj2 = property.GetValue(null, null);
                }
                else
                {
                    FieldInfo field = type.GetField("Null", BindingFlags.Public | BindingFlags.Static);
                    if (field == null)
                    {
                        throw ExceptionBuilder.INullableUDTwithoutStaticNull(type.AssemblyQualifiedName);
                    }
                    obj2 = field.GetValue(null);
                }
                lock (TypeToNull)
                {
                    TypeToNull[type] = obj2;
                }
            }
            return obj2;
        }

        public override bool IsNull(int record)
        {
            return ((INullable) this.values[record]).IsNull;
        }

        public override void Set(int recordNo, object value)
        {
            if (DBNull.Value == value)
            {
                this.values[recordNo] = base.NullValue;
                base.SetNullBit(recordNo, true);
            }
            else if (value == null)
            {
                if (base.IsValueType)
                {
                    throw ExceptionBuilder.StorageSetFailed();
                }
                this.values[recordNo] = base.NullValue;
                base.SetNullBit(recordNo, true);
            }
            else
            {
                if (!base.DataType.IsInstanceOfType(value))
                {
                    throw ExceptionBuilder.StorageSetFailed();
                }
                this.values[recordNo] = value;
                base.SetNullBit(recordNo, false);
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
            base.SetCapacity(capacity);
        }

        protected override void SetStorage(object store, BitArray nullbits)
        {
            this.values = (object[]) store;
        }
    }
}

