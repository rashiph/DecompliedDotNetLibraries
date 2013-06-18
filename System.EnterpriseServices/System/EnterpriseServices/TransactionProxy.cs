namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;
    using System.Transactions;

    internal sealed class TransactionProxy : MarshalByRefObject, ITransactionProxy
    {
        private CommittableTransaction committableTx;
        private bool owned;
        private Guid ownerGuid;
        private Transaction systemTx;

        public TransactionProxy(Transaction systemTx)
        {
            this.systemTx = systemTx;
            this.owned = false;
        }

        public TransactionProxy(DtcIsolationLevel isoLevel, int timeout)
        {
            TransactionOptions options = new TransactionOptions {
                Timeout = TimeSpan.FromSeconds((double) timeout),
                IsolationLevel = ConvertIsolationLevelFromDtc(isoLevel)
            };
            this.committableTx = new CommittableTransaction(options);
            this.systemTx = this.committableTx.Clone();
            this.owned = false;
        }

        public void Abort()
        {
            try
            {
                this.systemTx.Rollback();
            }
            catch (TransactionException exception)
            {
                this.MapTxExceptionToHR(exception);
            }
            finally
            {
                if (this.committableTx != null)
                {
                    this.committableTx.Dispose();
                    this.committableTx = null;
                    this.systemTx = null;
                }
            }
        }

        public void Commit(Guid guid)
        {
            try
            {
                if (this.committableTx == null)
                {
                    Marshal.ThrowExceptionForHR(-2147418113);
                }
                else if (this.owned)
                {
                    if (guid == this.ownerGuid)
                    {
                        this.committableTx.Commit();
                    }
                    else
                    {
                        Marshal.ThrowExceptionForHR(-2147418113);
                    }
                }
                else
                {
                    this.committableTx.Commit();
                }
            }
            catch (TransactionException exception)
            {
                this.MapTxExceptionToHR(exception, true);
            }
            finally
            {
                this.committableTx.Dispose();
                this.committableTx = null;
                this.systemTx = null;
            }
        }

        internal static IsolationLevel ConvertIsolationLevelFromDtc(DtcIsolationLevel proxyIsolationLevel)
        {
            switch (proxyIsolationLevel)
            {
                case DtcIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED:
                    return IsolationLevel.ReadUncommitted;

                case DtcIsolationLevel.ISOLATIONLEVEL_CURSORSTABILITY:
                    return IsolationLevel.ReadCommitted;

                case DtcIsolationLevel.ISOLATIONLEVEL_REPEATABLEREAD:
                    return IsolationLevel.RepeatableRead;

                case DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE:
                    return IsolationLevel.Serializable;
            }
            return IsolationLevel.Serializable;
        }

        public void CreateVoter(ITransactionVoterNotifyAsync2 voterNotification, out ITransactionVoterBallotAsync2 voterBallot)
        {
            voterBallot = null;
            try
            {
                if (voterNotification == null)
                {
                    throw new ArgumentNullException("voterNotification");
                }
                voterBallot = new VoterBallot(voterNotification, this.systemTx);
            }
            catch (TransactionException exception)
            {
                this.MapTxExceptionToHR(exception);
            }
        }

        public Guid GetIdentifier()
        {
            try
            {
                ITransaction dtcTransaction = (ITransaction) TransactionInterop.GetDtcTransaction(this.systemTx);
                return this.systemTx.TransactionInformation.DistributedIdentifier;
            }
            catch (TransactionException exception)
            {
                this.MapTxExceptionToHR(exception);
            }
            return Guid.Empty;
        }

        public DtcIsolationLevel GetIsolationLevel()
        {
            try
            {
                switch (this.systemTx.IsolationLevel)
                {
                    case IsolationLevel.Serializable:
                        return DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;

                    case IsolationLevel.RepeatableRead:
                        return DtcIsolationLevel.ISOLATIONLEVEL_REPEATABLEREAD;

                    case IsolationLevel.ReadCommitted:
                        return DtcIsolationLevel.ISOLATIONLEVEL_CURSORSTABILITY;

                    case IsolationLevel.ReadUncommitted:
                        return DtcIsolationLevel.ISOLATIONLEVEL_READUNCOMMITTED;
                }
                return DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
            }
            catch (TransactionException exception)
            {
                this.MapTxExceptionToHR(exception);
            }
            return DtcIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
        }

        public bool IsReusable()
        {
            return false;
        }

        private void MapTxExceptionToHR(TransactionException txException)
        {
            this.MapTxExceptionToHR(txException, false);
        }

        private void MapTxExceptionToHR(TransactionException txException, bool isInCommit)
        {
            TransactionAbortedException exception = txException as TransactionAbortedException;
            if (exception != null)
            {
                if (isInCommit)
                {
                    TransactionProxyException.ThrowTransactionProxyException(-2147164158, exception);
                }
                else
                {
                    TransactionProxyException.ThrowTransactionProxyException(-2147164157, exception);
                }
            }
            TransactionManagerCommunicationException exception2 = txException as TransactionManagerCommunicationException;
            if (exception2 != null)
            {
                TransactionProxyException.ThrowTransactionProxyException(-2147164145, exception2);
            }
            COMException baseException = txException.GetBaseException() as COMException;
            if (baseException != null)
            {
                TransactionProxyException.ThrowTransactionProxyException(baseException.ErrorCode, txException);
            }
            else
            {
                TransactionProxyException.ThrowTransactionProxyException(-2147418113, txException);
            }
        }

        public IDtcTransaction Promote()
        {
            try
            {
                return TransactionInterop.GetDtcTransaction(this.systemTx);
            }
            catch (TransactionException exception)
            {
                this.MapTxExceptionToHR(exception);
            }
            return null;
        }

        public void SetOwnerGuid(Guid guid)
        {
            this.ownerGuid = guid;
            this.owned = true;
        }

        internal Transaction SystemTransaction
        {
            get
            {
                return this.systemTx;
            }
        }
    }
}

