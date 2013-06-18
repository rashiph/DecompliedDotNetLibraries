namespace System.IdentityModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel.Diagnostics;

    internal class Privilege
    {
        private const int ERROR_NO_TOKEN = 0x3f0;
        private const int ERROR_NOT_ALL_ASSIGNED = 0x514;
        private const int ERROR_SUCCESS = 0;
        private bool initialEnabled;
        private bool isImpersonating;
        private LUID luid;
        private static Dictionary<string, LUID> luids = new Dictionary<string, LUID>();
        private bool needToRevert;
        private string privilege;
        private const uint SE_PRIVILEGE_DISABLED = 0;
        private const uint SE_PRIVILEGE_ENABLED = 2;
        private const uint SE_PRIVILEGE_ENABLED_BY_DEFAULT = 1;
        private const uint SE_PRIVILEGE_USED_FOR_ACCESS = 0x80000000;
        public const string SeAuditPrivilege = "SeAuditPrivilege";
        public const string SeTcbPrivilege = "SeTcbPrivilege";
        private SafeCloseHandle threadToken;

        public Privilege(string privilege)
        {
            this.privilege = privilege;
            this.luid = LuidFromPrivilege(privilege);
        }

        public void Enable()
        {
            this.threadToken = this.GetThreadToken();
            this.EnableTokenPrivilege(this.threadToken);
        }

        private void EnableTokenPrivilege(SafeCloseHandle threadToken)
        {
            TOKEN_PRIVILEGE token_privilege;
            token_privilege.PrivilegeCount = 1;
            token_privilege.Privilege.Luid = this.luid;
            token_privilege.Privilege.Attributes = 2;
            uint returnLength = 0;
            bool flag = false;
            int error = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                TOKEN_PRIVILEGE token_privilege2;
                flag = System.IdentityModel.NativeMethods.AdjustTokenPrivileges(threadToken, false, ref token_privilege, TOKEN_PRIVILEGE.Size, out token_privilege2, out returnLength);
                error = Marshal.GetLastWin32Error();
                if (flag && (error == 0))
                {
                    this.initialEnabled = 0 != (token_privilege2.Privilege.Attributes & 2);
                    this.needToRevert = true;
                }
            }
            if (error == 0x514)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PrivilegeNotHeldException(this.privilege));
            }
            if (!flag)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        private SafeCloseHandle GetThreadToken()
        {
            SafeCloseHandle handle;
            if (!System.IdentityModel.NativeMethods.OpenThreadToken(System.IdentityModel.NativeMethods.GetCurrentThread(), TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query, true, out handle))
            {
                SafeCloseHandle handle2;
                int error = Marshal.GetLastWin32Error();
                Utility.CloseInvalidOutSafeHandle(handle);
                if (error != 0x3f0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
                if (!System.IdentityModel.NativeMethods.OpenProcessToken(System.IdentityModel.NativeMethods.GetCurrentProcess(), TokenAccessLevels.Duplicate, out handle2))
                {
                    error = Marshal.GetLastWin32Error();
                    Utility.CloseInvalidOutSafeHandle(handle2);
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
                try
                {
                    if (!System.IdentityModel.NativeMethods.DuplicateTokenEx(handle2, TokenAccessLevels.AdjustPrivileges | TokenAccessLevels.Query | TokenAccessLevels.Impersonate, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.Impersonation, System.IdentityModel.TokenType.TokenImpersonation, out handle))
                    {
                        error = Marshal.GetLastWin32Error();
                        Utility.CloseInvalidOutSafeHandle(handle);
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                    this.SetThreadToken(handle);
                }
                finally
                {
                    handle2.Close();
                }
            }
            return handle;
        }

        private static LUID LuidFromPrivilege(string privilege)
        {
            LUID luid;
            lock (luids)
            {
                if (luids.TryGetValue(privilege, out luid))
                {
                    return luid;
                }
            }
            if (!System.IdentityModel.NativeMethods.LookupPrivilegeValueW(null, privilege, out luid))
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            lock (luids)
            {
                if (!luids.ContainsKey(privilege))
                {
                    luids[privilege] = luid;
                }
            }
            return luid;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public int Revert()
        {
            if (!this.isImpersonating)
            {
                if (this.needToRevert && !this.initialEnabled)
                {
                    TOKEN_PRIVILEGE token_privilege;
                    TOKEN_PRIVILEGE token_privilege2;
                    token_privilege.PrivilegeCount = 1;
                    token_privilege.Privilege.Luid = this.luid;
                    token_privilege.Privilege.Attributes = 0;
                    uint returnLength = 0;
                    if (!System.IdentityModel.NativeMethods.AdjustTokenPrivileges(this.threadToken, false, ref token_privilege, TOKEN_PRIVILEGE.Size, out token_privilege2, out returnLength))
                    {
                        return Marshal.GetLastWin32Error();
                    }
                }
                this.needToRevert = false;
            }
            else
            {
                if (!System.IdentityModel.NativeMethods.RevertToSelf())
                {
                    return Marshal.GetLastWin32Error();
                }
                this.isImpersonating = false;
            }
            if (this.threadToken != null)
            {
                this.threadToken.Close();
                this.threadToken = null;
            }
            return 0;
        }

        private void SetThreadToken(SafeCloseHandle threadToken)
        {
            int error = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                if (!System.IdentityModel.NativeMethods.SetThreadToken(IntPtr.Zero, threadToken))
                {
                    error = Marshal.GetLastWin32Error();
                }
                else
                {
                    this.isImpersonating = true;
                }
            }
            if (!this.isImpersonating)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }
    }
}

