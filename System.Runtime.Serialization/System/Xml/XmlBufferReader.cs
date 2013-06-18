namespace System.Xml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Text;

    internal class XmlBufferReader
    {
        private byte[] buffer;
        private char[] chars;
        private IXmlDictionary dictionary;
        private static XmlBufferReader empty = new XmlBufferReader(emptyByteArray);
        private static byte[] emptyByteArray = new byte[0];
        private byte[] guid;
        private ValueHandle listValue;
        private const int maxBytesPerChar = 3;
        private int offset;
        private int offsetMax;
        private int offsetMin;
        private XmlDictionaryReader reader;
        private XmlBinaryReaderSession session;
        private Stream stream;
        private byte[] streamBuffer;
        private int windowOffset;
        private int windowOffsetMax;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlBufferReader(XmlDictionaryReader reader)
        {
            this.reader = reader;
        }

        public XmlBufferReader(byte[] buffer)
        {
            this.reader = null;
            this.buffer = buffer;
        }

        public void Advance(int count)
        {
            this.offset += count;
        }

        public void Close()
        {
            if ((this.streamBuffer != null) && (this.streamBuffer.Length > 0x1000))
            {
                this.streamBuffer = null;
            }
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null;
            }
            this.buffer = emptyByteArray;
            this.offset = 0;
            this.offsetMax = 0;
            this.windowOffset = 0;
            this.windowOffsetMax = 0;
            this.dictionary = null;
            this.session = null;
        }

        public int Compare(int offset1, int length1, int offset2, int length2)
        {
            byte[] buffer = this.buffer;
            int num = Math.Min(length1, length2);
            for (int i = 0; i < num; i++)
            {
                int num3 = buffer[offset1 + i] - buffer[offset2 + i];
                if (num3 != 0)
                {
                    return num3;
                }
            }
            return (length1 - length2);
        }

        private void EnsureByte()
        {
            if (!this.TryEnsureByte())
            {
                XmlExceptionHelper.ThrowUnexpectedEndOfFile(this.reader);
            }
        }

        private void EnsureBytes(int count)
        {
            if (!this.TryEnsureBytes(count))
            {
                XmlExceptionHelper.ThrowUnexpectedEndOfFile(this.reader);
            }
        }

        public bool Equals2(int key1, XmlDictionaryString xmlString2)
        {
            if (((key1 & 1) == 0) && (xmlString2.Dictionary == this.dictionary))
            {
                return (xmlString2.Key == (key1 >> 1));
            }
            return (this.GetDictionaryString(key1).Value == xmlString2.Value);
        }

        public bool Equals2(int key1, int key2, XmlBufferReader bufferReader2)
        {
            return ((key1 == key2) || (this.GetDictionaryString(key1).Value == bufferReader2.GetDictionaryString(key2).Value));
        }

        public bool Equals2(int offset1, int length1, byte[] buffer2)
        {
            int length = buffer2.Length;
            if (length1 != length)
            {
                return false;
            }
            byte[] buffer = this.buffer;
            for (int i = 0; i < length1; i++)
            {
                if (buffer[offset1 + i] != buffer2[i])
                {
                    return false;
                }
            }
            return true;
        }

        [SecuritySafeCritical]
        public unsafe bool Equals2(int offset1, int length1, string s2)
        {
            int num = length1;
            int length = s2.Length;
            if ((num < length) || (num > (length * 3)))
            {
                return false;
            }
            byte[] buffer = this.buffer;
            if (length1 < 8)
            {
                int num3 = Math.Min(num, length);
                int num4 = offset1;
                for (int i = 0; i < num3; i++)
                {
                    byte num6 = buffer[num4 + i];
                    if (num6 >= 0x80)
                    {
                        return (XmlConverter.ToString(buffer, offset1, length1) == s2);
                    }
                    if (s2[i] != num6)
                    {
                        return false;
                    }
                }
                return (num == length);
            }
            int num7 = Math.Min(num, length);
            fixed (byte* numRef = &(buffer[offset1]))
            {
                byte* numPtr = numRef;
                byte* numPtr2 = numPtr + num7;
                fixed (char* str = ((char*) s2))
                {
                    char* chPtr2 = str;
                    int num8 = 0;
                    while ((numPtr < numPtr2) && (numPtr[0] < 0x80))
                    {
                        num8 = numPtr[0] - ((byte) chPtr2[0]);
                        if (num8 != 0)
                        {
                            break;
                        }
                        numPtr++;
                        chPtr2++;
                    }
                    if (num8 != 0)
                    {
                        return false;
                    }
                    if (numPtr == numPtr2)
                    {
                        return (num == length);
                    }
                }
            }
            return (XmlConverter.ToString(buffer, offset1, length1) == s2);
        }

        public bool Equals2(int offset1, int length1, int offset2, int length2)
        {
            if (length1 != length2)
            {
                return false;
            }
            if (offset1 != offset2)
            {
                byte[] buffer = this.buffer;
                for (int i = 0; i < length1; i++)
                {
                    if (buffer[offset1 + i] != buffer[offset2 + i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool Equals2(int offset1, int length1, XmlBufferReader bufferReader2, int offset2, int length2)
        {
            if (length1 != length2)
            {
                return false;
            }
            byte[] buffer = this.buffer;
            byte[] buffer2 = bufferReader2.buffer;
            for (int i = 0; i < length1; i++)
            {
                if (buffer[offset1 + i] != buffer2[offset2 + i])
                {
                    return false;
                }
            }
            return true;
        }

        private int GetAmpersandCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (((length != 5) || (buffer[offset + 1] != 0x61)) || ((buffer[offset + 2] != 0x6d) || (buffer[offset + 3] != 0x70)))
            {
                XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            }
            return 0x26;
        }

        private int GetApostropheCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (((length != 6) || (buffer[offset + 1] != 0x61)) || (((buffer[offset + 2] != 0x70) || (buffer[offset + 3] != 0x6f)) || (buffer[offset + 4] != 0x73)))
            {
                XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            }
            return 0x27;
        }

        public void GetBase64(int srcOffset, byte[] buffer, int dstOffset, int count)
        {
            System.Buffer.BlockCopy(this.buffer, srcOffset, buffer, dstOffset, count);
        }

        public byte[] GetBuffer(int count, out int offset)
        {
            offset = this.offset;
            if (offset <= (this.offsetMax - count))
            {
                return this.buffer;
            }
            return this.GetBufferHard(count, out offset);
        }

        public byte[] GetBuffer(out int offset, out int offsetMax)
        {
            offset = this.offset;
            offsetMax = this.offsetMax;
            return this.buffer;
        }

        public byte[] GetBuffer(int count, out int offset, out int offsetMax)
        {
            offset = this.offset;
            if (offset <= (this.offsetMax - count))
            {
                offsetMax = this.offset + count;
            }
            else
            {
                this.TryEnsureBytes(Math.Min(count, this.windowOffsetMax - offset));
                offsetMax = this.offsetMax;
            }
            return this.buffer;
        }

        private byte[] GetBufferHard(int count, out int offset)
        {
            offset = this.offset;
            this.EnsureBytes(count);
            return this.buffer;
        }

        public byte GetByte()
        {
            int offset = this.offset;
            if (offset < this.offsetMax)
            {
                return this.buffer[offset];
            }
            return this.GetByteHard();
        }

        public byte GetByte(int offset)
        {
            return this.buffer[offset];
        }

        private byte GetByteHard()
        {
            this.EnsureByte();
            return this.buffer[this.offset];
        }

        private char[] GetCharBuffer(int count)
        {
            if (count > 0x400)
            {
                return new char[count];
            }
            if ((this.chars == null) || (this.chars.Length < count))
            {
                this.chars = new char[count];
            }
            return this.chars;
        }

        public int GetCharEntity(int offset, int length)
        {
            if (length < 3)
            {
                XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            }
            byte[] buffer = this.buffer;
            switch (buffer[offset + 1])
            {
                case 0x67:
                    return this.GetGreaterThanCharEntity(offset, length);

                case 0x6c:
                    return this.GetLessThanCharEntity(offset, length);

                case 0x71:
                    return this.GetQuoteCharEntity(offset, length);

                case 0x23:
                    if (buffer[offset + 2] == 120)
                    {
                        return this.GetHexCharEntity(offset, length);
                    }
                    return this.GetDecimalCharEntity(offset, length);

                case 0x61:
                    if (buffer[offset + 2] == 0x6d)
                    {
                        return this.GetAmpersandCharEntity(offset, length);
                    }
                    return this.GetApostropheCharEntity(offset, length);
            }
            XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            return 0;
        }

        private int GetChars(int offset, int length, char[] chars)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i++)
            {
                byte num2 = buffer[offset + i];
                if (num2 >= 0x80)
                {
                    return (i + XmlConverter.ToChars(buffer, offset + i, length - i, chars, i));
                }
                chars[i] = (char) num2;
            }
            return length;
        }

        private int GetChars(int offset, int length, char[] chars, int charOffset)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i++)
            {
                byte num2 = buffer[offset + i];
                if (num2 >= 0x80)
                {
                    return (i + XmlConverter.ToChars(buffer, offset + i, length - i, chars, charOffset + i));
                }
                chars[charOffset + i] = (char) num2;
            }
            return length;
        }

        [SecuritySafeCritical]
        public unsafe decimal GetDecimal(int offset)
        {
            byte[] buffer = this.buffer;
            byte num = buffer[offset];
            byte num2 = buffer[offset + 1];
            byte num3 = buffer[offset + 2];
            byte num4 = buffer[offset + 3];
            int num5 = (((((num4 << 8) + num3) << 8) + num2) << 8) + num;
            if (((num5 & 0x7f00ffff) == 0) && ((num5 & 0xff0000) <= 0x1c0000))
            {
                decimal num6;
                byte* numPtr = (byte*) &num6;
                for (int i = 0; i < 0x10; i++)
                {
                    numPtr[i] = buffer[offset + i];
                }
                return num6;
            }
            XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            return 0M;
        }

        private int GetDecimalCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            int num = 0;
            for (int i = 2; i < (length - 1); i++)
            {
                byte num3 = buffer[offset + i];
                if ((num3 < 0x30) || (num3 > 0x39))
                {
                    XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
                }
                num = (num * 10) + (num3 - 0x30);
                if (num > 0x10ffff)
                {
                    XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
                }
            }
            return num;
        }

        public XmlDictionaryString GetDictionaryString(int key)
        {
            IXmlDictionary session;
            XmlDictionaryString str;
            if ((key & 1) != 0)
            {
                session = this.session;
            }
            else
            {
                session = this.dictionary;
            }
            if (!session.TryLookup((int) (key >> 1), out str))
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }
            return str;
        }

        [SecuritySafeCritical]
        public unsafe double GetDouble(int offset)
        {
            double num;
            byte[] buffer = this.buffer;
            byte* numPtr = (byte*) &num;
            numPtr[0] = buffer[offset];
            numPtr[1] = buffer[offset + 1];
            numPtr[2] = buffer[offset + 2];
            numPtr[3] = buffer[offset + 3];
            numPtr[4] = buffer[offset + 4];
            numPtr[5] = buffer[offset + 5];
            numPtr[6] = buffer[offset + 6];
            numPtr[7] = buffer[offset + 7];
            return num;
        }

        public int GetEscapedChars(int offset, int length, char[] chars)
        {
            byte[] buffer = this.buffer;
            int charOffset = 0;
            int num2 = offset;
            int num3 = offset + length;
            while (true)
            {
                while ((offset < num3) && this.IsAttrChar(buffer[offset]))
                {
                    offset++;
                }
                charOffset += this.GetChars(num2, offset - num2, chars, charOffset);
                if (offset == num3)
                {
                    return charOffset;
                }
                num2 = offset;
                if (buffer[offset] == 0x26)
                {
                    while ((offset < num3) && (buffer[offset] != 0x3b))
                    {
                        offset++;
                    }
                    offset++;
                    int charEntity = this.GetCharEntity(num2, offset - num2);
                    num2 = offset;
                    if (charEntity > 0xffff)
                    {
                        SurrogateChar ch = new SurrogateChar(charEntity);
                        chars[charOffset++] = ch.HighChar;
                        chars[charOffset++] = ch.LowChar;
                    }
                    else
                    {
                        chars[charOffset++] = (char) charEntity;
                    }
                }
                else if ((buffer[offset] == 10) || (buffer[offset] == 9))
                {
                    chars[charOffset++] = ' ';
                    offset++;
                    num2 = offset;
                }
                else
                {
                    chars[charOffset++] = ' ';
                    offset++;
                    if ((offset < num3) && (buffer[offset] == 10))
                    {
                        offset++;
                    }
                    num2 = offset;
                }
            }
        }

        public string GetEscapedString(int offset, int length)
        {
            char[] charBuffer = this.GetCharBuffer(length);
            return new string(charBuffer, 0, this.GetEscapedChars(offset, length, charBuffer));
        }

        public string GetEscapedString(int offset, int length, XmlNameTable nameTable)
        {
            char[] charBuffer = this.GetCharBuffer(length);
            int num = this.GetEscapedChars(offset, length, charBuffer);
            return nameTable.Add(charBuffer, 0, num);
        }

        private int GetGreaterThanCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (((length != 4) || (buffer[offset + 1] != 0x67)) || (buffer[offset + 2] != 0x74))
            {
                XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            }
            return 0x3e;
        }

        public Guid GetGuid(int offset)
        {
            if (this.guid == null)
            {
                this.guid = new byte[0x10];
            }
            System.Buffer.BlockCopy(this.buffer, offset, this.guid, 0, this.guid.Length);
            return new Guid(this.guid);
        }

        private int GetHexCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            int num = 0;
            for (int i = 3; i < (length - 1); i++)
            {
                byte num3 = buffer[offset + i];
                int num4 = 0;
                if ((num3 >= 0x30) && (num3 <= 0x39))
                {
                    num4 = num3 - 0x30;
                }
                else if ((num3 >= 0x61) && (num3 <= 0x66))
                {
                    num4 = 10 + (num3 - 0x61);
                }
                else if ((num3 >= 0x41) && (num3 <= 70))
                {
                    num4 = 10 + (num3 - 0x41);
                }
                else
                {
                    XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
                }
                num = (num * 0x10) + num4;
                if (num > 0x10ffff)
                {
                    XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
                }
            }
            return num;
        }

        public int GetInt16(int offset)
        {
            byte[] buffer = this.buffer;
            return (short) (buffer[offset] + (buffer[offset + 1] << 8));
        }

        public int GetInt32(int offset)
        {
            byte[] buffer = this.buffer;
            byte num = buffer[offset];
            byte num2 = buffer[offset + 1];
            byte num3 = buffer[offset + 2];
            byte num4 = buffer[offset + 3];
            return ((((((num4 << 8) + num3) << 8) + num2) << 8) + num);
        }

        public long GetInt64(int offset)
        {
            byte[] buffer = this.buffer;
            byte num = buffer[offset];
            byte num2 = buffer[offset + 1];
            byte num3 = buffer[offset + 2];
            byte num4 = buffer[offset + 3];
            long num5 = (long) ((ulong) ((((((num4 << 8) + num3) << 8) + num2) << 8) + num));
            num = buffer[offset + 4];
            num2 = buffer[offset + 5];
            num3 = buffer[offset + 6];
            num4 = buffer[offset + 7];
            long num6 = (long) ((ulong) ((((((num4 << 8) + num3) << 8) + num2) << 8) + num));
            return ((num6 << 0x20) + num5);
        }

        public int GetInt8(int offset)
        {
            return (sbyte) this.GetByte(offset);
        }

        private int GetLessThanCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (((length != 4) || (buffer[offset + 1] != 0x6c)) || (buffer[offset + 2] != 0x74))
            {
                XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            }
            return 60;
        }

        public object[] GetList(int offset, int count)
        {
            object[] objArray2;
            int num = this.Offset;
            this.Offset = offset;
            try
            {
                object[] objArray = new object[count];
                for (int i = 0; i < count; i++)
                {
                    XmlBinaryNodeType nodeType = this.GetNodeType();
                    this.SkipNodeType();
                    this.ReadValue(nodeType, this.listValue);
                    objArray[i] = this.listValue.ToObject();
                }
                objArray2 = objArray;
            }
            finally
            {
                this.Offset = num;
            }
            return objArray2;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public XmlBinaryNodeType GetNodeType()
        {
            return (XmlBinaryNodeType) this.GetByte();
        }

        private int GetQuoteCharEntity(int offset, int length)
        {
            byte[] buffer = this.buffer;
            if (((length != 6) || (buffer[offset + 1] != 0x71)) || (((buffer[offset + 2] != 0x75) || (buffer[offset + 3] != 0x6f)) || (buffer[offset + 4] != 0x74)))
            {
                XmlExceptionHelper.ThrowInvalidCharRef(this.reader);
            }
            return 0x22;
        }

        public int[] GetRows()
        {
            if (this.buffer == null)
            {
                return new int[1];
            }
            ArrayList list = new ArrayList();
            list.Add(this.offsetMin);
            for (int i = this.offsetMin; i < this.offsetMax; i++)
            {
                if ((this.buffer[i] == 13) || (this.buffer[i] == 10))
                {
                    if (((i + 1) < this.offsetMax) && (this.buffer[i + 1] == 10))
                    {
                        i++;
                    }
                    list.Add(i + 1);
                }
            }
            return (int[]) list.ToArray(typeof(int));
        }

        [SecuritySafeCritical]
        public unsafe float GetSingle(int offset)
        {
            float num;
            byte[] buffer = this.buffer;
            byte* numPtr = (byte*) &num;
            numPtr[0] = buffer[offset];
            numPtr[1] = buffer[offset + 1];
            numPtr[2] = buffer[offset + 2];
            numPtr[3] = buffer[offset + 3];
            return num;
        }

        public string GetString(int offset, int length)
        {
            char[] charBuffer = this.GetCharBuffer(length);
            return new string(charBuffer, 0, this.GetChars(offset, length, charBuffer));
        }

        public string GetString(int offset, int length, XmlNameTable nameTable)
        {
            char[] charBuffer = this.GetCharBuffer(length);
            int num = this.GetChars(offset, length, charBuffer);
            return nameTable.Add(charBuffer, 0, num);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ulong GetUInt64(int offset)
        {
            return (ulong) this.GetInt64(offset);
        }

        public string GetUnicodeString(int offset, int length)
        {
            return XmlConverter.ToStringUnicode(this.buffer, offset, length);
        }

        public UniqueId GetUniqueId(int offset)
        {
            return new UniqueId(this.buffer, offset);
        }

        public void InsertBytes(byte[] buffer, int offset, int count)
        {
            if (this.offsetMax > (buffer.Length - count))
            {
                byte[] dst = new byte[this.offsetMax + count];
                System.Buffer.BlockCopy(this.buffer, 0, dst, 0, this.offsetMax);
                this.buffer = dst;
                this.streamBuffer = dst;
            }
            System.Buffer.BlockCopy(this.buffer, this.offset, this.buffer, this.offset + count, this.offsetMax - this.offset);
            this.offsetMax += count;
            System.Buffer.BlockCopy(buffer, offset, this.buffer, this.offset, count);
        }

        private bool IsAttrChar(int ch)
        {
            switch (ch)
            {
                case 9:
                case 10:
                case 13:
                case 0x26:
                    return false;
            }
            return true;
        }

        public bool IsWhitespaceKey(int key)
        {
            string str = this.GetDictionaryString(key).Value;
            for (int i = 0; i < str.Length; i++)
            {
                if (!XmlConverter.IsWhitespace(str[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsWhitespaceUnicode(int offset, int length)
        {
            for (int i = 0; i < length; i += 2)
            {
                char ch = (char) this.GetInt16(offset + i);
                if (!XmlConverter.IsWhitespace(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsWhitespaceUTF8(int offset, int length)
        {
            byte[] buffer = this.buffer;
            for (int i = 0; i < length; i++)
            {
                if (!XmlConverter.IsWhitespace((char) buffer[offset + i]))
                {
                    return false;
                }
            }
            return true;
        }

        public int ReadBytes(int count)
        {
            int offset = this.offset;
            if (offset > (this.offsetMax - count))
            {
                this.EnsureBytes(count);
            }
            this.offset += count;
            return offset;
        }

        public DateTime ReadDateTime()
        {
            DateTime time;
            long dateData = 0L;
            try
            {
                dateData = this.ReadInt64();
                time = DateTime.FromBinary(dateData);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(dateData.ToString(CultureInfo.InvariantCulture), "DateTime", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(dateData.ToString(CultureInfo.InvariantCulture), "DateTime", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(dateData.ToString(CultureInfo.InvariantCulture), "DateTime", exception3));
            }
            return time;
        }

        [SecuritySafeCritical]
        public unsafe decimal ReadDecimal()
        {
            int num;
            byte[] buffer = this.GetBuffer(0x10, out num);
            byte num2 = buffer[num];
            byte num3 = buffer[num + 1];
            byte num4 = buffer[num + 2];
            byte num5 = buffer[num + 3];
            int num6 = (((((num5 << 8) + num4) << 8) + num3) << 8) + num2;
            if (((num6 & 0x7f00ffff) == 0) && ((num6 & 0xff0000) <= 0x1c0000))
            {
                decimal num7;
                byte* numPtr = (byte*) &num7;
                for (int i = 0; i < 0x10; i++)
                {
                    numPtr[i] = buffer[num + i];
                }
                this.Advance(0x10);
                return num7;
            }
            XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            return 0M;
        }

        public int ReadDictionaryKey()
        {
            XmlDictionaryString str2;
            int num = this.ReadMultiByteUInt31();
            if ((num & 1) != 0)
            {
                XmlDictionaryString str;
                if (this.session == null)
                {
                    XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
                }
                int num2 = num >> 1;
                if (!this.session.TryLookup(num2, out str))
                {
                    if ((num2 < 0) || (num2 > 0x1fffffff))
                    {
                        XmlExceptionHelper.ThrowXmlDictionaryStringIDOutOfRange(this.reader);
                    }
                    XmlExceptionHelper.ThrowXmlDictionaryStringIDUndefinedSession(this.reader, num2);
                }
                return num;
            }
            if (this.dictionary == null)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }
            int key = num >> 1;
            if (!this.dictionary.TryLookup(key, out str2))
            {
                if ((key < 0) || (key > 0x1fffffff))
                {
                    XmlExceptionHelper.ThrowXmlDictionaryStringIDOutOfRange(this.reader);
                }
                XmlExceptionHelper.ThrowXmlDictionaryStringIDUndefinedStatic(this.reader, key);
            }
            return num;
        }

        [SecuritySafeCritical]
        public unsafe double ReadDouble()
        {
            int num;
            double num2;
            byte[] buffer = this.GetBuffer(8, out num);
            byte* numPtr = (byte*) &num2;
            numPtr[0] = buffer[num];
            numPtr[1] = buffer[num + 1];
            numPtr[2] = buffer[num + 2];
            numPtr[3] = buffer[num + 3];
            numPtr[4] = buffer[num + 4];
            numPtr[5] = buffer[num + 5];
            numPtr[6] = buffer[num + 6];
            numPtr[7] = buffer[num + 7];
            this.Advance(8);
            return num2;
        }

        public Guid ReadGuid()
        {
            int num;
            this.GetBuffer(0x10, out num);
            Guid guid = this.GetGuid(num);
            this.Advance(0x10);
            return guid;
        }

        public int ReadInt16()
        {
            return (short) this.ReadUInt16();
        }

        public int ReadInt32()
        {
            int num;
            byte[] buffer = this.GetBuffer(4, out num);
            byte num2 = buffer[num];
            byte num3 = buffer[num + 1];
            byte num4 = buffer[num + 2];
            byte num5 = buffer[num + 3];
            this.Advance(4);
            return ((((((num5 << 8) + num4) << 8) + num3) << 8) + num2);
        }

        public long ReadInt64()
        {
            long num = (long) ((ulong) this.ReadInt32());
            long num2 = (long) ((ulong) this.ReadInt32());
            return ((num2 << 0x20) + num);
        }

        public int ReadInt8()
        {
            return (sbyte) this.ReadUInt8();
        }

        private void ReadList(ValueHandle value)
        {
            XmlBinaryNodeType type;
            if (this.listValue == null)
            {
                this.listValue = new ValueHandle(this);
            }
            int length = 0;
            int offset = this.Offset;
        Label_001D:
            type = this.GetNodeType();
            this.SkipNodeType();
            if (type == XmlBinaryNodeType.StartListText)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }
            if (type != XmlBinaryNodeType.EndListText)
            {
                this.ReadValue(type, this.listValue);
                length++;
                goto Label_001D;
            }
            value.SetValue(ValueHandleType.List, offset, length);
        }

        public int ReadMultiByteUInt31()
        {
            int @byte = this.GetByte();
            this.Advance(1);
            if ((@byte & 0x80) != 0)
            {
                @byte &= 0x7f;
                int num2 = this.GetByte();
                this.Advance(1);
                @byte |= (num2 & 0x7f) << 7;
                if ((num2 & 0x80) == 0)
                {
                    return @byte;
                }
                int num3 = this.GetByte();
                this.Advance(1);
                @byte |= (num3 & 0x7f) << 14;
                if ((num3 & 0x80) == 0)
                {
                    return @byte;
                }
                int num4 = this.GetByte();
                this.Advance(1);
                @byte |= (num4 & 0x7f) << 0x15;
                if ((num4 & 0x80) == 0)
                {
                    return @byte;
                }
                int num5 = this.GetByte();
                this.Advance(1);
                @byte |= num5 << 0x1c;
                if ((num5 & 0xf8) != 0)
                {
                    XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
                }
            }
            return @byte;
        }

        public void ReadQName(ValueHandle value)
        {
            int prefix = this.ReadUInt8();
            if (prefix >= 0x1a)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }
            int key = this.ReadDictionaryKey();
            value.SetQNameValue(prefix, key);
        }

        [SecuritySafeCritical]
        public unsafe float ReadSingle()
        {
            int num;
            float num2;
            byte[] buffer = this.GetBuffer(4, out num);
            byte* numPtr = (byte*) &num2;
            numPtr[0] = buffer[num];
            numPtr[1] = buffer[num + 1];
            numPtr[2] = buffer[num + 2];
            numPtr[3] = buffer[num + 3];
            this.Advance(4);
            return num2;
        }

        public TimeSpan ReadTimeSpan()
        {
            TimeSpan span;
            long num = 0L;
            try
            {
                num = this.ReadInt64();
                span = TimeSpan.FromTicks(num);
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(num.ToString(CultureInfo.InvariantCulture), "TimeSpan", exception));
            }
            catch (FormatException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(num.ToString(CultureInfo.InvariantCulture), "TimeSpan", exception2));
            }
            catch (OverflowException exception3)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlExceptionHelper.CreateConversionException(num.ToString(CultureInfo.InvariantCulture), "TimeSpan", exception3));
            }
            return span;
        }

        public int ReadUInt16()
        {
            int num;
            byte[] buffer = this.GetBuffer(2, out num);
            int num2 = buffer[num] + (buffer[num + 1] << 8);
            this.Advance(2);
            return num2;
        }

        public int ReadUInt31()
        {
            int num = this.ReadInt32();
            if (num < 0)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }
            return num;
        }

        public int ReadUInt8()
        {
            byte @byte = this.GetByte();
            this.Advance(1);
            return @byte;
        }

        private void ReadUnicodeValue(ValueHandle value, int length)
        {
            if ((length & 1) != 0)
            {
                XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
            }
            this.ReadValue(value, ValueHandleType.Unicode, length);
        }

        public UniqueId ReadUniqueId()
        {
            int num;
            UniqueId id = new UniqueId(this.GetBuffer(0x10, out num), num);
            this.Advance(0x10);
            return id;
        }

        public string ReadUTF8String(int length)
        {
            int num;
            this.GetBuffer(length, out num);
            char[] charBuffer = this.GetCharBuffer(length);
            int num2 = this.GetChars(num, length, charBuffer);
            string str = new string(charBuffer, 0, num2);
            this.Advance(length);
            return str;
        }

        public void ReadValue(XmlBinaryNodeType nodeType, ValueHandle value)
        {
            switch (nodeType)
            {
                case XmlBinaryNodeType.MinText:
                    value.SetValue(ValueHandleType.Zero);
                    return;

                case XmlBinaryNodeType.OneText:
                    value.SetValue(ValueHandleType.One);
                    return;

                case XmlBinaryNodeType.FalseText:
                    value.SetValue(ValueHandleType.False);
                    return;

                case XmlBinaryNodeType.TrueText:
                    value.SetValue(ValueHandleType.True);
                    return;

                case XmlBinaryNodeType.Int8Text:
                    this.ReadValue(value, ValueHandleType.Int8, 1);
                    return;

                case XmlBinaryNodeType.Int16Text:
                    this.ReadValue(value, ValueHandleType.Int16, 2);
                    return;

                case XmlBinaryNodeType.Int32Text:
                    this.ReadValue(value, ValueHandleType.Int32, 4);
                    return;

                case XmlBinaryNodeType.Int64Text:
                    this.ReadValue(value, ValueHandleType.Int64, 8);
                    return;

                case XmlBinaryNodeType.FloatText:
                    this.ReadValue(value, ValueHandleType.Single, 4);
                    return;

                case XmlBinaryNodeType.DoubleText:
                    this.ReadValue(value, ValueHandleType.Double, 8);
                    return;

                case XmlBinaryNodeType.DecimalText:
                    this.ReadValue(value, ValueHandleType.Decimal, 0x10);
                    return;

                case XmlBinaryNodeType.DateTimeText:
                    this.ReadValue(value, ValueHandleType.DateTime, 8);
                    return;

                case XmlBinaryNodeType.Chars8Text:
                    this.ReadValue(value, ValueHandleType.UTF8, this.ReadUInt8());
                    return;

                case XmlBinaryNodeType.Chars16Text:
                    this.ReadValue(value, ValueHandleType.UTF8, this.ReadUInt16());
                    return;

                case XmlBinaryNodeType.Chars32Text:
                    this.ReadValue(value, ValueHandleType.UTF8, this.ReadUInt31());
                    return;

                case XmlBinaryNodeType.Bytes8Text:
                    this.ReadValue(value, ValueHandleType.Base64, this.ReadUInt8());
                    return;

                case XmlBinaryNodeType.Bytes16Text:
                    this.ReadValue(value, ValueHandleType.Base64, this.ReadUInt16());
                    return;

                case XmlBinaryNodeType.Bytes32Text:
                    this.ReadValue(value, ValueHandleType.Base64, this.ReadUInt31());
                    return;

                case XmlBinaryNodeType.StartListText:
                    this.ReadList(value);
                    return;

                case XmlBinaryNodeType.EmptyText:
                    value.SetValue(ValueHandleType.Empty);
                    return;

                case XmlBinaryNodeType.DictionaryText:
                    value.SetDictionaryValue(this.ReadDictionaryKey());
                    return;

                case XmlBinaryNodeType.UniqueIdText:
                    this.ReadValue(value, ValueHandleType.UniqueId, 0x10);
                    return;

                case XmlBinaryNodeType.TimeSpanText:
                    this.ReadValue(value, ValueHandleType.TimeSpan, 8);
                    return;

                case XmlBinaryNodeType.GuidText:
                    this.ReadValue(value, ValueHandleType.Guid, 0x10);
                    return;

                case XmlBinaryNodeType.UInt64Text:
                    this.ReadValue(value, ValueHandleType.UInt64, 8);
                    return;

                case XmlBinaryNodeType.BoolText:
                    value.SetValue((this.ReadUInt8() != 0) ? ValueHandleType.True : ValueHandleType.False);
                    return;

                case XmlBinaryNodeType.UnicodeChars8Text:
                    this.ReadUnicodeValue(value, this.ReadUInt8());
                    return;

                case XmlBinaryNodeType.UnicodeChars16Text:
                    this.ReadUnicodeValue(value, this.ReadUInt16());
                    return;

                case XmlBinaryNodeType.UnicodeChars32Text:
                    this.ReadUnicodeValue(value, this.ReadUInt31());
                    return;

                case XmlBinaryNodeType.QNameDictionaryText:
                    this.ReadQName(value);
                    return;
            }
            XmlExceptionHelper.ThrowInvalidBinaryFormat(this.reader);
        }

        private void ReadValue(ValueHandle value, ValueHandleType type, int length)
        {
            int offset = this.ReadBytes(length);
            value.SetValue(type, offset, length);
        }

        public void SetBuffer(Stream stream, IXmlDictionary dictionary, XmlBinaryReaderSession session)
        {
            if (this.streamBuffer == null)
            {
                this.streamBuffer = new byte[0x80];
            }
            this.SetBuffer(stream, this.streamBuffer, 0, 0, dictionary, session);
            this.windowOffset = 0;
            this.windowOffsetMax = this.streamBuffer.Length;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SetBuffer(byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlBinaryReaderSession session)
        {
            this.SetBuffer(null, buffer, offset, count, dictionary, session);
        }

        private void SetBuffer(Stream stream, byte[] buffer, int offset, int count, IXmlDictionary dictionary, XmlBinaryReaderSession session)
        {
            this.stream = stream;
            this.buffer = buffer;
            this.offsetMin = offset;
            this.offset = offset;
            this.offsetMax = offset + count;
            this.dictionary = dictionary;
            this.session = session;
        }

        public void SetWindow(int windowOffset, int windowLength)
        {
            if (windowOffset > (0x7fffffff - windowLength))
            {
                windowLength = 0x7fffffff - windowOffset;
            }
            if (this.offset != windowOffset)
            {
                System.Buffer.BlockCopy(this.buffer, this.offset, this.buffer, windowOffset, this.offsetMax - this.offset);
                this.offsetMax = windowOffset + (this.offsetMax - this.offset);
                this.offset = windowOffset;
            }
            this.windowOffset = windowOffset;
            this.windowOffsetMax = Math.Max(windowOffset + windowLength, this.offsetMax);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SkipByte()
        {
            this.Advance(1);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SkipNodeType()
        {
            this.SkipByte();
        }

        private bool TryEnsureByte()
        {
            if (this.stream == null)
            {
                return false;
            }
            if (this.offsetMax >= this.windowOffsetMax)
            {
                XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this.reader, this.windowOffsetMax - this.windowOffset);
            }
            if (this.offsetMax >= this.buffer.Length)
            {
                return this.TryEnsureBytes(1);
            }
            int num = this.stream.ReadByte();
            if (num == -1)
            {
                return false;
            }
            this.buffer[this.offsetMax++] = (byte) num;
            return true;
        }

        private bool TryEnsureBytes(int count)
        {
            if (this.stream == null)
            {
                return false;
            }
            if (this.offset > (0x7fffffff - count))
            {
                XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this.reader, this.windowOffsetMax - this.windowOffset);
            }
            int num = this.offset + count;
            if (num >= this.offsetMax)
            {
                int num3;
                if (num > this.windowOffsetMax)
                {
                    XmlExceptionHelper.ThrowMaxBytesPerReadExceeded(this.reader, this.windowOffsetMax - this.windowOffset);
                }
                if (num > this.buffer.Length)
                {
                    byte[] dst = new byte[Math.Max(num, this.buffer.Length * 2)];
                    System.Buffer.BlockCopy(this.buffer, 0, dst, 0, this.offsetMax);
                    this.buffer = dst;
                    this.streamBuffer = dst;
                }
                for (int i = num - this.offsetMax; i > 0; i -= num3)
                {
                    num3 = this.stream.Read(this.buffer, this.offsetMax, i);
                    if (num3 == 0)
                    {
                        return false;
                    }
                    this.offsetMax += num3;
                }
            }
            return true;
        }

        [SecurityCritical]
        public unsafe void UnsafeReadArray(byte* dst, byte* dstMax)
        {
            this.UnsafeReadArray(dst, (int) ((long) ((dstMax - dst) / 1)));
        }

        [SecurityCritical]
        private unsafe void UnsafeReadArray(byte* dst, int length)
        {
            if (this.stream != null)
            {
                while (length >= 0x100)
                {
                    byte[] buffer = this.GetBuffer(0x100, out this.offset);
                    for (int i = 0; i < 0x100; i++)
                    {
                        dst++;
                        dst[0] = buffer[this.offset + i];
                    }
                    this.Advance(0x100);
                    length -= 0x100;
                }
            }
            if (length > 0)
            {
                fixed (byte* numRef = &(this.GetBuffer(length, out this.offset)[this.offset]))
                {
                    byte* numPtr = numRef;
                    byte* numPtr2 = dst + length;
                    while (dst < numPtr2)
                    {
                        dst[0] = numPtr[0];
                        dst++;
                        numPtr++;
                    }
                }
                this.Advance(length);
            }
        }

        public byte[] Buffer
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buffer;
            }
        }

        public static XmlBufferReader Empty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return empty;
            }
        }

        public bool EndOfFile
        {
            get
            {
                return ((this.offset == this.offsetMax) && !this.TryEnsureByte());
            }
        }

        public bool IsStreamed
        {
            get
            {
                return (this.stream != null);
            }
        }

        public int Offset
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.offset;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.offset = value;
            }
        }
    }
}

