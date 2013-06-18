namespace System.Web.Util
{
    using System;
    using System.EnterpriseServices;
    using System.Web;

    public class Transactions
    {
        public static void InvokeTransacted(TransactedCallback callback, TransactionOption mode)
        {
            bool transactionAborted = false;
            InvokeTransacted(callback, mode, ref transactionAborted);
        }

        public static void InvokeTransacted(TransactedCallback callback, TransactionOption mode, ref bool transactionAborted)
        {
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Medium, "Transaction_not_supported_in_low_trust");
            bool flag = false;
            if ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version.Major <= 4))
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("RequiresNT"));
            }
            if (mode == TransactionOption.Disabled)
            {
                flag = true;
            }
            if (flag)
            {
                callback();
                transactionAborted = false;
            }
            else
            {
                int num;
                TransactedInvocation invocation = new TransactedInvocation(callback);
                TransactedExecCallback callback2 = new TransactedExecCallback(invocation.ExecuteTransactedCode);
                PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_PENDING);
                try
                {
                    num = UnsafeNativeMethods.TransactManagedCallback(callback2, (int) mode);
                }
                finally
                {
                    PerfCounters.DecrementCounter(AppPerfCounter.TRANSACTIONS_PENDING);
                }
                if (invocation.Error != null)
                {
                    throw new HttpException(null, invocation.Error);
                }
                PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_TOTAL);
                switch (num)
                {
                    case 1:
                        PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_COMMITTED);
                        transactionAborted = false;
                        return;

                    case 0:
                        PerfCounters.IncrementCounter(AppPerfCounter.TRANSACTIONS_ABORTED);
                        transactionAborted = true;
                        return;
                }
                throw new HttpException(System.Web.SR.GetString("Cannot_execute_transacted_code"));
            }
        }

        internal class TransactedInvocation
        {
            private TransactedCallback _callback;
            private Exception _error;

            internal TransactedInvocation(TransactedCallback callback)
            {
                this._callback = callback;
            }

            internal int ExecuteTransactedCode()
            {
                TransactedExecState commitPending = TransactedExecState.CommitPending;
                try
                {
                    this._callback();
                    if (Transactions.Utils.AbortPending)
                    {
                        commitPending = TransactedExecState.AbortPending;
                    }
                }
                catch (Exception exception)
                {
                    this._error = exception;
                    commitPending = TransactedExecState.Error;
                }
                return (int) commitPending;
            }

            internal Exception Error
            {
                get
                {
                    return this._error;
                }
            }
        }

        internal class Utils
        {
            private Utils()
            {
            }

            internal static bool AbortPending
            {
                get
                {
                    bool flag = false;
                    try
                    {
                        if (ContextUtil.MyTransactionVote == TransactionVote.Abort)
                        {
                            flag = true;
                        }
                    }
                    catch
                    {
                    }
                    return flag;
                }
            }

            internal static bool IsInTransaction
            {
                get
                {
                    bool isInTransaction = false;
                    try
                    {
                        isInTransaction = ContextUtil.IsInTransaction;
                    }
                    catch
                    {
                    }
                    return isInTransaction;
                }
            }
        }
    }
}

