namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class UTF8Encoding : Encoding
    {
        private bool emitUTF8Identifier;
        private const int FinalByte = 0x20000000;
        private bool isThrowException;
        private const int SupplimentarySeq = 0x10000000;
        private const int ThreeByteSeq = 0x8000000;
        private const int UTF8_CODEPAGE = 0xfde9;

        public UTF8Encoding() : this(false)
        {
        }

        public UTF8Encoding(bool encoderShouldEmitUTF8Identifier) : this(encoderShouldEmitUTF8Identifier, false)
        {
        }

        public UTF8Encoding(bool encoderShouldEmitUTF8Identifier, bool throwOnInvalidBytes) : base(0xfde9)
        {
            this.emitUTF8Identifier = encoderShouldEmitUTF8Identifier;
            this.isThrowException = throwOnInvalidBytes;
            if (this.isThrowException)
            {
                this.SetDefaultFallbacks();
            }
        }

        public override bool Equals(object value)
        {
            UTF8Encoding encoding = value as UTF8Encoding;
            if (encoding == null)
            {
                return false;
            }
            return (((this.emitUTF8Identifier == encoding.emitUTF8Identifier) && base.EncoderFallback.Equals(encoding.EncoderFallback)) && base.DecoderFallback.Equals(encoding.DecoderFallback));
        }

        [SecurityCritical]
        private unsafe int FallbackInvalidByteSequence(byte* pSrc, int ch, DecoderFallbackBuffer fallback)
        {
            byte[] bytesUnknown = this.GetBytesUnknown(ref pSrc, ch);
            return fallback.InternalFallback(bytesUnknown, pSrc);
        }

        [SecurityCritical]
        private unsafe bool FallbackInvalidByteSequence(ref byte* pSrc, int ch, DecoderFallbackBuffer fallback, ref char* pTarget)
        {
            byte* numPtr = pSrc;
            byte[] bytesUnknown = this.GetBytesUnknown(ref numPtr, ch);
            if (!fallback.InternalFallback(bytesUnknown, pSrc, ref pTarget))
            {
                pSrc = numPtr;
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        public override unsafe int GetByteCount(string chars)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("s");
            }
            fixed (char* str = ((char*) chars))
            {
                char* chPtr = str;
                return this.GetByteCount(chPtr, chars.Length, null);
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
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS baseEncoder)
        {
            EncoderFallbackBuffer fallbackBuffer = null;
            char* chPtr = chars;
            char* charEnd = chPtr + count;
            int num = count;
            int ch = 0;
            if (baseEncoder != null)
            {
                UTF8Encoder encoder = (UTF8Encoder) baseEncoder;
                ch = encoder.surrogateChar;
                if (encoder.InternalHasFallbackBuffer)
                {
                    if ((fallbackBuffer = encoder.FallbackBuffer).Remaining > 0)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                    }
                    fallbackBuffer.InternalInitialize(chars, charEnd, encoder, false);
                }
            }
        Label_007D:
            if (chPtr < charEnd)
            {
                if (ch > 0)
                {
                    int num3 = chPtr[0];
                    num++;
                    if (InRange(num3, 0xdc00, 0xdfff))
                    {
                        ch = 0xfffd;
                        chPtr++;
                    }
                    goto Label_0171;
                }
                if (fallbackBuffer != null)
                {
                    ch = fallbackBuffer.InternalGetNextChar();
                    if (ch > 0)
                    {
                        num++;
                        goto Label_0155;
                    }
                }
                ch = chPtr[0];
                chPtr++;
                goto Label_0155;
            }
            if (ch == 0)
            {
                ch = (fallbackBuffer != null) ? fallbackBuffer.InternalGetNextChar() : 0;
                if (ch <= 0)
                {
                    goto Label_00EC;
                }
                num++;
                goto Label_0155;
            }
            if ((fallbackBuffer != null) && fallbackBuffer.bFallingBack)
            {
                ch = fallbackBuffer.InternalGetNextChar();
                num++;
                if (!InRange(ch, 0xdc00, 0xdfff))
                {
                    if (ch <= 0)
                    {
                        num--;
                        return num;
                    }
                    goto Label_0155;
                }
                ch = 0xfffd;
                num++;
                goto Label_0171;
            }
        Label_00EC:
            if ((ch <= 0) || ((baseEncoder != null) && !baseEncoder.MustFlush))
            {
                return num;
            }
            num++;
            goto Label_0171;
        Label_0155:
            if (InRange(ch, 0xd800, 0xdbff))
            {
                num--;
                goto Label_007D;
            }
        Label_0171:
            if (InRange(ch, 0xd800, 0xdfff))
            {
                if (fallbackBuffer == null)
                {
                    if (baseEncoder == null)
                    {
                        fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
                    }
                    else
                    {
                        fallbackBuffer = baseEncoder.FallbackBuffer;
                    }
                    fallbackBuffer.InternalInitialize(chars, chars + count, baseEncoder, false);
                }
                fallbackBuffer.InternalFallback((char) ch, ref chPtr);
                num--;
                ch = 0;
            }
            else
            {
                if (ch > 0x7f)
                {
                    if (ch > 0x7ff)
                    {
                        num++;
                    }
                    num++;
                }
                if ((fallbackBuffer != null) && ((ch = fallbackBuffer.InternalGetNextChar()) != '\0'))
                {
                    num++;
                    goto Label_0155;
                }
                int num4 = PtrDiff(charEnd, chPtr);
                if (num4 <= 13)
                {
                    char* chPtr3 = charEnd;
                    while (chPtr < chPtr3)
                    {
                        ch = chPtr[0];
                        chPtr++;
                        if (ch > 0x7f)
                        {
                            goto Label_0155;
                        }
                    }
                    return num;
                }
                char* chPtr4 = (chPtr + num4) - 7;
                while (chPtr < chPtr4)
                {
                    ch = chPtr[0];
                    chPtr++;
                    if (ch > 0x7f)
                    {
                        if (ch > 0x7ff)
                        {
                            if ((ch & 0xf800) == 0xd800)
                            {
                                goto Label_038C;
                            }
                            num++;
                        }
                        num++;
                    }
                    if ((((int) chPtr) & 2) != 0)
                    {
                        ch = chPtr[0];
                        chPtr++;
                        if (ch > 0x7f)
                        {
                            if (ch > 0x7ff)
                            {
                                if ((ch & 0xf800) == 0xd800)
                                {
                                    goto Label_038C;
                                }
                                num++;
                            }
                            num++;
                        }
                    }
                    while (chPtr < chPtr4)
                    {
                        ch = *((int*) chPtr);
                        int num5 = *((int*) (chPtr + 2));
                        if (((ch | num5) & -8323200) != 0)
                        {
                            if (((ch | num5) & -134154240) != 0)
                            {
                                goto Label_037C;
                            }
                            if ((ch & -8388608) != 0)
                            {
                                num++;
                            }
                            if ((ch & 0xff80) != 0)
                            {
                                num++;
                            }
                            if ((num5 & -8388608) != 0)
                            {
                                num++;
                            }
                            if ((num5 & 0xff80) != 0)
                            {
                                num++;
                            }
                        }
                        chPtr += 4;
                        ch = *((int*) chPtr);
                        num5 = *((int*) (chPtr + 2));
                        if (((ch | num5) & -8323200) != 0)
                        {
                            if (((ch | num5) & -134154240) != 0)
                            {
                                goto Label_037C;
                            }
                            if ((ch & -8388608) != 0)
                            {
                                num++;
                            }
                            if ((ch & 0xff80) != 0)
                            {
                                num++;
                            }
                            if ((num5 & -8388608) != 0)
                            {
                                num++;
                            }
                            if ((num5 & 0xff80) != 0)
                            {
                                num++;
                            }
                        }
                        chPtr += 4;
                    }
                    break;
                Label_037C:
                    ch = (ushort) ch;
                    chPtr++;
                    if (ch <= 0x7f)
                    {
                        continue;
                    }
                Label_038C:
                    if (ch > 0x7ff)
                    {
                        if (InRange(ch, 0xd800, 0xdfff))
                        {
                            int num6 = chPtr[0];
                            if ((ch > 0xdbff) || !InRange(num6, 0xdc00, 0xdfff))
                            {
                                chPtr--;
                                break;
                            }
                            chPtr++;
                        }
                        num++;
                    }
                    num++;
                }
                ch = 0;
            }
            goto Label_007D;
        }

        [ComVisible(false), CLSCompliant(false), SecurityCritical]
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
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS baseEncoder)
        {
            UTF8Encoder encoder = null;
            EncoderFallbackBuffer fallbackBuffer = null;
            char* chPtr = chars;
            byte* b = bytes;
            char* charEnd = chPtr + charCount;
            byte* a = b + byteCount;
            int ch = 0;
            if (baseEncoder != null)
            {
                encoder = (UTF8Encoder) baseEncoder;
                ch = encoder.surrogateChar;
                if (encoder.InternalHasFallbackBuffer)
                {
                    fallbackBuffer = encoder.FallbackBuffer;
                    if ((fallbackBuffer.Remaining > 0) && encoder.m_throwOnOverflow)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                    }
                    fallbackBuffer.InternalInitialize(chars, charEnd, encoder, true);
                }
            }
        Label_008B:
            if (chPtr < charEnd)
            {
                if (ch > 0)
                {
                    int num3 = chPtr[0];
                    if (InRange(num3, 0xdc00, 0xdfff))
                    {
                        ch = (num3 + (ch << 10)) + -56613888;
                        chPtr++;
                    }
                    goto Label_016F;
                }
                if (fallbackBuffer != null)
                {
                    ch = fallbackBuffer.InternalGetNextChar();
                    if (ch > 0)
                    {
                        goto Label_0159;
                    }
                }
                ch = chPtr[0];
                chPtr++;
                goto Label_0159;
            }
            if (ch == 0)
            {
                ch = (fallbackBuffer != null) ? fallbackBuffer.InternalGetNextChar() : 0;
                if (ch <= 0)
                {
                    goto Label_00F5;
                }
                goto Label_0159;
            }
            if ((fallbackBuffer != null) && fallbackBuffer.bFallingBack)
            {
                int num2 = ch;
                ch = fallbackBuffer.InternalGetNextChar();
                if (InRange(ch, 0xdc00, 0xdfff))
                {
                    ch = (ch + (num2 << 10)) + -56613888;
                    goto Label_016F;
                }
                if (ch <= 0)
                {
                    goto Label_04D9;
                }
                goto Label_0159;
            }
        Label_00F5:
            if ((ch <= 0) || ((encoder != null) && !encoder.MustFlush))
            {
                goto Label_04D9;
            }
            goto Label_016F;
        Label_0159:
            if (InRange(ch, 0xd800, 0xdbff))
            {
                goto Label_008B;
            }
        Label_016F:
            if (InRange(ch, 0xd800, 0xdfff))
            {
                if (fallbackBuffer == null)
                {
                    if (baseEncoder == null)
                    {
                        fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
                    }
                    else
                    {
                        fallbackBuffer = baseEncoder.FallbackBuffer;
                    }
                    fallbackBuffer.InternalInitialize(chars, charEnd, baseEncoder, true);
                }
                fallbackBuffer.InternalFallback((char) ch, ref chPtr);
                ch = 0;
                goto Label_008B;
            }
            int num4 = 1;
            if (ch > 0x7f)
            {
                if (ch > 0x7ff)
                {
                    if (ch > 0xffff)
                    {
                        num4++;
                    }
                    num4++;
                }
                num4++;
            }
            if (b > (a - num4))
            {
                if ((fallbackBuffer != null) && fallbackBuffer.bFallingBack)
                {
                    fallbackBuffer.MovePrevious();
                    if (ch > 0xffff)
                    {
                        fallbackBuffer.MovePrevious();
                    }
                }
                else
                {
                    chPtr--;
                    if (ch > 0xffff)
                    {
                        chPtr--;
                    }
                }
                base.ThrowBytesOverflow(encoder, b == bytes);
                ch = 0;
            }
            else
            {
                if (ch <= 0x7f)
                {
                    b[0] = (byte) ch;
                }
                else
                {
                    int num5;
                    if (ch <= 0x7ff)
                    {
                        num5 = (byte) (-64 | (ch >> 6));
                    }
                    else
                    {
                        if (ch <= 0xffff)
                        {
                            num5 = (byte) (-32 | (ch >> 12));
                        }
                        else
                        {
                            b[0] = (byte) (-16 | (ch >> 0x12));
                            b++;
                            num5 = -128 | ((ch >> 12) & 0x3f);
                        }
                        b[0] = (byte) num5;
                        b++;
                        num5 = -128 | ((ch >> 6) & 0x3f);
                    }
                    b[0] = (byte) num5;
                    b++;
                    b[0] = (byte) (-128 | (ch & 0x3f));
                }
                b++;
                if ((fallbackBuffer != null) && ((ch = fallbackBuffer.InternalGetNextChar()) != '\0'))
                {
                    goto Label_0159;
                }
                int num6 = PtrDiff(charEnd, chPtr);
                int num7 = PtrDiff(a, b);
                if (num6 <= 13)
                {
                    if (num7 < num6)
                    {
                        ch = 0;
                        goto Label_008B;
                    }
                    char* chPtr3 = charEnd;
                    while (chPtr < chPtr3)
                    {
                        ch = chPtr[0];
                        chPtr++;
                        if (ch > 0x7f)
                        {
                            goto Label_0159;
                        }
                        b[0] = (byte) ch;
                        b++;
                    }
                    ch = 0;
                }
                else
                {
                    if (num7 < num6)
                    {
                        num6 = num7;
                    }
                    char* chPtr4 = (chPtr + num6) - 5;
                    while (chPtr < chPtr4)
                    {
                        int num9;
                        ch = chPtr[0];
                        chPtr++;
                        if (ch > 0x7f)
                        {
                            goto Label_03F2;
                        }
                        b[0] = (byte) ch;
                        b++;
                        if ((((int) chPtr) & 2) != 0)
                        {
                            ch = chPtr[0];
                            chPtr++;
                            if (ch > 0x7f)
                            {
                                goto Label_03F2;
                            }
                            b[0] = (byte) ch;
                            b++;
                        }
                        while (chPtr < chPtr4)
                        {
                            ch = *((int*) chPtr);
                            int num8 = *((int*) (chPtr + 2));
                            if (((ch | num8) & -8323200) != 0)
                            {
                                goto Label_03D3;
                            }
                            b[0] = (byte) ch;
                            b[1] = (byte) (ch >> 0x10);
                            chPtr += 4;
                            b[2] = (byte) num8;
                            b[3] = (byte) (num8 >> 0x10);
                            b += 4;
                        }
                        continue;
                    Label_03D3:
                        ch = (ushort) ch;
                        chPtr++;
                        if (ch <= 0x7f)
                        {
                            b[0] = (byte) ch;
                            b++;
                            continue;
                        }
                    Label_03F2:
                        if (ch <= 0x7ff)
                        {
                            num9 = -64 | (ch >> 6);
                        }
                        else
                        {
                            if (!InRange(ch, 0xd800, 0xdfff))
                            {
                                num9 = -32 | (ch >> 12);
                            }
                            else
                            {
                                if (ch > 0xdbff)
                                {
                                    chPtr--;
                                    break;
                                }
                                num9 = chPtr[0];
                                chPtr++;
                                if (!InRange(num9, 0xdc00, 0xdfff))
                                {
                                    chPtr -= 2;
                                    break;
                                }
                                ch = (num9 + (ch << 10)) + -56613888;
                                b[0] = (byte) (-16 | (ch >> 0x12));
                                b++;
                                num9 = -128 | ((ch >> 12) & 0x3f);
                            }
                            b[0] = (byte) num9;
                            chPtr4--;
                            b++;
                            num9 = -128 | ((ch >> 6) & 0x3f);
                        }
                        b[0] = (byte) num9;
                        chPtr4--;
                        b++;
                        b[0] = (byte) (-128 | (ch & 0x3f));
                        b++;
                    }
                    ch = 0;
                    goto Label_008B;
                }
            }
        Label_04D9:
            if (encoder != null)
            {
                encoder.surrogateChar = ch;
                encoder.m_charsUsed = (int) ((long) ((chPtr - chars) / 2));
            }
            return (int) ((long) ((b - bytes) / 1));
        }

        [SecurityCritical]
        private unsafe byte[] GetBytesUnknown(ref byte* pSrc, int ch)
        {
            if ((ch < 0x100) && (ch >= 0))
            {
                pSrc -= (IntPtr) 1;
                return new byte[] { ((byte) ch) };
            }
            if ((ch & 0x18000000) == 0)
            {
                pSrc -= (IntPtr) 1;
                return new byte[] { ((byte) ((ch & 0x1f) | 0xc0)) };
            }
            if ((ch & 0x10000000) != 0)
            {
                if ((ch & 0x800000) != 0)
                {
                    pSrc -= (IntPtr) 3;
                    return new byte[] { ((byte) (((ch >> 12) & 7) | 240)), ((byte) (((ch >> 6) & 0x3f) | 0x80)), ((byte) ((ch & 0x3f) | 0x80)) };
                }
                if ((ch & 0x20000) != 0)
                {
                    pSrc -= (IntPtr) 2;
                    return new byte[] { ((byte) (((ch >> 6) & 7) | 240)), ((byte) ((ch & 0x3f) | 0x80)) };
                }
                pSrc -= (IntPtr) 1;
                return new byte[] { ((byte) ((ch & 7) | 240)) };
            }
            if ((ch & 0x800000) != 0)
            {
                pSrc -= (IntPtr) 2;
                return new byte[] { ((byte) (((ch >> 6) & 15) | 0xe0)), ((byte) ((ch & 0x3f) | 0x80)) };
            }
            pSrc -= (IntPtr) 1;
            return new byte[] { ((byte) ((ch & 15) | 0xe0)) };
        }

        [ComVisible(false), SecurityCritical, CLSCompliant(false)]
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
            int num4;
            byte* pSrc = bytes;
            byte* a = pSrc + count;
            int num = count;
            int ch = 0;
            DecoderFallbackBuffer fallback = null;
            if (baseDecoder != null)
            {
                UTF8Decoder decoder = (UTF8Decoder) baseDecoder;
                ch = decoder.bits;
                num -= ch >> 30;
            }
        Label_0027:
            if (pSrc >= a)
            {
                goto Label_0336;
            }
            if (ch == 0)
            {
                ch = pSrc[0];
                pSrc++;
                goto Label_010D;
            }
            int num3 = pSrc[0];
            pSrc++;
            if ((num3 & -64) != 0x80)
            {
                pSrc--;
                num += ch >> 30;
            }
            else
            {
                ch = (ch << 6) | (num3 & 0x3f);
                if ((ch & 0x20000000) == 0)
                {
                    if ((ch & 0x10000000) != 0)
                    {
                        if (((ch & 0x800000) != 0) || InRange(ch & 0x1f0, 0x10, 0x100))
                        {
                            goto Label_0027;
                        }
                    }
                    else if (((ch & 0x3e0) != 0) && ((ch & 0x3e0) != 0x360))
                    {
                        goto Label_0027;
                    }
                }
                else
                {
                    if ((ch & 0x101f0000) == 0x10000000)
                    {
                        num--;
                    }
                    goto Label_0183;
                }
            }
        Label_00C9:
            if (fallback == null)
            {
                if (baseDecoder == null)
                {
                    fallback = base.decoderFallback.CreateFallbackBuffer();
                }
                else
                {
                    fallback = baseDecoder.FallbackBuffer;
                }
                fallback.InternalInitialize(bytes, null);
            }
            num += this.FallbackInvalidByteSequence(pSrc, ch, fallback);
            ch = 0;
            goto Label_0027;
        Label_010D:
            if (ch > 0x7f)
            {
                num--;
                if ((ch & 0x40) == 0)
                {
                    goto Label_00C9;
                }
                if ((ch & 0x20) != 0)
                {
                    if ((ch & 0x10) != 0)
                    {
                        ch &= 15;
                        if (ch > 4)
                        {
                            ch |= 240;
                            goto Label_00C9;
                        }
                        ch |= 0x504d0c00;
                        num--;
                    }
                    else
                    {
                        ch = (ch & 15) | 0x48228000;
                        num--;
                    }
                }
                else
                {
                    ch &= 0x1f;
                    if (ch <= 1)
                    {
                        ch |= 0xc0;
                        goto Label_00C9;
                    }
                    ch |= 0x800000;
                }
                goto Label_0027;
            }
        Label_0183:
            num4 = PtrDiff(a, pSrc);
            if (num4 <= 13)
            {
                byte* numPtr3 = a;
                while (pSrc < numPtr3)
                {
                    ch = pSrc[0];
                    pSrc++;
                    if (ch > 0x7f)
                    {
                        goto Label_010D;
                    }
                }
                ch = 0;
                goto Label_0336;
            }
            byte* numPtr4 = (pSrc + num4) - 7;
            while (pSrc < numPtr4)
            {
                int num6;
                ch = pSrc[0];
                pSrc++;
                if (ch > 0x7f)
                {
                    goto Label_025A;
                }
                if ((((int) pSrc) & 1) != 0)
                {
                    ch = pSrc[0];
                    pSrc++;
                    if (ch > 0x7f)
                    {
                        goto Label_025A;
                    }
                }
                if ((((int) pSrc) & 2) != 0)
                {
                    ch = *((ushort*) pSrc);
                    if ((ch & 0x8080) != 0)
                    {
                        goto Label_0245;
                    }
                    pSrc += 2;
                }
                while (pSrc < numPtr4)
                {
                    ch = *((int*) pSrc);
                    int num5 = *((int*) (pSrc + 4));
                    if (((ch | num5) & -2139062144) != 0)
                    {
                        goto Label_0245;
                    }
                    pSrc += 8;
                    if (pSrc >= numPtr4)
                    {
                        break;
                    }
                    ch = *((int*) pSrc);
                    num5 = *((int*) (pSrc + 4));
                    if (((ch | num5) & -2139062144) != 0)
                    {
                        goto Label_0245;
                    }
                    pSrc += 8;
                }
                break;
            Label_0245:
                ch &= 0xff;
                pSrc++;
                if (ch <= 0x7f)
                {
                    continue;
                }
            Label_025A:
                num6 = pSrc[0];
                pSrc++;
                if (((ch & 0x40) == 0) || ((num6 & -64) != 0x80))
                {
                    goto Label_032A;
                }
                num6 &= 0x3f;
                if ((ch & 0x20) != 0)
                {
                    num6 |= (ch & 15) << 6;
                    if ((ch & 0x10) != 0)
                    {
                        ch = pSrc[0];
                        if (!InRange(num6 >> 4, 1, 0x10) || ((ch & -64) != 0x80))
                        {
                            goto Label_032A;
                        }
                        num6 = (num6 << 6) | (ch & 0x3f);
                        ch = pSrc[1];
                        if ((ch & -64) != 0x80)
                        {
                            goto Label_032A;
                        }
                        pSrc += 2;
                        num--;
                    }
                    else
                    {
                        ch = pSrc[0];
                        if ((((num6 & 0x3e0) == 0) || ((num6 & 0x3e0) == 0x360)) || ((ch & -64) != 0x80))
                        {
                            goto Label_032A;
                        }
                        pSrc++;
                        num--;
                    }
                }
                else if ((ch & 30) == 0)
                {
                    goto Label_032A;
                }
                num--;
            }
            ch = 0;
            goto Label_0027;
        Label_032A:
            pSrc -= 2;
            ch = 0;
            goto Label_0027;
        Label_0336:
            if (ch == 0)
            {
                return num;
            }
            num += ch >> 30;
            if ((baseDecoder != null) && !baseDecoder.MustFlush)
            {
                return num;
            }
            if (fallback == null)
            {
                if (baseDecoder == null)
                {
                    fallback = base.decoderFallback.CreateFallbackBuffer();
                }
                else
                {
                    fallback = baseDecoder.FallbackBuffer;
                }
                fallback.InternalInitialize(bytes, null);
            }
            return (num + this.FallbackInvalidByteSequence(pSrc, ch, fallback));
        }

        [CLSCompliant(false), SecurityCritical, ComVisible(false)]
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
            byte* pSrc = bytes;
            char* pTarget = chars;
            byte* a = pSrc + byteCount;
            char* charEnd = pTarget + charCount;
            int ch = 0;
            DecoderFallbackBuffer fallback = null;
            if (baseDecoder != null)
            {
                UTF8Decoder decoder = (UTF8Decoder) baseDecoder;
                ch = decoder.bits;
            }
        Label_002C:
            if (pSrc >= a)
            {
                goto Label_055F;
            }
            if (ch == 0)
            {
                ch = pSrc[0];
                pSrc++;
                goto Label_0165;
            }
            int num2 = pSrc[0];
            pSrc++;
            if ((num2 & -64) != 0x80)
            {
                pSrc--;
            }
            else
            {
                ch = (ch << 6) | (num2 & 0x3f);
                if ((ch & 0x20000000) == 0)
                {
                    if ((ch & 0x10000000) != 0)
                    {
                        if (((ch & 0x800000) != 0) || InRange(ch & 0x1f0, 0x10, 0x100))
                        {
                            goto Label_002C;
                        }
                    }
                    else if (((ch & 0x3e0) != 0) && ((ch & 0x3e0) != 0x360))
                    {
                        goto Label_002C;
                    }
                }
                else
                {
                    if (((ch & 0x101f0000) > 0x10000000) && (pTarget < charEnd))
                    {
                        pTarget[0] = (char) (((ch >> 10) & 0x7ff) + -10304);
                        pTarget++;
                        ch = (ch & 0x3ff) + 0xdc00;
                    }
                    goto Label_01E6;
                }
            }
        Label_0100:
            if (fallback == null)
            {
                if (baseDecoder == null)
                {
                    fallback = base.decoderFallback.CreateFallbackBuffer();
                }
                else
                {
                    fallback = baseDecoder.FallbackBuffer;
                }
                fallback.InternalInitialize(bytes, charEnd);
            }
            if (!this.FallbackInvalidByteSequence(ref pSrc, ch, fallback, ref pTarget))
            {
                fallback.InternalReset();
                base.ThrowCharsOverflow(baseDecoder, pTarget == chars);
                ch = 0;
                goto Label_055F;
            }
            ch = 0;
            goto Label_002C;
        Label_0165:
            if (ch > 0x7f)
            {
                if ((ch & 0x40) == 0)
                {
                    goto Label_0100;
                }
                if ((ch & 0x20) != 0)
                {
                    if ((ch & 0x10) != 0)
                    {
                        ch &= 15;
                        if (ch > 4)
                        {
                            ch |= 240;
                            goto Label_0100;
                        }
                        ch |= 0x504d0c00;
                    }
                    else
                    {
                        ch = (ch & 15) | 0x48228000;
                    }
                }
                else
                {
                    ch &= 0x1f;
                    if (ch <= 1)
                    {
                        ch |= 0xc0;
                        goto Label_0100;
                    }
                    ch |= 0x800000;
                }
                goto Label_002C;
            }
        Label_01E6:
            if (pTarget >= charEnd)
            {
                ch &= 0x1fffff;
                if (ch > 0x7f)
                {
                    if (ch > 0x7ff)
                    {
                        if ((ch >= 0xdc00) && (ch <= 0xdfff))
                        {
                            pSrc--;
                            pTarget--;
                        }
                        else if (ch > 0xffff)
                        {
                            pSrc--;
                        }
                        pSrc--;
                    }
                    pSrc--;
                }
                pSrc--;
                base.ThrowCharsOverflow(baseDecoder, pTarget == chars);
                ch = 0;
                goto Label_055F;
            }
            pTarget[0] = (char) ch;
            pTarget++;
            int num3 = PtrDiff(charEnd, pTarget);
            int num4 = PtrDiff(a, pSrc);
            if (num4 <= 13)
            {
                if (num3 < num4)
                {
                    ch = 0;
                    goto Label_002C;
                }
                byte* numPtr3 = a;
                while (pSrc < numPtr3)
                {
                    ch = pSrc[0];
                    pSrc++;
                    if (ch > 0x7f)
                    {
                        goto Label_0165;
                    }
                    pTarget[0] = (char) ch;
                    pTarget++;
                }
                ch = 0;
                goto Label_055F;
            }
            if (num3 < num4)
            {
                num4 = num3;
            }
            char* chPtr3 = (pTarget + num4) - 7;
            while (pTarget < chPtr3)
            {
                int num6;
                ch = pSrc[0];
                pSrc++;
                if (ch > 0x7f)
                {
                    goto Label_0407;
                }
                pTarget[0] = (char) ch;
                pTarget++;
                if ((((int) pSrc) & 1) != 0)
                {
                    ch = pSrc[0];
                    pSrc++;
                    if (ch > 0x7f)
                    {
                        goto Label_0407;
                    }
                    pTarget[0] = (char) ch;
                    pTarget++;
                }
                if ((((int) pSrc) & 2) != 0)
                {
                    ch = *((ushort*) pSrc);
                    if ((ch & 0x8080) != 0)
                    {
                        goto Label_03E3;
                    }
                    pTarget[0] = (char) (ch & 0x7f);
                    pSrc += 2;
                    pTarget[1] = (char) ((ch >> 8) & 0x7f);
                    pTarget += 2;
                }
                while (pTarget < chPtr3)
                {
                    ch = *((int*) pSrc);
                    int num5 = *((int*) (pSrc + 4));
                    if (((ch | num5) & -2139062144) != 0)
                    {
                        goto Label_03E3;
                    }
                    pTarget[0] = (char) (ch & 0x7f);
                    pTarget[1] = (char) ((ch >> 8) & 0x7f);
                    pTarget[2] = (char) ((ch >> 0x10) & 0x7f);
                    pTarget[3] = (char) ((ch >> 0x18) & 0x7f);
                    pSrc += 8;
                    pTarget[4] = (char) (num5 & 0x7f);
                    pTarget[5] = (char) ((num5 >> 8) & 0x7f);
                    pTarget[6] = (char) ((num5 >> 0x10) & 0x7f);
                    pTarget[7] = (char) ((num5 >> 0x18) & 0x7f);
                    pTarget += 8;
                }
                break;
            Label_03E3:
                ch &= 0xff;
                pSrc++;
                if (ch <= 0x7f)
                {
                    pTarget[0] = (char) ch;
                    pTarget++;
                    continue;
                }
            Label_0407:
                num6 = pSrc[0];
                pSrc++;
                if (((ch & 0x40) == 0) || ((num6 & -64) != 0x80))
                {
                    goto Label_0552;
                }
                num6 &= 0x3f;
                if ((ch & 0x20) != 0)
                {
                    num6 |= (ch & 15) << 6;
                    if ((ch & 0x10) != 0)
                    {
                        ch = pSrc[0];
                        if (!InRange(num6 >> 4, 1, 0x10) || ((ch & -64) != 0x80))
                        {
                            goto Label_0552;
                        }
                        num6 = (num6 << 6) | (ch & 0x3f);
                        ch = pSrc[1];
                        if ((ch & -64) != 0x80)
                        {
                            goto Label_0552;
                        }
                        pSrc += 2;
                        ch = (num6 << 6) | (ch & 0x3f);
                        pTarget[0] = (char) (((ch >> 10) & 0x7ff) + -10304);
                        pTarget++;
                        ch = (ch & 0x3ff) + -9216;
                        chPtr3--;
                    }
                    else
                    {
                        ch = pSrc[0];
                        if ((((num6 & 0x3e0) == 0) || ((num6 & 0x3e0) == 0x360)) || ((ch & -64) != 0x80))
                        {
                            goto Label_0552;
                        }
                        pSrc++;
                        ch = (num6 << 6) | (ch & 0x3f);
                        chPtr3--;
                    }
                }
                else
                {
                    ch &= 0x1f;
                    if (ch <= 1)
                    {
                        goto Label_0552;
                    }
                    ch = (ch << 6) | num6;
                }
                pTarget[0] = (char) ch;
                pTarget++;
                chPtr3--;
            }
            ch = 0;
            goto Label_002C;
        Label_0552:
            pSrc -= 2;
            ch = 0;
            goto Label_002C;
        Label_055F:
            if ((ch != 0) && ((baseDecoder == null) || baseDecoder.MustFlush))
            {
                if (fallback == null)
                {
                    if (baseDecoder == null)
                    {
                        fallback = base.decoderFallback.CreateFallbackBuffer();
                    }
                    else
                    {
                        fallback = baseDecoder.FallbackBuffer;
                    }
                    fallback.InternalInitialize(bytes, charEnd);
                }
                if (!this.FallbackInvalidByteSequence(ref pSrc, ch, fallback, ref pTarget))
                {
                    fallback.InternalReset();
                    base.ThrowCharsOverflow(baseDecoder, pTarget == chars);
                }
                ch = 0;
            }
            if (baseDecoder != null)
            {
                UTF8Decoder decoder2 = (UTF8Decoder) baseDecoder;
                decoder2.bits = ch;
                baseDecoder.m_bytesUsed = (int) ((long) ((pSrc - bytes) / 1));
            }
            return PtrDiff(pTarget, chars);
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new UTF8Decoder(this);
        }

        public override System.Text.Encoder GetEncoder()
        {
            return new UTF8Encoder(this);
        }

        public override int GetHashCode()
        {
            return (((base.EncoderFallback.GetHashCode() + base.DecoderFallback.GetHashCode()) + 0xfde9) + (this.emitUTF8Identifier ? 1 : 0));
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
            num *= 3L;
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
            long num = byteCount + 1L;
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
            if (this.emitUTF8Identifier)
            {
                return new byte[] { 0xef, 0xbb, 0xbf };
            }
            return Encoding.emptyByteArray;
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

        private static bool InRange(int ch, int start, int end)
        {
            return ((ch - start) <= (end - start));
        }

        [SecurityCritical]
        private static unsafe int PtrDiff(byte* a, byte* b)
        {
            return (int) ((long) ((a - b) / 1));
        }

        [SecurityCritical]
        private static unsafe int PtrDiff(char* a, char* b)
        {
            return (int) (((uint) ((long) ((a - b) / 1))) >> 1);
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
        internal class UTF8Decoder : DecoderNLS, ISerializable
        {
            internal int bits;

            public UTF8Decoder(UTF8Encoding encoding) : base(encoding)
            {
            }

            internal UTF8Decoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                base.m_encoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
                try
                {
                    this.bits = (int) info.GetValue("wbits", typeof(int));
                    base.m_fallback = (DecoderFallback) info.GetValue("m_fallback", typeof(DecoderFallback));
                }
                catch (SerializationException)
                {
                    this.bits = 0;
                    base.m_fallback = null;
                }
            }

            public override void Reset()
            {
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
                info.AddValue("wbits", this.bits);
                info.AddValue("m_fallback", base.m_fallback);
                info.AddValue("bits", 0);
                info.AddValue("trailCount", 0);
                info.AddValue("isSurrogate", false);
                info.AddValue("byteSequence", 0);
            }

            internal override bool HasState
            {
                get
                {
                    return (this.bits != 0);
                }
            }
        }

        [Serializable]
        internal class UTF8Encoder : EncoderNLS, ISerializable
        {
            internal int surrogateChar;

            public UTF8Encoder(UTF8Encoding encoding) : base(encoding)
            {
            }

            internal UTF8Encoder(SerializationInfo info, StreamingContext context)
            {
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                base.m_encoding = (Encoding) info.GetValue("encoding", typeof(Encoding));
                this.surrogateChar = (int) info.GetValue("surrogateChar", typeof(int));
                try
                {
                    base.m_fallback = (EncoderFallback) info.GetValue("m_fallback", typeof(EncoderFallback));
                }
                catch (SerializationException)
                {
                    base.m_fallback = null;
                }
            }

            public override void Reset()
            {
                this.surrogateChar = 0;
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
                info.AddValue("surrogateChar", this.surrogateChar);
                info.AddValue("m_fallback", base.m_fallback);
                info.AddValue("storedSurrogate", this.surrogateChar > 0);
                info.AddValue("mustFlush", false);
            }

            internal override bool HasState
            {
                get
                {
                    return (this.surrogateChar != 0);
                }
            }
        }
    }
}

