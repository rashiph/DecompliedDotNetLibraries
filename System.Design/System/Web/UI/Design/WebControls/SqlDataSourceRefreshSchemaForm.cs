namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Data;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceRefreshSchemaForm : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _commandLabel;
        private System.Windows.Forms.TextBox _commandTextBox;
        private string _connectionString;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Button _okButton;
        private DataGridView _parametersDataGridView;
        private System.Windows.Forms.Label _parametersLabel;
        private string _providerName;
        private string _selectCommand;
        private SqlDataSourceCommandType _selectCommandType;
        private SqlDataSource _sqlDataSource;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;

        public SqlDataSourceRefreshSchemaForm(IServiceProvider serviceProvider, SqlDataSourceDesigner sqlDataSourceDesigner, ParameterCollection parameters) : base(serviceProvider)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this._sqlDataSource = (SqlDataSource) this._sqlDataSourceDesigner.Component;
            this._connectionString = this._sqlDataSourceDesigner.ConnectionString;
            this._providerName = this._sqlDataSourceDesigner.ProviderName;
            this._selectCommand = this._sqlDataSourceDesigner.SelectCommand;
            this._selectCommandType = this._sqlDataSource.SelectCommandType;
            this.InitializeComponent();
            this.InitializeUI();
            Array values = Enum.GetValues(typeof(TypeCode));
            Array.Sort(values, new TypeCodeComparer());
            foreach (TypeCode code in values)
            {
                ((DataGridViewComboBoxColumn) this._parametersDataGridView.Columns[1]).Items.Add(code);
            }
            Array array = Enum.GetValues(typeof(DbType));
            Array.Sort(array, new DbTypeComparer());
            foreach (DbType type in array)
            {
                ((DataGridViewComboBoxColumn) this._parametersDataGridView.Columns[2]).Items.Add(type);
            }
            ArrayList list = new ArrayList(parameters.Count);
            foreach (Parameter parameter in parameters)
            {
                list.Add(new ParameterItem(parameter));
            }
            this._parametersDataGridView.DataSource = list;
            this._commandTextBox.Text = this._selectCommand;
            this._commandTextBox.Select(0, 0);
        }

        private void InitializeComponent()
        {
            DataGridViewTextBoxColumn dataGridViewColumn = new DataGridViewTextBoxColumn();
            DataGridViewCellStyle style = new DataGridViewCellStyle();
            DataGridViewComboBoxColumn column2 = new DataGridViewComboBoxColumn();
            DataGridViewComboBoxColumn column3 = new DataGridViewComboBoxColumn();
            DataGridViewTextBoxColumn column4 = new DataGridViewTextBoxColumn();
            this._parametersDataGridView = new DataGridView();
            this._okButton = new System.Windows.Forms.Button();
            this._commandTextBox = new System.Windows.Forms.TextBox();
            this._commandLabel = new System.Windows.Forms.Label();
            this._helpLabel = new System.Windows.Forms.Label();
            this._cancelButton = new System.Windows.Forms.Button();
            this._parametersLabel = new System.Windows.Forms.Label();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 12);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x240, 0x2f);
            this._helpLabel.TabIndex = 10;
            this._commandLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._commandLabel.Location = new Point(12, 0x40);
            this._commandLabel.Name = "_commandLabel";
            this._commandLabel.Size = new Size(0x240, 0x10);
            this._commandLabel.TabIndex = 20;
            this._commandTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._commandTextBox.BackColor = SystemColors.Control;
            this._commandTextBox.Location = new Point(12, 0x52);
            this._commandTextBox.Multiline = true;
            this._commandTextBox.Name = "_commandTextBox";
            this._commandTextBox.ReadOnly = true;
            this._commandTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._commandTextBox.Size = new Size(0x240, 50);
            this._commandTextBox.TabIndex = 30;
            this._parametersLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._parametersLabel.Location = new Point(13, 0x8e);
            this._parametersLabel.Name = "_parametersLabel";
            this._parametersLabel.Size = new Size(0x240, 0x10);
            this._parametersLabel.TabIndex = 40;
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
            this._parametersDataGridView.Location = new Point(12, 160);
            this._parametersDataGridView.MultiSelect = false;
            this._parametersDataGridView.Name = "_parametersDataGridView";
            this._parametersDataGridView.RowHeadersVisible = false;
            this._parametersDataGridView.Size = new Size(0x240, 0x9c);
            this._parametersDataGridView.TabIndex = 50;
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(0x1b0, 0x14b);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 60;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x201, 0x14b);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 70;
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(600, 0x16e);
            base.Controls.Add(this._parametersLabel);
            base.Controls.Add(this._parametersDataGridView);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._commandLabel);
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._commandTextBox);
            this.MinimumSize = new Size(600, 0x16e);
            base.Name = "SqlDataSourceRefreshSchemaForm";
            base.SizeGripStyle = SizeGripStyle.Show;
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this.Text = System.Design.SR.GetString("SqlDataSourceRefreshSchemaForm_Title", new object[] { this._sqlDataSource.ID });
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceRefreshSchemaForm_HelpLabel");
            this._commandLabel.Text = System.Design.SR.GetString("SqlDataSource_General_PreviewLabel");
            this._parametersLabel.Text = System.Design.SR.GetString("SqlDataSourceRefreshSchemaForm_ParametersLabel");
            this._parametersDataGridView.AccessibleName = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_ParametersGridAccessibleName");
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this._parametersDataGridView.Columns[0].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_ParameterColumnHeader");
            this._parametersDataGridView.Columns[1].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_TypeColumnHeader");
            this._parametersDataGridView.Columns[2].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_DbTypeColumnHeader");
            this._parametersDataGridView.Columns[3].HeaderText = System.Design.SR.GetString("SqlDataSourceParameterValueEditorForm_ValueColumnHeader");
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            ICollection dataSource = (ICollection) this._parametersDataGridView.DataSource;
            ParameterCollection parameters = new ParameterCollection();
            foreach (ParameterItem item in dataSource)
            {
                if (item.DbType == DbType.Object)
                {
                    parameters.Add(new Parameter(item.Name, item.Type, item.DefaultValue));
                }
                else
                {
                    parameters.Add(new Parameter(item.Name, item.DbType, item.DefaultValue));
                }
            }
            if (this._sqlDataSourceDesigner.RefreshSchema(new DesignerDataConnection(string.Empty, this._providerName, this._connectionString), this._selectCommand, this._selectCommandType, parameters, false))
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
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
                return "net.Asp.SqlDataSource.RefreshSchema";
            }
        }

        private sealed class DbTypeComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return string.Compare(Enum.GetName(typeof(DbType), x), Enum.GetName(typeof(DbType), y), StringComparison.OrdinalIgnoreCase);
            }
        }

        private sealed class ParameterItem
        {
            private System.Data.DbType _dbType;
            private string _defaultValue;
            private string _name;
            private TypeCode _type;

            public ParameterItem(Parameter p)
            {
                this._name = p.Name;
                this._dbType = p.DbType;
                this._type = p.Type;
                this._defaultValue = p.DefaultValue;
            }

            public System.Data.DbType DbType
            {
                get
                {
                    return this._dbType;
                }
                set
                {
                    this._dbType = value;
                }
            }

            public string DefaultValue
            {
                get
                {
                    return this._defaultValue;
                }
                set
                {
                    this._defaultValue = value;
                }
            }

            public string Name
            {
                get
                {
                    return this._name;
                }
            }

            public TypeCode Type
            {
                get
                {
                    return this._type;
                }
                set
                {
                    this._type = value;
                }
            }
        }

        private sealed class TypeCodeComparer : IComparer
        {
            int IComparer.Compare(object x, object y)
            {
                return string.Compare(Enum.GetName(typeof(TypeCode), x), Enum.GetName(typeof(TypeCode), y), StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

