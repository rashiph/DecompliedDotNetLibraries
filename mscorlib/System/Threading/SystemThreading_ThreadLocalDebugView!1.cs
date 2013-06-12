namespace System.Threading
{
    using System;

    internal sealed class SystemThreading_ThreadLocalDebugView<T>
    {
        private readonly ThreadLocal<T> m_tlocal;

        public SystemThreading_ThreadLocalDebugView(ThreadLocal<T> tlocal)
        {
            this.m_tlocal = tlocal;
        }

        public bool IsValueCreated
        {
            get
            {
                return this.m_tlocal.IsValueCreated;
            }
        }

        public T Value
        {
            get
            {
                return this.m_tlocal.ValueForDebugDisplay;
            }
        }
    }
}

