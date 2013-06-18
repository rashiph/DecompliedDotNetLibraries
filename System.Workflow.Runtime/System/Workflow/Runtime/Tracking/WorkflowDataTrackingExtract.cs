namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;

    [Serializable]
    public class WorkflowDataTrackingExtract : TrackingExtract
    {
        private TrackingAnnotationCollection _annotations;
        private string _name;

        public WorkflowDataTrackingExtract()
        {
            this._annotations = new TrackingAnnotationCollection();
        }

        public WorkflowDataTrackingExtract(string member)
        {
            this._annotations = new TrackingAnnotationCollection();
            this._name = member;
        }

        internal override void GetData(Activity activity, IServiceProvider provider, IList<TrackingDataItem> items)
        {
            Activity activity2 = ContextActivityUtils.RootContextActivity(activity);
            if ((this._name == null) || (this._name.Trim().Length == 0))
            {
                PropertyHelper.GetAllMembers(activity2, items, this._annotations);
            }
            else
            {
                TrackingDataItem item = null;
                PropertyHelper.GetProperty(this._name, activity2, this._annotations, out item);
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

