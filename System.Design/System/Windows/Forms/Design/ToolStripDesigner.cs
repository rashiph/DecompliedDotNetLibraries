namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripDesigner : ControlDesigner
    {
        private ToolStripActionList _actionLists;
        private bool _addingItem;
        internal static bool _autoAddNewItems = true;
        private uint _editingCollection;
        private DesignerTransaction _insertMenuItemTransaction;
        private System.Windows.Forms.ToolStrip _miniToolStrip;
        private DesignerTransaction _pendingTransaction;
        private ISelectionService _selectionSvc;
        private bool addingDummyItem;
        private Rectangle boundsToInvalidate = Rectangle.Empty;
        private bool cacheItems;
        private IComponentChangeService componentChangeSvc;
        private bool currentVisible = true;
        private bool disposed;
        private bool dontCloseOverflow;
        private Rectangle dragBoxFromMouseDown = Rectangle.Empty;
        internal static ToolStripItem dragItem = null;
        private ToolStripEditorManager editManager;
        private DesignerToolStripControlHost editorNode;
        internal static bool editTemplateNode = false;
        private bool fireSyncSelection;
        private const int GLYPHBORDER = 2;
        private IDesignerHost host;
        private int indexOfItemUnderMouseToDrag = -1;
        private ArrayList items;
        private ToolStripKeyboardHandlingService keyboardHandlingService;
        internal static Point LastCursorPosition = Point.Empty;
        private DesignerTransaction newItemTransaction;
        private bool parentNotVisible;
        internal static bool shiftState = false;
        private ToolStripTemplateNode tn;
        private IToolboxService toolboxService;
        private ToolStripAdornerWindowService toolStripAdornerWindowService;
        private ContextMenuStrip toolStripContextMenu;
        private bool toolStripSelected;
        private UndoEngine undoEngine;
        private bool undoingCalled;

        private void AddBodyGlyphsForOverflow()
        {
            foreach (ToolStripItem item in this.ToolStrip.Items)
            {
                if (!(item is DesignerToolStripControlHost) && (item.Placement == ToolStripItemPlacement.Overflow))
                {
                    this.AddItemBodyGlyph(item);
                }
            }
        }

        private void AddItemBodyGlyph(ToolStripItem item)
        {
            if (item != null)
            {
                ToolStripItemDesigner itemDesigner = (ToolStripItemDesigner) this.host.GetDesigner(item);
                if (itemDesigner != null)
                {
                    Rectangle glyphBounds = itemDesigner.GetGlyphBounds();
                    System.Windows.Forms.Design.Behavior.Behavior b = new ToolStripItemBehavior();
                    ToolStripItemGlyph glyph = new ToolStripItemGlyph(item, itemDesigner, glyphBounds, b);
                    itemDesigner.bodyGlyph = glyph;
                    if (this.toolStripAdornerWindowService != null)
                    {
                        this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Add(glyph);
                    }
                }
            }
        }

        private ToolStripItem AddNewItem(System.Type t)
        {
            this.NewItemTransaction = this.host.CreateTransaction(System.Design.SR.GetString("ToolStripCreatingNewItemTransaction"));
            IComponent component = null;
            try
            {
                this._addingItem = true;
                this.ToolStrip.SuspendLayout();
                ToolStripItemDesigner designer = null;
                try
                {
                    component = this.host.CreateComponent(t);
                    designer = this.host.GetDesigner(component) as ToolStripItemDesigner;
                    designer.InternalCreate = true;
                    if (designer != null)
                    {
                        designer.InitializeNewComponent(null);
                    }
                }
                finally
                {
                    if (designer != null)
                    {
                        designer.InternalCreate = false;
                    }
                    this.ToolStrip.ResumeLayout();
                }
            }
            catch (Exception exception)
            {
                if (this.NewItemTransaction != null)
                {
                    this.NewItemTransaction.Cancel();
                    this.NewItemTransaction = null;
                }
                CheckoutException exception2 = exception as CheckoutException;
                if ((exception2 == null) || !exception2.Equals(CheckoutException.Canceled))
                {
                    throw;
                }
            }
            finally
            {
                this._addingItem = false;
            }
            return (component as ToolStripItem);
        }

        internal ToolStripItem AddNewItem(System.Type t, string text, bool enterKeyPressed, bool tabKeyPressed)
        {
            DesignerTransaction transaction = this.host.CreateTransaction(System.Design.SR.GetString("ToolStripAddingItem", new object[] { t.Name }));
            ToolStripItem item = null;
            try
            {
                this._addingItem = true;
                this.ToolStrip.SuspendLayout();
                IComponent component = this.host.CreateComponent(t, NameFromText(text, t, base.Component.Site));
                ToolStripItemDesigner itemDesigner = this.host.GetDesigner(component) as ToolStripItemDesigner;
                try
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        itemDesigner.InternalCreate = true;
                    }
                    if (itemDesigner != null)
                    {
                        itemDesigner.InitializeNewComponent(null);
                    }
                }
                finally
                {
                    itemDesigner.InternalCreate = false;
                }
                item = component as ToolStripItem;
                if (item != null)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(item)["Text"];
                    if ((descriptor != null) && !string.IsNullOrEmpty(text))
                    {
                        descriptor.SetValue(item, text);
                    }
                    if (((item is ToolStripButton) || (item is ToolStripSplitButton)) || (item is ToolStripDropDownButton))
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
                        PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(item)["Image"];
                        if ((descriptor2 != null) && (image != null))
                        {
                            descriptor2.SetValue(item, image);
                        }
                        PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(item)["DisplayStyle"];
                        if (descriptor3 != null)
                        {
                            descriptor3.SetValue(item, ToolStripItemDisplayStyle.Image);
                        }
                        PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(item)["ImageTransparentColor"];
                        if (descriptor4 != null)
                        {
                            descriptor4.SetValue(item, Color.Magenta);
                        }
                    }
                }
                this.ToolStrip.ResumeLayout();
                if (!tabKeyPressed)
                {
                    if (enterKeyPressed)
                    {
                        if (!itemDesigner.SetSelection(enterKeyPressed) && (this.KeyboardHandlingService != null))
                        {
                            this.KeyboardHandlingService.SelectedDesignerControl = this.editorNode;
                            this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                        }
                    }
                    else
                    {
                        this.KeyboardHandlingService.SelectedDesignerControl = null;
                        this.SelectionService.SetSelectedComponents(new IComponent[] { item }, SelectionTypes.Replace);
                        this.editorNode.RefreshSelectionGlyph();
                    }
                }
                else if (this.keyboardHandlingService != null)
                {
                    this.KeyboardHandlingService.SelectedDesignerControl = this.editorNode;
                    this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                }
                if ((itemDesigner != null) && (item.Placement != ToolStripItemPlacement.Overflow))
                {
                    Rectangle glyphBounds = itemDesigner.GetGlyphBounds();
                    SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                    System.Windows.Forms.Design.Behavior.Behavior b = new ToolStripItemBehavior();
                    ToolStripItemGlyph glyph = new ToolStripItemGlyph(item, itemDesigner, glyphBounds, b);
                    service.BodyGlyphAdorner.Glyphs.Insert(0, glyph);
                    return item;
                }
                if ((itemDesigner != null) && (item.Placement == ToolStripItemPlacement.Overflow))
                {
                    this.RemoveBodyGlyphsForOverflow();
                    this.AddBodyGlyphsForOverflow();
                }
                return item;
            }
            catch (Exception exception2)
            {
                this.ToolStrip.ResumeLayout();
                if (this._pendingTransaction != null)
                {
                    this._pendingTransaction.Cancel();
                    this._pendingTransaction = null;
                }
                if (transaction != null)
                {
                    transaction.Cancel();
                    transaction = null;
                }
                CheckoutException exception3 = exception2 as CheckoutException;
                if ((exception3 != null) && (exception3 != CheckoutException.Canceled))
                {
                    throw;
                }
            }
            finally
            {
                if (this._pendingTransaction != null)
                {
                    this._pendingTransaction.Cancel();
                    this._pendingTransaction = null;
                    if (transaction != null)
                    {
                        transaction.Cancel();
                    }
                }
                else if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
                this._addingItem = false;
            }
            return item;
        }

        internal void AddNewTemplateNode(System.Windows.Forms.ToolStrip wb)
        {
            this.tn = new ToolStripTemplateNode(base.Component, System.Design.SR.GetString("ToolStripDesignerTemplateNodeEnterText"), null);
            this._miniToolStrip = this.tn.EditorToolStrip;
            int width = this.tn.EditorToolStrip.Width;
            this.editorNode = new DesignerToolStripControlHost(this.tn.EditorToolStrip);
            this.tn.ControlHost = this.editorNode;
            this.editorNode.Width = width;
            this.ToolStrip.Items.Add(this.editorNode);
            this.editorNode.Visible = false;
        }

        internal void CancelPendingMenuItemTransaction()
        {
            if (this._insertMenuItemTransaction != null)
            {
                this._insertMenuItemTransaction.Cancel();
            }
        }

        private bool CheckIfItemSelected()
        {
            bool flag = false;
            object primarySelection = this.SelectionService.PrimarySelection;
            if (primarySelection == null)
            {
                primarySelection = (IComponent) this.KeyboardHandlingService.SelectedDesignerControl;
            }
            ToolStripItem item = primarySelection as ToolStripItem;
            if (item != null)
            {
                if ((item.Placement == ToolStripItemPlacement.Overflow) && (item.Owner == this.ToolStrip))
                {
                    if (this.ToolStrip.CanOverflow && !this.ToolStrip.OverflowButton.DropDown.Visible)
                    {
                        this.ToolStrip.OverflowButton.ShowDropDown();
                    }
                    return true;
                }
                if (!this.ItemParentIsOverflow(item) && this.ToolStrip.OverflowButton.DropDown.Visible)
                {
                    this.ToolStrip.OverflowButton.HideDropDown();
                }
                if (item.Owner == this.ToolStrip)
                {
                    return true;
                }
                if (item is DesignerToolStripControlHost)
                {
                    if (item.IsOnDropDown && (item.Placement != ToolStripItemPlacement.Overflow))
                    {
                        ToolStripDropDown currentParent = (ToolStripDropDown) ((DesignerToolStripControlHost) primarySelection).GetCurrentParent();
                        if (currentParent == null)
                        {
                            return flag;
                        }
                        ToolStripItem ownerItem = currentParent.OwnerItem;
                        ToolStripDropDown firstDropDown = ((ToolStripMenuItemDesigner) this.host.GetDesigner(ownerItem)).GetFirstDropDown((ToolStripDropDownItem) ownerItem);
                        ToolStripItem item3 = (firstDropDown == null) ? ownerItem : firstDropDown.OwnerItem;
                        if ((item3 != null) && (item3.Owner == this.ToolStrip))
                        {
                            flag = true;
                        }
                    }
                    return flag;
                }
                if (item.IsOnDropDown && (item.Placement != ToolStripItemPlacement.Overflow))
                {
                    ToolStripItem component = ((ToolStripDropDown) item.Owner).OwnerItem;
                    if (component != null)
                    {
                        ToolStripMenuItemDesigner designer = (ToolStripMenuItemDesigner) this.host.GetDesigner(component);
                        ToolStripDropDown down3 = (designer == null) ? null : designer.GetFirstDropDown((ToolStripDropDownItem) component);
                        ToolStripItem item5 = (down3 == null) ? component : down3.OwnerItem;
                        if ((item5 != null) && (item5.Owner == this.ToolStrip))
                        {
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        internal bool Commit()
        {
            if ((this.tn != null) && this.tn.Active)
            {
                this.tn.Commit(false, false);
                this.editorNode.Width = this.tn.EditorToolStrip.Width;
            }
            else
            {
                ToolStripDropDownItem primarySelection = this.SelectionService.PrimarySelection as ToolStripDropDownItem;
                if (primarySelection != null)
                {
                    ToolStripMenuItemDesigner designer = this.host.GetDesigner(primarySelection) as ToolStripMenuItemDesigner;
                    if ((designer != null) && designer.IsEditorActive)
                    {
                        designer.Commit();
                        return true;
                    }
                }
                else if (this.KeyboardHandlingService != null)
                {
                    ToolStripItem selectedDesignerControl = this.KeyboardHandlingService.SelectedDesignerControl as ToolStripItem;
                    if ((selectedDesignerControl != null) && selectedDesignerControl.IsOnDropDown)
                    {
                        ToolStripDropDown currentParent = selectedDesignerControl.GetCurrentParent() as ToolStripDropDown;
                        if (currentParent != null)
                        {
                            ToolStripDropDownItem ownerItem = currentParent.OwnerItem as ToolStripDropDownItem;
                            if (ownerItem != null)
                            {
                                ToolStripMenuItemDesigner designer2 = this.host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner;
                                if ((designer2 != null) && designer2.IsEditorActive)
                                {
                                    designer2.Commit();
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        ToolStripItem component = this.SelectionService.PrimarySelection as ToolStripItem;
                        if (component != null)
                        {
                            ToolStripItemDesigner designer3 = (ToolStripItemDesigner) this.host.GetDesigner(component);
                            if ((designer3 != null) && designer3.IsEditorActive)
                            {
                                designer3.Editor.Commit(false, false);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void ComponentChangeSvc_ComponentAdded(object sender, ComponentEventArgs e)
        {
            if (this.toolStripSelected && (e.Component is System.Windows.Forms.ToolStrip))
            {
                this.toolStripSelected = false;
            }
            ToolStripItem component = e.Component as ToolStripItem;
            try
            {
                if (((component != null) && this._addingItem) && !component.IsOnDropDown)
                {
                    this._addingItem = false;
                    if (this.CacheItems)
                    {
                        this.items.Add(component);
                    }
                    else
                    {
                        int count = this.ToolStrip.Items.Count;
                        try
                        {
                            base.RaiseComponentChanging(TypeDescriptor.GetProperties(base.Component)["Items"]);
                            ToolStripItem primarySelection = this.SelectionService.PrimarySelection as ToolStripItem;
                            if (primarySelection != null)
                            {
                                if (primarySelection.Owner == this.ToolStrip)
                                {
                                    int index = this.ToolStrip.Items.IndexOf(primarySelection);
                                    this.ToolStrip.Items.Insert(index, component);
                                }
                            }
                            else if (count > 0)
                            {
                                this.ToolStrip.Items.Insert(count - 1, component);
                            }
                            else
                            {
                                this.ToolStrip.Items.Add(component);
                            }
                        }
                        finally
                        {
                            base.RaiseComponentChanged(TypeDescriptor.GetProperties(base.Component)["Items"], null, null);
                        }
                    }
                }
            }
            catch
            {
                if (this._pendingTransaction != null)
                {
                    this._pendingTransaction.Cancel();
                    this._pendingTransaction = null;
                    this._insertMenuItemTransaction = null;
                }
            }
            finally
            {
                if (this._pendingTransaction != null)
                {
                    this._pendingTransaction.Commit();
                    this._pendingTransaction = null;
                    this._insertMenuItemTransaction = null;
                }
            }
        }

        private void ComponentChangeSvc_ComponentAdding(object sender, ComponentEventArgs e)
        {
            if ((this.KeyboardHandlingService == null) || !this.KeyboardHandlingService.CopyInProgress)
            {
                object primarySelection = this.SelectionService.PrimarySelection;
                if ((primarySelection == null) && (this.keyboardHandlingService != null))
                {
                    primarySelection = this.KeyboardHandlingService.SelectedDesignerControl;
                }
                ToolStripItem item = primarySelection as ToolStripItem;
                if ((item == null) || (item.Owner == this.ToolStrip))
                {
                    ToolStripItem component = e.Component as ToolStripItem;
                    if ((((component == null) || (component.Owner == null)) || (component.Owner.Site != null)) && ((((this._insertMenuItemTransaction == null) && _autoAddNewItems) && ((component != null) && !this._addingItem)) && (this.IsToolStripOrItemSelected && !this.EditingCollection)))
                    {
                        this._addingItem = true;
                        if (this._pendingTransaction == null)
                        {
                            this._insertMenuItemTransaction = this._pendingTransaction = this.host.CreateTransaction(System.Design.SR.GetString("ToolStripDesignerTransactionAddingItem"));
                        }
                    }
                }
            }
        }

        private void ComponentChangeSvc_ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            ToolStripItem component = e.Component as ToolStripItem;
            if (((component != null) && (component.Owner == this.ToolStrip)) && ((e.Member != null) && (e.Member.Name == "Overflow")))
            {
                ToolStripItemOverflow oldValue = (ToolStripItemOverflow) e.OldValue;
                ToolStripItemOverflow newValue = (ToolStripItemOverflow) e.NewValue;
                if (((oldValue != ToolStripItemOverflow.Always) && (newValue == ToolStripItemOverflow.Always)) && (this.ToolStrip.CanOverflow && !this.ToolStrip.OverflowButton.DropDown.Visible))
                {
                    this.ToolStrip.OverflowButton.ShowDropDown();
                }
            }
        }

        private void ComponentChangeSvc_ComponentRemoved(object sender, ComponentEventArgs e)
        {
            if ((e.Component is ToolStripItem) && (((ToolStripItem) e.Component).Owner == base.Component))
            {
                ToolStripItem item = (ToolStripItem) e.Component;
                int index = this.ToolStrip.Items.IndexOf(item);
                try
                {
                    if (index != -1)
                    {
                        this.ToolStrip.Items.Remove(item);
                        base.RaiseComponentChanged(TypeDescriptor.GetProperties(base.Component)["Items"], null, null);
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
                if (this.ToolStrip.Items.Count > 1)
                {
                    index = Math.Min(this.ToolStrip.Items.Count - 1, index);
                    index = Math.Max(0, index);
                }
                else
                {
                    index = -1;
                }
                this.LayoutToolStrip();
                if (item.Placement == ToolStripItemPlacement.Overflow)
                {
                    this.RemoveBodyGlyphsForOverflow();
                    this.AddBodyGlyphsForOverflow();
                }
                if ((this.toolStripAdornerWindowService != null) && (this.boundsToInvalidate != Rectangle.Empty))
                {
                    this.toolStripAdornerWindowService.Invalidate(this.boundsToInvalidate);
                    base.BehaviorService.Invalidate(this.boundsToInvalidate);
                }
                if (this.KeyboardHandlingService.CutOrDeleteInProgress)
                {
                    IComponent component = (index == -1) ? ((IComponent) this.ToolStrip) : ((IComponent) this.ToolStrip.Items[index]);
                    if (component != null)
                    {
                        if (component is DesignerToolStripControlHost)
                        {
                            if (this.KeyboardHandlingService != null)
                            {
                                this.KeyboardHandlingService.SelectedDesignerControl = component;
                            }
                            this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                        }
                        else
                        {
                            this.SelectionService.SetSelectedComponents(new IComponent[] { component }, SelectionTypes.Replace);
                        }
                    }
                }
            }
        }

        private void ComponentChangeSvc_ComponentRemoving(object sender, ComponentEventArgs e)
        {
            if ((e.Component is ToolStripItem) && (((ToolStripItem) e.Component).Owner == base.Component))
            {
                try
                {
                    this._pendingTransaction = this.host.CreateTransaction(System.Design.SR.GetString("ToolStripDesignerTransactionRemovingItem"));
                    base.RaiseComponentChanging(TypeDescriptor.GetProperties(base.Component)["Items"]);
                    ToolStripDropDownItem component = e.Component as ToolStripDropDownItem;
                    if (component != null)
                    {
                        component.HideDropDown();
                        this.boundsToInvalidate = component.DropDown.Bounds;
                    }
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

        private void Control_HandleCreated(object sender, EventArgs e)
        {
            this.Control.HandleCreated -= new EventHandler(this.Control_HandleCreated);
            this.InitializeNewItemDropDown();
            this.ToolStrip.OverflowButton.DropDown.Closing += new ToolStripDropDownClosingEventHandler(this.OnOverflowDropDownClosing);
            this.ToolStrip.OverflowButton.DropDownOpening += new EventHandler(this.OnOverFlowDropDownOpening);
            this.ToolStrip.OverflowButton.DropDownOpened += new EventHandler(this.OnOverFlowDropDownOpened);
            this.ToolStrip.OverflowButton.DropDownClosed += new EventHandler(this.OnOverFlowDropDownClosed);
            this.ToolStrip.OverflowButton.DropDown.Resize += new EventHandler(this.OnOverflowDropDownResize);
            this.ToolStrip.OverflowButton.DropDown.Paint += new PaintEventHandler(this.OnOverFlowDropDownPaint);
            this.ToolStrip.Move += new EventHandler(this.OnToolStripMove);
            this.ToolStrip.VisibleChanged += new EventHandler(this.OnToolStripVisibleChanged);
            this.ToolStrip.ItemAdded += new ToolStripItemEventHandler(this.OnItemAdded);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.disposed = true;
                if (this.items != null)
                {
                    this.items = null;
                }
                if (this.undoEngine != null)
                {
                    this.undoEngine.Undoing -= new EventHandler(this.OnUndoing);
                    this.undoEngine.Undone -= new EventHandler(this.OnUndone);
                }
                if (this.componentChangeSvc != null)
                {
                    this.componentChangeSvc.ComponentRemoved -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoved);
                    this.componentChangeSvc.ComponentRemoving -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoving);
                    this.componentChangeSvc.ComponentAdded -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdded);
                    this.componentChangeSvc.ComponentAdding -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdding);
                    this.componentChangeSvc.ComponentChanged -= new ComponentChangedEventHandler(this.ComponentChangeSvc_ComponentChanged);
                }
                if (this._selectionSvc != null)
                {
                    this._selectionSvc.SelectionChanged -= new EventHandler(this.selSvc_SelectionChanged);
                    this._selectionSvc.SelectionChanging -= new EventHandler(this.selSvc_SelectionChanging);
                    this._selectionSvc = null;
                }
                base.EnableDragDrop(false);
                if (this.editManager != null)
                {
                    this.editManager.CloseManager();
                    this.editManager = null;
                }
                if (this.tn != null)
                {
                    this.tn.RollBack();
                    this.tn.CloseEditor();
                    this.tn = null;
                }
                if (this._miniToolStrip != null)
                {
                    this._miniToolStrip.Dispose();
                    this._miniToolStrip = null;
                }
                if (this.editorNode != null)
                {
                    this.editorNode.Dispose();
                    this.editorNode = null;
                }
                if (this.ToolStrip != null)
                {
                    this.ToolStrip.OverflowButton.DropDown.Closing -= new ToolStripDropDownClosingEventHandler(this.OnOverflowDropDownClosing);
                    this.ToolStrip.OverflowButton.DropDownOpening -= new EventHandler(this.OnOverFlowDropDownOpening);
                    this.ToolStrip.OverflowButton.DropDownOpened -= new EventHandler(this.OnOverFlowDropDownOpened);
                    this.ToolStrip.OverflowButton.DropDownClosed -= new EventHandler(this.OnOverFlowDropDownClosed);
                    this.ToolStrip.OverflowButton.DropDown.Resize -= new EventHandler(this.OnOverflowDropDownResize);
                    this.ToolStrip.OverflowButton.DropDown.Paint -= new PaintEventHandler(this.OnOverFlowDropDownPaint);
                    this.ToolStrip.Move -= new EventHandler(this.OnToolStripMove);
                    this.ToolStrip.VisibleChanged -= new EventHandler(this.OnToolStripVisibleChanged);
                    this.ToolStrip.ItemAdded -= new ToolStripItemEventHandler(this.OnItemAdded);
                    this.ToolStrip.Resize -= new EventHandler(this.ToolStrip_Resize);
                    this.ToolStrip.DockChanged -= new EventHandler(this.ToolStrip_Resize);
                    this.ToolStrip.LayoutCompleted -= new EventHandler(this.ToolStrip_LayoutCompleted);
                }
                if (this.toolStripContextMenu != null)
                {
                    this.toolStripContextMenu.Dispose();
                    this.toolStripContextMenu = null;
                }
                this.RemoveBodyGlyphsForOverflow();
                if (this.ToolStrip.OverflowButton.DropDown.Visible)
                {
                    this.ToolStrip.OverflowButton.HideDropDown();
                }
                if (this.toolStripAdornerWindowService != null)
                {
                    this.toolStripAdornerWindowService = null;
                }
            }
            base.Dispose(disposing);
        }

        public override void DoDefaultAction()
        {
            if (this.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly)
            {
                IComponent primarySelection = this.SelectionService.PrimarySelection as IComponent;
                if ((primarySelection == null) && (this.KeyboardHandlingService != null))
                {
                    primarySelection = (IComponent) this.KeyboardHandlingService.SelectedDesignerControl;
                }
                if ((primarySelection is ToolStripItem) && (this.host != null))
                {
                    IDesigner designer = this.host.GetDesigner(primarySelection);
                    if (designer != null)
                    {
                        designer.DoDefaultAction();
                        return;
                    }
                }
                base.DoDefaultAction();
            }
        }

        protected override ControlBodyGlyph GetControlGlyph(GlyphSelectionType selectionType)
        {
            if (!this.ToolStrip.IsHandleCreated)
            {
                return null;
            }
            SelectionManager manager = (SelectionManager) this.GetService(typeof(SelectionManager));
            if (((manager != null) && (this.ToolStrip != null)) && (this.CanAddItems && this.ToolStrip.Visible))
            {
                object primarySelection = this.SelectionService.PrimarySelection;
                System.Windows.Forms.Design.Behavior.Behavior b = new ToolStripItemBehavior();
                if (this.ToolStrip.Items.Count > 0)
                {
                    ToolStripItem[] array = new ToolStripItem[this.ToolStrip.Items.Count];
                    this.ToolStrip.Items.CopyTo(array, 0);
                    foreach (ToolStripItem item in array)
                    {
                        if (item != null)
                        {
                            ToolStripItemDesigner designer = (ToolStripItemDesigner) this.host.GetDesigner(item);
                            if (((item != primarySelection) && (designer != null)) && designer.IsEditorActive)
                            {
                                designer.Editor.Commit(false, false);
                            }
                        }
                    }
                }
                IMenuEditorService service = (IMenuEditorService) this.GetService(typeof(IMenuEditorService));
                if ((service == null) || ((service != null) && !service.IsActive()))
                {
                    foreach (ToolStripItem item2 in this.ToolStrip.Items)
                    {
                        if (!(item2 is DesignerToolStripControlHost) && (item2.Placement == ToolStripItemPlacement.Main))
                        {
                            ToolStripItemDesigner itemDesigner = (ToolStripItemDesigner) this.host.GetDesigner(item2);
                            if (itemDesigner != null)
                            {
                                bool flag2 = item2 == primarySelection;
                                if (flag2)
                                {
                                    ((ToolStripItemBehavior) b).dragBoxFromMouseDown = this.dragBoxFromMouseDown;
                                }
                                if (!flag2)
                                {
                                    item2.AutoSize = (itemDesigner != null) ? itemDesigner.AutoSize : true;
                                }
                                Rectangle glyphBounds = itemDesigner.GetGlyphBounds();
                                Control parent = this.ToolStrip.Parent;
                                Rectangle parentBounds = base.BehaviorService.ControlRectInAdornerWindow(parent);
                                if (IsGlyphTotallyVisible(glyphBounds, parentBounds) && item2.Visible)
                                {
                                    ToolStripItemGlyph glyph = new ToolStripItemGlyph(item2, itemDesigner, glyphBounds, b);
                                    itemDesigner.bodyGlyph = glyph;
                                    manager.BodyGlyphAdorner.Glyphs.Add(glyph);
                                }
                            }
                        }
                    }
                }
            }
            return base.GetControlGlyph(selectionType);
        }

        public override GlyphCollection GetGlyphs(GlyphSelectionType selType)
        {
            GlyphCollection glyphs = new GlyphCollection();
            foreach (object obj2 in this.SelectionService.GetSelectedComponents())
            {
                if (obj2 is System.Windows.Forms.ToolStrip)
                {
                    GlyphCollection glyphs2 = base.GetGlyphs(selType);
                    glyphs.AddRange(glyphs2);
                }
                else
                {
                    ToolStripItem component = obj2 as ToolStripItem;
                    if ((component != null) && component.Visible)
                    {
                        ToolStripItemDesigner designer = (ToolStripItemDesigner) this.host.GetDesigner(component);
                        if (designer != null)
                        {
                            designer.GetGlyphs(ref glyphs, this.StandardBehavior);
                        }
                    }
                }
            }
            if ((((this.SelectionRules & SelectionRules.Moveable) != SelectionRules.None) && (this.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly)) && (selType != GlyphSelectionType.NotSelected))
            {
                Point location = base.BehaviorService.ControlToAdornerWindow((Control) base.Component);
                Rectangle containerBounds = new Rectangle(location, ((Control) base.Component).Size);
                int glyphOffset = (int) (DesignerUtils.CONTAINERGRABHANDLESIZE * 0.5);
                if (containerBounds.Width < (2 * DesignerUtils.CONTAINERGRABHANDLESIZE))
                {
                    glyphOffset = -1 * glyphOffset;
                }
                ContainerSelectorBehavior behavior = new ContainerSelectorBehavior(this.ToolStrip, base.Component.Site, true);
                ContainerSelectorGlyph glyph = new ContainerSelectorGlyph(containerBounds, DesignerUtils.CONTAINERGRABHANDLESIZE, glyphOffset, behavior);
                glyphs.Insert(0, glyph);
            }
            return glyphs;
        }

        protected override bool GetHitTest(Point point)
        {
            point = this.Control.PointToClient(point);
            return ((((this._miniToolStrip != null) && this._miniToolStrip.Visible) && this.AddItemRect.Contains(point)) || (this.OverFlowButtonRect.Contains(point) || base.GetHitTest(point)));
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            base.AutoResizeHandles = true;
            this.host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (this.host != null)
            {
                this.componentChangeSvc = (IComponentChangeService) this.host.GetService(typeof(IComponentChangeService));
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
            this.editManager = new ToolStripEditorManager(component);
            if (this.Control.IsHandleCreated)
            {
                this.InitializeNewItemDropDown();
            }
            else
            {
                this.Control.HandleCreated += new EventHandler(this.Control_HandleCreated);
            }
            if (this.componentChangeSvc != null)
            {
                this.componentChangeSvc.ComponentRemoved += new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoved);
                this.componentChangeSvc.ComponentRemoving += new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoving);
                this.componentChangeSvc.ComponentAdded += new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdded);
                this.componentChangeSvc.ComponentAdding += new ComponentEventHandler(this.ComponentChangeSvc_ComponentAdding);
                this.componentChangeSvc.ComponentChanged += new ComponentChangedEventHandler(this.ComponentChangeSvc_ComponentChanged);
            }
            this.toolStripAdornerWindowService = (ToolStripAdornerWindowService) this.GetService(typeof(ToolStripAdornerWindowService));
            this.SelectionService.SelectionChanging += new EventHandler(this.selSvc_SelectionChanging);
            this.SelectionService.SelectionChanged += new EventHandler(this.selSvc_SelectionChanged);
            this.ToolStrip.Resize += new EventHandler(this.ToolStrip_Resize);
            this.ToolStrip.DockChanged += new EventHandler(this.ToolStrip_Resize);
            this.ToolStrip.LayoutCompleted += new EventHandler(this.ToolStrip_LayoutCompleted);
            this.ToolStrip.OverflowButton.DropDown.TopLevel = false;
            if (this.CanAddItems)
            {
                new EditorServiceContext(this, TypeDescriptor.GetProperties(base.Component)["Items"], System.Design.SR.GetString("ToolStripItemCollectionEditorVerb"));
                this.keyboardHandlingService = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
                if (this.keyboardHandlingService == null)
                {
                    this.keyboardHandlingService = new ToolStripKeyboardHandlingService(base.Component.Site);
                }
                if (((ISupportInSituService) this.GetService(typeof(ISupportInSituService))) == null)
                {
                    ISupportInSituService service = new ToolStripInSituService(base.Component.Site);
                }
            }
            this.toolStripSelected = true;
            if (this.keyboardHandlingService != null)
            {
                this.KeyboardHandlingService.SelectedDesignerControl = null;
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            Control control = defaultValues["Parent"] as Control;
            Form rootComponent = this.host.RootComponent as Form;
            MainMenu menu = null;
            FormDocumentDesigner designer = null;
            if (rootComponent != null)
            {
                designer = this.host.GetDesigner(rootComponent) as FormDocumentDesigner;
                if ((designer != null) && (designer.Menu != null))
                {
                    menu = designer.Menu;
                    designer.Menu = null;
                }
            }
            ToolStripPanel component = control as ToolStripPanel;
            if ((component == null) && (control is ToolStripContentPanel))
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this.ToolStrip)["Dock"];
                if (descriptor != null)
                {
                    descriptor.SetValue(this.ToolStrip, DockStyle.None);
                }
            }
            if ((component == null) || (this.ToolStrip is MenuStrip))
            {
                base.InitializeNewComponent(defaultValues);
            }
            if (designer != null)
            {
                if (menu != null)
                {
                    designer.Menu = menu;
                }
                if (this.ToolStrip is MenuStrip)
                {
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(rootComponent)["MainMenuStrip"];
                    if ((descriptor2 != null) && (descriptor2.GetValue(rootComponent) == null))
                    {
                        descriptor2.SetValue(rootComponent, this.ToolStrip as MenuStrip);
                    }
                }
            }
            if (component != null)
            {
                if (!(this.ToolStrip is MenuStrip))
                {
                    PropertyDescriptor member = TypeDescriptor.GetProperties(component)["Controls"];
                    if (this.componentChangeSvc != null)
                    {
                        this.componentChangeSvc.OnComponentChanging(component, member);
                    }
                    component.Join(this.ToolStrip, component.Rows.Length);
                    if (this.componentChangeSvc != null)
                    {
                        this.componentChangeSvc.OnComponentChanged(component, member, component.Controls, component.Controls);
                    }
                    PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(this.ToolStrip)["Location"];
                    if (this.componentChangeSvc != null)
                    {
                        this.componentChangeSvc.OnComponentChanging(this.ToolStrip, descriptor4);
                        this.componentChangeSvc.OnComponentChanged(this.ToolStrip, descriptor4, null, null);
                    }
                }
            }
            else if (control != null)
            {
                if (this.ToolStrip is MenuStrip)
                {
                    int newIndex = -1;
                    foreach (Control control2 in control.Controls)
                    {
                        if ((control2 is System.Windows.Forms.ToolStrip) && (control2 != this.ToolStrip))
                        {
                            newIndex = control.Controls.IndexOf(control2);
                        }
                    }
                    if (newIndex == -1)
                    {
                        newIndex = control.Controls.Count - 1;
                    }
                    control.Controls.SetChildIndex(this.ToolStrip, newIndex);
                }
                else
                {
                    int index = -1;
                    foreach (Control control3 in control.Controls)
                    {
                        MenuStrip strip = control3 as MenuStrip;
                        if ((control3 is System.Windows.Forms.ToolStrip) && (strip == null))
                        {
                            return;
                        }
                        if (strip != null)
                        {
                            index = control.Controls.IndexOf(control3);
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        index = control.Controls.Count;
                    }
                    control.Controls.SetChildIndex(this.ToolStrip, index - 1);
                }
            }
        }

        private void InitializeNewItemDropDown()
        {
            if (this.CanAddItems && this.SupportEditing)
            {
                System.Windows.Forms.ToolStrip component = (System.Windows.Forms.ToolStrip) base.Component;
                this.AddNewTemplateNode(component);
                this.selSvc_SelectionChanged(null, EventArgs.Empty);
            }
        }

        internal static bool IsGlyphTotallyVisible(Rectangle itemBounds, Rectangle parentBounds)
        {
            return parentBounds.Contains(itemBounds);
        }

        private bool ItemParentIsOverflow(ToolStripItem item)
        {
            ToolStripDropDown owner = item.Owner as ToolStripDropDown;
            if (owner != null)
            {
                while ((owner != null) && !(owner is ToolStripOverflow))
                {
                    if (owner.OwnerItem != null)
                    {
                        owner = owner.OwnerItem.GetCurrentParent() as ToolStripDropDown;
                    }
                    else
                    {
                        owner = null;
                    }
                }
            }
            return (owner is ToolStripOverflow);
        }

        private void LayoutToolStrip()
        {
            if (!this.disposed)
            {
                this.ToolStrip.PerformLayout();
            }
        }

        internal static string NameFromText(string text, System.Type componentType, IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                string str = null;
                INameCreationService service = serviceProvider.GetService(typeof(INameCreationService)) as INameCreationService;
                IContainer container = (IContainer) serviceProvider.GetService(typeof(IContainer));
                if ((service != null) && (container != null))
                {
                    str = service.CreateName(container, componentType);
                    if (((text == null) || (text.Length == 0)) || (text == "-"))
                    {
                        return str;
                    }
                    string name = componentType.Name;
                    StringBuilder builder = new StringBuilder(text.Length + name.Length);
                    bool flag = false;
                    for (int i = 0; i < text.Length; i++)
                    {
                        char c = text[i];
                        if (flag)
                        {
                            if (char.IsLower(c))
                            {
                                c = char.ToUpper(c, CultureInfo.CurrentCulture);
                            }
                            flag = false;
                        }
                        if (char.IsLetterOrDigit(c))
                        {
                            if (builder.Length == 0)
                            {
                                if (char.IsDigit(c))
                                {
                                    continue;
                                }
                                if (char.IsLower(c) != char.IsLower(str[0]))
                                {
                                    if (char.IsLower(c))
                                    {
                                        c = char.ToUpper(c, CultureInfo.CurrentCulture);
                                    }
                                    else
                                    {
                                        c = char.ToLower(c, CultureInfo.CurrentCulture);
                                    }
                                }
                            }
                            builder.Append(c);
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            flag = true;
                        }
                    }
                    if (builder.Length == 0)
                    {
                        return str;
                    }
                    builder.Append(name);
                    string str3 = builder.ToString();
                    if (container.Components[str3] == null)
                    {
                        if (!service.IsValidName(str3))
                        {
                            return str;
                        }
                        return str3;
                    }
                    string str4 = str3;
                    for (int j = 1; !service.IsValidName(str4) || (container.Components[str4] != null); j++)
                    {
                        str4 = str3 + j.ToString(CultureInfo.InvariantCulture);
                    }
                    return str4;
                }
            }
            return null;
        }

        internal static string NameFromText(string text, System.Type componentType, IServiceProvider serviceProvider, bool adjustCapitalization)
        {
            string str = NameFromText(text, componentType, serviceProvider);
            if (adjustCapitalization)
            {
                string str2 = NameFromText(null, typeof(ToolStripMenuItem), serviceProvider);
                if (!string.IsNullOrEmpty(str2) && char.IsUpper(str2[0]))
                {
                    str = char.ToUpper(str[0], CultureInfo.InvariantCulture) + str.Substring(1);
                }
            }
            return str;
        }

        protected override void OnContextMenu(int x, int y)
        {
            Component primarySelection = this.SelectionService.PrimarySelection as Component;
            if (primarySelection is System.Windows.Forms.ToolStrip)
            {
                this.DesignerContextMenu.Show(x, y);
            }
        }

        protected override void OnDragDrop(DragEventArgs de)
        {
            base.OnDragDrop(de);
            bool flag = false;
            System.Windows.Forms.ToolStrip toolStrip = this.ToolStrip;
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT(de.X, de.Y);
            System.Design.NativeMethods.MapWindowPoints(IntPtr.Zero, toolStrip.Handle, pt, 1);
            Point point2 = new Point(pt.x, pt.y);
            if (this.ToolStrip.Orientation == Orientation.Horizontal)
            {
                if (this.ToolStrip.RightToLeft == RightToLeft.Yes)
                {
                    if (point2.X >= toolStrip.Items[0].Bounds.X)
                    {
                        flag = true;
                    }
                }
                else if (point2.X <= toolStrip.Items[0].Bounds.X)
                {
                    flag = true;
                }
            }
            else if (point2.Y <= toolStrip.Items[0].Bounds.Y)
            {
                flag = true;
            }
            ToolStripItemDataObject data = de.Data as ToolStripItemDataObject;
            if ((data != null) && (data.Owner == toolStrip))
            {
                string str;
                ArrayList dragComponents = data.DragComponents;
                ToolStripItem primarySelection = data.PrimarySelection;
                int index = -1;
                bool flag2 = de.Effect == DragDropEffects.Copy;
                if (dragComponents.Count == 1)
                {
                    string componentName = TypeDescriptor.GetComponentName(dragComponents[0]);
                    if ((componentName == null) || (componentName.Length == 0))
                    {
                        componentName = dragComponents[0].GetType().Name;
                    }
                    str = System.Design.SR.GetString(flag2 ? "BehaviorServiceCopyControl" : "BehaviorServiceMoveControl", new object[] { componentName });
                }
                else
                {
                    str = System.Design.SR.GetString(flag2 ? "BehaviorServiceCopyControls" : "BehaviorServiceMoveControls", new object[] { dragComponents.Count });
                }
                DesignerTransaction transaction = this.host.CreateTransaction(str);
                try
                {
                    IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.OnComponentChanging(toolStrip, TypeDescriptor.GetProperties(toolStrip)["Items"]);
                    }
                    if (flag2)
                    {
                        if (primarySelection != null)
                        {
                            index = dragComponents.IndexOf(primarySelection);
                        }
                        if (this.KeyboardHandlingService != null)
                        {
                            this.KeyboardHandlingService.CopyInProgress = true;
                        }
                        dragComponents = DesignerUtils.CopyDragObjects(dragComponents, base.Component.Site) as ArrayList;
                        if (this.KeyboardHandlingService != null)
                        {
                            this.KeyboardHandlingService.CopyInProgress = false;
                        }
                        if (index != -1)
                        {
                            primarySelection = dragComponents[index] as ToolStripItem;
                        }
                    }
                    if ((de.Effect == DragDropEffects.Move) || flag2)
                    {
                        for (int i = 0; i < dragComponents.Count; i++)
                        {
                            if (flag)
                            {
                                toolStrip.Items.Insert(0, dragComponents[i] as ToolStripItem);
                            }
                            else
                            {
                                toolStrip.Items.Add(dragComponents[i] as ToolStripItem);
                            }
                        }
                        ToolStripDropDownItem component = primarySelection as ToolStripDropDownItem;
                        if (component != null)
                        {
                            ToolStripMenuItemDesigner designer = this.host.GetDesigner(component) as ToolStripMenuItemDesigner;
                            if (designer != null)
                            {
                                designer.InitializeDropDown();
                            }
                        }
                        this.SelectionService.SetSelectedComponents(new IComponent[] { primarySelection }, SelectionTypes.Click | SelectionTypes.Replace);
                    }
                    if (service != null)
                    {
                        service.OnComponentChanged(toolStrip, TypeDescriptor.GetProperties(toolStrip)["Items"], null, null);
                    }
                    if (flag2 && (service != null))
                    {
                        service.OnComponentChanging(toolStrip, TypeDescriptor.GetProperties(toolStrip)["Items"]);
                        service.OnComponentChanged(toolStrip, TypeDescriptor.GetProperties(toolStrip)["Items"], null, null);
                    }
                    base.BehaviorService.SyncSelection();
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
        }

        protected override void OnDragEnter(DragEventArgs de)
        {
            base.OnDragEnter(de);
            this.SetDragDropEffects(de);
        }

        protected override void OnDragOver(DragEventArgs de)
        {
            base.OnDragOver(de);
            this.SetDragDropEffects(de);
        }

        private void OnItemAdded(object sender, ToolStripItemEventArgs e)
        {
            if ((this.editorNode != null) && (e.Item != this.editorNode))
            {
                int index = this.ToolStrip.Items.IndexOf(this.editorNode);
                if ((index == -1) || (index != (this.ToolStrip.Items.Count - 1)))
                {
                    this.ToolStrip.ItemAdded -= new ToolStripItemEventHandler(this.OnItemAdded);
                    this.ToolStrip.SuspendLayout();
                    this.ToolStrip.Items.Add(this.editorNode);
                    this.ToolStrip.ResumeLayout();
                    this.ToolStrip.ItemAdded += new ToolStripItemEventHandler(this.OnItemAdded);
                }
            }
            this.LayoutToolStrip();
        }

        protected override void OnMouseDragMove(int x, int y)
        {
            if (!this.SelectionService.GetComponentSelected(this.ToolStrip))
            {
                base.OnMouseDragMove(x, y);
            }
        }

        private void OnOverFlowDropDownClosed(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            if ((this.toolStripAdornerWindowService != null) && (item != null))
            {
                this.toolStripAdornerWindowService.Invalidate(item.DropDown.Bounds);
                this.RemoveBodyGlyphsForOverflow();
            }
            ToolStripItem primarySelection = this.SelectionService.PrimarySelection as ToolStripItem;
            if ((primarySelection != null) && primarySelection.IsOnOverflow)
            {
                ToolStripItem nextItem = this.ToolStrip.GetNextItem(this.ToolStrip.OverflowButton, ArrowDirection.Left);
                if (nextItem != null)
                {
                    this.SelectionService.SetSelectedComponents(new IComponent[] { nextItem }, SelectionTypes.Replace);
                }
            }
        }

        private void OnOverflowDropDownClosing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
        }

        private void OnOverFlowDropDownOpened(object sender, EventArgs e)
        {
            if (this.editorNode != null)
            {
                this.editorNode.Control.Visible = true;
                this.editorNode.Visible = true;
            }
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            if (item != null)
            {
                this.RemoveBodyGlyphsForOverflow();
                this.AddBodyGlyphsForOverflow();
            }
            ToolStripItem primarySelection = this.SelectionService.PrimarySelection as ToolStripItem;
            if ((primarySelection == null) || ((primarySelection != null) && !primarySelection.IsOnOverflow))
            {
                ToolStripItem nextItem = item.DropDown.GetNextItem(null, ArrowDirection.Down);
                if (nextItem != null)
                {
                    this.SelectionService.SetSelectedComponents(new IComponent[] { nextItem }, SelectionTypes.Replace);
                    base.BehaviorService.Invalidate(base.BehaviorService.ControlRectInAdornerWindow(this.ToolStrip));
                }
            }
        }

        private void OnOverFlowDropDownOpening(object sender, EventArgs e)
        {
            ToolStripDropDownItem item = sender as ToolStripDropDownItem;
            if (item.DropDown.TopLevel)
            {
                item.DropDown.TopLevel = false;
            }
            if (this.toolStripAdornerWindowService != null)
            {
                this.ToolStrip.SuspendLayout();
                item.DropDown.Parent = this.toolStripAdornerWindowService.ToolStripAdornerWindowControl;
                this.ToolStrip.ResumeLayout();
            }
        }

        private void OnOverFlowDropDownPaint(object sender, PaintEventArgs e)
        {
            foreach (ToolStripItem item in this.ToolStrip.Items)
            {
                if ((item.Visible && item.IsOnOverflow) && this.SelectionService.GetComponentSelected(item))
                {
                    ToolStripItemDesigner designer = this.host.GetDesigner(item) as ToolStripItemDesigner;
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
            }
        }

        private void OnOverflowDropDownResize(object sender, EventArgs e)
        {
            ToolStripDropDown down = sender as ToolStripDropDown;
            if (down.Visible)
            {
                this.RemoveBodyGlyphsForOverflow();
                this.AddBodyGlyphsForOverflow();
            }
            if ((this.toolStripAdornerWindowService != null) && (down != null))
            {
                this.toolStripAdornerWindowService.Invalidate();
            }
        }

        protected override void OnSetCursor()
        {
            if (this.toolboxService == null)
            {
                this.toolboxService = (IToolboxService) this.GetService(typeof(IToolboxService));
            }
            if (((this.toolboxService == null) || !this.toolboxService.SetCursor()) || this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void OnToolStripMove(object sender, EventArgs e)
        {
            if (this.SelectionService.GetComponentSelected(this.ToolStrip))
            {
                base.BehaviorService.SyncSelection();
            }
        }

        private void OnToolStripVisibleChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolStrip strip = sender as System.Windows.Forms.ToolStrip;
            if ((strip != null) && !strip.Visible)
            {
                SelectionManager service = (SelectionManager) this.GetService(typeof(SelectionManager));
                Glyph[] array = new Glyph[service.BodyGlyphAdorner.Glyphs.Count];
                service.BodyGlyphAdorner.Glyphs.CopyTo(array, 0);
                foreach (Glyph glyph in array)
                {
                    if (glyph is ToolStripItemGlyph)
                    {
                        service.BodyGlyphAdorner.Glyphs.Remove(glyph);
                    }
                }
            }
        }

        private void OnUndoing(object source, EventArgs e)
        {
            if (this.CheckIfItemSelected() || this.SelectionService.GetComponentSelected(this.ToolStrip))
            {
                this.undoingCalled = true;
                this.ToolStrip.SuspendLayout();
            }
        }

        private void OnUndone(object source, EventArgs e)
        {
            if ((this.editorNode != null) && (this.ToolStrip.Items.IndexOf(this.editorNode) == -1))
            {
                this.ToolStrip.Items.Add(this.editorNode);
            }
            if (this.undoingCalled)
            {
                this.ToolStrip.ResumeLayout(true);
                this.ToolStrip.PerformLayout();
                ToolStripDropDownItem primarySelection = this.SelectionService.PrimarySelection as ToolStripDropDownItem;
                if (primarySelection != null)
                {
                    ToolStripMenuItemDesigner designer = this.host.GetDesigner(primarySelection) as ToolStripMenuItemDesigner;
                    if (designer != null)
                    {
                        designer.InitializeBodyGlyphsForItems(false, primarySelection);
                        designer.InitializeBodyGlyphsForItems(true, primarySelection);
                    }
                }
                this.undoingCalled = false;
            }
            base.BehaviorService.SyncSelection();
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "Visible", "AllowDrop", "AllowItemReorder" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(ToolStripDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private void RemoveBodyGlyphsForOverflow()
        {
            foreach (ToolStripItem item in this.ToolStrip.Items)
            {
                if (!(item is DesignerToolStripControlHost) && (item.Placement == ToolStripItemPlacement.Overflow))
                {
                    ToolStripItemDesigner designer = (ToolStripItemDesigner) this.host.GetDesigner(item);
                    if (designer != null)
                    {
                        ControlBodyGlyph bodyGlyph = designer.bodyGlyph;
                        if (((bodyGlyph != null) && (this.toolStripAdornerWindowService != null)) && this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Contains(bodyGlyph))
                        {
                            this.toolStripAdornerWindowService.DropDownAdorner.Glyphs.Remove(bodyGlyph);
                        }
                    }
                }
            }
        }

        private void ResetVisible()
        {
            this.Visible = true;
        }

        internal void RollBack()
        {
            if (this.tn != null)
            {
                this.tn.RollBack();
                this.editorNode.Width = this.tn.EditorToolStrip.Width;
            }
        }

        private void selSvc_SelectionChanged(object sender, EventArgs e)
        {
            if ((this._miniToolStrip != null) && (this.host != null))
            {
                bool flag2 = this.CheckIfItemSelected();
                if (flag2 || this.SelectionService.GetComponentSelected(this.ToolStrip))
                {
                    if ((this.SelectionService.GetComponentSelected(this.ToolStrip) && !this.DontCloseOverflow) && this.ToolStrip.OverflowButton.DropDown.Visible)
                    {
                        this.ToolStrip.OverflowButton.HideDropDown();
                    }
                    this.ShowHideToolStripItems(true);
                    this.currentVisible = this.Control.Visible && this.currentVisible;
                    if (!this.currentVisible)
                    {
                        this.Control.Visible = true;
                        if ((this.ToolStrip.Parent is ToolStripPanel) && !this.ToolStrip.Parent.Visible)
                        {
                            this.parentNotVisible = true;
                            this.ToolStrip.Parent.Visible = true;
                        }
                        base.BehaviorService.SyncSelection();
                    }
                    if ((this.editorNode != null) && ((this.SelectionService.PrimarySelection == this.ToolStrip) || flag2))
                    {
                        bool fireSyncSelection = this.FireSyncSelection;
                        ToolStripPanel parent = this.ToolStrip.Parent as ToolStripPanel;
                        try
                        {
                            if (parent != null)
                            {
                                parent.LocationChanged += new EventHandler(this.OnToolStripMove);
                            }
                            this.FireSyncSelection = true;
                            this.editorNode.Visible = true;
                        }
                        finally
                        {
                            this.FireSyncSelection = fireSyncSelection;
                            if (parent != null)
                            {
                                parent.LocationChanged -= new EventHandler(this.OnToolStripMove);
                            }
                        }
                    }
                    if (!(this.SelectionService.PrimarySelection is ToolStripItem) && (this.KeyboardHandlingService != null))
                    {
                        ToolStripItem selectedDesignerControl = this.KeyboardHandlingService.SelectedDesignerControl as ToolStripItem;
                    }
                    this.toolStripSelected = true;
                }
            }
        }

        private void selSvc_SelectionChanging(object sender, EventArgs e)
        {
            if ((this.toolStripSelected && (this.tn != null)) && this.tn.Active)
            {
                this.tn.Commit(false, false);
            }
            if (!this.CheckIfItemSelected() && !this.SelectionService.GetComponentSelected(this.ToolStrip))
            {
                this.ToolStrip.Visible = this.currentVisible;
                if (!this.currentVisible && this.parentNotVisible)
                {
                    this.ToolStrip.Parent.Visible = this.currentVisible;
                    this.parentNotVisible = false;
                }
                if (this.ToolStrip.OverflowButton.DropDown.Visible)
                {
                    this.ToolStrip.OverflowButton.HideDropDown();
                }
                if (this.editorNode != null)
                {
                    this.editorNode.Visible = false;
                }
                this.ShowHideToolStripItems(false);
                this.toolStripSelected = false;
            }
        }

        private void SetDragDropEffects(DragEventArgs de)
        {
            ToolStripItemDataObject data = de.Data as ToolStripItemDataObject;
            if (data != null)
            {
                if (data.Owner != this.ToolStrip)
                {
                    de.Effect = DragDropEffects.None;
                }
                else
                {
                    de.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
                }
            }
        }

        private bool ShouldSerializeAllowDrop()
        {
            return (bool) base.ShadowProperties["AllowDrop"];
        }

        private bool ShouldSerializeAllowItemReorder()
        {
            return (bool) base.ShadowProperties["AllowItemReorder"];
        }

        private bool ShouldSerializeVisible()
        {
            return !this.Visible;
        }

        internal void ShowEditNode(bool clicked)
        {
            ToolStripItem component = null;
            if (this.ToolStrip is MenuStrip)
            {
                if (this.KeyboardHandlingService != null)
                {
                    this.KeyboardHandlingService.ResetActiveTemplateNodeSelectionState();
                }
                try
                {
                    component = this.AddNewItem(typeof(ToolStripMenuItem));
                    if (component != null)
                    {
                        ToolStripItemDesigner designer = this.host.GetDesigner(component) as ToolStripItemDesigner;
                        if (designer != null)
                        {
                            designer.dummyItemAdded = true;
                            ((ToolStripMenuItemDesigner) designer).InitializeDropDown();
                            try
                            {
                                this.addingDummyItem = true;
                                designer.ShowEditNode(clicked);
                            }
                            finally
                            {
                                this.addingDummyItem = false;
                            }
                        }
                    }
                }
                catch (InvalidOperationException exception)
                {
                    ((IUIService) this.GetService(typeof(IUIService))).ShowError(exception.Message);
                    if (this.KeyboardHandlingService != null)
                    {
                        this.KeyboardHandlingService.ResetActiveTemplateNodeSelectionState();
                    }
                }
            }
        }

        private void ShowHideToolStripItems(bool toolStripSelected)
        {
            foreach (ToolStripItem item in this.ToolStrip.Items)
            {
                if (!(item is DesignerToolStripControlHost))
                {
                    ToolStripItemDesigner designer = (ToolStripItemDesigner) this.host.GetDesigner(item);
                    if (designer != null)
                    {
                        designer.SetItemVisible(toolStripSelected, this);
                    }
                }
            }
            if (this.FireSyncSelection)
            {
                base.BehaviorService.SyncSelection();
                this.FireSyncSelection = false;
            }
        }

        private void ToolStrip_LayoutCompleted(object sender, EventArgs e)
        {
            if (this.FireSyncSelection)
            {
                base.BehaviorService.SyncSelection();
            }
        }

        private void ToolStrip_Resize(object sender, EventArgs e)
        {
            if ((!this.addingDummyItem && !this.disposed) && (this.CheckIfItemSelected() || this.SelectionService.GetComponentSelected(this.ToolStrip)))
            {
                if ((this._miniToolStrip != null) && this._miniToolStrip.Visible)
                {
                    this.LayoutToolStrip();
                }
                base.BehaviorService.SyncSelection();
            }
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x7b:
                {
                    int x = System.Design.NativeMethods.Util.SignedLOWORD(m.LParam);
                    int y = System.Design.NativeMethods.Util.SignedHIWORD(m.LParam);
                    if (!this.GetHitTest(new Point(x, y)))
                    {
                        base.WndProc(ref m);
                    }
                    return;
                }
                case 0x201:
                case 0x204:
                    this.Commit();
                    base.WndProc(ref m);
                    return;
            }
            base.WndProc(ref m);
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                if (this._actionLists == null)
                {
                    this._actionLists = new ToolStripActionList(this);
                }
                lists.Add(this._actionLists);
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

        private Rectangle AddItemRect
        {
            get
            {
                Rectangle rectangle = new Rectangle();
                if (this._miniToolStrip == null)
                {
                    return rectangle;
                }
                return this._miniToolStrip.Bounds;
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
                if (value && this.AllowItemReorder)
                {
                    throw new ArgumentException(System.Design.SR.GetString("ToolStripAllowItemReorderAndAllowDropCannotBeSetToTrue"));
                }
                base.ShadowProperties["AllowDrop"] = value;
            }
        }

        private bool AllowItemReorder
        {
            get
            {
                return (bool) base.ShadowProperties["AllowItemReorder"];
            }
            set
            {
                if (value && this.AllowDrop)
                {
                    throw new ArgumentException(System.Design.SR.GetString("ToolStripAllowItemReorderAndAllowDropCannotBeSetToTrue"));
                }
                base.ShadowProperties["AllowItemReorder"] = value;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                ArrayList list = new ArrayList();
                foreach (ToolStripItem item in this.ToolStrip.Items)
                {
                    if (!(item is DesignerToolStripControlHost))
                    {
                        list.Add(item);
                    }
                }
                return list;
            }
        }

        public bool CacheItems
        {
            get
            {
                return this.cacheItems;
            }
            set
            {
                this.cacheItems = value;
            }
        }

        private bool CanAddItems
        {
            get
            {
                System.ComponentModel.InheritanceAttribute attribute = (System.ComponentModel.InheritanceAttribute) TypeDescriptor.GetAttributes(this.ToolStrip)[typeof(System.ComponentModel.InheritanceAttribute)];
                if ((attribute != null) && (attribute.InheritanceLevel != InheritanceLevel.NotInherited))
                {
                    return false;
                }
                return true;
            }
        }

        internal override bool ControlSupportsSnaplines
        {
            get
            {
                return !(this.ToolStrip.Parent is ToolStripPanel);
            }
        }

        private ContextMenuStrip DesignerContextMenu
        {
            get
            {
                if (this.toolStripContextMenu == null)
                {
                    this.toolStripContextMenu = new BaseContextMenuStrip(this.ToolStrip.Site, this.ToolStrip);
                    this.toolStripContextMenu.Text = "CustomContextMenu";
                }
                return this.toolStripContextMenu;
            }
        }

        public bool DontCloseOverflow
        {
            get
            {
                return this.dontCloseOverflow;
            }
            set
            {
                this.dontCloseOverflow = value;
            }
        }

        public Rectangle DragBoxFromMouseDown
        {
            get
            {
                return this.dragBoxFromMouseDown;
            }
            set
            {
                this.dragBoxFromMouseDown = value;
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

        public ToolStripEditorManager EditManager
        {
            get
            {
                return this.editManager;
            }
        }

        internal ToolStripTemplateNode Editor
        {
            get
            {
                return this.tn;
            }
        }

        public DesignerToolStripControlHost EditorNode
        {
            get
            {
                return this.editorNode;
            }
        }

        internal System.Windows.Forms.ToolStrip EditorToolStrip
        {
            get
            {
                return this._miniToolStrip;
            }
            set
            {
                this._miniToolStrip = value;
                this._miniToolStrip.Parent = this.ToolStrip;
                this.LayoutToolStrip();
            }
        }

        public bool FireSyncSelection
        {
            get
            {
                return this.fireSyncSelection;
            }
            set
            {
                this.fireSyncSelection = value;
            }
        }

        public int IndexOfItemUnderMouseToDrag
        {
            get
            {
                return this.indexOfItemUnderMouseToDrag;
            }
            set
            {
                this.indexOfItemUnderMouseToDrag = value;
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

        public DesignerTransaction InsertTansaction
        {
            get
            {
                return this._insertMenuItemTransaction;
            }
            set
            {
                this._insertMenuItemTransaction = value;
            }
        }

        private bool IsToolStripOrItemSelected
        {
            get
            {
                return this.toolStripSelected;
            }
        }

        public ArrayList Items
        {
            get
            {
                if (this.items == null)
                {
                    this.items = new ArrayList();
                }
                return this.items;
            }
        }

        private ToolStripKeyboardHandlingService KeyboardHandlingService
        {
            get
            {
                if (this.keyboardHandlingService == null)
                {
                    this.keyboardHandlingService = (ToolStripKeyboardHandlingService) this.GetService(typeof(ToolStripKeyboardHandlingService));
                    if (this.keyboardHandlingService == null)
                    {
                        this.keyboardHandlingService = new ToolStripKeyboardHandlingService(base.Component.Site);
                    }
                }
                return this.keyboardHandlingService;
            }
        }

        public DesignerTransaction NewItemTransaction
        {
            get
            {
                return this.newItemTransaction;
            }
            set
            {
                this.newItemTransaction = value;
            }
        }

        private Rectangle OverFlowButtonRect
        {
            get
            {
                Rectangle rectangle = new Rectangle();
                if (this.ToolStrip.OverflowButton.Visible)
                {
                    return this.ToolStrip.OverflowButton.Bounds;
                }
                return rectangle;
            }
        }

        internal ISelectionService SelectionService
        {
            get
            {
                if (this._selectionSvc == null)
                {
                    this._selectionSvc = (ISelectionService) this.GetService(typeof(ISelectionService));
                }
                return this._selectionSvc;
            }
        }

        internal override bool SerializePerformLayout
        {
            get
            {
                return true;
            }
        }

        public bool SupportEditing
        {
            get
            {
                WindowsFormsDesignerOptionService service = this.GetService(typeof(DesignerOptionService)) as WindowsFormsDesignerOptionService;
                if (service != null)
                {
                    return service.CompatibilityOptions.EnableInSituEditing;
                }
                return true;
            }
        }

        protected System.Windows.Forms.ToolStrip ToolStrip
        {
            get
            {
                return (System.Windows.Forms.ToolStrip) base.Component;
            }
        }

        internal bool Visible
        {
            get
            {
                return this.currentVisible;
            }
            set
            {
                this.currentVisible = value;
                if ((this.ToolStrip.Visible != value) && !this.SelectionService.GetComponentSelected(this.ToolStrip))
                {
                    this.Control.Visible = value;
                }
            }
        }
    }
}

