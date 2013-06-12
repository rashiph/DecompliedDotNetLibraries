namespace System.Collections.Specialized
{
    using System;
    using System.Globalization;

    internal static class FixedStringLookup
    {
        internal static bool Contains(string[][] lookupTable, string value, bool ignoreCase)
        {
            int length = value.Length;
            if ((length <= 0) || ((length - 1) >= lookupTable.Length))
            {
                return false;
            }
            string[] array = lookupTable[length - 1];
            if (array == null)
            {
                return false;
            }
            return Contains(array, value, ignoreCase);
        }

        private static bool Contains(string[] array, string value, bool ignoreCase)
        {
            int index = 0;
            int length = array.Length;
            int pos = 0;
            while (pos < value.Length)
            {
                char ch;
                if (ignoreCase)
                {
                    ch = char.ToLower(value[pos], CultureInfo.InvariantCulture);
                }
                else
                {
                    ch = value[pos];
                }
                if ((length - index) <= 1)
                {
                    if (ch != array[index][pos])
                    {
                        return false;
                    }
                    pos++;
                }
                else
                {
                    if (!FindCharacter(array, ch, pos, ref index, ref length))
                    {
                        return false;
                    }
                    pos++;
                }
            }
            return true;
        }

        private static bool FindCharacter(string[] array, char value, int pos, ref int min, ref int max)
        {
            int index = min;
            while (min < max)
            {
                index = (min + max) / 2;
                char ch = array[index][pos];
                if (value == ch)
                {
                    int num2 = index;
                    while ((num2 > min) && (array[num2 - 1][pos] == value))
                    {
                        num2--;
                    }
                    min = num2;
                    int num3 = index + 1;
                    while ((num3 < max) && (array[num3][pos] == value))
                    {
                        num3++;
                    }
                    max = num3;
                    return true;
                }
                if (value < ch)
                {
                    max = index;
                }
                else
                {
                    min = index + 1;
                }
            }
            return false;
        }
    }
}

