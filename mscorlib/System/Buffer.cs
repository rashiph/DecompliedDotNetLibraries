namespace System
{
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public static class Buffer
    {
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern int _ByteLength(Array array);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern byte _GetByte(Array array, int index);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void _SetByte(Array array, int index, byte value);
        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        public static extern void BlockCopy(Array src, int srcOffset, Array dst, int dstOffset, int count);
        [SecuritySafeCritical]
        public static int ByteLength(Array array)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (!IsPrimitiveTypeArray(array))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), "array");
            }
            return _ByteLength(array);
        }

        [SecuritySafeCritical]
        public static byte GetByte(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (!IsPrimitiveTypeArray(array))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), "array");
            }
            if ((index < 0) || (index >= _ByteLength(array)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return _GetByte(array, index);
        }

        [SecurityCritical]
        internal static unsafe int IndexOfByte(byte* src, byte value, int index, int count)
        {
            byte* numPtr = src + index;
            while ((((int) numPtr) & 3) != 0)
            {
                if (count == 0)
                {
                    return -1;
                }
                if (numPtr[0] == value)
                {
                    return (int) ((long) ((numPtr - src) / 1));
                }
                count--;
                numPtr++;
            }
            uint num = (uint) ((value << 8) + value);
            num = (num << 0x10) + num;
            while (count > 3)
            {
                uint num2 = *((uint*) numPtr);
                num2 ^= num;
                uint num3 = 0x7efefeff + num2;
                num2 ^= uint.MaxValue;
                num2 ^= num3;
                if ((num2 & 0x81010100) != 0)
                {
                    int num4 = (int) ((long) ((numPtr - src) / 1));
                    if (numPtr[0] == value)
                    {
                        return num4;
                    }
                    if (numPtr[1] == value)
                    {
                        return (num4 + 1);
                    }
                    if (numPtr[2] == value)
                    {
                        return (num4 + 2);
                    }
                    if (numPtr[3] == value)
                    {
                        return (num4 + 3);
                    }
                }
                count -= 4;
                numPtr += 4;
            }
            while (count > 0)
            {
                if (numPtr[0] == value)
                {
                    return (int) ((long) ((numPtr - src) / 1));
                }
                count--;
                numPtr++;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecuritySafeCritical]
        internal static extern void InternalBlockCopy(Array src, int srcOffsetBytes, Array dst, int dstOffsetBytes, int byteCount);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool IsPrimitiveTypeArray(Array array);
        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void memcpy(byte* src, int srcIndex, byte[] dest, int destIndex, int len)
        {
            if (len != 0)
            {
                fixed (byte* numRef = dest)
                {
                    memcpyimpl(src + srcIndex, numRef + destIndex, len);
                }
            }
        }

        [SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void memcpy(byte[] src, int srcIndex, byte* pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                fixed (byte* numRef = src)
                {
                    memcpyimpl(numRef + srcIndex, pDest + destIndex, len);
                }
            }
        }

        [ForceTokenStabilization, SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        internal static unsafe void memcpy(char* pSrc, int srcIndex, char* pDest, int destIndex, int len)
        {
            if (len != 0)
            {
                memcpyimpl((byte*) (pSrc + srcIndex), (byte*) (pDest + destIndex), len * 2);
            }
        }

        [SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), ForceTokenStabilization]
        internal static unsafe void memcpyimpl(byte* src, byte* dest, int len)
        {
            if (len >= 0x10)
            {
                do
                {
                    *((int*) dest) = *((int*) src);
                    *((int*) (dest + 4)) = *((int*) (src + 4));
                    *((int*) (dest + 8)) = *((int*) (src + 8));
                    *((int*) (dest + 12)) = *((int*) (src + 12));
                    dest += 0x10;
                    src += 0x10;
                }
                while ((len -= 0x10) >= 0x10);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *((int*) dest) = *((int*) src);
                    *((int*) (dest + 4)) = *((int*) (src + 4));
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *((int*) dest) = *((int*) src);
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *((short*) dest) = *((short*) src);
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    dest++;
                    src++;
                    dest[0] = src[0];
                }
            }
        }

        [SecuritySafeCritical]
        public static void SetByte(Array array, int index, byte value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (!IsPrimitiveTypeArray(array))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_MustBePrimArray"), "array");
            }
            if ((index < 0) || (index >= _ByteLength(array)))
            {
                throw new ArgumentOutOfRangeException("index");
            }
            _SetByte(array, index, value);
        }

        [SecurityCritical]
        internal static unsafe void ZeroMemory(byte* src, long len)
        {
            while (true)
            {
                len -= 1L;
                if (len <= 0L)
                {
                    return;
                }
                src[(int) len] = 0;
            }
        }
    }
}

