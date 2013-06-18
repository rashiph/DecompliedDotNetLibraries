namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    public class WorkflowOutlineNode : TreeNode
    {
        private System.Workflow.ComponentModel.Activity activity;

        public WorkflowOutlineNode(System.Workflow.ComponentModel.Activity activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            this.activity = activity;
            base.Name = activity.Name;
        }

        public virtual void OnActivityRename(string newName)
        {
            base.Text = newName;
        }

        public virtual void RefreshNode()
        {
            System.Workflow.ComponentModel.Activity activity = this.Activity;
            if (activity != null)
            {
                base.ForeColor = (!activity.Enabled || ActivityDesigner.IsCommentedActivity(activity)) ? WorkflowTheme.CurrentTheme.AmbientTheme.CommentIndicatorColor : SystemColors.WindowText;
                base.Text = activity.Name;
            }
        }

        public System.Workflow.ComponentModel.Activity Activity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.activity;
            }
        }
    }
}

