namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Forms;

    internal class ControlBindingsConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            if (!(value is ControlBindingsCollection))
            {
                return new PropertyDescriptorCollection(new PropertyDescriptor[0]);
            }
            ControlBindingsCollection component = (ControlBindingsCollection) value;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component.BindableComponent, (Attribute[]) null);
            ArrayList list = new ArrayList();
            for (int i = 0; i < properties.Count; i++)
            {
                Binding binding = component[properties[i].Name];
                bool readOnly = (((binding != null) && !(binding.DataSource is IListSource)) && !(binding.DataSource is IList)) && !(binding.DataSource is Array);
                DesignBindingPropertyDescriptor descriptor = new DesignBindingPropertyDescriptor(properties[i], null, readOnly);
                if (((BindableAttribute) properties[i].Attributes[typeof(BindableAttribute)]).Bindable || !((DesignBinding) descriptor.GetValue(component)).IsNull)
                {
                    list.Add(descriptor);
                }
            }
            list.Add(new AdvancedBindingPropertyDescriptor());
            PropertyDescriptor[] array = new PropertyDescriptor[list.Count];
            list.CopyTo(array, 0);
            return new PropertyDescriptorCollection(array);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

