namespace System.Collections.Generic
{
    using System;
    using System.Security;

    internal class BitHelper
    {
        private const byte IntSize = 0x20;
        private int[] m_array;
        private unsafe int* m_arrayPtr;
        private int m_length;
        private const byte MarkedBitFlag = 1;
        private bool useStackAlloc;

        [SecurityCritical]
        internal unsafe BitHelper(int* bitArrayPtr, int length)
        {
            this.m_arrayPtr = bitArrayPtr;
            this.m_length = length;
            this.useStackAlloc = true;
        }

        internal BitHelper(int[] bitArray, int length)
        {
            this.m_array = bitArray;
            this.m_length = length;
        }

        [SecurityCritical]
        internal unsafe bool IsMarked(int bitPosition)
        {
            if (this.useStackAlloc)
            {
                int num = bitPosition / 0x20;
                return (((num < this.m_length) && (num >= 0)) && ((this.m_arrayPtr[num] & (((int) 1) << (bitPosition % 0x20))) != 0));
            }
            int index = bitPosition / 0x20;
            return (((index < this.m_length) && (index >= 0)) && ((this.m_array[index] & (((int) 1) << (bitPosition % 0x20))) != 0));
        }

        [SecurityCritical]
        internal unsafe void MarkBit(int bitPosition)
        {
            if (this.useStackAlloc)
            {
                int num = bitPosition / 0x20;
                if ((num < this.m_length) && (num >= 0))
                {
                    int* numPtr1 = this.m_arrayPtr + num;
                    numPtr1[0] |= ((int) 1) << (bitPosition % 0x20);
                }
            }
            else
            {
                int index = bitPosition / 0x20;
                if ((index < this.m_length) && (index >= 0))
                {
                    this.m_array[index] |= ((int) 1) << (bitPosition % 0x20);
                }
            }
        }

        internal static int ToIntArrayLength(int n)
        {
            if (n <= 0)
            {
                return 0;
            }
            return (((n - 1) / 0x20) + 1);
        }
    }
}

