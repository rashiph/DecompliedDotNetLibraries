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

    internal class SqlDataSourceCustomCommandPanel : WizardPanel
    {
        private TabControl _commandsTabControl;
        private DesignerDataConnection _dataConnection;
        private SqlDataSourceCustomCommandEditor _deleteCommandEditor;
        private TabPage _deleteTabPage;
        private System.Windows.Forms.Label _helpLabel;
        private SqlDataSourceCustomCommandEditor _insertCommandEditor;
        private TabPage _insertTabPage;
        private SqlDataSourceCustomCommandEditor _selectCommandEditor;
        private TabPage _selectTabPage;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private SqlDataSourceCustomCommandEditor _updateCommandEditor;
        private TabPage _updateTabPage;

        public SqlDataSourceCustomCommandPanel(SqlDataSourceDesigner sqlDataSourceDesigner)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
            this._selectCommandEditor.SetCommandData(this._sqlDataSourceDesigner, QueryBuilderMode.Select);
            this._insertCommandEditor.SetCommandData(this._sqlDataSourceDesigner, QueryBuilderMode.Insert);
            this._updateCommandEditor.SetCommandData(this._sqlDataSourceDesigner, QueryBuilderMode.Update);
            this._deleteCommandEditor.SetCommandData(this._sqlDataSourceDesigner, QueryBuilderMode.Delete);
        }

        private void InitializeComponent()
        {
            this._commandsTabControl = new TabControl();
            this._selectTabPage = new TabPage();
            this._selectCommandEditor = new SqlDataSourceCustomCommandEditor();
            this._updateTabPage = new TabPage();
            this._updateCommandEditor = new SqlDataSourceCustomCommandEditor();
            this._insertTabPage = new TabPage();
            this._insertCommandEditor = new SqlDataSourceCustomCommandEditor();
            this._deleteTabPage = new TabPage();
            this._deleteCommandEditor = new SqlDataSourceCustomCommandEditor();
            this._helpLabel = new System.Windows.Forms.Label();
            this._commandsTabControl.SuspendLayout();
            this._selectTabPage.SuspendLayout();
            this._updateTabPage.SuspendLayout();
            this._insertTabPage.SuspendLayout();
            this._deleteTabPage.SuspendLayout();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x220, 0x10);
            this._helpLabel.TabIndex = 10;
            this._commandsTabControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._commandsTabControl.Controls.Add(this._selectTabPage);
            this._commandsTabControl.Controls.Add(this._updateTabPage);
            this._commandsTabControl.Controls.Add(this._insertTabPage);
            this._commandsTabControl.Controls.Add(this._deleteTabPage);
            this._commandsTabControl.Location = new Point(0, 0x16);
            this._commandsTabControl.Name = "_commandsTabControl";
            this._commandsTabControl.SelectedIndex = 0;
            this._commandsTabControl.ShowToolTips = true;
            this._commandsTabControl.Size = new Size(0x220, 0xfc);
            this._commandsTabControl.TabIndex = 20;
            this._selectTabPage.Controls.Add(this._selectCommandEditor);
            this._selectTabPage.Location = new Point(4, 0x16);
            this._selectTabPage.Name = "_selectTabPage";
            this._selectTabPage.Size = new Size(0x218, 0xe2);
            this._selectTabPage.TabIndex = 10;
            this._selectTabPage.Text = "SELECT";
            this._selectCommandEditor.Dock = DockStyle.Fill;
            this._selectCommandEditor.Location = new Point(0, 0);
            this._selectCommandEditor.Name = "_selectCommandEditor";
            this._selectCommandEditor.TabIndex = 10;
            this._selectCommandEditor.CommandChanged += new EventHandler(this.OnSelectCommandChanged);
            this._updateTabPage.Controls.Add(this._updateCommandEditor);
            this._updateTabPage.Location = new Point(4, 0x16);
            this._updateTabPage.Name = "_updateTabPage";
            this._updateTabPage.Size = new Size(0x218, 0xe2);
            this._updateTabPage.TabIndex = 20;
            this._updateTabPage.Text = "UPDATE";
            this._updateCommandEditor.Dock = DockStyle.Fill;
            this._updateCommandEditor.Location = new Point(0, 0);
            this._updateCommandEditor.Name = "_updateCommandEditor";
            this._updateCommandEditor.TabIndex = 10;
            this._insertTabPage.Controls.Add(this._insertCommandEditor);
            this._insertTabPage.Location = new Point(4, 0x16);
            this._insertTabPage.Name = "_insertTabPage";
            this._insertTabPage.Size = new Size(0x218, 0xe2);
            this._insertTabPage.TabIndex = 30;
            this._insertTabPage.Text = "INSERT";
            this._insertCommandEditor.Dock = DockStyle.Fill;
            this._insertCommandEditor.Location = new Point(0, 0);
            this._insertCommandEditor.Name = "_insertCommandEditor";
            this._insertCommandEditor.TabIndex = 10;
            this._deleteTabPage.Controls.Add(this._deleteCommandEditor);
            this._deleteTabPage.Location = new Point(4, 0x16);
            this._deleteTabPage.Name = "_deleteTabPage";
            this._deleteTabPage.Size = new Size(0x20a, 0xe2);
            this._deleteTabPage.TabIndex = 40;
            this._deleteTabPage.Text = "DELETE";
            this._deleteCommandEditor.Dock = DockStyle.Fill;
            this._deleteCommandEditor.Location = new Point(0, 0);
            this._deleteCommandEditor.Name = "_deleteCommandEditor";
            this._deleteCommandEditor.TabIndex = 10;
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._commandsTabControl);
            base.Name = "SqlDataSourceCustomCommandPanel";
            base.Size = new Size(0x220, 0x112);
            this._commandsTabControl.ResumeLayout(false);
            this._selectTabPage.ResumeLayout(false);
            this._updateTabPage.ResumeLayout(false);
            this._insertTabPage.ResumeLayout(false);
            this._deleteTabPage.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceCustomCommandPanel_HelpLabel");
            base.Caption = System.Design.SR.GetString("SqlDataSourceCustomCommandPanel_PanelCaption");
        }

        public override bool OnNext()
        {
            SqlDataSourceQuery selectQuery = this._selectCommandEditor.GetQuery();
            SqlDataSourceQuery query = this._insertCommandEditor.GetQuery();
            SqlDataSourceQuery updateQuery = this._updateCommandEditor.GetQuery();
            SqlDataSourceQuery deleteQuery = this._deleteCommandEditor.GetQuery();
            if (((selectQuery == null) || (query == null)) || ((updateQuery == null) || (deleteQuery == null)))
            {
                return false;
            }
            int num = 0;
            foreach (Parameter parameter in selectQuery.Parameters)
            {
                if ((parameter.Direction == ParameterDirection.Input) || (parameter.Direction == ParameterDirection.InputOutput))
                {
                    num++;
                }
            }
            if (num == 0)
            {
                SqlDataSourceSummaryPanel summaryPanel = base.NextPanel as SqlDataSourceSummaryPanel;
                if (summaryPanel == null)
                {
                    summaryPanel = ((SqlDataSourceWizardForm) base.ParentWizard).GetSummaryPanel();
                    base.NextPanel = summaryPanel;
                }
                summaryPanel.SetQueries(this._dataConnection, selectQuery, query, updateQuery, deleteQuery);
                return true;
            }
            SqlDataSourceConfigureParametersPanel nextPanel = base.NextPanel as SqlDataSourceConfigureParametersPanel;
            if (nextPanel == null)
            {
                nextPanel = ((SqlDataSourceWizardForm) base.ParentWizard).GetConfigureParametersPanel();
                base.NextPanel = nextPanel;
                SqlDataSource component = (SqlDataSource) this._sqlDataSourceDesigner.Component;
                Parameter[] selectParameters = new Parameter[component.SelectParameters.Count];
                for (int i = 0; i < component.SelectParameters.Count; i++)
                {
                    Parameter original = component.SelectParameters[i];
                    Parameter clone = (Parameter) ((ICloneable) original).Clone();
                    this._sqlDataSourceDesigner.RegisterClone(original, clone);
                    selectParameters[i] = clone;
                }
                nextPanel.InitializeParameters(selectParameters);
            }
            nextPanel.SetQueries(this._dataConnection, selectQuery, query, updateQuery, deleteQuery);
            return true;
        }

        public override void OnPrevious()
        {
        }

        private void OnSelectCommandChanged(object sender, EventArgs e)
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

        public void ResetUI()
        {
        }

        public void SetQueries(DesignerDataConnection dataConnection, SqlDataSourceQuery selectQuery, SqlDataSourceQuery insertQuery, SqlDataSourceQuery updateQuery, SqlDataSourceQuery deleteQuery)
        {
            DesignerDataConnection connection = dataConnection;
            if (!SqlDataSourceDesigner.ConnectionsEqual(this._dataConnection, connection))
            {
                this._dataConnection = connection;
                Cursor current = Cursor.Current;
                ArrayList storedProcedures = null;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    IDataEnvironment service = (IDataEnvironment) this._sqlDataSourceDesigner.Component.Site.GetService(typeof(IDataEnvironment));
                    if (service != null)
                    {
                        IDesignerDataSchema connectionSchema = service.GetConnectionSchema(this._dataConnection);
                        if ((connectionSchema != null) && connectionSchema.SupportsSchemaClass(DesignerDataSchemaClass.StoredProcedures))
                        {
                            ICollection schemaItems = connectionSchema.GetSchemaItems(DesignerDataSchemaClass.StoredProcedures);
                            if ((schemaItems != null) && (schemaItems.Count > 0))
                            {
                                storedProcedures = new ArrayList();
                                foreach (DesignerDataStoredProcedure procedure in schemaItems)
                                {
                                    if (!procedure.Name.ToLowerInvariant().StartsWith("AspNet_".ToLowerInvariant(), StringComparison.Ordinal))
                                    {
                                        storedProcedures.Add(procedure);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("SqlDataSourceConnectionPanel_CouldNotGetConnectionSchema"));
                }
                finally
                {
                    Cursor.Current = current;
                }
                this._selectCommandEditor.SetConnection(this._dataConnection);
                this._selectCommandEditor.SetStoredProcedures(storedProcedures);
                this._insertCommandEditor.SetConnection(this._dataConnection);
                this._insertCommandEditor.SetStoredProcedures(storedProcedures);
                this._updateCommandEditor.SetConnection(this._dataConnection);
                this._updateCommandEditor.SetStoredProcedures(storedProcedures);
                this._deleteCommandEditor.SetConnection(this._dataConnection);
                this._deleteCommandEditor.SetStoredProcedures(storedProcedures);
                this._selectCommandEditor.SetQuery(selectQuery);
                this._insertCommandEditor.SetQuery(insertQuery);
                this._updateCommandEditor.SetQuery(updateQuery);
                this._deleteCommandEditor.SetQuery(deleteQuery);
            }
        }

        private void UpdateEnabledState()
        {
            bool hasQuery = this._selectCommandEditor.HasQuery;
            base.ParentWizard.NextButton.Enabled = hasQuery;
            base.ParentWizard.FinishButton.Enabled = false;
        }
    }
}

