namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal delegate object XmlFormatCollectionReaderDelegate(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context, XmlDictionaryString itemName, XmlDictionaryString itemNamespace, CollectionDataContract collectionContract);
}

