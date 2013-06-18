namespace System.Windows.Forms.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal sealed class SelectionUIService : Control, ISelectionUIService
    {
        private bool batchChanged;
        private bool batchMode;
        private bool batchSync;
        private object containerDrag;
        private bool ctrlSelect;
        private object[] dragComponents;
        private ISelectionUIHandler dragHandler;
        private bool dragMoved;
        private SelectionRules dragRules;
        private DesignerTransaction dragTransaction;
        private const int HITTEST_CONTAINER_SELECTOR = 1;
        private const int HITTEST_DEFAULT = 3;
        private const int HITTEST_NORMAL_SELECTION = 2;
        private IDesignerHost host;
        private bool ignoreCaptureChanged;
        private static readonly Point InvalidPoint = new Point(-2147483648, -2147483648);
        private Point lastMoveScreenCoord = Point.Empty;
        private Point mouseDragAnchor = InvalidPoint;
        private bool mouseDragging;
        private int mouseDragHitTest;
        private Rectangle mouseDragOffset = Rectangle.Empty;
        private bool savedVisible;
        private Hashtable selectionHandlers;
        private Hashtable selectionItems;
        private ISelectionService selSvc;

        event ContainerSelectorActiveEventHandler ISelectionUIService.ContainerSelectorActive;

        public SelectionUIService(IDesignerHost host)
        {
            base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.StandardClick | ControlStyles.Opaque, true);
            this.host = host;
            this.dragHandler = null;
            this.dragComponents = null;
            this.selectionItems = new Hashtable();
            this.selectionHandlers = new Hashtable();
            this.AllowDrop = true;
            this.Text = "SelectionUIOverlay";
            this.selSvc = (ISelectionService) host.GetService(typeof(ISelectionService));
            if (this.selSvc != null)
            {
                this.selSvc.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            host.TransactionOpened += new EventHandler(this.OnTransactionOpened);
            host.TransactionClosed += new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
            if (host.InTransaction)
            {
                this.OnTransactionOpened(host, EventArgs.Empty);
            }
            IComponentChangeService service = (IComponentChangeService) host.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemove);
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
            }
            SystemEvents.DisplaySettingsChanged += new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.InstalledFontsChanged += new EventHandler(this.OnSystemSettingChanged);
            SystemEvents.UserPreferenceChanged += new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
        }

        private void BeginMouseDrag(Point anchor, int hitTest)
        {
            base.Capture = true;
            this.ignoreCaptureChanged = false;
            this.mouseDragAnchor = anchor;
            this.mouseDragging = true;
            this.mouseDragHitTest = hitTest;
            this.mouseDragOffset = new Rectangle();
            this.savedVisible = base.Visible;
        }

        private void DisplayError(Exception e)
        {
            IUIService service = (IUIService) this.host.GetService(typeof(IUIService));
            if (service != null)
            {
                service.ShowError(e);
            }
            else
            {
                string message = e.Message;
                if ((message == null) || (message.Length == 0))
                {
                    message = e.ToString();
                }
                System.Windows.Forms.Design.RTLAwareMessageBox.Show(null, message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.selSvc != null)
                {
                    this.selSvc.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                }
                if (this.host != null)
                {
                    this.host.TransactionOpened -= new EventHandler(this.OnTransactionOpened);
                    this.host.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.OnTransactionClosed);
                    if (this.host.InTransaction)
                    {
                        this.OnTransactionClosed(this.host, new DesignerTransactionCloseEventArgs(true, true));
                    }
                    IComponentChangeService service = (IComponentChangeService) this.host.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemove);
                        service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    }
                }
                foreach (SelectionUIItem item in this.selectionItems.Values)
                {
                    item.Dispose();
                }
                this.selectionHandlers.Clear();
                this.selectionItems.Clear();
                SystemEvents.DisplaySettingsChanged -= new EventHandler(this.OnSystemSettingChanged);
                SystemEvents.InstalledFontsChanged -= new EventHandler(this.OnSystemSettingChanged);
                SystemEvents.UserPreferenceChanged -= new UserPreferenceChangedEventHandler(this.OnUserPreferenceChanged);
            }
            base.Dispose(disposing);
        }

        private void EndMouseDrag(Point position)
        {
            if (!base.IsDisposed)
            {
                this.ignoreCaptureChanged = true;
                base.Capture = false;
                this.mouseDragAnchor = InvalidPoint;
                this.mouseDragOffset = Rectangle.Empty;
                this.mouseDragHitTest = 0;
                this.dragMoved = false;
                this.SetSelectionCursor(position);
                this.mouseDragging = this.ctrlSelect = false;
            }
        }

        private ISelectionUIHandler GetHandler(object component)
        {
            return (ISelectionUIHandler) this.selectionHandlers[component];
        }

        private HitTestInfo GetHitTest(Point value, int flags)
        {
            Point pt = base.PointToClient(value);
            foreach (SelectionUIItem item in this.selectionItems.Values)
            {
                if ((((flags & 1) != 0) && (item is ContainerSelectionUIItem)) && ((item.GetRules() & SelectionRules.Visible) != SelectionRules.None))
                {
                    int hitTest = item.GetHitTest(pt);
                    if ((hitTest & 0x200) != 0)
                    {
                        return new HitTestInfo(hitTest, item, true);
                    }
                }
                if ((((flags & 2) != 0) && !(item is ContainerSelectionUIItem)) && ((item.GetRules() & SelectionRules.Visible) != SelectionRules.None))
                {
                    int num2 = item.GetHitTest(pt);
                    if (num2 != 0x100)
                    {
                        if (num2 != 0)
                        {
                            return new HitTestInfo(num2, item);
                        }
                        return new HitTestInfo(0x100, item);
                    }
                }
            }
            return new HitTestInfo(0x100, null);
        }

        public static string GetTransactionName(SelectionRules rules, object[] objects)
        {
            if ((rules & SelectionRules.Moveable) != SelectionRules.None)
            {
                if (objects.Length > 1)
                {
                    return System.Design.SR.GetString("DragDropMoveComponents", new object[] { objects.Length });
                }
                string name = string.Empty;
                if (objects.Length > 0)
                {
                    IComponent component = objects[0] as IComponent;
                    if ((component != null) && (component.Site != null))
                    {
                        name = component.Site.Name;
                    }
                    else
                    {
                        name = objects[0].GetType().Name;
                    }
                }
                return System.Design.SR.GetString("DragDropMoveComponent", new object[] { name });
            }
            if ((rules & SelectionRules.AllSizeable) != SelectionRules.None)
            {
                if (objects.Length > 1)
                {
                    return System.Design.SR.GetString("DragDropSizeComponents", new object[] { objects.Length });
                }
                string str3 = string.Empty;
                if (objects.Length > 0)
                {
                    IComponent component2 = objects[0] as IComponent;
                    if ((component2 != null) && (component2.Site != null))
                    {
                        str3 = component2.Site.Name;
                    }
                    else
                    {
                        str3 = objects[0].GetType().Name;
                    }
                }
                return System.Design.SR.GetString("DragDropSizeComponent", new object[] { str3 });
            }
            return System.Design.SR.GetString("DragDropDragComponents", new object[] { objects.Length });
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs ccevent)
        {
            if (!this.batchMode)
            {
                ((ISelectionUIService) this).SyncSelection();
            }
            else
            {
                this.batchChanged = true;
            }
        }

        private void OnComponentRemove(object sender, ComponentEventArgs ce)
        {
            this.selectionHandlers.Remove(ce.Component);
            this.selectionItems.Remove(ce.Component);
            ((ISelectionUIService) this).SyncComponent(ce.Component);
        }

        private void OnContainerSelectorActive(ContainerSelectorActiveEventArgs e)
        {
            if (this.containerSelectorActive != null)
            {
                this.containerSelectorActive(this, e);
            }
        }

        protected override void OnDoubleClick(EventArgs devent)
        {
            base.OnDoubleClick(devent);
            if (this.selSvc != null)
            {
                object primarySelection = this.selSvc.PrimarySelection;
                if (primarySelection != null)
                {
                    ISelectionUIHandler handler = this.GetHandler(primarySelection);
                    if (handler != null)
                    {
                        handler.OnSelectionDoubleClick((IComponent) primarySelection);
                    }
                }
            }
        }

        protected override void OnDragDrop(DragEventArgs devent)
        {
            base.OnDragDrop(devent);
            if (this.dragHandler != null)
            {
                this.dragHandler.OleDragDrop(devent);
            }
        }

        protected override void OnDragEnter(DragEventArgs devent)
        {
            base.OnDragEnter(devent);
            if (this.dragHandler != null)
            {
                this.dragHandler.OleDragEnter(devent);
            }
        }

        protected override void OnDragLeave(EventArgs e)
        {
            base.OnDragLeave(e);
            if (this.dragHandler != null)
            {
                this.dragHandler.OleDragLeave();
            }
        }

        protected override void OnDragOver(DragEventArgs devent)
        {
            base.OnDragOver(devent);
            if (this.dragHandler != null)
            {
                this.dragHandler.OleDragOver(devent);
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            this.UpdateWindowRegion();
        }

        protected override void OnMouseDown(MouseEventArgs me)
        {
            if ((this.dragHandler == null) && (this.selSvc != null))
            {
                try
                {
                    Point point = base.PointToScreen(new Point(me.X, me.Y));
                    HitTestInfo hitTest = this.GetHitTest(point, 3);
                    int num = hitTest.hitTest;
                    if ((num & 0x200) != 0)
                    {
                        this.selSvc.SetSelectedComponents(new object[] { hitTest.selectionUIHit.component }, SelectionTypes.Auto);
                        SelectionRules moveable = SelectionRules.Moveable;
                        if (((ISelectionUIService) this).BeginDrag(moveable, point.X, point.Y))
                        {
                            base.Visible = false;
                            this.containerDrag = hitTest.selectionUIHit.component;
                            this.BeginMouseDrag(point, num);
                        }
                    }
                    else if ((num != 0x100) && (me.Button == MouseButtons.Left))
                    {
                        SelectionRules none = SelectionRules.None;
                        this.ctrlSelect = (Control.ModifierKeys & Keys.Control) != Keys.None;
                        if (!this.ctrlSelect)
                        {
                            this.selSvc.SetSelectedComponents(new object[] { hitTest.selectionUIHit.component }, SelectionTypes.Click);
                        }
                        if ((num & 12) != 0)
                        {
                            none |= SelectionRules.Moveable;
                        }
                        if ((num & 3) != 0)
                        {
                            if ((num & 0x41) == 0x41)
                            {
                                none |= SelectionRules.RightSizeable;
                            }
                            if ((num & 0x11) == 0x11)
                            {
                                none |= SelectionRules.LeftSizeable;
                            }
                            if ((num & 0x22) == 0x22)
                            {
                                none |= SelectionRules.TopSizeable;
                            }
                            if ((num & 130) == 130)
                            {
                                none |= SelectionRules.BottomSizeable;
                            }
                            if (((ISelectionUIService) this).BeginDrag(none, point.X, point.Y))
                            {
                                this.BeginMouseDrag(point, num);
                            }
                        }
                        else
                        {
                            this.dragRules = none;
                            this.BeginMouseDrag(point, num);
                        }
                    }
                    else if (num == 0x100)
                    {
                        this.dragRules = SelectionRules.None;
                        this.mouseDragAnchor = InvalidPoint;
                    }
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                    if (exception != CheckoutException.Canceled)
                    {
                        this.DisplayError(exception);
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs me)
        {
            base.OnMouseMove(me);
            Point point = base.PointToScreen(new Point(me.X, me.Y));
            HitTestInfo hitTest = this.GetHitTest(point, 1);
            if ((hitTest.hitTest != 0x200) && (hitTest.selectionUIHit != null))
            {
                this.OnContainerSelectorActive(new ContainerSelectorActiveEventArgs(hitTest.selectionUIHit.component));
            }
            if (this.lastMoveScreenCoord != point)
            {
                if (!this.mouseDragging)
                {
                    this.SetSelectionCursor(point);
                }
                else
                {
                    if (!((ISelectionUIService) this).Dragging && ((this.mouseDragHitTest & 12) != 0))
                    {
                        Size dragSize = SystemInformation.DragSize;
                        if ((Math.Abs((int) (point.X - this.mouseDragAnchor.X)) < dragSize.Width) && (Math.Abs((int) (point.Y - this.mouseDragAnchor.Y)) < dragSize.Height))
                        {
                            return;
                        }
                        this.ignoreCaptureChanged = true;
                        if (!((ISelectionUIService) this).BeginDrag(this.dragRules, this.mouseDragAnchor.X, this.mouseDragAnchor.Y))
                        {
                            this.EndMouseDrag(Control.MousePosition);
                            return;
                        }
                        this.ctrlSelect = false;
                    }
                    Rectangle mouseDragOffset = this.mouseDragOffset;
                    if ((this.mouseDragHitTest & 4) != 0)
                    {
                        this.mouseDragOffset.X = point.X - this.mouseDragAnchor.X;
                    }
                    if ((this.mouseDragHitTest & 8) != 0)
                    {
                        this.mouseDragOffset.Y = point.Y - this.mouseDragAnchor.Y;
                    }
                    if ((this.mouseDragHitTest & 1) != 0)
                    {
                        if ((this.mouseDragHitTest & 0x10) != 0)
                        {
                            this.mouseDragOffset.X = point.X - this.mouseDragAnchor.X;
                            this.mouseDragOffset.Width = this.mouseDragAnchor.X - point.X;
                        }
                        else
                        {
                            this.mouseDragOffset.Width = point.X - this.mouseDragAnchor.X;
                        }
                    }
                    if ((this.mouseDragHitTest & 2) != 0)
                    {
                        if ((this.mouseDragHitTest & 0x20) != 0)
                        {
                            this.mouseDragOffset.Y = point.Y - this.mouseDragAnchor.Y;
                            this.mouseDragOffset.Height = this.mouseDragAnchor.Y - point.Y;
                        }
                        else
                        {
                            this.mouseDragOffset.Height = point.Y - this.mouseDragAnchor.Y;
                        }
                    }
                    if (!mouseDragOffset.Equals(this.mouseDragOffset))
                    {
                        Rectangle offset = this.mouseDragOffset;
                        offset.X -= mouseDragOffset.X;
                        offset.Y -= mouseDragOffset.Y;
                        offset.Width -= mouseDragOffset.Width;
                        offset.Height -= mouseDragOffset.Height;
                        if (((offset.X != 0) || (offset.Y != 0)) || ((offset.Width != 0) || (offset.Height != 0)))
                        {
                            if (((this.mouseDragHitTest & 4) != 0) || ((this.mouseDragHitTest & 8) != 0))
                            {
                                this.Cursor = Cursors.Default;
                            }
                            ((ISelectionUIService) this).DragMoved(offset);
                        }
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs me)
        {
            try
            {
                Point point = base.PointToScreen(new Point(me.X, me.Y));
                if ((this.ctrlSelect && !this.mouseDragging) && (this.selSvc != null))
                {
                    HitTestInfo hitTest = this.GetHitTest(point, 3);
                    this.selSvc.SetSelectedComponents(new object[] { hitTest.selectionUIHit.component }, SelectionTypes.Click);
                }
                if (this.mouseDragging)
                {
                    object containerDrag = this.containerDrag;
                    bool dragMoved = this.dragMoved;
                    this.EndMouseDrag(point);
                    if (((ISelectionUIService) this).Dragging)
                    {
                        ((ISelectionUIService) this).EndDrag(false);
                    }
                    if (((me.Button == MouseButtons.Right) && (containerDrag != null)) && !dragMoved)
                    {
                        this.OnContainerSelectorActive(new ContainerSelectorActiveEventArgs(containerDrag, ContainerSelectorActiveEventArgsType.Contextmenu));
                    }
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
                if (exception != CheckoutException.Canceled)
                {
                    this.DisplayError(exception);
                }
            }
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            base.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            foreach (SelectionUIItem item in this.selectionItems.Values)
            {
                if (!(item is ContainerSelectionUIItem))
                {
                    item.DoPaint(e.Graphics);
                }
            }
            foreach (SelectionUIItem item2 in this.selectionItems.Values)
            {
                if (item2 is ContainerSelectionUIItem)
                {
                    item2.DoPaint(e.Graphics);
                }
            }
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            ICollection selectedComponents = this.selSvc.GetSelectedComponents();
            Hashtable hashtable = new Hashtable(selectedComponents.Count);
            bool flag = false;
            foreach (object obj2 in selectedComponents)
            {
                object obj3 = this.selectionItems[obj2];
                bool flag2 = true;
                if (obj3 != null)
                {
                    ContainerSelectionUIItem item = obj3 as ContainerSelectionUIItem;
                    if (item != null)
                    {
                        item.Dispose();
                        flag = true;
                    }
                    else
                    {
                        hashtable[obj2] = obj3;
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    flag = true;
                    hashtable[obj2] = new SelectionUIItem(this, obj2);
                }
            }
            if (!flag)
            {
                flag = this.selectionItems.Keys.Count != hashtable.Keys.Count;
            }
            this.selectionItems = hashtable;
            if (flag)
            {
                this.UpdateWindowRegion();
            }
            base.Invalidate();
            base.Update();
        }

        private void OnSystemSettingChanged(object sender, EventArgs e)
        {
            base.Invalidate();
        }

        private void OnTransactionClosed(object sender, DesignerTransactionCloseEventArgs e)
        {
            if (e.LastTransaction)
            {
                this.batchMode = false;
                if (this.batchChanged)
                {
                    this.batchChanged = false;
                    ((ISelectionUIService) this).SyncSelection();
                }
                if (this.batchSync)
                {
                    this.batchSync = false;
                    ((ISelectionUIService) this).SyncComponent(null);
                }
            }
        }

        private void OnTransactionOpened(object sender, EventArgs e)
        {
            this.batchMode = true;
        }

        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            base.Invalidate();
        }

        private void SetSelectionCursor(Point pt)
        {
            Point point = base.PointToClient(pt);
            foreach (SelectionUIItem item in this.selectionItems.Values)
            {
                if (!(item is ContainerSelectionUIItem))
                {
                    Cursor cursorAtPoint = item.GetCursorAtPoint(point);
                    if (cursorAtPoint != null)
                    {
                        if (cursorAtPoint == Cursors.Default)
                        {
                            this.Cursor = null;
                        }
                        else
                        {
                            this.Cursor = cursorAtPoint;
                        }
                        return;
                    }
                }
            }
            foreach (SelectionUIItem item2 in this.selectionItems.Values)
            {
                if (item2 is ContainerSelectionUIItem)
                {
                    Cursor cursor2 = item2.GetCursorAtPoint(point);
                    if (cursor2 != null)
                    {
                        if (cursor2 == Cursors.Default)
                        {
                            this.Cursor = null;
                        }
                        else
                        {
                            this.Cursor = cursor2;
                        }
                        return;
                    }
                }
            }
            this.Cursor = null;
        }

        void ISelectionUIService.AssignSelectionUIHandler(object component, ISelectionUIHandler handler)
        {
            ISelectionUIHandler handler2 = (ISelectionUIHandler) this.selectionHandlers[component];
            if (handler2 != null)
            {
                if (handler != handler2)
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                this.selectionHandlers[component] = handler;
                if ((this.selSvc != null) && this.selSvc.GetComponentSelected(component))
                {
                    SelectionUIItem item = new SelectionUIItem(this, component);
                    this.selectionItems[component] = item;
                    this.UpdateWindowRegion();
                    item.Invalidate();
                }
            }
        }

        bool ISelectionUIService.BeginDrag(SelectionRules rules, int initialX, int initialY)
        {
            if (this.dragHandler != null)
            {
                return false;
            }
            if (rules == SelectionRules.None)
            {
                return false;
            }
            if (this.selSvc == null)
            {
                return false;
            }
            this.savedVisible = base.Visible;
            ICollection selectedComponents = this.selSvc.GetSelectedComponents();
            object[] array = new object[selectedComponents.Count];
            selectedComponents.CopyTo(array, 0);
            array = ((ISelectionUIService) this).FilterSelection(array, rules);
            if (array.Length == 0)
            {
                return false;
            }
            ISelectionUIHandler handler = null;
            object primarySelection = this.selSvc.PrimarySelection;
            if (primarySelection != null)
            {
                handler = this.GetHandler(primarySelection);
            }
            if (handler == null)
            {
                return false;
            }
            ArrayList list = new ArrayList();
            for (int i = 0; i < array.Length; i++)
            {
                if ((this.GetHandler(array[i]) == handler) && ((handler.GetComponentRules(array[i]) & rules) == rules))
                {
                    list.Add(array[i]);
                }
            }
            if (list.Count == 0)
            {
                return false;
            }
            array = list.ToArray();
            bool flag = false;
            this.dragComponents = array;
            this.dragRules = rules;
            this.dragHandler = handler;
            string transactionName = GetTransactionName(rules, array);
            this.dragTransaction = this.host.CreateTransaction(transactionName);
            try
            {
                if (!handler.QueryBeginDrag(array, rules, initialX, initialY) || (this.dragHandler == null))
                {
                    return flag;
                }
                try
                {
                    flag = handler.BeginDrag(array, rules, initialX, initialY);
                }
                catch (Exception)
                {
                    flag = false;
                }
            }
            finally
            {
                if (!flag)
                {
                    this.dragComponents = null;
                    this.dragRules = SelectionRules.None;
                    this.dragHandler = null;
                    if (this.dragTransaction != null)
                    {
                        this.dragTransaction.Commit();
                        this.dragTransaction = null;
                    }
                }
            }
            return flag;
        }

        void ISelectionUIService.ClearSelectionUIHandler(object component, ISelectionUIHandler handler)
        {
            ISelectionUIHandler handler2 = (ISelectionUIHandler) this.selectionHandlers[component];
            if (handler2 == handler)
            {
                this.selectionHandlers[component] = null;
            }
        }

        void ISelectionUIService.DragMoved(Rectangle offset)
        {
            Rectangle empty = Rectangle.Empty;
            if (this.dragHandler == null)
            {
                throw new Exception(System.Design.SR.GetString("DesignerBeginDragNotCalled"));
            }
            if (((this.dragRules & SelectionRules.Moveable) == SelectionRules.None) && ((this.dragRules & (SelectionRules.LeftSizeable | SelectionRules.TopSizeable)) == SelectionRules.None))
            {
                empty = new Rectangle(0, 0, offset.Width, offset.Height);
            }
            if ((this.dragRules & SelectionRules.AllSizeable) == SelectionRules.None)
            {
                if (empty.IsEmpty)
                {
                    empty = new Rectangle(offset.X, offset.Y, 0, 0);
                }
                else
                {
                    empty.Width = empty.Height = 0;
                }
            }
            if (!empty.IsEmpty)
            {
                offset = empty;
            }
            base.Visible = false;
            this.dragMoved = true;
            this.dragHandler.DragMoved(this.dragComponents, offset);
        }

        void ISelectionUIService.EndDrag(bool cancel)
        {
            this.containerDrag = null;
            ISelectionUIHandler dragHandler = this.dragHandler;
            object[] dragComponents = this.dragComponents;
            this.dragHandler = null;
            this.dragComponents = null;
            this.dragRules = SelectionRules.None;
            if (dragHandler == null)
            {
                throw new InvalidOperationException();
            }
            DesignerTransaction transaction = null;
            try
            {
                IComponent component = dragComponents[0] as IComponent;
                if ((dragComponents.Length > 1) || (((dragComponents.Length == 1) && (component != null)) && (component.Site == null)))
                {
                    transaction = this.host.CreateTransaction(System.Design.SR.GetString("DragDropMoveComponents", new object[] { dragComponents.Length }));
                }
                else if ((dragComponents.Length == 1) && (component != null))
                {
                    transaction = this.host.CreateTransaction(System.Design.SR.GetString("DragDropMoveComponent", new object[] { component.Site.Name }));
                }
                try
                {
                    dragHandler.EndDrag(dragComponents, cancel);
                }
                catch (Exception)
                {
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
                base.Visible = this.savedVisible;
                ((ISelectionUIService) this).SyncSelection();
                if (this.dragTransaction != null)
                {
                    this.dragTransaction.Commit();
                    this.dragTransaction = null;
                }
                this.EndMouseDrag(Control.MousePosition);
            }
        }

        object[] ISelectionUIService.FilterSelection(object[] components, SelectionRules selectionRules)
        {
            object[] objArray = null;
            if (components != null)
            {
                if (selectionRules != SelectionRules.None)
                {
                    ArrayList list = new ArrayList();
                    foreach (object obj2 in components)
                    {
                        SelectionUIItem item = (SelectionUIItem) this.selectionItems[obj2];
                        if (((item != null) && !(item is ContainerSelectionUIItem)) && ((item.GetRules() & selectionRules) == selectionRules))
                        {
                            list.Add(obj2);
                        }
                    }
                    objArray = list.ToArray();
                }
                if (objArray != null)
                {
                    return objArray;
                }
            }
            return new object[0];
        }

        Size ISelectionUIService.GetAdornmentDimensions(AdornmentType adornmentType)
        {
            switch (adornmentType)
            {
                case AdornmentType.GrabHandle:
                    return new Size(7, 7);

                case AdornmentType.ContainerSelector:
                case AdornmentType.Maximum:
                    return new Size(13, 13);
            }
            return new Size(0, 0);
        }

        bool ISelectionUIService.GetAdornmentHitTest(object component, Point value)
        {
            return (this.GetHitTest(value, 3).hitTest != 0x100);
        }

        bool ISelectionUIService.GetContainerSelected(object component)
        {
            return ((component != null) && (this.selectionItems[component] is ContainerSelectionUIItem));
        }

        SelectionRules ISelectionUIService.GetSelectionRules(object component)
        {
            SelectionUIItem item = (SelectionUIItem) this.selectionItems[component];
            if (item == null)
            {
                throw new InvalidOperationException();
            }
            return item.GetRules();
        }

        SelectionStyles ISelectionUIService.GetSelectionStyle(object component)
        {
            SelectionUIItem item = (SelectionUIItem) this.selectionItems[component];
            if (item == null)
            {
                return SelectionStyles.None;
            }
            return item.Style;
        }

        void ISelectionUIService.SetContainerSelected(object component, bool selected)
        {
            if (selected)
            {
                SelectionUIItem item = (SelectionUIItem) this.selectionItems[component];
                if (!(item is ContainerSelectionUIItem))
                {
                    if (item != null)
                    {
                        item.Dispose();
                    }
                    SelectionUIItem item2 = new ContainerSelectionUIItem(this, component);
                    this.selectionItems[component] = item2;
                    this.UpdateWindowRegion();
                    if (item != null)
                    {
                        item.Invalidate();
                    }
                    item2.Invalidate();
                }
            }
            else
            {
                SelectionUIItem item3 = (SelectionUIItem) this.selectionItems[component];
                if ((item3 == null) || (item3 is ContainerSelectionUIItem))
                {
                    this.selectionItems.Remove(component);
                    if (item3 != null)
                    {
                        item3.Dispose();
                    }
                    this.UpdateWindowRegion();
                    item3.Invalidate();
                }
            }
        }

        void ISelectionUIService.SetSelectionStyle(object component, SelectionStyles style)
        {
            SelectionUIItem item = (SelectionUIItem) this.selectionItems[component];
            if ((this.selSvc != null) && this.selSvc.GetComponentSelected(component))
            {
                item = new SelectionUIItem(this, component);
                this.selectionItems[component] = item;
            }
            if (item != null)
            {
                item.Style = style;
                this.UpdateWindowRegion();
                item.Invalidate();
            }
        }

        void ISelectionUIService.SyncComponent(object component)
        {
            if (this.batchMode)
            {
                this.batchSync = true;
            }
            else if (base.IsHandleCreated)
            {
                foreach (SelectionUIItem item in this.selectionItems.Values)
                {
                    item.UpdateRules();
                    item.Dispose();
                }
                this.UpdateWindowRegion();
                base.Invalidate();
                base.Update();
            }
        }

        void ISelectionUIService.SyncSelection()
        {
            if (this.batchMode)
            {
                this.batchChanged = true;
            }
            else if (base.IsHandleCreated)
            {
                bool flag = false;
                foreach (SelectionUIItem item in this.selectionItems.Values)
                {
                    flag |= item.UpdateSize();
                    item.UpdateRules();
                }
                if (flag)
                {
                    this.UpdateWindowRegion();
                    base.Update();
                }
            }
        }

        private void UpdateWindowRegion()
        {
            Region region = new Region(new Rectangle(0, 0, 0, 0));
            foreach (SelectionUIItem item in this.selectionItems.Values)
            {
                region.Union(item.GetRegion());
            }
            base.Region = region;
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x202:
                case 0x205:
                    if (this.mouseDragAnchor != InvalidPoint)
                    {
                        this.ignoreCaptureChanged = true;
                    }
                    break;

                case 0x215:
                    if (!this.ignoreCaptureChanged && (this.mouseDragAnchor != InvalidPoint))
                    {
                        this.EndMouseDrag(Control.MousePosition);
                        if (((ISelectionUIService) this).Dragging)
                        {
                            ((ISelectionUIService) this).EndDrag(true);
                        }
                    }
                    this.ignoreCaptureChanged = false;
                    break;
            }
            base.WndProc(ref m);
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style &= -100663297;
                return createParams;
            }
        }

        bool ISelectionUIService.Dragging
        {
            get
            {
                return (this.dragHandler != null);
            }
        }

        bool ISelectionUIService.Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        private class ContainerSelectionUIItem : SelectionUIService.SelectionUIItem
        {
            public const int CONTAINER_HEIGHT = 13;
            public const int CONTAINER_WIDTH = 13;

            public ContainerSelectionUIItem(SelectionUIService selUIsvc, object component) : base(selUIsvc, component)
            {
            }

            public override void DoPaint(Graphics gr)
            {
                if ((base.GetRules() & SelectionRules.Visible) != SelectionRules.None)
                {
                    Rectangle bounds = new Rectangle(this.outerRect.X, this.outerRect.Y, 13, 13);
                    ControlPaint.DrawContainerGrabHandle(gr, bounds);
                }
            }

            public override Cursor GetCursorAtPoint(Point pt)
            {
                if (((this.GetHitTest(pt) & 0x200) != 0) && ((base.GetRules() & SelectionRules.Moveable) != SelectionRules.None))
                {
                    return Cursors.SizeAll;
                }
                return null;
            }

            public override int GetHitTest(Point pt)
            {
                int num = 0x100;
                if (((base.GetRules() & SelectionRules.Visible) != SelectionRules.None) && !this.outerRect.IsEmpty)
                {
                    Rectangle rectangle = new Rectangle(this.outerRect.X, this.outerRect.Y, 13, 13);
                    if (rectangle.Contains(pt))
                    {
                        num = 0x200;
                        if ((base.GetRules() & SelectionRules.Moveable) != SelectionRules.None)
                        {
                            num |= 12;
                        }
                    }
                }
                return num;
            }

            public override Region GetRegion()
            {
                if (base.region == null)
                {
                    if (((base.GetRules() & SelectionRules.Visible) != SelectionRules.None) && !this.outerRect.IsEmpty)
                    {
                        Rectangle rect = new Rectangle(this.outerRect.X, this.outerRect.Y, 13, 13);
                        base.region = new Region(rect);
                    }
                    else
                    {
                        base.region = new Region(new Rectangle(0, 0, 0, 0));
                    }
                }
                return base.region;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HitTestInfo
        {
            public readonly int hitTest;
            public readonly SelectionUIService.SelectionUIItem selectionUIHit;
            public readonly bool containerSelector;
            public HitTestInfo(int hitTest, SelectionUIService.SelectionUIItem selectionUIHit)
            {
                this.hitTest = hitTest;
                this.selectionUIHit = selectionUIHit;
                this.containerSelector = false;
            }

            public HitTestInfo(int hitTest, SelectionUIService.SelectionUIItem selectionUIHit, bool containerSelector)
            {
                this.hitTest = hitTest;
                this.selectionUIHit = selectionUIHit;
                this.containerSelector = containerSelector;
            }

            public override bool Equals(object obj)
            {
                try
                {
                    SelectionUIService.HitTestInfo info = (SelectionUIService.HitTestInfo) obj;
                    return (((this.hitTest == info.hitTest) && (this.selectionUIHit == info.selectionUIHit)) && (this.containerSelector == info.containerSelector));
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
                return false;
            }

            public static bool operator ==(SelectionUIService.HitTestInfo left, SelectionUIService.HitTestInfo right)
            {
                return (((left.hitTest == right.hitTest) && (left.selectionUIHit == right.selectionUIHit)) && (left.containerSelector == right.containerSelector));
            }

            public static bool operator !=(SelectionUIService.HitTestInfo left, SelectionUIService.HitTestInfo right)
            {
                return !(left == right);
            }

            public override int GetHashCode()
            {
                int num = this.hitTest | this.selectionUIHit.GetHashCode();
                if (this.containerSelector)
                {
                    num |= 0x10000;
                }
                return num;
            }
        }

        private class SelectionUIItem
        {
            internal static readonly Cursor[] activeCursorArrays = new Cursor[] { Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE, Cursors.SizeWE, Cursors.SizeNESW, Cursors.SizeNS, Cursors.SizeNWSE };
            internal static readonly int[] activeSizeArray = new int[] { 0x33, 0x22, 0x63, 0x11, 0x41, 0x93, 130, 0xc3 };
            internal object component;
            public const int CONTAINER_SELECTOR = 0x200;
            private Control control;
            internal Cursor[] cursors;
            public const int GRABHANDLE_HEIGHT = 7;
            public const int GRABHANDLE_WIDTH = 7;
            private ISelectionUIHandler handler;
            internal static readonly Cursor[] inactiveCursorArray = new Cursor[] { Cursors.Arrow, Cursors.Arrow, Cursors.Arrow, Cursors.Arrow, Cursors.Arrow, Cursors.Arrow, Cursors.Arrow, Cursors.Arrow };
            internal static readonly int[] inactiveSizeArray = new int[8];
            internal Rectangle innerRect = Rectangle.Empty;
            public const int MOVE_MASK = 12;
            public const int MOVE_X = 4;
            public const int MOVE_Y = 8;
            public const int NOHIT = 0x100;
            internal Rectangle outerRect = Rectangle.Empty;
            public const int POS_BOTTOM = 0x80;
            public const int POS_LEFT = 0x10;
            public const int POS_MASK = 240;
            public const int POS_RIGHT = 0x40;
            public const int POS_TOP = 0x20;
            internal Region region;
            private SelectionRules selectionRules;
            private SelectionStyles selectionStyle;
            internal SelectionUIService selUIsvc;
            public const int SIZE_MASK = 3;
            public const int SIZE_X = 1;
            public const int SIZE_Y = 2;
            internal int[] sizes;

            public SelectionUIItem(SelectionUIService selUIsvc, object component)
            {
                this.selUIsvc = selUIsvc;
                this.component = component;
                this.selectionStyle = SelectionStyles.Selected;
                this.handler = selUIsvc.GetHandler(component);
                this.sizes = inactiveSizeArray;
                this.cursors = inactiveCursorArray;
                IComponent component2 = component as IComponent;
                if (component2 != null)
                {
                    ControlDesigner designer = selUIsvc.host.GetDesigner(component2) as ControlDesigner;
                    if (designer != null)
                    {
                        this.control = designer.Control;
                    }
                }
                this.UpdateRules();
                this.UpdateGrabSettings();
                this.UpdateSize();
            }

            public void Dispose()
            {
                if (this.region != null)
                {
                    this.region.Dispose();
                    this.region = null;
                }
            }

            public virtual void DoPaint(Graphics gr)
            {
                if ((this.GetRules() & SelectionRules.Visible) != SelectionRules.None)
                {
                    bool primary = false;
                    if (this.selUIsvc.selSvc != null)
                    {
                        primary = this.component == this.selUIsvc.selSvc.PrimarySelection;
                        primary = primary == (this.selUIsvc.selSvc.SelectionCount <= 1);
                    }
                    Rectangle rectangle = new Rectangle(this.outerRect.X, this.outerRect.Y, 7, 7);
                    Rectangle innerRect = this.innerRect;
                    Rectangle outerRect = this.outerRect;
                    Region clip = gr.Clip;
                    System.Drawing.Color control = SystemColors.Control;
                    if ((this.control != null) && (this.control.Parent != null))
                    {
                        control = this.control.Parent.BackColor;
                    }
                    Brush brush = new SolidBrush(control);
                    gr.ExcludeClip(innerRect);
                    gr.FillRectangle(brush, outerRect);
                    brush.Dispose();
                    gr.Clip = clip;
                    ControlPaint.DrawSelectionFrame(gr, false, outerRect, innerRect, control);
                    if (((this.GetRules() & (SelectionRules.None | SelectionRules.Locked)) == SelectionRules.None) && ((this.GetRules() & SelectionRules.AllSizeable) != SelectionRules.None))
                    {
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[0] != 0);
                        rectangle.X = innerRect.X + innerRect.Width;
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[2] != 0);
                        rectangle.Y = innerRect.Y + innerRect.Height;
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[7] != 0);
                        rectangle.X = outerRect.X;
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[5] != 0);
                        rectangle.X += (outerRect.Width - 7) / 2;
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[6] != 0);
                        rectangle.Y = outerRect.Y;
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[1] != 0);
                        rectangle.X = outerRect.X;
                        rectangle.Y = innerRect.Y + ((innerRect.Height - 7) / 2);
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[3] != 0);
                        rectangle.X = innerRect.X + innerRect.Width;
                        ControlPaint.DrawGrabHandle(gr, rectangle, primary, this.sizes[4] != 0);
                    }
                    else
                    {
                        ControlPaint.DrawLockedFrame(gr, outerRect, primary);
                    }
                }
            }

            public virtual Cursor GetCursorAtPoint(Point pt)
            {
                Cursor cursor = null;
                if (!this.PointWithinSelection(pt))
                {
                    return cursor;
                }
                int index = -1;
                if ((this.GetRules() & SelectionRules.AllSizeable) != SelectionRules.None)
                {
                    index = this.GetHandleIndexOfPoint(pt);
                }
                if (-1 == index)
                {
                    if ((this.GetRules() & SelectionRules.Moveable) == SelectionRules.None)
                    {
                        return Cursors.Default;
                    }
                    return Cursors.SizeAll;
                }
                return this.cursors[index];
            }

            private int GetHandleIndexOfPoint(Point pt)
            {
                if ((pt.X >= this.outerRect.X) && (pt.X <= this.innerRect.X))
                {
                    if ((pt.Y >= this.outerRect.Y) && (pt.Y <= this.innerRect.Y))
                    {
                        return 0;
                    }
                    if ((pt.Y >= (this.innerRect.Y + this.innerRect.Height)) && (pt.Y <= (this.outerRect.Y + this.outerRect.Height)))
                    {
                        return 5;
                    }
                    if ((pt.Y >= (this.outerRect.Y + ((this.outerRect.Height - 7) / 2))) && (pt.Y <= (this.outerRect.Y + ((this.outerRect.Height + 7) / 2))))
                    {
                        return 3;
                    }
                    return -1;
                }
                if ((pt.Y >= this.outerRect.Y) && (pt.Y <= this.innerRect.Y))
                {
                    if ((pt.X >= (this.innerRect.X + this.innerRect.Width)) && (pt.X <= (this.outerRect.X + this.outerRect.Width)))
                    {
                        return 2;
                    }
                    if ((pt.X >= (this.outerRect.X + ((this.outerRect.Width - 7) / 2))) && (pt.X <= (this.outerRect.X + ((this.outerRect.Width + 7) / 2))))
                    {
                        return 1;
                    }
                    return -1;
                }
                if ((pt.X >= (this.innerRect.X + this.innerRect.Width)) && (pt.X <= (this.outerRect.X + this.outerRect.Width)))
                {
                    if ((pt.Y >= (this.innerRect.Y + this.innerRect.Height)) && (pt.Y <= (this.outerRect.Y + this.outerRect.Height)))
                    {
                        return 7;
                    }
                    if ((pt.Y >= (this.outerRect.Y + ((this.outerRect.Height - 7) / 2))) && (pt.Y <= (this.outerRect.Y + ((this.outerRect.Height + 7) / 2))))
                    {
                        return 4;
                    }
                    return -1;
                }
                if (((pt.Y >= (this.innerRect.Y + this.innerRect.Height)) && (pt.Y <= (this.outerRect.Y + this.outerRect.Height))) && ((pt.X >= (this.outerRect.X + ((this.outerRect.Width - 7) / 2))) && (pt.X <= (this.outerRect.X + ((this.outerRect.Width + 7) / 2)))))
                {
                    return 6;
                }
                return -1;
            }

            public virtual int GetHitTest(Point pt)
            {
                if (!this.PointWithinSelection(pt))
                {
                    return 0x100;
                }
                int handleIndexOfPoint = this.GetHandleIndexOfPoint(pt);
                if ((-1 != handleIndexOfPoint) && (this.sizes[handleIndexOfPoint] != 0))
                {
                    return this.sizes[handleIndexOfPoint];
                }
                if ((this.GetRules() & SelectionRules.Moveable) != SelectionRules.None)
                {
                    return 12;
                }
                return 0;
            }

            public virtual Region GetRegion()
            {
                if (this.region == null)
                {
                    if (((this.GetRules() & SelectionRules.Visible) != SelectionRules.None) && !this.outerRect.IsEmpty)
                    {
                        this.region = new Region(this.outerRect);
                        this.region.Exclude(this.innerRect);
                    }
                    else
                    {
                        this.region = new Region(new Rectangle(0, 0, 0, 0));
                    }
                    if (this.handler != null)
                    {
                        Rectangle selectionClipRect = this.handler.GetSelectionClipRect(this.component);
                        if (!selectionClipRect.IsEmpty)
                        {
                            this.region.Intersect(this.selUIsvc.RectangleToClient(selectionClipRect));
                        }
                    }
                }
                return this.region;
            }

            public SelectionRules GetRules()
            {
                return this.selectionRules;
            }

            public void Invalidate()
            {
                if (!this.outerRect.IsEmpty && !this.selUIsvc.Disposing)
                {
                    this.selUIsvc.Invalidate(this.outerRect);
                }
            }

            protected bool PointWithinSelection(Point pt)
            {
                if ((((this.GetRules() & SelectionRules.Visible) == SelectionRules.None) || this.outerRect.IsEmpty) || this.innerRect.IsEmpty)
                {
                    return false;
                }
                if ((pt.X < this.outerRect.X) || (pt.X > (this.outerRect.X + this.outerRect.Width)))
                {
                    return false;
                }
                if ((pt.Y < this.outerRect.Y) || (pt.Y > (this.outerRect.Y + this.outerRect.Height)))
                {
                    return false;
                }
                if (((pt.X > this.innerRect.X) && (pt.X < (this.innerRect.X + this.innerRect.Width))) && ((pt.Y > this.innerRect.Y) && (pt.Y < (this.innerRect.Y + this.innerRect.Height))))
                {
                    return false;
                }
                return true;
            }

            private void UpdateGrabSettings()
            {
                SelectionRules rules = this.GetRules();
                if ((rules & SelectionRules.AllSizeable) == SelectionRules.None)
                {
                    this.sizes = inactiveSizeArray;
                    this.cursors = inactiveCursorArray;
                }
                else
                {
                    this.sizes = new int[8];
                    this.cursors = new Cursor[8];
                    Array.Copy(activeCursorArrays, this.cursors, this.cursors.Length);
                    Array.Copy(activeSizeArray, this.sizes, this.sizes.Length);
                    if ((rules & SelectionRules.TopSizeable) != SelectionRules.TopSizeable)
                    {
                        this.sizes[0] = 0;
                        this.sizes[1] = 0;
                        this.sizes[2] = 0;
                        this.cursors[0] = Cursors.Arrow;
                        this.cursors[1] = Cursors.Arrow;
                        this.cursors[2] = Cursors.Arrow;
                    }
                    if ((rules & SelectionRules.LeftSizeable) != SelectionRules.LeftSizeable)
                    {
                        this.sizes[0] = 0;
                        this.sizes[3] = 0;
                        this.sizes[5] = 0;
                        this.cursors[0] = Cursors.Arrow;
                        this.cursors[3] = Cursors.Arrow;
                        this.cursors[5] = Cursors.Arrow;
                    }
                    if ((rules & SelectionRules.BottomSizeable) != SelectionRules.BottomSizeable)
                    {
                        this.sizes[5] = 0;
                        this.sizes[6] = 0;
                        this.sizes[7] = 0;
                        this.cursors[5] = Cursors.Arrow;
                        this.cursors[6] = Cursors.Arrow;
                        this.cursors[7] = Cursors.Arrow;
                    }
                    if ((rules & SelectionRules.RightSizeable) != SelectionRules.RightSizeable)
                    {
                        this.sizes[2] = 0;
                        this.sizes[4] = 0;
                        this.sizes[7] = 0;
                        this.cursors[2] = Cursors.Arrow;
                        this.cursors[4] = Cursors.Arrow;
                        this.cursors[7] = Cursors.Arrow;
                    }
                }
            }

            public void UpdateRules()
            {
                if (this.handler == null)
                {
                    this.selectionRules = SelectionRules.None;
                }
                else
                {
                    SelectionRules selectionRules = this.selectionRules;
                    this.selectionRules = this.handler.GetComponentRules(this.component);
                    if (this.selectionRules != selectionRules)
                    {
                        this.UpdateGrabSettings();
                        this.Invalidate();
                    }
                }
            }

            public virtual bool UpdateSize()
            {
                bool flag = false;
                if (this.handler == null)
                {
                    return false;
                }
                if ((this.GetRules() & SelectionRules.Visible) == SelectionRules.None)
                {
                    return false;
                }
                this.innerRect = this.handler.GetComponentBounds(this.component);
                if (!this.innerRect.IsEmpty)
                {
                    this.innerRect = this.selUIsvc.RectangleToClient(this.innerRect);
                    Rectangle rectangle = new Rectangle(this.innerRect.X - 7, this.innerRect.Y - 7, this.innerRect.Width + 14, this.innerRect.Height + 14);
                    if (!this.outerRect.IsEmpty && this.outerRect.Equals(rectangle))
                    {
                        return flag;
                    }
                    if (!this.outerRect.IsEmpty)
                    {
                        this.Invalidate();
                    }
                    this.outerRect = rectangle;
                    this.Invalidate();
                    if (this.region != null)
                    {
                        this.region.Dispose();
                        this.region = null;
                    }
                    return true;
                }
                Rectangle rectangle2 = new Rectangle(0, 0, 0, 0);
                flag = this.outerRect.IsEmpty || !this.outerRect.Equals(rectangle2);
                this.innerRect = this.outerRect = rectangle2;
                return flag;
            }

            public virtual SelectionStyles Style
            {
                get
                {
                    return this.selectionStyle;
                }
                set
                {
                    if (value != this.selectionStyle)
                    {
                        this.selectionStyle = value;
                        if (this.region != null)
                        {
                            this.region.Dispose();
                            this.region = null;
                        }
                    }
                }
            }
        }
    }
}

