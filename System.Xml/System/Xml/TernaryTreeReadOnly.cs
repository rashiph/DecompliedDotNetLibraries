namespace System.Xml
{
    using System;

    internal class TernaryTreeReadOnly
    {
        private byte[] nodeBuffer;

        public TernaryTreeReadOnly(byte[] nodeBuffer)
        {
            this.nodeBuffer = nodeBuffer;
        }

        public byte FindCaseInsensitiveString(string stringToFind)
        {
            int num = 0;
            int num2 = 0;
            byte[] nodeBuffer = this.nodeBuffer;
            int num3 = stringToFind[num];
            if (num3 <= 0x7a)
            {
                if (num3 >= 0x61)
                {
                    num3 -= 0x20;
                }
                while (true)
                {
                    int index = num2 * 4;
                    int num4 = nodeBuffer[index];
                    if (num3 < num4)
                    {
                        if (nodeBuffer[index + 1] == 0)
                        {
                            break;
                        }
                        num2 += nodeBuffer[index + 1];
                    }
                    else if (num3 > num4)
                    {
                        if (nodeBuffer[index + 2] == 0)
                        {
                            break;
                        }
                        num2 += nodeBuffer[index + 2];
                    }
                    else
                    {
                        if (num3 == 0)
                        {
                            return nodeBuffer[index + 3];
                        }
                        num2++;
                        num++;
                        if (num == stringToFind.Length)
                        {
                            num3 = 0;
                        }
                        else
                        {
                            num3 = stringToFind[num];
                            if (num3 > 0x7a)
                            {
                                return 0;
                            }
                            if (num3 >= 0x61)
                            {
                                num3 -= 0x20;
                            }
                        }
                    }
                }
            }
            return 0;
        }
    }
}

