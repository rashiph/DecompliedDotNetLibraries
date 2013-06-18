namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;

    internal class SqlDataSourceConfigureSortForm : DesignerForm
    {
        private Button _cancelButton;
        private AutoSizeComboBox _fieldComboBox1;
        private AutoSizeComboBox _fieldComboBox2;
        private AutoSizeComboBox _fieldComboBox3;
        private Label _helpLabel;
        private bool _loadingClauses;
        private Button _okButton;
        private Label _previewLabel;
        private TextBox _previewTextBox;
        private RadioButton _sortAscendingRadioButton1;
        private RadioButton _sortAscendingRadioButton2;
        private RadioButton _sortAscendingRadioButton3;
        private GroupBox _sortByGroupBox1;
        private GroupBox _sortByGroupBox2;
        private GroupBox _sortByGroupBox3;
        private RadioButton _sortDescendingRadioButton1;
        private RadioButton _sortDescendingRadioButton2;
        private RadioButton _sortDescendingRadioButton3;
        private Panel _sortDirectionPanel1;
        private Panel _sortDirectionPanel2;
        private Panel _sortDirectionPanel3;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private SqlDataSourceTableQuery _tableQuery;

        public SqlDataSourceConfigureSortForm(SqlDataSourceDesigner sqlDataSourceDesigner, SqlDataSourceTableQuery tableQuery) : base(sqlDataSourceDesigner.Component.Site)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this._tableQuery = tableQuery.Clone();
            this.InitializeComponent();
            this.InitializeUI();
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                this._loadingClauses = true;
                this._fieldComboBox1.Items.Add(new ColumnItem(null));
                this._fieldComboBox2.Items.Add(new ColumnItem(null));
                this._fieldComboBox3.Items.Add(new ColumnItem(null));
                foreach (DesignerDataColumn column in this._tableQuery.DesignerDataTable.Columns)
                {
                    this._fieldComboBox1.Items.Add(new ColumnItem(column));
                    this._fieldComboBox2.Items.Add(new ColumnItem(column));
                    this._fieldComboBox3.Items.Add(new ColumnItem(column));
                }
                this._fieldComboBox1.InvalidateDropDownWidth();
                this._fieldComboBox2.InvalidateDropDownWidth();
                this._fieldComboBox3.InvalidateDropDownWidth();
                this._sortByGroupBox2.Enabled = false;
                this._sortByGroupBox3.Enabled = false;
                this._sortDirectionPanel1.Enabled = false;
                this._sortDirectionPanel2.Enabled = false;
                this._sortDirectionPanel3.Enabled = false;
                this._sortAscendingRadioButton1.Checked = true;
                this._sortAscendingRadioButton2.Checked = true;
                this._sortAscendingRadioButton3.Checked = true;
                if (this._tableQuery.OrderClauses.Count >= 1)
                {
                    SqlDataSourceOrderClause clause = this._tableQuery.OrderClauses[0];
                    this.SelectFieldItem(this._fieldComboBox1, clause.DesignerDataColumn);
                    this._sortAscendingRadioButton1.Checked = !clause.IsDescending;
                    this._sortDescendingRadioButton1.Checked = clause.IsDescending;
                    if (this._tableQuery.OrderClauses.Count >= 2)
                    {
                        SqlDataSourceOrderClause clause2 = this._tableQuery.OrderClauses[1];
                        this.SelectFieldItem(this._fieldComboBox2, clause2.DesignerDataColumn);
                        this._sortAscendingRadioButton2.Checked = !clause2.IsDescending;
                        this._sortDescendingRadioButton2.Checked = clause2.IsDescending;
                        if (this._tableQuery.OrderClauses.Count >= 3)
                        {
                            SqlDataSourceOrderClause clause3 = this._tableQuery.OrderClauses[2];
                            this.SelectFieldItem(this._fieldComboBox3, clause3.DesignerDataColumn);
                            this._sortAscendingRadioButton3.Checked = !clause3.IsDescending;
                            this._sortDescendingRadioButton3.Checked = clause3.IsDescending;
                        }
                    }
                }
                this._loadingClauses = false;
                this.UpdateOrderClauses();
                this.UpdatePreview();
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private void InitializeComponent()
        {
            this._helpLabel = new Label();
            this._previewLabel = new Label();
            this._previewTextBox = new TextBox();
            this._sortAscendingRadioButton1 = new RadioButton();
            this._sortDescendingRadioButton1 = new RadioButton();
            this._sortDirectionPanel1 = new Panel();
            this._fieldComboBox1 = new AutoSizeComboBox();
            this._okButton = new Button();
            this._cancelButton = new Button();
            this._sortDescendingRadioButton2 = new RadioButton();
            this._sortAscendingRadioButton2 = new RadioButton();
            this._fieldComboBox2 = new AutoSizeComboBox();
            this._sortDirectionPanel2 = new Panel();
            this._sortDescendingRadioButton3 = new RadioButton();
            this._sortAscendingRadioButton3 = new RadioButton();
            this._fieldComboBox3 = new AutoSizeComboBox();
            this._sortDirectionPanel3 = new Panel();
            this._sortByGroupBox1 = new GroupBox();
            this._sortByGroupBox2 = new GroupBox();
            this._sortByGroupBox3 = new GroupBox();
            this._sortDirectionPanel1.SuspendLayout();
            this._sortDirectionPanel2.SuspendLayout();
            this._sortDirectionPanel3.SuspendLayout();
            this._sortByGroupBox1.SuspendLayout();
            this._sortByGroupBox2.SuspendLayout();
            this._sortByGroupBox3.SuspendLayout();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(12, 12);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x17e, 0x10);
            this._helpLabel.TabIndex = 10;
            this._sortAscendingRadioButton1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortAscendingRadioButton1.Location = new Point(0, 0);
            this._sortAscendingRadioButton1.Name = "_sortAscendingRadioButton1";
            this._sortAscendingRadioButton1.Size = new Size(200, 0x12);
            this._sortAscendingRadioButton1.TabIndex = 10;
            this._sortAscendingRadioButton1.CheckedChanged += new EventHandler(this.OnSortAscendingRadioButton1CheckedChanged);
            this._sortDescendingRadioButton1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortDescendingRadioButton1.Location = new Point(0, 0x12);
            this._sortDescendingRadioButton1.Name = "_sortDescendingRadioButton1";
            this._sortDescendingRadioButton1.Size = new Size(200, 0x12);
            this._sortDescendingRadioButton1.TabIndex = 20;
            this._sortDirectionPanel1.Controls.Add(this._sortDescendingRadioButton1);
            this._sortDirectionPanel1.Controls.Add(this._sortAscendingRadioButton1);
            this._sortDirectionPanel1.Location = new Point(0xa9, 12);
            this._sortDirectionPanel1.Name = "_sortDirectionPanel1";
            this._sortDirectionPanel1.Size = new Size(200, 0x26);
            this._sortDirectionPanel1.TabIndex = 20;
            this._fieldComboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            this._fieldComboBox1.Location = new Point(9, 20);
            this._fieldComboBox1.Name = "_fieldComboBox1";
            this._fieldComboBox1.Size = new Size(0x99, 0x15);
            this._fieldComboBox1.Sorted = true;
            this._fieldComboBox1.TabIndex = 10;
            this._fieldComboBox1.SelectedIndexChanged += new EventHandler(this.OnFieldComboBox1SelectedIndexChanged);
            this._sortDescendingRadioButton2.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortDescendingRadioButton2.Location = new Point(0, 0x12);
            this._sortDescendingRadioButton2.Name = "_sortDescendingRadioButton2";
            this._sortDescendingRadioButton2.Size = new Size(200, 0x12);
            this._sortDescendingRadioButton2.TabIndex = 20;
            this._sortAscendingRadioButton2.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortAscendingRadioButton2.Location = new Point(0, 0);
            this._sortAscendingRadioButton2.Name = "_sortAscendingRadioButton2";
            this._sortAscendingRadioButton2.Size = new Size(200, 0x12);
            this._sortAscendingRadioButton2.TabIndex = 10;
            this._sortAscendingRadioButton2.CheckedChanged += new EventHandler(this.OnSortAscendingRadioButton2CheckedChanged);
            this._fieldComboBox2.DropDownStyle = ComboBoxStyle.DropDownList;
            this._fieldComboBox2.Location = new Point(9, 20);
            this._fieldComboBox2.Name = "_fieldComboBox2";
            this._fieldComboBox2.Size = new Size(0x99, 0x15);
            this._fieldComboBox2.Sorted = true;
            this._fieldComboBox2.TabIndex = 10;
            this._fieldComboBox2.SelectedIndexChanged += new EventHandler(this.OnFieldComboBox2SelectedIndexChanged);
            this._sortDirectionPanel2.Controls.Add(this._sortDescendingRadioButton2);
            this._sortDirectionPanel2.Controls.Add(this._sortAscendingRadioButton2);
            this._sortDirectionPanel2.Location = new Point(0xa9, 12);
            this._sortDirectionPanel2.Name = "_sortDirectionPanel2";
            this._sortDirectionPanel2.Size = new Size(200, 0x26);
            this._sortDirectionPanel2.TabIndex = 20;
            this._sortDescendingRadioButton3.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortDescendingRadioButton3.Location = new Point(0, 0x12);
            this._sortDescendingRadioButton3.Name = "_sortDescendingRadioButton3";
            this._sortDescendingRadioButton3.Size = new Size(200, 0x12);
            this._sortDescendingRadioButton3.TabIndex = 20;
            this._sortAscendingRadioButton3.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortAscendingRadioButton3.Location = new Point(0, 0);
            this._sortAscendingRadioButton3.Name = "_sortAscendingRadioButton3";
            this._sortAscendingRadioButton3.Size = new Size(200, 0x12);
            this._sortAscendingRadioButton3.TabIndex = 10;
            this._sortAscendingRadioButton3.CheckedChanged += new EventHandler(this.OnSortAscendingRadioButton3CheckedChanged);
            this._fieldComboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
            this._fieldComboBox3.Location = new Point(9, 20);
            this._fieldComboBox3.Name = "_fieldComboBox3";
            this._fieldComboBox3.Size = new Size(0x99, 0x15);
            this._fieldComboBox3.Sorted = true;
            this._fieldComboBox3.TabIndex = 10;
            this._fieldComboBox3.SelectedIndexChanged += new EventHandler(this.OnFieldComboBox3SelectedIndexChanged);
            this._sortDirectionPanel3.Controls.Add(this._sortDescendingRadioButton3);
            this._sortDirectionPanel3.Controls.Add(this._sortAscendingRadioButton3);
            this._sortDirectionPanel3.Location = new Point(0xa9, 12);
            this._sortDirectionPanel3.Name = "_sortDirectionPanel3";
            this._sortDirectionPanel3.Size = new Size(200, 0x26);
            this._sortDirectionPanel3.TabIndex = 20;
            this._sortByGroupBox1.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortByGroupBox1.Controls.Add(this._fieldComboBox1);
            this._sortByGroupBox1.Controls.Add(this._sortDirectionPanel1);
            this._sortByGroupBox1.Location = new Point(12, 0x21);
            this._sortByGroupBox1.Name = "_sortByGroupBox1";
            this._sortByGroupBox1.Size = new Size(0x180, 0x38);
            this._sortByGroupBox1.TabIndex = 20;
            this._sortByGroupBox1.TabStop = false;
            this._sortByGroupBox2.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortByGroupBox2.Controls.Add(this._fieldComboBox2);
            this._sortByGroupBox2.Controls.Add(this._sortDirectionPanel2);
            this._sortByGroupBox2.Location = new Point(12, 0x5f);
            this._sortByGroupBox2.Name = "_sortByGroupBox2";
            this._sortByGroupBox2.Size = new Size(0x180, 0x38);
            this._sortByGroupBox2.TabIndex = 30;
            this._sortByGroupBox2.TabStop = false;
            this._sortByGroupBox3.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sortByGroupBox3.Controls.Add(this._fieldComboBox3);
            this._sortByGroupBox3.Controls.Add(this._sortDirectionPanel3);
            this._sortByGroupBox3.Location = new Point(12, 0x9d);
            this._sortByGroupBox3.Name = "_sortByGroupBox3";
            this._sortByGroupBox3.Size = new Size(0x180, 0x38);
            this._sortByGroupBox3.TabIndex = 40;
            this._sortByGroupBox3.TabStop = false;
            this._previewLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._previewLabel.Location = new Point(12, 0xdb);
            this._previewLabel.Name = "_previewLabel";
            this._previewLabel.Size = new Size(0x180, 13);
            this._previewLabel.TabIndex = 50;
            this._previewTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._previewTextBox.BackColor = SystemColors.Control;
            this._previewTextBox.Location = new Point(12, 0xed);
            this._previewTextBox.Multiline = true;
            this._previewTextBox.Name = "_previewTextBox";
            this._previewTextBox.ReadOnly = true;
            this._previewTextBox.ScrollBars = ScrollBars.Vertical;
            this._previewTextBox.Size = new Size(0x180, 0x48);
            this._previewTextBox.TabIndex = 60;
            this._previewTextBox.Text = "";
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(240, 0x141);
            this._okButton.Name = "_okButton";
            this._okButton.TabIndex = 70;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.DialogResult = DialogResult.Cancel;
            this._cancelButton.Location = new Point(0x141, 0x141);
            this._cancelButton.Name = "_cancelButton";
            this._cancelButton.TabIndex = 80;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x198, 0x164);
            base.Controls.Add(this._sortByGroupBox2);
            base.Controls.Add(this._sortByGroupBox3);
            base.Controls.Add(this._sortByGroupBox1);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._previewTextBox);
            base.Controls.Add(this._previewLabel);
            base.Controls.Add(this._helpLabel);
            this.MinimumSize = new Size(0x1a0, 390);
            base.Name = "SqlDataSourceConfigureSortForm";
            base.SizeGripStyle = SizeGripStyle.Show;
            this._sortDirectionPanel1.ResumeLayout(false);
            this._sortDirectionPanel2.ResumeLayout(false);
            this._sortDirectionPanel3.ResumeLayout(false);
            this._sortByGroupBox1.ResumeLayout(false);
            this._sortByGroupBox2.ResumeLayout(false);
            this._sortByGroupBox3.ResumeLayout(false);
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_HelpLabel");
            this._previewLabel.Text = System.Design.SR.GetString("SqlDataSource_General_PreviewLabel");
            this._sortByGroupBox1.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortByLabel");
            this._sortByGroupBox2.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_ThenByLabel");
            this._sortByGroupBox3.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_ThenByLabel");
            this._sortAscendingRadioButton1.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_AscendingLabel");
            this._sortDescendingRadioButton1.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_DescendingLabel");
            this._sortAscendingRadioButton2.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_AscendingLabel");
            this._sortDescendingRadioButton2.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_DescendingLabel");
            this._sortAscendingRadioButton3.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_AscendingLabel");
            this._sortDescendingRadioButton3.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_DescendingLabel");
            this._sortAscendingRadioButton1.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortDirection1");
            this._sortDescendingRadioButton1.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortDirection1");
            this._sortAscendingRadioButton2.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortDirection2");
            this._sortDescendingRadioButton2.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortDirection2");
            this._sortAscendingRadioButton3.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortDirection3");
            this._sortDescendingRadioButton3.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortDirection3");
            this._fieldComboBox1.AccessibleName = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortColumn1");
            this._fieldComboBox2.AccessibleName = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortColumn2");
            this._fieldComboBox3.AccessibleName = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortColumn3");
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this.Text = System.Design.SR.GetString("SqlDataSourceConfigureSortForm_Caption");
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnFieldComboBox1SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this._fieldComboBox1.SelectedIndex == -1) || ((this._fieldComboBox1.SelectedIndex == 0) && (((ColumnItem) this._fieldComboBox1.Items[0]).DesignerDataColumn == null)))
            {
                this._sortDirectionPanel1.Enabled = false;
                this._sortAscendingRadioButton1.Checked = true;
                this._fieldComboBox2.SelectedIndex = -1;
                this._sortAscendingRadioButton2.Checked = true;
                this._sortByGroupBox2.Enabled = false;
                this._fieldComboBox2.Enabled = false;
            }
            else
            {
                this._sortDirectionPanel1.Enabled = true;
                this._sortByGroupBox2.Enabled = true;
                this._fieldComboBox2.Enabled = true;
            }
            this.UpdateOrderClauses();
            this.UpdatePreview();
        }

        private void OnFieldComboBox2SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this._fieldComboBox2.SelectedIndex == -1) || ((this._fieldComboBox2.SelectedIndex == 0) && (((ColumnItem) this._fieldComboBox2.Items[0]).DesignerDataColumn == null)))
            {
                this._sortDirectionPanel2.Enabled = false;
                this._sortAscendingRadioButton2.Checked = true;
                this._fieldComboBox3.SelectedIndex = -1;
                this._sortAscendingRadioButton3.Checked = true;
                this._sortByGroupBox3.Enabled = false;
                this._fieldComboBox3.Enabled = false;
            }
            else
            {
                this._sortDirectionPanel2.Enabled = true;
                this._sortByGroupBox3.Enabled = true;
                this._fieldComboBox3.Enabled = true;
            }
            this.UpdateOrderClauses();
            this.UpdatePreview();
        }

        private void OnFieldComboBox3SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((this._fieldComboBox3.SelectedIndex == -1) || ((this._fieldComboBox3.SelectedIndex == 0) && (((ColumnItem) this._fieldComboBox3.Items[0]).DesignerDataColumn == null)))
            {
                this._sortDirectionPanel3.Enabled = false;
                this._sortAscendingRadioButton3.Checked = true;
            }
            else
            {
                this._sortDirectionPanel3.Enabled = true;
            }
            this.UpdateOrderClauses();
            this.UpdatePreview();
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void OnSortAscendingRadioButton1CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateOrderClauses();
            this.UpdatePreview();
        }

        private void OnSortAscendingRadioButton2CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateOrderClauses();
            this.UpdatePreview();
        }

        private void OnSortAscendingRadioButton3CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateOrderClauses();
            this.UpdatePreview();
        }

        private void SelectFieldItem(ComboBox comboBox, DesignerDataColumn field)
        {
            foreach (ColumnItem item in comboBox.Items)
            {
                if (item.DesignerDataColumn == field)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void UpdateOrderClauses()
        {
            if (!this._loadingClauses)
            {
                this._tableQuery.OrderClauses.Clear();
                if (this._fieldComboBox1.SelectedIndex >= 1)
                {
                    SqlDataSourceOrderClause item = new SqlDataSourceOrderClause(this._tableQuery.DesignerDataConnection, this._tableQuery.DesignerDataTable, ((ColumnItem) this._fieldComboBox1.SelectedItem).DesignerDataColumn, !this._sortAscendingRadioButton1.Checked);
                    this._tableQuery.OrderClauses.Add(item);
                }
                if (this._fieldComboBox2.SelectedIndex >= 1)
                {
                    SqlDataSourceOrderClause clause2 = new SqlDataSourceOrderClause(this._tableQuery.DesignerDataConnection, this._tableQuery.DesignerDataTable, ((ColumnItem) this._fieldComboBox2.SelectedItem).DesignerDataColumn, !this._sortAscendingRadioButton2.Checked);
                    this._tableQuery.OrderClauses.Add(clause2);
                }
                if (this._fieldComboBox3.SelectedIndex >= 1)
                {
                    SqlDataSourceOrderClause clause3 = new SqlDataSourceOrderClause(this._tableQuery.DesignerDataConnection, this._tableQuery.DesignerDataTable, ((ColumnItem) this._fieldComboBox3.SelectedItem).DesignerDataColumn, !this._sortAscendingRadioButton3.Checked);
                    this._tableQuery.OrderClauses.Add(clause3);
                }
            }
        }

        private void UpdatePreview()
        {
            SqlDataSourceQuery selectQuery = this._tableQuery.GetSelectQuery();
            this._previewTextBox.Text = (selectQuery == null) ? string.Empty : selectQuery.Command;
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.SqlDataSource.ConfigureSort";
            }
        }

        public IList<SqlDataSourceOrderClause> OrderClauses
        {
            get
            {
                return this._tableQuery.OrderClauses;
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
                if (this._designerDataColumn != null)
                {
                    return this._designerDataColumn.Name;
                }
                return System.Design.SR.GetString("SqlDataSourceConfigureSortForm_SortNone");
            }

            public System.ComponentModel.Design.Data.DesignerDataColumn DesignerDataColumn
            {
                get
                {
                    return this._designerDataColumn;
                }
            }
        }
    }
}

