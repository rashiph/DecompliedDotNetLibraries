namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("587bf538-4d90-4a3c-9ef1-58a200a8a9e7"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDefinitionIdentity
    {
        [return: MarshalAs(UnmanagedType.LPWStr)]
        [SecurityCritical]
        string GetAttribute([In, MarshalAs(UnmanagedType.LPWStr)] string Namespace, [In, MarshalAs(UnmanagedType.LPWStr)] string Name);
        [SecurityCritical]
        void SetAttribute([In, MarshalAs(UnmanagedType.LPWStr)] string Namespace, [In, MarshalAs(UnmanagedType.LPWStr)] string Name, [In, MarshalAs(UnmanagedType.LPWStr)] string Value);
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IEnumIDENTITY_ATTRIBUTE EnumAttributes();
        [SecurityCritical]
        System.Deployment.Internal.Isolation.IDefinitionIdentity Clone([In] IntPtr cDeltas, [In, MarshalAs(UnmanagedType.LPArray)] System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[] Deltas);
    }
}

