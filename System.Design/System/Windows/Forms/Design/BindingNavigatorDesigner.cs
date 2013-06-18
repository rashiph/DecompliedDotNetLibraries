namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Windows.Forms;

    internal class BindingNavigatorDesigner : ToolStripDesigner
    {
        private static string[] itemNames = new string[] { "MovePreviousItem", "MoveFirstItem", "MoveNextItem", "MoveLastItem", "AddNewItem", "DeleteItem", "PositionItem", "CountItem" };

        private void ComponentChangeSvc_ComponentChanged(object sender, ComponentChangedEventArgs e)
        {
            BindingNavigator component = (BindingNavigator) base.Component;
            if (((e.Component != null) && (e.Component == component.CountItem)) && ((e.Member != null) && (e.Member.Name == "Text")))
            {
                component.CountItemFormat = component.CountItem.Text;
            }
        }

        private void ComponentChangeSvc_ComponentRemoved(object sender, ComponentEventArgs e)
        {
            ToolStripItem component = e.Component as ToolStripItem;
            if (component != null)
            {
                BindingNavigator navigator = (BindingNavigator) base.Component;
                if (component == navigator.MoveFirstItem)
                {
                    navigator.MoveFirstItem = null;
                }
                else if (component == navigator.MovePreviousItem)
                {
                    navigator.MovePreviousItem = null;
                }
                else if (component == navigator.MoveNextItem)
                {
                    navigator.MoveNextItem = null;
                }
                else if (component == navigator.MoveLastItem)
                {
                    navigator.MoveLastItem = null;
                }
                else if (component == navigator.PositionItem)
                {
                    navigator.PositionItem = null;
                }
                else if (component == navigator.CountItem)
                {
                    navigator.CountItem = null;
                }
                else if (component == navigator.AddNewItem)
                {
                    navigator.AddNewItem = null;
                }
                else if (component == navigator.DeleteItem)
                {
                    navigator.DeleteItem = null;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRemoved -= new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoved);
                    service.ComponentChanged -= new ComponentChangedEventHandler(this.ComponentChangeSvc_ComponentChanged);
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
                service.ComponentRemoved += new ComponentEventHandler(this.ComponentChangeSvc_ComponentRemoved);
                service.ComponentChanged += new ComponentChangedEventHandler(this.ComponentChangeSvc_ComponentChanged);
            }
        }

        public override void InitializeNewComponent(IDictionary defaultValues)
        {
            base.InitializeNewComponent(defaultValues);
            BindingNavigator component = (BindingNavigator) base.Component;
            IDesignerHost service = (IDesignerHost) base.Component.Site.GetService(typeof(IDesignerHost));
            try
            {
                ToolStripDesigner._autoAddNewItems = false;
                component.SuspendLayout();
                component.AddStandardItems();
                this.SiteItems(service, component.Items);
                this.RaiseItemsChanged();
                component.ResumeLayout();
                component.ShowItemToolTips = true;
            }
            finally
            {
                ToolStripDesigner._autoAddNewItems = true;
            }
        }

        private void RaiseItemsChanged()
        {
            BindingNavigator component = (BindingNavigator) base.Component;
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                MemberDescriptor member = TypeDescriptor.GetProperties(component)["Items"];
                service.OnComponentChanging(component, member);
                service.OnComponentChanged(component, member, null, null);
                foreach (string str in itemNames)
                {
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(component)[str];
                    if (descriptor2 != null)
                    {
                        service.OnComponentChanging(component, descriptor2);
                        service.OnComponentChanged(component, descriptor2, null, null);
                    }
                }
            }
        }

        private void SiteItem(IDesignerHost host, ToolStripItem item)
        {
            if (!(item is DesignerToolStripControlHost))
            {
                host.Container.Add(item, DesignerUtils.GetUniqueSiteName(host, item.Name));
                item.Name = item.Site.Name;
                ToolStripDropDownItem item2 = item as ToolStripDropDownItem;
                if ((item2 != null) && item2.HasDropDownItems)
                {
                    this.SiteItems(host, item2.DropDownItems);
                }
            }
        }

        private void SiteItems(IDesignerHost host, ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items)
            {
                this.SiteItem(host, item);
            }
        }
    }
}

