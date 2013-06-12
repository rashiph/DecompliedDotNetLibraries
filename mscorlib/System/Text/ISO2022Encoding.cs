namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class ISO2022Encoding : DBCSCodePageEncoding
    {
        private const byte ESCAPE = 0x1b;
        private static ushort[] HalfToFullWidthKanaTable = new ushort[] { 
            0xa1a3, 0xa1d6, 0xa1d7, 0xa1a2, 0xa1a6, 0xa5f2, 0xa5a1, 0xa5a3, 0xa5a5, 0xa5a7, 0xa5a9, 0xa5e3, 0xa5e5, 0xa5e7, 0xa5c3, 0xa1bc, 
            0xa5a2, 0xa5a4, 0xa5a6, 0xa5a8, 0xa5aa, 0xa5ab, 0xa5ad, 0xa5af, 0xa5b1, 0xa5b3, 0xa5b5, 0xa5b7, 0xa5b9, 0xa5bb, 0xa5bd, 0xa5bf, 
            0xa5c1, 0xa5c4, 0xa5c6, 0xa5c8, 0xa5ca, 0xa5cb, 0xa5cc, 0xa5cd, 0xa5ce, 0xa5cf, 0xa5d2, 0xa5d5, 0xa5d8, 0xa5db, 0xa5de, 0xa5df, 
            0xa5e0, 0xa5e1, 0xa5e2, 0xa5e4, 0xa5e6, 0xa5e8, 0xa5e9, 0xa5ea, 0xa5eb, 0xa5ec, 0xa5ed, 0xa5ef, 0xa5f3, 0xa1ab, 0xa1ac
         };
        private const byte LEADBYTE_HALFWIDTH = 0x10;
        private const byte SHIFT_IN = 15;
        private const byte SHIFT_OUT = 14;
        private static int[] tableBaseCodePages = new int[] { 0x3a4, 0x3a4, 0x3a4, 0, 0, 0x3b5, 0x3a8, 0, 0, 0, 0, 0 };

        [SecurityCritical]
        internal ISO2022Encoding(int codePage) : base(codePage, tableBaseCodePages[codePage % 10])
        {
            base.m_bUseMlangTypeForSerialization = true;
        }

        [SecurityCritical]
        internal ISO2022Encoding(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_ExecutionEngineException"));
        }

        private ISO2022Modes CheckEscapeSequenceJP(byte[] bytes, int escapeCount)
        {
            if (bytes[0] == 0x1b)
            {
                if (escapeCount < 3)
                {
                    return ISO2022Modes.ModeIncompleteEscape;
                }
                if (bytes[1] == 40)
                {
                    if (bytes[2] == 0x42)
                    {
                        return ISO2022Modes.ModeASCII;
                    }
                    if (bytes[2] == 0x48)
                    {
                        return ISO2022Modes.ModeASCII;
                    }
                    if (bytes[2] == 0x4a)
                    {
                        return ISO2022Modes.ModeASCII;
                    }
                    if (bytes[2] == 0x49)
                    {
                        return ISO2022Modes.ModeHalfwidthKatakana;
                    }
                }
                else if (bytes[1] == 0x24)
                {
                    if ((bytes[2] == 0x40) || (bytes[2] == 0x42))
                    {
                        return ISO2022Modes.ModeJIS0208;
                    }
                    if (escapeCount < 4)
                    {
                        return ISO2022Modes.ModeIncompleteEscape;
                    }
                    if ((bytes[2] == 40) && (bytes[3] == 0x44))
                    {
                        return ISO2022Modes.ModeJIS0208;
                    }
                }
                else if ((bytes[1] == 0x26) && (bytes[2] == 0x40))
                {
                    return ISO2022Modes.ModeNOOP;
                }
            }
            return ISO2022Modes.ModeInvalidEscape;
        }

        private ISO2022Modes CheckEscapeSequenceKR(byte[] bytes, int escapeCount)
        {
            if (bytes[0] == 0x1b)
            {
                if (escapeCount < 4)
                {
                    return ISO2022Modes.ModeIncompleteEscape;
                }
                if (((bytes[1] == 0x24) && (bytes[2] == 0x29)) && (bytes[3] == 0x43))
                {
                    return ISO2022Modes.ModeKR;
                }
            }
            return ISO2022Modes.ModeInvalidEscape;
        }

        protected override bool CleanUpBytes(ref int bytes)
        {
            switch (this.CodePage)
            {
                case 0xc42c:
                case 0xc42d:
                case 0xc42e:
                    if (bytes < 0x100)
                    {
                        if ((bytes >= 0xa1) && (bytes <= 0xdf))
                        {
                            bytes += 0xf80;
                        }
                        if ((bytes >= 0x81) && ((bytes <= 0x9f) || ((bytes >= 0xe0) && (bytes <= 0xfc))))
                        {
                            return false;
                        }
                        goto Label_0283;
                    }
                    if ((bytes >= 0xfa40) && (bytes <= 0xfc4b))
                    {
                        if ((bytes < 0xfa40) || (bytes > 0xfa5b))
                        {
                            if ((bytes >= 0xfa5c) && (bytes <= 0xfc4b))
                            {
                                byte num = (byte) bytes;
                                if (num < 0x5c)
                                {
                                    bytes -= 0xd5f;
                                }
                                else if ((num >= 0x80) && (num <= 0x9b))
                                {
                                    bytes -= 0xd1d;
                                }
                                else
                                {
                                    bytes -= 0xd1c;
                                }
                            }
                            break;
                        }
                        if (bytes > 0xfa49)
                        {
                            if ((bytes >= 0xfa4a) && (bytes <= 0xfa53))
                            {
                                bytes -= 0x72f6;
                            }
                            else if ((bytes >= 0xfa54) && (bytes <= 0xfa57))
                            {
                                bytes -= 0xb5b;
                            }
                            else if (bytes == 0xfa58)
                            {
                                bytes = 0x878a;
                            }
                            else if (bytes == 0xfa59)
                            {
                                bytes = 0x8782;
                            }
                            else if (bytes == 0xfa5a)
                            {
                                bytes = 0x8784;
                            }
                            else if (bytes == 0xfa5b)
                            {
                                bytes = 0x879a;
                            }
                            break;
                        }
                        bytes -= 0xb51;
                    }
                    break;

                case 0xc431:
                    if ((bytes < 0x80) || (bytes > 0xff))
                    {
                        if ((bytes >= 0x100) && ((((bytes & 0xff) < 0xa1) || ((bytes & 0xff) == 0xff)) || (((bytes & 0xff00) < 0xa100) || ((bytes & 0xff00) == 0xff00))))
                        {
                            return false;
                        }
                        bytes &= 0x7f7f;
                        goto Label_0283;
                    }
                    return false;

                case 0xcec8:
                    if ((bytes >= 0x81) && (bytes <= 0xfe))
                    {
                        return false;
                    }
                    goto Label_0283;

                default:
                    goto Label_0283;
            }
            byte num2 = (byte) (bytes >> 8);
            byte num3 = (byte) bytes;
            num2 = (byte) (num2 - ((num2 > 0x9f) ? 0xb1 : 0x71));
            num2 = (byte) ((num2 << 1) + 1);
            if (num3 > 0x9e)
            {
                num3 = (byte) (num3 - 0x7e);
                num2 = (byte) (num2 + 1);
            }
            else
            {
                if (num3 > 0x7e)
                {
                    num3 = (byte) (num3 - 1);
                }
                num3 = (byte) (num3 - 0x1f);
            }
            bytes = (num2 << 8) | num3;
        Label_0283:
            return true;
        }

        private byte DecrementEscapeBytes(ref byte[] bytes, ref int count)
        {
            count--;
            byte num = bytes[0];
            for (int i = 0; i < count; i++)
            {
                bytes[i] = bytes[i + 1];
            }
            bytes[count] = 0;
            return num;
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS baseEncoder)
        {
            return this.GetBytes(chars, count, null, 0, baseEncoder);
        }

        [SecurityCritical]
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS baseEncoder)
        {
            ISO2022Encoder encoder = (ISO2022Encoder) baseEncoder;
            int num = 0;
            switch (this.CodePage)
            {
                case 0xc42c:
                case 0xc42d:
                case 0xc42e:
                    return this.GetBytesCP5022xJP(chars, charCount, bytes, byteCount, encoder);

                case 0xc42f:
                case 0xc430:
                    return num;

                case 0xc431:
                    return this.GetBytesCP50225KR(chars, charCount, bytes, byteCount, encoder);

                case 0xcec8:
                    return this.GetBytesCP52936(chars, charCount, bytes, byteCount, encoder);
            }
            return num;
        }

        [SecurityCritical]
        private unsafe int GetBytesCP50225KR(char* chars, int charCount, byte* bytes, int byteCount, ISO2022Encoder encoder)
        {
            Encoding.EncodingByteBuffer buffer = new Encoding.EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            ISO2022Modes modeASCII = ISO2022Modes.ModeASCII;
            ISO2022Modes shiftInOutMode = ISO2022Modes.ModeASCII;
            if (encoder != null)
            {
                char charLeftOver = encoder.charLeftOver;
                modeASCII = encoder.currentMode;
                shiftInOutMode = encoder.shiftInOutMode;
                if (charLeftOver > '\0')
                {
                    buffer.Fallback(charLeftOver);
                }
            }
            while (buffer.MoreData)
            {
                char nextChar = buffer.GetNextChar();
                ushort num = base.mapUnicodeToBytes[nextChar];
                byte num2 = (byte) (num >> 8);
                byte num3 = (byte) (num & 0xff);
                if (num2 != 0)
                {
                    if (shiftInOutMode != ISO2022Modes.ModeKR)
                    {
                        if (!buffer.AddByte(0x1b, 0x24, 0x29, (byte) 0x43))
                        {
                            break;
                        }
                        shiftInOutMode = ISO2022Modes.ModeKR;
                    }
                    if (modeASCII != ISO2022Modes.ModeKR)
                    {
                        if (!buffer.AddByte(14))
                        {
                            break;
                        }
                        modeASCII = ISO2022Modes.ModeKR;
                    }
                    if (buffer.AddByte(num2, num3))
                    {
                        continue;
                    }
                    break;
                }
                if ((num != 0) || (nextChar == '\0'))
                {
                    if (modeASCII != ISO2022Modes.ModeASCII)
                    {
                        if (!buffer.AddByte(15))
                        {
                            break;
                        }
                        modeASCII = ISO2022Modes.ModeASCII;
                    }
                    if (buffer.AddByte(num3))
                    {
                        continue;
                    }
                    break;
                }
                buffer.Fallback(nextChar);
            }
            if ((modeASCII != ISO2022Modes.ModeASCII) && ((encoder == null) || encoder.MustFlush))
            {
                if (buffer.AddByte(15))
                {
                    modeASCII = ISO2022Modes.ModeASCII;
                }
                else
                {
                    buffer.GetNextChar();
                }
            }
            if ((bytes != null) && (encoder != null))
            {
                if (!buffer.fallbackBuffer.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.currentMode = modeASCII;
                if (!encoder.MustFlush || (encoder.charLeftOver != '\0'))
                {
                    encoder.shiftInOutMode = shiftInOutMode;
                }
                else
                {
                    encoder.shiftInOutMode = ISO2022Modes.ModeASCII;
                }
                encoder.m_charsUsed = buffer.CharsUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical]
        private unsafe int GetBytesCP5022xJP(char* chars, int charCount, byte* bytes, int byteCount, ISO2022Encoder encoder)
        {
            Encoding.EncodingByteBuffer buffer = new Encoding.EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            ISO2022Modes modeASCII = ISO2022Modes.ModeASCII;
            ISO2022Modes shiftInOutMode = ISO2022Modes.ModeASCII;
            if (encoder != null)
            {
                char charLeftOver = encoder.charLeftOver;
                modeASCII = encoder.currentMode;
                shiftInOutMode = encoder.shiftInOutMode;
                if (charLeftOver > '\0')
                {
                    buffer.Fallback(charLeftOver);
                }
            }
            while (buffer.MoreData)
            {
                byte num2;
                byte num3;
                char nextChar = buffer.GetNextChar();
                ushort num = base.mapUnicodeToBytes[nextChar];
                while (true)
                {
                    num2 = (byte) (num >> 8);
                    num3 = (byte) (num & 0xff);
                    if (num2 != 0x10)
                    {
                        goto Label_010A;
                    }
                    if (this.CodePage != 0xc42c)
                    {
                        break;
                    }
                    if ((num3 < 0x21) || (num3 >= (0x21 + HalfToFullWidthKanaTable.Length)))
                    {
                        buffer.Fallback(nextChar);
                        continue;
                    }
                    num = (ushort) (HalfToFullWidthKanaTable[num3 - 0x21] & 0x7f7f);
                }
                if (modeASCII != ISO2022Modes.ModeHalfwidthKatakana)
                {
                    if (this.CodePage == 0xc42e)
                    {
                        if (!buffer.AddByte(14))
                        {
                            break;
                        }
                        shiftInOutMode = modeASCII;
                        modeASCII = ISO2022Modes.ModeHalfwidthKatakana;
                    }
                    else
                    {
                        if (!buffer.AddByte(0x1b, 40, (byte) 0x49))
                        {
                            break;
                        }
                        modeASCII = ISO2022Modes.ModeHalfwidthKatakana;
                    }
                }
                if (buffer.AddByte((byte) (num3 & 0x7f)))
                {
                    continue;
                }
                break;
            Label_010A:
                if (num2 != 0)
                {
                    if ((this.CodePage == 0xc42e) && (modeASCII == ISO2022Modes.ModeHalfwidthKatakana))
                    {
                        if (!buffer.AddByte(15))
                        {
                            break;
                        }
                        modeASCII = shiftInOutMode;
                    }
                    if (modeASCII != ISO2022Modes.ModeJIS0208)
                    {
                        if (!buffer.AddByte(0x1b, 0x24, (byte) 0x42))
                        {
                            break;
                        }
                        modeASCII = ISO2022Modes.ModeJIS0208;
                    }
                    if (buffer.AddByte(num2, num3))
                    {
                        continue;
                    }
                    break;
                }
                if ((num != 0) || (nextChar == '\0'))
                {
                    if ((this.CodePage == 0xc42e) && (modeASCII == ISO2022Modes.ModeHalfwidthKatakana))
                    {
                        if (!buffer.AddByte(15))
                        {
                            break;
                        }
                        modeASCII = shiftInOutMode;
                    }
                    if (modeASCII != ISO2022Modes.ModeASCII)
                    {
                        if (!buffer.AddByte(0x1b, 40, (byte) 0x42))
                        {
                            break;
                        }
                        modeASCII = ISO2022Modes.ModeASCII;
                    }
                    if (buffer.AddByte(num3))
                    {
                        continue;
                    }
                    break;
                }
                buffer.Fallback(nextChar);
            }
            if ((modeASCII != ISO2022Modes.ModeASCII) && ((encoder == null) || encoder.MustFlush))
            {
                if ((this.CodePage == 0xc42e) && (modeASCII == ISO2022Modes.ModeHalfwidthKatakana))
                {
                    if (buffer.AddByte(15))
                    {
                        modeASCII = shiftInOutMode;
                    }
                    else
                    {
                        buffer.GetNextChar();
                    }
                }
                if ((modeASCII != ISO2022Modes.ModeASCII) && ((this.CodePage != 0xc42e) || (modeASCII != ISO2022Modes.ModeHalfwidthKatakana)))
                {
                    if (buffer.AddByte(0x1b, 40, (byte) 0x42))
                    {
                        modeASCII = ISO2022Modes.ModeASCII;
                    }
                    else
                    {
                        buffer.GetNextChar();
                    }
                }
            }
            if ((bytes != null) && (encoder != null))
            {
                encoder.currentMode = modeASCII;
                encoder.shiftInOutMode = shiftInOutMode;
                if (!buffer.fallbackBuffer.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = buffer.CharsUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical]
        private unsafe int GetBytesCP52936(char* chars, int charCount, byte* bytes, int byteCount, ISO2022Encoder encoder)
        {
            Encoding.EncodingByteBuffer buffer = new Encoding.EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
            ISO2022Modes modeASCII = ISO2022Modes.ModeASCII;
            if (encoder != null)
            {
                char charLeftOver = encoder.charLeftOver;
                modeASCII = encoder.currentMode;
                if (charLeftOver > '\0')
                {
                    buffer.Fallback(charLeftOver);
                }
            }
            while (buffer.MoreData)
            {
                char nextChar = buffer.GetNextChar();
                ushort num = base.mapUnicodeToBytes[nextChar];
                if ((num == 0) && (nextChar != '\0'))
                {
                    buffer.Fallback(nextChar);
                }
                else
                {
                    byte num2 = (byte) (num >> 8);
                    byte num3 = (byte) (num & 0xff);
                    if (((num2 != 0) && (((num2 < 0xa1) || (num2 > 0xf7)) || ((num3 < 0xa1) || (num3 > 0xfe)))) || (((num2 == 0) && (num3 > 0x80)) && (num3 != 0xff)))
                    {
                        buffer.Fallback(nextChar);
                        continue;
                    }
                    if (num2 != 0)
                    {
                        if (modeASCII != ISO2022Modes.ModeHZ)
                        {
                            if (!buffer.AddByte(0x7e, 0x7b, 2))
                            {
                                break;
                            }
                            modeASCII = ISO2022Modes.ModeHZ;
                        }
                        if (buffer.AddByte((byte) (num2 & 0x7f), (byte) (num3 & 0x7f)))
                        {
                            continue;
                        }
                        break;
                    }
                    if (modeASCII != ISO2022Modes.ModeASCII)
                    {
                        if (!buffer.AddByte(0x7e, 0x7d, (num3 == 0x7e) ? 2 : 1))
                        {
                            break;
                        }
                        modeASCII = ISO2022Modes.ModeASCII;
                    }
                    if (((num3 == 0x7e) && !buffer.AddByte(0x7e, 1)) || !buffer.AddByte(num3))
                    {
                        break;
                    }
                }
            }
            if ((modeASCII != ISO2022Modes.ModeASCII) && ((encoder == null) || encoder.MustFlush))
            {
                if (buffer.AddByte(0x7e, (byte) 0x7d))
                {
                    modeASCII = ISO2022Modes.ModeASCII;
                }
                else
                {
                    buffer.GetNextChar();
                }
            }
            if ((encoder != null) && (bytes != null))
            {
                encoder.currentMode = modeASCII;
                if (!buffer.fallbackBuffer.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = buffer.CharsUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical]
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
        {
            return this.GetChars(bytes, count, null, 0, baseDecoder);
        }

        [SecurityCritical]
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
        {
            ISO2022Decoder decoder = (ISO2022Decoder) baseDecoder;
            int num = 0;
            switch (this.CodePage)
            {
                case 0xc42c:
                case 0xc42d:
                case 0xc42e:
                    return this.GetCharsCP5022xJP(bytes, byteCount, chars, charCount, decoder);

                case 0xc42f:
                case 0xc430:
                    return num;

                case 0xc431:
                    return this.GetCharsCP50225KR(bytes, byteCount, chars, charCount, decoder);

                case 0xcec8:
                    return this.GetCharsCP52936(bytes, byteCount, chars, charCount, decoder);
            }
            return num;
        }

        [SecurityCritical]
        private unsafe int GetCharsCP50225KR(byte* bytes, int byteCount, char* chars, int charCount, ISO2022Decoder decoder)
        {
            Encoding.EncodingCharBuffer buffer = new Encoding.EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            ISO2022Modes modeASCII = ISO2022Modes.ModeASCII;
            byte[] buffer2 = new byte[4];
            int escapeCount = 0;
            if (decoder != null)
            {
                modeASCII = decoder.currentMode;
                escapeCount = decoder.bytesLeftOverCount;
                for (int i = 0; i < escapeCount; i++)
                {
                    buffer2[i] = decoder.bytesLeftOver[i];
                }
            }
            while (buffer.MoreData || (escapeCount > 0))
            {
                byte nextByte;
                if (escapeCount > 0)
                {
                    if (buffer2[0] == 0x1b)
                    {
                        if (!buffer.MoreData)
                        {
                            if ((decoder != null) && !decoder.MustFlush)
                            {
                                break;
                            }
                        }
                        else
                        {
                            buffer2[escapeCount++] = buffer.GetNextByte();
                            ISO2022Modes modes2 = this.CheckEscapeSequenceKR(buffer2, escapeCount);
                            if (modes2 != ISO2022Modes.ModeInvalidEscape)
                            {
                                if (modes2 != ISO2022Modes.ModeIncompleteEscape)
                                {
                                    escapeCount = 0;
                                }
                                continue;
                            }
                        }
                    }
                    nextByte = this.DecrementEscapeBytes(ref buffer2, ref escapeCount);
                }
                else
                {
                    nextByte = buffer.GetNextByte();
                    if (nextByte == 0x1b)
                    {
                        if (escapeCount == 0)
                        {
                            buffer2[0] = nextByte;
                            escapeCount = 1;
                            continue;
                        }
                        buffer.AdjustBytes(-1);
                    }
                }
                if (nextByte == 14)
                {
                    modeASCII = ISO2022Modes.ModeKR;
                }
                else
                {
                    if (nextByte == 15)
                    {
                        modeASCII = ISO2022Modes.ModeASCII;
                        continue;
                    }
                    ushort index = nextByte;
                    bool flag = false;
                    if (((modeASCII == ISO2022Modes.ModeKR) && (nextByte != 0x20)) && ((nextByte != 9) && (nextByte != 10)))
                    {
                        if (escapeCount > 0)
                        {
                            if (buffer2[0] != 0x1b)
                            {
                                index = (ushort) (index << 8);
                                index = (ushort) (index | this.DecrementEscapeBytes(ref buffer2, ref escapeCount));
                                flag = true;
                            }
                        }
                        else if (buffer.MoreData)
                        {
                            index = (ushort) (index << 8);
                            index = (ushort) (index | buffer.GetNextByte());
                            flag = true;
                        }
                        else
                        {
                            if ((decoder == null) || decoder.MustFlush)
                            {
                                buffer.Fallback(nextByte);
                            }
                            else if (chars != null)
                            {
                                buffer2[0] = nextByte;
                                escapeCount = 1;
                            }
                            break;
                        }
                    }
                    char ch = base.mapBytesToUnicode[index];
                    if ((ch == '\0') && (index != 0))
                    {
                        if (flag)
                        {
                            if (buffer.Fallback((byte) (index >> 8), (byte) index))
                            {
                                continue;
                            }
                        }
                        else if (buffer.Fallback(nextByte))
                        {
                            continue;
                        }
                        break;
                    }
                    if (!buffer.AddChar(ch, flag ? 2 : 1))
                    {
                        break;
                    }
                }
            }
            if ((chars != null) && (decoder != null))
            {
                if (!decoder.MustFlush || (escapeCount != 0))
                {
                    decoder.currentMode = modeASCII;
                    decoder.bytesLeftOverCount = escapeCount;
                    decoder.bytesLeftOver = buffer2;
                }
                else
                {
                    decoder.currentMode = ISO2022Modes.ModeASCII;
                    decoder.shiftInOutMode = ISO2022Modes.ModeASCII;
                    decoder.bytesLeftOverCount = 0;
                }
                decoder.m_bytesUsed = buffer.BytesUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical]
        private unsafe int GetCharsCP5022xJP(byte* bytes, int byteCount, char* chars, int charCount, ISO2022Decoder decoder)
        {
            Encoding.EncodingCharBuffer buffer = new Encoding.EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            ISO2022Modes modeASCII = ISO2022Modes.ModeASCII;
            ISO2022Modes shiftInOutMode = ISO2022Modes.ModeASCII;
            byte[] buffer2 = new byte[4];
            int escapeCount = 0;
            if (decoder != null)
            {
                modeASCII = decoder.currentMode;
                shiftInOutMode = decoder.shiftInOutMode;
                escapeCount = decoder.bytesLeftOverCount;
                for (int i = 0; i < escapeCount; i++)
                {
                    buffer2[i] = decoder.bytesLeftOver[i];
                }
            }
            while (buffer.MoreData || (escapeCount > 0))
            {
                byte nextByte;
                if (escapeCount > 0)
                {
                    if (buffer2[0] == 0x1b)
                    {
                        if (!buffer.MoreData)
                        {
                            if ((decoder != null) && !decoder.MustFlush)
                            {
                                break;
                            }
                        }
                        else
                        {
                            buffer2[escapeCount++] = buffer.GetNextByte();
                            ISO2022Modes modes3 = this.CheckEscapeSequenceJP(buffer2, escapeCount);
                            if (modes3 != ISO2022Modes.ModeInvalidEscape)
                            {
                                if (modes3 != ISO2022Modes.ModeIncompleteEscape)
                                {
                                    escapeCount = 0;
                                    modeASCII = shiftInOutMode = modes3;
                                }
                                continue;
                            }
                        }
                    }
                    nextByte = this.DecrementEscapeBytes(ref buffer2, ref escapeCount);
                }
                else
                {
                    nextByte = buffer.GetNextByte();
                    if (nextByte == 0x1b)
                    {
                        if (escapeCount == 0)
                        {
                            buffer2[0] = nextByte;
                            escapeCount = 1;
                            continue;
                        }
                        buffer.AdjustBytes(-1);
                    }
                }
                if (nextByte == 14)
                {
                    shiftInOutMode = modeASCII;
                    modeASCII = ISO2022Modes.ModeHalfwidthKatakana;
                }
                else
                {
                    if (nextByte == 15)
                    {
                        modeASCII = shiftInOutMode;
                        continue;
                    }
                    ushort index = nextByte;
                    bool flag = false;
                    if (modeASCII == ISO2022Modes.ModeJIS0208)
                    {
                        if (escapeCount > 0)
                        {
                            if (buffer2[0] != 0x1b)
                            {
                                index = (ushort) (index << 8);
                                index = (ushort) (index | this.DecrementEscapeBytes(ref buffer2, ref escapeCount));
                                flag = true;
                            }
                        }
                        else if (buffer.MoreData)
                        {
                            index = (ushort) (index << 8);
                            index = (ushort) (index | buffer.GetNextByte());
                            flag = true;
                        }
                        else
                        {
                            if ((decoder == null) || decoder.MustFlush)
                            {
                                buffer.Fallback(nextByte);
                            }
                            else if (chars != null)
                            {
                                buffer2[0] = nextByte;
                                escapeCount = 1;
                            }
                            break;
                        }
                        if (flag && ((index & 0xff00) == 0x2a00))
                        {
                            index = (ushort) (index & 0xff);
                            index = (ushort) (index | 0x1000);
                        }
                    }
                    else if ((index >= 0xa1) && (index <= 0xdf))
                    {
                        index = (ushort) (index | 0x1000);
                        index = (ushort) (index & 0xff7f);
                    }
                    else if (modeASCII == ISO2022Modes.ModeHalfwidthKatakana)
                    {
                        index = (ushort) (index | 0x1000);
                    }
                    char ch = base.mapBytesToUnicode[index];
                    if ((ch == '\0') && (index != 0))
                    {
                        if (flag)
                        {
                            if (buffer.Fallback((byte) (index >> 8), (byte) index))
                            {
                                continue;
                            }
                        }
                        else if (buffer.Fallback(nextByte))
                        {
                            continue;
                        }
                        break;
                    }
                    if (!buffer.AddChar(ch, flag ? 2 : 1))
                    {
                        break;
                    }
                }
            }
            if ((chars != null) && (decoder != null))
            {
                if (!decoder.MustFlush || (escapeCount != 0))
                {
                    decoder.currentMode = modeASCII;
                    decoder.shiftInOutMode = shiftInOutMode;
                    decoder.bytesLeftOverCount = escapeCount;
                    decoder.bytesLeftOver = buffer2;
                }
                else
                {
                    decoder.currentMode = ISO2022Modes.ModeASCII;
                    decoder.shiftInOutMode = ISO2022Modes.ModeASCII;
                    decoder.bytesLeftOverCount = 0;
                }
                decoder.m_bytesUsed = buffer.BytesUsed;
            }
            return buffer.Count;
        }

        [SecurityCritical]
        private unsafe int GetCharsCP52936(byte* bytes, int byteCount, char* chars, int charCount, ISO2022Decoder decoder)
        {
            Encoding.EncodingCharBuffer buffer = new Encoding.EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            ISO2022Modes modeASCII = ISO2022Modes.ModeASCII;
            int num = -1;
            bool flag = false;
            if (decoder != null)
            {
                modeASCII = decoder.currentMode;
                if (decoder.bytesLeftOverCount != 0)
                {
                    num = decoder.bytesLeftOver[0];
                }
            }
            while (buffer.MoreData || (num >= 0))
            {
                byte nextByte;
                if (num >= 0)
                {
                    nextByte = (byte) num;
                    num = -1;
                }
                else
                {
                    nextByte = buffer.GetNextByte();
                }
                if (nextByte == 0x7e)
                {
                    if (!buffer.MoreData)
                    {
                        if ((decoder == null) || decoder.MustFlush)
                        {
                            buffer.Fallback(nextByte);
                        }
                        else
                        {
                            if (decoder != null)
                            {
                                decoder.ClearMustFlush();
                            }
                            if (chars != null)
                            {
                                decoder.bytesLeftOverCount = 1;
                                decoder.bytesLeftOver[0] = 0x7e;
                                flag = true;
                            }
                        }
                        break;
                    }
                    nextByte = buffer.GetNextByte();
                    if ((nextByte == 0x7e) && (modeASCII == ISO2022Modes.ModeASCII))
                    {
                        if (buffer.AddChar((char) nextByte, 2))
                        {
                            continue;
                        }
                        break;
                    }
                    if (nextByte == 0x7b)
                    {
                        modeASCII = ISO2022Modes.ModeHZ;
                        continue;
                    }
                    if (nextByte == 0x7d)
                    {
                        modeASCII = ISO2022Modes.ModeASCII;
                        continue;
                    }
                    if (nextByte == 10)
                    {
                        continue;
                    }
                    buffer.AdjustBytes(-1);
                    nextByte = 0x7e;
                }
                if ((modeASCII != ISO2022Modes.ModeASCII) && (nextByte >= 0x20))
                {
                    if (!buffer.MoreData)
                    {
                        if ((decoder == null) || decoder.MustFlush)
                        {
                            buffer.Fallback(nextByte);
                        }
                        else
                        {
                            if (decoder != null)
                            {
                                decoder.ClearMustFlush();
                            }
                            if (chars != null)
                            {
                                decoder.bytesLeftOverCount = 1;
                                decoder.bytesLeftOver[0] = nextByte;
                                flag = true;
                            }
                        }
                    }
                    else
                    {
                        char ch;
                        byte num3 = buffer.GetNextByte();
                        ushort index = (ushort) ((nextByte << 8) | num3);
                        if ((nextByte == 0x20) && (num3 != 0))
                        {
                            ch = (char) num3;
                        }
                        else
                        {
                            if ((((nextByte < 0x21) || (nextByte > 0x77)) || ((num3 < 0x21) || (num3 > 0x7e))) && (((nextByte < 0xa1) || (nextByte > 0xf7)) || ((num3 < 0xa1) || (num3 > 0xfe))))
                            {
                                if (((num3 == 0x20) && (0x21 <= nextByte)) && (nextByte <= 0x7d))
                                {
                                    index = 0x2121;
                                }
                                else
                                {
                                    if (buffer.Fallback((byte) (index >> 8), (byte) index))
                                    {
                                        continue;
                                    }
                                    break;
                                }
                            }
                            index = (ushort) (index | 0x8080);
                            ch = base.mapBytesToUnicode[index];
                        }
                        if ((ch == '\0') && (index != 0))
                        {
                            if (buffer.Fallback((byte) (index >> 8), (byte) index))
                            {
                                continue;
                            }
                        }
                        else if (buffer.AddChar(ch, 2))
                        {
                            continue;
                        }
                    }
                    break;
                }
                char ch2 = base.mapBytesToUnicode[nextByte];
                if (((ch2 == '\0') || (ch2 == '\0')) && (nextByte != 0))
                {
                    if (buffer.Fallback(nextByte))
                    {
                        continue;
                    }
                    break;
                }
                if (!buffer.AddChar(ch2))
                {
                    break;
                }
            }
            if ((chars != null) && (decoder != null))
            {
                if (!flag)
                {
                    decoder.bytesLeftOverCount = 0;
                }
                if (decoder.MustFlush && (decoder.bytesLeftOverCount == 0))
                {
                    decoder.currentMode = ISO2022Modes.ModeASCII;
                }
                else
                {
                    decoder.currentMode = modeASCII;
                }
                decoder.m_bytesUsed = buffer.BytesUsed;
            }
            return buffer.Count;
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new ISO2022Decoder(this);
        }

        public override System.Text.Encoder GetEncoder()
        {
            return new ISO2022Encoder(this);
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
            int num2 = 2;
            int num3 = 0;
            int num4 = 0;
            switch (this.CodePage)
            {
                case 0xc42c:
                case 0xc42d:
                    num2 = 5;
                    num4 = 3;
                    break;

                case 0xc42e:
                    num2 = 5;
                    num4 = 4;
                    break;

                case 0xc431:
                    num2 = 3;
                    num3 = 4;
                    num4 = 1;
                    break;

                case 0xcec8:
                    num2 = 4;
                    num4 = 2;
                    break;
            }
            num *= num2;
            num += num3 + num4;
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
            int num = 1;
            int num2 = 1;
            switch (this.CodePage)
            {
                case 0xc42c:
                case 0xc42d:
                case 0xc42e:
                case 0xc431:
                    num = 1;
                    num2 = 3;
                    break;

                case 0xcec8:
                    num = 1;
                    num2 = 1;
                    break;
            }
            long num3 = (byteCount * num) + num2;
            if (base.DecoderFallback.MaxCharCount > 1)
            {
                num3 *= base.DecoderFallback.MaxCharCount;
            }
            if (num3 > 0x7fffffffL)
            {
                throw new ArgumentOutOfRangeException("byteCount", Environment.GetResourceString("ArgumentOutOfRange_GetCharCountOverflow"));
            }
            return (int) num3;
        }

        [SecurityCritical]
        protected override unsafe string GetMemorySectionName()
        {
            string str;
            int num = base.bFlagDataTable ? base.dataTableCodePage : this.CodePage;
            switch (this.CodePage)
            {
                case 0xc42c:
                case 0xc42d:
                case 0xc42e:
                    str = "CodePage_{0}_{1}_{2}_{3}_{4}_ISO2022JP";
                    break;

                case 0xc431:
                    str = "CodePage_{0}_{1}_{2}_{3}_{4}_ISO2022KR";
                    break;

                case 0xcec8:
                    str = "CodePage_{0}_{1}_{2}_{3}_{4}_HZ";
                    break;

                default:
                    str = "CodePage_{0}_{1}_{2}_{3}_{4}";
                    break;
            }
            return string.Format(CultureInfo.InvariantCulture, str, new object[] { num, base.pCodePage.VersionMajor, base.pCodePage.VersionMinor, base.pCodePage.VersionRevision, base.pCodePage.VersionBuild });
        }

        [Serializable]
        internal class ISO2022Decoder : DecoderNLS
        {
            internal byte[] bytesLeftOver;
            internal int bytesLeftOverCount;
            internal ISO2022Encoding.ISO2022Modes currentMode;
            internal ISO2022Encoding.ISO2022Modes shiftInOutMode;

            internal ISO2022Decoder(EncodingNLS encoding) : base(encoding)
            {
            }

            public override void Reset()
            {
                this.bytesLeftOverCount = 0;
                this.bytesLeftOver = new byte[4];
                this.currentMode = ISO2022Encoding.ISO2022Modes.ModeASCII;
                this.shiftInOutMode = ISO2022Encoding.ISO2022Modes.ModeASCII;
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            internal override bool HasState
            {
                get
                {
                    if (this.bytesLeftOverCount == 0)
                    {
                        return (this.currentMode != ISO2022Encoding.ISO2022Modes.ModeASCII);
                    }
                    return true;
                }
            }
        }

        [Serializable]
        internal class ISO2022Encoder : EncoderNLS
        {
            internal ISO2022Encoding.ISO2022Modes currentMode;
            internal ISO2022Encoding.ISO2022Modes shiftInOutMode;

            internal ISO2022Encoder(EncodingNLS encoding) : base(encoding)
            {
            }

            public override void Reset()
            {
                this.currentMode = ISO2022Encoding.ISO2022Modes.ModeASCII;
                this.shiftInOutMode = ISO2022Encoding.ISO2022Modes.ModeASCII;
                base.charLeftOver = '\0';
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            internal override bool HasState
            {
                get
                {
                    if (base.charLeftOver == '\0')
                    {
                        return (this.currentMode != ISO2022Encoding.ISO2022Modes.ModeASCII);
                    }
                    return true;
                }
            }
        }

        internal enum ISO2022Modes
        {
            ModeASCII = 11,
            ModeCNS11643_1 = 9,
            ModeCNS11643_2 = 10,
            ModeGB2312 = 7,
            ModeHalfwidthKatakana = 0,
            ModeHZ = 6,
            ModeIncompleteEscape = -1,
            ModeInvalidEscape = -2,
            ModeJIS0208 = 1,
            ModeKR = 5,
            ModeNOOP = -3
        }
    }
}

