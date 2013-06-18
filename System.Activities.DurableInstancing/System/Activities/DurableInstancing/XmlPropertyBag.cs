namespace System.Activities.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    internal class XmlPropertyBag : Dictionary<XName, object>, IXmlSerializable
    {
        public static string ConvertNativeValueToString(object value)
        {
            if (value != null)
            {
                if (value is bool)
                {
                    return XmlConvert.ToString((bool) value);
                }
                if (value is byte)
                {
                    return XmlConvert.ToString((byte) value);
                }
                if (value is char)
                {
                    return XmlConvert.ToString((char) value);
                }
                if (value is DateTime)
                {
                    return XmlConvert.ToString((DateTime) value, XmlDateTimeSerializationMode.RoundtripKind);
                }
                if (value is DateTimeOffset)
                {
                    return XmlConvert.ToString((DateTimeOffset) value);
                }
                if (value is decimal)
                {
                    return XmlConvert.ToString((decimal) value);
                }
                if (value is double)
                {
                    return XmlConvert.ToString((double) value);
                }
                if (value is float)
                {
                    float num = (float) value;
                    return num.ToString("r", CultureInfo.InvariantCulture);
                }
                if (value is Guid)
                {
                    return XmlConvert.ToString((Guid) value);
                }
                if (value is int)
                {
                    return XmlConvert.ToString((int) value);
                }
                if (value is long)
                {
                    return XmlConvert.ToString((long) value);
                }
                if (value is sbyte)
                {
                    return XmlConvert.ToString((sbyte) value);
                }
                if (value is short)
                {
                    return XmlConvert.ToString((short) value);
                }
                if (value is string)
                {
                    return (string) value;
                }
                if (value is TimeSpan)
                {
                    return XmlConvert.ToString((TimeSpan) value);
                }
                if (value is Type)
                {
                    return value.ToString();
                }
                if (value is uint)
                {
                    return XmlConvert.ToString((uint) value);
                }
                if (value is ulong)
                {
                    return XmlConvert.ToString((ulong) value);
                }
                if (value is Uri)
                {
                    return ((Uri) value).ToString();
                }
                if (value is ushort)
                {
                    return XmlConvert.ToString((ushort) value);
                }
                if (value is XmlQualifiedName)
                {
                    return ((XmlQualifiedName) value).ToString();
                }
                Fx.AssertAndThrow("Should never reach here");
            }
            return null;
        }

        public static object ConvertStringToNativeType(string value, PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Bool:
                    return XmlConvert.ToBoolean(value);

                case PrimitiveType.Byte:
                    return XmlConvert.ToByte(value);

                case PrimitiveType.Char:
                    return XmlConvert.ToChar(value);

                case PrimitiveType.DateTime:
                    return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);

                case PrimitiveType.DateTimeOffset:
                    return XmlConvert.ToDateTimeOffset(value);

                case PrimitiveType.Decimal:
                    return XmlConvert.ToDecimal(value);

                case PrimitiveType.Double:
                    return XmlConvert.ToDouble(value);

                case PrimitiveType.Float:
                    return float.Parse(value, CultureInfo.InvariantCulture);

                case PrimitiveType.Guid:
                    return XmlConvert.ToGuid(value);

                case PrimitiveType.Int:
                    return XmlConvert.ToInt32(value);

                case PrimitiveType.Long:
                    return XmlConvert.ToInt64(value);

                case PrimitiveType.SByte:
                    return XmlConvert.ToSByte(value);

                case PrimitiveType.Short:
                    return XmlConvert.ToInt16(value);

                case PrimitiveType.String:
                    return value;

                case PrimitiveType.TimeSpan:
                    return XmlConvert.ToTimeSpan(value);

                case PrimitiveType.Type:
                    return Type.GetType(value);

                case PrimitiveType.UInt:
                    return XmlConvert.ToUInt32(value);

                case PrimitiveType.ULong:
                    return XmlConvert.ToUInt64(value);

                case PrimitiveType.Uri:
                    return new Uri(value);

                case PrimitiveType.UShort:
                    return XmlConvert.ToUInt16(value);

                case PrimitiveType.XmlQualifiedName:
                    return new XmlQualifiedName(value);
            }
            return null;
        }

        public static PrimitiveType GetPrimitiveType(object value)
        {
            if (value == null)
            {
                return PrimitiveType.Null;
            }
            if (value is bool)
            {
                return PrimitiveType.Bool;
            }
            if (value is byte)
            {
                return PrimitiveType.Byte;
            }
            if (value is char)
            {
                return PrimitiveType.Char;
            }
            if (value is DateTime)
            {
                return PrimitiveType.DateTime;
            }
            if (value is DateTimeOffset)
            {
                return PrimitiveType.DateTimeOffset;
            }
            if (value is decimal)
            {
                return PrimitiveType.Decimal;
            }
            if (value is double)
            {
                return PrimitiveType.Double;
            }
            if (value is float)
            {
                return PrimitiveType.Float;
            }
            if (value is Guid)
            {
                return PrimitiveType.Guid;
            }
            if (value is int)
            {
                return PrimitiveType.Int;
            }
            if (value is long)
            {
                return PrimitiveType.Long;
            }
            if (value is sbyte)
            {
                return PrimitiveType.SByte;
            }
            if (value is short)
            {
                return PrimitiveType.Short;
            }
            if (value is string)
            {
                return PrimitiveType.String;
            }
            if (value is TimeSpan)
            {
                return PrimitiveType.TimeSpan;
            }
            if (value is Type)
            {
                return PrimitiveType.Type;
            }
            if (value is uint)
            {
                return PrimitiveType.UInt;
            }
            if (value is ulong)
            {
                return PrimitiveType.ULong;
            }
            if (value is Uri)
            {
                return PrimitiveType.Uri;
            }
            if (value is ushort)
            {
                return PrimitiveType.UShort;
            }
            if (value is XmlQualifiedName)
            {
                return PrimitiveType.XmlQualifiedName;
            }
            return PrimitiveType.Unavailable;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.ReadToDescendant("Property"))
            {
                do
                {
                    reader.MoveToFirstAttribute();
                    XName key = XName.Get(reader.Value);
                    reader.MoveToNextAttribute();
                    PrimitiveType type = (PrimitiveType) int.Parse(reader.Value, CultureInfo.InvariantCulture);
                    reader.MoveToNextAttribute();
                    object obj2 = ConvertStringToNativeType(reader.Value, type);
                    base.Add(key, obj2);
                }
                while (reader.ReadToNextSibling("Property"));
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Properties");
            foreach (KeyValuePair<XName, object> pair in this)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("XName", pair.Key.ToString());
                writer.WriteAttributeString("Type", ((int) GetPrimitiveType(pair.Value)).ToString(CultureInfo.InvariantCulture));
                writer.WriteAttributeString("Value", ConvertNativeValueToString(pair.Value));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}

