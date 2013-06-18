namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class ReadOnlyDirectoryServerCollection : ReadOnlyCollectionBase
    {
        internal ReadOnlyDirectoryServerCollection()
        {
        }

        internal ReadOnlyDirectoryServerCollection(ArrayList values)
        {
            if (values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    this.Add((DirectoryServer) values[i]);
                }
            }
        }

        internal int Add(DirectoryServer server)
        {
            return base.InnerList.Add(server);
        }

        internal void AddRange(ICollection servers)
        {
            base.InnerList.AddRange(servers);
        }

        internal void Clear()
        {
            base.InnerList.Clear();
        }

        public bool Contains(DirectoryServer directoryServer)
        {
            if (directoryServer == null)
            {
                throw new ArgumentNullException("directoryServer");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DirectoryServer server = (DirectoryServer) base.InnerList[i];
                if (Utils.Compare(server.Name, directoryServer.Name) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(DirectoryServer[] directoryServers, int index)
        {
            base.InnerList.CopyTo(directoryServers, index);
        }

        public int IndexOf(DirectoryServer directoryServer)
        {
            if (directoryServer == null)
            {
                throw new ArgumentNullException("directoryServer");
            }
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                DirectoryServer server = (DirectoryServer) base.InnerList[i];
                if (Utils.Compare(server.Name, directoryServer.Name) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public DirectoryServer this[int index]
        {
            get
            {
                return (DirectoryServer) base.InnerList[index];
            }
        }
    }
}

