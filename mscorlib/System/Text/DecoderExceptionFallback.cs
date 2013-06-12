namespace System.Text
{
    using System;

    [Serializable]
    public sealed class DecoderExceptionFallback : DecoderFallback
    {
        public override DecoderFallbackBuffer CreateFallbackBuffer()
        {
            return new DecoderExceptionFallbackBuffer();
        }

        public override bool Equals(object value)
        {
            return (value is DecoderExceptionFallback);
        }

        public override int GetHashCode()
        {
            return 0x36f;
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

