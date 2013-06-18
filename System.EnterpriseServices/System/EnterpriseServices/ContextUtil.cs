namespace System.EnterpriseServices
{
    using System;
    using System.EnterpriseServices.Thunk;
    using System.Runtime.InteropServices;
    using System.Transactions;

    public sealed class ContextUtil
    {
        internal static readonly Guid GUID_JitActivationPolicy = new Guid("ecabaeb2-7f19-11d2-978e-0000f8757e2a");
        internal static readonly Guid GUID_TransactionProperty = new Guid("ecabaeb1-7f19-11d2-978e-0000f8757e2a");

        private ContextUtil()
        {
        }

        public static void DisableCommit()
        {
            ContextThunk.DisableCommit();
        }

        public static void EnableCommit()
        {
            ContextThunk.EnableCommit();
        }

        public static object GetNamedProperty(string name)
        {
            return ((IGetContextProperties) ObjectContext).GetProperty(name);
        }

        public static bool IsCallerInRole(string role)
        {
            return ((System.EnterpriseServices.IObjectContext) ObjectContext).IsCallerInRole(role);
        }

        public static bool IsDefaultContext()
        {
            return ContextThunk.IsDefaultContext();
        }

        public static void SetAbort()
        {
            ContextThunk.SetAbort();
        }

        public static void SetComplete()
        {
            ContextThunk.SetComplete();
        }

        public static void SetNamedProperty(string name, object value)
        {
            ((IContextProperties) ObjectContext).SetProperty(name, value);
        }

        public static Guid ActivityId
        {
            get
            {
                return ((System.EnterpriseServices.IObjectContextInfo) ObjectContext).GetActivityId();
            }
        }

        public static Guid ApplicationId
        {
            get
            {
                return ((IObjectContextInfo2) ObjectContext).GetApplicationId();
            }
        }

        public static Guid ApplicationInstanceId
        {
            get
            {
                return ((IObjectContextInfo2) ObjectContext).GetApplicationInstanceId();
            }
        }

        public static Guid ContextId
        {
            get
            {
                return ((System.EnterpriseServices.IObjectContextInfo) ObjectContext).GetContextId();
            }
        }

        public static bool DeactivateOnReturn
        {
            get
            {
                return ContextThunk.GetDeactivateOnReturn();
            }
            set
            {
                ContextThunk.SetDeactivateOnReturn(value);
            }
        }

        public static bool IsInTransaction
        {
            get
            {
                return ContextThunk.IsInTransaction();
            }
        }

        public static bool IsSecurityEnabled
        {
            get
            {
                try
                {
                    return ((System.EnterpriseServices.IObjectContext) ObjectContext).IsSecurityEnabled();
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    return false;
                }
            }
        }

        public static TransactionVote MyTransactionVote
        {
            get
            {
                return (TransactionVote) ContextThunk.GetMyTransactionVote();
            }
            set
            {
                ContextThunk.SetMyTransactionVote((int) value);
            }
        }

        internal static object ObjectContext
        {
            get
            {
                System.EnterpriseServices.IObjectContext pCtx = null;
                int objectContext = Util.GetObjectContext(out pCtx);
                switch (objectContext)
                {
                    case 0:
                        return pCtx;

                    case -2147467262:
                    case -2147164156:
                        throw new COMException(Resource.FormatString("Err_NoContext"), -2147164156);
                }
                Marshal.ThrowExceptionForHR(objectContext);
                return null;
            }
        }

        public static Guid PartitionId
        {
            get
            {
                return ((IObjectContextInfo2) ObjectContext).GetPartitionId();
            }
        }

        public static System.Transactions.Transaction SystemTransaction
        {
            get
            {
                object ppTx = null;
                TxInfo pTxInfo = new TxInfo();
                if (!ContextThunk.GetTransactionProxyOrTransaction(ref ppTx, pTxInfo))
                {
                    return null;
                }
                if (pTxInfo.isDtcTransaction)
                {
                    return TransactionInterop.GetTransactionFromDtcTransaction((IDtcTransaction) ppTx);
                }
                if (ppTx == null)
                {
                    TransactionProxy pTransactionProxy = new TransactionProxy((DtcIsolationLevel) pTxInfo.IsolationLevel, pTxInfo.timeout);
                    Guid guid = ContextThunk.RegisterTransactionProxy(pTransactionProxy);
                    pTransactionProxy.SetOwnerGuid(guid);
                    return pTransactionProxy.SystemTransaction;
                }
                TransactionProxy proxy2 = ppTx as TransactionProxy;
                if (proxy2 != null)
                {
                    return proxy2.SystemTransaction;
                }
                IDtcTransaction transaction = ContextThunk.GetTransaction() as IDtcTransaction;
                System.Transactions.Transaction transactionFromDtcTransaction = TransactionInterop.GetTransactionFromDtcTransaction(transaction);
                Marshal.ReleaseComObject(ppTx);
                return transactionFromDtcTransaction;
            }
        }

        public static object Transaction
        {
            get
            {
                return ContextThunk.GetTransaction();
            }
        }

        public static Guid TransactionId
        {
            get
            {
                return ContextThunk.GetTransactionId();
            }
        }
    }
}

