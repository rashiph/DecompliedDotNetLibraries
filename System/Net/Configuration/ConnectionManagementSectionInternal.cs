namespace System.Net.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Threading;

    internal sealed class ConnectionManagementSectionInternal
    {
        private static object classSyncObject;
        private Hashtable connectionManagement;

        internal ConnectionManagementSectionInternal(ConnectionManagementSection section)
        {
            if (section.ConnectionManagement.Count > 0)
            {
                this.connectionManagement = new Hashtable(section.ConnectionManagement.Count);
                foreach (ConnectionManagementElement element in section.ConnectionManagement)
                {
                    this.connectionManagement[element.Address] = element.MaxConnection;
                }
            }
        }

        internal static ConnectionManagementSectionInternal GetSection()
        {
            lock (ClassSyncObject)
            {
                ConnectionManagementSection section = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.ConnectionManagementSectionPath) as ConnectionManagementSection;
                if (section == null)
                {
                    return null;
                }
                return new ConnectionManagementSectionInternal(section);
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }

        internal Hashtable ConnectionManagement
        {
            get
            {
                Hashtable connectionManagement = this.connectionManagement;
                if (connectionManagement == null)
                {
                    connectionManagement = new Hashtable();
                }
                return connectionManagement;
            }
        }
    }
}

