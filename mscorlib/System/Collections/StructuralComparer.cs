namespace System.Collections
{
    using System;

    [Serializable]
    internal class StructuralComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            if (x == null)
            {
                if (y != null)
                {
                    return -1;
                }
                return 0;
            }
            if (y == null)
            {
                return 1;
            }
            IStructuralComparable comparable = x as IStructuralComparable;
            if (comparable != null)
            {
                return comparable.CompareTo(y, this);
            }
            return Comparer.Default.Compare(x, y);
        }
    }
}

