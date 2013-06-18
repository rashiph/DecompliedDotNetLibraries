namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    public class HitTestInfo
    {
        private ActivityDesigner activityDesigner;
        private HitTestLocations location;
        private static HitTestInfo nowhere;
        private IDictionary userData;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal HitTestInfo()
        {
        }

        public HitTestInfo(ActivityDesigner designer, HitTestLocations location)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            this.activityDesigner = designer;
            this.location = location;
        }

        public virtual int MapToIndex()
        {
            CompositeActivity activity = this.activityDesigner.Activity as CompositeActivity;
            if (activity != null)
            {
                return activity.Activities.Count;
            }
            return 0;
        }

        [Browsable(false)]
        public ActivityDesigner AssociatedDesigner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activityDesigner;
            }
        }

        [Browsable(false)]
        public virtual Rectangle Bounds
        {
            get
            {
                if (this.activityDesigner != null)
                {
                    return this.activityDesigner.Bounds;
                }
                return Rectangle.Empty;
            }
        }

        [Browsable(false)]
        public HitTestLocations HitLocation
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.location;
            }
        }

        public static HitTestInfo Nowhere
        {
            get
            {
                if (nowhere == null)
                {
                    nowhere = new HitTestInfo();
                }
                return nowhere;
            }
        }

        [Browsable(false)]
        public virtual object SelectableObject
        {
            get
            {
                if (this.activityDesigner != null)
                {
                    return this.activityDesigner.Activity;
                }
                return null;
            }
        }

        [Browsable(false)]
        public IDictionary UserData
        {
            get
            {
                if (this.userData == null)
                {
                    this.userData = new HybridDictionary();
                }
                return this.userData;
            }
        }
    }
}

