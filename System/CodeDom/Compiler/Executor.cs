namespace System.CodeDom.Compiler
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    [PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
    public static class Executor
    {
        private const int ProcessTimeOut = 0x927c0;

        private static FileStream CreateInheritedFile(string file)
        {
            return new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.Inheritable | FileShare.Read);
        }

        public static void ExecWait(string cmd, TempFileCollection tempFiles)
        {
            string outputName = null;
            string errorName = null;
            ExecWaitWithCapture(cmd, tempFiles, ref outputName, ref errorName);
        }

        public static int ExecWaitWithCapture(string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
        {
            return ExecWaitWithCapture(null, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
        }

        public static int ExecWaitWithCapture(IntPtr userToken, string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName)
        {
            return ExecWaitWithCapture(new SafeUserTokenHandle(userToken, false), cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
        }

        public static int ExecWaitWithCapture(string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
        {
            return ExecWaitWithCapture(null, cmd, currentDir, tempFiles, ref outputName, ref errorName, null);
        }

        public static int ExecWaitWithCapture(IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName)
        {
            return ExecWaitWithCapture(new SafeUserTokenHandle(userToken, false), cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
        }

        internal static int ExecWaitWithCapture(SafeUserTokenHandle userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName, string trueCmdLine)
        {
            int num = 0;
            try
            {
                WindowsImpersonationContext impersonation = RevertImpersonation();
                try
                {
                    num = ExecWaitWithCaptureUnimpersonated(userToken, cmd, currentDir, tempFiles, ref outputName, ref errorName, trueCmdLine);
                }
                finally
                {
                    ReImpersonate(impersonation);
                }
            }
            catch
            {
                throw;
            }
            return num;
        }

        private static unsafe int ExecWaitWithCaptureUnimpersonated(SafeUserTokenHandle userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName, string trueCmdLine)
        {
            IntSecurity.UnmanagedCode.Demand();
            if ((outputName == null) || (outputName.Length == 0))
            {
                outputName = tempFiles.AddExtension("out");
            }
            if ((errorName == null) || (errorName.Length == 0))
            {
                errorName = tempFiles.AddExtension("err");
            }
            FileStream stream = CreateInheritedFile(outputName);
            FileStream stream2 = CreateInheritedFile(errorName);
            bool flag = false;
            Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION lpProcessInformation = new Microsoft.Win32.SafeNativeMethods.PROCESS_INFORMATION();
            Microsoft.Win32.SafeHandles.SafeProcessHandle processHandle = new Microsoft.Win32.SafeHandles.SafeProcessHandle();
            Microsoft.Win32.SafeHandles.SafeThreadHandle handle2 = new Microsoft.Win32.SafeHandles.SafeThreadHandle();
            SafeUserTokenHandle hNewToken = null;
            try
            {
                Microsoft.Win32.NativeMethods.STARTUPINFO startupinfo;
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(currentDir);
                writer.Write("> ");
                writer.WriteLine((trueCmdLine != null) ? trueCmdLine : cmd);
                writer.WriteLine();
                writer.WriteLine();
                writer.Flush();
                startupinfo = new Microsoft.Win32.NativeMethods.STARTUPINFO {
                    cb = Marshal.SizeOf(startupinfo),
                    dwFlags = 0x101,
                    wShowWindow = 0,
                    hStdOutput = stream.SafeFileHandle,
                    hStdError = stream2.SafeFileHandle,
                    hStdInput = new SafeFileHandle(Microsoft.Win32.UnsafeNativeMethods.GetStdHandle(-10), false)
                };
                StringDictionary sd = new StringDictionary();
                foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                {
                    sd[(string) entry.Key] = (string) entry.Value;
                }
                sd["_ClrRestrictSecAttributes"] = "1";
                byte[] buffer = EnvironmentBlock.ToByteArray(sd, false);
                try
                {
                    fixed (byte* numRef = buffer)
                    {
                        IntPtr lpEnvironment = new IntPtr((void*) numRef);
                        if ((userToken == null) || userToken.IsInvalid)
                        {
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                                goto Label_0325;
                            }
                            finally
                            {
                                flag = Microsoft.Win32.NativeMethods.CreateProcess(null, new StringBuilder(cmd), null, null, true, 0, lpEnvironment, currentDir, startupinfo, lpProcessInformation);
                                if ((lpProcessInformation.hProcess != IntPtr.Zero) && (lpProcessInformation.hProcess != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                                {
                                    processHandle.InitialSetHandle(lpProcessInformation.hProcess);
                                }
                                if ((lpProcessInformation.hThread != IntPtr.Zero) && (lpProcessInformation.hThread != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                                {
                                    handle2.InitialSetHandle(lpProcessInformation.hThread);
                                }
                            }
                        }
                        flag = SafeUserTokenHandle.DuplicateTokenEx(userToken, 0xf01ff, null, 2, 1, out hNewToken);
                        if (flag)
                        {
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                            }
                            finally
                            {
                                flag = Microsoft.Win32.NativeMethods.CreateProcessAsUser(hNewToken, null, cmd, null, null, true, 0, new HandleRef(null, lpEnvironment), currentDir, startupinfo, lpProcessInformation);
                                if ((lpProcessInformation.hProcess != IntPtr.Zero) && (lpProcessInformation.hProcess != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                                {
                                    processHandle.InitialSetHandle(lpProcessInformation.hProcess);
                                }
                                if ((lpProcessInformation.hThread != IntPtr.Zero) && (lpProcessInformation.hThread != Microsoft.Win32.NativeMethods.INVALID_HANDLE_VALUE))
                                {
                                    handle2.InitialSetHandle(lpProcessInformation.hThread);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    numRef = null;
                }
            }
            finally
            {
                if ((!flag && (hNewToken != null)) && !hNewToken.IsInvalid)
                {
                    hNewToken.Close();
                    hNewToken = null;
                }
                stream.Close();
                stream2.Close();
            }
        Label_0325:
            if (flag)
            {
                try
                {
                    bool flag2;
                    ProcessWaitHandle handle4 = null;
                    try
                    {
                        handle4 = new ProcessWaitHandle(processHandle);
                        flag2 = handle4.WaitOne(0x927c0, false);
                    }
                    finally
                    {
                        if (handle4 != null)
                        {
                            handle4.Close();
                        }
                    }
                    if (!flag2)
                    {
                        throw new ExternalException(SR.GetString("ExecTimeout", new object[] { cmd }), 0x102);
                    }
                    int exitCode = 0x103;
                    if (!Microsoft.Win32.NativeMethods.GetExitCodeProcess(processHandle, out exitCode))
                    {
                        throw new ExternalException(SR.GetString("ExecCantGetRetCode", new object[] { cmd }), Marshal.GetLastWin32Error());
                    }
                    return exitCode;
                }
                finally
                {
                    processHandle.Close();
                    handle2.Close();
                    if ((hNewToken != null) && !hNewToken.IsInvalid)
                    {
                        hNewToken.Close();
                    }
                }
            }
            int error = Marshal.GetLastWin32Error();
            if (error == 8)
            {
                throw new OutOfMemoryException();
            }
            Win32Exception inner = new Win32Exception(error);
            ExternalException exception2 = new ExternalException(SR.GetString("ExecCantExec", new object[] { cmd }), inner);
            throw exception2;
        }

        internal static string GetRuntimeInstallDirectory()
        {
            return RuntimeEnvironment.GetRuntimeDirectory();
        }

        internal static void ReImpersonate(WindowsImpersonationContext impersonation)
        {
            impersonation.Undo();
        }

        [SecurityPermission(SecurityAction.Assert, ControlPrincipal=true, UnmanagedCode=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
        internal static WindowsImpersonationContext RevertImpersonation()
        {
            return WindowsIdentity.Impersonate(new IntPtr(0));
        }
    }
}

