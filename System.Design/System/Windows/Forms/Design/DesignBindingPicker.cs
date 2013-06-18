namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Data;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Windows.Forms;

    [DesignTimeVisible(false), ToolboxItem(false)]
    internal class DesignBindingPicker : ContainerControl
    {
        private BindingPickerLink addNewCtrl;
        private Panel addNewPanel;
        private BindingContext bindingContext = new BindingContext();
        private ITypeDescriptorContext context;
        private DataSourceProviderService dspSvc;
        private IWindowsFormsEditorService edSvc;
        private HelpTextLabel helpTextCtrl;
        private Panel helpTextPanel;
        private IDesignerHost hostSvc;
        private bool inSelectNode;
        private InstancesNode instancesNode;
        private const int minimumHeight = 250;
        private const int minimumWidth = 250;
        private NoneNode noneNode;
        private OtherNode otherNode;
        private ProjectNode projectNode;
        private string rootDataMember;
        private object rootDataSource;
        private static System.Type runtimeType = typeof(object).GetType().GetType();
        private DesignBinding selectedItem;
        private TreeNode selectedNode;
        private bool selectListMembers;
        private IServiceProvider serviceProvider;
        private bool showDataMembers;
        private bool showDataSources;
        private BindingPickerTree treeViewCtrl;
        private ITypeResolutionService typeSvc;

        public DesignBindingPicker()
        {
            base.SuspendLayout();
            this.treeViewCtrl = new BindingPickerTree();
            this.treeViewCtrl.HotTracking = true;
            this.treeViewCtrl.BackColor = SystemColors.Window;
            this.treeViewCtrl.ForeColor = SystemColors.WindowText;
            this.treeViewCtrl.BorderStyle = BorderStyle.None;
            Size size = this.treeViewCtrl.Size;
            this.treeViewCtrl.Dock = DockStyle.Fill;
            this.treeViewCtrl.MouseMove += new MouseEventHandler(this.treeViewCtrl_MouseMove);
            this.treeViewCtrl.MouseLeave += new EventHandler(this.treeViewCtrl_MouseLeave);
            this.treeViewCtrl.AfterExpand += new TreeViewEventHandler(this.treeViewCtrl_AfterExpand);
            this.treeViewCtrl.AccessibleName = System.Design.SR.GetString("DesignBindingPickerTreeViewAccessibleName");
            DesignerUtils.ApplyTreeViewThemeStyles(this.treeViewCtrl);
            Label label = new Label {
                Height = 1,
                BackColor = SystemColors.ControlDark,
                Dock = DockStyle.Top
            };
            this.addNewCtrl = new BindingPickerLink();
            this.addNewCtrl.Text = System.Design.SR.GetString("DesignBindingPickerAddProjDataSourceLabel");
            this.addNewCtrl.TextAlign = ContentAlignment.MiddleLeft;
            this.addNewCtrl.BackColor = SystemColors.Window;
            this.addNewCtrl.ForeColor = SystemColors.WindowText;
            this.addNewCtrl.LinkBehavior = LinkBehavior.HoverUnderline;
            int height = this.addNewCtrl.Height;
            this.addNewCtrl.Dock = DockStyle.Fill;
            this.addNewCtrl.LinkClicked += new LinkLabelLinkClickedEventHandler(this.addNewCtrl_Click);
            Bitmap bitmap = new Bitmap(typeof(DesignBindingPicker), "AddNewDataSource.bmp");
            bitmap.MakeTransparent(Color.Magenta);
            PictureBox box = new PictureBox {
                Image = bitmap,
                BackColor = SystemColors.Window,
                ForeColor = SystemColors.WindowText,
                Width = height,
                Height = height,
                Dock = DockStyle.Left,
                SizeMode = PictureBoxSizeMode.CenterImage,
                AccessibleRole = AccessibleRole.Graphic
            };
            this.addNewPanel = new Panel();
            this.addNewPanel.Controls.Add(this.addNewCtrl);
            this.addNewPanel.Controls.Add(box);
            this.addNewPanel.Controls.Add(label);
            this.addNewPanel.Height = height + 1;
            this.addNewPanel.Dock = DockStyle.Bottom;
            Label label2 = new Label {
                Height = 1,
                BackColor = SystemColors.ControlDark,
                Dock = DockStyle.Top
            };
            this.helpTextCtrl = new HelpTextLabel();
            this.helpTextCtrl.TextAlign = ContentAlignment.TopLeft;
            this.helpTextCtrl.BackColor = SystemColors.Window;
            this.helpTextCtrl.ForeColor = SystemColors.WindowText;
            this.helpTextCtrl.Height *= 2;
            int num2 = this.helpTextCtrl.Height;
            this.helpTextCtrl.Dock = DockStyle.Fill;
            this.helpTextPanel = new Panel();
            this.helpTextPanel.Controls.Add(this.helpTextCtrl);
            this.helpTextPanel.Controls.Add(label2);
            this.helpTextPanel.Height = num2 + 1;
            this.helpTextPanel.Dock = DockStyle.Bottom;
            base.Controls.Add(this.treeViewCtrl);
            base.Controls.Add(this.addNewPanel);
            base.Controls.Add(this.helpTextPanel);
            base.ResumeLayout(false);
            base.Size = size;
            this.BackColor = SystemColors.Control;
            base.ActiveControl = this.treeViewCtrl;
            base.AccessibleName = System.Design.SR.GetString("DesignBindingPickerAccessibleName");
            base.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        }

        private void AddDataMember(TreeNodeCollection nodes, object dataSource, string dataMember, string propertyName, bool isList)
        {
            bool flag = isList && (dataSource is BindingSource);
            bool flag2 = this.showDataMembers && !this.selectListMembers;
            bool flag3 = flag && flag2;
            bool flag4 = (flag && !flag2) || (this.context.Instance is BindingSource);
            if (!flag3 && (!this.selectListMembers || isList))
            {
                DataMemberNode node = new DataMemberNode(this, dataSource, dataMember, propertyName, isList);
                nodes.Add(node);
                if (((this.selectedItem != null) && this.selectedItem.Equals(dataSource, dataMember)) && (node != null))
                {
                    this.selectedNode = node;
                }
                if (!flag4)
                {
                    this.AddDataMemberContents(node);
                }
            }
        }

        private void AddDataMemberContents(DataMemberNode dataMemberNode)
        {
            this.AddDataMemberContents(dataMemberNode.Nodes, dataMemberNode);
        }

        private void AddDataMemberContents(TreeNodeCollection nodes, DataMemberNode dataMemberNode)
        {
            this.AddDataMemberContents(nodes, dataMemberNode.DataSource, dataMemberNode.DataMember, dataMemberNode.IsList);
        }

        private void AddDataMemberContents(TreeNodeCollection nodes, object dataSource, string dataMember, bool isList)
        {
            if (isList)
            {
                PropertyDescriptorCollection itemProperties = this.GetItemProperties(dataSource, dataMember);
                if (itemProperties != null)
                {
                    for (int i = 0; i < itemProperties.Count; i++)
                    {
                        PropertyDescriptor property = itemProperties[i];
                        if (this.IsBindableDataMember(property))
                        {
                            bool flag = this.IsListMember(property);
                            if (!this.selectListMembers || flag)
                            {
                                DataMemberNode node = new DataMemberNode(this, dataSource, dataMember + "." + property.Name, property.Name, flag);
                                nodes.Add(node);
                                if ((this.selectedItem != null) && (this.selectedItem.DataSource == node.DataSource))
                                {
                                    if (this.selectedItem.Equals(dataSource, node.DataMember))
                                    {
                                        this.selectedNode = node;
                                    }
                                    else if (!string.IsNullOrEmpty(this.selectedItem.DataMember) && (this.selectedItem.DataMember.IndexOf(node.DataMember) == 0))
                                    {
                                        this.AddDataMemberContents(node);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddDataSource(TreeNodeCollection nodes, IComponent dataSource, string dataMember)
        {
            if (this.showDataSources && this.IsBindableDataSource(dataSource))
            {
                string message = null;
                PropertyDescriptorCollection properties = null;
                try
                {
                    properties = this.GetItemProperties(dataSource, dataMember);
                    if (properties == null)
                    {
                        return;
                    }
                }
                catch (ArgumentException exception)
                {
                    message = exception.Message;
                }
                if (!this.showDataMembers || (properties.Count != 0))
                {
                    DataSourceNode node = new DataSourceNode(this, dataSource, dataSource.Site.Name);
                    nodes.Add(node);
                    if ((this.selectedItem != null) && this.selectedItem.Equals(dataSource, ""))
                    {
                        this.selectedNode = node;
                    }
                    if (message == null)
                    {
                        this.AddDataSourceContents(node.Nodes, dataSource, dataMember, properties);
                        node.SubNodesFilled = true;
                    }
                    else
                    {
                        node.Error = message;
                        node.ForeColor = SystemColors.GrayText;
                    }
                }
            }
        }

        private void AddDataSourceContents(TreeNodeCollection nodes, object dataSource, string dataMember, PropertyDescriptorCollection properties)
        {
            if (this.showDataMembers || (dataSource is BindingSource))
            {
                if (dataSource is System.Type)
                {
                    try
                    {
                        BindingSource source = new BindingSource {
                            DataSource = dataSource
                        };
                        dataSource = source.List;
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                }
                if (this.IsBindableDataSource(dataSource))
                {
                    if (properties == null)
                    {
                        properties = this.GetItemProperties(dataSource, dataMember);
                        if (properties == null)
                        {
                            return;
                        }
                    }
                    for (int i = 0; i < properties.Count; i++)
                    {
                        PropertyDescriptor property = properties[i];
                        if (this.IsBindableDataMember(property))
                        {
                            string str = string.IsNullOrEmpty(dataMember) ? property.Name : (dataMember + "." + property.Name);
                            this.AddDataMember(nodes, dataSource, str, property.Name, this.IsListMember(property));
                        }
                    }
                }
            }
        }

        private void AddFormDataSources()
        {
            IContainer container = null;
            if (this.context != null)
            {
                container = this.context.Container;
            }
            if ((container == null) && (this.hostSvc != null))
            {
                container = this.hostSvc.Container;
            }
            if (container != null)
            {
                ComponentCollection components = DesignerUtils.CheckForNestedContainer(container).Components;
                foreach (IComponent component in components)
                {
                    if ((component != this.context.Instance) && (!(component is DataTable) || !this.FindComponent(components, (component as DataTable).DataSet)))
                    {
                        if (component is BindingSource)
                        {
                            this.AddDataSource(this.treeViewCtrl.Nodes, component, null);
                        }
                        else
                        {
                            this.AddDataSource(this.instancesNode.Nodes, component, null);
                        }
                    }
                }
            }
        }

        private void addNewCtrl_Click(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ((this.dspSvc != null) && this.dspSvc.SupportsAddNewDataSource)
            {
                DataSourceGroup group = this.dspSvc.InvokeAddNewDataSource(this, FormStartPosition.CenterScreen);
                if ((group != null) && (group.DataSources.Count != 0))
                {
                    DataSourceDescriptor dataSource = group.DataSources[0];
                    this.FillTree(new DesignBinding(dataSource, ""));
                    if (this.selectedNode != null)
                    {
                        int count = this.selectedNode.Nodes.Count;
                        if (this.context.Instance is BindingSource)
                        {
                            this.treeViewCtrl.SetSelectedItem(this.selectedNode);
                        }
                        if ((count == 0) || (this.context.Instance is BindingSource))
                        {
                            this.treeViewCtrl.SetSelectedItem(this.selectedNode);
                        }
                        else if (count == 1)
                        {
                            this.treeViewCtrl.SetSelectedItem(this.selectedNode.Nodes[0]);
                        }
                        else
                        {
                            this.ShowSelectedNode();
                            this.selectedNode.Expand();
                            this.selectedNode = null;
                            this.UpdateHelpText(null);
                        }
                    }
                }
            }
        }

        private void AddProjectDataMember(TreeNodeCollection nodes, DataSourceDescriptor dsd, PropertyDescriptor pd, object dataSourceInstance, bool isList)
        {
            System.Type type = this.GetType(dsd.TypeName, true, true);
            if ((type == null) || (type.GetType() == runtimeType))
            {
                DataMemberNode node = new ProjectDataMemberNode(this, dsd, pd.Name, pd.Name, isList);
                nodes.Add(node);
                this.AddProjectDataMemberContents(node, dsd, pd, dataSourceInstance);
            }
        }

        private void AddProjectDataMemberContents(DataMemberNode projectDataMemberNode, DataSourceDescriptor dsd, PropertyDescriptor pd, object dataSourceInstance)
        {
            this.AddProjectDataMemberContents(projectDataMemberNode.Nodes, projectDataMemberNode, dsd, pd, dataSourceInstance);
        }

        private void AddProjectDataMemberContents(TreeNodeCollection nodes, DataMemberNode projectDataMemberNode, DataSourceDescriptor dsd, PropertyDescriptor propDesc, object dataSourceInstance)
        {
            if ((!this.selectListMembers && projectDataMemberNode.IsList) && (dataSourceInstance != null))
            {
                PropertyDescriptorCollection listItemProperties = ListBindingHelper.GetListItemProperties(dataSourceInstance, new PropertyDescriptor[] { propDesc });
                if (listItemProperties != null)
                {
                    foreach (PropertyDescriptor descriptor in listItemProperties)
                    {
                        if (this.IsBindableDataMember(descriptor) && descriptor.IsBrowsable)
                        {
                            bool isList = this.IsListMember(descriptor);
                            if (!isList)
                            {
                                this.AddProjectDataMember(nodes, dsd, descriptor, dataSourceInstance, isList);
                            }
                        }
                    }
                }
            }
        }

        private void AddProjectDataSource(TreeNodeCollection nodes, DataSourceDescriptor dsd)
        {
            System.Type type = this.GetType(dsd.TypeName, true, true);
            if ((type == null) || (type.GetType() == runtimeType))
            {
                ProjectDataSourceNode node = new ProjectDataSourceNode(this, dsd, dsd.Name, dsd.Image);
                nodes.Add(node);
                if ((this.selectedItem != null) && string.IsNullOrEmpty(this.selectedItem.DataMember))
                {
                    if ((this.selectedItem.DataSource is DataSourceDescriptor) && string.Equals(dsd.Name, (this.selectedItem.DataSource as DataSourceDescriptor).Name, StringComparison.OrdinalIgnoreCase))
                    {
                        this.selectedNode = node;
                    }
                    else if ((this.selectedItem.DataSource is System.Type) && string.Equals(dsd.TypeName, (this.selectedItem.DataSource as System.Type).FullName, StringComparison.OrdinalIgnoreCase))
                    {
                        this.selectedNode = node;
                    }
                }
            }
        }

        private void AddProjectDataSourceContents(DataSourceNode projectDataSourceNode)
        {
            this.AddProjectDataSourceContents(projectDataSourceNode.Nodes, projectDataSourceNode);
        }

        private void AddProjectDataSourceContents(TreeNodeCollection nodes, DataSourceNode projectDataSourceNode)
        {
            DataSourceDescriptor dataSource = projectDataSourceNode.DataSource as DataSourceDescriptor;
            if (dataSource != null)
            {
                System.Type type = this.GetType(dataSource.TypeName, false, false);
                if (type != null)
                {
                    object list = type;
                    try
                    {
                        list = Activator.CreateInstance(type);
                    }
                    catch (Exception exception)
                    {
                        if (System.Windows.Forms.ClientUtils.IsCriticalException(exception))
                        {
                            throw;
                        }
                    }
                    bool flag = (list is IListSource) && (list as IListSource).ContainsListCollection;
                    if (!flag || !(this.context.Instance is BindingSource))
                    {
                        PropertyDescriptorCollection listItemProperties = ListBindingHelper.GetListItemProperties(list);
                        if (listItemProperties != null)
                        {
                            foreach (PropertyDescriptor descriptor2 in listItemProperties)
                            {
                                if (this.IsBindableDataMember(descriptor2) && descriptor2.IsBrowsable)
                                {
                                    bool isList = this.IsListMember(descriptor2);
                                    if ((!this.selectListMembers || isList) && (flag || !isList))
                                    {
                                        this.AddProjectDataMember(nodes, dataSource, descriptor2, list, isList);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddProjectDataSources()
        {
            if (this.dspSvc != null)
            {
                DataSourceGroupCollection dataSources = this.dspSvc.GetDataSources();
                if (dataSources != null)
                {
                    bool addMembers = (this.selectedItem != null) && (this.selectedItem.DataSource is DataSourceDescriptor);
                    foreach (DataSourceGroup group in dataSources)
                    {
                        if (group != null)
                        {
                            if (group.IsDefault)
                            {
                                this.AddProjectGroupContents(this.projectNode.Nodes, group);
                            }
                            else
                            {
                                this.AddProjectGroup(this.projectNode.Nodes, group, addMembers);
                            }
                        }
                    }
                    if (addMembers)
                    {
                        this.projectNode.FillSubNodes();
                    }
                }
            }
        }

        private void AddProjectGroup(TreeNodeCollection nodes, DataSourceGroup group, bool addMembers)
        {
            ProjectGroupNode node = new ProjectGroupNode(this, group.Name, group.Image);
            this.AddProjectGroupContents(node.Nodes, group);
            nodes.Add(node);
            if (addMembers)
            {
                node.FillSubNodes();
            }
        }

        private void AddProjectGroupContents(TreeNodeCollection nodes, DataSourceGroup group)
        {
            DataSourceDescriptorCollection dataSources = group.DataSources;
            if (dataSources != null)
            {
                foreach (DataSourceDescriptor descriptor in dataSources)
                {
                    if (descriptor != null)
                    {
                        this.AddProjectDataSource(nodes, descriptor);
                    }
                }
            }
        }

        private void CloseDropDown()
        {
            if ((this.context.Instance is BindingSource) && (this.hostSvc != null))
            {
                BindingSourceDesigner designer = this.hostSvc.GetDesigner(this.context.Instance as IComponent) as BindingSourceDesigner;
                if (designer != null)
                {
                    designer.BindingUpdatedByUser = true;
                }
            }
            if (this.edSvc != null)
            {
                this.edSvc.CloseDropDown();
            }
        }

        private BindingSource CreateNewBindingSource(DataSourceDescriptor dataSourceDescriptor, string dataMember)
        {
            if ((this.hostSvc == null) || (this.dspSvc == null))
            {
                return null;
            }
            object projectDataSourceInstance = this.GetProjectDataSourceInstance(dataSourceDescriptor);
            if (projectDataSourceInstance == null)
            {
                return null;
            }
            return this.CreateNewBindingSource(projectDataSourceInstance, dataMember);
        }

        private BindingSource CreateNewBindingSource(object dataSource, string dataMember)
        {
            if ((this.hostSvc == null) || (this.dspSvc == null))
            {
                return null;
            }
            BindingSource component = new BindingSource();
            try
            {
                component.DataSource = dataSource;
                component.DataMember = dataMember;
            }
            catch (Exception exception)
            {
                IUIService uiService = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                DataGridViewDesigner.ShowErrorDialog(uiService, exception, this);
                return null;
            }
            string bindingSourceNamePrefix = this.GetBindingSourceNamePrefix(dataSource, dataMember);
            if (this.serviceProvider != null)
            {
                bindingSourceNamePrefix = ToolStripDesigner.NameFromText(bindingSourceNamePrefix, component.GetType(), this.serviceProvider);
            }
            else
            {
                bindingSourceNamePrefix = bindingSourceNamePrefix + component.GetType().Name;
            }
            string uniqueSiteName = DesignerUtils.GetUniqueSiteName(this.hostSvc, bindingSourceNamePrefix);
            DesignerTransaction transaction = this.hostSvc.CreateTransaction(System.Design.SR.GetString("DesignerBatchCreateTool", new object[] { uniqueSiteName }));
            try
            {
                try
                {
                    this.hostSvc.Container.Add(component, uniqueSiteName);
                }
                catch (InvalidOperationException exception2)
                {
                    if (transaction != null)
                    {
                        transaction.Cancel();
                    }
                    IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    DataGridViewDesigner.ShowErrorDialog(service, exception2, this);
                    return null;
                }
                catch (CheckoutException exception3)
                {
                    if (transaction != null)
                    {
                        transaction.Cancel();
                    }
                    IUIService service3 = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    DataGridViewDesigner.ShowErrorDialog(service3, exception3, this);
                    return null;
                }
                this.dspSvc.NotifyDataSourceComponentAdded(component);
                if (transaction != null)
                {
                    transaction.Commit();
                    transaction = null;
                }
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Cancel();
                }
            }
            return component;
        }

        private void EmptyTree()
        {
            this.noneNode = null;
            this.otherNode = null;
            this.projectNode = null;
            this.instancesNode = null;
            this.selectedNode = null;
            this.treeViewCtrl.Nodes.Clear();
        }

        private void FillTree(DesignBinding initialSelectedItem)
        {
            this.selectedItem = initialSelectedItem;
            this.EmptyTree();
            this.noneNode = new NoneNode();
            this.otherNode = new OtherNode();
            this.projectNode = new ProjectNode(this);
            if (((this.hostSvc != null) && (this.hostSvc.RootComponent != null)) && (this.hostSvc.RootComponent.Site != null))
            {
                this.instancesNode = new InstancesNode(this.hostSvc.RootComponent.Site.Name);
            }
            else
            {
                this.instancesNode = new InstancesNode(string.Empty);
            }
            this.treeViewCtrl.Nodes.Add(this.noneNode);
            if (this.showDataSources)
            {
                this.AddFormDataSources();
                this.AddProjectDataSources();
                if (this.projectNode.Nodes.Count > 0)
                {
                    this.otherNode.Nodes.Add(this.projectNode);
                }
                if (this.instancesNode.Nodes.Count > 0)
                {
                    this.otherNode.Nodes.Add(this.instancesNode);
                }
                if (this.otherNode.Nodes.Count > 0)
                {
                    this.treeViewCtrl.Nodes.Add(this.otherNode);
                }
            }
            else
            {
                this.AddDataSourceContents(this.treeViewCtrl.Nodes, this.rootDataSource, this.rootDataMember, null);
            }
            if (this.selectedNode == null)
            {
                this.selectedNode = this.noneNode;
            }
            this.selectedItem = null;
            base.Width = Math.Max(base.Width, this.treeViewCtrl.PreferredWidth + (SystemInformation.VerticalScrollBarWidth * 2));
        }

        private bool FindComponent(ComponentCollection components, IComponent targetComponent)
        {
            foreach (IComponent component in components)
            {
                if (component == targetComponent)
                {
                    return true;
                }
            }
            return false;
        }

        private string GetBindingSourceNamePrefix(object dataSource, string dataMember)
        {
            if (!string.IsNullOrEmpty(dataMember))
            {
                return dataMember;
            }
            if (dataSource == null)
            {
                return "";
            }
            System.Type type = dataSource as System.Type;
            if (type != null)
            {
                return type.Name;
            }
            IComponent component = dataSource as IComponent;
            if (component != null)
            {
                ISite site = component.Site;
                if ((site != null) && !string.IsNullOrEmpty(site.Name))
                {
                    return site.Name;
                }
            }
            return dataSource.GetType().Name;
        }

        private PropertyDescriptorCollection GetItemProperties(object dataSource, string dataMember)
        {
            CurrencyManager manager = (CurrencyManager) this.bindingContext[dataSource, dataMember];
            if (manager != null)
            {
                return manager.GetItemProperties();
            }
            return null;
        }

        private object GetProjectDataSourceInstance(DataSourceDescriptor dataSourceDescriptor)
        {
            System.Type type = this.GetType(dataSourceDescriptor.TypeName, true, true);
            if (!dataSourceDescriptor.IsDesignable)
            {
                return type;
            }
            foreach (IComponent component in this.hostSvc.Container.Components)
            {
                if (type.Equals(component.GetType()))
                {
                    return component;
                }
            }
            try
            {
                return this.dspSvc.AddDataSourceInstance(this.hostSvc, dataSourceDescriptor);
            }
            catch (InvalidOperationException exception)
            {
                IUIService uiService = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                DataGridViewDesigner.ShowErrorDialog(uiService, exception, this);
                return null;
            }
            catch (CheckoutException exception2)
            {
                IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
                DataGridViewDesigner.ShowErrorDialog(service, exception2, this);
                return null;
            }
        }

        private System.Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            if (this.typeSvc != null)
            {
                return this.typeSvc.GetType(name, throwOnError, ignoreCase);
            }
            return System.Type.GetType(name, throwOnError, ignoreCase);
        }

        private bool IsBindableDataMember(PropertyDescriptor property)
        {
            if (!typeof(byte[]).IsAssignableFrom(property.PropertyType))
            {
                ListBindableAttribute attribute = (ListBindableAttribute) property.Attributes[typeof(ListBindableAttribute)];
                if ((attribute != null) && !attribute.ListBindable)
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsBindableDataSource(object dataSource)
        {
            if ((!(dataSource is IListSource) && !(dataSource is IList)) && !(dataSource is Array))
            {
                return false;
            }
            ListBindableAttribute attribute = (ListBindableAttribute) TypeDescriptor.GetAttributes(dataSource)[typeof(ListBindableAttribute)];
            if ((attribute != null) && !attribute.ListBindable)
            {
                return false;
            }
            return true;
        }

        private bool IsListMember(PropertyDescriptor property)
        {
            if (typeof(byte[]).IsAssignableFrom(property.PropertyType))
            {
                return false;
            }
            return typeof(IList).IsAssignableFrom(property.PropertyType);
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.treeViewCtrl.Focus();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (base.Visible)
            {
                this.ShowSelectedNode();
            }
        }

        public DesignBinding Pick(ITypeDescriptorContext context, IServiceProvider provider, bool showDataSources, bool showDataMembers, bool selectListMembers, object rootDataSource, string rootDataMember, DesignBinding initialSelectedItem)
        {
            this.serviceProvider = provider;
            this.edSvc = (IWindowsFormsEditorService) this.serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            this.dspSvc = (DataSourceProviderService) this.serviceProvider.GetService(typeof(DataSourceProviderService));
            this.typeSvc = (ITypeResolutionService) this.serviceProvider.GetService(typeof(ITypeResolutionService));
            this.hostSvc = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
            if (this.edSvc == null)
            {
                return null;
            }
            this.context = context;
            this.showDataSources = showDataSources;
            this.showDataMembers = showDataMembers;
            this.selectListMembers = showDataMembers ? selectListMembers : true;
            this.rootDataSource = rootDataSource;
            this.rootDataMember = rootDataMember;
            IUIService service = this.serviceProvider.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                if (service.Styles["VsColorPanelHyperLink"] is Color)
                {
                    this.addNewCtrl.LinkColor = (Color) service.Styles["VsColorPanelHyperLink"];
                }
                if (service.Styles["VsColorPanelHyperLinkPressed"] is Color)
                {
                    this.addNewCtrl.ActiveLinkColor = (Color) service.Styles["VsColorPanelHyperLinkPressed"];
                }
            }
            this.FillTree(initialSelectedItem);
            this.addNewPanel.Visible = (showDataSources && (this.dspSvc != null)) && this.dspSvc.SupportsAddNewDataSource;
            this.helpTextPanel.Visible = showDataSources;
            this.UpdateHelpText(null);
            this.edSvc.DropDownControl(this);
            DesignBinding selectedItem = this.selectedItem;
            this.selectedItem = null;
            this.EmptyTree();
            this.serviceProvider = null;
            this.edSvc = null;
            this.dspSvc = null;
            this.hostSvc = null;
            context = null;
            return selectedItem;
        }

        private void PostSelectTreeNode(TreeNode node)
        {
            if ((node != null) && base.IsHandleCreated)
            {
                base.BeginInvoke(new PostSelectTreeNodeDelegate(this.PostSelectTreeNodeCallback), new object[] { node });
            }
        }

        private void PostSelectTreeNodeCallback(TreeNode node)
        {
            this.SelectTreeNode(null);
            this.SelectTreeNode(node);
        }

        private void SelectTreeNode(TreeNode node)
        {
            if (!this.inSelectNode)
            {
                try
                {
                    this.inSelectNode = true;
                    this.treeViewCtrl.BeginUpdate();
                    this.treeViewCtrl.SelectedNode = node;
                    this.treeViewCtrl.EndUpdate();
                }
                finally
                {
                    this.inSelectNode = false;
                }
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            if ((specified & BoundsSpecified.Width) == BoundsSpecified.Width)
            {
                width = Math.Max(width, 250);
            }
            if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height)
            {
                height = Math.Max(height, 250);
            }
            base.SetBoundsCore(x, y, width, height, specified);
        }

        private void ShowSelectedNode()
        {
            this.PostSelectTreeNode(this.selectedNode);
        }

        private void treeViewCtrl_AfterExpand(object sender, TreeViewEventArgs tvcevent)
        {
            if (!this.inSelectNode && base.Visible)
            {
                (tvcevent.Node as BindingPickerNode).OnExpand();
            }
        }

        private void treeViewCtrl_MouseLeave(object sender, EventArgs e)
        {
            this.UpdateHelpText(null);
        }

        private void treeViewCtrl_MouseMove(object sender, MouseEventArgs e)
        {
            Point pt = new Point(e.X, e.Y);
            TreeNode nodeAt = this.treeViewCtrl.GetNodeAt(pt);
            if ((nodeAt != null) && !nodeAt.Bounds.Contains(pt))
            {
                nodeAt = null;
            }
            this.UpdateHelpText(nodeAt as BindingPickerNode);
        }

        private void UpdateHelpText(BindingPickerNode mouseNode)
        {
            string str = (mouseNode == null) ? null : mouseNode.HelpText;
            string str2 = (mouseNode == null) ? null : mouseNode.Error;
            if ((str != null) || (str2 != null))
            {
                this.helpTextCtrl.BackColor = SystemColors.Info;
                this.helpTextCtrl.ForeColor = SystemColors.InfoText;
            }
            else
            {
                this.helpTextCtrl.BackColor = SystemColors.Window;
                this.helpTextCtrl.ForeColor = SystemColors.WindowText;
            }
            if (str2 != null)
            {
                this.helpTextCtrl.Text = str2;
            }
            else if (str != null)
            {
                this.helpTextCtrl.Text = str;
            }
            else if ((this.selectedNode != null) && (this.selectedNode != this.noneNode))
            {
                this.helpTextCtrl.Text = string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("DesignBindingPickerHelpGenCurrentBinding"), new object[] { this.selectedNode.Text });
            }
            else if (!this.showDataSources)
            {
                this.helpTextCtrl.Text = (this.treeViewCtrl.Nodes.Count > 1) ? System.Design.SR.GetString("DesignBindingPickerHelpGenPickMember") : "";
            }
            else if ((this.treeViewCtrl.Nodes.Count > 1) && (this.treeViewCtrl.Nodes[1] is DataSourceNode))
            {
                this.helpTextCtrl.Text = System.Design.SR.GetString("DesignBindingPickerHelpGenPickBindSrc");
            }
            else if ((this.instancesNode.Nodes.Count > 0) || (this.projectNode.Nodes.Count > 0))
            {
                this.helpTextCtrl.Text = System.Design.SR.GetString("DesignBindingPickerHelpGenPickDataSrc");
            }
            else if (this.addNewPanel.Visible)
            {
                this.helpTextCtrl.Text = System.Design.SR.GetString("DesignBindingPickerHelpGenAddDataSrc");
            }
            else
            {
                this.helpTextCtrl.Text = "";
            }
        }

        internal class BindingPickerLink : LinkLabel
        {
            protected override bool IsInputKey(Keys key)
            {
                if (key != Keys.Enter)
                {
                    return base.IsInputKey(key);
                }
                return true;
            }
        }

        internal class BindingPickerNode : TreeNode
        {
            private string error;
            protected DesignBindingPicker picker;
            private bool subNodesFilled;

            public BindingPickerNode(DesignBindingPicker picker, string nodeName) : base(nodeName)
            {
                this.picker = picker;
            }

            public BindingPickerNode(DesignBindingPicker picker, string nodeName, BindingImage index) : base(nodeName)
            {
                this.picker = picker;
                this.BindingImageIndex = (int) index;
            }

            public static BindingImage BindingImageIndexForDataSource(object dataSource)
            {
                if (dataSource is BindingSource)
                {
                    return BindingImage.BindingSource;
                }
                IListSource source = dataSource as IListSource;
                if (source != null)
                {
                    if (source.ContainsListCollection)
                    {
                        return BindingImage.DataSource;
                    }
                    return BindingImage.ListMember;
                }
                if (dataSource is IList)
                {
                    return BindingImage.ListMember;
                }
                return BindingImage.FieldMember;
            }

            public virtual void Fill()
            {
            }

            public virtual void FillSubNodes()
            {
                if (!this.SubNodesFilled)
                {
                    foreach (DesignBindingPicker.BindingPickerNode node in base.Nodes)
                    {
                        node.Fill();
                    }
                    this.SubNodesFilled = true;
                }
            }

            public virtual void OnExpand()
            {
                this.FillSubNodes();
            }

            public virtual DesignBinding OnSelect()
            {
                return null;
            }

            public int BindingImageIndex
            {
                set
                {
                    base.ImageIndex = value;
                    base.SelectedImageIndex = value;
                }
            }

            public virtual bool CanSelect
            {
                get
                {
                    return false;
                }
            }

            public Image CustomBindingImage
            {
                set
                {
                    try
                    {
                        ImageList.ImageCollection images = this.picker.treeViewCtrl.ImageList.Images;
                        images.Add(value, Color.Transparent);
                        this.BindingImageIndex = images.Count - 1;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            public virtual string Error
            {
                get
                {
                    return this.error;
                }
                set
                {
                    this.error = value;
                }
            }

            public virtual string HelpText
            {
                get
                {
                    return null;
                }
            }

            public bool SubNodesFilled
            {
                get
                {
                    return this.subNodesFilled;
                }
                set
                {
                    this.subNodesFilled = true;
                }
            }

            public enum BindingImage
            {
                None,
                Other,
                Project,
                Instances,
                BindingSource,
                ListMember,
                FieldMember,
                DataSource
            }
        }

        internal class BindingPickerTree : TreeView
        {
            internal BindingPickerTree()
            {
                Image image = new Bitmap(typeof(DesignBindingPicker), "DataPickerImages.bmp");
                ImageList list = new ImageList {
                    TransparentColor = Color.Magenta
                };
                list.Images.AddStrip(image);
                list.ColorDepth = ColorDepth.Depth24Bit;
                base.ImageList = list;
            }

            private int GetMaxItemWidth(TreeNodeCollection nodes)
            {
                int num = 0;
                foreach (TreeNode node in nodes)
                {
                    Rectangle bounds = node.Bounds;
                    int num2 = bounds.Left + bounds.Width;
                    num = Math.Max(num2, num);
                    if (node.IsExpanded)
                    {
                        num = Math.Max(num, this.GetMaxItemWidth(node.Nodes));
                    }
                }
                return num;
            }

            protected override bool IsInputKey(Keys key)
            {
                if (key != Keys.Enter)
                {
                    return base.IsInputKey(key);
                }
                return true;
            }

            protected override void OnKeyUp(KeyEventArgs e)
            {
                base.OnKeyUp(e);
                if ((e.KeyData == Keys.Enter) && (base.SelectedNode != null))
                {
                    this.SetSelectedItem(base.SelectedNode);
                }
            }

            protected override void OnNodeMouseClick(TreeNodeMouseClickEventArgs e)
            {
                TreeViewHitTestInfo info = base.HitTest(new Point(e.X, e.Y));
                if ((info.Node == e.Node) && ((info.Location == TreeViewHitTestLocations.Image) || (info.Location == TreeViewHitTestLocations.Label)))
                {
                    this.SetSelectedItem(e.Node);
                }
                base.OnNodeMouseClick(e);
            }

            public void SetSelectedItem(TreeNode node)
            {
                DesignBindingPicker parent = base.Parent as DesignBindingPicker;
                if (parent != null)
                {
                    DesignBindingPicker.BindingPickerNode node2 = node as DesignBindingPicker.BindingPickerNode;
                    parent.selectedItem = (node2.CanSelect && (node2.Error == null)) ? node2.OnSelect() : null;
                    if (parent.selectedItem != null)
                    {
                        parent.CloseDropDown();
                    }
                }
            }

            internal int PreferredWidth
            {
                get
                {
                    return this.GetMaxItemWidth(base.Nodes);
                }
            }
        }

        internal class DataMemberNode : DesignBindingPicker.DataSourceNode
        {
            private string dataMember;
            private bool isList;

            public DataMemberNode(DesignBindingPicker picker, object dataSource, string dataMember, string dataField, bool isList) : base(picker, dataSource, dataField)
            {
                this.dataMember = dataMember;
                this.isList = isList;
                base.BindingImageIndex = isList ? 5 : 6;
            }

            public override void Fill()
            {
                base.picker.AddDataMemberContents(this);
            }

            public override DesignBinding OnSelect()
            {
                if (base.picker.showDataMembers)
                {
                    return new DesignBinding(base.DataSource, this.DataMember);
                }
                BindingSource dataSource = base.picker.CreateNewBindingSource(base.DataSource, this.DataMember);
                if (dataSource != null)
                {
                    return new DesignBinding(dataSource, "");
                }
                return null;
            }

            public override bool CanSelect
            {
                get
                {
                    return (base.picker.selectListMembers == this.IsList);
                }
            }

            public string DataMember
            {
                get
                {
                    return this.dataMember;
                }
            }

            public bool IsList
            {
                get
                {
                    return this.isList;
                }
            }
        }

        internal class DataSourceNode : DesignBindingPicker.BindingPickerNode
        {
            private object dataSource;

            public DataSourceNode(DesignBindingPicker picker, object dataSource, string nodeName) : base(picker, nodeName)
            {
                this.dataSource = dataSource;
                base.BindingImageIndex = (int) DesignBindingPicker.BindingPickerNode.BindingImageIndexForDataSource(dataSource);
            }

            public override DesignBinding OnSelect()
            {
                return new DesignBinding(this.DataSource, "");
            }

            public override bool CanSelect
            {
                get
                {
                    return !base.picker.showDataMembers;
                }
            }

            public object DataSource
            {
                get
                {
                    return this.dataSource;
                }
            }

            public override string HelpText
            {
                get
                {
                    string str;
                    string str2;
                    if (this.DataSource is DataSourceDescriptor)
                    {
                        str = "Project";
                    }
                    else if (this.DataSource is BindingSource)
                    {
                        str = "BindSrc";
                    }
                    else
                    {
                        str = "FormInst";
                    }
                    if (this is DesignBindingPicker.DataMemberNode)
                    {
                        if ((this as DesignBindingPicker.DataMemberNode).IsList)
                        {
                            str2 = "LM";
                        }
                        else
                        {
                            str2 = "DM";
                        }
                    }
                    else
                    {
                        str2 = "DS";
                    }
                    try
                    {
                        return System.Design.SR.GetString(string.Format(CultureInfo.CurrentCulture, "DesignBindingPickerHelpNode{0}{1}{2}", new object[] { str, str2, this.CanSelect ? "1" : "0" }));
                    }
                    catch
                    {
                        return "";
                    }
                }
            }
        }

        internal class HelpTextLabel : Label
        {
            protected override void OnPaint(PaintEventArgs e)
            {
                TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak;
                Rectangle bounds = new Rectangle(base.ClientRectangle.Location, base.ClientRectangle.Size);
                bounds.Inflate(-2, -2);
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, bounds, this.ForeColor, flags);
            }
        }

        internal class InstancesNode : DesignBindingPicker.BindingPickerNode
        {
            public InstancesNode(string rootComponentName) : base(null, string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("DesignBindingPickerNodeInstances"), new object[] { rootComponentName }), DesignBindingPicker.BindingPickerNode.BindingImage.Instances)
            {
            }

            public override string HelpText
            {
                get
                {
                    return System.Design.SR.GetString("DesignBindingPickerHelpNodeInstances");
                }
            }
        }

        internal class NoneNode : DesignBindingPicker.BindingPickerNode
        {
            public NoneNode() : base(null, System.Design.SR.GetString("DesignBindingPickerNodeNone"), DesignBindingPicker.BindingPickerNode.BindingImage.None)
            {
            }

            public override DesignBinding OnSelect()
            {
                return DesignBinding.Null;
            }

            public override bool CanSelect
            {
                get
                {
                    return true;
                }
            }

            public override string HelpText
            {
                get
                {
                    return System.Design.SR.GetString("DesignBindingPickerHelpNodeNone");
                }
            }
        }

        internal class OtherNode : DesignBindingPicker.BindingPickerNode
        {
            public OtherNode() : base(null, System.Design.SR.GetString("DesignBindingPickerNodeOther"), DesignBindingPicker.BindingPickerNode.BindingImage.Other)
            {
            }

            public override string HelpText
            {
                get
                {
                    return System.Design.SR.GetString("DesignBindingPickerHelpNodeOther");
                }
            }
        }

        private delegate void PostSelectTreeNodeDelegate(TreeNode node);

        internal class ProjectDataMemberNode : DesignBindingPicker.DataMemberNode
        {
            public ProjectDataMemberNode(DesignBindingPicker picker, object dataSource, string dataMember, string dataField, bool isList) : base(picker, dataSource, dataMember, dataField, isList)
            {
            }

            public override void OnExpand()
            {
            }

            public override DesignBinding OnSelect()
            {
                string dataMember;
                string str2;
                DesignBindingPicker.ProjectDataMemberNode parent = base.Parent as DesignBindingPicker.ProjectDataMemberNode;
                if (parent != null)
                {
                    dataMember = parent.DataMember;
                    str2 = base.DataMember;
                }
                else if (base.IsList)
                {
                    dataMember = base.DataMember;
                    str2 = "";
                }
                else
                {
                    dataMember = "";
                    str2 = base.DataMember;
                }
                DataSourceDescriptor dataSource = (DataSourceDescriptor) base.DataSource;
                BindingSource source = base.picker.CreateNewBindingSource(dataSource, dataMember);
                if (source != null)
                {
                    return new DesignBinding(source, str2);
                }
                return null;
            }
        }

        internal class ProjectDataSourceNode : DesignBindingPicker.DataSourceNode
        {
            public ProjectDataSourceNode(DesignBindingPicker picker, object dataSource, string nodeName, Image image) : base(picker, dataSource, nodeName)
            {
                if (image != null)
                {
                    base.CustomBindingImage = image;
                }
            }

            public override void Fill()
            {
                base.picker.AddProjectDataSourceContents(this);
            }

            public override void OnExpand()
            {
            }

            public override DesignBinding OnSelect()
            {
                DataSourceDescriptor dataSource = (DataSourceDescriptor) base.DataSource;
                if (base.picker.context.Instance is BindingSource)
                {
                    object projectDataSourceInstance = base.picker.GetProjectDataSourceInstance(dataSource);
                    if (projectDataSourceInstance != null)
                    {
                        return new DesignBinding(projectDataSourceInstance, "");
                    }
                    return null;
                }
                BindingSource source = base.picker.CreateNewBindingSource(dataSource, "");
                if (source != null)
                {
                    return new DesignBinding(source, "");
                }
                return null;
            }
        }

        internal class ProjectGroupNode : DesignBindingPicker.BindingPickerNode
        {
            public ProjectGroupNode(DesignBindingPicker picker, string nodeName, Image image) : base(picker, nodeName, DesignBindingPicker.BindingPickerNode.BindingImage.Project)
            {
                if (image != null)
                {
                    base.CustomBindingImage = image;
                }
            }

            public override string HelpText
            {
                get
                {
                    return System.Design.SR.GetString("DesignBindingPickerHelpNodeProjectGroup");
                }
            }
        }

        internal class ProjectNode : DesignBindingPicker.BindingPickerNode
        {
            public ProjectNode(DesignBindingPicker picker) : base(picker, System.Design.SR.GetString("DesignBindingPickerNodeProject"), DesignBindingPicker.BindingPickerNode.BindingImage.Project)
            {
            }

            public override string HelpText
            {
                get
                {
                    return System.Design.SR.GetString("DesignBindingPickerHelpNodeProject");
                }
            }
        }
    }
}

