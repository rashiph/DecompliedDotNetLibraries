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

    internal class BaseContextMenuStrip : GroupedContextMenuStrip
    {
        private Component component;
        private ToolStripMenuItem selectionMenuItem;
        private IServiceProvider serviceProvider;

        public BaseContextMenuStrip(IServiceProvider provider, Component component)
        {
            this.serviceProvider = provider;
            this.component = component;
            this.InitializeContextMenu();
        }

        private void AddCodeMenuItem()
        {
            StandardCommandToolStripMenuItem item = new StandardCommandToolStripMenuItem(StandardCommands.ViewCode, System.Design.SR.GetString("ContextMenuViewCode"), "viewcode", this.serviceProvider);
            base.Groups["Code"].Items.Add(item);
        }

        private void AddEditMenuItem()
        {
            StandardCommandToolStripMenuItem item = new StandardCommandToolStripMenuItem(StandardCommands.Cut, System.Design.SR.GetString("ContextMenuCut"), "cut", this.serviceProvider);
            base.Groups["Edit"].Items.Add(item);
            item = new StandardCommandToolStripMenuItem(StandardCommands.Copy, System.Design.SR.GetString("ContextMenuCopy"), "copy", this.serviceProvider);
            base.Groups["Edit"].Items.Add(item);
            item = new StandardCommandToolStripMenuItem(StandardCommands.Paste, System.Design.SR.GetString("ContextMenuPaste"), "paste", this.serviceProvider);
            base.Groups["Edit"].Items.Add(item);
            item = new StandardCommandToolStripMenuItem(StandardCommands.Delete, System.Design.SR.GetString("ContextMenuDelete"), "delete", this.serviceProvider);
            base.Groups["Edit"].Items.Add(item);
        }

        private void AddGridMenuItem()
        {
            StandardCommandToolStripMenuItem item = new StandardCommandToolStripMenuItem(StandardCommands.AlignToGrid, System.Design.SR.GetString("ContextMenuAlignToGrid"), "alignToGrid", this.serviceProvider);
            base.Groups["Grid"].Items.Add(item);
        }

        private void AddLockMenuItem()
        {
            StandardCommandToolStripMenuItem item = new StandardCommandToolStripMenuItem(StandardCommands.LockControls, System.Design.SR.GetString("ContextMenuLockControls"), "lockControls", this.serviceProvider);
            base.Groups["Lock"].Items.Add(item);
        }

        private void AddPropertiesMenuItem()
        {
            StandardCommandToolStripMenuItem item = new StandardCommandToolStripMenuItem(StandardCommands.DocumentOutline, System.Design.SR.GetString("ContextMenuDocumentOutline"), "", this.serviceProvider);
            base.Groups["Properties"].Items.Add(item);
            item = new StandardCommandToolStripMenuItem(MenuCommands.DesignerProperties, System.Design.SR.GetString("ContextMenuProperties"), "properties", this.serviceProvider);
            base.Groups["Properties"].Items.Add(item);
        }

        private void AddVerbMenuItem()
        {
            IMenuCommandService service = (IMenuCommandService) this.serviceProvider.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                foreach (DesignerVerb verb in service.Verbs)
                {
                    DesignerVerbToolStripMenuItem item = new DesignerVerbToolStripMenuItem(verb);
                    base.Groups["Verbs"].Items.Add(item);
                }
            }
        }

        private void AddZorderMenuItem()
        {
            StandardCommandToolStripMenuItem item = new StandardCommandToolStripMenuItem(StandardCommands.BringToFront, System.Design.SR.GetString("ContextMenuBringToFront"), "bringToFront", this.serviceProvider);
            base.Groups["ZOrder"].Items.Add(item);
            item = new StandardCommandToolStripMenuItem(StandardCommands.SendToBack, System.Design.SR.GetString("ContextMenuSendToBack"), "sendToBack", this.serviceProvider);
            base.Groups["ZOrder"].Items.Add(item);
        }

        private void InitializeContextMenu()
        {
            base.Name = "designerContextMenuStrip";
            IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                base.Renderer = (ToolStripProfessionalRenderer) service.Styles["VsRenderer"];
            }
            base.GroupOrdering.AddRange(new string[] { "Code", "ZOrder", "Grid", "Lock", "Verbs", "Custom", "Selection", "Edit", "Properties" });
            this.AddCodeMenuItem();
            this.AddZorderMenuItem();
            this.AddGridMenuItem();
            this.AddLockMenuItem();
            this.AddVerbMenuItem();
            this.RefreshSelectionMenuItem();
            this.AddEditMenuItem();
            this.AddPropertiesMenuItem();
        }

        public override void RefreshItems()
        {
            IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                this.Font = (Font) service.Styles["DialogFont"];
            }
            foreach (ToolStripItem item in this.Items)
            {
                StandardCommandToolStripMenuItem item2 = item as StandardCommandToolStripMenuItem;
                if (item2 != null)
                {
                    item2.RefreshItem();
                }
            }
            this.RefreshSelectionMenuItem();
        }

        private void RefreshSelectionMenuItem()
        {
            int index = -1;
            if (this.selectionMenuItem != null)
            {
                index = this.Items.IndexOf(this.selectionMenuItem);
                base.Groups["Selection"].Items.Remove(this.selectionMenuItem);
                this.Items.Remove(this.selectionMenuItem);
            }
            ArrayList list = new ArrayList();
            int count = 0;
            ISelectionService service = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
            IDesignerHost host = this.serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if ((service != null) && (host != null))
            {
                IComponent rootComponent = host.RootComponent;
                Control primarySelection = service.PrimarySelection as Control;
                if (((primarySelection != null) && (rootComponent != null)) && (primarySelection != rootComponent))
                {
                    for (Control control2 = primarySelection.Parent; control2 != null; control2 = control2.Parent)
                    {
                        if (control2.Site != null)
                        {
                            list.Add(control2);
                            count++;
                        }
                        if (control2 == rootComponent)
                        {
                            break;
                        }
                    }
                }
                else if (service.PrimarySelection is ToolStripItem)
                {
                    ToolStripItem component = service.PrimarySelection as ToolStripItem;
                    ToolStripItemDesigner designer = host.GetDesigner(component) as ToolStripItemDesigner;
                    if (designer != null)
                    {
                        list = designer.AddParentTree();
                        count = list.Count;
                    }
                }
            }
            if (count > 0)
            {
                this.selectionMenuItem = new ToolStripMenuItem();
                IUIService service2 = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (service2 != null)
                {
                    this.selectionMenuItem.DropDown.Renderer = (ToolStripProfessionalRenderer) service2.Styles["VsRenderer"];
                    this.selectionMenuItem.DropDown.Font = (Font) service2.Styles["DialogFont"];
                }
                this.selectionMenuItem.Text = System.Design.SR.GetString("ContextMenuSelect");
                foreach (Component component2 in list)
                {
                    ToolStripMenuItem item2 = new SelectToolStripMenuItem(component2, this.serviceProvider);
                    this.selectionMenuItem.DropDownItems.Add(item2);
                }
                base.Groups["Selection"].Items.Add(this.selectionMenuItem);
                if (index != -1)
                {
                    this.Items.Insert(index, this.selectionMenuItem);
                }
            }
        }

        private class SelectToolStripMenuItem : ToolStripMenuItem
        {
            private bool _cachedImage;
            private System.Drawing.Image _image;
            private System.Type _itemType;
            private Component comp;
            private IServiceProvider serviceProvider;
            private static string systemWindowsFormsNamespace = typeof(ToolStripItem).Namespace;

            public SelectToolStripMenuItem(Component c, IServiceProvider provider)
            {
                this.comp = c;
                this.serviceProvider = provider;
                string fullName = null;
                if (this.comp != null)
                {
                    ISite site = this.comp.Site;
                    if (site != null)
                    {
                        INestedSite site2 = site as INestedSite;
                        if ((site2 != null) && !string.IsNullOrEmpty(site2.FullName))
                        {
                            fullName = site2.FullName;
                        }
                        else if (!string.IsNullOrEmpty(site.Name))
                        {
                            fullName = site.Name;
                        }
                    }
                }
                this.Text = System.Design.SR.GetString("ToolStripSelectMenuItem", new object[] { fullName });
                this._itemType = c.GetType();
            }

            protected override void OnClick(EventArgs e)
            {
                ISelectionService service = this.serviceProvider.GetService(typeof(ISelectionService)) as ISelectionService;
                if (service != null)
                {
                    service.SetSelectedComponents(new object[] { this.comp }, SelectionTypes.Replace);
                }
            }

            public override System.Drawing.Image Image
            {
                get
                {
                    if (!this._cachedImage)
                    {
                        this._cachedImage = true;
                        ToolboxItem toolboxItem = ToolboxService.GetToolboxItem(this._itemType);
                        if (toolboxItem != null)
                        {
                            this._image = toolboxItem.Bitmap;
                        }
                        else if (this._itemType.Namespace == systemWindowsFormsNamespace)
                        {
                            this._image = ToolboxBitmapAttribute.GetImageFromResource(this._itemType, null, false);
                        }
                        if (this._image == null)
                        {
                            this._image = ToolboxBitmapAttribute.GetImageFromResource(this.comp.GetType(), null, false);
                        }
                    }
                    return this._image;
                }
                set
                {
                    this._image = value;
                    this._cachedImage = true;
                }
            }
        }
    }
}

