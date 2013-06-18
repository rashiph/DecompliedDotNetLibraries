namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripItemBehavior : System.Windows.Forms.Design.Behavior.Behavior
    {
        private Timer _timer;
        private bool doubleClickFired;
        internal Rectangle dragBoxFromMouseDown = Rectangle.Empty;
        private Control dropSource;
        private IEventHandlerService eventSvc;
        private const int GLYPHBORDER = 1;
        private const int GLYPHINSET = 2;
        private bool mouseUpFired;
        private ToolStripItemGlyph selectedGlyph;

        private void ClearInsertionMark(ToolStripItem item)
        {
            if ((ToolStripDesigner.LastCursorPosition == Point.Empty) || (ToolStripDesigner.LastCursorPosition != Cursor.Position))
            {
                ToolStripKeyboardHandlingService keyBoardHandlingService = this.GetKeyBoardHandlingService(item);
                if ((keyBoardHandlingService == null) || !keyBoardHandlingService.TemplateNodeActive)
                {
                    Rectangle empty = Rectangle.Empty;
                    if ((item != null) && (item.Site != null))
                    {
                        IDesignerHost service = (IDesignerHost) item.Site.GetService(typeof(IDesignerHost));
                        if (service != null)
                        {
                            empty = GetPaintingBounds(service, item);
                            empty.Inflate(1, 1);
                            Region r = new Region(empty);
                            try
                            {
                                empty.Inflate(-2, -2);
                                r.Exclude(empty);
                                BehaviorService behaviorService = this.GetBehaviorService(item);
                                if ((behaviorService != null) && (empty != Rectangle.Empty))
                                {
                                    behaviorService.Invalidate(r);
                                }
                            }
                            finally
                            {
                                r.Dispose();
                                r = null;
                            }
                        }
                    }
                }
            }
        }

        private bool CommonParent(ToolStripItem oldSelection, ToolStripItem newSelection)
        {
            ToolStrip currentParent = oldSelection.GetCurrentParent();
            ToolStrip strip2 = newSelection.GetCurrentParent();
            return (currentParent == strip2);
        }

        private void EnterInSituMode(ToolStripItemGlyph glyph)
        {
            if ((glyph.ItemDesigner != null) && !glyph.ItemDesigner.IsEditorActive)
            {
                glyph.ItemDesigner.ShowEditNode(false);
            }
        }

        private BehaviorService GetBehaviorService(ToolStripItem item)
        {
            if (item.Site != null)
            {
                return (BehaviorService) item.Site.GetService(typeof(BehaviorService));
            }
            return null;
        }

        private ToolStripKeyboardHandlingService GetKeyBoardHandlingService(ToolStripItem item)
        {
            if (item.Site != null)
            {
                return (ToolStripKeyboardHandlingService) item.Site.GetService(typeof(ToolStripKeyboardHandlingService));
            }
            return null;
        }

        private static Rectangle GetPaintingBounds(IDesignerHost designerHost, ToolStripItem item)
        {
            Rectangle empty = Rectangle.Empty;
            ToolStripItemDesigner designer = designerHost.GetDesigner(item) as ToolStripItemDesigner;
            if (designer != null)
            {
                empty = designer.GetGlyphBounds();
                ToolStripDesignerUtils.GetAdjustedBounds(item, ref empty);
                empty.Inflate(1, 1);
                empty.Width--;
                empty.Height--;
            }
            return empty;
        }

        private ISelectionService GetSelectionService(ToolStripItem item)
        {
            if (item.Site != null)
            {
                return (ISelectionService) item.Site.GetService(typeof(ISelectionService));
            }
            return null;
        }

        private bool MouseHandlerPresent(ToolStripItem item)
        {
            IMouseHandler handler = null;
            if (this.eventSvc == null)
            {
                this.eventSvc = (IEventHandlerService) item.Site.GetService(typeof(IEventHandlerService));
            }
            if (this.eventSvc != null)
            {
                handler = (IMouseHandler) this.eventSvc.GetHandler(typeof(IMouseHandler));
            }
            return (handler != null);
        }

        private void OnDoubleClickTimerTick(object sender, EventArgs e)
        {
            if (this._timer != null)
            {
                this._timer.Enabled = false;
                this._timer.Tick -= new EventHandler(this.OnDoubleClickTimerTick);
                this._timer.Dispose();
                this._timer = null;
                if ((this.selectedGlyph != null) && (this.selectedGlyph.Item is ToolStripMenuItem))
                {
                    this.EnterInSituMode(this.selectedGlyph);
                }
            }
        }

        public override void OnDragDrop(Glyph g, DragEventArgs e)
        {
            ToolStripItem dragItem = ToolStripDesigner.dragItem;
            if ((e.Data is ToolStripItemDataObject) && (dragItem != null))
            {
                ToolStripItemDataObject data = (ToolStripItemDataObject) e.Data;
                ToolStripItem primarySelection = data.PrimarySelection;
                IDesignerHost host = (IDesignerHost) dragItem.Site.GetService(typeof(IDesignerHost));
                if ((dragItem != primarySelection) && (host != null))
                {
                    string str;
                    ArrayList dragComponents = data.DragComponents;
                    ToolStrip currentParent = dragItem.GetCurrentParent();
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
                        IComponentChangeService service = (IComponentChangeService) dragItem.Site.GetService(typeof(IComponentChangeService));
                        if (service != null)
                        {
                            ToolStripDropDown down = currentParent as ToolStripDropDown;
                            if (down != null)
                            {
                                ToolStripItem ownerItem = down.OwnerItem;
                                service.OnComponentChanging(ownerItem, TypeDescriptor.GetProperties(ownerItem)["DropDownItems"]);
                            }
                            else
                            {
                                service.OnComponentChanging(currentParent, TypeDescriptor.GetProperties(currentParent)["Items"]);
                            }
                        }
                        if (flag)
                        {
                            if (primarySelection != null)
                            {
                                index = dragComponents.IndexOf(primarySelection);
                            }
                            ToolStripKeyboardHandlingService keyBoardHandlingService = this.GetKeyBoardHandlingService(primarySelection);
                            if (keyBoardHandlingService != null)
                            {
                                keyBoardHandlingService.CopyInProgress = true;
                            }
                            dragComponents = DesignerUtils.CopyDragObjects(dragComponents, dragItem.Site) as ArrayList;
                            if (keyBoardHandlingService != null)
                            {
                                keyBoardHandlingService.CopyInProgress = false;
                            }
                            if (index != -1)
                            {
                                primarySelection = dragComponents[index] as ToolStripItem;
                            }
                        }
                        if ((e.Effect == DragDropEffects.Move) || flag)
                        {
                            ISelectionService selectionService = this.GetSelectionService(dragItem);
                            if (selectionService != null)
                            {
                                if (currentParent is ToolStripOverflow)
                                {
                                    currentParent = ((ToolStripOverflow) currentParent).OwnerItem.Owner;
                                }
                                int num2 = currentParent.Items.IndexOf(ToolStripDesigner.dragItem);
                                if (num2 != -1)
                                {
                                    int num3 = 0;
                                    if (primarySelection != null)
                                    {
                                        num3 = currentParent.Items.IndexOf(primarySelection);
                                    }
                                    if ((num3 != -1) && (num2 > num3))
                                    {
                                        num2--;
                                    }
                                    foreach (ToolStripItem item4 in dragComponents)
                                    {
                                        currentParent.Items.Insert(num2, item4);
                                    }
                                }
                                selectionService.SetSelectedComponents(new IComponent[] { primarySelection }, SelectionTypes.Click | SelectionTypes.Replace);
                            }
                        }
                        if (service != null)
                        {
                            ToolStripDropDown down2 = currentParent as ToolStripDropDown;
                            if (down2 != null)
                            {
                                ToolStripItem component = down2.OwnerItem;
                                service.OnComponentChanged(component, TypeDescriptor.GetProperties(component)["DropDownItems"], null, null);
                            }
                            else
                            {
                                service.OnComponentChanged(currentParent, TypeDescriptor.GetProperties(currentParent)["Items"], null, null);
                            }
                            if (flag)
                            {
                                if (down2 != null)
                                {
                                    ToolStripItem item6 = down2.OwnerItem;
                                    service.OnComponentChanging(item6, TypeDescriptor.GetProperties(item6)["DropDownItems"]);
                                    service.OnComponentChanged(item6, TypeDescriptor.GetProperties(item6)["DropDownItems"], null, null);
                                }
                                else
                                {
                                    service.OnComponentChanging(currentParent, TypeDescriptor.GetProperties(currentParent)["Items"]);
                                    service.OnComponentChanged(currentParent, TypeDescriptor.GetProperties(currentParent)["Items"], null, null);
                                }
                            }
                        }
                        foreach (ToolStripItem item7 in dragComponents)
                        {
                            if (item7 is ToolStripDropDownItem)
                            {
                                ToolStripMenuItemDesigner designer = host.GetDesigner(item7) as ToolStripMenuItemDesigner;
                                if (designer != null)
                                {
                                    designer.InitializeDropDown();
                                }
                            }
                            ToolStripDropDown down3 = item7.GetCurrentParent() as ToolStripDropDown;
                            if ((down3 != null) && !(down3 is ToolStripOverflow))
                            {
                                ToolStripDropDownItem item8 = down3.OwnerItem as ToolStripDropDownItem;
                                if (item8 != null)
                                {
                                    ToolStripMenuItemDesigner designer2 = host.GetDesigner(item8) as ToolStripMenuItemDesigner;
                                    if (designer2 != null)
                                    {
                                        designer2.InitializeBodyGlyphsForItems(false, item8);
                                        designer2.InitializeBodyGlyphsForItems(true, item8);
                                    }
                                }
                            }
                        }
                        BehaviorService behaviorService = this.GetBehaviorService(dragItem);
                        if (behaviorService != null)
                        {
                            behaviorService.SyncSelection();
                        }
                    }
                    catch (Exception exception)
                    {
                        if (transaction != null)
                        {
                            transaction.Cancel();
                            transaction = null;
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
                }
            }
        }

        public override void OnDragEnter(Glyph g, DragEventArgs e)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            ToolStripItem item = glyph.Item;
            ToolStripItemDataObject data = e.Data as ToolStripItemDataObject;
            if (data != null)
            {
                if (data.Owner == item.Owner)
                {
                    this.PaintInsertionMark(item);
                    ToolStripDesigner.dragItem = item;
                    e.Effect = DragDropEffects.Move;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public override void OnDragLeave(Glyph g, EventArgs e)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            this.ClearInsertionMark(glyph.Item);
        }

        public override void OnDragOver(Glyph g, DragEventArgs e)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            ToolStripItem item = glyph.Item;
            if (e.Data is ToolStripItemDataObject)
            {
                this.PaintInsertionMark(item);
                e.Effect = (Control.ModifierKeys == Keys.Control) ? DragDropEffects.Copy : DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        public override bool OnMouseDoubleClick(Glyph g, MouseButtons button, Point mouseLoc)
        {
            if (this.mouseUpFired)
            {
                this.doubleClickFired = true;
            }
            return false;
        }

        public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            ToolStripItem item = glyph.Item;
            ISelectionService selectionService = this.GetSelectionService(item);
            BehaviorService behaviorService = this.GetBehaviorService(item);
            ToolStripKeyboardHandlingService keyBoardHandlingService = this.GetKeyBoardHandlingService(item);
            if (((button != MouseButtons.Left) || (keyBoardHandlingService == null)) || (!keyBoardHandlingService.TemplateNodeActive || !keyBoardHandlingService.ActiveTemplateNode.IsSystemContextMenuDisplayed))
            {
                IDesignerHost service = (IDesignerHost) item.Site.GetService(typeof(IDesignerHost));
                ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                ICollection c = null;
                if (selectionService != null)
                {
                    c = selectionService.GetSelectedComponents();
                }
                ArrayList originalSelComps = new ArrayList(c);
                if (((originalSelComps.Count == 0) && (keyBoardHandlingService != null)) && (keyBoardHandlingService.SelectedDesignerControl != null))
                {
                    originalSelComps.Add(keyBoardHandlingService.SelectedDesignerControl);
                }
                if (keyBoardHandlingService != null)
                {
                    keyBoardHandlingService.SelectedDesignerControl = null;
                    if (keyBoardHandlingService.TemplateNodeActive)
                    {
                        keyBoardHandlingService.ActiveTemplateNode.CommitAndSelect();
                        if ((primarySelection != null) && (primarySelection == item))
                        {
                            selectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                        }
                    }
                }
                if ((selectionService == null) || this.MouseHandlerPresent(item))
                {
                    return false;
                }
                if ((glyph != null) && (button == MouseButtons.Left))
                {
                    ToolStripItem oldSelection = selectionService.PrimarySelection as ToolStripItem;
                    this.SetParentDesignerValuesForDragDrop(item, true, mouseLoc);
                    if ((oldSelection != null) && (oldSelection == item))
                    {
                        if (oldSelection != null)
                        {
                            ToolStripItemDesigner itemDesigner = glyph.ItemDesigner;
                            if ((itemDesigner != null) && itemDesigner.IsEditorActive)
                            {
                                return false;
                            }
                        }
                        if ((Control.ModifierKeys & (Keys.Control | Keys.Shift)) > Keys.None)
                        {
                            selectionService.SetSelectedComponents(new IComponent[] { oldSelection }, SelectionTypes.Remove);
                            return false;
                        }
                        if (oldSelection is ToolStripMenuItem)
                        {
                            this._timer = new Timer();
                            this._timer.Interval = SystemInformation.DoubleClickTime;
                            this._timer.Tick += new EventHandler(this.OnDoubleClickTimerTick);
                            this._timer.Enabled = true;
                            this.selectedGlyph = glyph;
                        }
                    }
                    else
                    {
                        bool flag2 = (Control.ModifierKeys & Keys.Shift) > Keys.None;
                        if (!selectionService.GetComponentSelected(item))
                        {
                            this.mouseUpFired = false;
                            this.doubleClickFired = false;
                            if ((flag2 && (oldSelection != null)) && this.CommonParent(oldSelection, item))
                            {
                                ToolStrip owner = null;
                                if (item.IsOnOverflow)
                                {
                                    owner = item.Owner;
                                }
                                else
                                {
                                    owner = item.GetCurrentParent();
                                }
                                int num = Math.Min(owner.Items.IndexOf(oldSelection), owner.Items.IndexOf(item));
                                int num2 = Math.Max(owner.Items.IndexOf(oldSelection), owner.Items.IndexOf(item));
                                int num3 = (num2 - num) + 1;
                                if (num3 == 2)
                                {
                                    selectionService.SetSelectedComponents(new IComponent[] { item });
                                }
                                else
                                {
                                    object[] components = new object[num3];
                                    int num4 = 0;
                                    for (int i = num; i <= num2; i++)
                                    {
                                        components[num4++] = owner.Items[i];
                                    }
                                    selectionService.SetSelectedComponents(new IComponent[] { owner }, SelectionTypes.Replace);
                                    ToolStripDesigner.shiftState = true;
                                    selectionService.SetSelectedComponents(components, SelectionTypes.Replace);
                                }
                            }
                            else
                            {
                                if (item.IsOnDropDown && ToolStripDesigner.shiftState)
                                {
                                    ToolStripDesigner.shiftState = false;
                                    if (behaviorService != null)
                                    {
                                        behaviorService.Invalidate(item.Owner.Bounds);
                                    }
                                }
                                selectionService.SetSelectedComponents(new IComponent[] { item }, SelectionTypes.Auto);
                            }
                            if (keyBoardHandlingService != null)
                            {
                                keyBoardHandlingService.ShiftPrimaryItem = item;
                            }
                        }
                        else if (flag2 || ((Control.ModifierKeys & Keys.Control) > Keys.None))
                        {
                            selectionService.SetSelectedComponents(new IComponent[] { item }, SelectionTypes.Remove);
                        }
                    }
                }
                if (((glyph != null) && (button == MouseButtons.Right)) && !selectionService.GetComponentSelected(item))
                {
                    selectionService.SetSelectedComponents(new IComponent[] { item });
                }
                ToolStripDesignerUtils.InvalidateSelection(originalSelComps, item, item.Site, false);
            }
            return false;
        }

        public override bool OnMouseEnter(Glyph g)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            if (glyph != null)
            {
                ToolStripItem item = glyph.Item;
                if (this.MouseHandlerPresent(item))
                {
                    return false;
                }
                ISelectionService selectionService = this.GetSelectionService(item);
                if ((selectionService != null) && !selectionService.GetComponentSelected(item))
                {
                    this.PaintInsertionMark(item);
                }
            }
            return false;
        }

        public override bool OnMouseLeave(Glyph g)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            if (glyph != null)
            {
                ToolStripItem item = glyph.Item;
                if (this.MouseHandlerPresent(item))
                {
                    return false;
                }
                ISelectionService selectionService = this.GetSelectionService(item);
                if ((selectionService != null) && !selectionService.GetComponentSelected(item))
                {
                    this.ClearInsertionMark(item);
                }
            }
            return false;
        }

        public override bool OnMouseMove(Glyph g, MouseButtons button, Point mouseLoc)
        {
            bool flag = false;
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            ToolStripItem item = glyph.Item;
            ISelectionService selectionService = this.GetSelectionService(item);
            if (((selectionService != null) && (item.Site != null)) && !this.MouseHandlerPresent(item))
            {
                if (!selectionService.GetComponentSelected(item))
                {
                    this.PaintInsertionMark(item);
                    flag = false;
                }
                if (((button != MouseButtons.Left) || (glyph == null)) || ((glyph.ItemDesigner == null) || glyph.ItemDesigner.IsEditorActive))
                {
                    return flag;
                }
                Rectangle empty = Rectangle.Empty;
                IDesignerHost service = (IDesignerHost) item.Site.GetService(typeof(IDesignerHost));
                if ((item.Placement == ToolStripItemPlacement.Overflow) || ((item.Placement == ToolStripItemPlacement.Main) && !item.IsOnDropDown))
                {
                    ToolStrip mainToolStrip = glyph.ItemDesigner.GetMainToolStrip();
                    ToolStripDesigner designer = service.GetDesigner(mainToolStrip) as ToolStripDesigner;
                    if (designer != null)
                    {
                        empty = designer.DragBoxFromMouseDown;
                    }
                }
                else if (item.IsOnDropDown)
                {
                    ToolStripDropDown owner = item.Owner as ToolStripDropDown;
                    if (owner != null)
                    {
                        ToolStripItem ownerItem = owner.OwnerItem;
                        ToolStripItemDesigner designer3 = service.GetDesigner(ownerItem) as ToolStripItemDesigner;
                        if (designer3 != null)
                        {
                            empty = designer3.dragBoxFromMouseDown;
                        }
                    }
                }
                if (!(empty != Rectangle.Empty) || empty.Contains(mouseLoc.X, mouseLoc.Y))
                {
                    return flag;
                }
                if (this._timer != null)
                {
                    this._timer.Enabled = false;
                    this._timer.Tick -= new EventHandler(this.OnDoubleClickTimerTick);
                    this._timer.Dispose();
                    this._timer = null;
                }
                try
                {
                    ArrayList dragComponents = new ArrayList();
                    foreach (IComponent component in selectionService.GetSelectedComponents())
                    {
                        ToolStripItem item3 = component as ToolStripItem;
                        if (item3 != null)
                        {
                            dragComponents.Add(item3);
                        }
                    }
                    ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                    if (primarySelection != null)
                    {
                        ToolStrip strip2 = primarySelection.Owner;
                        ToolStripItemDataObject data = new ToolStripItemDataObject(dragComponents, primarySelection, strip2);
                        this.DropSource.QueryContinueDrag += new QueryContinueDragEventHandler(this.QueryContinueDrag);
                        ToolStripDropDownItem item5 = item as ToolStripDropDownItem;
                        if (item5 != null)
                        {
                            ToolStripMenuItemDesigner designer4 = service.GetDesigner(item5) as ToolStripMenuItemDesigner;
                            if (designer4 != null)
                            {
                                designer4.InitializeBodyGlyphsForItems(false, item5);
                                item5.HideDropDown();
                            }
                        }
                        else if (item.IsOnDropDown && !item.IsOnOverflow)
                        {
                            ToolStripDropDown currentParent = item.GetCurrentParent() as ToolStripDropDown;
                            ToolStripDropDownItem item6 = currentParent.OwnerItem as ToolStripDropDownItem;
                            selectionService.SetSelectedComponents(new IComponent[] { item6 }, SelectionTypes.Replace);
                        }
                        this.DropSource.DoDragDrop(data, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll);
                    }
                }
                finally
                {
                    this.DropSource.QueryContinueDrag -= new QueryContinueDragEventHandler(this.QueryContinueDrag);
                    this.SetParentDesignerValuesForDragDrop(item, false, Point.Empty);
                    ToolStripDesigner.dragItem = null;
                    this.dropSource = null;
                }
            }
            return false;
        }

        public override bool OnMouseUp(Glyph g, MouseButtons button)
        {
            ToolStripItemGlyph glyph = g as ToolStripItemGlyph;
            ToolStripItem item = glyph.Item;
            if (!this.MouseHandlerPresent(item))
            {
                this.SetParentDesignerValuesForDragDrop(item, false, Point.Empty);
                if (this.doubleClickFired)
                {
                    if ((glyph != null) && (button == MouseButtons.Left))
                    {
                        ISelectionService selectionService = this.GetSelectionService(item);
                        if (selectionService == null)
                        {
                            return false;
                        }
                        ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                        if (primarySelection == item)
                        {
                            if (this._timer != null)
                            {
                                this._timer.Enabled = false;
                                this._timer.Tick -= new EventHandler(this.OnDoubleClickTimerTick);
                                this._timer.Dispose();
                                this._timer = null;
                            }
                            if (primarySelection != null)
                            {
                                ToolStripItemDesigner itemDesigner = glyph.ItemDesigner;
                                if ((itemDesigner != null) && itemDesigner.IsEditorActive)
                                {
                                    return false;
                                }
                                itemDesigner.DoDefaultAction();
                            }
                            this.doubleClickFired = false;
                            this.mouseUpFired = false;
                        }
                    }
                }
                else
                {
                    this.mouseUpFired = true;
                }
            }
            return false;
        }

        private void PaintInsertionMark(ToolStripItem item)
        {
            if ((ToolStripDesigner.LastCursorPosition == Point.Empty) || (ToolStripDesigner.LastCursorPosition != Cursor.Position))
            {
                ToolStripKeyboardHandlingService keyBoardHandlingService = this.GetKeyBoardHandlingService(item);
                if (((keyBoardHandlingService == null) || !keyBoardHandlingService.TemplateNodeActive) && ((item != null) && (item.Site != null)))
                {
                    ToolStripDesigner.LastCursorPosition = Cursor.Position;
                    IDesignerHost service = (IDesignerHost) item.Site.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        Rectangle paintingBounds = GetPaintingBounds(service, item);
                        BehaviorService behaviorService = this.GetBehaviorService(item);
                        if (behaviorService != null)
                        {
                            using (Graphics graphics = behaviorService.AdornerWindowGraphics)
                            {
                                using (Pen pen = new Pen(new SolidBrush(Color.Black)))
                                {
                                    pen.DashStyle = DashStyle.Dot;
                                    graphics.DrawRectangle(pen, paintingBounds);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            if ((e.Action != DragAction.Continue) && e.EscapePressed)
            {
                e.Action = DragAction.Cancel;
                ToolStripItem glyphItem = sender as ToolStripItem;
                this.SetParentDesignerValuesForDragDrop(glyphItem, false, Point.Empty);
                ISelectionService selectionService = this.GetSelectionService(glyphItem);
                if (selectionService != null)
                {
                    selectionService.SetSelectedComponents(new IComponent[] { glyphItem }, SelectionTypes.Auto);
                }
                ToolStripDesigner.dragItem = null;
            }
        }

        private void SetParentDesignerValuesForDragDrop(ToolStripItem glyphItem, bool setValues, Point mouseLoc)
        {
            if (glyphItem.Site != null)
            {
                Size size = new Size(1, 1);
                IDesignerHost service = (IDesignerHost) glyphItem.Site.GetService(typeof(IDesignerHost));
                if ((glyphItem.Placement == ToolStripItemPlacement.Overflow) || ((glyphItem.Placement == ToolStripItemPlacement.Main) && !glyphItem.IsOnDropDown))
                {
                    ToolStrip mainToolStrip = (service.GetDesigner(glyphItem) as ToolStripItemDesigner).GetMainToolStrip();
                    ToolStripDesigner designer = service.GetDesigner(mainToolStrip) as ToolStripDesigner;
                    if (designer != null)
                    {
                        if (setValues)
                        {
                            designer.IndexOfItemUnderMouseToDrag = mainToolStrip.Items.IndexOf(glyphItem);
                            designer.DragBoxFromMouseDown = this.dragBoxFromMouseDown = new Rectangle(new Point(mouseLoc.X - (size.Width / 2), mouseLoc.Y - (size.Height / 2)), size);
                        }
                        else
                        {
                            designer.IndexOfItemUnderMouseToDrag = -1;
                            designer.DragBoxFromMouseDown = this.dragBoxFromMouseDown = Rectangle.Empty;
                        }
                    }
                }
                else if (glyphItem.IsOnDropDown)
                {
                    ToolStripDropDown owner = glyphItem.Owner as ToolStripDropDown;
                    if (owner != null)
                    {
                        ToolStripItem ownerItem = owner.OwnerItem;
                        ToolStripItemDesigner designer3 = service.GetDesigner(ownerItem) as ToolStripItemDesigner;
                        if (designer3 != null)
                        {
                            if (setValues)
                            {
                                designer3.indexOfItemUnderMouseToDrag = owner.Items.IndexOf(glyphItem);
                                designer3.dragBoxFromMouseDown = this.dragBoxFromMouseDown = new Rectangle(new Point(mouseLoc.X - (size.Width / 2), mouseLoc.Y - (size.Height / 2)), size);
                            }
                            else
                            {
                                designer3.indexOfItemUnderMouseToDrag = -1;
                                designer3.dragBoxFromMouseDown = this.dragBoxFromMouseDown = Rectangle.Empty;
                            }
                        }
                    }
                }
            }
        }

        private Control DropSource
        {
            get
            {
                if (this.dropSource == null)
                {
                    this.dropSource = new Control();
                }
                return this.dropSource;
            }
        }
    }
}

