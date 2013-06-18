namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Xml;

    internal static class JsonGlobals
    {
        public const string applicationJsonMediaType = "application/json";
        public const string arrayString = "array";
        public const string booleanString = "boolean";
        public const string CacheControlString = "Cache-Control";
        public const byte CollectionByte = 0x5b;
        public const char CollectionChar = '[';
        public static readonly int DataContractXsdBaseNamespaceLength = "http://schemas.datacontract.org/2004/07/".Length;
        public const string DateTimeEndGuardReader = ")/";
        public const string DateTimeEndGuardWriter = @")\/";
        public const string DateTimeStartGuardReader = "/Date(";
        public const string DateTimeStartGuardWriter = @"\/Date(";
        public static readonly XmlDictionaryString dDictionaryString = new XmlDictionary().Add("d");
        public const string dString = "d";
        public const byte EndCollectionByte = 0x5d;
        public const char EndCollectionChar = ']';
        public const byte EndObjectByte = 0x7d;
        public const char EndObjectChar = '}';
        public const string ExpiresString = "Expires";
        public static readonly char[] floatingPointCharacters = new char[] { '.', 'e' };
        public const string IfModifiedSinceString = "If-Modified-Since";
        public static readonly XmlDictionaryString itemDictionaryString = new XmlDictionary().Add("item");
        public const string itemString = "item";
        public const string jsonerrorString = "jsonerror";
        public const string KeyString = "Key";
        public const string LastModifiedString = "Last-Modified";
        public const int maxScopeSize = 0x19;
        public const byte MemberSeparatorByte = 0x2c;
        public const char MemberSeparatorChar = ',';
        public const byte NameValueSeparatorByte = 0x3a;
        public const char NameValueSeparatorChar = ':';
        public const string NameValueSeparatorString = ":";
        public const string nullString = "null";
        public const string numberString = "number";
        public const byte ObjectByte = 0x7b;
        public const char ObjectChar = '{';
        public const string objectString = "object";
        public const string publicString = "public";
        public const byte QuoteByte = 0x22;
        public const char QuoteChar = '"';
        public static readonly XmlDictionaryString rootDictionaryString = new XmlDictionary().Add("root");
        public const string rootString = "root";
        public const string serverTypeString = "__type";
        public const string stringString = "string";
        public const string textJsonMediaType = "text/json";
        public const string trueString = "true";
        public const string typeString = "type";
        public static readonly long unixEpochTicks;
        public const string ValueString = "Value";
        public const char WhitespaceChar = ' ';
        public const string xmlnsPrefix = "xmlns";
        public const string xmlPrefix = "xml";

        static JsonGlobals()
        {
            DateTime time = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            unixEpochTicks = time.Ticks;
        }
    }
}

