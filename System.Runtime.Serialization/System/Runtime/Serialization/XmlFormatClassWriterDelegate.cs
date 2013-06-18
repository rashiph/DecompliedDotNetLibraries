namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void XmlFormatClassWriterDelegate(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context, ClassDataContract dataContract);
}

