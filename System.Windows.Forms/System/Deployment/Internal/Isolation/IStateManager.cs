namespace System.Deployment.Internal.Isolation
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComImport, Guid("07662534-750b-4ed5-9cfb-1c5bc5acfd07"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IStateManager
    {
        [SecurityCritical]
        void PrepareApplicationState([In] UIntPtr Inputs, ref UIntPtr Outputs);
        [SecurityCritical]
        void SetApplicationRunningState([In] uint Flags, [In] System.Deployment.Internal.Isolation.IActContext Context, [In] uint RunningState, out uint Disposition);
        [SecurityCritical]
        void GetApplicationStateFilesystemLocation([In] uint Flags, [In] System.Deployment.Internal.Isolation.IDefinitionAppId Appidentity, [In] System.Deployment.Internal.Isolation.IDefinitionIdentity ComponentIdentity, [In] UIntPtr Coordinates, [MarshalAs(UnmanagedType.LPWStr)] out string Path);
        [SecurityCritical]
        void Scavenge([In] uint Flags, out uint Disposition);
    }
}

