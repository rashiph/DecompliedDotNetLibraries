namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class ChangeToolStripParentVerb
    {
        private ToolStripDesigner _designer;
        private IDesignerHost _host;
        private IServiceProvider _provider;
        private IComponentChangeService componentChangeSvc;

        internal ChangeToolStripParentVerb(string text, ToolStripDesigner designer)
        {
            this._designer = designer;
            this._provider = designer.Component.Site;
            this._host = (IDesignerHost) this._provider.GetService(typeof(IDesignerHost));
            this.componentChangeSvc = (IComponentChangeService) this._provider.GetService(typeof(IComponentChangeService));
        }

        public void ChangeParent()
        {
            Cursor current = Cursor.Current;
            DesignerTransaction transaction = this._host.CreateTransaction("Add ToolStripContainer Transaction");
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                Control rootComponent = this._host.RootComponent as Control;
                ParentControlDesigner designer = this._host.GetDesigner(rootComponent) as ParentControlDesigner;
                if (designer != null)
                {
                    ToolStrip component = this._designer.Component as ToolStrip;
                    if (((component != null) && (this._designer != null)) && ((this._designer.Component != null) && (this._provider != null)))
                    {
                        (this._provider.GetService(typeof(DesignerActionUIService)) as DesignerActionUIService).HideUI(component);
                    }
                    ToolboxItem tool = new ToolboxItem(typeof(ToolStripContainer));
                    OleDragDropHandler oleDragHandler = designer.GetOleDragHandler();
                    if (oleDragHandler != null)
                    {
                        ToolStripContainer container = oleDragHandler.CreateTool(tool, rootComponent, 0, 0, 0, 0, false, false)[0] as ToolStripContainer;
                        if ((container != null) && (component != null))
                        {
                            IComponentChangeService service = this._provider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            Control parent = this.GetParent(container, component);
                            PropertyDescriptor member = TypeDescriptor.GetProperties(parent)["Controls"];
                            Control control3 = component.Parent;
                            if (control3 != null)
                            {
                                service.OnComponentChanging(control3, member);
                                control3.Controls.Remove(component);
                            }
                            if (parent != null)
                            {
                                service.OnComponentChanging(parent, member);
                                parent.Controls.Add(component);
                            }
                            if (((service != null) && (control3 != null)) && (parent != null))
                            {
                                service.OnComponentChanged(control3, member, null, null);
                                service.OnComponentChanged(parent, member, null, null);
                            }
                            ISelectionService service3 = this._provider.GetService(typeof(ISelectionService)) as ISelectionService;
                            if (service3 != null)
                            {
                                service3.SetSelectedComponents(new IComponent[] { container });
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (exception is InvalidOperationException)
                {
                    ((IUIService) this._provider.GetService(typeof(IUIService))).ShowError(exception.Message);
                }
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
                Cursor.Current = current;
            }
        }

        private Control GetParent(ToolStripContainer container, Control c)
        {
            Control contentPanel = container.ContentPanel;
            DockStyle dock = c.Dock;
            if (c.Parent is ToolStripPanel)
            {
                dock = c.Parent.Dock;
            }
            foreach (Control control2 in container.Controls)
            {
                if ((control2 is ToolStripPanel) && (control2.Dock == dock))
                {
                    return control2;
                }
            }
            return contentPanel;
        }
    }
}

