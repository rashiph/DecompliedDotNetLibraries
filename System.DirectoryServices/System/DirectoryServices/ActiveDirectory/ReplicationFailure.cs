namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public class ReplicationFailure
    {
        internal int lastResult;
        private Hashtable nameTable;
        private int numFailures;
        private DirectoryServer server;
        private string sourceDsaDN;
        private string sourceServer;
        private DateTime timeFirstFailure;
        private Guid uuidDsaObjGuid;

        internal ReplicationFailure(IntPtr addr, DirectoryServer server, Hashtable table)
        {
            DS_REPL_KCC_DSA_FAILURE structure = new DS_REPL_KCC_DSA_FAILURE();
            Marshal.PtrToStructure(addr, structure);
            this.sourceDsaDN = Marshal.PtrToStringUni(structure.pszDsaDN);
            this.uuidDsaObjGuid = structure.uuidDsaObjGuid;
            this.timeFirstFailure = DateTime.FromFileTime(structure.ftimeFirstFailure);
            this.numFailures = structure.cNumFailures;
            this.lastResult = structure.dwLastResult;
            this.server = server;
            this.nameTable = table;
        }

        public int ConsecutiveFailureCount
        {
            get
            {
                return this.numFailures;
            }
        }

        public DateTime FirstFailureTime
        {
            get
            {
                return this.timeFirstFailure;
            }
        }

        public int LastErrorCode
        {
            get
            {
                return this.lastResult;
            }
        }

        public string LastErrorMessage
        {
            get
            {
                return ExceptionHelper.GetErrorMessage(this.lastResult, false);
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
                    else if (this.sourceDsaDN != null)
                    {
                        this.sourceServer = Utils.GetServerNameFromInvocationID(this.sourceDsaDN, this.SourceServerGuid, this.server);
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
    }
}

