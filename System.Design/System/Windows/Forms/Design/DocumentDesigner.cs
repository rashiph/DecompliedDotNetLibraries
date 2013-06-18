namespace System.Windows.Forms.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    [ToolboxItemFilter("System.Windows.Forms")]
    public class DocumentDesigner : ScrollableControlDesigner, IRootDesigner, IDesigner, IDisposable, IToolboxUser, IOleDragClient
    {
        private static readonly string axClipFormat = "CLSID";
        private Hashtable axTools;
        private static TraceSwitch AxToolSwitch = new TraceSwitch("AxTool", "ActiveX Toolbox Tracing");
        private BehaviorService behaviorService;
        private ControlCommandSet commandSet;
        private ComponentTray componentTray;
        private DesignBindingValueUIHandler designBindingValueUIHandler;
        private DesignerExtenders designerExtenders;
        private EventHandlerService eventHandlerService;
        private DesignerFrame frame;
        private static Guid htmlDesignTime = new Guid("73CEF3DD-AE85-11CF-A406-00AA00C00940");
        private InheritanceService inheritanceService;
        private InheritanceUI inheritanceUI;
        private bool initializing;
        protected IMenuEditorService menuEditorService;
        private PbrsForward pbrsFwd;
        private bool queriedTabOrder;
        private SelectionManager selectionManager;
        private ArrayList suspendedComponents;
        private MenuCommand tabOrderCommand;
        private ToolboxItemCreatorCallback toolboxCreator;
        private bool trayAutoArrange;
        private int trayHeight = 80;
        private bool trayLargeIcon;
        private bool trayLayoutSuspended;
        private UndoEngine undoEngine;

        internal virtual bool CanDropComponents(DragEventArgs de)
        {
            if (this.componentTray != null)
            {
                object[] draggingObjects = base.GetOleDragHandler().GetDraggingObjects(de);
                if (draggingObjects != null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    for (int i = 0; i < draggingObjects.Length; i++)
                    {
                        IComponent comp = draggingObjects[i] as IComponent;
                        if (((service != null) && (draggingObjects[i] != null)) && ((comp != null) && this.componentTray.IsTrayComponent(comp)))
                        {
                            return false;
                        }
                    }
                }
                if (de.Data is ToolStripItemDataObject)
                {
                    return false;
                }
            }
            return true;
        }

        private ToolboxItem CreateAxToolboxItem(System.Windows.Forms.IDataObject dataObject)
        {
            AxToolboxItem item = null;
            MemoryStream data = (MemoryStream) dataObject.GetData(axClipFormat, true);
            int length = (int) data.Length;
            byte[] buffer = new byte[length];
            data.Read(buffer, 0, length);
            string clsid = Encoding.Default.GetString(buffer);
            int index = clsid.IndexOf("}");
            clsid = clsid.Substring(0, index + 1);
            if (!this.IsSupportedActiveXControl(clsid))
            {
                return null;
            }
            if (this.axTools != null)
            {
                item = (AxToolboxItem) this.axTools[clsid];
                if (item != null)
                {
                    bool traceVerbose = AxToolSwitch.TraceVerbose;
                    return item;
                }
            }
            item = new AxToolboxItem(clsid);
            if (this.axTools == null)
            {
                this.axTools = new Hashtable();
            }
            this.axTools.Add(clsid, item);
            return item;
        }

        private ToolboxItem CreateCfCodeToolboxItem(System.Windows.Forms.IDataObject dataObject)
        {
            object serializationData = null;
            serializationData = dataObject.GetData(OleDragDropHandler.NestedToolboxItemFormat, false);
            if (serializationData != null)
            {
                return (ToolboxItem) serializationData;
            }
            serializationData = dataObject.GetData(OleDragDropHandler.DataFormat, false);
            if (serializationData != null)
            {
                return new OleDragDropHandler.CfCodeToolboxItem(serializationData);
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    ToolStripAdornerWindowService service = (ToolStripAdornerWindowService) this.GetService(typeof(ToolStripAdornerWindowService));
                    if (service != null)
                    {
                        service.Dispose();
                        host.RemoveService(typeof(ToolStripAdornerWindowService));
                    }
                    host.Activated -= new EventHandler(this.OnDesignerActivate);
                    host.Deactivated -= new EventHandler(this.OnDesignerDeactivate);
                    if (this.componentTray != null)
                    {
                        ISplitWindowService service2 = (ISplitWindowService) this.GetService(typeof(ISplitWindowService));
                        if (service2 != null)
                        {
                            service2.RemoveSplitWindow(this.componentTray);
                            this.componentTray.Dispose();
                            this.componentTray = null;
                        }
                        host.RemoveService(typeof(ComponentTray));
                    }
                    IComponentChangeService service3 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    if (service3 != null)
                    {
                        service3.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                        service3.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                        service3.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    }
                    if (this.undoEngine != null)
                    {
                        this.undoEngine.Undoing -= new EventHandler(this.OnUndoing);
                        this.undoEngine.Undone -= new EventHandler(this.OnUndone);
                    }
                    if (this.toolboxCreator != null)
                    {
                        IToolboxService service4 = (IToolboxService) this.GetService(typeof(IToolboxService));
                        if (service4 != null)
                        {
                            service4.RemoveCreator(axClipFormat, host);
                            service4.RemoveCreator(OleDragDropHandler.DataFormat, host);
                            service4.RemoveCreator(OleDragDropHandler.NestedToolboxItemFormat, host);
                        }
                        this.toolboxCreator = null;
                    }
                }
                ISelectionService service5 = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (service5 != null)
                {
                    service5.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                if (this.behaviorService != null)
                {
                    this.behaviorService.Dispose();
                    this.behaviorService = null;
                }
                if (this.selectionManager != null)
                {
                    this.selectionManager.Dispose();
                    this.selectionManager = null;
                }
                if (this.componentTray != null)
                {
                    if (host != null)
                    {
                        ISplitWindowService service6 = (ISplitWindowService) this.GetService(typeof(ISplitWindowService));
                        if (service6 != null)
                        {
                            service6.RemoveSplitWindow(this.componentTray);
                        }
                    }
                    this.componentTray.Dispose();
                    this.componentTray = null;
                }
                if (this.pbrsFwd != null)
                {
                    this.pbrsFwd.Dispose();
                    this.pbrsFwd = null;
                }
                if (this.frame != null)
                {
                    this.frame.Dispose();
                    this.frame = null;
                }
                if (this.commandSet != null)
                {
                    this.commandSet.Dispose();
                    this.commandSet = null;
                }
                if (this.inheritanceService != null)
                {
                    this.inheritanceService.Dispose();
                    this.inheritanceService = null;
                }
                if (this.inheritanceUI != null)
                {
                    this.inheritanceUI.Dispose();
                    this.inheritanceUI = null;
                }
                if (this.designBindingValueUIHandler != null)
                {
                    IPropertyValueUIService service7 = (IPropertyValueUIService) this.GetService(typeof(IPropertyValueUIService));
                    if (service7 != null)
                    {
                        service7.RemovePropertyValueUIHandler(new PropertyValueUIHandler(this.designBindingValueUIHandler.OnGetUIValueItem));
                        service7 = null;
                    }
                    this.designBindingValueUIHandler.Dispose();
                    this.designBindingValueUIHandler = null;
                }
                if (this.designerExtenders != null)
                {
                    this.designerExtenders.Dispose();
                    this.designerExtenders = null;
                }
                if (this.axTools != null)
                {
                    this.axTools.Clear();
                }
                if (host != null)
                {
                    host.RemoveService(typeof(BehaviorService));
                    host.RemoveService(typeof(ToolStripAdornerWindowService));
                    host.RemoveService(typeof(SelectionManager));
                    host.RemoveService(typeof(IInheritanceService));
                    host.RemoveService(typeof(IEventHandlerService));
                    host.RemoveService(typeof(IOverlayService));
                    host.RemoveService(typeof(ISplitWindowService));
                    host.RemoveService(typeof(InheritanceUI));
                }
            }
            base.Dispose(disposing);
        }

        internal virtual void DoProperMenuSelection(ICollection selComponents)
        {
            foreach (object obj2 in selComponents)
            {
                if (obj2 is ContextMenu)
                {
                    this.menuEditorService.SetMenu((Menu) obj2);
                }
                else
                {
                    MenuItem item = obj2 as MenuItem;
                    if (item != null)
                    {
                        MenuItem parent = item;
                        while (parent.Parent is MenuItem)
                        {
                            parent = (MenuItem) parent.Parent;
                        }
                        if (this.menuEditorService.GetMenu() != parent.Parent)
                        {
                            this.menuEditorService.SetMenu(parent.Parent);
                        }
                        if (selComponents.Count == 1)
                        {
                            this.menuEditorService.SetSelection(item);
                        }
                    }
                    else
                    {
                        this.menuEditorService.SetMenu(null);
                    }
                }
            }
        }

        protected virtual void EnsureMenuEditorService(IComponent c)
        {
            if ((this.menuEditorService == null) && (c is ContextMenu))
            {
                this.menuEditorService = (IMenuEditorService) this.GetService(typeof(IMenuEditorService));
            }
        }

        public override GlyphCollection GetGlyphs(GlyphSelectionType selectionType)
        {
            GlyphCollection glyphs = new GlyphCollection();
            if (selectionType != GlyphSelectionType.NotSelected)
            {
                Point location = base.BehaviorService.ControlToAdornerWindow((Control) base.Component);
                Rectangle controlBounds = new Rectangle(location, ((Control) base.Component).Size);
                bool primarySelection = selectionType == GlyphSelectionType.SelectedPrimary;
                bool flag2 = false;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Locked"];
                if (descriptor != null)
                {
                    flag2 = (bool) descriptor.GetValue(base.Component);
                }
                bool flag3 = false;
                descriptor = TypeDescriptor.GetProperties(base.Component)["AutoSize"];
                if (descriptor != null)
                {
                    flag3 = (bool) descriptor.GetValue(base.Component);
                }
                AutoSizeMode growOnly = AutoSizeMode.GrowOnly;
                descriptor = TypeDescriptor.GetProperties(base.Component)["AutoSizeMode"];
                if (descriptor != null)
                {
                    growOnly = (AutoSizeMode) descriptor.GetValue(base.Component);
                }
                System.Windows.Forms.Design.SelectionRules selectionRules = this.SelectionRules;
                if (flag2)
                {
                    glyphs.Add(new LockedHandleGlyph(controlBounds, primarySelection));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Top));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Bottom));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Left));
                    glyphs.Add(new LockedBorderGlyph(controlBounds, SelectionBorderGlyphType.Right));
                    return glyphs;
                }
                if ((flag3 && (growOnly == AutoSizeMode.GrowAndShrink)) && !(this.Control is Form))
                {
                    glyphs.Add(new NoResizeHandleGlyph(controlBounds, selectionRules, primarySelection, null));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Top, null));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Bottom, null));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Left, null));
                    glyphs.Add(new NoResizeSelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Right, null));
                    return glyphs;
                }
                glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.MiddleRight, this.StandardBehavior, primarySelection));
                glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.LowerRight, this.StandardBehavior, primarySelection));
                glyphs.Add(new GrabHandleGlyph(controlBounds, GrabHandleGlyphType.MiddleBottom, this.StandardBehavior, primarySelection));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Top, null));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Bottom, this.StandardBehavior));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Left, null));
                glyphs.Add(new SelectionBorderGlyph(controlBounds, selectionRules, SelectionBorderGlyphType.Right, this.StandardBehavior));
            }
            return glyphs;
        }

        private ParentControlDesigner GetSelectedParentControlDesigner()
        {
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            ParentControlDesigner designer = null;
            if (service != null)
            {
                object primarySelection = service.PrimarySelection;
                if ((primarySelection == null) || !(primarySelection is Control))
                {
                    primarySelection = null;
                    foreach (object obj3 in service.GetSelectedComponents())
                    {
                        if (obj3 is Control)
                        {
                            primarySelection = obj3;
                            break;
                        }
                    }
                }
                if (primarySelection != null)
                {
                    Control component = (Control) primarySelection;
                    IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (host != null)
                    {
                        while (component != null)
                        {
                            ParentControlDesigner designer2 = host.GetDesigner(component) as ParentControlDesigner;
                            if (designer2 != null)
                            {
                                designer = designer2;
                                break;
                            }
                            component = component.Parent;
                        }
                    }
                }
            }
            if (designer == null)
            {
                designer = this;
            }
            return designer;
        }

        protected virtual bool GetToolSupported(ToolboxItem tool)
        {
            return true;
        }

        public override void Initialize(IComponent component)
        {
            this.initializing = true;
            base.Initialize(component);
            this.initializing = false;
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component.GetType())["BackColor"];
            if (((descriptor != null) && (descriptor.PropertyType == typeof(System.Drawing.Color))) && !descriptor.ShouldSerializeValue(base.Component))
            {
                this.Control.BackColor = SystemColors.Control;
            }
            IDesignerHost serviceProvider = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            IExtenderProviderService ex = (IExtenderProviderService) this.GetService(typeof(IExtenderProviderService));
            if (ex != null)
            {
                this.designerExtenders = new DesignerExtenders(ex);
            }
            if (serviceProvider != null)
            {
                serviceProvider.Activated += new EventHandler(this.OnDesignerActivate);
                serviceProvider.Deactivated += new EventHandler(this.OnDesignerDeactivate);
                ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
                serviceProvider.AddService(typeof(IEventHandlerService), callback);
                this.frame = new DesignerFrame(component.Site);
                IOverlayService frame = this.frame;
                serviceProvider.AddService(typeof(IOverlayService), frame);
                serviceProvider.AddService(typeof(ISplitWindowService), this.frame);
                this.behaviorService = new BehaviorService(base.Component.Site, this.frame);
                serviceProvider.AddService(typeof(BehaviorService), this.behaviorService);
                this.selectionManager = new SelectionManager(serviceProvider, this.behaviorService);
                serviceProvider.AddService(typeof(SelectionManager), this.selectionManager);
                serviceProvider.AddService(typeof(ToolStripAdornerWindowService), callback);
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                }
                this.inheritanceUI = new InheritanceUI();
                serviceProvider.AddService(typeof(InheritanceUI), this.inheritanceUI);
                InheritanceService serviceInstance = new DocumentInheritanceService(this);
                serviceProvider.AddService(typeof(IInheritanceService), serviceInstance);
                serviceInstance.AddInheritedComponents(component, component.Site.Container);
                this.inheritanceService = serviceInstance;
                if (this.Control.IsHandleCreated)
                {
                    this.OnCreateHandle();
                }
                IPropertyValueUIService service5 = (IPropertyValueUIService) component.Site.GetService(typeof(IPropertyValueUIService));
                if (service5 != null)
                {
                    this.designBindingValueUIHandler = new DesignBindingValueUIHandler();
                    service5.AddPropertyValueUIHandler(new PropertyValueUIHandler(this.designBindingValueUIHandler.OnGetUIValueItem));
                }
                IToolboxService service6 = (IToolboxService) serviceProvider.GetService(typeof(IToolboxService));
                if (service6 != null)
                {
                    this.toolboxCreator = new ToolboxItemCreatorCallback(this.OnCreateToolboxItem);
                    service6.AddCreator(this.toolboxCreator, axClipFormat, serviceProvider);
                    service6.AddCreator(this.toolboxCreator, OleDragDropHandler.DataFormat, serviceProvider);
                    service6.AddCreator(this.toolboxCreator, OleDragDropHandler.NestedToolboxItemFormat, serviceProvider);
                }
                serviceProvider.LoadComplete += new EventHandler(this.OnLoadComplete);
            }
            this.commandSet = new ControlCommandSet(component.Site);
            this.frame.Initialize(this.Control);
            this.pbrsFwd = new PbrsForward(this.frame, component.Site);
            this.Location = new Point(0, 0);
        }

        private bool IsSupportedActiveXControl(string clsid)
        {
            RegistryKey key = null;
            RegistryKey key2 = null;
            bool flag;
            try
            {
                string name = @"CLSID\" + clsid + @"\Control";
                key = Registry.ClassesRoot.OpenSubKey(name);
                if (key != null)
                {
                    string str2 = @"CLSID\" + clsid + @"\Implemented Categories\{" + htmlDesignTime.ToString() + "}";
                    key2 = Registry.ClassesRoot.OpenSubKey(str2);
                    return (key2 == null);
                }
                flag = false;
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
                if (key2 != null)
                {
                    key2.Close();
                }
            }
            return flag;
        }

        private void OnComponentAdded(object source, ComponentEventArgs ce)
        {
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                IComponent component = ce.Component;
                this.EnsureMenuEditorService(ce.Component);
                bool flag = true;
                if (!(host.GetDesigner(component) is ToolStripDesigner))
                {
                    ControlDesigner designer = host.GetDesigner(component) as ControlDesigner;
                    if (designer != null)
                    {
                        Form control = designer.Control as Form;
                        if ((control == null) || !control.TopLevel)
                        {
                            flag = false;
                        }
                    }
                }
                if (flag && TypeDescriptor.GetAttributes(component).Contains(DesignTimeVisibleAttribute.Yes))
                {
                    if (this.componentTray == null)
                    {
                        ISplitWindowService service = (ISplitWindowService) this.GetService(typeof(ISplitWindowService));
                        if (service != null)
                        {
                            this.componentTray = new ComponentTray(this, base.Component.Site);
                            service.AddSplitWindow(this.componentTray);
                            this.componentTray.Height = this.trayHeight;
                            this.componentTray.ShowLargeIcons = this.trayLargeIcon;
                            this.componentTray.AutoArrange = this.trayAutoArrange;
                            host.AddService(typeof(ComponentTray), this.componentTray);
                        }
                    }
                    if (this.componentTray != null)
                    {
                        if (((host != null) && host.Loading) && !this.trayLayoutSuspended)
                        {
                            this.trayLayoutSuspended = true;
                            this.componentTray.SuspendLayout();
                        }
                        this.componentTray.AddComponent(component);
                    }
                }
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            Control component = e.Component as Control;
            if ((component != null) && component.IsHandleCreated)
            {
                System.Design.UnsafeNativeMethods.NotifyWinEvent(0x800b, new HandleRef(component, component.Handle), -4, 0);
                if (this.frame.Focused)
                {
                    System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(component, component.Handle), -4, 0);
                }
            }
        }

        private void OnComponentRemoved(object source, ComponentEventArgs ce)
        {
            if (((!(ce.Component is Control) || (ce.Component is ToolStrip)) || ((ce.Component is Form) && ((Form) ce.Component).TopLevel)) && (this.componentTray != null))
            {
                this.componentTray.RemoveComponent(ce.Component);
                if (this.componentTray.ComponentCount == 0)
                {
                    ISplitWindowService service = (ISplitWindowService) this.GetService(typeof(ISplitWindowService));
                    if (service != null)
                    {
                        service.RemoveSplitWindow(this.componentTray);
                        IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                        if (host != null)
                        {
                            host.RemoveService(typeof(ComponentTray));
                        }
                        this.componentTray.Dispose();
                        this.componentTray = null;
                    }
                }
            }
        }

        protected override void OnContextMenu(int x, int y)
        {
            IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                ISelectionService service2 = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (service2 != null)
                {
                    if ((service2.SelectionCount == 1) && service2.GetComponentSelected(base.Component))
                    {
                        service.ShowContextMenu(MenuCommands.ContainerMenu, x, y);
                    }
                    else
                    {
                        Component primarySelection = service2.PrimarySelection as Component;
                        if (primarySelection != null)
                        {
                            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                            if (host != null)
                            {
                                ComponentDesigner designer = host.GetDesigner(primarySelection) as ComponentDesigner;
                                if (designer != null)
                                {
                                    designer.ShowContextMenu(x, y);
                                    return;
                                }
                            }
                        }
                        service.ShowContextMenu(MenuCommands.SelectionMenu, x, y);
                    }
                }
            }
        }

        protected override void OnCreateHandle()
        {
            if (this.inheritanceService != null)
            {
                base.OnCreateHandle();
            }
        }

        private object OnCreateService(IServiceContainer container, System.Type serviceType)
        {
            if (serviceType == typeof(IEventHandlerService))
            {
                if (this.eventHandlerService == null)
                {
                    this.eventHandlerService = new EventHandlerService(this.frame);
                }
                return this.eventHandlerService;
            }
            if (serviceType == typeof(ToolStripAdornerWindowService))
            {
                return new ToolStripAdornerWindowService(base.Component.Site, this.frame);
            }
            return null;
        }

        private ToolboxItem OnCreateToolboxItem(object serializedData, string format)
        {
            System.Windows.Forms.IDataObject dataObject = serializedData as System.Windows.Forms.IDataObject;
            if (dataObject == null)
            {
                return null;
            }
            if (format.Equals(axClipFormat))
            {
                return this.CreateAxToolboxItem(dataObject);
            }
            if (!format.Equals(OleDragDropHandler.DataFormat) && !format.Equals(OleDragDropHandler.NestedToolboxItemFormat))
            {
                return null;
            }
            return this.CreateCfCodeToolboxItem(dataObject);
        }

        private void OnDesignerActivate(object source, EventArgs evevent)
        {
            if (this.undoEngine == null)
            {
                this.undoEngine = this.GetService(typeof(UndoEngine)) as UndoEngine;
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undoing += new EventHandler(this.OnUndoing);
                    this.undoEngine.Undone += new EventHandler(this.OnUndone);
                }
            }
        }

        private void OnDesignerDeactivate(object sender, EventArgs e)
        {
            Control control = this.Control;
            if ((control != null) && control.IsHandleCreated)
            {
                System.Design.NativeMethods.SendMessage(control.Handle, 0x86, 0, 0);
                System.Design.SafeNativeMethods.RedrawWindow(control.Handle, null, IntPtr.Zero, 0x400);
            }
        }

        private void OnLoadComplete(object sender, EventArgs e)
        {
            ((IDesignerHost) sender).LoadComplete -= new EventHandler(this.OnLoadComplete);
            if (this.trayLayoutSuspended && (this.componentTray != null))
            {
                this.componentTray.ResumeLayout();
            }
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (service != null)
            {
                service.SelectionChanged += new EventHandler(this.OnSelectionChanged);
                service.SetSelectedComponents(new object[] { base.Component }, SelectionTypes.Replace);
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (service != null)
            {
                ICollection selectedComponents = service.GetSelectedComponents();
                foreach (object obj2 in selectedComponents)
                {
                    Control wrapper = obj2 as Control;
                    if (wrapper != null)
                    {
                        System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8007, new HandleRef(wrapper, wrapper.Handle), -4, 0);
                    }
                }
                Control primarySelection = service.PrimarySelection as Control;
                if (primarySelection != null)
                {
                    System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(primarySelection, primarySelection.Handle), -4, 0);
                }
                IHelpService service2 = (IHelpService) this.GetService(typeof(IHelpService));
                if (service2 != null)
                {
                    ushort num = 0;
                    string[] strArray = new string[] { "VisualSelection", "NonVisualSelection", "MixedSelection" };
                    foreach (object obj3 in selectedComponents)
                    {
                        if (obj3 is Control)
                        {
                            if (obj3 != base.Component)
                            {
                                num = (ushort) (num | 1);
                            }
                        }
                        else
                        {
                            num = (ushort) (num | 2);
                        }
                        if (num == 3)
                        {
                            break;
                        }
                    }
                    for (int i = 0; i < strArray.Length; i++)
                    {
                        service2.RemoveContextAttribute("Keyword", strArray[i]);
                    }
                    if (num != 0)
                    {
                        service2.AddContextAttribute("Keyword", strArray[num - 1], HelpKeywordType.GeneralKeyword);
                    }
                }
                if (this.menuEditorService != null)
                {
                    this.DoProperMenuSelection(selectedComponents);
                }
            }
        }

        private void OnUndoing(object source, EventArgs e)
        {
            IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service != null)
            {
                IContainer container = service.Container;
                if (container != null)
                {
                    this.suspendedComponents = new ArrayList(container.Components.Count + 1);
                    foreach (IComponent component in container.Components)
                    {
                        Control control = component as Control;
                        if (control != null)
                        {
                            control.SuspendLayout();
                            this.suspendedComponents.Add(control);
                        }
                    }
                    Control rootComponent = service.RootComponent as Control;
                    if (rootComponent != null)
                    {
                        Control parent = rootComponent.Parent;
                        if (parent != null)
                        {
                            parent.SuspendLayout();
                            this.suspendedComponents.Add(parent);
                        }
                    }
                }
            }
        }

        private void OnUndone(object source, EventArgs e)
        {
            if (this.suspendedComponents != null)
            {
                foreach (Control control in this.suspendedComponents)
                {
                    control.ResumeLayout(false);
                    control.PerformLayout();
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            PropertyDescriptor descriptor;
            base.PreFilterProperties(properties);
            properties["TrayHeight"] = TypeDescriptor.CreateProperty(typeof(DocumentDesigner), "TrayHeight", typeof(int), new Attribute[] { BrowsableAttribute.No, DesignOnlyAttribute.Yes, new System.Design.SRDescriptionAttribute("FormDocumentDesignerTraySizeDescr"), CategoryAttribute.Design });
            properties["TrayLargeIcon"] = TypeDescriptor.CreateProperty(typeof(DocumentDesigner), "TrayLargeIcon", typeof(bool), new Attribute[] { BrowsableAttribute.No, DesignOnlyAttribute.Yes, CategoryAttribute.Design });
            properties["DoubleBuffered"] = TypeDescriptor.CreateProperty(typeof(Control), "DoubleBuffered", typeof(bool), new Attribute[] { BrowsableAttribute.Yes, DesignOnlyAttribute.No });
            string[] strArray = new string[] { "Location", "BackColor" };
            string[] strArray2 = new string[] { "Anchor", "Dock", "TabIndex", "TabStop", "Visible" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                descriptor = (PropertyDescriptor) properties[strArray[i]];
                if (descriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(DocumentDesigner), descriptor, attributes);
                }
            }
            descriptor = (PropertyDescriptor) properties["AutoScaleDimensions"];
            if (descriptor != null)
            {
                properties["AutoScaleDimensions"] = TypeDescriptor.CreateProperty(typeof(DocumentDesigner), descriptor, new Attribute[] { DesignerSerializationVisibilityAttribute.Visible });
            }
            descriptor = (PropertyDescriptor) properties["AutoScaleMode"];
            if (descriptor != null)
            {
                properties["AutoScaleMode"] = TypeDescriptor.CreateProperty(typeof(DocumentDesigner), descriptor, new Attribute[] { DesignerSerializationVisibilityAttribute.Visible, BrowsableAttribute.Yes });
            }
            for (int j = 0; j < strArray2.Length; j++)
            {
                descriptor = (PropertyDescriptor) properties[strArray2[j]];
                if (descriptor != null)
                {
                    properties[strArray2[j]] = TypeDescriptor.CreateProperty(descriptor.ComponentType, descriptor, new Attribute[] { BrowsableAttribute.No, DesignerSerializationVisibilityAttribute.Hidden });
                }
            }
        }

        private void ResetBackColor()
        {
            this.BackColor = System.Drawing.Color.Empty;
        }

        private bool ShouldSerializeAutoScaleDimensions()
        {
            return ((!this.initializing && (this.AutoScaleMode != System.Windows.Forms.AutoScaleMode.None)) && (this.AutoScaleMode != System.Windows.Forms.AutoScaleMode.Inherit));
        }

        private bool ShouldSerializeAutoScaleMode()
        {
            return (!this.initializing && base.ShadowProperties.Contains("AutoScaleMode"));
        }

        private bool ShouldSerializeBackColor()
        {
            if (base.ShadowProperties.Contains("BackColor"))
            {
                System.Drawing.Color color = (System.Drawing.Color) base.ShadowProperties["BackColor"];
                if (!color.IsEmpty)
                {
                    return true;
                }
            }
            return false;
        }

        object IRootDesigner.GetView(ViewTechnology technology)
        {
            if ((technology != ViewTechnology.Default) && (technology != ViewTechnology.WindowsForms))
            {
                throw new ArgumentException();
            }
            return this.frame;
        }

        bool IToolboxUser.GetToolSupported(ToolboxItem tool)
        {
            return this.GetToolSupported(tool);
        }

        void IToolboxUser.ToolPicked(ToolboxItem tool)
        {
            this.ToolPicked(tool);
        }

        Control IOleDragClient.GetControlForComponent(object component)
        {
            Control control = base.GetControl(component);
            if (control != null)
            {
                return control;
            }
            if (this.componentTray != null)
            {
                return ((IOleDragClient) this.componentTray).GetControlForComponent(component);
            }
            return null;
        }

        protected virtual void ToolPicked(ToolboxItem tool)
        {
            IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                MenuCommand command = service.FindCommand(StandardCommands.TabOrder);
                if ((command != null) && command.Checked)
                {
                    return;
                }
            }
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                host.Activate();
            }
            try
            {
                ParentControlDesigner selectedParentControlDesigner = this.GetSelectedParentControlDesigner();
                if (!base.InvokeGetInheritanceAttribute(selectedParentControlDesigner).Equals(InheritanceAttribute.InheritedReadOnly))
                {
                    ParentControlDesigner.InvokeCreateTool(selectedParentControlDesigner, tool);
                    IToolboxService service2 = (IToolboxService) this.GetService(typeof(IToolboxService));
                    if (service2 != null)
                    {
                        service2.SelectedToolboxItemUsed();
                    }
                }
            }
            catch (Exception exception)
            {
                base.DisplayError(exception);
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
        }

        private unsafe void WmWindowPosChanged(ref Message m)
        {
            System.Design.NativeMethods.WINDOWPOS* lParam = (System.Design.NativeMethods.WINDOWPOS*) m.LParam;
            if (((lParam->flags & 1) == 0) && (this.menuEditorService != null))
            {
                base.BehaviorService.SyncSelection();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (((this.menuEditorService == null) || (this.TabOrderActive && ((m.Msg == 0xa1) || (m.Msg == 0xa4)))) || !this.menuEditorService.MessageFilter(ref m))
            {
                base.WndProc(ref m);
                if (m.Msg == 0x47)
                {
                    this.WmWindowPosChanged(ref m);
                }
            }
        }

        private SizeF AutoScaleDimensions
        {
            get
            {
                ContainerControl control = this.Control as ContainerControl;
                if (control != null)
                {
                    return control.CurrentAutoScaleDimensions;
                }
                return SizeF.Empty;
            }
            set
            {
                ContainerControl control = this.Control as ContainerControl;
                if (control != null)
                {
                    control.AutoScaleDimensions = value;
                }
            }
        }

        private System.Windows.Forms.AutoScaleMode AutoScaleMode
        {
            get
            {
                ContainerControl control = this.Control as ContainerControl;
                if (control != null)
                {
                    return control.AutoScaleMode;
                }
                return System.Windows.Forms.AutoScaleMode.Inherit;
            }
            set
            {
                base.ShadowProperties["AutoScaleMode"] = value;
                ContainerControl control = this.Control as ContainerControl;
                if ((control != null) && (control.AutoScaleMode != value))
                {
                    control.AutoScaleMode = value;
                    IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    if ((service != null) && !service.Loading)
                    {
                        control.AutoScaleDimensions = control.CurrentAutoScaleDimensions;
                    }
                }
            }
        }

        private System.Drawing.Color BackColor
        {
            get
            {
                return this.Control.BackColor;
            }
            set
            {
                base.ShadowProperties["BackColor"] = value;
                if (value.IsEmpty)
                {
                    value = SystemColors.Control;
                }
                this.Control.BackColor = value;
            }
        }

        [DefaultValue(typeof(Point), "0, 0")]
        private Point Location
        {
            get
            {
                return (Point) base.ShadowProperties["Location"];
            }
            set
            {
                base.ShadowProperties["Location"] = value;
            }
        }

        public override System.Windows.Forms.Design.SelectionRules SelectionRules
        {
            get
            {
                return (base.SelectionRules & ~(System.Windows.Forms.Design.SelectionRules.Moveable | System.Windows.Forms.Design.SelectionRules.LeftSizeable | System.Windows.Forms.Design.SelectionRules.TopSizeable));
            }
        }

        ViewTechnology[] IRootDesigner.SupportedTechnologies
        {
            get
            {
                return new ViewTechnology[] { ViewTechnology.Default, ViewTechnology.WindowsForms };
            }
        }

        private bool TabOrderActive
        {
            get
            {
                if (!this.queriedTabOrder)
                {
                    this.queriedTabOrder = true;
                    IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                    if (service != null)
                    {
                        this.tabOrderCommand = service.FindCommand(StandardCommands.TabOrder);
                    }
                }
                return ((this.tabOrderCommand != null) && this.tabOrderCommand.Checked);
            }
        }

        [DefaultValue(true)]
        private bool TrayAutoArrange
        {
            get
            {
                return this.trayAutoArrange;
            }
            set
            {
                this.trayAutoArrange = value;
                if (this.componentTray != null)
                {
                    this.componentTray.AutoArrange = this.trayAutoArrange;
                }
            }
        }

        [DefaultValue(80)]
        private int TrayHeight
        {
            get
            {
                if (this.componentTray != null)
                {
                    return this.componentTray.Height;
                }
                return this.trayHeight;
            }
            set
            {
                this.trayHeight = value;
                if (this.componentTray != null)
                {
                    this.componentTray.Height = this.trayHeight;
                }
            }
        }

        [DefaultValue(false)]
        private bool TrayLargeIcon
        {
            get
            {
                return this.trayLargeIcon;
            }
            set
            {
                this.trayLargeIcon = value;
                if (this.componentTray != null)
                {
                    this.componentTray.ShowLargeIcons = this.trayLargeIcon;
                }
            }
        }

        [Serializable]
        private class AxToolboxItem : ToolboxItem
        {
            private System.Type axctlType;
            private string clsid;
            private string version;

            public AxToolboxItem(string clsid) : base(typeof(AxHost))
            {
                this.version = string.Empty;
                this.clsid = clsid;
                base.Company = null;
                this.LoadVersionInfo();
            }

            private AxToolboxItem(SerializationInfo info, StreamingContext context)
            {
                this.version = string.Empty;
                this.Deserialize(info, context);
            }

            protected override IComponent[] CreateComponentsCore(IDesignerHost host)
            {
                IComponent[] componentArray = null;
                object references = this.GetReferences(host);
                if (references != null)
                {
                    try
                    {
                        System.Runtime.InteropServices.ComTypes.TYPELIBATTR typeLibAttr = this.GetTypeLibAttr();
                        object[] args = new object[] { "{" + typeLibAttr.guid.ToString() + "}", (int) typeLibAttr.wMajorVerNum, (int) typeLibAttr.wMinorVerNum, typeLibAttr.lcid, "" };
                        references.GetType().InvokeMember("AddActiveX", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, references, args, CultureInfo.InvariantCulture);
                        args[4] = "aximp";
                        object reference = references.GetType().InvokeMember("AddActiveX", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, references, args, CultureInfo.InvariantCulture);
                        this.axctlType = this.GetAxTypeFromReference(reference, host);
                    }
                    catch (TargetInvocationException exception)
                    {
                        throw exception.InnerException;
                    }
                    catch (Exception exception2)
                    {
                        throw exception2;
                    }
                }
                if (this.axctlType == null)
                {
                    IUIService service = (IUIService) host.GetService(typeof(IUIService));
                    if (service == null)
                    {
                        System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, System.Design.SR.GetString("AxImportFailed"), null, MessageBoxButtons.OK, MessageBoxIcon.Hand, MessageBoxDefaultButton.Button1, 0);
                    }
                    else
                    {
                        service.ShowError(System.Design.SR.GetString("AxImportFailed"));
                    }
                    return new IComponent[0];
                }
                componentArray = new IComponent[1];
                try
                {
                    componentArray[0] = host.CreateComponent(this.axctlType);
                }
                catch (Exception exception3)
                {
                    throw exception3;
                }
                return componentArray;
            }

            protected override void Deserialize(SerializationInfo info, StreamingContext context)
            {
                base.Deserialize(info, context);
                this.clsid = info.GetString("Clsid");
            }

            private System.Type GetAxTypeFromAssembly(Assembly a)
            {
                System.Type[] types = a.GetTypes();
                int length = types.Length;
                for (int i = 0; i < length; i++)
                {
                    System.Type c = types[i];
                    if (typeof(AxHost).IsAssignableFrom(c))
                    {
                        AxHost.ClsidAttribute attribute = (AxHost.ClsidAttribute) c.GetCustomAttributes(typeof(AxHost.ClsidAttribute), false)[0];
                        if (string.Equals(attribute.Value, this.clsid, StringComparison.OrdinalIgnoreCase))
                        {
                            return c;
                        }
                    }
                }
                return null;
            }

            private System.Type GetAxTypeFromReference(object reference, IDesignerHost host)
            {
                string fileName = (string) reference.GetType().InvokeMember("Path", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, reference, null, CultureInfo.InvariantCulture);
                if ((fileName == null) || (fileName.Length <= 0))
                {
                    return null;
                }
                FileInfo info = new FileInfo(fileName);
                string fullName = info.FullName;
                Assembly a = ((ITypeResolutionService) host.GetService(typeof(ITypeResolutionService))).GetAssembly(AssemblyName.GetAssemblyName(fullName));
                return this.GetAxTypeFromAssembly(a);
            }

            private object GetReferences(IDesignerHost host)
            {
                object target = null;
                System.Type serviceType = System.Type.GetType("EnvDTE.ProjectItem, EnvDTE, Version=7.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
                if (serviceType == null)
                {
                    return null;
                }
                target = host.GetService(serviceType);
                if (target == null)
                {
                    return null;
                }
                target.GetType().InvokeMember("Name", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, target, null, CultureInfo.InvariantCulture).ToString();
                object obj3 = target.GetType().InvokeMember("ContainingProject", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, target, null, CultureInfo.InvariantCulture);
                object obj4 = obj3.GetType().InvokeMember("Object", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, obj3, null, CultureInfo.InvariantCulture);
                return obj4.GetType().InvokeMember("References", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, obj4, null, CultureInfo.InvariantCulture);
            }

            private System.Runtime.InteropServices.ComTypes.TYPELIBATTR GetTypeLibAttr()
            {
                string name = @"CLSID\" + this.clsid;
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(name);
                if (key == null)
                {
                    bool traceVerbose = DocumentDesigner.AxToolSwitch.TraceVerbose;
                    throw new ArgumentException(System.Design.SR.GetString("AXNotRegistered", new object[] { name.ToString() }));
                }
                System.Runtime.InteropServices.ComTypes.ITypeLib o = null;
                Guid empty = Guid.Empty;
                RegistryKey key2 = key.OpenSubKey("TypeLib");
                if (key2 != null)
                {
                    RegistryKey key3 = key.OpenSubKey("Version");
                    short majorVersion = -1;
                    short minorVersion = -1;
                    string s = (string) key3.GetValue("");
                    int index = s.IndexOf('.');
                    if (index == -1)
                    {
                        majorVersion = short.Parse(s, CultureInfo.InvariantCulture);
                        minorVersion = 0;
                    }
                    else
                    {
                        majorVersion = short.Parse(s.Substring(0, index), CultureInfo.InvariantCulture);
                        minorVersion = short.Parse(s.Substring(index + 1, (s.Length - index) - 1), CultureInfo.InvariantCulture);
                    }
                    key3.Close();
                    object obj2 = key2.GetValue("");
                    empty = new Guid((string) obj2);
                    key2.Close();
                    try
                    {
                        o = System.Design.NativeMethods.LoadRegTypeLib(ref empty, majorVersion, minorVersion, Application.CurrentCulture.LCID);
                    }
                    catch (Exception exception)
                    {
                        bool enabled = AxWrapperGen.AxWrapper.Enabled;
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
                if (o == null)
                {
                    RegistryKey key4 = key.OpenSubKey("InprocServer32");
                    if (key4 != null)
                    {
                        string typelib = (string) key4.GetValue("");
                        key4.Close();
                        o = System.Design.NativeMethods.LoadTypeLib(typelib);
                    }
                }
                key.Close();
                if (o != null)
                {
                    try
                    {
                        IntPtr invalidIntPtr = System.Design.NativeMethods.InvalidIntPtr;
                        o.GetLibAttr(out invalidIntPtr);
                        if (invalidIntPtr == System.Design.NativeMethods.InvalidIntPtr)
                        {
                            throw new ArgumentException(System.Design.SR.GetString("AXNotRegistered", new object[] { name.ToString() }));
                        }
                        System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(invalidIntPtr, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                        o.ReleaseTLibAttr(invalidIntPtr);
                        return typelibattr;
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(o);
                    }
                }
                throw new ArgumentException(System.Design.SR.GetString("AXNotRegistered", new object[] { name.ToString() }));
            }

            private void LoadVersionInfo()
            {
                string name = @"CLSID\" + this.clsid;
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(name);
                if (key != null)
                {
                    RegistryKey key2 = key.OpenSubKey("Version");
                    if (key2 != null)
                    {
                        this.version = (string) key2.GetValue("");
                        key2.Close();
                    }
                    key.Close();
                }
            }

            protected override void Serialize(SerializationInfo info, StreamingContext context)
            {
                bool traceVerbose = DocumentDesigner.AxToolSwitch.TraceVerbose;
                base.Serialize(info, context);
                info.AddValue("Clsid", this.clsid);
            }

            public override string ComponentType
            {
                get
                {
                    return System.Design.SR.GetString("Ax_Control");
                }
            }

            public override string Version
            {
                get
                {
                    return this.version;
                }
            }
        }

        private class DocumentInheritanceService : InheritanceService
        {
            private DocumentDesigner designer;

            public DocumentInheritanceService(DocumentDesigner designer)
            {
                this.designer = designer;
            }

            protected override bool IgnoreInheritedMember(MemberInfo member, IComponent component)
            {
                bool flag = false;
                System.Type c = null;
                FieldInfo info = member as FieldInfo;
                MethodInfo info2 = member as MethodInfo;
                if (info != null)
                {
                    flag = info.IsPrivate || info.IsAssembly;
                    c = info.FieldType;
                }
                else if (info2 != null)
                {
                    flag = info2.IsPrivate || info2.IsAssembly;
                    c = info2.ReturnType;
                }
                else
                {
                    return true;
                }
                if (flag)
                {
                    if (typeof(Control).IsAssignableFrom(c))
                    {
                        Control parent = null;
                        if (info != null)
                        {
                            parent = (Control) info.GetValue(component);
                        }
                        else if (info2 != null)
                        {
                            parent = (Control) info2.Invoke(component, null);
                        }
                        Control control2 = this.designer.Control;
                        while ((parent != null) && (parent != control2))
                        {
                            parent = parent.Parent;
                        }
                        if (parent != null)
                        {
                            return false;
                        }
                    }
                    else if (typeof(Menu).IsAssignableFrom(c))
                    {
                        object obj2 = null;
                        if (info != null)
                        {
                            obj2 = info.GetValue(component);
                        }
                        else if (info2 != null)
                        {
                            obj2 = info2.Invoke(component, null);
                        }
                        if (obj2 != null)
                        {
                            return false;
                        }
                    }
                }
                return base.IgnoreInheritedMember(member, component);
            }
        }
    }
}

