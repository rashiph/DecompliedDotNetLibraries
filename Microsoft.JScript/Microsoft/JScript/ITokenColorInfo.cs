namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Guid("0F20D5C8-CBDB-4b64-AB7F-10B158407323")]
    public interface ITokenColorInfo
    {
        int StartPosition { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        int EndPosition { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        TokenColor Color { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
    }
}

