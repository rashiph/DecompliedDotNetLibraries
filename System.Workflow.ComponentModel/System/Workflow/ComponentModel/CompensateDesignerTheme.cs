namespace System.Workflow.ComponentModel
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class CompensateDesignerTheme : ActivityDesignerTheme
    {
        public CompensateDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x73, 0x51, 8);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xf7, 0xf7, 0x9c);
            this.BackColorEnd = Color.FromArgb(0xff, 0xde, 170, 0);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

