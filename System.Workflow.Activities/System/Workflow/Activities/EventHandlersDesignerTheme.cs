namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class EventHandlersDesignerTheme : ActivityPreviewDesignerTheme
    {
        public EventHandlersDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.None;
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0xe0, 0xe0, 0xe0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0x35, 0xff, 0xff, 0xb0);
            this.BackColorEnd = Color.FromArgb(0x35, 0xff, 0xff, 0xb0);
            base.PreviewForeColor = Color.FromArgb(0xff, 240, 240, 240);
            base.PreviewBorderColor = Color.FromArgb(0xff, 0x6b, 0x6d, 0x6b);
            base.PreviewBackColor = Color.FromArgb(0xff, 0xff, 0xff, 0xff);
        }
    }
}

