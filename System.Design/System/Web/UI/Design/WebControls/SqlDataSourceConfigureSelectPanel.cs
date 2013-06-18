namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceConfigureSelectPanel : WizardPanel
    {
        private System.Windows.Forms.Button _addFilterButton;
        private System.Windows.Forms.Button _addSortButton;
        private System.Windows.Forms.Button _advancedOptionsButton;
        private TableLayoutPanel _columnsTableLayoutPanel;
        private System.Windows.Forms.RadioButton _customSqlRadioButton;
        private DesignerDataConnection _dataConnection;
        private System.Windows.Forms.Panel _fieldChooserPanel;
        private CheckedListBox _fieldsCheckedListBox;
        private System.Windows.Forms.Label _fieldsLabel;
        private int _generateMode;
        private bool _ignoreFieldCheckChanges;
        private TableLayoutPanel _optionsTableLayoutPanel;
        private System.Windows.Forms.Label _previewLabel;
        private System.Windows.Forms.TextBox _previewTextBox;
        private TableItem _previousTable;
        private bool _requiresRefresh = true;
        private System.Windows.Forms.Label _retrieveDataLabel;
        private System.Windows.Forms.CheckBox _selectDistinctCheckBox;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private System.Windows.Forms.Label _tableNameLabel;
        private SqlDataSourceTableQuery _tableQuery;
        private System.Windows.Forms.RadioButton _tableRadioButton;
        private AutoSizeComboBox _tablesComboBox;
        private const string CompareAllValuesFormatString = "original_{0}";
        private const string OverwriteChangesFormatString = "{0}";

        public SqlDataSourceConfigureSelectPanel(SqlDataSourceDesigner sqlDataSourceDesigner)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
        }

        private DesignerDataColumn GetColumnFromTable(DesignerDataTableBase designerDataTable, string columnName)
        {
            foreach (DesignerDataColumn column in designerDataTable.Columns)
            {
                if (column.Name == columnName)
                {
                    return column;
                }
            }
            return null;
        }

        private static string GetOldValuesFormatString(SqlDataSource sqlDataSource, bool adjustForOptimisticConcurrency)
        {
            try
            {
                string a = string.Format(CultureInfo.InvariantCulture, sqlDataSource.OldValuesParameterFormatString, new object[] { "test" });
                if (string.Equals(a, sqlDataSource.OldValuesParameterFormatString, StringComparison.Ordinal))
                {
                    return (adjustForOptimisticConcurrency ? "original_{0}" : "{0}");
                }
                if (adjustForOptimisticConcurrency && string.Equals("test", a))
                {
                    return "original_{0}";
                }
                return sqlDataSource.OldValuesParameterFormatString;
            }
            catch
            {
                return (adjustForOptimisticConcurrency ? "original_{0}" : "{0}");
            }
        }

        private void InitializeComponent()
        {
            this._retrieveDataLabel = new System.Windows.Forms.Label();
            this._tableRadioButton = new System.Windows.Forms.RadioButton();
            this._customSqlRadioButton = new System.Windows.Forms.RadioButton();
            this._advancedOptionsButton = new System.Windows.Forms.Button();
            this._previewTextBox = new System.Windows.Forms.TextBox();
            this._previewLabel = new System.Windows.Forms.Label();
            this._tableNameLabel = new System.Windows.Forms.Label();
            this._addSortButton = new System.Windows.Forms.Button();
            this._addFilterButton = new System.Windows.Forms.Button();
            this._selectDistinctCheckBox = new System.Windows.Forms.CheckBox();
            this._fieldsCheckedListBox = new CheckedListBox();
            this._fieldsLabel = new System.Windows.Forms.Label();
            this._tablesComboBox = new AutoSizeComboBox();
            this._columnsTableLayoutPanel = new TableLayoutPanel();
            this._optionsTableLayoutPanel = new TableLayoutPanel();
            this._fieldChooserPanel = new System.Windows.Forms.Panel();
            this._columnsTableLayoutPanel.SuspendLayout();
            this._optionsTableLayoutPanel.SuspendLayout();
            this._fieldChooserPanel.SuspendLayout();
            base.SuspendLayout();
            this._retrieveDataLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._retrieveDataLabel.Location = new Point(0, 0);
            this._retrieveDataLabel.Name = "_retrieveDataLabel";
            this._retrieveDataLabel.Size = new Size(0x220, 0x10);
            this._retrieveDataLabel.TabIndex = 10;
            this._customSqlRadioButton.Location = new Point(7, 0x13);
            this._customSqlRadioButton.Name = "_customSqlRadioButton";
            this._customSqlRadioButton.Size = new Size(0x219, 0x12);
            this._customSqlRadioButton.TabIndex = 20;
            this._customSqlRadioButton.CheckedChanged += new EventHandler(this.OnCustomSqlRadioButtonCheckedChanged);
            this._tableRadioButton.Location = new Point(7, 0x26);
            this._tableRadioButton.Name = "_tableRadioButton";
            this._tableRadioButton.Size = new Size(0x219, 0x12);
            this._tableRadioButton.TabIndex = 30;
            this._tableRadioButton.CheckedChanged += new EventHandler(this.OnTableRadioButtonCheckedChanged);
            this._fieldChooserPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._fieldChooserPanel.Controls.Add(this._tableNameLabel);
            this._fieldChooserPanel.Controls.Add(this._tablesComboBox);
            this._fieldChooserPanel.Controls.Add(this._fieldsLabel);
            this._fieldChooserPanel.Controls.Add(this._columnsTableLayoutPanel);
            this._fieldChooserPanel.Controls.Add(this._previewLabel);
            this._fieldChooserPanel.Controls.Add(this._previewTextBox);
            this._fieldChooserPanel.Location = new Point(0x19, 0x3a);
            this._fieldChooserPanel.Name = "_fieldChooserPanel";
            this._fieldChooserPanel.Size = new Size(0x207, 0xd8);
            this._fieldChooserPanel.TabIndex = 40;
            this._tableNameLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._tableNameLabel.Location = new Point(0, 0);
            this._tableNameLabel.Name = "_tableNameLabel";
            this._tableNameLabel.Size = new Size(0x207, 0x10);
            this._tableNameLabel.TabIndex = 10;
            this._tablesComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._tablesComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._tablesComboBox.Location = new Point(0, 0x10);
            this._tablesComboBox.Name = "_tablesComboBox";
            this._tablesComboBox.Size = new Size(0x107, 0x15);
            this._tablesComboBox.TabIndex = 20;
            this._tablesComboBox.SelectedIndexChanged += new EventHandler(this.OnTablesComboBoxSelectedIndexChanged);
            this._fieldsLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._fieldsLabel.Location = new Point(0, 0x2a);
            this._fieldsLabel.Name = "_fieldsLabel";
            this._fieldsLabel.Size = new Size(0x207, 0x10);
            this._fieldsLabel.TabIndex = 30;
            this._columnsTableLayoutPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._columnsTableLayoutPanel.ColumnCount = 2;
            this._columnsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this._columnsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this._columnsTableLayoutPanel.Controls.Add(this._optionsTableLayoutPanel, 0, 1);
            this._columnsTableLayoutPanel.Controls.Add(this._fieldsCheckedListBox, 0, 0);
            this._columnsTableLayoutPanel.Controls.Add(this._selectDistinctCheckBox, 1, 0);
            this._columnsTableLayoutPanel.Location = new Point(0, 0x3a);
            this._columnsTableLayoutPanel.Name = "_columnsTableLayoutPanel";
            this._columnsTableLayoutPanel.RowCount = 2;
            this._columnsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._columnsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._columnsTableLayoutPanel.Size = new Size(0x207, 100);
            this._columnsTableLayoutPanel.TabIndex = 40;
            this._previewLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._previewLabel.Location = new Point(0, 0xa4);
            this._previewLabel.Name = "_previewLabel";
            this._previewLabel.Size = new Size(0x207, 0x10);
            this._previewLabel.TabIndex = 50;
            this._previewTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._previewTextBox.BackColor = SystemColors.Control;
            this._previewTextBox.Location = new Point(0, 180);
            this._previewTextBox.Multiline = true;
            this._previewTextBox.Name = "_previewTextBox";
            this._previewTextBox.ReadOnly = true;
            this._previewTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._previewTextBox.Size = new Size(0x207, 0x24);
            this._previewTextBox.TabIndex = 60;
            this._previewTextBox.Text = "";
            this._fieldsCheckedListBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._fieldsCheckedListBox.CheckOnClick = true;
            this._fieldsCheckedListBox.IntegralHeight = false;
            this._fieldsCheckedListBox.Location = new Point(0, 0);
            this._fieldsCheckedListBox.MultiColumn = true;
            this._fieldsCheckedListBox.Name = "_fieldsCheckedListBox";
            this._fieldsCheckedListBox.Margin = new Padding(0, 0, 3, 0);
            this._columnsTableLayoutPanel.SetRowSpan(this._fieldsCheckedListBox, 2);
            this._fieldsCheckedListBox.Size = new Size(0x184, 100);
            this._fieldsCheckedListBox.TabIndex = 10;
            this._fieldsCheckedListBox.ItemCheck += new ItemCheckEventHandler(this.OnFieldsCheckedListBoxItemCheck);
            this._selectDistinctCheckBox.AutoSize = true;
            this._selectDistinctCheckBox.Location = new Point(0x18a, 2);
            this._selectDistinctCheckBox.Margin = new Padding(3, 0, 0, 0);
            this._selectDistinctCheckBox.Name = "_selectDistinctCheckBox";
            this._selectDistinctCheckBox.Size = new Size(15, 14);
            this._selectDistinctCheckBox.TabIndex = 20;
            this._selectDistinctCheckBox.CheckedChanged += new EventHandler(this.OnSelectDistinctCheckBoxCheckedChanged);
            this._optionsTableLayoutPanel.AutoSize = true;
            this._optionsTableLayoutPanel.ColumnCount = 1;
            this._optionsTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this._optionsTableLayoutPanel.Controls.Add(this._addFilterButton, 0, 0);
            this._optionsTableLayoutPanel.Controls.Add(this._addSortButton, 0, 1);
            this._optionsTableLayoutPanel.Controls.Add(this._advancedOptionsButton, 0, 2);
            this._optionsTableLayoutPanel.Location = new Point(0x18a, 0x13);
            this._optionsTableLayoutPanel.Margin = new Padding(3, 0, 0, 0);
            this._optionsTableLayoutPanel.Name = "_optionsTableLayoutPanel";
            this._optionsTableLayoutPanel.RowCount = 3;
            this._optionsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._optionsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._optionsTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._optionsTableLayoutPanel.Size = new Size(0x73, 0x51);
            this._optionsTableLayoutPanel.TabIndex = 30;
            this._addFilterButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._addFilterButton.AutoSize = true;
            this._addFilterButton.Location = new Point(0, 2);
            this._addFilterButton.Margin = new Padding(0, 2, 0, 2);
            this._addFilterButton.MinimumSize = new Size(0x73, 0x17);
            this._addFilterButton.Name = "_addFilterButton";
            this._addFilterButton.Size = new Size(0x73, 0x17);
            this._addFilterButton.TabIndex = 10;
            this._addFilterButton.Click += new EventHandler(this.OnAddFilterButtonClick);
            this._addSortButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._addSortButton.AutoSize = true;
            this._addSortButton.Location = new Point(0, 0x1d);
            this._addSortButton.Margin = new Padding(0, 2, 0, 2);
            this._addSortButton.MinimumSize = new Size(0x73, 0x17);
            this._addSortButton.Name = "_addSortButton";
            this._addSortButton.Size = new Size(0x73, 0x17);
            this._addSortButton.TabIndex = 20;
            this._addSortButton.Click += new EventHandler(this.OnAddSortButtonClick);
            this._advancedOptionsButton.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            this._advancedOptionsButton.AutoSize = true;
            this._advancedOptionsButton.Location = new Point(0, 0x38);
            this._advancedOptionsButton.Margin = new Padding(0, 2, 0, 2);
            this._advancedOptionsButton.MinimumSize = new Size(0x73, 0x17);
            this._advancedOptionsButton.Name = "_advancedOptionsButton";
            this._advancedOptionsButton.Size = new Size(0x73, 0x17);
            this._advancedOptionsButton.TabIndex = 30;
            this._advancedOptionsButton.Click += new EventHandler(this.OnAdvancedOptionsButtonClick);
            base.Controls.Add(this._fieldChooserPanel);
            base.Controls.Add(this._customSqlRadioButton);
            base.Controls.Add(this._tableRadioButton);
            base.Controls.Add(this._retrieveDataLabel);
            base.Name = "SqlDataSourceConfigureSelectPanel";
            base.Size = new Size(0x220, 0x112);
            this._columnsTableLayoutPanel.ResumeLayout(false);
            this._columnsTableLayoutPanel.PerformLayout();
            this._optionsTableLayoutPanel.ResumeLayout(false);
            this._optionsTableLayoutPanel.PerformLayout();
            this._fieldChooserPanel.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            base.Caption = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_PanelCaption");
            this._retrieveDataLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_RetrieveDataLabel");
            this._tableRadioButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_TableLabel");
            this._customSqlRadioButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_CustomSqlLabel");
            this._previewLabel.Text = System.Design.SR.GetString("SqlDataSource_General_PreviewLabel");
            this._tableNameLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_TableNameLabel");
            this._addSortButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_SortButton");
            this._addFilterButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_FilterLabel");
            this._selectDistinctCheckBox.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_SelectDistinctLabel");
            this._advancedOptionsButton.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_AdvancedOptions");
            this._fieldsLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_FieldsLabel");
            this._tableRadioButton.AccessibleDescription = this._retrieveDataLabel.Text;
            this._tableRadioButton.AccessibleName = this._tableRadioButton.Text;
            this._customSqlRadioButton.AccessibleDescription = this._retrieveDataLabel.Text;
            this._customSqlRadioButton.AccessibleName = this._customSqlRadioButton.Text;
            this.UpdateFonts();
        }

        private bool LoadParsedSqlState()
        {
            SqlDataSource component = (SqlDataSource) this._sqlDataSourceDesigner.Component;
            string[] strArray = SqlDataSourceCommandParser.ParseSqlString(this._sqlDataSourceDesigner.SelectCommand);
            if (strArray == null)
            {
                return false;
            }
            bool flag = false;
            string lastIdentifierPart = SqlDataSourceCommandParser.GetLastIdentifierPart(strArray[strArray.Length - 1]);
            if (lastIdentifierPart == null)
            {
                return false;
            }
            List<string> list = new List<string>();
            for (int i = 0; i < (strArray.Length - 1); i++)
            {
                string str2 = SqlDataSourceCommandParser.GetLastIdentifierPart(strArray[i]);
                if (str2 == null)
                {
                    return false;
                }
                if (str2 == "*")
                {
                    flag = true;
                }
                else
                {
                    if (str2.Length == 0)
                    {
                        return false;
                    }
                    list.Add(str2);
                }
            }
            if (flag && (list.Count != 0))
            {
                return false;
            }
            TableItem item = null;
            foreach (TableItem item2 in this._tablesComboBox.Items)
            {
                if (item2.DesignerDataTable.Name == lastIdentifierPart)
                {
                    item = item2;
                    break;
                }
            }
            if (item == null)
            {
                return false;
            }
            DesignerDataTableBase designerDataTable = item.DesignerDataTable;
            List<DesignerDataColumn> list2 = new List<DesignerDataColumn>();
            foreach (string str3 in list)
            {
                DesignerDataColumn columnFromTable = this.GetColumnFromTable(designerDataTable, str3);
                if (columnFromTable == null)
                {
                    return false;
                }
                list2.Add(columnFromTable);
            }
            bool flag2 = ((component.DeleteCommand.Trim().Length > 0) || (component.InsertCommand.Trim().Length > 0)) || (component.UpdateCommand.Trim().Length > 0);
            if (flag2)
            {
                SqlDataSourceTableQuery query = new SqlDataSourceTableQuery(this._dataConnection, designerDataTable);
                foreach (DesignerDataColumn column2 in list2)
                {
                    query.Fields.Add(column2);
                }
                query.AsteriskField = flag;
                SqlDataSourceQuery insertQuery = query.GetInsertQuery();
                string oldValuesFormatString = GetOldValuesFormatString(component, false);
                SqlDataSourceQuery updateQuery = query.GetUpdateQuery(oldValuesFormatString, false);
                SqlDataSourceQuery deleteQuery = query.GetDeleteQuery(oldValuesFormatString, false);
                if (((insertQuery != null) && (component.InsertCommand.Trim().Length > 0)) && (component.InsertCommand != insertQuery.Command))
                {
                    return false;
                }
                if (((updateQuery != null) && (component.UpdateCommand.Trim().Length > 0)) && (component.UpdateCommand != updateQuery.Command))
                {
                    return false;
                }
                if (((deleteQuery != null) && (component.DeleteCommand.Trim().Length > 0)) && (component.DeleteCommand != deleteQuery.Command))
                {
                    return false;
                }
            }
            this._tableQuery = new SqlDataSourceTableQuery(this._dataConnection, designerDataTable);
            this._tablesComboBox.SelectedItem = item;
            ArrayList list3 = new ArrayList();
            foreach (DesignerDataColumn column3 in list2)
            {
                foreach (ColumnItem item3 in this._fieldsCheckedListBox.Items)
                {
                    if (item3.DesignerDataColumn == column3)
                    {
                        list3.Add(this._fieldsCheckedListBox.Items.IndexOf(item3));
                    }
                }
            }
            foreach (int num2 in list3)
            {
                this._fieldsCheckedListBox.SetItemChecked(num2, true);
            }
            if (flag)
            {
                this._fieldsCheckedListBox.SetItemChecked(0, true);
            }
            this._generateMode = flag2 ? 1 : 0;
            return true;
        }

        private bool LoadTableQueryState(Hashtable tableQueryState)
        {
            SqlDataSource component = this._sqlDataSourceDesigner.Component as SqlDataSource;
            int num = (int) tableQueryState["Conn_ConnectionStringHash"];
            string str = (string) tableQueryState["Conn_ProviderName"];
            if ((num != this._dataConnection.ConnectionString.GetHashCode()) || (str != this._dataConnection.ProviderName))
            {
                return false;
            }
            int num2 = (int) tableQueryState["Generate_Mode"];
            string str2 = (string) tableQueryState["Table_Name"];
            TableItem item = null;
            foreach (TableItem item2 in this._tablesComboBox.Items)
            {
                if (item2.DesignerDataTable.Name == str2)
                {
                    item = item2;
                    break;
                }
            }
            if (item == null)
            {
                return false;
            }
            DesignerDataTableBase designerDataTable = item.DesignerDataTable;
            int num3 = (int) tableQueryState["Fields_Count"];
            ArrayList list = new ArrayList();
            for (int i = 0; i < num3; i++)
            {
                string columnName = (string) tableQueryState["Fields_FieldName" + i.ToString(CultureInfo.InvariantCulture)];
                DesignerDataColumn columnFromTable = this.GetColumnFromTable(designerDataTable, columnName);
                if (columnFromTable == null)
                {
                    return false;
                }
                list.Add(columnFromTable);
            }
            bool flag = (bool) tableQueryState["AsteriskField"];
            bool flag2 = (bool) tableQueryState["Distinct"];
            List<Parameter> list2 = new List<Parameter>();
            foreach (ICloneable cloneable in component.SelectParameters)
            {
                list2.Add((Parameter) cloneable.Clone());
            }
            bool flag3 = SqlDataSourceDesigner.SupportsNamedParameters(SqlDataSourceDesigner.GetDbProviderFactory(this._dataConnection.ProviderName));
            int num5 = (int) tableQueryState["Filters_Count"];
            ArrayList list3 = new ArrayList();
            for (int j = 0; j < num5; j++)
            {
                string str4 = (string) tableQueryState["Filters_FieldName" + j.ToString(CultureInfo.InvariantCulture)];
                string operatorFormat = (string) tableQueryState["Filters_OperatorFormat" + j.ToString(CultureInfo.InvariantCulture)];
                bool isBinary = (bool) tableQueryState["Filters_IsBinary" + j.ToString(CultureInfo.InvariantCulture)];
                string str6 = (string) tableQueryState["Filters_Value" + j.ToString(CultureInfo.InvariantCulture)];
                string name = (string) tableQueryState["Filters_ParameterName" + j.ToString(CultureInfo.InvariantCulture)];
                DesignerDataColumn designerDataColumn = this.GetColumnFromTable(designerDataTable, str4);
                if (designerDataColumn == null)
                {
                    return false;
                }
                Parameter parameter = null;
                if (name != null)
                {
                    if (flag3)
                    {
                        foreach (Parameter parameter2 in list2)
                        {
                            if (parameter2.Name == name)
                            {
                                parameter = parameter2;
                                break;
                            }
                        }
                        if (parameter != null)
                        {
                            list2.Remove(parameter);
                        }
                        else
                        {
                            parameter = new Parameter(name);
                        }
                    }
                    else if (list2.Count > 0)
                    {
                        parameter = list2[0];
                        list2.RemoveAt(0);
                    }
                    else
                    {
                        parameter = new Parameter(name);
                    }
                }
                list3.Add(new SqlDataSourceFilterClause(this._dataConnection, designerDataTable, designerDataColumn, operatorFormat, isBinary, str6, parameter));
            }
            int num7 = (int) tableQueryState["Orders_Count"];
            ArrayList list4 = new ArrayList();
            for (int k = 0; k < num7; k++)
            {
                string str8 = (string) tableQueryState["Orders_FieldName" + k.ToString(CultureInfo.InvariantCulture)];
                bool isDescending = (bool) tableQueryState["Orders_IsDescending" + k.ToString(CultureInfo.InvariantCulture)];
                DesignerDataColumn column3 = this.GetColumnFromTable(designerDataTable, str8);
                if (column3 == null)
                {
                    return false;
                }
                list4.Add(new SqlDataSourceOrderClause(this._dataConnection, designerDataTable, column3, isDescending));
            }
            SqlDataSourceTableQuery query = new SqlDataSourceTableQuery(this._dataConnection, designerDataTable);
            foreach (DesignerDataColumn column4 in list)
            {
                query.Fields.Add(column4);
            }
            query.AsteriskField = flag;
            query.Distinct = flag2;
            foreach (SqlDataSourceFilterClause clause in list3)
            {
                query.FilterClauses.Add(clause);
            }
            foreach (SqlDataSourceOrderClause clause2 in list4)
            {
                query.OrderClauses.Add(clause2);
            }
            bool includeOldValues = num2 == 2;
            string oldValuesFormatString = GetOldValuesFormatString(component, false);
            SqlDataSourceQuery selectQuery = query.GetSelectQuery();
            SqlDataSourceQuery insertQuery = query.GetInsertQuery();
            SqlDataSourceQuery updateQuery = query.GetUpdateQuery(oldValuesFormatString, includeOldValues);
            SqlDataSourceQuery deleteQuery = query.GetDeleteQuery(oldValuesFormatString, includeOldValues);
            if ((selectQuery != null) && (component.SelectCommand != selectQuery.Command))
            {
                return false;
            }
            if (((insertQuery != null) && (component.InsertCommand.Trim().Length > 0)) && (component.InsertCommand != insertQuery.Command))
            {
                return false;
            }
            if (((updateQuery != null) && (component.UpdateCommand.Trim().Length > 0)) && (component.UpdateCommand != updateQuery.Command))
            {
                return false;
            }
            if (((deleteQuery != null) && (component.DeleteCommand.Trim().Length > 0)) && (component.DeleteCommand != deleteQuery.Command))
            {
                return false;
            }
            this._tableQuery = new SqlDataSourceTableQuery(this._dataConnection, designerDataTable);
            this._tablesComboBox.SelectedItem = item;
            ArrayList list5 = new ArrayList();
            foreach (DesignerDataColumn column5 in list)
            {
                foreach (ColumnItem item3 in this._fieldsCheckedListBox.Items)
                {
                    if (item3.DesignerDataColumn == column5)
                    {
                        list5.Add(this._fieldsCheckedListBox.Items.IndexOf(item3));
                    }
                }
            }
            foreach (int num9 in list5)
            {
                this._fieldsCheckedListBox.SetItemChecked(num9, true);
            }
            if (flag)
            {
                this._fieldsCheckedListBox.SetItemChecked(0, true);
            }
            this._selectDistinctCheckBox.Checked = flag2;
            this._generateMode = num2;
            foreach (SqlDataSourceFilterClause clause3 in list3)
            {
                this._tableQuery.FilterClauses.Add(clause3);
            }
            foreach (SqlDataSourceOrderClause clause4 in list4)
            {
                this._tableQuery.OrderClauses.Add(clause4);
            }
            return true;
        }

        private void OnAddFilterButtonClick(object sender, EventArgs e)
        {
            SqlDataSourceConfigureFilterForm form = new SqlDataSourceConfigureFilterForm(this._sqlDataSourceDesigner, this._tableQuery);
            if (UIServiceHelper.ShowDialog(this._sqlDataSourceDesigner.Component.Site, form) == DialogResult.OK)
            {
                this._tableQuery.FilterClauses.Clear();
                foreach (SqlDataSourceFilterClause clause in form.FilterClauses)
                {
                    this._tableQuery.FilterClauses.Add(clause);
                }
                this.UpdatePreview();
            }
        }

        private void OnAddSortButtonClick(object sender, EventArgs e)
        {
            SqlDataSourceConfigureSortForm form = new SqlDataSourceConfigureSortForm(this._sqlDataSourceDesigner, this._tableQuery);
            if (UIServiceHelper.ShowDialog(this._sqlDataSourceDesigner.Component.Site, form) == DialogResult.OK)
            {
                this._tableQuery.OrderClauses.Clear();
                foreach (SqlDataSourceOrderClause clause in form.OrderClauses)
                {
                    this._tableQuery.OrderClauses.Add(clause);
                }
                this.UpdatePreview();
            }
        }

        private void OnAdvancedOptionsButtonClick(object sender, EventArgs e)
        {
            SqlDataSourceAdvancedOptionsForm form = new SqlDataSourceAdvancedOptionsForm(base.ServiceProvider);
            form.SetAllowAutogenerate(this._tableQuery.IsPrimaryKeySelected() && !this._selectDistinctCheckBox.Checked);
            form.GenerateStatements = this._generateMode > 0;
            form.OptimisticConcurrency = this._generateMode == 2;
            if (UIServiceHelper.ShowDialog(base.ServiceProvider, form) == DialogResult.OK)
            {
                this._generateMode = 0;
                if (form.GenerateStatements)
                {
                    if (form.OptimisticConcurrency)
                    {
                        this._generateMode = 2;
                    }
                    else
                    {
                        this._generateMode = 1;
                    }
                }
            }
        }

        protected internal override void OnComplete()
        {
            if (this._tableRadioButton.Checked)
            {
                this._sqlDataSourceDesigner.TableQueryState = this.SaveTableQueryState();
                SqlDataSource component = (SqlDataSource) this._sqlDataSourceDesigner.Component;
                bool adjustForOptimisticConcurrency = this._generateMode == 2;
                component.OldValuesParameterFormatString = GetOldValuesFormatString(component, adjustForOptimisticConcurrency);
                if (adjustForOptimisticConcurrency)
                {
                    component.ConflictDetection = ConflictOptions.CompareAllValues;
                }
                else
                {
                    component.ConflictDetection = ConflictOptions.OverwriteChanges;
                }
            }
            else
            {
                this._sqlDataSourceDesigner.TableQueryState = null;
            }
        }

        private void OnCustomSqlRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledUI();
        }

        private void OnFieldsCheckedListBoxItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!this._ignoreFieldCheckChanges)
            {
                this.UpdateEnabledUI();
                base.ParentWizard.NextButton.Enabled = (e.NewValue != CheckState.Unchecked) || (this._fieldsCheckedListBox.CheckedItems.Count != 1);
                if ((e.Index == 0) && (e.NewValue == CheckState.Checked))
                {
                    this._tableQuery.AsteriskField = true;
                    this._ignoreFieldCheckChanges = true;
                    for (int i = 1; i < this._fieldsCheckedListBox.Items.Count; i++)
                    {
                        this._fieldsCheckedListBox.SetItemChecked(i, false);
                    }
                    this._ignoreFieldCheckChanges = false;
                }
                else
                {
                    this._tableQuery.AsteriskField = false;
                    this._ignoreFieldCheckChanges = true;
                    this._fieldsCheckedListBox.SetItemChecked(0, false);
                    if (e.Index > 0)
                    {
                        if (e.NewValue == CheckState.Checked)
                        {
                            this._tableQuery.Fields.Add(((ColumnItem) this._fieldsCheckedListBox.Items[e.Index]).DesignerDataColumn);
                        }
                        else
                        {
                            this._tableQuery.Fields.Remove(((ColumnItem) this._fieldsCheckedListBox.Items[e.Index]).DesignerDataColumn);
                        }
                    }
                    this._ignoreFieldCheckChanges = false;
                }
                if (!this._tableQuery.IsPrimaryKeySelected() || this._selectDistinctCheckBox.Checked)
                {
                    this._generateMode = 0;
                }
                this.UpdatePreview();
            }
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFonts();
        }

        public override bool OnNext()
        {
            if (this._tableRadioButton.Checked)
            {
                SqlDataSourceQuery insertQuery;
                SqlDataSourceQuery updateQuery;
                SqlDataSourceQuery deleteQuery;
                SqlDataSourceQuery selectQuery = this._tableQuery.GetSelectQuery();
                if (selectQuery == null)
                {
                    selectQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                }
                if (this._generateMode > 0)
                {
                    SqlDataSource sqlDataSource = (SqlDataSource) this._sqlDataSourceDesigner.Component;
                    bool adjustForOptimisticConcurrency = this._generateMode == 2;
                    string oldValuesFormatString = GetOldValuesFormatString(sqlDataSource, adjustForOptimisticConcurrency);
                    insertQuery = this._tableQuery.GetInsertQuery();
                    if (insertQuery == null)
                    {
                        insertQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                    }
                    updateQuery = this._tableQuery.GetUpdateQuery(oldValuesFormatString, adjustForOptimisticConcurrency);
                    if (updateQuery == null)
                    {
                        updateQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                    }
                    deleteQuery = this._tableQuery.GetDeleteQuery(oldValuesFormatString, adjustForOptimisticConcurrency);
                    if (deleteQuery == null)
                    {
                        deleteQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                    }
                }
                else
                {
                    insertQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                    updateQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                    deleteQuery = new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                }
                SqlDataSourceSummaryPanel summaryPanel = base.NextPanel as SqlDataSourceSummaryPanel;
                if (summaryPanel == null)
                {
                    summaryPanel = ((SqlDataSourceWizardForm) base.ParentWizard).GetSummaryPanel();
                    base.NextPanel = summaryPanel;
                }
                summaryPanel.SetQueries(this._dataConnection, selectQuery, insertQuery, updateQuery, deleteQuery);
                return true;
            }
            SqlDataSourceCustomCommandPanel nextPanel = base.NextPanel as SqlDataSourceCustomCommandPanel;
            if (nextPanel == null)
            {
                nextPanel = ((SqlDataSourceWizardForm) base.ParentWizard).GetCustomCommandPanel();
                base.NextPanel = nextPanel;
            }
            SqlDataSource component = (SqlDataSource) this._sqlDataSourceDesigner.Component;
            ArrayList dest = new ArrayList();
            ArrayList list2 = new ArrayList();
            ArrayList list3 = new ArrayList();
            ArrayList list4 = new ArrayList();
            this._sqlDataSourceDesigner.CopyList(component.SelectParameters, dest);
            this._sqlDataSourceDesigner.CopyList(component.InsertParameters, list2);
            this._sqlDataSourceDesigner.CopyList(component.UpdateParameters, list3);
            this._sqlDataSourceDesigner.CopyList(component.DeleteParameters, list4);
            nextPanel.SetQueries(this._dataConnection, new SqlDataSourceQuery(component.SelectCommand, component.SelectCommandType, dest), new SqlDataSourceQuery(component.InsertCommand, component.InsertCommandType, list2), new SqlDataSourceQuery(component.UpdateCommand, component.UpdateCommandType, list3), new SqlDataSourceQuery(component.DeleteCommand, component.DeleteCommandType, list4));
            return true;
        }

        public override void OnPrevious()
        {
        }

        private void OnSelectDistinctCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this._tableQuery.Distinct = this._selectDistinctCheckBox.Checked;
            if (!this._tableQuery.IsPrimaryKeySelected() || this._selectDistinctCheckBox.Checked)
            {
                this._generateMode = 0;
            }
            this.UpdatePreview();
        }

        private void OnTableRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledUI();
        }

        private void OnTablesComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            TableItem selectedItem = this._tablesComboBox.SelectedItem as TableItem;
            if ((selectedItem == null) || (this._previousTable != selectedItem))
            {
                Cursor current = Cursor.Current;
                this._fieldsCheckedListBox.Items.Clear();
                this._selectDistinctCheckBox.Checked = false;
                this._generateMode = 0;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    if (selectedItem != null)
                    {
                        ICollection columns = selectedItem.DesignerDataTable.Columns;
                        this._tableQuery = new SqlDataSourceTableQuery(this._dataConnection, selectedItem.DesignerDataTable);
                        this._fieldsCheckedListBox.Items.Add(new ColumnItem());
                        foreach (DesignerDataColumn column in columns)
                        {
                            this._fieldsCheckedListBox.Items.Add(new ColumnItem(column));
                        }
                        this._tableQuery.AsteriskField = true;
                        this._fieldsCheckedListBox.SetItemChecked(0, true);
                    }
                    else
                    {
                        this._tableQuery = null;
                    }
                    this._previousTable = selectedItem;
                }
                catch (Exception exception)
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("SqlDataSourceConfigureSelectPanel_CouldNotGetTableSchema"));
                }
                finally
                {
                    UIHelper.UpdateFieldsCheckedListBoxColumnWidth(this._fieldsCheckedListBox);
                    this.UpdateEnabledUI();
                    this.UpdatePreview();
                    Cursor.Current = current;
                }
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                base.ParentWizard.FinishButton.Enabled = false;
                DesignerDataConnection designerDataConnection = ((SqlDataSourceWizardForm) base.ParentWizard).DesignerDataConnection;
                if (!SqlDataSourceDesigner.ConnectionsEqual(this._dataConnection, designerDataConnection))
                {
                    this._dataConnection = designerDataConnection;
                    this._requiresRefresh = true;
                }
                if (this._requiresRefresh)
                {
                    Cursor current = Cursor.Current;
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        this._tablesComboBox.SelectedIndex = -1;
                        this._tablesComboBox.Items.Clear();
                        IDataEnvironment dataEnvironment = ((SqlDataSourceWizardForm) base.ParentWizard).DataEnvironment;
                        IDesignerDataSchema connectionSchema = null;
                        if (this._dataConnection != null)
                        {
                            connectionSchema = dataEnvironment.GetConnectionSchema(this._dataConnection);
                        }
                        if (connectionSchema != null)
                        {
                            List<TableItem> list = new List<TableItem>();
                            if (connectionSchema.SupportsSchemaClass(DesignerDataSchemaClass.Tables))
                            {
                                ICollection schemaItems = connectionSchema.GetSchemaItems(DesignerDataSchemaClass.Tables);
                                if (schemaItems != null)
                                {
                                    foreach (DesignerDataTable table in schemaItems)
                                    {
                                        if (!table.Name.ToLowerInvariant().StartsWith("AspNet_".ToLowerInvariant(), StringComparison.Ordinal))
                                        {
                                            list.Add(new TableItem(table));
                                        }
                                    }
                                }
                            }
                            if (connectionSchema.SupportsSchemaClass(DesignerDataSchemaClass.Views))
                            {
                                ICollection is3 = connectionSchema.GetSchemaItems(DesignerDataSchemaClass.Views);
                                if (is3 != null)
                                {
                                    foreach (DesignerDataView view in is3)
                                    {
                                        list.Add(new TableItem(view));
                                    }
                                }
                            }
                            list.Sort((Comparison<TableItem>) ((a, b) => string.Compare(a.DesignerDataTable.Name, b.DesignerDataTable.Name, StringComparison.InvariantCultureIgnoreCase)));
                            this._tablesComboBox.Items.AddRange(list.ToArray());
                            this._tablesComboBox.InvalidateDropDownWidth();
                        }
                        if (this._tablesComboBox.Items.Count > 0)
                        {
                            Hashtable tableQueryState = this._sqlDataSourceDesigner.TableQueryState;
                            bool flag = false;
                            if (tableQueryState != null)
                            {
                                flag = this.LoadTableQueryState(tableQueryState);
                            }
                            if (!flag)
                            {
                                flag = this.LoadParsedSqlState();
                            }
                            if (!flag)
                            {
                                this._tablesComboBox.SelectedIndex = 0;
                                SqlDataSource component = (SqlDataSource) this._sqlDataSourceDesigner.Component;
                                bool flag2 = (((component.SelectCommand.Trim().Length > 0) || (component.InsertCommand.Trim().Length > 0)) || (component.UpdateCommand.Trim().Length > 0)) || (component.DeleteCommand.Trim().Length > 0);
                                this._tableRadioButton.Checked = !flag2;
                                this._customSqlRadioButton.Checked = flag2;
                            }
                            else
                            {
                                this._tableRadioButton.Checked = true;
                                this._customSqlRadioButton.Checked = false;
                            }
                            this._tableRadioButton.Enabled = true;
                        }
                        else
                        {
                            this._customSqlRadioButton.Checked = true;
                            this._tableRadioButton.Enabled = false;
                        }
                        this.UpdatePreview();
                    }
                    finally
                    {
                        Cursor.Current = current;
                    }
                    this._requiresRefresh = false;
                }
                this.UpdateEnabledUI();
            }
        }

        public void ResetUI()
        {
            this._tableRadioButton.Checked = true;
            this._customSqlRadioButton.Checked = false;
            this._generateMode = 0;
            this._tablesComboBox.Items.Clear();
            this._fieldsCheckedListBox.Items.Clear();
            this._requiresRefresh = true;
        }

        private Hashtable SaveTableQueryState()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add("Conn_ConnectionStringHash", this._tableQuery.DesignerDataConnection.ConnectionString.GetHashCode());
            hashtable.Add("Conn_ProviderName", this._tableQuery.DesignerDataConnection.ProviderName);
            hashtable.Add("Generate_Mode", this._generateMode);
            hashtable.Add("Table_Name", this._tableQuery.DesignerDataTable.Name);
            hashtable.Add("Fields_Count", this._tableQuery.Fields.Count);
            for (int i = 0; i < this._tableQuery.Fields.Count; i++)
            {
                hashtable.Add("Fields_FieldName" + i.ToString(CultureInfo.InvariantCulture), this._tableQuery.Fields[i].Name);
            }
            hashtable.Add("AsteriskField", this._tableQuery.AsteriskField);
            hashtable.Add("Distinct", this._tableQuery.Distinct);
            hashtable.Add("Filters_Count", this._tableQuery.FilterClauses.Count);
            for (int j = 0; j < this._tableQuery.FilterClauses.Count; j++)
            {
                SqlDataSourceFilterClause clause = this._tableQuery.FilterClauses[j];
                string str = j.ToString(CultureInfo.InvariantCulture);
                hashtable.Add("Filters_FieldName" + str, clause.DesignerDataColumn.Name);
                hashtable.Add("Filters_OperatorFormat" + str, clause.OperatorFormat);
                hashtable.Add("Filters_IsBinary" + str, clause.IsBinary);
                hashtable.Add("Filters_Value" + str, clause.Value);
                hashtable.Add("Filters_ParameterName" + str, (clause.Parameter != null) ? clause.Parameter.Name : null);
            }
            hashtable.Add("Orders_Count", this._tableQuery.OrderClauses.Count);
            for (int k = 0; k < this._tableQuery.OrderClauses.Count; k++)
            {
                hashtable.Add("Orders_FieldName" + k.ToString(CultureInfo.InvariantCulture), this._tableQuery.OrderClauses[k].DesignerDataColumn.Name);
                hashtable.Add("Orders_IsDescending" + k.ToString(CultureInfo.InvariantCulture), this._tableQuery.OrderClauses[k].IsDescending);
            }
            return hashtable;
        }

        private void UpdateEnabledUI()
        {
            this._fieldChooserPanel.Enabled = this._tableRadioButton.Checked;
            if (this._customSqlRadioButton.Checked)
            {
                base.ParentWizard.NextButton.Enabled = true;
            }
            if (this._tableRadioButton.Checked)
            {
                base.ParentWizard.NextButton.Enabled = (this._tablesComboBox.Items.Count > 0) && (this._fieldsCheckedListBox.CheckedItems.Count > 0);
                bool flag = this._fieldsCheckedListBox.Items.Count > 0;
                this._fieldsLabel.Enabled = flag;
                this._fieldsCheckedListBox.Enabled = flag;
                this._selectDistinctCheckBox.Enabled = flag;
                this._addFilterButton.Enabled = flag;
                this._addSortButton.Enabled = flag;
                this._advancedOptionsButton.Enabled = flag;
                this._previewLabel.Enabled = flag;
                this._previewTextBox.Enabled = flag;
            }
        }

        private void UpdateFonts()
        {
            Font font = new Font(this.Font, FontStyle.Bold);
            this._retrieveDataLabel.Font = font;
        }

        private void UpdatePreview()
        {
            if (this._tableQuery != null)
            {
                SqlDataSourceQuery selectQuery = this._tableQuery.GetSelectQuery();
                this._previewTextBox.Text = (selectQuery == null) ? string.Empty : selectQuery.Command;
            }
            else
            {
                this._previewTextBox.Text = string.Empty;
            }
        }

        private sealed class ColumnItem
        {
            private System.ComponentModel.Design.Data.DesignerDataColumn _designerDataColumn;

            public ColumnItem()
            {
            }

            public ColumnItem(System.ComponentModel.Design.Data.DesignerDataColumn designerDataColumn)
            {
                this._designerDataColumn = designerDataColumn;
            }

            public override string ToString()
            {
                if (this._designerDataColumn != null)
                {
                    return this._designerDataColumn.Name;
                }
                return "*";
            }

            public System.ComponentModel.Design.Data.DesignerDataColumn DesignerDataColumn
            {
                get
                {
                    return this._designerDataColumn;
                }
            }
        }

        private sealed class TableItem
        {
            private DesignerDataTableBase _designerDataTable;

            public TableItem(DesignerDataTableBase designerDataTable)
            {
                this._designerDataTable = designerDataTable;
            }

            public override string ToString()
            {
                return this._designerDataTable.Name;
            }

            public DesignerDataTableBase DesignerDataTable
            {
                get
                {
                    return this._designerDataTable;
                }
            }
        }
    }
}

