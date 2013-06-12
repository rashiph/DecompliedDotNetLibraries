namespace System.Text
{
    using System;
    using System.Security;

    public sealed class DecoderReplacementFallbackBuffer : DecoderFallbackBuffer
    {
        private int fallbackCount = -1;
        private int fallbackIndex = -1;
        private string strDefault;

        public DecoderReplacementFallbackBuffer(DecoderReplacementFallback fallback)
        {
            this.strDefault = fallback.DefaultString;
        }

        public override bool Fallback(byte[] bytesUnknown, int index)
        {
            if (this.fallbackCount >= 1)
            {
                base.ThrowLastBytesRecursive(bytesUnknown);
            }
            if (this.strDefault.Length == 0)
            {
                return false;
            }
            this.fallbackCount = this.strDefault.Length;
            this.fallbackIndex = -1;
            return true;
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

        [SecurityCritical]
        internal override unsafe int InternalFallback(byte[] bytes, byte* pBytes)
        {
            return this.strDefault.Length;
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
            this.fallbackIndex = -1;
            base.byteStart = null;
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

