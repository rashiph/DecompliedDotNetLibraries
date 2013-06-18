namespace System.Deployment.Internal.Isolation.Manifest
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("9D46FB70-7B54-4f4f-9331-BA9E87833FF5"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IHashElementEntry
    {
        System.Deployment.Internal.Isolation.Manifest.HashElementEntry AllData { [SecurityCritical] get; }
        uint index { [SecurityCritical] get; }
        byte Transform { [SecurityCritical] get; }
        object TransformMetadata { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        byte DigestMethod { [SecurityCritical] get; }
        object DigestValue { [return: MarshalAs(UnmanagedType.Interface)] [SecurityCritical] get; }
        string Xml { [return: MarshalAs(UnmanagedType.LPWStr)] [SecurityCritical] get; }
    }
}

