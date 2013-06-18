namespace System.Web
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class StringResourceManager
    {
        internal const int RESOURCE_ID = 0x65;
        internal const int RESOURCE_TYPE = 0xebb;

        private StringResourceManager()
        {
        }

        internal static SafeStringResource ReadSafeStringResource(Type t)
        {
            if (HttpRuntime.CodegenDirInternal != null)
            {
                InternalSecurityPermissions.PathDiscovery(HttpRuntime.CodegenDirInternal).Assert();
            }
            IntPtr moduleHandle = UnsafeNativeMethods.GetModuleHandle(t.Module.FullyQualifiedName);
            if (moduleHandle == IntPtr.Zero)
            {
                moduleHandle = Marshal.GetHINSTANCE(t.Module);
                if (moduleHandle == IntPtr.Zero)
                {
                    throw new HttpException(System.Web.SR.GetString("Resource_problem", new object[] { "GetModuleHandle", HttpException.HResultFromLastError(Marshal.GetLastWin32Error()).ToString(CultureInfo.InvariantCulture) }));
                }
            }
            IntPtr hResInfo = UnsafeNativeMethods.FindResource(moduleHandle, (IntPtr) 0x65, (IntPtr) 0xebb);
            if (hResInfo == IntPtr.Zero)
            {
                throw new HttpException(System.Web.SR.GetString("Resource_problem", new object[] { "FindResource", HttpException.HResultFromLastError(Marshal.GetLastWin32Error()).ToString(CultureInfo.InvariantCulture) }));
            }
            int size = UnsafeNativeMethods.SizeofResource(moduleHandle, hResInfo);
            IntPtr hResData = UnsafeNativeMethods.LoadResource(moduleHandle, hResInfo);
            if (hResData == IntPtr.Zero)
            {
                throw new HttpException(System.Web.SR.GetString("Resource_problem", new object[] { "LoadResource", HttpException.HResultFromLastError(Marshal.GetLastWin32Error()).ToString(CultureInfo.InvariantCulture) }));
            }
            IntPtr ip = UnsafeNativeMethods.LockResource(hResData);
            if (ip == IntPtr.Zero)
            {
                throw new HttpException(System.Web.SR.GetString("Resource_problem", new object[] { "LockResource", HttpException.HResultFromLastError(Marshal.GetLastWin32Error()).ToString(CultureInfo.InvariantCulture) }));
            }
            if (!UnsafeNativeMethods.IsValidResource(moduleHandle, ip, size))
            {
                throw new InvalidOperationException();
            }
            return new SafeStringResource(ip, size);
        }

        internal static unsafe string ResourceToString(IntPtr pv, int offset, int size)
        {
            return new string((sbyte*) pv, offset, size, Encoding.UTF8);
        }
    }
}

