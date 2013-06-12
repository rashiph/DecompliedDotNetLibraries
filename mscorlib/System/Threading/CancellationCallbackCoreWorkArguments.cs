namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CancellationCallbackCoreWorkArguments
    {
        internal SparselyPopulatedArrayFragment<CancellationCallbackInfo> m_currArrayFragment;
        internal int m_currArrayIndex;
        public CancellationCallbackCoreWorkArguments(SparselyPopulatedArrayFragment<CancellationCallbackInfo> currArrayFragment, int currArrayIndex)
        {
            this.m_currArrayFragment = currArrayFragment;
            this.m_currArrayIndex = currArrayIndex;
        }
    }
}

