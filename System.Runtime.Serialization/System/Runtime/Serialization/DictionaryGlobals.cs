namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;
    using System.Xml;

    internal static class DictionaryGlobals
    {
        public static readonly XmlDictionaryString ArraySizeLocalName;
        public static readonly XmlDictionaryString AsmxTypesNamespace;
        public static readonly XmlDictionaryString BooleanLocalName;
        public static readonly XmlDictionaryString ByteArrayLocalName;
        public static readonly XmlDictionaryString CharLocalName;
        public static readonly XmlDictionaryString ClrAssemblyLocalName;
        public static readonly XmlDictionaryString ClrTypeLocalName;
        public static readonly XmlDictionaryString dateLocalName;
        public static readonly XmlDictionaryString DateTimeLocalName;
        public static readonly XmlDictionaryString DecimalLocalName;
        public static readonly XmlDictionaryString DoubleLocalName;
        public static readonly XmlDictionaryString EmptyString;
        public static readonly XmlDictionaryString ENTITIESLocalName;
        public static readonly XmlDictionaryString ENTITYLocalName;
        public static readonly XmlDictionaryString FloatLocalName;
        public static readonly XmlDictionaryString gDayLocalName;
        public static readonly XmlDictionaryString gMonthDayLocalName;
        public static readonly XmlDictionaryString gMonthLocalName;
        public static readonly XmlDictionaryString GuidLocalName;
        public static readonly XmlDictionaryString gYearLocalName;
        public static readonly XmlDictionaryString gYearMonthLocalName;
        public static readonly XmlDictionaryString hexBinaryLocalName;
        public static readonly XmlDictionaryString IdLocalName;
        public static readonly XmlDictionaryString IDREFLocalName;
        public static readonly XmlDictionaryString IDREFSLocalName;
        public static readonly XmlDictionaryString integerLocalName;
        public static readonly XmlDictionaryString IntLocalName;
        public static readonly XmlDictionaryString ISerializableFactoryTypeLocalName;
        public static readonly XmlDictionaryString languageLocalName;
        public static readonly XmlDictionaryString LongLocalName;
        public static readonly XmlDictionaryString NameLocalName;
        public static readonly XmlDictionaryString NCNameLocalName;
        public static readonly XmlDictionaryString negativeIntegerLocalName;
        public static readonly XmlDictionaryString NMTOKENLocalName;
        public static readonly XmlDictionaryString NMTOKENSLocalName;
        public static readonly XmlDictionaryString nonNegativeIntegerLocalName;
        public static readonly XmlDictionaryString nonPositiveIntegerLocalName;
        public static readonly XmlDictionaryString normalizedStringLocalName;
        public static readonly XmlDictionaryString ObjectLocalName;
        public static readonly XmlDictionaryString positiveIntegerLocalName;
        public static readonly XmlDictionaryString QNameLocalName;
        public static readonly XmlDictionaryString RefLocalName;
        public static readonly XmlDictionaryString SchemaInstanceNamespace;
        public static readonly XmlDictionaryString SchemaNamespace;
        public static readonly XmlDictionaryString SerializationNamespace;
        public static readonly XmlDictionaryString ShortLocalName;
        public static readonly XmlDictionaryString SignedByteLocalName;
        public static readonly XmlDictionaryString Space;
        public static readonly XmlDictionaryString StringLocalName;
        public static readonly XmlDictionaryString timeLocalName;
        public static readonly XmlDictionaryString TimeSpanLocalName;
        public static readonly XmlDictionaryString tokenLocalName;
        public static readonly XmlDictionaryString UnsignedByteLocalName;
        public static readonly XmlDictionaryString UnsignedIntLocalName;
        public static readonly XmlDictionaryString UnsignedLongLocalName;
        public static readonly XmlDictionaryString UnsignedShortLocalName;
        public static readonly XmlDictionaryString UriLocalName;
        public static readonly XmlDictionaryString XmlnsNamespace;
        public static readonly XmlDictionaryString XSDIDLocalName;
        public static readonly XmlDictionaryString XsiNilLocalName;
        public static readonly XmlDictionaryString XsiTypeLocalName;

        static DictionaryGlobals()
        {
            XmlDictionary dictionary = new XmlDictionary(0x3d);
            try
            {
                SchemaInstanceNamespace = dictionary.Add("http://www.w3.org/2001/XMLSchema-instance");
                SerializationNamespace = dictionary.Add("http://schemas.microsoft.com/2003/10/Serialization/");
                SchemaNamespace = dictionary.Add("http://www.w3.org/2001/XMLSchema");
                XsiTypeLocalName = dictionary.Add("type");
                XsiNilLocalName = dictionary.Add("nil");
                IdLocalName = dictionary.Add("Id");
                RefLocalName = dictionary.Add("Ref");
                ArraySizeLocalName = dictionary.Add("Size");
                EmptyString = dictionary.Add(string.Empty);
                ISerializableFactoryTypeLocalName = dictionary.Add("FactoryType");
                XmlnsNamespace = dictionary.Add("http://www.w3.org/2000/xmlns/");
                CharLocalName = dictionary.Add("char");
                BooleanLocalName = dictionary.Add("boolean");
                SignedByteLocalName = dictionary.Add("byte");
                UnsignedByteLocalName = dictionary.Add("unsignedByte");
                ShortLocalName = dictionary.Add("short");
                UnsignedShortLocalName = dictionary.Add("unsignedShort");
                IntLocalName = dictionary.Add("int");
                UnsignedIntLocalName = dictionary.Add("unsignedInt");
                LongLocalName = dictionary.Add("long");
                UnsignedLongLocalName = dictionary.Add("unsignedLong");
                FloatLocalName = dictionary.Add("float");
                DoubleLocalName = dictionary.Add("double");
                DecimalLocalName = dictionary.Add("decimal");
                DateTimeLocalName = dictionary.Add("dateTime");
                StringLocalName = dictionary.Add("string");
                ByteArrayLocalName = dictionary.Add("base64Binary");
                ObjectLocalName = dictionary.Add("anyType");
                TimeSpanLocalName = dictionary.Add("duration");
                GuidLocalName = dictionary.Add("guid");
                UriLocalName = dictionary.Add("anyURI");
                QNameLocalName = dictionary.Add("QName");
                ClrTypeLocalName = dictionary.Add("Type");
                ClrAssemblyLocalName = dictionary.Add("Assembly");
                Space = dictionary.Add(" ");
                timeLocalName = dictionary.Add("time");
                dateLocalName = dictionary.Add("date");
                hexBinaryLocalName = dictionary.Add("hexBinary");
                gYearMonthLocalName = dictionary.Add("gYearMonth");
                gYearLocalName = dictionary.Add("gYear");
                gMonthDayLocalName = dictionary.Add("gMonthDay");
                gDayLocalName = dictionary.Add("gDay");
                gMonthLocalName = dictionary.Add("gMonth");
                integerLocalName = dictionary.Add("integer");
                positiveIntegerLocalName = dictionary.Add("positiveInteger");
                negativeIntegerLocalName = dictionary.Add("negativeInteger");
                nonPositiveIntegerLocalName = dictionary.Add("nonPositiveInteger");
                nonNegativeIntegerLocalName = dictionary.Add("nonNegativeInteger");
                normalizedStringLocalName = dictionary.Add("normalizedString");
                tokenLocalName = dictionary.Add("token");
                languageLocalName = dictionary.Add("language");
                NameLocalName = dictionary.Add("Name");
                NCNameLocalName = dictionary.Add("NCName");
                XSDIDLocalName = dictionary.Add("ID");
                IDREFLocalName = dictionary.Add("IDREF");
                IDREFSLocalName = dictionary.Add("IDREFS");
                ENTITYLocalName = dictionary.Add("ENTITY");
                ENTITIESLocalName = dictionary.Add("ENTITIES");
                NMTOKENLocalName = dictionary.Add("NMTOKEN");
                NMTOKENSLocalName = dictionary.Add("NMTOKENS");
                AsmxTypesNamespace = dictionary.Add("http://microsoft.com/wsdl/types/");
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperFatal(exception.Message, exception);
            }
        }
    }
}

