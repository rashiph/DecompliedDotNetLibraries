namespace System.Collections
{
    using System;

    public static class StructuralComparisons
    {
        private static IComparer s_StructuralComparer;
        private static IEqualityComparer s_StructuralEqualityComparer;

        public static IComparer StructuralComparer
        {
            get
            {
                IComparer comparer = s_StructuralComparer;
                if (comparer == null)
                {
                    comparer = new System.Collections.StructuralComparer();
                    s_StructuralComparer = comparer;
                }
                return comparer;
            }
        }

        public static IEqualityComparer StructuralEqualityComparer
        {
            get
            {
                IEqualityComparer comparer = s_StructuralEqualityComparer;
                if (comparer == null)
                {
                    comparer = new System.Collections.StructuralEqualityComparer();
                    s_StructuralEqualityComparer = comparer;
                }
                return comparer;
            }
        }
    }
}

