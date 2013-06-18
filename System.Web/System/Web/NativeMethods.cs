namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web.Configuration;

    [ComVisible(false)]
    internal sealed class NativeMethods
    {
        private NativeMethods()
        {
        }

        [DllImport("Fusion.dll", CharSet=CharSet.Auto)]
        internal static extern int CreateAssemblyCache(out IAssemblyCache ppAsmCache, uint dwReserved);
    }
}

