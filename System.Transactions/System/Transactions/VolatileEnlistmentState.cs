namespace System.Transactions
{
    using System;
    using System.Threading;

    internal abstract class VolatileEnlistmentState : EnlistmentState
    {
        private static VolatileEnlistmentAborting _volatileEnlistmentAborting;
        private static VolatileEnlistmentActive _volatileEnlistmentActive;
        private static VolatileEnlistmentCommitting _volatileEnlistmentCommitting;
        private static VolatileEnlistmentDone _volatileEnlistmentDone;
        private static VolatileEnlistmentEnded _volatileEnlistmentEnded;
        private static VolatileEnlistmentInDoubt _volatileEnlistmentInDoubt;
        private static VolatileEnlistmentPrepared _volatileEnlistmentPrepared;
        private static VolatileEnlistmentPreparing _volatileEnlistmentPreparing;
        private static VolatileEnlistmentPreparingAborting _volatileEnlistmentPreparingAborting;
        private static VolatileEnlistmentSPC _volatileEnlistmentSPC;
        private static object classSyncObject;

        protected VolatileEnlistmentState()
        {
        }

        internal override byte[] RecoveryInformation(InternalEnlistment enlistment)
        {
            throw TransactionException.CreateInvalidOperationException(System.Transactions.SR.GetString("TraceSourceLtm"), System.Transactions.SR.GetString("VolEnlistNoRecoveryInfo"), null);
        }

        protected static VolatileEnlistmentAborting _VolatileEnlistmentAborting
        {
            get
            {
                if (_volatileEnlistmentAborting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentAborting == null)
                        {
                            VolatileEnlistmentAborting aborting = new VolatileEnlistmentAborting();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentAborting = aborting;
                        }
                    }
                }
                return _volatileEnlistmentAborting;
            }
        }

        internal static VolatileEnlistmentActive _VolatileEnlistmentActive
        {
            get
            {
                if (_volatileEnlistmentActive == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentActive == null)
                        {
                            VolatileEnlistmentActive active = new VolatileEnlistmentActive();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentActive = active;
                        }
                    }
                }
                return _volatileEnlistmentActive;
            }
        }

        protected static VolatileEnlistmentCommitting _VolatileEnlistmentCommitting
        {
            get
            {
                if (_volatileEnlistmentCommitting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentCommitting == null)
                        {
                            VolatileEnlistmentCommitting committing = new VolatileEnlistmentCommitting();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentCommitting = committing;
                        }
                    }
                }
                return _volatileEnlistmentCommitting;
            }
        }

        protected static VolatileEnlistmentDone _VolatileEnlistmentDone
        {
            get
            {
                if (_volatileEnlistmentDone == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentDone == null)
                        {
                            VolatileEnlistmentDone done = new VolatileEnlistmentDone();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentDone = done;
                        }
                    }
                }
                return _volatileEnlistmentDone;
            }
        }

        protected static VolatileEnlistmentEnded _VolatileEnlistmentEnded
        {
            get
            {
                if (_volatileEnlistmentEnded == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentEnded == null)
                        {
                            VolatileEnlistmentEnded ended = new VolatileEnlistmentEnded();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentEnded = ended;
                        }
                    }
                }
                return _volatileEnlistmentEnded;
            }
        }

        protected static VolatileEnlistmentInDoubt _VolatileEnlistmentInDoubt
        {
            get
            {
                if (_volatileEnlistmentInDoubt == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentInDoubt == null)
                        {
                            VolatileEnlistmentInDoubt doubt = new VolatileEnlistmentInDoubt();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentInDoubt = doubt;
                        }
                    }
                }
                return _volatileEnlistmentInDoubt;
            }
        }

        protected static VolatileEnlistmentPrepared _VolatileEnlistmentPrepared
        {
            get
            {
                if (_volatileEnlistmentPrepared == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentPrepared == null)
                        {
                            VolatileEnlistmentPrepared prepared = new VolatileEnlistmentPrepared();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentPrepared = prepared;
                        }
                    }
                }
                return _volatileEnlistmentPrepared;
            }
        }

        protected static VolatileEnlistmentPreparing _VolatileEnlistmentPreparing
        {
            get
            {
                if (_volatileEnlistmentPreparing == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentPreparing == null)
                        {
                            VolatileEnlistmentPreparing preparing = new VolatileEnlistmentPreparing();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentPreparing = preparing;
                        }
                    }
                }
                return _volatileEnlistmentPreparing;
            }
        }

        protected static VolatileEnlistmentPreparingAborting _VolatileEnlistmentPreparingAborting
        {
            get
            {
                if (_volatileEnlistmentPreparingAborting == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentPreparingAborting == null)
                        {
                            VolatileEnlistmentPreparingAborting aborting = new VolatileEnlistmentPreparingAborting();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentPreparingAborting = aborting;
                        }
                    }
                }
                return _volatileEnlistmentPreparingAborting;
            }
        }

        protected static VolatileEnlistmentSPC _VolatileEnlistmentSPC
        {
            get
            {
                if (_volatileEnlistmentSPC == null)
                {
                    lock (ClassSyncObject)
                    {
                        if (_volatileEnlistmentSPC == null)
                        {
                            VolatileEnlistmentSPC tspc = new VolatileEnlistmentSPC();
                            Thread.MemoryBarrier();
                            _volatileEnlistmentSPC = tspc;
                        }
                    }
                }
                return _volatileEnlistmentSPC;
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

