namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct StoreOperationMetadataProperty
    {
        public Guid GuidPropertySet;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
        [MarshalAs(UnmanagedType.SysUInt)]
        public IntPtr ValueSize;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
        public StoreOperationMetadataProperty(Guid PropertySet, string Name) : this(PropertySet, Name, null)
        {
        }

        public StoreOperationMetadataProperty(Guid PropertySet, string Name, string Value)
        {
            this.GuidPropertySet = PropertySet;
            this.Name = Name;
            this.Value = Value;
            this.ValueSize = (Value != null) ? new IntPtr((Value.Length + 1) * 2) : IntPtr.Zero;
        }
    }
}

