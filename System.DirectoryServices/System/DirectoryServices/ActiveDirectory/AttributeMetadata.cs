namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    public class AttributeMetadata
    {
        private bool advanced;
        private int dwVersion;
        private DateTime ftimeLastOriginatingChange;
        private Hashtable nameTable;
        private string originatingServerName;
        private string pszAttributeName;
        private string pszLastOriginatingDsaDN;
        private DirectoryServer server;
        private long usnLocalChange;
        private long usnOriginatingChange;
        private Guid uuidLastOriginatingDsaInvocationID;

        internal AttributeMetadata(IntPtr info, bool advanced, DirectoryServer server, Hashtable table)
        {
            if (advanced)
            {
                DS_REPL_ATTR_META_DATA_2 structure = new DS_REPL_ATTR_META_DATA_2();
                Marshal.PtrToStructure(info, structure);
                this.pszAttributeName = Marshal.PtrToStringUni(structure.pszAttributeName);
                this.dwVersion = structure.dwVersion;
                long fileTime = ((long) ((ulong) structure.ftimeLastOriginatingChange1)) + (structure.ftimeLastOriginatingChange2 << 0x20);
                this.ftimeLastOriginatingChange = DateTime.FromFileTime(fileTime);
                this.uuidLastOriginatingDsaInvocationID = structure.uuidLastOriginatingDsaInvocationID;
                this.usnOriginatingChange = structure.usnOriginatingChange;
                this.usnLocalChange = structure.usnLocalChange;
                this.pszLastOriginatingDsaDN = Marshal.PtrToStringUni(structure.pszLastOriginatingDsaDN);
            }
            else
            {
                DS_REPL_ATTR_META_DATA ds_repl_attr_meta_data = new DS_REPL_ATTR_META_DATA();
                Marshal.PtrToStructure(info, ds_repl_attr_meta_data);
                this.pszAttributeName = Marshal.PtrToStringUni(ds_repl_attr_meta_data.pszAttributeName);
                this.dwVersion = ds_repl_attr_meta_data.dwVersion;
                long num2 = ((long) ((ulong) ds_repl_attr_meta_data.ftimeLastOriginatingChange1)) + (ds_repl_attr_meta_data.ftimeLastOriginatingChange2 << 0x20);
                this.ftimeLastOriginatingChange = DateTime.FromFileTime(num2);
                this.uuidLastOriginatingDsaInvocationID = ds_repl_attr_meta_data.uuidLastOriginatingDsaInvocationID;
                this.usnOriginatingChange = ds_repl_attr_meta_data.usnOriginatingChange;
                this.usnLocalChange = ds_repl_attr_meta_data.usnLocalChange;
            }
            this.server = server;
            this.nameTable = table;
            this.advanced = advanced;
        }

        public DateTime LastOriginatingChangeTime
        {
            get
            {
                return this.ftimeLastOriginatingChange;
            }
        }

        public Guid LastOriginatingInvocationId
        {
            get
            {
                return this.uuidLastOriginatingDsaInvocationID;
            }
        }

        public long LocalChangeUsn
        {
            get
            {
                return this.usnLocalChange;
            }
        }

        public string Name
        {
            get
            {
                return this.pszAttributeName;
            }
        }

        public long OriginatingChangeUsn
        {
            get
            {
                return this.usnOriginatingChange;
            }
        }

        public string OriginatingServer
        {
            get
            {
                if (this.originatingServerName == null)
                {
                    if (this.nameTable.Contains(this.LastOriginatingInvocationId))
                    {
                        this.originatingServerName = (string) this.nameTable[this.LastOriginatingInvocationId];
                    }
                    else if (!this.advanced || (this.advanced && (this.pszLastOriginatingDsaDN != null)))
                    {
                        this.originatingServerName = Utils.GetServerNameFromInvocationID(this.pszLastOriginatingDsaDN, this.LastOriginatingInvocationId, this.server);
                        this.nameTable.Add(this.LastOriginatingInvocationId, this.originatingServerName);
                    }
                }
                return this.originatingServerName;
            }
        }

        public int Version
        {
            get
            {
                return this.dwVersion;
            }
        }
    }
}

