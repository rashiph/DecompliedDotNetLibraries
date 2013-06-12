namespace System.Text
{
    using System;
    using System.Security;

    public sealed class EncoderReplacementFallbackBuffer : EncoderFallbackBuffer
    {
        private int fallbackCount = -1;
        private int fallbackIndex = -1;
        private string strDefault;

        public EncoderReplacementFallbackBuffer(EncoderReplacementFallback fallback)
        {
            this.strDefault = fallback.DefaultString + fallback.DefaultString;
        }

        public override bool Fallback(char charUnknown, int index)
        {
            if (this.fallbackCount >= 1)
            {
                if ((char.IsHighSurrogate(charUnknown) && (this.fallbackCount >= 0)) && char.IsLowSurrogate(this.strDefault[this.fallbackIndex + 1]))
                {
                    base.ThrowLastCharRecursive(char.ConvertToUtf32(charUnknown, this.strDefault[this.fallbackIndex + 1]));
                }
                base.ThrowLastCharRecursive(charUnknown);
            }
            this.fallbackCount = this.strDefault.Length / 2;
            this.fallbackIndex = -1;
            return (this.fallbackCount != 0);
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
            if (this.fallbackCount >= 1)
            {
                base.ThrowLastCharRecursive(char.ConvertToUtf32(charUnknownHigh, charUnknownLow));
            }
            this.fallbackCount = this.strDefault.Length;
            this.fallbackIndex = -1;
            return (this.fallbackCount != 0);
        }

        public override char GetNextChar()
        {
            this.fallbackCount--;
            this.fallbackIndex++;
            if (this.fallbackCount < 0)
            {
                return '\0';
            }
            if (this.fallbackCount == 0x7fffffff)
            {
                this.fallbackCount = -1;
                return '\0';
            }
            return this.strDefault[this.fallbackIndex];
        }

        public override bool MovePrevious()
        {
            if ((this.fallbackCount >= -1) && (this.fallbackIndex >= 0))
            {
                this.fallbackIndex--;
                this.fallbackCount++;
                return true;
            }
            return false;
        }

        [SecuritySafeCritical]
        public override unsafe void Reset()
        {
            this.fallbackCount = -1;
            this.fallbackIndex = 0;
            base.charStart = null;
            base.bFallingBack = false;
        }

        public override int Remaining
        {
            get
            {
                if (this.fallbackCount >= 0)
                {
                    return this.fallbackCount;
                }
                return 0;
            }
        }
    }
}

