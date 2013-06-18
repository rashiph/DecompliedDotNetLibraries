namespace System.Xml
{
    using System;
    using System.IO;
    using System.Text;

    public interface IXmlTextReaderInitializer
    {
        void SetInput(Stream stream, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose);
        void SetInput(byte[] buffer, int offset, int count, Encoding encoding, XmlDictionaryReaderQuotas quotas, OnXmlDictionaryReaderClose onClose);
    }
}

