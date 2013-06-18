namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("586B842C-D9C7-43b8-84E4-9CFC3AF9F13B"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IBuildResults
    {
        [DispId(1)]
        bool Succeeded { get; }
        [DispId(2)]
        string KeyFile { get; }
        [DispId(3)]
        string[] ComponentFiles { get; }
        [DispId(4)]
        BuildMessage[] Messages { get; }
    }
}

