namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class SequentialWorkflowDesignerTheme : CompositeDesignerTheme
    {
        public SequentialWorkflowDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.WatermarkImagePath = "System.Workflow.Activities.ActivityDesignerResources.SequentialWorkflowDesigner";
            this.WatermarkAlignment = DesignerContentAlignment.BottomRight;
            this.ShowDropShadow = true;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x49, 0x77, 180);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0, 0, 0, 0);
            this.BackColorEnd = Color.FromArgb(0, 0, 0, 0);
        }
    }
}

