namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System.Runtime.InteropServices;

    [InterfaceType(ComInterfaceType.InterfaceIsDual), ComVisible(true), Guid("0777432F-A60D-48b3-83DB-90326FE8C96E")]
    public interface IProductBuilder
    {
        [DispId(1)]
        Microsoft.Build.Tasks.Deployment.Bootstrapper.Product Product { get; }
    }
}

