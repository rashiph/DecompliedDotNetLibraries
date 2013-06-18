namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Reflection;

    public class ReplicationConnectionCollection : ReadOnlyCollectionBase
    {
        internal ReplicationConnectionCollection()
        {
        }

        internal int Add(ReplicationConnection value)
        {
            return base.InnerList.Add(value);
        }

        public bool Contains(ReplicationConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (!connection.existingConnection)
            {
                throw new InvalidOperationException(Res.GetString("ConnectionNotCommitted", new object[] { connection.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(connection.context, connection.cachedDirectoryEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ReplicationConnection connection2 = (ReplicationConnection) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(connection2.context, connection2.cachedDirectoryEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ReplicationConnection[] connections, int index)
        {
            base.InnerList.CopyTo(connections, index);
        }

        public int IndexOf(ReplicationConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }
            if (!connection.existingConnection)
            {
                throw new InvalidOperationException(Res.GetString("ConnectionNotCommitted", new object[] { connection.Name }));
            }
            string str = (string) PropertyManager.GetPropertyValue(connection.context, connection.cachedDirectoryEntry, PropertyManager.DistinguishedName);
            for (int i = 0; i < base.InnerList.Count; i++)
            {
                ReplicationConnection connection2 = (ReplicationConnection) base.InnerList[i];
                string str2 = (string) PropertyManager.GetPropertyValue(connection2.context, connection2.cachedDirectoryEntry, PropertyManager.DistinguishedName);
                if (Utils.Compare(str2, str) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public ReplicationConnection this[int index]
        {
            get
            {
                return (ReplicationConnection) base.InnerList[index];
            }
        }
    }
}

