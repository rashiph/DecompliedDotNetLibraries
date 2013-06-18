namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Shared;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public static class ProcessorArchitecture
    {
        public const string AMD64 = "AMD64";
        private static string currentProcessArchitecture;
        private static bool currentProcessArchitectureInitialized;
        public const string IA64 = "IA64";
        public const string MSIL = "MSIL";
        public const string X86 = "x86";

        private static string GetCurrentProcessArchitecture()
        {
            string str = null;
            IntPtr module = NativeMethodsShared.LoadLibrary("kernel32.dll");
            try
            {
                if (!(module != NativeMethodsShared.NullIntPtr))
                {
                    return str;
                }
                IntPtr procAddress = NativeMethodsShared.GetProcAddress(module, "IsWow64Process");
                if (procAddress == NativeMethodsShared.NullIntPtr)
                {
                    return "x86";
                }
                IsWow64ProcessDelegate delegateForFunctionPointer = (IsWow64ProcessDelegate) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(IsWow64ProcessDelegate));
                bool flag = false;
                if (!delegateForFunctionPointer(Process.GetCurrentProcess().Handle, out flag))
                {
                    return str;
                }
                if (flag)
                {
                    return "x86";
                }
                NativeMethodsShared.SYSTEM_INFO lpSystemInfo = new NativeMethodsShared.SYSTEM_INFO();
                NativeMethodsShared.GetSystemInfo(ref lpSystemInfo);
                switch (lpSystemInfo.wProcessorArchitecture)
                {
                    case 0:
                        return "x86";

                    case 6:
                        return "IA64";

                    case 9:
                        return "AMD64";
                }
                str = null;
            }
            finally
            {
                if (module != NativeMethodsShared.NullIntPtr)
                {
                    NativeMethodsShared.FreeLibrary(module);
                }
            }
            return str;
        }

        public static string CurrentProcessArchitecture
        {
            get
            {
                if (!currentProcessArchitectureInitialized)
                {
                    currentProcessArchitectureInitialized = true;
                    currentProcessArchitecture = GetCurrentProcessArchitecture();
                }
                return currentProcessArchitecture;
            }
        }

        private delegate bool IsWow64ProcessDelegate([In] IntPtr hProcess, out bool Wow64Process);
    }
}

