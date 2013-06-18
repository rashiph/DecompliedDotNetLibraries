namespace System.Windows.Forms.Design
{
    using Accessibility;
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    public class ControlDesigner : ComponentDesigner
    {
        protected AccessibleObject accessibilityObj;
        private bool autoResizeHandles;
        private System.Windows.Forms.Design.Behavior.BehaviorService behaviorService;
        private DesignerControlCollection controls;
        private bool ctrlSelect;
        private static int currentProcessId;
        private CollectionChangeEventHandler dataBindingsCollectionChanged;
        private IDesignerTarget designerTarget;
        private DockingActionList dockingAction;
        private Point downPos = Point.Empty;
        private bool enabledchangerecursionguard;
        private IEventHandlerService eventSvc;
        private bool forceVisible = true;
        private bool hadDragDrop;
        private bool hasLocation;
        private IDesignerHost host;
        private static bool inContextMenu = false;
        private InheritanceUI inheritanceUI;
        private bool inHitTest;
        private bool initializing;
        protected static readonly Point InvalidPoint = new Point(-2147483648, -2147483648);
        private int lastClickMessagePositionX;
        private int lastClickMessagePositionY;
        private int lastClickMessageTime;
        private int lastMoveScreenX;
        private int lastMoveScreenY;
        private bool liveRegion;
        private bool locationChecked;
        private bool locked;
        private Point mouseDragLast = InvalidPoint;
        private bool mouseDragMoved;
        private ContainerSelectorBehavior moveBehavior;
        private IOverlayService overlayService;
        private bool removalNotificationHooked;
        private ResizeBehavior resizeBehavior;
        private bool revokeDragDrop = true;
        private ISelectionUIService selectionUISvc;
        private StatusCommandUI statusCommandUI;
        private Dictionary<IntPtr, bool> subclassedChildren;
        private Exception thrownException;
        private IToolboxService toolboxSvc;
        private bool toolPassThrough;

        private event EventHandler disposingHandler;

        protected void BaseWndProc(ref Message m)
        {
            m.Result = System.Design.NativeMethods.DefWindowProc(m.HWnd, m.Msg, m.WParam, m.LParam);
        }

        internal override bool CanBeAssociatedWith(IDesigner parentDesigner)
        {
            return this.CanBeParentedTo(parentDesigner);
        }

        public virtual bool CanBeParentedTo(IDesigner parentDesigner)
        {
            ParentControlDesigner designer = parentDesigner as ParentControlDesigner;
            return ((designer != null) && !this.Control.Contains(designer.Control));
        }

        private void DataBindingsCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            System.Windows.Forms.Control component = base.Component as System.Windows.Forms.Control;
            if (component != null)
            {
                if ((component.DataBindings.Count == 0) && this.removalNotificationHooked)
                {
                    IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentRemoved -= new ComponentEventHandler(this.DataSource_ComponentRemoved);
                    }
                    this.removalNotificationHooked = false;
                }
                else if ((component.DataBindings.Count > 0) && !this.removalNotificationHooked)
                {
                    IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    if (service2 != null)
                    {
                        service2.ComponentRemoved += new ComponentEventHandler(this.DataSource_ComponentRemoved);
                    }
                    this.removalNotificationHooked = true;
                }
            }
        }

        private void DataSource_ComponentRemoved(object sender, ComponentEventArgs e)
        {
            System.Windows.Forms.Control component = base.Component as System.Windows.Forms.Control;
            if (component != null)
            {
                component.DataBindings.CollectionChanged -= this.dataBindingsCollectionChanged;
                for (int i = 0; i < component.DataBindings.Count; i++)
                {
                    Binding binding = component.DataBindings[i];
                    if (binding.DataSource == e.Component)
                    {
                        component.DataBindings.Remove(binding);
                    }
                }
                if (component.DataBindings.Count == 0)
                {
                    IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentRemoved -= new ComponentEventHandler(this.DataSource_ComponentRemoved);
                    }
                    this.removalNotificationHooked = false;
                }
                component.DataBindings.CollectionChanged += this.dataBindingsCollectionChanged;
            }
        }

        protected void DefWndProc(ref Message m)
        {
            this.designerTarget.DefWndProc(ref m);
        }

        private void DetachContextMenu(object sender, EventArgs e)
        {
            this.ContextMenu = null;
        }

        protected void DisplayError(Exception e)
        {
            IUIService service = (IUIService) this.GetService(typeof(IUIService));
            if (service != null)
            {
                service.ShowError(e);
            }
            else
            {
                string message = e.Message;
                if ((message == null) || (message.Length == 0))
                {
                    message = e.ToString();
                }
                System.Windows.Forms.Design.RTLAwareMessageBox.Show(this.Control, message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Control != null)
                {
                    if (this.dataBindingsCollectionChanged != null)
                    {
                        this.Control.DataBindings.CollectionChanged -= this.dataBindingsCollectionChanged;
                    }
                    if (base.Inherited && (this.inheritanceUI != null))
                    {
                        this.inheritanceUI.RemoveInheritedControl(this.Control);
                    }
                    if (this.removalNotificationHooked)
                    {
                        IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                        if (service != null)
                        {
                            service.ComponentRemoved -= new ComponentEventHandler(this.DataSource_ComponentRemoved);
                        }
                        this.removalNotificationHooked = false;
                    }
                    if (this.disposingHandler != null)
                    {
                        this.disposingHandler(this, EventArgs.Empty);
                    }
                    this.UnhookChildControls(this.Control);
                }
                if (this.ContextMenu != null)
                {
                    this.ContextMenu.Disposed -= new EventHandler(this.DetachContextMenu);
                }
                if (this.designerTarget != null)
                {
                    this.designerTarget.Dispose();
                }
                this.downPos = Point.Empty;
                this.Control.ControlAdded -= new ControlEventHandler(this.OnControlAdded);
                this.Control.ControlRemoved -= new ControlEventHandler(this.OnControlRemoved);
                this.Control.ParentChanged -= new EventHandler(this.OnParentChanged);
                this.Control.SizeChanged -= new EventHandler(this.OnSizeChanged);
                this.Control.LocationChanged -= new EventHandler(this.OnLocationChanged);
                this.Control.EnabledChanged -= new EventHandler(this.OnEnabledChanged);
            }
            base.Dispose(disposing);
        }

        protected bool EnableDesignMode(System.Windows.Forms.Control child, string name)
        {
            if (child == null)
            {
                throw new ArgumentNullException("child");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            INestedContainer service = this.GetService(typeof(INestedContainer)) as INestedContainer;
            if (service == null)
            {
                return false;
            }
            for (int i = 0; i < service.Components.Count; i++)
            {
                if (service.Components[i].Equals(child))
                {
                    return true;
                }
            }
            service.Add(child, name);
            return true;
        }

        protected void EnableDragDrop(bool value)
        {
            System.Windows.Forms.Control control = this.Control;
            if (control != null)
            {
                if (value)
                {
                    control.DragDrop += new DragEventHandler(this.OnDragDrop);
                    control.DragOver += new DragEventHandler(this.OnDragOver);
                    control.DragEnter += new DragEventHandler(this.OnDragEnter);
                    control.DragLeave += new EventHandler(this.OnDragLeave);
                    control.GiveFeedback += new GiveFeedbackEventHandler(this.OnGiveFeedback);
                    this.hadDragDrop = control.AllowDrop;
                    if (!this.hadDragDrop)
                    {
                        control.AllowDrop = true;
                    }
                    this.revokeDragDrop = false;
                }
                else
                {
                    control.DragDrop -= new DragEventHandler(this.OnDragDrop);
                    control.DragOver -= new DragEventHandler(this.OnDragOver);
                    control.DragEnter -= new DragEventHandler(this.OnDragEnter);
                    control.DragLeave -= new EventHandler(this.OnDragLeave);
                    control.GiveFeedback -= new GiveFeedbackEventHandler(this.OnGiveFeedback);
                    if (!this.hadDragDrop)
                    {
                        control.AllowDrop = false;
                    }
                    this.revokeDragDrop = true;
                }
            }
        }

        protected virtual ControlBodyGlyph GetControlGlyph(GlyphSelectionType selectionType)
        {
            this.OnSetCursor();
            Cursor current = Cursor.Current;
            Rectangle bounds = this.BehaviorService.ControlRectInAdornerWindow(this.Control);
            ControlBodyGlyph glyph = null;
            System.Windows.Forms.Control parent = this.Control.Parent;
            if (((parent != null) && (this.host != null)) && (this.host.RootComponent != base.Component))
            {
                Rectangle rectangle2 = parent.RectangleToScreen(parent.ClientRectangle);
                Rectangle rect = this.Control.RectangleToScreen(this.Control.ClientRectangle);
                if (!rectangle2.Contains(rect) && !rectangle2.IntersectsWith(rect))
                {
                    ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                    if ((service != null) && service.GetComponentSelected(this.Control))
                    {
                        glyph = new ControlBodyGlyph(bounds, current, this.Control, this.MoveBehavior);
                    }
                    else if (current == Cursors.SizeAll)
                    {
                        current = Cursors.Default;
                    }
                }
            }
            if (glyph == null)
            {
                glyph = new ControlBodyGlyph(bounds, current, this.Control, this);
            }
            return glyph;
        }

        internal ControlBodyGlyph GetControlGlyphInternal(GlyphSelectionType selectionType)
        {
            return this.GetControlGlyph(selectionType);
        }

        public virtual GlyphCollection GetGlyphs(GlyphSelectionType selectionType)
        {
            GlyphCollection glyphs = new GlyphCollection();
            if (selectionType != GlyphSelectionType.NotSelected)
            {
                Rectangle controlBounds = this.BehaviorService.ControlRectInAdornerWindow(this.Control);
                bool primarySelection = selectionType == GlyphSelectionType.SelectedPrimary;
                System.Windows.Forms.Design.SelectionRules selectionRules = this.SelectionRules;
                if (this.Locked || (this.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
                {
                    glyphs.Add(new LockedHandleGlyph(controlBounds, primarySelection));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Top));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Bottom));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Left));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Right));
                    return glyphs;
                }
                if ((selectionRules & System.Windows.Forms.Design.SelectionRules.AllSizeable) == System.Windows.Forms.Design.SelectionRules.None)
                {
                    glyphs.Add(new NoResizeHandleGlyph(controlBounds, selectionRules, primarySelection, this.MoveBehavior));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Top, this.MoveBehavior));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Bottom, this.MoveBehavior));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Left, this.MoveBehavior));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Right, this.MoveBehavior));
                    if (TypeDescriptor.GetAttributes(base.Component).Contains(DesignTimeVisibleAttribute.Yes) && (this.behaviorService.DesignerActionUI != null))
                    {
                        Glyph designerActionGlyph = this.behaviorService.DesignerActionUI.GetDesignerActionGlyph(base.Component);
                        if (designerActionGlyph != null)
                        {
                            glyphs.Insert(0, designerActionGlyph);
                        }
                    }
                    return glyphs;
                }
                if ((selectionRules & System.Windows.Forms.Design.SelectionRules.TopSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                {
                    glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.MiddleTop, this.StandardBehavior, primarySelection));
                    if ((selectionRules & System.Windows.Forms.Design.SelectionRules.LeftSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                    {
                        glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.UpperLeft, this.StandardBehavior, primarySelection));
                    }
                    if ((selectionRules & System.Windows.Forms.Design.SelectionRules.RightSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                    {
                        glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.UpperRight, this.StandardBehavior, primarySelection));
                    }
                }
                if ((selectionRules & System.Windows.Forms.Design.SelectionRules.BottomSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                {
                    glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.MiddleBottom, this.StandardBehavior, primarySelection));
                    if ((selectionRules & System.Windows.Forms.Design.SelectionRules.LeftSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                    {
                        glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.LowerLeft, this.StandardBehavior, primarySelection));
                    }
                    if ((selectionRules & System.Windows.Forms.Design.SelectionRules.RightSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                    {
                        glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.LowerRight, this.StandardBehavior, primarySelection));
                    }
                }
                if ((selectionRules & System.Windows.Forms.Design.SelectionRules.LeftSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                {
                    glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.MiddleLeft, this.StandardBehavior, primarySelection));
                }
                if ((selectionRules & System.Windows.Forms.Design.SelectionRules.RightSizeable) != System.Windows.Forms.Design.SelectionRules.None)
                {
                    glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.MiddleRight, this.StandardBehavior, primarySelection));
                }
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Top, this.StandardBehavior));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Bottom, this.StandardBehavior));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Left, this.StandardBehavior));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Right, this.StandardBehavior));
                if (TypeDescriptor.GetAttributes(base.Component).Contains(DesignTimeVisibleAttribute.Yes) && (this.behaviorService.DesignerActionUI != null))
                {
                    Glyph glyph2 = this.behaviorService.DesignerActionUI.GetDesignerActionGlyph(base.Component);
                    if (glyph2 != null)
                    {
                        glyphs.Insert(0, glyph2);
                    }
                }
            }
            return glyphs;
        }

        protected virtual bool GetHitTest(Point point)
        {
            return false;
        }

        internal Point GetOffsetToClientArea()
        {
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(0, 0);
            System.Design.NativeMethods.MapWindowPoints(this.Control.Handle, this.Control.Parent.Handle, pt, 1);
            Point location = this.Control.Location;
            if (this.Control.IsMirrored != this.Control.Parent.IsMirrored)
            {
                location.Offset(this.Control.Width, 0);
            }
            return new Point(Math.Abs((int) (pt.x - location.X)), pt.y - location.Y);
        }

        private int GetParentPointFromLparam(IntPtr lParam)
        {
            Point p = new Point(System.Design.NativeMethods.Util.SignedLOWORD((int) ((long) lParam)), System.Design.NativeMethods.Util.SignedHIWORD((int) ((long) lParam)));
            p = this.Control.PointToScreen(p);
            p = this.Control.Parent.PointToClient(p);
            return System.Design.NativeMethods.Util.MAKELONG(p.X, p.Y);
        }

        protected void HookChildControls(System.Windows.Forms.Control firstChild)
        {
            foreach (System.Windows.Forms.Control control in firstChild.Controls)
            {
                if (((control != null) && (this.host != null)) && !(this.host.GetDesigner(control) is ControlDesigner))
                {
                    IWindowTarget windowTarget = control.WindowTarget;
                    if (!(windowTarget is ChildWindowTarget))
                    {
                        control.WindowTarget = new ChildWindowTarget(this, control, windowTarget);
                        control.ControlAdded += new ControlEventHandler(this.OnControlAdded);
                    }
                    if (control.IsHandleCreated)
                    {
                        Application.OleRequired();
                        System.Design.NativeMethods.RevokeDragDrop(control.Handle);
                        this.HookChildHandles(control.Handle);
                    }
                    else
                    {
                        control.HandleCreated += new EventHandler(this.OnChildHandleCreated);
                    }
                    this.HookChildControls(control);
                }
            }
        }

        internal void HookChildHandles(IntPtr firstChild)
        {
            for (IntPtr ptr = firstChild; ptr != IntPtr.Zero; ptr = System.Design.NativeMethods.GetWindow(ptr, 2))
            {
                if (!this.IsWindowInCurrentProcess(ptr))
                {
                    return;
                }
                System.Windows.Forms.Control control = System.Windows.Forms.Control.FromHandle(ptr);
                if ((control == null) && !this.SubclassedChildWindows.ContainsKey(ptr))
                {
                    System.Design.NativeMethods.RevokeDragDrop(ptr);
                    new ChildSubClass(this, ptr);
                    this.SubclassedChildWindows[ptr] = true;
                }
                if ((control == null) || (this.Control is UserControl))
                {
                    this.HookChildHandles(System.Design.NativeMethods.GetWindow(ptr, 5));
                }
            }
        }

        public override void Initialize(IComponent component)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component.GetType());
            PropertyDescriptor descriptor = properties["Visible"];
            if (((descriptor == null) || (descriptor.PropertyType != typeof(bool))) || !descriptor.ShouldSerializeValue(component))
            {
                this.Visible = true;
            }
            else
            {
                this.Visible = (bool) descriptor.GetValue(component);
            }
            PropertyDescriptor descriptor2 = properties["Enabled"];
            if (((descriptor2 == null) || (descriptor2.PropertyType != typeof(bool))) || !descriptor2.ShouldSerializeValue(component))
            {
                this.Enabled = true;
            }
            else
            {
                this.Enabled = (bool) descriptor2.GetValue(component);
            }
            this.initializing = true;
            base.Initialize(component);
            this.initializing = false;
            this.host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            DockingAttribute attribute = (DockingAttribute) TypeDescriptor.GetAttributes(base.Component)[typeof(DockingAttribute)];
            if ((attribute != null) && (attribute.DockingBehavior != DockingBehavior.Never))
            {
                this.dockingAction = new DockingActionList(this);
                DesignerActionService service = this.GetService(typeof(DesignerActionService)) as DesignerActionService;
                if (service != null)
                {
                    service.Add(base.Component, this.dockingAction);
                }
            }
            this.dataBindingsCollectionChanged = new CollectionChangeEventHandler(this.DataBindingsCollectionChanged);
            this.Control.DataBindings.CollectionChanged += this.dataBindingsCollectionChanged;
            this.Control.ControlAdded += new ControlEventHandler(this.OnControlAdded);
            this.Control.ControlRemoved += new ControlEventHandler(this.OnControlRemoved);
            this.Control.ParentChanged += new EventHandler(this.OnParentChanged);
            this.Control.SizeChanged += new EventHandler(this.OnSizeChanged);
            this.Control.LocationChanged += new EventHandler(this.OnLocationChanged);
            this.DesignerTarget = new DesignerWindowTarget(this);
            if (this.Control.IsHandleCreated)
            {
                this.OnCreateHandle();
            }
            if ((base.Inherited && (this.host != null)) && (this.host.RootComponent != component))
            {
                this.inheritanceUI = (InheritanceUI) this.GetService(typeof(InheritanceUI));
                if (this.inheritanceUI != null)
                {
                    this.inheritanceUI.AddInheritedControl(this.Control, this.InheritanceAttribute.InheritanceLevel);
                }
            }
            if (((this.host == null) || (this.host.RootComponent != component)) && this.ForceVisible)
            {
                this.Control.Visible = true;
            }
            this.Control.Enabled = true;
            this.Control.EnabledChanged += new EventHandler(this.OnEnabledChanged);
            this.AllowDrop = this.Control.AllowDrop;
            this.statusCommandUI = new StatusCommandUI(component.Site);
        }

        public override void InitializeExistingComponent(IDictionary defaultValues)
        {
            base.InitializeExistingComponent(defaultValues);
            foreach (System.Windows.Forms.Control control in this.Control.Controls)
            {
                if (control != null)
                {
                    ISite site = control.Site;
                    ChildWindowTarget windowTarget = control.WindowTarget as ChildWindowTarget;
                    if ((site != null) && (windowTarget != null))
                    {
                        control.WindowTarget = windowTarget.OldWindowTarget;
                    }
                }
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            ISite site = base.Component.Site;
            if (site != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Text"];
                if (((descriptor != null) && (descriptor.PropertyType == typeof(string))) && (!descriptor.IsReadOnly && descriptor.IsBrowsable))
                {
                    descriptor.SetValue(base.Component, site.Name);
                }
            }
            if (defaultValues != null)
            {
                IComponent component = defaultValues["Parent"] as IComponent;
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((component != null) && (service != null))
                {
                    ParentControlDesigner designer = service.GetDesigner(component) as ParentControlDesigner;
                    if (designer != null)
                    {
                        designer.AddControl(this.Control, defaultValues);
                    }
                    System.Windows.Forms.Control control = component as System.Windows.Forms.Control;
                    if (control != null)
                    {
                        DockingAttribute attribute = (DockingAttribute) TypeDescriptor.GetAttributes(base.Component)[typeof(DockingAttribute)];
                        if (((attribute != null) && (attribute.DockingBehavior != DockingBehavior.Never)) && (attribute.DockingBehavior == DockingBehavior.AutoDock))
                        {
                            bool flag = true;
                            foreach (System.Windows.Forms.Control control2 in control.Controls)
                            {
                                if ((control2 != this.Control) && (control2.Dock == DockStyle.None))
                                {
                                    flag = false;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(base.Component)["Dock"];
                                if ((descriptor2 != null) && descriptor2.IsBrowsable)
                                {
                                    descriptor2.SetValue(base.Component, DockStyle.Fill);
                                }
                            }
                        }
                    }
                }
            }
            base.InitializeNewComponent(defaultValues);
        }

        public virtual ControlDesigner InternalControlDesigner(int internalControlIndex)
        {
            return null;
        }

        private bool IsDoubleClick(int x, int y)
        {
            bool flag = false;
            int doubleClickTime = SystemInformation.DoubleClickTime;
            int num2 = System.Design.SafeNativeMethods.GetTickCount() - this.lastClickMessageTime;
            if (num2 <= doubleClickTime)
            {
                Size doubleClickSize = SystemInformation.DoubleClickSize;
                if (((x >= (this.lastClickMessagePositionX - doubleClickSize.Width)) && (x <= (this.lastClickMessagePositionX + doubleClickSize.Width))) && ((y >= (this.lastClickMessagePositionY - doubleClickSize.Height)) && (y <= (this.lastClickMessagePositionY + doubleClickSize.Height))))
                {
                    flag = true;
                }
            }
            if (!flag)
            {
                this.lastClickMessagePositionX = x;
                this.lastClickMessagePositionY = y;
                this.lastClickMessageTime = System.Design.SafeNativeMethods.GetTickCount();
                return flag;
            }
            this.lastClickMessagePositionX = this.lastClickMessagePositionY = 0;
            this.lastClickMessageTime = 0;
            return flag;
        }

        private bool IsMouseMessage(int msg)
        {
            if ((msg >= 0x200) && (msg <= 0x20a))
            {
                return true;
            }
            switch (msg)
            {
                case 160:
                case 0xa1:
                case 0xa2:
                case 0xa3:
                case 0xa4:
                case 0xa5:
                case 0xa6:
                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0xab:
                case 0xac:
                case 0xad:
                case 0x2a0:
                case 0x2a1:
                case 0x2a2:
                case 0x2a3:
                    return true;
            }
            return false;
        }

        private bool IsResizableConsiderAutoSize(PropertyDescriptor autoSizeProp, PropertyDescriptor autoSizeModeProp)
        {
            object component = base.Component;
            bool flag = true;
            bool flag2 = false;
            bool flag3 = false;
            if (((autoSizeProp != null) && !autoSizeProp.Attributes.Contains(DesignerSerializationVisibilityAttribute.Hidden)) && !autoSizeProp.Attributes.Contains(BrowsableAttribute.No))
            {
                flag2 = (bool) autoSizeProp.GetValue(component);
            }
            if (autoSizeModeProp != null)
            {
                AutoSizeMode mode = (AutoSizeMode) autoSizeModeProp.GetValue(component);
                flag3 = mode == AutoSizeMode.GrowOnly;
            }
            if (flag2)
            {
                flag = flag3;
            }
            return flag;
        }

        private bool IsWindowInCurrentProcess(IntPtr hwnd)
        {
            int num;
            System.Design.UnsafeNativeMethods.GetWindowThreadProcessId(new HandleRef(null, hwnd), out num);
            return (num == this.CurrentProcessId);
        }

        public virtual int NumberOfInternalControlDesigners()
        {
            return 0;
        }

        private void OnChildHandleCreated(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control = sender as System.Windows.Forms.Control;
            if (control != null)
            {
                this.HookChildHandles(control.Handle);
            }
        }

        protected virtual void OnContextMenu(int x, int y)
        {
            this.ShowContextMenu(x, y);
        }

        private void OnControlAdded(object sender, ControlEventArgs e)
        {
            if (((e.Control != null) && (this.host != null)) && !(this.host.GetDesigner(e.Control) is ControlDesigner))
            {
                IWindowTarget windowTarget = e.Control.WindowTarget;
                if (!(windowTarget is ChildWindowTarget))
                {
                    e.Control.WindowTarget = new ChildWindowTarget(this, e.Control, windowTarget);
                    e.Control.ControlAdded += new ControlEventHandler(this.OnControlAdded);
                }
                if (e.Control.IsHandleCreated)
                {
                    Application.OleRequired();
                    System.Design.NativeMethods.RevokeDragDrop(e.Control.Handle);
                    this.HookChildControls(e.Control);
                }
            }
        }

        private void OnControlRemoved(object sender, ControlEventArgs e)
        {
            if (e.Control != null)
            {
                ChildWindowTarget windowTarget = e.Control.WindowTarget as ChildWindowTarget;
                if (windowTarget != null)
                {
                    e.Control.WindowTarget = windowTarget.OldWindowTarget;
                }
                this.UnhookChildControls(e.Control);
            }
        }

        protected virtual void OnCreateHandle()
        {
            this.OnHandleChange();
            if (this.revokeDragDrop)
            {
                System.Design.NativeMethods.RevokeDragDrop(this.Control.Handle);
            }
        }

        protected virtual void OnDragComplete(DragEventArgs de)
        {
        }

        protected virtual void OnDragDrop(DragEventArgs de)
        {
            System.Windows.Forms.Control control = this.Control;
            DragEventHandler handler = new DragEventHandler(this.OnDragDrop);
            control.DragDrop -= handler;
            ((IDropTarget) this.Control).OnDragDrop(de);
            control.DragDrop += handler;
            this.OnDragComplete(de);
        }

        private void OnDragDrop(object s, DragEventArgs e)
        {
            if (this.BehaviorService != null)
            {
                this.BehaviorService.EndDragNotification();
            }
            this.OnDragDrop(e);
        }

        protected virtual void OnDragEnter(DragEventArgs de)
        {
            System.Windows.Forms.Control control = this.Control;
            DragEventHandler handler = new DragEventHandler(this.OnDragEnter);
            control.DragEnter -= handler;
            ((IDropTarget) this.Control).OnDragEnter(de);
            control.DragEnter += handler;
        }

        private void OnDragEnter(object s, DragEventArgs e)
        {
            if (this.BehaviorService != null)
            {
                this.BehaviorService.StartDragNotification();
            }
            this.OnDragEnter(e);
        }

        protected virtual void OnDragLeave(EventArgs e)
        {
            System.Windows.Forms.Control control = this.Control;
            EventHandler handler = new EventHandler(this.OnDragLeave);
            control.DragLeave -= handler;
            ((IDropTarget) this.Control).OnDragLeave(e);
            control.DragLeave += handler;
        }

        private void OnDragLeave(object s, EventArgs e)
        {
            this.OnDragLeave(e);
        }

        protected virtual void OnDragOver(DragEventArgs de)
        {
            System.Windows.Forms.Control control = this.Control;
            DragEventHandler handler = new DragEventHandler(this.OnDragOver);
            control.DragOver -= handler;
            ((IDropTarget) this.Control).OnDragOver(de);
            control.DragOver += handler;
        }

        private void OnDragOver(object s, DragEventArgs e)
        {
            this.OnDragOver(e);
        }

        private void OnEnabledChanged(object sender, EventArgs e)
        {
            if (!this.enabledchangerecursionguard)
            {
                this.enabledchangerecursionguard = true;
                try
                {
                    this.Control.Enabled = true;
                }
                finally
                {
                    this.enabledchangerecursionguard = false;
                }
            }
        }

        protected virtual void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
        }

        private void OnGiveFeedback(object s, GiveFeedbackEventArgs e)
        {
            this.OnGiveFeedback(e);
        }

        private void OnHandleChange()
        {
            this.HookChildHandles(System.Design.NativeMethods.GetWindow(this.Control.Handle, 5));
            this.HookChildControls(this.Control);
        }

        private void OnLocationChanged(object sender, EventArgs e)
        {
            ComponentCache service = (ComponentCache) this.GetService(typeof(ComponentCache));
            object component = base.Component;
            if ((service != null) && (component != null))
            {
                service.RemoveEntry(component);
            }
        }

        private void OnMouseDoubleClick()
        {
            try
            {
                this.DoDefaultAction();
            }
            catch (Exception exception)
            {
                this.DisplayError(exception);
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
        }

        protected virtual void OnMouseDragBegin(int x, int y)
        {
            if ((this.BehaviorService != null) || (this.mouseDragLast == InvalidPoint))
            {
                this.mouseDragLast = new Point(x, y);
                this.ctrlSelect = (System.Windows.Forms.Control.ModifierKeys & Keys.Control) != Keys.None;
                ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (!this.ctrlSelect && (service != null))
                {
                    service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Click);
                }
                this.Control.Capture = true;
            }
        }

        protected virtual void OnMouseDragEnd(bool cancel)
        {
            this.mouseDragLast = InvalidPoint;
            this.Control.Capture = false;
            if (!this.mouseDragMoved)
            {
                if (!cancel)
                {
                    ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                    if (((System.Windows.Forms.Control.ModifierKeys & Keys.Shift) == Keys.None) && (this.ctrlSelect || ((service != null) && !service.GetComponentSelected(base.Component))))
                    {
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Click);
                        }
                        this.ctrlSelect = false;
                    }
                }
            }
            else
            {
                this.mouseDragMoved = false;
                this.ctrlSelect = false;
                if (((this.BehaviorService != null) && this.BehaviorService.Dragging) && cancel)
                {
                    this.BehaviorService.CancelDrag = true;
                }
                if (this.selectionUISvc == null)
                {
                    this.selectionUISvc = (ISelectionUIService) this.GetService(typeof(ISelectionUIService));
                }
                if ((this.selectionUISvc != null) && this.selectionUISvc.Dragging)
                {
                    this.selectionUISvc.EndDrag(cancel);
                }
            }
        }

        protected virtual void OnMouseDragMove(int x, int y)
        {
            if (!this.mouseDragMoved)
            {
                Size dragSize = SystemInformation.DragSize;
                Size doubleClickSize = SystemInformation.DoubleClickSize;
                dragSize.Width = Math.Max(dragSize.Width, doubleClickSize.Width);
                dragSize.Height = Math.Max(dragSize.Height, doubleClickSize.Height);
                if ((this.mouseDragLast == InvalidPoint) || ((Math.Abs((int) (this.mouseDragLast.X - x)) < dragSize.Width) && (Math.Abs((int) (this.mouseDragLast.Y - y)) < dragSize.Height)))
                {
                    return;
                }
                this.mouseDragMoved = true;
                this.ctrlSelect = false;
            }
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if ((service != null) && !base.Component.Equals(service.PrimarySelection))
            {
                service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Toggle | SelectionTypes.Click);
            }
            if ((this.BehaviorService != null) && (service != null))
            {
                ArrayList dragComponents = new ArrayList();
                ICollection selectedComponents = service.GetSelectedComponents();
                System.Windows.Forms.Control parent = null;
                foreach (IComponent component in selectedComponents)
                {
                    System.Windows.Forms.Control control2 = component as System.Windows.Forms.Control;
                    if (control2 != null)
                    {
                        if (parent == null)
                        {
                            parent = control2.Parent;
                        }
                        else if (!parent.Equals(control2.Parent))
                        {
                            continue;
                        }
                        ControlDesigner designer = this.host.GetDesigner(component) as ControlDesigner;
                        if ((designer != null) && ((designer.SelectionRules & System.Windows.Forms.Design.SelectionRules.Moveable) != System.Windows.Forms.Design.SelectionRules.None))
                        {
                            dragComponents.Add(component);
                        }
                    }
                }
                if (dragComponents.Count > 0)
                {
                    using (this.BehaviorService.AdornerWindowGraphics)
                    {
                        DropSourceBehavior dropSourceBehavior = new DropSourceBehavior(dragComponents, this.Control.Parent, this.mouseDragLast);
                        this.BehaviorService.DoDragDrop(dropSourceBehavior);
                    }
                }
            }
            this.mouseDragLast = InvalidPoint;
            this.mouseDragMoved = false;
        }

        protected virtual void OnMouseEnter()
        {
            System.Windows.Forms.Control control = this.Control;
            object obj2 = null;
            while ((obj2 == null) && (control != null))
            {
                control = control.Parent;
                if (control != null)
                {
                    object obj3 = this.host.GetDesigner(control);
                    if (obj3 != this)
                    {
                        obj2 = obj3;
                    }
                }
            }
            ControlDesigner designer = obj2 as ControlDesigner;
            if (designer != null)
            {
                designer.OnMouseEnter();
            }
        }

        protected virtual void OnMouseHover()
        {
            System.Windows.Forms.Control control = this.Control;
            object obj2 = null;
            while ((obj2 == null) && (control != null))
            {
                control = control.Parent;
                if (control != null)
                {
                    object obj3 = this.host.GetDesigner(control);
                    if (obj3 != this)
                    {
                        obj2 = obj3;
                    }
                }
            }
            ControlDesigner designer = obj2 as ControlDesigner;
            if (designer != null)
            {
                designer.OnMouseHover();
            }
        }

        protected virtual void OnMouseLeave()
        {
            System.Windows.Forms.Control control = this.Control;
            object obj2 = null;
            while ((obj2 == null) && (control != null))
            {
                control = control.Parent;
                if (control != null)
                {
                    object obj3 = this.host.GetDesigner(control);
                    if (obj3 != this)
                    {
                        obj2 = obj3;
                    }
                }
            }
            ControlDesigner designer = obj2 as ControlDesigner;
            if (designer != null)
            {
                designer.OnMouseLeave();
            }
        }

        protected virtual void OnPaintAdornments(PaintEventArgs pe)
        {
            if ((this.inheritanceUI != null) && pe.ClipRectangle.IntersectsWith(this.inheritanceUI.InheritanceGlyphRectangle))
            {
                pe.Graphics.DrawImage(this.inheritanceUI.InheritanceGlyph, 0, 0);
            }
        }

        private void OnParentChanged(object sender, EventArgs e)
        {
            if (this.Control.IsHandleCreated)
            {
                this.OnHandleChange();
            }
        }

        [Obsolete("This method has been deprecated. Use InitializeNewComponent instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public override void OnSetComponentDefaults()
        {
            ISite site = base.Component.Site;
            if (site != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Text"];
                if ((descriptor != null) && descriptor.IsBrowsable)
                {
                    descriptor.SetValue(base.Component, site.Name);
                }
            }
        }

        protected virtual void OnSetCursor()
        {
            if (this.Control.Dock != DockStyle.None)
            {
                Cursor.Current = Cursors.Default;
            }
            else
            {
                if (this.toolboxSvc == null)
                {
                    this.toolboxSvc = (IToolboxService) this.GetService(typeof(IToolboxService));
                }
                if ((this.toolboxSvc == null) || !this.toolboxSvc.SetCursor())
                {
                    if (!this.locationChecked)
                    {
                        this.locationChecked = true;
                        try
                        {
                            this.hasLocation = TypeDescriptor.GetProperties(base.Component)["Location"] != null;
                        }
                        catch
                        {
                        }
                    }
                    if (!this.hasLocation)
                    {
                        Cursor.Current = Cursors.Default;
                    }
                    else if (this.Locked)
                    {
                        Cursor.Current = Cursors.Default;
                    }
                    else
                    {
                        Cursor.Current = Cursors.SizeAll;
                    }
                }
            }
        }

        private void OnSizeChanged(object sender, EventArgs e)
        {
            ComponentCache service = (ComponentCache) this.GetService(typeof(ComponentCache));
            object component = base.Component;
            if ((service != null) && (component != null))
            {
                service.RemoveEntry(component);
            }
        }

        private void PaintException(PaintEventArgs e, Exception ex)
        {
            StringFormat stringFormat = new StringFormat {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near
            };
            string text = ex.ToString();
            CharacterRange[] ranges = new CharacterRange[] { new CharacterRange(0, text.Length) };
            stringFormat.SetMeasurableCharacterRanges(ranges);
            int num = 2;
            Size iconSize = SystemInformation.IconSize;
            int x = num * 2;
            int y = num * 2;
            Rectangle clientRectangle = this.Control.ClientRectangle;
            Rectangle rect = clientRectangle;
            rect.X++;
            rect.Y++;
            rect.Width -= 2;
            rect.Height -= 2;
            Rectangle rectangle3 = new Rectangle(x, y, iconSize.Width, iconSize.Height);
            Rectangle layoutRect = clientRectangle;
            layoutRect.X = (rectangle3.X + rectangle3.Width) + (2 * x);
            layoutRect.Y = rectangle3.Y;
            layoutRect.Width -= (layoutRect.X + x) + num;
            layoutRect.Height -= (layoutRect.Y + y) + num;
            using (Font font = new Font(this.Control.Font.FontFamily, (float) Math.Max((SystemInformation.ToolWindowCaptionHeight - SystemInformation.BorderSize.Height) - 2, this.Control.Font.Height), GraphicsUnit.Pixel))
            {
                using (Region region = e.Graphics.MeasureCharacterRanges(text, font, layoutRect, stringFormat)[0])
                {
                    Region clip = e.Graphics.Clip;
                    e.Graphics.ExcludeClip(region);
                    e.Graphics.ExcludeClip(rectangle3);
                    try
                    {
                        e.Graphics.FillRectangle(Brushes.White, clientRectangle);
                    }
                    finally
                    {
                        e.Graphics.Clip = clip;
                    }
                    using (Pen pen = new Pen(Color.Red, (float) num))
                    {
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                    Icon error = SystemIcons.Error;
                    e.Graphics.FillRectangle(Brushes.White, rectangle3);
                    e.Graphics.DrawIcon(error, rectangle3.X, rectangle3.Y);
                    layoutRect.X++;
                    e.Graphics.IntersectClip(region);
                    try
                    {
                        e.Graphics.FillRectangle(Brushes.White, layoutRect);
                        e.Graphics.DrawString(text, font, new SolidBrush(this.Control.ForeColor), layoutRect, stringFormat);
                    }
                    finally
                    {
                        e.Graphics.Clip = clip;
                    }
                }
            }
            stringFormat.Dispose();
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Visible", "Enabled", "ContextMenu", "AllowDrop", "Location", "Name" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ControlDesigner), oldPropertyDescriptor, attributes);
                }
            }
            PropertyDescriptor descriptor2 = (PropertyDescriptor) properties["Controls"];
            if (descriptor2 != null)
            {
                Attribute[] array = new Attribute[descriptor2.Attributes.Count];
                descriptor2.Attributes.CopyTo(array, 0);
                properties["Controls"] = TypeDescriptor.CreateProperty(typeof(ControlDesigner), "Controls", typeof(DesignerControlCollection), array);
            }
            PropertyDescriptor pd = (PropertyDescriptor) properties["Size"];
            if (pd != null)
            {
                properties["Size"] = new CanResetSizePropertyDescriptor(pd);
            }
            properties["Locked"] = TypeDescriptor.CreateProperty(typeof(ControlDesigner), "Locked", typeof(bool), new Attribute[] { new DefaultValueAttribute(false), BrowsableAttribute.Yes, CategoryAttribute.Design, DesignOnlyAttribute.Yes, new System.Design.SRDescriptionAttribute("lockedDescr") });
        }

        internal void RemoveSubclassedWindow(IntPtr hwnd)
        {
            if (this.SubclassedChildWindows.ContainsKey(hwnd))
            {
                this.SubclassedChildWindows.Remove(hwnd);
            }
        }

        private void ResetEnabled()
        {
            this.Enabled = true;
        }

        private void ResetVisible()
        {
            this.Visible = true;
        }

        internal void SetUnhandledException(System.Windows.Forms.Control owner, Exception exception)
        {
            if (this.thrownException == null)
            {
                this.thrownException = exception;
                if (owner == null)
                {
                    owner = this.Control;
                }
                string str = string.Empty;
                string[] strArray = exception.StackTrace.Split(new char[] { '\r', '\n' });
                string fullName = owner.GetType().FullName;
                foreach (string str3 in strArray)
                {
                    if (str3.IndexOf(fullName) != -1)
                    {
                        str = string.Format(CultureInfo.CurrentCulture, "{0}\r\n{1}", new object[] { str, str3 });
                    }
                }
                Exception e = new Exception(System.Design.SR.GetString("ControlDesigner_WndProcException", new object[] { fullName, exception.Message, str }), exception);
                this.DisplayError(e);
                foreach (System.Windows.Forms.Control control in this.Control.Controls)
                {
                    control.Visible = false;
                }
                this.Control.Invalidate(true);
            }
        }

        private bool ShouldSerializeAllowDrop()
        {
            return (this.AllowDrop != this.hadDragDrop);
        }

        private bool ShouldSerializeEnabled()
        {
            return base.ShadowProperties.ShouldSerializeValue("Enabled", true);
        }

        private bool ShouldSerializeName()
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (!this.initializing)
            {
                return base.ShadowProperties.ShouldSerializeValue("Name", null);
            }
            return (base.Component != service.RootComponent);
        }

        private bool ShouldSerializeVisible()
        {
            return base.ShadowProperties.ShouldSerializeValue("Visible", true);
        }

        internal IList SnapLinesInternal()
        {
            return this.SnapLinesInternal(this.Control.Margin);
        }

        internal IList SnapLinesInternal(Padding margin)
        {
            ArrayList list = new ArrayList(4);
            int width = this.Control.Width;
            int height = this.Control.Height;
            list.Add(new SnapLine(SnapLineType.Top, 0, SnapLinePriority.Low));
            list.Add(new SnapLine(SnapLineType.Bottom, height - 1, SnapLinePriority.Low));
            list.Add(new SnapLine(SnapLineType.Left, 0, SnapLinePriority.Low));
            list.Add(new SnapLine(SnapLineType.Right, width - 1, SnapLinePriority.Low));
            list.Add(new SnapLine(SnapLineType.Horizontal, -margin.Top, "Margin.Top", SnapLinePriority.Always));
            list.Add(new SnapLine(SnapLineType.Horizontal, margin.Bottom + height, "Margin.Bottom", SnapLinePriority.Always));
            list.Add(new SnapLine(SnapLineType.Vertical, -margin.Left, "Margin.Left", SnapLinePriority.Always));
            list.Add(new SnapLine(SnapLineType.Vertical, margin.Right + width, "Margin.Right", SnapLinePriority.Always));
            return list;
        }

        protected void UnhookChildControls(System.Windows.Forms.Control firstChild)
        {
            if (this.host == null)
            {
                this.host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            }
            foreach (System.Windows.Forms.Control control in firstChild.Controls)
            {
                IWindowTarget windowTarget = null;
                if (control != null)
                {
                    windowTarget = control.WindowTarget;
                    ChildWindowTarget target2 = windowTarget as ChildWindowTarget;
                    if (target2 != null)
                    {
                        control.WindowTarget = target2.OldWindowTarget;
                    }
                }
                if (!(windowTarget is DesignerWindowTarget))
                {
                    this.UnhookChildControls(control);
                }
            }
        }

        protected virtual void WndProc(ref Message m)
        {
            IMouseHandler handler = null;
            if ((m.Msg == 0x84) && !this.inHitTest)
            {
                this.inHitTest = true;
                Point point = new Point((short) System.Design.NativeMethods.Util.LOWORD((int) ((long) m.LParam)), (short) System.Design.NativeMethods.Util.HIWORD((int) ((long) m.LParam)));
                try
                {
                    this.liveRegion = this.GetHitTest(point);
                }
                catch (Exception exception)
                {
                    this.liveRegion = false;
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
                this.inHitTest = false;
            }
            bool flag = m.Msg == 0x7b;
            if (this.liveRegion && (this.IsMouseMessage(m.Msg) || flag))
            {
                if (m.Msg == 0x7b)
                {
                    inContextMenu = true;
                }
                try
                {
                    this.DefWndProc(ref m);
                }
                finally
                {
                    if (m.Msg == 0x7b)
                    {
                        inContextMenu = false;
                    }
                    if (m.Msg == 0x202)
                    {
                        this.OnMouseDragEnd(true);
                    }
                }
                return;
            }
            int x = 0;
            int y = 0;
            if ((((m.Msg >= 0x200) && (m.Msg <= 0x20a)) || ((m.Msg >= 160) && (m.Msg <= 0xa9))) || (m.Msg == 0x20))
            {
                if (this.eventSvc == null)
                {
                    this.eventSvc = (IEventHandlerService) this.GetService(typeof(IEventHandlerService));
                }
                if (this.eventSvc != null)
                {
                    handler = (IMouseHandler) this.eventSvc.GetHandler(typeof(IMouseHandler));
                }
            }
            if ((m.Msg >= 0x200) && (m.Msg <= 0x20a))
            {
                System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                    x = System.Design.NativeMethods.Util.SignedLOWORD((int) ((long) m.LParam)),
                    y = System.Design.NativeMethods.Util.SignedHIWORD((int) ((long) m.LParam))
                };
                System.Design.NativeMethods.MapWindowPoints(m.HWnd, IntPtr.Zero, pt, 1);
                x = pt.x;
                y = pt.y;
            }
            else if ((m.Msg >= 160) && (m.Msg <= 0xa9))
            {
                x = System.Design.NativeMethods.Util.SignedLOWORD((int) ((long) m.LParam));
                y = System.Design.NativeMethods.Util.SignedHIWORD((int) ((long) m.LParam));
            }
            MouseButtons none = MouseButtons.None;
            switch (m.Msg)
            {
                case 0x1f:
                    this.OnMouseDragEnd(true);
                    this.DefWndProc(ref m);
                    return;

                case 0x20:
                    goto Label_0A82;

                case 0x3d:
                    if (-4 == ((int) ((long) m.LParam)))
                    {
                        Guid refiid = new Guid("{618736E0-3C3D-11CF-810C-00AA00389B71}");
                        try
                        {
                            IAccessible accessibilityObject = this.AccessibilityObject;
                            if (accessibilityObject == null)
                            {
                                m.Result = IntPtr.Zero;
                            }
                            else
                            {
                                IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(accessibilityObject);
                                try
                                {
                                    m.Result = System.Design.UnsafeNativeMethods.LresultFromObject(ref refiid, m.WParam, iUnknownForObject);
                                }
                                finally
                                {
                                    Marshal.Release(iUnknownForObject);
                                }
                            }
                            return;
                        }
                        catch (Exception exception2)
                        {
                            throw exception2;
                        }
                    }
                    this.DefWndProc(ref m);
                    return;

                case 15:
                    if (OleDragDropHandler.FreezePainting)
                    {
                        System.Design.NativeMethods.ValidateRect(m.HWnd, IntPtr.Zero);
                        return;
                    }
                    if (this.Control != null)
                    {
                        System.Design.NativeMethods.RECT rc = new System.Design.NativeMethods.RECT();
                        IntPtr hrgn = System.Design.NativeMethods.CreateRectRgn(0, 0, 0, 0);
                        System.Design.NativeMethods.GetUpdateRgn(m.HWnd, hrgn, false);
                        System.Design.NativeMethods.GetUpdateRect(m.HWnd, ref rc, false);
                        Region region = Region.FromHrgn(hrgn);
                        Rectangle empty = Rectangle.Empty;
                        try
                        {
                            if (this.thrownException == null)
                            {
                                this.DefWndProc(ref m);
                            }
                            using (Graphics graphics2 = Graphics.FromHwnd(m.HWnd))
                            {
                                if (m.HWnd != this.Control.Handle)
                                {
                                    System.Design.NativeMethods.POINT point3 = new System.Design.NativeMethods.POINT {
                                        x = 0,
                                        y = 0
                                    };
                                    System.Design.NativeMethods.MapWindowPoints(m.HWnd, this.Control.Handle, point3, 1);
                                    graphics2.TranslateTransform((float) -point3.x, (float) -point3.y);
                                    System.Design.NativeMethods.MapWindowPoints(m.HWnd, this.Control.Handle, ref rc, 2);
                                }
                                empty = new Rectangle(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
                                using (PaintEventArgs args2 = new PaintEventArgs(graphics2, empty))
                                {
                                    graphics2.Clip = region;
                                    if (this.thrownException == null)
                                    {
                                        this.OnPaintAdornments(args2);
                                    }
                                    else
                                    {
                                        System.Design.UnsafeNativeMethods.PAINTSTRUCT lpPaint = new System.Design.UnsafeNativeMethods.PAINTSTRUCT();
                                        System.Design.UnsafeNativeMethods.BeginPaint(m.HWnd, ref lpPaint);
                                        this.PaintException(args2, this.thrownException);
                                        System.Design.UnsafeNativeMethods.EndPaint(m.HWnd, ref lpPaint);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            region.Dispose();
                            System.Design.NativeMethods.DeleteObject(hrgn);
                        }
                        if (this.OverlayService == null)
                        {
                            return;
                        }
                        empty.Location = this.Control.PointToScreen(empty.Location);
                        this.OverlayService.InvalidateOverlays(empty);
                    }
                    return;

                case 5:
                    if (this.thrownException != null)
                    {
                        this.Control.Invalidate();
                    }
                    this.DefWndProc(ref m);
                    return;

                case 7:
                    if ((this.host != null) && (this.host.RootComponent != null))
                    {
                        IRootDesigner designer = this.host.GetDesigner(this.host.RootComponent) as IRootDesigner;
                        if (designer == null)
                        {
                            return;
                        }
                        ViewTechnology[] supportedTechnologies = designer.SupportedTechnologies;
                        if (supportedTechnologies.Length <= 0)
                        {
                            return;
                        }
                        System.Windows.Forms.Control view = designer.GetView(supportedTechnologies[0]) as System.Windows.Forms.Control;
                        if (view == null)
                        {
                            return;
                        }
                        view.Focus();
                    }
                    return;

                case 1:
                    this.DefWndProc(ref m);
                    if (m.HWnd == this.Control.Handle)
                    {
                        this.OnCreateHandle();
                    }
                    return;

                case 0x85:
                case 0x86:
                    if (m.Msg != 0x86)
                    {
                        if (this.thrownException == null)
                        {
                            this.DefWndProc(ref m);
                        }
                        break;
                    }
                    this.DefWndProc(ref m);
                    break;

                case 0x7b:
                    if (!inContextMenu)
                    {
                        x = System.Design.NativeMethods.Util.SignedLOWORD((int) ((long) m.LParam));
                        y = System.Design.NativeMethods.Util.SignedHIWORD((int) ((long) m.LParam));
                        ToolStripKeyboardHandlingService service2 = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
                        bool flag2 = false;
                        if (service2 != null)
                        {
                            flag2 = service2.OnContextMenu(x, y);
                        }
                        if (flag2)
                        {
                            return;
                        }
                        if ((x == -1) && (y == -1))
                        {
                            Point position = Cursor.Position;
                            x = position.X;
                            y = position.Y;
                        }
                        this.OnContextMenu(x, y);
                    }
                    return;

                case 160:
                case 0x200:
                    if ((((int) ((long) m.WParam)) & 1) != 0)
                    {
                        none = MouseButtons.Left;
                    }
                    else if ((((int) ((long) m.WParam)) & 2) != 0)
                    {
                        none = MouseButtons.Right;
                        this.toolPassThrough = false;
                    }
                    else
                    {
                        this.toolPassThrough = false;
                    }
                    if ((this.lastMoveScreenX != x) || (this.lastMoveScreenY != y))
                    {
                        if (this.toolPassThrough)
                        {
                            System.Design.NativeMethods.SendMessage(this.Control.Parent.Handle, m.Msg, m.WParam, (IntPtr) this.GetParentPointFromLparam(m.LParam));
                            return;
                        }
                        if (handler != null)
                        {
                            handler.OnMouseMove(base.Component, x, y);
                        }
                        else if (none == MouseButtons.Left)
                        {
                            this.OnMouseDragMove(x, y);
                        }
                    }
                    this.lastMoveScreenX = x;
                    this.lastMoveScreenY = y;
                    if (m.Msg == 0x200)
                    {
                        this.BaseWndProc(ref m);
                    }
                    return;

                case 0xa1:
                case 0xa4:
                case 0x201:
                case 0x204:
                    if ((m.Msg == 0xa4) || (m.Msg == 0x204))
                    {
                        none = MouseButtons.Right;
                    }
                    else
                    {
                        none = MouseButtons.Left;
                    }
                    System.Design.NativeMethods.SendMessage(this.Control.Handle, 7, 0, 0);
                    if ((none == MouseButtons.Left) && this.IsDoubleClick(x, y))
                    {
                        if (handler != null)
                        {
                            handler.OnMouseDoubleClick(base.Component);
                            return;
                        }
                        this.OnMouseDoubleClick();
                        return;
                    }
                    this.toolPassThrough = false;
                    if (!this.EnableDragRect && (none == MouseButtons.Left))
                    {
                        if (this.toolboxSvc == null)
                        {
                            this.toolboxSvc = (IToolboxService) this.GetService(typeof(IToolboxService));
                        }
                        if ((this.toolboxSvc != null) && (this.toolboxSvc.GetSelectedToolboxItem((IDesignerHost) this.GetService(typeof(IDesignerHost))) != null))
                        {
                            this.toolPassThrough = true;
                        }
                    }
                    else
                    {
                        this.toolPassThrough = false;
                    }
                    if (this.toolPassThrough)
                    {
                        System.Design.NativeMethods.SendMessage(this.Control.Parent.Handle, m.Msg, m.WParam, (IntPtr) this.GetParentPointFromLparam(m.LParam));
                        return;
                    }
                    if (handler != null)
                    {
                        handler.OnMouseDown(base.Component, none, x, y);
                    }
                    else if (none == MouseButtons.Left)
                    {
                        this.OnMouseDragBegin(x, y);
                    }
                    else if (none == MouseButtons.Right)
                    {
                        ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Click);
                        }
                    }
                    this.lastMoveScreenX = x;
                    this.lastMoveScreenY = y;
                    return;

                case 0xa2:
                case 0xa5:
                case 0x202:
                case 0x205:
                    if ((m.Msg == 0xa5) || (m.Msg == 0x205))
                    {
                        none = MouseButtons.Right;
                    }
                    else
                    {
                        none = MouseButtons.Left;
                    }
                    if (handler != null)
                    {
                        handler.OnMouseUp(base.Component, none);
                    }
                    else
                    {
                        if (this.toolPassThrough)
                        {
                            System.Design.NativeMethods.SendMessage(this.Control.Parent.Handle, m.Msg, m.WParam, (IntPtr) this.GetParentPointFromLparam(m.LParam));
                            this.toolPassThrough = false;
                            return;
                        }
                        if (none == MouseButtons.Left)
                        {
                            this.OnMouseDragEnd(false);
                        }
                    }
                    this.toolPassThrough = false;
                    this.BaseWndProc(ref m);
                    return;

                case 0xa3:
                case 0xa6:
                case 0x203:
                case 0x206:
                    if ((m.Msg == 0xa6) || (m.Msg == 0x206))
                    {
                        none = MouseButtons.Right;
                    }
                    else
                    {
                        none = MouseButtons.Left;
                    }
                    if (none == MouseButtons.Left)
                    {
                        if (handler != null)
                        {
                            handler.OnMouseDoubleClick(base.Component);
                            return;
                        }
                        this.OnMouseDoubleClick();
                    }
                    return;

                case 0xa7:
                case 0xa8:
                case 0xa9:
                case 0x207:
                case 520:
                case 0x209:
                case 0x20a:
                case 0x2a0:
                case 0x2a2:
                    return;

                case 0x2a1:
                    if (handler == null)
                    {
                        this.OnMouseHover();
                        return;
                    }
                    handler.OnMouseHover(base.Component);
                    return;

                case 0x2a3:
                    this.OnMouseLeave();
                    this.BaseWndProc(ref m);
                    return;

                case 0x318:
                {
                    using (Graphics graphics = Graphics.FromHdc(m.WParam))
                    {
                        using (PaintEventArgs args = new PaintEventArgs(graphics, this.Control.ClientRectangle))
                        {
                            this.DefWndProc(ref m);
                            this.OnPaintAdornments(args);
                        }
                        return;
                    }
                }
                default:
                    if (m.Msg == System.Design.NativeMethods.WM_MOUSEENTER)
                    {
                        this.OnMouseEnter();
                        this.BaseWndProc(ref m);
                    }
                    else if ((m.Msg < 0x100) || (m.Msg > 0x108))
                    {
                        this.DefWndProc(ref m);
                    }
                    return;
            }
            if (((this.OverlayService == null) || (this.Control == null)) || (!(this.Control.Size != this.Control.ClientSize) || (this.Control.Parent == null)))
            {
                return;
            }
            Rectangle rectangle2 = new Rectangle(this.Control.Parent.PointToScreen(this.Control.Location), this.Control.Size);
            Rectangle rectangle3 = new Rectangle(this.Control.PointToScreen(Point.Empty), this.Control.ClientSize);
            using (Region region2 = new Region(rectangle2))
            {
                region2.Exclude(rectangle3);
                this.OverlayService.InvalidateOverlays(region2);
                return;
            }
        Label_0A82:
            if (this.liveRegion)
            {
                this.DefWndProc(ref m);
            }
            else if (handler != null)
            {
                handler.OnSetCursor(base.Component);
            }
            else
            {
                this.OnSetCursor();
            }
        }

        public virtual AccessibleObject AccessibilityObject
        {
            get
            {
                if (this.accessibilityObj == null)
                {
                    this.accessibilityObj = new ControlDesignerAccessibleObject(this, this.Control);
                }
                return this.accessibilityObj;
            }
        }

        private bool AllowDrop
        {
            get
            {
                return (bool) base.ShadowProperties["AllowDrop"];
            }
            set
            {
                base.ShadowProperties["AllowDrop"] = value;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ArrayList list = null;
                foreach (System.Windows.Forms.Control control in this.Control.Controls)
                {
                    if (control.Site != null)
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        list.Add(control);
                    }
                }
                if (list != null)
                {
                    return list;
                }
                return base.AssociatedComponents;
            }
        }

        public bool AutoResizeHandles
        {
            get
            {
                return this.autoResizeHandles;
            }
            set
            {
                this.autoResizeHandles = value;
            }
        }

        protected System.Windows.Forms.Design.Behavior.BehaviorService BehaviorService
        {
            get
            {
                if (this.behaviorService == null)
                {
                    this.behaviorService = (System.Windows.Forms.Design.Behavior.BehaviorService) this.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService));
                }
                return this.behaviorService;
            }
        }

        private System.Windows.Forms.ContextMenu ContextMenu
        {
            get
            {
                return (System.Windows.Forms.ContextMenu) base.ShadowProperties["ContextMenu"];
            }
            set
            {
                System.Windows.Forms.ContextMenu menu = (System.Windows.Forms.ContextMenu) base.ShadowProperties["ContextMenu"];
                if (menu != value)
                {
                    EventHandler handler = new EventHandler(this.DetachContextMenu);
                    if (menu != null)
                    {
                        menu.Disposed -= handler;
                    }
                    base.ShadowProperties["ContextMenu"] = value;
                    if (value != null)
                    {
                        value.Disposed += handler;
                    }
                }
            }
        }

        public virtual System.Windows.Forms.Control Control
        {
            get
            {
                return (System.Windows.Forms.Control) base.Component;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        private DesignerControlCollection Controls
        {
            get
            {
                if (this.controls == null)
                {
                    this.controls = new DesignerControlCollection(this.Control);
                }
                return this.controls;
            }
        }

        internal virtual bool ControlSupportsSnaplines
        {
            get
            {
                return true;
            }
        }

        private int CurrentProcessId
        {
            get
            {
                if (currentProcessId == 0)
                {
                    currentProcessId = System.Design.SafeNativeMethods.GetCurrentProcessId();
                }
                return currentProcessId;
            }
        }

        private IDesignerTarget DesignerTarget
        {
            get
            {
                return this.designerTarget;
            }
            set
            {
                this.designerTarget = value;
            }
        }

        private bool Enabled
        {
            get
            {
                return (bool) base.ShadowProperties["Enabled"];
            }
            set
            {
                base.ShadowProperties["Enabled"] = value;
            }
        }

        protected virtual bool EnableDragRect
        {
            get
            {
                return false;
            }
        }

        internal bool ForceVisible
        {
            get
            {
                return this.forceVisible;
            }
            set
            {
                this.forceVisible = value;
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if (base.IsRootDesigner)
                {
                    return System.ComponentModel.InheritanceAttribute.Inherited;
                }
                return base.InheritanceAttribute;
            }
        }

        private Point Location
        {
            get
            {
                Point location = this.Control.Location;
                ScrollableControl parent = this.Control.Parent as ScrollableControl;
                if (parent != null)
                {
                    Point autoScrollPosition = parent.AutoScrollPosition;
                    location.Offset(-autoScrollPosition.X, -autoScrollPosition.Y);
                }
                return location;
            }
            set
            {
                ScrollableControl parent = this.Control.Parent as ScrollableControl;
                if (parent != null)
                {
                    Point autoScrollPosition = parent.AutoScrollPosition;
                    value.Offset(autoScrollPosition.X, autoScrollPosition.Y);
                }
                this.Control.Location = value;
            }
        }

        private bool Locked
        {
            get
            {
                return this.locked;
            }
            set
            {
                if (this.locked != value)
                {
                    this.locked = value;
                }
            }
        }

        internal System.Windows.Forms.Design.Behavior.Behavior MoveBehavior
        {
            get
            {
                if (this.moveBehavior == null)
                {
                    this.moveBehavior = new ContainerSelectorBehavior(this.Control, base.Component.Site);
                }
                return this.moveBehavior;
            }
        }

        private string Name
        {
            get
            {
                return base.Component.Site.Name;
            }
            set
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service == null) || ((service != null) && !service.Loading))
                {
                    base.Component.Site.Name = value;
                }
            }
        }

        private IOverlayService OverlayService
        {
            get
            {
                if (this.overlayService == null)
                {
                    this.overlayService = (IOverlayService) this.GetService(typeof(IOverlayService));
                }
                return this.overlayService;
            }
        }

        protected override IComponent ParentComponent
        {
            get
            {
                System.Windows.Forms.Control component = base.Component as System.Windows.Forms.Control;
                if ((component != null) && (component.Parent != null))
                {
                    return component.Parent;
                }
                return base.ParentComponent;
            }
        }

        public virtual bool ParticipatesWithSnapLines
        {
            get
            {
                return true;
            }
        }

        public virtual System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                PropertyDescriptor descriptor;
                System.Windows.Forms.Design.SelectionRules visible = System.Windows.Forms.Design.SelectionRules.Visible;
                object component = base.Component;
                visible = System.Windows.Forms.Design.SelectionRules.Visible;
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                PropertyDescriptor autoSizeProp = properties["AutoSize"];
                PropertyDescriptor autoSizeModeProp = properties["AutoSizeMode"];
                if (((descriptor = properties["Location"]) != null) && !descriptor.IsReadOnly)
                {
                    visible |= System.Windows.Forms.Design.SelectionRules.Moveable;
                }
                if (((descriptor = properties["Size"]) != null) && !descriptor.IsReadOnly)
                {
                    if (this.AutoResizeHandles && (base.Component != this.host.RootComponent))
                    {
                        visible = this.IsResizableConsiderAutoSize(autoSizeProp, autoSizeModeProp) ? (visible | System.Windows.Forms.Design.SelectionRules.AllSizeable) : visible;
                    }
                    else
                    {
                        visible |= System.Windows.Forms.Design.SelectionRules.AllSizeable;
                    }
                }
                PropertyDescriptor descriptor4 = properties["Dock"];
                if (descriptor4 != null)
                {
                    DockStyle right = (DockStyle) ((int) descriptor4.GetValue(component));
                    if ((this.Control.Parent != null) && this.Control.Parent.IsMirrored)
                    {
                        switch (right)
                        {
                            case DockStyle.Left:
                                right = DockStyle.Right;
                                break;

                            case DockStyle.Right:
                                right = DockStyle.Left;
                                break;
                        }
                    }
                    switch (right)
                    {
                        case DockStyle.Top:
                            visible &= ~(System.Windows.Forms.Design.SelectionRules.Moveable | System.Windows.Forms.Design.SelectionRules.RightSizeable | System.Windows.Forms.Design.SelectionRules.LeftSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable);
                            break;

                        case DockStyle.Bottom:
                            visible &= ~(System.Windows.Forms.Design.SelectionRules.Moveable | System.Windows.Forms.Design.SelectionRules.RightSizeable | System.Windows.Forms.Design.SelectionRules.LeftSizeable | System.Windows.Forms.Design.SelectionRules.BottomSizeable);
                            break;

                        case DockStyle.Left:
                            visible &= ~(System.Windows.Forms.Design.SelectionRules.Moveable | System.Windows.Forms.Design.SelectionRules.LeftSizeable | System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable);
                            break;

                        case DockStyle.Right:
                            visible &= ~(System.Windows.Forms.Design.SelectionRules.Moveable | System.Windows.Forms.Design.SelectionRules.RightSizeable | System.Windows.Forms.Design.SelectionRules.BottomSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable);
                            break;

                        case DockStyle.Fill:
                            visible &= ~(System.Windows.Forms.Design.SelectionRules.Moveable | System.Windows.Forms.Design.SelectionRules.AllSizeable);
                            break;
                    }
                }
                PropertyDescriptor descriptor5 = properties["Locked"];
                if (descriptor5 != null)
                {
                    object obj3 = descriptor5.GetValue(component);
                    if ((obj3 is bool) && ((bool) obj3))
                    {
                        visible = System.Windows.Forms.Design.SelectionRules.Visible | System.Windows.Forms.Design.SelectionRules.Locked;
                    }
                }
                return visible;
            }
        }

        internal virtual bool SerializePerformLayout
        {
            get
            {
                return false;
            }
        }

        public virtual IList SnapLines
        {
            get
            {
                return this.SnapLinesInternal();
            }
        }

        internal virtual System.Windows.Forms.Design.Behavior.Behavior StandardBehavior
        {
            get
            {
                if (this.resizeBehavior == null)
                {
                    this.resizeBehavior = new ResizeBehavior(base.Component.Site);
                }
                return this.resizeBehavior;
            }
        }

        private Dictionary<IntPtr, bool> SubclassedChildWindows
        {
            get
            {
                if (this.subclassedChildren == null)
                {
                    this.subclassedChildren = new Dictionary<IntPtr, bool>();
                }
                return this.subclassedChildren;
            }
        }

        private bool Visible
        {
            get
            {
                return (bool) base.ShadowProperties["Visible"];
            }
            set
            {
                base.ShadowProperties["Visible"] = value;
            }
        }

        private class CanResetSizePropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor _basePropDesc;

            public CanResetSizePropertyDescriptor(PropertyDescriptor pd) : base(pd)
            {
                this._basePropDesc = pd;
            }

            public override bool CanResetValue(object component)
            {
                return this._basePropDesc.ShouldSerializeValue(component);
            }

            public override object GetValue(object component)
            {
                return this._basePropDesc.GetValue(component);
            }

            public override void ResetValue(object component)
            {
                this._basePropDesc.ResetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                this._basePropDesc.SetValue(component, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return true;
            }

            public override System.Type ComponentType
            {
                get
                {
                    return this._basePropDesc.ComponentType;
                }
            }

            public override string DisplayName
            {
                get
                {
                    return this._basePropDesc.DisplayName;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this._basePropDesc.IsReadOnly;
                }
            }

            public override System.Type PropertyType
            {
                get
                {
                    return this._basePropDesc.PropertyType;
                }
            }
        }

        private class ChildSubClass : NativeWindow, ControlDesigner.IDesignerTarget, IDisposable
        {
            private ControlDesigner designer;

            public ChildSubClass(ControlDesigner designer, IntPtr hwnd)
            {
                this.designer = designer;
                if (designer != null)
                {
                    designer.disposingHandler += new EventHandler(this.OnDesignerDisposing);
                }
                base.AssignHandle(hwnd);
            }

            public void Dispose()
            {
                this.designer = null;
            }

            private void OnDesignerDisposing(object sender, EventArgs e)
            {
                this.Dispose();
            }

            void ControlDesigner.IDesignerTarget.DefWndProc(ref Message m)
            {
                base.DefWndProc(ref m);
            }

            protected override void WndProc(ref Message m)
            {
                if (this.designer == null)
                {
                    base.DefWndProc(ref m);
                }
                else
                {
                    if (m.Msg == 2)
                    {
                        this.designer.RemoveSubclassedWindow(m.HWnd);
                    }
                    if ((m.Msg == 0x210) && (System.Design.NativeMethods.Util.LOWORD((int) ((long) m.WParam)) == 1))
                    {
                        this.designer.HookChildHandles(m.LParam);
                    }
                    ControlDesigner.IDesignerTarget designerTarget = this.designer.DesignerTarget;
                    this.designer.DesignerTarget = this;
                    try
                    {
                        this.designer.WndProc(ref m);
                    }
                    catch (Exception exception)
                    {
                        this.designer.SetUnhandledException(Control.FromChildHandle(m.HWnd), exception);
                    }
                    finally
                    {
                        if ((this.designer != null) && (this.designer.Component != null))
                        {
                            this.designer.DesignerTarget = designerTarget;
                        }
                    }
                }
            }
        }

        private class ChildWindowTarget : IWindowTarget, ControlDesigner.IDesignerTarget, IDisposable
        {
            private Control childControl;
            private ControlDesigner designer;
            private IntPtr handle = IntPtr.Zero;
            private IWindowTarget oldWindowTarget;

            public ChildWindowTarget(ControlDesigner designer, Control childControl, IWindowTarget oldWindowTarget)
            {
                this.designer = designer;
                this.childControl = childControl;
                this.oldWindowTarget = oldWindowTarget;
            }

            public void DefWndProc(ref Message m)
            {
                this.oldWindowTarget.OnMessage(ref m);
            }

            public void Dispose()
            {
            }

            public void OnHandleChange(IntPtr newHandle)
            {
                this.handle = newHandle;
                this.oldWindowTarget.OnHandleChange(newHandle);
            }

            public void OnMessage(ref Message m)
            {
                if (this.designer.Component == null)
                {
                    this.oldWindowTarget.OnMessage(ref m);
                }
                else
                {
                    ControlDesigner.IDesignerTarget designerTarget = this.designer.DesignerTarget;
                    this.designer.DesignerTarget = this;
                    try
                    {
                        this.designer.WndProc(ref m);
                    }
                    catch (Exception exception)
                    {
                        this.designer.SetUnhandledException(this.childControl, exception);
                    }
                    finally
                    {
                        if (this.designer.DesignerTarget == null)
                        {
                            designerTarget.Dispose();
                        }
                        else
                        {
                            this.designer.DesignerTarget = designerTarget;
                        }
                        if (m.Msg == 1)
                        {
                            System.Design.NativeMethods.RevokeDragDrop(this.handle);
                        }
                    }
                }
            }

            public IWindowTarget OldWindowTarget
            {
                get
                {
                    return this.oldWindowTarget;
                }
            }
        }

        [ComVisible(true)]
        public class ControlDesignerAccessibleObject : AccessibleObject
        {
            private Control control;
            private ControlDesigner designer;
            private IDesignerHost host;
            private ISelectionService selSvc;

            public ControlDesignerAccessibleObject(ControlDesigner designer, Control control)
            {
                this.designer = designer;
                this.control = control;
            }

            public override AccessibleObject GetChild(int index)
            {
                Control.ControlAccessibleObject child = this.control.AccessibilityObject.GetChild(index) as Control.ControlAccessibleObject;
                if (child != null)
                {
                    AccessibleObject designerAccessibleObject = this.GetDesignerAccessibleObject(child);
                    if (designerAccessibleObject != null)
                    {
                        return designerAccessibleObject;
                    }
                }
                return this.control.AccessibilityObject.GetChild(index);
            }

            public override int GetChildCount()
            {
                return this.control.AccessibilityObject.GetChildCount();
            }

            private AccessibleObject GetDesignerAccessibleObject(Control.ControlAccessibleObject cao)
            {
                if (cao != null)
                {
                    ControlDesigner designer = this.DesignerHost.GetDesigner(cao.Owner) as ControlDesigner;
                    if (designer != null)
                    {
                        return designer.AccessibilityObject;
                    }
                }
                return null;
            }

            public override AccessibleObject GetFocused()
            {
                if ((this.State & AccessibleStates.Focused) != AccessibleStates.None)
                {
                    return this;
                }
                return base.GetFocused();
            }

            public override AccessibleObject GetSelected()
            {
                if ((this.State & AccessibleStates.Selected) != AccessibleStates.None)
                {
                    return this;
                }
                return base.GetFocused();
            }

            public override AccessibleObject HitTest(int x, int y)
            {
                return this.control.AccessibilityObject.HitTest(x, y);
            }

            public override Rectangle Bounds
            {
                get
                {
                    return this.control.AccessibilityObject.Bounds;
                }
            }

            public override string DefaultAction
            {
                get
                {
                    return "";
                }
            }

            public override string Description
            {
                get
                {
                    return this.control.AccessibilityObject.Description;
                }
            }

            private IDesignerHost DesignerHost
            {
                get
                {
                    if (this.host == null)
                    {
                        this.host = (IDesignerHost) this.designer.GetService(typeof(IDesignerHost));
                    }
                    return this.host;
                }
            }

            public override string Name
            {
                get
                {
                    return this.control.Name;
                }
            }

            public override AccessibleObject Parent
            {
                get
                {
                    return this.control.AccessibilityObject.Parent;
                }
            }

            public override AccessibleRole Role
            {
                get
                {
                    return this.control.AccessibilityObject.Role;
                }
            }

            private ISelectionService SelectionService
            {
                get
                {
                    if (this.selSvc == null)
                    {
                        this.selSvc = (ISelectionService) this.designer.GetService(typeof(ISelectionService));
                    }
                    return this.selSvc;
                }
            }

            public override AccessibleStates State
            {
                get
                {
                    AccessibleStates state = this.control.AccessibilityObject.State;
                    ISelectionService selectionService = this.SelectionService;
                    if (selectionService != null)
                    {
                        if (selectionService.GetComponentSelected(this.control))
                        {
                            state |= AccessibleStates.Selected;
                        }
                        if (selectionService.PrimarySelection == this.control)
                        {
                            state |= AccessibleStates.Focused;
                        }
                    }
                    return state;
                }
            }

            public override string Value
            {
                get
                {
                    return this.control.AccessibilityObject.Value;
                }
            }
        }

        [DesignerSerializer(typeof(ControlDesigner.DesignerControlCollectionCodeDomSerializer), typeof(CodeDomSerializer)), ListBindable(false)]
        internal class DesignerControlCollection : Control.ControlCollection, IList, ICollection, IEnumerable
        {
            private Control.ControlCollection realCollection;

            public DesignerControlCollection(Control owner) : base(owner)
            {
                this.realCollection = owner.Controls;
            }

            public override void Add(Control c)
            {
                this.realCollection.Add(c);
            }

            public override void AddRange(Control[] controls)
            {
                this.realCollection.AddRange(controls);
            }

            public override void Clear()
            {
                for (int i = this.realCollection.Count - 1; i >= 0; i--)
                {
                    if (((this.realCollection[i] != null) && (this.realCollection[i].Site != null)) && TypeDescriptor.GetAttributes(this.realCollection[i]).Contains(InheritanceAttribute.NotInherited))
                    {
                        this.realCollection.RemoveAt(i);
                    }
                }
            }

            public void CopyTo(Array dest, int index)
            {
                this.realCollection.CopyTo(dest, index);
            }

            public override bool Equals(object other)
            {
                return this.realCollection.Equals(other);
            }

            public override int GetChildIndex(Control child, bool throwException)
            {
                return this.realCollection.GetChildIndex(child, throwException);
            }

            public IEnumerator GetEnumerator()
            {
                return this.realCollection.GetEnumerator();
            }

            public override int GetHashCode()
            {
                return this.realCollection.GetHashCode();
            }

            public override void SetChildIndex(Control child, int newIndex)
            {
                this.realCollection.SetChildIndex(child, newIndex);
            }

            int IList.Add(object control)
            {
                return ((IList) this.realCollection).Add(control);
            }

            bool IList.Contains(object control)
            {
                return ((IList) this.realCollection).Contains(control);
            }

            int IList.IndexOf(object control)
            {
                return ((IList) this.realCollection).IndexOf(control);
            }

            void IList.Insert(int index, object value)
            {
                ((IList) this.realCollection).Insert(index, value);
            }

            void IList.Remove(object control)
            {
                ((IList) this.realCollection).Remove(control);
            }

            void IList.RemoveAt(int index)
            {
                ((IList) this.realCollection).RemoveAt(index);
            }

            public override int Count
            {
                get
                {
                    return this.realCollection.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return this.realCollection.IsReadOnly;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this.realCollection[index];
                }
                set
                {
                    throw new NotSupportedException();
                }
            }
        }

        internal class DesignerControlCollectionCodeDomSerializer : CollectionCodeDomSerializer
        {
            protected override object SerializeCollection(IDesignerSerializationManager manager, CodeExpression targetExpression, System.Type targetType, ICollection originalCollection, ICollection valuesToSerialize)
            {
                ArrayList list = new ArrayList();
                if ((valuesToSerialize != null) && (valuesToSerialize.Count > 0))
                {
                    foreach (object obj2 in valuesToSerialize)
                    {
                        IComponent component = obj2 as IComponent;
                        if (((component != null) && (component.Site != null)) && !(component.Site is INestedSite))
                        {
                            list.Add(component);
                        }
                    }
                }
                return base.SerializeCollection(manager, targetExpression, targetType, originalCollection, list);
            }
        }

        private class DesignerWindowTarget : IWindowTarget, ControlDesigner.IDesignerTarget, IDisposable
        {
            internal ControlDesigner designer;
            internal IWindowTarget oldTarget;

            public DesignerWindowTarget(ControlDesigner designer)
            {
                Control control = designer.Control;
                this.designer = designer;
                this.oldTarget = control.WindowTarget;
                control.WindowTarget = this;
            }

            public void DefWndProc(ref Message m)
            {
                this.oldTarget.OnMessage(ref m);
            }

            public void Dispose()
            {
                if (this.designer != null)
                {
                    this.designer.Control.WindowTarget = this.oldTarget;
                    this.designer = null;
                }
            }

            public void OnHandleChange(IntPtr newHandle)
            {
                this.oldTarget.OnHandleChange(newHandle);
                if (newHandle != IntPtr.Zero)
                {
                    this.designer.OnHandleChange();
                }
            }

            public void OnMessage(ref Message m)
            {
                ControlDesigner designer = this.designer;
                if (designer != null)
                {
                    ControlDesigner.IDesignerTarget designerTarget = designer.DesignerTarget;
                    designer.DesignerTarget = this;
                    try
                    {
                        designer.WndProc(ref m);
                    }
                    catch (Exception exception)
                    {
                        designer.SetUnhandledException(designer.Control, exception);
                    }
                    finally
                    {
                        designer.DesignerTarget = designerTarget;
                    }
                }
                else
                {
                    this.DefWndProc(ref m);
                }
            }
        }

        private class DockingActionList : DesignerActionList
        {
            private ControlDesigner _designer;
            private IDesignerHost _host;

            public DockingActionList(ControlDesigner owner) : base(owner.Component)
            {
                this._designer = owner;
                this._host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            }

            private string GetActionName()
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Dock"];
                if (descriptor == null)
                {
                    return null;
                }
                DockStyle style = (DockStyle) descriptor.GetValue(base.Component);
                if (style == DockStyle.Fill)
                {
                    return System.Design.SR.GetString("DesignerShortcutUndockInParent");
                }
                return System.Design.SR.GetString("DesignerShortcutDockInParent");
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                if (this.GetActionName() != null)
                {
                    items.Add(new DesignerActionVerbItem(new DesignerVerb(this.GetActionName(), new EventHandler(this.OnDockActionClick))));
                }
                return items;
            }

            private void OnDockActionClick(object sender, EventArgs e)
            {
                DesignerVerb verb = sender as DesignerVerb;
                if ((verb != null) && (this._host != null))
                {
                    using (DesignerTransaction transaction = this._host.CreateTransaction(verb.Text))
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Dock"];
                        DockStyle style = (DockStyle) descriptor.GetValue(base.Component);
                        if (style == DockStyle.Fill)
                        {
                            descriptor.SetValue(base.Component, DockStyle.None);
                        }
                        else
                        {
                            descriptor.SetValue(base.Component, DockStyle.Fill);
                        }
                        transaction.Commit();
                    }
                }
            }
        }

        private interface IDesignerTarget : IDisposable
        {
            void DefWndProc(ref Message m);
        }

        internal class TransparentBehavior : System.Windows.Forms.Design.Behavior.Behavior
        {
            private Rectangle controlRect = Rectangle.Empty;
            private ControlDesigner designer;

            internal TransparentBehavior(ControlDesigner designer)
            {
                this.designer = designer;
            }

            internal bool IsTransparent(Point p)
            {
                return this.designer.GetHitTest(p);
            }

            public override void OnDragDrop(Glyph g, DragEventArgs e)
            {
                this.controlRect = Rectangle.Empty;
                this.designer.OnDragDrop(e);
            }

            public override void OnDragEnter(Glyph g, DragEventArgs e)
            {
                if ((this.designer != null) && (this.designer.Control != null))
                {
                    this.controlRect = this.designer.Control.RectangleToScreen(this.designer.Control.ClientRectangle);
                }
                this.designer.OnDragEnter(e);
            }

            public override void OnDragLeave(Glyph g, EventArgs e)
            {
                this.controlRect = Rectangle.Empty;
                this.designer.OnDragLeave(e);
            }

            public override void OnDragOver(Glyph g, DragEventArgs e)
            {
                if (((e != null) && (this.controlRect != Rectangle.Empty)) && !this.controlRect.Contains(new Point(e.X, e.Y)))
                {
                    e.Effect = DragDropEffects.None;
                }
                else
                {
                    this.designer.OnDragOver(e);
                }
            }

            public override void OnGiveFeedback(Glyph g, GiveFeedbackEventArgs e)
            {
                this.designer.OnGiveFeedback(e);
            }
        }
    }
}

