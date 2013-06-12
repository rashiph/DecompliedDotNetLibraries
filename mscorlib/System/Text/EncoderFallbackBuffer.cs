namespace System.Text
{
    using System;
    using System.Security;

    public abstract class EncoderFallbackBuffer
    {
        internal bool bFallingBack;
        internal bool bUsedEncoder;
        internal unsafe char* charEnd;
        internal unsafe char* charStart;
        internal EncoderNLS encoder;
        private const int iMaxRecursion = 250;
        internal int iRecursionCount;
        internal bool setEncoder;

        protected EncoderFallbackBuffer()
        {
        }

        public abstract bool Fallback(char charUnknown, int index);
        public abstract bool Fallback(char charUnknownHigh, char charUnknownLow, int index);
        public abstract char GetNextChar();
        [SecurityCritical]
        internal virtual unsafe bool InternalFallback(char ch, ref char* chars)
        {
            int index = ((int) ((long) ((chars - this.charStart) / 2))) - 1;
            if (char.IsHighSurrogate(ch))
            {
                if (chars >= this.charEnd)
                {
                    if ((this.encoder != null) && !this.encoder.MustFlush)
                    {
                        if (this.setEncoder)
                        {
                            this.bUsedEncoder = true;
                            this.encoder.charLeftOver = ch;
                        }
                        this.bFallingBack = false;
                        return false;
                    }
                }
                else
                {
                    char c = (char) chars;
                    if (char.IsLowSurrogate(c))
                    {
                        if (this.bFallingBack && (this.iRecursionCount++ > 250))
                        {
                            this.ThrowLastCharRecursive(char.ConvertToUtf32(ch, c));
                        }
                        chars += (IntPtr) 2;
                        this.bFallingBack = this.Fallback(ch, c, index);
                        return this.bFallingBack;
                    }
                }
            }
            if (this.bFallingBack && (this.iRecursionCount++ > 250))
            {
                this.ThrowLastCharRecursive(ch);
            }
            this.bFallingBack = this.Fallback(ch, index);
            return this.bFallingBack;
        }

        internal char InternalGetNextChar()
        {
            char nextChar = this.GetNextChar();
            this.bFallingBack = nextChar != '\0';
            if (nextChar == '\0')
            {
                this.iRecursionCount = 0;
            }
            return nextChar;
        }

        [SecurityCritical]
        internal unsafe void InternalInitialize(char* charStart, char* charEnd, EncoderNLS encoder, bool setEncoder)
        {
            this.charStart = charStart;
            this.charEnd = charEnd;
            this.encoder = encoder;
            this.setEncoder = setEncoder;
            this.bUsedEncoder = false;
            this.bFallingBack = false;
            this.iRecursionCount = 0;
        }

        [SecurityCritical]
        internal unsafe void InternalReset()
        {
            this.charStart = null;
            this.bFallingBack = false;
            this.iRecursionCount = 0;
            this.Reset();
        }

        public abstract bool MovePrevious();
        public virtual void Reset()
        {
            while (this.GetNextChar() != '\0')
            {
            }
        }

        internal void ThrowLastCharRecursive(int charRecursive)
        {
            throw new ArgumentException(Environment.GetResourceString("Argument_RecursiveFallback", new object[] { charRecursive }), "chars");
        }

        public abstract int Remaining { get; }
    }
}

