namespace System.Transactions
{
    using System;
    using System.Threading;

    internal abstract class DurableEnlistmentState : EnlistmentState
    {
        private static DurableEnlistmentAborting _durableEnlistmentAborting;
        private static DurableEnlistmentActive _durableEnlistmentActive;
        private static DurableEnlistmentCommitting _durableEnlistmentCommitting;
        private static DurableEnlistmentDelegated _durableEnlistmentDelegated;
        private static DurableEnlistmentEnded _durableEnlistmentEnded;
        private static object classSyncObject;

        protected DurableEnlistmentState()
        {
        }

        protected static DurableEnlistmentAborting _DurableEnlistmentAborting
        {
            get
            {
                if (_durableEnlistmentAborting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_durableEnlistmentAborting == null)
                        {
                            DurableEnlistmentAborting aborting = new DurableEnlistmentAborting();
                            Thread.MemoryBarrier();
                            _durableEnlistmentAborting = aborting;
                        }
                    }
                }
                return _durableEnlistmentAborting;
            }
        }

        internal static DurableEnlistmentActive _DurableEnlistmentActive
        {
            get
            {
                if (_durableEnlistmentActive == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_durableEnlistmentActive == null)
                        {
                            DurableEnlistmentActive active = new DurableEnlistmentActive();
                            Thread.MemoryBarrier();
                            _durableEnlistmentActive = active;
                        }
                    }
                }
                return _durableEnlistmentActive;
            }
        }

        protected static DurableEnlistmentCommitting _DurableEnlistmentCommitting
        {
            get
            {
                if (_durableEnlistmentCommitting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_durableEnlistmentCommitting == null)
                        {
                            DurableEnlistmentCommitting committing = new DurableEnlistmentCommitting();
                            Thread.MemoryBarrier();
                            _durableEnlistmentCommitting = committing;
                        }
                    }
                }
                return _durableEnlistmentCommitting;
            }
        }

        protected static DurableEnlistmentDelegated _DurableEnlistmentDelegated
        {
            get
            {
                if (_durableEnlistmentDelegated == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_durableEnlistmentDelegated == null)
                        {
                            DurableEnlistmentDelegated delegated = new DurableEnlistmentDelegated();
                            Thread.MemoryBarrier();
                            _durableEnlistmentDelegated = delegated;
                        }
                    }
                }
                return _durableEnlistmentDelegated;
            }
        }

        protected static DurableEnlistmentEnded _DurableEnlistmentEnded
        {
            get
            {
                if (_durableEnlistmentEnded == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_durableEnlistmentEnded == null)
                        {
                            DurableEnlistmentEnded ended = new DurableEnlistmentEnded();
                            Thread.MemoryBarrier();
                            _durableEnlistmentEnded = ended;
                        }
                    }
                }
                return _durableEnlistmentEnded;
            }
        }

        private static object ClassSyncObject
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
    }
}

