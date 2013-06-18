namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(SetStateDesignerTheme)), ComVisible(false)]
    internal sealed class SetStateDesigner : ActivityDesigner
    {
        private string previousTargetState = string.Empty;
        private Size targetStateSize = Size.Empty;

        public override bool CanBeParentedTo(CompositeActivityDesigner parentActivityDesigner)
        {
            if (parentActivityDesigner == null)
            {
                throw new ArgumentNullException("parentActivityDesigner");
            }
            CompositeActivity parentActivity = parentActivityDesigner.Activity as CompositeActivity;
            if (parentActivity == null)
            {
                return false;
            }
            if (!ValidateParent(parentActivity))
            {
                return false;
            }
            return base.CanBeParentedTo(parentActivityDesigner);
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if (this.previousTargetState != this.TargetState)
            {
                base.PerformLayout();
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            string targetState = this.TargetState;
            if (string.IsNullOrEmpty(targetState))
            {
                targetState = "M";
            }
            Font font = e.DesignerTheme.Font;
            this.targetStateSize = StateMachineDesignerPaint.MeasureString(e.Graphics, font, targetState, StringAlignment.Near, Size.Empty);
            size.Height += this.targetStateSize.Height;
            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            string targetState = this.TargetState;
            ActivityDesignerPaint.DrawText(e.Graphics, e.DesignerTheme.Font, targetState, this.TargetStateRectangle, StringAlignment.Center, e.AmbientTheme.TextQuality, e.DesignerTheme.ForegroundBrush);
        }

        private static bool ValidateParent(CompositeActivity parentActivity)
        {
            if (parentActivity == null)
            {
                return false;
            }
            return (SetStateValidator.IsValidContainer(parentActivity) || ValidateParent(parentActivity.Parent));
        }

        protected override Rectangle ImageRectangle
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle imageRectangle = base.ImageRectangle;
                imageRectangle.Offset(0, (-this.targetStateSize.Height - margin.Height) / 2);
                return imageRectangle;
            }
        }

        private string TargetState
        {
            get
            {
                SetStateActivity activity = base.Activity as SetStateActivity;
                if (activity == null)
                {
                    return string.Empty;
                }
                string targetStateName = activity.TargetStateName;
                if (targetStateName == null)
                {
                    return string.Empty;
                }
                return targetStateName;
            }
        }

        internal Rectangle TargetStateRectangle
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle bounds = base.Bounds;
                Rectangle textRectangle = this.TextRectangle;
                Point location = new Point(bounds.Left + margin.Width, textRectangle.Bottom + (margin.Height / 2));
                return new Rectangle(location, new Size(bounds.Width - (margin.Width * 2), this.targetStateSize.Height));
            }
        }

        protected override Rectangle TextRectangle
        {
            get
            {
                Size margin = WorkflowTheme.CurrentTheme.AmbientTheme.Margin;
                Rectangle textRectangle = base.TextRectangle;
                textRectangle.Offset(0, (-this.targetStateSize.Height - margin.Height) / 2);
                return textRectangle;
            }
        }
    }
}

