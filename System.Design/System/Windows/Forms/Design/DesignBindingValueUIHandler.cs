namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class DesignBindingValueUIHandler : IDisposable
    {
        private Bitmap dataBitmap;

        public void Dispose()
        {
            if (this.dataBitmap != null)
            {
                this.dataBitmap.Dispose();
            }
        }

        internal void OnGetUIValueItem(ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList)
        {
            if (context.Instance is Control)
            {
                Control instance = (Control) context.Instance;
                foreach (Binding binding in instance.DataBindings)
                {
                    if ((((binding.DataSource is IListSource) || (binding.DataSource is IList)) || (binding.DataSource is Array)) && binding.PropertyName.Equals(propDesc.Name))
                    {
                        valueUIItemList.Add(new LocalUIItem(this, binding));
                    }
                }
            }
        }

        private void OnPropertyValueUIItemInvoke(ITypeDescriptorContext context, PropertyDescriptor descriptor, PropertyValueUIItem invokedItem)
        {
            LocalUIItem item = (LocalUIItem) invokedItem;
            IServiceProvider service = null;
            Control control = item.Binding.Control;
            if (control.Site != null)
            {
                service = (IServiceProvider) control.Site.GetService(typeof(IServiceProvider));
            }
            if (service != null)
            {
                AdvancedBindingPropertyDescriptor.advancedBindingEditor.EditValue(context, service, control.DataBindings);
            }
        }

        internal Bitmap DataBitmap
        {
            get
            {
                if (this.dataBitmap == null)
                {
                    this.dataBitmap = new Bitmap(typeof(DesignBindingValueUIHandler), "BoundProperty.bmp");
                    this.dataBitmap.MakeTransparent();
                }
                return this.dataBitmap;
            }
        }

        private class LocalUIItem : PropertyValueUIItem
        {
            private System.Windows.Forms.Binding binding;

            internal LocalUIItem(DesignBindingValueUIHandler handler, System.Windows.Forms.Binding binding) : base(handler.DataBitmap, new PropertyValueUIItemInvokeHandler(handler.OnPropertyValueUIItemInvoke), GetToolTip(binding))
            {
                this.binding = binding;
            }

            private static string GetToolTip(System.Windows.Forms.Binding binding)
            {
                string name = "";
                if (binding.DataSource is IComponent)
                {
                    IComponent dataSource = (IComponent) binding.DataSource;
                    if (dataSource.Site != null)
                    {
                        name = dataSource.Site.Name;
                    }
                }
                if (name.Length == 0)
                {
                    name = "(List)";
                }
                return (name + " - " + binding.BindingMemberInfo.BindingMember);
            }

            internal System.Windows.Forms.Binding Binding
            {
                get
                {
                    return this.binding;
                }
            }
        }
    }
}

