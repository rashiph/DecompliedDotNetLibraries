namespace System.Data.ProviderBase
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;
    using System.Transactions;

    internal sealed class DbConnectionPool
    {
        private Timer _cleanupTimer;
        private readonly int _cleanupWait;
        private readonly System.Data.ProviderBase.DbConnectionFactory _connectionFactory;
        private readonly System.Data.ProviderBase.DbConnectionPoolGroup _connectionPoolGroup;
        private readonly System.Data.ProviderBase.DbConnectionPoolGroupOptions _connectionPoolGroupOptions;
        private System.Data.ProviderBase.DbConnectionPoolProviderInfo _connectionPoolProviderInfo;
        private readonly WaitCallback _deactivateCallback;
        private readonly Queue _deactivateQueue;
        private volatile bool _errorOccurred;
        private Timer _errorTimer;
        private int _errorWait;
        private readonly System.Data.ProviderBase.DbConnectionPoolIdentity _identity;
        internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
        private readonly List<System.Data.ProviderBase.DbConnectionInternal> _objectList;
        private static int _objectTypeCount;
        private readonly WaitCallback _poolCreateRequest;
        private static readonly Random _random = new Random(0x4dd999);
        private Exception _resError;
        private readonly DbConnectionInternalListStack _stackNew = new DbConnectionInternalListStack();
        private readonly DbConnectionInternalListStack _stackOld = new DbConnectionInternalListStack();
        private State _state;
        private int _totalObjects;
        private readonly TransactedConnectionPool _transactedConnectionPool;
        private int _waitCount;
        private readonly PoolWaitHandles _waitHandles;
        private const int BOGUS_HANDLE = 3;
        private const int CREATION_HANDLE = 2;
        private const int ERROR_HANDLE = 1;
        private const int ERROR_WAIT_DEFAULT = 0x1388;
        private const int MAX_Q_SIZE = 0x100000;
        internal const Bid.ApiGroup PoolerTracePoints = Bid.ApiGroup.Pooling;
        private const int SEMAPHORE_HANDLE = 0;
        private const int WAIT_ABANDONED = 0x80;
        private const int WAIT_FAILED = -1;
        private const int WAIT_OBJECT_0 = 0;
        private const int WAIT_TIMEOUT = 0x102;

        internal DbConnectionPool(System.Data.ProviderBase.DbConnectionFactory connectionFactory, System.Data.ProviderBase.DbConnectionPoolGroup connectionPoolGroup, System.Data.ProviderBase.DbConnectionPoolIdentity identity, System.Data.ProviderBase.DbConnectionPoolProviderInfo connectionPoolProviderInfo)
        {
            if ((identity != null) && identity.IsRestricted)
            {
                throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.AttemptingToPoolOnRestrictedToken);
            }
            this._state = State.Initializing;
            lock (_random)
            {
                this._cleanupWait = (_random.Next(12, 0x18) * 10) * 0x3e8;
            }
            this._connectionFactory = connectionFactory;
            this._connectionPoolGroup = connectionPoolGroup;
            this._connectionPoolGroupOptions = connectionPoolGroup.PoolGroupOptions;
            this._connectionPoolProviderInfo = connectionPoolProviderInfo;
            this._identity = identity;
            if (this.UseDeactivateQueue)
            {
                this._deactivateQueue = new Queue();
                this._deactivateCallback = new WaitCallback(this.ProcessDeactivateQueue);
            }
            this._waitHandles = new PoolWaitHandles();
            this._errorWait = 0x1388;
            this._errorTimer = null;
            this._objectList = new List<System.Data.ProviderBase.DbConnectionInternal>(this.MaxPoolSize);
            if (System.Data.Common.ADP.IsPlatformNT5)
            {
                this._transactedConnectionPool = new TransactedConnectionPool(this);
            }
            this._poolCreateRequest = new WaitCallback(this.PoolCreateRequest);
            this._state = State.Running;
            Bid.PoolerTrace("<prov.DbConnectionPool.DbConnectionPool|RES|CPOOL> %d#, Constructed.\n", this.ObjectID);
        }

        private void CleanupCallback(object state)
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.CleanupCallback|RES|INFO|CPOOL> %d#\n", this.ObjectID);
            while (this.Count > this.MinPoolSize)
            {
                if (this._waitHandles.PoolSemaphore.WaitOne(0, false))
                {
                    System.Data.ProviderBase.DbConnectionInternal internal2 = this._stackOld.SynchronizedPop();
                    if (internal2 != null)
                    {
                        this.PerformanceCounters.NumberOfFreeConnections.Decrement();
                        bool flag = true;
                        lock (internal2)
                        {
                            if (internal2.IsTransactionRoot)
                            {
                                flag = false;
                            }
                        }
                        if (flag)
                        {
                            this.DestroyObject(internal2);
                        }
                        else
                        {
                            internal2.SetInStasis();
                        }
                        continue;
                    }
                    this._waitHandles.PoolSemaphore.Release(1);
                }
                break;
            }
            if (this._waitHandles.PoolSemaphore.WaitOne(0, false))
            {
                while (true)
                {
                    System.Data.ProviderBase.DbConnectionInternal internal3 = this._stackNew.SynchronizedPop();
                    if (internal3 == null)
                    {
                        break;
                    }
                    Bid.PoolerTrace("<prov.DbConnectionPool.CleanupCallback|RES|INFO|CPOOL> %d#, ChangeStacks=%d#\n", this.ObjectID, internal3.ObjectID);
                    this._stackOld.SynchronizedPush(internal3);
                }
                this._waitHandles.PoolSemaphore.Release(1);
            }
            this.QueuePoolCreateRequest();
        }

        internal void Clear()
        {
            System.Data.ProviderBase.DbConnectionInternal internal2;
            Bid.PoolerTrace("<prov.DbConnectionPool.Clear|RES|CPOOL> %d#, Clearing.\n", this.ObjectID);
            lock (this._objectList)
            {
                int count = this._objectList.Count;
                for (int i = 0; i < count; i++)
                {
                    internal2 = this._objectList[i];
                    if (internal2 != null)
                    {
                        internal2.DoNotPoolThisConnection();
                    }
                }
                goto Label_0074;
            }
        Label_005D:
            this.PerformanceCounters.NumberOfFreeConnections.Decrement();
            this.DestroyObject(internal2);
        Label_0074:
            if ((internal2 = this._stackNew.SynchronizedPop()) != null)
            {
                goto Label_005D;
            }
            while ((internal2 = this._stackOld.SynchronizedPop()) != null)
            {
                this.PerformanceCounters.NumberOfFreeConnections.Decrement();
                this.DestroyObject(internal2);
            }
            this.ReclaimEmancipatedObjects();
            Bid.PoolerTrace("<prov.DbConnectionPool.Clear|RES|CPOOL> %d#, Cleared.\n", this.ObjectID);
        }

        private Timer CreateCleanupTimer()
        {
            return new Timer(new TimerCallback(this.CleanupCallback), null, this._cleanupWait, this._cleanupWait);
        }

        private System.Data.ProviderBase.DbConnectionInternal CreateObject(DbConnection owningObject)
        {
            System.Data.ProviderBase.DbConnectionInternal item = null;
            try
            {
                item = this._connectionFactory.CreatePooledConnection(owningObject, this, this._connectionPoolGroup.ConnectionOptions);
                if (item == null)
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.CreateObjectReturnedNull);
                }
                if (!item.CanBePooled)
                {
                    throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.NewObjectCannotBePooled);
                }
                item.PrePush(null);
                lock (this._objectList)
                {
                    this._objectList.Add(item);
                    this._totalObjects = this._objectList.Count;
                    this.PerformanceCounters.NumberOfPooledConnections.Increment();
                }
                Bid.PoolerTrace("<prov.DbConnectionPool.CreateObject|RES|CPOOL> %d#, Connection %d#, Added to pool.\n", this.ObjectID, item.ObjectID);
                this._errorWait = 0x1388;
            }
            catch (Exception exception)
            {
                if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                {
                    throw;
                }
                System.Data.Common.ADP.TraceExceptionForCapture(exception);
                item = null;
                this._resError = exception;
                Timer timer = new Timer(new TimerCallback(this.ErrorCallback), null, -1, -1);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    this._waitHandles.ErrorEvent.Set();
                    this._errorOccurred = true;
                    this._errorTimer = timer;
                    timer.Change(this._errorWait, this._errorWait);
                }
                if (0x7530 < this._errorWait)
                {
                    this._errorWait = 0xea60;
                }
                else
                {
                    this._errorWait *= 2;
                }
                throw;
            }
            return item;
        }

        private void DeactivateObject(System.Data.ProviderBase.DbConnectionInternal obj)
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.DeactivateObject|RES|CPOOL> %d#, Connection %d#, Deactivating.\n", this.ObjectID, obj.ObjectID);
            obj.DeactivateConnection();
            bool flag2 = false;
            bool flag = false;
            if (obj.IsConnectionDoomed)
            {
                flag = true;
            }
            else
            {
                lock (obj)
                {
                    if (this._state == State.ShuttingDown)
                    {
                        if (obj.IsTransactionRoot)
                        {
                            obj.SetInStasis();
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    else if (obj.IsNonPoolableTransactionRoot)
                    {
                        obj.SetInStasis();
                    }
                    else if (obj.CanBePooled)
                    {
                        Transaction enlistedTransaction = obj.EnlistedTransaction;
                        if (null != enlistedTransaction)
                        {
                            this._transactedConnectionPool.PutTransactedObject(enlistedTransaction, obj);
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    else if (obj.IsTransactionRoot && !obj.IsConnectionDoomed)
                    {
                        obj.SetInStasis();
                    }
                    else
                    {
                        flag = true;
                    }
                }
            }
            if (flag2)
            {
                this.PutNewObject(obj);
            }
            else if (flag)
            {
                this.DestroyObject(obj);
                this.QueuePoolCreateRequest();
            }
        }

        internal void DestroyObject(System.Data.ProviderBase.DbConnectionInternal obj)
        {
            if (obj.IsTxRootWaitingForTxEnd)
            {
                Bid.PoolerTrace("<prov.DbConnectionPool.DestroyObject|RES|CPOOL> %d#, Connection %d#, Has Delegated Transaction, waiting to Dispose.\n", this.ObjectID, obj.ObjectID);
            }
            else
            {
                Bid.PoolerTrace("<prov.DbConnectionPool.DestroyObject|RES|CPOOL> %d#, Connection %d#, Removing from pool.\n", this.ObjectID, obj.ObjectID);
                bool flag = false;
                lock (this._objectList)
                {
                    flag = this._objectList.Remove(obj);
                    this._totalObjects = this._objectList.Count;
                }
                if (flag)
                {
                    Bid.PoolerTrace("<prov.DbConnectionPool.DestroyObject|RES|CPOOL> %d#, Connection %d#, Removed from pool.\n", this.ObjectID, obj.ObjectID);
                    this.PerformanceCounters.NumberOfPooledConnections.Decrement();
                }
                obj.Dispose();
                Bid.PoolerTrace("<prov.DbConnectionPool.DestroyObject|RES|CPOOL> %d#, Connection %d#, Disposed.\n", this.ObjectID, obj.ObjectID);
                this.PerformanceCounters.HardDisconnectsPerSecond.Increment();
            }
        }

        private void ErrorCallback(object state)
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.ErrorCallback|RES|CPOOL> %d#, Resetting Error handling.\n", this.ObjectID);
            this._errorOccurred = false;
            this._waitHandles.ErrorEvent.Reset();
            Timer timer = this._errorTimer;
            this._errorTimer = null;
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        internal System.Data.ProviderBase.DbConnectionInternal GetConnection(DbConnection owningObject)
        {
            System.Data.ProviderBase.DbConnectionInternal fromTransactedPool = null;
            int num;
            Transaction transaction = null;
            this.PerformanceCounters.SoftConnectsPerSecond.Increment();
            if (this._state != State.Running)
            {
                Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, DbConnectionInternal State != Running.\n", this.ObjectID);
                return null;
            }
            Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Getting connection.\n", this.ObjectID);
            if (this.HasTransactionAffinity)
            {
                fromTransactedPool = this.GetFromTransactedPool(out transaction);
            }
            if (fromTransactedPool != null)
            {
                goto Label_02EF;
            }
            Interlocked.Increment(ref this._waitCount);
            uint nCount = 3;
            uint creationTimeout = (uint) this.CreationTimeout;
            if (creationTimeout == 0)
            {
                creationTimeout = uint.MaxValue;
            }
        Label_0075:
            num = 3;
            int errorCode = 0;
            bool success = false;
            int num5 = 0;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this._waitHandles.DangerousAddRef(ref success);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    num = System.Data.Common.SafeNativeMethods.WaitForMultipleObjectsEx(nCount, this._waitHandles.DangerousGetHandle(), false, creationTimeout, false);
                    if (num == -1)
                    {
                        num5 = Marshal.GetHRForLastWin32Error();
                    }
                }
                switch (num)
                {
                    case -1:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Wait failed.\n", this.ObjectID);
                        Interlocked.Decrement(ref this._waitCount);
                        Marshal.ThrowExceptionForHR(num5);
                        break;

                    case 0:
                        Interlocked.Decrement(ref this._waitCount);
                        fromTransactedPool = this.GetFromGeneralPool();
                        goto Label_02E0;

                    case 1:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Errors are set.\n", this.ObjectID);
                        Interlocked.Decrement(ref this._waitCount);
                        throw this._resError;

                    case 2:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Creating new connection.\n", this.ObjectID);
                        try
                        {
                            fromTransactedPool = this.UserCreateRequest(owningObject);
                        }
                        catch
                        {
                            if (fromTransactedPool == null)
                            {
                                Interlocked.Decrement(ref this._waitCount);
                            }
                            throw;
                        }
                        finally
                        {
                            if (fromTransactedPool != null)
                            {
                                Interlocked.Decrement(ref this._waitCount);
                            }
                        }
                        if (((fromTransactedPool == null) && (this.Count >= this.MaxPoolSize)) && ((this.MaxPoolSize != 0) && !this.ReclaimEmancipatedObjects()))
                        {
                            nCount = 2;
                        }
                        goto Label_02E0;

                    case 0x80:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Semaphore handle abandonded.\n", this.ObjectID);
                        Interlocked.Decrement(ref this._waitCount);
                        throw new AbandonedMutexException(0, this._waitHandles.PoolSemaphore);

                    case 0x81:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Error handle abandonded.\n", this.ObjectID);
                        Interlocked.Decrement(ref this._waitCount);
                        throw new AbandonedMutexException(1, this._waitHandles.ErrorEvent);

                    case 130:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Creation handle abandoned.\n", this.ObjectID);
                        Interlocked.Decrement(ref this._waitCount);
                        throw new AbandonedMutexException(2, this._waitHandles.CreationSemaphore);

                    case 0x102:
                        Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, Wait timed out.\n", this.ObjectID);
                        Interlocked.Decrement(ref this._waitCount);
                        return null;
                }
                Bid.PoolerTrace("<prov.DbConnectionPool.GetConnection|RES|CPOOL> %d#, WaitForMultipleObjects=%d\n", this.ObjectID, num);
                Interlocked.Decrement(ref this._waitCount);
                throw System.Data.Common.ADP.InternalError(System.Data.Common.ADP.InternalErrorCode.UnexpectedWaitAnyResult);
            }
            finally
            {
                if ((2 == num) && (System.Data.Common.SafeNativeMethods.ReleaseSemaphore(this._waitHandles.CreationHandle.DangerousGetHandle(), 1, IntPtr.Zero) == 0))
                {
                    errorCode = Marshal.GetHRForLastWin32Error();
                }
                if (success)
                {
                    this._waitHandles.DangerousRelease();
                }
            }
        Label_02E0:
            if (errorCode != 0)
            {
                Marshal.ThrowExceptionForHR(errorCode);
            }
            if (fromTransactedPool == null)
            {
                goto Label_0075;
            }
        Label_02EF:
            if (fromTransactedPool != null)
            {
                lock (fromTransactedPool)
                {
                    fromTransactedPool.PostPop(owningObject);
                }
                try
                {
                    fromTransactedPool.ActivateConnection(transaction);
                }
                catch (SecurityException)
                {
                    this.PutObject(fromTransactedPool, owningObject);
                    throw;
                }
            }
            return fromTransactedPool;
        }

        private System.Data.ProviderBase.DbConnectionInternal GetFromGeneralPool()
        {
            System.Data.ProviderBase.DbConnectionInternal internal2 = null;
            internal2 = this._stackNew.SynchronizedPop();
            if (internal2 == null)
            {
                internal2 = this._stackOld.SynchronizedPop();
            }
            if (internal2 != null)
            {
                Bid.PoolerTrace("<prov.DbConnectionPool.GetFromGeneralPool|RES|CPOOL> %d#, Connection %d#, Popped from general pool.\n", this.ObjectID, internal2.ObjectID);
                this.PerformanceCounters.NumberOfFreeConnections.Decrement();
            }
            return internal2;
        }

        private System.Data.ProviderBase.DbConnectionInternal GetFromTransactedPool(out Transaction transaction)
        {
            transaction = System.Data.Common.ADP.GetCurrentTransaction();
            System.Data.ProviderBase.DbConnectionInternal transactedObject = null;
            if ((null != transaction) && (this._transactedConnectionPool != null))
            {
                transactedObject = this._transactedConnectionPool.GetTransactedObject(transaction);
                if (transactedObject != null)
                {
                    Bid.PoolerTrace("<prov.DbConnectionPool.GetFromTransactedPool|RES|CPOOL> %d#, Connection %d#, Popped from transacted pool.\n", this.ObjectID, transactedObject.ObjectID);
                    this.PerformanceCounters.NumberOfFreeConnections.Decrement();
                }
            }
            return transactedObject;
        }

        private void PoolCreateRequest(object state)
        {
            IntPtr ptr;
            Bid.PoolerScopeEnter(out ptr, "<prov.DbConnectionPool.PoolCreateRequest|RES|INFO|CPOOL> %d#\n", this.ObjectID);
            try
            {
                if (State.Running == this._state)
                {
                    this.ReclaimEmancipatedObjects();
                    if ((!this.ErrorOccurred && this.NeedToReplenish) && (!this.UsingIntegrateSecurity || this._identity.Equals(System.Data.ProviderBase.DbConnectionPoolIdentity.GetCurrent())))
                    {
                        bool success = false;
                        int num = 3;
                        uint creationTimeout = (uint) this.CreationTimeout;
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try
                        {
                            this._waitHandles.DangerousAddRef(ref success);
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                            }
                            finally
                            {
                                num = System.Data.Common.SafeNativeMethods.WaitForSingleObjectEx(this._waitHandles.CreationHandle.DangerousGetHandle(), creationTimeout, false);
                            }
                            if (num == 0)
                            {
                                if (!this.ErrorOccurred)
                                {
                                    while (this.NeedToReplenish)
                                    {
                                        System.Data.ProviderBase.DbConnectionInternal internal2 = this.CreateObject(null);
                                        if (internal2 == null)
                                        {
                                            return;
                                        }
                                        this.PutNewObject(internal2);
                                    }
                                }
                            }
                            else if (0x102 == num)
                            {
                                this.QueuePoolCreateRequest();
                            }
                            else
                            {
                                Bid.PoolerTrace("<prov.DbConnectionPool.PoolCreateRequest|RES|CPOOL> %d#, PoolCreateRequest called WaitForSingleObject failed %d", this.ObjectID, num);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (!System.Data.Common.ADP.IsCatchableExceptionType(exception))
                            {
                                throw;
                            }
                            Bid.PoolerTrace("<prov.DbConnectionPool.PoolCreateRequest|RES|CPOOL> %d#, PoolCreateRequest called CreateConnection which threw an exception: %ls", this.ObjectID, exception);
                        }
                        finally
                        {
                            if (num == 0)
                            {
                                num = System.Data.Common.SafeNativeMethods.ReleaseSemaphore(this._waitHandles.CreationHandle.DangerousGetHandle(), 1, IntPtr.Zero);
                            }
                            if (success)
                            {
                                this._waitHandles.DangerousRelease();
                            }
                        }
                    }
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        private void ProcessDeactivateQueue(object state)
        {
            IntPtr ptr;
            Bid.PoolerScopeEnter(out ptr, "<prov.DbConnectionPool.ProcessDeactivateQueue|RES|INFO|CPOOL> %d#\n", this.ObjectID);
            try
            {
                object[] objArray2;
                lock (this._deactivateQueue.SyncRoot)
                {
                    objArray2 = this._deactivateQueue.ToArray();
                    this._deactivateQueue.Clear();
                }
                foreach (System.Data.ProviderBase.DbConnectionInternal internal2 in objArray2)
                {
                    this.PerformanceCounters.NumberOfStasisConnections.Decrement();
                    this.DeactivateObject(internal2);
                }
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        internal void PutNewObject(System.Data.ProviderBase.DbConnectionInternal obj)
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.PutNewObject|RES|CPOOL> %d#, Connection %d#, Pushing to general pool.\n", this.ObjectID, obj.ObjectID);
            this._stackNew.SynchronizedPush(obj);
            this._waitHandles.PoolSemaphore.Release(1);
            this.PerformanceCounters.NumberOfFreeConnections.Increment();
        }

        internal void PutObject(System.Data.ProviderBase.DbConnectionInternal obj, object owningObject)
        {
            this.PerformanceCounters.SoftDisconnectsPerSecond.Increment();
            lock (obj)
            {
                obj.PrePush(owningObject);
            }
            if (this.UseDeactivateQueue)
            {
                bool flag;
                Bid.PoolerTrace("<prov.DbConnectionPool.PutObject|RES|CPOOL> %d#, Connection %d#, Queueing for deactivation.\n", this.ObjectID, obj.ObjectID);
                this.PerformanceCounters.NumberOfStasisConnections.Increment();
                lock (this._deactivateQueue.SyncRoot)
                {
                    flag = 0 == this._deactivateQueue.Count;
                    this._deactivateQueue.Enqueue(obj);
                }
                if (flag)
                {
                    ThreadPool.QueueUserWorkItem(this._deactivateCallback, null);
                }
            }
            else
            {
                this.DeactivateObject(obj);
            }
        }

        internal void PutObjectFromTransactedPool(System.Data.ProviderBase.DbConnectionInternal obj)
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.PutObjectFromTransactedPool|RES|CPOOL> %d#, Connection %d#, Transaction has ended.\n", this.ObjectID, obj.ObjectID);
            if ((this._state == State.Running) && obj.CanBePooled)
            {
                this.PutNewObject(obj);
            }
            else
            {
                this.DestroyObject(obj);
                this.QueuePoolCreateRequest();
            }
        }

        private void QueuePoolCreateRequest()
        {
            if (State.Running == this._state)
            {
                ThreadPool.QueueUserWorkItem(this._poolCreateRequest);
            }
        }

        private bool ReclaimEmancipatedObjects()
        {
            int count;
            bool flag2 = false;
            Bid.PoolerTrace("<prov.DbConnectionPool.ReclaimEmancipatedObjects|RES|CPOOL> %d#\n", this.ObjectID);
            List<System.Data.ProviderBase.DbConnectionInternal> list = new List<System.Data.ProviderBase.DbConnectionInternal>();
            lock (this._objectList)
            {
                count = this._objectList.Count;
                for (int j = 0; j < count; j++)
                {
                    System.Data.ProviderBase.DbConnectionInternal internal2 = this._objectList[j];
                    if (internal2 != null)
                    {
                        bool lockTaken = false;
                        try
                        {
                            Monitor.TryEnter(internal2, ref lockTaken);
                            if (lockTaken && internal2.IsEmancipated)
                            {
                                internal2.PrePush(null);
                                list.Add(internal2);
                            }
                        }
                        finally
                        {
                            if (lockTaken)
                            {
                                Monitor.Exit(internal2);
                            }
                        }
                    }
                }
            }
            count = list.Count;
            for (int i = 0; i < count; i++)
            {
                System.Data.ProviderBase.DbConnectionInternal internal3 = list[i];
                Bid.PoolerTrace("<prov.DbConnectionPool.ReclaimEmancipatedObjects|RES|CPOOL> %d#, Connection %d#, Reclaiming.\n", this.ObjectID, internal3.ObjectID);
                this.PerformanceCounters.NumberOfReclaimedConnections.Increment();
                flag2 = true;
                this.DeactivateObject(internal3);
            }
            return flag2;
        }

        internal void Shutdown()
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.Shutdown|RES|INFO|CPOOL> %d#\n", this.ObjectID);
            this._state = State.ShuttingDown;
            Timer timer = this._cleanupTimer;
            this._cleanupTimer = null;
            if (timer != null)
            {
                timer.Dispose();
            }
            timer = this._errorTimer;
            this._errorTimer = null;
            if (timer != null)
            {
                timer.Dispose();
            }
        }

        internal void Startup()
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.Startup|RES|INFO|CPOOL> %d#, CleanupWait=%d\n", this.ObjectID, this._cleanupWait);
            this._cleanupTimer = this.CreateCleanupTimer();
            if (this.NeedToReplenish)
            {
                this.QueuePoolCreateRequest();
            }
        }

        internal void TransactionEnded(Transaction transaction, System.Data.ProviderBase.DbConnectionInternal transactedObject)
        {
            Bid.PoolerTrace("<prov.DbConnectionPool.TransactionEnded|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Transaction Completed\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
            TransactedConnectionPool pool = this._transactedConnectionPool;
            if (pool != null)
            {
                pool.TransactionEnded(transaction, transactedObject);
            }
        }

        private System.Data.ProviderBase.DbConnectionInternal UserCreateRequest(DbConnection owningObject)
        {
            System.Data.ProviderBase.DbConnectionInternal internal2 = null;
            if (this.ErrorOccurred)
            {
                throw this._resError;
            }
            if ((this.Count >= this.MaxPoolSize) && (this.MaxPoolSize != 0))
            {
                return internal2;
            }
            if (((this.Count & 1) != 1) && this.ReclaimEmancipatedObjects())
            {
                return internal2;
            }
            return this.CreateObject(owningObject);
        }

        internal System.Data.ProviderBase.DbConnectionFactory ConnectionFactory
        {
            get
            {
                return this._connectionFactory;
            }
        }

        internal int Count
        {
            get
            {
                return this._totalObjects;
            }
        }

        private int CreationTimeout
        {
            get
            {
                return this.PoolGroupOptions.CreationTimeout;
            }
        }

        internal bool ErrorOccurred
        {
            get
            {
                return this._errorOccurred;
            }
        }

        private bool HasTransactionAffinity
        {
            get
            {
                return this.PoolGroupOptions.HasTransactionAffinity;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolIdentity Identity
        {
            get
            {
                return this._identity;
            }
        }

        internal bool IsRunning
        {
            get
            {
                return (State.Running == this._state);
            }
        }

        internal TimeSpan LoadBalanceTimeout
        {
            get
            {
                return this.PoolGroupOptions.LoadBalanceTimeout;
            }
        }

        private int MaxPoolSize
        {
            get
            {
                return this.PoolGroupOptions.MaxPoolSize;
            }
        }

        private int MinPoolSize
        {
            get
            {
                return this.PoolGroupOptions.MinPoolSize;
            }
        }

        private bool NeedToReplenish
        {
            get
            {
                if (State.Running != this._state)
                {
                    return false;
                }
                int count = this.Count;
                if (count >= this.MaxPoolSize)
                {
                    return false;
                }
                if (count < this.MinPoolSize)
                {
                    return true;
                }
                int num3 = this._stackNew.Count + this._stackOld.Count;
                int num2 = this._waitCount;
                return ((num3 < num2) || ((num3 == num2) && (count > 1)));
            }
        }

        internal int ObjectID
        {
            get
            {
                return this._objectID;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolCounters PerformanceCounters
        {
            get
            {
                return this._connectionFactory.PerformanceCounters;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolGroup PoolGroup
        {
            get
            {
                return this._connectionPoolGroup;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolGroupOptions PoolGroupOptions
        {
            get
            {
                return this._connectionPoolGroupOptions;
            }
        }

        internal System.Data.ProviderBase.DbConnectionPoolProviderInfo ProviderInfo
        {
            get
            {
                return this._connectionPoolProviderInfo;
            }
        }

        private bool UseDeactivateQueue
        {
            get
            {
                return this.PoolGroupOptions.UseDeactivateQueue;
            }
        }

        internal bool UseLoadBalancing
        {
            get
            {
                return this.PoolGroupOptions.UseLoadBalancing;
            }
        }

        private bool UsingIntegrateSecurity
        {
            get
            {
                return ((this._identity != null) && (System.Data.ProviderBase.DbConnectionPoolIdentity.NoIdentity != this._identity));
            }
        }

        private class DbConnectionInternalListStack
        {
            private System.Data.ProviderBase.DbConnectionInternal _stack;

            internal DbConnectionInternalListStack()
            {
            }

            internal System.Data.ProviderBase.DbConnectionInternal SynchronizedPop()
            {
                System.Data.ProviderBase.DbConnectionInternal internal2;
                lock (this)
                {
                    internal2 = this._stack;
                    if (internal2 != null)
                    {
                        this._stack = internal2.NextPooledObject;
                        internal2.NextPooledObject = null;
                    }
                }
                return internal2;
            }

            internal void SynchronizedPush(System.Data.ProviderBase.DbConnectionInternal value)
            {
                lock (this)
                {
                    value.NextPooledObject = this._stack;
                    this._stack = value;
                }
            }

            internal int Count
            {
                get
                {
                    int num = 0;
                    lock (this)
                    {
                        for (System.Data.ProviderBase.DbConnectionInternal internal2 = this._stack; internal2 != null; internal2 = internal2.NextPooledObject)
                        {
                            num++;
                        }
                    }
                    return num;
                }
            }
        }

        private sealed class PoolWaitHandles : System.Data.ProviderBase.DbBuffer
        {
            private readonly SafeHandle _creationHandle;
            private readonly Semaphore _creationSemaphore;
            private readonly ManualResetEvent _errorEvent;
            private readonly SafeHandle _errorHandle;
            private readonly SafeHandle _poolHandle;
            private readonly Semaphore _poolSemaphore;
            private readonly int _releaseFlags;

            internal PoolWaitHandles() : base(3 * IntPtr.Size)
            {
                bool success = false;
                bool flag2 = false;
                bool flag = false;
                this._poolSemaphore = new Semaphore(0, 0x100000);
                this._errorEvent = new ManualResetEvent(false);
                this._creationSemaphore = new Semaphore(1, 1);
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this._poolHandle = this._poolSemaphore.SafeWaitHandle;
                    this._errorHandle = this._errorEvent.SafeWaitHandle;
                    this._creationHandle = this._creationSemaphore.SafeWaitHandle;
                    this._poolHandle.DangerousAddRef(ref success);
                    this._errorHandle.DangerousAddRef(ref flag2);
                    this._creationHandle.DangerousAddRef(ref flag);
                    int size = IntPtr.Size;
                    base.WriteIntPtr(0, this._poolHandle.DangerousGetHandle());
                    base.WriteIntPtr(IntPtr.Size, this._errorHandle.DangerousGetHandle());
                    base.WriteIntPtr(2 * IntPtr.Size, this._creationHandle.DangerousGetHandle());
                }
                finally
                {
                    if (success)
                    {
                        this._releaseFlags |= 1;
                    }
                    if (flag2)
                    {
                        this._releaseFlags |= 2;
                    }
                    if (flag)
                    {
                        this._releaseFlags |= 4;
                    }
                }
            }

            protected override bool ReleaseHandle()
            {
                if ((1 & this._releaseFlags) != 0)
                {
                    this._poolHandle.DangerousRelease();
                }
                if ((2 & this._releaseFlags) != 0)
                {
                    this._errorHandle.DangerousRelease();
                }
                if ((4 & this._releaseFlags) != 0)
                {
                    this._creationHandle.DangerousRelease();
                }
                return base.ReleaseHandle();
            }

            internal SafeHandle CreationHandle
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
                get
                {
                    return this._creationHandle;
                }
            }

            internal Semaphore CreationSemaphore
            {
                get
                {
                    return this._creationSemaphore;
                }
            }

            internal ManualResetEvent ErrorEvent
            {
                get
                {
                    return this._errorEvent;
                }
            }

            internal Semaphore PoolSemaphore
            {
                get
                {
                    return this._poolSemaphore;
                }
            }
        }

        private enum State
        {
            Initializing,
            Running,
            ShuttingDown
        }

        private sealed class TransactedConnectionList : List<System.Data.ProviderBase.DbConnectionInternal>
        {
            private Transaction _transaction;

            internal TransactedConnectionList(int initialAllocation, Transaction tx) : base(initialAllocation)
            {
                this._transaction = tx;
            }

            internal void Dispose()
            {
                if (null != this._transaction)
                {
                    this._transaction.Dispose();
                }
            }
        }

        private sealed class TransactedConnectionPool
        {
            internal readonly int _objectID = Interlocked.Increment(ref _objectTypeCount);
            private static int _objectTypeCount;
            private System.Data.ProviderBase.DbConnectionPool _pool;
            private Dictionary<Transaction, System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList> _transactedCxns;

            internal TransactedConnectionPool(System.Data.ProviderBase.DbConnectionPool pool)
            {
                this._pool = pool;
                this._transactedCxns = new Dictionary<Transaction, System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList>();
                Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.TransactedConnectionPool|RES|CPOOL> %d#, Constructed for connection pool %d#\n", this.ObjectID, this._pool.ObjectID);
            }

            internal System.Data.ProviderBase.DbConnectionInternal GetTransactedObject(Transaction transaction)
            {
                System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList list;
                System.Data.ProviderBase.DbConnectionInternal internal2 = null;
                bool flag = false;
                lock (this._transactedCxns)
                {
                    flag = this._transactedCxns.TryGetValue(transaction, out list);
                }
                if (flag)
                {
                    lock (list)
                    {
                        int index = list.Count - 1;
                        if (0 <= index)
                        {
                            internal2 = list[index];
                            list.RemoveAt(index);
                        }
                    }
                }
                if (internal2 != null)
                {
                    Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.GetTransactedObject|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Popped.\n", this.ObjectID, transaction.GetHashCode(), internal2.ObjectID);
                }
                return internal2;
            }

            internal void PutTransactedObject(Transaction transaction, System.Data.ProviderBase.DbConnectionInternal transactedObject)
            {
                System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList list2;
                bool flag = false;
                lock (this._transactedCxns)
                {
                    if (flag = this._transactedCxns.TryGetValue(transaction, out list2))
                    {
                        lock (list2)
                        {
                            Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.PutTransactedObject|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Pushing.\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
                            list2.Add(transactedObject);
                        }
                    }
                }
                if (flag)
                {
                    goto Label_016A;
                }
                Transaction tx = null;
                System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList list = null;
                try
                {
                    tx = transaction.Clone();
                    list = new System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList(2, tx);
                    lock (this._transactedCxns)
                    {
                        if (flag = this._transactedCxns.TryGetValue(transaction, out list2))
                        {
                            lock (list2)
                            {
                                Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.PutTransactedObject|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Pushing.\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
                                list2.Add(transactedObject);
                                goto Label_014E;
                            }
                        }
                        Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.PutTransactedObject|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Adding List to transacted pool.\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
                        list.Add(transactedObject);
                        this._transactedCxns.Add(tx, list);
                        tx = null;
                    }
                }
                finally
                {
                    if (null != tx)
                    {
                        if (list != null)
                        {
                            list.Dispose();
                        }
                        else
                        {
                            tx.Dispose();
                        }
                    }
                }
            Label_014E:
                Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.PutTransactedObject|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Added.\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
            Label_016A:
                this.Pool.PerformanceCounters.NumberOfFreeConnections.Increment();
            }

            internal void TransactionEnded(Transaction transaction, System.Data.ProviderBase.DbConnectionInternal transactedObject)
            {
                Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.TransactionEnded|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Transaction Completed\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
                int index = -1;
                lock (this._transactedCxns)
                {
                    System.Data.ProviderBase.DbConnectionPool.TransactedConnectionList list;
                    if (this._transactedCxns.TryGetValue(transaction, out list))
                    {
                        index = list.IndexOf(transactedObject);
                        if (index >= 0)
                        {
                            list.RemoveAt(index);
                        }
                        if (0 >= list.Count)
                        {
                            Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.TransactionEnded|RES|CPOOL> %d#, Transaction %d#, Removing List from transacted pool.\n", this.ObjectID, transaction.GetHashCode());
                            this._transactedCxns.Remove(transaction);
                            list.Dispose();
                        }
                    }
                    else
                    {
                        Bid.PoolerTrace("<prov.DbConnectionPool.TransactedConnectionPool.TransactionEnded|RES|CPOOL> %d#, Transaction %d#, Connection %d#, Transacted pool not yet created prior to transaction completing. Connection may be leaked.\n", this.ObjectID, transaction.GetHashCode(), transactedObject.ObjectID);
                    }
                }
                if (0 <= index)
                {
                    this.Pool.PerformanceCounters.NumberOfFreeConnections.Decrement();
                    this.Pool.PutObjectFromTransactedPool(transactedObject);
                }
            }

            internal int ObjectID
            {
                get
                {
                    return this._objectID;
                }
            }

            internal System.Data.ProviderBase.DbConnectionPool Pool
            {
                get
                {
                    return this._pool;
                }
            }
        }
    }
}

