namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class UTF7Encoding : Encoding
    {
        private byte[] base64Bytes;
        private const string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        private sbyte[] base64Values;
        private const string directChars = "\t\n\r '(),-./0123456789:?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private bool[] directEncode;
        [OptionalField(VersionAdded=2)]
        private bool m_allowOptionals;
        private const string optionalChars = "!\"#$%&*;<=>@[]^_`{|}";
        private const int UTF7_CODEPAGE = 0xfde8;

        public UTF7Encoding() : this(false)
        {
        }

        public UTF7Encoding(bool allowOptionals) : base(0xfde8)
        {
            this.m_allowOptionals = allowOptionals;
            this.MakeTables();
        }

        [ComVisible(false)]
        public override bool Equals(object value)
        {
            UTF7Encoding encoding = value as UTF7Encoding;
            if (encoding == null)
            {
                return false;
            }
            return (((this.m_allowOptionals == encoding.m_allowOptionals) && base.EncoderFallback.Equals(encoding.EncoderFallback)) && base.DecoderFallback.Equals(encoding.DecoderFallback));
        }

        [ComVisible(false), SecuritySafeCritical]
        public override unsafe int GetByteCount(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            fixed (char* str = ((char*) s))
            {
                char* chars = str;
                return this.GetByteCount(chars, s.Length, null);
            }
        }

        [CLSCompliant(false), SecurityCritical, ComVisible(false)]
        public override unsafe int GetByteCount(char* chars, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return this.GetByteCount(chars, count, null);
        }

        [SecuritySafeCritical]
        public override unsafe int GetByteCount(char[] chars, int index, int count)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((chars.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (chars.Length == 0)
            {
                return 0;
            }
            fixed (char* chRef = chars)
            {
                return this.GetByteCount(chRef + index, count, null);
            }
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS baseEncoder)
        {
            return this.GetBytes(chars, count, null, 0, baseEncoder);
        }

        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charCount < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return this.GetBytes(chars, charCount, bytes, byteCount, null);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public override unsafe int GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if ((s == null) || (bytes == null))
            {
                throw new ArgumentNullException((s == null) ? "s" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charIndex < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((s.Length - charIndex) < charCount)
            {
                throw new ArgumentOutOfRangeException("s", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
            }
            if ((byteIndex < 0) || (byteIndex > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* str = ((char*) s))
            {
                char* chPtr = str;
                fixed (byte* numRef = bytes)
                {
                    return this.GetBytes(chPtr + charIndex, charCount, numRef + byteIndex, byteCount, null);
                }
            }
        }

        [SecuritySafeCritical]
        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
        {
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charIndex < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((chars.Length - charIndex) < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if ((byteIndex < 0) || (byteIndex > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (chars.Length == 0)
            {
                return 0;
            }
            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* chRef = chars)
            {
                fixed (byte* numRef = bytes)
                {
                    return this.GetBytes(chRef + charIndex, charCount, numRef + byteIndex, byteCount, null);
                }
            }
        }

        [SecurityCritical]
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS baseEncoder)
        {
            Encoder inEncoder = (Encoder) baseEncoder;
            int bits = 0;
            int bitCount = -1;
            Encoding.EncodingByteBuffer buffer = new Encoding.EncodingByteBuffer(this, inEncoder, bytes, byteCount, chars, charCount);
            if (inEncoder != null)
            {
                bits = inEncoder.bits;
                bitCount = inEncoder.bitCount;
                while (bitCount >= 6)
                {
                    bitCount -= 6;
                    if (!buffer.AddByte(this.base64Bytes[(bits >> bitCount) & 0x3f]))
                    {
                        base.ThrowBytesOverflow(inEncoder, buffer.Count == 0);
                    }
                }
            }
            while (buffer.MoreData)
            {
                char nextChar = buffer.GetNextChar();
                if ((nextChar < '\x0080') && this.directEncode[nextChar])
                {
                    if (bitCount >= 0)
                    {
                        if (bitCount > 0)
                        {
                            if (!buffer.AddByte(this.base64Bytes[(bits << (6 - bitCount)) & 0x3f]))
                            {
                                break;
                            }
                            bitCount = 0;
                        }
                        if (!buffer.AddByte(0x2d))
                        {
                            break;
                        }
                        bitCount = -1;
                    }
                    if (buffer.AddByte((byte) nextChar))
                    {
                        continue;
                    }
                    break;
                }
                if ((bitCount < 0) && (nextChar == '+'))
                {
                    if (buffer.AddByte(0x2b, (byte) 0x2d))
                    {
                        continue;
                    }
                    break;
                }
                if (bitCount < 0)
                {
                    if (!buffer.AddByte(0x2b))
                    {
                        break;
                    }
                    bitCount = 0;
                }
                bits = (bits << 0x10) | nextChar;
                bitCount += 0x10;
                while (bitCount >= 6)
                {
                    bitCount -= 6;
                    if (!buffer.AddByte(this.base64Bytes[(bits >> bitCount) & 0x3f]))
                    {
                        bitCount += 6;
                        nextChar = buffer.GetNextChar();
                        break;
                    }
                }
                if (bitCount >= 6)
                {
                    break;
                }
            }
            if ((bitCount >= 0) && ((inEncoder == null) || inEncoder.MustFlush))
            {
                if ((bitCount > 0) && buffer.AddByte(this.base64Bytes[(bits << (6 - bitCount)) & 0x3f]))
                {
                    bitCount = 0;
                }
                if (buffer.AddByte(0x2d))
                {
                    bits = 0;
                    bitCount = -1;
                }
                else
                {
                    buffer.GetNextChar();
                }
            }
            if ((bytes != null) && (inEncoder != null))
            {
                inEncoder.bits = bits;
                inEncoder.bitCount = bitCount;
                inEncoder.m_charsUsed = buffer.CharsUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
        public override unsafe int GetCharCount(byte* bytes, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return this.GetCharCount(bytes, count, null);
        }

        [SecuritySafeCritical]
        public override unsafe int GetCharCount(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (bytes.Length == 0)
            {
                return 0;
            }
            fixed (byte* numRef = bytes)
            {
                return this.GetCharCount(numRef + index, count, null);
            }
        }

        [SecurityCritical]
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
        {
            return this.GetChars(bytes, count, null, 0, baseDecoder);
        }

        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
        public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charCount < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            return this.GetChars(bytes, byteCount, chars, charCount, null);
        }

        [SecuritySafeCritical]
        public override unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteIndex < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - byteIndex) < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if ((charIndex < 0) || (charIndex > chars.Length))
            {
                throw new ArgumentOutOfRangeException("charIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (bytes.Length == 0)
            {
                return 0;
            }
            int charCount = chars.Length - charIndex;
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            fixed (byte* numRef = bytes)
            {
                fixed (char* chRef = chars)
                {
                    return this.GetChars(numRef + byteIndex, byteCount, chRef + charIndex, charCount, null);
                }
            }
        }

        [SecurityCritical]
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
        {
            Decoder decoder = (Decoder) baseDecoder;
            Encoding.EncodingCharBuffer buffer = new Encoding.EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            int bits = 0;
            int bitCount = -1;
            bool firstByte = false;
            if (decoder != null)
            {
                bits = decoder.bits;
                bitCount = decoder.bitCount;
                firstByte = decoder.firstByte;
            }
            if (bitCount >= 0x10)
            {
                if (!buffer.AddChar((char) ((bits >> (bitCount - 0x10)) & 0xffff)))
                {
                    base.ThrowCharsOverflow(decoder, true);
                }
                bitCount -= 0x10;
            }
            while (buffer.MoreData)
            {
                int num4;
                byte nextByte = buffer.GetNextByte();
                if (bitCount >= 0)
                {
                    sbyte num5;
                    if ((nextByte < 0x80) && ((num5 = this.base64Values[nextByte]) >= 0))
                    {
                        firstByte = false;
                        bits = (bits << 6) | ((byte) num5);
                        bitCount += 6;
                        if (bitCount < 0x10)
                        {
                            continue;
                        }
                        num4 = (bits >> (bitCount - 0x10)) & 0xffff;
                        bitCount -= 0x10;
                        goto Label_00FB;
                    }
                    bitCount = -1;
                    if (nextByte == 0x2d)
                    {
                        if (!firstByte)
                        {
                            continue;
                        }
                        num4 = 0x2b;
                        goto Label_00FB;
                    }
                    if (buffer.Fallback(nextByte))
                    {
                        continue;
                    }
                    break;
                }
                if (nextByte == 0x2b)
                {
                    bitCount = 0;
                    firstByte = true;
                    continue;
                }
                if (nextByte >= 0x80)
                {
                    if (buffer.Fallback(nextByte))
                    {
                        continue;
                    }
                    break;
                }
                num4 = nextByte;
            Label_00FB:
                if ((num4 >= 0) && !buffer.AddChar((char) num4))
                {
                    if (bitCount >= 0)
                    {
                        buffer.AdjustBytes(1);
                        bitCount += 0x10;
                    }
                    break;
                }
            }
            if ((chars != null) && (decoder != null))
            {
                if (decoder.MustFlush)
                {
                    decoder.bits = 0;
                    decoder.bitCount = -1;
                    decoder.firstByte = false;
                }
                else
                {
                    decoder.bits = bits;
                    decoder.bitCount = bitCount;
                    decoder.firstByte = firstByte;
                }
                decoder.m_bytesUsed = buffer.BytesUsed;
            }
            return buffer.Count;
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new Decoder(this);
        }

        public override System.Text.Encoder GetEncoder()
        {
            return new Encoder(this);
        }

        [ComVisible(false)]
        public override int GetHashCode()
        {
            return ((this.CodePage + base.EncoderFallback.GetHashCode()) + base.DecoderFallback.GetHashCode());
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            long num = (charCount * 3L) + 2L;
            if (num > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("ArgumentOutOfRange_GetByteCountOverflow"));
            }
            return (int) num;
        }

        public override int GetMaxCharCount(int byteCount)
        {
            if (byteCount < 0)
            {
                throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            int num = byteCount;
            if (num == 0)
            {
                num = 1;
            }
            return num;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public override unsafe string GetString(byte[] bytes, int index, int count)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            fixed (byte* numRef = bytes)
            {
                return string.CreateStringFromEncoding(numRef + index, count, this);
            }
        }

        private void MakeTables()
        {
            this.base64Bytes = new byte[0x40];
            for (int i = 0; i < 0x40; i++)
            {
                this.base64Bytes[i] = (byte) "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/"[i];
            }
            this.base64Values = new sbyte[0x80];
            for (int j = 0; j < 0x80; j++)
            {
                this.base64Values[j] = -1;
            }
            for (int k = 0; k < 0x40; k++)
            {
                this.base64Values[this.base64Bytes[k]] = (sbyte) k;
            }
            this.directEncode = new bool[0x80];
            int length = "\t\n\r '(),-./0123456789:?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Length;
            for (int m = 0; m < length; m++)
            {
                this.directEncode["\t\n\r '(),-./0123456789:?ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"[m]] = true;
            }
            if (this.m_allowOptionals)
            {
                length = "!\"#$%&*;<=>@[]^_`{|}".Length;
                for (int n = 0; n < length; n++)
                {
                    this.directEncode["!\"#$%&*;<=>@[]^_`{|}"[n]] = true;
                }
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext ctx)
        {
            base.OnDeserialized();
            if (base.m_deserializedFromEverett)
            {
                this.m_allowOptionals = this.directEncode["!\"#$%&*;<=>@[]^_`{|}"[0]];
            }
            this.MakeTables();
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            base.OnDeserializing();
        }

        internal override void SetDefaultFallbacks()
        {
            base.encoderFallback = new EncoderReplacementFallback(string.Empty);
            base.decoderFallback = new DecoderUTF7Fallback();
        }

        [Serializable]
        private class Decoder : DecoderNLS, ISerializable
        {
            internal int bitCount;
            internal int bits;
            internal bool firstByte;

            public Decoder(UTF7Encoding encoding) : base(encoding)
            {
            }

            internal Decoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.bits = (int) info.GetValue("bits", typeof(int));
                this.bitCount = (int) info.GetValue("bitCount", typeof(int));
                this.firstByte = (bool) info.GetValue("firstByte", typeof(bool));
                base.m_encoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
            }

            public override void Reset()
            {
                this.bits = 0;
                this.bitCount = -1;
                this.firstByte = false;
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                info.AddValue("encoding", base.m_encoding);
                info.AddValue("bits", this.bits);
                info.AddValue("bitCount", this.bitCount);
                info.AddValue("firstByte", this.firstByte);
            }

            internal override bool HasState
            {
                get
                {
                    return (this.bitCount != -1);
                }
            }
        }

        [Serializable]
        internal sealed class DecoderUTF7Fallback : DecoderFallback
        {
            public override DecoderFallbackBuffer CreateFallbackBuffer()
            {
                return new UTF7Encoding.DecoderUTF7FallbackBuffer(this);
            }

            public override bool Equals(object value)
            {
                return (value is UTF7Encoding.DecoderUTF7Fallback);
            }

            public override int GetHashCode()
            {
                return 0x3d8;
            }

            public override int MaxCharCount
            {
                get
                {
                    return 1;
                }
            }
        }

        internal sealed class DecoderUTF7FallbackBuffer : DecoderFallbackBuffer
        {
            private char cFallback;
            private int iCount = -1;
            private int iSize;

            public DecoderUTF7FallbackBuffer(UTF7Encoding.DecoderUTF7Fallback fallback)
            {
            }

            public override bool Fallback(byte[] bytesUnknown, int index)
            {
                this.cFallback = (char) bytesUnknown[0];
                if (this.cFallback == '\0')
                {
                    return false;
                }
                this.iCount = this.iSize = 1;
                return true;
            }

            public override char GetNextChar()
            {
                if (this.iCount-- > 0)
                {
                    return this.cFallback;
                }
                return '\0';
            }

            [SecurityCritical]
            internal override unsafe int InternalFallback(byte[] bytes, byte* pBytes)
            {
                if (bytes.Length != 1)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
                }
                if (bytes[0] != 0)
                {
                    return 1;
                }
                return 0;
            }

            public override bool MovePrevious()
            {
                if (this.iCount >= 0)
                {
                    this.iCount++;
                }
                return ((this.iCount >= 0) && (this.iCount <= this.iSize));
            }

            [SecuritySafeCritical]
            public override unsafe void Reset()
            {
                this.iCount = -1;
                base.byteStart = null;
            }

            public override int Remaining
            {
                get
                {
                    if (this.iCount <= 0)
                    {
                        return 0;
                    }
                    return this.iCount;
                }
            }
        }

        [Serializable]
        private class Encoder : EncoderNLS, ISerializable
        {
            internal int bitCount;
            internal int bits;

            public Encoder(UTF7Encoding encoding) : base(encoding)
            {
            }

            internal Encoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.bits = (int) info.GetValue("bits", typeof(int));
                this.bitCount = (int) info.GetValue("bitCount", typeof(int));
                base.m_encoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
            }

            public override void Reset()
            {
                this.bitCount = -1;
                this.bits = 0;
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            [SecurityCritical]
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                info.AddValue("encoding", base.m_encoding);
                info.AddValue("bits", this.bits);
                info.AddValue("bitCount", this.bitCount);
            }

            internal override bool HasState
            {
                get
                {
                    if (this.bits == 0)
                    {
                        return (this.bitCount != -1);
                    }
                    return true;
                }
            }
        }
    }
}

