namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual), Guid("0D593FC0-E3F1-4dad-A674-7EA4D327F79B")]
    public interface IProductBuilderCollection
    {
        [DispId(2)]
        void Add(ProductBuilder builder);
    }
}

