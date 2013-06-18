namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    internal class Wow64Helper
    {
        private const uint ERROR_CALL_NOT_IMPLEMENTED = 120;
        private const int MAX_PATH = 260;

        private Wow64Helper()
        {
        }

        [DllImport("KERNEL32.DLL", CallingConvention=CallingConvention.StdCall, ExactSpelling=true)]
        private static extern IntPtr GetCurrentProcess();
        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern uint GetSystemWow64Directory(char[] buffer, int length);
        public static bool IsWow64Process()
        {
            bool bIsWow = false;
            if (IsWow64Supported())
            {
                try
                {
                    if (!IsWow64Process(GetCurrentProcess(), ref bIsWow))
                    {
                        throw new RegistrationException(Resource.FormatString("Reg_CannotDetermineBitness", Marshal.GetLastWin32Error()));
                    }
                }
                catch (EntryPointNotFoundException)
                {
                    bIsWow = false;
                }
            }
            return bIsWow;
        }

        [DllImport("KERNEL32.DLL", CallingConvention=CallingConvention.StdCall, ExactSpelling=true)]
        private static extern bool IsWow64Process(IntPtr hProcess, ref bool bIsWow);
        public static bool IsWow64Supported()
        {
            bool flag = false;
            char[] buffer = new char[260];
            uint num = 0;
            try
            {
                num = GetSystemWow64Directory(buffer, 260);
            }
            catch (EntryPointNotFoundException)
            {
                return flag;
            }
            if (num == 0)
            {
                int num2 = Marshal.GetLastWin32Error();
                if (num2 != 120L)
                {
                    throw new RegistrationException(Resource.FormatString("Reg_CannotDetermineWow64", num2));
                }
                return flag;
            }
            if (num <= 0)
            {
                throw new RegistrationException(Resource.FormatString("Reg_CannotDetermineWow64Ex", num));
            }
            return true;
        }
    }
}

