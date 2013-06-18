namespace Microsoft.JScript.Vsa
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Obsolete("Use of this type is not recommended because it is being deprecated in Visual Studio 2005; there will be no replacement for this feature. Please see the ICodeCompiler documentation for additional help."), ComVisible(true), Guid("172341E0-9B0D-43E6-9EFF-75E030A46461"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IJSVsaItems : IEnumerable
    {
        int Count { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        IJSVsaItem this[string name] { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        IJSVsaItem this[int index] { [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")] get; }
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        IJSVsaItem CreateItem(string name, JSVsaItemType itemType, JSVsaItemFlag itemFlag);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Remove(string name);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        void Remove(int index);
    }
}

