namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class BindUITypeEditor : UITypeEditor
    {
        private const int MaxItems = 10;
        private IServiceProvider serviceProvider;

        internal static object EditValue(ITypeDescriptorContext context)
        {
            object obj2 = null;
            if (((context != null) && (context.PropertyDescriptor != null)) && (context.Instance != null))
            {
                BindUITypeEditor editor = new BindUITypeEditor();
                obj2 = context.PropertyDescriptor.GetValue(context.Instance);
                obj2 = editor.EditValue(context, context, obj2);
                try
                {
                    context.PropertyDescriptor.SetValue(context.Instance, obj2);
                }
                catch (Exception exception)
                {
                    string message = SR.GetString("Error_CanNotBindProperty", new object[] { context.PropertyDescriptor.Name });
                    if (!string.IsNullOrEmpty(exception.Message))
                    {
                        message = message + "\n\n" + exception.Message;
                    }
                    DesignerHelpers.ShowError(context, message);
                }
            }
            return obj2;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider serviceProvider, object value)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            this.serviceProvider = serviceProvider;
            object obj2 = value;
            if ((context != null) && (context.PropertyDescriptor is DynamicPropertyDescriptor))
            {
                try
                {
                    using (ActivityBindForm form = new ActivityBindForm(this.serviceProvider, context))
                    {
                        if (DialogResult.OK != form.ShowDialog())
                        {
                            return obj2;
                        }
                        if (form.CreateNew)
                        {
                            if (form.CreateNewProperty)
                            {
                                List<CustomProperty> customProperties = CustomActivityDesignerHelper.GetCustomProperties(context);
                                if (customProperties != null)
                                {
                                    customProperties.Add(CustomProperty.CreateCustomProperty(this.serviceProvider, form.NewMemberName, context.PropertyDescriptor, context.Instance));
                                    CustomActivityDesignerHelper.SetCustomProperties(customProperties, context);
                                }
                            }
                            else
                            {
                                ActivityBindPropertyDescriptor.CreateField(context, form.Binding, true);
                            }
                        }
                        return form.Binding;
                    }
                }
                catch (Exception exception)
                {
                    string message = SR.GetString("Error_CanNotBindProperty", new object[] { context.PropertyDescriptor.Name });
                    if (!string.IsNullOrEmpty(exception.Message))
                    {
                        message = message + "\n\n" + exception.Message;
                    }
                    DesignerHelpers.ShowError(context, message);
                }
                return obj2;
            }
            DesignerHelpers.ShowError(this.serviceProvider, SR.GetString("Error_MultipleSelectNotSupportedForBindAndPromote"));
            return obj2;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}

