namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Guid("556BA9E0-BD6A-4837-89F0-C79B14759181")]
    public interface ITokenEnumerator
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        ITokenColorInfo GetNext();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Reset();
    }
}

