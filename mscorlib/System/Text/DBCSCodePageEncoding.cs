namespace System.Text
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Threading;

    [Serializable]
    internal class DBCSCodePageEncoding : BaseCodePageEncoding, ISerializable
    {
        [NonSerialized]
        private int byteCountUnknown;
        [NonSerialized]
        private ushort bytesUnknown;
        [NonSerialized]
        protected char charUnknown;
        [NonSerialized]
        protected const char LEAD_BYTE_CHAR = '￾';
        [NonSerialized]
        protected unsafe char* mapBytesToUnicode;
        [NonSerialized]
        protected unsafe int* mapCodePageCached;
        [NonSerialized]
        protected unsafe ushort* mapUnicodeToBytes;
        private static object s_InternalSyncObject;
        [NonSerialized]
        protected const char UNICODE_REPLACEMENT_CHAR = '�';
        [NonSerialized]
        protected const char UNKNOWN_CHAR_FLAG = '\0';

        [SecurityCritical]
        public DBCSCodePageEncoding(int codePage) : this(codePage, codePage)
        {
        }

        [SecurityCritical]
        internal unsafe DBCSCodePageEncoding(int codePage, int dataCodePage) : base(codePage, dataCodePage)
        {
            this.mapBytesToUnicode = null;
            this.mapUnicodeToBytes = null;
            this.mapCodePageCached = null;
        }

        [SecurityCritical]
        internal unsafe DBCSCodePageEncoding(SerializationInfo info, StreamingContext context) : base(0)
        {
            this.mapBytesToUnicode = null;
            this.mapUnicodeToBytes = null;
            this.mapCodePageCached = null;
            throw new ArgumentNullException("this");
        }

        protected virtual bool CleanUpBytes(ref int bytes)
        {
            return true;
        }

        [SecurityCritical]
        protected virtual unsafe void CleanUpEndBytes(char* chars)
        {
        }

        [SecurityCritical]
        internal override unsafe int GetByteCount(char* chars, int count, EncoderNLS encoder)
        {
            char ch2;
            base.CheckMemorySection();
            char charLeftOver = '\0';
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                if (encoder.InternalHasFallbackBuffer && (encoder.FallbackBuffer.Remaining > 0))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                }
            }
            int num = 0;
            char* charEnd = chars + count;
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
                ushort num2 = this.mapUnicodeToBytes[ch2];
                if ((num2 == 0) && (ch2 != '\0'))
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
                    if (num2 >= 0x100)
                    {
                        num++;
                    }
                }
            }
            return num;
        }

        [SecurityCritical]
        internal override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, EncoderNLS encoder)
        {
            char ch2;
            base.CheckMemorySection();
            EncoderFallbackBuffer fallbackBuffer = null;
            char* charEnd = chars + charCount;
            char* chPtr2 = chars;
            byte* numPtr = bytes;
            byte* numPtr2 = bytes + byteCount;
            char charLeftOver = '\0';
            if (encoder != null)
            {
                charLeftOver = encoder.charLeftOver;
                fallbackBuffer = encoder.FallbackBuffer;
                fallbackBuffer.InternalInitialize(chars, charEnd, encoder, true);
                if (encoder.m_throwOnOverflow && (fallbackBuffer.Remaining > 0))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_EncoderFallbackNotEmpty", new object[] { this.EncodingName, encoder.Fallback.GetType() }));
                }
                if (charLeftOver > '\0')
                {
                    fallbackBuffer.InternalFallback(charLeftOver, ref chars);
                }
            }
            while (((ch2 = (fallbackBuffer == null) ? '\0' : fallbackBuffer.InternalGetNextChar()) != '\0') || (chars < charEnd))
            {
                if (ch2 == '\0')
                {
                    ch2 = chars[0];
                    chars++;
                }
                ushort num = this.mapUnicodeToBytes[ch2];
                if ((num == 0) && (ch2 != '\0'))
                {
                    if (fallbackBuffer == null)
                    {
                        fallbackBuffer = base.encoderFallback.CreateFallbackBuffer();
                        fallbackBuffer.InternalInitialize(charEnd - charCount, charEnd, encoder, true);
                    }
                    fallbackBuffer.InternalFallback(ch2, ref chars);
                }
                else
                {
                    if (num >= 0x100)
                    {
                        if ((bytes + 1) >= numPtr2)
                        {
                            if ((fallbackBuffer == null) || !fallbackBuffer.bFallingBack)
                            {
                                chars--;
                            }
                            else
                            {
                                fallbackBuffer.MovePrevious();
                            }
                            base.ThrowBytesOverflow(encoder, chars == chPtr2);
                            break;
                        }
                        bytes[0] = (byte) (num >> 8);
                        bytes++;
                    }
                    else if (bytes >= numPtr2)
                    {
                        if ((fallbackBuffer == null) || !fallbackBuffer.bFallingBack)
                        {
                            chars--;
                        }
                        else
                        {
                            fallbackBuffer.MovePrevious();
                        }
                        base.ThrowBytesOverflow(encoder, chars == chPtr2);
                        break;
                    }
                    bytes[0] = (byte) (num & 0xff);
                    bytes++;
                }
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
        internal override unsafe int GetCharCount(byte* bytes, int count, DecoderNLS baseDecoder)
        {
            base.CheckMemorySection();
            DBCSDecoder decoder = (DBCSDecoder) baseDecoder;
            DecoderFallbackBuffer fallbackBuffer = null;
            byte* numPtr = bytes + count;
            int num = count;
            if ((decoder != null) && (decoder.bLeftOver > 0))
            {
                if (count == 0)
                {
                    if (!decoder.MustFlush)
                    {
                        return 0;
                    }
                    fallbackBuffer = decoder.FallbackBuffer;
                    fallbackBuffer.InternalInitialize(bytes, null);
                    byte[] buffer2 = new byte[] { decoder.bLeftOver };
                    return fallbackBuffer.InternalFallback(buffer2, bytes);
                }
                int index = decoder.bLeftOver << 8;
                index |= bytes[0];
                bytes++;
                if ((this.mapBytesToUnicode[index] == '\0') && (index != 0))
                {
                    num--;
                    fallbackBuffer = decoder.FallbackBuffer;
                    fallbackBuffer.InternalInitialize(numPtr - count, null);
                    byte[] buffer3 = new byte[] { (byte) (index >> 8), (byte) index };
                    num += fallbackBuffer.InternalFallback(buffer3, bytes);
                }
            }
            while (bytes < numPtr)
            {
                int num3 = bytes[0];
                bytes++;
                char ch2 = this.mapBytesToUnicode[num3];
                if (ch2 == 0xfffe)
                {
                    num--;
                    if (bytes < numPtr)
                    {
                        num3 = num3 << 8;
                        num3 |= bytes[0];
                        bytes++;
                        ch2 = this.mapBytesToUnicode[num3];
                    }
                    else
                    {
                        if ((decoder != null) && !decoder.MustFlush)
                        {
                            return num;
                        }
                        num++;
                        ch2 = '\0';
                    }
                }
                if ((ch2 == '\0') && (num3 != 0))
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
                    num--;
                    byte[] buffer4 = null;
                    if (num3 < 0x100)
                    {
                        buffer4 = new byte[] { (byte) num3 };
                    }
                    else
                    {
                        buffer4 = new byte[] { (byte) (num3 >> 8), (byte) num3 };
                    }
                    num += fallbackBuffer.InternalFallback(buffer4, bytes);
                }
            }
            return num;
        }

        [SecurityCritical]
        internal override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, DecoderNLS baseDecoder)
        {
            base.CheckMemorySection();
            DBCSDecoder decoder = (DBCSDecoder) baseDecoder;
            byte* numPtr = bytes;
            byte* numPtr2 = bytes + byteCount;
            char* chPtr = chars;
            char* charEnd = chars + charCount;
            bool flag = false;
            DecoderFallbackBuffer fallbackBuffer = null;
            if ((decoder != null) && (decoder.bLeftOver > 0))
            {
                if (byteCount == 0)
                {
                    if (!decoder.MustFlush)
                    {
                        return 0;
                    }
                    fallbackBuffer = decoder.FallbackBuffer;
                    fallbackBuffer.InternalInitialize(bytes, charEnd);
                    byte[] buffer2 = new byte[] { decoder.bLeftOver };
                    if (!fallbackBuffer.InternalFallback(buffer2, bytes, ref chars))
                    {
                        base.ThrowCharsOverflow(decoder, true);
                    }
                    decoder.bLeftOver = 0;
                    return (int) ((long) ((chars - chPtr) / 2));
                }
                int index = decoder.bLeftOver << 8;
                index |= bytes[0];
                bytes++;
                char ch = this.mapBytesToUnicode[index];
                if ((ch == '\0') && (index != 0))
                {
                    fallbackBuffer = decoder.FallbackBuffer;
                    fallbackBuffer.InternalInitialize(numPtr2 - byteCount, charEnd);
                    byte[] buffer3 = new byte[] { (byte) (index >> 8), (byte) index };
                    if (!fallbackBuffer.InternalFallback(buffer3, bytes, ref chars))
                    {
                        base.ThrowCharsOverflow(decoder, true);
                    }
                }
                else
                {
                    if (chars >= charEnd)
                    {
                        base.ThrowCharsOverflow(decoder, true);
                    }
                    chars++;
                    chars[0] = ch;
                }
            }
            while (bytes < numPtr2)
            {
                int num2 = bytes[0];
                bytes++;
                char ch2 = this.mapBytesToUnicode[num2];
                if (ch2 == 0xfffe)
                {
                    if (bytes < numPtr2)
                    {
                        num2 = num2 << 8;
                        num2 |= bytes[0];
                        bytes++;
                        ch2 = this.mapBytesToUnicode[num2];
                    }
                    else if ((decoder == null) || decoder.MustFlush)
                    {
                        ch2 = '\0';
                    }
                    else
                    {
                        flag = true;
                        decoder.bLeftOver = (byte) num2;
                        break;
                    }
                }
                if ((ch2 == '\0') && (num2 != 0))
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
                        fallbackBuffer.InternalInitialize(numPtr2 - byteCount, charEnd);
                    }
                    byte[] buffer4 = null;
                    if (num2 < 0x100)
                    {
                        buffer4 = new byte[] { (byte) num2 };
                    }
                    else
                    {
                        buffer4 = new byte[] { (byte) (num2 >> 8), (byte) num2 };
                    }
                    if (fallbackBuffer.InternalFallback(buffer4, bytes, ref chars))
                    {
                        continue;
                    }
                    bytes -= buffer4.Length;
                    fallbackBuffer.InternalReset();
                    base.ThrowCharsOverflow(decoder, bytes == numPtr);
                    break;
                }
                if (chars >= charEnd)
                {
                    bytes--;
                    if (num2 >= 0x100)
                    {
                        bytes--;
                    }
                    base.ThrowCharsOverflow(decoder, bytes == numPtr);
                    break;
                }
                chars++;
                chars[0] = ch2;
            }
            if (decoder != null)
            {
                if (!flag)
                {
                    decoder.bLeftOver = 0;
                }
                decoder.m_bytesUsed = (int) ((long) ((bytes - numPtr) / 1));
            }
            return (int) ((long) ((chars - chPtr) / 2));
        }

        public override System.Text.Decoder GetDecoder()
        {
            return new DBCSDecoder(this);
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
            num *= 2L;
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

        [SecurityCritical]
        protected override unsafe void LoadManagedCodePage()
        {
            if (base.pCodePage.ByteCount != 2)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_NoCodepageData", new object[] { this.CodePage }));
            }
            this.bytesUnknown = base.pCodePage.ByteReplace;
            this.charUnknown = base.pCodePage.UnicodeReplace;
            if (base.DecoderFallback.IsMicrosoftBestFitFallback)
            {
                ((InternalDecoderBestFitFallback) base.DecoderFallback).cReplacement = this.charUnknown;
            }
            this.byteCountUnknown = 1;
            if (this.bytesUnknown > 0xff)
            {
                this.byteCountUnknown++;
            }
            byte* sharedMemory = base.GetSharedMemory(0x40004 + base.iExtraBytes);
            this.mapBytesToUnicode = (char*) sharedMemory;
            this.mapUnicodeToBytes = (ushort*) (sharedMemory + 0x20000);
            this.mapCodePageCached = (int*) ((sharedMemory + 0x40000) + base.iExtraBytes);
            if (this.mapCodePageCached[0] != 0)
            {
                if (((this.mapCodePageCached[0] != base.dataTableCodePage) && base.bFlagDataTable) || ((this.mapCodePageCached[0] != this.CodePage) && !base.bFlagDataTable))
                {
                    throw new OutOfMemoryException(Environment.GetResourceString("Arg_OutOfMemoryException"));
                }
            }
            else
            {
                char* chPtr = (char*) &base.pCodePage.FirstDataWord;
                int num = 0;
                int bytes = 0;
                while (num < 0x10000)
                {
                    char index = chPtr[0];
                    chPtr++;
                    if (index == '\x0001')
                    {
                        num = chPtr[0];
                        chPtr++;
                    }
                    else
                    {
                        if ((index < ' ') && (index > '\0'))
                        {
                            num += index;
                            continue;
                        }
                        if (index == 0xffff)
                        {
                            bytes = num;
                            index = (char) num;
                        }
                        else if (index == 0xfffe)
                        {
                            bytes = num;
                        }
                        else
                        {
                            if (index == 0xfffd)
                            {
                                num++;
                                continue;
                            }
                            bytes = num;
                        }
                        if (this.CleanUpBytes(ref bytes))
                        {
                            if (index != 0xfffe)
                            {
                                this.mapUnicodeToBytes[index] = (ushort) bytes;
                            }
                            this.mapBytesToUnicode[bytes] = index;
                        }
                        num++;
                    }
                }
                this.CleanUpEndBytes(this.mapBytesToUnicode);
                if (base.bFlagDataTable)
                {
                    this.mapCodePageCached[0] = base.dataTableCodePage;
                }
            }
        }

        [SecurityCritical]
        protected override unsafe void ReadBestFitTable()
        {
            lock (InternalSyncObject)
            {
                if (base.arrayUnicodeBestFit == null)
                {
                    char* chPtr = (char*) &base.pCodePage.FirstDataWord;
                    int num = 0;
                    while (num < 0x10000)
                    {
                        char ch = chPtr[0];
                        chPtr++;
                        if (ch == '\x0001')
                        {
                            num = chPtr[0];
                            chPtr++;
                        }
                        else
                        {
                            if ((ch < ' ') && (ch > '\0'))
                            {
                                num += ch;
                                continue;
                            }
                            num++;
                        }
                    }
                    char* chPtr2 = chPtr;
                    int num2 = 0;
                    num = chPtr[0];
                    chPtr++;
                    while (num < 0x10000)
                    {
                        char ch2 = chPtr[0];
                        chPtr++;
                        if (ch2 == '\x0001')
                        {
                            num = chPtr[0];
                            chPtr++;
                        }
                        else
                        {
                            if ((ch2 < ' ') && (ch2 > '\0'))
                            {
                                num += ch2;
                                continue;
                            }
                            if (ch2 != 0xfffd)
                            {
                                int bytes = num;
                                if (this.CleanUpBytes(ref bytes) && (this.mapBytesToUnicode[bytes] != ch2))
                                {
                                    num2++;
                                }
                            }
                            num++;
                        }
                    }
                    char[] chArray = new char[num2 * 2];
                    num2 = 0;
                    chPtr = chPtr2;
                    num = chPtr[0];
                    chPtr++;
                    bool flag = false;
                    while (num < 0x10000)
                    {
                        char ch3 = chPtr[0];
                        chPtr++;
                        if (ch3 == '\x0001')
                        {
                            num = chPtr[0];
                            chPtr++;
                        }
                        else
                        {
                            if ((ch3 < ' ') && (ch3 > '\0'))
                            {
                                num += ch3;
                                continue;
                            }
                            if (ch3 != 0xfffd)
                            {
                                int num4 = num;
                                if (this.CleanUpBytes(ref num4) && (this.mapBytesToUnicode[num4] != ch3))
                                {
                                    if (num4 != num)
                                    {
                                        flag = true;
                                    }
                                    chArray[num2++] = (char) num4;
                                    chArray[num2++] = ch3;
                                }
                            }
                            num++;
                        }
                    }
                    if (flag)
                    {
                        for (int i = 0; i < (chArray.Length - 2); i += 2)
                        {
                            int index = i;
                            char ch4 = chArray[i];
                            for (int j = i + 2; j < chArray.Length; j += 2)
                            {
                                if (ch4 > chArray[j])
                                {
                                    ch4 = chArray[j];
                                    index = j;
                                }
                            }
                            if (index != i)
                            {
                                char ch5 = chArray[index];
                                chArray[index] = chArray[i];
                                chArray[i] = ch5;
                                ch5 = chArray[index + 1];
                                chArray[index + 1] = chArray[i + 1];
                                chArray[i + 1] = ch5;
                            }
                        }
                    }
                    base.arrayBytesBestFit = chArray;
                    char* chPtr3 = chPtr;
                    chPtr++;
                    int num8 = chPtr[0];
                    num2 = 0;
                    while (num8 < 0x10000)
                    {
                        char ch6 = chPtr[0];
                        chPtr++;
                        if (ch6 == '\x0001')
                        {
                            num8 = chPtr[0];
                            chPtr++;
                        }
                        else
                        {
                            if ((ch6 < ' ') && (ch6 > '\0'))
                            {
                                num8 += ch6;
                                continue;
                            }
                            if (ch6 > '\0')
                            {
                                num2++;
                            }
                            num8++;
                        }
                    }
                    chArray = new char[num2 * 2];
                    chPtr = chPtr3;
                    chPtr++;
                    num8 = chPtr[0];
                    num2 = 0;
                    while (num8 < 0x10000)
                    {
                        char ch7 = chPtr[0];
                        chPtr++;
                        if (ch7 == '\x0001')
                        {
                            num8 = chPtr[0];
                            chPtr++;
                        }
                        else
                        {
                            if ((ch7 < ' ') && (ch7 > '\0'))
                            {
                                num8 += ch7;
                                continue;
                            }
                            if (ch7 > '\0')
                            {
                                int num9 = ch7;
                                if (this.CleanUpBytes(ref num9))
                                {
                                    chArray[num2++] = (char) num8;
                                    chArray[num2++] = this.mapBytesToUnicode[num9];
                                }
                            }
                            num8++;
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

        [Serializable]
        internal class DBCSDecoder : DecoderNLS
        {
            internal byte bLeftOver;

            public DBCSDecoder(DBCSCodePageEncoding encoding) : base(encoding)
            {
            }

            public override void Reset()
            {
                this.bLeftOver = 0;
                if (base.m_fallbackBuffer != null)
                {
                    base.m_fallbackBuffer.Reset();
                }
            }

            internal override bool HasState
            {
                get
                {
                    return (this.bLeftOver != 0);
                }
            }
        }
    }
}

