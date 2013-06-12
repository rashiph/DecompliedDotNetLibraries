namespace System.Text
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable]
    internal class EncoderNLS : System.Text.Encoder, ISerializable
    {
        internal char charLeftOver;
        [NonSerialized]
        internal int m_charsUsed;
        protected System.Text.Encoding m_encoding;
        [NonSerialized]
        protected bool m_mustFlush;
        [NonSerialized]
        internal bool m_throwOnOverflow;

        internal EncoderNLS()
        {
            this.m_encoding = null;
            this.Reset();
        }

        internal EncoderNLS(System.Text.Encoding encoding)
        {
            this.m_encoding = encoding;
            base.m_fallback = this.m_encoding.EncoderFallback;
            this.Reset();
        }

        internal EncoderNLS(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("NotSupported_TypeCannotDeserialized"), new object[] { base.GetType() }));
        }

        internal void ClearMustFlush()
        {
            this.m_mustFlush = false;
        }

        [SecurityCritical]
        public override unsafe void Convert(char* chars, int charCount, byte* bytes, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
        {
            if ((bytes == null) || (chars == null))
            {
                throw new ArgumentNullException((bytes == null) ? "bytes" : "chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charCount < 0) || (byteCount < 0))
            {
                throw new ArgumentOutOfRangeException((charCount < 0) ? "charCount" : "byteCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = false;
            this.m_charsUsed = 0;
            bytesUsed = this.m_encoding.GetBytes(chars, charCount, bytes, byteCount, this);
            charsUsed = this.m_charsUsed;
            completed = ((charsUsed == charCount) && (!flush || !this.HasState)) && ((base.m_fallbackBuffer == null) || (base.m_fallbackBuffer.Remaining == 0));
        }

        [SecuritySafeCritical]
        public override unsafe void Convert(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, int byteCount, bool flush, out int charsUsed, out int bytesUsed, out bool completed)
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
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* chRef = chars)
            {
                fixed (byte* numRef = bytes)
                {
                    this.Convert(chRef + charIndex, charCount, numRef + byteIndex, byteCount, flush, out charsUsed, out bytesUsed, out completed);
                }
            }
        }

        [SecurityCritical]
        public override unsafe int GetByteCount(char* chars, int count, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.m_mustFlush = flush;
            this.m_throwOnOverflow = true;
            return this.m_encoding.GetByteCount(chars, count, this);
        }

        [SecuritySafeCritical]
        public override unsafe int GetByteCount(char[] chars, int index, int count, bool flush)
        {
            if (chars == null)
            {
                throw new ArgumentNullException("chars", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((index < 0) || (count < 0))
            {
                throw new ArgumentOutOfRangeException((index < 0) ? "index" : "count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((chars.Length - index) < count)
            {
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            int num = -1;
            fixed (char* chRef = chars)
            {
                num = this.GetByteCount(chRef + index, count, flush);
            }
            return num;
        }

        [SecurityCritical]
        public override unsafe int GetBytes(char* chars, int charCount, byte* bytes, int byteCount, bool flush)
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
            return this.m_encoding.GetBytes(chars, charCount, bytes, byteCount, this);
        }

        [SecuritySafeCritical]
        public override unsafe int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
        {
            if ((chars == null) || (bytes == null))
            {
                throw new ArgumentNullException((chars == null) ? "chars" : "bytes", Environment.GetResourceString("ArgumentNull_Array"));
            }
            if ((charIndex < 0) || (charCount < 0))
            {
                throw new ArgumentOutOfRangeException((charIndex < 0) ? "charIndex" : "charCount", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((chars.Length - charIndex) < charCount)
            {
                throw new ArgumentOutOfRangeException("chars", Environment.GetResourceString("ArgumentOutOfRange_IndexCountBuffer"));
            }
            if ((byteIndex < 0) || (byteIndex > bytes.Length))
            {
                throw new ArgumentOutOfRangeException("byteIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            if (chars.Length == 0)
            {
                chars = new char[1];
            }
            int byteCount = bytes.Length - byteIndex;
            if (bytes.Length == 0)
            {
                bytes = new byte[1];
            }
            fixed (char* chRef = chars)
            {
                fixed (byte* numRef = bytes)
                {
                    return this.GetBytes(chRef + charIndex, charCount, numRef + byteIndex, byteCount, flush);
                }
            }
        }

        public override void Reset()
        {
            this.charLeftOver = '\0';
            if (base.m_fallbackBuffer != null)
            {
                base.m_fallbackBuffer.Reset();
            }
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.SerializeEncoder(info);
            info.AddValue("encoding", this.m_encoding);
            info.AddValue("charLeftOver", this.charLeftOver);
            info.SetType(typeof(System.Text.Encoding.DefaultEncoder));
        }

        public System.Text.Encoding Encoding
        {
            get
            {
                return this.m_encoding;
            }
        }

        internal virtual bool HasState
        {
            get
            {
                return (this.charLeftOver != '\0');
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

