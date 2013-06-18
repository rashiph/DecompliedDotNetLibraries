namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Security;
    using System.Text;

    internal class XmlUTF8NodeWriter : XmlStreamNodeWriter
    {
        private const int bufferLength = 0x200;
        private char[] chars;
        private static readonly bool[] defaultIsEscapedAttributeChar = new bool[] { 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            false, false, true, false, false, false, true, false, false, false, false, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false
         };
        private static readonly bool[] defaultIsEscapedElementChar = new bool[] { 
            true, true, true, true, true, true, true, true, true, false, false, true, true, true, true, true, 
            true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, 
            false, false, false, false, false, false, true, false, false, false, false, false, false, false, false, false, 
            false, false, false, false, false, false, false, false, false, false, false, false, true, false, true, false
         };
        private static readonly byte[] digits = new byte[] { 0x30, 0x31, 50, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 70 };
        private System.Text.Encoding encoding;
        private static readonly byte[] endDecl = new byte[] { 0x22, 0x3f, 0x3e };
        private byte[] entityChars;
        private bool inAttribute;
        private bool[] isEscapedAttributeChar;
        private bool[] isEscapedElementChar;
        private const int maxBytesPerChar = 3;
        private const int maxEntityLength = 0x20;
        private static readonly byte[] startDecl = new byte[] { 
            60, 0x3f, 120, 0x6d, 0x6c, 0x20, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 110, 0x3d, 0x22, 0x31, 
            0x2e, 0x30, 0x22, 0x20, 0x65, 110, 0x63, 0x6f, 100, 0x69, 110, 0x67, 0x3d, 0x22
         };
        private static readonly byte[] utf8Decl = new byte[] { 
            60, 0x3f, 120, 0x6d, 0x6c, 0x20, 0x76, 0x65, 0x72, 0x73, 0x69, 0x6f, 110, 0x3d, 0x22, 0x31, 
            0x2e, 0x30, 0x22, 0x20, 0x65, 110, 0x63, 0x6f, 100, 0x69, 110, 0x67, 0x3d, 0x22, 0x75, 0x74, 
            0x66, 0x2d, 0x38, 0x22, 0x3f, 0x3e
         };

        public XmlUTF8NodeWriter() : this(defaultIsEscapedAttributeChar, defaultIsEscapedElementChar)
        {
        }

        public XmlUTF8NodeWriter(bool[] isEscapedAttributeChar, bool[] isEscapedElementChar)
        {
            this.isEscapedAttributeChar = isEscapedAttributeChar;
            this.isEscapedElementChar = isEscapedElementChar;
            this.inAttribute = false;
        }

        private char[] GetCharBuffer(int charCount)
        {
            if (charCount >= 0x100)
            {
                return new char[charCount];
            }
            if ((this.chars == null) || (this.chars.Length < charCount))
            {
                this.chars = new char[charCount];
            }
            return this.chars;
        }

        private byte[] GetCharEntityBuffer()
        {
            if (this.entityChars == null)
            {
                this.entityChars = new byte[0x20];
            }
            return this.entityChars;
        }

        public void SetOutput(Stream stream, bool ownsStream, System.Text.Encoding encoding)
        {
            System.Text.Encoding encoding2 = null;
            if ((encoding != null) && (encoding.CodePage == System.Text.Encoding.UTF8.CodePage))
            {
                encoding2 = encoding;
                encoding = null;
            }
            base.SetOutput(stream, ownsStream, encoding2);
            this.encoding = encoding;
            this.inAttribute = false;
        }

        private int ToBase16(byte[] chars, int offset, uint value)
        {
            int num = 0;
            do
            {
                num++;
                chars[--offset] = digits[((int) value) & 15];
                value /= 0x10;
            }
            while (value != 0);
            return num;
        }

        [SecurityCritical]
        private unsafe void UnsafeWriteEscapedText(char* chars, int count)
        {
            bool[] flagArray = this.inAttribute ? this.isEscapedAttributeChar : this.isEscapedElementChar;
            int length = flagArray.Length;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                char index = chars[i];
                if (((index < length) && flagArray[index]) || (index >= 0xfffe))
                {
                    base.UnsafeWriteUTF8Chars(chars + num2, i - num2);
                    this.WriteCharEntity(index);
                    num2 = i + 1;
                }
            }
            base.UnsafeWriteUTF8Chars(chars + num2, count - num2);
        }

        public void WriteAmpersandCharEntity()
        {
            int num;
            byte[] buffer = base.GetBuffer(5, out num);
            buffer[num] = 0x26;
            buffer[num + 1] = 0x61;
            buffer[num + 2] = 0x6d;
            buffer[num + 3] = 0x70;
            buffer[num + 4] = 0x3b;
            base.Advance(5);
        }

        public void WriteApostropheCharEntity()
        {
            int num;
            byte[] buffer = base.GetBuffer(6, out num);
            buffer[num] = 0x26;
            buffer[num + 1] = 0x61;
            buffer[num + 2] = 0x70;
            buffer[num + 3] = 0x6f;
            buffer[num + 4] = 0x73;
            buffer[num + 5] = 0x3b;
            base.Advance(6);
        }

        private void WriteBase64Text(byte[] buffer, int offset, int count)
        {
            Base64Encoding encoding = XmlConverter.Base64Encoding;
            while (count >= 3)
            {
                int num3;
                int byteCount = Math.Min(0x180, count - (count % 3));
                int num2 = (byteCount / 3) * 4;
                byte[] chars = base.GetBuffer(num2, out num3);
                base.Advance(encoding.GetChars(buffer, offset, byteCount, chars, num3));
                offset += byteCount;
                count -= byteCount;
            }
            if (count > 0)
            {
                int num4;
                byte[] buffer3 = base.GetBuffer(4, out num4);
                base.Advance(encoding.GetChars(buffer, offset, count, buffer3, num4));
            }
        }

        public override void WriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count)
        {
            if (trailByteCount > 0)
            {
                this.WriteBase64Text(trailBytes, 0, trailByteCount);
            }
            this.WriteBase64Text(buffer, offset, count);
        }

        public override void WriteBoolText(bool value)
        {
            int num;
            byte[] buffer = base.GetBuffer(5, out num);
            base.Advance(XmlConverter.ToChars(value, buffer, num));
        }

        public override void WriteCData(string text)
        {
            int num;
            byte[] buffer = base.GetBuffer(9, out num);
            buffer[num] = 60;
            buffer[num + 1] = 0x21;
            buffer[num + 2] = 0x5b;
            buffer[num + 3] = 0x43;
            buffer[num + 4] = 0x44;
            buffer[num + 5] = 0x41;
            buffer[num + 6] = 0x54;
            buffer[num + 7] = 0x41;
            buffer[num + 8] = 0x5b;
            base.Advance(9);
            base.WriteUTF8Chars(text);
            buffer = base.GetBuffer(3, out num);
            buffer[num] = 0x5d;
            buffer[num + 1] = 0x5d;
            buffer[num + 2] = 0x3e;
            base.Advance(3);
        }

        public override void WriteCharEntity(int ch)
        {
            switch (ch)
            {
                case 0x26:
                    this.WriteAmpersandCharEntity();
                    return;

                case 0x27:
                    this.WriteApostropheCharEntity();
                    return;

                case 0x22:
                    this.WriteQuoteCharEntity();
                    return;

                case 60:
                    this.WriteLessThanCharEntity();
                    return;

                case 0x3e:
                    this.WriteGreaterThanCharEntity();
                    return;
            }
            this.WriteHexCharEntity(ch);
        }

        public override void WriteComment(string text)
        {
            this.WriteStartComment();
            base.WriteUTF8Chars(text);
            this.WriteEndComment();
        }

        public override void WriteDateTimeText(DateTime value)
        {
            int num;
            byte[] chars = base.GetBuffer(0x40, out num);
            base.Advance(XmlConverter.ToChars(value, chars, num));
        }

        public override void WriteDecimalText(decimal value)
        {
            int num;
            byte[] buffer = base.GetBuffer(40, out num);
            base.Advance(XmlConverter.ToChars(value, buffer, num));
        }

        public override void WriteDeclaration()
        {
            if (this.encoding == null)
            {
                base.WriteUTF8Chars(utf8Decl, 0, utf8Decl.Length);
            }
            else
            {
                base.WriteUTF8Chars(startDecl, 0, startDecl.Length);
                if (this.encoding.WebName == System.Text.Encoding.BigEndianUnicode.WebName)
                {
                    base.WriteUTF8Chars("utf-16BE");
                }
                else
                {
                    base.WriteUTF8Chars(this.encoding.WebName);
                }
                base.WriteUTF8Chars(endDecl, 0, endDecl.Length);
            }
        }

        public override void WriteDoubleText(double value)
        {
            int num;
            byte[] buffer = base.GetBuffer(0x20, out num);
            base.Advance(XmlConverter.ToChars(value, buffer, num));
        }

        public override void WriteEndAttribute()
        {
            base.WriteByte('"');
            this.inAttribute = false;
        }

        private void WriteEndComment()
        {
            int num;
            byte[] buffer = base.GetBuffer(3, out num);
            buffer[num] = 0x2d;
            buffer[num + 1] = 0x2d;
            buffer[num + 2] = 0x3e;
            base.Advance(3);
        }

        public override void WriteEndElement(string prefix, string localName)
        {
            base.WriteBytes('<', '/');
            if (prefix.Length != 0)
            {
                this.WritePrefix(prefix);
                base.WriteByte(':');
            }
            this.WriteLocalName(localName);
            base.WriteByte('>');
        }

        public override void WriteEndElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            base.WriteBytes('<', '/');
            if (prefixLength != 0)
            {
                this.WritePrefix(prefixBuffer, prefixOffset, prefixLength);
                base.WriteByte(':');
            }
            this.WriteLocalName(localNameBuffer, localNameOffset, localNameLength);
            base.WriteByte('>');
        }

        public override void WriteEndListText()
        {
        }

        public override void WriteEndStartElement(bool isEmpty)
        {
            if (!isEmpty)
            {
                base.WriteByte('>');
            }
            else
            {
                base.WriteBytes('/', '>');
            }
        }

        [SecuritySafeCritical]
        public override unsafe void WriteEscapedText(string s)
        {
            int length = s.Length;
            if (length > 0)
            {
                fixed (char* str = ((char*) s))
                {
                    char* chars = str;
                    this.UnsafeWriteEscapedText(chars, length);
                }
            }
        }

        public override void WriteEscapedText(XmlDictionaryString s)
        {
            this.WriteEscapedText(s.Value);
        }

        public override void WriteEscapedText(byte[] chars, int offset, int count)
        {
            bool[] flagArray = this.inAttribute ? this.isEscapedAttributeChar : this.isEscapedElementChar;
            int length = flagArray.Length;
            int num2 = 0;
            for (int i = 0; i < count; i++)
            {
                byte index = chars[offset + i];
                if ((index < length) && flagArray[index])
                {
                    base.WriteUTF8Chars(chars, offset + num2, i - num2);
                    this.WriteCharEntity(index);
                    num2 = i + 1;
                }
                else if ((index == 0xef) && (((offset + i) + 2) < count))
                {
                    byte num5 = chars[(offset + i) + 1];
                    byte num6 = chars[(offset + i) + 2];
                    if ((num5 == 0xbf) && ((num6 == 190) || (num6 == 0xbf)))
                    {
                        base.WriteUTF8Chars(chars, offset + num2, i - num2);
                        this.WriteCharEntity((num6 == 190) ? 0xfffe : 0xffff);
                        num2 = i + 3;
                    }
                }
            }
            base.WriteUTF8Chars(chars, offset + num2, count - num2);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteEscapedText(char[] s, int offset, int count)
        {
            if (count > 0)
            {
                fixed (char* chRef = &(s[offset]))
                {
                    this.UnsafeWriteEscapedText(chRef, count);
                }
            }
        }

        public override void WriteFloatText(float value)
        {
            int num;
            byte[] buffer = base.GetBuffer(0x10, out num);
            base.Advance(XmlConverter.ToChars(value, buffer, num));
        }

        public void WriteGreaterThanCharEntity()
        {
            int num;
            byte[] buffer = base.GetBuffer(4, out num);
            buffer[num] = 0x26;
            buffer[num + 1] = 0x67;
            buffer[num + 2] = 0x74;
            buffer[num + 3] = 0x3b;
            base.Advance(4);
        }

        public override void WriteGuidText(Guid value)
        {
            this.WriteText(value.ToString());
        }

        private void WriteHexCharEntity(int ch)
        {
            byte[] charEntityBuffer = this.GetCharEntityBuffer();
            int offset = 0x20;
            charEntityBuffer[--offset] = 0x3b;
            offset -= this.ToBase16(charEntityBuffer, offset, (uint) ch);
            charEntityBuffer[--offset] = 120;
            charEntityBuffer[--offset] = 0x23;
            charEntityBuffer[--offset] = 0x26;
            base.WriteUTF8Chars(charEntityBuffer, offset, 0x20 - offset);
        }

        public override void WriteInt32Text(int value)
        {
            int num;
            byte[] chars = base.GetBuffer(0x10, out num);
            base.Advance(XmlConverter.ToChars(value, chars, num));
        }

        public override void WriteInt64Text(long value)
        {
            int num;
            byte[] chars = base.GetBuffer(0x20, out num);
            base.Advance(XmlConverter.ToChars(value, chars, num));
        }

        public void WriteLessThanCharEntity()
        {
            int num;
            byte[] buffer = base.GetBuffer(4, out num);
            buffer[num] = 0x26;
            buffer[num + 1] = 0x6c;
            buffer[num + 2] = 0x74;
            buffer[num + 3] = 0x3b;
            base.Advance(4);
        }

        public override void WriteListSeparator()
        {
            base.WriteByte(' ');
        }

        private void WriteLocalName(string localName)
        {
            base.WriteUTF8Chars(localName);
        }

        private void WriteLocalName(byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            base.WriteUTF8Chars(localNameBuffer, localNameOffset, localNameLength);
        }

        private void WritePrefix(string prefix)
        {
            if (prefix.Length == 1)
            {
                base.WriteUTF8Char(prefix[0]);
            }
            else
            {
                base.WriteUTF8Chars(prefix);
            }
        }

        private void WritePrefix(byte[] prefixBuffer, int prefixOffset, int prefixLength)
        {
            if (prefixLength == 1)
            {
                base.WriteUTF8Char(prefixBuffer[prefixOffset]);
            }
            else
            {
                base.WriteUTF8Chars(prefixBuffer, prefixOffset, prefixLength);
            }
        }

        public override void WriteQualifiedName(string prefix, XmlDictionaryString localName)
        {
            if (prefix.Length != 0)
            {
                this.WritePrefix(prefix);
                base.WriteByte(':');
            }
            this.WriteText(localName);
        }

        public void WriteQuoteCharEntity()
        {
            int num;
            byte[] buffer = base.GetBuffer(6, out num);
            buffer[num] = 0x26;
            buffer[num + 1] = 0x71;
            buffer[num + 2] = 0x75;
            buffer[num + 3] = 0x6f;
            buffer[num + 4] = 0x74;
            buffer[num + 5] = 0x3b;
            base.Advance(6);
        }

        public override void WriteStartAttribute(string prefix, string localName)
        {
            base.WriteByte(' ');
            if (prefix.Length != 0)
            {
                this.WritePrefix(prefix);
                base.WriteByte(':');
            }
            this.WriteLocalName(localName);
            base.WriteBytes('=', '"');
            this.inAttribute = true;
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName)
        {
            this.WriteStartAttribute(prefix, localName.Value);
        }

        public override void WriteStartAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            base.WriteByte(' ');
            if (prefixLength != 0)
            {
                this.WritePrefix(prefixBuffer, prefixOffset, prefixLength);
                base.WriteByte(':');
            }
            this.WriteLocalName(localNameBuffer, localNameOffset, localNameLength);
            base.WriteBytes('=', '"');
            this.inAttribute = true;
        }

        private void WriteStartComment()
        {
            int num;
            byte[] buffer = base.GetBuffer(4, out num);
            buffer[num] = 60;
            buffer[num + 1] = 0x21;
            buffer[num + 2] = 0x2d;
            buffer[num + 3] = 0x2d;
            base.Advance(4);
        }

        public override void WriteStartElement(string prefix, string localName)
        {
            base.WriteByte('<');
            if (prefix.Length != 0)
            {
                this.WritePrefix(prefix);
                base.WriteByte(':');
            }
            this.WriteLocalName(localName);
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName)
        {
            this.WriteStartElement(prefix, localName.Value);
        }

        public override void WriteStartElement(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] localNameBuffer, int localNameOffset, int localNameLength)
        {
            base.WriteByte('<');
            if (prefixLength != 0)
            {
                this.WritePrefix(prefixBuffer, prefixOffset, prefixLength);
                base.WriteByte(':');
            }
            this.WriteLocalName(localNameBuffer, localNameOffset, localNameLength);
        }

        public override void WriteStartListText()
        {
        }

        private void WriteStartXmlnsAttribute()
        {
            int num;
            byte[] buffer = base.GetBuffer(6, out num);
            buffer[num] = 0x20;
            buffer[num + 1] = 120;
            buffer[num + 2] = 0x6d;
            buffer[num + 3] = 0x6c;
            buffer[num + 4] = 110;
            buffer[num + 5] = 0x73;
            base.Advance(6);
            this.inAttribute = true;
        }

        public void WriteText(int ch)
        {
            base.WriteUTF8Char(ch);
        }

        public override void WriteText(string value)
        {
            base.WriteUTF8Chars(value);
        }

        public override void WriteText(XmlDictionaryString value)
        {
            base.WriteUTF8Chars(value.Value);
        }

        public override void WriteText(byte[] chars, int offset, int count)
        {
            base.WriteUTF8Chars(chars, offset, count);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteText(char[] chars, int offset, int count)
        {
            if (count > 0)
            {
                fixed (char* chRef = &(chars[offset]))
                {
                    base.UnsafeWriteUTF8Chars(chRef, count);
                }
            }
        }

        public override void WriteTimeSpanText(TimeSpan value)
        {
            this.WriteText(XmlConvert.ToString(value));
        }

        public override void WriteUInt64Text(ulong value)
        {
            int num;
            byte[] buffer = base.GetBuffer(0x20, out num);
            base.Advance(XmlConverter.ToChars(value, buffer, num));
        }

        public override void WriteUniqueIdText(UniqueId value)
        {
            if (value.IsGuid)
            {
                int charArrayLength = value.CharArrayLength;
                char[] charBuffer = this.GetCharBuffer(charArrayLength);
                value.ToCharArray(charBuffer, 0);
                this.WriteText(charBuffer, 0, charArrayLength);
            }
            else
            {
                this.WriteEscapedText(value.ToString());
            }
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            this.WriteStartXmlnsAttribute();
            if (prefix.Length != 0)
            {
                base.WriteByte(':');
                this.WritePrefix(prefix);
            }
            base.WriteBytes('=', '"');
            this.WriteEscapedText(ns);
            this.WriteEndAttribute();
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            this.WriteXmlnsAttribute(prefix, ns.Value);
        }

        public override void WriteXmlnsAttribute(byte[] prefixBuffer, int prefixOffset, int prefixLength, byte[] nsBuffer, int nsOffset, int nsLength)
        {
            this.WriteStartXmlnsAttribute();
            if (prefixLength != 0)
            {
                base.WriteByte(':');
                this.WritePrefix(prefixBuffer, prefixOffset, prefixLength);
            }
            base.WriteBytes('=', '"');
            this.WriteEscapedText(nsBuffer, nsOffset, nsLength);
            this.WriteEndAttribute();
        }

        public System.Text.Encoding Encoding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.encoding;
            }
        }
    }
}

