namespace System.Collections.Generic
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    [TypeDependency("System.Collections.Generic.GenericArraySortHelper`1")]
    internal class ArraySortHelper<T> : IArraySortHelper<T>
    {
        private static IArraySortHelper<T> defaultArraySortHelper;

        public int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
        {
            int num;
            try
            {
                if (comparer == null)
                {
                    comparer = Comparer<T>.Default;
                }
                num = ArraySortHelper<T>.InternalBinarySearch(array, index, length, value, comparer);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
            }
            return num;
        }

        [SecuritySafeCritical]
        private static IArraySortHelper<T> CreateArraySortHelper()
        {
            if (typeof(IComparable<T>).IsAssignableFrom(typeof(T)))
            {
                ArraySortHelper<T>.defaultArraySortHelper = (IArraySortHelper<T>) RuntimeTypeHandle.Allocate(typeof(GenericArraySortHelper<string>).TypeHandle.Instantiate(new Type[] { typeof(T) }));
            }
            else
            {
                ArraySortHelper<T>.defaultArraySortHelper = new ArraySortHelper<T>();
            }
            return ArraySortHelper<T>.defaultArraySortHelper;
        }

        internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
        {
            int num = index;
            int num2 = (index + length) - 1;
            while (num <= num2)
            {
                int num3 = num + ((num2 - num) >> 1);
                int num4 = comparer.Compare(array[num3], value);
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

        internal static void QuickSort(T[] keys, int left, int right, IComparer<T> comparer)
        {
            do
            {
                int a = left;
                int b = right;
                int num3 = a + ((b - a) >> 1);
                ArraySortHelper<T>.SwapIfGreaterWithItems(keys, comparer, a, num3);
                ArraySortHelper<T>.SwapIfGreaterWithItems(keys, comparer, a, b);
                ArraySortHelper<T>.SwapIfGreaterWithItems(keys, comparer, num3, b);
                T y = keys[num3];
                do
                {
                    while (comparer.Compare(keys[a], y) < 0)
                    {
                        a++;
                    }
                    while (comparer.Compare(y, keys[b]) < 0)
                    {
                        b--;
                    }
                    if (a > b)
                    {
                        break;
                    }
                    if (a < b)
                    {
                        T local2 = keys[a];
                        keys[a] = keys[b];
                        keys[b] = local2;
                    }
                    a++;
                    b--;
                }
                while (a <= b);
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        ArraySortHelper<T>.QuickSort(keys, left, b, comparer);
                    }
                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        ArraySortHelper<T>.QuickSort(keys, a, right, comparer);
                    }
                    right = b;
                }
            }
            while (left < right);
        }

        public void Sort(T[] keys, int index, int length, IComparer<T> comparer)
        {
            try
            {
                if (comparer == null)
                {
                    comparer = Comparer<T>.Default;
                }
                ArraySortHelper<T>.QuickSort(keys, index, index + (length - 1), comparer);
            }
            catch (IndexOutOfRangeException)
            {
                object[] values = new object[3];
                values[1] = typeof(T).Name;
                values[2] = comparer;
                throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", values));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
            }
        }

        private static void SwapIfGreaterWithItems(T[] keys, IComparer<T> comparer, int a, int b)
        {
            if ((a != b) && (comparer.Compare(keys[a], keys[b]) > 0))
            {
                T local = keys[a];
                keys[a] = keys[b];
                keys[b] = local;
            }
        }

        public static IArraySortHelper<T> Default
        {
            get
            {
                IArraySortHelper<T> defaultArraySortHelper = ArraySortHelper<T>.defaultArraySortHelper;
                if (defaultArraySortHelper == null)
                {
                    defaultArraySortHelper = ArraySortHelper<T>.CreateArraySortHelper();
                }
                return defaultArraySortHelper;
            }
        }
    }
}

