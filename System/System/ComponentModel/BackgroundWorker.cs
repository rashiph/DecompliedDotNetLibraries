namespace System.ComponentModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [DefaultEvent("DoWork"), SRDescription("BackgroundWorker_Desc"), HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class BackgroundWorker : Component
    {
        private AsyncOperation asyncOperation;
        private bool canCancelWorker;
        private bool cancellationPending;
        private static readonly object doWorkKey = new object();
        private bool isRunning;
        private readonly SendOrPostCallback operationCompleted;
        private static readonly object progressChangedKey = new object();
        private readonly SendOrPostCallback progressReporter;
        private static readonly object runWorkerCompletedKey = new object();
        private readonly WorkerThreadStartDelegate threadStart;
        private bool workerReportsProgress;

        [SRCategory("PropertyCategoryAsynchronous"), SRDescription("BackgroundWorker_DoWork")]
        public event DoWorkEventHandler DoWork
        {
            add
            {
                base.Events.AddHandler(doWorkKey, value);
            }
            remove
            {
                base.Events.RemoveHandler(doWorkKey, value);
            }
        }

        [SRDescription("BackgroundWorker_ProgressChanged"), SRCategory("PropertyCategoryAsynchronous")]
        public event ProgressChangedEventHandler ProgressChanged
        {
            add
            {
                base.Events.AddHandler(progressChangedKey, value);
            }
            remove
            {
                base.Events.RemoveHandler(progressChangedKey, value);
            }
        }

        [SRDescription("BackgroundWorker_RunWorkerCompleted"), SRCategory("PropertyCategoryAsynchronous")]
        public event RunWorkerCompletedEventHandler RunWorkerCompleted
        {
            add
            {
                base.Events.AddHandler(runWorkerCompletedKey, value);
            }
            remove
            {
                base.Events.RemoveHandler(runWorkerCompletedKey, value);
            }
        }

        public BackgroundWorker()
        {
            this.threadStart = new WorkerThreadStartDelegate(this.WorkerThreadStart);
            this.operationCompleted = new SendOrPostCallback(this.AsyncOperationCompleted);
            this.progressReporter = new SendOrPostCallback(this.ProgressReporter);
        }

        private void AsyncOperationCompleted(object arg)
        {
            this.isRunning = false;
            this.cancellationPending = false;
            this.OnRunWorkerCompleted((RunWorkerCompletedEventArgs) arg);
        }

        public void CancelAsync()
        {
            if (!this.WorkerSupportsCancellation)
            {
                throw new InvalidOperationException(SR.GetString("BackgroundWorker_WorkerDoesntSupportCancellation"));
            }
            this.cancellationPending = true;
        }

        protected virtual void OnDoWork(DoWorkEventArgs e)
        {
            DoWorkEventHandler handler = (DoWorkEventHandler) base.Events[doWorkKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = (ProgressChangedEventHandler) base.Events[progressChangedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            RunWorkerCompletedEventHandler handler = (RunWorkerCompletedEventHandler) base.Events[runWorkerCompletedKey];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void ProgressReporter(object arg)
        {
            this.OnProgressChanged((ProgressChangedEventArgs) arg);
        }

        public void ReportProgress(int percentProgress)
        {
            this.ReportProgress(percentProgress, null);
        }

        public void ReportProgress(int percentProgress, object userState)
        {
            if (!this.WorkerReportsProgress)
            {
                throw new InvalidOperationException(SR.GetString("BackgroundWorker_WorkerDoesntReportProgress"));
            }
            ProgressChangedEventArgs arg = new ProgressChangedEventArgs(percentProgress, userState);
            if (this.asyncOperation != null)
            {
                this.asyncOperation.Post(this.progressReporter, arg);
            }
            else
            {
                this.progressReporter(arg);
            }
        }

        public void RunWorkerAsync()
        {
            this.RunWorkerAsync(null);
        }

        public void RunWorkerAsync(object argument)
        {
            if (this.isRunning)
            {
                throw new InvalidOperationException(SR.GetString("BackgroundWorker_WorkerAlreadyRunning"));
            }
            this.isRunning = true;
            this.cancellationPending = false;
            this.asyncOperation = AsyncOperationManager.CreateOperation(null);
            this.threadStart.BeginInvoke(argument, null, null);
        }

        private void WorkerThreadStart(object argument)
        {
            object result = null;
            Exception error = null;
            bool cancelled = false;
            try
            {
                DoWorkEventArgs e = new DoWorkEventArgs(argument);
                this.OnDoWork(e);
                if (e.Cancel)
                {
                    cancelled = true;
                }
                else
                {
                    result = e.Result;
                }
            }
            catch (Exception exception2)
            {
                error = exception2;
            }
            RunWorkerCompletedEventArgs arg = new RunWorkerCompletedEventArgs(result, error, cancelled);
            this.asyncOperation.PostOperationCompleted(this.operationCompleted, arg);
        }

        [Browsable(false), SRDescription("BackgroundWorker_CancellationPending")]
        public bool CancellationPending
        {
            get
            {
                return this.cancellationPending;
            }
        }

        [SRDescription("BackgroundWorker_IsBusy"), Browsable(false)]
        public bool IsBusy
        {
            get
            {
                return this.isRunning;
            }
        }

        [SRDescription("BackgroundWorker_WorkerReportsProgress"), DefaultValue(false), SRCategory("PropertyCategoryAsynchronous")]
        public bool WorkerReportsProgress
        {
            get
            {
                return this.workerReportsProgress;
            }
            set
            {
                this.workerReportsProgress = value;
            }
        }

        [DefaultValue(false), SRDescription("BackgroundWorker_WorkerSupportsCancellation"), SRCategory("PropertyCategoryAsynchronous")]
        public bool WorkerSupportsCancellation
        {
            get
            {
                return this.canCancelWorker;
            }
            set
            {
                this.canCancelWorker = value;
            }
        }

        private delegate void WorkerThreadStartDelegate(object argument);
    }
}

