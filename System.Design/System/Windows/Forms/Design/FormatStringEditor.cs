namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class FormatStringEditor : UITypeEditor
    {
        private FormatStringDialog formatStringDialog;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (service == null)
                {
                    return value;
                }
                DataGridViewCellStyle instance = context.Instance as DataGridViewCellStyle;
                ListControl component = context.Instance as ListControl;
                if (this.formatStringDialog == null)
                {
                    this.formatStringDialog = new FormatStringDialog(context);
                }
                if (component != null)
                {
                    this.formatStringDialog.ListControl = component;
                }
                else
                {
                    this.formatStringDialog.DataGridViewCellStyle = instance;
                }
                IComponentChangeService service2 = (IComponentChangeService) provider.GetService(typeof(IComponentChangeService));
                if (service2 != null)
                {
                    if (instance != null)
                    {
                        service2.OnComponentChanging(instance, TypeDescriptor.GetProperties(instance)["Format"]);
                        service2.OnComponentChanging(instance, TypeDescriptor.GetProperties(instance)["NullValue"]);
                        service2.OnComponentChanging(instance, TypeDescriptor.GetProperties(instance)["FormatProvider"]);
                    }
                    else
                    {
                        service2.OnComponentChanging(component, TypeDescriptor.GetProperties(component)["FormatString"]);
                        service2.OnComponentChanging(component, TypeDescriptor.GetProperties(component)["FormatInfo"]);
                    }
                }
                service.ShowDialog(this.formatStringDialog);
                this.formatStringDialog.End();
                if (!this.formatStringDialog.Dirty)
                {
                    return value;
                }
                TypeDescriptor.Refresh(context.Instance);
                if (service2 == null)
                {
                    return value;
                }
                if (instance != null)
                {
                    service2.OnComponentChanged(instance, TypeDescriptor.GetProperties(instance)["Format"], null, null);
                    service2.OnComponentChanged(instance, TypeDescriptor.GetProperties(instance)["NullValue"], null, null);
                    service2.OnComponentChanged(instance, TypeDescriptor.GetProperties(instance)["FormatProvider"], null, null);
                    return value;
                }
                service2.OnComponentChanged(component, TypeDescriptor.GetProperties(component)["FormatString"], null, null);
                service2.OnComponentChanged(component, TypeDescriptor.GetProperties(component)["FormatInfo"], null, null);
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

