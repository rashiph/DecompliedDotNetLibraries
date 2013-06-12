namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class DecoderNLS : System.Text.Decoder, ISerializable
    {
        [NonSerialized]
        internal int m_bytesUsed;
        protected Encoding m_encoding;
        [NonSerialized]
        protected bool m_mustFlush;
        [NonSerialized]
        internal bool m_throwOnOverflow;

        internal DecoderNLS()
        {
            this.m_encoding = null;
            this.Reset();
        }

        internal DecoderNLS(Encoding encoding)
        {
            this.m_encoding = encoding;
            base.m_fallback = this.m_encoding.DecoderFallback;
            this.Reset();
        }

        internal DecoderNLS(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("NotSupported_TypeCannotDeserialized"), new object[] { base.GetType() }));
        }

        internal void ClearMustFlush()
        {
            this.m_mustFlush = false;
        }

        [SecurityCritical]
        public override unsafe void Convert(byte* bytes, int byteCount, char* chars, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
        {
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteCount < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = false;
            this.m_bytesUsed = 0;
            charsUsed = this.m_encoding.GetChars(bytes, byteCount, chars, charCount, this);
            bytesUsed = this.m_bytesUsed;
            completed = ((bytesUsed == byteCount) && (!flush || !this.HasState)) && ((base.m_fallbackBuffer == null) || (base.m_fallbackBuffer.Remaining == 0));
        }

        [SecuritySafeCritical]
        public override unsafe void Convert(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, int charCount, bool flush, out int bytesUsed, out int charsUsed, out bool completed)
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
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            fixed (byte* numRef = bytes)
            {
                fixed (char* chRef = chars)
                {
                    this.Convert(numRef + byteIndex, byteCount, chRef + charIndex, charCount, flush, out bytesUsed, out charsUsed, out completed);
                }
            }
        }

        [SecurityCritical]
        public override unsafe int GetCharCount(byte* bytes, int count, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;
            return this.m_encoding.GetCharCount(bytes, count, this);
        }

        public override int GetCharCount(byte[] bytes, int index, int count)
        {
            return this.GetCharCount(bytes, index, count, false);
        }

        [SecuritySafeCritical]
        public override unsafe int GetCharCount(byte[] bytes, int index, int count, bool flush)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (byte* numRef = bytes)
            {
                return this.GetCharCount(numRef + index, count, flush);
            }
        }

        [SecurityCritical]
        public override unsafe int GetChars(byte* bytes, int byteCount, char* chars, int charCount, bool flush)
        {
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteCount < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteCount < 0) ? "byteCount" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;
            return this.m_encoding.GetChars(bytes, byteCount, chars, charCount, this);
        }

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
        {
            return this.GetChars(bytes, byteIndex, byteCount, chars, charIndex, false);
        }

        [SecuritySafeCritical]
        public override unsafe int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex, bool flush)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((byteIndex < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((byteIndex < 0) ? "byteIndex" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((bytes.Length - byteIndex) < byteCount)
            {
                throw new ArgumentOutOfRangeException("bytes", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if ((charIndex < 0) || (charIndex > chars.Length))
            {
                throw new ArgumentOutOfRangeException("charIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            int charCount = chars.Length - charIndex;
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            fixed (byte* numRef = bytes)
            {
                fixed (char* chRef = chars)
                {
                    return this.GetChars(numRef + byteIndex, byteCount, chRef + charIndex, charCount, flush);
                }
            }
        }

        public override void Reset()
        {
            if (base.m_fallbackBuffer != null)
            {
                base.m_fallbackBuffer.Reset();
            }
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.SerializeDecoder(info);
            info.AddValue("encoding", this.m_encoding);
            info.SetType(typeof(Encoding.DefaultDecoder));
        }

        internal virtual bool HasState
        {
            get
            {
                return false;
            }
        }

        public bool MustFlush
        {
            get
            {
                return this.m_mustFlush;
            }
        }
    }
}

