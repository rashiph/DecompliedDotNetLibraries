namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ApplicationPartition : ActiveDirectoryPartition
    {
        private ApplicationPartitionType appType;
        private DirectoryServerCollection cachedDirectoryServers;
        private bool committed;
        private DirectoryEntry crossRefEntry;
        private bool disposed;
        private string dnsName;
        private DirectoryEntry domainDNSEntry;
        private string securityRefDomain;
        private bool securityRefDomainModified;

        public ApplicationPartition(DirectoryContext context, string distinguishedName)
        {
            this.appType = ApplicationPartitionType.Unknown;
            this.committed = true;
            this.ValidateApplicationPartitionParameters(context, distinguishedName, null, false);
            this.CreateApplicationPartition(distinguishedName, "domainDns");
        }

        public ApplicationPartition(DirectoryContext context, string distinguishedName, string objectClass)
        {
            this.appType = ApplicationPartitionType.Unknown;
            this.committed = true;
            this.ValidateApplicationPartitionParameters(context, distinguishedName, objectClass, true);
            this.CreateApplicationPartition(distinguishedName, objectClass);
        }

        internal ApplicationPartition(DirectoryContext context, string distinguishedName, string dnsName, DirectoryEntryManager directoryEntryMgr) : this(context, distinguishedName, dnsName, GetApplicationPartitionType(context), directoryEntryMgr)
        {
        }

        internal ApplicationPartition(DirectoryContext context, string distinguishedName, string dnsName, ApplicationPartitionType appType, DirectoryEntryManager directoryEntryMgr) : base(context, distinguishedName)
        {
            this.appType = ApplicationPartitionType.Unknown;
            this.committed = true;
            base.directoryEntryMgr = directoryEntryMgr;
            this.appType = appType;
            this.dnsName = dnsName;
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        private void CreateApplicationPartition(string distinguishedName, string objectClass)
        {
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                DirectoryEntry entry = null;
                DirectoryEntry parent = null;
                try
                {
                    AuthenticationTypes authenticationType = (Utils.DefaultAuthType | AuthenticationTypes.FastBind) | AuthenticationTypes.Delegation;
                    if (DirectoryContext.ServerBindSupported)
                    {
                        authenticationType |= AuthenticationTypes.ServerBind;
                    }
                    entry = new DirectoryEntry("LDAP://" + base.context.GetServerName() + "/" + distinguishedName, base.context.UserName, base.context.Password, authenticationType);
                    parent = entry.Parent;
                    this.domainDNSEntry = parent.Children.Add(Utils.GetRdnFromDN(distinguishedName), PropertyManager.DomainDNS);
                    this.domainDNSEntry.Properties[PropertyManager.InstanceType].Value = NCFlags.InstanceTypeIsWriteable | NCFlags.InstanceTypeIsNCHead;
                    this.committed = false;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                }
                finally
                {
                    if (parent != null)
                    {
                        parent.Dispose();
                    }
                    if (entry != null)
                    {
                        entry.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    this.InitializeCrossRef(distinguishedName);
                    DirectoryEntry entry3 = null;
                    DirectoryEntry entry4 = null;
                    try
                    {
                        AuthenticationTypes types2 = Utils.DefaultAuthType | AuthenticationTypes.FastBind;
                        if (DirectoryContext.ServerBindSupported)
                        {
                            types2 |= AuthenticationTypes.ServerBind;
                        }
                        entry3 = new DirectoryEntry("LDAP://" + base.context.Name + "/" + distinguishedName, base.context.UserName, base.context.Password, types2);
                        entry4 = entry3.Parent;
                        this.domainDNSEntry = entry4.Children.Add(Utils.GetRdnFromDN(distinguishedName), objectClass);
                        this.domainDNSEntry.Properties[PropertyManager.InstanceType].Value = NCFlags.InstanceTypeIsWriteable | NCFlags.InstanceTypeIsNCHead;
                        this.committed = false;
                    }
                    finally
                    {
                        if (entry4 != null)
                        {
                            entry4.Dispose();
                        }
                        if (entry3 != null)
                        {
                            entry3.Dispose();
                        }
                    }
                }
                catch (COMException exception2)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception2);
                }
            }
        }

        public void Delete()
        {
            base.CheckIfDisposed();
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
            try
            {
                this.GetCrossRefEntry();
                directoryEntry.Children.Remove(this.crossRefEntry);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                directoryEntry.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    if (this.crossRefEntry != null)
                    {
                        this.crossRefEntry.Dispose();
                        this.crossRefEntry = null;
                    }
                    if (this.domainDNSEntry != null)
                    {
                        this.domainDNSEntry.Dispose();
                        this.domainDNSEntry = null;
                    }
                    this.disposed = true;
                }
                finally
                {
                    base.Dispose();
                }
            }
        }

        public ReadOnlyDirectoryServerCollection FindAllDirectoryServers()
        {
            base.CheckIfDisposed();
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                return this.FindAllDirectoryServersInternal(null);
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            ReadOnlyDirectoryServerCollection servers = new ReadOnlyDirectoryServerCollection();
            servers.AddRange(ConfigurationSet.FindAdamInstances(base.context, base.Name, null));
            return servers;
        }

        public ReadOnlyDirectoryServerCollection FindAllDirectoryServers(string siteName)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                return this.FindAllDirectoryServersInternal(siteName);
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            ReadOnlyDirectoryServerCollection servers = new ReadOnlyDirectoryServerCollection();
            servers.AddRange(ConfigurationSet.FindAdamInstances(base.context, base.Name, siteName));
            return servers;
        }

        private ReadOnlyDirectoryServerCollection FindAllDirectoryServersInternal(string siteName)
        {
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            ArrayList values = new ArrayList();
            foreach (string str in Utils.GetReplicaList(base.context, base.Name, siteName, false, false, false))
            {
                DirectoryContext context = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, base.context);
                values.Add(new DomainController(context, str));
            }
            return new ReadOnlyDirectoryServerCollection(values);
        }

        public ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServers()
        {
            base.CheckIfDisposed();
            if (this.appType != ApplicationPartitionType.ADApplicationPartition)
            {
                throw new NotSupportedException(Res.GetString("OperationInvalidForADAM"));
            }
            return this.FindAllDiscoverableDirectoryServersInternal(null);
        }

        public ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServers(string siteName)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (this.appType != ApplicationPartitionType.ADApplicationPartition)
            {
                throw new NotSupportedException(Res.GetString("OperationInvalidForADAM"));
            }
            return this.FindAllDiscoverableDirectoryServersInternal(siteName);
        }

        private ReadOnlyDirectoryServerCollection FindAllDiscoverableDirectoryServersInternal(string siteName)
        {
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            long dcFlags = 0x8000L;
            return new ReadOnlyDirectoryServerCollection(Locator.EnumerateDomainControllers(base.context, this.dnsName, siteName, dcFlags));
        }

        public static ApplicationPartition FindByName(DirectoryContext context, string distinguishedName)
        {
            DirectoryEntryManager directoryEntryMgr = null;
            DirectoryContext context2 = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
            }
            if (((context.Name != null) && !context.isRootDomain()) && (!context.isADAMConfigSet() && !context.isServer()))
            {
                throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
            }
            if (distinguishedName == null)
            {
                throw new ArgumentNullException("distinguishedName");
            }
            if (distinguishedName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "distinguishedName");
            }
            if (!Utils.IsValidDNFormat(distinguishedName))
            {
                throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
            }
            context = new DirectoryContext(context);
            directoryEntryMgr = new DirectoryEntryManager(context);
            DirectoryEntry searchRoot = null;
            try
            {
                searchRoot = DirectoryEntryManager.GetDirectoryEntry(context, directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
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
            builder.Append(Utils.GetEscapedFilterValue(distinguishedName));
            builder.Append("))");
            string filter = builder.ToString();
            string[] propertiesToLoad = new string[] { PropertyManager.DnsRoot, PropertyManager.NCName };
            ADSearcher searcher = new ADSearcher(searchRoot, filter, propertiesToLoad, SearchScope.OneLevel, false, false);
            SearchResult res = null;
            try
            {
                res = searcher.FindOne();
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode == -2147016656)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception2);
            }
            finally
            {
                searchRoot.Dispose();
            }
            if (res == null)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
            }
            string domainName = null;
            try
            {
                domainName = (res.Properties[PropertyManager.DnsRoot].Count > 0) ? ((string) res.Properties[PropertyManager.DnsRoot][0]) : null;
            }
            catch (COMException exception3)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception3);
            }
            ApplicationPartitionType applicationPartitionType = GetApplicationPartitionType(context);
            if (context.ContextType != DirectoryContextType.DirectoryServer)
            {
                if (applicationPartitionType == ApplicationPartitionType.ADApplicationPartition)
                {
                    DomainControllerInfo info;
                    int errorCode = 0;
                    errorCode = Locator.DsGetDcNameWrapper(null, domainName, null, 0x8000L, out info);
                    if (errorCode == 0x54b)
                    {
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
                    }
                    if (errorCode != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                    }
                    context2 = Utils.GetNewDirectoryContext(info.DomainControllerName.Substring(2), DirectoryContextType.DirectoryServer, context);
                }
                else
                {
                    context2 = Utils.GetNewDirectoryContext(ConfigurationSet.FindOneAdamInstance(context.Name, context, distinguishedName, null).Name, DirectoryContextType.DirectoryServer, context);
                }
                goto Label_03FC;
            }
            bool flag = false;
            DistinguishedName dn = new DistinguishedName(distinguishedName);
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
            try
            {
                foreach (string str3 in directoryEntry.Properties[PropertyManager.NamingContexts])
                {
                    DistinguishedName name2 = new DistinguishedName(str3);
                    if (name2.Equals(dn))
                    {
                        flag = true;
                        goto Label_0352;
                    }
                }
            }
            catch (COMException exception4)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception4);
            }
            finally
            {
                directoryEntry.Dispose();
            }
        Label_0352:
            if (!flag)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ApplicationPartition), distinguishedName);
            }
            context2 = context;
        Label_03FC:
            return new ApplicationPartition(context2, (string) PropertyManager.GetSearchResultPropertyValue(res, PropertyManager.NCName), domainName, applicationPartitionType, directoryEntryMgr);
        }

        public DirectoryServer FindDirectoryServer()
        {
            base.CheckIfDisposed();
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                return this.FindDirectoryServerInternal(null, false);
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            return ConfigurationSet.FindOneAdamInstance(base.context, base.Name, null);
        }

        public DirectoryServer FindDirectoryServer(bool forceRediscovery)
        {
            base.CheckIfDisposed();
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                return this.FindDirectoryServerInternal(null, forceRediscovery);
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            return ConfigurationSet.FindOneAdamInstance(base.context, base.Name, null);
        }

        public DirectoryServer FindDirectoryServer(string siteName)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                return this.FindDirectoryServerInternal(siteName, false);
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            return ConfigurationSet.FindOneAdamInstance(base.context, base.Name, siteName);
        }

        public DirectoryServer FindDirectoryServer(string siteName, bool forceRediscovery)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                return this.FindDirectoryServerInternal(siteName, forceRediscovery);
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            return ConfigurationSet.FindOneAdamInstance(base.context, base.Name, siteName);
        }

        private DirectoryServer FindDirectoryServerInternal(string siteName, bool forceRediscovery)
        {
            DomainControllerInfo info;
            LocatorOptions options = 0L;
            int errorCode = 0;
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotPerformOperationOnUncommittedObject"));
            }
            if (forceRediscovery)
            {
                options = LocatorOptions.ForceRediscovery;
            }
            errorCode = Locator.DsGetDcNameWrapper(null, this.dnsName, siteName, options | 0x8000L, out info);
            if (errorCode == 0x54b)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ReplicaNotFound"), typeof(DirectoryServer), null);
            }
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            string domainControllerName = info.DomainControllerName.Substring(2);
            return new DomainController(Utils.GetNewDirectoryContext(domainControllerName, DirectoryContextType.DirectoryServer, base.context), domainControllerName);
        }

        public static ApplicationPartition GetApplicationPartition(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.ApplicationPartition)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeAppNCDnsName"), "context");
            }
            if (!context.isNdnc())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("NDNCNotFound"), typeof(ApplicationPartition), context.Name);
            }
            context = new DirectoryContext(context);
            string dNFromDnsName = Utils.GetDNFromDnsName(context.Name);
            DirectoryEntryManager directoryEntryMgr = new DirectoryEntryManager(context);
            try
            {
                directoryEntryMgr.GetCachedDirectoryEntry(dNFromDnsName).Bind(true);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016646)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("NDNCNotFound"), typeof(ApplicationPartition), context.Name);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            return new ApplicationPartition(context, dNFromDnsName, context.Name, ApplicationPartitionType.ADApplicationPartition, directoryEntryMgr);
        }

        private static ApplicationPartitionType GetApplicationPartitionType(DirectoryContext context)
        {
            ApplicationPartitionType unknown = ApplicationPartitionType.Unknown;
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
            try
            {
                foreach (string str in directoryEntry.Properties[PropertyManager.SupportedCapabilities])
                {
                    if (string.Compare(str, SupportedCapability.ADOid, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        unknown = ApplicationPartitionType.ADApplicationPartition;
                    }
                    if (string.Compare(str, SupportedCapability.ADAMOid, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        unknown = ApplicationPartitionType.ADAMApplicationPartition;
                    }
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            if (unknown == ApplicationPartitionType.Unknown)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ApplicationPartitionTypeUnknown"));
            }
            return unknown;
        }

        internal DirectoryEntry GetCrossRefEntry()
        {
            if (this.crossRefEntry == null)
            {
                using (DirectoryEntry entry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer)))
                {
                    this.crossRefEntry = Utils.GetCrossRefEntry(base.context, entry, base.Name);
                }
            }
            return this.crossRefEntry;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override DirectoryEntry GetDirectoryEntry()
        {
            base.CheckIfDisposed();
            if (!this.committed)
            {
                throw new InvalidOperationException(Res.GetString("CannotGetObject"));
            }
            return DirectoryEntryManager.GetDirectoryEntry(base.context, base.Name);
        }

        internal string GetNamingRoleOwner()
        {
            using (DirectoryEntry entry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer)))
            {
                if (this.appType == ApplicationPartitionType.ADApplicationPartition)
                {
                    return Utils.GetDnsHostNameFromNTDSA(base.context, (string) PropertyManager.GetPropertyValue(base.context, entry, PropertyManager.FsmoRoleOwner));
                }
                return Utils.GetAdamDnsHostNameFromNTDSA(base.context, (string) PropertyManager.GetPropertyValue(base.context, entry, PropertyManager.FsmoRoleOwner));
            }
        }

        private void InitializeCrossRef(string distinguishedName)
        {
            if (this.crossRefEntry == null)
            {
                DirectoryEntry directoryEntry = null;
                try
                {
                    directoryEntry = DirectoryEntryManager.GetDirectoryEntry(Utils.GetNewDirectoryContext(this.GetNamingRoleOwner(), DirectoryContextType.DirectoryServer, base.context), WellKnownDN.PartitionsContainer);
                    string name = "CN={" + Guid.NewGuid() + "}";
                    this.crossRefEntry = directoryEntry.Children.Add(name, "crossRef");
                    string adamHostNameAndPortsFromNTDSA = null;
                    if (this.appType == ApplicationPartitionType.ADAMApplicationPartition)
                    {
                        DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                        string dn = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.DsServiceName);
                        adamHostNameAndPortsFromNTDSA = Utils.GetAdamHostNameAndPortsFromNTDSA(base.context, dn);
                    }
                    else
                    {
                        adamHostNameAndPortsFromNTDSA = base.context.Name;
                    }
                    this.crossRefEntry.Properties[PropertyManager.DnsRoot].Value = adamHostNameAndPortsFromNTDSA;
                    this.crossRefEntry.Properties[PropertyManager.Enabled].Value = false;
                    this.crossRefEntry.Properties[PropertyManager.NCName].Value = distinguishedName;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                }
                finally
                {
                    if (directoryEntry != null)
                    {
                        directoryEntry.Dispose();
                    }
                }
            }
        }

        public void Save()
        {
            base.CheckIfDisposed();
            if (this.committed)
            {
                goto Label_021F;
            }
            bool flag = false;
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                try
                {
                    this.domainDNSEntry.CommitChanges();
                    goto Label_004B;
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode != -2147016663)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                    }
                    flag = true;
                    goto Label_004B;
                }
            }
            flag = true;
        Label_004B:
            if (flag)
            {
                try
                {
                    this.InitializeCrossRef(base.partitionName);
                    this.crossRefEntry.CommitChanges();
                }
                catch (COMException exception2)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception2);
                }
                try
                {
                    this.domainDNSEntry.CommitChanges();
                }
                catch (COMException exception3)
                {
                    DirectoryEntry parent = this.crossRefEntry.Parent;
                    try
                    {
                        parent.Children.Remove(this.crossRefEntry);
                    }
                    catch (COMException exception4)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(exception4);
                    }
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception3);
                }
                try
                {
                    this.crossRefEntry.RefreshCache();
                }
                catch (COMException exception5)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception5);
                }
            }
            DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
            string str = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.DsServiceName);
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                this.GetCrossRefEntry();
            }
            string str2 = (string) PropertyManager.GetPropertyValue(base.context, this.crossRefEntry, PropertyManager.DistinguishedName);
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(Utils.GetNewDirectoryContext(this.GetNamingRoleOwner(), DirectoryContextType.DirectoryServer, base.context), WellKnownDN.RootDSE);
            try
            {
                directoryEntry.Properties[PropertyManager.ReplicateSingleObject].Value = str + ":" + str2;
                directoryEntry.CommitChanges();
            }
            catch (COMException exception6)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception6);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            this.committed = true;
            if ((this.cachedDirectoryServers == null) && !this.securityRefDomainModified)
            {
                goto Label_024C;
            }
            if (this.cachedDirectoryServers != null)
            {
                this.crossRefEntry.Properties[PropertyManager.MsDSNCReplicaLocations].AddRange(this.cachedDirectoryServers.GetMultiValuedProperty());
            }
            if (this.securityRefDomainModified)
            {
                this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Value = this.securityRefDomain;
            }
            try
            {
                this.crossRefEntry.CommitChanges();
                goto Label_024C;
            }
            catch (COMException exception7)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception7);
            }
        Label_021F:
            if ((this.cachedDirectoryServers != null) || this.securityRefDomainModified)
            {
                try
                {
                    this.crossRefEntry.CommitChanges();
                }
                catch (COMException exception8)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception8);
                }
            }
        Label_024C:
            this.cachedDirectoryServers = null;
            this.securityRefDomainModified = false;
        }

        private void ValidateApplicationPartitionParameters(DirectoryContext context, string distinguishedName, string objectClass, bool objectClassSpecified)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) || !context.isServer())
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeServer"), "context");
            }
            if (distinguishedName == null)
            {
                throw new ArgumentNullException("distinguishedName");
            }
            if (distinguishedName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "distinguishedName");
            }
            base.context = new DirectoryContext(context);
            base.directoryEntryMgr = new DirectoryEntryManager(base.context);
            this.dnsName = Utils.GetDnsNameFromDN(distinguishedName);
            base.partitionName = distinguishedName;
            if (Utils.GetDNComponents(distinguishedName).Length == 1)
            {
                throw new NotSupportedException(Res.GetString("OneLevelPartitionNotSupported"));
            }
            this.appType = GetApplicationPartitionType(base.context);
            if ((this.appType == ApplicationPartitionType.ADApplicationPartition) && objectClassSpecified)
            {
                throw new InvalidOperationException(Res.GetString("NoObjectClassForADPartition"));
            }
            if (objectClassSpecified)
            {
                if (objectClass == null)
                {
                    throw new ArgumentNullException("objectClass");
                }
                if (objectClass.Length == 0)
                {
                    throw new ArgumentException(Res.GetString("EmptyStringParameter"), "objectClass");
                }
            }
            if (this.appType == ApplicationPartitionType.ADApplicationPartition)
            {
                string name = null;
                try
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                    name = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.DnsHostName);
                }
                catch (COMException exception)
                {
                    ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                }
                base.context = Utils.GetNewDirectoryContext(name, DirectoryContextType.DirectoryServer, context);
            }
        }

        public DirectoryServerCollection DirectoryServers
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedDirectoryServers == null)
                {
                    ReadOnlyDirectoryServerCollection servers = this.committed ? this.FindAllDirectoryServers() : new ReadOnlyDirectoryServerCollection();
                    bool isADAM = this.appType == ApplicationPartitionType.ADAMApplicationPartition;
                    if (this.committed)
                    {
                        this.GetCrossRefEntry();
                    }
                    this.cachedDirectoryServers = new DirectoryServerCollection(base.context, this.committed ? this.crossRefEntry : null, isADAM, servers);
                }
                return this.cachedDirectoryServers;
            }
        }

        public string SecurityReferenceDomain
        {
            get
            {
                base.CheckIfDisposed();
                if (this.appType == ApplicationPartitionType.ADAMApplicationPartition)
                {
                    throw new NotSupportedException(Res.GetString("PropertyInvalidForADAM"));
                }
                if (this.committed)
                {
                    this.GetCrossRefEntry();
                    try
                    {
                        if (this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Count > 0)
                        {
                            return (string) this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Value;
                        }
                        return null;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                    }
                }
                return this.securityRefDomain;
            }
            set
            {
                base.CheckIfDisposed();
                if (this.appType == ApplicationPartitionType.ADAMApplicationPartition)
                {
                    throw new NotSupportedException(Res.GetString("PropertyInvalidForADAM"));
                }
                if (this.committed)
                {
                    this.GetCrossRefEntry();
                    if (value != null)
                    {
                        this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Value = value;
                        this.securityRefDomainModified = true;
                    }
                    else if (this.crossRefEntry.Properties.Contains(PropertyManager.MsDSSDReferenceDomain))
                    {
                        this.crossRefEntry.Properties[PropertyManager.MsDSSDReferenceDomain].Clear();
                        this.securityRefDomainModified = true;
                    }
                }
                else if ((this.securityRefDomain != null) || (value != null))
                {
                    this.securityRefDomain = value;
                    this.securityRefDomainModified = true;
                }
            }
        }
    }
}

