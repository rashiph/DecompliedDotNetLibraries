namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public class ReplicationOperation
    {
        private string dsaDN;
        private Hashtable nameTable;
        private string namingContext;
        private ReplicationOperationType operationType;
        private int priority;
        private int serialNumber;
        private DirectoryServer server;
        private string sourceServer;
        private DateTime timeEnqueued;
        private Guid uuidDsaObjGuid;

        internal ReplicationOperation(IntPtr addr, DirectoryServer server, Hashtable table)
        {
            DS_REPL_OP structure = new DS_REPL_OP();
            Marshal.PtrToStructure(addr, structure);
            this.timeEnqueued = DateTime.FromFileTime(structure.ftimeEnqueued);
            this.serialNumber = structure.ulSerialNumber;
            this.priority = structure.ulPriority;
            this.operationType = structure.OpType;
            this.namingContext = Marshal.PtrToStringUni(structure.pszNamingContext);
            this.dsaDN = Marshal.PtrToStringUni(structure.pszDsaDN);
            this.uuidDsaObjGuid = structure.uuidDsaObjGuid;
            this.server = server;
            this.nameTable = table;
        }

        public int OperationNumber
        {
            get
            {
                return this.serialNumber;
            }
        }

        public ReplicationOperationType OperationType
        {
            get
            {
                return this.operationType;
            }
        }

        public string PartitionName
        {
            get
            {
                return this.namingContext;
            }
        }

        public int Priority
        {
            get
            {
                return this.priority;
            }
        }

        public string SourceServer
        {
            get
            {
                if (this.sourceServer == null)
                {
                    if (this.nameTable.Contains(this.SourceServerGuid))
                    {
                        this.sourceServer = (string) this.nameTable[this.SourceServerGuid];
                    }
                    else if (this.dsaDN != null)
                    {
                        this.sourceServer = Utils.GetServerNameFromInvocationID(this.dsaDN, this.SourceServerGuid, this.server);
                        this.nameTable.Add(this.SourceServerGuid, this.sourceServer);
                    }
                }
                return this.sourceServer;
            }
        }

        private Guid SourceServerGuid
        {
            get
            {
                return this.uuidDsaObjGuid;
            }
        }

        public DateTime TimeEnqueued
        {
            get
            {
                return this.timeEnqueued;
            }
        }
    }
}

