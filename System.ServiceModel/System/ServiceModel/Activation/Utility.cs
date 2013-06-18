namespace System.ServiceModel.Activation
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.ComIntegration;
    using System.ServiceModel.Diagnostics;
    using System.Text;

    internal static class Utility
    {
        private const string WindowsServiceAccountFormat = @"NT Service\{0}";

        internal static void AddRightGrantedToAccount(SecurityIdentifier account, int right)
        {
            SafeCloseHandle kernelObject = OpenCurrentProcessForWrite();
            try
            {
                EditKernelObjectSecurity(kernelObject, null, account, right, true);
            }
            finally
            {
                kernelObject.Close();
            }
        }

        internal static void AddRightGrantedToAccounts(List<SecurityIdentifier> accounts, int right, bool onProcess)
        {
            SafeCloseHandle kernelObject = OpenCurrentProcessForWrite();
            try
            {
                if (onProcess)
                {
                    EditKernelObjectSecurity(kernelObject, accounts, null, right, true);
                }
                else
                {
                    SafeCloseHandle processToken = GetProcessToken(kernelObject, 0x60008);
                    try
                    {
                        EditKernelObjectSecurity(processToken, accounts, null, right, true);
                    }
                    finally
                    {
                        processToken.Close();
                    }
                }
            }
            finally
            {
                kernelObject.Close();
            }
        }

        private static void EditDacl(DiscretionaryAcl dacl, SecurityIdentifier account, int right, bool add)
        {
            if (add)
            {
                dacl.AddAccess(AccessControlType.Allow, account, right, InheritanceFlags.None, PropagationFlags.None);
            }
            else
            {
                dacl.RemoveAccess(AccessControlType.Allow, account, right, InheritanceFlags.None, PropagationFlags.None);
            }
        }

        private static void EditKernelObjectSecurity(SafeCloseHandle kernelObject, List<SecurityIdentifier> accounts, SecurityIdentifier account, int right, bool add)
        {
            int num;
            if (!ListenerUnsafeNativeMethods.GetKernelObjectSecurity(kernelObject, 4, null, 0, out num))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0x7a)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }
            byte[] pSecurityDescriptor = new byte[num];
            if (!ListenerUnsafeNativeMethods.GetKernelObjectSecurity(kernelObject, 4, pSecurityDescriptor, pSecurityDescriptor.Length, out num))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
            CommonSecurityDescriptor descriptor = new CommonSecurityDescriptor(false, false, pSecurityDescriptor, 0);
            DiscretionaryAcl discretionaryAcl = descriptor.DiscretionaryAcl;
            if (account != null)
            {
                EditDacl(discretionaryAcl, account, right, add);
            }
            else if (accounts != null)
            {
                foreach (SecurityIdentifier identifier in accounts)
                {
                    EditDacl(discretionaryAcl, identifier, right, add);
                }
            }
            pSecurityDescriptor = new byte[descriptor.BinaryLength];
            descriptor.GetBinaryForm(pSecurityDescriptor, 0);
            if (!ListenerUnsafeNativeMethods.SetKernelObjectSecurity(kernelObject, 4, pSecurityDescriptor))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
            }
        }

        internal static Uri FormatListenerEndpoint(string serviceName, string listenerEndPoint)
        {
            UriBuilder builder = new UriBuilder(Uri.UriSchemeNetPipe, serviceName) {
                Path = string.Format(CultureInfo.InvariantCulture, "/{0}/", new object[] { listenerEndPoint })
            };
            return builder.Uri;
        }

        internal static unsafe SecurityIdentifier GetLogonSidForPid(int pid)
        {
            SecurityIdentifier identifier;
            SafeCloseHandle process = OpenProcessForQuery(pid);
            try
            {
                SafeCloseHandle processToken = GetProcessToken(process, 8);
                try
                {
                    byte[] tokenInformation = new byte[GetTokenInformationLength(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups)];
                    try
                    {
                        fixed (byte* numRef = tokenInformation)
                        {
                            GetTokenInformation(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenGroups, tokenInformation);
                            ListenerUnsafeNativeMethods.TOKEN_GROUPS* token_groupsPtr = (ListenerUnsafeNativeMethods.TOKEN_GROUPS*) numRef;
                            ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES* sid_and_attributesPtr = (ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES*) &token_groupsPtr->Groups;
                            for (int i = 0; i < token_groupsPtr->GroupCount; i++)
                            {
                                if ((sid_and_attributesPtr[i].Attributes & ((ListenerUnsafeNativeMethods.SidAttribute) (-1073741824))) == ((ListenerUnsafeNativeMethods.SidAttribute) (-1073741824)))
                                {
                                    return new SecurityIdentifier(sid_and_attributesPtr[i].Sid);
                                }
                            }
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                    identifier = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
                }
                finally
                {
                    processToken.Close();
                }
            }
            finally
            {
                process.Close();
            }
            return identifier;
        }

        internal static int GetPidForService(string serviceName)
        {
            return GetStatusForService(serviceName).dwProcessId;
        }

        private static SafeCloseHandle GetProcessToken(SafeCloseHandle process, int requiredAccess)
        {
            SafeCloseHandle handle;
            bool flag = ListenerUnsafeNativeMethods.OpenProcessToken(process, requiredAccess, out handle);
            int error = Marshal.GetLastWin32Error();
            if (!flag)
            {
                System.ServiceModel.Diagnostics.Utility.CloseInvalidOutSafeHandle(handle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return handle;
        }

        private static unsafe ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS GetStatusForService(string serviceName)
        {
            ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS service_status_process;
            SafeServiceHandle scManager = OpenSCManager();
            try
            {
                SafeServiceHandle hService = OpenService(scManager, serviceName, 4);
                try
                {
                    int num;
                    if (!ListenerUnsafeNativeMethods.QueryServiceStatusEx(hService, 0, null, 0, out num))
                    {
                        int error = Marshal.GetLastWin32Error();
                        if (error != 0x7a)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                        }
                    }
                    byte[] pBuffer = new byte[num];
                    if (!ListenerUnsafeNativeMethods.QueryServiceStatusEx(hService, 0, pBuffer, pBuffer.Length, out num))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception());
                    }
                    try
                    {
                        fixed (byte* numRef = pBuffer)
                        {
                            service_status_process = (ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS) Marshal.PtrToStructure((IntPtr) numRef, typeof(ListenerUnsafeNativeMethods.SERVICE_STATUS_PROCESS));
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                }
                finally
                {
                    hService.Close();
                }
            }
            finally
            {
                scManager.Close();
            }
            return service_status_process;
        }

        private static void GetTokenInformation(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic, byte[] tokenInformation)
        {
            int num;
            if (!ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, tokenInformation, tokenInformation.Length, out num))
            {
                int error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
        }

        private static int GetTokenInformationLength(SafeCloseHandle token, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS tic)
        {
            int num;
            if (!ListenerUnsafeNativeMethods.GetTokenInformation(token, tic, null, 0, out num))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != 0x7a)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }
            return num;
        }

        internal static unsafe SecurityIdentifier GetUserSidForPid(int pid)
        {
            SecurityIdentifier identifier;
            SafeCloseHandle process = OpenProcessForQuery(pid);
            try
            {
                SafeCloseHandle processToken = GetProcessToken(process, 8);
                try
                {
                    byte[] tokenInformation = new byte[GetTokenInformationLength(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenUser)];
                    try
                    {
                        fixed (byte* numRef = tokenInformation)
                        {
                            GetTokenInformation(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenUser, tokenInformation);
                            ListenerUnsafeNativeMethods.TOKEN_USER* token_userPtr = (ListenerUnsafeNativeMethods.TOKEN_USER*) numRef;
                            ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES* sid_and_attributesPtr = (ListenerUnsafeNativeMethods.SID_AND_ATTRIBUTES*) &token_userPtr->User;
                            identifier = new SecurityIdentifier(sid_and_attributesPtr->Sid);
                        }
                    }
                    finally
                    {
                        numRef = null;
                    }
                }
                finally
                {
                    processToken.Close();
                }
            }
            finally
            {
                process.Close();
            }
            return identifier;
        }

        internal static SecurityIdentifier GetWindowsServiceSid(string name)
        {
            short num3;
            string accountName = string.Format(CultureInfo.InvariantCulture, @"NT Service\{0}", new object[] { name });
            byte[] sid = null;
            uint cbSid = 0;
            uint cchReferencedDomainName = 0;
            int error = 0;
            if (!ListenerUnsafeNativeMethods.LookupAccountName(null, accountName, sid, ref cbSid, null, ref cchReferencedDomainName, out num3))
            {
                error = Marshal.GetLastWin32Error();
                if (error != 0x7a)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                }
            }
            sid = new byte[cbSid];
            StringBuilder referencedDomainName = new StringBuilder((int) cchReferencedDomainName);
            if (!ListenerUnsafeNativeMethods.LookupAccountName(null, accountName, sid, ref cbSid, referencedDomainName, ref cchReferencedDomainName, out num3))
            {
                error = Marshal.GetLastWin32Error();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            return new SecurityIdentifier(sid, 0);
        }

        internal static unsafe void KeepOnlyPrivilegeInProcess(string privilege)
        {
            SafeCloseHandle process = OpenCurrentProcessForWrite();
            try
            {
                SafeCloseHandle processToken = GetProcessToken(process, 0x20028);
                try
                {
                    LUID luid;
                    if (!ListenerUnsafeNativeMethods.LookupPrivilegeValue(IntPtr.Zero, privilege, &luid))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
                    }
                    byte[] tokenInformation = new byte[GetTokenInformationLength(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenPrivileges)];
                    try
                    {
                        fixed (byte* numRef = tokenInformation)
                        {
                            GetTokenInformation(processToken, ListenerUnsafeNativeMethods.TOKEN_INFORMATION_CLASS.TokenPrivileges, tokenInformation);
                            ListenerUnsafeNativeMethods.TOKEN_PRIVILEGES* newState = (ListenerUnsafeNativeMethods.TOKEN_PRIVILEGES*) numRef;
                            LUID_AND_ATTRIBUTES* luid_and_attributesPtr = &newState->Privileges;
                            int index = 0;
                            for (int i = 0; i < newState->PrivilegeCount; i++)
                            {
                                if (!luid_and_attributesPtr[i].Luid.Equals(luid))
                                {
                                    luid_and_attributesPtr[index].Attributes = PrivilegeAttribute.SE_PRIVILEGE_DISABLED | PrivilegeAttribute.SE_PRIVILEGE_REMOVED;
                                    luid_and_attributesPtr[index].Luid = luid_and_attributesPtr[i].Luid;
                                    index++;
                                }
                            }
                            newState->PrivilegeCount = index;
                            bool flag = ListenerUnsafeNativeMethods.AdjustTokenPrivileges(processToken, false, newState, tokenInformation.Length, IntPtr.Zero, IntPtr.Zero);
                            int num5 = Marshal.GetLastWin32Error();
                            if (!flag || (num5 != 0))
                            {
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(num5));
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
                    processToken.Close();
                }
            }
            finally
            {
                process.Close();
            }
        }

        private static SafeCloseHandle OpenCurrentProcessForWrite()
        {
            int id = Process.GetCurrentProcess().Id;
            SafeCloseHandle handle = ListenerUnsafeNativeMethods.OpenProcess(0x60400, false, id);
            if (handle.IsInvalid)
            {
                Exception exception = new Win32Exception();
                handle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return handle;
        }

        private static SafeCloseHandle OpenProcessForQuery(int pid)
        {
            SafeCloseHandle handle = ListenerUnsafeNativeMethods.OpenProcess(0x400, false, pid);
            if (handle.IsInvalid)
            {
                Exception exception = new Win32Exception();
                handle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return handle;
        }

        private static SafeServiceHandle OpenSCManager()
        {
            SafeServiceHandle handle = ListenerUnsafeNativeMethods.OpenSCManager(null, null, 1);
            if (handle.IsInvalid)
            {
                Exception exception = new Win32Exception();
                handle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return handle;
        }

        private static SafeServiceHandle OpenService(SafeServiceHandle scManager, string serviceName, int purpose)
        {
            SafeServiceHandle handle = ListenerUnsafeNativeMethods.OpenService(scManager, serviceName, purpose);
            if (handle.IsInvalid)
            {
                Exception exception = new Win32Exception();
                handle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            return handle;
        }

        internal static void RemoveRightGrantedToAccount(SecurityIdentifier account, int right)
        {
            SafeCloseHandle kernelObject = OpenCurrentProcessForWrite();
            try
            {
                EditKernelObjectSecurity(kernelObject, null, account, right, false);
            }
            finally
            {
                kernelObject.Close();
            }
        }
    }
}

