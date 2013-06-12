namespace System.Text
{
    using System;

    [Serializable]
    public sealed class EncoderExceptionFallback : EncoderFallback
    {
        public override EncoderFallbackBuffer CreateFallbackBuffer()
        {
            return new EncoderExceptionFallbackBuffer();
        }

        public override bool Equals(object value)
        {
            return (value is EncoderExceptionFallback);
        }

        public override int GetHashCode()
        {
            return 0x28e;
        }

        public override int MaxCharCount
        {
            get
            {
                return 0;
            }
        }
    }
}

