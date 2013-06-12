namespace System.Text
{
    using System;

    public sealed class EncoderExceptionFallbackBuffer : EncoderFallbackBuffer
    {
        public override bool Fallback(char charUnknown, int index)
        {
            throw new EncoderFallbackException(Environment.GetResourceString("Argument_InvalidCodePageConversionIndex", new object[] { (int) charUnknown, index }), charUnknown, index);
        }

        public override bool Fallback(char charUnknownHigh, char charUnknownLow, int index)
        {
            if (!char.IsHighSurrogate(charUnknownHigh))
            {
                throw new ArgumentOutOfRangeException("charUnknownHigh", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0xd800, 0xdbff }));
            }
            if (!char.IsLowSurrogate(charUnknownLow))
            {
                throw new ArgumentOutOfRangeException("CharUnknownLow", Environment.GetResourceString("ArgumentOutOfRange_Range", new object[] { 0xdc00, 0xdfff }));
            }
            int num = char.ConvertToUtf32(charUnknownHigh, charUnknownLow);
            throw new EncoderFallbackException(Environment.GetResourceString("Argument_InvalidCodePageConversionIndex", new object[] { num, index }), charUnknownHigh, charUnknownLow, index);
        }

        public override char GetNextChar()
        {
            return '\0';
        }

        public override bool MovePrevious()
        {
            return false;
        }

        public override int Remaining
        {
            get
            {
                return 0;
            }
        }
    }
}

