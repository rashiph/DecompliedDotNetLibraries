namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel.Design;

    internal sealed class EventSinkDesignerTheme : ActivityDesignerTheme
    {
        public EventSinkDesignerTheme(WorkflowTheme theme) : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xff, 0, 0, 0);
            this.BorderColor = Color.FromArgb(0xff, 0x9c, 0xae, 0x73);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xff, 0xf5, 0xfb, 0xe1);
            this.BackColorEnd = Color.FromArgb(0xff, 0xd6, 0xeb, 0x84);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}

