namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("80182d03-5ea4-4831-ae97-55beffc2e590")]
    internal interface IServicePartitionConfig
    {
        void PartitionConfig(PartitionOption partitionConfig);
        void PartitionID([In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPartitionID);
    }
}

