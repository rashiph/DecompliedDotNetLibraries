namespace System
{
    internal class IPv4AddressHelper
    {
        private const int NumberOfLabels = 4;

        private IPv4AddressHelper()
        {
        }

        internal static unsafe bool IsValid(char* name, int start, ref int end, bool allowIPv6, bool notImplicitFile)
        {
            int num = 0;
            int num2 = 0;
            bool flag = false;
            while (start < end)
            {
                char ch = name[start];
                if (allowIPv6)
                {
                    if (((ch == ']') || (ch == '/')) || (ch == '%'))
                    {
                        break;
                    }
                }
                else if (((ch == '/') || (ch == '\\')) || (notImplicitFile && (((ch == ':') || (ch == '?')) || (ch == '#'))))
                {
                    break;
                }
                if ((ch <= '9') && (ch >= '0'))
                {
                    flag = true;
                    num2 = (num2 * 10) + (name[start] - '0');
                    if (num2 > 0xff)
                    {
                        return false;
                    }
                }
                else
                {
                    if (ch != '.')
                    {
                        return false;
                    }
                    if (!flag)
                    {
                        return false;
                    }
                    num++;
                    flag = false;
                    num2 = 0;
                }
                start++;
            }
            bool flag2 = (num == 3) && flag;
            if (flag2)
            {
                end = start;
            }
            return flag2;
        }

        private static unsafe bool Parse(string name, byte* numbers, int start, int end)
        {
            for (int i = 0; i < 4; i++)
            {
                char ch;
                byte num2 = 0;
                while (((start < end) && ((ch = name[start]) != '.')) && (ch != ':'))
                {
                    num2 = (byte) ((num2 * 10) + ((byte) (ch - '0')));
                    start++;
                }
                numbers[i] = num2;
                start++;
            }
            return (numbers[0] == 0x7f);
        }

        internal static unsafe string ParseCanonicalName(string str, int start, int end, ref bool isLoopback)
        {
            byte* numbers = stackalloc byte[4];
            isLoopback = Parse(str, numbers, start, end);
            return string.Concat(new object[] { numbers[0], ".", numbers[1], ".", numbers[2], ".", numbers[3] });
        }

        internal static unsafe int ParseHostNumber(string str, int start, int end)
        {
            byte* numbers = stackalloc byte[4];
            Parse(str, numbers, start, end);
            return ((((numbers[0] << 0x18) + (numbers[1] << 0x10)) + (numbers[2] << 8)) + numbers[3]);
        }
    }
}

