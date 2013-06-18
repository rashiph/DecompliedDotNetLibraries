namespace System.Workflow.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(WhileDesignerTheme))]
    internal sealed class WhileDesigner : SequentialActivityDesigner
    {
        public override bool CanInsertActivities(HitTestInfo insertLocation, ReadOnlyCollection<Activity> activitiesToInsert)
        {
            if ((this == base.ActiveView.AssociatedDesigner) && (this.ContainedDesigners.Count > 0))
            {
                return false;
            }
            return base.CanInsertActivities(insertLocation, activitiesToInsert);
        }

        protected override Rectangle[] GetConnectors()
        {
            Rectangle[] connectors = base.GetConnectors();
            CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
            if (this.Expanded && (connectors.GetLength(0) > 0))
            {
                connectors[connectors.GetLength(0) - 1].Height -= ((designerTheme != null) ? designerTheme.ConnectorSize.Height : 0) / 3;
            }
            return connectors;
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.HelpText = System.Workflow.Activities.DR.GetString("DropActivityHere");
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if ((designerTheme != null) && this.Expanded)
            {
                size.Width += 2 * designerTheme.ConnectorSize.Width;
                size.Height += designerTheme.ConnectorSize.Height;
            }
            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.Expanded)
            {
                CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme != null)
                {
                    Rectangle bounds = base.Bounds;
                    Rectangle textRectangle = this.TextRectangle;
                    Rectangle imageRectangle = this.ImageRectangle;
                    Point empty = Point.Empty;
                    if (!imageRectangle.IsEmpty)
                    {
                        empty = new Point(imageRectangle.Right + (e.AmbientTheme.Margin.Width / 2), imageRectangle.Top + (imageRectangle.Height / 2));
                    }
                    else if (!textRectangle.IsEmpty)
                    {
                        empty = new Point(textRectangle.Right + (e.AmbientTheme.Margin.Width / 2), textRectangle.Top + (textRectangle.Height / 2));
                    }
                    else
                    {
                        empty = new Point((bounds.Left + (bounds.Width / 2)) + (e.AmbientTheme.Margin.Width / 2), bounds.Top + (e.AmbientTheme.Margin.Height / 2));
                    }
                    Point[] points = new Point[4];
                    points[0].X = bounds.Left + (bounds.Width / 2);
                    points[0].Y = bounds.Bottom - (designerTheme.ConnectorSize.Height / 3);
                    points[1].X = bounds.Right - (designerTheme.ConnectorSize.Width / 3);
                    points[1].Y = bounds.Bottom - (designerTheme.ConnectorSize.Height / 3);
                    points[2].X = bounds.Right - (designerTheme.ConnectorSize.Width / 3);
                    points[2].Y = empty.Y;
                    points[3].X = empty.X;
                    points[3].Y = empty.Y;
                    base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, points, LineAnchor.None, LineAnchor.ArrowAnchor);
                    Point[] pointArray2 = new Point[] { points[0], new Point(bounds.Left + (bounds.Width / 2), bounds.Bottom) };
                    base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, pointArray2, LineAnchor.None, LineAnchor.None);
                }
            }
        }
    }
}

