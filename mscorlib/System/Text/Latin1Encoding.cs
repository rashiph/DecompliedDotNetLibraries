namespace System.Text
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class Latin1Encoding : EncodingNLS, ISerializable
    {
        private static readonly char[] arrayCharBestFit = new char[] { 
            'Ā', 'A', 'ā', 'a', 'Ă', 'A', 'ă', 'a', 'Ą', 'A', 'ą', 'a', 'Ć', 'C', 'ć', 'c', 
            'Ĉ', 'C', 'ĉ', 'c', 'Ċ', 'C', 'ċ', 'c', 'Č', 'C', 'č', 'c', 'Ď', 'D', 'ď', 'd', 
            'Đ', 'D', 'đ', 'd', 'Ē', 'E', 'ē', 'e', 'Ĕ', 'E', 'ĕ', 'e', 'Ė', 'E', 'ė', 'e', 
            'Ę', 'E', 'ę', 'e', 'Ě', 'E', 'ě', 'e', 'Ĝ', 'G', 'ĝ', 'g', 'Ğ', 'G', 'ğ', 'g', 
            'Ġ', 'G', 'ġ', 'g', 'Ģ', 'G', 'ģ', 'g', 'Ĥ', 'H', 'ĥ', 'h', 'Ħ', 'H', 'ħ', 'h', 
            'Ĩ', 'I', 'ĩ', 'i', 'Ī', 'I', 'ī', 'i', 'Ĭ', 'I', 'ĭ', 'i', 'Į', 'I', 'į', 'i', 
            'İ', 'I', 'ı', 'i', 'Ĵ', 'J', 'ĵ', 'j', 'Ķ', 'K', 'ķ', 'k', 'Ĺ', 'L', 'ĺ', 'l', 
            'Ļ', 'L', 'ļ', 'l', 'Ľ', 'L', 'ľ', 'l', 'Ł', 'L', 'ł', 'l', 'Ń', 'N', 'ń', 'n', 
            'Ņ', 'N', 'ņ', 'n', 'Ň', 'N', 'ň', 'n', 'Ō', 'O', 'ō', 'o', 'Ŏ', 'O', 'ŏ', 'o', 
            'Ő', 'O', 'ő', 'o', 'Œ', 'O', 'œ', 'o', 'Ŕ', 'R', 'ŕ', 'r', 'Ŗ', 'R', 'ŗ', 'r', 
            'Ř', 'R', 'ř', 'r', 'Ś', 'S', 'ś', 's', 'Ŝ', 'S', 'ŝ', 's', 'Ş', 'S', 'ş', 's', 
            'Š', 'S', 'š', 's', 'Ţ', 'T', 'ţ', 't', 'Ť', 'T', 'ť', 't', 'Ŧ', 'T', 'ŧ', 't', 
            'Ũ', 'U', 'ũ', 'u', 'Ū', 'U', 'ū', 'u', 'Ŭ', 'U', 'ŭ', 'u', 'Ů', 'U', 'ů', 'u', 
            'Ű', 'U', 'ű', 'u', 'Ų', 'U', 'ų', 'u', 'Ŵ', 'W', 'ŵ', 'w', 'Ŷ', 'Y', 'ŷ', 'y', 
            'Ÿ', 'Y', 'Ź', 'Z', 'ź', 'z', 'Ż', 'Z', 'ż', 'z', 'Ž', 'Z', 'ž', 'z', 'ƀ', 'b', 
            'Ɖ', 'D', 'Ƒ', 'F', 'ƒ', 'f', 'Ɨ', 'I', 'ƚ', 'l', 'Ɵ', 'O', 'Ơ', 'O', 'ơ', 'o', 
            'ƫ', 't', 'Ʈ', 'T', 'Ư', 'U', 'ư', 'u', 'ƶ', 'z', 'Ǎ', 'A', 'ǎ', 'a', 'Ǐ', 'I', 
            'ǐ', 'i', 'Ǒ', 'O', 'ǒ', 'o', 'Ǔ', 'U', 'ǔ', 'u', 'Ǖ', 'U', 'ǖ', 'u', 'Ǘ', 'U', 
            'ǘ', 'u', 'Ǚ', 'U', 'ǚ', 'u', 'Ǜ', 'U', 'ǜ', 'u', 'Ǟ', 'A', 'ǟ', 'a', 'Ǥ', 'G', 
            'ǥ', 'g', 'Ǧ', 'G', 'ǧ', 'g', 'Ǩ', 'K', 'ǩ', 'k', 'Ǫ', 'O', 'ǫ', 'o', 'Ǭ', 'O', 
            'ǭ', 'o', 'ǰ', 'j', 'ɡ', 'g', 'ʹ', '\'', 'ʺ', '"', 'ʼ', '\'', '˄', '^', 'ˆ', '^', 
            'ˈ', '\'', 'ˉ', '?', 'ˊ', '?', 'ˋ', '`', 'ˍ', '_', '˚', '?', '˜', '~', '̀', '`', 
            '̂', '^', '̃', '~', '̎', '"', '̱', '_', '̲', '_', ' ', ' ', ' ', ' ', ' ', ' ', 
            ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', '‐', '-', '‑', '-', '–', '-', '—', '-', 
            '‘', '\'', '’', '\'', '‚', ',', '“', '"', '”', '"', '„', '"', '†', '?', '‡', '?', 
            '•', '.', '…', '.', '‰', '?', '′', '\'', '‵', '`', '‹', '<', '›', '>', '™', 'T', 
            '！', '!', '＂', '"', '＃', '#', '＄', '$', '％', '%', '＆', '&', '＇', '\'', '（', '(', 
            '）', ')', '＊', '*', '＋', '+', '，', ',', '－', '-', '．', '.', '／', '/', '０', '0', 
            '１', '1', '２', '2', '３', '3', '４', '4', '５', '5', '６', '6', '７', '7', '８', '8', 
            '９', '9', '：', ':', '；', ';', '＜', '<', '＝', '=', '＞', '>', '？', '?', '＠', '@', 
            'Ａ', 'A', 'Ｂ', 'B', 'Ｃ', 'C', 'Ｄ', 'D', 'Ｅ', 'E', 'Ｆ', 'F', 'Ｇ', 'G', 'Ｈ', 'H', 
            'Ｉ', 'I', 'Ｊ', 'J', 'Ｋ', 'K', 'Ｌ', 'L', 'Ｍ', 'M', 'Ｎ', 'N', 'Ｏ', 'O', 'Ｐ', 'P', 
            'Ｑ', 'Q', 'Ｒ', 'R', 'Ｓ', 'S', 'Ｔ', 'T', 'Ｕ', 'U', 'Ｖ', 'V', 'Ｗ', 'W', 'Ｘ', 'X', 
            'Ｙ', 'Y', 'Ｚ', 'Z', '［', '[', '＼', '\\', '］', ']', '＾', '^', '＿', '_', '｀', '`', 
            'ａ', 'a', 'ｂ', 'b', 'ｃ', 'c', 'ｄ', 'd', 'ｅ', 'e', 'ｆ', 'f', 'ｇ', 'g', 'ｈ', 'h', 
            'ｉ', 'i', 'ｊ', 'j', 'ｋ', 'k', 'ｌ', 'l', 'ｍ', 'm', 'ｎ', 'n', 'ｏ', 'o', 'ｐ', 'p', 
            'ｑ', 'q', 'ｒ', 'r', 'ｓ', 's', 'ｔ', 't', 'ｕ', 'u', 'ｖ', 'v', 'ｗ', 'w', 'ｘ', 'x', 
            'ｙ', 'y', 'ｚ', 'z', '｛', '{', '｜', '|', '｝', '}', '～', '~'
         };

        public Latin1Encoding() : base(0x6faf)
        {
        }

        internal Latin1Encoding(SerializationInfo info, StreamingContext context) : base(0x6faf)
        {
            base.DeserializeEncoding(info, context);
        }

        internal override char[] GetBestFitUnicodeToBytesData()
        {
            return arrayCharBestFit;
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int charCount, EncoderNLS encoder)
        {
            EncoderReplacementFallback encoderFallback;
            char ch2;
            char charLeftOver = '\0';
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                encoderFallback = encoder.Fallback as EncoderReplacementFallback;
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
            char* charEnd = chars + charCount;
            EncoderFallbackBuffer fallbackBuffer = null;
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
                if (ch2 > '\x00ff')
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

        [SecurityCritical]
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
        {
            char ch4;
            char charLeftOver = '\0';
            EncoderReplacementFallback encoderFallback = null;
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                encoderFallback = encoder.Fallback as EncoderReplacementFallback;
            }
            else
            {
                encoderFallback = base.EncoderFallback as EncoderReplacementFallback;
            }
            char* charEnd = chars + charCount;
            byte* numPtr = bytes;
            char* chPtr2 = chars;
            if ((encoderFallback != null) && (encoderFallback.MaxCharCount == 1))
            {
                char ch2 = encoderFallback.DefaultString[0];
                if (ch2 <= '\x00ff')
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
                        if (ch3 > '\x00ff')
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
                        encoder.m_charsUsed = (int) ((long) ((chars - chPtr2) / 2));
                    }
                    return (int) ((long) ((bytes - numPtr) / 1));
                }
            }
            byte* numPtr2 = bytes + byteCount;
            EncoderFallbackBuffer fallbackBuffer = null;
            if (charLeftOver > '\0')
            {
                fallbackBuffer = encoder.FallbackBuffer;
                fallbackBuffer.InternalInitialize(chars, charEnd, encoder, true);
                fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                if (fallbackBuffer.Remaining > ((long) ((numPtr2 - bytes) / 1)))
                {
                    base.ThrowBytesOverflow(encoder, true);
                }
            }
            while (((ch4 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch4 == '\0')
                {
                    ch4 = chars[0];
                    chars++;
                }
                if (ch4 > '\x00ff')
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
                    if (fallbackBuffer.Remaining <= ((long) ((numPtr2 - bytes) / 1)))
                    {
                        continue;
                    }
                    chars--;
                    fallbackBuffer.InternalReset();
                    base.ThrowBytesOverflow(encoder, chars == chPtr2);
                    break;
                }
                if (bytes >= numPtr2)
                {
                    if ((fallbackBuffer == null) || !fallbackBuffer.bFallingBack)
                    {
                        chars--;
                    }
                    base.ThrowBytesOverflow(encoder, chars == chPtr2);
                    break;
                }
                bytes[0] = (byte) ch4;
                bytes++;
            }
            if (encoder != null)
            {
                if ((fallbackBuffer != null) && !fallbackBuffer.bUsedEncoder)
                {
                    encoder.charLeftOver = '\0';
                }
                encoder.m_charsUsed = (int) ((long) ((chars - chPtr2) / 2));
            }
            return (int) ((long) ((bytes - numPtr) / 1));
        }

        [SecurityCritical]
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS decoder)
        {
            return count;
        }

        [SecurityCritical]
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS decoder)
        {
            if (charCount < byteCount)
            {
                base.ThrowCharsOverflow(decoder, charCount < 1);
                byteCount = charCount;
            }
            byte* numPtr = bytes + byteCount;
            while (bytes < numPtr)
            {
                chars[0] = (char) bytes[0];
                chars++;
                bytes++;
            }
            if (decoder != null)
            {
                decoder.m_bytesUsed = byteCount;
            }
            return byteCount;
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

        public override bool IsAlwaysNormalized(NormalizationForm form)
        {
            return (form == NormalizationForm.FormC);
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.SerializeEncoding(info, context);
            info.AddValue("CodePageEncoding+maxCharSize", 1);
            info.AddValue("CodePageEncoding+m_codePage", this.CodePage);
            info.AddValue("CodePageEncoding+dataItem", null);
        }

        public override bool IsSingleByte
        {
            get
            {
                return true;
            }
        }
    }
}

