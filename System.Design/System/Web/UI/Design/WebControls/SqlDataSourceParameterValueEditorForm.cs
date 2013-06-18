namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceParameterValueEditorForm : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Button _okButton;
        private ArrayList _parameterItems;
        private DataGridView _parametersDataGridView;

        public SqlDataSourceParameterValueEditorForm(IServiceProvider serviceProvider, ParameterCollection parameters) : base(serviceProvider)
        {
            this._parameterItems = new ArrayList();
            foreach (Parameter parameter in parameters)
            {
                this._parameterItems.Add(new ParameterItem(parameter));
            }
            this.InitializeComponent();
            this.InitializeUI();
            string[] names = Enum.GetNames(typeof(TypeCode));
            Array.Sort<string>(names);
            ((DataGridViewComboBoxColumn) this._parametersDataGridView.Columns[1]).Items.AddRange(names);
            string[] array = Enum.GetNames(typeof(DbType));
            Array.Sort<string>(array);
            ((DataGridViewComboBoxColumn) this._parametersDataGridView.Columns[2]).Items.AddRange(array);
            this._parametersDataGridView.DataSource = this._parameterItems;
        }

        private void InitializeComponent()
        {
            DataGridViewTextBoxColumn dataGridViewColumn = new DataGridViewTextBoxColumn();
            DataGridViewCellStyle style = new DataGridViewCellStyle();
            DataGridViewComboBoxColumn column2 = new DataGridViewComboBoxColumn();
            DataGridViewComboBoxColumn column3 = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn();
            this._parametersDataGridView = new DataGridView();
            this._helpLabel = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 12);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(460, 0x20);
            this._helpLabel.TabIndex = 10;
            this._parametersDataGridView.AllowUserToAddRows = false;
            this._parametersDataGridView.AllowUserToDeleteRows = false;
            this._parametersDataGridView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._parametersDataGridView.AutoGenerateColumns = false;
            this._parametersDataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewColumn.DataPropertyName = "Name";
            dataGridViewColumn.DefaultCellStyle = style;
            dataGridViewColumn.Name = "_parameterNameColumn";
            dataGridViewColumn.ReadOnly = true;
            dataGridViewColumn.ValueType = typeof(string);
            column2.DataPropertyName = "Type";
            column2.DefaultCellStyle = style;
            column2.Name = "_parameterTypeColumn";
            column2.ValueType = typeof(string);
            column3.DataPropertyName = "DbType";
            column3.DefaultCellStyle = style;
            column3.Name = "_parameterDbTypeColumn";
            column3.ValueType = typeof(string);
            column4.DataPropertyName = "DefaultValue";
            column4.DefaultCellStyle = style;
            column4.Name = "_parameterValueColumn";
            column4.ValueType = typeof(string);
            this._parametersDataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
            this._parametersDataGridView.Columns.Add(dataGridViewColumn);
            this._parametersDataGridView.Columns.Add(column2);
            this._parametersDataGridView.Columns.Add(column3);
            this._parametersDataGridView.Columns.Add(column4);
            this._parametersDataGridView.Location = new Point(12, 0x33);
            this._parametersDataGridView.MultiSelect = false;
            this._parametersDataGridView.Name = "_parametersDataGridView";
            this._parametersDataGridView.RowHeadersVisible = false;
            this._parametersDataGridView.Size = new Size(460, 0xd7);
            this._parametersDataGridView.TabIndex = 20;
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(0x13c, 280);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 30;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x18d, 280);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 40;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x1e4, 0x13b);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._parametersDataGridView);
            this.MinimumSize = new Size(0x1e4, 0x13b);
            base.Name = "SqlDataSourceParameterValueEditorForm";
            base.SizeGripStyle = SizeGripStyle.Show;
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_HelpLabel");
            this._parametersDataGridView.AccessibleName = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_ParametersGridAccessibleName");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this._okButton.Text = System.Design.SR.GetString("OK");
            this.Text = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_Caption");
            this._parametersDataGridView.Columns[0].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_ParameterColumnHeader");
            this._parametersDataGridView.Columns[1].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_TypeColumnHeader");
            this._parametersDataGridView.Columns[2].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_DbTypeColumnHeader");
            this._parametersDataGridView.Columns[3].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_ValueColumnHeader");
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            ParameterCollection parameters = new ParameterCollection();
            foreach (ParameterItem item in this._parameterItems)
            {
                if (item.Parameter.DbType == DbType.Object)
                {
                    parameters.Add(new Parameter(item.Parameter.Name, item.Parameter.Type, item.Parameter.DefaultValue));
                }
                else
                {
                    parameters.Add(new Parameter(item.Parameter.Name, item.Parameter.DbType, item.Parameter.DefaultValue));
                }
            }
            try
            {
                parameters.GetValues(null, null);
            }
            catch (Exception exception)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_InvalidParameter"));
                return;
            }
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                int num = (int) Math.Floor((double) (((double) ((this._parametersDataGridView.ClientSize.Width - SystemInformation.VerticalScrollBarWidth) - (2 * SystemInformation.Border3DSize.Width))) / 4.5));
                this._parametersDataGridView.Columns[0].Width = (int) (num * 1.5);
                this._parametersDataGridView.Columns[1].Width = num;
                this._parametersDataGridView.Columns[2].Width = num;
                this._parametersDataGridView.Columns[3].Width = num;
                this._parametersDataGridView.AutoResizeColumnHeadersHeight();
                for (int i = 0; i < this._parametersDataGridView.Rows.Count; i++)
                {
                    this._parametersDataGridView.AutoResizeRow(i, DataGridViewAutoSizeRowMode.AllCells);
                }
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.SqlDataSource.ParameterValueEditor";
            }
        }

        private class ParameterItem
        {
            private System.Web.UI.WebControls.Parameter _parameter;

            public ParameterItem(System.Web.UI.WebControls.Parameter parameter)
            {
                this._parameter = parameter;
            }

            public string DbType
            {
                get
                {
                    return this._parameter.DbType.ToString();
                }
                set
                {
                    this._parameter.DbType = (System.Data.DbType) Enum.Parse(typeof(System.Data.DbType), value);
                }
            }

            public string DefaultValue
            {
                get
                {
                    return this._parameter.DefaultValue;
                }
                set
                {
                    this._parameter.DefaultValue = value;
                }
            }

            public string Name
            {
                get
                {
                    return this._parameter.Name;
                }
                set
                {
                    this._parameter.Name = value;
                }
            }

            public System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }

            public string Type
            {
                get
                {
                    return this._parameter.Type.ToString();
                }
                set
                {
                    this._parameter.Type = (TypeCode) Enum.Parse(typeof(TypeCode), value);
                }
            }
        }
    }
}

