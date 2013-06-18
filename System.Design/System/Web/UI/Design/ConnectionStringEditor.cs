namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Data;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Web.Compilation;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ConnectionStringEditor : UITypeEditor
    {
        private ConnectionStringPicker _connectionStringPicker;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            System.Web.UI.Control instance = context.Instance as System.Web.UI.Control;
            if (provider != null)
            {
                IDataEnvironment environment = (IDataEnvironment) provider.GetService(typeof(IDataEnvironment));
                if (environment != null)
                {
                    IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                    if ((edSvc != null) && (context.Instance != null))
                    {
                        if (this._connectionStringPicker == null)
                        {
                            this._connectionStringPicker = new ConnectionStringPicker();
                        }
                        string connectionString = (string) value;
                        ExpressionEditor expressionEditor = ExpressionEditor.GetExpressionEditor(typeof(ConnectionStringsExpressionBuilder), provider);
                        if (expressionEditor == null)
                        {
                            return value;
                        }
                        string expressionPrefix = expressionEditor.ExpressionPrefix;
                        DesignerDataConnection currentConnection = GetCurrentConnection(instance, context.PropertyDescriptor.Name, connectionString, expressionPrefix);
                        this._connectionStringPicker.Start(edSvc, environment.Connections, currentConnection);
                        edSvc.DropDownControl(this._connectionStringPicker);
                        if (this._connectionStringPicker.SelectedItem != null)
                        {
                            DesignerDataConnection selectedConnection = this._connectionStringPicker.SelectedConnection;
                            if (selectedConnection == null)
                            {
                                selectedConnection = environment.BuildConnection(UIServiceHelper.GetDialogOwnerWindow(provider), null);
                            }
                            if (selectedConnection != null)
                            {
                                if (selectedConnection.IsConfigured)
                                {
                                    ((IExpressionsAccessor) instance).Expressions.Add(new ExpressionBinding(context.PropertyDescriptor.Name, context.PropertyDescriptor.PropertyType, expressionPrefix, selectedConnection.Name));
                                    this.SetProviderName(context.Instance, selectedConnection);
                                    IComponentChangeService service = (IComponentChangeService) provider.GetService(typeof(IComponentChangeService));
                                    if (service != null)
                                    {
                                        service.OnComponentChanged(instance, null, null, null);
                                    }
                                }
                                else
                                {
                                    value = selectedConnection.ConnectionString;
                                    this.SetProviderName(context.Instance, selectedConnection);
                                }
                            }
                        }
                        this._connectionStringPicker.End();
                    }
                    return value;
                }
            }
            string providerName = this.GetProviderName(context.Instance);
            ConnectionStringEditorDialog form = new ConnectionStringEditorDialog(provider, providerName) {
                ConnectionString = (string) value
            };
            if (UIServiceHelper.ShowDialog(provider, form) == DialogResult.OK)
            {
                value = form.ConnectionString;
            }
            return value;
        }

        private static DesignerDataConnection GetCurrentConnection(System.Web.UI.Control control, string propertyName, string connectionString, string expressionPrefix)
        {
            ExpressionBinding binding = ((IExpressionsAccessor) control).Expressions[propertyName];
            string str = "." + "ConnectionString".ToLowerInvariant();
            if ((binding != null) && string.Equals(binding.ExpressionPrefix, expressionPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string expression = binding.Expression;
                if (expression.ToLowerInvariant().EndsWith(str, StringComparison.Ordinal))
                {
                    expression.Substring(0, expression.Length - str.Length);
                }
                return new DesignerDataConnection(binding.Expression, string.Empty, connectionString, true);
            }
            return new DesignerDataConnection(string.Empty, string.Empty, connectionString, false);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if ((context != null) && (((IDataEnvironment) context.GetService(typeof(IDataEnvironment))) != null))
            {
                return UITypeEditorEditStyle.DropDown;
            }
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual string GetProviderName(object instance)
        {
            return string.Empty;
        }

        protected virtual void SetProviderName(object instance, DesignerDataConnection connection)
        {
        }

        private sealed class ConnectionStringEditorDialog : DesignerForm
        {
            private Button _cancelButton;
            private TextBox _connectionStringTextBox;
            private NameValueCollection _defaultConnectionStrings;
            private Label _helpLabel;
            private Button _okButton;
            private string _providerName;

            public ConnectionStringEditorDialog(IServiceProvider serviceProvider, string providerName) : base(serviceProvider)
            {
                this.InitializeComponent();
                this.InitializeUI();
                this._providerName = providerName;
            }

            private void InitializeComponent()
            {
                this._helpLabel = new Label();
                this._okButton = new Button();
                this._cancelButton = new Button();
                this._connectionStringTextBox = new TextBox();
                base.SuspendLayout();
                this._helpLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
                this._helpLabel.Location = new Point(12, 12);
                this._helpLabel.Name = "_helpLabel";
                this._helpLabel.Size = new Size(0x171, 0x10);
                this._helpLabel.TabIndex = 10;
                this._okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this._okButton.Location = new Point(0xe4, 0xe9);
                this._okButton.Name = "_okButton";
                this._okButton.TabIndex = 30;
                this._okButton.Click += new EventHandler(this.OnOkButtonClick);
                this._cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this._cancelButton.DialogResult = DialogResult.Cancel;
                this._cancelButton.Location = new Point(310, 0xe9);
                this._cancelButton.Name = "_cancelButton";
                this._cancelButton.TabIndex = 40;
                this._cancelButton.Click += new EventHandler(this.OnCancelButtonClick);
                this._connectionStringTextBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Top;
                this._connectionStringTextBox.Location = new Point(12, 0x24);
                this._connectionStringTextBox.Multiline = true;
                this._connectionStringTextBox.Name = "_connectionStringTextBox";
                this._connectionStringTextBox.Size = new Size(0x171, 190);
                this._connectionStringTextBox.TabIndex = 20;
                base.AcceptButton = this._okButton;
                this.AutoSize = true;
                base.CancelButton = this._cancelButton;
                base.ClientSize = new Size(0x188, 0x10a);
                base.Controls.Add(this._connectionStringTextBox);
                base.Controls.Add(this._cancelButton);
                base.Controls.Add(this._okButton);
                base.Controls.Add(this._helpLabel);
                this.MinimumSize = new Size(400, 300);
                base.Name = "Form1";
                base.SizeGripStyle = SizeGripStyle.Hide;
                base.InitializeForm();
                base.ResumeLayout(false);
                base.PerformLayout();
            }

            private void InitializeUI()
            {
                this._helpLabel.Text = System.Design.SR.GetString("ConnectionStringEditor_HelpLabel");
                this._okButton.Text = System.Design.SR.GetString("OK");
                this._cancelButton.Text = System.Design.SR.GetString("Cancel");
                this.Text = System.Design.SR.GetString("ConnectionStringEditor_Title");
            }

            private void OnCancelButtonClick(object sender, EventArgs e)
            {
                base.DialogResult = DialogResult.Cancel;
                base.Close();
            }

            private void OnOkButtonClick(object sender, EventArgs e)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }

            public string ConnectionString
            {
                get
                {
                    return this._connectionStringTextBox.Text;
                }
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        if (string.IsNullOrEmpty(this._providerName))
                        {
                            this._connectionStringTextBox.Text = this.DefaultConnectionStrings["System.Data.SqlClient"];
                        }
                        else
                        {
                            this._connectionStringTextBox.Text = this.DefaultConnectionStrings[this._providerName];
                        }
                    }
                    else
                    {
                        this._connectionStringTextBox.Text = value;
                    }
                }
            }

            private NameValueCollection DefaultConnectionStrings
            {
                get
                {
                    if (this._defaultConnectionStrings == null)
                    {
                        this._defaultConnectionStrings = new NameValueCollection();
                        this._defaultConnectionStrings.Add("System.Data.SqlClient", "server=(local); trusted_connection=true; database=[database]");
                        this._defaultConnectionStrings.Add("System.Data.Odbc", "Driver=[driver]; Server=[server]; Database=[database]; Uid=[username]; Pwd=[password]");
                        this._defaultConnectionStrings.Add("System.Data.OleDb", "Provider=[provider]; Data Source=[server]; Initial Catalog=[database]; User Id=[username]; Password=[password]");
                        this._defaultConnectionStrings.Add("System.Data.OracleClient", "Data Source=Oracle8i; Integrated Security=SSPI");
                    }
                    return this._defaultConnectionStrings;
                }
            }

            protected override string HelpTopic
            {
                get
                {
                    return "net.Asp.ConnectionStrings.Editor";
                }
            }
        }

        private sealed class ConnectionStringPicker : ListBox
        {
            private IWindowsFormsEditorService _edSvc;
            private bool _keyDown;
            private bool _mouseClicked;

            public ConnectionStringPicker()
            {
                base.BorderStyle = BorderStyle.None;
            }

            public void End()
            {
                base.Items.Clear();
                this._edSvc = null;
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                base.OnKeyUp(e);
                this._keyDown = true;
                this._mouseClicked = false;
                if (e.KeyData == Keys.Enter)
                {
                    this._keyDown = false;
                    this._edSvc.CloseDropDown();
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                this._mouseClicked = true;
            }

            protected override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);
                this._mouseClicked = false;
            }

            protected override void OnSelectedIndexChanged(EventArgs e)
            {
                base.OnSelectedIndexChanged(e);
                if (this._mouseClicked && !this._keyDown)
                {
                    this._mouseClicked = false;
                    this._keyDown = false;
                    this._edSvc.CloseDropDown();
                }
            }

            public void Start(IWindowsFormsEditorService edSvc, ICollection connections, DesignerDataConnection currentConnection)
            {
                this._edSvc = edSvc;
                base.Items.Clear();
                object obj2 = null;
                foreach (DesignerDataConnection connection in connections)
                {
                    DataConnectionItem item = new DataConnectionItem(connection);
                    if ((connection.ConnectionString == currentConnection.ConnectionString) && (connection.IsConfigured == currentConnection.IsConfigured))
                    {
                        obj2 = item;
                    }
                    base.Items.Add(item);
                }
                base.Items.Add(new DataConnectionItem());
                base.SelectedItem = obj2;
            }

            public DesignerDataConnection SelectedConnection
            {
                get
                {
                    DataConnectionItem selectedItem = base.SelectedItem as DataConnectionItem;
                    if (selectedItem != null)
                    {
                        return selectedItem.DesignerDataConnection;
                    }
                    return null;
                }
            }

            private sealed class DataConnectionItem
            {
                private System.ComponentModel.Design.Data.DesignerDataConnection _designerDataConnection;

                public DataConnectionItem()
                {
                }

                public DataConnectionItem(System.ComponentModel.Design.Data.DesignerDataConnection designerDataConnection)
                {
                    this._designerDataConnection = designerDataConnection;
                }

                public override string ToString()
                {
                    if (this._designerDataConnection == null)
                    {
                        return System.Design.SR.GetString("ConnectionStringEditor_NewConnection");
                    }
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
        }
    }
}

