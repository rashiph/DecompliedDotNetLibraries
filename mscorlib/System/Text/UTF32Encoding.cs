namespace System.Text
{
    using System;
    using System.Security;

    [Serializable]
    public sealed class UTF32Encoding : Encoding
    {
        private bool bigEndian;
        private bool emitUTF32ByteOrderMark;
        private bool isThrowException;

        public UTF32Encoding() : this(false, true, false)
        {
        }

        public UTF32Encoding(bool bigEndian, bool byteOrderMark) : this(bigEndian, byteOrderMark, false)
        {
        }

        public UTF32Encoding(bool bigEndian, bool byteOrderMark, bool throwOnInvalidCharacters) : base(bigEndian ? 0x2ee1 : 0x2ee0)
        {
            this.bigEndian = bigEndian;
            this.emitUTF32ByteOrderMark = byteOrderMark;
            this.isThrowException = throwOnInvalidCharacters;
            if (this.isThrowException)
            {
                this.SetDefaultFallbacks();
            }
        }

        public override bool Equals(object value)
        {
            UTF32Encoding encoding = value as UTF32Encoding;
            if (encoding == null)
            {
                return false;
            }
            return ((((this.emitUTF32ByteOrderMark == encoding.emitUTF32ByteOrderMark) && (this.bigEndian == encoding.bigEndian)) && base.EncoderFallback.Equals(encoding.EncoderFallback)) && base.DecoderFallback.Equals(encoding.DecoderFallback));
        }

        [SecuritySafeCritical]
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

        [CLSCompliant(false), SecurityCritical]
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
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS encoder)
        {
            char ch2;
            char* charEnd = chars + count;
            char* charStart = chars;
            int num = 0;
            char charLeftOver = '\0';
            EncoderFallbackBuffer fallbackBuffer = null;
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                fallbackBuffer = encoder.FallbackBuffer;
                if (fallbackBuffer.Remaining > 0)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                }
            }
            else
            {
                fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
            }
            fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
        Label_00D9:
            while (((ch2 = fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch2 == '\0')
                {
                    ch2 = chars[0];
                    chars++;
                }
                if (charLeftOver != '\0')
                {
                    if (char.IsLowSurrogate(ch2))
                    {
                        charLeftOver = '\0';
                        num += 4;
                    }
                    else
                    {
                        chars--;
                        fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                        charLeftOver = '\0';
                    }
                }
                else if (char.IsHighSurrogate(ch2))
                {
                    charLeftOver = ch2;
                }
                else
                {
                    if (char.IsLowSurrogate(ch2))
                    {
                        fallbackBuffer.InternalFallback(ch2, ref chars);
                        continue;
                    }
                    num += 4;
                }
            }
            if (((encoder == null) || encoder.MustFlush) && (charLeftOver > '\0'))
            {
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                charLeftOver = '\0';
                goto Label_00D9;
            }
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GetByteCountOverflow"));
            }
            return num;
        }

        [SecurityCritical, CLSCompliant(false)]
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

        [SecuritySafeCritical]
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
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
        {
            char ch2;
            char* charStart = chars;
            char* charEnd = chars + charCount;
            byte* numPtr = bytes;
            byte* numPtr2 = bytes + byteCount;
            char cHigh = '\0';
            EncoderFallbackBuffer fallbackBuffer = null;
            if (encoder != null)
            {
                cHigh = encoder.charLeftOver;
                fallbackBuffer = encoder.FallbackBuffer;
                if (encoder.m_throwOnOverflow && (fallbackBuffer.Remaining > 0))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                }
            }
            else
            {
                fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
            }
            fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
        Label_023F:
            while (((ch2 = fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch2 == '\0')
                {
                    ch2 = chars[0];
                    chars++;
                }
                if (cHigh != '\0')
                {
                    if (char.IsLowSurrogate(ch2))
                    {
                        uint surrogate = this.GetSurrogate(cHigh, ch2);
                        cHigh = '\0';
                        if ((bytes + 3) >= numPtr2)
                        {
                            if (fallbackBuffer.bFallingBack)
                            {
                                fallbackBuffer.MovePrevious();
                                fallbackBuffer.MovePrevious();
                            }
                            else
                            {
                                chars -= 2;
                            }
                            base.ThrowBytesOverflow(encoder, bytes == numPtr);
                            cHigh = '\0';
                            break;
                        }
                        if (this.bigEndian)
                        {
                            bytes++;
                            bytes[0] = 0;
                            bytes++;
                            bytes[0] = (byte) (surrogate >> 0x10);
                            bytes++;
                            bytes[0] = (byte) (surrogate >> 8);
                            bytes++;
                            bytes[0] = (byte) surrogate;
                        }
                        else
                        {
                            bytes++;
                            bytes[0] = (byte) surrogate;
                            bytes++;
                            bytes[0] = (byte) (surrogate >> 8);
                            bytes++;
                            bytes[0] = (byte) (surrogate >> 0x10);
                            bytes++;
                            bytes[0] = 0;
                        }
                    }
                    else
                    {
                        chars--;
                        fallbackBuffer.InternalFallback(cHigh, ref chars);
                        cHigh = '\0';
                    }
                }
                else if (char.IsHighSurrogate(ch2))
                {
                    cHigh = ch2;
                }
                else
                {
                    if (char.IsLowSurrogate(ch2))
                    {
                        fallbackBuffer.InternalFallback(ch2, ref chars);
                        continue;
                    }
                    if ((bytes + 3) >= numPtr2)
                    {
                        if (fallbackBuffer.bFallingBack)
                        {
                            fallbackBuffer.MovePrevious();
                        }
                        else
                        {
                            chars--;
                        }
                        base.ThrowBytesOverflow(encoder, bytes == numPtr);
                        break;
                    }
                    if (this.bigEndian)
                    {
                        bytes++;
                        bytes[0] = 0;
                        bytes++;
                        bytes[0] = 0;
                        bytes++;
                        bytes[0] = (byte) (ch2 >> 8);
                        bytes++;
                        bytes[0] = (byte) ch2;
                    }
                    else
                    {
                        bytes++;
                        bytes[0] = (byte) ch2;
                        bytes++;
                        bytes[0] = (byte) (ch2 >> 8);
                        bytes++;
                        bytes[0] = 0;
                        bytes++;
                        bytes[0] = 0;
                    }
                }
            }
            if (((encoder == null) || encoder.MustFlush) && (cHigh > '\0'))
            {
                fallbackBuffer.InternalFallback(cHigh, ref chars);
                cHigh = '\0';
                goto Label_023F;
            }
            if (encoder != null)
            {
                encoder.charLeftOver = cHigh;
                encoder.m_charsUsed = (int) ((long) ((chars - charStart) / 2));
            }
            return (int) ((long) ((bytes - numPtr) / 1));
        }

        [CLSCompliant(false), SecurityCritical]
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
            UTF32Decoder decoder = (UTF32Decoder) baseDecoder;
            int num = 0;
            byte* numPtr = bytes + count;
            byte* byteStart = bytes;
            int readByteCount = 0;
            uint iChar = 0;
            DecoderFallbackBuffer fallbackBuffer = null;
            if (decoder != null)
            {
                readByteCount = decoder.readByteCount;
                iChar = (uint) decoder.iChar;
                fallbackBuffer = decoder.FallbackBuffer;
            }
            else
            {
                fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
            }
            fallbackBuffer.InternalInitialize(byteStart, null);
            while ((bytes < numPtr) && (num >= 0))
            {
                if (this.bigEndian)
                {
                    iChar = iChar << 8;
                    bytes++;
                    iChar += bytes[0];
                }
                else
                {
                    iChar = iChar >> 8;
                    bytes++;
                    iChar += (uint) (bytes[0] << 0x18);
                }
                readByteCount++;
                if (readByteCount >= 4)
                {
                    readByteCount = 0;
                    if ((iChar > 0x10ffff) || ((iChar >= 0xd800) && (iChar <= 0xdfff)))
                    {
                        byte[] buffer2;
                        if (this.bigEndian)
                        {
                            buffer2 = new byte[] { (byte) (iChar >> 0x18), (byte) (iChar >> 0x10), (byte) (iChar >> 8), (byte) iChar };
                        }
                        else
                        {
                            buffer2 = new byte[] { (byte) iChar, (byte) (iChar >> 8), (byte) (iChar >> 0x10), (byte) (iChar >> 0x18) };
                        }
                        num += fallbackBuffer.InternalFallback(buffer2, bytes);
                        iChar = 0;
                    }
                    else
                    {
                        if (iChar >= 0x10000)
                        {
                            num++;
                        }
                        num++;
                        iChar = 0;
                    }
                }
            }
            if ((readByteCount > 0) && ((decoder == null) || decoder.MustFlush))
            {
                byte[] buffer3 = new byte[readByteCount];
                if (!this.bigEndian)
                {
                    while (readByteCount > 0)
                    {
                        buffer3[--readByteCount] = (byte) (iChar >> 0x18);
                        iChar = iChar << 8;
                    }
                }
                else
                {
                    while (readByteCount > 0)
                    {
                        buffer3[--readByteCount] = (byte) iChar;
                        iChar = iChar >> 8;
                    }
                }
                num += fallbackBuffer.InternalFallback(buffer3, bytes);
            }
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GetByteCountOverflow"));
            }
            return num;
        }

        [CLSCompliant(false), SecurityCritical]
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
            UTF32Decoder decoder = (UTF32Decoder) baseDecoder;
            char* chPtr = chars;
            char* chPtr2 = chars + charCount;
            byte* numPtr = bytes;
            byte* numPtr2 = bytes + byteCount;
            int readByteCount = 0;
            uint iChar = 0;
            DecoderFallbackBuffer fallbackBuffer = null;
            if (decoder != null)
            {
                readByteCount = decoder.readByteCount;
                iChar = (uint) decoder.iChar;
                fallbackBuffer = baseDecoder.FallbackBuffer;
            }
            else
            {
                fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
            }
            fallbackBuffer.InternalInitialize(bytes, chars + charCount);
            while (bytes < numPtr2)
            {
                if (this.bigEndian)
                {
                    iChar = iChar << 8;
                    bytes++;
                    iChar += bytes[0];
                }
                else
                {
                    iChar = iChar >> 8;
                    bytes++;
                    iChar += (uint) (bytes[0] << 0x18);
                }
                readByteCount++;
                if (readByteCount >= 4)
                {
                    readByteCount = 0;
                    if ((iChar > 0x10ffff) || ((iChar >= 0xd800) && (iChar <= 0xdfff)))
                    {
                        byte[] buffer2;
                        if (this.bigEndian)
                        {
                            buffer2 = new byte[] { (byte) (iChar >> 0x18), (byte) (iChar >> 0x10), (byte) (iChar >> 8), (byte) iChar };
                        }
                        else
                        {
                            buffer2 = new byte[] { (byte) iChar, (byte) (iChar >> 8), (byte) (iChar >> 0x10), (byte) (iChar >> 0x18) };
                        }
                        if (!fallbackBuffer.InternalFallback(buffer2, bytes, ref chars))
                        {
                            bytes -= 4;
                            iChar = 0;
                            fallbackBuffer.InternalReset();
                            base.ThrowCharsOverflow(decoder, chars == chPtr);
                            break;
                        }
                        iChar = 0;
                    }
                    else
                    {
                        if (iChar >= 0x10000)
                        {
                            if (chars >= (chPtr2 - 1))
                            {
                                bytes -= 4;
                                iChar = 0;
                                base.ThrowCharsOverflow(decoder, chars == chPtr);
                                break;
                            }
                            chars++;
                            chars[0] = this.GetHighSurrogate(iChar);
                            iChar = this.GetLowSurrogate(iChar);
                        }
                        else if (chars >= chPtr2)
                        {
                            bytes -= 4;
                            iChar = 0;
                            base.ThrowCharsOverflow(decoder, chars == chPtr);
                            break;
                        }
                        chars++;
                        chars[0] = (char) iChar;
                        iChar = 0;
                    }
                }
            }
            if ((readByteCount > 0) && ((decoder == null) || decoder.MustFlush))
            {
                byte[] buffer3 = new byte[readByteCount];
                int num3 = readByteCount;
                if (!this.bigEndian)
                {
                    while (num3 > 0)
                    {
                        buffer3[--num3] = (byte) (iChar >> 0x18);
                        iChar = iChar << 8;
                    }
                }
                else
                {
                    while (num3 > 0)
                    {
                        buffer3[--num3] = (byte) iChar;
                        iChar = iChar >> 8;
                    }
                }
                if (!fallbackBuffer.InternalFallback(buffer3, bytes, ref chars))
                {
                    fallbackBuffer.InternalReset();
                    base.ThrowCharsOverflow(decoder, chars == chPtr);
                }
                else
                {
                    readByteCount = 0;
                    iChar = 0;
                }
            }
            if (decoder != null)
            {
                decoder.iChar = (int) iChar;
                decoder.readByteCount = readByteCount;
                decoder.m_bytesUsed = (int) ((long) ((bytes - numPtr) / 1));
            }
            return (int) ((long) ((chars - chPtr) / 2));
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new UTF32Decoder(this);
        }

        public override System.Text.Encoder GetEncoder()
        {
            return new EncoderNLS(this);
        }

        public override int GetHashCode()
        {
            return ((((base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode()) + this.CodePage) + (this.emitUTF32ByteOrderMark ? 4 : 0)) + (this.bigEndian ? 8 : 0));
        }

        private char GetHighSurrogate(uint iChar)
        {
            return (char) (((iChar - 0x10000) / 0x400) + 0xd800);
        }

        private char GetLowSurrogate(uint iChar)
        {
            return (char) (((iChar - 0x10000) % 0x400) + 0xdc00);
        }

        public override int GetMaxByteCount(int charCount)
        {
            if (charCount < 0)
            {
                throw new ArgumentOutOfRangeException("charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            long num = charCount + 1L;
            if (base.EncoderFallback.MaxCharCount > 1)
            {
                num *= base.EncoderFallback.MaxCharCount;
            }
            num *= 4L;
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
            int num = (byteCount / 2) + 2;
            if (base.DecoderFallback.MaxCharCount > 2)
            {
                num *= base.DecoderFallback.MaxCharCount;
                num /= 2;
            }
            if (num > 0x7fffffff)
            {
                throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("ArgumentOutOfRange_GetCharCountOverflow"));
            }
            return num;
        }

        public override byte[] GetPreamble()
        {
            if (!this.emitUTF32ByteOrderMark)
            {
                return Encoding.emptyByteArray;
            }
            if (this.bigEndian)
            {
                byte[] buffer = new byte[4];
                buffer[2] = 0xfe;
                buffer[3] = 0xff;
                return buffer;
            }
            byte[] buffer2 = new byte[4];
            buffer2[0] = 0xff;
            buffer2[1] = 0xfe;
            return buffer2;
        }

        [SecuritySafeCritical]
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

        private uint GetSurrogate(char cHigh, char cLow)
        {
            return (uint) ((((cHigh - 0xd800) * 0x400) + (cLow - 0xdc00)) + 0x10000);
        }

        internal override void SetDefaultFallbacks()
        {
            if (this.isThrowException)
            {
                base.encoderFallback = EncoderFallback.ExceptionFallback;
                base.decoderFallback = DecoderFallback.ExceptionFallback;
            }
            else
            {
                base.encoderFallback = new EncoderReplacementFallback("�");
                base.decoderFallback = new DecoderReplacementFallback("�");
            }
        }

        [Serializable]
        internal class UTF32Decoder : DecoderNLS
        {
            internal int iChar;
            internal int readByteCount;

            public UTF32Decoder(UTF32Encoding encoding) : base(encoding)
            {
            }

            public override void Reset()
            {
                this.iChar = 0;
                this.readByteCount = 0;
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            internal override bool HasState
            {
                get
                {
                    return (this.readByteCount != 0);
                }
            }
        }
    }
}

