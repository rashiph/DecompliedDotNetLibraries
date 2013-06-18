namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class AdamInstance : DirectoryServer
    {
        private IntPtr ADAMHandle;
        private IntPtr authIdentity;
        private string[] becomeRoleOwnerAttrs;
        private string cachedDefaultPartition;
        private string cachedHostName;
        private int cachedLdapPort;
        private AdamRoleCollection cachedRoles;
        private int cachedSslPort;
        private System.DirectoryServices.ActiveDirectory.ConfigurationSet currentConfigSet;
        private bool defaultPartitionInitialized;
        private bool defaultPartitionModified;
        private bool disposed;
        private SyncReplicaFromAllServersCallback syncAllFunctionPointer;
        private SyncUpdateCallback userDelegate;

        internal AdamInstance(DirectoryContext context, string adamInstanceName) : this(context, adamInstanceName, new DirectoryEntryManager(context), true)
        {
        }

        internal AdamInstance(DirectoryContext context, string adamHostName, DirectoryEntryManager directoryEntryMgr)
        {
            string str;
            this.cachedLdapPort = -1;
            this.cachedSslPort = -1;
            this.ADAMHandle = IntPtr.Zero;
            this.authIdentity = IntPtr.Zero;
            base.context = context;
            base.replicaName = adamHostName;
            Utils.SplitServerNameAndPortNumber(context.Name, out str);
            if (str != null)
            {
                base.replicaName = base.replicaName + ":" + str;
            }
            base.directoryEntryMgr = directoryEntryMgr;
            this.becomeRoleOwnerAttrs = new string[] { PropertyManager.BecomeSchemaMaster, PropertyManager.BecomeDomainMaster };
            this.syncAllFunctionPointer = new SyncReplicaFromAllServersCallback(this.SyncAllCallbackRoutine);
        }

        internal AdamInstance(DirectoryContext context, string adamInstanceName, DirectoryEntryManager directoryEntryMgr, bool nameIncludesPort)
        {
            this.cachedLdapPort = -1;
            this.cachedSslPort = -1;
            this.ADAMHandle = IntPtr.Zero;
            this.authIdentity = IntPtr.Zero;
            base.context = context;
            base.replicaName = adamInstanceName;
            base.directoryEntryMgr = directoryEntryMgr;
            this.becomeRoleOwnerAttrs = new string[] { PropertyManager.BecomeSchemaMaster, PropertyManager.BecomeDomainMaster };
            this.syncAllFunctionPointer = new SyncReplicaFromAllServersCallback(this.SyncAllCallbackRoutine);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void CheckReplicationConsistency()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetADAMHandle();
            base.CheckConsistencyHelper(this.ADAMHandle, DirectoryContext.ADAMHandle);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    this.FreeADAMHandle();
                    this.disposed = true;
                }
                finally
                {
                    base.Dispose();
                }
            }
        }

        ~AdamInstance()
        {
            this.Dispose(false);
        }

        public static AdamInstanceCollection FindAll(DirectoryContext context, string partitionName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.ConfigurationSet)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeConfigSet"), "context");
            }
            if (partitionName == null)
            {
                throw new ArgumentNullException("partitionName");
            }
            if (partitionName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
            }
            context = new DirectoryContext(context);
            try
            {
                return System.DirectoryServices.ActiveDirectory.ConfigurationSet.FindAdamInstances(context, partitionName, null);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                return new AdamInstanceCollection(new ArrayList());
            }
        }

        public static AdamInstance FindOne(DirectoryContext context, string partitionName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.ConfigurationSet)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeConfigSet"), "context");
            }
            if (partitionName == null)
            {
                throw new ArgumentNullException("partitionName");
            }
            if (partitionName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partitionName");
            }
            context = new DirectoryContext(context);
            return System.DirectoryServices.ActiveDirectory.ConfigurationSet.FindOneAdamInstance(context, partitionName, null);
        }

        private void FreeADAMHandle()
        {
            lock (this)
            {
                Utils.FreeDSHandle(this.ADAMHandle, DirectoryContext.ADAMHandle);
                Utils.FreeAuthIdentity(this.authIdentity, DirectoryContext.ADAMHandle);
            }
        }

        private void GetADAMHandle()
        {
            lock (this)
            {
                if (this.ADAMHandle == IntPtr.Zero)
                {
                    if (this.authIdentity == IntPtr.Zero)
                    {
                        this.authIdentity = Utils.GetAuthIdentity(base.context, DirectoryContext.ADAMHandle);
                    }
                    string domainControllerName = this.HostName + ":" + this.LdapPort;
                    this.ADAMHandle = Utils.GetDSHandle(domainControllerName, null, this.authIdentity, DirectoryContext.ADAMHandle);
                }
            }
        }

        public static AdamInstance GetAdamInstance(DirectoryContext context)
        {
            DirectoryEntryManager directoryEntryMgr = null;
            string adamHostName = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.DirectoryServer)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeADAMServer"), "context");
            }
            if (!context.isServer())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", new object[] { context.Name }), typeof(AdamInstance), context.Name);
            }
            context = new DirectoryContext(context);
            try
            {
                directoryEntryMgr = new DirectoryEntryManager(context);
                DirectoryEntry cachedDirectoryEntry = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (!Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryApplicationMode))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", new object[] { context.Name }), typeof(AdamInstance), context.Name);
                }
                adamHostName = (string) PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.DnsHostName);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016646)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AINotFound", new object[] { context.Name }), typeof(AdamInstance), context.Name);
                }
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            return new AdamInstance(context, adamHostName, directoryEntryMgr);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ReplicationNeighborCollection GetAllReplicationNeighbors()
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetADAMHandle();
            zero = base.GetReplicationInfoHelper(this.ADAMHandle, 0, 0, null, ref advanced, 0, DirectoryContext.ADAMHandle);
            return base.ConstructNeighbors(zero, this, DirectoryContext.ADAMHandle);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override ReplicationFailureCollection GetReplicationConnectionFailures()
        {
            return this.GetReplicationFailures(DS_REPL_INFO_TYPE.DS_REPL_INFO_KCC_DSA_CONNECT_FAILURES);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ReplicationCursorCollection GetReplicationCursors(string partition)
        {
            IntPtr zero = IntPtr.Zero;
            int context = 0;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetADAMHandle();
            zero = base.GetReplicationInfoHelper(this.ADAMHandle, 8, 1, partition, ref advanced, context, DirectoryContext.ADAMHandle);
            return base.ConstructReplicationCursors(this.ADAMHandle, advanced, zero, partition, this, DirectoryContext.ADAMHandle);
        }

        private ReplicationFailureCollection GetReplicationFailures(DS_REPL_INFO_TYPE type)
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetADAMHandle();
            zero = base.GetReplicationInfoHelper(this.ADAMHandle, (int) type, (int) type, null, ref advanced, 0, DirectoryContext.ADAMHandle);
            return base.ConstructFailures(zero, this, DirectoryContext.ADAMHandle);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ActiveDirectoryReplicationMetadata GetReplicationMetadata(string objectPath)
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (objectPath == null)
            {
                throw new ArgumentNullException("objectPath");
            }
            if (objectPath.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "objectPath");
            }
            this.GetADAMHandle();
            zero = base.GetReplicationInfoHelper(this.ADAMHandle, 9, 2, objectPath, ref advanced, 0, DirectoryContext.ADAMHandle);
            return base.ConstructMetaData(advanced, zero, this, DirectoryContext.ADAMHandle);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override ReplicationNeighborCollection GetReplicationNeighbors(string partition)
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetADAMHandle();
            zero = base.GetReplicationInfoHelper(this.ADAMHandle, 0, 0, partition, ref advanced, 0, DirectoryContext.ADAMHandle);
            return base.ConstructNeighbors(zero, this, DirectoryContext.ADAMHandle);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override ReplicationOperationInformation GetReplicationOperationInformation()
        {
            IntPtr zero = IntPtr.Zero;
            bool advanced = true;
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            this.GetADAMHandle();
            zero = base.GetReplicationInfoHelper(this.ADAMHandle, 5, 5, null, ref advanced, 0, DirectoryContext.ADAMHandle);
            return base.ConstructPendingOperations(zero, this, DirectoryContext.ADAMHandle);
        }

        public void Save()
        {
            base.CheckIfDisposed();
            if (this.defaultPartitionModified)
            {
                DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                try
                {
                    cachedDirectoryEntry.CommitChanges();
                }
                catch (COMException exception)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                }
            }
            this.defaultPartitionInitialized = false;
            this.defaultPartitionModified = false;
        }

        public void SeizeRoleOwnership(AdamRole role)
        {
            string dn = null;
            base.CheckIfDisposed();
            switch (role)
            {
                case AdamRole.SchemaRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
                    break;

                case AdamRole.NamingRole:
                    dn = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer);
                    break;

                default:
                    throw new InvalidEnumArgumentException("role", (int) role, typeof(AdamRole));
            }
            DirectoryEntry directoryEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, dn);
                directoryEntry.Properties[PropertyManager.FsmoRoleOwner].Value = this.NtdsaObjectName;
                directoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            this.cachedRoles = null;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void SyncReplicaFromAllServers(string partition, SyncFromAllServersOptions options)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetADAMHandle();
            base.SyncReplicaAllHelper(this.ADAMHandle, this.syncAllFunctionPointer, partition, options, this.SyncFromAllServersCallback, DirectoryContext.ADAMHandle);
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override void SyncReplicaFromServer(string partition, string sourceServer)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            if (sourceServer == null)
            {
                throw new ArgumentNullException("sourceServer");
            }
            if (sourceServer.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "sourceServer");
            }
            this.GetADAMHandle();
            base.SyncReplicaHelper(this.ADAMHandle, true, partition, sourceServer, 0, DirectoryContext.ADAMHandle);
        }

        public void TransferRoleOwnership(AdamRole role)
        {
            base.CheckIfDisposed();
            if ((role < AdamRole.SchemaRole) || (role > AdamRole.NamingRole))
            {
                throw new InvalidEnumArgumentException("role", (int) role, typeof(AdamRole));
            }
            try
            {
                DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                cachedDirectoryEntry.Properties[this.becomeRoleOwnerAttrs[(int) role]].Value = 1;
                cachedDirectoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            this.cachedRoles = null;
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override void TriggerSyncReplicaFromNeighbors(string partition)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (partition == null)
            {
                throw new ArgumentNullException("partition");
            }
            if (partition.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "partition");
            }
            this.GetADAMHandle();
            base.SyncReplicaHelper(this.ADAMHandle, true, partition, null, 0x11, DirectoryContext.ADAMHandle);
        }

        public System.DirectoryServices.ActiveDirectory.ConfigurationSet ConfigurationSet
        {
            get
            {
                base.CheckIfDisposed();
                if (this.currentConfigSet == null)
                {
                    DirectoryContext context = Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, base.context);
                    this.currentConfigSet = System.DirectoryServices.ActiveDirectory.ConfigurationSet.GetConfigurationSet(context);
                }
                return this.currentConfigSet;
            }
        }

        public string DefaultPartition
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.defaultPartitionInitialized || this.defaultPartitionModified)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                    try
                    {
                        cachedDirectoryEntry.RefreshCache();
                        if (cachedDirectoryEntry.Properties[PropertyManager.MsDSDefaultNamingContext].Value == null)
                        {
                            this.cachedDefaultPartition = null;
                        }
                        else
                        {
                            this.cachedDefaultPartition = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.MsDSDefaultNamingContext);
                        }
                    }
                    catch (COMException exception)
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                    }
                    this.defaultPartitionInitialized = true;
                }
                return this.cachedDefaultPartition;
            }
            set
            {
                base.CheckIfDisposed();
                DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                if (value == null)
                {
                    if (cachedDirectoryEntry.Properties.Contains(PropertyManager.MsDSDefaultNamingContext))
                    {
                        cachedDirectoryEntry.Properties[PropertyManager.MsDSDefaultNamingContext].Clear();
                    }
                }
                else
                {
                    if (!Utils.IsValidDNFormat(value))
                    {
                        throw new ArgumentException(Res.GetString("InvalidDNFormat"), "value");
                    }
                    if (!base.Partitions.Contains(value))
                    {
                        throw new ArgumentException(Res.GetString("ServerNotAReplica", new object[] { value }), "value");
                    }
                    cachedDirectoryEntry.Properties[PropertyManager.MsDSDefaultNamingContext].Value = value;
                }
                this.defaultPartitionModified = true;
            }
        }

        public string HostName
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedHostName == null)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.ServerObjectName);
                    this.cachedHostName = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.DnsHostName);
                }
                return this.cachedHostName;
            }
        }

        public override ReplicationConnectionCollection InboundConnections
        {
            [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            get
            {
                return base.GetInboundConnectionsHelper();
            }
        }

        public override string IPAddress
        {
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DnsPermission(SecurityAction.Assert, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            get
            {
                base.CheckIfDisposed();
                IPHostEntry hostEntry = Dns.GetHostEntry(this.HostName);
                if (hostEntry.AddressList.GetLength(0) > 0)
                {
                    return hostEntry.AddressList[0].ToString();
                }
                return null;
            }
        }

        public int LdapPort
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedLdapPort == -1)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                    this.cachedLdapPort = (int) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.MsDSPortLDAP);
                }
                return this.cachedLdapPort;
            }
        }

        internal Guid NtdsaObjectGuid
        {
            get
            {
                base.CheckIfDisposed();
                if (base.cachedNtdsaObjectGuid == Guid.Empty)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                    byte[] b = (byte[]) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.ObjectGuid);
                    base.cachedNtdsaObjectGuid = new Guid(b);
                }
                return base.cachedNtdsaObjectGuid;
            }
        }

        internal string NtdsaObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (base.cachedNtdsaObjectName == null)
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, WellKnownDN.RootDSE);
                    try
                    {
                        base.cachedNtdsaObjectName = (string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.DsServiceName);
                    }
                    catch (COMException exception)
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                    }
                    finally
                    {
                        directoryEntry.Dispose();
                    }
                }
                return base.cachedNtdsaObjectName;
            }
        }

        public override ReplicationConnectionCollection OutboundConnections
        {
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            get
            {
                return base.GetOutboundConnectionsHelper();
            }
        }

        public AdamRoleCollection Roles
        {
            get
            {
                base.CheckIfDisposed();
                DirectoryEntry directoryEntry = null;
                DirectoryEntry entry2 = null;
                try
                {
                    if (this.cachedRoles == null)
                    {
                        ArrayList values = new ArrayList();
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext));
                        if (this.NtdsaObjectName.Equals((string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.FsmoRoleOwner)))
                        {
                            values.Add(AdamRole.SchemaRole);
                        }
                        entry2 = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
                        if (this.NtdsaObjectName.Equals((string) PropertyManager.GetPropertyValue(base.context, entry2, PropertyManager.FsmoRoleOwner)))
                        {
                            values.Add(AdamRole.NamingRole);
                        }
                        this.cachedRoles = new AdamRoleCollection(values);
                    }
                }
                catch (COMException exception)
                {
                    throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                }
                finally
                {
                    if (directoryEntry != null)
                    {
                        directoryEntry.Dispose();
                    }
                    if (entry2 != null)
                    {
                        entry2.Dispose();
                    }
                }
                return this.cachedRoles;
            }
        }

        internal string ServerObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (base.cachedServerObjectName == null)
                {
                    DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, WellKnownDN.RootDSE);
                    try
                    {
                        base.cachedServerObjectName = (string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.ServerName);
                    }
                    catch (COMException exception)
                    {
                        throw System.DirectoryServices.ActiveDirectory.ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                    }
                    finally
                    {
                        directoryEntry.Dispose();
                    }
                }
                return base.cachedServerObjectName;
            }
        }

        public override string SiteName
        {
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            get
            {
                base.CheckIfDisposed();
                if (base.cachedSiteName == null)
                {
                    using (DirectoryEntry entry = DirectoryEntryManager.GetDirectoryEntry(base.context, this.SiteObjectName))
                    {
                        base.cachedSiteName = (string) PropertyManager.GetPropertyValue(base.context, entry, PropertyManager.Cn);
                    }
                }
                return base.cachedSiteName;
            }
        }

        internal string SiteObjectName
        {
            get
            {
                base.CheckIfDisposed();
                if (base.cachedSiteObjectName == null)
                {
                    string[] strArray = this.ServerObjectName.Split(new char[] { ',' });
                    if (strArray.GetLength(0) < 3)
                    {
                        throw new ActiveDirectoryOperationException(Res.GetString("InvalidServerNameFormat"));
                    }
                    base.cachedSiteObjectName = strArray[2];
                    for (int i = 3; i < strArray.GetLength(0); i++)
                    {
                        base.cachedSiteObjectName = base.cachedSiteObjectName + "," + strArray[i];
                    }
                }
                return base.cachedSiteObjectName;
            }
        }

        public int SslPort
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedSslPort == -1)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(this.NtdsaObjectName);
                    this.cachedSslPort = (int) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.MsDSPortSSL);
                }
                return this.cachedSslPort;
            }
        }

        public override SyncUpdateCallback SyncFromAllServersCallback
        {
            [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.userDelegate;
            }
            [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                this.userDelegate = value;
            }
        }
    }
}

