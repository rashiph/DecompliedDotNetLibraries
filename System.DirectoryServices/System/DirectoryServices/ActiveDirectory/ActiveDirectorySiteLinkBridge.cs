namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectorySiteLinkBridge : IDisposable
    {
        internal DirectoryEntry cachedEntry;
        internal DirectoryContext context;
        private bool disposed;
        private bool existing;
        private ActiveDirectorySiteLinkCollection links;
        private bool linksRetrieved;
        private string name;
        private ActiveDirectoryTransportType transport;

        public ActiveDirectorySiteLinkBridge(DirectoryContext context, string bridgeName) : this(context, bridgeName, ActiveDirectoryTransportType.Rpc)
        {
        }

        public ActiveDirectorySiteLinkBridge(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport)
        {
            DirectoryEntry directoryEntry;
            this.links = new ActiveDirectorySiteLinkCollection();
            ValidateArgument(context, bridgeName, transport);
            context = new DirectoryContext(context);
            this.context = context;
            this.name = bridgeName;
            this.transport = transport;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
                string dn = null;
                if (transport == ActiveDirectoryTransportType.Rpc)
                {
                    dn = "CN=IP,CN=Inter-Site Transports,CN=Sites," + str;
                }
                else
                {
                    dn = "CN=SMTP,CN=Inter-Site Transports,CN=Sites," + str;
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
                string escapedPath = Utils.GetEscapedPath("cn=" + this.name);
                this.cachedEntry = directoryEntry.Children.Add(escapedPath, "siteLinkBridge");
            }
            catch (COMException exception2)
            {
                if (((exception2.ErrorCode == -2147016656) && Utils.CheckCapability(DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE), Capability.ActiveDirectoryApplicationMode)) && (transport == ActiveDirectoryTransportType.Smtp))
                {
                    throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception2);
            }
            finally
            {
                directoryEntry.Dispose();
            }
        }

        internal ActiveDirectorySiteLinkBridge(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport, bool existing)
        {
            this.links = new ActiveDirectorySiteLinkCollection();
            this.context = context;
            this.name = bridgeName;
            this.transport = transport;
            this.existing = existing;
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
                this.cachedEntry.Parent.Children.Remove(this.cachedEntry);
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
            if (disposing && (this.cachedEntry != null))
            {
                this.cachedEntry.Dispose();
            }
            this.disposed = true;
        }

        public static ActiveDirectorySiteLinkBridge FindByName(DirectoryContext context, string bridgeName)
        {
            return FindByName(context, bridgeName, ActiveDirectoryTransportType.Rpc);
        }

        public static ActiveDirectorySiteLinkBridge FindByName(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport)
        {
            DirectoryEntry directoryEntry;
            ActiveDirectorySiteLinkBridge bridge2;
            ValidateArgument(context, bridgeName, transport);
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
                SearchResult result = new ADSearcher(directoryEntry, "(&(objectClass=siteLinkBridge)(objectCategory=SiteLinkBridge)(name=" + Utils.GetEscapedFilterValue(bridgeName) + "))", new string[] { "distinguishedName" }, SearchScope.OneLevel, false, false).FindOne();
                if (result == null)
                {
                    Exception exception2 = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLinkBridge), bridgeName);
                    throw exception2;
                }
                DirectoryEntry entry2 = result.GetDirectoryEntry();
                bridge2 = new ActiveDirectorySiteLinkBridge(context, bridgeName, transport, true) {
                    cachedEntry = entry2
                };
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode != -2147016656)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception3);
                }
                if (Utils.CheckCapability(DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE), Capability.ActiveDirectoryApplicationMode) && (transport == ActiveDirectoryTransportType.Smtp))
                {
                    throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLinkBridge), bridgeName);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            return bridge2;
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

        private void GetLinks()
        {
            ArrayList propertiesToLoad = new ArrayList();
            NativeComInterfaces.IAdsPathname pathname = null;
            pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            pathname.EscapedMode = 4;
            string str = "siteLinkList";
            propertiesToLoad.Add(str);
            ArrayList list2 = (ArrayList) Utils.GetValuesWithRangeRetrieval(this.cachedEntry, "(objectClass=*)", propertiesToLoad, 0)[str.ToLower(CultureInfo.InvariantCulture)];
            if (list2 != null)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    string bstrADsPath = (string) list2[i];
                    pathname.Set(bstrADsPath, 4);
                    string siteLinkName = pathname.Retrieve(11).Substring(3);
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, bstrADsPath);
                    ActiveDirectorySiteLink link = new ActiveDirectorySiteLink(this.context, siteLinkName, this.transport, true, directoryEntry);
                    this.links.Add(link);
                }
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
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            if (this.existing)
            {
                this.linksRetrieved = false;
            }
            else
            {
                this.existing = true;
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

        private static void ValidateArgument(DirectoryContext context, string bridgeName, ActiveDirectoryTransportType transport)
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
            if (bridgeName == null)
            {
                throw new ArgumentNullException("bridgeName");
            }
            if (bridgeName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "bridgeName");
            }
            if ((transport < ActiveDirectoryTransportType.Rpc) || (transport > ActiveDirectoryTransportType.Smtp))
            {
                throw new InvalidEnumArgumentException("value", (int) transport, typeof(ActiveDirectoryTransportType));
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

        public ActiveDirectorySiteLinkCollection SiteLinks
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.linksRetrieved)
                {
                    this.links.initialized = false;
                    this.links.Clear();
                    this.GetLinks();
                    this.linksRetrieved = true;
                }
                this.links.initialized = true;
                this.links.de = this.cachedEntry;
                this.links.context = this.context;
                return this.links;
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

