namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(StateMachineWorkflowDesignerTheme)), ComVisible(false)]
    internal sealed class StateMachineWorkflowDesigner : StateDesigner
    {
        private string helpText;
        private static readonly Size MinSize = new Size(240, 240);
        private string text;

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.text = System.Workflow.Activities.DR.GetString("EventBasedWorkFlow");
        }

        protected override bool IsSupportedActivityType(Type activityType)
        {
            if (typeof(ListenActivity).IsAssignableFrom(activityType))
            {
                return false;
            }
            return base.IsSupportedActivityType(activityType);
        }

        internal override string HelpText
        {
            get
            {
                if (this.helpText == null)
                {
                    this.helpText = System.Workflow.Activities.DR.GetString("StateMachineWorkflowHelpText");
                }
                return this.helpText;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size minimumSize = base.MinimumSize;
                minimumSize.Width = Math.Max(minimumSize.Width, MinSize.Width);
                minimumSize.Height = Math.Max(minimumSize.Height, MinSize.Height);
                if (base.IsRootDesigner && (this.InvokingDesigner == null))
                {
                    minimumSize.Width = Math.Max(minimumSize.Width, base.ParentView.ViewPortSize.Width - (StateDesigner.Separator.Width * 2));
                    minimumSize.Height = Math.Max(minimumSize.Height, base.ParentView.ViewPortSize.Height - (StateDesigner.Separator.Height * 2));
                }
                return minimumSize;
            }
        }

        public override string Text
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.text;
            }
        }
    }
}

