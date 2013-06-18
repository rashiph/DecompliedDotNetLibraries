namespace System.Xml
{
    using System;
    using System.IO;

    public interface IXmlBinaryWriterInitializer
    {
        void SetOutput(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session, bool ownsStream);
    }
}

