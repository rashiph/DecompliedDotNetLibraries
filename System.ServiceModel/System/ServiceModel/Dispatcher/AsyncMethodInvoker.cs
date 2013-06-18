namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;

    internal class AsyncMethodInvoker : IOperationInvoker
    {
        private MethodInfo beginMethod;
        private MethodInfo endMethod;
        private int inputParameterCount;
        private System.ServiceModel.Dispatcher.InvokeBeginDelegate invokeBeginDelegate;
        private System.ServiceModel.Dispatcher.InvokeEndDelegate invokeEndDelegate;
        private int outputParameterCount;

        public AsyncMethodInvoker(MethodInfo beginMethod, MethodInfo endMethod)
        {
            if (beginMethod == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("beginMethod"));
            }
            if (endMethod == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("endMethod"));
            }
            this.beginMethod = beginMethod;
            this.endMethod = endMethod;
        }

        public object[] AllocateInputs()
        {
            return EmptyArray.Allocate(this.InputParameterCount);
        }

        private void CreateActivityInfo(ref ServiceModelActivity activity, ref Activity boundActivity)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                activity = ServiceModelActivity.CreateAsyncActivity();
                TraceUtility.UpdateAsyncOperationContextWithActivity(activity);
                boundActivity = ServiceModelActivity.BoundOperation(activity, true);
            }
            else if (TraceUtility.MessageFlowTracingOnly)
            {
                Guid receivedActivityId = TraceUtility.GetReceivedActivityId(OperationContext.Current);
                if (receivedActivityId != Guid.Empty)
                {
                    DiagnosticTrace.ActivityId = receivedActivityId;
                }
            }
            else if (TraceUtility.ShouldPropagateActivity)
            {
                Guid activityId = ActivityIdHeader.ExtractActivityId(OperationContext.Current.IncomingMessage);
                if (activityId != Guid.Empty)
                {
                    boundActivity = Activity.CreateActivity(activityId);
                }
                TraceUtility.UpdateAsyncOperationContextWithActivity(activityId);
            }
        }

        private void EnsureIsInitialized()
        {
            if (this.invokeBeginDelegate == null)
            {
                int num;
                int num2;
                System.ServiceModel.Dispatcher.InvokeBeginDelegate delegate2 = new InvokerUtil().GenerateInvokeBeginDelegate(this.beginMethod, out num);
                this.inputParameterCount = num;
                System.ServiceModel.Dispatcher.InvokeEndDelegate delegate3 = new InvokerUtil().GenerateInvokeEndDelegate(this.endMethod, out num2);
                this.outputParameterCount = num2;
                this.invokeEndDelegate = delegate3;
                this.invokeBeginDelegate = delegate2;
            }
        }

        private void GetActivityInfo(ref ServiceModelActivity activity, ref Activity boundOperation)
        {
            if (TraceUtility.MessageFlowTracingOnly)
            {
                if (OperationContext.Current != null)
                {
                    Guid receivedActivityId = TraceUtility.GetReceivedActivityId(OperationContext.Current);
                    if (receivedActivityId != Guid.Empty)
                    {
                        DiagnosticTrace.ActivityId = receivedActivityId;
                    }
                }
            }
            else if (DiagnosticUtility.ShouldUseActivity || TraceUtility.ShouldPropagateActivity)
            {
                object obj2 = TraceUtility.ExtractAsyncOperationContextActivity();
                if (obj2 != null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        activity = obj2 as ServiceModelActivity;
                        boundOperation = ServiceModelActivity.BoundOperation(activity, true);
                    }
                    else if (TraceUtility.ShouldPropagateActivity && (obj2 is Guid))
                    {
                        Guid activityId = (Guid) obj2;
                        boundOperation = Activity.CreateActivity(activityId);
                    }
                }
            }
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            IAsyncResult result;
            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoServiceObject")));
            }
            if (inputs == null)
            {
                if (this.InputParameterCount > 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInputParametersToServiceNull", new object[] { this.InputParameterCount })));
                }
            }
            else if (inputs.Length != this.InputParameterCount)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInputParametersToServiceInvalid", new object[] { this.InputParameterCount, inputs.Length })));
            }
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.MethodCalled(this.beginMethod.Name.Substring("Begin".Length));
            }
            try
            {
                ServiceModelActivity activity = null;
                Activity boundActivity = null;
                this.CreateActivityInfo(ref activity, ref boundActivity);
                if (TD.OperationInvokedIsEnabled())
                {
                    TD.OperationInvoked(this.beginMethod.Name.Substring("Begin".Length), TraceUtility.GetCallerInfo(OperationContext.Current));
                }
                if ((TD.OperationCompletedIsEnabled() || TD.OperationFaultedIsEnabled()) || TD.OperationFailedIsEnabled())
                {
                    TraceUtility.UpdateAsyncOperationContextWithStartTime(DateTime.UtcNow.Ticks);
                }
                using (boundActivity)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        string activityName = null;
                        if (this.endMethod == null)
                        {
                            activityName = System.ServiceModel.SR.GetString("ActivityExecuteMethod", new object[] { this.beginMethod.DeclaringType.FullName, this.beginMethod.Name });
                        }
                        else
                        {
                            activityName = System.ServiceModel.SR.GetString("ActivityExecuteAsyncMethod", new object[] { this.beginMethod.DeclaringType.FullName, this.beginMethod.Name, this.endMethod.DeclaringType.FullName, this.endMethod.Name });
                        }
                        ServiceModelActivity.Start(activity, activityName, ActivityType.ExecuteUserCode);
                    }
                    result = this.InvokeBeginDelegate(instance, inputs, callback, state);
                }
                ServiceModelActivity.Stop(activity);
            }
            catch (SecurityException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(AuthorizationBehavior.CreateAccessDeniedFaultException());
            }
            catch (Exception exception2)
            {
                TraceUtility.TraceUserCodeException(exception2, this.beginMethod);
                throw;
            }
            return result;
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            object obj2;
            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxNoServiceObject")));
            }
            outputs = EmptyArray.Allocate(this.OutputParameterCount);
            bool flag = true;
            bool flag2 = false;
            try
            {
                ServiceModelActivity activity = null;
                Activity boundOperation = null;
                this.GetActivityInfo(ref activity, ref boundOperation);
                using (boundOperation)
                {
                    obj2 = this.InvokeEndDelegate(instance, outputs, result);
                    flag = false;
                }
                ServiceModelActivity.Stop(activity);
            }
            catch (SecurityException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(AuthorizationBehavior.CreateAccessDeniedFaultException());
            }
            catch (FaultException)
            {
                flag2 = true;
                flag = false;
                throw;
            }
            finally
            {
                if (flag)
                {
                    if (TD.OperationFailedIsEnabled())
                    {
                        TD.OperationFailed(this.EndMethod.Name, TraceUtility.GetUtcBasedDurationForTrace(TraceUtility.ExtractAsyncOperationStartTime()));
                    }
                }
                else if (flag2)
                {
                    if (TD.OperationFaultedIsEnabled())
                    {
                        TD.OperationFaulted(this.EndMethod.Name, TraceUtility.GetUtcBasedDurationForTrace(TraceUtility.ExtractAsyncOperationStartTime()));
                    }
                }
                else if (TD.OperationCompletedIsEnabled())
                {
                    TD.OperationCompleted(this.EndMethod.Name, TraceUtility.GetUtcBasedDurationForTrace(TraceUtility.ExtractAsyncOperationStartTime()));
                }
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    if (flag)
                    {
                        PerformanceCounters.MethodReturnedError(this.endMethod.Name.Substring("End".Length));
                    }
                    else if (flag2)
                    {
                        PerformanceCounters.MethodReturnedFault(this.endMethod.Name.Substring("End".Length));
                    }
                    else
                    {
                        PerformanceCounters.MethodReturnedSuccess(this.endMethod.Name.Substring("End".Length));
                    }
                }
            }
            return obj2;
        }

        public MethodInfo BeginMethod
        {
            get
            {
                return this.beginMethod;
            }
        }

        public MethodInfo EndMethod
        {
            get
            {
                return this.endMethod;
            }
        }

        private int InputParameterCount
        {
            get
            {
                this.EnsureIsInitialized();
                return this.inputParameterCount;
            }
        }

        private System.ServiceModel.Dispatcher.InvokeBeginDelegate InvokeBeginDelegate
        {
            get
            {
                this.EnsureIsInitialized();
                return this.invokeBeginDelegate;
            }
        }

        private System.ServiceModel.Dispatcher.InvokeEndDelegate InvokeEndDelegate
        {
            get
            {
                this.EnsureIsInitialized();
                return this.invokeEndDelegate;
            }
        }

        public bool IsSynchronous
        {
            get
            {
                return false;
            }
        }

        private int OutputParameterCount
        {
            get
            {
                this.EnsureIsInitialized();
                return this.outputParameterCount;
            }
        }
    }
}

