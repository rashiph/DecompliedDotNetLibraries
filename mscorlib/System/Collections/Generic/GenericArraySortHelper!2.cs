namespace System.Collections.Generic
{
    using System;

    internal class GenericArraySortHelper<TKey, TValue> : IArraySortHelper<TKey, TValue> where TKey: IComparable<TKey>
    {
        private static void QuickSort(TKey[] keys, TValue[] values, int left, int right)
        {
            int num;
        Label_0000:
            num = left;
            int b = right;
            int num3 = num + ((b - num) >> 1);
            GenericArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, num, num3);
            GenericArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, num, b);
            GenericArraySortHelper<TKey, TValue>.SwapIfGreaterWithItems(keys, values, num3, b);
            TKey local = keys[num3];
        Label_002F:
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
                    TKey local2 = keys[num];
                    keys[num] = keys[b];
                    keys[b] = local2;
                    if (values != null)
                    {
                        TValue local3 = values[num];
                        values[num] = values[b];
                        values[b] = local3;
                    }
                }
                num++;
                b--;
                if (num <= b)
                {
                    goto Label_002F;
                }
            }
            if ((b - left) <= (right - num))
            {
                if (left < b)
                {
                    GenericArraySortHelper<TKey, TValue>.QuickSort(keys, values, left, b);
                }
                left = num;
            }
            else
            {
                if (num < right)
                {
                    GenericArraySortHelper<TKey, TValue>.QuickSort(keys, values, num, right);
                }
                right = b;
            }
            if (left >= right)
            {
                return;
            }
            goto Label_0000;
        }

        public void Sort(TKey[] keys, TValue[] values, int index, int length, IComparer<TKey> comparer)
        {
            try
            {
                if ((comparer == null) || (comparer == Comparer<TKey>.Default))
                {
                    GenericArraySortHelper<TKey, TValue>.QuickSort(keys, values, index, (index + length) - 1);
                }
                else
                {
                    ArraySortHelper<TKey, TValue>.QuickSort(keys, values, index, (index + length) - 1, comparer);
                }
            }
            catch (IndexOutOfRangeException)
            {
                object[] objArray = new object[3];
                objArray[1] = typeof(TKey).Name;
                throw new ArgumentException(Environment.GetResourceString("Arg_BogusIComparer", objArray));
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), exception);
            }
        }

        private static void SwapIfGreaterWithItems(TKey[] keys, TValue[] values, int a, int b)
        {
            if ((a != b) && ((keys[a] == null) || (keys[a].CompareTo(keys[b]) > 0)))
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
    }
}

