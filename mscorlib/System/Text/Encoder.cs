namespace System.Text
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public abstract class Encoder
    {
        internal EncoderFallback m_fallback;
        [NonSerialized]
        internal EncoderFallbackBuffer m_fallbackBuffer;

        protected Encoder()
        {
        }

        [CLSCompliant(false), SecurityCritical, ComVisible(false)]
        public virtual unsafe void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charCount < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            charsUsed = charCount;
            while (charsUsed > 0)
            {
                if (this.GetByteCount(chars, charsUsed, flush) <= byteCount)
                {
                    bytesUsed = this.GetBytes(chars, charsUsed, bytes, byteCount, flush);
                    completed = (charsUsed == charCount) && ((this.m_fallbackBuffer == null) || (this.m_fallbackBuffer.Remaining == 0));
                    return;
                }
                flush = false;
                charsUsed /= 2;
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_ConversionOverflow"));
        }

        [ComVisible(false)]
        public virtual void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charIndex < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((byteIndex < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((chars.Length - charIndex) < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if ((bytes.Length - byteIndex) < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            charsUsed = charCount;
            while (charsUsed > 0)
            {
                if (this.GetByteCount(chars, charIndex, charsUsed, flush) <= byteCount)
                {
                    bytesUsed = this.GetBytes(chars, charIndex, charsUsed, bytes, byteIndex, flush);
                    completed = (charsUsed == charCount) && ((this.m_fallbackBuffer == null) || (this.m_fallbackBuffer.Remaining == 0));
                    return;
                }
                flush = false;
                charsUsed /= 2;
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_ConversionOverflow"));
        }

        [SecurityCritical, ComVisible(false), CLSCompliant(false)]
        public virtual unsafe int GetByteCount(char* chars, int count, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            char[] chArray = new char[count];
            for (int i = 0; i < count; i++)
            {
                chArray[i] = chars[i];
            }
            return this.GetByteCount(chArray, 0, count, flush);
        }

        public abstract int GetByteCount(char[] chars, int index, int count, bool flush);
        [SecurityCritical, CLSCompliant(false), ComVisible(false)]
        public virtual unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
        {
            int num;
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charCount < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            char[] chArray = new char[charCount];
            for (num = 0; num < charCount; num++)
            {
                chArray[num] = chars[num];
            }
            byte[] buffer = new byte[byteCount];
            int num2 = this.GetBytes(chArray, 0, charCount, buffer, 0, flush);
            if (num2 < byteCount)
            {
                byteCount = num2;
            }
            for (num = 0; num < byteCount; num++)
            {
                bytes[num] = buffer[num];
            }
            return byteCount;
        }

        public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush);
        [ComVisible(false)]
        public virtual void Reset()
        {
            char[] chars = new char[0];
            byte[] bytes = new byte[this.GetByteCount(chars, 0, 0, true)];
            this.GetBytes(chars, 0, 0, bytes, 0, true);
            if (this.m_fallbackBuffer != null)
            {
                this.m_fallbackBuffer.Reset();
            }
        }

        internal void SerializeEncoder(SerializationInfo info)
        {
            info.AddValue("m_fallback", this.m_fallback);
        }

        [ComVisible(false)]
        public EncoderFallback Fallback
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
        public EncoderFallbackBuffer FallbackBuffer
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
                        this.m_fallbackBuffer = EncoderFallback.ReplacementFallback.CreateFallbackBuffer();
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

