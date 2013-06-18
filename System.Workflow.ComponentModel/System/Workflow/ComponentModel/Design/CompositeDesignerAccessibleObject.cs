namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Runtime;
    using System.Windows.Forms;

    public class CompositeDesignerAccessibleObject : ActivityDesignerAccessibleObject
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompositeDesignerAccessibleObject(CompositeActivityDesigner activityDesigner) : base(activityDesigner)
        {
        }

        public override AccessibleObject GetChild(int index)
        {
            CompositeActivityDesigner activityDesigner = base.ActivityDesigner as CompositeActivityDesigner;
            if ((index >= 0) && (index < activityDesigner.ContainedDesigners.Count))
            {
                return activityDesigner.ContainedDesigners[index].AccessibilityObject;
            }
            return base.GetChild(index);
        }

        public override int GetChildCount()
        {
            CompositeActivityDesigner activityDesigner = base.ActivityDesigner as CompositeActivityDesigner;
            return activityDesigner.ContainedDesigners.Count;
        }

        public override AccessibleStates State
        {
            get
            {
                AccessibleStates state = base.State;
                CompositeActivityDesigner activityDesigner = base.ActivityDesigner as CompositeActivityDesigner;
                return (state | (activityDesigner.Expanded ? AccessibleStates.Expanded : AccessibleStates.Collapsed));
            }
        }
    }
}

