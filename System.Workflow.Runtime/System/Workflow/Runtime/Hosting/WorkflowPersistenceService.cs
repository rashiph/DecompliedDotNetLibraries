namespace System.Workflow.Runtime.Hosting
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    public abstract class WorkflowPersistenceService : WorkflowRuntimeService
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected WorkflowPersistenceService()
        {
        }

        protected static byte[] GetDefaultSerializedForm(Activity activity)
        {
            byte[] buffer;
            DateTime now = DateTime.Now;
            using (MemoryStream stream = new MemoryStream(0x2800))
            {
                stream.Position = 0L;
                activity.Save(stream);
                using (MemoryStream stream2 = new MemoryStream((int) stream.Length))
                {
                    using (GZipStream stream3 = new GZipStream(stream2, CompressionMode.Compress, true))
                    {
                        stream3.Write(stream.GetBuffer(), 0, (int) stream.Length);
                    }
                    ActivityExecutionContextInfo info = (ActivityExecutionContextInfo) activity.GetValue(Activity.ActivityExecutionContextInfoProperty);
                    TimeSpan span = (TimeSpan) (DateTime.Now - now);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Serialized a {0} with id {1} to length {2}. Took {3}.", new object[] { info, info.ContextGuid, stream2.Length, span });
                    buffer = stream2.GetBuffer();
                    Array.Resize<byte>(ref buffer, Convert.ToInt32(stream2.Length));
                }
            }
            return buffer;
        }

        protected internal static bool GetIsBlocked(Activity rootActivity)
        {
            return (bool) rootActivity.GetValue(WorkflowExecutor.IsBlockedProperty);
        }

        protected internal static string GetSuspendOrTerminateInfo(Activity rootActivity)
        {
            return (string) rootActivity.GetValue(WorkflowExecutor.SuspendOrTerminateInfoProperty);
        }

        protected internal static WorkflowStatus GetWorkflowStatus(Activity rootActivity)
        {
            return (WorkflowStatus) rootActivity.GetValue(WorkflowExecutor.WorkflowStatusProperty);
        }

        protected internal abstract Activity LoadCompletedContextActivity(Guid scopeId, Activity outerActivity);
        protected internal abstract Activity LoadWorkflowInstanceState(Guid instanceId);
        protected static Activity RestoreFromDefaultSerializedForm(byte[] activityBytes, Activity outerActivity)
        {
            Activity activity;
            DateTime now = DateTime.Now;
            MemoryStream stream = new MemoryStream(activityBytes) {
                Position = 0L
            };
            using (GZipStream stream2 = new GZipStream(stream, CompressionMode.Decompress, true))
            {
                activity = Activity.Load(stream2, outerActivity);
            }
            TimeSpan span = (TimeSpan) (DateTime.Now - now);
            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "Deserialized a {0} to length {1}. Took {2}.", new object[] { activity, stream.Length, span });
            return activity;
        }

        protected internal abstract void SaveCompletedContextActivity(Activity activity);
        protected internal abstract void SaveWorkflowInstanceState(Activity rootActivity, bool unlock);
        protected internal abstract bool UnloadOnIdle(Activity activity);
        protected internal abstract void UnlockWorkflowInstanceState(Activity rootActivity);
    }
}

