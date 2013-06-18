namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Data;
    using System.Data.Common;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceSaveConfiguredConnectionPanel : WizardPanel
    {
        private DesignerDataConnection _dataConnection;
        private IDataEnvironment _dataEnvironment;
        private System.Windows.Forms.Label _helpLabel;
        private System.Windows.Forms.Label _nameHelpLabel;
        private System.Windows.Forms.TextBox _nameTextBox;
        private System.Windows.Forms.CheckBox _saveCheckBox;
        private System.Windows.Forms.Label _saveLabel;
        private SqlDataSource _sqlDataSource;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        internal const string ConnectionStringExpressionConnectionSuffix = "ConnectionString";
        internal const string ConnectionStringExpressionPrefix = "ConnectionStrings";
        internal const string ConnectionStringExpressionProviderSuffix = "ProviderName";

        public SqlDataSourceSaveConfiguredConnectionPanel(SqlDataSourceDesigner sqlDataSourceDesigner, IDataEnvironment dataEnvironment)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this._sqlDataSource = (SqlDataSource) this._sqlDataSourceDesigner.Component;
            this._dataEnvironment = dataEnvironment;
            this.InitializeComponent();
            this.InitializeUI();
        }

        private void CheckShouldAllowNext()
        {
            if (base.ParentWizard != null)
            {
                base.ParentWizard.NextButton.Enabled = !this._saveCheckBox.Checked || (this._nameTextBox.Text.Trim().Length > 0);
            }
        }

        private string CreateDefaultConnectionName()
        {
            ICollection connections = this._dataEnvironment.Connections;
            StringDictionary dictionary = new StringDictionary();
            if (connections != null)
            {
                foreach (DesignerDataConnection connection in connections)
                {
                    if ((connection != null) && connection.IsConfigured)
                    {
                        dictionary.Add(connection.Name, null);
                    }
                }
            }
            int num = 2;
            string connectionName = ConnectionStringHelper.GetConnectionName(this._dataConnection);
            string key = connectionName;
            while (dictionary.ContainsKey(key))
            {
                key = connectionName + num.ToString(CultureInfo.InvariantCulture);
                num++;
            }
            return key;
        }

        private void InitializeComponent()
        {
            this._saveLabel = new System.Windows.Forms.Label();
            this._saveCheckBox = new System.Windows.Forms.CheckBox();
            this._nameTextBox = new System.Windows.Forms.TextBox();
            this._helpLabel = new System.Windows.Forms.Label();
            this._nameHelpLabel = new System.Windows.Forms.Label();
            base.SuspendLayout();
            this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._helpLabel.Location = new Point(0, 0);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new Size(0x220, 0x38);
            this._helpLabel.TabIndex = 10;
            this._saveLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._saveLabel.Location = new Point(0, 0x4b);
            this._saveLabel.Name = "_saveLabel";
            this._saveLabel.Size = new Size(0x220, 0x10);
            this._saveLabel.TabIndex = 20;
            this._saveCheckBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._saveCheckBox.Location = new Point(0, 0x5d);
            this._saveCheckBox.Name = "_saveCheckBox";
            this._saveCheckBox.Size = new Size(0x220, 0x12);
            this._saveCheckBox.TabIndex = 30;
            this._saveCheckBox.CheckedChanged += new EventHandler(this.OnSaveCheckBoxCheckedChanged);
            this._nameHelpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._nameHelpLabel.Location = new Point(0, 0);
            this._nameHelpLabel.Name = "_nameHelpLabel";
            this._nameHelpLabel.Size = new Size(0, 0);
            this._nameHelpLabel.TabIndex = 40;
            this._nameTextBox.Location = new Point(0x13, 0x71);
            this._nameTextBox.Name = "_nameTextBox";
            this._nameTextBox.Size = new Size(300, 20);
            this._nameTextBox.TabIndex = 50;
            this._nameTextBox.TextChanged += new EventHandler(this.OnNameTextBoxTextChanged);
            base.Controls.Add(this._nameHelpLabel);
            base.Controls.Add(this._saveCheckBox);
            base.Controls.Add(this._saveLabel);
            base.Controls.Add(this._nameTextBox);
            base.Controls.Add(this._helpLabel);
            base.Name = "SqlDataSourceSaveConfiguredConnectionPanel";
            base.Size = new Size(0x220, 0x112);
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._helpLabel.Text = System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_HelpLabel");
            this._saveLabel.Text = System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_SaveLabel");
            this._saveCheckBox.Text = System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_SaveCheckBox");
            this._nameHelpLabel.Text = System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_NameTextBoxDescription");
            base.Caption = System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_PanelCaption");
        }

        protected internal override void OnComplete()
        {
            DesignerDataConnection dataConnection = this._dataConnection;
            if (this._saveCheckBox.Checked)
            {
                try
                {
                    dataConnection = this._dataEnvironment.ConfigureConnection(this, this._dataConnection, this._nameTextBox.Text.Trim());
                }
                catch (Exception exception)
                {
                    if (exception != CheckoutException.Canceled)
                    {
                        UIServiceHelper.ShowError(base.ServiceProvider, exception, System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_CouldNotSaveConnection"));
                    }
                }
            }
            PersistConnectionSettings(this._sqlDataSource, this._sqlDataSourceDesigner, dataConnection);
            this._sqlDataSourceDesigner.SaveConfiguredConnectionState = dataConnection.IsConfigured;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFonts();
        }

        private void OnNameTextBoxTextChanged(object sender, EventArgs e)
        {
            this.CheckShouldAllowNext();
        }

        public override bool OnNext()
        {
            if (this._saveCheckBox.Checked)
            {
                ICollection connections = this._dataEnvironment.Connections;
                StringDictionary dictionary = new StringDictionary();
                foreach (DesignerDataConnection connection in connections)
                {
                    if (connection.IsConfigured)
                    {
                        dictionary.Add(connection.Name, null);
                    }
                }
                if (dictionary.ContainsKey(this._nameTextBox.Text))
                {
                    UIServiceHelper.ShowError(base.ServiceProvider, System.Design.SR.GetString("SqlDataSourceSaveConfiguredConnectionPanel_DuplicateName", new object[] { this._nameTextBox.Text }));
                    this._nameTextBox.Focus();
                    return false;
                }
            }
            WizardPanel panel = SqlDataSourceConnectionPanel.CreateCommandPanel((SqlDataSourceWizardForm) base.ParentWizard, this._dataConnection, base.NextPanel);
            if (panel == null)
            {
                return false;
            }
            base.NextPanel = panel;
            return true;
        }

        private void OnSaveCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            this._nameTextBox.Enabled = this._saveCheckBox.Checked;
            this.CheckShouldAllowNext();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            base.ParentWizard.FinishButton.Enabled = false;
            if (base.Visible)
            {
                this.CheckShouldAllowNext();
            }
            else
            {
                base.ParentWizard.NextButton.Enabled = true;
            }
        }

        internal static void PersistConnectionSettings(SqlDataSource sqlDataSource, SqlDataSourceDesigner sqlDataSourceDesigner, DesignerDataConnection dataConnection)
        {
            if (dataConnection.IsConfigured)
            {
                ExpressionBindingCollection expressions = sqlDataSource.Expressions;
                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(sqlDataSource)["ProviderName"];
                descriptor.ResetValue(sqlDataSource);
                if (dataConnection.ProviderName != "System.Data.SqlClient")
                {
                    expressions.Add(new ExpressionBinding(descriptor.Name, descriptor.PropertyType, "ConnectionStrings", dataConnection.Name + ".ProviderName"));
                }
                descriptor = TypeDescriptor.GetProperties(sqlDataSource)["ConnectionString"];
                descriptor.ResetValue(sqlDataSource);
                expressions.Add(new ExpressionBinding(descriptor.Name, descriptor.PropertyType, "ConnectionStrings", dataConnection.Name));
            }
            else
            {
                PropertyDescriptor descriptor2;
                if (sqlDataSource.ProviderName != dataConnection.ProviderName)
                {
                    descriptor2 = TypeDescriptor.GetProperties(sqlDataSource)["ProviderName"];
                    descriptor2.ResetValue(sqlDataSource);
                    descriptor2.SetValue(sqlDataSource, dataConnection.ProviderName);
                }
                if (sqlDataSource.ConnectionString != dataConnection.ConnectionString)
                {
                    descriptor2 = TypeDescriptor.GetProperties(sqlDataSource)["ConnectionString"];
                    descriptor2.ResetValue(sqlDataSource);
                    descriptor2.SetValue(sqlDataSource, dataConnection.ConnectionString);
                }
            }
        }

        public void ResetUI()
        {
            this.UpdateFonts();
            this._saveCheckBox.Checked = true;
            this._nameTextBox.Text = string.Empty;
        }

        public void SetConnectionInfo(DesignerDataConnection dataConnection)
        {
            this._dataConnection = dataConnection;
            this.ResetUI();
            bool saveConfiguredConnectionState = this._sqlDataSourceDesigner.SaveConfiguredConnectionState;
            DesignerDataConnection connection = new DesignerDataConnection(string.Empty, this._sqlDataSourceDesigner.ProviderName, this._sqlDataSourceDesigner.ConnectionString);
            if (SqlDataSourceDesigner.ConnectionsEqual(dataConnection, connection))
            {
                if (!saveConfiguredConnectionState)
                {
                    this._saveCheckBox.Checked = false;
                }
                if (this._nameTextBox.Text.Length == 0)
                {
                    this._nameTextBox.Text = this.CreateDefaultConnectionName();
                }
            }
            else
            {
                this._nameTextBox.Text = this.CreateDefaultConnectionName();
            }
        }

        private void UpdateFonts()
        {
            Font font = new Font(this.Font, FontStyle.Bold);
            this._saveLabel.Font = font;
        }

        internal DesignerDataConnection CurrentConnection
        {
            get
            {
                return this._dataConnection;
            }
        }

        private static class ConnectionStringHelper
        {
            private const string DefaultConnectionName = "ConnectionString";
            private const string JetOleDbProviderName = "MICROSOFT.JET";
            private const string OleDbProviderName = "System.Data.OleDb";
            private const string SqlClientProviderName = "System.Data.SqlClient";

            public static string GetConnectionName(DesignerDataConnection connection)
            {
                DbConnectionStringBuilder connectionStringBuilder = SqlDataSourceDesigner.GetDbProviderFactory(connection.ProviderName).CreateConnectionStringBuilder();
                if (connectionStringBuilder == null)
                {
                    connectionStringBuilder = new DbConnectionStringBuilder();
                }
                string str = null;
                try
                {
                    object obj2;
                    connectionStringBuilder.ConnectionString = connection.ConnectionString;
                    if (IsLocalDbFileConnectionString(connection.ProviderName, connectionStringBuilder))
                    {
                        string filePathKey = GetFilePathKey(connection.ProviderName, connectionStringBuilder);
                        if (!string.IsNullOrEmpty(filePathKey))
                        {
                            string str3 = connectionStringBuilder[filePathKey] as string;
                            if (!string.IsNullOrEmpty(str3))
                            {
                                str = Path.GetFileNameWithoutExtension(str3) + "ConnectionString";
                            }
                        }
                    }
                    if ((str == null) && connectionStringBuilder.TryGetValue("Database", out obj2))
                    {
                        string s = obj2 as string;
                        if (!StringIsEmpty(s))
                        {
                            str = s + "ConnectionString";
                        }
                    }
                }
                catch
                {
                }
                if (str == null)
                {
                    str = "ConnectionString";
                }
                return str.Trim();
            }

            private static string GetFilePathKey(string providerName, DbConnectionStringBuilder connectionStringBuilder)
            {
                if (IsAccessConnectionString(providerName, connectionStringBuilder))
                {
                    return "Data Source";
                }
                if (IsSqlLocalConnectionString(providerName, connectionStringBuilder))
                {
                    return "AttachDbFileName";
                }
                return null;
            }

            private static bool IsAccessConnectionString(string providerName, DbConnectionStringBuilder connectionStringBuilder)
            {
                if (string.Equals(providerName, "System.Data.OleDb", StringComparison.OrdinalIgnoreCase))
                {
                    string str = connectionStringBuilder["provider"] as string;
                    if (!string.IsNullOrEmpty(str) && str.ToUpperInvariant().StartsWith("MICROSOFT.JET", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
                return false;
            }

            private static bool IsLocalDbFileConnectionString(string providerName, DbConnectionStringBuilder connectionStringBuilder)
            {
                if (!IsSqlLocalConnectionString(providerName, connectionStringBuilder) && !IsAccessConnectionString(providerName, connectionStringBuilder))
                {
                    return false;
                }
                return true;
            }

            private static bool IsSqlLocalConnectionString(string providerName, DbConnectionStringBuilder connectionStringBuilder)
            {
                return (string.Equals(providerName, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(connectionStringBuilder["AttachDbFileName"] as string));
            }

            private static bool StringIsEmpty(string s)
            {
                return (string.IsNullOrEmpty(s) || (s.Trim().Length == 0));
            }
        }
    }
}

