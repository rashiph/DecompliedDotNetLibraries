namespace System.IO.MemoryMappedFiles
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;

    public class MemoryMappedFileSecurity : ObjectSecurity<MemoryMappedFileRights>
    {
        public MemoryMappedFileSecurity() : base(false, ResourceType.KernelObject)
        {
        }

        [SecuritySafeCritical]
        internal MemoryMappedFileSecurity(SafeMemoryMappedFileHandle safeHandle, AccessControlSections includeSections) : base(false, ResourceType.KernelObject, safeHandle, includeSections)
        {
        }

        [SecuritySafeCritical]
        internal void PersistHandle(SafeHandle handle)
        {
            base.Persist(handle);
        }
    }
}

