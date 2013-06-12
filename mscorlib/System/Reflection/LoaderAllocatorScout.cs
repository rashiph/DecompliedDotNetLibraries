namespace System.Reflection
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    internal sealed class LoaderAllocatorScout
    {
        internal IntPtr m_nativeLoaderAllocator;

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern bool Destroy(IntPtr nativeLoaderAllocator);
        [SecuritySafeCritical]
        ~LoaderAllocatorScout()
        {
            if (!this.m_nativeLoaderAllocator.IsNull() && ((!Destroy(this.m_nativeLoaderAllocator) && !Environment.HasShutdownStarted) && !AppDomain.CurrentDomain.IsFinalizingForUnload()))
            {
                GC.ReRegisterForFinalize(this);
            }
        }
    }
}

