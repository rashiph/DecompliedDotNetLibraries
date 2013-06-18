namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("1D202366-5EEA-4379-9255-6F8CDB8587C9"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IBootstrapperBuilder
    {
        [DispId(1)]
        string Path { get; set; }
        [DispId(4)]
        ProductCollection Products { get; }
        [DispId(5)]
        BuildResults Build(BuildSettings settings);
    }
}

