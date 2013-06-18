namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Workflow.ComponentModel;

    public class ParallelActivityDesigner : StructuredCompositeActivityDesigner
    {
        private ActivityDesignerVerbCollection designerVerbs;

        public override bool CanMoveActivities(HitTestInfo moveLocation, ReadOnlyCollection<Activity> activitiesToMove)
        {
            if (moveLocation == null)
            {
                throw new ArgumentNullException("moveLocation");
            }
            if (activitiesToMove == null)
            {
                throw new ArgumentNullException("activitiesToMove");
            }
            if ((base.ActiveDesigner == this) && ((((this.ContainedDesigners.Count - activitiesToMove.Count) < 2) && (moveLocation != null)) && (moveLocation.AssociatedDesigner != this)))
            {
                return false;
            }
            return base.CanMoveActivities(moveLocation, activitiesToMove);
        }

        public override bool CanRemoveActivities(ReadOnlyCollection<Activity> activitiesToRemove)
        {
            if (activitiesToRemove == null)
            {
                throw new ArgumentNullException("activitiesToRemove");
            }
            if ((base.ActiveDesigner == this) && ((this.ContainedDesigners.Count - activitiesToRemove.Count) < 2))
            {
                return false;
            }
            return base.CanRemoveActivities(activitiesToRemove);
        }

        private void DrawParallelConnectors(ActivityDesignerPaintEventArgs e)
        {
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme != null)
            {
                Rectangle bounds = base.Bounds;
                int num = bounds.Top + this.TitleHeight;
                ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
                ReadOnlyCollection<Point> connections = containedDesigners[0].GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                ReadOnlyCollection<Point> onlys3 = containedDesigners[containedDesigners.Count - 1].GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                Point[] points = new Point[2];
                points[0].X = bounds.Left + (bounds.Width / 2);
                points[0].Y = num;
                points[1].X = bounds.Left + (bounds.Width / 2);
                points[1].Y = num + ((designerTheme.ConnectorSize.Height * 3) / 4);
                base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, points, LineAnchor.None, LineAnchor.None);
                Point point = connections[0];
                points[0].X = point.X;
                points[0].Y = num + ((designerTheme.ConnectorSize.Height * 3) / 4);
                Point point2 = onlys3[0];
                points[1].X = point2.X;
                points[1].Y = num + ((designerTheme.ConnectorSize.Height * 3) / 4);
                base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, points, LineAnchor.None, LineAnchor.None);
                Point point3 = connections[connections.Count - 1];
                points[0].X = point3.X;
                points[0].Y = bounds.Bottom - ((designerTheme.ConnectorSize.Height * 3) / 4);
                Point point4 = onlys3[onlys3.Count - 1];
                points[1].X = point4.X;
                points[1].Y = bounds.Bottom - ((designerTheme.ConnectorSize.Height * 3) / 4);
                base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, points, LineAnchor.None, LineAnchor.None);
                points[0].X = bounds.Left + (bounds.Width / 2);
                points[0].Y = bounds.Bottom - ((designerTheme.ConnectorSize.Height * 3) / 4);
                points[1].X = bounds.Left + (bounds.Width / 2);
                points[1].Y = bounds.Bottom;
                base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, points, LineAnchor.None, LineAnchor.None);
                foreach (ActivityDesigner designer3 in containedDesigners)
                {
                    ReadOnlyCollection<Point> onlys4 = designer3.GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                    int count = onlys4.Count;
                    Point[] pointArray2 = new Point[2];
                    Point point5 = onlys4[0];
                    pointArray2[0].X = point5.X;
                    pointArray2[0].Y = num + ((designerTheme.ConnectorSize.Height * 3) / 4);
                    Point point6 = onlys4[0];
                    pointArray2[1].X = point6.X;
                    Point point7 = onlys4[0];
                    pointArray2[1].Y = point7.Y;
                    base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, pointArray2, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                    Point point8 = onlys4[count - 1];
                    pointArray2[0].X = point8.X;
                    Point point9 = onlys4[count - 1];
                    pointArray2[0].Y = point9.Y;
                    Point point10 = onlys4[count - 1];
                    pointArray2[1].X = point10.X;
                    pointArray2[1].Y = bounds.Bottom - ((designerTheme.ConnectorSize.Height * 3) / 4);
                    base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, pointArray2, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                }
            }
        }

        private void DrawParallelDropTargets(ActivityDesignerPaintEventArgs e, int index)
        {
            Rectangle[] dropTargets = this.GetDropTargets(Point.Empty);
            if ((index >= 0) && (index < dropTargets.Length))
            {
                CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme != null)
                {
                    ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
                    Rectangle rectangle = dropTargets[index];
                    Rectangle bounds = base.Bounds;
                    int num = bounds.Top + this.TitleHeight;
                    num += (containedDesigners.Count > 0) ? ((designerTheme.ConnectorSize.Height * 3) / 4) : 0;
                    int num2 = rectangle.Y - num;
                    num2 += bounds.Bottom - rectangle.Bottom;
                    num2 -= (containedDesigners.Count > 0) ? ((designerTheme.ConnectorSize.Height * 3) / 4) : 0;
                    rectangle.Y = num;
                    rectangle.Height += num2;
                    Point[] points = new Point[] { new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Top + 2), new Point(rectangle.Left + (rectangle.Width / 2), rectangle.Bottom - 2) };
                    base.DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, points, designerTheme.ConnectorStartCap, designerTheme.ConnectorEndCap);
                    if (containedDesigners.Count > 0)
                    {
                        if (index == 0)
                        {
                            ReadOnlyCollection<Point> connections = containedDesigners[0].GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                            Point[] pointArray = new Point[2];
                            pointArray[0].X = rectangle.X + (rectangle.Width / 2);
                            pointArray[0].Y = rectangle.Y;
                            Point point = connections[0];
                            pointArray[1].X = point.X;
                            pointArray[1].Y = rectangle.Y;
                            base.DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, pointArray, LineAnchor.None, LineAnchor.None);
                            pointArray[0].Y = rectangle.Bottom;
                            pointArray[1].Y = rectangle.Bottom;
                            base.DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, pointArray, LineAnchor.None, LineAnchor.None);
                        }
                        else if (index == containedDesigners.Count)
                        {
                            ReadOnlyCollection<Point> onlys3 = containedDesigners[containedDesigners.Count - 1].GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                            Point[] pointArray2 = new Point[2];
                            Point point2 = onlys3[0];
                            pointArray2[0].X = point2.X;
                            pointArray2[0].Y = rectangle.Y;
                            pointArray2[1].X = rectangle.X + (rectangle.Width / 2);
                            pointArray2[1].Y = rectangle.Y;
                            base.DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, pointArray2, LineAnchor.None, LineAnchor.None);
                            pointArray2[0].Y = rectangle.Bottom;
                            pointArray2[1].Y = rectangle.Bottom;
                            base.DrawConnectors(e.Graphics, e.AmbientTheme.DropIndicatorPen, pointArray2, LineAnchor.None, LineAnchor.None);
                        }
                    }
                }
            }
        }

        protected override Rectangle[] GetDropTargets(Point dropPoint)
        {
            if (!this.Expanded || (base.ActiveDesigner != this))
            {
                return new Rectangle[0];
            }
            CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
            Rectangle bounds = base.Bounds;
            ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
            Rectangle[] rectangleArray = new Rectangle[containedDesigners.Count + 1];
            if (containedDesigners.Count > 0)
            {
                ActivityDesigner designer = containedDesigners[0];
                rectangleArray[0].Location = new Point(bounds.X, designer.Location.Y);
                rectangleArray[0].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, designer.Size.Height);
                for (int i = 0; i < (containedDesigners.Count - 1); i++)
                {
                    ActivityDesigner designer2 = containedDesigners[i];
                    Rectangle rectangle2 = designer2.Bounds;
                    ActivityDesigner designer3 = containedDesigners[i + 1];
                    Rectangle rectangle3 = designer3.Bounds;
                    rectangleArray[i + 1].Location = new Point((rectangle2.Right + ((rectangle3.Left - rectangle2.Right) / 2)) - (((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2), rectangle2.Top);
                    rectangleArray[i + 1].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, rectangle2.Height);
                }
                ActivityDesigner designer4 = containedDesigners[containedDesigners.Count - 1];
                rectangleArray[containedDesigners.Count].Location = new Point(bounds.Right - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0), designer4.Location.Y);
                rectangleArray[containedDesigners.Count].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, designer4.Size.Height);
                return rectangleArray;
            }
            rectangleArray[0].Location = new Point(this.Location.X + ((this.Size.Width - ((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0)) / 2), this.TextRectangle.Bottom);
            rectangleArray[0].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, (this.Location.Y + this.Size.Height) - rectangleArray[0].Location.Y);
            return rectangleArray;
        }

        public override object GetNextSelectableObject(object obj, DesignerNavigationDirection direction)
        {
            if (base.ActiveDesigner != this)
            {
                return base.GetNextSelectableObject(obj, direction);
            }
            if ((direction != DesignerNavigationDirection.Left) && (direction != DesignerNavigationDirection.Right))
            {
                return null;
            }
            object activity = null;
            ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
            ActivityDesigner designer = ActivityDesigner.GetDesigner(obj as Activity);
            int num = (designer != null) ? containedDesigners.IndexOf(designer) : -1;
            if (((direction == DesignerNavigationDirection.Left) && (num >= 0)) && (num < containedDesigners.Count))
            {
                return containedDesigners[(num > 0) ? (num - 1) : (containedDesigners.Count - 1)].Activity;
            }
            if ((direction == DesignerNavigationDirection.Right) && (num <= (containedDesigners.Count - 1)))
            {
                activity = containedDesigners[(num < (containedDesigners.Count - 1)) ? (num + 1) : 0].Activity;
            }
            return activity;
        }

        private void OnAddBranch(object sender, EventArgs e)
        {
            CompositeActivity activity = this.OnCreateNewBranch();
            CompositeActivity activity2 = base.Activity as CompositeActivity;
            if ((activity2 != null) && (activity != null))
            {
                int count = this.ContainedDesigners.Count;
                CompositeActivityDesigner.InsertActivities(this, new ConnectorHitTestInfo(this, HitTestLocations.Designer, activity2.Activities.Count), new List<Activity>(new Activity[] { activity }).AsReadOnly(), DR.GetString("AddingBranch", new object[] { activity.GetType().Name }));
                if ((this.ContainedDesigners.Count > count) && (this.ContainedDesigners.Count > 0))
                {
                    this.ContainedDesigners[this.ContainedDesigners.Count - 1].EnsureVisible();
                }
            }
        }

        protected virtual CompositeActivity OnCreateNewBranch()
        {
            return null;
        }

        protected override void OnLayoutPosition(ActivityDesignerLayoutEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            base.OnLayoutPosition(e);
            if (this.Expanded && (base.ActiveDesigner == this))
            {
                CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
                if (designerTheme != null)
                {
                    ReadOnlyCollection<Point> innerConnections = this.GetInnerConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                    Point point = (innerConnections.Count > 0) ? new Point(this.Location.X, innerConnections[0].Y) : this.Location;
                    int num = 0;
                    int num2 = (designerTheme.ConnectorSize.Height * 3) / 2;
                    foreach (ActivityDesigner designer in this.ContainedDesigners)
                    {
                        num += e.AmbientTheme.SelectionSize.Width;
                        num += designerTheme.ConnectorSize.Width;
                        Size size = designer.Size;
                        designer.Location = new Point(point.X + num, point.Y + num2);
                        num += size.Width;
                        num += e.AmbientTheme.SelectionSize.Width;
                    }
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if ((this.Expanded && (base.ActiveDesigner == this)) && (designerTheme != null))
            {
                Size empty = Size.Empty;
                foreach (ActivityDesigner designer in this.ContainedDesigners)
                {
                    Size size3 = designer.Size;
                    empty.Width += e.AmbientTheme.SelectionSize.Width;
                    empty.Width += designerTheme.ConnectorSize.Width;
                    empty.Width += size3.Width;
                    empty.Width += e.AmbientTheme.SelectionSize.Width;
                    empty.Height = Math.Max(empty.Height, size3.Height);
                }
                empty.Width += (this.ContainedDesigners.Count > 0) ? designerTheme.ConnectorSize.Width : 0;
                foreach (ActivityDesigner designer2 in this.ContainedDesigners)
                {
                    designer2.Size = new Size(designer2.Size.Width, empty.Height);
                }
                empty.Height += 3 * designerTheme.ConnectorSize.Height;
                size.Width = Math.Max(size.Width, empty.Width);
                size.Height += empty.Height;
            }
            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            if (this.Expanded && (base.ActiveDesigner == this))
            {
                if (this.ContainedDesigners.Count > 0)
                {
                    this.DrawParallelConnectors(e);
                }
                if (this.CurrentDropTarget >= 0)
                {
                    this.DrawParallelDropTargets(e, this.CurrentDropTarget);
                }
            }
        }

        private void OnStatusAddBranch(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if (verb != null)
            {
                verb.Enabled = base.IsEditable;
            }
        }

        public override object FirstSelectableObject
        {
            get
            {
                if (base.ActiveDesigner != this)
                {
                    return base.FirstSelectableObject;
                }
                if (!this.Expanded || !this.IsVisible)
                {
                    return null;
                }
                object activity = null;
                if (this.ContainedDesigners.Count > 0)
                {
                    activity = this.ContainedDesigners[0].Activity;
                }
                return activity;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (base.ActiveDesigner != this)
                {
                    return base.FirstSelectableObject;
                }
                if (!this.Expanded || !this.IsVisible)
                {
                    return null;
                }
                object obj2 = (this.ContainedDesigners.Count > 0) ? this.ContainedDesigners[0].Activity : null;
                CompositeActivityDesigner designer = (obj2 is Activity) ? (ActivityDesigner.GetDesigner(obj2 as Activity) as CompositeActivityDesigner) : null;
                object lastSelectableObject = null;
                if (designer != null)
                {
                    lastSelectableObject = designer.LastSelectableObject;
                }
                if (lastSelectableObject == null)
                {
                    lastSelectableObject = obj2;
                }
                return lastSelectableObject;
            }
        }

        protected override ActivityDesignerVerbCollection Verbs
        {
            get
            {
                ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
                verbs.AddRange(base.Verbs);
                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new ActivityDesignerVerbCollection();
                    this.designerVerbs.Add(new ActivityDesignerVerb(this, DesignerVerbGroup.General, DR.GetString("AddBranch", new object[0]), new EventHandler(this.OnAddBranch), new EventHandler(this.OnStatusAddBranch)));
                }
                verbs.AddRange(this.designerVerbs);
                return verbs;
            }
        }
    }
}

