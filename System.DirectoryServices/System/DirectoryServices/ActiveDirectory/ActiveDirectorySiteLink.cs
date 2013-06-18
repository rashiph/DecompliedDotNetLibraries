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
    public class ActiveDirectorySiteLink : IDisposable
    {
        private const int appDefaultCost = 100;
        private const int appDefaultInterval = 180;
        internal DirectoryEntry cachedEntry;
        internal DirectoryContext context;
        private bool disposed;
        internal bool existing;
        private string name;
        private bool siteRetrieved;
        private ActiveDirectorySiteCollection sites;
        private const int systemDefaultCost = 0;
        private TimeSpan systemDefaultInterval;
        private ActiveDirectoryTransportType transport;

        public ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName) : this(context, siteLinkName, ActiveDirectoryTransportType.Rpc, null)
        {
        }

        public ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport) : this(context, siteLinkName, transport, null)
        {
        }

        public ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport, ActiveDirectorySchedule schedule)
        {
            DirectoryEntry directoryEntry;
            this.systemDefaultInterval = new TimeSpan(0, 15, 0);
            this.sites = new ActiveDirectorySiteCollection();
            ValidateArgument(context, siteLinkName, transport);
            context = new DirectoryContext(context);
            this.context = context;
            this.name = siteLinkName;
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
                this.cachedEntry = directoryEntry.Children.Add(escapedPath, "siteLink");
                this.cachedEntry.Properties["cost"].Value = 100;
                this.cachedEntry.Properties["replInterval"].Value = 180;
                if (schedule != null)
                {
                    this.cachedEntry.Properties["schedule"].Value = schedule.GetUnmanagedSchedule();
                }
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

        internal ActiveDirectorySiteLink(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport, bool existing, DirectoryEntry entry)
        {
            this.systemDefaultInterval = new TimeSpan(0, 15, 0);
            this.sites = new ActiveDirectorySiteCollection();
            this.context = context;
            this.name = siteLinkName;
            this.transport = transport;
            this.existing = existing;
            this.cachedEntry = entry;
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

        public static ActiveDirectorySiteLink FindByName(DirectoryContext context, string siteLinkName)
        {
            return FindByName(context, siteLinkName, ActiveDirectoryTransportType.Rpc);
        }

        public static ActiveDirectorySiteLink FindByName(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport)
        {
            DirectoryEntry directoryEntry;
            ActiveDirectorySiteLink link2;
            ValidateArgument(context, siteLinkName, transport);
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
                SearchResult result = new ADSearcher(directoryEntry, "(&(objectClass=siteLink)(objectCategory=SiteLink)(name=" + Utils.GetEscapedFilterValue(siteLinkName) + "))", new string[] { "distinguishedName" }, SearchScope.OneLevel, false, false).FindOne();
                if (result == null)
                {
                    Exception exception2 = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLink), siteLinkName);
                    throw exception2;
                }
                DirectoryEntry entry = result.GetDirectoryEntry();
                link2 = new ActiveDirectorySiteLink(context, siteLinkName, transport, true, entry);
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
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySiteLink), siteLinkName);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            return link2;
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

        private void GetSites()
        {
            NativeComInterfaces.IAdsPathname pathname = null;
            pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            ArrayList propertiesToLoad = new ArrayList();
            pathname.EscapedMode = 4;
            string str = "siteList";
            propertiesToLoad.Add(str);
            ArrayList list2 = (ArrayList) Utils.GetValuesWithRangeRetrieval(this.cachedEntry, "(objectClass=*)", propertiesToLoad, 0)[str.ToLower(CultureInfo.InvariantCulture)];
            if (list2 != null)
            {
                for (int i = 0; i < list2.Count; i++)
                {
                    string bstrADsPath = (string) list2[i];
                    pathname.Set(bstrADsPath, 4);
                    string siteName = pathname.Retrieve(11).Substring(3);
                    ActiveDirectorySite site = new ActiveDirectorySite(this.context, siteName, true);
                    this.sites.Add(site);
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
                this.siteRetrieved = false;
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

        private static void ValidateArgument(DirectoryContext context, string siteLinkName, ActiveDirectoryTransportType transport)
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
            if (siteLinkName == null)
            {
                throw new ArgumentNullException("siteLinkName");
            }
            if (siteLinkName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteLinkName");
            }
            if ((transport < ActiveDirectoryTransportType.Rpc) || (transport > ActiveDirectoryTransportType.Smtp))
            {
                throw new InvalidEnumArgumentException("value", (int) transport, typeof(ActiveDirectoryTransportType));
            }
        }

        public int Cost
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (this.cachedEntry.Properties.Contains("cost"))
                    {
                        return (int) this.cachedEntry.Properties["cost"][0];
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return 0;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (value < 0)
                {
                    throw new ArgumentException("value");
                }
                try
                {
                    this.cachedEntry.Properties["cost"].Value = value;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public bool DataCompressionEnabled
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count != 0)
                {
                    num = (int) values[0];
                }
                return ((num & 4) == 0);
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedEntry.Properties["options"];
                    if (values.Count != 0)
                    {
                        num = (int) values[0];
                    }
                    if (!value)
                    {
                        num |= 4;
                    }
                    else
                    {
                        num &= -5;
                    }
                    this.cachedEntry.Properties["options"].Value = num;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public ActiveDirectorySchedule InterSiteReplicationSchedule
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                ActiveDirectorySchedule schedule = null;
                try
                {
                    if (this.cachedEntry.Properties.Contains("schedule"))
                    {
                        byte[] unmanagedSchedule = (byte[]) this.cachedEntry.Properties["schedule"][0];
                        schedule = new ActiveDirectorySchedule();
                        schedule.SetUnmanagedSchedule(unmanagedSchedule);
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return schedule;
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
                        if (this.cachedEntry.Properties.Contains("schedule"))
                        {
                            this.cachedEntry.Properties["schedule"].Clear();
                        }
                    }
                    else
                    {
                        this.cachedEntry.Properties["schedule"].Value = value.GetUnmanagedSchedule();
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

        public bool NotificationEnabled
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count != 0)
                {
                    num = (int) values[0];
                }
                if ((num & 1) == 0)
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
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedEntry.Properties["options"];
                    if (values.Count != 0)
                    {
                        num = (int) values[0];
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

        public bool ReciprocalReplicationEnabled
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                int num = 0;
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count != 0)
                {
                    num = (int) values[0];
                }
                if ((num & 2) == 0)
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
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedEntry.Properties["options"];
                    if (values.Count != 0)
                    {
                        num = (int) values[0];
                    }
                    if (value)
                    {
                        num |= 2;
                    }
                    else
                    {
                        num &= -3;
                    }
                    this.cachedEntry.Properties["options"].Value = num;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public TimeSpan ReplicationInterval
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (this.cachedEntry.Properties.Contains("replInterval"))
                    {
                        return new TimeSpan(0, (int) this.cachedEntry.Properties["replInterval"][0], 0);
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return this.systemDefaultInterval;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (value < TimeSpan.Zero)
                {
                    throw new ArgumentException(Res.GetString("NoNegativeTime"), "value");
                }
                double totalMinutes = value.TotalMinutes;
                if (totalMinutes > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("ReplicationIntervalExceedMax"), "value");
                }
                int num2 = (int) totalMinutes;
                if (num2 < totalMinutes)
                {
                    throw new ArgumentException(Res.GetString("ReplicationIntervalInMinutes"), "value");
                }
                try
                {
                    this.cachedEntry.Properties["replInterval"].Value = num2;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public ActiveDirectorySiteCollection Sites
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.existing && !this.siteRetrieved)
                {
                    this.sites.initialized = false;
                    this.sites.Clear();
                    this.GetSites();
                    this.siteRetrieved = true;
                }
                this.sites.initialized = true;
                this.sites.de = this.cachedEntry;
                this.sites.context = this.context;
                return this.sites;
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

