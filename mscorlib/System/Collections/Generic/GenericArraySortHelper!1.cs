namespace System.Collections.Generic
{
    using System;

    [Serializable]
    internal class GenericArraySortHelper<T> : IArraySortHelper<T> where T: IComparable<T>
    {
        private static int BinarySearch(T[] array, int index, int length, T value)
        {
            int num = index;
            int num2 = (index + length) - 1;
            while (num <= num2)
            {
                int num4;
                int num3 = num + ((num2 - num) >> 1);
                if (array[num3] == null)
                {
                    num4 = (value == null) ? 0 : -1;
                }
                else
                {
                    num4 = array[num3].CompareTo(value);
                }
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

        public int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer)
        {
            int num;
            try
            {
                if ((comparer == null) || (comparer == Comparer<T>.Default))
                {
                    return GenericArraySortHelper<T>.BinarySearch(array, index, length, value);
                }
                num = ArraySortHelper<T>.InternalBinarySearch(array, index, length, value, comparer);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
            }
            return num;
        }

        private static void QuickSort(T[] keys, int left, int right)
        {
            int num;
        Label_0000:
            num = left;
            int b = right;
            int num3 = num + ((b - num) >> 1);
            GenericArraySortHelper<T>.SwapIfGreaterWithItems(keys, num, num3);
            GenericArraySortHelper<T>.SwapIfGreaterWithItems(keys, num, b);
            GenericArraySortHelper<T>.SwapIfGreaterWithItems(keys, num3, b);
            T local = keys[num3];
        Label_002C:
            if (local != null)
            {
                while (local.CompareTo(keys[num]) > 0)
                {
                    num++;
                }
                while (local.CompareTo(keys[b]) < 0)
                {
                    b--;
                }
            }
            else
            {
                while (keys[b] != null)
                {
                    b--;
                }
            }
            if (num <= b)
            {
                if (num < b)
                {
                    T local2 = keys[num];
                    keys[num] = keys[b];
                    keys[b] = local2;
                }
                num++;
                b--;
                if (num <= b)
                {
                    goto Label_002C;
                }
            }
            if ((b - left) <= (right - num))
            {
                if (left < b)
                {
                    GenericArraySortHelper<T>.QuickSort(keys, left, b);
                }
                left = num;
            }
            else
            {
                if (num < right)
                {
                    GenericArraySortHelper<T>.QuickSort(keys, num, right);
                }
                right = b;
            }
            if (left >= right)
            {
                return;
            }
            goto Label_0000;
        }

        public void Sort(T[] keys, int index, int length, IComparer<T> comparer)
        {
            try
            {
                if ((comparer == null) || (comparer == Comparer<T>.Default))
                {
                    GenericArraySortHelper<T>.QuickSort(keys, index, index + (length - 1));
                }
                else
                {
                    ArraySortHelper<T>.QuickSort(keys, index, index + (length - 1), comparer);
                }
            }
            catch (IndexOutOfRangeException)
            {
                object[] values = new object[3];
                values[0] = default(T);
                values[1] = typeof(T).Name;
                throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", values));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
            }
        }

        private static void SwapIfGreaterWithItems(T[] keys, int a, int b)
        {
            if ((a != b) && ((keys[a] == null) || (keys[a].CompareTo(keys[b]) > 0)))
            {
                T local = keys[a];
                keys[a] = keys[b];
                keys[b] = local;
            }
        }
    }
}

