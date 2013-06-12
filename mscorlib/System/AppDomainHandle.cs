namespace System
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct AppDomainHandle
    {
        private IntPtr m_appDomainHandle;
        internal AppDomainHandle(IntPtr domainHandle)
        {
            this.m_appDomainHandle = domainHandle;
        }
    }
}

