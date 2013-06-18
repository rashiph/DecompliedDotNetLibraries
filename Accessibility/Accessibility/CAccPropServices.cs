namespace Accessibility
{
    using System.Runtime.InteropServices;

    [ComImport, Guid("6E26E776-04F0-495D-80E4-3330352E3169"), CoClass(typeof(CAccPropServicesClass))]
    public interface CAccPropServices : IAccPropServices
    {
    }
}

