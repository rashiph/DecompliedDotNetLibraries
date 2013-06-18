namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ReplicationConnection : IDisposable
    {
        private const string ADAMGuid = "1.2.840.113556.1.4.1851";
        internal DirectoryEntry cachedDirectoryEntry;
        private bool checkADAM;
        private string connectionName;
        internal DirectoryContext context;
        private string destinationServerName;
        private bool disposed;
        internal bool existingConnection;
        private bool isADAMServer;
        private int options;
        private string sourceServerName;
        private ActiveDirectoryTransportType transport;

        internal ReplicationConnection(DirectoryContext context, DirectoryEntry connectionEntry, string name)
        {
            this.context = context;
            this.cachedDirectoryEntry = connectionEntry;
            this.connectionName = name;
            this.existingConnection = true;
        }

        public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer) : this(context, name, sourceServer, null, ActiveDirectoryTransportType.Rpc)
        {
        }

        public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectorySchedule schedule) : this(context, name, sourceServer, schedule, ActiveDirectoryTransportType.Rpc)
        {
        }

        public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectoryTransportType transport) : this(context, name, sourceServer, null, transport)
        {
        }

        public ReplicationConnection(DirectoryContext context, string name, DirectoryServer sourceServer, ActiveDirectorySchedule schedule, ActiveDirectoryTransportType transport)
        {
            ValidateArgument(context, name);
            if (sourceServer == null)
            {
                throw new ArgumentNullException("sourceServer");
            }
            if ((transport < ActiveDirectoryTransportType.Rpc) || (transport > ActiveDirectoryTransportType.Smtp))
            {
                throw new InvalidEnumArgumentException("value", (int) transport, typeof(ActiveDirectoryTransportType));
            }
            context = new DirectoryContext(context);
            this.ValidateTargetAndSourceServer(context, sourceServer);
            this.context = context;
            this.connectionName = name;
            this.transport = transport;
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
            try
            {
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ServerName);
                string dn = "CN=NTDS Settings," + str;
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dn);
                string escapedPath = Utils.GetEscapedPath("cn=" + this.connectionName);
                this.cachedDirectoryEntry = directoryEntry.Children.Add(escapedPath, "nTDSConnection");
                DirectoryContext context2 = sourceServer.Context;
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context2, WellKnownDN.RootDSE);
                string str4 = (string) PropertyManager.GetPropertyValue(context2, directoryEntry, PropertyManager.ServerName);
                str4 = "CN=NTDS Settings," + str4;
                this.cachedDirectoryEntry.Properties["fromServer"].Add(str4);
                if (schedule != null)
                {
                    this.cachedDirectoryEntry.Properties["schedule"].Value = schedule.GetUnmanagedSchedule();
                }
                string dNFromTransportType = Utils.GetDNFromTransportType(this.TransportType, context);
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dNFromTransportType);
                try
                {
                    directoryEntry.Bind(true);
                }
                catch (COMException exception)
                {
                    if (((exception.ErrorCode == -2147016656) && Utils.CheckCapability(DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE), Capability.ActiveDirectoryApplicationMode)) && (transport == ActiveDirectoryTransportType.Smtp))
                    {
                        throw new NotSupportedException(Res.GetString("NotSupportTransportSMTP"));
                    }
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                this.cachedDirectoryEntry.Properties["transportType"].Add(dNFromTransportType);
                this.cachedDirectoryEntry.Properties["enabledConnection"].Value = false;
                this.cachedDirectoryEntry.Properties["options"].Value = 0;
            }
            catch (COMException exception2)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception2);
            }
            finally
            {
                directoryEntry.Close();
            }
        }

        public void Delete()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.existingConnection)
            {
                throw new InvalidOperationException(Res.GetString("CannotDelete"));
            }
            try
            {
                this.cachedDirectoryEntry.Parent.Children.Remove(this.cachedDirectoryEntry);
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
            if (!this.disposed)
            {
                if (disposing && (this.cachedDirectoryEntry != null))
                {
                    this.cachedDirectoryEntry.Dispose();
                }
                this.disposed = true;
            }
        }

        ~ReplicationConnection()
        {
            this.Dispose(false);
        }

        public static ReplicationConnection FindByName(DirectoryContext context, string name)
        {
            ValidateArgument(context, name);
            context = new DirectoryContext(context);
            using (DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE))
            {
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ServerName);
                string dn = "CN=NTDS Settings," + str;
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dn);
                ADSearcher searcher = new ADSearcher(directoryEntry, "(&(objectClass=nTDSConnection)(objectCategory=NTDSConnection)(name=" + Utils.GetEscapedFilterValue(name) + "))", new string[] { "distinguishedName" }, SearchScope.OneLevel, false, false);
                SearchResult result = null;
                try
                {
                    result = searcher.FindOne();
                }
                catch (COMException exception)
                {
                    if (exception.ErrorCode == -2147016656)
                    {
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ReplicationConnection), name);
                    }
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if (result == null)
                {
                    Exception exception2 = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ReplicationConnection), name);
                    throw exception2;
                }
                return new ReplicationConnection(context, result.GetDirectoryEntry(), name);
            }
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.existingConnection)
            {
                throw new InvalidOperationException(Res.GetString("CannotGetObject"));
            }
            return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedDirectoryEntry.Path);
        }

        public void Save()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            try
            {
                this.cachedDirectoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            if (!this.existingConnection)
            {
                this.existingConnection = true;
            }
        }

        public override string ToString()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return this.Name;
        }

        private static void ValidateArgument(DirectoryContext context, string name)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) || !context.isServer())
            {
                throw new ArgumentException(Res.GetString("DirectoryContextNeedHost"));
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "name");
            }
        }

        private void ValidateTargetAndSourceServer(DirectoryContext context, DirectoryServer sourceServer)
        {
            bool flag = false;
            DirectoryEntry rootDSE = null;
            DirectoryEntry directoryEntry = null;
            rootDSE = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
            try
            {
                if (Utils.CheckCapability(rootDSE, Capability.ActiveDirectory))
                {
                    flag = true;
                }
                else if (!Utils.CheckCapability(rootDSE, Capability.ActiveDirectoryApplicationMode))
                {
                    throw new ArgumentException(Res.GetString("DirectoryContextNeedHost"), "context");
                }
                if (flag && !(sourceServer is DomainController))
                {
                    throw new ArgumentException(Res.GetString("ConnectionSourcServerShouldBeDC"), "sourceServer");
                }
                if (!flag && (sourceServer is DomainController))
                {
                    throw new ArgumentException(Res.GetString("ConnectionSourcServerShouldBeADAM"), "sourceServer");
                }
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(sourceServer.Context, WellKnownDN.RootDSE);
                if (flag)
                {
                    string str = (string) PropertyManager.GetPropertyValue(context, rootDSE, PropertyManager.RootDomainNamingContext);
                    string str2 = (string) PropertyManager.GetPropertyValue(sourceServer.Context, directoryEntry, PropertyManager.RootDomainNamingContext);
                    if (Utils.Compare(str, str2) != 0)
                    {
                        throw new ArgumentException(Res.GetString("ConnectionSourcServerSameForest"), "sourceServer");
                    }
                }
                else
                {
                    string str3 = (string) PropertyManager.GetPropertyValue(context, rootDSE, PropertyManager.ConfigurationNamingContext);
                    string str4 = (string) PropertyManager.GetPropertyValue(sourceServer.Context, directoryEntry, PropertyManager.ConfigurationNamingContext);
                    if (Utils.Compare(str3, str4) != 0)
                    {
                        throw new ArgumentException(Res.GetString("ConnectionSourcServerSameConfigSet"), "sourceServer");
                    }
                }
            }
            catch (COMException exception)
            {
                ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            finally
            {
                if (rootDSE != null)
                {
                    rootDSE.Close();
                }
                if (directoryEntry != null)
                {
                    directoryEntry.Close();
                }
            }
        }

        public NotificationStatus ChangeNotificationStatus
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedDirectoryEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count == 0)
                {
                    this.options = 0;
                }
                else
                {
                    this.options = (int) values[0];
                }
                int num = this.options & 4;
                int num2 = this.options & 8;
                if ((num == 4) && (num2 == 0))
                {
                    return NotificationStatus.NoNotification;
                }
                if ((num == 4) && (num2 == 8))
                {
                    return NotificationStatus.NotificationAlways;
                }
                return NotificationStatus.IntraSiteOnly;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if ((value < NotificationStatus.NoNotification) || (value > NotificationStatus.NotificationAlways))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(NotificationStatus));
                }
                try
                {
                    PropertyValueCollection values = this.cachedDirectoryEntry.Properties["options"];
                    if (values.Count == 0)
                    {
                        this.options = 0;
                    }
                    else
                    {
                        this.options = (int) values[0];
                    }
                    if (value == NotificationStatus.IntraSiteOnly)
                    {
                        this.options &= -5;
                        this.options &= -9;
                    }
                    else if (value == NotificationStatus.NoNotification)
                    {
                        this.options |= 4;
                        this.options &= -9;
                    }
                    else
                    {
                        this.options |= 4;
                        this.options |= 8;
                    }
                    this.cachedDirectoryEntry.Properties["options"].Value = this.options;
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
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedDirectoryEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count == 0)
                {
                    this.options = 0;
                }
                else
                {
                    this.options = (int) values[0];
                }
                return ((this.options & 0x10) == 0);
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    PropertyValueCollection values = this.cachedDirectoryEntry.Properties["options"];
                    if (values.Count == 0)
                    {
                        this.options = 0;
                    }
                    else
                    {
                        this.options = (int) values[0];
                    }
                    if (!value)
                    {
                        this.options |= 0x10;
                    }
                    else
                    {
                        this.options &= -17;
                    }
                    this.cachedDirectoryEntry.Properties["options"].Value = this.options;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public string DestinationServer
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.destinationServerName == null)
                {
                    DirectoryEntry directoryEntry = null;
                    DirectoryEntry parent = null;
                    try
                    {
                        directoryEntry = this.cachedDirectoryEntry.Parent;
                        parent = directoryEntry.Parent;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                    string str = (string) PropertyManager.GetPropertyValue(this.context, parent, PropertyManager.DnsHostName);
                    if (this.IsADAM)
                    {
                        int num = (int) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.MsDSPortLDAP);
                        if (num != 0x185)
                        {
                            this.destinationServerName = str + ":" + num;
                        }
                        else
                        {
                            this.destinationServerName = str;
                        }
                    }
                    else
                    {
                        this.destinationServerName = str;
                    }
                }
                return this.destinationServerName;
            }
        }

        public bool Enabled
        {
            get
            {
                bool flag;
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (this.cachedDirectoryEntry.Properties.Contains("enabledConnection"))
                    {
                        return (bool) this.cachedDirectoryEntry.Properties["enabledConnection"][0];
                    }
                    flag = false;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return flag;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    this.cachedDirectoryEntry.Properties["enabledConnection"].Value = value;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public bool GeneratedByKcc
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedDirectoryEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count == 0)
                {
                    this.options = 0;
                }
                else
                {
                    this.options = (int) values[0];
                }
                if ((this.options & 1) == 0)
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
                try
                {
                    PropertyValueCollection values = this.cachedDirectoryEntry.Properties["options"];
                    if (values.Count == 0)
                    {
                        this.options = 0;
                    }
                    else
                    {
                        this.options = (int) values[0];
                    }
                    if (value)
                    {
                        this.options |= 1;
                    }
                    else
                    {
                        this.options &= -2;
                    }
                    this.cachedDirectoryEntry.Properties["options"].Value = this.options;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
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
                    if (values.Contains("1.2.840.113556.1.4.1851"))
                    {
                        this.isADAMServer = true;
                    }
                }
                return this.isADAMServer;
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
                return this.connectionName;
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
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedDirectoryEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count == 0)
                {
                    this.options = 0;
                }
                else
                {
                    this.options = (int) values[0];
                }
                if ((this.options & 2) == 0)
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
                try
                {
                    PropertyValueCollection values = this.cachedDirectoryEntry.Properties["options"];
                    if (values.Count == 0)
                    {
                        this.options = 0;
                    }
                    else
                    {
                        this.options = (int) values[0];
                    }
                    if (value)
                    {
                        this.options |= 2;
                    }
                    else
                    {
                        this.options &= -3;
                    }
                    this.cachedDirectoryEntry.Properties["options"].Value = this.options;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public ActiveDirectorySchedule ReplicationSchedule
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                ActiveDirectorySchedule schedule = null;
                bool flag = false;
                try
                {
                    flag = this.cachedDirectoryEntry.Properties.Contains("schedule");
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (flag)
                {
                    byte[] unmanagedSchedule = (byte[]) this.cachedDirectoryEntry.Properties["schedule"][0];
                    schedule = new ActiveDirectorySchedule();
                    schedule.SetUnmanagedSchedule(unmanagedSchedule);
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
                        if (this.cachedDirectoryEntry.Properties.Contains("schedule"))
                        {
                            this.cachedDirectoryEntry.Properties["schedule"].Clear();
                        }
                    }
                    else
                    {
                        this.cachedDirectoryEntry.Properties["schedule"].Value = value.GetUnmanagedSchedule();
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public bool ReplicationScheduleOwnedByUser
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedDirectoryEntry.Properties["options"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count == 0)
                {
                    this.options = 0;
                }
                else
                {
                    this.options = (int) values[0];
                }
                if ((this.options & 0x20) == 0)
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
                try
                {
                    PropertyValueCollection values = this.cachedDirectoryEntry.Properties["options"];
                    if (values.Count == 0)
                    {
                        this.options = 0;
                    }
                    else
                    {
                        this.options = (int) values[0];
                    }
                    if (value)
                    {
                        this.options |= 0x20;
                    }
                    else
                    {
                        this.options &= -33;
                    }
                    this.cachedDirectoryEntry.Properties["options"].Value = this.options;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public System.DirectoryServices.ActiveDirectory.ReplicationSpan ReplicationSpan
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                string distinguishedName = (string) PropertyManager.GetPropertyValue(this.context, this.cachedDirectoryEntry, PropertyManager.FromServer);
                string str2 = Utils.GetDNComponents(distinguishedName)[3].Value;
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.RootDSE);
                string str3 = (string) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.ServerName);
                string str4 = Utils.GetDNComponents(str3)[2].Value;
                if (Utils.Compare(str2, str4) == 0)
                {
                    return System.DirectoryServices.ActiveDirectory.ReplicationSpan.IntraSite;
                }
                return System.DirectoryServices.ActiveDirectory.ReplicationSpan.InterSite;
            }
        }

        public string SourceServer
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if (this.sourceServerName == null)
                {
                    string dn = (string) PropertyManager.GetPropertyValue(this.context, this.cachedDirectoryEntry, PropertyManager.FromServer);
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, dn);
                    if (this.IsADAM)
                    {
                        int num = (int) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.MsDSPortLDAP);
                        string str2 = (string) PropertyManager.GetPropertyValue(this.context, directoryEntry.Parent, PropertyManager.DnsHostName);
                        if (num != 0x185)
                        {
                            this.sourceServerName = str2 + ":" + num;
                        }
                    }
                    else
                    {
                        this.sourceServerName = (string) PropertyManager.GetPropertyValue(this.context, directoryEntry.Parent, PropertyManager.DnsHostName);
                    }
                }
                return this.sourceServerName;
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
                if (!this.existingConnection)
                {
                    return this.transport;
                }
                PropertyValueCollection values = null;
                try
                {
                    values = this.cachedDirectoryEntry.Properties["transportType"];
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                if (values.Count == 0)
                {
                    return ActiveDirectoryTransportType.Rpc;
                }
                return Utils.GetTransportTypeFromDN((string) values[0]);
            }
        }
    }
}

