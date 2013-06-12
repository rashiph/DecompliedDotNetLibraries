namespace System.Threading
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.ExceptionServices;
    using System.Security;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.ControlPolicy | SecurityPermissionFlag.ControlEvidence)]
    public class SynchronizationContext
    {
        private SynchronizationContextProperties _props;

        public virtual SynchronizationContext CreateCopy()
        {
            return new SynchronizationContext();
        }

        [SecurityCritical]
        private static int InvokeWaitMethodHelper(SynchronizationContext syncContext, IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            return syncContext.Wait(waitHandles, waitAll, millisecondsTimeout);
        }

        public bool IsWaitNotificationRequired()
        {
            return ((this._props & SynchronizationContextProperties.RequireWaitNotification) != SynchronizationContextProperties.None);
        }

        public virtual void OperationCompleted()
        {
        }

        public virtual void OperationStarted()
        {
        }

        public virtual void Post(SendOrPostCallback d, object state)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(d.Invoke), state);
        }

        public virtual void Send(SendOrPostCallback d, object state)
        {
            d(state);
        }

        [SecurityCritical, TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static void SetSynchronizationContext(SynchronizationContext syncContext)
        {
            SetSynchronizationContext(syncContext, Thread.CurrentThread.ExecutionContext.SynchronizationContext);
        }

        [SecurityCritical, HandleProcessCorruptedStateExceptions]
        internal static SynchronizationContextSwitcher SetSynchronizationContext(SynchronizationContext syncContext, SynchronizationContext prevSyncContext)
        {
            ExecutionContext executionContext = Thread.CurrentThread.ExecutionContext;
            SynchronizationContextSwitcher switcher = new SynchronizationContextSwitcher();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                switcher._ec = executionContext;
                switcher.savedSC = prevSyncContext;
                switcher.currSC = syncContext;
                executionContext.SynchronizationContext = syncContext;
            }
            catch
            {
                switcher.UndoNoThrow();
                throw;
            }
            return switcher;
        }

        [SecuritySafeCritical]
        protected void SetWaitNotificationRequired()
        {
            RuntimeHelpers.PrepareDelegate(new WaitDelegate(this.Wait));
            this._props |= SynchronizationContextProperties.RequireWaitNotification;
        }

        [PrePrepareMethod, CLSCompliant(false), SecurityCritical]
        public virtual int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
        {
            if (waitHandles == null)
            {
                throw new ArgumentNullException("waitHandles");
            }
            return WaitHelper(waitHandles, waitAll, millisecondsTimeout);
        }

        [MethodImpl(MethodImplOptions.InternalCall), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), PrePrepareMethod, SecurityCritical, CLSCompliant(false)]
        protected static extern int WaitHelper(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout);

        public static SynchronizationContext Current
        {
            [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get
            {
                SynchronizationContext synchronizationContext = null;
                ExecutionContext executionContextNoCreate = Thread.CurrentThread.GetExecutionContextNoCreate();
                if (executionContextNoCreate != null)
                {
                    synchronizationContext = executionContextNoCreate.SynchronizationContext;
                }
                return synchronizationContext;
            }
        }
    }
}

