namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReplicationFailureCollection : ReadOnlyCollectionBase
    {
        private Hashtable nameTable;
        private DirectoryServer server;

        internal ReplicationFailureCollection(DirectoryServer server)
        {
            this.server = server;
            Hashtable table = new Hashtable();
            this.nameTable = Hashtable.Synchronized(table);
        }

        private int Add(ReplicationFailure failure)
        {
            return base.InnerList.Add(failure);
        }

        internal void AddHelper(DS_REPL_KCC_DSA_FAILURES failures, IntPtr info)
        {
            int cNumEntries = failures.cNumEntries;
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; i < cNumEntries; i++)
            {
                zero = (IntPtr) ((((long) info) + (Marshal.SizeOf(typeof(int)) * 2)) + (i * Marshal.SizeOf(typeof(DS_REPL_KCC_DSA_FAILURE))));
                ReplicationFailure failure = new ReplicationFailure(zero, this.server, this.nameTable);
                if (failure.LastErrorCode == 0)
                {
                    failure.lastResult = ExceptionHelper.ERROR_DS_UNKNOWN_ERROR;
                }
                this.Add(failure);
            }
        }

        public bool Contains(ReplicationFailure failure)
        {
            if (failure == null)
            {
                throw new ArgumentNullException("failure");
            }
            return base.InnerList.Contains(failure);
        }

        public void CopyTo(ReplicationFailure[] failures, int index)
        {
            base.InnerList.CopyTo(failures, index);
        }

        public int IndexOf(ReplicationFailure failure)
        {
            if (failure == null)
            {
                throw new ArgumentNullException("failure");
            }
            return base.InnerList.IndexOf(failure);
        }

        public ReplicationFailure this[int index]
        {
            get
            {
                return (ReplicationFailure) base.InnerList[index];
            }
        }
    }
}

