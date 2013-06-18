namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("87EEBC69-0948-4ce6-A2DE-819162B87CC6"), ComVisible(true)]
    public interface IBuildSettings
    {
        [DispId(1)]
        string ApplicationName { get; set; }
        [DispId(2)]
        string ApplicationFile { get; set; }
        [DispId(3)]
        string ApplicationUrl { get; set; }
        [DispId(4)]
        string ComponentsUrl { get; set; }
        [DispId(5)]
        bool CopyComponents { get; set; }
        [DispId(6)]
        int LCID { get; set; }
        [DispId(7)]
        int FallbackLCID { get; set; }
        [DispId(8)]
        string OutputPath { get; set; }
        [DispId(9)]
        ProductBuilderCollection ProductBuilders { get; }
        [DispId(10)]
        bool Validate { get; set; }
        [DispId(11)]
        Microsoft.Build.Tasks.Deployment.Bootstrapper.ComponentsLocation ComponentsLocation { get; set; }
        [DispId(12)]
        string SupportUrl { get; set; }
        [DispId(13)]
        bool ApplicationRequiresElevation { get; set; }
    }
}

