namespace System.Runtime.Serialization.Formatters.Soap
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Serialization;
    using System.Text;

    internal sealed class Converter
    {
        private static InternalPrimitiveTypeE[] codeA;
        private static bool[] escapeA;
        private static int primitiveTypeEnumLength = 0x2e;
        private static StringBuilder sb = new StringBuilder(30);
        private static Type[] typeA;
        private static TypeCode[] typeCodeA;
        internal static Type typeofBoolean = typeof(bool);
        internal static Type typeofByte = typeof(byte);
        internal static Type typeofChar = typeof(char);
        internal static Type typeofConverter = typeof(Converter);
        internal static Type typeofDateTime = typeof(DateTime);
        internal static Type typeofDecimal = typeof(decimal);
        internal static Type typeofDouble = typeof(double);
        internal static Type typeofHeader = typeof(Header);
        internal static Type typeofIConstructionCallMessage = typeof(IConstructionCallMessage);
        internal static Type typeofIMethodCallMessage = typeof(IMethodCallMessage);
        internal static Type typeofInt16 = typeof(short);
        internal static Type typeofInt32 = typeof(int);
        internal static Type typeofInt64 = typeof(long);
        internal static Type typeofInternalSoapMessage = typeof(InternalSoapMessage);
        internal static Type typeofISerializable = typeof(ISerializable);
        internal static Type typeofISoapXsd = typeof(ISoapXsd);
        internal static Type typeofMarshalByRefObject = typeof(MarshalByRefObject);
        internal static Type typeofObject = typeof(object);
        internal static Type typeofReturnMessage = typeof(ReturnMessage);
        internal static Type typeofSByte = typeof(sbyte);
        internal static Type typeofSingle = typeof(float);
        internal static Type typeofSoapAnyUri = typeof(SoapAnyUri);
        internal static Type typeofSoapBase64Binary = typeof(SoapBase64Binary);
        internal static Type typeofSoapDate = typeof(SoapDate);
        internal static Type typeofSoapDay = typeof(SoapDay);
        internal static Type typeofSoapEntities = typeof(SoapEntities);
        internal static Type typeofSoapEntity = typeof(SoapEntity);
        internal static Type typeofSoapFault = typeof(SoapFault);
        internal static Type typeofSoapHexBinary = typeof(SoapHexBinary);
        internal static Type typeofSoapId = typeof(SoapId);
        internal static Type typeofSoapIdref = typeof(SoapIdref);
        internal static Type typeofSoapIdrefs = typeof(SoapIdrefs);
        internal static Type typeofSoapInteger = typeof(SoapInteger);
        internal static Type typeofSoapLanguage = typeof(SoapLanguage);
        internal static Type typeofSoapMonth = typeof(SoapMonth);
        internal static Type typeofSoapMonthDay = typeof(SoapMonthDay);
        internal static Type typeofSoapName = typeof(SoapName);
        internal static Type typeofSoapNcName = typeof(SoapNcName);
        internal static Type typeofSoapNegativeInteger = typeof(SoapNegativeInteger);
        internal static Type typeofSoapNmtoken = typeof(SoapNmtoken);
        internal static Type typeofSoapNmtokens = typeof(SoapNmtokens);
        internal static Type typeofSoapNonNegativeInteger = typeof(SoapNonNegativeInteger);
        internal static Type typeofSoapNonPositiveInteger = typeof(SoapNonPositiveInteger);
        internal static Type typeofSoapNormalizedString = typeof(SoapNormalizedString);
        internal static Type typeofSoapNotation = typeof(SoapNotation);
        internal static Type typeofSoapPositiveInteger = typeof(SoapPositiveInteger);
        internal static Type typeofSoapQName = typeof(SoapQName);
        internal static Type typeofSoapTime = typeof(SoapTime);
        internal static Type typeofSoapToken = typeof(SoapToken);
        internal static Type typeofSoapYear = typeof(SoapYear);
        internal static Type typeofSoapYearMonth = typeof(SoapYearMonth);
        internal static Type typeofString = typeof(string);
        internal static Type typeofSystemVoid = typeof(void);
        internal static Type typeofTimeSpan = typeof(TimeSpan);
        internal static Type typeofTypeArray = typeof(Type[]);
        internal static Type typeofUInt16 = typeof(ushort);
        internal static Type typeofUInt32 = typeof(uint);
        internal static Type typeofUInt64 = typeof(ulong);
        internal static Assembly urtAssembly = Assembly.GetAssembly(typeofString);
        internal static string urtAssemblyString = urtAssembly.FullName;
        private static string[] valueA;
        private static string[] valueB;

        private Converter()
        {
        }

        internal static object FromString(string value, InternalPrimitiveTypeE code)
        {
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                    if (!(value == "1") && !(value == "true"))
                    {
                        if ((value != "0") && (value != "false"))
                        {
                            throw new SerializationException(string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("Serialization_typeCoercion"), new object[] { value, "Boolean" }));
                        }
                        return false;
                    }
                    return true;

                case InternalPrimitiveTypeE.Double:
                    if (!(value == "INF"))
                    {
                        if (value == "-INF")
                        {
                            return (double) -1.0 / (double) 0.0;
                        }
                        return double.Parse(value, CultureInfo.InvariantCulture);
                    }
                    return (double) 1.0 / (double) 0.0;

                case InternalPrimitiveTypeE.Single:
                    if (!(value == "INF"))
                    {
                        if (value == "-INF")
                        {
                            return (float) -1.0 / (float) 0.0;
                        }
                        return float.Parse(value, CultureInfo.InvariantCulture);
                    }
                    return (float) 1.0 / (float) 0.0;

                case InternalPrimitiveTypeE.TimeSpan:
                    return SoapDuration.Parse(value);

                case InternalPrimitiveTypeE.DateTime:
                    return SoapDateTime.Parse(value);

                case InternalPrimitiveTypeE.Time:
                    return SoapTime.Parse(value);

                case InternalPrimitiveTypeE.Date:
                    return SoapDate.Parse(value);

                case InternalPrimitiveTypeE.YearMonth:
                    return SoapYearMonth.Parse(value);

                case InternalPrimitiveTypeE.Year:
                    return SoapYear.Parse(value);

                case InternalPrimitiveTypeE.MonthDay:
                    return SoapMonthDay.Parse(value);

                case InternalPrimitiveTypeE.Day:
                    return SoapDay.Parse(value);

                case InternalPrimitiveTypeE.Month:
                    return SoapMonth.Parse(value);

                case InternalPrimitiveTypeE.HexBinary:
                    return SoapHexBinary.Parse(value);

                case InternalPrimitiveTypeE.Base64Binary:
                    return SoapBase64Binary.Parse(value);

                case InternalPrimitiveTypeE.Integer:
                    return SoapInteger.Parse(value);

                case InternalPrimitiveTypeE.PositiveInteger:
                    return SoapPositiveInteger.Parse(value);

                case InternalPrimitiveTypeE.NonPositiveInteger:
                    return SoapNonPositiveInteger.Parse(value);

                case InternalPrimitiveTypeE.NonNegativeInteger:
                    return SoapNonNegativeInteger.Parse(value);

                case InternalPrimitiveTypeE.NegativeInteger:
                    return SoapNegativeInteger.Parse(value);

                case InternalPrimitiveTypeE.AnyUri:
                    return SoapAnyUri.Parse(value);

                case InternalPrimitiveTypeE.QName:
                    return SoapQName.Parse(value);

                case InternalPrimitiveTypeE.Notation:
                    return SoapNotation.Parse(value);

                case InternalPrimitiveTypeE.NormalizedString:
                    return SoapNormalizedString.Parse(value);

                case InternalPrimitiveTypeE.Token:
                    return SoapToken.Parse(value);

                case InternalPrimitiveTypeE.Language:
                    return SoapLanguage.Parse(value);

                case InternalPrimitiveTypeE.Name:
                    return SoapName.Parse(value);

                case InternalPrimitiveTypeE.Idrefs:
                    return SoapIdrefs.Parse(value);

                case InternalPrimitiveTypeE.Entities:
                    return SoapEntities.Parse(value);

                case InternalPrimitiveTypeE.Nmtoken:
                    return SoapNmtoken.Parse(value);

                case InternalPrimitiveTypeE.Nmtokens:
                    return SoapNmtokens.Parse(value);

                case InternalPrimitiveTypeE.NcName:
                    return SoapNcName.Parse(value);

                case InternalPrimitiveTypeE.Id:
                    return SoapId.Parse(value);

                case InternalPrimitiveTypeE.Idref:
                    return SoapIdref.Parse(value);

                case InternalPrimitiveTypeE.Entity:
                    return SoapEntity.Parse(value);
            }
            if (code != InternalPrimitiveTypeE.Invalid)
            {
                return Convert.ChangeType(value, ToTypeCode(code), CultureInfo.InvariantCulture);
            }
            return value;
        }

        internal static InternalNameSpaceE GetNameSpaceEnum(InternalPrimitiveTypeE code, Type type, WriteObjectInfo objectInfo, out string typeName)
        {
            InternalNameSpaceE none = InternalNameSpaceE.None;
            typeName = null;
            if (code != InternalPrimitiveTypeE.Invalid)
            {
                if (code == InternalPrimitiveTypeE.Char)
                {
                    none = InternalNameSpaceE.UrtSystem;
                    typeName = "System.Char";
                }
                else
                {
                    none = InternalNameSpaceE.XdrPrimitive;
                    typeName = ToXmlDataType(code);
                }
            }
            if ((none == InternalNameSpaceE.None) && (type != null))
            {
                if (type == typeofString)
                {
                    none = InternalNameSpaceE.XdrString;
                }
                else if (objectInfo == null)
                {
                    typeName = type.FullName;
                    if (type.Module.Assembly == urtAssembly)
                    {
                        none = InternalNameSpaceE.UrtSystem;
                    }
                    else
                    {
                        none = InternalNameSpaceE.UrtUser;
                    }
                }
                else
                {
                    typeName = objectInfo.GetTypeFullName();
                    if (objectInfo.GetAssemblyString().Equals(urtAssemblyString))
                    {
                        none = InternalNameSpaceE.UrtSystem;
                    }
                    else
                    {
                        none = InternalNameSpaceE.UrtUser;
                    }
                }
            }
            if (objectInfo != null)
            {
                if (!objectInfo.isSi && ((objectInfo.IsAttributeNameSpace() || objectInfo.IsCustomXmlAttribute()) || objectInfo.IsCustomXmlElement()))
                {
                    return InternalNameSpaceE.Interop;
                }
                if (objectInfo.IsCallElement())
                {
                    none = InternalNameSpaceE.CallElement;
                }
            }
            return none;
        }

        private static void InitCodeA()
        {
            codeA = new InternalPrimitiveTypeE[] { 
                InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Boolean, InternalPrimitiveTypeE.Char, InternalPrimitiveTypeE.SByte, InternalPrimitiveTypeE.Byte, InternalPrimitiveTypeE.Int16, InternalPrimitiveTypeE.UInt16, InternalPrimitiveTypeE.Int32, InternalPrimitiveTypeE.UInt32, InternalPrimitiveTypeE.Int64, InternalPrimitiveTypeE.UInt64, InternalPrimitiveTypeE.Single, InternalPrimitiveTypeE.Double, InternalPrimitiveTypeE.Decimal, 
                InternalPrimitiveTypeE.DateTime, InternalPrimitiveTypeE.Invalid, InternalPrimitiveTypeE.Invalid
             };
        }

        private static void InitEscapeA()
        {
            escapeA = new bool[primitiveTypeEnumLength];
            escapeA[0] = true;
            escapeA[1] = false;
            escapeA[2] = false;
            escapeA[3] = true;
            escapeA[5] = false;
            escapeA[6] = false;
            escapeA[7] = false;
            escapeA[8] = false;
            escapeA[9] = false;
            escapeA[10] = false;
            escapeA[11] = false;
            escapeA[12] = false;
            escapeA[13] = false;
            escapeA[14] = false;
            escapeA[15] = false;
            escapeA[0x10] = false;
            escapeA[0x11] = false;
            escapeA[0x12] = false;
            escapeA[0x13] = false;
            escapeA[20] = false;
            escapeA[0x15] = false;
            escapeA[0x16] = false;
            escapeA[0x17] = false;
            escapeA[0x18] = false;
            escapeA[0x19] = false;
            escapeA[0x1a] = false;
            escapeA[0x1b] = false;
            escapeA[0x1c] = false;
            escapeA[0x1d] = false;
            escapeA[30] = false;
            escapeA[0x1f] = true;
            escapeA[0x20] = true;
            escapeA[0x21] = true;
            escapeA[0x22] = false;
            escapeA[0x23] = true;
            escapeA[0x24] = true;
            escapeA[0x25] = true;
            escapeA[0x26] = true;
            escapeA[0x27] = true;
            escapeA[40] = true;
            escapeA[0x29] = true;
            escapeA[0x2a] = true;
            escapeA[0x2b] = true;
            escapeA[0x2c] = true;
            escapeA[0x2d] = true;
        }

        private static void InitTypeA()
        {
            typeA = new Type[primitiveTypeEnumLength];
            typeA[0] = null;
            typeA[1] = typeofBoolean;
            typeA[2] = typeofByte;
            typeA[3] = typeofChar;
            typeA[5] = typeofDecimal;
            typeA[6] = typeofDouble;
            typeA[7] = typeofInt16;
            typeA[8] = typeofInt32;
            typeA[9] = typeofInt64;
            typeA[10] = typeofSByte;
            typeA[11] = typeofSingle;
            typeA[12] = typeofTimeSpan;
            typeA[13] = typeofDateTime;
            typeA[14] = typeofUInt16;
            typeA[15] = typeofUInt32;
            typeA[0x10] = typeofUInt64;
            typeA[0x11] = typeofSoapTime;
            typeA[0x12] = typeofSoapDate;
            typeA[0x13] = typeofSoapYearMonth;
            typeA[20] = typeofSoapYear;
            typeA[0x15] = typeofSoapMonthDay;
            typeA[0x16] = typeofSoapDay;
            typeA[0x17] = typeofSoapMonth;
            typeA[0x18] = typeofSoapHexBinary;
            typeA[0x19] = typeofSoapBase64Binary;
            typeA[0x1a] = typeofSoapInteger;
            typeA[0x1b] = typeofSoapPositiveInteger;
            typeA[0x1c] = typeofSoapNonPositiveInteger;
            typeA[0x1d] = typeofSoapNonNegativeInteger;
            typeA[30] = typeofSoapNegativeInteger;
            typeA[0x1f] = typeofSoapAnyUri;
            typeA[0x20] = typeofSoapQName;
            typeA[0x21] = typeofSoapNotation;
            typeA[0x22] = typeofSoapNormalizedString;
            typeA[0x23] = typeofSoapToken;
            typeA[0x24] = typeofSoapLanguage;
            typeA[0x25] = typeofSoapName;
            typeA[0x26] = typeofSoapIdrefs;
            typeA[0x27] = typeofSoapEntities;
            typeA[40] = typeofSoapNmtoken;
            typeA[0x29] = typeofSoapNmtokens;
            typeA[0x2a] = typeofSoapNcName;
            typeA[0x2b] = typeofSoapId;
            typeA[0x2c] = typeofSoapIdref;
            typeA[0x2d] = typeofSoapEntity;
        }

        private static void InitTypeCodeA()
        {
            typeCodeA = new TypeCode[primitiveTypeEnumLength];
            typeCodeA[0] = TypeCode.Object;
            typeCodeA[1] = TypeCode.Boolean;
            typeCodeA[2] = TypeCode.Byte;
            typeCodeA[3] = TypeCode.Char;
            typeCodeA[5] = TypeCode.Decimal;
            typeCodeA[6] = TypeCode.Double;
            typeCodeA[7] = TypeCode.Int16;
            typeCodeA[8] = TypeCode.Int32;
            typeCodeA[9] = TypeCode.Int64;
            typeCodeA[10] = TypeCode.SByte;
            typeCodeA[11] = TypeCode.Single;
            typeCodeA[12] = TypeCode.Object;
            typeCodeA[13] = TypeCode.DateTime;
            typeCodeA[14] = TypeCode.UInt16;
            typeCodeA[15] = TypeCode.UInt32;
            typeCodeA[0x10] = TypeCode.UInt64;
            typeCodeA[0x11] = TypeCode.Object;
            typeCodeA[0x12] = TypeCode.Object;
            typeCodeA[0x13] = TypeCode.Object;
            typeCodeA[20] = TypeCode.Object;
            typeCodeA[0x15] = TypeCode.Object;
            typeCodeA[0x16] = TypeCode.Object;
            typeCodeA[0x17] = TypeCode.Object;
            typeCodeA[0x18] = TypeCode.Object;
            typeCodeA[0x19] = TypeCode.Object;
            typeCodeA[0x1a] = TypeCode.Object;
            typeCodeA[0x1b] = TypeCode.Object;
            typeCodeA[0x1c] = TypeCode.Object;
            typeCodeA[0x1d] = TypeCode.Object;
            typeCodeA[30] = TypeCode.Object;
            typeCodeA[0x1f] = TypeCode.Object;
            typeCodeA[0x20] = TypeCode.Object;
            typeCodeA[0x21] = TypeCode.Object;
            typeCodeA[0x22] = TypeCode.Object;
            typeCodeA[0x23] = TypeCode.Object;
            typeCodeA[0x24] = TypeCode.Object;
            typeCodeA[0x25] = TypeCode.Object;
            typeCodeA[0x26] = TypeCode.Object;
            typeCodeA[0x27] = TypeCode.Object;
            typeCodeA[40] = TypeCode.Object;
            typeCodeA[0x29] = TypeCode.Object;
            typeCodeA[0x2a] = TypeCode.Object;
            typeCodeA[0x2b] = TypeCode.Object;
            typeCodeA[0x2c] = TypeCode.Object;
            typeCodeA[0x2d] = TypeCode.Object;
        }

        private static void InitValueA()
        {
            valueA = new string[primitiveTypeEnumLength];
            valueA[0] = null;
            valueA[1] = "System.Boolean";
            valueA[2] = "System.Byte";
            valueA[3] = "System.Char";
            valueA[5] = "System.Decimal";
            valueA[6] = "System.Double";
            valueA[7] = "System.Int16";
            valueA[8] = "System.Int32";
            valueA[9] = "System.Int64";
            valueA[10] = "System.SByte";
            valueA[11] = "System.Single";
            valueA[12] = "System.TimeSpan";
            valueA[13] = "System.DateTime";
            valueA[14] = "System.UInt16";
            valueA[15] = "System.UInt32";
            valueA[0x10] = "System.UInt64";
            valueA[0x11] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapTime";
            valueA[0x12] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDate";
            valueA[0x13] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapYearMonth";
            valueA[20] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapYear";
            valueA[0x15] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonthDay";
            valueA[0x16] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDay";
            valueA[0x17] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapMonth";
            valueA[0x18] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapHexBinary";
            valueA[0x19] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapBase64Binary";
            valueA[0x1a] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapInteger";
            valueA[0x1b] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapPositiveInteger";
            valueA[0x1c] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonPositiveInteger";
            valueA[0x1d] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNonNegativeInteger";
            valueA[30] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNegativeInteger";
            valueA[0x1f] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapAnyUri";
            valueA[0x20] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapQName";
            valueA[0x21] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNotation";
            valueA[0x22] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNormalizedString";
            valueA[0x23] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapToken";
            valueA[0x24] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapLanguage";
            valueA[0x25] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapName";
            valueA[0x26] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapIdrefs";
            valueA[0x27] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapEntities";
            valueA[40] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtoken";
            valueA[0x29] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNmtokens";
            valueA[0x2a] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapNcName";
            valueA[0x2b] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapId";
            valueA[0x2c] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapIdref";
            valueA[0x2d] = "System.Runtime.Remoting.Metadata.W3cXsd2001.SoapEntity";
        }

        private static void InitValueB()
        {
            valueB = new string[primitiveTypeEnumLength];
            valueB[0] = null;
            valueB[1] = "boolean";
            valueB[2] = "unsignedByte";
            valueB[3] = "char";
            valueB[5] = "decimal";
            valueB[6] = "double";
            valueB[7] = "short";
            valueB[8] = "int";
            valueB[9] = "long";
            valueB[10] = "byte";
            valueB[11] = "float";
            valueB[12] = "duration";
            valueB[13] = "dateTime";
            valueB[14] = "unsignedShort";
            valueB[15] = "unsignedInt";
            valueB[0x10] = "unsignedLong";
            valueB[0x11] = SoapTime.XsdType;
            valueB[0x12] = SoapDate.XsdType;
            valueB[0x13] = SoapYearMonth.XsdType;
            valueB[20] = SoapYear.XsdType;
            valueB[0x15] = SoapMonthDay.XsdType;
            valueB[0x16] = SoapDay.XsdType;
            valueB[0x17] = SoapMonth.XsdType;
            valueB[0x18] = SoapHexBinary.XsdType;
            valueB[0x19] = SoapBase64Binary.XsdType;
            valueB[0x1a] = SoapInteger.XsdType;
            valueB[0x1b] = SoapPositiveInteger.XsdType;
            valueB[0x1c] = SoapNonPositiveInteger.XsdType;
            valueB[0x1d] = SoapNonNegativeInteger.XsdType;
            valueB[30] = SoapNegativeInteger.XsdType;
            valueB[0x1f] = SoapAnyUri.XsdType;
            valueB[0x20] = SoapQName.XsdType;
            valueB[0x21] = SoapNotation.XsdType;
            valueB[0x22] = SoapNormalizedString.XsdType;
            valueB[0x23] = SoapToken.XsdType;
            valueB[0x24] = SoapLanguage.XsdType;
            valueB[0x25] = SoapName.XsdType;
            valueB[0x26] = SoapIdrefs.XsdType;
            valueB[0x27] = SoapEntities.XsdType;
            valueB[40] = SoapNmtoken.XsdType;
            valueB[0x29] = SoapNmtokens.XsdType;
            valueB[0x2a] = SoapNcName.XsdType;
            valueB[0x2b] = SoapId.XsdType;
            valueB[0x2c] = SoapIdref.XsdType;
            valueB[0x2d] = SoapEntity.XsdType;
        }

        internal static bool IsEscaped(InternalPrimitiveTypeE code)
        {
            lock (typeofConverter)
            {
                if (escapeA == null)
                {
                    InitEscapeA();
                }
            }
            return escapeA[(int) code];
        }

        internal static bool IsSiTransmitType(InternalPrimitiveTypeE code)
        {
            switch (code)
            {
                case InternalPrimitiveTypeE.Invalid:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                case InternalPrimitiveTypeE.Time:
                case InternalPrimitiveTypeE.Date:
                case InternalPrimitiveTypeE.YearMonth:
                case InternalPrimitiveTypeE.Year:
                case InternalPrimitiveTypeE.MonthDay:
                case InternalPrimitiveTypeE.Day:
                case InternalPrimitiveTypeE.Month:
                case InternalPrimitiveTypeE.HexBinary:
                case InternalPrimitiveTypeE.Base64Binary:
                case InternalPrimitiveTypeE.Integer:
                case InternalPrimitiveTypeE.PositiveInteger:
                case InternalPrimitiveTypeE.NonPositiveInteger:
                case InternalPrimitiveTypeE.NonNegativeInteger:
                case InternalPrimitiveTypeE.NegativeInteger:
                case InternalPrimitiveTypeE.AnyUri:
                case InternalPrimitiveTypeE.QName:
                case InternalPrimitiveTypeE.Notation:
                case InternalPrimitiveTypeE.NormalizedString:
                case InternalPrimitiveTypeE.Token:
                case InternalPrimitiveTypeE.Language:
                case InternalPrimitiveTypeE.Name:
                case InternalPrimitiveTypeE.Idrefs:
                case InternalPrimitiveTypeE.Entities:
                case InternalPrimitiveTypeE.Nmtoken:
                case InternalPrimitiveTypeE.Nmtokens:
                case InternalPrimitiveTypeE.NcName:
                case InternalPrimitiveTypeE.Id:
                case InternalPrimitiveTypeE.Idref:
                case InternalPrimitiveTypeE.Entity:
                    return true;
            }
            return false;
        }

        internal static bool IsWriteAsByteArray(InternalPrimitiveTypeE code)
        {
            bool flag = false;
            switch (code)
            {
                case InternalPrimitiveTypeE.Boolean:
                case InternalPrimitiveTypeE.Byte:
                case InternalPrimitiveTypeE.Char:
                case InternalPrimitiveTypeE.Double:
                case InternalPrimitiveTypeE.Int16:
                case InternalPrimitiveTypeE.Int32:
                case InternalPrimitiveTypeE.Int64:
                case InternalPrimitiveTypeE.SByte:
                case InternalPrimitiveTypeE.Single:
                case InternalPrimitiveTypeE.UInt16:
                case InternalPrimitiveTypeE.UInt32:
                case InternalPrimitiveTypeE.UInt64:
                    return true;

                case InternalPrimitiveTypeE.Currency:
                case InternalPrimitiveTypeE.Decimal:
                case InternalPrimitiveTypeE.TimeSpan:
                case InternalPrimitiveTypeE.DateTime:
                    return flag;
            }
            return flag;
        }

        internal static InternalPrimitiveTypeE SoapToCode(Type type)
        {
            return ToCode(type);
        }

        internal static string SoapToComType(InternalPrimitiveTypeE code)
        {
            return ToComType(code);
        }

        internal static string SoapToString(object data, InternalPrimitiveTypeE code)
        {
            return ToString(data, code);
        }

        internal static Type SoapToType(InternalPrimitiveTypeE code)
        {
            return ToType(code);
        }

        internal static InternalPrimitiveTypeE ToCode(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("serParser", string.Format(CultureInfo.CurrentCulture, SoapUtil.GetResourceString("ArgumentNull_WithParamName"), new object[] { value }));
            }
            string str = value.ToLower(CultureInfo.InvariantCulture);
            char ch = str[0];
            InternalPrimitiveTypeE invalid = InternalPrimitiveTypeE.Invalid;
            switch (ch)
            {
                case 'a':
                    if (str == "anyuri")
                    {
                        invalid = InternalPrimitiveTypeE.AnyUri;
                    }
                    return invalid;

                case 'b':
                    if (!(str == "boolean"))
                    {
                        switch (str)
                        {
                            case "byte":
                                return InternalPrimitiveTypeE.SByte;

                            case "base64binary":
                                return InternalPrimitiveTypeE.Base64Binary;

                            case "base64":
                                invalid = InternalPrimitiveTypeE.Base64Binary;
                                break;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Boolean;

                case 'c':
                    if (!(str == "char") && !(str == "character"))
                    {
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Char;

                case 'd':
                    switch (str)
                    {
                        case "double":
                            invalid = InternalPrimitiveTypeE.Double;
                            break;

                        case "datetime":
                            return InternalPrimitiveTypeE.DateTime;

                        case "duration":
                            return InternalPrimitiveTypeE.TimeSpan;

                        case "date":
                            return InternalPrimitiveTypeE.Date;

                        case "decimal":
                            invalid = InternalPrimitiveTypeE.Decimal;
                            break;
                    }
                    return invalid;

                case 'e':
                    if (!(str == "entities"))
                    {
                        if (str == "entity")
                        {
                            invalid = InternalPrimitiveTypeE.Entity;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Entities;

                case 'f':
                    if (str == "float")
                    {
                        invalid = InternalPrimitiveTypeE.Single;
                    }
                    return invalid;

                case 'g':
                    if (!(str == "gyearmonth"))
                    {
                        if (str == "gyear")
                        {
                            return InternalPrimitiveTypeE.Year;
                        }
                        if (str == "gmonthday")
                        {
                            return InternalPrimitiveTypeE.MonthDay;
                        }
                        if (str == "gday")
                        {
                            return InternalPrimitiveTypeE.Day;
                        }
                        if (str == "gmonth")
                        {
                            invalid = InternalPrimitiveTypeE.Month;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.YearMonth;

                case 'h':
                    if (str == "hexbinary")
                    {
                        invalid = InternalPrimitiveTypeE.HexBinary;
                    }
                    return invalid;

                case 'i':
                    switch (str)
                    {
                        case "int":
                            invalid = InternalPrimitiveTypeE.Int32;
                            break;

                        case "integer":
                            return InternalPrimitiveTypeE.Integer;

                        case "idrefs":
                            return InternalPrimitiveTypeE.Idrefs;

                        case "id":
                            return InternalPrimitiveTypeE.Id;

                        case "idref":
                            invalid = InternalPrimitiveTypeE.Idref;
                            break;
                    }
                    return invalid;

                case 'l':
                    if (!(str == "long"))
                    {
                        if (str == "language")
                        {
                            invalid = InternalPrimitiveTypeE.Language;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Int64;

                case 'n':
                    if (!(str == "number"))
                    {
                        if (str == "normalizedstring")
                        {
                            return InternalPrimitiveTypeE.NormalizedString;
                        }
                        if (str == "nonpositiveinteger")
                        {
                            return InternalPrimitiveTypeE.NonPositiveInteger;
                        }
                        if (str == "negativeinteger")
                        {
                            return InternalPrimitiveTypeE.NegativeInteger;
                        }
                        if (str == "nonnegativeinteger")
                        {
                            return InternalPrimitiveTypeE.NonNegativeInteger;
                        }
                        if (str == "notation")
                        {
                            return InternalPrimitiveTypeE.Notation;
                        }
                        if (str == "nmtoken")
                        {
                            return InternalPrimitiveTypeE.Nmtoken;
                        }
                        if (str == "nmtokens")
                        {
                            return InternalPrimitiveTypeE.Nmtokens;
                        }
                        if (str == "name")
                        {
                            return InternalPrimitiveTypeE.Name;
                        }
                        if (str == "ncname")
                        {
                            invalid = InternalPrimitiveTypeE.NcName;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Decimal;

                case 'p':
                    if (str == "positiveinteger")
                    {
                        invalid = InternalPrimitiveTypeE.PositiveInteger;
                    }
                    return invalid;

                case 'q':
                    if (str == "qname")
                    {
                        invalid = InternalPrimitiveTypeE.QName;
                    }
                    return invalid;

                case 's':
                    if (!(str == "short"))
                    {
                        if (str == "system.byte")
                        {
                            return InternalPrimitiveTypeE.Byte;
                        }
                        if (str == "system.sbyte")
                        {
                            return InternalPrimitiveTypeE.SByte;
                        }
                        if (str == "system")
                        {
                            return ToCode(value.Substring(7));
                        }
                        if (str == "system.runtime.remoting.metadata")
                        {
                            invalid = ToCode(value.Substring(0x21));
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Int16;

                case 't':
                    if (!(str == "time"))
                    {
                        if (str == "token")
                        {
                            return InternalPrimitiveTypeE.Token;
                        }
                        if (str == "timeinstant")
                        {
                            return InternalPrimitiveTypeE.DateTime;
                        }
                        if (str == "timeduration")
                        {
                            invalid = InternalPrimitiveTypeE.TimeSpan;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.Time;

                case 'u':
                    if (!(str == "unsignedlong"))
                    {
                        if (str == "unsignedint")
                        {
                            return InternalPrimitiveTypeE.UInt32;
                        }
                        if (str == "unsignedshort")
                        {
                            return InternalPrimitiveTypeE.UInt16;
                        }
                        if (str == "unsignedbyte")
                        {
                            invalid = InternalPrimitiveTypeE.Byte;
                        }
                        return invalid;
                    }
                    return InternalPrimitiveTypeE.UInt64;
            }
            return InternalPrimitiveTypeE.Invalid;
        }

        internal static InternalPrimitiveTypeE ToCode(Type type)
        {
            InternalPrimitiveTypeE invalid = InternalPrimitiveTypeE.Invalid;
            if (type.IsEnum)
            {
                return (invalid = InternalPrimitiveTypeE.Invalid);
            }
            TypeCode typeCode = Type.GetTypeCode(type);
            if (typeCode == TypeCode.Object)
            {
                if (typeofISoapXsd.IsAssignableFrom(type))
                {
                    if (type == typeofSoapTime)
                    {
                        return InternalPrimitiveTypeE.Time;
                    }
                    if (type == typeofSoapDate)
                    {
                        return InternalPrimitiveTypeE.Date;
                    }
                    if (type == typeofSoapYearMonth)
                    {
                        return InternalPrimitiveTypeE.YearMonth;
                    }
                    if (type == typeofSoapYear)
                    {
                        return InternalPrimitiveTypeE.Year;
                    }
                    if (type == typeofSoapMonthDay)
                    {
                        return InternalPrimitiveTypeE.MonthDay;
                    }
                    if (type == typeofSoapDay)
                    {
                        return InternalPrimitiveTypeE.Day;
                    }
                    if (type == typeofSoapMonth)
                    {
                        return InternalPrimitiveTypeE.Month;
                    }
                    if (type == typeofSoapHexBinary)
                    {
                        return InternalPrimitiveTypeE.HexBinary;
                    }
                    if (type == typeofSoapBase64Binary)
                    {
                        return InternalPrimitiveTypeE.Base64Binary;
                    }
                    if (type == typeofSoapInteger)
                    {
                        return InternalPrimitiveTypeE.Integer;
                    }
                    if (type == typeofSoapPositiveInteger)
                    {
                        return InternalPrimitiveTypeE.PositiveInteger;
                    }
                    if (type == typeofSoapNonPositiveInteger)
                    {
                        return InternalPrimitiveTypeE.NonPositiveInteger;
                    }
                    if (type == typeofSoapNonNegativeInteger)
                    {
                        return InternalPrimitiveTypeE.NonNegativeInteger;
                    }
                    if (type == typeofSoapNegativeInteger)
                    {
                        return InternalPrimitiveTypeE.NegativeInteger;
                    }
                    if (type == typeofSoapAnyUri)
                    {
                        return InternalPrimitiveTypeE.AnyUri;
                    }
                    if (type == typeofSoapQName)
                    {
                        return InternalPrimitiveTypeE.QName;
                    }
                    if (type == typeofSoapNotation)
                    {
                        return InternalPrimitiveTypeE.Notation;
                    }
                    if (type == typeofSoapNormalizedString)
                    {
                        return InternalPrimitiveTypeE.NormalizedString;
                    }
                    if (type == typeofSoapToken)
                    {
                        return InternalPrimitiveTypeE.Token;
                    }
                    if (type == typeofSoapLanguage)
                    {
                        return InternalPrimitiveTypeE.Language;
                    }
                    if (type == typeofSoapName)
                    {
                        return InternalPrimitiveTypeE.Name;
                    }
                    if (type == typeofSoapIdrefs)
                    {
                        return InternalPrimitiveTypeE.Idrefs;
                    }
                    if (type == typeofSoapEntities)
                    {
                        return InternalPrimitiveTypeE.Entities;
                    }
                    if (type == typeofSoapNmtoken)
                    {
                        return InternalPrimitiveTypeE.Nmtoken;
                    }
                    if (type == typeofSoapNmtokens)
                    {
                        return InternalPrimitiveTypeE.Nmtokens;
                    }
                    if (type == typeofSoapNcName)
                    {
                        return InternalPrimitiveTypeE.NcName;
                    }
                    if (type == typeofSoapId)
                    {
                        return InternalPrimitiveTypeE.Id;
                    }
                    if (type == typeofSoapIdref)
                    {
                        return InternalPrimitiveTypeE.Idref;
                    }
                    if (type == typeofSoapEntity)
                    {
                        invalid = InternalPrimitiveTypeE.Entity;
                    }
                    return invalid;
                }
                if (type == typeofTimeSpan)
                {
                    return InternalPrimitiveTypeE.TimeSpan;
                }
                return InternalPrimitiveTypeE.Invalid;
            }
            return ToPrimitiveTypeEnum(typeCode);
        }

        internal static string ToComType(InternalPrimitiveTypeE code)
        {
            lock (typeofConverter)
            {
                if (valueA == null)
                {
                    InitValueA();
                }
            }
            return valueA[(int) code];
        }

        internal static InternalPrimitiveTypeE ToPrimitiveTypeEnum(TypeCode typeCode)
        {
            lock (typeofConverter)
            {
                if (codeA == null)
                {
                    InitCodeA();
                }
            }
            return codeA[(int) typeCode];
        }

        internal static string ToString(object data, InternalPrimitiveTypeE code)
        {
            switch (code)
            {
                case InternalPrimitiveTypeE.Invalid:
                    return data.ToString();

                case InternalPrimitiveTypeE.Boolean:
                    if (!((bool) data))
                    {
                        return "false";
                    }
                    return "true";

                case InternalPrimitiveTypeE.Double:
                {
                    double d = (double) data;
                    if (!double.IsPositiveInfinity(d))
                    {
                        if (double.IsNegativeInfinity(d))
                        {
                            return "-INF";
                        }
                        return d.ToString("R", CultureInfo.InvariantCulture);
                    }
                    return "INF";
                }
                case InternalPrimitiveTypeE.Single:
                {
                    float f = (float) data;
                    if (!float.IsPositiveInfinity(f))
                    {
                        if (float.IsNegativeInfinity(f))
                        {
                            return "-INF";
                        }
                        return f.ToString("R", CultureInfo.InvariantCulture);
                    }
                    return "INF";
                }
                case InternalPrimitiveTypeE.TimeSpan:
                    return SoapDuration.ToString((TimeSpan) data);

                case InternalPrimitiveTypeE.DateTime:
                    return SoapDateTime.ToString((DateTime) data);

                case InternalPrimitiveTypeE.Time:
                case InternalPrimitiveTypeE.Date:
                case InternalPrimitiveTypeE.YearMonth:
                case InternalPrimitiveTypeE.Year:
                case InternalPrimitiveTypeE.MonthDay:
                case InternalPrimitiveTypeE.Day:
                case InternalPrimitiveTypeE.Month:
                case InternalPrimitiveTypeE.HexBinary:
                case InternalPrimitiveTypeE.Base64Binary:
                case InternalPrimitiveTypeE.Integer:
                case InternalPrimitiveTypeE.PositiveInteger:
                case InternalPrimitiveTypeE.NonPositiveInteger:
                case InternalPrimitiveTypeE.NonNegativeInteger:
                case InternalPrimitiveTypeE.NegativeInteger:
                case InternalPrimitiveTypeE.AnyUri:
                case InternalPrimitiveTypeE.QName:
                case InternalPrimitiveTypeE.Notation:
                case InternalPrimitiveTypeE.NormalizedString:
                case InternalPrimitiveTypeE.Token:
                case InternalPrimitiveTypeE.Language:
                case InternalPrimitiveTypeE.Name:
                case InternalPrimitiveTypeE.Idrefs:
                case InternalPrimitiveTypeE.Entities:
                case InternalPrimitiveTypeE.Nmtoken:
                case InternalPrimitiveTypeE.Nmtokens:
                case InternalPrimitiveTypeE.NcName:
                case InternalPrimitiveTypeE.Id:
                case InternalPrimitiveTypeE.Idref:
                case InternalPrimitiveTypeE.Entity:
                    return data.ToString();
            }
            return (string) Convert.ChangeType(data, typeofString, CultureInfo.InvariantCulture);
        }

        internal static Type ToType(InternalPrimitiveTypeE code)
        {
            lock (typeofConverter)
            {
                if (typeA == null)
                {
                    InitTypeA();
                }
            }
            return typeA[(int) code];
        }

        internal static TypeCode ToTypeCode(InternalPrimitiveTypeE code)
        {
            lock (typeofConverter)
            {
                if (typeCodeA == null)
                {
                    InitTypeCodeA();
                }
            }
            return typeCodeA[(int) code];
        }

        internal static string ToXmlDataType(InternalPrimitiveTypeE code)
        {
            lock (typeofConverter)
            {
                if (valueB == null)
                {
                    InitValueB();
                }
            }
            return valueB[(int) code];
        }
    }
}

