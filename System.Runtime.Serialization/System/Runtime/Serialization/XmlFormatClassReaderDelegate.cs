namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal delegate object XmlFormatClassReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString[] memberNames, XmlDictionaryString[] memberNamespaces);
}

