namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;

    internal sealed class DataBindingsDialog : DesignerForm
    {
        private CheckBox _allPropsCheckBox;
        private Label _bindablePropsLabels;
        private TreeView _bindablePropsTree;
        private Label _bindingLabel;
        private Panel _bindingOptionsPanel;
        private IDictionary _bindings;
        private bool _bindingsDirty;
        private Button _cancelButton;
        private string _controlID;
        private DesignTimeDataBinding _currentDataBinding;
        private bool _currentDataBindingDirty;
        private BindablePropertyNode _currentNode;
        private Panel _customBindingPanel;
        private RadioButton _exprBindingRadio;
        private Label _exprLabel;
        private TextBox _exprTextBox;
        private Panel _fieldBindingPanel;
        private RadioButton _fieldBindingRadio;
        private ComboBox _fieldCombo;
        private Label _fieldLabel;
        private bool _fieldsAvailable;
        private ComboBox _formatCombo;
        private bool _formatDirty;
        private Label _formatLabel;
        private object _formatSampleObject;
        private Label _instructionLabel;
        private bool _internalChange;
        private Button _okButton;
        private LinkLabel _refreshSchemaLink;
        private Label _sampleLabel;
        private TextBox _sampleTextBox;
        private CheckBox _twoWayBindingCheckBox;
        private static readonly Attribute[] BindablePropertiesFilter = new Attribute[] { BindableAttribute.Yes, ReadOnlyAttribute.No };
        private const int BoundImageIndex = 1;
        private static readonly Attribute[] BrowsablePropertiesFilter = new Attribute[] { BrowsableAttribute.Yes, ReadOnlyAttribute.No };
        private const int TwoWayBoundImageIndex = 2;
        private const int UnboundImageIndex = 0;
        private const int UnboundItemIndex = 0;

        public DataBindingsDialog(IServiceProvider serviceProvider, System.Web.UI.Control control) : base(serviceProvider)
        {
            this._controlID = control.ID;
            this.InitializeComponent();
            this.InitializeUserInterface();
        }

        private bool ContainingTemplateIsBindable(ControlDesigner designer)
        {
            bool flag = false;
            IControlDesignerView view = designer.View;
            if (view != null)
            {
                TemplatedEditableDesignerRegion containingRegion = view.ContainingRegion as TemplatedEditableDesignerRegion;
                if (containingRegion != null)
                {
                    TemplateDefinition templateDefinition = containingRegion.TemplateDefinition;
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(templateDefinition.TemplatedObject)[templateDefinition.TemplatePropertyName];
                    if (descriptor != null)
                    {
                        TemplateContainerAttribute attribute = descriptor.Attributes[typeof(TemplateContainerAttribute)] as TemplateContainerAttribute;
                        if ((attribute != null) && (attribute.BindingDirection == BindingDirection.TwoWay))
                        {
                            flag = true;
                        }
                    }
                }
            }
            return flag;
        }

        private void ExtractFields(IDataSourceProvider dataSourceProvider, ArrayList fields)
        {
            IEnumerable resolvedSelectedDataSource = dataSourceProvider.GetResolvedSelectedDataSource();
            if (resolvedSelectedDataSource != null)
            {
                PropertyDescriptorCollection dataFields = DesignTimeData.GetDataFields(resolvedSelectedDataSource);
                if ((dataFields != null) && (dataFields.Count != 0))
                {
                    foreach (PropertyDescriptor descriptor in dataFields)
                    {
                        fields.Add(new FieldItem(descriptor.Name, descriptor.PropertyType));
                    }
                }
            }
        }

        private void ExtractFields(IDataSourceViewSchema schema, ArrayList fields)
        {
            if (schema != null)
            {
                IDataSourceFieldSchema[] schemaArray = schema.GetFields();
                if (schemaArray != null)
                {
                    for (int i = 0; i < schemaArray.Length; i++)
                    {
                        fields.Add(new FieldItem(schemaArray[i].Name, schemaArray[i].DataType));
                    }
                }
            }
        }

        private IDesigner GetNamingContainerDesigner(ControlDesigner designer)
        {
            IControlDesignerView view = designer.View;
            if (view == null)
            {
                return null;
            }
            return view.NamingContainerDesigner;
        }

        private void InitializeComponent()
        {
            this._instructionLabel = new Label();
            this._bindablePropsLabels = new Label();
            this._bindablePropsTree = new TreeView();
            this._allPropsCheckBox = new CheckBox();
            this._bindingLabel = new Label();
            this._fieldBindingRadio = new RadioButton();
            this._fieldLabel = new Label();
            this._fieldCombo = new ComboBox();
            this._formatLabel = new Label();
            this._formatCombo = new ComboBox();
            this._sampleLabel = new Label();
            this._sampleTextBox = new TextBox();
            this._exprBindingRadio = new RadioButton();
            this._exprTextBox = new TextBox();
            this._okButton = new Button();
            this._cancelButton = new Button();
            this._refreshSchemaLink = new LinkLabel();
            this._exprLabel = new Label();
            this._twoWayBindingCheckBox = new CheckBox();
            this._fieldBindingPanel = new Panel();
            this._customBindingPanel = new Panel();
            this._bindingOptionsPanel = new Panel();
            base.SuspendLayout();
            this._instructionLabel.FlatStyle = FlatStyle.System;
            this._instructionLabel.Location = new Point(12, 12);
            this._instructionLabel.Name = "_instructionLabel";
            this._instructionLabel.Size = new Size(0x1fc, 30);
            this._instructionLabel.TabIndex = 0;
            this._bindablePropsLabels.FlatStyle = FlatStyle.System;
            this._bindablePropsLabels.Location = new Point(12, 0x34);
            this._bindablePropsLabels.Name = "_bindablePropsLabels";
            this._bindablePropsLabels.Size = new Size(0xb8, 0x10);
            this._bindablePropsLabels.TabIndex = 1;
            this._bindablePropsTree.HideSelection = false;
            this._bindablePropsTree.ImageIndex = -1;
            this._bindablePropsTree.Location = new Point(12, 0x48);
            this._bindablePropsTree.Name = "_bindablePropsTree";
            this._bindablePropsTree.SelectedImageIndex = -1;
            this._bindablePropsTree.ShowLines = false;
            this._bindablePropsTree.ShowPlusMinus = false;
            this._bindablePropsTree.ShowRootLines = false;
            this._bindablePropsTree.Size = new Size(0xb8, 0x70);
            this._bindablePropsTree.TabIndex = 2;
            this._bindablePropsTree.Sorted = true;
            this._bindablePropsTree.AfterSelect += new TreeViewEventHandler(this.OnBindablePropsTreeAfterSelect);
            this._allPropsCheckBox.FlatStyle = FlatStyle.System;
            this._allPropsCheckBox.Location = new Point(12, 190);
            this._allPropsCheckBox.Name = "_allPropsCheckBox";
            this._allPropsCheckBox.Size = new Size(0xb8, 40);
            this._allPropsCheckBox.TabIndex = 3;
            this._allPropsCheckBox.Visible = true;
            this._allPropsCheckBox.CheckedChanged += new EventHandler(this.OnShowAllCheckedChanged);
            this._allPropsCheckBox.TextAlign = ContentAlignment.TopLeft;
            this._allPropsCheckBox.CheckAlign = ContentAlignment.TopLeft;
            this._bindingLabel.Location = new Point(210, 0x34);
            this._bindingLabel.Name = "_bindingGroupLabel";
            this._bindingLabel.Size = new Size(0x132, 0x10);
            this._bindingLabel.TabIndex = 4;
            this._fieldLabel.FlatStyle = FlatStyle.System;
            this._fieldLabel.Location = new Point(0, 4);
            this._fieldLabel.Name = "_fieldLabel";
            this._fieldLabel.Size = new Size(0x68, 0x10);
            this._fieldLabel.TabIndex = 100;
            this._fieldCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            this._fieldCombo.Location = new Point(0x76, 0);
            this._fieldCombo.Name = "_fieldCombo";
            this._fieldCombo.Size = new Size(0xa4, 0x15);
            this._fieldCombo.TabIndex = 0x65;
            this._fieldCombo.SelectedIndexChanged += new EventHandler(this.OnFieldComboSelectedIndexChanged);
            this._formatLabel.FlatStyle = FlatStyle.System;
            this._formatLabel.Location = new Point(0, 0x20);
            this._formatLabel.Name = "_formatLabel";
            this._formatLabel.Size = new Size(0x72, 0x10);
            this._formatLabel.TabIndex = 0x66;
            this._formatCombo.Location = new Point(0x76, 0x1c);
            this._formatCombo.Name = "_formatCombo";
            this._formatCombo.Size = new Size(0xa4, 0x15);
            this._formatCombo.TabIndex = 0x67;
            this._formatCombo.LostFocus += new EventHandler(this.OnFormatComboLostFocus);
            this._formatCombo.TextChanged += new EventHandler(this.OnFormatComboTextChanged);
            this._formatCombo.SelectedIndexChanged += new EventHandler(this.OnFormatComboSelectedIndexChanged);
            this._sampleLabel.FlatStyle = FlatStyle.System;
            this._sampleLabel.Location = new Point(0, 60);
            this._sampleLabel.Name = "_sampleLabel";
            this._sampleLabel.Size = new Size(0x72, 0x10);
            this._sampleLabel.TabIndex = 0x68;
            this._sampleTextBox.Location = new Point(0x76, 0x38);
            this._sampleTextBox.Name = "_sampleTextBox";
            this._sampleTextBox.ReadOnly = true;
            this._sampleTextBox.Size = new Size(0xa4, 20);
            this._sampleTextBox.TabIndex = 0x69;
            this._exprTextBox.Location = new Point(0, 0x12);
            this._exprTextBox.Name = "_exprTextBox";
            this._exprTextBox.Size = new Size(0x11a, 20);
            this._exprTextBox.TabIndex = 0xc9;
            this._exprTextBox.TextChanged += new EventHandler(this.OnExprTextBoxTextChanged);
            this._okButton.FlatStyle = FlatStyle.System;
            this._okButton.Location = new Point(360, 0x117);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 7;
            this._okButton.Click += new EventHandler(this.OnOKButtonClick);
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.FlatStyle = FlatStyle.System;
            this._cancelButton.Location = new Point(0x1b9, 0x117);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 8;
            this._refreshSchemaLink.Visible = false;
            this._refreshSchemaLink.Location = new Point(12, 0x11b);
            this._refreshSchemaLink.Name = "_refreshSchemaLink";
            this._refreshSchemaLink.Size = new Size(0xc5, 0x10);
            this._refreshSchemaLink.TabIndex = 6;
            this._refreshSchemaLink.TabStop = true;
            this._refreshSchemaLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnRefreshSchemaLinkLinkClicked);
            this._exprLabel.Location = new Point(0, 0);
            this._exprLabel.Name = "_exprLabel";
            this._exprLabel.Size = new Size(290, 0x10);
            this._exprLabel.TabIndex = 200;
            this._twoWayBindingCheckBox.FlatStyle = FlatStyle.System;
            this._twoWayBindingCheckBox.Location = new Point(0x76, 0x53);
            this._twoWayBindingCheckBox.Name = "_twoWayBindingCheckBox";
            this._twoWayBindingCheckBox.Size = new Size(0xa8, 30);
            this._twoWayBindingCheckBox.TabIndex = 0x6a;
            this._twoWayBindingCheckBox.Enabled = true;
            this._twoWayBindingCheckBox.CheckedChanged += new EventHandler(this.OnTwoWayBindingChecked);
            this._twoWayBindingCheckBox.TextAlign = ContentAlignment.TopLeft;
            this._twoWayBindingCheckBox.CheckAlign = ContentAlignment.TopLeft;
            this._fieldBindingRadio.Checked = true;
            this._fieldBindingRadio.FlatStyle = FlatStyle.System;
            this._fieldBindingRadio.Location = new Point(0, 0);
            this._fieldBindingRadio.Name = "_fieldBindingRadio";
            this._fieldBindingRadio.Size = new Size(0x12e, 0x12);
            this._fieldBindingRadio.TabIndex = 0;
            this._fieldBindingRadio.TabStop = true;
            this._fieldBindingRadio.CheckedChanged += new EventHandler(this.OnFieldBindingRadioCheckedChanged);
            this._exprBindingRadio.Location = new Point(0, 0x7f);
            this._exprBindingRadio.Name = "_exprBindingRadio";
            this._exprBindingRadio.Size = new Size(0x12e, 0x12);
            this._exprBindingRadio.TabIndex = 2;
            this._exprBindingRadio.CheckedChanged += new EventHandler(this.OnExprBindingRadioCheckedChanged);
            this._fieldBindingPanel.TabIndex = 1;
            this._fieldBindingPanel.Name = "_fieldBindingPanel";
            this._fieldBindingPanel.Location = new Point(0x10, 20);
            this._fieldBindingPanel.Size = new Size(0x11e, 0x69);
            this._fieldBindingPanel.Controls.Add(this._fieldLabel);
            this._fieldBindingPanel.Controls.Add(this._fieldCombo);
            this._fieldBindingPanel.Controls.Add(this._formatLabel);
            this._fieldBindingPanel.Controls.Add(this._formatCombo);
            this._fieldBindingPanel.Controls.Add(this._sampleLabel);
            this._fieldBindingPanel.Controls.Add(this._sampleTextBox);
            this._fieldBindingPanel.Controls.Add(this._twoWayBindingCheckBox);
            this._customBindingPanel.TabIndex = 3;
            this._customBindingPanel.Name = "_customBindingPanel";
            this._customBindingPanel.Location = new Point(0x10, 0x94);
            this._customBindingPanel.Size = new Size(0x11e, 0x36);
            this._customBindingPanel.Controls.Add(this._exprLabel);
            this._customBindingPanel.Controls.Add(this._exprTextBox);
            this._bindingOptionsPanel.TabIndex = 5;
            this._bindingOptionsPanel.Name = "_bindingOptionsPanel";
            this._bindingOptionsPanel.Location = new Point(0xd6, 0x4c);
            this._bindingOptionsPanel.Size = new Size(0x12e, 200);
            this._bindingOptionsPanel.Controls.Add(this._fieldBindingRadio);
            this._bindingOptionsPanel.Controls.Add(this._exprBindingRadio);
            this._bindingOptionsPanel.Controls.Add(this._fieldBindingPanel);
            this._bindingOptionsPanel.Controls.Add(this._customBindingPanel);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x20c, 0x13a);
            base.Controls.AddRange(new System.Windows.Forms.Control[] { this._refreshSchemaLink, this._cancelButton, this._okButton, this._bindingLabel, this._allPropsCheckBox, this._bindablePropsTree, this._bindablePropsLabels, this._instructionLabel, this._bindingOptionsPanel });
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "DataBindingsDialog";
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUserInterface()
        {
            this.Text = System.Design.SR.GetString("DBDlg_Text", new object[] { this.Control.Site.Name });
            this._instructionLabel.Text = System.Design.SR.GetString("DBDlg_Inst");
            this._bindablePropsLabels.Text = System.Design.SR.GetString("DBDlg_BindableProps");
            this._allPropsCheckBox.Text = System.Design.SR.GetString("DBDlg_ShowAll");
            this._fieldBindingRadio.Text = System.Design.SR.GetString("DBDlg_FieldBinding");
            this._fieldLabel.Text = System.Design.SR.GetString("DBDlg_Field");
            this._formatLabel.Text = System.Design.SR.GetString("DBDlg_Format");
            this._sampleLabel.Text = System.Design.SR.GetString("DBDlg_Sample");
            this._exprBindingRadio.Text = System.Design.SR.GetString("DBDlg_CustomBinding");
            this._okButton.Text = System.Design.SR.GetString("DBDlg_OK");
            this._cancelButton.Text = System.Design.SR.GetString("DBDlg_Cancel");
            this._refreshSchemaLink.Text = System.Design.SR.GetString("DBDlg_RefreshSchema");
            this._exprLabel.Text = System.Design.SR.GetString("DBDlg_Expr");
            this._twoWayBindingCheckBox.Text = System.Design.SR.GetString("DBDlg_TwoWay");
            ImageList list = new ImageList {
                TransparentColor = Color.Magenta,
                ColorDepth = ColorDepth.Depth32Bit
            };
            list.Images.AddStrip(new Bitmap(typeof(DataBindingsDialog), "BindableProperties.bmp"));
            this._bindablePropsTree.ImageList = list;
            bool flag = false;
            IDesignerHost service = (IDesignerHost) this.Control.Site.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                ControlDesigner designer = service.GetDesigner(this.Control) as ControlDesigner;
                if (designer != null)
                {
                    flag = this.ContainingTemplateIsBindable(designer);
                }
            }
            this._twoWayBindingCheckBox.Visible = flag;
        }

        private void LoadBindableProperties(bool showAll)
        {
            string text = string.Empty;
            if (this._bindablePropsTree.SelectedNode != null)
            {
                text = this._bindablePropsTree.SelectedNode.Text;
            }
            this._bindablePropsTree.Nodes.Clear();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.Control.GetType(), BindablePropertiesFilter);
            if (showAll)
            {
                PropertyDescriptorCollection descriptors2 = TypeDescriptor.GetProperties(this.Control.GetType(), BrowsablePropertiesFilter);
                if ((descriptors2 != null) && (descriptors2.Count > 0))
                {
                    int count = properties.Count;
                    int num2 = descriptors2.Count;
                    PropertyDescriptor[] array = new PropertyDescriptor[count + num2];
                    properties.CopyTo(array, 0);
                    int length = count;
                    foreach (PropertyDescriptor descriptor in descriptors2)
                    {
                        if (!properties.Contains(descriptor) && !string.Equals(descriptor.Name, "id", StringComparison.OrdinalIgnoreCase))
                        {
                            array[length++] = descriptor;
                        }
                    }
                    PropertyDescriptor[] destinationArray = new PropertyDescriptor[length];
                    Array.Copy(array, destinationArray, length);
                    properties = new PropertyDescriptorCollection(destinationArray);
                }
            }
            string name = null;
            ControlValuePropertyAttribute attribute = TypeDescriptor.GetAttributes(this.Control)[typeof(ControlValuePropertyAttribute)] as ControlValuePropertyAttribute;
            if (attribute != null)
            {
                name = attribute.Name;
            }
            else
            {
                PropertyDescriptor defaultProperty = TypeDescriptor.GetDefaultProperty(this.Control);
                if (defaultProperty != null)
                {
                    name = defaultProperty.Name;
                }
            }
            TreeNodeCollection nodes = this._bindablePropsTree.Nodes;
            TreeNode node = null;
            TreeNode node2 = null;
            this._bindablePropsTree.BeginUpdate();
            foreach (PropertyDescriptor descriptor3 in properties)
            {
                bool flag = this._bindings[descriptor3.Name] != null;
                BindingMode notSet = BindingMode.NotSet;
                if (flag)
                {
                    if (((DesignTimeDataBinding) this._bindings[descriptor3.Name]).IsTwoWayBound)
                    {
                        notSet = BindingMode.TwoWay;
                    }
                    else
                    {
                        notSet = BindingMode.OneWay;
                    }
                }
                TreeNode node3 = new BindablePropertyNode(descriptor3, notSet);
                if (descriptor3.Name.Equals(name))
                {
                    node = node3;
                }
                if (descriptor3.Name.Equals(text))
                {
                    node2 = node3;
                }
                nodes.Add(node3);
            }
            this._bindablePropsTree.EndUpdate();
            if (((node2 == null) && (node == null)) && (nodes.Count != 0))
            {
                int num4 = nodes.Count;
                for (int i = 0; i < num4; i++)
                {
                    BindablePropertyNode node4 = (BindablePropertyNode) nodes[i];
                    if (node4.IsBound)
                    {
                        node2 = node4;
                        break;
                    }
                }
                if (node2 == null)
                {
                    node2 = nodes[0];
                }
            }
            if (node2 != null)
            {
                this._bindablePropsTree.SelectedNode = node2;
            }
            else if (node != null)
            {
                this._bindablePropsTree.SelectedNode = node;
            }
            this.UpdateUIState();
        }

        private void LoadCurrentDataBinding()
        {
            this._internalChange = true;
            try
            {
                this._fieldBindingRadio.Checked = this._fieldsAvailable;
                this._bindingLabel.Text = string.Empty;
                this._fieldCombo.SelectedIndex = -1;
                this._formatCombo.Text = string.Empty;
                this._sampleTextBox.Text = string.Empty;
                this._exprBindingRadio.Checked = !this._fieldsAvailable;
                this._exprTextBox.Text = string.Empty;
                this._twoWayBindingCheckBox.Checked = false;
                this._formatDirty = false;
                if (this._currentNode != null)
                {
                    this._bindingLabel.Text = System.Design.SR.GetString("DBDlg_BindingGroup", new object[] { this._currentNode.PropertyDescriptor.Name });
                    this._twoWayBindingCheckBox.Checked = this._currentNode.TwoWayBoundByDefault && this._twoWayBindingCheckBox.Visible;
                    if (this._currentDataBinding != null)
                    {
                        bool flag = true;
                        if (this._fieldsAvailable && !this._currentDataBinding.IsCustom)
                        {
                            string field = this._currentDataBinding.Field;
                            string format = this._currentDataBinding.Format;
                            field = field.TrimStart(new char[] { '[' }).TrimEnd(new char[] { ']' });
                            int num = this._fieldCombo.FindStringExact(field, 1);
                            if (num != -1)
                            {
                                flag = false;
                                this._fieldCombo.SelectedIndex = num;
                                this.UpdateFormatItems();
                                bool flag2 = false;
                                foreach (FormatItem item in this._formatCombo.Items)
                                {
                                    if (item.Format.Equals(format))
                                    {
                                        flag2 = true;
                                        this._formatCombo.SelectedItem = item;
                                    }
                                }
                                if (!flag2)
                                {
                                    this._formatCombo.Text = format;
                                }
                                this.UpdateFormatSample();
                                if (this._currentNode.BindingMode == BindingMode.TwoWay)
                                {
                                    this._twoWayBindingCheckBox.Checked = true;
                                }
                                else if (this._currentNode.BindingMode == BindingMode.OneWay)
                                {
                                    this._twoWayBindingCheckBox.Checked = false;
                                }
                            }
                        }
                        if (flag)
                        {
                            this._exprBindingRadio.Checked = true;
                            this._exprTextBox.Text = this._currentDataBinding.Expression;
                        }
                        else
                        {
                            this.UpdateExpression();
                        }
                    }
                }
            }
            finally
            {
                this._internalChange = false;
                this.UpdateUIState();
            }
        }

        private void LoadDataBindings()
        {
            this._bindings = new Hashtable();
            foreach (DataBinding binding in ((IDataBindingsAccessor) this.Control).DataBindings)
            {
                this._bindings[binding.PropertyName] = new DesignTimeDataBinding(binding);
            }
        }

        private void LoadFields()
        {
            this._fieldCombo.Items.Clear();
            ArrayList fields = new ArrayList();
            fields.Add(new FieldItem());
            IDesigner namingContainerDesigner = null;
            IDesignerHost host = (IDesignerHost) this.Control.Site.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                ControlDesigner designer2 = host.GetDesigner(this.Control) as ControlDesigner;
                if (designer2 != null)
                {
                    namingContainerDesigner = this.GetNamingContainerDesigner(designer2);
                }
            }
            if (namingContainerDesigner != null)
            {
                IDataBindingSchemaProvider provider = namingContainerDesigner as IDataBindingSchemaProvider;
                if (provider != null)
                {
                    if (provider.CanRefreshSchema)
                    {
                        this._refreshSchemaLink.Visible = true;
                    }
                    IDataSourceViewSchema schema = null;
                    try
                    {
                        schema = provider.Schema;
                    }
                    catch (Exception exception)
                    {
                        IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.ServiceProvider.GetService(typeof(IComponentDesignerDebugService));
                        if (service != null)
                        {
                            service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                        }
                    }
                    this.ExtractFields(schema, fields);
                }
                else if (namingContainerDesigner is IDataSourceProvider)
                {
                    this.ExtractFields((IDataSourceProvider) namingContainerDesigner, fields);
                }
            }
            this._fieldCombo.Items.AddRange(fields.ToArray());
            this._fieldsAvailable = fields.Count > 1;
        }

        private void OnBindablePropsTreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (this._currentDataBindingDirty)
            {
                this.SaveCurrentDataBinding();
            }
            this._currentDataBinding = null;
            this._currentNode = (BindablePropertyNode) this._bindablePropsTree.SelectedNode;
            if (this._currentNode != null)
            {
                this._currentDataBinding = (DesignTimeDataBinding) this._bindings[this._currentNode.PropertyDescriptor.Name];
            }
            this.LoadCurrentDataBinding();
        }

        private void OnExprBindingRadioCheckedChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._currentDataBindingDirty = true;
                this.UpdateUIState();
            }
        }

        private void OnExprTextBoxTextChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._currentDataBindingDirty = true;
            }
        }

        private void OnFieldBindingRadioCheckedChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._currentDataBindingDirty = true;
                if (this._fieldBindingRadio.Checked)
                {
                    this.UpdateExpression();
                }
                this.UpdateUIState();
            }
        }

        private void OnFieldComboSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._currentDataBindingDirty = true;
                this.UpdateFormatItems();
                this.UpdateExpression();
                this.UpdateUIState();
            }
        }

        private void OnFormatComboLostFocus(object sender, EventArgs e)
        {
            if (this._formatDirty)
            {
                this._formatDirty = false;
                this.UpdateFormatSample();
                this.UpdateExpression();
            }
        }

        private void OnFormatComboSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._formatDirty = true;
                this.UpdateFormatSample();
                this.UpdateExpression();
            }
        }

        private void OnFormatComboTextChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._formatDirty = true;
            }
        }

        protected override void OnInitialActivated(EventArgs e)
        {
            base.OnInitialActivated(e);
            this.LoadDataBindings();
            this.LoadFields();
            this.LoadBindableProperties(false);
        }

        private void OnOKButtonClick(object sender, EventArgs e)
        {
            if (this._currentDataBindingDirty)
            {
                this.SaveCurrentDataBinding();
            }
            if (this._bindingsDirty)
            {
                this.SaveDataBindings();
            }
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void OnRefreshSchemaLinkLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this._currentDataBindingDirty)
            {
                this.SaveCurrentDataBinding();
            }
            IDesigner namingContainerDesigner = null;
            IDesignerHost service = (IDesignerHost) this.Control.Site.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                ControlDesigner designer2 = service.GetDesigner(this.Control) as ControlDesigner;
                if (designer2 != null)
                {
                    namingContainerDesigner = this.GetNamingContainerDesigner(designer2);
                }
            }
            if (namingContainerDesigner != null)
            {
                IDataBindingSchemaProvider provider = namingContainerDesigner as IDataBindingSchemaProvider;
                if (provider != null)
                {
                    provider.RefreshSchema(false);
                }
            }
            this.LoadFields();
            if (this._currentNode != null)
            {
                this._currentDataBinding = (DesignTimeDataBinding) this._bindings[this._currentNode.PropertyDescriptor.Name];
            }
            this.LoadCurrentDataBinding();
        }

        private void OnShowAllCheckedChanged(object sender, EventArgs e)
        {
            this.LoadBindableProperties(this._allPropsCheckBox.Checked);
        }

        private void OnTwoWayBindingChecked(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._currentDataBindingDirty = true;
                this.UpdateExpression();
                this.UpdateUIState();
            }
        }

        private void SaveCurrentDataBinding()
        {
            DesignTimeDataBinding binding = null;
            if (this._fieldBindingRadio.Checked)
            {
                if (this._fieldCombo.SelectedIndex > 0)
                {
                    string text = this._fieldCombo.Text;
                    string format = this.SaveFormat();
                    binding = new DesignTimeDataBinding(this._currentNode.PropertyDescriptor, text, format, this._twoWayBindingCheckBox.Checked);
                }
            }
            else
            {
                string expression = this._exprTextBox.Text.Trim();
                if (expression.Length != 0)
                {
                    binding = new DesignTimeDataBinding(this._currentNode.PropertyDescriptor, expression);
                }
            }
            if (binding == null)
            {
                this._currentNode.BindingMode = BindingMode.NotSet;
                this._bindings.Remove(this._currentNode.PropertyDescriptor.Name);
            }
            else
            {
                if (this._fieldBindingRadio.Checked)
                {
                    if (this._twoWayBindingCheckBox.Checked && this._twoWayBindingCheckBox.Visible)
                    {
                        this._currentNode.BindingMode = BindingMode.TwoWay;
                    }
                    else
                    {
                        this._currentNode.BindingMode = BindingMode.OneWay;
                    }
                }
                else if (binding.IsTwoWayBound)
                {
                    this._currentNode.BindingMode = BindingMode.TwoWay;
                }
                else
                {
                    this._currentNode.BindingMode = BindingMode.OneWay;
                }
                this._bindings[this._currentNode.PropertyDescriptor.Name] = binding;
            }
            this._currentDataBindingDirty = false;
            this._bindingsDirty = true;
        }

        private void SaveDataBindings()
        {
            DataBindingCollection dataBindings = ((IDataBindingsAccessor) this.Control).DataBindings;
            ExpressionBindingCollection expressions = ((IExpressionsAccessor) this.Control).Expressions;
            dataBindings.Clear();
            foreach (DesignTimeDataBinding binding in this._bindings.Values)
            {
                dataBindings.Add(binding.RuntimeDataBinding);
                expressions.Remove(binding.RuntimeDataBinding.PropertyName);
            }
            this._bindingsDirty = false;
        }

        private string SaveFormat()
        {
            string text = string.Empty;
            FormatItem selectedItem = this._formatCombo.SelectedItem as FormatItem;
            if (selectedItem != null)
            {
                return selectedItem.Format;
            }
            text = this._formatCombo.Text;
            string str2 = text.Trim();
            if (str2.Length == 0)
            {
                text = str2;
            }
            return text;
        }

        private void UpdateExpression()
        {
            string str = string.Empty;
            if (this._fieldCombo.SelectedIndex > 0)
            {
                string text = this._fieldCombo.Text;
                string format = this.SaveFormat();
                if (this._twoWayBindingCheckBox.Checked)
                {
                    str = DesignTimeDataBinding.CreateBindExpression(text, format);
                }
                else
                {
                    str = DesignTimeDataBinding.CreateEvalExpression(text, format);
                }
            }
            this._exprTextBox.Text = str;
        }

        private void UpdateFormatItems()
        {
            FormatItem[] defaultFormats = FormatItem.DefaultFormats;
            this._formatSampleObject = null;
            this._formatCombo.SelectedIndex = -1;
            this._formatCombo.Text = string.Empty;
            FieldItem selectedItem = (FieldItem) this._fieldCombo.SelectedItem;
            if ((selectedItem != null) && (selectedItem.Type != null))
            {
                switch (System.Type.GetTypeCode(selectedItem.Type))
                {
                    case TypeCode.SByte:
                    case TypeCode.Byte:
                    case TypeCode.Int16:
                    case TypeCode.UInt16:
                    case TypeCode.Int32:
                    case TypeCode.UInt32:
                    case TypeCode.Int64:
                    case TypeCode.UInt64:
                        defaultFormats = FormatItem.NumericFormats;
                        this._formatSampleObject = 1;
                        break;

                    case TypeCode.Single:
                    case TypeCode.Double:
                    case TypeCode.Decimal:
                        defaultFormats = FormatItem.DecimalFormats;
                        this._formatSampleObject = 1;
                        break;

                    case TypeCode.DateTime:
                        defaultFormats = FormatItem.DateTimeFormats;
                        this._formatSampleObject = DateTime.Today;
                        break;

                    case TypeCode.String:
                        this._formatSampleObject = "abc";
                        break;
                }
            }
            this._formatCombo.Items.Clear();
            this._formatCombo.Items.AddRange(defaultFormats);
        }

        private void UpdateFormatSample()
        {
            string str = string.Empty;
            if (this._formatSampleObject != null)
            {
                string format = this.SaveFormat();
                if (format.Length != 0)
                {
                    try
                    {
                        str = string.Format(CultureInfo.CurrentCulture, format, new object[] { this._formatSampleObject });
                    }
                    catch
                    {
                        str = System.Design.SR.GetString("DBDlg_InvalidFormat");
                    }
                }
            }
            this._sampleTextBox.Text = str;
        }

        private void UpdateUIState()
        {
            if (this._currentNode == null)
            {
                this._fieldBindingRadio.Enabled = false;
                this._fieldCombo.Enabled = false;
                this._formatCombo.Enabled = false;
                this._sampleTextBox.Enabled = false;
                this._fieldLabel.Enabled = false;
                this._formatLabel.Enabled = false;
                this._sampleLabel.Enabled = false;
                this._twoWayBindingCheckBox.Enabled = false;
                this._exprBindingRadio.Enabled = false;
                this._exprTextBox.Enabled = false;
            }
            else
            {
                this._fieldBindingRadio.Enabled = this._fieldsAvailable;
                this._exprBindingRadio.Enabled = true;
                bool flag = this._fieldBindingRadio.Checked;
                bool flag2 = flag && (this._fieldCombo.SelectedIndex > 0);
                bool flag3 = flag2 && (this._currentNode.PropertyDescriptor.PropertyType == typeof(string));
                this._fieldCombo.Enabled = flag;
                this._fieldLabel.Enabled = flag;
                this._formatCombo.Enabled = flag3;
                this._formatLabel.Enabled = flag3;
                this._sampleTextBox.Enabled = flag3;
                this._sampleLabel.Enabled = flag3;
                this._twoWayBindingCheckBox.Enabled = flag2;
                this._exprTextBox.Enabled = !flag;
            }
        }

        private System.Web.UI.Control Control
        {
            get
            {
                IServiceProvider serviceProvider = base.ServiceProvider;
                if (serviceProvider != null)
                {
                    IContainer service = null;
                    ISite site = serviceProvider as ISite;
                    IContainer container2 = null;
                    if (site != null)
                    {
                        container2 = site.Container;
                    }
                    if ((container2 != null) && (container2 is NestedContainer))
                    {
                        service = container2;
                    }
                    else
                    {
                        service = (IContainer) serviceProvider.GetService(typeof(IContainer));
                    }
                    if (service != null)
                    {
                        return (service.Components[this._controlID] as System.Web.UI.Control);
                    }
                }
                return null;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.DataBinding.BindingsDialog";
            }
        }

        private sealed class BindablePropertyNode : TreeNode
        {
            private System.Web.UI.Design.DataBindingsDialog.BindingMode _bindingMode;
            private System.ComponentModel.PropertyDescriptor _propDesc;
            private bool _twoWayBoundByDefault;
            private bool _twoWayBoundByDefaultValid;

            public BindablePropertyNode(System.ComponentModel.PropertyDescriptor propDesc, System.Web.UI.Design.DataBindingsDialog.BindingMode bindingMode)
            {
                this._propDesc = propDesc;
                this._bindingMode = bindingMode;
                base.Text = propDesc.Name;
                int num = 0;
                if (bindingMode == System.Web.UI.Design.DataBindingsDialog.BindingMode.OneWay)
                {
                    num = 1;
                }
                else if (bindingMode == System.Web.UI.Design.DataBindingsDialog.BindingMode.TwoWay)
                {
                    num = 2;
                }
                base.ImageIndex = base.SelectedImageIndex = num;
            }

            public System.Web.UI.Design.DataBindingsDialog.BindingMode BindingMode
            {
                get
                {
                    return this._bindingMode;
                }
                set
                {
                    this._bindingMode = value;
                    int num = 0;
                    if (this._bindingMode == System.Web.UI.Design.DataBindingsDialog.BindingMode.OneWay)
                    {
                        num = 1;
                    }
                    else if (this._bindingMode == System.Web.UI.Design.DataBindingsDialog.BindingMode.TwoWay)
                    {
                        num = 2;
                    }
                    base.ImageIndex = base.SelectedImageIndex = num;
                }
            }

            public bool IsBound
            {
                get
                {
                    if (this._bindingMode != System.Web.UI.Design.DataBindingsDialog.BindingMode.OneWay)
                    {
                        return (this._bindingMode == System.Web.UI.Design.DataBindingsDialog.BindingMode.TwoWay);
                    }
                    return true;
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return this._propDesc;
                }
            }

            public bool TwoWayBoundByDefault
            {
                get
                {
                    if (!this._twoWayBoundByDefaultValid)
                    {
                        BindableAttribute attribute = this._propDesc.Attributes[typeof(BindableAttribute)] as BindableAttribute;
                        if (attribute != null)
                        {
                            this._twoWayBoundByDefault = attribute.Direction == BindingDirection.TwoWay;
                        }
                        this._twoWayBoundByDefaultValid = true;
                    }
                    return this._twoWayBoundByDefault;
                }
            }
        }

        private enum BindingMode
        {
            NotSet,
            OneWay,
            TwoWay
        }

        private sealed class FieldItem
        {
            private string _name;
            private System.Type _type;

            public FieldItem() : this(System.Design.SR.GetString("DBDlg_Unbound"), null)
            {
            }

            public FieldItem(string name, System.Type type)
            {
                this._name = name;
                this._type = type;
            }

            public override string ToString()
            {
                return this._name;
            }

            public System.Type Type
            {
                get
                {
                    return this._type;
                }
            }
        }

        private class FormatItem
        {
            private readonly string _displayText;
            private readonly string _format;
            public static readonly DataBindingsDialog.FormatItem[] DateTimeFormats = new DataBindingsDialog.FormatItem[] { nullFormat, generalFormat, dtShortTime, dtLongTime, dtShortDate, dtLongDate, dtDateTime, dtFullDate };
            public static readonly DataBindingsDialog.FormatItem[] DecimalFormats = new DataBindingsDialog.FormatItem[] { nullFormat, generalFormat, numNumber, numDecimal, numFixed, numCurrency, numScientific };
            public static readonly DataBindingsDialog.FormatItem[] DefaultFormats = new DataBindingsDialog.FormatItem[] { nullFormat, generalFormat };
            private static readonly DataBindingsDialog.FormatItem dtDateTime = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_DateTime"), "{0:g}");
            private static readonly DataBindingsDialog.FormatItem dtFullDate = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_FullDate"), "{0:G}");
            private static readonly DataBindingsDialog.FormatItem dtLongDate = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_LongDate"), "{0:D}");
            private static readonly DataBindingsDialog.FormatItem dtLongTime = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_LongTime"), "{0:T}");
            private static readonly DataBindingsDialog.FormatItem dtShortDate = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_ShortDate"), "{0:d}");
            private static readonly DataBindingsDialog.FormatItem dtShortTime = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_ShortTime"), "{0:t}");
            private static readonly DataBindingsDialog.FormatItem generalFormat = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_General"), "{0}");
            private static readonly DataBindingsDialog.FormatItem nullFormat = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_None"), string.Empty);
            private static readonly DataBindingsDialog.FormatItem numCurrency = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_Currency"), "{0:C}");
            private static readonly DataBindingsDialog.FormatItem numDecimal = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_Decimal"), "{0:D}");
            public static readonly DataBindingsDialog.FormatItem[] NumericFormats = new DataBindingsDialog.FormatItem[] { nullFormat, generalFormat, numNumber, numDecimal, numFixed, numCurrency, numScientific, numHex };
            private static readonly DataBindingsDialog.FormatItem numFixed = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_Fixed"), "{0:F}");
            private static readonly DataBindingsDialog.FormatItem numHex = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_Hexadecimal"), "0x{0:X}");
            private static readonly DataBindingsDialog.FormatItem numNumber = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_Numeric"), "{0:N}");
            private static readonly DataBindingsDialog.FormatItem numScientific = new DataBindingsDialog.FormatItem(System.Design.SR.GetString("DBDlg_Fmt_Scientific"), "{0:E}");

            private FormatItem(string displayText, string format)
            {
                this._displayText = string.Format(CultureInfo.CurrentCulture, displayText, new object[] { format });
                this._format = format;
            }

            public override string ToString()
            {
                return this._displayText;
            }

            public string Format
            {
                get
                {
                    return this._format;
                }
            }
        }
    }
}

