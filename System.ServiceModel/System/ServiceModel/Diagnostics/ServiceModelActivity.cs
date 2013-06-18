namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics.Application;
    using System.Threading;

    internal class ServiceModelActivity : IDisposable
    {
        private TransferActivity activity;
        private static string activityBoundaryDescription = null;
        private Guid activityId;
        private System.ServiceModel.Diagnostics.ActivityType activityType;
        private static string[] ActivityTypeNames = new string[14];
        private const int AsyncStopCount = 2;
        private bool autoResume;
        private bool autoStop;
        [ThreadStatic]
        private static ServiceModelActivity currentActivity;
        private bool disposed;
        private bool isAsync;
        private ActivityState lastState;
        private string name;
        private ServiceModelActivity previousActivity;
        private int stopCount;

        static ServiceModelActivity()
        {
            ActivityTypeNames[0] = "Unknown";
            ActivityTypeNames[1] = "Close";
            ActivityTypeNames[2] = "Construct";
            ActivityTypeNames[3] = "ExecuteUserCode";
            ActivityTypeNames[4] = "ListenAt";
            ActivityTypeNames[5] = "Open";
            ActivityTypeNames[6] = "Open";
            ActivityTypeNames[7] = "ProcessMessage";
            ActivityTypeNames[8] = "ProcessAction";
            ActivityTypeNames[9] = "ReceiveBytes";
            ActivityTypeNames[10] = "SecuritySetup";
            ActivityTypeNames[11] = "TransferToComPlus";
            ActivityTypeNames[12] = "WmiGetObject";
            ActivityTypeNames[13] = "WmiPutInstance";
        }

        private ServiceModelActivity(Guid activityId)
        {
            this.activityId = activityId;
            this.previousActivity = Current;
        }

        internal static Activity BoundOperation(ServiceModelActivity activity)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            return BoundOperation(activity, false);
        }

        internal static Activity BoundOperation(ServiceModelActivity activity, bool addTransfer)
        {
            if (activity != null)
            {
                return BoundOperationCore(activity, addTransfer);
            }
            return null;
        }

        private static Activity BoundOperationCore(ServiceModelActivity activity, bool addTransfer)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            TransferActivity activity2 = null;
            if (activity != null)
            {
                activity2 = TransferActivity.CreateActivity(activity.activityId, addTransfer);
                if (activity2 != null)
                {
                    activity2.SetPreviousServiceModelActivity(Current);
                }
                Current = activity;
            }
            return activity2;
        }

        private void CallEtwMileStoneEvent(TraceEventType type, DictionaryTraceRecord record)
        {
            TraceEventType type2 = type;
            if (type2 <= TraceEventType.Stop)
            {
                if (type2 != TraceEventType.Start)
                {
                    if (type2 == TraceEventType.Stop)
                    {
                        if (!TD.StopSignpostEventIsEnabled())
                        {
                            return;
                        }
                        TD.StopSignpostEvent(record);
                    }
                    return;
                }
            }
            else
            {
                switch (type2)
                {
                    case TraceEventType.Suspend:
                        if (TD.SuspendSignpostEventIsEnabled())
                        {
                            TD.SuspendSignpostEvent(record);
                        }
                        return;

                    case TraceEventType.Resume:
                        if (TD.ResumeSignpostEventIsEnabled())
                        {
                            TD.ResumeSignpostEvent(record);
                        }
                        return;
                }
                return;
            }
            if (TD.StartSignpostEventIsEnabled())
            {
                TD.StartSignpostEvent(record);
            }
        }

        internal static ServiceModelActivity CreateActivity()
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            return CreateActivity(Guid.NewGuid(), true);
        }

        internal static ServiceModelActivity CreateActivity(bool autoStop)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = CreateActivity(Guid.NewGuid(), true);
            if (activity != null)
            {
                activity.autoStop = autoStop;
            }
            return activity;
        }

        internal static ServiceModelActivity CreateActivity(Guid activityId)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = null;
            if (activityId != Guid.Empty)
            {
                activity = new ServiceModelActivity(activityId);
            }
            if (activity != null)
            {
                Current = activity;
            }
            return activity;
        }

        internal static ServiceModelActivity CreateActivity(Guid activityId, bool autoStop)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = CreateActivity(activityId);
            if (activity != null)
            {
                activity.autoStop = autoStop;
            }
            return activity;
        }

        internal static ServiceModelActivity CreateActivity(bool autoStop, string activityName, System.ServiceModel.Diagnostics.ActivityType activityType)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = CreateActivity(autoStop);
            Start(activity, activityName, activityType);
            return activity;
        }

        internal static ServiceModelActivity CreateAsyncActivity()
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = CreateActivity(true);
            if (activity != null)
            {
                activity.isAsync = true;
            }
            return activity;
        }

        internal static ServiceModelActivity CreateBoundedActivity()
        {
            return CreateBoundedActivity(false);
        }

        internal static ServiceModelActivity CreateBoundedActivity(bool suspendCurrent)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity current = Current;
            ServiceModelActivity activity = CreateActivity(true);
            if (activity != null)
            {
                activity.activity = (TransferActivity) BoundOperation(activity, true);
                activity.activity.SetPreviousServiceModelActivity(current);
                if (suspendCurrent)
                {
                    activity.autoResume = true;
                }
            }
            if (suspendCurrent && (current != null))
            {
                current.Suspend();
            }
            return activity;
        }

        internal static ServiceModelActivity CreateBoundedActivity(Guid activityId)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = CreateActivity(activityId, true);
            if (activity != null)
            {
                activity.activity = (TransferActivity) BoundOperation(activity, true);
            }
            return activity;
        }

        internal static ServiceModelActivity CreateBoundedActivityWithTransferInOnly(Guid activityId)
        {
            if (!DiagnosticUtility.ShouldUseActivity)
            {
                return null;
            }
            ServiceModelActivity activity = CreateActivity(activityId, true);
            if (activity != null)
            {
                if (FxTrace.Trace != null)
                {
                    FxTrace.Trace.TraceTransfer(activityId);
                }
                activity.activity = (TransferActivity) BoundOperation(activity);
            }
            return activity;
        }

        internal static ServiceModelActivity CreateLightWeightAsyncActivity(Guid activityId)
        {
            return new ServiceModelActivity(activityId);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                try
                {
                    if (this.activity != null)
                    {
                        this.activity.Dispose();
                    }
                    if (this.autoStop)
                    {
                        this.Stop();
                    }
                    if (this.autoResume && (Current != null))
                    {
                        Current.Resume();
                    }
                }
                finally
                {
                    Current = this.previousActivity;
                    GC.SuppressFinalize(this);
                }
            }
        }

        internal void Resume()
        {
            if (this.LastState == ActivityState.Suspend)
            {
                this.LastState = ActivityState.Resume;
                this.TraceMilestone(TraceEventType.Resume);
            }
        }

        internal void Resume(string activityName)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                this.name = activityName;
            }
            this.Resume();
        }

        internal static void Start(ServiceModelActivity activity, string activityName, System.ServiceModel.Diagnostics.ActivityType activityType)
        {
            if ((activity != null) && (activity.LastState == ActivityState.Unknown))
            {
                activity.LastState = ActivityState.Start;
                activity.name = activityName;
                activity.activityType = activityType;
                activity.TraceMilestone(TraceEventType.Start);
            }
        }

        internal void Stop()
        {
            int num = 0;
            if (this.isAsync)
            {
                num = Interlocked.Increment(ref this.stopCount);
            }
            if ((this.LastState != ActivityState.Stop) && (!this.isAsync || (this.isAsync && (num >= 2))))
            {
                this.LastState = ActivityState.Stop;
                this.TraceMilestone(TraceEventType.Stop);
            }
        }

        internal static void Stop(ServiceModelActivity activity)
        {
            if (activity != null)
            {
                activity.Stop();
            }
        }

        internal void Suspend()
        {
            if (this.LastState != ActivityState.Stop)
            {
                this.LastState = ActivityState.Suspend;
                this.TraceMilestone(TraceEventType.Suspend);
            }
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }

        private void TraceMilestone(TraceEventType type)
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                if (FxTrace.Trace != null)
                {
                    this.CallEtwMileStoneEvent(type, null);
                }
                if (DiagnosticUtility.DiagnosticTrace != null)
                {
                    TraceUtility.TraceEventNoCheck(type, 0x2000d, ActivityBoundaryDescription, null, ActivityBoundaryDescription, null);
                }
            }
            else
            {
                if (FxTrace.Trace != null)
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(2);
                    dictionary["ActivityName"] = this.Name;
                    dictionary["ActivityType"] = ActivityTypeNames[(int) this.activityType];
                    using ((DiagnosticUtility.ShouldUseActivity && (Guid.Empty == this.activityId)) ? null : Activity.CreateActivity(this.Id))
                    {
                        this.CallEtwMileStoneEvent(type, new DictionaryTraceRecord(dictionary));
                    }
                }
                if (DiagnosticUtility.DiagnosticTrace != null)
                {
                    Dictionary<string, string> dictionary2 = new Dictionary<string, string>(2);
                    dictionary2["ActivityName"] = this.Name;
                    dictionary2["ActivityType"] = ActivityTypeNames[(int) this.activityType];
                    TraceUtility.TraceEventNoCheck(type, 0x2000d, ActivityBoundaryDescription, new DictionaryTraceRecord(dictionary2), null, null, this.Id);
                }
            }
        }

        private static string ActivityBoundaryDescription
        {
            get
            {
                if (activityBoundaryDescription == null)
                {
                    activityBoundaryDescription = TraceSR.GetString("ActivityBoundary");
                }
                return activityBoundaryDescription;
            }
        }

        internal System.ServiceModel.Diagnostics.ActivityType ActivityType
        {
            get
            {
                return this.activityType;
            }
        }

        internal static ServiceModelActivity Current
        {
            get
            {
                return currentActivity;
            }
            private set
            {
                currentActivity = value;
            }
        }

        internal Guid Id
        {
            get
            {
                return this.activityId;
            }
        }

        private ActivityState LastState
        {
            get
            {
                return this.lastState;
            }
            set
            {
                this.lastState = value;
            }
        }

        internal string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        internal ServiceModelActivity PreviousActivity
        {
            get
            {
                return this.previousActivity;
            }
        }

        private enum ActivityState
        {
            Unknown,
            Start,
            Suspend,
            Resume,
            Stop
        }

        private class TransferActivity : Activity
        {
            private bool addTransfer;
            private bool changeCurrentServiceModelActivity;
            private ServiceModelActivity previousActivity;

            private TransferActivity(Guid activityId, Guid parentId) : base(activityId, parentId)
            {
            }

            internal static ServiceModelActivity.TransferActivity CreateActivity(Guid activityId, bool addTransfer)
            {
                if (!DiagnosticUtility.ShouldUseActivity)
                {
                    return null;
                }
                ServiceModelActivity.TransferActivity activity = null;
                if (!DiagnosticUtility.TracingEnabled || !(activityId != Guid.Empty))
                {
                    return activity;
                }
                Guid parentId = System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId;
                if (!(activityId != parentId))
                {
                    return activity;
                }
                if (addTransfer && (FxTrace.Trace != null))
                {
                    FxTrace.Trace.TraceTransfer(activityId);
                }
                return new ServiceModelActivity.TransferActivity(activityId, parentId) { addTransfer = addTransfer };
            }

            public override void Dispose()
            {
                try
                {
                    if (this.addTransfer)
                    {
                        using (Activity.CreateActivity(base.Id))
                        {
                            if (FxTrace.Trace != null)
                            {
                                FxTrace.Trace.TraceTransfer(base.parentId);
                            }
                        }
                    }
                }
                finally
                {
                    if (this.changeCurrentServiceModelActivity)
                    {
                        ServiceModelActivity.Current = this.previousActivity;
                    }
                    base.Dispose();
                }
            }

            internal void SetPreviousServiceModelActivity(ServiceModelActivity previous)
            {
                this.previousActivity = previous;
                this.changeCurrentServiceModelActivity = true;
            }
        }
    }
}

