namespace Microsoft.JScript.Vsa
{
    using System;
    using System.CodeDom;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("528BBC87-CCDC-4F07-B29C-9B10575DEB2F"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help.")]
    public interface IJSVsaCodeItem : IJSVsaItem
    {
        string SourceText { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        CodeObject CodeDOM { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void AppendSourceText(string text);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void AddEventSource(string eventSourceName, string eventSourceType);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void RemoveEventSource(string eventSourceName);
    }
}

