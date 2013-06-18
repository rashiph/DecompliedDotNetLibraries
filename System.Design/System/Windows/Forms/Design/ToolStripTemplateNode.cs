namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;
    using System.Windows.Forms.Design.Behavior;

    internal class ToolStripTemplateNode : IMenuStatusHandler
    {
        private IDesigner _designer;
        private IDesignerHost _designerHost;
        private DesignSurface _designSurface;
        private TransparentToolStrip _miniToolStrip;
        private bool active;
        private ToolStripItem activeItem;
        private MenuCommand[] addCommands;
        private ToolStripSplitButton addItemButton;
        private System.Windows.Forms.Design.Behavior.BehaviorService behaviorService;
        private Rectangle boundingRect;
        private ToolStripLabel centerLabel;
        private ToolStripControlHost centerTextBox;
        private MenuCommand[] commands;
        private IComponent component;
        private ToolStripDropDown contextMenu;
        private DesignerToolStripControlHost controlHost;
        private const int GLYPHBORDER = 1;
        private const int GLYPHINSET = 2;
        private Rectangle hotRegion;
        internal bool ignoreFirstKeyUp;
        private bool imeModeSet;
        private bool inSituMode;
        private bool isSystemContextMenuDisplayed;
        private System.Type itemType;
        private ItemTypeToolStripMenuItem lastSelection;
        private MenuCommand oldRedoCommand;
        private MenuCommand oldUndoCommand;
        private MiniToolStripRenderer renderer;
        private ISelectionService selectionService;
        private ToolStripKeyboardHandlingService toolStripKeyBoardService;

        public event EventHandler Activated;

        public event EventHandler Closed;

        public event EventHandler Deactivated;

        public ToolStripTemplateNode(IComponent component, string text, Image image)
        {
            this.component = component;
            this.activeItem = component as ToolStripItem;
            this._designerHost = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
            this._designer = this._designerHost.GetDesigner(component);
            this._designSurface = (DesignSurface) component.Site.GetService(typeof(DesignSurface));
            if (this._designSurface != null)
            {
                this._designSurface.Flushed += new EventHandler(this.OnLoaderFlushed);
            }
            this.SetupNewEditNode(this, text, image, component);
            this.commands = new MenuCommand[] { 
                new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyMoveUp), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyMoveDown), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyMoveLeft), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyMoveRight), new MenuCommand(new EventHandler(this.OnMenuCut), StandardCommands.Delete), new MenuCommand(new EventHandler(this.OnMenuCut), StandardCommands.Cut), new MenuCommand(new EventHandler(this.OnMenuCut), StandardCommands.Copy), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeUp), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeDown), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeLeft), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeRight), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeySizeWidthIncrease), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeySizeHeightIncrease), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeySizeWidthDecrease), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeySizeHeightDecrease), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeWidthIncrease), 
                new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeHeightIncrease), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeWidthDecrease), new MenuCommand(new EventHandler(this.OnMenuCut), MenuCommands.KeyNudgeHeightDecrease)
             };
            this.addCommands = new MenuCommand[] { new MenuCommand(new EventHandler(this.OnMenuCut), StandardCommands.Undo), new MenuCommand(new EventHandler(this.OnMenuCut), StandardCommands.Redo) };
        }

        private void AddNewItemClick(object sender, EventArgs e)
        {
            if (this.addItemButton != null)
            {
                this.addItemButton.DropDown.Visible = false;
            }
            if ((this.component is ToolStrip) && (this.SelectionService != null))
            {
                ToolStripDesigner designer = this._designerHost.GetDesigner(this.component) as ToolStripDesigner;
                try
                {
                    if (designer != null)
                    {
                        designer.DontCloseOverflow = true;
                    }
                    this.SelectionService.SetSelectedComponents(new object[] { this.component });
                }
                finally
                {
                    if (designer != null)
                    {
                        designer.DontCloseOverflow = false;
                    }
                }
            }
            ItemTypeToolStripMenuItem item = (ItemTypeToolStripMenuItem) sender;
            if (this.lastSelection != null)
            {
                this.lastSelection.Checked = false;
            }
            item.Checked = true;
            this.lastSelection = item;
            this.ToolStripItemType = item.ItemType;
            if (this.controlHost.GetCurrentParent() is MenuStrip)
            {
                this.CommitEditor(true, true, false);
            }
            else
            {
                this.CommitEditor(true, false, false);
            }
            if (this.KeyboardService != null)
            {
                this.KeyboardService.TemplateNodeActive = false;
            }
        }

        private void CenterLabelClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if ((this.KeyboardService == null) || !this.KeyboardService.TemplateNodeActive)
                {
                    if (this.KeyboardService != null)
                    {
                        this.KeyboardService.SelectedDesignerControl = this.controlHost;
                    }
                    this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                    if (this.BehaviorService != null)
                    {
                        Point p = this.BehaviorService.ControlToAdornerWindow(this._miniToolStrip);
                        p = this.BehaviorService.AdornerWindowPointToScreen(p);
                        p.Offset(e.Location);
                        this.DesignerContextMenu.Show(p);
                    }
                }
            }
            else if (this.hotRegion.Contains(e.Location) && !this.KeyboardService.TemplateNodeActive)
            {
                if (this.KeyboardService != null)
                {
                    this.KeyboardService.SelectedDesignerControl = this.controlHost;
                }
                this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                ToolStripDropDown contextMenu = this.contextMenu;
                if (contextMenu != null)
                {
                    contextMenu.Closed -= new ToolStripDropDownClosedEventHandler(this.OnContextMenuClosed);
                    contextMenu.Opened -= new EventHandler(this.OnContextMenuOpened);
                    contextMenu.Dispose();
                }
                this.contextMenu = null;
                this.ShowDropDownMenu();
            }
            else
            {
                ToolStripDesigner.LastCursorPosition = Cursor.Position;
                if (this._designer is ToolStripDesigner)
                {
                    if (this.KeyboardService.TemplateNodeActive)
                    {
                        this.KeyboardService.ActiveTemplateNode.Commit(false, false);
                    }
                    if (this.SelectionService.PrimarySelection == null)
                    {
                        this.SelectionService.SetSelectedComponents(new object[] { this.component }, SelectionTypes.Replace);
                    }
                    this.KeyboardService.SelectedDesignerControl = this.controlHost;
                    this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
                    ((ToolStripDesigner) this._designer).ShowEditNode(true);
                }
                if (this._designer is ToolStripMenuItemDesigner)
                {
                    IServiceProvider site = this.component.Site;
                    if (this.KeyboardService.TemplateNodeActive)
                    {
                        ToolStripItem component = this.component as ToolStripItem;
                        if (component != null)
                        {
                            if (component.Visible)
                            {
                                this.KeyboardService.ActiveTemplateNode.Commit(false, false);
                            }
                            else
                            {
                                this.KeyboardService.ActiveTemplateNode.Commit(false, true);
                            }
                        }
                        else
                        {
                            this.KeyboardService.ActiveTemplateNode.Commit(false, false);
                        }
                    }
                    if (this._designer != null)
                    {
                        ((ToolStripMenuItemDesigner) this._designer).EditTemplateNode(true);
                    }
                    else
                    {
                        ISelectionService service = (ISelectionService) site.GetService(typeof(ISelectionService));
                        ToolStripItem primarySelection = service.PrimarySelection as ToolStripItem;
                        if ((primarySelection != null) && (this._designerHost != null))
                        {
                            ToolStripMenuItemDesigner designer = this._designerHost.GetDesigner(primarySelection) as ToolStripMenuItemDesigner;
                            if (designer != null)
                            {
                                if (!primarySelection.IsOnDropDown)
                                {
                                    Rectangle glyphBounds = designer.GetGlyphBounds();
                                    ToolStripDesignerUtils.GetAdjustedBounds(primarySelection, ref glyphBounds);
                                    System.Windows.Forms.Design.Behavior.BehaviorService service2 = site.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService)) as System.Windows.Forms.Design.Behavior.BehaviorService;
                                    if (service2 != null)
                                    {
                                        service2.Invalidate(glyphBounds);
                                    }
                                }
                                designer.EditTemplateNode(true);
                            }
                        }
                    }
                }
            }
        }

        private void CenterLabelMouseEnter(object sender, EventArgs e)
        {
            if (((this.renderer != null) && !this.KeyboardService.TemplateNodeActive) && (this.renderer.State != 6))
            {
                this.renderer.State = 4;
                this._miniToolStrip.Invalidate();
            }
        }

        private void CenterLabelMouseLeave(object sender, EventArgs e)
        {
            if ((this.renderer != null) && !this.KeyboardService.TemplateNodeActive)
            {
                if (this.renderer.State != 6)
                {
                    this.renderer.State = 0;
                }
                if ((this.KeyboardService != null) && (this.KeyboardService.SelectedDesignerControl == this.controlHost))
                {
                    this.renderer.State = 1;
                }
                this._miniToolStrip.Invalidate();
            }
        }

        private void CenterLabelMouseMove(object sender, MouseEventArgs e)
        {
            if (((this.renderer != null) && !this.KeyboardService.TemplateNodeActive) && (this.renderer.State != 6))
            {
                if (this.hotRegion.Contains(e.Location))
                {
                    this.renderer.State = 5;
                }
                else
                {
                    this.renderer.State = 4;
                }
                this._miniToolStrip.Invalidate();
            }
        }

        private void CenterTextBoxMouseEnter(object sender, EventArgs e)
        {
            if (this.renderer != null)
            {
                this.renderer.State = 1;
                this._miniToolStrip.Invalidate();
            }
        }

        private void CenterTextBoxMouseLeave(object sender, EventArgs e)
        {
            if ((this.renderer != null) && !this.Active)
            {
                this.renderer.State = 0;
                this._miniToolStrip.Invalidate();
            }
        }

        internal void CloseEditor()
        {
            if (this._miniToolStrip != null)
            {
                this.Active = false;
                if (this.lastSelection != null)
                {
                    this.lastSelection.Dispose();
                    this.lastSelection = null;
                }
                ToolStrip component = this.component as ToolStrip;
                if (component != null)
                {
                    component.RightToLeftChanged -= new EventHandler(this.OnRightToLeftChanged);
                }
                else
                {
                    ToolStripDropDownItem item = this.component as ToolStripDropDownItem;
                    if (item != null)
                    {
                        item.RightToLeftChanged -= new EventHandler(this.OnRightToLeftChanged);
                    }
                }
                if (this.centerLabel != null)
                {
                    this.centerLabel.MouseUp -= new MouseEventHandler(this.CenterLabelClick);
                    this.centerLabel.MouseEnter -= new EventHandler(this.CenterLabelMouseEnter);
                    this.centerLabel.MouseMove -= new MouseEventHandler(this.CenterLabelMouseMove);
                    this.centerLabel.MouseLeave -= new EventHandler(this.CenterLabelMouseLeave);
                    this.centerLabel.Dispose();
                    this.centerLabel = null;
                }
                if (this.addItemButton != null)
                {
                    this.addItemButton.MouseMove -= new MouseEventHandler(this.OnMouseMove);
                    this.addItemButton.MouseUp -= new MouseEventHandler(this.OnMouseUp);
                    this.addItemButton.MouseDown -= new MouseEventHandler(this.OnMouseDown);
                    this.addItemButton.DropDownOpened -= new EventHandler(this.OnAddItemButtonDropDownOpened);
                    this.addItemButton.DropDown.Dispose();
                    this.addItemButton.Dispose();
                    this.addItemButton = null;
                }
                if (this.contextMenu != null)
                {
                    this.contextMenu.Closed -= new ToolStripDropDownClosedEventHandler(this.OnContextMenuClosed);
                    this.contextMenu.Opened -= new EventHandler(this.OnContextMenuOpened);
                    this.contextMenu = null;
                }
                this._miniToolStrip.MouseLeave -= new EventHandler(this.OnMouseLeave);
                this._miniToolStrip.Dispose();
                this._miniToolStrip = null;
                if (this._designSurface != null)
                {
                    this._designSurface.Flushed -= new EventHandler(this.OnLoaderFlushed);
                    this._designSurface = null;
                }
                this._designer = null;
                this.OnClosed(new EventArgs());
            }
        }

        internal void Commit(bool enterKeyPressed, bool tabKeyPressed)
        {
            if ((this._miniToolStrip != null) && this.inSituMode)
            {
                if (string.IsNullOrEmpty(((TextBox) this.centerTextBox.Control).Text))
                {
                    this.RollBack();
                }
                else
                {
                    this.CommitEditor(true, enterKeyPressed, tabKeyPressed);
                }
            }
        }

        internal void CommitAndSelect()
        {
            this.Commit(false, false);
        }

        private void CommitEditor(bool commit, bool enterKeyPressed, bool tabKeyPressed)
        {
            ToolStripItem primarySelection = this.SelectionService.PrimarySelection as ToolStripItem;
            string text = (this.centerTextBox != null) ? ((TextBox) this.centerTextBox.Control).Text : string.Empty;
            this.ExitInSituEdit();
            this.FocusForm();
            if (commit && ((this._designer is ToolStripDesigner) || (this._designer is ToolStripMenuItemDesigner)))
            {
                System.Type t = null;
                if ((text == "-") && (this._designer is ToolStripMenuItemDesigner))
                {
                    this.ToolStripItemType = typeof(ToolStripSeparator);
                }
                if (this.ToolStripItemType != null)
                {
                    t = this.ToolStripItemType;
                    this.ToolStripItemType = null;
                }
                else
                {
                    t = ToolStripDesignerUtils.GetStandardItemTypes(this.component)[0];
                }
                if (this._designer is ToolStripDesigner)
                {
                    ((ToolStripDesigner) this._designer).AddNewItem(t, text, enterKeyPressed, tabKeyPressed);
                }
                else
                {
                    ((ToolStripItemDesigner) this._designer).CommitEdit(t, text, commit, enterKeyPressed, tabKeyPressed);
                }
            }
            else if (this._designer is ToolStripItemDesigner)
            {
                ((ToolStripItemDesigner) this._designer).CommitEdit(this._designer.Component.GetType(), text, commit, enterKeyPressed, tabKeyPressed);
            }
            if ((primarySelection != null) && (this._designerHost != null))
            {
                ToolStripItemDesigner designer = this._designerHost.GetDesigner(primarySelection) as ToolStripItemDesigner;
                if (designer != null)
                {
                    Rectangle glyphBounds = designer.GetGlyphBounds();
                    ToolStripDesignerUtils.GetAdjustedBounds(primarySelection, ref glyphBounds);
                    glyphBounds.Inflate(1, 1);
                    Region r = new Region(glyphBounds);
                    glyphBounds.Inflate(-2, -2);
                    r.Exclude(glyphBounds);
                    if (this.BehaviorService != null)
                    {
                        this.BehaviorService.Invalidate(r);
                    }
                    r.Dispose();
                }
            }
        }

        private void EnterInSituEdit()
        {
            if (!this.inSituMode)
            {
                if (this._miniToolStrip.Parent != null)
                {
                    this._miniToolStrip.Parent.SuspendLayout();
                }
                try
                {
                    this.Active = true;
                    this.inSituMode = true;
                    if (this.renderer != null)
                    {
                        this.renderer.State = 1;
                    }
                    TextBox c = new TemplateTextBox(this._miniToolStrip, this) {
                        BorderStyle = BorderStyle.FixedSingle,
                        Text = this.centerLabel.Text,
                        ForeColor = SystemColors.WindowText
                    };
                    int num = 90;
                    this.centerTextBox = new ToolStripControlHost(c);
                    this.centerTextBox.Dock = DockStyle.None;
                    this.centerTextBox.AutoSize = false;
                    this.centerTextBox.Width = num;
                    ToolStripDropDownItem activeItem = this.activeItem as ToolStripDropDownItem;
                    if ((activeItem != null) && !activeItem.IsOnDropDown)
                    {
                        this.centerTextBox.Margin = new Padding(1, 2, 1, 3);
                    }
                    else
                    {
                        this.centerTextBox.Margin = new Padding(1);
                    }
                    this.centerTextBox.Size = this._miniToolStrip.DisplayRectangle.Size - this.centerTextBox.Margin.Size;
                    this.centerTextBox.Name = "centerTextBox";
                    this.centerTextBox.MouseEnter += new EventHandler(this.CenterTextBoxMouseEnter);
                    this.centerTextBox.MouseLeave += new EventHandler(this.CenterTextBoxMouseLeave);
                    int index = this._miniToolStrip.Items.IndexOf(this.centerLabel);
                    if (index != -1)
                    {
                        this._miniToolStrip.Items.Insert(index, this.centerTextBox);
                        this._miniToolStrip.Items.Remove(this.centerLabel);
                    }
                    c.KeyUp += new KeyEventHandler(this.OnKeyUp);
                    c.KeyDown += new KeyEventHandler(this.OnKeyDown);
                    c.SelectAll();
                    Control rootComponent = null;
                    if (this._designerHost != null)
                    {
                        rootComponent = (Control) this._designerHost.RootComponent;
                        System.Design.NativeMethods.SendMessage(rootComponent.Handle, 11, 0, 0);
                        c.Focus();
                        System.Design.NativeMethods.SendMessage(rootComponent.Handle, 11, 1, 0);
                    }
                }
                finally
                {
                    if (this._miniToolStrip.Parent != null)
                    {
                        this._miniToolStrip.Parent.ResumeLayout();
                    }
                }
            }
        }

        private void ExitInSituEdit()
        {
            if ((this.centerTextBox != null) && this.inSituMode)
            {
                if (this._miniToolStrip.Parent != null)
                {
                    this._miniToolStrip.Parent.SuspendLayout();
                }
                try
                {
                    int index = this._miniToolStrip.Items.IndexOf(this.centerTextBox);
                    if (index != -1)
                    {
                        this.centerLabel.Text = System.Design.SR.GetString("ToolStripDesignerTemplateNodeEnterText");
                        this._miniToolStrip.Items.Insert(index, this.centerLabel);
                        this._miniToolStrip.Items.Remove(this.centerTextBox);
                        ((TextBox) this.centerTextBox.Control).KeyUp -= new KeyEventHandler(this.OnKeyUp);
                        ((TextBox) this.centerTextBox.Control).KeyDown -= new KeyEventHandler(this.OnKeyDown);
                    }
                    this.centerTextBox.MouseEnter -= new EventHandler(this.CenterTextBoxMouseEnter);
                    this.centerTextBox.MouseLeave -= new EventHandler(this.CenterTextBoxMouseLeave);
                    this.centerTextBox.Dispose();
                    this.centerTextBox = null;
                    this.inSituMode = false;
                    this.SetWidth(null);
                }
                finally
                {
                    if (this._miniToolStrip.Parent != null)
                    {
                        this._miniToolStrip.Parent.ResumeLayout();
                    }
                    this.Active = false;
                }
            }
        }

        internal void FocusEditor(ToolStripItem currentItem)
        {
            if (currentItem != null)
            {
                this.centerLabel.Text = currentItem.Text;
            }
            this.EnterInSituEdit();
        }

        private void FocusForm()
        {
            DesignerFrame service = this.component.Site.GetService(typeof(ISplitWindowService)) as DesignerFrame;
            if (service != null)
            {
                Control rootComponent = null;
                if (this._designerHost != null)
                {
                    rootComponent = (Control) this._designerHost.RootComponent;
                    System.Design.NativeMethods.SendMessage(rootComponent.Handle, 11, 0, 0);
                    service.Focus();
                    System.Design.NativeMethods.SendMessage(rootComponent.Handle, 11, 1, 0);
                }
            }
        }

        protected void OnActivated(EventArgs e)
        {
            if (this.onActivated != null)
            {
                this.onActivated(this, e);
            }
        }

        private void OnAddItemButtonDropDownOpened(object sender, EventArgs e)
        {
            this.addItemButton.DropDown.Focus();
        }

        protected void OnClosed(EventArgs e)
        {
            if (this.onClosed != null)
            {
                this.onClosed(this, e);
            }
        }

        private void OnContextMenuClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            if (this.renderer != null)
            {
                this.renderer.State = 1;
                this._miniToolStrip.Invalidate();
            }
        }

        private void OnContextMenuOpened(object sender, EventArgs e)
        {
            if (this.KeyboardService != null)
            {
                this.KeyboardService.TemplateNodeContextMenuOpen = true;
            }
        }

        protected void OnDeactivated(EventArgs e)
        {
            if (this.onDeactivated != null)
            {
                this.onDeactivated(this, e);
            }
        }

        private void OnKeyDefaultAction(object sender, EventArgs e)
        {
            this.Active = false;
            if (this.centerTextBox.Control != null)
            {
                if (string.IsNullOrEmpty(((TextBox) this.centerTextBox.Control).Text))
                {
                    this.CommitEditor(false, false, false);
                }
                else
                {
                    this.CommitEditor(true, true, false);
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!this.IMEModeSet && ((e.KeyCode == Keys.A) && ((e.KeyData & Keys.Control) != Keys.None)))
            {
                TextBox box = sender as TextBox;
                if (box != null)
                {
                    box.SelectAll();
                }
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (!this.IMEModeSet)
            {
                switch (e.KeyCode)
                {
                    case Keys.Up:
                        this.Commit(false, true);
                        if (this.KeyboardService != null)
                        {
                            this.KeyboardService.ProcessUpDown(false);
                        }
                        return;

                    case Keys.Right:
                        return;

                    case Keys.Down:
                        this.Commit(true, false);
                        return;

                    case Keys.Escape:
                        this.CommitEditor(false, false, false);
                        return;

                    case Keys.Enter:
                        if (this.ignoreFirstKeyUp)
                        {
                            this.ignoreFirstKeyUp = false;
                            return;
                        }
                        this.OnKeyDefaultAction(sender, e);
                        return;
                }
            }
        }

        private void OnLoaderFlushed(object sender, EventArgs e)
        {
            this.Commit(false, false);
        }

        private void OnMenuCut(object sender, EventArgs e)
        {
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (this.KeyboardService != null)
            {
                this.KeyboardService.SelectedDesignerControl = this.controlHost;
            }
            this.SelectionService.SetSelectedComponents(null, SelectionTypes.Replace);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (this.SelectionService != null)
            {
                if (((this.SelectionService.PrimarySelection is ToolStripItem) && (this.renderer != null)) && (this.renderer.State != 6))
                {
                    this.renderer.State = 0;
                }
                if ((this.KeyboardService != null) && (this.KeyboardService.SelectedDesignerControl == this.controlHost))
                {
                    this.renderer.State = 1;
                }
                this._miniToolStrip.Invalidate();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            this.renderer.State = 0;
            if (this.renderer != null)
            {
                if (this.addItemButton != null)
                {
                    if (this.addItemButton.ButtonBounds.Contains(e.Location))
                    {
                        this.renderer.State = 2;
                    }
                    else if (this.addItemButton.DropDownButtonBounds.Contains(e.Location))
                    {
                        this.renderer.State = 3;
                    }
                }
                this._miniToolStrip.Invalidate();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && (this.BehaviorService != null))
            {
                Point p = this.BehaviorService.ControlToAdornerWindow(this._miniToolStrip);
                p = this.BehaviorService.AdornerWindowPointToScreen(p);
                p.Offset(e.Location);
                this.DesignerContextMenu.Show(p);
            }
        }

        private void OnRightToLeftChanged(object sender, EventArgs e)
        {
            ToolStrip strip = sender as ToolStrip;
            if (strip != null)
            {
                this._miniToolStrip.RightToLeft = strip.RightToLeft;
            }
            else
            {
                ToolStripDropDownItem item = sender as ToolStripDropDownItem;
                this._miniToolStrip.RightToLeft = item.RightToLeft;
            }
        }

        public bool OverrideInvoke(MenuCommand cmd)
        {
            for (int i = 0; i < this.commands.Length; i++)
            {
                if (this.commands[i].CommandID.Equals(cmd.CommandID) && (((cmd.CommandID == StandardCommands.Delete) || (cmd.CommandID == StandardCommands.Cut)) || (cmd.CommandID == StandardCommands.Copy)))
                {
                    this.commands[i].Invoke();
                    return true;
                }
            }
            return false;
        }

        public bool OverrideStatus(MenuCommand cmd)
        {
            for (int i = 0; i < this.commands.Length; i++)
            {
                if (this.commands[i].CommandID.Equals(cmd.CommandID))
                {
                    cmd.Enabled = false;
                    return true;
                }
            }
            return false;
        }

        internal void RollBack()
        {
            if ((this._miniToolStrip != null) && this.inSituMode)
            {
                this.CommitEditor(false, false, false);
            }
        }

        private void SetUpMenuTemplateNode(ToolStripTemplateNode owner, string text, Image image, IComponent currentItem)
        {
            this.centerLabel = new ToolStripLabel();
            this.centerLabel.Text = text;
            this.centerLabel.AutoSize = false;
            this.centerLabel.IsLink = false;
            this.centerLabel.Margin = new Padding(1);
            if (currentItem is ToolStripDropDownItem)
            {
                this.centerLabel.Margin = new Padding(1, 2, 1, 3);
            }
            this.centerLabel.Padding = new Padding(0, 1, 0, 0);
            this.centerLabel.Name = "centerLabel";
            this.centerLabel.Size = this._miniToolStrip.DisplayRectangle.Size - this.centerLabel.Margin.Size;
            this.centerLabel.ToolTipText = System.Design.SR.GetString("ToolStripDesignerTemplateNodeLabelToolTip");
            this.centerLabel.MouseUp += new MouseEventHandler(this.CenterLabelClick);
            this.centerLabel.MouseEnter += new EventHandler(this.CenterLabelMouseEnter);
            this.centerLabel.MouseMove += new MouseEventHandler(this.CenterLabelMouseMove);
            this.centerLabel.MouseLeave += new EventHandler(this.CenterLabelMouseLeave);
            this._miniToolStrip.Items.AddRange(new ToolStripItem[] { this.centerLabel });
        }

        private void SetupNewEditNode(ToolStripTemplateNode owner, string text, Image image, IComponent currentItem)
        {
            this.renderer = new MiniToolStripRenderer(owner);
            this._miniToolStrip = new TransparentToolStrip(owner);
            ToolStrip strip = currentItem as ToolStrip;
            if (strip != null)
            {
                this._miniToolStrip.RightToLeft = strip.RightToLeft;
                strip.RightToLeftChanged += new EventHandler(this.OnRightToLeftChanged);
                this._miniToolStrip.Site = strip.Site;
            }
            ToolStripDropDownItem item = currentItem as ToolStripDropDownItem;
            if (item != null)
            {
                this._miniToolStrip.RightToLeft = item.RightToLeft;
                item.RightToLeftChanged += new EventHandler(this.OnRightToLeftChanged);
            }
            this._miniToolStrip.SuspendLayout();
            this._miniToolStrip.CanOverflow = false;
            this._miniToolStrip.Cursor = Cursors.Default;
            this._miniToolStrip.Dock = DockStyle.None;
            this._miniToolStrip.GripStyle = ToolStripGripStyle.Hidden;
            this._miniToolStrip.Name = "miniToolStrip";
            this._miniToolStrip.TabIndex = 0;
            this._miniToolStrip.Text = "miniToolStrip";
            this._miniToolStrip.Visible = true;
            this._miniToolStrip.Renderer = this.renderer;
            if ((currentItem is MenuStrip) || (currentItem is ToolStripDropDownItem))
            {
                this.SetUpMenuTemplateNode(owner, text, image, currentItem);
            }
            else
            {
                this.SetUpToolTemplateNode(owner, text, image, currentItem);
            }
            this._miniToolStrip.MouseLeave += new EventHandler(this.OnMouseLeave);
            this._miniToolStrip.ResumeLayout();
        }

        private void SetUpToolTemplateNode(ToolStripTemplateNode owner, string text, Image image, IComponent component)
        {
            this.addItemButton = new ToolStripSplitButton();
            this.addItemButton.AutoSize = false;
            this.addItemButton.Margin = new Padding(1);
            this.addItemButton.Size = this._miniToolStrip.DisplayRectangle.Size - this.addItemButton.Margin.Size;
            this.addItemButton.DropDownButtonWidth = 11;
            this.addItemButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
            if (component is StatusStrip)
            {
                this.addItemButton.ToolTipText = System.Design.SR.GetString("ToolStripDesignerTemplateNodeSplitButtonStatusStripToolTip");
            }
            else
            {
                this.addItemButton.ToolTipText = System.Design.SR.GetString("ToolStripDesignerTemplateNodeSplitButtonToolTip");
            }
            this.addItemButton.MouseDown += new MouseEventHandler(this.OnMouseDown);
            this.addItemButton.MouseMove += new MouseEventHandler(this.OnMouseMove);
            this.addItemButton.MouseUp += new MouseEventHandler(this.OnMouseUp);
            this.addItemButton.DropDownOpened += new EventHandler(this.OnAddItemButtonDropDownOpened);
            this.contextMenu = ToolStripDesignerUtils.GetNewItemDropDown(component, null, new EventHandler(this.AddNewItemClick), false, component.Site);
            this.contextMenu.Text = "ItemSelectionMenu";
            this.contextMenu.Closed += new ToolStripDropDownClosedEventHandler(this.OnContextMenuClosed);
            this.contextMenu.Opened += new EventHandler(this.OnContextMenuOpened);
            this.addItemButton.DropDown = this.contextMenu;
            try
            {
                if (this.addItemButton.DropDownItems.Count > 0)
                {
                    ItemTypeToolStripMenuItem item = (ItemTypeToolStripMenuItem) this.addItemButton.DropDownItems[0];
                    this.addItemButton.ImageTransparentColor = Color.Lime;
                    this.addItemButton.Image = new Bitmap(typeof(ToolStripTemplateNode), "ToolStripTemplateNode.bmp");
                    this.addItemButton.DefaultItem = item;
                }
            }
            catch (Exception exception)
            {
                if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                {
                    throw;
                }
            }
            this._miniToolStrip.Items.AddRange(new ToolStripItem[] { this.addItemButton });
        }

        internal void SetWidth(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                this._miniToolStrip.Width = this.centerLabel.Width + 2;
            }
            else
            {
                this.centerLabel.Text = text;
            }
        }

        internal void ShowContextMenu(Point pt)
        {
            this.DesignerContextMenu.Show(pt);
        }

        internal void ShowDropDownMenu()
        {
            if (this.addItemButton != null)
            {
                this.addItemButton.ShowDropDown();
            }
            else if (this.BehaviorService != null)
            {
                Point p = this.BehaviorService.ControlToAdornerWindow(this._miniToolStrip);
                p = this.BehaviorService.AdornerWindowPointToScreen(p);
                Rectangle rectangle = new Rectangle(p, this._miniToolStrip.Size);
                if (this.contextMenu == null)
                {
                    this.contextMenu = ToolStripDesignerUtils.GetNewItemDropDown(this.component, null, new EventHandler(this.AddNewItemClick), false, this.component.Site);
                    this.contextMenu.Closed += new ToolStripDropDownClosedEventHandler(this.OnContextMenuClosed);
                    this.contextMenu.Opened += new EventHandler(this.OnContextMenuOpened);
                    this.contextMenu.Text = "ItemSelectionMenu";
                }
                ToolStrip component = this.component as ToolStrip;
                if (component != null)
                {
                    this.contextMenu.RightToLeft = component.RightToLeft;
                }
                else
                {
                    ToolStripDropDownItem item = this.component as ToolStripDropDownItem;
                    if (item != null)
                    {
                        this.contextMenu.RightToLeft = item.RightToLeft;
                    }
                }
                this.contextMenu.Show(rectangle.X, rectangle.Y + rectangle.Height);
                this.contextMenu.Focus();
                if (this.renderer != null)
                {
                    this.renderer.State = 6;
                    this._miniToolStrip.Invalidate();
                }
            }
        }

        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                if (this.active != value)
                {
                    this.active = value;
                    if (this.KeyboardService != null)
                    {
                        this.KeyboardService.TemplateNodeActive = value;
                    }
                    if (this.active)
                    {
                        this.OnActivated(new EventArgs());
                        if (this.KeyboardService != null)
                        {
                            this.KeyboardService.ActiveTemplateNode = this;
                        }
                        IMenuCommandService service = (IMenuCommandService) this.component.Site.GetService(typeof(IMenuCommandService));
                        if (service != null)
                        {
                            this.oldUndoCommand = service.FindCommand(StandardCommands.Undo);
                            if (this.oldUndoCommand != null)
                            {
                                service.RemoveCommand(this.oldUndoCommand);
                            }
                            this.oldRedoCommand = service.FindCommand(StandardCommands.Redo);
                            if (this.oldRedoCommand != null)
                            {
                                service.RemoveCommand(this.oldRedoCommand);
                            }
                            for (int i = 0; i < this.addCommands.Length; i++)
                            {
                                this.addCommands[i].Enabled = false;
                                service.AddCommand(this.addCommands[i]);
                            }
                        }
                        IEventHandlerService service2 = (IEventHandlerService) this.component.Site.GetService(typeof(IEventHandlerService));
                        if (service2 != null)
                        {
                            service2.PushHandler(this);
                        }
                    }
                    else
                    {
                        this.OnDeactivated(new EventArgs());
                        if (this.KeyboardService != null)
                        {
                            this.KeyboardService.ActiveTemplateNode = null;
                        }
                        IMenuCommandService service3 = (IMenuCommandService) this.component.Site.GetService(typeof(IMenuCommandService));
                        if (service3 != null)
                        {
                            for (int j = 0; j < this.addCommands.Length; j++)
                            {
                                service3.RemoveCommand(this.addCommands[j]);
                            }
                        }
                        if (this.oldUndoCommand != null)
                        {
                            service3.AddCommand(this.oldUndoCommand);
                        }
                        if (this.oldRedoCommand != null)
                        {
                            service3.AddCommand(this.oldRedoCommand);
                        }
                        IEventHandlerService service4 = (IEventHandlerService) this.component.Site.GetService(typeof(IEventHandlerService));
                        if (service4 != null)
                        {
                            service4.PopHandler(this);
                        }
                    }
                }
            }
        }

        public ToolStripItem ActiveItem
        {
            get
            {
                return this.activeItem;
            }
            set
            {
                this.activeItem = value;
            }
        }

        private System.Windows.Forms.Design.Behavior.BehaviorService BehaviorService
        {
            get
            {
                if (this.behaviorService == null)
                {
                    this.behaviorService = (System.Windows.Forms.Design.Behavior.BehaviorService) this.component.Site.GetService(typeof(System.Windows.Forms.Design.Behavior.BehaviorService));
                }
                return this.behaviorService;
            }
        }

        public Rectangle Bounds
        {
            get
            {
                return this.boundingRect;
            }
            set
            {
                this.boundingRect = value;
            }
        }

        public DesignerToolStripControlHost ControlHost
        {
            get
            {
                return this.controlHost;
            }
            set
            {
                this.controlHost = value;
            }
        }

        private ContextMenuStrip DesignerContextMenu
        {
            get
            {
                BaseContextMenuStrip strip = new BaseContextMenuStrip(this.component.Site, this.controlHost) {
                    Populated = false
                };
                strip.GroupOrdering.Clear();
                strip.GroupOrdering.AddRange(new string[] { "Code", "Custom", "Selection", "Edit" });
                strip.Text = "CustomContextMenu";
                TemplateNodeCustomMenuItemCollection items = new TemplateNodeCustomMenuItemCollection(this.component.Site, this.controlHost);
                foreach (ToolStripItem item in items)
                {
                    strip.Groups["Custom"].Items.Add(item);
                }
                return strip;
            }
        }

        internal TextBox EditBox
        {
            get
            {
                if (this.centerTextBox == null)
                {
                    return null;
                }
                return (TextBox) this.centerTextBox.Control;
            }
        }

        public ToolStrip EditorToolStrip
        {
            get
            {
                return this._miniToolStrip;
            }
        }

        public Rectangle HotRegion
        {
            get
            {
                return this.hotRegion;
            }
            set
            {
                this.hotRegion = value;
            }
        }

        public bool IMEModeSet
        {
            get
            {
                return this.imeModeSet;
            }
            set
            {
                this.imeModeSet = value;
            }
        }

        internal bool IsSystemContextMenuDisplayed
        {
            get
            {
                return this.isSystemContextMenuDisplayed;
            }
            set
            {
                this.isSystemContextMenuDisplayed = value;
            }
        }

        private ToolStripKeyboardHandlingService KeyboardService
        {
            get
            {
                if (this.toolStripKeyBoardService == null)
                {
                    this.toolStripKeyBoardService = (ToolStripKeyboardHandlingService) this.component.Site.GetService(typeof(ToolStripKeyboardHandlingService));
                }
                return this.toolStripKeyBoardService;
            }
        }

        private ISelectionService SelectionService
        {
            get
            {
                if (this.selectionService == null)
                {
                    this.selectionService = (ISelectionService) this.component.Site.GetService(typeof(ISelectionService));
                }
                return this.selectionService;
            }
        }

        public System.Type ToolStripItemType
        {
            get
            {
                return this.itemType;
            }
            set
            {
                this.itemType = value;
            }
        }

        public class MiniToolStripRenderer : ToolStripSystemRenderer
        {
            private Color defaultBorderColor;
            private Color dropDownMouseDownColor;
            private Color dropDownMouseOverColor;
            private Rectangle hotRegion = Rectangle.Empty;
            private ToolStripTemplateNode owner;
            private Color selectedBorderColor;
            private int state;
            private Color toolStripBorderColor;

            public MiniToolStripRenderer(ToolStripTemplateNode owner)
            {
                this.owner = owner;
                this.selectedBorderColor = Color.FromArgb(0x2e, 0x6a, 0xc5);
                this.defaultBorderColor = Color.FromArgb(0xab, 0xab, 0xab);
                this.dropDownMouseOverColor = Color.FromArgb(0xc1, 210, 0xee);
                this.dropDownMouseDownColor = Color.FromArgb(0x98, 0xb5, 0xe2);
                this.toolStripBorderColor = Color.White;
            }

            private void DrawArrow(Graphics g, Rectangle bounds)
            {
                bounds.Width--;
                base.DrawArrow(new ToolStripArrowRenderEventArgs(g, null, bounds, SystemColors.ControlText, ArrowDirection.Down));
            }

            private void DrawDropDown(Graphics g, Rectangle bounds, int state)
            {
                switch (state)
                {
                    case 4:
                    {
                        using (LinearGradientBrush brush = new LinearGradientBrush(bounds, Color.White, this.defaultBorderColor, LinearGradientMode.Vertical))
                        {
                            g.FillRectangle(brush, bounds);
                            break;
                        }
                    }
                    case 5:
                    {
                        using (SolidBrush brush2 = new SolidBrush(this.dropDownMouseOverColor))
                        {
                            g.FillRectangle(brush2, this.hotRegion);
                            break;
                        }
                    }
                    case 6:
                        using (SolidBrush brush3 = new SolidBrush(this.dropDownMouseDownColor))
                        {
                            g.FillRectangle(brush3, this.hotRegion);
                        }
                        break;
                }
                this.DrawArrow(g, bounds);
            }

            protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
            {
                base.OnRenderLabelBackground(e);
                ToolStripItem item = e.Item;
                Graphics g = e.Graphics;
                Rectangle rectangle = new Rectangle(Point.Empty, item.Size);
                Rectangle rect = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                Pen pen = new Pen(this.defaultBorderColor);
                if (this.state == 1)
                {
                    using (SolidBrush brush = new SolidBrush(this.toolStripBorderColor))
                    {
                        g.FillRectangle(brush, rect);
                    }
                    if (this.owner.EditorToolStrip.RightToLeft == RightToLeft.Yes)
                    {
                        this.hotRegion = new Rectangle(rectangle.Left + 2, rectangle.Top + 2, 9, rectangle.Bottom - 4);
                    }
                    else
                    {
                        this.hotRegion = new Rectangle(rectangle.Right - 11, rectangle.Top + 2, 9, rectangle.Bottom - 4);
                    }
                    this.owner.HotRegion = this.hotRegion;
                    pen.Color = Color.Black;
                    item.ForeColor = this.defaultBorderColor;
                    g.DrawRectangle(pen, rect);
                }
                if (this.state == 4)
                {
                    if (this.owner.EditorToolStrip.RightToLeft == RightToLeft.Yes)
                    {
                        this.hotRegion = new Rectangle(rectangle.Left + 2, rectangle.Top + 2, 9, rectangle.Bottom - 4);
                    }
                    else
                    {
                        this.hotRegion = new Rectangle(rectangle.Right - 11, rectangle.Top + 2, 9, rectangle.Bottom - 4);
                    }
                    this.owner.HotRegion = this.hotRegion;
                    g.Clear(this.toolStripBorderColor);
                    this.DrawDropDown(g, this.hotRegion, this.state);
                    pen.Color = Color.Black;
                    pen.DashStyle = DashStyle.Dot;
                    g.DrawRectangle(pen, rect);
                }
                if (this.state == 5)
                {
                    g.Clear(this.toolStripBorderColor);
                    this.DrawDropDown(g, this.hotRegion, this.state);
                    pen.Color = Color.Black;
                    pen.DashStyle = DashStyle.Dot;
                    item.ForeColor = this.defaultBorderColor;
                    g.DrawRectangle(pen, rect);
                }
                if (this.state == 6)
                {
                    g.Clear(this.toolStripBorderColor);
                    this.DrawDropDown(g, this.hotRegion, this.state);
                    pen.Color = Color.Black;
                    item.ForeColor = this.defaultBorderColor;
                    g.DrawRectangle(pen, rect);
                }
                if (this.state == 0)
                {
                    g.Clear(this.toolStripBorderColor);
                    g.DrawRectangle(pen, rect);
                    item.ForeColor = this.defaultBorderColor;
                }
                pen.Dispose();
            }

            protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
            {
                Graphics g = e.Graphics;
                ToolStripSplitButton item = e.Item as ToolStripSplitButton;
                if (item != null)
                {
                    Rectangle dropDownButtonBounds = item.DropDownButtonBounds;
                    using (Pen pen = new Pen(this.toolStripBorderColor))
                    {
                        g.DrawLine(pen, dropDownButtonBounds.Left, dropDownButtonBounds.Top + 1, dropDownButtonBounds.Left, dropDownButtonBounds.Bottom - 1);
                    }
                    Rectangle rectangle2 = new Rectangle(Point.Empty, item.Size);
                    Pen pen2 = null;
                    bool flag = false;
                    if (item.DropDownButtonPressed)
                    {
                        this.state = 0;
                        Rectangle rectangle3 = new Rectangle(dropDownButtonBounds.Left + 1, dropDownButtonBounds.Top, dropDownButtonBounds.Right, dropDownButtonBounds.Bottom);
                        using (SolidBrush brush = new SolidBrush(this.dropDownMouseDownColor))
                        {
                            g.FillRectangle(brush, rectangle3);
                        }
                        flag = true;
                    }
                    else if (this.state == 2)
                    {
                        using (SolidBrush brush2 = new SolidBrush(this.dropDownMouseOverColor))
                        {
                            g.FillRectangle(brush2, item.ButtonBounds);
                        }
                        flag = true;
                    }
                    else if (this.state == 3)
                    {
                        Rectangle rectangle4 = new Rectangle(dropDownButtonBounds.Left + 1, dropDownButtonBounds.Top, dropDownButtonBounds.Right, dropDownButtonBounds.Bottom);
                        using (SolidBrush brush3 = new SolidBrush(this.dropDownMouseOverColor))
                        {
                            g.FillRectangle(brush3, rectangle4);
                        }
                        flag = true;
                    }
                    else if (this.state == 1)
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        pen2 = new Pen(this.selectedBorderColor);
                    }
                    else
                    {
                        pen2 = new Pen(this.defaultBorderColor);
                    }
                    Rectangle rect = new Rectangle(rectangle2.X, rectangle2.Y, rectangle2.Width - 1, rectangle2.Height - 1);
                    g.DrawRectangle(pen2, rect);
                    pen2.Dispose();
                    base.DrawArrow(new ToolStripArrowRenderEventArgs(g, item, item.DropDownButtonBounds, SystemColors.ControlText, ArrowDirection.Down));
                }
            }

            protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
            {
                if ((this.owner.component is MenuStrip) || (this.owner.component is ToolStripDropDownItem))
                {
                    e.Graphics.Clear(this.toolStripBorderColor);
                }
                else
                {
                    base.OnRenderToolStripBackground(e);
                }
            }

            protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
            {
                Graphics graphics = e.Graphics;
                Rectangle rectangle = new Rectangle(Point.Empty, e.ToolStrip.Size);
                Pen pen = new Pen(this.toolStripBorderColor);
                Rectangle rect = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width - 1, rectangle.Height - 1);
                graphics.DrawRectangle(pen, rect);
                pen.Dispose();
            }

            public int State
            {
                get
                {
                    return this.state;
                }
                set
                {
                    this.state = value;
                }
            }
        }

        private class TemplateTextBox : TextBox
        {
            private const int IMEMODE = 0xe5;
            private ToolStripTemplateNode owner;
            private ToolStripTemplateNode.TransparentToolStrip parent;

            public TemplateTextBox(ToolStripTemplateNode.TransparentToolStrip parent, ToolStripTemplateNode owner)
            {
                this.parent = parent;
                this.owner = owner;
                this.AutoSize = false;
                this.Multiline = false;
            }

            protected override bool IsInputKey(Keys keyData)
            {
                Keys keys = keyData & Keys.KeyCode;
                if (keys == Keys.Enter)
                {
                    this.owner.Commit(true, false);
                    return true;
                }
                return base.IsInputKey(keyData);
            }

            private bool IsParentWindow(IntPtr hWnd)
            {
                return (hWnd == this.parent.Handle);
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                if (keyData == Keys.ProcessKey)
                {
                    this.owner.IMEModeSet = true;
                }
                else
                {
                    this.owner.IMEModeSet = false;
                    this.owner.ignoreFirstKeyUp = false;
                }
                return base.ProcessDialogKey(keyData);
            }

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case 8:
                    {
                        base.WndProc(ref m);
                        IntPtr wParam = m.WParam;
                        if (this.IsParentWindow(wParam))
                        {
                            break;
                        }
                        this.owner.Commit(false, false);
                        return;
                    }
                    case 0x7b:
                        this.owner.IsSystemContextMenuDisplayed = true;
                        base.WndProc(ref m);
                        this.owner.IsSystemContextMenuDisplayed = false;
                        return;

                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
        }

        public class TransparentToolStrip : ToolStrip
        {
            private IComponent currentItem;
            private ToolStripTemplateNode owner;

            public TransparentToolStrip(ToolStripTemplateNode owner)
            {
                this.owner = owner;
                this.currentItem = owner.component;
                base.TabStop = true;
                base.SetStyle(ControlStyles.Selectable, true);
                this.AutoSize = false;
            }

            private void CommitAndSelectNext(bool forward)
            {
                this.owner.Commit(false, true);
                if (this.owner.KeyboardService != null)
                {
                    this.owner.KeyboardService.ProcessKeySelect(!forward, null);
                }
            }

            [EditorBrowsable(EditorBrowsableState.Advanced)]
            public override Size GetPreferredSize(Size proposedSize)
            {
                if (this.currentItem is ToolStripDropDownItem)
                {
                    return new Size(base.Width, 0x16);
                }
                return new Size(base.Width, 0x13);
            }

            private ToolStripItem GetSelectedItem()
            {
                ToolStripItem item = null;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    if (this.Items[i].Selected)
                    {
                        item = this.Items[i];
                    }
                }
                return item;
            }

            protected override bool ProcessDialogKey(Keys keyData)
            {
                bool flag = false;
                if (this.owner.Active)
                {
                    if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None)
                    {
                        Keys keys = keyData & Keys.KeyCode;
                        switch (keys)
                        {
                            case Keys.Tab:
                            {
                                flag = this.ProcessTabKey((keyData & Keys.Shift) == Keys.None);
                            }
                        }
                    }
                    if (flag)
                    {
                        return flag;
                    }
                }
                return base.ProcessDialogKey(keyData);
            }

            private bool ProcessTabKey(bool forward)
            {
                if (this.GetSelectedItem() is ToolStripControlHost)
                {
                    this.CommitAndSelectNext(forward);
                    return true;
                }
                return false;
            }

            [EditorBrowsable(EditorBrowsableState.Advanced)]
            protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
            {
                if (this.currentItem is ToolStripDropDownItem)
                {
                    base.SetBoundsCore(x, y, 0x5c, 0x16, specified);
                }
                else if (this.currentItem is MenuStrip)
                {
                    base.SetBoundsCore(x, y, 0x5c, 0x13, specified);
                }
                else
                {
                    base.SetBoundsCore(x, y, 0x1f, 0x13, specified);
                }
            }

            public ToolStripTemplateNode TemplateNode
            {
                get
                {
                    return this.owner;
                }
            }
        }
    }
}

