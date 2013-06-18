namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Data;
    using System.Windows.Forms;

    internal class BindingSourceDesigner : ComponentDesigner
    {
        private bool bindingUpdatedByUser;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.OnComponentChanged);
                    service.ComponentRemoving -= new ComponentEventHandler(this.OnComponentRemoving);
                }
            }
            base.Dispose(disposing);
        }

        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentChanged += new ComponentChangedEventHandler(this.OnComponentChanged);
                service.ComponentRemoving += new ComponentEventHandler(this.OnComponentRemoving);
            }
        }

        private void OnComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            if (((this.bindingUpdatedByUser && (e.Component == base.Component)) && (e.Member != null)) && ((e.Member.Name == "DataSource") || (e.Member.Name == "DataMember")))
            {
                this.bindingUpdatedByUser = false;
                DataSourceProviderService service = (DataSourceProviderService) this.GetService(typeof(DataSourceProviderService));
                if (service != null)
                {
                    service.NotifyDataSourceComponentAdded(base.Component);
                }
            }
        }

        private void OnComponentRemoving(object sender, ComponentEventArgs e)
        {
            BindingSource component = base.Component as BindingSource;
            if ((component != null) && (component.DataSource == e.Component))
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

        public bool BindingUpdatedByUser
        {
            set
            {
                this.bindingUpdatedByUser = value;
            }
        }
    }
}

