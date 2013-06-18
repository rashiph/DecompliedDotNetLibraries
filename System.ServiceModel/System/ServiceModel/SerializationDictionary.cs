namespace System.ServiceModel
{
    using System;
    using System.Xml;

    internal class SerializationDictionary
    {
        public XmlDictionaryString AnyType;
        public XmlDictionaryString AnyURI;
        public XmlDictionaryString Base64Binary;
        public XmlDictionaryString Boolean;
        public XmlDictionaryString Byte;
        public XmlDictionaryString Char;
        public XmlDictionaryString Date;
        public XmlDictionaryString DateTime;
        public XmlDictionaryString Decimal;
        public XmlDictionaryString Double;
        public XmlDictionaryString Duration;
        public XmlDictionaryString Float;
        public XmlDictionaryString GDay;
        public XmlDictionaryString GMonth;
        public XmlDictionaryString GMonthDay;
        public XmlDictionaryString Guid;
        public XmlDictionaryString GYear;
        public XmlDictionaryString GYearMonth;
        public XmlDictionaryString HexBinary;
        public XmlDictionaryString Int;
        public XmlDictionaryString Integer;
        public XmlDictionaryString Long;
        public XmlDictionaryString NegativeInteger;
        public XmlDictionaryString Nil;
        public XmlDictionaryString NonNegativeInteger;
        public XmlDictionaryString NonPositiveInteger;
        public XmlDictionaryString NormalizedString;
        public XmlDictionaryString PositiveInteger;
        public XmlDictionaryString QName;
        public XmlDictionaryString Short;
        public XmlDictionaryString String;
        public XmlDictionaryString Time;
        public XmlDictionaryString Type;
        public XmlDictionaryString UnsignedByte;
        public XmlDictionaryString UnsignedInt;
        public XmlDictionaryString UnsignedLong;
        public XmlDictionaryString UnsignedShort;
        public XmlDictionaryString XmlSchemaInstanceNamespace;
        public XmlDictionaryString XmlSchemaNamespace;

        public SerializationDictionary(ServiceModelDictionary dictionary)
        {
            this.XmlSchemaInstanceNamespace = dictionary.CreateString("http://www.w3.org/2001/XMLSchema-instance", 0x1b9);
            this.XmlSchemaNamespace = dictionary.CreateString("http://www.w3.org/2001/XMLSchema", 0x1ba);
            this.Nil = dictionary.CreateString("nil", 0x1bb);
            this.Type = dictionary.CreateString("type", 0x1bc);
            this.Char = dictionary.CreateString("char", 0x1bd);
            this.Boolean = dictionary.CreateString("boolean", 0x1be);
            this.Byte = dictionary.CreateString("byte", 0x1bf);
            this.UnsignedByte = dictionary.CreateString("unsignedByte", 0x1c0);
            this.Short = dictionary.CreateString("short", 0x1c1);
            this.UnsignedShort = dictionary.CreateString("unsignedShort", 450);
            this.Int = dictionary.CreateString("int", 0x1c3);
            this.UnsignedInt = dictionary.CreateString("unsignedInt", 0x1c4);
            this.Long = dictionary.CreateString("long", 0x1c5);
            this.UnsignedLong = dictionary.CreateString("unsignedLong", 0x1c6);
            this.Float = dictionary.CreateString("float", 0x1c7);
            this.Double = dictionary.CreateString("double", 0x1c8);
            this.Decimal = dictionary.CreateString("decimal", 0x1c9);
            this.DateTime = dictionary.CreateString("dateTime", 0x1ca);
            this.String = dictionary.CreateString("string", 0x1cb);
            this.Base64Binary = dictionary.CreateString("base64Binary", 460);
            this.AnyType = dictionary.CreateString("anyType", 0x1cd);
            this.Duration = dictionary.CreateString("duration", 0x1ce);
            this.Guid = dictionary.CreateString("guid", 0x1cf);
            this.AnyURI = dictionary.CreateString("anyURI", 0x1d0);
            this.QName = dictionary.CreateString("QName", 0x1d1);
            this.Time = dictionary.CreateString("time", 0x1d2);
            this.Date = dictionary.CreateString("date", 0x1d3);
            this.HexBinary = dictionary.CreateString("hexBinary", 0x1d4);
            this.GYearMonth = dictionary.CreateString("gYearMonth", 0x1d5);
            this.GYear = dictionary.CreateString("gYear", 470);
            this.GMonthDay = dictionary.CreateString("gMonthDay", 0x1d7);
            this.GDay = dictionary.CreateString("gDay", 0x1d8);
            this.GMonth = dictionary.CreateString("gMonth", 0x1d9);
            this.Integer = dictionary.CreateString("integer", 0x1da);
            this.PositiveInteger = dictionary.CreateString("positiveInteger", 0x1db);
            this.NegativeInteger = dictionary.CreateString("negativeInteger", 0x1dc);
            this.NonPositiveInteger = dictionary.CreateString("nonPositiveInteger", 0x1dd);
            this.NonNegativeInteger = dictionary.CreateString("nonNegativeInteger", 0x1de);
            this.NormalizedString = dictionary.CreateString("normalizedString", 0x1df);
        }
    }
}

