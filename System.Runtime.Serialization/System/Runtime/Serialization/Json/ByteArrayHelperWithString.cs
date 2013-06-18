namespace System.Runtime.Serialization.Json
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Xml;

    internal class ByteArrayHelperWithString : ArrayHelper<string, byte>
    {
        public static readonly ByteArrayHelperWithString Instance = new ByteArrayHelperWithString();

        protected override int ReadArray(XmlDictionaryReader reader, string localName, string namespaceUri, byte[] array, int offset, int count)
        {
            XmlJsonReader.CheckArray(array, offset, count);
            int num = 0;
            while ((num < count) && reader.IsStartElement("item", string.Empty))
            {
                array[offset + num] = this.ToByte(reader.ReadElementContentAsInt());
                num++;
            }
            return num;
        }

        private void ThrowConversionException(string value, string type)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(System.Runtime.Serialization.SR.GetString("XmlInvalidConversion", new object[] { value, type })));
        }

        private byte ToByte(int value)
        {
            if ((value < 0) || (value > 0xff))
            {
                this.ThrowConversionException(value.ToString(NumberFormatInfo.CurrentInfo), "Byte");
            }
            return (byte) value;
        }

        internal void WriteArray(XmlWriter writer, byte[] array, int offset, int count)
        {
            XmlJsonReader.CheckArray(array, offset, count);
            writer.WriteAttributeString(string.Empty, "type", string.Empty, "array");
            for (int i = 0; i < count; i++)
            {
                writer.WriteStartElement("item", string.Empty);
                writer.WriteAttributeString(string.Empty, "type", string.Empty, "number");
                writer.WriteValue((int) array[offset + i]);
                writer.WriteEndElement();
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected override void WriteArray(XmlDictionaryWriter writer, string prefix, string localName, string namespaceUri, byte[] array, int offset, int count)
        {
            this.WriteArray(writer, array, offset, count);
        }
    }
}

