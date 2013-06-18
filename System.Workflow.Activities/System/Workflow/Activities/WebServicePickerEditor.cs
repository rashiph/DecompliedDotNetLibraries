namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel.Design;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal sealed class WebServicePickerEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            object obj2 = o;
            this.editorService = (IWindowsFormsEditorService) serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            IExtendedUIService service = (IExtendedUIService) serviceProvider.GetService(typeof(IExtendedUIService));
            if ((this.editorService != null) && (service != null))
            {
                Uri url = null;
                System.Type proxyClass = null;
                if (DialogResult.OK == service.AddWebReference(out url, out proxyClass))
                {
                    obj2 = (url != null) ? url.ToString() : string.Empty;
                    typeDescriptorContext.PropertyDescriptor.SetValue(typeDescriptorContext.Instance, obj2 as string);
                }
            }
            return obj2;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

