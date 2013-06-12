namespace System.Linq
{
    using System;

    internal abstract class EnumerableSorter<TElement>
    {
        protected EnumerableSorter()
        {
        }

        internal abstract int CompareKeys(int index1, int index2);
        internal abstract void ComputeKeys(TElement[] elements, int count);
        private void QuickSort(int[] map, int left, int right)
        {
            do
            {
                int index = left;
                int num2 = right;
                int num3 = map[index + ((num2 - index) >> 1)];
                do
                {
                    while ((index < map.Length) && (this.CompareKeys(num3, map[index]) > 0))
                    {
                        index++;
                    }
                    while ((num2 >= 0) && (this.CompareKeys(num3, map[num2]) < 0))
                    {
                        num2--;
                    }
                    if (index > num2)
                    {
                        break;
                    }
                    if (index < num2)
                    {
                        int num4 = map[index];
                        map[index] = map[num2];
                        map[num2] = num4;
                    }
                    index++;
                    num2--;
                }
                while (index <= num2);
                if ((num2 - left) <= (right - index))
                {
                    if (left < num2)
                    {
                        this.QuickSort(map, left, num2);
                    }
                    left = index;
                }
                else
                {
                    if (index < right)
                    {
                        this.QuickSort(map, index, right);
                    }
                    right = num2;
                }
            }
            while (left < right);
        }

        internal int[] Sort(TElement[] elements, int count)
        {
            this.ComputeKeys(elements, count);
            int[] map = new int[count];
            for (int i = 0; i < count; i++)
            {
                map[i] = i;
            }
            this.QuickSort(map, 0, count - 1);
            return map;
        }
    }
}

