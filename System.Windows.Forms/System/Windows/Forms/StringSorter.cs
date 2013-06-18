namespace System.Windows.Forms
{
    using System;
    using System.Globalization;

    internal sealed class StringSorter
    {
        private const int CompareOptions = 0x31007;
        private bool descending;
        public const int Descending = -2147483648;
        public const int IgnoreCase = 1;
        public const int IgnoreKanaType = 0x10000;
        public const int IgnoreNonSpace = 2;
        public const int IgnoreSymbols = 4;
        public const int IgnoreWidth = 0x20000;
        private object[] items;
        private string[] keys;
        private int lcid;
        private int options;
        public const int StringSort = 0x1000;

        private StringSorter(CultureInfo culture, string[] keys, object[] items, int options)
        {
            if (keys == null)
            {
                if (items is string[])
                {
                    keys = (string[]) items;
                    items = null;
                }
                else
                {
                    keys = new string[items.Length];
                    for (int i = 0; i < items.Length; i++)
                    {
                        object obj2 = items[i];
                        if (obj2 != null)
                        {
                            keys[i] = obj2.ToString();
                        }
                    }
                }
            }
            this.keys = keys;
            this.items = items;
            this.lcid = (culture == null) ? SafeNativeMethods.GetThreadLocale() : culture.LCID;
            this.options = options & 0x31007;
            this.descending = (options & -2147483648) != 0;
        }

        internal static int ArrayLength(object[] array)
        {
            if (array == null)
            {
                return 0;
            }
            return array.Length;
        }

        public static int Compare(string s1, string s2)
        {
            return Compare(SafeNativeMethods.GetThreadLocale(), s1, s2, 0);
        }

        public static int Compare(string s1, string s2, int options)
        {
            return Compare(SafeNativeMethods.GetThreadLocale(), s1, s2, options);
        }

        public static int Compare(CultureInfo culture, string s1, string s2, int options)
        {
            return Compare(culture.LCID, s1, s2, options);
        }

        private static int Compare(int lcid, string s1, string s2, int options)
        {
            if (s1 == null)
            {
                if (s2 != null)
                {
                    return -1;
                }
                return 0;
            }
            if (s2 == null)
            {
                return 1;
            }
            return string.Compare(s1, s2, false, CultureInfo.CurrentCulture);
        }

        private int CompareKeys(string s1, string s2)
        {
            int num = Compare(this.lcid, s1, s2, this.options);
            if (!this.descending)
            {
                return num;
            }
            return -num;
        }

        private void QuickSort(int left, int right)
        {
            do
            {
                int index = left;
                int num2 = right;
                string str = this.keys[(index + num2) >> 1];
                do
                {
                    while (this.CompareKeys(this.keys[index], str) < 0)
                    {
                        index++;
                    }
                    while (this.CompareKeys(str, this.keys[num2]) < 0)
                    {
                        num2--;
                    }
                    if (index > num2)
                    {
                        break;
                    }
                    if (index < num2)
                    {
                        string str2 = this.keys[index];
                        this.keys[index] = this.keys[num2];
                        this.keys[num2] = str2;
                        if (this.items != null)
                        {
                            object obj2 = this.items[index];
                            this.items[index] = this.items[num2];
                            this.items[num2] = obj2;
                        }
                    }
                    index++;
                    num2--;
                }
                while (index <= num2);
                if ((num2 - left) <= (right - index))
                {
                    if (left < num2)
                    {
                        this.QuickSort(left, num2);
                    }
                    left = index;
                }
                else
                {
                    if (index < right)
                    {
                        this.QuickSort(index, right);
                    }
                    right = num2;
                }
            }
            while (left < right);
        }

        public static void Sort(object[] items)
        {
            Sort(null, null, items, 0, ArrayLength(items), 0);
        }

        public static void Sort(object[] items, int options)
        {
            Sort(null, null, items, 0, ArrayLength(items), options);
        }

        public static void Sort(string[] keys, object[] items)
        {
            Sort(null, keys, items, 0, ArrayLength(items), 0);
        }

        public static void Sort(object[] items, int index, int count)
        {
            Sort(null, null, items, index, count, 0);
        }

        public static void Sort(string[] keys, object[] items, int options)
        {
            Sort(null, keys, items, 0, ArrayLength(items), options);
        }

        public static void Sort(CultureInfo culture, object[] items, int options)
        {
            Sort(culture, null, items, 0, ArrayLength(items), options);
        }

        public static void Sort(object[] items, int index, int count, int options)
        {
            Sort(null, null, items, index, count, options);
        }

        public static void Sort(string[] keys, object[] items, int index, int count)
        {
            Sort(null, keys, items, index, count, 0);
        }

        public static void Sort(CultureInfo culture, string[] keys, object[] items, int options)
        {
            Sort(culture, keys, items, 0, ArrayLength(items), options);
        }

        public static void Sort(string[] keys, object[] items, int index, int count, int options)
        {
            Sort(null, keys, items, index, count, options);
        }

        public static void Sort(CultureInfo culture, object[] items, int index, int count, int options)
        {
            Sort(culture, null, items, index, count, options);
        }

        public static void Sort(CultureInfo culture, string[] keys, object[] items, int index, int count, int options)
        {
            if ((items == null) || ((keys != null) && (keys.Length != items.Length)))
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("ArraysNotSameSize", new object[] { "keys", "items" }));
            }
            if (count > 1)
            {
                new StringSorter(culture, keys, items, options).QuickSort(index, (index + count) - 1);
            }
        }
    }
}

