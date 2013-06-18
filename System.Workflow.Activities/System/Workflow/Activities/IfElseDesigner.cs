namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(IfElseDesignerTheme))]
    internal sealed class IfElseDesigner : ParallelActivityDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            foreach (Activity activity in activitiesToInsert)
            {
                if (!(activity is IfElseBranchActivity))
                {
                    return false;
                }
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        public override bool CanMoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if ((((this.ContainedDesigners.Count - activitiesToMove.Count) < 1) && (moveLocation != null)) && (moveLocation.AssociatedDesigner != this))
            {
                return false;
            }
            return true;
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if ((this.ContainedDesigners.Count - activitiesToRemove.Count) < 1)
            {
                return false;
            }
            return true;
        }

        private GraphicsPath GetDiamondPath(Rectangle rectangle)
        {
            Point[] points = new Point[] { new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Top), new Point(rectangle.Right - 1, rectangle.Top + (rectangle.Height / 2)), new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Bottom - 1), new Point(rectangle.Left, rectangle.Top + (rectangle.Height / 2)), new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Top) };
            GraphicsPath path = new GraphicsPath();
            path.AddLines(points);
            path.CloseFigure();
            return path;
        }

        protected override CompositeActivity OnCreateNewBranch()
        {
            return new IfElseBranchActivity();
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
                    empty.Width = (designerTheme.ConnectorSize.Height - (2 * e.AmbientTheme.Margin.Height)) + 2;
                    empty.Height = empty.Width;
                    empty.X = (bounds.Left + (bounds.Width / 2)) - (empty.Width / 2);
                    empty.Y = ((bounds.Top + this.TitleHeight) + ((((designerTheme.ConnectorSize.Height * 3) / 2) - empty.Height) / 2)) + 1;
                    using (GraphicsPath path = this.GetDiamondPath(empty))
                    {
                        e.Graphics.FillPath(designerTheme.ForegroundBrush, path);
                        e.Graphics.DrawPath(designerTheme.ForegroundPen, path);
                    }
                    empty.Y = ((bounds.Bottom - ((designerTheme.ConnectorSize.Height * 3) / 2)) + ((((designerTheme.ConnectorSize.Height * 3) / 2) - empty.Height) / 2)) + 1;
                    using (GraphicsPath path2 = this.GetDiamondPath(empty))
                    {
                        e.Graphics.FillPath(designerTheme.ForegroundBrush, path2);
                        e.Graphics.DrawPath(designerTheme.ForegroundPen, path2);
                    }
                }
            }
        }
    }
}

