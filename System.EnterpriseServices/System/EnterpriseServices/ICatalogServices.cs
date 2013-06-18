namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("04C6BE1E-1DB1-4058-AB7A-700CCCFBF254"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ICatalogServices
    {
        [AutoComplete(true)]
        void Autodone();
        [AutoComplete(false)]
        void NotAutodone();
    }
}

