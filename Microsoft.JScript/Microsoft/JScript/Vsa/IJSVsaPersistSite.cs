namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help."), Guid("F901A1FF-8EBA-4C38-B6E0-E7E52606D325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IJSVsaPersistSite
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void SaveElement(string name, string source);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        string LoadElement(string name);
    }
}

