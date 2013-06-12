namespace System.Runtime.Remoting.Contexts
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable, SecurityCritical, AttributeUsage(AttributeTargets.Class), ComVisible(true), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    public class SynchronizationAttribute : ContextAttribute, IContributeServerContextSink, IContributeClientContextSink
    {
        [NonSerialized]
        private ArrayList _asyncLcidList;
        [NonSerialized]
        internal AutoResetEvent _asyncWorkEvent;
        internal bool _bReEntrant;
        [NonSerialized]
        private SynchronizationAttribute _cliCtxAttr;
        internal int _flavor;
        [NonSerialized]
        internal bool _locked;
        [NonSerialized]
        private string _syncLcid;
        private static readonly int _timeOut = -1;
        [NonSerialized]
        private RegisteredWaitHandle _waitHandle;
        [NonSerialized]
        internal Queue _workItemQueue;
        public const int NOT_SUPPORTED = 1;
        private const string PROPERTY_NAME = "Synchronization";
        public const int REQUIRED = 4;
        public const int REQUIRES_NEW = 8;
        public const int SUPPORTED = 2;

        public SynchronizationAttribute() : this(4, false)
        {
        }

        public SynchronizationAttribute(bool reEntrant) : this(4, reEntrant)
        {
        }

        public SynchronizationAttribute(int flag) : this(flag, false)
        {
        }

        public SynchronizationAttribute(int flag, bool reEntrant) : base("Synchronization")
        {
            this._bReEntrant = reEntrant;
            switch (flag)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                    this._flavor = flag;
                    return;
            }
            throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "flag");
        }

        private void DispatcherCallBack(object stateIgnored, bool ignored)
        {
            WorkItem item;
            lock (this._workItemQueue)
            {
                item = (WorkItem) this._workItemQueue.Dequeue();
            }
            this.ExecuteWorkItem(item);
            this.HandleWorkCompletion();
        }

        internal void Dispose()
        {
            if (this._waitHandle != null)
            {
                this._waitHandle.Unregister(null);
            }
        }

        internal void ExecuteWorkItem(WorkItem work)
        {
            work.Execute();
        }

        [SecurityCritical]
        public virtual IMessageSink GetClientContextSink(IMessageSink nextSink)
        {
            this.InitIfNecessary();
            return new SynchronizedClientContextSink(this, nextSink);
        }

        [ComVisible(true), SecurityCritical]
        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            if (((this._flavor != 1) && (this._flavor != 2)) && (ctorMsg != null))
            {
                if (this._cliCtxAttr != null)
                {
                    ctorMsg.ContextProperties.Add(this._cliCtxAttr);
                    this._cliCtxAttr = null;
                }
                else
                {
                    ctorMsg.ContextProperties.Add(this);
                }
            }
        }

        [SecurityCritical]
        public virtual IMessageSink GetServerContextSink(IMessageSink nextSink)
        {
            this.InitIfNecessary();
            return new SynchronizedServerContextSink(this, nextSink);
        }

        internal virtual void HandleThreadExit()
        {
            this.HandleWorkCompletion();
        }

        internal virtual void HandleThreadReEntry()
        {
            WorkItem work = new WorkItem(null, null, null);
            work.SetDummy();
            this.HandleWorkRequest(work);
        }

        internal virtual void HandleWorkCompletion()
        {
            WorkItem item = null;
            bool flag = false;
            lock (this._workItemQueue)
            {
                if (this._workItemQueue.Count >= 1)
                {
                    item = (WorkItem) this._workItemQueue.Peek();
                    flag = true;
                    item.SetSignaled();
                }
                else
                {
                    this._locked = false;
                }
            }
            if (flag)
            {
                if (item.IsAsync())
                {
                    this._asyncWorkEvent.Set();
                }
                else
                {
                    lock (item)
                    {
                        Monitor.Pulse(item);
                    }
                }
            }
        }

        internal virtual void HandleWorkRequest(WorkItem work)
        {
            if (!this.IsNestedCall(work._reqMsg))
            {
                bool flag;
                if (work.IsAsync())
                {
                    flag = true;
                    lock (this._workItemQueue)
                    {
                        work.SetWaiting();
                        this._workItemQueue.Enqueue(work);
                        if (!this._locked && (this._workItemQueue.Count == 1))
                        {
                            work.SetSignaled();
                            this._locked = true;
                            this._asyncWorkEvent.Set();
                        }
                        return;
                    }
                }
                lock (work)
                {
                    lock (this._workItemQueue)
                    {
                        if (!this._locked && (this._workItemQueue.Count == 0))
                        {
                            this._locked = true;
                            flag = false;
                        }
                        else
                        {
                            flag = true;
                            work.SetWaiting();
                            this._workItemQueue.Enqueue(work);
                        }
                    }
                    if (flag)
                    {
                        Monitor.Wait(work);
                        if (!work.IsDummy())
                        {
                            this.DispatcherCallBack(null, true);
                            return;
                        }
                        lock (this._workItemQueue)
                        {
                            this._workItemQueue.Dequeue();
                            return;
                        }
                    }
                    if (!work.IsDummy())
                    {
                        work.SetSignaled();
                        this.ExecuteWorkItem(work);
                        this.HandleWorkCompletion();
                    }
                    return;
                }
            }
            work.SetSignaled();
            work.Execute();
        }

        internal virtual void InitIfNecessary()
        {
            lock (this)
            {
                if (this._asyncWorkEvent == null)
                {
                    this._asyncWorkEvent = new AutoResetEvent(false);
                    this._workItemQueue = new Queue();
                    this._asyncLcidList = new ArrayList();
                    WaitOrTimerCallback callBack = new WaitOrTimerCallback(this.DispatcherCallBack);
                    this._waitHandle = ThreadPool.RegisterWaitForSingleObject(this._asyncWorkEvent, callBack, null, _timeOut, false);
                }
            }
        }

        [ComVisible(true), SecurityCritical]
        public override bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            if (ctx == null)
            {
                throw new ArgumentNullException("ctx");
            }
            if (msg == null)
            {
                throw new ArgumentNullException("msg");
            }
            bool flag = true;
            if (this._flavor == 8)
            {
                return false;
            }
            SynchronizationAttribute property = (SynchronizationAttribute) ctx.GetProperty("Synchronization");
            if (((this._flavor == 1) && (property != null)) || ((this._flavor == 4) && (property == null)))
            {
                flag = false;
            }
            if (this._flavor == 4)
            {
                this._cliCtxAttr = property;
            }
            return flag;
        }

        internal bool IsKnownLCID(IMessage reqMsg)
        {
            string logicalCallID = ((LogicalCallContext) reqMsg.Properties[Message.CallContextKey]).RemotingData.LogicalCallID;
            if (!logicalCallID.Equals(this._syncLcid))
            {
                return this._asyncLcidList.Contains(logicalCallID);
            }
            return true;
        }

        internal bool IsNestedCall(IMessage reqMsg)
        {
            bool flag = false;
            if (!this.IsReEntrant)
            {
                string syncCallOutLCID = this.SyncCallOutLCID;
                if (syncCallOutLCID != null)
                {
                    LogicalCallContext context = (LogicalCallContext) reqMsg.Properties[Message.CallContextKey];
                    if ((context != null) && syncCallOutLCID.Equals(context.RemotingData.LogicalCallID))
                    {
                        flag = true;
                    }
                }
                if (!flag && (this.AsyncCallOutLCIDList.Count > 0))
                {
                    LogicalCallContext context2 = (LogicalCallContext) reqMsg.Properties[Message.CallContextKey];
                    if (this.AsyncCallOutLCIDList.Contains(context2.RemotingData.LogicalCallID))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        internal ArrayList AsyncCallOutLCIDList
        {
            get
            {
                return this._asyncLcidList;
            }
        }

        public virtual bool IsReEntrant
        {
            get
            {
                return this._bReEntrant;
            }
        }

        public virtual bool Locked
        {
            get
            {
                return this._locked;
            }
            set
            {
                this._locked = value;
            }
        }

        internal string SyncCallOutLCID
        {
            get
            {
                return this._syncLcid;
            }
            set
            {
                this._syncLcid = value;
            }
        }
    }
}

