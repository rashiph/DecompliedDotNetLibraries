namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel;

    internal static class IntEncoder
    {
        public const int MaxEncodedSize = 5;

        public static int Encode(int value, byte[] bytes, int offset)
        {
            int num = 1;
            while ((value & 0xffffff80L) != 0L)
            {
                bytes[offset++] = (byte) ((value & 0x7f) | 0x80);
                num++;
                value = value >> 7;
            }
            bytes[offset] = (byte) value;
            return num;
        }

        public static int GetEncodedSize(int value)
        {
            if (value < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("ValueMustBeNonNegative")));
            }
            int num = 1;
            while ((value & 0xffffff80L) != 0L)
            {
                num++;
                value = value >> 7;
            }
            return num;
        }
    }
}

