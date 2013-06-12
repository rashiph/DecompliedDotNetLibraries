namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ASCIIEncoding : Encoding
    {
        public ASCIIEncoding() : base(0x4e9f)
        {
        }

        [SecuritySafeCritical]
        public override unsafe int GetByteCount(string chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars");
            }
            fixed (char* str = ((char*) chars))
            {
                char* chPtr = str;
                return this.GetByteCount(chPtr, chars.Length, null);
            }
        }

        [ComVisible(false), CLSCompliant(false), SecurityCritical]
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
        internal override unsafe int GetByteCount(char* chars, int charCount, EncoderNLS encoder)
        {
            char ch2;
            char charLeftOver = '\0';
            EncoderReplacementFallback encoderFallback = null;
            char* charEnd = chars + charCount;
            EncoderFallbackBuffer fallbackBuffer = null;
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                encoderFallback = encoder.Fallback as EncoderReplacementFallback;
                if (encoder.InternalHasFallbackBuffer)
                {
                    fallbackBuffer = encoder.FallbackBuffer;
                    if ((fallbackBuffer.Remaining > 0) && encoder.m_throwOnOverflow)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                    }
                    fallbackBuffer.InternalInitialize(chars, charEnd, encoder, false);
                }
            }
            else
            {
                encoderFallback = base.EncoderFallback as EncoderReplacementFallback;
            }
            if ((encoderFallback != null) && (encoderFallback.MaxCharCount == 1))
            {
                if (charLeftOver > '\0')
                {
                    charCount++;
                }
                return charCount;
            }
            int num = 0;
            if (charLeftOver > '\0')
            {
                fallbackBuffer = encoder.FallbackBuffer;
                fallbackBuffer.InternalInitialize(chars, charEnd, encoder, false);
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
            }
            while (((ch2 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch2 == '\0')
                {
                    ch2 = chars[0];
                    chars++;
                }
                if (ch2 > '\x007f')
                {
                    if (fallbackBuffer == null)
                    {
                        if (encoder == null)
                        {
                            fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = encoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(charEnd - charCount, charEnd, encoder, false);
                    }
                    fallbackBuffer.InternalFallback(ch2, ref chars);
                }
                else
                {
                    num++;
                }
            }
            return num;
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

        [SecuritySafeCritical]
        public override unsafe int GetBytes(string chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
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
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCount"));
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
            fixed (char* str = ((char*) chars))
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
            char ch4;
            char charLeftOver = '\0';
            EncoderReplacementFallback encoderFallback = null;
            EncoderFallbackBuffer fallbackBuffer = null;
            char* charEnd = chars + charCount;
            byte* numPtr = bytes;
            char* charStart = chars;
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                encoderFallback = encoder.Fallback as EncoderReplacementFallback;
                if (encoder.InternalHasFallbackBuffer)
                {
                    fallbackBuffer = encoder.FallbackBuffer;
                    if ((fallbackBuffer.Remaining > 0) && encoder.m_throwOnOverflow)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                    }
                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
                }
            }
            else
            {
                encoderFallback = base.EncoderFallback as EncoderReplacementFallback;
            }
            if ((encoderFallback != null) && (encoderFallback.MaxCharCount == 1))
            {
                char ch2 = encoderFallback.DefaultString[0];
                if (ch2 <= '\x007f')
                {
                    if (charLeftOver > '\0')
                    {
                        if (byteCount == 0)
                        {
                            base.ThrowBytesOverflow(encoder, true);
                        }
                        bytes++;
                        bytes[0] = (byte) ch2;
                        byteCount--;
                    }
                    if (byteCount < charCount)
                    {
                        base.ThrowBytesOverflow(encoder, byteCount < 1);
                        charEnd = chars + byteCount;
                    }
                    while (chars < charEnd)
                    {
                        chars++;
                        char ch3 = chars[0];
                        if (ch3 >= '\x0080')
                        {
                            bytes++;
                            bytes[0] = (byte) ch2;
                        }
                        else
                        {
                            bytes++;
                            bytes[0] = (byte) ch3;
                        }
                    }
                    if (encoder != null)
                    {
                        encoder.charLeftOver = '\0';
                        encoder.m_charsUsed = (int) ((long) ((chars - charStart) / 2));
                    }
                    return (int) ((long) ((bytes - numPtr) / 1));
                }
            }
            byte* numPtr2 = bytes + byteCount;
            if (charLeftOver > '\0')
            {
                fallbackBuffer = encoder.FallbackBuffer;
                fallbackBuffer.InternalInitialize(chars, charEnd, encoder, true);
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
            }
            while (((ch4 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch4 == '\0')
                {
                    ch4 = chars[0];
                    chars++;
                }
                if (ch4 > '\x007f')
                {
                    if (fallbackBuffer == null)
                    {
                        if (encoder == null)
                        {
                            fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = encoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(charEnd - charCount, charEnd, encoder, true);
                    }
                    fallbackBuffer.InternalFallback(ch4, ref chars);
                }
                else
                {
                    if (bytes >= numPtr2)
                    {
                        if ((fallbackBuffer == null) || !fallbackBuffer.bFallingBack)
                        {
                            chars--;
                        }
                        else
                        {
                            fallbackBuffer.MovePrevious();
                        }
                        base.ThrowBytesOverflow(encoder, bytes == numPtr);
                        break;
                    }
                    bytes[0] = (byte) ch4;
                    bytes++;
                }
            }
            if (encoder != null)
            {
                if ((fallbackBuffer != null) && !fallbackBuffer.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = (int) ((long) ((chars - charStart) / 2));
            }
            return (int) ((long) ((bytes - numPtr) / 1));
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
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS decoder)
        {
            DecoderReplacementFallback decoderFallback = null;
            if (decoder == null)
            {
                decoderFallback = base.DecoderFallback as DecoderReplacementFallback;
            }
            else
            {
                decoderFallback = decoder.Fallback as DecoderReplacementFallback;
            }
            if ((decoderFallback != null) && (decoderFallback.MaxCharCount == 1))
            {
                return count;
            }
            DecoderFallbackBuffer fallbackBuffer = null;
            int num = count;
            byte[] buffer2 = new byte[1];
            byte* numPtr = bytes + count;
            while (bytes < numPtr)
            {
                byte num2 = bytes[0];
                bytes++;
                if (num2 >= 0x80)
                {
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.DecoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(numPtr - count, null);
                    }
                    buffer2[0] = num2;
                    num--;
                    num += fallbackBuffer.InternalFallback(buffer2, bytes);
                }
            }
            return num;
        }

        [SecurityCritical, ComVisible(false), CLSCompliant(false)]
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
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS decoder)
        {
            byte* numPtr = bytes + byteCount;
            byte* numPtr2 = bytes;
            char* chPtr = chars;
            DecoderReplacementFallback decoderFallback = null;
            if (decoder == null)
            {
                decoderFallback = base.DecoderFallback as DecoderReplacementFallback;
            }
            else
            {
                decoderFallback = decoder.Fallback as DecoderReplacementFallback;
            }
            if ((decoderFallback != null) && (decoderFallback.MaxCharCount == 1))
            {
                char ch = decoderFallback.DefaultString[0];
                if (charCount < byteCount)
                {
                    base.ThrowCharsOverflow(decoder, charCount < 1);
                    numPtr = bytes + charCount;
                }
                while (bytes < numPtr)
                {
                    bytes++;
                    byte num = bytes[0];
                    if (num >= 0x80)
                    {
                        chars++;
                        chars[0] = ch;
                    }
                    else
                    {
                        chars++;
                        chars[0] = (char) num;
                    }
                }
                if (decoder != null)
                {
                    decoder.m_bytesUsed = (int) ((long) ((bytes - numPtr2) / 1));
                }
                return (int) ((long) ((chars - chPtr) / 2));
            }
            DecoderFallbackBuffer fallbackBuffer = null;
            byte[] buffer2 = new byte[1];
            char* charEnd = chars + charCount;
            while (bytes < numPtr)
            {
                byte num2 = bytes[0];
                bytes++;
                if (num2 >= 0x80)
                {
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.DecoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(numPtr - byteCount, charEnd);
                    }
                    buffer2[0] = num2;
                    if (fallbackBuffer.InternalFallback(buffer2, bytes, ref chars))
                    {
                        continue;
                    }
                    bytes--;
                    fallbackBuffer.InternalReset();
                    base.ThrowCharsOverflow(decoder, chars == chPtr);
                    break;
                }
                if (chars >= charEnd)
                {
                    bytes--;
                    base.ThrowCharsOverflow(decoder, chars == chPtr);
                    break;
                }
                chars[0] = (char) num2;
                chars++;
            }
            if (decoder != null)
            {
                decoder.m_bytesUsed = (int) ((long) ((bytes - numPtr2) / 1));
            }
            return (int) ((long) ((chars - chPtr) / 2));
        }

        [ComVisible(false)]
        public override System.Text.Decoder GetDecoder()
        {
            return new DecoderNLS(this);
        }

        [ComVisible(false)]
        public override System.Text.Encoder GetEncoder()
        {
            return new EncoderNLS(this);
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
            long num = byteCount;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num *= base.DecoderFallback.MaxCharCount;
            }
            if (num > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("ArgumentOutOfRange_GetCharCountOverflow"));
            }
            return (int) num;
        }

        [SecuritySafeCritical]
        public override unsafe string GetString(byte[] bytes, int byteIndex, int byteCount)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteIndex < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - byteIndex) < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (bytes.Length == 0)
            {
                return string.Empty;
            }
            fixed (byte* numRef = bytes)
            {
                return string.CreateStringFromEncoding(numRef + byteIndex, byteCount, this);
            }
        }

        internal override void SetDefaultFallbacks()
        {
            base.encoderFallback = EncoderFallback.ReplacementFallback;
            base.decoderFallback = DecoderFallback.ReplacementFallback;
        }

        [ComVisible(false)]
        public override bool IsSingleByte
        {
            get
            {
                return true;
            }
        }
    }
}

