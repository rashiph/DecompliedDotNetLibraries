namespace System.Xml
{
    using System;
    using System.IO;
    using System.Text;

    public interface IXmlTextWriterInitializer
    {
        void SetOutput(Stream stream, Encoding encoding, bool ownsStream);
    }
}

