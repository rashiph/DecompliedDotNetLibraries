namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class DragAssistanceManager
    {
        private Image backgroundImage;
        private Pen baselinePen;
        private BehaviorService behaviorService;
        private Rectangle cachedDragRect;
        private bool ctrlDrag;
        private bool disposeEdgePen;
        private bool disposeMarginPen;
        private Point dragOffset;
        private Pen edgePen;
        private Graphics graphics;
        private int[] horizontalDistances;
        private ArrayList horizontalSnapLines;
        private Line[] horzLines;
        private const int INVALID_VALUE = 0x1111;
        private Pen marginAndPaddingPen;
        private Line[] recentLines;
        private bool resizing;
        private IntPtr rootComponentHandle;
        private IServiceProvider serviceProvider;
        private const int snapDistance = 8;
        private Hashtable snapLineToBounds;
        private int snapPointX;
        private int snapPointY;
        private ArrayList targetHorizontalSnapLines;
        private ArrayList targetSnapLineTypes;
        private ArrayList targetVerticalSnapLines;
        private ArrayList tempHorzLines;
        private ArrayList tempVertLines;
        private int[] verticalDistances;
        private ArrayList verticalSnapLines;
        private Line[] vertLines;

        internal DragAssistanceManager(IServiceProvider serviceProvider) : this(serviceProvider, null, null, null, false, false)
        {
        }

        internal DragAssistanceManager(IServiceProvider serviceProvider, ArrayList dragComponents) : this(serviceProvider, null, dragComponents, null, false, false)
        {
        }

        internal DragAssistanceManager(IServiceProvider serviceProvider, ArrayList dragComponents, bool resizing) : this(serviceProvider, null, dragComponents, null, resizing, false)
        {
        }

        internal DragAssistanceManager(IServiceProvider serviceProvider, Graphics graphics, ArrayList dragComponents, Image backgroundImage, bool ctrlDrag) : this(serviceProvider, graphics, dragComponents, backgroundImage, false, ctrlDrag)
        {
        }

        internal DragAssistanceManager(IServiceProvider serviceProvider, Graphics graphics, ArrayList dragComponents, Image backgroundImage, bool resizing, bool ctrlDrag)
        {
            this.edgePen = SystemPens.Highlight;
            this.marginAndPaddingPen = SystemPens.InactiveCaption;
            this.baselinePen = new Pen(Color.Fuchsia);
            this.verticalSnapLines = new ArrayList();
            this.horizontalSnapLines = new ArrayList();
            this.targetVerticalSnapLines = new ArrayList();
            this.targetHorizontalSnapLines = new ArrayList();
            this.targetSnapLineTypes = new ArrayList();
            this.tempVertLines = new ArrayList();
            this.tempHorzLines = new ArrayList();
            this.vertLines = new Line[0];
            this.horzLines = new Line[0];
            this.snapLineToBounds = new Hashtable();
            this.serviceProvider = serviceProvider;
            this.behaviorService = serviceProvider.GetService(typeof(BehaviorService)) as BehaviorService;
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            IUIService service = serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if ((host != null) && (this.behaviorService != null))
            {
                if (graphics == null)
                {
                    this.graphics = this.behaviorService.AdornerWindowGraphics;
                }
                else
                {
                    this.graphics = graphics;
                }
                if (service != null)
                {
                    if (service.Styles["VsColorSnaplines"] is Color)
                    {
                        this.edgePen = new Pen((Color) service.Styles["VsColorSnaplines"]);
                        this.disposeEdgePen = true;
                    }
                    if (service.Styles["VsColorSnaplinesTextBaseline"] is Color)
                    {
                        this.baselinePen.Dispose();
                        this.baselinePen = new Pen((Color) service.Styles["VsColorSnaplinesTextBaseline"]);
                    }
                    if (service.Styles["VsColorSnaplinesMarginAndPadding"] is Color)
                    {
                        this.marginAndPaddingPen = new Pen((Color) service.Styles["VsColorSnaplinesMarginAndPadding"]);
                        this.disposeMarginPen = true;
                    }
                }
                this.backgroundImage = backgroundImage;
                this.rootComponentHandle = (host.RootComponent is Control) ? ((Control) host.RootComponent).Handle : IntPtr.Zero;
                this.resizing = resizing;
                this.ctrlDrag = ctrlDrag;
                this.Initialize(dragComponents, host);
            }
        }

        private bool AddChildCompSnaplines(IComponent comp, ArrayList dragComponents, Rectangle clipBounds, Control targetControl)
        {
            Control child = comp as Control;
            if (((child == null) || (((dragComponents != null) && dragComponents.Contains(comp)) && !this.ctrlDrag)) || ((IsChildOfParent(child, targetControl) || !clipBounds.IntersectsWith(child.Bounds)) || ((child.Parent == null) || !child.Visible)))
            {
                return false;
            }
            Control component = child;
            if (!component.Equals(targetControl))
            {
                IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    ControlDesigner designer = service.GetDesigner(component) as ControlDesigner;
                    if (designer != null)
                    {
                        return designer.ControlSupportsSnaplines;
                    }
                }
            }
            return true;
        }

        private bool AddControlSnaplinesWhenResizing(ControlDesigner designer, Control control, Control targetControl)
        {
            if (((this.resizing && (designer is ParentControlDesigner)) && (control.AutoSize && (targetControl != null))) && ((targetControl.Parent != null) && targetControl.Parent.Equals(control)))
            {
                return false;
            }
            return true;
        }

        private void AddSnapLines(ControlDesigner controlDesigner, ArrayList horizontalList, ArrayList verticalList, bool isTarget, bool validTarget)
        {
            IList snapLines = controlDesigner.SnapLines;
            Rectangle clientRectangle = controlDesigner.Control.ClientRectangle;
            Rectangle bounds = controlDesigner.Control.Bounds;
            bounds.Location = clientRectangle.Location = this.behaviorService.ControlToAdornerWindow(controlDesigner.Control);
            int left = bounds.Left;
            int top = bounds.Top;
            Point offsetToClientArea = controlDesigner.GetOffsetToClientArea();
            clientRectangle.X += offsetToClientArea.X;
            clientRectangle.Y += offsetToClientArea.Y;
            foreach (SnapLine line in snapLines)
            {
                if (isTarget)
                {
                    if ((line.Filter != null) && line.Filter.StartsWith("Padding"))
                    {
                        continue;
                    }
                    if (validTarget && !this.targetSnapLineTypes.Contains(line.SnapLineType))
                    {
                        this.targetSnapLineTypes.Add(line.SnapLineType);
                    }
                }
                else
                {
                    if (validTarget && !this.targetSnapLineTypes.Contains(line.SnapLineType))
                    {
                        continue;
                    }
                    if ((line.Filter != null) && line.Filter.StartsWith("Padding"))
                    {
                        this.snapLineToBounds.Add(line, clientRectangle);
                    }
                    else
                    {
                        this.snapLineToBounds.Add(line, bounds);
                    }
                }
                if (line.IsHorizontal)
                {
                    line.AdjustOffset(top);
                    horizontalList.Add(line);
                }
                else
                {
                    line.AdjustOffset(left);
                    verticalList.Add(line);
                }
            }
        }

        private int BuildDistanceArray(ArrayList snapLines, ArrayList targetSnapLines, int[] distances, Rectangle dragBounds)
        {
            int num = 0x1111;
            int num2 = 0;
            for (int i = 0; i < snapLines.Count; i++)
            {
                SnapLine snapLine = (SnapLine) snapLines[i];
                if (IsMarginOrPaddingSnapLine(snapLine) && !this.ValidateMarginOrPaddingLine(snapLine, dragBounds))
                {
                    distances[i] = 0x1111;
                }
                else
                {
                    int num4 = 0x1111;
                    for (int j = 0; j < targetSnapLines.Count; j++)
                    {
                        SnapLine line2 = (SnapLine) targetSnapLines[j];
                        if (SnapLine.ShouldSnap(snapLine, line2))
                        {
                            int num6 = line2.Offset - snapLine.Offset;
                            if (Math.Abs(num6) < Math.Abs(num4))
                            {
                                num4 = num6;
                            }
                        }
                    }
                    distances[i] = num4;
                    int priority = (int) ((SnapLine) snapLines[i]).Priority;
                    if ((Math.Abs(num4) < Math.Abs(num)) || ((Math.Abs(num4) == Math.Abs(num)) && (priority > num2)))
                    {
                        num = num4;
                        if (priority != 4)
                        {
                            num2 = priority;
                        }
                    }
                }
            }
            return num;
        }

        private static void CombineSnaplines(Line snapLine, ArrayList currentLines)
        {
            bool flag = false;
            for (int i = 0; i < currentLines.Count; i++)
            {
                Line line = (Line) currentLines[i];
                Line line2 = Line.Overlap(snapLine, line);
                if (line2 != null)
                {
                    currentLines[i] = line2;
                    flag = true;
                }
            }
            if (!flag)
            {
                currentLines.Add(snapLine);
            }
        }

        private Line[] EraseOldSnapLines(Line[] lines, ArrayList tempLines)
        {
            bool flag = false;
            Rectangle empty = Rectangle.Empty;
            if (lines != null)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    Line line = lines[i];
                    flag = false;
                    if (tempLines != null)
                    {
                        for (int j = 0; j < tempLines.Count; j++)
                        {
                            if (line.LineType == ((Line) tempLines[j]).LineType)
                            {
                                Line[] diffs = Line.GetDiffs(line, (Line) tempLines[j]);
                                if (diffs != null)
                                {
                                    for (int k = 0; k < diffs.Length; k++)
                                    {
                                        empty = new Rectangle(diffs[k].x1, diffs[k].y1, diffs[k].x2 - diffs[k].x1, diffs[k].y2 - diffs[k].y1);
                                        empty.Inflate(1, 1);
                                        if (this.backgroundImage != null)
                                        {
                                            this.graphics.DrawImage(this.backgroundImage, empty, empty, GraphicsUnit.Pixel);
                                        }
                                        else
                                        {
                                            this.behaviorService.Invalidate(empty);
                                        }
                                    }
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!flag)
                    {
                        empty = new Rectangle(line.x1, line.y1, line.x2 - line.x1, line.y2 - line.y1);
                        empty.Inflate(1, 1);
                        if (this.backgroundImage != null)
                        {
                            this.graphics.DrawImage(this.backgroundImage, empty, empty, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            this.behaviorService.Invalidate(empty);
                        }
                    }
                }
            }
            if (tempLines != null)
            {
                lines = new Line[tempLines.Count];
                tempLines.CopyTo(lines);
                return lines;
            }
            lines = new Line[0];
            return lines;
        }

        internal void EraseSnapLines()
        {
            this.EraseOldSnapLines(this.vertLines, null);
            this.EraseOldSnapLines(this.horzLines, null);
        }

        private static int FindSmallestValidDistance(ArrayList snapLines, int[] distances, int min, int max, int direction)
        {
            int distanceValue = 0;
            int index = 0;
            do
            {
                index = SmallestDistanceIndex(distances, direction, out distanceValue);
                if (index == 0x1111)
                {
                    return 0;
                }
            }
            while (!IsWithinValidRange(((SnapLine) snapLines[index]).Offset, min, max));
            distances[index] = distanceValue;
            return distanceValue;
        }

        internal Line[] GetRecentLines()
        {
            if (this.recentLines != null)
            {
                return this.recentLines;
            }
            return new Line[0];
        }

        private void IdentifyAndStoreValidLines(ArrayList snapLines, int[] distances, Rectangle dragBounds, int smallestDistance)
        {
            int num = 1;
            for (int i = 0; i < distances.Length; i++)
            {
                if (distances[i] == smallestDistance)
                {
                    int priority = (int) ((SnapLine) snapLines[i]).Priority;
                    if ((priority > num) && (priority != 4))
                    {
                        num = priority;
                    }
                }
            }
            for (int j = 0; j < distances.Length; j++)
            {
                if ((distances[j] == smallestDistance) && ((((SnapLine) snapLines[j]).Priority == num) || (((SnapLine) snapLines[j]).Priority == SnapLinePriority.Always)))
                {
                    this.StoreSnapLine((SnapLine) snapLines[j], dragBounds);
                }
            }
        }

        private void Initialize(ArrayList dragComponents, IDesignerHost host)
        {
            Control c = null;
            if ((dragComponents != null) && (dragComponents.Count > 0))
            {
                c = dragComponents[0] as Control;
            }
            Control rootComponent = host.RootComponent as Control;
            Rectangle clipBounds = new Rectangle(0, 0, rootComponent.ClientRectangle.Width, rootComponent.ClientRectangle.Height);
            clipBounds.Inflate(-1, -1);
            if (c != null)
            {
                this.dragOffset = this.behaviorService.ControlToAdornerWindow(c);
            }
            else
            {
                this.dragOffset = this.behaviorService.MapAdornerWindowPoint(rootComponent.Handle, Point.Empty);
                if ((rootComponent.Parent != null) && rootComponent.Parent.IsMirrored)
                {
                    this.dragOffset.Offset(-rootComponent.Width, 0);
                }
            }
            if (c != null)
            {
                ControlDesigner controlDesigner = host.GetDesigner(c) as ControlDesigner;
                bool flag = false;
                if (controlDesigner == null)
                {
                    controlDesigner = TypeDescriptor.CreateDesigner(c, typeof(IDesigner)) as ControlDesigner;
                    if (controlDesigner != null)
                    {
                        controlDesigner.ForceVisible = false;
                        controlDesigner.Initialize(c);
                        flag = true;
                    }
                }
                this.AddSnapLines(controlDesigner, this.targetHorizontalSnapLines, this.targetVerticalSnapLines, true, c != null);
                if (flag)
                {
                    controlDesigner.Dispose();
                }
            }
            foreach (IComponent component in host.Container.Components)
            {
                if (this.AddChildCompSnaplines(component, dragComponents, clipBounds, c))
                {
                    ControlDesigner designer = host.GetDesigner(component) as ControlDesigner;
                    if (designer != null)
                    {
                        if (this.AddControlSnaplinesWhenResizing(designer, component as Control, c))
                        {
                            this.AddSnapLines(designer, this.horizontalSnapLines, this.verticalSnapLines, false, c != null);
                        }
                        int num = designer.NumberOfInternalControlDesigners();
                        for (int i = 0; i < num; i++)
                        {
                            ControlDesigner designer3 = designer.InternalControlDesigner(i);
                            if (((designer3 != null) && this.AddChildCompSnaplines(designer3.Component, dragComponents, clipBounds, c)) && this.AddControlSnaplinesWhenResizing(designer3, designer3.Component as Control, c))
                            {
                                this.AddSnapLines(designer3, this.horizontalSnapLines, this.verticalSnapLines, false, c != null);
                            }
                        }
                    }
                }
            }
            this.verticalDistances = new int[this.verticalSnapLines.Count];
            this.horizontalDistances = new int[this.horizontalSnapLines.Count];
        }

        private static bool IsChildOfParent(Control child, Control parent)
        {
            if ((child != null) && (parent != null))
            {
                for (Control control = child.Parent; control != null; control = control.Parent)
                {
                    if (control.Equals(parent))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static bool IsMarginOrPaddingSnapLine(SnapLine snapLine)
        {
            if (snapLine.Filter == null)
            {
                return false;
            }
            if (!snapLine.Filter.StartsWith("Margin"))
            {
                return snapLine.Filter.StartsWith("Padding");
            }
            return true;
        }

        private static bool IsWithinValidRange(int offset, int min, int max)
        {
            return ((offset > min) && (offset < max));
        }

        internal Point OffsetToNearestSnapLocation(Control targetControl, Point directionOffset)
        {
            Point empty = Point.Empty;
            Rectangle dragBounds = new Rectangle(this.behaviorService.ControlToAdornerWindow(targetControl), targetControl.Size);
            if (directionOffset.X != 0)
            {
                this.BuildDistanceArray(this.verticalSnapLines, this.targetVerticalSnapLines, this.verticalDistances, dragBounds);
                int min = (directionOffset.X < 0) ? 0 : dragBounds.X;
                int max = (directionOffset.X < 0) ? dragBounds.Right : 0x7fffffff;
                empty.X = FindSmallestValidDistance(this.verticalSnapLines, this.verticalDistances, min, max, directionOffset.X);
                if (empty.X != 0)
                {
                    this.IdentifyAndStoreValidLines(this.verticalSnapLines, this.verticalDistances, dragBounds, empty.X);
                    if (directionOffset.X < 0)
                    {
                        empty.X *= -1;
                    }
                }
            }
            if (directionOffset.Y != 0)
            {
                this.BuildDistanceArray(this.horizontalSnapLines, this.targetHorizontalSnapLines, this.horizontalDistances, dragBounds);
                int num3 = (directionOffset.Y < 0) ? 0 : dragBounds.Y;
                int num4 = (directionOffset.Y < 0) ? dragBounds.Bottom : 0x7fffffff;
                empty.Y = FindSmallestValidDistance(this.horizontalSnapLines, this.horizontalDistances, num3, num4, directionOffset.Y);
                if (empty.Y != 0)
                {
                    this.IdentifyAndStoreValidLines(this.horizontalSnapLines, this.horizontalDistances, dragBounds, empty.Y);
                    if (directionOffset.Y < 0)
                    {
                        empty.Y *= -1;
                    }
                }
            }
            if (!empty.IsEmpty)
            {
                this.cachedDragRect = dragBounds;
                this.cachedDragRect.Offset(empty.X, empty.Y);
                if (empty.X != 0)
                {
                    this.vertLines = new Line[this.tempVertLines.Count];
                    this.tempVertLines.CopyTo(this.vertLines);
                }
                if (empty.Y != 0)
                {
                    this.horzLines = new Line[this.tempHorzLines.Count];
                    this.tempHorzLines.CopyTo(this.horzLines);
                }
            }
            return empty;
        }

        internal Point OffsetToNearestSnapLocation(Control targetControl, IList targetSnaplines, Point directionOffset)
        {
            this.targetHorizontalSnapLines.Clear();
            this.targetVerticalSnapLines.Clear();
            foreach (SnapLine line in targetSnaplines)
            {
                if (line.IsHorizontal)
                {
                    this.targetHorizontalSnapLines.Add(line);
                }
                else
                {
                    this.targetVerticalSnapLines.Add(line);
                }
            }
            return this.OffsetToNearestSnapLocation(targetControl, directionOffset);
        }

        internal Point OnMouseMove(Rectangle dragBounds)
        {
            bool didSnap = false;
            return this.OnMouseMove(dragBounds, true, ref didSnap, true);
        }

        internal Point OnMouseMove(Rectangle dragBounds, SnapLine[] snapLines)
        {
            bool didSnap = false;
            return this.OnMouseMove(dragBounds, snapLines, ref didSnap, true);
        }

        internal Point OnMouseMove(Rectangle dragBounds, SnapLine[] snapLines, ref bool didSnap, bool shouldSnapHorizontally)
        {
            if ((snapLines == null) || (snapLines.Length == 0))
            {
                return Point.Empty;
            }
            this.targetHorizontalSnapLines.Clear();
            this.targetVerticalSnapLines.Clear();
            foreach (SnapLine line in snapLines)
            {
                if (line.IsHorizontal)
                {
                    this.targetHorizontalSnapLines.Add(line);
                }
                else
                {
                    this.targetVerticalSnapLines.Add(line);
                }
            }
            return this.OnMouseMove(dragBounds, false, ref didSnap, shouldSnapHorizontally);
        }

        private Point OnMouseMove(Rectangle dragBounds, bool offsetSnapLines, ref bool didSnap, bool shouldSnapHorizontally)
        {
            this.tempVertLines.Clear();
            this.tempHorzLines.Clear();
            this.dragOffset = new Point(dragBounds.X - this.dragOffset.X, dragBounds.Y - this.dragOffset.Y);
            if (offsetSnapLines)
            {
                for (int i = 0; i < this.targetHorizontalSnapLines.Count; i++)
                {
                    ((SnapLine) this.targetHorizontalSnapLines[i]).AdjustOffset(this.dragOffset.Y);
                }
                for (int j = 0; j < this.targetVerticalSnapLines.Count; j++)
                {
                    ((SnapLine) this.targetVerticalSnapLines[j]).AdjustOffset(this.dragOffset.X);
                }
            }
            int num3 = this.BuildDistanceArray(this.verticalSnapLines, this.targetVerticalSnapLines, this.verticalDistances, dragBounds);
            int num4 = 0x1111;
            if (shouldSnapHorizontally)
            {
                num4 = this.BuildDistanceArray(this.horizontalSnapLines, this.targetHorizontalSnapLines, this.horizontalDistances, dragBounds);
            }
            this.snapPointX = (Math.Abs(num3) <= 8) ? -num3 : 0x1111;
            this.snapPointY = (Math.Abs(num4) <= 8) ? -num4 : 0x1111;
            didSnap = false;
            if (this.snapPointX != 0x1111)
            {
                this.IdentifyAndStoreValidLines(this.verticalSnapLines, this.verticalDistances, dragBounds, num3);
                didSnap = true;
            }
            if (this.snapPointY != 0x1111)
            {
                this.IdentifyAndStoreValidLines(this.horizontalSnapLines, this.horizontalDistances, dragBounds, num4);
                didSnap = true;
            }
            Point point = new Point((this.snapPointX != 0x1111) ? this.snapPointX : 0, (this.snapPointY != 0x1111) ? this.snapPointY : 0);
            Rectangle rectangle = new Rectangle(dragBounds.Left + point.X, dragBounds.Top + point.Y, dragBounds.Width, dragBounds.Height);
            this.vertLines = this.EraseOldSnapLines(this.vertLines, this.tempVertLines);
            this.horzLines = this.EraseOldSnapLines(this.horzLines, this.tempHorzLines);
            this.cachedDragRect = rectangle;
            this.dragOffset = dragBounds.Location;
            return point;
        }

        internal Point OnMouseMove(Control targetControl, SnapLine[] snapLines, ref bool didSnap, bool shouldSnapHorizontally)
        {
            Rectangle dragBounds = new Rectangle(this.behaviorService.ControlToAdornerWindow(targetControl), targetControl.Size);
            didSnap = false;
            return this.OnMouseMove(dragBounds, snapLines, ref didSnap, shouldSnapHorizontally);
        }

        internal void OnMouseUp()
        {
            if (this.behaviorService != null)
            {
                Line[] recentLines = this.GetRecentLines();
                string[] strArray = new string[recentLines.Length];
                for (int i = 0; i < recentLines.Length; i++)
                {
                    strArray[i] = recentLines[i].ToString();
                }
                this.behaviorService.RecentSnapLines = strArray;
            }
            this.EraseSnapLines();
            this.graphics.Dispose();
            if (this.disposeEdgePen && (this.edgePen != null))
            {
                this.edgePen.Dispose();
            }
            if (this.disposeMarginPen && (this.marginAndPaddingPen != null))
            {
                this.marginAndPaddingPen.Dispose();
            }
            if (this.baselinePen != null)
            {
                this.baselinePen.Dispose();
            }
            if (this.backgroundImage != null)
            {
                this.backgroundImage.Dispose();
            }
        }

        private void RenderSnapLines(Line[] lines, Rectangle dragRect)
        {
            Pen marginAndPaddingPen = null;
            for (int i = 0; i < lines.Length; i++)
            {
                if ((lines[i].LineType == LineType.Margin) || (lines[i].LineType == LineType.Padding))
                {
                    marginAndPaddingPen = this.marginAndPaddingPen;
                    if (lines[i].x1 == lines[i].x2)
                    {
                        int num2 = Math.Max(dragRect.Top, lines[i].OriginalBounds.Top);
                        num2 += (Math.Min(dragRect.Bottom, lines[i].OriginalBounds.Bottom) - num2) / 2;
                        lines[i].y1 = lines[i].y2 = num2;
                        if (lines[i].LineType == LineType.Margin)
                        {
                            lines[i].x1 = Math.Min(dragRect.Right, lines[i].OriginalBounds.Right);
                            lines[i].x2 = Math.Max(dragRect.Left, lines[i].OriginalBounds.Left);
                        }
                        else if (lines[i].PaddingLineType == PaddingLineType.PaddingLeft)
                        {
                            lines[i].x1 = lines[i].OriginalBounds.Left;
                            lines[i].x2 = dragRect.Left;
                        }
                        else
                        {
                            lines[i].x1 = dragRect.Right;
                            lines[i].x2 = lines[i].OriginalBounds.Right;
                        }
                        Line line1 = lines[i];
                        line1.x2--;
                    }
                    else
                    {
                        int num3 = Math.Max(dragRect.Left, lines[i].OriginalBounds.Left);
                        num3 += (Math.Min(dragRect.Right, lines[i].OriginalBounds.Right) - num3) / 2;
                        lines[i].x1 = lines[i].x2 = num3;
                        if (lines[i].LineType == LineType.Margin)
                        {
                            lines[i].y1 = Math.Min(dragRect.Bottom, lines[i].OriginalBounds.Bottom);
                            lines[i].y2 = Math.Max(dragRect.Top, lines[i].OriginalBounds.Top);
                        }
                        else if (lines[i].PaddingLineType == PaddingLineType.PaddingTop)
                        {
                            lines[i].y1 = lines[i].OriginalBounds.Top;
                            lines[i].y2 = dragRect.Top;
                        }
                        else
                        {
                            lines[i].y1 = dragRect.Bottom;
                            lines[i].y2 = lines[i].OriginalBounds.Bottom;
                        }
                        Line line2 = lines[i];
                        line2.y2--;
                    }
                }
                else if (lines[i].LineType == LineType.Baseline)
                {
                    marginAndPaddingPen = this.baselinePen;
                    Line line3 = lines[i];
                    line3.x2--;
                }
                else
                {
                    marginAndPaddingPen = this.edgePen;
                    if (lines[i].x1 == lines[i].x2)
                    {
                        Line line4 = lines[i];
                        line4.y2--;
                    }
                    else
                    {
                        Line line5 = lines[i];
                        line5.x2--;
                    }
                }
                this.graphics.DrawLine(marginAndPaddingPen, lines[i].x1, lines[i].y1, lines[i].x2, lines[i].y2);
            }
        }

        internal void RenderSnapLinesInternal()
        {
            this.RenderSnapLines(this.vertLines, this.cachedDragRect);
            this.RenderSnapLines(this.horzLines, this.cachedDragRect);
            this.recentLines = new Line[this.vertLines.Length + this.horzLines.Length];
            this.vertLines.CopyTo(this.recentLines, 0);
            this.horzLines.CopyTo(this.recentLines, this.vertLines.Length);
        }

        internal void RenderSnapLinesInternal(Rectangle dragRect)
        {
            this.cachedDragRect = dragRect;
            this.RenderSnapLinesInternal();
        }

        private static int SmallestDistanceIndex(int[] distances, int direction, out int distanceValue)
        {
            distanceValue = 0x1111;
            int index = 0x1111;
            if (distances.Length != 0)
            {
                for (int i = 0; i < distances.Length; i++)
                {
                    if (((distances[i] == 0) || ((distances[i] > 0) && (direction > 0))) || ((distances[i] < 0) && (direction < 0)))
                    {
                        distances[i] = 0x1111;
                    }
                    if (Math.Abs(distances[i]) < distanceValue)
                    {
                        distanceValue = Math.Abs(distances[i]);
                        index = i;
                    }
                }
                if (index < distances.Length)
                {
                    distances[index] = 0x1111;
                }
            }
            return index;
        }

        private void StoreSnapLine(SnapLine snapLine, Rectangle dragBounds)
        {
            Rectangle rectangle = (Rectangle) this.snapLineToBounds[snapLine];
            Line line = null;
            LineType standard = LineType.Standard;
            if (IsMarginOrPaddingSnapLine(snapLine))
            {
                standard = snapLine.Filter.StartsWith("Margin") ? LineType.Margin : LineType.Padding;
            }
            else if (snapLine.SnapLineType == SnapLineType.Baseline)
            {
                standard = LineType.Baseline;
            }
            if (snapLine.IsVertical)
            {
                line = new Line(snapLine.Offset, Math.Min(dragBounds.Top + ((this.snapPointY != 0x1111) ? this.snapPointY : 0), rectangle.Top), snapLine.Offset, Math.Max(dragBounds.Bottom + ((this.snapPointY != 0x1111) ? this.snapPointY : 0), rectangle.Bottom)) {
                    LineType = standard
                };
                CombineSnaplines(line, this.tempVertLines);
            }
            else
            {
                line = new Line(Math.Min(dragBounds.Left + ((this.snapPointX != 0x1111) ? this.snapPointX : 0), rectangle.Left), snapLine.Offset, Math.Max(dragBounds.Right + ((this.snapPointX != 0x1111) ? this.snapPointX : 0), rectangle.Right), snapLine.Offset) {
                    LineType = standard
                };
                CombineSnaplines(line, this.tempHorzLines);
            }
            if (IsMarginOrPaddingSnapLine(snapLine))
            {
                string str;
                line.OriginalBounds = rectangle;
                if ((line.LineType == LineType.Padding) && ((str = snapLine.Filter) != null))
                {
                    if (!(str == "Padding.Right"))
                    {
                        if (!(str == "Padding.Left"))
                        {
                            if (!(str == "Padding.Top"))
                            {
                                if (str == "Padding.Bottom")
                                {
                                    line.PaddingLineType = PaddingLineType.PaddingBottom;
                                }
                                return;
                            }
                            line.PaddingLineType = PaddingLineType.PaddingTop;
                            return;
                        }
                    }
                    else
                    {
                        line.PaddingLineType = PaddingLineType.PaddingRight;
                        return;
                    }
                    line.PaddingLineType = PaddingLineType.PaddingLeft;
                }
            }
        }

        private bool ValidateMarginOrPaddingLine(SnapLine snapLine, Rectangle dragBounds)
        {
            Rectangle rectangle = (Rectangle) this.snapLineToBounds[snapLine];
            if (snapLine.IsVertical)
            {
                if (rectangle.Top >= dragBounds.Top)
                {
                    if ((dragBounds.Top + dragBounds.Height) < rectangle.Top)
                    {
                        return false;
                    }
                }
                else if ((rectangle.Top + rectangle.Height) < dragBounds.Top)
                {
                    return false;
                }
            }
            else if (rectangle.Left < dragBounds.Left)
            {
                if ((rectangle.Left + rectangle.Width) < dragBounds.Left)
                {
                    return false;
                }
            }
            else if ((dragBounds.Left + dragBounds.Width) < rectangle.Left)
            {
                return false;
            }
            return true;
        }

        internal class Line
        {
            private System.Windows.Forms.Design.Behavior.DragAssistanceManager.LineType lineType;
            private Rectangle originalBounds;
            private System.Windows.Forms.Design.Behavior.DragAssistanceManager.PaddingLineType paddingLineType;
            public int x1;
            public int x2;
            public int y1;
            public int y2;

            public Line(int x1, int y1, int x2, int y2)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
                this.lineType = System.Windows.Forms.Design.Behavior.DragAssistanceManager.LineType.Standard;
            }

            private Line(int x1, int y1, int x2, int y2, System.Windows.Forms.Design.Behavior.DragAssistanceManager.LineType type)
            {
                this.x1 = x1;
                this.y1 = y1;
                this.x2 = x2;
                this.y2 = y2;
                this.lineType = type;
            }

            public static DragAssistanceManager.Line[] GetDiffs(DragAssistanceManager.Line l1, DragAssistanceManager.Line l2)
            {
                if ((l1.x1 == l1.x2) && (l1.x1 == l2.x1))
                {
                    return new DragAssistanceManager.Line[] { new DragAssistanceManager.Line(l1.x1, Math.Min(l1.y1, l2.y1), l1.x1, Math.Max(l1.y1, l2.y1)), new DragAssistanceManager.Line(l1.x1, Math.Min(l1.y2, l2.y2), l1.x1, Math.Max(l1.y2, l2.y2)) };
                }
                if ((l1.y1 == l1.y2) && (l1.y1 == l2.y1))
                {
                    return new DragAssistanceManager.Line[] { new DragAssistanceManager.Line(Math.Min(l1.x1, l2.x1), l1.y1, Math.Max(l1.x1, l2.x1), l1.y1), new DragAssistanceManager.Line(Math.Min(l1.x2, l2.x2), l1.y1, Math.Max(l1.x2, l2.x2), l1.y1) };
                }
                return null;
            }

            public static DragAssistanceManager.Line Overlap(DragAssistanceManager.Line l1, DragAssistanceManager.Line l2)
            {
                if (l1.LineType == l2.LineType)
                {
                    if ((l1.LineType != System.Windows.Forms.Design.Behavior.DragAssistanceManager.LineType.Standard) && (l1.LineType != System.Windows.Forms.Design.Behavior.DragAssistanceManager.LineType.Baseline))
                    {
                        return null;
                    }
                    if (((l1.x1 == l1.x2) && (l2.x1 == l2.x2)) && (l1.x1 == l2.x1))
                    {
                        return new DragAssistanceManager.Line(l1.x1, Math.Min(l1.y1, l2.y1), l1.x2, Math.Max(l1.y2, l2.y2), l1.LineType);
                    }
                    if (((l1.y1 == l1.y2) && (l2.y1 == l2.y2)) && (l1.y1 == l2.y2))
                    {
                        return new DragAssistanceManager.Line(Math.Min(l1.x1, l2.x1), l1.y1, Math.Max(l1.x2, l2.x2), l1.y2, l1.LineType);
                    }
                }
                return null;
            }

            public override string ToString()
            {
                return string.Concat(new object[] { "Line, type = ", this.lineType, ", dims =(", this.x1, ", ", this.y1, ")->(", this.x2, ", ", this.y2, ")" });
            }

            public System.Windows.Forms.Design.Behavior.DragAssistanceManager.LineType LineType
            {
                get
                {
                    return this.lineType;
                }
                set
                {
                    this.lineType = value;
                }
            }

            public Rectangle OriginalBounds
            {
                get
                {
                    return this.originalBounds;
                }
                set
                {
                    this.originalBounds = value;
                }
            }

            public System.Windows.Forms.Design.Behavior.DragAssistanceManager.PaddingLineType PaddingLineType
            {
                get
                {
                    return this.paddingLineType;
                }
                set
                {
                    this.paddingLineType = value;
                }
            }
        }

        internal enum LineType
        {
            Standard,
            Margin,
            Padding,
            Baseline
        }

        internal enum PaddingLineType
        {
            None,
            PaddingRight,
            PaddingLeft,
            PaddingTop,
            PaddingBottom
        }
    }
}

