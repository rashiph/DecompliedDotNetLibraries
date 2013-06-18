namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class StateDesignerTheme : StateMachineTheme
    {
        public StateDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.DiamondAnchor;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xff, 0x10, 0x10, 0x10);
            this.BorderColor = Color.FromArgb(0xff, 0x49, 0x77, 180);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xd0, 0xff, 0xff, 0xff);
            this.BackColorEnd = Color.FromArgb(0xd0, 0xff, 0xff, 0xff);
        }
    }
}

