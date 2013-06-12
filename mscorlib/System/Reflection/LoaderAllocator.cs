namespace System.Reflection
{
    using System;

    internal sealed class LoaderAllocator
    {
        internal CerHashtable<RuntimeMethodInfo, RuntimeMethodInfo> m_methodInstantiations;
        private LoaderAllocatorScout m_scout = new LoaderAllocatorScout();
        private object[] m_slots = new object[5];
        private int m_slotsUsed;

        private LoaderAllocator()
        {
        }
    }
}

