namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Transactions;
    using System.Workflow.Runtime;

    public class DefaultWorkflowCommitWorkBatchService : WorkflowCommitWorkBatchService
    {
        private bool _enableRetries;
        private bool _ignoreCommonEnableRetries;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DefaultWorkflowCommitWorkBatchService()
        {
        }

        public DefaultWorkflowCommitWorkBatchService(NameValueCollection parameters)
        {
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
        }

        protected internal override void CommitWorkBatch(WorkflowCommitWorkBatchService.CommitWorkBatchCallback commitWorkBatchCallback)
        {
            DbRetry retry = new DbRetry(this._enableRetries);
            short retryCount = 0;
        Label_000E:
            if (null != Transaction.Current)
            {
                retryCount = retry.MaxRetries;
            }
            try
            {
                base.CommitWorkBatch(commitWorkBatchCallback);
            }
            catch (Exception exception)
            {
                WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "DefaultWorkflowCommitWorkBatchService caught exception from commitWorkBatchCallback: " + exception.ToString());
                if (!retry.TryDoRetry(ref retryCount))
                {
                    throw;
                }
                WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "DefaultWorkflowCommitWorkBatchService retrying commitWorkBatchCallback (retry attempt " + retryCount.ToString(CultureInfo.InvariantCulture) + ")");
                goto Label_000E;
            }
        }

        protected override void OnStopped()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "DefaultWorkflowCommitWorkBatchService: Stopping");
            base.OnStopped();
        }

        protected internal override void Start()
        {
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "DefaultWorkflowCommitWorkBatchService: Starting");
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

