namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class StateMachineWorkflowDesignerTheme : StateMachineTheme
    {
        public StateMachineWorkflowDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ShowDropShadow = true;
            this.ConnectorStartCap = LineAnchor.DiamondAnchor;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x49, 0x77, 180);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0, 0, 0, 0);
            this.BackColorEnd = Color.FromArgb(0, 0, 0, 0);
        }

        [Browsable(false)]
        public override string CompletedStateDesignerImagePath
        {
            get
            {
                return base.CompletedStateDesignerImagePath;
            }
            set
            {
                base.CompletedStateDesignerImagePath = value;
            }
        }

        [Browsable(false)]
        public override string InitialStateDesignerImagePath
        {
            get
            {
                return base.InitialStateDesignerImagePath;
            }
            set
            {
                base.InitialStateDesignerImagePath = value;
            }
        }
    }
}

