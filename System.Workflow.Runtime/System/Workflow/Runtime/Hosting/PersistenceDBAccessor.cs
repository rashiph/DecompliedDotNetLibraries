namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.Runtime;

    internal sealed class PersistenceDBAccessor : IDisposable
    {
        private DbConnection connection;
        private DbResourceAllocator dbResourceAllocator;
        private DbRetry dbRetry;
        private DbTransaction localTransaction;
        private bool needToCloseConnection;

        internal PersistenceDBAccessor(DbResourceAllocator dbResourceAllocator, bool enableRetries)
        {
            this.dbResourceAllocator = dbResourceAllocator;
            this.dbRetry = new DbRetry(enableRetries);
            DbConnection connection = null;
            short retryCount = 0;
        Label_001D:
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService OpenConnection start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                connection = this.dbResourceAllocator.OpenNewConnection();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService. OpenConnection end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                if ((connection == null) || (ConnectionState.Open != connection.State))
                {
                    throw new InvalidOperationException(ExecutionStringManager.InvalidConnection);
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService caught exception from OpenConnection: " + exception.ToString());
                if (!this.dbRetry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService retrying.");
                goto Label_001D;
            }
            this.connection = connection;
            this.needToCloseConnection = true;
        }

        internal PersistenceDBAccessor(DbResourceAllocator dbResourceAllocator, Transaction transaction, WorkflowCommitWorkBatchService transactionService)
        {
            this.dbResourceAllocator = dbResourceAllocator;
            this.localTransaction = DbResourceAllocator.GetLocalTransaction(transactionService, transaction);
            this.connection = this.dbResourceAllocator.GetEnlistedConnection(transactionService, transaction, out this.needToCloseConnection);
            this.dbRetry = new DbRetry(false);
        }

        public void ActivationComplete(Guid instanceId, Guid ownerId)
        {
            DbCommand command = this.NewStoredProcCommand("UnlockInstanceState");
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@uidInstanceID", instanceId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", this.DbOwnerId(ownerId)));
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): unlocking instance: {1}, database: {2}", new object[] { ownerId.ToString(), instanceId.ToString(), this.connection.Database });
            command.ExecuteNonQuery();
        }

        private static void CheckOwnershipResult(DbCommand command)
        {
            DbParameter parameter = command.Parameters["@result"];
            if (((parameter != null) && (parameter.Value != null)) && (((int) parameter.Value) == -2))
            {
                if (command.Parameters.Contains("@currentOwnerID"))
                {
                    Guid empty = Guid.Empty;
                    if (command.Parameters["@currentOwnerID"].Value is Guid)
                    {
                        empty = (Guid) command.Parameters["@currentOwnerID"].Value;
                    }
                    Guid guid2 = (Guid) command.Parameters["@ownerID"].Value;
                    Guid guid3 = (Guid) command.Parameters["@uidInstanceID"].Value;
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): owership violation with {1} on instance {2}", new object[] { guid2.ToString(), empty.ToString(), guid3 });
                }
                DbParameter parameter2 = command.Parameters["@uidInstanceID"];
                throw new WorkflowOwnershipException((Guid) parameter2.Value);
            }
        }

        private object DbOwnerId(Guid ownerId)
        {
            if (ownerId == Guid.Empty)
            {
                return null;
            }
            return ownerId;
        }

        public void Dispose()
        {
            if (this.needToCloseConnection)
            {
                this.connection.Dispose();
            }
        }

        public void InsertCompletedScope(Guid instanceId, Guid scopeId, byte[] state)
        {
            DbCommand command = this.NewStoredProcCommand("InsertCompletedScope");
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("instanceID", instanceId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("completedScopeID", scopeId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("state", state));
            command.ExecuteNonQuery();
        }

        public void InsertInstanceState(PendingWorkItem item, Guid ownerId, DateTime ownedUntil)
        {
            DbCommand command = this.NewStoredProcCommand("InsertInstanceState");
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@uidInstanceID", item.InstanceId));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@state", item.SerializedActivity));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@status", item.Status));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@unlocked", item.Unlocked));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@blocked", item.Blocked));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@info", item.Info));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", (ownedUntil == DateTime.MaxValue) ? SqlDateTime.MaxValue : ownedUntil));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", this.DbOwnerId(ownerId)));
            command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@nextTimer", item.NextTimer));
            DbParameter parameter = this.dbResourceAllocator.NewDbParameter();
            parameter.ParameterName = "@result";
            parameter.DbType = DbType.Int32;
            parameter.Value = 0;
            parameter.Direction = ParameterDirection.InputOutput;
            command.Parameters.Add(parameter);
            DbParameter parameter2 = this.dbResourceAllocator.NewDbParameter();
            parameter2.ParameterName = "@currentOwnerID";
            parameter2.DbType = DbType.Guid;
            parameter2.Value = Guid.Empty;
            parameter2.Direction = ParameterDirection.InputOutput;
            command.Parameters.Add(parameter2);
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): inserting instance: {1}, unlocking: {2} database: {3}", new object[] { ownerId.ToString(), item.InstanceId.ToString(), item.Unlocked.ToString(), this.connection.Database });
            command.ExecuteNonQuery();
            CheckOwnershipResult(command);
        }

        private DbCommand NewStoredProcCommand(string commandText)
        {
            DbCommand command = DbResourceAllocator.NewCommand(commandText, this.connection, this.localTransaction);
            command.CommandType = CommandType.StoredProcedure;
            return command;
        }

        private DbConnection ResetConnection()
        {
            if (this.localTransaction != null)
            {
                throw new InvalidOperationException(ExecutionStringManager.InvalidOpConnectionReset);
            }
            if (!this.needToCloseConnection)
            {
                throw new InvalidOperationException(ExecutionStringManager.InvalidOpConnectionNotLocal);
            }
            if ((this.connection != null) && (this.connection.State != ConnectionState.Closed))
            {
                this.connection.Close();
            }
            this.connection.Dispose();
            this.connection = this.dbResourceAllocator.OpenNewConnection();
            return this.connection;
        }

        internal IEnumerable<SqlPersistenceWorkflowInstanceDescription> RetrieveAllInstanceDescriptions()
        {
            List<SqlPersistenceWorkflowInstanceDescription> list = new List<SqlPersistenceWorkflowInstanceDescription>();
            DbDataReader reader = null;
            try
            {
                reader = this.NewStoredProcCommand("RetrieveAllInstanceDescriptions").ExecuteReader(CommandBehavior.CloseConnection);
                while (reader.Read())
                {
                    list.Add(new SqlPersistenceWorkflowInstanceDescription(reader.GetGuid(0), (WorkflowStatus) reader.GetInt32(1), reader.GetInt32(2) == 1, reader.GetString(3), reader.GetDateTime(4)));
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return list;
        }

        public byte[] RetrieveCompletedScope(Guid scopeId)
        {
            short retryCount = 0;
            byte[] buffer = null;
        Label_0004:
            try
            {
                if ((this.connection == null) || (ConnectionState.Open != this.connection.State))
                {
                    this.ResetConnection();
                }
                DbCommand command = this.NewStoredProcCommand("RetrieveCompletedScope");
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@completedScopeID", scopeId));
                DbParameter parameter = this.dbResourceAllocator.NewDbParameter();
                parameter.ParameterName = "@result";
                parameter.DbType = DbType.Int32;
                parameter.Value = 0;
                parameter.Direction = ParameterDirection.InputOutput;
                command.Parameters.Add(parameter);
                buffer = RetrieveStateFromDB(command, false, WorkflowEnvironment.WorkflowInstanceId);
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveCompletedScope caught exception: " + exception.ToString());
                if (this.dbRetry.TryDoRetry(ref retryCount))
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveCompletedScope retrying.");
                    goto Label_0004;
                }
                if (!(exception is RetryReadException))
                {
                    throw;
                }
                retryCount = (short) (retryCount + 1);
                if (retryCount < 10)
                {
                    goto Label_0004;
                }
            }
            if ((buffer == null) || (buffer.Length == 0))
            {
                throw new InvalidOperationException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.CompletedScopeNotFound, new object[] { scopeId }));
            }
            return buffer;
        }

        public IList<Guid> RetrieveExpiredTimerIds(Guid ownerId, DateTime ownedUntil)
        {
            List<Guid> list = null;
            DbDataReader reader = null;
            short retryCount = 0;
        Label_0006:;
            try
            {
                if ((this.connection == null) || (ConnectionState.Open != this.connection.State))
                {
                    this.ResetConnection();
                }
                DbCommand command = this.NewStoredProcCommand("RetrieveExpiredTimerIds");
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", (ownedUntil == DateTime.MaxValue) ? SqlDateTime.MaxValue : ownedUntil));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", this.DbOwnerId(ownerId)));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@now", DateTime.UtcNow));
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds ExecuteReader start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds ExecuteReader end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                list = new List<Guid>();
                while (reader.Read())
                {
                    list.Add(reader.GetGuid(0));
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds caught exception from ExecuteReader: " + exception.ToString());
                if (!this.dbRetry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveExpiredTimerIds retrying.");
                goto Label_0006;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return list;
        }

        public byte[] RetrieveInstanceState(Guid instanceStateId, Guid ownerId, DateTime timeout)
        {
            short retryCount = 0;
            byte[] buffer = null;
        Label_0004:
            try
            {
                if ((this.connection == null) || (ConnectionState.Open != this.connection.State))
                {
                    this.ResetConnection();
                }
                DbCommand command = this.NewStoredProcCommand("RetrieveInstanceState");
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@uidInstanceID", instanceStateId));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", this.DbOwnerId(ownerId)));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", (timeout == DateTime.MaxValue) ? SqlDateTime.MaxValue : timeout));
                DbParameter parameter = this.dbResourceAllocator.NewDbParameter();
                parameter.ParameterName = "@result";
                parameter.DbType = DbType.Int32;
                parameter.Value = 0;
                parameter.Direction = ParameterDirection.InputOutput;
                command.Parameters.Add(parameter);
                DbParameter parameter2 = this.dbResourceAllocator.NewDbParameter();
                parameter2.ParameterName = "@currentOwnerID";
                parameter2.DbType = DbType.Guid;
                parameter2.Value = Guid.Empty;
                parameter2.Direction = ParameterDirection.InputOutput;
                command.Parameters.Add(parameter2);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService({0}): retreiving instance: {1}, database: {2}", new object[] { ownerId.ToString(), instanceStateId.ToString(), this.connection.Database });
                buffer = RetrieveStateFromDB(command, true, instanceStateId);
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveInstanceState caught exception: " + exception.ToString());
                if (this.dbRetry.TryDoRetry(ref retryCount))
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveInstanceState retrying.");
                    goto Label_0004;
                }
                if (!(exception is RetryReadException))
                {
                    throw;
                }
                retryCount = (short) (retryCount + 1);
                if (retryCount < 10)
                {
                    goto Label_0004;
                }
            }
            if ((buffer != null) && (buffer.Length != 0))
            {
                return buffer;
            }
            Exception exception2 = new InvalidOperationException(string.Format(Thread.CurrentThread.CurrentCulture, ExecutionStringManager.InstanceNotFound, new object[] { instanceStateId }));
            exception2.Data["WorkflowNotFound"] = true;
            throw exception2;
        }

        public IList<Guid> RetrieveNonblockingInstanceStateIds(Guid ownerId, DateTime ownedUntil)
        {
            List<Guid> list = null;
            DbDataReader reader = null;
            short retryCount = 0;
        Label_0006:;
            try
            {
                if ((this.connection == null) || (ConnectionState.Open != this.connection.State))
                {
                    this.ResetConnection();
                }
                DbCommand command = this.NewStoredProcCommand("RetrieveNonblockingInstanceStateIds");
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", (ownedUntil == DateTime.MaxValue) ? SqlDateTime.MaxValue : ownedUntil));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", this.DbOwnerId(ownerId)));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@now", DateTime.UtcNow));
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds ExecuteReader start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds ExecuteReader end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                list = new List<Guid>();
                while (reader.Read())
                {
                    list.Add(reader.GetGuid(0));
                }
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds caught exception from ExecuteReader: " + exception.ToString());
                if (!this.dbRetry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveNonblockingInstanceStateIds retrying.");
                goto Label_0006;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return list;
        }

        private static byte[] RetrieveStateFromDB(DbCommand command, bool checkOwnership, Guid instanceId)
        {
            DbDataReader reader = null;
            byte[] buffer = null;
            try
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB {0} ExecuteReader start: {1}", new object[] { instanceId, DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture) });
                reader = command.ExecuteReader();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB {0} ExecuteReader end: {1}", new object[] { instanceId, DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture) });
                if (reader.Read())
                {
                    buffer = (byte[]) reader.GetValue(0);
                }
                else
                {
                    DbParameter parameter = command.Parameters["@result"];
                    if ((parameter == null) || (parameter.Value == null))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB Failed to read results {0}", new object[] { instanceId });
                    }
                    else if (((int) parameter.Value) > 0)
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.RetrieveStateFromDB Failed to read results {1}, @result == {0}", new object[] { (int) parameter.Value, instanceId });
                        throw new RetryReadException();
                    }
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            if (checkOwnership)
            {
                CheckOwnershipResult(command);
            }
            return buffer;
        }

        public bool TryRetrieveANonblockingInstanceStateId(Guid ownerId, DateTime ownedUntil, out Guid instanceId)
        {
            bool flag;
            short retryCount = 0;
        Label_0002:
            try
            {
                if ((this.connection == null) || (ConnectionState.Open != this.connection.State))
                {
                    this.ResetConnection();
                }
                DbCommand command = this.NewStoredProcCommand("RetrieveANonblockingInstanceStateId");
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownedUntil", (ownedUntil == DateTime.MaxValue) ? SqlDateTime.MaxValue : ownedUntil));
                command.Parameters.Add(this.dbResourceAllocator.NewDbParameter("@ownerID", this.DbOwnerId(ownerId)));
                DbParameter parameter = this.dbResourceAllocator.NewDbParameter();
                parameter.ParameterName = "@uidInstanceID";
                parameter.DbType = DbType.Guid;
                parameter.Value = null;
                parameter.Direction = ParameterDirection.InputOutput;
                command.Parameters.Add(parameter);
                DbParameter parameter2 = this.dbResourceAllocator.NewDbParameter();
                parameter2.ParameterName = "@found";
                parameter2.DbType = DbType.Boolean;
                parameter2.Value = null;
                parameter2.Direction = ParameterDirection.Output;
                command.Parameters.Add(parameter2);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId ExecuteNonQuery start: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                command.ExecuteNonQuery();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId ExecuteNonQuery end: " + DateTime.UtcNow.ToString("G", CultureInfo.InvariantCulture));
                if ((parameter2.Value != null) && ((bool) parameter2.Value))
                {
                    instanceId = (Guid) parameter.Value;
                    return true;
                }
                instanceId = Guid.Empty;
                flag = false;
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId caught exception from ExecuteNonQuery: " + exception.ToString());
                if (!this.dbRetry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlWorkflowPersistenceService.TryRetrieveANonblockingInstanceStateId retrying.");
                goto Label_0002;
            }
            return flag;
        }

        private class RetryReadException : Exception
        {
        }
    }
}

