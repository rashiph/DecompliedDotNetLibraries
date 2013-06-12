namespace System.Collections.Generic
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;

    [TypeDependency("System.Collections.Generic.GenericArraySortHelper`2")]
    internal class ArraySortHelper<TKey, TValue> : IArraySortHelper<TKey, TValue>
    {
        private static IArraySortHelper<TKey, TValue> defaultArraySortHelper;

        [SecuritySafeCritical]
        public static IArraySortHelper<TKey, TValue> CreateArraySortHelper()
        {
            if (typeof(IComparable<TKey>).IsAssignableFrom(typeof(TKey)))
            {
                ArraySortHelper<TKey, TValue>.defaultArraySortHelper = (IArraySortHelper<TKey, TValue>) RuntimeTypeHandle.Allocate(typeof(GenericArraySortHelper<string, string>).TypeHandle.Instantiate(new Type[] { typeof(TKey), typeof(TValue) }));
            }
            else
            {
                ArraySortHelper<TKey, TValue>.defaultArraySortHelper = new ArraySortHelper<TKey, TValue>();
            }
            return ArraySortHelper<TKey, TValue>.defaultArraySortHelper;
        }

        internal static void QuickSort(TKey[] keys, TValue[] values, int left, int right, IComparer<TKey> comparer)
        {
            do
            {
                int a = left;
                int b = right;
                int num3 = a + ((b - a) >> 1);
                ArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, comparer, a, num3);
                ArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, comparer, a, b);
                ArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, comparer, num3, b);
                TKey y = keys[num3];
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
                        TKey local2 = keys[a];
                        keys[a] = keys[b];
                        keys[b] = local2;
                        if (values != null)
                        {
                            TValue local3 = values[a];
                            values[a] = values[b];
                            values[b] = local3;
                        }
                    }
                    a++;
                    b--;
                }
                while (a <= b);
                if ((b - left) <= (right - a))
                {
                    if (left < b)
                    {
                        ArraySortHelper<TKey, TValue>.QuickSort(keys, values, left, b, comparer);
                    }
                    left = a;
                }
                else
                {
                    if (a < right)
                    {
                        ArraySortHelper<TKey, TValue>.QuickSort(keys, values, a, right, comparer);
                    }
                    right = b;
                }
            }
            while (left < right);
        }

        public void Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer)
        {
            try
            {
                if ((comparer == null) || (comparer == Comparer<TKey>.Default))
                {
                    comparer = Comparer<TKey>.Default;
                }
                ArraySortHelper<TKey, TValue>.QuickSort(keys, values, index, index + (length - 1), comparer);
            }
            catch (IndexOutOfRangeException)
            {
                object[] objArray = new object[3];
                objArray[1] = typeof(TKey).Name;
                objArray[2] = comparer;
                throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", objArray));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
            }
        }

        private static void SwapIfGreaterWithItems(TKey[] keys, TValue[] values, IComparer<TKey> comparer, int a, int b)
        {
            if (((a != b) && (a != b)) && (comparer.Compare(keys[a], keys[b]) > 0))
            {
                TKey local = keys[a];
                keys[a] = keys[b];
                keys[b] = local;
                if (values != null)
                {
                    TValue local2 = values[a];
                    values[a] = values[b];
                    values[b] = local2;
                }
            }
        }

        public static IArraySortHelper<TKey, TValue> Default
        {
            get
            {
                IArraySortHelper<TKey, TValue> defaultArraySortHelper = ArraySortHelper<TKey, TValue>.defaultArraySortHelper;
                if (defaultArraySortHelper == null)
                {
                    defaultArraySortHelper = ArraySortHelper<TKey, TValue>.CreateArraySortHelper();
                }
                return defaultArraySortHelper;
            }
        }
    }
}

