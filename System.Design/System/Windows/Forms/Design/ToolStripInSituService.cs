namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class ToolStripInSituService : ISupportInSituService, IDisposable
    {
        private IComponentChangeService componentChangeSvc;
        private IDesignerHost designerHost;
        private IServiceProvider sp;
        private ToolStripDesigner toolDesigner;
        private ToolStripItemDesigner toolItemDesigner;
        private ToolStripKeyboardHandlingService toolStripKeyBoardService;

        public ToolStripInSituService(IServiceProvider provider)
        {
            this.sp = provider;
            this.designerHost = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
            if (this.designerHost != null)
            {
                this.designerHost.AddService(typeof(ISupportInSituService), this);
            }
            this.componentChangeSvc = (IComponentChangeService) this.designerHost.GetService(typeof(IComponentChangeService));
            if (this.componentChangeSvc != null)
            {
                this.componentChangeSvc.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
            }
        }

        public void Dispose()
        {
            if (this.toolDesigner != null)
            {
                this.toolDesigner.Dispose();
                this.toolDesigner = null;
            }
            if (this.toolItemDesigner != null)
            {
                this.toolItemDesigner.Dispose();
                this.toolItemDesigner = null;
            }
            if (this.componentChangeSvc != null)
            {
                this.componentChangeSvc.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                this.componentChangeSvc = null;
            }
        }

        public IntPtr GetEditWindow()
        {
            IntPtr zero = IntPtr.Zero;
            if (((this.toolDesigner != null) && (this.toolDesigner.Editor != null)) && (this.toolDesigner.Editor.EditBox != null))
            {
                return (this.toolDesigner.Editor.EditBox.Visible ? this.toolDesigner.Editor.EditBox.Handle : zero);
            }
            if (((this.toolItemDesigner != null) && (this.toolItemDesigner.Editor != null)) && (this.toolItemDesigner.Editor.EditBox != null))
            {
                zero = this.toolItemDesigner.Editor.EditBox.Visible ? this.toolItemDesigner.Editor.EditBox.Handle : zero;
            }
            return zero;
        }

        public void HandleKeyChar()
        {
            if ((this.toolDesigner != null) || (this.toolItemDesigner != null))
            {
                if (this.toolDesigner != null)
                {
                    this.toolDesigner.ShowEditNode(false);
                }
                else if (this.toolItemDesigner != null)
                {
                    ToolStripMenuItemDesigner toolItemDesigner = this.toolItemDesigner as ToolStripMenuItemDesigner;
                    if (toolItemDesigner != null)
                    {
                        ISelectionService service = (ISelectionService) this.sp.GetService(typeof(ISelectionService));
                        if (service != null)
                        {
                            object primarySelection = service.PrimarySelection;
                            if (primarySelection == null)
                            {
                                primarySelection = this.ToolStripKeyBoardService.SelectedDesignerControl;
                            }
                            if ((primarySelection is DesignerToolStripControlHost) || (primarySelection is ToolStripDropDown))
                            {
                                toolItemDesigner.EditTemplateNode(false);
                            }
                            else
                            {
                                toolItemDesigner.ShowEditNode(false);
                            }
                        }
                    }
                    else
                    {
                        this.toolItemDesigner.ShowEditNode(false);
                    }
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            bool flag = false;
            foreach (IComponent component in this.designerHost.Container.Components)
            {
                if (component is ToolStrip)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag && (((ToolStripInSituService) this.sp.GetService(typeof(ISupportInSituService))) != null))
            {
                this.designerHost.RemoveService(typeof(ISupportInSituService));
            }
        }

        public bool IgnoreMessages
        {
            get
            {
                ISelectionService service = (ISelectionService) this.sp.GetService(typeof(ISelectionService));
                IDesignerHost host = (IDesignerHost) this.sp.GetService(typeof(IDesignerHost));
                if ((service != null) && (host != null))
                {
                    IComponent primarySelection = service.PrimarySelection as IComponent;
                    if (primarySelection == null)
                    {
                        primarySelection = (IComponent) this.ToolStripKeyBoardService.SelectedDesignerControl;
                    }
                    if (primarySelection != null)
                    {
                        DesignerToolStripControlHost host2 = primarySelection as DesignerToolStripControlHost;
                        if (host2 != null)
                        {
                            ToolStripDropDown currentParent = host2.GetCurrentParent() as ToolStripDropDown;
                            if (currentParent == null)
                            {
                                MenuStrip component = host2.GetCurrentParent() as MenuStrip;
                                if (component != null)
                                {
                                    this.toolDesigner = host.GetDesigner(component) as ToolStripDesigner;
                                    if (this.toolDesigner != null)
                                    {
                                        this.toolItemDesigner = null;
                                        return true;
                                    }
                                }
                            }
                            else
                            {
                                ToolStripDropDownItem ownerItem = currentParent.OwnerItem as ToolStripDropDownItem;
                                if (ownerItem != null)
                                {
                                    if (ownerItem is ToolStripOverflowButton)
                                    {
                                        return false;
                                    }
                                    this.toolItemDesigner = host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner;
                                    if (this.toolItemDesigner != null)
                                    {
                                        this.toolDesigner = null;
                                        return true;
                                    }
                                }
                            }
                        }
                        else if (primarySelection is ToolStripDropDown)
                        {
                            ToolStripDropDownDesigner designer = host.GetDesigner(primarySelection) as ToolStripDropDownDesigner;
                            if (designer != null)
                            {
                                ToolStripMenuItem designerMenuItem = designer.DesignerMenuItem;
                                if (designerMenuItem != null)
                                {
                                    this.toolItemDesigner = host.GetDesigner(designerMenuItem) as ToolStripItemDesigner;
                                    if (this.toolItemDesigner != null)
                                    {
                                        this.toolDesigner = null;
                                        return true;
                                    }
                                }
                            }
                        }
                        else if (primarySelection is MenuStrip)
                        {
                            this.toolDesigner = host.GetDesigner(primarySelection) as ToolStripDesigner;
                            if (this.toolDesigner != null)
                            {
                                this.toolItemDesigner = null;
                                return true;
                            }
                        }
                        else if (primarySelection is ToolStripMenuItem)
                        {
                            this.toolItemDesigner = host.GetDesigner(primarySelection) as ToolStripItemDesigner;
                            if (this.toolItemDesigner != null)
                            {
                                this.toolDesigner = null;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
        }

        private ToolStripKeyboardHandlingService ToolStripKeyBoardService
        {
            get
            {
                if (this.toolStripKeyBoardService == null)
                {
                    this.toolStripKeyBoardService = (ToolStripKeyboardHandlingService) this.sp.GetService(typeof(ToolStripKeyboardHandlingService));
                }
                return this.toolStripKeyBoardService;
            }
        }
    }
}

