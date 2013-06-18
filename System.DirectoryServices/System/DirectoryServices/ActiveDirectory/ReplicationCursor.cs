namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;

    public class ReplicationCursor
    {
        private bool advanced;
        private Guid invocationID;
        private string partition;
        private DirectoryServer server;
        private string serverDN;
        private string sourceServer;
        private DateTime syncTime;
        private long USN;

        private ReplicationCursor()
        {
        }

        internal ReplicationCursor(DirectoryServer server, string partition, Guid guid, long filter)
        {
            this.partition = partition;
            this.invocationID = guid;
            this.USN = filter;
            this.server = server;
        }

        internal ReplicationCursor(DirectoryServer server, string partition, Guid guid, long filter, long time, IntPtr dn)
        {
            this.partition = partition;
            this.invocationID = guid;
            this.USN = filter;
            this.syncTime = DateTime.FromFileTime(time);
            this.serverDN = Marshal.PtrToStringUni(dn);
            this.advanced = true;
            this.server = server;
        }

        public DateTime LastSuccessfulSyncTime
        {
            get
            {
                if (this.advanced)
                {
                    return this.syncTime;
                }
                if ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor == 0))
                {
                    throw new PlatformNotSupportedException(Res.GetString("DSNotSupportOnClient"));
                }
                throw new PlatformNotSupportedException(Res.GetString("DSNotSupportOnDC"));
            }
        }

        public string PartitionName
        {
            get
            {
                return this.partition;
            }
        }

        public Guid SourceInvocationId
        {
            get
            {
                return this.invocationID;
            }
        }

        public string SourceServer
        {
            get
            {
                if (!this.advanced || (this.advanced && (this.serverDN != null)))
                {
                    this.sourceServer = Utils.GetServerNameFromInvocationID(this.serverDN, this.SourceInvocationId, this.server);
                }
                return this.sourceServer;
            }
        }

        public long UpToDatenessUsn
        {
            get
            {
                return this.USN;
            }
        }
    }
}

