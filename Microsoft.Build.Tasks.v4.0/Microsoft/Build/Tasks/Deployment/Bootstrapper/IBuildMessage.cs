namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("E3C981EA-99E6-4f48-8955-1AAFDFB5ACE4")]
    public interface IBuildMessage
    {
        [DispId(1)]
        BuildMessageSeverity Severity { get; }
        [DispId(2)]
        string Message { get; }
        [DispId(3)]
        string HelpKeyword { get; }
        [DispId(4)]
        int HelpId { get; }
    }
}

