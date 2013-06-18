namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    internal sealed class ConditionNameEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            if (typeDescriptorContext == null)
            {
                throw new ArgumentNullException("typeDescriptorContext");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            object selectedName = o;
            this.editorService = (IWindowsFormsEditorService) serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (this.editorService != null)
            {
                Activity component = null;
                IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                if (service != null)
                {
                    component = service.GetComponent(typeDescriptorContext.Instance) as Activity;
                }
                string name = typeDescriptorContext.PropertyDescriptor.GetValue(typeDescriptorContext.Instance) as string;
                ConditionBrowserDialog dialog = new ConditionBrowserDialog(component, name);
                if (DialogResult.OK == this.editorService.ShowDialog(dialog))
                {
                    selectedName = dialog.SelectedName;
                }
            }
            return selectedName;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

