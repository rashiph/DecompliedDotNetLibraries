namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Policy;

    [Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help."), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComVisible(true), Guid("8FA2C97B-47E4-4A31-A7F5-FF39D1195CD9")]
    public interface IJSVsaEngine
    {
        IJSVsaSite Site { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        string Name { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        string RootMoniker { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        string RootNamespace { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        int LCID { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        bool GenerateDebugInfo { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        System.Security.Policy.Evidence Evidence { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] set; }
        IJSVsaItems Items { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        bool IsDirty { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        string Language { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        string Version { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        object GetOption(string name);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void SetOption(string name, object value);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        bool Compile();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Run();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Reset();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Close();
        bool IsRunning { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        bool IsCompiled { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void RevokeCache();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void SaveSourceState(IJSVsaPersistSite site);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void LoadSourceState(IJSVsaPersistSite site);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void SaveCompiledState(out byte[] pe, out byte[] pdb);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void InitNew();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        bool IsValidIdentifier(string identifier);
        System.Reflection.Assembly Assembly { get; }
    }
}

