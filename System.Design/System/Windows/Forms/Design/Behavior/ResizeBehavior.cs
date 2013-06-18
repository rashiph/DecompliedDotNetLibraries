namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class ResizeBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private System.Windows.Forms.Design.Behavior.BehaviorService behaviorService;
        private const int borderSize = 2;
        private bool captureLost;
        private System.Windows.Forms.Cursor cursor = Cursors.Default;
        private bool didSnap;
        private bool dragging;
        private DragAssistanceManager dragManager;
        private Point initialPoint;
        private bool initialResize;
        private System.Design.NativeMethods.POINT lastMouseAbs;
        private Point lastMouseLoc;
        private Region lastResizeRegion;
        private Point lastSnapOffset;
        private const int MINSIZE = 10;
        private Size parentGridSize;
        private Point parentLocation;
        private Control primaryControl;
        private bool pushedBehavior;
        private ResizeComponent[] resizeComponents;
        private DesignerTransaction resizeTransaction;
        private IServiceProvider serviceProvider;
        private StatusCommandUI statusCommandUI;
        private SelectionRules targetResizeRules;

        internal ResizeBehavior(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.dragging = false;
            this.pushedBehavior = false;
            this.lastSnapOffset = Point.Empty;
            this.didSnap = false;
            this.statusCommandUI = new StatusCommandUI(serviceProvider);
        }

        internal static int AdjustPixelsForIntegralHeight(Control control, int pixelsMoved)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(control)["IntegralHeight"];
            if (descriptor == null)
            {
                return pixelsMoved;
            }
            object obj2 = descriptor.GetValue(control);
            if (!(obj2 is bool) || !((bool) obj2))
            {
                return pixelsMoved;
            }
            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(control)["ItemHeight"];
            if (descriptor2 == null)
            {
                return pixelsMoved;
            }
            if (pixelsMoved >= 0)
            {
                return (pixelsMoved - (pixelsMoved % ((int) descriptor2.GetValue(control))));
            }
            int num = (int) descriptor2.GetValue(control);
            return (pixelsMoved - (num - (Math.Abs(pixelsMoved) % num)));
        }

        private Rectangle AdjustToGrid(Rectangle controlBounds, SelectionRules rules)
        {
            Rectangle rectangle = controlBounds;
            if ((rules & SelectionRules.RightSizeable) != SelectionRules.None)
            {
                int num = controlBounds.Right % this.parentGridSize.Width;
                if (num > (this.parentGridSize.Width / 2))
                {
                    rectangle.Width += this.parentGridSize.Width - num;
                }
                else
                {
                    rectangle.Width -= num;
                }
            }
            else if ((rules & SelectionRules.LeftSizeable) != SelectionRules.None)
            {
                int num2 = controlBounds.Left % this.parentGridSize.Width;
                if (num2 > (this.parentGridSize.Width / 2))
                {
                    rectangle.X += this.parentGridSize.Width - num2;
                    rectangle.Width -= this.parentGridSize.Width - num2;
                }
                else
                {
                    rectangle.X -= num2;
                    rectangle.Width += num2;
                }
            }
            if ((rules & SelectionRules.BottomSizeable) != SelectionRules.None)
            {
                int num3 = controlBounds.Bottom % this.parentGridSize.Height;
                if (num3 > (this.parentGridSize.Height / 2))
                {
                    rectangle.Height += this.parentGridSize.Height - num3;
                }
                else
                {
                    rectangle.Height -= num3;
                }
            }
            else if ((rules & SelectionRules.TopSizeable) != SelectionRules.None)
            {
                int num4 = controlBounds.Top % this.parentGridSize.Height;
                if (num4 > (this.parentGridSize.Height / 2))
                {
                    rectangle.Y += this.parentGridSize.Height - num4;
                    rectangle.Height -= this.parentGridSize.Height - num4;
                }
                else
                {
                    rectangle.Y -= num4;
                    rectangle.Height += num4;
                }
            }
            rectangle.Width = Math.Max(rectangle.Width, this.parentGridSize.Width);
            rectangle.Height = Math.Max(rectangle.Height, this.parentGridSize.Height);
            return rectangle;
        }

        private SnapLine[] GenerateSnapLines(SelectionRules rules, Point loc)
        {
            ArrayList list = new ArrayList(2);
            if ((rules & SelectionRules.BottomSizeable) != SelectionRules.None)
            {
                list.Add(new SnapLine(SnapLineType.Bottom, loc.Y - 1));
                if (this.primaryControl != null)
                {
                    list.Add(new SnapLine(SnapLineType.Horizontal, loc.Y + this.primaryControl.Margin.Bottom, "Margin.Bottom", SnapLinePriority.Always));
                }
            }
            else if ((rules & SelectionRules.TopSizeable) != SelectionRules.None)
            {
                list.Add(new SnapLine(SnapLineType.Top, loc.Y));
                if (this.primaryControl != null)
                {
                    list.Add(new SnapLine(SnapLineType.Horizontal, loc.Y - this.primaryControl.Margin.Top, "Margin.Top", SnapLinePriority.Always));
                }
            }
            if ((rules & SelectionRules.RightSizeable) != SelectionRules.None)
            {
                list.Add(new SnapLine(SnapLineType.Right, loc.X - 1));
                if (this.primaryControl != null)
                {
                    list.Add(new SnapLine(SnapLineType.Vertical, loc.X + this.primaryControl.Margin.Right, "Margin.Right", SnapLinePriority.Always));
                }
            }
            else if ((rules & SelectionRules.LeftSizeable) != SelectionRules.None)
            {
                list.Add(new SnapLine(SnapLineType.Left, loc.X));
                if (this.primaryControl != null)
                {
                    list.Add(new SnapLine(SnapLineType.Vertical, loc.X - this.primaryControl.Margin.Left, "Margin.Left", SnapLinePriority.Always));
                }
            }
            SnapLine[] array = new SnapLine[list.Count];
            list.CopyTo(array);
            return array;
        }

        private void InitiateResize()
        {
            bool useSnapLines = this.BehaviorService.UseSnapLines;
            ArrayList dragComponents = new ArrayList();
            IDesignerHost service = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            for (int i = 0; i < this.resizeComponents.Length; i++)
            {
                this.resizeComponents[i].resizeBounds = ((Control) this.resizeComponents[i].resizeControl).Bounds;
                if (useSnapLines)
                {
                    dragComponents.Add(this.resizeComponents[i].resizeControl);
                }
                if (service != null)
                {
                    ControlDesigner designer = service.GetDesigner(this.resizeComponents[i].resizeControl as Component) as ControlDesigner;
                    if (designer != null)
                    {
                        this.resizeComponents[i].resizeRules = designer.SelectionRules;
                    }
                    else
                    {
                        this.resizeComponents[i].resizeRules = SelectionRules.None;
                    }
                }
            }
            this.BehaviorService.EnableAllAdorners(false);
            IDesignerHost host2 = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            if (host2 != null)
            {
                string str;
                if (this.resizeComponents.Length == 1)
                {
                    string componentName = TypeDescriptor.GetComponentName(this.resizeComponents[0].resizeControl);
                    if ((componentName == null) || (componentName.Length == 0))
                    {
                        componentName = this.resizeComponents[0].resizeControl.GetType().Name;
                    }
                    str = System.Design.SR.GetString("BehaviorServiceResizeControl", new object[] { componentName });
                }
                else
                {
                    str = System.Design.SR.GetString("BehaviorServiceResizeControls", new object[] { this.resizeComponents.Length });
                }
                this.resizeTransaction = host2.CreateTransaction(str);
            }
            this.initialResize = true;
            if (useSnapLines)
            {
                this.dragManager = new DragAssistanceManager(this.serviceProvider, dragComponents, true);
            }
            else if (this.resizeComponents.Length > 0)
            {
                Control resizeControl = this.resizeComponents[0].resizeControl as Control;
                if ((resizeControl != null) && (resizeControl.Parent != null))
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(resizeControl.Parent)["SnapToGrid"];
                    if ((descriptor != null) && ((bool) descriptor.GetValue(resizeControl.Parent)))
                    {
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(resizeControl.Parent)["GridSize"];
                        if (descriptor2 != null)
                        {
                            this.parentGridSize = (Size) descriptor2.GetValue(resizeControl.Parent);
                            this.parentLocation = this.behaviorService.ControlToAdornerWindow(resizeControl);
                            this.parentLocation.X -= resizeControl.Location.X;
                            this.parentLocation.Y -= resizeControl.Location.Y;
                        }
                    }
                }
            }
            this.captureLost = false;
        }

        public override void OnLoseCapture(Glyph g, EventArgs e)
        {
            this.captureLost = true;
            if (this.pushedBehavior)
            {
                this.pushedBehavior = false;
                if (this.BehaviorService != null)
                {
                    if (this.dragging)
                    {
                        this.dragging = false;
                        for (int i = 0; !this.captureLost && (i < this.resizeComponents.Length); i++)
                        {
                            Control resizeControl = this.resizeComponents[i].resizeControl as Control;
                            Rectangle rect = this.BehaviorService.ControlRectInAdornerWindow(resizeControl);
                            if (!rect.IsEmpty)
                            {
                                using (Graphics graphics = this.BehaviorService.AdornerWindowGraphics)
                                {
                                    graphics.SetClip(rect);
                                    using (Region region = new Region(rect))
                                    {
                                        region.Exclude(Rectangle.Inflate(rect, -2, -2));
                                        this.BehaviorService.Invalidate(region);
                                    }
                                    graphics.ResetClip();
                                }
                            }
                        }
                        this.BehaviorService.EnableAllAdorners(true);
                    }
                    this.BehaviorService.PopBehavior(this);
                    if (this.lastResizeRegion != null)
                    {
                        this.BehaviorService.Invalidate(this.lastResizeRegion);
                        this.lastResizeRegion.Dispose();
                        this.lastResizeRegion = null;
                    }
                }
            }
            if (this.resizeTransaction != null)
            {
                DesignerTransaction resizeTransaction = this.resizeTransaction;
                this.resizeTransaction = null;
                using (resizeTransaction)
                {
                    resizeTransaction.Cancel();
                }
            }
        }

        public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (button != MouseButtons.Left)
            {
                return this.pushedBehavior;
            }
            this.targetResizeRules = SelectionRules.None;
            SelectionGlyphBase base2 = g as SelectionGlyphBase;
            if (base2 != null)
            {
                this.targetResizeRules = base2.SelectionRules;
                this.cursor = base2.HitTestCursor;
            }
            if (this.targetResizeRules != SelectionRules.None)
            {
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service == null)
                {
                    return false;
                }
                this.initialPoint = mouseLoc;
                this.lastMouseLoc = mouseLoc;
                this.primaryControl = service.PrimarySelection as Control;
                ArrayList list = new ArrayList();
                foreach (object obj2 in service.GetSelectedComponents())
                {
                    if (obj2 is Control)
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(obj2)["Locked"];
                        if ((descriptor == null) || !((bool) descriptor.GetValue(obj2)))
                        {
                            list.Add(obj2);
                        }
                    }
                }
                if (list.Count == 0)
                {
                    return false;
                }
                this.resizeComponents = new ResizeComponent[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    this.resizeComponents[i].resizeControl = list[i];
                }
                this.pushedBehavior = true;
                this.BehaviorService.PushCaptureBehavior(this);
            }
            return false;
        }

        public override bool OnMouseMove(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (!this.pushedBehavior)
            {
                return false;
            }
            bool flag = Control.ModifierKeys == Keys.Alt;
            if (flag && (this.dragManager != null))
            {
                this.dragManager.EraseSnapLines();
            }
            if (flag || !mouseLoc.Equals(this.lastMouseLoc))
            {
                if (this.lastMouseAbs != null)
                {
                    System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(mouseLoc.X, mouseLoc.Y);
                    System.Design.UnsafeNativeMethods.ClientToScreen(new HandleRef(this, this.behaviorService.AdornerWindowControl.Handle), pt);
                    if ((pt.x == this.lastMouseAbs.x) && (pt.y == this.lastMouseAbs.y))
                    {
                        return true;
                    }
                }
                if (!this.dragging)
                {
                    if ((Math.Abs((int) (this.initialPoint.X - mouseLoc.X)) <= (DesignerUtils.MinDragSize.Width / 2)) && (Math.Abs((int) (this.initialPoint.Y - mouseLoc.Y)) <= (DesignerUtils.MinDragSize.Height / 2)))
                    {
                        return false;
                    }
                    this.InitiateResize();
                    this.dragging = true;
                }
                if ((this.resizeComponents == null) || (this.resizeComponents.Length == 0))
                {
                    return false;
                }
                PropertyDescriptor descriptor = null;
                PropertyDescriptor descriptor2 = null;
                PropertyDescriptor descriptor3 = null;
                PropertyDescriptor descriptor4 = null;
                if (this.initialResize)
                {
                    descriptor = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Width"];
                    descriptor2 = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Height"];
                    descriptor3 = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Top"];
                    descriptor4 = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Left"];
                    if ((descriptor != null) && !typeof(int).IsAssignableFrom(descriptor.PropertyType))
                    {
                        descriptor = null;
                    }
                    if ((descriptor2 != null) && !typeof(int).IsAssignableFrom(descriptor2.PropertyType))
                    {
                        descriptor2 = null;
                    }
                    if ((descriptor3 != null) && !typeof(int).IsAssignableFrom(descriptor3.PropertyType))
                    {
                        descriptor3 = null;
                    }
                    if ((descriptor4 != null) && !typeof(int).IsAssignableFrom(descriptor4.PropertyType))
                    {
                        descriptor4 = null;
                    }
                }
                Control resizeControl = this.resizeComponents[0].resizeControl as Control;
                this.lastMouseLoc = mouseLoc;
                this.lastMouseAbs = new System.Design.NativeMethods.POINT(mouseLoc.X, mouseLoc.Y);
                System.Design.UnsafeNativeMethods.ClientToScreen(new HandleRef(this, this.behaviorService.AdornerWindowControl.Handle), this.lastMouseAbs);
                int num = Math.Max(resizeControl.MinimumSize.Height, 10);
                int num2 = Math.Max(resizeControl.MinimumSize.Width, 10);
                if (this.dragManager != null)
                {
                    bool flag2 = true;
                    bool shouldSnapHorizontally = true;
                    if ((((this.targetResizeRules & SelectionRules.BottomSizeable) != SelectionRules.None) || ((this.targetResizeRules & SelectionRules.TopSizeable) != SelectionRules.None)) && (resizeControl.Height == num))
                    {
                        flag2 = false;
                    }
                    else if ((((this.targetResizeRules & SelectionRules.RightSizeable) != SelectionRules.None) || ((this.targetResizeRules & SelectionRules.LeftSizeable) != SelectionRules.None)) && (resizeControl.Width == num2))
                    {
                        flag2 = false;
                    }
                    PropertyDescriptor descriptor5 = TypeDescriptor.GetProperties(resizeControl)["IntegralHeight"];
                    if (descriptor5 != null)
                    {
                        object obj2 = descriptor5.GetValue(resizeControl);
                        if ((obj2 is bool) && ((bool) obj2))
                        {
                            shouldSnapHorizontally = false;
                        }
                    }
                    if (!flag && flag2)
                    {
                        this.lastSnapOffset = this.dragManager.OnMouseMove(resizeControl, this.GenerateSnapLines(this.targetResizeRules, mouseLoc), ref this.didSnap, shouldSnapHorizontally);
                    }
                    else
                    {
                        this.dragManager.OnMouseMove(new Rectangle(-100, -100, 0, 0));
                    }
                    mouseLoc.X += this.lastSnapOffset.X;
                    mouseLoc.Y += this.lastSnapOffset.Y;
                }
                Rectangle rectangle = new Rectangle(this.resizeComponents[0].resizeBounds.X, this.resizeComponents[0].resizeBounds.Y, this.resizeComponents[0].resizeBounds.Width, this.resizeComponents[0].resizeBounds.Height);
                if (this.didSnap && (resizeControl.Parent != null))
                {
                    rectangle.Location = this.behaviorService.MapAdornerWindowPoint(resizeControl.Parent.Handle, rectangle.Location);
                    if (resizeControl.Parent.IsMirrored)
                    {
                        rectangle.Offset(-rectangle.Width, 0);
                    }
                }
                Rectangle empty = Rectangle.Empty;
                Rectangle dragRect = Rectangle.Empty;
                bool flag4 = true;
                Color backColor = (resizeControl.Parent != null) ? resizeControl.Parent.BackColor : Color.Empty;
                for (int i = 0; i < this.resizeComponents.Length; i++)
                {
                    Control c = this.resizeComponents[i].resizeControl as Control;
                    Rectangle bounds = c.Bounds;
                    Rectangle rc = bounds;
                    Rectangle resizeBounds = this.resizeComponents[i].resizeBounds;
                    Rectangle rect = this.BehaviorService.ControlRectInAdornerWindow(c);
                    bool flag5 = true;
                    System.Design.UnsafeNativeMethods.SendMessage(c.Handle, 11, false, 0);
                    try
                    {
                        bool flag6 = false;
                        if ((c.Parent != null) && c.Parent.IsMirrored)
                        {
                            flag6 = true;
                        }
                        BoundsSpecified none = BoundsSpecified.None;
                        SelectionRules resizeRules = this.resizeComponents[i].resizeRules;
                        if (((this.targetResizeRules & SelectionRules.BottomSizeable) != SelectionRules.None) && ((resizeRules & SelectionRules.BottomSizeable) != SelectionRules.None))
                        {
                            int num4;
                            if (this.didSnap)
                            {
                                num4 = mouseLoc.Y - rectangle.Bottom;
                            }
                            else
                            {
                                num4 = AdjustPixelsForIntegralHeight(c, mouseLoc.Y - this.initialPoint.Y);
                            }
                            bounds.Height = Math.Max(num, resizeBounds.Height + num4);
                            none |= BoundsSpecified.Height;
                        }
                        if (((this.targetResizeRules & SelectionRules.TopSizeable) != SelectionRules.None) && ((resizeRules & SelectionRules.TopSizeable) != SelectionRules.None))
                        {
                            int num5;
                            if (this.didSnap)
                            {
                                num5 = rectangle.Y - mouseLoc.Y;
                            }
                            else
                            {
                                num5 = AdjustPixelsForIntegralHeight(c, this.initialPoint.Y - mouseLoc.Y);
                            }
                            none |= BoundsSpecified.Height;
                            bounds.Height = Math.Max(num, resizeBounds.Height + num5);
                            if ((bounds.Height != num) || ((bounds.Height == num) && (rc.Height != num)))
                            {
                                none |= BoundsSpecified.Y;
                                bounds.Y = Math.Min((int) (resizeBounds.Bottom - num), (int) (resizeBounds.Y - num5));
                            }
                        }
                        if (((((this.targetResizeRules & SelectionRules.RightSizeable) != SelectionRules.None) && ((resizeRules & SelectionRules.RightSizeable) != SelectionRules.None)) && !flag6) || ((((this.targetResizeRules & SelectionRules.LeftSizeable) != SelectionRules.None) && ((resizeRules & SelectionRules.LeftSizeable) != SelectionRules.None)) && flag6))
                        {
                            none |= BoundsSpecified.Width;
                            int x = this.initialPoint.X;
                            if (this.didSnap)
                            {
                                x = !flag6 ? rectangle.Right : rectangle.Left;
                            }
                            bounds.Width = Math.Max(num2, resizeBounds.Width + (!flag6 ? (mouseLoc.X - x) : (x - mouseLoc.X)));
                        }
                        if (((((this.targetResizeRules & SelectionRules.RightSizeable) != SelectionRules.None) && ((resizeRules & SelectionRules.RightSizeable) != SelectionRules.None)) && flag6) || ((((this.targetResizeRules & SelectionRules.LeftSizeable) != SelectionRules.None) && ((resizeRules & SelectionRules.LeftSizeable) != SelectionRules.None)) && !flag6))
                        {
                            none |= BoundsSpecified.Width;
                            int num7 = this.initialPoint.X;
                            if (this.didSnap)
                            {
                                num7 = !flag6 ? rectangle.Left : rectangle.Right;
                            }
                            int num8 = !flag6 ? (num7 - mouseLoc.X) : (mouseLoc.X - num7);
                            bounds.Width = Math.Max(num2, resizeBounds.Width + num8);
                            if ((bounds.Width != num2) || ((bounds.Width == num2) && (rc.Width != num2)))
                            {
                                none |= BoundsSpecified.X;
                                bounds.X = Math.Min((int) (resizeBounds.Right - num2), (int) (resizeBounds.X - num8));
                            }
                        }
                        if (!this.parentGridSize.IsEmpty)
                        {
                            bounds = this.AdjustToGrid(bounds, this.targetResizeRules);
                        }
                        if ((((none & BoundsSpecified.Width) == BoundsSpecified.Width) && this.dragging) && (this.initialResize && (descriptor != null)))
                        {
                            descriptor.SetValue(this.resizeComponents[i].resizeControl, bounds.Width);
                        }
                        if ((((none & BoundsSpecified.Height) == BoundsSpecified.Height) && this.dragging) && (this.initialResize && (descriptor2 != null)))
                        {
                            descriptor2.SetValue(this.resizeComponents[i].resizeControl, bounds.Height);
                        }
                        if ((((none & BoundsSpecified.X) == BoundsSpecified.X) && this.dragging) && (this.initialResize && (descriptor4 != null)))
                        {
                            descriptor4.SetValue(this.resizeComponents[i].resizeControl, bounds.X);
                        }
                        if ((((none & BoundsSpecified.Y) == BoundsSpecified.Y) && this.dragging) && (this.initialResize && (descriptor3 != null)))
                        {
                            descriptor3.SetValue(this.resizeComponents[i].resizeControl, bounds.Y);
                        }
                        if (this.dragging)
                        {
                            c.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height, none);
                            empty = this.BehaviorService.ControlRectInAdornerWindow(c);
                            if (c.Equals(resizeControl))
                            {
                                dragRect = empty;
                            }
                            if (c.Bounds == rc)
                            {
                                flag5 = false;
                            }
                            if (c.Bounds != bounds)
                            {
                                flag4 = false;
                            }
                        }
                        if ((c == this.primaryControl) && (this.statusCommandUI != null))
                        {
                            this.statusCommandUI.SetStatusInformation(c);
                        }
                    }
                    finally
                    {
                        System.Design.UnsafeNativeMethods.SendMessage(c.Handle, 11, true, 0);
                        if (flag5)
                        {
                            Control parent = c.Parent;
                            if (parent != null)
                            {
                                c.Invalidate(true);
                                parent.Invalidate(rc, true);
                                parent.Update();
                            }
                            else
                            {
                                c.Refresh();
                            }
                        }
                        if (!empty.IsEmpty)
                        {
                            using (Region region = new Region(empty))
                            {
                                region.Exclude(Rectangle.Inflate(empty, -2, -2));
                                if (flag5)
                                {
                                    using (Region region2 = new Region(rect))
                                    {
                                        region2.Exclude(Rectangle.Inflate(rect, -2, -2));
                                        this.BehaviorService.Invalidate(region2);
                                    }
                                }
                                if (!this.captureLost)
                                {
                                    using (Graphics graphics = this.BehaviorService.AdornerWindowGraphics)
                                    {
                                        if ((this.lastResizeRegion != null) && !this.lastResizeRegion.Equals(region, graphics))
                                        {
                                            this.lastResizeRegion.Exclude(region);
                                            this.BehaviorService.Invalidate(this.lastResizeRegion);
                                            this.lastResizeRegion.Dispose();
                                            this.lastResizeRegion = null;
                                        }
                                        DesignerUtils.DrawResizeBorder(graphics, region, backColor);
                                    }
                                    if (this.lastResizeRegion == null)
                                    {
                                        this.lastResizeRegion = region.Clone();
                                    }
                                }
                            }
                        }
                    }
                }
                if ((flag4 && !flag) && (this.dragManager != null))
                {
                    this.dragManager.RenderSnapLinesInternal(dragRect);
                }
                this.initialResize = false;
            }
            return true;
        }

        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            try
            {
                if (this.dragging)
                {
                    if (this.dragManager != null)
                    {
                        this.dragManager.OnMouseUp();
                        this.dragManager = null;
                        this.lastSnapOffset = Point.Empty;
                        this.didSnap = false;
                    }
                    if ((this.resizeComponents != null) && (this.resizeComponents.Length > 0))
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Width"];
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Height"];
                        PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Top"];
                        PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(this.resizeComponents[0].resizeControl)["Left"];
                        for (int i = 0; i < this.resizeComponents.Length; i++)
                        {
                            if ((descriptor != null) && (((Control) this.resizeComponents[i].resizeControl).Width != this.resizeComponents[i].resizeBounds.Width))
                            {
                                descriptor.SetValue(this.resizeComponents[i].resizeControl, ((Control) this.resizeComponents[i].resizeControl).Width);
                            }
                            if ((descriptor2 != null) && (((Control) this.resizeComponents[i].resizeControl).Height != this.resizeComponents[i].resizeBounds.Height))
                            {
                                descriptor2.SetValue(this.resizeComponents[i].resizeControl, ((Control) this.resizeComponents[i].resizeControl).Height);
                            }
                            if ((descriptor3 != null) && (((Control) this.resizeComponents[i].resizeControl).Top != this.resizeComponents[i].resizeBounds.Y))
                            {
                                descriptor3.SetValue(this.resizeComponents[i].resizeControl, ((Control) this.resizeComponents[i].resizeControl).Top);
                            }
                            if ((descriptor4 != null) && (((Control) this.resizeComponents[i].resizeControl).Left != this.resizeComponents[i].resizeBounds.X))
                            {
                                descriptor4.SetValue(this.resizeComponents[i].resizeControl, ((Control) this.resizeComponents[i].resizeControl).Left);
                            }
                            if ((this.resizeComponents[i].resizeControl == this.primaryControl) && (this.statusCommandUI != null))
                            {
                                this.statusCommandUI.SetStatusInformation(this.primaryControl);
                            }
                        }
                    }
                }
                if (this.resizeTransaction != null)
                {
                    DesignerTransaction resizeTransaction = this.resizeTransaction;
                    this.resizeTransaction = null;
                    using (resizeTransaction)
                    {
                        resizeTransaction.Commit();
                    }
                }
            }
            finally
            {
                this.OnLoseCapture(g, EventArgs.Empty);
            }
            return false;
        }

        private System.Windows.Forms.Design.Behavior.BehaviorService BehaviorService
        {
            get
            {
                if (this.behaviorService == null)
                {
                    this.behaviorService = (System.Windows.Forms.Design.Behavior.BehaviorService) this.serviceProvider.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService));
                }
                return this.behaviorService;
            }
        }

        public override System.Windows.Forms.Cursor Cursor
        {
            get
            {
                return this.cursor;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ResizeComponent
        {
            public object resizeControl;
            public Rectangle resizeBounds;
            public SelectionRules resizeRules;
        }
    }
}

