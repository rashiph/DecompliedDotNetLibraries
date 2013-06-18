namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    [Serializable]
    internal class OletxTransaction : ISerializable, IObjectReference
    {
        protected int disposed;
        private byte[] propagationTokenForDeserialize;
        protected const string propagationTokenString = "OletxTransactionPropagationToken";
        internal RealOletxTransaction realOletxTransaction;
        internal Transaction savedLtmPromotedTransaction;
        private TransactionTraceIdentifier traceIdentifier;

        internal OletxTransaction(RealOletxTransaction realOletxTransaction)
        {
            this.traceIdentifier = TransactionTraceIdentifier.Empty;
            this.realOletxTransaction = realOletxTransaction;
            this.realOletxTransaction.OletxTransactionCreated();
        }

        protected OletxTransaction(SerializationInfo serializationInfo, StreamingContext context)
        {
            this.traceIdentifier = TransactionTraceIdentifier.Empty;
            if (serializationInfo == null)
            {
                throw new ArgumentNullException("serializationInfo");
            }
            this.propagationTokenForDeserialize = (byte[]) serializationInfo.GetValue("OletxTransactionPropagationToken", typeof(byte[]));
            if (this.propagationTokenForDeserialize.Length < 0x18)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "serializationInfo");
            }
        }

        internal OletxDependentTransaction DependentClone(bool delayCommit)
        {
            OletxDependentTransaction transaction = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.DependentClone");
            }
            if (TransactionStatus.Aborted == this.Status)
            {
                throw TransactionAbortedException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), this.realOletxTransaction.innerException);
            }
            if (TransactionStatus.InDoubt == this.Status)
            {
                throw TransactionInDoubtException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), this.realOletxTransaction.innerException);
            }
            if (this.Status != TransactionStatus.Active)
            {
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TransactionAlreadyOver"), null);
            }
            transaction = new OletxDependentTransaction(this.realOletxTransaction, delayCommit);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.DependentClone");
            }
            return transaction;
        }

        internal void Dispose()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "IDisposable.Dispose");
            }
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
            {
                this.realOletxTransaction.OletxTransactionDisposed();
            }
            GC.SuppressFinalize(this);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "IDisposable.Dispose");
            }
        }

        internal IPromotedEnlistment EnlistDurable(Guid resourceManagerIdentifier, ISinglePhaseNotificationInternal singlePhaseNotification, bool canDoSinglePhase, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.EnlistDurable( ISinglePhaseNotificationInternal )");
            }
            if ((this.realOletxTransaction == null) || this.realOletxTransaction.TooLateForEnlistments)
            {
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TooLate"), null);
            }
            OletxEnlistment enlistment = this.realOletxTransaction.OletxTransactionManagerInstance.FindOrRegisterResourceManager(resourceManagerIdentifier).EnlistDurable(this, canDoSinglePhase, singlePhaseNotification, enlistmentOptions);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.EnlistDurable( ISinglePhaseNotificationInternal )");
            }
            return enlistment;
        }

        internal IPromotedEnlistment EnlistVolatile(IEnlistmentNotificationInternal enlistmentNotification, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.EnlistVolatile( IEnlistmentNotificationInternal )");
            }
            if ((this.realOletxTransaction == null) || this.realOletxTransaction.TooLateForEnlistments)
            {
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TooLate"), null);
            }
            IPromotedEnlistment enlistment = this.realOletxTransaction.EnlistVolatile(enlistmentNotification, enlistmentOptions, this);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.EnlistVolatile( IEnlistmentNotificationInternal )");
            }
            return enlistment;
        }

        internal IPromotedEnlistment EnlistVolatile(ISinglePhaseNotificationInternal singlePhaseNotification, EnlistmentOptions enlistmentOptions)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.EnlistVolatile( ISinglePhaseNotificationInternal )");
            }
            if ((this.realOletxTransaction == null) || this.realOletxTransaction.TooLateForEnlistments)
            {
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("TooLate"), null);
            }
            IPromotedEnlistment enlistment = this.realOletxTransaction.EnlistVolatile(singlePhaseNotification, enlistmentOptions, this);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.EnlistVolatile( ISinglePhaseNotificationInternal )");
            }
            return enlistment;
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo serializationInfo, StreamingContext context)
        {
            if (serializationInfo == null)
            {
                throw new ArgumentNullException("serializationInfo");
            }
            byte[] transmitterPropagationToken = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.GetObjectData");
            }
            transmitterPropagationToken = TransactionInterop.GetTransmitterPropagationToken(this);
            serializationInfo.SetType(typeof(OletxTransaction));
            serializationInfo.AddValue("OletxTransactionPropagationToken", transmitterPropagationToken);
            if (DiagnosticTrace.Information)
            {
                TransactionSerializedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), this.TransactionTraceId);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.GetObjectData");
            }
        }

        public object GetRealObject(StreamingContext context)
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "IObjectReference.GetRealObject");
            }
            if (this.propagationTokenForDeserialize == null)
            {
                if (DiagnosticTrace.Critical)
                {
                    InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("UnableToDeserializeTransaction"));
                }
                throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("UnableToDeserializeTransactionInternalError"), null);
            }
            if (null != this.savedLtmPromotedTransaction)
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "IObjectReference.GetRealObject");
                }
                return this.savedLtmPromotedTransaction;
            }
            Transaction transactionFromTransmitterPropagationToken = TransactionInterop.GetTransactionFromTransmitterPropagationToken(this.propagationTokenForDeserialize);
            this.savedLtmPromotedTransaction = transactionFromTransmitterPropagationToken;
            if (DiagnosticTrace.Verbose)
            {
                TransactionDeserializedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), transactionFromTransmitterPropagationToken.internalTransaction.PromotedTransaction.TransactionTraceId);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "IObjectReference.GetRealObject");
            }
            return transactionFromTransmitterPropagationToken;
        }

        internal void Rollback()
        {
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.Rollback");
            }
            if (DiagnosticTrace.Warning)
            {
                TransactionRollbackCalledTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), this.TransactionTraceId);
            }
            this.realOletxTransaction.Rollback();
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.Rollback");
            }
        }

        internal Guid Identifier
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.get_Identifier");
                }
                Guid identifier = this.realOletxTransaction.Identifier;
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.get_Identifier");
                }
                return identifier;
            }
        }

        internal Exception InnerException
        {
            get
            {
                return this.realOletxTransaction.innerException;
            }
        }

        public virtual System.Transactions.IsolationLevel IsolationLevel
        {
            get
            {
                return this.realOletxTransaction.TransactionIsolationLevel;
            }
        }

        internal RealOletxTransaction RealTransaction
        {
            get
            {
                return this.realOletxTransaction;
            }
        }

        internal TransactionStatus Status
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.get_Status");
                }
                TransactionStatus status = this.realOletxTransaction.Status;
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxTransaction.get_Status");
                }
                return status;
            }
        }

        internal TransactionTraceIdentifier TransactionTraceId
        {
            get
            {
                if (TransactionTraceIdentifier.Empty == this.traceIdentifier)
                {
                    lock (this.realOletxTransaction)
                    {
                        if (TransactionTraceIdentifier.Empty == this.traceIdentifier)
                        {
                            try
                            {
                                TransactionTraceIdentifier identifier = new TransactionTraceIdentifier(this.realOletxTransaction.Identifier.ToString(), 0);
                                Thread.MemoryBarrier();
                                this.traceIdentifier = identifier;
                            }
                            catch (TransactionException exception)
                            {
                                if (DiagnosticTrace.Verbose)
                                {
                                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                                }
                            }
                        }
                    }
                }
                return this.traceIdentifier;
            }
        }
    }
}

