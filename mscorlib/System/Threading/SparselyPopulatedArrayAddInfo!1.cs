namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SparselyPopulatedArrayAddInfo<T> where T: class
    {
        private SparselyPopulatedArrayFragment<T> m_source;
        private int m_index;
        internal SparselyPopulatedArrayAddInfo(SparselyPopulatedArrayFragment<T> source, int index)
        {
            this.m_source = source;
            this.m_index = index;
        }

        internal SparselyPopulatedArrayFragment<T> Source
        {
            get
            {
                return this.m_source;
            }
        }
        internal int Index
        {
            get
            {
                return this.m_index;
            }
        }
    }
}

