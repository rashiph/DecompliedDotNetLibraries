namespace System.Web.Hosting
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("F79648FB-558B-4a09-88F1-1E3BCB30E34F")]
    public interface IAppDomainInfoEnum
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        IAppDomainInfo GetData();
        [return: MarshalAs(UnmanagedType.I4)]
        int Count();
        [return: MarshalAs(UnmanagedType.Bool)]
        bool MoveNext();
        void Reset();
    }
}

