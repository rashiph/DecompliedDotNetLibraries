namespace System
{
    using System.Security;

    public static class BitConverter
    {
        public static readonly bool IsLittleEndian = true;

        [SecuritySafeCritical]
        public static unsafe long DoubleToInt64Bits(double value)
        {
            return *(((long*) &value));
        }

        public static byte[] GetBytes(bool value)
        {
            return new byte[] { (value ? ((byte) 1) : ((byte) 0)) };
        }

        public static byte[] GetBytes(char value)
        {
            return GetBytes((short) value);
        }

        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(double value)
        {
            return GetBytes(*((long*) &value));
        }

        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(short value)
        {
            byte[] buffer = new byte[2];
            fixed (byte* numRef = buffer)
            {
                *((short*) numRef) = value;
            }
            return buffer;
        }

        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(int value)
        {
            byte[] buffer = new byte[4];
            fixed (byte* numRef = buffer)
            {
                *((int*) numRef) = value;
            }
            return buffer;
        }

        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(long value)
        {
            byte[] buffer = new byte[8];
            fixed (byte* numRef = buffer)
            {
                *((long*) numRef) = value;
            }
            return buffer;
        }

        [SecuritySafeCritical]
        public static unsafe byte[] GetBytes(float value)
        {
            return GetBytes(*((int*) &value));
        }

        [CLSCompliant(false)]
        public static byte[] GetBytes(ushort value)
        {
            return GetBytes((short) value);
        }

        [CLSCompliant(false)]
        public static byte[] GetBytes(uint value)
        {
            return GetBytes((int) value);
        }

        [CLSCompliant(false)]
        public static byte[] GetBytes(ulong value)
        {
            return GetBytes((long) value);
        }

        private static char GetHexValue(int i)
        {
            if (i < 10)
            {
                return (char) (i + 0x30);
            }
            return (char) ((i - 10) + 0x41);
        }

        [SecuritySafeCritical]
        public static unsafe double Int64BitsToDouble(long value)
        {
            return *(((double*) &value));
        }

        public static bool ToBoolean(byte[] value, int startIndex)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (startIndex > (value.Length - 1))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_Index"));
            }
            return (value[startIndex] != 0);
        }

        public static char ToChar(byte[] value, int startIndex)
        {
            return (char) ((ushort) ToInt16(value, startIndex));
        }

        [SecuritySafeCritical]
        public static unsafe double ToDouble(byte[] value, int startIndex)
        {
            return *(((double*) &ToInt64(value, startIndex)));
        }

        [SecuritySafeCritical]
        public static unsafe short ToInt16(byte[] value, int startIndex)
        {
            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }
            if (((ulong) startIndex) >= value.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            }
            if (startIndex > (value.Length - 2))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            fixed (byte* numRef = &(value[startIndex]))
            {
                if ((startIndex % 2) == 0)
                {
                    return *(((short*) numRef));
                }
                if (IsLittleEndian)
                {
                    return (short) (numRef[0] | (numRef[1] << 8));
                }
                return (short) ((numRef[0] << 8) | numRef[1]);
            }
        }

        [SecuritySafeCritical]
        public static unsafe int ToInt32(byte[] value, int startIndex)
        {
            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }
            if (((ulong) startIndex) >= value.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            }
            if (startIndex > (value.Length - 4))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            fixed (byte* numRef = &(value[startIndex]))
            {
                if ((startIndex % 4) == 0)
                {
                    return *(((int*) numRef));
                }
                if (IsLittleEndian)
                {
                    return (((numRef[0] | (numRef[1] << 8)) | (numRef[2] << 0x10)) | (numRef[3] << 0x18));
                }
                return ((((numRef[0] << 0x18) | (numRef[1] << 0x10)) | (numRef[2] << 8)) | numRef[3]);
            }
        }

        [SecuritySafeCritical]
        public static unsafe long ToInt64(byte[] value, int startIndex)
        {
            if (value == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.value);
            }
            if (((ulong) startIndex) >= value.Length)
            {
                ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.startIndex, ExceptionResource.ArgumentOutOfRange_Index);
            }
            if (startIndex > (value.Length - 8))
            {
                ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
            }
            fixed (byte* numRef = &(value[startIndex]))
            {
                if ((startIndex % 8) == 0)
                {
                    return *(((long*) numRef));
                }
                if (IsLittleEndian)
                {
                    int num = ((numRef[0] | (numRef[1] << 8)) | (numRef[2] << 0x10)) | (numRef[3] << 0x18);
                    int num2 = ((numRef[4] | (numRef[5] << 8)) | (numRef[6] << 0x10)) | (numRef[7] << 0x18);
                    return (((long) ((ulong) num)) | (num2 << 0x20));
                }
                int num3 = (((numRef[0] << 0x18) | (numRef[1] << 0x10)) | (numRef[2] << 8)) | numRef[3];
                int num4 = (((numRef[4] << 0x18) | (numRef[5] << 0x10)) | (numRef[6] << 8)) | numRef[7];
                return (((long) ((ulong) num4)) | (num3 << 0x20));
            }
        }

        [SecuritySafeCritical]
        public static unsafe float ToSingle(byte[] value, int startIndex)
        {
            return *(((float*) &ToInt32(value, startIndex)));
        }

        public static string ToString(byte[] value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ToString(value, 0, value.Length);
        }

        public static string ToString(byte[] value, int startIndex)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return ToString(value, startIndex, value.Length - startIndex);
        }

        public static string ToString(byte[] value, int startIndex, int length)
        {
            if (value == null)
            {
                throw new ArgumentNullException("byteArray");
            }
            int num = value.Length;
            if ((startIndex < 0) || ((startIndex >= num) && (startIndex > 0)))
            {
                throw new ArgumentOutOfRangeException("startIndex", Environment.GetResourceString("ArgumentOutOfRange_StartIndex"));
            }
            int num2 = length;
            if (num2 < 0)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if (startIndex > (num - num2))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_ArrayPlusOffTooSmall"));
            }
            if (num2 == 0)
            {
                return string.Empty;
            }
            if (num2 > 0x2aaaaaaa)
            {
                throw new ArgumentOutOfRangeException("length", Environment.GetResourceString("ArgumentOutOfRange_LengthTooLarge", new object[] { 0x2aaaaaaa }));
            }
            int num3 = num2 * 3;
            char[] chArray = new char[num3];
            int index = 0;
            int num5 = startIndex;
            for (index = 0; index < num3; index += 3)
            {
                byte num6 = value[num5++];
                chArray[index] = GetHexValue(num6 / 0x10);
                chArray[index + 1] = GetHexValue(num6 % 0x10);
                chArray[index + 2] = '-';
            }
            return new string(chArray, 0, chArray.Length - 1);
        }

        [CLSCompliant(false)]
        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return (ushort) ToInt16(value, startIndex);
        }

        [CLSCompliant(false)]
        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return (uint) ToInt32(value, startIndex);
        }

        [CLSCompliant(false)]
        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return (ulong) ToInt64(value, startIndex);
        }
    }
}

