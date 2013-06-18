namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.Serialization;

    [DataContract]
    internal class ActivityInstanceReference : ActivityInstanceMap.IActivityReference
    {
        [DataMember]
        private System.Activities.ActivityInstance activityInstance;

        internal ActivityInstanceReference(System.Activities.ActivityInstance activity)
        {
            this.activityInstance = activity;
        }

        void ActivityInstanceMap.IActivityReference.Load(Activity activity, ActivityInstanceMap instanceMap)
        {
            if (this.activityInstance.Activity == null)
            {
                ((ActivityInstanceMap.IActivityReference) this.activityInstance).Load(activity, instanceMap);
            }
        }

        public System.Activities.ActivityInstance ActivityInstance
        {
            get
            {
                return this.activityInstance;
            }
        }

        Activity ActivityInstanceMap.IActivityReference.Activity
        {
            get
            {
                return this.activityInstance.Activity;
            }
        }
    }
}

