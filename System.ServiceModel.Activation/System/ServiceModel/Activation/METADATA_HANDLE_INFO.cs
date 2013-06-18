namespace System.ServiceModel.Activation
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    internal struct METADATA_HANDLE_INFO
    {
        public uint dwMDPermissions;
        public uint dwMDSystemChangeNumber;
    }
}

