namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReplicationOperationCollection : ReadOnlyCollectionBase
    {
        private Hashtable nameTable;
        private DirectoryServer server;

        internal ReplicationOperationCollection(DirectoryServer server)
        {
            this.server = server;
            Hashtable table = new Hashtable();
            this.nameTable = Hashtable.Synchronized(table);
        }

        private int Add(ReplicationOperation operation)
        {
            return base.InnerList.Add(operation);
        }

        internal void AddHelper(DS_REPL_PENDING_OPS operations, IntPtr info)
        {
            int cNumPendingOps = operations.cNumPendingOps;
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; i < cNumPendingOps; i++)
            {
                zero = (IntPtr) ((((long) info) + Marshal.SizeOf(typeof(DS_REPL_PENDING_OPS))) + (i * Marshal.SizeOf(typeof(DS_REPL_OP))));
                ReplicationOperation operation = new ReplicationOperation(zero, this.server, this.nameTable);
                this.Add(operation);
            }
        }

        public bool Contains(ReplicationOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }
            return base.InnerList.Contains(operation);
        }

        public void CopyTo(ReplicationOperation[] operations, int index)
        {
            base.InnerList.CopyTo(operations, index);
        }

        internal ReplicationOperation GetFirstOperation()
        {
            ReplicationOperation operation = (ReplicationOperation) base.InnerList[0];
            base.InnerList.RemoveAt(0);
            return operation;
        }

        public int IndexOf(ReplicationOperation operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException("operation");
            }
            return base.InnerList.IndexOf(operation);
        }

        public ReplicationOperation this[int index]
        {
            get
            {
                return (ReplicationOperation) base.InnerList[index];
            }
        }
    }
}

