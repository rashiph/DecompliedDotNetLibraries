namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Xml;

    internal class BinaryFormatBuilder
    {
        private List<byte> bytes = new List<byte>();

        private void AppendByte(int value)
        {
            if ((value < 0) || (value > 0xff))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, 0xff })));
            }
            this.bytes.Add((byte) value);
        }

        public void AppendDictionaryAttribute(char prefix, int key, char value)
        {
            this.AppendNode(System.Xml.XmlBinaryNodeType.DictionaryAttribute);
            this.AppendUtf8(prefix);
            this.AppendKey(key);
            this.AppendNode(System.Xml.XmlBinaryNodeType.Chars8Text);
            this.AppendUtf8(value);
        }

        public void AppendDictionaryTextWithEndElement()
        {
            this.AppendNode(System.Xml.XmlBinaryNodeType.DictionaryTextWithEndElement);
        }

        public void AppendDictionaryTextWithEndElement(int key)
        {
            this.AppendNode(System.Xml.XmlBinaryNodeType.DictionaryTextWithEndElement);
            this.AppendKey(key);
        }

        public void AppendDictionaryXmlnsAttribute(char prefix, int key)
        {
            this.AppendNode(System.Xml.XmlBinaryNodeType.DictionaryXmlnsAttribute);
            this.AppendUtf8(prefix);
            this.AppendKey(key);
        }

        public void AppendEndElement()
        {
            this.AppendNode(System.Xml.XmlBinaryNodeType.EndElement);
        }

        private void AppendKey(int key)
        {
            if ((key < 0) || (key >= 0x4000))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("key", key, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 0, 0x4000 })));
            }
            if (key >= 0x80)
            {
                this.AppendByte((key & 0x7f) | 0x80);
                this.AppendByte(key >> 7);
            }
            else
            {
                this.AppendByte(key);
            }
        }

        private void AppendNode(System.Xml.XmlBinaryNodeType value)
        {
            this.AppendByte((int) value);
        }

        public void AppendPrefixDictionaryAttribute(char prefix, int key, char value)
        {
            this.AppendNode((System.Xml.XmlBinaryNodeType) (12 + this.GetPrefixOffset(prefix)));
            this.AppendKey(key);
            if (value == '1')
            {
                this.AppendNode(System.Xml.XmlBinaryNodeType.OneText);
            }
            else
            {
                this.AppendNode(System.Xml.XmlBinaryNodeType.Chars8Text);
                this.AppendUtf8(value);
            }
        }

        public void AppendPrefixDictionaryElement(char prefix, int key)
        {
            this.AppendNode((System.Xml.XmlBinaryNodeType) (0x44 + this.GetPrefixOffset(prefix)));
            this.AppendKey(key);
        }

        public void AppendUniqueIDWithEndElement()
        {
            this.AppendNode(System.Xml.XmlBinaryNodeType.UniqueIdTextWithEndElement);
        }

        private void AppendUtf8(char value)
        {
            this.AppendByte(1);
            this.AppendByte(value);
        }

        private int GetPrefixOffset(char prefix)
        {
            if ((prefix < 'a') && (prefix > 'z'))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("prefix", prefix, System.ServiceModel.SR.GetString("ValueMustBeInRange", new object[] { 'a', 'z' })));
            }
            return (prefix - 'a');
        }

        public int GetSessionKey(int value)
        {
            return ((value * 2) + 1);
        }

        public int GetStaticKey(int value)
        {
            return (value * 2);
        }

        public byte[] ToByteArray()
        {
            byte[] buffer = this.bytes.ToArray();
            this.bytes.Clear();
            return buffer;
        }

        public int Count
        {
            get
            {
                return this.bytes.Count;
            }
        }
    }
}

