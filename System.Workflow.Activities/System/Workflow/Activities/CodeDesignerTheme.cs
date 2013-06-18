namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class CodeDesignerTheme : ActivityDesignerTheme
    {
        public CodeDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x80, 0x80, 0x80);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xf4, 0xf4, 0xf4);
            this.BackColorEnd = Color.FromArgb(0xff, 0xc0, 0xc0, 0xc0);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

