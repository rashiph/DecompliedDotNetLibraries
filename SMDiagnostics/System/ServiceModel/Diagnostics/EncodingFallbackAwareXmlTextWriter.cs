namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class EncodingFallbackAwareXmlTextWriter : XmlTextWriter
    {
        private Encoding encoding;

        internal EncodingFallbackAwareXmlTextWriter(TextWriter writer) : base(writer)
        {
            this.encoding = writer.Encoding;
        }

        private bool ContainsInvalidXmlChar(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                int num = 0;
                int length = value.Length;
                while (num < length)
                {
                    if (XmlConvert.IsXmlChar(value[num]))
                    {
                        num++;
                    }
                    else
                    {
                        if (((num + 1) < length) && XmlConvert.IsXmlSurrogatePair(value[num + 1], value[num]))
                        {
                            num += 2;
                            continue;
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        public override void WriteString(string value)
        {
            if (!string.IsNullOrEmpty(value) && this.ContainsInvalidXmlChar(value))
            {
                byte[] bytes = this.encoding.GetBytes(value);
                value = this.encoding.GetString(bytes);
            }
            base.WriteString(value);
        }
    }
}

