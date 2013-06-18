namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel.Compiler;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class TypeBrowserEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object value)
        {
            if (typeDescriptorContext == null)
            {
                throw new ArgumentNullException("typeDescriptorContext");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            object obj2 = value;
            this.editorService = (IWindowsFormsEditorService) serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (this.editorService != null)
            {
                ITypeFilterProvider filterProvider = null;
                TypeFilterProviderAttribute attribute = null;
                if ((typeDescriptorContext.PropertyDescriptor != null) && (typeDescriptorContext.PropertyDescriptor.Attributes != null))
                {
                    attribute = typeDescriptorContext.PropertyDescriptor.Attributes[typeof(TypeFilterProviderAttribute)] as TypeFilterProviderAttribute;
                }
                if (attribute != null)
                {
                    if (!(serviceProvider.GetService(typeof(ITypeProvider)) is ITypeProvider))
                    {
                        throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                    }
                    System.Type type = System.Type.GetType(attribute.TypeFilterProviderTypeName);
                    if (type != null)
                    {
                        filterProvider = Activator.CreateInstance(type, new object[] { serviceProvider }) as ITypeFilterProvider;
                    }
                }
                if (filterProvider == null)
                {
                    filterProvider = ((typeDescriptorContext.Instance is object[]) ? ((ITypeFilterProvider) ((object[]) typeDescriptorContext.Instance)[0]) : ((ITypeFilterProvider) typeDescriptorContext.Instance)) as ITypeFilterProvider;
                }
                if (filterProvider == null)
                {
                    filterProvider = value as ITypeFilterProvider;
                }
                if (filterProvider == null)
                {
                    IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (service != null)
                    {
                        IComponent component = service.GetComponent(typeDescriptorContext.Instance);
                        if (component is ITypeFilterProvider)
                        {
                            filterProvider = component as ITypeFilterProvider;
                        }
                    }
                }
                if (filterProvider == null)
                {
                    filterProvider = typeDescriptorContext.PropertyDescriptor as ITypeFilterProvider;
                }
                string selectedTypeName = value as string;
                if (((value != null) && (typeDescriptorContext.PropertyDescriptor.PropertyType != typeof(string))) && ((typeDescriptorContext.PropertyDescriptor.Converter != null) && typeDescriptorContext.PropertyDescriptor.Converter.CanConvertTo(typeof(string))))
                {
                    selectedTypeName = typeDescriptorContext.PropertyDescriptor.Converter.ConvertTo(typeDescriptorContext, CultureInfo.CurrentCulture, value, typeof(string)) as string;
                }
                using (TypeBrowserDialog dialog = new TypeBrowserDialog(serviceProvider, filterProvider, selectedTypeName))
                {
                    if (DialogResult.OK != this.editorService.ShowDialog(dialog))
                    {
                        return obj2;
                    }
                    if (typeDescriptorContext.PropertyDescriptor.PropertyType == typeof(System.Type))
                    {
                        return dialog.SelectedType;
                    }
                    if (typeDescriptorContext.PropertyDescriptor.PropertyType == typeof(string))
                    {
                        return dialog.SelectedType.FullName;
                    }
                    if ((typeDescriptorContext.PropertyDescriptor.Converter != null) && typeDescriptorContext.PropertyDescriptor.Converter.CanConvertFrom(typeDescriptorContext, typeof(string)))
                    {
                        obj2 = typeDescriptorContext.PropertyDescriptor.Converter.ConvertFrom(typeDescriptorContext, CultureInfo.CurrentCulture, dialog.SelectedType.FullName);
                    }
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

