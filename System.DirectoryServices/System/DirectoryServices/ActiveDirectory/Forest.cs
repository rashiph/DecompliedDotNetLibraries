namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class Forest : IDisposable
    {
        private IntPtr authIdentity;
        private ApplicationPartitionCollection cachedApplicationPartitions;
        private DomainCollection cachedDomains;
        private GlobalCatalogCollection cachedGlobalCatalogs;
        private DomainController cachedNamingRoleOwner;
        private Domain cachedRootDomain;
        private ActiveDirectorySchema cachedSchema;
        private DomainController cachedSchemaRoleOwner;
        private ReadOnlySiteCollection cachedSites;
        private DirectoryContext context;
        private System.DirectoryServices.ActiveDirectory.ForestMode currentForestMode;
        private DirectoryEntryManager directoryEntryMgr;
        private bool disposed;
        private IntPtr dsHandle;
        private string forestDnsName;

        internal Forest(DirectoryContext context, string name) : this(context, name, new DirectoryEntryManager(context))
        {
        }

        internal Forest(DirectoryContext context, string forestDnsName, DirectoryEntryManager directoryEntryMgr)
        {
            this.dsHandle = IntPtr.Zero;
            this.authIdentity = IntPtr.Zero;
            this.currentForestMode = ~System.DirectoryServices.ActiveDirectory.ForestMode.Windows2000Forest;
            this.context = context;
            this.directoryEntryMgr = directoryEntryMgr;
            this.forestDnsName = forestDnsName;
        }

        private void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public void CreateLocalSideOfTrustRelationship(string targetForestName, TrustDirection direction, string trustPassword)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            if ((direction < TrustDirection.Inbound) || (direction > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(TrustDirection));
            }
            if (trustPassword == null)
            {
                throw new ArgumentNullException("trustPassword");
            }
            if (trustPassword.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "trustPassword");
            }
            Locator.GetDomainControllerInfo(null, targetForestName, null, 80L);
            DirectoryContext targetContext = Utils.GetNewDirectoryContext(targetForestName, DirectoryContextType.Forest, this.context);
            TrustHelper.CreateTrust(this.context, this.Name, targetContext, targetForestName, true, direction, trustPassword);
        }

        public void CreateTrustRelationship(Forest targetForest, TrustDirection direction)
        {
            this.CheckIfDisposed();
            if (targetForest == null)
            {
                throw new ArgumentNullException("targetForest");
            }
            if ((direction < TrustDirection.Inbound) || (direction > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(TrustDirection));
            }
            string password = TrustHelper.CreateTrustPassword();
            TrustHelper.CreateTrust(this.context, this.Name, targetForest.GetDirectoryContext(), targetForest.Name, true, direction, password);
            int num = 0;
            if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                num |= 2;
            }
            if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                num |= 1;
            }
            TrustHelper.CreateTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.context, this.Name, true, (TrustDirection) num, password);
        }

        public void DeleteLocalSideOfTrustRelationship(string targetForestName)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            TrustHelper.DeleteTrust(this.context, this.Name, targetForestName, true);
        }

        public void DeleteTrustRelationship(Forest targetForest)
        {
            this.CheckIfDisposed();
            if (targetForest == null)
            {
                throw new ArgumentNullException("targetForest");
            }
            TrustHelper.DeleteTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true);
            TrustHelper.DeleteTrust(this.context, this.Name, targetForest.Name, true);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (DirectoryEntry entry in this.directoryEntryMgr.GetCachedDirectoryEntries())
                    {
                        entry.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        public GlobalCatalogCollection FindAllDiscoverableGlobalCatalogs()
        {
            long dcFlags = 0x40L;
            this.CheckIfDisposed();
            return new GlobalCatalogCollection(Locator.EnumerateDomainControllers(this.context, this.Name, null, dcFlags));
        }

        public GlobalCatalogCollection FindAllDiscoverableGlobalCatalogs(string siteName)
        {
            long dcFlags = 0x40L;
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (siteName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            return new GlobalCatalogCollection(Locator.EnumerateDomainControllers(this.context, this.Name, siteName, dcFlags));
        }

        public GlobalCatalogCollection FindAllGlobalCatalogs()
        {
            this.CheckIfDisposed();
            return GlobalCatalog.FindAllInternal(this.context, null);
        }

        public GlobalCatalogCollection FindAllGlobalCatalogs(string siteName)
        {
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return GlobalCatalog.FindAllInternal(this.context, siteName);
        }

        public GlobalCatalog FindGlobalCatalog()
        {
            this.CheckIfDisposed();
            return GlobalCatalog.FindOneInternal(this.context, this.Name, null, 0L);
        }

        public GlobalCatalog FindGlobalCatalog(LocatorOptions flag)
        {
            this.CheckIfDisposed();
            return GlobalCatalog.FindOneInternal(this.context, this.Name, null, flag);
        }

        public GlobalCatalog FindGlobalCatalog(string siteName)
        {
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return GlobalCatalog.FindOneInternal(this.context, this.Name, siteName, 0L);
        }

        public GlobalCatalog FindGlobalCatalog(string siteName, LocatorOptions flag)
        {
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return GlobalCatalog.FindOneInternal(this.context, this.Name, siteName, flag);
        }

        public TrustRelationshipInformationCollection GetAllTrustRelationships()
        {
            this.CheckIfDisposed();
            return this.GetTrustsHelper(null);
        }

        private ArrayList GetApplicationPartitions()
        {
            ArrayList list = new ArrayList();
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
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
            builder.Append(")))");
            string filter = builder.ToString();
            string[] propertiesToLoad = new string[] { PropertyManager.DnsRoot, PropertyManager.NCName };
            ADSearcher searcher = new ADSearcher(directoryEntry, filter, propertiesToLoad, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
                string str2 = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
                string str3 = this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.ConfigurationNamingContext);
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.NCName);
                    if (!searchResultPropertyValue.Equals(str2) && !searchResultPropertyValue.Equals(str3))
                    {
                        string name = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsRoot);
                        DirectoryContext context = Utils.GetNewDirectoryContext(name, DirectoryContextType.ApplicationPartition, this.context);
                        list.Add(new ApplicationPartition(context, searchResultPropertyValue, (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsRoot), ApplicationPartitionType.ADApplicationPartition, new DirectoryEntryManager(context)));
                    }
                }
                return list;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
                directoryEntry.Dispose();
            }
            return list;
        }

        public static Forest GetCurrentForest()
        {
            return GetForest(new DirectoryContext(DirectoryContextType.Forest));
        }

        internal DirectoryContext GetDirectoryContext()
        {
            return this.context;
        }

        private ArrayList GetDomains()
        {
            ArrayList list = new ArrayList();
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
            StringBuilder builder = new StringBuilder(15);
            builder.Append("(&(");
            builder.Append(PropertyManager.ObjectCategory);
            builder.Append("=crossRef)(");
            builder.Append(PropertyManager.SystemFlags);
            builder.Append(":1.2.840.113556.1.4.804:=");
            builder.Append(1);
            builder.Append(")(");
            builder.Append(PropertyManager.SystemFlags);
            builder.Append(":1.2.840.113556.1.4.804:=");
            builder.Append(2);
            builder.Append("))");
            string filter = builder.ToString();
            string[] propertiesToLoad = new string[] { PropertyManager.DnsRoot };
            ADSearcher searcher = new ADSearcher(directoryEntry, filter, propertiesToLoad, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsRoot);
                    DirectoryContext context = Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.Domain, this.context);
                    list.Add(new Domain(context, searchResultPropertyValue));
                }
                return list;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
                directoryEntry.Dispose();
            }
            return list;
        }

        private void GetDSHandle(out IntPtr dsHandle, out IntPtr authIdentity)
        {
            authIdentity = Utils.GetAuthIdentity(this.context, DirectoryContext.ADHandle);
            if (this.context.ContextType == DirectoryContextType.DirectoryServer)
            {
                dsHandle = Utils.GetDSHandle(this.context.GetServerName(), null, authIdentity, DirectoryContext.ADHandle);
            }
            else
            {
                dsHandle = Utils.GetDSHandle(null, this.context.GetServerName(), authIdentity, DirectoryContext.ADHandle);
            }
        }

        public static Forest GetForest(DirectoryContext context)
        {
            DirectoryEntryManager directoryEntryMgr = null;
            DirectoryEntry rootDSE = null;
            string distinguishedName = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.ContextType != DirectoryContextType.Forest) && (context.ContextType != DirectoryContextType.DirectoryServer))
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeServerORForest"), "context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(Forest), null);
            }
            if (((context.Name != null) && !context.isRootDomain()) && !context.isServer())
            {
                if (context.ContextType == DirectoryContextType.Forest)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(Forest), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(Forest), null);
            }
            context = new DirectoryContext(context);
            directoryEntryMgr = new DirectoryEntryManager(context);
            try
            {
                rootDSE = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (context.isServer() && !Utils.CheckCapability(rootDSE, Capability.ActiveDirectory))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(Forest), null);
                }
                distinguishedName = (string) PropertyManager.GetPropertyValue(context, rootDSE, PropertyManager.RootDomainNamingContext);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016646)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if (context.ContextType == DirectoryContextType.Forest)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(Forest), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(Forest), null);
            }
            return new Forest(context, Utils.GetDnsNameFromDN(distinguishedName), directoryEntryMgr);
        }

        private System.DirectoryServices.ActiveDirectory.ForestMode GetForestMode()
        {
            System.DirectoryServices.ActiveDirectory.ForestMode mode;
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
            try
            {
                if (!directoryEntry.Properties.Contains(PropertyManager.ForestFunctionality))
                {
                    return System.DirectoryServices.ActiveDirectory.ForestMode.Windows2000Forest;
                }
                return (System.DirectoryServices.ActiveDirectory.ForestMode) int.Parse((string) directoryEntry.Properties[PropertyManager.ForestFunctionality].Value, NumberFormatInfo.InvariantInfo);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            return mode;
        }

        private DomainController GetRoleOwner(ActiveDirectoryRole role)
        {
            DirectoryEntry directoryEntry = null;
            string domainControllerName = null;
            try
            {
                switch (role)
                {
                    case ActiveDirectoryRole.SchemaRole:
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext));
                        break;

                    case ActiveDirectoryRole.NamingRole:
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
                        break;
                }
                domainControllerName = Utils.GetDnsHostNameFromNTDSA(this.context, (string) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.FsmoRoleOwner));
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            return new DomainController(Utils.GetNewDirectoryContext(domainControllerName, DirectoryContextType.DirectoryServer, this.context), domainControllerName);
        }

        public bool GetSelectiveAuthenticationStatus(string targetForestName)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            return TrustHelper.GetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, true);
        }

        public bool GetSidFilteringStatus(string targetForestName)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            return TrustHelper.GetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL, true);
        }

        private ArrayList GetSites()
        {
            ArrayList list = new ArrayList();
            int errorCode = 0;
            IntPtr zero = IntPtr.Zero;
            IntPtr authIdentity = IntPtr.Zero;
            IntPtr ptr3 = IntPtr.Zero;
            try
            {
                this.GetDSHandle(out zero, out authIdentity);
                IntPtr procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsListSitesW");
                if (procAddress == IntPtr.Zero)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                }
                System.DirectoryServices.ActiveDirectory.NativeMethods.DsListSites delegateForFunctionPointer = (System.DirectoryServices.ActiveDirectory.NativeMethods.DsListSites) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.NativeMethods.DsListSites));
                errorCode = delegateForFunctionPointer(zero, out ptr3);
                if (errorCode == 0)
                {
                    try
                    {
                        DsNameResult structure = new DsNameResult();
                        Marshal.PtrToStructure(ptr3, structure);
                        IntPtr items = structure.items;
                        for (int i = 0; i < structure.itemCount; i++)
                        {
                            DsNameResultItem item = new DsNameResultItem();
                            Marshal.PtrToStructure(items, item);
                            if (item.status == 0)
                            {
                                string siteName = Utils.GetDNComponents(item.name)[0].Value;
                                list.Add(new ActiveDirectorySite(this.context, siteName, true));
                            }
                            items = (IntPtr) (((long) items) + Marshal.SizeOf(item));
                        }
                        return list;
                    }
                    finally
                    {
                        if (ptr3 != IntPtr.Zero)
                        {
                            procAddress = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
                            if (procAddress == IntPtr.Zero)
                            {
                                throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                            }
                            System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW tw = (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW) Marshal.GetDelegateForFunctionPointer(procAddress, typeof(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsFreeNameResultW));
                            tw(ptr3);
                        }
                    }
                }
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, this.context.GetServerName());
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Utils.FreeDSHandle(zero, DirectoryContext.ADHandle);
                }
                if (authIdentity != IntPtr.Zero)
                {
                    Utils.FreeAuthIdentity(authIdentity, DirectoryContext.ADHandle);
                }
            }
            return list;
        }

        public ForestTrustRelationshipInformation GetTrustRelationship(string targetForestName)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            TrustRelationshipInformationCollection trustsHelper = this.GetTrustsHelper(targetForestName);
            if (trustsHelper.Count == 0)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestTrustDoesNotExist", new object[] { this.Name, targetForestName }), typeof(TrustRelationshipInformation), null);
            }
            return (ForestTrustRelationshipInformation) trustsHelper[0];
        }

        private TrustRelationshipInformationCollection GetTrustsHelper(string targetForestName)
        {
            string serverName = null;
            TrustRelationshipInformationCollection informations2;
            IntPtr zero = IntPtr.Zero;
            int count = 0;
            TrustRelationshipInformationCollection informations = new TrustRelationshipInformationCollection();
            bool flag = false;
            int errorCode = 0;
            serverName = Utils.GetPolicyServerName(this.context, true, false, this.Name);
            flag = Utils.Impersonate(this.context);
            try
            {
                try
                {
                    errorCode = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsEnumerateDomainTrustsW(serverName, 0x2a, out zero, out count);
                }
                finally
                {
                    if (flag)
                    {
                        Utils.Revert();
                    }
                }
            }
            catch
            {
                throw;
            }
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
            }
            try
            {
                if ((zero != IntPtr.Zero) && (count != 0))
                {
                    IntPtr ptr = IntPtr.Zero;
                    for (int i = 0; i < count; i++)
                    {
                        ptr = (IntPtr) (((long) zero) + (i * Marshal.SizeOf(typeof(DS_DOMAIN_TRUSTS))));
                        DS_DOMAIN_TRUSTS structure = new DS_DOMAIN_TRUSTS();
                        Marshal.PtrToStructure(ptr, structure);
                        if (targetForestName != null)
                        {
                            bool flag2 = false;
                            string str2 = null;
                            string str3 = null;
                            if (structure.DnsDomainName != IntPtr.Zero)
                            {
                                str2 = Marshal.PtrToStringUni(structure.DnsDomainName);
                            }
                            if (structure.NetbiosDomainName != IntPtr.Zero)
                            {
                                str3 = Marshal.PtrToStringUni(structure.NetbiosDomainName);
                            }
                            if ((str2 != null) && (Utils.Compare(targetForestName, str2) == 0))
                            {
                                flag2 = true;
                            }
                            else if ((str3 != null) && (Utils.Compare(targetForestName, str3) == 0))
                            {
                                flag2 = true;
                            }
                            if (!flag2)
                            {
                                continue;
                            }
                        }
                        if (((structure.TrustType == TrustHelper.TRUST_TYPE_UPLEVEL) && ((structure.TrustAttributes & 8) != 0)) && ((structure.Flags & 8) == 0))
                        {
                            TrustRelationshipInformation info = new ForestTrustRelationshipInformation(this.context, this.Name, structure, TrustType.Forest);
                            informations.Add(info);
                        }
                    }
                }
                informations2 = informations;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.NetApiBufferFree(zero);
                }
            }
            return informations2;
        }

        public void RaiseForestFunctionality(System.DirectoryServices.ActiveDirectory.ForestMode forestMode)
        {
            this.CheckIfDisposed();
            if ((forestMode < System.DirectoryServices.ActiveDirectory.ForestMode.Windows2000Forest) || (forestMode > System.DirectoryServices.ActiveDirectory.ForestMode.Windows2008R2Forest))
            {
                throw new InvalidEnumArgumentException("forestMode", (int) forestMode, typeof(System.DirectoryServices.ActiveDirectory.ForestMode));
            }
            if (forestMode <= this.GetForestMode())
            {
                throw new ArgumentException(Res.GetString("InvalidMode"), "forestMode");
            }
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
            try
            {
                directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = (int) forestMode;
                directoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016694)
                {
                    throw new ArgumentException(Res.GetString("NoW2K3DCsInForest"), "forestMode");
                }
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            this.currentForestMode = ~System.DirectoryServices.ActiveDirectory.ForestMode.Windows2000Forest;
        }

        private void RepairTrustHelper(Forest targetForest, TrustDirection direction)
        {
            string password = TrustHelper.CreateTrustPassword();
            string preferredTargetServer = TrustHelper.UpdateTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, password, true);
            string str3 = TrustHelper.UpdateTrust(this.context, this.Name, targetForest.Name, password, true);
            if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(this.context, this.Name, targetForest.Name, true, TrustDirection.Outbound, true, preferredTargetServer);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { this.Name, targetForest.Name, direction }), typeof(ForestTrustRelationshipInformation), null);
                }
            }
            if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true, TrustDirection.Outbound, true, str3);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { this.Name, targetForest.Name, direction }), typeof(ForestTrustRelationshipInformation), null);
                }
            }
        }

        public void RepairTrustRelationship(Forest targetForest)
        {
            TrustDirection bidirectional = TrustDirection.Bidirectional;
            this.CheckIfDisposed();
            if (targetForest == null)
            {
                throw new ArgumentNullException("targetForest");
            }
            try
            {
                bidirectional = this.GetTrustRelationship(targetForest.Name).TrustDirection;
                if ((bidirectional & TrustDirection.Outbound) != ((TrustDirection) 0))
                {
                    TrustHelper.VerifyTrust(this.context, this.Name, targetForest.Name, true, TrustDirection.Outbound, true, null);
                }
                if ((bidirectional & TrustDirection.Inbound) != ((TrustDirection) 0))
                {
                    TrustHelper.VerifyTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true, TrustDirection.Outbound, true, null);
                }
            }
            catch (ActiveDirectoryOperationException)
            {
                this.RepairTrustHelper(targetForest, bidirectional);
            }
            catch (UnauthorizedAccessException)
            {
                this.RepairTrustHelper(targetForest, bidirectional);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { this.Name, targetForest.Name, bidirectional }), typeof(ForestTrustRelationshipInformation), null);
            }
        }

        public void SetSelectiveAuthenticationStatus(string targetForestName, bool enable)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            TrustHelper.SetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, enable, true);
        }

        public void SetSidFilteringStatus(string targetForestName, bool enable)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            TrustHelper.SetTrustedDomainInfoStatus(this.context, this.Name, targetForestName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_TREAT_AS_EXTERNAL, enable, true);
        }

        public override string ToString()
        {
            return this.Name;
        }

        public void UpdateLocalSideOfTrustRelationship(string targetForestName, string newTrustPassword)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            if (newTrustPassword == null)
            {
                throw new ArgumentNullException("newTrustPassword");
            }
            if (newTrustPassword.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "newTrustPassword");
            }
            TrustHelper.UpdateTrust(this.context, this.Name, targetForestName, newTrustPassword, true);
        }

        public void UpdateLocalSideOfTrustRelationship(string targetForestName, TrustDirection newTrustDirection, string newTrustPassword)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            if ((newTrustDirection < TrustDirection.Inbound) || (newTrustDirection > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("newTrustDirection", (int) newTrustDirection, typeof(TrustDirection));
            }
            if (newTrustPassword == null)
            {
                throw new ArgumentNullException("newTrustPassword");
            }
            if (newTrustPassword.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "newTrustPassword");
            }
            TrustHelper.UpdateTrustDirection(this.context, this.Name, targetForestName, newTrustPassword, true, newTrustDirection);
        }

        public void UpdateTrustRelationship(Forest targetForest, TrustDirection newTrustDirection)
        {
            this.CheckIfDisposed();
            if (targetForest == null)
            {
                throw new ArgumentNullException("targetForest");
            }
            if ((newTrustDirection < TrustDirection.Inbound) || (newTrustDirection > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("newTrustDirection", (int) newTrustDirection, typeof(TrustDirection));
            }
            string password = TrustHelper.CreateTrustPassword();
            TrustHelper.UpdateTrustDirection(this.context, this.Name, targetForest.Name, password, true, newTrustDirection);
            TrustDirection direction = (TrustDirection) 0;
            if ((newTrustDirection & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                direction |= TrustDirection.Outbound;
            }
            if ((newTrustDirection & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                direction |= TrustDirection.Inbound;
            }
            TrustHelper.UpdateTrustDirection(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, password, true, direction);
        }

        public void VerifyOutboundTrustRelationship(string targetForestName)
        {
            this.CheckIfDisposed();
            if (targetForestName == null)
            {
                throw new ArgumentNullException("targetForestName");
            }
            if (targetForestName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetForestName");
            }
            TrustHelper.VerifyTrust(this.context, this.Name, targetForestName, true, TrustDirection.Outbound, false, null);
        }

        public void VerifyTrustRelationship(Forest targetForest, TrustDirection direction)
        {
            this.CheckIfDisposed();
            if (targetForest == null)
            {
                throw new ArgumentNullException("targetForest");
            }
            if ((direction < TrustDirection.Inbound) || (direction > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(TrustDirection));
            }
            if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(this.context, this.Name, targetForest.Name, true, TrustDirection.Outbound, false, null);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { this.Name, targetForest.Name, direction }), typeof(ForestTrustRelationshipInformation), null);
                }
            }
            if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(targetForest.GetDirectoryContext(), targetForest.Name, this.Name, true, TrustDirection.Outbound, false, null);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { this.Name, targetForest.Name, direction }), typeof(ForestTrustRelationshipInformation), null);
                }
            }
        }

        public ApplicationPartitionCollection ApplicationPartitions
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedApplicationPartitions == null)
                {
                    this.cachedApplicationPartitions = new ApplicationPartitionCollection(this.GetApplicationPartitions());
                }
                return this.cachedApplicationPartitions;
            }
        }

        public DomainCollection Domains
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedDomains == null)
                {
                    this.cachedDomains = new DomainCollection(this.GetDomains());
                }
                return this.cachedDomains;
            }
        }

        public System.DirectoryServices.ActiveDirectory.ForestMode ForestMode
        {
            get
            {
                this.CheckIfDisposed();
                if (this.currentForestMode == ~System.DirectoryServices.ActiveDirectory.ForestMode.Windows2000Forest)
                {
                    this.currentForestMode = this.GetForestMode();
                }
                return this.currentForestMode;
            }
        }

        public GlobalCatalogCollection GlobalCatalogs
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedGlobalCatalogs == null)
                {
                    this.cachedGlobalCatalogs = this.FindAllGlobalCatalogs();
                }
                return this.cachedGlobalCatalogs;
            }
        }

        public string Name
        {
            get
            {
                this.CheckIfDisposed();
                return this.forestDnsName;
            }
        }

        public DomainController NamingRoleOwner
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedNamingRoleOwner == null)
                {
                    this.cachedNamingRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.NamingRole);
                }
                return this.cachedNamingRoleOwner;
            }
        }

        public Domain RootDomain
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedRootDomain == null)
                {
                    DirectoryContext context = Utils.GetNewDirectoryContext(this.Name, DirectoryContextType.Domain, this.context);
                    this.cachedRootDomain = new Domain(context, this.Name);
                }
                return this.cachedRootDomain;
            }
        }

        public ActiveDirectorySchema Schema
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedSchema == null)
                {
                    try
                    {
                        this.cachedSchema = new ActiveDirectorySchema(this.context, this.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext));
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                }
                return this.cachedSchema;
            }
        }

        public DomainController SchemaRoleOwner
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedSchemaRoleOwner == null)
                {
                    this.cachedSchemaRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.SchemaRole);
                }
                return this.cachedSchemaRoleOwner;
            }
        }

        public ReadOnlySiteCollection Sites
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedSites == null)
                {
                    this.cachedSites = new ReadOnlySiteCollection(this.GetSites());
                }
                return this.cachedSites;
            }
        }
    }
}

