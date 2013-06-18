namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Guid("DB283E60-7ADB-4cf6-9758-2931893A12FC"), ComVisible(true)]
    public interface IColorizeText
    {
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        ITokenEnumerator Colorize(string sourceCode, SourceState state);
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        SourceState GetStateForText(string sourceCode, SourceState currentState);
    }
}

