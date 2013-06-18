namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceDataConnectionChooserPanel : SqlDataSourceConnectionPanel
    {
        private System.Windows.Forms.Label _chooseLabel;
        private AutoSizeComboBox _connectionsComboBox;
        private System.Windows.Forms.Label _connectionStringLabel;
        private System.Windows.Forms.TextBox _connectionStringTextBox;
        private TableLayoutPanel _connectionTableLayoutPanel;
        private IDataEnvironment _dataEnvironment;
        private DetailsButton _detailsButton;
        private System.Windows.Forms.Label _dividerLabel;
        private bool _needsToPersistConnectionInfo;
        private System.Windows.Forms.Button _newConnectionButton;
        private SqlDataSource _sqlDataSource;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;

        public SqlDataSourceDataConnectionChooserPanel(SqlDataSourceDesigner sqlDataSourceDesigner, IDataEnvironment dataEnvironment) : base(sqlDataSourceDesigner)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this._sqlDataSource = (SqlDataSource) this._sqlDataSourceDesigner.Component;
            this._dataEnvironment = dataEnvironment;
            this.InitializeComponent();
            this.InitializeUI();
            DesignerDataConnection conn = new DesignerDataConnection(System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_CustomConnectionName"), this._sqlDataSource.ProviderName, this._sqlDataSource.ConnectionString);
            ExpressionBinding binding = this._sqlDataSource.Expressions["ConnectionString"];
            if ((binding != null) && string.Equals(binding.ExpressionPrefix, "ConnectionStrings", StringComparison.OrdinalIgnoreCase))
            {
                string expression = binding.Expression;
                string str2 = "." + "ConnectionString".ToLowerInvariant();
                if (expression.ToLowerInvariant().EndsWith(str2, StringComparison.Ordinal))
                {
                    expression = expression.Substring(0, expression.Length - str2.Length);
                }
                ICollection connections = this._dataEnvironment.Connections;
                if (connections != null)
                {
                    foreach (DesignerDataConnection connection2 in connections)
                    {
                        if (connection2.IsConfigured && string.Equals(connection2.Name, expression, StringComparison.OrdinalIgnoreCase))
                        {
                            conn = connection2;
                            break;
                        }
                    }
                }
            }
            this.SetConnectionSettings(conn);
        }

        private void CheckShouldAllowNext()
        {
            if (base.ParentWizard != null)
            {
                base.ParentWizard.NextButton.Enabled = this._connectionsComboBox.SelectedItem != null;
            }
        }

        private void InitializeComponent()
        {
            this._chooseLabel = new System.Windows.Forms.Label();
            this._connectionsComboBox = new AutoSizeComboBox();
            this._newConnectionButton = new System.Windows.Forms.Button();
            this._connectionTableLayoutPanel = new TableLayoutPanel();
            this._detailsButton = new DetailsButton();
            this._connectionStringLabel = new System.Windows.Forms.Label();
            this._dividerLabel = new System.Windows.Forms.Label();
            this._connectionStringTextBox = new System.Windows.Forms.TextBox();
            this._connectionTableLayoutPanel.SuspendLayout();
            base.SuspendLayout();
            this._chooseLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._chooseLabel.Location = new Point(0, 0);
            this._chooseLabel.Name = "_chooseLabel";
            this._chooseLabel.Size = new Size(0x220, 0x10);
            this._chooseLabel.TabIndex = 10;
            this._connectionTableLayoutPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._connectionTableLayoutPanel.ColumnCount = 2;
            this._connectionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            this._connectionTableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
            this._connectionTableLayoutPanel.Controls.Add(this._newConnectionButton, 1, 0);
            this._connectionTableLayoutPanel.Controls.Add(this._connectionsComboBox, 0, 0);
            this._connectionTableLayoutPanel.Location = new Point(0, 0x12);
            this._connectionTableLayoutPanel.Name = "_connectionTableLayoutPanel";
            this._connectionTableLayoutPanel.RowCount = 1;
            this._connectionTableLayoutPanel.RowStyles.Add(new RowStyle());
            this._connectionTableLayoutPanel.Size = new Size(0x220, 0x17);
            this._connectionTableLayoutPanel.TabIndex = 20;
            this._connectionsComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._connectionsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._connectionsComboBox.Location = new Point(0, 0);
            this._connectionsComboBox.Margin = new Padding(0, 0, 3, 0);
            this._connectionsComboBox.Name = "_connectionsComboBox";
            this._connectionsComboBox.Size = new Size(0x1cf, 0x15);
            this._connectionsComboBox.Sorted = true;
            this._connectionsComboBox.TabIndex = 10;
            this._connectionsComboBox.SelectedIndexChanged += new EventHandler(this.OnConnectionsComboBoxSelectedIndexChanged);
            this._newConnectionButton.AutoSize = true;
            this._newConnectionButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            this._newConnectionButton.Location = new Point(0x1d5, 0);
            this._newConnectionButton.Margin = new Padding(3, 0, 0, 0);
            this._newConnectionButton.MinimumSize = new Size(0x4b, 0x17);
            this._newConnectionButton.Name = "_newConnectionButton";
            this._newConnectionButton.Padding = new Padding(10, 0, 10, 0);
            this._newConnectionButton.Size = new Size(0x4b, 0x17);
            this._newConnectionButton.TabIndex = 20;
            this._newConnectionButton.Click += new EventHandler(this.OnNewConnectionButtonClick);
            this._detailsButton.Location = new Point(0, 0x33);
            this._detailsButton.Name = "_detailsButton";
            this._detailsButton.Size = new Size(15, 15);
            this._detailsButton.TabIndex = 30;
            this._detailsButton.Click += new EventHandler(this.OnDetailsButtonClick);
            this._connectionStringLabel.AutoSize = true;
            this._connectionStringLabel.Location = new Point(0x15, 0x33);
            this._connectionStringLabel.Name = "_connectionStringLabel";
            this._connectionStringLabel.Padding = new Padding(0, 0, 6, 0);
            this._connectionStringLabel.Size = new Size(0x5c, 0x10);
            this._connectionStringLabel.TabIndex = 40;
            this._dividerLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._dividerLabel.BackColor = SystemColors.ControlDark;
            this._dividerLabel.Location = new Point(30, 0x39);
            this._dividerLabel.Name = "_dividerLabel";
            this._dividerLabel.Size = new Size(0x202, 1);
            this._dividerLabel.TabIndex = 50;
            this._connectionStringTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._connectionStringTextBox.BackColor = SystemColors.Control;
            this._connectionStringTextBox.Location = new Point(0x15, 0x47);
            this._connectionStringTextBox.Multiline = true;
            this._connectionStringTextBox.Name = "_connectionStringTextBox";
            this._connectionStringTextBox.ReadOnly = true;
            this._connectionStringTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._connectionStringTextBox.Size = new Size(0x20b, 90);
            this._connectionStringTextBox.TabIndex = 60;
            this._connectionStringTextBox.Text = "";
            this._connectionStringTextBox.Visible = false;
            base.Controls.Add(this._connectionStringLabel);
            base.Controls.Add(this._dividerLabel);
            base.Controls.Add(this._detailsButton);
            base.Controls.Add(this._connectionStringTextBox);
            base.Controls.Add(this._chooseLabel);
            base.Controls.Add(this._connectionTableLayoutPanel);
            base.Name = "SqlDataSourceDataConnectionChooserPanel";
            base.Size = new Size(0x220, 0x112);
            this._connectionTableLayoutPanel.ResumeLayout(false);
            this._connectionTableLayoutPanel.PerformLayout();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void InitializeUI()
        {
            this._newConnectionButton.Text = System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_NewConnectionButton");
            this._chooseLabel.Text = System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_ChooseLabel");
            this._connectionStringLabel.Text = System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_ConnectionStringLabel");
            this._detailsButton.AccessibleName = System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_DetailsButtonName");
            this._detailsButton.AccessibleDescription = System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_DetailsButtonDesc");
            foreach (DesignerDataConnection connection in this._dataEnvironment.Connections)
            {
                this._connectionsComboBox.Items.Add(new DataConnectionItem(connection));
            }
            this._connectionsComboBox.InvalidateDropDownWidth();
            base.Caption = System.Design.SR.GetString("SqlDataSourceDataConnectionChooserPanel_PanelCaption");
            this.UpdateFonts();
        }

        protected internal override void OnComplete()
        {
            if (this._needsToPersistConnectionInfo)
            {
                SqlDataSourceSaveConfiguredConnectionPanel.PersistConnectionSettings(this._sqlDataSource, this._sqlDataSourceDesigner, this.DataConnection);
            }
        }

        private void OnConnectionsComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.CheckShouldAllowNext();
            DataConnectionItem selectedItem = this._connectionsComboBox.SelectedItem as DataConnectionItem;
            if (selectedItem != null)
            {
                this._connectionStringTextBox.Text = selectedItem.DesignerDataConnection.ConnectionString;
            }
        }

        private void OnDetailsButtonClick(object sender, EventArgs e)
        {
            this._connectionStringTextBox.Visible = !this._connectionStringTextBox.Visible;
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFonts();
        }

        private void OnNewConnectionButtonClick(object sender, EventArgs e)
        {
            DesignerDataConnection conn = this._dataEnvironment.BuildConnection(this, null);
            if (conn != null)
            {
                if (string.Equals(conn.ProviderName, "Microsoft.SqlServerCe.Client.4.0", StringComparison.OrdinalIgnoreCase))
                {
                    conn = new DesignerDataConnection(conn.Name, "System.Data.SqlServerCe.4.0", conn.ConnectionString, conn.IsConfigured);
                }
                if (!this.SelectConnection(conn))
                {
                    DataConnectionItem item = new DataConnectionItem(conn);
                    this._connectionsComboBox.Items.Add(item);
                    this._connectionsComboBox.SelectedItem = item;
                    this._connectionsComboBox.InvalidateDropDownWidth();
                }
            }
        }

        public override bool OnNext()
        {
            if (!base.CheckValidProvider())
            {
                return false;
            }
            DesignerDataConnection dataConnection = this.DataConnection;
            if (!dataConnection.IsConfigured)
            {
                this._needsToPersistConnectionInfo = false;
                SqlDataSourceSaveConfiguredConnectionPanel nextPanel = base.NextPanel as SqlDataSourceSaveConfiguredConnectionPanel;
                if (nextPanel == null)
                {
                    nextPanel = ((SqlDataSourceWizardForm) base.ParentWizard).GetSaveConfiguredConnectionPanel();
                    base.NextPanel = nextPanel;
                }
                if (!SqlDataSourceDesigner.ConnectionsEqual(dataConnection, nextPanel.CurrentConnection))
                {
                    nextPanel.SetConnectionInfo(dataConnection);
                }
                return true;
            }
            this._needsToPersistConnectionInfo = true;
            return base.OnNext();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                this.CheckShouldAllowNext();
            }
        }

        private bool SelectConnection(DesignerDataConnection conn)
        {
            if (conn.IsConfigured)
            {
                foreach (DataConnectionItem item in this._connectionsComboBox.Items)
                {
                    DesignerDataConnection designerDataConnection = item.DesignerDataConnection;
                    if (designerDataConnection.IsConfigured && (designerDataConnection.Name == conn.Name))
                    {
                        this._connectionsComboBox.SelectedItem = item;
                        return true;
                    }
                }
            }
            else
            {
                foreach (DataConnectionItem item2 in this._connectionsComboBox.Items)
                {
                    DesignerDataConnection connection2 = item2.DesignerDataConnection;
                    if (!connection2.IsConfigured && SqlDataSourceDesigner.ConnectionsEqual(connection2, conn))
                    {
                        this._connectionsComboBox.SelectedItem = item2;
                        return true;
                    }
                }
            }
            return false;
        }

        private void SetConnectionSettings(DesignerDataConnection conn)
        {
            bool flag = this.SelectConnection(conn);
            string providerName = conn.ProviderName;
            string connectionString = conn.ConnectionString;
            if (!flag && ((providerName.Length > 0) || (connectionString.Length > 0)))
            {
                if (providerName.Length == 0)
                {
                    providerName = "System.Data.SqlClient";
                }
                this._connectionsComboBox.Items.Insert(0, new DataConnectionItem(new DesignerDataConnection(conn.Name, providerName, connectionString)));
                this._connectionsComboBox.SelectedIndex = 0;
                this._connectionsComboBox.InvalidateDropDownWidth();
            }
            this._connectionStringTextBox.Text = connectionString;
        }

        private void UpdateFonts()
        {
            this._chooseLabel.Font = new Font(this.Font, FontStyle.Bold);
        }

        public override DesignerDataConnection DataConnection
        {
            get
            {
                return ((DataConnectionItem) this._connectionsComboBox.SelectedItem).DesignerDataConnection;
            }
        }

        private sealed class DataConnectionItem
        {
            private System.ComponentModel.Design.Data.DesignerDataConnection _designerDataConnection;

            public DataConnectionItem(System.ComponentModel.Design.Data.DesignerDataConnection designerDataConnection)
            {
                this._designerDataConnection = designerDataConnection;
            }

            public override string ToString()
            {
                return this._designerDataConnection.Name;
            }

            public System.ComponentModel.Design.Data.DesignerDataConnection DesignerDataConnection
            {
                get
                {
                    return this._designerDataConnection;
                }
            }
        }

        private sealed class DetailsButton : System.Windows.Forms.Button
        {
            private bool _details;
            private const int PlusLineLength = 3;

            protected override void OnClick(EventArgs e)
            {
                this._details = !this._details;
                base.OnClick(e);
                base.Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.DrawLine(SystemPens.ControlText, (int) ((base.Width / 2) - 3), (int) (base.Height / 2), (int) ((base.Width / 2) + 3), (int) (base.Height / 2));
                if (!this._details)
                {
                    e.Graphics.DrawLine(SystemPens.ControlText, (int) (base.Width / 2), (int) ((base.Height / 2) - 3), (int) (base.Width / 2), (int) ((base.Height / 2) + 3));
                }
            }
        }
    }
}

