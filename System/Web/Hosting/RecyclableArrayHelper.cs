namespace System.Web.Hosting
{
    using System;
    using System.Web;

    internal class RecyclableArrayHelper
    {
        private const int ARRAY_SIZE = 0x80;
        private const int MAX_FREE_ARRAYS = 0x40;
        private static IntegerArrayAllocator s_IntegerArrayAllocator = new IntegerArrayAllocator(0x80, 0x40);
        private static IntPtrArrayAllocator s_IntPtrArrayAllocator = new IntPtrArrayAllocator(0x80, 0x40);

        internal static int[] GetIntegerArray(int minimumLength)
        {
            if (minimumLength <= 0x80)
            {
                return (int[]) s_IntegerArrayAllocator.GetBuffer();
            }
            return new int[minimumLength];
        }

        internal static IntPtr[] GetIntPtrArray(int minimumLength)
        {
            if (minimumLength <= 0x80)
            {
                return (IntPtr[]) s_IntPtrArrayAllocator.GetBuffer();
            }
            return new IntPtr[minimumLength];
        }

        internal static void ReuseIntegerArray(int[] array)
        {
            if ((array != null) && (array.Length == 0x80))
            {
                s_IntegerArrayAllocator.ReuseBuffer(array);
            }
        }

        internal static void ReuseIntPtrArray(IntPtr[] array)
        {
            if ((array != null) && (array.Length == 0x80))
            {
                s_IntPtrArrayAllocator.ReuseBuffer(array);
            }
        }
    }
}

