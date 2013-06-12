namespace System
{
    internal class UncNameHelper
    {
        internal const int MaximumInternetNameLength = 0x100;

        private UncNameHelper()
        {
        }

        internal static unsafe bool IsValid(char* name, ushort start, ref int returnedEnd, bool notImplicitFile)
        {
            ushort num = (ushort) returnedEnd;
            if (start == num)
            {
                return false;
            }
            bool flag = false;
            ushort index = start;
            while (index < num)
            {
                if (((name[index] == '/') || (name[index] == '\\')) || (notImplicitFile && (((name[index] == ':') || (name[index] == '?')) || (name[index] == '#'))))
                {
                    num = index;
                    break;
                }
                if (name[index] == '.')
                {
                    index = (ushort) (index + 1);
                    break;
                }
                if ((char.IsLetter(name[index]) || (name[index] == '-')) || (name[index] == '_'))
                {
                    flag = true;
                }
                else if ((name[index] < '0') || (name[index] > '9'))
                {
                    return false;
                }
                index = (ushort) (index + 1);
            }
            if (flag)
            {
                while (index < num)
                {
                    if (((name[index] == '/') || (name[index] == '\\')) || (notImplicitFile && (((name[index] == ':') || (name[index] == '?')) || (name[index] == '#'))))
                    {
                        num = index;
                        break;
                    }
                    if (name[index] == '.')
                    {
                        if (!flag || (((index - 1) >= start) && (name[index - 1] == '.')))
                        {
                            return false;
                        }
                        flag = false;
                    }
                    else if ((name[index] == '-') || (name[index] == '_'))
                    {
                        if (!flag)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!char.IsLetter(name[index]) && ((name[index] < '0') || (name[index] > '9')))
                        {
                            return false;
                        }
                        if (!flag)
                        {
                            flag = true;
                        }
                    }
                    index = (ushort) (index + 1);
                }
            }
            else
            {
                return false;
            }
            if (((index - 1) >= start) && (name[index - 1] == '.'))
            {
                flag = true;
            }
            if (!flag)
            {
                return false;
            }
            returnedEnd = num;
            return true;
        }

        internal static string ParseCanonicalName(string str, int start, int end, ref bool loopback)
        {
            return DomainNameHelper.ParseCanonicalName(str, start, end, ref loopback);
        }
    }
}

