namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("0C66F299-E08E-48c5-9264-7CCBEB4D5CBB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileAssociationEntry
    {
        FileAssociationEntry AllData { [SecurityCritical] get; }
        string Extension { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Description { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string ProgID { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string DefaultIcon { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Parameter { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

