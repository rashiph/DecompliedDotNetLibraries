namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    public class ComponentDocumentDesigner : ComponentDesigner, IRootDesigner, IDesigner, IDisposable, IToolboxUser, IOleDragClient, ITypeDescriptorFilterService
    {
        private bool autoArrange = true;
        private CompositionCommandSet commandSet;
        private CompositionUI compositionUI;
        private ITypeDescriptorFilterService delegateFilterService;
        private DesignerExtenders designerExtenders;
        private IEventHandlerService eventHandlerService;
        private InheritanceService inheritanceService;
        private bool largeIcons;
        private PbrsForward pbrsFwd;
        private SelectionUIService selectionUIService;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    host.RemoveService(typeof(IInheritanceService));
                    host.RemoveService(typeof(IEventHandlerService));
                    host.RemoveService(typeof(ISelectionUIService));
                    host.RemoveService(typeof(ComponentTray));
                    IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                        service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    }
                }
                if (this.selectionUIService != null)
                {
                    this.selectionUIService.Dispose();
                    this.selectionUIService = null;
                }
                if (this.commandSet != null)
                {
                    this.commandSet.Dispose();
                    this.commandSet = null;
                }
                if (this.pbrsFwd != null)
                {
                    this.pbrsFwd.Dispose();
                    this.pbrsFwd = null;
                }
                if (this.compositionUI != null)
                {
                    this.compositionUI.Dispose();
                    this.compositionUI = null;
                }
                if (this.designerExtenders != null)
                {
                    this.designerExtenders.Dispose();
                    this.designerExtenders = null;
                }
                if (this.inheritanceService != null)
                {
                    this.inheritanceService.Dispose();
                    this.inheritanceService = null;
                }
            }
            base.Dispose(disposing);
        }

        protected virtual bool GetToolSupported(ToolboxItem tool)
        {
            return true;
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this.inheritanceService = new InheritanceService();
            ISite provider = component.Site;
            IContainer container = null;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            IExtenderProviderService ex = (IExtenderProviderService) this.GetService(typeof(IExtenderProviderService));
            if (ex != null)
            {
                this.designerExtenders = new DesignerExtenders(ex);
            }
            if (host != null)
            {
                this.eventHandlerService = new EventHandlerService(null);
                this.selectionUIService = new SelectionUIService(host);
                host.AddService(typeof(IInheritanceService), this.inheritanceService);
                host.AddService(typeof(IEventHandlerService), this.eventHandlerService);
                host.AddService(typeof(ISelectionUIService), this.selectionUIService);
                this.compositionUI = new CompositionUI(this, provider);
                host.AddService(typeof(ComponentTray), this.compositionUI);
                IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service2 != null)
                {
                    service2.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                    service2.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                }
                ISelectionService service3 = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (service3 != null)
                {
                    service3.SetSelectedComponents(new object[] { component }, SelectionTypes.Auto);
                }
            }
            if (provider != null)
            {
                this.commandSet = new CompositionCommandSet(this.compositionUI, provider);
                container = provider.Container;
            }
            this.pbrsFwd = new PbrsForward(this.compositionUI, provider);
            this.inheritanceService.AddInheritedComponents(component, container);
            IServiceContainer service = (IServiceContainer) this.GetService(typeof(IServiceContainer));
            if (service != null)
            {
                this.delegateFilterService = (ITypeDescriptorFilterService) this.GetService(typeof(ITypeDescriptorFilterService));
                if (this.delegateFilterService != null)
                {
                    service.RemoveService(typeof(ITypeDescriptorFilterService));
                }
                service.AddService(typeof(ITypeDescriptorFilterService), this);
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs ce)
        {
            if (ce.Component != base.Component)
            {
                this.compositionUI.AddComponent(ce.Component);
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs ce)
        {
            this.compositionUI.RemoveComponent(ce.Component);
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            properties["TrayLargeIcon"] = TypeDescriptor.CreateProperty(base.GetType(), "TrayLargeIcon", typeof(bool), new Attribute[] { BrowsableAttribute.No, DesignOnlyAttribute.Yes, CategoryAttribute.Design });
        }

        object IRootDesigner.GetView(ViewTechnology technology)
        {
            if ((technology != ViewTechnology.Default) && (technology != ViewTechnology.WindowsForms))
            {
                throw new ArgumentException();
            }
            return this.compositionUI;
        }

        bool ITypeDescriptorFilterService.FilterAttributes(IComponent component, IDictionary attributes)
        {
            if (this.delegateFilterService != null)
            {
                return this.delegateFilterService.FilterAttributes(component, attributes);
            }
            return true;
        }

        bool ITypeDescriptorFilterService.FilterEvents(IComponent component, IDictionary events)
        {
            if (this.delegateFilterService != null)
            {
                return this.delegateFilterService.FilterEvents(component, events);
            }
            return true;
        }

        bool ITypeDescriptorFilterService.FilterProperties(IComponent component, IDictionary properties)
        {
            if (this.delegateFilterService != null)
            {
                this.delegateFilterService.FilterProperties(component, properties);
            }
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["Location"];
            if (oldPropertyDescriptor != null)
            {
                properties["Location"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { BrowsableAttribute.No });
            }
            return true;
        }

        bool IToolboxUser.GetToolSupported(ToolboxItem tool)
        {
            return true;
        }

        void IToolboxUser.ToolPicked(ToolboxItem tool)
        {
            this.compositionUI.CreateComponentFromTool(tool);
            IToolboxService service = (IToolboxService) this.GetService(typeof(IToolboxService));
            if (service != null)
            {
                service.SelectedToolboxItemUsed();
            }
        }

        bool IOleDragClient.AddComponent(IComponent component, string name, bool firstAdd)
        {
            IContainer container = base.Component.Site.Container;
            if (((container != null) && (name != null)) && (container.Components[name] != null))
            {
                name = null;
            }
            IContainer container2 = null;
            bool flag = false;
            if (!firstAdd)
            {
                if (component.Site != null)
                {
                    container2 = component.Site.Container;
                    if (container2 != container)
                    {
                        container2.Remove(component);
                        flag = true;
                    }
                }
                if (container2 != container)
                {
                    container.Add(component, name);
                }
            }
            if (flag)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    IComponentInitializer designer = service.GetDesigner(component) as IComponentInitializer;
                    if (designer != null)
                    {
                        designer.InitializeExistingComponent(null);
                    }
                }
            }
            if (container2 == container)
            {
                return !firstAdd;
            }
            return true;
        }

        System.Windows.Forms.Control IOleDragClient.GetControlForComponent(object component)
        {
            if (this.compositionUI != null)
            {
                return ((IOleDragClient) this.compositionUI).GetControlForComponent(component);
            }
            return null;
        }

        System.Windows.Forms.Control IOleDragClient.GetDesignerControl()
        {
            if (this.compositionUI != null)
            {
                return ((IOleDragClient) this.compositionUI).GetDesignerControl();
            }
            return null;
        }

        bool IOleDragClient.IsDropOk(IComponent component)
        {
            return true;
        }

        public System.Windows.Forms.Control Control
        {
            get
            {
                return this.compositionUI;
            }
        }

        ViewTechnology[] IRootDesigner.SupportedTechnologies
        {
            get
            {
                return new ViewTechnology[] { ViewTechnology.Default, ViewTechnology.WindowsForms };
            }
        }

        bool IOleDragClient.CanModifyComponents
        {
            get
            {
                return true;
            }
        }

        public bool TrayAutoArrange
        {
            get
            {
                return this.autoArrange;
            }
            set
            {
                this.autoArrange = value;
                this.compositionUI.AutoArrange = value;
            }
        }

        public bool TrayLargeIcon
        {
            get
            {
                return this.largeIcons;
            }
            set
            {
                this.largeIcons = value;
                this.compositionUI.ShowLargeIcons = value;
            }
        }

        private class CompositionUI : ComponentTray
        {
            private const int bannerHeight = 40;
            private const int borderWidth = 10;
            private ComponentDocumentDesigner compositionDesigner;
            private SelectionUIHandler dragHandler;
            private IServiceProvider serviceProvider;
            private IToolboxService toolboxService;
            private ComponentDocumentDesigner.WatermarkLabel watermark;

            public CompositionUI(ComponentDocumentDesigner compositionDesigner, IServiceProvider provider) : base(compositionDesigner, provider)
            {
                this.compositionDesigner = compositionDesigner;
                this.serviceProvider = provider;
                this.watermark = new ComponentDocumentDesigner.WatermarkLabel(this);
                this.watermark.Font = new Font(this.watermark.Font.FontFamily, 11f);
                this.watermark.TextAlign = ContentAlignment.MiddleCenter;
                this.watermark.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnLinkClick);
                this.watermark.Dock = DockStyle.Fill;
                this.watermark.TabStop = false;
                this.watermark.Text = System.Design.SR.GetString("CompositionDesignerWaterMark");
                try
                {
                    string str = System.Design.SR.GetString("CompositionDesignerWaterMarkFirstLink");
                    int index = this.watermark.Text.IndexOf(str);
                    int length = str.Length;
                    this.watermark.Links.Add(index, length, "Toolbox");
                    str = System.Design.SR.GetString("CompositionDesignerWaterMarkSecondLink");
                    index = this.watermark.Text.IndexOf(str);
                    length = str.Length;
                    this.watermark.Links.Add(index, length, "CodeView");
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
                base.Controls.Add(this.watermark);
            }

            public override void AddComponent(IComponent component)
            {
                base.AddComponent(component);
                if (base.Controls.Count > 0)
                {
                    this.watermark.Visible = false;
                }
            }

            protected override bool CanCreateComponentFromTool(ToolboxItem tool)
            {
                return true;
            }

            internal override OleDragDropHandler GetOleDragHandler()
            {
                if (base.oleDragDropHandler == null)
                {
                    base.oleDragDropHandler = new OleDragDropHandler(this.DragHandler, this.serviceProvider, this);
                }
                return base.oleDragDropHandler;
            }

            protected override void OnDragDrop(DragEventArgs de)
            {
                if (base.ClientRectangle.Contains(base.PointToClient(new Point(de.X, de.Y))))
                {
                    base.OnDragDrop(de);
                }
                else
                {
                    de.Effect = DragDropEffects.None;
                }
            }

            protected override void OnDragOver(DragEventArgs de)
            {
                if (base.ClientRectangle.Contains(base.PointToClient(new Point(de.X, de.Y))))
                {
                    base.OnDragOver(de);
                }
                else
                {
                    de.Effect = DragDropEffects.None;
                }
            }

            private void OnLinkClick(object sender, LinkLabelLinkClickedEventArgs e)
            {
                IUIService service = (IUIService) this.compositionDesigner.GetService(typeof(IUIService));
                if (service != null)
                {
                    switch (((string) e.Link.LinkData))
                    {
                        case "ServerExplorer":
                            service.ShowToolWindow(StandardToolWindows.ServerExplorer);
                            return;

                        case "Toolbox":
                            service.ShowToolWindow(StandardToolWindows.Toolbox);
                            return;
                    }
                    IEventBindingService service2 = (IEventBindingService) this.serviceProvider.GetService(typeof(IEventBindingService));
                    if (service2 != null)
                    {
                        service2.ShowCode();
                    }
                }
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                if (this.watermark != null)
                {
                    this.watermark.Location = new Point(0, base.Size.Height / 2);
                    this.watermark.Size = new Size(base.Width, base.Size.Height / 2);
                }
            }

            protected override void OnSetCursor()
            {
                this.SetCursor();
            }

            public override void RemoveComponent(IComponent component)
            {
                base.RemoveComponent(component);
                if (base.Controls.Count == 1)
                {
                    this.watermark.Visible = true;
                }
            }

            internal void SetCursor()
            {
                if (this.toolboxService == null)
                {
                    this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
                }
                if ((this.toolboxService == null) || !this.toolboxService.SetCursor())
                {
                    base.OnSetCursor();
                }
            }

            protected override void WndProc(ref Message m)
            {
                int msg = m.Msg;
                base.WndProc(ref m);
            }

            internal override SelectionUIHandler DragHandler
            {
                get
                {
                    if (this.dragHandler == null)
                    {
                        this.dragHandler = new CompositionSelectionUIHandler(this.compositionDesigner);
                    }
                    return this.dragHandler;
                }
            }

            private class CompositionSelectionUIHandler : SelectionUIHandler
            {
                private ComponentDocumentDesigner compositionDesigner;

                public CompositionSelectionUIHandler(ComponentDocumentDesigner compositionDesigner)
                {
                    this.compositionDesigner = compositionDesigner;
                }

                protected override IComponent GetComponent()
                {
                    return this.compositionDesigner.Component;
                }

                protected override Control GetControl()
                {
                    return this.compositionDesigner.Control;
                }

                protected override Control GetControl(IComponent component)
                {
                    return ComponentTray.TrayControl.FromComponent(component);
                }

                protected override Size GetCurrentSnapSize()
                {
                    return new Size(8, 8);
                }

                protected override object GetService(System.Type serviceType)
                {
                    return this.compositionDesigner.GetService(serviceType);
                }

                protected override bool GetShouldSnapToGrid()
                {
                    return false;
                }

                public override Rectangle GetUpdatedRect(Rectangle originalRect, Rectangle dragRect, bool updateSize)
                {
                    if (this.GetShouldSnapToGrid())
                    {
                        Rectangle rectangle2 = dragRect;
                        int x = dragRect.X;
                        int y = dragRect.Y;
                        int num3 = dragRect.X + dragRect.Width;
                        int num4 = dragRect.Y + dragRect.Height;
                        Size size = new Size(8, 8);
                        int num5 = (size.Width / 2) * ((x < 0) ? -1 : 1);
                        int num6 = (size.Height / 2) * ((y < 0) ? -1 : 1);
                        rectangle2.X = ((x + num5) / size.Width) * size.Width;
                        rectangle2.Y = ((y + num6) / size.Height) * size.Height;
                        num5 = (size.Width / 2) * ((num3 < 0) ? -1 : 1);
                        num6 = (size.Height / 2) * ((num4 < 0) ? -1 : 1);
                        if (updateSize)
                        {
                            rectangle2.Width = (((num3 + num5) / size.Width) * size.Width) - rectangle2.X;
                            rectangle2.Height = (((num4 + num6) / size.Height) * size.Height) - rectangle2.Y;
                        }
                        return rectangle2;
                    }
                    return dragRect;
                }

                public override void SetCursor()
                {
                    this.compositionDesigner.compositionUI.OnSetCursor();
                }
            }
        }

        private class WatermarkLabel : LinkLabel
        {
            private ComponentDocumentDesigner.CompositionUI compositionUI;

            public WatermarkLabel(ComponentDocumentDesigner.CompositionUI compositionUI)
            {
                this.compositionUI = compositionUI;
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 0x20:
                        if (base.OverrideCursor == null)
                        {
                            this.compositionUI.SetCursor();
                            return;
                        }
                        base.WndProc(ref m);
                        return;

                    case 0x84:
                    {
                        Point point = base.PointToClient(new Point((int) ((long) m.LParam)));
                        if (base.PointInLink(point.X, point.Y) == null)
                        {
                            m.Result = (IntPtr) (-1);
                            return;
                        }
                        base.WndProc(ref m);
                        return;
                    }
                }
                base.WndProc(ref m);
            }
        }
    }
}

