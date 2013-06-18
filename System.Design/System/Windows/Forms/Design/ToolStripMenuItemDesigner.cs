namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripMenuItemDesigner : ToolStripDropDownItemDesigner
    {
        private DesignerTransaction _pendingTransaction;
        private bool addingDummyItem;
        private Rectangle boundsToInvalidateOnRemove = Rectangle.Empty;
        private DesignerToolStripControlHost commitedEditorNode;
        private ToolStripTemplateNode commitedTemplateNode;
        private bool componentAddingFired;
        private ToolStripDropDown customDropDown;
        private IDesignerHost designerHost;
        private bool dropDownSet;
        private bool dropDownSetFailed;
        private Rectangle dropDownSizeToInvalidate = Rectangle.Empty;
        private bool fireComponentChanged;
        private const int GLYPHINSET = 2;
        private int indexToInsertNewItem = -1;
        private bool initialized;
        private DesignerTransaction insertMenuItemTransaction;
        private ToolStripKeyboardHandlingService keyboardHandlingService;
        private DesignerTransaction newMenuItemTransaction;
        private ToolStripItem parentItem;
        private ToolStripDropDownGlyph rootControlGlyph;
        private ISelectionService selSvc;
        private SerializationStore serializedDataForDropDownItems;
        private ToolStripAdornerWindowService toolStripAdornerWindowService;
        private DesignerToolStripControlHost typeHereNode;
        private ToolStripTemplateNode typeHereTemplateNode;
        private UndoEngine undoEngine;
        private bool undoingCalled;

        private void AddBodyGlyphs(ToolStripDropDownItem item)
        {
            if ((item != null) && (((ToolStripMenuItemDesigner) this.designerHost.GetDesigner(item)) != null))
            {
                foreach (ToolStripItem item2 in item.DropDownItems)
                {
                    this.AddItemBodyGlyph(item2);
                }
            }
        }

        internal void AddItemBodyGlyph(ToolStripItem item)
        {
            if (item != null)
            {
                ToolStripItemDesigner itemDesigner = (ToolStripItemDesigner) this.designerHost.GetDesigner(item);
                if (itemDesigner != null)
                {
                    Rectangle glyphBounds = itemDesigner.GetGlyphBounds();
                    System.Windows.Forms.Design.Behavior.Behavior b = new ToolStripItemBehavior();
                    ToolStripItemGlyph glyph = new ToolStripItemGlyph(item, itemDesigner, glyphBounds, b);
                    itemDesigner.bodyGlyph = glyph;
                    if (this.toolStripAdornerWindowService != null)
                    {
                        this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Insert(0, glyph);
                    }
                }
            }
        }

        internal void AddNewTemplateNode(ToolStripDropDown dropDown)
        {
            foreach (ToolStripItem item in dropDown.Items)
            {
                if (item is DesignerToolStripControlHost)
                {
                    this.typeHereNode = (DesignerToolStripControlHost) item;
                }
            }
            if (this.typeHereNode != null)
            {
                dropDown.Items.Remove(this.typeHereNode);
            }
            this.typeHereTemplateNode = new ToolStripTemplateNode(base.Component, System.Design.SR.GetString("ToolStripDesignerTemplateNodeEnterText"), null);
            int width = this.typeHereTemplateNode.EditorToolStrip.Width;
            this.typeHereNode = new DesignerToolStripControlHost(this.typeHereTemplateNode.EditorToolStrip);
            this.typeHereTemplateNode.ControlHost = this.typeHereNode;
            this.typeHereNode.AutoSize = false;
            this.typeHereNode.Width = width;
            dropDown.Items.Add(this.typeHereNode);
        }

        private bool CheckSameOwner(ToolStripDropDownItem lastSelected, ToolStripDropDownItem currentSelected)
        {
            if (((lastSelected != null) && (currentSelected != null)) && ((lastSelected.Owner is ToolStripDropDown) && (currentSelected.Owner is ToolStripDropDown)))
            {
                ToolStripItem ownerItem = ((ToolStripDropDown) lastSelected.Owner).OwnerItem;
                ToolStripItem item2 = ((ToolStripDropDown) currentSelected.Owner).OwnerItem;
                return (ownerItem == item2);
            }
            return false;
        }

        internal void Commit()
        {
            if ((this.commitedTemplateNode != null) && this.commitedTemplateNode.Active)
            {
                int index = this.MenuItem.DropDownItems.IndexOf(this.commitedEditorNode);
                this.commitedTemplateNode.Commit(false, false);
                if ((index != -1) && (this.MenuItem.DropDownItems.Count > index))
                {
                    ToolStripDropDownItem item = this.MenuItem.DropDownItems[index] as ToolStripDropDownItem;
                    if (item != null)
                    {
                        item.HideDropDown();
                    }
                }
            }
            else if ((this.typeHereTemplateNode != null) && this.typeHereTemplateNode.Active)
            {
                this.typeHereTemplateNode.Commit(false, false);
            }
            ToolStripDropDownItem menuItem = this.MenuItem;
            while ((menuItem != null) && (menuItem.Owner is ToolStripDropDown))
            {
                menuItem = (ToolStripDropDownItem) ((ToolStripDropDown) menuItem.Owner).OwnerItem;
                if (menuItem != null)
                {
                    ToolStripMenuItemDesigner designer = (ToolStripMenuItemDesigner) this.designerHost.GetDesigner(menuItem);
                    if (designer != null)
                    {
                        designer.Commit();
                    }
                }
            }
        }

        internal override void CommitEdit(System.Type type, string text, bool commit, bool enterKeyPressed, bool tabKeyPressed)
        {
            base.IsEditorActive = false;
            if (!(this.MenuItem.Owner is ToolStripDropDown) && (base.Editor != null))
            {
                base.CommitEdit(type, text, commit, enterKeyPressed, tabKeyPressed);
                return;
            }
            if (!commit)
            {
                if (this.commitedEditorNode != null)
                {
                    this.MenuItem.DropDown.SuspendLayout();
                    bool flag2 = base.dummyItemAdded;
                    base.dummyItemAdded = false;
                    int index = this.MenuItem.DropDownItems.IndexOf(this.commitedEditorNode);
                    ToolStripItem item4 = this.MenuItem.DropDownItems[index + 1];
                    this.MenuItem.DropDown.Items.Remove(this.commitedEditorNode);
                    item4.Visible = true;
                    if (this.commitedTemplateNode != null)
                    {
                        this.commitedTemplateNode.CloseEditor();
                        this.commitedTemplateNode = null;
                    }
                    if (this.commitedEditorNode != null)
                    {
                        this.commitedEditorNode.Dispose();
                        this.commitedEditorNode = null;
                    }
                    if (flag2)
                    {
                        this.MenuItem.DropDownItems.Remove(item4);
                        try
                        {
                            this.designerHost.DestroyComponent(item4);
                        }
                        catch
                        {
                            if (this.newMenuItemTransaction != null)
                            {
                                try
                                {
                                    this.newMenuItemTransaction.Cancel();
                                }
                                catch
                                {
                                }
                                this.newMenuItemTransaction = null;
                            }
                        }
                        item4 = null;
                    }
                    this.MenuItem.DropDown.ResumeLayout();
                    if (item4 != null)
                    {
                        this.AddItemBodyGlyph(item4);
                    }
                    if (flag2)
                    {
                        SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                        service.NeedRefresh = false;
                        if (this.newMenuItemTransaction != null)
                        {
                            try
                            {
                                base.dummyItemAdded = true;
                                this.newMenuItemTransaction.Cancel();
                                this.newMenuItemTransaction = null;
                                if (this.MenuItem.DropDownItems.Count == 0)
                                {
                                    this.CreatetypeHereNode();
                                }
                            }
                            finally
                            {
                                base.dummyItemAdded = false;
                            }
                        }
                        flag2 = false;
                    }
                    this.MenuItem.DropDown.PerformLayout();
                }
                return;
            }
            int dummyIndex = -1;
            bool dummyItemAdded = base.dummyItemAdded;
            base.dummyItemAdded = false;
            this.MenuItem.DropDown.SuspendLayout();
            if (this.commitedEditorNode != null)
            {
                dummyIndex = this.MenuItem.DropDownItems.IndexOf(this.commitedEditorNode);
                ToolStripItem component = this.MenuItem.DropDownItems[dummyIndex + 1];
                this.MenuItem.DropDown.Items.Remove(this.commitedEditorNode);
                if (this.commitedTemplateNode != null)
                {
                    this.commitedTemplateNode.CloseEditor();
                    this.commitedTemplateNode = null;
                }
                if (this.commitedEditorNode != null)
                {
                    this.commitedEditorNode.Dispose();
                    this.commitedEditorNode = null;
                }
                if (text == "-")
                {
                    ToolStripItemDesigner designer = this.designerHost.GetDesigner(component) as ToolStripItemDesigner;
                    if (designer == null)
                    {
                        goto Label_028C;
                    }
                    try
                    {
                        try
                        {
                            component = designer.MorphCurrentItem(typeof(ToolStripSeparator));
                            this.RemoveItemBodyGlyph(component);
                        }
                        catch
                        {
                            if (this.newMenuItemTransaction != null)
                            {
                                try
                                {
                                    this.newMenuItemTransaction.Cancel();
                                }
                                catch
                                {
                                }
                                this.newMenuItemTransaction = null;
                            }
                        }
                        goto Label_028C;
                    }
                    finally
                    {
                        if (this.newMenuItemTransaction != null)
                        {
                            this.newMenuItemTransaction.Commit();
                            this.newMenuItemTransaction = null;
                        }
                    }
                }
                if (dummyItemAdded)
                {
                    try
                    {
                        try
                        {
                            base.dummyItemAdded = true;
                            this.CreateNewItem(type, dummyIndex, text);
                            this.designerHost.DestroyComponent(component);
                            if (enterKeyPressed)
                            {
                                this.typeHereNode.SelectControl();
                            }
                        }
                        catch
                        {
                            if (this.newMenuItemTransaction != null)
                            {
                                try
                                {
                                    this.newMenuItemTransaction.Cancel();
                                }
                                catch
                                {
                                }
                                this.newMenuItemTransaction = null;
                            }
                        }
                        goto Label_028C;
                    }
                    finally
                    {
                        if (this.newMenuItemTransaction != null)
                        {
                            this.newMenuItemTransaction.Commit();
                            this.newMenuItemTransaction = null;
                        }
                        base.dummyItemAdded = false;
                    }
                }
                component.Visible = true;
                DesignerTransaction transaction = this.designerHost.CreateTransaction(System.Design.SR.GetString("ToolStripItemPropertyChangeTransaction"));
                try
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Text"];
                    string str = (string) descriptor.GetValue(component);
                    if ((descriptor != null) && (text != str))
                    {
                        descriptor.SetValue(component, text);
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
                }
            }
            else
            {
                dummyIndex = this.MenuItem.DropDownItems.IndexOf(this.typeHereNode);
                try
                {
                    base.dummyItemAdded = true;
                    this.CreateNewItem(type, dummyIndex, text);
                }
                finally
                {
                    base.dummyItemAdded = false;
                }
                this.typeHereNode.SelectControl();
            }
        Label_028C:
            this.MenuItem.DropDown.ResumeLayout(true);
            this.MenuItem.DropDown.PerformLayout();
            this.ResetGlyphs(this.MenuItem);
            if (this.selSvc != null)
            {
                if (enterKeyPressed)
                {
                    ToolStripItem item2 = null;
                    if (((this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveLeft) || (this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveRight)) && (dummyIndex >= 1))
                    {
                        item2 = this.MenuItem.DropDownItems[dummyIndex - 1];
                    }
                    else
                    {
                        item2 = this.MenuItem.DropDownItems[dummyIndex + 1];
                    }
                    if (this.KeyboardHandlingService != null)
                    {
                        if (item2 != null)
                        {
                            ToolStripDropDownItem item3 = this.MenuItem.DropDownItems[dummyIndex] as ToolStripDropDownItem;
                            if (item3 != null)
                            {
                                item3.HideDropDown();
                            }
                        }
                        if (item2 == this.typeHereNode)
                        {
                            this.KeyboardHandlingService.SelectedDesignerControl = item2;
                            this.selSvc.SetSelectedComponents(null, SelectionTypes.Replace);
                        }
                        else
                        {
                            this.KeyboardHandlingService.SelectedDesignerControl = null;
                            this.selSvc.SetSelectedComponents(new object[] { item2 });
                        }
                    }
                }
                else if (tabKeyPressed)
                {
                    this.selSvc.SetSelectedComponents(new object[] { this.MenuItem.DropDownItems[dummyIndex] }, SelectionTypes.Replace);
                }
            }
        }

        private void CommitInsertTransaction(bool commit)
        {
            if (!this.IsOnContextMenu)
            {
                ToolStrip mainToolStrip = this.GetMainToolStrip();
                ToolStripDesigner designer = this.designerHost.GetDesigner(mainToolStrip) as ToolStripDesigner;
                if ((designer != null) && (designer.InsertTansaction != null))
                {
                    if (commit)
                    {
                        designer.InsertTansaction.Commit();
                    }
                    else
                    {
                        designer.InsertTansaction.Cancel();
                    }
                    designer.InsertTansaction = null;
                }
            }
            else if (this.insertMenuItemTransaction != null)
            {
                if (commit)
                {
                    this.insertMenuItemTransaction.Commit();
                }
                else
                {
                    this.insertMenuItemTransaction.Cancel();
                }
                this.insertMenuItemTransaction = null;
            }
        }

        private void ComponentChangeSvc_ComponentAdded(object sender, ComponentEventArgs e)
        {
            ToolStripItem component = e.Component as ToolStripItem;
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (((component != null) && this.componentAddingFired) && (this.MenuItemSelected || this.fireComponentChanged))
            {
                this.componentAddingFired = false;
                try
                {
                    if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                    {
                        if (service != null)
                        {
                            MemberDescriptor member = TypeDescriptor.GetProperties(this.MenuItem.DropDown)["Items"];
                            service.OnComponentChanging(this.MenuItem.DropDown, member);
                        }
                    }
                    else
                    {
                        base.RaiseComponentChanging(TypeDescriptor.GetProperties(this.MenuItem)["DropDownItems"]);
                    }
                    int count = this.MenuItem.DropDownItems.Count;
                    if (this.indexToInsertNewItem != -1)
                    {
                        if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                        {
                            this.MenuItem.DropDown.Items.Insert(this.indexToInsertNewItem, component);
                        }
                        else
                        {
                            this.MenuItem.DropDownItems.Insert(this.indexToInsertNewItem, component);
                        }
                    }
                    else
                    {
                        ToolStripItem primarySelection = this.selSvc.PrimarySelection as ToolStripItem;
                        if ((primarySelection != null) && (primarySelection != this.MenuItem))
                        {
                            int index = this.MenuItem.DropDownItems.IndexOf(primarySelection);
                            if ((this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveLeft) || (this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveRight))
                            {
                                if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                                {
                                    this.MenuItem.DropDown.Items.Insert(index + 1, component);
                                }
                                else
                                {
                                    this.MenuItem.DropDownItems.Insert(index + 1, component);
                                }
                            }
                            else if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                            {
                                this.MenuItem.DropDown.Items.Insert(index, component);
                            }
                            else
                            {
                                this.MenuItem.DropDownItems.Insert(index, component);
                            }
                        }
                        else if (count > 0)
                        {
                            if ((this.MenuItem.DropDownDirection != ToolStripDropDownDirection.AboveLeft) && (this.MenuItem.DropDownDirection != ToolStripDropDownDirection.AboveRight))
                            {
                                if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                                {
                                    this.MenuItem.DropDown.Items.Insert(count - 1, component);
                                }
                                else
                                {
                                    this.MenuItem.DropDownItems.Insert(count - 1, component);
                                }
                            }
                        }
                        else if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                        {
                            this.MenuItem.DropDown.Items.Add(component);
                        }
                        else
                        {
                            this.MenuItem.DropDownItems.Add(component);
                        }
                    }
                    if (this.undoingCalled && (this.selSvc != null))
                    {
                        this.selSvc.SetSelectedComponents(new IComponent[] { component }, SelectionTypes.Replace);
                    }
                    this.ResetGlyphs(this.MenuItem);
                }
                catch
                {
                    this.CommitInsertTransaction(false);
                }
                finally
                {
                    if (this.IsOnContextMenu && (this.MenuItem.DropDown.Site != null))
                    {
                        if (service != null)
                        {
                            MemberDescriptor descriptor2 = TypeDescriptor.GetProperties(this.MenuItem.DropDown)["Items"];
                            service.OnComponentChanged(this.MenuItem.DropDown, descriptor2, null, null);
                        }
                    }
                    else
                    {
                        base.RaiseComponentChanged(TypeDescriptor.GetProperties(this.MenuItem)["DropDownItems"], null, null);
                    }
                    this.CommitInsertTransaction(true);
                }
            }
        }

        private void ComponentChangeSvc_ComponentAdding(object sender, ComponentEventArgs e)
        {
            if (((this.KeyboardHandlingService == null) || !this.KeyboardHandlingService.CopyInProgress) && ((e.Component is ToolStripItem) && (this.MenuItemSelected || this.fireComponentChanged)))
            {
                if (!this.IsOnContextMenu)
                {
                    ToolStrip mainToolStrip = this.GetMainToolStrip();
                    ToolStripDesigner designer = this.designerHost.GetDesigner(mainToolStrip) as ToolStripDesigner;
                    if (((designer != null) && !designer.EditingCollection) && (designer.InsertTansaction == null))
                    {
                        this.componentAddingFired = true;
                        designer.InsertTansaction = this.designerHost.CreateTransaction(System.Design.SR.GetString("ToolStripInsertingIntoDropDownTransaction"));
                    }
                }
                else
                {
                    ToolStripItem component = e.Component as ToolStripItem;
                    if ((component != null) && (component.Owner == null))
                    {
                        this.componentAddingFired = true;
                        this.insertMenuItemTransaction = this.designerHost.CreateTransaction(System.Design.SR.GetString("ToolStripInsertingIntoDropDownTransaction"));
                    }
                }
            }
        }

        private void ComponentChangeSvc_ComponentRemoved(object sender, ComponentEventArgs e)
        {
            ToolStripItem item = e.Component as ToolStripItem;
            if ((item != null) && item.IsOnDropDown)
            {
                ToolStripDropDownItem ownerItem = (ToolStripDropDownItem) ((ToolStripDropDown) item.Owner).OwnerItem;
                if ((ownerItem != null) && (ownerItem == this.MenuItem))
                {
                    int index = ownerItem.DropDownItems.IndexOf(item);
                    try
                    {
                        if (index != -1)
                        {
                            ownerItem.DropDownItems.Remove(item);
                            base.RaiseComponentChanged(TypeDescriptor.GetProperties(ownerItem)["DropDownItems"], null, null);
                        }
                    }
                    finally
                    {
                        if (this._pendingTransaction != null)
                        {
                            this._pendingTransaction.Commit();
                            this._pendingTransaction = null;
                        }
                    }
                    this.ResetGlyphs(ownerItem);
                    if (ownerItem.DropDownItems.Count > 1)
                    {
                        index = Math.Min(ownerItem.DropDownItems.Count - 1, index);
                        index = Math.Max(0, index);
                    }
                    else
                    {
                        index = -1;
                    }
                    if ((this.toolStripAdornerWindowService != null) && (this.boundsToInvalidateOnRemove != Rectangle.Empty))
                    {
                        using (Region region = new Region(this.boundsToInvalidateOnRemove))
                        {
                            region.Exclude(this.MenuItem.DropDown.Bounds);
                            this.toolStripAdornerWindowService.Invalidate(region);
                            this.boundsToInvalidateOnRemove = Rectangle.Empty;
                        }
                    }
                    if (((this.KeyboardHandlingService != null) && this.KeyboardHandlingService.CutOrDeleteInProgress) && ((this.selSvc != null) && !base.dummyItemAdded))
                    {
                        IComponent component = (index == -1) ? ownerItem : ownerItem.DropDownItems[index];
                        if (component is DesignerToolStripControlHost)
                        {
                            this.KeyboardHandlingService.SelectedDesignerControl = component;
                            this.KeyboardHandlingService.OwnerItemAfterCut = this.MenuItem;
                            this.selSvc.SetSelectedComponents(null, SelectionTypes.Replace);
                        }
                        else
                        {
                            this.selSvc.SetSelectedComponents(new IComponent[] { component }, SelectionTypes.Replace);
                        }
                    }
                }
            }
        }

        private void ComponentChangeSvc_ComponentRemoving(object sender, ComponentEventArgs e)
        {
            if (!base.dummyItemAdded)
            {
                ToolStripItem component = e.Component as ToolStripItem;
                if (((component != null) && component.IsOnDropDown) && (component.Placement == ToolStripItemPlacement.Main))
                {
                    ToolStripDropDownItem ownerItem = (ToolStripDropDownItem) ((ToolStripDropDown) component.Owner).OwnerItem;
                    if ((ownerItem != null) && (ownerItem == this.MenuItem))
                    {
                        this.RemoveItemBodyGlyph(component);
                        this.InitializeBodyGlyphsForItems(false, ownerItem);
                        this.boundsToInvalidateOnRemove = ownerItem.DropDown.Bounds;
                        ToolStripDropDownItem item3 = component as ToolStripDropDownItem;
                        if (item3 != null)
                        {
                            this.boundsToInvalidateOnRemove = Rectangle.Union(this.boundsToInvalidateOnRemove, item3.DropDown.Bounds);
                        }
                        try
                        {
                            this._pendingTransaction = this.designerHost.CreateTransaction(System.Design.SR.GetString("ToolStripDesignerTransactionRemovingItem"));
                            base.RaiseComponentChanging(TypeDescriptor.GetProperties(ownerItem)["DropDownItems"]);
                        }
                        catch
                        {
                            if (this._pendingTransaction != null)
                            {
                                this._pendingTransaction.Cancel();
                                this._pendingTransaction = null;
                            }
                        }
                    }
                }
            }
        }

        private ToolStripItem CreateDummyItem(System.Type t, int dummyIndex)
        {
            if (this.designerHost == null)
            {
                return null;
            }
            ToolStripItem component = null;
            if ((this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveLeft) || (this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveRight))
            {
                dummyIndex++;
            }
            try
            {
                ToolStripDesigner._autoAddNewItems = false;
                this.indexToInsertNewItem = dummyIndex;
                try
                {
                    if (this.newMenuItemTransaction == null)
                    {
                        this.newMenuItemTransaction = this.designerHost.CreateTransaction(System.Design.SR.GetString("ToolStripCreatingNewItemTransaction"));
                    }
                    this.fireComponentChanged = true;
                    component = (ToolStripItem) this.designerHost.CreateComponent(t);
                }
                finally
                {
                    this.fireComponentChanged = false;
                }
                ToolStripItemDesigner designer = this.designerHost.GetDesigner(component) as ToolStripItemDesigner;
                try
                {
                    designer.InternalCreate = true;
                    if (designer != null)
                    {
                        designer.InitializeNewComponent(null);
                    }
                }
                finally
                {
                    designer.InternalCreate = false;
                }
                return component;
            }
            catch (InvalidOperationException exception)
            {
                this.CommitInsertTransaction(false);
                if (this.newMenuItemTransaction != null)
                {
                    this.newMenuItemTransaction.Cancel();
                    this.newMenuItemTransaction = null;
                }
                ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception.Message);
            }
            finally
            {
                ToolStripDesigner._autoAddNewItems = true;
                this.indexToInsertNewItem = -1;
            }
            return component;
        }

        private void CreateDummyMenuItem(ToolStripItem item, string text, Image image)
        {
            this.commitedTemplateNode = new ToolStripTemplateNode(base.Component, text, image);
            this.commitedTemplateNode.ActiveItem = item;
            int width = this.commitedTemplateNode.EditorToolStrip.Width;
            this.commitedEditorNode = new DesignerToolStripControlHost(this.commitedTemplateNode.EditorToolStrip);
            this.commitedEditorNode.AutoSize = false;
            this.commitedEditorNode.Width = width;
        }

        private ToolStripItem CreateNewItem(System.Type t, int dummyIndex, string newText)
        {
            if (this.designerHost == null)
            {
                return null;
            }
            ToolStripItem component = null;
            if ((this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveLeft) || (this.MenuItem.DropDownDirection == ToolStripDropDownDirection.AboveRight))
            {
                dummyIndex++;
            }
            DesignerTransaction transaction = this.designerHost.CreateTransaction(System.Design.SR.GetString("ToolStripCreatingNewItemTransaction"));
            try
            {
                ToolStripDesigner._autoAddNewItems = false;
                this.indexToInsertNewItem = dummyIndex;
                try
                {
                    this.fireComponentChanged = true;
                    component = (ToolStripItem) this.designerHost.CreateComponent(t, ToolStripDesigner.NameFromText(newText, t, this.MenuItem.Site));
                }
                finally
                {
                    this.fireComponentChanged = false;
                }
                ToolStripItemDesigner designer = this.designerHost.GetDesigner(component) as ToolStripItemDesigner;
                try
                {
                    if (!string.IsNullOrEmpty(newText) || this.addingDummyItem)
                    {
                        designer.InternalCreate = true;
                    }
                    if (designer != null)
                    {
                        designer.InitializeNewComponent(null);
                    }
                }
                finally
                {
                    designer.InternalCreate = false;
                }
                if (component != null)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["Text"];
                    if ((descriptor != null) && !string.IsNullOrEmpty(newText))
                    {
                        descriptor.SetValue(component, newText);
                    }
                }
            }
            catch
            {
                this.CommitInsertTransaction(false);
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
                ToolStripDesigner._autoAddNewItems = true;
                this.indexToInsertNewItem = -1;
            }
            return component;
        }

        private void CreatetypeHereNode()
        {
            if (this.typeHereNode == null)
            {
                this.AddNewTemplateNode(this.MenuItem.DropDown);
                if (this.MenuItem.DropDown.Site == null)
                {
                    this.MenuItem.DropDown.Text = this.MenuItem.Name + ".DropDown";
                }
            }
            else if ((this.typeHereNode != null) && (this.MenuItem.DropDownItems.IndexOf(this.typeHereNode) == -1))
            {
                this.MenuItem.DropDown.Items.Add(this.typeHereNode);
                this.typeHereNode.Visible = true;
            }
            this.MenuItem.DropDown.PerformLayout();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.selSvc != null)
                {
                    this.selSvc.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undoing -= new EventHandler(this.OnUndoing);
                    this.undoEngine.Undone -= new EventHandler(this.OnUndone);
                }
                if (this.MenuItem != null)
                {
                    this.MenuItem.DropDown.Hide();
                    this.UnHookEvents();
                }
                if (this.toolStripAdornerWindowService != null)
                {
                    this.toolStripAdornerWindowService = null;
                }
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRemoved -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoved);
                    service.ComponentRemoving -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoving);
                    service.ComponentAdding -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdding);
                    service.ComponentAdded -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdded);
                }
                if (this.typeHereTemplateNode != null)
                {
                    this.typeHereTemplateNode.RollBack();
                    this.typeHereTemplateNode.CloseEditor();
                    this.typeHereTemplateNode = null;
                }
                if (this.typeHereNode != null)
                {
                    this.typeHereNode.Dispose();
                    this.typeHereNode = null;
                }
                if (this.commitedTemplateNode != null)
                {
                    this.commitedTemplateNode.RollBack();
                    this.commitedTemplateNode.CloseEditor();
                    this.commitedTemplateNode = null;
                }
                if (this.commitedEditorNode != null)
                {
                    this.commitedEditorNode.Dispose();
                    this.commitedEditorNode = null;
                }
                if (this.parentItem != null)
                {
                    this.parentItem = null;
                }
            }
            base.Dispose(disposing);
        }

        private void DropDownClick(object sender, EventArgs e)
        {
            if ((this.KeyboardHandlingService != null) && this.KeyboardHandlingService.TemplateNodeActive)
            {
                this.KeyboardHandlingService.ActiveTemplateNode.CommitAndSelect();
            }
        }

        private void DropDownItem_DropDownClosed(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            if (item != null)
            {
                if (((this.toolStripAdornerWindowService != null) && (this.rootControlGlyph != null)) && this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Contains(this.rootControlGlyph))
                {
                    this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Remove(this.rootControlGlyph);
                }
                this.InitializeBodyGlyphsForItems(false, item);
                this.initialized = false;
                this.UnHookEvents();
                if ((item.DropDown.Site != null) || (item.DropDownItems.Count == 1))
                {
                    this.RemoveTypeHereNode(item);
                }
                else if (this.toolStripAdornerWindowService != null)
                {
                    this.toolStripAdornerWindowService.Invalidate(item.DropDown.Bounds);
                }
            }
        }

        private void DropDownItem_DropDownOpened(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            if (item != null)
            {
                this.ResetGlyphs(item);
            }
            Control dropDown = item.DropDown;
            if (dropDown != null)
            {
                ControlDesigner designer = this.designerHost.GetDesigner(this.designerHost.RootComponent) as ControlDesigner;
                if (designer != null)
                {
                    this.rootControlGlyph = new ToolStripDropDownGlyph(dropDown.Bounds, new DropDownBehavior(designer, this));
                }
                if (this.toolStripAdornerWindowService != null)
                {
                    this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Add(this.rootControlGlyph);
                }
            }
        }

        private void DropDownItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            if (this.toolStripAdornerWindowService != null)
            {
                item.DropDown.TopLevel = false;
                item.DropDown.Parent = this.toolStripAdornerWindowService.ToolStripAdornerWindowControl;
            }
        }

        private void DropDownLocationChanged(object sender, EventArgs e)
        {
            ToolStripDropDown down = sender as ToolStripDropDown;
            if (down.Visible)
            {
                BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                if (service != null)
                {
                    service.Invalidate();
                }
            }
        }

        private void DropDownPaint(object sender, PaintEventArgs e)
        {
            if ((this.selSvc != null) && (this.MenuItem != null))
            {
                foreach (ToolStripItem item in this.MenuItem.DropDownItems)
                {
                    if (item.Visible && this.selSvc.GetComponentSelected(item))
                    {
                        ToolStripItemDesigner designer = this.designerHost.GetDesigner(item) as ToolStripItemDesigner;
                        if (designer != null)
                        {
                            Rectangle glyphBounds = designer.GetGlyphBounds();
                            ToolStripDesignerUtils.GetAdjustedBounds(item, ref glyphBounds);
                            glyphBounds.Inflate(2, 2);
                            BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                            if (service != null)
                            {
                                service.ProcessPaintMessage(glyphBounds);
                            }
                        }
                    }
                    DesignerToolStripControlHost host = item as DesignerToolStripControlHost;
                    if (host != null)
                    {
                        host.Control.Refresh();
                    }
                }
            }
        }

        private void DropDownResize(object sender, EventArgs e)
        {
            ToolStripDropDown down = sender as ToolStripDropDown;
            if (!base.dummyItemAdded)
            {
                if ((((down != null) && down.Visible) && (this.toolStripAdornerWindowService != null)) && ((down.Width < this.dropDownSizeToInvalidate.Width) || (down.Size.Height < this.dropDownSizeToInvalidate.Height)))
                {
                    using (Region region = new Region(this.dropDownSizeToInvalidate))
                    {
                        region.Exclude(down.Bounds);
                        this.toolStripAdornerWindowService.Invalidate(region);
                        BehaviorService service = (BehaviorService) this.GetService(typeof(BehaviorService));
                        if (service != null)
                        {
                            service.Invalidate(region);
                        }
                    }
                }
                if (this.toolStripAdornerWindowService != null)
                {
                    if ((this.rootControlGlyph != null) && this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Contains(this.rootControlGlyph))
                    {
                        this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Remove(this.rootControlGlyph);
                    }
                    ControlDesigner designer = this.designerHost.GetDesigner(this.designerHost.RootComponent) as ControlDesigner;
                    if (designer != null)
                    {
                        this.rootControlGlyph = new ToolStripDropDownGlyph(down.Bounds, new DropDownBehavior(designer, this));
                    }
                    this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Add(this.rootControlGlyph);
                }
            }
            this.dropDownSizeToInvalidate = down.Bounds;
        }

        internal void EditTemplateNode(bool clicked)
        {
            this.typeHereNode.RefreshSelectionGlyph();
            if ((this.KeyboardHandlingService != null) && this.KeyboardHandlingService.TemplateNodeActive)
            {
                this.KeyboardHandlingService.ActiveTemplateNode.CommitAndSelect();
            }
            if (!clicked || (this.MenuItem != null))
            {
                try
                {
                    ToolStripDesigner.editTemplateNode = true;
                    this.selSvc.SetSelectedComponents(new object[] { this.MenuItem }, SelectionTypes.Replace);
                }
                finally
                {
                    ToolStripDesigner.editTemplateNode = false;
                }
                ToolStripDropDownItem ownerItem = null;
                if ((this.selSvc.PrimarySelection == null) && (this.KeyboardHandlingService != null))
                {
                    ToolStripItem selectedDesignerControl = this.KeyboardHandlingService.SelectedDesignerControl as ToolStripItem;
                    if (selectedDesignerControl != null)
                    {
                        ownerItem = ((ToolStripDropDown) selectedDesignerControl.Owner).OwnerItem as ToolStripDropDownItem;
                    }
                }
                else
                {
                    ownerItem = this.selSvc.PrimarySelection as ToolStripDropDownItem;
                }
                if ((ownerItem != null) && (ownerItem != this.MenuItem))
                {
                    this.HideSiblingDropDowns(ownerItem);
                }
                this.MenuItem.DropDown.SuspendLayout();
                base.dummyItemAdded = true;
                int index = this.MenuItem.DropDownItems.IndexOf(this.typeHereNode);
                ToolStripItem component = null;
                try
                {
                    this.addingDummyItem = true;
                    component = this.CreateDummyItem(typeof(ToolStripMenuItem), index);
                }
                catch (CheckoutException exception)
                {
                    if (!exception.Equals(CheckoutException.Canceled))
                    {
                        throw;
                    }
                    this.CommitInsertTransaction(false);
                    if (this.newMenuItemTransaction != null)
                    {
                        this.newMenuItemTransaction.Cancel();
                        this.newMenuItemTransaction = null;
                    }
                }
                finally
                {
                    base.dummyItemAdded = component != null;
                    this.addingDummyItem = false;
                }
                this.MenuItem.DropDown.ResumeLayout();
                if (component != null)
                {
                    ToolStripMenuItemDesigner designer = this.designerHost.GetDesigner(component) as ToolStripMenuItemDesigner;
                    if (designer != null)
                    {
                        designer.InitializeDropDown();
                        designer.ShowEditNode(clicked);
                    }
                }
            }
        }

        internal void EnterInSituEdit(ToolStripItem toolItem)
        {
            this.MenuItem.DropDown.SuspendLayout();
            this.RemoveItemBodyGlyph(toolItem);
            if ((toolItem != null) && !base.IsEditorActive)
            {
                this.CreateDummyMenuItem(toolItem, toolItem.Text, toolItem.Image);
                int index = this.MenuItem.DropDownItems.IndexOf(toolItem);
                this.MenuItem.DropDownItems.Insert(index, this.commitedEditorNode);
                if (toolItem is ToolStripControlHost)
                {
                    ((ToolStripControlHost) toolItem).Control.Visible = false;
                }
                toolItem.Visible = false;
                this.MenuItem.DropDown.ResumeLayout();
                if (this.commitedTemplateNode != null)
                {
                    this.commitedTemplateNode.FocusEditor(toolItem);
                }
                ToolStripDropDownItem item = toolItem as ToolStripDropDownItem;
                if ((!(item.Owner is ToolStripDropDownMenu) && (item != null)) && (item.Bounds.Width < this.commitedEditorNode.Bounds.Width))
                {
                    item.Width = this.commitedEditorNode.Width;
                    item.DropDown.Location = new Point((item.DropDown.Location.X + this.commitedEditorNode.Bounds.Width) - item.Bounds.Width, item.DropDown.Location.Y);
                }
                base.IsEditorActive = true;
            }
        }

        private void EnterInSituMode()
        {
            if (this.MenuItem.Owner is ToolStripDropDown)
            {
                ToolStripItem ownerItem = ((ToolStripDropDown) this.MenuItem.Owner).OwnerItem;
                if (this.designerHost != null)
                {
                    IDesigner designer = this.designerHost.GetDesigner(ownerItem);
                    if (designer is ToolStripMenuItemDesigner)
                    {
                        this.MenuItem.HideDropDown();
                        ((ToolStripMenuItemDesigner) designer).EnterInSituEdit(this.MenuItem);
                    }
                }
            }
        }

        private int GetItemInsertionIndex(ToolStripDropDown wb, Point ownerClientAreaRelativeDropPoint)
        {
            for (int i = 0; i < wb.Items.Count; i++)
            {
                Rectangle bounds = wb.Items[i].Bounds;
                bounds.Inflate(wb.Items[i].Margin.Size);
                if (bounds.Contains(ownerClientAreaRelativeDropPoint))
                {
                    return wb.Items.IndexOf(wb.Items[i]);
                }
            }
            return -1;
        }

        internal override ToolStrip GetMainToolStrip()
        {
            ToolStripDropDown firstDropDown = base.GetFirstDropDown(this.MenuItem);
            ToolStripItem item = (firstDropDown == null) ? null : firstDropDown.OwnerItem;
            if (item != null)
            {
                return item.Owner;
            }
            return this.MenuItem.Owner;
        }

        protected override Component GetOwnerForActionList()
        {
            return this.MenuItem;
        }

        private void HideAllDropDowns(ToolStripDropDownItem item)
        {
            try
            {
                if (this.MenuItem.Owner is ToolStripDropDown)
                {
                    ToolStripItem ownerItem = ((ToolStripDropDown) this.MenuItem.Owner).OwnerItem;
                    while (item != ownerItem)
                    {
                        if (item.DropDown.Visible)
                        {
                            item.HideDropDown();
                        }
                        if (item.Owner is ToolStripDropDown)
                        {
                            item = (ToolStripDropDownItem) ((ToolStripDropDown) item.Owner).OwnerItem;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
        }

        private void HideSiblingDropDowns(ToolStripDropDownItem item)
        {
            try
            {
                ToolStripItem menuItem = this.MenuItem;
                while (item != menuItem)
                {
                    item.HideDropDown();
                    if (item.Owner is ToolStripDropDown)
                    {
                        item = (ToolStripDropDownItem) ((ToolStripDropDown) item.Owner).OwnerItem;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
        }

        internal void HookEvents()
        {
            if (this.MenuItem != null)
            {
                this.MenuItem.DropDown.Closing += new ToolStripDropDownClosingEventHandler(this.OnDropDownClosing);
                this.MenuItem.DropDownOpening += new EventHandler(this.DropDownItem_DropDownOpening);
                this.MenuItem.DropDownOpened += new EventHandler(this.DropDownItem_DropDownOpened);
                this.MenuItem.DropDownClosed += new EventHandler(this.DropDownItem_DropDownClosed);
                this.MenuItem.DropDown.Resize += new EventHandler(this.DropDownResize);
                this.MenuItem.DropDown.ItemAdded += new ToolStripItemEventHandler(this.OnItemAdded);
                this.MenuItem.DropDown.Paint += new PaintEventHandler(this.DropDownPaint);
                this.MenuItem.DropDown.Click += new EventHandler(this.DropDownClick);
                this.MenuItem.DropDown.LocationChanged += new EventHandler(this.DropDownLocationChanged);
            }
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.Visible = true;
            this.DoubleClickEnabled = this.MenuItem.DoubleClickEnabled;
            this.selSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
            if (this.selSvc != null)
            {
                this.selSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            this.toolStripAdornerWindowService = (ToolStripAdornerWindowService) this.GetService(typeof(ToolStripAdornerWindowService));
            this.designerHost = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            this.MenuItem.DoubleClickEnabled = true;
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentRemoved += new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoved);
                service.ComponentRemoving += new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoving);
                service.ComponentAdding += new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdding);
                service.ComponentAdded += new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdded);
            }
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

        internal void InitializeBodyGlyphsForItems(bool addGlyphs, ToolStripDropDownItem item)
        {
            if (addGlyphs)
            {
                this.AddBodyGlyphs(item);
            }
            else
            {
                this.RemoveBodyGlyphs(item);
            }
        }

        internal void InitializeDropDown()
        {
            ToolStrip mainToolStrip = this.GetMainToolStrip();
            ToolStripDropDown firstDropDown = base.GetFirstDropDown(this.MenuItem);
            if (firstDropDown != null)
            {
                ToolStripItem ownerItem = firstDropDown.OwnerItem;
                if (((ownerItem != null) && (ownerItem.GetCurrentParent() is ToolStripOverflow)) && !mainToolStrip.CanOverflow)
                {
                    return;
                }
            }
            if (!this.initialized)
            {
                this.initialized = true;
                ToolStripDropDownItem component = this.MenuItem.DropDown.OwnerItem as ToolStripDropDownItem;
                if ((component != null) && (component != this.MenuItem))
                {
                    ToolStripMenuItemDesigner designer = this.designerHost.GetDesigner(component) as ToolStripMenuItemDesigner;
                    if (designer != null)
                    {
                        designer.RemoveTypeHereNode(component);
                    }
                    component.HideDropDown();
                }
                if (this.MenuItem.DropDown.Site != null)
                {
                    ToolStripDropDownDesigner designer2 = this.designerHost.GetDesigner(this.MenuItem.DropDown) as ToolStripDropDownDesigner;
                    if (designer2 != null)
                    {
                        designer2.currentParent = this.MenuItem as ToolStripMenuItem;
                    }
                }
                this.CreatetypeHereNode();
                this.MenuItem.DropDown.TopLevel = false;
                this.MenuItem.DropDown.AllowDrop = true;
                this.HookEvents();
                this.MenuItem.DropDown.AutoClose = false;
                this.MenuItem.ShowDropDown();
                this.ShowOwnerDropDown(this.MenuItem);
                this.ResetGlyphs(this.MenuItem);
                if (!this.IsOnContextMenu && !base.dummyItemAdded)
                {
                    SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                    if (service != null)
                    {
                        service.Refresh();
                    }
                }
            }
        }

        private bool IsParentDropDown(ToolStripDropDown currentDropDown)
        {
            if (currentDropDown == null)
            {
                return false;
            }
            ToolStripDropDown owner = this.MenuItem.Owner as ToolStripDropDown;
            while ((owner != null) && (owner != currentDropDown))
            {
                ToolStripDropDownItem ownerItem = owner.OwnerItem as ToolStripDropDownItem;
                if (ownerItem != null)
                {
                    owner = ownerItem.Owner as ToolStripDropDown;
                }
                else
                {
                    owner = null;
                }
            }
            if (owner == null)
            {
                return false;
            }
            return true;
        }

        internal override ToolStripItem MorphCurrentItem(System.Type t)
        {
            Rectangle bounds = this.MenuItem.GetCurrentParent().Bounds;
            Rectangle b = this.MenuItem.DropDown.Bounds;
            this.InitializeBodyGlyphsForItems(false, this.MenuItem);
            Rectangle rect = Rectangle.Union(bounds, b);
            ToolStripAdornerWindowService toolStripAdornerWindowService = this.toolStripAdornerWindowService;
            ToolStripItem item = base.MorphCurrentItem(t);
            if (toolStripAdornerWindowService != null)
            {
                toolStripAdornerWindowService.Invalidate(rect);
                toolStripAdornerWindowService = null;
            }
            return item;
        }

        private void OnDropDownClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
        }

        private void OnDropDownDisposed(object sender, EventArgs e)
        {
            if (this.MenuItem != null)
            {
                if (this.MenuItem.DropDown != null)
                {
                    this.MenuItem.DropDown.Disposed -= new EventHandler(this.OnDropDownDisposed);
                }
                this.MenuItem.DropDown = null;
            }
        }

        private void OnItemAdded(object sender, ToolStripItemEventArgs e)
        {
            if (((this.MenuItem.DropDownDirection != ToolStripDropDownDirection.AboveLeft) && (this.MenuItem.DropDownDirection != ToolStripDropDownDirection.AboveRight)) && ((this.typeHereNode != null) && (e.Item != this.typeHereNode)))
            {
                int index = this.MenuItem.DropDown.Items.IndexOf(this.typeHereNode);
                if ((index >= 0) && (index < (this.MenuItem.DropDown.Items.Count - 1)))
                {
                    this.MenuItem.DropDown.ItemAdded -= new ToolStripItemEventHandler(this.OnItemAdded);
                    this.MenuItem.DropDown.SuspendLayout();
                    this.MenuItem.DropDown.Items.Remove(this.typeHereNode);
                    this.MenuItem.DropDown.Items.Add(this.typeHereNode);
                    this.MenuItem.DropDown.ResumeLayout();
                    this.MenuItem.DropDown.ItemAdded += new ToolStripItemEventHandler(this.OnItemAdded);
                }
                else
                {
                    this.CreatetypeHereNode();
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.MenuItem != null)
            {
                ISelectionService service = sender as ISelectionService;
                if (service != null)
                {
                    if ((this.commitedTemplateNode != null) && this.commitedTemplateNode.Active)
                    {
                        this.commitedTemplateNode.Commit(false, false);
                    }
                    else if ((this.typeHereTemplateNode != null) && this.typeHereTemplateNode.Active)
                    {
                        this.typeHereTemplateNode.Commit(false, false);
                    }
                    if (this.MenuItem.Equals(service.PrimarySelection))
                    {
                        ArrayList originalSelComps = ToolStripDesignerUtils.originalSelComps;
                        if (originalSelComps != null)
                        {
                            ToolStripDesignerUtils.InvalidateSelection(originalSelComps, this.MenuItem, this.MenuItem.Site, false);
                        }
                        if (this.IsOnContextMenu && !this.MenuItem.Owner.Visible)
                        {
                            ToolStripDropDown firstDropDown = base.GetFirstDropDown(this.MenuItem);
                            ToolStripDropDownDesigner designer = this.designerHost.GetDesigner(firstDropDown) as ToolStripDropDownDesigner;
                            if (designer != null)
                            {
                                this.InitializeDropDown();
                                designer.ShowMenu();
                                designer.AddSelectionGlyphs();
                            }
                        }
                        else
                        {
                            this.InitializeDropDown();
                        }
                        ICollection c = null;
                        if (this.selSvc != null)
                        {
                            c = service.GetSelectedComponents();
                        }
                        originalSelComps = new ArrayList(c);
                        if (((originalSelComps.Count == 0) && (this.KeyboardHandlingService != null)) && (this.KeyboardHandlingService.SelectedDesignerControl != null))
                        {
                            originalSelComps.Add(this.KeyboardHandlingService.SelectedDesignerControl);
                        }
                        if (originalSelComps.Count > 0)
                        {
                            ToolStripDesignerUtils.originalSelComps = originalSelComps;
                        }
                    }
                    else
                    {
                        object primarySelection = ((ISelectionService) sender).PrimarySelection;
                        if ((primarySelection == null) && (this.KeyboardHandlingService != null))
                        {
                            primarySelection = this.KeyboardHandlingService.SelectedDesignerControl;
                        }
                        ToolStripItem item = primarySelection as ToolStripItem;
                        if (item != null)
                        {
                            for (ToolStripDropDown down2 = item.Owner as ToolStripDropDown; down2 != null; down2 = down2.OwnerItem.Owner as ToolStripDropDown)
                            {
                                if ((down2.OwnerItem == this.MenuItem) || (down2.OwnerItem == null))
                                {
                                    return;
                                }
                            }
                        }
                        if (this.MenuItem.DropDown.Visible)
                        {
                            ToolStripDropDown down3 = primarySelection as ToolStripDropDown;
                            if ((down3 == null) || (this.MenuItem.DropDown != down3))
                            {
                                ToolStripItem item2 = primarySelection as ToolStripItem;
                                if (item2 != null)
                                {
                                    for (ToolStripDropDown down4 = item2.Owner as ToolStripDropDown; down4 != null; down4 = down4.OwnerItem.Owner as ToolStripDropDown)
                                    {
                                        if (down4 == this.MenuItem.DropDown)
                                        {
                                            return;
                                        }
                                    }
                                }
                                if (this.MenuItem.DropDown.OwnerItem == this.MenuItem)
                                {
                                    this.MenuItem.HideDropDown();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnUndoing(object source, EventArgs e)
        {
            if (!base.dummyItemAdded && (!this.IsOnContextMenu && this.MenuItem.DropDown.Visible))
            {
                this.MenuItem.HideDropDown();
                if (!this.MenuItem.DropDown.IsAutoGenerated)
                {
                    this.dropDownSet = true;
                    ToolStrip mainToolStrip = this.GetMainToolStrip();
                    ToolStripDesigner designer = this.designerHost.GetDesigner(mainToolStrip) as ToolStripDesigner;
                    if (designer != null)
                    {
                        designer.CacheItems = true;
                        designer.Items.Clear();
                    }
                }
                this.undoingCalled = true;
            }
        }

        private void OnUndone(object source, EventArgs e)
        {
            if (this.undoingCalled)
            {
                if (this.dropDownSet && this.MenuItem.DropDown.IsAutoGenerated)
                {
                    ToolStrip mainToolStrip = this.GetMainToolStrip();
                    ToolStripDesigner designer = this.designerHost.GetDesigner(mainToolStrip) as ToolStripDesigner;
                    if ((designer != null) && designer.CacheItems)
                    {
                        foreach (ToolStripItem item in designer.Items)
                        {
                            this.MenuItem.DropDownItems.Insert(0, item);
                        }
                        designer.CacheItems = false;
                    }
                    this.ResetGlyphs(this.MenuItem);
                }
                if ((this.MenuItem != null) && this.selSvc.GetComponentSelected(this.MenuItem))
                {
                    this.InitializeDropDown();
                    this.MenuItem.DropDown.PerformLayout();
                }
                this.undoingCalled = false;
                this.dropDownSet = false;
            }
            if (this.selSvc.GetComponentSelected(this.MenuItem) && !this.dropDownSetFailed)
            {
                this.InitializeDropDown();
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Visible", "DoubleClickEnabled", "CheckOnClick", "DropDown" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ToolStripMenuItemDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private void RemoveBodyGlyphs(ToolStripDropDownItem item)
        {
            if (item != null)
            {
                foreach (ToolStripItem item2 in item.DropDownItems)
                {
                    ToolStripItemDesigner designer = (ToolStripItemDesigner) this.designerHost.GetDesigner(item2);
                    if (designer != null)
                    {
                        ControlBodyGlyph bodyGlyph = designer.bodyGlyph;
                        if (((bodyGlyph != null) && (this.toolStripAdornerWindowService != null)) && this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Contains(bodyGlyph))
                        {
                            this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Remove(bodyGlyph);
                            designer.bodyGlyph = null;
                        }
                    }
                }
            }
        }

        internal void RemoveItemBodyGlyph(ToolStripItem item)
        {
            if (item != null)
            {
                ToolStripItemDesigner designer = (ToolStripItemDesigner) this.designerHost.GetDesigner(item);
                if (designer != null)
                {
                    ControlBodyGlyph bodyGlyph = designer.bodyGlyph;
                    if (((bodyGlyph != null) && (this.toolStripAdornerWindowService != null)) && this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Contains(bodyGlyph))
                    {
                        this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Remove(bodyGlyph);
                        designer.bodyGlyph = null;
                    }
                }
            }
        }

        internal void RemoveTypeHereNode(ToolStripDropDownItem ownerItem)
        {
            Rectangle bounds = ownerItem.DropDown.Bounds;
            if ((ownerItem.DropDownItems.Count > 0) && (ownerItem.DropDownItems[0] is DesignerToolStripControlHost))
            {
                ownerItem.DropDownItems.RemoveAt(0);
            }
            if ((this.typeHereTemplateNode != null) && this.typeHereTemplateNode.Active)
            {
                this.typeHereTemplateNode.RollBack();
                this.typeHereTemplateNode.CloseEditor();
                this.typeHereTemplateNode = null;
            }
            if (this.typeHereNode != null)
            {
                this.typeHereNode.Dispose();
                this.typeHereNode = null;
            }
            if (this.toolStripAdornerWindowService != null)
            {
                this.toolStripAdornerWindowService.Invalidate(bounds);
            }
        }

        private void ResetCheckOnClick()
        {
            this.CheckOnClick = false;
        }

        private void ResetDoubleClickEnabled()
        {
            this.DoubleClickEnabled = false;
        }

        private void ResetDropDown()
        {
            this.DropDown = null;
        }

        internal void ResetGlyphs(ToolStripDropDownItem item)
        {
            if (item.DropDown.Visible)
            {
                this.InitializeBodyGlyphsForItems(false, item);
                this.InitializeBodyGlyphsForItems(true, item);
            }
        }

        private void ResetVisible()
        {
            base.Visible = true;
        }

        private void RestoreVisible()
        {
            this.MenuItem.Visible = base.Visible;
        }

        private void RollBack()
        {
            if (this.commitedEditorNode != null)
            {
                int index = this.MenuItem.DropDownItems.IndexOf(this.commitedEditorNode);
                ToolStripDropDownItem item = (ToolStripDropDownItem) this.MenuItem.DropDownItems[index + 1];
                if (item != null)
                {
                    item.Visible = true;
                }
                this.MenuItem.DropDown.Items.Remove(this.commitedEditorNode);
                if (this.commitedTemplateNode != null)
                {
                    this.commitedTemplateNode.RollBack();
                    this.commitedTemplateNode.CloseEditor();
                    this.commitedTemplateNode = null;
                }
                if (this.commitedEditorNode != null)
                {
                    this.commitedEditorNode.Dispose();
                    this.commitedEditorNode = null;
                }
            }
        }

        private void SelectItems(ToolStripDropDownItem oldSelection, ISelectionService selSvc)
        {
            ToolStripDropDown owner = (ToolStripDropDown) this.MenuItem.Owner;
            int num = Math.Max(owner.Items.IndexOf(oldSelection), owner.Items.IndexOf(this.MenuItem));
            int num2 = Math.Min(owner.Items.IndexOf(oldSelection), owner.Items.IndexOf(this.MenuItem));
            ToolStripItem[] components = new ToolStripItem[(num - num2) + 1];
            int index = 0;
            while (num2 <= num)
            {
                components[index] = owner.Items[num2];
                index++;
                num2++;
            }
            selSvc.SetSelectedComponents(components);
        }

        internal override bool SetSelection(bool enterKeyPressed)
        {
            if (!enterKeyPressed)
            {
                return false;
            }
            if (!this.initialized)
            {
                this.InitializeDropDown();
            }
            if ((this.selSvc != null) && (this.KeyboardHandlingService != null))
            {
                int num = 0;
                if ((this.MenuItem.DropDownDirection != ToolStripDropDownDirection.AboveLeft) && (this.MenuItem.DropDownDirection != ToolStripDropDownDirection.AboveRight))
                {
                    num = this.MenuItem.DropDownItems.Count - 1;
                }
                this.selSvc.SetSelectedComponents(new object[] { this.MenuItem }, SelectionTypes.Replace);
                if (num >= 0)
                {
                    this.KeyboardHandlingService.SelectedDesignerControl = this.MenuItem.DropDownItems[num];
                    this.selSvc.SetSelectedComponents(null, SelectionTypes.Replace);
                }
            }
            return true;
        }

        private bool ShouldSerializeCheckOnClick()
        {
            return (bool) base.ShadowProperties["CheckOnClick"];
        }

        private bool ShouldSerializeDoubleClickEnabled()
        {
            return (bool) base.ShadowProperties["DoubleClickEnabled"];
        }

        private bool ShouldSerializeDropDown()
        {
            return (this.customDropDown != null);
        }

        private bool ShouldSerializeVisible()
        {
            return !base.Visible;
        }

        internal override void ShowEditNode(bool clicked)
        {
            if (this.MenuItem != null)
            {
                try
                {
                    if (this.MenuItem.Owner is ToolStripDropDown)
                    {
                        this.parentItem = ((ToolStripDropDown) this.MenuItem.Owner).OwnerItem;
                        if (this.designerHost != null)
                        {
                            IDesigner designer = this.designerHost.GetDesigner(this.parentItem);
                            if (designer is ToolStripMenuItemDesigner)
                            {
                                ((ToolStripMenuItemDesigner) designer).EnterInSituEdit(this.MenuItem);
                            }
                        }
                    }
                    else
                    {
                        base.ShowEditNode(clicked);
                    }
                }
                catch (CheckoutException exception)
                {
                    if (!exception.Equals(CheckoutException.Canceled))
                    {
                        throw;
                    }
                }
            }
        }

        internal void ShowOwnerDropDown(ToolStripDropDownItem currentSelection)
        {
            while ((currentSelection != null) && (currentSelection.Owner is ToolStripDropDown))
            {
                currentSelection = (ToolStripDropDownItem) ((ToolStripDropDown) currentSelection.Owner).OwnerItem;
                if ((currentSelection != null) && !currentSelection.DropDown.Visible)
                {
                    ToolStripMenuItemDesigner designer = this.designerHost.GetDesigner(currentSelection) as ToolStripMenuItemDesigner;
                    if (designer != null)
                    {
                        designer.InitializeDropDown();
                    }
                }
            }
        }

        internal void UnHookEvents()
        {
            if (this.MenuItem != null)
            {
                this.MenuItem.DropDown.Closing -= new ToolStripDropDownClosingEventHandler(this.OnDropDownClosing);
                this.MenuItem.DropDownOpening -= new EventHandler(this.DropDownItem_DropDownOpening);
                this.MenuItem.DropDownOpened -= new EventHandler(this.DropDownItem_DropDownOpened);
                this.MenuItem.DropDownClosed -= new EventHandler(this.DropDownItem_DropDownClosed);
                this.MenuItem.DropDown.Resize -= new EventHandler(this.DropDownResize);
                this.MenuItem.DropDown.ItemAdded -= new ToolStripItemEventHandler(this.OnItemAdded);
                this.MenuItem.DropDown.Paint -= new PaintEventHandler(this.DropDownPaint);
                this.MenuItem.DropDown.LocationChanged -= new EventHandler(this.DropDownLocationChanged);
                this.MenuItem.DropDown.Click -= new EventHandler(this.DropDownClick);
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ArrayList list = new ArrayList();
                if (this.MenuItem.DropDown.IsAutoGenerated)
                {
                    foreach (ToolStripItem item in this.MenuItem.DropDownItems)
                    {
                        if (!(item is DesignerToolStripControlHost))
                        {
                            list.Add(item);
                        }
                    }
                }
                return list;
            }
        }

        private bool CheckOnClick
        {
            get
            {
                return (bool) base.ShadowProperties["CheckOnClick"];
            }
            set
            {
                base.ShadowProperties["CheckOnClick"] = value;
            }
        }

        private bool DoubleClickEnabled
        {
            get
            {
                return (bool) base.ShadowProperties["DoubleClickEnabled"];
            }
            set
            {
                base.ShadowProperties["DoubleClickEnabled"] = value;
            }
        }

        private ToolStripDropDown DropDown
        {
            get
            {
                return this.customDropDown;
            }
            set
            {
                this.dropDownSetFailed = false;
                if (this.IsParentDropDown(value))
                {
                    this.dropDownSetFailed = true;
                    throw new ArgumentException(System.Design.SR.GetString("InvalidArgument", new object[] { "DropDown", value.ToString() }));
                }
                if (this.MenuItem.DropDown != null)
                {
                    this.RemoveTypeHereNode(this.MenuItem);
                    if (this.MenuItem.DropDown.IsAutoGenerated && (this.MenuItem.DropDownItems.Count > 0))
                    {
                        ComponentSerializationService service = this.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                        if (service != null)
                        {
                            this.serializedDataForDropDownItems = service.CreateStore();
                            foreach (ToolStripItem item in this.MenuItem.DropDownItems)
                            {
                                if (!(item is DesignerToolStripControlHost))
                                {
                                    service.Serialize(this.serializedDataForDropDownItems, item);
                                }
                            }
                            this.serializedDataForDropDownItems.Close();
                        }
                        ToolStripItem[] array = new ToolStripItem[this.MenuItem.DropDownItems.Count];
                        this.MenuItem.DropDownItems.CopyTo(array, 0);
                        foreach (ToolStripItem item2 in array)
                        {
                            this.MenuItem.DropDownItems.Remove(item2);
                            this.designerHost.DestroyComponent(item2);
                        }
                    }
                    this.MenuItem.HideDropDown();
                }
                this.MenuItem.DropDown = value;
                this.customDropDown = value;
                if (((value == null) && !this.dropDownSet) && (this.serializedDataForDropDownItems != null))
                {
                    try
                    {
                        ToolStripDesigner._autoAddNewItems = false;
                        this.CreatetypeHereNode();
                        ComponentSerializationService service2 = this.GetService(typeof(ComponentSerializationService)) as ComponentSerializationService;
                        if (service2 != null)
                        {
                            service2.Deserialize(this.serializedDataForDropDownItems);
                            this.serializedDataForDropDownItems = null;
                        }
                    }
                    finally
                    {
                        ToolStripDesigner._autoAddNewItems = true;
                    }
                }
                this.MenuItem.DropDown.OwnerItem = this.MenuItem;
                this.MenuItem.DropDown.Disposed += new EventHandler(this.OnDropDownDisposed);
                if (this.MenuItem.Equals(this.selSvc.PrimarySelection))
                {
                    this.InitializeDropDown();
                }
            }
        }

        internal override ToolStripTemplateNode Editor
        {
            get
            {
                if (base.Editor != null)
                {
                    return base.Editor;
                }
                if (this.commitedTemplateNode != null)
                {
                    return this.commitedTemplateNode;
                }
                return this.typeHereTemplateNode;
            }
            set
            {
                this.commitedTemplateNode = value;
            }
        }

        private bool IsOnContextMenu
        {
            get
            {
                ToolStrip mainToolStrip = this.GetMainToolStrip();
                if (((mainToolStrip != null) && (mainToolStrip.Site != null)) && !(mainToolStrip is ContextMenuStrip))
                {
                    return false;
                }
                return true;
            }
        }

        private ToolStripKeyboardHandlingService KeyboardHandlingService
        {
            get
            {
                if (this.keyboardHandlingService == null)
                {
                    this.keyboardHandlingService = this.GetService(typeof(ToolStripKeyboardHandlingService)) as ToolStripKeyboardHandlingService;
                }
                return this.keyboardHandlingService;
            }
        }

        private ToolStripDropDownItem MenuItem
        {
            get
            {
                return (base.ToolStripItem as ToolStripDropDownItem);
            }
        }

        private bool MenuItemSelected
        {
            get
            {
                if (this.selSvc != null)
                {
                    object primarySelection = this.selSvc.PrimarySelection;
                    ToolStripItem component = null;
                    if (primarySelection == null)
                    {
                        if (this.KeyboardHandlingService != null)
                        {
                            primarySelection = this.KeyboardHandlingService.SelectedDesignerControl;
                        }
                        component = primarySelection as ToolStripItem;
                    }
                    else
                    {
                        component = primarySelection as ToolStripItem;
                    }
                    if (component != null)
                    {
                        if (this.designerHost != null)
                        {
                            ToolStripItemDesigner designer = this.designerHost.GetDesigner(component) as ToolStripItemDesigner;
                            if ((designer != null) && designer.dummyItemAdded)
                            {
                                return (component == this.MenuItem);
                            }
                        }
                        if (component.IsOnDropDown && (component.Owner is ToolStripDropDown))
                        {
                            ToolStripDropDownItem ownerItem = ((ToolStripDropDown) component.Owner).OwnerItem as ToolStripDropDownItem;
                            return (ownerItem == this.MenuItem);
                        }
                        return (component == this.MenuItem);
                    }
                    if ((primarySelection is ContextMenuStrip) && (this.MenuItem.DropDown == primarySelection))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        protected override IComponent ParentComponent
        {
            get
            {
                if (base.ToolStripItem == null)
                {
                    return null;
                }
                if (!base.ToolStripItem.IsOnOverflow && base.ToolStripItem.IsOnDropDown)
                {
                    ToolStripDropDown owner = this.MenuItem.Owner as ToolStripDropDown;
                    if (owner != null)
                    {
                        if (owner.IsAutoGenerated)
                        {
                            return owner.OwnerItem;
                        }
                        return owner;
                    }
                }
                return base.ParentComponent;
            }
        }

        internal class DropDownBehavior : ControlDesigner.TransparentBehavior
        {
            private ToolStripMenuItemDesigner menuItemDesigner;

            internal DropDownBehavior(ControlDesigner designer, ToolStripMenuItemDesigner menuItemDesigner) : base(designer)
            {
                this.menuItemDesigner = menuItemDesigner;
            }

            public override void OnDragDrop(Glyph g, DragEventArgs e)
            {
                ToolStripItemDataObject data = e.Data as ToolStripItemDataObject;
                if (data != null)
                {
                    ToolStripItem primarySelection = data.PrimarySelection;
                    IDesignerHost host = (IDesignerHost) primarySelection.Site.GetService(typeof(IDesignerHost));
                    ToolStripDropDown currentParent = primarySelection.GetCurrentParent() as ToolStripDropDown;
                    ToolStripDropDownItem component = null;
                    if (currentParent != null)
                    {
                        component = currentParent.OwnerItem as ToolStripDropDownItem;
                    }
                    if ((component != null) && (host != null))
                    {
                        string str;
                        ArrayList dragComponents = data.DragComponents;
                        int index = -1;
                        bool flag = e.Effect == DragDropEffects.Copy;
                        if (dragComponents.Count == 1)
                        {
                            string componentName = TypeDescriptor.GetComponentName(dragComponents[0]);
                            if ((componentName == null) || (componentName.Length == 0))
                            {
                                componentName = dragComponents[0].GetType().Name;
                            }
                            str = System.Design.SR.GetString(flag ? "BehaviorServiceCopyControl" : "BehaviorServiceMoveControl", new object[] { componentName });
                        }
                        else
                        {
                            str = System.Design.SR.GetString(flag ? "BehaviorServiceCopyControls" : "BehaviorServiceMoveControls", new object[] { dragComponents.Count });
                        }
                        DesignerTransaction transaction = host.CreateTransaction(str);
                        try
                        {
                            IComponentChangeService service = (IComponentChangeService) primarySelection.Site.GetService(typeof(IComponentChangeService));
                            if (service != null)
                            {
                                service.OnComponentChanging(component, TypeDescriptor.GetProperties(component)["DropDownItems"]);
                            }
                            if (flag)
                            {
                                if (primarySelection != null)
                                {
                                    index = dragComponents.IndexOf(primarySelection);
                                }
                                ToolStripKeyboardHandlingService service2 = (ToolStripKeyboardHandlingService) primarySelection.Site.GetService(typeof(ToolStripKeyboardHandlingService));
                                if (service2 != null)
                                {
                                    service2.CopyInProgress = true;
                                }
                                dragComponents = DesignerUtils.CopyDragObjects(dragComponents, primarySelection.Site) as ArrayList;
                                if (service2 != null)
                                {
                                    service2.CopyInProgress = false;
                                }
                                if (index != -1)
                                {
                                    primarySelection = dragComponents[index] as ToolStripItem;
                                }
                            }
                            if ((e.Effect == DragDropEffects.Move) || flag)
                            {
                                foreach (ToolStripItem item3 in dragComponents)
                                {
                                    currentParent.Items.Add(item3);
                                }
                                ToolStripDropDownItem item4 = primarySelection as ToolStripDropDownItem;
                                if (item4 != null)
                                {
                                    ToolStripMenuItemDesigner designer = host.GetDesigner(item4) as ToolStripMenuItemDesigner;
                                    if (designer != null)
                                    {
                                        designer.InitializeDropDown();
                                    }
                                }
                                this.menuItemDesigner.selSvc.SetSelectedComponents(new IComponent[] { primarySelection }, SelectionTypes.Click | SelectionTypes.Replace);
                            }
                            if (service != null)
                            {
                                service.OnComponentChanged(component, TypeDescriptor.GetProperties(component)["DropDownItems"], null, null);
                            }
                            if (flag && (service != null))
                            {
                                service.OnComponentChanging(component, TypeDescriptor.GetProperties(component)["DropDownItems"]);
                                service.OnComponentChanged(component, TypeDescriptor.GetProperties(component)["DropDownItems"], null, null);
                            }
                            if (component != null)
                            {
                                ToolStripMenuItemDesigner designer2 = host.GetDesigner(component) as ToolStripMenuItemDesigner;
                                if (designer2 != null)
                                {
                                    designer2.InitializeBodyGlyphsForItems(false, component);
                                    designer2.InitializeBodyGlyphsForItems(true, component);
                                }
                            }
                            BehaviorService service3 = (BehaviorService) primarySelection.Site.GetService(typeof(BehaviorService));
                            if (service3 != null)
                            {
                                service3.SyncSelection();
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
                            }
                            transaction = null;
                        }
                    }
                }
            }

            public override void OnDragEnter(Glyph g, DragEventArgs e)
            {
                if (e.Data is ToolStripItemDataObject)
                {
                    e.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
                }
                else
                {
                    base.OnDragEnter(g, e);
                }
            }

            public override void OnDragOver(Glyph g, DragEventArgs e)
            {
                if (e.Data is ToolStripItemDataObject)
                {
                    e.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
                }
                else
                {
                    base.OnDragOver(g, e);
                }
            }
        }

        internal class ToolStripDropDownGlyph : Glyph
        {
            private Rectangle _bounds;

            internal ToolStripDropDownGlyph(Rectangle bounds, System.Windows.Forms.Design.Behavior.Behavior b) : base(b)
            {
                this._bounds = bounds;
            }

            public override Cursor GetHitTest(Point p)
            {
                if (this._bounds.Contains(p))
                {
                    return Cursors.Default;
                }
                return null;
            }

            public override void Paint(PaintEventArgs pe)
            {
            }
        }
    }
}

