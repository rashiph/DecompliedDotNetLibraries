namespace System.ServiceModel.Activities.Tracking
{
    using System;
    using System.Activities.Tracking;
    using System.Runtime;

    internal abstract class TrackingProfileManager
    {
        protected TrackingProfileManager()
        {
        }

        public virtual IAsyncResult BeginLoad(string profileName, string activityDefinitionId, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult<TrackingProfile>(this.Load(profileName, activityDefinitionId, timeout), callback, state);
        }

        public virtual TrackingProfile EndLoad(IAsyncResult result)
        {
            return CompletedAsyncResult<TrackingProfile>.End(result);
        }

        public abstract TrackingProfile Load(string profileName, string activityDefinitionId, TimeSpan timeout);
    }
}

