namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class AdvancedBindingEditor : UITypeEditor
    {
        private BindingFormattingDialog bindingFormattingDialog;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                IDesignerHost host = provider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service == null) || (host == null))
                {
                    return value;
                }
                if (this.bindingFormattingDialog == null)
                {
                    this.bindingFormattingDialog = new BindingFormattingDialog();
                }
                this.bindingFormattingDialog.Context = context;
                this.bindingFormattingDialog.Bindings = (ControlBindingsCollection) value;
                this.bindingFormattingDialog.Host = host;
                using (DesignerTransaction transaction = host.CreateTransaction())
                {
                    service.ShowDialog(this.bindingFormattingDialog);
                    if (this.bindingFormattingDialog.Dirty)
                    {
                        TypeDescriptor.Refresh(((ControlBindingsCollection) context.Instance).BindableComponent);
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                        return value;
                    }
                    transaction.Cancel();
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

