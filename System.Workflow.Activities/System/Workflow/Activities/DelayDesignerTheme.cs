namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class DelayDesignerTheme : ActivityDesignerTheme
    {
        public DelayDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x80, 0x40, 0x40);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0x80, 0x40, 0x40);
            this.BackColorEnd = Color.FromArgb(0xff, 0xf1, 0xe4, 0xe4);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

