namespace System
{
    using System.Runtime.CompilerServices;
    using System.Security;

    internal static class ParseNumbers
    {
        internal const int IsTight = 0x1000;
        internal const int NoSpace = 0x2000;
        internal const int PrintAsI1 = 0x40;
        internal const int PrintAsI2 = 0x80;
        internal const int PrintAsI4 = 0x100;
        internal const int TreatAsI1 = 0x400;
        internal const int TreatAsI2 = 0x800;
        internal const int TreatAsUnsigned = 0x200;

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string IntToString(int l, int radix, int width, char paddingChar, int flags);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern string LongToString(long l, int radix, int width, char paddingChar, int flags);
        [SecuritySafeCritical]
        public static unsafe int StringToInt(string s, int radix, int flags)
        {
            return StringToInt(s, radix, flags, (int*) null);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern unsafe int StringToInt(string s, int radix, int flags, int* currPos);
        [SecuritySafeCritical]
        public static unsafe int StringToInt(string s, int radix, int flags, ref int currPos)
        {
            fixed (int* numRef = ((int*) currPos))
            {
                return StringToInt(s, radix, flags, numRef);
            }
        }

        [SecuritySafeCritical]
        public static unsafe long StringToLong(string s, int radix, int flags)
        {
            return StringToLong(s, radix, flags, (int*) null);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        public static extern unsafe long StringToLong(string s, int radix, int flags, int* currPos);
        [SecuritySafeCritical]
        public static unsafe long StringToLong(string s, int radix, int flags, ref int currPos)
        {
            fixed (int* numRef = ((int*) currPos))
            {
                return StringToLong(s, radix, flags, numRef);
            }
        }
    }
}

