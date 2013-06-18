namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ComponentModel;
    using System.IdentityModel;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal static class SecurityUtils
    {
        private static WindowsIdentity anonymousIdentity;
        private static object lockObject = new object();
        private static WindowsIdentity processIdentity;

        public static WindowsIdentity GetAnonymousIdentity()
        {
            SafeCloseHandle tokenHandle = null;
            bool flag = false;
            lock (lockObject)
            {
                if (anonymousIdentity == null)
                {
                    try
                    {
                        try
                        {
                            if (!SafeNativeMethods.ImpersonateAnonymousUserOnCurrentThread(SafeNativeMethods.GetCurrentThread()))
                            {
                                int error = Marshal.GetLastWin32Error();
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("ImpersonateAnonymousTokenFailed", new object[] { error })));
                            }
                            flag = true;
                            if (!SafeNativeMethods.OpenCurrentThreadToken(SafeNativeMethods.GetCurrentThread(), TokenAccessLevels.Query, true, out tokenHandle))
                            {
                                int num2 = Marshal.GetLastWin32Error();
                                if (!SafeNativeMethods.RevertToSelf())
                                {
                                    num2 = Marshal.GetLastWin32Error();
                                    System.ServiceModel.DiagnosticUtility.FailFast("RevertToSelf() failed with " + num2);
                                }
                                flag = false;
                                Utility.CloseInvalidOutSafeHandle(tokenHandle);
                                throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num2, System.ServiceModel.SR.GetString("OpenThreadTokenFailed", new object[] { num2 })));
                            }
                            if (!SafeNativeMethods.RevertToSelf())
                            {
                                System.ServiceModel.DiagnosticUtility.FailFast("RevertToSelf() failed with " + Marshal.GetLastWin32Error());
                            }
                            flag = false;
                            using (tokenHandle)
                            {
                                anonymousIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle());
                            }
                        }
                        finally
                        {
                            if (flag && !SafeNativeMethods.RevertToSelf())
                            {
                                System.ServiceModel.DiagnosticUtility.FailFast("RevertToSelf() failed with " + Marshal.GetLastWin32Error());
                            }
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
            }
            return anonymousIdentity;
        }

        internal static System.ServiceModel.ComIntegration.LUID GetModifiedIDLUID(SafeCloseHandle token)
        {
            using (SafeHandle handle = GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenStatistics))
            {
                TOKEN_STATISTICS token_statistics = (TOKEN_STATISTICS) Marshal.PtrToStructure(handle.DangerousGetHandle(), typeof(TOKEN_STATISTICS));
                return token_statistics.ModifiedId;
            }
        }

        public static WindowsIdentity GetProcessIdentity()
        {
            SafeCloseHandle tokenHandle = null;
            lock (lockObject)
            {
                try
                {
                    if (!SafeNativeMethods.GetCurrentProcessToken(SafeNativeMethods.GetCurrentProcess(), TokenAccessLevels.Query, out tokenHandle))
                    {
                        int error = Marshal.GetLastWin32Error();
                        Utility.CloseInvalidOutSafeHandle(tokenHandle);
                        throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("OpenProcessTokenFailed", new object[] { error })));
                    }
                    processIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle());
                }
                finally
                {
                    if (tokenHandle != null)
                    {
                        tokenHandle.Dispose();
                    }
                }
            }
            return processIdentity;
        }

        public static SafeHandle GetTokenInformation(SafeCloseHandle token, TOKEN_INFORMATION_CLASS infoClass)
        {
            uint num;
            if (!SafeNativeMethods.GetTokenInformation(token, infoClass, SafeHGlobalHandle.InvalidHandle, 0, out num))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0x7a)
                {
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error, System.ServiceModel.SR.GetString("GetTokenInfoFailed", new object[] { error })));
                }
            }
            SafeHandle tokenInformation = SafeHGlobalHandle.AllocHGlobal(num);
            try
            {
                if (!SafeNativeMethods.GetTokenInformation(token, infoClass, tokenInformation, num, out num))
                {
                    int num3 = Marshal.GetLastWin32Error();
                    throw System.ServiceModel.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num3, System.ServiceModel.SR.GetString("GetTokenInfoFailed", new object[] { num3 })));
                }
            }
            catch
            {
                tokenInformation.Dispose();
                throw;
            }
            return tokenInformation;
        }

        internal static bool IsAtleastImpersonationToken(SafeCloseHandle token)
        {
            using (SafeHandle handle = GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenImpersonationLevel))
            {
                if (Marshal.ReadInt32(handle.DangerousGetHandle()) < 2)
                {
                    return false;
                }
                return true;
            }
        }

        internal static bool IsPrimaryToken(SafeCloseHandle token)
        {
            using (SafeHandle handle = GetTokenInformation(token, TOKEN_INFORMATION_CLASS.TokenType))
            {
                return (Marshal.ReadInt32(handle.DangerousGetHandle()) == 1);
            }
        }
    }
}

