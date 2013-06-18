namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripDropDownDesigner : ComponentDesigner
    {
        private uint _editingCollection;
        internal ToolStripMenuItem currentParent;
        private MenuStrip designMenu;
        private ToolStripDropDown dropDown;
        private ControlBodyGlyph dummyToolStripGlyph;
        private IDesignerHost host;
        private ToolStripMenuItem menuItem;
        private INestedContainer nestedContainer;
        private FormDocumentDesigner parentFormDesigner;
        private MainMenu parentMenu;
        private bool selected;
        private ISelectionService selSvc;
        private UndoEngine undoEngine;

        internal void AddSelectionGlyphs()
        {
            SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
            if (service != null)
            {
                this.AddSelectionGlyphs(service, this.selSvc);
            }
        }

        private void AddSelectionGlyphs(SelectionManager selMgr, ISelectionService selectionService)
        {
            ICollection selectedComponents = selectionService.GetSelectedComponents();
            GlyphCollection glyphs = new GlyphCollection();
            foreach (object obj2 in selectedComponents)
            {
                ToolStripItem component = obj2 as ToolStripItem;
                if (component != null)
                {
                    ToolStripItemDesigner designer = (ToolStripItemDesigner) this.host.GetDesigner(component);
                    if (designer != null)
                    {
                        designer.GetGlyphs(ref glyphs, new ResizeBehavior(component.Site));
                    }
                }
            }
            if (glyphs.Count > 0)
            {
                selMgr.SelectionGlyphAdorner.Glyphs.AddRange(glyphs);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.selSvc != null)
                {
                    this.selSvc.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                    this.selSvc.SelectionChanging -= new EventHandler(this.OnSelectionChanging);
                }
                this.DisposeMenu();
                if (this.designMenu != null)
                {
                    this.designMenu.Dispose();
                    this.designMenu = null;
                }
                if (this.dummyToolStripGlyph != null)
                {
                    this.dummyToolStripGlyph = null;
                }
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undone -= new EventHandler(this.OnUndone);
                }
            }
            base.Dispose(disposing);
        }

        private void DisposeMenu()
        {
            this.HideMenu();
            Control rootComponent = this.host.RootComponent as Control;
            if (rootComponent != null)
            {
                if (this.designMenu != null)
                {
                    rootComponent.Controls.Remove(this.designMenu);
                }
                if (this.menuItem != null)
                {
                    if (this.nestedContainer != null)
                    {
                        this.nestedContainer.Dispose();
                        this.nestedContainer = null;
                    }
                    this.menuItem.Dispose();
                    this.menuItem = null;
                }
            }
        }

        private void HideMenu()
        {
            if (this.menuItem != null)
            {
                if ((this.parentMenu != null) && (this.parentFormDesigner != null))
                {
                    this.parentFormDesigner.Menu = this.parentMenu;
                }
                this.selected = false;
                if (this.host.RootComponent is Control)
                {
                    this.menuItem.DropDown.AutoClose = true;
                    this.menuItem.HideDropDown();
                    this.menuItem.Visible = false;
                    this.designMenu.Visible = false;
                    ToolStripAdornerWindowService service = (ToolStripAdornerWindowService) this.GetService(typeof(ToolStripAdornerWindowService));
                    if (service != null)
                    {
                        service.Invalidate();
                    }
                    if (((BehaviorService) this.GetService(typeof(BehaviorService))) != null)
                    {
                        if (this.dummyToolStripGlyph != null)
                        {
                            SelectionManager manager = (SelectionManager) this.GetService(typeof(SelectionManager));
                            if (manager != null)
                            {
                                if (manager.BodyGlyphAdorner.Glyphs.Contains(this.dummyToolStripGlyph))
                                {
                                    manager.BodyGlyphAdorner.Glyphs.Remove(this.dummyToolStripGlyph);
                                }
                                manager.Refresh();
                            }
                        }
                        this.dummyToolStripGlyph = null;
                    }
                    if (this.menuItem != null)
                    {
                        ToolStripMenuItemDesigner designer = this.host.GetDesigner(this.menuItem) as ToolStripMenuItemDesigner;
                        if (designer != null)
                        {
                            designer.UnHookEvents();
                            designer.RemoveTypeHereNode(this.menuItem);
                        }
                    }
                }
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this.host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (((ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService))) == null)
            {
                ToolStripKeyboardHandlingService service = new ToolStripKeyboardHandlingService(component.Site);
            }
            if (((ISupportInSituService) this.GetService(typeof(ISupportInSituService))) == null)
            {
                ISupportInSituService service2 = new ToolStripInSituService(base.Component.Site);
            }
            this.dropDown = (ToolStripDropDown) base.Component;
            this.dropDown.Visible = false;
            this.AutoClose = this.dropDown.AutoClose;
            this.AllowDrop = this.dropDown.AllowDrop;
            this.selSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (this.selSvc != null)
            {
                if ((this.host != null) && !this.host.Loading)
                {
                    this.selSvc.SetSelectedComponents(new IComponent[] { this.host.RootComponent }, SelectionTypes.Replace);
                }
                this.selSvc.SelectionChanging += new EventHandler(this.OnSelectionChanging);
                this.selSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            this.designMenu = new MenuStrip();
            this.designMenu.Visible = false;
            this.designMenu.AutoSize = false;
            this.designMenu.Dock = DockStyle.Top;
            Control rootComponent = this.host.RootComponent as Control;
            if (rootComponent != null)
            {
                this.menuItem = new ToolStripMenuItem();
                this.menuItem.BackColor = SystemColors.Window;
                this.menuItem.Name = base.Component.Site.Name;
                this.menuItem.Text = (this.dropDown != null) ? this.dropDown.GetType().Name : this.menuItem.Name;
                this.designMenu.Items.Add(this.menuItem);
                rootComponent.Controls.Add(this.designMenu);
                this.designMenu.SendToBack();
                this.nestedContainer = this.GetService(typeof(INestedContainer)) as INestedContainer;
                if (this.nestedContainer != null)
                {
                    this.nestedContainer.Add(this.menuItem, "ContextMenuStrip");
                }
            }
            new EditorServiceContext(this, TypeDescriptor.GetProperties(base.Component)["Items"], System.Design.SR.GetString("ToolStripItemCollectionEditorVerb"));
            if (this.undoEngine == null)
            {
                this.undoEngine = this.GetService(typeof(UndoEngine)) as UndoEngine;
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undone += new EventHandler(this.OnUndone);
                }
            }
        }

        private bool IsContextMenuStripItemSelected(ISelectionService selectionService)
        {
            bool flag = false;
            if (this.menuItem != null)
            {
                ToolStripDropDown dropDown = null;
                IComponent primarySelection = (IComponent) selectionService.PrimarySelection;
                if ((primarySelection == null) && this.dropDown.Visible)
                {
                    ToolStripKeyboardHandlingService service = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
                    if (service != null)
                    {
                        primarySelection = (IComponent) service.SelectedDesignerControl;
                    }
                }
                if (primarySelection is ToolStripDropDownItem)
                {
                    ToolStripDropDownItem item = primarySelection as ToolStripDropDownItem;
                    if ((item != null) && (item == this.menuItem))
                    {
                        dropDown = this.menuItem.DropDown;
                    }
                    else
                    {
                        ToolStripMenuItemDesigner designer = (ToolStripMenuItemDesigner) this.host.GetDesigner(primarySelection);
                        if (designer != null)
                        {
                            dropDown = designer.GetFirstDropDown((ToolStripDropDownItem) primarySelection);
                        }
                    }
                }
                else if (primarySelection is ToolStripItem)
                {
                    ToolStripDropDown currentParent = ((ToolStripItem) primarySelection).GetCurrentParent() as ToolStripDropDown;
                    if (currentParent == null)
                    {
                        currentParent = ((ToolStripItem) primarySelection).Owner as ToolStripDropDown;
                    }
                    if ((currentParent != null) && currentParent.Visible)
                    {
                        ToolStripItem ownerItem = currentParent.OwnerItem;
                        if ((ownerItem != null) && (ownerItem == this.menuItem))
                        {
                            dropDown = this.menuItem.DropDown;
                        }
                        else
                        {
                            ToolStripMenuItemDesigner designer2 = (ToolStripMenuItemDesigner) this.host.GetDesigner(ownerItem);
                            if (designer2 != null)
                            {
                                dropDown = designer2.GetFirstDropDown((ToolStripDropDownItem) ownerItem);
                            }
                        }
                    }
                }
                if ((dropDown != null) && (dropDown.OwnerItem == this.menuItem))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if ((base.Component != null) && (this.menuItem != null))
            {
                ISelectionService selectionService = (ISelectionService) sender;
                if (selectionService.GetComponentSelected(this.menuItem))
                {
                    selectionService.SetSelectedComponents(new IComponent[] { base.Component }, SelectionTypes.Replace);
                }
                if ((!base.Component.Equals(selectionService.PrimarySelection) || !this.selected) && (this.IsContextMenuStripItemSelected(selectionService) || base.Component.Equals(selectionService.PrimarySelection)))
                {
                    if (!this.dropDown.Visible)
                    {
                        this.ShowMenu();
                    }
                    SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                    if (service != null)
                    {
                        if (this.dummyToolStripGlyph != null)
                        {
                            service.BodyGlyphAdorner.Glyphs.Insert(0, this.dummyToolStripGlyph);
                        }
                        this.AddSelectionGlyphs(service, selectionService);
                    }
                }
            }
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            ISelectionService selectionService = (ISelectionService) sender;
            bool flag = this.IsContextMenuStripItemSelected(selectionService) || base.Component.Equals(selectionService.PrimarySelection);
            if (this.selected && !flag)
            {
                this.HideMenu();
            }
        }

        private void OnUndone(object source, EventArgs e)
        {
            if ((this.selSvc != null) && base.Component.Equals(this.selSvc.PrimarySelection))
            {
                this.HideMenu();
                this.ShowMenu();
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "AutoClose", "SettingsKey", "RightToLeft", "AllowDrop" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ToolStripDropDownDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private void ResetAllowDrop()
        {
            base.ShadowProperties["AllowDrop"] = false;
        }

        private void ResetAutoClose()
        {
            base.ShadowProperties["AutoClose"] = true;
        }

        private void ResetRightToLeft()
        {
            this.RightToLeft = System.Windows.Forms.RightToLeft.No;
        }

        public void ResetSettingsKey()
        {
            if (base.Component is IPersistComponentSettings)
            {
                this.SettingsKey = null;
            }
        }

        private void RestoreAllowDrop()
        {
            this.dropDown.AutoClose = (bool) base.ShadowProperties["AllowDrop"];
        }

        private void RestoreAutoClose()
        {
            this.dropDown.AutoClose = (bool) base.ShadowProperties["AutoClose"];
        }

        private bool ShouldSerializeAllowDrop()
        {
            return this.AllowDrop;
        }

        private bool ShouldSerializeAutoClose()
        {
            bool flag = (bool) base.ShadowProperties["AutoClose"];
            return !flag;
        }

        private bool ShouldSerializeRightToLeft()
        {
            return (this.RightToLeft != System.Windows.Forms.RightToLeft.No);
        }

        private bool ShouldSerializeSettingsKey()
        {
            IPersistComponentSettings component = base.Component as IPersistComponentSettings;
            return (((component != null) && component.SaveSettings) && (this.SettingsKey != null));
        }

        public void ShowMenu()
        {
            if (this.menuItem != null)
            {
                Control parent = this.designMenu.Parent;
                Form component = parent as Form;
                if (component != null)
                {
                    this.parentFormDesigner = this.host.GetDesigner(component) as FormDocumentDesigner;
                    if ((this.parentFormDesigner != null) && (this.parentFormDesigner.Menu != null))
                    {
                        this.parentMenu = this.parentFormDesigner.Menu;
                        this.parentFormDesigner.Menu = null;
                    }
                }
                this.selected = true;
                this.designMenu.Visible = true;
                this.designMenu.BringToFront();
                this.menuItem.Visible = true;
                if ((this.currentParent != null) && (this.currentParent != this.menuItem))
                {
                    ToolStripMenuItemDesigner designer = this.host.GetDesigner(this.currentParent) as ToolStripMenuItemDesigner;
                    if (designer != null)
                    {
                        designer.RemoveTypeHereNode(this.currentParent);
                    }
                }
                this.menuItem.DropDown = this.dropDown;
                this.menuItem.DropDown.OwnerItem = this.menuItem;
                if (this.dropDown.Items.Count > 0)
                {
                    ToolStripItem[] array = new ToolStripItem[this.dropDown.Items.Count];
                    this.dropDown.Items.CopyTo(array, 0);
                    foreach (ToolStripItem item in array)
                    {
                        if (item is DesignerToolStripControlHost)
                        {
                            this.dropDown.Items.Remove(item);
                        }
                    }
                }
                ToolStripMenuItemDesigner designer2 = (ToolStripMenuItemDesigner) this.host.GetDesigner(this.menuItem);
                BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                if (service != null)
                {
                    if ((designer2 != null) && (parent != null))
                    {
                        Rectangle parentBounds = service.ControlRectInAdornerWindow(parent);
                        if (ToolStripDesigner.IsGlyphTotallyVisible(service.ControlRectInAdornerWindow(this.designMenu), parentBounds))
                        {
                            designer2.InitializeDropDown();
                        }
                    }
                    if (this.dummyToolStripGlyph == null)
                    {
                        Point pos = service.ControlToAdornerWindow(this.designMenu);
                        Rectangle bounds = this.designMenu.Bounds;
                        bounds.Offset(pos);
                        this.dummyToolStripGlyph = new ControlBodyGlyph(bounds, Cursor.Current, this.menuItem, new ContextMenuStripBehavior(this.menuItem));
                        SelectionManager manager = (SelectionManager) this.GetService(typeof(SelectionManager));
                        if (manager != null)
                        {
                            manager.BodyGlyphAdorner.Glyphs.Insert(0, this.dummyToolStripGlyph);
                        }
                    }
                    ToolStripKeyboardHandlingService service2 = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
                    if (service2 != null)
                    {
                        int num = this.dropDown.Items.Count - 1;
                        if (num >= 0)
                        {
                            service2.SelectedDesignerControl = this.dropDown.Items[num];
                        }
                    }
                }
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                ContextMenuStripActionList list = new ContextMenuStripActionList(this);
                if (list != null)
                {
                    lists.Add(list);
                }
                DesignerVerbCollection verbs = this.Verbs;
                if ((verbs != null) && (verbs.Count != 0))
                {
                    DesignerVerb[] array = new DesignerVerb[verbs.Count];
                    verbs.CopyTo(array, 0);
                    lists.Add(new DesignerActionVerbList(array));
                }
                return lists;
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
                return ((ToolStrip) base.Component).Items;
            }
        }

        private bool AutoClose
        {
            get
            {
                return (bool) base.ShadowProperties["AutoClose"];
            }
            set
            {
                base.ShadowProperties["AutoClose"] = value;
            }
        }

        public ToolStripMenuItem DesignerMenuItem
        {
            get
            {
                return this.menuItem;
            }
        }

        internal bool EditingCollection
        {
            get
            {
                return (this._editingCollection != 0);
            }
            set
            {
                if (value)
                {
                    this._editingCollection++;
                }
                else
                {
                    this._editingCollection--;
                }
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if (base.InheritanceAttribute == System.ComponentModel.InheritanceAttribute.Inherited)
                {
                    return System.ComponentModel.InheritanceAttribute.InheritedReadOnly;
                }
                return base.InheritanceAttribute;
            }
        }

        private System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                return this.dropDown.RightToLeft;
            }
            set
            {
                if (((this.menuItem != null) && (this.designMenu != null)) && (value != this.RightToLeft))
                {
                    Rectangle empty = Rectangle.Empty;
                    try
                    {
                        empty = this.dropDown.Bounds;
                        this.menuItem.HideDropDown();
                        this.designMenu.RightToLeft = value;
                        this.dropDown.RightToLeft = value;
                    }
                    finally
                    {
                        BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                        if ((service != null) && (empty != Rectangle.Empty))
                        {
                            service.Invalidate(empty);
                        }
                        ToolStripMenuItemDesigner designer = (ToolStripMenuItemDesigner) this.host.GetDesigner(this.menuItem);
                        if (designer != null)
                        {
                            designer.InitializeDropDown();
                        }
                    }
                }
            }
        }

        private string SettingsKey
        {
            get
            {
                if (string.IsNullOrEmpty((string) base.ShadowProperties["SettingsKey"]))
                {
                    IPersistComponentSettings settings = base.Component as IPersistComponentSettings;
                    if ((settings != null) && (this.host != null))
                    {
                        if (settings.SettingsKey == null)
                        {
                            IComponent rootComponent = this.host.RootComponent;
                            if ((rootComponent != null) && (rootComponent != settings))
                            {
                                base.ShadowProperties["SettingsKey"] = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { rootComponent.Site.Name, base.Component.Site.Name });
                            }
                            else
                            {
                                base.ShadowProperties["SettingsKey"] = base.Component.Site.Name;
                            }
                        }
                        settings.SettingsKey = base.ShadowProperties["SettingsKey"] as string;
                        return settings.SettingsKey;
                    }
                }
                return (base.ShadowProperties["SettingsKey"] as string);
            }
            set
            {
                base.ShadowProperties["SettingsKey"] = value;
                IPersistComponentSettings component = base.Component as IPersistComponentSettings;
                if (component != null)
                {
                    component.SettingsKey = value;
                }
            }
        }

        internal class ContextMenuStripBehavior : System.Windows.Forms.Design.Behavior.Behavior
        {
            private ToolStripMenuItem item;

            internal ContextMenuStripBehavior(ToolStripMenuItem menuItem)
            {
                this.item = menuItem;
            }

            public override bool OnMouseUp(Glyph g, MouseButtons button)
            {
                return (button == MouseButtons.Left);
            }
        }
    }
}

