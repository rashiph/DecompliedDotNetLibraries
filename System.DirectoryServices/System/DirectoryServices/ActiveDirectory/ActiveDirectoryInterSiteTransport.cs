namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectoryInterSiteTransport : IDisposable
    {
        private ReadOnlySiteLinkBridgeCollection bridgeCollection = new ReadOnlySiteLinkBridgeCollection();
        private bool bridgeRetrieved;
        private DirectoryEntry cachedEntry;
        private DirectoryContext context;
        private bool disposed;
        private bool linkRetrieved;
        private ReadOnlySiteLinkCollection siteLinkCollection = new ReadOnlySiteLinkCollection();
        private ActiveDirectoryTransportType transport;

        internal ActiveDirectoryInterSiteTransport(DirectoryContext context, ActiveDirectoryTransportType transport, DirectoryEntry entry)
        {
            this.context = context;
            this.transport = transport;
            this.cachedEntry = entry;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.cachedEntry != null))
            {
                this.cachedEntry.Dispose();
            }
            this.disposed = true;
        }

        public static ActiveDirectoryInterSiteTransport FindByTransportType(DirectoryContext context, ActiveDirectoryTransportType transport)
        {
            DirectoryEntry directoryEntry;
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
            if ((transport < ActiveDirectoryTransportType.Rpc) || (transport > ActiveDirectoryTransportType.Smtp))
            {
                throw new InvalidEnumArgumentException("value", (int) transport, typeof(ActiveDirectoryTransportType));
            }
            context = new DirectoryContext(context);
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
                string dn = "CN=Inter-Site Transports,CN=Sites," + str;
                if (transport == ActiveDirectoryTransportType.Rpc)
                {
                    dn = "CN=IP," + dn;
                }
                else
                {
                    dn = "CN=SMTP," + dn;
                }
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
                directoryEntry.RefreshCache(new string[] { "options" });
            }
            catch (COMException exception2)
            {
                if (exception2.ErrorCode != -2147016656)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception2);
                }
                if (Utils.CheckCapability(DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE), Capability.ActiveDirectoryApplicationMode) && (transport == ActiveDirectoryTransportType.Smtp))
                {
                    throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("TransportNotFound", new object[] { transport.ToString() }), typeof(ActiveDirectoryInterSiteTransport), transport.ToString());
            }
            return new ActiveDirectoryInterSiteTransport(context, transport, directoryEntry);
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedEntry.Path);
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
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        public override string ToString()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return this.transport.ToString();
        }

        public bool BridgeAllSiteLinks
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                try
                {
                    if (this.cachedEntry.Properties.Contains("options"))
                    {
                        num = (int) this.cachedEntry.Properties["options"][0];
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if ((num & 2) != 0)
                {
                    return false;
                }
                return true;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                try
                {
                    if (this.cachedEntry.Properties.Contains("options"))
                    {
                        num = (int) this.cachedEntry.Properties["options"][0];
                    }
                    if (value)
                    {
                        num &= -3;
                    }
                    else
                    {
                        num |= 2;
                    }
                    this.cachedEntry.Properties["options"].Value = num;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public bool IgnoreReplicationSchedule
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                try
                {
                    if (this.cachedEntry.Properties.Contains("options"))
                    {
                        num = (int) this.cachedEntry.Properties["options"][0];
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return ((num & 1) != 0);
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                try
                {
                    if (this.cachedEntry.Properties.Contains("options"))
                    {
                        num = (int) this.cachedEntry.Properties["options"][0];
                    }
                    if (value)
                    {
                        num |= 1;
                    }
                    else
                    {
                        num &= -2;
                    }
                    this.cachedEntry.Properties["options"].Value = num;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public ReadOnlySiteLinkBridgeCollection SiteLinkBridges
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (!this.bridgeRetrieved)
                {
                    this.bridgeCollection.Clear();
                    ADSearcher searcher = new ADSearcher(this.cachedEntry, "(&(objectClass=siteLinkBridge)(objectCategory=SiteLinkBridge))", new string[] { "cn" }, SearchScope.OneLevel);
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
                            DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                            string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                            ActiveDirectorySiteLinkBridge bridge = new ActiveDirectorySiteLinkBridge(this.context, searchResultPropertyValue, this.transport, true) {
                                cachedEntry = directoryEntry
                            };
                            this.bridgeCollection.Add(bridge);
                        }
                    }
                    finally
                    {
                        results.Dispose();
                    }
                    this.bridgeRetrieved = true;
                }
                return this.bridgeCollection;
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
                if (!this.linkRetrieved)
                {
                    this.siteLinkCollection.Clear();
                    ADSearcher searcher = new ADSearcher(this.cachedEntry, "(&(objectClass=siteLink)(objectCategory=SiteLink))", new string[] { "cn" }, SearchScope.OneLevel);
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
                            DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                            string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                            ActiveDirectorySiteLink link = new ActiveDirectorySiteLink(this.context, searchResultPropertyValue, this.transport, true, directoryEntry);
                            this.siteLinkCollection.Add(link);
                        }
                    }
                    finally
                    {
                        results.Dispose();
                    }
                    this.linkRetrieved = true;
                }
                return this.siteLinkCollection;
            }
        }

        public ActiveDirectoryTransportType TransportType
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.transport;
            }
        }
    }
}

