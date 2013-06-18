namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public class ReplicationNeighborCollection : ReadOnlyCollectionBase
    {
        private Hashtable nameTable;
        private DirectoryServer server;

        internal ReplicationNeighborCollection(DirectoryServer server)
        {
            this.server = server;
            Hashtable table = new Hashtable();
            this.nameTable = Hashtable.Synchronized(table);
        }

        private int Add(ReplicationNeighbor neighbor)
        {
            return base.InnerList.Add(neighbor);
        }

        internal void AddHelper(DS_REPL_NEIGHBORS neighbors, IntPtr info)
        {
            int cNumNeighbors = neighbors.cNumNeighbors;
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; i < cNumNeighbors; i++)
            {
                zero = (IntPtr) ((((long) info) + (Marshal.SizeOf(typeof(int)) * 2)) + (i * Marshal.SizeOf(typeof(DS_REPL_NEIGHBOR))));
                ReplicationNeighbor neighbor = new ReplicationNeighbor(zero, this.server, this.nameTable);
                this.Add(neighbor);
            }
        }

        public bool Contains(ReplicationNeighbor neighbor)
        {
            if (neighbor == null)
            {
                throw new ArgumentNullException("neighbor");
            }
            return base.InnerList.Contains(neighbor);
        }

        public void CopyTo(ReplicationNeighbor[] neighbors, int index)
        {
            base.InnerList.CopyTo(neighbors, index);
        }

        public int IndexOf(ReplicationNeighbor neighbor)
        {
            if (neighbor == null)
            {
                throw new ArgumentNullException("neighbor");
            }
            return base.InnerList.IndexOf(neighbor);
        }

        public ReplicationNeighbor this[int index]
        {
            get
            {
                return (ReplicationNeighbor) base.InnerList[index];
            }
        }
    }
}

