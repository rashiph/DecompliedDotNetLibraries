namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal sealed class Locator
    {
        private Locator()
        {
        }

        private static Hashtable DnsGetDcWrapper(string domainName, string siteName, long dcFlags)
        {
            Hashtable hashtable = new Hashtable();
            int optionFlags = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr dnsHostName = IntPtr.Zero;
            int num2 = 0;
            IntPtr sockAddressCount = new IntPtr(num2);
            IntPtr sockAdresses = IntPtr.Zero;
            int errorCode = 0;
            errorCode = System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcOpen(domainName, optionFlags, siteName, IntPtr.Zero, null, (int) dcFlags, out zero);
            if (errorCode == 0)
            {
                try
                {
                    errorCode = System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcNext(zero, ref sockAddressCount, out sockAdresses, out dnsHostName);
                    switch (errorCode)
                    {
                        case 0:
                        case 0x44d:
                        case 0x232b:
                        case 0x103:
                            goto Label_0116;

                        default:
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                    }
                Label_008E:
                    switch (errorCode)
                    {
                        case 0x44d:
                        case 0x232b:
                            break;

                        default:
                            try
                            {
                                string key = Marshal.PtrToStringUni(dnsHostName).ToLower(CultureInfo.InvariantCulture);
                                if (!hashtable.Contains(key))
                                {
                                    hashtable.Add(key, null);
                                }
                            }
                            finally
                            {
                                if (dnsHostName != IntPtr.Zero)
                                {
                                    errorCode = System.DirectoryServices.ActiveDirectory.NativeMethods.NetApiBufferFree(dnsHostName);
                                }
                            }
                            break;
                    }
                    errorCode = System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcNext(zero, ref sockAddressCount, out sockAdresses, out dnsHostName);
                    if (((errorCode != 0) && (errorCode != 0x44d)) && ((errorCode != 0x232b) && (errorCode != 0x103)))
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                    }
                Label_0116:
                    if (errorCode != 0x103)
                    {
                        goto Label_008E;
                    }
                    return hashtable;
                }
                finally
                {
                    System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcClose(zero);
                }
            }
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            return hashtable;
        }

        private static Hashtable DnsQueryWrapper(string domainName, string siteName, long dcFlags)
        {
            Hashtable hashtable = new Hashtable();
            string recordName = "_ldap._tcp.";
            int errorCode = 0;
            int options = 0;
            IntPtr zero = IntPtr.Zero;
            if ((siteName != null) && (siteName.Length != 0))
            {
                recordName = recordName + siteName + "._sites.";
            }
            if ((dcFlags & 0x40L) != 0L)
            {
                recordName = recordName + "gc._msdcs.";
            }
            else if ((dcFlags & 0x1000L) != 0L)
            {
                recordName = recordName + "dc._msdcs.";
            }
            recordName = recordName + domainName;
            if ((dcFlags & 1L) != 0L)
            {
                options |= 8;
            }
            errorCode = System.DirectoryServices.ActiveDirectory.NativeMethods.DnsQuery(recordName, 0x21, options, IntPtr.Zero, out zero, IntPtr.Zero);
            if (errorCode == 0)
            {
                try
                {
                    PartialDnsRecord record;
                    for (IntPtr ptr2 = zero; ptr2 != IntPtr.Zero; ptr2 = record.next)
                    {
                        record = new PartialDnsRecord();
                        Marshal.PtrToStructure(ptr2, record);
                        if (record.type == 0x21)
                        {
                            DnsRecord structure = new DnsRecord();
                            Marshal.PtrToStructure(ptr2, structure);
                            string key = structure.data.targetName.ToLower(CultureInfo.InvariantCulture);
                            if (!hashtable.Contains(key))
                            {
                                hashtable.Add(key, null);
                            }
                        }
                    }
                    return hashtable;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        System.DirectoryServices.ActiveDirectory.NativeMethods.DnsRecordListFree(zero, true);
                    }
                }
            }
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            return hashtable;
        }

        internal static int DsGetDcNameWrapper(string computerName, string domainName, string siteName, long flags, out DomainControllerInfo domainControllerInfo)
        {
            IntPtr zero = IntPtr.Zero;
            int num = 0;
            if ((computerName != null) && (computerName.Length == 0))
            {
                computerName = null;
            }
            if ((siteName != null) && (siteName.Length == 0))
            {
                siteName = null;
            }
            num = System.DirectoryServices.ActiveDirectory.NativeMethods.DsGetDcName(computerName, domainName, IntPtr.Zero, siteName, (int) (flags | 0x40000000L), out zero);
            if (num == 0)
            {
                try
                {
                    domainControllerInfo = new DomainControllerInfo();
                    Marshal.PtrToStructure(zero, domainControllerInfo);
                    return num;
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        num = System.DirectoryServices.ActiveDirectory.NativeMethods.NetApiBufferFree(zero);
                    }
                }
            }
            domainControllerInfo = new DomainControllerInfo();
            return num;
        }

        internal static ArrayList EnumerateDomainControllers(DirectoryContext context, string domainName, string siteName, long dcFlags)
        {
            Hashtable hashtable = null;
            ArrayList list = new ArrayList();
            if (siteName == null)
            {
                DomainControllerInfo info;
                int errorCode = DsGetDcNameWrapper(null, domainName, null, dcFlags & 0x9040L, out info);
                switch (errorCode)
                {
                    case 0:
                        siteName = info.ClientSiteName;
                        goto Label_003C;

                    case 0x54b:
                        return list;
                }
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
        Label_003C:
            if (DirectoryContext.DnsgetdcSupported)
            {
                hashtable = DnsGetDcWrapper(domainName, siteName, dcFlags);
            }
            else
            {
                hashtable = DnsQueryWrapper(domainName, null, dcFlags);
                if (siteName != null)
                {
                    foreach (string str in DnsQueryWrapper(domainName, siteName, dcFlags).Keys)
                    {
                        if (!hashtable.Contains(str))
                        {
                            hashtable.Add(str, null);
                        }
                    }
                }
            }
            foreach (string str2 in hashtable.Keys)
            {
                DirectoryContext context2 = Utils.GetNewDirectoryContext(str2, DirectoryContextType.DirectoryServer, context);
                if ((dcFlags & 0x40L) != 0L)
                {
                    list.Add(new GlobalCatalog(context2, str2));
                }
                else
                {
                    list.Add(new DomainController(context2, str2));
                }
            }
            return list;
        }

        internal static DomainControllerInfo GetDomainControllerInfo(string computerName, string domainName, string siteName, long flags)
        {
            DomainControllerInfo info;
            int errorCode = 0;
            errorCode = DsGetDcNameWrapper(computerName, domainName, siteName, flags, out info);
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, domainName);
            }
            return info;
        }
    }
}

