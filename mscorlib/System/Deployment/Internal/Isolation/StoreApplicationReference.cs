namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreApplicationReference
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public RefFlags Flags;
        public Guid GuidScheme;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Identifier;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string NonCanonicalData;
        [SecuritySafeCritical]
        public StoreApplicationReference(Guid RefScheme, string Id, string NcData)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(StoreApplicationReference));
            this.Flags = RefFlags.Nothing;
            this.GuidScheme = RefScheme;
            this.Identifier = Id;
            this.NonCanonicalData = NcData;
        }

        [SecurityCritical]
        public IntPtr ToIntPtr()
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(Marshal.SizeOf(this));
            Marshal.StructureToPtr(this, ptr, false);
            return ptr;
        }

        [SecurityCritical]
        public static void Destroy(IntPtr ip)
        {
            if (ip != IntPtr.Zero)
            {
                Marshal.DestroyStructure(ip, typeof(StoreApplicationReference));
                Marshal.FreeCoTaskMem(ip);
            }
        }
        [Flags]
        public enum RefFlags
        {
            Nothing
        }
    }
}

