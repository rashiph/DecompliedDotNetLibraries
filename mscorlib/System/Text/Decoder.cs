namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public abstract class Decoder
    {
        internal DecoderFallback m_fallback;
        [NonSerialized]
        internal DecoderFallbackBuffer m_fallbackBuffer;

        protected Decoder()
        {
        }

        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
        public virtual unsafe void Convert(byte* bytes, int byteCount, char* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteCount < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            bytesUsed = byteCount;
            while (bytesUsed > 0)
            {
                if (this.GetCharCount(bytes, bytesUsed, flush) <= charCount)
                {
                    charsUsed = this.GetChars(bytes, bytesUsed, chars, charCount, flush);
                    completed = (bytesUsed == byteCount) && ((this.m_fallbackBuffer == null) || (this.m_fallbackBuffer.Remaining == 0));
                    return;
                }
                flush = false;
                bytesUsed /= 2;
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_ConversionOverflow"));
        }

        [ComVisible(false)]
        public virtual void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteIndex < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((charIndex < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - byteIndex) < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if ((chars.Length - charIndex) < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            bytesUsed = byteCount;
            while (bytesUsed > 0)
            {
                if (this.GetCharCount(bytes, byteIndex, bytesUsed, flush) <= charCount)
                {
                    charsUsed = this.GetChars(bytes, byteIndex, bytesUsed, chars, charIndex, flush);
                    completed = (bytesUsed == byteCount) && ((this.m_fallbackBuffer == null) || (this.m_fallbackBuffer.Remaining == 0));
                    return;
                }
                flush = false;
                bytesUsed /= 2;
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_ConversionOverflow"));
        }

        [ComVisible(false), CLSCompliant(false), SecurityCritical]
        public virtual unsafe int GetCharCount(byte* bytes, int count, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            byte[] buffer = new byte[count];
            for (int i = 0; i < count; i++)
            {
                buffer[i] = bytes[i];
            }
            return this.GetCharCount(buffer, 0, count);
        }

        public abstract int GetCharCount(byte[] bytes, int index, int count);
        [ComVisible(false)]
        public virtual int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            return this.GetCharCount(bytes, index, count);
        }

        [ComVisible(false), SecurityCritical, CLSCompliant(false)]
        public virtual unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
        {
            int num;
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteCount < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            byte[] buffer = new byte[byteCount];
            for (num = 0; num < byteCount; num++)
            {
                buffer[num] = bytes[num];
            }
            char[] chArray = new char[charCount];
            int num2 = this.GetChars(buffer, 0, byteCount, chArray, 0, flush);
            if (num2 < charCount)
            {
                charCount = num2;
            }
            for (num = 0; num < charCount; num++)
            {
                chars[num] = chArray[num];
            }
            return charCount;
        }

        public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);
        public virtual int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
        {
            return this.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
        }

        [ComVisible(false)]
        public virtual void Reset()
        {
            byte[] bytes = new byte[0];
            char[] chars = new char[this.GetCharCount(bytes, 0, 0, true)];
            this.GetChars(bytes, 0, 0, chars, 0, true);
            if (this.m_fallbackBuffer != null)
            {
                this.m_fallbackBuffer.Reset();
            }
        }

        internal void SerializeDecoder(SerializationInfo info)
        {
            info.AddValue("m_fallback", this.m_fallback);
        }

        [ComVisible(false)]
        public DecoderFallback Fallback
        {
            get
            {
                return this.m_fallback;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if ((this.m_fallbackBuffer != null) && (this.m_fallbackBuffer.Remaining > 0))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_FallbackBufferNotEmpty"), "value");
                }
                this.m_fallback = value;
                this.m_fallbackBuffer = null;
            }
        }

        [ComVisible(false)]
        public DecoderFallbackBuffer FallbackBuffer
        {
            get
            {
                if (this.m_fallbackBuffer == null)
                {
                    if (this.m_fallback != null)
                    {
                        this.m_fallbackBuffer = this.m_fallback.CreateFallbackBuffer();
                    }
                    else
                    {
                        this.m_fallbackBuffer = DecoderFallback.ReplacementFallback.CreateFallbackBuffer();
                    }
                }
                return this.m_fallbackBuffer;
            }
        }

        internal bool InternalHasFallbackBuffer
        {
            get
            {
                return (this.m_fallbackBuffer != null);
            }
        }
    }
}

