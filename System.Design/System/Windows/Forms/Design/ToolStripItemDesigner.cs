namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripItemDesigner : ComponentDesigner
    {
        private ToolStripTemplateNode _editorNode;
        internal ControlBodyGlyph bodyGlyph;
        private bool currentVisible;
        internal Rectangle dragBoxFromMouseDown = Rectangle.Empty;
        internal bool dummyItemAdded;
        private const int GLYPHBORDER = 1;
        private const int GLYPHINSET = 2;
        internal int indexOfItemUnderMouseToDrag = -1;
        private bool internalCreate;
        private bool isEditorActive;
        private Rectangle lastInsertionMarkRect = Rectangle.Empty;
        private ISelectionService selSvc;
        private ToolStripItemCustomMenuItemCollection toolStripItemCustomMenuItemCollection;

        internal ArrayList AddParentTree()
        {
            ArrayList list = new ArrayList();
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                IComponent rootComponent = service.RootComponent;
                Component toolStripItem = this.ToolStripItem;
                if ((toolStripItem == null) || (rootComponent == null))
                {
                    return list;
                }
                while (toolStripItem != rootComponent)
                {
                    if (toolStripItem is System.Windows.Forms.ToolStripItem)
                    {
                        System.Windows.Forms.ToolStripItem item = toolStripItem as System.Windows.Forms.ToolStripItem;
                        if (item.IsOnDropDown)
                        {
                            if (item.IsOnOverflow)
                            {
                                list.Add(item.Owner);
                                toolStripItem = item.Owner;
                            }
                            else
                            {
                                ToolStripDropDown owner = item.Owner as ToolStripDropDown;
                                if (owner != null)
                                {
                                    System.Windows.Forms.ToolStripItem ownerItem = owner.OwnerItem;
                                    if (ownerItem != null)
                                    {
                                        list.Add(ownerItem);
                                        toolStripItem = ownerItem;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (item.Owner.Site != null)
                            {
                                list.Add(item.Owner);
                            }
                            toolStripItem = item.Owner;
                        }
                    }
                    else if (toolStripItem is Control)
                    {
                        Control control = toolStripItem as Control;
                        Control parent = control.Parent;
                        if (parent.Site != null)
                        {
                            list.Add(parent);
                        }
                        toolStripItem = parent;
                    }
                }
            }
            return list;
        }

        internal override bool CanBeAssociatedWith(IDesigner parentDesigner)
        {
            return (parentDesigner is ToolStripDesigner);
        }

        internal virtual void CommitEdit(System.Type type, string text, bool commit, bool enterKeyPressed, bool tabKeyPressed)
        {
            System.Windows.Forms.ToolStripItem component = null;
            SelectionManager manager = (SelectionManager) this.GetService(typeof(SelectionManager));
            BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
            ToolStrip immediateParent = this.ImmediateParent as ToolStrip;
            immediateParent.SuspendLayout();
            this.HideDummyNode();
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            ToolStripDesigner designer = (ToolStripDesigner) host.GetDesigner(this.ToolStripItem.Owner);
            if ((designer != null) && (designer.EditManager != null))
            {
                designer.EditManager.ActivateEditor(null, false);
            }
            if ((immediateParent is MenuStrip) && (type == typeof(ToolStripSeparator)))
            {
                IDesignerHost host2 = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (host2 != null)
                {
                    IUIService service2 = (IUIService) host2.GetService(typeof(IUIService));
                    if (service2 != null)
                    {
                        service2.ShowError(System.Design.SR.GetString("ToolStripSeparatorError"));
                        commit = false;
                        if (this.selSvc != null)
                        {
                            this.selSvc.SetSelectedComponents(new object[] { immediateParent });
                        }
                    }
                }
            }
            if (!commit)
            {
                if (this.dummyItemAdded)
                {
                    this.dummyItemAdded = false;
                    this.RemoveItem();
                    if (designer.NewItemTransaction != null)
                    {
                        designer.NewItemTransaction.Cancel();
                        designer.NewItemTransaction = null;
                    }
                }
                goto Label_0246;
            }
            if (this.dummyItemAdded)
            {
                try
                {
                    this.RemoveItem();
                    component = designer.AddNewItem(type, text, enterKeyPressed, false);
                    goto Label_020B;
                }
                finally
                {
                    if (designer.NewItemTransaction != null)
                    {
                        designer.NewItemTransaction.Commit();
                        designer.NewItemTransaction = null;
                    }
                }
            }
            DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("ToolStripItemPropertyChangeTransaction"));
            try
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.ToolStripItem)["Text"];
                string str = (string) descriptor.GetValue(this.ToolStripItem);
                if ((descriptor != null) && (text != str))
                {
                    descriptor.SetValue(this.ToolStripItem, text);
                }
                if (enterKeyPressed && (this.selSvc != null))
                {
                    this.SelectNextItem(this.selSvc, enterKeyPressed, designer);
                }
            }
            catch (Exception exception)
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                    transaction = null;
                }
                if (manager != null)
                {
                    manager.Refresh();
                }
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
            }
        Label_020B:
            this.dummyItemAdded = false;
        Label_0246:
            immediateParent.ResumeLayout();
            if ((component != null) && !component.IsOnDropDown)
            {
                ToolStripDropDownItem item2 = component as ToolStripDropDownItem;
                if (item2 != null)
                {
                    Rectangle glyphBounds = ((ToolStripItemDesigner) host.GetDesigner(component)).GetGlyphBounds();
                    Control rootComponent = host.RootComponent as Control;
                    if ((rootComponent != null) && (service != null))
                    {
                        Rectangle parentBounds = service.ControlRectInAdornerWindow(rootComponent);
                        if (!ToolStripDesigner.IsGlyphTotallyVisible(glyphBounds, parentBounds))
                        {
                            item2.HideDropDown();
                        }
                    }
                }
            }
            if (manager != null)
            {
                manager.Refresh();
            }
        }

        private void CreateDummyNode()
        {
            this._editorNode = new ToolStripTemplateNode(this.ToolStripItem, this.ToolStripItem.Text, this.ToolStripItem.Image);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._editorNode != null)
                {
                    this._editorNode.CloseEditor();
                    this._editorNode = null;
                }
                if (this.ToolStripItem != null)
                {
                    this.ToolStripItem.Paint -= new PaintEventHandler(this.OnItemPaint);
                }
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                }
                if (this.selSvc != null)
                {
                    this.selSvc.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                if (this.bodyGlyph != null)
                {
                    ToolStripAdornerWindowService service2 = (ToolStripAdornerWindowService) this.GetService(typeof(ToolStripAdornerWindowService));
                    if ((service2 != null) && service2.DropDownAdorner.Glyphs.Contains(this.bodyGlyph))
                    {
                        service2.DropDownAdorner.Glyphs.Remove(this.bodyGlyph);
                    }
                }
                if ((this.toolStripItemCustomMenuItemCollection != null) && (this.toolStripItemCustomMenuItemCollection.Count > 0))
                {
                    foreach (System.Windows.Forms.ToolStripItem item in this.toolStripItemCustomMenuItemCollection)
                    {
                        item.Dispose();
                    }
                    this.toolStripItemCustomMenuItemCollection.Clear();
                }
                this.toolStripItemCustomMenuItemCollection = null;
            }
            base.Dispose(disposing);
        }

        private void FireComponentChanged(ToolStripDropDownItem parent)
        {
            if (parent != null)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if ((service != null) && (parent.Site != null))
                {
                    service.OnComponentChanged(parent, TypeDescriptor.GetProperties(parent)["DropDownItems"], null, null);
                }
                foreach (System.Windows.Forms.ToolStripItem item in parent.DropDownItems)
                {
                    ToolStripDropDownItem item2 = item as ToolStripDropDownItem;
                    if ((item2 != null) && (item2.DropDownItems.Count > 1))
                    {
                        this.FireComponentChanged(item2);
                    }
                }
            }
        }

        private void FireComponentChanging(ToolStripDropDownItem parent)
        {
            if (parent != null)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if ((service != null) && (parent.Site != null))
                {
                    service.OnComponentChanging(parent, TypeDescriptor.GetProperties(parent)["DropDownItems"]);
                }
                foreach (System.Windows.Forms.ToolStripItem item in parent.DropDownItems)
                {
                    ToolStripDropDownItem item2 = item as ToolStripDropDownItem;
                    if ((item2 != null) && (item2.DropDownItems.Count > 1))
                    {
                        this.FireComponentChanging(item2);
                    }
                }
            }
        }

        internal ToolStripDropDown GetFirstDropDown(System.Windows.Forms.ToolStripItem currentItem)
        {
            if (!(currentItem.Owner is ToolStripDropDown))
            {
                return null;
            }
            ToolStripDropDown owner = currentItem.Owner as ToolStripDropDown;
            while ((owner.OwnerItem != null) && (owner.OwnerItem.Owner is ToolStripDropDown))
            {
                owner = owner.OwnerItem.Owner as ToolStripDropDown;
            }
            return owner;
        }

        public Rectangle GetGlyphBounds()
        {
            BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
            Rectangle empty = Rectangle.Empty;
            if ((service != null) && (this.ImmediateParent != null))
            {
                Point pos = service.ControlToAdornerWindow((Control) this.ImmediateParent);
                empty = this.ToolStripItem.Bounds;
                empty.Offset(pos);
            }
            return empty;
        }

        public void GetGlyphs(ref GlyphCollection glyphs, System.Windows.Forms.Design.Behavior.Behavior standardBehavior)
        {
            if (this.ImmediateParent != null)
            {
                Rectangle glyphBounds = this.GetGlyphBounds();
                ToolStripDesignerUtils.GetAdjustedBounds(this.ToolStripItem, ref glyphBounds);
                BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                if (service.ControlRectInAdornerWindow((Control) this.ImmediateParent).Contains(glyphBounds.Left, glyphBounds.Top))
                {
                    if (this.ToolStripItem.IsOnDropDown)
                    {
                        ToolStrip currentParent = this.ToolStripItem.GetCurrentParent();
                        if (currentParent == null)
                        {
                            currentParent = this.ToolStripItem.Owner;
                        }
                        if ((currentParent != null) && currentParent.Visible)
                        {
                            glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Top, standardBehavior, true));
                            glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Bottom, standardBehavior, true));
                            glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Left, standardBehavior, true));
                            glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Right, standardBehavior, true));
                        }
                    }
                    else
                    {
                        glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Top, standardBehavior, true));
                        glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Bottom, standardBehavior, true));
                        glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Left, standardBehavior, true));
                        glyphs.Add(new MiniLockedBorderGlyph(glyphBounds, SelectionBorderGlyphType.Right, standardBehavior, true));
                    }
                }
            }
        }

        internal virtual ToolStrip GetMainToolStrip()
        {
            return this.ToolStripItem.Owner;
        }

        protected virtual Component GetOwnerForActionList()
        {
            if (this.ToolStripItem.Placement != ToolStripItemPlacement.Main)
            {
                return this.ToolStripItem.Owner;
            }
            return this.ToolStripItem.GetCurrentParent();
        }

        private void HideDummyNode()
        {
            this.ToolStripItem.AutoSize = this.AutoSize;
            if (this._editorNode != null)
            {
                this._editorNode.CloseEditor();
                this._editorNode = null;
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            this.AutoSize = this.ToolStripItem.AutoSize;
            this.Visible = true;
            this.currentVisible = this.Visible;
            this.AccessibleName = this.ToolStripItem.AccessibleName;
            this.ToolStripItem.Paint += new PaintEventHandler(this.OnItemPaint);
            this.ToolStripItem.AccessibleName = this.ToolStripItem.Name;
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
            }
            this.selSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (this.selSvc != null)
            {
                this.selSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            if (!this.internalCreate)
            {
                ISite site = base.Component.Site;
                if ((site != null) && (base.Component is ToolStripDropDownItem))
                {
                    if (defaultValues == null)
                    {
                        defaultValues = new Hashtable();
                    }
                    defaultValues["Text"] = site.Name;
                    IComponent component = base.Component;
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.ToolStripItem)["Text"];
                    if ((descriptor != null) && descriptor.PropertyType.Equals(typeof(string)))
                    {
                        string str = (string) descriptor.GetValue(component);
                        if ((str == null) || (str.Length == 0))
                        {
                            descriptor.SetValue(component, site.Name);
                        }
                    }
                }
            }
            base.InitializeNewComponent(defaultValues);
            if ((base.Component is ToolStripTextBox) || (base.Component is ToolStripComboBox))
            {
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(base.Component)["Text"];
                if (((descriptor2 != null) && (descriptor2.PropertyType == typeof(string))) && (!descriptor2.IsReadOnly && descriptor2.IsBrowsable))
                {
                    descriptor2.SetValue(base.Component, "");
                }
            }
        }

        internal virtual System.Windows.Forms.ToolStripItem MorphCurrentItem(System.Type t)
        {
            System.Windows.Forms.ToolStripItem component = null;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("ToolStripMorphingItemTransaction"));
                ToolStrip immediateParent = (ToolStrip) this.ImmediateParent;
                if (immediateParent is ToolStripOverflow)
                {
                    immediateParent = this.ToolStripItem.Owner;
                }
                ToolStripMenuItemDesigner designer = null;
                int index = immediateParent.Items.IndexOf(this.ToolStripItem);
                string name = this.ToolStripItem.Name;
                System.Windows.Forms.ToolStripItem ownerItem = null;
                if (this.ToolStripItem.IsOnDropDown)
                {
                    ToolStripDropDown down = this.ImmediateParent as ToolStripDropDown;
                    if (down != null)
                    {
                        ownerItem = down.OwnerItem;
                        if (ownerItem != null)
                        {
                            designer = (ToolStripMenuItemDesigner) host.GetDesigner(ownerItem);
                        }
                    }
                }
                try
                {
                    ToolStripDesigner._autoAddNewItems = false;
                    ComponentSerializationService service = this.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                    SerializationStore store = null;
                    if (service != null)
                    {
                        store = service.CreateStore();
                        service.Serialize(store, base.Component);
                        SerializationStore store2 = null;
                        ToolStripDropDownItem toolStripItem = this.ToolStripItem as ToolStripDropDownItem;
                        if ((toolStripItem != null) && typeof(ToolStripDropDownItem).IsAssignableFrom(t))
                        {
                            toolStripItem.HideDropDown();
                            store2 = service.CreateStore();
                            this.SerializeDropDownItems(toolStripItem, ref store2, service);
                            store2.Close();
                        }
                        store.Close();
                        IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                        if (service2 != null)
                        {
                            if (immediateParent.Site != null)
                            {
                                service2.OnComponentChanging(immediateParent, TypeDescriptor.GetProperties(immediateParent)["Items"]);
                            }
                            else if (ownerItem != null)
                            {
                                service2.OnComponentChanging(ownerItem, TypeDescriptor.GetProperties(ownerItem)["DropDownItems"]);
                                service2.OnComponentChanged(ownerItem, TypeDescriptor.GetProperties(ownerItem)["DropDownItems"], null, null);
                            }
                        }
                        this.FireComponentChanging(toolStripItem);
                        immediateParent.Items.Remove(this.ToolStripItem);
                        host.DestroyComponent(this.ToolStripItem);
                        System.Windows.Forms.ToolStripItem item4 = (System.Windows.Forms.ToolStripItem) host.CreateComponent(t, name);
                        if ((item4 is ToolStripDropDownItem) && (store2 != null))
                        {
                            service.Deserialize(store2);
                        }
                        service.DeserializeTo(store, host.Container, false, true);
                        component = (System.Windows.Forms.ToolStripItem) host.Container.Components[name];
                        if ((component.Image == null) && (component is ToolStripButton))
                        {
                            Image image = null;
                            try
                            {
                                image = new Bitmap(typeof(ToolStripButton), "blank.bmp");
                            }
                            catch (Exception exception)
                            {
                                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                                {
                                    throw;
                                }
                            }
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Image"];
                            if ((descriptor != null) && (image != null))
                            {
                                descriptor.SetValue(component, image);
                            }
                            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)["DisplayStyle"];
                            if (descriptor2 != null)
                            {
                                descriptor2.SetValue(component, ToolStripItemDisplayStyle.Image);
                            }
                            PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(component)["ImageTransparentColor"];
                            if (descriptor3 != null)
                            {
                                descriptor3.SetValue(component, Color.Magenta);
                            }
                        }
                        immediateParent.Items.Insert(index, component);
                        if (service2 != null)
                        {
                            if (immediateParent.Site != null)
                            {
                                service2.OnComponentChanged(immediateParent, TypeDescriptor.GetProperties(immediateParent)["Items"], null, null);
                            }
                            else if (ownerItem != null)
                            {
                                service2.OnComponentChanging(ownerItem, TypeDescriptor.GetProperties(ownerItem)["DropDownItems"]);
                                service2.OnComponentChanged(ownerItem, TypeDescriptor.GetProperties(ownerItem)["DropDownItems"], null, null);
                            }
                        }
                        this.FireComponentChanged(toolStripItem);
                        if (component.IsOnDropDown && (designer != null))
                        {
                            designer.RemoveItemBodyGlyph(component);
                            designer.AddItemBodyGlyph(component);
                        }
                        ToolStripDesigner._autoAddNewItems = true;
                        if (component != null)
                        {
                            if (component is ToolStripSeparator)
                            {
                                immediateParent.PerformLayout();
                            }
                            BehaviorService service3 = (BehaviorService) component.Site.GetService(typeof(BehaviorService));
                            if (service3 != null)
                            {
                                service3.Invalidate();
                            }
                            ISelectionService service4 = (ISelectionService) component.Site.GetService(typeof(ISelectionService));
                            if (service4 != null)
                            {
                                service4.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                            }
                        }
                    }
                    return component;
                }
                catch
                {
                    host.Container.Add(this.ToolStripItem);
                    immediateParent.Items.Insert(index, this.ToolStripItem);
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
                }
            }
            return component;
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if (e.Component == this.ToolStripItem)
            {
                this.ToolStripItem.AccessibleName = e.NewName;
            }
        }

        private void OnItemPaint(object sender, PaintEventArgs e)
        {
            if (((this.ToolStripItem.GetCurrentParent() is ToolStripDropDown) && (this.selSvc != null)) && (!this.IsEditorActive && this.ToolStripItem.Equals(this.selSvc.PrimarySelection)))
            {
                BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                if (service != null)
                {
                    Point pos = service.ControlToAdornerWindow((Control) this.ImmediateParent);
                    Rectangle bounds = this.ToolStripItem.Bounds;
                    bounds.Offset(pos);
                    bounds.Inflate(2, 2);
                    service.ProcessPaintMessage(bounds);
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ISelectionService service = sender as ISelectionService;
            if (service != null)
            {
                System.Windows.Forms.ToolStripItem primarySelection = service.PrimarySelection as System.Windows.Forms.ToolStripItem;
                System.Windows.Forms.ToolStripItem.ToolStripItemAccessibleObject accessibilityObject = this.ToolStripItem.AccessibilityObject as System.Windows.Forms.ToolStripItem.ToolStripItemAccessibleObject;
                if (accessibilityObject != null)
                {
                    accessibilityObject.AddState(AccessibleStates.None);
                    ToolStrip mainToolStrip = this.GetMainToolStrip();
                    if (service.GetComponentSelected(this.ToolStripItem))
                    {
                        ToolStrip immediateParent = this.ImmediateParent as ToolStrip;
                        int index = 0;
                        if (immediateParent != null)
                        {
                            index = immediateParent.Items.IndexOf(primarySelection);
                        }
                        accessibilityObject.AddState(AccessibleStates.Selected);
                        if (mainToolStrip != null)
                        {
                            System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8007, new HandleRef(immediateParent, immediateParent.Handle), -4, index + 1);
                        }
                        if (primarySelection == this.ToolStripItem)
                        {
                            accessibilityObject.AddState(AccessibleStates.Focused);
                            if (mainToolStrip != null)
                            {
                                System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(immediateParent, immediateParent.Handle), -4, index + 1);
                            }
                        }
                    }
                }
                if ((((primarySelection != null) && (this.ToolStripItem != null)) && (primarySelection.IsOnDropDown && this.ToolStripItem.Equals(primarySelection))) && !(this.ToolStripItem is ToolStripMenuItem))
                {
                    IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (host != null)
                    {
                        ToolStripDropDown owner = primarySelection.Owner as ToolStripDropDown;
                        if ((owner != null) && !owner.Visible)
                        {
                            ToolStripDropDownItem ownerItem = owner.OwnerItem as ToolStripDropDownItem;
                            if (ownerItem != null)
                            {
                                ToolStripMenuItemDesigner designer = (ToolStripMenuItemDesigner) host.GetDesigner(ownerItem);
                                if (designer != null)
                                {
                                    designer.InitializeDropDown();
                                }
                                SelectionManager manager = (SelectionManager) this.GetService(typeof(SelectionManager));
                                if (manager != null)
                                {
                                    manager.Refresh();
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "AutoSize", "AccessibleName", "Visible", "Overflow" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ToolStripItemDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        public void RemoveItem()
        {
            this.dummyItemAdded = false;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                ToolStrip immediateParent = (ToolStrip) this.ImmediateParent;
                if (immediateParent is ToolStripOverflow)
                {
                    immediateParent = this.ParentComponent as ToolStrip;
                }
                immediateParent.Items.Remove(this.ToolStripItem);
                service.DestroyComponent(this.ToolStripItem);
            }
        }

        private void ResetAccessibleName()
        {
            base.ShadowProperties["AccessibleName"] = null;
        }

        private void ResetAutoSize()
        {
            base.ShadowProperties["AutoSize"] = false;
        }

        private void ResetOverflow()
        {
            this.ToolStripItem.Overflow = ToolStripItemOverflow.AsNeeded;
        }

        private void ResetVisible()
        {
            this.Visible = true;
        }

        private void RestoreAccessibleName()
        {
            this.ToolStripItem.AccessibleName = (string) base.ShadowProperties["AccessibleName"];
        }

        private void RestoreAutoSize()
        {
            this.ToolStripItem.AutoSize = (bool) base.ShadowProperties["AutoSize"];
        }

        private void RestoreOverflow()
        {
            this.ToolStripItem.Overflow = (ToolStripItemOverflow) base.ShadowProperties["Overflow"];
        }

        internal void SelectNextItem(ISelectionService service, bool enterKeyPressed, ToolStripDesigner designer)
        {
            if (this.ToolStripItem is ToolStripDropDownItem)
            {
                this.SetSelection(enterKeyPressed);
            }
            else
            {
                ToolStrip immediateParent = (ToolStrip) this.ImmediateParent;
                if (immediateParent is ToolStripOverflow)
                {
                    immediateParent = this.ToolStripItem.Owner;
                }
                int index = immediateParent.Items.IndexOf(this.ToolStripItem);
                System.Windows.Forms.ToolStripItem item2 = immediateParent.Items[index + 1];
                ToolStripKeyboardHandlingService service2 = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
                if (service2 != null)
                {
                    if (item2 == designer.EditorNode)
                    {
                        service2.SelectedDesignerControl = item2;
                        this.selSvc.SetSelectedComponents(null, SelectionTypes.Replace);
                    }
                    else
                    {
                        service2.SelectedDesignerControl = null;
                        this.selSvc.SetSelectedComponents(new object[] { item2 });
                    }
                }
            }
        }

        private void SerializeDropDownItems(ToolStripDropDownItem parent, ref SerializationStore _serializedDataForDropDownItems, ComponentSerializationService _serializationService)
        {
            foreach (System.Windows.Forms.ToolStripItem item in parent.DropDownItems)
            {
                if (!(item is DesignerToolStripControlHost))
                {
                    _serializationService.Serialize(_serializedDataForDropDownItems, item);
                    ToolStripDropDownItem item2 = item as ToolStripDropDownItem;
                    if (item2 != null)
                    {
                        this.SerializeDropDownItems(item2, ref _serializedDataForDropDownItems, _serializationService);
                    }
                }
            }
        }

        internal void SetItemVisible(bool toolStripSelected, ToolStripDesigner designer)
        {
            if (toolStripSelected)
            {
                if (!this.currentVisible)
                {
                    this.ToolStripItem.Visible = true;
                    if ((designer != null) && !designer.FireSyncSelection)
                    {
                        designer.FireSyncSelection = true;
                    }
                }
            }
            else if (!this.currentVisible)
            {
                this.ToolStripItem.Visible = this.currentVisible;
            }
        }

        internal virtual bool SetSelection(bool enterKeyPressed)
        {
            return false;
        }

        private bool ShouldSerializeAccessibleName()
        {
            return (base.ShadowProperties["AccessibleName"] != null);
        }

        private bool ShouldSerializeAutoSize()
        {
            return base.ShadowProperties.Contains("AutoSize");
        }

        private bool ShouldSerializeOverflow()
        {
            return (base.ShadowProperties["Overflow"] != null);
        }

        private bool ShouldSerializeVisible()
        {
            return !this.Visible;
        }

        internal override void ShowContextMenu(int x, int y)
        {
            ToolStripKeyboardHandlingService service = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
            if (service != null)
            {
                if (!service.ContextMenuShownByKeyBoard)
                {
                    BehaviorService service2 = (BehaviorService) this.GetService(typeof(BehaviorService));
                    Point empty = Point.Empty;
                    if (service2 != null)
                    {
                        empty = service2.ScreenToAdornerWindow(new Point(x, y));
                    }
                    if (this.GetGlyphBounds().Contains(empty))
                    {
                        this.DesignerContextMenu.Show(x, y);
                    }
                }
                else
                {
                    service.ContextMenuShownByKeyBoard = false;
                    this.DesignerContextMenu.Show(x, y);
                }
            }
        }

        internal virtual void ShowEditNode(bool clicked)
        {
            if (this.ToolStripItem is ToolStripMenuItem)
            {
                if (this._editorNode == null)
                {
                    this.CreateDummyNode();
                }
                IDesignerHost host = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
                ToolStrip immediateParent = this.ImmediateParent as ToolStrip;
                if (immediateParent != null)
                {
                    ToolStripDesigner designer = (ToolStripDesigner) host.GetDesigner(immediateParent);
                    BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                    Point pos = service.ControlToAdornerWindow(immediateParent);
                    Rectangle bounds = this.ToolStripItem.Bounds;
                    bounds.Offset(pos);
                    this.ToolStripItem.AutoSize = false;
                    this._editorNode.SetWidth(this.ToolStripItem.Text);
                    if (immediateParent.Orientation == Orientation.Horizontal)
                    {
                        this.ToolStripItem.Width = this._editorNode.EditorToolStrip.Width + 2;
                    }
                    else
                    {
                        this.ToolStripItem.Height = this._editorNode.EditorToolStrip.Height;
                    }
                    if (!this.dummyItemAdded)
                    {
                        service.SyncSelection();
                    }
                    if (this.ToolStripItem.Placement != ToolStripItemPlacement.None)
                    {
                        Rectangle b = this.ToolStripItem.Bounds;
                        b.Offset(pos);
                        if (immediateParent.Orientation == Orientation.Horizontal)
                        {
                            b.X++;
                            b.Y += (this.ToolStripItem.Height - this._editorNode.EditorToolStrip.Height) / 2;
                            b.Y++;
                        }
                        else
                        {
                            b.X += (this.ToolStripItem.Width - this._editorNode.EditorToolStrip.Width) / 2;
                            b.X++;
                        }
                        this._editorNode.Bounds = b;
                        b = Rectangle.Union(bounds, b);
                        service.Invalidate(b);
                        if ((designer != null) && (designer.EditManager != null))
                        {
                            designer.EditManager.ActivateEditor(this.ToolStripItem, clicked);
                        }
                        SelectionManager manager = (SelectionManager) this.GetService(typeof(SelectionManager));
                        if (this.bodyGlyph != null)
                        {
                            manager.BodyGlyphAdorner.Glyphs.Remove(this.bodyGlyph);
                        }
                    }
                    else
                    {
                        this.ToolStripItem.AutoSize = this.AutoSize;
                        if (this.ToolStripItem is ToolStripDropDownItem)
                        {
                            ToolStripDropDownItem toolStripItem = this.ToolStripItem as ToolStripDropDownItem;
                            if (toolStripItem != null)
                            {
                                toolStripItem.HideDropDown();
                            }
                            this.selSvc.SetSelectedComponents(new object[] { this.ImmediateParent });
                        }
                    }
                }
            }
        }

        private string AccessibleName
        {
            get
            {
                return (string) base.ShadowProperties["AccessibleName"];
            }
            set
            {
                base.ShadowProperties["AccessibleName"] = value;
            }
        }

        internal bool AutoSize
        {
            get
            {
                return (bool) base.ShadowProperties["AutoSize"];
            }
            set
            {
                bool flag = (bool) base.ShadowProperties["AutoSize"];
                base.ShadowProperties["AutoSize"] = value;
                if (value != flag)
                {
                    this.ToolStripItem.AutoSize = value;
                }
            }
        }

        private ContextMenuStrip DesignerContextMenu
        {
            get
            {
                BaseContextMenuStrip strip = new BaseContextMenuStrip(base.Component.Site, this.ToolStripItem);
                if (this.selSvc.SelectionCount > 1)
                {
                    strip.GroupOrdering.Clear();
                    strip.GroupOrdering.AddRange(new string[] { "Code", "Selection", "Edit", "Properties" });
                }
                else
                {
                    strip.GroupOrdering.Clear();
                    strip.GroupOrdering.AddRange(new string[] { "Code", "Custom", "Selection", "Edit", "Properties" });
                    strip.Text = "CustomContextMenu";
                    if (this.toolStripItemCustomMenuItemCollection == null)
                    {
                        this.toolStripItemCustomMenuItemCollection = new ToolStripItemCustomMenuItemCollection(base.Component.Site, this.ToolStripItem);
                    }
                    foreach (System.Windows.Forms.ToolStripItem item in this.toolStripItemCustomMenuItemCollection)
                    {
                        strip.Groups["Custom"].Items.Add(item);
                    }
                }
                if (this.toolStripItemCustomMenuItemCollection != null)
                {
                    this.toolStripItemCustomMenuItemCollection.RefreshItems();
                }
                strip.Populated = false;
                return strip;
            }
        }

        internal virtual ToolStripTemplateNode Editor
        {
            get
            {
                return this._editorNode;
            }
            set
            {
                this._editorNode = value;
            }
        }

        protected IComponent ImmediateParent
        {
            get
            {
                if (this.ToolStripItem == null)
                {
                    return null;
                }
                ToolStrip currentParent = this.ToolStripItem.GetCurrentParent();
                if (currentParent == null)
                {
                    return this.ToolStripItem.Owner;
                }
                return currentParent;
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

        internal bool InternalCreate
        {
            get
            {
                return this.internalCreate;
            }
            set
            {
                this.internalCreate = value;
            }
        }

        internal bool IsEditorActive
        {
            get
            {
                return this.isEditorActive;
            }
            set
            {
                this.isEditorActive = value;
            }
        }

        private ToolStripItemOverflow Overflow
        {
            get
            {
                return (ToolStripItemOverflow) base.ShadowProperties["Overflow"];
            }
            set
            {
                if (this.ToolStripItem.IsOnOverflow)
                {
                    ToolStrip owner = this.ToolStripItem.Owner;
                    if (owner.OverflowButton.DropDown.Visible)
                    {
                        owner.OverflowButton.HideDropDown();
                    }
                }
                if (this.ToolStripItem is ToolStripDropDownItem)
                {
                    (this.ToolStripItem as ToolStripDropDownItem).HideDropDown();
                }
                if (value != this.ToolStripItem.Overflow)
                {
                    this.ToolStripItem.Overflow = value;
                    base.ShadowProperties["Overflow"] = value;
                }
                BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                if (service != null)
                {
                    service.SyncSelection();
                }
            }
        }

        protected override IComponent ParentComponent
        {
            get
            {
                if (this.ToolStripItem == null)
                {
                    return null;
                }
                if (this.ToolStripItem.IsOnDropDown && !this.ToolStripItem.IsOnOverflow)
                {
                    ToolStripDropDown immediateParent = this.ImmediateParent as ToolStripDropDown;
                    if (immediateParent != null)
                    {
                        if (immediateParent.IsAutoGenerated)
                        {
                            return immediateParent.OwnerItem;
                        }
                        return immediateParent;
                    }
                }
                return this.GetMainToolStrip();
            }
        }

        public System.Windows.Forms.ToolStripItem ToolStripItem
        {
            get
            {
                return (System.Windows.Forms.ToolStripItem) base.Component;
            }
        }

        protected bool Visible
        {
            get
            {
                return (bool) base.ShadowProperties["Visible"];
            }
            set
            {
                base.ShadowProperties["Visible"] = value;
                this.currentVisible = value;
            }
        }
    }
}

