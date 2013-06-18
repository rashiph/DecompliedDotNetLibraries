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

    internal class ToolStripKeyboardHandlingService
    {
        private ToolStripTemplateNode activeTemplateNode;
        private bool commandsAdded;
        private IComponentChangeService componentChangeSvc;
        private bool contextMenuShownByKeyBoard;
        private bool copyInProgress;
        private object currentSelection;
        private bool cutOrDeleteInProgress;
        private IDesignerHost designerHost;
        private const int GLYPHBORDER = 1;
        private const int GLYPHINSET = 2;
        private IMenuCommandService menuCommandService;
        private MenuCommand newCommandPaste;
        private ArrayList newCommands;
        private MenuCommand oldCommandPaste;
        private ArrayList oldCommands;
        private object ownerItemAfterCut;
        private IServiceProvider provider;
        private ISelectionService selectionService;
        private bool shiftPressed;
        private object shiftPrimary;
        private bool templateNodeActive;
        private bool templateNodeContextMenuOpen;

        public ToolStripKeyboardHandlingService(IServiceProvider serviceProvider)
        {
            this.provider = serviceProvider;
            this.selectionService = (ISelectionService) serviceProvider.GetService(typeof(ISelectionService));
            if (this.selectionService != null)
            {
                this.selectionService.SelectionChanging += new EventHandler(this.OnSelectionChanging);
                this.selectionService.SelectionChanged += new EventHandler(this.OnSelectionChanged);
            }
            this.designerHost = (IDesignerHost) this.provider.GetService(typeof(IDesignerHost));
            if (this.designerHost != null)
            {
                this.designerHost.AddService(typeof(ToolStripKeyboardHandlingService), this);
            }
            this.componentChangeSvc = (IComponentChangeService) this.designerHost.GetService(typeof(IComponentChangeService));
            if (this.componentChangeSvc != null)
            {
                this.componentChangeSvc.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
            }
        }

        public void AddCommands()
        {
            IMenuCommandService menuService = this.MenuService;
            if ((menuService != null) & !this.commandsAdded)
            {
                if (this.oldCommands == null)
                {
                    this.PopulateOldCommands();
                }
                foreach (MenuCommand command in this.oldCommands)
                {
                    if (command != null)
                    {
                        menuService.RemoveCommand(command);
                    }
                }
                if (this.newCommands == null)
                {
                    this.PopulateNewCommands();
                }
                foreach (MenuCommand command2 in this.newCommands)
                {
                    if ((command2 != null) && (menuService.FindCommand(command2.CommandID) == null))
                    {
                        menuService.AddCommand(command2);
                    }
                }
                this.commandsAdded = true;
            }
        }

        private Control GetNextControlInTab(Control basectl, Control ctl, bool forward)
        {
            if (forward)
            {
                while (ctl != basectl)
                {
                    int tabIndex = ctl.TabIndex;
                    bool flag = false;
                    Control control = null;
                    Control parent = ctl.Parent;
                    int count = 0;
                    Control.ControlCollection controls = parent.Controls;
                    if (controls != null)
                    {
                        count = controls.Count;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        if (controls[i] != ctl)
                        {
                            if (((controls[i].TabIndex >= tabIndex) && ((control == null) || (control.TabIndex > controls[i].TabIndex))) && (((controls[i].Site != null) && (controls[i].TabIndex != tabIndex)) || flag))
                            {
                                control = controls[i];
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if (control != null)
                    {
                        return control;
                    }
                    ctl = ctl.Parent;
                }
            }
            else if (ctl != basectl)
            {
                int num4 = ctl.TabIndex;
                bool flag2 = false;
                Control control3 = null;
                Control control4 = ctl.Parent;
                int num5 = 0;
                Control.ControlCollection controls2 = control4.Controls;
                if (controls2 != null)
                {
                    num5 = controls2.Count;
                }
                for (int j = num5 - 1; j >= 0; j--)
                {
                    if (controls2[j] != ctl)
                    {
                        if (((controls2[j].TabIndex <= num4) && ((control3 == null) || (control3.TabIndex < controls2[j].TabIndex))) && ((controls2[j].TabIndex != num4) || flag2))
                        {
                            control3 = controls2[j];
                        }
                    }
                    else
                    {
                        flag2 = true;
                    }
                }
                if (control3 == null)
                {
                    if (control4 == basectl)
                    {
                        return null;
                    }
                    return control4;
                }
                ctl = control3;
            }
            if (ctl != basectl)
            {
                return ctl;
            }
            return null;
        }

        private ToolStripItem GetNextItem(ToolStrip parent, ToolStripItem startItem, ArrowDirection direction)
        {
            if ((parent.RightToLeft == RightToLeft.Yes) && ((direction == ArrowDirection.Left) || (direction == ArrowDirection.Right)))
            {
                if (direction == ArrowDirection.Right)
                {
                    direction = ArrowDirection.Left;
                }
                else if (direction == ArrowDirection.Left)
                {
                    direction = ArrowDirection.Right;
                }
            }
            return parent.GetNextItem(startItem, direction);
        }

        private void InvokeOldCommand(object sender)
        {
            MenuCommand command = sender as MenuCommand;
            foreach (MenuCommand command2 in this.oldCommands)
            {
                if ((command2 != null) && (command2.CommandID == command.CommandID))
                {
                    command2.Invoke();
                    break;
                }
            }
        }

        private void OnCommandCopy(object sender, EventArgs e)
        {
            bool flag = false;
            try
            {
                MenuCommand command = sender as MenuCommand;
                if ((command != null) && (command.CommandID == StandardCommands.Cut))
                {
                    flag = true;
                    this.CutOrDeleteInProgress = true;
                }
                this.InvokeOldCommand(sender);
                if (flag)
                {
                    ToolStripDropDownItem ownerItemAfterCut = this.OwnerItemAfterCut as ToolStripDropDownItem;
                    if (ownerItemAfterCut != null)
                    {
                        ToolStripDropDown dropDown = ownerItemAfterCut.DropDown;
                        ToolStripDropDownDesigner designer = this.Host.GetDesigner(dropDown) as ToolStripDropDownDesigner;
                        if (designer != null)
                        {
                            this.SelectionService.SetSelectedComponents(new object[] { designer.Component }, SelectionTypes.Replace);
                        }
                        else if ((ownerItemAfterCut != null) && !ownerItemAfterCut.DropDown.Visible)
                        {
                            ToolStripMenuItemDesigner designer2 = this.Host.GetDesigner(ownerItemAfterCut) as ToolStripMenuItemDesigner;
                            if (designer2 != null)
                            {
                                designer2.SetSelection(true);
                                DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                                if (selectedDesignerControl != null)
                                {
                                    selectedDesignerControl.SelectControl();
                                }
                            }
                        }
                    }
                }
                IMenuCommandService menuService = this.MenuService;
                if ((menuService != null) && (this.newCommandPaste == null))
                {
                    this.oldCommandPaste = menuService.FindCommand(StandardCommands.Paste);
                    if (this.oldCommandPaste != null)
                    {
                        menuService.RemoveCommand(this.oldCommandPaste);
                    }
                    this.newCommandPaste = new MenuCommand(new EventHandler(this.OnCommandPaste), StandardCommands.Paste);
                    if ((this.newCommandPaste != null) && (menuService.FindCommand(this.newCommandPaste.CommandID) == null))
                    {
                        menuService.AddCommand(this.newCommandPaste);
                    }
                }
            }
            finally
            {
                flag = false;
                this.CutOrDeleteInProgress = false;
            }
        }

        private void OnCommandDelete(object sender, EventArgs e)
        {
            try
            {
                this.CutOrDeleteInProgress = true;
                this.InvokeOldCommand(sender);
            }
            finally
            {
                this.CutOrDeleteInProgress = false;
            }
        }

        private void OnCommandEnd(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl as ToolStripItem;
                }
                if (primarySelection != null)
                {
                    ToolStrip currentParent = primarySelection.GetCurrentParent();
                    int count = currentParent.Items.Count;
                    if (count >= 3)
                    {
                        if ((Control.ModifierKeys & Keys.Shift) > Keys.None)
                        {
                            int index = currentParent.Items.IndexOf(primarySelection);
                            int num3 = Math.Max(index, count - 2);
                            int num4 = (num3 - index) + 1;
                            object[] components = new object[num4];
                            int num5 = 0;
                            for (int i = index; i <= num3; i++)
                            {
                                components[num5++] = currentParent.Items[i];
                            }
                            selectionService.SetSelectedComponents(components, SelectionTypes.Replace);
                        }
                        else
                        {
                            this.SetSelection(currentParent.Items[count - 2]);
                        }
                    }
                }
            }
        }

        private void OnCommandHome(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl as ToolStripItem;
                }
                if (primarySelection != null)
                {
                    ToolStrip currentParent = primarySelection.GetCurrentParent();
                    if (currentParent.Items.Count >= 3)
                    {
                        if ((Control.ModifierKeys & Keys.Shift) > Keys.None)
                        {
                            int num2 = 0;
                            int num3 = Math.Max(0, currentParent.Items.IndexOf(primarySelection));
                            int num4 = (num3 - num2) + 1;
                            object[] components = new object[num4];
                            int num5 = 0;
                            for (int i = num2; i <= num3; i++)
                            {
                                components[num5++] = currentParent.Items[i];
                            }
                            selectionService.SetSelectedComponents(components, SelectionTypes.Replace);
                        }
                        else
                        {
                            this.SetSelection(currentParent.Items[0]);
                        }
                    }
                }
            }
        }

        private void OnCommandPaste(object sender, EventArgs e)
        {
            if (!this.TemplateNodeActive)
            {
                ISelectionService selectionService = this.SelectionService;
                IDesignerHost host = this.Host;
                if ((selectionService != null) && (host != null))
                {
                    IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                    if (primarySelection == null)
                    {
                        primarySelection = (IComponent) this.SelectedDesignerControl;
                    }
                    ToolStripItem component = primarySelection as ToolStripItem;
                    ToolStrip currentParent = null;
                    if (component != null)
                    {
                        currentParent = component.GetCurrentParent();
                    }
                    if (currentParent != null)
                    {
                        currentParent.SuspendLayout();
                    }
                    if (this.oldCommandPaste != null)
                    {
                        this.oldCommandPaste.Invoke();
                    }
                    if (currentParent != null)
                    {
                        currentParent.ResumeLayout();
                        BehaviorService service = (BehaviorService) this.provider.GetService(typeof(BehaviorService));
                        if (service != null)
                        {
                            service.SyncSelection();
                        }
                        ToolStripItemDesigner designer = host.GetDesigner(component) as ToolStripItemDesigner;
                        if (designer != null)
                        {
                            ToolStripDropDown firstDropDown = designer.GetFirstDropDown(component);
                            if ((firstDropDown != null) && !firstDropDown.IsAutoGenerated)
                            {
                                ToolStripDropDownDesigner designer2 = host.GetDesigner(firstDropDown) as ToolStripDropDownDesigner;
                                if (designer2 != null)
                                {
                                    designer2.AddSelectionGlyphs();
                                }
                            }
                        }
                        ToolStripDropDown down2 = currentParent as ToolStripDropDown;
                        if ((down2 != null) && down2.Visible)
                        {
                            ToolStripDropDownItem ownerItem = down2.OwnerItem as ToolStripDropDownItem;
                            if (ownerItem != null)
                            {
                                ToolStripMenuItemDesigner designer3 = host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner;
                                if (designer3 != null)
                                {
                                    designer3.ResetGlyphs(ownerItem);
                                }
                            }
                        }
                        ToolStripDropDownItem item3 = selectionService.PrimarySelection as ToolStripDropDownItem;
                        if ((item3 != null) && item3.DropDown.Visible)
                        {
                            item3.HideDropDown();
                            ToolStripMenuItemDesigner designer4 = host.GetDesigner(item3) as ToolStripMenuItemDesigner;
                            if (designer4 != null)
                            {
                                designer4.InitializeDropDown();
                                designer4.InitializeBodyGlyphsForItems(false, item3);
                                designer4.InitializeBodyGlyphsForItems(true, item3);
                            }
                        }
                    }
                }
            }
        }

        private void OnCommandSelectAll(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                object primarySelection = selectionService.PrimarySelection;
                if (primarySelection is ToolStripItem)
                {
                    ToolStripItem item = primarySelection as ToolStripItem;
                    ToolStrip currentParent = item.GetCurrentParent();
                    if (currentParent is ToolStripOverflow)
                    {
                        currentParent = item.Owner;
                    }
                    this.SelectItems(currentParent);
                    BehaviorService service = (BehaviorService) this.provider.GetService(typeof(BehaviorService));
                    if (service != null)
                    {
                        service.Invalidate();
                    }
                }
                else if (primarySelection is ToolStrip)
                {
                    ToolStrip parent = primarySelection as ToolStrip;
                    this.SelectItems(parent);
                }
                else if (primarySelection is ToolStripPanel)
                {
                    ToolStripPanel panel = primarySelection as ToolStripPanel;
                    selectionService.SetSelectedComponents(panel.Controls, SelectionTypes.Replace);
                }
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            bool flag = false;
            foreach (IComponent component in this.designerHost.Container.Components)
            {
                if (component is ToolStrip)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                ToolStripKeyboardHandlingService service = (ToolStripKeyboardHandlingService) this.provider.GetService(typeof(ToolStripKeyboardHandlingService));
                if (service != null)
                {
                    service.RestoreCommands();
                    service.RemoveCommands();
                    this.designerHost.RemoveService(typeof(ToolStripKeyboardHandlingService));
                }
            }
        }

        public bool OnContextMenu(int x, int y)
        {
            if (this.TemplateNodeActive)
            {
                return true;
            }
            if ((this.commandsAdded && (x == -1)) && (y == -1))
            {
                this.ContextMenuShownByKeyBoard = true;
                Point position = Cursor.Position;
                x = position.X;
                y = position.Y;
            }
            if (!(this.SelectionService.PrimarySelection is Component))
            {
                DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                if (selectedDesignerControl != null)
                {
                    ToolStripTemplateNode.TransparentToolStrip control = selectedDesignerControl.Control as ToolStripTemplateNode.TransparentToolStrip;
                    if (control != null)
                    {
                        ToolStripTemplateNode templateNode = control.TemplateNode;
                        if (templateNode != null)
                        {
                            templateNode.ShowContextMenu(new Point(x, y));
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void OnKeyCancel(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl as ToolStripItem;
                }
                if (primarySelection != null)
                {
                    MenuCommand command = (MenuCommand) sender;
                    bool backwards = command.CommandID.Equals(MenuCommands.KeyReverseCancel);
                    this.RotateParent(backwards);
                }
                else
                {
                    ToolStripDropDown down = selectionService.PrimarySelection as ToolStripDropDown;
                    if ((down != null) && (down.Site != null))
                    {
                        selectionService.SetSelectedComponents(new object[] { this.Host.RootComponent }, SelectionTypes.Replace);
                    }
                    else
                    {
                        this.InvokeOldCommand(sender);
                    }
                }
            }
        }

        private void OnKeyDefault(object sender, EventArgs e)
        {
            if (this.templateNodeContextMenuOpen)
            {
                this.templateNodeContextMenuOpen = false;
            }
            else
            {
                ISelectionService selectionService = this.SelectionService;
                IDesignerHost host = this.Host;
                if (selectionService != null)
                {
                    IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                    if (primarySelection == null)
                    {
                        DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                        if ((selectedDesignerControl != null) && (host != null))
                        {
                            if (!selectedDesignerControl.IsOnDropDown || selectedDesignerControl.IsOnOverflow)
                            {
                                ToolStripDesigner designer2 = host.GetDesigner(selectedDesignerControl.Owner) as ToolStripDesigner;
                                if (designer2 != null)
                                {
                                    designer2.ShowEditNode(true);
                                    if (this.ActiveTemplateNode != null)
                                    {
                                        this.ActiveTemplateNode.ignoreFirstKeyUp = true;
                                    }
                                }
                            }
                            else
                            {
                                ToolStripDropDownItem ownerItem = (ToolStripDropDownItem) ((ToolStripDropDown) selectedDesignerControl.Owner).OwnerItem;
                                ToolStripMenuItemDesigner designer = host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner;
                                if ((designer != null) && !designer.IsEditorActive)
                                {
                                    designer.EditTemplateNode(true);
                                    if (this.ActiveTemplateNode != null)
                                    {
                                        this.ActiveTemplateNode.ignoreFirstKeyUp = true;
                                    }
                                }
                            }
                        }
                    }
                    else if (host != null)
                    {
                        IDesigner designer3 = host.GetDesigner(primarySelection);
                        ToolStripMenuItemDesigner designer4 = designer3 as ToolStripMenuItemDesigner;
                        if (designer4 != null)
                        {
                            if (!designer4.IsEditorActive)
                            {
                                designer4.ShowEditNode(false);
                                if (this.ActiveTemplateNode != null)
                                {
                                    this.ActiveTemplateNode.ignoreFirstKeyUp = true;
                                }
                            }
                        }
                        else if (designer3 != null)
                        {
                            this.InvokeOldCommand(sender);
                        }
                    }
                }
            }
        }

        private void OnKeyEdit(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            IDesignerHost host = this.Host;
            if (selectionService != null)
            {
                IComponent primarySelection = selectionService.PrimarySelection as IComponent;
                if (primarySelection == null)
                {
                    primarySelection = (IComponent) this.SelectedDesignerControl;
                }
                if (((primarySelection is ToolStripItem) && (host != null)) && ((MenuCommand) sender).CommandID.Equals(MenuCommands.EditLabel))
                {
                    if (primarySelection is ToolStripMenuItem)
                    {
                        ToolStripMenuItemDesigner designer = host.GetDesigner(primarySelection) as ToolStripMenuItemDesigner;
                        if ((designer != null) && !designer.IsEditorActive)
                        {
                            designer.ShowEditNode(false);
                        }
                    }
                    if (primarySelection is DesignerToolStripControlHost)
                    {
                        DesignerToolStripControlHost host2 = primarySelection as DesignerToolStripControlHost;
                        if (host2.IsOnDropDown)
                        {
                            ToolStripDropDownItem ownerItem = (ToolStripDropDownItem) ((ToolStripDropDown) host2.Owner).OwnerItem;
                            ToolStripMenuItemDesigner designer2 = host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner;
                            if ((designer2 != null) && !designer2.IsEditorActive)
                            {
                                designer2.EditTemplateNode(false);
                            }
                        }
                        else
                        {
                            ToolStripDesigner designer3 = host.GetDesigner(host2.Owner) as ToolStripDesigner;
                            if (designer3 != null)
                            {
                                designer3.ShowEditNode(false);
                            }
                        }
                    }
                }
            }
        }

        private void OnKeyMove(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                MenuCommand command = (MenuCommand) sender;
                if ((command.CommandID.Equals(MenuCommands.KeySizeWidthIncrease) || command.CommandID.Equals(MenuCommands.KeySizeWidthDecrease)) || (command.CommandID.Equals(MenuCommands.KeySizeHeightDecrease) || command.CommandID.Equals(MenuCommands.KeySizeHeightIncrease)))
                {
                    this.shiftPressed = true;
                }
                else
                {
                    this.shiftPressed = false;
                }
                if (selectionService.PrimarySelection is ContextMenuStrip)
                {
                    if (command.CommandID.Equals(MenuCommands.KeyMoveDown))
                    {
                        this.ProcessUpDown(true);
                    }
                }
                else
                {
                    ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                    if (primarySelection == null)
                    {
                        primarySelection = this.SelectedDesignerControl as ToolStripItem;
                    }
                    if (primarySelection != null)
                    {
                        if (((command.CommandID.Equals(MenuCommands.KeyMoveRight) || command.CommandID.Equals(MenuCommands.KeyNudgeRight)) || command.CommandID.Equals(MenuCommands.KeySizeWidthIncrease)) && !this.ProcessRightLeft(true))
                        {
                            this.RotateTab(false);
                        }
                        else if (((command.CommandID.Equals(MenuCommands.KeyMoveLeft) || command.CommandID.Equals(MenuCommands.KeyNudgeLeft)) || command.CommandID.Equals(MenuCommands.KeySizeWidthDecrease)) && !this.ProcessRightLeft(false))
                        {
                            this.RotateTab(true);
                        }
                        else if ((command.CommandID.Equals(MenuCommands.KeyMoveDown) || command.CommandID.Equals(MenuCommands.KeyNudgeDown)) || command.CommandID.Equals(MenuCommands.KeySizeHeightIncrease))
                        {
                            this.ProcessUpDown(true);
                        }
                        else if ((command.CommandID.Equals(MenuCommands.KeyMoveUp) || command.CommandID.Equals(MenuCommands.KeyNudgeUp)) || command.CommandID.Equals(MenuCommands.KeySizeHeightDecrease))
                        {
                            this.ProcessUpDown(false);
                        }
                    }
                    else
                    {
                        this.InvokeOldCommand(sender);
                    }
                }
            }
        }

        private void OnKeySelect(object sender, EventArgs e)
        {
            MenuCommand cmd = (MenuCommand) sender;
            bool reverse = cmd.CommandID.Equals(MenuCommands.KeySelectPrevious);
            this.ProcessKeySelect(reverse, cmd);
        }

        private void OnKeyShowDesignerActions(object sender, EventArgs e)
        {
            ISelectionService selectionService = this.SelectionService;
            if ((selectionService != null) && (selectionService.PrimarySelection == null))
            {
                DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                if (selectedDesignerControl != null)
                {
                    ToolStripTemplateNode.TransparentToolStrip control = selectedDesignerControl.Control as ToolStripTemplateNode.TransparentToolStrip;
                    if (control != null)
                    {
                        ToolStripTemplateNode templateNode = control.TemplateNode;
                        if (templateNode != null)
                        {
                            templateNode.ShowDropDownMenu();
                            return;
                        }
                    }
                }
            }
            this.InvokeOldCommand(sender);
        }

        private void OnSelectionChanged(object sender, EventArgs e)
        {
            Component primarySelection = this.SelectionService.PrimarySelection as Component;
            if (primarySelection == null)
            {
                primarySelection = this.SelectedDesignerControl as ToolStripItem;
            }
            ToolStrip component = primarySelection as ToolStrip;
            if (component != null)
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                if ((attribute != null) && ((attribute.InheritanceLevel == InheritanceLevel.Inherited) || (attribute.InheritanceLevel == InheritanceLevel.InheritedReadOnly)))
                {
                    return;
                }
            }
            if ((component != null) || (primarySelection is ToolStripItem))
            {
                BehaviorService service = (BehaviorService) this.provider.GetService(typeof(BehaviorService));
                if (service != null)
                {
                    DesignerActionUI designerActionUI = service.DesignerActionUI;
                    if (designerActionUI != null)
                    {
                        designerActionUI.HideDesignerActionPanel();
                    }
                }
                this.AddCommands();
            }
        }

        private void OnSelectionChanging(object sender, EventArgs e)
        {
            Component primarySelection = this.SelectionService.PrimarySelection as Component;
            if (primarySelection == null)
            {
                primarySelection = this.SelectedDesignerControl as ToolStripItem;
            }
            ToolStrip component = primarySelection as ToolStrip;
            if (component != null)
            {
                InheritanceAttribute attribute = (InheritanceAttribute) TypeDescriptor.GetAttributes(component)[typeof(InheritanceAttribute)];
                if ((attribute != null) && ((attribute.InheritanceLevel == InheritanceLevel.Inherited) || (attribute.InheritanceLevel == InheritanceLevel.InheritedReadOnly)))
                {
                    return;
                }
            }
            if ((component == null) && !(primarySelection is ToolStripItem))
            {
                this.RestoreCommands();
                this.SelectedDesignerControl = null;
            }
        }

        private void PopulateNewCommands()
        {
            if (this.newCommands == null)
            {
                this.newCommands = new ArrayList();
            }
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeySelect), MenuCommands.KeySelectNext));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeySelect), MenuCommands.KeySelectPrevious));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyDefault), MenuCommands.KeyDefaultAction));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyEdit), MenuCommands.EditLabel));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveUp));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveDown));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveLeft));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyMoveRight));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeUp));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeDown));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeLeft));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeyNudgeRight));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeySizeWidthIncrease));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeySizeHeightIncrease));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeySizeWidthDecrease));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyMove), MenuCommands.KeySizeHeightDecrease));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyCancel), MenuCommands.KeyCancel));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyCancel), MenuCommands.KeyReverseCancel));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandCopy), StandardCommands.Copy));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandSelectAll), StandardCommands.SelectAll));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandHome), MenuCommands.KeyHome));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandEnd), MenuCommands.KeyEnd));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandHome), MenuCommands.KeyShiftHome));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandEnd), MenuCommands.KeyShiftEnd));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnKeyShowDesignerActions), MenuCommands.KeyInvokeSmartTag));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandCopy), StandardCommands.Cut));
            this.newCommands.Add(new MenuCommand(new EventHandler(this.OnCommandDelete), StandardCommands.Delete));
        }

        private void PopulateOldCommands()
        {
            if (this.oldCommands == null)
            {
                this.oldCommands = new ArrayList();
            }
            IMenuCommandService menuService = this.MenuService;
            if (menuService != null)
            {
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeySelectNext));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeySelectPrevious));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyDefaultAction));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyMoveUp));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyMoveDown));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyMoveLeft));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyMoveRight));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyNudgeUp));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyNudgeDown));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyNudgeLeft));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyNudgeRight));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeySizeWidthIncrease));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeySizeHeightIncrease));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeySizeWidthDecrease));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeySizeHeightDecrease));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyCancel));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyReverseCancel));
                this.oldCommands.Add(menuService.FindCommand(StandardCommands.Copy));
                this.oldCommands.Add(menuService.FindCommand(StandardCommands.SelectAll));
                this.oldCommands.Add(menuService.FindCommand(MenuCommands.KeyInvokeSmartTag));
                this.oldCommands.Add(menuService.FindCommand(StandardCommands.Cut));
                this.oldCommands.Add(menuService.FindCommand(StandardCommands.Delete));
            }
        }

        public void ProcessKeySelect(bool reverse, MenuCommand cmd)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                ToolStripItem primarySelection = selectionService.PrimarySelection as ToolStripItem;
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl as ToolStripItem;
                }
                if (primarySelection != null)
                {
                    if (!this.ProcessRightLeft(!reverse))
                    {
                        this.RotateTab(reverse);
                    }
                }
                else if ((primarySelection == null) && (selectionService.PrimarySelection is ToolStrip))
                {
                    this.RotateTab(reverse);
                }
            }
        }

        private bool ProcessRightLeft(bool right)
        {
            object targetSelection = null;
            ISelectionService selectionService = this.SelectionService;
            IDesignerHost host = this.Host;
            if (((selectionService != null) && (host != null)) && (host.RootComponent is Control))
            {
                object primarySelection = selectionService.PrimarySelection;
                if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                {
                    primarySelection = this.ShiftPrimaryItem;
                }
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl;
                }
                Control control = primarySelection as Control;
                if ((targetSelection == null) && (control == null))
                {
                    ToolStripItem startItem = selectionService.PrimarySelection as ToolStripItem;
                    if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                    {
                        startItem = this.ShiftPrimaryItem as ToolStripItem;
                    }
                    if (startItem == null)
                    {
                        startItem = this.SelectedDesignerControl as ToolStripItem;
                    }
                    ToolStripDropDown currentParent = startItem.GetCurrentParent() as ToolStripDropDown;
                    if ((startItem is DesignerToolStripControlHost) && (currentParent != null))
                    {
                        if ((currentParent != null) && !right)
                        {
                            if (currentParent is ToolStripOverflow)
                            {
                                targetSelection = this.GetNextItem(currentParent, startItem, ArrowDirection.Left);
                            }
                            else
                            {
                                targetSelection = currentParent.OwnerItem;
                            }
                        }
                        if (targetSelection != null)
                        {
                            this.SetSelection(targetSelection);
                            return true;
                        }
                    }
                    else
                    {
                        ToolStripItem shiftPrimaryItem = selectionService.PrimarySelection as ToolStripItem;
                        if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                        {
                            shiftPrimaryItem = this.ShiftPrimaryItem as ToolStripDropDownItem;
                        }
                        if (shiftPrimaryItem == null)
                        {
                            shiftPrimaryItem = this.SelectedDesignerControl as ToolStripDropDownItem;
                        }
                        if ((shiftPrimaryItem != null) && shiftPrimaryItem.IsOnDropDown)
                        {
                            bool rightAlignedMenus = SystemInformation.RightAlignedMenus;
                            if ((rightAlignedMenus && right) || (!rightAlignedMenus && right))
                            {
                                ToolStripDropDownItem component = shiftPrimaryItem as ToolStripDropDownItem;
                                if (component != null)
                                {
                                    targetSelection = this.GetNextItem(component.DropDown, null, ArrowDirection.Right);
                                    if (targetSelection != null)
                                    {
                                        this.SetSelection(targetSelection);
                                        if (!component.DropDown.Visible)
                                        {
                                            ToolStripMenuItemDesigner designer = host.GetDesigner(component) as ToolStripMenuItemDesigner;
                                            if (designer != null)
                                            {
                                                designer.InitializeDropDown();
                                            }
                                        }
                                        return true;
                                    }
                                }
                            }
                            if (!right && !rightAlignedMenus)
                            {
                                ToolStripItem ownerItem = ((ToolStripDropDown) shiftPrimaryItem.Owner).OwnerItem;
                                if (!ownerItem.IsOnDropDown)
                                {
                                    ToolStrip parent = ownerItem.GetCurrentParent();
                                    targetSelection = this.GetNextItem(parent, ownerItem, ArrowDirection.Left);
                                }
                                else
                                {
                                    targetSelection = ownerItem;
                                }
                                if (targetSelection != null)
                                {
                                    this.SetSelection(targetSelection);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void ProcessUpDown(bool down)
        {
            object targetSelection = null;
            ISelectionService selectionService = this.SelectionService;
            IDesignerHost host = this.Host;
            if (((selectionService != null) && (host != null)) && (host.RootComponent is Control))
            {
                object primarySelection = selectionService.PrimarySelection;
                if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                {
                    primarySelection = this.ShiftPrimaryItem;
                }
                ContextMenuStrip parent = primarySelection as ContextMenuStrip;
                if (parent != null)
                {
                    if (down)
                    {
                        targetSelection = this.GetNextItem(parent, null, ArrowDirection.Down);
                        this.SetSelection(targetSelection);
                    }
                }
                else
                {
                    if (primarySelection == null)
                    {
                        primarySelection = this.SelectedDesignerControl;
                    }
                    Control control = primarySelection as Control;
                    if ((targetSelection == null) && (control == null))
                    {
                        ToolStripItem startItem = selectionService.PrimarySelection as ToolStripItem;
                        if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                        {
                            startItem = this.ShiftPrimaryItem as ToolStripItem;
                        }
                        if (startItem == null)
                        {
                            startItem = this.SelectedDesignerControl as ToolStripItem;
                        }
                        ToolStripDropDown currentParent = null;
                        if (startItem != null)
                        {
                            if (startItem is DesignerToolStripControlHost)
                            {
                                if (down)
                                {
                                    DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                                    if (selectedDesignerControl != null)
                                    {
                                        ToolStripTemplateNode.TransparentToolStrip strip2 = selectedDesignerControl.Control as ToolStripTemplateNode.TransparentToolStrip;
                                        if (strip2 != null)
                                        {
                                            ToolStripTemplateNode templateNode = strip2.TemplateNode;
                                            if (templateNode != null)
                                            {
                                                if (!(startItem.Owner is MenuStrip) && !(startItem.Owner is ToolStripDropDown))
                                                {
                                                    templateNode.ShowDropDownMenu();
                                                    return;
                                                }
                                                currentParent = startItem.GetCurrentParent() as ToolStripDropDown;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    currentParent = startItem.GetCurrentParent() as ToolStripDropDown;
                                }
                            }
                            else
                            {
                                ToolStripDropDownItem item2 = startItem as ToolStripDropDownItem;
                                if ((item2 != null) && !item2.IsOnDropDown)
                                {
                                    currentParent = item2.DropDown;
                                    startItem = null;
                                }
                                else if (item2 != null)
                                {
                                    currentParent = ((item2.Placement == ToolStripItemPlacement.Overflow) ? item2.Owner.OverflowButton.DropDown : ((ToolStripDropDown) item2.Owner)) as ToolStripDropDown;
                                    startItem = item2;
                                }
                                if (item2 == null)
                                {
                                    currentParent = startItem.GetCurrentParent() as ToolStripDropDown;
                                }
                            }
                            if (currentParent != null)
                            {
                                if (down)
                                {
                                    targetSelection = this.GetNextItem(currentParent, startItem, ArrowDirection.Down);
                                    if (((currentParent.OwnerItem != null) && !currentParent.OwnerItem.IsOnDropDown) && ((currentParent.OwnerItem.Owner != null) && (currentParent.OwnerItem.Owner.Site != null)))
                                    {
                                        ToolStripItem item3 = targetSelection as ToolStripItem;
                                        if (((item3 != null) && (currentParent.Items.IndexOf(item3) != -1)) && (currentParent.Items.IndexOf(item3) <= currentParent.Items.IndexOf(startItem)))
                                        {
                                            targetSelection = currentParent.OwnerItem;
                                        }
                                    }
                                    if (this.shiftPressed && this.SelectionService.GetComponentSelected(targetSelection))
                                    {
                                        this.SelectionService.SetSelectedComponents(new object[] { this.ShiftPrimaryItem, targetSelection }, SelectionTypes.Remove);
                                    }
                                }
                                else
                                {
                                    if (currentParent is ToolStripOverflow)
                                    {
                                        ToolStripItem item4 = this.GetNextItem(currentParent, null, ArrowDirection.Down);
                                        if (startItem == item4)
                                        {
                                            ToolStrip owner = startItem.Owner;
                                            if (owner != null)
                                            {
                                                targetSelection = this.GetNextItem(owner, currentParent.OwnerItem, ArrowDirection.Left);
                                            }
                                        }
                                        else
                                        {
                                            targetSelection = this.GetNextItem(currentParent, startItem, ArrowDirection.Up);
                                        }
                                    }
                                    else
                                    {
                                        targetSelection = this.GetNextItem(currentParent, startItem, ArrowDirection.Up);
                                    }
                                    if (((currentParent.OwnerItem != null) && !currentParent.OwnerItem.IsOnDropDown) && ((currentParent.OwnerItem.Owner != null) && (currentParent.OwnerItem.Owner.Site != null)))
                                    {
                                        ToolStripItem item5 = targetSelection as ToolStripItem;
                                        if (((item5 != null) && (startItem != null)) && ((currentParent.Items.IndexOf(item5) != -1) && (currentParent.Items.IndexOf(item5) >= currentParent.Items.IndexOf(startItem))))
                                        {
                                            targetSelection = currentParent.OwnerItem;
                                        }
                                    }
                                    if (this.shiftPressed && this.SelectionService.GetComponentSelected(targetSelection))
                                    {
                                        this.SelectionService.SetSelectedComponents(new object[] { this.ShiftPrimaryItem, targetSelection }, SelectionTypes.Remove);
                                    }
                                }
                                if ((targetSelection != null) && (targetSelection != startItem))
                                {
                                    this.SetSelection(targetSelection);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RemoveCommands()
        {
            IMenuCommandService menuService = this.MenuService;
            if (((menuService != null) && this.commandsAdded) && (this.newCommands != null))
            {
                foreach (MenuCommand command in this.newCommands)
                {
                    menuService.RemoveCommand(command);
                }
            }
            if (this.newCommandPaste != null)
            {
                menuService.RemoveCommand(this.newCommandPaste);
                this.newCommandPaste = null;
            }
            if (this.oldCommandPaste != null)
            {
                this.oldCommandPaste = null;
            }
            if (this.newCommands != null)
            {
                this.newCommands.Clear();
                this.newCommands = null;
            }
            if (this.oldCommands != null)
            {
                this.oldCommands.Clear();
                this.oldCommands = null;
            }
            if (this.selectionService != null)
            {
                this.selectionService.SelectionChanging -= new EventHandler(this.OnSelectionChanging);
                this.selectionService.SelectionChanged -= new EventHandler(this.OnSelectionChanged);
                this.selectionService = null;
            }
            if (this.componentChangeSvc != null)
            {
                this.componentChangeSvc.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                this.componentChangeSvc = null;
            }
            this.currentSelection = null;
            this.shiftPrimary = null;
            this.provider = null;
            this.menuCommandService = null;
            this.activeTemplateNode = null;
        }

        internal void ResetActiveTemplateNodeSelectionState()
        {
            if (this.SelectedDesignerControl != null)
            {
                DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                if (selectedDesignerControl != null)
                {
                    selectedDesignerControl.RefreshSelectionGlyph();
                }
            }
        }

        public void RestoreCommands()
        {
            IMenuCommandService menuService = this.MenuService;
            if ((menuService != null) & this.commandsAdded)
            {
                if (this.newCommands != null)
                {
                    foreach (MenuCommand command in this.newCommands)
                    {
                        menuService.RemoveCommand(command);
                    }
                }
                if (this.oldCommands != null)
                {
                    foreach (MenuCommand command2 in this.oldCommands)
                    {
                        if ((command2 != null) && (menuService.FindCommand(command2.CommandID) == null))
                        {
                            menuService.AddCommand(command2);
                        }
                    }
                }
                if (this.newCommandPaste != null)
                {
                    menuService.RemoveCommand(this.newCommandPaste);
                    this.newCommandPaste = null;
                }
                if ((this.oldCommandPaste != null) && (menuService.FindCommand(this.oldCommandPaste.CommandID) == null))
                {
                    menuService.AddCommand(this.oldCommandPaste);
                    this.oldCommandPaste = null;
                }
                this.commandsAdded = false;
            }
        }

        private void RotateParent(bool backwards)
        {
            Control rootComponent = null;
            object parent = null;
            ToolStripItem selectedDesignerControl = null;
            ISelectionService selectionService = this.SelectionService;
            IDesignerHost host = this.Host;
            if (((selectionService != null) && (host != null)) && (host.RootComponent is Control))
            {
                IContainer container = host.Container;
                Control primarySelection = selectionService.PrimarySelection as Control;
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl as Control;
                }
                if (primarySelection != null)
                {
                    rootComponent = primarySelection;
                }
                else
                {
                    selectedDesignerControl = selectionService.PrimarySelection as ToolStripItem;
                    if (selectedDesignerControl == null)
                    {
                        selectedDesignerControl = this.SelectedDesignerControl as ToolStripItem;
                    }
                    if (selectedDesignerControl == null)
                    {
                        rootComponent = (Control) host.RootComponent;
                    }
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
                    else if (selectedDesignerControl != null)
                    {
                        parent = selectedDesignerControl.Owner.Controls[0];
                    }
                }
                else if (rootComponent != null)
                {
                    parent = rootComponent.Parent;
                    Control control3 = parent as Control;
                    if (((control3 == null) || (control3.Site == null)) || (control3.Site.Container != container))
                    {
                        parent = rootComponent;
                    }
                }
                else if (selectedDesignerControl != null)
                {
                    if (selectedDesignerControl.IsOnDropDown && (selectedDesignerControl.Placement != ToolStripItemPlacement.Overflow))
                    {
                        parent = ((ToolStripDropDown) selectedDesignerControl.Owner).OwnerItem;
                    }
                    else if (selectedDesignerControl.IsOnDropDown && (selectedDesignerControl.Placement == ToolStripItemPlacement.Overflow))
                    {
                        ToolStrip owner = selectedDesignerControl.Owner;
                        if (owner != null)
                        {
                            owner.OverflowButton.HideDropDown();
                        }
                        parent = selectedDesignerControl.Owner;
                    }
                    else
                    {
                        parent = selectedDesignerControl.Owner;
                    }
                }
                if (parent is DesignerToolStripControlHost)
                {
                    this.SelectedDesignerControl = parent;
                    selectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                }
                else
                {
                    this.SelectedDesignerControl = null;
                    selectionService.SetSelectedComponents(new object[] { parent }, SelectionTypes.Replace);
                }
            }
        }

        public void RotateTab(bool backwards)
        {
            object component = null;
            ISelectionService selectionService = this.SelectionService;
            IDesignerHost host = this.Host;
            if (((selectionService != null) && (host != null)) && (host.RootComponent is Control))
            {
                Control owner;
                IContainer container = host.Container;
                Control rootComponent = (Control) host.RootComponent;
                object primarySelection = selectionService.PrimarySelection;
                if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                {
                    primarySelection = this.ShiftPrimaryItem;
                }
                if (primarySelection == null)
                {
                    primarySelection = this.SelectedDesignerControl;
                    if (primarySelection != null)
                    {
                        DesignerToolStripControlHost host2 = primarySelection as DesignerToolStripControlHost;
                        if ((host2 != null) && (!host2.IsOnDropDown || (host2.IsOnDropDown && host2.IsOnOverflow)))
                        {
                            owner = host2.Owner;
                            if (((owner.RightToLeft != RightToLeft.Yes) && !backwards) || ((owner.RightToLeft == RightToLeft.Yes) && backwards))
                            {
                                component = this.GetNextControlInTab(rootComponent, owner, !backwards);
                                if (component == null)
                                {
                                    ComponentTray service = (ComponentTray) this.provider.GetService(typeof(ComponentTray));
                                    if (service != null)
                                    {
                                        component = service.GetNextComponent((IComponent) primarySelection, !backwards);
                                        if (component != null)
                                        {
                                            ControlDesigner designer = host.GetDesigner((IComponent) component) as ControlDesigner;
                                            while (designer != null)
                                            {
                                                component = service.GetNextComponent((IComponent) component, !backwards);
                                                if (component != null)
                                                {
                                                    designer = host.GetDesigner((IComponent) component) as ControlDesigner;
                                                }
                                                else
                                                {
                                                    designer = null;
                                                }
                                            }
                                        }
                                    }
                                    if (component == null)
                                    {
                                        component = rootComponent;
                                    }
                                }
                            }
                        }
                    }
                }
                owner = primarySelection as Control;
                ToolStrip strip = owner as ToolStrip;
                if ((component == null) && (strip != null))
                {
                    ToolStripItemCollection items = strip.Items;
                    if (items != null)
                    {
                        if (!backwards)
                        {
                            component = items[0];
                        }
                        else
                        {
                            component = items[strip.Items.Count - 1];
                        }
                    }
                }
                if ((component == null) && (owner == null))
                {
                    ToolStripItem startItem = selectionService.PrimarySelection as ToolStripItem;
                    if (this.shiftPressed && (this.ShiftPrimaryItem != null))
                    {
                        startItem = this.ShiftPrimaryItem as ToolStripItem;
                    }
                    if (startItem == null)
                    {
                        startItem = this.SelectedDesignerControl as ToolStripItem;
                    }
                    if (((startItem != null) && startItem.IsOnDropDown) && (startItem.Placement != ToolStripItemPlacement.Overflow))
                    {
                        DesignerToolStripControlHost host3 = startItem as DesignerToolStripControlHost;
                        if (host3 != null)
                        {
                            ToolStripItem ownerItem = ((ToolStripDropDown) host3.Owner).OwnerItem;
                            ToolStripDropDown firstDropDown = (host.GetDesigner(ownerItem) as ToolStripMenuItemDesigner).GetFirstDropDown((ToolStripDropDownItem) ownerItem);
                            if (firstDropDown != null)
                            {
                                startItem = firstDropDown.OwnerItem;
                            }
                            else
                            {
                                startItem = ownerItem;
                            }
                        }
                    }
                    if ((startItem != null) && !(startItem is DesignerToolStripControlHost))
                    {
                        ToolStrip currentParent = startItem.GetCurrentParent();
                        if (currentParent != null)
                        {
                            if (backwards)
                            {
                                if (currentParent is ToolStripOverflow)
                                {
                                    ToolStripItem item3 = this.GetNextItem(currentParent, null, ArrowDirection.Down);
                                    if (startItem == item3)
                                    {
                                        ToolStrip parent = startItem.Owner;
                                        if (parent != null)
                                        {
                                            component = this.GetNextItem(parent, ((ToolStripDropDown) currentParent).OwnerItem, ArrowDirection.Left);
                                        }
                                    }
                                    else
                                    {
                                        component = this.GetNextItem(currentParent, startItem, ArrowDirection.Left);
                                    }
                                }
                                else if ((startItem == currentParent.Items[0]) && (currentParent.RightToLeft != RightToLeft.Yes))
                                {
                                    if (this.shiftPressed)
                                    {
                                        return;
                                    }
                                    component = this.GetNextControlInTab(rootComponent, currentParent, !backwards);
                                    if (component == null)
                                    {
                                        ComponentTray tray2 = (ComponentTray) this.provider.GetService(typeof(ComponentTray));
                                        if (tray2 != null)
                                        {
                                            component = tray2.GetNextComponent((IComponent) primarySelection, !backwards);
                                            if (component != null)
                                            {
                                                ControlDesigner designer3 = host.GetDesigner((IComponent) component) as ControlDesigner;
                                                while (designer3 != null)
                                                {
                                                    component = tray2.GetNextComponent((IComponent) component, !backwards);
                                                    if (component != null)
                                                    {
                                                        designer3 = host.GetDesigner((IComponent) component) as ControlDesigner;
                                                    }
                                                    else
                                                    {
                                                        designer3 = null;
                                                    }
                                                }
                                            }
                                        }
                                        if (component == null)
                                        {
                                            component = rootComponent;
                                        }
                                    }
                                }
                                else
                                {
                                    component = this.GetNextItem(currentParent, startItem, ArrowDirection.Left);
                                    if (this.shiftPressed && this.SelectionService.GetComponentSelected(component))
                                    {
                                        this.SelectionService.SetSelectedComponents(new object[] { this.ShiftPrimaryItem, component }, SelectionTypes.Remove);
                                    }
                                }
                            }
                            else if (currentParent is ToolStripOverflow)
                            {
                                component = this.GetNextItem(currentParent, startItem, ArrowDirection.Down);
                            }
                            else if ((startItem == currentParent.Items[0]) && (currentParent.RightToLeft == RightToLeft.Yes))
                            {
                                if (this.shiftPressed)
                                {
                                    return;
                                }
                                component = this.GetNextControlInTab(rootComponent, currentParent, !backwards);
                                if (component == null)
                                {
                                    component = rootComponent;
                                }
                            }
                            else
                            {
                                component = this.GetNextItem(currentParent, startItem, ArrowDirection.Right);
                                if (this.shiftPressed && this.SelectionService.GetComponentSelected(component))
                                {
                                    this.SelectionService.SetSelectedComponents(new object[] { this.ShiftPrimaryItem, component }, SelectionTypes.Remove);
                                }
                            }
                        }
                    }
                    else if (startItem != null)
                    {
                        ToolStrip ctl = startItem.GetCurrentParent();
                        if (ctl != null)
                        {
                            if (ctl.RightToLeft == RightToLeft.Yes)
                            {
                                backwards = !backwards;
                            }
                            if (backwards)
                            {
                                ToolStripItemCollection items2 = ctl.Items;
                                if (items2.Count >= 2)
                                {
                                    component = items2[items2.Count - 2];
                                }
                                else
                                {
                                    component = this.GetNextControlInTab(rootComponent, ctl, !backwards);
                                }
                            }
                            else
                            {
                                component = ctl.Items[0];
                            }
                        }
                    }
                }
                if (((component == null) && (owner != null)) && (rootComponent.Contains(owner) || (rootComponent == primarySelection)))
                {
                    while ((owner = this.GetNextControlInTab(rootComponent, owner, !backwards)) != null)
                    {
                        if (((owner.Site != null) && (owner.Site.Container == container)) && !(owner is ToolStripPanel))
                        {
                            break;
                        }
                    }
                    component = owner;
                }
                if (component == null)
                {
                    ComponentTray tray3 = (ComponentTray) this.provider.GetService(typeof(ComponentTray));
                    if (tray3 != null)
                    {
                        component = tray3.GetNextComponent((IComponent) primarySelection, !backwards);
                    }
                    if ((component == null) || (component == primarySelection))
                    {
                        component = rootComponent;
                    }
                }
                if ((component is DesignerToolStripControlHost) && (primarySelection is DesignerToolStripControlHost))
                {
                    this.SelectedDesignerControl = component;
                    selectionService.SetSelectedComponents(new object[] { component }, SelectionTypes.Replace);
                    selectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                }
                else
                {
                    this.SetSelection(component);
                }
            }
        }

        private void SelectItems(ToolStrip parent)
        {
            object[] components = new object[parent.Items.Count - 1];
            for (int i = 0; i < (parent.Items.Count - 1); i++)
            {
                if (!(parent.Items[i] is DesignerToolStripControlHost))
                {
                    components[i] = parent.Items[i];
                }
            }
            this.SelectionService.SetSelectedComponents(components, SelectionTypes.Replace);
        }

        private void SetSelection(object targetSelection)
        {
            ISelectionService selectionService = this.SelectionService;
            if (selectionService != null)
            {
                ArrayList originalSelComps = new ArrayList(selectionService.GetSelectedComponents());
                if ((originalSelComps.Count == 0) && (this.SelectedDesignerControl != null))
                {
                    originalSelComps.Add(this.SelectedDesignerControl);
                }
                if (targetSelection is DesignerToolStripControlHost)
                {
                    if (!this.shiftPressed)
                    {
                        this.SelectedDesignerControl = targetSelection;
                        selectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                    }
                }
                else
                {
                    ToolStripOverflowButton button = targetSelection as ToolStripOverflowButton;
                    if (button != null)
                    {
                        this.SelectedDesignerControl = null;
                        if (button != null)
                        {
                            button.ShowDropDown();
                        }
                        object obj2 = this.GetNextItem(button.DropDown, null, ArrowDirection.Down);
                        if (!this.shiftPressed)
                        {
                            this.ShiftPrimaryItem = null;
                            selectionService.SetSelectedComponents(new object[] { obj2 }, SelectionTypes.Replace);
                        }
                        else
                        {
                            selectionService.SetSelectedComponents(new object[] { obj2 });
                            this.ShiftPrimaryItem = targetSelection;
                        }
                    }
                    else
                    {
                        this.SelectedDesignerControl = null;
                        if (!this.shiftPressed)
                        {
                            this.ShiftPrimaryItem = null;
                            selectionService.SetSelectedComponents(new object[] { targetSelection }, SelectionTypes.Replace);
                        }
                        else
                        {
                            selectionService.SetSelectedComponents(new object[] { targetSelection });
                            this.ShiftPrimaryItem = targetSelection;
                        }
                    }
                }
                ToolStripDesignerUtils.InvalidateSelection(originalSelComps, targetSelection as ToolStripItem, this.provider, this.shiftPressed);
            }
            this.shiftPressed = false;
        }

        internal ToolStripTemplateNode ActiveTemplateNode
        {
            get
            {
                return this.activeTemplateNode;
            }
            set
            {
                this.activeTemplateNode = value;
                this.ResetActiveTemplateNodeSelectionState();
            }
        }

        internal bool ContextMenuShownByKeyBoard
        {
            get
            {
                return this.contextMenuShownByKeyBoard;
            }
            set
            {
                this.contextMenuShownByKeyBoard = value;
            }
        }

        internal bool CopyInProgress
        {
            get
            {
                return this.copyInProgress;
            }
            set
            {
                if (value != this.CopyInProgress)
                {
                    this.copyInProgress = value;
                }
            }
        }

        internal bool CutOrDeleteInProgress
        {
            get
            {
                return this.cutOrDeleteInProgress;
            }
            set
            {
                if (value != this.cutOrDeleteInProgress)
                {
                    this.cutOrDeleteInProgress = value;
                }
            }
        }

        private IDesignerHost Host
        {
            get
            {
                return this.designerHost;
            }
        }

        private IMenuCommandService MenuService
        {
            get
            {
                if ((this.menuCommandService == null) && (this.provider != null))
                {
                    this.menuCommandService = (IMenuCommandService) this.provider.GetService(typeof(IMenuCommandService));
                }
                return this.menuCommandService;
            }
        }

        internal object OwnerItemAfterCut
        {
            get
            {
                return this.ownerItemAfterCut;
            }
            set
            {
                this.ownerItemAfterCut = value;
            }
        }

        internal object SelectedDesignerControl
        {
            get
            {
                return this.currentSelection;
            }
            set
            {
                if (value != this.SelectedDesignerControl)
                {
                    DesignerToolStripControlHost selectedDesignerControl = this.SelectedDesignerControl as DesignerToolStripControlHost;
                    if (selectedDesignerControl != null)
                    {
                        selectedDesignerControl.RefreshSelectionGlyph();
                    }
                    this.currentSelection = value;
                    if (this.currentSelection != null)
                    {
                        DesignerToolStripControlHost currentSelection = this.currentSelection as DesignerToolStripControlHost;
                        if (currentSelection != null)
                        {
                            currentSelection.SelectControl();
                            ToolStripItem.ToolStripItemAccessibleObject accessibilityObject = currentSelection.AccessibilityObject as ToolStripItem.ToolStripItemAccessibleObject;
                            if (accessibilityObject != null)
                            {
                                accessibilityObject.AddState(AccessibleStates.Focused | AccessibleStates.Selected);
                                ToolStrip currentParent = currentSelection.GetCurrentParent();
                                int index = 0;
                                if (currentParent != null)
                                {
                                    index = currentParent.Items.IndexOf(currentSelection);
                                }
                                System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8007, new HandleRef(currentParent, currentParent.Handle), -4, index + 1);
                                System.Design.UnsafeNativeMethods.NotifyWinEvent(0x8005, new HandleRef(currentParent, currentParent.Handle), -4, index + 1);
                            }
                        }
                    }
                }
            }
        }

        private ISelectionService SelectionService
        {
            get
            {
                return this.selectionService;
            }
        }

        internal object ShiftPrimaryItem
        {
            get
            {
                return this.shiftPrimary;
            }
            set
            {
                this.shiftPrimary = value;
            }
        }

        internal bool TemplateNodeActive
        {
            get
            {
                return this.templateNodeActive;
            }
            set
            {
                this.templateNodeActive = value;
                if (this.newCommands != null)
                {
                    foreach (MenuCommand command in this.newCommands)
                    {
                        command.Enabled = !this.templateNodeActive;
                    }
                }
            }
        }

        internal bool TemplateNodeContextMenuOpen
        {
            get
            {
                return this.templateNodeContextMenuOpen;
            }
            set
            {
                this.templateNodeContextMenuOpen = value;
                if (this.newCommands != null)
                {
                    foreach (MenuCommand command in this.newCommands)
                    {
                        command.Enabled = !this.templateNodeActive;
                    }
                }
            }
        }
    }
}

