namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Data;
    using System.Data;
    using System.Data.Common;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceSummaryPanel : WizardPanel
    {
        private DesignerDataConnection _dataConnection;
        private SqlDataSourceQuery _deleteQuery;
        private System.Windows.Forms.Label _helpLabel;
        private SqlDataSourceQuery _insertQuery;
        private System.Windows.Forms.Label _previewLabel;
        private System.Windows.Forms.TextBox _previewTextBox;
        private DataGridView _resultsGridView;
        private SqlDataSourceQuery _selectQuery;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private System.Windows.Forms.Button _testQueryButton;
        private SqlDataSourceQuery _updateQuery;

        public SqlDataSourceSummaryPanel(SqlDataSourceDesigner sqlDataSourceDesigner)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
        }

        private void InitializeComponent()
        {
            this._resultsGridView = new DataGridView();
            this._testQueryButton = new System.Windows.Forms.Button();
            this._previewTextBox = new System.Windows.Forms.TextBox();
            this._previewLabel = new System.Windows.Forms.Label();
            this._helpLabel = new System.Windows.Forms.Label();
            ((ISupportInitialize) this._resultsGridView).BeginInit();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x220, 0x20);
            this._helpLabel.TabIndex = 10;
            this._resultsGridView.AllowUserToAddRows = false;
            this._resultsGridView.AllowUserToDeleteRows = false;
            this._resultsGridView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._resultsGridView.Location = new Point(0, 0x26);
            this._resultsGridView.MultiSelect = false;
            this._resultsGridView.Name = "_resultsGridView";
            this._resultsGridView.ReadOnly = true;
            this._resultsGridView.RowHeadersVisible = false;
            this._resultsGridView.Size = new Size(0x220, 0x8d);
            this._resultsGridView.TabIndex = 20;
            this._resultsGridView.DataError += new DataGridViewDataErrorEventHandler(this.OnResultsGridViewDataError);
            this._testQueryButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._testQueryButton.Location = new Point(0x1a8, 0xb9);
            this._testQueryButton.Name = "_testQueryButton";
            this._testQueryButton.Size = new Size(120, 0x17);
            this._testQueryButton.TabIndex = 30;
            this._testQueryButton.Click += new EventHandler(this.OnTestQueryButtonClick);
            this._previewLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._previewLabel.Location = new Point(0, 0xd6);
            this._previewLabel.Name = "_previewLabel";
            this._previewLabel.Size = new Size(0x220, 0x10);
            this._previewLabel.TabIndex = 40;
            this._previewTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._previewTextBox.BackColor = SystemColors.Control;
            this._previewTextBox.Location = new Point(0, 0xe8);
            this._previewTextBox.Multiline = true;
            this._previewTextBox.Name = "_previewTextBox";
            this._previewTextBox.ReadOnly = true;
            this._previewTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._previewTextBox.Size = new Size(0x220, 0x2a);
            this._previewTextBox.TabIndex = 50;
            this._previewTextBox.Text = "";
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._previewLabel);
            base.Controls.Add(this._previewTextBox);
            base.Controls.Add(this._testQueryButton);
            base.Controls.Add(this._resultsGridView);
            base.Name = "SqlDataSourceSummaryPanel";
            base.Size = new Size(0x220, 0x112);
            ((ISupportInitialize) this._resultsGridView).EndInit();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            base.Caption = System.Design.SR.GetString("SqlDataSourceSummaryPanel_PanelCaption");
            this._testQueryButton.Text = System.Design.SR.GetString("SqlDataSourceSummaryPanel_TestQueryButton");
            this._previewLabel.Text = System.Design.SR.GetString("SqlDataSource_General_PreviewLabel");
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceSummaryPanel_HelpLabel");
            this._resultsGridView.AccessibleName = System.Design.SR.GetString("SqlDataSourceSummaryPanel_ResultsAccessibleName");
        }

        protected internal override void OnComplete()
        {
            PropertyDescriptor descriptor;
            SqlDataSource component = (SqlDataSource) this._sqlDataSourceDesigner.Component;
            if (component.DeleteCommand != this._deleteQuery.Command)
            {
                descriptor = TypeDescriptor.GetProperties(component)["DeleteCommand"];
                descriptor.SetValue(component, this._deleteQuery.Command);
            }
            if (component.DeleteCommandType != this._deleteQuery.CommandType)
            {
                descriptor = TypeDescriptor.GetProperties(component)["DeleteCommandType"];
                descriptor.SetValue(component, this._deleteQuery.CommandType);
            }
            if (component.InsertCommand != this._insertQuery.Command)
            {
                descriptor = TypeDescriptor.GetProperties(component)["InsertCommand"];
                descriptor.SetValue(component, this._insertQuery.Command);
            }
            if (component.InsertCommandType != this._insertQuery.CommandType)
            {
                descriptor = TypeDescriptor.GetProperties(component)["InsertCommandType"];
                descriptor.SetValue(component, this._insertQuery.CommandType);
            }
            if (component.SelectCommand != this._selectQuery.Command)
            {
                descriptor = TypeDescriptor.GetProperties(component)["SelectCommand"];
                descriptor.SetValue(component, this._selectQuery.Command);
            }
            if (component.SelectCommandType != this._selectQuery.CommandType)
            {
                descriptor = TypeDescriptor.GetProperties(component)["SelectCommandType"];
                descriptor.SetValue(component, this._selectQuery.CommandType);
            }
            if (component.UpdateCommand != this._updateQuery.Command)
            {
                descriptor = TypeDescriptor.GetProperties(component)["UpdateCommand"];
                descriptor.SetValue(component, this._updateQuery.Command);
            }
            if (component.UpdateCommandType != this._updateQuery.CommandType)
            {
                TypeDescriptor.GetProperties(component)["UpdateCommandType"].SetValue(component, this._updateQuery.CommandType);
            }
            this._sqlDataSourceDesigner.CopyList(this._selectQuery.Parameters, component.SelectParameters);
            this._sqlDataSourceDesigner.CopyList(this._insertQuery.Parameters, component.InsertParameters);
            this._sqlDataSourceDesigner.CopyList(this._updateQuery.Parameters, component.UpdateParameters);
            this._sqlDataSourceDesigner.CopyList(this._deleteQuery.Parameters, component.DeleteParameters);
            ParameterCollection parameters = new ParameterCollection();
            foreach (Parameter parameter in this._selectQuery.Parameters)
            {
                parameters.Add(parameter);
            }
            this._sqlDataSourceDesigner.RefreshSchema(this._dataConnection, this._selectQuery.Command, this._selectQuery.CommandType, parameters, true);
        }

        public override bool OnNext()
        {
            return true;
        }

        public override void OnPrevious()
        {
        }

        private void OnResultsGridViewDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.ThrowException = false;
        }

        private void OnTestQueryButtonClick(object sender, EventArgs e)
        {
            ParameterCollection parameters = new ParameterCollection();
            foreach (Parameter parameter in this._selectQuery.Parameters)
            {
                if (parameter.DbType == DbType.Object)
                {
                    parameters.Add(new Parameter(parameter.Name, parameter.Type, parameter.DefaultValue));
                }
                else
                {
                    parameters.Add(new Parameter(parameter.Name, parameter.DbType, parameter.DefaultValue));
                }
            }
            if (parameters.Count > 0)
            {
                SqlDataSourceParameterValueEditorForm form = new SqlDataSourceParameterValueEditorForm(base.ServiceProvider, parameters);
                if (UIServiceHelper.ShowDialog(base.ServiceProvider, form) == DialogResult.Cancel)
                {
                    return;
                }
            }
            this._resultsGridView.DataSource = null;
            DbCommand command = null;
            Cursor current = Cursor.Current;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                DbProviderFactory dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this._dataConnection.ProviderName);
                DbConnection designTimeConnection = null;
                try
                {
                    designTimeConnection = SqlDataSourceDesigner.GetDesignTimeConnection(base.ServiceProvider, this._dataConnection);
                }
                catch (Exception exception)
                {
                    if (designTimeConnection == null)
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("SqlDataSourceSummaryPanel_CouldNotCreateConnection"));
                        return;
                    }
                }
                if (designTimeConnection == null)
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("SqlDataSourceSummaryPanel_CouldNotCreateConnection"));
                }
                else
                {
                    command = this._sqlDataSourceDesigner.BuildSelectCommand(dbProviderFactory, designTimeConnection, this._selectQuery.Command, parameters, this._selectQuery.CommandType);
                    DbDataAdapter adapter = SqlDataSourceDesigner.CreateDataAdapter(dbProviderFactory, command);
                    adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);
                    if (dataSet.Tables.Count == 0)
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("SqlDataSourceSummaryPanel_CannotExecuteQueryNoTables"));
                    }
                    else
                    {
                        this._resultsGridView.DataSource = dataSet.Tables[0];
                        foreach (DataGridViewColumn column in this._resultsGridView.Columns)
                        {
                            column.SortMode = DataGridViewColumnSortMode.NotSortable;
                        }
                        this._resultsGridView.AutoResizeColumnHeadersHeight();
                        this._resultsGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                    }
                }
            }
            catch (Exception exception2)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, exception2, System.Design.SR.GetString("SqlDataSourceSummaryPanel_CannotExecuteQuery"));
            }
            finally
            {
                if ((command != null) && (command.Connection.State == ConnectionState.Open))
                {
                    command.Connection.Close();
                }
                Cursor.Current = current;
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                base.ParentWizard.NextButton.Enabled = false;
                base.ParentWizard.FinishButton.Enabled = true;
            }
        }

        public void ResetUI()
        {
            this._resultsGridView.DataSource = null;
        }

        public void SetQueries(DesignerDataConnection dataConnection, SqlDataSourceQuery selectQuery, SqlDataSourceQuery insertQuery, SqlDataSourceQuery updateQuery, SqlDataSourceQuery deleteQuery)
        {
            this._dataConnection = dataConnection;
            this._selectQuery = selectQuery;
            this._insertQuery = insertQuery;
            this._updateQuery = updateQuery;
            this._deleteQuery = deleteQuery;
            this._previewTextBox.Text = this._selectQuery.Command;
        }
    }
}

