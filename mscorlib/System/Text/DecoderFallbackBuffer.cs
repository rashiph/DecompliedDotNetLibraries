namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Security;

    public abstract class DecoderFallbackBuffer
    {
        internal unsafe byte* byteStart;
        internal unsafe char* charEnd;

        protected DecoderFallbackBuffer()
        {
        }

        public abstract bool Fallback(byte[] bytesUnknown, int index);
        public abstract char GetNextChar();
        [SecurityCritical]
        internal virtual unsafe int InternalFallback(byte[] bytes, byte* pBytes)
        {
            char ch;
            if (!this.Fallback(bytes, ((int) ((long) ((pBytes - this.byteStart) / 1))) - bytes.Length))
            {
                return 0;
            }
            int num = 0;
            bool flag = false;
            while ((ch = this.GetNextChar()) != '\0')
            {
                if (char.IsSurrogate(ch))
                {
                    if (char.IsHighSurrogate(ch))
                    {
                        if (flag)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
                        }
                        flag = true;
                    }
                    else
                    {
                        if (!flag)
                        {
                            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
                        }
                        flag = false;
                    }
                }
                num++;
            }
            if (flag)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
            }
            return num;
        }

        [SecurityCritical]
        internal virtual unsafe bool InternalFallback(byte[] bytes, byte* pBytes, ref char* chars)
        {
            if (this.Fallback(bytes, ((int) ((long) ((pBytes - this.byteStart) / 1))) - bytes.Length))
            {
                char ch;
                char* chPtr = chars;
                bool flag = false;
                while ((ch = this.GetNextChar()) != '\0')
                {
                    if (char.IsSurrogate(ch))
                    {
                        if (char.IsHighSurrogate(ch))
                        {
                            if (flag)
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
                            }
                            flag = true;
                        }
                        else
                        {
                            if (!flag)
                            {
                                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
                            }
                            flag = false;
                        }
                    }
                    if (chPtr >= this.charEnd)
                    {
                        return false;
                    }
                    chPtr++;
                    chPtr[0] = ch;
                }
                if (flag)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidCharSequenceNoIndex"));
                }
                chars = chPtr;
            }
            return true;
        }

        [SecurityCritical]
        internal unsafe void InternalInitialize(byte* byteStart, char* charEnd)
        {
            this.byteStart = byteStart;
            this.charEnd = charEnd;
        }

        [SecurityCritical]
        internal unsafe void InternalReset()
        {
            this.byteStart = null;
            this.Reset();
        }

        public abstract bool MovePrevious();
        public virtual void Reset()
        {
            while (this.GetNextChar() != '\0')
            {
            }
        }

        internal void ThrowLastBytesRecursive(byte[] bytesUnknown)
        {
            StringBuilder builder = new StringBuilder(bytesUnknown.Length * 3);
            int index = 0;
            while ((index < bytesUnknown.Length) && (index < 20))
            {
                if (builder.Length > 0)
                {
                    builder.Append(" ");
                }
                builder.Append(string.Format(CultureInfo.InvariantCulture, @"\x{0:X2}", new object[] { bytesUnknown[index] }));
                index++;
            }
            if (index == 20)
            {
                builder.Append(" ...");
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_RecursiveFallbackBytes", new object[] { builder.ToString() }), "bytesUnknown");
        }

        public abstract int Remaining { get; }
    }
}

