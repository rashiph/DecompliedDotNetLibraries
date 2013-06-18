namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Runtime.InteropServices;

    [Guid("63F63663-8503-4875-814C-09168E595367"), InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true)]
    public interface IProductCollection
    {
        [DispId(1)]
        int Count { get; }
        [DispId(2)]
        Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Item(int index);
        [DispId(3)]
        Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Product(string productCode);
    }
}

