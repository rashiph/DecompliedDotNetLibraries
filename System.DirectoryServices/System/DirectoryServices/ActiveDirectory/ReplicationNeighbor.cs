namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public class ReplicationNeighbor
    {
        private int consecutiveSyncFailures;
        private int lastSyncResult;
        private Hashtable nameTable;
        private string namingContext;
        private ReplicationNeighborOptions replicaFlags;
        private DirectoryServer server;
        private string sourceServer;
        private string sourceServerDN;
        private DateTime timeLastSyncAttempt;
        private DateTime timeLastSyncSuccess;
        private ActiveDirectoryTransportType transportType;
        private long usnAttributeFilter;
        private long usnLastObjChangeSynced;
        private Guid uuidSourceDsaInvocationID;

        internal ReplicationNeighbor(IntPtr addr, DirectoryServer server, Hashtable table)
        {
            DS_REPL_NEIGHBOR structure = new DS_REPL_NEIGHBOR();
            Marshal.PtrToStructure(addr, structure);
            this.namingContext = Marshal.PtrToStringUni(structure.pszNamingContext);
            this.sourceServerDN = Marshal.PtrToStringUni(structure.pszSourceDsaDN);
            string distinguishedName = Marshal.PtrToStringUni(structure.pszAsyncIntersiteTransportDN);
            if (distinguishedName != null)
            {
                if (string.Compare(Utils.GetDNComponents(Utils.GetRdnFromDN(distinguishedName))[0].Value, "SMTP", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    this.transportType = ActiveDirectoryTransportType.Smtp;
                }
                else
                {
                    this.transportType = ActiveDirectoryTransportType.Rpc;
                }
            }
            this.replicaFlags = (ReplicationNeighborOptions) structure.dwReplicaFlags;
            this.uuidSourceDsaInvocationID = structure.uuidSourceDsaInvocationID;
            this.usnLastObjChangeSynced = structure.usnLastObjChangeSynced;
            this.usnAttributeFilter = structure.usnAttributeFilter;
            this.timeLastSyncSuccess = DateTime.FromFileTime(structure.ftimeLastSyncSuccess);
            this.timeLastSyncAttempt = DateTime.FromFileTime(structure.ftimeLastSyncAttempt);
            this.lastSyncResult = structure.dwLastSyncResult;
            this.consecutiveSyncFailures = structure.cNumConsecutiveSyncFailures;
            this.server = server;
            this.nameTable = table;
        }

        public int ConsecutiveFailureCount
        {
            get
            {
                return this.consecutiveSyncFailures;
            }
        }

        public DateTime LastAttemptedSync
        {
            get
            {
                return this.timeLastSyncAttempt;
            }
        }

        public DateTime LastSuccessfulSync
        {
            get
            {
                return this.timeLastSyncSuccess;
            }
        }

        public string LastSyncMessage
        {
            get
            {
                return ExceptionHelper.GetErrorMessage(this.lastSyncResult, false);
            }
        }

        public int LastSyncResult
        {
            get
            {
                return this.lastSyncResult;
            }
        }

        public string PartitionName
        {
            get
            {
                return this.namingContext;
            }
        }

        public ReplicationNeighborOptions ReplicationNeighborOption
        {
            get
            {
                return this.replicaFlags;
            }
        }

        public Guid SourceInvocationId
        {
            get
            {
                return this.uuidSourceDsaInvocationID;
            }
        }

        public string SourceServer
        {
            get
            {
                if (this.sourceServer == null)
                {
                    if (this.nameTable.Contains(this.SourceInvocationId))
                    {
                        this.sourceServer = (string) this.nameTable[this.SourceInvocationId];
                    }
                    else if (this.sourceServerDN != null)
                    {
                        this.sourceServer = Utils.GetServerNameFromInvocationID(this.sourceServerDN, this.SourceInvocationId, this.server);
                        this.nameTable.Add(this.SourceInvocationId, this.sourceServer);
                    }
                }
                return this.sourceServer;
            }
        }

        public ActiveDirectoryTransportType TransportType
        {
            get
            {
                return this.transportType;
            }
        }

        public long UsnAttributeFilter
        {
            get
            {
                return this.usnAttributeFilter;
            }
        }

        public long UsnLastObjectChangeSynced
        {
            get
            {
                return this.usnLastObjChangeSynced;
            }
        }

        [Flags]
        public enum ReplicationNeighborOptions : long
        {
            CompressChanges = 0x10000000L,
            DisableScheduledSync = 0x8000000L,
            FullSyncInProgress = 0x10000L,
            FullSyncNextPacket = 0x20000L,
            IgnoreChangeNotifications = 0x4000000L,
            NeverSynced = 0x200000L,
            NoChangeNotifications = 0x20000000L,
            PartialAttributeSet = 0x40000000L,
            Preempted = 0x1000000L,
            ReturnObjectParent = 0x800L,
            ScheduledSync = 0x40L,
            SyncOnStartup = 0x20L,
            TwoWaySync = 0x200L,
            UseInterSiteTransport = 0x80L,
            Writeable = 0x10L
        }
    }
}

