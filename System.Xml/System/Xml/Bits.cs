namespace System.Xml
{
    using System;

    internal static class Bits
    {
        private static readonly uint MASK_0000000011111111 = 0xff00ff;
        private static readonly uint MASK_0000111100001111 = 0xf0f0f0f;
        private static readonly uint MASK_0011001100110011 = 0x33333333;
        private static readonly uint MASK_0101010101010101 = 0x55555555;
        private static readonly uint MASK_1111111111111111 = 0xffff;

        public static uint ClearLeast(uint num)
        {
            return (num & (num - 1));
        }

        public static int Count(uint num)
        {
            num = (num & MASK_0101010101010101) + ((num >> 1) & MASK_0101010101010101);
            num = (num & MASK_0011001100110011) + ((num >> 2) & MASK_0011001100110011);
            num = (num & MASK_0000111100001111) + ((num >> 4) & MASK_0000111100001111);
            num = (num & MASK_0000000011111111) + ((num >> 8) & MASK_0000000011111111);
            num = (num & MASK_1111111111111111) + (num >> 0x10);
            return (int) num;
        }

        public static bool ExactlyOne(uint num)
        {
            return ((num != 0) && ((num & (num - 1)) == 0));
        }

        public static int LeastPosition(uint num)
        {
            if (num == 0)
            {
                return 0;
            }
            return Count(num ^ (num - 1));
        }

        public static bool MoreThanOne(uint num)
        {
            return ((num & (num - 1)) != 0);
        }
    }
}

