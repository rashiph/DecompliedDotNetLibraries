namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        public const uint LOAD_LIBRARY_AS_DATAFILE = 2;
        public static readonly IntPtr RT_MANIFEST = new IntPtr(0x18);

        [DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern int EnumResourceNames(IntPtr hModule, IntPtr pType, EnumResNameProc enumFunc, IntPtr param);
        [DllImport("Kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        public static extern IntPtr FindResource(IntPtr hModule, IntPtr pName, IntPtr pType);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern bool FreeLibrary(IntPtr hModule);
        [return: MarshalAs(UnmanagedType.IUnknown)]
        [DllImport("mscorwks.dll", CharSet=CharSet.Unicode, ExactSpelling=true, PreserveSig=false)]
        public static extern object GetAssemblyIdentityFromFile([In, MarshalAs(UnmanagedType.LPWStr)] string filePath, [In] ref Guid riid);
        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern IntPtr LoadLibraryExW(string strFileName, IntPtr hFile, uint ulFlags);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResource);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
        public static extern void LoadTypeLibEx(string strTypeLibName, RegKind regKind, [MarshalAs(UnmanagedType.Interface)] out object typeLib);
        [DllImport("Kernel32.dll")]
        public static extern IntPtr LockResource(IntPtr hGlobal);
        [DllImport("sfc.dll", CharSet=CharSet.Unicode)]
        public static extern int SfcIsFileProtected(IntPtr RpcHandle, string ProtFileName);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern uint SizeofResource(IntPtr hModule, IntPtr hResource);

        public delegate bool EnumResNameProc(IntPtr hModule, IntPtr pType, IntPtr pName, IntPtr param);

        public enum RegKind
        {
            RegKind_Default,
            RegKind_Register,
            RegKind_None
        }
    }
}

