namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [Serializable]
    public class ActivityDataTrackingExtract : TrackingExtract
    {
        private TrackingAnnotationCollection _annotations;
        private string _name;

        public ActivityDataTrackingExtract()
        {
            this._annotations = new TrackingAnnotationCollection();
        }

        public ActivityDataTrackingExtract(string member)
        {
            this._annotations = new TrackingAnnotationCollection();
            this._name = member;
        }

        internal override void GetData(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items)
        {
            if ((this._name == null) || (this._name.Trim().Length == 0))
            {
                PropertyHelper.GetAllMembers(activity, items, this._annotations);
            }
            else
            {
                TrackingDataItem item = null;
                PropertyHelper.GetProperty(this._name, activity, this._annotations, out item);
                if (item != null)
                {
                    items.Add(item);
                }
            }
        }

        public override TrackingAnnotationCollection Annotations
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._annotations;
            }
        }

        public override string Member
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._name = value;
            }
        }
    }
}

