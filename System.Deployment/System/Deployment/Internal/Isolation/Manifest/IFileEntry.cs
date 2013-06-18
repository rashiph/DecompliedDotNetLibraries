namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("A2A55FAD-349B-469b-BF12-ADC33D14A937"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileEntry
    {
        System.Deployment.Internal.Isolation.Manifest.FileEntry AllData { [SecurityCritical] get; }
        string Name { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint HashAlgorithm { [SecurityCritical] get; }
        string LoadFrom { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SourcePath { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string ImportPath { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string SourceName { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        string Location { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        object HashValue { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        ulong Size { [SecurityCritical] get; }
        string Group { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
        uint Flags { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.Manifest.IMuiResourceMapEntry MuiMapping { [SecurityCritical] get; }
        uint WritableType { [SecurityCritical] get; }
        System.Deployment.Internal.Isolation.ISection HashElements { [SecurityCritical] get; }
    }
}

