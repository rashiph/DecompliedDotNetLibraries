namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal class TrustHelper
    {
        private static int ERROR_ALREADY_EXISTS = 0xb7;
        private static int ERROR_INVALID_LEVEL = 0x7c;
        internal static int ERROR_NOT_FOUND = 0x490;
        internal static int NETLOGON_CONTROL_REDISCOVER = 5;
        private static int NETLOGON_CONTROL_TC_VERIFY = 10;
        internal static int NETLOGON_QUERY_LEVEL = 2;
        private static int NETLOGON_VERIFY_STATUS_RETURNED = 0x80;
        private static int PASSWORD_LENGTH = 15;
        private static int PolicyDnsDomainInformation = 12;
        private static char[] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
        private static int STATUS_OBJECT_NAME_NOT_FOUND = 2;
        private static int TRUST_AUTH_TYPE_CLEAR = 2;
        internal static int TRUST_TYPE_DOWNLEVEL = 1;
        internal static int TRUST_TYPE_MIT = 3;
        internal static int TRUST_TYPE_UPLEVEL = 2;
        private static int TRUSTED_SET_AUTH = 0x20;
        private static int TRUSTED_SET_POSIX = 0x10;

        private TrustHelper()
        {
        }

        internal static void CreateTrust(DirectoryContext sourceContext, string sourceName, DirectoryContext targetContext, string targetName, bool isForest, TrustDirection direction, string password)
        {
            LSA_AUTH_INFORMATION structure = null;
            TRUSTED_DOMAIN_AUTH_INFORMATION authInfo = null;
            TRUSTED_DOMAIN_INFORMATION_EX domainEx = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr hglobal = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr domainHandle = IntPtr.Zero;
            PolicySafeHandle handle = null;
            IntPtr ptr5 = IntPtr.Zero;
            bool flag = false;
            string serverName = null;
            ptr = GetTrustedDomainInfo(targetContext, targetName, isForest);
            try
            {
                try
                {
                    POLICY_DNS_DOMAIN_INFO policy_dns_domain_info = new POLICY_DNS_DOMAIN_INFO();
                    Marshal.PtrToStructure(ptr, policy_dns_domain_info);
                    structure = new LSA_AUTH_INFORMATION();
                    zero = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
                    UnsafeNativeMethods.GetSystemTimeAsFileTime(zero);
                    FileTime time = new FileTime();
                    Marshal.PtrToStructure(zero, time);
                    structure.LastUpdateTime = new LARGE_INTEGER();
                    structure.LastUpdateTime.lowPart = time.lower;
                    structure.LastUpdateTime.highPart = time.higher;
                    structure.AuthType = TRUST_AUTH_TYPE_CLEAR;
                    hglobal = Marshal.StringToHGlobalUni(password);
                    structure.AuthInfo = hglobal;
                    structure.AuthInfoLength = password.Length * 2;
                    ptr5 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_AUTH_INFORMATION)));
                    Marshal.StructureToPtr(structure, ptr5, false);
                    authInfo = new TRUSTED_DOMAIN_AUTH_INFORMATION();
                    if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
                    {
                        authInfo.IncomingAuthInfos = 1;
                        authInfo.IncomingAuthenticationInformation = ptr5;
                        authInfo.IncomingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
                    {
                        authInfo.OutgoingAuthInfos = 1;
                        authInfo.OutgoingAuthenticationInformation = ptr5;
                        authInfo.OutgoingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    domainEx = new TRUSTED_DOMAIN_INFORMATION_EX {
                        FlatName = policy_dns_domain_info.Name,
                        Name = policy_dns_domain_info.DnsDomainName,
                        Sid = policy_dns_domain_info.Sid,
                        TrustType = TRUST_TYPE_UPLEVEL,
                        TrustDirection = (int) direction
                    };
                    if (isForest)
                    {
                        domainEx.TrustAttributes = TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE;
                    }
                    else
                    {
                        domainEx.TrustAttributes = TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN;
                    }
                    serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(sourceContext, isForest, false, sourceName);
                    flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(sourceContext);
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    int status = UnsafeNativeMethods.LsaCreateTrustedDomainEx(handle, domainEx, authInfo, TRUSTED_SET_POSIX | TRUSTED_SET_AUTH, out domainHandle);
                    if (status != 0)
                    {
                        status = UnsafeNativeMethods.LsaNtStatusToWinError(status);
                        if (status != ERROR_ALREADY_EXISTS)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(status, serverName);
                        }
                        if (isForest)
                        {
                            throw new ActiveDirectoryObjectExistsException(Res.GetString("AlreadyExistingForestTrust", new object[] { sourceName, targetName }));
                        }
                        throw new ActiveDirectoryObjectExistsException(Res.GetString("AlreadyExistingDomainTrust", new object[] { sourceName, targetName }));
                    }
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                    if (domainHandle != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.LsaClose(domainHandle);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.LsaFreeMemory(ptr);
                    }
                    if (hglobal != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(hglobal);
                    }
                    if (ptr5 != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr5);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        internal static string CreateTrustPassword()
        {
            byte[] data = new byte[PASSWORD_LENGTH];
            char[] chArray = new char[PASSWORD_LENGTH];
            new RNGCryptoServiceProvider().GetBytes(data);
            for (int i = 0; i < PASSWORD_LENGTH; i++)
            {
                int num2 = data[i] % 0x57;
                if (num2 < 10)
                {
                    chArray[i] = (char) (0x30 + num2);
                }
                else if (num2 < 0x24)
                {
                    chArray[i] = (char) ((0x41 + num2) - 10);
                }
                else if (num2 < 0x3e)
                {
                    chArray[i] = (char) ((0x61 + num2) - 0x24);
                }
                else
                {
                    chArray[i] = punctuations[num2 - 0x3e];
                }
            }
            return new string(chArray);
        }

        internal static void DeleteTrust(DirectoryContext sourceContext, string sourceName, string targetName, bool isForest)
        {
            PolicySafeHandle handle = null;
            LSA_UNICODE_STRING result = null;
            int errorCode = 0;
            bool flag = false;
            IntPtr zero = IntPtr.Zero;
            string serverName = null;
            IntPtr buffer = IntPtr.Zero;
            serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(sourceContext, isForest, false, sourceName);
            flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(sourceContext);
            try
            {
                try
                {
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    result = new LSA_UNICODE_STRING();
                    zero = Marshal.StringToHGlobalUni(targetName);
                    UnsafeNativeMethods.RtlInitUnicodeString(result, zero);
                    int status = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref buffer);
                    if (status != 0)
                    {
                        errorCode = UnsafeNativeMethods.LsaNtStatusToWinError(status);
                        if (errorCode != STATUS_OBJECT_NAME_NOT_FOUND)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                        }
                        if (isForest)
                        {
                            throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                        }
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
                    }
                    try
                    {
                        TRUSTED_DOMAIN_INFORMATION_EX structure = new TRUSTED_DOMAIN_INFORMATION_EX();
                        Marshal.PtrToStructure(buffer, structure);
                        ValidateTrustAttribute(structure, isForest, sourceName, targetName);
                        status = UnsafeNativeMethods.LsaDeleteTrustedDomain(handle, structure.Sid);
                        if (status != 0)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(status), serverName);
                        }
                    }
                    finally
                    {
                        if (buffer != IntPtr.Zero)
                        {
                            UnsafeNativeMethods.LsaFreeMemory(buffer);
                        }
                    }
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static IntPtr GetTrustedDomainInfo(DirectoryContext targetContext, string targetName, bool isForest)
        {
            PolicySafeHandle handle = null;
            IntPtr ptr2;
            IntPtr zero = IntPtr.Zero;
            bool flag = false;
            string serverName = null;
            try
            {
                try
                {
                    serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(targetContext, isForest, false, targetName);
                    flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(targetContext);
                    try
                    {
                        handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    }
                    catch (ActiveDirectoryOperationException)
                    {
                        if (flag)
                        {
                            System.DirectoryServices.ActiveDirectory.Utils.Revert();
                            flag = false;
                        }
                        System.DirectoryServices.ActiveDirectory.Utils.ImpersonateAnonymous();
                        flag = true;
                        handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (flag)
                        {
                            System.DirectoryServices.ActiveDirectory.Utils.Revert();
                            flag = false;
                        }
                        System.DirectoryServices.ActiveDirectory.Utils.ImpersonateAnonymous();
                        flag = true;
                        handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    }
                    int status = UnsafeNativeMethods.LsaQueryInformationPolicy(handle, PolicyDnsDomainInformation, out zero);
                    if (status != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(status), serverName);
                    }
                    ptr2 = zero;
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                }
            }
            catch
            {
                throw;
            }
            return ptr2;
        }

        internal static bool GetTrustedDomainInfoStatus(DirectoryContext context, string sourceName, string targetName, TRUST_ATTRIBUTE attribute, bool isForest)
        {
            PolicySafeHandle handle = null;
            bool flag2;
            IntPtr zero = IntPtr.Zero;
            LSA_UNICODE_STRING result = null;
            bool flag = false;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(context, isForest, false, sourceName);
            flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(context);
            try
            {
                try
                {
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(targetName);
                    UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    int status = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref zero);
                    if (status != 0)
                    {
                        int errorCode = UnsafeNativeMethods.LsaNtStatusToWinError(status);
                        if (errorCode != STATUS_OBJECT_NAME_NOT_FOUND)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                        }
                        if (isForest)
                        {
                            throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                        }
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
                    }
                    TRUSTED_DOMAIN_INFORMATION_EX structure = new TRUSTED_DOMAIN_INFORMATION_EX();
                    Marshal.PtrToStructure(zero, structure);
                    ValidateTrustAttribute(structure, isForest, sourceName, targetName);
                    if (attribute == TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION)
                    {
                        if ((structure.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION) == 0)
                        {
                            return false;
                        }
                        return true;
                    }
                    if (attribute == TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL)
                    {
                        return ((structure.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL) == 0);
                    }
                    if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN)
                    {
                        throw new ArgumentException("attribute");
                    }
                    if ((structure.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN) == 0)
                    {
                        return false;
                    }
                    return true;
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                    if (zero != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.LsaFreeMemory(zero);
                    }
                }
            }
            catch
            {
                throw;
            }
            return flag2;
        }

        internal static void SetTrustedDomainInfoStatus(DirectoryContext context, string sourceName, string targetName, TRUST_ATTRIBUTE attribute, bool status, bool isForest)
        {
            PolicySafeHandle handle = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            LSA_UNICODE_STRING result = null;
            bool flag = false;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(context, isForest, false, sourceName);
            flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(context);
            try
            {
                try
                {
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(targetName);
                    UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    int num = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref zero);
                    if (num != 0)
                    {
                        int errorCode = UnsafeNativeMethods.LsaNtStatusToWinError(num);
                        if (errorCode != STATUS_OBJECT_NAME_NOT_FOUND)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                        }
                        if (isForest)
                        {
                            throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                        }
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
                    }
                    TRUSTED_DOMAIN_INFORMATION_EX structure = new TRUSTED_DOMAIN_INFORMATION_EX();
                    Marshal.PtrToStructure(zero, structure);
                    ValidateTrustAttribute(structure, isForest, sourceName, targetName);
                    if (attribute == TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION)
                    {
                        if (status)
                        {
                            structure.TrustAttributes |= TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION;
                        }
                        else
                        {
                            structure.TrustAttributes &= ~TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION;
                        }
                    }
                    else if (attribute == TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL)
                    {
                        if (status)
                        {
                            structure.TrustAttributes &= ~TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL;
                        }
                        else
                        {
                            structure.TrustAttributes |= TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL;
                        }
                    }
                    else
                    {
                        if (attribute != TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN)
                        {
                            throw new ArgumentException("attribute");
                        }
                        if (status)
                        {
                            structure.TrustAttributes |= TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN;
                        }
                        else
                        {
                            structure.TrustAttributes &= ~TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN;
                        }
                    }
                    ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TRUSTED_DOMAIN_INFORMATION_EX)));
                    Marshal.StructureToPtr(structure, ptr, false);
                    num = UnsafeNativeMethods.LsaSetTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ptr);
                    if (num != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num), serverName);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                    if (zero != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.LsaFreeMemory(zero);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        internal static string UpdateTrust(DirectoryContext context, string sourceName, string targetName, string password, bool isForest)
        {
            PolicySafeHandle handle = null;
            string str2;
            IntPtr zero = IntPtr.Zero;
            LSA_UNICODE_STRING result = null;
            IntPtr ptr = IntPtr.Zero;
            bool flag = false;
            LSA_AUTH_INFORMATION structure = null;
            IntPtr fileTime = IntPtr.Zero;
            IntPtr hglobal = IntPtr.Zero;
            IntPtr ptr5 = IntPtr.Zero;
            TRUSTED_DOMAIN_AUTH_INFORMATION trusted_domain_auth_information = null;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(context, isForest, false, sourceName);
            flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(context);
            try
            {
                try
                {
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(targetName);
                    UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    int status = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, ref zero);
                    if (status != 0)
                    {
                        int errorCode = UnsafeNativeMethods.LsaNtStatusToWinError(status);
                        if (errorCode != STATUS_OBJECT_NAME_NOT_FOUND)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                        }
                        if (isForest)
                        {
                            throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                        }
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
                    }
                    TRUSTED_DOMAIN_FULL_INFORMATION trusted_domain_full_information = new TRUSTED_DOMAIN_FULL_INFORMATION();
                    Marshal.PtrToStructure(zero, trusted_domain_full_information);
                    ValidateTrustAttribute(trusted_domain_full_information.Information, isForest, sourceName, targetName);
                    TrustDirection trustDirection = (TrustDirection) trusted_domain_full_information.Information.TrustDirection;
                    structure = new LSA_AUTH_INFORMATION();
                    fileTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
                    UnsafeNativeMethods.GetSystemTimeAsFileTime(fileTime);
                    FileTime time = new FileTime();
                    Marshal.PtrToStructure(fileTime, time);
                    structure.LastUpdateTime = new LARGE_INTEGER();
                    structure.LastUpdateTime.lowPart = time.lower;
                    structure.LastUpdateTime.highPart = time.higher;
                    structure.AuthType = TRUST_AUTH_TYPE_CLEAR;
                    hglobal = Marshal.StringToHGlobalUni(password);
                    structure.AuthInfo = hglobal;
                    structure.AuthInfoLength = password.Length * 2;
                    ptr5 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_AUTH_INFORMATION)));
                    Marshal.StructureToPtr(structure, ptr5, false);
                    trusted_domain_auth_information = new TRUSTED_DOMAIN_AUTH_INFORMATION();
                    if ((trustDirection & TrustDirection.Inbound) != ((TrustDirection) 0))
                    {
                        trusted_domain_auth_information.IncomingAuthInfos = 1;
                        trusted_domain_auth_information.IncomingAuthenticationInformation = ptr5;
                        trusted_domain_auth_information.IncomingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    if ((trustDirection & TrustDirection.Outbound) != ((TrustDirection) 0))
                    {
                        trusted_domain_auth_information.OutgoingAuthInfos = 1;
                        trusted_domain_auth_information.OutgoingAuthenticationInformation = ptr5;
                        trusted_domain_auth_information.OutgoingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    trusted_domain_full_information.AuthInformation = trusted_domain_auth_information;
                    ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TRUSTED_DOMAIN_FULL_INFORMATION)));
                    Marshal.StructureToPtr(trusted_domain_full_information, ptr, false);
                    status = UnsafeNativeMethods.LsaSetTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, ptr);
                    if (status != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(status), serverName);
                    }
                    str2 = serverName;
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                    if (zero != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.LsaFreeMemory(zero);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    if (fileTime != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(fileTime);
                    }
                    if (hglobal != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(hglobal);
                    }
                    if (ptr5 != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr5);
                    }
                }
            }
            catch
            {
                throw;
            }
            return str2;
        }

        internal static void UpdateTrustDirection(DirectoryContext context, string sourceName, string targetName, string password, bool isForest, TrustDirection newTrustDirection)
        {
            PolicySafeHandle handle = null;
            IntPtr zero = IntPtr.Zero;
            LSA_UNICODE_STRING result = null;
            IntPtr ptr = IntPtr.Zero;
            bool flag = false;
            LSA_AUTH_INFORMATION structure = null;
            IntPtr fileTime = IntPtr.Zero;
            IntPtr hglobal = IntPtr.Zero;
            IntPtr ptr5 = IntPtr.Zero;
            TRUSTED_DOMAIN_AUTH_INFORMATION trusted_domain_auth_information = null;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(context, isForest, false, sourceName);
            flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(context);
            try
            {
                try
                {
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(targetName);
                    UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    int status = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, ref zero);
                    if (status != 0)
                    {
                        int errorCode = UnsafeNativeMethods.LsaNtStatusToWinError(status);
                        if (errorCode != STATUS_OBJECT_NAME_NOT_FOUND)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                        }
                        if (isForest)
                        {
                            throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                        }
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
                    }
                    TRUSTED_DOMAIN_FULL_INFORMATION trusted_domain_full_information = new TRUSTED_DOMAIN_FULL_INFORMATION();
                    Marshal.PtrToStructure(zero, trusted_domain_full_information);
                    ValidateTrustAttribute(trusted_domain_full_information.Information, isForest, sourceName, targetName);
                    structure = new LSA_AUTH_INFORMATION();
                    fileTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
                    UnsafeNativeMethods.GetSystemTimeAsFileTime(fileTime);
                    FileTime time = new FileTime();
                    Marshal.PtrToStructure(fileTime, time);
                    structure.LastUpdateTime = new LARGE_INTEGER();
                    structure.LastUpdateTime.lowPart = time.lower;
                    structure.LastUpdateTime.highPart = time.higher;
                    structure.AuthType = TRUST_AUTH_TYPE_CLEAR;
                    hglobal = Marshal.StringToHGlobalUni(password);
                    structure.AuthInfo = hglobal;
                    structure.AuthInfoLength = password.Length * 2;
                    ptr5 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_AUTH_INFORMATION)));
                    Marshal.StructureToPtr(structure, ptr5, false);
                    trusted_domain_auth_information = new TRUSTED_DOMAIN_AUTH_INFORMATION();
                    if ((newTrustDirection & TrustDirection.Inbound) != ((TrustDirection) 0))
                    {
                        trusted_domain_auth_information.IncomingAuthInfos = 1;
                        trusted_domain_auth_information.IncomingAuthenticationInformation = ptr5;
                        trusted_domain_auth_information.IncomingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    else
                    {
                        trusted_domain_auth_information.IncomingAuthInfos = 0;
                        trusted_domain_auth_information.IncomingAuthenticationInformation = IntPtr.Zero;
                        trusted_domain_auth_information.IncomingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    if ((newTrustDirection & TrustDirection.Outbound) != ((TrustDirection) 0))
                    {
                        trusted_domain_auth_information.OutgoingAuthInfos = 1;
                        trusted_domain_auth_information.OutgoingAuthenticationInformation = ptr5;
                        trusted_domain_auth_information.OutgoingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    else
                    {
                        trusted_domain_auth_information.OutgoingAuthInfos = 0;
                        trusted_domain_auth_information.OutgoingAuthenticationInformation = IntPtr.Zero;
                        trusted_domain_auth_information.OutgoingPreviousAuthenticationInformation = IntPtr.Zero;
                    }
                    trusted_domain_full_information.AuthInformation = trusted_domain_auth_information;
                    trusted_domain_full_information.Information.TrustDirection = (int) newTrustDirection;
                    ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TRUSTED_DOMAIN_FULL_INFORMATION)));
                    Marshal.StructureToPtr(trusted_domain_full_information, ptr, false);
                    status = UnsafeNativeMethods.LsaSetTrustedDomainInfoByName(handle, result, TRUSTED_INFORMATION_CLASS.TrustedDomainFullInformation, ptr);
                    if (status != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(status), serverName);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                    if (zero != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.LsaFreeMemory(zero);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    if (fileTime != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(fileTime);
                    }
                    if (hglobal != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(hglobal);
                    }
                    if (ptr5 != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr5);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private static void ValidateTrust(PolicySafeHandle handle, LSA_UNICODE_STRING trustedDomainName, string sourceName, string targetName, bool isForest, int direction, string serverName)
        {
            IntPtr zero = IntPtr.Zero;
            int status = UnsafeNativeMethods.LsaQueryTrustedDomainInfoByName(handle, trustedDomainName, TRUSTED_INFORMATION_CLASS.TrustedDomainInformationEx, ref zero);
            if (status != 0)
            {
                int errorCode = UnsafeNativeMethods.LsaNtStatusToWinError(status);
                if (errorCode != STATUS_OBJECT_NAME_NOT_FOUND)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                }
                if (isForest)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
            }
            try
            {
                TRUSTED_DOMAIN_INFORMATION_EX structure = new TRUSTED_DOMAIN_INFORMATION_EX();
                Marshal.PtrToStructure(zero, structure);
                ValidateTrustAttribute(structure, isForest, sourceName, targetName);
                if ((direction != 0) && ((direction & structure.TrustDirection) == 0))
                {
                    if (isForest)
                    {
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { sourceName, targetName, (TrustDirection) direction }), typeof(ForestTrustRelationshipInformation), null);
                    }
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { sourceName, targetName, (TrustDirection) direction }), typeof(TrustRelationshipInformation), null);
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    UnsafeNativeMethods.LsaFreeMemory(zero);
                }
            }
        }

        private static void ValidateTrustAttribute(TRUSTED_DOMAIN_INFORMATION_EX domainInfo, bool isForest, string sourceName, string targetName)
        {
            if (isForest)
            {
                if ((domainInfo.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE) == 0)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { sourceName, targetName }), typeof(ForestTrustRelationshipInformation), null);
                }
            }
            else
            {
                if ((domainInfo.TrustAttributes & TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_FOREST_TRANSITIVE) != 0)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongForestTrust", new object[] { sourceName, targetName }), typeof(TrustRelationshipInformation), null);
                }
                if (domainInfo.TrustType == TRUST_TYPE_DOWNLEVEL)
                {
                    throw new InvalidOperationException(Res.GetString("NT4NotSupported"));
                }
                if (domainInfo.TrustType == TRUST_TYPE_MIT)
                {
                    throw new InvalidOperationException(Res.GetString("KerberosNotSupported"));
                }
            }
        }

        internal static void VerifyTrust(DirectoryContext context, string sourceName, string targetName, bool isForest, TrustDirection direction, bool forceSecureChannelReset, string preferredTargetServer)
        {
            PolicySafeHandle handle = null;
            LSA_UNICODE_STRING result = null;
            int errorCode = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr buffer = IntPtr.Zero;
            IntPtr ptr4 = IntPtr.Zero;
            bool flag = true;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            serverName = System.DirectoryServices.ActiveDirectory.Utils.GetPolicyServerName(context, isForest, false, sourceName);
            flag = System.DirectoryServices.ActiveDirectory.Utils.Impersonate(context);
            try
            {
                try
                {
                    handle = new PolicySafeHandle(System.DirectoryServices.ActiveDirectory.Utils.GetPolicyHandle(serverName));
                    result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(targetName);
                    UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    ValidateTrust(handle, result, sourceName, targetName, isForest, (int) direction, serverName);
                    if (preferredTargetServer == null)
                    {
                        zero = Marshal.StringToHGlobalUni(targetName);
                    }
                    else
                    {
                        zero = Marshal.StringToHGlobalUni(targetName + @"\" + preferredTargetServer);
                    }
                    ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
                    Marshal.WriteIntPtr(ptr, zero);
                    if (!forceSecureChannelReset)
                    {
                        errorCode = UnsafeNativeMethods.I_NetLogonControl2(serverName, NETLOGON_CONTROL_TC_VERIFY, NETLOGON_QUERY_LEVEL, ptr, out buffer);
                        if (errorCode != 0)
                        {
                            if (errorCode == ERROR_INVALID_LEVEL)
                            {
                                throw new NotSupportedException(Res.GetString("TrustVerificationNotSupport"));
                            }
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                        }
                        NETLOGON_INFO_2 structure = new NETLOGON_INFO_2();
                        Marshal.PtrToStructure(buffer, structure);
                        if ((structure.netlog2_flags & NETLOGON_VERIFY_STATUS_RETURNED) == 0)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(structure.netlog2_tc_connection_status);
                        }
                        int num2 = structure.netlog2_pdc_connection_status;
                        if (num2 != 0)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(num2);
                        }
                    }
                    else
                    {
                        errorCode = UnsafeNativeMethods.I_NetLogonControl2(serverName, NETLOGON_CONTROL_REDISCOVER, NETLOGON_QUERY_LEVEL, ptr, out ptr4);
                        if (errorCode != 0)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                        }
                    }
                }
                finally
                {
                    if (flag)
                    {
                        System.DirectoryServices.ActiveDirectory.Utils.Revert();
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                    if (buffer != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.NetApiBufferFree(buffer);
                    }
                    if (ptr4 != IntPtr.Zero)
                    {
                        UnsafeNativeMethods.NetApiBufferFree(ptr4);
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

