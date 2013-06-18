namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("15B2CCE5-D1EA-4EB9-9E06-8729C72D631B"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public interface IJSVsaGlobalItem : IJSVsaItem
    {
        string TypeString { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        bool ExposeMembers { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
    }
}

