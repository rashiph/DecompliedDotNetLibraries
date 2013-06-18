namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Windows.Forms;

    [ComplexBindingProperties("DataSource", "DataMember")]
    internal class ListControlBoundActionList : DesignerActionList
    {
        private bool _boundMode;
        private object _boundSelectedValue;
        private ControlDesigner _owner;
        private DesignerActionUIService uiService;

        public ListControlBoundActionList(ControlDesigner owner) : base(owner.Component)
        {
            this._owner = owner;
            ListControl component = (ListControl) base.Component;
            if (component.DataSource != null)
            {
                this._boundMode = true;
            }
            this.uiService = base.GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        private Binding GetSelectedValueBinding()
        {
            ListControl component = (ListControl) base.Component;
            Binding binding = null;
            if (component.DataBindings != null)
            {
                foreach (Binding binding2 in component.DataBindings)
                {
                    if (binding2.PropertyName == "SelectedValue")
                    {
                        binding = binding2;
                    }
                }
            }
            return binding;
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            DesignerActionItemCollection items = new DesignerActionItemCollection();
            items.Add(new DesignerActionPropertyItem("BoundMode", System.Design.SR.GetString("BoundModeDisplayName"), System.Design.SR.GetString("DataCategoryName"), System.Design.SR.GetString("BoundModeDescription")));
            ListControl component = base.Component as ListControl;
            if (this._boundMode || ((component != null) && (component.DataSource != null)))
            {
                this._boundMode = true;
                items.Add(new DesignerActionHeaderItem(System.Design.SR.GetString("BoundModeHeader"), System.Design.SR.GetString("DataCategoryName")));
                items.Add(new DesignerActionPropertyItem("DataSource", System.Design.SR.GetString("DataSourceDisplayName"), System.Design.SR.GetString("DataCategoryName"), System.Design.SR.GetString("DataSourceDescription")));
                items.Add(new DesignerActionPropertyItem("DisplayMember", System.Design.SR.GetString("DisplayMemberDisplayName"), System.Design.SR.GetString("DataCategoryName"), System.Design.SR.GetString("DisplayMemberDescription")));
                items.Add(new DesignerActionPropertyItem("ValueMember", System.Design.SR.GetString("ValueMemberDisplayName"), System.Design.SR.GetString("DataCategoryName"), System.Design.SR.GetString("ValueMemberDescription")));
                items.Add(new DesignerActionPropertyItem("BoundSelectedValue", System.Design.SR.GetString("BoundSelectedValueDisplayName"), System.Design.SR.GetString("DataCategoryName"), System.Design.SR.GetString("BoundSelectedValueDescription")));
                return items;
            }
            items.Add(new DesignerActionHeaderItem(System.Design.SR.GetString("UnBoundModeHeader"), System.Design.SR.GetString("DataCategoryName")));
            items.Add(new DesignerActionMethodItem(this, "InvokeItemsDialog", System.Design.SR.GetString("EditItemDisplayName"), System.Design.SR.GetString("DataCategoryName"), System.Design.SR.GetString("EditItemDescription"), true));
            return items;
        }

        public void InvokeItemsDialog()
        {
            EditorServiceContext.EditValue(this._owner, base.Component, "Items");
        }

        private void RefreshPanelContent()
        {
            if (this.uiService != null)
            {
                this.uiService.Refresh(this._owner.Component);
            }
        }

        private void SetSelectedValueBinding(object dataSource, string dataMember)
        {
            ListControl component = (ListControl) base.Component;
            IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
            IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
            PropertyDescriptor member = TypeDescriptor.GetProperties(component)["DataBindings"];
            if ((host != null) && (service != null))
            {
                using (DesignerTransaction transaction = host.CreateTransaction("TextBox DataSource RESX"))
                {
                    service.OnComponentChanging(this._owner.Component, member);
                    Binding selectedValueBinding = this.GetSelectedValueBinding();
                    if (selectedValueBinding != null)
                    {
                        component.DataBindings.Remove(selectedValueBinding);
                    }
                    if (((component.DataBindings != null) && (dataSource != null)) && !string.IsNullOrEmpty(dataMember))
                    {
                        component.DataBindings.Add("SelectedValue", dataSource, dataMember);
                    }
                    service.OnComponentChanged(this._owner.Component, member, null, null);
                    transaction.Commit();
                }
            }
        }

        public bool BoundMode
        {
            get
            {
                return this._boundMode;
            }
            set
            {
                if (!value)
                {
                    this.DataSource = null;
                }
                if (this.DataSource == null)
                {
                    this._boundMode = value;
                }
                this.RefreshPanelContent();
            }
        }

        [TypeConverter("System.Windows.Forms.Design.DesignBindingConverter"), Editor("System.Windows.Forms.Design.DesignBindingEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public object BoundSelectedValue
        {
            get
            {
                string bindingMember;
                object dataSource;
                Binding selectedValueBinding = this.GetSelectedValueBinding();
                if (selectedValueBinding == null)
                {
                    bindingMember = null;
                    dataSource = null;
                }
                else
                {
                    bindingMember = selectedValueBinding.BindingMemberInfo.BindingMember;
                    dataSource = selectedValueBinding.DataSource;
                }
                string typeName = string.Format(CultureInfo.InvariantCulture, "System.Windows.Forms.Design.DesignBinding, {0}", new object[] { typeof(ControlDesigner).Assembly.FullName });
                this._boundSelectedValue = TypeDescriptor.CreateInstance(null, System.Type.GetType(typeName), new System.Type[] { typeof(object), typeof(string) }, new object[] { dataSource, bindingMember });
                return this._boundSelectedValue;
            }
            set
            {
                if (value is string)
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(this)["BoundSelectedValue"];
                    this._boundSelectedValue = descriptor.Converter.ConvertFrom(new EditorServiceContext(this._owner), CultureInfo.InvariantCulture, value);
                }
                else
                {
                    this._boundSelectedValue = value;
                    if (value != null)
                    {
                        object dataSource = TypeDescriptor.GetProperties(this._boundSelectedValue)["DataSource"].GetValue(this._boundSelectedValue);
                        string dataMember = (string) TypeDescriptor.GetProperties(this._boundSelectedValue)["DataMember"].GetValue(this._boundSelectedValue);
                        this.SetSelectedValueBinding(dataSource, dataMember);
                    }
                }
            }
        }

        [AttributeProvider(typeof(IListSource))]
        public object DataSource
        {
            get
            {
                return ((ListControl) base.Component).DataSource;
            }
            set
            {
                ListControl component = (ListControl) base.Component;
                IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(component)["DataSource"];
                if ((host != null) && (service != null))
                {
                    using (DesignerTransaction transaction = host.CreateTransaction("DGV DataSource TX Name"))
                    {
                        service.OnComponentChanging(base.Component, member);
                        component.DataSource = value;
                        if (value == null)
                        {
                            component.DisplayMember = "";
                            component.ValueMember = "";
                        }
                        service.OnComponentChanged(base.Component, member, null, null);
                        transaction.Commit();
                        this.RefreshPanelContent();
                    }
                }
            }
        }

        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string DisplayMember
        {
            get
            {
                return ((ListControl) base.Component).DisplayMember;
            }
            set
            {
                ListControl component = (ListControl) base.Component;
                IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(component)["DisplayMember"];
                if ((host != null) && (service != null))
                {
                    using (DesignerTransaction transaction = host.CreateTransaction("DGV DataSource TX Name"))
                    {
                        service.OnComponentChanging(base.Component, member);
                        component.DisplayMember = value;
                        service.OnComponentChanged(base.Component, member, null, null);
                        transaction.Commit();
                    }
                }
            }
        }

        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValueMember
        {
            get
            {
                return ((ListControl) base.Component).ValueMember;
            }
            set
            {
                ListControl component = (ListControl) this._owner.Component;
                IDesignerHost host = base.GetService(typeof(IDesignerHost)) as IDesignerHost;
                IComponentChangeService service = base.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                PropertyDescriptor member = TypeDescriptor.GetProperties(component)["ValueMember"];
                if ((host != null) && (service != null))
                {
                    using (DesignerTransaction transaction = host.CreateTransaction("DGV DataSource TX Name"))
                    {
                        service.OnComponentChanging(base.Component, member);
                        component.ValueMember = value;
                        service.OnComponentChanged(base.Component, member, null, null);
                        transaction.Commit();
                    }
                }
            }
        }
    }
}

