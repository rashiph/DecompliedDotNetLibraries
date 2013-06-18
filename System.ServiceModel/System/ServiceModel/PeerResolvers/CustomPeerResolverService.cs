namespace System.ServiceModel.PeerResolvers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    [ServiceBehavior(UseSynchronizationContext=false, InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    public class CustomPeerResolverService : IPeerResolverContract
    {
        private TimeSpan cleanupInterval = TimeSpan.FromMinutes(1.0);
        private bool controlShape;
        private ReaderWriterLock gate = new ReaderWriterLock();
        private bool isCleaning = false;
        private TimeSpan LockWait = TimeSpan.FromSeconds(5.0);
        private Dictionary<string, MeshEntry> meshId2Entry = new Dictionary<string, MeshEntry>();
        private bool opened;
        private TimeSpan refreshInterval = TimeSpan.FromMinutes(10.0);
        private object thisLock = new object();
        private TimeSpan timeout = TimeSpan.FromMinutes(1.0);
        private IOThreadTimer timer;

        internal virtual void CleanupActivity(object state)
        {
            if (this.opened && !this.isCleaning)
            {
                lock (this.ThisLock)
                {
                    if (!this.isCleaning)
                    {
                        this.isCleaning = true;
                        try
                        {
                            MeshEntry meshEntry = null;
                            ICollection<string> keys = null;
                            LiteLock liteLock = null;
                            try
                            {
                                LiteLock.Acquire(out liteLock, this.gate);
                                keys = this.meshId2Entry.Keys;
                            }
                            finally
                            {
                                LiteLock.Release(liteLock);
                            }
                            foreach (string str in keys)
                            {
                                meshEntry = this.GetMeshEntry(str);
                                this.CleanupMeshEntry(meshEntry);
                            }
                        }
                        finally
                        {
                            this.isCleaning = false;
                            if (this.opened)
                            {
                                this.timer.Set(this.CleanupInterval);
                            }
                        }
                    }
                }
            }
        }

        private void CleanupMeshEntry(MeshEntry meshEntry)
        {
            List<Guid> list = new List<Guid>();
            if (this.opened)
            {
                LiteLock liteLock = null;
                try
                {
                    LiteLock.Acquire(out liteLock, meshEntry.Gate, true);
                    foreach (KeyValuePair<Guid, RegistrationEntry> pair in meshEntry.EntryTable)
                    {
                        if ((pair.Value.Expires <= DateTime.UtcNow) || (pair.Value.State == RegistrationState.Deleted))
                        {
                            list.Add(pair.Key);
                            meshEntry.EntryList.Remove(pair.Value);
                            meshEntry.Service2EntryTable.Remove(pair.Value.Address.ServicePath);
                        }
                    }
                    foreach (Guid guid in list)
                    {
                        meshEntry.EntryTable.Remove(guid);
                    }
                }
                finally
                {
                    LiteLock.Release(liteLock);
                }
            }
        }

        public virtual void Close()
        {
            this.ThrowIfClosed("Close");
            this.timer.Cancel();
            this.opened = false;
        }

        private MeshEntry GetMeshEntry(string meshId)
        {
            return this.GetMeshEntry(meshId, true);
        }

        private MeshEntry GetMeshEntry(string meshId, bool createIfNotExists)
        {
            MeshEntry entry = null;
            LiteLock liteLock = null;
            try
            {
                LiteLock.Acquire(out liteLock, this.gate);
                if (this.meshId2Entry.TryGetValue(meshId, out entry) || !createIfNotExists)
                {
                    return entry;
                }
                entry = new MeshEntry();
                try
                {
                    liteLock.UpgradeToWriterLock();
                    this.meshId2Entry.Add(meshId, entry);
                }
                finally
                {
                    liteLock.DowngradeFromWriterLock();
                }
            }
            finally
            {
                LiteLock.Release(liteLock);
            }
            return entry;
        }

        public virtual ServiceSettingsResponseInfo GetServiceSettings()
        {
            this.ThrowIfClosed("GetServiceSettings");
            return new ServiceSettingsResponseInfo(this.ControlShape);
        }

        public virtual void Open()
        {
            this.ThrowIfOpened("Open");
            if (this.refreshInterval <= TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("RefreshInterval", System.ServiceModel.SR.GetString("RefreshIntervalMustBeGreaterThanZero", new object[] { this.refreshInterval }));
            }
            if (this.CleanupInterval <= TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("CleanupInterval", System.ServiceModel.SR.GetString("CleanupIntervalMustBeGreaterThanZero", new object[] { this.cleanupInterval }));
            }
            this.timer = new IOThreadTimer(new Action<object>(this.CleanupActivity), null, false);
            this.timer.Set(this.CleanupInterval);
            this.opened = true;
        }

        public virtual RefreshResponseInfo Refresh(RefreshInfo refreshInfo)
        {
            if (refreshInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("refreshInfo", System.ServiceModel.SR.GetString("PeerNullRefreshInfo"));
            }
            this.ThrowIfClosed("Refresh");
            if ((!refreshInfo.HasBody() || string.IsNullOrEmpty(refreshInfo.MeshId)) || (refreshInfo.RegistrationId == Guid.Empty))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("refreshInfo", System.ServiceModel.SR.GetString("PeerInvalidMessageBody", new object[] { refreshInfo }));
            }
            RefreshResult registrationNotFound = RefreshResult.RegistrationNotFound;
            RegistrationEntry entry = null;
            MeshEntry meshEntry = this.GetMeshEntry(refreshInfo.MeshId, false);
            LiteLock liteLock = null;
            if (meshEntry != null)
            {
                try
                {
                    LiteLock.Acquire(out liteLock, meshEntry.Gate);
                    if (!meshEntry.EntryTable.TryGetValue(refreshInfo.RegistrationId, out entry))
                    {
                        return new RefreshResponseInfo(this.RefreshInterval, registrationNotFound);
                    }
                    lock (entry)
                    {
                        if (entry.State == RegistrationState.OK)
                        {
                            entry.Expires = DateTime.UtcNow + this.RefreshInterval;
                            registrationNotFound = RefreshResult.Success;
                        }
                    }
                }
                finally
                {
                    LiteLock.Release(liteLock);
                }
            }
            return new RefreshResponseInfo(this.RefreshInterval, registrationNotFound);
        }

        public virtual RegisterResponseInfo Register(RegisterInfo registerInfo)
        {
            if (registerInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("registerInfo", System.ServiceModel.SR.GetString("PeerNullRegistrationInfo"));
            }
            this.ThrowIfClosed("Register");
            if (!registerInfo.HasBody() || string.IsNullOrEmpty(registerInfo.MeshId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("registerInfo", System.ServiceModel.SR.GetString("PeerInvalidMessageBody", new object[] { registerInfo }));
            }
            return this.Register(registerInfo.ClientId, registerInfo.MeshId, registerInfo.NodeAddress);
        }

        public virtual RegisterResponseInfo Register(Guid clientId, string meshId, PeerNodeAddress address)
        {
            Guid registrationId = Guid.NewGuid();
            DateTime expires = DateTime.UtcNow + this.RefreshInterval;
            RegistrationEntry entry = null;
            MeshEntry meshEntry = null;
            lock (this.ThisLock)
            {
                entry = new RegistrationEntry(clientId, registrationId, meshId, expires, address);
                meshEntry = this.GetMeshEntry(meshId);
                if (meshEntry.Service2EntryTable.ContainsKey(address.ServicePath))
                {
                    PeerExceptionHelper.ThrowInvalidOperation_DuplicatePeerRegistration(address.ServicePath);
                }
                LiteLock liteLock = null;
                try
                {
                    LiteLock.Acquire(out liteLock, meshEntry.Gate, true);
                    meshEntry.EntryTable.Add(registrationId, entry);
                    meshEntry.EntryList.Add(entry);
                    meshEntry.Service2EntryTable.Add(address.ServicePath, entry);
                }
                finally
                {
                    LiteLock.Release(liteLock);
                }
            }
            return new RegisterResponseInfo(registrationId, this.RefreshInterval);
        }

        public virtual ResolveResponseInfo Resolve(ResolveInfo resolveInfo)
        {
            if (resolveInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("resolveInfo", System.ServiceModel.SR.GetString("PeerNullResolveInfo"));
            }
            this.ThrowIfClosed("Resolve");
            if (!resolveInfo.HasBody())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("resolveInfo", System.ServiceModel.SR.GetString("PeerInvalidMessageBody", new object[] { resolveInfo }));
            }
            int num = 0;
            int num2 = 0;
            int maxAddresses = resolveInfo.MaxAddresses;
            ResolveResponseInfo info = new ResolveResponseInfo();
            List<PeerNodeAddress> list = new List<PeerNodeAddress>();
            List<RegistrationEntry> entryList = null;
            MeshEntry meshEntry = this.GetMeshEntry(resolveInfo.MeshId, false);
            if (meshEntry != null)
            {
                LiteLock liteLock = null;
                try
                {
                    LiteLock.Acquire(out liteLock, meshEntry.Gate);
                    entryList = meshEntry.EntryList;
                    if (entryList.Count <= maxAddresses)
                    {
                        foreach (RegistrationEntry entry3 in entryList)
                        {
                            list.Add(entry3.Address);
                        }
                    }
                    else
                    {
                        Random random = new Random();
                        while (num < maxAddresses)
                        {
                            num2 = random.Next(entryList.Count);
                            RegistrationEntry entry = entryList[num2];
                            PeerNodeAddress item = entry.Address;
                            if (!list.Contains(item))
                            {
                                list.Add(item);
                            }
                            num++;
                        }
                    }
                }
                finally
                {
                    LiteLock.Release(liteLock);
                }
            }
            info.Addresses = list.ToArray();
            return info;
        }

        private void ThrowIfClosed(string operation)
        {
            if (!this.opened)
            {
                PeerExceptionHelper.ThrowInvalidOperation_NotValidWhenClosed(operation);
            }
        }

        private void ThrowIfOpened(string operation)
        {
            if (this.opened)
            {
                PeerExceptionHelper.ThrowInvalidOperation_NotValidWhenOpen(operation);
            }
        }

        public virtual void Unregister(UnregisterInfo unregisterInfo)
        {
            if (unregisterInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("unregisterinfo", System.ServiceModel.SR.GetString("PeerNullRegistrationInfo"));
            }
            this.ThrowIfClosed("Unregister");
            if ((!unregisterInfo.HasBody() || string.IsNullOrEmpty(unregisterInfo.MeshId)) || (unregisterInfo.RegistrationId == Guid.Empty))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("unregisterInfo", System.ServiceModel.SR.GetString("PeerInvalidMessageBody", new object[] { unregisterInfo }));
            }
            RegistrationEntry entry = null;
            MeshEntry meshEntry = this.GetMeshEntry(unregisterInfo.MeshId, false);
            LiteLock liteLock = null;
            try
            {
                LiteLock.Acquire(out liteLock, meshEntry.Gate, true);
                if (!meshEntry.EntryTable.TryGetValue(unregisterInfo.RegistrationId, out entry))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("unregisterInfo", System.ServiceModel.SR.GetString("PeerInvalidMessageBody", new object[] { unregisterInfo }));
                }
                meshEntry.EntryTable.Remove(unregisterInfo.RegistrationId);
                meshEntry.EntryList.Remove(entry);
                meshEntry.Service2EntryTable.Remove(entry.Address.ServicePath);
                entry.State = RegistrationState.Deleted;
            }
            finally
            {
                LiteLock.Release(liteLock);
            }
        }

        public virtual RegisterResponseInfo Update(UpdateInfo updateInfo)
        {
            if (updateInfo == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("updateInfo", System.ServiceModel.SR.GetString("PeerNullRegistrationInfo"));
            }
            this.ThrowIfClosed("Update");
            if ((!updateInfo.HasBody() || string.IsNullOrEmpty(updateInfo.MeshId)) || (updateInfo.NodeAddress == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("updateInfo", System.ServiceModel.SR.GetString("PeerInvalidMessageBody", new object[] { updateInfo }));
            }
            Guid registrationId = updateInfo.RegistrationId;
            MeshEntry meshEntry = this.GetMeshEntry(updateInfo.MeshId);
            LiteLock liteLock = null;
            if ((updateInfo.RegistrationId == Guid.Empty) || (meshEntry == null))
            {
                return this.Register(updateInfo.ClientId, updateInfo.MeshId, updateInfo.NodeAddress);
            }
            try
            {
                RegistrationEntry entry;
                LiteLock.Acquire(out liteLock, meshEntry.Gate);
                if (!meshEntry.EntryTable.TryGetValue(updateInfo.RegistrationId, out entry))
                {
                    return this.Register(updateInfo.ClientId, updateInfo.MeshId, updateInfo.NodeAddress);
                }
                lock (entry)
                {
                    entry.Address = updateInfo.NodeAddress;
                    entry.Expires = DateTime.UtcNow + this.RefreshInterval;
                }
            }
            finally
            {
                LiteLock.Release(liteLock);
            }
            return new RegisterResponseInfo(registrationId, this.RefreshInterval);
        }

        public TimeSpan CleanupInterval
        {
            get
            {
                return this.cleanupInterval;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                lock (this.ThisLock)
                {
                    this.ThrowIfOpened("Set CleanupInterval");
                    this.cleanupInterval = value;
                }
            }
        }

        public bool ControlShape
        {
            get
            {
                return this.controlShape;
            }
            set
            {
                lock (this.ThisLock)
                {
                    this.ThrowIfOpened("Set ControlShape");
                    this.controlShape = value;
                }
            }
        }

        public TimeSpan RefreshInterval
        {
            get
            {
                return this.refreshInterval;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                lock (this.ThisLock)
                {
                    this.ThrowIfOpened("Set RefreshInterval");
                    this.refreshInterval = value;
                }
            }
        }

        private object ThisLock
        {
            get
            {
                return this.thisLock;
            }
        }

        internal class LiteLock
        {
            private bool forWrite;
            private LockCookie lc;
            private ReaderWriterLock locker;
            private TimeSpan timeout = TimeSpan.FromMinutes(1.0);
            private bool upgraded;

            private LiteLock(ReaderWriterLock locker, bool forWrite)
            {
                this.locker = locker;
                this.forWrite = forWrite;
            }

            public static void Acquire(out CustomPeerResolverService.LiteLock liteLock, ReaderWriterLock locker)
            {
                Acquire(out liteLock, locker, false);
            }

            public static void Acquire(out CustomPeerResolverService.LiteLock liteLock, ReaderWriterLock locker, bool forWrite)
            {
                CustomPeerResolverService.LiteLock @lock = new CustomPeerResolverService.LiteLock(locker, forWrite);
                try
                {
                }
                finally
                {
                    if (forWrite)
                    {
                        locker.AcquireWriterLock(@lock.timeout);
                    }
                    else
                    {
                        locker.AcquireReaderLock(@lock.timeout);
                    }
                    liteLock = @lock;
                }
            }

            public void DowngradeFromWriterLock()
            {
                if (this.upgraded)
                {
                    this.locker.DowngradeFromWriterLock(ref this.lc);
                    this.upgraded = false;
                }
            }

            public static void Release(CustomPeerResolverService.LiteLock liteLock)
            {
                if (liteLock != null)
                {
                    if (liteLock.forWrite)
                    {
                        liteLock.locker.ReleaseWriterLock();
                    }
                    else
                    {
                        liteLock.locker.ReleaseReaderLock();
                    }
                }
            }

            public void UpgradeToWriterLock()
            {
                try
                {
                }
                finally
                {
                    this.lc = this.locker.UpgradeToWriterLock(this.timeout);
                    this.upgraded = true;
                }
            }
        }

        internal class MeshEntry
        {
            private List<CustomPeerResolverService.RegistrationEntry> entryList;
            private Dictionary<Guid, CustomPeerResolverService.RegistrationEntry> entryTable;
            private ReaderWriterLock gate;
            private Dictionary<string, CustomPeerResolverService.RegistrationEntry> service2EntryTable;

            internal MeshEntry()
            {
                this.EntryTable = new Dictionary<Guid, CustomPeerResolverService.RegistrationEntry>();
                this.Service2EntryTable = new Dictionary<string, CustomPeerResolverService.RegistrationEntry>();
                this.EntryList = new List<CustomPeerResolverService.RegistrationEntry>();
                this.Gate = new ReaderWriterLock();
            }

            public List<CustomPeerResolverService.RegistrationEntry> EntryList
            {
                get
                {
                    return this.entryList;
                }
                set
                {
                    this.entryList = value;
                }
            }

            public Dictionary<Guid, CustomPeerResolverService.RegistrationEntry> EntryTable
            {
                get
                {
                    return this.entryTable;
                }
                set
                {
                    this.entryTable = value;
                }
            }

            public ReaderWriterLock Gate
            {
                get
                {
                    return this.gate;
                }
                set
                {
                    this.gate = value;
                }
            }

            public Dictionary<string, CustomPeerResolverService.RegistrationEntry> Service2EntryTable
            {
                get
                {
                    return this.service2EntryTable;
                }
                set
                {
                    this.service2EntryTable = value;
                }
            }
        }

        internal class RegistrationEntry
        {
            private PeerNodeAddress address;
            private Guid clientId;
            private DateTime expires;
            private string meshId;
            private Guid registrationId;
            private CustomPeerResolverService.RegistrationState state;

            public RegistrationEntry(Guid clientId, Guid registrationId, string meshId, DateTime expires, PeerNodeAddress address)
            {
                this.ClientId = clientId;
                this.RegistrationId = registrationId;
                this.MeshId = meshId;
                this.Expires = expires;
                this.Address = address;
                this.State = CustomPeerResolverService.RegistrationState.OK;
            }

            public PeerNodeAddress Address
            {
                get
                {
                    return this.address;
                }
                set
                {
                    this.address = value;
                }
            }

            public Guid ClientId
            {
                get
                {
                    return this.clientId;
                }
                set
                {
                    this.clientId = value;
                }
            }

            public DateTime Expires
            {
                get
                {
                    return this.expires;
                }
                set
                {
                    this.expires = value;
                }
            }

            public string MeshId
            {
                get
                {
                    return this.meshId;
                }
                set
                {
                    this.meshId = value;
                }
            }

            public Guid RegistrationId
            {
                get
                {
                    return this.registrationId;
                }
                set
                {
                    this.registrationId = value;
                }
            }

            public CustomPeerResolverService.RegistrationState State
            {
                get
                {
                    return this.state;
                }
                set
                {
                    this.state = value;
                }
            }
        }

        internal enum RegistrationState
        {
            OK,
            Deleted
        }
    }
}

