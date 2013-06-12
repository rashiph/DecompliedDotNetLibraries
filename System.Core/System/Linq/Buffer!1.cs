namespace System.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Buffer<TElement>
    {
        internal TElement[] items;
        internal int count;
        internal Buffer(IEnumerable<TElement> source)
        {
            TElement[] array = null;
            int length = 0;
            ICollection<TElement> is2 = source as ICollection<TElement>;
            if (is2 != null)
            {
                length = is2.Count;
                if (length > 0)
                {
                    array = new TElement[length];
                    is2.CopyTo(array, 0);
                }
            }
            else
            {
                foreach (TElement local in source)
                {
                    if (array == null)
                    {
                        array = new TElement[4];
                    }
                    else if (array.Length == length)
                    {
                        TElement[] destinationArray = new TElement[length * 2];
                        Array.Copy(array, 0, destinationArray, 0, length);
                        array = destinationArray;
                    }
                    array[length] = local;
                    length++;
                }
            }
            this.items = array;
            this.count = length;
        }

        internal TElement[] ToArray()
        {
            if (this.count == 0)
            {
                return new TElement[0];
            }
            if (this.items.Length == this.count)
            {
                return this.items;
            }
            TElement[] destinationArray = new TElement[this.count];
            Array.Copy(this.items, 0, destinationArray, 0, this.count);
            return destinationArray;
        }
    }
}

