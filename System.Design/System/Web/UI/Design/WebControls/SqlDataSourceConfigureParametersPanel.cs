namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceConfigureParametersPanel : WizardPanel
    {
        private DesignerDataConnection _dataConnection;
        private SqlDataSourceQuery _deleteQuery;
        private System.Windows.Forms.Label _helpLabel;
        private SqlDataSourceQuery _insertQuery;
        private ParameterEditorUserControl _parameterEditorUserControl;
        private System.Windows.Forms.Label _previewLabel;
        private System.Windows.Forms.TextBox _previewTextBox;
        private SqlDataSourceQuery _selectQuery;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private SqlDataSourceQuery _updateQuery;

        public SqlDataSourceConfigureParametersPanel(SqlDataSourceDesigner sqlDataSourceDesigner)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
            this._parameterEditorUserControl.SetAllowCollectionChanges(false);
        }

        private static Parameter CreateMergedParameter(Parameter parameter, List<Parameter> unusedOldParameters)
        {
            Parameter item = null;
            foreach (Parameter parameter3 in unusedOldParameters)
            {
                if (ParametersMatch(parameter, parameter3))
                {
                    item = parameter3;
                    break;
                }
            }
            if (item != null)
            {
                unusedOldParameters.Remove(item);
                return item;
            }
            return parameter;
        }

        private void InitializeComponent()
        {
            this._previewTextBox = new System.Windows.Forms.TextBox();
            this._previewLabel = new System.Windows.Forms.Label();
            this._helpLabel = new System.Windows.Forms.Label();
            this._parameterEditorUserControl = new ParameterEditorUserControl(this._sqlDataSourceDesigner.Component.Site, (SqlDataSource) this._sqlDataSourceDesigner.Component);
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x220, 0x20);
            this._helpLabel.TabIndex = 10;
            this._parameterEditorUserControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._parameterEditorUserControl.Location = new Point(0, 0x26);
            this._parameterEditorUserControl.Name = "_parameterEditorUserControl";
            this._parameterEditorUserControl.Size = new Size(0x220, 0x98);
            this._parameterEditorUserControl.TabIndex = 20;
            this._parameterEditorUserControl.ParametersChanged += new EventHandler(this.OnParameterEditorUserControlParametersChanged);
            this._previewLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._previewLabel.Location = new Point(0, 0xd6);
            this._previewLabel.Name = "_previewLabel";
            this._previewLabel.Size = new Size(0x220, 0x10);
            this._previewLabel.TabIndex = 30;
            this._previewTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._previewTextBox.BackColor = SystemColors.Control;
            this._previewTextBox.Location = new Point(0, 0xe8);
            this._previewTextBox.Multiline = true;
            this._previewTextBox.Name = "_previewTextBox";
            this._previewTextBox.ReadOnly = true;
            this._previewTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._previewTextBox.Size = new Size(0x220, 0x2a);
            this._previewTextBox.TabIndex = 40;
            this._previewTextBox.Text = "";
            base.Controls.Add(this._parameterEditorUserControl);
            base.Controls.Add(this._helpLabel);
            base.Controls.Add(this._previewLabel);
            base.Controls.Add(this._previewTextBox);
            base.Name = "SqlDataSourceConfigureParametersPanel";
            base.Size = new Size(0x220, 0x112);
            base.ResumeLayout(false);
        }

        public void InitializeParameters(Parameter[] selectParameters)
        {
            this._parameterEditorUserControl.AddParameters(selectParameters);
        }

        private void InitializeUI()
        {
            base.Caption = System.Design.SR.GetString("SqlDataSourceConfigureParametersPanel_PanelCaption");
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceConfigureParametersPanel_HelpLabel");
            this._previewLabel.Text = System.Design.SR.GetString("SqlDataSource_General_PreviewLabel");
        }

        private static Parameter[] MergeParameters(Parameter[] oldParameters, Parameter[] newParameters)
        {
            Parameter[] parameterArray = new Parameter[newParameters.Length];
            List<Parameter> unusedOldParameters = new List<Parameter>();
            foreach (Parameter parameter in oldParameters)
            {
                unusedOldParameters.Add(parameter);
            }
            for (int i = 0; i < newParameters.Length; i++)
            {
                parameterArray[i] = CreateMergedParameter(newParameters[i], unusedOldParameters);
            }
            return parameterArray;
        }

        public override bool OnNext()
        {
            SqlDataSourceQuery selectQuery = new SqlDataSourceQuery(this._selectQuery.Command, this._selectQuery.CommandType, this._parameterEditorUserControl.GetParameters());
            SqlDataSourceSummaryPanel nextPanel = base.NextPanel as SqlDataSourceSummaryPanel;
            if (nextPanel == null)
            {
                nextPanel = ((SqlDataSourceWizardForm) base.ParentWizard).GetSummaryPanel();
                base.NextPanel = nextPanel;
            }
            nextPanel.SetQueries(this._dataConnection, selectQuery, this._insertQuery, this._updateQuery, this._deleteQuery);
            return true;
        }

        private void OnParameterEditorUserControlParametersChanged(object sender, EventArgs e)
        {
            this.UpdateUI();
        }

        public override void OnPrevious()
        {
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                this.UpdateUI();
                base.ParentWizard.FinishButton.Enabled = false;
            }
        }

        private static bool ParametersMatch(Parameter parameter1, Parameter parameter2)
        {
            if (!string.Equals(parameter1.Name, parameter2.Name, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (parameter1.Direction != parameter2.Direction)
            {
                return false;
            }
            if (parameter1.DbType != parameter2.DbType)
            {
                return false;
            }
            return ((((parameter1.Type == TypeCode.Object) || (parameter1.Type == TypeCode.Empty)) && ((parameter2.Type == TypeCode.Object) || (parameter2.Type == TypeCode.Empty))) || (parameter1.Type == parameter2.Type));
        }

        public void ResetUI()
        {
            this._parameterEditorUserControl.ClearParameters();
        }

        public void SetQueries(DesignerDataConnection dataConnection, SqlDataSourceQuery selectQuery, SqlDataSourceQuery insertQuery, SqlDataSourceQuery updateQuery, SqlDataSourceQuery deleteQuery)
        {
            this._dataConnection = dataConnection;
            this._selectQuery = selectQuery;
            this._insertQuery = insertQuery;
            this._updateQuery = updateQuery;
            this._deleteQuery = deleteQuery;
            this._previewTextBox.Text = this._selectQuery.Command;
            Parameter[] array = new Parameter[this._selectQuery.Parameters.Count];
            this._selectQuery.Parameters.CopyTo(array, 0);
            Parameter[] parameters = MergeParameters(this._parameterEditorUserControl.GetParameters(), array);
            this._parameterEditorUserControl.ClearParameters();
            this._parameterEditorUserControl.AddParameters(parameters);
        }

        private void UpdateUI()
        {
            base.ParentWizard.NextButton.Enabled = this._parameterEditorUserControl.ParametersConfigured;
        }
    }
}

