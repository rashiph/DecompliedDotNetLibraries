namespace System.Windows.Forms.Design
{
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class OleDragDropHandler
    {
        protected const int AllowLocalMoveOnly = 0x4000000;
        public const string CF_CODE = "CF_XMLCODE";
        public const string CF_COMPONENTTYPES = "CF_COMPONENTTYPES";
        public const string CF_TOOLBOXITEM = "CF_NESTEDTOOLBOXITEM";
        private IOleDragClient client;
        private static CodeMarkers codemarkers = CodeMarkers.Instance;
        private static Hashtable currentDrags;
        private Point dragBase = Point.Empty;
        private object[] dragComps;
        private bool dragOk;
        private bool forceDrawFrames;
        private static bool freezePainting = false;
        private bool localDrag;
        private DragDropEffects localDragEffect;
        private bool localDragInside;
        private Point localDragOffset = Point.Empty;
        private SelectionUIHandler selectionHandler;
        private IServiceProvider serviceProvider;

        public OleDragDropHandler(SelectionUIHandler selectionHandler, IServiceProvider serviceProvider, IOleDragClient client)
        {
            this.serviceProvider = serviceProvider;
            this.selectionHandler = selectionHandler;
            this.client = client;
        }

        private static void AddCurrentDrag(IDataObject data, IComponent component)
        {
            if (currentDrags == null)
            {
                currentDrags = new Hashtable();
            }
            currentDrags[data] = component;
        }

        protected virtual bool CanDropDataObject(IDataObject dataObj)
        {
            if (dataObj != null)
            {
                if (dataObj is ComponentDataObjectWrapper)
                {
                    object[] draggingObjects = this.GetDraggingObjects(dataObj, true);
                    if (draggingObjects == null)
                    {
                        return false;
                    }
                    bool flag = true;
                    for (int i = 0; flag && (i < draggingObjects.Length); i++)
                    {
                        flag = (flag && (draggingObjects[i] is IComponent)) && this.client.IsDropOk((IComponent) draggingObjects[i]);
                    }
                    return flag;
                }
                try
                {
                    object data = dataObj.GetData(DataFormat, false);
                    if (data == null)
                    {
                        return false;
                    }
                    IDesignerSerializationService service = (IDesignerSerializationService) this.GetService(typeof(IDesignerSerializationService));
                    if (service == null)
                    {
                        return false;
                    }
                    ICollection is2 = service.Deserialize(data);
                    if (is2.Count > 0)
                    {
                        bool flag2 = true;
                        foreach (object obj3 in is2)
                        {
                            if (obj3 is IComponent)
                            {
                                flag2 = flag2 && this.client.IsDropOk((IComponent) obj3);
                                if (!flag2)
                                {
                                    return flag2;
                                }
                            }
                        }
                        return flag2;
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
            return false;
        }

        public IComponent[] CreateTool(ToolboxItem tool, Control parent, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
            return this.CreateTool(tool, parent, x, y, width, height, hasLocation, hasSize, null);
        }

        public IComponent[] CreateTool(ToolboxItem tool, Control parent, int x, int y, int width, int height, bool hasLocation, bool hasSize, ToolboxSnapDragDropEventArgs e)
        {
            IToolboxService service = (IToolboxService) this.GetService(typeof(IToolboxService));
            ISelectionService service2 = (ISelectionService) this.GetService(typeof(ISelectionService));
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            IComponent[] c = new IComponent[0];
            Cursor current = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            DesignerTransaction transaction = null;
            try
            {
                try
                {
                    if (host != null)
                    {
                        transaction = host.CreateTransaction(System.Design.SR.GetString("DesignerBatchCreateTool", new object[] { tool.ToString() }));
                    }
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw exception;
                    }
                    return c;
                }
                try
                {
                    try
                    {
                        if ((host != null) && this.CurrentlyLocalizing(host.RootComponent))
                        {
                            IUIService service3 = (IUIService) this.GetService(typeof(IUIService));
                            if (service3 != null)
                            {
                                service3.ShowMessage(System.Design.SR.GetString("LocalizingCannotAdd"));
                            }
                            return new IComponent[0];
                        }
                        Hashtable defaultValues = new Hashtable();
                        if (parent != null)
                        {
                            defaultValues["Parent"] = parent;
                        }
                        if ((parent != null) && parent.IsMirrored)
                        {
                            x += width;
                        }
                        if (hasLocation)
                        {
                            defaultValues["Location"] = new Point(x, y);
                        }
                        if (hasSize)
                        {
                            defaultValues["Size"] = new Size(width, height);
                        }
                        if (e != null)
                        {
                            defaultValues["ToolboxSnapDragDropEventArgs"] = e;
                        }
                        c = tool.CreateComponents(host, defaultValues);
                    }
                    catch (CheckoutException exception2)
                    {
                        if (exception2 != CheckoutException.Canceled)
                        {
                            throw;
                        }
                        c = new IComponent[0];
                    }
                    catch (ArgumentException exception3)
                    {
                        IUIService service4 = (IUIService) this.GetService(typeof(IUIService));
                        if (service4 != null)
                        {
                            service4.ShowError(exception3);
                        }
                    }
                    catch (Exception exception4)
                    {
                        IUIService service5 = (IUIService) this.GetService(typeof(IUIService));
                        string message = string.Empty;
                        if (exception4.InnerException != null)
                        {
                            message = exception4.InnerException.ToString();
                        }
                        if (string.IsNullOrEmpty(message))
                        {
                            message = exception4.ToString();
                        }
                        if (exception4 is InvalidOperationException)
                        {
                            message = exception4.Message;
                        }
                        if (service5 == null)
                        {
                            throw;
                        }
                        service5.ShowError(exception4, System.Design.SR.GetString("FailedToCreateComponent", new object[] { tool.DisplayName, message }));
                    }
                    if (c == null)
                    {
                        c = new IComponent[0];
                    }
                }
                finally
                {
                    if ((service != null) && tool.Equals(service.GetSelectedToolboxItem(host)))
                    {
                        service.SelectedToolboxItemUsed();
                    }
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Commit();
                }
                Cursor.Current = current;
            }
            if ((service2 != null) && (c.Length > 0))
            {
                if (host != null)
                {
                    host.Activate();
                }
                ArrayList list = new ArrayList(c);
                for (int i = 0; i < c.Length; i++)
                {
                    if (!TypeDescriptor.GetAttributes(c[i]).Contains(DesignTimeVisibleAttribute.Yes))
                    {
                        list.Remove(c[i]);
                    }
                }
                service2.SetSelectedComponents(list.ToArray(), SelectionTypes.Replace);
            }
            codemarkers.CodeMarker(CodeMarkerEvent.perfFXDesignCreateComponentEnd);
            return c;
        }

        private bool CurrentlyLocalizing(IComponent rootComponent)
        {
            if (rootComponent != null)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(rootComponent)["Language"];
                if ((descriptor != null) && (descriptor.PropertyType == typeof(CultureInfo)))
                {
                    CultureInfo info = (CultureInfo) descriptor.GetValue(rootComponent);
                    if (!info.Equals(CultureInfo.InvariantCulture))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void DisableDragDropChildren(ICollection controls, ArrayList allowDropCache)
        {
            foreach (Control control in controls)
            {
                if (control != null)
                {
                    if (control.AllowDrop)
                    {
                        allowDropCache.Add(control);
                        control.AllowDrop = false;
                    }
                    if (control.HasChildren)
                    {
                        this.DisableDragDropChildren(control.Controls, allowDropCache);
                    }
                }
            }
        }

        public bool DoBeginDrag(object[] components, SelectionRules rules, int initialX, int initialY)
        {
            if (((rules & SelectionRules.AllSizeable) != SelectionRules.None) || (Control.MouseButtons == MouseButtons.None))
            {
                return true;
            }
            Control designerControl = this.client.GetDesignerControl();
            this.localDrag = true;
            this.localDragInside = false;
            this.dragComps = components;
            this.dragBase = new Point(initialX, initialY);
            this.localDragOffset = Point.Empty;
            designerControl.PointToClient(new Point(initialX, initialY));
            DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Copy;
            for (int i = 0; i < components.Length; i++)
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(components[i])[typeof(InheritanceAttribute)];
                if (!attribute.Equals(InheritanceAttribute.NotInherited) && !attribute.Equals(InheritanceAttribute.InheritedReadOnly))
                {
                    allowedEffects &= ~DragDropEffects.Move;
                    allowedEffects |= 0x4000000;
                }
            }
            DataObject data = new ComponentDataObjectWrapper(new ComponentDataObject(this.client, this.serviceProvider, components, initialX, initialY));
            System.Design.NativeMethods.MSG msg = new System.Design.NativeMethods.MSG();
            while (System.Design.NativeMethods.PeekMessage(ref msg, IntPtr.Zero, 15, 15, 1))
            {
                System.Design.NativeMethods.TranslateMessage(ref msg);
                System.Design.NativeMethods.DispatchMessage(ref msg);
            }
            bool freezePainting = OleDragDropHandler.freezePainting;
            AddCurrentDrag(data, this.client.Component);
            ArrayList allowDropCache = new ArrayList();
            foreach (object obj3 in components)
            {
                Control control2 = obj3 as Control;
                if ((control2 != null) && control2.HasChildren)
                {
                    this.DisableDragDropChildren(control2.Controls, allowDropCache);
                }
            }
            DragDropEffects none = DragDropEffects.None;
            IDesignerHost host = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DesignerTransaction transaction = null;
            if (host != null)
            {
                transaction = host.CreateTransaction(System.Design.SR.GetString("DragDropDragComponents", new object[] { components.Length }));
            }
            try
            {
                none = designerControl.DoDragDrop(data, allowedEffects);
                if (transaction != null)
                {
                    transaction.Commit();
                }
            }
            finally
            {
                RemoveCurrentDrag(data);
                foreach (Control control3 in allowDropCache)
                {
                    control3.AllowDrop = true;
                }
                OleDragDropHandler.freezePainting = freezePainting;
                if (transaction != null)
                {
                    ((IDisposable) transaction).Dispose();
                }
            }
            bool flag3 = (((none & DragDropEffects.Move) != DragDropEffects.None) || ((none & 0x4000000) != DragDropEffects.None)) && this.localDragInside;
            ISelectionUIService service = (ISelectionUIService) this.GetService(typeof(ISelectionUIService));
            if ((service != null) && service.Dragging)
            {
                service.EndDrag(!flag3);
            }
            if (!this.localDragOffset.IsEmpty && (none != DragDropEffects.None))
            {
                this.DrawDragFrames(this.dragComps, this.localDragOffset, this.localDragEffect, Point.Empty, DragDropEffects.None, false);
            }
            this.localDragOffset = Point.Empty;
            this.dragComps = null;
            this.localDrag = this.localDragInside = false;
            this.dragBase = Point.Empty;
            return false;
        }

        public void DoEndDrag(object[] components, bool cancel)
        {
            this.dragComps = null;
            this.localDrag = false;
            this.localDragInside = false;
        }

        public void DoOleDragDrop(DragEventArgs de)
        {
            freezePainting = false;
            if (this.selectionHandler == null)
            {
                de.Effect = DragDropEffects.None;
            }
            else if (((this.localDrag && (de.X == this.dragBase.X)) && (de.Y == this.dragBase.Y)) || ((de.AllowedEffect == DragDropEffects.None) || (!this.localDrag && !this.dragOk)))
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                bool flag = ((de.AllowedEffect & 0x4000000) != DragDropEffects.None) && this.localDragInside;
                bool flag2 = ((de.AllowedEffect & DragDropEffects.Move) != DragDropEffects.None) || flag;
                bool flag3 = (de.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.None;
                if (((de.Effect & DragDropEffects.Move) != DragDropEffects.None) && !flag2)
                {
                    de.Effect = DragDropEffects.Copy;
                }
                if (((de.Effect & DragDropEffects.Copy) != DragDropEffects.None) && !flag3)
                {
                    de.Effect = DragDropEffects.None;
                }
                else
                {
                    if (flag && ((de.Effect & DragDropEffects.Move) != DragDropEffects.None))
                    {
                        de.Effect |= 0x4000002;
                    }
                    else if ((de.Effect & DragDropEffects.Copy) != DragDropEffects.None)
                    {
                        de.Effect = DragDropEffects.Copy;
                    }
                    if (this.forceDrawFrames || this.localDragInside)
                    {
                        this.localDragOffset = this.DrawDragFrames(this.dragComps, this.localDragOffset, this.localDragEffect, Point.Empty, DragDropEffects.None, this.forceDrawFrames);
                        this.forceDrawFrames = false;
                    }
                    Cursor current = Cursor.Current;
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        if (this.dragOk || (this.localDragInside && (de.Effect == DragDropEffects.Copy)))
                        {
                            object[] components;
                            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                            IContainer container = service.RootComponent.Site.Container;
                            IDataObject data = de.Data;
                            bool flag4 = false;
                            if (data is ComponentDataObjectWrapper)
                            {
                                ComponentDataObject innerData = ((ComponentDataObjectWrapper) data).InnerData;
                                IComponent dragOwnerComponent = this.GetDragOwnerComponent(de.Data);
                                bool flag5 = ((dragOwnerComponent == null) || (this.client.Component == null)) || (dragOwnerComponent.Site.Container != this.client.Component.Site.Container);
                                bool flag6 = false;
                                if ((de.Effect == DragDropEffects.Copy) || flag5)
                                {
                                    innerData.Deserialize(this.serviceProvider, (de.Effect & DragDropEffects.Copy) == DragDropEffects.None);
                                }
                                else
                                {
                                    flag6 = true;
                                }
                                flag4 = true;
                                components = innerData.Components;
                                if (flag6)
                                {
                                    components = this.GetTopLevelComponents(components);
                                }
                            }
                            else
                            {
                                object serializationData = data.GetData(DataFormat, true);
                                if (serializationData == null)
                                {
                                    components = new IComponent[0];
                                }
                                else
                                {
                                    data = new ComponentDataObject(this.client, this.serviceProvider, serializationData);
                                    components = ((ComponentDataObject) data).Components;
                                    flag4 = true;
                                }
                            }
                            if ((components != null) && (components.Length > 0))
                            {
                                IComponent component = null;
                                DesignerTransaction transaction = null;
                                try
                                {
                                    transaction = service.CreateTransaction(System.Design.SR.GetString("DragDropDropComponents"));
                                    if (!this.localDrag)
                                    {
                                        service.Activate();
                                    }
                                    ArrayList list = new ArrayList();
                                    for (int i = 0; i < components.Length; i++)
                                    {
                                        component = components[i] as IComponent;
                                        if (component == null)
                                        {
                                            component = null;
                                        }
                                        else
                                        {
                                            try
                                            {
                                                string name = null;
                                                if (component.Site != null)
                                                {
                                                    name = component.Site.Name;
                                                }
                                                Control designerControl = null;
                                                if (flag4)
                                                {
                                                    designerControl = this.client.GetDesignerControl();
                                                    System.Design.NativeMethods.SendMessage(designerControl.Handle, 11, 0, 0);
                                                }
                                                Point snappedPoint = this.client.GetDesignerControl().PointToClient(new Point(de.X, de.Y));
                                                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["TrayLocation"];
                                                if (descriptor == null)
                                                {
                                                    descriptor = TypeDescriptor.GetProperties(component)["Location"];
                                                }
                                                if ((descriptor != null) && !descriptor.IsReadOnly)
                                                {
                                                    Rectangle dragRect = new Rectangle();
                                                    Point point2 = (Point) descriptor.GetValue(component);
                                                    dragRect.X = snappedPoint.X + point2.X;
                                                    dragRect.Y = snappedPoint.Y + point2.Y;
                                                    dragRect = this.selectionHandler.GetUpdatedRect(Rectangle.Empty, dragRect, false);
                                                }
                                                if (!this.client.AddComponent(component, name, false))
                                                {
                                                    de.Effect = DragDropEffects.None;
                                                }
                                                else if (this.client.GetControlForComponent(component) == null)
                                                {
                                                    flag4 = false;
                                                }
                                                if (flag4)
                                                {
                                                    ParentControlDesigner client = this.client as ParentControlDesigner;
                                                    if (client != null)
                                                    {
                                                        Control controlForComponent = this.client.GetControlForComponent(component);
                                                        snappedPoint = client.GetSnappedPoint(controlForComponent.Location);
                                                        controlForComponent.Location = snappedPoint;
                                                    }
                                                }
                                                if (designerControl != null)
                                                {
                                                    System.Design.NativeMethods.SendMessage(designerControl.Handle, 11, 1, 0);
                                                    designerControl.Invalidate(true);
                                                }
                                                if (TypeDescriptor.GetAttributes(component).Contains(DesignTimeVisibleAttribute.Yes))
                                                {
                                                    list.Add(component);
                                                }
                                            }
                                            catch (CheckoutException exception)
                                            {
                                                if (exception != CheckoutException.Canceled)
                                                {
                                                    throw;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                    if (service != null)
                                    {
                                        service.Activate();
                                    }
                                    ((ISelectionService) this.GetService(typeof(ISelectionService))).SetSelectedComponents((object[]) list.ToArray(typeof(IComponent)), SelectionTypes.Replace);
                                    this.localDragInside = false;
                                }
                                finally
                                {
                                    if (transaction != null)
                                    {
                                        transaction.Commit();
                                    }
                                }
                            }
                        }
                        if (this.localDragInside)
                        {
                            ISelectionUIService service2 = (ISelectionUIService) this.GetService(typeof(ISelectionUIService));
                            if (((service2 != null) && service2.Dragging) && flag2)
                            {
                                Rectangle offset = new Rectangle(de.X - this.dragBase.X, de.Y - this.dragBase.Y, 0, 0);
                                service2.DragMoved(offset);
                            }
                        }
                        this.dragOk = false;
                    }
                    finally
                    {
                        Cursor.Current = current;
                    }
                }
            }
        }

        public void DoOleDragEnter(DragEventArgs de)
        {
            if ((!this.localDrag && this.CanDropDataObject(de.Data)) && (de.AllowedEffect != DragDropEffects.None))
            {
                if (this.client.CanModifyComponents)
                {
                    this.dragOk = true;
                    if (((de.KeyState & 8) != 0) && ((de.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.None))
                    {
                        de.Effect = DragDropEffects.Copy;
                    }
                    else if ((de.AllowedEffect & DragDropEffects.Move) != DragDropEffects.None)
                    {
                        de.Effect = DragDropEffects.Move;
                    }
                    else
                    {
                        de.Effect = DragDropEffects.None;
                    }
                }
            }
            else if (this.localDrag && (de.AllowedEffect != DragDropEffects.None))
            {
                this.localDragInside = true;
                if ((((de.KeyState & 8) != 0) && ((de.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.None)) && this.client.CanModifyComponents)
                {
                    de.Effect = DragDropEffects.Copy;
                }
                if (((de.AllowedEffect & 0x4000000) != DragDropEffects.None) && this.localDragInside)
                {
                    de.Effect |= 0x4000000;
                }
                if ((de.AllowedEffect & DragDropEffects.Move) != DragDropEffects.None)
                {
                    de.Effect |= DragDropEffects.Move;
                }
            }
            else
            {
                de.Effect = DragDropEffects.None;
            }
        }

        public void DoOleDragLeave()
        {
            if (this.localDrag || this.forceDrawFrames)
            {
                this.localDragInside = false;
                this.localDragOffset = this.DrawDragFrames(this.dragComps, this.localDragOffset, this.localDragEffect, Point.Empty, DragDropEffects.None, this.forceDrawFrames);
                if (this.forceDrawFrames && this.dragOk)
                {
                    this.dragBase = Point.Empty;
                    this.dragComps = null;
                }
                this.forceDrawFrames = false;
            }
            this.dragOk = false;
        }

        public void DoOleDragOver(DragEventArgs de)
        {
            if (!this.localDrag && !this.dragOk)
            {
                de.Effect = DragDropEffects.None;
            }
            else
            {
                bool flag = (((de.KeyState & 8) != 0) && ((de.AllowedEffect & DragDropEffects.Copy) != DragDropEffects.None)) && this.client.CanModifyComponents;
                bool flag2 = ((de.AllowedEffect & 0x4000000) != DragDropEffects.None) && this.localDragInside;
                bool flag3 = ((de.AllowedEffect & DragDropEffects.Move) != DragDropEffects.None) || flag2;
                if ((flag || flag3) && (this.localDrag || this.forceDrawFrames))
                {
                    Point empty = Point.Empty;
                    Point pt = this.client.GetDesignerControl().PointToClient(new Point(de.X, de.Y));
                    if (this.forceDrawFrames)
                    {
                        empty = pt;
                    }
                    else
                    {
                        empty = new Point(de.X - this.dragBase.X, de.Y - this.dragBase.Y);
                    }
                    if (!this.client.GetDesignerControl().ClientRectangle.Contains(pt))
                    {
                        flag = false;
                        flag3 = false;
                        empty = this.localDragOffset;
                    }
                    if (empty != this.localDragOffset)
                    {
                        this.DrawDragFrames(this.dragComps, this.localDragOffset, this.localDragEffect, empty, de.Effect, this.forceDrawFrames);
                        this.localDragOffset = empty;
                        this.localDragEffect = de.Effect;
                    }
                }
                if (flag)
                {
                    de.Effect = DragDropEffects.Copy;
                }
                else if (flag3)
                {
                    de.Effect = DragDropEffects.Move;
                }
                else
                {
                    de.Effect = DragDropEffects.None;
                }
                if (flag2)
                {
                    de.Effect |= 0x4000000;
                }
            }
        }

        public void DoOleGiveFeedback(GiveFeedbackEventArgs e)
        {
            SelectionUIHandler selectionHandler = this.selectionHandler;
            e.UseDefaultCursors = ((!this.localDragInside && !this.forceDrawFrames) || ((e.Effect & DragDropEffects.Copy) != DragDropEffects.None)) || (e.Effect == DragDropEffects.None);
            if (!e.UseDefaultCursors && (this.selectionHandler != null))
            {
                this.selectionHandler.SetCursor();
            }
        }

        private Point DrawDragFrames(object[] comps, Point oldOffset, DragDropEffects oldEffect, Point newOffset, DragDropEffects newEffect, bool drawAtNewOffset)
        {
            Rectangle empty = Rectangle.Empty;
            Control designerControl = this.client.GetDesignerControl();
            if (this.selectionHandler == null)
            {
                return Point.Empty;
            }
            if (comps == null)
            {
                return Point.Empty;
            }
            for (int i = 0; i < comps.Length; i++)
            {
                Control controlForComponent = this.client.GetControlForComponent(comps[i]);
                Color control = SystemColors.Control;
                try
                {
                    control = controlForComponent.BackColor;
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
                bool isReadOnly = true;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(comps[i])["Location"];
                if (descriptor != null)
                {
                    isReadOnly = descriptor.IsReadOnly;
                }
                if (!oldOffset.IsEmpty && (((oldEffect & DragDropEffects.Move) == DragDropEffects.None) || !isReadOnly))
                {
                    empty = controlForComponent.Bounds;
                    if (drawAtNewOffset)
                    {
                        empty.X = oldOffset.X;
                        empty.Y = oldOffset.Y;
                    }
                    else
                    {
                        empty.Offset(oldOffset.X, oldOffset.Y);
                    }
                    empty = this.selectionHandler.GetUpdatedRect(controlForComponent.Bounds, empty, false);
                    this.DrawReversibleFrame(designerControl.Handle, empty, control);
                }
                if (!newOffset.IsEmpty && (((oldEffect & DragDropEffects.Move) == DragDropEffects.None) || !isReadOnly))
                {
                    empty = controlForComponent.Bounds;
                    if (drawAtNewOffset)
                    {
                        empty.X = newOffset.X;
                        empty.Y = newOffset.Y;
                    }
                    else
                    {
                        empty.Offset(newOffset.X, newOffset.Y);
                    }
                    empty = this.selectionHandler.GetUpdatedRect(controlForComponent.Bounds, empty, false);
                    this.DrawReversibleFrame(designerControl.Handle, empty, control);
                }
            }
            return newOffset;
        }

        private void DrawReversibleFrame(IntPtr handle, Rectangle rectangle, Color backColor)
        {
            int num;
            Color white;
            if (rectangle.Width == 0)
            {
                rectangle.Width = 5;
            }
            if (rectangle.Height == 0)
            {
                rectangle.Height = 5;
            }
            if (backColor.GetBrightness() < 0.5)
            {
                num = 10;
                white = Color.White;
            }
            else
            {
                num = 7;
                white = Color.Black;
            }
            IntPtr dC = System.Design.UnsafeNativeMethods.GetDC(new HandleRef(null, handle));
            IntPtr ptr2 = System.Design.SafeNativeMethods.CreatePen(System.Design.NativeMethods.PS_SOLID, 2, ColorTranslator.ToWin32(backColor));
            int nDrawMode = System.Design.SafeNativeMethods.SetROP2(new HandleRef(null, dC), num);
            IntPtr ptr3 = System.Design.SafeNativeMethods.SelectObject(new HandleRef(null, dC), new HandleRef(null, System.Design.UnsafeNativeMethods.GetStockObject(5)));
            IntPtr ptr4 = System.Design.SafeNativeMethods.SelectObject(new HandleRef(null, dC), new HandleRef(null, ptr2));
            System.Design.SafeNativeMethods.SetBkColor(new HandleRef(null, dC), ColorTranslator.ToWin32(white));
            System.Design.SafeNativeMethods.Rectangle(new HandleRef(null, dC), rectangle.X, rectangle.Y, rectangle.Right, rectangle.Bottom);
            System.Design.SafeNativeMethods.SetROP2(new HandleRef(null, dC), nDrawMode);
            System.Design.SafeNativeMethods.SelectObject(new HandleRef(null, dC), new HandleRef(null, ptr3));
            System.Design.SafeNativeMethods.SelectObject(new HandleRef(null, dC), new HandleRef(null, ptr4));
            if (ptr2 != IntPtr.Zero)
            {
                System.Design.SafeNativeMethods.DeleteObject(new HandleRef(null, ptr2));
            }
            System.Design.UnsafeNativeMethods.ReleaseDC(System.Design.NativeMethods.NullHandleRef, new HandleRef(null, dC));
        }

        public object[] GetDraggingObjects(DragEventArgs de)
        {
            return this.GetDraggingObjects(de.Data);
        }

        public object[] GetDraggingObjects(IDataObject dataObj)
        {
            return this.GetDraggingObjects(dataObj, false);
        }

        private object[] GetDraggingObjects(IDataObject dataObj, bool topLevelOnly)
        {
            object[] comps = null;
            if (dataObj is ComponentDataObjectWrapper)
            {
                dataObj = ((ComponentDataObjectWrapper) dataObj).InnerData;
                ComponentDataObject obj2 = (ComponentDataObject) dataObj;
                comps = obj2.Components;
            }
            if (topLevelOnly && (comps != null))
            {
                return this.GetTopLevelComponents(comps);
            }
            return comps;
        }

        private IComponent GetDragOwnerComponent(IDataObject data)
        {
            if ((currentDrags != null) && currentDrags.Contains(data))
            {
                return (currentDrags[data] as IComponent);
            }
            return null;
        }

        protected object GetService(System.Type t)
        {
            return this.serviceProvider.GetService(t);
        }

        private object[] GetTopLevelComponents(ICollection comps)
        {
            if (!(comps is IList))
            {
                comps = new ArrayList(comps);
            }
            IList list = (IList) comps;
            ArrayList list2 = new ArrayList();
            foreach (object obj2 in list)
            {
                Control control = obj2 as Control;
                if ((control == null) && (obj2 != null))
                {
                    list2.Add(obj2);
                }
                else if ((control != null) && ((control.Parent == null) || !list.Contains(control.Parent)))
                {
                    list2.Add(obj2);
                }
            }
            return list2.ToArray();
        }

        protected virtual void OnInitializeComponent(IComponent comp, int x, int y, int width, int height, bool hasLocation, bool hasSize)
        {
        }

        private static void RemoveCurrentDrag(IDataObject data)
        {
            currentDrags.Remove(data);
        }

        public static string DataFormat
        {
            get
            {
                return "CF_XMLCODE";
            }
        }

        internal IOleDragClient Destination
        {
            get
            {
                return this.client;
            }
        }

        public bool Dragging
        {
            get
            {
                return this.localDrag;
            }
        }

        public static string ExtraInfoFormat
        {
            get
            {
                return "CF_COMPONENTTYPES";
            }
        }

        public static bool FreezePainting
        {
            get
            {
                return freezePainting;
            }
        }

        public static string NestedToolboxItemFormat
        {
            get
            {
                return "CF_NESTEDTOOLBOXITEM";
            }
        }

        [Serializable]
        internal class CfCodeToolboxItem : ToolboxItem
        {
            private bool displaynameset;
            private object serializationData;
            private static int template;

            public CfCodeToolboxItem(object serializationData)
            {
                this.serializationData = serializationData;
            }

            private CfCodeToolboxItem(SerializationInfo info, StreamingContext context)
            {
                this.Deserialize(info, context);
            }

            protected override IComponent[] CreateComponentsCore(IDesignerHost host)
            {
                return this.CreateComponentsCore(host, null);
            }

            protected override IComponent[] CreateComponentsCore(IDesignerHost host, IDictionary defaultValues)
            {
                Control control;
                IDesignerSerializationService service = (IDesignerSerializationService) host.GetService(typeof(IDesignerSerializationService));
                if (service == null)
                {
                    return null;
                }
                ICollection is2 = service.Deserialize(this.serializationData);
                ArrayList list = new ArrayList();
                foreach (object obj2 in is2)
                {
                    if ((obj2 != null) && (obj2 is IComponent))
                    {
                        list.Add(obj2);
                    }
                }
                IComponent[] array = new IComponent[list.Count];
                list.CopyTo(array, 0);
                ArrayList components = null;
                if (defaultValues == null)
                {
                    defaultValues = new Hashtable();
                }
                control = control = defaultValues["Parent"] as Control;
                if (control != null)
                {
                    ParentControlDesigner designer = host.GetDesigner(control) as ParentControlDesigner;
                    if (designer != null)
                    {
                        Rectangle empty = Rectangle.Empty;
                        foreach (IComponent component in array)
                        {
                            Control control2 = component as Control;
                            if (((control2 != null) && (control2 != control)) && (control2.Parent == null))
                            {
                                if (empty.IsEmpty)
                                {
                                    empty = control2.Bounds;
                                }
                                else
                                {
                                    empty = Rectangle.Union(empty, control2.Bounds);
                                }
                            }
                        }
                        defaultValues.Remove("Size");
                        foreach (IComponent component2 in array)
                        {
                            Control newChild = component2 as Control;
                            Form form = newChild as Form;
                            if (((newChild != null) && ((form == null) || !form.TopLevel)) && (newChild.Parent == null))
                            {
                                defaultValues["Offset"] = new Size(newChild.Bounds.X - empty.X, newChild.Bounds.Y - empty.Y);
                                designer.AddControl(newChild, defaultValues);
                            }
                        }
                    }
                }
                ComponentTray tray = (ComponentTray) host.GetService(typeof(ComponentTray));
                if (tray != null)
                {
                    foreach (IComponent component3 in array)
                    {
                        ComponentTray.TrayControl trayControlFromComponent = tray.GetTrayControlFromComponent(component3);
                        if (trayControlFromComponent != null)
                        {
                            if (components == null)
                            {
                                components = new ArrayList();
                            }
                            components.Add(trayControlFromComponent);
                        }
                    }
                    if (components != null)
                    {
                        tray.UpdatePastePositions(components);
                    }
                }
                return array;
            }

            protected override void Deserialize(SerializationInfo info, StreamingContext context)
            {
                base.Deserialize(info, context);
                SerializationInfoEnumerator enumerator = info.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    SerializationEntry current = enumerator.Current;
                    if (current.Name == "CfCodeToolboxItem.serializationData")
                    {
                        this.serializationData = current.Value;
                        return;
                    }
                }
            }

            protected override void Serialize(SerializationInfo info, StreamingContext context)
            {
                base.Serialize(info, context);
                if (this.serializationData != null)
                {
                    info.AddValue("CfCodeToolboxItem.serializationData", this.serializationData);
                }
            }

            public void SetDisplayName()
            {
                if (!this.displaynameset)
                {
                    this.displaynameset = true;
                    base.DisplayName = "Template" + ++template.ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        protected class ComponentDataObject : IDataObject
        {
            private object[] components;
            private IOleDragClient dragClient;
            private int initialX;
            private int initialY;
            private object serializationData;
            private Stream serializationStream;
            private IServiceProvider serviceProvider;
            private OleDragDropHandler.CfCodeToolboxItem toolboxitemdata;

            public ComponentDataObject(IOleDragClient dragClient, IServiceProvider sp, object serializationData)
            {
                this.serviceProvider = sp;
                this.serializationData = serializationData;
                this.dragClient = dragClient;
            }

            public ComponentDataObject(IOleDragClient dragClient, IServiceProvider sp, object[] comps, int x, int y)
            {
                this.serviceProvider = sp;
                this.components = this.GetComponentList(comps, null, -1);
                this.initialX = x;
                this.initialY = y;
                this.dragClient = dragClient;
            }

            public void Deserialize(IServiceProvider serviceProvider, bool removeCurrentComponents)
            {
                if (serviceProvider == null)
                {
                    serviceProvider = this.serviceProvider;
                }
                IDesignerSerializationService service = (IDesignerSerializationService) serviceProvider.GetService(typeof(IDesignerSerializationService));
                IDesignerHost host = null;
                DesignerTransaction transaction = null;
                try
                {
                    if (this.serializationData == null)
                    {
                        this.serializationData = new BinaryFormatter().Deserialize(this.SerializationStream);
                    }
                    if (removeCurrentComponents && (this.components != null))
                    {
                        foreach (IComponent component in this.components)
                        {
                            if ((host == null) && (component.Site != null))
                            {
                                host = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
                                if (host != null)
                                {
                                    transaction = host.CreateTransaction(System.Design.SR.GetString("DragDropMoveComponents", new object[] { this.components.Length }));
                                }
                            }
                            if (host != null)
                            {
                                host.DestroyComponent(component);
                            }
                        }
                        this.components = null;
                    }
                    ICollection is2 = service.Deserialize(this.serializationData);
                    this.components = new IComponent[is2.Count];
                    IEnumerator enumerator = is2.GetEnumerator();
                    int num = 0;
                    while (enumerator.MoveNext())
                    {
                        this.components[num++] = (IComponent) enumerator.Current;
                    }
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < this.components.Length; i++)
                    {
                        if (this.components[i] is Control)
                        {
                            Control control = (Control) this.components[i];
                            if (control.Parent == null)
                            {
                                list.Add(this.components[i]);
                            }
                        }
                        else
                        {
                            list.Add(this.components[i]);
                        }
                    }
                    this.components = list.ToArray();
                }
                finally
                {
                    if (transaction != null)
                    {
                        transaction.Commit();
                    }
                }
            }

            private void GetAssociatedComponents(IComponent component, IDesignerHost host, ArrayList list)
            {
                ComponentDesigner designer = host.GetDesigner(component) as ComponentDesigner;
                if (designer != null)
                {
                    foreach (IComponent component2 in designer.AssociatedComponents)
                    {
                        list.Add(component2);
                        this.GetAssociatedComponents(component2, host, list);
                    }
                }
            }

            private object[] GetComponentList(object[] components, ArrayList list, int index)
            {
                ICollection selectedComponents;
                if (this.serviceProvider == null)
                {
                    return components;
                }
                ISelectionService service = (ISelectionService) this.serviceProvider.GetService(typeof(ISelectionService));
                if (service == null)
                {
                    return components;
                }
                if (components == null)
                {
                    selectedComponents = service.GetSelectedComponents();
                }
                else
                {
                    selectedComponents = new ArrayList(components);
                }
                IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    ArrayList list2 = new ArrayList();
                    foreach (IComponent component in selectedComponents)
                    {
                        list2.Add(component);
                        this.GetAssociatedComponents(component, host, list2);
                    }
                    selectedComponents = list2;
                }
                object[] array = new object[selectedComponents.Count];
                selectedComponents.CopyTo(array, 0);
                return array;
            }

            public virtual object GetData(string format)
            {
                return this.GetData(format, false);
            }

            public virtual object GetData(System.Type t)
            {
                return this.GetData(t.FullName);
            }

            public virtual object GetData(string format, bool autoConvert)
            {
                if (format.Equals(OleDragDropHandler.DataFormat))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    this.SerializationStream.Seek(0L, SeekOrigin.Begin);
                    return formatter.Deserialize(this.SerializationStream);
                }
                if (format.Equals(OleDragDropHandler.NestedToolboxItemFormat))
                {
                    this.NestedToolboxItem.SetDisplayName();
                    return this.NestedToolboxItem;
                }
                return null;
            }

            public bool GetDataPresent(string format)
            {
                return this.GetDataPresent(format, false);
            }

            public bool GetDataPresent(System.Type format)
            {
                return this.GetDataPresent(format.FullName, false);
            }

            public bool GetDataPresent(string format, bool autoConvert)
            {
                return (Array.IndexOf<string>(this.GetFormats(), format) != -1);
            }

            public string[] GetFormats()
            {
                return new string[] { OleDragDropHandler.NestedToolboxItemFormat, OleDragDropHandler.DataFormat, DataFormats.Serializable, OleDragDropHandler.ExtraInfoFormat };
            }

            public string[] GetFormats(bool autoConvert)
            {
                return this.GetFormats();
            }

            public void SetData(object data)
            {
                this.SetData(data.GetType(), data);
            }

            public void SetData(string format, object data)
            {
                throw new Exception(System.Design.SR.GetString("DragDropSetDataError"));
            }

            public void SetData(System.Type format, object data)
            {
                this.SetData(format.FullName, data);
            }

            public void SetData(string format, bool autoConvert, object data)
            {
                this.SetData(format, data);
            }

            public object[] Components
            {
                get
                {
                    if ((this.components == null) && ((this.serializationStream != null) || (this.serializationData != null)))
                    {
                        this.Deserialize(null, false);
                        if (this.components == null)
                        {
                            return new object[0];
                        }
                    }
                    return (object[]) this.components.Clone();
                }
            }

            private OleDragDropHandler.CfCodeToolboxItem NestedToolboxItem
            {
                get
                {
                    if (this.toolboxitemdata == null)
                    {
                        this.toolboxitemdata = new OleDragDropHandler.CfCodeToolboxItem(this.GetData(OleDragDropHandler.DataFormat));
                    }
                    return this.toolboxitemdata;
                }
            }

            private Stream SerializationStream
            {
                get
                {
                    if ((this.serializationStream == null) && (this.Components != null))
                    {
                        IDesignerSerializationService service = (IDesignerSerializationService) this.serviceProvider.GetService(typeof(IDesignerSerializationService));
                        if (service != null)
                        {
                            object[] objects = new object[this.components.Length];
                            for (int i = 0; i < this.components.Length; i++)
                            {
                                objects[i] = (IComponent) this.components[i];
                            }
                            object graph = service.Serialize(objects);
                            this.serializationStream = new MemoryStream();
                            new BinaryFormatter().Serialize(this.serializationStream, graph);
                            this.serializationStream.Seek(0L, SeekOrigin.Begin);
                        }
                    }
                    return this.serializationStream;
                }
            }
        }

        protected class ComponentDataObjectWrapper : DataObject
        {
            private OleDragDropHandler.ComponentDataObject innerData;

            public ComponentDataObjectWrapper(OleDragDropHandler.ComponentDataObject dataObject) : base(dataObject)
            {
                this.innerData = dataObject;
            }

            public OleDragDropHandler.ComponentDataObject InnerData
            {
                get
                {
                    return this.innerData;
                }
            }
        }
    }
}

