namespace System.Transactions
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Transactions.Diagnostics;
    using System.Transactions.Oletx;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public static class TransactionInterop
    {
        internal static OletxTransaction ConvertToOletxTransaction(Transaction transaction)
        {
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if (transaction.Disposed)
            {
                throw new ObjectDisposedException("Transaction");
            }
            if (transaction.complete)
            {
                throw TransactionException.CreateTransactionCompletedException(System.Transactions.SR.GetString("TraceSourceLtm"));
            }
            return transaction.Promote();
        }

        public static IDtcTransaction GetDtcTransaction(Transaction transaction)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetDtcTransaction");
            }
            IDtcTransaction transactionNative = null;
            OletxTransaction transaction2 = ConvertToOletxTransaction(transaction);
            try
            {
                transaction2.realOletxTransaction.TransactionShim.GetITransactionNative(out transactionNative);
            }
            catch (COMException exception)
            {
                OletxTransactionManager.ProxyException(exception);
                throw;
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetDtcTransaction");
            }
            return transactionNative;
        }

        public static byte[] GetExportCookie(Transaction transaction, byte[] whereabouts)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            byte[] destination = null;
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if (whereabouts == null)
            {
                throw new ArgumentNullException("whereabouts");
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetExportCookie");
            }
            byte[] destinationArray = new byte[whereabouts.Length];
            Array.Copy(whereabouts, destinationArray, whereabouts.Length);
            whereabouts = destinationArray;
            int cookieIndex = 0;
            uint cookieSize = 0;
            CoTaskMemHandle cookieBuffer = null;
            OletxTransaction transaction2 = ConvertToOletxTransaction(transaction);
            try
            {
                transaction2.realOletxTransaction.TransactionShim.Export(Convert.ToUInt32(whereabouts.Length), whereabouts, out cookieIndex, out cookieSize, out cookieBuffer);
                destination = new byte[cookieSize];
                Marshal.Copy(cookieBuffer.DangerousGetHandle(), destination, 0, Convert.ToInt32(cookieSize));
            }
            catch (COMException exception)
            {
                OletxTransactionManager.ProxyException(exception);
                throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
            }
            finally
            {
                if (cookieBuffer != null)
                {
                    cookieBuffer.Close();
                }
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetExportCookie");
            }
            return destination;
        }

        internal static OletxTransaction GetOletxTransactionFromTransmitterPropigationToken(byte[] propagationToken)
        {
            OutcomeEnlistment enlistment;
            ITransactionShim transactionShim = null;
            OletxTransactionIsolationLevel level;
            Guid guid;
            if (propagationToken == null)
            {
                throw new ArgumentNullException("propagationToken");
            }
            if (propagationToken.Length < 0x18)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "propagationToken");
            }
            byte[] destinationArray = new byte[propagationToken.Length];
            Array.Copy(propagationToken, destinationArray, propagationToken.Length);
            propagationToken = destinationArray;
            OletxTransactionManager distributedTransactionManager = TransactionManager.DistributedTransactionManager;
            distributedTransactionManager.dtcTransactionManagerLock.AcquireReaderLock(-1);
            try
            {
                enlistment = new OutcomeEnlistment();
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    zero = HandleTable.AllocHandle(enlistment);
                    distributedTransactionManager.DtcTransactionManager.ProxyShimFactory.ReceiveTransaction(Convert.ToUInt32(propagationToken.Length), propagationToken, zero, out guid, out level, out transactionShim);
                }
                finally
                {
                    if ((transactionShim == null) && (zero != IntPtr.Zero))
                    {
                        HandleTable.FreeHandle(zero);
                    }
                }
            }
            catch (COMException exception)
            {
                OletxTransactionManager.ProxyException(exception);
                throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
            }
            finally
            {
                distributedTransactionManager.dtcTransactionManagerLock.ReleaseReaderLock();
            }
            return new OletxTransaction(new RealOletxTransaction(distributedTransactionManager, transactionShim, enlistment, guid, level, false));
        }

        public static Transaction GetTransactionFromDtcTransaction(IDtcTransaction transactionNative)
        {
            OletxXactTransInfo info;
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            bool flag = false;
            ITransactionShim transactionShim = null;
            Guid empty = Guid.Empty;
            OletxTransactionIsolationLevel isolationLevel = OletxTransactionIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
            OutcomeEnlistment target = null;
            RealOletxTransaction realOletxTransaction = null;
            OletxTransaction oletx = null;
            if (transactionNative == null)
            {
                throw new ArgumentNullException("transactionNative");
            }
            Transaction transaction = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromDtc");
            }
            ITransactionNativeInternal internal2 = transactionNative as ITransactionNativeInternal;
            if (internal2 == null)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "transactionNative");
            }
            try
            {
                internal2.GetTransactionInfo(out info);
            }
            catch (COMException exception2)
            {
                if (System.Transactions.Oletx.NativeMethods.XACT_E_NOTRANSACTION != exception2.ErrorCode)
                {
                    throw;
                }
                flag = true;
                info.uow = Guid.Empty;
            }
            OletxTransactionManager distributedTransactionManager = TransactionManager.DistributedTransactionManager;
            if (!flag)
            {
                transaction = TransactionManager.FindPromotedTransaction(info.uow);
                if (null != transaction)
                {
                    if (DiagnosticTrace.Verbose)
                    {
                        MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromDtcTransaction");
                    }
                    return transaction;
                }
                distributedTransactionManager.dtcTransactionManagerLock.AcquireReaderLock(-1);
                try
                {
                    target = new OutcomeEnlistment();
                    IntPtr zero = IntPtr.Zero;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        zero = HandleTable.AllocHandle(target);
                        distributedTransactionManager.DtcTransactionManager.ProxyShimFactory.CreateTransactionShim(transactionNative, zero, out empty, out isolationLevel, out transactionShim);
                    }
                    finally
                    {
                        if ((transactionShim == null) && (zero != IntPtr.Zero))
                        {
                            HandleTable.FreeHandle(zero);
                        }
                    }
                }
                catch (COMException exception)
                {
                    OletxTransactionManager.ProxyException(exception);
                    throw;
                }
                finally
                {
                    distributedTransactionManager.dtcTransactionManagerLock.ReleaseReaderLock();
                }
                realOletxTransaction = new RealOletxTransaction(distributedTransactionManager, transactionShim, target, empty, isolationLevel, false);
                oletx = new OletxTransaction(realOletxTransaction);
                transaction = TransactionManager.FindOrCreatePromotedTransaction(info.uow, oletx);
            }
            else
            {
                realOletxTransaction = new RealOletxTransaction(distributedTransactionManager, null, null, empty, OletxTransactionIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE, false);
                oletx = new OletxTransaction(realOletxTransaction);
                transaction = new Transaction(oletx);
                TransactionManager.FireDistributedTransactionStarted(transaction);
                oletx.savedLtmPromotedTransaction = transaction;
                InternalTransaction.DistributedTransactionOutcome(transaction.internalTransaction, TransactionStatus.InDoubt);
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromDtc");
            }
            return transaction;
        }

        public static Transaction GetTransactionFromExportCookie(byte[] cookie)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (cookie == null)
            {
                throw new ArgumentNullException("cookie");
            }
            if (cookie.Length < 0x20)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "cookie");
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromExportCookie");
            }
            byte[] destinationArray = new byte[cookie.Length];
            Array.Copy(cookie, destinationArray, cookie.Length);
            cookie = destinationArray;
            Transaction transaction = null;
            ITransactionShim transactionShim = null;
            Guid empty = Guid.Empty;
            OletxTransactionIsolationLevel isolationLevel = OletxTransactionIsolationLevel.ISOLATIONLEVEL_SERIALIZABLE;
            OutcomeEnlistment target = null;
            OletxTransaction oletx = null;
            byte[] b = new byte[0x10];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = cookie[i + 0x10];
            }
            Guid transactionIdentifier = new Guid(b);
            transaction = TransactionManager.FindPromotedTransaction(transactionIdentifier);
            if (null != transaction)
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromExportCookie");
                }
                return transaction;
            }
            RealOletxTransaction realOletxTransaction = null;
            OletxTransactionManager distributedTransactionManager = TransactionManager.DistributedTransactionManager;
            distributedTransactionManager.dtcTransactionManagerLock.AcquireReaderLock(-1);
            try
            {
                target = new OutcomeEnlistment();
                IntPtr zero = IntPtr.Zero;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    zero = HandleTable.AllocHandle(target);
                    distributedTransactionManager.DtcTransactionManager.ProxyShimFactory.Import(Convert.ToUInt32(cookie.Length), cookie, zero, out empty, out isolationLevel, out transactionShim);
                }
                finally
                {
                    if ((transactionShim == null) && (zero != IntPtr.Zero))
                    {
                        HandleTable.FreeHandle(zero);
                    }
                }
            }
            catch (COMException exception)
            {
                OletxTransactionManager.ProxyException(exception);
                throw TransactionManagerCommunicationException.Create(System.Transactions.SR.GetString("TraceSourceOletx"), exception);
            }
            finally
            {
                distributedTransactionManager.dtcTransactionManagerLock.ReleaseReaderLock();
            }
            realOletxTransaction = new RealOletxTransaction(distributedTransactionManager, transactionShim, target, empty, isolationLevel, false);
            oletx = new OletxTransaction(realOletxTransaction);
            transaction = TransactionManager.FindOrCreatePromotedTransaction(transactionIdentifier, oletx);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromExportCookie");
            }
            return transaction;
        }

        public static Transaction GetTransactionFromTransmitterPropagationToken(byte[] propagationToken)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            Transaction transaction2 = null;
            if (propagationToken == null)
            {
                throw new ArgumentNullException("propagationToken");
            }
            if (propagationToken.Length < 0x18)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("InvalidArgument"), "propagationToken");
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromTransmitterPropagationToken");
            }
            byte[] b = new byte[0x10];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = propagationToken[i + 8];
            }
            Guid transactionIdentifier = new Guid(b);
            Transaction transaction = TransactionManager.FindPromotedTransaction(transactionIdentifier);
            if (null != transaction)
            {
                if (DiagnosticTrace.Verbose)
                {
                    MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromTransmitterPropagationToken");
                }
                return transaction;
            }
            OletxTransaction oletxTransactionFromTransmitterPropigationToken = GetOletxTransactionFromTransmitterPropigationToken(propagationToken);
            transaction2 = TransactionManager.FindOrCreatePromotedTransaction(transactionIdentifier, oletxTransactionFromTransmitterPropigationToken);
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransactionFromTransmitterPropagationToken");
            }
            return transaction2;
        }

        internal static byte[] GetTransmitterPropagationToken(OletxTransaction oletxTx)
        {
            CoTaskMemHandle propgationToken = null;
            byte[] destination = null;
            uint propagationTokeSize = 0;
            try
            {
                oletxTx.realOletxTransaction.TransactionShim.GetPropagationToken(out propagationTokeSize, out propgationToken);
                destination = new byte[propagationTokeSize];
                Marshal.Copy(propgationToken.DangerousGetHandle(), destination, 0, Convert.ToInt32(propagationTokeSize));
            }
            catch (COMException exception)
            {
                OletxTransactionManager.ProxyException(exception);
                throw;
            }
            finally
            {
                if (propgationToken != null)
                {
                    propgationToken.Close();
                }
            }
            return destination;
        }

        public static byte[] GetTransmitterPropagationToken(Transaction transaction)
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            if (null == transaction)
            {
                throw new ArgumentNullException("transaction");
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransmitterPropagationToken");
            }
            byte[] transmitterPropagationToken = GetTransmitterPropagationToken(ConvertToOletxTransaction(transaction));
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetTransmitterPropagationToken");
            }
            return transmitterPropagationToken;
        }

        public static byte[] GetWhereabouts()
        {
            if (!TransactionManager._platformValidated)
            {
                TransactionManager.ValidatePlatform();
            }
            byte[] whereabouts = null;
            if (DiagnosticTrace.Verbose)
            {
                MethodEnteredTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetWhereabouts");
            }
            OletxTransactionManager distributedTransactionManager = TransactionManager.DistributedTransactionManager;
            if (distributedTransactionManager == null)
            {
                throw new ArgumentException(System.Transactions.SR.GetString("ArgumentWrongType"), "transactionManager");
            }
            distributedTransactionManager.dtcTransactionManagerLock.AcquireReaderLock(-1);
            try
            {
                whereabouts = distributedTransactionManager.DtcTransactionManager.Whereabouts;
            }
            finally
            {
                distributedTransactionManager.dtcTransactionManagerLock.ReleaseReaderLock();
            }
            if (DiagnosticTrace.Verbose)
            {
                MethodExitedTraceRecord.Trace(System.Transactions.SR.GetString("TraceSourceOletx"), "TransactionInterop.GetWhereabouts");
            }
            return whereabouts;
        }
    }
}

