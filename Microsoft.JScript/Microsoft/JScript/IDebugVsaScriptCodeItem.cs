namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("6DFE759A-CB8B-4ca0-A973-1D04E0BF0B53"), ComVisible(true)]
    public interface IDebugVsaScriptCodeItem
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        object Evaluate();
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        bool ParseNamedBreakPoint(string input, out string functionName, out int nargs, out string arguments, out string returnType, out ulong offset);
    }
}

