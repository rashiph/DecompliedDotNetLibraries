namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    internal class EmbeddedManifestReader
    {
        private static readonly IntPtr id1 = new IntPtr(1);
        private Stream manifest;

        private EmbeddedManifestReader(string path)
        {
            IntPtr zero = IntPtr.Zero;
            try
            {
                zero = Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.LoadLibraryExW(path, IntPtr.Zero, 2);
                if (zero != IntPtr.Zero)
                {
                    Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.EnumResNameProc enumFunc = new Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.EnumResNameProc(this.EnumResNameCallback);
                    Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.EnumResourceNames(zero, Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.RT_MANIFEST, enumFunc, IntPtr.Zero);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.FreeLibrary(zero);
                }
            }
        }

        private bool EnumResNameCallback(IntPtr hModule, IntPtr pType, IntPtr pName, IntPtr param)
        {
            if (pName == id1)
            {
                IntPtr hResource = Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.FindResource(hModule, pName, Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.RT_MANIFEST);
                if (hResource == IntPtr.Zero)
                {
                    return false;
                }
                IntPtr hGlobal = Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.LoadResource(hModule, hResource);
                Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.LockResource(hGlobal);
                byte[] destination = new byte[Microsoft.Build.Tasks.Deployment.ManifestUtilities.NativeMethods.SizeofResource(hModule, hResource)];
                Marshal.Copy(hGlobal, destination, 0, destination.Length);
                this.manifest = new MemoryStream(destination, false);
            }
            return false;
        }

        public static Stream Read(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (!path.EndsWith(".manifest", StringComparison.Ordinal) && !path.EndsWith(".dll", StringComparison.Ordinal))
            {
                return null;
            }
            int tickCount = Environment.TickCount;
            EmbeddedManifestReader reader = new EmbeddedManifestReader(path);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "EmbeddedManifestReader.Read t={0}", new object[] { Environment.TickCount - tickCount }));
            return reader.manifest;
        }
    }
}

