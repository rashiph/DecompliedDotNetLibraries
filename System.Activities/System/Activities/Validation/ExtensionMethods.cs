namespace System.Activities.Validation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class ExtensionMethods
    {
        public static string AsCommaSeparatedValues(this IEnumerable<string> c)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in c)
            {
                if (!string.IsNullOrEmpty(str))
                {
                    if (builder.Length == 0)
                    {
                        builder.Append(str);
                    }
                    else
                    {
                        builder.Append(", ");
                        builder.Append(str);
                    }
                }
            }
            return builder.ToString();
        }

        public static int BinarySearch<T>(this IList<T> items, T value, IComparer<T> comparer)
        {
            return BinarySearch<T>(items, 0, items.Count, value, comparer);
        }

        private static int BinarySearch<T>(IList<T> items, int startIndex, int length, T value, IComparer<T> comparer)
        {
            int num = startIndex;
            int num2 = (startIndex + length) - 1;
            while (num <= num2)
            {
                int num3 = num + ((num2 - num) >> 1);
                int num4 = comparer.Compare(items[num3], value);
                if (num4 == 0)
                {
                    return num3;
                }
                if (num4 < 0)
                {
                    num = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return ~num;
        }

        public static bool IsNullOrEmpty(this ICollection c)
        {
            if (c != null)
            {
                return (c.Count == 0);
            }
            return true;
        }

        public static void QuickSort<T>(this IList<T> items, IComparer<T> comparer)
        {
            QuickSort<T>(items, 0, items.Count - 1, comparer);
        }

        private static void QuickSort<T>(IList<T> items, int startIndex, int endIndex, IComparer<T> comparer)
        {
            Stack<int> stack = new Stack<int>();
            do
            {
                if (stack.Count != 0)
                {
                    endIndex = stack.Pop();
                    startIndex = stack.Pop();
                }
                T x = items[startIndex];
                int i = startIndex;
                for (int j = startIndex + 1; j <= endIndex; j++)
                {
                    if (comparer.Compare(x, items[j]) > 0)
                    {
                        i++;
                        if (i != j)
                        {
                            items.Swap<T>(i, j);
                        }
                    }
                }
                if (startIndex != i)
                {
                    items.Swap<T>(startIndex, i);
                }
                if ((i + 1) < endIndex)
                {
                    stack.Push(i + 1);
                    stack.Push(endIndex);
                }
                if (startIndex < (i - 1))
                {
                    stack.Push(startIndex);
                    stack.Push(i - 1);
                }
            }
            while (stack.Count != 0);
        }

        private static void Swap<T>(this IList<T> items, int i, int j)
        {
            T local = items[i];
            items[i] = items[j];
            items[j] = local;
        }
    }
}

