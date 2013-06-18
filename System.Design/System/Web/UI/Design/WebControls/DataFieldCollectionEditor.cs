namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class DataFieldCollectionEditor : StringCollectionEditor
    {
        private const int SC_CONTEXTHELP = 0xf180;
        private const int WM_SYSCOMMAND = 0x112;

        public DataFieldCollectionEditor(System.Type type) : base(type)
        {
        }

        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            if (this.HasSchema)
            {
                ITypeDescriptorContext context = base.Context;
                if ((context != null) && (context.Instance != null))
                {
                    System.Web.UI.Control instance = context.Instance as System.Web.UI.Control;
                    if (instance != null)
                    {
                        return new DataFieldCollectionForm(instance.Site, this);
                    }
                }
            }
            return base.CreateCollectionForm();
        }

        private bool HasSchema
        {
            get
            {
                ITypeDescriptorContext context = base.Context;
                bool flag = false;
                if ((context != null) && (context.Instance != null))
                {
                    System.Web.UI.Control instance = context.Instance as System.Web.UI.Control;
                    if (instance != null)
                    {
                        ISite site = instance.Site;
                        if (site != null)
                        {
                            IDesignerHost host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                            if (host == null)
                            {
                                return flag;
                            }
                            DataBoundControlDesigner designer = host.GetDesigner(instance) as DataBoundControlDesigner;
                            if (designer == null)
                            {
                                return flag;
                            }
                            DesignerDataSourceView designerView = designer.DesignerView;
                            if (designerView == null)
                            {
                                return flag;
                            }
                            IDataSourceViewSchema schema = null;
                            try
                            {
                                schema = designerView.Schema;
                            }
                            catch (Exception exception)
                            {
                                IComponentDesignerDebugService service = (IComponentDesignerDebugService) site.GetService(typeof(IComponentDesignerDebugService));
                                if (service != null)
                                {
                                    service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                                }
                            }
                            if (schema != null)
                            {
                                flag = true;
                            }
                        }
                        return flag;
                    }
                    IDataSourceViewSchemaAccessor accessor = context.Instance as IDataSourceViewSchemaAccessor;
                    if ((accessor != null) && (accessor.DataSourceViewSchema != null))
                    {
                        flag = true;
                    }
                }
                return flag;
            }
        }

        private class DataFieldCollectionForm : CollectionEditor.CollectionForm
        {
            private string[] _dataFields;
            private IServiceProvider _serviceProvider;
            private Button cancelButton;
            private DataFieldCollectionEditor editor;
            private Label fieldLabel;
            private ArrayList fields;
            private ListBoxWithEnter fieldsList;
            private TableLayoutPanel layoutPanel;
            private Button moveDown;
            private Button moveLeft;
            private Panel moveLeftRightPanel;
            private Button moveRight;
            private Button moveUp;
            private Panel moveUpDownPanel;
            private Button okButton;
            private Label selectedFieldsLabel;
            private ListBoxWithEnter selectedFieldsList;

            public DataFieldCollectionForm(IServiceProvider serviceProvider, CollectionEditor editor) : base(editor)
            {
                this.fieldLabel = new Label();
                this.fieldsList = new ListBoxWithEnter();
                this.selectedFieldsLabel = new Label();
                this.selectedFieldsList = new ListBoxWithEnter();
                this.moveLeft = new Button();
                this.moveRight = new Button();
                this.moveUp = new Button();
                this.moveDown = new Button();
                this.okButton = new Button();
                this.cancelButton = new Button();
                this.layoutPanel = new TableLayoutPanel();
                this.moveUpDownPanel = new Panel();
                this.moveLeftRightPanel = new Panel();
                this.editor = (DataFieldCollectionEditor) editor;
                this._serviceProvider = serviceProvider;
                if (!string.Equals(System.Design.SR.GetString("RTL"), "RTL_False", StringComparison.Ordinal))
                {
                    this.RightToLeft = RightToLeft.Yes;
                    this.RightToLeftLayout = true;
                }
                this.InitializeComponent();
                this._dataFields = this.GetControlDataFieldNames();
            }

            private void AddFieldToSelectedList()
            {
                int selectedIndex = this.fieldsList.SelectedIndex;
                object selectedItem = this.fieldsList.SelectedItem;
                if (selectedIndex >= 0)
                {
                    this.fieldsList.Items.RemoveAt(selectedIndex);
                    this.selectedFieldsList.SelectedIndex = this.selectedFieldsList.Items.Add(selectedItem);
                    if (this.fieldsList.Items.Count > 0)
                    {
                        this.fieldsList.SelectedIndex = (this.fieldsList.Items.Count > selectedIndex) ? selectedIndex : (this.fieldsList.Items.Count - 1);
                    }
                }
            }

            private string[] GetControlDataFieldNames()
            {
                if (this._dataFields == null)
                {
                    ITypeDescriptorContext context = this.editor.Context;
                    IDataSourceFieldSchema[] fields = null;
                    IDataSourceViewSchema dataSourceViewSchema = null;
                    if ((context != null) && (context.Instance != null))
                    {
                        System.Web.UI.Control instance = context.Instance as System.Web.UI.Control;
                        if (instance != null)
                        {
                            ISite site = instance.Site;
                            if (site != null)
                            {
                                IDesignerHost host = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                                if (host != null)
                                {
                                    DataBoundControlDesigner designer = host.GetDesigner(instance) as DataBoundControlDesigner;
                                    if (designer != null)
                                    {
                                        DesignerDataSourceView designerView = designer.DesignerView;
                                        if (designerView != null)
                                        {
                                            try
                                            {
                                                dataSourceViewSchema = designerView.Schema;
                                            }
                                            catch (Exception exception)
                                            {
                                                IComponentDesignerDebugService service = (IComponentDesignerDebugService) site.GetService(typeof(IComponentDesignerDebugService));
                                                if (service != null)
                                                {
                                                    service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            IDataSourceViewSchemaAccessor accessor = context.Instance as IDataSourceViewSchemaAccessor;
                            if (accessor != null)
                            {
                                dataSourceViewSchema = accessor.DataSourceViewSchema as IDataSourceViewSchema;
                            }
                        }
                    }
                    if (dataSourceViewSchema != null)
                    {
                        fields = dataSourceViewSchema.GetFields();
                        if (fields != null)
                        {
                            int length = fields.Length;
                            this._dataFields = new string[length];
                            for (int i = 0; i < length; i++)
                            {
                                this._dataFields[i] = fields[i].Name;
                            }
                        }
                    }
                }
                return this._dataFields;
            }

            private void InitializeComponent()
            {
                int height = 0xd9;
                int width = 0x16c;
                base.SuspendLayout();
                this.fieldLabel.AutoSize = true;
                this.fieldLabel.TabStop = false;
                this.fieldLabel.TabIndex = 0;
                this.fieldLabel.Text = System.Design.SR.GetString("DataFieldCollectionAvailableFields");
                this.fieldLabel.MinimumSize = new Size(0x87, 15);
                this.fieldLabel.MaximumSize = new Size(0x87, 30);
                this.fieldLabel.SetBounds(0, 0, 0x87, 15);
                this.selectedFieldsLabel.AutoSize = true;
                this.selectedFieldsLabel.TabStop = false;
                this.selectedFieldsLabel.Text = System.Design.SR.GetString("DataFieldCollectionSelectedFields");
                this.selectedFieldsLabel.MinimumSize = new Size(0x87, 15);
                this.selectedFieldsLabel.MaximumSize = new Size(0x87, 30);
                this.selectedFieldsLabel.SetBounds(0xad, 0, 0x87, 15);
                this.fieldsList.TabIndex = 1;
                this.fieldsList.AllowDrop = false;
                this.fieldsList.SelectedIndexChanged += new EventHandler(this.OnFieldsSelectedIndexChanged);
                this.fieldsList.MouseDoubleClick += new MouseEventHandler(this.OnDoubleClickField);
                this.fieldsList.KeyPress += new KeyPressEventHandler(this.OnKeyPressField);
                this.fieldsList.SetBounds(0, 0, 0x87, 130);
                this.selectedFieldsList.TabIndex = 3;
                this.selectedFieldsList.AllowDrop = false;
                this.selectedFieldsList.SelectedIndexChanged += new EventHandler(this.OnSelectedFieldsSelectedIndexChanged);
                this.selectedFieldsList.MouseDoubleClick += new MouseEventHandler(this.OnDoubleClickSelectedField);
                this.selectedFieldsList.KeyPress += new KeyPressEventHandler(this.OnKeyPressSelectedField);
                this.selectedFieldsList.SetBounds(0, 0, 0x87, 130);
                this.moveRight.TabIndex = 100;
                this.moveRight.Text = ">";
                this.moveRight.AccessibleName = System.Design.SR.GetString("DataFieldCollection_MoveRight");
                this.moveRight.AccessibleDescription = System.Design.SR.GetString("DataFieldCollection_MoveRightDesc");
                this.moveRight.Click += new EventHandler(this.OnMoveRight);
                this.moveRight.Location = new Point(0, 0x2a);
                this.moveRight.Size = new Size(0x1a, 0x17);
                this.moveLeft.TabIndex = 0x65;
                this.moveLeft.Text = "<";
                this.moveLeft.AccessibleName = System.Design.SR.GetString("DataFieldCollection_MoveLeft");
                this.moveLeft.AccessibleDescription = System.Design.SR.GetString("DataFieldCollection_MoveLeftDesc");
                this.moveLeft.Click += new EventHandler(this.OnMoveLeft);
                this.moveLeft.Location = new Point(0, 0x41);
                this.moveLeft.Size = new Size(0x1a, 0x17);
                this.moveLeftRightPanel.TabIndex = 2;
                this.moveLeftRightPanel.Location = new Point(6, 0);
                this.moveLeftRightPanel.Size = new Size(0x1c, 130);
                this.moveLeftRightPanel.Controls.Add(this.moveLeft);
                this.moveLeftRightPanel.Controls.Add(this.moveRight);
                this.moveUp.TabIndex = 200;
                Bitmap bitmap = new Icon(base.GetType(), "SortUp.ico").ToBitmap();
                bitmap.MakeTransparent();
                this.moveUp.Image = bitmap;
                this.moveUp.AccessibleName = System.Design.SR.GetString("DataFieldCollection_MoveUp");
                this.moveUp.AccessibleDescription = System.Design.SR.GetString("DataFieldCollection_MoveUpDesc");
                this.moveUp.Click += new EventHandler(this.OnMoveUp);
                this.moveUp.Location = new Point(0, 0);
                this.moveUp.Size = new Size(0x1a, 0x17);
                this.moveDown.TabIndex = 0xc9;
                Bitmap bitmap2 = new Icon(base.GetType(), "SortDown.ico").ToBitmap();
                bitmap2.MakeTransparent();
                this.moveDown.Image = bitmap2;
                this.moveDown.AccessibleName = System.Design.SR.GetString("DataFieldCollection_MoveDown");
                this.moveDown.AccessibleDescription = System.Design.SR.GetString("DataFieldCollection_MoveDownDesc");
                this.moveDown.Click += new EventHandler(this.OnMoveDown);
                this.moveDown.Location = new Point(0, 0x18);
                this.moveDown.Size = new Size(0x1a, 0x17);
                this.moveUpDownPanel.TabIndex = 4;
                this.moveUpDownPanel.Location = new Point(6, 0);
                this.moveUpDownPanel.Size = new Size(0x1a, 0x2f);
                this.moveUpDownPanel.Controls.Add(this.moveUp);
                this.moveUpDownPanel.Controls.Add(this.moveDown);
                this.okButton.TabIndex = 5;
                this.okButton.Text = System.Design.SR.GetString("OKCaption");
                this.okButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this.okButton.DialogResult = DialogResult.OK;
                this.okButton.Click += new EventHandler(this.OKButton_click);
                this.okButton.SetBounds(((width - 12) - 150) - 6, (height - 12) - 0x17, 0x4b, 0x17);
                this.cancelButton.TabIndex = 6;
                this.cancelButton.Text = System.Design.SR.GetString("CancelCaption");
                this.cancelButton.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                this.cancelButton.DialogResult = DialogResult.Cancel;
                this.cancelButton.SetBounds((width - 12) - 0x4b, (height - 12) - 0x17, 0x4b, 0x17);
                this.layoutPanel.AutoSize = true;
                this.layoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                this.layoutPanel.ColumnCount = 4;
                this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135f));
                this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 38f));
                this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135f));
                this.layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32f));
                this.layoutPanel.Location = new Point(12, 12);
                this.layoutPanel.Size = new Size(340, 0x93);
                this.layoutPanel.RowCount = 2;
                this.layoutPanel.RowStyles.Add(new RowStyle());
                this.layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130f));
                this.layoutPanel.Controls.Add(this.fieldLabel, 0, 0);
                this.layoutPanel.Controls.Add(this.selectedFieldsLabel, 2, 0);
                this.layoutPanel.Controls.Add(this.fieldsList, 0, 1);
                this.layoutPanel.Controls.Add(this.selectedFieldsList, 2, 1);
                this.layoutPanel.Controls.Add(this.moveLeftRightPanel, 1, 1);
                this.layoutPanel.Controls.Add(this.moveUpDownPanel, 3, 1);
                Font dialogFont = UIServiceHelper.GetDialogFont(this._serviceProvider);
                if (dialogFont != null)
                {
                    this.Font = dialogFont;
                }
                this.Text = System.Design.SR.GetString("DataFieldCollectionEditorTitle");
                base.AcceptButton = this.okButton;
                this.AutoScaleBaseSize = new Size(5, 14);
                base.CancelButton = this.cancelButton;
                base.ClientSize = new Size(width, height);
                base.FormBorderStyle = FormBorderStyle.FixedDialog;
                base.HelpButton = true;
                base.MaximizeBox = false;
                base.MinimizeBox = false;
                if (!string.Equals(System.Design.SR.GetString("RTL"), "RTL_False", StringComparison.Ordinal))
                {
                    this.RightToLeft = RightToLeft.Yes;
                    this.RightToLeftLayout = true;
                }
                base.ShowIcon = false;
                base.ShowInTaskbar = false;
                base.StartPosition = FormStartPosition.CenterParent;
                base.Controls.Clear();
                base.Controls.AddRange(new System.Windows.Forms.Control[] { this.layoutPanel, this.okButton, this.cancelButton });
                base.ResumeLayout(false);
                base.PerformLayout();
            }

            private void OKButton_click(object sender, EventArgs e)
            {
                object[] destination = new object[this.selectedFieldsList.Items.Count];
                this.selectedFieldsList.Items.CopyTo(destination, 0);
                base.Items = destination;
            }

            private void OnDoubleClickField(object sender, MouseEventArgs e)
            {
                if ((this.fieldsList.IndexFromPoint(e.Location) != -1) && (e.Button == MouseButtons.Left))
                {
                    this.AddFieldToSelectedList();
                }
            }

            private void OnDoubleClickSelectedField(object sender, MouseEventArgs e)
            {
                if ((this.selectedFieldsList.IndexFromPoint(e.Location) != -1) && (e.Button == MouseButtons.Left))
                {
                    this.RemoveFieldFromSelectedList();
                }
            }

            protected override void OnEditValueChanged()
            {
                this.fields = null;
                this.fieldsList.Items.Clear();
                this.selectedFieldsList.Items.Clear();
                this.fields = new ArrayList();
                foreach (string str in this.GetControlDataFieldNames())
                {
                    this.fields.Add(str);
                    if (Array.IndexOf<object>(base.Items, str) < 0)
                    {
                        this.fieldsList.Items.Add(str);
                    }
                }
                foreach (string str2 in base.Items)
                {
                    this.selectedFieldsList.Items.Add(str2);
                }
                if (this.fieldsList.Items.Count > 0)
                {
                    this.fieldsList.SelectedIndex = 0;
                }
                this.SetButtonsEnabled();
            }

            private void OnFieldsSelectedIndexChanged(object sender, EventArgs e)
            {
                if (this.fieldsList.SelectedIndex > -1)
                {
                    this.selectedFieldsList.SelectedIndex = -1;
                }
                this.SetButtonsEnabled();
            }

            private void OnKeyPressField(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == '\r')
                {
                    this.AddFieldToSelectedList();
                    e.Handled = true;
                }
            }

            private void OnKeyPressSelectedField(object sender, KeyPressEventArgs e)
            {
                if (e.KeyChar == '\r')
                {
                    this.RemoveFieldFromSelectedList();
                    e.Handled = true;
                }
            }

            private void OnMoveDown(object sender, EventArgs e)
            {
                int selectedIndex = this.selectedFieldsList.SelectedIndex;
                object selectedItem = this.selectedFieldsList.SelectedItem;
                this.selectedFieldsList.Items.RemoveAt(selectedIndex);
                this.selectedFieldsList.Items.Insert(selectedIndex + 1, selectedItem);
                this.selectedFieldsList.SelectedIndex = selectedIndex + 1;
            }

            private void OnMoveLeft(object sender, EventArgs e)
            {
                this.RemoveFieldFromSelectedList();
            }

            private void OnMoveRight(object sender, EventArgs e)
            {
                this.AddFieldToSelectedList();
            }

            private void OnMoveUp(object sender, EventArgs e)
            {
                int selectedIndex = this.selectedFieldsList.SelectedIndex;
                object selectedItem = this.selectedFieldsList.SelectedItem;
                this.selectedFieldsList.Items.RemoveAt(selectedIndex);
                this.selectedFieldsList.Items.Insert(selectedIndex - 1, selectedItem);
                this.selectedFieldsList.SelectedIndex = selectedIndex - 1;
            }

            private void OnSelectedFieldsSelectedIndexChanged(object sender, EventArgs e)
            {
                if (this.selectedFieldsList.SelectedIndex > -1)
                {
                    this.fieldsList.SelectedIndex = -1;
                }
                this.SetButtonsEnabled();
            }

            private void RemoveFieldFromSelectedList()
            {
                int selectedIndex = this.selectedFieldsList.SelectedIndex;
                int index = 0;
                int num3 = 0;
                if (selectedIndex >= 0)
                {
                    string str = this.selectedFieldsList.SelectedItem.ToString();
                    num3 = this.fields.IndexOf(str);
                    for (int i = 0; i < this.fieldsList.Items.Count; i++)
                    {
                        if (this.fields.IndexOf(this.fieldsList.Items[i]) > num3)
                        {
                            break;
                        }
                        index++;
                    }
                    this.fieldsList.Items.Insert(index, str);
                    this.selectedFieldsList.Items.RemoveAt(selectedIndex);
                    this.fieldsList.SelectedIndex = index;
                    if (this.selectedFieldsList.Items.Count > 0)
                    {
                        this.selectedFieldsList.SelectedIndex = (this.selectedFieldsList.Items.Count > selectedIndex) ? selectedIndex : (this.selectedFieldsList.Items.Count - 1);
                    }
                }
            }

            private void SetButtonsEnabled()
            {
                int count = this.selectedFieldsList.Items.Count;
                int selectedIndex = this.selectedFieldsList.SelectedIndex;
                bool flag = false;
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                if (this.fieldsList.SelectedIndex > -1)
                {
                    flag3 = true;
                }
                if (selectedIndex > -1)
                {
                    flag4 = true;
                    if (count > 0)
                    {
                        if (selectedIndex > 0)
                        {
                            flag = true;
                        }
                        if (selectedIndex < (count - 1))
                        {
                            flag2 = true;
                        }
                    }
                }
                this.moveRight.Enabled = flag3;
                this.moveLeft.Enabled = flag4;
                this.moveUp.Enabled = flag;
                this.moveDown.Enabled = flag2;
            }

            protected override void WndProc(ref Message m)
            {
                if ((m.Msg == 0x112) && (((int) m.WParam) == 0xf180))
                {
                    if (this._serviceProvider != null)
                    {
                        IHelpService service = (IHelpService) this._serviceProvider.GetService(typeof(IHelpService));
                        if (service != null)
                        {
                            service.ShowHelpFromKeyword("net.Asp.DataFieldCollectionEditor");
                        }
                    }
                }
                else
                {
                    base.WndProc(ref m);
                }
            }

            private class ListBoxWithEnter : ListBox
            {
                protected override bool IsInputKey(Keys keyData)
                {
                    return ((keyData == Keys.Enter) || base.IsInputKey(keyData));
                }
            }
        }
    }
}

