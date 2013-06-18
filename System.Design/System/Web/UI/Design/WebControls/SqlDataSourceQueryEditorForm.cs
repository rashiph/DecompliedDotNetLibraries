namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceQueryEditorForm : DesignerForm
    {
        private System.Windows.Forms.Button _cancelButton;
        private System.Windows.Forms.Label _commandLabel;
        private System.Windows.Forms.TextBox _commandTextBox;
        private SqlDataSourceCommandType _commandType;
        private DesignerDataConnection _dataConnection;
        private IDataEnvironment _dataEnvironment;
        private System.Windows.Forms.Button _inferParametersButton;
        private System.Windows.Forms.Button _okButton;
        private IList _originalParameters;
        private ParameterEditorUserControl _parameterEditorUserControl;
        private System.Windows.Forms.Button _queryBuilderButton;
        private QueryBuilderMode _queryBuilderMode;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;

        public SqlDataSourceQueryEditorForm(IServiceProvider serviceProvider, SqlDataSourceDesigner sqlDataSourceDesigner, string providerName, string connectionString, DataSourceOperation operation, SqlDataSourceCommandType commandType, string command, IList originalParameters) : base(serviceProvider)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this.InitializeComponent();
            this.InitializeUI();
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = "System.Data.SqlClient";
            }
            this._dataConnection = new DesignerDataConnection(string.Empty, providerName, connectionString);
            this._commandType = commandType;
            this._commandTextBox.Text = command;
            this._originalParameters = originalParameters;
            string str = Enum.GetName(typeof(DataSourceOperation), operation).ToUpperInvariant();
            this._commandLabel.Text = System.Design.SR.GetString("SqlDataSourceQueryEditorForm_CommandLabel", new object[] { str });
            ArrayList dest = new ArrayList(originalParameters.Count);
            sqlDataSourceDesigner.CopyList(originalParameters, dest);
            this._parameterEditorUserControl.AddParameters((Parameter[]) dest.ToArray(typeof(Parameter)));
            this._commandTextBox.Select(0, 0);
            switch (operation)
            {
                case DataSourceOperation.Delete:
                    this._queryBuilderMode = QueryBuilderMode.Delete;
                    return;

                case DataSourceOperation.Insert:
                    this._queryBuilderMode = QueryBuilderMode.Insert;
                    return;

                case DataSourceOperation.Select:
                    this._queryBuilderMode = QueryBuilderMode.Select;
                    return;

                case DataSourceOperation.Update:
                    this._queryBuilderMode = QueryBuilderMode.Update;
                    return;
            }
        }

        private void InitializeComponent()
        {
            this._okButton = new System.Windows.Forms.Button();
            this._cancelButton = new System.Windows.Forms.Button();
            this._inferParametersButton = new System.Windows.Forms.Button();
            this._queryBuilderButton = new System.Windows.Forms.Button();
            this._commandLabel = new System.Windows.Forms.Label();
            this._commandTextBox = new System.Windows.Forms.TextBox();
            this._parameterEditorUserControl = new ParameterEditorUserControl(base.ServiceProvider, (SqlDataSource) this._sqlDataSourceDesigner.Component);
            base.SuspendLayout();
            this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._okButton.Location = new Point(0x179, 0x17b);
            this._okButton.TabIndex = 150;
            this._okButton.Click += new EventHandler(this.OnOkButtonClick);
            this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._cancelButton.Location = new Point(0x1c9, 0x17b);
            this._cancelButton.TabIndex = 160;
            this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
            this._commandLabel.Location = new Point(12, 12);
            this._commandLabel.Size = new Size(200, 0x10);
            this._commandLabel.TabIndex = 10;
            this._commandTextBox.AcceptsReturn = true;
            this._commandTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._commandTextBox.Location = new Point(12, 30);
            this._commandTextBox.Multiline = true;
            this._commandTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._commandTextBox.Size = new Size(520, 0x4e);
            this._commandTextBox.TabIndex = 20;
            this._inferParametersButton.AutoSize = true;
            this._inferParametersButton.Location = new Point(12, 0x70);
            this._inferParametersButton.Size = new Size(0x80, 0x17);
            this._inferParametersButton.TabIndex = 30;
            this._inferParametersButton.Click += new EventHandler(this.OnInferParametersButtonClick);
            this._queryBuilderButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            this._queryBuilderButton.AutoSize = true;
            this._queryBuilderButton.Location = new Point(0x194, 0x70);
            this._queryBuilderButton.Size = new Size(0x80, 0x17);
            this._queryBuilderButton.TabIndex = 40;
            this._queryBuilderButton.Click += new EventHandler(this.OnQueryBuilderButtonClick);
            this._parameterEditorUserControl.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._parameterEditorUserControl.Location = new Point(12, 0x90);
            this._parameterEditorUserControl.Size = new Size(520, 0xe0);
            this._parameterEditorUserControl.TabIndex = 50;
            base.AcceptButton = this._okButton;
            base.CancelButton = this._cancelButton;
            base.ClientSize = new Size(0x220, 410);
            base.Controls.Add(this._queryBuilderButton);
            base.Controls.Add(this._inferParametersButton);
            base.Controls.Add(this._commandTextBox);
            base.Controls.Add(this._commandLabel);
            base.Controls.Add(this._cancelButton);
            base.Controls.Add(this._okButton);
            base.Controls.Add(this._parameterEditorUserControl);
            this.MinimumSize = new Size(0x1e8, 440);
            base.InitializeForm();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._okButton.Text = System.Design.SR.GetString("OK");
            this._cancelButton.Text = System.Design.SR.GetString("Cancel");
            this._inferParametersButton.Text = System.Design.SR.GetString("SqlDataSourceQueryEditorForm_InferParametersButton");
            this._queryBuilderButton.Text = System.Design.SR.GetString("SqlDataSourceQueryEditorForm_QueryBuilderButton");
            this.Text = System.Design.SR.GetString("SqlDataSourceQueryEditorForm_Caption");
            this._dataEnvironment = (IDataEnvironment) base.ServiceProvider.GetService(typeof(IDataEnvironment));
            this._queryBuilderButton.Enabled = this._dataEnvironment != null;
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void OnInferParametersButtonClick(object sender, EventArgs e)
        {
            if (this._commandTextBox.Text.Trim().Length == 0)
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("SqlDataSourceQueryEditorForm_InferNeedsCommand"));
            }
            else
            {
                Parameter[] parameterArray = this._sqlDataSourceDesigner.InferParameterNames(this._dataConnection, this._commandTextBox.Text, this._commandType);
                if (parameterArray != null)
                {
                    Parameter[] parameters = this._parameterEditorUserControl.GetParameters();
                    StringCollection strings = new StringCollection();
                    foreach (Parameter parameter in parameters)
                    {
                        strings.Add(parameter.Name);
                    }
                    bool flag = true;
                    try
                    {
                        flag = SqlDataSourceDesigner.SupportsNamedParameters(SqlDataSourceDesigner.GetDbProviderFactory(this._dataConnection.ProviderName));
                    }
                    catch
                    {
                    }
                    if (flag)
                    {
                        List<Parameter> list = new List<Parameter>();
                        foreach (Parameter parameter2 in parameterArray)
                        {
                            if (!strings.Contains(parameter2.Name))
                            {
                                list.Add(parameter2);
                            }
                            else
                            {
                                strings.Remove(parameter2.Name);
                            }
                        }
                        this._parameterEditorUserControl.AddParameters(list.ToArray());
                    }
                    else
                    {
                        List<Parameter> list2 = new List<Parameter>();
                        foreach (Parameter parameter3 in parameterArray)
                        {
                            list2.Add(parameter3);
                        }
                        foreach (Parameter parameter4 in parameters)
                        {
                            Parameter item = null;
                            foreach (Parameter parameter6 in list2)
                            {
                                if (parameter6.Direction == parameter4.Direction)
                                {
                                    item = parameter6;
                                    break;
                                }
                            }
                            if (item != null)
                            {
                                list2.Remove(item);
                            }
                        }
                        this._parameterEditorUserControl.AddParameters(list2.ToArray());
                    }
                }
            }
        }

        private void OnOkButtonClick(object sender, EventArgs e)
        {
            this._sqlDataSourceDesigner.CopyList(this._parameterEditorUserControl.GetParameters(), this._originalParameters);
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        private void OnQueryBuilderButtonClick(object sender, EventArgs e)
        {
            if ((this._dataConnection.ConnectionString == null) || (this._dataConnection.ConnectionString.Trim().Length == 0))
            {
                UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("SqlDataSourceQueryEditorForm_QueryBuilderNeedsConnectionString"));
            }
            else
            {
                string str = this._dataEnvironment.BuildQuery(this, this._dataConnection, this._queryBuilderMode, this._commandTextBox.Text);
                if ((str != null) && (str.Length > 0))
                {
                    this._commandTextBox.Text = str;
                }
                this._commandTextBox.Focus();
                this._commandTextBox.Select(0, 0);
            }
        }

        public string Command
        {
            get
            {
                return this._commandTextBox.Text;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.SqlDataSource.QueryEditor";
            }
        }
    }
}

