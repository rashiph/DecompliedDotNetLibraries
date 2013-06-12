namespace System.Web.Configuration
{
    using System;
    using System.Runtime.InteropServices;

    internal interface IGac
    {
        [DispId(13)]
        void GacInstall([MarshalAs(UnmanagedType.BStr)] string assemblyPath);
    }
}

