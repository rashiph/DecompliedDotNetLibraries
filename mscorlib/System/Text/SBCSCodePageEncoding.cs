namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class SBCSCodePageEncoding : BaseCodePageEncoding, ISerializable
    {
        [NonSerialized]
        private byte byteUnknown;
        [NonSerialized]
        private char charUnknown;
        [NonSerialized]
        private unsafe char* mapBytesToUnicode;
        [NonSerialized]
        private unsafe int* mapCodePageCached;
        [NonSerialized]
        private unsafe byte* mapUnicodeToBytes;
        private static object s_InternalSyncObject;
        private const char UNKNOWN_CHAR = '�';

        [SecurityCritical]
        public SBCSCodePageEncoding(int codePage) : this(codePage, codePage)
        {
        }

        [SecurityCritical]
        internal unsafe SBCSCodePageEncoding(int codePage, int dataCodePage) : base(codePage, dataCodePage)
        {
            this.mapBytesToUnicode = null;
            this.mapUnicodeToBytes = null;
            this.mapCodePageCached = null;
        }

        [SecurityCritical]
        internal unsafe SBCSCodePageEncoding(SerializationInfo info, StreamingContext context) : base(0)
        {
            this.mapBytesToUnicode = null;
            this.mapUnicodeToBytes = null;
            this.mapCodePageCached = null;
            throw new ArgumentNullException("this");
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS encoder)
        {
            char ch2;
            base.CheckMemorySection();
            EncoderReplacementFallback encoderFallback = null;
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
                    count++;
                }
                return count;
            }
            EncoderFallbackBuffer fallbackBuffer = null;
            int num = 0;
            char* charEnd = chars + count;
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
                if ((this.mapUnicodeToBytes[ch2] == 0) && (ch2 != '\0'))
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
                        fallbackBuffer.InternalInitialize(charEnd - count, charEnd, encoder, false);
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
            char ch3;
            base.CheckMemorySection();
            EncoderReplacementFallback encoderFallback = null;
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
            char* charEnd = chars + charCount;
            byte* numPtr = bytes;
            char* chPtr2 = chars;
            if ((encoderFallback != null) && (encoderFallback.MaxCharCount == 1))
            {
                byte num = this.mapUnicodeToBytes[(int) ((byte*) encoderFallback.DefaultString[0])];
                if (num != 0)
                {
                    if (charLeftOver > '\0')
                    {
                        if (byteCount == 0)
                        {
                            base.ThrowBytesOverflow(encoder, true);
                        }
                        bytes++;
                        bytes[0] = num;
                        byteCount--;
                    }
                    if (byteCount < charCount)
                    {
                        base.ThrowBytesOverflow(encoder, byteCount < 1);
                        charEnd = chars + byteCount;
                    }
                    while (chars < charEnd)
                    {
                        char ch2 = chars[0];
                        chars++;
                        byte num2 = this.mapUnicodeToBytes[(int) ((byte*) ch2)];
                        if ((num2 == 0) && (ch2 != '\0'))
                        {
                            bytes[0] = num;
                        }
                        else
                        {
                            bytes[0] = num2;
                        }
                        bytes++;
                    }
                    if (encoder != null)
                    {
                        encoder.charLeftOver = '\0';
                        encoder.m_charsUsed = (int) ((long) ((chars - chPtr2) / 2));
                    }
                    return (int) ((long) ((bytes - numPtr) / 1));
                }
            }
            EncoderFallbackBuffer fallbackBuffer = null;
            byte* numPtr2 = bytes + byteCount;
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
            while (((ch3 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch3 == '\0')
                {
                    ch3 = chars[0];
                    chars++;
                }
                byte num3 = this.mapUnicodeToBytes[(int) ((byte*) ch3)];
                if ((num3 == 0) && (ch3 != '\0'))
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
                    fallbackBuffer.InternalFallback(ch3, ref chars);
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
                bytes[0] = num3;
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
            base.CheckMemorySection();
            bool isMicrosoftBestFitFallback = false;
            DecoderReplacementFallback decoderFallback = null;
            if (decoder == null)
            {
                decoderFallback = base.DecoderFallback as DecoderReplacementFallback;
                isMicrosoftBestFitFallback = base.DecoderFallback.IsMicrosoftBestFitFallback;
            }
            else
            {
                decoderFallback = decoder.Fallback as DecoderReplacementFallback;
                isMicrosoftBestFitFallback = decoder.Fallback.IsMicrosoftBestFitFallback;
            }
            if (isMicrosoftBestFitFallback || ((decoderFallback != null) && (decoderFallback.MaxCharCount == 1)))
            {
                return count;
            }
            DecoderFallbackBuffer fallbackBuffer = null;
            int num = count;
            byte[] buffer2 = new byte[1];
            byte* numPtr = bytes + count;
            while (bytes < numPtr)
            {
                char ch = this.mapBytesToUnicode[bytes[0]];
                bytes++;
                if (ch == 0xfffd)
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
                    buffer2[0] = *(bytes - 1);
                    num--;
                    num += fallbackBuffer.InternalFallback(buffer2, bytes);
                }
            }
            return num;
        }

        [SecurityCritical]
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS decoder)
        {
            base.CheckMemorySection();
            bool isMicrosoftBestFitFallback = false;
            byte* numPtr = bytes + byteCount;
            byte* numPtr2 = bytes;
            char* chPtr = chars;
            DecoderReplacementFallback decoderFallback = null;
            if (decoder == null)
            {
                decoderFallback = base.DecoderFallback as DecoderReplacementFallback;
                isMicrosoftBestFitFallback = base.DecoderFallback.IsMicrosoftBestFitFallback;
            }
            else
            {
                decoderFallback = decoder.Fallback as DecoderReplacementFallback;
                isMicrosoftBestFitFallback = decoder.Fallback.IsMicrosoftBestFitFallback;
            }
            if (isMicrosoftBestFitFallback || ((decoderFallback != null) && (decoderFallback.MaxCharCount == 1)))
            {
                char ch;
                if (decoderFallback == null)
                {
                    ch = '?';
                }
                else
                {
                    ch = decoderFallback.DefaultString[0];
                }
                if (charCount < byteCount)
                {
                    base.ThrowCharsOverflow(decoder, charCount < 1);
                    numPtr = bytes + charCount;
                }
                while (bytes < numPtr)
                {
                    char ch2;
                    if (isMicrosoftBestFitFallback)
                    {
                        if (base.arrayBytesBestFit == null)
                        {
                            this.ReadBestFitTable();
                        }
                        ch2 = base.arrayBytesBestFit[bytes[0]];
                    }
                    else
                    {
                        ch2 = this.mapBytesToUnicode[bytes[0]];
                    }
                    bytes++;
                    if (ch2 == 0xfffd)
                    {
                        chars[0] = ch;
                    }
                    else
                    {
                        chars[0] = ch2;
                    }
                    chars++;
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
                char ch3 = this.mapBytesToUnicode[bytes[0]];
                bytes++;
                if (ch3 == 0xfffd)
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
                    buffer2[0] = *(bytes - 1);
                    if (fallbackBuffer.InternalFallback(buffer2, bytes, ref chars))
                    {
                        continue;
                    }
                    bytes--;
                    fallbackBuffer.InternalReset();
                    base.ThrowCharsOverflow(decoder, bytes == numPtr2);
                    break;
                }
                if (chars >= charEnd)
                {
                    bytes--;
                    base.ThrowCharsOverflow(decoder, bytes == numPtr2);
                    break;
                }
                chars[0] = ch3;
                chars++;
            }
            if (decoder != null)
            {
                decoder.m_bytesUsed = (int) ((long) ((bytes - numPtr2) / 1));
            }
            return (int) ((long) ((chars - chPtr) / 2));
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

        [ComVisible(false)]
        public override bool IsAlwaysNormalized(NormalizationForm form)
        {
            if (form == NormalizationForm.FormC)
            {
                switch (this.CodePage)
                {
                    case 720:
                    case 0x2e1:
                    case 0x307:
                    case 0x25:
                    case 0x1b5:
                    case 500:
                    case 850:
                    case 0x354:
                    case 0x357:
                    case 0x35a:
                    case 860:
                    case 0x35d:
                    case 0x35e:
                    case 0x35f:
                    case 0x361:
                    case 0x362:
                    case 0x365:
                    case 870:
                    case 0x402:
                    case 0x417:
                    case 0x474:
                    case 0x475:
                    case 0x476:
                    case 0x477:
                    case 0x478:
                    case 0x479:
                    case 0x47a:
                    case 0x47b:
                    case 0x47c:
                    case 0x47d:
                    case 0x4e2:
                    case 0x4e3:
                    case 0x4e4:
                    case 0x4e6:
                    case 0x4e8:
                    case 0x2717:
                    case 0x4f35:
                    case 0x4f36:
                    case 0x4f38:
                    case 0x4f3c:
                    case 0x4f3d:
                    case 0x4f49:
                    case 0x5182:
                    case 0x2721:
                    case 0x272d:
                    case 0x4f31:
                    case 0x5187:
                    case 0x5190:
                    case 0x51bc:
                    case 0x5221:
                    case 0x556a:
                    case 0x6faf:
                    case 0x6fb0:
                    case 0x6fb2:
                    case 0x6fb3:
                    case 0x6fb7:
                    case 0x6fbb:
                    case 0x6fbd:
                        return true;
                }
            }
            return false;
        }

        [SecurityCritical]
        protected override unsafe void LoadManagedCodePage()
        {
            if (base.pCodePage.ByteCount != 1)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoCodepageData", new object[] { this.CodePage }));
            }
            this.byteUnknown = (byte) base.pCodePage.ByteReplace;
            this.charUnknown = base.pCodePage.UnicodeReplace;
            byte* sharedMemory = base.GetSharedMemory(0x10204 + base.iExtraBytes);
            this.mapBytesToUnicode = (char*) sharedMemory;
            this.mapUnicodeToBytes = sharedMemory + 0x200;
            this.mapCodePageCached = (int*) (((sharedMemory + 0x200) + 0x10000) + base.iExtraBytes);
            if (this.mapCodePageCached[0] != 0)
            {
                if (this.mapCodePageCached[0] != base.dataTableCodePage)
                {
                    throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
                }
            }
            else
            {
                char* chPtr = (char*) &base.pCodePage.FirstDataWord;
                for (int i = 0; i < 0x100; i++)
                {
                    if ((chPtr[i] != '\0') || (i == 0))
                    {
                        this.mapBytesToUnicode[i] = chPtr[i];
                        if (chPtr[i] != 0xfffd)
                        {
                            this.mapUnicodeToBytes[chPtr[i]] = (byte) i;
                        }
                    }
                    else
                    {
                        this.mapBytesToUnicode[i] = 0xfffd;
                    }
                }
                this.mapCodePageCached[0] = base.dataTableCodePage;
            }
        }

        [SecurityCritical]
        protected override unsafe void ReadBestFitTable()
        {
            lock (InternalSyncObject)
            {
                if (base.arrayUnicodeBestFit == null)
                {
                    ushort num2;
                    byte* numPtr = (byte*) &base.pCodePage.FirstDataWord;
                    numPtr += 0x200;
                    char[] chArray = new char[0x100];
                    for (int i = 0; i < 0x100; i++)
                    {
                        chArray[i] = this.mapBytesToUnicode[i];
                    }
                    while ((num2 = *((ushort*) numPtr)) != 0)
                    {
                        numPtr += 2;
                        chArray[num2] = *((char*) numPtr);
                        numPtr += 2;
                    }
                    base.arrayBytesBestFit = chArray;
                    numPtr += 2;
                    byte* numPtr2 = numPtr;
                    int num3 = 0;
                    int num4 = *((ushort*) numPtr);
                    numPtr += 2;
                    while (num4 < 0x10000)
                    {
                        byte num5 = numPtr[0];
                        numPtr++;
                        if (num5 == 1)
                        {
                            num4 = *((ushort*) numPtr);
                            numPtr += 2;
                        }
                        else
                        {
                            if (((num5 < 0x20) && (num5 > 0)) && (num5 != 30))
                            {
                                num4 += num5;
                                continue;
                            }
                            if (num5 > 0)
                            {
                                num3++;
                            }
                            num4++;
                        }
                    }
                    chArray = new char[num3 * 2];
                    numPtr = numPtr2;
                    num4 = *((ushort*) numPtr);
                    numPtr += 2;
                    num3 = 0;
                    while (num4 < 0x10000)
                    {
                        byte index = numPtr[0];
                        numPtr++;
                        if (index == 1)
                        {
                            num4 = *((ushort*) numPtr);
                            numPtr += 2;
                        }
                        else
                        {
                            if (((index < 0x20) && (index > 0)) && (index != 30))
                            {
                                num4 += index;
                                continue;
                            }
                            if (index == 30)
                            {
                                index = numPtr[0];
                                numPtr++;
                            }
                            if (index > 0)
                            {
                                chArray[num3++] = (char) num4;
                                chArray[num3++] = this.mapBytesToUnicode[index];
                            }
                            num4++;
                        }
                    }
                    base.arrayUnicodeBestFit = chArray;
                }
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange<object>(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
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

