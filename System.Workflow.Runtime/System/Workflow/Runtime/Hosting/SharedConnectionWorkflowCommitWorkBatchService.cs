namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Threading;
    using System.Transactions;
    using System.Workflow.Runtime;

    public class SharedConnectionWorkflowCommitWorkBatchService : WorkflowCommitWorkBatchService
    {
        private bool _enableRetries;
        private bool _ignoreCommonEnableRetries;
        private NameValueCollection configParameters;
        private DbResourceAllocator dbResourceAllocator;
        private object tableSyncObject;
        private IDictionary<Transaction, SharedConnectionInfo> transactionConnectionTable;
        private string unvalidatedConnectionString;

        public SharedConnectionWorkflowCommitWorkBatchService(NameValueCollection parameters)
        {
            this.tableSyncObject = new object();
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters", ExecutionStringManager.MissingParameters);
            }
            if (parameters.Count > 0)
            {
                foreach (string str in parameters.Keys)
                {
                    if (string.Compare("EnableRetries", str, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._enableRetries = bool.Parse(parameters[str]);
                        this._ignoreCommonEnableRetries = true;
                    }
                }
            }
            this.configParameters = parameters;
        }

        public SharedConnectionWorkflowCommitWorkBatchService(string connectionString)
        {
            this.tableSyncObject = new object();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);
            }
            this.unvalidatedConnectionString = connectionString;
        }

        private void AddToConnectionInfoTable(Transaction transaction, SharedConnectionInfo connectionInfo)
        {
            lock (this.tableSyncObject)
            {
                this.transactionConnectionTable.Add(transaction, connectionInfo);
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "AddToConnectionInfoTable ", transaction.GetHashCode(), " in table of count ", this.transactionConnectionTable.Count }));
            }
        }

        protected internal override void CommitWorkBatch(WorkflowCommitWorkBatchService.CommitWorkBatchCallback commitWorkBatchCallback)
        {
            ManualResetEvent event2;
            DbRetry retry = new DbRetry(this._enableRetries);
            short maxRetries = retry.MaxRetries;
        Label_0013:
            event2 = new ManualResetEvent(false);
            Transaction transaction = null;
            SharedConnectionInfo connectionInfo = null;
            try
            {
                if (null == Transaction.Current)
                {
                    maxRetries = 0;
                    transaction = new CommittableTransaction();
                    connectionInfo = new SharedConnectionInfo(this.dbResourceAllocator, transaction, false, event2);
                }
                else
                {
                    transaction = Transaction.Current.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                    connectionInfo = new SharedConnectionInfo(this.dbResourceAllocator, transaction, true, event2);
                }
                this.AddToConnectionInfoTable(transaction, connectionInfo);
                using (TransactionScope scope = new TransactionScope(transaction))
                {
                    try
                    {
                        commitWorkBatchCallback();
                        scope.Complete();
                    }
                    finally
                    {
                        this.RemoveConnectionFromInfoTable(transaction);
                        event2.Set();
                    }
                }
                CommittableTransaction transaction2 = transaction as CommittableTransaction;
                if (transaction2 != null)
                {
                    transaction2.Commit();
                }
                DependentTransaction transaction3 = transaction as DependentTransaction;
                if (transaction3 != null)
                {
                    transaction3.Complete();
                }
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SharedConnectionWorkflowCommitWorkBatchService caught exception from commitWorkBatchCallback: " + exception.ToString());
                if (!retry.TryDoRetry(ref maxRetries))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SharedConnectionWorkflowCommitWorkBatchService retrying commitWorkBatchCallback (retry attempt " + maxRetries.ToString(CultureInfo.InvariantCulture) + ")");
                goto Label_0013;
            }
            finally
            {
                event2.Close();
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SharedConnectionInfo GetConnectionInfo(Transaction transaction)
        {
            return this.LookupConnectionInfoTable(transaction);
        }

        private SharedConnectionInfo LookupConnectionInfoTable(Transaction transaction)
        {
            lock (this.tableSyncObject)
            {
                return this.transactionConnectionTable[transaction];
            }
        }

        protected override void OnStopped()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SharedConnectionWorkflowCommitWorkBatchService: Stopping");
            foreach (KeyValuePair<Transaction, SharedConnectionInfo> pair in this.transactionConnectionTable)
            {
                pair.Value.Dispose();
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Removing transaction " + pair.Key.GetHashCode());
            }
            this.transactionConnectionTable.Clear();
            this.dbResourceAllocator = null;
            base.OnStopped();
        }

        private void RemoveConnectionFromInfoTable(Transaction transaction)
        {
            lock (this.tableSyncObject)
            {
                SharedConnectionInfo info;
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "TransactionCompleted " + transaction.GetHashCode());
                if (this.transactionConnectionTable.TryGetValue(transaction, out info))
                {
                    info.Dispose();
                    this.transactionConnectionTable.Remove(transaction);
                }
                else
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, string.Concat(new object[] { "TransactionCompleted ", transaction.GetHashCode(), " not found in table of count ", this.transactionConnectionTable.Count }));
                }
            }
        }

        protected internal override void Start()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SharedConnectionWorkflowCommitWorkBatchService: Starting");
            this.dbResourceAllocator = new DbResourceAllocator(base.Runtime, this.configParameters, this.unvalidatedConnectionString);
            if (this.transactionConnectionTable == null)
            {
                this.transactionConnectionTable = new Dictionary<Transaction, SharedConnectionInfo>();
            }
            if (!this._ignoreCommonEnableRetries && (base.Runtime != null))
            {
                NameValueConfigurationCollection commonParameters = base.Runtime.CommonParameters;
                if (commonParameters != null)
                {
                    foreach (string str in commonParameters.AllKeys)
                    {
                        if (string.Compare("EnableRetries", str, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._enableRetries = bool.Parse(commonParameters[str].Value);
                            break;
                        }
                    }
                }
            }
            base.Start();
        }

        internal string ConnectionString
        {
            get
            {
                if (this.dbResourceAllocator == null)
                {
                    if (base.Runtime == null)
                    {
                        throw new InvalidOperationException(ExecutionStringManager.WorkflowRuntimeNotStarted);
                    }
                    this.dbResourceAllocator = new DbResourceAllocator(base.Runtime, this.configParameters, this.unvalidatedConnectionString);
                }
                return this.dbResourceAllocator.ConnectionString;
            }
        }

        public bool EnableRetries
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._enableRetries;
            }
            set
            {
                this._enableRetries = value;
                this._ignoreCommonEnableRetries = true;
            }
        }
    }
}

