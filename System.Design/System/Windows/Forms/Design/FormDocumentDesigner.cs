namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class FormDocumentDesigner : DocumentDesigner
    {
        private System.Drawing.Size autoScaleBaseSize = System.Drawing.Size.Empty;
        private bool autoSize;
        private bool hasMenu;
        private int heightDelta;
        private bool inAutoscale;
        private InheritanceAttribute inheritanceAttribute;
        private bool initializing;
        private bool isMenuInherited;
        private ToolStripAdornerWindowService toolStripAdornerWindowService;

        private void ApplyAutoScaling(SizeF baseVar, Form form)
        {
            if (!baseVar.IsEmpty)
            {
                SizeF autoScaleSize = Form.GetAutoScaleSize(form.Font);
                System.Drawing.Size size = new System.Drawing.Size((int) Math.Round((double) autoScaleSize.Width), (int) Math.Round((double) autoScaleSize.Height));
                if (!baseVar.Equals(size))
                {
                    float dy = ((float) size.Height) / baseVar.Height;
                    float dx = ((float) size.Width) / baseVar.Width;
                    try
                    {
                        this.inAutoscale = true;
                        form.Scale(dx, dy);
                    }
                    finally
                    {
                        this.inAutoscale = false;
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    host.LoadComplete -= new EventHandler(this.OnLoadComplete);
                    host.Activated -= new EventHandler(this.OnDesignerActivate);
                    host.Deactivated -= new EventHandler(this.OnDesignerDeactivate);
                }
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                }
            }
            base.Dispose(disposing);
        }

        internal override void DoProperMenuSelection(ICollection selComponents)
        {
            foreach (object obj2 in selComponents)
            {
                System.Windows.Forms.Menu menu = obj2 as System.Windows.Forms.Menu;
                if (menu != null)
                {
                    MenuItem item = menu as MenuItem;
                    if (item != null)
                    {
                        System.Windows.Forms.Menu menu2 = base.menuEditorService.GetMenu();
                        MenuItem parent = item;
                        while (parent.Parent is MenuItem)
                        {
                            parent = (MenuItem) parent.Parent;
                        }
                        if (menu2 != parent.Parent)
                        {
                            base.menuEditorService.SetMenu(parent.Parent);
                        }
                        if (selComponents.Count == 1)
                        {
                            base.menuEditorService.SetSelection(item);
                        }
                    }
                    else
                    {
                        base.menuEditorService.SetMenu(menu);
                    }
                    break;
                }
                if ((this.Menu != null) && (this.Menu.MenuItems.Count == 0))
                {
                    base.menuEditorService.SetMenu(null);
                }
                else
                {
                    base.menuEditorService.SetMenu(this.Menu);
                }
                System.Design.NativeMethods.SendMessage(this.Control.Handle, 0x86, 1, 0);
            }
        }

        protected override void EnsureMenuEditorService(IComponent c)
        {
            if ((base.menuEditorService == null) && (c is System.Windows.Forms.Menu))
            {
                base.menuEditorService = (IMenuEditorService) this.GetService(typeof(IMenuEditorService));
            }
        }

        private void EnsureToolStripWindowAdornerService()
        {
            if (this.toolStripAdornerWindowService == null)
            {
                this.toolStripAdornerWindowService = (ToolStripAdornerWindowService) this.GetService(typeof(ToolStripAdornerWindowService));
            }
        }

        private int GetMenuHeight()
        {
            if ((this.Menu == null) || (this.IsMenuInherited && this.initializing))
            {
                return 0;
            }
            if (base.menuEditorService != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.menuEditorService)["MenuHeight"];
                if (descriptor != null)
                {
                    return (int) descriptor.GetValue(base.menuEditorService);
                }
            }
            return SystemInformation.MenuHeight;
        }

        public override void Initialize(IComponent component)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component.GetType())["WindowState"];
            if ((descriptor != null) && (descriptor.PropertyType == typeof(FormWindowState)))
            {
                this.WindowState = (FormWindowState) descriptor.GetValue(component);
            }
            this.initializing = true;
            base.Initialize(component);
            this.initializing = false;
            base.AutoResizeHandles = true;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                host.LoadComplete += new EventHandler(this.OnLoadComplete);
                host.Activated += new EventHandler(this.OnDesignerActivate);
                host.Deactivated += new EventHandler(this.OnDesignerDeactivate);
            }
            Form control = (Form) this.Control;
            control.WindowState = FormWindowState.Normal;
            base.ShadowProperties["AcceptButton"] = control.AcceptButton;
            base.ShadowProperties["CancelButton"] = control.CancelButton;
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
            }
        }

        private void OnComponentAdded(object source, ComponentEventArgs ce)
        {
            if (ce.Component is System.Windows.Forms.Menu)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (((service != null) && !service.Loading) && ((ce.Component is MainMenu) && !this.hasMenu))
                {
                    TypeDescriptor.GetProperties(base.Component)["Menu"].SetValue(base.Component, ce.Component);
                    this.hasMenu = true;
                }
            }
            if (((ce.Component is ToolStrip) && (this.toolStripAdornerWindowService == null)) && (((IDesignerHost) this.GetService(typeof(IDesignerHost))) != null))
            {
                this.EnsureToolStripWindowAdornerService();
            }
        }

        private void OnComponentRemoved(object source, ComponentEventArgs ce)
        {
            if (ce.Component is System.Windows.Forms.Menu)
            {
                if (ce.Component == this.Menu)
                {
                    TypeDescriptor.GetProperties(base.Component)["Menu"].SetValue(base.Component, null);
                    this.hasMenu = false;
                }
                else if ((base.menuEditorService != null) && (ce.Component == base.menuEditorService.GetMenu()))
                {
                    base.menuEditorService.SetMenu(this.Menu);
                }
            }
            if ((ce.Component is ToolStrip) && (this.toolStripAdornerWindowService != null))
            {
                this.toolStripAdornerWindowService = null;
            }
            if (ce.Component is IButtonControl)
            {
                if (ce.Component == base.ShadowProperties["AcceptButton"])
                {
                    this.AcceptButton = null;
                }
                if (ce.Component == base.ShadowProperties["CancelButton"])
                {
                    this.CancelButton = null;
                }
            }
        }

        protected override void OnCreateHandle()
        {
            if ((this.Menu != null) && (base.menuEditorService != null))
            {
                base.menuEditorService.SetMenu(null);
                base.menuEditorService.SetMenu(this.Menu);
            }
            if (this.heightDelta != 0)
            {
                Form component = (Form) base.Component;
                component.Height += this.heightDelta;
                this.heightDelta = 0;
            }
        }

        private void OnDesignerActivate(object source, EventArgs evevent)
        {
            Control control = this.Control;
            if ((control != null) && control.IsHandleCreated)
            {
                System.Design.NativeMethods.SendMessage(control.Handle, 0x86, 1, 0);
                System.Design.SafeNativeMethods.RedrawWindow(control.Handle, null, IntPtr.Zero, 0x400);
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

        private void OnLoadComplete(object source, EventArgs evevent)
        {
            Form control = this.Control as Form;
            if (control != null)
            {
                int width = control.ClientSize.Width;
                int height = control.ClientSize.Height;
                if (control.HorizontalScroll.Visible && control.AutoScroll)
                {
                    height += SystemInformation.HorizontalScrollBarHeight;
                }
                if (control.VerticalScroll.Visible && control.AutoScroll)
                {
                    width += SystemInformation.VerticalScrollBarWidth;
                }
                this.ApplyAutoScaling((SizeF) this.autoScaleBaseSize, control);
                this.ClientSize = new System.Drawing.Size(width, height);
                BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                if (service != null)
                {
                    service.SyncSelection();
                }
                if (this.heightDelta == 0)
                {
                    this.heightDelta = this.GetMenuHeight();
                }
                if (this.heightDelta != 0)
                {
                    control.Height += this.heightDelta;
                    this.heightDelta = 0;
                }
                if (((!control.ControlBox && !control.ShowInTaskbar) && (!string.IsNullOrEmpty(control.Text) && (this.Menu != null))) && !this.IsMenuInherited)
                {
                    control.Height += SystemInformation.CaptionHeight + 1;
                }
                control.PerformLayout();
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            PropertyDescriptor descriptor;
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Opacity", "Menu", "IsMdiContainer", "Size", "ShowInTaskBar", "WindowState", "AutoSize", "AcceptButton", "CancelButton" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                descriptor = (PropertyDescriptor) properties[strArray[i]];
                if (descriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(FormDocumentDesigner), descriptor, attributes);
                }
            }
            descriptor = (PropertyDescriptor) properties["AutoScaleBaseSize"];
            if (descriptor != null)
            {
                properties["AutoScaleBaseSize"] = TypeDescriptor.CreateProperty(typeof(FormDocumentDesigner), descriptor, new Attribute[] { DesignerSerializationVisibilityAttribute.Visible });
            }
            descriptor = (PropertyDescriptor) properties["ClientSize"];
            if (descriptor != null)
            {
                properties["ClientSize"] = TypeDescriptor.CreateProperty(typeof(FormDocumentDesigner), descriptor, new Attribute[] { new DefaultValueAttribute(new System.Drawing.Size(-1, -1)) });
            }
        }

        private bool ShouldSerializeAutoScaleBaseSize()
        {
            if (this.initializing)
            {
                return false;
            }
            return (((Form) base.Component).AutoScale && base.ShadowProperties.Contains("AutoScaleBaseSize"));
        }

        private unsafe void WmWindowPosChanging(ref Message m)
        {
            System.Design.NativeMethods.WINDOWPOS* lParam = (System.Design.NativeMethods.WINDOWPOS*) m.LParam;
            bool inAutoscale = this.inAutoscale;
            if (!inAutoscale)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    inAutoscale = service.Loading;
                }
            }
            if (((inAutoscale && (this.Menu != null)) && ((lParam->flags & 1) == 0)) && (this.IsMenuInherited || this.inAutoscale))
            {
                this.heightDelta = this.GetMenuHeight();
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 70)
            {
                this.WmWindowPosChanging(ref m);
            }
            base.WndProc(ref m);
        }

        private IButtonControl AcceptButton
        {
            get
            {
                return (base.ShadowProperties["AcceptButton"] as IButtonControl);
            }
            set
            {
                ((Form) base.Component).AcceptButton = value;
                base.ShadowProperties["AcceptButton"] = value;
            }
        }

        private System.Drawing.Size AutoScaleBaseSize
        {
            get
            {
                SizeF autoScaleSize = Form.GetAutoScaleSize(((Form) base.Component).Font);
                return new System.Drawing.Size((int) Math.Round((double) autoScaleSize.Width), (int) Math.Round((double) autoScaleSize.Height));
            }
            set
            {
                this.autoScaleBaseSize = value;
                base.ShadowProperties["AutoScaleBaseSize"] = value;
            }
        }

        private bool AutoSize
        {
            get
            {
                return this.autoSize;
            }
            set
            {
                this.autoSize = value;
            }
        }

        private IButtonControl CancelButton
        {
            get
            {
                return (base.ShadowProperties["CancelButton"] as IButtonControl);
            }
            set
            {
                ((Form) base.Component).CancelButton = value;
                base.ShadowProperties["CancelButton"] = value;
            }
        }

        private System.Drawing.Size ClientSize
        {
            get
            {
                if (this.initializing)
                {
                    return new System.Drawing.Size(-1, -1);
                }
                System.Drawing.Size clientSize = new System.Drawing.Size(-1, -1);
                Form component = base.Component as Form;
                if (component != null)
                {
                    clientSize = component.ClientSize;
                    if (component.HorizontalScroll.Visible)
                    {
                        clientSize.Height += SystemInformation.HorizontalScrollBarHeight;
                    }
                    if (component.VerticalScroll.Visible)
                    {
                        clientSize.Width += SystemInformation.VerticalScrollBarWidth;
                    }
                }
                return clientSize;
            }
            set
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((service != null) && service.Loading)
                {
                    this.heightDelta = this.GetMenuHeight();
                }
                ((Form) base.Component).ClientSize = value;
            }
        }

        private bool IsMdiContainer
        {
            get
            {
                return ((Form) this.Control).IsMdiContainer;
            }
            set
            {
                if (!value)
                {
                    base.UnhookChildControls(this.Control);
                }
                ((Form) this.Control).IsMdiContainer = value;
                if (value)
                {
                    base.HookChildControls(this.Control);
                }
            }
        }

        private bool IsMenuInherited
        {
            get
            {
                if ((this.inheritanceAttribute == null) && (this.Menu != null))
                {
                    this.inheritanceAttribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(this.Menu)[typeof(InheritanceAttribute)];
                    if (this.inheritanceAttribute.Equals(InheritanceAttribute.NotInherited))
                    {
                        this.isMenuInherited = false;
                    }
                    else
                    {
                        this.isMenuInherited = true;
                    }
                }
                return this.isMenuInherited;
            }
        }

        internal MainMenu Menu
        {
            get
            {
                return (MainMenu) base.ShadowProperties["Menu"];
            }
            set
            {
                if (value != base.ShadowProperties["Menu"])
                {
                    base.ShadowProperties["Menu"] = value;
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if ((service != null) && !service.Loading)
                    {
                        this.EnsureMenuEditorService(value);
                        if (base.menuEditorService != null)
                        {
                            base.menuEditorService.SetMenu(value);
                        }
                    }
                    if (this.heightDelta == 0)
                    {
                        this.heightDelta = this.GetMenuHeight();
                    }
                }
            }
        }

        private double Opacity
        {
            get
            {
                return (double) base.ShadowProperties["Opacity"];
            }
            set
            {
                if ((value < 0.0) || (value > 1.0))
                {
                    object[] args = new object[] { "value", value.ToString(CultureInfo.CurrentCulture), 0f.ToString(CultureInfo.CurrentCulture), 1f.ToString(CultureInfo.CurrentCulture) };
                    throw new ArgumentException(System.Design.SR.GetString("InvalidBoundArgument", args), "value");
                }
                base.ShadowProperties["Opacity"] = value;
            }
        }

        private bool ShowInTaskbar
        {
            get
            {
                return (bool) base.ShadowProperties["ShowInTaskbar"];
            }
            set
            {
                base.ShadowProperties["ShowInTaskbar"] = value;
            }
        }

        private System.Drawing.Size Size
        {
            get
            {
                return this.Control.Size;
            }
            set
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(base.Component);
                if (service != null)
                {
                    service.OnComponentChanging(base.Component, properties["ClientSize"]);
                }
                this.Control.Size = value;
                if (service != null)
                {
                    service.OnComponentChanged(base.Component, properties["ClientSize"], null, null);
                }
            }
        }

        public override IList SnapLines
        {
            get
            {
                ArrayList snapLines = null;
                base.AddPaddingSnapLines(ref snapLines);
                if (snapLines == null)
                {
                    snapLines = new ArrayList(4);
                }
                if ((this.Control.Padding == Padding.Empty) && (snapLines != null))
                {
                    int num = 0;
                    for (int i = 0; i < snapLines.Count; i++)
                    {
                        SnapLine line = snapLines[i] as SnapLine;
                        if (((line != null) && (line.Filter != null)) && line.Filter.StartsWith("Padding"))
                        {
                            if (line.Filter.Equals("Padding.Left") || line.Filter.Equals("Padding.Top"))
                            {
                                line.AdjustOffset(DesignerUtils.DEFAULTFORMPADDING);
                                num++;
                            }
                            if (line.Filter.Equals("Padding.Right") || line.Filter.Equals("Padding.Bottom"))
                            {
                                line.AdjustOffset(-DesignerUtils.DEFAULTFORMPADDING);
                                num++;
                            }
                            if (num == 4)
                            {
                                return snapLines;
                            }
                        }
                    }
                }
                return snapLines;
            }
        }

        private FormWindowState WindowState
        {
            get
            {
                return (FormWindowState) base.ShadowProperties["WindowState"];
            }
            set
            {
                base.ShadowProperties["WindowState"] = value;
            }
        }
    }
}

