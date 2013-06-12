namespace System.StubHelpers
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SecurityCritical]
    internal sealed class CleanupWorkListElement
    {
        public SafeHandle m_handle;
        public bool m_owned;

        public CleanupWorkListElement(SafeHandle handle)
        {
            this.m_handle = handle;
        }
    }
}

