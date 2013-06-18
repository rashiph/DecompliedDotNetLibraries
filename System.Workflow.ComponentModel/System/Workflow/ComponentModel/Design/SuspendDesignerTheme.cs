namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;

    internal sealed class SuspendDesignerTheme : ActivityDesignerTheme
    {
        public SuspendDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0xa5, 0x79, 0x73);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xff, 0xff, 0xdf);
            this.BackColorEnd = Color.FromArgb(0xff, 0xff, 0xff, 0x95);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

