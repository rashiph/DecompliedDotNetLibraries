namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class AddDataControlFieldDialog : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private DataBoundControlDesigner _controlDesigner;
        private System.Windows.Forms.Panel _controlsPanel;
        private IDictionary<System.Type, DataControlFieldDesigner> _customFieldDesigners;
        private DataControlFieldControl[] _dataControlFieldControls;
        private bool _dynamicDataEnabled;
        private System.Windows.Forms.Label _fieldLabel;
        private ComboBox _fieldList;
        private IDataSourceFieldSchema[] _fieldSchemas;
        private bool _initialIgnoreRefreshSchemaValue;
        private System.Windows.Forms.Button _okButton;
        private LinkLabel _refreshSchemaLink;
        private const int bottomPadding = 12;
        private const int buttonHeight = 0x17;
        private const int buttonWidth = 0x4b;
        private const int checkBoxWidth = 0x7d;
        private const int controlHeight = 20;
        private const int controlLeft = 12;
        private const int fieldChooserWidth = 150;
        private int fieldControlTop;
        private const int formHeight = 510;
        private const int formWidth = 330;
        private const int horizPadding = 6;
        private const int labelHeight = 0x11;
        private const int labelLeft = 12;
        private const int labelPadding = 2;
        private const int labelWidth = 270;
        private const int linkWidth = 100;
        private const int rightPadding = 12;
        private const int textBoxWidth = 270;
        private const int topPadding = 12;
        private const int vertPadding = 4;

        public AddDataControlFieldDialog(DataBoundControlDesigner controlDesigner) : base(controlDesigner.Component.Site)
        {
            this.fieldControlTop = 0x33;
            this._controlDesigner = controlDesigner;
            this.IgnoreRefreshSchemaEvents();
            this.InitForm();
        }

        private void AddControls()
        {
            this._okButton.SetBounds(0xa2, 0x1db, 0x4b, 0x17);
            this._okButton.Click += new EventHandler(this.OnClickOKButton);
            this._okButton.Text = System.Design.SR.GetString("OKCaption");
            this._okButton.TabIndex = 0xc9;
            this._okButton.FlatStyle = FlatStyle.System;
            this._okButton.DialogResult = DialogResult.OK;
            this._cancelButton.SetBounds(0xf3, 0x1db, 0x4b, 0x17);
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Text = System.Design.SR.GetString("CancelCaption");
            this._cancelButton.FlatStyle = FlatStyle.System;
            this._cancelButton.TabIndex = 0xca;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._fieldLabel.Text = System.Design.SR.GetString("DCFAdd_ChooseField");
            this._fieldLabel.TabStop = false;
            this._fieldLabel.TextAlign = ContentAlignment.BottomLeft;
            this._fieldLabel.SetBounds(12, 12, 0x132, 0x11);
            this._fieldLabel.TabIndex = 0;
            this._fieldList.DropDownStyle = ComboBoxStyle.DropDownList;
            this._fieldList.TabIndex = 1;
            this._controlsPanel.SetBounds(12, this.fieldControlTop, 330, (((510 - this.fieldControlTop) - 12) - 0x17) - 4);
            this._controlsPanel.TabIndex = 100;
            for (int i = 0; i < this.GetDataControlFieldControls().Length; i++)
            {
                DataControlFieldControl control = this.GetDataControlFieldControls()[i];
                this._fieldList.Items.Add(control.FieldName);
                control.Visible = false;
                control.TabStop = false;
                control.SetBounds(0, 0, 330, (((510 - this.fieldControlTop) - 12) - 0x17) - 4);
                this._controlsPanel.Controls.Add(control);
            }
            this._fieldList.SelectedIndex = 0;
            this._fieldList.SelectedIndexChanged += new EventHandler(this.OnSelectedFieldTypeChanged);
            this.SetSelectedFieldControlVisible();
            this._fieldList.SetBounds(12, 0x1f, 150, 20);
            this._refreshSchemaLink.SetBounds(12, 0x1db, 100, 0x2a);
            this._refreshSchemaLink.TabIndex = 200;
            this._refreshSchemaLink.Visible = false;
            this._refreshSchemaLink.Text = System.Design.SR.GetString("DataSourceDesigner_RefreshSchemaNoHotkey");
            this._refreshSchemaLink.UseMnemonic = true;
            this._refreshSchemaLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnClickRefreshSchema);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.Controls.AddRange(new System.Windows.Forms.Control[] { this._cancelButton, this._okButton, this._fieldLabel, this._fieldList, this._controlsPanel, this._refreshSchemaLink });
        }

        private IDataSourceFieldSchema[] GetBooleanFieldSchemas()
        {
            IDataSourceFieldSchema[] fieldSchemas = this.GetFieldSchemas();
            ArrayList list = new ArrayList();
            IDataSourceFieldSchema[] array = null;
            if (fieldSchemas != null)
            {
                foreach (IDataSourceFieldSchema schema in fieldSchemas)
                {
                    if (schema.DataType == typeof(bool))
                    {
                        list.Add(schema);
                    }
                }
                array = new IDataSourceFieldSchema[list.Count];
                list.CopyTo(array);
            }
            return array;
        }

        private DataControlFieldControl[] GetDataControlFieldControls()
        {
            System.Type controlType = this.Control.GetType();
            if (this._dataControlFieldControls == null)
            {
                List<DataControlFieldControl> designerDataControlFieldControls = this.GetDesignerDataControlFieldControls();
                DataControlFieldDesigner dynamicFieldDesigner = null;
                DataControlFieldControl item = null;
                foreach (DataControlFieldControl control2 in designerDataControlFieldControls)
                {
                    DataControlFieldDesignerControl control3 = control2 as DataControlFieldDesignerControl;
                    if ((control3 != null) && (control3.Designer.GetType().FullName == "System.Web.DynamicData.Design.DynamicFieldDesigner"))
                    {
                        item = control3;
                        dynamicFieldDesigner = control3.Designer;
                        this._dynamicDataEnabled = true;
                        break;
                    }
                }
                if (this._dynamicDataEnabled)
                {
                    designerDataControlFieldControls.Remove(item);
                }
                int num = this._dynamicDataEnabled ? 8 : 7;
                this._dataControlFieldControls = new DataControlFieldControl[num + designerDataControlFieldControls.Count];
                this._dataControlFieldControls[0] = new BoundFieldControl(this.GetFieldSchemas(), controlType);
                this._dataControlFieldControls[1] = new CheckBoxFieldControl(this.GetBooleanFieldSchemas(), controlType);
                this._dataControlFieldControls[2] = new HyperLinkFieldControl(this.GetFieldSchemas(), controlType);
                this._dataControlFieldControls[3] = new ButtonFieldControl(null, controlType);
                this._dataControlFieldControls[4] = new CommandFieldControl(null, controlType);
                this._dataControlFieldControls[5] = new ImageFieldControl(this.GetFieldSchemas(), controlType);
                this._dataControlFieldControls[6] = new TemplateFieldControl(null, controlType);
                if (this._dynamicDataEnabled)
                {
                    this._dataControlFieldControls[7] = new DynamicDataFieldControl(dynamicFieldDesigner, this.GetFieldSchemas(), controlType);
                }
                int num2 = num;
                foreach (DataControlFieldControl control4 in designerDataControlFieldControls)
                {
                    this._dataControlFieldControls[num2++] = control4;
                }
            }
            return this._dataControlFieldControls;
        }

        private List<DataControlFieldControl> GetDesignerDataControlFieldControls()
        {
            if (this._customFieldDesigners == null)
            {
                this._customFieldDesigners = DataControlFieldHelper.GetCustomFieldDesigners(this, this.Control);
            }
            System.Type controlType = this.Control.GetType();
            List<DataControlFieldControl> list = new List<DataControlFieldControl>();
            foreach (KeyValuePair<System.Type, DataControlFieldDesigner> pair in this._customFieldDesigners)
            {
                DataControlFieldDesigner designer = pair.Value;
                list.Add(new DataControlFieldDesignerControl(this._controlDesigner, base.ServiceProvider, designer, null, controlType));
            }
            return list;
        }

        private IDataSourceFieldSchema[] GetFieldSchemas()
        {
            if (this._fieldSchemas == null)
            {
                IDataSourceViewSchema schema = null;
                if (this._controlDesigner != null)
                {
                    DesignerDataSourceView designerView = this._controlDesigner.DesignerView;
                    if (designerView != null)
                    {
                        try
                        {
                            schema = designerView.Schema;
                        }
                        catch (Exception exception)
                        {
                            IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.ServiceProvider.GetService(typeof(IComponentDesignerDebugService));
                            if (service != null)
                            {
                                service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                            }
                        }
                    }
                }
                if (schema != null)
                {
                    this._fieldSchemas = schema.GetFields();
                }
            }
            return this._fieldSchemas;
        }

        private void IgnoreRefreshSchemaEvents()
        {
            this._initialIgnoreRefreshSchemaValue = this.IgnoreRefreshSchema;
            this.IgnoreRefreshSchema = true;
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if (dataSourceDesigner != null)
            {
                dataSourceDesigner.SuppressDataSourceEvents();
            }
        }

        private void InitForm()
        {
            base.SuspendLayout();
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._fieldLabel = new System.Windows.Forms.Label();
            this._fieldList = new ComboBox();
            this._refreshSchemaLink = new LinkLabel();
            this._controlsPanel = new System.Windows.Forms.Panel();
            this.AddControls();
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if ((dataSourceDesigner != null) && dataSourceDesigner.CanRefreshSchema)
            {
                this._refreshSchemaLink.Visible = true;
            }
            this.Text = System.Design.SR.GetString("DCFAdd_Title");
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.ClientSize = new Size(330, 510);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.Icon = null;
            base.InitializeForm();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void OnClickOKButton(object sender, EventArgs e)
        {
            DataControlFieldControl control = this.GetDataControlFieldControls()[this._fieldList.SelectedIndex];
            DataBoundControl control2 = this.Control;
            if (control2 is GridView)
            {
                ((GridView) control2).Columns.Add(control.SaveValues());
            }
            else if (control2 is DetailsView)
            {
                ((DetailsView) control2).Fields.Add(control.SaveValues());
            }
        }

        private void OnClickRefreshSchema(object source, LinkLabelLinkClickedEventArgs e)
        {
            if (this._controlDesigner != null)
            {
                IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
                if ((dataSourceDesigner != null) && dataSourceDesigner.CanRefreshSchema)
                {
                    IDictionary table = this.GetDataControlFieldControls()[this._fieldList.SelectedIndex].PreserveFields();
                    dataSourceDesigner.RefreshSchema(false);
                    this._fieldSchemas = this.GetFieldSchemas();
                    this.GetDataControlFieldControls()[0].RefreshSchema(this._fieldSchemas);
                    this.GetDataControlFieldControls()[1].RefreshSchema(this.GetBooleanFieldSchemas());
                    this.GetDataControlFieldControls()[2].RefreshSchema(this._fieldSchemas);
                    this.GetDataControlFieldControls()[5].RefreshSchema(this._fieldSchemas);
                    if (this._dynamicDataEnabled)
                    {
                        this._dataControlFieldControls[7].RefreshSchema(this._fieldSchemas);
                    }
                    this.GetDataControlFieldControls()[this._fieldList.SelectedIndex].RestoreFields(table);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if (dataSourceDesigner != null)
            {
                dataSourceDesigner.ResumeDataSourceEvents();
            }
            this.IgnoreRefreshSchema = this._initialIgnoreRefreshSchemaValue;
        }

        private void OnSelectedFieldTypeChanged(object sender, EventArgs e)
        {
            this.SetSelectedFieldControlVisible();
        }

        private void SetSelectedFieldControlVisible()
        {
            foreach (DataControlFieldControl control in this.GetDataControlFieldControls())
            {
                control.Visible = false;
            }
            this.GetDataControlFieldControls()[this._fieldList.SelectedIndex].Visible = true;
            this.Refresh();
        }

        private DataBoundControl Control
        {
            get
            {
                return (this._controlDesigner.Component as DataBoundControl);
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.DataControlField.AddDataControlFieldDialog";
            }
        }

        private bool IgnoreRefreshSchema
        {
            get
            {
                if (this._controlDesigner is GridViewDesigner)
                {
                    return ((GridViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent;
                }
                return ((this._controlDesigner is DetailsViewDesigner) && ((DetailsViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent);
            }
            set
            {
                if (this._controlDesigner is GridViewDesigner)
                {
                    ((GridViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent = value;
                }
                if (this._controlDesigner is DetailsViewDesigner)
                {
                    ((DetailsViewDesigner) this._controlDesigner)._ignoreSchemaRefreshedEvent = value;
                }
            }
        }

        private class BoundFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            protected System.Windows.Forms.TextBox _dataFieldBox;
            private System.Windows.Forms.Label _dataFieldLabel;
            protected ComboBox _dataFieldList;
            protected System.Windows.Forms.CheckBox _readOnlyCheckBox;

            public BoundFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                this._dataFieldList = new ComboBox();
                this._dataFieldBox = new System.Windows.Forms.TextBox();
                this._dataFieldLabel = new System.Windows.Forms.Label();
                this._readOnlyCheckBox = new System.Windows.Forms.CheckBox();
                this._dataFieldLabel.Text = System.Design.SR.GetString("DCFAdd_DataField");
                this._dataFieldLabel.TextAlign = ContentAlignment.BottomLeft;
                this._dataFieldLabel.SetBounds(0, 0x2b, 270, 0x11);
                this._dataFieldList.DropDownStyle = ComboBoxStyle.DropDownList;
                this._dataFieldList.TabIndex = 1;
                this._dataFieldList.SetBounds(0, 0x3e, 270, 20);
                this._dataFieldList.SelectedIndexChanged += new EventHandler(this.OnSelectedDataFieldChanged);
                this._dataFieldBox.TabIndex = 1;
                this._dataFieldBox.SetBounds(0, 0x3e, 270, 20);
                this._readOnlyCheckBox.TabIndex = 2;
                this._readOnlyCheckBox.Text = System.Design.SR.GetString("DCFAdd_ReadOnly");
                this._readOnlyCheckBox.SetBounds(0, 0x56, 270, 20);
                this.RefreshSchemaFields();
                base.Controls.AddRange(new Control[] { this._dataFieldLabel, this._dataFieldBox, this._dataFieldList, this._readOnlyCheckBox });
            }

            private void OnSelectedDataFieldChanged(object sender, EventArgs e)
            {
                if (base._haveSchema)
                {
                    int index = Array.IndexOf<string>(base.GetFieldSchemaNames(), this._dataFieldList.Text);
                    if ((index >= 0) && base._fieldSchemas[index].PrimaryKey)
                    {
                        this._readOnlyCheckBox.Checked = true;
                        return;
                    }
                }
                this._readOnlyCheckBox.Checked = false;
            }

            protected override void PreserveFields(IDictionary table)
            {
                if (base._haveSchema)
                {
                    table["DataField"] = this._dataFieldList.Text;
                }
                else
                {
                    table["DataField"] = this._dataFieldBox.Text;
                }
                table["ReadOnly"] = this._readOnlyCheckBox.Checked;
            }

            protected override void RefreshSchemaFields()
            {
                if (base._haveSchema)
                {
                    this._dataFieldList.Items.Clear();
                    this._dataFieldList.Items.AddRange(base.GetFieldSchemaNames());
                    this._dataFieldList.SelectedIndex = 0;
                    this._dataFieldList.Visible = true;
                    this._dataFieldBox.Visible = false;
                }
                else
                {
                    this._dataFieldList.Visible = false;
                    this._dataFieldBox.Visible = true;
                }
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
                string strA = table["DataField"].ToString();
                if (!base._haveSchema)
                {
                    this._dataFieldBox.Text = strA;
                }
                else if (strA.Length > 0)
                {
                    bool flag = false;
                    foreach (object obj2 in this._dataFieldList.Items)
                    {
                        if (string.Compare(strA, obj2.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._dataFieldList.SelectedItem = obj2;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        this._dataFieldList.Items.Insert(0, strA);
                        this._dataFieldList.SelectedIndex = 0;
                    }
                }
                this._readOnlyCheckBox.Checked = (bool) table["ReadOnly"];
            }

            protected override DataControlField SaveValues(string headerText)
            {
                BoundField field = new BoundField {
                    HeaderText = headerText
                };
                if (base._haveSchema)
                {
                    field.DataField = this._dataFieldList.Text;
                }
                else
                {
                    field.DataField = this._dataFieldBox.Text;
                }
                field.ReadOnly = this._readOnlyCheckBox.Checked;
                field.SortExpression = field.DataField;
                return field;
            }

            public override string FieldName
            {
                get
                {
                    return "BoundField";
                }
            }
        }

        private class ButtonFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            private System.Windows.Forms.Label _buttonTypeLabel;
            private ComboBox _buttonTypeList;
            private System.Windows.Forms.Label _commandNameLabel;
            private ComboBox _commandNameList;
            private System.Windows.Forms.TextBox _textBox;
            private System.Windows.Forms.Label _textLabel;

            public ButtonFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                this._buttonTypeLabel = new System.Windows.Forms.Label();
                this._commandNameLabel = new System.Windows.Forms.Label();
                this._textLabel = new System.Windows.Forms.Label();
                this._buttonTypeList = new ComboBox();
                this._commandNameList = new ComboBox();
                this._textBox = new System.Windows.Forms.TextBox();
                this._buttonTypeLabel.Text = System.Design.SR.GetString("DCFAdd_ButtonType");
                this._buttonTypeLabel.TextAlign = ContentAlignment.BottomLeft;
                this._buttonTypeLabel.SetBounds(0, 0x2b, 270, 0x11);
                this._buttonTypeList.Items.Add(ButtonType.Link.ToString());
                this._buttonTypeList.Items.Add(ButtonType.Button.ToString());
                this._buttonTypeList.SelectedIndex = 0;
                this._buttonTypeList.DropDownStyle = ComboBoxStyle.DropDownList;
                this._buttonTypeList.TabIndex = 1;
                this._buttonTypeList.SetBounds(0, 0x3e, 270, 20);
                this._commandNameLabel.Text = System.Design.SR.GetString("DCFAdd_CommandName");
                this._commandNameLabel.TextAlign = ContentAlignment.BottomLeft;
                this._commandNameLabel.SetBounds(0, 0x56, 270, 0x11);
                this._commandNameList.Items.Add("Cancel");
                this._commandNameList.Items.Add("Delete");
                this._commandNameList.Items.Add("Edit");
                this._commandNameList.Items.Add("Update");
                if (base._controlType == typeof(DetailsView))
                {
                    this._commandNameList.Items.Insert(3, "Insert");
                    this._commandNameList.Items.Insert(4, "New");
                }
                else if (base._controlType == typeof(GridView))
                {
                    this._commandNameList.Items.Insert(3, "Select");
                }
                this._commandNameList.SelectedIndex = 0;
                this._commandNameList.DropDownStyle = ComboBoxStyle.DropDownList;
                this._commandNameList.TabIndex = 2;
                this._commandNameList.SetBounds(0, 0x69, 270, 20);
                this._textLabel.Text = System.Design.SR.GetString("DCFAdd_Text");
                this._textLabel.TextAlign = ContentAlignment.BottomLeft;
                this._textLabel.SetBounds(0, 0x81, 270, 0x11);
                this._textBox.TabIndex = 3;
                this._textBox.Text = System.Design.SR.GetString("DCFEditor_Button");
                this._textBox.SetBounds(0, 0x94, 270, 20);
                base.Controls.AddRange(new Control[] { this._buttonTypeLabel, this._commandNameLabel, this._textLabel, this._buttonTypeList, this._commandNameList, this._textBox });
            }

            protected override void PreserveFields(IDictionary table)
            {
                table["ButtonType"] = this._buttonTypeList.SelectedIndex;
                table["CommandName"] = this._commandNameList.SelectedIndex;
                table["Text"] = this._textBox.Text;
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
                this._buttonTypeList.SelectedIndex = (int) table["ButtonType"];
                this._commandNameList.SelectedIndex = (int) table["CommandName"];
                this._textBox.Text = table["Text"].ToString();
            }

            protected override DataControlField SaveValues(string headerText)
            {
                ButtonField field = new ButtonField();
                if ((headerText != null) && (headerText.Length > 0))
                {
                    field.HeaderText = headerText;
                    field.ShowHeader = true;
                }
                field.CommandName = this._commandNameList.Text;
                field.Text = this._textBox.Text;
                if (this._buttonTypeList.SelectedIndex == 0)
                {
                    field.ButtonType = ButtonType.Link;
                    return field;
                }
                field.ButtonType = ButtonType.Button;
                return field;
            }

            public override string FieldName
            {
                get
                {
                    return "ButtonField";
                }
            }
        }

        private class CheckBoxFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            private System.Windows.Forms.TextBox _dataFieldBox;
            private System.Windows.Forms.Label _dataFieldLabel;
            private ComboBox _dataFieldList;
            private System.Windows.Forms.CheckBox _readOnlyCheckBox;

            public CheckBoxFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                this._dataFieldList = new ComboBox();
                this._dataFieldBox = new System.Windows.Forms.TextBox();
                this._dataFieldLabel = new System.Windows.Forms.Label();
                this._readOnlyCheckBox = new System.Windows.Forms.CheckBox();
                this._dataFieldLabel.Text = System.Design.SR.GetString("DCFAdd_DataField");
                this._dataFieldLabel.TextAlign = ContentAlignment.BottomLeft;
                this._dataFieldLabel.SetBounds(0, 0x2b, 270, 0x11);
                this._dataFieldList.DropDownStyle = ComboBoxStyle.DropDownList;
                this._dataFieldList.TabIndex = 1;
                this._dataFieldList.SetBounds(0, 0x3e, 270, 20);
                this._dataFieldBox.TabIndex = 1;
                this._dataFieldBox.SetBounds(0, 0x3e, 270, 20);
                this._readOnlyCheckBox.TabIndex = 2;
                this._readOnlyCheckBox.Text = System.Design.SR.GetString("DCFAdd_ReadOnly");
                this._readOnlyCheckBox.SetBounds(0, 0x56, 270, 20);
                this.RefreshSchemaFields();
                base.Controls.AddRange(new Control[] { this._dataFieldLabel, this._dataFieldBox, this._dataFieldList, this._readOnlyCheckBox });
            }

            protected override void PreserveFields(IDictionary table)
            {
                if (base._haveSchema)
                {
                    table["DataField"] = this._dataFieldList.Text;
                }
                else
                {
                    table["DataField"] = this._dataFieldBox.Text;
                }
                table["ReadOnly"] = this._readOnlyCheckBox.Checked;
            }

            protected override void RefreshSchemaFields()
            {
                if (base._haveSchema)
                {
                    this._dataFieldList.Items.Clear();
                    this._dataFieldList.Items.AddRange(base.GetFieldSchemaNames());
                    this._dataFieldList.SelectedIndex = 0;
                    this._dataFieldList.Visible = true;
                    this._dataFieldBox.Visible = false;
                }
                else
                {
                    this._dataFieldList.Visible = false;
                    this._dataFieldBox.Visible = true;
                }
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
                string strA = table["DataField"].ToString();
                if (!base._haveSchema)
                {
                    this._dataFieldBox.Text = strA;
                }
                else if (strA.Length > 0)
                {
                    bool flag = false;
                    foreach (object obj2 in this._dataFieldList.Items)
                    {
                        if (string.Compare(strA, obj2.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._dataFieldList.SelectedItem = obj2;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        this._dataFieldList.Items.Insert(0, strA);
                        this._dataFieldList.SelectedIndex = 0;
                    }
                }
                this._readOnlyCheckBox.Checked = (bool) table["ReadOnly"];
            }

            protected override DataControlField SaveValues(string headerText)
            {
                CheckBoxField field = new CheckBoxField {
                    HeaderText = headerText
                };
                if (base._haveSchema)
                {
                    field.DataField = this._dataFieldList.Text;
                }
                else
                {
                    field.DataField = this._dataFieldBox.Text;
                }
                field.ReadOnly = this._readOnlyCheckBox.Checked;
                field.SortExpression = field.DataField;
                return field;
            }

            public override string FieldName
            {
                get
                {
                    return "CheckBoxField";
                }
            }
        }

        private class CommandFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            private System.Windows.Forms.Label _buttonTypeLabel;
            private ComboBox _buttonTypeList;
            private System.Windows.Forms.CheckBox _cancelBox;
            private System.Windows.Forms.Label _commandButtonsLabel;
            private System.Windows.Forms.CheckBox _deleteBox;
            private System.Windows.Forms.CheckBox _insertBox;
            private System.Windows.Forms.CheckBox _selectBox;
            private System.Windows.Forms.CheckBox _updateBox;
            private const int checkBoxLeft = 8;

            public CommandFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                this._buttonTypeLabel = new System.Windows.Forms.Label();
                this._buttonTypeList = new ComboBox();
                this._commandButtonsLabel = new System.Windows.Forms.Label();
                this._deleteBox = new System.Windows.Forms.CheckBox();
                this._selectBox = new System.Windows.Forms.CheckBox();
                this._cancelBox = new System.Windows.Forms.CheckBox();
                this._updateBox = new System.Windows.Forms.CheckBox();
                this._insertBox = new System.Windows.Forms.CheckBox();
                this._buttonTypeLabel.Text = System.Design.SR.GetString("DCFAdd_ButtonType");
                this._buttonTypeLabel.TextAlign = ContentAlignment.BottomLeft;
                this._buttonTypeLabel.SetBounds(0, 0x2b, 270, 0x11);
                this._buttonTypeList.Items.Add(ButtonType.Link.ToString());
                this._buttonTypeList.Items.Add(ButtonType.Button.ToString());
                this._buttonTypeList.SelectedIndex = 0;
                this._buttonTypeList.DropDownStyle = ComboBoxStyle.DropDownList;
                this._buttonTypeList.TabIndex = 1;
                this._buttonTypeList.SetBounds(0, 0x3e, 270, 20);
                this._commandButtonsLabel.Text = System.Design.SR.GetString("DCFAdd_CommandButtons");
                this._commandButtonsLabel.TextAlign = ContentAlignment.BottomLeft;
                this._commandButtonsLabel.SetBounds(0, 0x56, 270, 0x11);
                this._deleteBox.Text = System.Design.SR.GetString("DCFAdd_Delete");
                this._deleteBox.AccessibleDescription = System.Design.SR.GetString("DCFAdd_DeleteDesc");
                this._deleteBox.TextAlign = ContentAlignment.TopLeft;
                this._deleteBox.CheckAlign = ContentAlignment.TopLeft;
                this._deleteBox.TabIndex = 2;
                this._deleteBox.SetBounds(8, 0x69, 0x7d, 20);
                this._selectBox.Text = System.Design.SR.GetString("DCFAdd_Select");
                this._selectBox.AccessibleDescription = System.Design.SR.GetString("DCFAdd_SelectDesc");
                this._selectBox.TextAlign = ContentAlignment.TopLeft;
                this._selectBox.CheckAlign = ContentAlignment.TopLeft;
                this._selectBox.TabIndex = 4;
                this._selectBox.SetBounds(8, 0x7d, 0x7d, 20);
                this._cancelBox.Text = System.Design.SR.GetString("DCFAdd_ShowCancel");
                this._cancelBox.AccessibleDescription = System.Design.SR.GetString("DCFAdd_ShowCancelDesc");
                this._cancelBox.TextAlign = ContentAlignment.TopLeft;
                this._cancelBox.CheckAlign = ContentAlignment.TopLeft;
                this._cancelBox.Enabled = false;
                this._cancelBox.Checked = true;
                this._cancelBox.TabIndex = 6;
                this._cancelBox.SetBounds(8, 0x91, 270, 0x2c);
                this._updateBox.Text = System.Design.SR.GetString("DCFAdd_EditUpdate");
                this._updateBox.AccessibleDescription = System.Design.SR.GetString("DCFAdd_EditUpdateDesc");
                this._updateBox.TextAlign = ContentAlignment.TopLeft;
                this._updateBox.CheckAlign = ContentAlignment.TopLeft;
                this._updateBox.TabIndex = 3;
                this._updateBox.CheckedChanged += new EventHandler(this.OnCheckedChanged);
                this._updateBox.SetBounds(0x8b, 0x69, 0x7d, 20);
                this._insertBox.Text = System.Design.SR.GetString("DCFAdd_NewInsert");
                this._insertBox.AccessibleDescription = System.Design.SR.GetString("DCFAdd_NewInsertDesc");
                this._insertBox.TextAlign = ContentAlignment.TopLeft;
                this._insertBox.CheckAlign = ContentAlignment.TopLeft;
                this._insertBox.TabIndex = 5;
                this._insertBox.CheckedChanged += new EventHandler(this.OnCheckedChanged);
                this._insertBox.SetBounds(8, 0x7d, 0x7d, 20);
                if (base._controlType == typeof(GridView))
                {
                    this._insertBox.Visible = false;
                }
                else if (base._controlType == typeof(DetailsView))
                {
                    this._selectBox.Visible = false;
                }
                base.Controls.AddRange(new Control[] { this._buttonTypeLabel, this._buttonTypeList, this._commandButtonsLabel, this._deleteBox, this._selectBox, this._cancelBox, this._updateBox, this._insertBox });
            }

            private void OnCheckedChanged(object sender, EventArgs e)
            {
                this._cancelBox.Enabled = this._updateBox.Checked || this._insertBox.Checked;
            }

            protected override void PreserveFields(IDictionary table)
            {
                table["ButtonType"] = this._buttonTypeList.SelectedIndex;
                table["ShowDeleteButton"] = this._deleteBox.Checked;
                table["ShowSelectButton"] = this._selectBox.Checked;
                table["ShowCancelButton"] = this._cancelBox.Checked;
                table["ShowEditButton"] = this._updateBox.Checked;
                table["ShowInsertButton"] = this._insertBox.Checked;
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
                this._buttonTypeList.SelectedIndex = (int) table["ButtonType"];
                this._deleteBox.Checked = (bool) table["ShowDeleteButton"];
                this._selectBox.Checked = (bool) table["ShowSelectButton"];
                this._cancelBox.Checked = (bool) table["ShowCancelButton"];
                this._updateBox.Checked = (bool) table["ShowEditButton"];
                this._insertBox.Checked = (bool) table["ShowInsertButton"];
            }

            protected override DataControlField SaveValues(string headerText)
            {
                CommandField field = new CommandField();
                if ((headerText != null) && (headerText.Length > 0))
                {
                    field.HeaderText = headerText;
                    field.ShowHeader = true;
                }
                if (this._buttonTypeList.SelectedIndex == 0)
                {
                    field.ButtonType = ButtonType.Link;
                }
                else
                {
                    field.ButtonType = ButtonType.Button;
                }
                field.ShowDeleteButton = this._deleteBox.Checked;
                field.ShowSelectButton = this._selectBox.Checked;
                if (this._cancelBox.Enabled)
                {
                    field.ShowCancelButton = this._cancelBox.Checked;
                }
                field.ShowEditButton = this._updateBox.Checked;
                field.ShowInsertButton = this._insertBox.Checked;
                return field;
            }

            public override string FieldName
            {
                get
                {
                    return "CommandField";
                }
            }
        }

        private abstract class DataControlFieldControl : Control
        {
            protected System.Type _controlType;
            protected string[] _fieldSchemaNames;
            protected IDataSourceFieldSchema[] _fieldSchemas;
            protected bool _haveSchema;
            private System.Windows.Forms.TextBox _headerTextBox;
            private System.Windows.Forms.Label _headerTextLabel;

            public DataControlFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType)
            {
                this._fieldSchemas = fieldSchemas;
                if ((fieldSchemas != null) && (fieldSchemas.Length > 0))
                {
                    this._haveSchema = true;
                }
                this._controlType = controlType;
                this.InitializeComponent();
            }

            protected string[] GetFieldSchemaNames()
            {
                if ((this._fieldSchemaNames == null) && (this._fieldSchemas != null))
                {
                    int length = this._fieldSchemas.Length;
                    this._fieldSchemaNames = new string[length];
                    for (int i = 0; i < length; i++)
                    {
                        this._fieldSchemaNames[i] = this._fieldSchemas[i].Name;
                    }
                }
                return this._fieldSchemaNames;
            }

            protected virtual void InitializeComponent()
            {
                this._headerTextLabel = new System.Windows.Forms.Label();
                this._headerTextBox = new System.Windows.Forms.TextBox();
                this._headerTextLabel.Text = System.Design.SR.GetString("DCFAdd_HeaderText");
                this._headerTextLabel.TextAlign = ContentAlignment.BottomLeft;
                this._headerTextLabel.SetBounds(0, 0, 270, 0x11);
                this._headerTextBox.TabIndex = 0;
                this._headerTextBox.SetBounds(0, 0x13, 270, 20);
                base.Controls.AddRange(new Control[] { this._headerTextLabel, this._headerTextBox });
            }

            public IDictionary PreserveFields()
            {
                Hashtable table = new Hashtable();
                table["HeaderText"] = this._headerTextBox.Text;
                this.PreserveFields(table);
                return table;
            }

            protected abstract void PreserveFields(IDictionary table);
            public void RefreshSchema(IDataSourceFieldSchema[] fieldSchemas)
            {
                this._fieldSchemas = fieldSchemas;
                this._fieldSchemaNames = null;
                if ((fieldSchemas != null) && (fieldSchemas.Length > 0))
                {
                    this._haveSchema = true;
                }
                this.RefreshSchemaFields();
            }

            protected virtual void RefreshSchemaFields()
            {
            }

            public void RestoreFields(IDictionary table)
            {
                this._headerTextBox.Text = table["HeaderText"].ToString();
                this.RestoreFieldsInternal(table);
            }

            protected abstract void RestoreFieldsInternal(IDictionary table);
            public DataControlField SaveValues()
            {
                string headerText = (this._headerTextBox == null) ? string.Empty : this._headerTextBox.Text;
                return this.SaveValues(headerText);
            }

            protected abstract DataControlField SaveValues(string headerText);
            protected string StripAccelerators(string text)
            {
                return text.Replace("&", string.Empty);
            }

            public abstract string FieldName { get; }
        }

        private class DataControlFieldDesignerControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            private DataBoundControlDesigner _controlDesigner;
            private DataControlFieldDesigner _designer;
            private DataControlField _field;
            private PropertyGrid _fieldProps;
            private IServiceProvider _serviceProvider;

            public DataControlFieldDesignerControl(DataBoundControlDesigner controlDesigner, IServiceProvider serviceProvider, DataControlFieldDesigner designer, IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
                this._controlDesigner = controlDesigner;
                this._serviceProvider = serviceProvider;
                this._designer = designer;
                this.Initialize();
            }

            private void Initialize()
            {
                this._field = this._designer.CreateField();
                this._fieldProps = new VsPropertyGrid(this._serviceProvider);
                this._fieldProps.SelectedObject = this._field;
                this._fieldProps.CommandsVisibleIfAvailable = true;
                this._fieldProps.LargeButtons = false;
                this._fieldProps.LineColor = SystemColors.ScrollBar;
                this._fieldProps.Name = "_fieldProps";
                this._fieldProps.Size = new Size(0xf8, 0x119);
                this._fieldProps.ToolbarVisible = true;
                this._fieldProps.ViewBackColor = SystemColors.Window;
                this._fieldProps.ViewForeColor = SystemColors.WindowText;
                this._fieldProps.Site = this._controlDesigner.Component.Site;
                base.Controls.Add(this._fieldProps);
            }

            protected override void InitializeComponent()
            {
            }

            protected override void PreserveFields(IDictionary table)
            {
            }

            protected override void RefreshSchemaFields()
            {
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
            }

            protected override DataControlField SaveValues(string headerText)
            {
                this._fieldProps.Refresh();
                return this._field;
            }

            public DataControlFieldDesigner Designer
            {
                get
                {
                    return this._designer;
                }
            }

            public override string FieldName
            {
                get
                {
                    return this._designer.DefaultNodeText;
                }
            }
        }

        private class DynamicDataFieldControl : AddDataControlFieldDialog.BoundFieldControl
        {
            private DataControlFieldDesigner _designer;

            public DynamicDataFieldControl(DataControlFieldDesigner dynamicFieldDesigner, IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
                this._designer = dynamicFieldDesigner;
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                base._readOnlyCheckBox.Visible = false;
            }

            protected override DataControlField SaveValues(string headerText)
            {
                DataControlField target = this._designer.CreateField();
                target.HeaderText = headerText;
                string str = base._haveSchema ? base._dataFieldList.Text : base._dataFieldBox.Text;
                SetProperty(target, "DataField", str);
                return target;
            }

            private static void SetProperty(DataControlField target, string propertyName, object value)
            {
                target.GetType().GetProperty(propertyName).SetValue(target, value, null);
            }

            public override string FieldName
            {
                get
                {
                    return "DynamicField";
                }
            }
        }

        private class HyperLinkFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            private System.Windows.Forms.Panel _bindTextPanel;
            private System.Windows.Forms.RadioButton _bindTextRadio;
            private System.Windows.Forms.Panel _bindUrlPanel;
            private System.Windows.Forms.RadioButton _bindUrlRadio;
            private System.Windows.Forms.TextBox _dataNavFieldBox;
            private ComboBox _dataNavFieldList;
            private System.Windows.Forms.TextBox _dataNavFSBox;
            private System.Windows.Forms.TextBox _dataTextFieldBox;
            private ComboBox _dataTextFieldList;
            private System.Windows.Forms.TextBox _linkBox;
            private GroupBox _linkGroupBox;
            private System.Windows.Forms.Label _linkTextFormatStringExampleLabel;
            private System.Windows.Forms.Label _linkTextFormatStringLabel;
            private System.Windows.Forms.Label _linkUrlFormatStringExampleLabel;
            private System.Windows.Forms.Label _linkUrlFormatStringLabel;
            private System.Windows.Forms.Panel _staticTextPanel;
            private System.Windows.Forms.RadioButton _staticTextRadio;
            private System.Windows.Forms.Panel _staticUrlPanel;
            private System.Windows.Forms.RadioButton _staticUrlRadio;
            private System.Windows.Forms.TextBox _textBox;
            private System.Windows.Forms.TextBox _textFSBox;
            private GroupBox _textGroupBox;

            public HyperLinkFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                this._dataTextFieldBox = new System.Windows.Forms.TextBox();
                this._dataNavFieldBox = new System.Windows.Forms.TextBox();
                this._dataNavFSBox = new System.Windows.Forms.TextBox();
                this._linkBox = new System.Windows.Forms.TextBox();
                this._textBox = new System.Windows.Forms.TextBox();
                this._textFSBox = new System.Windows.Forms.TextBox();
                this._dataTextFieldList = new ComboBox();
                this._dataNavFieldList = new ComboBox();
                this._staticTextRadio = new System.Windows.Forms.RadioButton();
                this._bindTextRadio = new System.Windows.Forms.RadioButton();
                this._staticUrlRadio = new System.Windows.Forms.RadioButton();
                this._bindUrlRadio = new System.Windows.Forms.RadioButton();
                this._linkTextFormatStringLabel = new System.Windows.Forms.Label();
                this._linkUrlFormatStringLabel = new System.Windows.Forms.Label();
                this._linkTextFormatStringExampleLabel = new System.Windows.Forms.Label();
                this._linkUrlFormatStringExampleLabel = new System.Windows.Forms.Label();
                this._textGroupBox = new GroupBox();
                this._linkGroupBox = new GroupBox();
                this._staticTextPanel = new System.Windows.Forms.Panel();
                this._bindTextPanel = new System.Windows.Forms.Panel();
                this._staticUrlPanel = new System.Windows.Forms.Panel();
                this._bindUrlPanel = new System.Windows.Forms.Panel();
                this._textGroupBox.SetBounds(0, 0x2f, 290, 0xa9);
                this._textGroupBox.Text = System.Design.SR.GetString("DCFAdd_HyperlinkText");
                this._textGroupBox.TabIndex = 1;
                this._staticTextRadio.TabIndex = 0;
                this._staticTextRadio.Text = System.Design.SR.GetString("DCFAdd_SpecifyText");
                this._staticTextRadio.CheckedChanged += new EventHandler(this.OnTextRadioChanged);
                this._staticTextRadio.Checked = true;
                this._staticTextRadio.SetBounds(9, 0x13, 0x105, 20);
                this._textBox.TabIndex = 0;
                this._textBox.SetBounds(0, 0, 0xf6, 20);
                this._textBox.AccessibleName = base.StripAccelerators(System.Design.SR.GetString("DCFAdd_SpecifyText"));
                this._staticTextPanel.TabIndex = 1;
                this._staticTextPanel.SetBounds(0x18, 0x27, 0xf6, 0x18);
                this._staticTextPanel.Controls.Add(this._textBox);
                this._bindTextRadio.TabIndex = 2;
                this._bindTextRadio.Text = System.Design.SR.GetString("DCFAdd_BindText");
                this._bindTextRadio.SetBounds(9, 0x3f, 0x105, 20);
                this._dataTextFieldList.TabIndex = 0;
                this._dataTextFieldList.SetBounds(0, 0, 0xf6, 20);
                this._dataTextFieldList.AccessibleName = base.StripAccelerators(System.Design.SR.GetString("DCFAdd_BindText"));
                this._dataTextFieldBox.TabIndex = 1;
                this._dataTextFieldBox.SetBounds(0, 0, 0xf6, 20);
                this._dataTextFieldBox.AccessibleName = base.StripAccelerators(System.Design.SR.GetString("DCFAdd_BindText"));
                this._linkTextFormatStringLabel.Text = System.Design.SR.GetString("DCFAdd_TextFormatString");
                this._linkTextFormatStringLabel.TabIndex = 2;
                this._linkTextFormatStringLabel.TextAlign = ContentAlignment.BottomLeft;
                this._linkTextFormatStringLabel.SetBounds(0, 20, 0xf6, 0x11);
                this._textFSBox.TabIndex = 3;
                this._textFSBox.SetBounds(0, 0x27, 0xf6, 20);
                this._linkTextFormatStringExampleLabel.Text = System.Design.SR.GetString("DCFAdd_TextFormatStringExample");
                this._linkTextFormatStringExampleLabel.Enabled = false;
                this._linkTextFormatStringExampleLabel.TextAlign = ContentAlignment.BottomLeft;
                this._linkTextFormatStringExampleLabel.SetBounds(0, 0x3b, 0xf6, 0x11);
                this._bindTextPanel.TabIndex = 3;
                this._bindTextPanel.SetBounds(0x18, 0x53, 0xf6, 0x4e);
                this._bindTextPanel.Controls.AddRange(new Control[] { this._bindTextRadio, this._dataTextFieldList, this._dataTextFieldBox, this._linkTextFormatStringLabel, this._textFSBox, this._linkTextFormatStringExampleLabel });
                this._textGroupBox.Controls.AddRange(new Control[] { this._staticTextRadio, this._staticTextPanel, this._bindTextRadio, this._bindTextPanel });
                this._linkGroupBox.SetBounds(0, 220, 290, 0xad);
                this._linkGroupBox.Text = System.Design.SR.GetString("DCFAdd_HyperlinkURL");
                this._linkGroupBox.TabIndex = 2;
                this._staticUrlRadio.TabIndex = 0;
                this._staticUrlRadio.Text = System.Design.SR.GetString("DCFAdd_SpecifyURL");
                this._staticUrlRadio.CheckedChanged += new EventHandler(this.OnUrlRadioChanged);
                this._staticUrlRadio.Checked = true;
                this._staticUrlRadio.SetBounds(9, 0x13, 0x105, 20);
                this._linkBox.TabIndex = 0;
                this._linkBox.SetBounds(0, 0, 0xf6, 20);
                this._linkBox.AccessibleName = base.StripAccelerators(System.Design.SR.GetString("DCFAdd_SpecifyURL"));
                this._staticUrlPanel.TabIndex = 1;
                this._staticUrlPanel.SetBounds(0x18, 0x27, 0xf6, 0x18);
                this._staticUrlPanel.Controls.Add(this._linkBox);
                this._bindUrlRadio.TabIndex = 2;
                this._bindUrlRadio.Text = System.Design.SR.GetString("DCFAdd_BindURL");
                this._bindUrlRadio.SetBounds(9, 0x3f, 0x105, 20);
                this._dataNavFieldList.TabIndex = 0;
                this._dataNavFieldList.SetBounds(0, 0, 0xf6, 20);
                this._dataNavFieldList.AccessibleName = base.StripAccelerators(System.Design.SR.GetString("DCFAdd_BindURL"));
                this._dataNavFieldBox.TabIndex = 1;
                this._dataNavFieldBox.SetBounds(0, 0, 0xf6, 20);
                this._dataNavFieldBox.AccessibleName = base.StripAccelerators(System.Design.SR.GetString("DCFAdd_BindURL"));
                this._linkUrlFormatStringLabel.Text = System.Design.SR.GetString("DCFAdd_URLFormatString");
                this._linkUrlFormatStringLabel.TabIndex = 2;
                this._linkUrlFormatStringLabel.TextAlign = ContentAlignment.BottomLeft;
                this._linkUrlFormatStringLabel.SetBounds(0, 20, 0xf6, 0x11);
                this._dataNavFSBox.TabIndex = 3;
                this._dataNavFSBox.SetBounds(0, 0x27, 0xf6, 20);
                this._linkUrlFormatStringExampleLabel.Text = System.Design.SR.GetString("DCFAdd_URLFormatStringExample");
                this._linkUrlFormatStringExampleLabel.Enabled = false;
                this._linkUrlFormatStringExampleLabel.TextAlign = ContentAlignment.BottomLeft;
                this._linkUrlFormatStringExampleLabel.SetBounds(0, 0x3b, 0xf6, 0x11);
                this._bindUrlPanel.TabIndex = 3;
                this._bindUrlPanel.SetBounds(0x18, 0x53, 0xf6, 0x4e);
                this._bindUrlPanel.Controls.AddRange(new Control[] { this._dataNavFieldList, this._dataNavFieldBox, this._linkUrlFormatStringLabel, this._dataNavFSBox, this._linkUrlFormatStringExampleLabel });
                this._linkGroupBox.Controls.AddRange(new Control[] { this._staticUrlRadio, this._staticUrlPanel, this._bindUrlRadio, this._bindUrlPanel });
                this.RefreshSchemaFields();
                base.Controls.AddRange(new Control[] { this._textGroupBox, this._linkGroupBox });
            }

            private void OnTextRadioChanged(object sender, EventArgs e)
            {
                if (this._staticTextRadio.Checked)
                {
                    this._textBox.Enabled = true;
                    this._dataTextFieldList.Enabled = false;
                    this._dataTextFieldBox.Enabled = false;
                    this._textFSBox.Enabled = false;
                    this._linkTextFormatStringLabel.Enabled = false;
                }
                else
                {
                    this._textBox.Enabled = false;
                    this._dataTextFieldList.Enabled = true;
                    this._dataTextFieldBox.Enabled = true;
                    this._textFSBox.Enabled = true;
                    this._linkTextFormatStringLabel.Enabled = true;
                }
            }

            private void OnUrlRadioChanged(object sender, EventArgs e)
            {
                if (this._staticUrlRadio.Checked)
                {
                    this._linkBox.Enabled = true;
                    this._dataNavFieldList.Enabled = false;
                    this._dataNavFieldBox.Enabled = false;
                    this._dataNavFSBox.Enabled = false;
                    this._linkUrlFormatStringLabel.Enabled = false;
                }
                else
                {
                    this._linkBox.Enabled = false;
                    this._dataNavFieldList.Enabled = true;
                    this._dataNavFieldBox.Enabled = true;
                    this._dataNavFSBox.Enabled = true;
                    this._linkUrlFormatStringLabel.Enabled = true;
                }
            }

            protected override void PreserveFields(IDictionary table)
            {
                if (base._haveSchema)
                {
                    table["DataTextField"] = this._dataTextFieldList.Text;
                    table["DataNavigateUrlField"] = this._dataNavFieldList.Text;
                }
                else
                {
                    table["DataTextField"] = this._dataTextFieldBox.Text;
                    table["DataNavigateUrlField"] = this._dataNavFieldBox.Text;
                }
                table["DataNavigateUrlFormatString"] = this._dataNavFSBox.Text;
                table["DataTextFormatString"] = this._textFSBox.Text;
                table["NavigateUrl"] = this._linkBox.Text;
                table["linkMode"] = this._staticUrlRadio.Checked;
                table["textMode"] = this._staticTextRadio.Checked;
                table["Text"] = this._textBox.Text;
            }

            protected override void RefreshSchemaFields()
            {
                if (base._haveSchema)
                {
                    this._dataTextFieldList.Items.Clear();
                    this._dataTextFieldList.Items.AddRange(base.GetFieldSchemaNames());
                    this._dataTextFieldList.Items.Insert(0, string.Empty);
                    this._dataTextFieldList.SelectedIndex = 0;
                    this._dataTextFieldList.Visible = true;
                    this._dataTextFieldBox.Visible = false;
                    this._dataNavFieldList.Items.Clear();
                    this._dataNavFieldList.Items.AddRange(base.GetFieldSchemaNames());
                    this._dataNavFieldList.Items.Insert(0, string.Empty);
                    this._dataNavFieldList.SelectedIndex = 0;
                    this._dataNavFieldList.Visible = true;
                    this._dataNavFieldBox.Visible = false;
                }
                else
                {
                    this._dataTextFieldList.Visible = false;
                    this._dataTextFieldBox.Visible = true;
                    this._dataNavFieldList.Visible = false;
                    this._dataNavFieldBox.Visible = true;
                }
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
                string strA = table["DataTextField"].ToString();
                string str2 = table["DataNavigateUrlField"].ToString();
                if (!base._haveSchema)
                {
                    this._dataTextFieldBox.Text = strA;
                    this._dataNavFieldBox.Text = str2;
                }
                else
                {
                    bool flag = false;
                    if (strA.Length > 0)
                    {
                        foreach (object obj2 in this._dataTextFieldList.Items)
                        {
                            if (string.Compare(strA, obj2.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this._dataTextFieldList.SelectedItem = obj2;
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            this._dataTextFieldList.Items.Insert(0, strA);
                            this._dataTextFieldList.SelectedIndex = 0;
                        }
                    }
                    if (str2.Length > 0)
                    {
                        flag = false;
                        foreach (object obj3 in this._dataNavFieldList.Items)
                        {
                            if (string.Compare(str2, obj3.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                this._dataNavFieldList.SelectedItem = obj3;
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            this._dataNavFieldList.Items.Insert(0, str2);
                            this._dataNavFieldList.SelectedIndex = 0;
                        }
                    }
                }
                this._dataNavFSBox.Text = table["DataNavigateUrlFormatString"].ToString();
                this._textFSBox.Text = table["DataTextFormatString"].ToString();
                this._linkBox.Text = table["NavigateUrl"].ToString();
                this._textBox.Text = table["Text"].ToString();
                this._staticUrlRadio.Checked = (bool) table["linkMode"];
                this._staticTextRadio.Checked = (bool) table["textMode"];
            }

            protected override DataControlField SaveValues(string headerText)
            {
                HyperLinkField field = new HyperLinkField {
                    HeaderText = headerText
                };
                if (this._staticTextRadio.Checked)
                {
                    field.Text = this._textBox.Text;
                }
                else
                {
                    field.DataTextFormatString = this._textFSBox.Text;
                    if (base._haveSchema)
                    {
                        field.DataTextField = this._dataTextFieldList.Text;
                    }
                    else
                    {
                        field.DataTextField = this._dataTextFieldBox.Text;
                    }
                }
                if (this._staticUrlRadio.Checked)
                {
                    field.NavigateUrl = this._linkBox.Text;
                    return field;
                }
                field.DataNavigateUrlFormatString = this._dataNavFSBox.Text;
                if (base._haveSchema)
                {
                    field.DataNavigateUrlFields = new string[] { this._dataNavFieldList.Text };
                    return field;
                }
                field.DataNavigateUrlFields = new string[] { this._dataNavFieldBox.Text };
                return field;
            }

            public override string FieldName
            {
                get
                {
                    return "HyperLinkField";
                }
            }
        }

        private class ImageFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            private System.Windows.Forms.TextBox _imageUrlFieldBox;
            private System.Windows.Forms.Label _imageUrlFieldLabel;
            private ComboBox _imageUrlFieldList;
            private System.Windows.Forms.CheckBox _readOnlyCheckBox;
            private System.Windows.Forms.TextBox _urlFormatBox;
            private System.Windows.Forms.Label _urlFormatBoxLabel;
            private System.Windows.Forms.Label _urlFormatExampleLabel;

            public ImageFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
                this._imageUrlFieldList = new ComboBox();
                this._imageUrlFieldBox = new System.Windows.Forms.TextBox();
                this._imageUrlFieldLabel = new System.Windows.Forms.Label();
                this._readOnlyCheckBox = new System.Windows.Forms.CheckBox();
                this._urlFormatBox = new System.Windows.Forms.TextBox();
                this._urlFormatBoxLabel = new System.Windows.Forms.Label();
                this._urlFormatExampleLabel = new System.Windows.Forms.Label();
                this._imageUrlFieldLabel.Text = System.Design.SR.GetString("DCFAdd_DataField");
                this._imageUrlFieldLabel.TextAlign = ContentAlignment.BottomLeft;
                this._imageUrlFieldLabel.SetBounds(0, 0x2b, 270, 0x11);
                this._imageUrlFieldList.DropDownStyle = ComboBoxStyle.DropDownList;
                this._imageUrlFieldList.TabIndex = 1;
                this._imageUrlFieldList.SetBounds(0, 0x3e, 270, 20);
                this._imageUrlFieldBox.TabIndex = 2;
                this._imageUrlFieldBox.SetBounds(0, 0x3e, 270, 20);
                this._urlFormatBoxLabel.TabIndex = 3;
                this._urlFormatBoxLabel.Text = System.Design.SR.GetString("DCFAdd_LinkFormatString");
                this._urlFormatBoxLabel.TextAlign = ContentAlignment.BottomLeft;
                this._urlFormatBoxLabel.SetBounds(0, 0x56, 270, 0x11);
                this._urlFormatBox.TabIndex = 4;
                this._urlFormatBox.SetBounds(0, 0x69, 270, 20);
                this._urlFormatExampleLabel.Enabled = false;
                this._urlFormatExampleLabel.Text = System.Design.SR.GetString("DCFAdd_ExampleFormatString");
                this._urlFormatExampleLabel.TextAlign = ContentAlignment.BottomLeft;
                this._urlFormatExampleLabel.SetBounds(0, 0x7d, 270, 0x11);
                this._readOnlyCheckBox.TabIndex = 5;
                this._readOnlyCheckBox.Text = System.Design.SR.GetString("DCFAdd_ReadOnly");
                this._readOnlyCheckBox.SetBounds(0, 0x90, 270, 20);
                if (base._haveSchema)
                {
                    this._imageUrlFieldList.Items.AddRange(base.GetFieldSchemaNames());
                    this._imageUrlFieldList.SelectedIndex = 0;
                    this._imageUrlFieldList.Visible = true;
                    this._imageUrlFieldBox.Visible = false;
                }
                else
                {
                    this._imageUrlFieldList.Visible = false;
                    this._imageUrlFieldBox.Visible = true;
                }
                base.Controls.AddRange(new Control[] { this._imageUrlFieldLabel, this._imageUrlFieldBox, this._imageUrlFieldList, this._readOnlyCheckBox, this._urlFormatBoxLabel, this._urlFormatBox, this._urlFormatExampleLabel });
            }

            protected override void PreserveFields(IDictionary table)
            {
                if (base._haveSchema)
                {
                    table["ImageUrlField"] = this._imageUrlFieldList.Text;
                }
                else
                {
                    table["ImageUrlField"] = this._imageUrlFieldBox.Text;
                }
                table["ReadOnly"] = this._readOnlyCheckBox.Checked;
                table["FormatString"] = this._urlFormatBox.Text;
            }

            protected override void RefreshSchemaFields()
            {
                if (base._haveSchema)
                {
                    this._imageUrlFieldList.Items.Clear();
                    this._imageUrlFieldList.Items.AddRange(base.GetFieldSchemaNames());
                    this._imageUrlFieldList.SelectedIndex = 0;
                    this._imageUrlFieldList.Visible = true;
                    this._imageUrlFieldBox.Visible = false;
                }
                else
                {
                    this._imageUrlFieldList.Visible = false;
                    this._imageUrlFieldBox.Visible = true;
                }
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
                string strA = table["ImageUrlField"].ToString();
                if (!base._haveSchema)
                {
                    this._imageUrlFieldBox.Text = strA;
                }
                else if (strA.Length > 0)
                {
                    bool flag = false;
                    foreach (object obj2 in this._imageUrlFieldList.Items)
                    {
                        if (string.Compare(strA, obj2.ToString(), StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            this._imageUrlFieldList.SelectedItem = obj2;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        this._imageUrlFieldList.Items.Insert(0, strA);
                        this._imageUrlFieldList.SelectedIndex = 0;
                    }
                }
                this._readOnlyCheckBox.Checked = (bool) table["ReadOnly"];
                this._urlFormatBox.Text = (string) table["FormatString"];
            }

            protected override DataControlField SaveValues(string headerText)
            {
                ImageField field = new ImageField {
                    HeaderText = headerText
                };
                if (base._haveSchema)
                {
                    field.DataImageUrlField = this._imageUrlFieldList.Text;
                }
                else
                {
                    field.DataImageUrlField = this._imageUrlFieldBox.Text;
                }
                field.ReadOnly = this._readOnlyCheckBox.Checked;
                field.DataImageUrlFormatString = this._urlFormatBox.Text;
                return field;
            }

            public override string FieldName
            {
                get
                {
                    return "ImageField";
                }
            }
        }

        private class TemplateFieldControl : AddDataControlFieldDialog.DataControlFieldControl
        {
            public TemplateFieldControl(IDataSourceFieldSchema[] fieldSchemas, System.Type controlType) : base(fieldSchemas, controlType)
            {
            }

            protected override void InitializeComponent()
            {
                base.InitializeComponent();
            }

            protected override void PreserveFields(IDictionary table)
            {
            }

            protected override void RestoreFieldsInternal(IDictionary table)
            {
            }

            protected override DataControlField SaveValues(string headerText)
            {
                return new TemplateField { HeaderText = headerText };
            }

            public override string FieldName
            {
                get
                {
                    return "TemplateField";
                }
            }
        }
    }
}

