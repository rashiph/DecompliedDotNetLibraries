namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal sealed class FaultHandlerActivityDesignerTheme : CompositeDesignerTheme
    {
        public FaultHandlerActivityDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ShowDropShadow = false;
            this.ConnectorStartCap = LineAnchor.None;
            this.ConnectorEndCap = LineAnchor.ArrowAnchor;
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0xe0, 0xe0, 0xe0);
            this.BorderStyle = DashStyle.Dash;
            this.BackColorStart = Color.FromArgb(0, 0, 0, 0);
            this.BackColorEnd = Color.FromArgb(0, 0, 0, 0);
        }
    }
}

