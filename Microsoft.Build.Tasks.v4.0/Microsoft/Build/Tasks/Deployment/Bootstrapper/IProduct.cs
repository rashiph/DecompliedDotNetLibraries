namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("9E81BE3D-530F-4a10-8349-5D5947BA59AD"), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IProduct
    {
        [DispId(1)]
        Microsoft.Build.Tasks.Deployment.Bootstrapper.ProductBuilder ProductBuilder { get; }
        [DispId(2)]
        string Name { get; }
        [DispId(3)]
        string ProductCode { get; }
        [DispId(4)]
        ProductCollection Includes { get; }
    }
}

