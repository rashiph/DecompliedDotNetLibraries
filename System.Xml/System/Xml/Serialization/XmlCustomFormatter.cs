namespace System.Xml.Serialization
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using System.Xml.Serialization.Configuration;

    internal class XmlCustomFormatter
    {
        private static string[] allDateFormats = new string[] { 
            "yyyy-MM-ddzzzzzz", "yyyy-MM-dd", "yyyy-MM-ddZ", "yyyy", "---dd", "---ddZ", "---ddzzzzzz", "--MM-dd", "--MM-ddZ", "--MM-ddzzzzzz", "--MM--", "--MM--Z", "--MM--zzzzzz", "yyyy-MM", "yyyy-MMZ", "yyyy-MMzzzzzz", 
            "yyyyzzzzzz"
         };
        private static string[] allDateTimeFormats = new string[] { 
            "yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz", "yyyy", "---dd", "---ddZ", "---ddzzzzzz", "--MM-dd", "--MM-ddZ", "--MM-ddzzzzzz", "--MM--", "--MM--Z", "--MM--zzzzzz", "yyyy-MM", "yyyy-MMZ", "yyyy-MMzzzzzz", "yyyyzzzzzz", "yyyy-MM-dd", 
            "yyyy-MM-ddZ", "yyyy-MM-ddzzzzzz", "HH:mm:ss", "HH:mm:ss.f", "HH:mm:ss.ff", "HH:mm:ss.fff", "HH:mm:ss.ffff", "HH:mm:ss.fffff", "HH:mm:ss.ffffff", "HH:mm:ss.fffffff", "HH:mm:ssZ", "HH:mm:ss.fZ", "HH:mm:ss.ffZ", "HH:mm:ss.fffZ", "HH:mm:ss.ffffZ", "HH:mm:ss.fffffZ", 
            "HH:mm:ss.ffffffZ", "HH:mm:ss.fffffffZ", "HH:mm:sszzzzzz", "HH:mm:ss.fzzzzzz", "HH:mm:ss.ffzzzzzz", "HH:mm:ss.fffzzzzzz", "HH:mm:ss.ffffzzzzzz", "HH:mm:ss.fffffzzzzzz", "HH:mm:ss.ffffffzzzzzz", "HH:mm:ss.fffffffzzzzzz", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.f", "yyyy-MM-ddTHH:mm:ss.ff", "yyyy-MM-ddTHH:mm:ss.fff", "yyyy-MM-ddTHH:mm:ss.ffff", "yyyy-MM-ddTHH:mm:ss.fffff", 
            "yyyy-MM-ddTHH:mm:ss.ffffff", "yyyy-MM-ddTHH:mm:ss.fffffff", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss.fZ", "yyyy-MM-ddTHH:mm:ss.ffZ", "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ss.ffffZ", "yyyy-MM-ddTHH:mm:ss.fffffZ", "yyyy-MM-ddTHH:mm:ss.ffffffZ", "yyyy-MM-ddTHH:mm:ss.fffffffZ", "yyyy-MM-ddTHH:mm:sszzzzzz", "yyyy-MM-ddTHH:mm:ss.fzzzzzz", "yyyy-MM-ddTHH:mm:ss.ffzzzzzz", "yyyy-MM-ddTHH:mm:ss.fffzzzzzz", "yyyy-MM-ddTHH:mm:ss.ffffzzzzzz", "yyyy-MM-ddTHH:mm:ss.fffffzzzzzz", 
            "yyyy-MM-ddTHH:mm:ss.ffffffzzzzzz"
         };
        private static string[] allTimeFormats = new string[] { 
            "HH:mm:ss.fffffffzzzzzz", "HH:mm:ss", "HH:mm:ss.f", "HH:mm:ss.ff", "HH:mm:ss.fff", "HH:mm:ss.ffff", "HH:mm:ss.fffff", "HH:mm:ss.ffffff", "HH:mm:ss.fffffff", "HH:mm:ssZ", "HH:mm:ss.fZ", "HH:mm:ss.ffZ", "HH:mm:ss.fffZ", "HH:mm:ss.ffffZ", "HH:mm:ss.fffffZ", "HH:mm:ss.ffffffZ", 
            "HH:mm:ss.fffffffZ", "HH:mm:sszzzzzz", "HH:mm:ss.fzzzzzz", "HH:mm:ss.ffzzzzzz", "HH:mm:ss.fffzzzzzz", "HH:mm:ss.ffffzzzzzz", "HH:mm:ss.fffffzzzzzz", "HH:mm:ss.ffffffzzzzzz"
         };
        private static DateTimeSerializationSection.DateTimeSerializationMode mode;

        private XmlCustomFormatter()
        {
        }

        private static string CollapseWhitespace(string value)
        {
            if (value == null)
            {
                return null;
            }
            return value.Trim();
        }

        internal static string FromByteArrayHex(byte[] value)
        {
            if (value == null)
            {
                return null;
            }
            if (value.Length == 0)
            {
                return "";
            }
            return XmlConvert.ToBinHexString(value);
        }

        internal static string FromChar(char value)
        {
            return XmlConvert.ToString((ushort) value);
        }

        internal static string FromDate(DateTime value)
        {
            return XmlConvert.ToString(value, "yyyy-MM-dd");
        }

        internal static string FromDateTime(DateTime value)
        {
            if (Mode == DateTimeSerializationSection.DateTimeSerializationMode.Local)
            {
                return XmlConvert.ToString(value, "yyyy-MM-ddTHH:mm:ss.fffffffzzzzzz");
            }
            return XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal static string FromDefaultValue(object value, string formatter)
        {
            if (value == null)
            {
                return null;
            }
            Type type = value.GetType();
            if (type == typeof(DateTime))
            {
                if (formatter == "DateTime")
                {
                    return FromDateTime((DateTime) value);
                }
                if (formatter == "Date")
                {
                    return FromDate((DateTime) value);
                }
                if (formatter == "Time")
                {
                    return FromTime((DateTime) value);
                }
            }
            else if (type == typeof(string))
            {
                if (formatter == "XmlName")
                {
                    return FromXmlName((string) value);
                }
                if (formatter == "XmlNCName")
                {
                    return FromXmlNCName((string) value);
                }
                if (formatter == "XmlNmToken")
                {
                    return FromXmlNmToken((string) value);
                }
                if (formatter == "XmlNmTokens")
                {
                    return FromXmlNmTokens((string) value);
                }
            }
            throw new Exception(Res.GetString("XmlUnsupportedDefaultType", new object[] { type.FullName }));
        }

        internal static string FromEnum(long val, string[] vals, long[] ids, string typeName)
        {
            long num = val;
            StringBuilder builder = new StringBuilder();
            int index = -1;
            for (int i = 0; i < ids.Length; i++)
            {
                if (ids[i] == 0L)
                {
                    index = i;
                }
                else
                {
                    if (val == 0L)
                    {
                        break;
                    }
                    if ((ids[i] & num) == ids[i])
                    {
                        if (builder.Length != 0)
                        {
                            builder.Append(" ");
                        }
                        builder.Append(vals[i]);
                        val &= ~ids[i];
                    }
                }
            }
            if (val != 0L)
            {
                throw new InvalidOperationException(Res.GetString("XmlUnknownConstant", new object[] { num, (typeName == null) ? "enum" : typeName }));
            }
            if ((builder.Length == 0) && (index >= 0))
            {
                builder.Append(vals[index]);
            }
            return builder.ToString();
        }

        internal static string FromTime(DateTime value)
        {
            return XmlConvert.ToString(DateTime.MinValue + value.TimeOfDay, "HH:mm:ss.fffffffzzzzzz");
        }

        internal static string FromXmlName(string name)
        {
            return XmlConvert.EncodeName(name);
        }

        internal static string FromXmlNCName(string ncName)
        {
            return XmlConvert.EncodeLocalName(ncName);
        }

        internal static string FromXmlNmToken(string nmToken)
        {
            return XmlConvert.EncodeNmToken(nmToken);
        }

        internal static string FromXmlNmTokens(string nmTokens)
        {
            if (nmTokens == null)
            {
                return null;
            }
            if (nmTokens.IndexOf(' ') < 0)
            {
                return FromXmlNmToken(nmTokens);
            }
            string[] strArray = nmTokens.Split(new char[] { ' ' });
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(FromXmlNmToken(strArray[i]));
            }
            return builder.ToString();
        }

        internal static byte[] ToByteArrayBase64(string value)
        {
            if (value == null)
            {
                return null;
            }
            value = value.Trim();
            if (value.Length == 0)
            {
                return new byte[0];
            }
            return Convert.FromBase64String(value);
        }

        internal static byte[] ToByteArrayHex(string value)
        {
            if (value == null)
            {
                return null;
            }
            value = value.Trim();
            return XmlConvert.FromBinHexString(value);
        }

        internal static char ToChar(string value)
        {
            return (char) XmlConvert.ToUInt16(value);
        }

        internal static DateTime ToDate(string value)
        {
            return ToDateTime(value, allDateFormats);
        }

        internal static DateTime ToDateTime(string value)
        {
            if (Mode == DateTimeSerializationSection.DateTimeSerializationMode.Local)
            {
                return ToDateTime(value, allDateTimeFormats);
            }
            return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind);
        }

        internal static DateTime ToDateTime(string value, string[] formats)
        {
            return XmlConvert.ToDateTime(value, formats);
        }

        internal static object ToDefaultValue(string value, string formatter)
        {
            if (formatter == "DateTime")
            {
                return ToDateTime(value);
            }
            if (formatter == "Date")
            {
                return ToDate(value);
            }
            if (formatter == "Time")
            {
                return ToTime(value);
            }
            if (formatter == "XmlName")
            {
                return ToXmlName(value);
            }
            if (formatter == "XmlNCName")
            {
                return ToXmlNCName(value);
            }
            if (formatter == "XmlNmToken")
            {
                return ToXmlNmToken(value);
            }
            if (formatter != "XmlNmTokens")
            {
                throw new Exception(Res.GetString("XmlUnsupportedDefaultValue", new object[] { formatter }));
            }
            return ToXmlNmTokens(value);
        }

        internal static long ToEnum(string val, Hashtable vals, string typeName, bool validate)
        {
            long num = 0L;
            string[] strArray = val.Split(null);
            for (int i = 0; i < strArray.Length; i++)
            {
                object obj2 = vals[strArray[i]];
                if (obj2 != null)
                {
                    num |= (long) obj2;
                }
                else if (validate && (strArray[i].Length > 0))
                {
                    throw new InvalidOperationException(Res.GetString("XmlUnknownConstant", new object[] { strArray[i], typeName }));
                }
            }
            return num;
        }

        internal static DateTime ToTime(string value)
        {
            return DateTime.ParseExact(value, allTimeFormats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowLeadingWhite);
        }

        internal static string ToXmlName(string value)
        {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }

        internal static string ToXmlNCName(string value)
        {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }

        internal static string ToXmlNmToken(string value)
        {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }

        internal static string ToXmlNmTokens(string value)
        {
            return XmlConvert.DecodeName(CollapseWhitespace(value));
        }

        internal static void WriteArrayBase64(XmlWriter writer, byte[] inData, int start, int count)
        {
            if ((inData != null) && (count != 0))
            {
                writer.WriteBase64(inData, start, count);
            }
        }

        private static DateTimeSerializationSection.DateTimeSerializationMode Mode
        {
            get
            {
                if (mode == DateTimeSerializationSection.DateTimeSerializationMode.Default)
                {
                    DateTimeSerializationSection section = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.DateTimeSerializationSectionPath) as DateTimeSerializationSection;
                    if (section != null)
                    {
                        mode = section.Mode;
                    }
                    else
                    {
                        mode = DateTimeSerializationSection.DateTimeSerializationMode.Roundtrip;
                    }
                }
                return mode;
            }
        }
    }
}

