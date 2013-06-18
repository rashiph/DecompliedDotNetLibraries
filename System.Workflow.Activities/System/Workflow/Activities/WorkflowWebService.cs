namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Security.Principal;
    using System.Threading;
    using System.Web;
    using System.Web.Services;
    using System.Workflow.Runtime;
    using System.Workflow.Runtime.Hosting;

    public abstract class WorkflowWebService : WebService
    {
        internal const string ConfigSectionName = "WorkflowRuntime";
        private Type workflowType;
        private static System.Workflow.Runtime.WorkflowRuntime wRuntime;
        private static object wRuntimeSync = new object();

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowWebService(Type workflowType)
        {
            this.workflowType = workflowType;
        }

        private static Guid GetWorkflowInstanceId(ref bool isActivation)
        {
            Guid empty = Guid.Empty;
            object obj2 = HttpContext.Current.Items["__WorkflowInstanceId__"];
            if ((obj2 == null) && !isActivation)
            {
                throw new InvalidOperationException(SR.GetString("Error_NoInstanceInSession"));
            }
            if (obj2 != null)
            {
                empty = (Guid) obj2;
                object obj3 = HttpContext.Current.Items["__IsActivationContext__"];
                if (obj3 != null)
                {
                    isActivation = (bool) obj3;
                    return empty;
                }
                isActivation = false;
                return empty;
            }
            if (isActivation)
            {
                empty = Guid.NewGuid();
                HttpContext.Current.Items["__WorkflowInstanceId__"] = empty;
            }
            return empty;
        }

        protected object[] Invoke(Type interfaceType, string methodName, bool isActivation, object[] parameters)
        {
            EventHandler<WorkflowCompletedEventArgs> handler3 = null;
            object[] objArray;
            Guid workflowInstanceId = GetWorkflowInstanceId(ref isActivation);
            EventQueueName key = new EventQueueName(interfaceType, methodName);
            MethodInfo method = interfaceType.GetMethod(methodName);
            bool responseRequired = method.ReturnType != typeof(void);
            if (!responseRequired)
            {
                foreach (ParameterInfo info2 in method.GetParameters())
                {
                    if (info2.ParameterType.IsByRef || info2.IsOut)
                    {
                        responseRequired = true;
                        break;
                    }
                }
            }
            MethodMessage methodMessage = PrepareMessage(interfaceType, methodName, parameters, responseRequired);
            EventHandler<WorkflowTerminatedEventArgs> handler = null;
            EventHandler<WorkflowCompletedEventArgs> handler2 = null;
            try
            {
                WorkflowInstance workflow;
                if (isActivation)
                {
                    workflow = this.WorkflowRuntime.CreateWorkflow(this.workflowType, null, workflowInstanceId);
                    SafeEnqueueItem(workflow, key, methodMessage);
                    workflow.Start();
                }
                else
                {
                    workflow = this.WorkflowRuntime.GetWorkflow(workflowInstanceId);
                    SafeEnqueueItem(workflow, key, methodMessage);
                }
                bool workflowTerminated = false;
                handler = delegate (object sender, WorkflowTerminatedEventArgs e) {
                    if (e.WorkflowInstance.InstanceId.Equals(workflowInstanceId))
                    {
                        methodMessage.SendException(e.Exception);
                        workflowTerminated = true;
                    }
                };
                if (handler3 == null)
                {
                    handler3 = delegate (object sender, WorkflowCompletedEventArgs e) {
                        if (e.WorkflowInstance.InstanceId.Equals(workflowInstanceId))
                        {
                            methodMessage.SendException(new ApplicationException(SR.GetString(CultureInfo.CurrentCulture, "Error_WorkflowCompleted")));
                        }
                    };
                }
                handler2 = handler3;
                this.WorkflowRuntime.WorkflowTerminated += handler;
                this.WorkflowRuntime.WorkflowCompleted += handler2;
                ManualWorkflowSchedulerService service = this.WorkflowRuntime.GetService<ManualWorkflowSchedulerService>();
                if (service != null)
                {
                    service.RunWorkflow(workflow.InstanceId);
                }
                if (!responseRequired)
                {
                    return new object[0];
                }
                IMethodResponseMessage message = methodMessage.WaitForResponseMessage();
                if (message.Exception != null)
                {
                    if (!workflowTerminated)
                    {
                        throw message.Exception;
                    }
                    throw new ApplicationException(SR.GetString(CultureInfo.CurrentCulture, "Error_WorkflowTerminated"), message.Exception);
                }
                if (message.OutArgs != null)
                {
                    return ((ArrayList) message.OutArgs).ToArray();
                }
                objArray = new object[0];
            }
            finally
            {
                if (handler != null)
                {
                    this.WorkflowRuntime.WorkflowTerminated -= handler;
                }
                if (handler2 != null)
                {
                    this.WorkflowRuntime.WorkflowCompleted -= handler2;
                }
            }
            return objArray;
        }

        private static MethodMessage PrepareMessage(Type interfaceType, string operation, object[] parameters, bool responseRequired)
        {
            string name = null;
            IIdentity identity = Thread.CurrentPrincipal.Identity;
            WindowsIdentity identity2 = identity as WindowsIdentity;
            if ((identity2 != null) && (identity2.User != null))
            {
                name = identity2.User.Translate(typeof(NTAccount)).ToString();
            }
            else if (identity != null)
            {
                name = identity.Name;
            }
            return new MethodMessage(interfaceType, operation, parameters, name, responseRequired);
        }

        private static void SafeEnqueueItem(WorkflowInstance instance, EventQueueName key, MethodMessage message)
        {
        Label_0000:
            try
            {
                instance.EnqueueItem(key, message, null, null);
            }
            catch (WorkflowOwnershipException)
            {
                WorkflowActivityTrace.Activity.TraceEvent(TraceEventType.Warning, 0, string.Format(CultureInfo.InvariantCulture, "Workflow Web Host Encountered Workflow Instance Ownership conflict for instanceid {0}.", new object[] { instance.InstanceId }));
                Thread.Sleep(500);
                goto Label_0000;
            }
        }

        internal static System.Workflow.Runtime.WorkflowRuntime CurrentWorkflowRuntime
        {
            get
            {
                if (wRuntime == null)
                {
                    lock (wRuntimeSync)
                    {
                        if (wRuntime == null)
                        {
                            System.Workflow.Runtime.WorkflowRuntime runtime = new System.Workflow.Runtime.WorkflowRuntime("WorkflowRuntime");
                            try
                            {
                                runtime.StartRuntime();
                            }
                            catch
                            {
                                runtime.Dispose();
                                throw;
                            }
                            Thread.MemoryBarrier();
                            wRuntime = runtime;
                        }
                    }
                }
                return wRuntime;
            }
        }

        protected System.Workflow.Runtime.WorkflowRuntime WorkflowRuntime
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return CurrentWorkflowRuntime;
                }
                return null;
            }
        }
    }
}

