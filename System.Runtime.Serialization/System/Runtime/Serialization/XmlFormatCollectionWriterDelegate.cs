namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void XmlFormatCollectionWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, CollectionDataContract dataContract);
}

