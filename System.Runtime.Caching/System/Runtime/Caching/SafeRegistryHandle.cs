namespace System.Runtime.Caching
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Security;

    [SecurityCritical]
    internal class SafeRegistryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        internal SafeRegistryHandle() : base(true)
        {
        }

        [SecurityCritical]
        protected override bool ReleaseHandle()
        {
            return (UnsafeNativeMethods.RegCloseKey(base.handle) == 0);
        }
    }
}

