namespace System.Deployment.Application.Win32InterOp
{
    using System;
    using System.ComponentModel;
    using System.Deployment.Application;
    using System.Deployment.Internal.Isolation;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class SystemUtils
    {
        private const int MAX_CLR_VERSION_LENGTH = 0x18;

        internal static void CheckSupportedImageAndCLRVersions(string path)
        {
            uint num;
            uint num2;
            Logger.AddMethodCall("CheckSupportedImageAndCLRVersions(" + path + ") called.");
            StringBuilder szBuffer = new StringBuilder(0x18);
            try
            {
                System.Deployment.Application.NativeMethods.GetFileVersion(path, szBuffer, (uint) szBuffer.Capacity, out num);
            }
            catch (BadImageFormatException)
            {
                throw;
            }
            if (szBuffer[0] != 'v')
            {
                throw new InvalidDeploymentException(ExceptionTypes.ClrValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidCLRVersionInFile"), new object[] { szBuffer, Path.GetFileName(path) }));
            }
            Version version = new Version(szBuffer.ToString(1, szBuffer.Length - 1));
            if (version.Major < 2L)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ClrValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_ImageVersionCLRNotSupported"), new object[] { version, Path.GetFileName(path) }));
            }
            uint runtimeInfoFlags = 0x1d1;
            System.Deployment.Application.NativeMethods.GetRequestedRuntimeInfo(path, null, null, 0, runtimeInfoFlags, null, 0, out num2, szBuffer, (uint) szBuffer.Capacity, out num);
            if (szBuffer[0] != 'v')
            {
                throw new FormatException(string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_InvalidCLRVersionInFile"), new object[] { szBuffer, Path.GetFileName(path) }));
            }
            string str = szBuffer.ToString(1, szBuffer.Length - 1);
            int index = str.IndexOf(".", StringComparison.Ordinal);
            if (uint.Parse((index >= 0) ? str.Substring(0, index) : str, CultureInfo.InvariantCulture) < 2)
            {
                throw new InvalidDeploymentException(ExceptionTypes.ClrValidation, string.Format(CultureInfo.CurrentUICulture, Resources.GetString("Ex_RuntimeVersionCLRNotSupported"), new object[] { str, Path.GetFileName(path) }));
            }
        }

        internal static System.Deployment.Application.DefinitionIdentity GetDefinitionIdentityFromManagedAssembly(string filePath)
        {
            Guid guidOfType = System.Deployment.Internal.Isolation.IsolationInterop.GetGuidOfType(typeof(System.Deployment.Internal.Isolation.IReferenceIdentity));
            System.Deployment.Internal.Isolation.IReferenceIdentity assemblyIdentityFromFile = (System.Deployment.Internal.Isolation.IReferenceIdentity) System.Deployment.Application.NativeMethods.GetAssemblyIdentityFromFile(filePath, ref guidOfType);
            System.Deployment.Application.ReferenceIdentity refId = new System.Deployment.Application.ReferenceIdentity(assemblyIdentityFromFile);
            string processorArchitecture = refId.ProcessorArchitecture;
            if (processorArchitecture != null)
            {
                refId.ProcessorArchitecture = processorArchitecture.ToLower(CultureInfo.InvariantCulture);
            }
            System.Deployment.Application.DefinitionIdentity identity3 = new System.Deployment.Application.DefinitionIdentity(refId);
            Logger.AddInternalState("Managed Assembly Identity = " + identity3.ToString());
            return identity3;
        }

        public static byte[] GetManifestFromPEResources(string filePath)
        {
            IntPtr zero = IntPtr.Zero;
            IntPtr hModule = IntPtr.Zero;
            IntPtr hglobal = IntPtr.Zero;
            IntPtr source = IntPtr.Zero;
            IntPtr hFile = new IntPtr(0);
            uint dwFlags = 2;
            byte[] destination = null;
            int num2 = 0;
            try
            {
                hModule = System.Deployment.Application.NativeMethods.LoadLibraryEx(filePath, hFile, dwFlags);
                num2 = Marshal.GetLastWin32Error();
                if (hModule == IntPtr.Zero)
                {
                    Win32LoadExceptionHelper(num2, "Ex_Win32LoadException", filePath);
                }
                zero = System.Deployment.Application.NativeMethods.FindResource(hModule, "#1", "#24");
                if (!(zero != IntPtr.Zero))
                {
                    return destination;
                }
                uint num3 = System.Deployment.Application.NativeMethods.SizeofResource(hModule, zero);
                num2 = Marshal.GetLastWin32Error();
                if (num3 == 0)
                {
                    Win32LoadExceptionHelper(num2, "Ex_Win32ResourceLoadException", filePath);
                }
                hglobal = System.Deployment.Application.NativeMethods.LoadResource(hModule, zero);
                num2 = Marshal.GetLastWin32Error();
                if (hglobal == IntPtr.Zero)
                {
                    Win32LoadExceptionHelper(num2, "Ex_Win32ResourceLoadException", filePath);
                }
                source = System.Deployment.Application.NativeMethods.LockResource(hglobal);
                if (source == IntPtr.Zero)
                {
                    throw new Win32Exception(0x21);
                }
                destination = new byte[num3];
                Marshal.Copy(source, destination, 0, (int) num3);
            }
            finally
            {
                if (hModule != IntPtr.Zero)
                {
                    bool flag = System.Deployment.Application.NativeMethods.FreeLibrary(hModule);
                    num2 = Marshal.GetLastWin32Error();
                    if (!flag)
                    {
                        throw new Win32Exception(num2);
                    }
                }
            }
            return destination;
        }

        internal static AssemblyInfo QueryAssemblyInfo(System.Deployment.Application.NativeMethods.IAssemblyCache AssemblyCache, QueryAssemblyInfoFlags flags, string assemblyName)
        {
            string str = assemblyName;
            AssemblyInfo info = new AssemblyInfo();
            System.Deployment.Application.NativeMethods.AssemblyInfoInternal assemblyInfo = new System.Deployment.Application.NativeMethods.AssemblyInfoInternal();
            if ((flags & QueryAssemblyInfoFlags.GetCurrentPath) != 0)
            {
                assemblyInfo.cchBuf = 0x400;
                assemblyInfo.currentAssemblyPathBuf = Marshal.AllocHGlobal((int) (assemblyInfo.cchBuf * 2));
            }
            else
            {
                assemblyInfo.cchBuf = 0;
                assemblyInfo.currentAssemblyPathBuf = IntPtr.Zero;
            }
            try
            {
                AssemblyCache.QueryAssemblyInfo((int) flags, str, ref assemblyInfo);
            }
            catch (Exception exception)
            {
                if (ExceptionUtility.IsHardException(exception))
                {
                    throw;
                }
                Logger.AddInternalState("Exception thrown : " + exception.GetType().ToString() + ":" + exception.Message);
                info = null;
            }
            if (info != null)
            {
                info.AssemblyInfoSizeInByte = assemblyInfo.cbAssemblyInfo;
                info.AssemblyFlags = (AssemblyInfoFlags) assemblyInfo.assemblyFlags;
                info.AssemblySizeInKB = assemblyInfo.assemblySizeInKB;
                if ((flags & QueryAssemblyInfoFlags.GetCurrentPath) != 0)
                {
                    info.CurrentAssemblyPath = Marshal.PtrToStringUni(assemblyInfo.currentAssemblyPathBuf);
                    Marshal.FreeHGlobal(assemblyInfo.currentAssemblyPathBuf);
                }
            }
            return info;
        }

        private static void Win32LoadExceptionHelper(int win32ErrorCode, string resourceId, string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string message = string.Format(CultureInfo.CurrentUICulture, Resources.GetString(resourceId), new object[] { fileName, Convert.ToString(win32ErrorCode, 0x10) });
            throw new Win32Exception(win32ErrorCode, message);
        }

        internal class AssemblyInfo
        {
            private SystemUtils.AssemblyInfoFlags assemblyFlags;
            private int assemblyInfoSizeInByte;
            private long assemblySizeInKB;
            private string currentAssemblyPath;

            internal SystemUtils.AssemblyInfoFlags AssemblyFlags
            {
                set
                {
                    this.assemblyFlags = value;
                }
            }

            internal int AssemblyInfoSizeInByte
            {
                set
                {
                    this.assemblyInfoSizeInByte = value;
                }
            }

            internal long AssemblySizeInKB
            {
                set
                {
                    this.assemblySizeInKB = value;
                }
            }

            internal string CurrentAssemblyPath
            {
                set
                {
                    this.currentAssemblyPath = value;
                }
            }
        }

        internal enum AssemblyInfoFlags
        {
            Installed = 1,
            PayLoadResident = 2
        }

        [Flags]
        internal enum QueryAssemblyInfoFlags
        {
            All = 7,
            GetCurrentPath = 4,
            GetSize = 2,
            Validate = 1
        }

        private enum RUNTIME_INFO_FLAGS : uint
        {
            RUNTIME_INFO_CONSIDER_POST_2_0 = 0x80,
            RUNTIME_INFO_DONT_RETURN_DIRECTORY = 0x10,
            RUNTIME_INFO_DONT_RETURN_VERSION = 0x20,
            RUNTIME_INFO_DONT_SHOW_ERROR_DIALOG = 0x40,
            RUNTIME_INFO_EMULATE_EXE_LAUNCH = 0x100,
            RUNTIME_INFO_REQUEST_AMD64 = 4,
            RUNTIME_INFO_REQUEST_IA64 = 2,
            RUNTIME_INFO_REQUEST_X86 = 8,
            RUNTIME_INFO_UPGRADE_VERSION = 1
        }
    }
}

