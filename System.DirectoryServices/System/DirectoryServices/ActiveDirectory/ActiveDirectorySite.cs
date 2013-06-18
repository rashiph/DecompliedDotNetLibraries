namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectorySite : IDisposable
    {
        private ReadOnlySiteCollection adjacentSites;
        private bool adjacentSitesRetrieved;
        private bool belongLinksRetrieved;
        private bool bridgeheadServerRetrieved;
        private ReadOnlyDirectoryServerCollection bridgeheadServers;
        internal DirectoryEntry cachedEntry;
        private bool checkADAM;
        internal DirectoryContext context;
        private bool disposed;
        private DomainCollection domains;
        private bool domainsRetrieved;
        private static int ERROR_NO_SITENAME = 0x77f;
        internal bool existing;
        private bool isADAMServer;
        private ReadOnlySiteLinkCollection links;
        private string name;
        private DirectoryEntry ntdsEntry;
        private byte[] replicationSchedule;
        private DirectoryServerCollection RPCBridgeheadServers;
        private bool RPCBridgeRetrieved;
        private ReadOnlyDirectoryServerCollection servers;
        private bool serversRetrieved;
        private string siteDN;
        private ActiveDirectorySiteOptions siteOptions;
        private DirectoryServerCollection SMTPBridgeheadServers;
        private bool SMTPBridgeRetrieved;
        private bool subnetRetrieved;
        private ActiveDirectorySubnetCollection subnets;
        private DirectoryServer topologyGenerator;
        private bool topologyTouched;

        public ActiveDirectorySite(DirectoryContext context, string siteName)
        {
            this.adjacentSites = new ReadOnlySiteCollection();
            this.domains = new DomainCollection(null);
            this.servers = new ReadOnlyDirectoryServerCollection();
            this.links = new ReadOnlySiteLinkCollection();
            this.bridgeheadServers = new ReadOnlyDirectoryServerCollection();
            ValidateArgument(context, siteName);
            context = new DirectoryContext(context);
            this.context = context;
            this.name = siteName;
            DirectoryEntry directoryEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
                this.siteDN = "CN=Sites," + str;
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, this.siteDN);
                string escapedPath = Utils.GetEscapedPath("cn=" + this.name);
                this.cachedEntry = directoryEntry.Children.Add(escapedPath, "site");
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            this.subnets = new ActiveDirectorySubnetCollection(context, "CN=" + siteName + "," + this.siteDN);
            string transportName = "CN=IP,CN=Inter-Site Transports," + this.siteDN;
            this.RPCBridgeheadServers = new DirectoryServerCollection(context, "CN=" + siteName + "," + this.siteDN, transportName);
            transportName = "CN=SMTP,CN=Inter-Site Transports," + this.siteDN;
            this.SMTPBridgeheadServers = new DirectoryServerCollection(context, "CN=" + siteName + "," + this.siteDN, transportName);
        }

        internal ActiveDirectorySite(DirectoryContext context, string siteName, bool existing)
        {
            this.adjacentSites = new ReadOnlySiteCollection();
            this.domains = new DomainCollection(null);
            this.servers = new ReadOnlyDirectoryServerCollection();
            this.links = new ReadOnlySiteLinkCollection();
            this.bridgeheadServers = new ReadOnlyDirectoryServerCollection();
            this.context = context;
            this.name = siteName;
            this.existing = existing;
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
            this.siteDN = "CN=Sites," + ((string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext));
            this.cachedEntry = DirectoryEntryManager.GetDirectoryEntry(context, "CN=" + siteName + "," + this.siteDN);
            this.subnets = new ActiveDirectorySubnetCollection(context, "CN=" + siteName + "," + this.siteDN);
            string transportName = "CN=IP,CN=Inter-Site Transports," + this.siteDN;
            this.RPCBridgeheadServers = new DirectoryServerCollection(context, (string) PropertyManager.GetPropertyValue(context, this.cachedEntry, PropertyManager.DistinguishedName), transportName);
            transportName = "CN=SMTP,CN=Inter-Site Transports," + this.siteDN;
            this.SMTPBridgeheadServers = new DirectoryServerCollection(context, (string) PropertyManager.GetPropertyValue(context, this.cachedEntry, PropertyManager.DistinguishedName), transportName);
        }

        public void Delete()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.existing)
            {
                throw new InvalidOperationException(Res.GetString("CannotDelete"));
            }
            try
            {
                this.cachedEntry.DeleteTree();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.cachedEntry != null)
                {
                    this.cachedEntry.Dispose();
                }
                if (this.ntdsEntry != null)
                {
                    this.ntdsEntry.Dispose();
                }
            }
            this.disposed = true;
        }

        public static ActiveDirectorySite FindByName(DirectoryContext context, string siteName)
        {
            DirectoryEntry directoryEntry;
            ActiveDirectorySite site2;
            ValidateArgument(context, siteName);
            context = new DirectoryContext(context);
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                string dn = "CN=Sites," + ((string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext));
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dn);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
            try
            {
                ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=site)(objectCategory=site)(name=" + Utils.GetEscapedFilterValue(siteName) + "))", new string[] { "distinguishedName" }, SearchScope.OneLevel, false, false);
                if (searcher.FindOne() == null)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySite), siteName);
                }
                site2 = new ActiveDirectorySite(context, siteName, true);
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode == -2147016656)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySite), siteName);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception2);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            return site2;
        }

        private void GetAdjacentSites()
        {
            string str = (string) DirectoryEntryManager.GetDirectoryEntry(this.context, 0).Properties["configurationNamingContext"][0];
            string dn = "CN=Inter-Site Transports,CN=Sites," + str;
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
            ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=siteLink)(objectCategory=SiteLink)(siteList=" + Utils.GetEscapedFilterValue((string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)) + "))", new string[] { "cn", "distinguishedName" }, SearchScope.Subtree);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            try
            {
                ActiveDirectorySiteLink link = null;
                foreach (SearchResult result in results)
                {
                    ActiveDirectoryTransportType rpc;
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DistinguishedName);
                    string siteLinkName = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                    string strA = Utils.GetDNComponents(searchResultPropertyValue)[1].Value;
                    if (string.Compare(strA, "IP", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        rpc = ActiveDirectoryTransportType.Rpc;
                    }
                    else
                    {
                        if (string.Compare(strA, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new ActiveDirectoryOperationException(Res.GetString("UnknownTransport", new object[] { strA }));
                        }
                        rpc = ActiveDirectoryTransportType.Smtp;
                    }
                    using (link = new ActiveDirectorySiteLink(this.context, siteLinkName, rpc, true, result.GetDirectoryEntry()))
                    {
                        foreach (ActiveDirectorySite site in link.Sites)
                        {
                            if ((Utils.Compare(site.Name, this.Name) != 0) && !this.adjacentSites.Contains(site))
                            {
                                this.adjacentSites.Add(site);
                            }
                        }
                    }
                }
            }
            finally
            {
                results.Dispose();
                directoryEntry.Dispose();
            }
        }

        private ReadOnlyDirectoryServerCollection GetBridgeheadServers()
        {
            NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            pathname.EscapedMode = 4;
            ReadOnlyDirectoryServerCollection servers = new ReadOnlyDirectoryServerCollection();
            if (this.existing)
            {
                Hashtable hashtable = new Hashtable();
                Hashtable hashtable2 = new Hashtable();
                Hashtable hashtable3 = new Hashtable();
                string dn = "CN=Servers," + ((string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName));
                using (DirectoryEntry entry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn))
                {
                    ADSearcher searcher = new ADSearcher(entry, "(|(objectCategory=server)(objectCategory=NTDSConnection))", new string[] { "fromServer", "distinguishedName", "dNSHostName", "objectCategory" }, SearchScope.Subtree, true, true);
                    SearchResultCollection results = null;
                    try
                    {
                        results = searcher.FindAll();
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                    try
                    {
                        foreach (SearchResult result in results)
                        {
                            string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.ObjectCategory);
                            if (Utils.Compare(searchResultPropertyValue, 0, "CN=Server".Length, "CN=Server", 0, "CN=Server".Length) == 0)
                            {
                                hashtable3.Add((string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DistinguishedName), (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsHostName));
                            }
                        }
                        foreach (SearchResult result2 in results)
                        {
                            string str3 = (string) PropertyManager.GetSearchResultPropertyValue(result2, PropertyManager.ObjectCategory);
                            if (Utils.Compare(str3, 0, "CN=Server".Length, "CN=Server", 0, "CN=Server".Length) != 0)
                            {
                                string distinguishedName = (string) PropertyManager.GetSearchResultPropertyValue(result2, PropertyManager.FromServer);
                                string partialDN = Utils.GetPartialDN(distinguishedName, 3);
                                pathname.Set(partialDN, 4);
                                partialDN = pathname.Retrieve(11).Substring(3);
                                string key = Utils.GetPartialDN((string) PropertyManager.GetSearchResultPropertyValue(result2, PropertyManager.DistinguishedName), 2);
                                if (!hashtable.Contains(key))
                                {
                                    string str7 = (string) hashtable3[key];
                                    if (!hashtable2.Contains(key))
                                    {
                                        hashtable2.Add(key, str7);
                                    }
                                    if (Utils.Compare((string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.Cn), partialDN) != 0)
                                    {
                                        hashtable.Add(key, str7);
                                        hashtable2.Remove(key);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        results.Dispose();
                    }
                }
                if (hashtable2.Count != 0)
                {
                    DirectoryEntry searchRoot = DirectoryEntryManager.GetDirectoryEntry(this.context, this.siteDN);
                    StringBuilder builder = new StringBuilder(100);
                    if (hashtable2.Count > 1)
                    {
                        builder.Append("(|");
                    }
                    foreach (DictionaryEntry entry3 in hashtable2)
                    {
                        builder.Append("(fromServer=");
                        builder.Append("CN=NTDS Settings,");
                        builder.Append(Utils.GetEscapedFilterValue((string) entry3.Key));
                        builder.Append(")");
                    }
                    if (hashtable2.Count > 1)
                    {
                        builder.Append(")");
                    }
                    ADSearcher searcher2 = new ADSearcher(searchRoot, "(&(objectClass=nTDSConnection)(objectCategory=NTDSConnection)" + builder.ToString() + ")", new string[] { "fromServer", "distinguishedName" }, SearchScope.Subtree);
                    SearchResultCollection results2 = null;
                    try
                    {
                        results2 = searcher2.FindAll();
                    }
                    catch (COMException exception2)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
                    }
                    try
                    {
                        foreach (SearchResult result3 in results2)
                        {
                            string str9 = ((string) PropertyManager.GetSearchResultPropertyValue(result3, PropertyManager.FromServer)).Substring(0x11);
                            if (hashtable2.Contains(str9))
                            {
                                string bstrADsPath = Utils.GetPartialDN((string) PropertyManager.GetSearchResultPropertyValue(result3, PropertyManager.DistinguishedName), 4);
                                pathname.Set(bstrADsPath, 4);
                                if (Utils.Compare(pathname.Retrieve(11).Substring(3), (string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.Cn)) != 0)
                                {
                                    string str11 = (string) hashtable2[str9];
                                    hashtable2.Remove(str9);
                                    hashtable.Add(str9, str11);
                                }
                            }
                        }
                    }
                    finally
                    {
                        results2.Dispose();
                        searchRoot.Dispose();
                    }
                }
                DirectoryEntry directoryEntry = null;
                foreach (DictionaryEntry entry5 in hashtable)
                {
                    DirectoryServer server = null;
                    string domainControllerName = (string) entry5.Value;
                    if (this.IsADAM)
                    {
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, "CN=NTDS Settings," + entry5.Key);
                        int num = (int) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.MsDSPortLDAP);
                        string adamInstanceName = domainControllerName;
                        if (num != 0x185)
                        {
                            adamInstanceName = domainControllerName + ":" + num;
                        }
                        server = new AdamInstance(Utils.GetNewDirectoryContext(adamInstanceName, DirectoryContextType.DirectoryServer, this.context), adamInstanceName);
                    }
                    else
                    {
                        server = new DomainController(Utils.GetNewDirectoryContext(domainControllerName, DirectoryContextType.DirectoryServer, this.context), domainControllerName);
                    }
                    servers.Add(server);
                }
            }
            return servers;
        }

        public static ActiveDirectorySite GetComputerSite()
        {
            new DirectoryContext(DirectoryContextType.Forest);
            IntPtr zero = IntPtr.Zero;
            int errorCode = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsGetSiteName(null, ref zero);
            if (errorCode == 0)
            {
                ActiveDirectorySite site2;
                try
                {
                    string siteName = Marshal.PtrToStringUni(zero);
                    site2 = FindByName(Utils.GetNewDirectoryContext(Locator.GetDomainControllerInfo(null, null, null, 0x10L).DnsForestName, DirectoryContextType.Forest, null), siteName);
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                }
                return site2;
            }
            if (errorCode == ERROR_NO_SITENAME)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("NoCurrentSite"), typeof(ActiveDirectorySite), null);
            }
            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.existing)
            {
                throw new InvalidOperationException(Res.GetString("CannotGetObject"));
            }
            return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedEntry.Path);
        }

        private void GetDomains()
        {
            if (!this.IsADAM)
            {
                string currentServerName = this.cachedEntry.Options.GetCurrentServerName();
                IntPtr handle = DomainController.GetDomainController(Utils.GetNewDirectoryContext(currentServerName, DirectoryContextType.DirectoryServer, this.context)).Handle;
                IntPtr zero = IntPtr.Zero;
                IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsListDomainsInSiteW");
                if (procAddress == IntPtr.Zero)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsListDomainsInSiteW delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsListDomainsInSiteW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsListDomainsInSiteW));
                int errorCode = delegateForFunctionPointer(handle, (string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName), ref zero);
                if (errorCode != 0)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, currentServerName);
                }
                try
                {
                    DS_NAME_RESULT structure = new DS_NAME_RESULT();
                    Marshal.PtrToStructure(zero, structure);
                    int cItems = structure.cItems;
                    IntPtr rItems = structure.rItems;
                    if (cItems > 0)
                    {
                        Marshal.ReadInt32(rItems);
                        IntPtr ptr = IntPtr.Zero;
                        for (int i = 0; i < cItems; i++)
                        {
                            ptr = (IntPtr) (((long) rItems) + (Marshal.SizeOf(typeof(DS_NAME_RESULT_ITEM)) * i));
                            DS_NAME_RESULT_ITEM ds_name_result_item = new DS_NAME_RESULT_ITEM();
                            Marshal.PtrToStructure(ptr, ds_name_result_item);
                            if ((ds_name_result_item.status == DS_NAME_ERROR.DS_NAME_NO_ERROR) || (ds_name_result_item.status == DS_NAME_ERROR.DS_NAME_ERROR_DOMAIN_ONLY))
                            {
                                string distinguishedName = Marshal.PtrToStringUni(ds_name_result_item.pName);
                                if ((distinguishedName != null) && (distinguishedName.Length > 0))
                                {
                                    string dnsNameFromDN = Utils.GetDnsNameFromDN(distinguishedName);
                                    Domain domain = new Domain(Utils.GetNewDirectoryContext(dnsNameFromDN, DirectoryContextType.Domain, this.context), dnsNameFromDN);
                                    this.domains.Add(domain);
                                }
                            }
                        }
                    }
                }
                finally
                {
                    procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
                    if (procAddress == IntPtr.Zero)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                    }
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW tw = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW));
                    tw(zero);
                }
            }
        }

        private void GetLinks()
        {
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
            string str = (string) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ConfigurationNamingContext);
            string dn = "CN=Inter-Site Transports,CN=Sites," + str;
            directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
            ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=siteLink)(objectCategory=SiteLink)(siteList=" + Utils.GetEscapedFilterValue((string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)) + "))", new string[] { "cn", "distinguishedName" }, SearchScope.Subtree);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            try
            {
                foreach (SearchResult result in results)
                {
                    DirectoryEntry entry = result.GetDirectoryEntry();
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                    string strA = Utils.GetDNComponents((string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DistinguishedName))[1].Value;
                    ActiveDirectorySiteLink link = null;
                    if (string.Compare(strA, "IP", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        link = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue, ActiveDirectoryTransportType.Rpc, true, entry);
                    }
                    else
                    {
                        if (string.Compare(strA, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
                        {
                            throw new ActiveDirectoryOperationException(Res.GetString("UnknownTransport", new object[] { strA }));
                        }
                        link = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue, ActiveDirectoryTransportType.Smtp, true, entry);
                    }
                    this.links.Add(link);
                }
            }
            finally
            {
                results.Dispose();
                directoryEntry.Dispose();
            }
        }

        private void GetPreferredBridgeheadServers(ActiveDirectoryTransportType transport)
        {
            string dn = "CN=Servers," + PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName);
            string filterValue = null;
            if (transport == ActiveDirectoryTransportType.Smtp)
            {
                filterValue = "CN=SMTP,CN=Inter-Site Transports," + this.siteDN;
            }
            else
            {
                filterValue = "CN=IP,CN=Inter-Site Transports," + this.siteDN;
            }
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
            ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=server)(objectCategory=Server)(bridgeheadTransportList=" + Utils.GetEscapedFilterValue(filterValue) + "))", new string[] { "dNSHostName", "distinguishedName" }, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            try
            {
                DirectoryEntry entry2 = null;
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsHostName);
                    DirectoryEntry entry3 = result.GetDirectoryEntry();
                    DirectoryServer server = null;
                    try
                    {
                        entry2 = entry3.Children.Find("CN=NTDS Settings", "nTDSDSA");
                    }
                    catch (COMException exception2)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
                    }
                    if (this.IsADAM)
                    {
                        int num = (int) PropertyManager.GetPropertyValue(this.context, entry2, PropertyManager.MsDSPortLDAP);
                        string adamInstanceName = searchResultPropertyValue;
                        if (num != 0x185)
                        {
                            adamInstanceName = searchResultPropertyValue + ":" + num;
                        }
                        server = new AdamInstance(Utils.GetNewDirectoryContext(adamInstanceName, DirectoryContextType.DirectoryServer, this.context), adamInstanceName);
                    }
                    else
                    {
                        server = new DomainController(Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.DirectoryServer, this.context), searchResultPropertyValue);
                    }
                    if (transport == ActiveDirectoryTransportType.Smtp)
                    {
                        this.SMTPBridgeheadServers.Add(server);
                    }
                    else
                    {
                        this.RPCBridgeheadServers.Add(server);
                    }
                }
            }
            finally
            {
                directoryEntry.Dispose();
                results.Dispose();
            }
        }

        private void GetServers()
        {
            ADSearcher searcher = new ADSearcher(this.cachedEntry, "(&(objectClass=server)(objectCategory=server))", new string[] { "dNSHostName" }, SearchScope.Subtree);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            try
            {
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsHostName);
                    DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                    DirectoryEntry entry2 = null;
                    DirectoryServer server = null;
                    try
                    {
                        entry2 = directoryEntry.Children.Find("CN=NTDS Settings", "nTDSDSA");
                    }
                    catch (COMException exception2)
                    {
                        if (exception2.ErrorCode != -2147016656)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
                        }
                        continue;
                    }
                    if (this.IsADAM)
                    {
                        int num = (int) PropertyManager.GetPropertyValue(this.context, entry2, PropertyManager.MsDSPortLDAP);
                        string adamInstanceName = searchResultPropertyValue;
                        if (num != 0x185)
                        {
                            adamInstanceName = searchResultPropertyValue + ":" + num;
                        }
                        server = new AdamInstance(Utils.GetNewDirectoryContext(adamInstanceName, DirectoryContextType.DirectoryServer, this.context), adamInstanceName);
                    }
                    else
                    {
                        server = new DomainController(Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.DirectoryServer, this.context), searchResultPropertyValue);
                    }
                    this.servers.Add(server);
                }
            }
            finally
            {
                results.Dispose();
            }
        }

        private void GetSubnets()
        {
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
            string str = (string) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ConfigurationNamingContext);
            string dn = "CN=Subnets,CN=Sites," + str;
            directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
            ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=subnet)(objectCategory=subnet)(siteObject=" + Utils.GetEscapedFilterValue((string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)) + "))", new string[] { "cn", "location" }, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            try
            {
                string subnetName = null;
                foreach (SearchResult result in results)
                {
                    subnetName = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                    ActiveDirectorySubnet subnet = new ActiveDirectorySubnet(this.context, subnetName, null, true) {
                        cachedEntry = result.GetDirectoryEntry(),
                        Site = this
                    };
                    this.subnets.Add(subnet);
                }
            }
            finally
            {
                results.Dispose();
                directoryEntry.Dispose();
            }
        }

        public void Save()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            try
            {
                this.cachedEntry.CommitChanges();
                foreach (DictionaryEntry entry in this.subnets.changeList)
                {
                    try
                    {
                        ((DirectoryEntry) entry.Value).CommitChanges();
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode != -2147016694)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(exception);
                        }
                    }
                }
                this.subnets.changeList.Clear();
                this.subnetRetrieved = false;
                foreach (DictionaryEntry entry2 in this.SMTPBridgeheadServers.changeList)
                {
                    try
                    {
                        ((DirectoryEntry) entry2.Value).CommitChanges();
                    }
                    catch (COMException exception2)
                    {
                        if (this.IsADAM && (exception2.ErrorCode == -2147016657))
                        {
                            throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
                        }
                        if (exception2.ErrorCode != -2147016694)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(exception2);
                        }
                    }
                }
                this.SMTPBridgeheadServers.changeList.Clear();
                this.SMTPBridgeRetrieved = false;
                foreach (DictionaryEntry entry3 in this.RPCBridgeheadServers.changeList)
                {
                    try
                    {
                        ((DirectoryEntry) entry3.Value).CommitChanges();
                    }
                    catch (COMException exception3)
                    {
                        if (exception3.ErrorCode != -2147016694)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(exception3);
                        }
                    }
                }
                this.RPCBridgeheadServers.changeList.Clear();
                this.RPCBridgeRetrieved = false;
                if (this.existing)
                {
                    if (this.topologyTouched)
                    {
                        try
                        {
                            DirectoryServer interSiteTopologyGenerator = this.InterSiteTopologyGenerator;
                            string str = (interSiteTopologyGenerator is DomainController) ? ((DomainController) interSiteTopologyGenerator).NtdsaObjectName : ((AdamInstance) interSiteTopologyGenerator).NtdsaObjectName;
                            this.NTDSSiteEntry.Properties["interSiteTopologyGenerator"].Value = str;
                        }
                        catch (COMException exception4)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception4);
                        }
                    }
                    this.NTDSSiteEntry.CommitChanges();
                    this.topologyTouched = false;
                }
                else
                {
                    try
                    {
                        DirectoryEntry entry4 = this.cachedEntry.Children.Add("CN=NTDS Site Settings", "nTDSSiteSettings");
                        DirectoryServer server2 = this.InterSiteTopologyGenerator;
                        if (server2 != null)
                        {
                            string str2 = (server2 is DomainController) ? ((DomainController) server2).NtdsaObjectName : ((AdamInstance) server2).NtdsaObjectName;
                            entry4.Properties["interSiteTopologyGenerator"].Value = str2;
                        }
                        entry4.Properties["options"].Value = this.siteOptions;
                        if (this.replicationSchedule != null)
                        {
                            entry4.Properties["schedule"].Value = this.replicationSchedule;
                        }
                        entry4.CommitChanges();
                        this.ntdsEntry = entry4;
                        this.cachedEntry.Children.Add("CN=Servers", "serversContainer").CommitChanges();
                        if (!this.IsADAM)
                        {
                            this.cachedEntry.Children.Add("CN=Licensing Site Settings", "licensingSiteSettings").CommitChanges();
                        }
                    }
                    finally
                    {
                        this.existing = true;
                    }
                }
            }
            catch (COMException exception5)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception5);
            }
        }

        public override string ToString()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return this.name;
        }

        private static void ValidateArgument(DirectoryContext context, string siteName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
            }
            if (((context.Name != null) && !context.isRootDomain()) && (!context.isServer() && !context.isADAMConfigSet()))
            {
                throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (siteName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
        }

        public ReadOnlySiteCollection AdjacentSites
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.adjacentSitesRetrieved)
                {
                    this.adjacentSites.Clear();
                    this.GetAdjacentSites();
                    this.adjacentSitesRetrieved = true;
                }
                return this.adjacentSites;
            }
        }

        public ReadOnlyDirectoryServerCollection BridgeheadServers
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (!this.bridgeheadServerRetrieved)
                {
                    this.bridgeheadServers = this.GetBridgeheadServers();
                    this.bridgeheadServerRetrieved = true;
                }
                return this.bridgeheadServers;
            }
        }

        public DomainCollection Domains
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.domainsRetrieved)
                {
                    this.domains.Clear();
                    this.GetDomains();
                    this.domainsRetrieved = true;
                }
                return this.domains;
            }
        }

        public DirectoryServer InterSiteTopologyGenerator
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if ((this.existing && (this.topologyGenerator == null)) && !this.topologyTouched)
                {
                    bool flag;
                    try
                    {
                        flag = this.NTDSSiteEntry.Properties.Contains("interSiteTopologyGenerator");
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                    if (flag)
                    {
                        string dn = (string) PropertyManager.GetPropertyValue(this.context, this.NTDSSiteEntry, PropertyManager.InterSiteTopologyGenerator);
                        string domainControllerName = null;
                        DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
                        try
                        {
                            domainControllerName = (string) PropertyManager.GetPropertyValue(this.context, directoryEntry.Parent, PropertyManager.DnsHostName);
                        }
                        catch (COMException exception2)
                        {
                            if (exception2.ErrorCode == -2147016656)
                            {
                                return null;
                            }
                        }
                        if (this.IsADAM)
                        {
                            int num = (int) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.MsDSPortLDAP);
                            string adamInstanceName = domainControllerName;
                            if (num != 0x185)
                            {
                                adamInstanceName = domainControllerName + ":" + num;
                            }
                            this.topologyGenerator = new AdamInstance(Utils.GetNewDirectoryContext(adamInstanceName, DirectoryContextType.DirectoryServer, this.context), adamInstanceName);
                        }
                        else
                        {
                            this.topologyGenerator = new DomainController(Utils.GetNewDirectoryContext(domainControllerName, DirectoryContextType.DirectoryServer, this.context), domainControllerName);
                        }
                    }
                }
                return this.topologyGenerator;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (this.existing)
                {
                    DirectoryEntry nTDSSiteEntry = this.NTDSSiteEntry;
                }
                this.topologyTouched = true;
                this.topologyGenerator = value;
            }
        }

        public ActiveDirectorySchedule IntraSiteReplicationSchedule
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                ActiveDirectorySchedule schedule = null;
                if (this.existing)
                {
                    try
                    {
                        if (this.NTDSSiteEntry.Properties.Contains("schedule"))
                        {
                            byte[] unmanagedSchedule = (byte[]) this.NTDSSiteEntry.Properties["schedule"][0];
                            schedule = new ActiveDirectorySchedule();
                            schedule.SetUnmanagedSchedule(unmanagedSchedule);
                        }
                        return schedule;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                }
                if (this.replicationSchedule != null)
                {
                    schedule = new ActiveDirectorySchedule();
                    schedule.SetUnmanagedSchedule(this.replicationSchedule);
                }
                return schedule;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing)
                {
                    try
                    {
                        if (value == null)
                        {
                            if (this.NTDSSiteEntry.Properties.Contains("schedule"))
                            {
                                this.NTDSSiteEntry.Properties["schedule"].Clear();
                            }
                        }
                        else
                        {
                            this.NTDSSiteEntry.Properties["schedule"].Value = value.GetUnmanagedSchedule();
                        }
                        return;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                }
                if (value == null)
                {
                    this.replicationSchedule = null;
                }
                else
                {
                    this.replicationSchedule = value.GetUnmanagedSchedule();
                }
            }
        }

        private bool IsADAM
        {
            get
            {
                if (!this.checkADAM)
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
                    PropertyValueCollection values = null;
                    try
                    {
                        values = directoryEntry.Properties["supportedCapabilities"];
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                    if (values.Contains(SupportedCapability.ADAMOid))
                    {
                        this.isADAMServer = true;
                    }
                }
                return this.isADAMServer;
            }
        }

        public string Location
        {
            get
            {
                string str;
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (this.cachedEntry.Properties.Contains("location"))
                    {
                        return (string) this.cachedEntry.Properties["location"][0];
                    }
                    str = null;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return str;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (value == null)
                    {
                        if (this.cachedEntry.Properties.Contains("location"))
                        {
                            this.cachedEntry.Properties["location"].Clear();
                        }
                    }
                    else
                    {
                        this.cachedEntry.Properties["location"].Value = value;
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public string Name
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.name;
            }
        }

        private DirectoryEntry NTDSSiteEntry
        {
            get
            {
                if (this.ntdsEntry == null)
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, "CN=NTDS Site Settings," + ((string) PropertyManager.GetPropertyValue(this.context, this.cachedEntry, PropertyManager.DistinguishedName)));
                    try
                    {
                        directoryEntry.RefreshCache();
                    }
                    catch (COMException exception)
                    {
                        if (exception.ErrorCode == -2147016656)
                        {
                            throw new ActiveDirectoryOperationException(Res.GetString("NTDSSiteSetting", new object[] { this.name }), exception, 0x2030);
                        }
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                    this.ntdsEntry = directoryEntry;
                }
                return this.ntdsEntry;
            }
        }

        public ActiveDirectorySiteOptions Options
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing)
                {
                    try
                    {
                        if (this.NTDSSiteEntry.Properties.Contains("options"))
                        {
                            return (ActiveDirectorySiteOptions) this.NTDSSiteEntry.Properties["options"][0];
                        }
                        return ActiveDirectorySiteOptions.None;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                }
                return this.siteOptions;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing)
                {
                    try
                    {
                        this.NTDSSiteEntry.Properties["options"].Value = value;
                        return;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                }
                this.siteOptions = value;
            }
        }

        public DirectoryServerCollection PreferredRpcBridgeheadServers
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.RPCBridgeRetrieved)
                {
                    this.RPCBridgeheadServers.initialized = false;
                    this.RPCBridgeheadServers.Clear();
                    this.GetPreferredBridgeheadServers(ActiveDirectoryTransportType.Rpc);
                    this.RPCBridgeRetrieved = true;
                }
                this.RPCBridgeheadServers.initialized = true;
                return this.RPCBridgeheadServers;
            }
        }

        public DirectoryServerCollection PreferredSmtpBridgeheadServers
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.SMTPBridgeRetrieved)
                {
                    this.SMTPBridgeheadServers.initialized = false;
                    this.SMTPBridgeheadServers.Clear();
                    this.GetPreferredBridgeheadServers(ActiveDirectoryTransportType.Smtp);
                    this.SMTPBridgeRetrieved = true;
                }
                this.SMTPBridgeheadServers.initialized = true;
                return this.SMTPBridgeheadServers;
            }
        }

        public ReadOnlyDirectoryServerCollection Servers
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.serversRetrieved)
                {
                    this.servers.Clear();
                    this.GetServers();
                    this.serversRetrieved = true;
                }
                return this.servers;
            }
        }

        public ReadOnlySiteLinkCollection SiteLinks
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.belongLinksRetrieved)
                {
                    this.links.Clear();
                    this.GetLinks();
                    this.belongLinksRetrieved = true;
                }
                return this.links;
            }
        }

        public ActiveDirectorySubnetCollection Subnets
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.subnetRetrieved)
                {
                    this.subnets.initialized = false;
                    this.subnets.Clear();
                    this.GetSubnets();
                    this.subnetRetrieved = true;
                }
                this.subnets.initialized = true;
                return this.subnets;
            }
        }
    }
}

