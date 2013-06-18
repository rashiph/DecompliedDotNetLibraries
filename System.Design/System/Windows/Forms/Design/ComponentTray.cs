namespace System.Windows.Forms.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    [ToolboxItem(false), DesignTimeVisible(false), ProvideProperty("Location", typeof(IComponent)), ProvideProperty("TrayLocation", typeof(IComponent))]
    public class ComponentTray : ScrollableControl, IExtenderProvider, ISelectionUIHandler, IOleDragClient
    {
        private bool autoArrange;
        private Point autoScrollPosBeforeDragging = Point.Empty;
        private ArrayList controls;
        private SelectionUIHandler dragHandler;
        private IEventHandlerService eventHandlerService;
        private bool fResetAmbient;
        private ComponentTrayGlyphManager glyphManager;
        private Size grabHandle = Size.Empty;
        private System.Windows.Forms.Design.InheritanceUI inheritanceUI;
        private static readonly Point InvalidPoint = new Point(-2147483648, -2147483648);
        private IDesigner mainDesigner;
        private MenuCommand menucmdArrangeIcons;
        private MenuCommand menucmdLargeIcons;
        private MenuCommand menucmdLineupIcons;
        private IMenuCommandService menuCommandService;
        private Point mouseDragEnd = InvalidPoint;
        private Point mouseDragStart = InvalidPoint;
        private ToolboxItem mouseDragTool;
        private Rectangle mouseDragWorkspace = Rectangle.Empty;
        private Point mouseDropLocation = InvalidPoint;
        internal OleDragDropHandler oleDragDropHandler;
        private CommandSet privateCommandSet;
        private bool queriedTabOrder;
        private ICollection selectedObjects;
        private ISelectionUIService selectionUISvc;
        private IServiceProvider serviceProvider;
        private bool showLargeIcons;
        private MenuCommand tabOrderCommand;
        private IToolboxService toolboxService;
        private Point whiteSpace = Point.Empty;

        public ComponentTray(IDesigner mainDesigner, IServiceProvider serviceProvider)
        {
            this.AutoScroll = true;
            this.mainDesigner = mainDesigner;
            this.serviceProvider = serviceProvider;
            this.AllowDrop = true;
            this.Text = "ComponentTray";
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.controls = new ArrayList();
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            IExtenderProviderService service = (IExtenderProviderService) this.GetService(typeof(IExtenderProviderService));
            if (service != null)
            {
                service.AddExtenderProvider(this);
            }
            if ((this.GetService(typeof(IEventHandlerService)) == null) && (host != null))
            {
                this.eventHandlerService = new EventHandlerService(this);
                host.AddService(typeof(IEventHandlerService), this.eventHandlerService);
            }
            IMenuCommandService menuService = this.MenuService;
            if (menuService != null)
            {
                this.menucmdArrangeIcons = new MenuCommand(new EventHandler(this.OnMenuArrangeIcons), StandardCommands.ArrangeIcons);
                this.menucmdLineupIcons = new MenuCommand(new EventHandler(this.OnMenuLineupIcons), StandardCommands.LineupIcons);
                this.menucmdLargeIcons = new MenuCommand(new EventHandler(this.OnMenuShowLargeIcons), StandardCommands.ShowLargeIcons);
                this.menucmdArrangeIcons.Checked = this.AutoArrange;
                this.menucmdLargeIcons.Checked = this.ShowLargeIcons;
                menuService.AddCommand(this.menucmdArrangeIcons);
                menuService.AddCommand(this.menucmdLineupIcons);
                menuService.AddCommand(this.menucmdLargeIcons);
            }
            IComponentChangeService service3 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service3 != null)
            {
                service3.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
            }
            IUIService service4 = (IUIService) this.GetService(typeof(IUIService));
            if (service4 != null)
            {
                System.Drawing.Color info;
                if (service4.Styles["VsColorDesignerTray"] is System.Drawing.Color)
                {
                    info = (System.Drawing.Color) service4.Styles["VsColorDesignerTray"];
                }
                else if (service4.Styles["HighlightColor"] is System.Drawing.Color)
                {
                    info = (System.Drawing.Color) service4.Styles["HighlightColor"];
                }
                else
                {
                    info = SystemColors.Info;
                }
                this.BackColor = info;
                this.Font = (Font) service4.Styles["DialogFont"];
            }
            ISelectionService selSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (selSvc != null)
            {
                selSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            SystemEvents.DisplaySettingsChanged += new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.InstalledFontsChanged += new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
            TypeDescriptor.Refreshed += new RefreshEventHandler(this.OnComponentRefresh);
            BehaviorService behaviorSvc = this.GetService(typeof(BehaviorService)) as BehaviorService;
            if (behaviorSvc != null)
            {
                this.glyphManager = new ComponentTrayGlyphManager(selSvc, behaviorSvc);
            }
        }

        public virtual void AddComponent(IComponent component)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (this.CanDisplayComponent(component))
            {
                if (this.selectionUISvc == null)
                {
                    this.selectionUISvc = (ISelectionUIService) this.GetService(typeof(ISelectionUIService));
                    if (this.selectionUISvc == null)
                    {
                        this.selectionUISvc = new SelectionUIService(service);
                        service.AddService(typeof(ISelectionUIService), this.selectionUISvc);
                    }
                    this.grabHandle = this.selectionUISvc.GetAdornmentDimensions(AdornmentType.GrabHandle);
                }
                TrayControl control = new TrayControl(this, component);
                base.SuspendLayout();
                try
                {
                    base.Controls.Add(control);
                    this.controls.Add(control);
                    TypeDescriptor.Refresh(component);
                    if ((service != null) && !service.Loading)
                    {
                        this.PositionControl(control);
                    }
                    if (this.selectionUISvc != null)
                    {
                        this.selectionUISvc.AssignSelectionUIHandler(component, this);
                    }
                    InheritanceAttribute inheritanceAttribute = control.InheritanceAttribute;
                    if (inheritanceAttribute.InheritanceLevel != InheritanceLevel.NotInherited)
                    {
                        System.Windows.Forms.Design.InheritanceUI inheritanceUI = this.InheritanceUI;
                        if (inheritanceUI != null)
                        {
                            inheritanceUI.AddInheritedControl(control, inheritanceAttribute.InheritanceLevel);
                        }
                    }
                }
                finally
                {
                    base.ResumeLayout();
                }
                if ((service != null) && !service.Loading)
                {
                    base.ScrollControlIntoView(control);
                }
            }
        }

        protected virtual bool CanCreateComponentFromTool(ToolboxItem tool)
        {
            System.Type t = ((IDesignerHost) this.GetService(typeof(IDesignerHost))).GetType(tool.TypeName);
            if (t != null)
            {
                if (!t.IsSubclassOf(typeof(Control)))
                {
                    return true;
                }
                System.Type designerType = this.GetDesignerType(t, typeof(IDesigner));
                if (typeof(ControlDesigner).IsAssignableFrom(designerType))
                {
                    return false;
                }
            }
            return true;
        }

        protected virtual bool CanDisplayComponent(IComponent component)
        {
            return TypeDescriptor.GetAttributes(component).Contains(DesignTimeVisibleAttribute.Yes);
        }

        public void CreateComponentFromTool(ToolboxItem tool)
        {
            if (this.CanCreateComponentFromTool(tool))
            {
                this.GetOleDragHandler().CreateTool(tool, null, 0, 0, 0, 0, false, false);
            }
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
                System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.controls != null))
            {
                IExtenderProviderService service = (IExtenderProviderService) this.GetService(typeof(IExtenderProviderService));
                bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                if (service != null)
                {
                    service.RemoveExtenderProvider(this);
                }
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((this.eventHandlerService != null) && (host != null))
                {
                    host.RemoveService(typeof(IEventHandlerService));
                    this.eventHandlerService = null;
                }
                IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service2 != null)
                {
                    service2.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                }
                TypeDescriptor.Refreshed -= new RefreshEventHandler(this.OnComponentRefresh);
                SystemEvents.DisplaySettingsChanged -= new EventHandler(this.OnSystemSettingChanged);
                SystemEvents.InstalledFontsChanged -= new EventHandler(this.OnSystemSettingChanged);
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
                IMenuCommandService menuService = this.MenuService;
                if (menuService != null)
                {
                    menuService.RemoveCommand(this.menucmdArrangeIcons);
                    menuService.RemoveCommand(this.menucmdLineupIcons);
                    menuService.RemoveCommand(this.menucmdLargeIcons);
                }
                if (this.privateCommandSet != null)
                {
                    this.privateCommandSet.Dispose();
                    if (host != null)
                    {
                        host.RemoveService(typeof(ISelectionUIService));
                    }
                }
                this.selectionUISvc = null;
                if (this.inheritanceUI != null)
                {
                    this.inheritanceUI.Dispose();
                    this.inheritanceUI = null;
                }
                this.serviceProvider = null;
                this.controls.Clear();
                this.controls = null;
                if (this.glyphManager != null)
                {
                    this.glyphManager.Dispose();
                    this.glyphManager = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DoAutoArrange(bool dirtyDesigner)
        {
            if ((this.controls != null) && (this.controls.Count > 0))
            {
                this.controls.Sort(new AutoArrangeComparer());
                base.SuspendLayout();
                base.AutoScrollPosition = new Point(0, 0);
                try
                {
                    Control prevCtl = null;
                    bool flag = true;
                    foreach (Control control2 in this.controls)
                    {
                        if (control2.Visible)
                        {
                            if (this.autoArrange)
                            {
                                this.PositionInNextAutoSlot(control2 as TrayControl, prevCtl, dirtyDesigner);
                            }
                            else if (!((TrayControl) control2).Positioned || !flag)
                            {
                                this.PositionInNextAutoSlot(control2 as TrayControl, prevCtl, false);
                                flag = false;
                            }
                            prevCtl = control2;
                        }
                    }
                    if (this.selectionUISvc != null)
                    {
                        this.selectionUISvc.SyncSelection();
                    }
                }
                finally
                {
                    base.ResumeLayout();
                }
            }
        }

        private void DoLineupIcons()
        {
            if (!this.autoArrange)
            {
                bool autoArrange = this.autoArrange;
                this.autoArrange = true;
                try
                {
                    this.DoAutoArrange(true);
                }
                finally
                {
                    this.autoArrange = autoArrange;
                }
            }
        }

        private void DrawRubber(Point start, Point end)
        {
            this.mouseDragWorkspace.X = Math.Min(start.X, end.X);
            this.mouseDragWorkspace.Y = Math.Min(start.Y, end.Y);
            this.mouseDragWorkspace.Width = Math.Abs((int) (end.X - start.X));
            this.mouseDragWorkspace.Height = Math.Abs((int) (end.Y - start.Y));
            this.mouseDragWorkspace = base.RectangleToScreen(this.mouseDragWorkspace);
            ControlPaint.DrawReversibleFrame(this.mouseDragWorkspace, this.BackColor, FrameStyle.Dashed);
        }

        internal void FocusDesigner()
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((service != null) && (service.RootComponent != null))
            {
                IRootDesigner designer = service.GetDesigner(service.RootComponent) as IRootDesigner;
                if (designer != null)
                {
                    ViewTechnology[] supportedTechnologies = designer.SupportedTechnologies;
                    if (supportedTechnologies.Length > 0)
                    {
                        Control view = designer.GetView(supportedTechnologies[0]) as Control;
                        if (view != null)
                        {
                            view.Focus();
                        }
                    }
                }
            }
        }

        private object[] GetComponentsInRect(Rectangle rect)
        {
            ArrayList list = new ArrayList();
            int count = base.Controls.Count;
            for (int i = 0; i < count; i++)
            {
                Control control = base.Controls[i];
                Rectangle bounds = control.Bounds;
                TrayControl control2 = control as TrayControl;
                if ((control2 != null) && bounds.IntersectsWith(rect))
                {
                    list.Add(control2.Component);
                }
            }
            return list.ToArray();
        }

        private System.Type GetDesignerType(System.Type t, System.Type designerBaseType)
        {
            System.Type type = null;
            AttributeCollection attributes = TypeDescriptor.GetAttributes(t);
            for (int i = 0; i < attributes.Count; i++)
            {
                DesignerAttribute attribute = attributes[i] as DesignerAttribute;
                if (attribute != null)
                {
                    System.Type type2 = System.Type.GetType(attribute.DesignerBaseTypeName);
                    if ((type2 != null) && (type2 == designerBaseType))
                    {
                        bool flag = false;
                        ITypeResolutionService service = (ITypeResolutionService) this.GetService(typeof(ITypeResolutionService));
                        if (service != null)
                        {
                            flag = true;
                            type = service.GetType(attribute.DesignerTypeName);
                        }
                        if (!flag)
                        {
                            type = System.Type.GetType(attribute.DesignerTypeName);
                        }
                        if (type != null)
                        {
                            return type;
                        }
                    }
                }
            }
            return type;
        }

        internal Size GetDragDimensions()
        {
            if (this.AutoArrange)
            {
                ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                IComponent primarySelection = null;
                if (service != null)
                {
                    primarySelection = (IComponent) service.PrimarySelection;
                }
                Control controlForComponent = null;
                if (primarySelection != null)
                {
                    controlForComponent = ((IOleDragClient) this).GetControlForComponent(primarySelection);
                }
                if ((controlForComponent == null) && (this.controls.Count > 0))
                {
                    controlForComponent = (Control) this.controls[0];
                }
                if (controlForComponent != null)
                {
                    Size size = controlForComponent.Size;
                    size.Width += 2 * this.whiteSpace.X;
                    size.Height += 2 * this.whiteSpace.Y;
                    return size;
                }
            }
            return new Size(10, 10);
        }

        [Browsable(false), System.Design.SRDescription("ControlLocationDescr"), DesignOnly(true), Category("Layout"), Localizable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Point GetLocation(IComponent receiver)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(receiver.GetType())["Location"];
            if (descriptor != null)
            {
                return (Point) descriptor.GetValue(receiver);
            }
            return this.GetTrayLocation(receiver);
        }

        public IComponent GetNextComponent(IComponent component, bool forward)
        {
            for (int i = 0; i < this.controls.Count; i++)
            {
                TrayControl control = (TrayControl) this.controls[i];
                if (control.Component == component)
                {
                    int num2 = forward ? (i + 1) : (i - 1);
                    if ((num2 >= 0) && (num2 < this.controls.Count))
                    {
                        return ((TrayControl) this.controls[num2]).Component;
                    }
                    return null;
                }
            }
            if (this.controls.Count > 0)
            {
                int num3 = forward ? 0 : (this.controls.Count - 1);
                return ((TrayControl) this.controls[num3]).Component;
            }
            return null;
        }

        internal virtual OleDragDropHandler GetOleDragHandler()
        {
            if (this.oleDragDropHandler == null)
            {
                this.oleDragDropHandler = new TrayOleDragDropHandler(this.DragHandler, this.serviceProvider, this);
            }
            return this.oleDragDropHandler;
        }

        protected override object GetService(System.Type serviceType)
        {
            object service = null;
            if (this.serviceProvider != null)
            {
                service = this.serviceProvider.GetService(serviceType);
            }
            return service;
        }

        internal TrayControl GetTrayControlFromComponent(IComponent comp)
        {
            return TrayControl.FromComponent(comp);
        }

        [DesignOnly(true), Localizable(false), System.Design.SRDescription("ControlLocationDescr"), Browsable(false), Category("Layout")]
        public Point GetTrayLocation(IComponent receiver)
        {
            Control control = TrayControl.FromComponent(receiver);
            if (control == null)
            {
                return new Point();
            }
            Point location = control.Location;
            Point autoScrollPosition = base.AutoScrollPosition;
            return new Point(location.X - autoScrollPosition.X, location.Y - autoScrollPosition.Y);
        }

        public bool IsTrayComponent(IComponent comp)
        {
            if (TrayControl.FromComponent(comp) != null)
            {
                foreach (Control control in base.Controls)
                {
                    TrayControl control2 = control as TrayControl;
                    if ((control2 != null) && (control2.Component == comp))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnComponentRefresh(RefreshEventArgs e)
        {
            IComponent componentChanged = e.ComponentChanged as IComponent;
            if (componentChanged != null)
            {
                TrayControl control = TrayControl.FromComponent(componentChanged);
                if (control != null)
                {
                    bool flag = this.CanDisplayComponent(componentChanged);
                    if ((flag != control.Visible) || !flag)
                    {
                        control.Visible = flag;
                        Rectangle bounds = control.Bounds;
                        bounds.Inflate(this.grabHandle);
                        bounds.Inflate(this.grabHandle);
                        base.Invalidate(bounds);
                        base.PerformLayout();
                    }
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs cevent)
        {
            this.RemoveComponent(cevent.Component);
        }

        private void OnContextMenu(int x, int y, bool useSelection)
        {
            if (!this.TabOrderActive)
            {
                base.Capture = false;
                IMenuCommandService menuService = this.MenuService;
                if (menuService != null)
                {
                    base.Capture = false;
                    Cursor.Clip = Rectangle.Empty;
                    ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                    if ((useSelection && (service != null)) && ((1 != service.SelectionCount) || (service.PrimarySelection != this.mainDesigner.Component)))
                    {
                        menuService.ShowContextMenu(MenuCommands.TraySelectionMenu, x, y);
                    }
                    else
                    {
                        menuService.ShowContextMenu(MenuCommands.ComponentTrayMenu, x, y);
                    }
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            this.mouseDropLocation = base.PointToClient(new Point(de.X, de.Y));
            this.autoScrollPosBeforeDragging = base.AutoScrollPosition;
            if (this.mouseDragTool != null)
            {
                ToolboxItem mouseDragTool = this.mouseDragTool;
                this.mouseDragTool = null;
                bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                try
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    IToolboxUser designer = service.GetDesigner(service.RootComponent) as IToolboxUser;
                    if (designer != null)
                    {
                        designer.ToolPicked(mouseDragTool);
                    }
                    else
                    {
                        this.CreateComponentFromTool(mouseDragTool);
                    }
                }
                catch (Exception exception)
                {
                    this.DisplayError(exception);
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
                de.Effect = DragDropEffects.Copy;
            }
            else
            {
                this.GetOleDragHandler().DoOleDragDrop(de);
            }
            this.mouseDropLocation = InvalidPoint;
            base.ResumeLayout();
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            if (!this.TabOrderActive)
            {
                base.SuspendLayout();
                if (this.toolboxService == null)
                {
                    this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
                }
                OleDragDropHandler oleDragHandler = this.GetOleDragHandler();
                object[] draggingObjects = oleDragHandler.GetDraggingObjects(de);
                if ((this.toolboxService != null) && (draggingObjects == null))
                {
                    this.mouseDragTool = this.toolboxService.DeserializeToolboxItem(de.Data, (IDesignerHost) this.GetService(typeof(IDesignerHost)));
                }
                if (this.mouseDragTool != null)
                {
                    if ((de.AllowedEffect & DragDropEffects.Move) != DragDropEffects.None)
                    {
                        de.Effect = DragDropEffects.Move;
                    }
                    else
                    {
                        de.Effect = DragDropEffects.Copy;
                    }
                }
                else
                {
                    oleDragHandler.DoOleDragEnter(de);
                }
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            this.mouseDragTool = null;
            this.GetOleDragHandler().DoOleDragLeave();
            base.ResumeLayout();
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            if (this.mouseDragTool != null)
            {
                de.Effect = DragDropEffects.Copy;
            }
            else
            {
                this.GetOleDragHandler().DoOleDragOver(de);
            }
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs gfevent)
        {
            base.OnGiveFeedback(gfevent);
            this.GetOleDragHandler().DoOleGiveFeedback(gfevent);
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            this.DoAutoArrange(false);
            base.Invalidate(true);
            base.OnLayout(levent);
        }

        protected virtual void OnLostCapture()
        {
            if (this.mouseDragStart != InvalidPoint)
            {
                Cursor.Clip = Rectangle.Empty;
                if (this.mouseDragEnd != InvalidPoint)
                {
                    this.DrawRubber(this.mouseDragStart, this.mouseDragEnd);
                    this.mouseDragEnd = InvalidPoint;
                }
                this.mouseDragStart = InvalidPoint;
            }
        }

        private void OnMenuArrangeIcons(object sender, EventArgs e)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            DesignerTransaction transaction = null;
            try
            {
                transaction = service.CreateTransaction(System.Design.SR.GetString("TrayAutoArrange"));
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.mainDesigner.Component)["TrayAutoArrange"];
                if (descriptor != null)
                {
                    descriptor.SetValue(this.mainDesigner.Component, !this.AutoArrange);
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
        }

        private void OnMenuLineupIcons(object sender, EventArgs e)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            DesignerTransaction transaction = null;
            try
            {
                transaction = service.CreateTransaction(System.Design.SR.GetString("TrayLineUpIcons"));
                this.DoLineupIcons();
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
        }

        private void OnMenuShowLargeIcons(object sender, EventArgs e)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            DesignerTransaction transaction = null;
            try
            {
                transaction = service.CreateTransaction(System.Design.SR.GetString("TrayShowLargeIcons"));
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.mainDesigner.Component)["TrayLargeIcon"];
                if (descriptor != null)
                {
                    descriptor.SetValue(this.mainDesigner.Component, !this.ShowLargeIcons);
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
        }

        internal void OnMessage(ref Message m)
        {
            this.WndProc(ref m);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if ((this.glyphManager == null) || !this.glyphManager.OnMouseDoubleClick(e))
            {
                base.OnDoubleClick(e);
                if (!this.TabOrderActive)
                {
                    this.OnLostCapture();
                    IEventBindingService service = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                    bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                    if (service != null)
                    {
                        service.ShowCode();
                    }
                }
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((this.glyphManager == null) || !this.glyphManager.OnMouseDown(e))
            {
                base.OnMouseDown(e);
                if (!this.TabOrderActive)
                {
                    if (this.toolboxService == null)
                    {
                        this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
                    }
                    this.FocusDesigner();
                    if ((e.Button == MouseButtons.Left) && (this.toolboxService != null))
                    {
                        ToolboxItem selectedToolboxItem = this.toolboxService.GetSelectedToolboxItem((IDesignerHost) this.GetService(typeof(IDesignerHost)));
                        if (selectedToolboxItem != null)
                        {
                            this.mouseDropLocation = new Point(e.X, e.Y);
                            try
                            {
                                this.CreateComponentFromTool(selectedToolboxItem);
                                this.toolboxService.SelectedToolboxItemUsed();
                            }
                            catch (Exception exception)
                            {
                                this.DisplayError(exception);
                                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                                {
                                    throw;
                                }
                            }
                            this.mouseDropLocation = InvalidPoint;
                            return;
                        }
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        this.mouseDragStart = new Point(e.X, e.Y);
                        base.Capture = true;
                        Cursor.Clip = base.RectangleToScreen(base.ClientRectangle);
                    }
                    else
                    {
                        try
                        {
                            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                            bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                            if (service != null)
                            {
                                service.SetSelectedComponents(new object[] { this.mainDesigner.Component });
                            }
                        }
                        catch (Exception exception2)
                        {
                            if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if ((this.glyphManager == null) || !this.glyphManager.OnMouseMove(e))
            {
                base.OnMouseMove(e);
                if (this.mouseDragStart != InvalidPoint)
                {
                    if (this.mouseDragEnd != InvalidPoint)
                    {
                        this.DrawRubber(this.mouseDragStart, this.mouseDragEnd);
                    }
                    else
                    {
                        this.mouseDragEnd = new Point(0, 0);
                    }
                    this.mouseDragEnd.X = e.X;
                    this.mouseDragEnd.Y = e.Y;
                    this.DrawRubber(this.mouseDragStart, this.mouseDragEnd);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if ((this.glyphManager == null) || !this.glyphManager.OnMouseUp(e))
            {
                if ((this.mouseDragStart != InvalidPoint) && (e.Button == MouseButtons.Left))
                {
                    object[] components = null;
                    base.Capture = false;
                    Cursor.Clip = Rectangle.Empty;
                    if (this.mouseDragEnd != InvalidPoint)
                    {
                        this.DrawRubber(this.mouseDragStart, this.mouseDragEnd);
                        Rectangle rect = new Rectangle {
                            X = Math.Min(this.mouseDragStart.X, e.X),
                            Y = Math.Min(this.mouseDragStart.Y, e.Y),
                            Width = Math.Abs((int) (e.X - this.mouseDragStart.X)),
                            Height = Math.Abs((int) (e.Y - this.mouseDragStart.Y))
                        };
                        components = this.GetComponentsInRect(rect);
                        this.mouseDragEnd = InvalidPoint;
                    }
                    else
                    {
                        components = new object[0];
                    }
                    if (components.Length == 0)
                    {
                        components = new object[] { this.mainDesigner.Component };
                    }
                    try
                    {
                        ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                        bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                        if (service != null)
                        {
                            service.SetSelectedComponents(components);
                        }
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                    this.mouseDragStart = InvalidPoint;
                }
                base.OnMouseUp(e);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            if (this.fResetAmbient)
            {
                this.fResetAmbient = false;
                IUIService service = (IUIService) this.GetService(typeof(IUIService));
                if (service != null)
                {
                    System.Drawing.Color info;
                    if (service.Styles["VsColorDesignerTray"] is System.Drawing.Color)
                    {
                        info = (System.Drawing.Color) service.Styles["VsColorDesignerTray"];
                    }
                    else if (service.Styles["HighlightColor"] is System.Drawing.Color)
                    {
                        info = (System.Drawing.Color) service.Styles["HighlightColor"];
                    }
                    else
                    {
                        info = SystemColors.Info;
                    }
                    this.BackColor = info;
                    this.Font = (Font) service.Styles["DialogFont"];
                }
            }
            base.OnPaint(pe);
            Graphics graphics = pe.Graphics;
            if (this.selectedObjects != null)
            {
                bool primarySelection = true;
                foreach (object obj2 in this.selectedObjects)
                {
                    Control controlForComponent = ((IOleDragClient) this).GetControlForComponent(obj2);
                    if ((controlForComponent != null) && controlForComponent.Visible)
                    {
                        Rectangle bounds = controlForComponent.Bounds;
                        NoResizeHandleGlyph glyph = new NoResizeHandleGlyph(bounds, SelectionRules.None, primarySelection, null);
                        DesignerUtils.DrawSelectionBorder(graphics, DesignerUtils.GetBoundsForNoResizeSelectionType(bounds, SelectionBorderGlyphType.Top));
                        DesignerUtils.DrawSelectionBorder(graphics, DesignerUtils.GetBoundsForNoResizeSelectionType(bounds, SelectionBorderGlyphType.Bottom));
                        DesignerUtils.DrawSelectionBorder(graphics, DesignerUtils.GetBoundsForNoResizeSelectionType(bounds, SelectionBorderGlyphType.Left));
                        DesignerUtils.DrawSelectionBorder(graphics, DesignerUtils.GetBoundsForNoResizeSelectionType(bounds, SelectionBorderGlyphType.Right));
                        DesignerUtils.DrawNoResizeHandle(graphics, glyph.Bounds, primarySelection, glyph);
                    }
                    primarySelection = false;
                }
            }
            if (this.glyphManager != null)
            {
                this.glyphManager.OnPaintGlyphs(pe);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            this.selectedObjects = ((ISelectionService) sender).GetSelectedComponents();
            object primarySelection = ((ISelectionService) sender).PrimarySelection;
            base.Invalidate();
            foreach (object obj3 in this.selectedObjects)
            {
                IComponent component = obj3 as IComponent;
                if (component != null)
                {
                    Control wrapper = TrayControl.FromComponent(component);
                    if (wrapper != null)
                    {
                        System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8007, new HandleRef(wrapper, wrapper.Handle), -4, 0);
                    }
                }
            }
            IComponent component2 = primarySelection as IComponent;
            if (component2 != null)
            {
                Control activeControl = TrayControl.FromComponent(component2);
                if ((activeControl != null) && base.IsHandleCreated)
                {
                    base.ScrollControlIntoView(activeControl);
                    System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(activeControl, activeControl.Handle), -4, 0);
                }
                if (this.glyphManager != null)
                {
                    this.glyphManager.SelectionGlyphs.Clear();
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    foreach (object obj4 in this.selectedObjects)
                    {
                        IComponent component3 = obj4 as IComponent;
                        if ((component3 != null) && !(service.GetDesigner(component3) is ControlDesigner))
                        {
                            GlyphCollection glyphsForComponent = this.glyphManager.GetGlyphsForComponent(component3);
                            if ((glyphsForComponent != null) && (glyphsForComponent.Count > 0))
                            {
                                this.SelectionGlyphs.AddRange(glyphsForComponent);
                            }
                        }
                    }
                }
            }
        }

        protected virtual void OnSetCursor()
        {
            if (this.toolboxService == null)
            {
                this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
            }
            if ((this.toolboxService == null) || !this.toolboxService.SetCursor())
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnSystemSettingChanged(object sender, EventArgs e)
        {
            this.fResetAmbient = true;
            this.ResetTrayControls();
            base.BeginInvoke(new AsyncInvokeHandler(this.Invalidate), new object[] { true });
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            this.fResetAmbient = true;
            this.ResetTrayControls();
            base.BeginInvoke(new AsyncInvokeHandler(this.Invalidate), new object[] { true });
        }

        private void PositionControl(TrayControl c)
        {
            if (!this.autoArrange)
            {
                if (this.mouseDropLocation == InvalidPoint)
                {
                    Control prevCtl = null;
                    if (this.controls.Count > 1)
                    {
                        int index = this.controls.IndexOf(c);
                        if (index >= 1)
                        {
                            prevCtl = (Control) this.controls[index - 1];
                        }
                    }
                    this.PositionInNextAutoSlot(c, prevCtl, true);
                }
                else if (!c.Location.Equals(this.mouseDropLocation))
                {
                    c.Location = this.mouseDropLocation;
                }
            }
            else if (this.mouseDropLocation != InvalidPoint)
            {
                this.RearrangeInAutoSlots(c, this.mouseDropLocation);
            }
            else
            {
                Control control2 = null;
                if (this.controls.Count > 1)
                {
                    int num2 = this.controls.IndexOf(c);
                    if (num2 >= 1)
                    {
                        control2 = (Control) this.controls[num2 - 1];
                    }
                }
                this.PositionInNextAutoSlot(c, control2, true);
            }
        }

        private bool PositionInNextAutoSlot(TrayControl c, Control prevCtl, bool dirtyDesigner)
        {
            if (this.whiteSpace.IsEmpty)
            {
                this.whiteSpace = new Point(this.selectionUISvc.GetAdornmentDimensions(AdornmentType.GrabHandle));
                this.whiteSpace.X = (this.whiteSpace.X * 2) + 3;
                this.whiteSpace.Y = (this.whiteSpace.Y * 2) + 3;
            }
            if (prevCtl == null)
            {
                Rectangle displayRectangle = this.DisplayRectangle;
                Point point = new Point(displayRectangle.X + this.whiteSpace.X, displayRectangle.Y + this.whiteSpace.Y);
                if (!c.Location.Equals(point))
                {
                    c.Location = point;
                    if (dirtyDesigner)
                    {
                        IComponent component = c.Component;
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["TrayLocation"];
                        if (descriptor != null)
                        {
                            Point autoScrollPosition = base.AutoScrollPosition;
                            point = new Point(point.X - autoScrollPosition.X, point.Y - autoScrollPosition.Y);
                            descriptor.SetValue(component, point);
                        }
                    }
                    else
                    {
                        c.Location = point;
                    }
                    return true;
                }
            }
            else
            {
                Rectangle bounds = prevCtl.Bounds;
                Point point3 = new Point((bounds.X + bounds.Width) + this.whiteSpace.X, bounds.Y);
                if ((point3.X + c.Size.Width) > base.Size.Width)
                {
                    point3.X = this.whiteSpace.X;
                    point3.Y += bounds.Height + this.whiteSpace.Y;
                }
                if (!c.Location.Equals(point3))
                {
                    if (dirtyDesigner)
                    {
                        IComponent component2 = c.Component;
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component2)["TrayLocation"];
                        if (descriptor2 != null)
                        {
                            Point point4 = base.AutoScrollPosition;
                            point3 = new Point(point3.X - point4.X, point3.Y - point4.Y);
                            descriptor2.SetValue(component2, point3);
                        }
                    }
                    else
                    {
                        c.Location = point3;
                    }
                    return true;
                }
            }
            return false;
        }

        internal void RearrangeInAutoSlots(Control c, Point pos)
        {
            TrayControl control = (TrayControl) c;
            control.Positioned = true;
            control.Location = pos;
        }

        public virtual void RemoveComponent(IComponent component)
        {
            TrayControl c = TrayControl.FromComponent(component);
            if (c != null)
            {
                try
                {
                    if ((c.InheritanceAttribute.InheritanceLevel != InheritanceLevel.NotInherited) && (this.inheritanceUI != null))
                    {
                        this.inheritanceUI.RemoveInheritedControl(c);
                    }
                    if (this.controls != null)
                    {
                        int index = this.controls.IndexOf(c);
                        if (index != -1)
                        {
                            this.controls.RemoveAt(index);
                        }
                    }
                }
                finally
                {
                    c.Dispose();
                }
            }
        }

        private void ResetTrayControls()
        {
            Control.ControlCollection controls = base.Controls;
            if (controls != null)
            {
                for (int i = 0; i < controls.Count; i++)
                {
                    TrayControl control = controls[i] as TrayControl;
                    if (control != null)
                    {
                        control.fRecompute = true;
                    }
                }
            }
        }

        public void SetLocation(IComponent receiver, Point location)
        {
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((service != null) && service.Loading)
            {
                this.SetTrayLocation(receiver, location);
            }
            else
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(receiver.GetType())["Location"];
                if (descriptor != null)
                {
                    descriptor.SetValue(receiver, location);
                }
                else
                {
                    this.SetTrayLocation(receiver, location);
                }
            }
        }

        public void SetTrayLocation(IComponent receiver, Point location)
        {
            TrayControl c = TrayControl.FromComponent(receiver);
            if (c != null)
            {
                if (c.Parent == this)
                {
                    Point autoScrollPosition = base.AutoScrollPosition;
                    location = new Point(location.X + autoScrollPosition.X, location.Y + autoScrollPosition.Y);
                    if (c.Visible)
                    {
                        this.RearrangeInAutoSlots(c, location);
                    }
                }
                else if (!c.Location.Equals(location))
                {
                    c.Location = location;
                    c.Positioned = true;
                }
            }
        }

        bool IExtenderProvider.CanExtend(object component)
        {
            IComponent component2 = component as IComponent;
            return ((component2 != null) && (TrayControl.FromComponent(component2) != null));
        }

        bool IOleDragClient.AddComponent(IComponent component, string name, bool firstAdd)
        {
            IOleDragClient mainDesigner = this.mainDesigner as IOleDragClient;
            if (mainDesigner != null)
            {
                try
                {
                    mainDesigner.AddComponent(component, name, firstAdd);
                    this.PositionControl(TrayControl.FromComponent(component));
                    this.mouseDropLocation = InvalidPoint;
                    return true;
                }
                catch
                {
                    goto Label_0084;
                }
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            try
            {
                if ((service != null) && (service.Container != null))
                {
                    if (service.Container.Components[name] != null)
                    {
                        name = null;
                    }
                    service.Container.Add(component, name);
                    return true;
                }
            }
            catch
            {
            }
        Label_0084:
            return false;
        }

        Control IOleDragClient.GetControlForComponent(object component)
        {
            IComponent component2 = component as IComponent;
            if (component2 != null)
            {
                return TrayControl.FromComponent(component2);
            }
            return null;
        }

        Control IOleDragClient.GetDesignerControl()
        {
            return this;
        }

        bool IOleDragClient.IsDropOk(IComponent component)
        {
            return true;
        }

        bool ISelectionUIHandler.BeginDrag(object[] components, SelectionRules rules, int initialX, int initialY)
        {
            if (this.TabOrderActive)
            {
                return false;
            }
            bool flag = this.DragHandler.BeginDrag(components, rules, initialX, initialY);
            if (flag && !this.GetOleDragHandler().DoBeginDrag(components, rules, initialX, initialY))
            {
                return false;
            }
            return flag;
        }

        void ISelectionUIHandler.DragMoved(object[] components, Rectangle offset)
        {
            this.DragHandler.DragMoved(components, offset);
        }

        void ISelectionUIHandler.EndDrag(object[] components, bool cancel)
        {
            this.DragHandler.EndDrag(components, cancel);
            this.GetOleDragHandler().DoEndDrag(components, cancel);
            if (!this.autoScrollPosBeforeDragging.IsEmpty)
            {
                foreach (IComponent component in components)
                {
                    TrayControl control = TrayControl.FromComponent(component);
                    if (control != null)
                    {
                        this.SetTrayLocation(component, new Point(control.Location.X - this.autoScrollPosBeforeDragging.X, control.Location.Y - this.autoScrollPosBeforeDragging.Y));
                    }
                }
                base.AutoScrollPosition = new Point(-this.autoScrollPosBeforeDragging.X, -this.autoScrollPosBeforeDragging.Y);
            }
        }

        Rectangle ISelectionUIHandler.GetComponentBounds(object component)
        {
            return Rectangle.Empty;
        }

        SelectionRules ISelectionUIHandler.GetComponentRules(object component)
        {
            return (SelectionRules.Visible | SelectionRules.Moveable);
        }

        Rectangle ISelectionUIHandler.GetSelectionClipRect(object component)
        {
            if (base.IsHandleCreated)
            {
                return base.RectangleToScreen(base.ClientRectangle);
            }
            return Rectangle.Empty;
        }

        void ISelectionUIHandler.OleDragDrop(DragEventArgs de)
        {
            this.GetOleDragHandler().DoOleDragDrop(de);
        }

        void ISelectionUIHandler.OleDragEnter(DragEventArgs de)
        {
            this.GetOleDragHandler().DoOleDragEnter(de);
        }

        void ISelectionUIHandler.OleDragLeave()
        {
            this.GetOleDragHandler().DoOleDragLeave();
        }

        void ISelectionUIHandler.OleDragOver(DragEventArgs de)
        {
            this.GetOleDragHandler().DoOleDragOver(de);
        }

        void ISelectionUIHandler.OnSelectionDoubleClick(IComponent component)
        {
            if (!this.TabOrderActive)
            {
                TrayControl controlForComponent = ((IOleDragClient) this).GetControlForComponent(component) as TrayControl;
                if (controlForComponent != null)
                {
                    controlForComponent.ViewDefaultEvent(component);
                }
            }
        }

        bool ISelectionUIHandler.QueryBeginDrag(object[] components, SelectionRules rules, int initialX, int initialY)
        {
            return this.DragHandler.QueryBeginDrag(components, rules, initialX, initialY);
        }

        void ISelectionUIHandler.ShowContextMenu(IComponent component)
        {
            Point mousePosition = Control.MousePosition;
            this.OnContextMenu(mousePosition.X, mousePosition.Y, true);
        }

        internal void UpdatePastePositions(ArrayList components)
        {
            foreach (TrayControl control in components)
            {
                if (!this.CanDisplayComponent(control.Component))
                {
                    break;
                }
                if (this.mouseDropLocation == InvalidPoint)
                {
                    Control prevCtl = null;
                    if (this.controls.Count > 1)
                    {
                        prevCtl = (Control) this.controls[this.controls.Count - 1];
                    }
                    this.PositionInNextAutoSlot(control, prevCtl, true);
                }
                else
                {
                    this.PositionControl(control);
                }
                control.BringToFront();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x114:
                case 0x115:
                    base.WndProc(ref m);
                    if (this.selectionUISvc != null)
                    {
                        this.selectionUISvc.SyncSelection();
                    }
                    return;

                case 0x84:
                    if (this.glyphManager != null)
                    {
                        Point p = new Point((short) System.Design.NativeMethods.Util.LOWORD((int) ((long) m.LParam)), (short) System.Design.NativeMethods.Util.HIWORD((int) ((long) m.LParam)));
                        System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                            x = 0,
                            y = 0
                        };
                        System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, base.Handle, pt, 1);
                        p.Offset(pt.x, pt.y);
                        this.glyphManager.GetHitTest(p);
                    }
                    base.WndProc(ref m);
                    return;

                case 0x1f:
                    this.OnLostCapture();
                    return;

                case 0x20:
                    this.OnSetCursor();
                    return;

                case 0x7b:
                {
                    int x = System.Design.NativeMethods.Util.SignedLOWORD((int) ((long) m.LParam));
                    int y = System.Design.NativeMethods.Util.SignedHIWORD((int) ((long) m.LParam));
                    if ((x == -1) && (y == -1))
                    {
                        Point mousePosition = Control.MousePosition;
                        x = mousePosition.X;
                        y = mousePosition.Y;
                    }
                    this.OnContextMenu(x, y, true);
                    return;
                }
                case 0x7d:
                    base.Invalidate();
                    return;
            }
            base.WndProc(ref m);
        }

        public bool AutoArrange
        {
            get
            {
                return this.autoArrange;
            }
            set
            {
                if (this.autoArrange != value)
                {
                    this.autoArrange = value;
                    this.menucmdArrangeIcons.Checked = value;
                    if (this.autoArrange)
                    {
                        this.DoAutoArrange(true);
                    }
                }
            }
        }

        public int ComponentCount
        {
            get
            {
                return base.Controls.Count;
            }
        }

        internal virtual SelectionUIHandler DragHandler
        {
            get
            {
                if (this.dragHandler == null)
                {
                    this.dragHandler = new TraySelectionUIHandler(this);
                }
                return this.dragHandler;
            }
        }

        private System.Windows.Forms.Design.InheritanceUI InheritanceUI
        {
            get
            {
                if (this.inheritanceUI == null)
                {
                    this.inheritanceUI = new System.Windows.Forms.Design.InheritanceUI();
                }
                return this.inheritanceUI;
            }
        }

        internal bool IsWindowVisible
        {
            get
            {
                return (base.IsHandleCreated && System.Design.NativeMethods.IsWindowVisible(base.Handle));
            }
        }

        private IMenuCommandService MenuService
        {
            get
            {
                if (this.menuCommandService == null)
                {
                    this.menuCommandService = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                }
                return this.menuCommandService;
            }
        }

        internal Size ParentGridSize
        {
            get
            {
                ParentControlDesigner mainDesigner = this.mainDesigner as ParentControlDesigner;
                if (mainDesigner != null)
                {
                    return mainDesigner.ParentGridSize;
                }
                return new Size(8, 8);
            }
        }

        internal GlyphCollection SelectionGlyphs
        {
            get
            {
                if (this.glyphManager != null)
                {
                    return this.glyphManager.SelectionGlyphs;
                }
                return null;
            }
        }

        public bool ShowLargeIcons
        {
            get
            {
                return this.showLargeIcons;
            }
            set
            {
                if (this.showLargeIcons != value)
                {
                    this.showLargeIcons = value;
                    this.menucmdLargeIcons.Checked = this.ShowLargeIcons;
                    this.ResetTrayControls();
                    base.Invalidate(true);
                }
            }
        }

        bool IOleDragClient.CanModifyComponents
        {
            get
            {
                return true;
            }
        }

        IComponent IOleDragClient.Component
        {
            get
            {
                return this.mainDesigner.Component;
            }
        }

        private bool TabOrderActive
        {
            get
            {
                if (!this.queriedTabOrder)
                {
                    this.queriedTabOrder = true;
                    IMenuCommandService menuService = this.MenuService;
                    if (menuService != null)
                    {
                        this.tabOrderCommand = menuService.FindCommand(StandardCommands.TabOrder);
                    }
                }
                return ((this.tabOrderCommand != null) && this.tabOrderCommand.Checked);
            }
        }

        private delegate void AsyncInvokeHandler(bool children);

        internal class AutoArrangeComparer : IComparer
        {
            int IComparer.Compare(object o1, object o2)
            {
                Point location = ((Control) o1).Location;
                Point point2 = ((Control) o2).Location;
                int num = ((Control) o1).Height / 2;
                if ((location.X == point2.X) && (location.Y == point2.Y))
                {
                    return 0;
                }
                if ((location.Y + num) > point2.Y)
                {
                    if ((point2.Y + num) <= location.Y)
                    {
                        return 1;
                    }
                    if (location.X > point2.X)
                    {
                        return 1;
                    }
                }
                return -1;
            }
        }

        private class ComponentTrayGlyphManager
        {
            private BehaviorService behaviorSvc;
            private Glyph hitTestedGlyph;
            private ISelectionService selSvc;
            private Adorner traySelectionAdorner;

            public ComponentTrayGlyphManager(ISelectionService selSvc, BehaviorService behaviorSvc)
            {
                this.selSvc = selSvc;
                this.behaviorSvc = behaviorSvc;
                this.traySelectionAdorner = new Adorner();
            }

            public void Dispose()
            {
                if (this.traySelectionAdorner != null)
                {
                    this.traySelectionAdorner.Glyphs.Clear();
                    this.traySelectionAdorner = null;
                }
            }

            public GlyphCollection GetGlyphsForComponent(IComponent comp)
            {
                GlyphCollection glyphs = new GlyphCollection();
                if (((this.behaviorSvc != null) && (comp != null)) && (this.behaviorSvc.DesignerActionUI != null))
                {
                    Glyph designerActionGlyph = this.behaviorSvc.DesignerActionUI.GetDesignerActionGlyph(comp);
                    if (designerActionGlyph != null)
                    {
                        glyphs.Add(designerActionGlyph);
                    }
                }
                return glyphs;
            }

            public Cursor GetHitTest(Point p)
            {
                for (int i = 0; i < this.traySelectionAdorner.Glyphs.Count; i++)
                {
                    Cursor hitTest = this.traySelectionAdorner.Glyphs[i].GetHitTest(p);
                    if (hitTest != null)
                    {
                        this.hitTestedGlyph = this.traySelectionAdorner.Glyphs[i];
                        return hitTest;
                    }
                }
                this.hitTestedGlyph = null;
                return null;
            }

            public bool OnMouseDoubleClick(MouseEventArgs e)
            {
                return (((this.hitTestedGlyph != null) && (this.hitTestedGlyph.Behavior != null)) && this.hitTestedGlyph.Behavior.OnMouseDoubleClick(this.hitTestedGlyph, e.Button, new Point(e.X, e.Y)));
            }

            public bool OnMouseDown(MouseEventArgs e)
            {
                return (((this.hitTestedGlyph != null) && (this.hitTestedGlyph.Behavior != null)) && this.hitTestedGlyph.Behavior.OnMouseDown(this.hitTestedGlyph, e.Button, new Point(e.X, e.Y)));
            }

            public bool OnMouseMove(MouseEventArgs e)
            {
                return (((this.hitTestedGlyph != null) && (this.hitTestedGlyph.Behavior != null)) && this.hitTestedGlyph.Behavior.OnMouseMove(this.hitTestedGlyph, e.Button, new Point(e.X, e.Y)));
            }

            public bool OnMouseUp(MouseEventArgs e)
            {
                return (((this.hitTestedGlyph != null) && (this.hitTestedGlyph.Behavior != null)) && this.hitTestedGlyph.Behavior.OnMouseUp(this.hitTestedGlyph, e.Button));
            }

            public void OnPaintGlyphs(PaintEventArgs pe)
            {
                foreach (Glyph glyph in this.traySelectionAdorner.Glyphs)
                {
                    glyph.Paint(pe);
                }
            }

            public void UpdateLocation(ComponentTray.TrayControl trayControl)
            {
                foreach (Glyph glyph in this.traySelectionAdorner.Glyphs)
                {
                    DesignerActionGlyph glyph2 = glyph as DesignerActionGlyph;
                    if ((glyph2 != null) && ((DesignerActionBehavior) glyph2.Behavior).RelatedComponent.Equals(trayControl.Component))
                    {
                        glyph2.UpdateAlternativeBounds(trayControl.Bounds);
                    }
                }
            }

            public GlyphCollection SelectionGlyphs
            {
                get
                {
                    return this.traySelectionAdorner.Glyphs;
                }
            }
        }

        internal class TrayControl : Control
        {
            private int borderWidth;
            private IComponent component;
            private bool ctrlSelect;
            private int cxIcon;
            private int cyIcon;
            internal bool fRecompute;
            private System.ComponentModel.InheritanceAttribute inheritanceAttribute;
            private Point mouseDragLast = ComponentTray.InvalidPoint;
            private bool mouseDragMoved;
            private bool positioned;
            private Image toolboxBitmap;
            private ComponentTray tray;
            private const int whiteSpace = 5;

            public TrayControl(ComponentTray tray, IComponent component)
            {
                this.tray = tray;
                this.component = component;
                base.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                base.SetStyle(ControlStyles.Selectable, false);
                this.borderWidth = SystemInformation.BorderSize.Width;
                this.UpdateIconInfo();
                IComponentChangeService service = (IComponentChangeService) tray.GetService(typeof(IComponentChangeService));
                bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                if (service != null)
                {
                    service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
                }
                ISite site = component.Site;
                string name = null;
                if (site != null)
                {
                    name = site.Name;
                    IDictionaryService service2 = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    if (service2 != null)
                    {
                        service2.SetValue(base.GetType(), this);
                    }
                }
                if (name == null)
                {
                    name = component.GetType().Name;
                }
                this.Text = name;
                this.inheritanceAttribute = (System.ComponentModel.InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(System.ComponentModel.InheritanceAttribute)];
                base.TabStop = false;
            }

            private void AdjustSize(bool autoArrange)
            {
                using (Graphics graphics = base.CreateGraphics())
                {
                    Size size = Size.Ceiling(graphics.MeasureString(this.Text, this.Font));
                    Rectangle bounds = base.Bounds;
                    if (this.tray.ShowLargeIcons)
                    {
                        bounds.Width = (Math.Max(this.cxIcon, size.Width) + (4 * this.borderWidth)) + 10;
                        bounds.Height = ((this.cyIcon + 10) + size.Height) + (4 * this.borderWidth);
                    }
                    else
                    {
                        bounds.Width = ((this.cxIcon + size.Width) + (4 * this.borderWidth)) + 10;
                        bounds.Height = Math.Max(this.cyIcon, size.Height) + (4 * this.borderWidth);
                    }
                    base.Bounds = bounds;
                    base.Invalidate();
                }
                if (this.tray.glyphManager != null)
                {
                    this.tray.glyphManager.UpdateLocation(this);
                }
            }

            protected override AccessibleObject CreateAccessibilityInstance()
            {
                return new TrayControlAccessibleObject(this, this.tray);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    ISite site = this.component.Site;
                    if (site != null)
                    {
                        IComponentChangeService service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                        bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                        if (service != null)
                        {
                            service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                        }
                        IDictionaryService service2 = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                        bool flag2 = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                        if (service2 != null)
                        {
                            service2.SetValue(typeof(ComponentTray.TrayControl), null);
                        }
                    }
                }
                base.Dispose(disposing);
            }

            public static ComponentTray.TrayControl FromComponent(IComponent component)
            {
                ComponentTray.TrayControl control = null;
                if (component == null)
                {
                    return null;
                }
                ISite site = component.Site;
                if (site != null)
                {
                    IDictionaryService service = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                    if (service != null)
                    {
                        control = (ComponentTray.TrayControl) service.GetValue(typeof(ComponentTray.TrayControl));
                    }
                }
                return control;
            }

            private void OnComponentRename(object sender, ComponentRenameEventArgs e)
            {
                if (e.Component == this.component)
                {
                    this.Text = e.NewName;
                    this.AdjustSize(true);
                }
            }

            private void OnContextMenu(int x, int y)
            {
                if (!this.tray.TabOrderActive)
                {
                    base.Capture = false;
                    ISelectionService service = (ISelectionService) this.tray.GetService(typeof(ISelectionService));
                    if ((service != null) && !service.GetComponentSelected(this.component))
                    {
                        service.SetSelectedComponents(new object[] { this.component }, SelectionTypes.Replace);
                    }
                    IMenuCommandService menuService = this.tray.MenuService;
                    if (menuService != null)
                    {
                        base.Capture = false;
                        Cursor.Clip = Rectangle.Empty;
                        menuService.ShowContextMenu(MenuCommands.TraySelectionMenu, x, y);
                    }
                }
            }

            protected override void OnDoubleClick(EventArgs e)
            {
                base.OnDoubleClick(e);
                if (!this.tray.TabOrderActive)
                {
                    IDesignerHost service = (IDesignerHost) this.tray.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        this.mouseDragLast = ComponentTray.InvalidPoint;
                        base.Capture = false;
                        IDesigner designer = service.GetDesigner(this.component);
                        if (designer == null)
                        {
                            this.ViewDefaultEvent(this.component);
                        }
                        else
                        {
                            designer.DoDefaultAction();
                        }
                    }
                }
            }

            private void OnEndDrag(bool cancel)
            {
                this.mouseDragLast = ComponentTray.InvalidPoint;
                if (!this.mouseDragMoved)
                {
                    if (this.ctrlSelect)
                    {
                        ISelectionService service = (ISelectionService) this.tray.GetService(typeof(ISelectionService));
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { this.Component }, SelectionTypes.Click);
                        }
                        this.ctrlSelect = false;
                    }
                }
                else
                {
                    this.mouseDragMoved = false;
                    this.ctrlSelect = false;
                    base.Capture = false;
                    this.OnSetCursor();
                    if ((this.tray.selectionUISvc != null) && this.tray.selectionUISvc.Dragging)
                    {
                        this.tray.selectionUISvc.EndDrag(cancel);
                    }
                }
            }

            protected override void OnFontChanged(EventArgs e)
            {
                this.AdjustSize(true);
                base.OnFontChanged(e);
            }

            protected override void OnHandleCreated(EventArgs e)
            {
                base.OnHandleCreated(e);
                this.AdjustSize(false);
            }

            protected override void OnLocationChanged(EventArgs e)
            {
                if (this.tray.glyphManager != null)
                {
                    this.tray.glyphManager.UpdateLocation(this);
                }
            }

            protected override void OnMouseDown(MouseEventArgs me)
            {
                base.OnMouseDown(me);
                if (!this.tray.TabOrderActive)
                {
                    this.tray.FocusDesigner();
                    if (me.Button == MouseButtons.Left)
                    {
                        base.Capture = true;
                        this.mouseDragLast = base.PointToScreen(new Point(me.X, me.Y));
                        this.ctrlSelect = System.Design.NativeMethods.GetKeyState(0x11) != 0;
                        if (!this.ctrlSelect)
                        {
                            ISelectionService service = (ISelectionService) this.tray.GetService(typeof(ISelectionService));
                            bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                            if (service != null)
                            {
                                service.SetSelectedComponents(new object[] { this.Component }, SelectionTypes.Click);
                            }
                        }
                    }
                }
            }

            protected override void OnMouseMove(MouseEventArgs me)
            {
                base.OnMouseMove(me);
                if (this.mouseDragLast != ComponentTray.InvalidPoint)
                {
                    if (!this.mouseDragMoved)
                    {
                        Size dragSize = SystemInformation.DragSize;
                        Size doubleClickSize = SystemInformation.DoubleClickSize;
                        dragSize.Width = Math.Max(dragSize.Width, doubleClickSize.Width);
                        dragSize.Height = Math.Max(dragSize.Height, doubleClickSize.Height);
                        Point point = base.PointToScreen(new Point(me.X, me.Y));
                        if ((this.mouseDragLast == ComponentTray.InvalidPoint) || ((Math.Abs((int) (this.mouseDragLast.X - point.X)) < dragSize.Width) && (Math.Abs((int) (this.mouseDragLast.Y - point.Y)) < dragSize.Height)))
                        {
                            return;
                        }
                        this.mouseDragMoved = true;
                        this.ctrlSelect = false;
                    }
                    try
                    {
                        ISelectionService service = (ISelectionService) this.tray.GetService(typeof(ISelectionService));
                        if (service != null)
                        {
                            service.SetSelectedComponents(new object[] { this.Component }, SelectionTypes.Click);
                        }
                        if ((this.tray.selectionUISvc != null) && this.tray.selectionUISvc.BeginDrag(SelectionRules.Visible | SelectionRules.Moveable, this.mouseDragLast.X, this.mouseDragLast.Y))
                        {
                            this.OnSetCursor();
                        }
                    }
                    finally
                    {
                        this.mouseDragMoved = false;
                        this.mouseDragLast = ComponentTray.InvalidPoint;
                    }
                }
            }

            protected override void OnMouseUp(MouseEventArgs me)
            {
                base.OnMouseUp(me);
                this.OnEndDrag(false);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (this.fRecompute)
                {
                    this.fRecompute = false;
                    this.UpdateIconInfo();
                }
                base.OnPaint(e);
                Rectangle clientRectangle = base.ClientRectangle;
                clientRectangle.X += 5 + this.borderWidth;
                clientRectangle.Y += this.borderWidth;
                clientRectangle.Width -= (2 * this.borderWidth) + 5;
                clientRectangle.Height -= 2 * this.borderWidth;
                StringFormat format = new StringFormat();
                Brush brush = new SolidBrush(this.ForeColor);
                try
                {
                    format.Alignment = StringAlignment.Center;
                    if (this.tray.ShowLargeIcons)
                    {
                        if (this.toolboxBitmap != null)
                        {
                            int x = clientRectangle.X + ((clientRectangle.Width - this.cxIcon) / 2);
                            int y = clientRectangle.Y + 5;
                            e.Graphics.DrawImage(this.toolboxBitmap, new Rectangle(x, y, this.cxIcon, this.cyIcon));
                        }
                        clientRectangle.Y += this.cyIcon + 5;
                        clientRectangle.Height -= this.cyIcon;
                        e.Graphics.DrawString(this.Text, this.Font, brush, clientRectangle, format);
                    }
                    else
                    {
                        if (this.toolboxBitmap != null)
                        {
                            int num3 = clientRectangle.Y + ((clientRectangle.Height - this.cyIcon) / 2);
                            e.Graphics.DrawImage(this.toolboxBitmap, new Rectangle(clientRectangle.X, num3, this.cxIcon, this.cyIcon));
                        }
                        clientRectangle.X += this.cxIcon + this.borderWidth;
                        clientRectangle.Width -= this.cxIcon;
                        clientRectangle.Y += 3;
                        e.Graphics.DrawString(this.Text, this.Font, brush, clientRectangle);
                    }
                }
                finally
                {
                    if (format != null)
                    {
                        format.Dispose();
                    }
                    if (brush != null)
                    {
                        brush.Dispose();
                    }
                }
                if (!System.ComponentModel.InheritanceAttribute.NotInherited.Equals(this.inheritanceAttribute))
                {
                    InheritanceUI inheritanceUI = this.tray.InheritanceUI;
                    if (inheritanceUI != null)
                    {
                        e.Graphics.DrawImage(inheritanceUI.InheritanceGlyph, 0, 0);
                    }
                }
            }

            private void OnSetCursor()
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.component)["Locked"];
                if ((descriptor != null) && ((bool) descriptor.GetValue(this.component)))
                {
                    Cursor.Current = Cursors.Default;
                }
                else if (this.tray.TabOrderActive)
                {
                    Cursor.Current = Cursors.Default;
                }
                else if (this.mouseDragMoved)
                {
                    Cursor.Current = Cursors.Default;
                }
                else if (this.mouseDragLast != ComponentTray.InvalidPoint)
                {
                    Cursor.Current = Cursors.Cross;
                }
                else
                {
                    Cursor.Current = Cursors.SizeAll;
                }
            }

            protected override void OnTextChanged(EventArgs e)
            {
                this.AdjustSize(true);
                base.OnTextChanged(e);
            }

            protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
            {
                if ((!this.tray.AutoArrange || ((specified & BoundsSpecified.Width) == BoundsSpecified.Width)) || ((specified & BoundsSpecified.Height) == BoundsSpecified.Height))
                {
                    base.SetBoundsCore(x, y, width, height, specified);
                }
                Rectangle bounds = base.Bounds;
                Size parentGridSize = this.tray.ParentGridSize;
                if ((Math.Abs((int) (bounds.X - x)) > parentGridSize.Width) || (Math.Abs((int) (bounds.Y - y)) > parentGridSize.Height))
                {
                    base.SetBoundsCore(x, y, width, height, specified);
                }
            }

            protected override void SetVisibleCore(bool value)
            {
                if (!value || this.tray.CanDisplayComponent(this.component))
                {
                    base.SetVisibleCore(value);
                }
            }

            public override string ToString()
            {
                return ("ComponentTray: " + this.component.ToString());
            }

            internal void UpdateIconInfo()
            {
                ToolboxBitmapAttribute attribute = (ToolboxBitmapAttribute) TypeDescriptor.GetAttributes(this.component)[typeof(ToolboxBitmapAttribute)];
                if (attribute != null)
                {
                    this.toolboxBitmap = attribute.GetImage(this.component, this.tray.ShowLargeIcons);
                }
                if (this.toolboxBitmap == null)
                {
                    this.cxIcon = 0;
                    this.cyIcon = SystemInformation.IconSize.Height;
                }
                else
                {
                    Size size = this.toolboxBitmap.Size;
                    this.cxIcon = size.Width;
                    this.cyIcon = size.Height;
                }
                this.AdjustSize(true);
            }

            public virtual void ViewDefaultEvent(IComponent component)
            {
                EventDescriptor defaultEvent = TypeDescriptor.GetDefaultEvent(component);
                PropertyDescriptor eventProperty = null;
                string str = null;
                bool flag = false;
                IEventBindingService service = (IEventBindingService) this.GetService(typeof(IEventBindingService));
                bool enabled = System.ComponentModel.CompModSwitches.CommonDesignerServices.Enabled;
                if (service != null)
                {
                    eventProperty = service.GetEventProperty(defaultEvent);
                }
                if ((eventProperty == null) || eventProperty.IsReadOnly)
                {
                    if (service != null)
                    {
                        service.ShowCode();
                    }
                }
                else
                {
                    str = (string) eventProperty.GetValue(component);
                    if (str == null)
                    {
                        flag = true;
                        str = service.CreateUniqueMethodName(component, defaultEvent);
                    }
                    IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    DesignerTransaction transaction = null;
                    try
                    {
                        if (host != null)
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("WindowsFormsAddEvent", new object[] { defaultEvent.Name }));
                        }
                        if (flag && (eventProperty != null))
                        {
                            eventProperty.SetValue(component, str);
                        }
                        service.ShowCode(component, defaultEvent);
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                }
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 0x20:
                        this.OnSetCursor();
                        return;

                    case 0x7b:
                    {
                        int x = System.Design.NativeMethods.Util.SignedLOWORD((int) ((long) m.LParam));
                        int y = System.Design.NativeMethods.Util.SignedHIWORD((int) ((long) m.LParam));
                        if ((x == -1) && (y == -1))
                        {
                            Point mousePosition = Control.MousePosition;
                            x = mousePosition.X;
                            y = mousePosition.Y;
                        }
                        this.OnContextMenu(x, y);
                        return;
                    }
                    case 0x84:
                        if (this.tray.glyphManager != null)
                        {
                            Point p = new Point((short) System.Design.NativeMethods.Util.LOWORD((int) ((long) m.LParam)), (short) System.Design.NativeMethods.Util.HIWORD((int) ((long) m.LParam)));
                            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT {
                                x = 0,
                                y = 0
                            };
                            System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, base.Handle, pt, 1);
                            p.Offset(pt.x, pt.y);
                            p.Offset(base.Location.X, base.Location.Y);
                            this.tray.glyphManager.GetHitTest(p);
                        }
                        base.WndProc(ref m);
                        return;
                }
                base.WndProc(ref m);
            }

            public IComponent Component
            {
                get
                {
                    return this.component;
                }
            }

            public override System.Drawing.Font Font
            {
                get
                {
                    return this.tray.Font;
                }
            }

            public System.ComponentModel.InheritanceAttribute InheritanceAttribute
            {
                get
                {
                    return this.inheritanceAttribute;
                }
            }

            public bool Positioned
            {
                get
                {
                    return this.positioned;
                }
                set
                {
                    this.positioned = value;
                }
            }

            private class TrayControlAccessibleObject : Control.ControlAccessibleObject
            {
                private ComponentTray tray;

                public TrayControlAccessibleObject(ComponentTray.TrayControl owner, ComponentTray tray) : base(owner)
                {
                    this.tray = tray;
                }

                private IComponent Component
                {
                    get
                    {
                        return ((ComponentTray.TrayControl) base.Owner).Component;
                    }
                }

                public override AccessibleStates State
                {
                    get
                    {
                        AccessibleStates state = base.State;
                        ISelectionService service = (ISelectionService) this.tray.GetService(typeof(ISelectionService));
                        if (service != null)
                        {
                            if (service.GetComponentSelected(this.Component))
                            {
                                state |= AccessibleStates.Selected;
                            }
                            if (service.PrimarySelection == this.Component)
                            {
                                state |= AccessibleStates.Focused;
                            }
                        }
                        return state;
                    }
                }
            }
        }

        private class TrayOleDragDropHandler : OleDragDropHandler
        {
            public TrayOleDragDropHandler(SelectionUIHandler selectionHandler, IServiceProvider serviceProvider, IOleDragClient client) : base(selectionHandler, serviceProvider, client)
            {
            }

            protected override bool CanDropDataObject(IDataObject dataObj)
            {
                ICollection components = null;
                if (dataObj != null)
                {
                    OleDragDropHandler.ComponentDataObjectWrapper wrapper = dataObj as OleDragDropHandler.ComponentDataObjectWrapper;
                    if (wrapper != null)
                    {
                        components = wrapper.InnerData.Components;
                    }
                    else
                    {
                        try
                        {
                            object data = dataObj.GetData(OleDragDropHandler.DataFormat, true);
                            if (data == null)
                            {
                                return false;
                            }
                            IDesignerSerializationService service = (IDesignerSerializationService) base.GetService(typeof(IDesignerSerializationService));
                            if (service == null)
                            {
                                return false;
                            }
                            components = service.Deserialize(data);
                        }
                        catch (Exception exception)
                        {
                            if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                            {
                                throw;
                            }
                        }
                    }
                }
                if ((components == null) || (components.Count <= 0))
                {
                    return false;
                }
                foreach (object obj4 in components)
                {
                    if (!(obj4 is Point) && ((obj4 is Control) || !(obj4 is IComponent)))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private class TraySelectionUIHandler : SelectionUIHandler
        {
            private Size snapSize = Size.Empty;
            private ComponentTray tray;

            public TraySelectionUIHandler(ComponentTray tray)
            {
                this.tray = tray;
                this.snapSize = new Size();
            }

            public override bool BeginDrag(object[] components, SelectionRules rules, int initialX, int initialY)
            {
                bool flag = base.BeginDrag(components, rules, initialX, initialY);
                this.tray.SuspendLayout();
                return flag;
            }

            public override void EndDrag(object[] components, bool cancel)
            {
                base.EndDrag(components, cancel);
                this.tray.ResumeLayout();
            }

            protected override IComponent GetComponent()
            {
                return this.tray;
            }

            protected override Control GetControl()
            {
                return this.tray;
            }

            protected override Control GetControl(IComponent component)
            {
                return ComponentTray.TrayControl.FromComponent(component);
            }

            protected override Size GetCurrentSnapSize()
            {
                return this.snapSize;
            }

            protected override object GetService(System.Type serviceType)
            {
                return this.tray.GetService(serviceType);
            }

            protected override bool GetShouldSnapToGrid()
            {
                return false;
            }

            public override Rectangle GetUpdatedRect(Rectangle originalRect, Rectangle dragRect, bool updateSize)
            {
                return dragRect;
            }

            public override void SetCursor()
            {
                this.tray.OnSetCursor();
            }
        }
    }
}

