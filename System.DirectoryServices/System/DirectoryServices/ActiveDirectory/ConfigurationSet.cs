namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ConfigurationSet
    {
        private AdamInstanceCollection cachedADAMInstances;
        private ApplicationPartitionCollection cachedApplicationPartitions;
        private AdamInstance cachedNamingRoleOwner;
        private ActiveDirectorySchema cachedSchema;
        private AdamInstance cachedSchemaRoleOwner;
        private ReplicationSecurityLevel cachedSecurityLevel;
        private ReadOnlySiteCollection cachedSites;
        private string configSetName;
        private DirectoryContext context;
        private DirectoryEntryManager directoryEntryMgr;
        private bool disposed;
        private static TimeSpan locationTimeout = new TimeSpan(0, 4, 0);

        internal ConfigurationSet(DirectoryContext context, string configSetName) : this(context, configSetName, new DirectoryEntryManager(context))
        {
        }

        internal ConfigurationSet(DirectoryContext context, string configSetName, DirectoryEntryManager directoryEntryMgr)
        {
            this.cachedSecurityLevel = ~ReplicationSecurityLevel.NegotiatePassThrough;
            this.context = context;
            this.configSetName = configSetName;
            this.directoryEntryMgr = directoryEntryMgr;
        }

        private void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
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

        public AdamInstance FindAdamInstance()
        {
            this.CheckIfDisposed();
            return FindOneAdamInstance(this.Name, this.context, null, null);
        }

        public AdamInstance FindAdamInstance(string partitionName)
        {
            this.CheckIfDisposed();
            if (partitionName == null)
            {
                throw new ArgumentNullException("partitionName");
            }
            return FindOneAdamInstance(this.Name, this.context, partitionName, null);
        }

        public AdamInstance FindAdamInstance(string partitionName, string siteName)
        {
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return FindOneAdamInstance(this.Name, this.context, partitionName, siteName);
        }

        internal static AdamInstanceCollection FindAdamInstances(DirectoryContext context, string partitionName, string siteName)
        {
            if ((partitionName != null) && (partitionName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
            }
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            ArrayList values = new ArrayList();
            foreach (string str in Utils.GetReplicaList(context, partitionName, siteName, false, true, false))
            {
                DirectoryContext context2 = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
                values.Add(new AdamInstance(context2, str));
            }
            return new AdamInstanceCollection(values);
        }

        internal static AdamInstance FindAliveAdamInstance(string configSetName, DirectoryContext context, ArrayList adamInstanceNames)
        {
            bool flag = false;
            AdamInstance instance = null;
            DateTime utcNow = DateTime.UtcNow;
            foreach (string str in adamInstanceNames)
            {
                DirectoryContext context2 = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
                DirectoryEntryManager directoryEntryMgr = new DirectoryEntryManager(context2);
                DirectoryEntry cachedDirectoryEntry = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                try
                {
                    cachedDirectoryEntry.Bind(true);
                    instance = new AdamInstance(context2, str, directoryEntryMgr, true);
                    flag = true;
                }
                catch (COMException exception)
                {
                    if (((exception.ErrorCode != -2147016646) && (exception.ErrorCode != -2147016690)) && ((exception.ErrorCode != -2147016689) && (exception.ErrorCode != -2147023436)))
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                    }
                    if (DateTime.UtcNow.Subtract(utcNow) > locationTimeout)
                    {
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { (configSetName != null) ? configSetName : context.Name }), typeof(AdamInstance), null);
                    }
                }
                if (flag)
                {
                    return instance;
                }
            }
            throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { (configSetName != null) ? configSetName : context.Name }), typeof(AdamInstance), null);
        }

        public AdamInstanceCollection FindAllAdamInstances()
        {
            this.CheckIfDisposed();
            return FindAdamInstances(this.context, null, null);
        }

        public AdamInstanceCollection FindAllAdamInstances(string partitionName)
        {
            this.CheckIfDisposed();
            if (partitionName == null)
            {
                throw new ArgumentNullException("partitionName");
            }
            return FindAdamInstances(this.context, partitionName, null);
        }

        public AdamInstanceCollection FindAllAdamInstances(string partitionName, string siteName)
        {
            this.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return FindAdamInstances(this.context, partitionName, siteName);
        }

        internal static AdamInstance FindAnyAdamInstance(DirectoryContext context)
        {
            if (context.ContextType != DirectoryContextType.ConfigurationSet)
            {
                DirectoryEntryManager directoryEntryMgr = new DirectoryEntryManager(context);
                DirectoryEntry cachedDirectoryEntry = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (!Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryApplicationMode))
                {
                    directoryEntryMgr.RemoveIfExists(directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RootDSE));
                    throw new ArgumentException(Res.GetString("TargetShouldBeServerORConfigSet"), "context");
                }
                return new AdamInstance(context, (string) PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DnsHostName), directoryEntryMgr);
            }
            DirectoryEntry searchRootEntry = GetSearchRootEntry(Forest.GetCurrentForest());
            ArrayList adamInstanceNames = new ArrayList();
            try
            {
                string text1 = (string) searchRootEntry.Properties["distinguishedName"].Value;
                StringBuilder builder = new StringBuilder(15);
                builder.Append("(&(");
                builder.Append(PropertyManager.ObjectCategory);
                builder.Append("=serviceConnectionPoint)");
                builder.Append("(");
                builder.Append(PropertyManager.Keywords);
                builder.Append("=1.2.840.113556.1.4.1851)(");
                builder.Append(PropertyManager.Keywords);
                builder.Append("=");
                builder.Append(Utils.GetEscapedFilterValue(context.Name));
                builder.Append("))");
                string filter = builder.ToString();
                string[] propertiesToLoad = new string[] { PropertyManager.ServiceBindingInformation };
                ADSearcher searcher = new ADSearcher(searchRootEntry, filter, propertiesToLoad, SearchScope.Subtree, false, false);
                using (SearchResultCollection results = searcher.FindAll())
                {
                    foreach (SearchResult result in results)
                    {
                        string strB = "ldap://";
                        foreach (string str4 in result.Properties[PropertyManager.ServiceBindingInformation])
                        {
                            if ((str4.Length > strB.Length) && (string.Compare(str4.Substring(0, strB.Length), strB, StringComparison.OrdinalIgnoreCase) == 0))
                            {
                                adamInstanceNames.Add(str4.Substring(strB.Length));
                            }
                        }
                    }
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            finally
            {
                searchRootEntry.Dispose();
            }
            return FindAliveAdamInstance(null, context, adamInstanceNames);
        }

        internal static AdamInstance FindOneAdamInstance(DirectoryContext context, string partitionName, string siteName)
        {
            return FindOneAdamInstance(null, context, partitionName, siteName);
        }

        internal static AdamInstance FindOneAdamInstance(string configSetName, DirectoryContext context, string partitionName, string siteName)
        {
            if ((partitionName != null) && (partitionName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
            }
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            ArrayList adamInstanceNames = Utils.GetReplicaList(context, partitionName, siteName, false, true, false);
            if (adamInstanceNames.Count < 1)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ADAMInstanceNotFound"), typeof(AdamInstance), null);
            }
            return FindAliveAdamInstance(configSetName, context, adamInstanceNames);
        }

        private ArrayList GetApplicationPartitions()
        {
            ArrayList list = new ArrayList();
            DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
            DirectoryEntry searchRoot = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.PartitionsContainer);
            StringBuilder builder = new StringBuilder(100);
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
            string[] propertiesToLoad = new string[] { PropertyManager.NCName, PropertyManager.MsDSNCReplicaLocations };
            ADSearcher searcher = new ADSearcher(searchRoot, filter, propertiesToLoad, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
                string str2 = (string) PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.SchemaNamingContext);
                string str3 = (string) PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.ConfigurationNamingContext);
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.NCName);
                    if (!searchResultPropertyValue.Equals(str2) && !searchResultPropertyValue.Equals(str3))
                    {
                        ResultPropertyValueCollection values = result.Properties[PropertyManager.MsDSNCReplicaLocations];
                        if (values.Count > 0)
                        {
                            DirectoryContext context = Utils.GetNewDirectoryContext(Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string) values[Utils.GetRandomIndex(values.Count)]), DirectoryContextType.DirectoryServer, this.context);
                            list.Add(new ApplicationPartition(context, searchResultPropertyValue, null, ApplicationPartitionType.ADAMApplicationPartition, new DirectoryEntryManager(context)));
                        }
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
            }
            return list;
        }

        public static ConfigurationSet GetConfigurationSet(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.ContextType != DirectoryContextType.ConfigurationSet) && (context.ContextType != DirectoryContextType.DirectoryServer))
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeServerORConfigSet"), "context");
            }
            if (!context.isServer() && !context.isADAMConfigSet())
            {
                if (context.ContextType == DirectoryContextType.ConfigurationSet)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ConfigurationSet), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", new object[] { context.Name }), typeof(ConfigurationSet), null);
            }
            context = new DirectoryContext(context);
            DirectoryEntryManager directoryEntryMgr = new DirectoryEntryManager(context);
            DirectoryEntry rootDSE = null;
            string configSetName = null;
            try
            {
                rootDSE = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (context.isServer() && !Utils.CheckCapability(rootDSE, Capability.ActiveDirectoryApplicationMode))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", new object[] { context.Name }), typeof(ConfigurationSet), null);
                }
                configSetName = (string) PropertyManager.GetPropertyValue(context, rootDSE, PropertyManager.ConfigurationNamingContext);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016646)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if (context.ContextType == DirectoryContextType.ConfigurationSet)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ConfigurationSet), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", new object[] { context.Name }), typeof(ConfigurationSet), null);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                if (context.ContextType == DirectoryContextType.ConfigurationSet)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ConfigurationSet), context.Name);
                }
                throw;
            }
            return new ConfigurationSet(context, configSetName, directoryEntryMgr);
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            this.CheckIfDisposed();
            return DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.ConfigurationNamingContext);
        }

        private AdamInstance GetRoleOwner(AdamRole role)
        {
            DirectoryEntry directoryEntry = null;
            string adamInstanceName = null;
            try
            {
                switch (role)
                {
                    case AdamRole.SchemaRole:
                        directoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.SchemaNamingContext);
                        break;

                    case AdamRole.NamingRole:
                        directoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.PartitionsContainer);
                        break;
                }
                directoryEntry.RefreshCache();
                adamInstanceName = Utils.GetAdamDnsHostNameFromNTDSA(this.context, (string) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.FsmoRoleOwner));
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
            return new AdamInstance(Utils.GetNewDirectoryContext(adamInstanceName, DirectoryContextType.DirectoryServer, this.context), adamInstanceName);
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        private static DirectoryEntry GetSearchRootEntry(Forest forest)
        {
            DirectoryContext directoryContext = forest.GetDirectoryContext();
            bool flag = false;
            bool flag2 = false;
            AuthenticationTypes defaultAuthType = Utils.DefaultAuthType;
            if (directoryContext.ContextType == DirectoryContextType.DirectoryServer)
            {
                flag = true;
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(directoryContext, WellKnownDN.RootDSE);
                string str = (string) PropertyManager.GetPropertyValue(directoryContext, directoryEntry, PropertyManager.IsGlobalCatalogReady);
                flag2 = Utils.Compare(str, "TRUE") == 0;
            }
            if (flag)
            {
                if (DirectoryContext.ServerBindSupported)
                {
                    defaultAuthType |= AuthenticationTypes.ServerBind;
                }
                if (flag2)
                {
                    return new DirectoryEntry("GC://" + directoryContext.GetServerName(), directoryContext.UserName, directoryContext.Password, defaultAuthType);
                }
                return new DirectoryEntry("LDAP://" + directoryContext.GetServerName(), directoryContext.UserName, directoryContext.Password, defaultAuthType);
            }
            return new DirectoryEntry("GC://" + forest.Name, directoryContext.UserName, directoryContext.Password, defaultAuthType);
        }

        public ReplicationSecurityLevel GetSecurityLevel()
        {
            this.CheckIfDisposed();
            if (this.cachedSecurityLevel == ~ReplicationSecurityLevel.NegotiatePassThrough)
            {
                DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.ConfigurationNamingContext);
                this.cachedSecurityLevel = (ReplicationSecurityLevel) ((int) PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.MsDSReplAuthenticationMode));
            }
            return this.cachedSecurityLevel;
        }

        private ArrayList GetSites()
        {
            ArrayList list = new ArrayList();
            DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.SitesContainer);
            string filter = "(" + PropertyManager.ObjectCategory + "=site)";
            string[] propertiesToLoad = new string[] { PropertyManager.Cn };
            ADSearcher searcher = new ADSearcher(cachedDirectoryEntry, filter, propertiesToLoad, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    list.Add(new ActiveDirectorySite(this.context, (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn), true));
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
            }
            return list;
        }

        public void SetSecurityLevel(ReplicationSecurityLevel securityLevel)
        {
            this.CheckIfDisposed();
            if ((securityLevel < ReplicationSecurityLevel.NegotiatePassThrough) || (securityLevel > ReplicationSecurityLevel.MutualAuthentication))
            {
                throw new InvalidEnumArgumentException("securityLevel", (int) securityLevel, typeof(ReplicationSecurityLevel));
            }
            try
            {
                DirectoryEntry cachedDirectoryEntry = this.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.ConfigurationNamingContext);
                cachedDirectoryEntry.Properties[PropertyManager.MsDSReplAuthenticationMode].Value = (int) securityLevel;
                cachedDirectoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            this.cachedSecurityLevel = ~ReplicationSecurityLevel.NegotiatePassThrough;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public AdamInstanceCollection AdamInstances
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedADAMInstances == null)
                {
                    this.cachedADAMInstances = this.FindAllAdamInstances();
                }
                return this.cachedADAMInstances;
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

        public string Name
        {
            get
            {
                this.CheckIfDisposed();
                return this.configSetName;
            }
        }

        public AdamInstance NamingRoleOwner
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedNamingRoleOwner == null)
                {
                    this.cachedNamingRoleOwner = this.GetRoleOwner(AdamRole.NamingRole);
                }
                return this.cachedNamingRoleOwner;
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

        public AdamInstance SchemaRoleOwner
        {
            get
            {
                this.CheckIfDisposed();
                if (this.cachedSchemaRoleOwner == null)
                {
                    this.cachedSchemaRoleOwner = this.GetRoleOwner(AdamRole.SchemaRole);
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

