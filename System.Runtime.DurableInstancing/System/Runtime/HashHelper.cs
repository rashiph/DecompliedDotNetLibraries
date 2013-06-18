namespace System.Runtime
{
    using System;

    internal static class HashHelper
    {
        public static byte[] ComputeHash(byte[] buffer)
        {
            int[] numArray = new int[] { 7, 12, 0x11, 0x16, 5, 9, 14, 20, 4, 11, 0x10, 0x17, 6, 10, 15, 0x15 };
            uint[] numArray2 = new uint[] { 
                0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501, 0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821, 
                0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x2441453, 0xd8a1e681, 0xe7d3fbc8, 0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a, 
                0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70, 0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x4881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665, 
                0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1, 0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391
             };
            int num = ((buffer.Length + 8) / 0x40) + 1;
            uint num2 = 0x67452301;
            uint num3 = 0xefcdab89;
            uint num4 = 0x98badcfe;
            uint num5 = 0x10325476;
            for (int i = 0; i < num; i++)
            {
                byte[] buffer2 = buffer;
                int num7 = i * 0x40;
                if ((num7 + 0x40) > buffer.Length)
                {
                    buffer2 = new byte[0x40];
                    for (int k = num7; k < buffer.Length; k++)
                    {
                        buffer2[k - num7] = buffer[k];
                    }
                    if (num7 <= buffer.Length)
                    {
                        buffer2[buffer.Length - num7] = 0x80;
                    }
                    if (i == (num - 1))
                    {
                        buffer2[0x38] = (byte) (buffer.Length << 3);
                        buffer2[0x39] = (byte) (buffer.Length >> 5);
                        buffer2[0x3a] = (byte) (buffer.Length >> 13);
                        buffer2[0x3b] = (byte) (buffer.Length >> 0x15);
                    }
                    num7 = 0;
                }
                uint num9 = num2;
                uint num10 = num3;
                uint num11 = num4;
                uint num12 = num5;
                for (int j = 0; j < 0x40; j++)
                {
                    uint num13;
                    int num14;
                    if (j < 0x10)
                    {
                        num13 = (num10 & num11) | (~num10 & num12);
                        num14 = j;
                    }
                    else if (j < 0x20)
                    {
                        num13 = (num10 & num12) | (num11 & ~num12);
                        num14 = (5 * j) + 1;
                    }
                    else if (j < 0x30)
                    {
                        num13 = (num10 ^ num11) ^ num12;
                        num14 = (3 * j) + 5;
                    }
                    else
                    {
                        num13 = num11 ^ (num10 | ~num12);
                        num14 = 7 * j;
                    }
                    num14 = ((num14 & 15) * 4) + num7;
                    uint num16 = num12;
                    num12 = num11;
                    num11 = num10;
                    num10 = ((num9 + num13) + numArray2[j]) + ((uint) (((buffer2[num14] + (buffer2[num14 + 1] << 8)) + (buffer2[num14 + 2] << 0x10)) + (buffer2[num14 + 3] << 0x18)));
                    num10 = (num10 << numArray[(j & 3) | ((j >> 2) & -4)]) | (num10 >> (0x20 - numArray[(j & 3) | ((j >> 2) & -4)]));
                    num10 += num11;
                    num9 = num16;
                }
                num2 += num9;
                num3 += num10;
                num4 += num11;
                num5 += num12;
            }
            return new byte[] { ((byte) num2), ((byte) (num2 >> 8)), ((byte) (num2 >> 0x10)), ((byte) (num2 >> 0x18)), ((byte) num3), ((byte) (num3 >> 8)), ((byte) (num3 >> 0x10)), ((byte) (num3 >> 0x18)), ((byte) num4), ((byte) (num4 >> 8)), ((byte) (num4 >> 0x10)), ((byte) (num4 >> 0x18)), ((byte) num5), ((byte) (num5 >> 8)), ((byte) (num5 >> 0x10)), ((byte) (num5 >> 0x18)) };
        }
    }
}

