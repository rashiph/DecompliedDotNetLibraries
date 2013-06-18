namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal class ComPlusSynchronizationContext : SynchronizationContext
    {
        private IServiceActivity activity;
        private bool postSynchronous;

        public ComPlusSynchronizationContext(IServiceActivity activity, bool postSynchronous)
        {
            this.activity = activity;
            this.postSynchronous = postSynchronous;
        }

        public void Dispose()
        {
            while (Marshal.ReleaseComObject(this.activity) > 0)
            {
            }
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            ComPlusActivityTrace.Trace(TraceEventType.Verbose, 0x50015, "TraceCodeComIntegrationEnteringActivity");
            ServiceCall pIServiceCall = new ServiceCall(d, state);
            if (this.postSynchronous)
            {
                this.activity.SynchronousCall(pIServiceCall);
            }
            else
            {
                this.activity.AsynchronousCall(pIServiceCall);
            }
            ComPlusActivityTrace.Trace(TraceEventType.Verbose, 0x50017, "TraceCodeComIntegrationLeftActivity");
        }

        public override void Send(SendOrPostCallback d, object state)
        {
        }

        private class ServiceCall : IServiceCall
        {
            private SendOrPostCallback callback;
            private object state;

            public ServiceCall(SendOrPostCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
            }

            public void OnCall()
            {
                ServiceModelActivity activity = null;
                try
                {
                    Guid empty = Guid.Empty;
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        IComThreadingInfo info = (IComThreadingInfo) SafeNativeMethods.CoGetObjectContext(ComPlusActivityTrace.IID_IComThreadingInfo);
                        if (info != null)
                        {
                            info.GetCurrentLogicalThreadId(out empty);
                            activity = ServiceModelActivity.CreateBoundedActivity(empty);
                        }
                        ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("TransferringToComplus", new object[] { empty.ToString() }), ActivityType.TransferToComPlus);
                    }
                    ComPlusActivityTrace.Trace(TraceEventType.Verbose, 0x50016, "TraceCodeComIntegrationExecutingCall");
                    this.callback(this.state);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    DiagnosticUtility.InvokeFinalHandler(exception);
                }
                finally
                {
                    if (activity != null)
                    {
                        activity.Dispose();
                        activity = null;
                    }
                }
            }
        }
    }
}

