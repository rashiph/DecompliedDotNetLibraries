namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Text;

    internal class XmlSigningNodeWriter : XmlNodeWriter
    {
        private byte[] base64Chars;
        private byte[] chars;
        private XmlCanonicalWriter signingWriter;
        private bool text;
        private XmlNodeWriter writer;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlSigningNodeWriter(bool text)
        {
            this.text = text;
        }

        public override void Close()
        {
            this.writer.Close();
            this.signingWriter.Close();
        }

        public override void Flush()
        {
            this.writer.Flush();
            this.signingWriter.Flush();
        }

        public void SetOutput(XmlNodeWriter writer, Stream stream, bool includeComments, string[] inclusivePrefixes)
        {
            this.writer = writer;
            if (this.signingWriter == null)
            {
                this.signingWriter = new XmlCanonicalWriter();
            }
            this.signingWriter.SetOutput(stream, includeComments, inclusivePrefixes);
            this.chars = new byte[0x40];
            this.base64Chars = null;
        }

        private void WriteBase64Text(byte[] buffer, int offset, int count)
        {
            if (this.base64Chars == null)
            {
                this.base64Chars = new byte[0x200];
            }
            Base64Encoding encoding = XmlConverter.Base64Encoding;
            while (count >= 3)
            {
                int byteCount = Math.Min((int) ((this.base64Chars.Length / 4) * 3), (int) (count - (count % 3)));
                int num2 = (byteCount / 3) * 4;
                encoding.GetChars(buffer, offset, byteCount, this.base64Chars, 0);
                this.signingWriter.WriteText(this.base64Chars, 0, num2);
                if (this.text)
                {
                    this.writer.WriteText(this.base64Chars, 0, num2);
                }
                offset += byteCount;
                count -= byteCount;
            }
            if (count > 0)
            {
                encoding.GetChars(buffer, offset, count, this.base64Chars, 0);
                this.signingWriter.WriteText(this.base64Chars, 0, 4);
                if (this.text)
                {
                    this.writer.WriteText(this.base64Chars, 0, 4);
                }
            }
        }

        public override void WriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count)
        {
            if (trailByteCount > 0)
            {
                this.WriteBase64Text(trailBytes, 0, trailByteCount);
            }
            this.WriteBase64Text(buffer, offset, count);
            if (!this.text)
            {
                this.writer.WriteBase64Text(trailBytes, trailByteCount, buffer, offset, count);
            }
        }

        public override void WriteBoolText(bool value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteBoolText(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteCData(string text)
        {
            this.writer.WriteCData(text);
            this.signingWriter.WriteEscapedText(text);
        }

        public override void WriteCharEntity(int ch)
        {
            this.writer.WriteCharEntity(ch);
            this.signingWriter.WriteCharEntity(ch);
        }

        public override void WriteComment(string text)
        {
            this.writer.WriteComment(text);
            this.signingWriter.WriteComment(text);
        }

        public override void WriteDateTimeText(DateTime value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteDateTimeText(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteDecimalText(decimal value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteDecimalText(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteDeclaration()
        {
            this.writer.WriteDeclaration();
            this.signingWriter.WriteDeclaration();
        }

        public override void WriteDoubleText(double value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteDoubleText(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteEndAttribute()
        {
            this.writer.WriteEndAttribute();
            this.signingWriter.WriteEndAttribute();
        }

        public override void WriteEndElement(string prefix, string localName)
        {
            this.writer.WriteEndElement(prefix, localName);
            this.signingWriter.WriteEndElement(prefix, localName);
        }

        public override void WriteEndListText()
        {
            this.writer.WriteEndListText();
        }

        public override void WriteEndStartElement(bool isEmpty)
        {
            this.writer.WriteEndStartElement(isEmpty);
            this.signingWriter.WriteEndStartElement(isEmpty);
        }

        public override void WriteEscapedText(string value)
        {
            this.writer.WriteEscapedText(value);
            this.signingWriter.WriteEscapedText(value);
        }

        public override void WriteEscapedText(XmlDictionaryString value)
        {
            this.writer.WriteEscapedText(value);
            this.signingWriter.WriteEscapedText(value.Value);
        }

        public override void WriteEscapedText(byte[] chars, int offset, int count)
        {
            this.writer.WriteEscapedText(chars, offset, count);
            this.signingWriter.WriteEscapedText(chars, offset, count);
        }

        public override void WriteEscapedText(char[] chars, int offset, int count)
        {
            this.writer.WriteEscapedText(chars, offset, count);
            this.signingWriter.WriteEscapedText(chars, offset, count);
        }

        public override void WriteFloatText(float value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteFloatText(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteGuidText(Guid value)
        {
            string str = XmlConverter.ToString(value);
            if (this.text)
            {
                this.writer.WriteText(str);
            }
            else
            {
                this.writer.WriteGuidText(value);
            }
            this.signingWriter.WriteText(str);
        }

        public override void WriteInt32Text(int value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteInt32Text(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteInt64Text(long value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteInt64Text(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteListSeparator()
        {
            this.writer.WriteListSeparator();
            this.signingWriter.WriteText(0x20);
        }

        public override void WriteQualifiedName(string prefix, XmlDictionaryString localName)
        {
            this.writer.WriteQualifiedName(prefix, localName);
            if (prefix.Length != 0)
            {
                this.signingWriter.WriteText(prefix);
                this.signingWriter.WriteText(":");
            }
            this.signingWriter.WriteText(localName.Value);
        }

        public override void WriteStartAttribute(string prefix, string localName)
        {
            this.writer.WriteStartAttribute(prefix, localName);
            this.signingWriter.WriteStartAttribute(prefix, localName);
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName)
        {
            this.writer.WriteStartAttribute(prefix, localName);
            this.signingWriter.WriteStartAttribute(prefix, localName.Value);
        }

        public override void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            this.writer.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
            this.signingWriter.WriteStartAttribute(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
        }

        public override void WriteStartElement(string prefix, string localName)
        {
            this.writer.WriteStartElement(prefix, localName);
            this.signingWriter.WriteStartElement(prefix, localName);
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName)
        {
            this.writer.WriteStartElement(prefix, localName);
            this.signingWriter.WriteStartElement(prefix, localName.Value);
        }

        public override void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            this.writer.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
            this.signingWriter.WriteStartElement(prefixBuffer, prefixOffset, prefixLength, localNameBuffer, localNameOffset, localNameLength);
        }

        public override void WriteStartListText()
        {
            this.writer.WriteStartListText();
        }

        public override void WriteText(string value)
        {
            this.writer.WriteText(value);
            this.signingWriter.WriteText(value);
        }

        public override void WriteText(XmlDictionaryString value)
        {
            this.writer.WriteText(value);
            this.signingWriter.WriteText(value.Value);
        }

        public override void WriteText(byte[] chars, int offset, int count)
        {
            this.writer.WriteText(chars, offset, count);
            this.signingWriter.WriteText(chars, offset, count);
        }

        public override void WriteText(char[] chars, int offset, int count)
        {
            this.writer.WriteText(chars, offset, count);
            this.signingWriter.WriteText(chars, offset, count);
        }

        public override void WriteTimeSpanText(TimeSpan value)
        {
            string str = XmlConverter.ToString(value);
            if (this.text)
            {
                this.writer.WriteText(str);
            }
            else
            {
                this.writer.WriteTimeSpanText(value);
            }
            this.signingWriter.WriteText(str);
        }

        public override void WriteUInt64Text(ulong value)
        {
            int count = XmlConverter.ToChars(value, this.chars, 0);
            if (this.text)
            {
                this.writer.WriteText(this.chars, 0, count);
            }
            else
            {
                this.writer.WriteUInt64Text(value);
            }
            this.signingWriter.WriteText(this.chars, 0, count);
        }

        public override void WriteUniqueIdText(UniqueId value)
        {
            string str = XmlConverter.ToString(value);
            if (this.text)
            {
                this.writer.WriteText(str);
            }
            else
            {
                this.writer.WriteUniqueIdText(value);
            }
            this.signingWriter.WriteText(str);
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            this.writer.WriteXmlnsAttribute(prefix, ns);
            this.signingWriter.WriteXmlnsAttribute(prefix, ns);
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            this.writer.WriteXmlnsAttribute(prefix, ns);
            this.signingWriter.WriteXmlnsAttribute(prefix, ns.Value);
        }

        public override void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            this.writer.WriteXmlnsAttribute(prefixBuffer, prefixOffset, prefixLength, nsBuffer, nsOffset, nsLength);
            this.signingWriter.WriteXmlnsAttribute(prefixBuffer, prefixOffset, prefixLength, nsBuffer, nsOffset, nsLength);
        }

        public XmlCanonicalWriter CanonicalWriter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.signingWriter;
            }
        }

        public XmlNodeWriter NodeWriter
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.writer;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.writer = value;
            }
        }
    }
}

