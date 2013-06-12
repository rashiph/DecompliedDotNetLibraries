namespace System.Net
{
    using System;
    using System.Runtime.InteropServices;

    internal static class ChunkParse
    {
        internal static int GetChunkSize(IReadChunkBytes Source, out int chunkSize)
        {
            int num2 = 0;
            int nextByte = Source.NextByte;
            int num = 0;
            switch (nextByte)
            {
                case 10:
                case 13:
                    num++;
                    nextByte = Source.NextByte;
                    break;
            }
            while (nextByte != -1)
            {
                if ((nextByte >= 0x30) && (nextByte <= 0x39))
                {
                    nextByte -= 0x30;
                }
                else
                {
                    if ((nextByte >= 0x61) && (nextByte <= 0x66))
                    {
                        nextByte -= 0x61;
                    }
                    else if ((nextByte >= 0x41) && (nextByte <= 70))
                    {
                        nextByte -= 0x41;
                    }
                    else
                    {
                        Source.NextByte = nextByte;
                        chunkSize = num2;
                        return num;
                    }
                    nextByte += 10;
                }
                num2 *= 0x10;
                num2 += nextByte;
                num++;
                nextByte = Source.NextByte;
            }
            chunkSize = num2;
            return -1;
        }

        internal static int SkipPastCRLF(IReadChunkBytes Source)
        {
            int num = 0;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            int nextByte = Source.NextByte;
            num++;
            while (nextByte != -1)
            {
                if (flag3)
                {
                    if (nextByte != 10)
                    {
                        return 0;
                    }
                    if (flag)
                    {
                        return 0;
                    }
                    if (!flag2)
                    {
                        return num;
                    }
                    flag4 = true;
                    flag = true;
                    flag3 = false;
                }
                else if (flag4)
                {
                    if ((nextByte != 0x20) && (nextByte != 9))
                    {
                        return 0;
                    }
                    flag = true;
                    flag4 = false;
                }
                if (!flag)
                {
                    switch (nextByte)
                    {
                        case 0x22:
                            if (flag2)
                            {
                                flag2 = false;
                            }
                            else
                            {
                                flag2 = true;
                            }
                            break;

                        case 0x5c:
                            if (flag2)
                            {
                                flag = true;
                            }
                            break;

                        case 10:
                            return 0;

                        case 13:
                            flag3 = true;
                            break;
                    }
                }
                else
                {
                    flag = false;
                }
                nextByte = Source.NextByte;
                num++;
            }
            return -1;
        }
    }
}

