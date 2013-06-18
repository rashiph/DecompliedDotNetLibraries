namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class WebServiceResponseDesignerTheme : ActivityDesignerTheme
    {
        public WebServiceResponseDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x94, 0xb6, 0xf7);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xff, 0xff, 0xdf);
            this.BackColorEnd = Color.FromArgb(0xff, 0xa5, 0xc3, 0xf7);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

