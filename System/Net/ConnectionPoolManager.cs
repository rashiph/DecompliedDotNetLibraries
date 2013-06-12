namespace System.Net
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Threading;

    internal class ConnectionPoolManager
    {
        private static Hashtable m_ConnectionPools = new Hashtable();
        private static object s_InternalSyncObject;

        private ConnectionPoolManager()
        {
        }

        internal static void CleanupConnectionPool(ServicePoint servicePoint, string groupName)
        {
            string str = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
            lock (InternalSyncObject)
            {
                ConnectionPool pool = (ConnectionPool) m_ConnectionPools[str];
                if (pool != null)
                {
                    pool.ForceCleanup();
                }
            }
        }

        private static string GenerateKey(string hostName, int port, string groupName)
        {
            return (hostName + "\r" + port.ToString(NumberFormatInfo.InvariantInfo) + "\r" + groupName);
        }

        internal static ConnectionPool GetConnectionPool(ServicePoint servicePoint, string groupName, CreateConnectionDelegate createConnectionCallback)
        {
            string str = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
            lock (InternalSyncObject)
            {
                ConnectionPool pool = (ConnectionPool) m_ConnectionPools[str];
                if (pool == null)
                {
                    pool = new ConnectionPool(servicePoint, servicePoint.ConnectionLimit, 0, servicePoint.MaxIdleTime, createConnectionCallback);
                    m_ConnectionPools[str] = pool;
                }
                return pool;
            }
        }

        internal static bool RemoveConnectionPool(ServicePoint servicePoint, string groupName)
        {
            string key = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
            lock (InternalSyncObject)
            {
                if (((ConnectionPool) m_ConnectionPools[key]) != null)
                {
                    m_ConnectionPools[key] = null;
                    m_ConnectionPools.Remove(key);
                    return true;
                }
            }
            return false;
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }
    }
}

