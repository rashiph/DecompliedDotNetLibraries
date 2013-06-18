namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;

    internal class ToolStripCollectionEditor : CollectionEditor
    {
        public ToolStripCollectionEditor() : base(typeof(ToolStripItemCollection))
        {
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            return new ToolStripItemEditorForm(this);
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ToolStripDesigner designer = null;
            object obj3;
            if (provider != null)
            {
                ISelectionService service = (ISelectionService) provider.GetService(typeof(ISelectionService));
                if (service != null)
                {
                    object primarySelection = service.PrimarySelection;
                    if (primarySelection is ToolStripDropDownItem)
                    {
                        primarySelection = ((ToolStripDropDownItem) primarySelection).Owner;
                    }
                    if (primarySelection is ToolStrip)
                    {
                        IDesignerHost host = (IDesignerHost) provider.GetService(typeof(IDesignerHost));
                        if (host != null)
                        {
                            designer = host.GetDesigner((IComponent) primarySelection) as ToolStripDesigner;
                        }
                    }
                }
            }
            try
            {
                if (designer != null)
                {
                    designer.EditingCollection = true;
                }
                obj3 = base.EditValue(context, provider, value);
            }
            finally
            {
                if (designer != null)
                {
                    designer.EditingCollection = false;
                }
            }
            return obj3;
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.ComponentModel.ToolStripCollectionEditor";
            }
        }

        protected class ToolStripItemEditorForm : CollectionEditor.CollectionForm
        {
            private IComponentChangeService _componentChangeSvc;
            private EditorItemCollection _itemList;
            private string _originalText;
            private ToolStripItemCollection _targetToolStripCollection;
            private TableLayoutPanel addTableLayoutPanel;
            private Button btnAddNew;
            private Button btnCancel;
            private Button btnMoveDown;
            private Button btnMoveUp;
            private Button btnOK;
            private Button btnRemove;
            private int customItemIndex;
            private ToolStripCollectionEditor editor;
            private const int GdiPlusFudge = 5;
            private const int ICON_HEIGHT = 0x10;
            private const int IMAGE_PADDING = 1;
            private const int INDENT_SPACING = 4;
            private Label lblItems;
            private Label lblMembers;
            private CollectionEditor.FilterListBox listBoxItems;
            private ComboBox newItemTypes;
            private TableLayoutPanel okCancelTableLayoutPanel;
            private Label selectedItemName;
            private VsPropertyGrid selectedItemProps;
            private const int SEPARATOR_HEIGHT = 4;
            private TableLayoutPanel tableLayoutPanel;
            private const int TEXT_IMAGE_SPACING = 6;
            private ToolStripCustomTypeDescriptor toolStripCustomTypeDescriptor;

            internal ToolStripItemEditorForm(CollectionEditor parent) : base(parent)
            {
                this.customItemIndex = -1;
                this.editor = (ToolStripCollectionEditor) parent;
                this.InitializeComponent();
                base.ActiveControl = this.listBoxItems;
                this._originalText = this.Text;
                base.SetStyle(ControlStyles.ResizeRedraw, true);
            }

            private void AddItem(ToolStripItem newItem, int index)
            {
                if (index == -1)
                {
                    this._itemList.Add(newItem);
                }
                else
                {
                    if ((index < 0) || (index >= this._itemList.Count))
                    {
                        throw new IndexOutOfRangeException();
                    }
                    this._itemList.Insert(index, newItem);
                }
                ToolStrip strip = (base.Context != null) ? ToolStripFromObject(base.Context.Instance) : null;
                if (strip != null)
                {
                    strip.Items.Add(newItem);
                }
                this.listBoxItems.ClearSelected();
                this.listBoxItems.SelectedItem = newItem;
            }

            private void InitializeComponent()
            {
                ComponentResourceManager manager = new ComponentResourceManager(typeof(ToolStripCollectionEditor.ToolStripItemEditorForm));
                this.btnCancel = new Button();
                this.btnOK = new Button();
                this.tableLayoutPanel = new TableLayoutPanel();
                this.addTableLayoutPanel = new TableLayoutPanel();
                this.btnAddNew = new Button();
                this.newItemTypes = new ImageComboBox();
                this.okCancelTableLayoutPanel = new TableLayoutPanel();
                this.lblItems = new Label();
                this.selectedItemName = new Label();
                this.selectedItemProps = new VsPropertyGrid(base.Context);
                this.lblMembers = new Label();
                this.listBoxItems = new CollectionEditor.FilterListBox();
                this.btnMoveUp = new Button();
                this.btnMoveDown = new Button();
                this.btnRemove = new Button();
                this.tableLayoutPanel.SuspendLayout();
                this.addTableLayoutPanel.SuspendLayout();
                this.okCancelTableLayoutPanel.SuspendLayout();
                base.SuspendLayout();
                manager.ApplyResources(this.btnCancel, "btnCancel");
                this.btnCancel.DialogResult = DialogResult.Cancel;
                this.btnCancel.Margin = new Padding(3, 0, 0, 0);
                this.btnCancel.Name = "btnCancel";
                manager.ApplyResources(this.btnOK, "btnOK");
                this.btnOK.Margin = new Padding(0, 0, 3, 0);
                this.btnOK.Name = "btnOK";
                manager.ApplyResources(this.tableLayoutPanel, "tableLayoutPanel");
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 274f));
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                this.tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                this.tableLayoutPanel.Controls.Add(this.addTableLayoutPanel, 0, 1);
                this.tableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 0, 6);
                this.tableLayoutPanel.Controls.Add(this.lblItems, 0, 0);
                this.tableLayoutPanel.Controls.Add(this.selectedItemName, 2, 0);
                this.tableLayoutPanel.Controls.Add(this.selectedItemProps, 2, 1);
                this.tableLayoutPanel.Controls.Add(this.lblMembers, 0, 2);
                this.tableLayoutPanel.Controls.Add(this.listBoxItems, 0, 3);
                this.tableLayoutPanel.Controls.Add(this.btnMoveUp, 1, 3);
                this.tableLayoutPanel.Controls.Add(this.btnMoveDown, 1, 4);
                this.tableLayoutPanel.Controls.Add(this.btnRemove, 1, 5);
                this.tableLayoutPanel.Name = "tableLayoutPanel";
                this.tableLayoutPanel.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel.RowStyles.Add(new RowStyle());
                this.tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
                this.tableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.addTableLayoutPanel, "addTableLayoutPanel");
                this.addTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                this.addTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                this.addTableLayoutPanel.Controls.Add(this.btnAddNew, 1, 0);
                this.addTableLayoutPanel.Controls.Add(this.newItemTypes, 0, 0);
                this.addTableLayoutPanel.Margin = new Padding(0, 3, 3, 3);
                this.addTableLayoutPanel.Name = "addTableLayoutPanel";
                this.addTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.btnAddNew, "btnAddNew");
                this.btnAddNew.Margin = new Padding(3, 0, 0, 0);
                this.btnAddNew.Name = "btnAddNew";
                manager.ApplyResources(this.newItemTypes, "newItemTypes");
                this.newItemTypes.DropDownStyle = ComboBoxStyle.DropDownList;
                this.newItemTypes.FormattingEnabled = true;
                this.newItemTypes.Margin = new Padding(0, 0, 3, 0);
                this.newItemTypes.Name = "newItemTypes";
                this.newItemTypes.DrawMode = DrawMode.OwnerDrawVariable;
                manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
                this.tableLayoutPanel.SetColumnSpan(this.okCancelTableLayoutPanel, 3);
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
                this.okCancelTableLayoutPanel.Controls.Add(this.btnOK, 0, 0);
                this.okCancelTableLayoutPanel.Controls.Add(this.btnCancel, 1, 0);
                this.okCancelTableLayoutPanel.Margin = new Padding(3, 6, 0, 0);
                this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
                this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle());
                manager.ApplyResources(this.lblItems, "lblItems");
                this.lblItems.Margin = new Padding(0, 3, 3, 0);
                this.lblItems.Name = "lblItems";
                manager.ApplyResources(this.selectedItemName, "selectedItemName");
                this.selectedItemName.Margin = new Padding(3, 3, 3, 0);
                this.selectedItemName.Name = "selectedItemName";
                this.selectedItemProps.CommandsVisibleIfAvailable = false;
                manager.ApplyResources(this.selectedItemProps, "selectedItemProps");
                this.selectedItemProps.Margin = new Padding(3, 3, 0, 3);
                this.selectedItemProps.Name = "selectedItemProps";
                this.tableLayoutPanel.SetRowSpan(this.selectedItemProps, 5);
                manager.ApplyResources(this.lblMembers, "lblMembers");
                this.lblMembers.Margin = new Padding(0, 3, 3, 0);
                this.lblMembers.Name = "lblMembers";
                manager.ApplyResources(this.listBoxItems, "listBoxItems");
                this.listBoxItems.DrawMode = DrawMode.OwnerDrawVariable;
                this.listBoxItems.FormattingEnabled = true;
                this.listBoxItems.Margin = new Padding(0, 3, 3, 3);
                this.listBoxItems.Name = "listBoxItems";
                this.tableLayoutPanel.SetRowSpan(this.listBoxItems, 3);
                this.listBoxItems.SelectionMode = SelectionMode.MultiExtended;
                manager.ApplyResources(this.btnMoveUp, "btnMoveUp");
                this.btnMoveUp.Margin = new Padding(3, 3, 0x12, 0);
                this.btnMoveUp.Name = "btnMoveUp";
                manager.ApplyResources(this.btnMoveDown, "btnMoveDown");
                this.btnMoveDown.Margin = new Padding(3, 1, 0x12, 3);
                this.btnMoveDown.Name = "btnMoveDown";
                manager.ApplyResources(this.btnRemove, "btnRemove");
                this.btnRemove.Margin = new Padding(3, 3, 0x12, 3);
                this.btnRemove.Name = "btnRemove";
                base.AutoScaleMode = AutoScaleMode.Font;
                base.AcceptButton = this.btnOK;
                manager.ApplyResources(this, "$this");
                base.CancelButton = this.btnCancel;
                base.Controls.Add(this.tableLayoutPanel);
                base.HelpButton = true;
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                base.Name = "ToolStripCollectionEditor";
                base.Padding = new Padding(9);
                base.ShowIcon = false;
                base.ShowInTaskbar = false;
                base.SizeGripStyle = SizeGripStyle.Show;
                this.tableLayoutPanel.ResumeLayout(false);
                this.tableLayoutPanel.PerformLayout();
                this.addTableLayoutPanel.ResumeLayout(false);
                this.addTableLayoutPanel.PerformLayout();
                this.okCancelTableLayoutPanel.ResumeLayout(false);
                this.okCancelTableLayoutPanel.PerformLayout();
                base.ResumeLayout(false);
                base.HelpButtonClicked += new CancelEventHandler(this.ToolStripCollectionEditor_HelpButtonClicked);
                this.newItemTypes.DropDown += new EventHandler(this.OnnewItemTypes_DropDown);
                this.newItemTypes.HandleCreated += new EventHandler(this.OnComboHandleCreated);
                this.newItemTypes.SelectedIndexChanged += new EventHandler(this.OnnewItemTypes_SelectedIndexChanged);
                this.btnAddNew.Click += new EventHandler(this.OnnewItemTypes_SelectionChangeCommitted);
                this.btnMoveUp.Click += new EventHandler(this.OnbtnMoveUp_Click);
                this.btnMoveDown.Click += new EventHandler(this.OnbtnMoveDown_Click);
                this.btnRemove.Click += new EventHandler(this.OnbtnRemove_Click);
                this.btnOK.Click += new EventHandler(this.OnbtnOK_Click);
                this.selectedItemName.Paint += new PaintEventHandler(this.OnselectedItemName_Paint);
                this.listBoxItems.SelectedIndexChanged += new EventHandler(this.OnlistBoxItems_SelectedIndexChanged);
                this.listBoxItems.DrawItem += new DrawItemEventHandler(this.OnlistBoxItems_DrawItem);
                this.listBoxItems.MeasureItem += new MeasureItemEventHandler(this.OnlistBoxItems_MeasureItem);
                this.selectedItemProps.PropertyValueChanged += new PropertyValueChangedEventHandler(this.PropertyGrid_propertyValueChanged);
                base.Load += new EventHandler(this.OnFormLoad);
            }

            private void MoveItem(int fromIndex, int toIndex)
            {
                this._itemList.Move(fromIndex, toIndex);
            }

            private void OnbtnMoveDown_Click(object sender, EventArgs e)
            {
                ToolStripItem selectedItem = (ToolStripItem) this.listBoxItems.SelectedItem;
                int index = this.listBoxItems.Items.IndexOf(selectedItem);
                this.MoveItem(index, ++index);
                this.listBoxItems.SelectedIndex = index;
            }

            private void OnbtnMoveUp_Click(object sender, EventArgs e)
            {
                ToolStripItem selectedItem = (ToolStripItem) this.listBoxItems.SelectedItem;
                int index = this.listBoxItems.Items.IndexOf(selectedItem);
                if (index > 1)
                {
                    this.MoveItem(index, --index);
                    this.listBoxItems.SelectedIndex = index;
                }
            }

            private void OnbtnOK_Click(object sender, EventArgs e)
            {
                base.DialogResult = DialogResult.OK;
            }

            private void OnbtnRemove_Click(object sender, EventArgs e)
            {
                ToolStripItem[] destination = new ToolStripItem[this.listBoxItems.SelectedItems.Count];
                this.listBoxItems.SelectedItems.CopyTo(destination, 0);
                for (int i = 0; i < destination.Length; i++)
                {
                    this.RemoveItem(destination[i]);
                }
            }

            private void OnComboHandleCreated(object sender, EventArgs e)
            {
                this.newItemTypes.HandleCreated -= new EventHandler(this.OnComboHandleCreated);
                this.newItemTypes.MeasureItem += new MeasureItemEventHandler(this.OnlistBoxItems_MeasureItem);
                this.newItemTypes.DrawItem += new DrawItemEventHandler(this.OnlistBoxItems_DrawItem);
            }

            private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
            {
                if (((e.Component is ToolStripItem) && (e.Member is PropertyDescriptor)) && (e.Member.Name == "Name"))
                {
                    this.lblItems.Invalidate();
                }
            }

            protected override void OnEditValueChanged()
            {
                this.selectedItemProps.SelectedObjects = null;
                this.Collection = (ToolStripItemCollection) base.EditValue;
            }

            private void OnFormLoad(object sender, EventArgs e)
            {
                this.newItemTypes.ItemHeight = Math.Max(0x10, this.Font.Height);
                Component instance = base.Context.Instance as Component;
                if (instance != null)
                {
                    System.Type[] standardItemTypes = ToolStripDesignerUtils.GetStandardItemTypes(instance);
                    this.newItemTypes.Items.Clear();
                    foreach (System.Type type in standardItemTypes)
                    {
                        this.newItemTypes.Items.Add(new TypeListItem(type));
                    }
                    this.newItemTypes.SelectedIndex = 0;
                    this.customItemIndex = -1;
                    standardItemTypes = ToolStripDesignerUtils.GetCustomItemTypes(instance, instance.Site);
                    if (standardItemTypes.Length > 0)
                    {
                        this.customItemIndex = this.newItemTypes.Items.Count;
                        foreach (System.Type type2 in standardItemTypes)
                        {
                            this.newItemTypes.Items.Add(new TypeListItem(type2));
                        }
                    }
                    if (this.listBoxItems.Items.Count > 0)
                    {
                        this.listBoxItems.SelectedIndex = 0;
                    }
                }
            }

            private void OnlistBoxItems_DrawItem(object sender, DrawItemEventArgs e)
            {
                if (e.Index != -1)
                {
                    System.Type itemType = null;
                    string str = null;
                    bool flag = false;
                    bool flag2 = false;
                    bool flag3 = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
                    if (sender is ListBox)
                    {
                        ListBox box = sender as ListBox;
                        Component component = box.Items[e.Index] as Component;
                        if (component == null)
                        {
                            return;
                        }
                        if (component is ToolStripItem)
                        {
                            flag = true;
                        }
                        itemType = component.GetType();
                        str = (component.Site != null) ? component.Site.Name : itemType.Name;
                    }
                    else
                    {
                        if (!(sender is ComboBox))
                        {
                            return;
                        }
                        flag2 = (e.Index == this.customItemIndex) && !flag3;
                        TypeListItem item = ((ComboBox) sender).Items[e.Index] as TypeListItem;
                        if (item == null)
                        {
                            return;
                        }
                        itemType = item.Type;
                        str = item.ToString();
                    }
                    if (itemType != null)
                    {
                        Color empty = Color.Empty;
                        if (flag2)
                        {
                            e.Graphics.DrawLine(SystemPens.ControlDark, (int) (e.Bounds.X + 2), (int) (e.Bounds.Y + 2), (int) (e.Bounds.Right - 2), (int) (e.Bounds.Y + 2));
                        }
                        Rectangle bounds = e.Bounds;
                        bounds.Size = new Size(0x10, 0x10);
                        int x = flag3 ? 0 : 2;
                        bounds.Offset(x, 1);
                        if (flag2)
                        {
                            bounds.Offset(0, 4);
                        }
                        if (flag)
                        {
                            bounds.X += 20;
                        }
                        if (!flag3)
                        {
                            bounds.Intersect(e.Bounds);
                        }
                        Bitmap toolboxBitmap = ToolStripDesignerUtils.GetToolboxBitmap(itemType);
                        if (toolboxBitmap != null)
                        {
                            if (flag3)
                            {
                                e.Graphics.DrawImage(toolboxBitmap, e.Bounds.X, e.Bounds.Y, 0x10, 0x10);
                            }
                            else
                            {
                                e.Graphics.FillRectangle(SystemBrushes.Window, bounds);
                                e.Graphics.DrawImage(toolboxBitmap, bounds);
                            }
                        }
                        Rectangle rectangle2 = e.Bounds;
                        rectangle2.X = bounds.Right + 6;
                        rectangle2.Y = bounds.Top - 1;
                        if (!flag3)
                        {
                            rectangle2.Y += 2;
                        }
                        rectangle2.Intersect(e.Bounds);
                        Rectangle rect = e.Bounds;
                        rect.X = rectangle2.X - 2;
                        if (flag2)
                        {
                            rect.Y += 4;
                            rect.Height -= 4;
                        }
                        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                        {
                            empty = SystemColors.HighlightText;
                            e.Graphics.FillRectangle(SystemBrushes.Highlight, rect);
                        }
                        else
                        {
                            empty = SystemColors.WindowText;
                            e.Graphics.FillRectangle(SystemBrushes.Window, rect);
                        }
                        if (!string.IsNullOrEmpty(str))
                        {
                            TextFormatFlags flags = TextFormatFlags.Default;
                            TextRenderer.DrawText(e.Graphics, str, this.Font, rectangle2, empty, flags);
                        }
                        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
                        {
                            rect.Width--;
                            ControlPaint.DrawFocusRectangle(e.Graphics, rect, e.ForeColor, e.BackColor);
                        }
                    }
                }
            }

            private void OnlistBoxItems_MeasureItem(object sender, MeasureItemEventArgs e)
            {
                int num = 0;
                if (sender is ComboBox)
                {
                    bool flag = e.Index == this.customItemIndex;
                    if ((e.Index >= 0) && flag)
                    {
                        num = 4;
                    }
                }
                Font font = this.Font;
                e.ItemHeight = Math.Max((int) (0x10 + num), (int) (font.Height + num)) + 2;
            }

            private void OnlistBoxItems_SelectedIndexChanged(object sender, EventArgs e)
            {
                object[] destination = new object[this.listBoxItems.SelectedItems.Count];
                if (destination.Length > 0)
                {
                    this.listBoxItems.SelectedItems.CopyTo(destination, 0);
                }
                if ((destination.Length == 1) && (destination[0] is ToolStrip))
                {
                    ToolStrip strip = destination[0] as ToolStrip;
                    if ((strip != null) && (strip.Site != null))
                    {
                        if (this.toolStripCustomTypeDescriptor == null)
                        {
                            this.toolStripCustomTypeDescriptor = new ToolStripCustomTypeDescriptor((ToolStrip) destination[0]);
                        }
                        this.selectedItemProps.SelectedObjects = new object[] { this.toolStripCustomTypeDescriptor };
                    }
                    else
                    {
                        this.selectedItemProps.SelectedObjects = null;
                    }
                }
                else
                {
                    this.selectedItemProps.SelectedObjects = destination;
                }
                this.btnMoveUp.Enabled = (this.listBoxItems.SelectedItems.Count == 1) && (this.listBoxItems.SelectedIndex > 1);
                this.btnMoveDown.Enabled = (this.listBoxItems.SelectedItems.Count == 1) && (this.listBoxItems.SelectedIndex < (this.listBoxItems.Items.Count - 1));
                this.btnRemove.Enabled = destination.Length > 0;
                foreach (object obj2 in this.listBoxItems.SelectedItems)
                {
                    if (obj2 is ToolStrip)
                    {
                        this.btnRemove.Enabled = this.btnMoveUp.Enabled = this.btnMoveDown.Enabled = false;
                        break;
                    }
                }
                this.listBoxItems.Invalidate();
                this.selectedItemName.Invalidate();
            }

            private void OnnewItemTypes_DropDown(object sender, EventArgs e)
            {
                if ((this.newItemTypes.Tag == null) || !((bool) this.newItemTypes.Tag))
                {
                    int itemHeight = this.newItemTypes.ItemHeight;
                    int num2 = 0;
                    using (Graphics graphics = this.newItemTypes.CreateGraphics())
                    {
                        foreach (TypeListItem item in this.newItemTypes.Items)
                        {
                            itemHeight = (int) Math.Max((float) itemHeight, ((this.newItemTypes.ItemHeight + 1) + graphics.MeasureString(item.Type.Name, this.newItemTypes.Font).Width) + 5f);
                            num2 += (this.Font.Height + 4) + 2;
                        }
                    }
                    this.newItemTypes.DropDownWidth = itemHeight;
                    this.newItemTypes.DropDownHeight = num2;
                    this.newItemTypes.Tag = true;
                }
            }

            private void OnnewItemTypes_SelectedIndexChanged(object sender, EventArgs e)
            {
                this.newItemTypes.Invalidate();
            }

            private void OnnewItemTypes_SelectionChangeCommitted(object sender, EventArgs e)
            {
                TypeListItem selectedItem = this.newItemTypes.SelectedItem as TypeListItem;
                if (selectedItem != null)
                {
                    ToolStripItem component = (ToolStripItem) base.CreateInstance(selectedItem.Type);
                    if (((component is ToolStripButton) || (component is ToolStripSplitButton)) || (component is ToolStripDropDownButton))
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
                    this.AddItem(component, -1);
                    this.listBoxItems.Focus();
                }
            }

            private void OnselectedItemName_Paint(object sender, PaintEventArgs e)
            {
                using (Font font = new Font(this.selectedItemName.Font, FontStyle.Bold))
                {
                    Component selectedItem;
                    string str;
                    Label label = sender as Label;
                    Rectangle clientRectangle = label.ClientRectangle;
                    StringFormat format = null;
                    bool flag = label.RightToLeft == RightToLeft.Yes;
                    if (flag)
                    {
                        format = new StringFormat(StringFormatFlags.DirectionRightToLeft);
                    }
                    else
                    {
                        format = new StringFormat();
                    }
                    format.HotkeyPrefix = HotkeyPrefix.Show;
                    switch (this.listBoxItems.SelectedItems.Count)
                    {
                        case 0:
                            e.Graphics.FillRectangle(SystemBrushes.Control, clientRectangle);
                            if (label != null)
                            {
                                label.Text = System.Design.SR.GetString("ToolStripItemCollectionEditorLabelNone");
                            }
                            e.Graphics.DrawString(System.Design.SR.GetString("ToolStripItemCollectionEditorLabelNone"), font, SystemBrushes.WindowText, clientRectangle, format);
                            goto Label_025B;

                        case 1:
                            selectedItem = null;
                            if (!(this.listBoxItems.SelectedItem is ToolStrip))
                            {
                                break;
                            }
                            selectedItem = (ToolStrip) this.listBoxItems.SelectedItem;
                            goto Label_00A8;

                        default:
                            e.Graphics.FillRectangle(SystemBrushes.Control, clientRectangle);
                            if (label != null)
                            {
                                label.Text = System.Design.SR.GetString("ToolStripItemCollectionEditorLabelMultipleItems");
                            }
                            e.Graphics.DrawString(System.Design.SR.GetString("ToolStripItemCollectionEditorLabelMultipleItems"), font, SystemBrushes.WindowText, clientRectangle, format);
                            goto Label_025B;
                    }
                    selectedItem = (ToolStripItem) this.listBoxItems.SelectedItem;
                Label_00A8:
                    str = "&" + selectedItem.GetType().Name;
                    if (selectedItem.Site != null)
                    {
                        e.Graphics.FillRectangle(SystemBrushes.Control, clientRectangle);
                        string name = selectedItem.Site.Name;
                        if (label != null)
                        {
                            label.Text = str + name;
                        }
                        int width = 0;
                        width = (int) e.Graphics.MeasureString(str, font).Width;
                        e.Graphics.DrawString(str, font, SystemBrushes.WindowText, clientRectangle, format);
                        int num2 = (int) e.Graphics.MeasureString(name, this.selectedItemName.Font).Width;
                        Rectangle bounds = new Rectangle(width + 5, 0, clientRectangle.Width - (width + 5), clientRectangle.Height);
                        if (num2 > bounds.Width)
                        {
                            label.AutoEllipsis = true;
                        }
                        else
                        {
                            label.AutoEllipsis = false;
                        }
                        TextFormatFlags endEllipsis = TextFormatFlags.EndEllipsis;
                        if (flag)
                        {
                            endEllipsis |= TextFormatFlags.RightToLeft;
                        }
                        TextRenderer.DrawText(e.Graphics, name, this.selectedItemName.Font, bounds, SystemColors.WindowText, endEllipsis);
                    }
                Label_025B:
                    format.Dispose();
                }
            }

            private void PropertyGrid_propertyValueChanged(object sender, PropertyValueChangedEventArgs e)
            {
                this.listBoxItems.Invalidate();
                this.selectedItemName.Invalidate();
            }

            private void RemoveItem(ToolStripItem item)
            {
                int index;
                try
                {
                    index = this._itemList.IndexOf(item);
                    this._itemList.Remove(item);
                }
                finally
                {
                    item.Dispose();
                }
                if (this._itemList.Count > 0)
                {
                    this.listBoxItems.ClearSelected();
                    index = Math.Max(0, Math.Min(index, this.listBoxItems.Items.Count - 1));
                    this.listBoxItems.SelectedIndex = index;
                }
            }

            private void ToolStripCollectionEditor_HelpButtonClicked(object sender, CancelEventArgs e)
            {
                e.Cancel = true;
                this.editor.ShowHelp();
            }

            internal static ToolStrip ToolStripFromObject(object instance)
            {
                ToolStrip strip = null;
                if (instance == null)
                {
                    return strip;
                }
                if (instance is ToolStripDropDownItem)
                {
                    return ((ToolStripDropDownItem) instance).DropDown;
                }
                return (instance as ToolStrip);
            }

            internal ToolStripItemCollection Collection
            {
                set
                {
                    if (value != this._targetToolStripCollection)
                    {
                        if (this._itemList != null)
                        {
                            this._itemList.Clear();
                        }
                        if (value != null)
                        {
                            if (base.Context != null)
                            {
                                this._itemList = new EditorItemCollection(this, this.listBoxItems.Items, value);
                                ToolStrip strip = ToolStripFromObject(base.Context.Instance);
                                this._itemList.Add(strip);
                                ToolStripItem instance = base.Context.Instance as ToolStripItem;
                                if ((instance != null) && (instance.Site != null))
                                {
                                    this.Text = this._originalText + " (" + instance.Site.Name + "." + base.Context.PropertyDescriptor.Name + ")";
                                }
                                foreach (ToolStripItem item2 in value)
                                {
                                    if (!(item2 is DesignerToolStripControlHost))
                                    {
                                        this._itemList.Add(item2);
                                    }
                                }
                                IComponentChangeService service = (IComponentChangeService) base.Context.GetService(typeof(IComponentChangeService));
                                if (service != null)
                                {
                                    service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                                }
                                this.selectedItemProps.Site = new CollectionEditor.PropertyGridSite(base.Context, this.selectedItemProps);
                            }
                        }
                        else
                        {
                            if (this._componentChangeSvc != null)
                            {
                                this._componentChangeSvc.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                            }
                            this._componentChangeSvc = null;
                            this.selectedItemProps.Site = null;
                        }
                        this._targetToolStripCollection = value;
                    }
                }
            }

            private IComponentChangeService ComponentChangeService
            {
                get
                {
                    if ((this._componentChangeSvc == null) && (base.Context != null))
                    {
                        this._componentChangeSvc = (IComponentChangeService) base.Context.GetService(typeof(IComponentChangeService));
                    }
                    return this._componentChangeSvc;
                }
            }

            private class EditorItemCollection : CollectionBase
            {
                private IList _listBoxList;
                private ToolStripCollectionEditor.ToolStripItemEditorForm _owner;
                private IList _targetCollectionList;

                internal EditorItemCollection(ToolStripCollectionEditor.ToolStripItemEditorForm owner, IList displayList, IList componentList)
                {
                    this._owner = owner;
                    this._listBoxList = displayList;
                    this._targetCollectionList = componentList;
                }

                public void Add(object item)
                {
                    base.List.Add(new EditorItem(item));
                }

                public int IndexOf(ToolStripItem item)
                {
                    for (int i = 0; i < base.List.Count; i++)
                    {
                        EditorItem item2 = (EditorItem) base.List[i];
                        if (item2.Component == item)
                        {
                            return i;
                        }
                    }
                    return -1;
                }

                public void Insert(int index, ToolStripItem item)
                {
                    base.List.Insert(index, new EditorItem(item));
                }

                public void Move(int fromIndex, int toIndex)
                {
                    if (toIndex != fromIndex)
                    {
                        EditorItem item = (EditorItem) base.List[fromIndex];
                        if (item.Host == null)
                        {
                            try
                            {
                                this._owner.Context.OnComponentChanging();
                                this._listBoxList.Remove(item.Component);
                                this._targetCollectionList.Remove(item.Component);
                                base.InnerList.Remove(item);
                                this._listBoxList.Insert(toIndex, item.Component);
                                this._targetCollectionList.Insert(toIndex - 1, item.Component);
                                base.InnerList.Insert(toIndex, item);
                            }
                            finally
                            {
                                this._owner.Context.OnComponentChanged();
                            }
                        }
                    }
                }

                protected override void OnClear()
                {
                    this._listBoxList.Clear();
                    foreach (EditorItem item in base.List)
                    {
                        item.Dispose();
                    }
                    base.OnClear();
                }

                protected override void OnInsertComplete(int index, object value)
                {
                    EditorItem item = (EditorItem) value;
                    if (item.Host != null)
                    {
                        this._listBoxList.Insert(index, item.Host);
                        base.OnInsertComplete(index, value);
                    }
                    else
                    {
                        if (!this._targetCollectionList.Contains(item.Component))
                        {
                            try
                            {
                                this._owner.Context.OnComponentChanging();
                                this._targetCollectionList.Insert(index - 1, item.Component);
                            }
                            finally
                            {
                                this._owner.Context.OnComponentChanged();
                            }
                        }
                        this._listBoxList.Insert(index, item.Component);
                        base.OnInsertComplete(index, value);
                    }
                }

                protected override void OnRemove(int index, object value)
                {
                    EditorItem item = (EditorItem) base.List[index];
                    this._listBoxList.RemoveAt(index);
                    try
                    {
                        this._owner.Context.OnComponentChanging();
                        this._targetCollectionList.RemoveAt(index - 1);
                    }
                    finally
                    {
                        this._owner.Context.OnComponentChanged();
                    }
                    item.Dispose();
                    base.OnRemove(index, value);
                }

                public void Remove(ToolStripItem item)
                {
                    int index = this.IndexOf(item);
                    base.List.RemoveAt(index);
                }

                private class EditorItem
                {
                    public ToolStripItem _component;
                    public ToolStrip _host;

                    internal EditorItem(object componentItem)
                    {
                        if (componentItem is ToolStrip)
                        {
                            this._host = (ToolStrip) componentItem;
                        }
                        else
                        {
                            this._component = (ToolStripItem) componentItem;
                        }
                    }

                    public void Dispose()
                    {
                        GC.SuppressFinalize(this);
                        this._component = null;
                    }

                    public ToolStripItem Component
                    {
                        get
                        {
                            return this._component;
                        }
                    }

                    public ToolStrip Host
                    {
                        get
                        {
                            return this._host;
                        }
                    }
                }
            }

            private class ImageComboBox : ComboBox
            {
                protected override void OnDropDownClosed(EventArgs e)
                {
                    base.OnDropDownClosed(e);
                    base.Invalidate(this.ImageRect);
                }

                protected override void OnSelectedIndexChanged(EventArgs e)
                {
                    base.OnSelectedIndexChanged(e);
                    base.Invalidate(this.ImageRect);
                }

                protected override void WndProc(ref Message m)
                {
                    base.WndProc(ref m);
                    switch (m.Msg)
                    {
                        case 7:
                        case 8:
                            base.Invalidate(this.ImageRect);
                            return;
                    }
                }

                private Rectangle ImageRect
                {
                    get
                    {
                        if (this.RightToLeft == RightToLeft.Yes)
                        {
                            return new Rectangle(4 + SystemInformation.HorizontalScrollBarThumbWidth, 3, 0x10, 0x10);
                        }
                        return new Rectangle(3, 3, 0x10, 0x10);
                    }
                }
            }

            private class TypeListItem
            {
                public readonly System.Type Type;

                public TypeListItem(System.Type t)
                {
                    this.Type = t;
                }

                public override string ToString()
                {
                    return ToolStripDesignerUtils.GetToolboxDescription(this.Type);
                }
            }
        }
    }
}

