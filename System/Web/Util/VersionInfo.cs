namespace System.Web.Util
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Web;

    internal class VersionInfo
    {
        private static string _engineVersion;
        private static string _exeName;
        private static object _lock = new object();
        private static string _mscoreeVersion;

        private VersionInfo()
        {
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        internal static string GetFileVersion(string filename)
        {
            try
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(filename);
                return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", new object[] { versionInfo.FileMajorPart, versionInfo.FileMinorPart, versionInfo.FileBuildPart, versionInfo.FilePrivatePart });
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static string GetLoadedModuleFileName(string module)
        {
            IntPtr moduleHandle = System.Web.UnsafeNativeMethods.GetModuleHandle(module);
            if (moduleHandle == IntPtr.Zero)
            {
                return null;
            }
            StringBuilder filename = new StringBuilder(0x100);
            if (System.Web.UnsafeNativeMethods.GetModuleFileName(moduleHandle, filename, 0x100) == 0)
            {
                return null;
            }
            string str = filename.ToString();
            if (StringUtil.StringStartsWith(str, @"\\?\"))
            {
                str = str.Substring(4);
            }
            return str;
        }

        internal static string GetLoadedModuleVersion(string module)
        {
            string loadedModuleFileName = GetLoadedModuleFileName(module);
            if (loadedModuleFileName == null)
            {
                return null;
            }
            return GetFileVersion(loadedModuleFileName);
        }

        internal static string ClrVersion
        {
            get
            {
                if (_mscoreeVersion == null)
                {
                    lock (_lock)
                    {
                        if (_mscoreeVersion == null)
                        {
                            _mscoreeVersion = RuntimeEnvironment.GetSystemVersion().Substring(1);
                        }
                    }
                }
                return _mscoreeVersion;
            }
        }

        internal static string EngineVersion
        {
            get
            {
                if (_engineVersion == null)
                {
                    lock (_lock)
                    {
                        if (_engineVersion == null)
                        {
                            _engineVersion = GetLoadedModuleVersion("webengine4.dll");
                        }
                    }
                }
                return _engineVersion;
            }
        }

        internal static string ExeName
        {
            get
            {
                if (_exeName == null)
                {
                    lock (_lock)
                    {
                        if (_exeName == null)
                        {
                            string loadedModuleFileName = GetLoadedModuleFileName(null);
                            if (loadedModuleFileName == null)
                            {
                                loadedModuleFileName = string.Empty;
                            }
                            int length = loadedModuleFileName.LastIndexOf('\\');
                            if (length >= 0)
                            {
                                loadedModuleFileName = loadedModuleFileName.Substring(length + 1);
                            }
                            length = loadedModuleFileName.LastIndexOf('.');
                            if (length >= 0)
                            {
                                loadedModuleFileName = loadedModuleFileName.Substring(0, length);
                            }
                            _exeName = loadedModuleFileName.ToLower(CultureInfo.InvariantCulture);
                        }
                    }
                }
                return _exeName;
            }
        }

        internal static string SystemWebVersion
        {
            get
            {
                return "4.0.30319.272";
            }
        }
    }
}

