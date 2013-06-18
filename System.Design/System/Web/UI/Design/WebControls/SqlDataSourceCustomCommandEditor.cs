namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.Design.Data;
    using System.Data;
    using System.Data.Common;
    using System.Design;
    using System.Drawing;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    internal class SqlDataSourceCustomCommandEditor : UserControl
    {
        private System.Windows.Forms.TextBox _commandTextBox;
        private SqlDataSourceCommandType _commandType;
        private DesignerDataConnection _dataConnection;
        private IDataEnvironment _dataEnvironment;
        private QueryBuilderMode _editorMode;
        private string _originalCommand;
        private ICollection _parameters;
        private System.Windows.Forms.Button _queryBuilderButton;
        private bool _queryInitialized;
        private SqlDataSourceDesigner _sqlDataSourceDesigner;
        private System.Windows.Forms.Panel _sqlPanel;
        private System.Windows.Forms.RadioButton _sqlRadioButton;
        private AutoSizeComboBox _storedProcedureComboBox;
        private System.Windows.Forms.Panel _storedProcedurePanel;
        private System.Windows.Forms.RadioButton _storedProcedureRadioButton;
        private ICollection _storedProcedures;
        private static readonly object EventCommandChanged = new object();

        public event EventHandler CommandChanged
        {
            add
            {
                base.Events.AddHandler(EventCommandChanged, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventCommandChanged, value);
            }
        }

        public SqlDataSourceCustomCommandEditor()
        {
            this.InitializeComponent();
            this.InitializeUI();
        }

        public SqlDataSourceQuery GetQuery()
        {
            SqlDataSourceQuery query;
            Cursor current = Cursor.Current;
            try
            {
                DbProviderFactory dbProviderFactory;
                Cursor.Current = Cursors.WaitCursor;
                if (this._sqlRadioButton.Checked)
                {
                    SqlDataSourceCommandType text;
                    ICollection is2;
                    if (this._commandTextBox.Text.Trim().Length <= 0)
                    {
                        return new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                    }
                    if (string.Equals(this._commandTextBox.Text, this._originalCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        text = this._commandType;
                    }
                    else
                    {
                        text = SqlDataSourceCommandType.Text;
                    }
                    dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this._dataConnection.ProviderName);
                    if ((this._editorMode == QueryBuilderMode.Select) || SqlDataSourceDesigner.SupportsNamedParameters(dbProviderFactory))
                    {
                        Parameter[] c = this._sqlDataSourceDesigner.InferParameterNames(this._dataConnection, this._commandTextBox.Text, text);
                        if (c == null)
                        {
                            return null;
                        }
                        ArrayList list = new ArrayList(c);
                        is2 = this.MergeParameters(this._parameters, list, SqlDataSourceDesigner.SupportsNamedParameters(dbProviderFactory));
                    }
                    else
                    {
                        is2 = this._parameters;
                    }
                    return new SqlDataSourceQuery(this._commandTextBox.Text, text, is2);
                }
                StoredProcedureItem selectedItem = this._storedProcedureComboBox.SelectedItem as StoredProcedureItem;
                if (selectedItem == null)
                {
                    return new SqlDataSourceQuery(string.Empty, SqlDataSourceCommandType.Text, new Parameter[0]);
                }
                ArrayList newParameters = new ArrayList();
                ICollection is3 = null;
                try
                {
                    is3 = selectedItem.DesignerDataStoredProcedure.Parameters;
                }
                catch (Exception exception)
                {
                    UIServiceHelper.ShowError(this._sqlDataSourceDesigner.Component.Site, exception, System.Design.SR.GetString("SqlDataSourceCustomCommandEditor_CouldNotGetStoredProcedureSchema"));
                    return null;
                }
                dbProviderFactory = SqlDataSourceDesigner.GetDbProviderFactory(this._dataConnection.ProviderName);
                if ((is3 != null) && (is3.Count > 0))
                {
                    foreach (DesignerDataParameter parameter in is3)
                    {
                        string name = SqlDataSourceDesigner.StripParameterPrefix(parameter.Name);
                        Parameter parameter2 = SqlDataSourceDesigner.CreateParameter(dbProviderFactory, name, parameter.DataType);
                        parameter2.Direction = parameter.Direction;
                        newParameters.Add(parameter2);
                    }
                }
                ICollection parameters = this.MergeParameters(this._parameters, newParameters, SqlDataSourceDesigner.SupportsNamedParameters(dbProviderFactory));
                query = new SqlDataSourceQuery(selectedItem.DesignerDataStoredProcedure.Name, SqlDataSourceCommandType.StoredProcedure, parameters);
            }
            finally
            {
                Cursor.Current = current;
            }
            return query;
        }

        private void InitializeComponent()
        {
            this._commandTextBox = new System.Windows.Forms.TextBox();
            this._queryBuilderButton = new System.Windows.Forms.Button();
            this._sqlRadioButton = new System.Windows.Forms.RadioButton();
            this._storedProcedureRadioButton = new System.Windows.Forms.RadioButton();
            this._storedProcedureComboBox = new AutoSizeComboBox();
            this._storedProcedurePanel = new System.Windows.Forms.Panel();
            this._sqlPanel = new System.Windows.Forms.Panel();
            this._storedProcedurePanel.SuspendLayout();
            this._sqlPanel.SuspendLayout();
            base.SuspendLayout();
            this._sqlRadioButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._sqlRadioButton.Location = new Point(12, 12);
            this._sqlRadioButton.Name = "_sqlRadioButton";
            this._sqlRadioButton.Size = new Size(0x1e9, 20);
            this._sqlRadioButton.TabIndex = 10;
            this._sqlRadioButton.CheckedChanged += new EventHandler(this.OnSqlRadioButtonCheckedChanged);
            this._sqlPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._sqlPanel.Controls.Add(this._queryBuilderButton);
            this._sqlPanel.Controls.Add(this._commandTextBox);
            this._sqlPanel.Location = new Point(0x1c, 0x20);
            this._sqlPanel.Name = "_sqlPanel";
            this._sqlPanel.Size = new Size(480, 0x79);
            this._sqlPanel.TabIndex = 20;
            this._storedProcedureRadioButton.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._storedProcedureRadioButton.Location = new Point(12, 160);
            this._storedProcedureRadioButton.Name = "_storedProcedureRadioButton";
            this._storedProcedureRadioButton.Size = new Size(0x1e9, 20);
            this._storedProcedureRadioButton.TabIndex = 30;
            this._storedProcedurePanel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom;
            this._storedProcedurePanel.Controls.Add(this._storedProcedureComboBox);
            this._storedProcedurePanel.Location = new Point(0x1c, 180);
            this._storedProcedurePanel.Name = "_storedProcedurePanel";
            this._storedProcedurePanel.Size = new Size(0x109, 0x15);
            this._storedProcedurePanel.TabIndex = 40;
            this._commandTextBox.AcceptsReturn = true;
            this._commandTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._commandTextBox.Location = new Point(0, 0);
            this._commandTextBox.Multiline = true;
            this._commandTextBox.Name = "_commandTextBox";
            this._commandTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._commandTextBox.Size = new Size(480, 0x5d);
            this._commandTextBox.TabIndex = 20;
            this._commandTextBox.TextChanged += new EventHandler(this.OnCommandTextBoxTextChanged);
            this._queryBuilderButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
            this._queryBuilderButton.Location = new Point(330, 0x62);
            this._queryBuilderButton.Name = "_queryBuilderButton";
            this._queryBuilderButton.Size = new Size(150, 0x17);
            this._queryBuilderButton.TabIndex = 30;
            this._queryBuilderButton.Click += new EventHandler(this.OnQueryBuilderButtonClick);
            this._storedProcedureComboBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
            this._storedProcedureComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._storedProcedureComboBox.Location = new Point(0, 0);
            this._storedProcedureComboBox.Name = "_storedProcedureComboBox";
            this._storedProcedureComboBox.Size = new Size(0x109, 0x15);
            this._storedProcedureComboBox.TabIndex = 10;
            this._storedProcedureComboBox.SelectedIndexChanged += new EventHandler(this.OnStoredProcedureComboBoxSelectedIndexChanged);
            base.Controls.Add(this._sqlRadioButton);
            base.Controls.Add(this._sqlPanel);
            base.Controls.Add(this._storedProcedureRadioButton);
            base.Controls.Add(this._storedProcedurePanel);
            base.Name = "SqlDataSourceCustomCommandEditor";
            base.Size = new Size(0x20a, 230);
            this._storedProcedurePanel.ResumeLayout(false);
            this._sqlPanel.ResumeLayout(false);
            this._sqlPanel.PerformLayout();
            base.ResumeLayout(false);
        }

        private void InitializeUI()
        {
            this._queryBuilderButton.Text = System.Design.SR.GetString("SqlDataSourceCustomCommandEditor_QueryBuilderButton");
            this._sqlRadioButton.Text = System.Design.SR.GetString("SqlDataSourceCustomCommandEditor_SqlLabel");
            this._storedProcedureRadioButton.Text = System.Design.SR.GetString("SqlDataSourceCustomCommandEditor_StoredProcedureLabel");
        }

        private ICollection MergeParameters(ICollection originalParameters, ArrayList newParameters, bool useNamedParameters)
        {
            List<Parameter> list = new List<Parameter>();
            foreach (Parameter parameter in originalParameters)
            {
                list.Add(parameter);
            }
            List<Parameter> list2 = new List<Parameter>();
            for (int i = 0; i < newParameters.Count; i++)
            {
                Parameter item = (Parameter) newParameters[i];
                Parameter parameter3 = null;
                foreach (Parameter parameter4 in list)
                {
                    bool flag = useNamedParameters ? (string.Equals(parameter4.Name, item.Name, StringComparison.OrdinalIgnoreCase) && (parameter4.Direction == item.Direction)) : (parameter4.Direction == item.Direction);
                    bool flag2 = (parameter4.Direction == ParameterDirection.ReturnValue) && (item.Direction == ParameterDirection.ReturnValue);
                    if (flag || flag2)
                    {
                        parameter3 = parameter4;
                        break;
                    }
                }
                if (parameter3 != null)
                {
                    list2.Add(parameter3);
                    list.Remove(parameter3);
                }
                else if ((item.Direction == ParameterDirection.Input) || (item.Direction == ParameterDirection.InputOutput))
                {
                    list2.Add(item);
                }
            }
            return list2;
        }

        private void OnCommandChanged(EventArgs e)
        {
            EventHandler handler = base.Events[EventCommandChanged] as EventHandler;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnCommandTextBoxTextChanged(object sender, EventArgs e)
        {
            this.OnCommandChanged(EventArgs.Empty);
        }

        private void OnQueryBuilderButtonClick(object sender, EventArgs e)
        {
            IServiceProvider site = this._sqlDataSourceDesigner.Component.Site;
            if ((this._dataConnection.ConnectionString != null) && (this._dataConnection.ConnectionString.Trim().Length == 0))
            {
                UIServiceHelper.ShowError(site, System.Design.SR.GetString("SqlDataSourceCustomCommandEditor_NoConnectionString"));
            }
            else
            {
                DesignerDataConnection connection = this._dataConnection;
                if (string.IsNullOrEmpty(this._dataConnection.ProviderName))
                {
                    connection = new DesignerDataConnection(this._dataConnection.Name, "System.Data.SqlClient", this._dataConnection.ConnectionString, this._dataConnection.IsConfigured);
                }
                string str = this._dataEnvironment.BuildQuery(this, connection, this._editorMode, this._commandTextBox.Text);
                if ((str != null) && (str.Length > 0))
                {
                    this._commandTextBox.Text = str;
                    this._commandTextBox.Focus();
                    this._commandTextBox.Select(0, 0);
                }
            }
        }

        private void OnSqlRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            this.UpdateEnabledState();
        }

        private void OnStoredProcedureComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            this.OnCommandChanged(EventArgs.Empty);
        }

        public void SetCommandData(SqlDataSourceDesigner sqlDataSourceDesigner, QueryBuilderMode editorMode)
        {
            this._sqlDataSourceDesigner = sqlDataSourceDesigner;
            this._editorMode = editorMode;
            this._queryBuilderButton.Enabled = false;
            IServiceProvider site = this._sqlDataSourceDesigner.Component.Site;
            if (site != null)
            {
                this._dataEnvironment = (IDataEnvironment) site.GetService(typeof(IDataEnvironment));
            }
        }

        public void SetConnection(DesignerDataConnection dataConnection)
        {
            this._dataConnection = dataConnection;
        }

        public void SetQuery(SqlDataSourceQuery query)
        {
            this._storedProcedureComboBox.SelectedIndex = -1;
            if (this._storedProcedures != null)
            {
                foreach (StoredProcedureItem item in this._storedProcedureComboBox.Items)
                {
                    if (item.DesignerDataStoredProcedure.Name == query.Command)
                    {
                        this._storedProcedureComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            if (this._storedProcedureComboBox.SelectedIndex != -1)
            {
                this._sqlRadioButton.Checked = false;
                this._storedProcedureRadioButton.Checked = true;
            }
            else
            {
                this._sqlRadioButton.Checked = true;
                this._storedProcedureRadioButton.Checked = false;
                if (this._storedProcedureComboBox.Items.Count > 0)
                {
                    this._storedProcedureComboBox.SelectedIndex = 0;
                }
            }
            if (!this._queryInitialized)
            {
                this._commandTextBox.Text = query.Command;
                this._originalCommand = query.Command;
                this._commandType = query.CommandType;
                this._parameters = query.Parameters;
                this._queryInitialized = true;
            }
            this.UpdateEnabledState();
        }

        public void SetStoredProcedures(ICollection storedProcedures)
        {
            this._storedProcedures = storedProcedures;
            bool flag = (this._storedProcedures != null) && (this._storedProcedures.Count > 0);
            this._storedProcedureRadioButton.Enabled = flag;
            this._storedProcedureComboBox.Items.Clear();
            if (flag)
            {
                List<StoredProcedureItem> list = new List<StoredProcedureItem>();
                foreach (DesignerDataStoredProcedure procedure in this._storedProcedures)
                {
                    list.Add(new StoredProcedureItem(procedure));
                }
                list.Sort((Comparison<StoredProcedureItem>) ((a, b) => string.Compare(a.DesignerDataStoredProcedure.Name, b.DesignerDataStoredProcedure.Name, StringComparison.InvariantCultureIgnoreCase)));
                this._storedProcedureComboBox.Items.AddRange(list.ToArray());
                this._storedProcedureComboBox.InvalidateDropDownWidth();
            }
        }

        private void UpdateEnabledState()
        {
            bool flag = this._sqlRadioButton.Checked;
            this._commandTextBox.Enabled = flag;
            this._queryBuilderButton.Enabled = flag;
            this._storedProcedureComboBox.Enabled = !flag;
            this.OnCommandChanged(EventArgs.Empty);
        }

        public bool HasQuery
        {
            get
            {
                if (this._sqlRadioButton.Checked)
                {
                    return (this._commandTextBox.Text.Trim().Length > 0);
                }
                StoredProcedureItem selectedItem = this._storedProcedureComboBox.SelectedItem as StoredProcedureItem;
                return (selectedItem != null);
            }
        }

        private sealed class StoredProcedureItem
        {
            private System.ComponentModel.Design.Data.DesignerDataStoredProcedure _designerDataStoredProcedure;

            public StoredProcedureItem(System.ComponentModel.Design.Data.DesignerDataStoredProcedure designerDataStoredProcedure)
            {
                this._designerDataStoredProcedure = designerDataStoredProcedure;
            }

            public override string ToString()
            {
                return this._designerDataStoredProcedure.Name;
            }

            public System.ComponentModel.Design.Data.DesignerDataStoredProcedure DesignerDataStoredProcedure
            {
                get
                {
                    return this._designerDataStoredProcedure;
                }
            }
        }
    }
}

