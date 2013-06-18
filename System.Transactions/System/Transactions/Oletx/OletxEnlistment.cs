namespace System.Transactions.Oletx
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;
    using System.Transactions.Diagnostics;

    internal class OletxEnlistment : OletxBaseEnlistment, IPromotedEnlistment
    {
        private bool aborting;
        private bool canDoSinglePhase;
        private IEnlistmentShim enlistmentShim;
        private bool fabricateRollback;
        private IEnlistmentNotificationInternal iEnlistmentNotification;
        private bool isSinglePhase;
        private IPhase0EnlistmentShim phase0Shim;
        internal IntPtr phase1Handle;
        private byte[] prepareInfoByteArray;
        private byte[] proxyPrepareInfoByteArray;
        private OletxEnlistmentState state;
        private bool tmWentDown;
        private Guid transactionGuid;

        internal OletxEnlistment(IEnlistmentNotificationInternal enlistmentNotification, OletxTransactionStatus xactStatus, byte[] prepareInfoByteArray, OletxResourceManager oletxResourceManager) : base(oletxResourceManager, null)
        {
            this.transactionGuid = Guid.Empty;
            this.phase1Handle = IntPtr.Zero;
            this.enlistmentShim = null;
            this.phase0Shim = null;
            this.canDoSinglePhase = false;
            this.iEnlistmentNotification = enlistmentNotification;
            this.state = OletxEnlistmentState.Active;
            int length = prepareInfoByteArray.Length;
            this.proxyPrepareInfoByteArray = new byte[length];
            Array.Copy(prepareInfoByteArray, this.proxyPrepareInfoByteArray, length);
            byte[] destinationArray = new byte[0x10];
            Array.Copy(this.proxyPrepareInfoByteArray, destinationArray, 0x10);
            this.transactionGuid = new Guid(destinationArray);
            base.transactionGuidString = this.transactionGuid.ToString();
            switch (xactStatus)
            {
                case OletxTransactionStatus.OLETX_TRANSACTION_STATUS_PREPARED:
                    this.state = OletxEnlistmentState.Prepared;
                    lock (oletxResourceManager.reenlistList)
                    {
                        oletxResourceManager.reenlistList.Add(this);
                        oletxResourceManager.StartReenlistThread();
                        goto Label_01CD;
                    }
                    break;

                case OletxTransactionStatus.OLETX_TRANSACTION_STATUS_ABORTED:
                    this.state = OletxEnlistmentState.Aborting;
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Rollback);
                    }
                    this.iEnlistmentNotification.Rollback(this);
                    goto Label_01CD;

                case OletxTransactionStatus.OLETX_TRANSACTION_STATUS_COMMITTED:
                    this.state = OletxEnlistmentState.Committing;
                    lock (oletxResourceManager.reenlistList)
                    {
                        oletxResourceManager.reenlistPendingList.Add(this);
                    }
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Commit);
                    }
                    this.iEnlistmentNotification.Commit(this);
                    goto Label_01CD;
            }
            if (DiagnosticTrace.Critical)
            {
                InternalErrorTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("OletxEnlistmentUnexpectedTransactionStatus"));
            }
            throw TransactionException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), System.Transactions.SR.GetString("OletxEnlistmentUnexpectedTransactionStatus"), null);
        Label_01CD:
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentType.Durable, EnlistmentOptions.None);
            }
            base.AddToEnlistmentTable();
        }

        internal OletxEnlistment(bool canDoSinglePhase, IEnlistmentNotificationInternal enlistmentNotification, Guid transactionGuid, EnlistmentOptions enlistmentOptions, OletxResourceManager oletxResourceManager, OletxTransaction oletxTransaction) : base(oletxResourceManager, oletxTransaction)
        {
            this.transactionGuid = Guid.Empty;
            this.phase1Handle = IntPtr.Zero;
            this.enlistmentShim = null;
            this.phase0Shim = null;
            this.canDoSinglePhase = canDoSinglePhase;
            this.iEnlistmentNotification = enlistmentNotification;
            this.state = OletxEnlistmentState.Active;
            this.transactionGuid = transactionGuid;
            this.proxyPrepareInfoByteArray = null;
            if (DiagnosticTrace.Information)
            {
                EnlistmentTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentType.Durable, enlistmentOptions);
            }
            base.AddToEnlistmentTable();
        }

        public void Aborted()
        {
            this.Aborted(null);
        }

        public void Aborted(Exception e)
        {
            IEnlistmentShim enlistmentShim = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxSinglePhaseEnlistment.Aborted");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.Aborted);
            }
            lock (this)
            {
                if (!this.isSinglePhase || (OletxEnlistmentState.SinglePhaseCommitting != this.state))
                {
                    throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                this.state = OletxEnlistmentState.Aborted;
                enlistmentShim = this.EnlistmentShim;
            }
            Interlocked.CompareExchange<Exception>(ref base.oletxTransaction.realOletxTransaction.innerException, e, null);
            try
            {
                if (enlistmentShim != null)
                {
                    enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.Failed);
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                {
                    throw;
                }
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            finally
            {
                this.FinishEnlistment();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxSinglePhaseEnlistment.Aborted");
            }
        }

        public void AbortRequest()
        {
            IEnlistmentShim enlistmentShim = null;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            bool flag = false;
            lock (this)
            {
                if ((this.state == OletxEnlistmentState.Active) || (OletxEnlistmentState.Prepared == this.state))
                {
                    this.state = OletxEnlistmentState.Aborting;
                    iEnlistmentNotification = this.iEnlistmentNotification;
                }
                else
                {
                    if (OletxEnlistmentState.Phase0Preparing == this.state)
                    {
                        this.fabricateRollback = true;
                    }
                    else
                    {
                        flag = true;
                    }
                    enlistmentShim = this.EnlistmentShim;
                }
            }
            if (iEnlistmentNotification != null)
            {
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Rollback);
                }
                iEnlistmentNotification.Rollback(this);
            }
            else if (enlistmentShim != null)
            {
                try
                {
                    enlistmentShim.AbortRequestDone();
                }
                catch (COMException exception)
                {
                    if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                    {
                        throw;
                    }
                    flag = true;
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.FinishEnlistment();
                    }
                }
            }
        }

        public void CommitRequest()
        {
            IEnlistmentShim enlistmentShim = null;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            bool flag = false;
            lock (this)
            {
                if (OletxEnlistmentState.Prepared == this.state)
                {
                    this.state = OletxEnlistmentState.Committing;
                    iEnlistmentNotification = this.iEnlistmentNotification;
                }
                else
                {
                    enlistmentShim = this.EnlistmentShim;
                    flag = true;
                }
            }
            if (iEnlistmentNotification != null)
            {
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Commit);
                }
                iEnlistmentNotification.Commit(this);
            }
            else if (enlistmentShim != null)
            {
                try
                {
                    enlistmentShim.CommitRequestDone();
                }
                catch (COMException exception)
                {
                    if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                    {
                        throw;
                    }
                    flag = true;
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.FinishEnlistment();
                    }
                }
            }
        }

        public void Committed()
        {
            IEnlistmentShim enlistmentShim = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxSinglePhaseEnlistment.Committed");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.Committed);
            }
            lock (this)
            {
                if (!this.isSinglePhase || (OletxEnlistmentState.SinglePhaseCommitting != this.state))
                {
                    throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                this.state = OletxEnlistmentState.Committed;
                enlistmentShim = this.EnlistmentShim;
            }
            try
            {
                if (enlistmentShim != null)
                {
                    enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.SinglePhase);
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                {
                    throw;
                }
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            finally
            {
                this.FinishEnlistment();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxSinglePhaseEnlistment.Committed");
            }
        }

        public void EnlistmentDone()
        {
            bool flag;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.EnlistmentDone");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.Done);
            }
            IEnlistmentShim enlistmentShim = null;
            IPhase0EnlistmentShim shim2 = null;
            OletxEnlistmentState active = OletxEnlistmentState.Active;
            bool fabricateRollback = false;
            lock (this)
            {
                active = this.state;
                if (this.state == OletxEnlistmentState.Active)
                {
                    shim2 = this.Phase0EnlistmentShim;
                    if (shim2 != null)
                    {
                        base.oletxTransaction.realOletxTransaction.DecrementUndecidedEnlistments();
                    }
                    flag = false;
                }
                else if (OletxEnlistmentState.Preparing == this.state)
                {
                    enlistmentShim = this.EnlistmentShim;
                    flag = true;
                }
                else if (OletxEnlistmentState.Phase0Preparing == this.state)
                {
                    shim2 = this.Phase0EnlistmentShim;
                    base.oletxTransaction.realOletxTransaction.DecrementUndecidedEnlistments();
                    if (this.fabricateRollback)
                    {
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                else
                {
                    if (((OletxEnlistmentState.Committing != this.state) && (OletxEnlistmentState.Aborting != this.state)) && (OletxEnlistmentState.SinglePhaseCommitting != this.state))
                    {
                        throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                    }
                    enlistmentShim = this.EnlistmentShim;
                    flag = true;
                }
                fabricateRollback = this.fabricateRollback;
                this.state = OletxEnlistmentState.Done;
            }
            try
            {
                if (enlistmentShim != null)
                {
                    if (OletxEnlistmentState.Preparing == active)
                    {
                        try
                        {
                            enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.ReadOnly);
                            goto Label_01C1;
                        }
                        finally
                        {
                            HandleTable.FreeHandle(this.phase1Handle);
                        }
                    }
                    if (OletxEnlistmentState.Committing != active)
                    {
                        if (OletxEnlistmentState.Aborting != active)
                        {
                            if (OletxEnlistmentState.SinglePhaseCommitting != active)
                            {
                                throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                            }
                            enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.SinglePhase);
                        }
                        else if (!fabricateRollback)
                        {
                            enlistmentShim.AbortRequestDone();
                        }
                    }
                    else
                    {
                        enlistmentShim.CommitRequestDone();
                    }
                }
                else if (shim2 != null)
                {
                    if (active != OletxEnlistmentState.Active)
                    {
                        if (OletxEnlistmentState.Phase0Preparing != active)
                        {
                            throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                        }
                        shim2.Phase0Done(true);
                    }
                    else
                    {
                        shim2.Unenlist();
                    }
                }
            }
            catch (COMException exception)
            {
                flag = true;
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            finally
            {
                if (flag)
                {
                    this.FinishEnlistment();
                }
            }
        Label_01C1:
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.EnlistmentDone");
            }
        }

        internal void FinishEnlistment()
        {
            lock (this)
            {
                if (this.enlistmentShim == null)
                {
                    base.oletxResourceManager.RemoveFromReenlistPending(this);
                }
                this.iEnlistmentNotification = null;
                base.RemoveFromEnlistmentTable();
            }
        }

        public void ForceRollback()
        {
            this.ForceRollback(null);
        }

        public void ForceRollback(Exception e)
        {
            IPhase0EnlistmentShim shim = null;
            IEnlistmentShim enlistmentShim = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.ForceRollback");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.ForceRollback);
            }
            lock (this)
            {
                if (OletxEnlistmentState.Preparing == this.state)
                {
                    enlistmentShim = this.EnlistmentShim;
                }
                else
                {
                    if (OletxEnlistmentState.Phase0Preparing != this.state)
                    {
                        throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                    }
                    shim = this.Phase0EnlistmentShim;
                    if (shim != null)
                    {
                        base.oletxTransaction.realOletxTransaction.DecrementUndecidedEnlistments();
                    }
                }
                this.state = OletxEnlistmentState.Aborted;
            }
            Interlocked.CompareExchange<Exception>(ref base.oletxTransaction.realOletxTransaction.innerException, e, null);
            try
            {
                if (enlistmentShim != null)
                {
                    try
                    {
                        enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.Failed);
                    }
                    finally
                    {
                        HandleTable.FreeHandle(this.phase1Handle);
                    }
                }
                if (shim != null)
                {
                    shim.Phase0Done(false);
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                {
                    throw;
                }
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            finally
            {
                this.FinishEnlistment();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.ForceRollback");
            }
        }

        public byte[] GetRecoveryInformation()
        {
            if (this.prepareInfoByteArray == null)
            {
                throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
            }
            return this.prepareInfoByteArray;
        }

        public void InDoubt()
        {
            this.InDoubt(null);
        }

        public void InDoubt(Exception e)
        {
            IEnlistmentShim enlistmentShim = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxSinglePhaseEnlistment.InDoubt");
            }
            if (DiagnosticTrace.Warning)
            {
                EnlistmentCallbackNegativeTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.InDoubt);
            }
            lock (this)
            {
                if (!this.isSinglePhase || (OletxEnlistmentState.SinglePhaseCommitting != this.state))
                {
                    throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                }
                this.state = OletxEnlistmentState.InDoubt;
                enlistmentShim = this.EnlistmentShim;
            }
            lock (base.oletxTransaction.realOletxTransaction)
            {
                if (base.oletxTransaction.realOletxTransaction.innerException == null)
                {
                    base.oletxTransaction.realOletxTransaction.innerException = e;
                }
            }
            try
            {
                if (enlistmentShim != null)
                {
                    enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.InDoubt);
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN != exception.ErrorCode) && (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE != exception.ErrorCode))
                {
                    throw;
                }
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            finally
            {
                this.FinishEnlistment();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxSinglePhaseEnlistment.InDoubt");
            }
        }

        public void Phase0Request(bool abortingHint)
        {
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            OletxEnlistmentState active = OletxEnlistmentState.Active;
            OletxCommittableTransaction committableTransaction = null;
            bool flag = false;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.Phase0Request");
            }
            committableTransaction = base.oletxTransaction.realOletxTransaction.committableTransaction;
            if ((committableTransaction != null) && !committableTransaction.CommitCalled)
            {
                flag = true;
            }
            lock (this)
            {
                this.aborting = abortingHint;
                if (this.state == OletxEnlistmentState.Active)
                {
                    if ((this.aborting || flag) || this.tmWentDown)
                    {
                        if (this.phase0Shim != null)
                        {
                            try
                            {
                                this.phase0Shim.Phase0Done(false);
                            }
                            catch (COMException exception)
                            {
                                if (DiagnosticTrace.Verbose)
                                {
                                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                                }
                            }
                        }
                    }
                    else
                    {
                        active = this.state = OletxEnlistmentState.Phase0Preparing;
                        iEnlistmentNotification = this.iEnlistmentNotification;
                    }
                }
            }
            if (iEnlistmentNotification != null)
            {
                if (OletxEnlistmentState.Phase0Preparing != active)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.Phase0Request");
                    }
                    return;
                }
                byte[] buffer = this.transactionGuid.ToByteArray();
                byte[] buffer2 = base.oletxResourceManager.resourceManagerIdentifier.ToByteArray();
                byte[] buffer4 = new byte[buffer.Length + buffer2.Length];
                Thread.MemoryBarrier();
                this.proxyPrepareInfoByteArray = buffer4;
                int index = 0;
                for (index = 0; index < buffer.Length; index++)
                {
                    this.proxyPrepareInfoByteArray[index] = buffer[index];
                }
                for (index = 0; index < buffer2.Length; index++)
                {
                    this.proxyPrepareInfoByteArray[buffer.Length + index] = buffer2[index];
                }
                OletxRecoveryInformation thingToConvert = new OletxRecoveryInformation(this.proxyPrepareInfoByteArray);
                byte[] resourceManagerRecoveryInformation = TransactionManager.ConvertToByteArray(thingToConvert);
                this.prepareInfoByteArray = TransactionManager.GetRecoveryInformation(base.oletxResourceManager.oletxTransactionManager.CreationNodeName, resourceManagerRecoveryInformation);
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Prepare);
                }
                iEnlistmentNotification.Prepare(this);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.Phase0Request");
            }
        }

        public void Prepared()
        {
            int num1 = System.Transactions.Oletx.NativeMethods.S_OK;
            IEnlistmentShim enlistmentShim = null;
            IPhase0EnlistmentShim shim = null;
            bool fabricateRollback = false;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.Prepared");
                EnlistmentCallbackPositiveTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, EnlistmentCallback.Prepared);
            }
            lock (this)
            {
                if (OletxEnlistmentState.Preparing == this.state)
                {
                    enlistmentShim = this.EnlistmentShim;
                }
                else
                {
                    if (OletxEnlistmentState.Phase0Preparing != this.state)
                    {
                        throw TransactionException.CreateEnlistmentStateException(System.Transactions.SR.GetString("TraceSourceOletx"), null);
                    }
                    shim = this.Phase0EnlistmentShim;
                    if (base.oletxTransaction.realOletxTransaction.Doomed || this.fabricateRollback)
                    {
                        this.fabricateRollback = true;
                        fabricateRollback = this.fabricateRollback;
                    }
                }
                this.state = OletxEnlistmentState.Prepared;
            }
            try
            {
                if (enlistmentShim != null)
                {
                    enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.Prepared);
                }
                else if (shim != null)
                {
                    base.oletxTransaction.realOletxTransaction.DecrementUndecidedEnlistments();
                    shim.Phase0Done(!fabricateRollback);
                }
                else
                {
                    fabricateRollback = true;
                }
                if (fabricateRollback)
                {
                    this.AbortRequest();
                }
            }
            catch (COMException exception)
            {
                if ((System.Transactions.Oletx.NativeMethods.XACT_E_CONNECTION_DOWN == exception.ErrorCode) || (System.Transactions.Oletx.NativeMethods.XACT_E_TMNOTAVAILABLE == exception.ErrorCode))
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
                else
                {
                    if (System.Transactions.Oletx.NativeMethods.XACT_E_PROTOCOL != exception.ErrorCode)
                    {
                        throw;
                    }
                    this.Phase0EnlistmentShim = null;
                    if (DiagnosticTrace.Verbose)
                    {
                        ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                    }
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxPreparingEnlistment.Prepared");
            }
        }

        public bool PrepareRequest(bool singlePhase, byte[] prepareInfo)
        {
            IEnlistmentShim enlistmentShim = null;
            OletxEnlistmentState active = OletxEnlistmentState.Active;
            IEnlistmentNotificationInternal iEnlistmentNotification = null;
            OletxRecoveryInformation thingToConvert = null;
            lock (this)
            {
                if (this.state == OletxEnlistmentState.Active)
                {
                    active = this.state = OletxEnlistmentState.Preparing;
                }
                else
                {
                    active = this.state;
                }
                iEnlistmentNotification = this.iEnlistmentNotification;
                enlistmentShim = this.EnlistmentShim;
                base.oletxTransaction.realOletxTransaction.TooLateForEnlistments = true;
            }
            if (OletxEnlistmentState.Preparing == active)
            {
                thingToConvert = new OletxRecoveryInformation(prepareInfo);
                this.isSinglePhase = singlePhase;
                long length = prepareInfo.Length;
                this.proxyPrepareInfoByteArray = new byte[length];
                Array.Copy(prepareInfo, this.proxyPrepareInfoByteArray, length);
                if (this.isSinglePhase && this.canDoSinglePhase)
                {
                    ISinglePhaseNotificationInternal internal3 = (ISinglePhaseNotificationInternal) iEnlistmentNotification;
                    this.state = OletxEnlistmentState.SinglePhaseCommitting;
                    if (DiagnosticTrace.Verbose)
                    {
                        EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.SinglePhaseCommit);
                    }
                    internal3.SinglePhaseCommit(this);
                    return true;
                }
                byte[] resourceManagerRecoveryInformation = TransactionManager.ConvertToByteArray(thingToConvert);
                this.state = OletxEnlistmentState.Preparing;
                this.prepareInfoByteArray = TransactionManager.GetRecoveryInformation(base.oletxResourceManager.oletxTransactionManager.CreationNodeName, resourceManagerRecoveryInformation);
                if (DiagnosticTrace.Verbose)
                {
                    EnlistmentNotificationCallTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), base.InternalTraceIdentifier, NotificationCall.Prepare);
                }
                iEnlistmentNotification.Prepare(this);
                return false;
            }
            if (OletxEnlistmentState.Prepared == active)
            {
                try
                {
                    enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.Prepared);
                    return false;
                }
                catch (COMException exception3)
                {
                    OletxTransactionManager.ProxyException(exception3);
                    throw;
                }
            }
            if (OletxEnlistmentState.Done == active)
            {
                try
                {
                    bool flag;
                    try
                    {
                        enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.ReadOnly);
                        flag = true;
                    }
                    finally
                    {
                        this.FinishEnlistment();
                    }
                    return flag;
                }
                catch (COMException exception2)
                {
                    OletxTransactionManager.ProxyException(exception2);
                    throw;
                }
            }
            try
            {
                enlistmentShim.PrepareRequestDone(OletxPrepareVoteType.Failed);
            }
            catch (COMException exception)
            {
                if (DiagnosticTrace.Verbose)
                {
                    ExceptionConsumedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
                }
            }
            return true;
        }

        public void TMDown()
        {
            lock (base.oletxResourceManager.reenlistList)
            {
                lock (this)
                {
                    this.tmWentDown = true;
                    if ((OletxEnlistmentState.Prepared == this.state) || (OletxEnlistmentState.Committing == this.state))
                    {
                        base.oletxResourceManager.reenlistList.Add(this);
                    }
                }
            }
        }

        internal void TMDownFromInternalRM(OletxTransactionManager oletxTm)
        {
            lock (this)
            {
                if ((base.oletxTransaction == null) || (oletxTm == base.oletxTransaction.realOletxTransaction.OletxTransactionManagerInstance))
                {
                    this.tmWentDown = true;
                }
            }
        }

        internal IEnlistmentNotificationInternal EnlistmentNotification
        {
            get
            {
                return this.iEnlistmentNotification;
            }
        }

        internal IEnlistmentShim EnlistmentShim
        {
            get
            {
                return this.enlistmentShim;
            }
            set
            {
                this.enlistmentShim = value;
            }
        }

        public EnlistmentTraceIdentifier EnlistmentTraceId
        {
            get
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.get_TraceIdentifier");
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "OletxEnlistment.get_TraceIdentifier");
                }
                return base.InternalTraceIdentifier;
            }
        }

        public System.Transactions.InternalEnlistment InternalEnlistment
        {
            get
            {
                return base.internalEnlistment;
            }
            set
            {
                base.internalEnlistment = value;
            }
        }

        internal IPhase0EnlistmentShim Phase0EnlistmentShim
        {
            get
            {
                return this.phase0Shim;
            }
            set
            {
                lock (this)
                {
                    if ((value != null) && (this.aborting || this.tmWentDown))
                    {
                        value.Phase0Done(false);
                    }
                    this.phase0Shim = value;
                }
            }
        }

        internal byte[] ProxyPrepareInfoByteArray
        {
            get
            {
                return this.proxyPrepareInfoByteArray;
            }
        }

        internal OletxEnlistmentState State
        {
            get
            {
                return this.state;
            }
            set
            {
                this.state = value;
            }
        }

        internal Guid TransactionIdentifier
        {
            get
            {
                return this.transactionGuid;
            }
        }

        internal enum OletxEnlistmentState
        {
            Active,
            Phase0Preparing,
            Preparing,
            SinglePhaseCommitting,
            Prepared,
            Committing,
            Committed,
            Aborting,
            Aborted,
            InDoubt,
            Done
        }
    }
}

