namespace System.Xml
{
    using System;

    internal class XmlTextWriterBase64Encoder : Base64Encoder
    {
        private XmlTextEncoder xmlTextEncoder;

        internal XmlTextWriterBase64Encoder(XmlTextEncoder xmlTextEncoder)
        {
            this.xmlTextEncoder = xmlTextEncoder;
        }

        internal override void WriteChars(char[] chars, int index, int count)
        {
            this.xmlTextEncoder.WriteRaw(chars, index, count);
        }
    }
}

