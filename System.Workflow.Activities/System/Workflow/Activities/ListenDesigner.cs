namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(ListenDesignerTheme))]
    internal sealed class ListenDesigner : ParallelActivityDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is EventDrivenActivity))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override CompositeActivity OnCreateNewBranch()
        {
            return new EventDrivenActivity();
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            if ((this.Expanded && (this.ContainedDesigners.Count != 0)) && (this == base.ActiveView.AssociatedDesigner))
            {
                CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme != null)
                {
                    Rectangle bounds = base.Bounds;
                    Rectangle imageRectangle = this.ImageRectangle;
                    Rectangle empty = Rectangle.Empty;
                    empty.Width = (designerTheme.ConnectorSize.Height - (2 * e.AmbientTheme.Margin.Height)) - 1;
                    empty.Height = empty.Width;
                    empty.X = (bounds.Left + (bounds.Width / 2)) - (empty.Width / 2);
                    empty.Y = (bounds.Top + this.TitleHeight) + ((((designerTheme.ConnectorSize.Height * 3) / 2) - empty.Height) / 2);
                    e.Graphics.FillEllipse(designerTheme.ForegroundBrush, empty);
                    e.Graphics.DrawEllipse(designerTheme.ForegroundPen, empty);
                    empty.Y = (bounds.Bottom - ((designerTheme.ConnectorSize.Height * 3) / 2)) + ((((designerTheme.ConnectorSize.Height * 3) / 2) - empty.Height) / 2);
                    e.Graphics.FillEllipse(designerTheme.ForegroundBrush, empty);
                    e.Graphics.DrawEllipse(designerTheme.ForegroundPen, empty);
                }
            }
        }
    }
}

