namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;

    public sealed class ActivityTrackPoint
    {
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private ActivityTrackingLocationCollection _exclude = new ActivityTrackingLocationCollection();
        private ExtractCollection _extracts = new ExtractCollection();
        private ActivityTrackingLocationCollection _match = new ActivityTrackingLocationCollection();

        internal bool IsMatch(Activity activity, ActivityExecutionStatus status)
        {
            bool flag = false;
            foreach (ActivityTrackingLocation location in this._match)
            {
                if (location.Match(activity, false) && location.ExecutionStatusEvents.Contains(status))
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return false;
            }
            foreach (ActivityTrackingLocation location2 in this._exclude)
            {
                if (location2.Match(activity, false) && location2.ExecutionStatusEvents.Contains(status))
                {
                    return false;
                }
            }
            return flag;
        }

        internal bool IsMatch(Activity activity, out List<ActivityExecutionStatus> status, out bool hasCondition)
        {
            hasCondition = false;
            foreach (ActivityTrackingLocation location in this._exclude)
            {
                if ((location.Conditions != null) && (location.Conditions.Count > 0))
                {
                    hasCondition = true;
                    break;
                }
            }
            foreach (ActivityTrackingLocation location2 in this._match)
            {
                if ((location2.Conditions != null) && (location2.Conditions.Count > 0))
                {
                    hasCondition = true;
                    break;
                }
            }
            status = new List<ActivityExecutionStatus>(9);
            foreach (ActivityTrackingLocation location3 in this._match)
            {
                if (location3.Match(activity, true))
                {
                    foreach (ActivityExecutionStatus status2 in location3.ExecutionStatusEvents)
                    {
                        if (!status.Contains(status2))
                        {
                            status.Add(status2);
                        }
                    }
                }
            }
            if (status.Count == 0)
            {
                return false;
            }
            if (!hasCondition)
            {
                foreach (ActivityTrackingLocation location4 in this._exclude)
                {
                    if (location4.Match(activity, true))
                    {
                        foreach (ActivityExecutionStatus status3 in location4.ExecutionStatusEvents)
                        {
                            status.Remove(status3);
                        }
                    }
                }
            }
            return (status.Count > 0);
        }

        internal void Track(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items)
        {
            foreach (TrackingExtract extract in this._extracts)
            {
                extract.GetData(activity, provider, items);
            }
        }

        public TrackingAnnotationCollection Annotations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._annotations;
            }
        }

        public ActivityTrackingLocationCollection ExcludedLocations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._exclude;
            }
        }

        public ExtractCollection Extracts
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._extracts;
            }
        }

        public ActivityTrackingLocationCollection MatchingLocations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._match;
            }
        }
    }
}

