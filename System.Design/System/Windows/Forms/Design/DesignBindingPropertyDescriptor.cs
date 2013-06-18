namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    internal class DesignBindingPropertyDescriptor : PropertyDescriptor
    {
        private static TypeConverter designBindingConverter = new DesignBindingConverter();
        private PropertyDescriptor property;
        private bool readOnly;

        internal DesignBindingPropertyDescriptor(PropertyDescriptor property, Attribute[] attrs, bool readOnly) : base(property.Name, attrs)
        {
            this.property = property;
            this.readOnly = readOnly;
            if ((base.AttributeArray != null) && (base.AttributeArray.Length > 0))
            {
                Attribute[] array = new Attribute[this.AttributeArray.Length + 2];
                this.AttributeArray.CopyTo(array, 0);
                array[this.AttributeArray.Length - 1] = NotifyParentPropertyAttribute.Yes;
                array[this.AttributeArray.Length] = RefreshPropertiesAttribute.Repaint;
                base.AttributeArray = array;
            }
            else
            {
                base.AttributeArray = new Attribute[] { NotifyParentPropertyAttribute.Yes, RefreshPropertiesAttribute.Repaint };
            }
        }

        public override bool CanResetValue(object component)
        {
            return !GetBinding((ControlBindingsCollection) component, this.property).IsNull;
        }

        private static DesignBinding GetBinding(ControlBindingsCollection bindings, PropertyDescriptor property)
        {
            Binding binding = bindings[property.Name];
            if (binding == null)
            {
                return DesignBinding.Null;
            }
            return new DesignBinding(binding.DataSource, binding.BindingMemberInfo.BindingMember);
        }

        public override object GetValue(object component)
        {
            return GetBinding((ControlBindingsCollection) component, this.property);
        }

        public override void ResetValue(object component)
        {
            SetBinding((ControlBindingsCollection) component, this.property, DesignBinding.Null);
        }

        private static void SetBinding(ControlBindingsCollection bindings, PropertyDescriptor property, DesignBinding designBinding)
        {
            if (designBinding != null)
            {
                Binding binding = bindings[property.Name];
                if (binding != null)
                {
                    bindings.Remove(binding);
                }
                if (!designBinding.IsNull)
                {
                    bindings.Add(property.Name, designBinding.DataSource, designBinding.DataMember);
                }
            }
        }

        public override void SetValue(object component, object value)
        {
            SetBinding((ControlBindingsCollection) component, this.property, (DesignBinding) value);
            this.OnValueChanged(component, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override System.Type ComponentType
        {
            get
            {
                return typeof(ControlBindingsCollection);
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return designBindingConverter;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.readOnly;
            }
        }

        public override System.Type PropertyType
        {
            get
            {
                return typeof(DesignBinding);
            }
        }
    }
}

