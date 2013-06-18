namespace System.IdentityModel.Tokens
{
    using System;
    using System.IdentityModel;

    internal static class DEREncoding
    {
        private static byte[] mech = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x12, 1, 2, 2 };
        private static byte[] type;

        static DEREncoding()
        {
            byte[] buffer = new byte[2];
            buffer[0] = 1;
            type = buffer;
        }

        private static bool BufferIsEqual(byte[] arrayOne, int offsetOne, byte[] arrayTwo, int offsetTwo, int length)
        {
            if (length > (arrayOne.Length - offsetOne))
            {
                return false;
            }
            if (length > (arrayTwo.Length - offsetTwo))
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (arrayOne[offsetOne + i] != arrayTwo[offsetTwo + i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int LengthSize(int length)
        {
            if (length < 0x80)
            {
                return 1;
            }
            if (length < 0x100)
            {
                return 2;
            }
            if (length < 0x10000)
            {
                return 3;
            }
            if (length < 0x1000000)
            {
                return 4;
            }
            return 5;
        }

        public static void MakeTokenHeader(int bodySize, byte[] buffer, ref int offset, ref int len)
        {
            buffer[offset++] = 0x60;
            len--;
            WriteLength(buffer, ref offset, ref len, (((1 + LengthSize(mech.Length)) + mech.Length) + type.Length) + bodySize);
            buffer[offset++] = 6;
            len--;
            WriteLength(buffer, ref offset, ref len, mech.Length);
            Buffer.BlockCopy(mech, 0, buffer, offset, mech.Length);
            offset += mech.Length;
            len -= mech.Length;
            Buffer.BlockCopy(type, 0, buffer, offset, type.Length);
            offset += type.Length;
            len -= type.Length;
        }

        public static int ReadLength(byte[] buffer, ref int offset, ref int length)
        {
            int num2 = 0;
            if (length < 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            int num = buffer[offset++];
            length--;
            if ((num & 0x80) == 0)
            {
                return num;
            }
            if ((num &= 0x7f) > (length - 1))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (num > 4)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            while (num != 0)
            {
                num2 = (num2 << 8) + buffer[offset++];
                length--;
                num--;
            }
            return num2;
        }

        public static int TokenSize(int bodySize)
        {
            bodySize += ((2 + mech.Length) + LengthSize(mech.Length)) + 1;
            return ((1 + LengthSize(bodySize)) + bodySize);
        }

        public static void VerifyTokenHeader(byte[] buffer, ref int offset, ref int len)
        {
            if (--len < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (buffer[offset++] != 0x60)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (ReadLength(buffer, ref offset, ref len) != len)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (--len < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (buffer[offset++] != 6)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            int num2 = ReadLength(buffer, ref offset, ref len);
            if ((num2 & 0x7fffffff) != mech.Length)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if ((len -= num2) < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (!BufferIsEqual(mech, 0, buffer, offset, mech.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            offset += num2;
            if ((len -= type.Length) < 0)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            if (!BufferIsEqual(type, 0, buffer, offset, type.Length))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SystemException());
            }
            offset += type.Length;
        }

        public static void WriteLength(byte[] buffer, ref int offset, ref int bufferLength, int length)
        {
            if (length < 0x80)
            {
                buffer[offset++] = (byte) length;
                bufferLength--;
            }
            else
            {
                buffer[offset++] = (byte) (LengthSize(length) + 0x7f);
                if (length >= 0x1000000)
                {
                    buffer[offset++] = (byte) (length >> 0x18);
                    bufferLength--;
                }
                if (length >= 0x10000)
                {
                    buffer[offset++] = (byte) ((length >> 0x10) & 0xff);
                    bufferLength--;
                }
                if (length >= 0x100)
                {
                    buffer[offset++] = (byte) ((length >> 8) & 0xff);
                    bufferLength--;
                }
                buffer[offset++] = (byte) (length & 0xff);
                bufferLength--;
            }
        }
    }
}

