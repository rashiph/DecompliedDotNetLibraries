namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    [ComImport, Guid("9e3aaeb4-d1cd-11d2-bab9-00c04f8eceae"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAssemblyCacheItem
    {
        void CreateStream([MarshalAs(UnmanagedType.LPWStr)] string pszName, uint dwFormat, uint dwFlags, uint dwMaxSize, out IStream ppStream);
        void IsNameEqual(IAssemblyName pName);
        void Commit(uint dwFlags);
        void MarkAssemblyVisible(uint dwFlags);
    }
}

