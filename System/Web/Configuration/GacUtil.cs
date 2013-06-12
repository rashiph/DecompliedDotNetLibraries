namespace System.Web.Configuration
{
    using System;
    using System.Security.Permissions;
    using System.Web;

    internal sealed class GacUtil : IGac
    {
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public void GacInstall(string assemblyPath)
        {
            IAssemblyCache ppAsmCache = null;
            int num = NativeMethods.CreateAssemblyCache(out ppAsmCache, 0);
            if (num == 0)
            {
                num = ppAsmCache.InstallAssembly(0, assemblyPath, IntPtr.Zero);
            }
            if (num != 0)
            {
                throw new Exception(System.Web.SR.GetString("Failed_gac_install"));
            }
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public bool GacUnInstall(string assemblyName)
        {
            IAssemblyCache ppAsmCache = null;
            uint pulDisposition = 0;
            int num2 = NativeMethods.CreateAssemblyCache(out ppAsmCache, 0);
            if (num2 == 0)
            {
                num2 = ppAsmCache.UninstallAssembly(0, assemblyName, IntPtr.Zero, out pulDisposition);
                if (pulDisposition == 3)
                {
                    return false;
                }
            }
            if (num2 != 0)
            {
                throw new Exception(System.Web.SR.GetString("Failed_gac_uninstall"));
            }
            return true;
        }
    }
}

