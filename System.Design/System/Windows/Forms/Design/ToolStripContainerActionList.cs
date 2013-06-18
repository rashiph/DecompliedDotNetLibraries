namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripContainerActionList : DesignerActionList
    {
        private ToolStripContainer container;
        private IDesignerHost host;
        private IServiceProvider provider;

        public ToolStripContainerActionList(ToolStripContainer control) : base(control)
        {
            this.container = control;
            this.provider = this.container.Site;
            this.host = this.provider.GetService(typeof(IDesignerHost)) as IDesignerHost;
        }

        private void ChangeProperty(Component comp, string propertyName, object value)
        {
            if (this.host != null)
            {
                ToolStripPanel panel = comp as ToolStripPanel;
                ToolStripPanelDesigner designer = this.host.GetDesigner(comp) as ToolStripPanelDesigner;
                if (propertyName.Equals("Visible"))
                {
                    foreach (Control control in panel.Controls)
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(control)["Visible"];
                        if (descriptor != null)
                        {
                            descriptor.SetValue(control, value);
                        }
                    }
                    if (!((bool) value))
                    {
                        if (panel != null)
                        {
                            panel.Padding = new Padding(0);
                        }
                        if ((designer != null) && (designer.ToolStripPanelSelectorGlyph != null))
                        {
                            designer.ToolStripPanelSelectorGlyph.IsExpanded = false;
                        }
                    }
                }
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(comp)[propertyName];
                if (descriptor2 != null)
                {
                    descriptor2.SetValue(comp, value);
                }
                SelectionManager service = (SelectionManager) this.provider.GetService(typeof(SelectionManager));
                if (service != null)
                {
                    service.Refresh();
                }
                if (designer != null)
                {
                    designer.InvalidateGlyph();
                }
            }
        }

        private Control GetParent(Control c)
        {
            Control contentPanel = this.container.ContentPanel;
            DockStyle dock = c.Dock;
            foreach (Control control2 in this.container.Controls)
            {
                if ((control2 is ToolStripPanel) && (control2.Dock == dock))
                {
                    return control2;
                }
            }
            return contentPanel;
        }

        private object GetProperty(Component comp, string propertyName)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(comp)[propertyName];
            if (descriptor != null)
            {
                return descriptor.GetValue(comp);
            }
            return null;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionHeaderItem(System.Design.SR.GetString("ToolStripContainerActionList_Visible"), System.Design.SR.GetString("ToolStripContainerActionList_Show")));
            items.Add(new DesignerActionPropertyItem("TopVisible", System.Design.SR.GetString("ToolStripContainerActionList_Top"), System.Design.SR.GetString("ToolStripContainerActionList_Show"), System.Design.SR.GetString("ToolStripContainerActionList_TopDesc")));
            items.Add(new DesignerActionPropertyItem("BottomVisible", System.Design.SR.GetString("ToolStripContainerActionList_Bottom"), System.Design.SR.GetString("ToolStripContainerActionList_Show"), System.Design.SR.GetString("ToolStripContainerActionList_BottomDesc")));
            items.Add(new DesignerActionPropertyItem("LeftVisible", System.Design.SR.GetString("ToolStripContainerActionList_Left"), System.Design.SR.GetString("ToolStripContainerActionList_Show"), System.Design.SR.GetString("ToolStripContainerActionList_LeftDesc")));
            items.Add(new DesignerActionPropertyItem("RightVisible", System.Design.SR.GetString("ToolStripContainerActionList_Right"), System.Design.SR.GetString("ToolStripContainerActionList_Show"), System.Design.SR.GetString("ToolStripContainerActionList_RightDesc")));
            if (!this.IsDockFilled)
            {
                bool flag = true;
                if ((this.host != null) && (this.host.RootComponent is UserControl))
                {
                    flag = false;
                }
                items.Add(new DesignerActionMethodItem(this, "SetDockToForm", flag ? System.Design.SR.GetString("DesignerShortcutDockInForm") : System.Design.SR.GetString("DesignerShortcutDockInUserControl")));
            }
            if (this.ProvideReparent)
            {
                items.Add(new DesignerActionMethodItem(this, "ReparentControls", System.Design.SR.GetString("DesignerShortcutReparentControls")));
            }
            return items;
        }

        public void ReparentControls()
        {
            if (this.host != null)
            {
                Control rootComponent = this.host.RootComponent as Control;
                if (((rootComponent != null) && (this.container.Parent == rootComponent)) && (rootComponent.Controls.Count > 1))
                {
                    Control contentPanel = this.container.ContentPanel;
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(contentPanel)["AutoScroll"];
                    if (descriptor != null)
                    {
                        descriptor.SetValue(contentPanel, true);
                    }
                    DesignerTransaction transaction = this.host.CreateTransaction("Reparent Transaction");
                    try
                    {
                        Control[] array = new Control[rootComponent.Controls.Count];
                        rootComponent.Controls.CopyTo(array, 0);
                        foreach (Control control3 in array)
                        {
                            if ((control3 != this.container) && !(control3 is MdiClient))
                            {
                                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(control3)[typeof(InheritanceAttribute)];
                                if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.InheritedReadOnly))
                                {
                                    IComponentChangeService service = this.provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                                    if (control3 is ToolStrip)
                                    {
                                        contentPanel = this.GetParent(control3);
                                    }
                                    else
                                    {
                                        contentPanel = this.container.ContentPanel;
                                    }
                                    PropertyDescriptor member = TypeDescriptor.GetProperties(contentPanel)["Controls"];
                                    Control parent = control3.Parent;
                                    if (parent != null)
                                    {
                                        if (service != null)
                                        {
                                            service.OnComponentChanging(parent, member);
                                        }
                                        parent.Controls.Remove(control3);
                                    }
                                    if (service != null)
                                    {
                                        service.OnComponentChanging(contentPanel, member);
                                    }
                                    contentPanel.Controls.Add(control3);
                                    if ((service != null) && (parent != null))
                                    {
                                        service.OnComponentChanged(parent, member, null, null);
                                    }
                                    if (service != null)
                                    {
                                        service.OnComponentChanged(contentPanel, member, null, null);
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        if (transaction != null)
                        {
                            transaction.Cancel();
                            transaction = null;
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                            transaction = null;
                        }
                        ISelectionService service2 = this.provider.GetService(typeof(ISelectionService)) as ISelectionService;
                        if (service2 != null)
                        {
                            service2.SetSelectedComponents(new IComponent[] { contentPanel });
                        }
                    }
                }
            }
        }

        public void SetDockToForm()
        {
            if (this.host != null)
            {
                Control rootComponent = this.host.RootComponent as Control;
                if ((rootComponent != null) && (this.container.Parent != rootComponent))
                {
                    rootComponent.Controls.Add(this.container);
                }
                if (!this.IsDockFilled)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.container)["Dock"];
                    if (descriptor != null)
                    {
                        descriptor.SetValue(this.container, DockStyle.Fill);
                    }
                }
            }
        }

        public bool BottomVisible
        {
            get
            {
                return (bool) this.GetProperty(this.container, "BottomToolStripPanelVisible");
            }
            set
            {
                if (value != this.BottomVisible)
                {
                    this.ChangeProperty(this.container, "BottomToolStripPanelVisible", value);
                }
            }
        }

        private bool IsDockFilled
        {
            get
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.container)["Dock"];
                if ((descriptor != null) && (((DockStyle) descriptor.GetValue(this.container)) != DockStyle.Fill))
                {
                    return false;
                }
                return true;
            }
        }

        public bool LeftVisible
        {
            get
            {
                return (bool) this.GetProperty(this.container, "LeftToolStripPanelVisible");
            }
            set
            {
                if (value != this.LeftVisible)
                {
                    this.ChangeProperty(this.container, "LeftToolStripPanelVisible", value);
                }
            }
        }

        private bool ProvideReparent
        {
            get
            {
                if (this.host != null)
                {
                    Control rootComponent = this.host.RootComponent as Control;
                    if (((rootComponent != null) && (this.container.Parent == rootComponent)) && (this.IsDockFilled && (rootComponent.Controls.Count > 1)))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool RightVisible
        {
            get
            {
                return (bool) this.GetProperty(this.container, "RightToolStripPanelVisible");
            }
            set
            {
                if (value != this.RightVisible)
                {
                    this.ChangeProperty(this.container, "RightToolStripPanelVisible", value);
                }
            }
        }

        public bool TopVisible
        {
            get
            {
                return (bool) this.GetProperty(this.container, "TopToolStripPanelVisible");
            }
            set
            {
                if (value != this.TopVisible)
                {
                    this.ChangeProperty(this.container, "TopToolStripPanelVisible", value);
                }
            }
        }
    }
}

