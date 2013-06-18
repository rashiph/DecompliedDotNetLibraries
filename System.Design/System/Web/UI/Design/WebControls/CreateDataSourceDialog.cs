namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal sealed class CreateDataSourceDialog : TaskForm
    {
        private bool _configure;
        private ControlDesigner _controlDesigner;
        private string _controlID;
        private string _dataSourceID;
        private System.Type _dataSourceType;
        private ListView _dataSourceTypesListView;
        private TextBox _descriptionBox;
        private DisplayNameComparer _displayNameComparer;
        private Label _idLabel;
        private TextBox _idTextBox;
        private Label _selectLabel;

        public CreateDataSourceDialog(ControlDesigner controlDesigner, System.Type dataSourceType, bool configure) : base(controlDesigner.Component.Site)
        {
            this._controlDesigner = controlDesigner;
            this._controlID = ((System.Web.UI.Control) controlDesigner.Component).ID;
            this._dataSourceType = dataSourceType;
            this._configure = configure;
            this._displayNameComparer = new DisplayNameComparer();
            base.Glyph = new Bitmap(base.GetType(), "datasourcewizard.bmp");
            this.CreatePanel();
        }

        private string CreateNewDataSource(System.Type dataSourceType)
        {
            string text = this._idTextBox.Text;
            string str2 = string.Empty;
            if (dataSourceType != null)
            {
                object obj2 = Activator.CreateInstance(dataSourceType);
                if (obj2 == null)
                {
                    return str2;
                }
                System.Web.UI.Control newControl = obj2 as System.Web.UI.Control;
                if (newControl == null)
                {
                    return str2;
                }
                newControl.ID = text;
                ISite serviceProvider = this.GetSite();
                if (serviceProvider == null)
                {
                    return str2;
                }
                INameCreationService service = (INameCreationService) serviceProvider.GetService(typeof(INameCreationService));
                if (service != null)
                {
                    try
                    {
                        service.ValidateName(text);
                    }
                    catch (Exception exception)
                    {
                        UIServiceHelper.ShowError(serviceProvider, System.Design.SR.GetString("CreateDataSource_NameNotValid", new object[] { exception.Message }));
                        this._idTextBox.Focus();
                        return str2;
                    }
                    IContainer container = serviceProvider.Container;
                    if (container != null)
                    {
                        ComponentCollection components = container.Components;
                        if ((components != null) && (components[text] != null))
                        {
                            UIServiceHelper.ShowError(serviceProvider, System.Design.SR.GetString("CreateDataSource_NameNotUnique"));
                            this._idTextBox.Focus();
                            return str2;
                        }
                    }
                }
                IDesignerHost host = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
                if (host == null)
                {
                    return str2;
                }
                IComponent rootComponent = host.RootComponent;
                if (rootComponent == null)
                {
                    return str2;
                }
                WebFormsRootDesigner designer = host.GetDesigner(rootComponent) as WebFormsRootDesigner;
                if (designer == null)
                {
                    return str2;
                }
                System.Web.UI.Control component = this.GetComponent() as System.Web.UI.Control;
                str2 = designer.AddControlToDocument(newControl, component, ControlLocation.After);
                IDesigner designer2 = host.GetDesigner(newControl);
                IDataSourceDesigner designer3 = designer2 as IDataSourceDesigner;
                if (designer3 != null)
                {
                    if (designer3.CanConfigure && this._configure)
                    {
                        designer3.Configure();
                    }
                    return str2;
                }
                IHierarchicalDataSourceDesigner designer4 = designer2 as IHierarchicalDataSourceDesigner;
                if (((designer4 != null) && designer4.CanConfigure) && this._configure)
                {
                    designer4.Configure();
                }
            }
            return str2;
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
            this._selectLabel = new Label();
            this._dataSourceTypesListView = new ListView();
            this._descriptionBox = new TextBox();
            this._idLabel = new Label();
            this._idTextBox = new TextBox();
            this._selectLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._selectLabel.Location = new Point(0, 0);
            this._selectLabel.Name = "_selectLabel";
            this._selectLabel.Size = new Size(0x220, 0x10);
            this._selectLabel.TabIndex = 0;
            this._dataSourceTypesListView.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._dataSourceTypesListView.Location = new Point(0, 0x12);
            this._dataSourceTypesListView.Name = "_dataSourceTypesListView";
            this._dataSourceTypesListView.Size = new Size(0x220, 90);
            this._dataSourceTypesListView.TabIndex = 1;
            this._dataSourceTypesListView.SelectedIndexChanged += new EventHandler(this.OnDataSourceTypeChosen);
            this._dataSourceTypesListView.Alignment = ListViewAlignment.Left;
            this._dataSourceTypesListView.LabelWrap = true;
            this._dataSourceTypesListView.MultiSelect = false;
            this._dataSourceTypesListView.HideSelection = false;
            this._dataSourceTypesListView.ListViewItemSorter = this._displayNameComparer;
            this._dataSourceTypesListView.Sorting = SortOrder.Ascending;
            this._dataSourceTypesListView.MouseDoubleClick += new MouseEventHandler(this.OnListViewDoubleClick);
            this._descriptionBox.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._descriptionBox.Location = new Point(0, 0x70);
            this._descriptionBox.Name = "_descriptionBox";
            this._descriptionBox.Size = new Size(0x220, 0x37);
            this._descriptionBox.TabIndex = 2;
            this._descriptionBox.ReadOnly = true;
            this._descriptionBox.Multiline = true;
            this._descriptionBox.TabStop = false;
            this._descriptionBox.BackColor = SystemColors.Control;
            this._descriptionBox.Multiline = true;
            this._idLabel.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            this._idLabel.Location = new Point(0, 0xb0);
            this._idLabel.Name = "_idLabel";
            this._idLabel.Size = new Size(0x220, 0x10);
            this._idLabel.TabIndex = 3;
            this._idTextBox.Location = new Point(0, 0xc2);
            this._idTextBox.Name = "_idTextBox";
            this._idTextBox.Size = new Size(220, 20);
            this._idTextBox.TabIndex = 4;
            this._idTextBox.TextChanged += new EventHandler(this.OnIDChanged);
            base.TaskPanel.Controls.Add(this._idTextBox);
            base.TaskPanel.Controls.Add(this._idLabel);
            base.TaskPanel.Controls.Add(this._descriptionBox);
            base.TaskPanel.Controls.Add(this._dataSourceTypesListView);
            base.TaskPanel.Controls.Add(this._selectLabel);
        }

        private IComponent GetComponent()
        {
            if (this._controlDesigner != null)
            {
                return this._controlDesigner.Component;
            }
            return null;
        }

        private string GetNewDataSourceName(System.Type dataSourceType)
        {
            if (dataSourceType != null)
            {
                ISite site = this.GetSite();
                if (site != null)
                {
                    INameCreationService service = (INameCreationService) site.GetService(typeof(INameCreationService));
                    if (service != null)
                    {
                        return service.CreateName(site.Container, dataSourceType);
                    }
                    return (site.Name + "_DataSource");
                }
            }
            return string.Empty;
        }

        private ISite GetSite()
        {
            IComponent component = this.GetComponent();
            if (component != null)
            {
                return component.Site;
            }
            return null;
        }

        private void InitializePanelControls()
        {
            this._selectLabel.Text = System.Design.SR.GetString("CreateDataSource_SelectType");
            this._idLabel.Text = System.Design.SR.GetString("CreateDataSource_ID");
            base.OKButton.Enabled = false;
            this.Text = System.Design.SR.GetString("CreateDataSource_Title");
            this._descriptionBox.Text = System.Design.SR.GetString("CreateDataSource_SelectTypeDesc");
            base.AccessibleDescription = System.Design.SR.GetString("CreateDataSource_Description");
            base.CaptionLabel.Text = System.Design.SR.GetString("CreateDataSource_Caption");
            this.UpdateFonts();
            ISite site = this.GetSite();
            if (site != null)
            {
                IComponentDiscoveryService service = (IComponentDiscoveryService) site.GetService(typeof(IComponentDiscoveryService));
                IDesignerHost designerHost = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                if (service != null)
                {
                    ICollection componentTypes = service.GetComponentTypes(designerHost, this._dataSourceType);
                    if (componentTypes != null)
                    {
                        ImageList list = new ImageList {
                            ColorDepth = ColorDepth.Depth32Bit
                        };
                        System.Type[] array = new System.Type[componentTypes.Count];
                        componentTypes.CopyTo(array, 0);
                        foreach (System.Type type in array)
                        {
                            System.ComponentModel.AttributeCollection attributes = TypeDescriptor.GetAttributes(type);
                            Bitmap image = null;
                            if (attributes != null)
                            {
                                ToolboxBitmapAttribute attribute = attributes[typeof(ToolboxBitmapAttribute)] as ToolboxBitmapAttribute;
                                if ((attribute != null) && !attribute.Equals(ToolboxBitmapAttribute.Default))
                                {
                                    image = attribute.GetImage(type, true) as Bitmap;
                                }
                            }
                            if (image == null)
                            {
                                image = new Bitmap(base.GetType(), "CustomDataSource.bmp");
                            }
                            list.ImageSize = new Size(0x20, 0x20);
                            list.Images.Add(type.FullName, image);
                            this._dataSourceTypesListView.Items.Add(new DataSourceListViewItem(type));
                        }
                        this._dataSourceTypesListView.Sort();
                        this._dataSourceTypesListView.LargeImageList = list;
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if ((base.DialogResult == DialogResult.OK) && (this._dataSourceTypesListView.SelectedItems.Count > 0))
            {
                DataSourceListViewItem item = this._dataSourceTypesListView.SelectedItems[0] as DataSourceListViewItem;
                System.Type dataSourceType = item.DataSourceType;
                string str = this.CreateNewDataSource(dataSourceType);
                if (str.Length > 0)
                {
                    this._dataSourceID = str;
                }
                else
                {
                    e.Cancel = true;
                }
                TypeDescriptor.Refresh(this.GetComponent());
            }
        }

        private void OnDataSourceTypeChosen(object sender, EventArgs e)
        {
            if (this._dataSourceTypesListView.SelectedItems.Count > 0)
            {
                DataSourceListViewItem item = this._dataSourceTypesListView.SelectedItems[0] as DataSourceListViewItem;
                System.Type dataSourceType = item.DataSourceType;
                this._idTextBox.Text = this.GetNewDataSourceName(dataSourceType);
                this._descriptionBox.Text = item.GetDescriptionText();
            }
            this.UpdateOKButtonEnabled();
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.UpdateFonts();
        }

        private void OnIDChanged(object sender, EventArgs e)
        {
            this.UpdateOKButtonEnabled();
        }

        private void OnListViewDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }
        }

        private void UpdateFonts()
        {
            this._selectLabel.Font = new Font(this.Font, FontStyle.Bold);
        }

        private void UpdateOKButtonEnabled()
        {
            if ((this._idTextBox.Text.Length > 0) && (this._dataSourceTypesListView.SelectedItems.Count > 0))
            {
                base.OKButton.Enabled = true;
            }
            else
            {
                base.OKButton.Enabled = false;
            }
        }

        public string DataSourceID
        {
            get
            {
                if (this._dataSourceID == null)
                {
                    return string.Empty;
                }
                return this._dataSourceID;
            }
        }

        protected override string HelpTopic
        {
            get
            {
                return "net.Asp.DataBoundControl.CreateDataSourceDialog";
            }
        }

        private class DataSourceListViewItem : ListViewItem
        {
            private System.Type _dataSourceType;
            private string _displayName;

            public DataSourceListViewItem(System.Type dataSourceType)
            {
                this._dataSourceType = dataSourceType;
                base.Text = this.GetDisplayName();
                base.ImageKey = this._dataSourceType.FullName;
            }

            public string GetDescriptionText()
            {
                System.ComponentModel.AttributeCollection attributes = TypeDescriptor.GetAttributes(this._dataSourceType);
                if (attributes != null)
                {
                    DescriptionAttribute attribute = attributes[typeof(DescriptionAttribute)] as DescriptionAttribute;
                    if (attribute != null)
                    {
                        return attribute.Description;
                    }
                }
                return string.Empty;
            }

            public string GetDisplayName()
            {
                if (this._displayName == null)
                {
                    System.ComponentModel.AttributeCollection attributes = TypeDescriptor.GetAttributes(this._dataSourceType);
                    this._displayName = string.Empty;
                    if (attributes != null)
                    {
                        DisplayNameAttribute attribute = attributes[typeof(DisplayNameAttribute)] as DisplayNameAttribute;
                        if (attribute != null)
                        {
                            this._displayName = attribute.DisplayName;
                        }
                    }
                    if (string.IsNullOrEmpty(this._displayName))
                    {
                        this._displayName = this._dataSourceType.Name;
                    }
                }
                return this._displayName;
            }

            public System.Type DataSourceType
            {
                get
                {
                    return this._dataSourceType;
                }
            }
        }

        private class DisplayNameComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if ((x is CreateDataSourceDialog.DataSourceListViewItem) && (y is CreateDataSourceDialog.DataSourceListViewItem))
                {
                    return this.Compare((CreateDataSourceDialog.DataSourceListViewItem) x, (CreateDataSourceDialog.DataSourceListViewItem) y);
                }
                return 0;
            }

            private int Compare(CreateDataSourceDialog.DataSourceListViewItem x, CreateDataSourceDialog.DataSourceListViewItem y)
            {
                return StringComparer.Create(CultureInfo.CurrentCulture, true).Compare(x.GetDisplayName(), y.GetDisplayName());
            }
        }
    }
}

