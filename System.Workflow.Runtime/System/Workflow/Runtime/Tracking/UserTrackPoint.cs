namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public sealed class UserTrackPoint
    {
        private TrackingAnnotationCollection _annotations = new TrackingAnnotationCollection();
        private UserTrackingLocationCollection _exclude = new UserTrackingLocationCollection();
        private ExtractCollection _extracts = new ExtractCollection();
        private UserTrackingLocationCollection _match = new UserTrackingLocationCollection();

        internal bool IsMatch(Activity activity)
        {
            foreach (UserTrackingLocation location in this._match)
            {
                if (location.Match(activity))
                {
                    return true;
                }
            }
            return false;
        }

        internal bool IsMatch(Activity activity, string keyName, object argument)
        {
            foreach (UserTrackingLocation location in this._exclude)
            {
                if (location.Match(activity, keyName, argument))
                {
                    return false;
                }
            }
            foreach (UserTrackingLocation location2 in this._match)
            {
                if (location2.Match(activity, keyName, argument))
                {
                    return true;
                }
            }
            return false;
        }

        internal void Track(Activity activity, object arg, IServiceProvider provider, IList<TrackingDataItem> items)
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

        public UserTrackingLocationCollection ExcludedLocations
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

        public UserTrackingLocationCollection MatchingLocations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._match;
            }
        }
    }
}

