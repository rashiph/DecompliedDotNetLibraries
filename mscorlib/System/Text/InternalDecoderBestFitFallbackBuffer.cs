namespace System.Text
{
    using System;
    using System.Security;
    using System.Threading;

    internal sealed class InternalDecoderBestFitFallbackBuffer : DecoderFallbackBuffer
    {
        internal char cBestFit;
        internal int iCount = -1;
        internal int iSize;
        private InternalDecoderBestFitFallback oFallback;
        private static object s_InternalSyncObject;

        public InternalDecoderBestFitFallbackBuffer(InternalDecoderBestFitFallback fallback)
        {
            this.oFallback = fallback;
            if (this.oFallback.arrayBestFit == null)
            {
                lock (InternalSyncObject)
                {
                    if (this.oFallback.arrayBestFit == null)
                    {
                        this.oFallback.arrayBestFit = fallback.encoding.GetBestFitBytesToUnicodeData();
                    }
                }
            }
        }

        public override bool Fallback(byte[] bytesUnknown, int index)
        {
            this.cBestFit = this.TryBestFit(bytesUnknown);
            if (this.cBestFit == '\0')
            {
                this.cBestFit = this.oFallback.cReplacement;
            }
            this.iCount = this.iSize = 1;
            return true;
        }

        public override char GetNextChar()
        {
            this.iCount--;
            if (this.iCount < 0)
            {
                return '\0';
            }
            if (this.iCount == 0x7fffffff)
            {
                this.iCount = -1;
                return '\0';
            }
            return this.cBestFit;
        }

        [SecurityCritical]
        internal override unsafe int InternalFallback(byte[] bytes, byte* pBytes)
        {
            return 1;
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

        private char TryBestFit(byte[] bytesCheck)
        {
            int num = 0;
            int length = this.oFallback.arrayBestFit.Length;
            if (length != 0)
            {
                char ch;
                if ((bytesCheck.Length == 0) || (bytesCheck.Length > 2))
                {
                    return '\0';
                }
                if (bytesCheck.Length == 1)
                {
                    ch = (char) bytesCheck[0];
                }
                else
                {
                    ch = (char) ((bytesCheck[0] << 8) + bytesCheck[1]);
                }
                if ((ch >= this.oFallback.arrayBestFit[0]) && (ch <= this.oFallback.arrayBestFit[length - 2]))
                {
                    int num3;
                    int num4;
                    while ((num4 = length - num) > 6)
                    {
                        num3 = ((num4 / 2) + num) & 0xfffe;
                        char ch2 = this.oFallback.arrayBestFit[num3];
                        if (ch2 == ch)
                        {
                            return this.oFallback.arrayBestFit[num3 + 1];
                        }
                        if (ch2 < ch)
                        {
                            num = num3;
                        }
                        else
                        {
                            length = num3;
                        }
                    }
                    for (num3 = num; num3 < length; num3 += 2)
                    {
                        if (this.oFallback.arrayBestFit[num3] == ch)
                        {
                            return this.oFallback.arrayBestFit[num3 + 1];
                        }
                    }
                    return '\0';
                }
            }
            return '\0';
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
}

