namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Runtime.InteropServices;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;

    public abstract class BaseDataBoundControlDesigner : ControlDesigner
    {
        private bool _keepDataSourceBrowsable;

        protected BaseDataBoundControlDesigner()
        {
        }

        protected abstract bool ConnectToDataSource();
        protected abstract void CreateDataSource();
        protected abstract void DataBind(BaseDataBoundControl dataBoundControl);
        protected abstract void DisconnectFromDataSource();
        protected override void Dispose(bool disposing)
        {
            if ((disposing && (base.Component != null)) && (base.Component.Site != null))
            {
                this.DisconnectFromDataSource();
                if (base.RootDesigner != null)
                {
                    base.RootDesigner.LoadComplete -= new EventHandler(this.OnDesignerLoadComplete);
                }
                IComponentChangeService service = (IComponentChangeService) base.Component.Site.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentAdded -= new ComponentEventHandler(this.OnComponentAdded);
                    service.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                    service.ComponentRemoved -= new ComponentEventHandler(this.OnComponentRemoved);
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnAnyComponentChanged);
                }
            }
            base.Dispose(disposing);
        }

        public override string GetDesignTimeHtml()
        {
            try
            {
                this.DataBind((BaseDataBoundControl) base.ViewControl);
                return base.GetDesignTimeHtml();
            }
            catch (Exception exception)
            {
                return this.GetErrorDesignTimeHtml(exception);
            }
        }

        protected override string GetEmptyDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml(null);
        }

        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRenderingShort") + "<br/>" + e.Message);
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(BaseDataBoundControl));
            base.Initialize(component);
            base.SetViewFlags(ViewFlags.DesignTimeHtmlRequiresLoadComplete, true);
            if (base.RootDesigner != null)
            {
                if (base.RootDesigner.IsLoading)
                {
                    base.RootDesigner.LoadComplete += new EventHandler(this.OnDesignerLoadComplete);
                }
                else
                {
                    this.OnDesignerLoadComplete(null, EventArgs.Empty);
                }
            }
            IComponentChangeService service = (IComponentChangeService) component.Site.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentAdded += new ComponentEventHandler(this.OnComponentAdded);
                service.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
                service.ComponentRemoved += new ComponentEventHandler(this.OnComponentRemoved);
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnAnyComponentChanged);
            }
        }

        private void OnAnyComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            if ((((ce.Component is System.Web.UI.Control) && (ce.Member != null)) && ((ce.Member.Name == "ID") && (base.Component != null))) && ((((string) ce.OldValue) == this.DataSourceID) || (((string) ce.NewValue) == this.DataSourceID)))
            {
                this.OnDataSourceChanged(false);
            }
        }

        private void OnComponentAdded(object sender, ComponentEventArgs e)
        {
            System.Web.UI.Control component = e.Component as System.Web.UI.Control;
            if ((component != null) && (component.ID == this.DataSourceID))
            {
                this.OnDataSourceChanged(false);
            }
        }

        private void OnComponentRemoved(object sender, ComponentEventArgs e)
        {
            System.Web.UI.Control component = e.Component as System.Web.UI.Control;
            if (((component != null) && (base.Component != null)) && (component.ID == this.DataSourceID))
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((service != null) && !service.Loading)
                {
                    this.OnDataSourceChanged(false);
                }
            }
        }

        private void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            System.Web.UI.Control component = e.Component as System.Web.UI.Control;
            if (((component != null) && (base.Component != null)) && (component.ID == this.DataSourceID))
            {
                this.DisconnectFromDataSource();
            }
        }

        protected virtual void OnDataSourceChanged(bool forceUpdateView)
        {
            if (this.ConnectToDataSource() || forceUpdateView)
            {
                this.UpdateDesignTimeHtml();
            }
        }

        private void OnDesignerLoadComplete(object sender, EventArgs e)
        {
            this.OnDataSourceChanged(false);
        }

        protected virtual void OnSchemaRefreshed()
        {
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            int num2;
            base.PreFilterProperties(properties);
            PropertyDescriptor descriptor = (PropertyDescriptor) properties["DataSource"];
            System.ComponentModel.AttributeCollection attributes = descriptor.Attributes;
            int index = -1;
            int count = attributes.Count;
            string dataSource = this.DataSource;
            bool flag = (dataSource != null) && (dataSource.Length > 0);
            if (flag)
            {
                this._keepDataSourceBrowsable = true;
            }
            for (int i = 0; i < attributes.Count; i++)
            {
                if (attributes[i] is BrowsableAttribute)
                {
                    index = i;
                    break;
                }
            }
            if (((index == -1) && !flag) && !this._keepDataSourceBrowsable)
            {
                num2 = count + 1;
            }
            else
            {
                num2 = count;
            }
            Attribute[] array = new Attribute[num2];
            attributes.CopyTo(array, 0);
            if (!flag && !this._keepDataSourceBrowsable)
            {
                if (index == -1)
                {
                    array[count] = BrowsableAttribute.No;
                }
                else
                {
                    array[index] = BrowsableAttribute.No;
                }
            }
            descriptor = TypeDescriptor.CreateProperty(base.GetType(), "DataSource", typeof(string), array);
            properties["DataSource"] = descriptor;
        }

        public static DialogResult ShowCreateDataSourceDialog(ControlDesigner controlDesigner, System.Type dataSourceType, bool configure, out string dataSourceID)
        {
            CreateDataSourceDialog form = new CreateDataSourceDialog(controlDesigner, dataSourceType, configure);
            DialogResult result = UIServiceHelper.ShowDialog(controlDesigner.Component.Site, form);
            dataSourceID = form.DataSourceID;
            return result;
        }

        public string DataSource
        {
            get
            {
                DataBinding binding = base.DataBindings["DataSource"];
                if (binding != null)
                {
                    return binding.Expression;
                }
                return string.Empty;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    base.DataBindings.Remove("DataSource");
                }
                else
                {
                    DataBinding binding = base.DataBindings["DataSource"];
                    if (binding == null)
                    {
                        binding = new DataBinding("DataSource", typeof(IEnumerable), value);
                    }
                    else
                    {
                        binding.Expression = value;
                    }
                    base.DataBindings.Add(binding);
                }
                this.OnDataSourceChanged(true);
                base.OnBindingsCollectionChangedInternal("DataSource");
            }
        }

        public string DataSourceID
        {
            get
            {
                return ((BaseDataBoundControl) base.Component).DataSourceID;
            }
            set
            {
                if (value != this.DataSourceID)
                {
                    if (value == System.Design.SR.GetString("DataSourceIDChromeConverter_NewDataSource"))
                    {
                        this.CreateDataSource();
                        TypeDescriptor.Refresh(base.Component);
                    }
                    else
                    {
                        if (value == System.Design.SR.GetString("DataSourceIDChromeConverter_NoDataSource"))
                        {
                            value = string.Empty;
                        }
                        TypeDescriptor.Refresh(base.Component);
                        TypeDescriptor.GetProperties(typeof(BaseDataBoundControl))["DataSourceID"].SetValue(base.Component, value);
                        this.OnDataSourceChanged(false);
                        this.OnSchemaRefreshed();
                    }
                }
            }
        }
    }
}

