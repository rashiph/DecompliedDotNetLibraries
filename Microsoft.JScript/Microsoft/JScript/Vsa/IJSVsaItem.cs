namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help."), ComVisible(true), Guid("1F2377AC-8A09-417B-89DC-D146769F0B45")]
    public interface IJSVsaItem
    {
        string Name { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        JSVsaItemType ItemType { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        bool IsDirty { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        object GetOption(string name);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void SetOption(string name, object value);
    }
}

