namespace System.Linq.Parallel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Pair<T, U>
    {
        internal T m_first;
        internal U m_second;
        public Pair(T first, U second)
        {
            this.m_first = first;
            this.m_second = second;
        }

        public T First
        {
            get
            {
                return this.m_first;
            }
            set
            {
                this.m_first = value;
            }
        }
        public U Second
        {
            get
            {
                return this.m_second;
            }
            set
            {
                this.m_second = value;
            }
        }
    }
}

