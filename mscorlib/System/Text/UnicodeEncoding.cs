namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class UnicodeEncoding : Encoding
    {
        internal bool bigEndian;
        internal bool byteOrderMark;
        public const int CharSize = 2;
        [OptionalField(VersionAdded=2)]
        internal bool isThrowException;

        public UnicodeEncoding() : this(false, true)
        {
        }

        public UnicodeEncoding(bool bigEndian, bool byteOrderMark) : this(bigEndian, byteOrderMark, false)
        {
        }

        public UnicodeEncoding(bool bigEndian, bool byteOrderMark, bool throwOnInvalidBytes) : base(bigEndian ? 0x4b1 : 0x4b0)
        {
            this.byteOrderMark = true;
            this.isThrowException = throwOnInvalidBytes;
            this.bigEndian = bigEndian;
            this.byteOrderMark = byteOrderMark;
            if (this.isThrowException)
            {
                this.SetDefaultFallbacks();
            }
        }

        public override bool Equals(object value)
        {
            UnicodeEncoding encoding = value as UnicodeEncoding;
            if (encoding == null)
            {
                return false;
            }
            return ((((this.CodePage == encoding.CodePage) && (this.byteOrderMark == encoding.byteOrderMark)) && ((this.bigEndian == encoding.bigEndian) && base.EncoderFallback.Equals(encoding.EncoderFallback))) && base.DecoderFallback.Equals(encoding.DecoderFallback));
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

        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
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
            int num = count << 1;
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_GetByteCountOverflow"));
            }
            char* charStart = chars;
            char* charEnd = chars + count;
            char charLeftOver = '\0';
            bool flag = false;
            ulong* numPtr = (ulong*) (charEnd - 3);
            EncoderFallbackBuffer fallbackBuffer = null;
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                if (charLeftOver > '\0')
                {
                    num += 2;
                }
                if (encoder.InternalHasFallbackBuffer)
                {
                    if ((fallbackBuffer = encoder.FallbackBuffer).Remaining > 0)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                    }
                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
                }
            }
            while (true)
            {
                char ch2;
                while (((ch2 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
                {
                    if (ch2 == '\0')
                    {
                        if ((!this.bigEndian && (charLeftOver == '\0')) && ((((int) chars) & 3) == 0))
                        {
                            ulong* numPtr2 = (ulong*) chars;
                            while (numPtr2 < numPtr)
                            {
                                if ((9223512776490647552L & numPtr2[0]) != 0L)
                                {
                                    ulong num2 = (17870556004450629632L & numPtr2[0]) ^ 15564677810327967744L;
                                    if (((((num2 & 18446462598732840960L) == 0L) || ((num2 & ((ulong) 0xffff00000000L)) == 0L)) || (((num2 & 0xffff0000L) == 0L) || ((num2 & ((ulong) 0xffffL)) == 0L))) && (((18158790778715962368L & numPtr2[0]) ^ 15852908186546788352L) != 0L))
                                    {
                                        break;
                                    }
                                }
                                numPtr2++;
                            }
                            chars = (char*) numPtr2;
                            if (chars >= charEnd)
                            {
                                break;
                            }
                        }
                        ch2 = chars[0];
                        chars++;
                    }
                    else
                    {
                        num += 2;
                    }
                    if ((ch2 >= 0xd800) && (ch2 <= 0xdfff))
                    {
                        if (ch2 <= 0xdbff)
                        {
                            if (charLeftOver > '\0')
                            {
                                chars--;
                                num -= 2;
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
                                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
                                }
                                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                                charLeftOver = '\0';
                            }
                            else
                            {
                                charLeftOver = ch2;
                            }
                        }
                        else if (charLeftOver == '\0')
                        {
                            num -= 2;
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
                                fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
                            }
                            fallbackBuffer.InternalFallback(ch2, ref chars);
                        }
                        else
                        {
                            charLeftOver = '\0';
                        }
                    }
                    else if (charLeftOver > '\0')
                    {
                        chars--;
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
                            fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
                        }
                        fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                        num -= 2;
                        charLeftOver = '\0';
                    }
                }
                if (charLeftOver <= '\0')
                {
                    return num;
                }
                num -= 2;
                if ((encoder != null) && !encoder.MustFlush)
                {
                    return num;
                }
                if (flag)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_RecursiveFallback", new object[] { charLeftOver }), "chars");
                }
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
                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
                }
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                charLeftOver = '\0';
                flag = true;
            }
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
            char charLeftOver = '\0';
            bool flag = false;
            byte* numPtr = bytes + byteCount;
            char* charEnd = chars + charCount;
            byte* numPtr2 = bytes;
            char* charStart = chars;
            EncoderFallbackBuffer fallbackBuffer = null;
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                if (encoder.InternalHasFallbackBuffer)
                {
                    fallbackBuffer = encoder.FallbackBuffer;
                    if ((fallbackBuffer.Remaining > 0) && encoder.m_throwOnOverflow)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                    }
                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, false);
                }
            }
        Label_047C:
            while (((ch2 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch2 == '\0')
                {
                    if ((!this.bigEndian && ((((int) chars) & 3) == 0)) && (((((int) bytes) & 3) == 0) && (charLeftOver == '\0')))
                    {
                        ulong* numPtr3 = (ulong*) ((chars - 3) + ((((((long) ((numPtr - bytes) / 1)) >> 1) < ((long) ((charEnd - chars) / 2))) ? (((long) ((numPtr - bytes) / 1)) >> 1) : ((long) ((charEnd - chars) / 2))) * 2L));
                        ulong* numPtr4 = (ulong*) chars;
                        ulong* numPtr5 = (ulong*) bytes;
                        while (numPtr4 < numPtr3)
                        {
                            if ((9223512776490647552L & numPtr4[0]) != 0L)
                            {
                                ulong num = (17870556004450629632L & numPtr4[0]) ^ 15564677810327967744L;
                                if (((((num & 18446462598732840960L) == 0L) || ((num & ((ulong) 0xffff00000000L)) == 0L)) || (((num & 0xffff0000L) == 0L) || ((num & ((ulong) 0xffffL)) == 0L))) && (((18158790778715962368L & numPtr4[0]) ^ 15852908186546788352L) != 0L))
                                {
                                    break;
                                }
                            }
                            numPtr5[0] = numPtr4[0];
                            numPtr4++;
                            numPtr5++;
                        }
                        chars = (char*) numPtr4;
                        bytes = (byte*) numPtr5;
                        if (chars >= charEnd)
                        {
                            break;
                        }
                    }
                    else if (((charLeftOver == '\0') && !this.bigEndian) && (((((int) chars) & 3) != (((int) bytes) & 3)) && ((((int) bytes) & 1) == 0)))
                    {
                        long num2 = ((((long) ((numPtr - bytes) / 1)) >> 1) < ((long) ((charEnd - chars) / 2))) ? (((long) ((numPtr - bytes) / 1)) >> 1) : ((long) ((charEnd - chars) / 2));
                        char* chPtr3 = (char*) bytes;
                        char* chPtr4 = (char*) ((chars + (num2 * 2L)) - 1);
                        while (chars < chPtr4)
                        {
                            if ((chars[0] >= 0xd800) && (chars[0] <= 0xdfff))
                            {
                                if (((chars[0] >= 0xdc00) || (chars[1] < 0xdc00)) || (chars[1] > 0xdfff))
                                {
                                    break;
                                }
                            }
                            else if ((chars[1] >= 0xd800) && (chars[1] <= 0xdfff))
                            {
                                chPtr3[0] = chars[0];
                                chPtr3++;
                                chars++;
                                continue;
                            }
                            chPtr3[0] = chars[0];
                            chPtr3[1] = chars[1];
                            chPtr3 += 2;
                            chars += 2;
                        }
                        bytes = (byte*) chPtr3;
                        if (chars >= charEnd)
                        {
                            break;
                        }
                    }
                    ch2 = chars[0];
                    chars++;
                }
                if ((ch2 >= 0xd800) && (ch2 <= 0xdfff))
                {
                    if (ch2 <= 0xdbff)
                    {
                        if (charLeftOver > '\0')
                        {
                            chars--;
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
                                fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
                            }
                            fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                            charLeftOver = '\0';
                        }
                        else
                        {
                            charLeftOver = ch2;
                        }
                        continue;
                    }
                    if (charLeftOver == '\0')
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
                            fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
                        }
                        fallbackBuffer.InternalFallback(ch2, ref chars);
                        continue;
                    }
                    if ((bytes + 3) >= numPtr)
                    {
                        if ((fallbackBuffer != null) && fallbackBuffer.bFallingBack)
                        {
                            fallbackBuffer.MovePrevious();
                            fallbackBuffer.MovePrevious();
                        }
                        else
                        {
                            chars -= 2;
                        }
                        base.ThrowBytesOverflow(encoder, bytes == numPtr2);
                        charLeftOver = '\0';
                        break;
                    }
                    if (this.bigEndian)
                    {
                        bytes++;
                        bytes[0] = (byte) (charLeftOver >> 8);
                        bytes++;
                        bytes[0] = (byte) charLeftOver;
                    }
                    else
                    {
                        bytes++;
                        bytes[0] = (byte) charLeftOver;
                        bytes++;
                        bytes[0] = (byte) (charLeftOver >> 8);
                    }
                    charLeftOver = '\0';
                }
                else if (charLeftOver > '\0')
                {
                    chars--;
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
                        fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
                    }
                    fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                    charLeftOver = '\0';
                    continue;
                }
                if ((bytes + 1) >= numPtr)
                {
                    if ((fallbackBuffer != null) && fallbackBuffer.bFallingBack)
                    {
                        fallbackBuffer.MovePrevious();
                    }
                    else
                    {
                        chars--;
                    }
                    base.ThrowBytesOverflow(encoder, bytes == numPtr2);
                    break;
                }
                if (this.bigEndian)
                {
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
                }
            }
            if ((charLeftOver > '\0') && ((encoder == null) || encoder.MustFlush))
            {
                if (flag)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_RecursiveFallback", new object[] { charLeftOver }), "chars");
                }
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
                    fallbackBuffer.InternalInitialize(charStart, charEnd, encoder, true);
                }
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                charLeftOver = '\0';
                flag = true;
                goto Label_047C;
            }
            if (encoder != null)
            {
                encoder.charLeftOver = charLeftOver;
                encoder.m_charsUsed = (int) ((long) ((chars - charStart) / 2));
            }
            return (int) ((long) ((bytes - numPtr2) / 1));
        }

        [SecurityCritical, ComVisible(false), CLSCompliant(false)]
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
            Decoder decoder = (Decoder) baseDecoder;
            byte* numPtr = bytes + count;
            byte* byteStart = bytes;
            int lastByte = -1;
            char lastChar = '\0';
            int num2 = count >> 1;
            ulong* numPtr3 = (ulong*) (numPtr - 7);
            DecoderFallbackBuffer fallbackBuffer = null;
            if (decoder != null)
            {
                lastByte = decoder.lastByte;
                lastChar = decoder.lastChar;
                if (lastChar > '\0')
                {
                    num2++;
                }
                if ((lastByte >= 0) && ((count & 1) == 1))
                {
                    num2++;
                }
            }
            while (bytes < numPtr)
            {
                char ch2;
                if ((!this.bigEndian && ((((int) bytes) & 3) == 0)) && ((lastByte == -1) && (lastChar == '\0')))
                {
                    ulong* numPtr4 = (ulong*) bytes;
                    while (numPtr4 < numPtr3)
                    {
                        if ((9223512776490647552L & numPtr4[0]) != 0L)
                        {
                            ulong num3 = (17870556004450629632L & numPtr4[0]) ^ 15564677810327967744L;
                            if (((((num3 & 18446462598732840960L) == 0L) || ((num3 & ((ulong) 0xffff00000000L)) == 0L)) || (((num3 & 0xffff0000L) == 0L) || ((num3 & ((ulong) 0xffffL)) == 0L))) && (((18158790778715962368L & numPtr4[0]) ^ 15852908186546788352L) != 0L))
                            {
                                break;
                            }
                        }
                        numPtr4++;
                    }
                    bytes = (byte*) numPtr4;
                    if (bytes >= numPtr)
                    {
                        break;
                    }
                }
                if (lastByte < 0)
                {
                    bytes++;
                    lastByte = bytes[0];
                    if (bytes >= numPtr)
                    {
                        break;
                    }
                }
                if (this.bigEndian)
                {
                    bytes++;
                    ch2 = (char) ((lastByte << 8) | bytes[0]);
                }
                else
                {
                    bytes++;
                    ch2 = (char) ((bytes[0] << 8) | lastByte);
                }
                lastByte = -1;
                if ((ch2 >= 0xd800) && (ch2 <= 0xdfff))
                {
                    if (ch2 <= 0xdbff)
                    {
                        if (lastChar > '\0')
                        {
                            num2--;
                            byte[] buffer2 = null;
                            if (this.bigEndian)
                            {
                                buffer2 = new byte[] { (byte) (lastChar >> 8), (byte) lastChar };
                            }
                            else
                            {
                                buffer2 = new byte[] { (byte) lastChar, (byte) (lastChar >> 8) };
                            }
                            if (fallbackBuffer == null)
                            {
                                if (decoder == null)
                                {
                                    fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                                }
                                else
                                {
                                    fallbackBuffer = decoder.FallbackBuffer;
                                }
                                fallbackBuffer.InternalInitialize(byteStart, null);
                            }
                            num2 += fallbackBuffer.InternalFallback(buffer2, bytes);
                        }
                        lastChar = ch2;
                    }
                    else if (lastChar == '\0')
                    {
                        num2--;
                        byte[] buffer3 = null;
                        if (this.bigEndian)
                        {
                            buffer3 = new byte[] { (byte) (ch2 >> 8), (byte) ch2 };
                        }
                        else
                        {
                            buffer3 = new byte[] { (byte) ch2, (byte) (ch2 >> 8) };
                        }
                        if (fallbackBuffer == null)
                        {
                            if (decoder == null)
                            {
                                fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                            }
                            else
                            {
                                fallbackBuffer = decoder.FallbackBuffer;
                            }
                            fallbackBuffer.InternalInitialize(byteStart, null);
                        }
                        num2 += fallbackBuffer.InternalFallback(buffer3, bytes);
                    }
                    else
                    {
                        lastChar = '\0';
                    }
                }
                else if (lastChar > '\0')
                {
                    num2--;
                    byte[] buffer4 = null;
                    if (this.bigEndian)
                    {
                        buffer4 = new byte[] { (byte) (lastChar >> 8), (byte) lastChar };
                    }
                    else
                    {
                        buffer4 = new byte[] { (byte) lastChar, (byte) (lastChar >> 8) };
                    }
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(byteStart, null);
                    }
                    num2 += fallbackBuffer.InternalFallback(buffer4, bytes);
                    lastChar = '\0';
                }
            }
            if ((decoder == null) || decoder.MustFlush)
            {
                if (lastChar > '\0')
                {
                    num2--;
                    byte[] buffer5 = null;
                    if (this.bigEndian)
                    {
                        buffer5 = new byte[] { (byte) (lastChar >> 8), (byte) lastChar };
                    }
                    else
                    {
                        buffer5 = new byte[] { (byte) lastChar, (byte) (lastChar >> 8) };
                    }
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(byteStart, null);
                    }
                    num2 += fallbackBuffer.InternalFallback(buffer5, bytes);
                    lastChar = '\0';
                }
                if (lastByte >= 0)
                {
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(byteStart, null);
                    }
                    num2 += fallbackBuffer.InternalFallback(new byte[] { (byte) lastByte }, bytes);
                    lastByte = -1;
                }
            }
            if (lastChar > '\0')
            {
                num2--;
            }
            return num2;
        }

        [ComVisible(false), SecurityCritical, CLSCompliant(false)]
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
            int lastByte = -1;
            char lastChar = '\0';
            if (decoder != null)
            {
                lastByte = decoder.lastByte;
                lastChar = decoder.lastChar;
            }
            DecoderFallbackBuffer fallbackBuffer = null;
            byte* numPtr = bytes + byteCount;
            char* charEnd = chars + charCount;
            byte* byteStart = bytes;
            char* chPtr2 = chars;
            while (bytes < numPtr)
            {
                if (((!this.bigEndian && ((((int) chars) & 3) == 0)) && (((((int) bytes) & 3) == 0) && (lastByte == -1))) && (lastChar == '\0'))
                {
                    ulong* numPtr3 = (ulong*) ((bytes - 7) + (((((long) ((numPtr - bytes) / 1)) >> 1) < ((long) ((charEnd - chars) / 2))) ? ((long) ((numPtr - bytes) / 1)) : (((long) ((charEnd - chars) / 2)) << 1)));
                    ulong* numPtr4 = (ulong*) bytes;
                    ulong* numPtr5 = (ulong*) chars;
                    while (numPtr4 < numPtr3)
                    {
                        if ((9223512776490647552L & numPtr4[0]) != 0L)
                        {
                            ulong num2 = (17870556004450629632L & numPtr4[0]) ^ 15564677810327967744L;
                            if (((((num2 & 18446462598732840960L) == 0L) || ((num2 & ((ulong) 0xffff00000000L)) == 0L)) || (((num2 & 0xffff0000L) == 0L) || ((num2 & ((ulong) 0xffffL)) == 0L))) && (((18158790778715962368L & numPtr4[0]) ^ 15852908186546788352L) != 0L))
                            {
                                break;
                            }
                        }
                        numPtr5[0] = numPtr4[0];
                        numPtr4++;
                        numPtr5++;
                    }
                    chars = (char*) numPtr5;
                    bytes = (byte*) numPtr4;
                    if (bytes >= numPtr)
                    {
                        break;
                    }
                }
                if (lastByte < 0)
                {
                    bytes++;
                    lastByte = bytes[0];
                }
                else
                {
                    char ch2;
                    if (this.bigEndian)
                    {
                        bytes++;
                        ch2 = (char) ((lastByte << 8) | bytes[0]);
                    }
                    else
                    {
                        bytes++;
                        ch2 = (char) ((bytes[0] << 8) | lastByte);
                    }
                    lastByte = -1;
                    if ((ch2 >= 0xd800) && (ch2 <= 0xdfff))
                    {
                        if (ch2 <= 0xdbff)
                        {
                            if (lastChar > '\0')
                            {
                                byte[] buffer2 = null;
                                if (this.bigEndian)
                                {
                                    buffer2 = new byte[] { (byte) (lastChar >> 8), (byte) lastChar };
                                }
                                else
                                {
                                    buffer2 = new byte[] { (byte) lastChar, (byte) (lastChar >> 8) };
                                }
                                if (fallbackBuffer == null)
                                {
                                    if (decoder == null)
                                    {
                                        fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                                    }
                                    else
                                    {
                                        fallbackBuffer = decoder.FallbackBuffer;
                                    }
                                    fallbackBuffer.InternalInitialize(byteStart, charEnd);
                                }
                                if (!fallbackBuffer.InternalFallback(buffer2, bytes, ref chars))
                                {
                                    bytes -= 2;
                                    fallbackBuffer.InternalReset();
                                    base.ThrowCharsOverflow(decoder, chars == chPtr2);
                                    break;
                                }
                            }
                            lastChar = ch2;
                            continue;
                        }
                        if (lastChar == '\0')
                        {
                            byte[] buffer3 = null;
                            if (this.bigEndian)
                            {
                                buffer3 = new byte[] { (byte) (ch2 >> 8), (byte) ch2 };
                            }
                            else
                            {
                                buffer3 = new byte[] { (byte) ch2, (byte) (ch2 >> 8) };
                            }
                            if (fallbackBuffer == null)
                            {
                                if (decoder == null)
                                {
                                    fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                                }
                                else
                                {
                                    fallbackBuffer = decoder.FallbackBuffer;
                                }
                                fallbackBuffer.InternalInitialize(byteStart, charEnd);
                            }
                            if (fallbackBuffer.InternalFallback(buffer3, bytes, ref chars))
                            {
                                continue;
                            }
                            bytes -= 2;
                            fallbackBuffer.InternalReset();
                            base.ThrowCharsOverflow(decoder, chars == chPtr2);
                            break;
                        }
                        if (chars >= (charEnd - 1))
                        {
                            bytes -= 2;
                            base.ThrowCharsOverflow(decoder, chars == chPtr2);
                            break;
                        }
                        chars++;
                        chars[0] = lastChar;
                        lastChar = '\0';
                    }
                    else if (lastChar > '\0')
                    {
                        byte[] buffer4 = null;
                        if (this.bigEndian)
                        {
                            buffer4 = new byte[] { (byte) (lastChar >> 8), (byte) lastChar };
                        }
                        else
                        {
                            buffer4 = new byte[] { (byte) lastChar, (byte) (lastChar >> 8) };
                        }
                        if (fallbackBuffer == null)
                        {
                            if (decoder == null)
                            {
                                fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                            }
                            else
                            {
                                fallbackBuffer = decoder.FallbackBuffer;
                            }
                            fallbackBuffer.InternalInitialize(byteStart, charEnd);
                        }
                        if (!fallbackBuffer.InternalFallback(buffer4, bytes, ref chars))
                        {
                            bytes -= 2;
                            fallbackBuffer.InternalReset();
                            base.ThrowCharsOverflow(decoder, chars == chPtr2);
                            break;
                        }
                        lastChar = '\0';
                    }
                    if (chars >= charEnd)
                    {
                        bytes -= 2;
                        base.ThrowCharsOverflow(decoder, chars == chPtr2);
                        break;
                    }
                    chars++;
                    chars[0] = ch2;
                }
            }
            if ((decoder == null) || decoder.MustFlush)
            {
                if (lastChar > '\0')
                {
                    byte[] buffer5 = null;
                    if (this.bigEndian)
                    {
                        buffer5 = new byte[] { (byte) (lastChar >> 8), (byte) lastChar };
                    }
                    else
                    {
                        buffer5 = new byte[] { (byte) lastChar, (byte) (lastChar >> 8) };
                    }
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(byteStart, charEnd);
                    }
                    if (!fallbackBuffer.InternalFallback(buffer5, bytes, ref chars))
                    {
                        bytes -= 2;
                        if (lastByte >= 0)
                        {
                            bytes--;
                        }
                        fallbackBuffer.InternalReset();
                        base.ThrowCharsOverflow(decoder, chars == chPtr2);
                        bytes += 2;
                        if (lastByte >= 0)
                        {
                            bytes++;
                        }
                        goto Label_04F5;
                    }
                    lastChar = '\0';
                }
                if (lastByte >= 0)
                {
                    if (fallbackBuffer == null)
                    {
                        if (decoder == null)
                        {
                            fallbackBuffer = base.decoderFallback.CreateFallbackBuffer();
                        }
                        else
                        {
                            fallbackBuffer = decoder.FallbackBuffer;
                        }
                        fallbackBuffer.InternalInitialize(byteStart, charEnd);
                    }
                    if (!fallbackBuffer.InternalFallback(new byte[] { (byte) lastByte }, bytes, ref chars))
                    {
                        bytes--;
                        fallbackBuffer.InternalReset();
                        base.ThrowCharsOverflow(decoder, chars == chPtr2);
                        bytes++;
                    }
                    else
                    {
                        lastByte = -1;
                    }
                }
            }
        Label_04F5:
            if (decoder != null)
            {
                decoder.m_bytesUsed = (int) ((long) ((bytes - byteStart) / 1));
                decoder.lastChar = lastChar;
                decoder.lastByte = lastByte;
            }
            return (int) ((long) ((chars - chPtr2) / 2));
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new Decoder(this);
        }

        [ComVisible(false)]
        public override System.Text.Encoder GetEncoder()
        {
            return new EncoderNLS(this);
        }

        public override int GetHashCode()
        {
            return ((((this.CodePage + base.EncoderFallback.GetHashCode()) + base.DecoderFallback.GetHashCode()) + (this.byteOrderMark ? 4 : 0)) + (this.bigEndian ? 8 : 0));
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
            num = num << 1;
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
            long num = ((byteCount >> 1) + (byteCount & 1)) + 1L;
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

        public override byte[] GetPreamble()
        {
            if (!this.byteOrderMark)
            {
                return Encoding.emptyByteArray;
            }
            if (this.bigEndian)
            {
                return new byte[] { 0xfe, 0xff };
            }
            return new byte[] { 0xff, 0xfe };
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

        [OnDeserializing]
        private void OnDeserializing(StreamingContext ctx)
        {
            this.isThrowException = false;
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
        private class Decoder : DecoderNLS, ISerializable
        {
            internal int lastByte;
            internal char lastChar;

            public Decoder(UnicodeEncoding encoding) : base(encoding)
            {
                this.lastByte = -1;
            }

            internal Decoder(SerializationInfo info, StreamingContext context)
            {
                this.lastByte = -1;
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                this.lastByte = (int) info.GetValue("lastByte", typeof(int));
                try
                {
                    base.m_encoding = (Encoding) info.GetValue("m_encoding", typeof(Encoding));
                    this.lastChar = (char) info.GetValue("lastChar", typeof(char));
                    base.m_fallback = (DecoderFallback) info.GetValue("m_fallback", typeof(DecoderFallback));
                }
                catch (SerializationException)
                {
                    bool bigEndian = (bool) info.GetValue("bigEndian", typeof(bool));
                    base.m_encoding = new UnicodeEncoding(bigEndian, false);
                }
            }

            public override void Reset()
            {
                this.lastByte = -1;
                this.lastChar = '\0';
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
                info.AddValue("m_encoding", base.m_encoding);
                info.AddValue("m_fallback", base.m_fallback);
                info.AddValue("lastChar", this.lastChar);
                info.AddValue("lastByte", this.lastByte);
                info.AddValue("bigEndian", ((UnicodeEncoding) base.m_encoding).bigEndian);
            }

            internal override bool HasState
            {
                get
                {
                    if (this.lastByte == -1)
                    {
                        return (this.lastChar != '\0');
                    }
                    return true;
                }
            }
        }
    }
}

