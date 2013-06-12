namespace System.Text
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DecoderFallbackException : ArgumentException
    {
        private byte[] bytesUnknown;
        private int index;

        public DecoderFallbackException() : base(Environment.GetResourceString("Arg_ArgumentException"))
        {
            base.SetErrorCode(-2147024809);
        }

        public DecoderFallbackException(string message) : base(message)
        {
            base.SetErrorCode(-2147024809);
        }

        internal DecoderFallbackException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DecoderFallbackException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147024809);
        }

        public DecoderFallbackException(string message, byte[] bytesUnknown, int index) : base(message)
        {
            this.bytesUnknown = bytesUnknown;
            this.index = index;
        }

        public byte[] BytesUnknown
        {
            get
            {
                return this.bytesUnknown;
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }
    }
}

