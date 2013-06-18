namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    internal class BindingFormattingDialog : Form
    {
        private Label bindingLabel;
        private ControlBindingsCollection bindings;
        private ComboBox bindingUpdateDropDown;
        private static Bitmap boundBitmap;
        private const int BOUNDIMAGEINDEX = 0;
        private Button cancelButton;
        private ITypeDescriptorContext context;
        private BindingTreeNode currentBindingTreeNode;
        private BindingFormattingWindowsFormsEditorService dataSourcePicker;
        private bool dirty;
        private Label explanationLabel;
        private FormatControl formatControl1;
        private IDesignerHost host;
        private bool inLoad;
        private TableLayoutPanel mainTableLayoutPanel;
        private Button okButton;
        private TableLayoutPanel okCancelTableLayoutPanel;
        private TreeView propertiesTreeView;
        private Label propertyLabel;
        private static Bitmap unboundBitmap;
        private const int UNBOUNDIMAGEINDEX = 1;
        private Label updateModeLabel;

        public BindingFormattingDialog()
        {
            this.InitializeComponent();
        }

        private void BindingFormattingDialog_Closing(object sender, CancelEventArgs e)
        {
            this.currentBindingTreeNode = null;
            this.dataSourcePicker.OwnerComponent = null;
            this.formatControl1.ResetFormattingInfo();
        }

        private void BindingFormattingDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            this.BindingFormattingDialog_HelpRequestHandled();
            e.Cancel = true;
        }

        private void BindingFormattingDialog_HelpRequested(object sender, HelpEventArgs e)
        {
            this.BindingFormattingDialog_HelpRequestHandled();
            e.Handled = true;
        }

        private void BindingFormattingDialog_HelpRequestHandled()
        {
            IHelpService service = this.context.GetService(typeof(IHelpService)) as IHelpService;
            if (service != null)
            {
                service.ShowHelpFromKeyword("vs.BindingFormattingDialog");
            }
        }

        private void BindingFormattingDialog_Load(object sender, EventArgs e)
        {
            this.inLoad = true;
            try
            {
                BindingTreeNode node6;
                this.dirty = false;
                Font defaultFont = Control.DefaultFont;
                IUIService service = null;
                if (this.bindings.BindableComponent.Site != null)
                {
                    service = (IUIService) this.bindings.BindableComponent.Site.GetService(typeof(IUIService));
                }
                if (service != null)
                {
                    defaultFont = (Font) service.Styles["DialogFont"];
                }
                this.Font = defaultFont;
                DesignerUtils.ApplyTreeViewThemeStyles(this.propertiesTreeView);
                if (this.propertiesTreeView.ImageList == null)
                {
                    ImageList list = new ImageList();
                    list.Images.Add(BoundBitmap);
                    list.Images.Add(UnboundBitmap);
                    this.propertiesTreeView.ImageList = list;
                }
                BindingTreeNode node = null;
                BindingTreeNode node2 = null;
                string name = null;
                string str2 = null;
                foreach (Attribute attribute in TypeDescriptor.GetAttributes(this.bindings.BindableComponent))
                {
                    if (attribute is DefaultBindingPropertyAttribute)
                    {
                        name = ((DefaultBindingPropertyAttribute) attribute).Name;
                        break;
                    }
                    if (attribute is DefaultPropertyAttribute)
                    {
                        str2 = ((DefaultPropertyAttribute) attribute).Name;
                    }
                }
                this.propertiesTreeView.Nodes.Clear();
                TreeNode node3 = new TreeNode(System.Design.SR.GetString("BindingFormattingDialogCommonTreeNode"));
                TreeNode node4 = new TreeNode(System.Design.SR.GetString("BindingFormattingDialogAllTreeNode"));
                this.propertiesTreeView.Nodes.Add(node3);
                this.propertiesTreeView.Nodes.Add(node4);
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.bindings.BindableComponent);
                for (int i = 0; i < properties.Count; i++)
                {
                    if (!properties[i].IsReadOnly)
                    {
                        BindableAttribute attribute2 = (BindableAttribute) properties[i].Attributes[typeof(BindableAttribute)];
                        BrowsableAttribute attribute3 = (BrowsableAttribute) properties[i].Attributes[typeof(BrowsableAttribute)];
                        if (((attribute3 == null) || attribute3.Browsable) || ((attribute2 != null) && attribute2.Bindable))
                        {
                            BindingTreeNode node5 = new BindingTreeNode(properties[i].Name) {
                                Binding = this.FindBinding(properties[i].Name)
                            };
                            if (node5.Binding != null)
                            {
                                node5.FormatType = FormatControl.FormatTypeStringFromFormatString(node5.Binding.FormatString);
                            }
                            else
                            {
                                node5.FormatType = System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormatting");
                            }
                            if ((attribute2 != null) && attribute2.Bindable)
                            {
                                node3.Nodes.Add(node5);
                            }
                            else
                            {
                                node4.Nodes.Add(node5);
                            }
                            if (((node == null) && !string.IsNullOrEmpty(name)) && (string.Compare(properties[i].Name, name, false, CultureInfo.CurrentCulture) == 0))
                            {
                                node = node5;
                            }
                            else if (((node2 == null) && !string.IsNullOrEmpty(str2)) && (string.Compare(properties[i].Name, str2, false, CultureInfo.CurrentCulture) == 0))
                            {
                                node2 = node5;
                            }
                        }
                    }
                }
                node3.Expand();
                node4.Expand();
                this.propertiesTreeView.Sort();
                if (node != null)
                {
                    node6 = node;
                }
                else if (node2 != null)
                {
                    node6 = node2;
                }
                else if (node3.Nodes.Count > 0)
                {
                    node6 = FirstNodeInAlphabeticalOrder(node3.Nodes) as BindingTreeNode;
                }
                else if (node4.Nodes.Count > 0)
                {
                    node6 = FirstNodeInAlphabeticalOrder(node4.Nodes) as BindingTreeNode;
                }
                else
                {
                    node6 = null;
                }
                this.propertiesTreeView.SelectedNode = node6;
                if (node6 != null)
                {
                    node6.EnsureVisible();
                }
                this.dataSourcePicker.PropertyName = node6.Text;
                this.dataSourcePicker.Binding = (node6 != null) ? node6.Binding : null;
                this.dataSourcePicker.Enabled = true;
                this.dataSourcePicker.OwnerComponent = this.bindings.BindableComponent;
                this.dataSourcePicker.DefaultDataSourceUpdateMode = this.bindings.DefaultDataSourceUpdateMode;
                if ((node6 != null) && (node6.Binding != null))
                {
                    this.bindingUpdateDropDown.Enabled = true;
                    this.bindingUpdateDropDown.SelectedItem = node6.Binding.DataSourceUpdateMode;
                    this.updateModeLabel.Enabled = true;
                    this.formatControl1.Enabled = true;
                    this.formatControl1.FormatType = node6.FormatType;
                    this.formatControl1.FormatTypeItem.PushFormatStringIntoFormatType(node6.Binding.FormatString);
                    if (node6.Binding.NullValue != null)
                    {
                        this.formatControl1.NullValue = node6.Binding.NullValue.ToString();
                    }
                    else
                    {
                        this.formatControl1.NullValue = string.Empty;
                    }
                }
                else
                {
                    this.bindingUpdateDropDown.Enabled = false;
                    this.bindingUpdateDropDown.SelectedItem = this.bindings.DefaultDataSourceUpdateMode;
                    this.updateModeLabel.Enabled = false;
                    this.formatControl1.Enabled = false;
                    this.formatControl1.FormatType = string.Empty;
                }
                this.formatControl1.Dirty = false;
                this.currentBindingTreeNode = this.propertiesTreeView.SelectedNode as BindingTreeNode;
            }
            finally
            {
                this.inLoad = false;
            }
        }

        private void bindingUpdateDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this.inLoad)
            {
                this.dirty = true;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.dirty = false;
        }

        private void ConsolidateBindingInformation()
        {
            Binding binding = this.dataSourcePicker.Binding;
            if (binding != null)
            {
                binding.FormattingEnabled = true;
                this.currentBindingTreeNode.Binding = binding;
                this.currentBindingTreeNode.FormatType = this.formatControl1.FormatType;
                FormatControl.FormatTypeClass formatTypeItem = this.formatControl1.FormatTypeItem;
                if (formatTypeItem != null)
                {
                    binding.FormatString = formatTypeItem.FormatString;
                    binding.NullValue = this.formatControl1.NullValue;
                }
                binding.DataSourceUpdateMode = (DataSourceUpdateMode) this.bindingUpdateDropDown.SelectedItem;
            }
        }

        private void dataSourcePicker_PropertyValueChanged(object sender, EventArgs e)
        {
            if (!this.inLoad)
            {
                BindingTreeNode selectedNode = this.propertiesTreeView.SelectedNode as BindingTreeNode;
                if (this.dataSourcePicker.Binding != selectedNode.Binding)
                {
                    Binding binding = this.dataSourcePicker.Binding;
                    if (binding != null)
                    {
                        binding.FormattingEnabled = true;
                        Binding binding2 = selectedNode.Binding;
                        if (binding2 != null)
                        {
                            binding.FormatString = binding2.FormatString;
                            binding.NullValue = binding2.NullValue;
                            binding.FormatInfo = binding2.FormatInfo;
                        }
                    }
                    selectedNode.Binding = binding;
                    if (binding != null)
                    {
                        this.formatControl1.Enabled = true;
                        this.updateModeLabel.Enabled = true;
                        this.bindingUpdateDropDown.Enabled = true;
                        this.bindingUpdateDropDown.SelectedItem = binding.DataSourceUpdateMode;
                        if (!string.IsNullOrEmpty(this.formatControl1.FormatType))
                        {
                            this.formatControl1.FormatType = this.formatControl1.FormatType;
                        }
                        else
                        {
                            this.formatControl1.FormatType = System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormatting");
                        }
                    }
                    else
                    {
                        this.formatControl1.Enabled = false;
                        this.updateModeLabel.Enabled = false;
                        this.bindingUpdateDropDown.Enabled = false;
                        this.bindingUpdateDropDown.SelectedItem = this.bindings.DefaultDataSourceUpdateMode;
                        this.formatControl1.FormatType = System.Design.SR.GetString("BindingFormattingDialogFormatTypeNoFormatting");
                    }
                    this.dirty = true;
                }
            }
        }

        private Binding FindBinding(string propertyName)
        {
            for (int i = 0; i < this.bindings.Count; i++)
            {
                if (string.Equals(propertyName, this.bindings[i].PropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    return this.bindings[i];
                }
            }
            return null;
        }

        private static TreeNode FirstNodeInAlphabeticalOrder(TreeNodeCollection nodes)
        {
            if (nodes.Count == 0)
            {
                return null;
            }
            TreeNode node = nodes[0];
            for (int i = 1; i < nodes.Count; i++)
            {
                if (string.Compare(node.Text, nodes[i].Text, false, CultureInfo.CurrentCulture) > 0)
                {
                    node = nodes[i];
                }
            }
            return node;
        }

        private void InitializeComponent()
        {
            ComponentResourceManager manager = new ComponentResourceManager(typeof(BindingFormattingDialog));
            this.explanationLabel = new Label();
            this.mainTableLayoutPanel = new TableLayoutPanel();
            this.propertiesTreeView = new TreeView();
            this.propertyLabel = new Label();
            this.dataSourcePicker = new BindingFormattingWindowsFormsEditorService();
            this.bindingLabel = new Label();
            this.updateModeLabel = new Label();
            this.bindingUpdateDropDown = new ComboBox();
            this.formatControl1 = new FormatControl();
            this.okCancelTableLayoutPanel = new TableLayoutPanel();
            this.okButton = new Button();
            this.cancelButton = new Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.okCancelTableLayoutPanel.SuspendLayout();
            base.ShowIcon = false;
            base.SuspendLayout();
            manager.ApplyResources(this.explanationLabel, "explanationLabel");
            this.mainTableLayoutPanel.SetColumnSpan(this.explanationLabel, 3);
            this.explanationLabel.Name = "explanationLabel";
            manager.ApplyResources(this.mainTableLayoutPanel, "mainTableLayoutPanel");
            this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.mainTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this.mainTableLayoutPanel.Controls.Add(this.okCancelTableLayoutPanel, 2, 4);
            this.mainTableLayoutPanel.Controls.Add(this.formatControl1, 1, 3);
            this.mainTableLayoutPanel.Controls.Add(this.bindingUpdateDropDown, 2, 2);
            this.mainTableLayoutPanel.Controls.Add(this.propertiesTreeView, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.updateModeLabel, 2, 1);
            this.mainTableLayoutPanel.Controls.Add(this.dataSourcePicker, 1, 2);
            this.mainTableLayoutPanel.Controls.Add(this.explanationLabel, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.bindingLabel, 1, 1);
            this.mainTableLayoutPanel.Controls.Add(this.propertyLabel, 0, 1);
            this.mainTableLayoutPanel.MinimumSize = new Size(0x21e, 0x11b);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            this.mainTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            this.mainTableLayoutPanel.RowStyles.Add(new RowStyle());
            manager.ApplyResources(this.propertiesTreeView, "propertiesTreeView");
            this.propertiesTreeView.Name = "propertiesTreeView";
            this.propertiesTreeView.HideSelection = false;
            this.propertiesTreeView.TreeViewNodeSorter = new TreeNodeComparer();
            this.mainTableLayoutPanel.SetRowSpan(this.propertiesTreeView, 2);
            this.propertiesTreeView.BeforeSelect += new TreeViewCancelEventHandler(this.propertiesTreeView_BeforeSelect);
            this.propertiesTreeView.AfterSelect += new TreeViewEventHandler(this.propertiesTreeView_AfterSelect);
            manager.ApplyResources(this.propertyLabel, "propertyLabel");
            this.propertyLabel.Name = "propertyLabel";
            manager.ApplyResources(this.dataSourcePicker, "dataSourcePicker");
            this.dataSourcePicker.Name = "dataSourcePicker";
            this.dataSourcePicker.PropertyValueChanged += new EventHandler(this.dataSourcePicker_PropertyValueChanged);
            manager.ApplyResources(this.bindingLabel, "bindingLabel");
            this.bindingLabel.Name = "bindingLabel";
            manager.ApplyResources(this.updateModeLabel, "updateModeLabel");
            this.updateModeLabel.Name = "updateModeLabel";
            this.bindingUpdateDropDown.FormattingEnabled = true;
            manager.ApplyResources(this.bindingUpdateDropDown, "bindingUpdateDropDown");
            this.bindingUpdateDropDown.DropDownStyle = ComboBoxStyle.DropDownList;
            this.bindingUpdateDropDown.Name = "bindingUpdateDropDown";
            this.bindingUpdateDropDown.Items.AddRange(new object[] { DataSourceUpdateMode.Never, DataSourceUpdateMode.OnPropertyChanged, DataSourceUpdateMode.OnValidation });
            this.bindingUpdateDropDown.SelectedIndexChanged += new EventHandler(this.bindingUpdateDropDown_SelectedIndexChanged);
            this.mainTableLayoutPanel.SetColumnSpan(this.formatControl1, 2);
            manager.ApplyResources(this.formatControl1, "formatControl1");
            this.formatControl1.MinimumSize = new Size(390, 0xed);
            this.formatControl1.Name = "formatControl1";
            this.formatControl1.NullValueTextBoxEnabled = true;
            manager.ApplyResources(this.okCancelTableLayoutPanel, "okCancelTableLayoutPanel");
            this.okCancelTableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            this.okCancelTableLayoutPanel.Controls.Add(this.cancelButton, 1, 0);
            this.okCancelTableLayoutPanel.Controls.Add(this.okButton, 0, 0);
            this.okCancelTableLayoutPanel.Name = "okCancelTableLayoutPanel";
            this.okCancelTableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 29f));
            manager.ApplyResources(this.okButton, "okButton");
            this.okButton.Name = "okButton";
            this.okButton.DialogResult = DialogResult.OK;
            this.okButton.Click += new EventHandler(this.okButton_Click);
            manager.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.DialogResult = DialogResult.Cancel;
            this.cancelButton.Click += new EventHandler(this.cancelButton_Click);
            manager.ApplyResources(this, "$this");
            base.AutoScaleMode = AutoScaleMode.Font;
            base.StartPosition = FormStartPosition.CenterParent;
            base.CancelButton = this.cancelButton;
            base.AcceptButton = this.okButton;
            base.Controls.Add(this.mainTableLayoutPanel);
            base.FormBorderStyle = FormBorderStyle.Sizable;
            base.Name = "BindingFormattingDialog";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.mainTableLayoutPanel.PerformLayout();
            this.okCancelTableLayoutPanel.ResumeLayout(false);
            base.HelpButton = true;
            base.ShowInTaskbar = false;
            base.MinimizeBox = false;
            base.MaximizeBox = false;
            base.Load += new EventHandler(this.BindingFormattingDialog_Load);
            base.Closing += new CancelEventHandler(this.BindingFormattingDialog_Closing);
            base.HelpButtonClicked += new CancelEventHandler(this.BindingFormattingDialog_HelpButtonClicked);
            base.HelpRequested += new HelpEventHandler(this.BindingFormattingDialog_HelpRequested);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            if (this.currentBindingTreeNode != null)
            {
                this.ConsolidateBindingInformation();
            }
            this.PushChanges();
        }

        private void propertiesTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!this.inLoad)
            {
                BindingTreeNode node = e.Node as BindingTreeNode;
                if (node == null)
                {
                    this.dataSourcePicker.Binding = null;
                    this.bindingLabel.Enabled = this.dataSourcePicker.Enabled = false;
                    this.updateModeLabel.Enabled = this.bindingUpdateDropDown.Enabled = false;
                    this.formatControl1.Enabled = false;
                }
                else
                {
                    this.bindingLabel.Enabled = this.dataSourcePicker.Enabled = true;
                    this.dataSourcePicker.PropertyName = node.Text;
                    this.updateModeLabel.Enabled = this.bindingUpdateDropDown.Enabled = false;
                    this.formatControl1.Enabled = false;
                    if (node.Binding != null)
                    {
                        this.formatControl1.Enabled = true;
                        this.formatControl1.FormatType = node.FormatType;
                        FormatControl.FormatTypeClass formatTypeItem = this.formatControl1.FormatTypeItem;
                        this.dataSourcePicker.Binding = node.Binding;
                        formatTypeItem.PushFormatStringIntoFormatType(node.Binding.FormatString);
                        if (node.Binding.NullValue != null)
                        {
                            this.formatControl1.NullValue = node.Binding.NullValue.ToString();
                        }
                        else
                        {
                            this.formatControl1.NullValue = string.Empty;
                        }
                        this.bindingUpdateDropDown.SelectedItem = node.Binding.DataSourceUpdateMode;
                        this.updateModeLabel.Enabled = this.bindingUpdateDropDown.Enabled = true;
                    }
                    else
                    {
                        bool dirty = this.dirty;
                        this.dataSourcePicker.Binding = null;
                        this.formatControl1.FormatType = node.FormatType;
                        this.bindingUpdateDropDown.SelectedItem = this.bindings.DefaultDataSourceUpdateMode;
                        this.formatControl1.NullValue = null;
                        this.dirty = dirty;
                    }
                    this.formatControl1.Dirty = false;
                    this.currentBindingTreeNode = node;
                }
            }
        }

        private void propertiesTreeView_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            if (((!this.inLoad && (this.currentBindingTreeNode != null)) && (this.dataSourcePicker.Binding != null)) && this.formatControl1.Enabled)
            {
                this.ConsolidateBindingInformation();
                this.dirty = this.dirty || this.formatControl1.Dirty;
            }
        }

        private void PushChanges()
        {
            if (this.Dirty)
            {
                IComponentChangeService service = this.host.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = null;
                IBindableComponent bindableComponent = this.bindings.BindableComponent;
                if ((service != null) && (bindableComponent != null))
                {
                    member = TypeDescriptor.GetProperties(bindableComponent)["DataBindings"];
                    if (member != null)
                    {
                        service.OnComponentChanging(bindableComponent, member);
                    }
                }
                this.bindings.Clear();
                TreeNode node = this.propertiesTreeView.Nodes[0];
                for (int i = 0; i < node.Nodes.Count; i++)
                {
                    BindingTreeNode node2 = node.Nodes[i] as BindingTreeNode;
                    if (node2.Binding != null)
                    {
                        this.bindings.Add(node2.Binding);
                    }
                }
                TreeNode node3 = this.propertiesTreeView.Nodes[1];
                for (int j = 0; j < node3.Nodes.Count; j++)
                {
                    BindingTreeNode node4 = node3.Nodes[j] as BindingTreeNode;
                    if (node4.Binding != null)
                    {
                        this.bindings.Add(node4.Binding);
                    }
                }
                if (((service != null) && (bindableComponent != null)) && (member != null))
                {
                    service.OnComponentChanged(bindableComponent, member, null, null);
                }
            }
        }

        public ControlBindingsCollection Bindings
        {
            set
            {
                this.bindings = value;
            }
        }

        private static Bitmap BoundBitmap
        {
            get
            {
                if (boundBitmap == null)
                {
                    boundBitmap = new Bitmap(typeof(BindingFormattingDialog), "BindingFormattingDialog.Bound.bmp");
                    boundBitmap.MakeTransparent(Color.Red);
                }
                return boundBitmap;
            }
        }

        public ITypeDescriptorContext Context
        {
            get
            {
                return this.context;
            }
            set
            {
                this.context = value;
                this.dataSourcePicker.Context = value;
            }
        }

        public bool Dirty
        {
            get
            {
                if (!this.dirty)
                {
                    return this.formatControl1.Dirty;
                }
                return true;
            }
        }

        public IDesignerHost Host
        {
            set
            {
                this.host = value;
            }
        }

        private static Bitmap UnboundBitmap
        {
            get
            {
                if (unboundBitmap == null)
                {
                    unboundBitmap = new Bitmap(typeof(BindingFormattingDialog), "BindingFormattingDialog.Unbound.bmp");
                    unboundBitmap.MakeTransparent(Color.Red);
                }
                return unboundBitmap;
            }
        }

        private class BindingTreeNode : TreeNode
        {
            private System.Windows.Forms.Binding binding;
            private string formatType;

            public BindingTreeNode(string name) : base(name)
            {
            }

            public System.Windows.Forms.Binding Binding
            {
                get
                {
                    return this.binding;
                }
                set
                {
                    this.binding = value;
                    base.ImageIndex = (this.binding != null) ? 0 : 1;
                    base.SelectedImageIndex = (this.binding != null) ? 0 : 1;
                }
            }

            public string FormatType
            {
                get
                {
                    return this.formatType;
                }
                set
                {
                    this.formatType = value;
                }
            }
        }

        private class TreeNodeComparer : IComparer
        {
            int IComparer.Compare(object o1, object o2)
            {
                TreeNode node = o1 as TreeNode;
                TreeNode node2 = o2 as TreeNode;
                BindingFormattingDialog.BindingTreeNode node3 = node as BindingFormattingDialog.BindingTreeNode;
                BindingFormattingDialog.BindingTreeNode node4 = node2 as BindingFormattingDialog.BindingTreeNode;
                if (node3 != null)
                {
                    return string.Compare(node3.Text, node4.Text, false, CultureInfo.CurrentCulture);
                }
                if (string.Compare(node.Text, System.Design.SR.GetString("BindingFormattingDialogAllTreeNode"), false, CultureInfo.CurrentCulture) == 0)
                {
                    if (string.Compare(node2.Text, System.Design.SR.GetString("BindingFormattingDialogAllTreeNode"), false, CultureInfo.CurrentCulture) == 0)
                    {
                        return 0;
                    }
                    return 1;
                }
                if (string.Compare(node2.Text, System.Design.SR.GetString("BindingFormattingDialogCommonTreeNode"), false, CultureInfo.CurrentCulture) == 0)
                {
                    return 0;
                }
                return -1;
            }
        }
    }
}

