namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
    internal sealed class Utils
    {
        internal static uint DEFAULT_CMP_FLAGS = ((((NORM_IGNORECASE | NORM_IGNOREKANATYPE) | NORM_IGNORENONSPACE) | NORM_IGNOREWIDTH) | SORT_STRINGSORT);
        internal static AuthenticationTypes DefaultAuthType = (AuthenticationTypes.Sealing | AuthenticationTypes.Signing | AuthenticationTypes.Secure);
        private static uint LANG_ENGLISH = 9;
        private static uint LANGID = ((uint) ((((ushort) SUBLANG_ENGLISH_US) << 10) | ((ushort) LANG_ENGLISH)));
        private static uint LCID = ((uint) ((((ushort) SORT_DEFAULT) << 0x10) | ((ushort) LANGID)));
        private static int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private static int LOGON32_PROVIDER_WINNT50 = 3;
        internal static uint NORM_IGNORECASE = 1;
        internal static uint NORM_IGNOREKANATYPE = 0x10000;
        internal static uint NORM_IGNORENONSPACE = 2;
        internal static uint NORM_IGNOREWIDTH = 0x20000;
        private static string NTAuthorityString = null;
        private static int POLICY_VIEW_LOCAL_INFORMATION = 1;
        private static uint SORT_DEFAULT = 0;
        internal static uint SORT_STRINGSORT = 0x1000;
        private static uint STANDARD_RIGHTS_REQUIRED = 0xf0000;
        private static uint SUBLANG_ENGLISH_US = 1;
        private static uint SYNCHRONIZE = 0x100000;
        private static uint THREAD_ALL_ACCESS = ((STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE) | 0x3ff);

        private Utils()
        {
        }

        internal static bool CheckCapability(DirectoryEntry rootDSE, Capability capability)
        {
            bool flag = false;
            if (rootDSE != null)
            {
                if (capability == Capability.ActiveDirectory)
                {
                    foreach (string str in rootDSE.Properties[PropertyManager.SupportedCapabilities])
                    {
                        if (string.Compare(str, SupportedCapability.ADOid, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                    return flag;
                }
                if (capability == Capability.ActiveDirectoryApplicationMode)
                {
                    foreach (string str2 in rootDSE.Properties[PropertyManager.SupportedCapabilities])
                    {
                        if (string.Compare(str2, SupportedCapability.ADAMOid, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return true;
                        }
                    }
                    return flag;
                }
                if (capability != Capability.ActiveDirectoryOrADAM)
                {
                    return flag;
                }
                foreach (string str3 in rootDSE.Properties[PropertyManager.SupportedCapabilities])
                {
                    if ((string.Compare(str3, SupportedCapability.ADAMOid, StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(str3, SupportedCapability.ADOid, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return true;
                    }
                }
            }
            return flag;
        }

        internal static int Compare(string s1, string s2)
        {
            return Compare(s1, s2, DEFAULT_CMP_FLAGS);
        }

        internal static int Compare(string s1, string s2, uint compareFlags)
        {
            if ((s1 == null) || (s2 == null))
            {
                return string.Compare(s1, s2);
            }
            int num = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr2 = IntPtr.Zero;
            int length = 0;
            int num3 = 0;
            try
            {
                zero = Marshal.StringToHGlobalUni(s1);
                length = s1.Length;
                ptr2 = Marshal.StringToHGlobalUni(s2);
                num3 = s2.Length;
                num = System.DirectoryServices.ActiveDirectory.NativeMethods.CompareString(LCID, compareFlags, zero, length, ptr2, num3);
                if (num == 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(zero);
                }
                if (ptr2 != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr2);
                }
            }
            return (num - 2);
        }

        internal static int Compare(string s1, int offset1, int length1, string s2, int offset2, int length2)
        {
            if (s1 == null)
            {
                throw new ArgumentNullException("s1");
            }
            if (s2 == null)
            {
                throw new ArgumentNullException("s2");
            }
            return Compare(s1.Substring(offset1, length1), s2.Substring(offset2, length2));
        }

        internal static int Compare(string s1, int offset1, int length1, string s2, int offset2, int length2, uint compareFlags)
        {
            if (s1 == null)
            {
                throw new ArgumentNullException("s1");
            }
            if (s2 == null)
            {
                throw new ArgumentNullException("s2");
            }
            return Compare(s1.Substring(offset1, length1), s2.Substring(offset2, length2), compareFlags);
        }

        internal static void FreeAuthIdentity(IntPtr authIdentity, LoadLibrarySafeHandle libHandle)
        {
            if (authIdentity != IntPtr.Zero)
            {
                IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsFreePasswordCredentials");
                if (procAddress == IntPtr.Zero)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.NativeMethods.DsFreePasswordCredentials delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsFreePasswordCredentials) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsFreePasswordCredentials));
                delegateForFunctionPointer(authIdentity);
            }
        }

        internal static void FreeDSHandle(IntPtr dsHandle, LoadLibrarySafeHandle libHandle)
        {
            if (dsHandle != IntPtr.Zero)
            {
                IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsUnBindW");
                if (procAddress == IntPtr.Zero)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.NativeMethods.DsUnBind delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsUnBind) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsUnBind));
                delegateForFunctionPointer(ref dsHandle);
            }
        }

        internal static string GetAdamDnsHostNameFromNTDSA(DirectoryContext context, string dn)
        {
            string str = null;
            int num = -1;
            string filterValue = dn;
            string partialDN = GetPartialDN(dn, 1);
            string str4 = GetPartialDN(dn, 2);
            string str5 = "CN=NTDS-DSA";
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str4);
            string filter = "(|(&(" + PropertyManager.ObjectCategory + "=server)(" + PropertyManager.DistinguishedName + "=" + GetEscapedFilterValue(partialDN) + "))(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.DistinguishedName + "=" + GetEscapedFilterValue(filterValue) + ")))";
            string[] propertiesToLoad = new string[] { PropertyManager.DnsHostName, PropertyManager.MsDSPortLDAP, PropertyManager.ObjectCategory };
            SearchResultCollection results = new ADSearcher(directoryEntry, filter, propertiesToLoad, SearchScope.Subtree, true, true).FindAll();
            try
            {
                if (results.Count != 2)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", new object[] { dn }));
                }
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.ObjectCategory);
                    if ((searchResultPropertyValue.Length >= str5.Length) && (Compare(searchResultPropertyValue, 0, str5.Length, str5, 0, str5.Length) == 0))
                    {
                        num = (int) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.MsDSPortLDAP);
                    }
                    else
                    {
                        str = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsHostName);
                    }
                }
            }
            finally
            {
                results.Dispose();
                directoryEntry.Dispose();
            }
            if ((num == -1) || (str == null))
            {
                throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", new object[] { dn }));
            }
            return (str + ":" + num);
        }

        internal static string GetAdamHostNameAndPortsFromNTDSA(DirectoryContext context, string dn)
        {
            string str = null;
            int num = -1;
            int num2 = -1;
            string filterValue = dn;
            string partialDN = GetPartialDN(dn, 1);
            string str4 = GetPartialDN(dn, 2);
            string str5 = "CN=NTDS-DSA";
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str4);
            string filter = "(|(&(" + PropertyManager.ObjectCategory + "=server)(" + PropertyManager.DistinguishedName + "=" + GetEscapedFilterValue(partialDN) + "))(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.DistinguishedName + "=" + GetEscapedFilterValue(filterValue) + ")))";
            string[] propertiesToLoad = new string[] { PropertyManager.DnsHostName, PropertyManager.MsDSPortLDAP, PropertyManager.MsDSPortSSL, PropertyManager.ObjectCategory };
            SearchResultCollection results = new ADSearcher(directoryEntry, filter, propertiesToLoad, SearchScope.Subtree, true, true).FindAll();
            try
            {
                if (results.Count != 2)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", new object[] { dn }));
                }
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.ObjectCategory);
                    if ((searchResultPropertyValue.Length >= str5.Length) && (Compare(searchResultPropertyValue, 0, str5.Length, str5, 0, str5.Length) == 0))
                    {
                        num = (int) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.MsDSPortLDAP);
                        num2 = (int) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.MsDSPortSSL);
                    }
                    else
                    {
                        str = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsHostName);
                    }
                }
            }
            finally
            {
                results.Dispose();
                directoryEntry.Dispose();
            }
            if (((num == -1) || (num2 == -1)) || (str == null))
            {
                throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", new object[] { dn }));
            }
            return string.Concat(new object[] { str, ":", num, ":", num2 });
        }

        internal static IntPtr GetAuthIdentity(DirectoryContext context, LoadLibrarySafeHandle libHandle)
        {
            IntPtr ptr;
            string str;
            string str2;
            int errorCode = 0;
            GetDomainAndUsername(context, out str, out str2);
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsMakePasswordCredentialsW");
            if (procAddress == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.NativeMethods.DsMakePasswordCredentials delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsMakePasswordCredentials) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsMakePasswordCredentials));
            errorCode = delegateForFunctionPointer(str, str2, context.Password, out ptr);
            if (errorCode != 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            return ptr;
        }

        internal static DirectoryEntry GetCrossRefEntry(DirectoryContext context, DirectoryEntry partitionsEntry, string partitionName)
        {
            StringBuilder builder = new StringBuilder(15);
            builder.Append("(&(");
            builder.Append(PropertyManager.ObjectCategory);
            builder.Append("=crossRef)(");
            builder.Append(PropertyManager.SystemFlags);
            builder.Append(":1.2.840.113556.1.4.804:=");
            builder.Append(1);
            builder.Append(")(!(");
            builder.Append(PropertyManager.SystemFlags);
            builder.Append(":1.2.840.113556.1.4.803:=");
            builder.Append(2);
            builder.Append("))(");
            builder.Append(PropertyManager.NCName);
            builder.Append("=");
            builder.Append(GetEscapedFilterValue(partitionName));
            builder.Append("))");
            string filter = builder.ToString();
            string[] propertiesToLoad = new string[] { PropertyManager.DistinguishedName };
            ADSearcher searcher = new ADSearcher(partitionsEntry, filter, propertiesToLoad, SearchScope.OneLevel, false, false);
            SearchResult res = null;
            try
            {
                res = searcher.FindOne();
                if (res == null)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ActiveDirectoryPartition), partitionName);
                }
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(res, PropertyManager.DistinguishedName);
            return res.GetDirectoryEntry();
        }

        internal static Component[] GetDNComponents(string distinguishedName)
        {
            string[] strArray = Split(distinguishedName, ',');
            Component[] componentArray = new Component[strArray.GetLength(0)];
            for (int i = 0; i < strArray.GetLength(0); i++)
            {
                string[] strArray2 = Split(strArray[i], '=');
                if (strArray2.GetLength(0) != 2)
                {
                    throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
                }
                componentArray[i].Name = strArray2[0].Trim();
                if (componentArray[i].Name.Length == 0)
                {
                    throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
                }
                componentArray[i].Value = strArray2[1].Trim();
                if (componentArray[i].Value.Length == 0)
                {
                    throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
                }
            }
            return componentArray;
        }

        internal static string GetDNFromDnsName(string dnsName)
        {
            int errorCode = 0;
            string name = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsCrackNamesW");
            if (procAddress == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.NativeMethods.DsCrackNames delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsCrackNames) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsCrackNames));
            IntPtr val = Marshal.StringToHGlobalUni(dnsName + "/");
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            Marshal.WriteIntPtr(ptr, val);
            errorCode = delegateForFunctionPointer(IntPtr.Zero, 1, 7, 1, 1, ptr, out zero);
            switch (errorCode)
            {
                case 0:
                    try
                    {
                        DsNameResult structure = new DsNameResult();
                        Marshal.PtrToStructure(zero, structure);
                        if ((structure.itemCount >= 1) && (structure.items != IntPtr.Zero))
                        {
                            DsNameResultItem item = new DsNameResultItem();
                            Marshal.PtrToStructure(structure.items, item);
                            name = item.name;
                        }
                        return name;
                    }
                    finally
                    {
                        if (ptr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr);
                        }
                        if (val != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(val);
                        }
                        if (zero != IntPtr.Zero)
                        {
                            procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
                            if (procAddress == IntPtr.Zero)
                            {
                                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                            }
                            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW tw = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW));
                            tw(zero);
                        }
                    }
                    break;

                case 6:
                    throw new ArgumentException(Res.GetString("InvalidDNFormat"));
            }
            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
        }

        internal static string GetDNFromTransportType(ActiveDirectoryTransportType transport, DirectoryContext context)
        {
            string str = DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.SitesContainer);
            string str2 = "CN=Inter-Site Transports," + str;
            if (transport == ActiveDirectoryTransportType.Rpc)
            {
                return ("CN=IP," + str2);
            }
            return ("CN=SMTP," + str2);
        }

        internal static string GetDnsHostNameFromNTDSA(DirectoryContext context, string dn)
        {
            int index = dn.IndexOf(',');
            if (index == -1)
            {
                throw new ArgumentException(Res.GetString("InvalidDNFormat"), "dn");
            }
            string str2 = dn.Substring(index + 1);
            using (DirectoryEntry entry = DirectoryEntryManager.GetDirectoryEntry(context, str2))
            {
                return (string) PropertyManager.GetPropertyValue(context, entry, PropertyManager.DnsHostName);
            }
        }

        internal static string GetDnsNameFromDN(string distinguishedName)
        {
            int errorCode = 0;
            string name = null;
            IntPtr zero = IntPtr.Zero;
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsCrackNamesW");
            if (procAddress == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.NativeMethods.DsCrackNames delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsCrackNames) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsCrackNames));
            IntPtr val = Marshal.StringToHGlobalUni(distinguishedName);
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            Marshal.WriteIntPtr(ptr, val);
            errorCode = delegateForFunctionPointer(IntPtr.Zero, 1, 1, 7, 1, ptr, out zero);
            switch (errorCode)
            {
                case 0:
                    try
                    {
                        DsNameResult structure = new DsNameResult();
                        Marshal.PtrToStructure(zero, structure);
                        if ((structure.itemCount < 1) || !(structure.items != IntPtr.Zero))
                        {
                            return name;
                        }
                        DsNameResultItem item = new DsNameResultItem();
                        Marshal.PtrToStructure(structure.items, item);
                        if ((item.status == 6) || (item.name == null))
                        {
                            throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
                        }
                        if (item.status != 0)
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                        }
                        if ((item.name.Length - 1) == item.name.IndexOf('/'))
                        {
                            name = item.name.Substring(0, item.name.Length - 1);
                        }
                        name = item.name;
                    }
                    finally
                    {
                        if (ptr != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(ptr);
                        }
                        if (val != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(val);
                        }
                        if (zero != IntPtr.Zero)
                        {
                            procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
                            if (procAddress == IntPtr.Zero)
                            {
                                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                            }
                            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW tw = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW));
                            tw(zero);
                        }
                    }
                    break;

                case 6:
                    throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
            }
            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode);
        }

        internal static void GetDomainAndUsername(DirectoryContext context, out string username, out string domain)
        {
            if ((context.UserName != null) && (context.UserName.Length > 0))
            {
                string userName = context.UserName;
                int length = -1;
                length = userName.IndexOf('\\');
                if (length != -1)
                {
                    domain = userName.Substring(0, length);
                    username = userName.Substring(length + 1, (userName.Length - length) - 1);
                }
                else
                {
                    username = userName;
                    domain = null;
                }
            }
            else
            {
                username = context.UserName;
                domain = null;
            }
        }

        internal static IntPtr GetDSHandle(string domainControllerName, string domainName, IntPtr authIdentity, LoadLibrarySafeHandle libHandle)
        {
            IntPtr ptr;
            int errorCode = 0;
            IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(libHandle, "DsBindWithCredW");
            if (procAddress == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            System.DirectoryServices.ActiveDirectory.NativeMethods.DsBindWithCred delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsBindWithCred) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsBindWithCred));
            errorCode = delegateForFunctionPointer(domainControllerName, domainName, authIdentity, out ptr);
            if (errorCode != 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(errorCode, (domainControllerName != null) ? domainControllerName : domainName);
            }
            return ptr;
        }

        internal static string GetEscapedFilterValue(string filterValue)
        {
            int length = -1;
            char[] anyOf = new char[] { '(', ')', '*', '\\' };
            length = filterValue.IndexOfAny(anyOf);
            if (length == -1)
            {
                return filterValue;
            }
            StringBuilder builder = new StringBuilder(2 * filterValue.Length);
            builder.Append(filterValue.Substring(0, length));
            for (int i = length; i < filterValue.Length; i++)
            {
                switch (filterValue[i])
                {
                    case '(':
                        builder.Append(@"\28");
                        break;

                    case ')':
                        builder.Append(@"\29");
                        break;

                    case '*':
                        builder.Append(@"\2A");
                        break;

                    case '\\':
                        builder.Append(@"\5C");
                        break;

                    default:
                        builder.Append(filterValue[i]);
                        break;
                }
            }
            return builder.ToString();
        }

        internal static string GetEscapedPath(string originalPath)
        {
            NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            return pathname.GetEscapedElement(0, originalPath);
        }

        internal static DirectoryContext GetNewDirectoryContext(string name, DirectoryContextType contextType, DirectoryContext context)
        {
            return new DirectoryContext(contextType, name, context);
        }

        internal static string GetNtAuthorityString()
        {
            if (NTAuthorityString == null)
            {
                SecurityIdentifier identifier = new SecurityIdentifier("S-1-5-18");
                NTAccount account = (NTAccount) identifier.Translate(typeof(NTAccount));
                int index = account.Value.IndexOf('\\');
                NTAuthorityString = account.Value.Substring(0, index);
            }
            return NTAuthorityString;
        }

        internal static string GetPartialDN(string distinguishedName, int startingIndex)
        {
            string str = "";
            Component[] dNComponents = GetDNComponents(distinguishedName);
            bool flag = true;
            for (int i = startingIndex; i < dNComponents.GetLength(0); i++)
            {
                if (flag)
                {
                    str = dNComponents[i].Name + "=" + dNComponents[i].Value;
                    flag = false;
                }
                else
                {
                    string str2 = str;
                    str = str2 + "," + dNComponents[i].Name + "=" + dNComponents[i].Value;
                }
            }
            return str;
        }

        internal static IntPtr GetPolicyHandle(string serverName)
        {
            IntPtr ptr3;
            IntPtr zero = IntPtr.Zero;
            LSA_OBJECT_ATTRIBUTES objectAttributes = new LSA_OBJECT_ATTRIBUTES();
            IntPtr s = IntPtr.Zero;
            int access = POLICY_VIEW_LOCAL_INFORMATION;
            LSA_UNICODE_STRING result = new LSA_UNICODE_STRING();
            s = Marshal.StringToHGlobalUni(serverName);
            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RtlInitUnicodeString(result, s);
            try
            {
                int status = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaOpenPolicy(result, objectAttributes, access, out zero);
                if (status != 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError(status), serverName);
                }
                ptr3 = zero;
            }
            finally
            {
                if (s != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(s);
                }
            }
            return ptr3;
        }

        internal static string GetPolicyServerName(DirectoryContext context, bool isForest, bool needPdc, string source)
        {
            PrivateLocatorFlags directoryServicesRequired = PrivateLocatorFlags.DirectoryServicesRequired;
            if (context.isDomain())
            {
                if (needPdc)
                {
                    directoryServicesRequired |= PrivateLocatorFlags.PdcRequired;
                }
                return Locator.GetDomainControllerInfo(null, source, null, (long) directoryServicesRequired).DomainControllerName.Substring(2);
            }
            if (isForest)
            {
                if (needPdc)
                {
                    directoryServicesRequired |= PrivateLocatorFlags.PdcRequired;
                    return Locator.GetDomainControllerInfo(null, source, null, (long) directoryServicesRequired).DomainControllerName.Substring(2);
                }
                if (context.ContextType == DirectoryContextType.DirectoryServer)
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                    string str2 = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.DefaultNamingContext);
                    string str3 = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.RootDomainNamingContext);
                    if (Compare(str2, str3) == 0)
                    {
                        return context.Name;
                    }
                    return Locator.GetDomainControllerInfo(null, source, null, (long) directoryServicesRequired).DomainControllerName.Substring(2);
                }
                return Locator.GetDomainControllerInfo(null, source, null, (long) directoryServicesRequired).DomainControllerName.Substring(2);
            }
            return context.Name;
        }

        internal static int GetRandomIndex(int count)
        {
            Random random = new Random();
            return (random.Next() % count);
        }

        internal static string GetRdnFromDN(string distinguishedName)
        {
            Component[] dNComponents = GetDNComponents(distinguishedName);
            return (dNComponents[0].Name + "=" + dNComponents[0].Value);
        }

        internal static ArrayList GetReplicaList(DirectoryContext context, string partitionName, string siteName, bool isDefaultNC, bool isADAM, bool isGC)
        {
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            StringBuilder builder = new StringBuilder(10);
            StringBuilder builder2 = new StringBuilder(10);
            StringBuilder builder3 = new StringBuilder(10);
            StringBuilder builder4 = new StringBuilder(10);
            bool flag = false;
            string dn = null;
            try
            {
                dn = DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.ConfigurationNamingContext);
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            if ((partitionName != null) && !isDefaultNC)
            {
                DistinguishedName name = new DistinguishedName(partitionName);
                DistinguishedName name2 = new DistinguishedName(dn);
                DistinguishedName name3 = new DistinguishedName("CN=Schema," + dn);
                if (!name2.Equals(name) && !name3.Equals(name))
                {
                    flag = true;
                }
            }
            if (flag)
            {
                DirectoryEntry directoryEntry = null;
                DirectoryEntry searchRootEntry = null;
                try
                {
                    directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, "CN=Partitions," + dn);
                    string adamDnsHostNameFromNTDSA = null;
                    if (isADAM)
                    {
                        adamDnsHostNameFromNTDSA = GetAdamDnsHostNameFromNTDSA(context, (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.FsmoRoleOwner));
                    }
                    else
                    {
                        adamDnsHostNameFromNTDSA = GetDnsHostNameFromNTDSA(context, (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.FsmoRoleOwner));
                    }
                    DirectoryContext context2 = GetNewDirectoryContext(adamDnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, context);
                    searchRootEntry = DirectoryEntryManager.GetDirectoryEntry(context2, "CN=Partitions," + dn);
                    string filter = "(&(" + PropertyManager.ObjectCategory + "=crossRef)(" + PropertyManager.NCName + "=" + GetEscapedFilterValue(partitionName) + "))";
                    ArrayList propertiesToLoad = new ArrayList();
                    propertiesToLoad.Add(PropertyManager.MsDSNCReplicaLocations);
                    propertiesToLoad.Add(PropertyManager.MsDSNCROReplicaLocations);
                    Hashtable hashtable3 = null;
                    try
                    {
                        hashtable3 = GetValuesWithRangeRetrieval(searchRootEntry, filter, propertiesToLoad, SearchScope.OneLevel);
                    }
                    catch (COMException exception2)
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context2, exception2);
                    }
                    catch (ActiveDirectoryObjectNotFoundException)
                    {
                        return list2;
                    }
                    ArrayList list4 = (ArrayList) hashtable3[PropertyManager.MsDSNCReplicaLocations.ToLower(CultureInfo.InvariantCulture)];
                    ArrayList list5 = (ArrayList) hashtable3[PropertyManager.MsDSNCROReplicaLocations.ToLower(CultureInfo.InvariantCulture)];
                    if (list4.Count == 0)
                    {
                        return list2;
                    }
                    foreach (string str4 in list4)
                    {
                        builder.Append("(");
                        builder.Append(PropertyManager.DistinguishedName);
                        builder.Append("=");
                        builder.Append(GetEscapedFilterValue(str4));
                        builder.Append(")");
                        builder2.Append("(");
                        builder2.Append(PropertyManager.DistinguishedName);
                        builder2.Append("=");
                        builder2.Append(GetEscapedFilterValue(GetPartialDN(str4, 1)));
                        builder2.Append(")");
                    }
                    foreach (string str5 in list5)
                    {
                        builder3.Append("(");
                        builder3.Append(PropertyManager.DistinguishedName);
                        builder3.Append("=");
                        builder3.Append(GetEscapedFilterValue(str5));
                        builder3.Append(")");
                        builder4.Append("(");
                        builder4.Append(PropertyManager.DistinguishedName);
                        builder4.Append("=");
                        builder4.Append(GetEscapedFilterValue(GetPartialDN(str5, 1)));
                        builder4.Append(")");
                    }
                }
                catch (COMException exception3)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception3);
                }
                finally
                {
                    if (directoryEntry != null)
                    {
                        directoryEntry.Dispose();
                    }
                    if (searchRootEntry != null)
                    {
                        searchRootEntry.Dispose();
                    }
                }
            }
            string str6 = null;
            using (DirectoryEntry entry3 = null)
            {
                if (siteName != null)
                {
                    str6 = "CN=Servers,CN=" + siteName + ",CN=Sites," + dn;
                }
                else
                {
                    str6 = "CN=Sites," + dn;
                }
                entry3 = DirectoryEntryManager.GetDirectoryEntry(context, str6);
                string str7 = null;
                if (builder.ToString().Length == 0)
                {
                    if (isDefaultNC)
                    {
                        str7 = "(|(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.HasMasterNCs + "=" + GetEscapedFilterValue(partitionName) + "))(&(" + PropertyManager.ObjectCategory + "=nTDSDSARO)(" + PropertyManager.MsDSHasFullReplicaNCs + "=" + GetEscapedFilterValue(partitionName) + "))(" + PropertyManager.ObjectCategory + "=server))";
                    }
                    else if (isGC)
                    {
                        str7 = "(|(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.Options + ":1.2.840.113556.1.4.804:=1))(&(" + PropertyManager.ObjectCategory + "=nTDSDSARO)(" + PropertyManager.Options + ":1.2.840.113556.1.4.804:=1))(" + PropertyManager.ObjectCategory + "=server))";
                    }
                    else
                    {
                        str7 = "(|(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.ObjectCategory + "=nTDSDSARO)(" + PropertyManager.ObjectCategory + "=server))";
                    }
                }
                else if (isGC)
                {
                    if (builder3.Length > 0)
                    {
                        str7 = "(|(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.Options + ":1.2.840.113556.1.4.804:=1)(" + PropertyManager.MsDSHasMasterNCs + "=" + GetEscapedFilterValue(partitionName) + ")(|" + builder.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=nTDSDSARO)(" + PropertyManager.Options + ":1.2.840.113556.1.4.804:=1)(|" + builder3.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=server)(|" + builder2.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=server)(|" + builder4.ToString() + ")))";
                    }
                    else
                    {
                        str7 = "(|(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.Options + ":1.2.840.113556.1.4.804:=1)(" + PropertyManager.MsDSHasMasterNCs + "=" + GetEscapedFilterValue(partitionName) + ")(|" + builder.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=server)(|" + builder2.ToString() + ")))";
                    }
                }
                else if (builder3.Length > 0)
                {
                    str7 = "(|(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.MsDSHasMasterNCs + "=" + GetEscapedFilterValue(partitionName) + ")(|" + builder.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=nTDSDSARO)(|" + builder3.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=server)(|" + builder2.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=server)(|" + builder4.ToString() + ")))";
                }
                else
                {
                    str7 = "(|(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)(" + PropertyManager.MsDSHasMasterNCs + "=" + GetEscapedFilterValue(partitionName) + ")(|" + builder.ToString() + "))(&(" + PropertyManager.ObjectCategory + "=server)(|" + builder2.ToString() + ")))";
                }
                ADSearcher searcher = new ADSearcher(entry3, str7, new string[0], SearchScope.Subtree);
                SearchResultCollection results = null;
                bool flag2 = false;
                ArrayList list6 = new ArrayList();
                int num = 0;
                string str8 = PropertyManager.MsDSHasInstantiatedNCs + ";range=0-*";
                searcher.PropertiesToLoad.Add(PropertyManager.DistinguishedName);
                searcher.PropertiesToLoad.Add(PropertyManager.DnsHostName);
                searcher.PropertiesToLoad.Add(str8);
                searcher.PropertiesToLoad.Add(PropertyManager.ObjectCategory);
                if (isADAM)
                {
                    searcher.PropertiesToLoad.Add(PropertyManager.MsDSPortLDAP);
                }
                try
                {
                    string str9 = "CN=NTDS-DSA";
                    string str10 = "CN=NTDS-DSA-RO";
                    using (results = searcher.FindAll())
                    {
                        foreach (SearchResult result in results)
                        {
                            string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.ObjectCategory);
                            if ((searchResultPropertyValue.Length >= str9.Length) && (Compare(searchResultPropertyValue, 0, str9.Length, str9, 0, str9.Length) == 0))
                            {
                                string str12 = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DistinguishedName);
                                if (flag)
                                {
                                    if ((searchResultPropertyValue.Length >= str10.Length) && (Compare(searchResultPropertyValue, 0, str10.Length, str10, 0, str10.Length) == 0))
                                    {
                                        list.Add(str12);
                                        if (isADAM)
                                        {
                                            hashtable2.Add(str12, (int) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.MsDSPortLDAP));
                                        }
                                        continue;
                                    }
                                    string str13 = null;
                                    if (!result.Properties.Contains(str8))
                                    {
                                        foreach (string str14 in result.Properties.PropertyNames)
                                        {
                                            if ((str14.Length >= PropertyManager.MsDSHasInstantiatedNCs.Length) && (Compare(str14, 0, PropertyManager.MsDSHasInstantiatedNCs.Length, PropertyManager.MsDSHasInstantiatedNCs, 0, PropertyManager.MsDSHasInstantiatedNCs.Length) == 0))
                                            {
                                                str13 = str14;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        str13 = str8;
                                    }
                                    if (str13 == null)
                                    {
                                        continue;
                                    }
                                    bool flag3 = false;
                                    int num2 = 0;
                                    foreach (string str15 in result.Properties[str13])
                                    {
                                        if (((str15.Length - 13) >= partitionName.Length) && (Compare(str15, 13, partitionName.Length, partitionName, 0, partitionName.Length) == 0))
                                        {
                                            flag3 = true;
                                            if (string.Compare(str15, 10, "0", 0, 1, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                list.Add(str12);
                                                if (isADAM)
                                                {
                                                    hashtable2.Add(str12, (int) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.MsDSPortLDAP));
                                                }
                                                break;
                                            }
                                        }
                                        num2++;
                                    }
                                    if ((!flag3 && (str13.Length >= str8.Length)) && (Compare(str13, 0, str8.Length, str8, 0, str8.Length) != 0))
                                    {
                                        flag2 = true;
                                        list6.Add(str12);
                                        num = num2;
                                    }
                                    continue;
                                }
                                list.Add(str12);
                                if (isADAM)
                                {
                                    hashtable2.Add(str12, (int) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.MsDSPortLDAP));
                                }
                                continue;
                            }
                            if (result.Properties.Contains(PropertyManager.DnsHostName))
                            {
                                hashtable.Add("CN=NTDS Settings," + ((string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DistinguishedName)), (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsHostName));
                            }
                        }
                    }
                    if (flag2)
                    {
                        do
                        {
                            StringBuilder builder5 = new StringBuilder(20);
                            if (list6.Count > 1)
                            {
                                builder5.Append("(|");
                            }
                            foreach (string str16 in list6)
                            {
                                builder5.Append("(");
                                builder5.Append(PropertyManager.NCName);
                                builder5.Append("=");
                                builder5.Append(GetEscapedFilterValue(str16));
                                builder5.Append(")");
                            }
                            if (list6.Count > 1)
                            {
                                builder5.Append(")");
                            }
                            list6.Clear();
                            flag2 = false;
                            searcher.Filter = "(&(" + PropertyManager.ObjectCategory + "=nTDSDSA)" + builder5.ToString() + ")";
                            string str17 = string.Concat(new object[] { PropertyManager.MsDSHasInstantiatedNCs, ";range=", num, "-*" });
                            searcher.PropertiesToLoad.Clear();
                            searcher.PropertiesToLoad.Add(str17);
                            searcher.PropertiesToLoad.Add(PropertyManager.DistinguishedName);
                            using (SearchResultCollection results2 = searcher.FindAll())
                            {
                                foreach (SearchResult result2 in results2)
                                {
                                    string str18 = (string) PropertyManager.GetSearchResultPropertyValue(result2, PropertyManager.DistinguishedName);
                                    string str19 = null;
                                    if (!result2.Properties.Contains(str17))
                                    {
                                        foreach (string str20 in result2.Properties.PropertyNames)
                                        {
                                            if (string.Compare(str20, 0, PropertyManager.MsDSHasInstantiatedNCs, 0, PropertyManager.MsDSHasInstantiatedNCs.Length, StringComparison.OrdinalIgnoreCase) == 0)
                                            {
                                                str19 = str20;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        str19 = str17;
                                    }
                                    if (str19 != null)
                                    {
                                        bool flag4 = false;
                                        int num3 = 0;
                                        foreach (string str21 in result2.Properties[str19])
                                        {
                                            if (((str21.Length - 13) >= partitionName.Length) && (Compare(str21, 13, partitionName.Length, partitionName, 0, partitionName.Length) == 0))
                                            {
                                                flag4 = true;
                                                if (string.Compare(str21, 10, "0", 0, 1, StringComparison.OrdinalIgnoreCase) == 0)
                                                {
                                                    list.Add(str18);
                                                    if (isADAM)
                                                    {
                                                        hashtable2.Add(str18, (int) PropertyManager.GetSearchResultPropertyValue(result2, PropertyManager.MsDSPortLDAP));
                                                    }
                                                    break;
                                                }
                                            }
                                            num3++;
                                        }
                                        if ((!flag4 && (str19.Length >= str17.Length)) && (Compare(str19, 0, str17.Length, str17, 0, str17.Length) != 0))
                                        {
                                            flag2 = true;
                                            list6.Add(str18);
                                            num += num3;
                                        }
                                    }
                                }
                            }
                        }
                        while (flag2);
                    }
                }
                catch (COMException exception4)
                {
                    if ((exception4.ErrorCode != -2147016656) || (siteName == null))
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception4);
                    }
                    return list2;
                }
            }
            foreach (string str22 in list)
            {
                string str23 = (string) hashtable[str22];
                if (str23 == null)
                {
                    if (isADAM)
                    {
                        throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", new object[] { str22 }));
                    }
                    throw new ActiveDirectoryOperationException(Res.GetString("NoHostName", new object[] { str22 }));
                }
                if (isADAM && (hashtable2[str22] == null))
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", new object[] { str22 }));
                }
                if (isADAM)
                {
                    list2.Add(str23 + ":" + ((int) hashtable2[str22]));
                }
                else
                {
                    list2.Add(str23);
                }
            }
            return list2;
        }

        internal static string GetServerNameFromInvocationID(string serverObjectDN, Guid invocationID, DirectoryServer server)
        {
            string propertyValue = null;
            DirectoryEntry entry3;
            if (serverObjectDN == null)
            {
                string dn = (server is DomainController) ? ((DomainController) server).SiteObjectName : ((AdamInstance) server).SiteObjectName;
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(server.Context, dn);
                byte[] data = invocationID.ToByteArray();
                IntPtr zero = IntPtr.Zero;
                string str3 = null;
                int errorCode = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ADsEncodeBinaryData(data, data.Length, ref zero);
                if (errorCode == 0)
                {
                    try
                    {
                        str3 = Marshal.PtrToStringUni(zero);
                        ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=nTDSDSA)(invocationID=" + str3 + "))", new string[] { "distinguishedName" }, SearchScope.Subtree, false, false);
                        SearchResult result = null;
                        try
                        {
                            result = searcher.FindOne();
                            if (result != null)
                            {
                                DirectoryEntry parent = result.GetDirectoryEntry().Parent;
                                propertyValue = (string) PropertyManager.GetPropertyValue(server.Context, parent, PropertyManager.DnsHostName);
                            }
                            return propertyValue;
                        }
                        catch (COMException exception)
                        {
                            throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(server.Context, exception);
                        }
                        goto Label_010C;
                    }
                    finally
                    {
                        if (zero != IntPtr.Zero)
                        {
                            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.FreeADsMem(zero);
                        }
                    }
                }
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(new COMException(System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetErrorMessage(errorCode, true), errorCode));
            }
        Label_010C:
            entry3 = DirectoryEntryManager.GetDirectoryEntry(server.Context, serverObjectDN);
            try
            {
                propertyValue = (string) PropertyManager.GetPropertyValue(entry3.Parent, PropertyManager.DnsHostName);
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode != -2147016656)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(server.Context, exception2);
                }
                return null;
            }
            if (server is AdamInstance)
            {
                int num2 = (int) PropertyManager.GetPropertyValue(server.Context, entry3, PropertyManager.MsDSPortLDAP);
                if (num2 != 0x185)
                {
                    propertyValue = propertyValue + ":" + num2;
                }
            }
            return propertyValue;
        }

        internal static ActiveDirectoryTransportType GetTransportTypeFromDN(string DN)
        {
            string strA = GetDNComponents(GetRdnFromDN(DN))[0].Value;
            if (string.Compare(strA, "IP", StringComparison.OrdinalIgnoreCase) == 0)
            {
                return ActiveDirectoryTransportType.Rpc;
            }
            if (string.Compare(strA, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("UnknownTransport", new object[] { strA }));
            }
            return ActiveDirectoryTransportType.Smtp;
        }

        internal static Hashtable GetValuesWithRangeRetrieval(DirectoryEntry searchRootEntry, string filter, ArrayList propertiesToLoad, SearchScope searchScope)
        {
            return GetValuesWithRangeRetrieval(searchRootEntry, filter, propertiesToLoad, new ArrayList(), searchScope);
        }

        internal static Hashtable GetValuesWithRangeRetrieval(DirectoryEntry searchRootEntry, string filter, ArrayList propertiesWithRangeRetrieval, ArrayList propertiesWithoutRangeRetrieval, SearchScope searchScope)
        {
            ADSearcher searcher = new ADSearcher(searchRootEntry, filter, new string[0], searchScope, false, false);
            SearchResult result = null;
            int num = 0;
            Hashtable hashtable = new Hashtable();
            Hashtable hashtable2 = new Hashtable();
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            foreach (string str in propertiesWithoutRangeRetrieval)
            {
                string str2 = str.ToLower(CultureInfo.InvariantCulture);
                list.Add(str2);
                hashtable.Add(str2, new ArrayList());
                searcher.PropertiesToLoad.Add(str);
            }
            foreach (string str3 in propertiesWithRangeRetrieval)
            {
                string str4 = str3.ToLower(CultureInfo.InvariantCulture);
                list2.Add(str4);
                hashtable.Add(str4, new ArrayList());
            }
            do
            {
                foreach (string str5 in list2)
                {
                    string str6 = string.Concat(new object[] { str5, ";range=", num, "-*" });
                    searcher.PropertiesToLoad.Add(str6);
                    hashtable2.Add(str5.ToLower(CultureInfo.InvariantCulture), str6);
                }
                list2.Clear();
                result = searcher.FindOne();
                if (result == null)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"));
                }
                foreach (string str7 in result.Properties.PropertyNames)
                {
                    int index = str7.IndexOf(';');
                    string key = null;
                    if (index != -1)
                    {
                        key = str7.Substring(0, index);
                    }
                    else
                    {
                        key = str7;
                    }
                    if (hashtable2.Contains(key) || list.Contains(key))
                    {
                        ((ArrayList) hashtable[key]).AddRange(result.Properties[str7]);
                        if (hashtable2.Contains(key))
                        {
                            string str9 = (string) hashtable2[key];
                            if ((str7.Length >= str9.Length) && (Compare(str9, 0, str9.Length, str7, 0, str9.Length) != 0))
                            {
                                list2.Add(key);
                                num += result.Properties[str7].Count;
                            }
                        }
                    }
                }
                searcher.PropertiesToLoad.Clear();
                hashtable2.Clear();
            }
            while (list2.Count > 0);
            return hashtable;
        }

        internal static bool Impersonate(DirectoryContext context)
        {
            string str;
            string str2;
            IntPtr zero = IntPtr.Zero;
            if ((context.UserName == null) && (context.Password == null))
            {
                return false;
            }
            GetDomainAndUsername(context, out str, out str2);
            if (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LogonUserW(str, str2, context.Password, LOGON32_LOGON_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, ref zero) == 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            try
            {
                if (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ImpersonateLoggedOnUser(zero) == 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.CloseHandle(zero);
                }
            }
            return true;
        }

        internal static void ImpersonateAnonymous()
        {
            IntPtr zero = IntPtr.Zero;
            zero = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.OpenThread(THREAD_ALL_ACCESS, false, System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetCurrentThreadId());
            if (zero == IntPtr.Zero)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
            try
            {
                if (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ImpersonateAnonymousToken(zero) == 0)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.CloseHandle(zero);
                }
            }
        }

        internal static bool IsValidDNFormat(string distinguishedName)
        {
            string[] strArray = Split(distinguishedName, ',');
            Component[] componentArray = new Component[strArray.GetLength(0)];
            for (int i = 0; i < strArray.GetLength(0); i++)
            {
                string[] strArray2 = Split(strArray[i], '=');
                if (strArray2.GetLength(0) != 2)
                {
                    return false;
                }
                componentArray[i].Name = strArray2[0].Trim();
                if (componentArray[i].Name.Length == 0)
                {
                    return false;
                }
                componentArray[i].Value = strArray2[1].Trim();
                if (componentArray[i].Value.Length == 0)
                {
                    return false;
                }
            }
            return true;
        }

        internal static void Revert()
        {
            if (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RevertToSelf() == 0)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
            }
        }

        public static string[] Split(string distinguishedName, char delim)
        {
            bool flag = false;
            char ch2 = '"';
            char ch3 = '\\';
            int startIndex = 0;
            ArrayList list = new ArrayList();
            for (int i = 0; i < distinguishedName.Length; i++)
            {
                char ch = distinguishedName[i];
                if (ch == ch2)
                {
                    flag = !flag;
                }
                else if (ch == ch3)
                {
                    if (i < (distinguishedName.Length - 1))
                    {
                        i++;
                    }
                }
                else if (!flag && (ch == delim))
                {
                    list.Add(distinguishedName.Substring(startIndex, i - startIndex));
                    startIndex = i + 1;
                }
                if (i == (distinguishedName.Length - 1))
                {
                    if (flag)
                    {
                        throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
                    }
                    list.Add(distinguishedName.Substring(startIndex, (i - startIndex) + 1));
                }
            }
            string[] strArray = new string[list.Count];
            for (int j = 0; j < list.Count; j++)
            {
                strArray[j] = (string) list[j];
            }
            return strArray;
        }

        internal static string SplitServerNameAndPortNumber(string serverName, out string portNumber)
        {
            portNumber = null;
            int length = serverName.LastIndexOf(':');
            if (length != -1)
            {
                if (serverName.StartsWith("["))
                {
                    if (serverName.EndsWith("]"))
                    {
                        serverName = serverName.Substring(1, serverName.Length - 2);
                        return serverName;
                    }
                    int num2 = serverName.LastIndexOf("]:");
                    if ((num2 != -1) && ((num2 + 1) == length))
                    {
                        portNumber = serverName.Substring(length + 1);
                        serverName = serverName.Substring(1, num2 - 1);
                    }
                    return serverName;
                }
                try
                {
                    if (IPAddress.Parse(serverName).AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        return serverName;
                    }
                }
                catch (FormatException)
                {
                }
                portNumber = serverName.Substring(length + 1);
                serverName = serverName.Substring(0, length);
            }
            return serverName;
        }
    }
}

