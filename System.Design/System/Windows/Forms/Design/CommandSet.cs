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
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class CommandSet : IDisposable
    {
        private System.Windows.Forms.Design.Behavior.BehaviorService behaviorService;
        private const string CF_DESIGNER = "CF_DESIGNERCOMPONENTS_V2";
        private CommandSetItem[] commandSet;
        protected bool controlsOnlySelection;
        protected DragAssistanceManager dragManager;
        private IEventHandlerService eventService;
        private IMenuCommandService menuService;
        protected IComponent primarySelection;
        protected int selCount;
        private bool selectionInherited;
        private ISelectionService selectionService;
        private int selectionVersion = 1;
        protected ISite site;
        private Timer snapLineTimer;
        private const int SORT_HORIZONTAL = 0;
        private const int SORT_VERTICAL = 1;
        private const int SORT_ZORDER = 2;
        private StatusCommandUI statusCommandUI;

        public CommandSet(ISite site)
        {
            this.site = site;
            this.eventService = (IEventHandlerService) site.GetService(typeof(IEventHandlerService));
            this.eventService.EventHandlerChanged += new EventHandler(this.OnEventHandlerChanged);
            IDesignerHost host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                host.Activated += new EventHandler(this.UpdateClipboardItems);
            }
            this.statusCommandUI = new StatusCommandUI(site);
            IUIService uiService = site.GetService(typeof(IUIService)) as IUIService;
            this.commandSet = new CommandSetItem[] { 
                new CommandSetItem(this, new EventHandler(this.OnStatusDelete), new EventHandler(this.OnMenuDelete), StandardCommands.Delete, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusCopy), new EventHandler(this.OnMenuCopy), StandardCommands.Copy, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusCut), new EventHandler(this.OnMenuCut), StandardCommands.Cut, uiService), new ImmediateCommandSetItem(this, new EventHandler(this.OnStatusPaste), new EventHandler(this.OnMenuPaste), StandardCommands.Paste, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusSelectAll), new EventHandler(this.OnMenuSelectAll), StandardCommands.SelectAll, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnMenuDesignerProperties), MenuCommands.DesignerProperties, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyCancel), MenuCommands.KeyCancel, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeyCancel), MenuCommands.KeyReverseCancel, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusPrimarySelection), new EventHandler(this.OnKeyDefault), MenuCommands.KeyDefaultAction, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveUp, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveDown, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveLeft, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveRight, true), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeUp, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeDown, true, uiService), new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeLeft, true, uiService), 
                new CommandSetItem(this, new EventHandler(this.OnStatusAnySelection), new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeRight, true, uiService)
             };
            this.selectionService = (ISelectionService) site.GetService(typeof(ISelectionService));
            if (this.selectionService != null)
            {
                this.selectionService.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            this.menuService = (IMenuCommandService) site.GetService(typeof(IMenuCommandService));
            if (this.menuService != null)
            {
                for (int i = 0; i < this.commandSet.Length; i++)
                {
                    this.menuService.AddCommand(this.commandSet[i]);
                }
            }
            IDictionaryService service = site.GetService(typeof(IDictionaryService)) as IDictionaryService;
            if (service != null)
            {
                service.SetValue(typeof(CommandID), new CommandID(new Guid("BA09E2AF-9DF2-4068-B2F0-4C7E5CC19E2F"), 0));
            }
        }

        protected bool CanCheckout(IComponent comp)
        {
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                try
                {
                    service.OnComponentChanging(comp, null);
                }
                catch (CheckoutException exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        throw exception;
                    }
                    return false;
                }
            }
            return true;
        }

        private bool CheckComponentEditor(object obj, bool launchEditor)
        {
            if (obj is IComponent)
            {
                try
                {
                    if (launchEditor)
                    {
                        ComponentEditor editor = (ComponentEditor) TypeDescriptor.GetEditor(obj, typeof(ComponentEditor));
                        if (editor == null)
                        {
                            return false;
                        }
                        bool flag = false;
                        IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                        if (service != null)
                        {
                            try
                            {
                                service.OnComponentChanging(obj, null);
                            }
                            catch (CheckoutException exception)
                            {
                                if (exception != CheckoutException.Canceled)
                                {
                                    throw exception;
                                }
                                return false;
                            }
                            catch
                            {
                                throw;
                            }
                        }
                        WindowsFormsComponentEditor editor2 = editor as WindowsFormsComponentEditor;
                        if (editor2 != null)
                        {
                            IWin32Window owner = null;
                            if (obj is IWin32Window)
                            {
                                owner = owner;
                            }
                            flag = editor2.EditComponent(obj, owner);
                        }
                        else
                        {
                            flag = editor.EditComponent(obj);
                        }
                        if (flag && (service != null))
                        {
                            service.OnComponentChanged(obj, null, null, null);
                        }
                    }
                    return true;
                }
                catch (Exception exception2)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception2))
                    {
                        throw;
                    }
                }
            }
            return false;
        }

        public virtual void Dispose()
        {
            if (this.menuService != null)
            {
                for (int i = 0; i < this.commandSet.Length; i++)
                {
                    this.menuService.RemoveCommand(this.commandSet[i]);
                    this.commandSet[i].Dispose();
                }
                this.menuService = null;
            }
            if (this.selectionService != null)
            {
                this.selectionService.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                this.selectionService = null;
            }
            if (this.eventService != null)
            {
                this.eventService.EventHandlerChanged -= new EventHandler(this.OnEventHandlerChanged);
                this.eventService = null;
            }
            IDesignerHost service = (IDesignerHost) this.site.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                service.Activated -= new EventHandler(this.UpdateClipboardItems);
            }
            if (this.snapLineTimer != null)
            {
                this.snapLineTimer.Stop();
                this.snapLineTimer.Tick -= new EventHandler(this.OnSnapLineTimerExpire);
                this.snapLineTimer = null;
            }
            this.EndDragManager();
            this.statusCommandUI = null;
            this.site = null;
        }

        protected void EndDragManager()
        {
            if (this.dragManager != null)
            {
                if (this.snapLineTimer != null)
                {
                    this.snapLineTimer.Stop();
                }
                this.dragManager.EraseSnapLines();
                this.dragManager.OnMouseUp();
                this.dragManager = null;
            }
        }

        private object[] FilterSelection(object[] components, SelectionRules selectionRules)
        {
            object[] objArray = null;
            if (components != null)
            {
                if (selectionRules != SelectionRules.None)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        ArrayList list = new ArrayList();
                        foreach (IComponent component in components)
                        {
                            ControlDesigner designer = service.GetDesigner(component) as ControlDesigner;
                            if ((designer != null) && ((designer.SelectionRules & selectionRules) == selectionRules))
                            {
                                list.Add(component);
                            }
                        }
                        objArray = list.ToArray();
                    }
                }
                if (objArray != null)
                {
                    return objArray;
                }
            }
            return new object[0];
        }

        private void GetAssociatedComponents(IComponent component, IDesignerHost host, ArrayList list)
        {
            ComponentDesigner designer = host.GetDesigner(component) as ComponentDesigner;
            if (designer != null)
            {
                foreach (IComponent component2 in designer.AssociatedComponents)
                {
                    if (component2.Site != null)
                    {
                        list.Add(component2);
                        this.GetAssociatedComponents(component2, host, list);
                    }
                }
            }
        }

        protected virtual ICollection GetCopySelection()
        {
            ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
            bool flag = false;
            object[] array = new object[selectedComponents.Count];
            selectedComponents.CopyTo(array, 0);
            foreach (object obj2 in array)
            {
                if (obj2 is Control)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                this.SortSelection(array, 2);
            }
            selectedComponents = array;
            IDesignerHost service = (IDesignerHost) this.site.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                return selectedComponents;
            }
            ArrayList list = new ArrayList();
            foreach (IComponent component in selectedComponents)
            {
                list.Add(component);
                this.GetAssociatedComponents(component, service, list);
            }
            return list;
        }

        private Point GetLocation(IComponent comp)
        {
            PropertyDescriptor property = this.GetProperty(comp, "Location");
            if (property != null)
            {
                try
                {
                    return (Point) property.GetValue(comp);
                }
                catch (Exception exception)
                {
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
            }
            return Point.Empty;
        }

        protected PropertyDescriptor GetProperty(object comp, string propName)
        {
            return TypeDescriptor.GetProperties(comp)[propName];
        }

        protected virtual object GetService(System.Type serviceType)
        {
            if (this.site != null)
            {
                return this.site.GetService(serviceType);
            }
            return null;
        }

        private Size GetSize(IComponent comp)
        {
            PropertyDescriptor property = this.GetProperty(comp, "Size");
            if (property != null)
            {
                return (Size) property.GetValue(comp);
            }
            return Size.Empty;
        }

        protected virtual void GetSnapInformation(IDesignerHost host, IComponent component, out Size snapSize, out IComponent snapComponent, out PropertyDescriptor snapProperty)
        {
            IComponent rootComponent = null;
            PropertyDescriptor descriptor = null;
            PropertyDescriptor descriptor2 = null;
            rootComponent = host.RootComponent;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(rootComponent);
            descriptor2 = properties["SnapToGrid"];
            if ((descriptor2 != null) && (descriptor2.PropertyType != typeof(bool)))
            {
                descriptor2 = null;
            }
            descriptor = properties["GridSize"];
            if ((descriptor != null) && (descriptor.PropertyType != typeof(Size)))
            {
                descriptor = null;
            }
            snapComponent = rootComponent;
            snapProperty = descriptor2;
            if (descriptor != null)
            {
                snapSize = (Size) descriptor.GetValue(snapComponent);
            }
            else
            {
                snapSize = Size.Empty;
            }
        }

        private void OnEventHandlerChanged(object sender, EventArgs e)
        {
            this.OnUpdateCommandStatus();
        }

        protected virtual bool OnKeyCancel(object sender)
        {
            bool flag = false;
            if ((this.BehaviorService != null) && this.BehaviorService.HasCapture)
            {
                this.BehaviorService.OnLoseCapture();
                return true;
            }
            IToolboxService service = (IToolboxService) this.GetService(typeof(IToolboxService));
            if ((service == null) || (service.GetSelectedToolboxItem((IDesignerHost) this.GetService(typeof(IDesignerHost))) == null))
            {
                return flag;
            }
            service.SelectedToolboxItemUsed();
            System.Design.NativeMethods.POINT pt = new System.Design.NativeMethods.POINT();
            System.Design.NativeMethods.GetCursorPos(pt);
            IntPtr hWnd = System.Design.NativeMethods.WindowFromPoint(pt.x, pt.y);
            if (hWnd != IntPtr.Zero)
            {
                System.Design.NativeMethods.SendMessage(hWnd, 0x20, hWnd, (IntPtr) 1);
            }
            else
            {
                Cursor.Current = Cursors.Default;
            }
            return true;
        }

        private void OnKeyCancel(object sender, EventArgs e)
        {
            this.OnKeyCancel(sender);
        }

        protected void OnKeyDefault(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                if (primarySelection != null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        IDesigner designer = service.GetDesigner(primarySelection);
                        if (designer != null)
                        {
                            designer.DoDefaultAction();
                        }
                    }
                }
            }
        }

        protected virtual void OnKeyMove(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                if (primarySelection != null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(primarySelection)["Locked"];
                        if (((descriptor == null) || (descriptor.PropertyType != typeof(bool))) || !((bool) descriptor.GetValue(primarySelection)))
                        {
                            DesignerTransaction transaction;
                            CommandID commandID = ((MenuCommand) sender).CommandID;
                            bool flag = false;
                            int x = 0;
                            int y = 0;
                            if (commandID.Equals(MenuCommands.KeyMoveUp))
                            {
                                y = -1;
                            }
                            else if (commandID.Equals(MenuCommands.KeyMoveDown))
                            {
                                y = 1;
                            }
                            else if (commandID.Equals(MenuCommands.KeyMoveLeft))
                            {
                                x = -1;
                            }
                            else if (commandID.Equals(MenuCommands.KeyMoveRight))
                            {
                                x = 1;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeUp))
                            {
                                y = -1;
                                flag = true;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeDown))
                            {
                                y = 1;
                                flag = true;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeLeft))
                            {
                                x = -1;
                                flag = true;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeRight))
                            {
                                x = 1;
                                flag = true;
                            }
                            if (selectionService.SelectionCount > 1)
                            {
                                transaction = service.CreateTransaction(System.Design.SR.GetString("DragDropMoveComponents", new object[] { selectionService.SelectionCount }));
                            }
                            else
                            {
                                transaction = service.CreateTransaction(System.Design.SR.GetString("DragDropMoveComponent", new object[] { primarySelection.Site.Name }));
                            }
                            try
                            {
                                if (this.BehaviorService != null)
                                {
                                    Control targetControl = primarySelection as Control;
                                    bool useSnapLines = this.BehaviorService.UseSnapLines;
                                    if (this.dragManager != null)
                                    {
                                        this.EndDragManager();
                                    }
                                    if ((flag && useSnapLines) && (targetControl != null))
                                    {
                                        ArrayList dragComponents = new ArrayList(selectionService.GetSelectedComponents());
                                        this.dragManager = new DragAssistanceManager(primarySelection.Site, dragComponents);
                                        Point point = this.dragManager.OffsetToNearestSnapLocation(targetControl, new Point(x, y));
                                        x = point.X;
                                        y = point.Y;
                                        if (targetControl.Parent.IsMirrored)
                                        {
                                            x *= -1;
                                        }
                                    }
                                    else if (!flag && !useSnapLines)
                                    {
                                        bool flag3 = false;
                                        Size empty = Size.Empty;
                                        IComponent snapComponent = null;
                                        PropertyDescriptor snapProperty = null;
                                        this.GetSnapInformation(service, primarySelection, out empty, out snapComponent, out snapProperty);
                                        if (snapProperty != null)
                                        {
                                            flag3 = (bool) snapProperty.GetValue(snapComponent);
                                        }
                                        if (flag3 && !empty.IsEmpty)
                                        {
                                            x *= empty.Width;
                                            y *= empty.Height;
                                            if (targetControl != null)
                                            {
                                                ParentControlDesigner designer = service.GetDesigner(targetControl.Parent) as ParentControlDesigner;
                                                if (designer != null)
                                                {
                                                    Point location = targetControl.Location;
                                                    if (targetControl.Parent.IsMirrored)
                                                    {
                                                        x *= -1;
                                                    }
                                                    location.Offset(x, y);
                                                    location = designer.GetSnappedPoint(location);
                                                    if (x != 0)
                                                    {
                                                        x = location.X - targetControl.Location.X;
                                                    }
                                                    if (y != 0)
                                                    {
                                                        y = location.Y - targetControl.Location.Y;
                                                    }
                                                }
                                            }
                                        }
                                        else if ((targetControl != null) && targetControl.Parent.IsMirrored)
                                        {
                                            x *= -1;
                                        }
                                    }
                                    else if ((targetControl != null) && targetControl.Parent.IsMirrored)
                                    {
                                        x *= -1;
                                    }
                                    SelectionRules rules = SelectionRules.Visible | SelectionRules.Moveable;
                                    foreach (IComponent component3 in selectionService.GetSelectedComponents())
                                    {
                                        ControlDesigner designer2 = service.GetDesigner(component3) as ControlDesigner;
                                        if ((designer2 == null) || ((designer2.SelectionRules & rules) == rules))
                                        {
                                            PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(component3)["Location"];
                                            if (descriptor3 != null)
                                            {
                                                Point point3 = (Point) descriptor3.GetValue(component3);
                                                point3.Offset(x, y);
                                                descriptor3.SetValue(component3, point3);
                                            }
                                            if ((component3 == selectionService.PrimarySelection) && (this.statusCommandUI != null))
                                            {
                                                this.statusCommandUI.SetStatusInformation(component3 as Component);
                                            }
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (transaction != null)
                                {
                                    transaction.Commit();
                                }
                                if (this.dragManager != null)
                                {
                                    this.SnapLineTimer.Start();
                                    this.dragManager.RenderSnapLinesInternal();
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void OnMenuAlignByPrimary(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            CommandID commandID = command.CommandID;
            Point location = this.GetLocation(this.primarySelection);
            Size size = this.GetSize(this.primarySelection);
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    DesignerTransaction transaction = null;
                    try
                    {
                        if (service != null)
                        {
                            transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetAlignByPrimary", new object[] { selectedComponents.Count }));
                        }
                        bool flag = true;
                        Point empty = Point.Empty;
                        foreach (object obj2 in selectedComponents)
                        {
                            if (obj2 != this.primarySelection)
                            {
                                IComponent component = obj2 as IComponent;
                                if (((component == null) || (service == null)) || (service.GetDesigner(component) is ControlDesigner))
                                {
                                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                                    PropertyDescriptor descriptor = properties["Location"];
                                    PropertyDescriptor descriptor2 = properties["Size"];
                                    PropertyDescriptor descriptor3 = properties["Locked"];
                                    if ((((descriptor3 == null) || !((bool) descriptor3.GetValue(component))) && ((descriptor != null) && !descriptor.IsReadOnly)) && (((!commandID.Equals(StandardCommands.AlignBottom) && !commandID.Equals(StandardCommands.AlignHorizontalCenters)) && (!commandID.Equals(StandardCommands.AlignVerticalCenters) && !commandID.Equals(StandardCommands.AlignRight))) || ((descriptor2 != null) && !descriptor2.IsReadOnly)))
                                    {
                                        if (commandID.Equals(StandardCommands.AlignBottom))
                                        {
                                            empty = (Point) descriptor.GetValue(component);
                                            Size size2 = (Size) descriptor2.GetValue(component);
                                            empty.Y = (location.Y + size.Height) - size2.Height;
                                        }
                                        else if (commandID.Equals(StandardCommands.AlignHorizontalCenters))
                                        {
                                            empty = (Point) descriptor.GetValue(component);
                                            Size size3 = (Size) descriptor2.GetValue(component);
                                            empty.Y = ((size.Height / 2) + location.Y) - (size3.Height / 2);
                                        }
                                        else if (commandID.Equals(StandardCommands.AlignLeft))
                                        {
                                            empty = (Point) descriptor.GetValue(component);
                                            empty.X = location.X;
                                        }
                                        else if (commandID.Equals(StandardCommands.AlignRight))
                                        {
                                            empty = (Point) descriptor.GetValue(component);
                                            Size size4 = (Size) descriptor2.GetValue(component);
                                            empty.X = (location.X + size.Width) - size4.Width;
                                        }
                                        else if (commandID.Equals(StandardCommands.AlignTop))
                                        {
                                            empty = (Point) descriptor.GetValue(component);
                                            empty.Y = location.Y;
                                        }
                                        else if (commandID.Equals(StandardCommands.AlignVerticalCenters))
                                        {
                                            empty = (Point) descriptor.GetValue(component);
                                            Size size5 = (Size) descriptor2.GetValue(component);
                                            empty.X = ((size.Width / 2) + location.X) - (size5.Width / 2);
                                        }
                                        if (flag && !this.CanCheckout(component))
                                        {
                                            return;
                                        }
                                        flag = false;
                                        descriptor.SetValue(component, empty);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected void OnMenuAlignToGrid(object sender, EventArgs e)
        {
            Size empty = Size.Empty;
            PropertyDescriptor descriptor = null;
            PropertyDescriptor descriptor2 = null;
            Point point = Point.Empty;
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    DesignerTransaction transaction = null;
                    try
                    {
                        if (service != null)
                        {
                            transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetAlignToGrid", new object[] { selectedComponents.Count }));
                            Control rootComponent = service.RootComponent as Control;
                            if (rootComponent != null)
                            {
                                PropertyDescriptor property = this.GetProperty(rootComponent, "GridSize");
                                if (property != null)
                                {
                                    empty = (Size) property.GetValue(rootComponent);
                                }
                                if ((property == null) || empty.IsEmpty)
                                {
                                    return;
                                }
                            }
                        }
                        bool flag = true;
                        foreach (object obj2 in selectedComponents)
                        {
                            descriptor2 = this.GetProperty(obj2, "Locked");
                            if ((descriptor2 == null) || !((bool) descriptor2.GetValue(obj2)))
                            {
                                IComponent component = obj2 as IComponent;
                                if (((component == null) || (service == null)) || (service.GetDesigner(component) is ControlDesigner))
                                {
                                    descriptor = this.GetProperty(obj2, "Location");
                                    if ((descriptor != null) && !descriptor.IsReadOnly)
                                    {
                                        point = (Point) descriptor.GetValue(obj2);
                                        int num = point.X % empty.Width;
                                        if (num < (empty.Width / 2))
                                        {
                                            point.X -= num;
                                        }
                                        else
                                        {
                                            point.X += empty.Width - num;
                                        }
                                        num = point.Y % empty.Height;
                                        if (num < (empty.Height / 2))
                                        {
                                            point.Y -= num;
                                        }
                                        else
                                        {
                                            point.Y += empty.Height - num;
                                        }
                                        if (flag && !this.CanCheckout(component))
                                        {
                                            return;
                                        }
                                        flag = false;
                                        descriptor.SetValue(obj2, point);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected void OnMenuCenterSelection(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            CommandID commandID = command.CommandID;
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                    Control parent = null;
                    Size empty = Size.Empty;
                    Point point = Point.Empty;
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    DesignerTransaction transaction = null;
                    try
                    {
                        if (service != null)
                        {
                            string str;
                            if (commandID == StandardCommands.CenterHorizontally)
                            {
                                str = System.Design.SR.GetString("WindowsFormsCommandCenterX", new object[] { selectedComponents.Count });
                            }
                            else
                            {
                                str = System.Design.SR.GetString("WindowsFormsCommandCenterY", new object[] { selectedComponents.Count });
                            }
                            transaction = service.CreateTransaction(str);
                        }
                        int y = 0x7fffffff;
                        int x = 0x7fffffff;
                        int num3 = -2147483648;
                        int num4 = -2147483648;
                        foreach (object obj2 in selectedComponents)
                        {
                            if (obj2 is Control)
                            {
                                IComponent component = (IComponent) obj2;
                                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                                PropertyDescriptor descriptor = properties["Location"];
                                PropertyDescriptor descriptor2 = properties["Size"];
                                if (((descriptor != null) && (descriptor2 != null)) && (!descriptor.IsReadOnly && !descriptor2.IsReadOnly))
                                {
                                    PropertyDescriptor descriptor3 = properties["Locked"];
                                    if ((descriptor3 == null) || !((bool) descriptor3.GetValue(component)))
                                    {
                                        empty = (Size) descriptor2.GetValue(component);
                                        point = (Point) descriptor.GetValue(component);
                                        if (parent == null)
                                        {
                                            parent = ((Control) component).Parent;
                                        }
                                        if (point.X < x)
                                        {
                                            x = point.X;
                                        }
                                        if (point.Y < y)
                                        {
                                            y = point.Y;
                                        }
                                        if ((point.X + empty.Width) > num3)
                                        {
                                            num3 = point.X + empty.Width;
                                        }
                                        if ((point.Y + empty.Height) > num4)
                                        {
                                            num4 = point.Y + empty.Height;
                                        }
                                    }
                                }
                            }
                        }
                        if (parent != null)
                        {
                            int num5 = (x + num3) / 2;
                            int num6 = (y + num4) / 2;
                            int num7 = parent.ClientSize.Width / 2;
                            int num8 = parent.ClientSize.Height / 2;
                            int num9 = 0;
                            int num10 = 0;
                            bool flag = false;
                            bool flag2 = false;
                            if (num7 >= num5)
                            {
                                num9 = num7 - num5;
                                flag = true;
                            }
                            else
                            {
                                num9 = num5 - num7;
                            }
                            if (num8 >= num6)
                            {
                                num10 = num8 - num6;
                                flag2 = true;
                            }
                            else
                            {
                                num10 = num6 - num8;
                            }
                            bool flag3 = true;
                            foreach (object obj3 in selectedComponents)
                            {
                                if (obj3 is Control)
                                {
                                    IComponent component2 = (IComponent) obj3;
                                    PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(component2)["Location"];
                                    if (!descriptor4.IsReadOnly)
                                    {
                                        point = (Point) descriptor4.GetValue(component2);
                                        if (commandID == StandardCommands.CenterHorizontally)
                                        {
                                            if (flag)
                                            {
                                                point.X += num9;
                                            }
                                            else
                                            {
                                                point.X -= num9;
                                            }
                                        }
                                        else if (commandID == StandardCommands.CenterVertically)
                                        {
                                            if (flag2)
                                            {
                                                point.Y += num10;
                                            }
                                            else
                                            {
                                                point.Y -= num10;
                                            }
                                        }
                                        if (flag3 && !this.CanCheckout(component2))
                                        {
                                            return;
                                        }
                                        flag3 = false;
                                        descriptor4.SetValue(component2, point);
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected void OnMenuCopy(object sender, EventArgs e)
        {
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection copySelection = this.GetCopySelection();
                    copySelection = this.PrependComponentNames(copySelection);
                    IDesignerSerializationService service = (IDesignerSerializationService) this.GetService(typeof(IDesignerSerializationService));
                    if (service != null)
                    {
                        object graph = service.Serialize(copySelection);
                        MemoryStream serializationStream = new MemoryStream();
                        new BinaryFormatter().Serialize(serializationStream, graph);
                        serializationStream.Seek(0L, SeekOrigin.Begin);
                        byte[] data = serializationStream.GetBuffer();
                        IDataObject obj3 = new DataObject("CF_DESIGNERCOMPONENTS_V2", data);
                        Clipboard.SetDataObject(obj3);
                    }
                    this.UpdateClipboardItems(null, null);
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected void OnMenuCut(object sender, EventArgs e)
        {
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection copySelection = this.GetCopySelection();
                    int count = copySelection.Count;
                    copySelection = this.PrependComponentNames(copySelection);
                    IDesignerSerializationService service = (IDesignerSerializationService) this.GetService(typeof(IDesignerSerializationService));
                    if (service != null)
                    {
                        object graph = service.Serialize(copySelection);
                        MemoryStream serializationStream = new MemoryStream();
                        new BinaryFormatter().Serialize(serializationStream, graph);
                        serializationStream.Seek(0L, SeekOrigin.Begin);
                        byte[] data = serializationStream.GetBuffer();
                        IDataObject obj3 = new DataObject("CF_DESIGNERCOMPONENTS_V2", data);
                        Clipboard.SetDataObject(obj3);
                        IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                        Control ctl = null;
                        if (host != null)
                        {
                            IComponentChangeService service2 = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                            DesignerTransaction transaction = null;
                            ArrayList list = new ArrayList();
                            try
                            {
                                transaction = host.CreateTransaction(System.Design.SR.GetString("CommandSetCutMultiple", new object[] { count }));
                                this.SelectionService.SetSelectedComponents(new object[0], SelectionTypes.Replace);
                                object[] array = new object[copySelection.Count];
                                copySelection.CopyTo(array, 0);
                                for (int i = 0; i < array.Length; i++)
                                {
                                    object obj4 = array[i];
                                    IComponent component = obj4 as IComponent;
                                    if ((obj4 != host.RootComponent) && (component != null))
                                    {
                                        Control control2 = obj4 as Control;
                                        if (control2 != null)
                                        {
                                            Control parent = control2.Parent;
                                            if (parent != null)
                                            {
                                                ParentControlDesigner item = host.GetDesigner(parent) as ParentControlDesigner;
                                                if ((item != null) && !list.Contains(item))
                                                {
                                                    item.SuspendChangingEvents();
                                                    list.Add(item);
                                                    item.ForceComponentChanging();
                                                }
                                            }
                                        }
                                    }
                                }
                                for (int j = 0; j < array.Length; j++)
                                {
                                    object obj5 = array[j];
                                    IComponent component2 = obj5 as IComponent;
                                    if ((obj5 != host.RootComponent) && (component2 != null))
                                    {
                                        Control control4 = obj5 as Control;
                                        if ((ctl == null) && (control4 != null))
                                        {
                                            ctl = control4.Parent;
                                        }
                                        else if ((ctl != null) && (control4 != null))
                                        {
                                            Control control5 = control4;
                                            if ((control5.Parent != ctl) && !ctl.Contains(control5))
                                            {
                                                if ((control5 == ctl) || control5.Contains(ctl))
                                                {
                                                    ctl = control5.Parent;
                                                }
                                                else
                                                {
                                                    ctl = null;
                                                }
                                            }
                                        }
                                        if (component2 != null)
                                        {
                                            ArrayList list2 = new ArrayList();
                                            this.GetAssociatedComponents(component2, host, list2);
                                            foreach (IComponent component3 in list2)
                                            {
                                                service2.OnComponentChanging(component3, null);
                                            }
                                            host.DestroyComponent(component2);
                                        }
                                    }
                                }
                            }
                            finally
                            {
                                if (transaction != null)
                                {
                                    transaction.Commit();
                                }
                                foreach (ParentControlDesigner designer2 in list)
                                {
                                    if (designer2 != null)
                                    {
                                        designer2.ResumeChangingEvents();
                                    }
                                }
                            }
                            if (ctl != null)
                            {
                                this.SelectionService.SetSelectedComponents(new object[] { ctl }, SelectionTypes.Replace);
                            }
                            else if (this.SelectionService.PrimarySelection == null)
                            {
                                this.SelectionService.SetSelectedComponents(new object[] { host.RootComponent }, SelectionTypes.Replace);
                            }
                        }
                    }
                }
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected void OnMenuDelete(object sender, EventArgs e)
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (this.site != null)
                {
                    IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if ((this.SelectionService != null) && (host != null))
                    {
                        IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                        ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                        string description = System.Design.SR.GetString("CommandSetDelete", new object[] { selectedComponents.Count });
                        DesignerTransaction transaction = null;
                        IComponent component = null;
                        bool flag = false;
                        ArrayList list = new ArrayList();
                        try
                        {
                            transaction = host.CreateTransaction(description);
                            this.SelectionService.SetSelectedComponents(new object[0], SelectionTypes.Replace);
                            foreach (object obj2 in selectedComponents)
                            {
                                IComponent component2 = obj2 as IComponent;
                                if ((component2 != null) && (component2.Site != null))
                                {
                                    Control control = obj2 as Control;
                                    if (control != null)
                                    {
                                        Control parent = control.Parent;
                                        if (parent != null)
                                        {
                                            ParentControlDesigner item = host.GetDesigner(parent) as ParentControlDesigner;
                                            if ((item != null) && !list.Contains(item))
                                            {
                                                item.SuspendChangingEvents();
                                                list.Add(item);
                                                item.ForceComponentChanging();
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (object obj3 in selectedComponents)
                            {
                                Control control4;
                                Control control5;
                                ITreeDesigner designer4;
                                ArrayList list6;
                                IComponent component3 = obj3 as IComponent;
                                if (((component3 == null) || (component3.Site == null)) || (obj3 == host.RootComponent))
                                {
                                    continue;
                                }
                                Control control3 = obj3 as Control;
                                if (!flag)
                                {
                                    if (control3 != null)
                                    {
                                        component = control3.Parent;
                                    }
                                    else
                                    {
                                        ITreeDesigner designer2 = host.GetDesigner((IComponent) obj3) as ITreeDesigner;
                                        if (designer2 != null)
                                        {
                                            IDesigner designer3 = designer2.Parent;
                                            if (designer3 != null)
                                            {
                                                component = designer3.Component;
                                            }
                                        }
                                    }
                                    flag = component != null;
                                }
                                else if (component != null)
                                {
                                    if ((control3 == null) || !(component is Control))
                                    {
                                        goto Label_0265;
                                    }
                                    control4 = control3;
                                    control5 = (Control) component;
                                    if ((control4.Parent != control5) && !control5.Contains(control4))
                                    {
                                        if ((control4 != control5) && !control4.Contains(control5))
                                        {
                                            goto Label_024D;
                                        }
                                        component = control4.Parent;
                                    }
                                }
                                goto Label_03B7;
                            Label_0244:
                                control5 = control5.Parent;
                            Label_024D:
                                if ((control5 != null) && !control5.Contains(control4))
                                {
                                    goto Label_0244;
                                }
                                component = control5;
                                goto Label_03B7;
                            Label_0265:
                                designer4 = host.GetDesigner((IComponent) obj3) as ITreeDesigner;
                                ITreeDesigner designer = host.GetDesigner(component) as ITreeDesigner;
                                if (((designer4 != null) && (designer != null)) && (designer4.Parent != designer))
                                {
                                    ArrayList list2 = new ArrayList();
                                    ArrayList list3 = new ArrayList();
                                    for (designer4 = designer4.Parent as ITreeDesigner; designer4 != null; designer4 = designer4.Parent as ITreeDesigner)
                                    {
                                        list2.Add(designer4);
                                    }
                                    designer = designer.Parent as ITreeDesigner;
                                    while (designer != null)
                                    {
                                        list3.Add(designer);
                                        designer = designer.Parent as ITreeDesigner;
                                    }
                                    ArrayList list4 = (list2.Count < list3.Count) ? list2 : list3;
                                    ArrayList list5 = (list4 == list2) ? list3 : list2;
                                    designer = null;
                                    if ((list4.Count > 0) && (list5.Count > 0))
                                    {
                                        int num = Math.Max(0, list4.Count - 1);
                                        for (int i = Math.Max(0, list5.Count - 1); (num >= 0) && (i >= 0); i--)
                                        {
                                            if (list4[num] != list5[i])
                                            {
                                                break;
                                            }
                                            designer = (ITreeDesigner) list4[num];
                                            num--;
                                        }
                                    }
                                    if (designer != null)
                                    {
                                        component = designer.Component;
                                    }
                                    else
                                    {
                                        component = null;
                                    }
                                }
                            Label_03B7:
                                list6 = new ArrayList();
                                this.GetAssociatedComponents((IComponent) obj3, host, list6);
                                foreach (IComponent component4 in list6)
                                {
                                    service.OnComponentChanging(component4, null);
                                }
                                host.DestroyComponent((IComponent) obj3);
                            }
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Commit();
                            }
                            foreach (ParentControlDesigner designer6 in list)
                            {
                                if (designer6 != null)
                                {
                                    designer6.ResumeChangingEvents();
                                }
                            }
                        }
                        if ((component != null) && (this.SelectionService.PrimarySelection == null))
                        {
                            ITreeDesigner designer7 = host.GetDesigner(component) as ITreeDesigner;
                            if ((designer7 != null) && (designer7.Children != null))
                            {
                                foreach (IDesigner designer8 in designer7.Children)
                                {
                                    IComponent component5 = designer8.Component;
                                    if (component5.Site != null)
                                    {
                                        component = component5;
                                        break;
                                    }
                                }
                            }
                            else if (component is Control)
                            {
                                Control control6 = (Control) component;
                                if (control6.Controls.Count > 0)
                                {
                                    control6 = control6.Controls[0];
                                    while ((control6 != null) && (control6.Site == null))
                                    {
                                        control6 = control6.Parent;
                                    }
                                    component = control6;
                                }
                            }
                            if (component != null)
                            {
                                this.SelectionService.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                            }
                            else
                            {
                                this.SelectionService.SetSelectedComponents(new object[] { host.RootComponent }, SelectionTypes.Replace);
                            }
                        }
                        else if (this.SelectionService.PrimarySelection == null)
                        {
                            this.SelectionService.SetSelectedComponents(new object[] { host.RootComponent }, SelectionTypes.Replace);
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        protected void OnMenuDesignerProperties(object sender, EventArgs e)
        {
            object primarySelection = this.SelectionService.PrimarySelection;
            if (!this.CheckComponentEditor(primarySelection, true))
            {
                IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                if (service != null)
                {
                    service.GlobalInvoke(StandardCommands.PropertiesWindow);
                }
            }
        }

        protected void OnMenuPaste(object sender, EventArgs e)
        {
            Cursor current = Cursor.Current;
            ArrayList list = new ArrayList();
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                ICollection is2 = null;
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    IDataObject dataObject = Clipboard.GetDataObject();
                    ICollection is3 = null;
                    bool firstAdd = false;
                    ComponentTray tray = null;
                    int num = 0;
                    tray = this.GetService(typeof(ComponentTray)) as ComponentTray;
                    num = (tray != null) ? tray.Controls.Count : 0;
                    object data = dataObject.GetData("CF_DESIGNERCOMPONENTS_V2");
                    using (DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("CommandSetPaste")))
                    {
                        byte[] buffer = data as byte[];
                        if (buffer != null)
                        {
                            MemoryStream serializationStream = new MemoryStream(buffer);
                            if (serializationStream != null)
                            {
                                IDesignerSerializationService service = (IDesignerSerializationService) this.GetService(typeof(IDesignerSerializationService));
                                if (service != null)
                                {
                                    BinaryFormatter formatter = new BinaryFormatter();
                                    serializationStream.Seek(0L, SeekOrigin.Begin);
                                    object serializationData = formatter.Deserialize(serializationStream);
                                    is3 = service.Deserialize(serializationData);
                                }
                            }
                        }
                        else
                        {
                            IToolboxService service2 = (IToolboxService) this.GetService(typeof(IToolboxService));
                            if ((service2 != null) && service2.IsSupported(dataObject, host))
                            {
                                ToolboxItem item = service2.DeserializeToolboxItem(dataObject, host);
                                if (item != null)
                                {
                                    is3 = item.CreateComponents(host);
                                    firstAdd = true;
                                }
                            }
                        }
                        if ((is3 != null) && (is3.Count > 0))
                        {
                            object[] array = new object[is3.Count];
                            is3.CopyTo(array, 0);
                            ArrayList list2 = new ArrayList();
                            ArrayList list3 = new ArrayList();
                            string[] strArray = null;
                            int num2 = 0;
                            IComponent primarySelection = null;
                            IDesigner designer = null;
                            bool flag2 = false;
                            IComponent rootComponent = host.RootComponent;
                            primarySelection = (IComponent) this.SelectionService.PrimarySelection;
                            if (primarySelection == null)
                            {
                                primarySelection = rootComponent;
                            }
                            flag2 = false;
                            ITreeDesigner parent = host.GetDesigner(primarySelection) as ITreeDesigner;
                            while (!flag2 && (parent != null))
                            {
                                if (parent is IOleDragClient)
                                {
                                    designer = parent;
                                    flag2 = true;
                                }
                                else
                                {
                                    if (parent == parent.Parent)
                                    {
                                        break;
                                    }
                                    parent = parent.Parent as ITreeDesigner;
                                }
                            }
                            foreach (object obj5 in is3)
                            {
                                string name = null;
                                IComponent component = obj5 as IComponent;
                                if (obj5 is IComponent)
                                {
                                    if ((strArray != null) && (num2 < strArray.Length))
                                    {
                                        name = strArray[num2++];
                                    }
                                }
                                else
                                {
                                    string[] strArray2 = obj5 as string[];
                                    if ((strArray == null) && (strArray2 != null))
                                    {
                                        strArray = strArray2;
                                        num2 = 0;
                                        continue;
                                    }
                                }
                                IEventBindingService service3 = this.GetService(typeof(IEventBindingService)) as IEventBindingService;
                                if (service3 != null)
                                {
                                    foreach (PropertyDescriptor descriptor in service3.GetEventProperties(TypeDescriptor.GetEvents(component)))
                                    {
                                        if (((descriptor != null) && !descriptor.IsReadOnly) && (descriptor.GetValue(component) is string))
                                        {
                                            descriptor.SetValue(component, null);
                                        }
                                    }
                                }
                                if (flag2)
                                {
                                    bool flag3 = false;
                                    if (is2 != null)
                                    {
                                        foreach (Component component4 in is2)
                                        {
                                            if (component4 == (obj5 as Component))
                                            {
                                                flag3 = true;
                                                break;
                                            }
                                        }
                                    }
                                    if (!flag3)
                                    {
                                        ComponentDesigner designer3 = host.GetDesigner(component) as ComponentDesigner;
                                        ICollection associatedComponents = null;
                                        if (designer3 != null)
                                        {
                                            associatedComponents = designer3.AssociatedComponents;
                                            ComponentDesigner designer4 = ((ITreeDesigner) designer3).Parent as ComponentDesigner;
                                            Component component5 = null;
                                            if (designer4 != null)
                                            {
                                                component5 = designer4.Component as Component;
                                            }
                                            ArrayList list4 = new ArrayList();
                                            if ((component5 != null) && (designer4 != null))
                                            {
                                                foreach (IComponent component6 in designer4.AssociatedComponents)
                                                {
                                                    list4.Add(component6 as Component);
                                                }
                                            }
                                            if ((component5 == null) || !list4.Contains(component))
                                            {
                                                if (component5 != null)
                                                {
                                                    ParentControlDesigner designer5 = host.GetDesigner(component5) as ParentControlDesigner;
                                                    if ((designer5 != null) && !list.Contains(designer5))
                                                    {
                                                        designer5.SuspendChangingEvents();
                                                        list.Add(designer5);
                                                        designer5.ForceComponentChanging();
                                                    }
                                                }
                                                if (!((IOleDragClient) designer).AddComponent(component, name, firstAdd))
                                                {
                                                    is2 = associatedComponents;
                                                    return;
                                                }
                                                Control controlForComponent = ((IOleDragClient) designer).GetControlForComponent(component);
                                                if (controlForComponent != null)
                                                {
                                                    list3.Add(controlForComponent);
                                                }
                                                if (TypeDescriptor.GetAttributes(component).Contains(DesignTimeVisibleAttribute.Yes) || (component is ToolStripItem))
                                                {
                                                    list2.Add(component);
                                                }
                                            }
                                            else if (list4.Contains(component) && (Array.IndexOf<object>(array, component5) == -1))
                                            {
                                                list2.Add(component);
                                            }
                                            Control control2 = component as Control;
                                            bool flag4 = false;
                                            if (((control2 != null) && (name != null)) && name.Equals(control2.Text))
                                            {
                                                flag4 = true;
                                            }
                                            if (flag4)
                                            {
                                                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                                                PropertyDescriptor descriptor2 = properties["Name"];
                                                if ((descriptor2 != null) && (descriptor2.PropertyType == typeof(string)))
                                                {
                                                    string str3 = (string) descriptor2.GetValue(component);
                                                    if (!str3.Equals(name))
                                                    {
                                                        PropertyDescriptor descriptor3 = properties["Text"];
                                                        if ((descriptor3 != null) && (descriptor3.PropertyType == descriptor2.PropertyType))
                                                        {
                                                            descriptor3.SetValue(component, descriptor2.GetValue(component));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            ArrayList controls = new ArrayList();
                            foreach (Control control3 in list3)
                            {
                                if (host.GetDesigner(control3) is ControlDesigner)
                                {
                                    controls.Add(control3);
                                }
                            }
                            if (controls.Count > 0)
                            {
                                this.UpdatePastePositions(controls);
                            }
                            if (tray == null)
                            {
                                tray = this.GetService(typeof(ComponentTray)) as ComponentTray;
                            }
                            if (tray != null)
                            {
                                int num3 = tray.Controls.Count - num;
                                if (num3 > 0)
                                {
                                    ArrayList components = new ArrayList();
                                    for (int i = 0; i < num3; i++)
                                    {
                                        components.Add(tray.Controls[num + i]);
                                    }
                                    tray.UpdatePastePositions(components);
                                }
                            }
                            list3.Sort(new TabIndexCompare());
                            foreach (Control control4 in list3)
                            {
                                this.UpdatePasteTabIndex(control4, control4.Parent);
                            }
                            this.SelectionService.SetSelectedComponents(list2.ToArray(), SelectionTypes.Replace);
                            ParentControlDesigner designer7 = designer as ParentControlDesigner;
                            if ((designer7 != null) && designer7.AllowSetChildIndexOnDrop)
                            {
                                MenuCommand command = this.MenuService.FindCommand(StandardCommands.BringToFront);
                                if (command != null)
                                {
                                    command.Invoke();
                                }
                            }
                            transaction.Commit();
                        }
                    }
                }
            }
            finally
            {
                Cursor.Current = current;
                foreach (ParentControlDesigner designer8 in list)
                {
                    if (designer8 != null)
                    {
                        designer8.ResumeChangingEvents();
                    }
                }
            }
        }

        protected void OnMenuSelectAll(object sender, EventArgs e)
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if ((this.site != null) && (this.SelectionService != null))
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        object[] objArray;
                        ComponentCollection components = service.Container.Components;
                        if ((components == null) || (components.Count == 0))
                        {
                            objArray = new IComponent[0];
                        }
                        else
                        {
                            objArray = new object[components.Count - 1];
                            object rootComponent = service.RootComponent;
                            int num = 0;
                            foreach (IComponent component in components)
                            {
                                if (rootComponent != component)
                                {
                                    objArray[num++] = component;
                                }
                            }
                        }
                        this.SelectionService.SetSelectedComponents(objArray, SelectionTypes.Replace);
                    }
                }
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        protected void OnMenuShowGrid(object sender, EventArgs e)
        {
            if (this.site != null)
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    DesignerTransaction transaction = null;
                    try
                    {
                        transaction = service.CreateTransaction();
                        IComponent rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is Control))
                        {
                            PropertyDescriptor property = this.GetProperty(rootComponent, "DrawGrid");
                            if (property != null)
                            {
                                bool flag = (bool) property.GetValue(rootComponent);
                                property.SetValue(rootComponent, !flag);
                                ((MenuCommand) sender).Checked = !flag;
                            }
                        }
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
        }

        protected void OnMenuSizeToGrid(object sender, EventArgs e)
        {
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                DesignerTransaction transaction = null;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                    object[] array = new object[selectedComponents.Count];
                    selectedComponents.CopyTo(array, 0);
                    array = this.FilterSelection(array, SelectionRules.Visible);
                    Size empty = Size.Empty;
                    Point point = Point.Empty;
                    Size size2 = Size.Empty;
                    PropertyDescriptor descriptor = null;
                    PropertyDescriptor descriptor2 = null;
                    if (service != null)
                    {
                        transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetSizeToGrid", new object[] { array.Length }));
                        IComponent rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is Control))
                        {
                            PropertyDescriptor property = this.GetProperty(rootComponent, "CurrentGridSize");
                            if (property != null)
                            {
                                size2 = (Size) property.GetValue(rootComponent);
                            }
                        }
                    }
                    if (!size2.IsEmpty)
                    {
                        foreach (object obj2 in array)
                        {
                            IComponent comp = obj2 as IComponent;
                            if (obj2 != null)
                            {
                                descriptor = this.GetProperty(comp, "Size");
                                descriptor2 = this.GetProperty(comp, "Location");
                                if (((descriptor != null) && (descriptor2 != null)) && (!descriptor.IsReadOnly && !descriptor2.IsReadOnly))
                                {
                                    empty = (Size) descriptor.GetValue(comp);
                                    point = (Point) descriptor2.GetValue(comp);
                                    empty.Width = ((empty.Width + (size2.Width / 2)) / size2.Width) * size2.Width;
                                    empty.Height = ((empty.Height + (size2.Height / 2)) / size2.Height) * size2.Height;
                                    point.X = (point.X / size2.Width) * size2.Width;
                                    point.Y = (point.Y / size2.Height) * size2.Height;
                                    descriptor.SetValue(comp, empty);
                                    descriptor2.SetValue(comp, point);
                                }
                            }
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
            }
        }

        protected void OnMenuSizingCommand(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            CommandID commandID = command.CommandID;
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                try
                {
                    PropertyDescriptor property;
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                    object[] array = new object[selectedComponents.Count];
                    selectedComponents.CopyTo(array, 0);
                    array = this.FilterSelection(array, SelectionRules.Visible);
                    object primarySelection = this.SelectionService.PrimarySelection;
                    Size empty = Size.Empty;
                    Size size2 = Size.Empty;
                    IComponent comp = primarySelection as IComponent;
                    if (comp != null)
                    {
                        property = this.GetProperty(comp, "Size");
                        if (property == null)
                        {
                            return;
                        }
                        empty = (Size) property.GetValue(comp);
                    }
                    if (primarySelection != null)
                    {
                        IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                        DesignerTransaction transaction = null;
                        try
                        {
                            if (service != null)
                            {
                                transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetSize", new object[] { array.Length }));
                            }
                            foreach (object obj3 in array)
                            {
                                if (!obj3.Equals(primarySelection))
                                {
                                    IComponent component2 = obj3 as IComponent;
                                    if (component2 != null)
                                    {
                                        PropertyDescriptor descriptor2 = this.GetProperty(obj3, "Locked");
                                        if ((descriptor2 == null) || !((bool) descriptor2.GetValue(obj3)))
                                        {
                                            property = this.GetProperty(component2, "Size");
                                            if ((property != null) && !property.IsReadOnly)
                                            {
                                                size2 = (Size) property.GetValue(component2);
                                                if ((commandID == StandardCommands.SizeToControlHeight) || (commandID == StandardCommands.SizeToControl))
                                                {
                                                    size2.Height = empty.Height;
                                                }
                                                if ((commandID == StandardCommands.SizeToControlWidth) || (commandID == StandardCommands.SizeToControl))
                                                {
                                                    size2.Width = empty.Width;
                                                }
                                                property.SetValue(component2, size2);
                                            }
                                        }
                                    }
                                }
                            }
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
                finally
                {
                    Cursor.Current = current;
                }
            }
        }

        protected void OnMenuSnapToGrid(object sender, EventArgs e)
        {
            if (this.site != null)
            {
                IDesignerHost service = (IDesignerHost) this.site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    DesignerTransaction transaction = null;
                    try
                    {
                        transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetPaste", new object[] { 0 }));
                        IComponent rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is Control))
                        {
                            PropertyDescriptor property = this.GetProperty(rootComponent, "SnapToGrid");
                            if (property != null)
                            {
                                bool flag = (bool) property.GetValue(rootComponent);
                                property.SetValue(rootComponent, !flag);
                                ((MenuCommand) sender).Checked = !flag;
                            }
                        }
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
        }

        protected void OnMenuSpacingCommand(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            CommandID commandID = command.CommandID;
            DesignerTransaction transaction = null;
            if (this.SelectionService != null)
            {
                Cursor current = Cursor.Current;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Size empty = Size.Empty;
                    ICollection selectedComponents = this.SelectionService.GetSelectedComponents();
                    object[] array = new object[selectedComponents.Count];
                    selectedComponents.CopyTo(array, 0);
                    if (service != null)
                    {
                        transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetFormatSpacing", new object[] { array.Length }));
                        IComponent rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is Control))
                        {
                            PropertyDescriptor property = this.GetProperty(rootComponent, "CurrentGridSize");
                            if (property != null)
                            {
                                empty = (Size) property.GetValue(rootComponent);
                            }
                        }
                    }
                    array = this.FilterSelection(array, SelectionRules.Visible);
                    int num = 0;
                    PropertyDescriptor descriptor2 = null;
                    PropertyDescriptor descriptor3 = null;
                    PropertyDescriptor descriptor4 = null;
                    PropertyDescriptor descriptor5 = null;
                    Size size2 = Size.Empty;
                    Size size3 = Size.Empty;
                    Point point = Point.Empty;
                    Point point2 = Point.Empty;
                    Point point3 = Point.Empty;
                    IComponent comp = null;
                    IComponent component3 = null;
                    int nSortBy = -1;
                    if (((commandID == StandardCommands.HorizSpaceConcatenate) || (commandID == StandardCommands.HorizSpaceDecrease)) || ((commandID == StandardCommands.HorizSpaceIncrease) || (commandID == StandardCommands.HorizSpaceMakeEqual)))
                    {
                        nSortBy = 0;
                    }
                    else
                    {
                        if (((commandID != StandardCommands.VertSpaceConcatenate) && (commandID != StandardCommands.VertSpaceDecrease)) && ((commandID != StandardCommands.VertSpaceIncrease) && (commandID != StandardCommands.VertSpaceMakeEqual)))
                        {
                            throw new ArgumentException(System.Design.SR.GetString("CommandSetUnknownSpacingCommand"));
                        }
                        nSortBy = 1;
                    }
                    this.SortSelection(array, nSortBy);
                    object primarySelection = this.SelectionService.PrimarySelection;
                    int index = 0;
                    if (primarySelection != null)
                    {
                        index = Array.IndexOf<object>(array, primarySelection);
                    }
                    if ((commandID == StandardCommands.HorizSpaceMakeEqual) || (commandID == StandardCommands.VertSpaceMakeEqual))
                    {
                        int num4 = 0;
                        int num5 = 0;
                        while (num5 < array.Length)
                        {
                            size2 = Size.Empty;
                            IComponent component4 = array[num5] as IComponent;
                            if (component4 != null)
                            {
                                comp = component4;
                                descriptor2 = this.GetProperty(comp, "Size");
                                if (descriptor2 != null)
                                {
                                    size2 = (Size) descriptor2.GetValue(comp);
                                }
                            }
                            if (nSortBy == 0)
                            {
                                num4 += size2.Width;
                            }
                            else
                            {
                                num4 += size2.Height;
                            }
                            num5++;
                        }
                        component3 = (IComponent) (comp = null);
                        size2 = Size.Empty;
                        point = Point.Empty;
                        num5 = 0;
                        while (num5 < array.Length)
                        {
                            comp = array[num5] as IComponent;
                            if (comp != null)
                            {
                                if ((component3 == null) || (comp.GetType() != component3.GetType()))
                                {
                                    descriptor2 = this.GetProperty(comp, "Size");
                                    descriptor4 = this.GetProperty(comp, "Location");
                                }
                                component3 = comp;
                                if (descriptor4 != null)
                                {
                                    point = (Point) descriptor4.GetValue(comp);
                                    if (descriptor2 != null)
                                    {
                                        size2 = (Size) descriptor2.GetValue(comp);
                                        if (!size2.IsEmpty && !point.IsEmpty)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            num5++;
                        }
                        for (num5 = array.Length - 1; num5 >= 0; num5--)
                        {
                            comp = array[num5] as IComponent;
                            if (comp != null)
                            {
                                if ((component3 == null) || (comp.GetType() != component3.GetType()))
                                {
                                    descriptor2 = this.GetProperty(comp, "Size");
                                    descriptor4 = this.GetProperty(comp, "Location");
                                }
                                component3 = comp;
                                if (descriptor4 != null)
                                {
                                    point2 = (Point) descriptor4.GetValue(comp);
                                    if (descriptor2 != null)
                                    {
                                        size3 = (Size) descriptor2.GetValue(comp);
                                        if ((descriptor2 != null) && (descriptor4 != null))
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if ((descriptor2 != null) && (descriptor4 != null))
                        {
                            if (nSortBy == 0)
                            {
                                num = (((size3.Width + point2.X) - point.X) - num4) / (array.Length - 1);
                            }
                            else
                            {
                                num = (((size3.Height + point2.Y) - point.Y) - num4) / (array.Length - 1);
                            }
                            if (num < 0)
                            {
                                num = 0;
                            }
                        }
                    }
                    comp = (IComponent) (component3 = null);
                    if (primarySelection != null)
                    {
                        PropertyDescriptor descriptor6 = this.GetProperty(primarySelection, "Location");
                        if (descriptor6 != null)
                        {
                            point3 = (Point) descriptor6.GetValue(primarySelection);
                        }
                    }
                    for (int i = 0; i < array.Length; i++)
                    {
                        comp = (IComponent) array[i];
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(comp);
                        PropertyDescriptor descriptor7 = properties["Locked"];
                        if ((descriptor7 == null) || !((bool) descriptor7.GetValue(comp)))
                        {
                            if ((component3 == null) || (component3.GetType() != comp.GetType()))
                            {
                                descriptor2 = properties["Size"];
                                descriptor4 = properties["Location"];
                            }
                            else
                            {
                                descriptor2 = descriptor3;
                                descriptor4 = descriptor5;
                            }
                            if (descriptor4 != null)
                            {
                                point = (Point) descriptor4.GetValue(comp);
                                if (descriptor2 != null)
                                {
                                    size2 = (Size) descriptor2.GetValue(comp);
                                    int num7 = Math.Max(0, i - 1);
                                    component3 = (IComponent) array[num7];
                                    if (component3.GetType() != comp.GetType())
                                    {
                                        descriptor3 = this.GetProperty(component3, "Size");
                                        descriptor5 = this.GetProperty(component3, "Location");
                                    }
                                    else
                                    {
                                        descriptor3 = descriptor2;
                                        descriptor5 = descriptor4;
                                    }
                                    if (descriptor5 != null)
                                    {
                                        point2 = (Point) descriptor5.GetValue(component3);
                                        if (descriptor3 != null)
                                        {
                                            size3 = (Size) descriptor3.GetValue(component3);
                                            if ((commandID == StandardCommands.HorizSpaceConcatenate) && (i > 0))
                                            {
                                                point.X = point2.X + size3.Width;
                                            }
                                            else if (commandID == StandardCommands.HorizSpaceDecrease)
                                            {
                                                if (index < i)
                                                {
                                                    point.X -= empty.Width * (i - index);
                                                    if (point.X < point3.X)
                                                    {
                                                        point.X = point3.X;
                                                    }
                                                }
                                                else if (index > i)
                                                {
                                                    point.X += empty.Width * (index - i);
                                                    if (point.X > point3.X)
                                                    {
                                                        point.X = point3.X;
                                                    }
                                                }
                                            }
                                            else if (commandID == StandardCommands.HorizSpaceIncrease)
                                            {
                                                if (index < i)
                                                {
                                                    point.X += empty.Width * (i - index);
                                                }
                                                else if (index > i)
                                                {
                                                    point.X -= empty.Width * (index - i);
                                                }
                                            }
                                            else if ((commandID == StandardCommands.HorizSpaceMakeEqual) && (i > 0))
                                            {
                                                point.X = (point2.X + size3.Width) + num;
                                            }
                                            else if ((commandID == StandardCommands.VertSpaceConcatenate) && (i > 0))
                                            {
                                                point.Y = point2.Y + size3.Height;
                                            }
                                            else if (commandID == StandardCommands.VertSpaceDecrease)
                                            {
                                                if (index < i)
                                                {
                                                    point.Y -= empty.Height * (i - index);
                                                    if (point.Y < point3.Y)
                                                    {
                                                        point.Y = point3.Y;
                                                    }
                                                }
                                                else if (index > i)
                                                {
                                                    point.Y += empty.Height * (index - i);
                                                    if (point.Y > point3.Y)
                                                    {
                                                        point.Y = point3.Y;
                                                    }
                                                }
                                            }
                                            else if (commandID == StandardCommands.VertSpaceIncrease)
                                            {
                                                if (index < i)
                                                {
                                                    point.Y += empty.Height * (i - index);
                                                }
                                                else if (index > i)
                                                {
                                                    point.Y -= empty.Height * (index - i);
                                                }
                                            }
                                            else if ((commandID == StandardCommands.VertSpaceMakeEqual) && (i > 0))
                                            {
                                                point.Y = (point2.Y + size3.Height) + num;
                                            }
                                            if (!descriptor4.IsReadOnly)
                                            {
                                                descriptor4.SetValue(comp, point);
                                            }
                                            component3 = comp;
                                        }
                                    }
                                }
                            }
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
            }
        }

        protected void OnSelectionChanged(object sender, EventArgs e)
        {
            if (this.SelectionService != null)
            {
                this.selectionVersion++;
                this.selCount = this.SelectionService.SelectionCount;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((this.selCount > 0) && (service != null))
                {
                    object rootComponent = service.RootComponent;
                    if ((rootComponent != null) && this.SelectionService.GetComponentSelected(rootComponent))
                    {
                        this.selCount = 0;
                    }
                }
                this.primarySelection = this.SelectionService.PrimarySelection as IComponent;
                this.selectionInherited = false;
                this.controlsOnlySelection = true;
                if (this.selCount > 0)
                {
                    foreach (object obj3 in this.SelectionService.GetSelectedComponents())
                    {
                        if (!(obj3 is Control))
                        {
                            this.controlsOnlySelection = false;
                        }
                        if (!TypeDescriptor.GetAttributes(obj3)[typeof(InheritanceAttribute)].Equals(InheritanceAttribute.NotInherited))
                        {
                            this.selectionInherited = true;
                            break;
                        }
                    }
                }
                this.OnUpdateCommandStatus();
            }
        }

        private void OnSnapLineTimerExpire(object sender, EventArgs e)
        {
            Control adornerWindowControl = this.BehaviorService.AdornerWindowControl;
            if ((adornerWindowControl != null) && adornerWindowControl.IsHandleCreated)
            {
                adornerWindowControl.BeginInvoke(new EventHandler(this.OnSnapLineTimerExpireMarshalled), new object[] { sender, e });
            }
        }

        private void OnSnapLineTimerExpireMarshalled(object sender, EventArgs e)
        {
            this.snapLineTimer.Stop();
            this.EndDragManager();
        }

        protected void OnStatusAlways(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = true;
        }

        protected void OnStatusAnySelection(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = this.selCount > 0;
        }

        protected void OnStatusCopy(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            bool flag = false;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((!this.selectionInherited && (host != null)) && !host.Loading)
            {
                ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    ICollection selectedComponents = service.GetSelectedComponents();
                    object rootComponent = host.RootComponent;
                    if (!service.GetComponentSelected(rootComponent))
                    {
                        foreach (object obj3 in selectedComponents)
                        {
                            IComponent component = obj3 as IComponent;
                            if (((component != null) && (component.Site != null)) && (component.Site.Container == host.Container))
                            {
                                flag = true;
                                break;
                            }
                        }
                    }
                }
            }
            command.Enabled = flag;
        }

        protected void OnStatusCut(object sender, EventArgs e)
        {
            this.OnStatusDelete(sender, e);
            if (((MenuCommand) sender).Enabled)
            {
                this.OnStatusCopy(sender, e);
            }
        }

        protected void OnStatusDelete(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            if (this.selectionInherited)
            {
                command.Enabled = false;
            }
            else
            {
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (host != null)
                {
                    ISelectionService service = (ISelectionService) this.GetService(typeof(ISelectionService));
                    if (service != null)
                    {
                        foreach (object obj2 in service.GetSelectedComponents())
                        {
                            IComponent component = obj2 as IComponent;
                            if ((component != null) && ((component.Site == null) || ((component.Site != null) && (component.Site.Container != host.Container))))
                            {
                                command.Enabled = false;
                                return;
                            }
                        }
                    }
                }
                this.OnStatusAnySelection(sender, e);
            }
        }

        protected void OnStatusPaste(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (((this.primarySelection != null) && (host != null)) && (host.GetDesigner(this.primarySelection) is ParentControlDesigner))
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(this.primarySelection)[typeof(InheritanceAttribute)];
                if (attribute.InheritanceLevel == InheritanceLevel.InheritedReadOnly)
                {
                    command.Enabled = false;
                    return;
                }
            }
            IDataObject dataObject = Clipboard.GetDataObject();
            bool flag = false;
            if (dataObject != null)
            {
                if (dataObject.GetDataPresent("CF_DESIGNERCOMPONENTS_V2"))
                {
                    flag = true;
                }
                else
                {
                    IToolboxService service = (IToolboxService) this.GetService(typeof(IToolboxService));
                    if (service != null)
                    {
                        flag = (host != null) ? service.IsSupported(dataObject, host) : service.IsToolboxItem(dataObject);
                    }
                }
            }
            command.Enabled = flag;
        }

        private void OnStatusPrimarySelection(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = this.primarySelection != null;
        }

        protected virtual void OnStatusSelectAll(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            command.Enabled = service.Container.Components.Count > 1;
        }

        protected virtual void OnUpdateCommandStatus()
        {
            for (int i = 0; i < this.commandSet.Length; i++)
            {
                this.commandSet[i].UpdateStatus();
            }
        }

        private ICollection PrependComponentNames(ICollection objects)
        {
            object[] objArray = new object[objects.Count + 1];
            int num = 1;
            ArrayList list = new ArrayList(objects.Count);
            foreach (object obj2 in objects)
            {
                IComponent component = obj2 as IComponent;
                if (component != null)
                {
                    string name = null;
                    if (component.Site != null)
                    {
                        name = component.Site.Name;
                    }
                    list.Add(name);
                }
                objArray[num++] = obj2;
            }
            string[] array = new string[list.Count];
            list.CopyTo(array, 0);
            objArray[0] = array;
            return objArray;
        }

        private void SortSelection(object[] selectedObjects, int nSortBy)
        {
            IComparer comparer = null;
            switch (nSortBy)
            {
                case 0:
                    comparer = new ComponentLeftCompare();
                    break;

                case 1:
                    comparer = new ComponentTopCompare();
                    break;

                case 2:
                    comparer = new ControlZOrderCompare();
                    break;

                default:
                    return;
            }
            Array.Sort(selectedObjects, comparer);
        }

        private void UpdateClipboardItems(object s, EventArgs e)
        {
            int num = 0;
            for (int i = 0; (num < 3) && (i < this.commandSet.Length); i++)
            {
                CommandSetItem item = this.commandSet[i];
                if (((item.CommandID == StandardCommands.Paste) || (item.CommandID == StandardCommands.Copy)) || (item.CommandID == StandardCommands.Cut))
                {
                    num++;
                    item.UpdateStatus();
                }
            }
        }

        private void UpdatePastePositions(ArrayList controls)
        {
            if (controls.Count != 0)
            {
                Control parent = ((Control) controls[0]).Parent;
                Point location = ((Control) controls[0]).Location;
                Point point2 = location;
                foreach (Control control2 in controls)
                {
                    Point point3 = control2.Location;
                    Size size = control2.Size;
                    if (location.X > point3.X)
                    {
                        location.X = point3.X;
                    }
                    if (location.Y > point3.Y)
                    {
                        location.Y = point3.Y;
                    }
                    if (point2.X < (point3.X + size.Width))
                    {
                        point2.X = point3.X + size.Width;
                    }
                    if (point2.Y < (point3.Y + size.Height))
                    {
                        point2.Y = point3.Y + size.Height;
                    }
                }
                Point pos = new Point(-location.X, -location.Y);
                if (parent != null)
                {
                    bool flag;
                    Point point5;
                    bool flag2 = false;
                    Size clientSize = parent.ClientSize;
                    Size empty = Size.Empty;
                    point5 = new Point(clientSize.Width / 2, clientSize.Height / 2) {
                        X = point5.X - ((point2.X - location.X) / 2),
                        Y = point5.Y - ((point2.Y - location.Y) / 2)
                    };
                    do
                    {
                        flag = false;
                        foreach (Control control3 in parent.Controls)
                        {
                            Rectangle bounds = control3.Bounds;
                            if (controls.Contains(control3))
                            {
                                if (!control3.Size.Equals(clientSize))
                                {
                                    continue;
                                }
                                bounds.Offset(pos);
                            }
                            Control control4 = (Control) controls[0];
                            Rectangle rectangle2 = control4.Bounds;
                            rectangle2.Offset(pos);
                            rectangle2.Offset(point5);
                            if (rectangle2.Equals(bounds))
                            {
                                int num;
                                int num2;
                                flag = true;
                                if (empty.IsEmpty)
                                {
                                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                                    IComponent rootComponent = service.RootComponent;
                                    if ((rootComponent != null) && (rootComponent is Control))
                                    {
                                        PropertyDescriptor property = this.GetProperty(rootComponent, "GridSize");
                                        if (property != null)
                                        {
                                            empty = (Size) property.GetValue(rootComponent);
                                        }
                                    }
                                    if (empty.IsEmpty)
                                    {
                                        empty.Width = 8;
                                        empty.Height = 8;
                                    }
                                }
                                point5 += empty;
                                if (controls.Count > 1)
                                {
                                    num = (point5.X + point2.X) - location.X;
                                    num2 = (point5.Y + point2.Y) - location.Y;
                                }
                                else
                                {
                                    num = point5.X + empty.Width;
                                    num2 = point5.Y + empty.Height;
                                }
                                if ((num > clientSize.Width) || (num2 > clientSize.Height))
                                {
                                    point5.X = 0;
                                    point5.Y = 0;
                                    if (flag2)
                                    {
                                        flag = false;
                                    }
                                    else
                                    {
                                        flag2 = true;
                                    }
                                }
                                break;
                            }
                        }
                    }
                    while (flag);
                    pos.Offset(point5.X, point5.Y);
                }
                if (parent != null)
                {
                    parent.SuspendLayout();
                }
                try
                {
                    foreach (Control control5 in controls)
                    {
                        Point point6 = control5.Location;
                        point6.Offset(pos.X, pos.Y);
                        control5.Location = point6;
                    }
                }
                finally
                {
                    if (parent != null)
                    {
                        parent.ResumeLayout();
                    }
                }
            }
        }

        private void UpdatePasteTabIndex(Control componentControl, object parentComponent)
        {
            Control control = parentComponent as Control;
            if ((control != null) && (componentControl != null))
            {
                bool flag = false;
                int tabIndex = componentControl.TabIndex;
                int num2 = 0;
                foreach (Control control2 in control.Controls)
                {
                    int num3 = control2.TabIndex;
                    if (num2 <= num3)
                    {
                        num2 = num3 + 1;
                    }
                    if (num3 == tabIndex)
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    componentControl.TabIndex = num2;
                }
            }
        }

        protected System.Windows.Forms.Design.Behavior.BehaviorService BehaviorService
        {
            get
            {
                if (this.behaviorService == null)
                {
                    this.behaviorService = this.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService)) as System.Windows.Forms.Design.Behavior.BehaviorService;
                }
                return this.behaviorService;
            }
        }

        protected IMenuCommandService MenuService
        {
            get
            {
                if (this.menuService == null)
                {
                    this.menuService = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
                }
                return this.menuService;
            }
        }

        protected ISelectionService SelectionService
        {
            get
            {
                return this.selectionService;
            }
        }

        protected int SelectionVersion
        {
            get
            {
                return this.selectionVersion;
            }
        }

        protected Timer SnapLineTimer
        {
            get
            {
                if (this.snapLineTimer == null)
                {
                    this.snapLineTimer = new Timer();
                    this.snapLineTimer.Interval = DesignerUtils.SNAPELINEDELAY;
                    this.snapLineTimer.Tick += new EventHandler(this.OnSnapLineTimerExpire);
                }
                return this.snapLineTimer;
            }
        }

        protected class CommandSetItem : MenuCommand
        {
            private CommandSet commandSet;
            private static Hashtable commandStatusHash;
            private IEventHandlerService eventService;
            private EventHandler statusHandler;
            private IUIService uiService;
            private bool updatingCommand;

            public CommandSetItem(CommandSet commandSet, EventHandler statusHandler, EventHandler invokeHandler, CommandID id) : this(commandSet, statusHandler, invokeHandler, id, false, null)
            {
            }

            public CommandSetItem(CommandSet commandSet, EventHandler statusHandler, EventHandler invokeHandler, CommandID id, bool optimizeStatus) : this(commandSet, statusHandler, invokeHandler, id, optimizeStatus, null)
            {
            }

            public CommandSetItem(CommandSet commandSet, EventHandler statusHandler, EventHandler invokeHandler, CommandID id, IUIService uiService) : this(commandSet, statusHandler, invokeHandler, id, false, uiService)
            {
            }

            public CommandSetItem(CommandSet commandSet, EventHandler statusHandler, EventHandler invokeHandler, CommandID id, bool optimizeStatus, IUIService uiService) : base(invokeHandler, id)
            {
                this.uiService = uiService;
                this.eventService = commandSet.eventService;
                this.statusHandler = statusHandler;
                if (optimizeStatus && (statusHandler != null))
                {
                    this.commandSet = commandSet;
                    lock (typeof(CommandSet.CommandSetItem))
                    {
                        if (commandStatusHash == null)
                        {
                            commandStatusHash = new Hashtable();
                        }
                    }
                    StatusState state = commandStatusHash[statusHandler] as StatusState;
                    if (state == null)
                    {
                        state = new StatusState();
                        commandStatusHash.Add(statusHandler, state);
                    }
                    state.refCount++;
                }
            }

            private void ApplyCachedStatus()
            {
                if ((this.commandSet != null) && commandStatusHash.Contains(this.statusHandler))
                {
                    try
                    {
                        this.updatingCommand = true;
                        (commandStatusHash[this.statusHandler] as StatusState).ApplyState(this);
                    }
                    finally
                    {
                        this.updatingCommand = false;
                    }
                }
            }

            public virtual void Dispose()
            {
                StatusState state = commandStatusHash[this.statusHandler] as StatusState;
                if (state != null)
                {
                    state.refCount--;
                    if (state.refCount == 0)
                    {
                        commandStatusHash.Remove(this.statusHandler);
                    }
                }
            }

            public override void Invoke()
            {
                try
                {
                    if (this.eventService != null)
                    {
                        IMenuStatusHandler handler = (IMenuStatusHandler) this.eventService.GetHandler(typeof(IMenuStatusHandler));
                        if ((handler != null) && handler.OverrideInvoke(this))
                        {
                            return;
                        }
                    }
                    base.Invoke();
                }
                catch (Exception exception)
                {
                    if (this.uiService != null)
                    {
                        this.uiService.ShowError(exception, System.Design.SR.GetString("CommandSetError", new object[] { exception.Message }));
                    }
                    if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                    {
                        throw;
                    }
                }
            }

            protected override void OnCommandChanged(EventArgs e)
            {
                if (!this.updatingCommand)
                {
                    base.OnCommandChanged(e);
                }
            }

            private void SaveCommandStatus()
            {
                if (this.commandSet != null)
                {
                    StatusState state = null;
                    if (commandStatusHash.Contains(this.statusHandler))
                    {
                        state = commandStatusHash[this.statusHandler] as StatusState;
                    }
                    else
                    {
                        state = new StatusState();
                    }
                    state.SaveState(this, this.commandSet.SelectionVersion);
                }
            }

            public void UpdateStatus()
            {
                if (this.eventService != null)
                {
                    IMenuStatusHandler handler = (IMenuStatusHandler) this.eventService.GetHandler(typeof(IMenuStatusHandler));
                    if ((handler != null) && handler.OverrideStatus(this))
                    {
                        return;
                    }
                }
                if (this.statusHandler != null)
                {
                    if (!this.CommandStatusValid)
                    {
                        try
                        {
                            this.statusHandler(this, EventArgs.Empty);
                            this.SaveCommandStatus();
                        }
                        catch
                        {
                        }
                    }
                    else
                    {
                        this.ApplyCachedStatus();
                    }
                }
            }

            private bool CommandStatusValid
            {
                get
                {
                    if ((this.commandSet != null) && commandStatusHash.Contains(this.statusHandler))
                    {
                        StatusState state = commandStatusHash[this.statusHandler] as StatusState;
                        if ((state != null) && (state.SelectionVersion == this.commandSet.SelectionVersion))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            private class StatusState
            {
                private const int Checked = 4;
                private const int Enabled = 1;
                private const int NeedsUpdate = 0x10;
                internal int refCount;
                private int selectionVersion;
                private int statusFlags = 0x10;
                private const int Supported = 8;
                private const int Visible = 2;

                internal void ApplyState(CommandSet.CommandSetItem item)
                {
                    item.Enabled = (this.statusFlags & 1) == 1;
                    item.Visible = (this.statusFlags & 2) == 2;
                    item.Checked = (this.statusFlags & 4) == 4;
                    item.Supported = (this.statusFlags & 8) == 8;
                }

                internal void SaveState(CommandSet.CommandSetItem item, int version)
                {
                    this.selectionVersion = version;
                    this.statusFlags = 0;
                    if (item.Enabled)
                    {
                        this.statusFlags |= 1;
                    }
                    if (item.Visible)
                    {
                        this.statusFlags |= 2;
                    }
                    if (item.Checked)
                    {
                        this.statusFlags |= 4;
                    }
                    if (item.Supported)
                    {
                        this.statusFlags |= 8;
                    }
                }

                public int SelectionVersion
                {
                    get
                    {
                        return this.selectionVersion;
                    }
                }
            }
        }

        private class ComponentLeftCompare : IComparer
        {
            public int Compare(object p, object q)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(p)["Location"];
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(q)["Location"];
                Point point = (Point) descriptor.GetValue(p);
                Point point2 = (Point) descriptor2.GetValue(q);
                if (point.X == point2.X)
                {
                    return (point.Y - point2.Y);
                }
                return (point.X - point2.X);
            }
        }

        private class ComponentTopCompare : IComparer
        {
            public int Compare(object p, object q)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(p)["Location"];
                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(q)["Location"];
                Point point = (Point) descriptor.GetValue(p);
                Point point2 = (Point) descriptor2.GetValue(q);
                if (point.Y == point2.Y)
                {
                    return (point.X - point2.X);
                }
                return (point.Y - point2.Y);
            }
        }

        private class ControlZOrderCompare : IComparer
        {
            public int Compare(object p, object q)
            {
                if (p == null)
                {
                    return -1;
                }
                if (q != null)
                {
                    if (p == q)
                    {
                        return 0;
                    }
                    Control child = p as Control;
                    Control control2 = q as Control;
                    if ((child == null) || (control2 == null))
                    {
                        return 1;
                    }
                    if ((child.Parent == control2.Parent) && (child.Parent != null))
                    {
                        return (child.Parent.Controls.GetChildIndex(child) - child.Parent.Controls.GetChildIndex(control2));
                    }
                }
                return 1;
            }
        }

        protected class ImmediateCommandSetItem : CommandSet.CommandSetItem
        {
            public ImmediateCommandSetItem(CommandSet commandSet, EventHandler statusHandler, EventHandler invokeHandler, CommandID id, IUIService uiService) : base(commandSet, statusHandler, invokeHandler, id, uiService)
            {
            }

            public override int OleStatus
            {
                get
                {
                    base.UpdateStatus();
                    return base.OleStatus;
                }
            }
        }

        private class TabIndexCompare : IComparer
        {
            public int Compare(object p, object q)
            {
                Control control = p as Control;
                Control control2 = q as Control;
                if (control == control2)
                {
                    return 0;
                }
                if (control == null)
                {
                    return -1;
                }
                if (control2 == null)
                {
                    return 1;
                }
                return (control.TabIndex - control2.TabIndex);
            }
        }
    }
}

