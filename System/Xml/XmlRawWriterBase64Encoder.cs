namespace System.Xml
{
    using System;

    internal class XmlRawWriterBase64Encoder : Base64Encoder
    {
        private XmlRawWriter rawWriter;

        internal XmlRawWriterBase64Encoder(XmlRawWriter rawWriter)
        {
            this.rawWriter = rawWriter;
        }

        internal override void WriteChars(char[] chars, int index, int count)
        {
            this.rawWriter.WriteRaw(chars, index, count);
        }
    }
}

