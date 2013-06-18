namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Design;
    using System.Drawing;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class ExpressionBindingsDialog : DesignerForm
    {
        private Label _bindablePropsLabels;
        private TreeView _bindablePropsTree;
        private bool _bindingsDirty;
        private Button _cancelButton;
        private IDictionary _complexBindings;
        private System.Web.UI.Control _control;
        private string _controlID;
        private ExpressionEditor _currentEditor;
        private BindablePropertyNode _currentNode;
        private ExpressionEditorSheet _currentSheet;
        private AutoSizeComboBox _expressionBuilderComboBox;
        private Label _expressionBuilderLabel;
        private PropertyGrid _expressionBuilderPropertyGrid;
        private IDictionary _expressionEditors;
        private Label _generatedHelpLabel;
        private Label _instructionLabel;
        private bool _internalChange;
        private ExpressionItem _noneItem;
        private Button _okButton;
        private Panel _propertiesPanel;
        private Label _propertyGridLabel;
        private static readonly Attribute[] BindablePropertiesFilter = new Attribute[] { BrowsableAttribute.Yes, ReadOnlyAttribute.No };
        private const int BoundImageIndex = 1;
        private const int ImplicitBoundImageIndex = 2;
        private const int UnboundImageIndex = 0;

        public ExpressionBindingsDialog(IServiceProvider serviceProvider, System.Web.UI.Control control) : base(serviceProvider)
        {
            this._control = control;
            this._controlID = control.ID;
            this.InitializeComponent();
            this.InitializeUserInterface();
        }

        private void InitializeComponent()
        {
            this._instructionLabel = new Label();
            this._bindablePropsLabels = new Label();
            this._bindablePropsTree = new TreeView();
            this._okButton = new Button();
            this._cancelButton = new Button();
            this._expressionBuilderComboBox = new AutoSizeComboBox();
            this._expressionBuilderPropertyGrid = new VsPropertyGrid(base.ServiceProvider);
            this._expressionBuilderLabel = new Label();
            this._propertyGridLabel = new Label();
            this._propertiesPanel = new Panel();
            this._generatedHelpLabel = new Label();
            base.SuspendLayout();
            this._instructionLabel.Location = new Point(12, 12);
            this._instructionLabel.Name = "_instructionLabel";
            this._instructionLabel.Size = new Size(0x1dc, 0x31);
            this._instructionLabel.TabIndex = 0;
            this._bindablePropsLabels.Location = new Point(12, 0x41);
            this._bindablePropsLabels.Name = "_bindablePropsLabels";
            this._bindablePropsLabels.Size = new Size(0xc4, 0x10);
            this._bindablePropsLabels.TabIndex = 1;
            this._bindablePropsTree.HideSelection = false;
            this._bindablePropsTree.ImageIndex = -1;
            this._bindablePropsTree.Location = new Point(12, 0x53);
            this._bindablePropsTree.Name = "_bindablePropsTree";
            this._bindablePropsTree.SelectedImageIndex = -1;
            this._bindablePropsTree.Sorted = true;
            this._bindablePropsTree.ShowLines = false;
            this._bindablePropsTree.ShowPlusMinus = false;
            this._bindablePropsTree.ShowRootLines = false;
            this._bindablePropsTree.Size = new Size(0xc4, 0xb6);
            this._bindablePropsTree.TabIndex = 2;
            this._bindablePropsTree.AfterSelect += new TreeViewEventHandler(this.OnBindablePropsTreeAfterSelect);
            this._okButton.Location = new Point(0x138, 0x113);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 0x10;
            this._okButton.Size = new Size(0x55, 0x17);
            this._okButton.Click += new EventHandler(this.OnOKButtonClick);
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x193, 0x113);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new Size(0x55, 0x17);
            this._cancelButton.TabIndex = 0x11;
            this._expressionBuilderLabel.Location = new Point(0, 0);
            this._expressionBuilderLabel.Name = "_expressionBuilderLabel";
            this._expressionBuilderLabel.Size = new Size(0x10c, 0x10);
            this._expressionBuilderLabel.TabIndex = 10;
            this._expressionBuilderComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._expressionBuilderComboBox.Location = new Point(0, 0x12);
            this._expressionBuilderComboBox.Name = "_expressionBuilderComboBox";
            this._expressionBuilderComboBox.TabIndex = 20;
            this._expressionBuilderComboBox.Size = new Size(0x10c, 0x15);
            this._expressionBuilderComboBox.Sorted = true;
            this._expressionBuilderComboBox.SelectedIndexChanged += new EventHandler(this.OnExpressionBuilderComboBoxSelectedIndexChanged);
            this._propertyGridLabel.Location = new Point(0, 0x2b);
            this._propertyGridLabel.Name = "_propertyGridLabel";
            this._propertyGridLabel.Size = new Size(0x10c, 0x10);
            this._propertyGridLabel.TabIndex = 30;
            this._expressionBuilderPropertyGrid.Location = new Point(0, 0x3d);
            this._expressionBuilderPropertyGrid.Name = "_expressionBuilderPropertyGrid";
            this._expressionBuilderPropertyGrid.TabIndex = 40;
            this._expressionBuilderPropertyGrid.Size = new Size(0x10c, 0x8b);
            this._expressionBuilderPropertyGrid.PropertySort = PropertySort.Alphabetical;
            this._expressionBuilderPropertyGrid.ToolbarVisible = false;
            this._expressionBuilderPropertyGrid.PropertyValueChanged += new PropertyValueChangedEventHandler(this.OnExpressionBuilderPropertyGridPropertyValueChanged);
            this._expressionBuilderPropertyGrid.Site = this._control.Site;
            this._propertiesPanel.Location = new Point(220, 0x41);
            this._propertiesPanel.Name = "_propertiesPanel";
            this._propertiesPanel.Size = new Size(0x10c, 200);
            this._propertiesPanel.TabIndex = 5;
            this._propertiesPanel.Controls.AddRange(new System.Windows.Forms.Control[] { this._expressionBuilderLabel, this._expressionBuilderComboBox, this._propertyGridLabel, this._expressionBuilderPropertyGrid });
            this._generatedHelpLabel.Location = new Point(220, 0x48);
            this._generatedHelpLabel.Name = "_generatedHelpLabel";
            this._generatedHelpLabel.Size = new Size(0x10c, 180);
            this._generatedHelpLabel.TabIndex = 5;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(500, 310);
            base.Controls.AddRange(new System.Windows.Forms.Control[] { this._cancelButton, this._okButton, this._propertiesPanel, this._bindablePropsTree, this._bindablePropsLabels, this._instructionLabel, this._generatedHelpLabel });
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "ExpressionBindingsDialog";
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUserInterface()
        {
            string name = string.Empty;
            if ((this.Control != null) && (this.Control.Site != null))
            {
                name = this.Control.Site.Name;
            }
            this.Text = System.Design.SR.GetString("ExpressionBindingsDialog_Text", new object[] { name });
            this._instructionLabel.Text = System.Design.SR.GetString("ExpressionBindingsDialog_Inst");
            this._bindablePropsLabels.Text = System.Design.SR.GetString("ExpressionBindingsDialog_BindableProps");
            this._okButton.Text = System.Design.SR.GetString("ExpressionBindingsDialog_OK");
            this._cancelButton.Text = System.Design.SR.GetString("ExpressionBindingsDialog_Cancel");
            this._expressionBuilderLabel.Text = System.Design.SR.GetString("ExpressionBindingsDialog_ExpressionType");
            this._propertyGridLabel.Text = System.Design.SR.GetString("ExpressionBindingsDialog_Properties");
            this._generatedHelpLabel.Text = System.Design.SR.GetString("ExpressionBindingsDialog_GeneratedExpression");
            ImageList list = new ImageList {
                TransparentColor = Color.Fuchsia,
                ColorDepth = ColorDepth.Depth32Bit
            };
            list.Images.AddStrip(new Bitmap(typeof(ExpressionBindingsDialog), "ExpressionBindableProperties.bmp"));
            this._bindablePropsTree.ImageList = list;
        }

        private void LoadBindableProperties()
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.Control, BindablePropertiesFilter);
            string name = null;
            PropertyDescriptor defaultProperty = TypeDescriptor.GetDefaultProperty(this.Control);
            if (defaultProperty != null)
            {
                name = defaultProperty.Name;
            }
            TreeNodeCollection nodes = this._bindablePropsTree.Nodes;
            ExpressionBindingCollection expressions = ((IExpressionsAccessor) this.Control).Expressions;
            Hashtable hashtable = new Hashtable(StringComparer.OrdinalIgnoreCase);
            foreach (ExpressionBinding binding in expressions)
            {
                hashtable[binding.PropertyName] = binding;
            }
            TreeNode node = null;
            foreach (PropertyDescriptor descriptor2 in properties)
            {
                if (string.Compare(descriptor2.Name, "ID", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    ExpressionBinding binding2 = null;
                    if (hashtable.Contains(descriptor2.Name))
                    {
                        binding2 = (ExpressionBinding) hashtable[descriptor2.Name];
                        hashtable.Remove(descriptor2.Name);
                    }
                    TreeNode node2 = new BindablePropertyNode(descriptor2, binding2);
                    if (descriptor2.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        node = node2;
                    }
                    nodes.Add(node2);
                }
            }
            this._complexBindings = hashtable;
            if ((node == null) && (nodes.Count != 0))
            {
                int count = nodes.Count;
                for (int i = 0; i < count; i++)
                {
                    BindablePropertyNode node3 = (BindablePropertyNode) nodes[i];
                    if (node3.IsBound)
                    {
                        node = node3;
                        break;
                    }
                }
                if (node == null)
                {
                    node = nodes[0];
                }
            }
            if (node != null)
            {
                this._bindablePropsTree.SelectedNode = node;
            }
        }

        private void LoadExpressionEditors()
        {
            this._expressionEditors = new HybridDictionary(true);
            IWebApplication service = (IWebApplication) base.ServiceProvider.GetService(typeof(IWebApplication));
            if (service != null)
            {
                try
                {
                    System.Configuration.Configuration configuration = service.OpenWebConfiguration(true);
                    if (configuration != null)
                    {
                        CompilationSection section = (CompilationSection) configuration.GetSection("system.web/compilation");
                        foreach (ExpressionBuilder builder in section.ExpressionBuilders)
                        {
                            string expressionPrefix = builder.ExpressionPrefix;
                            ExpressionEditor expressionEditor = ExpressionEditor.GetExpressionEditor(expressionPrefix, base.ServiceProvider);
                            if (expressionEditor != null)
                            {
                                this._expressionEditors[expressionPrefix] = expressionEditor;
                                this._expressionBuilderComboBox.Items.Add(new ExpressionItem(expressionPrefix));
                            }
                        }
                    }
                }
                catch
                {
                }
                this._expressionBuilderComboBox.InvalidateDropDownWidth();
            }
            this._expressionBuilderComboBox.Items.Add(this.NoneItem);
        }

        private void OnBindablePropsTreeAfterSelect(object sender, TreeViewEventArgs e)
        {
            BindablePropertyNode selectedNode = (BindablePropertyNode) this._bindablePropsTree.SelectedNode;
            if (this._currentNode != selectedNode)
            {
                this._currentNode = selectedNode;
                if ((this._currentNode != null) && this._currentNode.IsBound)
                {
                    ExpressionBinding binding = this._currentNode.Binding;
                    if (!this._currentNode.IsGenerated)
                    {
                        ExpressionEditor editor = (ExpressionEditor) this._expressionEditors[binding.ExpressionPrefix];
                        if (editor == null)
                        {
                            UIServiceHelper.ShowMessage(base.ServiceProvider, System.Design.SR.GetString("ExpressionBindingsDialog_UndefinedExpressionPrefix", new object[] { binding.ExpressionPrefix }), System.Design.SR.GetString("ExpressionBindingsDialog_Text", new object[] { this.Control.Site.Name }), MessageBoxButtons.OK);
                            editor = new GenericExpressionEditor();
                        }
                        this._currentEditor = editor;
                        this._currentSheet = this._currentEditor.GetExpressionEditorSheet(binding.Expression, base.ServiceProvider);
                        this._internalChange = true;
                        try
                        {
                            foreach (ExpressionItem item in this._expressionBuilderComboBox.Items)
                            {
                                if (string.Equals(item.ToString(), binding.ExpressionPrefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    this._expressionBuilderComboBox.SelectedItem = item;
                                }
                            }
                            this._currentNode.IsValid = this._currentSheet.IsValid;
                        }
                        finally
                        {
                            this._internalChange = false;
                        }
                    }
                }
                else
                {
                    this._expressionBuilderComboBox.SelectedItem = this.NoneItem;
                    this._currentEditor = null;
                    this._currentSheet = null;
                }
                this._expressionBuilderPropertyGrid.SelectedObject = this._currentSheet;
                this.UpdateUIState();
            }
        }

        private void OnExpressionBuilderComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (!this._internalChange)
            {
                this._currentSheet = null;
                if (this._expressionBuilderComboBox.SelectedItem != this.NoneItem)
                {
                    this._currentEditor = (ExpressionEditor) this._expressionEditors[this._expressionBuilderComboBox.SelectedItem.ToString()];
                    if (this._currentNode != null)
                    {
                        if (this._currentNode.IsBound)
                        {
                            ExpressionBinding binding = this._currentNode.Binding;
                            if (this._expressionEditors[binding.ExpressionPrefix] == this._currentEditor)
                            {
                                this._currentSheet = this._currentEditor.GetExpressionEditorSheet(binding.Expression, base.ServiceProvider);
                            }
                        }
                        if (this._currentSheet == null)
                        {
                            this._currentSheet = this._currentEditor.GetExpressionEditorSheet(string.Empty, base.ServiceProvider);
                        }
                        this._currentNode.IsValid = this._currentSheet.IsValid;
                    }
                }
                this.SaveCurrentExpressionBinding();
                this._expressionBuilderPropertyGrid.SelectedObject = this._currentSheet;
                this.UpdateUIState();
            }
        }

        private void OnExpressionBuilderPropertyGridPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            this.SaveCurrentExpressionBinding();
            this.UpdateUIState();
        }

        protected override void OnInitialActivated(EventArgs e)
        {
            base.OnInitialActivated(e);
            this.LoadExpressionEditors();
            this.LoadBindableProperties();
            this.UpdateUIState();
        }

        private void OnOKButtonClick(object sender, EventArgs e)
        {
            if (this._bindingsDirty)
            {
                ExpressionBindingCollection expressions = ((IExpressionsAccessor) this.Control).Expressions;
                DataBindingCollection dataBindings = ((IDataBindingsAccessor) this.Control).DataBindings;
                expressions.Clear();
                foreach (BindablePropertyNode node in this._bindablePropsTree.Nodes)
                {
                    if (node.IsBound)
                    {
                        expressions.Add(node.Binding);
                        if (dataBindings.Contains(node.Binding.PropertyName))
                        {
                            dataBindings.Remove(node.Binding.PropertyName);
                        }
                    }
                }
                foreach (ExpressionBinding binding in this._complexBindings.Values)
                {
                    expressions.Add(binding);
                }
            }
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void SaveCurrentExpressionBinding()
        {
            if (this._expressionBuilderComboBox.SelectedItem == this.NoneItem)
            {
                this._currentNode.Binding = null;
                this._currentNode.IsValid = true;
            }
            else
            {
                string expression = this._currentSheet.GetExpression();
                PropertyDescriptor propertyDescriptor = this._currentNode.PropertyDescriptor;
                ExpressionBinding binding = new ExpressionBinding(propertyDescriptor.Name, propertyDescriptor.PropertyType, this._expressionBuilderComboBox.SelectedItem.ToString(), expression);
                this._currentNode.Binding = binding;
                this._currentNode.IsValid = this._currentSheet.IsValid;
            }
            this._bindingsDirty = true;
        }

        private void UpdateUIState()
        {
            if (this._currentNode == null)
            {
                this._expressionBuilderComboBox.Enabled = false;
                this._expressionBuilderPropertyGrid.Enabled = false;
                this._propertiesPanel.Visible = true;
                this._generatedHelpLabel.Visible = false;
            }
            else
            {
                this._expressionBuilderComboBox.Enabled = true;
                bool flag = this._expressionBuilderComboBox.SelectedItem == this.NoneItem;
                this._expressionBuilderPropertyGrid.Enabled = !flag;
                this._propertyGridLabel.Enabled = !flag;
                this._propertiesPanel.Visible = !this._currentNode.IsGenerated;
                this._generatedHelpLabel.Visible = this._currentNode.IsGenerated;
            }
            this._okButton.Enabled = true;
            foreach (BindablePropertyNode node in this._bindablePropsTree.Nodes)
            {
                if (!node.IsValid)
                {
                    this._okButton.Enabled = false;
                    break;
                }
            }
        }

        private System.Web.UI.Control Control
        {
            get
            {
                return this._control;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.Expressions.BindingsDialog";
            }
        }

        private ExpressionItem NoneItem
        {
            get
            {
                if (this._noneItem == null)
                {
                    this._noneItem = new ExpressionItem(System.Design.SR.GetString("ExpressionBindingsDialog_None"));
                }
                return this._noneItem;
            }
        }

        private sealed class BindablePropertyNode : TreeNode
        {
            private ExpressionBinding _binding;
            private bool _isValid;
            private System.ComponentModel.PropertyDescriptor _propDesc;

            public BindablePropertyNode(System.ComponentModel.PropertyDescriptor propDesc, ExpressionBinding binding)
            {
                this._binding = binding;
                this._propDesc = propDesc;
                this._isValid = true;
                base.Text = propDesc.Name;
                base.ImageIndex = base.SelectedImageIndex = this.IsBound ? (this.IsGenerated ? 2 : 1) : 0;
            }

            public ExpressionBinding Binding
            {
                get
                {
                    return this._binding;
                }
                set
                {
                    this._binding = value;
                    base.ImageIndex = base.SelectedImageIndex = this.IsBound ? 1 : 0;
                }
            }

            public bool IsBound
            {
                get
                {
                    return (this._binding != null);
                }
            }

            public bool IsGenerated
            {
                get
                {
                    return ((this._binding != null) && this._binding.Generated);
                }
            }

            public bool IsValid
            {
                get
                {
                    return this._isValid;
                }
                set
                {
                    this._isValid = value;
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return this._propDesc;
                }
            }
        }

        private sealed class ExpressionItem
        {
            private string _prefix;

            public ExpressionItem(string prefix)
            {
                this._prefix = prefix;
            }

            public override string ToString()
            {
                return this._prefix;
            }
        }

        private sealed class GenericExpressionEditor : ExpressionEditor
        {
            public override object EvaluateExpression(string expression, object parsedExpressionData, System.Type propertyType, IServiceProvider serviceProvider)
            {
                return string.Empty;
            }
        }
    }
}

