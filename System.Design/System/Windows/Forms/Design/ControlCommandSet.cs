namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ControlCommandSet : CommandSet
    {
        private Control baseControl;
        private CommandSet.CommandSetItem[] commandSet;
        private StatusCommandUI statusCommandUI;
        private TabOrder tabOrder;

        public ControlCommandSet(ISite site) : base(site)
        {
            this.statusCommandUI = new StatusCommandUI(site);
            this.commandSet = new CommandSet.CommandSetItem[] { 
                new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuAlignByPrimary), StandardCommands.AlignLeft, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuAlignByPrimary), StandardCommands.AlignTop, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusControlsOnlySelectionAndGrid), new EventHandler(this.OnMenuAlignToGrid), StandardCommands.AlignToGrid, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuAlignByPrimary), StandardCommands.AlignBottom, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuAlignByPrimary), StandardCommands.AlignHorizontalCenters, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuAlignByPrimary), StandardCommands.AlignRight, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuAlignByPrimary), StandardCommands.AlignVerticalCenters, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusControlsOnlySelection), new EventHandler(this.OnMenuCenterSelection), StandardCommands.CenterHorizontally, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusControlsOnlySelection), new EventHandler(this.OnMenuCenterSelection), StandardCommands.CenterVertically, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.HorizSpaceConcatenate, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.HorizSpaceDecrease, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.HorizSpaceIncrease, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.HorizSpaceMakeEqual, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.VertSpaceConcatenate, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.VertSpaceDecrease, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.VertSpaceIncrease, true), 
                new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectNonContained), new EventHandler(this.OnMenuSpacingCommand), StandardCommands.VertSpaceMakeEqual, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuSizingCommand), StandardCommands.SizeToControl, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuSizingCommand), StandardCommands.SizeToControlWidth, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusMultiSelectPrimary), new EventHandler(this.OnMenuSizingCommand), StandardCommands.SizeToControlHeight, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusControlsOnlySelectionAndGrid), new EventHandler(this.OnMenuSizeToGrid), StandardCommands.SizeToGrid, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusZOrder), new EventHandler(this.OnMenuZOrderSelection), StandardCommands.BringToFront, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusZOrder), new EventHandler(this.OnMenuZOrderSelection), StandardCommands.SendToBack, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusShowGrid), new EventHandler(this.OnMenuShowGrid), StandardCommands.ShowGrid, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusSnapToGrid), new EventHandler(this.OnMenuSnapToGrid), StandardCommands.SnapToGrid, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAnyControls), new EventHandler(this.OnMenuTabOrder), StandardCommands.TabOrder, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusLockControls), new EventHandler(this.OnMenuLockControls), StandardCommands.LockControls, true), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeySizeWidthIncrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeySizeHeightIncrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeySizeWidthDecrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeySizeHeightDecrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeyNudgeWidthIncrease), 
                new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeyNudgeHeightIncrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeyNudgeWidthDecrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySize), MenuCommands.KeyNudgeHeightDecrease), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySelect), MenuCommands.KeySelectNext), new CommandSet.CommandSetItem(this, new EventHandler(this.OnStatusAlways), new EventHandler(this.OnKeySelect), MenuCommands.KeySelectPrevious)
             };
            if (base.MenuService != null)
            {
                for (int i = 0; i < this.commandSet.Length; i++)
                {
                    base.MenuService.AddCommand(this.commandSet[i]);
                }
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                Control rootComponent = service.RootComponent as Control;
                if (rootComponent != null)
                {
                    this.baseControl = rootComponent;
                }
            }
        }

        private bool CheckSelectionParenting()
        {
            ICollection selectedComponents = base.SelectionService.GetSelectedComponents();
            Hashtable hashtable = new Hashtable(selectedComponents.Count);
            foreach (object obj2 in selectedComponents)
            {
                Control control = obj2 as Control;
                if ((control == null) || (control.Site == null))
                {
                    return false;
                }
                hashtable.Add(obj2, obj2);
            }
            Control parent = null;
            foreach (object obj3 in selectedComponents)
            {
                Control control3 = obj3 as Control;
                if ((control3 == null) || (control3.Site == null))
                {
                    return false;
                }
                for (Control control4 = control3.Parent; control4 != null; control4 = control4.Parent)
                {
                    if (control4 != parent)
                    {
                        object obj4 = hashtable[control4];
                        if ((obj4 != null) && (obj4 != obj3))
                        {
                            return false;
                        }
                    }
                }
                parent = control3.Parent;
            }
            return true;
        }

        public override void Dispose()
        {
            if (base.MenuService != null)
            {
                for (int i = 0; i < this.commandSet.Length; i++)
                {
                    base.MenuService.RemoveCommand(this.commandSet[i]);
                    this.commandSet[i].Dispose();
                }
            }
            if (this.tabOrder != null)
            {
                this.tabOrder.Dispose();
                this.tabOrder = null;
            }
            this.statusCommandUI = null;
            base.Dispose();
        }

        private ArrayList GenerateSnapLines(SelectionRules rules, Control primaryControl, int directionOffsetX, int directionOffsetY)
        {
            ArrayList list = new ArrayList(2);
            Point point = base.BehaviorService.ControlToAdornerWindow(primaryControl);
            bool flag = (primaryControl.Parent != null) && primaryControl.Parent.IsMirrored;
            if (directionOffsetX != 0)
            {
                if (!flag)
                {
                    if ((rules & SelectionRules.RightSizeable) != SelectionRules.None)
                    {
                        list.Add(new SnapLine(SnapLineType.Right, (point.X + primaryControl.Width) - 1));
                        list.Add(new SnapLine(SnapLineType.Vertical, (point.X + primaryControl.Width) + primaryControl.Margin.Right, "Margin.Right", SnapLinePriority.Always));
                    }
                }
                else if ((rules & SelectionRules.LeftSizeable) != SelectionRules.None)
                {
                    list.Add(new SnapLine(SnapLineType.Left, point.X));
                    list.Add(new SnapLine(SnapLineType.Vertical, point.X - primaryControl.Margin.Left, "Margin.Left", SnapLinePriority.Always));
                }
            }
            if ((directionOffsetY != 0) && ((rules & SelectionRules.BottomSizeable) != SelectionRules.None))
            {
                list.Add(new SnapLine(SnapLineType.Bottom, (point.Y + primaryControl.Height) - 1));
                list.Add(new SnapLine(SnapLineType.Horizontal, (point.Y + primaryControl.Height) + primaryControl.Margin.Bottom, "Margin.Bottom", SnapLinePriority.Always));
            }
            return list;
        }

        private Control GetNextControlInTab(Control basectl, Control ctl, bool forward)
        {
            if (!forward)
            {
                if (ctl != basectl)
                {
                    int tabIndex = ctl.TabIndex;
                    bool flag2 = false;
                    Control control4 = null;
                    Control parent = ctl.Parent;
                    int count = 0;
                    Control.ControlCollection controls3 = parent.Controls;
                    if (controls3 != null)
                    {
                        count = controls3.Count;
                    }
                    for (int i = count - 1; i >= 0; i--)
                    {
                        if (controls3[i] != ctl)
                        {
                            if (((controls3[i].TabIndex <= tabIndex) && ((control4 == null) || (control4.TabIndex < controls3[i].TabIndex))) && ((controls3[i].TabIndex != tabIndex) || flag2))
                            {
                                control4 = controls3[i];
                            }
                        }
                        else
                        {
                            flag2 = true;
                        }
                    }
                    if (control4 == null)
                    {
                        if (parent == basectl)
                        {
                            return null;
                        }
                        return parent;
                    }
                    ctl = control4;
                }
                for (Control.ControlCollection controls4 = ctl.Controls; (controls4 != null) && (controls4.Count > 0); controls4 = ctl.Controls)
                {
                    Control control6 = null;
                    for (int j = controls4.Count - 1; j >= 0; j--)
                    {
                        if ((control6 == null) || (control6.TabIndex < controls4[j].TabIndex))
                        {
                            control6 = controls4[j];
                        }
                    }
                    ctl = control6;
                }
            }
            else
            {
                Control.ControlCollection controls = ctl.Controls;
                if ((controls == null) || (controls.Count <= 0))
                {
                    while (ctl != basectl)
                    {
                        int num2 = ctl.TabIndex;
                        bool flag = false;
                        Control control2 = null;
                        Control control3 = ctl.Parent;
                        int num3 = 0;
                        Control.ControlCollection controls2 = control3.Controls;
                        if (controls2 != null)
                        {
                            num3 = controls2.Count;
                        }
                        for (int k = 0; k < num3; k++)
                        {
                            if (controls2[k] != ctl)
                            {
                                if (((controls2[k].TabIndex >= num2) && ((control2 == null) || (control2.TabIndex > controls2[k].TabIndex))) && ((controls2[k].TabIndex != num2) || flag))
                                {
                                    control2 = controls2[k];
                                }
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                        if (control2 != null)
                        {
                            return control2;
                        }
                        ctl = ctl.Parent;
                    }
                }
                else
                {
                    Control control = null;
                    for (int m = 0; m < controls.Count; m++)
                    {
                        if ((control == null) || (control.TabIndex > controls[m].TabIndex))
                        {
                            control = controls[m];
                        }
                    }
                    return control;
                }
            }
            if (ctl != basectl)
            {
                return ctl;
            }
            return null;
        }

        protected override void GetSnapInformation(IDesignerHost host, IComponent component, out Size snapSize, out IComponent snapComponent, out PropertyDescriptor snapProperty)
        {
            IComponent rootComponent = null;
            IContainer container = component.Site.Container;
            PropertyDescriptor descriptor = null;
            PropertyDescriptor descriptor2 = null;
            Control control = component as Control;
            if (control != null)
            {
                for (Control control2 = control.Parent; (control2 != null) && (rootComponent == null); control2 = control2.Parent)
                {
                    descriptor2 = TypeDescriptor.GetProperties(control2)["SnapToGrid"];
                    if (descriptor2 != null)
                    {
                        if (((descriptor2.PropertyType == typeof(bool)) && (control2.Site != null)) && (control2.Site.Container == container))
                        {
                            rootComponent = control2;
                        }
                        else
                        {
                            descriptor2 = null;
                        }
                    }
                }
            }
            if (rootComponent == null)
            {
                rootComponent = host.RootComponent;
            }
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(rootComponent);
            if (descriptor2 == null)
            {
                descriptor2 = properties["SnapToGrid"];
                if ((descriptor2 != null) && (descriptor2.PropertyType != typeof(bool)))
                {
                    descriptor2 = null;
                }
            }
            if (descriptor == null)
            {
                descriptor = properties["GridSize"];
                if ((descriptor != null) && (descriptor.PropertyType != typeof(Size)))
                {
                    descriptor = null;
                }
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

        protected override bool OnKeyCancel(object sender)
        {
            if (!base.OnKeyCancel(sender))
            {
                MenuCommand command = (MenuCommand) sender;
                bool backwards = command.CommandID.Equals(MenuCommands.KeyReverseCancel);
                this.RotateParentSelection(backwards);
                return true;
            }
            return false;
        }

        protected override void OnKeyMove(object sender, EventArgs e)
        {
            base.OnKeyMove(sender, e);
        }

        protected void OnKeySelect(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            bool backwards = command.CommandID.Equals(MenuCommands.KeySelectPrevious);
            this.RotateTabSelection(backwards);
        }

        protected void OnKeySize(object sender, EventArgs e)
        {
            ISelectionService selectionService = base.SelectionService;
            if (selectionService != null)
            {
                IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                if (primarySelection != null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        ControlDesigner designer = service.GetDesigner(primarySelection) as ControlDesigner;
                        if ((designer != null) && ((designer.SelectionRules & (SelectionRules.None | SelectionRules.Locked)) == SelectionRules.None))
                        {
                            bool flag = false;
                            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(primarySelection)["Dock"];
                            if (descriptor != null)
                            {
                                DockStyle style = (DockStyle) descriptor.GetValue(primarySelection);
                                flag = (style == DockStyle.Bottom) || (style == DockStyle.Right);
                            }
                            SelectionRules visible = SelectionRules.Visible;
                            CommandID commandID = ((MenuCommand) sender).CommandID;
                            bool flag2 = false;
                            int directionOffsetX = 0;
                            int directionOffsetY = 0;
                            bool flag3 = false;
                            if (commandID.Equals(MenuCommands.KeySizeHeightDecrease))
                            {
                                directionOffsetY = flag ? 1 : -1;
                                visible |= SelectionRules.BottomSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeySizeHeightIncrease))
                            {
                                directionOffsetY = flag ? -1 : 1;
                                visible |= SelectionRules.BottomSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeySizeWidthDecrease))
                            {
                                directionOffsetX = flag ? 1 : -1;
                                visible |= SelectionRules.RightSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeySizeWidthIncrease))
                            {
                                directionOffsetX = flag ? -1 : 1;
                                visible |= SelectionRules.RightSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeHeightDecrease))
                            {
                                directionOffsetY = -1;
                                flag2 = true;
                                visible |= SelectionRules.BottomSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeHeightIncrease))
                            {
                                directionOffsetY = 1;
                                flag2 = true;
                                visible |= SelectionRules.BottomSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeWidthDecrease))
                            {
                                directionOffsetX = -1;
                                flag2 = true;
                                visible |= SelectionRules.RightSizeable;
                            }
                            else if (commandID.Equals(MenuCommands.KeyNudgeWidthIncrease))
                            {
                                directionOffsetX = 1;
                                flag2 = true;
                                visible |= SelectionRules.RightSizeable;
                            }
                            DesignerTransaction transaction = null;
                            if (selectionService.SelectionCount > 1)
                            {
                                transaction = service.CreateTransaction(System.Design.SR.GetString("DragDropSizeComponents", new object[] { selectionService.SelectionCount }));
                            }
                            else
                            {
                                transaction = service.CreateTransaction(System.Design.SR.GetString("DragDropSizeComponent", new object[] { primarySelection.Site.Name }));
                            }
                            try
                            {
                                if (base.BehaviorService != null)
                                {
                                    Control primaryControl = primarySelection as Control;
                                    bool useSnapLines = base.BehaviorService.UseSnapLines;
                                    if (base.dragManager != null)
                                    {
                                        base.EndDragManager();
                                    }
                                    if (flag2 && useSnapLines)
                                    {
                                        ArrayList dragComponents = new ArrayList(selectionService.GetSelectedComponents());
                                        base.dragManager = new DragAssistanceManager(designer.Component.Site, dragComponents);
                                        ArrayList targetSnaplines = this.GenerateSnapLines(designer.SelectionRules, primaryControl, directionOffsetX, directionOffsetY);
                                        Point point = base.dragManager.OffsetToNearestSnapLocation(primaryControl, targetSnaplines, new Point(directionOffsetX, directionOffsetY));
                                        Size size = primaryControl.Size + new Size(point.X, point.Y);
                                        if ((size.Width <= 0) || (size.Height <= 0))
                                        {
                                            directionOffsetX = 0;
                                            directionOffsetY = 0;
                                            base.EndDragManager();
                                        }
                                        else
                                        {
                                            directionOffsetX = point.X;
                                            directionOffsetY = point.Y;
                                        }
                                        if (primaryControl.Parent.IsMirrored)
                                        {
                                            directionOffsetX *= -1;
                                        }
                                    }
                                    else if (!flag2 && !useSnapLines)
                                    {
                                        bool flag5 = false;
                                        Size empty = Size.Empty;
                                        IComponent snapComponent = null;
                                        PropertyDescriptor snapProperty = null;
                                        this.GetSnapInformation(service, primarySelection, out empty, out snapComponent, out snapProperty);
                                        if (snapProperty != null)
                                        {
                                            flag5 = (bool) snapProperty.GetValue(snapComponent);
                                        }
                                        if (flag5 && !empty.IsEmpty)
                                        {
                                            ParentControlDesigner designer2 = service.GetDesigner(primaryControl.Parent) as ParentControlDesigner;
                                            if (designer2 != null)
                                            {
                                                directionOffsetX *= empty.Width;
                                                directionOffsetY *= empty.Height;
                                                if (primaryControl.Parent.IsMirrored)
                                                {
                                                    directionOffsetX *= -1;
                                                }
                                                Rectangle dragRect = new Rectangle(primaryControl.Location.X, primaryControl.Location.Y, primaryControl.Width + directionOffsetX, primaryControl.Height + directionOffsetY);
                                                Rectangle rectangle2 = designer2.GetSnappedRect(primaryControl.Bounds, dragRect, true);
                                                if (directionOffsetX != 0)
                                                {
                                                    directionOffsetX = rectangle2.Width - primaryControl.Width;
                                                }
                                                if (directionOffsetY != 0)
                                                {
                                                    directionOffsetY = rectangle2.Height - primaryControl.Height;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            flag3 = true;
                                            if (primaryControl.Parent.IsMirrored)
                                            {
                                                directionOffsetX *= -1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        flag3 = true;
                                        if (primaryControl.Parent.IsMirrored)
                                        {
                                            directionOffsetX *= -1;
                                        }
                                    }
                                    foreach (IComponent component3 in selectionService.GetSelectedComponents())
                                    {
                                        designer = service.GetDesigner(component3) as ControlDesigner;
                                        if ((designer == null) || ((designer.SelectionRules & visible) == visible))
                                        {
                                            Control selectedComponent = component3 as Control;
                                            if (selectedComponent != null)
                                            {
                                                int height = directionOffsetY;
                                                if (flag3)
                                                {
                                                    PropertyDescriptor descriptor3 = TypeDescriptor.GetProperties(component3)["IntegralHeight"];
                                                    if (descriptor3 != null)
                                                    {
                                                        object obj2 = descriptor3.GetValue(component3);
                                                        if ((obj2 is bool) && ((bool) obj2))
                                                        {
                                                            PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(component3)["ItemHeight"];
                                                            if (descriptor4 != null)
                                                            {
                                                                height *= (int) descriptor4.GetValue(component3);
                                                            }
                                                        }
                                                    }
                                                }
                                                PropertyDescriptor descriptor5 = TypeDescriptor.GetProperties(component3)["Size"];
                                                if (descriptor5 != null)
                                                {
                                                    Size size3 = (Size) descriptor5.GetValue(component3);
                                                    size3 += new Size(directionOffsetX, height);
                                                    descriptor5.SetValue(component3, size3);
                                                }
                                            }
                                            if ((selectedComponent == selectionService.PrimarySelection) && (this.statusCommandUI != null))
                                            {
                                                this.statusCommandUI.SetStatusInformation(selectedComponent);
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
                                if (base.dragManager != null)
                                {
                                    base.SnapLineTimer.Start();
                                    base.dragManager.RenderSnapLinesInternal();
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void OnMenuLockControls(object sender, EventArgs e)
        {
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    ComponentCollection components = service.Container.Components;
                    if ((components != null) && (components.Count > 0))
                    {
                        DesignerTransaction transaction = null;
                        try
                        {
                            transaction = service.CreateTransaction(System.Design.SR.GetString("CommandSetLockControls", new object[] { components.Count }));
                            MenuCommand command = (MenuCommand) sender;
                            bool flag = !command.Checked;
                            bool flag2 = true;
                            foreach (IComponent component in components)
                            {
                                PropertyDescriptor property = base.GetProperty(component, "Locked");
                                if ((property != null) && !property.IsReadOnly)
                                {
                                    if (flag2 && !base.CanCheckout(component))
                                    {
                                        return;
                                    }
                                    flag2 = false;
                                    property.SetValue(component, flag);
                                }
                            }
                            command.Checked = flag;
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
            finally
            {
                Cursor.Current = current;
            }
        }

        private void OnMenuTabOrder(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            if (command.Checked)
            {
                if (this.tabOrder != null)
                {
                    this.tabOrder.Dispose();
                    this.tabOrder = null;
                }
                command.Checked = false;
            }
            else
            {
                ISelectionService selectionService = base.SelectionService;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((service != null) && (selectionService != null))
                {
                    object rootComponent = service.RootComponent;
                    if (rootComponent != null)
                    {
                        selectionService.SetSelectedComponents(new object[] { rootComponent }, SelectionTypes.Replace);
                    }
                }
                this.tabOrder = new TabOrder((IDesignerHost) this.GetService(typeof(IDesignerHost)));
                command.Checked = true;
            }
        }

        private void OnMenuZOrderSelection(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            CommandID commandID = command.CommandID;
            if (base.SelectionService != null)
            {
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                    IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    DesignerTransaction transaction = null;
                    try
                    {
                        string str;
                        ICollection selectedComponents = base.SelectionService.GetSelectedComponents();
                        object[] array = new object[selectedComponents.Count];
                        selectedComponents.CopyTo(array, 0);
                        if (commandID == StandardCommands.BringToFront)
                        {
                            str = System.Design.SR.GetString("CommandSetBringToFront", new object[] { array.Length });
                        }
                        else
                        {
                            str = System.Design.SR.GetString("CommandSetSendToBack", new object[] { array.Length });
                        }
                        Array.Sort(array, new ControlComparer());
                        transaction = host.CreateTransaction(str);
                        if (array.Length > 0)
                        {
                            int length = array.Length;
                            for (int i = length - 1; i >= 0; i--)
                            {
                                Control owner = array[i] as Control;
                                IComponent component = array[i] as IComponent;
                                if (component != null)
                                {
                                    INestedSite site = component.Site as INestedSite;
                                    if (site != null)
                                    {
                                        INestedContainer container = site.Container as INestedContainer;
                                        if (container != null)
                                        {
                                            owner = container.Owner as Control;
                                            array[i] = owner;
                                        }
                                    }
                                }
                                if (owner != null)
                                {
                                    Control parent = owner.Parent;
                                    PropertyDescriptor member = null;
                                    if (parent != null)
                                    {
                                        if (service != null)
                                        {
                                            try
                                            {
                                                if (!list2.Contains(parent))
                                                {
                                                    member = TypeDescriptor.GetProperties(parent)["Controls"];
                                                    if (member != null)
                                                    {
                                                        list2.Add(parent);
                                                        service.OnComponentChanging(parent, member);
                                                    }
                                                }
                                            }
                                            catch (CheckoutException exception)
                                            {
                                                if (exception != CheckoutException.Canceled)
                                                {
                                                    throw exception;
                                                }
                                                if (transaction != null)
                                                {
                                                    transaction.Cancel();
                                                }
                                                return;
                                            }
                                        }
                                        if (!list.Contains(parent))
                                        {
                                            list.Add(parent);
                                            parent.SuspendLayout();
                                        }
                                    }
                                }
                            }
                            for (int j = length - 1; j >= 0; j--)
                            {
                                if (commandID == StandardCommands.BringToFront)
                                {
                                    Control control3 = array[(length - j) - 1] as Control;
                                    if (control3 != null)
                                    {
                                        control3.BringToFront();
                                    }
                                }
                                else if (commandID == StandardCommands.SendToBack)
                                {
                                    Control control4 = array[j] as Control;
                                    if (control4 != null)
                                    {
                                        control4.SendToBack();
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        if ((transaction != null) && !transaction.Canceled)
                        {
                            foreach (Control control5 in list2)
                            {
                                PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(control5)["Controls"];
                                if ((service != null) && (descriptor2 != null))
                                {
                                    service.OnComponentChanged(control5, descriptor2, null, null);
                                }
                            }
                            foreach (Control control6 in list)
                            {
                                control6.ResumeLayout();
                            }
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

        protected void OnStatusAnyControls(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            bool flag = false;
            if ((this.baseControl != null) && (this.baseControl.Controls.Count > 0))
            {
                flag = true;
            }
            command.Enabled = flag;
        }

        protected void OnStatusControlsOnlySelection(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = (base.selCount > 0) && base.controlsOnlySelection;
        }

        protected void OnStatusControlsOnlySelectionAndGrid(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = ((base.selCount > 0) && base.controlsOnlySelection) && !base.BehaviorService.UseSnapLines;
        }

        protected void OnStatusLockControls(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            if (this.baseControl == null)
            {
                command.Enabled = false;
            }
            else
            {
                command.Enabled = base.controlsOnlySelection;
                command.Checked = false;
                PropertyDescriptor descriptor = null;
                descriptor = TypeDescriptor.GetProperties(this.baseControl)["Locked"];
                if ((descriptor != null) && ((bool) descriptor.GetValue(this.baseControl)))
                {
                    command.Checked = true;
                }
                else
                {
                    IDesignerHost service = (IDesignerHost) base.site.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        ComponentDesigner designer = service.GetDesigner(this.baseControl) as ComponentDesigner;
                        foreach (object obj2 in designer.AssociatedComponents)
                        {
                            descriptor = TypeDescriptor.GetProperties(obj2)["Locked"];
                            if ((descriptor != null) && ((bool) descriptor.GetValue(obj2)))
                            {
                                command.Checked = true;
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected void OnStatusMultiSelect(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = base.controlsOnlySelection && (base.selCount > 1);
        }

        private void OnStatusMultiSelectNonContained(object sender, EventArgs e)
        {
            this.OnStatusMultiSelect(sender, e);
            MenuCommand command = (MenuCommand) sender;
            if (command.Enabled)
            {
                command.Enabled = this.CheckSelectionParenting();
            }
        }

        protected void OnStatusMultiSelectPrimary(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            command.Enabled = (base.controlsOnlySelection && (base.selCount > 1)) && (base.primarySelection != null);
        }

        protected void OnStatusShowGrid(object sender, EventArgs e)
        {
            if (base.site != null)
            {
                IDesignerHost service = (IDesignerHost) base.site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    IComponent rootComponent = service.RootComponent;
                    if ((rootComponent != null) && (rootComponent is Control))
                    {
                        PropertyDescriptor property = base.GetProperty(rootComponent, "DrawGrid");
                        if (property != null)
                        {
                            bool flag = (bool) property.GetValue(rootComponent);
                            MenuCommand command = (MenuCommand) sender;
                            command.Enabled = true;
                            command.Checked = flag;
                        }
                    }
                }
            }
        }

        protected void OnStatusSnapToGrid(object sender, EventArgs e)
        {
            if (base.site != null)
            {
                IDesignerHost service = (IDesignerHost) base.site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    IComponent rootComponent = service.RootComponent;
                    if ((rootComponent != null) && (rootComponent is Control))
                    {
                        PropertyDescriptor property = base.GetProperty(rootComponent, "SnapToGrid");
                        if (property != null)
                        {
                            bool flag = (bool) property.GetValue(rootComponent);
                            MenuCommand command = (MenuCommand) sender;
                            command.Enabled = base.controlsOnlySelection;
                            command.Checked = flag;
                        }
                    }
                }
            }
        }

        private void OnStatusZOrder(object sender, EventArgs e)
        {
            MenuCommand command = (MenuCommand) sender;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                command.Enabled = false;
            }
            else
            {
                ComponentCollection components = service.Container.Components;
                object rootComponent = service.RootComponent;
                bool flag = ((components != null) && (components.Count > 2)) && base.controlsOnlySelection;
                if (flag)
                {
                    if (base.SelectionService == null)
                    {
                        return;
                    }
                    ICollection selectedComponents = base.SelectionService.GetSelectedComponents();
                    flag = false;
                    foreach (object obj3 in selectedComponents)
                    {
                        if ((obj3 is Control) && !TypeDescriptor.GetAttributes(obj3)[typeof(InheritanceAttribute)].Equals(InheritanceAttribute.InheritedReadOnly))
                        {
                            flag = true;
                        }
                        if (obj3 == rootComponent)
                        {
                            flag = false;
                            break;
                        }
                    }
                }
                command.Enabled = flag;
            }
        }

        protected override void OnUpdateCommandStatus()
        {
            for (int i = 0; i < this.commandSet.Length; i++)
            {
                this.commandSet[i].UpdateStatus();
            }
            base.OnUpdateCommandStatus();
        }

        private void RotateParentSelection(bool backwards)
        {
            Control rootComponent = null;
            object parent = null;
            ISelectionService selectionService = base.SelectionService;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (((selectionService != null) && (service != null)) && (service.RootComponent is Control))
            {
                IContainer container = service.Container;
                Control primarySelection = selectionService.PrimarySelection as Control;
                if (primarySelection != null)
                {
                    rootComponent = primarySelection;
                }
                else
                {
                    rootComponent = (Control) service.RootComponent;
                }
                if (backwards)
                {
                    if (rootComponent != null)
                    {
                        if (rootComponent.Controls.Count > 0)
                        {
                            parent = rootComponent.Controls[0];
                        }
                        else
                        {
                            parent = rootComponent;
                        }
                    }
                }
                else if (rootComponent != null)
                {
                    parent = rootComponent.Parent;
                    Control control3 = parent as Control;
                    IContainer container2 = null;
                    if ((control3 != null) && (control3.Site != null))
                    {
                        container2 = DesignerUtils.CheckForNestedContainer(control3.Site.Container);
                    }
                    if (((control3 == null) || (control3.Site == null)) || (container2 != container))
                    {
                        parent = rootComponent;
                    }
                }
                selectionService.SetSelectedComponents(new object[] { parent }, SelectionTypes.Replace);
            }
        }

        private void RotateTabSelection(bool backwards)
        {
            object nextComponent = null;
            ISelectionService selectionService = base.SelectionService;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (((selectionService != null) && (service != null)) && (service.RootComponent is Control))
            {
                Control rootComponent = (Control) service.RootComponent;
                object primarySelection = selectionService.PrimarySelection;
                Control ctl = primarySelection as Control;
                if (((nextComponent == null) && (ctl != null)) && (rootComponent.Contains(ctl) || (rootComponent == primarySelection)))
                {
                    while ((ctl = this.GetNextControlInTab(rootComponent, ctl, !backwards)) != null)
                    {
                        if ((ctl.Site != null) && (ctl.Site.Container == ctl.Container))
                        {
                            break;
                        }
                    }
                    nextComponent = ctl;
                }
                if (nextComponent == null)
                {
                    ComponentTray tray = (ComponentTray) this.GetService(typeof(ComponentTray));
                    if (tray != null)
                    {
                        nextComponent = tray.GetNextComponent((IComponent) primarySelection, !backwards);
                        if (nextComponent != null)
                        {
                            IComponent component = nextComponent as IComponent;
                            ControlDesigner designer = service.GetDesigner(component) as ControlDesigner;
                            while (designer != null)
                            {
                                component = tray.GetNextComponent(component, !backwards);
                                if (component != null)
                                {
                                    designer = service.GetDesigner(component) as ControlDesigner;
                                }
                                else
                                {
                                    designer = null;
                                }
                            }
                        }
                    }
                    if (nextComponent == null)
                    {
                        nextComponent = rootComponent;
                    }
                }
                selectionService.SetSelectedComponents(new object[] { nextComponent }, SelectionTypes.Replace);
            }
        }

        private class ControlComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (x != y)
                {
                    Control child = x as Control;
                    Control control2 = y as Control;
                    if ((child != null) && (control2 != null))
                    {
                        if (child.Parent == control2.Parent)
                        {
                            Control parent = child.Parent;
                            if (parent == null)
                            {
                                return 0;
                            }
                            if (parent.Controls.GetChildIndex(child) > parent.Controls.GetChildIndex(control2))
                            {
                                return -1;
                            }
                            return 1;
                        }
                        if ((child.Parent == null) || child.Contains(control2))
                        {
                            return 1;
                        }
                        if ((control2.Parent != null) && !control2.Contains(child))
                        {
                            return (((int) child.Parent.Handle) - ((int) control2.Parent.Handle));
                        }
                        return -1;
                    }
                    if (control2 != null)
                    {
                        return -1;
                    }
                    if (child != null)
                    {
                        return 1;
                    }
                }
                return 0;
            }
        }
    }
}

