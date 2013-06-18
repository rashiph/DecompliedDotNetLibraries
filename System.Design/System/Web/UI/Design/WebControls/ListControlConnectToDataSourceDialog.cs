namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class ListControlConnectToDataSourceDialog : TaskForm
    {
        private ListControlDesigner _controlDesigner;
        private ComboBox _dataSourceBox;
        private System.Windows.Forms.Label _dataSourceLabel;
        private ComboBox _dataTextFieldBox;
        private System.Windows.Forms.Label _dataTextFieldLabel;
        private ComboBox _dataValueFieldBox;
        private System.Windows.Forms.Label _dataValueFieldLabel;
        private IDataSourceFieldSchema[] _fieldSchemas;
        private string _originalDataSourceID;
        private LinkLabel _refreshSchemaLink;
        private IList<IDataSourceDesigner> _suppressedDataSources;

        public ListControlConnectToDataSourceDialog(ListControlDesigner controlDesigner) : base(controlDesigner.Component.Site)
        {
            this._controlDesigner = controlDesigner;
            this._originalDataSourceID = controlDesigner.DataSourceID;
            this.SuppressChangedEvents(this._controlDesigner.DataSourceDesigner);
            base.Glyph = new Bitmap(base.GetType(), "datasourcewizard.bmp");
            this.CreatePanel();
        }

        private void CreatePanel()
        {
            base.SuspendLayout();
            this.CreatePanelControls();
            this.InitializePanelControls();
            base.InitializeForm();
            base.ResumeLayout(false);
            base.PerformLayout();
        }

        private void CreatePanelControls()
        {
            this._dataSourceBox = new ComboBox();
            this._dataTextFieldBox = new ComboBox();
            this._dataValueFieldBox = new ComboBox();
            this._refreshSchemaLink = new LinkLabel();
            this._dataSourceLabel = new System.Windows.Forms.Label();
            this._dataTextFieldLabel = new System.Windows.Forms.Label();
            this._dataValueFieldLabel = new System.Windows.Forms.Label();
            this._dataSourceLabel.Location = new Point(0, 0);
            this._dataSourceLabel.Name = "_dataSourceLabel";
            this._dataSourceLabel.Size = new Size(450, 0x10);
            this._dataSourceLabel.TabIndex = 0;
            this._dataSourceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this._dataSourceBox.Location = new Point(0, 0x12);
            this._dataSourceBox.Name = "_dataSourceBox";
            this._dataSourceBox.Size = new Size(0xc0, 0x15);
            this._dataSourceBox.TabIndex = 1;
            this._dataSourceBox.SelectedIndexChanged += new EventHandler(this.OnSelectedDataSourceChanged);
            this._dataTextFieldLabel.Location = new Point(0, 0x2f);
            this._dataTextFieldLabel.Name = "_dataTextFieldLabel";
            this._dataTextFieldLabel.Size = new Size(450, 0x10);
            this._dataTextFieldLabel.TabIndex = 2;
            this._dataTextFieldBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._dataTextFieldBox.Location = new Point(0, 0x41);
            this._dataTextFieldBox.Name = "_dataTextFieldBox";
            this._dataTextFieldBox.Size = new Size(0xc0, 0x15);
            this._dataTextFieldBox.TabIndex = 3;
            this._dataValueFieldLabel.Location = new Point(0, 0x5e);
            this._dataValueFieldLabel.Name = "_dataValueFieldLabel";
            this._dataValueFieldLabel.Size = new Size(450, 0x10);
            this._dataValueFieldLabel.TabIndex = 4;
            this._dataValueFieldBox.DropDownStyle = ComboBoxStyle.DropDown;
            this._dataValueFieldBox.Location = new Point(0, 0x70);
            this._dataValueFieldBox.Name = "_dataValueFieldBox";
            this._dataValueFieldBox.Size = new Size(0xc0, 0x15);
            this._dataValueFieldBox.TabIndex = 5;
            this._refreshSchemaLink.Links.Add(new LinkLabel.Link(0, 150));
            this._refreshSchemaLink.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            this._refreshSchemaLink.Location = new Point(0, 0xfe);
            this._refreshSchemaLink.Name = "_refreshSchemaLink";
            this._refreshSchemaLink.Size = new Size(290, 0x10);
            this._refreshSchemaLink.TabIndex = 6;
            this._refreshSchemaLink.TabStop = true;
            this._refreshSchemaLink.Visible = false;
            this._refreshSchemaLink.LinkClicked += new LinkLabelLinkClickedEventHandler(this.OnRefreshSchema);
            base.TaskPanel.Controls.Add(this._dataValueFieldLabel);
            base.TaskPanel.Controls.Add(this._dataTextFieldLabel);
            base.TaskPanel.Controls.Add(this._dataSourceLabel);
            base.TaskPanel.Controls.Add(this._refreshSchemaLink);
            base.TaskPanel.Controls.Add(this._dataValueFieldBox);
            base.TaskPanel.Controls.Add(this._dataTextFieldBox);
            base.TaskPanel.Controls.Add(this._dataSourceBox);
        }

        private void FillDataSourceList()
        {
            this._dataSourceBox.Items.Clear();
            IComponent component = this.GetComponent();
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["DataSourceID"];
            TypeConverter converter = descriptor.Converter;
            ITypeDescriptorContext context = new TypeDescriptorContext(component);
            foreach (string str in (IEnumerable) converter.GetStandardValues(context))
            {
                this._dataSourceBox.Items.Add(str);
            }
            string dataSourceID = this.Control.DataSourceID;
            if (dataSourceID.Length > 0)
            {
                int index = this._dataSourceBox.Items.IndexOf(dataSourceID);
                if (index > -1)
                {
                    this._dataSourceBox.SelectedIndex = index;
                }
                else
                {
                    this._dataSourceBox.SelectedIndex = this._dataSourceBox.Items.Add(dataSourceID);
                }
            }
            else
            {
                this._dataSourceBox.SelectedIndex = this._dataSourceBox.Items.IndexOf(System.Design.SR.GetString("DataSourceIDChromeConverter_NoDataSource"));
            }
        }

        private void FillFieldLists(bool preserveSelection)
        {
            object selectedItem = this._dataTextFieldBox.SelectedItem;
            object obj3 = this._dataValueFieldBox.SelectedItem;
            this._dataTextFieldBox.Items.Clear();
            this._dataTextFieldBox.Text = string.Empty;
            this._dataValueFieldBox.Items.Clear();
            this._dataValueFieldBox.Text = string.Empty;
            IDataSourceFieldSchema[] fieldSchemas = this.GetFieldSchemas();
            if ((fieldSchemas != null) && (fieldSchemas.Length > 0))
            {
                foreach (IDataSourceFieldSchema schema in fieldSchemas)
                {
                    this._dataTextFieldBox.Items.Add(schema.Name);
                    this._dataValueFieldBox.Items.Add(schema.Name);
                }
                this._dataTextFieldBox.SelectedIndex = 0;
                if ((selectedItem != null) && preserveSelection)
                {
                    if (this._dataTextFieldBox.Items.Contains(selectedItem))
                    {
                        this._dataTextFieldBox.SelectedItem = selectedItem;
                    }
                    else
                    {
                        this._dataTextFieldBox.Items.Insert(0, selectedItem);
                    }
                }
                this._dataValueFieldBox.SelectedIndex = 0;
                if ((obj3 != null) && preserveSelection)
                {
                    if (this._dataValueFieldBox.Items.Contains(obj3))
                    {
                        this._dataValueFieldBox.SelectedItem = obj3;
                    }
                    else
                    {
                        this._dataValueFieldBox.Items.Insert(0, obj3);
                    }
                }
            }
        }

        private IComponent GetComponent()
        {
            if (this._controlDesigner != null)
            {
                return this._controlDesigner.Component;
            }
            return null;
        }

        private IDataSourceFieldSchema[] GetFieldSchemas()
        {
            if (this._fieldSchemas == null)
            {
                IDataSourceViewSchema schema = null;
                DesignerDataSourceView designerView = this._controlDesigner.DesignerView;
                if (designerView != null)
                {
                    try
                    {
                        schema = designerView.Schema;
                    }
                    catch (Exception exception)
                    {
                        IComponentDesignerDebugService service = (IComponentDesignerDebugService) base.ServiceProvider.GetService(typeof(IComponentDesignerDebugService));
                        if (service != null)
                        {
                            service.Fail(System.Design.SR.GetString("DataSource_DebugService_FailedCall", new object[] { "DesignerDataSourceView.Schema", exception.Message }));
                        }
                    }
                }
                if (schema != null)
                {
                    this._fieldSchemas = schema.GetFields();
                }
            }
            return this._fieldSchemas;
        }

        private void InitializePanelControls()
        {
            string name = this.Control.GetType().Name;
            this._dataSourceLabel.Text = System.Design.SR.GetString("ListControlCreateDataSource_SelectDataSource");
            this._dataTextFieldLabel.Text = System.Design.SR.GetString("ListControlCreateDataSource_SelectDataTextField", new object[] { name });
            this._dataValueFieldLabel.Text = System.Design.SR.GetString("ListControlCreateDataSource_SelectDataValueField", new object[] { name });
            this._refreshSchemaLink.Text = System.Design.SR.GetString("DataSourceDesigner_RefreshSchemaNoHotkey");
            this.Text = System.Design.SR.GetString("ListControlCreateDataSource_Title");
            base.AccessibleDescription = System.Design.SR.GetString("ListControlCreateDataSource_Description", new object[] { name });
            base.CaptionLabel.Text = System.Design.SR.GetString("ListControlCreateDataSource_Caption");
            this.FillDataSourceList();
        }

        protected override void OnCancelButtonClick(object sender, EventArgs e)
        {
            this.DataSourceID = this._originalDataSourceID;
        }

        protected override void OnClosed(EventArgs e)
        {
            this.ResumeChangedEvents();
        }

        protected override void OnOKButtonClick(object sender, EventArgs e)
        {
            this.Control.DataTextField = this._dataTextFieldBox.Text;
            this.Control.DataValueField = this._dataValueFieldBox.Text;
            TypeDescriptor.Refresh(this.GetComponent());
        }

        private void OnRefreshSchema(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this._fieldSchemas = null;
            IDataSourceDesigner dataSourceDesigner = this._controlDesigner.DataSourceDesigner;
            if ((dataSourceDesigner != null) && dataSourceDesigner.CanRefreshSchema)
            {
                dataSourceDesigner.RefreshSchema(false);
                this.FillFieldLists(true);
            }
        }

        private void OnSelectedDataSourceChanged(object sender, EventArgs e)
        {
            this._fieldSchemas = null;
            this.DataSourceID = this._dataSourceBox.Text;
            string dataSourceID = this._controlDesigner.DataSourceID;
            if (dataSourceID.Length <= 0)
            {
                this._dataTextFieldBox.Items.Clear();
                this._dataValueFieldBox.Items.Clear();
                this._dataTextFieldBox.Text = string.Empty;
                this._dataValueFieldBox.Text = string.Empty;
                this._dataTextFieldBox.Enabled = false;
                this._dataValueFieldBox.Enabled = false;
                base.OKButton.Enabled = !string.Equals(dataSourceID, this._originalDataSourceID, StringComparison.Ordinal);
                this._refreshSchemaLink.Visible = false;
            }
            else
            {
                if (!this._dataSourceBox.Items.Contains(dataSourceID))
                {
                    this.FillDataSourceList();
                }
                this._dataSourceBox.SelectedItem = dataSourceID;
                this._dataTextFieldBox.Enabled = true;
                this._dataValueFieldBox.Enabled = true;
                base.OKButton.Enabled = true;
                this._refreshSchemaLink.Visible = false;
                if (this._controlDesigner.DataSourceDesigner != null)
                {
                    this.SuppressChangedEvents(this._controlDesigner.DataSourceDesigner);
                    this._refreshSchemaLink.Visible = this._controlDesigner.DataSourceDesigner.CanRefreshSchema;
                }
                this.FillFieldLists(false);
                string dataTextField = this.Control.DataTextField;
                if (dataTextField.Length > 0)
                {
                    int num = -1;
                    for (int i = 0; i < this._dataTextFieldBox.Items.Count; i++)
                    {
                        if (string.Compare(this._dataTextFieldBox.Items[i].ToString(), dataTextField, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            num = i;
                            break;
                        }
                    }
                    if (this._dataTextFieldBox.Items.Count > 0)
                    {
                        if (num >= 0)
                        {
                            this._dataTextFieldBox.SelectedIndex = num;
                        }
                    }
                    else
                    {
                        this._dataTextFieldBox.Items.Add(dataTextField);
                        this._dataTextFieldBox.SelectedIndex = 0;
                    }
                }
                string dataValueField = this.Control.DataValueField;
                if (dataValueField.Length > 0)
                {
                    int num3 = -1;
                    for (int j = 0; j < this._dataValueFieldBox.Items.Count; j++)
                    {
                        if (string.Compare(this._dataValueFieldBox.Items[j].ToString(), dataValueField, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            num3 = j;
                            break;
                        }
                    }
                    if (this._dataValueFieldBox.Items.Count > 0)
                    {
                        if (num3 >= 0)
                        {
                            this._dataValueFieldBox.SelectedIndex = num3;
                        }
                    }
                    else
                    {
                        this._dataValueFieldBox.Items.Add(dataValueField);
                        this._dataValueFieldBox.SelectedIndex = 0;
                    }
                }
            }
        }

        private void ResumeChangedEvents()
        {
            foreach (IDataSourceDesigner designer in this.SuppressedDataSources)
            {
                designer.ResumeDataSourceEvents();
            }
        }

        private void SuppressChangedEvents(IDataSourceDesigner dsd)
        {
            if ((dsd != null) && !this.SuppressedDataSources.Contains(dsd))
            {
                this.SuppressedDataSources.Add(dsd);
                dsd.SuppressDataSourceEvents();
            }
        }

        private System.Web.UI.WebControls.ListControl Control
        {
            get
            {
                return (this._controlDesigner.Component as System.Web.UI.WebControls.ListControl);
            }
        }

        private string DataSourceID
        {
            set
            {
                this._controlDesigner.DataSourceID = value;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.ListControl.ConnectToDataSource";
            }
        }

        private IList<IDataSourceDesigner> SuppressedDataSources
        {
            get
            {
                if (this._suppressedDataSources == null)
                {
                    this._suppressedDataSources = new List<IDataSourceDesigner>();
                }
                return this._suppressedDataSources;
            }
        }

        private sealed class TypeDescriptorContext : ITypeDescriptorContext, IServiceProvider
        {
            private IComponent _component;

            public TypeDescriptorContext(IComponent component)
            {
                this._component = component;
            }

            public object GetService(System.Type serviceType)
            {
                if (this._component.Site == null)
                {
                    return null;
                }
                return this._component.Site.GetService(serviceType);
            }

            public void OnComponentChanged()
            {
            }

            public bool OnComponentChanging()
            {
                return true;
            }

            public IContainer Container
            {
                get
                {
                    ISite site = this._component.Site;
                    if (site != null)
                    {
                        return site.Container;
                    }
                    return null;
                }
            }

            public object Instance
            {
                get
                {
                    return this._component;
                }
            }

            public System.ComponentModel.PropertyDescriptor PropertyDescriptor
            {
                get
                {
                    return null;
                }
            }
        }
    }
}

