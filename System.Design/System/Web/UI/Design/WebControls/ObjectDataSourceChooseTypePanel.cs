namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Text;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    internal sealed class ObjectDataSourceChooseTypePanel : WizardPanel
    {
        private bool _discoveryServiceMode;
        private System.Windows.Forms.Label _exampleLabel;
        private System.Windows.Forms.CheckBox _filterCheckBox;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Label _nameLabel;
        private ObjectDataSource _objectDataSource;
        private ObjectDataSourceDesigner _objectDataSourceDesigner;
        private System.Type _previousSelectedType;
        private List<TypeItem> _typeItems;
        private AutoSizeComboBox _typeNameComboBox;
        private System.Windows.Forms.TextBox _typeNameTextBox;
        private const string CompareAllValuesFormatString = "original_{0}";

        public ObjectDataSourceChooseTypePanel(ObjectDataSourceDesigner objectDataSourceDesigner)
        {
            this._objectDataSourceDesigner = objectDataSourceDesigner;
            this._objectDataSource = (ObjectDataSource) this._objectDataSourceDesigner.Component;
            this.InitializeComponent();
            this.InitializeUI();
            ITypeDiscoveryService service = null;
            if (this._objectDataSource.Site != null)
            {
                service = (ITypeDiscoveryService) this._objectDataSource.Site.GetService(typeof(ITypeDiscoveryService));
            }
            this._discoveryServiceMode = service != null;
            if (this._discoveryServiceMode)
            {
                this._typeNameTextBox.Visible = false;
                this._exampleLabel.Visible = false;
                Cursor current = Cursor.Current;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    ICollection is2 = DesignerUtils.FilterGenericTypes(service.GetTypes(typeof(object), true));
                    this._typeNameComboBox.BeginUpdate();
                    if (is2 != null)
                    {
                        StringCollection strings = new StringCollection();
                        strings.Add("My.MyApplication");
                        strings.Add("My.MyComputer");
                        strings.Add("My.MyProject");
                        strings.Add("My.MyUser");
                        this._typeItems = new List<TypeItem>(is2.Count);
                        bool flag = false;
                        foreach (System.Type type in is2)
                        {
                            if (!type.IsEnum && !type.IsInterface)
                            {
                                object[] customAttributes = type.GetCustomAttributes(typeof(DataObjectAttribute), true);
                                if ((customAttributes.Length > 0) && ((DataObjectAttribute) customAttributes[0]).IsDataObject)
                                {
                                    this._typeItems.Add(new TypeItem(type, true));
                                    flag = true;
                                }
                                else if (!strings.Contains(type.FullName))
                                {
                                    this._typeItems.Add(new TypeItem(type, false));
                                }
                            }
                        }
                        object showOnlyDataComponentsState = this._objectDataSourceDesigner.ShowOnlyDataComponentsState;
                        if (showOnlyDataComponentsState == null)
                        {
                            this._filterCheckBox.Checked = flag;
                        }
                        else
                        {
                            this._filterCheckBox.Checked = (bool) showOnlyDataComponentsState;
                        }
                        this.UpdateTypeList();
                    }
                }
                finally
                {
                    this._typeNameComboBox.EndUpdate();
                    Cursor.Current = current;
                }
            }
            else
            {
                this._typeNameComboBox.Visible = false;
                this._filterCheckBox.Visible = false;
            }
            this.TypeName = this._objectDataSource.TypeName;
        }

        private void InitializeComponent()
        {
            this._helpLabel = new System.Windows.Forms.Label();
            this._nameLabel = new System.Windows.Forms.Label();
            this._exampleLabel = new System.Windows.Forms.Label();
            this._typeNameTextBox = new System.Windows.Forms.TextBox();
            this._typeNameComboBox = new AutoSizeComboBox();
            this._filterCheckBox = new System.Windows.Forms.CheckBox();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x220, 60);
            this._helpLabel.TabIndex = 10;
            this._nameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._nameLabel.Location = new Point(0, 0x44);
            this._nameLabel.Name = "_nameLabel";
            this._nameLabel.Size = new Size(0x220, 0x10);
            this._nameLabel.TabIndex = 20;
            this._typeNameTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._typeNameTextBox.Location = new Point(0, 0x56);
            this._typeNameTextBox.Name = "_typeNameTextBox";
            this._typeNameTextBox.Size = new Size(300, 20);
            this._typeNameTextBox.TabIndex = 30;
            this._typeNameTextBox.TextChanged += new EventHandler(this.OnTypeNameTextBoxTextChanged);
            this._typeNameComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._typeNameComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._typeNameComboBox.Location = new Point(0, 0x56);
            this._typeNameComboBox.Name = "_typeNameComboBox";
            this._typeNameComboBox.Size = new Size(300, 0x15);
            this._typeNameComboBox.Sorted = true;
            this._typeNameComboBox.TabIndex = 30;
            this._typeNameComboBox.SelectedIndexChanged += new EventHandler(this.OnTypeNameComboBoxSelectedIndexChanged);
            this._filterCheckBox.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._filterCheckBox.Location = new Point(0x132, 0x56);
            this._filterCheckBox.Name = "_filterCheckBox";
            this._filterCheckBox.Size = new Size(200, 0x12);
            this._filterCheckBox.TabIndex = 50;
            this._filterCheckBox.CheckedChanged += new EventHandler(this.OnFilterCheckBoxCheckedChanged);
            this._exampleLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._exampleLabel.ForeColor = SystemColors.GrayText;
            this._exampleLabel.Location = new Point(0, 0x7a);
            this._exampleLabel.Name = "_exampleLabel";
            this._exampleLabel.Size = new Size(0x220, 0x10);
            this._exampleLabel.TabIndex = 60;
            base.Controls.Add(this._filterCheckBox);
            base.Controls.Add(this._typeNameComboBox);
            base.Controls.Add(this._typeNameTextBox);
            base.Controls.Add(this._exampleLabel);
            base.Controls.Add(this._nameLabel);
            base.Controls.Add(this._helpLabel);
            base.Name = "ObjectDataSourceChooseTypePanel";
            base.Size = new Size(0x220, 0x112);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            base.Caption = System.Design.SR.GetString("ObjectDataSourceChooseTypePanel_PanelCaption");
            this._helpLabel.Text = System.Design.SR.GetString("ObjectDataSourceChooseTypePanel_HelpLabel");
            this._nameLabel.Text = System.Design.SR.GetString("ObjectDataSourceChooseTypePanel_NameLabel");
            this._exampleLabel.Text = System.Design.SR.GetString("ObjectDataSourceChooseTypePanel_ExampleLabel");
            this._filterCheckBox.Text = System.Design.SR.GetString("ObjectDataSourceChooseTypePanel_FilterCheckBox");
        }

        protected internal override void OnComplete()
        {
            if (this._objectDataSource.TypeName != this.TypeName)
            {
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this._objectDataSource)["TypeName"];
                descriptor.SetValue(this._objectDataSource, this.TypeName);
            }
            if ((this.SelectedTypeItem != null) && this.SelectedTypeItem.Filtered)
            {
                TypeDescriptor.GetProperties(this._objectDataSource)["OldValuesParameterFormatString"].SetValue(this._objectDataSource, "original_{0}");
            }
            this._objectDataSourceDesigner.ShowOnlyDataComponentsState = this._filterCheckBox.Checked;
        }

        private void OnFilterCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this.UpdateTypeList();
        }

        public override bool OnNext()
        {
            TypeItem selectedTypeItem = this.SelectedTypeItem;
            System.Type type = selectedTypeItem.Type;
            if (type == null)
            {
                ITypeResolutionService service = (ITypeResolutionService) base.ServiceProvider.GetService(typeof(ITypeResolutionService));
                if (service == null)
                {
                    return false;
                }
                try
                {
                    type = service.GetType(selectedTypeItem.TypeName, true, true);
                }
                catch (Exception exception)
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("ObjectDataSourceDesigner_CannotGetType", new object[] { selectedTypeItem.TypeName }));
                    return false;
                }
            }
            if (type == null)
            {
                return false;
            }
            if (type != this._previousSelectedType)
            {
                (base.NextPanel as ObjectDataSourceChooseMethodsPanel).SetType(type);
                this._previousSelectedType = type;
            }
            return true;
        }

        public override void OnPrevious()
        {
        }

        private void OnTypeNameComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledState();
        }

        private void OnTypeNameTextBoxTextChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledState();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                this.UpdateEnabledState();
            }
        }

        private void UpdateEnabledState()
        {
            if (base.ParentWizard != null)
            {
                base.ParentWizard.FinishButton.Enabled = false;
                if (this._discoveryServiceMode)
                {
                    base.ParentWizard.NextButton.Enabled = this._typeNameComboBox.SelectedItem != null;
                }
                else
                {
                    base.ParentWizard.NextButton.Enabled = this._typeNameTextBox.Text.Length > 0;
                }
            }
        }

        private void UpdateTypeList()
        {
            object selectedItem = this._typeNameComboBox.SelectedItem;
            try
            {
                this._typeNameComboBox.BeginUpdate();
                this._typeNameComboBox.Items.Clear();
                bool flag = this._filterCheckBox.Checked;
                foreach (TypeItem item in this._typeItems)
                {
                    if (flag)
                    {
                        if (item.Filtered)
                        {
                            this._typeNameComboBox.Items.Add(item);
                        }
                    }
                    else
                    {
                        this._typeNameComboBox.Items.Add(item);
                    }
                }
            }
            finally
            {
                this._typeNameComboBox.EndUpdate();
            }
            this._typeNameComboBox.SelectedItem = selectedItem;
            this.UpdateEnabledState();
            this._typeNameComboBox.InvalidateDropDownWidth();
        }

        private TypeItem SelectedTypeItem
        {
            get
            {
                if (this._discoveryServiceMode)
                {
                    return (this._typeNameComboBox.SelectedItem as TypeItem);
                }
                return new TypeItem(this._typeNameTextBox.Text, false);
            }
        }

        private string TypeName
        {
            get
            {
                TypeItem selectedTypeItem = this.SelectedTypeItem;
                if (selectedTypeItem != null)
                {
                    return selectedTypeItem.TypeName;
                }
                return string.Empty;
            }
            set
            {
                if (!this._discoveryServiceMode)
                {
                    this._typeNameTextBox.Text = value;
                }
                else
                {
                    foreach (TypeItem item in this._typeNameComboBox.Items)
                    {
                        if (string.Compare(item.TypeName, value, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._typeNameComboBox.SelectedItem = item;
                            break;
                        }
                    }
                    if ((this._typeNameComboBox.SelectedItem == null) && (value.Length > 0))
                    {
                        TypeItem item2 = new TypeItem(value, true);
                        this._typeItems.Add(item2);
                        this.UpdateTypeList();
                        this._typeNameComboBox.SelectedItem = item2;
                    }
                }
            }
        }

        private sealed class TypeItem
        {
            private bool _filtered;
            private string _prettyTypeName;
            private System.Type _type;
            private string _typeName;

            public TypeItem(string typeName, bool filtered)
            {
                this._typeName = typeName;
                this._prettyTypeName = this._typeName;
                this._type = null;
                this._filtered = filtered;
            }

            public TypeItem(System.Type type, bool filtered)
            {
                StringBuilder sb = new StringBuilder(0x40);
                ObjectDataSourceMethodEditor.AppendTypeName(type, true, sb);
                this._prettyTypeName = sb.ToString();
                this._typeName = type.FullName;
                this._type = type;
                this._filtered = filtered;
            }

            public override string ToString()
            {
                return this._prettyTypeName;
            }

            public bool Filtered
            {
                get
                {
                    return this._filtered;
                }
            }

            public System.Type Type
            {
                get
                {
                    return this._type;
                }
            }

            public string TypeName
            {
                get
                {
                    return this._typeName;
                }
            }
        }
    }
}

