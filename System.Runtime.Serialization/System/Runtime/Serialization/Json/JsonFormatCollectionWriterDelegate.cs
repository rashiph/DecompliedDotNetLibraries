namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    internal delegate void JsonFormatCollectionWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContextComplexJson context, CollectionDataContract dataContract);
}

