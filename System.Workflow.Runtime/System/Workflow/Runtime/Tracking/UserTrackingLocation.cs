namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    public sealed class UserTrackingLocation
    {
        private string _activityName;
        private Type _activityType;
        private string _argName;
        private Type _argType;
        private TrackingConditionCollection _conditions;
        private string _keyName;
        private bool _trackDerivedActivities;
        private bool _trackDerivedArgs;

        public UserTrackingLocation()
        {
            this._conditions = new TrackingConditionCollection();
        }

        public UserTrackingLocation(string argumentTypeName)
        {
            this._conditions = new TrackingConditionCollection();
            this._argName = argumentTypeName;
        }

        public UserTrackingLocation(Type argumentType)
        {
            this._conditions = new TrackingConditionCollection();
            this._argType = argumentType;
        }

        public UserTrackingLocation(string argumentTypeName, string activityTypeName)
        {
            this._conditions = new TrackingConditionCollection();
            this._argName = argumentTypeName;
            this._activityName = activityTypeName;
        }

        public UserTrackingLocation(string argumentTypeName, Type activityType)
        {
            this._conditions = new TrackingConditionCollection();
            this._argName = argumentTypeName;
            this._activityType = activityType;
        }

        public UserTrackingLocation(Type argumentType, string activityTypeName)
        {
            this._conditions = new TrackingConditionCollection();
            this._argType = argumentType;
            this._activityName = activityTypeName;
        }

        public UserTrackingLocation(Type argumentType, Type activityType)
        {
            this._conditions = new TrackingConditionCollection();
            this._argType = argumentType;
            this._activityType = activityType;
        }

        private bool ActTypeIsMatch(Activity activity)
        {
            if (null != this._activityType)
            {
                return TypeMatch.IsMatch(activity, this._activityType, this._trackDerivedActivities);
            }
            return TypeMatch.IsMatch(activity, this._activityName, this._trackDerivedActivities);
        }

        private bool ConditionsAreMatch(object obj)
        {
            foreach (TrackingCondition condition in this._conditions)
            {
                if (!condition.Match(obj))
                {
                    return false;
                }
            }
            return true;
        }

        internal bool Match(Activity activity)
        {
            if (!this.ActTypeIsMatch(activity))
            {
                return false;
            }
            return this.ConditionsAreMatch(activity);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal bool Match(Activity activity, string keyName, object arg)
        {
            return this.RuntimeMatch(activity, keyName, arg);
        }

        private bool RuntimeMatch(Activity activity, string keyName, object obj)
        {
            if (!this.ActTypeIsMatch(activity))
            {
                return false;
            }
            if ((this._keyName != null) && (string.Compare(this._keyName, keyName, StringComparison.Ordinal) != 0))
            {
                return false;
            }
            if (null != this._argType)
            {
                return TypeMatch.IsMatch(obj, this._argType, this._trackDerivedArgs);
            }
            return TypeMatch.IsMatch(obj, this._argName, this._trackDerivedArgs);
        }

        public Type ActivityType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._activityType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._activityType = value;
            }
        }

        public string ActivityTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._activityName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._activityName = value;
            }
        }

        public Type ArgumentType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._argType;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._argType = value;
            }
        }

        public string ArgumentTypeName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._argName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._argName = value;
            }
        }

        public TrackingConditionCollection Conditions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._conditions;
            }
        }

        public string KeyName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._keyName;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._keyName = value;
            }
        }

        public bool MatchDerivedActivityTypes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._trackDerivedActivities;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._trackDerivedActivities = value;
            }
        }

        public bool MatchDerivedArgumentTypes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._trackDerivedArgs;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._trackDerivedArgs = value;
            }
        }
    }
}

