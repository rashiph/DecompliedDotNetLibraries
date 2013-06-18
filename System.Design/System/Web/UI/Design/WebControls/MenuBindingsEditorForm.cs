namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal class MenuBindingsEditorForm : DesignerForm
    {
        private System.Windows.Forms.Button _addBindingButton;
        private System.Windows.Forms.Button _applyButton;
        private System.Windows.Forms.Label _bindingsLabel;
        private System.Windows.Forms.ListBox _bindingsListView;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Button _deleteBindingButton;
        private System.Web.UI.WebControls.Menu _menu;
        private System.Windows.Forms.Button _moveBindingDownButton;
        private System.Windows.Forms.Button _moveBindingUpButton;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label _propertiesLabel;
        private PropertyGrid _propertyGrid;
        private IDataSourceSchema _schema;
        private System.Windows.Forms.Label _schemaLabel;
        private System.Windows.Forms.TreeView _schemaTreeView;
        private Container components;

        public MenuBindingsEditorForm(IServiceProvider serviceProvider, System.Web.UI.WebControls.Menu menu, MenuDesigner menuDesigner) : base(serviceProvider)
        {
            this._menu = menu;
            this.InitializeComponent();
            this.InitializeUI();
            foreach (MenuItemBinding binding in this._menu.DataBindings)
            {
                MenuItemBinding clone = (MenuItemBinding) ((ICloneable) binding).Clone();
                menuDesigner.RegisterClone(binding, clone);
                this._bindingsListView.Items.Add(clone);
            }
        }

        private void AddBinding()
        {
            System.Windows.Forms.TreeNode selectedNode = this._schemaTreeView.SelectedNode;
            if (selectedNode != null)
            {
                MenuItemBinding binding = new MenuItemBinding();
                if (selectedNode.Text != this._schemaTreeView.Nodes[0].Text)
                {
                    binding.DataMember = selectedNode.Text;
                    if (((SchemaTreeNode) selectedNode).Duplicate)
                    {
                        binding.Depth = selectedNode.FullPath.Split(new char[] { this._schemaTreeView.PathSeparator[0] }).Length - 1;
                    }
                    ((IDataSourceViewSchemaAccessor) binding).DataSourceViewSchema = ((SchemaTreeNode) selectedNode).Schema;
                    int index = this._bindingsListView.Items.IndexOf(binding);
                    if (index == -1)
                    {
                        this._bindingsListView.Items.Add(binding);
                        this._bindingsListView.SetSelected(this._bindingsListView.Items.Count - 1, true);
                    }
                    else
                    {
                        binding = (MenuItemBinding) this._bindingsListView.Items[index];
                        this._bindingsListView.SetSelected(index, true);
                    }
                }
                else
                {
                    this._bindingsListView.Items.Add(binding);
                    this._bindingsListView.SetSelected(this._bindingsListView.Items.Count - 1, true);
                }
                this._propertyGrid.SelectedObject = binding;
                this._propertyGrid.Refresh();
                this.UpdateEnabledStates();
            }
            this._bindingsListView.Focus();
        }

        private void ApplyBindings()
        {
            System.Web.UI.Design.ControlDesigner.InvokeTransactedChange(this._menu, new TransactedChangeCallback(this.ApplyBindingsChangeCallback), null, System.Design.SR.GetString("MenuDesigner_EditBindingsTransactionDescription"));
        }

        private bool ApplyBindingsChangeCallback(object context)
        {
            this._menu.DataBindings.Clear();
            foreach (MenuItemBinding binding in this._bindingsListView.Items)
            {
                this._menu.DataBindings.Add(binding);
            }
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private IDataSourceViewSchema FindViewSchema(string viewName, int level)
        {
            return TreeViewBindingsEditorForm.FindViewSchemaRecursive(this.Schema, 0, viewName, level, null);
        }

        private void InitializeComponent()
        {
            this._schemaLabel = new System.Windows.Forms.Label();
            this._bindingsLabel = new System.Windows.Forms.Label();
            this._bindingsListView = new System.Windows.Forms.ListBox();
            this._addBindingButton = new System.Windows.Forms.Button();
            this._propertiesLabel = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._propertyGrid = new VsPropertyGrid(base.ServiceProvider);
            this._schemaTreeView = new System.Windows.Forms.TreeView();
            this._moveBindingUpButton = new System.Windows.Forms.Button();
            this._moveBindingDownButton = new System.Windows.Forms.Button();
            this._deleteBindingButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._applyButton = new System.Windows.Forms.Button();
            base.SuspendLayout();
            this._schemaLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._schemaLabel.Location = new Point(12, 12);
            this._schemaLabel.Name = "_schemaLabel";
            this._schemaLabel.Size = new Size(0xc4, 14);
            this._schemaLabel.TabIndex = 10;
            this._bindingsLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._bindingsLabel.Location = new Point(12, 0xba);
            this._bindingsLabel.Name = "_bindingsLabel";
            this._bindingsLabel.Size = new Size(0xc4, 14);
            this._bindingsLabel.TabIndex = 0x19;
            this._bindingsListView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._bindingsListView.Location = new Point(12, 0xca);
            this._bindingsListView.Name = "_bindingsListView";
            this._bindingsListView.Size = new Size(0xa4, 0x87);
            this._bindingsListView.TabIndex = 30;
            this._bindingsListView.SelectedIndexChanged += new EventHandler(this.OnBindingsListViewSelectedIndexChanged);
            this._bindingsListView.GotFocus += new EventHandler(this.OnBindingsListViewGotFocus);
            this._addBindingButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._addBindingButton.FlatStyle = FlatStyle.System;
            this._addBindingButton.Location = new Point(0x85, 0x9a);
            this._addBindingButton.Name = "_addBindingButton";
            this._addBindingButton.Size = new Size(0x4b, 0x17);
            this._addBindingButton.TabIndex = 20;
            this._addBindingButton.Click += new EventHandler(this.OnAddBindingButtonClick);
            this._propertiesLabel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._propertiesLabel.Location = new Point(0xe5, 12);
            this._propertiesLabel.Name = "_propertiesLabel";
            this._propertiesLabel.Size = new Size(0x10a, 14);
            this._propertiesLabel.TabIndex = 50;
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.FlatStyle = FlatStyle.System;
            this._cancelButton.Location = new Point(340, 0x15a);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 0x41;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.FlatStyle = FlatStyle.System;
            this._okButton.Location = new Point(260, 0x15a);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 60;
            this._okButton.Click += new EventHandler(this.OnOKButtonClick);
            this._applyButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._applyButton.FlatStyle = FlatStyle.System;
            this._applyButton.Location = new Point(420, 0x15a);
            this._applyButton.Name = "_applyButton";
            this._applyButton.TabIndex = 60;
            this._applyButton.Click += new EventHandler(this.OnApplyButtonClick);
            this._applyButton.Enabled = false;
            this._propertyGrid.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            this._propertyGrid.CommandsVisibleIfAvailable = true;
            this._propertyGrid.Cursor = Cursors.HSplit;
            this._propertyGrid.LargeButtons = false;
            this._propertyGrid.LineColor = SystemColors.ScrollBar;
            this._propertyGrid.Location = new Point(0xe5, 0x1c);
            this._propertyGrid.Name = "_propertyGrid";
            this._propertyGrid.Size = new Size(0x10a, 0x135);
            this._propertyGrid.TabIndex = 0x37;
            this._propertyGrid.Text = System.Design.SR.GetString("MenuItemCollectionEditor_PropertyGrid");
            this._propertyGrid.ToolbarVisible = true;
            this._propertyGrid.ViewBackColor = SystemColors.Window;
            this._propertyGrid.ViewForeColor = SystemColors.WindowText;
            this._propertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnPropertyGridPropertyValueChanged);
            this._propertyGrid.Site = this._menu.Site;
            this._schemaTreeView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._schemaTreeView.HideSelection = false;
            this._schemaTreeView.ImageIndex = -1;
            this._schemaTreeView.Location = new Point(12, 0x1c);
            this._schemaTreeView.Name = "_schemaTreeView";
            this._schemaTreeView.SelectedImageIndex = -1;
            this._schemaTreeView.Size = new Size(0xc4, 120);
            this._schemaTreeView.TabIndex = 15;
            this._schemaTreeView.AfterSelect += new TreeViewEventHandler(this.OnSchemaTreeViewAfterSelect);
            this._schemaTreeView.GotFocus += new EventHandler(this.OnSchemaTreeViewGotFocus);
            this._moveBindingUpButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._moveBindingUpButton.Location = new Point(0xb6, 0xca);
            this._moveBindingUpButton.Name = "_moveBindingUpButton";
            this._moveBindingUpButton.Size = new Size(0x1a, 0x17);
            this._moveBindingUpButton.TabIndex = 0x23;
            this._moveBindingUpButton.Click += new EventHandler(this.OnMoveBindingUpButtonClick);
            this._moveBindingDownButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._moveBindingDownButton.Location = new Point(0xb6, 0xe2);
            this._moveBindingDownButton.Name = "_moveBindingDownButton";
            this._moveBindingDownButton.Size = new Size(0x1a, 0x17);
            this._moveBindingDownButton.TabIndex = 40;
            this._moveBindingDownButton.Click += new EventHandler(this.OnMoveBindingDownButtonClick);
            this._deleteBindingButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._deleteBindingButton.Location = new Point(0xb6, 0xff);
            this._deleteBindingButton.Name = "_deleteBindingButton";
            this._deleteBindingButton.Size = new Size(0x1a, 0x17);
            this._deleteBindingButton.TabIndex = 0x2d;
            this._deleteBindingButton.Click += new EventHandler(this.OnDeleteBindingButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x1fb, 0x17d);
            base.Controls.Add(this._deleteBindingButton);
            base.Controls.Add(this._moveBindingDownButton);
            base.Controls.Add(this._moveBindingUpButton);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._applyButton);
            base.Controls.Add(this._propertiesLabel);
            base.Controls.Add(this._addBindingButton);
            base.Controls.Add(this._bindingsListView);
            base.Controls.Add(this._bindingsLabel);
            base.Controls.Add(this._schemaTreeView);
            base.Controls.Add(this._schemaLabel);
            base.Controls.Add(this._propertyGrid);
            this.MinimumSize = new Size(0x1fb, 0x17d);
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "MenuBindingsEditor";
            base.SizeGripStyle = SizeGripStyle.Hide;
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._bindingsLabel.Text = System.Design.SR.GetString("MenuBindingsEditor_Bindings");
            this._schemaLabel.Text = System.Design.SR.GetString("MenuBindingsEditor_Schema");
            this._okButton.Text = System.Design.SR.GetString("MenuBindingsEditor_OK");
            this._applyButton.Text = System.Design.SR.GetString("MenuBindingsEditor_Apply");
            this._cancelButton.Text = System.Design.SR.GetString("MenuBindingsEditor_Cancel");
            this._propertiesLabel.Text = System.Design.SR.GetString("MenuBindingsEditor_BindingProperties");
            this._addBindingButton.Text = System.Design.SR.GetString("MenuBindingsEditor_AddBinding");
            this.Text = System.Design.SR.GetString("MenuBindingsEditor_Title");
            Bitmap bitmap = new Icon(typeof(MenuBindingsEditorForm), "SortUp.ico").ToBitmap();
            bitmap.MakeTransparent();
            this._moveBindingUpButton.Image = bitmap;
            this._moveBindingUpButton.AccessibleName = System.Design.SR.GetString("MenuBindingsEditor_MoveBindingUpName");
            this._moveBindingUpButton.AccessibleDescription = System.Design.SR.GetString("MenuBindingsEditor_MoveBindingUpDescription");
            Bitmap bitmap2 = new Icon(typeof(MenuBindingsEditorForm), "SortDown.ico").ToBitmap();
            bitmap2.MakeTransparent();
            this._moveBindingDownButton.Image = bitmap2;
            this._moveBindingDownButton.AccessibleName = System.Design.SR.GetString("MenuBindingsEditor_MoveBindingDownName");
            this._moveBindingDownButton.AccessibleDescription = System.Design.SR.GetString("MenuBindingsEditor_MoveBindingDownDescription");
            Bitmap bitmap3 = new Icon(typeof(MenuBindingsEditorForm), "Delete.ico").ToBitmap();
            bitmap3.MakeTransparent();
            this._deleteBindingButton.Image = bitmap3;
            this._deleteBindingButton.AccessibleName = System.Design.SR.GetString("MenuBindingsEditor_DeleteBindingName");
            this._deleteBindingButton.AccessibleDescription = System.Design.SR.GetString("MenuBindingsEditor_DeleteBindingDescription");
            base.Icon = null;
        }

        private void OnAddBindingButtonClick(object sender, EventArgs e)
        {
            this._applyButton.Enabled = true;
            this.AddBinding();
        }

        private void OnApplyButtonClick(object sender, EventArgs e)
        {
            this.ApplyBindings();
            this._applyButton.Enabled = false;
        }

        private void OnBindingsListViewGotFocus(object sender, EventArgs e)
        {
            this.UpdateSelectedBinding();
        }

        private void OnBindingsListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateSelectedBinding();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnDeleteBindingButtonClick(object sender, EventArgs e)
        {
            if (this._bindingsListView.SelectedIndices.Count > 0)
            {
                this._applyButton.Enabled = true;
                int index = this._bindingsListView.SelectedIndices[0];
                this._bindingsListView.Items.RemoveAt(index);
                if (index >= this._bindingsListView.Items.Count)
                {
                    index--;
                }
                if ((index >= 0) && (this._bindingsListView.Items.Count > 0))
                {
                    this._bindingsListView.SetSelected(index, true);
                }
            }
        }

        protected override void OnInitialActivated(EventArgs e)
        {
            base.OnInitialActivated(e);
            System.Windows.Forms.TreeNode node = this._schemaTreeView.Nodes.Add(System.Design.SR.GetString("MenuBindingsEditor_EmptyBindingText"));
            if (this.Schema != null)
            {
                this.PopulateSchema(this.Schema);
                this._schemaTreeView.ExpandAll();
            }
            this._schemaTreeView.SelectedNode = node;
            this.UpdateEnabledStates();
        }

        private void OnMoveBindingDownButtonClick(object sender, EventArgs e)
        {
            if (this._bindingsListView.SelectedIndices.Count > 0)
            {
                this._applyButton.Enabled = true;
                int index = this._bindingsListView.SelectedIndices[0];
                if ((index + 1) < this._bindingsListView.Items.Count)
                {
                    MenuItemBinding item = (MenuItemBinding) this._bindingsListView.Items[index];
                    this._bindingsListView.Items.RemoveAt(index);
                    this._bindingsListView.Items.Insert(index + 1, item);
                    this._bindingsListView.SetSelected(index + 1, true);
                }
            }
        }

        private void OnMoveBindingUpButtonClick(object sender, EventArgs e)
        {
            if (this._bindingsListView.SelectedIndices.Count > 0)
            {
                this._applyButton.Enabled = true;
                int index = this._bindingsListView.SelectedIndices[0];
                if (index > 0)
                {
                    MenuItemBinding item = (MenuItemBinding) this._bindingsListView.Items[index];
                    this._bindingsListView.Items.RemoveAt(index);
                    this._bindingsListView.Items.Insert(index - 1, item);
                    this._bindingsListView.SetSelected(index - 1, true);
                }
            }
        }

        private void OnOKButtonClick(object sender, EventArgs e)
        {
            try
            {
                this.ApplyBindings();
            }
            finally
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        private void OnPropertyGridPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            this._applyButton.Enabled = true;
            if (e.ChangedItem.PropertyDescriptor.Name == "DataMember")
            {
                string viewName = (string) e.ChangedItem.Value;
                MenuItemBinding binding = (MenuItemBinding) this._bindingsListView.Items[this._bindingsListView.SelectedIndex];
                this._bindingsListView.Items[this._bindingsListView.SelectedIndex] = binding;
                this._bindingsListView.Refresh();
                IDataSourceViewSchema schema = this.FindViewSchema(viewName, binding.Depth);
                if (schema != null)
                {
                    ((IDataSourceViewSchemaAccessor) binding).DataSourceViewSchema = schema;
                }
                this._propertyGrid.SelectedObject = binding;
                this._propertyGrid.Refresh();
            }
            else if (e.ChangedItem.PropertyDescriptor.Name == "Depth")
            {
                int level = (int) e.ChangedItem.Value;
                MenuItemBinding binding2 = (MenuItemBinding) this._bindingsListView.Items[this._bindingsListView.SelectedIndex];
                IDataSourceViewSchema schema2 = this.FindViewSchema(binding2.DataMember, level);
                if (schema2 != null)
                {
                    ((IDataSourceViewSchemaAccessor) binding2).DataSourceViewSchema = schema2;
                }
                this._propertyGrid.SelectedObject = binding2;
                this._propertyGrid.Refresh();
            }
        }

        private void OnSchemaTreeViewAfterSelect(object sender, TreeViewEventArgs e)
        {
            this.UpdateEnabledStates();
        }

        private void OnSchemaTreeViewGotFocus(object sender, EventArgs e)
        {
            this._propertyGrid.SelectedObject = null;
        }

        private void PopulateSchema(IDataSourceSchema schema)
        {
            if (schema != null)
            {
                IDictionary duplicates = new Hashtable();
                IDataSourceViewSchema[] views = schema.GetViews();
                if (views != null)
                {
                    for (int i = 0; i < views.Length; i++)
                    {
                        this.PopulateSchemaRecursive(this._schemaTreeView.Nodes, views[i], 0, duplicates);
                    }
                }
            }
        }

        private void PopulateSchemaRecursive(System.Windows.Forms.TreeNodeCollection nodes, IDataSourceViewSchema viewSchema, int depth, IDictionary duplicates)
        {
            if (viewSchema != null)
            {
                SchemaTreeNode node = new SchemaTreeNode(viewSchema);
                nodes.Add(node);
                SchemaTreeNode node2 = (SchemaTreeNode) duplicates[viewSchema.Name];
                if (node2 != null)
                {
                    node2.Duplicate = true;
                    node.Duplicate = true;
                }
                foreach (MenuItemBinding binding in this._bindingsListView.Items)
                {
                    if (string.Compare(binding.DataMember, viewSchema.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        IDataSourceViewSchemaAccessor accessor = binding;
                        if ((depth == binding.Depth) || (accessor.DataSourceViewSchema == null))
                        {
                            accessor.DataSourceViewSchema = viewSchema;
                        }
                    }
                }
                IDataSourceViewSchema[] children = viewSchema.GetChildren();
                if (children != null)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        this.PopulateSchemaRecursive(node.Nodes, children[i], depth + 1, duplicates);
                    }
                }
            }
        }

        private void UpdateEnabledStates()
        {
            if (this._bindingsListView.SelectedIndices.Count > 0)
            {
                int num = this._bindingsListView.SelectedIndices[0];
                this._moveBindingDownButton.Enabled = (num + 1) < this._bindingsListView.Items.Count;
                this._moveBindingUpButton.Enabled = num > 0;
                this._deleteBindingButton.Enabled = true;
            }
            else
            {
                this._moveBindingDownButton.Enabled = false;
                this._moveBindingUpButton.Enabled = false;
                this._deleteBindingButton.Enabled = false;
            }
            this._addBindingButton.Enabled = this._schemaTreeView.SelectedNode != null;
        }

        private void UpdateSelectedBinding()
        {
            MenuItemBinding binding = null;
            if (this._bindingsListView.SelectedItems.Count > 0)
            {
                binding = (MenuItemBinding) this._bindingsListView.SelectedItems[0];
            }
            this._propertyGrid.SelectedObject = binding;
            this._propertyGrid.Refresh();
            this.UpdateEnabledStates();
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.Menu.BindingsEditorForm";
            }
        }

        private IDataSourceSchema Schema
        {
            get
            {
                if (this._schema == null)
                {
                    IDesignerHost host = (IDesignerHost) base.ServiceProvider.GetService(typeof(IDesignerHost));
                    if (host != null)
                    {
                        HierarchicalDataBoundControlDesigner designer = host.GetDesigner(this._menu) as HierarchicalDataBoundControlDesigner;
                        if (designer != null)
                        {
                            DesignerHierarchicalDataSourceView designerView = designer.DesignerView;
                            if (designerView != null)
                            {
                                try
                                {
                                    this._schema = designerView.Schema;
                                }
                                catch (Exception exception)
                                {
                                    IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.ServiceProvider.GetService(typeof(IComponentDesignerDebugService));
                                    if (service != null)
                                    {
                                        service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerHierarchicalDataSourceView.Schema", exception.Message }));
                                    }
                                }
                            }
                        }
                    }
                }
                return this._schema;
            }
        }

        private class SchemaTreeNode : System.Windows.Forms.TreeNode
        {
            private bool _duplicate;
            private IDataSourceViewSchema _schema;

            public SchemaTreeNode(IDataSourceViewSchema schema) : base(schema.Name)
            {
                this._schema = schema;
            }

            public bool Duplicate
            {
                get
                {
                    return this._duplicate;
                }
                set
                {
                    this._duplicate = value;
                }
            }

            public object Schema
            {
                get
                {
                    return this._schema;
                }
            }
        }
    }
}

