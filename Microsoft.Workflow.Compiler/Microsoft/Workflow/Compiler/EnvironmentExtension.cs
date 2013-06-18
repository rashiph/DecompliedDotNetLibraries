namespace Microsoft.Workflow.Compiler
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal static class EnvironmentExtension
    {
        private static bool? is64BitOS;
        private static bool? isWowProcess;

        private static bool CheckIf64BitOS()
        {
            return ((IntPtr.Size == 8) || IsWowProcess());
        }

        private static bool CheckIfWowProcess()
        {
            bool flag2;
            bool flag = false;
            int major = Environment.OSVersion.Version.Major;
            int minor = Environment.OSVersion.Version.Minor;
            if ((major <= 5) && ((major != 5) || (minor < 1)))
            {
                return flag;
            }
            if (!IsWow64Process(Process.GetCurrentProcess().Handle, out flag2))
            {
                throw new Win32Exception();
            }
            return flag2;
        }

        public static bool Is64BitOS()
        {
            if (!is64BitOS.HasValue)
            {
                is64BitOS = new bool?(CheckIf64BitOS());
            }
            return is64BitOS.Value;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", SetLastError=true)]
        private static extern bool IsWow64Process([In] IntPtr hProcess, out bool lpSystemInfo);
        public static bool IsWowProcess()
        {
            if (!isWowProcess.HasValue)
            {
                isWowProcess = new bool?(CheckIfWowProcess());
            }
            return isWowProcess.Value;
        }
    }
}

