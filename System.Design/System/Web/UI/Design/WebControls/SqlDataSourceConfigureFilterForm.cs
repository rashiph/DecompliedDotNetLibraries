namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Data;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceConfigureFilterForm : DesignerForm
    {
        private System.Windows.Forms.Button _addButton;
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _columnLabel;
        private AutoSizeComboBox _columnsComboBox;
        private SqlDataSource _dataSource;
        private ColumnHeader _expressionColumnHeader;
        private System.Windows.Forms.Label _expressionLabel;
        private System.Windows.Forms.TextBox _expressionTextBox;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Button _okButton;
        private System.Windows.Forms.Label _operatorLabel;
        private AutoSizeComboBox _operatorsComboBox;
        private static IDictionary<System.Type, ParameterEditor> _parameterEditors;
        private GroupBox _propertiesGroupBox;
        private System.Windows.Forms.Panel _propertiesPanel;
        private System.ComponentModel.TypeDescriptionProvider _provider;
        private System.Windows.Forms.Button _removeButton;
        private IServiceProvider _serviceProvider;
        private AutoSizeComboBox _sourceComboBox;
        private System.Windows.Forms.Label _sourceLabel;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private SqlDataSourceTableQuery _tableQuery;
        private ColumnHeader _valueColumnHeader;
        private System.Windows.Forms.Label _valueLabel;
        private System.Windows.Forms.TextBox _valueTextBox;
        private System.Windows.Forms.Label _whereClausesLabel;
        private ListView _whereClausesListView;

        public SqlDataSourceConfigureFilterForm(SqlDataSourceDesigner sqlDataSourceDesigner, SqlDataSourceTableQuery tableQuery) : base(sqlDataSourceDesigner.Component.Site)
        {
            this._dataSource = (SqlDataSource) sqlDataSourceDesigner.Component;
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this._tableQuery = tableQuery.Clone();
            this.InitializeComponent();
            this.InitializeUI();
            _parameterEditors = this.CreateParameterList();
            foreach (ParameterEditor editor in _parameterEditors.Values)
            {
                editor.Visible = false;
                this._propertiesPanel.Controls.Add(editor);
                this._sourceComboBox.Items.Add(editor);
                editor.ParameterChanged += new EventHandler(this.OnParameterChanged);
            }
            this._sourceComboBox.InvalidateDropDownWidth();
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                foreach (DesignerDataColumn column in tableQuery.DesignerDataTable.Columns)
                {
                    this._columnsComboBox.Items.Add(new ColumnItem(column));
                }
                this._columnsComboBox.InvalidateDropDownWidth();
                foreach (SqlDataSourceFilterClause clause in this._tableQuery.FilterClauses)
                {
                    FilterClauseItem item = new FilterClauseItem(this._sqlDataSourceDesigner.Component.Site, this._tableQuery, clause, (SqlDataSource) this._sqlDataSourceDesigner.Component);
                    this._whereClausesListView.Items.Add(item);
                    item.Refresh();
                }
                if (this._whereClausesListView.Items.Count > 0)
                {
                    this._whereClausesListView.Items[0].Selected = true;
                    this._whereClausesListView.Items[0].Focused = true;
                }
                this._okButton.Enabled = false;
                this.UpdateDeleteButton();
                this.UpdateOperators();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        internal SqlDataSourceConfigureFilterForm(ISite site, IServiceProvider serviceProvider, SqlDataSource dataSource, System.ComponentModel.TypeDescriptionProvider provider) : base(site)
        {
            this._serviceProvider = serviceProvider;
            this._dataSource = dataSource;
            this._provider = provider;
        }

        internal IDictionary<System.Type, ParameterEditor> CreateParameterList()
        {
            Dictionary<System.Type, ParameterEditor> dictionary = new Dictionary<System.Type, ParameterEditor>();
            dictionary.Add(typeof(Parameter), new StaticParameterEditor(this.ServiceProvider));
            dictionary.Add(typeof(ControlParameter), new ControlParameterEditor(this.ServiceProvider, this._dataSource));
            dictionary.Add(typeof(CookieParameter), new CookieParameterEditor(this.ServiceProvider));
            dictionary.Add(typeof(FormParameter), new FormParameterEditor(this.ServiceProvider));
            dictionary.Add(typeof(ProfileParameter), new ProfileParameterEditor(this.ServiceProvider));
            dictionary.Add(typeof(QueryStringParameter), new QueryStringParameterEditor(this.ServiceProvider));
            dictionary.Add(typeof(SessionParameter), new SessionParameterEditor(this.ServiceProvider));
            if (this.TypeDescriptionProvider.IsSupportedType(typeof(RouteParameter)))
            {
                dictionary.Add(typeof(RouteParameter), new RouteParameterEditor(this.ServiceProvider));
            }
            return dictionary;
        }

        private SqlDataSourceFilterClause GetCurrentFilterClause()
        {
            string parameterPlaceholder;
            Parameter parameter;
            OperatorItem selectedItem = this._operatorsComboBox.SelectedItem as OperatorItem;
            if (selectedItem == null)
            {
                return null;
            }
            ColumnItem item2 = this._columnsComboBox.SelectedItem as ColumnItem;
            if (item2 == null)
            {
                return null;
            }
            if (selectedItem.IsBinary)
            {
                ParameterEditor editor = this._sourceComboBox.SelectedItem as ParameterEditor;
                if (editor == null)
                {
                    return null;
                }
                parameter = editor.Parameter;
                if (parameter != null)
                {
                    SqlDataSourceQuery selectQuery = this._tableQuery.GetSelectQuery();
                    StringCollection usedNames = new StringCollection();
                    if ((selectQuery != null) && (selectQuery.Parameters != null))
                    {
                        foreach (Parameter parameter2 in selectQuery.Parameters)
                        {
                            usedNames.Add(parameter2.Name);
                        }
                    }
                    SqlDataSourceColumnData data = new SqlDataSourceColumnData(this._tableQuery.DesignerDataConnection, item2.DesignerDataColumn, usedNames);
                    parameter.Name = data.WebParameterName;
                    if (SqlDataSourceDesigner.IsNewSqlServer2008Type(SqlDataSourceDesigner.GetDbProviderFactory(this._tableQuery.DesignerDataConnection.ProviderName), item2.DesignerDataColumn.DataType))
                    {
                        parameter.DbType = item2.DesignerDataColumn.DataType;
                        parameter.Type = TypeCode.Empty;
                    }
                    else
                    {
                        parameter.DbType = DbType.Object;
                        parameter.Type = SqlDataSourceDesigner.ConvertDbTypeToTypeCode(item2.DesignerDataColumn.DataType);
                    }
                    parameterPlaceholder = data.ParameterPlaceholder;
                }
                else
                {
                    parameterPlaceholder = string.Empty;
                }
            }
            else
            {
                parameterPlaceholder = "";
                parameter = null;
            }
            return new SqlDataSourceFilterClause(this._tableQuery.DesignerDataConnection, this._tableQuery.DesignerDataTable, item2.DesignerDataColumn, selectedItem.OperatorFormat, selectedItem.IsBinary, parameterPlaceholder, parameter);
        }

        private void InitializeComponent()
        {
            this._helpLabel = new System.Windows.Forms.Label();
            this._columnLabel = new System.Windows.Forms.Label();
            this._columnsComboBox = new AutoSizeComboBox();
            this._operatorsComboBox = new AutoSizeComboBox();
            this._operatorLabel = new System.Windows.Forms.Label();
            this._whereClausesLabel = new System.Windows.Forms.Label();
            this._addButton = new System.Windows.Forms.Button();
            this._removeButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._okButton = new System.Windows.Forms.Button();
            this._expressionLabel = new System.Windows.Forms.Label();
            this._propertiesGroupBox = new GroupBox();
            this._propertiesPanel = new System.Windows.Forms.Panel();
            this._sourceComboBox = new AutoSizeComboBox();
            this._sourceLabel = new System.Windows.Forms.Label();
            this._expressionTextBox = new System.Windows.Forms.TextBox();
            this._whereClausesListView = new ListView();
            this._expressionColumnHeader = new ColumnHeader("");
            this._valueColumnHeader = new ColumnHeader("");
            this._valueTextBox = new System.Windows.Forms.TextBox();
            this._valueLabel = new System.Windows.Forms.Label();
            this._propertiesGroupBox.SuspendLayout();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 11);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x20c, 0x2a);
            this._helpLabel.TabIndex = 10;
            this._columnLabel.Location = new Point(12, 0x3b);
            this._columnLabel.Name = "_columnLabel";
            this._columnLabel.Size = new Size(0xac, 15);
            this._columnLabel.TabIndex = 20;
            this._columnsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._columnsComboBox.Location = new Point(12, 0x4d);
            this._columnsComboBox.Name = "_columnsComboBox";
            this._columnsComboBox.Size = new Size(0xac, 0x15);
            this._columnsComboBox.Sorted = true;
            this._columnsComboBox.TabIndex = 30;
            this._columnsComboBox.SelectedIndexChanged += new EventHandler(this.OnColumnsComboBoxSelectedIndexChanged);
            this._operatorLabel.Location = new Point(12, 0x68);
            this._operatorLabel.Name = "_operatorLabel";
            this._operatorLabel.Size = new Size(0xac, 15);
            this._operatorLabel.TabIndex = 40;
            this._operatorsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._operatorsComboBox.Location = new Point(12, 0x7a);
            this._operatorsComboBox.Name = "_operatorsComboBox";
            this._operatorsComboBox.Size = new Size(0xac, 0x15);
            this._operatorsComboBox.TabIndex = 50;
            this._operatorsComboBox.SelectedIndexChanged += new EventHandler(this.OnOperatorsComboBoxSelectedIndexChanged);
            this._sourceLabel.Location = new Point(12, 0x94);
            this._sourceLabel.Name = "_sourceLabel";
            this._sourceLabel.Size = new Size(0xac, 15);
            this._sourceLabel.TabIndex = 60;
            this._sourceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._sourceComboBox.Location = new Point(12, 0xa6);
            this._sourceComboBox.Name = "_sourceComboBox";
            this._sourceComboBox.Size = new Size(0xac, 0x15);
            this._sourceComboBox.TabIndex = 70;
            this._sourceComboBox.SelectedIndexChanged += new EventHandler(this.OnSourceComboBoxSelectedIndexChanged);
            this._propertiesGroupBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._propertiesGroupBox.Controls.Add(this._propertiesPanel);
            this._propertiesGroupBox.Location = new Point(0xf3, 0x3b);
            this._propertiesGroupBox.Name = "_propertiesGroupBox";
            this._propertiesGroupBox.Size = new Size(0xc2, 0x7f);
            this._propertiesGroupBox.TabIndex = 80;
            this._propertiesGroupBox.TabStop = false;
            this._propertiesPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._propertiesPanel.Location = new Point(10, 15);
            this._propertiesPanel.Name = "_propertiesPanel";
            this._propertiesPanel.Size = new Size(0xa4, 100);
            this._propertiesPanel.TabIndex = 10;
            this._expressionLabel.Location = new Point(12, 0xc2);
            this._expressionLabel.Name = "_expressionLabel";
            this._expressionLabel.Size = new Size(0xe1, 15);
            this._expressionLabel.TabIndex = 90;
            this._expressionTextBox.Location = new Point(12, 0xd4);
            this._expressionTextBox.Name = "_expressionTextBox";
            this._expressionTextBox.ReadOnly = true;
            this._expressionTextBox.Size = new Size(0xe0, 20);
            this._expressionTextBox.TabIndex = 100;
            this._valueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._valueLabel.Location = new Point(0xf3, 0xc2);
            this._valueLabel.Name = "_valueLabel";
            this._valueLabel.Size = new Size(0xc2, 15);
            this._valueLabel.TabIndex = 110;
            this._valueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._valueTextBox.Location = new Point(0xf3, 0xd4);
            this._valueTextBox.Name = "_valueTextBox";
            this._valueTextBox.ReadOnly = true;
            this._valueTextBox.Size = new Size(0xc2, 20);
            this._valueTextBox.TabIndex = 120;
            this._addButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._addButton.Location = new Point(0x1bb, 0xd4);
            this._addButton.Name = "_addButton";
            this._addButton.Size = new Size(90, 0x17);
            this._addButton.TabIndex = 0x7d;
            this._addButton.Click += new EventHandler(this.OnAddButtonClick);
            this._whereClausesLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._whereClausesLabel.Location = new Point(12, 0xf2);
            this._whereClausesLabel.Name = "_whereClausesLabel";
            this._whereClausesLabel.Size = new Size(0x1a9, 15);
            this._whereClausesLabel.TabIndex = 130;
            this._whereClausesListView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._whereClausesListView.Columns.AddRange(new ColumnHeader[] { this._expressionColumnHeader, this._valueColumnHeader });
            this._whereClausesListView.FullRowSelect = true;
            this._whereClausesListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this._whereClausesListView.HideSelection = false;
            this._whereClausesListView.Location = new Point(12, 260);
            this._whereClausesListView.MultiSelect = false;
            this._whereClausesListView.Name = "_whereClausesListView";
            this._whereClausesListView.Size = new Size(0x1a9, 0x4e);
            this._whereClausesListView.TabIndex = 0x87;
            this._whereClausesListView.View = System.Windows.Forms.View.Details;
            this._whereClausesListView.SelectedIndexChanged += new EventHandler(this.OnWhereClausesListViewSelectedIndexChanged);
            this._expressionColumnHeader.Text = "";
            this._expressionColumnHeader.Width = 0xe1;
            this._valueColumnHeader.Text = "";
            this._valueColumnHeader.Width = 160;
            this._removeButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._removeButton.Location = new Point(0x1ba, 260);
            this._removeButton.Name = "_removeButton";
            this._removeButton.Size = new Size(90, 0x17);
            this._removeButton.TabIndex = 140;
            this._removeButton.Click += new EventHandler(this.OnRemoveButtonClick);
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(0x178, 0x15a);
            this._okButton.Name = "_okButton";
            this._okButton.Size = new Size(0x4b, 0x17);
            this._okButton.TabIndex = 150;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x1c9, 0x15a);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.Size = new Size(0x4b, 0x17);
            this._cancelButton.TabIndex = 160;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x220, 0x17d);
            base.Controls.Add(this._valueTextBox);
            base.Controls.Add(this._valueLabel);
            base.Controls.Add(this._whereClausesListView);
            base.Controls.Add(this._expressionTextBox);
            base.Controls.Add(this._propertiesGroupBox);
            base.Controls.Add(this._expressionLabel);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._removeButton);
            base.Controls.Add(this._addButton);
            base.Controls.Add(this._whereClausesLabel);
            base.Controls.Add(this._operatorsComboBox);
            base.Controls.Add(this._operatorLabel);
            base.Controls.Add(this._columnsComboBox);
            base.Controls.Add(this._columnLabel);
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._sourceLabel);
            base.Controls.Add(this._sourceComboBox);
            this.MinimumSize = new Size(0x228, 0x19f);
            base.Name = "SqlDataSourceConfigureFilterForm";
            base.SizeGripStyle = SizeGripStyle.Show;
            this._propertiesGroupBox.ResumeLayout(false);
            base.InitializeForm();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_HelpLabel");
            this._columnLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ColumnLabel");
            this._operatorLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_OperatorLabel");
            this._whereClausesLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_WhereLabel");
            this._expressionLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ExpressionLabel");
            this._valueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ValueLabel");
            this._expressionColumnHeader.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ExpressionColumnHeader");
            this._valueColumnHeader.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ValueColumnHeader");
            this._propertiesGroupBox.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterPropertiesGroup");
            this._sourceLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_SourceLabel");
            this._addButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_AddButton");
            this._removeButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_RemoveButton");
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_Caption");
        }

        private void OnAddButtonClick(object sender, EventArgs e)
        {
            SqlDataSourceFilterClause currentFilterClause = this.GetCurrentFilterClause();
            FilterClauseItem item = new FilterClauseItem(this._sqlDataSourceDesigner.Component.Site, this._tableQuery, currentFilterClause, (SqlDataSource) this._sqlDataSourceDesigner.Component);
            this._whereClausesListView.Items.Add(item);
            item.Selected = true;
            item.Focused = true;
            item.EnsureVisible();
            this._tableQuery.FilterClauses.Add(currentFilterClause);
            this._columnsComboBox.SelectedIndex = -1;
            this._okButton.Enabled = true;
            item.Refresh();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnColumnsComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateOperators();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void OnOperatorsComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateParameter();
        }

        private void OnParameterChanged(object sender, EventArgs e)
        {
            this.UpdateExpression();
            this.UpdateAddButtonEnabled();
        }

        private void OnRemoveButtonClick(object sender, EventArgs e)
        {
            if (this._whereClausesListView.SelectedItems.Count > 0)
            {
                int num = this._whereClausesListView.SelectedIndices[0];
                FilterClauseItem item = this._whereClausesListView.SelectedItems[0] as FilterClauseItem;
                this._whereClausesListView.Items.Remove(item);
                this._tableQuery.FilterClauses.Remove(item.FilterClause);
                this._okButton.Enabled = true;
                if (num < this._whereClausesListView.Items.Count)
                {
                    ListViewItem item2 = this._whereClausesListView.Items[num];
                    item2.Selected = true;
                    item2.Focused = true;
                    item2.EnsureVisible();
                    this._whereClausesListView.Focus();
                }
                else if (this._whereClausesListView.Items.Count > 0)
                {
                    ListViewItem item3 = this._whereClausesListView.Items[num - 1];
                    item3.Selected = true;
                    item3.Focused = true;
                    item3.EnsureVisible();
                    this._whereClausesListView.Focus();
                }
            }
        }

        private void OnSourceComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateParameter();
        }

        private void OnWhereClausesListViewSelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateDeleteButton();
        }

        private void UpdateAddButtonEnabled()
        {
            if (!(this._columnsComboBox.SelectedItem is ColumnItem))
            {
                this._addButton.Enabled = false;
            }
            else
            {
                OperatorItem selectedItem = this._operatorsComboBox.SelectedItem as OperatorItem;
                if (selectedItem == null)
                {
                    this._addButton.Enabled = false;
                }
                else
                {
                    ParameterEditor editor = this._sourceComboBox.SelectedItem as ParameterEditor;
                    this._addButton.Enabled = !selectedItem.IsBinary ^ ((editor != null) && editor.HasCompleteInformation);
                }
            }
        }

        private void UpdateDeleteButton()
        {
            this._removeButton.Enabled = this._whereClausesListView.SelectedItems.Count > 0;
        }

        private void UpdateExpression()
        {
            ParameterEditor selectedItem = this._sourceComboBox.SelectedItem as ParameterEditor;
            if ((this._operatorsComboBox.SelectedItem != null) && (selectedItem != null))
            {
                SqlDataSourceFilterClause currentFilterClause = this.GetCurrentFilterClause();
                if (currentFilterClause != null)
                {
                    this._expressionTextBox.Text = currentFilterClause.ToString();
                }
                else
                {
                    this._expressionTextBox.Text = string.Empty;
                }
                if (selectedItem.Parameter == null)
                {
                    this._valueTextBox.Text = string.Empty;
                }
                else
                {
                    bool flag;
                    string str = ParameterEditorUserControl.GetParameterExpression(this._sqlDataSourceDesigner.Component.Site, selectedItem.Parameter, (SqlDataSource) this._sqlDataSourceDesigner.Component, out flag);
                    if (flag)
                    {
                        this._valueTextBox.Text = string.Empty;
                    }
                    else
                    {
                        this._valueTextBox.Text = str;
                    }
                }
            }
            else
            {
                this._expressionTextBox.Text = string.Empty;
                this._valueTextBox.Text = string.Empty;
            }
        }

        private void UpdateOperators()
        {
            if (this._columnsComboBox.SelectedItem == null)
            {
                this._operatorsComboBox.SelectedItem = -1;
                this._operatorsComboBox.Items.Clear();
                this._operatorsComboBox.Enabled = false;
                this._operatorLabel.Enabled = false;
                this.UpdateParameter();
            }
            else
            {
                this._operatorsComboBox.Enabled = true;
                this._operatorLabel.Enabled = true;
                this._operatorsComboBox.Items.Clear();
                this._operatorsComboBox.Items.Add(new OperatorItem("{0} = {1}", "=", true));
                this._operatorsComboBox.Items.Add(new OperatorItem("{0} < {1}", "<", true));
                this._operatorsComboBox.Items.Add(new OperatorItem("{0} > {1}", ">", true));
                this._operatorsComboBox.Items.Add(new OperatorItem("{0} <= {1}", "<=", true));
                this._operatorsComboBox.Items.Add(new OperatorItem("{0} >= {1}", ">=", true));
                this._operatorsComboBox.Items.Add(new OperatorItem("{0} <> {1}", "<>", true));
                ColumnItem selectedItem = (ColumnItem) this._columnsComboBox.SelectedItem;
                DesignerDataColumn designerDataColumn = selectedItem.DesignerDataColumn;
                if (designerDataColumn.Nullable)
                {
                    this._operatorsComboBox.Items.Add(new OperatorItem("{0} IS NULL", "IS NULL", false));
                    this._operatorsComboBox.Items.Add(new OperatorItem("{0} IS NOT NULL", "IS NOT NULL", false));
                }
                switch (designerDataColumn.DataType)
                {
                    case DbType.String:
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.StringFixedLength:
                        this._operatorsComboBox.Items.Add(new OperatorItem("{0} LIKE '%' + {1} + '%'", "LIKE", true));
                        this._operatorsComboBox.Items.Add(new OperatorItem("{0} NOT LIKE '%' + {1} + '%'", "NOT LIKE", true));
                        this._operatorsComboBox.Items.Add(new OperatorItem("CONTAINS({0}, {1})", "CONTAINS", true));
                        break;
                }
                this._operatorsComboBox.InvalidateDropDownWidth();
                this._operatorsComboBox.SelectedIndex = 0;
                this.UpdateParameter();
            }
        }

        private void UpdateParameter()
        {
            OperatorItem selectedItem = this._operatorsComboBox.SelectedItem as OperatorItem;
            if ((selectedItem != null) && selectedItem.IsBinary)
            {
                this._expressionLabel.Enabled = true;
                this._expressionTextBox.Enabled = true;
                this._valueLabel.Enabled = true;
                this._valueTextBox.Enabled = true;
                this._propertiesGroupBox.Enabled = true;
                this._sourceLabel.Enabled = true;
                this._sourceComboBox.Enabled = true;
            }
            else
            {
                this._expressionLabel.Enabled = false;
                this._expressionTextBox.Enabled = false;
                this._valueLabel.Enabled = false;
                this._valueTextBox.Enabled = false;
                this._propertiesGroupBox.Enabled = false;
                this._sourceLabel.Enabled = false;
                this._sourceComboBox.Enabled = false;
                this._sourceComboBox.SelectedItem = null;
            }
            foreach (ParameterEditor editor in _parameterEditors.Values)
            {
                editor.Visible = false;
            }
            ParameterEditor editor2 = this._sourceComboBox.SelectedItem as ParameterEditor;
            if (editor2 != null)
            {
                editor2.Visible = true;
                editor2.Initialize();
                this._propertiesPanel.Visible = true;
            }
            else
            {
                this._propertiesPanel.Visible = false;
            }
            this.UpdateExpression();
            this.UpdateAddButtonEnabled();
        }

        public IList<SqlDataSourceFilterClause> FilterClauses
        {
            get
            {
                return this._tableQuery.FilterClauses;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.SqlDataSource.ConfigureFilter";
            }
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return (this._serviceProvider ?? base.ServiceProvider);
            }
        }

        public System.ComponentModel.TypeDescriptionProvider TypeDescriptionProvider
        {
            get
            {
                if (this._provider != null)
                {
                    return this._provider;
                }
                if (this._dataSource != null)
                {
                    return TypeDescriptor.GetProvider(this._dataSource);
                }
                return null;
            }
        }

        private sealed class ColumnItem
        {
            private System.ComponentModel.Design.Data.DesignerDataColumn _designerDataColumn;

            public ColumnItem(System.ComponentModel.Design.Data.DesignerDataColumn designerDataColumn)
            {
                this._designerDataColumn = designerDataColumn;
            }

            public override string ToString()
            {
                return this._designerDataColumn.Name;
            }

            public System.ComponentModel.Design.Data.DesignerDataColumn DesignerDataColumn
            {
                get
                {
                    return this._designerDataColumn;
                }
            }
        }

        internal sealed class ControlParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Web.UI.Control _control;
            private AutoSizeComboBox _controlIDComboBox;
            private System.Windows.Forms.Label _controlIDLabel;
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private ControlParameter _parameter;

            public ControlParameterEditor(IServiceProvider serviceProvider, System.Web.UI.Control control) : base(serviceProvider)
            {
                this._control = control;
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._controlIDLabel = new System.Windows.Forms.Label();
                this._controlIDComboBox = new AutoSizeComboBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._controlIDLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._controlIDLabel.Location = new Point(0, 0);
                this._controlIDLabel.Name = "ControlIDLabel";
                this._controlIDLabel.Size = new Size(220, 0x10);
                this._controlIDLabel.TabIndex = 10;
                this._controlIDLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ControlParameterEditor_ControlIDLabel");
                this._controlIDComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._controlIDComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                this._controlIDComboBox.Location = new Point(0, 0x17);
                this._controlIDComboBox.Name = "ControlIDComboBox";
                this._controlIDComboBox.Size = new Size(220, 20);
                this._controlIDComboBox.Sorted = true;
                this._controlIDComboBox.TabIndex = 20;
                this._controlIDComboBox.SelectedIndexChanged += new EventHandler(this.OnControlIDComboBoxSelectedIndexChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "ControlDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "ControlDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._controlIDLabel);
                base.Controls.Add(this._controlIDComboBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                if (base.ServiceProvider != null)
                {
                    IDesignerHost service = (IDesignerHost) base.ServiceProvider.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        foreach (ParameterEditorUserControl.ControlItem item in ParameterEditorUserControl.ControlItem.GetControlItems(service, this._control))
                        {
                            this._controlIDComboBox.Items.Add(item);
                        }
                        this._controlIDComboBox.InvalidateDropDownWidth();
                    }
                }
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new ControlParameter();
                this._controlIDComboBox.SelectedItem = null;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnControlIDComboBoxSelectedIndexChanged(object s, EventArgs e)
            {
                ParameterEditorUserControl.ControlItem selectedItem = this._controlIDComboBox.SelectedItem as ParameterEditorUserControl.ControlItem;
                if (selectedItem == null)
                {
                    this._parameter.ControlID = string.Empty;
                    this._parameter.PropertyName = string.Empty;
                }
                else
                {
                    this._parameter.ControlID = selectedItem.ControlID;
                    this._parameter.PropertyName = selectedItem.PropertyName;
                }
                base.OnParameterChanged();
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            public override string EditorName
            {
                get
                {
                    return "Control";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._controlIDComboBox.SelectedItem != null);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        internal sealed class CookieParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _cookieNameLabel;
            private System.Windows.Forms.TextBox _cookieNameTextBox;
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private CookieParameter _parameter;

            public CookieParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._cookieNameLabel = new System.Windows.Forms.Label();
                this._cookieNameTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._cookieNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._cookieNameLabel.Location = new Point(0, 0);
                this._cookieNameLabel.Name = "CookieNameLabel";
                this._cookieNameLabel.Size = new Size(220, 0x10);
                this._cookieNameLabel.TabIndex = 10;
                this._cookieNameLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_CookieParameterEditor_CookieNameLabel");
                this._cookieNameTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._cookieNameTextBox.Location = new Point(0, 0x17);
                this._cookieNameTextBox.Name = "CookieNameTextBox";
                this._cookieNameTextBox.Size = new Size(220, 20);
                this._cookieNameTextBox.TabIndex = 20;
                this._cookieNameTextBox.TextChanged += new EventHandler(this.OnCookieNameTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "CookieDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "CookieDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._cookieNameLabel);
                base.Controls.Add(this._cookieNameTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new CookieParameter();
                this._cookieNameTextBox.Text = string.Empty;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnCookieNameTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.CookieName = this._cookieNameTextBox.Text;
                base.OnParameterChanged();
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            public override string EditorName
            {
                get
                {
                    return "Cookie";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._parameter.CookieName.Length > 0);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        private sealed class FilterClauseItem : ListViewItem
        {
            private SqlDataSourceFilterClause _filterClause;
            private IServiceProvider _serviceProvider;
            private SqlDataSource _sqlDataSource;
            private SqlDataSourceTableQuery _tableQuery;

            public FilterClauseItem(IServiceProvider serviceProvider, SqlDataSourceTableQuery tableQuery, SqlDataSourceFilterClause filterClause, SqlDataSource sqlDataSource)
            {
                this._filterClause = filterClause;
                this._tableQuery = tableQuery;
                this._serviceProvider = serviceProvider;
                this._sqlDataSource = sqlDataSource;
            }

            public void Refresh()
            {
                string str;
                base.SubItems.Clear();
                base.Text = this._filterClause.ToString();
                ListView listView = base.ListView;
                IServiceProvider serviceProvider = null;
                if (listView != null)
                {
                    serviceProvider = ((SqlDataSourceConfigureFilterForm) listView.Parent).ServiceProvider;
                }
                if (this._filterClause.Parameter == null)
                {
                    str = string.Empty;
                }
                else
                {
                    bool flag;
                    str = ParameterEditorUserControl.GetParameterExpression(serviceProvider, this._filterClause.Parameter, this._sqlDataSource, out flag);
                    if (flag)
                    {
                        str = string.Empty;
                    }
                }
                ListViewItem.ListViewSubItem item = new ListViewItem.ListViewSubItem {
                    Text = str
                };
                base.SubItems.Add(item);
            }

            public SqlDataSourceFilterClause FilterClause
            {
                get
                {
                    return this._filterClause;
                }
            }
        }

        internal sealed class FormParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Windows.Forms.Label _formFieldLabel;
            private System.Windows.Forms.TextBox _formFieldTextBox;
            private FormParameter _parameter;

            public FormParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._formFieldLabel = new System.Windows.Forms.Label();
                this._formFieldTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._formFieldLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._formFieldLabel.Location = new Point(0, 0);
                this._formFieldLabel.Name = "FormFieldLabel";
                this._formFieldLabel.Size = new Size(220, 0x10);
                this._formFieldLabel.TabIndex = 10;
                this._formFieldLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_FormParameterEditor_FormFieldLabel");
                this._formFieldTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._formFieldTextBox.Location = new Point(0, 0x17);
                this._formFieldTextBox.Name = "FormFieldTextBox";
                this._formFieldTextBox.Size = new Size(220, 20);
                this._formFieldTextBox.TabIndex = 20;
                this._formFieldTextBox.TextChanged += new EventHandler(this.OnFormFieldTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "FormDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "FormDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._formFieldLabel);
                base.Controls.Add(this._formFieldTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new FormParameter();
                this._formFieldTextBox.Text = string.Empty;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            private void OnFormFieldTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.FormField = this._formFieldTextBox.Text;
                base.OnParameterChanged();
            }

            public override string EditorName
            {
                get
                {
                    return "Form";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._parameter.FormField.Length > 0);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        private sealed class OperatorItem
        {
            private bool _isBinary;
            private string _operatorFormat;
            private string _operatorName;

            public OperatorItem(string operatorFormat, string operatorName, bool isBinary)
            {
                this._operatorName = operatorName;
                this._operatorFormat = operatorFormat;
                this._isBinary = isBinary;
            }

            public override string ToString()
            {
                return this._operatorName;
            }

            public bool IsBinary
            {
                get
                {
                    return this._isBinary;
                }
            }

            public string OperatorFormat
            {
                get
                {
                    return this._operatorFormat;
                }
            }

            public string OperatorName
            {
                get
                {
                    return this._operatorName;
                }
            }
        }

        internal abstract class ParameterEditor : System.Windows.Forms.Panel
        {
            private IServiceProvider _serviceProvider;
            protected const int ControlWidth = 220;
            private static readonly object EventParameterChanged = new object();

            public event EventHandler ParameterChanged
            {
                add
                {
                    base.Events.AddHandler(EventParameterChanged, value);
                }
                remove
                {
                    base.Events.RemoveHandler(EventParameterChanged, value);
                }
            }

            protected ParameterEditor(IServiceProvider serviceProvider)
            {
                this._serviceProvider = serviceProvider;
            }

            public abstract void Initialize();
            protected void OnParameterChanged()
            {
                EventHandler handler = base.Events[EventParameterChanged] as EventHandler;
                if (handler != null)
                {
                    handler(this, EventArgs.Empty);
                }
            }

            public override string ToString()
            {
                return this.EditorName;
            }

            public abstract string EditorName { get; }

            public abstract bool HasCompleteInformation { get; }

            public abstract System.Web.UI.WebControls.Parameter Parameter { get; }

            protected IServiceProvider ServiceProvider
            {
                get
                {
                    return this._serviceProvider;
                }
            }
        }

        internal sealed class ProfileParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private ProfileParameter _parameter;
            private System.Windows.Forms.Label _propertyNameLabel;
            private System.Windows.Forms.TextBox _propertyNameTextBox;

            public ProfileParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._propertyNameLabel = new System.Windows.Forms.Label();
                this._propertyNameTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._propertyNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._propertyNameLabel.Location = new Point(0, 0);
                this._propertyNameLabel.Name = "ProfilePropertyNameLabel";
                this._propertyNameLabel.Size = new Size(220, 0x10);
                this._propertyNameLabel.TabIndex = 10;
                this._propertyNameLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ProfileParameterEditor_PropertyNameLabel");
                this._propertyNameTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._propertyNameTextBox.Location = new Point(0, 0x17);
                this._propertyNameTextBox.Name = "ProfilePropertyNameTextBox";
                this._propertyNameTextBox.Size = new Size(220, 20);
                this._propertyNameTextBox.TabIndex = 20;
                this._propertyNameTextBox.TextChanged += new EventHandler(this.OnPropertyNameTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "ProfileDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "ProfileDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._propertyNameLabel);
                base.Controls.Add(this._propertyNameTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new ProfileParameter();
                this._propertyNameTextBox.Text = string.Empty;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            private void OnPropertyNameTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.PropertyName = this._propertyNameTextBox.Text;
                base.OnParameterChanged();
            }

            public override string EditorName
            {
                get
                {
                    return "Profile";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._parameter.PropertyName.Length > 0);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        internal sealed class QueryStringParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private QueryStringParameter _parameter;
            private System.Windows.Forms.Label _queryStringFieldLabel;
            private System.Windows.Forms.TextBox _queryStringFieldTextBox;

            public QueryStringParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._queryStringFieldLabel = new System.Windows.Forms.Label();
                this._queryStringFieldTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._queryStringFieldLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._queryStringFieldLabel.Location = new Point(0, 0);
                this._queryStringFieldLabel.Name = "QueryStringFieldLabel";
                this._queryStringFieldLabel.Size = new Size(220, 0x10);
                this._queryStringFieldLabel.TabIndex = 10;
                this._queryStringFieldLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_QueryStringParameterEditor_QueryStringFieldLabel");
                this._queryStringFieldTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._queryStringFieldTextBox.Location = new Point(0, 0x17);
                this._queryStringFieldTextBox.Name = "QueryStringFieldTextBox";
                this._queryStringFieldTextBox.Size = new Size(220, 20);
                this._queryStringFieldTextBox.TabIndex = 20;
                this._queryStringFieldTextBox.TextChanged += new EventHandler(this.OnQueryStringFieldTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "QueryStringDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "QueryStringDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._queryStringFieldLabel);
                base.Controls.Add(this._queryStringFieldTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new QueryStringParameter();
                this._queryStringFieldTextBox.Text = string.Empty;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            private void OnQueryStringFieldTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.QueryStringField = this._queryStringFieldTextBox.Text;
                base.OnParameterChanged();
            }

            public override string EditorName
            {
                get
                {
                    return "QueryString";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._parameter.QueryStringField.Length > 0);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        internal sealed class RouteParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private RouteParameter _parameter;
            private System.Windows.Forms.Label _routeKeyLabel;
            private System.Windows.Forms.TextBox _routeKeyTextBox;

            public RouteParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._routeKeyLabel = new System.Windows.Forms.Label();
                this._routeKeyTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._routeKeyLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._routeKeyLabel.Location = new Point(0, 0);
                this._routeKeyLabel.Name = "RouteKeyLabel";
                this._routeKeyLabel.Size = new Size(220, 0x10);
                this._routeKeyLabel.TabIndex = 10;
                this._routeKeyLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_RouteParameterEditor_RouteKeyLabel");
                this._routeKeyTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._routeKeyTextBox.Location = new Point(0, 0x17);
                this._routeKeyTextBox.Name = "RouteKeyTextBox";
                this._routeKeyTextBox.Size = new Size(220, 20);
                this._routeKeyTextBox.TabIndex = 20;
                this._routeKeyTextBox.TextChanged += new EventHandler(this.OnRouteKeyTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "RouteDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "RouteDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._routeKeyLabel);
                base.Controls.Add(this._routeKeyTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new RouteParameter();
                this._routeKeyTextBox.Text = string.Empty;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            private void OnRouteKeyTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.RouteKey = this._routeKeyTextBox.Text;
                base.OnParameterChanged();
            }

            public override string EditorName
            {
                get
                {
                    return "Route";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._parameter.RouteKey.Length > 0);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        internal sealed class SessionParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private SessionParameter _parameter;
            private System.Windows.Forms.Label _sessionFieldLabel;
            private System.Windows.Forms.TextBox _sessionFieldTextBox;

            public SessionParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._sessionFieldLabel = new System.Windows.Forms.Label();
                this._sessionFieldTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._sessionFieldLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._sessionFieldLabel.Location = new Point(0, 0);
                this._sessionFieldLabel.Name = "SessionFieldLabel";
                this._sessionFieldLabel.Size = new Size(220, 0x10);
                this._sessionFieldLabel.TabIndex = 10;
                this._sessionFieldLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_SessionParameterEditor_SessionFieldLabel");
                this._sessionFieldTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._sessionFieldTextBox.Location = new Point(0, 0x17);
                this._sessionFieldTextBox.Name = "SessionFieldTextBox";
                this._sessionFieldTextBox.Size = new Size(220, 20);
                this._sessionFieldTextBox.TabIndex = 20;
                this._sessionFieldTextBox.TextChanged += new EventHandler(this.OnSessionFieldTextBoxTextChanged);
                this._defaultValueLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0x30);
                this._defaultValueLabel.Name = "SessionDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 30;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_ParameterEditor_DefaultValue");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x44);
                this._defaultValueTextBox.Name = "SessionDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 40;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._sessionFieldLabel);
                base.Controls.Add(this._sessionFieldTextBox);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new SessionParameter();
                this._sessionFieldTextBox.Text = string.Empty;
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
            }

            private void OnSessionFieldTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.SessionField = this._sessionFieldTextBox.Text;
                base.OnParameterChanged();
            }

            public override string EditorName
            {
                get
                {
                    return "Session";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return (this._parameter.SessionField.Length > 0);
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }

        internal sealed class StaticParameterEditor : SqlDataSourceConfigureFilterForm.ParameterEditor
        {
            private System.Windows.Forms.Label _defaultValueLabel;
            private System.Windows.Forms.TextBox _defaultValueTextBox;
            private System.Web.UI.WebControls.Parameter _parameter;

            public StaticParameterEditor(IServiceProvider serviceProvider) : base(serviceProvider)
            {
                base.SuspendLayout();
                base.Size = new Size(220, 0x2c);
                this._defaultValueLabel = new System.Windows.Forms.Label();
                this._defaultValueTextBox = new System.Windows.Forms.TextBox();
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueLabel.Location = new Point(0, 0);
                this._defaultValueLabel.Name = "StaticDefaultValueLabel";
                this._defaultValueLabel.Size = new Size(220, 0x10);
                this._defaultValueLabel.TabIndex = 10;
                this._defaultValueLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureFilterForm_StaticParameterEditor_ValueLabel");
                this._defaultValueTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._defaultValueTextBox.Location = new Point(0, 0x17);
                this._defaultValueTextBox.Name = "StaticDefaultValueTextBox";
                this._defaultValueTextBox.Size = new Size(220, 20);
                this._defaultValueTextBox.TabIndex = 20;
                this._defaultValueTextBox.TextChanged += new EventHandler(this.OnDefaultValueTextBoxTextChanged);
                base.Controls.Add(this._defaultValueLabel);
                base.Controls.Add(this._defaultValueTextBox);
                this.Dock = DockStyle.Fill;
                base.ResumeLayout();
            }

            public override void Initialize()
            {
                this._parameter = new System.Web.UI.WebControls.Parameter();
                this._defaultValueTextBox.Text = string.Empty;
            }

            private void OnDefaultValueTextBoxTextChanged(object s, EventArgs e)
            {
                this._parameter.DefaultValue = this._defaultValueTextBox.Text;
                base.OnParameterChanged();
            }

            public override string EditorName
            {
                get
                {
                    return "None";
                }
            }

            public override bool HasCompleteInformation
            {
                get
                {
                    return true;
                }
            }

            public override System.Web.UI.WebControls.Parameter Parameter
            {
                get
                {
                    return this._parameter;
                }
            }
        }
    }
}

