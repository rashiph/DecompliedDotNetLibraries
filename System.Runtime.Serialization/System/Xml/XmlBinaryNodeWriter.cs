namespace System.Xml
{
    using System;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    internal class XmlBinaryNodeWriter : XmlStreamNodeWriter
    {
        private AttributeValue attributeValue;
        private IXmlDictionary dictionary;
        private bool inAttribute;
        private bool inList;
        private const int maxBytesPerChar = 3;
        private XmlBinaryWriterSession session;
        private int textNodeOffset;
        private bool wroteAttributeValue;

        public override void Close()
        {
            base.Close();
            this.attributeValue.Clear();
        }

        protected override void FlushBuffer()
        {
            base.FlushBuffer();
            this.textNodeOffset = -1;
        }

        private byte[] GetTextNodeBuffer(int size, out int offset)
        {
            if (this.inAttribute)
            {
                this.WroteAttributeValue();
            }
            byte[] buffer = base.GetBuffer(size, out offset);
            this.textNodeOffset = offset;
            return buffer;
        }

        public void SetOutput(Stream stream, IXmlDictionary dictionary, XmlBinaryWriterSession session, bool ownsStream)
        {
            this.dictionary = dictionary;
            this.session = session;
            this.inAttribute = false;
            this.inList = false;
            this.attributeValue.Clear();
            this.textNodeOffset = -1;
            base.SetOutput(stream, ownsStream, null);
        }

        private bool TryGetKey(XmlDictionaryString s, out int key)
        {
            XmlDictionaryString str;
            int num;
            key = -1;
            if (s.Dictionary == this.dictionary)
            {
                key = s.Key * 2;
                return true;
            }
            if ((this.dictionary != null) && this.dictionary.TryLookup(s, out str))
            {
                key = str.Key * 2;
                return true;
            }
            if (this.session == null)
            {
                return false;
            }
            if (!this.session.TryLookup(s, out num) && !this.session.TryAdd(s, out num))
            {
                return false;
            }
            key = (num * 2) + 1;
            return true;
        }

        [SecurityCritical]
        private unsafe void UnsafeWriteArray(byte* array, int byteCount)
        {
            base.UnsafeWriteBytes(array, byteCount);
        }

        [SecurityCritical]
        public unsafe void UnsafeWriteArray(XmlBinaryNodeType nodeType, int count, byte* array, byte* arrayMax)
        {
            this.WriteArrayInfo(nodeType, count);
            this.UnsafeWriteArray(array, (int) ((long) ((arrayMax - array) / 1)));
        }

        [SecurityCritical]
        private unsafe void UnsafeWriteName(char* chars, int charCount)
        {
            if (charCount < 0x2a)
            {
                int num;
                byte[] buffer = base.GetBuffer(1 + (charCount * 3), out num);
                int num2 = base.UnsafeGetUTF8Chars(chars, charCount, buffer, num + 1);
                buffer[num] = (byte) num2;
                base.Advance(1 + num2);
            }
            else
            {
                int i = base.UnsafeGetUTF8Length(chars, charCount);
                this.WriteMultiByteInt32(i);
                base.UnsafeWriteUTF8Chars(chars, charCount);
            }
        }

        [SecurityCritical]
        private unsafe void UnsafeWriteText(char* chars, int charCount)
        {
            if (charCount == 1)
            {
                switch (chars[0])
                {
                    case '0':
                        this.WriteTextNode(XmlBinaryNodeType.MinText);
                        return;

                    case '1':
                        this.WriteTextNode(XmlBinaryNodeType.OneText);
                        return;
                }
            }
            if (charCount <= 0x55)
            {
                int num;
                byte[] buffer = base.GetBuffer(2 + (charCount * 3), out num);
                int num2 = base.UnsafeGetUTF8Chars(chars, charCount, buffer, num + 2);
                if ((num2 / 2) <= charCount)
                {
                    buffer[num] = 0x98;
                }
                else
                {
                    buffer[num] = 0xb6;
                    num2 = base.UnsafeGetUnicodeChars(chars, charCount, buffer, num + 2);
                }
                this.textNodeOffset = num;
                buffer[num + 1] = (byte) num2;
                base.Advance(2 + num2);
            }
            else
            {
                int length = base.UnsafeGetUTF8Length(chars, charCount);
                if ((length / 2) > charCount)
                {
                    this.WriteTextNodeWithLength(XmlBinaryNodeType.UnicodeChars8Text, charCount * 2);
                    base.UnsafeWriteUnicodeChars(chars, charCount);
                }
                else
                {
                    this.WriteTextNodeWithLength(XmlBinaryNodeType.Chars8Text, length);
                    base.UnsafeWriteUTF8Chars(chars, charCount);
                }
            }
        }

        private void WriteArrayInfo(XmlBinaryNodeType nodeType, int count)
        {
            this.WriteNode(nodeType);
            this.WriteMultiByteInt32(count);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void WriteArrayNode()
        {
            this.WriteNode(XmlBinaryNodeType.Array);
        }

        public override void WriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] base64Buffer, int base64Offset, int base64Count)
        {
            if (this.inAttribute)
            {
                this.attributeValue.WriteBase64Text(trailBytes, trailByteCount, base64Buffer, base64Offset, base64Count);
            }
            else
            {
                int length = trailByteCount + base64Count;
                if (length > 0)
                {
                    this.WriteTextNodeWithLength(XmlBinaryNodeType.Bytes8Text, length);
                    if (trailByteCount > 0)
                    {
                        int num2;
                        byte[] buffer = base.GetBuffer(trailByteCount, out num2);
                        for (int i = 0; i < trailByteCount; i++)
                        {
                            buffer[num2 + i] = trailBytes[i];
                        }
                        base.Advance(trailByteCount);
                    }
                    if (base64Count > 0)
                    {
                        base.WriteBytes(base64Buffer, base64Offset, base64Count);
                    }
                }
                else
                {
                    this.WriteEmptyText();
                }
            }
        }

        public override void WriteBoolText(bool value)
        {
            if (value)
            {
                this.WriteTextNode(XmlBinaryNodeType.TrueText);
            }
            else
            {
                this.WriteTextNode(XmlBinaryNodeType.FalseText);
            }
        }

        public override void WriteCData(string value)
        {
            this.WriteText(value);
        }

        public override void WriteCharEntity(int ch)
        {
            if (ch > 0xffff)
            {
                SurrogateChar ch2 = new SurrogateChar(ch);
                char[] chars = new char[] { ch2.HighChar, ch2.LowChar };
                this.WriteText(chars, 0, 2);
            }
            else
            {
                char[] chArray2 = new char[] { (char) ch };
                this.WriteText(chArray2, 0, 1);
            }
        }

        public override void WriteComment(string value)
        {
            this.WriteNode(XmlBinaryNodeType.Comment);
            this.WriteName(value);
        }

        public void WriteDateTimeArray(DateTime[] array, int offset, int count)
        {
            this.WriteArrayInfo(XmlBinaryNodeType.DateTimeTextWithEndElement, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteInt64(array[offset + i].ToBinary());
            }
        }

        public override void WriteDateTimeText(DateTime dt)
        {
            this.WriteTextNodeWithInt64(XmlBinaryNodeType.DateTimeText, dt.ToBinary());
        }

        [SecuritySafeCritical]
        public override unsafe void WriteDecimalText(decimal d)
        {
            int num;
            byte[] textNodeBuffer = this.GetTextNodeBuffer(0x11, out num);
            byte* numPtr = (byte*) &d;
            textNodeBuffer[num++] = 0x94;
            for (int i = 0; i < 0x10; i++)
            {
                textNodeBuffer[num++] = numPtr[i];
            }
            base.Advance(0x11);
        }

        public override void WriteDeclaration()
        {
        }

        private void WriteDictionaryString(XmlDictionaryString s, int key)
        {
            this.WriteMultiByteInt32(key);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteDoubleText(double d)
        {
            float num;
            if (((d >= -3.4028234663852886E+38) && (d <= 3.4028234663852886E+38)) && ((num = (float) d) == d))
            {
                this.WriteFloatText(num);
            }
            else
            {
                int num2;
                byte[] textNodeBuffer = this.GetTextNodeBuffer(9, out num2);
                byte* numPtr = (byte*) &d;
                textNodeBuffer[num2] = 0x92;
                textNodeBuffer[num2 + 1] = numPtr[0];
                textNodeBuffer[num2 + 2] = numPtr[1];
                textNodeBuffer[num2 + 3] = numPtr[2];
                textNodeBuffer[num2 + 4] = numPtr[3];
                textNodeBuffer[num2 + 5] = numPtr[4];
                textNodeBuffer[num2 + 6] = numPtr[5];
                textNodeBuffer[num2 + 7] = numPtr[6];
                textNodeBuffer[num2 + 8] = numPtr[7];
                base.Advance(9);
            }
        }

        private void WriteEmptyText()
        {
            this.WriteTextNode(XmlBinaryNodeType.EmptyText);
        }

        public override void WriteEndAttribute()
        {
            this.inAttribute = false;
            if (!this.wroteAttributeValue)
            {
                this.attributeValue.WriteTo(this);
            }
            this.textNodeOffset = -1;
        }

        private void WriteEndElement()
        {
            if (this.textNodeOffset != -1)
            {
                byte[] streamBuffer = base.StreamBuffer;
                XmlBinaryNodeType type = (XmlBinaryNodeType) streamBuffer[this.textNodeOffset];
                streamBuffer[this.textNodeOffset] = (byte) (type + 1);
                this.textNodeOffset = -1;
            }
            else
            {
                this.WriteNode(XmlBinaryNodeType.EndElement);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public override void WriteEndElement(string prefix, string localName)
        {
            this.WriteEndElement();
        }

        public override void WriteEndListText()
        {
            this.inList = false;
            this.wroteAttributeValue = true;
            this.WriteNode(XmlBinaryNodeType.EndListText);
        }

        public override void WriteEndStartElement(bool isEmpty)
        {
            if (isEmpty)
            {
                this.WriteEndElement();
            }
        }

        public override void WriteEscapedText(string value)
        {
            this.WriteText(value);
        }

        public override void WriteEscapedText(XmlDictionaryString value)
        {
            this.WriteText(value);
        }

        public override void WriteEscapedText(byte[] chars, int offset, int count)
        {
            this.WriteText(chars, offset, count);
        }

        public override void WriteEscapedText(char[] chars, int offset, int count)
        {
            this.WriteText(chars, offset, count);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteFloatText(float f)
        {
            long num;
            if (((f >= -9.223372E+18f) && (f <= 9.223372E+18f)) && ((num = (long) f) == f))
            {
                this.WriteInt64Text(num);
            }
            else
            {
                int num2;
                byte[] textNodeBuffer = this.GetTextNodeBuffer(5, out num2);
                byte* numPtr = (byte*) &f;
                textNodeBuffer[num2] = 0x90;
                textNodeBuffer[num2 + 1] = numPtr[0];
                textNodeBuffer[num2 + 2] = numPtr[1];
                textNodeBuffer[num2 + 3] = numPtr[2];
                textNodeBuffer[num2 + 4] = numPtr[3];
                base.Advance(5);
            }
        }

        public void WriteGuidArray(Guid[] array, int offset, int count)
        {
            this.WriteArrayInfo(XmlBinaryNodeType.GuidTextWithEndElement, count);
            for (int i = 0; i < count; i++)
            {
                byte[] byteBuffer = array[offset + i].ToByteArray();
                base.WriteBytes(byteBuffer, 0, 0x10);
            }
        }

        public override void WriteGuidText(Guid guid)
        {
            int num;
            byte[] textNodeBuffer = this.GetTextNodeBuffer(0x11, out num);
            textNodeBuffer[num] = 0xb0;
            Buffer.BlockCopy(guid.ToByteArray(), 0, textNodeBuffer, num + 1, 0x10);
            base.Advance(0x11);
        }

        public override void WriteInt32Text(int value)
        {
            if ((value >= -128) && (value < 0x80))
            {
                if (value == 0)
                {
                    this.WriteTextNode(XmlBinaryNodeType.MinText);
                }
                else if (value == 1)
                {
                    this.WriteTextNode(XmlBinaryNodeType.OneText);
                }
                else
                {
                    int num;
                    byte[] textNodeBuffer = this.GetTextNodeBuffer(2, out num);
                    textNodeBuffer[num] = 0x88;
                    textNodeBuffer[num + 1] = (byte) value;
                    base.Advance(2);
                }
            }
            else if ((value >= -32768) && (value < 0x8000))
            {
                int num2;
                byte[] buffer2 = this.GetTextNodeBuffer(3, out num2);
                buffer2[num2] = 0x8a;
                buffer2[num2 + 1] = (byte) value;
                value = value >> 8;
                buffer2[num2 + 2] = (byte) value;
                base.Advance(3);
            }
            else
            {
                int num3;
                byte[] buffer3 = this.GetTextNodeBuffer(5, out num3);
                buffer3[num3] = 140;
                buffer3[num3 + 1] = (byte) value;
                value = value >> 8;
                buffer3[num3 + 2] = (byte) value;
                value = value >> 8;
                buffer3[num3 + 3] = (byte) value;
                value = value >> 8;
                buffer3[num3 + 4] = (byte) value;
                base.Advance(5);
            }
        }

        private void WriteInt64(long value)
        {
            int num;
            byte[] buffer = base.GetBuffer(8, out num);
            buffer[num] = (byte) value;
            value = value >> 8;
            buffer[num + 1] = (byte) value;
            value = value >> 8;
            buffer[num + 2] = (byte) value;
            value = value >> 8;
            buffer[num + 3] = (byte) value;
            value = value >> 8;
            buffer[num + 4] = (byte) value;
            value = value >> 8;
            buffer[num + 5] = (byte) value;
            value = value >> 8;
            buffer[num + 6] = (byte) value;
            value = value >> 8;
            buffer[num + 7] = (byte) value;
            base.Advance(8);
        }

        public override void WriteInt64Text(long value)
        {
            if ((value >= -2147483648L) && (value <= 0x7fffffffL))
            {
                this.WriteInt32Text((int) value);
            }
            else
            {
                this.WriteTextNodeWithInt64(XmlBinaryNodeType.Int64Text, value);
            }
        }

        public override void WriteListSeparator()
        {
        }

        private void WriteMultiByteInt32(int i)
        {
            int num;
            byte[] buffer = base.GetBuffer(5, out num);
            int num2 = num;
            while ((i & 0xffffff80L) != 0L)
            {
                buffer[num++] = (byte) ((i & 0x7f) | 0x80);
                i = i >> 7;
            }
            buffer[num++] = (byte) i;
            base.Advance(num - num2);
        }

        [SecuritySafeCritical]
        private unsafe void WriteName(string s)
        {
            int length = s.Length;
            if (length == 0)
            {
                base.WriteByte((byte) 0);
            }
            else
            {
                fixed (char* str = ((char*) s))
                {
                    char* chars = str;
                    this.UnsafeWriteName(chars, length);
                }
            }
        }

        private void WriteNode(XmlBinaryNodeType nodeType)
        {
            base.WriteByte((byte) nodeType);
            this.textNodeOffset = -1;
        }

        private void WritePrefixNode(XmlBinaryNodeType nodeType, int ch)
        {
            this.WriteNode(nodeType + ch);
        }

        public override void WriteQualifiedName(string prefix, XmlDictionaryString localName)
        {
            if (prefix.Length == 0)
            {
                this.WriteText(localName);
            }
            else
            {
                int num;
                char ch = prefix[0];
                if (((prefix.Length == 1) && (ch >= 'a')) && ((ch <= 'z') && this.TryGetKey(localName, out num)))
                {
                    this.WriteTextNode(XmlBinaryNodeType.QNameDictionaryText);
                    base.WriteByte((byte) (ch - 'a'));
                    this.WriteDictionaryString(localName, num);
                }
                else
                {
                    this.WriteText(prefix);
                    this.WriteText(":");
                    this.WriteText(localName);
                }
            }
        }

        public override void WriteStartAttribute(string prefix, string localName)
        {
            if (prefix.Length == 0)
            {
                this.WriteNode(XmlBinaryNodeType.MinAttribute);
                this.WriteName(localName);
            }
            else
            {
                char ch = prefix[0];
                if (((prefix.Length == 1) && (ch >= 'a')) && (ch <= 'z'))
                {
                    this.WritePrefixNode(XmlBinaryNodeType.PrefixAttributeA, ch - 'a');
                    this.WriteName(localName);
                }
                else
                {
                    this.WriteNode(XmlBinaryNodeType.Attribute);
                    this.WriteName(prefix);
                    this.WriteName(localName);
                }
            }
            this.inAttribute = true;
            this.wroteAttributeValue = false;
        }

        public override void WriteStartAttribute(string prefix, XmlDictionaryString localName)
        {
            int num;
            if (!this.TryGetKey(localName, out num))
            {
                this.WriteStartAttribute(prefix, localName.Value);
            }
            else
            {
                if (prefix.Length == 0)
                {
                    this.WriteNode(XmlBinaryNodeType.ShortDictionaryAttribute);
                    this.WriteDictionaryString(localName, num);
                }
                else
                {
                    char ch = prefix[0];
                    if (((prefix.Length == 1) && (ch >= 'a')) && (ch <= 'z'))
                    {
                        this.WritePrefixNode(XmlBinaryNodeType.PrefixDictionaryAttributeA, ch - 'a');
                        this.WriteDictionaryString(localName, num);
                    }
                    else
                    {
                        this.WriteNode(XmlBinaryNodeType.DictionaryAttribute);
                        this.WriteName(prefix);
                        this.WriteDictionaryString(localName, num);
                    }
                }
                this.inAttribute = true;
                this.wroteAttributeValue = false;
            }
        }

        public override void WriteStartElement(string prefix, string localName)
        {
            if (prefix.Length == 0)
            {
                this.WriteNode(XmlBinaryNodeType.MinElement);
                this.WriteName(localName);
            }
            else
            {
                char ch = prefix[0];
                if (((prefix.Length == 1) && (ch >= 'a')) && (ch <= 'z'))
                {
                    this.WritePrefixNode(XmlBinaryNodeType.PrefixElementA, ch - 'a');
                    this.WriteName(localName);
                }
                else
                {
                    this.WriteNode(XmlBinaryNodeType.Element);
                    this.WriteName(prefix);
                    this.WriteName(localName);
                }
            }
        }

        public override void WriteStartElement(string prefix, XmlDictionaryString localName)
        {
            int num;
            if (!this.TryGetKey(localName, out num))
            {
                this.WriteStartElement(prefix, localName.Value);
            }
            else if (prefix.Length == 0)
            {
                this.WriteNode(XmlBinaryNodeType.ShortDictionaryElement);
                this.WriteDictionaryString(localName, num);
            }
            else
            {
                char ch = prefix[0];
                if (((prefix.Length == 1) && (ch >= 'a')) && (ch <= 'z'))
                {
                    this.WritePrefixNode(XmlBinaryNodeType.PrefixDictionaryElementA, ch - 'a');
                    this.WriteDictionaryString(localName, num);
                }
                else
                {
                    this.WriteNode(XmlBinaryNodeType.DictionaryElement);
                    this.WriteName(prefix);
                    this.WriteDictionaryString(localName, num);
                }
            }
        }

        public override void WriteStartListText()
        {
            this.inList = true;
            this.WriteNode(XmlBinaryNodeType.StartListText);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteText(string value)
        {
            if (this.inAttribute)
            {
                this.attributeValue.WriteText(value);
            }
            else if (value.Length > 0)
            {
                fixed (char* str = ((char*) value))
                {
                    char* chars = str;
                    this.UnsafeWriteText(chars, value.Length);
                }
            }
            else
            {
                this.WriteEmptyText();
            }
        }

        public override void WriteText(XmlDictionaryString value)
        {
            if (this.inAttribute)
            {
                this.attributeValue.WriteText(value);
            }
            else
            {
                int num;
                if (!this.TryGetKey(value, out num))
                {
                    this.WriteText(value.Value);
                }
                else
                {
                    this.WriteTextNode(XmlBinaryNodeType.DictionaryText);
                    this.WriteDictionaryString(value, num);
                }
            }
        }

        public override void WriteText(byte[] chars, int charOffset, int charCount)
        {
            this.WriteTextNodeWithLength(XmlBinaryNodeType.Chars8Text, charCount);
            base.WriteBytes(chars, charOffset, charCount);
        }

        [SecuritySafeCritical]
        public override unsafe void WriteText(char[] chars, int offset, int count)
        {
            if (this.inAttribute)
            {
                this.attributeValue.WriteText(new string(chars, offset, count));
            }
            else if (count > 0)
            {
                fixed (char* chRef = &(chars[offset]))
                {
                    this.UnsafeWriteText(chRef, count);
                }
            }
            else
            {
                this.WriteEmptyText();
            }
        }

        private void WriteTextNode(XmlBinaryNodeType nodeType)
        {
            if (this.inAttribute)
            {
                this.WroteAttributeValue();
            }
            base.WriteByte((byte) nodeType);
            this.textNodeOffset = base.BufferOffset - 1;
        }

        private void WriteTextNodeWithInt64(XmlBinaryNodeType nodeType, long value)
        {
            int num;
            byte[] textNodeBuffer = this.GetTextNodeBuffer(9, out num);
            textNodeBuffer[num] = (byte) nodeType;
            textNodeBuffer[num + 1] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 2] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 3] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 4] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 5] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 6] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 7] = (byte) value;
            value = value >> 8;
            textNodeBuffer[num + 8] = (byte) value;
            base.Advance(9);
        }

        private void WriteTextNodeWithLength(XmlBinaryNodeType nodeType, int length)
        {
            int num;
            byte[] textNodeBuffer = this.GetTextNodeBuffer(5, out num);
            if (length < 0x100)
            {
                textNodeBuffer[num] = (byte) nodeType;
                textNodeBuffer[num + 1] = (byte) length;
                base.Advance(2);
            }
            else if (length < 0x10000)
            {
                textNodeBuffer[num] = (byte) (nodeType + 2);
                textNodeBuffer[num + 1] = (byte) length;
                length = length >> 8;
                textNodeBuffer[num + 2] = (byte) length;
                base.Advance(3);
            }
            else
            {
                textNodeBuffer[num] = (byte) (nodeType + 4);
                textNodeBuffer[num + 1] = (byte) length;
                length = length >> 8;
                textNodeBuffer[num + 2] = (byte) length;
                length = length >> 8;
                textNodeBuffer[num + 3] = (byte) length;
                length = length >> 8;
                textNodeBuffer[num + 4] = (byte) length;
                base.Advance(5);
            }
        }

        public void WriteTimeSpanArray(TimeSpan[] array, int offset, int count)
        {
            this.WriteArrayInfo(XmlBinaryNodeType.TimeSpanTextWithEndElement, count);
            for (int i = 0; i < count; i++)
            {
                this.WriteInt64(array[offset + i].Ticks);
            }
        }

        public override void WriteTimeSpanText(TimeSpan value)
        {
            this.WriteTextNodeWithInt64(XmlBinaryNodeType.TimeSpanText, value.Ticks);
        }

        public override void WriteUInt64Text(ulong value)
        {
            if (value <= 0x7fffffffffffffffL)
            {
                this.WriteInt64Text((long) value);
            }
            else
            {
                this.WriteTextNodeWithInt64(XmlBinaryNodeType.UInt64Text, (long) value);
            }
        }

        public override void WriteUniqueIdText(UniqueId value)
        {
            if (value.IsGuid)
            {
                int num;
                byte[] textNodeBuffer = this.GetTextNodeBuffer(0x11, out num);
                textNodeBuffer[num] = 0xac;
                value.TryGetGuid(textNodeBuffer, num + 1);
                base.Advance(0x11);
            }
            else
            {
                this.WriteText(value.ToString());
            }
        }

        public override void WriteXmlnsAttribute(string prefix, string ns)
        {
            if (prefix.Length == 0)
            {
                this.WriteNode(XmlBinaryNodeType.ShortXmlnsAttribute);
                this.WriteName(ns);
            }
            else
            {
                this.WriteNode(XmlBinaryNodeType.XmlnsAttribute);
                this.WriteName(prefix);
                this.WriteName(ns);
            }
        }

        public override void WriteXmlnsAttribute(string prefix, XmlDictionaryString ns)
        {
            int num;
            if (!this.TryGetKey(ns, out num))
            {
                this.WriteXmlnsAttribute(prefix, ns.Value);
            }
            else if (prefix.Length == 0)
            {
                this.WriteNode(XmlBinaryNodeType.ShortDictionaryXmlnsAttribute);
                this.WriteDictionaryString(ns, num);
            }
            else
            {
                this.WriteNode(XmlBinaryNodeType.DictionaryXmlnsAttribute);
                this.WriteName(prefix);
                this.WriteDictionaryString(ns, num);
            }
        }

        private void WroteAttributeValue()
        {
            if (this.wroteAttributeValue && !this.inList)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.Runtime.Serialization.SR.GetString("XmlOnlySingleValue")));
            }
            this.wroteAttributeValue = true;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AttributeValue
        {
            private string captureText;
            private XmlDictionaryString captureXText;
            private MemoryStream captureStream;
            public void Clear()
            {
                this.captureText = null;
                this.captureXText = null;
                this.captureStream = null;
            }

            public void WriteText(string s)
            {
                if (this.captureStream != null)
                {
                    this.captureText = XmlConverter.Base64Encoding.GetString(this.captureStream.GetBuffer(), 0, (int) this.captureStream.Length);
                    this.captureStream = null;
                }
                if (this.captureXText != null)
                {
                    this.captureText = this.captureXText.Value;
                    this.captureXText = null;
                }
                if ((this.captureText == null) || (this.captureText.Length == 0))
                {
                    this.captureText = s;
                }
                else
                {
                    this.captureText = this.captureText + s;
                }
            }

            public void WriteText(XmlDictionaryString s)
            {
                if ((this.captureText != null) || (this.captureStream != null))
                {
                    this.WriteText(s.Value);
                }
                else
                {
                    this.captureXText = s;
                }
            }

            public void WriteBase64Text(byte[] trailBytes, int trailByteCount, byte[] buffer, int offset, int count)
            {
                if ((this.captureText != null) || (this.captureXText != null))
                {
                    if (trailByteCount > 0)
                    {
                        this.WriteText(XmlConverter.Base64Encoding.GetString(trailBytes, 0, trailByteCount));
                    }
                    this.WriteText(XmlConverter.Base64Encoding.GetString(buffer, offset, count));
                }
                else
                {
                    if (this.captureStream == null)
                    {
                        this.captureStream = new MemoryStream();
                    }
                    if (trailByteCount > 0)
                    {
                        this.captureStream.Write(trailBytes, 0, trailByteCount);
                    }
                    this.captureStream.Write(buffer, offset, count);
                }
            }

            public void WriteTo(XmlBinaryNodeWriter writer)
            {
                if (this.captureText != null)
                {
                    writer.WriteText(this.captureText);
                    this.captureText = null;
                }
                else if (this.captureXText != null)
                {
                    writer.WriteText(this.captureXText);
                    this.captureXText = null;
                }
                else if (this.captureStream != null)
                {
                    writer.WriteBase64Text(null, 0, this.captureStream.GetBuffer(), 0, (int) this.captureStream.Length);
                    this.captureStream = null;
                }
                else
                {
                    writer.WriteEmptyText();
                }
            }
        }
    }
}

