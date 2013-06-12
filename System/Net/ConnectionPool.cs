namespace System.Net
{
    using System;
    using System.Collections;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class ConnectionPool
    {
        private const int CreationHandleIndex = 2;
        private const int ErrorHandleIndex = 1;
        private const int ErrorWait = 0x1388;
        private Thread m_AsyncThread;
        private readonly TimerThread.Queue m_CleanupQueue;
        private CreateConnectionDelegate m_CreateConnectionCallback;
        private volatile bool m_ErrorOccured;
        private TimerThread.Timer m_ErrorTimer;
        private int m_MaxPoolSize;
        private int m_MinPoolSize;
        private ArrayList m_ObjectList;
        private System.Collections.Queue m_QueuedRequests;
        private Exception m_ResError;
        private System.Net.ServicePoint m_ServicePoint;
        private InterlockedStack m_StackNew;
        private InterlockedStack m_StackOld;
        private State m_State = State.Initializing;
        private int m_TotalObjects;
        private int m_WaitCount;
        private WaitHandle[] m_WaitHandles;
        private const int MaxQueueSize = 0x100000;
        private static TimerThread.Callback s_CancelErrorCallback = new TimerThread.Callback(ConnectionPool.CancelErrorCallbackWrapper);
        private static TimerThread.Queue s_CancelErrorQueue = TimerThread.GetOrCreateQueue(0x1388);
        private static TimerThread.Callback s_CleanupCallback = new TimerThread.Callback(ConnectionPool.CleanupCallbackWrapper);
        private const int SemaphoreHandleIndex = 0;
        private const int WaitAbandoned = 0x80;
        private const int WaitTimeout = 0x102;

        internal ConnectionPool(System.Net.ServicePoint servicePoint, int maxPoolSize, int minPoolSize, int idleTimeout, CreateConnectionDelegate createConnectionCallback)
        {
            this.m_CreateConnectionCallback = createConnectionCallback;
            this.m_MaxPoolSize = maxPoolSize;
            this.m_MinPoolSize = minPoolSize;
            this.m_ServicePoint = servicePoint;
            this.Initialize();
            if (idleTimeout > 0)
            {
                this.m_CleanupQueue = TimerThread.GetOrCreateQueue((idleTimeout == 1) ? 1 : (idleTimeout / 2));
                this.m_CleanupQueue.CreateTimer(s_CleanupCallback, this);
            }
        }

        internal void Abort()
        {
            if (this.m_ResError == null)
            {
                this.m_ResError = new WebException(NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled), WebExceptionStatus.RequestCanceled);
            }
            this.ErrorEvent.Set();
            this.m_ErrorOccured = true;
            this.m_ErrorTimer = s_CancelErrorQueue.CreateTimer(s_CancelErrorCallback, this);
        }

        private void AsyncThread()
        {
        Label_00B1:
            while (this.m_QueuedRequests.Count > 0)
            {
                bool continueLoop = true;
                AsyncConnectionPoolRequest request = null;
                lock (this.m_QueuedRequests)
                {
                    request = (AsyncConnectionPoolRequest) this.m_QueuedRequests.Dequeue();
                }
                WaitHandle[] waitHandles = this.m_WaitHandles;
                PooledStream pooledStream = null;
                try
                {
                    while ((pooledStream == null) && continueLoop)
                    {
                        int result = WaitHandle.WaitAny(waitHandles, request.CreationTimeout, false);
                        pooledStream = this.Get(request.OwningObject, result, ref continueLoop, ref waitHandles);
                    }
                    pooledStream.Activate(request.OwningObject, request.AsyncCallback);
                    continue;
                }
                catch (Exception exception)
                {
                    if (pooledStream != null)
                    {
                        this.PutConnection(pooledStream, request.OwningObject, request.CreationTimeout, false);
                    }
                    request.AsyncCallback(request.OwningObject, exception);
                    continue;
                }
            }
            Thread.Sleep(500);
            lock (this.m_QueuedRequests)
            {
                if (this.m_QueuedRequests.Count == 0)
                {
                    this.m_AsyncThread = null;
                }
                else
                {
                    goto Label_00B1;
                }
            }
        }

        private void CancelErrorCallback()
        {
            TimerThread.Timer errorTimer = this.m_ErrorTimer;
            if ((errorTimer != null) && errorTimer.Cancel())
            {
                this.m_ErrorOccured = false;
                this.ErrorEvent.Reset();
                this.m_ErrorTimer = null;
                this.m_ResError = null;
            }
        }

        private static void CancelErrorCallbackWrapper(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ((ConnectionPool) context).CancelErrorCallback();
        }

        private void CleanupCallback()
        {
            while (this.Count > this.MinPoolSize)
            {
                if (this.Semaphore.WaitOne(0, false))
                {
                    PooledStream pooledStream = (PooledStream) this.m_StackOld.Pop();
                    if (pooledStream != null)
                    {
                        this.Destroy(pooledStream);
                        continue;
                    }
                    this.Semaphore.ReleaseSemaphore();
                }
                break;
            }
            if (this.Semaphore.WaitOne(0, false))
            {
                while (true)
                {
                    PooledStream stream2 = (PooledStream) this.m_StackNew.Pop();
                    if (stream2 == null)
                    {
                        break;
                    }
                    this.m_StackOld.Push(stream2);
                }
                this.Semaphore.ReleaseSemaphore();
            }
        }

        private static void CleanupCallbackWrapper(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ConnectionPool pool = (ConnectionPool) context;
            try
            {
                pool.CleanupCallback();
            }
            finally
            {
                pool.m_CleanupQueue.CreateTimer(s_CleanupCallback, context);
            }
        }

        private PooledStream Create(CreateConnectionDelegate createConnectionCallback)
        {
            PooledStream stream = null;
            try
            {
                stream = createConnectionCallback(this);
                if (stream == null)
                {
                    throw new InternalException();
                }
                if (!stream.CanBePooled)
                {
                    throw new InternalException();
                }
                stream.PrePush(null);
                lock (this.m_ObjectList.SyncRoot)
                {
                    this.m_ObjectList.Add(stream);
                    this.m_TotalObjects = this.m_ObjectList.Count;
                }
            }
            catch (Exception exception)
            {
                stream = null;
                this.m_ResError = exception;
                this.Abort();
            }
            return stream;
        }

        private void Destroy(PooledStream pooledStream)
        {
            if (pooledStream != null)
            {
                try
                {
                    lock (this.m_ObjectList.SyncRoot)
                    {
                        this.m_ObjectList.Remove(pooledStream);
                        this.m_TotalObjects = this.m_ObjectList.Count;
                    }
                }
                finally
                {
                    pooledStream.Dispose();
                }
            }
        }

        internal void ForceCleanup()
        {
            if (Logging.On)
            {
                Logging.Enter(Logging.Web, "ConnectionPool::ForceCleanup");
            }
            while (this.Count > 0)
            {
                if (!this.Semaphore.WaitOne(0, false))
                {
                    break;
                }
                PooledStream pooledStream = (PooledStream) this.m_StackNew.Pop();
                if (pooledStream == null)
                {
                    pooledStream = (PooledStream) this.m_StackOld.Pop();
                }
                this.Destroy(pooledStream);
            }
            if (Logging.On)
            {
                Logging.Exit(Logging.Web, "ConnectionPool::ForceCleanup");
            }
        }

        private PooledStream Get(object owningObject, int result, ref bool continueLoop, ref WaitHandle[] waitHandles)
        {
            PooledStream fromPool = null;
            int num2 = result;
            switch (num2)
            {
                case 1:
                {
                    int num = Interlocked.Decrement(ref this.m_WaitCount);
                    continueLoop = false;
                    Exception resError = this.m_ResError;
                    if (num == 0)
                    {
                        this.CancelErrorCallback();
                    }
                    throw resError;
                }
                case 2:
                    try
                    {
                        continueLoop = true;
                        fromPool = this.UserCreateRequest();
                        if (fromPool != null)
                        {
                            fromPool.PostPop(owningObject);
                            Interlocked.Decrement(ref this.m_WaitCount);
                            continueLoop = false;
                            return fromPool;
                        }
                        if (((this.Count >= this.MaxPoolSize) && (this.MaxPoolSize != 0)) && !this.ReclaimEmancipatedObjects())
                        {
                            waitHandles = new WaitHandle[] { this.m_WaitHandles[0], this.m_WaitHandles[1] };
                        }
                        return fromPool;
                    }
                    finally
                    {
                        this.CreationMutex.ReleaseMutex();
                    }
                    break;

                default:
                    if (num2 == 0x102)
                    {
                        Interlocked.Decrement(ref this.m_WaitCount);
                        continueLoop = false;
                        throw new WebException(NetRes.GetWebStatusString("net_timeout", WebExceptionStatus.ConnectFailure), WebExceptionStatus.Timeout);
                    }
                    break;
            }
            Interlocked.Decrement(ref this.m_WaitCount);
            fromPool = this.GetFromPool(owningObject);
            continueLoop = false;
            return fromPool;
        }

        internal PooledStream GetConnection(object owningObject, GeneralAsyncDelegate asyncCallback, int creationTimeout)
        {
            int num;
            PooledStream pooledStream = null;
            bool continueLoop = true;
            bool flag2 = asyncCallback != null;
            if (this.m_State != State.Running)
            {
                throw new InternalException();
            }
            Interlocked.Increment(ref this.m_WaitCount);
            WaitHandle[] waitHandles = this.m_WaitHandles;
            if (!flag2)
            {
                while ((pooledStream == null) && continueLoop)
                {
                    num = WaitHandle.WaitAny(waitHandles, creationTimeout, false);
                    pooledStream = this.Get(owningObject, num, ref continueLoop, ref waitHandles);
                }
            }
            else
            {
                num = WaitHandle.WaitAny(waitHandles, 0, false);
                if (num != 0x102)
                {
                    pooledStream = this.Get(owningObject, num, ref continueLoop, ref waitHandles);
                }
                if (pooledStream == null)
                {
                    AsyncConnectionPoolRequest asyncRequest = new AsyncConnectionPoolRequest(this, owningObject, asyncCallback, creationTimeout);
                    this.QueueRequest(asyncRequest);
                }
            }
            if (pooledStream != null)
            {
                if (!pooledStream.IsInitalizing)
                {
                    asyncCallback = null;
                }
                try
                {
                    if (!pooledStream.Activate(owningObject, asyncCallback))
                    {
                        pooledStream = null;
                    }
                    return pooledStream;
                }
                catch
                {
                    this.PutConnection(pooledStream, owningObject, creationTimeout, false);
                    throw;
                }
            }
            if (!flag2)
            {
                throw new InternalException();
            }
            return pooledStream;
        }

        private PooledStream GetFromPool(object owningObject)
        {
            PooledStream stream = null;
            stream = (PooledStream) this.m_StackNew.Pop();
            if (stream == null)
            {
                stream = (PooledStream) this.m_StackOld.Pop();
            }
            if (stream != null)
            {
                stream.PostPop(owningObject);
            }
            return stream;
        }

        private void Initialize()
        {
            this.m_StackOld = new InterlockedStack();
            this.m_StackNew = new InterlockedStack();
            this.m_QueuedRequests = new System.Collections.Queue();
            this.m_WaitHandles = new WaitHandle[] { new System.Net.Semaphore(0, 0x100000), new ManualResetEvent(false), new Mutex() };
            this.m_ErrorTimer = null;
            this.m_ObjectList = new ArrayList();
            this.m_State = State.Running;
        }

        internal void PutConnection(PooledStream pooledStream, object owningObject, int creationTimeout)
        {
            this.PutConnection(pooledStream, owningObject, creationTimeout, true);
        }

        internal void PutConnection(PooledStream pooledStream, object owningObject, int creationTimeout, bool canReuse)
        {
            if (pooledStream == null)
            {
                throw new ArgumentNullException("pooledStream");
            }
            pooledStream.PrePush(owningObject);
            if (this.m_State != State.ShuttingDown)
            {
                pooledStream.Deactivate();
                if (this.m_WaitCount == 0)
                {
                    this.CancelErrorCallback();
                }
                if (canReuse && pooledStream.CanBePooled)
                {
                    this.PutNew(pooledStream);
                }
                else
                {
                    this.Destroy(pooledStream);
                    if (this.m_WaitCount > 0)
                    {
                        if (!this.CreationMutex.WaitOne(creationTimeout, false))
                        {
                            this.Abort();
                        }
                        else
                        {
                            try
                            {
                                pooledStream = this.UserCreateRequest();
                                if (pooledStream != null)
                                {
                                    this.PutNew(pooledStream);
                                }
                            }
                            finally
                            {
                                this.CreationMutex.ReleaseMutex();
                            }
                        }
                    }
                }
            }
            else
            {
                this.Destroy(pooledStream);
            }
        }

        private void PutNew(PooledStream pooledStream)
        {
            this.m_StackNew.Push(pooledStream);
            this.Semaphore.ReleaseSemaphore();
        }

        private void QueueRequest(AsyncConnectionPoolRequest asyncRequest)
        {
            lock (this.m_QueuedRequests)
            {
                this.m_QueuedRequests.Enqueue(asyncRequest);
                if (this.m_AsyncThread == null)
                {
                    this.m_AsyncThread = new Thread(new ThreadStart(this.AsyncThread));
                    this.m_AsyncThread.IsBackground = true;
                    this.m_AsyncThread.Start();
                }
            }
        }

        private bool ReclaimEmancipatedObjects()
        {
            bool flag = false;
            lock (this.m_ObjectList.SyncRoot)
            {
                object[] objArray = this.m_ObjectList.ToArray();
                if (objArray == null)
                {
                    return flag;
                }
                for (int i = 0; i < objArray.Length; i++)
                {
                    PooledStream stream = (PooledStream) objArray[i];
                    if (stream != null)
                    {
                        bool lockTaken = false;
                        try
                        {
                            Monitor.TryEnter(stream, ref lockTaken);
                            if (lockTaken && stream.IsEmancipated)
                            {
                                this.PutConnection(stream, null, -1);
                                flag = true;
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(stream);
                            }
                        }
                    }
                }
            }
            return flag;
        }

        private PooledStream UserCreateRequest()
        {
            PooledStream stream = null;
            if (this.ErrorOccurred || ((this.Count >= this.MaxPoolSize) && (this.MaxPoolSize != 0)))
            {
                return stream;
            }
            if (((this.Count & 1) != 1) && this.ReclaimEmancipatedObjects())
            {
                return stream;
            }
            return this.Create(this.m_CreateConnectionCallback);
        }

        internal int Count
        {
            get
            {
                return this.m_TotalObjects;
            }
        }

        private Mutex CreationMutex
        {
            get
            {
                return (Mutex) this.m_WaitHandles[2];
            }
        }

        private ManualResetEvent ErrorEvent
        {
            get
            {
                return (ManualResetEvent) this.m_WaitHandles[1];
            }
        }

        private bool ErrorOccurred
        {
            get
            {
                return this.m_ErrorOccured;
            }
        }

        internal int MaxPoolSize
        {
            get
            {
                return this.m_MaxPoolSize;
            }
        }

        internal int MinPoolSize
        {
            get
            {
                return this.m_MinPoolSize;
            }
        }

        private System.Net.Semaphore Semaphore
        {
            get
            {
                return (System.Net.Semaphore) this.m_WaitHandles[0];
            }
        }

        internal System.Net.ServicePoint ServicePoint
        {
            get
            {
                return this.m_ServicePoint;
            }
        }

        private class AsyncConnectionPoolRequest
        {
            public GeneralAsyncDelegate AsyncCallback;
            public bool Completed;
            public int CreationTimeout;
            public object OwningObject;
            public ConnectionPool Pool;

            public AsyncConnectionPoolRequest(ConnectionPool pool, object owningObject, GeneralAsyncDelegate asyncCallback, int creationTimeout)
            {
                this.Pool = pool;
                this.OwningObject = owningObject;
                this.AsyncCallback = asyncCallback;
                this.CreationTimeout = creationTimeout;
            }
        }

        private enum State
        {
            Initializing,
            Running,
            ShuttingDown
        }
    }
}

