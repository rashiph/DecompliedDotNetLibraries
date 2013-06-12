namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationSetCanonicalizationContext
    {
        [MarshalAs(UnmanagedType.U4)]
        public uint Size;
        [MarshalAs(UnmanagedType.U4)]
        public OpFlags Flags;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string BaseAddressFilePath;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string ExportsFilePath;
        [SecurityCritical]
        public StoreOperationSetCanonicalizationContext(string Bases, string Exports)
        {
            this.Size = (uint) Marshal.SizeOf(typeof(StoreOperationSetCanonicalizationContext));
            this.Flags = OpFlags.Nothing;
            this.BaseAddressFilePath = Bases;
            this.ExportsFilePath = Exports;
        }

        public void Destroy()
        {
        }
        [Flags]
        public enum OpFlags
        {
            Nothing
        }
    }
}

