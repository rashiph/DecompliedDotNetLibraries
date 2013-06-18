namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Design;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Windows.Forms;

    internal class AdvancedBindingPropertyDescriptor : PropertyDescriptor
    {
        internal static AdvancedBindingEditor advancedBindingEditor = new AdvancedBindingEditor();
        internal static AdvancedBindingTypeConverter advancedBindingTypeConverter = new AdvancedBindingTypeConverter();

        internal AdvancedBindingPropertyDescriptor() : base(System.Design.SR.GetString("AdvancedBindingPropertyDescName"), null)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        protected override void FillAttributes(IList attributeList)
        {
            attributeList.Add(RefreshPropertiesAttribute.All);
            base.FillAttributes(attributeList);
        }

        public override object GetEditor(System.Type type)
        {
            if (type == typeof(UITypeEditor))
            {
                return advancedBindingEditor;
            }
            return base.GetEditor(type);
        }

        public override object GetValue(object component)
        {
            return component;
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                return new AttributeCollection(new Attribute[] { new System.Design.SRDescriptionAttribute("AdvancedBindingPropertyDescriptorDesc"), NotifyParentPropertyAttribute.Yes, new MergablePropertyAttribute(false) });
            }
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
                if (advancedBindingTypeConverter == null)
                {
                    advancedBindingTypeConverter = new AdvancedBindingTypeConverter();
                }
                return advancedBindingTypeConverter;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override System.Type PropertyType
        {
            get
            {
                return typeof(object);
            }
        }

        internal class AdvancedBindingTypeConverter : TypeConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    return string.Empty;
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}

