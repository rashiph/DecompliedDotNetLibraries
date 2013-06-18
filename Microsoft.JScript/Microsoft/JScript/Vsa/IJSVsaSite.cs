namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("605A62B5-3BA8-49E0-A056-0A6A7A5846A3"), ComVisible(true), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public interface IJSVsaSite
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void GetCompiledState(out byte[] pe, out byte[] debugInfo);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        bool OnCompilerError(IJSVsaError error);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetGlobalInstance(string name);
        [return: MarshalAs(UnmanagedType.Interface)]
        object GetEventSourceInstance(string itemName, string eventSourceName);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Notify(string notify, object info);
    }
}

