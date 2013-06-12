namespace System.Data.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.ProviderBase;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Transactions;

    internal sealed class SqlInternalConnectionTds : SqlInternalConnection, IDisposable
    {
        private int _asyncCommandCount;
        private string _currentFailoverPartner;
        private string _currentLanguage;
        private int _currentPacketSize;
        private bool _fConnectionOpen;
        private bool _fResetConnection;
        private DbConnectionPoolIdentity _identity;
        private string _instanceName;
        private SqlLoginAck _loginAck;
        private string _originalDatabase;
        private string _originalLanguage;
        private TdsParser _parser;
        private readonly SqlConnectionPoolGroupProviderInfo _poolGroupProviderInfo;
        private List<WeakReference> _preparedCommands;
        private RoutingInfo _routingInfo;

        internal SqlInternalConnectionTds(DbConnectionPoolIdentity identity, SqlConnectionString connectionOptions, object providerInfo, string newPassword, SqlConnection owningObject, bool redirectedUserInstance) : base(connectionOptions)
        {
            this._instanceName = string.Empty;
            if (connectionOptions.UserInstance && InOutOfProcHelper.InProc)
            {
                throw SQL.UserInstanceNotAvailableInProc();
            }
            this._identity = identity;
            this._poolGroupProviderInfo = (SqlConnectionPoolGroupProviderInfo) providerInfo;
            this._fResetConnection = connectionOptions.ConnectionReset;
            if (this._fResetConnection)
            {
                this._originalDatabase = connectionOptions.InitialCatalog;
                this._originalLanguage = connectionOptions.CurrentLanguage;
            }
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                TimeoutTimer timeout = TimeoutTimer.StartSecondsTimeout(connectionOptions.ConnectTimeout);
                this.OpenLoginEnlist(owningObject, timeout, connectionOptions, newPassword, redirectedUserInstance);
            }
            catch (OutOfMemoryException)
            {
                base.DoomThisConnection();
                throw;
            }
            catch (StackOverflowException)
            {
                base.DoomThisConnection();
                throw;
            }
            catch (ThreadAbortException)
            {
                base.DoomThisConnection();
                throw;
            }
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionTds.ctor|ADV> %d#, constructed new TDS internal connection\n", base.ObjectID);
            }
        }

        protected override void Activate(Transaction transaction)
        {
            this.FailoverPermissionDemand();
            if (null != transaction)
            {
                if (base.ConnectionOptions.Enlist)
                {
                    base.Enlist(transaction);
                }
            }
            else
            {
                base.Enlist(null);
            }
        }

        internal override void AddPreparedCommand(SqlCommand cmd)
        {
            if (this._preparedCommands == null)
            {
                this._preparedCommands = new List<WeakReference>(5);
            }
            for (int i = 0; i < this._preparedCommands.Count; i++)
            {
                if (!this._preparedCommands[i].IsAlive)
                {
                    this._preparedCommands[i].Target = cmd;
                    return;
                }
            }
            this._preparedCommands.Add(new WeakReference(cmd));
        }

        private void AttemptOneLogin(ServerInfo serverInfo, string newPassword, bool ignoreSniOpenTimeout, TimeoutTimer timeout, SqlConnection owningObject)
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionTds.AttemptOneLogin|ADV> %d#, timout=%I64d{msec}, server=", base.ObjectID, timeout.MillisecondsRemaining);
                Bid.PutStr(serverInfo.ExtendedServerName);
                Bid.Trace("\n");
            }
            this._routingInfo = null;
            this._parser._physicalStateObj.SniContext = SniContext.Snix_Connect;
            this._parser.Connect(serverInfo, this, ignoreSniOpenTimeout, timeout.LegacyTimerExpire, base.ConnectionOptions.Encrypt, base.ConnectionOptions.TrustServerCertificate, base.ConnectionOptions.IntegratedSecurity);
            this._parser._physicalStateObj.SniContext = SniContext.Snix_Login;
            this.Login(serverInfo, timeout, newPassword);
            this.CompleteLogin(!base.ConnectionOptions.Pooling);
        }

        internal void BreakConnection()
        {
            Bid.Trace("<sc.SqlInternalConnectionTds.BreakConnection|RES|CPOOL> %d#, Breaking connection.\n", base.ObjectID);
            base.DoomThisConnection();
            if (base.Connection != null)
            {
                base.Connection.Close();
            }
        }

        protected override void ChangeDatabaseInternal(string database)
        {
            database = SqlConnection.FixupDatabaseTransactionName(database);
            this._parser.TdsExecuteSQLBatch("use " + database, base.ConnectionOptions.ConnectTimeout, null, this._parser._physicalStateObj);
            this._parser.Run(RunBehavior.UntilDone, null, null, null, this._parser._physicalStateObj);
        }

        internal void CheckEnlistedTransactionBinding()
        {
            Transaction enlistedTransaction = base.EnlistedTransaction;
            if (enlistedTransaction != null)
            {
                if (base.ConnectionOptions.TransactionBinding == SqlConnectionString.TransactionBindingEnum.ExplicitUnbind)
                {
                    Transaction current = Transaction.Current;
                    if ((enlistedTransaction.TransactionInformation.Status != TransactionStatus.Active) || !enlistedTransaction.Equals(current))
                    {
                        throw ADP.TransactionConnectionMismatch();
                    }
                }
                else if (enlistedTransaction.TransactionInformation.Status != TransactionStatus.Active)
                {
                    if (!base.EnlistedTransactionDisposed)
                    {
                        throw ADP.TransactionCompletedButNotDisposed();
                    }
                    base.DetachTransaction(enlistedTransaction, true);
                }
            }
        }

        internal override void ClearPreparedCommands()
        {
            if (this._preparedCommands != null)
            {
                for (int i = 0; i < this._preparedCommands.Count; i++)
                {
                    SqlCommand target = this._preparedCommands[i].Target as SqlCommand;
                    if (target != null)
                    {
                        target.Unprepare(true);
                        this._preparedCommands[i].Target = null;
                    }
                }
                this._preparedCommands = null;
            }
        }

        private void CompleteLogin(bool enlistOK)
        {
            this._parser.Run(RunBehavior.UntilDone, null, null, null, this._parser._physicalStateObj);
            this._parser._physicalStateObj.SniContext = SniContext.Snix_EnableMars;
            this._parser.EnableMars();
            this._fConnectionOpen = true;
            if (enlistOK && base.ConnectionOptions.Enlist)
            {
                this._parser._physicalStateObj.SniContext = SniContext.Snix_AutoEnlist;
                Transaction currentTransaction = ADP.GetCurrentTransaction();
                base.Enlist(currentTransaction);
            }
            this._parser._physicalStateObj.SniContext = SniContext.Snix_Login;
        }

        internal void DecrementAsyncCount()
        {
            Interlocked.Decrement(ref this._asyncCommandCount);
        }

        internal override void DelegatedTransactionEnded()
        {
            base.DelegatedTransactionEnded();
        }

        internal override void DisconnectTransaction(SqlInternalTransaction internalTransaction)
        {
            TdsParser parser = this.Parser;
            if (parser != null)
            {
                parser.DisconnectTransaction(internalTransaction);
            }
        }

        public override void Dispose()
        {
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionTds.Dispose|ADV> %d# disposing\n", base.ObjectID);
            }
            try
            {
                TdsParser parser = Interlocked.Exchange<TdsParser>(ref this._parser, null);
                if (parser != null)
                {
                    parser.Disconnect();
                }
            }
            finally
            {
                this._loginAck = null;
                this._fConnectionOpen = false;
            }
            base.Dispose();
        }

        internal void ExecuteTransaction(SqlInternalConnection.TransactionRequest transactionRequest, string name, System.Data.IsolationLevel iso)
        {
            this.ExecuteTransaction(transactionRequest, name, iso, null, false);
        }

        internal override void ExecuteTransaction(SqlInternalConnection.TransactionRequest transactionRequest, string name, System.Data.IsolationLevel iso, SqlInternalTransaction internalTransaction, bool isDelegateControlRequest)
        {
            if (base.IsConnectionDoomed)
            {
                if ((transactionRequest != SqlInternalConnection.TransactionRequest.Rollback) && (transactionRequest != SqlInternalConnection.TransactionRequest.IfRollback))
                {
                    throw SQL.ConnectionDoomed();
                }
            }
            else
            {
                if ((((transactionRequest == SqlInternalConnection.TransactionRequest.Commit) || (transactionRequest == SqlInternalConnection.TransactionRequest.Rollback)) || (transactionRequest == SqlInternalConnection.TransactionRequest.IfRollback)) && (!this.Parser.MARSOn && this.Parser._physicalStateObj.BcpLock))
                {
                    throw SQL.ConnectionLockedForBcpEvent();
                }
                string transactionName = (name == null) ? string.Empty : name;
                if (!this._parser.IsYukonOrNewer)
                {
                    this.ExecuteTransactionPreYukon(transactionRequest, transactionName, iso, internalTransaction);
                }
                else
                {
                    this.ExecuteTransactionYukon(transactionRequest, transactionName, iso, internalTransaction, isDelegateControlRequest);
                }
            }
        }

        internal void ExecuteTransactionPreYukon(SqlInternalConnection.TransactionRequest transactionRequest, string transactionName, System.Data.IsolationLevel iso, SqlInternalTransaction internalTransaction)
        {
            StringBuilder builder = new StringBuilder();
            switch (iso)
            {
                case System.Data.IsolationLevel.Unspecified:
                    break;

                case System.Data.IsolationLevel.Chaos:
                    throw SQL.NotSupportedIsolationLevel(iso);

                case System.Data.IsolationLevel.ReadUncommitted:
                    builder.Append("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED");
                    builder.Append(";");
                    break;

                case System.Data.IsolationLevel.Serializable:
                    builder.Append("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE");
                    builder.Append(";");
                    break;

                case System.Data.IsolationLevel.Snapshot:
                    throw SQL.SnapshotNotSupported(System.Data.IsolationLevel.Snapshot);

                case System.Data.IsolationLevel.ReadCommitted:
                    builder.Append("SET TRANSACTION ISOLATION LEVEL READ COMMITTED");
                    builder.Append(";");
                    break;

                case System.Data.IsolationLevel.RepeatableRead:
                    builder.Append("SET TRANSACTION ISOLATION LEVEL REPEATABLE READ");
                    builder.Append(";");
                    break;

                default:
                    throw ADP.InvalidIsolationLevel(iso);
            }
            if (!ADP.IsEmpty(transactionName))
            {
                transactionName = " " + SqlConnection.FixupDatabaseTransactionName(transactionName);
            }
            switch (transactionRequest)
            {
                case SqlInternalConnection.TransactionRequest.Begin:
                    builder.Append("BEGIN TRANSACTION");
                    builder.Append(transactionName);
                    break;

                case SqlInternalConnection.TransactionRequest.Commit:
                    builder.Append("COMMIT TRANSACTION");
                    builder.Append(transactionName);
                    break;

                case SqlInternalConnection.TransactionRequest.Rollback:
                    builder.Append("ROLLBACK TRANSACTION");
                    builder.Append(transactionName);
                    break;

                case SqlInternalConnection.TransactionRequest.IfRollback:
                    builder.Append("IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION");
                    builder.Append(transactionName);
                    break;

                case SqlInternalConnection.TransactionRequest.Save:
                    builder.Append("SAVE TRANSACTION");
                    builder.Append(transactionName);
                    break;
            }
            this._parser.TdsExecuteSQLBatch(builder.ToString(), base.ConnectionOptions.ConnectTimeout, null, this._parser._physicalStateObj);
            this._parser.Run(RunBehavior.UntilDone, null, null, null, this._parser._physicalStateObj);
            if (transactionRequest == SqlInternalConnection.TransactionRequest.Begin)
            {
                this._parser.CurrentTransaction = internalTransaction;
            }
        }

        internal void ExecuteTransactionYukon(SqlInternalConnection.TransactionRequest transactionRequest, string transactionName, System.Data.IsolationLevel iso, SqlInternalTransaction internalTransaction, bool isDelegateControlRequest)
        {
            TdsEnums.TransactionManagerRequestType begin = TdsEnums.TransactionManagerRequestType.Begin;
            TdsEnums.TransactionManagerIsolationLevel readCommitted = TdsEnums.TransactionManagerIsolationLevel.ReadCommitted;
            switch (iso)
            {
                case System.Data.IsolationLevel.Unspecified:
                    readCommitted = TdsEnums.TransactionManagerIsolationLevel.Unspecified;
                    break;

                case System.Data.IsolationLevel.Chaos:
                    throw SQL.NotSupportedIsolationLevel(iso);

                case System.Data.IsolationLevel.ReadUncommitted:
                    readCommitted = TdsEnums.TransactionManagerIsolationLevel.ReadUncommitted;
                    break;

                case System.Data.IsolationLevel.Serializable:
                    readCommitted = TdsEnums.TransactionManagerIsolationLevel.Serializable;
                    break;

                case System.Data.IsolationLevel.Snapshot:
                    readCommitted = TdsEnums.TransactionManagerIsolationLevel.Snapshot;
                    break;

                case System.Data.IsolationLevel.ReadCommitted:
                    readCommitted = TdsEnums.TransactionManagerIsolationLevel.ReadCommitted;
                    break;

                case System.Data.IsolationLevel.RepeatableRead:
                    readCommitted = TdsEnums.TransactionManagerIsolationLevel.RepeatableRead;
                    break;

                default:
                    throw ADP.InvalidIsolationLevel(iso);
            }
            TdsParserStateObject session = this._parser._physicalStateObj;
            TdsParser parser = this._parser;
            bool flag = false;
            bool lockTaken = false;
            try
            {
                switch (transactionRequest)
                {
                    case SqlInternalConnection.TransactionRequest.Begin:
                        begin = TdsEnums.TransactionManagerRequestType.Begin;
                        break;

                    case SqlInternalConnection.TransactionRequest.Promote:
                        begin = TdsEnums.TransactionManagerRequestType.Promote;
                        break;

                    case SqlInternalConnection.TransactionRequest.Commit:
                        begin = TdsEnums.TransactionManagerRequestType.Commit;
                        break;

                    case SqlInternalConnection.TransactionRequest.Rollback:
                    case SqlInternalConnection.TransactionRequest.IfRollback:
                        begin = TdsEnums.TransactionManagerRequestType.Rollback;
                        break;

                    case SqlInternalConnection.TransactionRequest.Save:
                        begin = TdsEnums.TransactionManagerRequestType.Save;
                        break;
                }
                if ((internalTransaction != null) && internalTransaction.IsDelegated)
                {
                    if (!this._parser.MARSOn)
                    {
                        if (internalTransaction.OpenResultsCount != 0)
                        {
                            throw SQL.CannotCompleteDelegatedTransactionWithOpenResults();
                        }
                        Monitor.Enter(session, ref lockTaken);
                        if (internalTransaction.OpenResultsCount != 0)
                        {
                            throw SQL.CannotCompleteDelegatedTransactionWithOpenResults();
                        }
                    }
                    else
                    {
                        session = this._parser.GetSession(this);
                        flag = true;
                    }
                }
                this._parser.TdsExecuteTransactionManagerRequest(null, begin, transactionName, readCommitted, base.ConnectionOptions.ConnectTimeout, internalTransaction, session, isDelegateControlRequest);
            }
            finally
            {
                if (flag)
                {
                    parser.PutSession(session);
                }
                if (lockTaken)
                {
                    Monitor.Exit(session);
                }
            }
        }

        internal void FailoverPermissionDemand()
        {
            if (this.PoolGroupProviderInfo != null)
            {
                this.PoolGroupProviderInfo.FailoverPermissionDemand();
            }
        }

        protected override byte[] GetDTCAddress()
        {
            return this._parser.GetDTCAddress(base.ConnectionOptions.ConnectTimeout, this._parser._physicalStateObj);
        }

        internal void IncrementAsyncCount()
        {
            Interlocked.Increment(ref this._asyncCommandCount);
        }

        protected override void InternalDeactivate()
        {
            if (this._asyncCommandCount != 0)
            {
                base.DoomThisConnection();
            }
            if (!this.IsNonPoolableTransactionRoot)
            {
                this._parser.Deactivate(base.IsConnectionDoomed);
                if (!base.IsConnectionDoomed)
                {
                    this.ResetConnection();
                }
            }
        }

        internal override bool IsConnectionAlive(bool throwOnException)
        {
            return this._parser._physicalStateObj.IsConnectionAlive(throwOnException);
        }

        private bool IsDoNotRetryConnectError(SqlException exc)
        {
            if (((0x4818 != exc.Number) && (0x4838 != exc.Number)) && (0x542 != exc.Number))
            {
                return exc._doNotReconnect;
            }
            return true;
        }

        private void Login(ServerInfo server, TimeoutTimer timeout, string newPassword)
        {
            SqlLogin rec = new SqlLogin();
            base.CurrentDatabase = server.ResolvedDatabaseName;
            this._currentPacketSize = base.ConnectionOptions.PacketSize;
            this._currentLanguage = base.ConnectionOptions.CurrentLanguage;
            int num2 = 0;
            if (!timeout.IsInfinite)
            {
                long num = timeout.MillisecondsRemaining / 0x3e8L;
                if (0x7fffffffL > num)
                {
                    num2 = (int) num;
                }
            }
            rec.timeout = num2;
            rec.userInstance = base.ConnectionOptions.UserInstance;
            rec.hostName = base.ConnectionOptions.ObtainWorkstationId();
            rec.userName = base.ConnectionOptions.UserID;
            rec.password = base.ConnectionOptions.Password;
            rec.applicationName = base.ConnectionOptions.ApplicationName;
            rec.language = this._currentLanguage;
            if (!rec.userInstance)
            {
                rec.database = base.CurrentDatabase;
                rec.attachDBFilename = base.ConnectionOptions.AttachDBFilename;
            }
            rec.serverName = server.UserServerName;
            rec.useReplication = base.ConnectionOptions.Replication;
            rec.useSSPI = base.ConnectionOptions.IntegratedSecurity;
            rec.packetSize = this._currentPacketSize;
            rec.newPassword = newPassword;
            rec.readOnlyIntent = base.ConnectionOptions.ApplicationIntent == ApplicationIntent.ReadOnly;
            this._parser.TdsLogin(rec);
        }

        private void LoginFailure()
        {
            Bid.Trace("<sc.SqlInternalConnectionTds.LoginFailure|RES|CPOOL> %d#\n", base.ObjectID);
            if (this._parser != null)
            {
                this._parser.Disconnect();
            }
        }

        private void LoginNoFailover(ServerInfo serverInfo, string newPassword, bool redirectedUserInstance, SqlConnection owningObject, SqlConnectionString connectionOptions, TimeoutTimer timeout)
        {
            int num2 = 0;
            ServerInfo info = serverInfo;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionTds.LoginNoFailover|ADV> %d#, host=%ls\n", base.ObjectID, serverInfo.UserServerName);
            }
            int num = 100;
            this.ResolveExtendedServerName(serverInfo, !redirectedUserInstance, owningObject);
        Label_0032:
            if (this._parser != null)
            {
                this._parser.Disconnect();
            }
            this._parser = new TdsParser(base.ConnectionOptions.MARS, base.ConnectionOptions.Asynchronous);
            try
            {
                this.AttemptOneLogin(serverInfo, newPassword, true, timeout, owningObject);
                if (connectionOptions.MultiSubnetFailover && (this.ServerProvidedFailOverPartner != null))
                {
                    bool serverProvidedFailoverPartner = true;
                    throw SQL.MultiSubnetFailoverWithFailoverPartner(serverProvidedFailoverPartner);
                }
                if (this._routingInfo == null)
                {
                    if (this.PoolGroupProviderInfo != null)
                    {
                        this.PoolGroupProviderInfo.FailoverCheck(this, false, connectionOptions, this.ServerProvidedFailOverPartner);
                    }
                    base.CurrentDataSource = info.UserServerName;
                    return;
                }
                Bid.Trace("<sc.SqlInternalConnectionTds.LoginNoFailover> Routed to %ls", serverInfo.ExtendedServerName);
                if (num2 > 0)
                {
                    throw SQL.ROR_RecursiveRoutingNotSupported();
                }
                if (timeout.IsExpired)
                {
                    throw SQL.ROR_TimeoutAfterRoutingInfo();
                }
                serverInfo = new ServerInfo(base.ConnectionOptions, this._routingInfo, serverInfo.ResolvedServerName);
                this._currentPacketSize = base.ConnectionOptions.PacketSize;
                this._currentLanguage = this._originalLanguage = base.ConnectionOptions.CurrentLanguage;
                base.CurrentDatabase = this._originalDatabase = base.ConnectionOptions.InitialCatalog;
                this._currentFailoverPartner = null;
                this._instanceName = string.Empty;
                num2++;
                goto Label_0032;
            }
            catch (SqlException exception)
            {
                if (((this._parser == null) || (this._parser.State != TdsParserState.Closed)) || (this.IsDoNotRetryConnectError(exception) || timeout.IsExpired))
                {
                    throw;
                }
                if (timeout.MillisecondsRemaining <= num)
                {
                    throw;
                }
            }
            if (this.ServerProvidedFailOverPartner != null)
            {
                if (connectionOptions.MultiSubnetFailover)
                {
                    bool flag = true;
                    throw SQL.MultiSubnetFailoverWithFailoverPartner(flag);
                }
                this.LoginWithFailover(true, serverInfo, this.ServerProvidedFailOverPartner, newPassword, redirectedUserInstance, owningObject, connectionOptions, timeout);
            }
            else
            {
                if (Bid.AdvancedOn)
                {
                    Bid.Trace("<sc.SqlInternalConnectionTds.LoginNoFailover|ADV> %d#, sleeping %d{milisec}\n", base.ObjectID, num);
                }
                Thread.Sleep(num);
                num = (num < 500) ? (num * 2) : 0x3e8;
                goto Label_0032;
            }
        }

        private void LoginWithFailover(bool useFailoverHost, ServerInfo primaryServerInfo, string failoverHost, string newPassword, bool redirectedUserInstance, SqlConnection owningObject, SqlConnectionString connectionOptions, TimeoutTimer timeout)
        {
            long num5;
            if (Bid.AdvancedOn)
            {
                Bid.Trace("<sc.SqlInternalConnectionTds.LoginWithFailover|ADV> %d#, useFailover=%d{bool}, primary=", base.ObjectID, useFailoverHost);
                Bid.PutStr(primaryServerInfo.UserServerName);
                Bid.PutStr(", failover=");
                Bid.PutStr(failoverHost);
                Bid.PutStr("\n");
            }
            int num = 100;
            string networkLibrary = base.ConnectionOptions.NetworkLibrary;
            ServerInfo serverInfo = new ServerInfo(connectionOptions, failoverHost);
            this.ResolveExtendedServerName(primaryServerInfo, !redirectedUserInstance, owningObject);
            if (this.ServerProvidedFailOverPartner == null)
            {
                this.ResolveExtendedServerName(serverInfo, !redirectedUserInstance && (failoverHost != primaryServerInfo.UserServerName), owningObject);
            }
            if (timeout.IsInfinite)
            {
                ADP.TimerFromSeconds(15);
                num5 = 0L;
            }
            else
            {
                num5 = (long) (0.08f * timeout.MillisecondsRemaining);
            }
            bool flag = false;
            int num2 = 0;
            while (true)
            {
                ServerInfo info2;
                long milliseconds = num5 * ((num2 / 2) + 1);
                long millisecondsRemaining = timeout.MillisecondsRemaining;
                if (milliseconds > millisecondsRemaining)
                {
                    milliseconds = millisecondsRemaining;
                }
                TimeoutTimer timer = TimeoutTimer.StartMillisecondsTimeout(milliseconds);
                if (this._parser != null)
                {
                    this._parser.Disconnect();
                }
                this._parser = new TdsParser(base.ConnectionOptions.MARS, base.ConnectionOptions.Asynchronous);
                if (useFailoverHost)
                {
                    if (!flag)
                    {
                        this.FailoverPermissionDemand();
                        flag = true;
                    }
                    if ((this.ServerProvidedFailOverPartner != null) && (serverInfo.ResolvedServerName != this.ServerProvidedFailOverPartner))
                    {
                        if (Bid.AdvancedOn)
                        {
                            Bid.Trace("<sc.SqlInternalConnectionTds.LoginWithFailover|ADV> %d#, new failover partner=%ls\n", base.ObjectID, this.ServerProvidedFailOverPartner);
                        }
                        serverInfo.SetDerivedNames(networkLibrary, this.ServerProvidedFailOverPartner);
                    }
                    info2 = serverInfo;
                }
                else
                {
                    info2 = primaryServerInfo;
                }
                try
                {
                    this.AttemptOneLogin(info2, newPassword, false, timer, owningObject);
                    if (this._routingInfo != null)
                    {
                        Bid.Trace("<sc.SqlInternalConnectionTds.LoginWithFailover> Routed to %ls", this._routingInfo.ServerName);
                        throw SQL.ROR_UnexpectedRoutingInfo();
                    }
                    break;
                }
                catch (SqlException exception)
                {
                    if (this.IsDoNotRetryConnectError(exception) || timeout.IsExpired)
                    {
                        throw;
                    }
                    if (base.IsConnectionDoomed)
                    {
                        throw;
                    }
                    if ((1 == (num2 % 2)) && (timeout.MillisecondsRemaining <= num))
                    {
                        throw;
                    }
                }
                if (1 == (num2 % 2))
                {
                    if (Bid.AdvancedOn)
                    {
                        Bid.Trace("<sc.SqlInternalConnectionTds.LoginWithFailover|ADV> %d#, sleeping %d{milisec}\n", base.ObjectID, num);
                    }
                    Thread.Sleep(num);
                    num = (num < 500) ? (num * 2) : 0x3e8;
                }
                num2++;
                useFailoverHost = !useFailoverHost;
            }
            if (useFailoverHost && (this.ServerProvidedFailOverPartner == null))
            {
                throw SQL.InvalidPartnerConfiguration(failoverHost, base.CurrentDatabase);
            }
            if (this.PoolGroupProviderInfo != null)
            {
                this.PoolGroupProviderInfo.FailoverCheck(this, useFailoverHost, connectionOptions, this.ServerProvidedFailOverPartner);
            }
            base.CurrentDataSource = useFailoverHost ? failoverHost : primaryServerInfo.UserServerName;
        }

        internal void OnEnvChange(SqlEnvChange rec)
        {
            switch (rec.type)
            {
                case 1:
                    if (!this._fConnectionOpen)
                    {
                        this._originalDatabase = rec.newValue;
                    }
                    base.CurrentDatabase = rec.newValue;
                    return;

                case 2:
                    if (!this._fConnectionOpen)
                    {
                        this._originalLanguage = rec.newValue;
                    }
                    this._currentLanguage = rec.newValue;
                    return;

                case 3:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 14:
                case 0x10:
                case 0x11:
                case 0x12:
                    break;

                case 4:
                    this._currentPacketSize = int.Parse(rec.newValue, CultureInfo.InvariantCulture);
                    return;

                case 13:
                    if (base.ConnectionOptions.ApplicationIntent == ApplicationIntent.ReadOnly)
                    {
                        throw SQL.ROR_FailoverNotSupportedServer();
                    }
                    this._currentFailoverPartner = rec.newValue;
                    return;

                case 15:
                    base.PromotedDTCToken = rec.newBinValue;
                    return;

                case 0x13:
                    this._instanceName = rec.newValue;
                    return;

                case 20:
                    if ((string.IsNullOrEmpty(rec.newRoutingInfo.ServerName) || (rec.newRoutingInfo.Protocol != 0)) || (rec.newRoutingInfo.Port == 0))
                    {
                        throw SQL.ROR_InvalidRoutingInfo();
                    }
                    this._routingInfo = rec.newRoutingInfo;
                    break;

                default:
                    return;
            }
        }

        internal void OnLoginAck(SqlLoginAck rec)
        {
            this._loginAck = rec;
        }

        private void OpenLoginEnlist(SqlConnection owningObject, TimeoutTimer timeout, SqlConnectionString connectionOptions, string newPassword, bool redirectedUserInstance)
        {
            string failoverPartner;
            bool useFailoverPartner;
            ServerInfo primaryServerInfo = new ServerInfo(connectionOptions);
            if (this.PoolGroupProviderInfo != null)
            {
                useFailoverPartner = this.PoolGroupProviderInfo.UseFailoverPartner;
                failoverPartner = this.PoolGroupProviderInfo.FailoverPartner;
            }
            else
            {
                useFailoverPartner = false;
                failoverPartner = base.ConnectionOptions.FailoverPartner;
            }
            bool flag2 = !ADP.IsEmpty(failoverPartner);
            try
            {
                if (flag2)
                {
                    this.LoginWithFailover(useFailoverPartner, primaryServerInfo, failoverPartner, newPassword, redirectedUserInstance, owningObject, connectionOptions, timeout);
                }
                else
                {
                    this.LoginNoFailover(primaryServerInfo, newPassword, redirectedUserInstance, owningObject, connectionOptions, timeout);
                }
            }
            catch (Exception exception)
            {
                if (ADP.IsCatchableExceptionType(exception))
                {
                    this.LoginFailure();
                }
                throw;
            }
        }

        protected override void PropagateTransactionCookie(byte[] cookie)
        {
            this._parser.PropagateDistributedTransaction(cookie, base.ConnectionOptions.ConnectTimeout, this._parser._physicalStateObj);
        }

        internal override void RemovePreparedCommand(SqlCommand cmd)
        {
            if ((this._preparedCommands != null) && (this._preparedCommands.Count != 0))
            {
                for (int i = 0; i < this._preparedCommands.Count; i++)
                {
                    if (this._preparedCommands[i].Target == cmd)
                    {
                        this._preparedCommands[i].Target = null;
                        return;
                    }
                }
            }
        }

        private void ResetConnection()
        {
            if (this._fResetConnection)
            {
                if (this.IsShiloh)
                {
                    this._parser.PrepareResetConnection(this.IsTransactionRoot && !this.IsNonPoolableTransactionRoot);
                }
                else if (!base.IsEnlistedInTransaction)
                {
                    try
                    {
                        this._parser.TdsExecuteSQLBatch("sp_reset_connection", 30, null, this._parser._physicalStateObj);
                        this._parser.Run(RunBehavior.UntilDone, null, null, null, this._parser._physicalStateObj);
                    }
                    catch (Exception exception)
                    {
                        if (!ADP.IsCatchableExceptionType(exception))
                        {
                            throw;
                        }
                        base.DoomThisConnection();
                        ADP.TraceExceptionWithoutRethrow(exception);
                    }
                }
                base.CurrentDatabase = this._originalDatabase;
                this._currentLanguage = this._originalLanguage;
            }
        }

        private void ResolveExtendedServerName(ServerInfo serverInfo, bool aliasLookup, SqlConnection owningObject)
        {
            if (serverInfo.ExtendedServerName == null)
            {
                string userServerName = serverInfo.UserServerName;
                string userProtocol = serverInfo.UserProtocol;
                if (aliasLookup)
                {
                    TdsParserStaticMethods.AliasRegistryLookup(ref userServerName, ref userProtocol);
                    if ((owningObject != null) && ((SqlConnectionString) owningObject.UserConnectionOptions).EnforceLocalHost)
                    {
                        SqlConnectionString.VerifyLocalHostAndFixup(ref userServerName, true, true);
                    }
                }
                serverInfo.SetDerivedNames(userProtocol, userServerName);
            }
        }

        internal override void ValidateConnectionForExecute(SqlCommand command)
        {
            SqlDataReader reader = null;
            if (this.Parser.MARSOn)
            {
                if (command != null)
                {
                    reader = base.FindLiveReader(command);
                }
            }
            else
            {
                if (this._asyncCommandCount > 0)
                {
                    throw SQL.MARSUnspportedOnConnection();
                }
                reader = base.FindLiveReader(null);
            }
            if (reader != null)
            {
                throw ADP.OpenReaderExists();
            }
            if (!this.Parser.MARSOn && this.Parser._physicalStateObj._pendingData)
            {
                this.Parser._physicalStateObj.CleanWire();
            }
            this.Parser.RollbackOrphanedAPITransactions();
        }

        internal override SqlInternalTransaction AvailableInternalTransaction
        {
            get
            {
                if (!this._parser._fResetConnection)
                {
                    return this.CurrentTransaction;
                }
                return null;
            }
        }

        internal override SqlInternalTransaction CurrentTransaction
        {
            get
            {
                return this._parser.CurrentTransaction;
            }
        }

        internal DbConnectionPoolIdentity Identity
        {
            get
            {
                return this._identity;
            }
        }

        internal bool IgnoreEnvChange
        {
            get
            {
                return (this._routingInfo != null);
            }
        }

        internal string InstanceName
        {
            get
            {
                return this._instanceName;
            }
        }

        internal override bool IsKatmaiOrNewer
        {
            get
            {
                return this._parser.IsKatmaiOrNewer;
            }
        }

        internal override bool IsLockedForBulkCopy
        {
            get
            {
                return (!this.Parser.MARSOn && this.Parser._physicalStateObj.BcpLock);
            }
        }

        protected internal override bool IsNonPoolableTransactionRoot
        {
            get
            {
                if (!this.IsTransactionRoot)
                {
                    return false;
                }
                if (this.IsKatmaiOrNewer)
                {
                    return (null == base.Pool);
                }
                return true;
            }
        }

        internal override bool IsShiloh
        {
            get
            {
                return this._loginAck.isVersion8;
            }
        }

        internal override bool IsYukonOrNewer
        {
            get
            {
                return this._parser.IsYukonOrNewer;
            }
        }

        internal int PacketSize
        {
            get
            {
                return this._currentPacketSize;
            }
        }

        internal TdsParser Parser
        {
            get
            {
                return this._parser;
            }
        }

        internal override SqlInternalTransaction PendingTransaction
        {
            get
            {
                return this._parser.PendingTransaction;
            }
        }

        internal SqlConnectionPoolGroupProviderInfo PoolGroupProviderInfo
        {
            get
            {
                return this._poolGroupProviderInfo;
            }
        }

        protected override bool ReadyToPrepareTransaction
        {
            get
            {
                return (null == base.FindLiveReader(null));
            }
        }

        internal string ServerProvidedFailOverPartner
        {
            get
            {
                return this._currentFailoverPartner;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return string.Format(null, "{0:00}.{1:00}.{2:0000}", new object[] { this._loginAck.majorVersion, (short) this._loginAck.minorVersion, this._loginAck.buildNum });
            }
        }

        protected override bool UnbindOnTransactionCompletion
        {
            get
            {
                return false;
            }
        }
    }
}

