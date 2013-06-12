namespace System.Text
{
    using System;
    using System.Security;
    using System.Threading;

    internal sealed class InternalEncoderBestFitFallbackBuffer : EncoderFallbackBuffer
    {
        private char cBestFit;
        private int iCount = -1;
        private int iSize;
        private InternalEncoderBestFitFallback oFallback;
        private static object s_InternalSyncObject;

        public InternalEncoderBestFitFallbackBuffer(InternalEncoderBestFitFallback fallback)
        {
            this.oFallback = fallback;
            if (this.oFallback.arrayBestFit == null)
            {
                lock (InternalSyncObject)
                {
                    if (this.oFallback.arrayBestFit == null)
                    {
                        this.oFallback.arrayBestFit = fallback.encoding.GetBestFitUnicodeToBytesData();
                    }
                }
            }
        }

        public override bool Fallback(char charUnknown, int index)
        {
            this.iCount = this.iSize = 1;
            this.cBestFit = this.TryBestFit(charUnknown);
            if (this.cBestFit == '\0')
            {
                this.cBestFit = '?';
            }
            return true;
        }

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
        {
            if (!char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException("charUnknownHigh", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0xd800, 0xdbff }));
            }
            if (!char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException("CharUnknownLow", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0xdc00, 0xdfff }));
            }
            this.cBestFit = '?';
            this.iCount = this.iSize = 2;
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
            base.charStart = null;
            base.bFallingBack = false;
        }

        private char TryBestFit(char cUnknown)
        {
            int num3;
            int num4;
            int num = 0;
            int length = this.oFallback.arrayBestFit.Length;
            while ((num4 = length - num) > 6)
            {
                num3 = ((num4 / 2) + num) & 0xfffe;
                char ch = this.oFallback.arrayBestFit[num3];
                if (ch == cUnknown)
                {
                    return this.oFallback.arrayBestFit[num3 + 1];
                }
                if (ch < cUnknown)
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
                if (this.oFallback.arrayBestFit[num3] == cUnknown)
                {
                    return this.oFallback.arrayBestFit[num3 + 1];
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

