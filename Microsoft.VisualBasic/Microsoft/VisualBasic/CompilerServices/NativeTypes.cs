namespace Microsoft.VisualBasic.CompilerServices
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class NativeTypes
    {
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_ALREADY_EXISTS = 0xb7;
        internal const int ERROR_CANCELLED = 0x4c7;
        internal const int ERROR_FILE_EXISTS = 80;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_FILENAME_EXCED_RANGE = 0xce;
        internal const int ERROR_INVALID_DRIVE = 15;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int ERROR_OPERATION_ABORTED = 0x3e3;
        internal const int ERROR_PATH_NOT_FOUND = 3;
        internal const int ERROR_SHARING_VIOLATION = 0x20;
        internal const int GW_CHILD = 5;
        internal const int GW_HWNDFIRST = 0;
        internal const int GW_HWNDLAST = 1;
        internal const int GW_HWNDNEXT = 2;
        internal const int GW_HWNDPREV = 3;
        internal const int GW_MAX = 5;
        internal const int GW_OWNER = 4;
        internal static readonly IntPtr INVALID_HANDLE = new IntPtr(-1);
        internal const int LCMAP_FULLWIDTH = 0x800000;
        internal const int LCMAP_HALFWIDTH = 0x400000;
        internal const int LCMAP_HIRAGANA = 0x100000;
        internal const int LCMAP_KATAKANA = 0x200000;
        internal const int LCMAP_LOWERCASE = 0x100;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x2000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x4000000;
        internal const int LCMAP_UPPERCASE = 0x200;
        internal const int NORMAL_PRIORITY_CLASS = 0x20;
        internal const int STARTF_USESHOWWINDOW = 1;

        private NativeTypes()
        {
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        internal sealed class LateInitSafeHandleZeroOrMinusOneIsInvalid : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            internal LateInitSafeHandleZeroOrMinusOneIsInvalid() : base(true)
            {
            }

            [SecurityCritical]
            internal void InitialSetHandle(IntPtr h)
            {
                base.SetHandle(h);
            }

            [SecurityCritical]
            protected override bool ReleaseHandle()
            {
                return (Microsoft.VisualBasic.CompilerServices.NativeMethods.CloseHandle(base.handle) != 0);
            }
        }

        [Flags]
        internal enum MoveFileExFlags
        {
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_WRITE_THROUGH = 8
        }

        [StructLayout(LayoutKind.Sequential), SuppressUnmanagedCodeSecurity, SecurityCritical]
        internal sealed class PROCESS_INFORMATION
        {
            public IntPtr hProcess = IntPtr.Zero;
            public IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
            internal PROCESS_INFORMATION()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class SECURITY_ATTRIBUTES : IDisposable
        {
            public int nLength = Marshal.SizeOf(typeof(NativeTypes.SECURITY_ATTRIBUTES));
            public IntPtr lpSecurityDescriptor;
            public bool bInheritHandle;
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
            public void Dispose()
            {
                if (this.lpSecurityDescriptor != IntPtr.Zero)
                {
                    UnsafeNativeMethods.LocalFree(this.lpSecurityDescriptor);
                    this.lpSecurityDescriptor = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
            }

            protected override void Finalize()
            {
                this.Dispose();
                base.Finalize();
            }

            internal SECURITY_ATTRIBUTES()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto), SuppressUnmanagedCodeSecurity, SecurityCritical]
        internal sealed class STARTUPINFO : IDisposable
        {
            public int cb;
            public IntPtr lpReserved = IntPtr.Zero;
            public IntPtr lpDesktop = IntPtr.Zero;
            public IntPtr lpTitle = IntPtr.Zero;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2 = IntPtr.Zero;
            public IntPtr hStdInput = IntPtr.Zero;
            public IntPtr hStdOutput = IntPtr.Zero;
            public IntPtr hStdError = IntPtr.Zero;
            private bool m_HasBeenDisposed;
            [SecuritySafeCritical, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            protected override void Finalize()
            {
                this.Dispose(false);
            }

            [SecurityCritical]
            private void Dispose(bool disposing)
            {
                if (!this.m_HasBeenDisposed && disposing)
                {
                    this.m_HasBeenDisposed = true;
                    if ((this.dwFlags & 0x100) != 0)
                    {
                        if ((this.hStdInput != IntPtr.Zero) && (this.hStdInput != NativeTypes.INVALID_HANDLE))
                        {
                            Microsoft.VisualBasic.CompilerServices.NativeMethods.CloseHandle(this.hStdInput);
                            this.hStdInput = NativeTypes.INVALID_HANDLE;
                        }
                        if ((this.hStdOutput != IntPtr.Zero) && (this.hStdOutput != NativeTypes.INVALID_HANDLE))
                        {
                            Microsoft.VisualBasic.CompilerServices.NativeMethods.CloseHandle(this.hStdOutput);
                            this.hStdOutput = NativeTypes.INVALID_HANDLE;
                        }
                        if ((this.hStdError != IntPtr.Zero) && (this.hStdError != NativeTypes.INVALID_HANDLE))
                        {
                            Microsoft.VisualBasic.CompilerServices.NativeMethods.CloseHandle(this.hStdError);
                            this.hStdError = NativeTypes.INVALID_HANDLE;
                        }
                    }
                }
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), SecuritySafeCritical]
            internal void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            internal STARTUPINFO()
            {
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal sealed class SystemTime
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
            internal SystemTime()
            {
            }
        }
    }
}

