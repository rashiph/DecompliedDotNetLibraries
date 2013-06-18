namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Windows.Forms;

    internal class DataGridViewDesigner : ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        private CurrencyManager cm;
        protected DesignerVerbCollection designerVerbs;
        private static System.Type typeofDataGridViewCheckBoxColumn = typeof(DataGridViewCheckBoxColumn);
        private static System.Type typeofDataGridViewImageColumn = typeof(DataGridViewImageColumn);
        private static System.Type typeofDataGridViewTextBoxColumn = typeof(DataGridViewTextBoxColumn);
        private static System.Type typeofIList = typeof(IList);

        public DataGridViewDesigner()
        {
            base.AutoResizeHandles = true;
        }

        private void BuildActionLists()
        {
            this.actionLists = new DesignerActionListCollection();
            this.actionLists.Add(new DataGridViewChooseDataSourceActionList(this));
            this.actionLists.Add(new DataGridViewColumnEditingActionList(this));
            this.actionLists.Add(new DataGridViewPropertiesActionList(this));
            this.actionLists[0].AutoShow = true;
        }

        private void dataGridView_ColumnRemoved(object sender, DataGridViewColumnEventArgs e)
        {
            if ((e.Column != null) && !e.Column.IsDataBound)
            {
                e.Column.DisplayIndex = -1;
            }
        }

        private void dataGridViewChanged(object sender, EventArgs e)
        {
            DataGridView component = (DataGridView) base.Component;
            CurrencyManager manager = null;
            if ((component.DataSource != null) && (component.BindingContext != null))
            {
                manager = (CurrencyManager) component.BindingContext[component.DataSource, component.DataMember];
            }
            if (manager != this.cm)
            {
                if (this.cm != null)
                {
                    this.cm.MetaDataChanged -= new EventHandler(this.dataGridViewMetaDataChanged);
                }
                this.cm = manager;
                if (this.cm != null)
                {
                    this.cm.MetaDataChanged += new EventHandler(this.dataGridViewMetaDataChanged);
                }
            }
            if (component.BindingContext == null)
            {
                MakeSureColumnsAreSited(component);
            }
            else if (component.AutoGenerateColumns && (component.DataSource != null))
            {
                component.AutoGenerateColumns = false;
                MakeSureColumnsAreSited(component);
            }
            else
            {
                if (component.DataSource == null)
                {
                    if (component.AutoGenerateColumns)
                    {
                        MakeSureColumnsAreSited(component);
                        return;
                    }
                    component.AutoGenerateColumns = true;
                }
                else
                {
                    component.AutoGenerateColumns = false;
                }
                this.RefreshColumnCollection();
            }
        }

        private void DataGridViewDesigner_ComponentRemoving(object sender, ComponentEventArgs e)
        {
            DataGridView component = base.Component as DataGridView;
            if ((e.Component != null) && (e.Component == component.DataSource))
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                string dataMember = component.DataMember;
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component);
                PropertyDescriptor member = (properties != null) ? properties["DataMember"] : null;
                if ((service != null) && (member != null))
                {
                    service.OnComponentChanging(component, member);
                }
                component.DataSource = null;
                if ((service != null) && (member != null))
                {
                    service.OnComponentChanged(component, member, dataMember, "");
                }
            }
        }

        private void dataGridViewMetaDataChanged(object sender, EventArgs e)
        {
            this.RefreshColumnCollection();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataGridView component = base.Component as DataGridView;
                component.DataSourceChanged -= new EventHandler(this.dataGridViewChanged);
                component.DataMemberChanged -= new EventHandler(this.dataGridViewChanged);
                component.BindingContextChanged -= new EventHandler(this.dataGridViewChanged);
                component.ColumnRemoved -= new DataGridViewColumnEventHandler(this.dataGridView_ColumnRemoved);
                if (this.cm != null)
                {
                    this.cm.MetaDataChanged -= new EventHandler(this.dataGridViewMetaDataChanged);
                }
                this.cm = null;
                if (base.Component.Site != null)
                {
                    IComponentChangeService service = base.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    if (service != null)
                    {
                        service.ComponentRemoving -= new ComponentEventHandler(this.DataGridViewDesigner_ComponentRemoving);
                    }
                }
            }
            base.Dispose(disposing);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            if (component.Site != null)
            {
                IComponentChangeService service = component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service != null)
                {
                    service.ComponentRemoving += new ComponentEventHandler(this.DataGridViewDesigner_ComponentRemoving);
                }
            }
            DataGridView view = (DataGridView) component;
            view.AutoGenerateColumns = view.DataSource == null;
            view.DataSourceChanged += new EventHandler(this.dataGridViewChanged);
            view.DataMemberChanged += new EventHandler(this.dataGridViewChanged);
            view.BindingContextChanged += new EventHandler(this.dataGridViewChanged);
            this.dataGridViewChanged(base.Component, EventArgs.Empty);
            view.ColumnRemoved += new DataGridViewColumnEventHandler(this.dataGridView_ColumnRemoved);
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            ((DataGridView) base.Component).ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        }

        private static void MakeSureColumnsAreSited(DataGridView dataGridView)
        {
            IContainer container = (dataGridView.Site != null) ? dataGridView.Site.Container : null;
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                DataGridViewColumn component = dataGridView.Columns[i];
                IContainer container2 = (component.Site != null) ? component.Site.Container : null;
                if (container != container2)
                {
                    if (container2 != null)
                    {
                        container2.Remove(component);
                    }
                    if (container != null)
                    {
                        container.Add(component);
                    }
                }
            }
        }

        public void OnAddColumn(object sender, EventArgs e)
        {
            DesignerTransaction transaction = (base.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost).CreateTransaction(System.Design.SR.GetString("DataGridViewAddColumnTransactionString"));
            DialogResult cancel = DialogResult.Cancel;
            DataGridViewAddColumnDialog dialog = new DataGridViewAddColumnDialog(((DataGridView) base.Component).Columns, (DataGridView) base.Component);
            dialog.Start(((DataGridView) base.Component).Columns.Count, true);
            try
            {
                cancel = this.ShowDialog(dialog);
            }
            finally
            {
                if (cancel == DialogResult.OK)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Cancel();
                }
            }
        }

        public void OnEditColumns(object sender, EventArgs e)
        {
            IDesignerHost service = base.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
            DataGridViewColumnCollectionDialog dialog = new DataGridViewColumnCollectionDialog(((DataGridView) base.Component).Site);
            dialog.SetLiveDataGridView((DataGridView) base.Component);
            DesignerTransaction transaction = service.CreateTransaction(System.Design.SR.GetString("DataGridViewEditColumnsTransactionString"));
            DialogResult cancel = DialogResult.Cancel;
            try
            {
                cancel = this.ShowDialog(dialog);
            }
            finally
            {
                if (cancel == DialogResult.OK)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Cancel();
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            string[] strArray = new string[] { "AutoSizeColumnsMode", "DataSource" };
            Attribute[] attributes = new Attribute[0];
            for (int i = 0; i < strArray.Length; i++)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties[strArray[i]];
                if (oldPropertyDescriptor != null)
                {
                    properties[strArray[i]] = TypeDescriptor.CreateProperty(typeof(DataGridViewDesigner), oldPropertyDescriptor, attributes);
                }
            }
        }

        private bool ProcessSimilarSchema(DataGridView dataGridView)
        {
            PropertyDescriptorCollection itemProperties = null;
            if (this.cm != null)
            {
                try
                {
                    itemProperties = this.cm.GetItemProperties();
                }
                catch (ArgumentException exception)
                {
                    throw new InvalidOperationException(System.Design.SR.GetString("DataGridViewDataSourceNoLongerValid"), exception);
                }
            }
            IContainer container = (dataGridView.Site != null) ? dataGridView.Site.Container : null;
            bool flag = false;
            for (int i = 0; i < dataGridView.Columns.Count; i++)
            {
                DataGridViewColumn component = dataGridView.Columns[i];
                if (!string.IsNullOrEmpty(component.DataPropertyName))
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)["UserAddedColumn"];
                    if ((descriptor == null) || !((bool) descriptor.GetValue(component)))
                    {
                        PropertyDescriptor descriptor2 = (itemProperties != null) ? itemProperties[component.DataPropertyName] : null;
                        bool flag2 = false;
                        if (descriptor2 == null)
                        {
                            flag2 = true;
                        }
                        else if (typeofIList.IsAssignableFrom(descriptor2.PropertyType) && !TypeDescriptor.GetConverter(typeof(Image)).CanConvertFrom(descriptor2.PropertyType))
                        {
                            flag2 = true;
                        }
                        flag = !flag2;
                        if (flag)
                        {
                            break;
                        }
                    }
                }
            }
            if (flag)
            {
                IComponentChangeService service = base.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(base.Component)["Columns"];
                try
                {
                    service.OnComponentChanging(base.Component, member);
                }
                catch (InvalidOperationException)
                {
                    return flag;
                }
                int num2 = 0;
                while (num2 < dataGridView.Columns.Count)
                {
                    DataGridViewColumn column2 = dataGridView.Columns[num2];
                    if (string.IsNullOrEmpty(column2.DataPropertyName))
                    {
                        num2++;
                    }
                    else
                    {
                        PropertyDescriptor descriptor4 = TypeDescriptor.GetProperties(column2)["UserAddedColumn"];
                        if ((descriptor4 != null) && ((bool) descriptor4.GetValue(column2)))
                        {
                            num2++;
                            continue;
                        }
                        PropertyDescriptor descriptor5 = (itemProperties != null) ? itemProperties[column2.DataPropertyName] : null;
                        bool flag3 = false;
                        if (descriptor5 == null)
                        {
                            flag3 = true;
                        }
                        else if (typeofIList.IsAssignableFrom(descriptor5.PropertyType) && !TypeDescriptor.GetConverter(typeof(Image)).CanConvertFrom(descriptor5.PropertyType))
                        {
                            flag3 = true;
                        }
                        if (flag3)
                        {
                            dataGridView.Columns.Remove(column2);
                            if (container != null)
                            {
                                container.Remove(column2);
                            }
                        }
                        else
                        {
                            num2++;
                        }
                    }
                }
                service.OnComponentChanged(base.Component, member, null, null);
            }
            return flag;
        }

        private void RefreshColumnCollection()
        {
            DataGridView component = (DataGridView) base.Component;
            ISupportInitializeNotification dataSource = component.DataSource as ISupportInitializeNotification;
            if ((dataSource == null) || dataSource.IsInitialized)
            {
                IComponentChangeService service = null;
                PropertyDescriptor member = null;
                IDesignerHost provider = base.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (!this.ProcessSimilarSchema(component))
                {
                    PropertyDescriptorCollection itemProperties = null;
                    if (this.cm != null)
                    {
                        try
                        {
                            itemProperties = this.cm.GetItemProperties();
                        }
                        catch (ArgumentException exception)
                        {
                            throw new InvalidOperationException(System.Design.SR.GetString("DataGridViewDataSourceNoLongerValid"), exception);
                        }
                    }
                    IContainer container = (component.Site != null) ? component.Site.Container : null;
                    service = base.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    member = TypeDescriptor.GetProperties(base.Component)["Columns"];
                    service.OnComponentChanging(base.Component, member);
                    DataGridViewColumn[] columnArray = new DataGridViewColumn[component.Columns.Count];
                    int index = 0;
                    for (int i = 0; i < component.Columns.Count; i++)
                    {
                        DataGridViewColumn column = component.Columns[i];
                        if (!string.IsNullOrEmpty(column.DataPropertyName))
                        {
                            PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(column)["UserAddedColumn"];
                            if ((descriptor2 == null) || !((bool) descriptor2.GetValue(column)))
                            {
                                columnArray[index] = column;
                                index++;
                            }
                        }
                    }
                    for (int j = 0; j < index; j++)
                    {
                        component.Columns.Remove(columnArray[j]);
                    }
                    service.OnComponentChanged(base.Component, member, null, null);
                    if (container != null)
                    {
                        for (int m = 0; m < index; m++)
                        {
                            container.Remove(columnArray[m]);
                        }
                    }
                    DataGridViewColumn[] columnArray2 = null;
                    int num5 = 0;
                    if (component.DataSource != null)
                    {
                        columnArray2 = new DataGridViewColumn[itemProperties.Count];
                        num5 = 0;
                        for (int n = 0; n < itemProperties.Count; n++)
                        {
                            System.Type typeofDataGridViewImageColumn;
                            DataGridViewColumn column2 = null;
                            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Image));
                            System.Type propertyType = itemProperties[n].PropertyType;
                            if (typeof(IList).IsAssignableFrom(propertyType))
                            {
                                if (!converter.CanConvertFrom(propertyType))
                                {
                                    continue;
                                }
                                typeofDataGridViewImageColumn = DataGridViewDesigner.typeofDataGridViewImageColumn;
                            }
                            else if ((propertyType == typeof(bool)) || (propertyType == typeof(CheckState)))
                            {
                                typeofDataGridViewImageColumn = typeofDataGridViewCheckBoxColumn;
                            }
                            else if (typeof(Image).IsAssignableFrom(propertyType) || converter.CanConvertFrom(propertyType))
                            {
                                typeofDataGridViewImageColumn = DataGridViewDesigner.typeofDataGridViewImageColumn;
                            }
                            else
                            {
                                typeofDataGridViewImageColumn = typeofDataGridViewTextBoxColumn;
                            }
                            string name = ToolStripDesigner.NameFromText(itemProperties[n].Name, typeofDataGridViewImageColumn, base.Component.Site);
                            column2 = TypeDescriptor.CreateInstance(provider, typeofDataGridViewImageColumn, null, null) as DataGridViewColumn;
                            column2.DataPropertyName = itemProperties[n].Name;
                            column2.HeaderText = !string.IsNullOrEmpty(itemProperties[n].DisplayName) ? itemProperties[n].DisplayName : itemProperties[n].Name;
                            column2.Name = itemProperties[n].Name;
                            column2.ValueType = itemProperties[n].PropertyType;
                            column2.ReadOnly = itemProperties[n].IsReadOnly;
                            provider.Container.Add(column2, name);
                            columnArray2[num5] = column2;
                            num5++;
                        }
                    }
                    service.OnComponentChanging(base.Component, member);
                    for (int k = 0; k < num5; k++)
                    {
                        columnArray2[k].DisplayIndex = -1;
                        component.Columns.Add(columnArray2[k]);
                    }
                    service.OnComponentChanged(base.Component, member, null, null);
                }
            }
        }

        private bool ShouldSerializeAutoSizeColumnsMode()
        {
            DataGridView component = base.Component as DataGridView;
            return ((component != null) && (component.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None));
        }

        private bool ShouldSerializeDataSource()
        {
            return (((DataGridView) base.Component).DataSource != null);
        }

        private DialogResult ShowDialog(Form dialog)
        {
            IUIService service = base.Component.Site.GetService(typeof(IUIService)) as IUIService;
            if (service != null)
            {
                return service.ShowDialog(dialog);
            }
            return dialog.ShowDialog(base.Component as IWin32Window);
        }

        internal static void ShowErrorDialog(IUIService uiService, Exception ex, Control dataGridView)
        {
            if (uiService != null)
            {
                uiService.ShowError(ex);
            }
            else
            {
                string message = ex.Message;
                if ((message == null) || (message.Length == 0))
                {
                    message = ex.ToString();
                }
                System.Windows.Forms.Design.RTLAwareMessageBox.Show(dataGridView, message, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        internal static void ShowErrorDialog(IUIService uiService, string errorString, Control dataGridView)
        {
            if (uiService != null)
            {
                uiService.ShowError(errorString);
            }
            else
            {
                System.Windows.Forms.Design.RTLAwareMessageBox.Show(dataGridView, errorString, null, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (this.actionLists == null)
                {
                    this.BuildActionLists();
                }
                return this.actionLists;
            }
        }

        public override ICollection AssociatedComponents
        {
            get
            {
                DataGridView component = base.Component as DataGridView;
                if (component != null)
                {
                    return component.Columns;
                }
                return base.AssociatedComponents;
            }
        }

        public DataGridViewAutoSizeColumnsMode AutoSizeColumnsMode
        {
            get
            {
                DataGridView component = base.Component as DataGridView;
                return component.AutoSizeColumnsMode;
            }
            set
            {
                DataGridView component = base.Component as DataGridView;
                IComponentChangeService service = base.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(typeof(DataGridViewColumn))["Width"];
                for (int i = 0; i < component.Columns.Count; i++)
                {
                    service.OnComponentChanging(component.Columns[i], member);
                }
                component.AutoSizeColumnsMode = value;
                for (int j = 0; j < component.Columns.Count; j++)
                {
                    service.OnComponentChanged(component.Columns[j], member, null, null);
                }
            }
        }

        public object DataSource
        {
            get
            {
                return ((DataGridView) base.Component).DataSource;
            }
            set
            {
                DataGridView component = base.Component as DataGridView;
                if ((component.AutoGenerateColumns && (component.DataSource == null)) && (value != null))
                {
                    component.AutoGenerateColumns = false;
                }
                ((DataGridView) base.Component).DataSource = value;
            }
        }

        protected override System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if ((base.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.Inherited) && (base.InheritanceAttribute != System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
                {
                    return base.InheritanceAttribute;
                }
                return System.ComponentModel.InheritanceAttribute.InheritedReadOnly;
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (this.designerVerbs == null)
                {
                    this.designerVerbs = new DesignerVerbCollection();
                    this.designerVerbs.Add(new DesignerVerb(System.Design.SR.GetString("DataGridViewEditColumnsVerb"), new EventHandler(this.OnEditColumns)));
                    this.designerVerbs.Add(new DesignerVerb(System.Design.SR.GetString("DataGridViewAddColumnVerb"), new EventHandler(this.OnAddColumn)));
                }
                return this.designerVerbs;
            }
        }

        [ComplexBindingProperties("DataSource", "DataMember")]
        private class DataGridViewChooseDataSourceActionList : DesignerActionList
        {
            private DataGridViewDesigner owner;

            public DataGridViewChooseDataSourceActionList(DataGridViewDesigner owner) : base(owner.Component)
            {
                this.owner = owner;
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                DesignerActionPropertyItem item = new DesignerActionPropertyItem("DataSource", System.Design.SR.GetString("DataGridViewChooseDataSource")) {
                    RelatedComponent = this.owner.Component
                };
                items.Add(item);
                return items;
            }

            [AttributeProvider(typeof(IListSource))]
            public object DataSource
            {
                get
                {
                    return this.owner.DataSource;
                }
                set
                {
                    DataGridView component = (DataGridView) this.owner.Component;
                    IDesignerHost host = this.owner.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    PropertyDescriptor member = TypeDescriptor.GetProperties(component)["DataSource"];
                    IComponentChangeService service = this.owner.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    DesignerTransaction transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewChooseDataSourceTransactionString", new object[] { component.Name }));
                    try
                    {
                        service.OnComponentChanging(this.owner.Component, member);
                        this.owner.DataSource = value;
                        service.OnComponentChanged(this.owner.Component, member, null, null);
                        transaction.Commit();
                        transaction = null;
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Cancel();
                        }
                    }
                }
            }
        }

        private class DataGridViewColumnEditingActionList : DesignerActionList
        {
            private DataGridViewDesigner owner;

            public DataGridViewColumnEditingActionList(DataGridViewDesigner owner) : base(owner.Component)
            {
                this.owner = owner;
            }

            public void AddColumn()
            {
                this.owner.OnAddColumn(this, EventArgs.Empty);
            }

            public void EditColumns()
            {
                this.owner.OnEditColumns(this, EventArgs.Empty);
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionMethodItem(this, "EditColumns", System.Design.SR.GetString("DataGridViewEditColumnsVerb"), true));
                items.Add(new DesignerActionMethodItem(this, "AddColumn", System.Design.SR.GetString("DataGridViewAddColumnVerb"), true));
                return items;
            }
        }

        private class DataGridViewPropertiesActionList : DesignerActionList
        {
            private DataGridViewDesigner owner;

            public DataGridViewPropertiesActionList(DataGridViewDesigner owner) : base(owner.Component)
            {
                this.owner = owner;
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                items.Add(new DesignerActionPropertyItem("AllowUserToAddRows", System.Design.SR.GetString("DataGridViewEnableAdding")));
                items.Add(new DesignerActionPropertyItem("ReadOnly", System.Design.SR.GetString("DataGridViewEnableEditing")));
                items.Add(new DesignerActionPropertyItem("AllowUserToDeleteRows", System.Design.SR.GetString("DataGridViewEnableDeleting")));
                items.Add(new DesignerActionPropertyItem("AllowUserToOrderColumns", System.Design.SR.GetString("DataGridViewEnableColumnReordering")));
                return items;
            }

            public bool AllowUserToAddRows
            {
                get
                {
                    return ((DataGridView) this.owner.Component).AllowUserToAddRows;
                }
                set
                {
                    if (value != this.AllowUserToAddRows)
                    {
                        DesignerTransaction transaction;
                        IDesignerHost host = this.owner.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (value)
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewEnableAddingTransactionString"));
                        }
                        else
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewDisableAddingTransactionString"));
                        }
                        try
                        {
                            IComponentChangeService service = this.owner.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            PropertyDescriptor member = TypeDescriptor.GetProperties(this.owner.Component)["AllowUserToAddRows"];
                            service.OnComponentChanging(this.owner.Component, member);
                            ((DataGridView) this.owner.Component).AllowUserToAddRows = value;
                            service.OnComponentChanged(this.owner.Component, member, null, null);
                            transaction.Commit();
                            transaction = null;
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                        }
                    }
                }
            }

            public bool AllowUserToDeleteRows
            {
                get
                {
                    return ((DataGridView) this.owner.Component).AllowUserToDeleteRows;
                }
                set
                {
                    if (value != this.AllowUserToDeleteRows)
                    {
                        DesignerTransaction transaction;
                        IDesignerHost host = this.owner.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (value)
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewEnableDeletingTransactionString"));
                        }
                        else
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewDisableDeletingTransactionString"));
                        }
                        try
                        {
                            IComponentChangeService service = this.owner.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            PropertyDescriptor member = TypeDescriptor.GetProperties(this.owner.Component)["AllowUserToDeleteRows"];
                            service.OnComponentChanging(this.owner.Component, member);
                            ((DataGridView) this.owner.Component).AllowUserToDeleteRows = value;
                            service.OnComponentChanged(this.owner.Component, member, null, null);
                            transaction.Commit();
                            transaction = null;
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                        }
                    }
                }
            }

            public bool AllowUserToOrderColumns
            {
                get
                {
                    return ((DataGridView) this.owner.Component).AllowUserToOrderColumns;
                }
                set
                {
                    if (value != this.AllowUserToOrderColumns)
                    {
                        DesignerTransaction transaction;
                        IDesignerHost host = this.owner.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (value)
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewEnableColumnReorderingTransactionString"));
                        }
                        else
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewDisableColumnReorderingTransactionString"));
                        }
                        try
                        {
                            IComponentChangeService service = this.owner.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            PropertyDescriptor member = TypeDescriptor.GetProperties(this.owner.Component)["AllowUserToReorderColumns"];
                            service.OnComponentChanging(this.owner.Component, member);
                            ((DataGridView) this.owner.Component).AllowUserToOrderColumns = value;
                            service.OnComponentChanged(this.owner.Component, member, null, null);
                            transaction.Commit();
                            transaction = null;
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                        }
                    }
                }
            }

            public bool ReadOnly
            {
                get
                {
                    return !((DataGridView) this.owner.Component).ReadOnly;
                }
                set
                {
                    if (value != this.ReadOnly)
                    {
                        DesignerTransaction transaction;
                        IDesignerHost host = this.owner.Component.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                        if (value)
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewEnableEditingTransactionString"));
                        }
                        else
                        {
                            transaction = host.CreateTransaction(System.Design.SR.GetString("DataGridViewDisableEditingTransactionString"));
                        }
                        try
                        {
                            IComponentChangeService service = this.owner.Component.Site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                            PropertyDescriptor member = TypeDescriptor.GetProperties(this.owner.Component)["ReadOnly"];
                            service.OnComponentChanging(this.owner.Component, member);
                            ((DataGridView) this.owner.Component).ReadOnly = !value;
                            service.OnComponentChanged(this.owner.Component, member, null, null);
                            transaction.Commit();
                            transaction = null;
                        }
                        finally
                        {
                            if (transaction != null)
                            {
                                transaction.Cancel();
                            }
                        }
                    }
                }
            }
        }
    }
}

