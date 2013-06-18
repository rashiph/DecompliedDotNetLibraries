namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class WindowsFormsSynchronizationContext : SynchronizationContext, IDisposable
    {
        private Control controlToSendTo;
        private WeakReference destinationThreadRef;
        [ThreadStatic]
        private static bool dontAutoInstall;
        [ThreadStatic]
        private static bool inSyncContextInstallation;
        [ThreadStatic]
        private static SynchronizationContext previousSyncContext;

        public WindowsFormsSynchronizationContext()
        {
            this.DestinationThread = Thread.CurrentThread;
            Application.ThreadContext context = Application.ThreadContext.FromCurrent();
            if (context != null)
            {
                this.controlToSendTo = context.MarshalingControl;
            }
        }

        private WindowsFormsSynchronizationContext(Control marshalingControl, Thread destinationThread)
        {
            this.controlToSendTo = marshalingControl;
            this.DestinationThread = destinationThread;
        }

        public override SynchronizationContext CreateCopy()
        {
            return new WindowsFormsSynchronizationContext(this.controlToSendTo, this.DestinationThread);
        }

        public void Dispose()
        {
            if (this.controlToSendTo != null)
            {
                if (!this.controlToSendTo.IsDisposed)
                {
                    this.controlToSendTo.Dispose();
                }
                this.controlToSendTo = null;
            }
        }

        internal static void InstallIfNeeded()
        {
            if (AutoInstall && !inSyncContextInstallation)
            {
                if (SynchronizationContext.Current == null)
                {
                    previousSyncContext = null;
                }
                if (previousSyncContext == null)
                {
                    inSyncContextInstallation = true;
                    try
                    {
                        SynchronizationContext synchronizationContext = AsyncOperationManager.SynchronizationContext;
                        if ((synchronizationContext == null) || (synchronizationContext.GetType() == typeof(SynchronizationContext)))
                        {
                            previousSyncContext = synchronizationContext;
                            new PermissionSet(PermissionState.Unrestricted).Assert();
                            try
                            {
                                AsyncOperationManager.SynchronizationContext = new WindowsFormsSynchronizationContext();
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                    finally
                    {
                        inSyncContextInstallation = false;
                    }
                }
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            if (this.controlToSendTo != null)
            {
                this.controlToSendTo.BeginInvoke(d, new object[] { state });
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            Thread destinationThread = this.DestinationThread;
            if ((destinationThread == null) || !destinationThread.IsAlive)
            {
                throw new InvalidAsynchronousStateException(System.Windows.Forms.SR.GetString("ThreadNoLongerValid"));
            }
            if (this.controlToSendTo != null)
            {
                this.controlToSendTo.Invoke(d, new object[] { state });
            }
        }

        public static void Uninstall()
        {
            Uninstall(true);
        }

        internal static void Uninstall(bool turnOffAutoInstall)
        {
            if (AutoInstall && (AsyncOperationManager.SynchronizationContext is WindowsFormsSynchronizationContext))
            {
                try
                {
                    new PermissionSet(PermissionState.Unrestricted).Assert();
                    if (previousSyncContext == null)
                    {
                        AsyncOperationManager.SynchronizationContext = new SynchronizationContext();
                    }
                    else
                    {
                        AsyncOperationManager.SynchronizationContext = previousSyncContext;
                    }
                }
                finally
                {
                    previousSyncContext = null;
                    CodeAccessPermission.RevertAssert();
                }
            }
            if (turnOffAutoInstall)
            {
                AutoInstall = false;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static bool AutoInstall
        {
            get
            {
                return !dontAutoInstall;
            }
            set
            {
                dontAutoInstall = !value;
            }
        }

        private Thread DestinationThread
        {
            get
            {
                if ((this.destinationThreadRef != null) && this.destinationThreadRef.IsAlive)
                {
                    return (this.destinationThreadRef.Target as Thread);
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    this.destinationThreadRef = new WeakReference(value);
                }
            }
        }
    }
}

