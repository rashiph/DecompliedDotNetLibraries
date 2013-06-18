namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    public class ParentControlDesigner : ControlDesigner, IOleDragClient
    {
        private Point adornerWindowToScreenOffset;
        private bool checkSnapLineSetting = true;
        private IComponentChangeService componentChangeSvc;
        private bool defaultUseSnapLines;
        private DragAssistanceManager dragManager;
        private bool drawGrid = true;
        private EscapeHandler escapeHandler;
        private bool getDefaultDrawGrid = true;
        private bool getDefaultGridSize = true;
        private bool getDefaultGridSnap = true;
        private Graphics graphics;
        private Size gridSize = Size.Empty;
        private bool gridSnap = true;
        private const int maxGridSize = 200;
        private const int minGridSize = 2;
        private Point mouseDragBase = ControlDesigner.InvalidPoint;
        private FrameStyle mouseDragFrame;
        private Rectangle mouseDragOffset = Rectangle.Empty;
        private ToolboxItem mouseDragTool;
        private OleDragDropHandler oleDragDropHandler;
        private bool parentCanSetDrawGrid = true;
        private bool parentCanSetGridSize = true;
        private bool parentCanSetGridSnap = true;
        private Control pendingRemoveControl;
        private StatusCommandUI statusCommandUI;
        private static BooleanSwitch StepControls = new BooleanSwitch("StepControls", "ParentControlDesigner: step added controls");
        private int suspendChanging;
        private ToolboxItemSnapLineBehavior toolboxItemSnapLineBehavior;
        private IToolboxService toolboxService;
        private ToolboxSnapDragDropEventArgs toolboxSnapDragDropEventArgs;

        private void AddChildComponents(IComponent component, IContainer container, IDesignerHost host)
        {
            Control control = this.GetControl(component);
            if (control != null)
            {
                Control control2 = control;
                Control[] array = new Control[control2.Controls.Count];
                control2.Controls.CopyTo(array, 0);
                IContainer container2 = null;
                for (int i = 0; i < array.Length; i++)
                {
                    ISite site = array[i].Site;
                    if (site != null)
                    {
                        string name = site.Name;
                        if (container.Components[name] != null)
                        {
                            name = null;
                        }
                        container2 = site.Container;
                        if (container2 != null)
                        {
                            container2.Remove(array[i]);
                        }
                        if (name != null)
                        {
                            container.Add(array[i], name);
                        }
                        else
                        {
                            container.Add(array[i]);
                        }
                        if (array[i].Parent != control2)
                        {
                            control2.Controls.Add(array[i]);
                        }
                        else
                        {
                            int childIndex = control2.Controls.GetChildIndex(array[i]);
                            control2.Controls.Remove(array[i]);
                            control2.Controls.Add(array[i]);
                            control2.Controls.SetChildIndex(array[i], childIndex);
                        }
                        IComponentInitializer designer = host.GetDesigner(component) as IComponentInitializer;
                        if (designer != null)
                        {
                            designer.InitializeExistingComponent(null);
                        }
                        this.AddChildComponents(array[i], container, host);
                    }
                }
            }
        }

        internal virtual void AddChildControl(Control newChild)
        {
            if (((newChild.Left == 0) && (newChild.Top == 0)) && ((newChild.Width >= this.Control.Width) && (newChild.Height >= this.Control.Height)))
            {
                Point location = newChild.Location;
                location.Offset(this.GridSize.Width, this.GridSize.Height);
                newChild.Location = location;
            }
            this.Control.Controls.Add(newChild);
            int newIndex = DetermineTopChildIndex(this.Control);
            this.Control.Controls.SetChildIndex(newChild, newIndex);
        }

        internal void AddControl(Control newChild, IDictionary defaultValues)
        {
            Point empty = Point.Empty;
            Size size = Size.Empty;
            Size size2 = new Size(0, 0);
            bool flag = (defaultValues != null) && defaultValues.Contains("Location");
            bool flag2 = (defaultValues != null) && defaultValues.Contains("Size");
            if (flag)
            {
                empty = (Point) defaultValues["Location"];
            }
            if (flag2)
            {
                size = (Size) defaultValues["Size"];
            }
            if ((defaultValues != null) && defaultValues.Contains("Offset"))
            {
                size2 = (Size) defaultValues["Offset"];
            }
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((((host != null) && (newChild != null)) && (!this.Control.Contains(newChild) && (host.GetDesigner(newChild) is ControlDesigner))) && (!(newChild is Form) || !((Form) newChild).TopLevel))
            {
                Rectangle dragRect = new Rectangle();
                if (flag)
                {
                    empty = this.Control.PointToClient(empty);
                    dragRect.X = empty.X;
                    dragRect.Y = empty.Y;
                }
                else
                {
                    ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                    object primarySelection = service.PrimarySelection;
                    Control controlForComponent = null;
                    if (primarySelection != null)
                    {
                        controlForComponent = ((IOleDragClient) this).GetControlForComponent(primarySelection);
                    }
                    if ((controlForComponent != null) && (controlForComponent.Site == null))
                    {
                        controlForComponent = null;
                    }
                    if ((primarySelection == base.Component) || (controlForComponent == null))
                    {
                        dragRect.X = this.DefaultControlLocation.X;
                        dragRect.Y = this.DefaultControlLocation.Y;
                    }
                    else
                    {
                        dragRect.X = controlForComponent.Location.X + this.GridSize.Width;
                        dragRect.Y = controlForComponent.Location.Y + this.GridSize.Height;
                    }
                }
                if (flag2)
                {
                    dragRect.Width = size.Width;
                    dragRect.Height = size.Height;
                }
                else
                {
                    dragRect.Size = this.GetDefaultSize(newChild);
                }
                if (!flag2 && !flag)
                {
                    Rectangle adjustedSnapLocation = this.GetAdjustedSnapLocation(Rectangle.Empty, dragRect);
                    dragRect = this.GetControlStackLocation(adjustedSnapLocation);
                }
                else
                {
                    dragRect = this.GetAdjustedSnapLocation(Rectangle.Empty, dragRect);
                }
                dragRect.X += size2.Width;
                dragRect.Y += size2.Height;
                if ((defaultValues != null) && defaultValues.Contains("ToolboxSnapDragDropEventArgs"))
                {
                    ToolboxSnapDragDropEventArgs e = defaultValues["ToolboxSnapDragDropEventArgs"] as ToolboxSnapDragDropEventArgs;
                    Rectangle rectangle3 = DesignerUtils.GetBoundsFromToolboxSnapDragDropInfo(e, dragRect, this.Control.IsMirrored);
                    Control rootComponent = host.RootComponent as Control;
                    if ((rootComponent != null) && rectangle3.IntersectsWith(rootComponent.ClientRectangle))
                    {
                        dragRect = rectangle3;
                    }
                }
                PropertyDescriptor member = TypeDescriptor.GetProperties(this.Control)["Controls"];
                if (this.componentChangeSvc != null)
                {
                    this.componentChangeSvc.OnComponentChanging(this.Control, member);
                }
                this.AddChildControl(newChild);
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(newChild);
                if (properties != null)
                {
                    PropertyDescriptor descriptor2 = properties["Size"];
                    if (descriptor2 != null)
                    {
                        descriptor2.SetValue(newChild, new Size(dragRect.Width, dragRect.Height));
                    }
                    Point point2 = new Point(dragRect.X, dragRect.Y);
                    ScrollableControl parent = newChild.Parent as ScrollableControl;
                    if (parent != null)
                    {
                        Point autoScrollPosition = parent.AutoScrollPosition;
                        point2.Offset(-autoScrollPosition.X, -autoScrollPosition.Y);
                    }
                    descriptor2 = properties["Location"];
                    if (descriptor2 != null)
                    {
                        descriptor2.SetValue(newChild, point2);
                    }
                }
                if (this.componentChangeSvc != null)
                {
                    this.componentChangeSvc.OnComponentChanged(this.Control, member, this.Control.Controls, this.Control.Controls);
                }
                newChild.Update();
            }
        }

        protected void AddPaddingSnapLines(ref ArrayList snapLines)
        {
            if (snapLines == null)
            {
                snapLines = new ArrayList(4);
            }
            Point offsetToClientArea = base.GetOffsetToClientArea();
            Rectangle displayRectangle = this.Control.DisplayRectangle;
            displayRectangle.X += offsetToClientArea.X;
            displayRectangle.Y += offsetToClientArea.Y;
            snapLines.Add(new SnapLine(SnapLineType.Vertical, displayRectangle.Left, "Padding.Left", SnapLinePriority.Always));
            snapLines.Add(new SnapLine(SnapLineType.Vertical, displayRectangle.Right, "Padding.Right", SnapLinePriority.Always));
            snapLines.Add(new SnapLine(SnapLineType.Horizontal, displayRectangle.Top, "Padding.Top", SnapLinePriority.Always));
            snapLines.Add(new SnapLine(SnapLineType.Horizontal, displayRectangle.Bottom, "Padding.Bottom", SnapLinePriority.Always));
        }

        protected internal virtual bool CanAddComponent(IComponent component)
        {
            return true;
        }

        public virtual bool CanParent(Control control)
        {
            return !control.Contains(this.Control);
        }

        public virtual bool CanParent(ControlDesigner controlDesigner)
        {
            return this.CanParent(controlDesigner.Control);
        }

        protected void CreateTool(ToolboxItem tool)
        {
            this.CreateToolCore(tool, 0, 0, 0, 0, false, false);
        }

        protected void CreateTool(ToolboxItem tool, Point location)
        {
            this.CreateToolCore(tool, location.X, location.Y, 0, 0, true, false);
        }

        protected void CreateTool(ToolboxItem tool, Rectangle bounds)
        {
            this.CreateToolCore(tool, bounds.X, bounds.Y, bounds.Width, bounds.Height, true, true);
        }

        protected virtual IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            IComponent[] componentArray = null;
            try
            {
                componentArray = this.GetOleDragHandler().CreateTool(tool, this.Control, x, y, width, height, hasLocation, hasSize, this.toolboxSnapDragDropEventArgs);
            }
            finally
            {
                this.toolboxSnapDragDropEventArgs = null;
            }
            return componentArray;
        }

        internal static int DetermineTopChildIndex(Control parent)
        {
            int num = 0;
            num = 0;
            while (num < (parent.Controls.Count - 1))
            {
                Control component = parent.Controls[num];
                if (component.Site != null)
                {
                    InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                    InheritanceLevel notInherited = InheritanceLevel.NotInherited;
                    if (attribute != null)
                    {
                        notInherited = attribute.InheritanceLevel;
                    }
                    if (notInherited == InheritanceLevel.NotInherited)
                    {
                        return num;
                    }
                }
                num++;
            }
            return num;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.OnMouseDragEnd(this.mouseDragBase == ControlDesigner.InvalidPoint);
                base.EnableDragDrop(false);
                if (this.Control is ScrollableControl)
                {
                    ((ScrollableControl) this.Control).Scroll -= new ScrollEventHandler(this.OnScroll);
                }
                if (((IDesignerHost) this.GetService(typeof(IDesignerHost))) != null)
                {
                    this.componentChangeSvc.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                    this.componentChangeSvc.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    this.componentChangeSvc = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DrawGridOfParentChanged(bool drawGrid)
        {
            if (this.parentCanSetDrawGrid)
            {
                bool getDefaultDrawGrid = this.getDefaultDrawGrid;
                this.DrawGrid = drawGrid;
                this.parentCanSetDrawGrid = true;
                this.getDefaultDrawGrid = getDefaultDrawGrid;
            }
        }

        internal void ForceComponentChanging()
        {
            this.componentChangeSvc.OnComponentChanging(this.Control, TypeDescriptor.GetProperties(this.Control)["Controls"]);
        }

        private static int FrameWidth(FrameStyle style)
        {
            if (style != FrameStyle.Dashed)
            {
                return 2;
            }
            return 1;
        }

        private SnapLine[] GenerateNewToolSnapLines(Rectangle r)
        {
            return new SnapLine[] { new SnapLine(SnapLineType.Left, r.Right), new SnapLine(SnapLineType.Right, r.Right), new SnapLine(SnapLineType.Bottom, r.Bottom), new SnapLine(SnapLineType.Top, r.Bottom) };
        }

        private Rectangle GetAdjustedSnapLocation(Rectangle originalRect, Rectangle dragRect)
        {
            Rectangle rectangle = this.GetUpdatedRect(originalRect, dragRect, true);
            rectangle.Width = dragRect.Width;
            rectangle.Height = dragRect.Height;
            Point defaultControlLocation = this.DefaultControlLocation;
            if (rectangle.X < defaultControlLocation.X)
            {
                rectangle.X = defaultControlLocation.X;
            }
            if (rectangle.Y < defaultControlLocation.Y)
            {
                rectangle.Y = defaultControlLocation.Y;
            }
            return rectangle;
        }

        internal object[] GetComponentsInRect(Rectangle value, bool screenCoords, bool containRect)
        {
            ArrayList list = new ArrayList();
            Rectangle rect = screenCoords ? this.Control.RectangleToClient(value) : value;
            IContainer container = base.Component.Site.Container;
            Control control = this.Control;
            int count = control.Controls.Count;
            for (int i = 0; i < count; i++)
            {
                Control control2 = control.Controls[i];
                Rectangle bounds = control2.Bounds;
                container = DesignerUtils.CheckForNestedContainer(container);
                if ((control2.Visible && ((containRect && rect.Contains(bounds)) || (!containRect && bounds.IntersectsWith(rect)))) && ((control2.Site != null) && (control2.Site.Container == container)))
                {
                    list.Add(control2);
                }
            }
            return list.ToArray();
        }

        protected Control GetControl(object component)
        {
            IComponent component2 = component as IComponent;
            if (component2 != null)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    ControlDesigner designer = service.GetDesigner(component2) as ControlDesigner;
                    if (designer != null)
                    {
                        return designer.Control;
                    }
                }
            }
            return null;
        }

        protected override ControlBodyGlyph GetControlGlyph(GlyphSelectionType selectionType)
        {
            this.OnSetCursor();
            Rectangle b = base.BehaviorService.ControlRectInAdornerWindow(this.Control);
            Control parent = this.Control.Parent;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (((parent != null) && (service != null)) && (service.RootComponent != base.Component))
            {
                Rectangle a = base.BehaviorService.ControlRectInAdornerWindow(parent);
                Rectangle bounds = Rectangle.Intersect(a, b);
                if (selectionType == GlyphSelectionType.NotSelected)
                {
                    if (!bounds.IsEmpty && !a.Contains(b))
                    {
                        return new ControlBodyGlyph(bounds, Cursor.Current, this.Control, this);
                    }
                    if (bounds.IsEmpty)
                    {
                        return null;
                    }
                }
            }
            return new ControlBodyGlyph(b, Cursor.Current, this.Control, this);
        }

        private Rectangle GetControlStackLocation(Rectangle centeredLocation)
        {
            Control control = this.Control;
            int height = control.ClientSize.Height;
            int width = control.ClientSize.Width;
            if ((centeredLocation.Bottom >= height) || (centeredLocation.Right >= width))
            {
                centeredLocation.X = this.DefaultControlLocation.X;
                centeredLocation.Y = this.DefaultControlLocation.Y;
            }
            return centeredLocation;
        }

        private Size GetDefaultSize(IComponent component)
        {
            Size empty = Size.Empty;
            DefaultValueAttribute attribute = null;
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["AutoSize"];
            if (((descriptor != null) && !descriptor.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) && (!descriptor.Attributes.Contains(BrowsableAttribute.No) && ((bool) descriptor.GetValue(component))))
            {
                descriptor = TypeDescriptor.GetProperties(component)["PreferredSize"];
                if (descriptor != null)
                {
                    empty = (Size) descriptor.GetValue(component);
                    if (empty != Size.Empty)
                    {
                        return empty;
                    }
                }
            }
            descriptor = TypeDescriptor.GetProperties(component)["Size"];
            if (descriptor != null)
            {
                empty = (Size) descriptor.GetValue(component);
                if ((empty.Width > 0) && (empty.Height > 0))
                {
                    return empty;
                }
                attribute = (DefaultValueAttribute) descriptor.Attributes[typeof(DefaultValueAttribute)];
                if (attribute != null)
                {
                    return (Size) attribute.Value;
                }
            }
            return new Size(0x4b, 0x17);
        }

        public override GlyphCollection GetGlyphs(GlyphSelectionType selectionType)
        {
            GlyphCollection glyphs = base.GetGlyphs(selectionType);
            if ((((this.SelectionRules & SelectionRules.Moveable) != SelectionRules.None) && (this.InheritanceAttribute != InheritanceAttribute.InheritedReadOnly)) && (selectionType != GlyphSelectionType.NotSelected))
            {
                Point location = base.BehaviorService.ControlToAdornerWindow((Control) base.Component);
                Rectangle containerBounds = new Rectangle(location, ((Control) base.Component).Size);
                int glyphOffset = (int) (DesignerUtils.CONTAINERGRABHANDLESIZE * 0.5);
                if (containerBounds.Width < (2 * DesignerUtils.CONTAINERGRABHANDLESIZE))
                {
                    glyphOffset = -1 * glyphOffset;
                }
                ContainerSelectorBehavior behavior = new ContainerSelectorBehavior((Control) base.Component, base.Component.Site, true);
                ContainerSelectorGlyph glyph = new ContainerSelectorGlyph(containerBounds, DesignerUtils.CONTAINERGRABHANDLESIZE, glyphOffset, behavior);
                glyphs.Insert(0, glyph);
            }
            return glyphs;
        }

        internal OleDragDropHandler GetOleDragHandler()
        {
            if (this.oleDragDropHandler == null)
            {
                this.oleDragDropHandler = new OleDragDropHandler(null, (IServiceProvider) this.GetService(typeof(IDesignerHost)), this);
            }
            return this.oleDragDropHandler;
        }

        private ParentControlDesigner GetParentControlDesignerOfParent()
        {
            Control parent = this.Control.Parent;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((parent != null) && (service != null))
            {
                return (service.GetDesigner(parent) as ParentControlDesigner);
            }
            return null;
        }

        protected virtual Control GetParentForComponent(IComponent component)
        {
            return this.Control;
        }

        internal Point GetSnappedPoint(Point pt)
        {
            Rectangle rectangle = this.GetUpdatedRect(Rectangle.Empty, new Rectangle(pt.X, pt.Y, 0, 0), false);
            return new Point(rectangle.X, rectangle.Y);
        }

        internal Rectangle GetSnappedRect(Rectangle originalRect, Rectangle dragRect, bool updateSize)
        {
            return this.GetUpdatedRect(originalRect, dragRect, updateSize);
        }

        protected Rectangle GetUpdatedRect(Rectangle originalRect, Rectangle dragRect, bool updateSize)
        {
            Rectangle empty = Rectangle.Empty;
            if (this.SnapToGrid)
            {
                Size gridSize = this.GridSize;
                Point point = new Point(gridSize.Width / 2, gridSize.Height / 2);
                empty = dragRect;
                empty.X = originalRect.X;
                empty.Y = originalRect.Y;
                if (dragRect.X != originalRect.X)
                {
                    empty.X = (dragRect.X / gridSize.Width) * gridSize.Width;
                    if ((dragRect.X - empty.X) > point.X)
                    {
                        empty.X += gridSize.Width;
                    }
                }
                if (dragRect.Y != originalRect.Y)
                {
                    empty.Y = (dragRect.Y / gridSize.Height) * gridSize.Height;
                    if ((dragRect.Y - empty.Y) > point.Y)
                    {
                        empty.Y += gridSize.Height;
                    }
                }
                if (updateSize)
                {
                    empty.Width = (((dragRect.X + dragRect.Width) / gridSize.Width) * gridSize.Width) - empty.X;
                    empty.Height = (((dragRect.Y + dragRect.Height) / gridSize.Height) * gridSize.Height) - empty.Y;
                    if (empty.Width < gridSize.Width)
                    {
                        empty.Width = gridSize.Width;
                    }
                    if (empty.Height < gridSize.Height)
                    {
                        empty.Height = gridSize.Height;
                    }
                }
                return empty;
            }
            return dragRect;
        }

        private void GridSizeOfParentChanged(Size gridSize)
        {
            if (this.parentCanSetGridSize)
            {
                bool getDefaultGridSize = this.getDefaultGridSize;
                this.GridSize = gridSize;
                this.parentCanSetGridSize = true;
                this.getDefaultGridSize = getDefaultGridSize;
            }
        }

        private void GridSnapOfParentChanged(bool gridSnap)
        {
            if (this.parentCanSetGridSnap)
            {
                bool getDefaultGridSnap = this.getDefaultGridSnap;
                this.SnapToGrid = gridSnap;
                this.parentCanSetGridSnap = true;
                this.getDefaultGridSnap = getDefaultGridSnap;
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            if (this.Control is ScrollableControl)
            {
                ((ScrollableControl) this.Control).Scroll += new ScrollEventHandler(this.OnScroll);
            }
            base.EnableDragDrop(true);
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                this.componentChangeSvc = (IComponentChangeService) service.GetService(typeof(IComponentChangeService));
                if (this.componentChangeSvc != null)
                {
                    this.componentChangeSvc.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
                    this.componentChangeSvc.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                }
            }
            this.statusCommandUI = new StatusCommandUI(component.Site);
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            if (this.AllowControlLasso && (((defaultValues != null) && (defaultValues["Size"] != null)) && ((defaultValues["Location"] != null) && (defaultValues["Parent"] != null))))
            {
                Rectangle rectangle = new Rectangle((Point) defaultValues["Location"], (Size) defaultValues["Size"]);
                IComponent component = defaultValues["Parent"] as IComponent;
                if (component != null)
                {
                    IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if (service != null)
                    {
                        ParentControlDesigner designer = service.GetDesigner(component) as ParentControlDesigner;
                        if (designer != null)
                        {
                            object[] c = designer.GetComponentsInRect(rectangle, true, true);
                            if ((c != null) && (c.Length != 0))
                            {
                                ArrayList controls = new ArrayList(c);
                                if (controls.Contains(this.Control))
                                {
                                    controls.Remove(this.Control);
                                }
                                this.ReParentControls(this.Control, controls, System.Design.SR.GetString("ParentControlDesignerLassoShortcutRedo", new object[] { this.Control.Site.Name }), service);
                            }
                        }
                    }
                }
            }
        }

        protected static void InvokeCreateTool(ParentControlDesigner toInvoke, ToolboxItem tool)
        {
            toInvoke.CreateTool(tool);
        }

        private bool IsOptionDefault(string optionName, object value)
        {
            IDesignerOptionService service = (IDesignerOptionService) this.GetService(typeof(IDesignerOptionService));
            object optionValue = null;
            if (service == null)
            {
                if (optionName.Equals("ShowGrid"))
                {
                    optionValue = true;
                }
                else if (optionName.Equals("SnapToGrid"))
                {
                    optionValue = true;
                }
                else if (optionName.Equals("GridSize"))
                {
                    optionValue = new Size(8, 8);
                }
            }
            else
            {
                optionValue = DesignerUtils.GetOptionValue(this.ServiceProvider, optionName);
            }
            if (optionValue != null)
            {
                return optionValue.Equals(value);
            }
            return (value == null);
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            if (e.Component == this.pendingRemoveControl)
            {
                this.pendingRemoveControl = null;
                this.componentChangeSvc.OnComponentChanged(this.Control, TypeDescriptor.GetProperties(this.Control)["Controls"], null, null);
            }
        }

        private void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            Control component = e.Component as Control;
            if (((component != null) && (component.Parent != null)) && (component.Parent == this.Control))
            {
                this.pendingRemoveControl = component;
                if (this.suspendChanging == 0)
                {
                    this.componentChangeSvc.OnComponentChanging(this.Control, TypeDescriptor.GetProperties(this.Control)["Controls"]);
                }
            }
        }

        protected override void OnDragComplete(DragEventArgs de)
        {
            DropSourceBehavior.BehaviorDataObject data = de.Data as DropSourceBehavior.BehaviorDataObject;
            if (data != null)
            {
                data.CleanupDrag();
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            if (de is ToolboxSnapDragDropEventArgs)
            {
                this.toolboxSnapDragDropEventArgs = de as ToolboxSnapDragDropEventArgs;
            }
            DropSourceBehavior.BehaviorDataObject data = de.Data as DropSourceBehavior.BehaviorDataObject;
            if (data != null)
            {
                data.Target = base.Component;
                data.EndDragDrop(this.AllowSetChildIndexOnDrop);
                this.OnDragComplete(de);
            }
            else if ((this.mouseDragTool == null) && (data == null))
            {
                OleDragDropHandler oleDragHandler = this.GetOleDragHandler();
                if (oleDragHandler != null)
                {
                    IOleDragClient destination = oleDragHandler.Destination;
                    if (((destination != null) && (destination.Component != null)) && (destination.Component.Site != null))
                    {
                        IContainer container = destination.Component.Site.Container;
                        if (container != null)
                        {
                            object[] draggingObjects = oleDragHandler.GetDraggingObjects(de);
                            for (int i = 0; i < draggingObjects.Length; i++)
                            {
                                IComponent component = draggingObjects[i] as IComponent;
                                container.Add(component);
                            }
                        }
                    }
                }
            }
            if (this.mouseDragTool != null)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    service.Activate();
                }
                try
                {
                    if (base.BehaviorService != null)
                    {
                        base.BehaviorService.EndDragNotification();
                    }
                    this.CreateTool(this.mouseDragTool, new Point(de.X, de.Y));
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                    base.DisplayError(exception);
                }
                this.mouseDragTool = null;
            }
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            bool flag = false;
            DropSourceBehavior.BehaviorDataObject obj2 = null;
            DropSourceBehavior.BehaviorDataObject data = de.Data as DropSourceBehavior.BehaviorDataObject;
            if (data != null)
            {
                obj2 = data;
                obj2.Target = base.Component;
                de.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
                flag = !data.Source.Equals(base.Component);
            }
            IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                MenuCommand command = service.FindCommand(StandardCommands.TabOrder);
                if ((command != null) && command.Checked)
                {
                    de.Effect = DragDropEffects.None;
                    return;
                }
            }
            object[] array = null;
            if ((obj2 != null) && (obj2.DragComponents != null))
            {
                array = new object[obj2.DragComponents.Count];
                obj2.DragComponents.CopyTo(array, 0);
            }
            else
            {
                array = this.GetOleDragHandler().GetDraggingObjects(de);
            }
            Control controlForComponent = null;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                DocumentDesigner designer = host.GetDesigner(host.RootComponent) as DocumentDesigner;
                if ((designer != null) && !designer.CanDropComponents(de))
                {
                    de.Effect = DragDropEffects.None;
                    return;
                }
            }
            if (array != null)
            {
                if (data == null)
                {
                    flag = true;
                }
                for (int i = 0; i < array.Length; i++)
                {
                    IComponent component = array[i] as IComponent;
                    if ((host != null) && (component != null))
                    {
                        if (flag)
                        {
                            InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                            if (((attribute != null) && !attribute.Equals(InheritanceAttribute.NotInherited)) && !attribute.Equals(InheritanceAttribute.InheritedReadOnly))
                            {
                                de.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                        if (host.GetDesigner(component) is IOleDragClient)
                        {
                            controlForComponent = ((IOleDragClient) this).GetControlForComponent(array[i]);
                        }
                        Control control2 = array[i] as Control;
                        if ((controlForComponent == null) && (control2 != null))
                        {
                            controlForComponent = control2;
                        }
                        if (controlForComponent != null)
                        {
                            if ((this.InheritanceAttribute == InheritanceAttribute.InheritedReadOnly) && (controlForComponent.Parent != this.Control))
                            {
                                de.Effect = DragDropEffects.None;
                                return;
                            }
                            if (!((IOleDragClient) this).IsDropOk(component))
                            {
                                de.Effect = DragDropEffects.None;
                                return;
                            }
                        }
                    }
                }
                if (data == null)
                {
                    this.PerformDragEnter(de, host);
                }
            }
            if (this.toolboxService == null)
            {
                this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
            }
            if ((this.toolboxService != null) && (array == null))
            {
                this.mouseDragTool = this.toolboxService.DeserializeToolboxItem(de.Data, host);
                if (((this.mouseDragTool != null) && (base.BehaviorService != null)) && base.BehaviorService.UseSnapLines)
                {
                    if (this.toolboxItemSnapLineBehavior == null)
                    {
                        this.toolboxItemSnapLineBehavior = new ToolboxItemSnapLineBehavior(base.Component.Site, base.BehaviorService, this, this.AllowGenericDragBox);
                    }
                    if (!this.toolboxItemSnapLineBehavior.IsPushed)
                    {
                        base.BehaviorService.PushBehavior(this.toolboxItemSnapLineBehavior);
                        this.toolboxItemSnapLineBehavior.IsPushed = true;
                    }
                }
                if (this.mouseDragTool != null)
                {
                    this.PerformDragEnter(de, host);
                }
                if (this.toolboxItemSnapLineBehavior != null)
                {
                    this.toolboxItemSnapLineBehavior.OnBeginDrag();
                }
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            if ((this.toolboxItemSnapLineBehavior != null) && this.toolboxItemSnapLineBehavior.IsPushed)
            {
                base.BehaviorService.PopBehavior(this.toolboxItemSnapLineBehavior);
                this.toolboxItemSnapLineBehavior.IsPushed = false;
            }
            this.mouseDragTool = null;
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            DropSourceBehavior.BehaviorDataObject data = de.Data as DropSourceBehavior.BehaviorDataObject;
            if (data != null)
            {
                data.Target = base.Component;
                de.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
            }
            IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                MenuCommand command = service.FindCommand(StandardCommands.TabOrder);
                if ((command != null) && command.Checked)
                {
                    de.Effect = DragDropEffects.None;
                    return;
                }
            }
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                DocumentDesigner designer = host.GetDesigner(host.RootComponent) as DocumentDesigner;
                if ((designer != null) && !designer.CanDropComponents(de))
                {
                    de.Effect = DragDropEffects.None;
                    return;
                }
            }
            if (this.mouseDragTool != null)
            {
                de.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnMouseDragBegin(int x, int y)
        {
            Control control = this.Control;
            if (!this.InheritanceAttribute.Equals(InheritanceAttribute.InheritedReadOnly))
            {
                if (this.toolboxService == null)
                {
                    this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
                }
                if (this.toolboxService != null)
                {
                    this.mouseDragTool = this.toolboxService.GetSelectedToolboxItem((IDesignerHost) this.GetService(typeof(IDesignerHost)));
                }
            }
            control.Capture = true;
            System.Design.NativeMethods.RECT rect = new System.Design.NativeMethods.RECT();
            System.Design.NativeMethods.GetWindowRect(control.Handle, ref rect);
            Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
            this.mouseDragFrame = (this.mouseDragTool == null) ? FrameStyle.Dashed : FrameStyle.Thick;
            this.mouseDragBase = new Point(x, y);
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (service != null)
            {
                service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Click);
            }
            IEventHandlerService service2 = (IEventHandlerService) this.GetService(typeof(IEventHandlerService));
            if ((service2 != null) && (this.escapeHandler == null))
            {
                this.escapeHandler = new EscapeHandler(this);
                service2.PushHandler(this.escapeHandler);
            }
            this.adornerWindowToScreenOffset = base.BehaviorService.AdornerWindowToScreen();
        }

        protected override void OnMouseDragEnd(bool cancel)
        {
            if (this.mouseDragBase == ControlDesigner.InvalidPoint)
            {
                base.OnMouseDragEnd(cancel);
            }
            else
            {
                Rectangle mouseDragOffset = this.mouseDragOffset;
                ToolboxItem mouseDragTool = this.mouseDragTool;
                Point mouseDragBase = this.mouseDragBase;
                this.mouseDragOffset = Rectangle.Empty;
                this.mouseDragBase = ControlDesigner.InvalidPoint;
                this.mouseDragTool = null;
                this.Control.Capture = false;
                Cursor.Clip = Rectangle.Empty;
                if (!mouseDragOffset.IsEmpty && (this.graphics != null))
                {
                    Rectangle rect = new Rectangle(mouseDragOffset.X - this.adornerWindowToScreenOffset.X, mouseDragOffset.Y - this.adornerWindowToScreenOffset.Y, mouseDragOffset.Width, mouseDragOffset.Height);
                    int num = FrameWidth(this.mouseDragFrame);
                    this.graphics.SetClip(rect);
                    using (Region region = new Region(rect))
                    {
                        region.Exclude(Rectangle.Inflate(rect, -num, -num));
                        base.BehaviorService.Invalidate(region);
                    }
                    this.graphics.ResetClip();
                }
                if (this.graphics != null)
                {
                    this.graphics.Dispose();
                    this.graphics = null;
                }
                if (this.dragManager != null)
                {
                    this.dragManager.OnMouseUp();
                    this.dragManager = null;
                }
                IEventHandlerService service = (IEventHandlerService) this.GetService(typeof(IEventHandlerService));
                if ((service != null) && (this.escapeHandler != null))
                {
                    service.PopHandler(this.escapeHandler);
                    this.escapeHandler = null;
                }
                if ((this.statusCommandUI != null) && !mouseDragOffset.IsEmpty)
                {
                    System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(mouseDragBase.X, mouseDragBase.Y);
                    System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, this.Control.Handle, pt, 1);
                    if (this.statusCommandUI != null)
                    {
                        this.statusCommandUI.SetStatusInformation(new Rectangle(pt.x, pt.y, mouseDragOffset.Width, mouseDragOffset.Height));
                    }
                }
                if (mouseDragOffset.IsEmpty && !cancel)
                {
                    if (mouseDragTool != null)
                    {
                        try
                        {
                            this.CreateTool(mouseDragTool, mouseDragBase);
                            if (this.toolboxService != null)
                            {
                                this.toolboxService.SelectedToolboxItemUsed();
                            }
                        }
                        catch (Exception exception)
                        {
                            if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                            {
                                throw;
                            }
                            base.DisplayError(exception);
                        }
                    }
                }
                else if (!cancel)
                {
                    if (mouseDragTool != null)
                    {
                        try
                        {
                            Size size = new Size(DesignerUtils.MinDragSize.Width * 2, DesignerUtils.MinDragSize.Height * 2);
                            if (mouseDragOffset.Width < size.Width)
                            {
                                mouseDragOffset.Width = size.Width;
                            }
                            if (mouseDragOffset.Height < size.Height)
                            {
                                mouseDragOffset.Height = size.Height;
                            }
                            this.CreateTool(mouseDragTool, mouseDragOffset);
                            if (this.toolboxService != null)
                            {
                                this.toolboxService.SelectedToolboxItemUsed();
                            }
                            return;
                        }
                        catch (Exception exception2)
                        {
                            if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                            {
                                throw;
                            }
                            base.DisplayError(exception2);
                            return;
                        }
                    }
                    ISelectionService service2 = null;
                    service2 = (ISelectionService) this.GetService(typeof(ISelectionService));
                    if (service2 != null)
                    {
                        object[] components = this.GetComponentsInRect(mouseDragOffset, true, false);
                        if (components.Length > 0)
                        {
                            service2.SetSelectedComponents(components);
                        }
                    }
                }
            }
        }

        protected override void OnMouseDragMove(int x, int y)
        {
            if ((this.toolboxItemSnapLineBehavior != null) && this.toolboxItemSnapLineBehavior.IsPushed)
            {
                base.BehaviorService.PopBehavior(this.toolboxItemSnapLineBehavior);
                this.toolboxItemSnapLineBehavior.IsPushed = false;
            }
            if (!this.GetOleDragHandler().Dragging && (this.mouseDragBase != ControlDesigner.InvalidPoint))
            {
                Rectangle mouseDragOffset = this.mouseDragOffset;
                this.mouseDragOffset.X = this.mouseDragBase.X;
                this.mouseDragOffset.Y = this.mouseDragBase.Y;
                this.mouseDragOffset.Width = x - this.mouseDragBase.X;
                this.mouseDragOffset.Height = y - this.mouseDragBase.Y;
                if (((this.dragManager == null) && this.ParticipatesWithSnapLines) && ((this.mouseDragTool != null) && base.BehaviorService.UseSnapLines))
                {
                    this.dragManager = new DragAssistanceManager(base.Component.Site);
                }
                if (this.dragManager != null)
                {
                    Rectangle dragBounds = new Rectangle(this.mouseDragBase.X - this.adornerWindowToScreenOffset.X, this.mouseDragBase.Y - this.adornerWindowToScreenOffset.Y, x - this.mouseDragBase.X, y - this.mouseDragBase.Y);
                    Point point = this.dragManager.OnMouseMove(dragBounds, this.GenerateNewToolSnapLines(dragBounds));
                    this.mouseDragOffset.Width += point.X;
                    this.mouseDragOffset.Height += point.Y;
                    this.dragManager.RenderSnapLinesInternal();
                }
                if (this.mouseDragOffset.Width < 0)
                {
                    this.mouseDragOffset.X += this.mouseDragOffset.Width;
                    this.mouseDragOffset.Width = -this.mouseDragOffset.Width;
                }
                if (this.mouseDragOffset.Height < 0)
                {
                    this.mouseDragOffset.Y += this.mouseDragOffset.Height;
                    this.mouseDragOffset.Height = -this.mouseDragOffset.Height;
                }
                if (this.mouseDragTool != null)
                {
                    this.mouseDragOffset = this.Control.RectangleToClient(this.mouseDragOffset);
                    this.mouseDragOffset = this.GetUpdatedRect(Rectangle.Empty, this.mouseDragOffset, true);
                    this.mouseDragOffset = this.Control.RectangleToScreen(this.mouseDragOffset);
                }
                if (this.graphics == null)
                {
                    this.graphics = base.BehaviorService.AdornerWindowGraphics;
                }
                if (!this.mouseDragOffset.IsEmpty && (this.graphics != null))
                {
                    Rectangle rect = new Rectangle(this.mouseDragOffset.X - this.adornerWindowToScreenOffset.X, this.mouseDragOffset.Y - this.adornerWindowToScreenOffset.Y, this.mouseDragOffset.Width, this.mouseDragOffset.Height);
                    using (Region region = new Region(rect))
                    {
                        int num = FrameWidth(this.mouseDragFrame);
                        region.Exclude(Rectangle.Inflate(rect, -num, -num));
                        if (!mouseDragOffset.IsEmpty)
                        {
                            mouseDragOffset.X -= this.adornerWindowToScreenOffset.X;
                            mouseDragOffset.Y -= this.adornerWindowToScreenOffset.Y;
                            using (Region region2 = new Region(mouseDragOffset))
                            {
                                region2.Exclude(Rectangle.Inflate(mouseDragOffset, -num, -num));
                                base.BehaviorService.Invalidate(region2);
                            }
                        }
                        DesignerUtils.DrawFrame(this.graphics, region, this.mouseDragFrame, this.Control.BackColor);
                    }
                }
                if (this.statusCommandUI != null)
                {
                    System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(this.mouseDragOffset.X, this.mouseDragOffset.Y);
                    System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, this.Control.Handle, pt, 1);
                    if (this.statusCommandUI != null)
                    {
                        this.statusCommandUI.SetStatusInformation(new Rectangle(pt.x, pt.y, this.mouseDragOffset.Width, this.mouseDragOffset.Height));
                    }
                }
            }
        }

        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            if (this.DrawGrid)
            {
                Control control = this.Control;
                Rectangle displayRectangle = this.Control.DisplayRectangle;
                Rectangle clientRectangle = this.Control.ClientRectangle;
                int x = Math.Min(displayRectangle.X, clientRectangle.X);
                int y = Math.Min(displayRectangle.Y, clientRectangle.Y);
                int width = Math.Max(displayRectangle.Width, clientRectangle.Width);
                Rectangle area = new Rectangle(x, y, width, Math.Max(displayRectangle.Height, clientRectangle.Height));
                float dx = area.X;
                float dy = area.Y;
                pe.Graphics.TranslateTransform(dx, dy);
                area.X = area.Y = 0;
                area.Width++;
                area.Height++;
                ControlPaint.DrawGrid(pe.Graphics, area, this.GridSize, control.BackColor);
                pe.Graphics.TranslateTransform(-dx, -dy);
            }
            base.OnPaintAdornments(pe);
        }

        private void OnScroll(object sender, ScrollEventArgs se)
        {
            base.BehaviorService.Invalidate(base.BehaviorService.ControlRectInAdornerWindow(this.Control));
        }

        protected override void OnSetCursor()
        {
            if (this.toolboxService == null)
            {
                this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
            }
            try
            {
                if (((this.toolboxService == null) || !this.toolboxService.SetCursor()) || this.InheritanceAttribute.Equals(InheritanceAttribute.InheritedReadOnly))
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            catch
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void PerformDragEnter(DragEventArgs de, IDesignerHost host)
        {
            if (host != null)
            {
                host.Activate();
            }
            if ((de.AllowedEffect & DragDropEffects.Move) != DragDropEffects.None)
            {
                de.Effect = DragDropEffects.Move;
            }
            else
            {
                de.Effect = DragDropEffects.Copy;
            }
            if (this.InheritanceAttribute == InheritanceAttribute.InheritedReadOnly)
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Replace);
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            if (!this.DefaultUseSnapLines)
            {
                properties["DrawGrid"] = TypeDescriptor.CreateProperty(typeof(ParentControlDesigner), "DrawGrid", typeof(bool), new Attribute[] { BrowsableAttribute.Yes, DesignOnlyAttribute.Yes, new System.Design.SRDescriptionAttribute("ParentControlDesignerDrawGridDescr"), CategoryAttribute.Design });
                properties["SnapToGrid"] = TypeDescriptor.CreateProperty(typeof(ParentControlDesigner), "SnapToGrid", typeof(bool), new Attribute[] { BrowsableAttribute.Yes, DesignOnlyAttribute.Yes, new System.Design.SRDescriptionAttribute("ParentControlDesignerSnapToGridDescr"), CategoryAttribute.Design });
                properties["GridSize"] = TypeDescriptor.CreateProperty(typeof(ParentControlDesigner), "GridSize", typeof(Size), new Attribute[] { BrowsableAttribute.Yes, new System.Design.SRDescriptionAttribute("ParentControlDesignerGridSizeDescr"), DesignOnlyAttribute.Yes, CategoryAttribute.Design });
            }
            properties["CurrentGridSize"] = TypeDescriptor.CreateProperty(typeof(ParentControlDesigner), "CurrentGridSize", typeof(Size), new Attribute[] { BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden });
        }

        private void ReParentControls(Control newParent, ArrayList controls, string transactionName, IDesignerHost host)
        {
            using (DesignerTransaction transaction = host.CreateTransaction(transactionName))
            {
                IComponentChangeService service = this.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(newParent)["Controls"];
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(newParent)["Location"];
                Point empty = Point.Empty;
                if (descriptor2 != null)
                {
                    empty = (Point) descriptor2.GetValue(newParent);
                }
                if (service != null)
                {
                    service.OnComponentChanging(newParent, member);
                }
                foreach (object obj2 in controls)
                {
                    Control component = obj2 as Control;
                    Control parent = component.Parent;
                    Point point2 = Point.Empty;
                    InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                    if ((attribute == null) || (attribute != InheritanceAttribute.InheritedReadOnly))
                    {
                        PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(component)["Location"];
                        if (descriptor3 != null)
                        {
                            point2 = (Point) descriptor3.GetValue(component);
                        }
                        if (parent != null)
                        {
                            if (service != null)
                            {
                                service.OnComponentChanging(parent, member);
                            }
                            parent.Controls.Remove(component);
                        }
                        newParent.Controls.Add(component);
                        Point point3 = Point.Empty;
                        if (parent != null)
                        {
                            if (parent.Controls.Contains(newParent))
                            {
                                point3 = new Point(point2.X - empty.X, point2.Y - empty.Y);
                            }
                            else
                            {
                                Point point4 = (Point) descriptor3.GetValue(parent);
                                point3 = new Point(point2.X + point4.X, point2.Y + point4.Y);
                            }
                        }
                        descriptor3.SetValue(component, point3);
                        if ((service != null) && (parent != null))
                        {
                            service.OnComponentChanged(parent, member, null, null);
                        }
                    }
                }
                if (service != null)
                {
                    service.OnComponentChanged(newParent, member, null, null);
                }
                transaction.Commit();
            }
        }

        private void ResetDrawGrid()
        {
            this.getDefaultDrawGrid = true;
            this.parentCanSetDrawGrid = true;
            Control control = this.Control;
            if (control != null)
            {
                control.Invalidate(true);
            }
        }

        private void ResetGridSize()
        {
            this.getDefaultGridSize = true;
            this.parentCanSetGridSize = true;
            Control control = this.Control;
            if (control != null)
            {
                control.Invalidate(true);
            }
        }

        private void ResetSnapToGrid()
        {
            this.getDefaultGridSnap = true;
            this.parentCanSetGridSnap = true;
        }

        internal void ResumeChangingEvents()
        {
            this.suspendChanging--;
        }

        private bool ShouldSerializeDrawGrid()
        {
            ParentControlDesigner parentControlDesignerOfParent = this.GetParentControlDesignerOfParent();
            if (parentControlDesignerOfParent != null)
            {
                return (this.DrawGrid != parentControlDesignerOfParent.DrawGrid);
            }
            return !this.IsOptionDefault("ShowGrid", this.DrawGrid);
        }

        private bool ShouldSerializeGridSize()
        {
            ParentControlDesigner parentControlDesignerOfParent = this.GetParentControlDesignerOfParent();
            if (parentControlDesignerOfParent != null)
            {
                return !this.GridSize.Equals(parentControlDesignerOfParent.GridSize);
            }
            return !this.IsOptionDefault("GridSize", this.GridSize);
        }

        private bool ShouldSerializeSnapToGrid()
        {
            ParentControlDesigner parentControlDesignerOfParent = this.GetParentControlDesignerOfParent();
            if (parentControlDesignerOfParent != null)
            {
                return (this.SnapToGrid != parentControlDesignerOfParent.SnapToGrid);
            }
            return !this.IsOptionDefault("SnapToGrid", this.SnapToGrid);
        }

        internal void SuspendChangingEvents()
        {
            this.suspendChanging++;
        }

        bool IOleDragClient.AddComponent(IComponent component, string name, bool firstAdd)
        {
            IContainer container = DesignerUtils.CheckForNestedContainer(base.Component.Site.Container);
            bool flag = true;
            IContainer container2 = null;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (!firstAdd)
            {
                if (component.Site != null)
                {
                    container2 = component.Site.Container;
                    flag = container != container2;
                    if (flag)
                    {
                        container2.Remove(component);
                    }
                }
                if (flag)
                {
                    if ((name != null) && (container.Components[name] != null))
                    {
                        name = null;
                    }
                    if (name != null)
                    {
                        container.Add(component, name);
                    }
                    else
                    {
                        container.Add(component);
                    }
                }
            }
            if (!((IOleDragClient) this).IsDropOk(component))
            {
                try
                {
                    IUIService service = (IUIService) this.GetService(typeof(IUIService));
                    string message = System.Design.SR.GetString("DesignerCantParentType", new object[] { component.GetType().Name, base.Component.GetType().Name });
                    if (service != null)
                    {
                        service.ShowError(message);
                    }
                    else
                    {
                        System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, null, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    }
                    return false;
                }
                finally
                {
                    if (flag)
                    {
                        container.Remove(component);
                        if (container2 != null)
                        {
                            container2.Add(component);
                        }
                    }
                    else
                    {
                        container.Remove(component);
                    }
                }
            }
            if (!this.CanAddComponent(component))
            {
                return false;
            }
            Control control = this.GetControl(component);
            if (control != null)
            {
                Control parentForComponent = this.GetParentForComponent(component);
                Form form = control as Form;
                if ((form == null) || !form.TopLevel)
                {
                    if (control.Parent != parentForComponent)
                    {
                        PropertyDescriptor member = TypeDescriptor.GetProperties(parentForComponent)["Controls"];
                        if (control.Parent != null)
                        {
                            Control parent = control.Parent;
                            if (this.componentChangeSvc != null)
                            {
                                this.componentChangeSvc.OnComponentChanging(parent, member);
                            }
                            parent.Controls.Remove(control);
                            if (this.componentChangeSvc != null)
                            {
                                this.componentChangeSvc.OnComponentChanged(parent, member, parent.Controls, parent.Controls);
                            }
                        }
                        if ((this.suspendChanging == 0) && (this.componentChangeSvc != null))
                        {
                            this.componentChangeSvc.OnComponentChanging(parentForComponent, member);
                        }
                        parentForComponent.Controls.Add(control);
                        if (this.componentChangeSvc != null)
                        {
                            this.componentChangeSvc.OnComponentChanged(parentForComponent, member, parentForComponent.Controls, parentForComponent.Controls);
                        }
                    }
                    else
                    {
                        int childIndex = parentForComponent.Controls.GetChildIndex(control);
                        parentForComponent.Controls.Remove(control);
                        parentForComponent.Controls.Add(control);
                        parentForComponent.Controls.SetChildIndex(control, childIndex);
                    }
                }
                control.Invalidate(true);
            }
            if ((host != null) && flag)
            {
                IComponentInitializer designer = host.GetDesigner(component) as IComponentInitializer;
                if (designer != null)
                {
                    designer.InitializeExistingComponent(null);
                }
                this.AddChildComponents(component, container, host);
            }
            return true;
        }

        Control IOleDragClient.GetControlForComponent(object component)
        {
            return this.GetControl(component);
        }

        Control IOleDragClient.GetDesignerControl()
        {
            return this.Control;
        }

        bool IOleDragClient.IsDropOk(IComponent component)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                IDesigner designer = service.GetDesigner(component);
                bool flag = false;
                if (designer == null)
                {
                    designer = TypeDescriptor.CreateDesigner(component, typeof(IDesigner));
                    ControlDesigner designer2 = designer as ControlDesigner;
                    if (designer2 != null)
                    {
                        designer2.ForceVisible = false;
                    }
                    designer.Initialize(component);
                    flag = true;
                }
                try
                {
                    ComponentDesigner designer3 = designer as ComponentDesigner;
                    if (designer3 != null)
                    {
                        if (designer3.CanBeAssociatedWith(this))
                        {
                            ControlDesigner controlDesigner = designer3 as ControlDesigner;
                            if (controlDesigner != null)
                            {
                                return this.CanParent(controlDesigner);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                finally
                {
                    if (flag)
                    {
                        designer.Dispose();
                    }
                }
            }
            return true;
        }

        protected virtual bool AllowControlLasso
        {
            get
            {
                return true;
            }
        }

        protected virtual bool AllowGenericDragBox
        {
            get
            {
                return true;
            }
        }

        protected internal virtual bool AllowSetChildIndexOnDrop
        {
            get
            {
                return true;
            }
        }

        private Size CurrentGridSize
        {
            get
            {
                return this.GridSize;
            }
        }

        protected virtual Point DefaultControlLocation
        {
            get
            {
                return new Point(0, 0);
            }
        }

        private bool DefaultUseSnapLines
        {
            get
            {
                if (this.checkSnapLineSetting)
                {
                    this.checkSnapLineSetting = false;
                    this.defaultUseSnapLines = DesignerUtils.UseSnapLines(base.Component.Site);
                }
                return this.defaultUseSnapLines;
            }
        }

        protected virtual bool DrawGrid
        {
            get
            {
                if (this.DefaultUseSnapLines)
                {
                    return false;
                }
                if (this.getDefaultDrawGrid)
                {
                    this.drawGrid = true;
                    ParentControlDesigner parentControlDesignerOfParent = this.GetParentControlDesignerOfParent();
                    if (parentControlDesignerOfParent != null)
                    {
                        this.drawGrid = parentControlDesignerOfParent.DrawGrid;
                    }
                    else
                    {
                        object optionValue = DesignerUtils.GetOptionValue(this.ServiceProvider, "ShowGrid");
                        if (optionValue is bool)
                        {
                            this.drawGrid = (bool) optionValue;
                        }
                    }
                }
                return this.drawGrid;
            }
            set
            {
                if (value != this.drawGrid)
                {
                    if (this.parentCanSetDrawGrid)
                    {
                        this.parentCanSetDrawGrid = false;
                    }
                    if (this.getDefaultDrawGrid)
                    {
                        this.getDefaultDrawGrid = false;
                    }
                    this.drawGrid = value;
                    Control control = this.Control;
                    if (control != null)
                    {
                        control.Invalidate(true);
                    }
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        foreach (Control control2 in this.Control.Controls)
                        {
                            ParentControlDesigner designer = service.GetDesigner(control2) as ParentControlDesigner;
                            if (designer != null)
                            {
                                designer.DrawGridOfParentChanged(this.drawGrid);
                            }
                        }
                    }
                }
            }
        }

        protected override bool EnableDragRect
        {
            get
            {
                return true;
            }
        }

        protected Size GridSize
        {
            get
            {
                if (this.getDefaultGridSize)
                {
                    this.gridSize = new Size(8, 8);
                    ParentControlDesigner parentControlDesignerOfParent = this.GetParentControlDesignerOfParent();
                    if (parentControlDesignerOfParent != null)
                    {
                        this.gridSize = parentControlDesignerOfParent.GridSize;
                    }
                    else
                    {
                        object optionValue = DesignerUtils.GetOptionValue(this.ServiceProvider, "GridSize");
                        if (optionValue is Size)
                        {
                            this.gridSize = (Size) optionValue;
                        }
                    }
                }
                return this.gridSize;
            }
            set
            {
                if (this.parentCanSetGridSize)
                {
                    this.parentCanSetGridSize = false;
                }
                if (this.getDefaultGridSize)
                {
                    this.getDefaultGridSize = false;
                }
                if (((value.Width < 2) || (value.Height < 2)) || ((value.Width > 200) || (value.Height > 200)))
                {
                    throw new ArgumentException(System.Design.SR.GetString("InvalidArgument", new object[] { "GridSize", value.ToString() }));
                }
                this.gridSize = value;
                Control control = this.Control;
                if (control != null)
                {
                    control.Invalidate(true);
                }
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    foreach (Control control2 in this.Control.Controls)
                    {
                        ParentControlDesigner designer = service.GetDesigner(control2) as ParentControlDesigner;
                        if (designer != null)
                        {
                            designer.GridSizeOfParentChanged(this.gridSize);
                        }
                    }
                }
            }
        }

        protected ToolboxItem MouseDragTool
        {
            get
            {
                return this.mouseDragTool;
            }
        }

        internal Size ParentGridSize
        {
            get
            {
                return this.GridSize;
            }
        }

        private IServiceProvider ServiceProvider
        {
            get
            {
                if (base.Component != null)
                {
                    return base.Component.Site;
                }
                return null;
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = base.SnapLines as ArrayList;
                if (snapLines == null)
                {
                    snapLines = new ArrayList(4);
                }
                this.AddPaddingSnapLines(ref snapLines);
                return snapLines;
            }
        }

        private bool SnapToGrid
        {
            get
            {
                if (this.DefaultUseSnapLines)
                {
                    return false;
                }
                if (this.getDefaultGridSnap)
                {
                    this.gridSnap = true;
                    ParentControlDesigner parentControlDesignerOfParent = this.GetParentControlDesignerOfParent();
                    if (parentControlDesignerOfParent != null)
                    {
                        this.gridSnap = parentControlDesignerOfParent.SnapToGrid;
                    }
                    else
                    {
                        object optionValue = DesignerUtils.GetOptionValue(this.ServiceProvider, "SnapToGrid");
                        if ((optionValue != null) && (optionValue is bool))
                        {
                            this.gridSnap = (bool) optionValue;
                        }
                    }
                }
                return this.gridSnap;
            }
            set
            {
                if (this.gridSnap != value)
                {
                    if (this.parentCanSetGridSnap)
                    {
                        this.parentCanSetGridSnap = false;
                    }
                    if (this.getDefaultGridSnap)
                    {
                        this.getDefaultGridSnap = false;
                    }
                    this.gridSnap = value;
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        foreach (Control control in this.Control.Controls)
                        {
                            ParentControlDesigner designer = service.GetDesigner(control) as ParentControlDesigner;
                            if (designer != null)
                            {
                                designer.GridSnapOfParentChanged(this.gridSnap);
                            }
                        }
                    }
                }
            }
        }

        bool IOleDragClient.CanModifyComponents
        {
            get
            {
                return !this.InheritanceAttribute.Equals(InheritanceAttribute.InheritedReadOnly);
            }
        }

        IComponent IOleDragClient.Component
        {
            get
            {
                return base.Component;
            }
        }

        private class EscapeHandler : IMenuStatusHandler
        {
            private ParentControlDesigner designer;

            public EscapeHandler(ParentControlDesigner designer)
            {
                this.designer = designer;
            }

            public bool OverrideInvoke(MenuCommand cmd)
            {
                if (cmd.CommandID.Equals(MenuCommands.KeyCancel))
                {
                    this.designer.OnMouseDragEnd(true);
                    return true;
                }
                return false;
            }

            public bool OverrideStatus(MenuCommand cmd)
            {
                if (cmd.CommandID.Equals(MenuCommands.KeyCancel))
                {
                    cmd.Enabled = true;
                }
                else
                {
                    cmd.Enabled = false;
                }
                return true;
            }
        }
    }
}

