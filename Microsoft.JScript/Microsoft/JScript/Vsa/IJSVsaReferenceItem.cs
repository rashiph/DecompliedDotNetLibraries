namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), Guid("8EFD265B-677A-4B09-A471-E086787AA727"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public interface IJSVsaReferenceItem : IJSVsaItem
    {
        string AssemblyName { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
    }
}

