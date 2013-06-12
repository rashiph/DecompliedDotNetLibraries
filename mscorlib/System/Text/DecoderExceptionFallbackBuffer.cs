namespace System.Text
{
    using System;
    using System.Globalization;

    public sealed class DecoderExceptionFallbackBuffer : DecoderFallbackBuffer
    {
        public override bool Fallback(byte[] bytesUnknown, int index)
        {
            this.Throw(bytesUnknown, index);
            return true;
        }

        public override char GetNextChar()
        {
            return '\0';
        }

        public override bool MovePrevious()
        {
            return false;
        }

        private void Throw(byte[] bytesUnknown, int index)
        {
            StringBuilder builder = new StringBuilder(bytesUnknown.Length * 3);
            int num = 0;
            while ((num < bytesUnknown.Length) && (num < 20))
            {
                builder.Append("[");
                builder.Append(bytesUnknown[num].ToString("X2", CultureInfo.InvariantCulture));
                builder.Append("]");
                num++;
            }
            if (num == 20)
            {
                builder.Append(" ...");
            }
            throw new DecoderFallbackException(Environment.GetResourceString("Argument_InvalidCodePageBytesIndex", new object[] { builder, index }), bytesUnknown, index);
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

