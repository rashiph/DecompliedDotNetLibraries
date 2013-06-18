namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class InvokeMethodDesignerTheme : ActivityDesignerTheme
    {
        public InvokeMethodDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x73, 0x79, 0xa5);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xdf, 0xe8, 0xff);
            this.BackColorEnd = Color.FromArgb(0xff, 0x95, 0xb3, 0xff);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

