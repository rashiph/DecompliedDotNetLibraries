namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help."), Guid("425EA439-6417-4F3E-BCC9-1AFAC79E3F66"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IJSVsaError
    {
        int Line { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        int Severity { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        string Description { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        string LineText { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        IJSVsaItem SourceItem { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        int EndColumn { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        int StartColumn { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        int Number { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        string SourceMoniker { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
    }
}

