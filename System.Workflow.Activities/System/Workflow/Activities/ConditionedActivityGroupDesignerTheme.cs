namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class ConditionedActivityGroupDesignerTheme : ActivityPreviewDesignerTheme
    {
        public ConditionedActivityGroupDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x6b, 0x6d, 0x6b);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xef, 0xef, 0xef);
            this.BackColorEnd = Color.FromArgb(0xff, 0xef, 0xef, 0xef);
            base.PreviewForeColor = Color.FromArgb(0xff, 240, 240, 240);
            base.PreviewBorderColor = Color.FromArgb(0xff, 0x6b, 0x6d, 0x6b);
            base.PreviewBackColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        }
    }
}

