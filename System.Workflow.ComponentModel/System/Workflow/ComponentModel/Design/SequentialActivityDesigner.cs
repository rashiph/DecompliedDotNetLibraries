namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Windows.Forms;
    using System.Workflow.ComponentModel;

    public class SequentialActivityDesigner : StructuredCompositeActivityDesigner
    {
        private SequenceDesignerAccessibleObject accessibilityObject;
        private static readonly Size DefaultHelpTextSize = new Size(100, 0x55);
        private string helpText = string.Empty;
        private Size helpTextSize = Size.Empty;

        private DesignerGlyph[] CreateConnectorDragDropGlyphs()
        {
            WorkflowView parentView = base.ParentView;
            DragDropManager service = base.GetService(typeof(DragDropManager)) as DragDropManager;
            if (((parentView == null) || (service == null)) || (!parentView.DragDropInProgress || (base.DrawingState != ActivityDesigner.DrawingStates.Valid)))
            {
                return new DesignerGlyph[0];
            }
            List<DesignerGlyph> list = new List<DesignerGlyph>();
            Rectangle rectangle = parentView.ClientRectangleToLogical(new Rectangle(Point.Empty, parentView.ViewPortSize));
            AmbientTheme ambientTheme = WorkflowTheme.CurrentTheme.AmbientTheme;
            Rectangle[] connectors = this.GetConnectors();
            Rectangle helpTextRectangle = this.HelpTextRectangle;
            for (int i = 0; i < connectors.Length; i++)
            {
                if ((rectangle.IntersectsWith(connectors[i]) && (i != this.CurrentDropTarget)) && service.IsValidDropContext(new ConnectorHitTestInfo(this, HitTestLocations.Designer, i)))
                {
                    Point empty = Point.Empty;
                    if (helpTextRectangle.IsEmpty)
                    {
                        empty = new Point((connectors[i].Location.X + (connectors[i].Size.Width / 2)) + 1, connectors[i].Location.Y + (connectors[i].Size.Height / 2));
                    }
                    else
                    {
                        empty = new Point((helpTextRectangle.Left + (helpTextRectangle.Width / 2)) + 1, helpTextRectangle.Top - (ambientTheme.DropIndicatorSize.Height / 2));
                    }
                    list.Add(new ConnectorDragDropGlyph(i, empty));
                }
            }
            return list.ToArray();
        }

        protected internal virtual Rectangle[] GetConnectors()
        {
            if (base.ActiveDesigner != this)
            {
                return new Rectangle[0];
            }
            if (!this.Expanded)
            {
                return new Rectangle[0];
            }
            CompositeDesignerTheme designerTheme = base.DesignerTheme as CompositeDesignerTheme;
            if (designerTheme == null)
            {
                return new Rectangle[0];
            }
            ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
            int num = (containedDesigners.Count > 0) ? (2 + (containedDesigners.Count - 1)) : 1;
            Rectangle[] rectangleArray = new Rectangle[num];
            ReadOnlyCollection<Point> innerConnections = this.GetInnerConnections(DesignerEdges.Bottom | DesignerEdges.Top);
            Point point = new Point();
            Point point2 = new Point();
            if ((innerConnections != null) && (innerConnections.Count > 0))
            {
                point = innerConnections[0];
                point2 = innerConnections[innerConnections.Count - 1];
            }
            if (containedDesigners.Count > 0)
            {
                ActivityDesigner designer = containedDesigners[0];
                ReadOnlyCollection<Point> connections = designer.GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                if (connections.Count == 0)
                {
                    Rectangle bounds = designer.Bounds;
                    connections = new List<Point> { new Point(bounds.Left + (bounds.Width / 2), bounds.Top), new Point(bounds.Left + (bounds.Width / 2), bounds.Bottom) }.AsReadOnly();
                }
                rectangleArray[0].Location = new Point(point.X - (((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2), point.Y);
                Point point3 = connections[0];
                rectangleArray[0].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, point3.Y - point.Y);
                for (int j = 0; j < (containedDesigners.Count - 1); j++)
                {
                    ActivityDesigner designer2 = containedDesigners[j];
                    ActivityDesigner designer3 = containedDesigners[j + 1];
                    if ((designer2 != null) && (designer3 != null))
                    {
                        ReadOnlyCollection<Point> onlys4 = designer2.GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                        int count = onlys4.Count;
                        ReadOnlyCollection<Point> onlys5 = designer3.GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                        Point point4 = onlys4[count - 1];
                        Point point5 = onlys4[count - 1];
                        rectangleArray[j + 1].Location = new Point(point4.X - (((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2), point5.Y);
                        Point point6 = onlys5[0];
                        Point point7 = onlys4[count - 1];
                        rectangleArray[j + 1].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, point6.Y - point7.Y);
                    }
                }
                ActivityDesigner designer4 = containedDesigners[containedDesigners.Count - 1];
                ReadOnlyCollection<Point> onlys6 = designer4.GetConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                if (onlys6.Count == 0)
                {
                    Rectangle rectangle2 = designer4.Bounds;
                    onlys6 = new List<Point> { new Point(rectangle2.Left + (rectangle2.Width / 2), rectangle2.Top), new Point(rectangle2.Left + (rectangle2.Width / 2), rectangle2.Bottom) }.AsReadOnly();
                }
                Point point8 = onlys6[onlys6.Count - 1];
                Point point9 = onlys6[onlys6.Count - 1];
                rectangleArray[num - 1].Location = new Point(point8.X - (((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2), point9.Y);
                Point point10 = onlys6[onlys6.Count - 1];
                rectangleArray[num - 1].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, point2.Y - point10.Y);
            }
            else
            {
                rectangleArray[0].Location = new Point(point.X - (((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0) / 2), point.Y);
                rectangleArray[0].Size = new Size((designerTheme != null) ? designerTheme.ConnectorSize.Width : 0, point2.Y - point.Y);
            }
            for (int i = 0; i < rectangleArray.Length; i++)
            {
                rectangleArray[i].Inflate(3 * rectangleArray[i].Width, 0);
            }
            return rectangleArray;
        }

        protected override Rectangle[] GetDropTargets(Point dropPoint)
        {
            if (this.HelpTextRectangle.Contains(dropPoint))
            {
                return new Rectangle[] { this.HelpTextRectangle };
            }
            return this.GetConnectors();
        }

        public override object GetNextSelectableObject(object obj, DesignerNavigationDirection direction)
        {
            if (base.ActiveDesigner != this)
            {
                return base.GetNextSelectableObject(obj, direction);
            }
            if ((direction != DesignerNavigationDirection.Down) && (direction != DesignerNavigationDirection.Up))
            {
                return null;
            }
            object activity = null;
            ReadOnlyCollection<ActivityDesigner> containedDesigners = this.ContainedDesigners;
            if (direction == DesignerNavigationDirection.Down)
            {
                if (obj is ConnectorHitTestInfo)
                {
                    int num = ((ConnectorHitTestInfo) obj).MapToIndex();
                    if ((num >= 0) && (num < containedDesigners.Count))
                    {
                        activity = containedDesigners[num].Activity;
                    }
                    return activity;
                }
                if (obj is Activity)
                {
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(obj as Activity);
                    int num2 = (designer != null) ? containedDesigners.IndexOf(designer) : -1;
                    if ((num2 >= 0) && ((num2 + 1) < this.GetConnectors().Length))
                    {
                        activity = new ConnectorHitTestInfo(this, HitTestLocations.Designer, num2 + 1);
                    }
                }
                return activity;
            }
            if (direction == DesignerNavigationDirection.Up)
            {
                if (obj is ConnectorHitTestInfo)
                {
                    int num3 = ((ConnectorHitTestInfo) obj).MapToIndex();
                    if ((num3 > 0) && (num3 < this.GetConnectors().Length))
                    {
                        activity = containedDesigners[num3 - 1].Activity;
                    }
                    return activity;
                }
                if (obj is Activity)
                {
                    ActivityDesigner designer2 = ActivityDesigner.GetDesigner(obj as Activity);
                    int connector = (designer2 != null) ? containedDesigners.IndexOf(designer2) : -1;
                    if ((connector >= 0) && (connector < this.GetConnectors().Length))
                    {
                        activity = new ConnectorHitTestInfo(this, HitTestLocations.Designer, connector);
                    }
                }
            }
            return activity;
        }

        public override System.Workflow.ComponentModel.Design.HitTestInfo HitTest(Point point)
        {
            if (base.ActiveDesigner != this)
            {
                return base.HitTest(point);
            }
            System.Workflow.ComponentModel.Design.HitTestInfo nowhere = System.Workflow.ComponentModel.Design.HitTestInfo.Nowhere;
            if (!this.Expanded)
            {
                return base.HitTest(point);
            }
            if ((this.ContainedDesigners.Count == 0) && this.HelpTextRectangle.Contains(point))
            {
                return new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0);
            }
            Rectangle[] connectors = this.GetConnectors();
            for (int i = 0; i < connectors.Length; i++)
            {
                if (connectors[i].Contains(point))
                {
                    nowhere = new ConnectorHitTestInfo(this, HitTestLocations.Designer, i);
                    break;
                }
            }
            if (nowhere.HitLocation == HitTestLocations.None)
            {
                nowhere = base.HitTest(point);
            }
            return nowhere;
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            this.HelpText = DR.GetString("DropActivitiesHere", new object[0]);
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
                int num = 0;
                ReadOnlyCollection<Point> innerConnections = this.GetInnerConnections(DesignerEdges.Bottom | DesignerEdges.Top);
                Point point = (innerConnections.Count > 0) ? innerConnections[0] : this.Location;
                if (this.ContainedDesigners.Count == 1)
                {
                    int num2 = 0;
                    if (innerConnections.Count > 0)
                    {
                        Point point2 = innerConnections[innerConnections.Count - 1];
                        Point point3 = innerConnections[0];
                        num2 = this.Size.Height - (point2.Y - point3.Y);
                    }
                    num += ((this.Size.Height - num2) / 2) - (this.ContainedDesigners[0].Size.Height / 2);
                }
                else
                {
                    num += (designerTheme != null) ? designerTheme.ConnectorSize.Height : 0;
                }
                foreach (ActivityDesigner designer in this.ContainedDesigners)
                {
                    Size size = designer.Size;
                    designer.Location = new Point(point.X - (size.Width / 2), point.Y + num);
                    num += size.Height + ((designerTheme != null) ? designerTheme.ConnectorSize.Height : 0);
                }
            }
        }

        protected override Size OnLayoutSize(ActivityDesignerLayoutEventArgs e)
        {
            Size size = base.OnLayoutSize(e);
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if ((this.Expanded && (base.ActiveDesigner == this)) && (designerTheme != null))
            {
                if (this.HelpText.Length > 0)
                {
                    this.helpTextSize = ActivityDesignerPaint.MeasureString(e.Graphics, designerTheme.Font, this.HelpText, StringAlignment.Center, DefaultHelpTextSize);
                }
                size.Height += designerTheme.ConnectorSize.Height;
                foreach (ActivityDesigner designer in this.ContainedDesigners)
                {
                    Size size2 = designer.Size;
                    size.Width = Math.Max(size.Width, size2.Width);
                    size.Height += size2.Height;
                    size.Height += designerTheme.ConnectorSize.Height;
                }
                if (this.ContainedDesigners.Count == 0)
                {
                    Rectangle helpTextRectangle = this.HelpTextRectangle;
                    size.Width = Math.Max(helpTextRectangle.Width, size.Width);
                    size.Height += helpTextRectangle.Height;
                    size.Height += designerTheme.ConnectorSize.Height;
                }
                size.Width = Math.Max(size.Width, designerTheme.Size.Width);
                size.Width += 3 * e.AmbientTheme.Margin.Width;
                size.Width += 2 * e.AmbientTheme.SelectionSize.Width;
                size.Height = Math.Max(size.Height, designerTheme.Size.Height);
            }
            return size;
        }

        protected override void OnPaint(ActivityDesignerPaintEventArgs e)
        {
            base.OnPaint(e);
            CompositeDesignerTheme designerTheme = e.DesignerTheme as CompositeDesignerTheme;
            if (this.Expanded && (designerTheme != null))
            {
                Rectangle helpTextRectangle = this.HelpTextRectangle;
                if ((this.CurrentDropTarget == -1) && !helpTextRectangle.Size.IsEmpty)
                {
                    Rectangle[] connectors = this.GetConnectors();
                    if (connectors.Length > 0)
                    {
                        Point[] points = new Point[] { new Point(connectors[0].X + (connectors[0].Width / 2), connectors[0].Y + 2), new Point(connectors[0].X + (connectors[0].Width / 2), helpTextRectangle.Top - 2) };
                        base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, points, designerTheme.ConnectorStartCap, LineAnchor.None);
                        Point[] pointArray2 = new Point[] { new Point(connectors[0].X + (connectors[0].Width / 2), helpTextRectangle.Bottom + 2), new Point(connectors[0].X + (connectors[0].Width / 2), connectors[0].Bottom - 2) };
                        base.DrawConnectors(e.Graphics, designerTheme.ForegroundPen, pointArray2, LineAnchor.None, designerTheme.ConnectorEndCap);
                    }
                    ActivityDesignerPaint.DrawText(e.Graphics, designerTheme.Font, this.HelpText, helpTextRectangle, StringAlignment.Center, e.AmbientTheme.TextQuality, designerTheme.ForegroundBrush);
                }
                else
                {
                    Rectangle[] rectangleArray2 = this.GetConnectors();
                    for (int i = 0; i < rectangleArray2.Length; i++)
                    {
                        Pen pen = (i == this.CurrentDropTarget) ? e.AmbientTheme.DropIndicatorPen : designerTheme.ForegroundPen;
                        LineAnchor startCap = (((i == 0) && (rectangleArray2.Length > 2)) || (i == (rectangleArray2.Length - 1))) ? LineAnchor.None : designerTheme.ConnectorStartCap;
                        LineAnchor endCap = ((i == 0) || ((i == (rectangleArray2.Length - 1)) && (rectangleArray2.Length > 2))) ? LineAnchor.None : designerTheme.ConnectorEndCap;
                        Point[] pointArray3 = new Point[] { new Point(rectangleArray2[i].Left + (rectangleArray2[i].Width / 2), rectangleArray2[i].Top + 2), new Point(rectangleArray2[i].Left + (rectangleArray2[i].Width / 2), rectangleArray2[i].Bottom - 2) };
                        base.DrawConnectors(e.Graphics, pen, pointArray3, startCap, endCap);
                    }
                }
            }
        }

        public override AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObject == null)
                {
                    this.accessibilityObject = new SequenceDesignerAccessibleObject(this);
                }
                return this.accessibilityObject;
            }
        }

        public override bool CanExpandCollapse
        {
            get
            {
                if (base.ParentDesigner is ParallelActivityDesigner)
                {
                    return false;
                }
                return base.CanExpandCollapse;
            }
        }

        public override bool Expanded
        {
            get
            {
                return ((base.ParentDesigner is ParallelActivityDesigner) || base.Expanded);
            }
            set
            {
                if (base.ParentDesigner is ParallelActivityDesigner)
                {
                    value = true;
                }
                base.Expanded = value;
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
                if (((this.GetConnectors().Length != 0) && this.Expanded) && this.IsVisible)
                {
                    return new ConnectorHitTestInfo(this, HitTestLocations.Designer, 0);
                }
                return null;
            }
        }

        protected internal override ActivityDesignerGlyphCollection Glyphs
        {
            get
            {
                ActivityDesignerGlyphCollection glyphs = new ActivityDesignerGlyphCollection();
                ISelectionService service = base.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    foreach (object obj2 in service.GetSelectedComponents())
                    {
                        ConnectorHitTestInfo info = obj2 as ConnectorHitTestInfo;
                        if ((info != null) && (info.AssociatedDesigner == this))
                        {
                            glyphs.Add(new SequentialConnectorSelectionGlyph(info.MapToIndex(), service.PrimarySelection == obj2));
                        }
                    }
                }
                glyphs.AddRange(this.CreateConnectorDragDropGlyphs());
                glyphs.AddRange(base.Glyphs);
                return glyphs;
            }
        }

        protected virtual string HelpText
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helpText;
            }
            set
            {
                this.helpText = value;
                base.PerformLayout();
            }
        }

        protected virtual Rectangle HelpTextRectangle
        {
            get
            {
                Rectangle[] connectors = this.GetConnectors();
                if (((this.HelpText.Length == 0) || (this.ContainedDesigners.Count > 0)) || (!this.Expanded || (connectors.Length == 0)))
                {
                    return Rectangle.Empty;
                }
                Rectangle empty = Rectangle.Empty;
                empty.X = (connectors[0].Left + (connectors[0].Width / 2)) - (this.helpTextSize.Width / 2);
                empty.Y = (connectors[0].Top + (connectors[0].Height / 2)) - (this.helpTextSize.Height / 2);
                empty.Size = this.helpTextSize;
                return empty;
            }
        }

        protected Size HelpTextSize
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.helpTextSize;
            }
        }

        public override object LastSelectableObject
        {
            get
            {
                if (base.ActiveDesigner != this)
                {
                    return base.LastSelectableObject;
                }
                Rectangle[] connectors = this.GetConnectors();
                if (((connectors.Length != 0) && this.Expanded) && this.IsVisible)
                {
                    return new ConnectorHitTestInfo(this, HitTestLocations.Designer, connectors.Length - 1);
                }
                return null;
            }
        }

        private sealed class SequentialConnectorSelectionGlyph : ConnectorSelectionGlyph
        {
            public SequentialConnectorSelectionGlyph(int connectorIndex, bool isPrimarySelectionGlyph) : base(connectorIndex, isPrimarySelectionGlyph)
            {
            }

            public override Rectangle GetBounds(ActivityDesigner designer, bool activated)
            {
                Rectangle empty = Rectangle.Empty;
                if (designer is SequentialActivityDesigner)
                {
                    Rectangle[] connectors = ((SequentialActivityDesigner) designer).GetConnectors();
                    if (base.connectorIndex < connectors.Length)
                    {
                        empty = connectors[base.connectorIndex];
                    }
                }
                return empty;
            }

            protected override void OnPaint(Graphics graphics, bool activated, AmbientTheme ambientTheme, ActivityDesigner designer)
            {
                Rectangle bounds = this.GetBounds(designer, activated);
                Rectangle[] grabHandles = new Rectangle[] { new Rectangle((bounds.X + (bounds.Width / 2)) - (ambientTheme.SelectionSize.Width / 2), bounds.Y, ambientTheme.SelectionSize.Width, ambientTheme.SelectionSize.Height), new Rectangle((bounds.X + (bounds.Width / 2)) - (ambientTheme.SelectionSize.Width / 2), bounds.Bottom - ambientTheme.SelectionSize.Height, ambientTheme.SelectionSize.Width, ambientTheme.SelectionSize.Height) };
                ActivityDesignerPaint.DrawGrabHandles(graphics, grabHandles, base.isPrimarySelectionGlyph);
            }

            public override bool IsPrimarySelection
            {
                get
                {
                    return base.isPrimarySelectionGlyph;
                }
            }
        }
    }
}

