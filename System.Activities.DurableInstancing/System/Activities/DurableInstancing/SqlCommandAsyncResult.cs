namespace System.Activities.DurableInstancing
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Transactions;

    internal sealed class SqlCommandAsyncResult : AsyncResult
    {
        private string connectionString;
        private DependentTransaction dependentTransaction;
        private static readonly TimeSpan MaximumOpenTimeout = TimeSpan.FromMinutes(2.0);
        private int maximumRetries;
        private static AsyncResult.AsyncCompletion onExecuteReaderCallback = new AsyncResult.AsyncCompletion(SqlCommandAsyncResult.OnExecuteReader);
        private static AsyncResult.AsyncCompletion onRetryCommandCallback = new AsyncResult.AsyncCompletion(SqlCommandAsyncResult.OnRetryCommand);
        private int retryCount;
        private static readonly RetryErrorCode[] retryErrorCodes = new RetryErrorCode[] { new RetryErrorCode(-2, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x35, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(20, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x40, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x79, RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x4b5, RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x4c6, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0xf46, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryOnBegin), new RetryErrorCode(0x21c5, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x21c1, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x2745, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x2746, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0xe9, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x9d05, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x9e35, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd), new RetryErrorCode(0x9ea5, RetryErrorOptions.RetryWhenTransaction | RetryErrorOptions.RetryBeginOrEnd) };
        private SqlCommand sqlCommand;
        private SqlDataReader sqlDataReader;
        private TimeoutHelper timeoutHelper;

        public SqlCommandAsyncResult(SqlCommand sqlCommand, string connectionString, DependentTransaction dependentTransaction, TimeSpan timeout, int retryCount, int maximumRetries, AsyncCallback callback, object state) : base(callback, state)
        {
            long num = Math.Min(timeout.Ticks, MaximumOpenTimeout.Ticks);
            this.sqlCommand = sqlCommand;
            this.connectionString = connectionString;
            this.dependentTransaction = dependentTransaction;
            this.timeoutHelper = new TimeoutHelper(TimeSpan.FromTicks(num));
            this.retryCount = retryCount;
            this.maximumRetries = maximumRetries;
        }

        private bool CheckRetryCount()
        {
            return (++this.retryCount < this.maximumRetries);
        }

        private bool CheckRetryCountAndTimer()
        {
            return (this.CheckRetryCount() && !this.HasOperationTimedOut());
        }

        private static SqlCommand CloneSqlCommand(SqlCommand command)
        {
            SqlCommand command2 = new SqlCommand {
                CommandType = command.CommandType,
                CommandText = command.CommandText
            };
            SqlParameter[] values = new SqlParameter[command.Parameters.Count];
            for (int i = 0; i < command.Parameters.Count; i++)
            {
                values[i] = command.Parameters[i];
            }
            command.Parameters.Clear();
            command2.Parameters.AddRange(values);
            return command2;
        }

        private bool CompleteExecuteReader(IAsyncResult result)
        {
            bool flag = true;
            try
            {
                this.sqlDataReader = this.sqlCommand.EndExecuteReader(result);
            }
            catch (SqlException exception)
            {
                if (TD.SqlExceptionCaughtIsEnabled())
                {
                    TD.SqlExceptionCaught(exception.Number.ToString(CultureInfo.InvariantCulture), exception.Message);
                }
                if (this.sqlDataReader != null)
                {
                    this.sqlDataReader.Close();
                }
                if (this.sqlCommand.Connection != null)
                {
                    this.sqlCommand.Connection.Close();
                }
                if ((!result.CompletedSynchronously && this.CheckRetryCountAndTimer()) && (ShouldRetryForSqlError(exception.Number, RetryErrorOptions.RetryOnEnd) && this.EnqueueRetry()))
                {
                    if (TD.RetryingSqlCommandDueToSqlErrorIsEnabled())
                    {
                        TD.RetryingSqlCommandDueToSqlError(exception.Number.ToString(CultureInfo.InvariantCulture));
                    }
                    flag = false;
                }
                if (!flag)
                {
                    return flag;
                }
                if ((this.retryCount == this.maximumRetries) && TD.MaximumRetriesExceededForSqlCommandIsEnabled())
                {
                    TD.MaximumRetriesExceededForSqlCommand();
                }
                throw;
            }
            return flag;
        }

        public static SqlDataReader End(IAsyncResult result)
        {
            return AsyncResult.End<SqlCommandAsyncResult>(result).sqlDataReader;
        }

        private bool EnqueueRetry()
        {
            bool flag = false;
            int retryDelay = this.GetRetryDelay();
            if (this.timeoutHelper.RemainingTime().TotalMilliseconds <= retryDelay)
            {
                return flag;
            }
            this.sqlCommand.Dispose();
            new IOThreadTimer(new Action<object>(SqlCommandAsyncResult.StartCommandCallback), new SqlCommandAsyncResult(CloneSqlCommand(this.sqlCommand), this.connectionString, this.dependentTransaction, this.timeoutHelper.RemainingTime(), this.retryCount, this.maximumRetries, base.PrepareAsyncCompletion(onRetryCommandCallback), this), false).Set(retryDelay);
            if (TD.QueingSqlRetryIsEnabled())
            {
                TD.QueingSqlRetry(retryDelay.ToString(CultureInfo.InvariantCulture));
            }
            return true;
        }

        private int GetRetryDelay()
        {
            return 0x3e8;
        }

        private bool HasOperationTimedOut()
        {
            return (this.timeoutHelper.RemainingTime() <= TimeSpan.Zero);
        }

        private static bool OnExecuteReader(IAsyncResult result)
        {
            SqlCommandAsyncResult asyncState = (SqlCommandAsyncResult) result.AsyncState;
            return asyncState.CompleteExecuteReader(result);
        }

        private static bool OnRetryCommand(IAsyncResult childPtr)
        {
            SqlCommandAsyncResult asyncState = (SqlCommandAsyncResult) childPtr.AsyncState;
            asyncState.sqlDataReader = End(childPtr);
            return true;
        }

        private static bool ShouldRetryForSqlError(int error, RetryErrorOptions retryErrorOptions)
        {
            if (Transaction.Current != null)
            {
                retryErrorOptions |= RetryErrorOptions.RetryWhenTransaction;
            }
            return retryErrorCodes.Any<RetryErrorCode>(x => ((x.ErrorCode == error) && ((x.RetryErrorOptions & retryErrorOptions) == retryErrorOptions)));
        }

        public void StartCommand()
        {
            this.StartCommandInternal(true);
        }

        private static void StartCommandCallback(object state)
        {
            SqlCommandAsyncResult result = (SqlCommandAsyncResult) state;
            try
            {
                result.StartCommandInternal(false);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (result.sqlCommand.Connection != null)
                {
                    result.sqlCommand.Connection.Close();
                }
                result.Complete(false, exception);
            }
        }

        private void StartCommandInternal(bool synchronous)
        {
            if (!this.HasOperationTimedOut())
            {
                try
                {
                    IAsyncResult result;
                    using (base.PrepareTransactionalCall(this.dependentTransaction))
                    {
                        AsyncCallback callback = base.PrepareAsyncCompletion(onExecuteReaderCallback);
                        this.sqlCommand.Connection = StoreUtilities.CreateConnection(this.connectionString);
                        if (!this.HasOperationTimedOut())
                        {
                            result = this.sqlCommand.BeginExecuteReader(callback, this, CommandBehavior.CloseConnection);
                        }
                        else
                        {
                            this.sqlCommand.Connection.Close();
                            base.Complete(synchronous, new TimeoutException(System.Activities.DurableInstancing.SR.TimeoutOnSqlOperation(this.timeoutHelper.OriginalTimeout.ToString())));
                            return;
                        }
                    }
                    if (base.CheckSyncContinue(result) && this.CompleteExecuteReader(result))
                    {
                        base.Complete(synchronous);
                    }
                    return;
                }
                catch (SqlException exception)
                {
                    if (TD.SqlExceptionCaughtIsEnabled())
                    {
                        TD.SqlExceptionCaught(exception.Number.ToString(CultureInfo.InvariantCulture), exception.Message);
                    }
                    if (this.sqlCommand.Connection != null)
                    {
                        this.sqlCommand.Connection.Close();
                    }
                    if (!this.CheckRetryCount() || !ShouldRetryForSqlError(exception.Number, RetryErrorOptions.RetryOnBegin))
                    {
                        throw;
                    }
                    if (TD.RetryingSqlCommandDueToSqlErrorIsEnabled())
                    {
                        TD.RetryingSqlCommandDueToSqlError(exception.Number.ToString(CultureInfo.InvariantCulture));
                    }
                }
                catch (InvalidOperationException)
                {
                    if (!this.CheckRetryCount())
                    {
                        throw;
                    }
                }
                if (this.EnqueueRetry())
                {
                    return;
                }
            }
            if (this.HasOperationTimedOut())
            {
                if (TD.TimeoutOpeningSqlConnectionIsEnabled())
                {
                    TD.TimeoutOpeningSqlConnection(this.timeoutHelper.OriginalTimeout.ToString());
                }
            }
            else if (TD.MaximumRetriesExceededForSqlCommandIsEnabled())
            {
                TD.MaximumRetriesExceededForSqlCommand();
            }
            base.Complete(synchronous, new TimeoutException(System.Activities.DurableInstancing.SR.TimeoutOnSqlOperation(this.timeoutHelper.OriginalTimeout.ToString())));
        }

        private class RetryErrorCode
        {
            public RetryErrorCode(int code, System.Activities.DurableInstancing.SqlCommandAsyncResult.RetryErrorOptions retryErrorOptions)
            {
                this.ErrorCode = code;
                this.RetryErrorOptions = retryErrorOptions;
            }

            public int ErrorCode { get; private set; }

            public System.Activities.DurableInstancing.SqlCommandAsyncResult.RetryErrorOptions RetryErrorOptions { get; private set; }
        }

        [Flags]
        private enum RetryErrorOptions
        {
            RetryBeginOrEnd = 3,
            RetryOnBegin = 1,
            RetryOnEnd = 2,
            RetryWhenTransaction = 4
        }
    }
}

