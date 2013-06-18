namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    public sealed class ActivityTrackingLocation
    {
        private string _activityName;
        private Type _activityType;
        private TrackingConditionCollection _conditions;
        private List<ActivityExecutionStatus> _events;
        private bool _trackDerived;

        public ActivityTrackingLocation()
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
        }

        public ActivityTrackingLocation(string activityTypeName)
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
            if (activityTypeName == null)
            {
                throw new ArgumentNullException("activityTypeName");
            }
            this._activityName = activityTypeName;
        }

        public ActivityTrackingLocation(Type activityType)
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
            if (null == activityType)
            {
                throw new ArgumentNullException("activityType");
            }
            this._activityType = activityType;
        }

        public ActivityTrackingLocation(string activityTypeName, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
            if (activityTypeName == null)
            {
                throw new ArgumentNullException("activityTypeName");
            }
            if (executionStatusEvents == null)
            {
                throw new ArgumentNullException("executionStatusEvents");
            }
            this._activityName = activityTypeName;
            this._events.AddRange(executionStatusEvents);
        }

        public ActivityTrackingLocation(Type activityType, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
            if (null == activityType)
            {
                throw new ArgumentNullException("activityType");
            }
            if (executionStatusEvents == null)
            {
                throw new ArgumentNullException("executionStatusEvents");
            }
            this._activityType = activityType;
            this._events.AddRange(executionStatusEvents);
        }

        public ActivityTrackingLocation(string activityTypeName, bool matchDerivedTypes, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
            if (activityTypeName == null)
            {
                throw new ArgumentNullException("activityTypeName");
            }
            if (executionStatusEvents == null)
            {
                throw new ArgumentNullException("executionStatusEvents");
            }
            this._activityName = activityTypeName;
            this._trackDerived = matchDerivedTypes;
            this._events.AddRange(executionStatusEvents);
        }

        public ActivityTrackingLocation(Type activityType, bool matchDerivedTypes, IEnumerable<ActivityExecutionStatus> executionStatusEvents)
        {
            this._conditions = new TrackingConditionCollection();
            this._events = new List<ActivityExecutionStatus>();
            if (null == activityType)
            {
                throw new ArgumentNullException("activityType");
            }
            if (executionStatusEvents == null)
            {
                throw new ArgumentNullException("executionStatusEvents");
            }
            this._activityType = activityType;
            this._trackDerived = matchDerivedTypes;
            this._events.AddRange(executionStatusEvents);
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

        internal bool Match(Activity activity, bool typeMatchOnly)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            if (!this.TypeIsMatch(activity))
            {
                return false;
            }
            return (typeMatchOnly || this.ConditionsAreMatch(activity));
        }

        private bool TypeIsMatch(Activity activity)
        {
            if (null != this._activityType)
            {
                return TypeMatch.IsMatch(activity, this._activityType, this._trackDerived);
            }
            return TypeMatch.IsMatch(activity, this._activityName, this._trackDerived);
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

        public TrackingConditionCollection Conditions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._conditions;
            }
        }

        public IList<ActivityExecutionStatus> ExecutionStatusEvents
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._events;
            }
        }

        public bool MatchDerivedTypes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._trackDerived;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._trackDerived = value;
            }
        }
    }
}

