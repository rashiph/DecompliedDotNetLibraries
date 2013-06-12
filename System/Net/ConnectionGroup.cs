namespace System.Net
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class ConnectionGroup
    {
        private const int DefaultConnectionListSize = 3;
        private HttpAbortDelegate m_AbortDelegate;
        internal bool m_AuthenticationGroup;
        private System.Collections.Queue m_AuthenticationRequestQueue;
        private int m_ConnectionLimit;
        private ArrayList m_ConnectionList;
        private object m_Event;
        private int m_IISVersion = -1;
        private string m_Name;
        private bool m_NtlmNegGroup;
        private System.Net.ServicePoint m_ServicePoint;

        internal ConnectionGroup(System.Net.ServicePoint servicePoint, string connName)
        {
            this.m_ServicePoint = servicePoint;
            this.m_ConnectionLimit = servicePoint.ConnectionLimit;
            this.m_ConnectionList = new ArrayList(3);
            this.m_Name = MakeQueryStr(connName);
            this.m_AbortDelegate = new HttpAbortDelegate(this.Abort);
        }

        private bool Abort(HttpWebRequest request, WebException webException)
        {
            lock (this.m_ConnectionList)
            {
                this.AsyncWaitHandle.Set();
            }
            return true;
        }

        internal void Associate(Connection connection)
        {
            lock (this.m_ConnectionList)
            {
                this.m_ConnectionList.Add(connection);
            }
        }

        internal void ConnectionGoneIdle()
        {
            if (this.m_AuthenticationGroup)
            {
                lock (this.m_ConnectionList)
                {
                    this.AsyncWaitHandle.Set();
                }
            }
        }

        [Conditional("DEBUG")]
        internal void Debug(int requestHash)
        {
            using (IEnumerator enumerator = this.m_ConnectionList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Connection current = (Connection) enumerator.Current;
                }
            }
        }

        internal void DisableKeepAliveOnConnections()
        {
            ArrayList list = new ArrayList();
            lock (this.m_ConnectionList)
            {
                foreach (Connection connection in this.m_ConnectionList)
                {
                    list.Add(connection);
                }
                this.m_ConnectionList.Clear();
            }
            foreach (Connection connection2 in list)
            {
                connection2.CloseOnIdle();
            }
        }

        internal void Disassociate(Connection connection)
        {
            lock (this.m_ConnectionList)
            {
                this.m_ConnectionList.Remove(connection);
            }
        }

        internal Connection FindConnection(HttpWebRequest request, string connName, out bool forcedsubmit)
        {
            Connection connection = null;
            Connection connection2 = null;
            bool flag = false;
            forcedsubmit = false;
            if (this.m_AuthenticationGroup || request.LockConnection)
            {
                this.m_AuthenticationGroup = true;
                return this.FindConnectionAuthenticationGroup(request, connName);
            }
            lock (this.m_ConnectionList)
            {
                int busyCount = 0x7fffffff;
                bool flag2 = false;
                foreach (Connection connection3 in this.m_ConnectionList)
                {
                    bool flag3 = false;
                    if (flag2)
                    {
                        flag3 = !connection3.NonKeepAliveRequestPipelined && (busyCount > connection3.BusyCount);
                    }
                    else
                    {
                        flag3 = !connection3.NonKeepAliveRequestPipelined || (busyCount > connection3.BusyCount);
                    }
                    if (flag3)
                    {
                        connection = connection3;
                        busyCount = connection3.BusyCount;
                        if (!flag2)
                        {
                            flag2 = !connection3.NonKeepAliveRequestPipelined;
                        }
                        if (flag2 && (busyCount == 0))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag && (this.CurrentConnections < this.ConnectionLimit))
                {
                    connection2 = new Connection(this);
                    forcedsubmit = false;
                }
                else
                {
                    connection2 = connection;
                    forcedsubmit = !flag2;
                }
                connection2.MarkAsReserved();
            }
            return connection2;
        }

        private Connection FindConnectionAuthenticationGroup(HttpWebRequest request, string connName)
        {
            Connection leastbusyConnection = null;
            Connection connection3;
            lock (this.m_ConnectionList)
            {
                Connection connection2 = this.FindMatchingConnection(request, connName, out leastbusyConnection);
                if (connection2 != null)
                {
                    connection2.MarkAsReserved();
                    return connection2;
                }
                if (this.AuthenticationRequestQueue.Count == 0)
                {
                    if (leastbusyConnection != null)
                    {
                        if (request.LockConnection)
                        {
                            this.m_NtlmNegGroup = true;
                            this.m_IISVersion = leastbusyConnection.IISVersion;
                        }
                        if (request.LockConnection || ((this.m_NtlmNegGroup && !request.Pipelined) && (request.UnsafeOrProxyAuthenticatedConnectionSharing && (this.m_IISVersion >= 6))))
                        {
                            leastbusyConnection.LockedRequest = request;
                        }
                        leastbusyConnection.MarkAsReserved();
                        return leastbusyConnection;
                    }
                }
                else if (leastbusyConnection != null)
                {
                    this.AsyncWaitHandle.Set();
                }
                this.AuthenticationRequestQueue.Enqueue(request);
            }
        Label_00C1:
            request.AbortDelegate = this.m_AbortDelegate;
            if (!request.Aborted)
            {
                this.AsyncWaitHandle.WaitOne();
            }
            lock (this.m_ConnectionList)
            {
                if (request.Aborted)
                {
                    this.PruneAbortedRequests();
                    return null;
                }
                this.FindMatchingConnection(request, connName, out leastbusyConnection);
                if (this.AuthenticationRequestQueue.Peek() == request)
                {
                    this.AuthenticationRequestQueue.Dequeue();
                    if (leastbusyConnection != null)
                    {
                        if (request.LockConnection)
                        {
                            this.m_NtlmNegGroup = true;
                            this.m_IISVersion = leastbusyConnection.IISVersion;
                        }
                        if (request.LockConnection || ((this.m_NtlmNegGroup && !request.Pipelined) && (request.UnsafeOrProxyAuthenticatedConnectionSharing && (this.m_IISVersion >= 6))))
                        {
                            leastbusyConnection.LockedRequest = request;
                        }
                        leastbusyConnection.MarkAsReserved();
                        return leastbusyConnection;
                    }
                    this.AuthenticationRequestQueue.Enqueue(request);
                }
                if (leastbusyConnection == null)
                {
                    this.AsyncWaitHandle.Reset();
                }
                goto Label_00C1;
            }
            return connection3;
        }

        private Connection FindMatchingConnection(HttpWebRequest request, string connName, out Connection leastbusyConnection)
        {
            int busyCount = 0x7fffffff;
            bool flag = false;
            leastbusyConnection = null;
            lock (this.m_ConnectionList)
            {
                busyCount = 0x7fffffff;
                foreach (Connection connection in this.m_ConnectionList)
                {
                    if (connection.LockedRequest == request)
                    {
                        leastbusyConnection = connection;
                        return connection;
                    }
                    if ((!connection.NonKeepAliveRequestPipelined && (connection.BusyCount < busyCount)) && (connection.LockedRequest == null))
                    {
                        leastbusyConnection = connection;
                        busyCount = connection.BusyCount;
                        if (busyCount == 0)
                        {
                            flag = true;
                        }
                    }
                }
                if (!flag && (this.CurrentConnections < this.ConnectionLimit))
                {
                    leastbusyConnection = new Connection(this);
                }
            }
            return null;
        }

        internal static string MakeQueryStr(string connName)
        {
            if (connName != null)
            {
                return connName;
            }
            return "";
        }

        private void PruneAbortedRequests()
        {
            lock (this.m_ConnectionList)
            {
                System.Collections.Queue queue = new System.Collections.Queue();
                foreach (HttpWebRequest request in this.AuthenticationRequestQueue)
                {
                    if (!request.Aborted)
                    {
                        queue.Enqueue(request);
                    }
                }
                this.AuthenticationRequestQueue = queue;
            }
        }

        private void PruneExcesiveConnections()
        {
            ArrayList list = new ArrayList();
            lock (this.m_ConnectionList)
            {
                int connectionLimit = this.ConnectionLimit;
                if (this.CurrentConnections > connectionLimit)
                {
                    int count = this.CurrentConnections - connectionLimit;
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(this.m_ConnectionList[i]);
                    }
                    this.m_ConnectionList.RemoveRange(0, count);
                }
            }
            foreach (Connection connection in list)
            {
                connection.CloseOnIdle();
            }
        }

        private ManualResetEvent AsyncWaitHandle
        {
            get
            {
                if (this.m_Event == null)
                {
                    Interlocked.CompareExchange(ref this.m_Event, new ManualResetEvent(false), null);
                }
                return (ManualResetEvent) this.m_Event;
            }
        }

        private System.Collections.Queue AuthenticationRequestQueue
        {
            get
            {
                if (this.m_AuthenticationRequestQueue == null)
                {
                    lock (this.m_ConnectionList)
                    {
                        if (this.m_AuthenticationRequestQueue == null)
                        {
                            this.m_AuthenticationRequestQueue = new System.Collections.Queue();
                        }
                    }
                }
                return this.m_AuthenticationRequestQueue;
            }
            set
            {
                this.m_AuthenticationRequestQueue = value;
            }
        }

        internal int ConnectionLimit
        {
            get
            {
                return this.m_ConnectionLimit;
            }
            set
            {
                this.m_ConnectionLimit = value;
                this.PruneExcesiveConnections();
            }
        }

        internal int CurrentConnections
        {
            get
            {
                return this.m_ConnectionList.Count;
            }
        }

        internal System.Net.ServicePoint ServicePoint
        {
            get
            {
                return this.m_ServicePoint;
            }
        }
    }
}

