namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal sealed class ThrowDesignerTheme : ActivityDesignerTheme
    {
        public ThrowDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 200, 0x2d, 0x11);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xfb, 0xd7, 0xd0);
            this.BackColorEnd = Color.FromArgb(0xff, 0xf3, 0x85, 0x72);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

