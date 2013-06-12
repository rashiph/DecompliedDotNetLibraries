namespace System.Text
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal sealed class GB18030Encoding : DBCSCodePageEncoding, ISerializable
    {
        private const int GB18030 = 0xd698;
        private const int GBLast4ByteCode = 0x99fb;
        private const int GBLastSurrogateOffset = 0x12e247;
        private const int GBSurrogateOffset = 0x2e248;
        [NonSerialized]
        internal unsafe char* map4BytesToUnicode;
        [NonSerialized]
        internal unsafe byte* mapUnicodeTo4BytesFlags;
        private readonly ushort[] tableUnicodeToGBDiffs;

        [SecurityCritical]
        internal unsafe GB18030Encoding() : base(0xd698, 0x3a8)
        {
            this.map4BytesToUnicode = null;
            this.mapUnicodeTo4BytesFlags = null;
            this.tableUnicodeToGBDiffs = new ushort[] { 
                0x8080, 0x24, 0x8001, 2, 0x8002, 7, 0x8002, 5, 0x8001, 0x1f, 0x8001, 8, 0x8002, 6, 0x8003, 1, 
                0x8002, 4, 0x8002, 3, 0x8001, 1, 0x8002, 1, 0x8001, 4, 0x8001, 0x11, 0x8001, 7, 0x8001, 15, 
                0x8001, 0x18, 0x8001, 3, 0x8001, 4, 0x8001, 0x1d, 0x8001, 0x62, 0x8001, 1, 0x8001, 1, 0x8001, 1, 
                0x8001, 1, 0x8001, 1, 0x8001, 1, 0x8001, 1, 0x8001, 0x1c, 0xa8bf, 0x57, 0x8001, 15, 0x8001, 0x65, 
                0x8001, 1, 0x8003, 13, 0x8001, 0xb7, 0x8011, 1, 0x8007, 7, 0x8011, 1, 0x8007, 0x37, 0x8001, 14, 
                0x8040, 1, 0x8001, 0x1bbe, 0x8001, 2, 0x8004, 1, 0x8002, 2, 0x8002, 7, 0x8002, 9, 0x8001, 1, 
                0x8002, 1, 0x8001, 5, 0x8001, 0x70, 0xa2e3, 0x56, 0x8001, 1, 0x8001, 3, 0x8001, 12, 0x8001, 10, 
                0x8001, 0x3e, 0x800c, 4, 0x800a, 0x16, 0x8004, 2, 0x8004, 110, 0x8001, 6, 0x8001, 1, 0x8001, 3, 
                0x8001, 4, 0x8001, 2, 0x8004, 2, 0x8001, 1, 0x8001, 1, 0x8005, 2, 0x8001, 5, 0x8004, 5, 
                0x8001, 10, 0x8001, 3, 0x8001, 5, 0x8001, 13, 0x8002, 2, 0x8004, 6, 0x8002, 0x25, 0x8001, 3, 
                0x8001, 11, 0x8001, 0x19, 0x8001, 0x52, 0x8001, 0x14d, 0x800a, 10, 0x8028, 100, 0x804c, 4, 0x8024, 13, 
                0x800f, 3, 0x8003, 10, 0x8002, 0x10, 0x8002, 8, 0x8002, 8, 0x8002, 3, 0x8001, 2, 0x8002, 0x12, 
                0x8004, 0x1f, 0x8002, 2, 0x8001, 0x36, 0x8001, 1, 0x8001, 0x83e, 0xfe50, 2, 0xfe54, 3, 0xfe57, 2, 
                0xfe58, 0xfe5d, 10, 0xfe5e, 15, 0xfe6b, 2, 0xfe6e, 3, 0xfe71, 4, 0xfe73, 2, 0xfe74, 0xfe75, 3, 
                0xfe79, 14, 0xfe84, 0x125, 0xa98a, 0xa98b, 0xa98c, 0xa98d, 0xa98e, 0xa98f, 0xa990, 0xa991, 0xa992, 0xa993, 0xa994, 0xa995, 
                4, 0x8004, 1, 0x8013, 5, 0x8002, 2, 0x8009, 20, 0xa989, 2, 0x8053, 7, 0x8004, 2, 0x8056, 
                5, 0x8003, 6, 0x8025, 0xf6, 0x800a, 7, 0x8001, 0x71, 0x8001, 0xea, 0x8002, 12, 0x8003, 2, 0x8001, 
                0x22, 0x8001, 9, 0x8001, 2, 0x8002, 2, 0x8001, 0x71, 0xfe56, 0x2b, 0xfe55, 0x12a, 0xfe5a, 0x6f, 0xfe5c, 
                11, 0xfe5b, 0x2fd, 0xfe60, 0x55, 0xfe5f, 0x60, 0xfe62, 0xfe65, 14, 0xfe63, 0x93, 0xfe64, 0xda, 0xfe68, 0x11f, 
                0xfe69, 0x71, 0xfe6a, 0x375, 0xfe6f, 0x108, 0xfe70, 0x1d7, 0xfe72, 0x74, 0xfe78, 4, 0xfe77, 0x2b, 0xfe7a, 0xf8, 
                0xfe7b, 0x175, 0xfe7d, 20, 0xfe7c, 0xc1, 0xfe80, 5, 0xfe81, 0x52, 0xfe82, 0x10, 0xfe83, 0x1b9, 0xfe85, 50, 
                0xfe86, 2, 0xfe87, 4, 0xfe88, 0xfe89, 1, 0xfe8a, 0xfe8b, 20, 0xfe8d, 3, 0xfe8c, 0x16, 0xfe8f, 0xfe8e, 
                0x2bf, 0xfe96, 0x27, 0xfe93, 0xfe94, 0xfe95, 0xfe97, 0xfe92, 0x6f, 0xfe98, 0xfe99, 0xfe9a, 0xfe9b, 0xfe9c, 0xfe9d, 0xfe9e, 
                0x94, 0xfe9f, 0x51, 0xd1a6, 0x385a, 0x8f6c, 1, 0x805b, 1, 0x801e, 13, 0x8021, 1, 0x8003, 5, 0x8001, 
                7, 0x8001, 4, 0x8002, 4, 0x8002, 8, 0x8001, 7, 0x8001, 0x10, 0x8002, 14, 0x8001, 0x10c7, 0x8001, 
                0x4c, 0x8001, 0x1b, 0x8001, 0x51, 0x8001, 9, 0x8001, 0x1a, 0x8004, 1, 0x8001, 1, 0x8002, 3, 0x8001, 
                6, 0x8003, 1, 0x8002, 2, 0x8003, 0x406, 0x8002, 1, 0x8012, 4, 0x800a, 1, 0x8004, 1, 0x800e, 
                1, 0x8004, 0x95, 0x805e, 0x81, 0x8006, 0x1a
             };
        }

        [SecurityCritical]
        internal unsafe GB18030Encoding(SerializationInfo info, StreamingContext context) : base(0xd698, 0x3a8)
        {
            this.map4BytesToUnicode = null;
            this.mapUnicodeTo4BytesFlags = null;
            this.tableUnicodeToGBDiffs = new ushort[] { 
                0x8080, 0x24, 0x8001, 2, 0x8002, 7, 0x8002, 5, 0x8001, 0x1f, 0x8001, 8, 0x8002, 6, 0x8003, 1, 
                0x8002, 4, 0x8002, 3, 0x8001, 1, 0x8002, 1, 0x8001, 4, 0x8001, 0x11, 0x8001, 7, 0x8001, 15, 
                0x8001, 0x18, 0x8001, 3, 0x8001, 4, 0x8001, 0x1d, 0x8001, 0x62, 0x8001, 1, 0x8001, 1, 0x8001, 1, 
                0x8001, 1, 0x8001, 1, 0x8001, 1, 0x8001, 1, 0x8001, 0x1c, 0xa8bf, 0x57, 0x8001, 15, 0x8001, 0x65, 
                0x8001, 1, 0x8003, 13, 0x8001, 0xb7, 0x8011, 1, 0x8007, 7, 0x8011, 1, 0x8007, 0x37, 0x8001, 14, 
                0x8040, 1, 0x8001, 0x1bbe, 0x8001, 2, 0x8004, 1, 0x8002, 2, 0x8002, 7, 0x8002, 9, 0x8001, 1, 
                0x8002, 1, 0x8001, 5, 0x8001, 0x70, 0xa2e3, 0x56, 0x8001, 1, 0x8001, 3, 0x8001, 12, 0x8001, 10, 
                0x8001, 0x3e, 0x800c, 4, 0x800a, 0x16, 0x8004, 2, 0x8004, 110, 0x8001, 6, 0x8001, 1, 0x8001, 3, 
                0x8001, 4, 0x8001, 2, 0x8004, 2, 0x8001, 1, 0x8001, 1, 0x8005, 2, 0x8001, 5, 0x8004, 5, 
                0x8001, 10, 0x8001, 3, 0x8001, 5, 0x8001, 13, 0x8002, 2, 0x8004, 6, 0x8002, 0x25, 0x8001, 3, 
                0x8001, 11, 0x8001, 0x19, 0x8001, 0x52, 0x8001, 0x14d, 0x800a, 10, 0x8028, 100, 0x804c, 4, 0x8024, 13, 
                0x800f, 3, 0x8003, 10, 0x8002, 0x10, 0x8002, 8, 0x8002, 8, 0x8002, 3, 0x8001, 2, 0x8002, 0x12, 
                0x8004, 0x1f, 0x8002, 2, 0x8001, 0x36, 0x8001, 1, 0x8001, 0x83e, 0xfe50, 2, 0xfe54, 3, 0xfe57, 2, 
                0xfe58, 0xfe5d, 10, 0xfe5e, 15, 0xfe6b, 2, 0xfe6e, 3, 0xfe71, 4, 0xfe73, 2, 0xfe74, 0xfe75, 3, 
                0xfe79, 14, 0xfe84, 0x125, 0xa98a, 0xa98b, 0xa98c, 0xa98d, 0xa98e, 0xa98f, 0xa990, 0xa991, 0xa992, 0xa993, 0xa994, 0xa995, 
                4, 0x8004, 1, 0x8013, 5, 0x8002, 2, 0x8009, 20, 0xa989, 2, 0x8053, 7, 0x8004, 2, 0x8056, 
                5, 0x8003, 6, 0x8025, 0xf6, 0x800a, 7, 0x8001, 0x71, 0x8001, 0xea, 0x8002, 12, 0x8003, 2, 0x8001, 
                0x22, 0x8001, 9, 0x8001, 2, 0x8002, 2, 0x8001, 0x71, 0xfe56, 0x2b, 0xfe55, 0x12a, 0xfe5a, 0x6f, 0xfe5c, 
                11, 0xfe5b, 0x2fd, 0xfe60, 0x55, 0xfe5f, 0x60, 0xfe62, 0xfe65, 14, 0xfe63, 0x93, 0xfe64, 0xda, 0xfe68, 0x11f, 
                0xfe69, 0x71, 0xfe6a, 0x375, 0xfe6f, 0x108, 0xfe70, 0x1d7, 0xfe72, 0x74, 0xfe78, 4, 0xfe77, 0x2b, 0xfe7a, 0xf8, 
                0xfe7b, 0x175, 0xfe7d, 20, 0xfe7c, 0xc1, 0xfe80, 5, 0xfe81, 0x52, 0xfe82, 0x10, 0xfe83, 0x1b9, 0xfe85, 50, 
                0xfe86, 2, 0xfe87, 4, 0xfe88, 0xfe89, 1, 0xfe8a, 0xfe8b, 20, 0xfe8d, 3, 0xfe8c, 0x16, 0xfe8f, 0xfe8e, 
                0x2bf, 0xfe96, 0x27, 0xfe93, 0xfe94, 0xfe95, 0xfe97, 0xfe92, 0x6f, 0xfe98, 0xfe99, 0xfe9a, 0xfe9b, 0xfe9c, 0xfe9d, 0xfe9e, 
                0x94, 0xfe9f, 0x51, 0xd1a6, 0x385a, 0x8f6c, 1, 0x805b, 1, 0x801e, 13, 0x8021, 1, 0x8003, 5, 0x8001, 
                7, 0x8001, 4, 0x8002, 4, 0x8002, 8, 0x8001, 7, 0x8001, 0x10, 0x8002, 14, 0x8001, 0x10c7, 0x8001, 
                0x4c, 0x8001, 0x1b, 0x8001, 0x51, 0x8001, 9, 0x8001, 0x1a, 0x8004, 1, 0x8001, 1, 0x8002, 3, 0x8001, 
                6, 0x8003, 1, 0x8002, 2, 0x8003, 0x406, 0x8002, 1, 0x8012, 4, 0x800a, 1, 0x8004, 1, 0x800e, 
                1, 0x8004, 0x95, 0x805e, 0x81, 0x8006, 0x1a
             };
            base.DeserializeEncoding(info, context);
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS encoder)
        {
            return this.GetBytes(chars, count, null, 0, encoder);
        }

        [SecurityCritical]
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
        {
            char charFallback = '\0';
            if (encoder != null)
            {
                charFallback = encoder.charLeftOver;
            }
            Encoding.EncodingByteBuffer buffer = new Encoding.EncodingByteBuffer(this, encoder, bytes, byteCount, chars, charCount);
        Label_0183:
            while (buffer.MoreData)
            {
                char nextChar = buffer.GetNextChar();
                if (charFallback != '\0')
                {
                    if (!char.IsLowSurrogate(nextChar))
                    {
                        buffer.MovePrevious(false);
                        if (!buffer.Fallback(charFallback))
                        {
                            charFallback = '\0';
                            break;
                        }
                        charFallback = '\0';
                    }
                    else
                    {
                        int num = ((charFallback - 0xd800) << 10) + (nextChar - 0xdc00);
                        byte num2 = (byte) ((num % 10) + 0x30);
                        num /= 10;
                        byte num3 = (byte) ((num % 0x7e) + 0x81);
                        num /= 0x7e;
                        byte num4 = (byte) ((num % 10) + 0x30);
                        num /= 10;
                        charFallback = '\0';
                        if (!buffer.AddByte((byte) (num + 0x90), num4, num3, num2))
                        {
                            buffer.MovePrevious(false);
                            break;
                        }
                        charFallback = '\0';
                    }
                }
                else
                {
                    if (nextChar <= '\x007f')
                    {
                        if (buffer.AddByte((byte) nextChar))
                        {
                            continue;
                        }
                        break;
                    }
                    if (char.IsHighSurrogate(nextChar))
                    {
                        charFallback = nextChar;
                    }
                    else
                    {
                        if (char.IsLowSurrogate(nextChar))
                        {
                            if (buffer.Fallback(nextChar))
                            {
                                continue;
                            }
                            break;
                        }
                        ushort num5 = base.mapUnicodeToBytes[nextChar];
                        if (this.Is4Byte(nextChar))
                        {
                            byte num6 = (byte) ((num5 % 10) + 0x30);
                            num5 = (ushort) (num5 / 10);
                            byte num7 = (byte) ((num5 % 0x7e) + 0x81);
                            num5 = (ushort) (num5 / 0x7e);
                            byte num8 = (byte) ((num5 % 10) + 0x30);
                            num5 = (ushort) (num5 / 10);
                            if (buffer.AddByte((byte) (num5 + 0x81), num8, num7, num6))
                            {
                                continue;
                            }
                            break;
                        }
                        if (!buffer.AddByte((byte) (num5 >> 8), (byte) (num5 & 0xff)))
                        {
                            break;
                        }
                    }
                }
            }
            if (((encoder == null) || encoder.MustFlush) && (charFallback > '\0'))
            {
                buffer.Fallback(charFallback);
                charFallback = '\0';
                goto Label_0183;
            }
            if (encoder != null)
            {
                if (bytes != null)
                {
                    encoder.charLeftOver = charFallback;
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
            GB18030Decoder decoder = (GB18030Decoder) baseDecoder;
            Encoding.EncodingCharBuffer buffer = new Encoding.EncodingCharBuffer(this, decoder, chars, charCount, bytes, byteCount);
            short ch = -1;
            short nextByte = -1;
            short num3 = -1;
            short num4 = -1;
            if ((decoder != null) && (decoder.bLeftOver1 != -1))
            {
                ch = decoder.bLeftOver1;
                nextByte = decoder.bLeftOver2;
                num3 = decoder.bLeftOver3;
                num4 = decoder.bLeftOver4;
                while (ch != -1)
                {
                    if (this.IsGBLeadByte(ch))
                    {
                        goto Label_00FC;
                    }
                    if (ch <= 0x7f)
                    {
                        if (!buffer.AddChar((char) ((ushort) ch)))
                        {
                            break;
                        }
                    }
                    else if (!buffer.Fallback((byte) ch))
                    {
                        break;
                    }
                    ch = nextByte;
                    nextByte = num3;
                    num3 = num4;
                    num4 = -1;
                    continue;
                Label_0092:
                    if (!buffer.MoreData)
                    {
                        if (!decoder.MustFlush)
                        {
                            if (chars != null)
                            {
                                decoder.bLeftOver1 = ch;
                                decoder.bLeftOver2 = nextByte;
                                decoder.bLeftOver3 = num3;
                                decoder.bLeftOver4 = num4;
                            }
                            decoder.m_bytesUsed = buffer.BytesUsed;
                            return buffer.Count;
                        }
                        goto Label_010E;
                    }
                    if (nextByte == -1)
                    {
                        nextByte = buffer.GetNextByte();
                    }
                    else if (num3 == -1)
                    {
                        num3 = buffer.GetNextByte();
                    }
                    else
                    {
                        num4 = buffer.GetNextByte();
                    }
                Label_00FC:
                    if ((nextByte == -1) || (this.IsGBFourByteTrailing(nextByte) && (num4 == -1)))
                    {
                        goto Label_0092;
                    }
                Label_010E:
                    if (this.IsGBTwoByteTrailing(nextByte))
                    {
                        int index = ch << 8;
                        index |= (byte) nextByte;
                        if (!buffer.AddChar(base.mapBytesToUnicode[index], 2))
                        {
                            break;
                        }
                        ch = -1;
                        nextByte = -1;
                        continue;
                    }
                    if ((this.IsGBFourByteTrailing(nextByte) && this.IsGBLeadByte(num3)) && this.IsGBFourByteTrailing(num4))
                    {
                        int num6 = this.GetFourBytesOffset(ch, nextByte, num3, num4);
                        if (num6 <= 0x99fb)
                        {
                            if (!buffer.AddChar(this.map4BytesToUnicode[num6], 4))
                            {
                                break;
                            }
                        }
                        else if ((num6 >= 0x2e248) && (num6 <= 0x12e247))
                        {
                            num6 -= 0x2e248;
                            if (!buffer.AddChar((char) (0xd800 + (num6 / 0x400)), (char) (0xdc00 + (num6 % 0x400)), 4))
                            {
                                break;
                            }
                        }
                        else if (!buffer.Fallback((byte) ch, (byte) nextByte, (byte) num3, (byte) num4))
                        {
                            break;
                        }
                        ch = -1;
                        nextByte = -1;
                        num3 = -1;
                        num4 = -1;
                        continue;
                    }
                    if (!buffer.Fallback((byte) ch))
                    {
                        break;
                    }
                    ch = nextByte;
                    nextByte = num3;
                    num3 = num4;
                    num4 = -1;
                }
            }
            while (buffer.MoreData)
            {
                byte num7 = buffer.GetNextByte();
                if (num7 <= 0x7f)
                {
                    if (buffer.AddChar((char) num7))
                    {
                        continue;
                    }
                    break;
                }
                if (this.IsGBLeadByte(num7))
                {
                    if (buffer.MoreData)
                    {
                        byte num8 = buffer.GetNextByte();
                        if (this.IsGBTwoByteTrailing(num8))
                        {
                            int num9 = num7 << 8;
                            num9 |= num8;
                            if (buffer.AddChar(base.mapBytesToUnicode[num9], 2))
                            {
                                continue;
                            }
                        }
                        else if (this.IsGBFourByteTrailing(num8))
                        {
                            if (buffer.EvenMoreData(2))
                            {
                                byte num10 = buffer.GetNextByte();
                                byte num11 = buffer.GetNextByte();
                                if (this.IsGBLeadByte(num10) && this.IsGBFourByteTrailing(num11))
                                {
                                    int num12 = this.GetFourBytesOffset(num7, num8, num10, num11);
                                    if (num12 <= 0x99fb)
                                    {
                                        if (buffer.AddChar(this.map4BytesToUnicode[num12], 4))
                                        {
                                            continue;
                                        }
                                    }
                                    else if ((num12 >= 0x2e248) && (num12 <= 0x12e247))
                                    {
                                        num12 -= 0x2e248;
                                        if (buffer.AddChar((char) (0xd800 + (num12 / 0x400)), (char) (0xdc00 + (num12 % 0x400)), 4))
                                        {
                                            continue;
                                        }
                                    }
                                    else if (buffer.Fallback(num7, num8, num10, num11))
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    buffer.AdjustBytes(-3);
                                    if (buffer.Fallback(num7))
                                    {
                                        continue;
                                    }
                                }
                            }
                            else if ((decoder != null) && !decoder.MustFlush)
                            {
                                if (chars != null)
                                {
                                    ch = num7;
                                    nextByte = num8;
                                    if (buffer.MoreData)
                                    {
                                        num3 = buffer.GetNextByte();
                                    }
                                    else
                                    {
                                        num3 = -1;
                                    }
                                    num4 = -1;
                                }
                            }
                            else if (buffer.Fallback(num7, num8))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            buffer.AdjustBytes(-1);
                            if (buffer.Fallback(num7))
                            {
                                continue;
                            }
                        }
                    }
                    else if ((decoder != null) && !decoder.MustFlush)
                    {
                        if (chars != null)
                        {
                            ch = num7;
                            nextByte = -1;
                            num3 = -1;
                            num4 = -1;
                        }
                    }
                    else if (buffer.Fallback(num7))
                    {
                        continue;
                    }
                    break;
                }
                if (!buffer.Fallback(num7))
                {
                    break;
                }
            }
            if (decoder != null)
            {
                if (chars != null)
                {
                    decoder.bLeftOver1 = ch;
                    decoder.bLeftOver2 = nextByte;
                    decoder.bLeftOver3 = num3;
                    decoder.bLeftOver4 = num4;
                }
                decoder.m_bytesUsed = buffer.BytesUsed;
            }
            return buffer.Count;
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new GB18030Decoder(this);
        }

        internal int GetFourBytesOffset(short offset1, short offset2, short offset3, short offset4)
        {
            return ((((((((offset1 - 0x81) * 10) * 0x7e) * 10) + (((offset2 - 0x30) * 0x7e) * 10)) + ((offset3 - 0x81) * 10)) + offset4) - 0x30);
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
            long num = byteCount + 3L;
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

        [SecurityCritical]
        internal unsafe bool Is4Byte(char charTest)
        {
            byte num = this.mapUnicodeTo4BytesFlags[charTest / '\b'];
            return ((num != 0) && ((num & (((int) 1) << (charTest % 8))) != 0));
        }

        internal bool IsGBFourByteTrailing(short ch)
        {
            return ((ch >= 0x30) && (ch <= 0x39));
        }

        internal bool IsGBLeadByte(short ch)
        {
            return ((ch >= 0x81) && (ch <= 0xfe));
        }

        internal bool IsGBTwoByteTrailing(short ch)
        {
            return (((ch >= 0x40) && (ch <= 0x7e)) || ((ch >= 0x80) && (ch <= 0xfe)));
        }

        [SecurityCritical]
        protected override unsafe void LoadManagedCodePage()
        {
            base.bFlagDataTable = false;
            base.iExtraBytes = 0x153f8;
            base.LoadManagedCodePage();
            byte* handle = (byte*) base.safeMemorySectionHandle.DangerousGetHandle();
            this.mapUnicodeTo4BytesFlags = handle + 0x40000;
            this.map4BytesToUnicode = (char*) ((handle + 0x40000) + 0x2000);
            if (base.mapCodePageCached[0] != this.CodePage)
            {
                char index = '\0';
                ushort num = 0;
                for (int i = 0; i < this.tableUnicodeToGBDiffs.Length; i++)
                {
                    ushort num3 = this.tableUnicodeToGBDiffs[i];
                    if ((num3 & 0x8000) == 0)
                    {
                        goto Label_0105;
                    }
                    if ((num3 > 0x9000) && (num3 != 0xd1a6))
                    {
                        base.mapBytesToUnicode[num3] = index;
                        base.mapUnicodeToBytes[index] = num3;
                        index = (char) (index + '\x0001');
                    }
                    else
                    {
                        index = (char) (index + ((char) (num3 & 0x7fff)));
                    }
                    continue;
                Label_00C2:
                    this.map4BytesToUnicode[num] = index;
                    base.mapUnicodeToBytes[index] = num;
                    byte* numPtr1 = this.mapUnicodeTo4BytesFlags + (index / '\b');
                    numPtr1[0] = (byte) (numPtr1[0] | ((byte) (((int) 1) << (index % 8))));
                    index = (char) (index + '\x0001');
                    num = (ushort) (num + 1);
                    num3 = (ushort) (num3 - 1);
                Label_0105:
                    if (num3 > 0)
                    {
                        goto Label_00C2;
                    }
                }
                base.mapCodePageCached[0] = this.CodePage;
            }
        }

        internal override void SetDefaultFallbacks()
        {
            base.encoderFallback = EncoderFallback.ReplacementFallback;
            base.decoderFallback = DecoderFallback.ReplacementFallback;
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.SerializeEncoding(info, context);
        }

        [Serializable]
        internal sealed class GB18030Decoder : DecoderNLS, ISerializable
        {
            internal short bLeftOver1;
            internal short bLeftOver2;
            internal short bLeftOver3;
            internal short bLeftOver4;

            internal GB18030Decoder(EncodingNLS encoding) : base(encoding)
            {
                this.bLeftOver1 = -1;
                this.bLeftOver2 = -1;
                this.bLeftOver3 = -1;
                this.bLeftOver4 = -1;
            }

            [SecurityCritical]
            internal GB18030Decoder(SerializationInfo info, StreamingContext context)
            {
                this.bLeftOver1 = -1;
                this.bLeftOver2 = -1;
                this.bLeftOver3 = -1;
                this.bLeftOver4 = -1;
                if (info == null)
                {
                    throw new ArgumentNullException("info");
                }
                try
                {
                    base.m_encoding = (Encoding) info.GetValue("m_encoding", typeof(Encoding));
                    base.m_fallback = (DecoderFallback) info.GetValue("m_fallback", typeof(DecoderFallback));
                    this.bLeftOver1 = (short) info.GetValue("bLeftOver1", typeof(short));
                    this.bLeftOver2 = (short) info.GetValue("bLeftOver2", typeof(short));
                    this.bLeftOver3 = (short) info.GetValue("bLeftOver3", typeof(short));
                    this.bLeftOver4 = (short) info.GetValue("bLeftOver4", typeof(short));
                }
                catch (SerializationException)
                {
                    base.m_encoding = new GB18030Encoding();
                }
            }

            public override void Reset()
            {
                this.bLeftOver1 = -1;
                this.bLeftOver2 = -1;
                this.bLeftOver3 = -1;
                this.bLeftOver4 = -1;
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
                info.AddValue("bLeftOver1", this.bLeftOver1);
                info.AddValue("bLeftOver2", this.bLeftOver2);
                info.AddValue("bLeftOver3", this.bLeftOver3);
                info.AddValue("bLeftOver4", this.bLeftOver4);
                info.AddValue("m_leftOverBytes", 0);
                info.AddValue("leftOver", new byte[8]);
            }

            internal override bool HasState
            {
                get
                {
                    return (this.bLeftOver1 >= 0);
                }
            }
        }
    }
}

