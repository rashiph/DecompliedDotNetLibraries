namespace System.Runtime.Remoting.MetadataServices
{
    using System;
    using System.Globalization;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;

    internal static class SudsConverter
    {
        internal static Type typeofBoolean = typeof(bool);
        internal static Type typeofByte = typeof(byte);
        internal static Type typeofChar = typeof(char);
        internal static Type typeofDateTime = typeof(DateTime);
        internal static Type typeofDecimal = typeof(decimal);
        internal static Type typeofDouble = typeof(double);
        internal static Type typeofInt16 = typeof(short);
        internal static Type typeofInt32 = typeof(int);
        internal static Type typeofInt64 = typeof(long);
        internal static Type typeofISoapXsd = typeof(ISoapXsd);
        internal static Type typeofObject = typeof(object);
        internal static Type typeofSByte = typeof(sbyte);
        internal static Type typeofSingle = typeof(float);
        internal static Type typeofSoapAnyUri = typeof(SoapAnyUri);
        internal static Type typeofSoapBase64Binary = typeof(SoapBase64Binary);
        internal static Type typeofSoapDate = typeof(SoapDate);
        internal static Type typeofSoapDay = typeof(SoapDay);
        internal static Type typeofSoapEntities = typeof(SoapEntities);
        internal static Type typeofSoapEntity = typeof(SoapEntity);
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
        internal static Type typeofTimeSpan = typeof(TimeSpan);
        internal static Type typeofUInt16 = typeof(ushort);
        internal static Type typeofUInt32 = typeof(uint);
        internal static Type typeofUInt64 = typeof(ulong);
        internal static Type typeofVoid = typeof(void);
        internal static string Xsd1999 = "http://www.w3.org/1999/XMLSchema";
        internal static string Xsd2000 = "http://www.w3.org/2000/10/XMLSchema";
        internal static string Xsd2001 = "http://www.w3.org/2001/XMLSchema";
        internal static string Xsi1999 = "http://www.w3.org/1999/XMLSchema-instance";
        internal static string Xsi2000 = "http://www.w3.org/2000/10/XMLSchema-instance";
        internal static string Xsi2001 = "http://www.w3.org/2001/XMLSchema-instance";

        internal static string GetXsdVersion(XsdVersion xsdVersion)
        {
            if (xsdVersion == XsdVersion.V1999)
            {
                return Xsd1999;
            }
            if (xsdVersion == XsdVersion.V2000)
            {
                return Xsd2000;
            }
            return Xsd2001;
        }

        internal static string GetXsiVersion(XsdVersion xsdVersion)
        {
            if (xsdVersion == XsdVersion.V1999)
            {
                return Xsi1999;
            }
            if (xsdVersion == XsdVersion.V2000)
            {
                return Xsi2000;
            }
            return Xsi2001;
        }

        internal static string MapClrTypeToXsdType(Type clrType)
        {
            string xsdType = null;
            if (clrType == typeofChar)
            {
                return null;
            }
            if (clrType.IsPrimitive)
            {
                if (clrType == typeofByte)
                {
                    return "xsd:unsignedByte";
                }
                if (clrType == typeofSByte)
                {
                    return "xsd:byte";
                }
                if (clrType == typeofBoolean)
                {
                    return "xsd:boolean";
                }
                if (clrType == typeofChar)
                {
                    return "xsd:char";
                }
                if (clrType == typeofDouble)
                {
                    return "xsd:double";
                }
                if (clrType == typeofSingle)
                {
                    return "xsd:float";
                }
                if (clrType == typeofDecimal)
                {
                    return "xsd:decimal";
                }
                if (clrType == typeofDateTime)
                {
                    return "xsd:dateTime";
                }
                if (clrType == typeofInt16)
                {
                    return "xsd:short";
                }
                if (clrType == typeofInt32)
                {
                    return "xsd:int";
                }
                if (clrType == typeofInt64)
                {
                    return "xsd:long";
                }
                if (clrType == typeofUInt16)
                {
                    return "xsd:unsignedShort";
                }
                if (clrType == typeofUInt32)
                {
                    return "xsd:unsignedInt";
                }
                if (clrType == typeofUInt64)
                {
                    return "xsd:unsignedLong";
                }
                if (clrType == typeofTimeSpan)
                {
                    xsdType = "xsd:duration";
                }
                return xsdType;
            }
            if (typeofISoapXsd.IsAssignableFrom(clrType))
            {
                if (clrType == typeofSoapTime)
                {
                    xsdType = SoapTime.XsdType;
                }
                else if (clrType == typeofSoapDate)
                {
                    xsdType = SoapDate.XsdType;
                }
                else if (clrType == typeofSoapYearMonth)
                {
                    xsdType = SoapYearMonth.XsdType;
                }
                else if (clrType == typeofSoapYear)
                {
                    xsdType = SoapYear.XsdType;
                }
                else if (clrType == typeofSoapMonthDay)
                {
                    xsdType = SoapMonthDay.XsdType;
                }
                else if (clrType == typeofSoapDay)
                {
                    xsdType = SoapDay.XsdType;
                }
                else if (clrType == typeofSoapMonth)
                {
                    xsdType = SoapMonth.XsdType;
                }
                else if (clrType == typeofSoapHexBinary)
                {
                    xsdType = SoapHexBinary.XsdType;
                }
                else if (clrType == typeofSoapBase64Binary)
                {
                    xsdType = SoapBase64Binary.XsdType;
                }
                else if (clrType == typeofSoapInteger)
                {
                    xsdType = SoapInteger.XsdType;
                }
                else if (clrType == typeofSoapPositiveInteger)
                {
                    xsdType = SoapPositiveInteger.XsdType;
                }
                else if (clrType == typeofSoapNonPositiveInteger)
                {
                    xsdType = SoapNonPositiveInteger.XsdType;
                }
                else if (clrType == typeofSoapNonNegativeInteger)
                {
                    xsdType = SoapNonNegativeInteger.XsdType;
                }
                else if (clrType == typeofSoapNegativeInteger)
                {
                    xsdType = SoapNegativeInteger.XsdType;
                }
                else if (clrType == typeofSoapAnyUri)
                {
                    xsdType = SoapAnyUri.XsdType;
                }
                else if (clrType == typeofSoapQName)
                {
                    xsdType = SoapQName.XsdType;
                }
                else if (clrType == typeofSoapNotation)
                {
                    xsdType = SoapNotation.XsdType;
                }
                else if (clrType == typeofSoapNormalizedString)
                {
                    xsdType = SoapNormalizedString.XsdType;
                }
                else if (clrType == typeofSoapToken)
                {
                    xsdType = SoapToken.XsdType;
                }
                else if (clrType == typeofSoapLanguage)
                {
                    xsdType = SoapLanguage.XsdType;
                }
                else if (clrType == typeofSoapName)
                {
                    xsdType = SoapName.XsdType;
                }
                else if (clrType == typeofSoapIdrefs)
                {
                    xsdType = SoapIdrefs.XsdType;
                }
                else if (clrType == typeofSoapEntities)
                {
                    xsdType = SoapEntities.XsdType;
                }
                else if (clrType == typeofSoapNmtoken)
                {
                    xsdType = SoapNmtoken.XsdType;
                }
                else if (clrType == typeofSoapNmtokens)
                {
                    xsdType = SoapNmtokens.XsdType;
                }
                else if (clrType == typeofSoapNcName)
                {
                    xsdType = SoapNcName.XsdType;
                }
                else if (clrType == typeofSoapId)
                {
                    xsdType = SoapId.XsdType;
                }
                else if (clrType == typeofSoapIdref)
                {
                    xsdType = SoapIdref.XsdType;
                }
                else if (clrType == typeofSoapEntity)
                {
                    xsdType = SoapEntity.XsdType;
                }
                return ("xsd:" + xsdType);
            }
            if (clrType == typeofString)
            {
                return "xsd:string";
            }
            if (clrType == typeofDecimal)
            {
                return "xsd:decimal";
            }
            if (clrType == typeofObject)
            {
                return "xsd:anyType";
            }
            if (clrType == typeofVoid)
            {
                return "void";
            }
            if (clrType == typeofDateTime)
            {
                return "xsd:dateTime";
            }
            if (clrType == typeofTimeSpan)
            {
                xsdType = "xsd:duration";
            }
            return xsdType;
        }

        internal static string MapXsdToClrTypes(string xsdType)
        {
            string str = xsdType.ToLower(CultureInfo.InvariantCulture);
            string str2 = null;
            if ((xsdType == null) || (xsdType.Length == 0))
            {
                return null;
            }
            switch (str[0])
            {
                case 'a':
                    if (!(str == "anyuri"))
                    {
                        if (!(str == "anytype") && !(str == "ur-type"))
                        {
                            return str2;
                        }
                        return "Object";
                    }
                    return "SoapAnyUri";

                case 'b':
                    if (!(str == "boolean"))
                    {
                        switch (str)
                        {
                            case "byte":
                                return "SByte";

                            case "base64binary":
                                str2 = "SoapBase64Binary";
                                break;
                        }
                        return str2;
                    }
                    return "Boolean";

                case 'c':
                    if (str == "char")
                    {
                        str2 = "Char";
                    }
                    return str2;

                case 'd':
                    if (!(str == "double"))
                    {
                        if (str == "datetime")
                        {
                            return "DateTime";
                        }
                        if (str == "decimal")
                        {
                            return "Decimal";
                        }
                        if (str == "duration")
                        {
                            return "TimeSpan";
                        }
                        if (str == "date")
                        {
                            str2 = "SoapDate";
                        }
                        return str2;
                    }
                    return "Double";

                case 'e':
                    if (!(str == "entities"))
                    {
                        if (str == "entity")
                        {
                            str2 = "SoapEntity";
                        }
                        return str2;
                    }
                    return "SoapEntities";

                case 'f':
                    if (str == "float")
                    {
                        str2 = "Single";
                    }
                    return str2;

                case 'g':
                    if (!(str == "gyearmonth"))
                    {
                        if (str == "gyear")
                        {
                            return "SoapYear";
                        }
                        if (str == "gmonthday")
                        {
                            return "SoapMonthDay";
                        }
                        if (str == "gday")
                        {
                            return "SoapDay";
                        }
                        if (str == "gmonth")
                        {
                            str2 = "SoapMonth";
                        }
                        return str2;
                    }
                    return "SoapYearMonth";

                case 'h':
                    if (str == "hexbinary")
                    {
                        str2 = "SoapHexBinary";
                    }
                    return str2;

                case 'i':
                    if (!(str == "int"))
                    {
                        if (str == "integer")
                        {
                            return "SoapInteger";
                        }
                        if (str == "idrefs")
                        {
                            return "SoapIdrefs";
                        }
                        if (str == "id")
                        {
                            return "SoapId";
                        }
                        if (str == "idref")
                        {
                            str2 = "SoapIdref";
                        }
                        return str2;
                    }
                    return "Int32";

                case 'j':
                case 'k':
                case 'm':
                case 'o':
                case 'r':
                    return str2;

                case 'l':
                    if (!(str == "long"))
                    {
                        if (str == "language")
                        {
                            str2 = "SoapLanguage";
                        }
                        return str2;
                    }
                    return "Int64";

                case 'n':
                    if (!(str == "number"))
                    {
                        if (str == "normalizedstring")
                        {
                            return "SoapNormalizedString";
                        }
                        if (str == "nonpositiveinteger")
                        {
                            return "SoapNonPositiveInteger";
                        }
                        if (str == "negativeinteger")
                        {
                            return "SoapNegativeInteger";
                        }
                        if (str == "nonnegativeinteger")
                        {
                            return "SoapNonNegativeInteger";
                        }
                        if (str == "notation")
                        {
                            return "SoapNotation";
                        }
                        if (str == "nmtoken")
                        {
                            return "SoapNmtoken";
                        }
                        if (str == "nmtokens")
                        {
                            return "SoapNmtokens";
                        }
                        if (str == "name")
                        {
                            return "SoapName";
                        }
                        if (str == "ncname")
                        {
                            str2 = "SoapNcName";
                        }
                        return str2;
                    }
                    return "Decimal";

                case 'p':
                    if (str == "positiveinteger")
                    {
                        str2 = "SoapPositiveInteger";
                    }
                    return str2;

                case 'q':
                    if (str == "qname")
                    {
                        str2 = "SoapQName";
                    }
                    return str2;

                case 's':
                    if (!(str == "string"))
                    {
                        if (str == "short")
                        {
                            str2 = "Int16";
                        }
                        return str2;
                    }
                    return "String";

                case 't':
                    if (!(str == "time"))
                    {
                        if (str == "token")
                        {
                            str2 = "SoapToken";
                        }
                        return str2;
                    }
                    return "SoapTime";

                case 'u':
                    if (!(str == "unsignedlong"))
                    {
                        if (str == "unsignedint")
                        {
                            return "UInt32";
                        }
                        if (str == "unsignedshort")
                        {
                            return "UInt16";
                        }
                        if (str == "unsignedbyte")
                        {
                            str2 = "Byte";
                        }
                        return str2;
                    }
                    return "UInt64";
            }
            return str2;
        }
    }
}

