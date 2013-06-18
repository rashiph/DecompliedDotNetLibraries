namespace System.Workflow.Runtime
{
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Runtime.Tracking;

    internal class TrackingChannelWrapper
    {
        private System.Workflow.Runtime.Tracking.TrackingChannel _channel;
        [NonSerialized]
        private RTTrackingProfile _profile;
        private Version _profileVersionId;
        private Type _scheduleType;
        private Type _serviceType;

        private TrackingChannelWrapper()
        {
        }

        public TrackingChannelWrapper(System.Workflow.Runtime.Tracking.TrackingChannel channel, Type serviceType, Type workflowType, RTTrackingProfile profile)
        {
            this._serviceType = serviceType;
            this._scheduleType = workflowType;
            this._channel = channel;
            this._profile = profile;
            this._profileVersionId = profile.Version;
        }

        internal RTTrackingProfile GetTrackingProfile(WorkflowExecutor skedExec)
        {
            if (this._profile == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.NullProfileForChannel, new object[] { this._scheduleType.AssemblyQualifiedName }));
            }
            return this._profile;
        }

        internal void MakeProfilePrivate(WorkflowExecutor exec)
        {
            if (this._profile != null)
            {
                if (!this._profile.IsPrivate)
                {
                    this._profile = this._profile.Clone();
                    this._profile.IsPrivate = true;
                }
            }
            else
            {
                this._profile = this.GetTrackingProfile(exec).Clone();
                this._profile.IsPrivate = true;
            }
        }

        internal void SetTrackingProfile(RTTrackingProfile profile)
        {
            this._profile = profile;
        }

        internal System.Workflow.Runtime.Tracking.TrackingChannel TrackingChannel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._channel;
            }
        }

        internal Type TrackingServiceType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._serviceType;
            }
        }
    }
}

